using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
    public class DashboardType
    {
        public static string Task = "Task";
        public static string Bug = "Bug";
        public static string CrititalFailureTask = "CriticalFailure";
        public static string DebugTree = "DebugTree";
    }
    public class IDashboardVM
    {
        public IDashboardVM()
        {
            UserWeek = null;
            TaskList = null;
            TaskClosedList = null;
            BugList = null;
            BugClosedList = null;
            CriticalTaskList = null;
            CriticalTaskClosedList = null;
            DebugTreeList = null;
        }
        public UserWeekVM UserWeek { set; get; }
        public IssueDashboardVM TaskList { set; get; }
        public IssueDashboardVM TaskClosedList { set; get; }
        public IssueDashboardVM BugList { set; get; }
        public IssueDashboardVM BugClosedList { set; get; }
        public IssueDashboardVM CriticalTaskList { set; get; }
        public IssueDashboardVM CriticalTaskClosedList { set; get; }
        public DebugTree4DashboardWithWeek DebugTreeList { set; get; }

        public static Dictionary<string, IDashboardVM> RetrieveDashboardData(List<string> uList, string iType = "", string sDate = "", string eDate = "")
        {
            var n_datas = GetIssuesByConditions(uList, iType, sDate, eDate, 1);
            var c_datas = GetIssuesByConditions(uList, iType, sDate, eDate, 2);
            var d_datas = GetDebugTreeByConditions(uList, sDate, eDate);
            var nw_data = CombineData(n_datas);
            var cw_data = CombineData(c_datas);
            var user_week = CalculateUserWeeks(uList, sDate, eDate);
            var res = new Dictionary<string, IDashboardVM>();
            foreach (var uw in user_week)
            {
                if (!res.ContainsKey(uw.Key))
                {
                    var tmp = new IDashboardVM();
                    tmp.UserWeek = uw.Value;
                    if (nw_data.ContainsKey(uw.Key))
                    {
                        foreach (var item in nw_data[uw.Key])
                        {
                            //Task
                            if (string.Compare(item.Key, ISSUESUBTYPE.Task.ToString(), true) == 0
                                || string.Compare(item.Key, ISSUESUBTYPE.Other.ToString(), true) == 0
                                || string.Compare(item.Key, ISSUESUBTYPE.NonCrititalFailureTask.ToString(), true) == 0
                                || string.Compare(item.Key, ISSUESUBTYPE.OBA.ToString(), true) == 0
                                || string.Compare(item.Key, ISSUESUBTYPE.RMA.ToString(), true) == 0
                                || string.Compare(item.Key, ISSUESUBTYPE.REL.ToString(), true) == 0
                                || string.Compare(item.Key, ISSUESUBTYPE.NPIProcess.ToString(), true) == 0)
                            {
                                if (tmp.TaskList == null)
                                {
                                    tmp.TaskList = item.Value;
                                }
                                else
                                {
                                    tmp.TaskList.Count += item.Value.Count;
                                    tmp.TaskList.IssueList.AddRange(item.Value.IssueList);
                                }
                            }
                            //CriticalTask
                            if (string.Compare(item.Key, ISSUESUBTYPE.CrititalFailureTask.ToString(), true) == 0)
                            {
                                if (tmp.CriticalTaskList == null)
                                {
                                    tmp.CriticalTaskList = item.Value;
                                }
                                else
                                {
                                    tmp.CriticalTaskList.Count += item.Value.Count;
                                    tmp.CriticalTaskList.IssueList.AddRange(item.Value.IssueList);
                                }
                            }
                            //Bug
                            if (string.Compare(item.Key, ISSUESUBTYPE.Bug.ToString(), true) == 0)
                            {
                                if (tmp.BugList == null)
                                {
                                    tmp.BugList = item.Value;
                                }
                                else
                                {
                                    tmp.BugList.Count += item.Value.Count;
                                    tmp.BugList.IssueList.AddRange(item.Value.IssueList);
                                }
                            }
                        }
                    }
                    if (cw_data.ContainsKey(uw.Key))
                    {
                        foreach (var item in cw_data[uw.Key])
                        {
                            //Task
                            if (string.Compare(item.Key, ISSUESUBTYPE.Task.ToString(), true) == 0
                                || string.Compare(item.Key, ISSUESUBTYPE.Other.ToString(), true) == 0
                                || string.Compare(item.Key, ISSUESUBTYPE.NonCrititalFailureTask.ToString(), true) == 0
                                || string.Compare(item.Key, ISSUESUBTYPE.OBA.ToString(), true) == 0
                                || string.Compare(item.Key, ISSUESUBTYPE.RMA.ToString(), true) == 0
                                || string.Compare(item.Key, ISSUESUBTYPE.REL.ToString(), true) == 0
                                || string.Compare(item.Key, ISSUESUBTYPE.NPIProcess.ToString(), true) == 0)
                            {
                                if (tmp.TaskClosedList == null)
                                {
                                    tmp.TaskClosedList = item.Value;
                                }
                                else
                                {
                                    tmp.TaskClosedList.Count += item.Value.Count;
                                    tmp.TaskClosedList.IssueList.AddRange(item.Value.IssueList);
                                }
                            }
                            //CriticalTask
                            if (string.Compare(item.Key, ISSUESUBTYPE.CrititalFailureTask.ToString(), true) == 0)
                            {
                                if (tmp.CriticalTaskClosedList == null)
                                {
                                    tmp.CriticalTaskClosedList = item.Value;
                                }
                                else
                                {
                                    tmp.CriticalTaskClosedList.Count += item.Value.Count;
                                    tmp.CriticalTaskClosedList.IssueList.AddRange(item.Value.IssueList);
                                }
                            }
                            //Bug
                            if (string.Compare(item.Key, ISSUESUBTYPE.Bug.ToString(), true) == 0)
                            {
                                if (tmp.BugClosedList == null)
                                {
                                    tmp.BugClosedList = item.Value;
                                }
                                else
                                {
                                    tmp.BugClosedList.Count += item.Value.Count;
                                    tmp.BugClosedList.IssueList.AddRange(item.Value.IssueList);
                                }
                            }
                        }
                    }
                    if (d_datas.ContainsKey(uw.Key))
                    {
                        tmp.DebugTreeList = d_datas[uw.Key];
                    }
                    res.Add(uw.Key, tmp);
                }
            }

            return res;
        }

        public static Dictionary<string, UserWeekVM> CalculateUserWeeks(List<string> uList, string sDate, string eDate)
        {
            var weeks = CalculateWeek(sDate, eDate);
            var user_week = new Dictionary<string, UserWeekVM>();
            foreach (var user in uList)
            {
                foreach (var week in weeks)
                {
                    if (!user_week.ContainsKey(user + "_" + week.Year + "_" + week.Week))
                    {
                        var tmp = new UserWeekVM();
                        tmp.UserName = user;
                        tmp.Week = week;
                        user_week.Add(user + "_" + week.Year + "_" + week.Week, tmp);
                    }
                }
            }
            return user_week;
        }

        public static List<WeekVM> CalculateWeek(string sDate, string eDate)
        {
            var s_year = Convert.ToDateTime(sDate).ToString("yyyy");
            var e_year = Convert.ToDateTime(eDate).ToString("yyyy");
            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            var s_week = dfi.Calendar.GetWeekOfYear(Convert.ToDateTime(sDate), dfi.CalendarWeekRule, DayOfWeek.Friday);
            var e_week = dfi.Calendar.GetWeekOfYear(Convert.ToDateTime(eDate), dfi.CalendarWeekRule, DayOfWeek.Friday);
            var idx = 0;
            var res = new List<WeekVM>();
            if (string.Compare(s_year, e_year) == 0)
            {
                for (var i = s_week; i <= e_week; i++)
                {
                    var tmp = new WeekVM();
                    tmp.Year = s_year;
                    tmp.Week = i.ToString();
                    tmp.Date = Convert.ToDateTime(sDate).AddDays(+(idx * 7)).ToString("yyyy-MM-dd");
                    res.Add(tmp);
                    idx++;
                }
            }
            else
            {
                for (var m = Convert.ToInt32(s_year); m < Convert.ToInt32(e_year); m++)
                {
                    var s_max_week = dfi.Calendar.GetWeekOfYear(Convert.ToDateTime(m + "-12-31"), dfi.CalendarWeekRule, DayOfWeek.Friday);
                    for (var i = s_week; i < s_max_week; i++)
                    {
                        var tmp = new WeekVM();
                        tmp.Year = m.ToString();
                        tmp.Week = i.ToString();
                        tmp.Date = Convert.ToDateTime(sDate).AddDays(+(idx * 7)).ToString("yyyy-MM-dd");
                        res.Add(tmp);
                        idx++;
                    }
                }
                for (var n = 1; n <= e_week; n++)
                {
                    var tmp = new WeekVM();
                    tmp.Year = e_year;
                    tmp.Week = n.ToString();
                    tmp.Date = Convert.ToDateTime(sDate).AddDays(+(idx * 7)).ToString("yyyy-MM-dd");
                    res.Add(tmp);
                    idx++;
                }
            }
            return res;
        }

        public static Dictionary<string, Dictionary<string, IssueDashboardVM>> CombineData(Dictionary<string, IssueList4DashboardWithWeek> data)
        {
            var nw_data = new Dictionary<string, Dictionary<string, IssueDashboardVM>>();
            foreach (var item in data)
            {
                var key = item.Value.Assignee + "_" + item.Value.Year + "_" + item.Value.Week;
                if (!nw_data.ContainsKey(key))
                {
                    var tmp = new Dictionary<string, IssueDashboardVM>();
                    var i_tmp = new IssueDashboardVM();
                    i_tmp.Count = 1;
                    i_tmp.IssueList = new List<IssueList4DashboardWithWeek> { item.Value };
                    tmp.Add(item.Value.IssueSubType, i_tmp);
                    nw_data.Add(key, tmp);
                }
                else
                {
                    var s_key = item.Value.IssueSubType;
                    if (!nw_data[key].ContainsKey(s_key))
                    {
                        var i_tmp = new IssueDashboardVM();
                        i_tmp.Count = 1;
                        i_tmp.IssueList = new List<IssueList4DashboardWithWeek> { item.Value };
                        nw_data[key].Add(s_key, i_tmp);
                    }
                    else
                    {
                        nw_data[key][s_key].Count += 1;
                        nw_data[key][s_key].IssueList.Add(item.Value);
                    }
                }
            }
            return nw_data;
        }

        public static Dictionary<string, IssueList4DashboardWithWeek> GetIssuesByConditions(List<string> uList, string iType = "", string sDate = "", string eDate = "", int stype = 1)
        {
            var cons = "";
            var sql = @"SET DATEFIRST 5; select i.IssueKey, i.Assignee, 
                i.Resolution, it.IssueSubType, it.IssueSsubType, ";
            if (stype == 1)
            {
                //new issues in this week
                sql += " DATEPART(wk, i.ReportDate) as Wk, DATEPART(yyyy, i.ReportDate) as Year ";
            }
            else
            {
                //solved issues in this week
                sql += " DATEPART(wk, i.ResolvedDate) as Wk, DATEPART(yyyy, i.ResolvedDate) as Year ";
                cons = " and i.Resolution not in ('" + Resolute.Pending+ "', '" + Resolute.Working + "','" + Resolute.Reopen + "')";
            }
            sql += @", i.ProjectKey, i.Summary, i.ReportDate, i.ResolvedDate from issue as i left join IssueType as it on i.IssueKey = it.IssueKey
                    where i.APVal1 <> 'delete' and i.ParentIssueKey = '' and i.Resolution <> 'AutoClose' 
                    and i.Assignee in <#uList> <#cons>";
            sql = sql.Replace("<#uList>", "('" + string.Join("','", uList) + "')")
                    .Replace("<#cons>", cons);
            var param = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(iType))
            {
                sql += " and it.IssueSubType in <#iType> ";
                sql = sql.Replace("<#iType>", iType);
            }
            if (!string.IsNullOrEmpty(sDate))
            {
                if (stype == 1)
                {
                    sql += " and i.ReportDate >= @sDate ";
                }
                else
                {
                    sql += " and i.ResolvedDate >= @sDate ";
                }
                param.Add("@sDate", sDate);
            }
            if (!string.IsNullOrEmpty(eDate))
            {
                if (stype == 1)
                {
                    sql += " and i.ReportDate <= @eDate ";
                }
                else
                {
                    sql += " and i.ResolvedDate <= @eDate ";
                }
                param.Add("@eDate", eDate);
            }
            sql += " order by Assignee, Wk asc; SET DATEFIRST 7;";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, param);
            DBUtility.ExeLocalSqlNoRes(sql);
            var res = new Dictionary<string, IssueList4DashboardWithWeek>();
            if (dbret.Count > 0)
            {
                foreach (var item in dbret)
                {
                    if (!res.ContainsKey(Convert.ToString(item[0])))
                    {
                        var tmp = new IssueList4DashboardWithWeek();
                        tmp.IssueKey = Convert.ToString(item[0]);
                        tmp.Assignee = Convert.ToString(item[1]).ToUpper();
                        tmp.Resolution = Convert.ToString(item[2]);
                        tmp.IssueSubType = Convert.ToString(item[3]);
                        tmp.IssueSsubType = Convert.ToString(item[4]);
                        tmp.Week = Convert.ToString(item[5]);
                        tmp.Year = Convert.ToString(item[6]);
                        tmp.ProjectKey = Convert.ToString(item[7]);
                        tmp.Summary = Convert.ToString(item[8]);
                        tmp.ReportDate = Convert.ToString(item[9]);
                        tmp.ResolvedDate = Convert.ToString(item[10]);
                        res.Add(Convert.ToString(item[0]), tmp);
                    }
                }
            }
            return res;
        }

        public static Dictionary<string, DebugTree4DashboardWithWeek> GetDebugTreeByConditions(List<string> uList, string sDate = "", string eDate = "")
        {
            var sql = @"SET DATEFIRST 5;
                    select i.Reporter, DATEPART(yyyy, i.CommentDate) as y, DATEPART(wk, i.CommentDate) as wk, 
                    count(distinct i.AnalyzeID) as cnt
                    from ErrorComments as i
                    where i.APVal1 <> 'delete' and i.AnalyzeID <> ''
                    and i.Reporter in <#uList> ";
            sql = sql.Replace("<#uList>", "('" + string.Join("','", uList) + "')");
            var param = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(sDate))
            {
                sql += " and i.CommentDate >= @sDate ";
                param.Add("@sDate", sDate);
            }
            if (!string.IsNullOrEmpty(eDate))
            {
                sql += " and i.CommentDate <= @eDate ";
                param.Add("@eDate", eDate);
            }
            sql += @" group by i.Reporter, DATEPART(yyyy, i.CommentDate), DATEPART(wk, i.CommentDate)
                    order by Reporter, y, wk;
                    Set Datefirst 7;";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, param);
            var res = new Dictionary<string, DebugTree4DashboardWithWeek>();
            if (dbret.Count > 0)
            {
                foreach (var item in dbret)
                {
                    var tmp = new DebugTree4DashboardWithWeek();
                    tmp.Reporter = Convert.ToString(item[0]);
                    tmp.Year = Convert.ToString(item[1]);
                    tmp.Week = Convert.ToString(item[2]);
                    tmp.Count = Convert.ToInt32(item[3]);
                    var u_key = tmp.Reporter + "_" + tmp.Year + "_" + tmp.Week;
                    if (!res.ContainsKey(u_key))
                    {
                        res.Add(u_key, tmp);
                    }
                }
            }

            return res;
        }

        public static Dictionary<string, DebugTreeVM> GetDebugTreeDetail(List<string> uList, string sDate = "", string eDate = "")
        {
            var sql = @"select ec.AnalyzeID, ec.Comment, ec.CommentDate, ec.CommentType, ec.Reporter, 
                ec.ErrorKey, pe.OrignalCode, pe.ShortDesc, pe.ProjectKey, pe.ErrorCount, pe.SolvedCount
                from ErrorComments as ec left join ProjectError as pe on ec.ErrorKey = pe.ErrorKey 
                where ec.APVal1 <> 'delete' 
                and ec.Reporter in <#uList> ";
            sql = sql.Replace("<#uList>", "('" + string.Join("','", uList) + "')");

            var param = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(sDate))
            {
                sql += " and ec.CommentDate >= @sDate ";
                param.Add("@sDate", sDate);
            }
            if (!string.IsNullOrEmpty(eDate))
            {
                sql += " and ec.CommentDate <= @eDate ";
                param.Add("@eDate", eDate);
            }
            sql += " order by pe.OrignalCode, ec.CommentDate desc";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, param);
            var res = new Dictionary<string, DebugTreeVM>();
            if(dbret.Count > 0)
            {
                foreach(var item in dbret)
                {
                    var com_tmp = new CommentVM();
                    com_tmp.Comment = (!string.IsNullOrEmpty(Convert.ToString(item[1]))) ? ConvertComment(Convert.ToString(item[1])) : "";
                    com_tmp.CommentDate = Convert.ToString(item[2]);
                    com_tmp.CommentType = Convert.ToString(item[3]);
                    if (!res.ContainsKey(Convert.ToString(item[0])))
                    {
                        var tmp = new DebugTreeVM();
                        tmp.AnalyzeID = Convert.ToString(item[0]);
                        tmp.Reporter = Convert.ToString(item[4]);
                        tmp.ErrorKey = Convert.ToString(item[5]);
                        tmp.OrignalCode = Convert.ToString(item[6]);
                        tmp.ShortDesc = Convert.ToString(item[7]);
                        tmp.ProjectKey = Convert.ToString(item[8]);
                        tmp.ErrorCount = Convert.ToString(item[9]);
                        tmp.SolvedCount = Convert.ToString(item[10]);
                        RetrieveComment(Convert.ToString(item[3]), tmp, com_tmp);
                        res.Add(tmp.AnalyzeID, tmp);
                    }
                    else
                    {
                        RetrieveComment(Convert.ToString(item[3]), res[Convert.ToString(item[0])], com_tmp);
                    }
                }
            }
            return res;
        }

        public static string ConvertComment(string comment)
        {
            var com = new ErrorComments();
            com.dbComment = comment;
            return com.Comment;
        }

        public static void RetrieveComment(string ctype, DebugTreeVM dtree, CommentVM comment)
        {
            if (string.Compare(Convert.ToString(ctype), PJERRORCOMMENTTYPE.AnalyzeTitle, true) == 0)
            {
                dtree.AnalyzeTitle = comment;
            }
            else if (string.Compare(Convert.ToString(ctype), PJERRORCOMMENTTYPE.FailureDetail, true) == 0)
            {
                dtree.FailureDetail = comment;
            }
            else if (string.Compare(Convert.ToString(ctype), PJERRORCOMMENTTYPE.RootCause, true) == 0)
            {
                dtree.RootCause = comment;
            }
            else if (string.Compare(Convert.ToString(ctype), PJERRORCOMMENTTYPE.Result, true) == 0)
            {
                dtree.Result = comment;
            }
        }
    }
    public class IssueDashboardVM
    {
        public int Count { set; get; }
        public List<IssueList4DashboardWithWeek> IssueList { set; get; }
    }

    public class WeekVM
    {
        public string Year { set; get; }
        public string Week { set; get; }
        public string Date { set; get; }
    }

    public class UserWeekVM
    {
        public string UserName { set; get; }
        public WeekVM Week { set; get; }
    }

    public class IssueList4DashboardWithWeek
    {
        public string IssueKey { set; get; }
        public string Assignee { set; get; }
        public string Resolution { set; get; }
        public string IssueSubType { set; get; }
        public string IssueSsubType { set; get; }
        public string Week { set; get; }
        public string Year { set; get; }
        public string ProjectKey { set; get; }
        public string Summary { set; get; }
        public string ReportDate { set; get; }
        public string ResolvedDate { set; get; }

    }

    public class DebugTree4DashboardWithWeek
    {
        public string Reporter { set; get; }
        public string Year { set; get; }
        public string Week { set; get; }
        public int Count { set; get; }
    }

    public class DebugTreeVM
    {
        public string AnalyzeID { set; get; }
        public CommentVM AnalyzeTitle { set; get; }
        public CommentVM RootCause { set; get; }
        public CommentVM FailureDetail { set; get; }
        public CommentVM Result { set; get; }
        public CommentVM Description { set; get; }
        public string Reporter { set; get; }
        public string ErrorKey { set; get; }
        public string OrignalCode { set; get; }
        public string ShortDesc { set; get; }
        public string ProjectKey { set; get; }
        public string ErrorCount { set; get; }
        public string SolvedCount { set; get; }

    }

    public class CommentVM
    {
        public string Comment { set; get; }
        public string CommentDate { set; get; }
        public string CommentType { set; get; }
    }
}