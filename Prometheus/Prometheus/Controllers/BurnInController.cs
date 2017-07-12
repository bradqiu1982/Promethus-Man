using Prometheus.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Prometheus.Controllers
{
    public class BurnInController : Controller
    {

        public static void ProjectWeeklyTrend(Controller ctrl, string ProjectKey,int Weeks)
        {
            var vmlist = ProjectBIYieldViewModule.GetYieldByWeeks(ProjectKey, Weeks);
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
                var reurl = "window.location.href = '/BurnIn/ProjectBIWYieldDetail?ProjectKey=" + ProjectKey + "'" + "+'&EndDate='+this.category+'"+ "&Weeks="+Weeks.ToString()+"'";


                var tempscript = System.IO.File.ReadAllText(ctrl.Server.MapPath("~/Scripts/BISuperYield.xml"));
                ctrl.ViewBag.chartscript = tempscript.Replace("#ElementID#", "weeklyyield")
                    .Replace("#Title#", "Weekly Yield Trend")
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

        private List<SelectListItem> CreateSelectList1(List<string> valist, string defVal)
        {
            bool selected = false;
            var pslist = new List<SelectListItem>();

            foreach (var p in valist)
            {
                var pitem = new SelectListItem();
                pitem.Text = p;
                pitem.Value = p;
                if (!string.IsNullOrEmpty(defVal) && string.Compare(defVal, p, true) == 0)
                {
                    pitem.Selected = true;
                    selected = true;
                }
                pslist.Add(pitem);
            }

            if (!selected && pslist.Count > 0)
            {
                pslist[0].Selected = true;
            }

            return pslist;
        }


        // GET: BurnIn
        public ActionResult BurnInMainPage(string ProjectKey)
        {
            if (!string.IsNullOrEmpty(ProjectKey))
            {
                ViewBag.pjkey = ProjectKey;
                ProjectWeeklyTrend(this, ProjectKey,4);
                ViewBag.waferlist = CreateWaferList(ProjectKey);

                var sarray = new string[] { "8", "16", "24", "32", "40", "48", "56" };
                var slist = new List<string>();
                slist.AddRange(sarray);
                ViewBag.weeklylist = CreateSelectList1(slist, "");
            }
            return View();
        }

        public ActionResult ProjectBIWeeklyYield(string ProjectKey, string Weeks)
        {
            if (!string.IsNullOrEmpty(ProjectKey) && !string.IsNullOrEmpty(Weeks))
            {
                ViewBag.pjkey = ProjectKey;
                ProjectWeeklyTrend(this, ProjectKey,Convert.ToInt32(Weeks));
            }
            return View();
        }

        private void retestparetofun(List<KeyValuePair<string, int>> retestdatalist,string pjkey=null)
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

                var reurl = "";
                if (!string.IsNullOrEmpty(pjkey))
                {
                    reurl = "window.location.href = '/Project/ProjectErrAbbr?ProjectKey=" + pjkey + "'" + "+'&ErrAbbr='+this.category";
                }

                var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/ParetoChart.xml"));
                ViewBag.rparetoscript = tempscript.Replace("#ElementID#", "rparetochart")
                    .Replace("#Title#", "Pareto of Final Defect")
                    .Replace("#XAxisTitle#", "Defect")
                    .Replace("#ChartxAxisValues#", ChartxAxisValues)
                    .Replace("#AmountMAX#", sum.ToString())
                    .Replace("#PCount#", pcountvalue)
                    .Replace("#ABPercent#", abpecentvalue)
                    .Replace("#PPercent#", ppecentvalue)
                    .Replace("#REDIRECTURL#", reurl);
            }
        }

        public ActionResult ProjectBIWYieldDetail(string ProjectKey, string EndDate,string Weeks)
        {
            if (!string.IsNullOrEmpty(ProjectKey) && !string.IsNullOrEmpty(EndDate))
            {
                ViewBag.ProjectKey = ProjectKey;
                ViewBag.Weeks = Weeks;

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
                    retestparetofun(retestdatalist, ProjectKey);
                }

                return View(yieldvm);
            }
            return View();
        }

        public ActionResult ProjectBIDailyYield(string ProjectKey, string StartDate, string EndDate)
        {
            if (ProjectKey != null && StartDate != null && EndDate != null)
            {
                var EndDate2 = DateTime.Parse(EndDate).AddDays(1).ToString();

                ViewBag.pjkey = ProjectKey;
                var vmlist = ProjectBIYieldViewModule.GetYieldByDay(ProjectKey, StartDate, EndDate2);
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
                    var reurl = "window.location.href = '/BurnIn/ProjectBIDailyYieldDetail?ProjectKey=" + ProjectKey + "'" + "+'&EndDate='+this.category+'&VStartDate='+'" + StartDate + "'+'&VEndDate='+'" + EndDate2 + "'";

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
                    retestparetofun(retestdatalist, ProjectKey);
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
                        .Replace("#Title#", "Period Yield")
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
                    retestparetofun(retestdatalist, ProjectKey);
                }

                return View(yieldvm);
            }
            return View();
        }

        private void projectwaferyield(string ProjectKey, string Wafer)
        {
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
                    var tempurl = "/BurnIn/ProjectBIWaferYieldDetail?ProjectKey=" + ProjectKey + "&Wafer=" + Wafer+ "&Pjkey4return="+ ProjectKey;
                    var reurl = "window.location.href = '" + tempurl + "'";

                    var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/BISuperYield.xml"));
                    ViewBag.chartscript = tempscript.Replace("#ElementID#", "waferyield")
                        .Replace("#Title#", "Project Wafer "+ Wafer + " Yield")
                        .Replace("#ChartxAxisValues#", ChartxAxisValues)
                        .Replace("#XAxisTitle#", "Date")
                        .Replace("#AmountMAX#", maxamout.ToString())
                        .Replace("#FirstAmount#", famout)
                        .Replace("#RetestYield#", rtempvalue)
                        .Replace("#CorrectiveYield#", crtempvalue)
                        .Replace("#REDIRECTURL#", reurl);
                }
        }


        private void wholewaferyield(string ProjectKey,string Wafer)
        {
            var vmlist = new List<ProjectBIYieldViewModule>();
            var tempret = ProjectBIYieldViewModule.GetYieldByWafer("", Wafer);
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
                var tempurl = "/BurnIn/ProjectBIWaferYieldDetail?ProjectKey=&Wafer=" + Wafer + "&Pjkey4return=" + ProjectKey;
                var reurl = "window.location.href = '" + tempurl + "'";

                var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/BISuperYield.xml"));
                ViewBag.wholechartscript = tempscript.Replace("#ElementID#", "wholewaferyield")
                    .Replace("#Title#", "Whole Wafer "+ Wafer + " Yield")
                    .Replace("#ChartxAxisValues#", ChartxAxisValues)
                    .Replace("#XAxisTitle#", "Date")
                    .Replace("#AmountMAX#", maxamout.ToString())
                    .Replace("#FirstAmount#", famout)
                    .Replace("#RetestYield#", rtempvalue)
                    .Replace("#CorrectiveYield#", crtempvalue)
                    .Replace("#REDIRECTURL#", reurl);
            }
        }

        public ActionResult ProjectBIWaferYield(string ProjectKey, string Wafer)
        {
            if (!string.IsNullOrEmpty(ProjectKey) && !string.IsNullOrEmpty(Wafer))
            {
                ViewBag.pjkey = ProjectKey;
                projectwaferyield(ProjectKey, Wafer);
                wholewaferyield(ProjectKey, Wafer);

                return View();
            }

            return View();
        }

        public ActionResult ProjectBIWaferYieldDetail(string ProjectKey, string Wafer,string Pjkey4return)
        {
            if (!string.IsNullOrEmpty(Wafer))
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
                    retestparetofun(retestdatalist, Pjkey4return);
                }

                yieldvm.ProjectKey = Pjkey4return;
                yieldvm.Wafer = Wafer;
                return View(yieldvm);
            }
            return View();
        }

        public ActionResult BIError(string ProjectKey)
        {
            ViewBag.pjkey = ProjectKey;

            var vm = ProjectErrorViewModels.RetrieveErrorByPJKey(ProjectErrorViewModels.BURNIN,this);

            var countdict = new Dictionary<string, int>();
            foreach (var item in vm)
            {
                var errorcode = item.OrignalCode.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries)[0];
                if (countdict.ContainsKey(errorcode))
                {
                    countdict[errorcode] = countdict[errorcode] + item.ErrorCount;
                }
                else
                {
                    countdict.Add(errorcode, item.ErrorCount);
                }
            }


            var sumvm = new List<ProjectErrorViewModels>();
            foreach (var kv in countdict)
            {
                var temp = new ProjectErrorViewModels();
                temp.OrignalCode = kv.Key;
                temp.ErrorCount = kv.Value;
                sumvm.Add(temp);
            }

            sumvm.Sort(delegate (ProjectErrorViewModels item1, ProjectErrorViewModels item2)
            {
                if (item1.ErrorCount > item2.ErrorCount)
                {
                    return -1;
                }
                else if (item1.ErrorCount < item2.ErrorCount)
                {
                    return 1;
                }
                else
                    return 0;
            });

            var piedatadict = new Dictionary<string, int>();
                foreach (var item in sumvm)
                {
                    if (!piedatadict.ContainsKey(item.OrignalCode))
                    {
                        piedatadict.Add(item.OrignalCode, item.ErrorCount);
                    }
                }

                var keys = piedatadict.Keys;
                if (keys.Count > 0)
                {
                    var namevaluepair = "";
                    foreach (var k in keys)
                    {
                        namevaluepair = namevaluepair + "{ name:'" + k + "',y:" + piedatadict[k].ToString() + "},";
                    }

                    namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                    var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart.xml"));
                    ViewBag.chartscript = tempscript.Replace("#ElementID#", "failurepie")
                        .Replace("#Title#", "BURNIN Realtime Failure")
                        .Replace("#SERIESNAME#", "Failure")
                        .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                }

            ViewBag.sumvm = sumvm;

                return View(vm);
        }

        public ActionResult ErrorAttach(string ErrorKey)
        {
            if (!string.IsNullOrEmpty(ErrorKey))
            {
                var tempvm = ProjectErrorViewModels.RetrieveErrorByErrorKey(ErrorKey,this);
                return View(tempvm[0]);
            }
            return RedirectToAction("ViewAll", "Project");
        }

        public ActionResult UpdateBIError(string ErrorKey)
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {

            }
            else
            {
                var ck = new Dictionary<string, string>();
                ck.Add("logonredirectctrl", "BurnIn");
                ck.Add("logonredirectact", "UpdateBIError");
                ck.Add("errorkey", ErrorKey);
                CookieUtility.SetCookie(this, ck);
                return RedirectToAction("LoginUser", "User");
            }

            var key = "";
            if (!string.IsNullOrEmpty(ErrorKey))
            {
                var ck = new Dictionary<string, string>();
                ck.Add("errorkey", ErrorKey);
                ck.Add("currentaction", "UpdateBIError");
                CookieUtility.SetCookie(this, ck);
                key = ErrorKey;
            }
            else if (ckdict.ContainsKey("errorkey") && !string.IsNullOrEmpty(ckdict["errorkey"]))
            {
                key = ckdict["errorkey"];
                var ck = new Dictionary<string, string>();
                ck.Add("currentaction", "UpdateBIError");
                CookieUtility.SetCookie(this, ck);
            }

            if (!string.IsNullOrEmpty(key))
            {
                var vm = ProjectErrorViewModels.RetrieveErrorByErrorKey(key,this);
                //var FirstEngineer = ProjectViewModels.RetrieveOneProject(vm[0].ProjectKey).FirstEngineer;
                var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

                var syscfgdict = CfgUtility.GetSysConfig(this);
                if(syscfgdict["BITEAM"].ToUpper().Contains(updater.ToUpper()))
                {
                    ViewBag.assigee = true;
                }
                else
                {
                    ViewBag.assigee = false;
                }
                return View(vm[0]);
            }

            return View();
        }

        public ActionResult UpdateBIError2(string ErrorCode)
        {
            var tempvm = ProjectErrorViewModels.RetrieveErrorByPJKey(ProjectErrorViewModels.BURNIN, ErrorCode,this);
            var ErrorKey = "";
            if (tempvm.Count > 0)
            {
                ErrorKey = tempvm[0].ErrorKey;
            }

            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {

            }
            else
            {
                var ck = new Dictionary<string, string>();
                ck.Add("logonredirectctrl", "BurnIn");
                ck.Add("logonredirectact", "UpdateBIError2");
                ck.Add("errorkey", ErrorKey);
                CookieUtility.SetCookie(this, ck);
                return RedirectToAction("LoginUser", "User");
            }

            var key = "";
            if (!string.IsNullOrEmpty(ErrorKey))
            {
                var ck = new Dictionary<string, string>();
                ck.Add("errorkey", ErrorKey);
                ck.Add("currentaction", "UpdateBIError2");
                CookieUtility.SetCookie(this, ck);
                key = ErrorKey;
            }
            else if (ckdict.ContainsKey("errorkey") && !string.IsNullOrEmpty(ckdict["errorkey"]))
            {
                key = ckdict["errorkey"];
                var ck = new Dictionary<string, string>();
                ck.Add("currentaction", "UpdateBIError2");
                CookieUtility.SetCookie(this, ck);
            }

            if (!string.IsNullOrEmpty(key))
            {
                var vm = ProjectErrorViewModels.RetrieveErrorByErrorKey(key,this);
                //var FirstEngineer = ProjectViewModels.RetrieveOneProject(vm[0].ProjectKey).FirstEngineer;
                var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
                var syscfgdict = CfgUtility.GetSysConfig(this);
                if (syscfgdict["BITEAM"].ToUpper().Contains(updater.ToUpper()))
                {
                    ViewBag.assigee = true;
                }
                else
                {
                    ViewBag.assigee = false;
                }
                return View("UpdateBIError",vm[0]);
            }

            return View();
        }

        [HttpPost, ActionName("UpdateBIError")]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateBIErrorPost()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

            var currenttime = DateTime.Now.ToString();

            var vm = new ProjectErrorViewModels();
            vm.ErrorKey = Request.Form["ErrorKey"];
            vm.ShortDesc = Request.Form["ShortDesc"];
            vm.Reporter = ckdict["logonuser"].Split(new char[] { '|' })[0];

            vm.UpdateShortDesc();

            var temphtml = Request.Form["editor1"];
            if (!string.IsNullOrEmpty(temphtml))
            {
                vm.Description = SeverHtmlDecode.Decode(this,temphtml);
                ProjectErrorViewModels.StoreErrorComment(vm.ErrorKey, vm.dbDescription, PJERRORCOMMENTTYPE.Description, vm.Reporter, currenttime);
            }

            var urls = ReceiveAttachFiles();

            var detailcontenturl = string.Empty;
            var detailcontentreffile = string.Empty;
            if (!string.IsNullOrEmpty(Request.Form["detailattach"]))
            {
                var internalreportfile = Request.Form["detailattach"];
                var originalname = Path.GetFileNameWithoutExtension(internalreportfile)
                    .Replace(" ", "_").Replace("#", "").Replace("'", "")
                    .Replace("&", "").Replace("?", "").Replace("%", "").Replace("+", "");

                foreach (var r in urls)
                {
                    if (r.Contains(originalname))
                    {
                        detailcontentreffile = originalname;
                        detailcontenturl = r;
                        break;
                    }
                }
            }

            var rootcontenturl = string.Empty;
            var rootcontentreffile = string.Empty;
            if (!string.IsNullOrEmpty(Request.Form["rootattach"]))
            {
                var internalreportfile = Request.Form["rootattach"];
                var originalname = Path.GetFileNameWithoutExtension(internalreportfile)
                    .Replace(" ", "_").Replace("#", "").Replace("'", "")
                    .Replace("&", "").Replace("?", "").Replace("%", "").Replace("+", "");

                foreach (var r in urls)
                {
                    if (r.Contains(originalname))
                    {
                        rootcontentreffile = originalname;
                        rootcontenturl = r;
                        break;
                    }
                }
            }

            var resultcontenturl = string.Empty;
            var resultcontentreffile = string.Empty;
            if (!string.IsNullOrEmpty(Request.Form["resultattach"]))
            {
                var internalreportfile = Request.Form["resultattach"];
                var originalname = Path.GetFileNameWithoutExtension(internalreportfile)
                    .Replace(" ", "_").Replace("#", "").Replace("'", "")
                    .Replace("&", "").Replace("?", "").Replace("%", "").Replace("+", "");

                foreach (var r in urls)
                {
                    if (r.Contains(originalname))
                    {
                        resultcontentreffile = originalname;
                        resultcontenturl = r;
                        break;
                    }
                }
            }


            var failuredetail = string.Empty;
            var result = string.Empty;

            if (Request.Form["editor3"] != null)
            {
                var com = new ErrorComments();
                com.Comment = SeverHtmlDecode.Decode(this,Request.Form["editor3"]);
                if (!string.IsNullOrEmpty(com.Comment))
                {
                    if (!string.IsNullOrEmpty(detailcontenturl))
                    {
                        com.Comment = com.Comment + "<p><a href='" + detailcontenturl + "' target='_blank'>Reference File: " + detailcontentreffile + " " + "</a></p>";
                    }

                    ProjectErrorViewModels.StoreErrorComment(vm.ErrorKey, com.dbComment, PJERRORCOMMENTTYPE.FailureDetail, vm.Reporter, currenttime);
                    failuredetail = com.Comment;
                }
            }

            if (Request.Form["resulteditor"] != null)
            {
                var com = new ErrorComments();
                com.Comment = SeverHtmlDecode.Decode(this,Request.Form["resulteditor"]);
                if (!string.IsNullOrEmpty(com.Comment))
                {
                    if (!string.IsNullOrEmpty(resultcontenturl))
                    {
                        com.Comment = com.Comment + "<p><a href='" + resultcontenturl + "' target='_blank'>Reference File: " + resultcontentreffile + " " + "</a></p>";
                    }

                    ProjectErrorViewModels.StoreErrorComment(vm.ErrorKey, com.dbComment, PJERRORCOMMENTTYPE.Result, vm.Reporter, currenttime);
                    result = com.Comment;
                }
            }

            if (Request.Form["editor2"] != null)
            {
                var com = new ErrorComments();
                com.Comment = SeverHtmlDecode.Decode(this,Request.Form["editor2"]);
                if (!string.IsNullOrEmpty(com.Comment))
                {
                    if (!string.IsNullOrEmpty(rootcontenturl))
                    {
                        com.Comment = com.Comment + "<p><a href='" + rootcontenturl + "' target='_blank'>Reference File: " + rootcontentreffile + " " + "</a></p>";
                    }

                    ProjectErrorViewModels.StoreErrorComment(vm.ErrorKey, com.dbComment, PJERRORCOMMENTTYPE.RootCause, vm.Reporter, currenttime);

                    UserKPIVM.AddUserDailyRank(vm.ErrorKey, updater, UserRankType.ADDITIONAL
                        , "Add analyse to " + vm.ProjectKey + " " + vm.OrignalCode, "/BurnIn/UpdateBIError?ErrorKey=" + vm.ErrorKey, 6);

                    if (string.IsNullOrEmpty(failuredetail))
                    {
                        var com1 = new ErrorComments();
                        com1.Comment = "<p>To Be Edit</p>";
                        if (!string.IsNullOrEmpty(detailcontenturl))
                        {
                            com1.Comment = com1.Comment + "<p><a href='" + detailcontenturl + "' target='_blank'>Reference File: " + detailcontentreffile + " " + "</a></p>";
                        }
                        ProjectErrorViewModels.StoreErrorComment(vm.ErrorKey, com1.dbComment, PJERRORCOMMENTTYPE.FailureDetail, vm.Reporter, currenttime);
                    }

                    if (string.IsNullOrEmpty(result))
                    {
                        var com1 = new ErrorComments();
                        com1.Comment = "<p>To Be Edit</p>";
                        if (!string.IsNullOrEmpty(resultcontenturl))
                        {
                            com1.Comment = com1.Comment + "<p><a href='" + resultcontenturl + "' target='_blank'>Reference File: " + resultcontentreffile + " " + "</a></p>";
                        }
                        ProjectErrorViewModels.StoreErrorComment(vm.ErrorKey, com1.dbComment, PJERRORCOMMENTTYPE.Result, vm.Reporter, currenttime);
                    }
                }
            }

            if (!string.IsNullOrEmpty(Request.Form["attachmentupload"]))
            {

                var internalreportfile = Request.Form["attachmentupload"];
                var originalname = Path.GetFileNameWithoutExtension(internalreportfile)
                    .Replace(" ", "_").Replace("#", "").Replace("'", "")
                    .Replace("&", "").Replace("?", "").Replace("%", "").Replace("+", "");

                var url = "";
                foreach (var r in urls)
                {
                    if (r.Contains(originalname))
                    {
                        url = r;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(url))
                {
                    var tempkeys = url.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                    var dockey = tempkeys[tempkeys.Length - 1];

                    ProjectErrorViewModels.StoreErrorAttachment(vm.ErrorKey, url);
                    UserKPIVM.AddUserAttachDailyRank(vm.ErrorKey, updater, UserRankType.ADDITIONAL
                        , "Add attachment to " + vm.ProjectKey + " " + vm.OrignalCode, "/BurnIn/UpdateBIError?ErrorKey=" + vm.ErrorKey, 2,dockey,this);
                }
            }

            var dict = new RouteValueDictionary();
            dict.Add("ErrorKey", vm.ErrorKey);
            return RedirectToAction("UpdateBIError", "BurnIn", dict);
        }

        public ActionResult DeleteErrorComment(string ErrorKey, string CommentType, string Date)
        {
            if (!string.IsNullOrEmpty(ErrorKey) && !string.IsNullOrEmpty(CommentType) && !string.IsNullOrEmpty(Date))
            {
                ProjectErrorViewModels.DeleteErrorComment(ErrorKey, CommentType, Date);
                var dict = new RouteValueDictionary();
                dict.Add("ErrorKey", ErrorKey);
                return RedirectToAction("UpdateBIError", "BurnIn", dict);
            }
            return RedirectToAction("ViewAll", "Project");
        }

        public ActionResult DeleteErrorAttachment(string errorkey, string filename)
        {
            if (!string.IsNullOrEmpty(errorkey) && !string.IsNullOrEmpty(filename))
            {
                var tempvm = ProjectErrorViewModels.RetrieveErrorByErrorKey(errorkey,this);
                //var FirstEngineer = ProjectViewModels.RetrieveOneProject(tempvm[0].ProjectKey).FirstEngineer;
                var ckdict = CookieUtility.UnpackCookie(this);
                var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

                ProjectErrorViewModels.DeleteAttachment(errorkey, filename);

                var dict = new RouteValueDictionary();
                dict.Add("ErrorKey", errorkey);
                return RedirectToAction("UpdateBIError", "BurnIn", dict);

            }
            else if (!string.IsNullOrEmpty(errorkey))
            {
                var dict = new RouteValueDictionary();
                dict.Add("ErrorKey", errorkey);
                return RedirectToAction("UpdateBIError", "BurnIn", dict);
            }
            else
            {
                return RedirectToAction("ViewAll", "Project");
            }
        }

        private List<string> ReceiveAttachFiles()
        {
            var ret = new List<string>();

            try
            {
                foreach (string fl in Request.Files)
                {
                    if (fl != null && Request.Files[fl].ContentLength > 0)
                    {
                        string fn = Path.GetFileName(Request.Files[fl].FileName)
                            .Replace(" ", "_").Replace("#", "").Replace("'", "")
                            .Replace("&", "").Replace("?", "").Replace("%", "").Replace("+", "");

                        string datestring = DateTime.Now.ToString("yyyyMMdd");
                        string imgdir = Server.MapPath("~/userfiles") + "\\docs\\" + datestring + "\\";

                        if (!Directory.Exists(imgdir))
                        {
                            Directory.CreateDirectory(imgdir);
                        }

                        fn = Path.GetFileNameWithoutExtension(fn) + "-" + DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(fn);
                        Request.Files[fl].SaveAs(imgdir + fn);

                        var url = "/userfiles/docs/" + datestring + "/" + fn;

                        ret.Add(url);
                    }
                }

            }
            catch (Exception ex)
            { return ret; }

            return ret;
        }


        public ActionResult UpdateErrorComment(string ErrorKey, string CommentType, string Date)
        {
            if (!string.IsNullOrEmpty(ErrorKey) && !string.IsNullOrEmpty(CommentType) && !string.IsNullOrEmpty(Date))
            {
                var errorcomment = ProjectErrorViewModels.RetrieveSPComment(ErrorKey, CommentType, Date);
                return View(errorcomment);
            }

            return RedirectToAction("ViewAll", "Project");
        }

        [HttpPost, ActionName("UpdateErrorComment")]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateErrorCommentPost()
        {
            var errorkey = Request.Form["HErrorKey"];
            var commenttype = Request.Form["HType"];
            var commentdate = Request.Form["HDate"];

            if (!string.IsNullOrEmpty(Request.Form["editor1"]))
            {
                var tempcommment = new ErrorComments();
                tempcommment.Comment = SeverHtmlDecode.Decode(this,Request.Form["editor1"]);
                ProjectErrorViewModels.UpdateSPComment(errorkey, commenttype, commentdate, tempcommment.dbComment);
            }
            else
            {
                var tempcommment = new ErrorComments();
                tempcommment.Comment = "<p>To Be Edit</p>";
                ProjectErrorViewModels.UpdateSPComment(errorkey, commenttype, commentdate, tempcommment.dbComment);
            }

            var dict = new RouteValueDictionary();
            dict.Add("ErrorKey", errorkey);
            return RedirectToAction("UpdateBIError", "BurnIn", dict);
        }


    }
}