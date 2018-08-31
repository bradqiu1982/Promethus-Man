using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MathNet.Numerics;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;
using Meta.Numerics.Statistics;
using Meta.Numerics.Statistics.Distributions;

namespace Prometheus.Models
{
    public class CPKRESULT
    {
        public static string EMPTY = "EMPTY";
        public static string CPU = "CPU";
        public static string CPL = "CPL";
        public static string CPK = "CPK";
    }


    public class CPKData
    {
        public CPKData()
        {
            CPKResult = CPKRESULT.EMPTY;
            Mean = 0.0;
            Stdev = 0.0;

            RobustMean = 0.0;
            RobustStdev = 0.0;

            Ca = 0.0;

            Cpu = 0.0;
            Cpl = 0.0;
            Cp = 0.0;

            Cpk_3s = 0.0;
            Cpk_ca = 0.0;
            Cpk_robust = 0.0;
        }

        public string CPKResult { set; get; }

        public double Mean { set; get; }
        public double Stdev { set; get; }

        public double RobustMean { set; get; }
        public double RobustStdev { set; get; }

        public double Ca { set; get; }

        public double Cpu { set; get; }
        public double Cpl { set; get; }
        public double Cp { set; get; }

        public double Cpk_3s { set; get; }
        public double Cpk_ca { set; get; }
        public double Cpk_robust { set; get; }


        public static void Test()
        {
            var rawdata = new List<double>();

            //var sql = "SELECT [TxPower]  FROM [NPITrace].[dbo].[ModuleTXOData] WHERE TestName= 'ersetup' and TestTimeStamp > '2018-08-10 00:00:00' and TestTimeStamp < '2018-08-11 00:00:00' and PN='1220278'";
            var sql = "SELECT [TxPower]  FROM [NPITrace].[dbo].[ModuleTXOData] WHERE TestTimeStamp > '2018-08-10 00:00:00' and TestTimeStamp < '2018-08-10 13:00:00' and TXPower > -0.5 and TXPower < 0.5";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            { rawdata.Add(Math.Round(Convert.ToDouble(line[0]), 3)); }

            rawdata.Sort();

            var mean = Statistics.Mean(rawdata);
            var stdev = Statistics.StandardDeviation(rawdata);

            var normal = new NormalDistribution(mean,stdev);
            var sample = new Sample(rawdata);
            var isnormal = sample.KolmogorovSmirnovTest(normal);

            //var ax = "";
            //var hist = new MathNet.Numerics.Statistics.Histogram(rawdata, 10);
            //for (var idx = 0; idx < 10; idx++)
            //{
            //    var item = hist[idx];
            //    ax = ax + idx + "," + item.Count+"   ";
            //}

            var p = isnormal.Probability;
            var pv = Math.Round(p, 5);
        }

