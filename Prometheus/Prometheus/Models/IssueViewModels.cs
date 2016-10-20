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
        public static string NPIPROC = "NPI PROCESS";
        public static string NewFeature = "New Feature";
        public static string Task = "Task";
        public static string Improvement = "Improvement";
        public static string Document = "Document";
    }

    public class ISSUEPR
    {
        public static string Major = "Major";
        public static string Blocker = "Blocker";
        public static string Critical = "Critical";
        public static string Minor = "Minor";
        public static string Trivial = "Trivial";
    }

    public class Resolute
    {
        public static string Pending = "Pending";
        public static string Working = "Working";
        public static string Reopen = "Reopen";
        public static string Fixed = "Fixed";
        public static string Done = "Done";
        public static string NotFix = "Will Not Fix";
        public static string Unresolved = "Unresolved";
        public static string NotReproduce ="Cannot Reproduce";
        public static string Started = "Started";

        public static string ColorStatus(string status)
        {
            if (string.Compare(status, Pending) == 0 || string.Compare(status, Reopen) == 0)
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

    public class COMMENTTYPE
    {
        public static string Description = "Description";
        public static string RootCause = "Root Cause";
        public static string CustomReport = "Report for Customer";
        public static string InternalReport = "Internal Report";
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
        { }

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
                foreach (var item in cemlist)
                {
                    if (string.Compare(item.CommentType,COMMENTTYPE.Description) == 0)
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
                }
                return Resolute.ColorStatus(Resolute.Done);
            }
            return Resolute.ColorStatus(Resolute.Pending);
        }

        #region RMA

        public string FinisarRMA { set; get; }

        public string RMAFailureCode { set; get; }

        public string FinisarModel { set; get; }
        public string ModuleSN { set; get; }
        public string ECustomer { set; get; }

        public string CRMANUM { set; get; }

        public string CReport { set; get; }

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
            var sql = "insert into Issue(ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,Creator,AlertEmailUpdateDate,RelativePeoples) values('<ProjectKey>','<IssueKey>','<IssueType>','<Summary>','<Priority>','<DueDate>','<ResolvedDate>','<ReportDate>','<Assignee>','<Reporter>','<Resolution>','<Creator>','<AlertEmailUpdateDate>','<RelativePeoples>')";
            sql = sql.Replace("<ProjectKey>", ProjectKey).Replace("<IssueKey>", IssueKey).Replace("<IssueType>", IssueType)
                .Replace("<Summary>", Summary).Replace("<Priority>", Priority).Replace("<DueDate>", DueDate.ToString())
                .Replace("<ResolvedDate>", ResolvedDate.ToString()).Replace("<ReportDate>", ReportDate.ToString()).Replace("<Assignee>", Assignee)
                .Replace("<Reporter>", Reporter).Replace("<Resolution>", Resolution).Replace("<Creator>", Reporter)
                .Replace("<AlertEmailUpdateDate>", DateTime.Now.AddHours(-25).ToString()).Replace("<RelativePeoples>", RelativePeoples);
            DBUtility.ExeLocalSqlNoRes(sql);

            StoreIssueComment(DateTime.Now.ToString());

            if (string.Compare(IssueType, ISSUETP.RMA) == 0)
            {
                StoreRMA();
            }
        }

        private void StoreRMA()
        {

            var sql = "insert into IssueRMA(IssueKey,FinisarRMA,FinisarModel,ECustomer,CRMANUM,CReport,ModuleSN,RMAFailureCode) values('<IssueKey>','<FinisarRMA>','<FinisarModel>','<ECustomer>','<CRMANUM>','<CReport>','<ModuleSN>','<RMAFailureCode>')";
            sql = sql.Replace("<IssueKey>", IssueKey).Replace("<FinisarRMA>", FinisarRMA).Replace("<RMAFailureCode>", RMAFailureCode)
                .Replace("<FinisarModel>", FinisarModel).Replace("<ECustomer>", ECustomer)
                .Replace("<CRMANUM>", CRMANUM).Replace("<CReport>", CReport).Replace("<ModuleSN>", ModuleSN);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        private void RetrieveRMA()
        {
            var sql = "select FinisarRMA,FinisarModel,ECustomer,CRMANUM,CReport,ModuleSN,RMAFailureCode from IssueRMA where IssueKey = '<IssueKey>'";
            sql = sql.Replace("<IssueKey>", IssueKey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            if (dbret.Count > 0)
            {
                FinisarRMA = Convert.ToString(dbret[0][0]);
                FinisarModel = Convert.ToString(dbret[0][1]);
                ECustomer = Convert.ToString(dbret[0][2]);
                CRMANUM = Convert.ToString(dbret[0][3]);
                CReport = Convert.ToString(dbret[0][4]);
                ModuleSN = Convert.ToString(dbret[0][5]);
                RMAFailureCode = Convert.ToString(dbret[0][6]);
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
                    sql = "delete from Issue where IssueKey = '<IssueKey>'";
                    sql = sql.Replace("<IssueKey>", item.IssueKey);
                    DBUtility.ExeLocalSqlNoRes(sql);

                    sql = "delete from IssueRMA where IssueKey = '<IssueKey>'";
                    sql = sql.Replace("<IssueKey>", item.IssueKey);
                    DBUtility.ExeLocalSqlNoRes(sql);

                    sql = "delete from IssueAttachment where IssueKey = '<IssueKey>'";
                    sql = sql.Replace("<IssueKey>", item.IssueKey);
                    DBUtility.ExeLocalSqlNoRes(sql);

                    sql = "delete from IssueComments where IssueKey = '<IssueKey>'";
                    sql = sql.Replace("<IssueKey>", item.IssueKey);
                    DBUtility.ExeLocalSqlNoRes(sql);
                }

                sql = "delete from Issue where IssueKey = '<IssueKey>'";
                sql = sql.Replace("<IssueKey>", issuekey);
                DBUtility.ExeLocalSqlNoRes(sql);

                sql = "delete from IssueRMA where IssueKey = '<IssueKey>'";
                sql = sql.Replace("<IssueKey>", issuekey);
                DBUtility.ExeLocalSqlNoRes(sql);

                sql = "delete from IssueAttachment where IssueKey = '<IssueKey>'";
                sql = sql.Replace("<IssueKey>", issuekey);
                DBUtility.ExeLocalSqlNoRes(sql);

                sql = "delete from IssueComments where IssueKey = '<IssueKey>'";
                sql = sql.Replace("<IssueKey>", issuekey);
                DBUtility.ExeLocalSqlNoRes(sql);
            }
            catch (Exception ex)
            {

            }

        }

        public void StoreSubIssue()
        {
            var sql = "insert into Issue(ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,ParentIssueKey,Creator,AlertEmailUpdateDate,RelativePeoples) values('<ProjectKey>','<IssueKey>','<IssueType>','<Summary>','<Priority>','<DueDate>','<ResolvedDate>','<ReportDate>','<Assignee>','<Reporter>','<Resolution>','<ParentIssueKey>','<Creator>','<AlertEmailUpdateDate>','<RelativePeoples>')";
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
            var sql = "insert into IssueAttachment(IssueKey,Attachment,UpdateTime) values('<IssueKey>','<Attachment>','<UpdateTime>')";
            sql = sql.Replace("<IssueKey>", issuekey).Replace("<Attachment>", attachmenturl).Replace("<UpdateTime>", DateTime.Now.ToString());
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public void RetrieveAttachment(string issuekey)
        {
            var ret = new List<string>();
            var csql = "select Attachment from IssueAttachment where IssueKey = '<IssueKey>' order by UpdateTime ASC";
            csql = csql.Replace("<IssueKey>", issuekey);

            var cdbret = DBUtility.ExeLocalSqlWithRes(csql);
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
            var sql = "update IssueRMA set CRMANUM = '<CRMANUM>',CReport = '<CReport>',ModuleSN = '<ModuleSN>',RMAFailureCode = '<RMAFailureCode>'  where IssueKey = '<IssueKey>'";
            sql = sql.Replace("<IssueKey>", IssueKey).Replace("<CRMANUM>", CRMANUM)
                .Replace("<CReport>", CReport).Replace("<ModuleSN>", ModuleSN).Replace("<RMAFailureCode>", RMAFailureCode);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public void UpdateIAssign()
        {
            var sql = "update Issue set Reporter = '<Reporter>' where IssueKey = '<IssueKey>'";
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
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
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
            var sql = "select ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples from Issue where ParentIssueKey = '<ParentIssueKey>'";
            sql = sql.Replace("<ParentIssueKey>", parentkey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var tempvm = new IssueViewModels(Convert.ToString(line[0])
                    , Convert.ToString(line[1]), Convert.ToString(line[2])
                    , Convert.ToString(line[3]), Convert.ToString(line[4])
                    , Convert.ToString(line[5]), Convert.ToString(line[6])
                    , Convert.ToString(line[7]), Convert.ToString(line[8])
                    , Convert.ToString(line[9]), Convert.ToString(line[10]), parentkey, Convert.ToString(line[11]));
                


                var tempclist = new List<IssueComments>();
                var csql = "select IssueKey,Comment,Reporter,CommentDate,CommentType from IssueComments where IssueKey = '<IssueKey>' order by CommentDate ASC";
                csql = csql.Replace("<IssueKey>", tempvm.IssueKey);

                var cdbret = DBUtility.ExeLocalSqlWithRes(csql);
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
                tempvm.CommentList = tempclist;

                ret.Add(tempvm);
            }
            return ret;
        }

        public static IssueViewModels RetrieveIssueByIssueKey(string issuekey)
        {
            var sql = "select ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,ParentIssueKey,RelativePeoples from Issue where IssueKey = '<IssueKey>'";
            sql = sql.Replace("<IssueKey>", issuekey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            if (dbret.Count > 0)
            {
                var ret = new IssueViewModels(Convert.ToString(dbret[0][0])
                    , Convert.ToString(dbret[0][1]), Convert.ToString(dbret[0][2])
                    , Convert.ToString(dbret[0][3]), Convert.ToString(dbret[0][4])
                    , Convert.ToString(dbret[0][5]), Convert.ToString(dbret[0][6])
                    , Convert.ToString(dbret[0][7]), Convert.ToString(dbret[0][8])
                    , Convert.ToString(dbret[0][9]), Convert.ToString(dbret[0][10])
                    , Convert.ToString(dbret[0][11]), Convert.ToString(dbret[0][12]));


                var tempclist = new List<IssueComments>();
                sql = "select IssueKey,Comment,Reporter,CommentDate,CommentType from IssueComments where IssueKey = '<IssueKey>' order by CommentDate ASC";
                sql = sql.Replace("<IssueKey>", issuekey);
                dbret = DBUtility.ExeLocalSqlWithRes(sql);
                foreach (var r in dbret)
                {
                    var tempcomment = new IssueComments();
                    tempcomment.IssueKey = Convert.ToString(r[0]);
                    tempcomment.dbComment = Convert.ToString(r[1]);
                    tempcomment.Reporter = Convert.ToString(r[2]);
                    tempcomment.CommentDate = DateTime.Parse(Convert.ToString(r[3]));
                    tempcomment.CommentType = Convert.ToString(r[4]);
                    tempclist.Add(tempcomment);
                }
                ret.CommentList = tempclist;

                if (string.Compare(ret.IssueType, ISSUETP.RMA) == 0)
                {
                    ret.RetrieveRMA();
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

            var sql = "select top <topnum> ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,ParentIssueKey,RelativePeoples from Issue where ProjectKey = '<ProjectKey>' and Resolution in <cond> and Creator = 'System' and IssueType <> '<IssueType>' order by ReportDate DESC";
            sql = sql.Replace("<ProjectKey>", pjkey).Replace("<cond>", cond).Replace("<topnum>", Convert.ToString(topnum))
                    .Replace("<IssueType>", ISSUETP.NPIPROC);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach(var line in dbret)
            {
                var ret = new IssueViewModels(Convert.ToString(line[0])
                    , Convert.ToString(line[1]), Convert.ToString(line[2])
                    , Convert.ToString(line[3]), Convert.ToString(line[4])
                    , Convert.ToString(line[5]), Convert.ToString(line[6])
                    , Convert.ToString(line[7]), Convert.ToString(line[8])
                    , Convert.ToString(line[9]), Convert.ToString(line[10])
                    , Convert.ToString(line[11]), Convert.ToString(line[12]));


                //var tempclist = new List<IssueComments>();
                //sql = "select IssueKey,Comment,Reporter,CommentDate,CommentType from IssueComments where IssueKey = '<IssueKey>' order by CommentDate ASC";
                //sql = sql.Replace("<IssueKey>", ret.IssueKey);
                //var cret = DBUtility.ExeLocalSqlWithRes(sql);
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

                ret.RetrieveAttachment(ret.IssueKey);
                retdict.Add( ret);
            }

            return retdict;
        }

        public static List<IssueViewModels> RRetrieveFABySN(string pjkey,string SN,string whichtest, string issuestatus)
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

            var sql = "select top  ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,ParentIssueKey,RelativePeoples from Issue where ProjectKey = '<ProjectKey>' and Summary like '%<ModuleSerialNum>%' and Summary like '%<whichtest>%'  and Resolution in <cond> and Creator = 'System' and IssueType <> '<IssueType1>' and IssueType <> '<IssueType2>' order by ReportDate DESC";
            sql = sql.Replace("<ModuleSerialNum>", SN).Replace("<cond>", cond).Replace("<whichtest>", whichtest).Replace("<ProjectKey>", pjkey)
                    .Replace("<IssueType1>", ISSUETP.NPIPROC).Replace("<IssueType2>", ISSUETP.RMA);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var ret = new IssueViewModels(Convert.ToString(line[0])
                    , Convert.ToString(line[1]), Convert.ToString(line[2])
                    , Convert.ToString(line[3]), Convert.ToString(line[4])
                    , Convert.ToString(line[5]), Convert.ToString(line[6])
                    , Convert.ToString(line[7]), Convert.ToString(line[8])
                    , Convert.ToString(line[9]), Convert.ToString(line[10])
                    , Convert.ToString(line[11]), Convert.ToString(line[12]));

                retdict.Add(ret);
            }

            return retdict;
        }

        public static Dictionary<string, IssueViewModels> RRetrieveFADictByPjkey(string pjkey, string issuestatus, int topnum)
        {
            var retdict = new Dictionary<string, IssueViewModels>();

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

            var sql = "select top <topnum> ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,ParentIssueKey,RelativePeoples from Issue where ProjectKey = '<ProjectKey>' and Resolution in <cond> and Creator = 'System' and IssueType <> '<IssueType>' order by ReportDate DESC";
            sql = sql.Replace("<ProjectKey>", pjkey).Replace("<cond>", cond).Replace("<topnum>", Convert.ToString(topnum))
                    .Replace("<IssueType>", ISSUETP.NPIPROC);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var ret = new IssueViewModels(Convert.ToString(line[0])
                    , Convert.ToString(line[1]), Convert.ToString(line[2])
                    , Convert.ToString(line[3]), Convert.ToString(line[4])
                    , Convert.ToString(line[5]), Convert.ToString(line[6])
                    , Convert.ToString(line[7]), Convert.ToString(line[8])
                    , Convert.ToString(line[9]), Convert.ToString(line[10])
                    , Convert.ToString(line[11]), Convert.ToString(line[12]));


                //var tempclist = new List<IssueComments>();
                //sql = "select IssueKey,Comment,Reporter,CommentDate,CommentType from IssueComments where IssueKey = '<IssueKey>' order by CommentDate ASC";
                //sql = sql.Replace("<IssueKey>", ret.IssueKey);
                //var cret = DBUtility.ExeLocalSqlWithRes(sql);
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

                ret.RetrieveAttachment(ret.IssueKey);
                retdict.Add(ret.IssueKey, ret);
            }

            return retdict;
        }

        //public static bool RetrieveFAStatusByIssueKey(string issuekey, string issuestatus)
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

        //    var sql = "select IssueKey from Issue where IssueKey = '<IssueKey>'  and Resolution in <cond>";
        //    sql = sql.Replace("<IssueKey>", issuekey).Replace("<cond>", cond);
        //    var dbret = DBUtility.ExeLocalSqlWithRes(sql);
        //    if (dbret.Count > 0)
        //    {
        //        return true;
        //    }
        //    return false;
        //}

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

            var sql = "select IssueKey from Issue where ProjectKey = '<ProjectKey>' and Resolution in <cond> and Creator = 'System' and IssueType <> '<IssueType>'";
            sql = sql.Replace("<ProjectKey>", pjkey).Replace("<cond>", cond).Replace("<IssueType>", ISSUETP.NPIPROC);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
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

            var sql = "select  ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples from Issue where ProjectKey = '<ProjectKey>' and Resolution in <cond> and  ParentIssueKey = '' and IssueType <> '<IssueType1>' and IssueType <> '<IssueType2>' and Creator <> 'System' order by ReportDate DESC";
            sql = sql.Replace("<ProjectKey>", projectkey).Replace("<cond>", cond).Replace("<IssueType1>", ISSUETP.NPIPROC).Replace("<IssueType2>", ISSUETP.RMA);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
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

            var sql = "select IssueKey from Issue where ProjectKey = '<ProjectKey>' and Resolution in <cond> and  ParentIssueKey = '' and IssueType <> '<IssueType1>' and IssueType <> '<IssueType2>' and Creator <> 'System' order by ReportDate DESC";
            sql = sql.Replace("<ProjectKey>", projectkey).Replace("<cond>", cond).Replace("<IssueType1>", ISSUETP.NPIPROC).Replace("<IssueType2>", ISSUETP.RMA);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            return dbret.Count;
        }

        public static List<IssueViewModels> RetrieveIssueForIncreaseSummary(string projectkey,string startdate)
        {
            var sql = "select  ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples from Issue where ProjectKey = '<ProjectKey>' and  ParentIssueKey = '' and IssueType <> '<IssueType>' and ReportDate > '<ReportDate>' order by ReportDate ASC";
            sql = sql.Replace("<ProjectKey>", projectkey).Replace("<ReportDate>", startdate).Replace("<IssueType>", ISSUETP.NPIPROC);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
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

        public static List<IssueViewModels> RetrieveIssueForSolveSummary(string projectkey, string startdate)
        {
            var sql = "select  ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples from Issue where ProjectKey = '<ProjectKey>' and  ParentIssueKey = '' and IssueType <> '<IssueType>' and ResolvedDate > '<ResolvedDate>' order by ResolvedDate ASC";
            sql = sql.Replace("<ProjectKey>", projectkey).Replace("<ResolvedDate>", startdate).Replace("<IssueType>", ISSUETP.NPIPROC);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
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


        public static List<IssueViewModels> RetrieveRMAByProjectKey(string projectkey, string issuestatus)
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

            var sql = "select  ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples from Issue where ProjectKey = '<ProjectKey>' and Resolution in <cond> and  ParentIssueKey = '' and IssueType = '<IssueType>' order by ReportDate DESC";
            sql = sql.Replace("<ProjectKey>", projectkey).Replace("<cond>", cond).Replace("<IssueType>", ISSUETP.RMA);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
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

                tempvm.RetrieveRMA();

                var tempclist = new List<IssueComments>();
                var csql = "select IssueKey,Comment,Reporter,CommentDate,CommentType from IssueComments where IssueKey = '<IssueKey>' order by CommentDate ASC";
                csql = csql.Replace("<IssueKey>", tempvm.IssueKey);
                var cdbret = DBUtility.ExeLocalSqlWithRes(csql);
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
                tempvm.CommentList = tempclist;
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
            var stattime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " 00:00:00").AddDays(10).ToString();
            var endtime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59").AddDays(10).ToString();

            if (!fv)
            {
                stattime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " 00:00:00").AddDays(5).ToString();
                endtime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59").AddDays(5).ToString();
            }

            var sql = "select ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples,AlertEmailUpdateDate from Issue where ProjectKey = '<ProjectKey>' and Resolution in <cond> and  ParentIssueKey = '' and IssueType = '<IssueType>' and DueDate >= '<stattime>' and DueDate <= '<endtime>' ";
            sql = sql.Replace("<ProjectKey>", projectkey).Replace("<cond>", cond).Replace("<IssueType>", ISSUETP.RMA).Replace("<stattime>", stattime).Replace("<endtime>", endtime);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
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

            var sql = "select IssueKey from Issue where ProjectKey = '<ProjectKey>' and Resolution in <cond> and  ParentIssueKey = '' and IssueType = '<IssueType>' order by ReportDate DESC";
            sql = sql.Replace("<ProjectKey>", projectkey).Replace("<cond>", cond).Replace("<IssueType>", ISSUETP.RMA);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
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

            var sql = "select top <topnum> ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples from Issue where Assignee = '<Assignee>' and Resolution in <cond> order by ReportDate DESC";
            sql = sql.Replace("<Assignee>", assignee).Replace("<topnum>", Convert.ToString(topnum)).Replace("<cond>", cond);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
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


        public static List<IssueViewModels> RetrieveIssueByCreator(string creator, int topnum)
        {
            var sql = "select top <topnum> ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples from Issue where (Creator = '<Creator>' or Reporter = '<Creator>') order by ReportDate DESC";
            sql = sql.Replace("<Creator>", creator).Replace("<topnum>", Convert.ToString(topnum));
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
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
            var sql = "select ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples from Issue where ProjectKey = '<ProjectKey>' and ParentIssueKey = '' and IssueType = '<IssueType>' order by DueDate ASC";
            sql = sql.Replace("<ProjectKey>", pjkey).Replace("<IssueType>", Convert.ToString(ISSUETP.NPIPROC));
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
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
            var sql = "select top <topnum> ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,RelativePeoples from Issue where <cond> order by ReportDate DESC";

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

            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
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
            var csql = "select Attachment from IssueAttachment where IssueKey = '<IssueKey>' and Attachment = '<cond>'";
            csql = csql.Replace("<IssueKey>", issuekey).Replace("<cond>", cond);
            var cdbret = DBUtility.ExeLocalSqlWithRes(csql);
            if (cdbret.Count > 0 && cdbret.Count < 3)
            {
                csql = "delete from IssueAttachment where IssueKey = '<IssueKey>' and Attachment = '<cond>'";
                csql = csql.Replace("<IssueKey>", issuekey).Replace("<cond>", cond);
                DBUtility.ExeLocalSqlNoRes(csql);
            }//end if
        }

        public static void DeleteComment(string issuekey, string cond)
        {
            var csql = "select Comment from IssueComments where IssueKey = '<IssueKey>' and Comment = '<cond>'";
            csql = csql.Replace("<IssueKey>", issuekey).Replace("<cond>", cond);
            var cdbret = DBUtility.ExeLocalSqlWithRes(csql);
            if (cdbret.Count > 0 && cdbret.Count < 3)
            {
                csql = "delete from IssueComments where IssueKey = '<IssueKey>' and Comment = '<cond>'";
                csql = csql.Replace("<IssueKey>", issuekey).Replace("<cond>", cond);
                DBUtility.ExeLocalSqlNoRes(csql);
            }//end if
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

            var sum = "Complete Firmware Architecture Review";
            var desc = "Complete Firmware Architecture Review";
            CreateNPISubIssue(projectname,pjkey, parentkey,firstengineer,sum,desc,3);

            sum = "EVT Presentation Completed and ECO Approved";
            desc = "EVT Presentation Completed and ECO Approved";
            CreateNPISubIssue(projectname, pjkey, parentkey, firstengineer, sum, desc,3);

            sum = "EVT Presentation Completed and ECO Approved";
            desc = "EVT Presentation Completed and ECO Approved";
            CreateNPISubIssue(projectname, pjkey, parentkey, firstengineer, sum, desc,3);

            sum = "Design Review Completed and ECO Approved";
            desc = "Design Review Completed and ECO Approved";
            CreateNPISubIssue(projectname, pjkey, parentkey, firstengineer, sum, desc,3);

            sum = "PVT type testing performed";
            desc = "PVT type testing performed. Results pass or corrective actions identified for failures";
            CreateNPISubIssue(projectname, pjkey, parentkey, firstengineer, sum, desc,3);

            sum = "IC - Prototype released";
            desc = "IC - Prototype released";
            CreateNPISubIssue(projectname, pjkey, parentkey, firstengineer, sum, desc,3);

            sum = "OSAs: internal passed EVT external Prototype released";
            desc = "OSAs: internal passed EVT external Prototype released";
            CreateNPISubIssue(projectname, pjkey, parentkey, firstengineer, sum, desc,3);
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

            var sum = "DVT Presentation Completed and ECO Approved";
            var desc = "DVT Presentation Completed and ECO Approved";
            CreateNPISubIssue(projectname, pjkey, parentkey, firstengineer, sum, desc, 6);

            sum = "Design Review Completed";
            desc = "Design Review Completed";
            CreateNPISubIssue(projectname, pjkey, parentkey, firstengineer, sum, desc, 6);

            sum = "Standard and Logic/Firmware PVT report is a pass";
            desc = "Standard and Logic/Firmware PVT report is a pass";
            CreateNPISubIssue(projectname, pjkey, parentkey, firstengineer, sum, desc, 6);

            sum = "Cpk from NPI Build Data shows Cpk > 0.7";
            desc = "Cpk from NPI Build Data shows Cpk > 0.7";
            CreateNPISubIssue(projectname, pjkey, parentkey, firstengineer, sum, desc, 6);

            sum = "Yield Requirements";
            desc = "Low Complexity : 1st Pass 50%, Cumulative without Rework 80%.Medium Complexity : 1st Pass 40%, Cumulative without Rework 75%.High Complexity : 1st Pass 25%, Cumulative without Rework 70%";
            CreateNPISubIssue(projectname, pjkey, parentkey, firstengineer, sum, desc, 6);

            sum = "IC - Pre-Production released";
            desc = "IC - Pre-Production released";
            CreateNPISubIssue(projectname, pjkey, parentkey, firstengineer, sum, desc, 6);

            sum = "OSAs: internal passed DVT, external Pre-Production released";
            desc = "OSAs: internal passed DVT, external Pre-Production released";
            CreateNPISubIssue(projectname, pjkey, parentkey, firstengineer, sum, desc, 6);
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

            var sum = "MVT Presentation Completed and ECO Approved";
            var desc = "MVT Presentation Completed and ECO Approved";
            CreateNPISubIssue(projectname, pjkey, parentkey, firstengineer, sum, desc, 9);

            sum = "Standard and Logic/Firmware PVT report is a pass";
            desc = "Standard and Logic/Firmware PVT report is a pass";
            CreateNPISubIssue(projectname, pjkey, parentkey, firstengineer, sum, desc, 9);

            sum = "Cpk from NPI Build Data shows Cpk > 1.0";
            desc = "Cpk from NPI Build Data shows Cpk > 1.0";
            CreateNPISubIssue(projectname, pjkey, parentkey, firstengineer, sum, desc, 9);

            sum = "Yield Requirements";
            desc = "Low Complexity : 1st Pass 70%, Cumulative without Rework 90%.Medium Complexity : 1st Pass 55% Cumulative w/o Rework 85%.High Complexity : 1st Pass 40%, Cumulative without Rework 80%";
            CreateNPISubIssue(projectname, pjkey, parentkey, firstengineer, sum, desc, 9);

            sum = "IC - Pre-Production released";
            desc = "IC - Pre-Production released";
            CreateNPISubIssue(projectname, pjkey, parentkey, firstengineer, sum, desc, 9);

            sum = "OSAs: internal passed DVT, external Pre-Production released";
            desc = "OSAs: internal passed DVT, external Pre-Production released";
            CreateNPISubIssue(projectname, pjkey, parentkey, firstengineer, sum, desc, 9);

            sum = "Rel - 1,000 Hours Complete";
            desc = "Rel - 1,000 Hours Complete";
            CreateNPISubIssue(projectname, pjkey, parentkey, firstengineer, sum, desc, 9);
        }


        public static void CreateNPIProcTasks(string projectname,string pjkey,string firstengineer)
        {
            CreateEVTIssues(projectname, pjkey, firstengineer);
            CreateDVTIssues(projectname, pjkey, firstengineer);
            CreateMVTIssues(projectname, pjkey, firstengineer);
        }

    }
}