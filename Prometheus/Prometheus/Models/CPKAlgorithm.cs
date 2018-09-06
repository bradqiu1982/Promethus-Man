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
            DPPM = 0.0;
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

        public double Cpk_final {
            get {
                if (IsNormalProbability > 0.05)
                {
                    return Cpk_ca;
                }
                else
                {
                    return Cpk_robust;
                }
            }
        }
        public string IsNormalStr
        {
            get
            {
                if (IsNormalProbability > 0.05)
                {
                    return "YES";
                }
                else
                {
                    return "NO";
                }
            }
        }

        public double DPPM { set; get; }

        private static double IsNormal(List<double> rawdata)
        {
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

                tempvm.Cpk_ca = Math.Round(tempvm.Cp * (1.0 - tempvm.Ca),5);

            }
            else if (!string.IsNullOrEmpty(highlimit))
            {
                var hlimit = Convert.ToDouble(highlimit);
                tempvm.Cpk_ca = Math.Round((hlimit - tempvm.Mean) / (3.0 * tempvm.Stdev),5);

            }
            else if (!string.IsNullOrEmpty(lowlimit))
            {
                var llimit = Convert.ToDouble(lowlimit);
                tempvm.Cpk_ca = Math.Round((tempvm.Mean - llimit) / (3.0 * tempvm.Stdev),5);
            }

            Normal n = new Normal();
            tempvm.Cpk_robust = Math.Round(GetRobustCpk(rawdata, highlimit, lowlimit,n),5);

            if (tempvm.IsNormalProbability > 0.05)
            {
                tempvm.DPPM = Math.Round(GetDPPM(tempvm.Cpk_ca, n), 0);
            }
            else
            {
                tempvm.DPPM = Math.Round(GetDPPM(tempvm.Cpk_robust, n),0);
            }


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

    public class HistogramChart
    {
        public static object GetChartData(string id, string title, string lowlimit, string highlimit, List<double> rawdata)
        {
            rawdata.Sort();
            var min = rawdata.Min();
            var max = rawdata.Max();

            double step = (max - min) / 50.0;
            var steplist = new List<double>();
            var startidx = Math.Round((min + 0.5 * step),5);
            steplist.Add(startidx);
            for (var idx = 1; idx < 50; idx++)
            {
                steplist.Add(Math.Round((startidx+step*idx), 5));
            }

            var hist = new MathNet.Numerics.Statistics.Histogram(rawdata, 50);
            var frequencelist = new List<object>();
            for (var idx = 0; idx < 50; idx++)
            {
                var templist = new List<double>();
                templist.Add(steplist[idx]);
                templist.Add(hist[idx].Count);
                frequencelist.Add(templist);
            }

            var mean = Statistics.Mean(rawdata);
            var stddev = Statistics.StandardDeviation(rawdata);
            var left3stddev = Math.Min(mean - 3.0 * stddev, mean + 3.0 * stddev);
            var right3stddev = Math.Max(mean - 3.0 * stddev, mean + 3.0 * stddev);

            var lowbound = lowlimit;
            if (string.IsNullOrEmpty(lowlimit))
            {
                lowbound = min.ToString();
            }
            var upperbound = highlimit;
            if (string.IsNullOrEmpty(highlimit))
            {
                upperbound = max.ToString();
            }
            var alldata = new List<object>();
            alldata.Add(new
            {
                name = "Frequence",
                data = frequencelist
            });

            var xmin = left3stddev;
            var xmax = right3stddev;
            if (xmin > min)
            { xmin = min; }
            if (xmin > Convert.ToDouble(lowbound))
            { xmin = Convert.ToDouble(lowbound); }

            if (xmax < max)
            { xmax = max; }
            if (xmax < Convert.ToDouble(upperbound))
            { xmax = Convert.ToDouble(upperbound); }

            var ret = new {
                id = id,
                title = title,
                xmin = xmin,
                xmax = xmax,
                mean = mean,
                left3stddev = left3stddev,
                right3stddev = right3stddev,
                lowbound = lowbound,
                upperbound = upperbound,
                data = alldata
            };
            return ret;
        }

    }
}