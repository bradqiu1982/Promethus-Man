using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
            //var sql = "SELECT MAX(Log.Date) as UpdateTime, ut.UserName " +
            //        "FROM UserTable as ut " +
            //        "Left Join Log ON (ut.UserName = Log.UserName and Log.LogType = '<LogType>') " +
            //        "WHERE ut.UserName in (<UserName>) " +
            //        "Group by ut.UserName " +
            //        "ORDER BY UpdateTime DESC";
            var sql = @"SELECT MAX(UpdateTime) as UpdateTime, UserName FROM [NPITrace].[dbo].[WeeklyReport] 
                            where UserName in  (<UserName>)  group by UserName";
            //sql = sql.Replace("<UserName>", username.ToUpper())
            //        .Replace("<LogType>", LogType.Task.ToString());
            sql = sql.Replace("<UserName>", username.ToUpper());
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
            var sql = "select ID, UserName, ProjectKey, IssueKey, Summary, Type, Year, Week, " +
                "Mark, Status, CreateTime, UpdateTime " +
                "from WeeklyReport " +
                "where IssueKey = N'<IssueKey>' " +
                "and Status = '<Status>' " +
                "and (Summary <> '' or Summary <> null)" +
                "order by UpdateTime Desc; ";
            sql = sql.Replace("<IssueKey>", iKey).Replace("<Status>", SummaryStatus.Valid.ToString());
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            var ret = new List<WeeklyReportVM>();
            foreach (var line in dbret)
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
            foreach (var rep in report)
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
                    if (exist_data.Count > 0)
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
            if (dbret.Count > 0)
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

    public class KPIVM
    {
        public KPIVM()
        {
            UserName = "";
            Task = 0;
            CriticalTask = 0;
            Bug = 0;
            RMA = 0;
            DebugTree = 0;
            TaskAll = 0;
            CriticalTaskAll = 0;
            BugAll = 0;
            RMAAll = 0;
        }
        public string UserName { set; get; }
        public double Task { set; get; }
        public double TaskAll { set; get; }
        public double CriticalTask { set; get; }
        public double CriticalTaskAll { set; get; }
        public double Bug { set; get; }
        public double BugAll { set; get; }
        public double RMA { set; get; }
        public double RMAAll { set; get; }
        public double DebugTree { set; get; }
        public double ShareDocument { set; get; }


        public static Dictionary<string, KPIVM> GetAllKPI(Controller ctrl, List<string> uName = null, string sDate = "", string eDate = "", int period = 1)
        {
            var res_task = GetKPI(uName, sDate, eDate, period);
            var avg = GetGroupAvg(ctrl, sDate, eDate);

            res_task.Add("h_avg", avg["h_avg"]);
            res_task.Add("y_avg", avg["y_avg"]);

            return res_task;
        }

        public static Dictionary<string, double> GetShareDoc(List<string> uName = null, string sDate = "", string eDate = "")
        {
            var param = new Dictionary<string, string>();
            var sql = @"select i.DOCCreator, count(*) as cnt 
                        from ShareDoc as i 
                        where DOCTag <> 'RMA;' and DOCTag not like '%WEEKLYREPORT;%' and DOCTag <> 'REL;'  and DOCTag <>  'Process;'";
            if (uName != null)
            {
                var arr_name = new List<string>();
                var idx = 0;
                foreach (var item in uName)
                {
                    arr_name.Add("@uName" + idx);
                    param.Add("@uName" + idx, item);
                    idx++;
                }
                sql += " and i.DOCCreator in (" + string.Join(",", arr_name.ToArray()) + ")";
            }
            if (!string.IsNullOrEmpty(sDate))
            {
                sql += " and i.DOCDate >= @sDate ";
                param.Add("@sDate", sDate);
            }

            if (!string.IsNullOrEmpty(eDate))
            {
                sql += " and i.DOCDate <= @eDate ";
                param.Add("@eDate", eDate);
            }
            sql += " group by i.DOCCreator order by i.DOCCreator; ";

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, param);
            var ret = new Dictionary<string, double>();
            if (dbret.Count > 0)
            {
                foreach (var item in dbret)
                {
                    ret.Add(Convert.ToString(item[0]).ToUpper(), Convert.ToDouble(item[1]));
                }
            }
            return ret;

        }

        public static Dictionary<string, KPIVM> GetKPI(List<string> uName = null, string sDate = "", string eDate = "", int period = 1)
        {
            if (string.IsNullOrEmpty(sDate))
            {
                sDate = (period == 1) ? DateTime.Now.AddYears(-1).ToString("yyyy-MM-dd 00:00:00") : DateTime.Now.AddMonths(-6).ToString("yyyy-MM-dd 00:00:00");
            }
            if (string.IsNullOrEmpty(eDate))
            {
                eDate = DateTime.Now.ToString("yyyy-MM-dd 23:59:59");
            }
            var res_task = GetTasks(uName, sDate, eDate);
            var res_debug = GetDebugTree(uName, sDate, eDate);
            var allTasks = GetAllTasks(uName, sDate, eDate);
            var sharedoc = GetShareDoc(uName, sDate, eDate);

            foreach (var item in uName)
            {
                if (res_task.ContainsKey(item))
                {
                    if (res_debug.ContainsKey(item))
                    {
                        res_task[item].DebugTree = res_debug[item].DebugTree;
                    }
                    if (allTasks.ContainsKey(item))
                    {
                        res_task[item].TaskAll = allTasks[item].TaskAll;
                        res_task[item].CriticalTaskAll = allTasks[item].CriticalTaskAll;
                        res_task[item].BugAll = allTasks[item].BugAll;
                        res_task[item].RMAAll = allTasks[item].RMAAll;
                    }
                    if (sharedoc.ContainsKey(item))
                    {
                        res_task[item].ShareDocument = sharedoc[item];
                    }
                }
                else
                {
                    var tmp = new KPIVM();
                    if (res_debug.ContainsKey(item))
                    {
                        tmp.DebugTree = res_debug[item].DebugTree;
                    }
                    if (allTasks.ContainsKey(item))
                    {
                        tmp.TaskAll = allTasks[item].TaskAll;
                        tmp.CriticalTaskAll = allTasks[item].CriticalTaskAll;
                        tmp.BugAll = allTasks[item].BugAll;
                        tmp.RMAAll = allTasks[item].RMAAll;
                    }
                    if (sharedoc.ContainsKey(item))
                    {
                        tmp.ShareDocument = sharedoc[item];
                    }
                    res_task.Add(item, tmp);
                }
            }

            return res_task;
        }

        public static Dictionary<string, KPIVM> GetTasks(List<string> uName = null, string sDate = "", string eDate = "")
        {
            var param = new Dictionary<string, string>();
            var sql = @"select i.Assignee, it.IssueSubType, count(*) as cnt 
                        from issue as i 
                        left join IssueType as it on i.IssueKey = it.IssueKey 
                        where i.APVal1 <> 'delete'
                        and i.Resolution in (@Fixed, @NotFix, @Done)";
            param.Add("@Fixed", Resolute.Fixed);
            param.Add("@NotFix", Resolute.NotFix);
            param.Add("@Done", Resolute.Done);
            if (uName != null)
            {
                var arr_name = new List<string>();
                var idx = 0;
                foreach (var item in uName)
                {
                    arr_name.Add("@uName" + idx);
                    param.Add("@uName" + idx, item);
                    idx++;
                }
                sql += " and i.Assignee in (" + string.Join(",", arr_name.ToArray()) + ")";
            }
            if (!string.IsNullOrEmpty(sDate))
            {
                sql += " and i.ResolvedDate >= @sDate ";
                param.Add("@sDate", sDate);
            }

            if (!string.IsNullOrEmpty(eDate))
            {
                sql += " and i.ResolvedDate <= @eDate ";
                param.Add("@eDate", eDate);
            }
            sql += " group by i.Assignee, it.IssueSubType order by Assignee; ";

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, param);
            var res = new Dictionary<string, KPIVM>();
            if (dbret.Count > 0)
            {
                foreach (var item in dbret)
                {
                    if (res.ContainsKey(Convert.ToString(item[0]).ToUpper()))
                    {
                        if (Convert.ToInt32(item[1]) == ISSUESUBTYPE.Task)
                        {
                            res[Convert.ToString(item[0]).ToUpper()].Task = Convert.ToDouble(item[2]);
                        }
                        else if (Convert.ToInt32(item[1]) == ISSUESUBTYPE.CrititalFailureTask)
                        {
                            res[Convert.ToString(item[0]).ToUpper()].CriticalTask = Convert.ToDouble(item[2]);
                        }
                        else if (Convert.ToInt32(item[1]) == ISSUESUBTYPE.Bug || Convert.ToInt32(item[1]) == ISSUESUBTYPE.NonCrititalFailureTask)
                        {
                            res[Convert.ToString(item[0]).ToUpper()].Bug += Convert.ToDouble(item[2]);
                        }
                        else if (Convert.ToInt32(item[1]) == ISSUESUBTYPE.RMA)
                        {
                            res[Convert.ToString(item[0]).ToUpper()].RMA = Convert.ToDouble(item[2]);
                        }
                    }
                    else
                    {
                        var kpi_tmp = new KPIVM();
                        kpi_tmp.UserName = Convert.ToString(item[0]).ToUpper();
                        if (Convert.ToInt32(item[1]) == ISSUESUBTYPE.Task)
                        {
                            kpi_tmp.Task = Convert.ToDouble(item[2]);
                        }
                        else if (Convert.ToInt32(item[1]) == ISSUESUBTYPE.CrititalFailureTask)
                        {
                            kpi_tmp.CriticalTask = Convert.ToDouble(item[2]);
                        }
                        else if (Convert.ToInt32(item[1]) == ISSUESUBTYPE.Bug || Convert.ToInt32(item[1]) == ISSUESUBTYPE.NonCrititalFailureTask)
                        {
                            kpi_tmp.Bug += Convert.ToDouble(item[2]);
                        }
                        else if (Convert.ToInt32(item[1]) == ISSUESUBTYPE.RMA)
                        {
                            kpi_tmp.RMA = Convert.ToDouble(item[2]);
                        }
                        res.Add(Convert.ToString(item[0]).ToUpper(), kpi_tmp);
                    }
                }
            }
            return res;
        }

        public static Dictionary<string, KPIVM> GetAllTasks(List<string> uName = null, string sDate = "", string eDate = "")
        {
            var param = new Dictionary<string, string>();
            var sql = @"select i.Assignee, it.IssueSubType, count(*) as cnt 
                        from issue as i
                        left join IssueType as it on i.IssueKey = it.IssueKey
                        where i.APVal1 <> 'delete' ";
            if (uName != null)
            {
                var arr_name = new List<string>();
                var idx = 0;
                foreach (var item in uName)
                {
                    arr_name.Add("@uName" + idx);
                    param.Add("@uName" + idx, item);
                    idx++;
                }
                sql += " and i.Assignee in (" + string.Join(",", arr_name.ToArray()) + ")";
            }
            if (!string.IsNullOrEmpty(sDate))
            {
                sql += " and i.ReportDate >= @sDate ";
                param.Add("@sDate", sDate);
            }

            if (!string.IsNullOrEmpty(eDate))
            {
                sql += " and i.ReportDate <= @eDate ";
                param.Add("@eDate", eDate);
            }
            sql += " group by i.Assignee, it.IssueSubType order by Assignee; ";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, param);
            var res = new Dictionary<string, KPIVM>();
            if (dbret.Count > 0)
            {
                foreach (var item in dbret)
                {
                    if (res.ContainsKey(Convert.ToString(item[0]).ToUpper()))
                    {
                        if (Convert.ToInt32(item[1]) == ISSUESUBTYPE.Task)
                        {
                            res[Convert.ToString(item[0]).ToUpper()].TaskAll = Convert.ToDouble(item[2]);
                        }
                        else if (Convert.ToInt32(item[1]) == ISSUESUBTYPE.CrititalFailureTask)
                        {
                            res[Convert.ToString(item[0]).ToUpper()].CriticalTaskAll = Convert.ToDouble(item[2]);
                        }
                        else if (Convert.ToInt32(item[1]) == ISSUESUBTYPE.Bug || Convert.ToInt32(item[1]) == ISSUESUBTYPE.NonCrititalFailureTask)
                        {
                            res[Convert.ToString(item[0]).ToUpper()].BugAll += Convert.ToDouble(item[2]);
                        }
                        else if (Convert.ToInt32(item[1]) == ISSUESUBTYPE.RMA)
                        {
                            res[Convert.ToString(item[0]).ToUpper()].RMAAll = Convert.ToDouble(item[2]);
                        }
                    }
                    else
                    {
                        var kpi_tmp = new KPIVM();
                        kpi_tmp.UserName = Convert.ToString(item[0]).ToUpper();
                        if (Convert.ToInt32(item[1]) == ISSUESUBTYPE.Task)
                        {
                            kpi_tmp.TaskAll = Convert.ToDouble(item[2]);
                        }
                        else if (Convert.ToInt32(item[1]) == ISSUESUBTYPE.CrititalFailureTask)
                        {
                            kpi_tmp.CriticalTaskAll = Convert.ToDouble(item[2]);
                        }
                        else if (Convert.ToInt32(item[1]) == ISSUESUBTYPE.Bug || Convert.ToInt32(item[1]) == ISSUESUBTYPE.NonCrititalFailureTask)
                        {
                            kpi_tmp.BugAll += Convert.ToDouble(item[2]);
                        }
                        else if (Convert.ToInt32(item[1]) == ISSUESUBTYPE.RMA)
                        {
                            kpi_tmp.RMAAll = Convert.ToDouble(item[2]);
                        }
                        res.Add(Convert.ToString(item[0]).ToUpper(), kpi_tmp);
                    }
                }
            }
            return res;
        }

        public static Dictionary<string, double> GetRMAWorking(List<string> uName = null, string sDate = "", string eDate = "")
        {
            var param = new Dictionary<string, string>();
            var sql = @"select i.Assignee, count(*) as cnt 
                        from issue as i
                        left join IssueType as it on i.IssueKey = it.IssueKey
                        where i.APVal1 <> 'delete'
                        and i.Resolution = @Working and it.IssueSubType = @RMAType ";
            param.Add("@Working", Resolute.Working);
            param.Add("@RMAType", ISSUESUBTYPE.RMA.ToString());
            if (uName != null)
            {
                var arr_name = new List<string>();
                var idx = 0;
                foreach (var item in uName)
                {
                    arr_name.Add("@uName" + idx);
                    param.Add("@uName" + idx, item);
                    idx++;
                }
                sql += " and i.Assignee in (" + string.Join(",", arr_name.ToArray()) + ")";
            }
            if (!string.IsNullOrEmpty(sDate))
            {
                sql += " and i.ReportDate >= @sDate ";
                param.Add("@sDate", sDate);
            }

            if (!string.IsNullOrEmpty(eDate))
            {
                sql += " and i.ReportDate <= @eDate ";
                param.Add("@eDate", eDate);
            }
            sql += " group by i.Assignee order by Assignee; ";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, param);
            var rma_working = new Dictionary<string, double>();
            if(dbret.Count > 0)
            {
                foreach(var item in dbret)
                {
                    rma_working.Add(Convert.ToString(item[0]), Convert.ToDouble(item[1]));
                }
            }
            return rma_working;
        }

        public static Dictionary<string, KPIVM> GetDebugTree(List<string> uName = null, string sDate = "", string eDate = "")
        {
            var param = new Dictionary<string, string>();
            var sql = @"select Reporter, count(*) as cnt 
                from (
                    select distinct(AnalyzeID), Reporter, CommentDate 
                    from ErrorComments 
                    where APVal1 <> 'delete' and AnalyzeID <> '' and Reporter <> '' ";
            if (uName != null)
            {
                var arr_name = new List<string>();
                var idx = 0;
                foreach (var item in uName)
                {
                    arr_name.Add("@Reporter" + idx);
                    param.Add("@Reporter" + idx, item);
                    idx++;
                }
                sql += " and Reporter in (" + string.Join(",", arr_name.ToArray()) + ")";
            }
            if (!string.IsNullOrEmpty(sDate))
            {
                sql += " and CommentDate >= @sDate ";
                param.Add("@sDate", sDate);
            }

            if (!string.IsNullOrEmpty(eDate))
            {
                sql += " and CommentDate <= @eDate ";
                param.Add("@eDate", eDate);
            }
            sql += " ) tmp_a group by Reporter order by Reporter; ";

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, param);
            var res = new Dictionary<string, KPIVM>();
            if(dbret.Count > 0)
            {
                foreach (var item in dbret)
                {
                    var kpi_tmp = new KPIVM();
                    kpi_tmp.UserName = Convert.ToString(item[0]).ToUpper();
                    kpi_tmp.DebugTree = Convert.ToDouble(item[1]);
                    res.Add(Convert.ToString(item[0]).ToUpper(), kpi_tmp);
                }
            }
            return res;
        }

        public static Dictionary<string, KPIVM> GetGroupAvg(Controller ctrl, string sDate, string eDate)
        {
            var syscfg = CfgUtility.GetSysConfig(ctrl);
            var ulist = syscfg["KPIGROUP1"].ToUpper().Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();

            var half_data = GetKPI(ulist, sDate, eDate, 0);
            var year_data = GetKPI(ulist, sDate, eDate, 1);

            var half_avg = GetAvg(half_data);
            var year_avg = GetAvg(year_data);

            var res = new Dictionary<string, KPIVM>();
            res.Add("h_avg", half_avg);
            res.Add("y_avg", year_avg);
            return res;
        }

        public static KPIVM GetAvg(Dictionary<string, KPIVM> data)
        {
            var u_cnt = data.Count();
            var total = new KPIVM();
            foreach(var user in data)
            {
                total.Task += user.Value.Task;
                total.CriticalTask += user.Value.CriticalTask;
                total.Bug += user.Value.Bug;
                total.RMA += user.Value.RMA; 
                total.DebugTree += user.Value.DebugTree;
                total.ShareDocument += user.Value.ShareDocument;
            }
            var res = new KPIVM();
            res.Task = Math.Round(total.Task / u_cnt, 4);
            res.CriticalTask = Math.Round(total.CriticalTask / u_cnt, 4);
            res.Bug = Math.Round(total.Bug / u_cnt, 4);
            res.RMA = Math.Round(total.RMA / u_cnt, 4);
            res.DebugTree = Math.Round(total.DebugTree / u_cnt, 4);
            res.ShareDocument = Math.Round(total.ShareDocument / u_cnt, 4);
            return res;
        }
    }
}