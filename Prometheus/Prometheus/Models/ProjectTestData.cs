using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
    public class ProjectTestData
    {
        public ProjectTestData()
        { }

        public ProjectTestData(string pk, string did, string sn, string wtest, string mt, string err, string testtime, string station, string p)
        {
            ProjectKey = pk;
            DataID = did;
            ModuleSerialNum = sn;
            WhichTest = wtest;
            ModuleType = mt;
            ErrAbbr = err;
            TestTimeStamp = DateTime.Parse(testtime);
            TestStation = station;
            PN = p;
        }

        public ProjectTestData(string pk, string sn, string wtest, string err, string testtime)
        {
            ProjectKey = pk;
            ModuleSerialNum = sn;
            WhichTest = wtest;
            ErrAbbr = err;
            TestTimeStamp = DateTime.Parse(testtime);
        }

        public string ProjectKey { set; get; }
        public string DataID { set; get; }
        public string ModuleSerialNum { set; get; }
        public string WhichTest { set; get; }
        public string ModuleType { set; get; }
        public string ErrAbbr { set; get; }
        public DateTime TestTimeStamp { set; get; }
        public string TestStation { set; get; }
        public string PN { set; get; }

        public static ConcurrentDictionary<string, bool> UpdateLockUsing = new ConcurrentDictionary<string, bool>();
        public static bool UpdatePJLockUsing(string pjkey)
        {
            if (UpdateLockUsing.ContainsKey(pjkey))
            {
                if (UpdateLockUsing[pjkey])
                {
                    return true;
                }
                else
                {
                    UpdateLockUsing[pjkey] = true;
                    return false;
                }
            }
            else
            {
                try
                {
                    var ret = UpdateLockUsing.TryAdd(pjkey, true);
                    if (ret)
                    {
                        return false;
                    }
                    else
                    {
                        if (UpdateLockUsing[pjkey])
                        {
                            return true;
                        }
                        else
                        {
                            UpdateLockUsing[pjkey] = true;
                            return false;
                        }
                    }
                }
                catch
                {
                    return true;
                }
            }
        }

        public static void ResetUpdatePJLock(string pjkey)
        {
            if (UpdateLockUsing.ContainsKey(pjkey))
            {
                UpdateLockUsing[pjkey] = false;
            }
        }


        public void StoreProjectTestData()
        {
            var isql = "insert into ProjectTestData(ProjectKey,DataID,ModuleSerialNum,WhichTest,ModuleType,ErrAbbr,TestTimeStamp,TestStation,PN,UpdateTime) values('<ProjectKey>','<DataID>','<ModuleSerialNum>','<WhichTest>','<ModuleType>','<ErrAbbr>','<TestTimeStamp>','<TestStation>','<PN>','<UpdateTime>')";
            isql = isql.Replace("<ProjectKey>", ProjectKey).Replace("<DataID>", DataID).Replace("<ModuleSerialNum>", ModuleSerialNum)
                .Replace("<WhichTest>", WhichTest).Replace("<ModuleType>", ModuleType).Replace("<ErrAbbr>", ErrAbbr)
                .Replace("<TestTimeStamp>", TestTimeStamp.ToString()).Replace("<TestStation>", TestStation).Replace("<PN>", PN).Replace("<UpdateTime>", DateTime.Now.ToString());
            DBUtility.ExeLocalSqlNoRes(isql);
        }

        public static void PrePareMESLatestData(string projectkey)
        {
            if (UpdatePJLockUsing(projectkey))
                return;

            try
            {
                var vm = ProjectViewModels.RetrieveOneProject(projectkey);
                if (vm.TabList.Count > 0
                    && vm.PNList.Count > 0
                    && vm.StationList.Count > 0)
                {
                    string lastupdatetime = ProjectTestData.RetrieveLatestTimeOfLocalProject(projectkey);
                    if (!string.IsNullOrEmpty(lastupdatetime))
                    {
                        //var vlast = DateTime.Parse(lastupdatetime);
                        //vlast = vlast.AddMinutes(18);
                        //if (vlast < DateTime.Now)
                        //{
                            MESUtility.UpdateProjectData(vm, lastupdatetime, DateTime.Now.ToString());
                        //}
                    }
                    else
                    {
                        MESUtility.UpdateProjectData(vm, vm.StartDate.ToString(), DateTime.Now.ToString());
                    }
                }

                ResetUpdatePJLock(projectkey);
            }
            catch (Exception ex)
            {
                ResetUpdatePJLock(projectkey);
            }
        }

        public static void PrePareATELatestData(string projectkey)
        {
            if (UpdatePJLockUsing(projectkey))
                return;

            try
            {
                var vm = ProjectViewModels.RetrieveOneProject(projectkey);
                if (vm.SumDatasetList.Count > 0
                    && vm.MDIDList.Count > 0)
                {
                    string lastupdatetime = ProjectTestData.RetrieveLatestTimeOfLocalProject(projectkey);
                    if (!string.IsNullOrEmpty(lastupdatetime))
                    {
                        //var vlast = DateTime.Parse(lastupdatetime);
                        //vlast = vlast.AddMinutes(18);
                        //if (vlast < DateTime.Now)
                        //{
                            ATEUtility.UpdateProjectData(vm, lastupdatetime);
                        //}
                    }
                    else
                    {
                        ATEUtility.UpdateProjectData(vm, vm.StartDate.ToString());
                    }
                }

                ResetUpdatePJLock(projectkey);
            }
            catch (Exception ex)
            {
                ResetUpdatePJLock(projectkey);
            }
        }

        public static string RetrieveLatestTimeOfLocalProject(string projectkey)
        {
            var sql = "select top 1 TestTimeStamp from ProjectTestData where ProjectKey = '<ProjectKey>' order by TestTimeStamp DESC";
            sql = sql.Replace("<ProjectKey>", projectkey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            if (dbret.Count > 0)
            {
                return Convert.ToString(dbret[0][0]);
            }
            else
                return null;
        }

        public static List<ProjectTestData> RetrieveProjectTestData(int topnum,string projectkey)
        {
            //ProjectTestData.PrePareLatestData(projectkey);

            var ret = new List<ProjectTestData>();
            var sql = "select top <topnum> ProjectKey,DataID,ModuleSerialNum,WhichTest,ModuleType,ErrAbbr,TestTimeStamp,TestStation,PN from ProjectTestData where ProjectKey = '<ProjectKey>' order by TestTimeStamp DESC";
            sql = sql.Replace("<ProjectKey>", projectkey).Replace("<topnum>", Convert.ToString(topnum));
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var item in dbret)
            {
                var tempdata = new ProjectTestData(Convert.ToString(item[0]), Convert.ToString(item[1]), Convert.ToString(item[2])
                    , Convert.ToString(item[3]), Convert.ToString(item[4]), Convert.ToString(item[5])
                    , Convert.ToString(item[6]), Convert.ToString(item[7]),Convert.ToString(item[8]));
                ret.Add(tempdata);
            }
            return ret;
        }

        public static List<ProjectTestData> RetrieveProjectTestData(string projectkey,string startdate,string enddate,bool firstyield)
        {
            //ProjectTestData.PrePareLatestData(projectkey);

            var ret = new List<ProjectTestData>();
            var sql = "";
            if (firstyield)
            {
                sql = "select ModuleSerialNum,WhichTest,ErrAbbr,TestTimeStamp from ProjectTestData where ProjectKey = '<ProjectKey>' and TestTimeStamp > '<StartDate>' and TestTimeStamp < '<EndDate>' order by ModuleSerialNum,TestTimeStamp ASC";
            }
            else
            {
                sql = "select ModuleSerialNum,WhichTest,ErrAbbr,TestTimeStamp from ProjectTestData where ProjectKey = '<ProjectKey>' and TestTimeStamp > '<StartDate>' and TestTimeStamp < '<EndDate>' order by ModuleSerialNum,TestTimeStamp DESC";
            }

            sql = sql.Replace("<ProjectKey>", projectkey).Replace("<StartDate>", startdate).Replace("<EndDate>", enddate);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var item in dbret)
            {
                //public ProjectTestData(string pk, string sn, string wtest, string err, string testtime)
                var tempdata = new ProjectTestData(projectkey,Convert.ToString(item[0]), Convert.ToString(item[1]), Convert.ToString(item[2])
                    , Convert.ToString(item[3]));
                ret.Add(tempdata);
            }
            return ret;
        }

        public static List<ProjectTestData> RetrieveProjectFailedTestData(int topnum, string projectkey)
        {
            //ProjectTestData.PrePareLatestData(projectkey);

            var ret = new List<ProjectTestData>();
            var sql = "select top <topnum> ProjectKey,DataID,ModuleSerialNum,WhichTest,ModuleType,ErrAbbr,TestTimeStamp,TestStation,PN from ProjectTestData where ProjectKey = '<ProjectKey>' and ErrAbbr <> 'PASS' order by ErrAbbr,TestTimeStamp DESC";
            sql = sql.Replace("<ProjectKey>", projectkey).Replace("<topnum>", Convert.ToString(topnum));
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var item in dbret)
            {
                var tempdata = new ProjectTestData(Convert.ToString(item[0]), Convert.ToString(item[1]), Convert.ToString(item[2])
                    , Convert.ToString(item[3]), Convert.ToString(item[4]), Convert.ToString(item[5])
                    , Convert.ToString(item[6]), Convert.ToString(item[7]), Convert.ToString(item[8]));
                ret.Add(tempdata);
            }
            return ret;
        }

        public static List<ProjectTestData> RetrieveProjectTestData(string DataID)
        {
            var ret = new List<ProjectTestData>();
            var sql = "select  ProjectKey,DataID,ModuleSerialNum,WhichTest,ModuleType,ErrAbbr,TestTimeStamp,TestStation,PN from ProjectTestData where DataID = '<DataID>'";
            sql = sql.Replace("<DataID>", DataID);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var item in dbret)
            {
                var tempdata = new ProjectTestData(Convert.ToString(item[0]), Convert.ToString(item[1]), Convert.ToString(item[2])
                    , Convert.ToString(item[3]), Convert.ToString(item[4]), Convert.ToString(item[5])
                    , Convert.ToString(item[6]), Convert.ToString(item[7]), Convert.ToString(item[8]));
                ret.Add(tempdata);
            }
            return ret;
        }

        public static List<string> RetrieveProjectFailedDataID(string projectkey)
        {
            var ret = new List<string>();
            var sql = "select DataID from ProjectTestData where ProjectKey = '<ProjectKey>' and ErrAbbr <> 'PASS' order by ErrAbbr,TestTimeStamp DESC";
            sql = sql.Replace("<ProjectKey>", projectkey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var item in dbret)
            {
                ret.Add(Convert.ToString(item[0]));
            }
            return ret;
        }

        public static List<ProjectTestData> RetrieveProjectTestDataWithErrAbbr(int topnum, string ProjectKey, string ErrAbbr)
        {
            //ProjectTestData.PrePareLatestData(ProjectKey);

            var ret = new List<ProjectTestData>();
            var sql = "select top <topnum> ProjectKey,DataID,ModuleSerialNum,WhichTest,ModuleType,ErrAbbr,TestTimeStamp,TestStation,PN from ProjectTestData where ProjectKey = '<ProjectKey>' and ErrAbbr = '<ErrAbbr>' order by TestTimeStamp DESC";
            sql = sql.Replace("<ProjectKey>", ProjectKey).Replace("<topnum>", Convert.ToString(topnum)).Replace("<ErrAbbr>", ErrAbbr);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var item in dbret)
            {
                var tempdata = new ProjectTestData(Convert.ToString(item[0]), Convert.ToString(item[1]), Convert.ToString(item[2])
                    , Convert.ToString(item[3]), Convert.ToString(item[4]), Convert.ToString(item[5])
                    , Convert.ToString(item[6]), Convert.ToString(item[7]), Convert.ToString(item[8]));
                ret.Add(tempdata);
            }
            return ret;
        }

        public static List<ProjectTestData> RetrieveProjectTestDataWithSN(int topnum, string ProjectKey, string SN)
        {
            //ProjectTestData.PrePareLatestData(ProjectKey);

            var ret = new List<ProjectTestData>();
            var sql = "select top <topnum> ProjectKey,DataID,ModuleSerialNum,WhichTest,ModuleType,ErrAbbr,TestTimeStamp,TestStation,PN from ProjectTestData where ProjectKey = '<ProjectKey>' and ModuleSerialNum = '<ModuleSerialNum>' order by TestTimeStamp DESC";
            sql = sql.Replace("<ProjectKey>", ProjectKey).Replace("<topnum>", Convert.ToString(topnum)).Replace("<ModuleSerialNum>", SN);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var item in dbret)
            {
                var tempdata = new ProjectTestData(Convert.ToString(item[0]), Convert.ToString(item[1]), Convert.ToString(item[2])
                    , Convert.ToString(item[3]), Convert.ToString(item[4]), Convert.ToString(item[5])
                    , Convert.ToString(item[6]), Convert.ToString(item[7]), Convert.ToString(item[8]));
                ret.Add(tempdata);
            }
            return ret;
        }

        public static Dictionary<string,bool> RetrieveAllDataID(string projectkey)
        {
            var ret = new Dictionary<string, bool>();
            var sql = "select DataID from ProjectTestData where ProjectKey = '<ProjectKey>'";
            sql = sql.Replace("<ProjectKey>", projectkey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var item in dbret)
            {
                if (!ret.ContainsKey(Convert.ToString(item[0])))
                {
                    ret.Add(Convert.ToString(item[0]), true);
                }
            }
            return ret;
        }

        public static Dictionary<string, bool> RetrieveSNBeforeDateWithStation(string projectkey, string edate)
        {
            var ret = new Dictionary<string, bool>();
            var sql = "select ModuleSerialNum,WhichTest from ProjectTestData where ProjectKey = '<ProjectKey>' and TestTimeStamp < '<ENDDATE>'";
            sql = sql.Replace("<ProjectKey>", projectkey).Replace("<ENDDATE>", edate);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var item in dbret)
            {
                var key = Convert.ToString(item[0]);
                var key2 = Convert.ToString(item[1]);

                if (!ret.ContainsKey(key+"-"+ key2))
                {
                    ret.Add(key + "-" + key2, true);
                }
            }
            return ret;
        }

        public static Dictionary<string, bool> RetrieveSNBeforeDate(string projectkey, string edate)
        {
            var ret = new Dictionary<string, bool>();
            var sql = "select ModuleSerialNum from ProjectTestData where ProjectKey = '<ProjectKey>' and TestTimeStamp < '<ENDDATE>'";
            sql = sql.Replace("<ProjectKey>", projectkey).Replace("<ENDDATE>", edate);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var item in dbret)
            {
                var key = Convert.ToString(item[0]);
                
                if (!ret.ContainsKey(key))
                {
                    ret.Add(key, true);
                }
            }
            return ret;
        }
    }
}