using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Prometheus.Models;

namespace Prometheus.Controllers
{
    public class PJReportController : Controller
    {

        private List<SelectListItem> CreateSelectList(List<string> valist, string defVal)
        {
            var pslist = new List<SelectListItem>();
            foreach (var p in valist)
            {
                var pitem = new SelectListItem();
                pitem.Text = p;
                pitem.Value = p;
                if (!string.IsNullOrEmpty(defVal) && string.Compare(defVal, p) == 0)
                {
                    pitem.Selected = true;
                }
                pslist.Add(pitem);
            }

            if (string.IsNullOrEmpty(defVal) && pslist.Count > 0)
            {
                pslist[0].Selected = true;
            }

            return pslist;
        }

        // GET: PJReport
        public ActionResult IBook()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {

            }
            else
            {
                var ck = new Dictionary<string, string>();
                ck.Add("logonredirectctrl", "PJReport");
                ck.Add("logonredirectact", "IBook");
                CookieUtility.SetCookie(this, ck);
                return RedirectToAction("LoginUser", "User");
            }

            ViewBag.PJList = ProjectViewModels.RetrieveAllProjectKey();

            var temptypelist = new List<string>();
            temptypelist.Add(PJReportType.YieldTrend);
            temptypelist.Add(PJReportType.IssueTrend);
            temptypelist.Add(PJReportType.FailureTrend);
            temptypelist.Add(PJReportType.MonthlyPareto);
            ViewBag.RepTypeList = temptypelist;

            var username = ckdict["logonuser"].Split(new char[] { '|' })[0];
            var vm = PJReportViewModels.RetrieveBookReportRecord(username);

            var rsilist = new List<string>();
            string[] rlist = { ReportSelectTime.MON, ReportSelectTime.TUE, ReportSelectTime.WEN, ReportSelectTime.THU, ReportSelectTime.FRI, ReportSelectTime.SAT, ReportSelectTime.SUN, ReportSelectTime.EVE };
            rsilist.AddRange(rlist);
            if (vm != null)
            {
                ViewBag.ReportTimeList = CreateSelectList(rsilist, vm.reporttime);
            }
            else
            {
                ViewBag.ReportTimeList = CreateSelectList(rsilist,"");
            }
            
            return View(vm);
        }

        [HttpPost, ActionName("IBook")]
        [ValidateAntiForgeryToken]
        public ActionResult IBookPost()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

            var vm = new PJReportViewModels();
            vm.username = updater;

            for (var i = 0; i < 20; i++)
            {
                if (Request.Form["pckbox" + i] != null && string.Compare(Request.Form["pckbox" + i], "true", true) == 0)
                {
                    vm.ProjectList.Add(Request.Form["PJKEY" + i]);
                }
            }

            for (var i = 0; i < 20; i++)
            {
                if (Request.Form["rckbox" + i] != null && string.Compare(Request.Form["rckbox" + i], "true", true) == 0)
                {
                    vm.ReportTypeList.Add(Request.Form["RPType" + i]);
                }
            }

            vm.reporttime = Request.Form["ReportTimeList"].ToString();
            vm.lastupdatetime = DateTime.Now;
            vm.StoreBookReportRecord();


            ViewBag.PJList = ProjectViewModels.RetrieveAllProjectKey();

            var temptypelist = new List<string>();
            temptypelist.Add(PJReportType.YieldTrend);
            temptypelist.Add(PJReportType.IssueTrend);
            temptypelist.Add(PJReportType.FailureTrend);
            temptypelist.Add(PJReportType.MonthlyPareto);
            ViewBag.RepTypeList = temptypelist;

            vm = PJReportViewModels.RetrieveBookReportRecord(updater);
            var rsilist = new List<string>();
            string[] rlist = { ReportSelectTime.MON, ReportSelectTime.TUE, ReportSelectTime.WEN, ReportSelectTime.THU, ReportSelectTime.FRI, ReportSelectTime.SAT, ReportSelectTime.SUN, ReportSelectTime.EVE };
            rsilist.AddRange(rlist);
            if (vm != null)
            {
                ViewBag.ReportTimeList = CreateSelectList(rsilist, vm.reporttime);
            }
            else
            {
                ViewBag.ReportTimeList = CreateSelectList(rsilist, "");
            }

