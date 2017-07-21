using Prometheus.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Prometheus.Models
{
    public class MESUtility
    {
        private static bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        private static string RMSpectialCh(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                {
                    sb.Append(c);
                }
            }
            return sb.ToString().ToUpper();
        }

        private static string PNCondition(List<ProjectPn> pns)
        {
            var cond = "";
            foreach (var pn in pns)
            {
                if (!IsDigitsOnly(pn.Pn.Trim()))
                {
                    if (string.IsNullOrEmpty(cond))
                    {
                        cond = " c.Description like '%" + pn.Pn.Trim() + "%' ";
                    }
                    else
                    {
                        cond = cond + " or c.Description like '%" + pn.Pn.Trim() + "%' ";
                    }
                }
            }

            if (string.IsNullOrEmpty(cond))
            {
                return string.Empty;
            }
            else
            {
                var ret = "select DISTINCT p.ProductName from insite.Product c (nolock) left join insite.ProductBase p on c.ProductBaseId = p.ProductBaseId where " + cond;
                return ret;
            }
        }


        private static Dictionary<string, string> RetrieveSqlFromProjectModel(ProjectViewModels projectmodel)
        {
            var tables = new Dictionary<string,string>();

            foreach (var s in projectmodel.StationList)
            {
                foreach (var m in projectmodel.TabList)
                {
                    if (string.Compare(s.Station.ToUpper(), m.Station.ToUpper()) == 0)
                    {
                        if (!tables.ContainsKey(s.Station)) { 
                        tables.Add(s.Station,m.TableName);
                        }
                        break;
                    }
                }
            }

            string pncond = PNCondition(projectmodel.PNList);

            var joinstr = " LEFT JOIN Insite.Container b WITH (NOLOCK) ON b.containername = a.ModuleSerialNum LEFT JOIN Insite.MfgOrder d WITH(NOLOCK) ON d.MfgOrderId = b.MfgOrderId ";
            var sql = "select a.dc_<DCTABLE>HistoryId,a.ModuleSerialNum, a.WhichTest, a.ModuleType, a.ErrAbbr, a.TestTimeStamp, a.TestStation,a.assemblypartnum ,d.MfgOrderName from "
                + " insite.dc_<DCTABLE> a (nolock) "+ joinstr + " where assemblypartnum in  (<PNCOND>)  <TIMECOND>  order by  moduleserialnum,testtimestamp DESC";

            var ret = new Dictionary<string, string>();
            foreach (var tb in tables)
            {
                if (string.IsNullOrEmpty(pncond))
                {
                    ret.Add(tb.Key,string.Empty);
                }
                else
                {
                     ret.Add(tb.Key,sql.Replace("<DCTABLE>",tb.Value).Replace("<PNCOND>", pncond));
                }
                
            }
            return ret;
        }

        private static void CreateFA(ProjectTestData item,string firstengineer,Controller ctrl)
        {
            var vm = new IssueViewModels();
            vm.ProjectKey = item.ProjectKey;
            vm.IssueKey = item.DataID;
            vm.IssueType = ISSUETP.Bug;
            vm.Summary = "Module " + item.ModuleSerialNum + " failed for "+item.ErrAbbr + " @ "+item.WhichTest;
            vm.Priority = ISSUEPR.Major;
            vm.DueDate = DateTime.Now.AddDays(7);
            vm.ReportDate = item.TestTimeStamp;
            vm.Assignee = firstengineer;
            vm.Reporter = "System";
            vm.Resolution = Resolute.Pending;
            vm.ResolvedDate = DateTime.Parse("1982-05-06 01:01:01");
            vm.Description = "Module " + item.ModuleSerialNum + " failed for " + item.ErrAbbr + " @ " + item.WhichTest +" on tester "+item.TestStation + " "+item.TestTimeStamp.ToString("yyyy-MM-dd hh:mm:ss");
            vm.CommentType = COMMENTTYPE.Description;
            vm.ModuleSN = item.ModuleSerialNum;
            vm.ErrAbbr = item.ErrAbbr;
            vm.DataID = item.DataID;
            //ProjectEvent.CreateIssueEvent(vm.ProjectKey, "System", vm.Assignee, vm.Summary, vm.IssueKey);
            vm.StoreIssue();

            var traceviewlist = ExternalDataCollector.LoadTraceView2Local(item.TestStation, item.ModuleSerialNum, item.WhichTest, item.TestTimeStamp.ToString(), ctrl);
            foreach (var trace in traceviewlist)
            {
                var traces = trace.Split(new string[] { "userfiles\\" }, StringSplitOptions.RemoveEmptyEntries);
                if (traces.Length == 2)
                {
                    var url = "/userfiles/" + traces[1].Replace("\\", "/");
                    IssueViewModels.StoreIssueAttachment(vm.IssueKey, url);
                }
            }//end foreach

        }

        private static List<TraceViewData> RetrieveTraceViewData(ProjectCriticalErrorVM pjerror, ProjectTestData pjdata, Controller ctrl,List<string> traceviewfilelist)
        {
            var ret = new List<TraceViewData>();

            var traceviewlist = ExternalDataCollector.LoadTraceView2Local(pjdata.TestStation, pjdata.ModuleSerialNum, pjdata.WhichTest, pjdata.TestTimeStamp.ToString(), ctrl);
            traceviewfilelist.AddRange(traceviewlist);
            foreach (var filename in traceviewlist)
            {
                var tempret = ExternalDataCollector.RetrieveTestDataFromTraceView(filename, pjerror.TestCaseName, pjerror.MatchCond);
                if (tempret.Count > 0)
                {
                    ret.AddRange(tempret);
                }
            }
            return ret;
        }

        private static CleanDataWithStdDev GetCleanDataWithStdDev(List<double> rawdata,double sigma)
        {
            var ret = new CleanDataWithStdDev();

            rawdata.Sort();
            var mean = NormalDistributeAlg.Mean(rawdata);
            var stddev = NormalDistributeAlg.StandardDeviation(rawdata, mean);
            ret.Mean = mean;
            ret.StdDev = stddev;

            var startvalue = mean - sigma * stddev;
            var endvalue = mean + sigma * stddev;
            foreach (var item in rawdata)
            {
                if (item >= startvalue && item <= endvalue)
                {
                    ret.FiltedList.Add(item);
                }
                else
                {
                    ret.OutlierList.Add(item);
                }
            }
            return ret;
        }

        private static bool StdDevAlgorithm(List<TraceViewData> tempfiltereddata,double sigma)
        {
            var lowtemplist = new List<double>();
            var hightemplist = new List<double>();
            var normaltemplist = new List<double>();
            foreach (var item in tempfiltereddata)
            {
                if (item.Temp > 45)
                {
                    hightemplist.Add(item.Value);
                }
                else if (item.Temp < 15)
                {
                    lowtemplist.Add(item.Value);
                }
                else
                {
                    normaltemplist.Add(item.Value);
                }
            }

            try
            {
                if (lowtemplist.Count > 2)
                {
                    
                    var ret = GetCleanDataWithStdDev(lowtemplist, sigma);
                    if (ret.OutlierList.Count > 0)
                        return true;
                }
                if (hightemplist.Count > 2)
                {
                    var ret = GetCleanDataWithStdDev(hightemplist, sigma);
                    if (ret.OutlierList.Count > 0)
                        return true;
                }
                if (normaltemplist.Count > 2)
                {
                    var ret = GetCleanDataWithStdDev(normaltemplist, sigma);
                    if (ret.OutlierList.Count > 0)
                        return true;
                }
            }
            catch (Exception ex) { }

            return false;
        }

        private static bool DeltaAlgorithm(List<TraceViewData> tempfiltereddata, double delta)
        {
            var lowtemplist = new List<double>();
            var hightemplist = new List<double>();
            var normaltemplist = new List<double>();
            foreach (var item in tempfiltereddata)
            {
                if (item.Temp > 45)
                {
                    hightemplist.Add(item.Value);
                }
                else if (item.Temp < 15)
                {
                    lowtemplist.Add(item.Value);
                }
                else
                {
                    normaltemplist.Add(item.Value);
                }
            }

            try
            {
                if (lowtemplist.Count > 2)
                {
                    lowtemplist.Sort();
                    if ((lowtemplist[lowtemplist.Count - 1] - lowtemplist[0]) > delta)
                        return true;
                }
                if (hightemplist.Count > 2)
                {
                    hightemplist.Sort();
                    if ((hightemplist[hightemplist.Count - 1] - hightemplist[0]) > delta)
                        return true;
                }
                if (normaltemplist.Count > 2)
                {
                    normaltemplist.Sort();
                    if ((normaltemplist[normaltemplist.Count - 1] - normaltemplist[0]) > delta)
                        return true;
                }
            }
            catch (Exception ex) { }

            return false;
        }

        private static bool UniformityAlgorithm(List<TraceViewData> tempfiltereddata, double rate)
        {
            var lowtemplist = new List<double>();
            var hightemplist = new List<double>();
            var normaltemplist = new List<double>();
            foreach (var item in tempfiltereddata)
            {
                if (item.Temp > 45)
                {
                    hightemplist.Add(item.Value);
                }
                else if (item.Temp < 15)
                {
                    lowtemplist.Add(item.Value);
                }
                else
                {
                    normaltemplist.Add(item.Value);
                }
            }

            try
            {
                if (lowtemplist.Count > 2)
                {
                    lowtemplist.Sort();
                    var mean = NormalDistributeAlg.Mean(lowtemplist);
                    var uniformity = (lowtemplist[lowtemplist.Count - 1] - lowtemplist[0]) / mean;
                    if (uniformity > rate)
                        return true;
                }
                if (hightemplist.Count > 2)
                {
                    hightemplist.Sort();
                    var mean = NormalDistributeAlg.Mean(hightemplist);
                    var uniformity = (hightemplist[hightemplist.Count - 1] - hightemplist[0]) / mean;
                    if (uniformity > rate)
                        return true;
                }
                if (normaltemplist.Count > 2)
                {
                    normaltemplist.Sort();
                    var mean = NormalDistributeAlg.Mean(normaltemplist);
                    var uniformity = (normaltemplist[normaltemplist.Count - 1] - normaltemplist[0]) / mean;
                    if (uniformity > rate)
                        return true;
                }
            }
            catch (Exception ex) { }

            return false;
        }

        private static List<TraceViewData> FilterTraceViewDataWithCHTEMP(List<TraceViewData> traceviewdata, ProjectCriticalErrorVM pjerror)
        {
            var chfiltereddata = new List<TraceViewData>();
            var tempfiltereddata = new List<TraceViewData>();

            if (!string.IsNullOrEmpty(pjerror.Channel))
            {
                foreach (var item in traceviewdata)
                {
                    if (item.CH == Convert.ToInt32(pjerror.Channel))
                    {
                        chfiltereddata.Add(item);
                    }
                }//end foreach
            }
            else
            {
                chfiltereddata.AddRange(traceviewdata);
            }

            if (!string.IsNullOrEmpty(pjerror.Temperature))
            {
                foreach (var item in chfiltereddata)
                {
                    if (pjerror.Temperature.ToUpper().Contains(TestTemperatureType.High.ToUpper()))
                    {
                        if (item.Temp > 45)
                        {
                            tempfiltereddata.Add(item);
                        }
                    }
                    else if (pjerror.Temperature.ToUpper().Contains(TestTemperatureType.Low.ToUpper()))
                    {
                        if (item.Temp < 15)
                        {
                            tempfiltereddata.Add(item);
                        }
                    }
                    else
                    {
                        if (item.Temp > 15 && item.Temp < 45)
                        {
                            tempfiltereddata.Add(item);
                        }
                    }
                }//end foreach
            }
            else
            {
                tempfiltereddata.AddRange(chfiltereddata);
            }

            return tempfiltereddata;
        }

        private static bool CheckPJCriticalRule(List<TraceViewData> traceviewdata, ProjectCriticalErrorVM pjerror,List<TraceViewData> filtereddata)
        {

            var tempfiltereddata = FilterTraceViewDataWithCHTEMP(traceviewdata, pjerror);
            if (tempfiltereddata.Count == 0)
                return false;
            filtereddata.AddRange(tempfiltereddata);

            if (pjerror.WithLimit == 1)
            {
                foreach (var item in tempfiltereddata)
                {
                    if (item.Value > pjerror.LowLimit && item.Value < pjerror.HighLimit)
                    {
                        return true;
                    }
                }
            }

            if (pjerror.WithAlgorithm == 1)
            {
                if (string.Compare(pjerror.Algorithm, PJCriticalAlgorithm.MAXDELTA, true) == 0)
                {
                    try
                    {
                        var delta = Convert.ToDouble(pjerror.AlgorithmParam.Trim());
                        if (DeltaAlgorithm(tempfiltereddata, delta))
                        {
                            return true;
                        }
                    }
                    catch (Exception ex) { }
                }

                if (string.Compare(pjerror.Algorithm, PJCriticalAlgorithm.STDDEV, true) == 0)
                {
                    try
                    {
                        var sigma = Convert.ToDouble(pjerror.AlgorithmParam.Trim());
                        if (StdDevAlgorithm(tempfiltereddata, sigma))
                        {
                            return true;
                        }
                    }
                    catch (Exception ex) { }
                }

                if (string.Compare(pjerror.Algorithm, PJCriticalAlgorithm.UNIFORMITY, true) == 0)
                {
                    try
                    {
                        var rate = Convert.ToDouble(pjerror.AlgorithmParam.Trim());
                        if (UniformityAlgorithm(tempfiltereddata, rate))
                        {
                            return true;
                        }
                    }
                    catch (Exception ex) { }
                }
            }

            return false;
        }


        private static void SendTaskEvent(IssueViewModels vm,string comment,Controller ctrl)
        {
            var routevalue = new RouteValueDictionary();
            routevalue.Add("issuekey", vm.IssueKey);
            //send validate email
            string scheme = ctrl.Url.RequestContext.HttpContext.Request.Url.Scheme;
            string validatestr = ctrl.Url.Action("UpdateIssue", "Issue", routevalue, scheme);

            var netcomputername = "";
            try { netcomputername = System.Net.Dns.GetHostName(); }
            catch (Exception ex) { }
            validatestr = validatestr.Replace("//localhost", "//" + netcomputername);

            var content = vm.Summary + " is created :\r\n " + validatestr;
            content = content +"\r\n\r\n"+ comment;

            var toaddrs = new List<string>();
            toaddrs.Add(vm.Assignee);
            if (vm.RelativePeopleList.Count > 0)
            {
                toaddrs.AddRange(vm.RelativePeopleList);
            }

            var reporter = vm.Reporter.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries)[0].Replace(".", " ");
            EmailUtility.SendEmail(ctrl, "WUXI NPI System_" + reporter, toaddrs, content);
            new System.Threading.ManualResetEvent(false).WaitOne(30);
        }

        private static string Create2ndCheckTask(ProjectTestData pjdata, ProjectCriticalErrorVM item, Controller ctrl,List<string> traceviewfilelist)
        {
            try
            {
                var asignee = item.Creater;
                var now = DateTime.Now;
                var vm = new IssueViewModels();
                vm.ProjectKey = item.ProjectKey;
                vm.IssueKey = IssueViewModels.GetUniqKey();
                vm.IssueType = ISSUETP.Task;
                vm.Summary = "[2nd Match] " + pjdata.ModuleSerialNum + " failed for " + pjdata.ErrAbbr + " @ " + pjdata.WhichTest;
                vm.ModuleSN = pjdata.ModuleSerialNum;
                vm.ErrAbbr = pjdata.ErrAbbr;
                vm.Priority = ISSUEPR.Major;
                vm.DueDate = now.AddDays(7);
                vm.ReportDate = DateTime.Now;
                vm.Assignee = asignee;
                vm.Reporter = asignee;
                vm.Creator = asignee;
                vm.Resolution = Resolute.Pending;
                vm.ResolvedDate = DateTime.Parse("1982-05-06 01:01:01");
                vm.RelativePeoples = item.Appv_4;
                vm.StoreIssue();

                var comment1 = new IssueComments();
                comment1.Comment = "<p>"+ pjdata.ModuleSerialNum + " failed for " + pjdata.ErrAbbr + " @ " + pjdata.WhichTest + "</p>";
                comment1.Comment = comment1.Comment+"<p> match rule: test case -- " +item.TestCaseName+" datefield -- "+item.MatchCond+ "</p>";
                if (item.WithLimit == 1)
                {
                    comment1.Comment = comment1.Comment + "<p> with limit: low limit -- " + item.LowLimit.ToString() + " high limit -- " + item.HighLimit.ToString() + "</p>";
                }
                if (item.WithAlgorithm == 1)
                {
                    comment1.Comment = comment1.Comment + "<p> with algorithm: argorithm -- " + item.Algorithm + " param -- " + item.AlgorithmParam + "</p>";
                }
                IssueViewModels.StoreIssueComment(vm.IssueKey, comment1.dbComment, vm.Assignee, COMMENTTYPE.Description);

                foreach (var trace in traceviewfilelist)
                {
                    var traces = trace.Split(new string[] { "userfiles\\" }, StringSplitOptions.RemoveEmptyEntries);
                    if (traces.Length == 2)
                    {
                        var url = "/userfiles/" + traces[1].Replace("\\", "/");
                        IssueViewModels.StoreIssueAttachment(vm.IssueKey, url);
                    }
                }//end foreach

                SendTaskEvent(vm, comment1.Comment, ctrl);

                return vm.IssueKey;
            }
            catch (Exception ex) { }
            return null;
        }

        private static bool CheckPJCriticalError(ProjectTestData pjdata, List<ProjectCriticalErrorVM> pjcriticalerrorlist, Controller ctrl)
        {
            var ret = false;

            foreach (var item in pjcriticalerrorlist)
            {
                if (string.Compare(item.ErrorCode, pjdata.ErrAbbr, true) == 0)
                {
                    if (item.Appv_1 > 0)//FA SWITCH CLOSED
                    {
                        ret = true;
                    }

                    var filtereddata = new List<TraceViewData>();
                    var traceviewfilelist = new List<string>();
                    var traceviewdata = RetrieveTraceViewData(item, pjdata, ctrl, traceviewfilelist);
                    if (traceviewdata.Count == 0)
                        continue;
                    if (!CheckPJCriticalRule(traceviewdata, item,filtereddata))
                        continue;
                    //match rule

                    //check previous match date
                    if ((DateTime.Now - item.Appv_5).Days < 1)
                    {
                        if (!string.IsNullOrEmpty(item.Appv_3))
                        {
                            var commentlist = IssueViewModels.RetrieveIssueByIssueKey(item.Appv_3, ctrl).CommentList;
                            if (commentlist.Count > 0)
                            {
                                var fcomment = commentlist[0];
                                if (fcomment.Comment.Contains("</div>"))
                                {
                                    
                                    fcomment.Comment = fcomment.Comment + "<div class=\"col-lg-2\">" + pjdata.ModuleSerialNum + "</div>";
                                }
                                else
                                {
                                    fcomment.Comment = fcomment.Comment + "<p>&nbsp;</p>";
                                    fcomment.Comment = fcomment.Comment + "<div class=\"col-lg-2\">" + pjdata.ModuleSerialNum + "</div>";
                                }

                                IssueViewModels.UpdateSPComment(item.Appv_3, fcomment.CommentType, fcomment.CommentDate.ToString(), fcomment.dbComment);
                            }//end if
                        }//end if
                    }
                    else
                    {
                        item.Appv_3 = Create2ndCheckTask(pjdata, item, ctrl, traceviewfilelist);
                        if (!string.IsNullOrEmpty(item.Appv_3))
                        {
                            item.Appv_5 = DateTime.Now;
                            item.UpdateMatchDateandTaskKey();
                        }
                    }

                    return true;
                }//end if
            }//end foreach

            return ret;
        }


        private static void CreateSystemIssues(List<ProjectTestData> failurelist, Controller ctrl)
        {
            if (failurelist.Count > 0)
            {
                var pj = ProjectViewModels.RetrieveOneProject(failurelist[0].ProjectKey);
                var firstengineer = "";
                foreach (var m in pj.MemberList)
                {
                    if (string.Compare(m.Role,ProjectViewModels.ENGROLE) == 0)
                    {
                        firstengineer = m.Name;
                        break;
                    }
                }
                var pjcriticalerrorlist = ProjectCriticalErrorVM.RetrievePJCriticalError(failurelist[0].ProjectKey, null);

                foreach (var item in failurelist)
                {
                    if (!CheckPJCriticalError(item,pjcriticalerrorlist,ctrl))
                    {
                        CreateFA(item, firstengineer,ctrl);
                    }
                }
            }
        }

        public static void StartProjectBonding(ProjectViewModels vm)
        {
            try
            {
                if (ProjectTestData.UpdatePJLockUsing(vm.ProjectKey))
                    return;


                if (vm.StationList.Count > 0
                    && vm.TabList.Count > 0
                    && vm.PNList.Count > 0)
                {

                    var failurelist = new List<ProjectTestData>();
                    

                    bool bondinged = false;
                    if (ProjectTestData.RetrieveLatestTimeOfLocalProject(vm.ProjectKey) != null)
                    {
                        bondinged = true;
                    }

                    var bondingeddatadict = new Dictionary<string, bool>();
                    if (bondinged)
                    {
                        bondingeddatadict = ProjectTestData.RetrieveAllDataID(vm.ProjectKey);
                    }
                    
                    var sqls = RetrieveSqlFromProjectModel(vm);
                    foreach (var s in sqls)
                    {
                        if (string.IsNullOrEmpty(s.Value))
                        {
                            continue;
                        }

                        var sndict = new Dictionary<string, bool>();
                        var sql = s.Value.Replace("<TIMECOND>", "and TestTimeStamp > '" + vm.StartDate.ToString("yyyy-MM-dd hh:mm:ss") + "' and TestTimeStamp < '" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "'");
                        var dbret = DBUtility.ExeMESSqlWithRes(sql);
                        foreach (var item in dbret)
                        {
                            try
                            {

                                var tempdata = new ProjectTestData(vm.ProjectKey, Convert.ToString(item[0]), Convert.ToString(item[1])
                                        ,s.Key, Convert.ToString(item[3]), Convert.ToString(item[4])
                                        , Convert.ToString(item[5]), Convert.ToString(item[6]), Convert.ToString(item[7]));
                                tempdata.JO = Convert.ToString(item[8]);

                                if (!bondinged)
                                {
                                    tempdata.StoreProjectTestData();

                                    if (!sndict.ContainsKey(tempdata.ModuleSerialNum))
                                    {
                                        sndict.Add(tempdata.ModuleSerialNum, true);
                                        if (string.Compare(tempdata.ErrAbbr, "PASS", true) != 0)
                                        {
                                            var ekey = ProjectErrorViewModels.GetUniqKey();
                                            var pjerror = new ProjectErrorViewModels(vm.ProjectKey, ekey, tempdata.ErrAbbr, "", 1);
                                            pjerror.Reporter = "System";
                                            pjerror.Description = "";
                                            pjerror.AddandUpdateProjectError();

                                            failurelist.Add(tempdata);
                                        }
                                    }
                                }
                                else
                                {
                                    if (!bondingeddatadict.ContainsKey(tempdata.DataID))
                                    {
                                        bondingeddatadict.Add(tempdata.DataID,true);
                                        tempdata.StoreProjectTestData();

                                    if (!sndict.ContainsKey(tempdata.ModuleSerialNum))
                                        {
                                            sndict.Add(tempdata.ModuleSerialNum, true);
                                            if (string.Compare(tempdata.ErrAbbr, "PASS", true) != 0)
                                            {
                                                var ekey = ProjectErrorViewModels.GetUniqKey();
                                                var pjerror = new ProjectErrorViewModels(vm.ProjectKey, ekey, tempdata.ErrAbbr, "", 1);
                                                pjerror.Reporter = "System";
                                                pjerror.Description = "";
                                                pjerror.AddandUpdateProjectError();

                                                failurelist.Add(tempdata);
                                            }
                                        }
                                }
                                }
                            }
                            catch (Exception ex)
                            { }
                        }
                    }

                    //if (vm.FinishRating < 90)
                    //{
                    //    CreateSystemIssues(failurelist);
                    //}
                }

                ProjectTestData.ResetUpdatePJLock(vm.ProjectKey);
            }
            catch (Exception ex)
            {
                ProjectTestData.ResetUpdatePJLock(vm.ProjectKey);
            }

            
        }


        public static void UpdateProjectData(ProjectViewModels vm,string starttime,string endtime, Controller ctrl)
        {
            try
            {

                if (vm.StationList.Count > 0
                && vm.TabList.Count > 0
                && vm.PNList.Count > 0)
            {

                var failurelist = new List<ProjectTestData>();
                var passlist = new List<ProjectTestData>();

                bool bondinged = false;
                if (ProjectTestData.RetrieveLatestTimeOfLocalProject(vm.ProjectKey) != null)
                {
                    bondinged = true;
                }

                var bondingeddatadict = new Dictionary<string, bool>();
                if (bondinged)
                {
                    bondingeddatadict = ProjectTestData.RetrieveAllDataID(vm.ProjectKey);
                }

                var sqls = RetrieveSqlFromProjectModel(vm);
                foreach (var s in sqls)
                {
                    
                    if (string.IsNullOrEmpty(s.Value))
                    {
                        continue;
                    }

                    var realstarttime = ProjectTestData.RetrieveLatestTimeOfLocalProjectWithTestName(vm.ProjectKey, s.Key);
                    if (!string.IsNullOrEmpty(realstarttime))
                    {
                        starttime = realstarttime;
                    }

                    var sndict = new Dictionary<string, bool>();
                    var sql = s.Value.Replace("<TIMECOND>", "and TestTimeStamp > '" + starttime + "' and TestTimeStamp < '"+ endtime + "'");
                    var dbret = DBUtility.ExeMESSqlWithRes(sql);
                    foreach (var item in dbret)
                    {
                        try
                        {

                            var tempdata = new ProjectTestData(vm.ProjectKey, Convert.ToString(item[0]), Convert.ToString(item[1])
                                    ,s.Key, Convert.ToString(item[3]), Convert.ToString(item[4])
                                    , Convert.ToString(item[5]), Convert.ToString(item[6]), Convert.ToString(item[7]));
                                tempdata.JO = Convert.ToString(item[8]);

                            if (!bondinged)
                            {
                                tempdata.StoreProjectTestData();

                                if (!sndict.ContainsKey(tempdata.ModuleSerialNum))
                                {
                                    sndict.Add(tempdata.ModuleSerialNum, true);
                                    if (string.Compare(tempdata.ErrAbbr, "PASS", true) != 0)
                                    {
                                        var ekey = ProjectErrorViewModels.GetUniqKey();
                                        var pjerror = new ProjectErrorViewModels(vm.ProjectKey, ekey, tempdata.ErrAbbr, "", 1);
                                        pjerror.Reporter = "System";
                                        pjerror.Description = "";
                                        pjerror.AddandUpdateProjectError();

                                        failurelist.Add(tempdata);
                                    }
                                    else
                                    {
                                        passlist.Add(tempdata);
                                    }
                                }
                            }
                            else
                            {
                                if (!bondingeddatadict.ContainsKey(tempdata.DataID))
                                {
                                    bondingeddatadict.Add(tempdata.DataID,true);

                                    tempdata.StoreProjectTestData();

                                    if (!sndict.ContainsKey(tempdata.ModuleSerialNum))
                                    {
                                        sndict.Add(tempdata.ModuleSerialNum, true);
                                        if (string.Compare(tempdata.ErrAbbr, "PASS", true) != 0)
                                        {
                                            var ekey = ProjectErrorViewModels.GetUniqKey();
                                            var pjerror = new ProjectErrorViewModels(vm.ProjectKey, ekey, tempdata.ErrAbbr, "", 1);
                                            pjerror.Reporter = "System";
                                            pjerror.Description = "";
                                            pjerror.AddandUpdateProjectError();

                                            failurelist.Add(tempdata);
                                        }
                                        else
                                        {
                                            passlist.Add(tempdata);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        { }
                    }
                }

                if (vm.FinishRating < 90 && DateTime.Parse(starttime) != vm.StartDate)
                {
                        //use latest failure cover previous failure
                        foreach (var item in failurelist)
                        {
                            IssueViewModels.CloseIssueAutomaticllyWithFailedSN(item.ProjectKey, item.ModuleSerialNum, item.WhichTest, item.TestStation, item.TestTimeStamp.ToString("yyyy-MM-dd hh:mm:ss"));
                        }

                        CreateSystemIssues(failurelist,ctrl);
                }

                if (vm.FinishRating < 90 && DateTime.Parse(starttime) != vm.StartDate)
                {
                        //use pass sn cover previous failure
                        foreach (var item in passlist)
                    {
                        IssueViewModels.CloseIssueAutomaticlly(item.ProjectKey,item.ModuleSerialNum, item.WhichTest, item.TestStation, item.TestTimeStamp.ToString("yyyy-MM-dd hh:mm:ss"));
                    }
                }
                
            }


            }
            catch (Exception ex)
            {
                
            }

        }

        public static List<string> RetrieveAllPN(List<string> pndeslist)
        {
            var cond = "";
            foreach (var pn in pndeslist)
            {
                if (!IsDigitsOnly(pn.Trim()))
                {
                    if (string.IsNullOrEmpty(cond))
                    {
                        cond = " c.Description like '%" + pn.Trim() + "%' ";
                    }
                    else
                    {
                        cond = cond + " or c.Description like '%" + pn.Trim() + "%' ";
                    }
                }
            }

            if (string.IsNullOrEmpty(cond))
            {
                return new List<string>();
            }
            else
            {
                var ret = new List<string>();
                var sql = "select DISTINCT p.ProductName from insite.Product c (nolock) left join insite.ProductBase p on c.ProductBaseId = p.ProductBaseId where " + cond;
                var dbret = DBUtility.ExeMESSqlWithRes(sql);
                foreach (var item in dbret)
                {
                    ret.Add(Convert.ToString(item[0]));
                }
                return ret;
            }
        }


    }
}