        public static List<CPKData> GetCpk(List<double> rawdata, string highlimit, string lowlimit)
        {
            if (string.IsNullOrEmpty(highlimit) && string.IsNullOrEmpty(lowlimit))
            {
                return new List<CPKData>();
            }

            var ret = new List<CPKData>();

            rawdata.Sort();

            var tempvm = new CPKData();
            tempvm.Mean = MeanVal(rawdata);
            tempvm.Stdev = StandardDeviation(rawdata, tempvm.Mean);

            if (!string.IsNullOrEmpty(highlimit) && !string.IsNullOrEmpty(lowlimit))
            {
                tempvm.CPKResult = CPKRESULT.CPK;
                var hlimit = Convert.ToDouble(highlimit);
                var llimit = Convert.ToDouble(lowlimit);

                tempvm.Ca = (tempvm.Mean - (hlimit + llimit)/2.0) / ((hlimit - llimit) / 2.0);
                if (tempvm.Ca < 0)
                { tempvm.Ca = 0.0 - tempvm.Ca; }

                tempvm.Cp = (hlimit - llimit) / (6.0 * tempvm.Stdev);

                var cpk3s1 = (hlimit - tempvm.Mean) / (3.0 * tempvm.Stdev);
                var cpk3s2 = (tempvm.Mean - llimit) / (3.0 * tempvm.Stdev);
                tempvm.Cpk_3s = cpk3s1;
                if (cpk3s2 < tempvm.Cpk_3s)
                { tempvm.Cpk_3s = cpk3s2; }

                tempvm.Cpk_ca = tempvm.Cp * (1.0 - tempvm.Ca);

            }
            else if (!string.IsNullOrEmpty(highlimit))
            {
                tempvm.CPKResult = CPKRESULT.CPU;
                var hlimit = Convert.ToDouble(highlimit);
                tempvm.Cpu = (hlimit - tempvm.Mean) / (3.0 * tempvm.Stdev);

            }
            else if (!string.IsNullOrEmpty(lowlimit))
            {
                tempvm.CPKResult = CPKRESULT.CPL;
                var llimit = Convert.ToDouble(lowlimit);
                tempvm.Cpl = (tempvm.Mean - llimit) / (3.0 * tempvm.Stdev);
            }

            var robustckp = GetRobustCpk(rawdata, highlimit, lowlimit);
            if (robustckp.Count > 0)
            {
                tempvm.Cpk_robust = robustckp.Min();
            }

            ret.Add(tempvm);
            return ret;
        }

        public static double MeanVal(List<double> data)
        {
            var sum = 0.0;
            foreach (var val in data)
                sum = sum + val;
            return sum / data.Count;
        }
        public static double StandardDeviation(List<double> data, double mean)
        {
            double sumDeviation = 0.0;
            int dataSize = data.Count;

            for (int i = 0; i < dataSize; i++)
                sumDeviation = sumDeviation + (data[i] - mean) * (data[i] - mean);

            return Math.Sqrt(sumDeviation / (dataSize - 1));
        }

        private static List<double> GetRobustCpk(List<double> rawdata, string highlimit, string lowlimit)
        {
            Normal n = new Normal();

            var quantilesDouble = GetDataQuantiles(rawdata.Count, n);

            var rCpks = new List<double>();
            if (!string.IsNullOrEmpty(highlimit))
                rCpks.Add(GetRobustCPKWithSingleSpec(rawdata, quantilesDouble, Convert.ToDouble(highlimit), "USL"));
            if (!string.IsNullOrEmpty(lowlimit))
                rCpks.Add(GetRobustCPKWithSingleSpec(rawdata, quantilesDouble, Convert.ToDouble(lowlimit), "LSL"));
            //if (rCpks.Count != 0)
            //{
            //    // Math.Round(rCpks.Min(), 2).ToString();
            //}
            return rCpks;
        }

        private static List<double> GetDataQuantiles(int qty, Normal n)
        {
            var quantilesDouble = new List<double>();
            for (int i = 0; i < qty; i++)
            {
                //double p = (double)(i + 1)*0.999999 / qty;
                double p = (double)(i + 0.5) / qty;
                quantilesDouble.Add(n.InverseCumulativeDistribution(p));
            }
            return quantilesDouble;
        }

        private static double GetRobustCPKWithSingleSpec(List<double> data, List<double> quantiles, double LSL, string type)
        {
            var slopes = new List<double>();
            for (int i = 4; i < data.Count; i++)
            {
                var temp = (data[i] - data[i - 4]) / (quantiles[i] - quantiles[i - 4]);
                slopes.Add(temp);
            }
            var loc = 0;
            if (type.Equals("LSL"))
            {
                for (int i = 0; i < slopes.Count; i++)
                {
                    if (slopes[i] != 0)
                    { loc = i; break; }
                }
            }
            else if (type.Equals("USL"))
            {
                for (int i = slopes.Count - 1; i > 0; i--)
                {
                    if (slopes[i] != 0)
                    { loc = i; break; }
                }
            }
            var sig = quantiles[loc] + (LSL - data[loc]) / slopes[loc];
            sig = Math.Abs(sig) / 3;
            return sig;
        }

    }
}