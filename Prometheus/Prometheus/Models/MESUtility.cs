using Prometheus.Controllers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using MathNet.Numerics.Statistics;

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
                //var ret = "select DISTINCT p.ProductName from insite.Product c (nolock) left join insite.ProductBase p on c.ProductBaseId = p.ProductBaseId where " + cond;
                //return ret;
                var pnkey = "name_" + cond.Replace("c.Description like", "").Replace("or", ",").Replace("'", "").Replace("%", "").Replace(" ", "");
                var pnlist = PNDescCache.RetrievePNs(pnkey);
                if (pnlist.Count > 0)
                {
                    var ret = "'";
                    foreach (var item in pnlist)
                    {
                        ret = ret + item + "','";
                    }
                    ret = ret.Substring(0, ret.Length - 2);
                    return ret;
                }

                var sql = "select DISTINCT p.ProductName from insite.Product c (nolock) left join insite.ProductBase p on c.ProductBaseId = p.ProductBaseId where " + cond;
                var dbret = DBUtility.ExeMESSqlWithRes(sql);
                if (dbret.Count == 0)
                {
                    return string.Empty;
                }

                var ret1 = "'";
                foreach (var line in dbret)
                {
                    var pn = Convert.ToString(line[0]);
                    ret1 = ret1 + pn + "','";
                    PNDescCache.StorePN(pnkey, pn);
                }
                ret1 = ret1.Substring(0, ret1.Length - 2);
                return ret1;
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
            //var joinstr = " LEFT JOIN Insite.SUM_Popular_Contnaier d WITH (NOLOCK) ON d.ContainerName = a.ModuleSerialNum ";

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

        private static string RetrieveOSASql(ProjectViewModels projectmodel)
        {
            string pncond = PNCondition(projectmodel.PNList);
            if (string.IsNullOrEmpty(pncond))
                return string.Empty;

            var joinstr = " LEFT JOIN Insite.Container b WITH (NOLOCK) ON b.containername = a.ModuleSerialNum LEFT JOIN Insite.MfgOrder d WITH(NOLOCK) ON d.MfgOrderId = b.MfgOrderId ";
            //var joinstr = " LEFT JOIN Insite.SUM_Popular_Contnaier d WITH (NOLOCK) ON d.ContainerName = a.ModuleSerialNum ";

            var sql = "select a.dc_OSAMultiValueTestHistoryId,a.ModuleSerialNum, a.WhichTest, a.ModuleType,a.TestResult, a.TestTimeStamp, a.TestStation,a.ModulePartNum ,d.MfgOrderName from "
                + " insite.dc_OSAMultiValueTest a (nolock) " + joinstr + " where a.ModulePartNum in  (<PNCOND>)  <TIMECOND>  order by  moduleserialnum,testtimestamp DESC";
            return sql.Replace("<PNCOND>", pncond);
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
            vm.Description = "Module " + item.ModuleSerialNum + " failed for " + item.ErrAbbr + " @ " + item.WhichTest +" on tester "+item.TestStation + " "+item.TestTimeStamp.ToString("yyyy-MM-dd HH:mm:ss");
            vm.CommentType = COMMENTTYPE.Description;
            vm.ModuleSN = item.ModuleSerialNum;
            vm.ErrAbbr = item.ErrAbbr;
            vm.DataID = item.DataID;
            //ProjectEvent.CreateIssueEvent(vm.ProjectKey, "System", vm.Assignee, vm.Summary, vm.IssueKey);
            vm.StoreIssue();

            IssueTypeVM.SaveIssueType(vm.IssueKey, ISSUESUBTYPE.Bug.ToString());

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
                var tempret = ExternalDataCollector.RetrieveTestDataFromTraceView( pjerror, filename, pjerror.TestCaseName.Trim(), pjerror.MatchCond.Trim());
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
                    hightemplist.Add(item.dValue);
                }
                else if (item.Temp < 15)
                {
                    lowtemplist.Add(item.dValue);
                }
                else
                {
                    normaltemplist.Add(item.dValue);
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
                    hightemplist.Add(item.dValue);
                }
                else if (item.Temp < 15)
                {
                    lowtemplist.Add(item.dValue);
                }
                else
                {
                    normaltemplist.Add(item.dValue);
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
                    hightemplist.Add(item.dValue);
                }
                else if (item.Temp < 15)
                {
                    lowtemplist.Add(item.dValue);
                }
                else
                {
                    normaltemplist.Add(item.dValue);
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
                    if (item.dValue > pjerror.LowLimit && item.dValue < pjerror.HighLimit)
                    {
                        return true;
                    }
                }
            }

            if (pjerror.WithWildMatch == 1)
            {
                foreach (var item in tempfiltereddata)
                {
                    if (pjerror.WildMatchParam.Contains("##"))
                    {
                        try
                        {
                            var lowhigh = pjerror.WildMatchParam.Split(new string[] { "##" }, StringSplitOptions.RemoveEmptyEntries);
                            var low = 0.0;
                            var high = 0.0;
                            if (lowhigh[0].ToUpper().Contains("0X"))
                            {
                                var parsed = long.Parse(lowhigh[0].ToUpper().Replace("0X", ""), NumberStyles.AllowHexSpecifier);
                                low = Convert.ToDouble(parsed);
                            }
                            else
                            {
                                low = Convert.ToDouble(lowhigh[0]);
                            }
                            if (lowhigh[1].ToUpper().Contains("0X"))
                            {
                                var parsed = long.Parse(lowhigh[1].ToUpper().Replace("0X", ""), NumberStyles.AllowHexSpecifier);
                                high = Convert.ToDouble(parsed);
                            }
                            else
                            {
                                high = Convert.ToDouble(lowhigh[1]);
                            }

                            if (item.dValue > low && item.dValue < high)
                            {
                                return true;
                            }
                        }
                        catch(Exception E) { }
                    }
                    else
                    {
                        if (string.Compare(item.Value.ToUpper().Trim(),pjerror.WildMatchParam.ToUpper().Trim()) != 0)
                        {
                            return true;
                        }
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

            var netcomputername = EmailUtility.RetrieveCurrentMachineName();
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
            EmailUtility.SendEmail(ctrl, "Project " + vm.ProjectKey + " Critical Error Alarm - WUXI Engineering System", toaddrs, content);
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
                vm.IssueKey = pjdata.DataID;
                vm.IssueType = ISSUETP.Bug;
                vm.Summary = CRITICALERRORTYPE.SECONDMATCH + " " + pjdata.ModuleSerialNum + " failed for " + pjdata.ErrAbbr + " @ " + pjdata.WhichTest;
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

                IssueTypeVM.SaveIssueType(vm.IssueKey, ISSUESUBTYPE.CrititalFailureTask.ToString(), ISSUESUBTYPE.CrititalFailureTask.ToString());

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

                if (item.WithWildMatch == 1)
                {
                    comment1.Comment = comment1.Comment + "<p> with wild match, param -- " + item.WildMatchParam + "</p>";
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

        private static void Create2ndCheckTask2(ProjectTestData pjdata,string asignee, KeyValuePair<string,string> rule, Controller ctrl, List<string> traceviewfilelist)
        {
            try
            {
                var now = DateTime.Now;
                var vm = new IssueViewModels();
                vm.ProjectKey = pjdata.ProjectKey;
                vm.IssueKey = pjdata.DataID;
                vm.IssueType = ISSUETP.Bug;
                vm.Summary = CRITICALERRORTYPE.SECONDMATCH + " " + pjdata.ModuleSerialNum + " failed for " + pjdata.ErrAbbr + " @ " + pjdata.WhichTest;
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
                vm.RelativePeoples = "";
                vm.StoreIssue();

                IssueTypeVM.SaveIssueType(vm.IssueKey, ISSUESUBTYPE.CrititalFailureTask.ToString(),ISSUESUBTYPE.CrititalFailureTask.ToString());

                var comment1 = new IssueComments();
                comment1.Comment = "<p>" + pjdata.ModuleSerialNum + " failed for " + pjdata.ErrAbbr + " @ " + pjdata.WhichTest + "</p>";
                comment1.Comment = comment1.Comment + "<p> match default rule: test -- " +rule.Key + " failure code -- " + rule.Value + "</p>";

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
            }
            catch (Exception ex) { }
        }

        private static bool CheckPJCriticalError(ProjectTestData pjdata, List<ProjectCriticalErrorVM> pjcriticalerrorlist, Controller ctrl)
        {
            var ret = false;

            foreach (var item in pjcriticalerrorlist)
            {
                if (string.Compare(item.ErrorCode, pjdata.ErrAbbr, true) == 0 
                    || string.Compare(item.ErrorCode,"ALL",true) == 0)
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

        private static bool CheckDefaultCriticalError(ProjectTestData pjdata,string firstengineer, List<KeyValuePair<string, string>> defrulelist,Controller ctrl)
        {
            foreach (var defrule in defrulelist)
            {
                if (pjdata.WhichTest.ToUpper().Contains(defrule.Key.ToUpper())
                    && pjdata.ErrAbbr.ToUpper().Contains(defrule.Value.ToUpper()))
                {
                    var traceviewlist = ExternalDataCollector.LoadTraceView2Local(pjdata.TestStation, pjdata.ModuleSerialNum, pjdata.WhichTest, pjdata.TestTimeStamp.ToString(), ctrl);
                    Create2ndCheckTask2(pjdata, firstengineer, defrule, ctrl, traceviewlist);
                    return true;
                }
            }//end foreach

            return false;
        }

        private static void CreateSystemIssues(List<ProjectTestData> failurelist, Controller ctrl,bool normalFAEnable = true, bool transflg = false)
        {
            if (failurelist.Count > 0)
            {
                //get default critical rule
                var defrulelist = new List<KeyValuePair<string, string>>();
                var defaultrules = string.Empty;
                CfgUtility.GetSysConfig(ctrl).TryGetValue("CRITICALDEFAULTRULE",out defaultrules);
                if (!string.IsNullOrEmpty(defaultrules))
                {
                    var splitstrs = defaultrules.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var item in splitstrs)
                    {
                        var kv = item.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        var tempdef = new KeyValuePair<string,string>(kv[0], kv[1]);
                        defrulelist.Add(tempdef);
                    }
                }

                //get first engineer
                var pjlist = ProjectViewModels.RetrieveOneProject(failurelist[0].ProjectKey);
                if (pjlist.Count == 0) { return; }
                var pj = pjlist[0];

                var firstengineer = "";
                var role = transflg ? ProjectViewModels.MEROLE : ProjectViewModels.ENGROLE;
                foreach (var m in pj.MemberList)
                {
                    if (string.Compare(m.Role, role) == 0)
                    {
                        firstengineer = m.Name;
                        break;
                    }
                }
                //get user critical rule
                var pjcriticalerrorlist = ProjectCriticalErrorVM.RetrievePJCriticalError(failurelist[0].ProjectKey, null);

                foreach (var item in failurelist)
                {
                    //default critical rule
                    if (defrulelist.Count > 0)
                    {
                        var ret = CheckDefaultCriticalError(item, firstengineer, defrulelist, ctrl);
                        if (ret)  continue;
                    }//end if

                    if (!CheckPJCriticalError(item,pjcriticalerrorlist,ctrl))
                    {
                        if (normalFAEnable && !transflg)
                        {
                            CreateFA(item, firstengineer,ctrl);
                        }
                    }
                }
            }
        }


        public static void DebugCriticalFunction(string dataid, string projectkey, Controller ctrl)
        {
            var pjdata = ProjectTestData.RetrieveProjectTestData(dataid);
            var pjcriticalerrorlist = ProjectCriticalErrorVM.RetrievePJCriticalError(projectkey, null);


                if (!CheckPJCriticalError(pjdata[0], pjcriticalerrorlist, ctrl))
                {
                    //CreateFA(pjdata[0], "BRAD.QIU@FINISAR.COM", ctrl);
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
                        var sql = s.Value.Replace("<TIMECOND>", "and TestTimeStamp > '" + vm.StartDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and TestTimeStamp < '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'");
                        var dbret = DBUtility.ExeMESSqlWithRes(sql);
                        foreach (var item in dbret)
                        {
                            try
                            {
                                if (item[1] == null)
                                { continue; }
                                var sn = Convert.ToString(item[1]).Trim();
                                if (string.IsNullOrEmpty(sn))
                                { continue; }

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


        //private static void logthdinfo(string info)
        //{
        //    try
        //    {
        //        var filename = "d:\\log\\osamapfilenocontain-" + DateTime.Now.ToString("yyyy-MM-dd");
        //        if (File.Exists(filename))
        //        {
        //            var content = System.IO.File.ReadAllText(filename);
        //            content = content + "\r\n" + DateTime.Now.ToString() + " : " + info;
        //            System.IO.File.WriteAllText(filename, content);
        //        }
        //        else
        //        {
        //            System.IO.File.WriteAllText(filename, DateTime.Now.ToString() + " : " + info);
        //        }
        //    }
        //    catch (Exception ex)
        //    { }

        //}

        private static string RetrieveOSAFailure(Dictionary<string, List<KeyValuePair<string, double>>> datafield,ProjectTestData pjdata, Dictionary<string, OSAFailureVM> mapfile)
        {
            try
            {
                if (datafield.ContainsKey(pjdata.DataID))
                {
                    var keyvaluelist = datafield[pjdata.DataID];
                    foreach(var kv in keyvaluelist)
                    { 
                        var combinekey = (kv.Key + "_" + pjdata.WhichTest).ToUpper();
                        if (mapfile.ContainsKey(combinekey))
                        {
                            var osafailurevm = mapfile[combinekey];
                            if (kv.Value >= osafailurevm.LowLimit && kv.Value <= osafailurevm.HighLimit)
                            {
                                //do nothing
                            }
                            else
                            {
                                return osafailurevm.FailureMode+"::"+osafailurevm.ErrorPriority+"::"+osafailurevm.ParameterName;
                            }
                        }
                        else
                        {
                            //logthdinfo(combinekey);
                        }
                    }//end foreach
                }
                return "OTHERS";
            }
            catch (Exception ex)
            {
                return "OTHERS";
            }
        }

        public static void StartOSAProjectBonding(ProjectViewModels vm)
        {
            try
            {
                if (ProjectTestData.UpdatePJLockUsing(vm.ProjectKey))
                    return;

                var osafailuremapdata = OSAFailureVM.RetrieveAllOSAFailureVM(vm.ProjectKey);

                if (osafailuremapdata.Count > 0
                    && vm.PNList.Count > 0)
                {

                    var sql = RetrieveOSASql(vm);
                    if (string.IsNullOrEmpty(sql))
                    {
                        ProjectTestData.ResetUpdatePJLock(vm.ProjectKey);
                        return;
                    }
                   
                    sql = sql.Replace("<TIMECOND>", "and TestTimeStamp > '" + vm.StartDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and TestTimeStamp < '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'");

                    var newdatalist = new List<ProjectTestData>();
                    var dbret = DBUtility.ExeMESSqlWithRes(sql);
                    foreach (var item in dbret)
                    {
                        try
                        {
                            var tempdata = new ProjectTestData(vm.ProjectKey, Convert.ToString(item[0]), Convert.ToString(item[1])
                                    ,Convert.ToString(item[2]), Convert.ToString(item[3]), Convert.ToString(item[4])
                                    , Convert.ToString(item[5]), Convert.ToString(item[6]), Convert.ToString(item[7]));
                            tempdata.JO = Convert.ToString(item[8]);
                            newdatalist.Add(tempdata);
                        }
                        catch (Exception ex)
                        { }
                    }//end foreach

                    if (newdatalist.Count > 0)
                    {
                        var datafield = RetrieveOSAFailedDataField(newdatalist);

                        //store data
                        foreach (var dt in newdatalist)
                        {
                            if (string.Compare(dt.ErrAbbr, "0", true) == 0)
                            {
                                if (datafield.Count > 0)
                                {
                                    dt.ErrAbbr = RetrieveOSAFailure(datafield, dt, osafailuremapdata).Split(new string[] { "::" },StringSplitOptions.RemoveEmptyEntries)[0];

                                    var ekey = ProjectErrorViewModels.GetUniqKey();
                                    var pjerror = new ProjectErrorViewModels(vm.ProjectKey, ekey, dt.ErrAbbr, "", 1);
                                    pjerror.Reporter = "System";
                                    pjerror.Description = "";
                                    pjerror.AddandUpdateProjectError();
                                }
                                else
                                {
                                    dt.ErrAbbr = "OTHERS";
                                }
                            }
                            else
                            {
                                dt.ErrAbbr = "PASS";
                            }

                            dt.StoreProjectTestData();
                        }

                    }//end if

                }

                ProjectTestData.ResetUpdatePJLock(vm.ProjectKey);
            }
            catch (Exception ex)
            {
                ProjectTestData.ResetUpdatePJLock(vm.ProjectKey);
            }

        }


        public static Dictionary<string, List<KeyValuePair<string, double>>> RetrieveOSAFailedDataField(List<ProjectTestData> newdatalist)
        {
            var datafield = new Dictionary<string, List<KeyValuePair<string, double>>>();

            var dataidcond = "('";
            foreach (var dt in newdatalist)
            {
                if (string.Compare(dt.ErrAbbr, "0", true) == 0)
                {
                    dataidcond = dataidcond + dt.DataID + "','";
                }
            }
            if (dataidcond.Length > 2)
                dataidcond = dataidcond.Substring(0, dataidcond.Length - 2) + ")";
            else
                dataidcond = string.Empty;

            if (!string.IsNullOrEmpty(dataidcond))
            {
                var sql = "  select ParentHistoryID,DataColumn,DataValue1 from insite.DCE_OSAMultiValueTest_Main(nolock) where ParentHistoryID in <ParentHistoryID> order by ParentHistoryID";
                sql = sql.Replace("<ParentHistoryID>", dataidcond);
                var dbret = DBUtility.ExeRealMESSqlWithRes(sql);
                foreach (var item in dbret)
                {
                    try
                    {
                        var dataid = Convert.ToString(item[0]).Trim();
                        var key = Convert.ToString(item[1]).Trim();
                        var value = Convert.ToDouble(item[2]);

                        if (datafield.ContainsKey(dataid))
                        {
                            datafield[dataid].Add(new KeyValuePair<string, double>(key, value));
                        }
                        else
                        {
                            var templist = new List<KeyValuePair<string, double>>();
                            templist.Add(new KeyValuePair<string, double>(key, value));
                            datafield.Add(dataid, templist);
                        }

                    }
                    catch (Exception ex)
                    { }

                }//end foreach
            }//end if


            return datafield;
        }

        public static void UpdateOSAProjectData(ProjectViewModels vm, string starttime, Controller ctrl, bool transflg = false)
        {
            var endtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            try
            {
                var osafailuremapdata = OSAFailureVM.RetrieveAllOSAFailureVM(vm.ProjectKey);

                if (osafailuremapdata.Count > 0
                    && vm.PNList.Count > 0)
                {
                    var sql = RetrieveOSASql(vm);
                    if (string.IsNullOrEmpty(sql))
                    {
                        return;
                    }
                    sql = sql.Replace("<TIMECOND>", "and TestTimeStamp > '" + starttime + "' and TestTimeStamp < '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'");
                    //sql = sql.Replace("<TIMECOND>", "and TestTimeStamp > '" + starttime + "' and TestTimeStamp < '" + DateTime.Parse(starttime).AddDays(3).ToString("yyyy-MM-dd HH:mm:ss") + "'");

                    var bondingeddatadict = ProjectTestData.RetrieveAllDataID(vm.ProjectKey);

                    var newdatalist = new List<ProjectTestData>();
                    var dbret = DBUtility.ExeMESSqlWithRes(sql);
                    foreach (var item in dbret)
                    {
                        try
                        {
                            var did = Convert.ToString(item[0]);
                            if (!bondingeddatadict.ContainsKey(did))
                            {
                                bondingeddatadict.Add(did,true);

                                var tempdata = new ProjectTestData(vm.ProjectKey, did, Convert.ToString(item[1])
                                        , Convert.ToString(item[2]), Convert.ToString(item[3]), Convert.ToString(item[4])
                                        , Convert.ToString(item[5]), Convert.ToString(item[6]), Convert.ToString(item[7]));
                                tempdata.JO = Convert.ToString(item[8]);
                                newdatalist.Add(tempdata);
                            }

                        }
                        catch (Exception ex)
                        { }
                    }//end foreach


                    if (newdatalist.Count > 0)
                    {
                        var sndict = new Dictionary<string, bool>();
                        
                        //close past FA
                        foreach (var dt in newdatalist)
                        {
                            if (sndict.ContainsKey(dt.ModuleSerialNum))
                            {
                                continue;
                            }
                            else
                            {
                                sndict.Add(dt.ModuleSerialNum, true);
                            }

                            if (vm.FinishRating < 90 && DateTime.Parse(starttime) != vm.StartDate)
                            {
                                if (string.Compare(dt.ErrAbbr, "0", true) == 0)
                                {
                                    IssueViewModels.CloseIssueAutomaticllyWithFailedSN(dt.ProjectKey, dt.ModuleSerialNum, dt.WhichTest, dt.TestStation, dt.TestTimeStamp.ToString("yyyy-MM-dd HH:mm:ss"), ctrl);
                                }
                                else
                                {
                                    IssueViewModels.CloseIssueAutomaticlly(dt.ProjectKey, dt.ModuleSerialNum, dt.WhichTest, dt.TestStation, dt.TestTimeStamp.ToString("yyyy-MM-dd HH:mm:ss"), ctrl);
                                }
                            }
                        }
                        sndict.Clear();

                        //get failed sn data field
                        var datafield = RetrieveOSAFailedDataField(newdatalist);

                        var firstengineer = "";
                        var role = transflg ? ProjectViewModels.MEROLE : ProjectViewModels.ENGROLE;
                        foreach (var m in vm.MemberList)
                        {
                            if (string.Compare(m.Role, role) == 0)
                            {
                                firstengineer = m.Name;
                                break;
                            }
                        }
                        //create FA
                        foreach (var dt in newdatalist)
                        {
                            if (string.Compare(dt.ErrAbbr, "0", true) == 0)
                            {
                                if (datafield.Count > 0)
                                {
                                    try
                                    {
                                        var failureret = RetrieveOSAFailure(datafield, dt, osafailuremapdata);
                                        if (failureret.Contains("::"))
                                        {
                                            var splitstrs = failureret.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
                                            dt.ErrAbbr = splitstrs[0];
                                            var priority = splitstrs[1];
                                            var failedparam = splitstrs[2];

                                            if (!sndict.ContainsKey(dt.ModuleSerialNum))
                                            {
                                                sndict.Add(dt.ModuleSerialNum, true);
                                                if (vm.FinishRating < 90 && DateTime.Parse(starttime) != vm.StartDate)
                                                {
                                                    if (!transflg)
                                                    {
                                                        CreateOSAFA(vm, dt, datafield[dt.DataID], priority, failedparam, firstengineer, ctrl);
                                                    }
                                                }
                                            }

                                            var ekey = ProjectErrorViewModels.GetUniqKey();
                                            var pjerror = new ProjectErrorViewModels(vm.ProjectKey, ekey, dt.ErrAbbr, "", 1);
                                            pjerror.Reporter = "System";
                                            pjerror.Description = "";
                                            pjerror.AddandUpdateProjectError();
                                        }
                                        else
                                        {
                                            dt.ErrAbbr = failureret;
                                        }
                                    }
                                    catch (Exception ex)
                                    { }
                                }
                                else
                                {
                                    dt.ErrAbbr = "OTHERS";
                                }
                            }
                            else
                            {
                                dt.ErrAbbr = "PASS";
                            }

                            if (!sndict.ContainsKey(dt.ModuleSerialNum))
                            {
                                sndict.Add(dt.ModuleSerialNum, true);
                            }
                        }

                        //store project data
                        foreach (var dt in newdatalist)
                        {
                            dt.StoreProjectTestData();
                        }

                   }//end if

                }

            }
            catch (Exception ex)
            {
            }
        }

        private static void CreateOSANormalFA(ProjectTestData item, List<KeyValuePair<string, double>> RawData, string FailedParam, string firstengineer)
        {
            var vm = new IssueViewModels();
            vm.ProjectKey = item.ProjectKey;
            vm.IssueKey = item.DataID;
            vm.IssueType = ISSUETP.Bug;
            vm.Summary = "Module " + item.ModuleSerialNum + " failed for " + item.ErrAbbr + " @ " + item.WhichTest;
            vm.Priority = ISSUEPR.Major;
            vm.DueDate = DateTime.Now.AddDays(7);
            vm.ReportDate = item.TestTimeStamp;
            vm.Assignee = firstengineer;
            vm.Reporter = "System";
            vm.Resolution = Resolute.Pending;
            vm.ResolvedDate = DateTime.Parse("1982-05-06 01:01:01");
            vm.CommentType = COMMENTTYPE.Description;
            vm.ModuleSN = item.ModuleSerialNum;
            vm.ErrAbbr = item.ErrAbbr;
            vm.DataID = item.DataID;

            vm.Description ="<p>" + "Module " + item.ModuleSerialNum + " failed for " + item.ErrAbbr + " @ " + item.WhichTest + " on tester " + item.TestStation + " " + item.TestTimeStamp.ToString("yyyy-MM-dd HH:mm:ss")+"</p>";
            vm.Description = vm.Description + "<table class='table table-hover'><thead><tr><th>Parameter</th><th>value</th></tr></thead><tbody>";
            foreach (var kv in RawData)
            {
                if (string.Compare(kv.Key, FailedParam, true) == 0)
                {
                    vm.Description = vm.Description + "<tr style='color:red'><td>" + kv.Key + "</td><td>" + kv.Value.ToString() + "</td></tr>";
                }
                else
                {
                    vm.Description = vm.Description + "<tr><td>"+kv.Key+"</td><td>"+kv.Value.ToString()+"</td></tr>";
                }
            }
            vm.Description = vm.Description + "</tbody></table>";
            vm.StoreIssue();

            IssueTypeVM.SaveIssueType(vm.IssueKey, ISSUESUBTYPE.Bug.ToString());
        }

        private static void CreateOSACriticalFA(ProjectTestData item, List<KeyValuePair<string, double>> RawData, string FailedParam, string firstengineer,Controller ctrl)
        {
            var vm = new IssueViewModels();
            vm.ProjectKey = item.ProjectKey;
            vm.IssueKey = item.DataID;
            vm.IssueType = ISSUETP.Bug;
            vm.Summary = CRITICALERRORTYPE.SECONDMATCH + " " + item.ModuleSerialNum + " failed for " + item.ErrAbbr + " @ " + item.WhichTest;
            vm.Priority = ISSUEPR.Critical;
            vm.DueDate = DateTime.Now.AddDays(7);
            vm.ReportDate = DateTime.Now;
            vm.Assignee = firstengineer;
            vm.Reporter = firstengineer;
            vm.Creator = firstengineer;
            vm.Resolution = Resolute.Pending;
            vm.ResolvedDate = DateTime.Parse("1982-05-06 01:01:01");
            vm.CommentType = COMMENTTYPE.Description;
            vm.ModuleSN = item.ModuleSerialNum;
            vm.ErrAbbr = item.ErrAbbr;
            vm.DataID = item.DataID;

            vm.Description = "<p>" + "Module " + item.ModuleSerialNum + " failed for " + item.ErrAbbr + " @ " + item.WhichTest + " on tester " + item.TestStation + " " + item.TestTimeStamp.ToString("yyyy-MM-dd HH:mm:ss") + "</p>";
            vm.Description = vm.Description + "<table class='table table-hover'><thead><tr><th>Parameter</th><th>value</th></tr></thead><tbody>";
            foreach (var kv in RawData)
            {
                if (string.Compare(kv.Key, FailedParam, true) == 0)
                {
                    vm.Description = vm.Description + "<tr style='color:red'><td>" + kv.Key + "</td><td>" + kv.Value.ToString() + "</td></tr>";
                }
                else
                {
                    vm.Description = vm.Description + "<tr><td>" + kv.Key + "</td><td>" + kv.Value.ToString() + "</td></tr>";
                }
            }
            vm.Description = vm.Description + "</tbody></table>";
            vm.StoreIssue();

            IssueTypeVM.SaveIssueType(vm.IssueKey, ISSUESUBTYPE.CrititalFailureTask.ToString(), ISSUESUBTYPE.CrititalFailureTask.ToString());

            SendTaskEvent(vm, vm.Summary, ctrl);
        }

        private static void CreateOSAFA(ProjectViewModels vm, ProjectTestData pjdata, List<KeyValuePair<string, double>> RawData, string ErrorPriority, string FailedParam,string firstengineer,Controller ctrl)
        {
            if (string.Compare(ErrorPriority, OSAFAILUREPRIORITY.NORMAL) == 0)
            {
                CreateOSANormalFA(pjdata, RawData, FailedParam, firstengineer);
            }
            else
            {
                CreateOSACriticalFA(pjdata, RawData, FailedParam, firstengineer,ctrl);
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
                    //var sql = s.Value.Replace("<TIMECOND>", "and TestTimeStamp > '" + starttime + "' and TestTimeStamp < '" + DateTime.Parse(starttime).AddDays(3).ToString("yyyy-MM-dd HH:mm:ss") + "'");
                    var dbret = DBUtility.ExeMESSqlWithRes(sql);
                    foreach (var item in dbret)
                    {
                        try
                        {
                            if (item[1] == null)
                            { continue; }
                            var sn = Convert.ToString(item[1]).Trim();
                            if (string.IsNullOrEmpty(sn))
                            { continue; }

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
                        IssueViewModels.CloseIssueAutomaticllyWithFailedSN(item.ProjectKey, item.ModuleSerialNum, item.WhichTest, item.TestStation, item.TestTimeStamp.ToString("yyyy-MM-dd HH:mm:ss"), ctrl);
                    }

                    CreateSystemIssues(failurelist, ctrl, true, !string.IsNullOrEmpty(vm.TransferFlg));
                }
                else
                {
                    CreateSystemIssues(failurelist, ctrl, false);
                }

                if (vm.FinishRating < 90 && DateTime.Parse(starttime) != vm.StartDate)
                {
                    //use pass sn cover previous failure
                    foreach (var item in passlist)
                    {
                        IssueViewModels.CloseIssueAutomaticlly(item.ProjectKey,item.ModuleSerialNum, item.WhichTest, item.TestStation, item.TestTimeStamp.ToString("yyyy-MM-dd HH:mm:ss"), ctrl);
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

        public static Dictionary<string, string> RetrievePNDescByPn(List<string> pnlist)
        {
            var ret = new Dictionary<string, string>();

            StringBuilder sb1 = new StringBuilder(10 * (pnlist.Count + 5));
            sb1.Append("('");
            foreach (var line in pnlist)
            {
                sb1.Append(line + "','");
            }
            var tempstr1 = sb1.ToString();
            var pncond = tempstr1.Substring(0, tempstr1.Length - 2) + ")";

            var sql = @"  select distinct pb.ProductName,p.[Description] from [InsiteDB].[insite].[ProductBase] pb (nolock) 
                        left join [InsiteDB].[insite].[Product] p with (nolock) on p.ProductBaseId = pb.ProductBaseId  
                        where pb.ProductName in <pncond>";
            sql = sql.Replace("<pncond>", pncond);
            var dbret = DBUtility.ExeMESSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var pn = Convert.ToString(line[0]);
                var pndesc = Convert.ToString(line[1]);
                if (!ret.ContainsKey(pn))
                {
                    ret.Add(pn, pndesc);
                }
            }
            return ret;
        }

        public static Dictionary<string,KeyValuePair<string,string>> RetrievePnPndescFromSN(List<string> snlist)
        {
            var ret = new Dictionary<string, KeyValuePair<string, string>>();

            var sql = @" select c.ContainerName,pb.ProductName,p.[Description] from [InsiteDB].[insite].[Container] c (nolock)
                     left join [InsiteDB].[insite].[Product] p with (nolock) on c.ProductId =p.ProductId
                     left join [InsiteDB].[insite].[ProductBase] pb with (nolock) on p.ProductBaseId = pb.ProductBaseId  
                     where c.ContainerName in <sncond>";
            var sncond = DBUtility.ConstructCond(snlist);
            sql = sql.Replace("<sncond>", sncond);
            var dbret = DBUtility.ExeMESSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var sn = Convert.ToString(line[0]);
                if (!ret.ContainsKey(sn))
                {
                    ret.Add(sn, new KeyValuePair<string, string>(Convert.ToString(line[1]),Convert.ToString(line[2])));
                }
            }
            return ret;
        }

        private static List<string> GetPNsFromPNDesc(List<string> pns)
        {
            var ret = new List<string>();
            if (pns.Count == 0)
            { return ret; }

            var cond = "";
            foreach (var pn in pns)
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
            { return ret; }

            var sql = "select DISTINCT p.ProductName from insite.Product c (nolock) left join insite.ProductBase p on c.ProductBaseId = p.ProductBaseId where " + cond;
            var dbret = DBUtility.ExeMESSqlWithRes(sql);

            foreach (var line in dbret)
            {
                var pn = Convert.ToString(line[0]);
                ret.Add(pn);
            }

            return ret;
        }

        public static string GetPNCondFromPNDesc(string pndesc)
        {
            var pncond = "";

            var pns = pndesc.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var pnnumlist = GetPNsFromPNDesc(pns);
            foreach (var pn in pns)
            {
                if (IsDigitsOnly(pn.Trim()))
                {
                    pnnumlist.Add(pn);
                }
            }
            if (pnnumlist.Count == 0)
            { return pncond; }

            var sb = new StringBuilder(pnnumlist.Count * 30);
            sb.Append("('");
            foreach (var pn in pnnumlist)
            {
                sb.Append(pn + "','");
            }
            pncond = sb.ToString();
            pncond = pncond.Substring(0, pncond.Length - 2) + ")";

            return pncond;
        }


        public static List<double> FilterStrangeValue(List<double> rawdata)
        {
            var filter1list = new List<double>();
            foreach (var v in rawdata)
            {
                if (v != -9999.0 && v != 9999.0
                    && v != -999.0 && v != 999.0 
                    && v != double.NaN)
                {
                    filter1list.Add(v);
                }
            }

            var mean = Statistics.Mean(filter1list);
            var stddev = Statistics.StandardDeviation(filter1list);

            var min = Math.Min(mean - 6.0 * stddev, mean + 6.0 * stddev);
            var max = Math.Max(mean - 6.0 * stddev, mean + 6.0 * stddev);
            var filter2list = new List<double>();
            foreach (var v in filter1list)
            {
                if (v >= min && v <= max)
                {
                    filter2list.Add(v);
                }
            }

            return filter2list;
        }

        public static List<object> GetMinMaxList(List<List<object>> dbret)
        {
            var minlist = new List<double>();
            var maxlist = new List<double>();

            var currentvallist = new List<double>();
            var currentsn = "";

            foreach (var line in dbret)
            {
                try
                {
                    var sn = Convert.ToString(line[0]);
                    var val = Convert.ToDouble(line[1]);
                    if (string.Compare(sn, currentsn, true) != 0)
                    {
                        if (currentvallist.Count > 0)
                        {
                            minlist.Add(currentvallist.Min());
                            maxlist.Add(currentvallist.Max());
                        }
                        currentsn = sn;
                        currentvallist.Clear();

                        currentvallist.Add(val);
                    }
                    else
                    {
                        currentvallist.Add(val);
                    }
                } catch (Exception ex) { }
            }

            if (currentvallist.Count > 0)
            {
                minlist.Add(currentvallist.Min());
                maxlist.Add(currentvallist.Max());
            }

            var ret = new List<object>();
            ret.Add(FilterStrangeValue(minlist));
            ret.Add(FilterStrangeValue(maxlist));
            return ret;
        }

        public static List<object> GetTestData(string pndesc, string mestab, string param,string cornid, string startdate, string enddate, bool onlypass)
        {
            var ret = new List<object>();
            var pncond = GetPNCondFromPNDesc(pndesc);
            if (string.IsNullOrEmpty(pncond))
            { return ret; }

            var sql = "";
            if (onlypass)
            {
                sql = @"select top 100000 dc.[ModuleSerialNum],dce.[<DATAFIELDPARAMETER>] from [InsiteDB].[insite].[dce_<MESTABNAME>_main] dce 
                        left join [InsiteDB].[insite].[dc_<MESTABNAME>] dc on dc.dc_<MESTABNAME>HistoryId = dce.ParentHistoryID
                        where dc.ErrAbbr = 'pass' and dc.TestTimeStamp > '<STARTDATE>' and dc.TestTimeStamp < '<ENDDATE>' 
                        and dc.[ModuleSerialNum] is not null and dce.CornerID like '<CORNID>' and dc.AssemblyPartNum in <PNCOND>
                        order by dc.TestTimeStamp desc,dc.[ModuleSerialNum]";
            }
            else
            {
                sql = @"select top 100000 dc.[ModuleSerialNum],dce.[<DATAFIELDPARAMETER>] from [InsiteDB].[insite].[dce_<MESTABNAME>_main] dce 
                        left join [InsiteDB].[insite].[dc_<MESTABNAME>] dc on dc.dc_<MESTABNAME>HistoryId = dce.ParentHistoryID 
                        where dc.TestTimeStamp > '<STARTDATE>'  and dc.TestTimeStamp < '<ENDDATE>' and dc.[ModuleSerialNum] is not null 
                        and dce.CornerID like '<CORNID>' and dc.AssemblyPartNum in <PNCOND> 
                        order by dc.TestTimeStamp desc,dc.[ModuleSerialNum]";
            }

            sql = sql.Replace("<DATAFIELDPARAMETER>", param).Replace("<MESTABNAME>", mestab)
                .Replace("<STARTDATE>", startdate + " 00:00:00").Replace("<ENDDATE>", enddate + " 23:59:59")
                .Replace("<CORNID>", cornid).Replace("<PNCOND>", pncond);

            var dbret = DBUtility.ExeMESSqlWithRes(sql);
            if (dbret.Count == 0)
            { return ret; }

            return GetMinMaxList(dbret);
        }

    }
}