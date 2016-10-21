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

                var ftimelist = new List<string>();
                var famountlist = new List<int>();
                var ryieldlist = new List<double>();
                var cyieldlist = new List<double>();
                var maxamout = 0;

                foreach (var item in vmlist)
                {
                    ftimelist.Add(item.EndDate.ToString("yyyy-MM-dd"));
                    ryieldlist.Add(item.LastYield * 100.0);
                    cyieldlist.Add(item.CorrectLastYield * 100.0);

                    var tempfamount = 0;
                    foreach (var d in item.LastYields)
                    {
                        if (d.InputCount > tempfamount) { tempfamount = d.InputCount; }
                        if (d.InputCount > maxamout) { maxamout = d.InputCount; }
                    }
                    famountlist.Add(tempfamount);
                }

                //xaxis
                foreach (var item in ftimelist)
                {
                    ChartxAxisValues = ChartxAxisValues + "'" + item + "',";
                }
                ChartxAxisValues = ChartxAxisValues.Substring(0, ChartxAxisValues.Length - 1);


                var famout = "";
                foreach (var item in famountlist)
                {
                    famout = famout + item.ToString() + ",";
                }
                famout = famout.Substring(0, famout.Length - 1);

                var rtempvalue = "";
                foreach (var item in ryieldlist)
                {
                    rtempvalue = rtempvalue + item.ToString("0.00") + ",";
                }
                rtempvalue = rtempvalue.Substring(0, rtempvalue.Length - 1);

                var crtempvalue = "";
                foreach (var item in cyieldlist)
                {
                    crtempvalue = crtempvalue + item.ToString("0.00") + ",";
                }
                crtempvalue = crtempvalue.Substring(0, crtempvalue.Length - 1);

                //rederect url
                var reurl = "window.location.href = '/BurnIn/ProjectBIWYieldDetail?ProjectKey=" + ProjectKey + "'" + "+'&EndDate='+this.category";


                var tempscript = System.IO.File.ReadAllText(ctrl.Server.MapPath("~/Scripts/BISuperYield.xml"));
                ctrl.ViewBag.chartscript = tempscript.Replace("#ElementID#", "weeklyyield")
                    .Replace("#Title#", "Weekly Yiled Trend")
                    .Replace("#ChartxAxisValues#", ChartxAxisValues)
                    .Replace("#XAxisTitle#", "Date")
                    .Replace("#AmountMAX#", maxamout.ToString())
                    .Replace("#FirstAmount#", famout)
                    .Replace("#RetestYield#", rtempvalue)
                    .Replace("#CorrectiveYield#", crtempvalue)
                    .Replace("#REDIRECTURL#", reurl);
            }
        }

        private List<SelectListItem> CreateWaferList(string ProjectKey)
        {
            var plist = new List<SelectListItem>();
            var waferlist = BITestData.RetrieveAllWafer(ProjectKey);

            var p = new SelectListItem();
            p.Text = "NONE";
            p.Value = "NONE";
            plist.Add(p);
            foreach (var item in waferlist)
            {
                p = new SelectListItem();
                p.Text = item;
                p.Value = item;
                plist.Add(p);
            }
            plist[0].Selected = true;
            return plist;
        }

        // GET: BurnIn
        public ActionResult BurnInMainPage(string ProjectKey)
        {
            if (!string.IsNullOrEmpty(ProjectKey))
            {
                ViewBag.pjkey = ProjectKey;
                ProjectWeeklyTrend(this, ProjectKey);
                ViewBag.waferlist = CreateWaferList(ProjectKey);
            }
            return View();
        }

        private void retestparetofun(List<KeyValuePair<string, int>> retestdatalist)
        {
            if (retestdatalist.Count > 0)
            {
                var peralist = new List<ParetoData>();

                if (retestdatalist.Count > 1)
                {
                    retestdatalist.Sort(delegate (KeyValuePair<string, int> pair1, KeyValuePair<string, int> pair2)
                    {
                        return pair2.Value.CompareTo(pair1.Value);
                    });
                }

                var sum = 0;
                for (var i = 0; i < retestdatalist.Count; i++)
                {
                    sum = sum + retestdatalist[i].Value;
                }

                var otherpercent = 0.0;

                for (var i = 0; i < retestdatalist.Count; i++)
                {
                    if (retestdatalist.Count > 5 && peralist.Count > 0 && peralist[peralist.Count - 1].sumpercent > 0.95)
                    {
                        otherpercent = otherpercent + retestdatalist[i].Value / (double)sum;
                        if (i == (retestdatalist.Count - 1))
                        {
                            var tempperato = new ParetoData();
                            tempperato.key = "Other";
                            tempperato.count = (int)(otherpercent * sum);
                            tempperato.percent = otherpercent;
                            tempperato.sumpercent = 1.0;
                            peralist.Add(tempperato);
                        }
                    }
                    else
                    {
                        var tempperato = new ParetoData();
                        tempperato.key = retestdatalist[i].Key;
                        if (i == 0)
                        {
                            tempperato.count = retestdatalist[i].Value;
                            tempperato.percent = tempperato.count / (double)sum;
                            tempperato.sumpercent = tempperato.percent;
                            peralist.Add(tempperato);
                        }
                        else
                        {
                            tempperato.count = retestdatalist[i].Value;
                            tempperato.percent = tempperato.count / (double)sum;
                            tempperato.sumpercent = peralist[peralist.Count - 1].sumpercent + tempperato.percent;
                            peralist.Add(tempperato);
                        }
                    }
                }

                //xaxis
                var ChartxAxisValues = "";

                foreach (var item in peralist)
                {
                    ChartxAxisValues = ChartxAxisValues + "'" + item.key + "',";
                }
                ChartxAxisValues = ChartxAxisValues.Substring(0, ChartxAxisValues.Length - 1);


                //yaxis
                //var ChartSearies = "{name:'Defect',data:[<fvalue>]}";

                var pcountvalue = "";
                foreach (var item in peralist)
                {
                    pcountvalue = pcountvalue + item.count.ToString() + ",";
                }
                pcountvalue = pcountvalue.Substring(0, pcountvalue.Length - 1);

                var ppecentvalue = "";
                foreach (var item in peralist)
                {
                    ppecentvalue = ppecentvalue + (item.sumpercent * 100).ToString("0.0") + ",";
                }
                ppecentvalue = ppecentvalue.Substring(0, ppecentvalue.Length - 1);

                var abpecentvalue = "";
                foreach (var item in peralist)
                {
                    abpecentvalue = abpecentvalue + (item.percent * 100).ToString("0.0") + ",";
                }
                abpecentvalue = abpecentvalue.Substring(0, abpecentvalue.Length - 1);

                //ChartSearies = ChartSearies.Replace("<fvalue>", tempvalue);

                var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/ParetoChart.xml"));
                ViewBag.rparetoscript = tempscript.Replace("#ElementID#", "rparetochart")
                    .Replace("#Title#", "Pareto of Final Defect")
                    .Replace("#XAxisTitle#", "Defect")
                    .Replace("#ChartxAxisValues#", ChartxAxisValues)
                    .Replace("#AmountMAX#", sum.ToString())
                    .Replace("#PCount#", pcountvalue)
                    .Replace("#ABPercent#", abpecentvalue)
                    .Replace("#PPercent#", ppecentvalue);
            }
        }

        public ActionResult ProjectBIWYieldDetail(string ProjectKey, string EndDate)
        {
            if (!string.IsNullOrEmpty(ProjectKey) && !string.IsNullOrEmpty(EndDate))
            {
                var edate = DateTime.Parse(DateTime.Parse(EndDate).ToString("yyyy-MM-dd") + " 07:30:00");
                var sdate = edate.AddDays(-7);
                if (sdate.DayOfWeek != DayOfWeek.Thursday)
                {
                    for (int i = 6; i > 0; i--)
                    {
                        sdate = edate.AddDays(0 - i);
                        if (sdate.DayOfWeek == DayOfWeek.Thursday)
                        {
                            break;
                        }
                    }
                }


                var retestdatalist = new List<KeyValuePair<string, int>>();

                var pvm = ProjectViewModels.RetrieveOneProject(ProjectKey);
                var yieldvm = ProjectBIYieldViewModule.GetYieldByDateRange(ProjectKey, sdate.ToString(), edate.ToString(), pvm);

                if (yieldvm.LastYields.Count > 0)
                {
                    var piedatadict = new Dictionary<string, int>();
                    var eklist = new List<string>();
                    foreach (var error in yieldvm.LErrorMap.Keys)
                    {
                        eklist.Add(error);
                    }

                    foreach (var error in eklist)
                    {
                        if (string.Compare(error, "PASS", true) != 0)
                        {
                            foreach (var test in yieldvm.LastYields)
                            {
                                var val = ProjectBIYieldViewModule.RetrieveErrorCount(error, test.WhichTest, yieldvm.LErrorMap);

                                if (piedatadict.ContainsKey(error))
                                {
                                    var preval = piedatadict[error];
                                    piedatadict[error] = preval + val;
                                }
                                else
                                {
                                    piedatadict.Add(error, val);
                                }
                            }
                        }
                    }

                    retestdatalist = piedatadict.ToList();

                    piedatadict["PASS"] = ProjectBIYieldViewModule.RetrieveErrorCount("PASS", yieldvm.LastYields[yieldvm.LastYields.Count - 1].WhichTest, yieldvm.LErrorMap);

                    var keys = piedatadict.Keys;
                    var namevaluepair = "";
                    foreach (var k in keys)
                    {
                        if (piedatadict[k] > 0)
                            namevaluepair = namevaluepair + "{ name:'" + k + "',y:" + piedatadict[k].ToString() + "},";
                    }

                    namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                    var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart.xml"));
                    ViewBag.rchartscript = tempscript.Replace("#ElementID#", "rfailurepie")
                        .Replace("#Title#", "Final Failure")
                        .Replace("#SERIESNAME#", "RFailure")
                        .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                }

                if (retestdatalist.Count > 0)
                {
                    retestparetofun(retestdatalist);
                }

                return View(yieldvm);
            }
            return View();
        }

        public ActionResult ProjectBIDailyYield(string ProjectKey, string StartDate, string EndDate)
        {
            if (ProjectKey != null && StartDate != null && EndDate != null)
            {
                ViewBag.pjkey = ProjectKey;
                var vmlist = ProjectBIYieldViewModule.GetYieldByDay(ProjectKey, StartDate, EndDate);
                if (vmlist.Count > 0)
                {
                    var ChartxAxisValues = "";
                    //var ChartSearies = "";

                    var ftimelist = new List<string>();
                    var famountlist = new List<int>();
                    var ryieldlist = new List<double>();
                    var cyieldlist = new List<double>();
                    var maxamout = 0;

                    foreach (var item in vmlist)
                    {
                        ftimelist.Add(item.StartDate.ToString("yyyy-MM-dd"));
                        ryieldlist.Add(item.LastYield * 100.0);
                        cyieldlist.Add(item.CorrectLastYield * 100.0);

                        var tempfamount = 0;
                        foreach (var d in item.LastYields)
                        {
                            if (d.InputCount > tempfamount) { tempfamount = d.InputCount; }
                            if (d.InputCount > maxamout) { maxamout = d.InputCount; }
                        }
                        famountlist.Add(tempfamount);
                    }

                    //xaxis
                    foreach (var item in ftimelist)
                    {
                        ChartxAxisValues = ChartxAxisValues + "'" + item + "',";
                    }
                    ChartxAxisValues = ChartxAxisValues.Substring(0, ChartxAxisValues.Length - 1);


                    //yaxis
                    var famout = "";
                    foreach (var item in famountlist)
                    {
                        famout = famout + item.ToString() + ",";
                    }
                    famout = famout.Substring(0, famout.Length - 1);

                    var rtempvalue = "";
                    foreach (var item in ryieldlist)
                    {
                        rtempvalue = rtempvalue + item.ToString("0.00") + ",";
                    }
                    rtempvalue = rtempvalue.Substring(0, rtempvalue.Length - 1);

                    var crtempvalue = "";
                    foreach (var item in cyieldlist)
                    {
                        crtempvalue = crtempvalue + item.ToString("0.00") + ",";
                    }
                    crtempvalue = crtempvalue.Substring(0, crtempvalue.Length - 1);

                    //rederect url
                    var reurl = "window.location.href = '/BurnIn/ProjectBIDailyYieldDetail?ProjectKey=" + ProjectKey + "'" + "+'&EndDate='+this.category+'&VStartDate='+'" + StartDate + "'+'&VEndDate='+'" + EndDate + "'";

                    var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/BISuperYield.xml"));
                    ViewBag.chartscript = tempscript.Replace("#ElementID#", "dailyyield")
                        .Replace("#Title#", "Daily Yield")
                        .Replace("#ChartxAxisValues#", ChartxAxisValues)
                        .Replace("#XAxisTitle#", "Date")
                        .Replace("#AmountMAX#", maxamout.ToString())
                        .Replace("#FirstAmount#", famout)
                        .Replace("#RetestYield#", rtempvalue)
                        .Replace("#CorrectiveYield#", crtempvalue)
                        .Replace("#REDIRECTURL#", reurl);
                }

                return View();
            }
            return View();
        }

        public ActionResult ProjectBIDailyYieldDetail(string ProjectKey, string EndDate, string VStartDate, string VEndDate)
        {
            if (!string.IsNullOrEmpty(ProjectKey) && !string.IsNullOrEmpty(EndDate))
            {

                ViewBag.StartDate = VStartDate;
                ViewBag.EndDate = VEndDate;

                var edate = DateTime.Parse(DateTime.Parse(EndDate).AddDays(1).ToString("yyyy-MM-dd") + " 07:30:00");
                var sdate = edate.AddDays(-1);
                
                var retestdatalist = new List<KeyValuePair<string, int>>();

                var pvm = ProjectViewModels.RetrieveOneProject(ProjectKey);
                var yieldvm = ProjectBIYieldViewModule.GetYieldByDateRange(ProjectKey, sdate.ToString(), edate.ToString(), pvm);

                if (yieldvm.LastYields.Count > 0)
                {
                    var piedatadict = new Dictionary<string, int>();
                    var eklist = new List<string>();
                    foreach (var error in yieldvm.LErrorMap.Keys)
                    {
                        eklist.Add(error);
                    }

                    foreach (var error in eklist)
                    {
                        if (string.Compare(error, "PASS", true) != 0)
                        {
                            foreach (var test in yieldvm.LastYields)
                            {
                                var val = ProjectBIYieldViewModule.RetrieveErrorCount(error, test.WhichTest, yieldvm.LErrorMap);

                                if (piedatadict.ContainsKey(error))
                                {
                                    var preval = piedatadict[error];
                                    piedatadict[error] = preval + val;
                                }
                                else
                                {
                                    piedatadict.Add(error, val);
                                }
                            }
                        }
                    }

                    retestdatalist = piedatadict.ToList();

                    piedatadict["PASS"] = ProjectBIYieldViewModule.RetrieveErrorCount("PASS", yieldvm.LastYields[yieldvm.LastYields.Count - 1].WhichTest, yieldvm.LErrorMap);

                    var keys = piedatadict.Keys;
                    var namevaluepair = "";
                    foreach (var k in keys)
                    {
                        if (piedatadict[k] > 0)
                            namevaluepair = namevaluepair + "{ name:'" + k + "',y:" + piedatadict[k].ToString() + "},";
                    }

                    namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                    var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart.xml"));
                    ViewBag.rchartscript = tempscript.Replace("#ElementID#", "rfailurepie")
                        .Replace("#Title#", "Final Failure")
                        .Replace("#SERIESNAME#", "RFailure")
                        .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                }


                if (retestdatalist.Count > 0)
                {
                    retestparetofun(retestdatalist);
                }

                return View(yieldvm);
            }
            return View();
        }

        public ActionResult ProjectBIPeriodYield(string ProjectKey, string StartDate, string EndDate)
        {
            if (!string.IsNullOrEmpty(ProjectKey) && !string.IsNullOrEmpty(StartDate) && !string.IsNullOrEmpty(EndDate))
            {
                ViewBag.pjkey = ProjectKey;

                var pvm = ProjectViewModels.RetrieveOneProject(ProjectKey);

                var vmlist = new List<ProjectBIYieldViewModule>();
                var tempret = ProjectBIYieldViewModule.GetYieldByDateRange(ProjectKey, StartDate, EndDate, pvm);
                if (tempret.LastYields.Count > 0)
                {
                    vmlist.Add(tempret);
                }

                if (vmlist.Count > 0)
                {
                    var ChartxAxisValues = "";
                    //var ChartSearies = "";

                    var ftimelist = new List<string>();
                    var famountlist = new List<int>();
                    var ryieldlist = new List<double>();
                    var cyieldlist = new List<double>();
                    var maxamout = 0;

                    foreach (var item in vmlist)
                    {
                        ftimelist.Add(item.EndDate.ToString("yyyy-MM-dd"));
                        ryieldlist.Add(item.LastYield * 100.0);
                        cyieldlist.Add(item.CorrectLastYield * 100.0);

                        var tempfamount = 0;
                        foreach (var d in item.LastYields)
                        {
                            if (d.InputCount > tempfamount) { tempfamount = d.InputCount; }
                            if (d.InputCount > maxamout) { maxamout = d.InputCount; }
                        }
                        famountlist.Add(tempfamount);
                    }

                    //xaxis
                    foreach (var item in ftimelist)
                    {
                        ChartxAxisValues = ChartxAxisValues + "'" + item + "',";
                    }
                    ChartxAxisValues = ChartxAxisValues.Substring(0, ChartxAxisValues.Length - 1);


                    var famout = "";
                    foreach (var item in famountlist)
                    {
                        famout = famout + item.ToString() + ",";
                    }
                    famout = famout.Substring(0, famout.Length - 1);

                    var rtempvalue = "";
                    foreach (var item in ryieldlist)
                    {
                        rtempvalue = rtempvalue + item.ToString("0.00") + ",";
                    }
                    rtempvalue = rtempvalue.Substring(0, rtempvalue.Length - 1);

                    var crtempvalue = "";
                    foreach (var item in cyieldlist)
                    {
                        crtempvalue = crtempvalue + item.ToString("0.00") + ",";
                    }
                    crtempvalue = crtempvalue.Substring(0, crtempvalue.Length - 1);

                    //rederect url
                    var tempurl = "/BurnIn/ProjectBIPYieldDetail?ProjectKey=" + ProjectKey + "&StartDate=" + StartDate + "&EndDate=" + EndDate;
                    var reurl = "window.location.href = '" + tempurl + "'";

                    var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/BISuperYield.xml"));
                    ViewBag.chartscript = tempscript.Replace("#ElementID#", "periodyield")
                        .Replace("#Title#", "Period Yiled")
                        .Replace("#ChartxAxisValues#", ChartxAxisValues)
                        .Replace("#XAxisTitle#", "Date")
                        .Replace("#AmountMAX#", maxamout.ToString())
                        .Replace("#FirstAmount#", famout)
                        .Replace("#RetestYield#", rtempvalue)
                        .Replace("#CorrectiveYield#", crtempvalue)
                        .Replace("#REDIRECTURL#", reurl);
                }

                return View();
            }
            return View();
        }

        public ActionResult ProjectBIPYieldDetail(string ProjectKey, string StartDate, string EndDate)
        {
            if (!string.IsNullOrEmpty(ProjectKey) && !string.IsNullOrEmpty(StartDate) && !string.IsNullOrEmpty(EndDate))
            {
                ViewBag.StartDate = StartDate;
                ViewBag.EndDate = EndDate;

                var retestdatalist = new List<KeyValuePair<string, int>>();

                var pvm = ProjectViewModels.RetrieveOneProject(ProjectKey);
                var yieldvm = ProjectBIYieldViewModule.GetYieldByDateRange(ProjectKey, StartDate, EndDate, pvm);

                if (yieldvm.LastYields.Count > 0)
                {
                    var piedatadict = new Dictionary<string, int>();
                    var eklist = new List<string>();
                    foreach (var error in yieldvm.LErrorMap.Keys)
                    {
                        eklist.Add(error);
                    }

                    foreach (var error in eklist)
                    {
                        if (string.Compare(error, "PASS", true) != 0)
                        {
                            foreach (var test in yieldvm.LastYields)
                            {
                                var val = ProjectBIYieldViewModule.RetrieveErrorCount(error, test.WhichTest, yieldvm.LErrorMap);

                                if (piedatadict.ContainsKey(error))
                                {
                                    var preval = piedatadict[error];
                                    piedatadict[error] = preval + val;
                                }
                                else
                                {
                                    piedatadict.Add(error, val);
                                }
                            }
                        }
                    }

                    retestdatalist = piedatadict.ToList();

                    piedatadict["PASS"] = ProjectBIYieldViewModule.RetrieveErrorCount("PASS", yieldvm.LastYields[yieldvm.LastYields.Count - 1].WhichTest, yieldvm.LErrorMap);

                    var keys = piedatadict.Keys;
                    var namevaluepair = "";
                    foreach (var k in keys)
                    {
                        if (piedatadict[k] > 0)
                            namevaluepair = namevaluepair + "{ name:'" + k + "',y:" + piedatadict[k].ToString() + "},";
                    }

                    namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                    var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart.xml"));
                    ViewBag.rchartscript = tempscript.Replace("#ElementID#", "rfailurepie")
                        .Replace("#Title#", "Final Failure")
                        .Replace("#SERIESNAME#", "RFailure")
                        .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                }

                if (retestdatalist.Count > 0)
                {
                    retestparetofun(retestdatalist);
                }

                return View(yieldvm);
            }
            return View();
        }

        public ActionResult ProjectBIWaferYield(string ProjectKey, string Wafer)
        {
            if (!string.IsNullOrEmpty(ProjectKey) && !string.IsNullOrEmpty(Wafer))
            {
                ViewBag.pjkey = ProjectKey;

                //var pvm = ProjectViewModels.RetrieveOneProject(ProjectKey);

                var vmlist = new List<ProjectBIYieldViewModule>();
                var tempret = ProjectBIYieldViewModule.GetYieldByWafer(ProjectKey, Wafer);
                if (tempret.LastYields.Count > 0)
                {
                    vmlist.Add(tempret);
                }

                if (vmlist.Count > 0)
                {
                    var ChartxAxisValues = "";
                    //var ChartSearies = "";


                    var ftimelist = new List<string>();
                    var famountlist = new List<int>();
                    var ryieldlist = new List<double>();
                    var cyieldlist = new List<double>();
                    var maxamout = 0;

                    foreach (var item in vmlist)
                    {
                        ftimelist.Add(item.EndDate.ToString("yyyy-MM-dd"));
                        ryieldlist.Add(item.LastYield * 100.0);
                        cyieldlist.Add(item.CorrectLastYield * 100.0);

                        var tempfamount = 0;
                        foreach (var d in item.LastYields)
                        {
                            if (d.InputCount > tempfamount) { tempfamount = d.InputCount; }
                            if (d.InputCount > maxamout) { maxamout = d.InputCount; }
                        }
                        famountlist.Add(tempfamount);
                    }

                    //xaxis
                    foreach (var item in ftimelist)
                    {
                        ChartxAxisValues = ChartxAxisValues + "'" + item + "',";
                    }
                    ChartxAxisValues = ChartxAxisValues.Substring(0, ChartxAxisValues.Length - 1);


                    var famout = "";
                    foreach (var item in famountlist)
                    {
                        famout = famout + item.ToString() + ",";
                    }
                    famout = famout.Substring(0, famout.Length - 1);

                    var rtempvalue = "";
                    foreach (var item in ryieldlist)
                    {
                        rtempvalue = rtempvalue + item.ToString("0.00") + ",";
                    }
                    rtempvalue = rtempvalue.Substring(0, rtempvalue.Length - 1);

                    var crtempvalue = "";
                    foreach (var item in cyieldlist)
                    {
                        crtempvalue = crtempvalue + item.ToString("0.00") + ",";
                    }
                    crtempvalue = crtempvalue.Substring(0, crtempvalue.Length - 1);

                    //rederect url
                    var tempurl = "/BurnIn/ProjectBIWaferYieldDetail?ProjectKey=" + ProjectKey + "&Wafer=" + Wafer;
                    var reurl = "window.location.href = '" + tempurl + "'";

                    var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/BISuperYield.xml"));
                    ViewBag.chartscript = tempscript.Replace("#ElementID#", "waferyield")
                        .Replace("#Title#", "Wafer Yiled")
                        .Replace("#ChartxAxisValues#", ChartxAxisValues)
                        .Replace("#XAxisTitle#", "Date")
                        .Replace("#AmountMAX#", maxamout.ToString())
                        .Replace("#FirstAmount#", famout)
                        .Replace("#RetestYield#", rtempvalue)
                        .Replace("#CorrectiveYield#", crtempvalue)
                        .Replace("#REDIRECTURL#", reurl);
                }

                return View();
            }

            return View();
        }

        public ActionResult ProjectBIWaferYieldDetail(string ProjectKey, string Wafer)
        {
            if (!string.IsNullOrEmpty(ProjectKey) && !string.IsNullOrEmpty(Wafer))
            {
                var retestdatalist = new List<KeyValuePair<string, int>>();

                //var pvm = ProjectViewModels.RetrieveOneProject(ProjectKey);
                var yieldvm = ProjectBIYieldViewModule.GetYieldByWafer(ProjectKey, Wafer);

                if (yieldvm.LastYields.Count > 0)
                {
                    var piedatadict = new Dictionary<string, int>();
                    var eklist = new List<string>();
                    foreach (var error in yieldvm.LErrorMap.Keys)
                    {
                        eklist.Add(error);
                    }

                    foreach (var error in eklist)
                    {
                        if (string.Compare(error, "PASS", true) != 0)
                        {
                            foreach (var test in yieldvm.LastYields)
                            {
                                var val = ProjectBIYieldViewModule.RetrieveErrorCount(error, test.WhichTest, yieldvm.LErrorMap);

                                if (piedatadict.ContainsKey(error))
                                {
                                    var preval = piedatadict[error];
                                    piedatadict[error] = preval + val;
                                }
                                else
                                {
                                    piedatadict.Add(error, val);
                                }
                            }
                        }
                    }

                    retestdatalist = piedatadict.ToList();

                    piedatadict["PASS"] = ProjectBIYieldViewModule.RetrieveErrorCount("PASS", yieldvm.LastYields[yieldvm.LastYields.Count - 1].WhichTest, yieldvm.LErrorMap);

                    var keys = piedatadict.Keys;
                    var namevaluepair = "";
                    foreach (var k in keys)
                    {
                        if (piedatadict[k] > 0)
                            namevaluepair = namevaluepair + "{ name:'" + k + "',y:" + piedatadict[k].ToString() + "},";
                    }

                    namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                    var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart.xml"));
                    ViewBag.rchartscript = tempscript.Replace("#ElementID#", "rfailurepie")
                        .Replace("#Title#", "Final Failure")
                        .Replace("#SERIESNAME#", "RFailure")
                        .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                }

                if (retestdatalist.Count > 0)
                {
                    retestparetofun(retestdatalist);
                }

                return View(yieldvm);
            }
            return View();
        }
    }
}