            return RedirectToAction("ViewReport");
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
            else
            {
                ViewBag.rparetoscript = string.Empty;
            }
        }

        private void MonthlyPareto(string ProjectKey)
        {
            var edate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " 07:30:00");
            var sdate = edate.AddDays(-30);
            var retestdatalist = new List<KeyValuePair<string, int>>();
            var pvm = ProjectViewModels.RetrieveOneProject(ProjectKey);
            var yieldvm = ProjectYieldViewModule.GetYieldByDateRange(ProjectKey, sdate.ToString(), edate.ToString(), pvm);

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
                            var val = ProjectYieldViewModule.RetrieveErrorCount(error, test.WhichTest, yieldvm.LErrorMap);

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
            }


            if (retestdatalist.Count > 0)
            {
                retestparetofun(retestdatalist);
            }
        }


        private void IssueCountTrend(string ProjectKey)
        {
            var edate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " 07:30:00");
            var sdate = edate.AddDays(-30);
            var pendingissues = IssueViewModels.RetrieveIssueForIncreaseSummary(ProjectKey,sdate.ToString());
            var solvedissues = IssueViewModels.RetrieveIssueForSolveSummary(ProjectKey, sdate.ToString());
            
            var datelist = new List<string>();
            var pendinglist = new List<int>();
            var solvedlist = new List<int>();
            var pendingsum = 0;
            var slovedsum = 0;

            for (var temptime = sdate; temptime < edate; )
            {
                var tempstime = temptime;
                temptime = temptime.AddDays(1);

                foreach (var item in pendingissues)
                {
                    if (item.ReportDate < temptime && item.ReportDate >= tempstime)
                    {
                        pendingsum = pendingsum + 1;
                    }

                }

                foreach (var item in solvedissues)
                {
                    if (item.ResolvedDate <= temptime && item.ResolvedDate >= tempstime)
                    {
                        slovedsum = slovedsum + 1;
                    }
                }

                datelist.Add(temptime.ToString());
                pendinglist.Add(pendingsum);
                solvedlist.Add(slovedsum);
            }

            var ChartxAxisValues = "";
            var ChartSearies = "";
            //xaxis
            foreach (var item in datelist)
            {
                ChartxAxisValues = ChartxAxisValues + "'" + DateTime.Parse(item).ToString("yyyy-MM-dd") + "',";
            }
            ChartxAxisValues = ChartxAxisValues.Substring(0, ChartxAxisValues.Length - 1);


            //yaxis
            ChartSearies = "{name:'Issue Increase',data:[<fvalue>]},{name:'Issue Solved',data:[<cvalue>]}";

            var tempvalue = "";
            foreach (var item in pendinglist)
            {
                tempvalue = tempvalue + item.ToString() + ",";
            }
            tempvalue = tempvalue.Substring(0, tempvalue.Length - 1);
            ChartSearies = ChartSearies.Replace("<fvalue>", tempvalue);

            tempvalue = "";
            foreach (var item in solvedlist)
            {
                tempvalue = tempvalue + item.ToString("0.00") + ",";
            }
            tempvalue = tempvalue.Substring(0, tempvalue.Length - 1);
            ChartSearies = ChartSearies.Replace("<cvalue>", tempvalue);

            var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/AreaChart.xml"));
            ViewBag.issuetrendscript = tempscript.Replace("#ElementID#", "issuetrendchart")
                .Replace("#Title#", "Issue Count Trend")
                .Replace("#ChartxAxisValues#", ChartxAxisValues)
                .Replace("#NAMEVALUEPAIRS#", ChartSearies);
        }

        private string FailurePieChart(Dictionary<string,int> piedatadict, string date)
        {
            if (piedatadict.Count > 0)
            {
                var keys = piedatadict.Keys;
                var namevaluepair = "";
                foreach (var k in keys)
                {
                    if (piedatadict[k] > 0)
                        namevaluepair = namevaluepair + "{ name:'" + k + "',y:" + piedatadict[k].ToString() + "},";
                }

                namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart.xml"));
                return tempscript.Replace("#Title#", date+" Failure")
                    .Replace("#SERIESNAME#", "Failure")
                    .Replace("#NAMEVALUEPAIRS#", namevaluepair);
            }

            return string.Empty;
        }

        public List<string> PJFailureTrend(string ProjectKey)
        {
            var ret = new List<string>();

            var edate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " 07:30:00");
            var sdate = edate.AddDays(-30);
            var pendingissues = IssueViewModels.RetrieveIssueForIncreaseSummary(ProjectKey, sdate.ToString());

            var datelist = new List<string>();
            var dictlist = new List<Dictionary<string, int>>();
            

            for (var temptime = sdate; temptime < edate;)
            {
                var tempstime = temptime;
                temptime = temptime.AddDays(10);

                var tempdict = new Dictionary<string, int>();

                foreach (var item in pendingissues)
                {
                    if (item.ReportDate < temptime && item.ReportDate >= tempstime 
                        && string.Compare(item.Reporter, "System",true) == 0)
                    {
                        var errorkey = item.Summary.Split(new string[] { "failed for", "@" }, StringSplitOptions.None)[1].Trim();
                        if (tempdict.ContainsKey(errorkey))
                        {
                            tempdict[errorkey] = tempdict[errorkey] + 1;
                        }
                        else
                        {
                            tempdict.Add(errorkey, 1);
                        }
                    }
                }

                dictlist.Add(tempdict);
                datelist.Add(temptime.ToString("yyyy-MM-dd"));
            }

            var idx = 0;
            foreach (var item in dictlist)
            {
                ret.Add(FailurePieChart(item,datelist[idx]));
                idx = idx + 1;
            }

            return ret;
        }

        public ActionResult UserBookedReport(string username)
        {
            var ck = new Dictionary<string, string>();
            ck.Add("logonuser", username);
            CookieUtility.SetCookie(this, ck);
            return RedirectToAction("ViewReport");
        }

        public ActionResult ViewReport()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {

            }
            else
            {
                var ck = new Dictionary<string, string>();
                ck.Add("logonredirectctrl", "PJReport");
                ck.Add("logonredirectact", "ViewReport");
                CookieUtility.SetCookie(this, ck);
                return RedirectToAction("LoginUser", "User");
            }

            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
            var vm = PJReportViewModels.RetrieveBookReportRecord(updater);
            if (vm == null)
            {
                return RedirectToAction("IBook");
            }

            var pjreportdict = new Dictionary<string, PJReportItem>();

            foreach (var pjkey in vm.ProjectList)
            {
                foreach (var reptype in vm.ReportTypeList)
                {
                    if (string.Compare(reptype, PJReportType.YieldTrend) == 0)
                    {
                        ProjectController.ProjectWeeklyTrend(this, pjkey);
                        var reportitem = new PJReportItem();
                        if (!string.IsNullOrEmpty(this.ViewBag.chartscript))
                        {
                            reportitem.YieldTrend = this.ViewBag.chartscript.Replace("weeklyyield",pjkey+ "weeklyyield");
                            this.ViewBag.chartscript = null;
                            if (!pjreportdict.ContainsKey(pjkey))
                            {
                                pjreportdict.Add(pjkey, reportitem);
                            }
                            else
                            {
                                pjreportdict[pjkey].YieldTrend = reportitem.YieldTrend;
                            }
                            
                        }

                    }

                    if (string.Compare(reptype, PJReportType.MonthlyPareto) == 0)
                    {
                        MonthlyPareto(pjkey);
                        if (pjreportdict.ContainsKey(pjkey))
                        {
                            pjreportdict[pjkey].MonthlyPareto = ViewBag.rparetoscript.Replace("rparetochart", pjkey + "rparetochart");
                            ViewBag.rparetoscript = null;
                        }
                        else
                        {
                            var reportitem = new PJReportItem();
                            reportitem.MonthlyPareto = ViewBag.rparetoscript.Replace("rparetochart", pjkey + "rparetochart");
                            ViewBag.rparetoscript = null;
                            pjreportdict.Add(pjkey, reportitem);
                        }
                    }

                    if (string.Compare(reptype, PJReportType.IssueTrend) == 0)
                    {
                        IssueCountTrend(pjkey);
                        if (pjreportdict.ContainsKey(pjkey))
                        {
                            pjreportdict[pjkey].IssueTrend = ViewBag.issuetrendscript.Replace("issuetrendchart", pjkey + "issuetrendchart");
                            ViewBag.issuetrendscript = null;
                        }
                        else
                        {
                            var reportitem = new PJReportItem();
                            reportitem.IssueTrend = ViewBag.issuetrendscript.Replace("issuetrendchart", pjkey + "issuetrendchart");
                            ViewBag.issuetrendscript = null;
                            pjreportdict.Add(pjkey, reportitem);
                        }
                    }

                    if (string.Compare(reptype, PJReportType.FailureTrend) == 0)
                    {
                        var chartlist  = PJFailureTrend(pjkey);
                        for (var idx=0;idx < chartlist.Count;idx++)
                        {
                            chartlist[idx] = chartlist[idx].Replace("#ElementID#", pjkey +"failuretrend"+idx);
                        }

                        if (pjreportdict.ContainsKey(pjkey))
                        {
                            pjreportdict[pjkey].FailureTrends = chartlist;
                        }
                        else
                        {
                            var reportitem = new PJReportItem();
                            reportitem.FailureTrends = chartlist;
                            pjreportdict.Add(pjkey, reportitem);
                        }
                    }
                }
            }

            ViewBag.ReportDict = pjreportdict;

            return View();
        }
    }
}