using System;
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

        public string ProjectKey { set; get; }
        public string DataID { set; get; }
        public string ModuleSerialNum { set; get; }
        public string WhichTest { set; get; }
        public string ModuleType { set; get; }
        public string ErrAbbr { set; get; }
        public DateTime TestTimeStamp { set; get; }
        public string TestStation { set; get; }
        public string PN { set; get; }

        public void StoreProjectTestData()
        {
            var isql = "insert into ProjectTestData(ProjectKey,DataID,ModuleSerialNum,WhichTest,ModuleType,ErrAbbr,TestTimeStamp,TestStation,PN) values('<ProjectKey>','<DataID>','<ModuleSerialNum>','<WhichTest>','<ModuleType>','<ErrAbbr>','<TestTimeStamp>','<TestStation>','<PN>')";
            isql = isql.Replace("<ProjectKey>", ProjectKey).Replace("<DataID>", DataID).Replace("<ModuleSerialNum>", ModuleSerialNum)
                .Replace("<WhichTest>", WhichTest).Replace("<ModuleType>", ModuleType).Replace("<ErrAbbr>", ErrAbbr)
                .Replace("<TestTimeStamp>", TestTimeStamp.ToString()).Replace("<TestStation>", TestStation).Replace("<PN>", PN);
            DBUtility.ExeLocalSqlNoRes(isql);
        }

        public static List<ProjectTestData> RetrieveProjectTestData(int topnum,string projectkey)
        {
            var ret = new List<ProjectTestData>();
            var sql = "select top <topnum> ProjectKey,DataID,ModuleSerialNum,WhichTest,ModuleType,ErrAbbr,TestTimeStamp,TestStation,PN from ProjectTestData where ProjectKey = '<ProjectKey>' order by ErrAbbr,TestTimeStamp DESC";
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
        public static List<ProjectTestData> RetrieveProjectFailedTestData(int topnum, string projectkey)
        {
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
        public static List<ProjectTestData> RetrieveProjectTestDataWithErrAbbr(int topnum, string ProjectKey, string ErrAbbr)
        {
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

        public static Dictionary<string,bool> RetrieveAllDataID(string projectkey)
        {
            var ret = new Dictionary<string, bool>();
            var sql = "select DataID from ProjectTestData where ProjectKey = '<ProjectKey>'";
            sql = sql.Replace("<ProjectKey>", projectkey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var item in dbret)
            {
                ret.Add(Convert.ToString(item[0]), true);
            }
            return ret;
        }
    }
}