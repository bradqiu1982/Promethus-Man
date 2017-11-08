using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prometheus.Models
{
    public class ISSUETP
    {
        public static string Bug = "Bug";
        public static string RMA = "RMA";
        public static string Task = "Task";
        public static string OBA = "OBA";
        public static string Quality = "Quality";
        public static string NPIPROC = "NPI PROCESS";
        public static string NewFeature = "New Feature";
        public static string Improvement = "Improvement";
        public static string Document = "Document";
        public static string Rel = "Rel";
    }
    public class ISSUESUBTYPE
    {    
        //0: Other 1: Task 2: Critital Failure Task 3: Non Critical Failure Task 
        //4: OBA 5: RMA 6: Bug 7: REL 8: NPI
        public static int Other = 0;
        public static int Task = 1;
        public static int CrititalFailureTask = 2;
        public static int NonCrititalFailureTask = 3;
        public static int OBA = 4;
        public static int RMA = 5;
        public static int Bug = 6;
        public static int REL = 7;
        public static int NPIProcess = 8;
    }

    public class ISSUEATTACHTYPE
    {
        public static string Normal = "Normal";
        public static string InternalRMA = "InternalRMA";
        public static string CustomRMA = "CustomRMA";
    }

    public class ISSUEPR
    {
        public static string Major = "Major";
        public static string Blocker = "Blocker";
        public static string Critical = "Critical";
        public static string Minor = "Minor";
        public static string Trivial = "Trivial";
    }

    public class BIROOTCAUSE
    {
        public static string VCSELISSUE = "VCSEL Issue";
        public static string DATAISSUE = "Data Issue";
        public static string COMMISSUE = "Communication Issue";
        public static string QVLDISSUE = "QVLD Issue";
        public static string PCBAISSUE = "PCBA Issue";
        public static string VMIISSUE = "VMI Issue";
        public static string OTHERISSUE = "Other Issue";
    }

    public class Resolute
    {
        public static string Pending = "Pending";
        public static string Working = "Working";
        public static string Reopen = "Reopen";
        public static string Fixed = "Fixed";
        public static string Done = "Done";
        public static string NotFix = "No Action";
        public static string Unresolved = "Unresolved";
        public static string NotReproduce ="Cannot Reproduce";
        public static string Started = "Started";
        public static string AutoClose = "AutoClose";

        public static string ColorStatus(string status)
        {
            if (string.Compare(status, Pending) == 0 
                || string.Compare(status, Reopen) == 0
                || string.Compare(status, Unresolved) == 0
                || string.Compare(status, NotReproduce) == 0)
            {
                return "<font color = \"red\" ><strong>" +status + "</strong></font>";
            }

            if (string.Compare(status, Working) == 0 || string.Compare(status, Started) == 0)
            {
                return "<font color = \"Orange\" ><strong>" + status + "</strong></font>";
            }

            return "<font color = \"green\" ><strong>" + status + "</strong></font>";
        }

        public static string ColorStatus2(string status)
        {
            if (string.Compare(status, Pending) == 0 || string.Compare(status, Reopen) == 0)
            {
                return "<font color = \"red\" ><strong>" + status + "</strong></font>";
            }

            if (string.Compare(status, Working) == 0 || string.Compare(status, Started) == 0)
            {
                return "<font color = \"Orange\" ><strong> Working </strong></font>";
            }

            return "<font color = \"green\" ><strong> Close </strong></font>";
        }
    }

    public class RMAFAILCODE
    {
        public static string VCSEL = "VCSEL";
        public static string PD = "PD";
        public static string MPD = "MPD";
        public static string Process = "Process";
        public static string Flex = "Flex";
        public static string PCB = "PCB";
        public static string SMT = "SMT";
        public static string Epoxy = "Epoxy";
        public static string Cable = "Cable";
        public static string LaserDriver = "Laser Driver";
        public static string TIA = "TIA";
        public static string CDR = "CDR";
        public static string MCU = "MCU";
        public static string OtherIC = "Other IC";
        public static string Passivecomponent = "Passive component";
        public static string Contamination = "Contamination";
        public static string Mechanical = "Mechanical";
        public static string Firmware = "Firmware";
        public static string WrongEEPROM = "Wrong EEPROM";
        public static string VMI = "VMI";
        public static string Lens = "Lens";
        public static string NTF = "NTF";
        public static string Others = "Others";
        public static string Customerissue = "Customer issue";
    }

    public class COMMENTTYPE
    {
        public static string Description = "Description";
        public static string RootCause = "Root Cause";
        public static string CustomReport = "Report for Customer";
        public static string InternalReport = "Internal Report";
        public static string FailureDetail = "FailureDetail";
        public static string Result = "Result";
        public static string Analysis = "Analysis";
    }

    public class IssueComments
    {
        public string IssueKey { set; get; }

        private string sComment = "";
        public string Comment
        {
            set { sComment = value; }
            get { return sComment; }
        }

        public string CommentType { set; get; }

        public string dbComment
        {
            get
            {
                if (string.IsNullOrEmpty(sComment))
                {
                    return "";
                }
                else
                {
                    try
                    {
                        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(sComment));
                    }
                    catch (Exception)
                    {
                        return "";
                    }
                }

            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    sComment = "";
                }
                else
                {
                    try
                    {
                        sComment = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(value));
                    }
                    catch (Exception)
                    {
                        sComment = "";
                    }

                }

            }
        }

        public string Reporter { set; get; }

        public DateTime CommentDate { set; get; }
    }

    public class IssueViewModels
    {
        public IssueViewModels()
        {
            ModuleSN = "";
            ErrAbbr = "";
            Summary = "";
            ProjectKey = "";
            IssueKey = "";
            IssueType = "";
            Summary = "";
            Priority = "";
            Assignee = "";
            Reporter = "";
            Resolution = "";
            ParentIssueKey = "";
            RelativePeoples = "";
            SharedTag = "";
        }

        public IssueViewModels(string pk,string ik,string it,string sum,string pri,string ddate,string rsdate,string rtdate,string assign,string rt,string resolute,string pik,string rpeople)
        {
            ProjectKey = pk;
            IssueKey = ik;
            IssueType = it;
            Summary = sum;
            Priority = pri;
            DueDate = DateTime.Parse(ddate);
            ResolvedDate = DateTime.Parse(rsdate);
            ReportDate = DateTime.Parse(rtdate);
            Assignee = assign;
            Reporter = rt;
            Resolution = resolute;
            ParentIssueKey = pik;
            RelativePeoples = rpeople;
        }

        public string ProjectKey{ set;get; }

        public string IssueKey { set; get; }

        public string SharedTag { set; get; }

        public string IssueType { set; get; }
        public string Summary { set; get; }
        public string Priority { set; get; }
        public DateTime DueDate { set; get; }
        public DateTime ResolvedDate { set; get; }
        public DateTime ReportDate { set; get; }
        public string Assignee { set; get; }
        public string Reporter { set; get; }
        public string Creator { set; get; }
        public string Resolution { set; get; }

        private string sDescription = "";
        public string Description {
            set { sDescription = value; }
            get { return sDescription; }
        }

        public string dbDescription
        {
            get
            {
                if (string.IsNullOrEmpty(sDescription))
                {
                    return "";
                }
                else
                {
                    try
                    {
                        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(sDescription));
                    }
                    catch (Exception)
                    {
                        return "";
                    }
                }

            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    sDescription = "";
                }
                else
                {
                    try
                    {
                        sDescription = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(value));
                    }
                    catch (Exception)
                    {
                        sDescription = "";
                    }

                }

            }
        }

        public string CommentType { set; get; }


        private List<IssueComments> cemlist = new List<IssueComments>();
        public List<IssueComments> CommentList
        {
            set
            {
                cemlist.Clear();
                cemlist.AddRange(value);

                generalcommentlist.Clear();
                rootcausecommentlist.Clear();
                repot4customercommentlist.Clear();
                internalrepotcommentlist.Clear();
                failuredetailcommentlist.Clear();
                resultcommentlist.Clear();
                analysiscommentlist.Clear();

                foreach (var item in cemlist)
                {
                    if (string.Compare(item.CommentType,COMMENTTYPE.Description) == 0
                        ||string.IsNullOrEmpty(item.CommentType))
                    {
                        generalcommentlist.Add(item);
                    }
                    if (string.Compare(item.CommentType, COMMENTTYPE.RootCause) == 0)
                    {
                        rootcausecommentlist.Add(item);
                    }
                    if (string.Compare(item.CommentType, COMMENTTYPE.CustomReport) == 0)
                    {
                        repot4customercommentlist.Add(item);
                    }
                    if (string.Compare(item.CommentType, COMMENTTYPE.InternalReport) == 0)
                    {
                        internalrepotcommentlist.Add(item);
                    }
                    if (string.Compare(item.CommentType, COMMENTTYPE.FailureDetail) == 0)
                    {
                        failuredetailcommentlist.Add(item);
                    }
                    if (string.Compare(item.CommentType, COMMENTTYPE.Result) == 0)
                    {
                        resultcommentlist.Add(item);
                    }
                    if (string.Compare(item.CommentType, COMMENTTYPE.Analysis) == 0)
                    {
                        analysiscommentlist.Add(item);
                    }
                }
            }
            get
            {
                return cemlist;
            }
        }

        private List<IssueComments> generalcommentlist = new List<IssueComments>();
        private List<IssueComments> rootcausecommentlist = new List<IssueComments>();
        private List<IssueComments> repot4customercommentlist = new List<IssueComments>();
        private List<IssueComments> internalrepotcommentlist = new List<IssueComments>();
        private List<IssueComments> failuredetailcommentlist = new List<IssueComments>();
        private List<IssueComments> resultcommentlist = new List<IssueComments>();
        private List<IssueComments> analysiscommentlist = new List<IssueComments>();

        public List<IssueComments> GeneralCommentList
        {
            get { return generalcommentlist; }
        }

        public List<IssueComments> RootCauseCommentList
        {
            get { return rootcausecommentlist; }
        }

        public List<IssueComments> Report4CustomerCommentList
        {
            get { return repot4customercommentlist; }
        }

        public List<IssueComments> InternalReportCommentList
        {
            get { return internalrepotcommentlist; }
        }

        public List<IssueComments> FailureDetailCommentList
        {
            get { return failuredetailcommentlist; }
        }

        public List<IssueComments> ResultCommentList
        {
            get { return resultcommentlist; }
        }

        public List<IssueComments> AnalysisCommentList
        {
            get { return analysiscommentlist; }
        }

        private List<string> attachlist = new List<string>();
        public List<string> AttachList
        {
            set
            {
                attachlist.Clear();
                attachlist.AddRange(value);
            }
            get
            {
                return attachlist;
            }
        }

        private List<string> internalattachlist = new List<string>();
        public List<string> InternalAttachList
        {
            set
            {
                internalattachlist.Clear();
                internalattachlist.AddRange(value);
            }
            get
            {
                return internalattachlist;
            }
        }

        private List<string> customattachlist = new List<string>();
        public List<string> CustomAttachList
        {
            set
            {
                customattachlist.Clear();
                customattachlist.AddRange(value);
            }
            get
            {
                return customattachlist;
            }
        }


        public string ParentIssueKey { set; get; }

        private List<IssueViewModels> sissue = new List<IssueViewModels>();
        public List<IssueViewModels> SubIssues
        {
            set
            {
                sissue.Clear();
                sissue.AddRange(value);

                containmentactionlist.Clear();
                Correctiveactionlist.Clear();
                failureverifyactionlist.Clear();
                correctivverifyactionlist.Clear();
                generalactionlist.Clear();

                foreach (var item in value)
                {
                    if (item.Summary.Contains("[Containment]")
                        || item.Summary.Contains(RELSubIssueType.CONTAINMENTACTION))
                    {
                        containmentactionlist.Add(item);
                    }
                    else if (item.Summary.Contains("[Corrective]")
                        || item.Summary.Contains(RELSubIssueType.CORRECTIVEACTION))
                    {
                        Correctiveactionlist.Add(item);
                    }
                    else if (item.Summary.Contains(RELSubIssueType.FAILVERIFYACTION))
                    {
                        failureverifyactionlist.Add(item);
                    }
                    else if (item.Summary.Contains(RELSubIssueType.VERIFYCORRECTIVEACTION))
                    {
                        correctivverifyactionlist.Add(item);
                    }
                    else
                    {
                        generalactionlist.Add(item);
                    }
                }//end foreach
            }

            get
            {
                return sissue;
            }
        }


        private List<IssueViewModels> containmentactionlist = new List<IssueViewModels>();
        private List<IssueViewModels> Correctiveactionlist = new List<IssueViewModels>();
        private List<IssueViewModels> failureverifyactionlist = new List<IssueViewModels>();
        private List<IssueViewModels> correctivverifyactionlist = new List<IssueViewModels>();
        private List<IssueViewModels> generalactionlist = new List<IssueViewModels>();

        public List<IssueViewModels> GeneralActions
        {
            get { return generalactionlist; }
        }

        public List<IssueViewModels> ContainmentActions
        {
            get
            { return containmentactionlist; }
        }

        public string ContainmentActionStatus()
        {
            if (containmentactionlist.Count > 0)
            {
                foreach (var item in containmentactionlist)
                {
                    if (item.Resolution == Resolute.Pending || item.Resolution == Resolute.Reopen)
                    {
                        return Resolute.ColorStatus(Resolute.Pending);
                    }
                    if (item.Resolution == Resolute.Working)
                    {
                        return Resolute.ColorStatus(Resolute.Working);
                    }
                    if (item.Resolution == Resolute.NotFix)
                    {
                        return Resolute.ColorStatus(Resolute.NotFix);
                    }
                    if (item.Resolution == Resolute.Unresolved)
                    {
                        return Resolute.ColorStatus(Resolute.Unresolved);
                    }
                    if(item.Resolution == Resolute.NotReproduce)
                    {
                        return Resolute.ColorStatus(Resolute.NotReproduce);
                    }
                }
                return Resolute.ColorStatus(Resolute.Done);
            }
            return Resolute.ColorStatus(Resolute.Pending);
        }

        public List<IssueViewModels> CorrectiveActions
        {
            get
            { return Correctiveactionlist; }
        }

        public string CorrectiveActionStatus()
        {
            if (Correctiveactionlist.Count > 0)
            {
                foreach (var item in Correctiveactionlist)
                {
                    if (item.Resolution == Resolute.Pending || item.Resolution == Resolute.Reopen)
                    {
                        return Resolute.ColorStatus(Resolute.Pending);
                    }
                    if (item.Resolution == Resolute.Working)
                    {
                        return Resolute.ColorStatus(Resolute.Working);
                    }
                    if (item.Resolution == Resolute.NotFix)
                    {
                        return Resolute.ColorStatus(Resolute.NotFix);
                    }
                    if (item.Resolution == Resolute.Unresolved)
                    {
                        return Resolute.ColorStatus(Resolute.Unresolved);
                    }
                    if (item.Resolution == Resolute.NotReproduce)
                    {
                        return Resolute.ColorStatus(Resolute.NotReproduce);
                    }
                }
                return Resolute.ColorStatus(Resolute.Done);
            }
            return Resolute.ColorStatus(Resolute.Pending);
        }

        public List<IssueViewModels> FailureVerifyActions
        {
            get
            { return failureverifyactionlist; }
        }
        public string FVerifyActionStatus()
        {
            if (failureverifyactionlist.Count > 0)
            {
                foreach (var item in failureverifyactionlist)
                {
                    if (item.Resolution == Resolute.Pending || item.Resolution == Resolute.Reopen)
                    {
                        return Resolute.ColorStatus(Resolute.Pending);
                    }
                    if (item.Resolution == Resolute.Working)
                    {
                        return Resolute.ColorStatus(Resolute.Working);
                    }
                    if (item.Resolution == Resolute.NotFix)
                    {
                        return Resolute.ColorStatus(Resolute.NotFix);
                    }
                    if (item.Resolution == Resolute.Unresolved)
                    {
                        return Resolute.ColorStatus(Resolute.Unresolved);
                    }
                    if (item.Resolution == Resolute.NotReproduce)
                    {
                        return Resolute.ColorStatus(Resolute.NotReproduce);
                    }
                }
                return Resolute.ColorStatus(Resolute.Done);
            }
            return Resolute.ColorStatus(Resolute.Pending);
        }
        public List<IssueViewModels> CorrectiveVerifyActions
        {
            get
            { return correctivverifyactionlist; }
        }
        public string CVerifyActionStatus()
        {
            if (correctivverifyactionlist.Count > 0)
            {
                foreach (var item in correctivverifyactionlist)
                {
                    if (item.Resolution == Resolute.Pending || item.Resolution == Resolute.Reopen)
                    {
                        return Resolute.ColorStatus(Resolute.Pending);
                    }
                    if (item.Resolution == Resolute.Working)
                    {
                        return Resolute.ColorStatus(Resolute.Working);
                    }
                    if (item.Resolution == Resolute.NotFix)
                    {
                        return Resolute.ColorStatus(Resolute.NotFix);
                    }
                    if (item.Resolution == Resolute.Unresolved)
                    {
                        return Resolute.ColorStatus(Resolute.Unresolved);
                    }
                    if (item.Resolution == Resolute.NotReproduce)
                    {
                        return Resolute.ColorStatus(Resolute.NotReproduce);
                    }
                }
                return Resolute.ColorStatus(Resolute.Done);
            }
            return Resolute.ColorStatus(Resolute.Pending);
        }

        #region OBA
        public string FinisarDMR { set; get; }
        public string OBAFailureRate { set; get; }
        public string MaterialDisposition { set; get; }
        public string ProductType { set; get; }
        #endregion

        #region Quallity
        public string AffectRange { set; get; }
        #endregion

        #region RMA

        public string FinisarRMA { set; get; }

        public string RMAFailureCode { set; get; }

        public string FVCode { set; get; }

        public string FinisarModel { set; get; }
        public string ModuleSN { set; get; }

        public string ErrAbbr { set; get; }

        public string ECustomer { set; get; }

        public string CRMANUM { set; get; }

        public string CReport { set; get; }

        public string DataID { set; get; }

        private List<string> relatpeople = new List<string>();
        public List<string> RelativePeopleList
        {
            get
            { return relatpeople; }
            set
            {
                relatpeople.Clear();
                relatpeople.AddRange(value);
            }
        }

        public string RelativePeoples
        {
            set
            {
                relatpeople.Clear();
                if (!string.IsNullOrEmpty(value))
                {
                    var temppn = value.Replace("'", "");
                    var temppns = temppn.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var p in temppns)
                    {
                        relatpeople.Add(p.Trim());
                    }
                }
            }

            get
            {
                var ret = "";
                foreach (var p in relatpeople)
                {
                    if (string.IsNullOrEmpty(ret))
                        ret = p;
                    else
                        ret = ret + ";" + p;
                }
                return ret;
            }
        }

        public DateTime AlertEmailUpdateDate { set; get; }
        #endregion

        #region Rel

        public string QualType { set; get; }
        public string RequestID { set; get; }
        public string LineCategory { set; get; }
        public string TestType { set; get; }
        public string FailureInterval { set; get; }
        public string FailQTY { set; get; }
        public string TotalQTY { set; get; }
        public string Location { set; get; }
        public string CaseID { set; get; }
        public string TestFailure { set; get; }

        #endregion

        public string LYT { set; get; }


        public static string GetUniqKey()
        {
            return Guid.NewGuid().ToString("N");
        }

        public bool IssueClosed()
        {
            if(string.Compare(Resolution, Resolute.Pending) != 0 
                && string.Compare(Resolution, Resolute.Working) != 0
                && string.Compare(Resolution, Resolute.Reopen) != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void UpdateSummary(string Summary)
        {
            var sql = "update Issue set Summary = '<Summary>' where IssueKey = '<IssueKey>'";
            sql = sql.Replace("<Summary>", Summary).Replace("<IssueKey>", IssueKey);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public void StoreIssue()
        {
            var sql = "insert into Issue(ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,Creator,AlertEmailUpdateDate,RelativePeoples,ModuleSN,ErrAbbr,DataID,databackuptm) values('<ProjectKey>','<IssueKey>','<IssueType>',N'<Summary>','<Priority>','<DueDate>','<ResolvedDate>','<ReportDate>','<Assignee>','<Reporter>','<Resolution>','<Creator>','<AlertEmailUpdateDate>','<RelativePeoples>',N'<ModuleSN>','<ErrAbbr>','<DataID>','<databackuptm>')";
            sql = sql.Replace("<ProjectKey>", ProjectKey).Replace("<IssueKey>", IssueKey).Replace("<IssueType>", IssueType)
                .Replace("<Summary>", Summary).Replace("<Priority>", Priority).Replace("<DueDate>", DueDate.ToString())
                .Replace("<ResolvedDate>", ResolvedDate.ToString()).Replace("<ReportDate>", ReportDate.ToString()).Replace("<Assignee>", Assignee)
                .Replace("<Reporter>", Reporter).Replace("<Resolution>", Resolution).Replace("<Creator>", Reporter)
                .Replace("<AlertEmailUpdateDate>", DateTime.Now.AddHours(-25).ToString()).Replace("<RelativePeoples>", RelativePeoples)
                .Replace("<ModuleSN>", ModuleSN).Replace("<ErrAbbr>", ErrAbbr).Replace("<DataID>", DataID).Replace("<databackuptm>", DateTime.Now.ToString());
            DBUtility.ExeLocalSqlNoRes(sql);

            StoreIssueComment(DateTime.Now.ToString());

            if (string.Compare(IssueType, ISSUETP.RMA) == 0)
            {
                StoreRMA();
            }

            if (string.Compare(IssueType, ISSUETP.OBA) == 0)
            {
                StoreOBA();
            }

            if (string.Compare(IssueType, ISSUETP.Quality) == 0)
            {
                StoreQuality();
            }

            if (string.Compare(IssueType, ISSUETP.Rel) == 0)
            {
                StoreReliability();
            }
        }

        private void StoreOBA()
        {
            var sql = "insert into IssueOBA(IssueKey,FinisarDMR,OBAFailureRate,MaterialDisposition,ModuleSN,FVCode,APVal1,databackuptm) values('<IssueKey>','<FinisarDMR>','<OBAFailureRate>','<MaterialDisposition>',N'<ModuleSN>','<FVCode>','<ProductType>','<databackuptm>')";
            sql = sql.Replace("<IssueKey>", IssueKey).Replace("<FinisarDMR>", FinisarDMR).Replace("<OBAFailureRate>", OBAFailureRate).Replace("<FVCode>", FVCode)
                .Replace("<MaterialDisposition>", MaterialDisposition).Replace("<ModuleSN>", ModuleSN).Replace("<ProductType>", ProductType).Replace("<databackuptm>", DateTime.Now.ToString());
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        private void StoreQuality()
        {
            var sql = "insert into IssueAttribute(IssueKey,APVal1,APVal2,APVal3,APVal4,databackuptm) values('<IssueKey>','<RMAFailureCode>','<ProductType>','<AffectRange>','<MaterialDisposition>','<databackuptm>')";
            sql = sql.Replace("<IssueKey>", IssueKey).Replace("<RMAFailureCode>", RMAFailureCode).Replace("<ProductType>", ProductType).Replace("<AffectRange>", AffectRange)
                .Replace("<MaterialDisposition>", MaterialDisposition).Replace("<databackuptm>", DateTime.Now.ToString());
            DBUtility.ExeLocalSqlNoRes(sql);
        }


        //    vm.FVCode = "";
        //    vm.ReportDate = DateTime.Parse(rawdata.AppV_C);
        //    vm.CaseID = rawdata.AppV_A.ToString();
        //    vm.ProductType = rawdata.AppV_H;
        //    vm.LineCategory = rawdata.AppV_G;
        //    vm.QualType = rawdata.AppV_E;
        //    vm.TestType = rawdata.AppV_I;
        //    vm.FailureInterval = rawdata.AppV_J;
        //    vm.TestFailure = rawdata.AppV_P;
        //    vm.Location = rawdata.AppV_U;
        //    vm.RequestID = rawdata.AppV_F;

        private void StoreReliability()
        {
            var sql = "insert into IssueAttribute(IssueKey,APVal1,APVal2,APVal3,APVal4,APVal5,APVal6,APVal7,APVal8,APVal9,APVal10,APVal11,APVal12,databackuptm) "
                + " values('<IssueKey>','<CaseID>',N'<ProductType>',N'<LineCategory>',N'<QualType>',N'<TestType>',N'<FailureInterval>',N'<TestFailure>',N'<Location>',N'<RequestID>',N'<FailQTY>',N'<TotalQTY>',N'<FVCode>','<databackuptm>')";

            sql = sql.Replace("<IssueKey>", IssueKey).Replace("<CaseID>", CaseID).Replace("<ProductType>", ProductType).Replace("<LineCategory>", LineCategory)
                .Replace("<QualType>", QualType).Replace("<TestType>", TestType).Replace("<FailureInterval>", FailureInterval).Replace("<TestFailure>", TestFailure)
                .Replace("<Location>", Location).Replace("<RequestID>", RequestID).Replace("<FailQTY>", FailQTY).Replace("<TotalQTY>", TotalQTY).Replace("<FVCode>", FVCode).Replace("<databackuptm>", DateTime.Now.ToString());

            DBUtility.ExeLocalSqlNoRes(sql);
        }

        private void RetrieveOBA()
        {
            var sql = "select FinisarDMR,OBAFailureRate,MaterialDisposition,ModuleSN,FVCode,APVal1 from IssueOBA where IssueKey = '<IssueKey>'";
            sql = sql.Replace("<IssueKey>", IssueKey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            if (dbret.Count > 0)
            {
                FinisarDMR = Convert.ToString(dbret[0][0]);
                OBAFailureRate = Convert.ToString(dbret[0][1]);
                MaterialDisposition = Convert.ToString(dbret[0][2]);
                ModuleSN = Convert.ToString(dbret[0][3]);
                FVCode = Convert.ToString(dbret[0][4]);
                ProductType = Convert.ToString(dbret[0][5]);
            }
        }

        private void RetrieveQuality()
        {
            var sql = "select APVal1,APVal2,APVal3,APVal4 from IssueAttribute where IssueKey = '<IssueKey>'";
            sql = sql.Replace("<IssueKey>", IssueKey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            if (dbret.Count > 0)
            {
                RMAFailureCode = Convert.ToString(dbret[0][0]);
                ProductType = Convert.ToString(dbret[0][1]);
                AffectRange = Convert.ToString(dbret[0][2]);
                MaterialDisposition = Convert.ToString(dbret[0][3]);
            }
        }

        //var sql = "insert into IssueAttribute(IssueKey,APVal1,APVal2,APVal3,APVal4,APVal5,APVal6,APVal7,APVal8,APVal9,APVal10,APVal11,APVal12) "
        //        + " values('<IssueKey>','<CaseID>',N'<ProductType>',N'<LineCategory>',N'<QualType>',N'<TestType>',N'<FailureInterval>',N'<TestFailure>',N'<Location>',N'<RequestID>',N'<FailQTY>',N'<TotalQTY>',N'<FVCode>')";
        private void RetrieveReliability()
        {
            var sql = "select APVal1,APVal2,APVal3,APVal4,APVal5,APVal6,APVal7,APVal8,APVal9,APVal10,APVal11,APVal12 from IssueAttribute where IssueKey = '<IssueKey>'";
            sql = sql.Replace("<IssueKey>", IssueKey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            if (dbret.Count > 0)
            {
                CaseID = Convert.ToString(dbret[0][0]);
                ProductType = Convert.ToString(dbret[0][1]);
                LineCategory = Convert.ToString(dbret[0][2]);
                QualType = Convert.ToString(dbret[0][3]);
                TestType = Convert.ToString(dbret[0][4]);
                FailureInterval = Convert.ToString(dbret[0][5]);
                TestFailure = Convert.ToString(dbret[0][6]);
                Location = Convert.ToString(dbret[0][7]);
                RequestID = Convert.ToString(dbret[0][8]);
                FailQTY = Convert.ToString(dbret[0][9]);
                TotalQTY = Convert.ToString(dbret[0][10]);
                FVCode = Convert.ToString(dbret[0][11]);
            }
        }

        private void StoreRMA()
        {

            var sql = "insert into IssueRMA(IssueKey,FinisarRMA,FinisarModel,ECustomer,CRMANUM,CReport,ModuleSN,RMAFailureCode,FVCode,databackuptm) values('<IssueKey>','<FinisarRMA>','<FinisarModel>','<ECustomer>','<CRMANUM>','<CReport>',N'<ModuleSN>','<RMAFailureCode>','<FVCode>','<databackuptm>')";
            sql = sql.Replace("<IssueKey>", IssueKey).Replace("<FinisarRMA>", FinisarRMA).Replace("<RMAFailureCode>", RMAFailureCode).Replace("<FVCode>", FVCode)
                .Replace("<FinisarModel>", FinisarModel).Replace("<ECustomer>", ECustomer)
                .Replace("<CRMANUM>", CRMANUM).Replace("<CReport>", CReport).Replace("<ModuleSN>", ModuleSN).Replace("<databackuptm>", DateTime.Now.ToString());
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        private void RetrieveRMA()
        {
            var sql = "select FinisarRMA,FinisarModel,ECustomer,CRMANUM,CReport,ModuleSN,RMAFailureCode,FVCode from IssueRMA where IssueKey = '<IssueKey>'";
            sql = sql.Replace("<IssueKey>", IssueKey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            if (dbret.Count > 0)
            {
                FinisarRMA = Convert.ToString(dbret[0][0]);
                FinisarModel = Convert.ToString(dbret[0][1]);
                ECustomer = Convert.ToString(dbret[0][2]);
                CRMANUM = Convert.ToString(dbret[0][3]);
                CReport = Convert.ToString(dbret[0][4]);
                ModuleSN = Convert.ToString(dbret[0][5]);
                RMAFailureCode = Convert.ToString(dbret[0][6]);
                FVCode = Convert.ToString(dbret[0][7]);
            }
        }

        public static void RemoveIssue(string issuekey, Controller ctrl)
        {
            try
            {
                var subissues = RetrieveSubIssue(issuekey,ctrl);
                var sql = "";

                foreach (var item in subissues)
                {
                    sql = "update Issue  set APVal1 = 'delete' where IssueKey = '<IssueKey>'";
                    sql = sql.Replace("<IssueKey>", item.IssueKey);
                    DBUtility.ExeLocalSqlNoRes(sql);

                }

                sql = "update Issue  set APVal1 = 'delete' where IssueKey = '<IssueKey>'";
                sql = sql.Replace("<IssueKey>", issuekey);
                DBUtility.ExeLocalSqlNoRes(sql);

            }
            catch (Exception ex)
            {

            }

        }

        public void StoreSubIssue()
        {
            var sql = "insert into Issue(ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,ParentIssueKey,Creator,AlertEmailUpdateDate,RelativePeoples,databackuptm) values('<ProjectKey>','<IssueKey>','<IssueType>',N'<Summary>','<Priority>','<DueDate>','<ResolvedDate>','<ReportDate>','<Assignee>','<Reporter>','<Resolution>','<ParentIssueKey>','<Creator>','<AlertEmailUpdateDate>','<RelativePeoples>','<databackuptm>')";
            sql = sql.Replace("<ProjectKey>", ProjectKey).Replace("<IssueKey>", IssueKey).Replace("<IssueType>", IssueType)
                .Replace("<Summary>", Summary).Replace("<Priority>", Priority).Replace("<DueDate>", DueDate.ToString())
                .Replace("<ResolvedDate>", ResolvedDate.ToString()).Replace("<ReportDate>", ReportDate.ToString()).Replace("<Assignee>", Assignee)
                .Replace("<Reporter>", Reporter).Replace("<Resolution>", Resolution).Replace("<ParentIssueKey>", ParentIssueKey).Replace("<Creator>", Reporter)
                .Replace("<AlertEmailUpdateDate>", DateTime.Now.AddHours(-6).ToString()).Replace("<RelativePeoples>", RelativePeoples).Replace("<databackuptm>", DateTime.Now.ToString());
            DBUtility.ExeLocalSqlNoRes(sql);

            StoreIssueComment(DateTime.Now.ToString());
        }

        private void StoreIssueComment(string CommentDate)
        {
            if (!string.IsNullOrEmpty(Description))
            {
                if (string.IsNullOrEmpty(CommentType))
                {
                    CommentType = COMMENTTYPE.Description;
                }

                var sql = "insert into IssueComments(IssueKey,Comment,Reporter,CommentDate,CommentType,databackuptm) values('<IssueKey>','<Comment>','<Reporter>','<CommentDate>','<CommentType>','<databackuptm>')";
                sql = sql.Replace("<IssueKey>", IssueKey).Replace("<Comment>", dbDescription)
                    .Replace("<Reporter>", Reporter).Replace("<CommentDate>", CommentDate).Replace("<CommentType>", CommentType).Replace("<databackuptm>", DateTime.Now.ToString());
                DBUtility.ExeLocalSqlNoRes(sql);
            }
        }

        public static void StoreIssueComment(string isk,string desc,string rept,string cmtype)
        {
            var sql = "insert into IssueComments(IssueKey,Comment,Reporter,CommentDate,CommentType,databackuptm) values('<IssueKey>','<Comment>','<Reporter>','<CommentDate>','<CommentType>','<databackuptm>')";
            sql = sql.Replace("<IssueKey>", isk).Replace("<Comment>", desc).Replace("<Reporter>", rept)
                .Replace("<CommentDate>", DateTime.Now.ToString()).Replace("<CommentType>", cmtype).Replace("<databackuptm>", DateTime.Now.ToString());
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        

        public static void StoreIssueAttachment(string issuekey,string attachmenturl,string attachtype = "Normal")
        {
            var sql = "insert into IssueAttachment(IssueKey,Attachment,UpdateTime,APVal1,databackuptm) values('<IssueKey>',N'<Attachment>','<UpdateTime>','<attachtype>','<databackuptm>')";
            sql = sql.Replace("<IssueKey>", issuekey).Replace("<Attachment>", attachmenturl).Replace("<UpdateTime>", DateTime.Now.ToString()).Replace("<attachtype>", attachtype).Replace("<databackuptm>", DateTime.Now.ToString());
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public void RetrieveAttachment(string issuekey)
        {
            var csql = "select Attachment,APVal1 from IssueAttachment where IssueKey = '<IssueKey>' order by UpdateTime ASC";
            csql = csql.Replace("<IssueKey>", issuekey);

            AttachList.Clear();
            InternalAttachList.Clear();
            CustomAttachList.Clear();

            var cdbret = DBUtility.ExeLocalSqlWithRes(csql,null);
            foreach (var r in cdbret)
            {
                var type = Convert.ToString(r[1]);
                
                if (string.IsNullOrEmpty(type))
                {
                    AttachList.Add(Convert.ToString(r[0]));
                }
                else if (string.Compare(type,ISSUEATTACHTYPE.Normal) == 0)
                {
                    AttachList.Add(Convert.ToString(r[0]));
                }
                else if (string.Compare(type, ISSUEATTACHTYPE.InternalRMA) == 0)
                {
                    InternalAttachList.Add(Convert.ToString(r[0]));
                }
                else if (string.Compare(type, ISSUEATTACHTYPE.CustomRMA) == 0)
                {
                    CustomAttachList.Add(Convert.ToString(r[0]));
                }
                else
                {
                    AttachList.Add(Convert.ToString(r[0]));
                }
            }
        }

        //public static void RepairAttachUrl()
        //{
        //    //IssueComments(IssueKey, Comment, Reporter, CommentDate, CommentType)
        //    var sql = "select IssueKey,Comment,Reporter,CommentDate,CommentType from IssueComments where CommentType = '<CommentType2>'";
        //    sql = sql.Replace("<CommentType2>", COMMENTTYPE.CustomReport);
        //    var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
        //    foreach (var r in dbret)
        //    {
        //        var tempcomment = new IssueComments();
        //        tempcomment.IssueKey = Convert.ToString(r[0]);
        //        tempcomment.dbComment = Convert.ToString(r[1]);
        //        tempcomment.Reporter = Convert.ToString(r[2]);
        //        tempcomment.CommentDate = DateTime.Parse(Convert.ToString(r[3]));
        //        tempcomment.CommentType = Convert.ToString(r[4]);

        //        if (tempcomment.Comment.Contains("<p><a href=\"")
        //            && tempcomment.Comment.Contains("\" target"))
        //        {
        //            var attachtype = ISSUEATTACHTYPE.CustomRMA;
        //            var splitstr = tempcomment.Comment.Split(new string[] { "<a href=\"" }, StringSplitOptions.RemoveEmptyEntries);

        //            var url = splitstr[1].Split(new string[] { "\" target" }, StringSplitOptions.RemoveEmptyEntries)[0];
        //            var csql = "insert into IssueAttachment (IssueKey,Attachment,UpdateTime,APVal1) values('<IssueKey>',N'<Attachment>','<UpdateTime>','<attachtype>')";
        //            csql = csql.Replace("<IssueKey>", tempcomment.IssueKey).Replace("<Attachment>", url).Replace("<UpdateTime>", tempcomment.CommentDate.ToString()).Replace("<attachtype>", attachtype);
        //            DBUtility.ExeLocalSqlNoRes(csql);
        //        }
        //    }
        //}

        public void UpdateIssue()
        {
            var sql = "update Issue set IssueType = '<IssueType>',Priority = '<Priority>',DueDate = '<DueDate>', Assignee = '<Assignee>',Resolution = '<Resolution>',RelativePeoples='<RelativePeoples>' where IssueKey = '<IssueKey>'";
            sql = sql.Replace("<IssueKey>", IssueKey).Replace("<IssueType>", IssueType)
                .Replace("<Priority>", Priority).Replace("<DueDate>", DueDate.ToString())
                .Replace("<Assignee>", Assignee).Replace("<Resolution>", Resolution).Replace("<RelativePeoples>", RelativePeoples);
            DBUtility.ExeLocalSqlNoRes(sql);

            StoreIssueComment(DateTime.Now.ToString());
        }

        public void UpdateRMA()
        {
            var sql = "update Issue set Priority = '<Priority>',DueDate = '<DueDate>', Assignee = '<Assignee>',Resolution = '<Resolution>',RelativePeoples = '<RelativePeoples>' where IssueKey = '<IssueKey>'";
            sql = sql.Replace("<IssueKey>", IssueKey).Replace("<Priority>", Priority)
                .Replace("<DueDate>", DueDate.ToString()).Replace("<Assignee>", Assignee)
                .Replace("<Resolution>", Resolution).Replace("<RelativePeoples>", RelativePeoples);
            DBUtility.ExeLocalSqlNoRes(sql);

            StoreIssueComment(DateTime.Now.ToString());

            UpdateRMAInfo();
        }

        private void UpdateRMAInfo()
        {
            var sql = "update IssueRMA set CRMANUM = '<CRMANUM>',CReport = '<CReport>',ModuleSN = '<ModuleSN>',RMAFailureCode = '<RMAFailureCode>',FVCode = '<FVCode>'  where IssueKey = '<IssueKey>'";
            sql = sql.Replace("<IssueKey>", IssueKey).Replace("<CRMANUM>", CRMANUM)
                .Replace("<CReport>", CReport).Replace("<ModuleSN>", ModuleSN).Replace("<RMAFailureCode>", RMAFailureCode).Replace("<FVCode>", FVCode);
            DBUtility.ExeLocalSqlNoRes(sql);
        }


        public void UpdateRel()
        {
            var sql = "update Issue set Priority = '<Priority>',DueDate = '<DueDate>', Assignee = '<Assignee>',Resolution = '<Resolution>',RelativePeoples = '<RelativePeoples>' where IssueKey = '<IssueKey>'";
            sql = sql.Replace("<IssueKey>", IssueKey).Replace("<Priority>", Priority)
                .Replace("<DueDate>", DueDate.ToString()).Replace("<Assignee>", Assignee)
                .Replace("<Resolution>", Resolution).Replace("<RelativePeoples>", RelativePeoples);
            DBUtility.ExeLocalSqlNoRes(sql);

            StoreIssueComment(DateTime.Now.ToString());

            UpdateRelInfo();
        }

        private void UpdateRelInfo()
        {
            //var sql = "insert into IssueAttribute(IssueKey,APVal1,APVal2,APVal3,APVal4,APVal5,APVal6,APVal7,APVal8,APVal9,APVal10,APVal11,APVal12) "
            //    + " values('<IssueKey>','<CaseID>',N'<ProductType>',N'<LineCategory>',N'<QualType>',N'<TestType>',N'<FailureInterval>',N'<TestFailure>',N'<Location>',N'<RequestID>',N'<FailQTY>',N'<TotalQTY>',N'<FVCode>')";

            var sql = "update IssueAttribute set APVal12 = N'<FVCode>' where IssueKey = '<IssueKey>'";
            sql = sql.Replace("<FVCode>", FVCode).Replace("<IssueKey>", IssueKey);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public void UpdateOBA()
        {
            var sql = "update Issue set Priority = '<Priority>',DueDate = '<DueDate>', Assignee = '<Assignee>',Resolution = '<Resolution>',RelativePeoples = '<RelativePeoples>' where IssueKey = '<IssueKey>'";
            sql = sql.Replace("<IssueKey>", IssueKey).Replace("<Priority>", Priority)
                .Replace("<DueDate>", DueDate.ToString()).Replace("<Assignee>", Assignee)
                .Replace("<Resolution>", Resolution).Replace("<RelativePeoples>", RelativePeoples);
            DBUtility.ExeLocalSqlNoRes(sql);

            StoreIssueComment(DateTime.Now.ToString());

            UpdateOBAInfo();
        }

        private void UpdateOBAInfo()
        {//ssueKey,FinisarDMR,OBAFailureRate,MaterialDisposition,ModuleSN,FVCode
            var sql = "update IssueOBA set FinisarDMR = '<FinisarDMR>',OBAFailureRate = '<OBAFailureRate>',MaterialDisposition = '<MaterialDisposition>',FVCode = '<FVCode>',APVal1='<ProductType>'  where IssueKey = '<IssueKey>'";
            sql = sql.Replace("<IssueKey>", IssueKey).Replace("<FinisarDMR>", FinisarDMR)
                .Replace("<OBAFailureRate>", OBAFailureRate).Replace("<MaterialDisposition>", MaterialDisposition).Replace("<FVCode>", FVCode).Replace("<ProductType>", ProductType);
            DBUtility.ExeLocalSqlNoRes(sql);
        }


        public void UpdateQuality()
        {
            var sql = "update Issue set Priority = '<Priority>',DueDate = '<DueDate>', Assignee = '<Assignee>',Resolution = '<Resolution>',RelativePeoples = '<RelativePeoples>' where IssueKey = '<IssueKey>'";
            sql = sql.Replace("<IssueKey>", IssueKey).Replace("<Priority>", Priority)
                .Replace("<DueDate>", DueDate.ToString()).Replace("<Assignee>", Assignee)
                .Replace("<Resolution>", Resolution).Replace("<RelativePeoples>", RelativePeoples);
            DBUtility.ExeLocalSqlNoRes(sql);

            StoreIssueComment(DateTime.Now.ToString());

            UpdateQualitynfo();
        }

        private void UpdateQualitynfo()
        {//IssueAttribute(IssueKey,APVal1,APVal2,APVal3,APVal4) values('<IssueKey>','<RMAFailureCode>','<ProductType>','<AffectRange>','<MaterialDisposition>'
            var sql = "update IssueAttribute set APVal1 = '<RMAFailureCode>',APVal2 = '<ProductType>',APVal3 = '<AffectRange>',APVal4 = '<MaterialDisposition>' where IssueKey = '<IssueKey>'";
            sql = sql.Replace("<IssueKey>", IssueKey).Replace("<RMAFailureCode>", RMAFailureCode)
                .Replace("<ProductType>", ProductType).Replace("<MaterialDisposition>", MaterialDisposition).Replace("<AffectRange>", AffectRange);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public void UpdateIAssign()
        {
            var sql =  "update Issue set Reporter = '<Reporter>' where IssueKey = '<IssueKey>'";
            sql = sql.Replace("<IssueKey>", IssueKey).Replace("<Reporter>", Reporter);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public void UpdateAlertEmailDate()
        {
            var sql = "update Issue set AlertEmailUpdateDate = '<AlertEmailUpdateDate>' where IssueKey = '<IssueKey>'";
            sql = sql.Replace("<IssueKey>", IssueKey).Replace("<AlertEmailUpdateDate>", DateTime.Now.ToString());
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public string RetrieveAlertEmailDate(string issuekey)
        {
            var sql = "select AlertEmailUpdateDate from Issue where IssueKey = '<IssueKey>'";
            sql = sql.Replace("<IssueKey>", issuekey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            if (dbret.Count > 0)
            {
                return Convert.ToString(dbret[0][0]);
            }
            else
                return string.Empty;
        }

        public void CloseIssue()
        {
            var sql = "update Issue set ResolvedDate = '<ResolvedDate>' where IssueKey = '<IssueKey>'";
            sql = sql.Replace("<IssueKey>", IssueKey).Replace("<ResolvedDate>", DateTime.Now.ToString());
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static List<IssueViewModels> RetrieveSubIssue(string parentkey, Controller ctrl)
        {
            var ret = new List<IssueViewModels>();
            var sql = "select ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples,ModuleSN from Issue where APVal1 <> 'delete' and ParentIssueKey = '<ParentIssueKey>'";
            sql = sql.Replace("<ParentIssueKey>", parentkey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            foreach (var line in dbret)
            {
                var tempvm = new IssueViewModels(Convert.ToString(line[0])
                    , Convert.ToString(line[1]), Convert.ToString(line[2])
                    , Convert.ToString(line[3]), Convert.ToString(line[4])
                    , Convert.ToString(line[5]), Convert.ToString(line[6])
                    , Convert.ToString(line[7]), Convert.ToString(line[8])
                    , Convert.ToString(line[9]), Convert.ToString(line[10]), parentkey, Convert.ToString(line[11]));
                tempvm.ModuleSN = Convert.ToString(line[12]);

                tempvm.RetrieveComment(ctrl);
                ret.Add(tempvm);
            }
            return ret;
        }

        public static List<IssueViewModels> RetrieveSptIssue(Controller ctrl,string ProjectKey)
        {
            var sql = string.Empty;
            if (string.IsNullOrEmpty(ProjectKey))
            {
                sql = "select ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,ParentIssueKey,RelativePeoples,APVal2,ErrAbbr,Creator,ModuleSN from Issue where APVal1 <> 'delete' and (Summary like '%<cond1>%' or Summary like '%<cond11>%' or Summary like '%<cond2>%' or Summary like '%<cond22>%') and  ParentIssueKey = '' order by Resolution DESC,ReportDate DESC,ProjectKey";
                sql = sql.Replace("<cond1>", CRITICALERRORTYPE.LYTTASK1.Replace("[","").Replace("]", "")).Replace("<cond2>",CRITICALERRORTYPE.SECONDMATCH1.Replace("[", "").Replace("]", ""))
                .Replace("<cond11>", CRITICALERRORTYPE.LYTTASK.Replace("[", "").Replace("]", "")).Replace("<cond22>", CRITICALERRORTYPE.SECONDMATCH.Replace("[", "").Replace("]", ""));
            }
            else
            {
                sql = "select ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,ParentIssueKey,RelativePeoples,APVal2,ErrAbbr,Creator,ModuleSN from Issue where APVal1 <> 'delete' and (Summary like '%<cond1>%' or Summary like '%<cond11>%' or Summary like '%<cond2>%' or Summary like '%<cond22>%') and ProjectKey = '<ProjectKey>' and  ParentIssueKey = '' order by Resolution DESC,ReportDate DESC";
                sql = sql.Replace("<cond1>", CRITICALERRORTYPE.LYTTASK1.Replace("[", "").Replace("]", "")).Replace("<cond2>", CRITICALERRORTYPE.SECONDMATCH1.Replace("[", "").Replace("]", ""))
                .Replace("<cond11>", CRITICALERRORTYPE.LYTTASK.Replace("[", "").Replace("]", "")).Replace("<cond22>", CRITICALERRORTYPE.SECONDMATCH.Replace("[", "").Replace("]", "")).Replace("<ProjectKey>", ProjectKey);
            }

            var ret = new List<IssueViewModels>();

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var tempret = new IssueViewModels(Convert.ToString(line[0])
                    , Convert.ToString(line[1]), Convert.ToString(line[2])
                    , Convert.ToString(line[3]), Convert.ToString(line[4])
                    , Convert.ToString(line[5]), Convert.ToString(line[6])
                    , Convert.ToString(line[7]), Convert.ToString(line[8])
                    , Convert.ToString(line[9]), Convert.ToString(line[10])
                    , Convert.ToString(line[11]), Convert.ToString(line[12]));
                tempret.LYT = Convert.ToString(line[13]);
                tempret.ErrAbbr = Convert.ToString(line[14]);
                tempret.Creator = Convert.ToString(line[15]);
                tempret.ModuleSN = Convert.ToString(line[16]);
                tempret.RetrieveComment(ctrl);
                tempret.RetrieveAttachment(tempret.IssueKey);
                tempret.SharedTag = ShareDocVM.RetrieveSharedDocTag(tempret.IssueKey);
                ret.Add(tempret);
            }

            return ret;
        }

        public static List<IssueViewModels> RetrievePMTask(string pjkey,string resolution, Controller ctrl)
        {
            var sql = string.Empty;
            var ret = new List<IssueViewModels>();

            if (string.Compare(resolution, Resolute.Pending, true) == 0)
            {
                sql = "select ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,ParentIssueKey,RelativePeoples,APVal2,ErrAbbr,Creator,ModuleSN from Issue where APVal1 <> 'delete' and Summary like '%<cond1>%' and ProjectKey= '<ProjectKey>' and Resolution = '<Resolution>' order by DueDate ASC";
                sql = sql.Replace("<cond1>", CRITICALERRORTYPE.PMTASK.Replace("[", "").Replace("]", "")).Replace("<ProjectKey>", pjkey).Replace("<Resolution>", Resolute.Pending);
            }
            if (string.Compare(resolution, Resolute.Working, true) == 0)
            {
                sql = "select ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,ParentIssueKey,RelativePeoples,APVal2,ErrAbbr,Creator,ModuleSN from Issue where APVal1 <> 'delete' and Summary like '%<cond1>%' and ProjectKey= '<ProjectKey>' and (Resolution = '<Resolution1>' or Resolution = '<Resolution2>') order by DueDate ASC";
                sql = sql.Replace("<cond1>", CRITICALERRORTYPE.PMTASK.Replace("[", "").Replace("]", "")).Replace("<ProjectKey>", pjkey).Replace("<Resolution1>", Resolute.Working).Replace("<Resolution2>", Resolute.Reopen);
            }
            if (string.Compare(resolution, Resolute.Done, true) == 0)
            {
                sql = "select top 20 ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,ParentIssueKey,RelativePeoples,APVal2,ErrAbbr,Creator,ModuleSN from Issue where APVal1 <> 'delete' and Summary like '%<cond1>%' and ProjectKey= '<ProjectKey>' and (Resolution = '<Resolution1>' or Resolution = '<Resolution2>') order by ReportDate DESC";
                sql = sql.Replace("<cond1>", CRITICALERRORTYPE.PMTASK.Replace("[", "").Replace("]", "")).Replace("<ProjectKey>", pjkey).Replace("<Resolution1>", Resolute.Fixed).Replace("<Resolution2>", Resolute.Done);
            }

            if (!string.IsNullOrEmpty(sql))
            {
                var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
                foreach (var line in dbret)
                {
                    var tempret = new IssueViewModels(Convert.ToString(line[0])
                        , Convert.ToString(line[1]), Convert.ToString(line[2])
                        , Convert.ToString(line[3]), Convert.ToString(line[4])
                        , Convert.ToString(line[5]), Convert.ToString(line[6])
                        , Convert.ToString(line[7]), Convert.ToString(line[8])
                        , Convert.ToString(line[9]), Convert.ToString(line[10])
                        , Convert.ToString(line[11]), Convert.ToString(line[12]));
                    tempret.LYT = Convert.ToString(line[13]);
                    tempret.ErrAbbr = Convert.ToString(line[14]);
                    tempret.Creator = Convert.ToString(line[15]);
                    tempret.ModuleSN = Convert.ToString(line[16]);
                    tempret.RetrieveComment(ctrl);
                    ret.Add(tempret);
                }
            }
            return ret;
        }

        public static IssueViewModels RetrieveIssueByIssueKey(string issuekey, Controller ctrl)
        {
            var sql = "select ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,ParentIssueKey,RelativePeoples,APVal2,ErrAbbr,Creator,ModuleSN from Issue where APVal1 <> 'delete' and IssueKey = '<IssueKey>'";
            sql = sql.Replace("<IssueKey>", issuekey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            if (dbret.Count > 0)
            {
                var ret = new IssueViewModels(Convert.ToString(dbret[0][0])
                    , Convert.ToString(dbret[0][1]), Convert.ToString(dbret[0][2])
                    , Convert.ToString(dbret[0][3]), Convert.ToString(dbret[0][4])
                    , Convert.ToString(dbret[0][5]), Convert.ToString(dbret[0][6])
                    , Convert.ToString(dbret[0][7]), Convert.ToString(dbret[0][8])
                    , Convert.ToString(dbret[0][9]), Convert.ToString(dbret[0][10])
                    , Convert.ToString(dbret[0][11]), Convert.ToString(dbret[0][12]));
                ret.LYT = Convert.ToString(dbret[0][13]);
                ret.ErrAbbr = Convert.ToString(dbret[0][14]);
                ret.Creator = Convert.ToString(dbret[0][15]);
                ret.ModuleSN = Convert.ToString(dbret[0][16]);

                ret.RetrieveComment(ctrl);

                if (string.Compare(ret.IssueType, ISSUETP.RMA) == 0)
                {
                    ret.RetrieveRMA();
                }

                if (string.Compare(ret.IssueType, ISSUETP.OBA) == 0)
                {
                    ret.RetrieveOBA();
                }

                if (string.Compare(ret.IssueType, ISSUETP.Quality) == 0)
                {
                    ret.RetrieveQuality();
                }

                if (string.Compare(ret.IssueType, ISSUETP.Rel) == 0)
                {
                    ret.RetrieveReliability();
                }

                ret.SubIssues = RetrieveSubIssue(ret.IssueKey,ctrl);
                ret.RetrieveAttachment(ret.IssueKey);

                return ret;
            }
            else
                return null;
        }

        public static List<IssueViewModels> RRetrieveFAByPjkey(string pjkey, string issuestatus,int topnum, Controller ctrl)
        {
            var retdict = new List<IssueViewModels>();

            var cond = "";
            var fixresolve = "";
            if (string.Compare(issuestatus, Resolute.Pending) == 0)
            {
                cond = "('" + Resolute.Pending + "','" + Resolute.Reopen + "')";
                fixresolve = Resolute.Pending;
            }
            else if (string.Compare(issuestatus, Resolute.Working) == 0)
            {
                cond = "('" + Resolute.Working + "')";
                fixresolve = Resolute.Working;
            }
            else
            {
                cond = "('" + Resolute.Fixed + "','" + Resolute.Done + "','" + Resolute.NotFix + "','" + Resolute.NotReproduce + "','" + Resolute.Unresolved + "')";
                fixresolve = Resolute.Done;
            }

            //var sql = "select top <topnum> ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,ParentIssueKey,RelativePeoples,APVal2,ModuleSN,DataID from Issue where  APVal1 <> 'delete' and  ParentIssueKey = '' and ProjectKey = '<ProjectKey>' and Resolution in <cond> and Creator = 'System' and IssueType <> '<IssueType>' order by ReportDate DESC";
            var sql = "select top <topnum> ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,ParentIssueKey,RelativePeoples,APVal2,ModuleSN,DataID,ErrAbbr from Issue where  APVal1 <> 'delete' and  ParentIssueKey = '' and ProjectKey = '<ProjectKey>' and Resolution in <cond> and IssueType <> '<IssueType>' order by ReportDate DESC";
            sql = sql.Replace("<ProjectKey>", pjkey).Replace("<cond>", cond).Replace("<topnum>", Convert.ToString(topnum))
                    .Replace("<IssueType>", ISSUETP.NPIPROC);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            foreach(var line in dbret)
            {
                var ret = new IssueViewModels(Convert.ToString(line[0])
                    , Convert.ToString(line[1]), Convert.ToString(line[2])
                    , Convert.ToString(line[3]), Convert.ToString(line[4])
                    , Convert.ToString(line[5]), Convert.ToString(line[6])
                    , Convert.ToString(line[7]), Convert.ToString(line[8])
                    , Convert.ToString(line[9]), Convert.ToString(line[10])
                    , Convert.ToString(line[11]), Convert.ToString(line[12]));
                ret.LYT = Convert.ToString(line[13]);
                ret.ModuleSN = Convert.ToString(line[14]);
                ret.DataID = Convert.ToString(line[15]);
                ret.ErrAbbr = Convert.ToString(line[16]);

                //var tempclist = new List<IssueComments>();
                //sql = "select IssueKey,Comment,Reporter,CommentDate,CommentType from IssueComments where IssueKey = '<IssueKey>' order by CommentDate ASC";
                //sql = sql.Replace("<IssueKey>", ret.IssueKey);
                //var cret = DBUtility.ExeLocalSqlWithRes(sql,null);
                //foreach (var r in cret)
                //{
                //    var tempcomment = new IssueComments();
                //    tempcomment.IssueKey = Convert.ToString(r[0]);
                //    tempcomment.dbComment = Convert.ToString(r[1]);
                //    tempcomment.Reporter = Convert.ToString(r[2]);
                //    tempcomment.CommentDate = DateTime.Parse(Convert.ToString(r[3]));
                //    tempcomment.CommentType = Convert.ToString(r[4]);
                //    tempclist.Add(tempcomment);
                //}
                //ret.CommentList = tempclist;
                //ret.SubIssues = RetrieveSubIssue(ret.IssueKey);

                //ret.RetrieveComment(ctrl);
                //ret.RetrieveAttachment(ret.IssueKey);
                retdict.Add( ret);
            }

            return retdict;
        }

        public static List<IssueViewModels> RRetrieveFAByErrAbbr(string pjkey, string errabbr, int topnum, Controller ctrl)
        {
            var retdict = new List<IssueViewModels>();

            var sql = "select top <topnum> ProjectKey,IssueKey,IssueType,Summary,Priority,"
                +"DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,ParentIssueKey,"
                +"RelativePeoples,APVal2,ModuleSN,DataID,ErrAbbr from Issue "
                + " where  APVal1 <> 'delete' and  ParentIssueKey = '' and ProjectKey = '<ProjectKey>' and ErrAbbr = '<ErrAbbr>' order by ReportDate DESC";
            sql = sql.Replace("<ProjectKey>", pjkey).Replace("<topnum>", Convert.ToString(topnum)).Replace("<ErrAbbr>", errabbr);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var ret = new IssueViewModels(Convert.ToString(line[0])
                    , Convert.ToString(line[1]), Convert.ToString(line[2])
                    , Convert.ToString(line[3]), Convert.ToString(line[4])
                    , Convert.ToString(line[5]), Convert.ToString(line[6])
                    , Convert.ToString(line[7]), Convert.ToString(line[8])
                    , Convert.ToString(line[9]), Convert.ToString(line[10])
                    , Convert.ToString(line[11]), Convert.ToString(line[12]));
                ret.LYT = Convert.ToString(line[13]);
                ret.ModuleSN = Convert.ToString(line[14]);
                ret.DataID = Convert.ToString(line[15]);
                ret.ErrAbbr = Convert.ToString(line[16]);

                ret.RetrieveComment(ctrl);
                //ret.RetrieveAttachment(ret.IssueKey);
                retdict.Add(ret);
            }

            return retdict;
        }

        public static List<IssueViewModels> RRetrieveFABySN(string pjkey, string sn, Controller ctrl)
        {
            var retdict = new List<IssueViewModels>();

            var sql = "select ProjectKey,IssueKey,IssueType,Summary,Priority,"
                + "DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,ParentIssueKey,"
                + "RelativePeoples,APVal2,ModuleSN,DataID,ErrAbbr from Issue "
                + " where  APVal1 <> 'delete' and  ParentIssueKey = '' and ProjectKey = '<ProjectKey>' and ModuleSN = '<ModuleSN>' order by ReportDate DESC";
            sql = sql.Replace("<ProjectKey>", pjkey).Replace("<ModuleSN>", sn);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var ret = new IssueViewModels(Convert.ToString(line[0])
                    , Convert.ToString(line[1]), Convert.ToString(line[2])
                    , Convert.ToString(line[3]), Convert.ToString(line[4])
                    , Convert.ToString(line[5]), Convert.ToString(line[6])
                    , Convert.ToString(line[7]), Convert.ToString(line[8])
                    , Convert.ToString(line[9]), Convert.ToString(line[10])
                    , Convert.ToString(line[11]), Convert.ToString(line[12]));
                ret.LYT = Convert.ToString(line[13]);
                ret.ModuleSN = Convert.ToString(line[14]);
                ret.DataID = Convert.ToString(line[15]);
                ret.ErrAbbr = Convert.ToString(line[16]);

                //var tempclist = new List<IssueComments>();
                //sql = "select IssueKey,Comment,Reporter,CommentDate,CommentType from IssueComments where IssueKey = '<IssueKey>' order by CommentDate ASC";
                //sql = sql.Replace("<IssueKey>", ret.IssueKey);
                //var cret = DBUtility.ExeLocalSqlWithRes(sql,null);
                //foreach (var r in cret)
                //{
                //    var tempcomment = new IssueComments();
                //    tempcomment.IssueKey = Convert.ToString(r[0]);
                //    tempcomment.dbComment = Convert.ToString(r[1]);
                //    tempcomment.Reporter = Convert.ToString(r[2]);
                //    tempcomment.CommentDate = DateTime.Parse(Convert.ToString(r[3]));
                //    tempcomment.CommentType = Convert.ToString(r[4]);
                //    tempclist.Add(tempcomment);
                //}
                //ret.CommentList = tempclist;
                //ret.SubIssues = RetrieveSubIssue(ret.IssueKey);

                ret.RetrieveComment(ctrl);
                ret.RetrieveAttachment(ret.IssueKey);
                retdict.Add(ret);
            }

            return retdict;
        }

        private static List<IssueViewModels> RRetrieveFABySN(string pjkey,string SN,string whichtest,Controller ctrl)
        {
            var retdict = new List<IssueViewModels>();

            var cond = "";
            cond = "('" + Resolute.Pending + "','" + Resolute.Reopen + "')";

            var sql = "select ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,ParentIssueKey,RelativePeoples,ModuleSN from Issue where  APVal1 <> 'delete' and  ParentIssueKey = '' and ProjectKey = '<ProjectKey>' and ModuleSN = '<ModuleSerialNum>' and Summary like '%<whichtest>%'  and Resolution in <cond> and Creator = 'System' and IssueType <> '<IssueType1>' and IssueType <> '<IssueType2>' order by ReportDate DESC";
            sql = sql.Replace("<ModuleSerialNum>", SN).Replace("<cond>", cond).Replace("<whichtest>", whichtest).Replace("<ProjectKey>", pjkey)
                    .Replace("<IssueType1>", ISSUETP.NPIPROC).Replace("<IssueType2>", ISSUETP.RMA);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            foreach (var line in dbret)
            {
                var ret = new IssueViewModels(Convert.ToString(line[0])
                    , Convert.ToString(line[1]), Convert.ToString(line[2])
                    , Convert.ToString(line[3]), Convert.ToString(line[4])
                    , Convert.ToString(line[5]), Convert.ToString(line[6])
                    , Convert.ToString(line[7]), Convert.ToString(line[8])
                    , Convert.ToString(line[9]), Convert.ToString(line[10])
                    , Convert.ToString(line[11]), Convert.ToString(line[12]));
                ret.ModuleSN = Convert.ToString(line[13]);

                ret.RetrieveComment(ctrl);
                retdict.Add(ret);
            }

            return retdict;
        }

        private static List<IssueViewModels> RRetrieveDupFABySN(string pjkey, string SN,string datestr)
        {
            var retdict = new List<IssueViewModels>();

            var cond = "";
            cond = "('" + Resolute.Pending + "','" + Resolute.Reopen + "')";

            var sql = "select ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,ParentIssueKey,RelativePeoples,ModuleSN from Issue where  APVal1 <> 'delete' and  ParentIssueKey = '' and ProjectKey = '<ProjectKey>' and ModuleSN = '<ModuleSerialNum>' and ReportDate < '<DATESTR>'  and Resolution in <cond> and Creator = 'System' and IssueType <> '<IssueType1>' and IssueType <> '<IssueType2>' order by ReportDate DESC";
            sql = sql.Replace("<ModuleSerialNum>", SN).Replace("<cond>", cond).Replace("<DATESTR>", datestr).Replace("<ProjectKey>", pjkey)
                    .Replace("<IssueType1>", ISSUETP.NPIPROC).Replace("<IssueType2>", ISSUETP.RMA);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            foreach (var line in dbret)
            {
                var ret = new IssueViewModels(Convert.ToString(line[0])
                    , Convert.ToString(line[1]), Convert.ToString(line[2])
                    , Convert.ToString(line[3]), Convert.ToString(line[4])
                    , Convert.ToString(line[5]), Convert.ToString(line[6])
                    , Convert.ToString(line[7]), Convert.ToString(line[8])
                    , Convert.ToString(line[9]), Convert.ToString(line[10])
                    , Convert.ToString(line[11]), Convert.ToString(line[12]));
                ret.ModuleSN = Convert.ToString(line[13]);

                retdict.Add(ret);
            }

            return retdict;
        }

        public static Dictionary<string, bool> RRetrieveBIPendingFA(string pjkey)
        {
            var retdict = new Dictionary<string, bool>();

            var cond = "";
            cond = "('" + Resolute.Pending + "','" + Resolute.Reopen + "')";

            var sql = "select  Summary,ModuleSN from Issue where APVal1 <> 'delete' and  ProjectKey = '<ProjectKey>' and Resolution in <cond> and Creator = 'System' and IssueType <> '<IssueType1>' and IssueType <> '<IssueType2>' order by ReportDate DESC";
            sql = sql.Replace("<cond>", cond).Replace("<ProjectKey>", pjkey)
                    .Replace("<IssueType1>", ISSUETP.NPIPROC).Replace("<IssueType2>", ISSUETP.RMA);


            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            foreach (var line in dbret)
            {
                if (Convert.ToString(line[0]).Contains("@Burn-In Step"))
                {
                    var sn = Convert.ToString(line[1]);
                    if (!string.IsNullOrEmpty(sn) && !retdict.ContainsKey(sn))
                    {
                        retdict.Add(sn, true);
                    }
                }
            }

            return retdict;
        }


        private static List<IssueViewModels> RRetrieveBIFABySN(string pjkey, string SN)
        {
            var retdict = new List<IssueViewModels>();

            var cond = "";
            cond = "('" + Resolute.Pending + "','" + Resolute.Reopen + "')";

            var sql = "select  ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,ParentIssueKey,RelativePeoples,ModuleSN from Issue where APVal1 <> 'delete' and  ProjectKey = '<ProjectKey>' and Summary like '%<BICond>%' and Summary like '%<ModuleSerialNum>%' and Resolution in <cond> and Creator = 'System' and IssueType <> '<IssueType1>' and IssueType <> '<IssueType2>' order by ReportDate DESC";
            sql = sql.Replace("<ModuleSerialNum>", SN).Replace("<BICond>", " @Burn-In Step ").Replace("<cond>", cond).Replace("<ProjectKey>", pjkey)
                    .Replace("<IssueType1>", ISSUETP.NPIPROC).Replace("<IssueType2>", ISSUETP.RMA);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            foreach (var line in dbret)
            {
                var ret = new IssueViewModels(Convert.ToString(line[0])
                    , Convert.ToString(line[1]), Convert.ToString(line[2])
                    , Convert.ToString(line[3]), Convert.ToString(line[4])
                    , Convert.ToString(line[5]), Convert.ToString(line[6])
                    , Convert.ToString(line[7]), Convert.ToString(line[8])
                    , Convert.ToString(line[9]), Convert.ToString(line[10])
                    , Convert.ToString(line[11]), Convert.ToString(line[12]));
                ret.ModuleSN = Convert.ToString(line[13]);

                retdict.Add(ret);
            }

            return retdict;
        }

        public static List<IssueViewModels> RRetrieveDupBIFABySN(string pjkey, string SN, string datestr)
        {
            var retdict = new List<IssueViewModels>();

            var cond = "";
            cond = "('" + Resolute.Pending + "','" + Resolute.Reopen + "')";

            var sql = "select  ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,ParentIssueKey,RelativePeoples,ModuleSN from Issue where APVal1 <> 'delete' and  ProjectKey = '<ProjectKey>' and Summary like '%<BICond>%' and ReportDate < '<DATESTR>' and Summary like '%<ModuleSerialNum>%' and Resolution in <cond> and Creator = 'System' and IssueType <> '<IssueType1>' and IssueType <> '<IssueType2>' order by ReportDate DESC";
            sql = sql.Replace("<ModuleSerialNum>", SN).Replace("<BICond>", " @Burn-In Step").Replace("<cond>", cond).Replace("<ProjectKey>", pjkey).Replace("<DATESTR>", datestr)
                    .Replace("<IssueType1>", ISSUETP.NPIPROC).Replace("<IssueType2>", ISSUETP.RMA);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            foreach (var line in dbret)
            {
                var ret = new IssueViewModels(Convert.ToString(line[0])
                    , Convert.ToString(line[1]), Convert.ToString(line[2])
                    , Convert.ToString(line[3]), Convert.ToString(line[4])
                    , Convert.ToString(line[5]), Convert.ToString(line[6])
                    , Convert.ToString(line[7]), Convert.ToString(line[8])
                    , Convert.ToString(line[9]), Convert.ToString(line[10])
                    , Convert.ToString(line[11]), Convert.ToString(line[12]));
                ret.ModuleSN = Convert.ToString(line[13]);

                retdict.Add(ret);
            }

            return retdict;
        }


        public static int RRetrieveFAStatusByPjkey(string pjkey, string issuestatus)
        {
            var cond = "";
            var fixresolve = "";
            if (string.Compare(issuestatus, Resolute.Pending) == 0)
            {
                cond = "('" + Resolute.Pending + "','" + Resolute.Reopen + "')";
                fixresolve = Resolute.Pending;
            }
            else if (string.Compare(issuestatus, Resolute.Working) == 0)
            {
                cond = "('" + Resolute.Working + "')";
                fixresolve = Resolute.Working;
            }
            else
            {
                cond = "('" + Resolute.Fixed + "','" + Resolute.Done + "','" + Resolute.NotFix + "','" + Resolute.NotReproduce + "','" + Resolute.Unresolved + "')";
                fixresolve = Resolute.Done;
            }

            var sql = "select IssueKey from Issue where APVal1 <> 'delete' and  ParentIssueKey = '' and  ProjectKey = '<ProjectKey>' and Resolution in <cond> and Creator = 'System' and IssueType <> '<IssueType>'";
            sql = sql.Replace("<ProjectKey>", pjkey).Replace("<cond>", cond).Replace("<IssueType>", ISSUETP.NPIPROC);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            return dbret.Count;
        }


        public static List<IssueViewModels> RetrieveTaskByProjectKey(string projectkey,string issuestatus)
        {
            var cond = "";
            var fixresolve = "";
            if (string.Compare(issuestatus, Resolute.Pending) == 0)
            {
                cond = "('" +Resolute.Pending+ "','"+Resolute.Reopen + "')";
                fixresolve = Resolute.Pending;
            }
            else if (string.Compare(issuestatus, Resolute.Working) == 0)
            {
                cond = "('" + Resolute.Working + "')";
                fixresolve = Resolute.Working;
            }
            else
            {
                cond = "('" + Resolute.Fixed + "','" + Resolute.Done + "','" + Resolute.NotFix + "','" + Resolute.NotReproduce + "','" + Resolute.Unresolved+ "')";
                fixresolve = Resolute.Done;
            }

            var sql = "select  ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples,ModuleSN from Issue where APVal1 <> 'delete' and  ProjectKey = '<ProjectKey>' and Resolution in <cond> and  ParentIssueKey = '' and IssueType <> '<IssueType1>' and IssueType <> '<IssueType2>' and IssueType <> '<IssueType3>' and IssueType <> '<IssueType4>' and IssueType <> '<IssueType5>' and Creator <> 'System' order by ReportDate DESC";
            sql = sql.Replace("<ProjectKey>", projectkey).Replace("<cond>", cond)
                .Replace("<IssueType1>", ISSUETP.NPIPROC).Replace("<IssueType2>", ISSUETP.RMA)
                .Replace("<IssueType3>", ISSUETP.OBA).Replace("<IssueType4>", ISSUETP.Quality).Replace("<IssueType5>", ISSUETP.Rel);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            var ret = new List<IssueViewModels>();
            foreach (var line in dbret)
            {
                ret.Add(new IssueViewModels(Convert.ToString(line[0])
                    , Convert.ToString(line[1]), Convert.ToString(line[2])
                    , Convert.ToString(line[3]), Convert.ToString(line[4])
                    , Convert.ToString(line[5]), Convert.ToString(line[6])
                    , Convert.ToString(line[7]), Convert.ToString(line[8])
                    , Convert.ToString(line[9]), fixresolve, "", Convert.ToString(line[11])));
            }

            return ret;
        }

        public static int RetrieveTaskCountByProjectKey(string projectkey, string issuestatus)
        {
            var cond = "";
            var fixresolve = "";
            if (string.Compare(issuestatus, Resolute.Pending) == 0)
            {
                cond = "('" + Resolute.Pending + "','" + Resolute.Reopen + "')";
                fixresolve = Resolute.Pending;
            }
            else if (string.Compare(issuestatus, Resolute.Working) == 0)
            {
                cond = "('" + Resolute.Working + "')";
                fixresolve = Resolute.Working;
            }
            else
            {
                cond = "('" + Resolute.Fixed + "','" + Resolute.Done + "','" + Resolute.NotFix + "','" + Resolute.NotReproduce + "','" + Resolute.Unresolved + "')";
                fixresolve = Resolute.Done;
            }

            var sql = "select IssueKey from Issue where APVal1 <> 'delete' and  ProjectKey = '<ProjectKey>' and Resolution in <cond> and  ParentIssueKey = '' and IssueType <> '<IssueType1>' and IssueType <> '<IssueType2>' and IssueType <> '<IssueType3>' and IssueType <> '<IssueType4>' and Creator <> 'System' order by ReportDate DESC";
            sql = sql.Replace("<ProjectKey>", projectkey).Replace("<cond>", cond)
                .Replace("<IssueType1>", ISSUETP.NPIPROC).Replace("<IssueType2>", ISSUETP.RMA)
                .Replace("<IssueType3>", ISSUETP.OBA).Replace("<IssueType4>", ISSUETP.Quality);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            return dbret.Count;
        }

        public static List<IssueViewModels> RetrieveIssueForIncreaseSummary(string projectkey,string startdate)
        {
            var sql = "select  ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples,ModuleSN,ErrAbbr from Issue where APVal1 <> 'delete' and  ProjectKey = '<ProjectKey>' and  ParentIssueKey = '' and IssueType <> '<IssueType>' and ReportDate > '<ReportDate>' order by ReportDate ASC";
            sql = sql.Replace("<ProjectKey>", projectkey).Replace("<ReportDate>", startdate).Replace("<IssueType>", ISSUETP.NPIPROC);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            var ret = new List<IssueViewModels>();
            foreach (var line in dbret)
            {
                var tempvm = new IssueViewModels(Convert.ToString(line[0])
                    , Convert.ToString(line[1]), Convert.ToString(line[2])
                    , Convert.ToString(line[3]), Convert.ToString(line[4])
                    , Convert.ToString(line[5]), Convert.ToString(line[6])
                    , Convert.ToString(line[7]), Convert.ToString(line[8])
                    , Convert.ToString(line[9]), Convert.ToString(line[10]), "", Convert.ToString(line[11]));
                tempvm.ModuleSN = Convert.ToString(line[12]);
                tempvm.ErrAbbr = Convert.ToString(line[13]);

                ret.Add(tempvm);
            }

            return ret;
        }

        public static List<IssueViewModels> RetrieveIssueForSolveSummary(string projectkey, string startdate)
        {
            var sql = "select  ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples,ModuleSN from Issue where APVal1 <> 'delete' and  ProjectKey = '<ProjectKey>' and  ParentIssueKey = '' and IssueType <> '<IssueType>' and ResolvedDate > '<ResolvedDate>' order by ResolvedDate ASC";
            sql = sql.Replace("<ProjectKey>", projectkey).Replace("<ResolvedDate>", startdate).Replace("<IssueType>", ISSUETP.NPIPROC);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            var ret = new List<IssueViewModels>();
            foreach (var line in dbret)
            {
                var tempvm = new IssueViewModels(Convert.ToString(line[0])
                    , Convert.ToString(line[1]), Convert.ToString(line[2])
                    , Convert.ToString(line[3]), Convert.ToString(line[4])
                    , Convert.ToString(line[5]), Convert.ToString(line[6])
                    , Convert.ToString(line[7]), Convert.ToString(line[8])
                    , Convert.ToString(line[9]), Convert.ToString(line[10]), "", Convert.ToString(line[11]));
                tempvm.ModuleSN = Convert.ToString(line[12]);

                ret.Add(tempvm);
            }

            return ret;
        }


        public static List<IssueViewModels> RetrieveIssueAllByUser(string user, string startdate, Controller ctrl)
        {
            var sql = "select  ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples,ModuleSN from Issue where APVal1 <> 'delete' and Resolution <> '<Resolution>' and  Assignee = '<Assignee>' and  ParentIssueKey = '' and IssueType <> '<IssueType>' and ReportDate > '<ReportDate>' order by ReportDate ASC";
            sql = sql.Replace("<Assignee>", user).Replace("<ReportDate>", startdate).Replace("<IssueType>", ISSUETP.NPIPROC).Replace("<Resolution>",Resolute.AutoClose);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            var ret = new List<IssueViewModels>();
            foreach (var line in dbret)
            {
                var tempvm = new IssueViewModels(Convert.ToString(line[0])
                    , Convert.ToString(line[1]), Convert.ToString(line[2])
                    , Convert.ToString(line[3]), Convert.ToString(line[4])
                    , Convert.ToString(line[5]), Convert.ToString(line[6])
                    , Convert.ToString(line[7]), Convert.ToString(line[8])
                    , Convert.ToString(line[9]), Convert.ToString(line[10]), "", Convert.ToString(line[11]));
                tempvm.ModuleSN = Convert.ToString(line[12]);

                tempvm.RetrieveComment(ctrl);
                tempvm.RetrieveAttachment(tempvm.IssueKey);

                if (string.Compare(tempvm.Resolution,Resolute.Pending) != 0
                    && string.Compare(tempvm.Resolution, Resolute.Working) != 0
                    && string.Compare(tempvm.Resolution, Resolute.Reopen) != 0
                    && tempvm.CommentList.Count == 1 && tempvm.AttachList.Count == 0)
                {
                    continue;
                }

                if (tempvm.CommentList.Count == 2)
                {
                    bool sameas = false;
                    foreach (var com in tempvm.CommentList)
                    {
                        if (com.Comment.Contains("<p>Issue Same As <a"))
                        {
                            sameas = true;
                            break;
                        }

                        if (com.Comment.Contains("passed")
                            && string.Compare(com.Reporter, "System", true) == 0)
                        {
                            sameas = true;
                            break;
                        }
                    }
                    if (sameas)
                    {
                        continue;
                    }
                }

                ret.Add(tempvm);
            }

            return ret;
        }

        private void RetrieveComment(Controller ctrl)
        {
                var tempclist = new List<IssueComments>();
                var csql = "select IssueKey,Comment,Reporter,CommentDate,CommentType from IssueComments where IssueKey = '<IssueKey>' order by CommentDate ASC";
                csql = csql.Replace("<IssueKey>", IssueKey);
                var cdbret = DBUtility.ExeLocalSqlWithRes(csql,null);
                foreach (var r in cdbret)
                {
                    var tempcomment = new IssueComments();
                    tempcomment.IssueKey = Convert.ToString(r[0]);
                    tempcomment.dbComment = Convert.ToString(r[1]);
                    tempcomment.Reporter = Convert.ToString(r[2]);
                    tempcomment.CommentDate = DateTime.Parse(Convert.ToString(r[3]));
                    tempcomment.CommentType = Convert.ToString(r[4]);
                    tempclist.Add(tempcomment);
                }

                CommentList = RepairBase64Image4IE(tempclist,ctrl);
        }

        private static string WriteBase64ImgFile(string commentcontent, Controller ctrl)
        {
            try
            {
                var idx = commentcontent.IndexOf("<img alt=\"\" src=\"data:image/png;base64");
                var base64idx = commentcontent.IndexOf("data:image/png;base64,", idx) + 22;
                var base64end = commentcontent.IndexOf("\"", base64idx);
                var imgstrend = commentcontent.IndexOf("/>", base64end) + 2;
                var base64img = commentcontent.Substring(base64idx, base64end - base64idx);
                var imgbytes = Convert.FromBase64String(base64img);

                var imgkey = Guid.NewGuid().ToString("N");
                string datestring = DateTime.Now.ToString("yyyyMMdd");
                string imgdir = ctrl.Server.MapPath("~/userfiles") + "\\images\\" + datestring + "\\";

                if (!Directory.Exists(imgdir))
                {
                    Directory.CreateDirectory(imgdir);
                }
                var realpath = imgdir + imgkey + ".jpg";

                var fs = File.Create(realpath);
                fs.Write(imgbytes, 0, imgbytes.Length);
                fs.Close();


                var url = "/userfiles/images/" + datestring + "/" + imgkey + ".jpg";
                var ret = commentcontent;
                ret = ret.Remove(idx, imgstrend - idx);
                ret = ret.Insert(idx, "<img src='" + url + "'/>");

                return ret;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }

        }

        private static string ReplaceBase64data2File(string commentcontent, Controller ctrl)
        {
            var ret = commentcontent;
            if (commentcontent.Contains("<img alt=\"\" src=\"data:image/png;base64"))
            {
                while (ret.Contains("<img alt=\"\" src=\"data:image/png;base64"))
                {
                    ret = WriteBase64ImgFile(ret, ctrl);
                    if (string.IsNullOrEmpty(ret))
                    {
                        ret = commentcontent;
                        break;
                    }
                }
            }

            return ret;
        }

        public static List<IssueComments> RepairBase64Image4IE(List<IssueComments> coments, Controller ctrl)
        {
            foreach (var com in coments)
            {
                var newcomment = ReplaceBase64data2File(com.Comment, ctrl);
                if (newcomment.Length != com.Comment.Length)
                {
                    com.Comment = newcomment;
                    UpdateSPComment(com.IssueKey, com.CommentType, com.CommentDate.ToString(), com.dbComment);
                }
            }
            return coments;
        }

        public static void DeleteSPComment(string issuekey,string commenttype,string date)
        {
            var csql = "delete from IssueComments where IssueKey = '<IssueKey>' and CommentDate = '<CommentDate>' and CommentType = '<CommentType>'";
            csql = csql.Replace("<IssueKey>", issuekey).Replace("<CommentDate>", date).Replace("<CommentType>", commenttype);
            DBUtility.ExeLocalSqlNoRes(csql);
        }

        public static IssueComments RetrieveSPComment(string issuekey, string commenttype, string date)
        {
            var tempcomment = new IssueComments();
            var csql = "select IssueKey,Comment,Reporter,CommentDate,CommentType from IssueComments where IssueKey = '<IssueKey>' and CommentDate = '<CommentDate>' and CommentType = '<CommentType>'";
            csql = csql.Replace("<IssueKey>", issuekey).Replace("<CommentDate>", date).Replace("<CommentType>", commenttype);
            var cdbret = DBUtility.ExeLocalSqlWithRes(csql, null);
            foreach (var r in cdbret)
            {
                tempcomment.IssueKey = Convert.ToString(r[0]);
                tempcomment.dbComment = Convert.ToString(r[1]);
                tempcomment.Reporter = Convert.ToString(r[2]);
                tempcomment.CommentDate = DateTime.Parse(Convert.ToString(r[3]));
                tempcomment.CommentType = Convert.ToString(r[4]);
            }
            return tempcomment;
        }

        public static void UpdateSPComment(string issuekey, string commenttype, string date,string dbcomment)
        {
            var csql = "update IssueComments set Comment = '<Comment>'  where IssueKey = '<IssueKey>' and CommentDate = '<CommentDate>' and CommentType = '<CommentType>'";
            csql = csql.Replace("<IssueKey>", issuekey).Replace("<CommentDate>", date).Replace("<CommentType>", commenttype).Replace("<Comment>", dbcomment);
            DBUtility.ExeLocalSqlNoRes(csql);
        }

        public static List<IssueViewModels> RetrieveIssuePendingByUser(string user, string startdate)
        {
            var cond = "('" + Resolute.Pending + "','" + Resolute.Reopen + "')";

            var sql = "select  ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples,ModuleSN from Issue where APVal1 <> 'delete' and  Assignee = '<Assignee>' and Resolution in <cond> and  ParentIssueKey = '' and IssueType <> '<IssueType>' and ReportDate > '<ReportDate>' order by ReportDate ASC";
            sql = sql.Replace("<Assignee>", user).Replace("<cond>", cond).Replace("<ReportDate>", startdate).Replace("<IssueType>", ISSUETP.NPIPROC);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            var ret = new List<IssueViewModels>();
            foreach (var line in dbret)
            {
                var tempvm = new IssueViewModels(Convert.ToString(line[0])
                    , Convert.ToString(line[1]), Convert.ToString(line[2])
                    , Convert.ToString(line[3]), Convert.ToString(line[4])
                    , Convert.ToString(line[5]), Convert.ToString(line[6])
                    , Convert.ToString(line[7]), Convert.ToString(line[8])
                    , Convert.ToString(line[9]), Convert.ToString(line[10]), "", Convert.ToString(line[11]));
                tempvm.ModuleSN = Convert.ToString(line[12]);

                tempvm.RetrieveAttachment(tempvm.IssueKey);

                ret.Add(tempvm);
            }

            return ret;
        }

        public static List<IssueViewModels> RetrieveIssueWorkingByUser(string user, string startdate)
        {
            var cond = "('" + Resolute.Working + "')";

            var sql = "select  ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples,ModuleSN from Issue where APVal1 <> 'delete' and  Assignee = '<Assignee>' and Resolution in <cond> and  ParentIssueKey = '' and IssueType <> '<IssueType>' and ReportDate > '<ReportDate>' order by ReportDate ASC";
            sql = sql.Replace("<Assignee>", user).Replace("<cond>", cond).Replace("<ReportDate>", startdate).Replace("<IssueType>", ISSUETP.NPIPROC);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            var ret = new List<IssueViewModels>();
            foreach (var line in dbret)
            {
                var tempvm = new IssueViewModels(Convert.ToString(line[0])
                    , Convert.ToString(line[1]), Convert.ToString(line[2])
                    , Convert.ToString(line[3]), Convert.ToString(line[4])
                    , Convert.ToString(line[5]), Convert.ToString(line[6])
                    , Convert.ToString(line[7]), Convert.ToString(line[8])
                    , Convert.ToString(line[9]), Convert.ToString(line[10]), "", Convert.ToString(line[11]));
                tempvm.ModuleSN = Convert.ToString(line[12]);

                tempvm.RetrieveAttachment(tempvm.IssueKey);

                ret.Add(tempvm);
            }

            return ret;
        }

        public static List<IssueViewModels> RetrieveIssueDoneByUser(string user, string startdate, Controller ctrl)
        {
            var sql = "select  ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples,ModuleSN from Issue where APVal1 <> 'delete' and Resolution <> '<Resolution>' and  Assignee = '<Assignee>' and  ParentIssueKey = '' and IssueType <> '<IssueType>' and ResolvedDate > '<ResolvedDate>' order by ResolvedDate ASC";
            sql = sql.Replace("<Assignee>", user.ToUpper()).Replace("<ResolvedDate>", startdate).Replace("<IssueType>", ISSUETP.NPIPROC).Replace("<Resolution>", Resolute.AutoClose);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            var ret = new List<IssueViewModels>();
            foreach (var line in dbret)
            {
                var tempvm = new IssueViewModels(Convert.ToString(line[0])
                    , Convert.ToString(line[1]), Convert.ToString(line[2])
                    , Convert.ToString(line[3]), Convert.ToString(line[4])
                    , Convert.ToString(line[5]), Convert.ToString(line[6])
                    , Convert.ToString(line[7]), Convert.ToString(line[8])
                    , Convert.ToString(line[9]), Convert.ToString(line[10]), "", Convert.ToString(line[11]));
                tempvm.ModuleSN = Convert.ToString(line[12]);

                tempvm.RetrieveComment(ctrl);
                tempvm.RetrieveAttachment(tempvm.IssueKey);

                if (tempvm.CommentList.Count == 1 && tempvm.AttachList.Count == 0)
                {
                    continue;
                }

                if (tempvm.CommentList.Count == 2)
                {
                    bool sameas = false;
                    foreach (var com in tempvm.CommentList)
                    {
                        if (com.Comment.Contains("<p>Issue Same As <a"))
                        {
                            sameas = true;
                            break;
                        }

                        if (com.Comment.Contains("passed")
                            && string.Compare(com.Reporter, "System", true) == 0)
                        {
                            sameas = true;
                            break;
                        }
                    }
                    if (sameas)
                    {
                        continue;
                    }
                }
                ret.Add(tempvm);
            }

            return ret;
        }

        

        public static List<IssueViewModels> RetrieveIssueTypeByProjectKey(string projectkey, string issuestatus,string issuetype, Controller ctrl)
        {
            var cond = "";
            var fixresolve = "";
            if (string.Compare(issuestatus, Resolute.Pending) == 0)
            {
                cond = "('" + Resolute.Pending + "','" + Resolute.Reopen + "')";
                fixresolve = Resolute.Pending;
            }
            else if (string.Compare(issuestatus, Resolute.Working) == 0)
            {
                cond = "('" + Resolute.Working + "')";
                fixresolve = Resolute.Working;
            }
            else
            {
                cond = "('" + Resolute.Fixed + "','" + Resolute.Done + "','" + Resolute.NotFix + "','" + Resolute.NotReproduce + "','" + Resolute.Unresolved + "')";
                fixresolve = Resolute.Done;
            }

            var sql = "select  ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples,ModuleSN,APVal2 from Issue where APVal1 <> 'delete' and  ProjectKey = '<ProjectKey>' and Resolution in <cond> and  ParentIssueKey = '' and IssueType = '<IssueType>' order by DueDate DESC";
            sql = sql.Replace("<ProjectKey>", projectkey).Replace("<cond>", cond).Replace("<IssueType>", issuetype);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            var ret = new List<IssueViewModels>();
            foreach (var line in dbret)
            {
                var tempsolve = fixresolve;
                if (string.Compare(Convert.ToString(line[10]), Resolute.Reopen) == 0)
                    tempsolve = Resolute.Reopen;

                var tempvm = new IssueViewModels(Convert.ToString(line[0])
                    , Convert.ToString(line[1]), Convert.ToString(line[2])
                    , Convert.ToString(line[3]), Convert.ToString(line[4])
                    , Convert.ToString(line[5]), Convert.ToString(line[6])
                    , Convert.ToString(line[7]), Convert.ToString(line[8])
                    , Convert.ToString(line[9]), tempsolve, "", Convert.ToString(line[11]));
                tempvm.ModuleSN = Convert.ToString(line[12]);
                tempvm.LYT = Convert.ToString(Convert.ToString(line[13]));

                if (string.Compare(issuetype, ISSUETP.RMA) == 0)
                {
                    tempvm.RetrieveRMA();
                }

                if (string.Compare(issuetype, ISSUETP.OBA) == 0)
                {
                    tempvm.RetrieveOBA();
                }

                if (string.Compare(issuetype, ISSUETP.Quality) == 0)
                {
                    tempvm.RetrieveQuality();
                }

                if (string.Compare(issuetype, ISSUETP.Rel) == 0)
                {
                    tempvm.RetrieveReliability();
                }

                tempvm.RetrieveComment(ctrl);

                tempvm.SubIssues = RetrieveSubIssue(tempvm.IssueKey,ctrl);

                tempvm.RetrieveAttachment(tempvm.IssueKey);

                ret.Add(tempvm);
            }

            return ret;
        }

        public static List<IssueViewModels> RetrieveIssueTypeByProjectKey(string projectkey, string StartDate, string EndDate, string issuetype, Controller ctrl)
        {
            var sql = "";
            if (string.Compare(StartDate, "NONE", true) == 0)
            {
                sql = "select  ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples,ModuleSN,APVal2 from Issue where APVal1 <> 'delete' and  ProjectKey = '<ProjectKey>' and  ParentIssueKey = '' and IssueType = '<IssueType>' order by DueDate DESC";
                sql = sql.Replace("<ProjectKey>", projectkey).Replace("<IssueType>", issuetype);
            }
            else
            {
                if (string.Compare(issuetype, ISSUETP.RMA, true) == 0
                    || string.Compare(issuetype, ISSUETP.Quality, true) == 0)
                {
                    sql = "select  ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples,ModuleSN,APVal2 from Issue where APVal1 <> 'delete' and  ProjectKey = '<ProjectKey>' and  ParentIssueKey = '' and IssueType = '<IssueType>' and DueDate >= '<StartDate>' and DueDate <= '<EndDate>' order by DueDate DESC";
                    sql = sql.Replace("<ProjectKey>", projectkey).Replace("<IssueType>", issuetype).Replace("<StartDate>", DateTime.Parse(StartDate).AddDays(12).ToString()).Replace("<EndDate>", DateTime.Parse(EndDate).AddDays(13).ToString());
                }
                else
                {
                    sql = "select  ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples,ModuleSN,APVal2 from Issue where APVal1 <> 'delete' and  ProjectKey = '<ProjectKey>' and  ParentIssueKey = '' and IssueType = '<IssueType>' and DueDate >= '<StartDate>' and DueDate <= '<EndDate>' order by DueDate DESC";
                    sql = sql.Replace("<ProjectKey>", projectkey).Replace("<IssueType>", issuetype).Replace("<StartDate>", DateTime.Parse(StartDate).AddDays(6).ToString()).Replace("<EndDate>", DateTime.Parse(EndDate).AddDays(7).ToString());
                }
            }

            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            var ret = new List<IssueViewModels>();
            foreach (var line in dbret)
            {

                var tempvm = new IssueViewModels(Convert.ToString(line[0])
                    , Convert.ToString(line[1]), Convert.ToString(line[2])
                    , Convert.ToString(line[3]), Convert.ToString(line[4])
                    , Convert.ToString(line[5]), Convert.ToString(line[6])
                    , Convert.ToString(line[7]), Convert.ToString(line[8])
                    , Convert.ToString(line[9]), Convert.ToString(line[10]), "", Convert.ToString(line[11]));

                tempvm.ModuleSN = Convert.ToString(line[12]);
                tempvm.LYT = Convert.ToString(line[13]);

                if (string.Compare(issuetype, ISSUETP.RMA) == 0)
                {
                    tempvm.RetrieveRMA();
                }

                if (string.Compare(issuetype, ISSUETP.OBA) == 0)
                {
                    tempvm.RetrieveOBA();
                }

                if (string.Compare(issuetype, ISSUETP.Quality) == 0)
                {
                    tempvm.RetrieveQuality();
                }

                if (string.Compare(issuetype, ISSUETP.Rel) == 0)
                {
                    tempvm.RetrieveReliability();
                }

                tempvm.RetrieveComment(ctrl);

                tempvm.SubIssues = RetrieveSubIssue(tempvm.IssueKey,ctrl);

                tempvm.RetrieveAttachment(tempvm.IssueKey);

                ret.Add(tempvm);
            }

            return ret;
        }

        public static List<IssueViewModels> RetrieveAllIssueTypeIssue(string StartDate, string EndDate, string issuetype, Controller ctrl)
        {
            var sql = "";
            if (string.Compare(StartDate, "NONE", true) == 0)
            {
                sql = "select  ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples,ModuleSN from Issue where APVal1 <> 'delete' and  ParentIssueKey = '' and IssueType = '<IssueType>' order by DueDate DESC";
                sql = sql.Replace("<IssueType>", issuetype);
            }
            else
            {
                if (string.Compare(issuetype, ISSUETP.RMA, true) == 0 
                    || string.Compare(issuetype, ISSUETP.Quality, true) == 0)
                {
                    sql = "select ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples,ModuleSN from Issue where APVal1 <> 'delete' and  ParentIssueKey = '' and IssueType = '<IssueType>' and DueDate >= '<StartDate>' and DueDate <= '<EndDate>' order by DueDate DESC";
                    sql = sql.Replace("<IssueType>", issuetype).Replace("<StartDate>", DateTime.Parse(StartDate).AddDays(12).ToString()).Replace("<EndDate>", DateTime.Parse(EndDate).AddDays(13).ToString());
                }
                else
                {
                    sql = "select ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples,ModuleSN from Issue where APVal1 <> 'delete' and  ParentIssueKey = '' and IssueType = '<IssueType>' and DueDate >= '<StartDate>' and DueDate <= '<EndDate>' order by DueDate DESC";
                    sql = sql.Replace("<IssueType>", issuetype).Replace("<StartDate>", DateTime.Parse(StartDate).AddDays(6).ToString()).Replace("<EndDate>", DateTime.Parse(EndDate).AddDays(7).ToString());
                }
            }

            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            var ret = new List<IssueViewModels>();
            foreach (var line in dbret)
            {

                var tempvm = new IssueViewModels(Convert.ToString(line[0])
                    , Convert.ToString(line[1]), Convert.ToString(line[2])
                    , Convert.ToString(line[3]), Convert.ToString(line[4])
                    , Convert.ToString(line[5]), Convert.ToString(line[6])
                    , Convert.ToString(line[7]), Convert.ToString(line[8])
                    , Convert.ToString(line[9]), Convert.ToString(line[10]), "", Convert.ToString(line[11]));
                tempvm.ModuleSN = Convert.ToString(line[12]);

                if (string.Compare(issuetype, ISSUETP.RMA) == 0)
                {
                    tempvm.RetrieveRMA();
                }

                if (string.Compare(issuetype, ISSUETP.OBA) == 0)
                {
                    tempvm.RetrieveOBA();
                }

                if (string.Compare(issuetype, ISSUETP.Quality) == 0)
                {
                    tempvm.RetrieveQuality();
                }

                if (string.Compare(issuetype, ISSUETP.Rel) == 0)
                {
                    tempvm.RetrieveReliability();
                }

                tempvm.RetrieveComment(ctrl);

                tempvm.SubIssues = RetrieveSubIssue(tempvm.IssueKey,ctrl);

                tempvm.RetrieveAttachment(tempvm.IssueKey);

                ret.Add(tempvm);
            }

            return ret;
        }


        public static List<IssueViewModels> Retrieve_Alert_RMAByProjectKey(string projectkey,bool fv)
        {

            var cond = "('" + Resolute.Pending + "','" + Resolute.Working + "','" + Resolute.Reopen + "')";
            var fixresolve = Resolute.Pending;
            
            //2 days for fv,5 days for fa
            var stattime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " 00:00:01").AddDays(10).ToString();
            var endtime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59").AddDays(10).ToString();

            if (!fv)
            {
                stattime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " 00:00:01").AddDays(5).ToString();
                endtime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59").AddDays(5).ToString();
            }

            var sql = "select ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples,ModuleSN,AlertEmailUpdateDate from Issue where APVal1 <> 'delete' and  ProjectKey = '<ProjectKey>' and Resolution in <cond> and  ParentIssueKey = '' and IssueType = '<IssueType>' and DueDate >= '<stattime>' and DueDate <= '<endtime>' ";
            sql = sql.Replace("<ProjectKey>", projectkey).Replace("<cond>", cond).Replace("<IssueType>", ISSUETP.RMA).Replace("<stattime>", stattime).Replace("<endtime>", endtime);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            var ret = new List<IssueViewModels>();
            foreach (var line in dbret)
            {
                var alertdate = DateTime.Parse(Convert.ToString(line[13]));

                if (string.Compare(alertdate.ToString("yyyy-MM-dd"), DateTime.Now.ToString("yyyy-MM-dd")) != 0)
                {
                    var tempvm = new IssueViewModels(Convert.ToString(line[0])
                        , Convert.ToString(line[1]), Convert.ToString(line[2])
                        , Convert.ToString(line[3]), Convert.ToString(line[4])
                        , Convert.ToString(line[5]), Convert.ToString(line[6])
                        , Convert.ToString(line[7]), Convert.ToString(line[8])
                        , Convert.ToString(line[9]), fixresolve, "", Convert.ToString(line[11]));
                    tempvm.ModuleSN = Convert.ToString(line[12]);

                    tempvm.AlertEmailUpdateDate = alertdate;
                    tempvm.RetrieveRMA();
                    ret.Add(tempvm);
                }//only add the issues which have not send alert email today

            }//end foreach

            return ret;
        }

        public static List<IssueViewModels> Retrieve_Alert_OBAByProjectKey(string projectkey)
        {

            var cond = "('" + Resolute.Pending + "','" + Resolute.Working + "','" + Resolute.Reopen + "')";
            var fixresolve = Resolute.Pending;

            //2 days for fv,5 days for fa
            var stattime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " 00:00:01").AddDays(4).ToString();
            var endtime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59").AddDays(4).ToString();


            var sql = "select ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples,ModuleSN,AlertEmailUpdateDate from Issue where APVal1 <> 'delete' and  ProjectKey = '<ProjectKey>' and Resolution in <cond> and  ParentIssueKey = '' and IssueType = '<IssueType>' and DueDate >= '<stattime>' and DueDate <= '<endtime>' ";
            sql = sql.Replace("<ProjectKey>", projectkey).Replace("<cond>", cond).Replace("<IssueType>", ISSUETP.OBA).Replace("<stattime>", stattime).Replace("<endtime>", endtime);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            var ret = new List<IssueViewModels>();
            foreach (var line in dbret)
            {
                var alertdate = DateTime.Parse(Convert.ToString(line[13]));

                if (string.Compare(alertdate.ToString("yyyy-MM-dd"), DateTime.Now.ToString("yyyy-MM-dd")) != 0)
                {
                    var tempvm = new IssueViewModels(Convert.ToString(line[0])
                        , Convert.ToString(line[1]), Convert.ToString(line[2])
                        , Convert.ToString(line[3]), Convert.ToString(line[4])
                        , Convert.ToString(line[5]), Convert.ToString(line[6])
                        , Convert.ToString(line[7]), Convert.ToString(line[8])
                        , Convert.ToString(line[9]), fixresolve, "", Convert.ToString(line[11]));
                    tempvm.ModuleSN = Convert.ToString(line[12]);

                    tempvm.AlertEmailUpdateDate = alertdate;
                    tempvm.RetrieveOBA();
                    ret.Add(tempvm);
                }//only add the issues which have not send alert email today

            }//end foreach

            return ret;
        }

        public static List<IssueViewModels> Retrieve_Alert_TaskByProjectKey()
        {

            var cond = "('" + Resolute.Pending + "','" + Resolute.Working + "','" + Resolute.Reopen + "')";
            var fixresolve = Resolute.Pending;

            //2 days for fv,5 days for fa
            var stattime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " 00:00:01").AddDays(2).ToString();
            var endtime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59").AddDays(2).ToString();

            var sql = "select ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples,ModuleSN,AlertEmailUpdateDate from Issue where APVal1 <> 'delete' and Resolution in <cond> and  ParentIssueKey = '' and IssueType = '<IssueType>' and DueDate >= '<stattime>' and DueDate <= '<endtime>' ";
            sql = sql.Replace("<cond>", cond).Replace("<IssueType>", ISSUETP.Task).Replace("<stattime>", stattime).Replace("<endtime>", endtime);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            var ret = new List<IssueViewModels>();
            foreach (var line in dbret)
            {
                var alertdate = DateTime.Parse(Convert.ToString(line[13]));

                if (string.Compare(alertdate.ToString("yyyy-MM-dd"), DateTime.Now.ToString("yyyy-MM-dd")) != 0)
                {
                    var tempvm = new IssueViewModels(Convert.ToString(line[0])
                        , Convert.ToString(line[1]), Convert.ToString(line[2])
                        , Convert.ToString(line[3]), Convert.ToString(line[4])
                        , Convert.ToString(line[5]), Convert.ToString(line[6])
                        , Convert.ToString(line[7]), Convert.ToString(line[8])
                        , Convert.ToString(line[9]), fixresolve, "", Convert.ToString(line[11]));
                    tempvm.ModuleSN = Convert.ToString(line[12]);
                    tempvm.AlertEmailUpdateDate = alertdate;
                    ret.Add(tempvm);
                }//only add the issues which have not send alert email today

            }//end foreach

            return ret;
        }

        public static int RetrieveRMACountByProjectKey(string projectkey, string issuestatus)
        {
            var cond = "";
            var fixresolve = "";
            if (string.Compare(issuestatus, Resolute.Pending) == 0)
            {
                cond = "('" + Resolute.Pending + "','" + Resolute.Reopen + "')";
                fixresolve = Resolute.Pending;
            }
            else if (string.Compare(issuestatus, Resolute.Working) == 0)
            {
                cond = "('" + Resolute.Working + "')";
                fixresolve = Resolute.Working;
            }
            else
            {
                cond = "('" + Resolute.Fixed + "','" + Resolute.Done + "','" + Resolute.NotFix + "','" + Resolute.NotReproduce + "','" + Resolute.Unresolved + "')";
                fixresolve = Resolute.Done;
            }

            var sql = "select IssueKey from Issue where APVal1 <> 'delete' and  ProjectKey = '<ProjectKey>' and Resolution in <cond> and  ParentIssueKey = '' and IssueType = '<IssueType>' order by ReportDate DESC";
            sql = sql.Replace("<ProjectKey>", projectkey).Replace("<cond>", cond).Replace("<IssueType>", ISSUETP.RMA);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            return dbret.Count;
        }


        public static List<IssueViewModels> RetrieveIssueByAssignee(string assignee, string issuestatus, int topnum)
        {
            var cond = "";
            var fixresolve = "";
            if (string.Compare(issuestatus, Resolute.Pending) == 0)
            {
                cond = "('" + Resolute.Pending + "','" + Resolute.Reopen + "')";
                fixresolve = Resolute.Pending;
            }
            else if (string.Compare(issuestatus, Resolute.Working) == 0)
            {
                cond = "('" + Resolute.Working + "')";
                fixresolve = Resolute.Working;
            }
            else
            {
                cond = "('" + Resolute.Fixed + "','" + Resolute.Done + "','" + Resolute.NotFix + "','" + Resolute.NotReproduce + "','" + Resolute.Unresolved + "')";
                fixresolve = Resolute.Done;
            }

            var sql = "select top <topnum> ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples,ModuleSN,ParentIssueKey from Issue where APVal1 <> 'delete' and  Assignee = '<Assignee>' and Resolution in <cond> and IssueType <> '<IssueType1>' and  Reporter <> 'System' order by ReportDate DESC";
            sql = sql.Replace("<Assignee>", assignee).Replace("<topnum>", Convert.ToString(topnum)).Replace("<cond>", cond).Replace("<IssueType1>", ISSUETP.NPIPROC);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            var ret = new List<IssueViewModels>();
            foreach (var line in dbret)
            {
                var vm = new IssueViewModels(Convert.ToString(line[0])
                    , Convert.ToString(line[1]), Convert.ToString(line[2])
                    , Convert.ToString(line[3]), Convert.ToString(line[4])
                    , Convert.ToString(line[5]), Convert.ToString(line[6])
                    , Convert.ToString(line[7]), Convert.ToString(line[8])
                    , Convert.ToString(line[9]), fixresolve, "", Convert.ToString(line[11]));
                vm.ModuleSN = Convert.ToString(line[12]);
                vm.ParentIssueKey = Convert.ToString(line[13]);
                ret.Add(vm);
            }

            return ret;
        }

        public static List<IssueViewModels> RetrieveIssueByAssigneeWorking(string assignee, string issuestatus, int topnum)
        {
            var cond = "";
            var fixresolve = "";
            if (string.Compare(issuestatus, Resolute.Pending) == 0)
            {
                cond = "('" + Resolute.Pending + "','" + Resolute.Reopen + "')";
                fixresolve = Resolute.Pending;
            }
            else if (string.Compare(issuestatus, Resolute.Working) == 0)
            {
                cond = "('" + Resolute.Working + "')";
                fixresolve = Resolute.Working;
            }
            else
            {
                cond = "('" + Resolute.Fixed + "','" + Resolute.Done + "','" + Resolute.NotFix + "','" + Resolute.NotReproduce + "','" + Resolute.Unresolved + "')";
                fixresolve = Resolute.Done;
            }

            var sql = "select top <topnum> ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples,ModuleSN,ParentIssueKey from Issue where APVal1 <> 'delete' and  Assignee = '<Assignee>' and Resolution in <cond> order by ReportDate DESC";
            sql = sql.Replace("<Assignee>", assignee).Replace("<topnum>", Convert.ToString(topnum)).Replace("<cond>", cond).Replace("<IssueType>", ISSUETP.Bug);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            var ret = new List<IssueViewModels>();
            foreach (var line in dbret)
            {
                var vm = new IssueViewModels(Convert.ToString(line[0])
                    , Convert.ToString(line[1]), Convert.ToString(line[2])
                    , Convert.ToString(line[3]), Convert.ToString(line[4])
                    , Convert.ToString(line[5]), Convert.ToString(line[6])
                    , Convert.ToString(line[7]), Convert.ToString(line[8])
                    , Convert.ToString(line[9]), fixresolve, "", Convert.ToString(line[11]));
                vm.ModuleSN = Convert.ToString(line[12]);
                vm.ParentIssueKey = Convert.ToString(line[13]);

                ret.Add(vm);
            }

            return ret;
        }


        public static List<IssueViewModels> RetrieveIssueByCreator(string creator, int topnum, Controller ctrl)
        {
            var sql = "select top <topnum> ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples,ModuleSN from Issue where APVal1 <> 'delete' and IssueType <> '<IssueType>' and  (Creator = '<Creator>' or Reporter = '<Creator>') and ParentIssueKey = '' order by ReportDate DESC";
            sql = sql.Replace("<Creator>", creator).Replace("<topnum>", Convert.ToString(topnum)).Replace("<IssueType>", ISSUETP.NPIPROC);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            var ret = new List<IssueViewModels>();
            foreach (var line in dbret)
            {
                var tempvm = new IssueViewModels(Convert.ToString(line[0])
                    , Convert.ToString(line[1]), Convert.ToString(line[2])
                    , Convert.ToString(line[3]), Convert.ToString(line[4])
                    , Convert.ToString(line[5]), Convert.ToString(line[6])
                    , Convert.ToString(line[7]), Convert.ToString(line[8])
                    , Convert.ToString(line[9]), Convert.ToString(line[10]), "", Convert.ToString(line[11]));
                tempvm.ModuleSN = Convert.ToString(line[12]);

                tempvm.SubIssues = RetrieveSubIssue(tempvm.IssueKey,ctrl);
                ret.Add(tempvm);
            }

            return ret;
        }

        public static List<IssueViewModels> RetrieveIssueByCreator(string creator,string issuestatus, Controller ctrl)
        {

            var cond = "";
            var fixresolve = "";
            if (string.Compare(issuestatus, Resolute.Pending) == 0)
            {
                cond = "('" + Resolute.Pending + "','" + Resolute.Reopen + "')";
                fixresolve = Resolute.Pending;
            }
            else if (string.Compare(issuestatus, Resolute.Working) == 0)
            {
                cond = "('" + Resolute.Working + "')";
                fixresolve = Resolute.Working;
            }
            else
            {
                cond = "('" + Resolute.Fixed + "','" + Resolute.Done + "','" + Resolute.NotFix + "','" + Resolute.NotReproduce + "','" + Resolute.Unresolved + "')";
                fixresolve = Resolute.Done;
            }

            var sql = "select ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples,ModuleSN from Issue where APVal1 <> 'delete' and IssueType <> '<IssueType>' and  (Creator = '<Creator>' or Reporter = '<Creator>') and ParentIssueKey = '' and Resolution in <cond>  order by ReportDate DESC";
            sql = sql.Replace("<Creator>", creator).Replace("<IssueType>", ISSUETP.NPIPROC).Replace("<cond>", cond);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            var ret = new List<IssueViewModels>();
            foreach (var line in dbret)
            {
                var tempvm = new IssueViewModels(Convert.ToString(line[0])
                    , Convert.ToString(line[1]), Convert.ToString(line[2])
                    , Convert.ToString(line[3]), Convert.ToString(line[4])
                    , Convert.ToString(line[5]), Convert.ToString(line[6])
                    , Convert.ToString(line[7]), Convert.ToString(line[8])
                    , Convert.ToString(line[9]), Convert.ToString(line[10]), "", Convert.ToString(line[11]));
                tempvm.ModuleSN = Convert.ToString(line[12]);

                tempvm.SubIssues = RetrieveSubIssue(tempvm.IssueKey,ctrl);
                ret.Add(tempvm);
            }

            return ret;
        }

        public static List<IssueViewModels> RetrieveNPIPROCIssue(string pjkey, Controller ctrl)
        {
            var sql = "select ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples,ModuleSN from Issue where APVal1 <> 'delete' and  ProjectKey = '<ProjectKey>' and ParentIssueKey = '' and IssueType = '<IssueType>' order by DueDate ASC";
            sql = sql.Replace("<ProjectKey>", pjkey).Replace("<IssueType>", Convert.ToString(ISSUETP.NPIPROC));
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            var ret = new List<IssueViewModels>();
            foreach (var line in dbret)
            {
                var tempvm = new IssueViewModels(Convert.ToString(line[0])
                    , Convert.ToString(line[1]), Convert.ToString(line[2])
                    , Convert.ToString(line[3]), Convert.ToString(line[4])
                    , Convert.ToString(line[5]), Convert.ToString(line[6])
                    , Convert.ToString(line[7]), Convert.ToString(line[8])
                    , Convert.ToString(line[9]), Convert.ToString(line[10]), "", Convert.ToString(line[11]));
                tempvm.ModuleSN = Convert.ToString(line[12]);

                tempvm.SubIssues = RetrieveSubIssue(tempvm.IssueKey,ctrl);
                tempvm.RetrieveAttachment(tempvm.IssueKey);
                ret.Add(tempvm);
            }
            return ret;
        }

        public static List<IssueViewModels> SearchIssue(string pjkey,string type,string resolution,string asignee,string startdate,string enddate,string summary, int topnum)
        {
            var sql = "select top <topnum> ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples,ModuleSN from Issue where APVal1 <> 'delete' and  <cond> order by ReportDate DESC";

            bool withand = false;
            var cond = "";
            if (!string.IsNullOrEmpty(pjkey))
            {
                withand = true;
                cond = cond + "ProjectKey = '"+ pjkey + "' ";
            }

            if (!string.IsNullOrEmpty(type))
            {
                if (withand)
                {
                    cond = cond + " and IssueType = '" + type + "' ";
                }
                else
                {
                    withand = true;
                    cond = cond + "IssueType = '" + type + "' ";
                }
            }

            if (!string.IsNullOrEmpty(resolution))
            {
                if (withand)
                {
                    cond = cond + " and Resolution = '" + resolution + "' ";
                }
                else
                {
                    withand = true;
                    cond = cond + "Resolution = '" + resolution + "' ";
                }
            }

            if (!string.IsNullOrEmpty(asignee))
            {
                if (withand)
                {
                    cond = cond + " and Assignee = '" + asignee + "' ";
                }
                else
                {
                    withand = true;
                    cond = cond + "Assignee = '" + asignee + "' ";
                }
            }

            if (!string.IsNullOrEmpty(startdate))
            {
                if (withand)
                {
                    cond = cond + " and ReportDate > '" + startdate + "' ";
                }
                else
                {
                    withand = true;
                    cond = cond + "ReportDate > '" + startdate + "' ";
                }
            }

            if (!string.IsNullOrEmpty(enddate))
            {
                if (withand)
                {
                    cond = cond + " and ReportDate < '" + enddate + "' ";
                }
                else
                {
                    withand = true;
                    cond = cond + "ReportDate < '" + enddate + "' ";
                }
            }

            if (!string.IsNullOrEmpty(summary))
            {
                if (withand)
                {
                    cond = cond + " and Summary like '%" + summary + "%' ";
                }
                else
                {
                    withand = true;
                    cond = cond + "Summary like '%" + summary + "%' ";
                }
            }

            sql = sql.Replace("<topnum>", Convert.ToString(topnum)).Replace("<cond>", cond);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            var ret = new List<IssueViewModels>();
            foreach (var line in dbret)
            {
                var tempvm = new IssueViewModels(Convert.ToString(line[0])
                    , Convert.ToString(line[1]), Convert.ToString(line[2])
                    , Convert.ToString(line[3]), Convert.ToString(line[4])
                    , Convert.ToString(line[5]), Convert.ToString(line[6])
                    , Convert.ToString(line[7]), Convert.ToString(line[8])
                    , Convert.ToString(line[9]), Convert.ToString(line[10]), "", Convert.ToString(line[11]));
                tempvm.ModuleSN = Convert.ToString(line[12]);

                ret.Add(tempvm);
            }

            return ret;
        }

        public static void DeleteAttachment(string issuekey,string cond)
        {
            var csql = "select Attachment from IssueAttachment where IssueKey = '<IssueKey>' and Attachment = N'<cond>'";
            csql = csql.Replace("<IssueKey>", issuekey).Replace("<cond>", cond);
            var cdbret = DBUtility.ExeLocalSqlWithRes(csql,null);
            if (cdbret.Count > 0 && cdbret.Count < 3)
            {
                csql = "delete from IssueAttachment where IssueKey = '<IssueKey>' and Attachment = N'<cond>'";
                csql = csql.Replace("<IssueKey>", issuekey).Replace("<cond>", cond);
                DBUtility.ExeLocalSqlNoRes(csql);
            }//end if
        }

        //public static void DeleteAttachComment(string issuekey, string cond)
        //{
        //    var csql = "select Comment from IssueComments where IssueKey = '<IssueKey>' and Comment = '<cond>'";
        //    csql = csql.Replace("<IssueKey>", issuekey).Replace("<cond>", cond);
        //    var cdbret = DBUtility.ExeLocalSqlWithRes(csql,null);
        //    if (cdbret.Count > 0 && cdbret.Count < 3)
        //    {
        //        csql = "delete from IssueComments where IssueKey = '<IssueKey>' and Comment = '<cond>'";
        //        csql = csql.Replace("<IssueKey>", issuekey).Replace("<cond>", cond);
        //        DBUtility.ExeLocalSqlNoRes(csql);
        //    }//end if
        //}


        public static void StoreBIRootCause(string pjkey, string sn,string rootcause)
        {
            var csql = "delete from BIROOTCAUSE where ProjectKey = '<ProjectKey>' and ModuleSN = N'<ModuleSN>'";
            csql = csql.Replace("<ProjectKey>", pjkey).Replace("<ModuleSN>", sn);
            DBUtility.ExeLocalSqlNoRes(csql);

            var sql = "insert into BIROOTCAUSE(ProjectKey,ModuleSN,RootCause,databackuptm) values('<ProjectKey>',N'<ModuleSN>','<RootCause>','<databackuptm>')";
            sql = sql.Replace("<ProjectKey>", pjkey).Replace("<ModuleSN>", sn).Replace("<RootCause>", rootcause).Replace("<databackuptm>", DateTime.Now.ToString());
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static string RetrieveBIRootCause(string pjkey, string sn)
        {
            var csql = "select RootCause from BIROOTCAUSE where ProjectKey = '<ProjectKey>' and ModuleSN = N'<ModuleSN>'";
            csql = csql.Replace("<ProjectKey>", pjkey).Replace("<ModuleSN>", sn);
            var dbret = DBUtility.ExeLocalSqlWithRes(csql,null);
            if (dbret.Count > 0)
            {
                return Convert.ToString(dbret[0][0]);
            }
            else
            {
                return string.Empty;
            }
        }

        public static void UpdateLYT(string IssueKey)
        {
            var sql = "update Issue set APVal2 = '<LYT>' where IssueKey = '<IssueKey>'";
            sql = sql.Replace("<IssueKey>", IssueKey).Replace("<LYT>", "LYT");
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static Dictionary<string,string> RetrieveAllBIRootCause(string pjkey)
        {
            var ret =  new Dictionary<string, string>();
            var csql = "select ModuleSN,RootCause from BIROOTCAUSE where ProjectKey = '<ProjectKey>'";
            csql = csql.Replace("<ProjectKey>", pjkey);
            var dbret = DBUtility.ExeLocalSqlWithRes(csql,null);
            foreach (var line in dbret)
            {
                var sn = Convert.ToString(line[0]);
                var root = Convert.ToString(line[1]);
                if (!ret.ContainsKey(sn))
                {
                    ret.Add(sn, root);
                }
            }
            return ret;
        }

        private static void CreateNPISubIssue(string projectname, string pjkey,string parentkey, string firstengineer, string sum, string desc,int duemonth)
        {
            var vm = new IssueViewModels();
            vm.ProjectKey = pjkey;
            vm.IssueKey = IssueViewModels.GetUniqKey();
            vm.ParentIssueKey = parentkey;
            vm.IssueType = ISSUETP.NPIPROC;
            vm.Summary = projectname+" "+sum;
            vm.Priority = ISSUEPR.Major;
            vm.DueDate = DateTime.Now.AddMonths(duemonth);
            vm.ReportDate = DateTime.Now;
            vm.Assignee = firstengineer;
            vm.Reporter = "System";
            vm.Resolution = Resolute.Pending;
            vm.ResolvedDate = DateTime.Parse("1982-05-06 01:01:01");
            vm.Description = desc;
            vm.CommentType = COMMENTTYPE.Description;
            vm.StoreSubIssue();
        }

        private static void CreatePIP1Issues(string projectname, string pjkey, string firstengineer)
        {
            var parentkey = "";
            var vm = new IssueViewModels();
            vm.ProjectKey = pjkey;
            vm.IssueKey = IssueViewModels.GetUniqKey();
            parentkey = vm.IssueKey;
            vm.IssueType = ISSUETP.NPIPROC;
            vm.Summary = projectname + " PIP1";
            vm.Priority = ISSUEPR.Major;
            vm.DueDate = DateTime.Now;
            vm.ReportDate = DateTime.Now;
            vm.Assignee = firstengineer;
            vm.Reporter = "System";
            vm.Resolution = Resolute.Pending;
            vm.ResolvedDate = DateTime.Parse("1982-05-06 01:01:01");
            vm.Description = "Assign resources and initiate project";
            vm.CommentType = COMMENTTYPE.Description;
            vm.StoreIssue();
        }

        private static void CreateEVTIssues(string projectname, string pjkey, string firstengineer)
        {
            var parentkey = "";
            var vm = new IssueViewModels();
            vm.ProjectKey = pjkey;
            vm.IssueKey = IssueViewModels.GetUniqKey();
            parentkey = vm.IssueKey;
            vm.IssueType = ISSUETP.NPIPROC;
            vm.Summary = projectname + " EVT";
            vm.Priority = ISSUEPR.Major;
            vm.DueDate = DateTime.Now.AddMonths(3);
            vm.ReportDate = DateTime.Now;
            vm.Assignee = firstengineer;
            vm.Reporter = "System";
            vm.Resolution = Resolute.Pending;
            vm.ResolvedDate = DateTime.Parse("1982-05-06 01:01:01");
            vm.Description = "Build prototypes and prove feasibility";
            vm.CommentType = COMMENTTYPE.Description;
            vm.StoreIssue();

            //var sum = "Complete Firmware Architecture Review";
            //var desc = "Complete Firmware Architecture Review";
            //CreateNPISubIssue(projectname,pjkey, parentkey,firstengineer,sum,desc,3);

            //sum = "EVT Presentation Completed and ECO Approved";
            //desc = "EVT Presentation Completed and ECO Approved";
            //CreateNPISubIssue(projectname, pjkey, parentkey, firstengineer, sum, desc,3);

            //sum = "EVT Presentation Completed and ECO Approved";
            //desc = "EVT Presentation Completed and ECO Approved";
            //CreateNPISubIssue(projectname, pjkey, parentkey, firstengineer, sum, desc,3);

            //sum = "Design Review Completed and ECO Approved";
            //desc = "Design Review Completed and ECO Approved";
            //CreateNPISubIssue(projectname, pjkey, parentkey, firstengineer, sum, desc,3);

            //sum = "PVT type testing performed";
            //desc = "PVT type testing performed. Results pass or corrective actions identified for failures";
            //CreateNPISubIssue(projectname, pjkey, parentkey, firstengineer, sum, desc,3);

            //sum = "IC - Prototype released";
            //desc = "IC - Prototype released";
            //CreateNPISubIssue(projectname, pjkey, parentkey, firstengineer, sum, desc,3);

            //sum = "OSAs: internal passed EVT external Prototype released";
            //desc = "OSAs: internal passed EVT external Prototype released";
            //CreateNPISubIssue(projectname, pjkey, parentkey, firstengineer, sum, desc,3);
        }


        private static void CreateDVTIssues(string projectname, string pjkey, string firstengineer)
        {
            var parentkey = "";
            var vm = new IssueViewModels();
            vm.ProjectKey = pjkey;
            vm.IssueKey = IssueViewModels.GetUniqKey();
            parentkey = vm.IssueKey;
            vm.IssueType = ISSUETP.NPIPROC;
            vm.Summary = projectname + " DVT";
            vm.Priority = ISSUEPR.Major;
            vm.DueDate = DateTime.Now.AddMonths(6);
            vm.ReportDate = DateTime.Now;
            vm.Assignee = firstengineer;
            vm.Reporter = "System";
            vm.Resolution = Resolute.Pending;
            vm.ResolvedDate = DateTime.Parse("1982-05-06 01:01:01");
            vm.Description = "Finalize Product Design and Verify suitability for production";
            vm.CommentType = COMMENTTYPE.Description;
            vm.StoreIssue();

            //var sum = "DVT Presentation Completed and ECO Approved";
            //var desc = "DVT Presentation Completed and ECO Approved";
            //CreateNPISubIssue(projectname, pjkey, parentkey, firstengineer, sum, desc, 6);

            //sum = "Design Review Completed";
            //desc = "Design Review Completed";
            //CreateNPISubIssue(projectname, pjkey, parentkey, firstengineer, sum, desc, 6);

            //sum = "Standard and Logic/Firmware PVT report is a pass";
            //desc = "Standard and Logic/Firmware PVT report is a pass";
            //CreateNPISubIssue(projectname, pjkey, parentkey, firstengineer, sum, desc, 6);

            //sum = "Cpk from NPI Build Data shows Cpk > 0.7";
            //desc = "Cpk from NPI Build Data shows Cpk > 0.7";
            //CreateNPISubIssue(projectname, pjkey, parentkey, firstengineer, sum, desc, 6);

            //sum = "Yield Requirements";
            //desc = "Low Complexity : 1st Pass 50%, Cumulative without Rework 80%.Medium Complexity : 1st Pass 40%, Cumulative without Rework 75%.High Complexity : 1st Pass 25%, Cumulative without Rework 70%";
            //CreateNPISubIssue(projectname, pjkey, parentkey, firstengineer, sum, desc, 6);

            //sum = "IC - Pre-Production released";
            //desc = "IC - Pre-Production released";
            //CreateNPISubIssue(projectname, pjkey, parentkey, firstengineer, sum, desc, 6);

            //sum = "OSAs: internal passed DVT, external Pre-Production released";
            //desc = "OSAs: internal passed DVT, external Pre-Production released";
            //CreateNPISubIssue(projectname, pjkey, parentkey, firstengineer, sum, desc, 6);
        }

        private static void CreateMVTIssues(string projectname, string pjkey, string firstengineer)
        {
            var parentkey = "";
            var vm = new IssueViewModels();
            vm.ProjectKey = pjkey;
            vm.IssueKey = IssueViewModels.GetUniqKey();
            parentkey = vm.IssueKey;
            vm.IssueType = ISSUETP.NPIPROC;
            vm.Summary = projectname + " MVT";
            vm.Priority = ISSUEPR.Major;
            vm.DueDate = DateTime.Now.AddMonths(9);
            vm.ReportDate = DateTime.Now;
            vm.Assignee = firstengineer;
            vm.Reporter = "System";
            vm.Resolution = Resolute.Pending;
            vm.ResolvedDate = DateTime.Parse("1982-05-06 01:01:01");
            vm.Description = "Finalize Manufacturing Process and verify readiness of Product for Production Release and Volume Ramp";
            vm.CommentType = COMMENTTYPE.Description;
            vm.StoreIssue();

            //var sum = "MVT Presentation Completed and ECO Approved";
            //var desc = "MVT Presentation Completed and ECO Approved";
            //CreateNPISubIssue(projectname, pjkey, parentkey, firstengineer, sum, desc, 9);

            //sum = "Standard and Logic/Firmware PVT report is a pass";
            //desc = "Standard and Logic/Firmware PVT report is a pass";
            //CreateNPISubIssue(projectname, pjkey, parentkey, firstengineer, sum, desc, 9);

            //sum = "Cpk from NPI Build Data shows Cpk > 1.0";
            //desc = "Cpk from NPI Build Data shows Cpk > 1.0";
            //CreateNPISubIssue(projectname, pjkey, parentkey, firstengineer, sum, desc, 9);

            //sum = "Yield Requirements";
            //desc = "Low Complexity : 1st Pass 70%, Cumulative without Rework 90%.Medium Complexity : 1st Pass 55% Cumulative w/o Rework 85%.High Complexity : 1st Pass 40%, Cumulative without Rework 80%";
            //CreateNPISubIssue(projectname, pjkey, parentkey, firstengineer, sum, desc, 9);

            //sum = "IC - Pre-Production released";
            //desc = "IC - Pre-Production released";
            //CreateNPISubIssue(projectname, pjkey, parentkey, firstengineer, sum, desc, 9);

            //sum = "OSAs: internal passed DVT, external Pre-Production released";
            //desc = "OSAs: internal passed DVT, external Pre-Production released";
            //CreateNPISubIssue(projectname, pjkey, parentkey, firstengineer, sum, desc, 9);

            //sum = "Rel - 1,000 Hours Complete";
            //desc = "Rel - 1,000 Hours Complete";
            //CreateNPISubIssue(projectname, pjkey, parentkey, firstengineer, sum, desc, 9);
        }

        private static void CreateMPIssues(string projectname, string pjkey, string firstengineer)
        {
            var parentkey = "";
            var vm = new IssueViewModels();
            vm.ProjectKey = pjkey;
            vm.IssueKey = IssueViewModels.GetUniqKey();
            parentkey = vm.IssueKey;
            vm.IssueType = ISSUETP.NPIPROC;
            vm.Summary = projectname + " MP";
            vm.Priority = ISSUEPR.Major;
            vm.DueDate = DateTime.Now.AddMonths(12);
            vm.ReportDate = DateTime.Now;
            vm.Assignee = firstengineer;
            vm.Reporter = "System";
            vm.Resolution = Resolute.Pending;
            vm.ResolvedDate = DateTime.Parse("1982-05-06 01:01:01");
            vm.Description = "Mass Produce";
            vm.CommentType = COMMENTTYPE.Description;
            vm.StoreIssue();
        }

        public static void CreateNPIProcTasks(string projectname,string pjkey,string firstengineer)
        {
            CreatePIP1Issues(projectname, pjkey, firstengineer);
            CreateEVTIssues(projectname, pjkey, firstengineer);
            CreateDVTIssues(projectname, pjkey, firstengineer);
            CreateMVTIssues(projectname, pjkey, firstengineer);
            CreateMPIssues(projectname, pjkey, firstengineer);
        }


        public static void CloseIssueAutomaticlly(string pjkey, string SN, string whichtest, string tester, string datestr,Controller ctrl)
        {
            var issues = IssueViewModels.RRetrieveFABySN(pjkey, SN, whichtest,ctrl);
            foreach (var tobedata in issues)
            {
                if (tobedata.CommentList.Count < 2)
                {
                    tobedata.Resolution = Resolute.AutoClose;
                    tobedata.Description = "Module " + SN + " passed " + whichtest + " test @" + tester + " @" + datestr;
                    tobedata.UpdateIssue();
                    tobedata.CloseIssue();
                }
            }
        }

        public static void CloseIssueAutomaticllyWithFailedSN(string pjkey, string SN, string whichtest, string tester, string datestr, Controller ctrl)
        {
            var issues = IssueViewModels.RRetrieveFABySN(pjkey, SN, whichtest,ctrl);
            foreach (var tobedata in issues)
            {
                if (tobedata.CommentList.Count < 2)
                {
                    tobedata.Resolution = Resolute.AutoClose;
                    tobedata.Description = "Module " + SN + " faild for " + whichtest + " test @" + tester + " @" + datestr+" again";
                    tobedata.UpdateIssue();
                    tobedata.CloseIssue();
                }
            }
        }

        public static void CloseDupIssueAutomaticlly(string pjkey, string SN, string datestr)
        {
            var issues = IssueViewModels.RRetrieveDupFABySN(pjkey, SN, datestr);
            foreach (var tobedata in issues)
            {
                tobedata.Resolution = Resolute.AutoClose;
                tobedata.Description = "Module " + SN + " is closed automaticlly for duplication reason " + " @" + DateTime.Now.ToString();
                tobedata.UpdateIssue();
                tobedata.CloseIssue();
            }
        }

        public static void CloseBIIssueAutomaticlly(List<BITestData> passlist)
        {
            var closesn = new List<BITestData>();
            if (passlist.Count > 0)
            {
                var pendingissue = IssueViewModels.RRetrieveBIPendingFA(passlist[0].ProjectKey);
                foreach (var item in passlist)
                {
                    if (pendingissue.ContainsKey(item.ModuleSerialNum))
                    {
                        closesn.Add(item);
                    }
                }
            }

            foreach (var item in closesn)
            {
                var issues = RRetrieveBIFABySN(item.ProjectKey, item.ModuleSerialNum);
                foreach (var tobedata in issues)
                {
                    if (item.TestTimeStamp > tobedata.ReportDate)
                    {
                        tobedata.Resolution = Resolute.AutoClose;
                        tobedata.Description = "Module " + item.ModuleSerialNum + " passed " + item.WhichTest + " test @" + item.TestStation + " @" + item.TestTimeStamp.ToString("yyyy-MM-dd HH:mm:ss");
                        tobedata.UpdateIssue();
                        tobedata.CloseIssue();
                    }
                }
            }
        }

        public static void CloseDupBIIssueAutomaticlly(string pjkey, string SN, string datestr)
        {
            var issues = IssueViewModels.RRetrieveDupBIFABySN(pjkey, SN, datestr);
            foreach (var tobedata in issues)
            {
                tobedata.Resolution = Resolute.AutoClose;
                tobedata.Description = "Module " + SN + "  is closed automaticlly for duplication reason " + " @" + DateTime.Now.ToString();
                tobedata.UpdateIssue();
                tobedata.CloseIssue();
            }
        }

        public static void UpdateIssueAssigneeAndResolution(string issuekey, string assignee, string resolution, string summary)
        {
            var sql = "update Issue set Summary = '<Summary>', Assignee = '<Assignee>', Resolution = '<Resolution>' where IssueKey = '<IssueKey>'";
            sql = sql.Replace("<IssueKey>", issuekey).Replace("<Assignee>", assignee).Replace("<Resolution>", resolution).Replace("<Summary>", summary);
            DBUtility.ExeLocalSqlNoRes(sql);
        }


        public static int RetrieveSolveIssueCount(string pjkey, string errAbbr)
        {
            var ret = 0;
            var cond = "('" + Resolute.Fixed + "','" + Resolute.Done + "','" + Resolute.NotFix + "','" + Resolute.NotReproduce + "','" + Resolute.Unresolved + "')";
            var sql = "select Count(*) from Issue where APVal1 <> 'delete' and ProjectKey = '<ProjectKey>' and Resolution in <cond> and ErrAbbr = '<ErrAbbr>'";
            sql = sql.Replace("<ProjectKey>", pjkey).Replace("<ErrAbbr>", errAbbr).Replace("<cond>", cond);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            if(dbret.Count > 0)
            {
                try
                {
                    return Convert.ToInt32(dbret[0][0]);
                }
                catch (Exception ex) { return 0; }
            }

            return ret;
        }


        public static Dictionary<string, TaskData> getProjectTask(string uName, string pKey, int tPeriod, string sDate, string eDate, int iType, bool wSubTask = true)
        {
            //var issuetypes = "";
            //if (iType == ISSUESUBTYPE.CrititalFailureTask)
            //{
            //    issuetypes = "('" + ISSUESUBTYPE.CrititalFailureTask + "', '"+ISSUESUBTYPE.OCAP+"')";
            //}
            //else
            //{
            //    issuetypes = "('" + iType + "')";
            //}
            var sql = "select Issue.Summary as Description, Issue.IssueKey, Issue.IssueType, " +
                            "Issue.Resolution, Issue.ReportDate as StartDate, "+
                            "Issue.DueDate, Log.Date as UpdateTme, ia.Attachment, it.IssueSubType " +
                      "from Issue " +
                          "left join Log on Issue.IssueKey = Log.IssueKey " +
                          "left join IssueAttachment as ia on ia.IssueKey = Issue.IssueKey " +
                          "left join IssueType as it on it.IssueKey = Issue.IssueKey " +
                      "where Reporter <> 'System' " +
                          "and Issue.ProjectKey = '<ProjectKey>' " +
                          "and Issue.APVal1 <> 'delete' " +
                          "and Issue.Assignee = '<UserName>' " +
                          "and it.IssueSubType = '<IssueType>' ";
            if( !wSubTask)
            {
                sql += "and Issue.ParentIssueKey = '' ";
            }
            var cond = "";
            if (tPeriod == 0)
            {
                //history
                cond = "('" + Resolute.Fixed + "','" + Resolute.NotFix + "','" + Resolute.Done + "')";
                sql += "and Issue.Resolution in <cond> and (Log.Date is null or Log.Date < '<sDate>') " +
                      "order by Log.Date Desc, Issue.ReportDate Desc; ";
            }
            else
            {
                cond = "('" + Resolute.Pending + "','" + Resolute.Reopen + "','" + Resolute.Working + "')";
                sql += "and (Issue.Resolution in <cond> or (Log.Date Between '<sDate>' And '<eDate>')) " +
                      "order by Log.Date Desc, Issue.ReportDate Desc; ";
            }
                         
            sql = sql.Replace("<UserName>", uName).Replace("<cond>", cond).Replace("<IssueType>", iType.ToString()).Replace("<ProjectKey>", pKey).Replace("<sDate>", sDate).Replace("<eDate>", eDate);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            var ret = new Dictionary<string, TaskData>();
            foreach (var line in dbret)
            {
                if (ret.ContainsKey(Convert.ToString(line[1])))
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(line[7])))
                    {
                        ret[Convert.ToString(line[1])].Attachment.Add(Convert.ToString(line[7]));
                    }
                }
                else
                {
                    var attach = new List<string>();
                    if (!string.IsNullOrEmpty(Convert.ToString(line[7])))
                    {
                        attach.Add(Convert.ToString(line[7]));
                    }
                    var tmp = new TaskData(
                        Convert.ToString(line[1]),
                        Convert.ToString(line[0]),
                        Convert.ToString(line[2]),
                        Convert.ToString(line[8]),
                        Convert.ToString(line[3]),
                        convertDate(Convert.ToString(line[4])),
                        convertDate(Convert.ToString(line[5])),
                        convertDate(Convert.ToString(line[6])),
                        attach
                    );
                    ret.Add(Convert.ToString(line[1]), tmp);
                }
            }

            return ret;
        }

        private static DateTime convertDate(string date)
        {
            return string.IsNullOrEmpty(date) ? new DateTime(1, 1, 1, 0, 0, 0) : Convert.ToDateTime(date);
        }
    }

    public class IssueType
    {
        public IssueType()
        {
            IssueKey = "";
            IssueSubType = "";
            IssueSsubType = "";
        }
        public IssueType(string iKey, string iSubType, string iSsubType)
        {
            IssueKey = iKey;
            IssueSubType = iSubType;
            IssueSsubType = iSsubType;
        }
        public string IssueKey { set; get; }
        public string IssueSubType { set; get; }
        public string IssueSsubType { set; get; }

        public void SaveIssueType(List<IssueType> iTypeList)
        {
            var ikeylist = "(";
            var items = "";
            foreach (var iType in iTypeList)
            {
                ikeylist += "'" + iType.IssueKey + "',";

                items += "('" + iType.IssueKey + "',"
                        + "'" + iType.IssueSubType + "',"
                        + "'" + iType.IssueSsubType + "'),";
            }
            ikeylist = ikeylist.Substring(0, ikeylist.Length - 1) + ")";
            var sqltmp = "delete from IssueType where IssueKey in <IssueKey> ";
            sqltmp = sqltmp.Replace("<IssueKey>", ikeylist);
            DBUtility.ExeLocalSqlNoRes(sqltmp);

            items = items.Substring(0, items.Length - 1);
            var sql = "insert into IssueType (IssueKey, IssueSubType, IssueSsubType) values <Items>; ";
            sql = sql.Replace("<Items>", items);

            DBUtility.ExeLocalSqlNoRes(sql);
        }
    }
}