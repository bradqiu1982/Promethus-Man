﻿using System;
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
        public static int ICare = 6;
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
            var sql = "SELECT MAX(Log.Date) as UpdateTime, ut.UserName " +
                    "FROM UserTable as ut " +
                    "Left Join Log ON ut.UserName = Log.UserName " +
                    "WHERE ut.UserName in (<UserName>) " +
                    "and Log.LogType = '<LogType>' " +
                    "Group by ut.UserName " +
                    "ORDER BY UpdateTime DESC";
            sql = sql.Replace("<UserName>", username.ToUpper())
                    .Replace("<LogType>", LogType.Task.ToString());
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

        public static List<WeeklyReportVM> GetSummary(string iKey)
        {
            var sql = "select ID, UserName, ProjectKey, IssueKey, Summary, Type, Year, Week, "+
                "Mark, Status, CreateTime, UpdateTime "+
                "from WeeklyReport "+
                "where IssueKey = N'<IssueKey>' "+ 
                "and Status = '<Status>' " +
                "and (Summary <> '' or Summary <> null)" +
                "order by UpdateTime Desc; ";
            sql = sql.Replace("<IssueKey>", iKey).Replace("<Status>", SummaryStatus.Valid.ToString());
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            var ret = new List<WeeklyReportVM>();
            foreach(var line in dbret)
            {
                ret.Add(new WeeklyReportVM(
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
                ));
            }
            return ret;
        }

        public static void SaveWeeklyReport(List<WeeklyReportVM> report)
        {
            foreach(var rep in report)
            {
                var s_sql = "select Summary, Mark from WeeklyReport " +
                        "where ProjectKey = @ProjectKey " +
                        "and IssueKey = @IssueKey " +
                        "and Year = @Year and Week = @Week " +
                        "and Type = @Type and UserName = @UserName " +
                        "and Status = @Status;";
                var param = new Dictionary<string, string>();
                    param.Add("@ProjectKey", rep.ProjectKey);
                    param.Add("@IssueKey", rep.IssueKey);
                    param.Add("@Year", rep.Year);
                    param.Add("@Week", rep.Week);
                    param.Add("@Type", rep.Type);
                    param.Add("@UserName", rep.UserName);
                    param.Add("@Status", SummaryStatus.Valid.ToString());
                var exist_data = DBUtility.ExeLocalSqlWithRes(s_sql, null, param);
                if (exist_data.Count == 1 && string.Compare(exist_data[0][0].ToString(), rep.Summary) == 0
                    && string.Compare(exist_data[0][1].ToString(), rep.Mark) == 0)
                {

                }
                else
                {
                    if(exist_data.Count > 0)
                    {
                        var sqltmp = "update WeeklyReport set Status = @DelStatus " +
                            "where ProjectKey = @ProjectKey " +
                            "and IssueKey = @IssueKey " +
                            "and Year = @Year and Week = @Week " +
                            "and Type = @Type and UserName = @UserName " +
                            "and Status = @Status;";
                        var param1 = new Dictionary<string, string>();
                            param1.Add("@DelStatus", SummaryStatus.Delete.ToString());
                            param1.Add("@ProjectKey", rep.ProjectKey);
                            param1.Add("@IssueKey", rep.IssueKey);
                            param1.Add("@Year", rep.Year);
                            param1.Add("@Week", rep.Week);
                            param1.Add("@Type", rep.Type);
                            param1.Add("@UserName", rep.UserName);
                            param1.Add("@Status", SummaryStatus.Valid.ToString());
                        DBUtility.ExeLocalSqlNoRes(sqltmp, param1);
                    }
                    var sql = @"insert into WeeklyReport (UserName, ProjectKey, IssueKey, 
                                Summary, [Type], [Year], [Week], [Mark], [Status], 
                                [CreateTime], [UpdateTime]) values (@UserName, @ProjectKey,
                                @IssueKey, @Summary, @Type, @Year, @Week, @Mark,
                                @Status, @CreateTime, @UpdateTime); ";
                    var param2 = new Dictionary<string, string>();
                        param2.Add("@UserName", rep.UserName);
                        param2.Add("@ProjectKey", rep.ProjectKey);
                        param2.Add("@IssueKey", rep.IssueKey);
                        param2.Add("@Summary", rep.Summary);
                        param2.Add("@Type", rep.Type);
                        param2.Add("@Year", rep.Year);
                        param2.Add("@Week", rep.Week);
                        param2.Add("@Mark", rep.Mark);
                        param2.Add("@Status", rep.Status);
                        param2.Add("@CreateTime", rep.CreateTime);
                        param2.Add("@UpdateTime", rep.UpdateTime);

                    DBUtility.ExeLocalSqlNoRes(sql, param2);
                }
            }
        }

        public static Dictionary<string, List<WeeklyReportVM>> GetIssueSummary(string pKey, string sDate, string eDate)
        {
            var sql = "select ID, UserName, ProjectKey, IssueKey, Summary, Type, Year, Week, " +
                "Mark, Status, CreateTime, UpdateTime " +
                "from WeeklyReport " +
                "where ProjectKey = N'<ProjectKey>' " +
                "and UpdateTime between '<sDate>' and '<eDate>' and Status = '<Status>' " +
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
                var key_tmp = Convert.ToString(line[3]);
                if (Convert.ToInt32(line[5]) == SummaryType.Others
                    || Convert.ToInt32(line[5]) == SummaryType.Yield
                        || Convert.ToInt32(line[5]) == SummaryType.DebugTree)
                {
                    key_tmp += "@@" + Convert.ToString(line[5]);
                }
                if (ret.ContainsKey(key_tmp))
                {
                    ret[key_tmp].Add(tmp);
                }
                else
                {
                    var tmplist = new List<WeeklyReportVM>();
                    tmplist.Add(tmp);
                    ret.Add(key_tmp, tmplist);
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
            ParentIssueKey = string.Empty;
            Assignee = string.Empty;
            Rule = string.Empty;
            Attachment = new List<string>();
        }
        public TaskData(string iKey, string des, string type, string subtype, string status, DateTime sDate, DateTime dDate, DateTime uUpdate, string pIKey, string assignee, string rule, List<string> attach)
        {
            IssueKey = iKey;
            Description = des;
            IssueType = type;
            IssueSubType = subtype;
            Status = status;
            StartDate = sDate;
            DueDate = dDate;
            UpdateTime = uUpdate;
            ParentIssueKey = pIKey;
            Assignee = assignee;
            Rule = rule;
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
        public string ParentIssueKey { set; get; }
        public string Assignee { set; get; }
        public string Rule { set; get; }
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
            ICare = 1;
            Task = 1;
            CriticalFailure = 1;
            RMA = 1;
            DebugTree = 1;
            Others = 1;
            CreateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public WeeklyReportSetting(string id, string username, int yield, int icare, int task, int critask, int rma, int dtree, int others, string ctime, string utime)
        {
            ID = id;
            UserName = username;
            Yield = yield;
            ICare = icare;
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
        public int ICare { set; get; }
        public int Task { set; get; }
        public int CriticalFailure { set; get; }
        public int RMA { set; get; }
        public int DebugTree { set; get; }
        public int Others { set; get; }
        public string CreateTime { set; get; }
        public string UpdateTime { set; get; }

        public static WeeklyReportSetting GetWeeklyReportSetting(string username)
        {
            var sql = "select ID, UserName, Yield, ICare, Task, CriticalFailure, RMA, "
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
                ret.ICare = Convert.ToInt32(line[3]);
                ret.Task = Convert.ToInt32(line[4]);
                ret.CriticalFailure = Convert.ToInt32(line[5]);
                ret.RMA = Convert.ToInt32(line[6]);
                ret.DebugTree = Convert.ToInt32(line[7]);
                ret.Others = Convert.ToInt32(line[8]);
                ret.CreateTime = Convert.ToString(line[9]);
                ret.UpdateTime = Convert.ToString(line[10]);
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
                            + "ICare = '<ICare>', Task = '<Task>', CriticalFailure = '<CriticalFailure>', "
                            + "RMA = '<RMA>', DebugTree = '<DebugTree>', "
                            + "Others = '<Others>', UpdateTime = '<UpdateTime>' "
                            + "where UserName = N'<UserName>'; ";
                updatesql = updatesql.Replace("<UserName>", setting.UserName)
                        .Replace("<Yield>", setting.Yield.ToString())
                        .Replace("<ICare>", setting.ICare.ToString())
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
                            + "(UserName, Yield, ICare, Task, CriticalFailure, RMA, "
                            + "DebugTree, Others, CreateTime, UpdateTime) values "
                            + "(N'<UserName>', '<Yield>', '<ICare>', '<Task>', '<CriticalFailure>', "
                            + "'<RMA>', '<DebugTree>', '<Others>', '<CreateTime>', '<UpdateTime>'); ";
                insertsql = insertsql.Replace("<UserName>", setting.UserName)
                        .Replace("<Yield>", setting.Yield.ToString())
                        .Replace("<ICare>", setting.ICare.ToString())
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

    public class TaskDataWithUpdateFlg
    {
        public TaskDataWithUpdateFlg()
        {
            IsUpdate = false;
            TaskList = new Dictionary<string, TaskData>();
        }

        public TaskDataWithUpdateFlg(bool isupdate, Dictionary<string, TaskData> tasklist)
        {
            IsUpdate = isupdate;
            TaskList = tasklist;
        }
        
        public bool IsUpdate { set; get; }

        public Dictionary<string, TaskData> TaskList { set; get; }
    }
}