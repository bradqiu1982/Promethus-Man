using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
    public class ParetoData
    {
        public string key { set; get; }
        public int count { set; get; }
        public double percent { set; get; }
        public double sumpercent { set; get; }
    }
}