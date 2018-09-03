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
            Mean = 0.0;
            Stdev = 0.0;

            RobustMean = 0.0;
            RobustStdev = 0.0;

            Ca = 0.0;
            Cp = 0.0;

            Cpk_ca = 0.0;
            Cpk_robust = 0.0;
            IsNormalProbability = 0.0;
            DPPM_robust = 0.0;
        }

        public double Mean { set; get; }
        public double Stdev { set; get; }

        private double RobustMean { set; get; }
        private double RobustStdev { set; get; }

        private double Ca { set; get; }
        private double Cp { set; get; }

        //private double Cpk_3s { set; get; }

        public double IsNormalProbability { set; get; }
        public double Cpk_ca { set; get; }
        public double Cpk_robust { set; get; }
        public double DPPM_robust { set; get; }

        public static void Test()
        {
            var rawdata = new List<double>();

            var sql = "SELECT [TxPower]  FROM [NPITrace].[dbo].[ModuleTXOData] WHERE TestName= 'ersetup' and TestTimeStamp > '2018-08-10 00:00:00' and TestTimeStamp < '2018-08-11 00:00:00' and PN='1220278'";
            //var sql = "SELECT [TxPower]  FROM [NPITrace].[dbo].[ModuleTXOData] WHERE TestTimeStamp > '2018-08-10 00:00:00' and TestTimeStamp < '2018-08-10 13:00:00' and TXPower > -0.5 and TXPower < 0.5";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            { rawdata.Add(Math.Round(Convert.ToDouble(line[0]), 3)); }

            var ax = "";
            var hist = new MathNet.Numerics.Statistics.Histogram(rawdata, 50);
            for (var idx = 0; idx < 50; idx++)
            {
                var item = hist[idx];
                ax = ax + idx + "," + item.Count + "   ";
            }

            var cpk = CPKData.GetCpk(rawdata, "1.3", "-3.0");

            var p = cpk[0].IsNormalProbability;
        }

        private static double IsNormal(List<double> rawdata)
        {
            rawdata.Sort();
            var mean = Statistics.Mean(rawdata);
            var stdev = Statistics.StandardDeviation(rawdata);
            var normal = new NormalDistribution(mean, stdev);
            var sample = new Sample(rawdata);
            var isnormal = sample.KolmogorovSmirnovTest(normal);
            return isnormal.Probability;
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
            tempvm.Stdev = StandardDeviation(rawdata);
            tempvm.IsNormalProbability = Math.Round(IsNormal(rawdata),5);

            if (!string.IsNullOrEmpty(highlimit) && !string.IsNullOrEmpty(lowlimit))
            {
                var hlimit = Convert.ToDouble(highlimit);
                var llimit = Convert.ToDouble(lowlimit);

                tempvm.Ca = (tempvm.Mean - (hlimit + llimit)/2.0) / ((hlimit - llimit) / 2.0);
                if (tempvm.Ca < 0)
                { tempvm.Ca = 0.0 - tempvm.Ca; }

                tempvm.Cp = (hlimit - llimit) / (6.0 * tempvm.Stdev);

                //var cpk3s1 = (hlimit - tempvm.Mean) / (3.0 * tempvm.Stdev);
                //var cpk3s2 = (tempvm.Mean - llimit) / (3.0 * tempvm.Stdev);
                //tempvm.Cpk_3s = cpk3s1;
                //if (cpk3s2 < tempvm.Cpk_3s)
                //{ tempvm.Cpk_3s = cpk3s2; }

                tempvm.Cpk_ca = tempvm.Cp * (1.0 - tempvm.Ca);

            }
            else if (!string.IsNullOrEmpty(highlimit))
            {
                var hlimit = Convert.ToDouble(highlimit);
                tempvm.Cpk_ca = (hlimit - tempvm.Mean) / (3.0 * tempvm.Stdev);

            }
            else if (!string.IsNullOrEmpty(lowlimit))
            {
                var llimit = Convert.ToDouble(lowlimit);
                tempvm.Cpk_ca = (tempvm.Mean - llimit) / (3.0 * tempvm.Stdev);
            }

            Normal n = new Normal();
            tempvm.Cpk_robust = GetRobustCpk(rawdata, highlimit, lowlimit,n);
            tempvm.DPPM_robust = Math.Round(GetDPPM(tempvm.Cpk_robust, n),0);

            ret.Add(tempvm);
            return ret;
        }

        public static double MeanVal(List<double> data)
        {
            return Statistics.Mean(data);
        }
        public static double StandardDeviation(List<double> data)
        {
            return Statistics.StandardDeviation(data);
        }

        private static double GetRobustCpk(List<double> rawdata, string highlimit, string lowlimit, Normal n)
        {
            var quantilesDouble = GetDataQuantiles(rawdata.Count, n);

            var rCpks = new List<double>();
            if (!string.IsNullOrEmpty(highlimit))
                rCpks.Add(GetRobustCPKWithSingleSpec(rawdata, quantilesDouble, Convert.ToDouble(highlimit), "USL"));
            if (!string.IsNullOrEmpty(lowlimit))
                rCpks.Add(GetRobustCPKWithSingleSpec(rawdata, quantilesDouble, Convert.ToDouble(lowlimit), "LSL"));
            return rCpks.Min();
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

        private static double GetDPPM(double cpk, Normal n)
        {
            return (1 - n.CumulativeDistribution(3 * cpk)) * 1000000;
        }

    }
}