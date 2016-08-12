using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
    public class ISSUETP
    {
        public static string Bug = "Bug";
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

        public IssueViewModels(string pk,string ik,string it,string sum,string pri,string ddate,string rsdate,string rtdate,string assign,string rt,string resolute,string pik)
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


        private List<IssueComments> cemlist = new List<IssueComments>();
        public List<IssueComments> CommentList
        {
            set
            {
                cemlist.Clear();
                cemlist.AddRange(value);
            }
            get
            {
                return cemlist;
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
            }
            get
            {
                return sissue;
            }
        }

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
            var sql = "insert into Issue(ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,Creator) values('<ProjectKey>','<IssueKey>','<IssueType>','<Summary>','<Priority>','<DueDate>','<ResolvedDate>','<ReportDate>','<Assignee>','<Reporter>','<Resolution>','<Creator>')";
            sql = sql.Replace("<ProjectKey>", ProjectKey).Replace("<IssueKey>", IssueKey).Replace("<IssueType>", IssueType)
                .Replace("<Summary>", Summary).Replace("<Priority>", Priority).Replace("<DueDate>", DueDate.ToString())
                .Replace("<ResolvedDate>", ResolvedDate.ToString()).Replace("<ReportDate>", ReportDate.ToString()).Replace("<Assignee>", Assignee)
                .Replace("<Reporter>", Reporter).Replace("<Resolution>", Resolution).Replace("<Creator>", Reporter);
            DBUtility.ExeLocalSqlNoRes(sql);

            StoreIssueComment(DateTime.Now.ToString());
        }

        public void StoreSubIssue()
        {
            var sql = "insert into Issue(ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,ParentIssueKey,Creator) values('<ProjectKey>','<IssueKey>','<IssueType>','<Summary>','<Priority>','<DueDate>','<ResolvedDate>','<ReportDate>','<Assignee>','<Reporter>','<Resolution>','<ParentIssueKey>','<Creator>')";
            sql = sql.Replace("<ProjectKey>", ProjectKey).Replace("<IssueKey>", IssueKey).Replace("<IssueType>", IssueType)
                .Replace("<Summary>", Summary).Replace("<Priority>", Priority).Replace("<DueDate>", DueDate.ToString())
                .Replace("<ResolvedDate>", ResolvedDate.ToString()).Replace("<ReportDate>", ReportDate.ToString()).Replace("<Assignee>", Assignee)
                .Replace("<Reporter>", Reporter).Replace("<Resolution>", Resolution).Replace("<ParentIssueKey>", ParentIssueKey).Replace("<Creator>", Reporter);
            DBUtility.ExeLocalSqlNoRes(sql);

            StoreIssueComment(DateTime.Now.ToString());
        }

        private void StoreIssueComment(string CommentDate)
        {
            if (!string.IsNullOrEmpty(Description))
            {
                var sql = "insert into IssueComments(IssueKey,Comment,Reporter,CommentDate) values('<IssueKey>','<Comment>','<Reporter>','<CommentDate>')";
                sql = sql.Replace("<IssueKey>", IssueKey).Replace("<Comment>", dbDescription).Replace("<Reporter>", Reporter).Replace("<CommentDate>", CommentDate);
                DBUtility.ExeLocalSqlNoRes(sql);
            }
        }

        public void UpdateIssue()
        {
            var sql = "update Issue set IssueType = '<IssueType>',Priority = '<Priority>',DueDate = '<DueDate>', Assignee = '<Assignee>',Resolution = '<Resolution>' where IssueKey = '<IssueKey>'";
            sql = sql.Replace("<IssueKey>", IssueKey).Replace("<IssueType>", IssueType)
                .Replace("<Priority>", Priority).Replace("<DueDate>", DueDate.ToString())
                .Replace("<Assignee>", Assignee).Replace("<Resolution>", Resolution);
            DBUtility.ExeLocalSqlNoRes(sql);

            StoreIssueComment(DateTime.Now.ToString());
        }

        public void UpdateIAssign()
        {
            var sql = "update Issue set Reporter = '<Reporter>' where IssueKey = '<IssueKey>'";
            sql = sql.Replace("<IssueKey>", IssueKey).Replace("<Reporter>", Reporter);
            DBUtility.ExeLocalSqlNoRes(sql);
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
            var sql = "select ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution from Issue where ParentIssueKey = '<ParentIssueKey>'";
            sql = sql.Replace("<ParentIssueKey>", parentkey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var tempvm = new IssueViewModels(Convert.ToString(line[0])
                    , Convert.ToString(line[1]), Convert.ToString(line[2])
                    , Convert.ToString(line[3]), Convert.ToString(line[4])
                    , Convert.ToString(line[5]), Convert.ToString(line[6])
                    , Convert.ToString(line[7]), Convert.ToString(line[8])
                    , Convert.ToString(line[9]), Convert.ToString(line[10]), parentkey);


                var tempclist = new List<IssueComments>();
                var csql = "select IssueKey,Comment,Reporter,CommentDate from IssueComments where IssueKey = '<IssueKey>' order by CommentDate ASC";
                csql = csql.Replace("<IssueKey>", tempvm.IssueKey);

                var cdbret = DBUtility.ExeLocalSqlWithRes(csql);
                foreach (var r in cdbret)
                {
                    var tempcomment = new IssueComments();
                    tempcomment.IssueKey = Convert.ToString(r[0]);
                    tempcomment.dbComment = Convert.ToString(r[1]);
                    tempcomment.Reporter = Convert.ToString(r[2]);
                    tempcomment.CommentDate = DateTime.Parse(Convert.ToString(r[3]));
                    tempclist.Add(tempcomment);
                }
                tempvm.CommentList = tempclist;

                ret.Add(tempvm);
            }
            return ret;
        }

        public static IssueViewModels RetrieveIssueByIssueKey(string issuekey)
        {
            var sql = "select ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution,ParentIssueKey from Issue where IssueKey = '<IssueKey>'";
            sql = sql.Replace("<IssueKey>", issuekey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            if (dbret.Count > 0)
            {
                var ret = new IssueViewModels(Convert.ToString(dbret[0][0])
                    , Convert.ToString(dbret[0][1]), Convert.ToString(dbret[0][2])
                    , Convert.ToString(dbret[0][3]), Convert.ToString(dbret[0][4])
                    , Convert.ToString(dbret[0][5]), Convert.ToString(dbret[0][6])
                    , Convert.ToString(dbret[0][7]), Convert.ToString(dbret[0][8])
                    , Convert.ToString(dbret[0][9]), Convert.ToString(dbret[0][10]), Convert.ToString(dbret[0][11]));


                var tempclist = new List<IssueComments>();
                sql = "select IssueKey,Comment,Reporter,CommentDate from IssueComments where IssueKey = '<IssueKey>' order by CommentDate ASC";
                sql = sql.Replace("<IssueKey>", issuekey);
                dbret = DBUtility.ExeLocalSqlWithRes(sql);
                foreach (var r in dbret)
                {
                    var tempcomment = new IssueComments();
                    tempcomment.IssueKey = Convert.ToString(r[0]);
                    tempcomment.dbComment = Convert.ToString(r[1]);
                    tempcomment.Reporter = Convert.ToString(r[2]);
                    tempcomment.CommentDate = DateTime.Parse(Convert.ToString(r[3]));
                    tempclist.Add(tempcomment);
                }
                ret.CommentList = tempclist;

                ret.SubIssues = RetrieveSubIssue(ret.IssueKey);

                return ret;
            }
            else
                return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectkey"></param>
        /// <param name="issuestatus"> comes from the resolution has pending,working,done three status</param>
        /// <returns></returns>
        /// 
        public static List<IssueViewModels> RetrieveIssueByProjectKey(string projectkey,string issuestatus,int topnum)
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

            var sql = "select top <topnum> ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution from Issue where ProjectKey = '<ProjectKey>' and Resolution in <cond> and  ParentIssueKey = '' order by ReportDate DESC";
            sql = sql.Replace("<ProjectKey>", projectkey).Replace("<topnum>", Convert.ToString(topnum)).Replace("<cond>", cond);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            var ret = new List<IssueViewModels>();
            foreach (var line in dbret)
            {
                ret.Add(new IssueViewModels(Convert.ToString(line[0])
                    , Convert.ToString(line[1]), Convert.ToString(line[2])
                    , Convert.ToString(line[3]), Convert.ToString(line[4])
                    , Convert.ToString(line[5]), Convert.ToString(line[6])
                    , Convert.ToString(line[7]), Convert.ToString(line[8])
                    , Convert.ToString(line[9]), fixresolve, ""));
            }

            return ret;
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

            var sql = "select top <topnum> ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution from Issue where Assignee = '<Assignee>' and Resolution in <cond> and ParentIssueKey = '' order by ReportDate DESC";
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
                    , Convert.ToString(line[9]), fixresolve,""));
            }

            return ret;
        }


        public static List<IssueViewModels> RetrieveIssueByCreator(string creator, int topnum)
        {
            var sql = "select top <topnum> ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution from Issue where (Creator = '<Creator>' or Reporter = '<Creator>') and ParentIssueKey = '' order by ReportDate DESC";
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
                    , Convert.ToString(line[9]), Convert.ToString(line[10]), "");
                
                tempvm.SubIssues = RetrieveSubIssue(tempvm.IssueKey);
                ret.Add(tempvm);
            }

            return ret;
        }

        public static List<IssueViewModels> SearchIssue(string pjkey,string type,string resolution,string asignee,string startdate,string enddate,string summary, int topnum)
        {
            var sql = "select top <topnum> ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution from Issue where <cond> order by ReportDate DESC";

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
                    , Convert.ToString(line[9]), Convert.ToString(line[10]), "");
                ret.Add(tempvm);
            }

            return ret;
        }

    }
}