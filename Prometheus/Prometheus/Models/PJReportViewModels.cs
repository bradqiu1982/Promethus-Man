using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
    public class ReportSelectTime
    {
        public static string MON = "MONDAY";
        public static string TUE = "TUESDAY";
        public static string WEN = "WENDSDAY";
        public static string THU = "THURSDAY";
        public static string FRI = "FRIDAY";
        public static string SAT = "SATURDAY";
        public static string SUN = "SUNDAY";
        public static string EVE = "EVERY DAY";

        public static int GetWeekDay(string day)
        {
            if (string.Compare(day, ReportSelectTime.MON) == 0)
            {
                return (int)DayOfWeek.Monday;
            }
            if (string.Compare(day, ReportSelectTime.TUE) == 0)
            {
                return (int)DayOfWeek.Tuesday;
            }
            if (string.Compare(day, ReportSelectTime.WEN) == 0)
            {
                return (int)DayOfWeek.Wednesday;
            }
            if (string.Compare(day, ReportSelectTime.THU) == 0)
            {
                return (int)DayOfWeek.Thursday;
            }
            if (string.Compare(day, ReportSelectTime.FRI) == 0)
            {
                return (int)DayOfWeek.Friday;
            }
            if (string.Compare(day, ReportSelectTime.SAT) == 0)
            {
                return (int)DayOfWeek.Saturday;
            }
            if (string.Compare(day, ReportSelectTime.SUN) == 0)
            {
                return (int)DayOfWeek.Sunday;
            }
            return 8;
        }
    }

    public class PJReportType
    {
        public static string YieldTrend = "Yield Trend";
        public static string IssueTrend = "Issue Count Trend";
        public static string FailureTrend = "Failure Trend";
        public static string MonthlyPareto = "Monthly Pareto";
    }

    public class PJReportItem
    {
        public string YieldTrend { set; get; }
        public string MonthlyPareto { set; get; }
        public string IssueTrend { set; get; }

        private List<string> failurelist = new List<string>();
        public List<string> FailureTrends
        {
            get { return failurelist; }
            set {
                failurelist.Clear();
                failurelist.AddRange(value);
                    }
        }
    }

    public class PJReportViewModels
    {
        public string username { set; get; }

        private List<string> projectlist = new List<string>();

        public List<string> ProjectList
        {
            get
            { return projectlist; }
            set
            {
                projectlist.Clear();
                projectlist.AddRange(value);
            }
        }

        public string projects
        {
            set
            {
                projectlist.Clear();
                if (!string.IsNullOrEmpty(value))
                {
                    var temppn = value.Replace("'", "");
                    var temppns = temppn.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var p in temppns)
                    {
                        projectlist.Add(p.Trim());
                    }
                }
            }

            get
            {
                var ret = "";
                foreach (var p in projectlist)
                {
                    if (string.IsNullOrEmpty(ret))
                        ret = p;
                    else
                        ret = ret + ";" + p;
                }
                return ret;
            }
        }


        private List<string> reporttypelist = new List<string>();

        public List<string> ReportTypeList
        {
            get
            { return reporttypelist; }
            set
            {
                reporttypelist.Clear();
                reporttypelist.AddRange(value);
            }
        }

        public string reporttypes
        {
            set
            {
                reporttypelist.Clear();
                if (!string.IsNullOrEmpty(value))
                {
                    var temppn = value.Replace("'", "");
                    var temppns = temppn.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var p in temppns)
                    {
                        reporttypelist.Add( p.Trim());
                    }
                }
            }

            get
            {
                var ret = "";
                foreach (var p in reporttypelist)
                {
                    if (string.IsNullOrEmpty(ret))
                        ret = p;
                    else
                        ret = ret + ";" + p;
                }
                return ret;
            }
        }


        public string reporttime { set; get; }

        public DateTime lastupdatetime { set; get; }

        public void StoreBookReportRecord()
        {
            if (!string.IsNullOrEmpty(projects) && !string.IsNullOrEmpty(reporttypes))
            {
                var sql = "delete from BookReportRecord where username = '<username>'";
                sql = sql.Replace("<username>", username);
                DBUtility.ExeLocalSqlNoRes(sql);

                sql = "insert into BookReportRecord(username,projects,reporttypes,reporttime,lastupdatetime) values('<username>','<projects>','<reporttypes>','<reporttime>','<lastupdatetime>')";
                sql = sql.Replace("<username>", username).Replace("<projects>", projects)
                    .Replace("<reporttypes>", reporttypes).Replace("<reporttime>", reporttime).Replace("<lastupdatetime>", lastupdatetime.ToString());
                DBUtility.ExeLocalSqlNoRes(sql);
            }
        }

        public static PJReportViewModels RetrieveBookReportRecord(string name)
        {
            var sql = "select username,projects,reporttypes,reporttime,lastupdatetime from BookReportRecord where username = '<username>'";
            sql = sql.Replace("<username>", name);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            if (dbret.Count > 0)
            {
                var ret = new PJReportViewModels();
                ret.username = Convert.ToString(dbret[0][0]);
                ret.projects = Convert.ToString(dbret[0][1]);
                ret.reporttypes = Convert.ToString(dbret[0][2]);
                ret.reporttime = Convert.ToString(dbret[0][3]);
                ret.lastupdatetime = DateTime.Parse(Convert.ToString(dbret[0][4]));
                return ret;
            }
            return null;
        }

        public static List<PJReportViewModels> RetrieveBookReportRecord()
        {
            var ret = new List<PJReportViewModels>();

            var sql = "select username,projects,reporttypes,reporttime,lastupdatetime from BookReportRecord";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach(var line in dbret)
            {
                var temp = new PJReportViewModels();
                temp.username = Convert.ToString(line[0]);
                temp.projects = Convert.ToString(line[1]);
                temp.reporttypes = Convert.ToString(line[2]);
                temp.reporttime = Convert.ToString(line[3]);
                temp.lastupdatetime = DateTime.Parse(Convert.ToString(line[4]));
                ret.Add(temp);
            }

            return ret;
        }

    }
}