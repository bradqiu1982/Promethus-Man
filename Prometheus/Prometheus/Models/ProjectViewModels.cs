using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.Models
{
    
    public class ProjectStation
    {
        public ProjectStation(string key, string stat)
        {
            ProjectKey = key;
            Station = stat;
        }

        public string ProjectKey { set; get; }
        public string Station { set; get; }
    }

    public class ProjectMembers
    {
        public ProjectMembers(string key, string sname,string srole)
        {
            ProjectKey = key;
            Name = sname;
            Role = srole;
        }

        public string ProjectKey { set; get; }
        public string Name { set; get; }
        public string Role { set; get; }
    }

    public class ProjectPn
    {
        public ProjectPn(string key, string p)
        {
            ProjectKey = key;
            Pn = p;
        }
        public string ProjectKey { set; get; }
        public string Pn { set; get; }
    }

    public class ProjectMesTable
    {
        public ProjectMesTable(string key,string stat,string table)
        {
            ProjectKey = key;
            Station = stat;
            TableName = table;
        }

        public string ProjectKey { set; get; }
        public string Station { set; get; }
        public string TableName { set; get; }
    }

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
            BuildDate = DateTime.Parse(bdate);
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

        public DateTime BuildDate { set; get; }

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
            var sql = "insert into ProjectEvent(ProjectKey,EventKey,Event,EventStatus,BuildDate) values('<ProjectKey>','<EventKey>','<Event>','<EventStatus>','<BuildDate>')";
            sql = sql.Replace("<ProjectKey>", ProjectKey).Replace("<EventKey>", EventKey).Replace("<Event>", dbEvent).Replace("<EventStatus>", EventStatus).Replace("<BuildDate>", BuildDate.ToString());
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static List<ProjectEvent> RetrieveProjectEvent(string pkey,string estatus,int topnum)
        {
            var ret = new List<ProjectEvent>();

            var sql = "select top <num> ProjectKey,EventKey,Event,EventStatus,BuildDate from ProjectEvent where ProjectKey = '<ProjectKey>' and EventStatus = '<EventStatus>' order by BuildDate DESC";
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


        public static List<ProjectEvent> RetrieveProjectEvent(int topnum)
        {
            var ret = new List<ProjectEvent>();

            var sql = "select top <num> ProjectKey,EventKey,Event,EventStatus,BuildDate from ProjectEvent order by BuildDate DESC";
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

        public static void UpdateEventStatus(string ekey, string estaus)
        {
            var sql = "update ProjectEvent set EventStatus = '<EventStatus>' where EventKey = '<EventKey>'";
            sql = sql.Replace("<EventStatus>", estaus).Replace("<EventKey>", ekey);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        private static void BuildProjectEvent(string who, string projectkey, string projectname,string operate)
        {
            var wholink = "<a href = \"/User/UserCenter?username=" + who + "\" >"+who.Split(new char[] {'@'})[0]+"</a>";
            var projectlink = "<a href = \"/Project/ProjectIssues?ProjectKey=" + projectkey + "\" >"+ projectname + "</a>";

            var vm = new ProjectEvent();
            vm.ProjectKey = projectkey;
            vm.EventKey = GetUniqKey();
            vm.Event = wholink + " " + operate + " " + projectlink;
            vm.EventStatus = ProjectEvent.Done;
            vm.BuildDate = DateTime.Now;
            vm.StoreProjectEvent();
        }

        public static void CreateProjectEvent(string who,string projectkey,string projectname)
        {
            BuildProjectEvent(who, projectkey, projectname, "create");
        }

        public static void UpdateProjectEvent(string who, string projectkey, string projectname)
        {
            BuildProjectEvent(who, projectkey, projectname, "update");
        }

    }

    public class ProjectViewModels
    {
        public static string PMROLE = "PM";
        public static string ENGROLE = "ENG";

        public ProjectViewModels()
        {
            
        }

        public ProjectViewModels(string prokey, string proname, string startdate, double finshrate, string sdescription)
        {
            this.ProjectKey = prokey;
            this.dbProjectName = proname;
            this.StartDate = DateTime.Parse(startdate);
            this.FinishRating = finshrate;
            this.dbDescription = sdescription;
        }

        public string ProjectKey { set; get; }

        public string sProjectName = "";

        public string ProjectName {
            set { sProjectName = value; }
            get { return sProjectName; }
        }

        public string dbProjectName
        {
            get
            {
                if (string.IsNullOrEmpty(sProjectName))
                {
                    return "";
                }
                else
                {
                    try
                    {
                        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(sProjectName));
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
                    sProjectName = "";
                }
                else
                {
                    try
                    {
                        sProjectName = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(value));
                    }
                    catch (Exception)
                    {
                        sProjectName = "";
                    }

                }

            }
        }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { set; get; }


        [RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)")]
        public string PM { set; get; }

        //[StringLength(260, MinimumLength = 6)]
        //[Required]
        //[RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+).*")]
        //public string Engineers { set; get; }

        public double FinishRating { set; get; }


        private string sDescription = "";

        [StringLength(90, MinimumLength = 6)]
        [Required]
        public string Description {
            get
            {
                return sDescription;
            }

            set
            {
                sDescription = value;
            }
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

        private List<ProjectStation> lstation = new List<ProjectStation>();
        public List<ProjectStation> StationList
        {
            get
            { return lstation; }
            set
            {
                lstation.Clear();
                lstation.AddRange(value);
            }
        }

        private List<ProjectPn> lpn = new List<ProjectPn>();
        public List<ProjectPn> PNList
        {
            get
            { return lpn; }
            set
            {
                lpn.Clear();
                lpn.AddRange(value);
            }
        }

        private List<ProjectMesTable> ltab = new List<ProjectMesTable>();
        public List<ProjectMesTable> TabList
        {
            get
            { return ltab; }
            set
            {
                ltab.Clear();
                ltab.AddRange(value);
            }
        }

        private List<ProjectMembers> lmeb = new List<ProjectMembers>();
        public List<ProjectMembers> MemberList
        {
            get
            { return lmeb; }
            set
            {
                lmeb.Clear();
                lmeb.AddRange(value);
            }
        }


        private void StoreProjectBaseInfo()
        {
            var sql = "delete from Project where ProjectKey = '<ProjectKey>'";
            sql = sql.Replace("<ProjectKey>", ProjectKey);
            DBUtility.ExeLocalSqlNoRes(sql);

            sql = "insert into Project(ProjectKey,ProjectName,StartDate,Description,FinishRate) values('<ProjectKey>','<ProjectName>','<StartDate>','<Description>',<FinishRate>)";
            sql = sql.Replace("<ProjectKey>", ProjectKey).Replace("<ProjectName>", dbProjectName).Replace("<StartDate>", StartDate.ToString("yyyy-MM-dd")).Replace("<Description>", dbDescription).Replace("<FinishRate>", Convert.ToString(FinishRating));
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        private void StoreProjectMembers()
        {
            if (MemberList.Count > 0)
            {
                var sql = "delete from ProjectMembers where ProjectKey = '<ProjectKey>'";
                sql = sql.Replace("<ProjectKey>", ProjectKey);
                DBUtility.ExeLocalSqlNoRes(sql);
            }

            foreach (var item in MemberList)
            {
                var sql = "insert into ProjectMembers(ProjectKey,Name,Role) values('<ProjectKey>','<Name>','<Role>')";
                sql = sql.Replace("<ProjectKey>", ProjectKey).Replace("<Name>", item.Name).Replace("<Role>", item.Role);
                DBUtility.ExeLocalSqlNoRes(sql);
            }
        }

        private void StoreProjectMesTable()
        {
            if (TabList.Count > 0)
            {
                var sql = "delete from ProjectMesTable where ProjectKey = '<ProjectKey>'";
                sql = sql.Replace("<ProjectKey>", ProjectKey);
                DBUtility.ExeLocalSqlNoRes(sql);
            }

            foreach (var item in TabList)
            {
                var sql = "insert into ProjectMesTable(ProjectKey,Station,TableName) values('<ProjectKey>','<Station>','<TableName>')";
                sql = sql.Replace("<ProjectKey>", ProjectKey).Replace("<Station>", item.Station).Replace("<TableName>", item.TableName);
                DBUtility.ExeLocalSqlNoRes(sql);
            }
        }

        private void StoreProjectPN()
        {
            if (PNList.Count > 0)
            {
                var sql = "delete from ProjectPn where ProjectKey = '<ProjectKey>'";
                sql = sql.Replace("<ProjectKey>", ProjectKey);
                DBUtility.ExeLocalSqlNoRes(sql);
            }

            foreach (var item in PNList)
            {
                var sql = "insert into ProjectPn(ProjectKey,PN) values('<ProjectKey>','<PN>')";
                sql = sql.Replace("<ProjectKey>", ProjectKey).Replace("<PN>", item.Pn);
                DBUtility.ExeLocalSqlNoRes(sql);
            }
        }

        private void StoreProjectStation()
        {
            if (StationList.Count > 0)
            {
                var sql = "delete from ProjectStation where ProjectKey = '<ProjectKey>'";
                sql = sql.Replace("<ProjectKey>", ProjectKey);
                DBUtility.ExeLocalSqlNoRes(sql);
            }

            foreach (var item in StationList)
            {
                var sql = "insert into ProjectStation(ProjectKey,Station) values('<ProjectKey>','<Station>')";
                sql = sql.Replace("<ProjectKey>", ProjectKey).Replace("<Station>", item.Station);
                DBUtility.ExeLocalSqlNoRes(sql);
            }
        }

        public void StoreProject()
        {
            StoreProjectBaseInfo();
            StoreProjectMembers();
            StoreProjectMesTable();
            StoreProjectPN();
            StoreProjectStation();
        }

        public bool CheckExistProject()
        {
            var sql = "select * from Project where ProjectKey = '<ProjectKey>'";
            sql = sql.Replace("<ProjectKey>", ProjectKey);
            var ret = DBUtility.ExeLocalSqlWithRes(sql);
            if (ret.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static List<string> RetrieveAllProjectKey()
        {
            var sql = "select ProjectKey from Project";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            var ret = new List<string>();
            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString( line[0]));
            }
            return ret;
        }

        private static List<ProjectMembers> RetrieveProjectMembers(string key)
        {
            var ret = new List<ProjectMembers>();

            var sql = "select Name,Role from ProjectMembers where ProjectKey = '<ProjectKey>'";
            sql = sql.Replace("<ProjectKey>", key);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var m = new ProjectMembers(key, Convert.ToString(line[0]), Convert.ToString(line[1]));
                ret.Add(m);
            }

            return ret;
        }
        private static List<ProjectMesTable> RetrieveProjectMesTable(string key)
        {
            var ret = new List<ProjectMesTable>();

            var sql = "select Station,TableName from ProjectMesTable where ProjectKey = '<ProjectKey>'";
            sql = sql.Replace("<ProjectKey>", key);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var m = new ProjectMesTable(key, Convert.ToString(line[0]), Convert.ToString(line[1]));
                ret.Add(m);
            }
            return ret;
        }

        private static List<ProjectPn> RetrieveProjectPn(string key)
        {
            var ret = new List<ProjectPn>();

            var sql = "select PN from ProjectPn where ProjectKey = '<ProjectKey>'";
            sql = sql.Replace("<ProjectKey>", key);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);

            foreach (var line in dbret)
            {
                var m = new ProjectPn(key, Convert.ToString(line[0]));
                ret.Add(m);
            }
            return ret;
        }

        private static List<ProjectStation> RetrieveProjectStation(string key)
        {
            var ret = new List<ProjectStation>();

            var sql = "select Station from ProjectStation where ProjectKey = '<ProjectKey>'";
            sql = sql.Replace("<ProjectKey>", key);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);

            foreach (var line in dbret)
            {
                var m = new ProjectStation(key, Convert.ToString(line[0]));
                ret.Add(m);
            }
            return ret;
        }

        public static ProjectViewModels RetrieveOneProject(string key)
        {
            var sql = "select ProjectKey,ProjectName,StartDate,FinishRate,Description from Project where ProjectKey = '<ProjectKey>' and validate = 1";
            sql = sql.Replace("<ProjectKey>", key);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            if (dbret.Count > 0)
            {
                var ret = new ProjectViewModels(Convert.ToString(dbret[0][0])
                    , Convert.ToString(dbret[0][1]), Convert.ToString(dbret[0][2])
                    , Convert.ToDouble(dbret[0][3]), Convert.ToString(dbret[0][4]));

                ret.MemberList = RetrieveProjectMembers(key);
                ret.TabList = RetrieveProjectMesTable(key);
                ret.PNList = RetrieveProjectPn(key);
                ret.StationList = RetrieveProjectStation(key);
                return ret;
            }
            else
                return null;

        }

        public static List<ProjectViewModels> RetrieveAllProject()
        {
            var ret = new List<ProjectViewModels>();
            var keys = RetrieveAllProjectKey();
            foreach (var key in keys)
            {
                var r = RetrieveOneProject(key);
                if (r != null)
                {
                    ret.Add(r);
                }
            }
            return ret;
        }
    }

    }
