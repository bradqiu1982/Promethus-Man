using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
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

        public IssueViewModels(string pk,string ik,string it,string sum,string pri,string ddate,string rsdate,string rtdate,string assign,string rt,string resolute)
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

        public static string GetUniqKey()
        {
            return Guid.NewGuid().ToString("N");
        }

        public void StoreIssue()
        {
            var sql = "insert into Issue(ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution) values('<ProjectKey>','<IssueKey>','<IssueType>','<Summary>','<Priority>','<DueDate>','<ResolvedDate>','<ReportDate>','<Assignee>','<Reporter>','<Resolution>')";
            sql = sql.Replace("<ProjectKey>", ProjectKey).Replace("<IssueKey>", IssueKey).Replace("<IssueType>", IssueType)
                .Replace("<Summary>", Summary).Replace("<Priority>", Priority).Replace("<DueDate>", DueDate.ToString())
                .Replace("<ResolvedDate>", ResolvedDate.ToString()).Replace("<ReportDate>", ReportDate.ToString()).Replace("<Assignee>", Assignee)
                .Replace("<Reporter>", Reporter).Replace("<Resolution>", Resolution);
            DBUtility.ExeLocalSqlNoRes(sql);

            StoreIssueComment(DateTime.Now.ToString());
        }

        public void StoreIssueComment(string CommentDate)
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

        //public static List<string> RetrieveIssueKeys(string projectkey, int topnum)
        //{
        //    var ret = new List<string>();

        //    var sql = "select top <num> IssueKey from Issue where ProjectKey = '<ProjectKey>'  order by ReportDate DESC";
        //    sql = sql.Replace("<num>", Convert.ToString(topnum)).Replace("<ProjectKey>", projectkey);
        //    var dbret = DBUtility.ExeLocalSqlWithRes(sql);
        //    foreach (var item in dbret)
        //    {
        //        ret.Add(Convert.ToString(item[0]));
        //    }

        //    return ret;
        //}

        public static IssueViewModels RetrieveIssueByIssueKey(string issuekey)
        {
            var sql = "select ProjectKey,IssueKey,IssueType,Summary,Priority,DueDate,ResolvedDate,ReportDate,Assignee,Reporter,Resolution from Issue where IssueKey = '<IssueKey>'";
            sql = sql.Replace("<IssueKey>", issuekey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            if (dbret.Count > 0)
            {
                var ret = new IssueViewModels(Convert.ToString(dbret[0][0])
                    , Convert.ToString(dbret[0][1]), Convert.ToString(dbret[0][2])
                    , Convert.ToString(dbret[0][3]), Convert.ToString(dbret[0][4])
                    , Convert.ToString(dbret[0][5]), Convert.ToString(dbret[0][6])
                    , Convert.ToString(dbret[0][7]), Convert.ToString(dbret[0][8])
                    , Convert.ToString(dbret[0][8]), Convert.ToString(dbret[0][10]));


                var tempclist = new List<IssueComments>();
                sql = "select IssueKey,Comment,Reporter,CommentDate from IssueComments where IssueKey = '<IssueKey>' order by CommentDate DESC";
                sql = sql.Replace("<IssueKey>", issuekey);
                dbret = DBUtility.ExeLocalSqlWithRes(sql);
                foreach (var r in dbret)
                {
                    var tempcomment = new IssueComments();
                    tempcomment.dbComment = Convert.ToString(dbret[0][1]);
                    tempcomment.Reporter = Convert.ToString(dbret[0][2]);
                    tempcomment.CommentDate = DateTime.Parse(Convert.ToString(dbret[0][3]));
                    tempclist.Add(tempcomment);
                }
                ret.CommentList = tempclist;

                return ret;
            }
            else
                return null;
        }

    }
}