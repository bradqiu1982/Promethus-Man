using Prometheus.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Prometheus.Models
{
    public class BIDataUtility
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
            bool bondinged = false;
            string ret = "('";
            foreach (var pn in pns)
            {
                if (IsDigitsOnly(pn.Pn.Trim()))
                {
                    bondinged = true;
                    ret = ret + RMSpectialCh(pn.Pn.Trim()) + "','";
                }
            }

            if (bondinged)
            {
                ret = ret.Substring(0, ret.Length - 2) + ")";
            }
            else
            {
                ret = string.Empty;
            }

            return ret;
        }

        private static string GetUniqKey()
        {
            return Guid.NewGuid().ToString("N");
        }

        private static double ConvertDouble(object value)
        {
            if (value != null)
            {
                try
                {
                    return Convert.ToDouble(value);
                }
                catch (Exception ex)
                {
                    return 0.0;
                }
            }
            else
            {
                return 0.0;
            }
        }

        private static string ConvertString(object value)
        {
            if (value != null)
            {
                try
                {
                    return Convert.ToString(value);
                }
                catch (Exception ex)
                {
                    return string.Empty;
                }
            }
            else
            {
                return string.Empty;
            }
        }

        public static void StartProjectBonding(Controller ctrl, ProjectViewModels vm)
        {
            var vtab = new List<string>();
            vtab.Add("dbo.PRLL_VcselInfoSummary_2016");
            vtab.Add("dbo.PRLL_VcselInfoSummary_2017");
            foreach (var vt in vtab)
            {
                try
                {
                    if (BITestData.UpdatePJLockUsing(vm.ProjectKey))
                        return;

                        if (vm.PNList.Count > 0)
                        {
                            var bondeddatadict = BITestData.RetrieveAllDataID(vm.ProjectKey);

                            //var failurelist = new List<BITestData>();
                            var sql = "select ModuleSerialNum,Step,ModuleType,ErrAbbr,TestTimeStamp,TestStation,ModulePartNum,wafer,waferpn from  "+vt+ "(nolock) where wafer <> 'NULL' and waferpn  <> 'NULL' and ErrAbbr  <> 'NULL' and ModulePartNum in <PNCOND> and TestTimeStamp > '<TIMECOND>'  and TestTimeStamp < '" + DateTime.Now.ToString() + "' order by TestTimeStamp Desc,ModuleSerialNum";
                            var pncond = PNCondition(vm.PNList);

                        if (string.IsNullOrEmpty(pncond))
                        {
                            BITestData.ResetUpdatePJLock(vm.ProjectKey);
                            return;
                        }

                            sql = sql.Replace("<PNCOND>",pncond).Replace("<TIMECOND>", vm.StartDate.ToString());

                            var tempdataiddict = new Dictionary<string, BITestData>();
                            var dbret = DBUtility.ExePRLSqlWithRes(ctrl,sql);
                            foreach (var line in dbret)
                            {
                                var tempid = ConvertString(line[0])+ "_" + ConvertString(line[1]);

                                if (bondeddatadict.ContainsKey(tempid))
                                {
                                    if (DateTime.Parse(ConvertString(line[4])) <= bondeddatadict[tempid])
                                    {
                                        continue;
                                    }
                                }

                                if (tempdataiddict.ContainsKey(tempid))
                                {
                                    if (DateTime.Parse(ConvertString(line[4])) == tempdataiddict[tempid].TestTimeStamp)
                                    {
                                        if (string.Compare(ConvertString(line[3]), "Pass", true) != 0)
                                        {
                                            tempdataiddict[tempid].ErrAbbr = ConvertString(line[3]);

                                            var ekey = ProjectErrorViewModels.GetUniqKey();
                                            var pjerror = new ProjectErrorViewModels(ProjectErrorViewModels.BURNIN, ekey, ConvertString(line[3]), "", 1);
                                            pjerror.Reporter = "System";
                                            pjerror.Description = "";
                                            pjerror.AddandUpdateProjectError();
                                        }

                                        
                                    }//same time
                                }
                                else
                                {
                                    var tempdata = new BITestData();
                                    tempdata.DataID = GetUniqKey();
                                    tempdata.ProjectKey = vm.ProjectKey;
                                    tempdata.ModuleSerialNum = ConvertString(line[0]);
                                    tempdata.WhichTest = ConvertString(line[1]);
                                    tempdata.ModuleType = ConvertString(line[2]);
                                    tempdata.ErrAbbr = ConvertString(line[3]);
                                    tempdata.TestTimeStamp = DateTime.Parse(ConvertString(line[4]));
                                    tempdata.TestStation = ConvertString(line[5]);
                                    tempdata.PN = ConvertString(line[6]);
                                    tempdata.Wafer = ConvertString(line[7]);
                                    tempdata.Waferpn = ConvertString(line[8]);

                                    tempdataiddict.Add(tempid, tempdata);
                                }
                            }//end foreach

                            foreach (var kv in tempdataiddict)
                            {
                                kv.Value.StoreBIData();
                            }//end foreach

                        }

                    BITestData.ResetUpdatePJLock(vm.ProjectKey);
                }
                catch (Exception ex)
                {
                    BITestData.ResetUpdatePJLock(vm.ProjectKey);
                }
            }




        }


        public static void UpdateProjectData(Controller ctrl, ProjectViewModels vm)
        {
            var vtab = new List<string>();
            vtab.Add("dbo.PRLL_VcselInfoSummary_2016");
            vtab.Add("dbo.PRLL_VcselInfoSummary_2017");
            foreach (var vt in vtab)
            {
                try
                {
                    if (vm.PNList.Count > 0)
                        {
                            var failurelist = new List<BITestData>();
                            var failuredict = new Dictionary<string, bool>();
                            var passlist = new List<BITestData>();

                            var starttime = BITestData.RetrieveLatestTimeOfLocalBI(vm.ProjectKey);
                            if (string.IsNullOrEmpty(starttime))
                            {
                                starttime = vm.StartDate.ToString();
                            }

                            var sql = "select ModuleSerialNum,Step,ModuleType,ErrAbbr,TestTimeStamp,TestStation,ModulePartNum,wafer,waferpn from  "+vt+"(nolock) where wafer <> 'NULL' and waferpn  <> 'NULL' and ErrAbbr  <> 'NULL' and ModulePartNum in <PNCOND> and TestTimeStamp > '<TIMECOND>' order by TestTimeStamp Desc,ModuleSerialNum";
                            var pncond = PNCondition(vm.PNList);

                        if (string.IsNullOrEmpty(pncond))
                        {
                            return;
                        }

                        sql = sql.Replace("<PNCOND>", pncond).Replace("<TIMECOND>", starttime);

                            var tempdataiddict = new Dictionary<string, BITestData>();
                            var dbret = DBUtility.ExePRLSqlWithRes(ctrl,sql);
                            foreach (var line in dbret)
                            {
                                var tempid = ConvertString(line[0]) + "_" + ConvertString(line[1]);
                                if (tempdataiddict.ContainsKey(tempid))
                                {
                                    if (DateTime.Parse(ConvertString(line[4])) == tempdataiddict[tempid].TestTimeStamp)
                                    {
                                        if (string.Compare(ConvertString(line[3]), "Pass", true) != 0)
                                        {
                                            tempdataiddict[tempid].ErrAbbr = ConvertString(line[3]);

                                            if (!failuredict.ContainsKey(tempdataiddict[tempid].ModuleSerialNum))
                                            {
                                                failuredict.Add(tempdataiddict[tempid].ModuleSerialNum, true);
                                                failurelist.Add(tempdataiddict[tempid]);

                                                var ekey = ProjectErrorViewModels.GetUniqKey();
                                                var pjerror = new ProjectErrorViewModels(ProjectErrorViewModels.BURNIN, ekey, ConvertString(line[3]), "", 1);
                                                pjerror.Reporter = "System";
                                                pjerror.Description = "";
                                                pjerror.AddandUpdateProjectError();
                                        }
                                
                                        }

                                    }//same time
                                }
                                else
                                {
                                    var tempdata = new BITestData();
                                    tempdata.DataID = GetUniqKey();
                                    tempdata.ProjectKey = vm.ProjectKey;
                                    tempdata.ModuleSerialNum = ConvertString(line[0]);
                                    tempdata.WhichTest = ConvertString(line[1]);
                                    tempdata.ModuleType = ConvertString(line[2]);
                                    tempdata.ErrAbbr = ConvertString(line[3]);
                                    tempdata.TestTimeStamp = DateTime.Parse(ConvertString(line[4]));
                                    tempdata.TestStation = ConvertString(line[5]);
                                    tempdata.PN = ConvertString(line[6]);
                                    tempdata.Wafer = ConvertString(line[7]);
                                    tempdata.Waferpn = ConvertString(line[8]);

                                    tempdataiddict.Add(tempid, tempdata);

                                    if (string.Compare(tempdata.ErrAbbr, "Pass", true) != 0)
                                    {
                                        if (!failuredict.ContainsKey(tempdata.ModuleSerialNum))
                                        {
                                            failuredict.Add(tempdata.ModuleSerialNum, true);
                                            failurelist.Add(tempdata);

                                            var ekey = ProjectErrorViewModels.GetUniqKey();
                                            var pjerror = new ProjectErrorViewModels(ProjectErrorViewModels.BURNIN, ekey, tempdata.ErrAbbr, "", 1);
                                            pjerror.Reporter = "System";
                                            pjerror.Description = "";
                                            pjerror.AddandUpdateProjectError();
                                    }
                                    }
                                    else
                                    {
                                        passlist.Add(tempdata);
                                    }
                                }
                            }//end foreach

                            foreach (var kv in tempdataiddict)
                            {
                                kv.Value.StoreBIData();
                            }//end foreach

                            if (DateTime.Parse(starttime) != vm.StartDate)
                            {
                                CreateSystemIssues(failurelist);
                                IssueViewModels.CloseBIIssueAutomaticlly(passlist);
                            }
                        }

                }
                catch (Exception ex)
                {
                }
            }



        }

        public static void RetrievePjWaferAllData(Controller ctrl, ProjectViewModels vm)
        {
            var vtab = new List<string>();
            vtab.Add("dbo.PRLL_VcselInfoSummary_2016");
            vtab.Add("dbo.PRLL_VcselInfoSummary_2017");
            foreach (var vt in vtab)
            {
                try
                {
                    var wafers = BITestData.RetrieveAllWafer(vm.ProjectKey);
                    if (vm.PNList.Count > 0 && wafers.Count > 0)
                    {
                        var failurelist = new List<BITestData>();
                        var failuredict = new Dictionary<string, bool>();
                        var passlist = new List<BITestData>();

                        var starttime = vm.StartDate.ToString();

                        var sql = "select ModuleSerialNum,Step,ModuleType,ErrAbbr,TestTimeStamp,TestStation,ModulePartNum,wafer,waferpn from  "+vt+"(nolock) where wafer in <WAFERCOND> and waferpn  <> 'NULL' and ErrAbbr  <> 'NULL' and ModulePartNum in <PNCOND> and TestTimeStamp <= '<TIMECOND>' order by TestTimeStamp Desc,ModuleSerialNum";
                        var pncond = PNCondition(vm.PNList);
                        if (string.IsNullOrEmpty(pncond))
                        {
                            return;
                        }
                        var wafercond = "('";
                        foreach (var w in wafers)
                        {
                            wafercond = wafercond + w + "','";
                        }
                        wafercond = wafercond.Substring(0, wafercond.Length - 2) + ")";

                        sql = sql.Replace("<PNCOND>", pncond).Replace("<TIMECOND>", starttime).Replace("<WAFERCOND>", wafercond);

                        var dbret = DBUtility.ExePRLSqlWithRes(ctrl,sql);
                        if (dbret.Count > 0)
                        {
                            var bondeddatadict = BITestData.RetrieveAllDataIDASC(vm.ProjectKey);
                            var tempdataiddict = new Dictionary<string, BITestData>();

                            foreach (var line in dbret)
                            {
                                var tempid = ConvertString(line[0]) + "_" + ConvertString(line[1]);

                                    if (!bondeddatadict.ContainsKey(tempid))
                                    {
                                        if (tempdataiddict.ContainsKey(tempid))
                                        {
                                            if (DateTime.Parse(ConvertString(line[4])) == tempdataiddict[tempid].TestTimeStamp)
                                            {
                                                if (string.Compare(ConvertString(line[3]), "Pass", true) != 0)
                                                {
                                                    tempdataiddict[tempid].ErrAbbr = ConvertString(line[3]);

                                                    if (!failuredict.ContainsKey(tempdataiddict[tempid].ModuleSerialNum))
                                                    {
                                                        failuredict.Add(tempdataiddict[tempid].ModuleSerialNum, true);
                                                        failurelist.Add(tempdataiddict[tempid]);

                                                        var ekey = ProjectErrorViewModels.GetUniqKey();
                                                        var pjerror = new ProjectErrorViewModels(ProjectErrorViewModels.BURNIN, ekey, ConvertString(line[3]), "", 1);
                                                        pjerror.Reporter = "System";
                                                        pjerror.Description = "";
                                                        pjerror.AddandUpdateProjectError();
                                                }

                                                }

                                            }//same time
                                        }
                                        else
                                        {
                                            var tempdata = new BITestData();
                                            tempdata.DataID = GetUniqKey();
                                            tempdata.ProjectKey = vm.ProjectKey;
                                            tempdata.ModuleSerialNum = ConvertString(line[0]);
                                            tempdata.WhichTest = ConvertString(line[1]);
                                            tempdata.ModuleType = ConvertString(line[2]);
                                            tempdata.ErrAbbr = ConvertString(line[3]);
                                            tempdata.TestTimeStamp = DateTime.Parse(ConvertString(line[4]));
                                            tempdata.TestStation = ConvertString(line[5]);
                                            tempdata.PN = ConvertString(line[6]);
                                            tempdata.Wafer = ConvertString(line[7]);
                                            tempdata.Waferpn = ConvertString(line[8]);

                                            tempdataiddict.Add(tempid, tempdata);

                                        }
                                }//end if (!bondeddatadict.ContainsKey(tempid))
                            }//end foreach

                            foreach (var kv in tempdataiddict)
                            {
                                kv.Value.StoreBIData();
                            }//end foreach

                        }//end if (dbret.Count > 0)
                    }//end if pn > 0

                }
                catch (Exception ex)
                {
                }
            }



        }

        private static void CreateSystemIssues(List<BITestData> failurelist)
        {
            if (failurelist.Count > 0)
            {
                foreach (var item in failurelist)
                {
                    var vm = new IssueViewModels();
                    vm.ProjectKey = item.ProjectKey;
                    vm.IssueKey = item.DataID;
                    vm.IssueType = ISSUETP.Bug;
                    vm.Summary = "Module " + item.ModuleSerialNum + " failed for " + item.ErrAbbr + " @Burn-In Step " + item.WhichTest;
                    vm.Priority = ISSUEPR.Major;
                    vm.DueDate = DateTime.Now.AddDays(7);
                    vm.ReportDate = item.TestTimeStamp;
                    vm.Assignee = "DALY.LI@FINISAR.COM";
                    vm.Reporter = "System";
                    vm.Resolution = Resolute.Pending;
                    vm.ResolvedDate = DateTime.Parse("1982-05-06 01:01:01");
                    vm.Description = "Module " + item.ModuleSerialNum + " failed for " + item.ErrAbbr + " @Burn-In Step " + item.WhichTest + " on tester " + item.TestStation + " " + item.TestTimeStamp.ToString("yyyy-MM-dd hh:mm:ss");
                    vm.CommentType = COMMENTTYPE.Description;
                    vm.ModuleSN = item.ModuleSerialNum;
                    vm.ErrAbbr = item.ErrAbbr;
                    vm.DataID = item.DataID;
                    //ProjectEvent.CreateIssueEvent(vm.ProjectKey, "System", vm.Assignee, vm.Summary, vm.IssueKey);
                    vm.StoreIssue();
                }
            }
        }


    }
}