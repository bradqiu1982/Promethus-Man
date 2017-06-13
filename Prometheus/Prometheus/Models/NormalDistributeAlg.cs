using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
    public class NormalDistributeAlg
    {
        public static double Mean(List<double> data)
        {
            var sum = 0.0;
            foreach (var val in data)
                sum = sum + val;
            return sum / data.Count;
        }
        public static double StandardDeviation(List<double> data,double mean)
        {
            double sumDeviation = 0.0;
            int dataSize = data.Count;

            for (int i = 0; i < dataSize; i++)
                sumDeviation = sumDeviation + (data[i] - mean) * (data[i] - mean);

            return Math.Sqrt(sumDeviation / (dataSize-1));
        }

        public static double getY(double x,double stddev,double mean)
        {
            var variance = stddev * stddev;
            double a = Math.Exp(-(((x - mean) * (x - mean)) / ((2 * variance))));
            double b = 1.0 / (stddev * Math.Sqrt(2 * Math.PI));
            return a*b;
        }

      }
}