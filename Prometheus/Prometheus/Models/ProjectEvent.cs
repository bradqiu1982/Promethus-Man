using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.Models
{

    public class ProjectEvent
    {
        public static string Pending = "Pending";
        public static string Working = "Working";
        public static string Done = "Done";

        public ProjectEvent()
        {
        }

        public ProjectEvent(string key, string ekey, string devent, string bdate)
        {
            ProjectKey = key;
            EventKey = ekey;
            dbEvent = devent;
            UpdateDate = DateTime.Parse(bdate);
        }

        public string ProjectKey { set; get; }
        public string EventKey { set; get; }

        private string sEvent = "";

        public string Event
        {
            set { sEvent = value; }
            get { return sEvent; }
        }

        public string dbEvent
        {
            get
            {
                if (string.IsNullOrEmpty(sEvent))
                {
                    return "";
                }
                else
                {
                    try
                    {
                        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(sEvent));
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
                    sEvent = "";
                }
                else
                {
                    try
                    {
                        sEvent = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(value));
                    }
                    catch (Exception)
                    {
                        sEvent = "";
                    }

                }

            }
        }

        public DateTime UpdateDate { set; get; }

        public static string GetUniqKey()
        {
            return Guid.NewGuid().ToString("N");
        }

        public void StoreEvent()
        {
            var sql = "insert into ProjectEvent(ProjectKey,EventKey,Event,UpdateDate) values('<ProjectKey>','<EventKey>','<Event>','<UpdateDate>')";
            sql = sql.Replace("<ProjectKey>", ProjectKey).Replace("<EventKey>", EventKey).Replace("<Event>", dbEvent).Replace("<UpdateDate>", UpdateDate.ToString());
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static List<ProjectEvent> RetrieveEvent(int topnum)
        {
            var ret = new List<ProjectEvent>();

            var sql = "select top <num> ProjectKey,EventKey,Event,UpdateDate from ProjectEvent order by UpdateDate DESC";
            sql = sql.Replace("<num>", Convert.ToString(topnum));
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var item in dbret)
            {
                var e = new ProjectEvent(Convert.ToString(item[0]), Convert.ToString(item[1]), Convert.ToString(item[2])
                    , Convert.ToString(item[3]));
                ret.Add(e);
            }

            return ret;
        }

        private static void UpdateEvent(string ekey, string Event)
        {
            var sql = "update ProjectEvent set Event = '<Event>',UpdateDate='<UpdateDate>' where EventKey = '<EventKey>'";
            sql = sql.Replace("<EventKey>", ekey).Replace("<Event>", Event).Replace("<UpdateDate>", DateTime.Now.ToString());
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        private static void BuildProjectEvent(string who, string projectkey, string projectname, string operate)
        {
            var wholink = "<a href = \"/User/UserCenter?username=" + who + "\" >" + who.Split(new char[] { '@' })[0] + "</a>";
            var projectlink = "<a href = \"/Project/ProjectIssues?ProjectKey=" + projectkey + "\" >" + projectname + "</a>";

            var vm = new ProjectEvent();
            vm.ProjectKey = projectkey;
            vm.EventKey = GetUniqKey();
            vm.Event = wholink + " " + operate + " " + projectlink;
            vm.UpdateDate = DateTime.Now;
            vm.StoreEvent();
        }

        public static void CreateProjectEvent(string who, string projectkey, string projectname)
        {
            BuildProjectEvent(who, projectkey, projectname, "created");
        }

        public static void UpdateProjectEvent(string who, string projectkey, string projectname)
        {
            BuildProjectEvent(who, projectkey, projectname, "updated");
        }

        public static void CreateIssueEvent(string projectkey, string reporter, string asignee, string summary, string evkey)
        {
            var reportlink = "<a href = \"/User/UserCenter?username=" + reporter + "\" >" + reporter.Split(new char[] { '@' })[0] + "</a>";
            var asigneelink = "<a href = \"/User/UserCenter?username=" + asignee + "\" >" + asignee.Split(new char[] { '@' })[0] + "</a>";
            var projectlink = "<a href = \"/Project/ProjectIssues?ProjectKey=" + projectkey + "\" >[" + projectkey + "] </a>";
            var issuelink = "<a href = \"/Issue/UpdateIssue?issuekey=" + evkey + "\" >" + summary + "</a>";

            var vm = new ProjectEvent();
            vm.ProjectKey = projectkey;
            vm.EventKey = evkey;
            vm.Event = projectlink + issuelink + " is created by "+ reportlink +" and asigned to "+ asigneelink;
            vm.UpdateDate = DateTime.Now;
            vm.StoreEvent();
        }

        public static void OperateIssueEvent(string projectkey, string who, string operate, string summary, string evkey)
        {
            var wholink = "<a href = \"/User/UserCenter?username=" + who + "\" >" + who.Split(new char[] { '@' })[0] + "</a>";
            var projectlink = "<a href = \"/Project/ProjectIssues?ProjectKey=" + projectkey + "\" >[" + projectkey + "] </a>";
            var issuelink = "<a href = \"/Issue/UpdateIssue?issuekey=" + evkey + "\" >" + summary + "</a>";

            var vm = new ProjectEvent();
            vm.Event = projectlink + issuelink + " is " + operate + " by " + wholink;
            UpdateEvent(evkey,vm.dbEvent);
        }

        public static void AssignIssueEvent(string projectkey, string who, string asignee, string summary, string evkey)
        {
            var wholink = "<a href = \"/User/UserCenter?username=" + who + "\" >" + who.Split(new char[] { '@' })[0] + "</a>";
            var aslink = "<a href = \"/User/UserCenter?username=" + asignee + "\" >" + asignee.Split(new char[] { '@' })[0] + "</a>";
            var projectlink = "<a href = \"/Project/ProjectIssues?ProjectKey=" + projectkey + "\" >[" + projectkey + "] </a>";
            var issuelink = "<a href = \"/Issue/UpdateIssue?issuekey=" + evkey + "\" >" + summary + "</a>";
            var vm = new ProjectEvent();
            vm.Event = projectlink + issuelink + " is assigned to " + aslink + " by " + wholink;
            UpdateEvent(evkey, vm.dbEvent);
        }

    }
}