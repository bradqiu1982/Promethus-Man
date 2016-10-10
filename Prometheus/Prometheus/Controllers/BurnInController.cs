using Prometheus.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prometheus.Controllers
{
    public class BurnInController : Controller
    {

        public static void ProjectWeeklyTrend(Controller ctrl, string ProjectKey)
        {
            var vmlist = ProjectBIYieldViewModule.GetYieldByWeeks(ProjectKey);
            if (vmlist.Count > 0)
            {
                var ChartxAxisValues = "";
                var ChartSearies = "";

                var ftimelist = new List<string>();
                var cvalues = new List<double>();

                foreach (var item in vmlist)
                {
                    ftimelist.Add(item.EndDate.ToString("yyyy-MM-dd"));
                    cvalues.Add(item.LastYield * 100.0);
                }

                //xaxis
                foreach (var item in ftimelist)
                {
                    ChartxAxisValues = ChartxAxisValues + "'" + item + "',";
                }
                ChartxAxisValues = ChartxAxisValues.Substring(0, ChartxAxisValues.Length - 1);


                //yaxis
                ChartSearies = "{name:'Retest Yield',data:[<cvalue>]}";

                var tempvalue = "";
                foreach (var item in cvalues)
                {
                    tempvalue = tempvalue + item.ToString("0.00") + ",";
                }
                tempvalue = tempvalue.Substring(0, tempvalue.Length - 1);
                ChartSearies = ChartSearies.Replace("<cvalue>", tempvalue);

                //rederect url
                //var reurl = "window.location.href = '/Project/ProjectWYieldDetail?ProjectKey=" + ProjectKey + "'" + "+'&EndDate='+this.category";

                var tempscript = System.IO.File.ReadAllText(ctrl.Server.MapPath("~/Scripts/ColumnChart.xml"));
                ctrl.ViewBag.chartscript = tempscript.Replace("#ElementID#", "weeklyyield")
                    .Replace("#ChartType#", "column")
                    .Replace("#Title#", "Weekly Yiled Trend")
                    .Replace("#ChartxAxisValues#", ChartxAxisValues)
                    .Replace("#yAxisTitle#", "Yield Percent")
                    .Replace("#ChartSearies#", ChartSearies)
                    .Replace("#REDIRECTURL#", "");
            }
        }

        // GET: BurnIn
        public ActionResult BurnInMainPage(string ProjectKey)
        {
            if (!string.IsNullOrEmpty(ProjectKey))
            {
                ProjectWeeklyTrend(this, ProjectKey);
            }
            return View();
        }
    }
}