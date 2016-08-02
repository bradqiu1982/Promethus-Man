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

        public ProjectEvent(string key, string ekey, string devent, string estatus, string bdate)
        {
            ProjectKey = key;
            EventKey = ekey;
            dbEvent = devent;
            EventStatus = estatus;
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

        public string EventStatus { set; get; }

        public DateTime UpdateDate { set; get; }

        public string Asignee { set; get; }

        private bool bsysenvent = true;
        public bool SystemEvent
        {
            set { bsysenvent = value; }
            get { return bsysenvent; }
        }

        public static string GetUniqKey()
        {
            return Guid.NewGuid().ToString("N");
        }

        public void StoreProjectEvent()
        {
            var sql = "insert into ProjectEvent(ProjectKey,EventKey,Event,EventStatus,UpdateDate) values('<ProjectKey>','<EventKey>','<Event>','<EventStatus>','<UpdateDate>')";
            sql = sql.Replace("<ProjectKey>", ProjectKey).Replace("<EventKey>", EventKey).Replace("<Event>", dbEvent).Replace("<EventStatus>", EventStatus).Replace("<UpdateDate>", UpdateDate.ToString());
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public void StoreUserEvent()
        {
            var sql = "insert into ProjectEvent(ProjectKey,EventKey,Event,EventStatus,UpdateDate,Asignee) values('<ProjectKey>','<EventKey>','<Event>','<EventStatus>','<UpdateDate>','<Asignee>')";
            sql = sql.Replace("<ProjectKey>", ProjectKey).Replace("<EventKey>", EventKey)
                .Replace("<Event>", dbEvent).Replace("<EventStatus>", EventStatus)
                .Replace("<UpdateDate>", UpdateDate.ToString()).Replace("<Asignee>", Asignee.ToUpper());
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static List<ProjectEvent> RetrieveProjectEvent(string pkey, string estatus, int topnum)
        {
            var ret = new List<ProjectEvent>();

            var sql = "select top <num> ProjectKey,EventKey,Event,EventStatus,UpdateDate from ProjectEvent where ProjectKey = '<ProjectKey>' and EventStatus = '<EventStatus>' order by UpdateDate DESC";
            sql = sql.Replace("<ProjectKey>", pkey).Replace("<EventStatus>", estatus).Replace("<num>", Convert.ToString(topnum));
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var item in dbret)
            {
                var e = new ProjectEvent(Convert.ToString(item[0]), Convert.ToString(item[1]), Convert.ToString(item[2])
                    , Convert.ToString(item[3]), Convert.ToString(item[4]));
                ret.Add(e);
            }

            return ret;
        }




        public static List<ProjectEvent> RetrieveSystemEvent(int topnum)
        {
            var ret = new List<ProjectEvent>();

            var sql = "select top <num> ProjectKey,EventKey,Event,EventStatus,UpdateDate from ProjectEvent order by UpdateDate DESC";
            sql = sql.Replace("<num>", Convert.ToString(topnum));
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var item in dbret)
            {
                var e = new ProjectEvent(Convert.ToString(item[0]), Convert.ToString(item[1]), Convert.ToString(item[2])
                    , Convert.ToString(item[3]), Convert.ToString(item[4]));
                ret.Add(e);
            }

            return ret;
        }

        public static List<ProjectEvent> RetrieveUserEvent(string username, string estatus, int topnum)
        {
            var ret = new List<ProjectEvent>();

            var sql = "select top <num> ProjectKey,EventKey,Event,EventStatus,UpdateDate,Asignee from ProjectEvent where Asignee = '<Asignee>' and EventStatus = '<EventStatus>' order by UpdateDate DESC";
            sql = sql.Replace("<Asignee>", username.ToUpper()).Replace("<EventStatus>", estatus).Replace("<num>", Convert.ToString(topnum));
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var item in dbret)
            {
                var e = new ProjectEvent(Convert.ToString(item[0]), Convert.ToString(item[1]), Convert.ToString(item[2])
                    , Convert.ToString(item[3]), Convert.ToString(item[4]));
                e.SystemEvent = false;
                ret.Add(e);
            }

            return ret;
        }

        public static void UpdateEventStatus(string ekey, string estaus)
        {
            var sql = "update ProjectEvent set EventStatus = '<EventStatus>' where EventKey = '<EventKey>'";
            sql = sql.Replace("<EventStatus>", estaus).Replace("<EventKey>", ekey);
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
            vm.EventStatus = ProjectEvent.Done;
            vm.UpdateDate = DateTime.Now;
            vm.StoreProjectEvent();
        }

        public static void CreateProjectEvent(string who, string projectkey, string projectname)
        {
            BuildProjectEvent(who, projectkey, projectname, "create");
        }

        public static void UpdateProjectEvent(string who, string projectkey, string projectname)
        {
            BuildProjectEvent(who, projectkey, projectname, "update");
        }

        public static void CreateUserEvent(string projectkey, string reporter, string asignee, string summary, string evkey)
        {
            var reportlink = "<a href = \"/User/UserCenter?username=" + reporter + "\" >" + reporter.Split(new char[] { '@' })[0] + "</a>";
            var asigneelink = "<a href = \"/User/UserCenter?username=" + asignee + "\" >" + asignee.Split(new char[] { '@' })[0] + "</a>";
            var projectlink = "<a href = \"/Project/ProjectIssues?ProjectKey=" + projectkey + "\" >" + projectkey + "</a>";
            var issuelink = "<a href = \"/Issue/UpdateIssue?issuekey=" + evkey + "\" >" + summary + "</a>";

            var vm = new ProjectEvent();
            vm.ProjectKey = projectkey;
            vm.EventKey = evkey;
            vm.Event = issuelink + " is create by "+ reportlink + " in project "+ projectlink+" and asigned to "+ asigneelink;
            vm.EventStatus = ProjectEvent.Pending;
            vm.UpdateDate = DateTime.Now;
            vm.Asignee = asignee;
            vm.StoreUserEvent();
        }

    }
}