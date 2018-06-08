using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.Models
{
    public class ProjectTypeInf
    {
        public static string Parallel = "Parallel";
        public static string Tunable = "Tunable";
        public static string OSA = "OSA";
        public static string LineCard = "LineCard";
        public static string QM = "QM";
        public static string Others = "Others";
    }

    public class ProjectStatus
    {
        public static string Open = "Open";
        public static string Close = "Close";
    }

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
        public ProjectMembers(string key, string sname, string srole)
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
        public ProjectMesTable(string key, string stat, string table)
        {
            ProjectKey = key;
            Station = stat;
            TableName = table;
        }

        public string ProjectKey { set; get; }
        public string Station { set; get; }
        public string TableName { set; get; }
    }

    public class ProjectExcept
    {
        public ProjectExcept()
        {
            ProjectKey = string.Empty;
            Except = string.Empty;
            ExceptType = string.Empty;
        }
        public string ProjectKey { set; get; }
        public string Except { set; get; }
        public string ExceptType { set; get; }
    }

    public class ProjectExceptType
    {
        public static string WAFERYIELDEXCEPT = "WAFERYIELDEXCEPT";
        public static string PROCESSEXCEPT = "PROCESSEXCEPT";
    }

    public class ProjectViewModels
    {
        public static string PMROLE = "PM";
        public static string ENGROLE = "ENG";
        public static string PEROLE = "PE";
        public static string MEROLE = "ME";
        public static string PQEROLE = "PQE";

        public ProjectViewModels()
        {
            MonitorVcsel = "";
            ProjectType = "";
            PMListStr = string.Empty;
            EngListStr = string.Empty;
            PeListStr = string.Empty;
            MeListStr = string.Empty;
            PqeListStr = string.Empty;
            TransferFlg = string.Empty;
            ProStatus = ProjectStatus.Open;
        }

        public ProjectViewModels(string prokey, string proname, string startdate, double finshrate, string sdescription, string monitorvcsel, string vcselwarning, string pjtype, string transfer_flg = "", string status = "")
        {
            this.ProjectKey = prokey;
            this.dbProjectName = proname;
            this.StartDate = DateTime.Parse(startdate);
            this.FinishRating = finshrate;
            this.dbDescription = sdescription;
            this.MonitorVcsel = monitorvcsel;
            this.ProjectType = pjtype;
            this.TransferFlg = transfer_flg;
            this.ProStatus = status;
            if (string.IsNullOrEmpty(vcselwarning))
            {
                this.VcselWarningYield = "98.0";
            }
            else
            {
                try
                { this.VcselWarningYield = Convert.ToDouble(vcselwarning).ToString("0.0"); }
                catch (Exception ex) { this.VcselWarningYield = Convert.ToDouble(98).ToString("0.0"); }
            }

            WaferYieldExceptList = "";
            PMListStr = string.Empty;
            EngListStr = string.Empty;
            PeListStr = string.Empty;
            MeListStr = string.Empty;
            PqeListStr = string.Empty;
        }

        public string PMListStr { set; get; }
        public string EngListStr { set; get; }
        public string PeListStr { set; get; }
        public string MeListStr { set; get; }
        public string PqeListStr { set; get; }
        public string TransferFlg { set; get; }
        public string ProStatus { set; get; }

        public string WaferYieldExceptList { set; get; }

        public string ProjectKey { set; get; }

        public string ProjectType { set; get; }

        public string sProjectName = "";

        public string ProjectName
        {
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


        //[StringLength(260, MinimumLength = 6)]
        //[Required]
        //[RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+).*")]
        //public string Engineers { set; get; }

        public double FinishRating { set; get; }

        public string MonitorVcsel { set; get; }

        public string VcselWarningYield { set; get; }

        private string sDescription = "";

        [StringLength(90, MinimumLength = 6)]
        [Required]
        public string Description
        {
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

        public string Stations
        {
            set
            {
                lstation.Clear();
                if (!string.IsNullOrEmpty(value))
                {
                    var tempstat = value.Replace("'", "");
                    var tempstats = tempstat.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var s in tempstats)
                    {
                        lstation.Add(new ProjectStation(ProjectKey, s.Trim()));
                    }
                }
            }

            get
            {
                var ret = "";
                foreach (var p in lstation)
                {
                    if (string.IsNullOrEmpty(ret))
                        ret = p.Station;
                    else
                        ret = ret + ";" + p.Station;
                }
                return ret;
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

        public string PNs
        {
            set
            {
                lpn.Clear();
                if (!string.IsNullOrEmpty(value))
                {
                    var temppn = value.Replace("'", "");
                    var temppns = temppn.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var p in temppns)
                    {
                        lpn.Add(new ProjectPn(ProjectKey, p.Trim()));
                    }
                }
            }

            get
            {
                var ret = "";
                foreach (var p in lpn)
                {
                    if (string.IsNullOrEmpty(ret))
                        ret = p.Pn;
                    else
                        ret = ret + ";" + p.Pn;
                }
                return ret;
            }
        }

        private List<ProjectPn> lmd = new List<ProjectPn>();
        public List<ProjectPn> MDIDList
        {
            get
            { return lmd; }
            set
            {
                lmd.Clear();
                lmd.AddRange(value);
            }
        }

        public string ModelIDs
        {
            set
            {
                lmd.Clear();
                if (!string.IsNullOrEmpty(value))
                {
                    var temppn = value.Replace("'", "");
                    var temppns = temppn.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var p in temppns)
                    {
                        lmd.Add(new ProjectPn(ProjectKey, p.Trim()));
                    }
                }
            }

            get
            {
                var ret = "";
                foreach (var p in lmd)
                {
                    if (string.IsNullOrEmpty(ret))
                        ret = p.Pn;
                    else
                        ret = ret + ";" + p.Pn;
                }
                return ret;
            }
        }

        private List<ProjectStation> lsumdataset = new List<ProjectStation>();
        public List<ProjectStation> SumDatasetList
        {
            get
            { return lsumdataset; }
            set
            {
                lsumdataset.Clear();
                lsumdataset.AddRange(value);
            }
        }

        public string SumDatasets
        {
            set
            {
                lsumdataset.Clear();
                if (!string.IsNullOrEmpty(value))
                {
                    var tempstat = value.Replace("'", "");
                    var tempstats = tempstat.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var s in tempstats)
                    {
                        lsumdataset.Add(new ProjectStation(ProjectKey, s.Trim()));
                    }
                }
            }

            get
            {
                var ret = "";
                foreach (var p in lsumdataset)
                {
                    if (string.IsNullOrEmpty(ret))
                        ret = p.Station;
                    else
                        ret = ret + ";" + p.Station;
                }
                return ret;
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
        private List<OSAFailureVM> osatab = new List<OSAFailureVM>();
        public List<OSAFailureVM> OSATabList
        {
            get
            { return osatab; }
            set
            {
                osatab.Clear();
                osatab.AddRange(value);
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

        public string AllPJMember
        {
            get
            {
                var allmemb = string.Empty;
                foreach (var eg in lmeb)
                {
                    allmemb = allmemb + ";" + eg.Name;
                }
                return allmemb;
            }
        }

        //public string DVTDate { set; get; }
        //public string DVTStatus { set; get; }
        //public string DVTIssueKey { set; get; }

        //public string MVTDate { set; get; }
        //public string MVTStatus { set; get; }
        //public string MVTIssueKey { set; get; }

        public string PIP1Date { set; get; }

        public string MVTDate { set; get; }

        public string CurrentNPIProc { set; get; }
        public string CurrentNPIProcKey { set; get; }

        public double FirstYield { set; get; }
        public double RetestYield { set; get; }
        public string PendingTaskCount { set; get; }
        public string PendingFACount { set; get; }
        public string PendingRMACount { set; get; }
        public string PendingSptCount { set; get; }

        private void StoreProjectBaseInfo()
        {
            var sql = "delete from Project where ProjectKey = '<ProjectKey>'";
            sql = sql.Replace("<ProjectKey>", ProjectKey);
            DBUtility.ExeLocalSqlNoRes(sql);

            sql = "insert into Project(ProjectKey,ProjectName,StartDate,Description,FinishRate,APVal1,APVal2,ProjectType,databackuptm) values('<ProjectKey>','<ProjectName>','<StartDate>','<Description>',<FinishRate>,'<MonitorVcsel>','<VcselWarningYield>','<ProjectType>','<databackuptm>')";
            sql = sql.Replace("<ProjectKey>", ProjectKey).Replace("<ProjectName>", dbProjectName)
                .Replace("<StartDate>", StartDate.ToString("yyyy-MM-dd"))
                .Replace("<Description>", dbDescription).Replace("<FinishRate>", Convert.ToString(FinishRating))
                .Replace("<MonitorVcsel>", MonitorVcsel).Replace("<VcselWarningYield>", VcselWarningYield)
                .Replace("<ProjectType>", ProjectType).Replace("<databackuptm>", DateTime.Now.ToString());
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
                var sql = "insert into ProjectMembers(ProjectKey,Name,Role,databackuptm) values('<ProjectKey>','<Name>','<Role>','<databackuptm>')";
                sql = sql.Replace("<ProjectKey>", ProjectKey).Replace("<Name>", item.Name).Replace("<Role>", item.Role).Replace("<databackuptm>", DateTime.Now.ToString());
                DBUtility.ExeLocalSqlNoRes(sql);

                UserViewModels.UpdateUserProject(item.Name.ToUpper(), ProjectKey);
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
                var sql = "insert into ProjectMesTable(ProjectKey,Station,TableName,databackuptm) values('<ProjectKey>','<Station>','<TableName>','<databackuptm>')";
                sql = sql.Replace("<ProjectKey>", ProjectKey).Replace("<Station>", item.Station).Replace("<TableName>", item.TableName).Replace("<databackuptm>", DateTime.Now.ToString());
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
                var sql = "insert into ProjectPn(ProjectKey,PN,databackuptm) values('<ProjectKey>','<PN>','<databackuptm>')";
                sql = sql.Replace("<ProjectKey>", ProjectKey).Replace("<PN>", item.Pn).Replace("<databackuptm>", DateTime.Now.ToString());
                DBUtility.ExeLocalSqlNoRes(sql);
            }
        }

        private void StoreProjectModelID()
        {
            if (MDIDList.Count > 0)
            {
                var sql = "delete from ProjectModelID where ProjectKey = '<ProjectKey>'";
                sql = sql.Replace("<ProjectKey>", ProjectKey);
                DBUtility.ExeLocalSqlNoRes(sql);
            }

            foreach (var item in MDIDList)
            {
                var sql = "insert into ProjectModelID(ProjectKey,ModelID,databackuptm) values('<ProjectKey>','<ModelID>','<databackuptm>')";
                sql = sql.Replace("<ProjectKey>", ProjectKey).Replace("<ModelID>", item.Pn).Replace("<databackuptm>", DateTime.Now.ToString());
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
                var sql = "insert into ProjectStation(ProjectKey,Station,databackuptm) values('<ProjectKey>','<Station>','<databackuptm>')";
                sql = sql.Replace("<ProjectKey>", ProjectKey).Replace("<Station>", item.Station).Replace("<databackuptm>", DateTime.Now.ToString());
                DBUtility.ExeLocalSqlNoRes(sql);
            }
        }

        public static void StoreOSAStation(string pjkey, List<string> stationlist)
        {
            if (stationlist.Count > 0)
            {
                var sql = "delete from ProjectStation where ProjectKey = '<ProjectKey>'";
                sql = sql.Replace("<ProjectKey>", pjkey);
                DBUtility.ExeLocalSqlNoRes(sql);
            }

            foreach (var item in stationlist)
            {
                var sql = "insert into ProjectStation(ProjectKey,Station,databackuptm) values('<ProjectKey>','<Station>','<databackuptm>')";
                sql = sql.Replace("<ProjectKey>", pjkey).Replace("<Station>", item).Replace("<databackuptm>", DateTime.Now.ToString());
                DBUtility.ExeLocalSqlNoRes(sql);
            }
        }

        private void StoreProjectSumDataSet()
        {
            if (SumDatasetList.Count > 0)
            {
                var sql = "delete from ProjectSumDataSet where ProjectKey = '<ProjectKey>'";
                sql = sql.Replace("<ProjectKey>", ProjectKey);
                DBUtility.ExeLocalSqlNoRes(sql);
            }

            foreach (var item in SumDatasetList)
            {
                var sql = "insert into ProjectSumDataSet(ProjectKey,SumDataSet,databackuptm) values('<ProjectKey>','<SumDataSet>','<databackuptm>')";
                sql = sql.Replace("<ProjectKey>", ProjectKey).Replace("<SumDataSet>", item.Station).Replace("<databackuptm>", DateTime.Now.ToString());
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
            StoreProjectModelID();
            StoreProjectSumDataSet();
        }

        public bool CheckExistProject()
        {
            var sql = "select * from Project where ProjectKey = '<ProjectKey>'";
            sql = sql.Replace("<ProjectKey>", ProjectKey);
            var ret = DBUtility.ExeLocalSqlWithRes(sql, null);
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
            var sql = "select ProjectKey from Project where APVal5 <> 'Close' order by ProjectKey ASC";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            var ret = new List<string>();
            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString(line[0]));
            }
            return ret;
        }

        public static List<ProjectMembers> RetrieveProjectMembers(string key)
        {
            var ret = new List<ProjectMembers>();

            var sql = "select Name,Role from ProjectMembers where ProjectKey = '<ProjectKey>'";
            sql = sql.Replace("<ProjectKey>", key);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
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
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
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
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);

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
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);

            foreach (var line in dbret)
            {
                var m = new ProjectStation(key, Convert.ToString(line[0]));
                ret.Add(m);
            }
            return ret;
        }

        private static List<ProjectPn> RetrieveProjectModelID(string key)
        {
            var ret = new List<ProjectPn>();

            var sql = "select ModelID from ProjectModelID where ProjectKey = '<ProjectKey>'";
            sql = sql.Replace("<ProjectKey>", key);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);

            foreach (var line in dbret)
            {
                var m = new ProjectPn(key, Convert.ToString(line[0]));
                ret.Add(m);
            }
            return ret;
        }

        private static List<ProjectStation> RetrieveProjectSumDataSet(string key)
        {
            var ret = new List<ProjectStation>();

            var sql = "select SumDataSet from ProjectSumDataSet where ProjectKey = '<ProjectKey>'";
            sql = sql.Replace("<ProjectKey>", key);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);

            foreach (var line in dbret)
            {
                var m = new ProjectStation(key, Convert.ToString(line[0]));
                ret.Add(m);
            }
            return ret;
        }


        public static List<ProjectViewModels> RetrieveOneProject(string key)
        {
            var ret = new List<ProjectViewModels>();
            var sql = "select ProjectKey,ProjectName,StartDate,FinishRate,Description,APVal1,APVal2,ProjectType,APVal4 from Project where APVal5 <> 'Close' and ProjectKey = '<ProjectKey>' and validate = 1";
            sql = sql.Replace("<ProjectKey>", key);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            if (dbret.Count > 0)
            {
                var tempvm = new ProjectViewModels(Convert.ToString(dbret[0][0])
                    , Convert.ToString(dbret[0][1]), Convert.ToString(dbret[0][2])
                    , Convert.ToDouble(dbret[0][3]), Convert.ToString(dbret[0][4])
                    , Convert.ToString(dbret[0][5]), Convert.ToString(dbret[0][6])
                    , Convert.ToString(dbret[0][7]), Convert.ToString(dbret[0][8]));

                tempvm.MemberList = RetrieveProjectMembers(key);
                tempvm.TabList = RetrieveProjectMesTable(key);
                tempvm.PNList = RetrieveProjectPn(key);
                tempvm.StationList = RetrieveProjectStation(key);
                tempvm.MDIDList = RetrieveProjectModelID(key);
                tempvm.SumDatasetList = RetrieveProjectSumDataSet(key);
                ret.Add(tempvm);
            }

            return ret;
        }

        public static List<ProjectViewModels> RetrieveOneProjectWithClose(string key)
        {
            var ret = new List<ProjectViewModels>();
            var sql = "select ProjectKey,ProjectName,StartDate,FinishRate,Description,APVal1,APVal2,ProjectType,APVal4 from Project where ProjectKey = '<ProjectKey>' and validate = 1";
            sql = sql.Replace("<ProjectKey>", key);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            if (dbret.Count > 0)
            {
                var tempvm = new ProjectViewModels(Convert.ToString(dbret[0][0])
                    , Convert.ToString(dbret[0][1]), Convert.ToString(dbret[0][2])
                    , Convert.ToDouble(dbret[0][3]), Convert.ToString(dbret[0][4])
                    , Convert.ToString(dbret[0][5]), Convert.ToString(dbret[0][6])
                    , Convert.ToString(dbret[0][7]), Convert.ToString(dbret[0][8]));

                tempvm.MemberList = RetrieveProjectMembers(key);
                tempvm.TabList = RetrieveProjectMesTable(key);
                tempvm.PNList = RetrieveProjectPn(key);
                tempvm.StationList = RetrieveProjectStation(key);
                tempvm.MDIDList = RetrieveProjectModelID(key);
                tempvm.SumDatasetList = RetrieveProjectSumDataSet(key);
                ret.Add(tempvm);
            }

            return ret;
        }


        public static List<ProjectViewModels> RetrieveAllProject()
        {
            var ret = new List<ProjectViewModels>();
            var keys = RetrieveAllProjectKey();
            foreach (var key in keys)
            {
                var r = RetrieveOneProject(key);
                if (r.Count>0)
                {
                    ret.Add(r[0]);
                }
            }
            return ret;
        }

        public static List<ProjectViewModels> N_RetrieveProjectInfo(string pKey = "")
        {
            var ret = new List<ProjectViewModels>();
            var sql = @"select ProjectKey, ProjectName, ProjectType, 
                    FinishRate from Project
                    where APVal5 <> 'Close' ";
            var param = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(pKey))
            {
                sql += " and ProjectKey = @pKey";
                param.Add("@pKey", pKey);
            }
            sql += " order by ProjectKey ASC";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, param);
            foreach (var line in dbret)
            {
                var tmp = new ProjectViewModels();
                tmp.ProjectKey = Convert.ToString(line[0]);
                tmp.dbProjectName = Convert.ToString(line[1]);
                tmp.ProjectType = Convert.ToString(line[2]);
                tmp.FinishRating = Convert.ToDouble(line[3]);

                tmp.StationList = RetrieveProjectStation(Convert.ToString(line[0]));
                tmp.SumDatasetList = RetrieveProjectSumDataSet(Convert.ToString(line[0]));

                ret.Add(tmp);
            }
            return ret;
        }

        public static void UpdateProjectExcept(string ProjectKey, string Except, string ExceptType)
        {
            var sql = "select ProjectKey from ProjectException where ExceptionType='<ExceptType>'";
            sql = sql.Replace("<ExceptType>", ExceptType);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            if (dbret.Count > 0)
            {
                sql = "update ProjectException set Exception='<Except>' where ExceptionType='<ExceptType>'";
                sql = sql.Replace("<ProjectKey>", ProjectKey).Replace("<ExceptType>", ExceptType).Replace("<Except>", Except);
                DBUtility.ExeLocalSqlNoRes(sql);
            }
            else
            {
                sql = "insert into ProjectException(ProjectKey,Exception,ExceptionType,databackuptm) values('<ProjectKey>','<Except>','<ExceptType>','<databackuptm>')";
                sql = sql.Replace("<ProjectKey>", ProjectKey).Replace("<ExceptType>", ExceptType).Replace("<Except>", Except).Replace("<databackuptm>", DateTime.Now.ToString());
                DBUtility.ExeLocalSqlNoRes(sql);
            }
        }

        public static ProjectExcept RetrieveProjectExcept(string ProjectKey, string ExceptType)
        {
            var ret = new ProjectExcept();
            var sql = "select ProjectKey,Exception,ExceptionType from ProjectException where ExceptionType='<ExceptType>'";
            sql = sql.Replace("<ExceptType>", ExceptType);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                ret.ProjectKey = Convert.ToString(line[0]);
                ret.Except = Convert.ToString(line[1]);
                ret.ExceptType = Convert.ToString(line[2]);
            }
            return ret;
        }

        public static List<ProjectExcept> RetrieveProjectAllExcept(string ExceptType)
        {
            var ret = new List<ProjectExcept>();
            var sql = "select ProjectKey,Exception,ExceptionType from ProjectException where ExceptionType='<ExceptType>'";
            sql = sql.Replace("<ExceptType>", ExceptType);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var tempret = new ProjectExcept();
                tempret.ProjectKey = Convert.ToString(line[0]);
                tempret.Except = Convert.ToString(line[1]);
                tempret.ExceptType = Convert.ToString(line[2]);
                ret.Add(tempret);
            }
            return ret;
        }


        public static void StoreProjectProcessBonding(string PJKey, string BondingProcess)
        {

            var sql = "delete from ProjectException where ProjectKey='<ProjectKey>' and ExceptionType='<ExceptType>'";
            sql = sql.Replace("<ProjectKey>", PJKey).Replace("<ExceptType>", ProjectExceptType.PROCESSEXCEPT);
            DBUtility.ExeLocalSqlNoRes(sql);

            if (!string.IsNullOrEmpty(BondingProcess) && !BondingProcess.ToUpper().Contains("PLEASE"))
            {
                sql = "insert into ProjectException(ProjectKey,Exception,ExceptionType,databackuptm) values('<ProjectKey>','<Except>','<ExceptType>','<databackuptm>')";
                sql = sql.Replace("<ProjectKey>", PJKey).Replace("<ExceptType>", ProjectExceptType.PROCESSEXCEPT).Replace("<Except>", BondingProcess).Replace("<databackuptm>", DateTime.Now.ToString());
                DBUtility.ExeLocalSqlNoRes(sql);
            }
        }

        public static List<ProjectExcept> RetriveProjectProcessBonding(string PJKey)
        {
            var ret = new List<ProjectExcept>();
            var sql = "select ProjectKey,Exception,ExceptionType from ProjectException where ProjectKey='<ProjectKey>' and ExceptionType='<ExceptType>'";
            sql = sql.Replace("<ProjectKey>", PJKey).Replace("<ExceptType>", ProjectExceptType.PROCESSEXCEPT);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var temp = new ProjectExcept();
                temp.ProjectKey = Convert.ToString(line[0]);
                temp.Except = Convert.ToString(line[1]);
                temp.ExceptType = Convert.ToString(line[2]);
                ret.Add(temp);
            }
            return ret;
        }

        public static void ProjectTransfer(string pKey, string trans_to)
        {
            var sql = @"update Project set ApVal4 = @trans_to where ProjectKey = @pKey";
            var param = new Dictionary<string, string>();
            param.Add("@trans_to", trans_to);
            param.Add("@pKey", pKey);

            DBUtility.ExeLocalSqlNoRes(sql, param);
        }
        public static void UpdateProjectMemberbyRole(string pKey, string role, List<string> memberlist)
        {
            var sql = @"delete from ProjectMembers 
                where ProjectKey = @pKey and Role = @Role";
            var param = new Dictionary<string, string>();
            param.Add("@pKey", pKey);
            param.Add("@Role", role);
            
            DBUtility.ExeLocalSqlNoRes(sql, param);
            foreach(var item in memberlist)
            {
                sql = @"insert into ProjectMembers(ProjectKey,Name,Role,databackuptm) values
                    ('<ProjectKey>','<Name>','<Role>','<databackuptm>')";
                sql = sql.Replace("<ProjectKey>", pKey).Replace("<Name>", item.ToUpper()).Replace("<Role>", role)
                    .Replace("<databackuptm>", DateTime.Now.ToString());
                DBUtility.ExeLocalSqlNoRes(sql);

                UserViewModels.UpdateUserProject(item.ToUpper(), pKey);
            }
        }

        public static void UpdateProjectStatus(string pKey, string status)
        {
            var sql = @"update Project set APVal5 = @Status where ProjectKey = @pKey ";
            var param = new Dictionary<string, string>();
            param.Add("@pKey", pKey);
            param.Add("@Status", status);

            DBUtility.ExeLocalSqlNoRes(sql, param);
        }

        public static Dictionary<string, string> PN2PJKey()
        {
            var ret = new Dictionary<string, string>();
            var sql = "select PN,ProjectKey from ProjectPn";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var pn = Convert.ToString(line[0]);
                var pjkey = Convert.ToString(line[1]);
                if (!ret.ContainsKey(pn))
                { ret.Add(pn, pjkey); }
            }
            return ret;
        }

        public static List<Dictionary<string, string>> GetClosedProjects()
        {
            var sql = @"select p.ProjectKey, tmp_a.UserName, tmp_a.Date from Project as p left join
                    (select * from Log where ID in 
	                    (select max(ID) from Log where LogType = @logtype group by ProjectKey)
                    ) as tmp_a on p.ProjectKey = tmp_a.ProjectKey 
                    where p.APVal5 = @status order by tmp_a.Date Desc";
            var param = new Dictionary<string, string>();
            param.Add("@logtype", LogType.OpenOrCloseProject.ToString());
            param.Add("@status", ProjectStatus.Close);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, param);
            var res = new List<Dictionary<string, string>>();
            if(dbret.Count > 0)
            {
                foreach(var item in dbret)
                {
                    var tmp = new Dictionary<string, string>();
                    tmp.Add("ProjectKey", Convert.ToString(item[0]));
                    tmp.Add("UserName", Convert.ToString(item[1]));
                    tmp.Add("Date", Convert.ToString(item[2]));
                    res.Add(tmp);
                }
            }

            return res;
        }
        
    }
}
