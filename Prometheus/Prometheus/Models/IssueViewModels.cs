using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
                foreach (var item in value)
                {
                    if (item.Summary.Contains("[Containment]"))
                    {
                        containmentactionlist.Add(item);
                    }
                    if (item.Summary.Contains("[Corrective]"))
                    {
                        Correctiveactionlist.Add(item);
                    }
                }
            }
            get
            {
                return sissue;
            }
        }

        private List<IssueViewModels> containmentactionlist = new List<IssueViewModels>();
        private List<IssueViewModels> Correctiveactionlist = new List<IssueViewModels>();

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

        public void StoreIssue()
        {
            var sql = "insert into Issue(ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,Creator,AlertEmailUpdateDate,RelativePeoples,ModuleSN,ErrAbbr,DataID) values('<ProjectKey>','<IssueKey>','<IssueType>',N'<Summary>','<Priority>','<DueDate>','<ResolvedDate>','<ReportDate>','<Assignee>','<Reporter>','<Resolution>','<Creator>','<AlertEmailUpdateDate>','<RelativePeoples>','<ModuleSN>','<ErrAbbr>','<DataID>')";
            sql = sql.Replace("<ProjectKey>", ProjectKey).Replace("<IssueKey>", IssueKey).Replace("<IssueType>", IssueType)
                .Replace("<Summary>", Summary).Replace("<Priority>", Priority).Replace("<DueDate>", DueDate.ToString())
                .Replace("<ResolvedDate>", ResolvedDate.ToString()).Replace("<ReportDate>", ReportDate.ToString()).Replace("<Assignee>", Assignee)
                .Replace("<Reporter>", Reporter).Replace("<Resolution>", Resolution).Replace("<Creator>", Reporter)
                .Replace("<AlertEmailUpdateDate>", DateTime.Now.AddHours(-25).ToString()).Replace("<RelativePeoples>", RelativePeoples)
                .Replace("<ModuleSN>", ModuleSN).Replace("<ErrAbbr>", ErrAbbr).Replace("<DataID>", DataID);
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
            var sql = "insert into IssueOBA(IssueKey,FinisarDMR,OBAFailureRate,MaterialDisposition,ModuleSN,FVCode,APVal1) values('<IssueKey>','<FinisarDMR>','<OBAFailureRate>','<MaterialDisposition>','<ModuleSN>','<FVCode>','<ProductType>')";
            sql = sql.Replace("<IssueKey>", IssueKey).Replace("<FinisarDMR>", FinisarDMR).Replace("<OBAFailureRate>", OBAFailureRate).Replace("<FVCode>", FVCode)
                .Replace("<MaterialDisposition>", MaterialDisposition).Replace("<ModuleSN>", ModuleSN).Replace("<ProductType>", ProductType);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        private void StoreQuality()
        {
            var sql = "insert into IssueAttribute(IssueKey,APVal1,APVal2,APVal3,APVal4) values('<IssueKey>','<RMAFailureCode>','<ProductType>','<AffectRange>','<MaterialDisposition>')";
            sql = sql.Replace("<IssueKey>", IssueKey).Replace("<RMAFailureCode>", RMAFailureCode).Replace("<ProductType>", ProductType).Replace("<AffectRange>", AffectRange)
                .Replace("<MaterialDisposition>", MaterialDisposition);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

    //    QualType { set; get; }
    //public string RequestID { set; get; }
    //public string LineCategory { set; get; }
    //public string TestType { set; get; }
    //public string FailureInterval { set; get; }
    //public string FailQTY { set; get; }
    //public string TotalQTY { set; get; }
    //public string Location { set; get; }

        private void StoreReliability()
        {
            var sql = "insert into IssueAttribute(IssueKey,APVal1,APVal2,APVal3,APVal4,APVal5,APVal6,APVal7,APVal8,APVal9,APVal10) "
                + " values('<IssueKey>','<QualType>','<RequestID>','<LineCategory>','<ProductType>','<TestType>','<FailureInterval>','<FailQTY>','<TotalQTY>','<Location>','<FVCode>')";
            sql = sql.Replace("<IssueKey>", IssueKey).Replace("<QualType>", QualType).Replace("<RequestID>", RequestID).Replace("<LineCategory>", LineCategory)
                .Replace("<ProductType>", ProductType).Replace("<TestType>", TestType).Replace("<FailureInterval>", FailureInterval).Replace("<FailQTY>", FailQTY)
                .Replace("<TotalQTY>", TotalQTY).Replace("<Location>", Location).Replace("<FVCode>", FVCode);
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

        private void RetrieveReliability()
        {
            var sql = "select APVal1,APVal2,APVal3,APVal4,APVal5,APVal6,APVal7,APVal8,APVal9,APVal10 from IssueAttribute where IssueKey = '<IssueKey>'";
            sql = sql.Replace("<IssueKey>", IssueKey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            if (dbret.Count > 0)
            {
                QualType = Convert.ToString(dbret[0][0]);
                RequestID = Convert.ToString(dbret[0][1]);
                LineCategory = Convert.ToString(dbret[0][2]);
                ProductType = Convert.ToString(dbret[0][3]);
                TestType = Convert.ToString(dbret[0][4]);
                FailureInterval = Convert.ToString(dbret[0][5]);
                FailQTY = Convert.ToString(dbret[0][6]);
                TotalQTY = Convert.ToString(dbret[0][7]);
                Location = Convert.ToString(dbret[0][8]);
                FVCode = Convert.ToString(dbret[0][9]);
            }
        }

        private void StoreRMA()
        {

            var sql = "insert into IssueRMA(IssueKey,FinisarRMA,FinisarModel,ECustomer,CRMANUM,CReport,ModuleSN,RMAFailureCode,FVCode) values('<IssueKey>','<FinisarRMA>','<FinisarModel>','<ECustomer>','<CRMANUM>','<CReport>','<ModuleSN>','<RMAFailureCode>','<FVCode>')";
            sql = sql.Replace("<IssueKey>", IssueKey).Replace("<FinisarRMA>", FinisarRMA).Replace("<RMAFailureCode>", RMAFailureCode).Replace("<FVCode>", FVCode)
                .Replace("<FinisarModel>", FinisarModel).Replace("<ECustomer>", ECustomer)
                .Replace("<CRMANUM>", CRMANUM).Replace("<CReport>", CReport).Replace("<ModuleSN>", ModuleSN);
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

        public static void RemoveIssue(string issuekey)
        {
            try
            {
                var subissues = RetrieveSubIssue(issuekey);
                var sql = "";

                foreach (var item in subissues)
                {
                    sql = "update Issue  set APVal1 = 'delete' where IssueKey = '<IssueKey>'";
                    sql = sql.Replace("<IssueKey>", item.IssueKey);
                    DBUtility.ExeLocalSqlNoRes(sql);

                    //sql = "delete from IssueRMA where IssueKey = '<IssueKey>'";
                    //sql = sql.Replace("<IssueKey>", item.IssueKey);
                    //DBUtility.ExeLocalSqlNoRes(sql);

                    //sql = "delete from IssueAttachment where IssueKey = '<IssueKey>'";
                    //sql = sql.Replace("<IssueKey>", item.IssueKey);
                    //DBUtility.ExeLocalSqlNoRes(sql);

                    //sql = "delete from IssueComments where IssueKey = '<IssueKey>'";
                    //sql = sql.Replace("<IssueKey>", item.IssueKey);
                    //DBUtility.ExeLocalSqlNoRes(sql);
                }

                sql = "update Issue  set APVal1 = 'delete' where IssueKey = '<IssueKey>'";
                sql = sql.Replace("<IssueKey>", issuekey);
                DBUtility.ExeLocalSqlNoRes(sql);

                //sql = "delete from IssueRMA where IssueKey = '<IssueKey>'";
                //sql = sql.Replace("<IssueKey>", issuekey);
                //DBUtility.ExeLocalSqlNoRes(sql);

                //sql = "delete from IssueAttachment where IssueKey = '<IssueKey>'";
                //sql = sql.Replace("<IssueKey>", issuekey);
                //DBUtility.ExeLocalSqlNoRes(sql);

                //sql = "delete from IssueComments where IssueKey = '<IssueKey>'";
                //sql = sql.Replace("<IssueKey>", issuekey);
                //DBUtility.ExeLocalSqlNoRes(sql);
            }
            catch (Exception ex)
            {

            }

        }

        public void StoreSubIssue()
        {
            var sql = "insert into Issue(ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,ParentIssueKey,Creator,AlertEmailUpdateDate,RelativePeoples) values('<ProjectKey>','<IssueKey>','<IssueType>',N'<Summary>','<Priority>','<DueDate>','<ResolvedDate>','<ReportDate>','<Assignee>','<Reporter>','<Resolution>','<ParentIssueKey>','<Creator>','<AlertEmailUpdateDate>','<RelativePeoples>')";
            sql = sql.Replace("<ProjectKey>", ProjectKey).Replace("<IssueKey>", IssueKey).Replace("<IssueType>", IssueType)
                .Replace("<Summary>", Summary).Replace("<Priority>", Priority).Replace("<DueDate>", DueDate.ToString())
                .Replace("<ResolvedDate>", ResolvedDate.ToString()).Replace("<ReportDate>", ReportDate.ToString()).Replace("<Assignee>", Assignee)
                .Replace("<Reporter>", Reporter).Replace("<Resolution>", Resolution).Replace("<ParentIssueKey>", ParentIssueKey).Replace("<Creator>", Reporter)
                .Replace("<AlertEmailUpdateDate>", DateTime.Now.AddHours(-6).ToString()).Replace("<RelativePeoples>", RelativePeoples);
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

                var sql = "insert into IssueComments(IssueKey,Comment,Reporter,CommentDate,CommentType) values('<IssueKey>','<Comment>','<Reporter>','<CommentDate>','<CommentType>')";
                sql = sql.Replace("<IssueKey>", IssueKey).Replace("<Comment>", dbDescription)
                    .Replace("<Reporter>", Reporter).Replace("<CommentDate>", CommentDate).Replace("<CommentType>", CommentType);
                DBUtility.ExeLocalSqlNoRes(sql);
            }
        }

        public static void StoreIssueComment(string isk,string desc,string rept,string cmtype)
        {
            var sql = "insert into IssueComments(IssueKey,Comment,Reporter,CommentDate,CommentType) values('<IssueKey>','<Comment>','<Reporter>','<CommentDate>','<CommentType>')";
            sql = sql.Replace("<IssueKey>", isk).Replace("<Comment>", desc).Replace("<Reporter>", rept)
                .Replace("<CommentDate>", DateTime.Now.ToString()).Replace("<CommentType>", cmtype);
            DBUtility.ExeLocalSqlNoRes(sql);
        }


        public static void StoreIssueAttachment(string issuekey,string attachmenturl)
        {
            var sql = "insert into IssueAttachment(IssueKey,Attachment,UpdateTime) values('<IssueKey>',N'<Attachment>','<UpdateTime>')";
            sql = sql.Replace("<IssueKey>", issuekey).Replace("<Attachment>", attachmenturl).Replace("<UpdateTime>", DateTime.Now.ToString());
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public void RetrieveAttachment(string issuekey)
        {
            var ret = new List<string>();
            var csql = "select Attachment from IssueAttachment where IssueKey = '<IssueKey>' order by UpdateTime ASC";
            csql = csql.Replace("<IssueKey>", issuekey);

            var cdbret = DBUtility.ExeLocalSqlWithRes(csql,null);
            foreach (var r in cdbret)
            {
                ret.Add(Convert.ToString(r[0]));
            }

            AttachList = ret;
        }

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
            //IssueAttribute(IssueKey,APVal1,APVal2,APVal3,APVal4,APVal5,APVal6,APVal7,APVal8,APVal9,APVal10) "
            //" values('<IssueKey>','<QualType>','<RequestID>','<LineCategory>','<ProductType>','<TestType>','<FailureInterval>','<FailQTY>','<TotalQTY>','<Location>','<FVCode>')

            var sql = "update IssueAttribute set APVal1 = '<QualType>',APVal2 = '<RequestID>',APVal3 = '<LineCategory>',APVal4 = '<ProductType>',APVal5 = '<TestType>',APVal10 = '<FVCode>' where IssueKey = '<IssueKey>'";
            sql = sql.Replace("<IssueKey>", IssueKey).Replace("<QualType>", QualType)
                .Replace("<RequestID>", RequestID).Replace("<LineCategory>", LineCategory)
                .Replace("<ProductType>", ProductType).Replace("<TestType>", TestType).Replace("<FVCode>", FVCode);
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

        public static List<IssueViewModels> RetrieveSubIssue(string parentkey)
        {
            var ret = new List<IssueViewModels>();
            var sql = "select ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples from Issue where APVal1 <> 'delete' and ParentIssueKey = '<ParentIssueKey>'";
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


                tempvm.RetrieveComment();

                //var tempclist = new List<IssueComments>();
                //var csql = "select IssueKey,Comment,Reporter,CommentDate,CommentType from IssueComments where IssueKey = '<IssueKey>' order by CommentDate ASC";
                //csql = csql.Replace("<IssueKey>", tempvm.IssueKey);

                //var cdbret = DBUtility.ExeLocalSqlWithRes(csql,null);
                //foreach (var r in cdbret)
                //{
                //    var tempcomment = new IssueComments();
                //    tempcomment.IssueKey = Convert.ToString(r[0]);
                //    tempcomment.dbComment = Convert.ToString(r[1]);
                //    tempcomment.Reporter = Convert.ToString(r[2]);
                //    tempcomment.CommentDate = DateTime.Parse(Convert.ToString(r[3]));
                //    tempcomment.CommentType = Convert.ToString(r[4]);
                //    tempclist.Add(tempcomment);
                //}
                //tempvm.CommentList = tempclist;

                ret.Add(tempvm);
            }
            return ret;
        }

        public static IssueViewModels RetrieveIssueByIssueKey(string issuekey)
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

                ret.RetrieveComment();

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

                ret.SubIssues = RetrieveSubIssue(ret.IssueKey);
                ret.RetrieveAttachment(ret.IssueKey);

                return ret;
            }
            else
                return null;
        }

        public static List<IssueViewModels> RRetrieveFAByPjkey(string pjkey, string issuestatus,int topnum)
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

            var sql = "select top <topnum> ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,ParentIssueKey,RelativePeoples,APVal2,ModuleSN from Issue where  APVal1 <> 'delete' and  ParentIssueKey = '' and ProjectKey = '<ProjectKey>' and Resolution in <cond> and Creator = 'System' and IssueType <> '<IssueType>' order by ReportDate DESC";
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

                ret.RetrieveComment();
                ret.RetrieveAttachment(ret.IssueKey);
                retdict.Add( ret);
            }

            return retdict;
        }

        public static List<IssueViewModels> RRetrieveFABySN(string pjkey,string SN,string whichtest)
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

                retdict.Add(ret);
            }

            return retdict;
        }

        public static List<IssueViewModels> RRetrieveDupFABySN(string pjkey, string SN,string datestr)
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

            //.Replace("<BICond>", " @Burn-In Step ")

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


        public static List<IssueViewModels> RRetrieveBIFABySN(string pjkey, string SN)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectkey"></param>
        /// <param name="issuestatus"> comes from the resolution has pending,working,done three status</param>
        /// <returns></returns>
        /// 
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

            var sql = "select  ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples from Issue where APVal1 <> 'delete' and  ProjectKey = '<ProjectKey>' and Resolution in <cond> and  ParentIssueKey = '' and IssueType <> '<IssueType1>' and IssueType <> '<IssueType2>' and IssueType <> '<IssueType3>' and IssueType <> '<IssueType4>' and Creator <> 'System' order by ReportDate DESC";
            sql = sql.Replace("<ProjectKey>", projectkey).Replace("<cond>", cond)
                .Replace("<IssueType1>", ISSUETP.NPIPROC).Replace("<IssueType2>", ISSUETP.RMA)
                .Replace("<IssueType3>", ISSUETP.OBA).Replace("<IssueType4>", ISSUETP.Quality);

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
            var sql = "select  ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples,ErrAbbr from Issue where APVal1 <> 'delete' and  ProjectKey = '<ProjectKey>' and  ParentIssueKey = '' and IssueType <> '<IssueType>' and ReportDate > '<ReportDate>' order by ReportDate ASC";
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
                tempvm.ErrAbbr = Convert.ToString(line[12]);

                ret.Add(tempvm);
            }

            return ret;
        }

        public static List<IssueViewModels> RetrieveIssueForSolveSummary(string projectkey, string startdate)
        {
            var sql = "select  ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples from Issue where APVal1 <> 'delete' and  ProjectKey = '<ProjectKey>' and  ParentIssueKey = '' and IssueType <> '<IssueType>' and ResolvedDate > '<ResolvedDate>' order by ResolvedDate ASC";
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

                ret.Add(tempvm);
            }

            return ret;
        }


        public static List<IssueViewModels> RetrieveIssueAllByUser(string user, string startdate)
        {
            var sql = "select  ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples from Issue where APVal1 <> 'delete' and Resolution <> '<Resolution>' and  Assignee = '<Assignee>' and  ParentIssueKey = '' and IssueType <> '<IssueType>' and ReportDate > '<ReportDate>' order by ReportDate ASC";
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

                tempvm.RetrieveComment();
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

        private void RetrieveComment()
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
                CommentList = tempclist;
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

            var sql = "select  ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples from Issue where APVal1 <> 'delete' and  Assignee = '<Assignee>' and Resolution in <cond> and  ParentIssueKey = '' and IssueType <> '<IssueType>' and ReportDate > '<ReportDate>' order by ReportDate ASC";
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

                tempvm.RetrieveAttachment(tempvm.IssueKey);

                ret.Add(tempvm);
            }

            return ret;
        }

        public static List<IssueViewModels> RetrieveIssueWorkingByUser(string user, string startdate)
        {
            var cond = "('" + Resolute.Working + "')";

            var sql = "select  ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples from Issue where APVal1 <> 'delete' and  Assignee = '<Assignee>' and Resolution in <cond> and  ParentIssueKey = '' and IssueType <> '<IssueType>' and ReportDate > '<ReportDate>' order by ReportDate ASC";
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

                tempvm.RetrieveAttachment(tempvm.IssueKey);

                ret.Add(tempvm);
            }

            return ret;
        }

        public static List<IssueViewModels> RetrieveIssueDoneByUser(string user, string startdate)
        {
            var sql = "select  ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples from Issue where APVal1 <> 'delete' and Resolution <> '<Resolution>' and  Assignee = '<Assignee>' and  ParentIssueKey = '' and IssueType <> '<IssueType>' and ResolvedDate > '<ResolvedDate>' order by ResolvedDate ASC";
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

                tempvm.RetrieveComment();
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

        //public static List<IssueViewModels> RetrieveRMAByProjectKey(string projectkey, string issuestatus)
        //{
        //    var cond = "";
        //    var fixresolve = "";
        //    if (string.Compare(issuestatus, Resolute.Pending) == 0)
        //    {
        //        cond = "('" + Resolute.Pending + "','" + Resolute.Reopen + "')";
        //        fixresolve = Resolute.Pending;
        //    }
        //    else if (string.Compare(issuestatus, Resolute.Working) == 0)
        //    {
        //        cond = "('" + Resolute.Working + "')";
        //        fixresolve = Resolute.Working;
        //    }
        //    else
        //    {
        //        cond = "('" + Resolute.Fixed + "','" + Resolute.Done + "','" + Resolute.NotFix + "','" + Resolute.NotReproduce + "','" + Resolute.Unresolved + "')";
        //        fixresolve = Resolute.Done;
        //    }

        //    var sql = "select  ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples,APVal2 from Issue where APVal1 <> 'delete' and  ProjectKey = '<ProjectKey>' and Resolution in <cond> and  ParentIssueKey = '' and IssueType = '<IssueType>' order by ReportDate DESC";
        //    sql = sql.Replace("<ProjectKey>", projectkey).Replace("<cond>", cond).Replace("<IssueType>", ISSUETP.RMA);
        //    var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
        //    var ret = new List<IssueViewModels>();
        //    foreach (var line in dbret)
        //    {
        //        var tempsolve = fixresolve;
        //        if (string.Compare(Convert.ToString(line[10]), Resolute.Reopen) == 0)
        //            tempsolve = Resolute.Reopen;

        //        var tempvm = new IssueViewModels(Convert.ToString(line[0])
        //            , Convert.ToString(line[1]), Convert.ToString(line[2])
        //            , Convert.ToString(line[3]), Convert.ToString(line[4])
        //            , Convert.ToString(line[5]), Convert.ToString(line[6])
        //            , Convert.ToString(line[7]), Convert.ToString(line[8])
        //            , Convert.ToString(line[9]), tempsolve, "", Convert.ToString(line[11]));
        //        tempvm.LYT = Convert.ToString(Convert.ToString(line[12]));

        //        tempvm.RetrieveRMA();

        //        tempvm.RetrieveComment();

        //        tempvm.SubIssues = RetrieveSubIssue(tempvm.IssueKey);

        //        tempvm.RetrieveAttachment(tempvm.IssueKey);

        //        ret.Add(tempvm);
        //    }

        //    return ret;
        //}

        //public static List<IssueViewModels> RetrieveRMAByProjectKey(string projectkey, string StartDate, string EndDate)
        //{
        //    var sql = "";
        //    if (string.Compare(StartDate, "NONE", true) == 0)
        //    {
        //        sql = "select  ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples from Issue where APVal1 <> 'delete' and  ProjectKey = '<ProjectKey>' and  ParentIssueKey = '' and IssueType = '<IssueType>' order by ReportDate DESC";
        //        sql = sql.Replace("<ProjectKey>", projectkey).Replace("<IssueType>", ISSUETP.RMA);
        //    }
        //    else
        //    {
        //        sql = "select  ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples from Issue where APVal1 <> 'delete' and  ProjectKey = '<ProjectKey>' and  ParentIssueKey = '' and IssueType = '<IssueType>' and ReportDate >= '<StartDate>' and ReportDate <= '<EndDate>' order by ReportDate DESC";
        //        sql = sql.Replace("<ProjectKey>", projectkey).Replace("<IssueType>", ISSUETP.RMA).Replace("<StartDate>", StartDate).Replace("<EndDate>", DateTime.Parse(EndDate).AddDays(1).ToString());
        //    }

        //    var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
        //    var ret = new List<IssueViewModels>();
        //    foreach (var line in dbret)
        //    {

        //        var tempvm = new IssueViewModels(Convert.ToString(line[0])
        //            , Convert.ToString(line[1]), Convert.ToString(line[2])
        //            , Convert.ToString(line[3]), Convert.ToString(line[4])
        //            , Convert.ToString(line[5]), Convert.ToString(line[6])
        //            , Convert.ToString(line[7]), Convert.ToString(line[8])
        //            , Convert.ToString(line[9]), Convert.ToString(line[10]), "", Convert.ToString(line[11]));

        //        tempvm.RetrieveRMA();

        //        tempvm.RetrieveComment();

        //        tempvm.SubIssues = RetrieveSubIssue(tempvm.IssueKey);

        //        tempvm.RetrieveAttachment(tempvm.IssueKey);

        //        ret.Add(tempvm);
        //    }

        //    return ret;
        //}

        //public static List<IssueViewModels> RetrieveAllRMAIssue(string StartDate, string EndDate)
        //{
        //    var sql = "";
        //    if (string.Compare(StartDate, "NONE", true) == 0)
        //    {
        //        sql = "select  ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples from Issue where APVal1 <> 'delete' and  ParentIssueKey = '' and IssueType = '<IssueType>' order by ReportDate DESC";
        //        sql = sql.Replace("<IssueType>", ISSUETP.RMA);
        //    }
        //    else
        //    {
        //        sql = "select ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples from Issue where APVal1 <> 'delete' and  ParentIssueKey = '' and IssueType = '<IssueType>' and ReportDate >= '<StartDate>' and ReportDate <= '<EndDate>' order by ReportDate DESC";
        //        sql = sql.Replace("<IssueType>", ISSUETP.RMA).Replace("<StartDate>", StartDate).Replace("<EndDate>", DateTime.Parse(EndDate).AddDays(1).ToString());
        //    }

        //    var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
        //    var ret = new List<IssueViewModels>();
        //    foreach (var line in dbret)
        //    {

        //        var tempvm = new IssueViewModels(Convert.ToString(line[0])
        //            , Convert.ToString(line[1]), Convert.ToString(line[2])
        //            , Convert.ToString(line[3]), Convert.ToString(line[4])
        //            , Convert.ToString(line[5]), Convert.ToString(line[6])
        //            , Convert.ToString(line[7]), Convert.ToString(line[8])
        //            , Convert.ToString(line[9]), Convert.ToString(line[10]), "", Convert.ToString(line[11]));

        //        tempvm.RetrieveRMA();

        //        tempvm.RetrieveComment();

        //        tempvm.SubIssues = RetrieveSubIssue(tempvm.IssueKey);

        //        tempvm.RetrieveAttachment(tempvm.IssueKey);

        //        ret.Add(tempvm);
        //    }

        //    return ret;
        //}


        public static List<IssueViewModels> RetrieveIssueTypeByProjectKey(string projectkey, string issuestatus,string issuetype)
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

            var sql = "select  ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples,APVal2 from Issue where APVal1 <> 'delete' and  ProjectKey = '<ProjectKey>' and Resolution in <cond> and  ParentIssueKey = '' and IssueType = '<IssueType>' order by DueDate DESC";
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
                tempvm.LYT = Convert.ToString(Convert.ToString(line[12]));

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

                tempvm.RetrieveComment();

                tempvm.SubIssues = RetrieveSubIssue(tempvm.IssueKey);

                tempvm.RetrieveAttachment(tempvm.IssueKey);

                ret.Add(tempvm);
            }

            return ret;
        }

        public static List<IssueViewModels> RetrieveIssueTypeByProjectKey(string projectkey, string StartDate, string EndDate, string issuetype)
        {
            var sql = "";
            if (string.Compare(StartDate, "NONE", true) == 0)
            {
                sql = "select  ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples from Issue where APVal1 <> 'delete' and  ProjectKey = '<ProjectKey>' and  ParentIssueKey = '' and IssueType = '<IssueType>' order by DueDate DESC";
                sql = sql.Replace("<ProjectKey>", projectkey).Replace("<IssueType>", issuetype);
            }
            else
            {
                if (string.Compare(issuetype, ISSUETP.RMA, true) == 0
                    || string.Compare(issuetype, ISSUETP.Quality, true) == 0)
                {
                    sql = "select  ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples from Issue where APVal1 <> 'delete' and  ProjectKey = '<ProjectKey>' and  ParentIssueKey = '' and IssueType = '<IssueType>' and DueDate >= '<StartDate>' and DueDate <= '<EndDate>' order by DueDate DESC";
                    sql = sql.Replace("<ProjectKey>", projectkey).Replace("<IssueType>", issuetype).Replace("<StartDate>", DateTime.Parse(StartDate).AddDays(12).ToString()).Replace("<EndDate>", DateTime.Parse(EndDate).AddDays(13).ToString());
                }
                else
                {
                    sql = "select  ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples from Issue where APVal1 <> 'delete' and  ProjectKey = '<ProjectKey>' and  ParentIssueKey = '' and IssueType = '<IssueType>' and DueDate >= '<StartDate>' and DueDate <= '<EndDate>' order by DueDate DESC";
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

                tempvm.RetrieveComment();

                tempvm.SubIssues = RetrieveSubIssue(tempvm.IssueKey);

                tempvm.RetrieveAttachment(tempvm.IssueKey);

                ret.Add(tempvm);
            }

            return ret;
        }

        public static List<IssueViewModels> RetrieveAllIssueTypeIssue(string StartDate, string EndDate, string issuetype)
        {
            var sql = "";
            if (string.Compare(StartDate, "NONE", true) == 0)
            {
                sql = "select  ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples from Issue where APVal1 <> 'delete' and  ParentIssueKey = '' and IssueType = '<IssueType>' order by DueDate DESC";
                sql = sql.Replace("<IssueType>", issuetype);
            }
            else
            {
                if (string.Compare(issuetype, ISSUETP.RMA, true) == 0 
                    || string.Compare(issuetype, ISSUETP.Quality, true) == 0)
                {
                    sql = "select ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples from Issue where APVal1 <> 'delete' and  ParentIssueKey = '' and IssueType = '<IssueType>' and DueDate >= '<StartDate>' and DueDate <= '<EndDate>' order by DueDate DESC";
                    sql = sql.Replace("<IssueType>", issuetype).Replace("<StartDate>", DateTime.Parse(StartDate).AddDays(12).ToString()).Replace("<EndDate>", DateTime.Parse(EndDate).AddDays(13).ToString());
                }
                else
                {
                    sql = "select ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples from Issue where APVal1 <> 'delete' and  ParentIssueKey = '' and IssueType = '<IssueType>' and DueDate >= '<StartDate>' and DueDate <= '<EndDate>' order by DueDate DESC";
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

                tempvm.RetrieveComment();

                tempvm.SubIssues = RetrieveSubIssue(tempvm.IssueKey);

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

            var sql = "select ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples,AlertEmailUpdateDate from Issue where APVal1 <> 'delete' and  ProjectKey = '<ProjectKey>' and Resolution in <cond> and  ParentIssueKey = '' and IssueType = '<IssueType>' and DueDate >= '<stattime>' and DueDate <= '<endtime>' ";
            sql = sql.Replace("<ProjectKey>", projectkey).Replace("<cond>", cond).Replace("<IssueType>", ISSUETP.RMA).Replace("<stattime>", stattime).Replace("<endtime>", endtime);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            var ret = new List<IssueViewModels>();
            foreach (var line in dbret)
            {
                var alertdate = DateTime.Parse(Convert.ToString(line[12]));

                if (string.Compare(alertdate.ToString("yyyy-MM-dd"), DateTime.Now.ToString("yyyy-MM-dd")) != 0)
                {
                    var tempvm = new IssueViewModels(Convert.ToString(line[0])
                        , Convert.ToString(line[1]), Convert.ToString(line[2])
                        , Convert.ToString(line[3]), Convert.ToString(line[4])
                        , Convert.ToString(line[5]), Convert.ToString(line[6])
                        , Convert.ToString(line[7]), Convert.ToString(line[8])
                        , Convert.ToString(line[9]), fixresolve, "", Convert.ToString(line[11]));
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


            var sql = "select ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples,AlertEmailUpdateDate from Issue where APVal1 <> 'delete' and  ProjectKey = '<ProjectKey>' and Resolution in <cond> and  ParentIssueKey = '' and IssueType = '<IssueType>' and DueDate >= '<stattime>' and DueDate <= '<endtime>' ";
            sql = sql.Replace("<ProjectKey>", projectkey).Replace("<cond>", cond).Replace("<IssueType>", ISSUETP.OBA).Replace("<stattime>", stattime).Replace("<endtime>", endtime);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            var ret = new List<IssueViewModels>();
            foreach (var line in dbret)
            {
                var alertdate = DateTime.Parse(Convert.ToString(line[12]));

                if (string.Compare(alertdate.ToString("yyyy-MM-dd"), DateTime.Now.ToString("yyyy-MM-dd")) != 0)
                {
                    var tempvm = new IssueViewModels(Convert.ToString(line[0])
                        , Convert.ToString(line[1]), Convert.ToString(line[2])
                        , Convert.ToString(line[3]), Convert.ToString(line[4])
                        , Convert.ToString(line[5]), Convert.ToString(line[6])
                        , Convert.ToString(line[7]), Convert.ToString(line[8])
                        , Convert.ToString(line[9]), fixresolve, "", Convert.ToString(line[11]));
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

            var sql = "select ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples,AlertEmailUpdateDate from Issue where APVal1 <> 'delete' and Resolution in <cond> and  ParentIssueKey = '' and IssueType = '<IssueType>' and DueDate >= '<stattime>' and DueDate <= '<endtime>' ";
            sql = sql.Replace("<cond>", cond).Replace("<IssueType>", ISSUETP.Task).Replace("<stattime>", stattime).Replace("<endtime>", endtime);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            var ret = new List<IssueViewModels>();
            foreach (var line in dbret)
            {
                var alertdate = DateTime.Parse(Convert.ToString(line[12]));

                if (string.Compare(alertdate.ToString("yyyy-MM-dd"), DateTime.Now.ToString("yyyy-MM-dd")) != 0)
                {
                    var tempvm = new IssueViewModels(Convert.ToString(line[0])
                        , Convert.ToString(line[1]), Convert.ToString(line[2])
                        , Convert.ToString(line[3]), Convert.ToString(line[4])
                        , Convert.ToString(line[5]), Convert.ToString(line[6])
                        , Convert.ToString(line[7]), Convert.ToString(line[8])
                        , Convert.ToString(line[9]), fixresolve, "", Convert.ToString(line[11]));
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

            var sql = "select top <topnum> ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples from Issue where APVal1 <> 'delete' and  Assignee = '<Assignee>' and Resolution in <cond> and IssueType <> '<IssueType1>' and  Reporter <> 'System' order by ReportDate DESC";
            sql = sql.Replace("<Assignee>", assignee).Replace("<topnum>", Convert.ToString(topnum)).Replace("<cond>", cond).Replace("<IssueType1>", ISSUETP.NPIPROC);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            var ret = new List<IssueViewModels>();
            foreach (var line in dbret)
            {
                ret.Add(new IssueViewModels(Convert.ToString(line[0])
                    , Convert.ToString(line[1]), Convert.ToString(line[2])
                    , Convert.ToString(line[3]), Convert.ToString(line[4])
                    , Convert.ToString(line[5]), Convert.ToString(line[6])
                    , Convert.ToString(line[7]), Convert.ToString(line[8])
                    , Convert.ToString(line[9]), fixresolve,"", Convert.ToString(line[11])));
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

            var sql = "select top <topnum> ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples from Issue where APVal1 <> 'delete' and  Assignee = '<Assignee>' and Resolution in <cond> order by ReportDate DESC";
            sql = sql.Replace("<Assignee>", assignee).Replace("<topnum>", Convert.ToString(topnum)).Replace("<cond>", cond).Replace("<IssueType>", ISSUETP.Bug);
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


        public static List<IssueViewModels> RetrieveIssueByCreator(string creator, int topnum)
        {
            var sql = "select top <topnum> ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples from Issue where APVal1 <> 'delete' and IssueType <> '<IssueType>' and  (Creator = '<Creator>' or Reporter = '<Creator>') and ParentIssueKey = '' order by ReportDate DESC";
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
                
                tempvm.SubIssues = RetrieveSubIssue(tempvm.IssueKey);
                ret.Add(tempvm);
            }

            return ret;
        }

        public static List<IssueViewModels> RetrieveIssueByCreator(string creator,string issuestatus)
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

            var sql = "select ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples from Issue where APVal1 <> 'delete' and IssueType <> '<IssueType>' and  (Creator = '<Creator>' or Reporter = '<Creator>') and ParentIssueKey = '' and Resolution in <cond>  order by ReportDate DESC";
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

                tempvm.SubIssues = RetrieveSubIssue(tempvm.IssueKey);
                ret.Add(tempvm);
            }

            return ret;
        }

        public static List<IssueViewModels> RetrieveNPIPROCIssue(string pjkey)
        {
            var sql = "select ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples from Issue where APVal1 <> 'delete' and  ProjectKey = '<ProjectKey>' and ParentIssueKey = '' and IssueType = '<IssueType>' order by DueDate ASC";
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

                tempvm.SubIssues = RetrieveSubIssue(tempvm.IssueKey);
                tempvm.RetrieveAttachment(tempvm.IssueKey);
                ret.Add(tempvm);
            }
            return ret;
        }

        public static List<IssueViewModels> SearchIssue(string pjkey,string type,string resolution,string asignee,string startdate,string enddate,string summary, int topnum)
        {
            var sql = "select top <topnum> ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples from Issue where APVal1 <> 'delete' and  <cond> order by ReportDate DESC";

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

        public static void DeleteAttachComment(string issuekey, string cond)
        {
            var csql = "select Comment from IssueComments where IssueKey = '<IssueKey>' and Comment = '<cond>'";
            csql = csql.Replace("<IssueKey>", issuekey).Replace("<cond>", cond);
            var cdbret = DBUtility.ExeLocalSqlWithRes(csql,null);
            if (cdbret.Count > 0 && cdbret.Count < 3)
            {
                csql = "delete from IssueComments where IssueKey = '<IssueKey>' and Comment = '<cond>'";
                csql = csql.Replace("<IssueKey>", issuekey).Replace("<cond>", cond);
                DBUtility.ExeLocalSqlNoRes(csql);
            }//end if
        }


        public static void StoreBIRootCause(string pjkey, string sn,string rootcause)
        {
            var csql = "delete from BIROOTCAUSE where ProjectKey = '<ProjectKey>' and ModuleSN = '<ModuleSN>'";
            csql = csql.Replace("<ProjectKey>", pjkey).Replace("<ModuleSN>", sn);
            DBUtility.ExeLocalSqlNoRes(csql);

            var sql = "insert into BIROOTCAUSE(ProjectKey,ModuleSN,RootCause) values('<ProjectKey>','<ModuleSN>','<RootCause>')";
            sql = sql.Replace("<ProjectKey>", pjkey).Replace("<ModuleSN>", sn).Replace("<RootCause>", rootcause);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static string RetrieveBIRootCause(string pjkey, string sn)
        {
            var csql = "select RootCause from BIROOTCAUSE where ProjectKey = '<ProjectKey>' and ModuleSN = '<ModuleSN>'";
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


        public static void CloseIssueAutomaticlly(string pjkey, string SN, string whichtest, string tester, string datestr)
        {
            var issues = IssueViewModels.RRetrieveFABySN(pjkey, SN, whichtest);
            foreach (var tobedata in issues)
            {
                tobedata.Resolution = Resolute.AutoClose;
                tobedata.Description = "Module " + SN + " passed " + whichtest + " test @" + tester + " @" + datestr;
                tobedata.UpdateIssue();
                tobedata.CloseIssue();
            }
        }

        public static void CloseIssueAutomaticllyWithFailedSN(string pjkey, string SN, string whichtest, string tester, string datestr)
        {
            var issues = IssueViewModels.RRetrieveFABySN(pjkey, SN, whichtest);
            foreach (var tobedata in issues)
            {
                tobedata.Resolution = Resolute.AutoClose;
                tobedata.Description = "Module " + SN + " faild for " + whichtest + " test @" + tester + " @" + datestr+" again";
                tobedata.UpdateIssue();
                tobedata.CloseIssue();
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
                        tobedata.Description = "Module " + item.ModuleSerialNum + " passed " + item.WhichTest + " test @" + item.TestStation + " @" + item.TestTimeStamp.ToString("yyyy-MM-dd hh:mm:ss");
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
    }
}