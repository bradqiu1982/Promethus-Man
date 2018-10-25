using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
    public class MachineVM
    {
        public static List<string> RetrieveWhichTest(string ProjectKey)
        {
            var ret = new List<string>();
            var sql = "select distinct WhichTest from ProjectTestData where ProjectKey = '<ProjectKey>' order by WhichTest asc";
            sql = sql.Replace("<ProjectKey>", ProjectKey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString(line[0]));
            }
            return ret;
        }

        public static List<ProjectTestData> RetrieveWhichTestData(string ProjectKey, string WhichTest,string starttime,string endtime)
        {
            var ret = new List<ProjectTestData>();
            var sql = "select ProjectKey,DataID,ModuleSerialNum,WhichTest,ModuleType,ErrAbbr,TestTimeStamp,TestStation,PN from ProjectTestData where ProjectKey = '<ProjectKey>' and WhichTest = '<WhichTest>' and  TestTimeStamp >= '<starttime>' and TestTimeStamp <= '<endtime>' order by TestTimeStamp DESC";
            sql = sql.Replace("<ProjectKey>", ProjectKey).Replace("<WhichTest>", WhichTest).Replace("<starttime>", starttime).Replace("<endtime>", endtime);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var item in dbret)
            {
                var tempdata = new ProjectTestData(Convert.ToString(item[0]), Convert.ToString(item[1]), Convert.ToString(item[2])
                    , Convert.ToString(item[3]), Convert.ToString(item[4]), Convert.ToString(item[5])
                    , Convert.ToString(item[6]), Convert.ToString(item[7]), Convert.ToString(item[8]));
                ret.Add(tempdata);
            }
            return ret;
        }

        public static Dictionary<string, ProjectYieldViewModule> RetrieveWhichTestYieldByStation(List<ProjectTestData> testdata,int dayspan)
        {
            var ret = new Dictionary<string, ProjectYieldViewModule>();

            var stationtestdata = new Dictionary<string, List<ProjectTestData>>();
            foreach (var item in testdata)
            {
                if (stationtestdata.ContainsKey(item.TestStation))
                {
                    stationtestdata[item.TestStation].Add(item);
                }
                else
                {
                    var templist = new List<ProjectTestData>();
                    templist.Add(item);
                    stationtestdata.Add(item.TestStation, templist);
                }
            }

            var leastdatalimit = 0;
            if (dayspan >= 8)
            {
                leastdatalimit = 30;
            }
            else
            {
                leastdatalimit = 10;
            }

            var removekeylist = new List<string>();
            foreach (var item in stationtestdata) {
                if (item.Value.Count < leastdatalimit)
                {
                    removekeylist.Add(item.Key);
                }
            }

            foreach (var key in removekeylist)
            {
                stationtestdata.Remove(key);
            }

            foreach (var kv in stationtestdata)
            {
                ret.Add(kv.Key,ProjectYieldViewModule.GetYieldByTestData(kv.Value));
            }

            return ret;
        }

    }
}