using Prometheus.Controllers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Prometheus.Models
{
    public class BISNRelation
    {
        public BISNRelation()
        {
            wafer = string.Empty;
            productname = string.Empty;
        }

        public string wafer { set; get; }
        public string productname { set; get; }
    }

    public class ModuleSNRelation
    {
        public ModuleSNRelation()
        {
            wafer = string.Empty;
            jo = string.Empty;
        }

        public string wafer { set; get; }
        public string jo { set; get; }
    }

    public class BITestResult
    {
        public BITestResult()
        {
            Init();
        }

        public BITestResult(string sn,string testname,string station,string failure,DateTime testtime,string pn,string jo,string bitable,string dataid)
        {
            Init();
            SN = sn;
            TestName = testname;
            Station = station;
            Failure = failure;
            TestTimeStamp = testtime;
            PN = pn;
            JO = jo;
            BITable = bitable;
            DataID = dataid;
        }

        private void Init()
        {
            SN=string.Empty;
            TestName=string.Empty;
            Station=string.Empty;
            Failure=string.Empty;
            TestTimeStamp = DateTime.Parse("1982-05-06 07:30:00");
            PN=string.Empty;
            Wafer=string.Empty;
            JO=string.Empty;
            BITable=string.Empty;
            ProductName = string.Empty;
            ProjectKey = string.Empty;
            DataID = string.Empty;
            Appv_1 =string.Empty;
            Appv_2=string.Empty;
            Appv_3=string.Empty;
            Appv_4 = DateTime.Parse("1982-05-06 07:30:00");
        }

        public string SN { set; get; }
        public string TestName { set; get; }
        public string Station { set; get; }
        public string Failure { set; get; }
        public DateTime TestTimeStamp { set; get; }
        public string PN { set; get; }
        public string Wafer { set; get; }
        public string JO { set; get; }
        public string BITable {set;get;}
        public string ProductName { set; get; }
        public string ProjectKey { set; get; }
        public string DataID { set; get; }
        public string Appv_1 { set; get; }
        public string Appv_2 { set; get; }
        public string Appv_3 { set; get; }
        public DateTime Appv_4 { set; get; }
    }

    public class BITestResultDataField
    {
        public BITestResultDataField()
        {
            Init();
        }

        public BITestResultDataField(string sn, string testname, DateTime testtime, string pn, string jo
            ,string ch,double slope,double pold,double pouniformity,double thold,double dpold,double dslope,double dthold,double dpouniformity,string dataid)
        {
            Init();
            SN = sn;
            TestName = testname;
            TestTimeStamp = testtime;
            PN = pn;
            JO = jo;
            Channel = ch;
            SLOPE = slope;
            PO_LD = pold;
            PO_Uniformity = pouniformity;
            THOLD = thold;
            Delta_PO_LD = dpold;
            Delta_SLOPE = dslope;
            Delta_THOLD = dthold;
            Delta_PO_Uniformity = dpouniformity;
            DataID = dataid;
        }

        private void Init()
        {
            SN = string.Empty;
            TestName = string.Empty;
            TestTimeStamp = DateTime.Parse("1982-05-06 07:30:00");
            PN = string.Empty;
            Wafer = string.Empty;
            JO = string.Empty;
            Channel = string.Empty;
            SLOPE = 0;
            PO_LD = 0;
            PO_Uniformity = 0;
            THOLD = 0;
            Delta_PO_LD = 0;
            Delta_SLOPE = 0;
            Delta_THOLD = 0;
            Delta_PO_Uniformity = 0;
            ProductName = string.Empty;
            ProjectKey = string.Empty;
            DataID = string.Empty;
            Appv_1 = 0;
            Appv_2 = 0;
            Appv_3 = string.Empty;
            Appv_4 = string.Empty;
            Appv_5 = DateTime.Parse("1982-05-06 07:30:00");
        }

        public string SN { set; get; }
        public string TestName { set; get; }
        public DateTime TestTimeStamp { set; get; }
        public string PN { set; get; }
        public string Wafer { set; get; }
        public string JO { set; get; }
        public string Channel { set; get; }
        public double SLOPE { set; get; }
        public double PO_LD { set; get; }
        public double PO_Uniformity { set; get; }
        public double THOLD { set; get; }
        public double Delta_PO_LD { set; get; }
        public double Delta_SLOPE { set; get; }
        public double Delta_THOLD { set; get; }
        public double Delta_PO_Uniformity { set; get; }
        public string ProductName { set; get; }
        public string ProjectKey { set; get; }
        public string DataID { set; get; }
        public double Appv_1 { set; get; }
        public double Appv_2 { set; get; }
        public string Appv_3 { set; get; }
        public string Appv_4 { set; get; }
        public DateTime Appv_5 { set; get; }
    }

    public class ModuleTXOData
    {
        public ModuleTXOData()
        {
            Init();
        }

        private void Init()
        {
            SN = string.Empty;
            TxPower = -99999;
            TestName = string.Empty;
            TestTimeStamp = DateTime.Parse("1982-05-06 07:30:00");
            PN = string.Empty;
            Wafer = string.Empty;
            JO = string.Empty;
            Channel = string.Empty;
            Temperature = 0;
            Appv_1 = 0;
            Appv_2 = 0;
            Appv_3 = string.Empty;
            Appv_4 = string.Empty;
            Appv_5 = DateTime.Parse("1982-05-06 07:30:00");
        }

        public string SN { set; get; }
        public double TxPower { set; get; }
        public string TestName { set; get; }
        public DateTime TestTimeStamp { set; get; }
        public string PN { set; get; }
        public string Wafer { set; get; }
        public string JO { set; get; }
        public string Channel { set; get; }
        public double Temperature { set; get; }
        public double Appv_1 { set; get; }
        public double Appv_2 { set; get; }
        public string Appv_3 { set; get; }
        public string Appv_4 { set; get; }
        public DateTime Appv_5 { set; get; }
    }

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
                                CreateSystemIssues(failurelist,ctrl);
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

        private static void CreateSystemIssues(List<BITestData> failurelist, Controller ctrl)
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

                    var syscfgdict = CfgUtility.GetSysConfig(ctrl);
                    vm.Assignee = syscfgdict["BIADMIN"].ToUpper();
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

        private static string RetrieveLatestTimeStampOfAnBITable(string bt,string bizerotime)
        {
            var sql = "select top 1 TestTimeStamp from BITestResult where BITable = '<BITable>' order by TestTimeStamp DESC";
            sql = sql.Replace("<BITable>", bt);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            if (dbret.Count > 0)
            {
                try
                {
                    return DateTime.Parse(Convert.ToString(dbret[0][0])).ToString();
                }
                catch (Exception ex)
                {
                    return bizerotime;
                }
            }
            else
            {
                return bizerotime;
            }
        }

        private static string ConvertToDateStr(object datestr)
        {
            try
            {
                return Convert.ToDateTime(datestr).ToString();
            }
            catch (Exception ex) { return "1982-05-06 10:00:00"; }
        }

        public static void LoadBITestDateFromAuto(Controller ctrl)
        {
            var syscfgdict = CfgUtility.GetSysConfig(ctrl);
            var bitables = syscfgdict["BIAUTOTABLES"].Split(new string[] { ";"},StringSplitOptions.RemoveEmptyEntries);
            var bizerotime = syscfgdict["BIZEROTIME"];

            foreach (var bt in bitables)
            {
                var testresultlist = new List<BITestResult>();
                var testresultfieldlist = new List<BITestResultDataField>();

                var bilatesttime = RetrieveLatestTimeStampOfAnBITable(bt, bizerotime);

                var sql = "select ContainerName,ProcessStep,DateTime,Failure_Mode,Station,Work_Order,PN,Channel,SLOPE,PO_LD,PO_Uniformity,THOLD,Delta_PO_LD,Delta_SLOPE,Delta_THOLD,Delta_PO_Uniformity from <bitable> where DateTime > '<bilatesttime>' "
                    + " and  ProcessStep is not null and PN is not null and Work_Order is not null and ContainerName is not null and Failure_Mode is not null and ContainerName <> '' "
                    + " and Failure_Mode <> '--' and Failure_Mode <> '' and Failure_Mode is not null and Delta_PO_LD is not null and Delta_SLOPE is not null and Delta_THOLD is not null "
                    + " and Delta_PO_Uniformity is not null  order by ContainerName,DateTime";

                sql = sql.Replace("<bitable>", bt).Replace("<bilatesttime>", bilatesttime);


                var previoussn = string.Empty;
                var dataid = string.Empty;

                var dbret = DBUtility.ExeAutoSqlWithRes(sql);
                foreach (var line in dbret)
                {
                    try
                    {
                        var sn = Convert.ToString(line[0]);
                        var testname = Convert.ToString(line[1]);
                        var testtime = DateTime.Parse(ConvertToDateStr(line[2]));
                        var failure = Convert.ToString(line[3]);
                        var station = Convert.ToString(line[4]);
                        var jo = Convert.ToString(line[5]);
                        var pn = Convert.ToString(line[6]);
                        var ch = Convert.ToString(line[7]);
                        var slope = Convert.ToDouble(line[8]);
                        var pold = Convert.ToDouble(line[9]);
                        var pouniformity = Convert.ToDouble(line[10]);
                        var thold = Convert.ToDouble(line[11]);
                        var dpold = Convert.ToDouble(line[12]);
                        var dslope = Convert.ToDouble(line[13]);
                        var dthold = Convert.ToDouble(line[14]);
                        var dpouniformity = Convert.ToDouble(line[15]);

                        if (string.Compare(previoussn, sn + ":" + testname, true) != 0)
                        {
                            previoussn = sn+":"+testname;
                            dataid = IssueViewModels.GetUniqKey();
                            var tempresult = new BITestResult(sn, testname, station, failure,testtime, pn, jo, bt,dataid);
                            testresultlist.Add(tempresult);
                        }

                        var tempfield = new BITestResultDataField(sn, testname, testtime, pn, jo, ch, slope, pold, pouniformity, thold, dpold, dslope, dthold, dpouniformity,dataid);
                        testresultfieldlist.Add(tempfield);
                    }
                    catch (Exception ex) { }

                }//end foreach

                var snwaferdict = new Dictionary<string, BISNRelation>();
                var sumdblist = new List<string>();
                sumdblist.Add("SummaryDB.dbo.PRLL_VcselInfoSummary_2017");
                sumdblist.Add("SummaryDB.dbo.PRLL_VcselInfoSummary_2016");
                sumdblist.Add("SummaryDB.dbo.PRLL_VcselInfoSummary_2018");
                sumdblist.Add("SummaryDB.dbo.PRLL_VcselInfoSummary_2019");

                foreach (var tab in sumdblist)
                {
                    RetrieveBIWaferBySN(testresultlist, snwaferdict,tab);
                }

                foreach (var item in testresultlist)
                {
                    if (snwaferdict.ContainsKey(item.SN))
                    {
                        item.Wafer = snwaferdict[item.SN].wafer;
                        item.ProductName = snwaferdict[item.SN].productname;
                    }
                }//end foreach
                foreach (var item in testresultfieldlist)
                {
                    if (snwaferdict.ContainsKey(item.SN))
                    {
                        item.Wafer = snwaferdict[item.SN].wafer;
                        item.ProductName = snwaferdict[item.SN].productname;
                    }
                }

                StoreBITestResult(testresultlist);
                StoreBITestResultDateField(testresultfieldlist);

            }//end foreach
        }

        private static void StoreBITestResult(List<BITestResult> testresultlist)
        {
            //BITestResult
            var datatable = new System.Data.DataTable();

            PropertyInfo[] properties = typeof(BITestResult).GetProperties();
            var i = 0;
            for (i = 0; i < properties.Length;)
            {
                datatable.Columns.Add(properties[i].Name, properties[i].PropertyType);
                i = i + 1;
            }

            foreach (var testresult in testresultlist)
            {
                properties = typeof(BITestResult).GetProperties();
                var temprow = new object[properties.Length];
                for (i = 0; i < properties.Length;)
                {
                    temprow[i] = properties[i].GetValue(testresult);
                    i = i + 1;
                }
                datatable.Rows.Add(temprow);
            }

            WriteDBWithTable(datatable, "BITestResult");

            //datatable.Columns.Add("SN", typeof(string));
            //datatable.Columns.Add("TestName", typeof(string));
            //datatable.Columns.Add("Station", typeof(string));
            //datatable.Columns.Add("Failure", typeof(string));
            //datatable.Columns.Add("TestTimeStamp", typeof(DateTime));
            //datatable.Columns.Add("PN", typeof(string));
            //datatable.Columns.Add("Wafer", typeof(string));
            //datatable.Columns.Add("JO", typeof(string));
            //datatable.Columns.Add("BITable", typeof(string));
            //datatable.Columns.Add("Appv_1", typeof(string));
            //datatable.Columns.Add("Appv_2", typeof(string));
            //datatable.Columns.Add("Appv_3", typeof(string));
            //datatable.Columns.Add("Appv_4", typeof(DateTime));
        }

        private static void StoreBITestResultDateField(List<BITestResultDataField> testresultfieldlist)
        {
            //BITestResultDataField
            var datatable = new System.Data.DataTable();
            PropertyInfo[] properties = typeof(BITestResultDataField).GetProperties();
            var i = 0;
            for (i = 0; i < properties.Length;)
            {
                datatable.Columns.Add(properties[i].Name, properties[i].PropertyType);
                i = i + 1;
            }

            foreach (var df in testresultfieldlist)
            {
                properties = typeof(BITestResultDataField).GetProperties();
                var temprow = new object[properties.Length];
                for (i = 0; i < properties.Length;)
                {
                    temprow[i] = properties[i].GetValue(df);
                    i = i + 1;
                }
                datatable.Rows.Add(temprow);
            }

            WriteDBWithTable(datatable, "BITestResultDataField");

            //datatable.Columns.Add("SN", typeof(string));
            //datatable.Columns.Add("TestName", typeof(string));
            //datatable.Columns.Add("TestTimeStamp", typeof(DateTime));
            //datatable.Columns.Add("PN", typeof(string));
            //datatable.Columns.Add("Wafer", typeof(string));
            //datatable.Columns.Add("JO", typeof(string));
            //datatable.Columns.Add("Channel", typeof(string));
            //datatable.Columns.Add("SLOPE", typeof(double));
            //datatable.Columns.Add("PO_LD", typeof(double));
            //datatable.Columns.Add("PO_Uniformity", typeof(double));
            //datatable.Columns.Add("THOLD", typeof(double));
            //datatable.Columns.Add("Delta_PO_LD", typeof(double));
            //datatable.Columns.Add("Delta_SLOPE", typeof(double));
            //datatable.Columns.Add("Delta_THOLD", typeof(double));
            //datatable.Columns.Add("Delta_PO_Uniformity", typeof(double));
            //datatable.Columns.Add("Appv_1", typeof(double));
            //datatable.Columns.Add("Appv_2", typeof(double));
            //datatable.Columns.Add("Appv_3", typeof(string));
            //datatable.Columns.Add("Appv_4", typeof(string));
            //datatable.Columns.Add("Appv_5", typeof(DateTime));
        }

        private static void WriteDBWithTable(System.Data.DataTable dt,string tablename)
        {
            if (dt.Rows.Count > 0)
            {
                var targetcon = DBUtility.GetLocalConnector();
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(targetcon))
                {
                    bulkCopy.DestinationTableName = tablename;
                    try
                    {
                        for (int i = 0; i < dt.Columns.Count; i++)
                        {
                            bulkCopy.ColumnMappings.Add(dt.Columns[i].ColumnName, dt.Columns[i].ColumnName);
                        }
                        bulkCopy.WriteToServer(dt);
                        dt.Clear();
                    }
                    catch (Exception ex) { }
                    
                }//end using
                DBUtility.CloseConnector(targetcon);
            }//end if
        }


        private static void RealBIWaferBySN(List<string> snlist, Dictionary<string, BISNRelation> snwaferdict, string tab)
        {
            var sncond = "('";
            foreach (var item in snlist)
            {
                    sncond = sncond + item + "','";
            }

            if (sncond.Length > 5)
                sncond = sncond.Substring(0, sncond.Length - 2);
            sncond = sncond + ")";

            var sql = "  select distinct ModuleSerialNum,wafer,productfamilyname from <tab> where ModuleSerialNum in <SNCOND> order by ModuleSerialNum";

            sql = sql.Replace("<SNCOND>", sncond).Replace("<tab>",tab);

            var dbret = DBUtility.ExePRLSqlWithRes(null, sql);
            foreach (var line in dbret)
            {
                var sn = Convert.ToString(line[0]);
                var snitems = new BISNRelation();
                snitems.wafer = Convert.ToString(line[1]);
                snitems.productname = Convert.ToString(line[2]);
                if (!snwaferdict.ContainsKey(sn))
                {
                    snwaferdict.Add(sn, snitems);
                }
            }
        }

        private static void RetrieveBIWaferBySN(List<BITestResult> testresultlist, Dictionary<string, BISNRelation> snwaferdict,string tab)
        {
            var sndict = new Dictionary<string, bool>();
            foreach (var item in testresultlist)
            {
                if (!sndict.ContainsKey(item.SN))
                { sndict.Add(item.SN, true); }
            }
            var snlist = sndict.Keys.ToList();
            var startidx = 0;
            var totalsn = snlist.Count;

            while (true)
            {
                var done = false;
                var tempsnlist = new List<string>();
                for (var idx = startidx; idx < startidx + 20000; idx++)
                {
                    if (idx < totalsn)
                    {
                        tempsnlist.Add(snlist[idx]);
                    }
                    else
                    {
                        done = true;
                        break;
                    }
                }

                if (tempsnlist.Count > 0)
                {
                    RealBIWaferBySN(tempsnlist, snwaferdict, tab);
                }

                if (done)
                    break;

                startidx = startidx + 20000;
            }//end while(true)

        }

        //private static Dictionary<string, BISNRelation> RetrieveBIWaferBySN(List<BITestResult> testresultlist)
        //{
        //    var snwaferdict = new Dictionary<string, BISNRelation>();

        //    var sncond = "('";
        //    var sndict = new Dictionary<string, bool>();
        //    foreach (var item in testresultlist)
        //    {
        //        if (!sndict.ContainsKey(item.SN))
        //        {
        //            sncond = sncond + item.SN + "','";
        //            sndict.Add(item.SN,true);
        //        }
        //    }

        //    if (sncond.Length > 5)
        //        sncond = sncond.Substring(0, sncond.Length - 2);
        //    sncond = sncond + ")";

        //    var sql = "SELECT distinct c.ContainerName as SN,dc.[ParamValueString] as wafer,pb.productname MaterialPN,pf.productfamilyname FROM insite.container c with(nolock)"
        //    + " inner join insite.currentStatus cs(nolock) on c.currentStatusId = cs.currentStatusId "
        //    + "inner join insite.workflowstep ws(nolock) on cs.WorkflowStepId = ws.WorkflowStepId "
        //    + "inner join insite.historyMainline hml with (nolock) on c.containerId = hml.containerId "
        //    + "inner join insite.componentIssueHistory cih with (nolock) on hml.historyMainlineId=cih.historyMainlineId "
        //    + "inner join insite.issueHistoryDetail ihd with (nolock) on cih.componentIssueHistoryId = ihd.componentIssueHistoryId "
        //    + "inner join insite.issueActualsHistory iah with (nolock) on ihd.issueHistoryDetailId = iah.issueHistoryDetailId "
        //    + "inner join insite.container cFrom with (nolock) on iah.fromContainerId = cFrom.containerId "
        //    + "inner join insite.product p with (nolock) on cFrom.productId = p.productId "
        //    + "inner join insite.productBase pb with (nolock) on p.productBaseId  = pb.productBaseId "
        //    + "inner join insite.historyMainline hmll with (nolock)on cFrom.containerId=hmll.historyid "
        //    + "inner join insite.product pp with (nolock) on c.productid=pp.productid "
        //    + "left outer join insite.productfamily pf (nolock) on pp.productFamilyId = pf.productFamilyId "
        //    + "inner join insite.productbase pbb with (nolock) on pp.productbaseid=pbb.productbaseid "
        //    + "inner join[InsiteDB].[insite].[dc_AOC_ManualInspection] dc (nolock) on hmll.[HistoryMainlineId]= dc.[HistoryMainlineId] "
        //    + "where dc.parametername= 'Trace_ID'  and p.description like '%VCSEL%'  and c.containername in <SNCOND> order by c.ContainerName,pb.productname ";

        //    sql = sql.Replace("<SNCOND>", sncond);

        //    var dbret = DBUtility.ExeMESBackupSqlWithRes(null, sql);
        //    foreach (var line in dbret)
        //    {
        //        var sn = Convert.ToString(line[0]);
        //        var snitems = new BISNRelation();
        //        snitems.wafer = Convert.ToString(line[1]);
        //        snitems.productname = Convert.ToString(line[3]);
        //        if (!snwaferdict.ContainsKey(sn))
        //        {
        //            snwaferdict.Add(sn, snitems);
        //        }
        //    }

        //    return snwaferdict;
        //}

        private static string RetrieveLatestTimeStampOfATxpTable(string bt, string bizerotime)
        {
            var sql = "select top 1 TestTimeStamp from ModuleTXOData where TestName = '<TestName>' order by TestTimeStamp DESC";
            sql = sql.Replace("<TestName>", bt);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            if (dbret.Count > 0)
            {
                try
                {
                    return DateTime.Parse(Convert.ToString(dbret[0][0])).ToString();
                }
                catch (Exception ex)
                {
                    return bizerotime;
                }
            }
            else
            {
                return bizerotime;
            }
        }

        private static void RealModuleWaferBySN(List<string> snlist, Dictionary<string, ModuleSNRelation> snwaferdict)
        {
            var sncond = "('";
            foreach (var item in snlist)
            {
                sncond = sncond + item + "','";
            }

            if (sncond.Length > 5)
                sncond = sncond.Substring(0, sncond.Length - 2);
            sncond = sncond + ")";

            var sql = "select distinct SN,Wafer,JO from dbo.BITestResult where SN in <SNCOND> order by SN";
            sql = sql.Replace("<SNCOND>", sncond);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            foreach (var line in dbret)
            {
                var sn = Convert.ToString(line[0]);
                var snitems = new ModuleSNRelation();
                snitems.wafer = Convert.ToString(line[1]);
                snitems.jo = Convert.ToString(line[2]);
                if (!snwaferdict.ContainsKey(sn))
                {
                    snwaferdict.Add(sn, snitems);
                }
            }
        }

        private static void RetrieveModuleWaferBySN(List<ModuleTXOData> txplist, Dictionary<string, ModuleSNRelation> snwaferdict)
        {
            var sndict = new Dictionary<string, bool>();
            foreach (var item in txplist)
            {
                if (!sndict.ContainsKey(item.SN))
                { sndict.Add(item.SN, true); }
            }
            var snlist = sndict.Keys.ToList();
            var startidx = 0;
            var totalsn = snlist.Count;

            while (true)
            {
                var done = false;
                var tempsnlist = new List<string>();
                for (var idx = startidx; idx < startidx + 20000; idx++)
                {
                    if (idx < totalsn)
                    {
                        tempsnlist.Add(snlist[idx]);
                    }
                    else
                    {
                        done = true;
                        break;
                    }
                }

                if (tempsnlist.Count > 0)
                {
                    RealModuleWaferBySN(tempsnlist, snwaferdict);
                }

                if (done)
                    break;

                startidx = startidx + 20000;
            }//end while(true)
        }

        public static void LoadModuleTXOFromMESBackup(Controller ctrl)
        {
            var syscfgdict = CfgUtility.GetSysConfig(ctrl);
            var txptables = syscfgdict["TXPOWERTABLES"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            var txpzerotime = syscfgdict["TXPOWERZEROTIME"];

            foreach (var bt in txptables)
            {
                var txpdatalist = new List<ModuleTXOData>();
                var txplatesttime = RetrieveLatestTimeStampOfATxpTable(bt, txpzerotime);

                var sql = "select  c.moduleserialnum,m.TxPower_dBm,c.TestTimeStamp,m.OvenTemp_C,m.ChannelNumber,c.AssemblyPartNum from insitedb.insite.dc_<dctab> c(nolock) "
                    + " left join insitedb.insite.dce_<dctab>_main m(nolock) on  c.dc_<dctab>HistoryId=m.parenthistoryid "
                    + " where m.TxPower_dBm is not null and c.moduleserialnum is not null and c.TestTimeStamp > '<TestTimeStamp>'";
                sql = sql.Replace("<dctab>", bt).Replace("<TestTimeStamp>", txplatesttime);

                var dbret = DBUtility.ExeMESBackupSqlWithRes(ctrl, sql);
                foreach (var line in dbret)
                {
                    try
                    {
                        var tempdata = new ModuleTXOData();
                        tempdata.SN = Convert.ToString(line[0]);
                        tempdata.TxPower = Convert.ToDouble(line[1]);
                        tempdata.TestTimeStamp = Convert.ToDateTime(line[2]);
                        tempdata.Temperature = Convert.ToDouble(line[3]);
                        tempdata.Channel = Convert.ToString(line[4]);
                        tempdata.PN = Convert.ToString(line[5]);
                        tempdata.TestName = bt;
                        txpdatalist.Add(tempdata);
                    }
                    catch (Exception ex) { }

                }//end foreach

                var snwaferdict = new Dictionary<string, ModuleSNRelation>();
                RetrieveModuleWaferBySN(txpdatalist, snwaferdict);


                foreach (var item in txpdatalist)
                {
                    if (snwaferdict.ContainsKey(item.SN))
                    {
                        item.Wafer = snwaferdict[item.SN].wafer;
                        item.JO = snwaferdict[item.SN].jo;
                    }
                }//end foreach

                StoreModuleTestResult(txpdatalist);
            }//end foreach
        }

        private static void StoreModuleTestResult(List<ModuleTXOData> testresultlist)
        {
            //BITestResult
            var datatable = new System.Data.DataTable();

            PropertyInfo[] properties = typeof(ModuleTXOData).GetProperties();
            var i = 0;
            for (i = 0; i < properties.Length;)
            {
                datatable.Columns.Add(properties[i].Name, properties[i].PropertyType);
                i = i + 1;
            }

            foreach (var testresult in testresultlist)
            {
                properties = typeof(ModuleTXOData).GetProperties();
                var temprow = new object[properties.Length];
                for (i = 0; i < properties.Length;)
                {
                    temprow[i] = properties[i].GetValue(testresult);
                    i = i + 1;
                }
                datatable.Rows.Add(temprow);
            }

            WriteDBWithTable(datatable, "ModuleTXOData");

        }

    }
}