using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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


        public static List<CPKData> GetCpk(List<double> rawdata, string highlimit, string lowlimit)
        {
            if (string.IsNullOrEmpty(highlimit) && string.IsNullOrEmpty(lowlimit))
            {
                return new List<CPKData>();
            }

            var ret = new List<CPKData>();

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
    }
}