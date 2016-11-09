using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prometheus.Models
{
    public class UserIssueViewModels
    {
        public UserIssueViewModels() { }

        public string CurrentUser { set; get; }
        public int PendingIssueCount { set; get; }
        public int WorkingIssueCount { set; get; }
        public int DoneIssueCount { set; get; }
        public string UserIssueTrend { set; get; }


        public static List<string> RetrieveAllICareUser(string me)
        {
            var ret = UserViewModels.RetrieveICare(me);
            ret.Sort();
            return ret;
        }

        public static List<UserIssueViewModels> RetrieveICareData(Controller ctrl, string me,int month)
        {
            var uselist = RetrieveAllICareUser(me);
            var ret = new List<UserIssueViewModels>();
            foreach (var item in uselist)
            {
                var tempuserdata = new UserIssueViewModels();
                tempuserdata.CurrentUser = item;
                IssueCountTrend(ctrl, tempuserdata, item, month);
                ret.Add(tempuserdata);
            }
            return ret;
        }

        public static List<UserIssueViewModels> RetrieveMyIssuerSummary(Controller ctrl, string me, int month)
        {
            var uselist = new List<string>();
            uselist.Add(me);

            var ret = new List<UserIssueViewModels>();
            foreach (var item in uselist)
            {
                var tempuserdata = new UserIssueViewModels();
                tempuserdata.CurrentUser = item;
                IssueCountTrend(ctrl, tempuserdata, item, month);
                ret.Add(tempuserdata);
            }
            return ret;
        }

        private static void IssueCountTrend(Controller ctrl, UserIssueViewModels uservm, string username,int month)
        {
            var edate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " 07:30:00");
            var sdate = edate.AddMonths(0-month);

            var allissue = IssueViewModels.RetrieveIssueAllByUser(username, sdate.ToString());
            var pendingissues = IssueViewModels.RetrieveIssuePendingByUser(username, sdate.ToString());
            var workingissues = IssueViewModels.RetrieveIssueWorkingByUser(username, sdate.ToString());
            var solvedissues = IssueViewModels.RetrieveIssueDoneByUser(username, sdate.ToString());

            uservm.PendingIssueCount = pendingissues.Count();
            uservm.WorkingIssueCount = workingissues.Count();
            uservm.DoneIssueCount = solvedissues.Count();

            var datelist = new List<string>();
            var pendinglist = new List<int>();
            var solvedlist = new List<int>();
            var pendingsum = 0;
            var slovedsum = 0;

            for (var temptime = sdate; temptime < edate;)
            {
                var tempstime = temptime;
                temptime = temptime.AddDays(1);

                foreach (var item in allissue)
                {
                    if (item.ReportDate < temptime && item.ReportDate >= tempstime)
                    {
                        pendingsum = pendingsum + 1;
                    }

                }

                foreach (var item in solvedissues)
                {
                    if (item.ResolvedDate <= temptime && item.ResolvedDate >= tempstime)
                    {
                        slovedsum = slovedsum + 1;
                    }
                }

                datelist.Add(temptime.ToString());
                pendinglist.Add(pendingsum);
                solvedlist.Add(slovedsum);
            }

            var ChartxAxisValues = "";
            var ChartSearies = "";
            //xaxis
            foreach (var item in datelist)
            {
                ChartxAxisValues = ChartxAxisValues + "'" + DateTime.Parse(item).ToString("yyyy-MM-dd") + "',";
            }
            ChartxAxisValues = ChartxAxisValues.Substring(0, ChartxAxisValues.Length - 1);


            //yaxis
            ChartSearies = "{name:'Issue Increase',data:[<fvalue>]},{name:'Issue Solved',data:[<cvalue>]}";

            var tempvalue = "";
            foreach (var item in pendinglist)
            {
                tempvalue = tempvalue + item.ToString() + ",";
            }
            tempvalue = tempvalue.Substring(0, tempvalue.Length - 1);
            ChartSearies = ChartSearies.Replace("<fvalue>", tempvalue);

            tempvalue = "";
            foreach (var item in solvedlist)
            {
                tempvalue = tempvalue + item.ToString("0.00") + ",";
            }
            tempvalue = tempvalue.Substring(0, tempvalue.Length - 1);
            ChartSearies = ChartSearies.Replace("<cvalue>", tempvalue);

            var tempscript = System.IO.File.ReadAllText(ctrl.Server.MapPath("~/Scripts/AreaChart.xml"));
            uservm.UserIssueTrend = tempscript.Replace("#ElementID#", (uservm.CurrentUser.Split(new char[] { '@' })[0]).Replace(".", "") + "issuetrendchart")
                .Replace("#Title#", "Issue Count Trend")
                .Replace("#ChartxAxisValues#", ChartxAxisValues)
                .Replace("#NAMEVALUEPAIRS#", ChartSearies);
        }

    }
}