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
        public static int DebugTree = 5;
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
        public WeeklyReportVM()
        {
            ID = "";
            ProjectKey = "";
            UserName = "";
            IssueKey = "";
            Summary = "";
            Type = "";
            Year = "";
            Week = "";
            Mark = "";
            Status = "";
            CreateTime = "";
            UpdateTime = "";
        }
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
        public TaskData()
        {
            IssueKey = "";
            Description = "";
            IssueType = "";
            IssueSubType = "";
            Status = "";
            StartDate = DateTime.Now;
            DueDate = DateTime.Now;
            UpdateTime = DateTime.Now;
            Attachment = new List<string>();
        }
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

    public class WeeklyYieldData
    {
        public WeeklyYieldData()
        {
            Data = new List<string>();
            Pareto = "";
            Trend = "";
        }

        public WeeklyYieldData(List<string> data, string pareto, string trend)
        {
            Data = data;
            Pareto = pareto;
            Trend = trend;
        }
        public List<string> Data { set; get; }

        public string Pareto { set; get; }

        public string Trend { set; get; }

    }

    public class WeeklyReportSetting
    {
        public WeeklyReportSetting()
        {
            ID = "";
            UserName = "";
            Yield = 1;
            Task = 1;
            CriticalFailure = 1;
            RMA = 1;
            DebugTree = 1;
            Others = 1;
            CreateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public WeeklyReportSetting(string id, string username, int yield, int task, int critask, int rma, int dtree, int others, string ctime, string utime)
        {
            ID = id;
            UserName = username;
            Yield = yield;
            Task = task;
            CriticalFailure = critask;
            RMA = rma;
            DebugTree = dtree;
            Others = others;
            CreateTime = ctime;
            UpdateTime = utime;
        }

        public string ID { set; get; }
        public string UserName { set; get; }
        public int Yield { set; get; }
        public int Task { set; get; }
        public int CriticalFailure { set; get; }
        public int RMA { set; get; }
        public int DebugTree { set; get; }
        public int Others { set; get; }
        public string CreateTime { set; get; }
        public string UpdateTime { set; get; }

        public static WeeklyReportSetting GetWeeklyReportSetting(string username)
        {
            var sql = "select ID, UserName, Yield, Task, CriticalFailure, RMA, "
                    + "DebugTree, Others, CreateTime, UpdateTime "
                    + "from WeeklyReportSetting "
                    + "where UserName = '<UserName>' ";
            sql = sql.Replace("<UserName>", username);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            var ret = new WeeklyReportSetting();
            foreach (var line in dbret)
            {
                ret.ID = Convert.ToString(line[0]);
                ret.UserName = Convert.ToString(line[1]);
                ret.Yield = Convert.ToInt32(line[2]);
                ret.Task = Convert.ToInt32(line[3]);
                ret.CriticalFailure = Convert.ToInt32(line[4]);
                ret.RMA = Convert.ToInt32(line[5]);
                ret.DebugTree = Convert.ToInt32(line[6]);
                ret.Others = Convert.ToInt32(line[7]);
                ret.CreateTime = Convert.ToString(line[8]);
                ret.UpdateTime = Convert.ToString(line[9]);
            }

            return ret;
        }

        public static void SaveWeeklyReportSetting(WeeklyReportSetting setting)
        {
            var sql = "select ID from WeeklyReportSetting where UserName = '<UserName>'";
            sql = sql.Replace("<UserName>", setting.UserName);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            if(dbret.Count > 0)
            {
                var updatesql = "update WeeklyReportSetting set Yield = '<Yield>', "
                            + "Task = '<Task>', CriticalFailure = '<CriticalFailure>', "
                            + "RMA = '<RMA>', DebugTree = '<DebugTree>', "
                            + "Others = '<Others>', UpdateTime = '<UpdateTime>' "
                            + "where UserName = '<UserName>'; ";
                updatesql = updatesql.Replace("<UserName>", setting.UserName)
                        .Replace("<Yield>", setting.Yield.ToString())
                        .Replace("<Task>", setting.Task.ToString())
                        .Replace("<CriticalFailure>", setting.CriticalFailure.ToString())
                        .Replace("<RMA>", setting.RMA.ToString())
                        .Replace("<DebugTree>", setting.DebugTree.ToString())
                        .Replace("<Others>", setting.Others.ToString())
                        .Replace("<UpdateTime>", setting.UpdateTime);
                DBUtility.ExeLocalSqlNoRes(updatesql);
            }
            else
            {
                var insertsql = "insert into WeeklyReportSetting "
                            + "(UserName, Yield, Task, CriticalFailure, RMA, "
                            + "DebugTree, Others, CreateTime, UpdateTime) values "
                            + "('<UserName>', '<Yield>', '<Task>', '<CriticalFailure>', "
                            + "'<RMA>', '<DebugTree>', '<Others>', '<CreateTime>', '<UpdateTime>'); ";
                insertsql = insertsql.Replace("<UserName>", setting.UserName)
                        .Replace("<Yield>", setting.Yield.ToString())
                        .Replace("<Task>", setting.Task.ToString())
                        .Replace("<CriticalFailure>", setting.CriticalFailure.ToString())
                        .Replace("<RMA>", setting.RMA.ToString())
                        .Replace("<DebugTree>", setting.DebugTree.ToString())
                        .Replace("<Others>", setting.Others.ToString())
                        .Replace("<CreateTime>", setting.CreateTime)
                        .Replace("<UpdateTime>", setting.UpdateTime);

                DBUtility.ExeLocalSqlNoRes(insertsql);
            }
        }
    }
}