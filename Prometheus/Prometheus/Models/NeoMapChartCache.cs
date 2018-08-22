using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
    public class NEOMAPCHARTTYPE
    {
        public static string HOTMAP = "HOTMAP";
        public static string NORMALDIS = "NORMALDIS";
        public static string BOXPLOT = "BOXPLOT";
    }

    public class NeoMapChartCache
    {
        public NeoMapChartCache()
        {
            WaferNo = "";
            FieldName = "";
            ChartType = "";
            Chart = "";
        }

        public NeoMapChartCache(string wf,string fd,string ct,string chart)
        {
            WaferNo = wf;
            FieldName = fd;
            ChartType = ct;
            Chart = chart;
        }

        private static void DeleteChart(string wf, string fd, string ct)
        {
            var sql = "delete from NeoMapChartCache where WaferNo = @WaferNo and FieldName = @FieldName and ChartType = @ChartType";
            var dict = new Dictionary<string, string>();
            dict.Add("@WaferNo",wf);
            dict.Add("@FieldName",fd);
            dict.Add("@ChartType",ct);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public static void UpdateChart(string wf, string fd, string ct, string chart)
        {
            DeleteChart(wf, fd, ct);
            var sql = "insert into NeoMapChartCache(WaferNo,FieldName,ChartType,Chart,UpdateTime) values(@WaferNo,@FieldName,@ChartType,@Chart,@UpdateTime)";
            var dict = new Dictionary<string, string>();
            dict.Add("@WaferNo", wf);
            dict.Add("@FieldName", fd);
            dict.Add("@ChartType", ct);
            dict.Add("@Chart", chart);
            dict.Add("@UpdateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public static List<NeoMapChartCache> RetrieveCharts(string wf, string fd)
        {
            var ret = new List<NeoMapChartCache>();

            var sql = "select WaferNo,FieldName,ChartType,Chart from NeoMapChartCache where WaferNo = @WaferNo and FieldName = @FieldName";
            var dict = new Dictionary<string, string>();
            dict.Add("@WaferNo", wf);
            dict.Add("@FieldName", fd);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
            foreach (var line in dbret)
            {
                ret.Add(new NeoMapChartCache(Convert.ToString(line[0]), Convert.ToString(line[1]), Convert.ToString(line[2]), Convert.ToString(line[3])));
            }

            return ret;
        }

        public string WaferNo { set; get; }
        public string FieldName { set; get; }
        public string ChartType { set; get; }
        public string Chart { set; get; }

    }
}