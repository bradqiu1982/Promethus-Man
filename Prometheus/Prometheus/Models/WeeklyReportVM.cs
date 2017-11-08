using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
    public class SummaryType
    {
        //0: Others Summary 1: Yield Summary 2: Task 3: Critical Failure 4: RMA
        public static int Others = 0;
        public static int Yield = 1;
        public static int Task = 2;
        public static int CriticalFailure = 3;
        public static int RMA = 4;
    }

    public class MarkType
    {
        //0: Normal 1: High 2: Low
        public static int Normal = 0;
        public static int High = 1;
        public static int Low = 2;
    }

    public class SummaryStatus
    {
        public static int Delete = 0;
        public static int Valid = 1;
    }

    public class WeeklyReportVM
    {
        public WeeklyReportVM(string id, string pkey, string username, string ikey, string summary, string type,
            string year, string week, string mark, string status, string createtime, string updatetime)
        {
            ID = id;
            ProjectKey = pkey;
            UserName = username;
            IssueKey = ikey;
            Summary = summary;
            Type = type;
            Year = year;
            Week = week;
            Mark = mark;
            Status = status;
            CreateTime = createtime;
            UpdateTime = updatetime;
        }
        public string ID { set; get; }
        public string ProjectKey { set; get; }
        public string UserName { set; get; }
        public string IssueKey { set; get; }
        public string Summary { set; get; }
        public string Type { set; get; }
        public string Year { set; get; }
        public string Week { set; get; }
        public string Mark { set; get; }
        public string Status { set; get; }
        public string CreateTime { set; get; }
        public string UpdateTime { set; get; }

        public static Dictionary<string, string> GetUserLatestTime(String username)
        {
            var ret = new Dictionary<string, string>();
            var sql = "SELECT MAX(wr.UpdateTime) as UpdateTime, ut.UserName " +
                    "FROM UserTable as ut " +
                    "Left Join WeeklyReport as wr ON ut.UserName = wr.UserName " +
                    "WHERE ut.UserName in (<username>) " +
                    "Group by ut.UserName " +
                    "ORDER BY UpdateTime DESC";
            sql = sql.Replace("<username>", username.ToUpper());
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            var UserName = "";
            var UpdateTime = "";
            foreach (var line in dbret)
            {
                UserName = Convert.ToString(line[1]);
                try
                {
                    UpdateTime = Convert.ToDateTime(line[0]).ToString("yyyy-MM-dd HH:mm");
                }
                catch
                {
                    UpdateTime = "";
                }
                ret.Add(UserName, UpdateTime);
            }
            return ret;
        }

        public static List<WeeklyReportVM> GetSummary(string sType, string iKey)
        {
            var sql = "select ID, UserName, ProjectKey, IssueKey, Summary, Type, Year, Week, "+
                "Mark, Status, CreateTime, UpdateTime "+
                "from WeeklyReport "+
                "where IssueKey = '<IssueKey>' and Type = '<SummaryType>' and Status = '<Status>'"+
                "order by UpdateTime Desc; ";
            sql = sql.Replace("<IssueKey>", iKey).Replace("<SummaryType>", sType).Replace("<Status>", SummaryStatus.Valid.ToString());
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            var ret = new List<WeeklyReportVM>();
            foreach(var line in dbret)
            {
                ret.Add(new WeeklyReportVM(
                    Convert.ToString(line[0]),
                    Convert.ToString(line[1]),
                    Convert.ToString(line[2]),
                    Convert.ToString(line[3]),
                    Convert.ToString(line[4]),
                    Convert.ToString(line[5]),
                    Convert.ToString(line[6]),
                    Convert.ToString(line[7]),
                    Convert.ToString(line[8]),
                    Convert.ToString(line[9]),
                    Convert.ToDateTime(Convert.ToString(line[10])).ToString("yyyy/MM/dd HH:mm"),
                    Convert.ToDateTime(Convert.ToString(line[11])).ToString("yyyy/MM/dd HH:mm")
                ));
            }
            return ret;
        }

        public static void SaveWeeklyReport(List<WeeklyReportVM> report)
        {
            var items = "";
            foreach(var rep in report)
            {
                //var sqltmp = "update WeeklyReport set Status = '<DelStatus>'" +
                var sqltmp = "delete from WeeklyReport " +
                        "where ProjectKey = '<ProjectKey>' " +
                        "and IssueKey = '<IssueKey>' " +
                        "and Year = '<Year>' and Week = '<Week>' "+
                        "and Type = '<Type>' and UserName = '<UserName>' "+
                        "and Status = '<Status>';";
                sqltmp = sqltmp.Replace("<DelStatus>", SummaryStatus.Delete.ToString())
                        .Replace("<ProjectKey>", rep.ProjectKey)
                        .Replace("<IssueKey>", rep.IssueKey)
                        .Replace("<Year>", rep.Year).Replace("<Week>", rep.Week)
                        .Replace("<Type>", rep.Type)
                        .Replace("<UserName>", rep.UserName)
                        .Replace("<Status>", SummaryStatus.Valid.ToString());
                DBUtility.ExeLocalSqlNoRes(sqltmp);
                items += "('" + rep.UserName + "'," 
                        + "'" + rep.ProjectKey + "'," 
                        + "'" + rep.IssueKey + "',"
                        + "'" + rep.Summary + "',"
                        + "'" + rep.Type + "',"
                        + "'" + rep.Year + "',"
                        + "'" + rep.Week + "',"
                        + "'" + rep.Mark + "',"
                        + "'" + rep.Status + "',"
                        + "'" + rep.CreateTime + "',"
                        + "'" + rep.UpdateTime + "'),";
            }
            items = items.Substring(0, items.Length - 1);
            var sql = "insert into WeeklyReport (UserName, ProjectKey, IssueKey, Summary, [Type], [Year], [Week], [Mark], [Status], [CreateTime], [UpdateTime]) values <Items>; ";
            sql = sql.Replace("<Items>", items);

            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static Dictionary<string, List<WeeklyReportVM>> GetIssueSummary(string pKey, string sDate, string eDate)
        {
            var sql = "select ID, UserName, ProjectKey, IssueKey, Summary, Type, Year, Week, " +
                "Mark, Status, CreateTime, UpdateTime " +
                "from WeeklyReport " +
                "where ProjectKey = '<ProjectKey>' and UpdateTime "+
                "between '<sDate>' and '<eDate>' and Status = '<Status>' " +
                "order by UpdateTime Desc; ";
            sql = sql.Replace("<ProjectKey>", pKey)
                    .Replace("<sDate>", sDate)
                    .Replace("<eDate>", eDate)
                    .Replace("<Status>", SummaryStatus.Valid.ToString());
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            var ret = new Dictionary<string, List<WeeklyReportVM>>();
            foreach (var line in dbret)
            {
                var tmp = new WeeklyReportVM(
                    Convert.ToString(line[0]),
                    Convert.ToString(line[2]),
                    Convert.ToString(line[1]),
                    Convert.ToString(line[3]),
                    Convert.ToString(line[4]),
                    Convert.ToString(line[5]),
                    Convert.ToString(line[6]),
                    Convert.ToString(line[7]),
                    Convert.ToString(line[8]),
                    Convert.ToString(line[9]),
                    Convert.ToDateTime(Convert.ToString(line[10])).ToString("yyyy/MM/dd HH:mm"),
                    Convert.ToDateTime(Convert.ToString(line[11])).ToString("yyyy/MM/dd HH:mm")
                );
                if (ret.ContainsKey(Convert.ToString(line[3]) + "@@" + Convert.ToString(line[5])))
                {
                    ret[Convert.ToString(line[3]) + "@@" + Convert.ToString(line[5])].Add(tmp);
                }
                else
                {
                    var tmplist = new List<WeeklyReportVM>();
                    tmplist.Add(tmp);
                    ret.Add(Convert.ToString(line[3]) + "@@" + Convert.ToString(line[5]), tmplist);

                }
            }
            return ret;

        }
    }

    public class TaskData
    {
        public TaskData(string iKey, string des, string type, string subtype, string status, DateTime sDate, DateTime dDate, DateTime uUpdate, List<string> attach)
        {
            IssueKey = iKey;
            Description = des;
            IssueType = type;
            IssueSubType = subtype;
            Status = status;
            StartDate = sDate;
            DueDate = dDate;
            UpdateTime = uUpdate;
            Attachment = attach;
        }
        public string IssueKey { set; get; }
        public string Description { set; get; }
        public string IssueType { set; get; }
        public string IssueSubType { set; get; }
        public string Status { set; get; }
        public DateTime StartDate { set; get; }
        public DateTime DueDate { set; get; }
        public DateTime UpdateTime { set; get; }
        public List<string> Attachment { set; get; }
    }
}