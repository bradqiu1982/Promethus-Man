using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Prometheus.Models
{
    public class CDPUtility
    {
        private static string RMSpectialCh(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                {
                    sb.Append(c);
                }
            }
            return sb.ToString().ToUpper();
        }

        private static string PNCondition(List<ProjectPn> pns)
        {

            var cond = "";
            foreach (var pn in pns)
            {
                if (string.IsNullOrEmpty(cond))
                {
                    cond = " modulepartnum like '%" + pn.Pn.Trim() + "%' ";
                }
                else
                {
                    cond = cond + " or modulepartnum like '%" + pn.Pn.Trim() + "%' ";
                }
            }
            return cond;
        }

        private static Dictionary<string,string> RetrieveSQLs(ProjectViewModels vm, Controller ctrl)
        {
            var ret = new Dictionary<string, string>();

            var tabledict = new Dictionary<string, string>();
            foreach (var cdp in vm.CDPTabList)
            {
                if (!tabledict.ContainsKey(cdp.Station))
                { tabledict.Add(cdp.Station, cdp.TableName); }
                
            }

            foreach (var station in vm.StationList)
            {
                if (tabledict.ContainsKey(station.Station))
                {
                    var table = tabledict[station.Station].ToUpper();
                    if (table.Contains("DC_"))
                    {
                        var tabidx = table.IndexOf("DC_");
                        var tabname = table.Substring(tabidx);

                        var starttime = ProjectTestData.RetrieveLatestTimeOfLocalProjectWithTestName(vm.ProjectKey, station.Station);
                        var endtime = "";
                        if (string.IsNullOrEmpty(starttime))
                        {
                            starttime = vm.StartDate.ToString("yyyy-MM-dd HH:mm:ss");
                            endtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        else
                        {
                            endtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        }

                        var pncond = PNCondition(vm.PNList);
                        var sql = "select distinct " + tabname + "HistoryId,ModuleSerialNum, WhichTest, ModuleType, ErrAbbr, TestTimeStamp, TestStation, assemblypartnum,'JO' as JO from " + table
                            + "  where (" + pncond + ") and TestTimeStamp > '" + starttime + "' and TestTimeStamp < '" + endtime+ "'  order by moduleserialnum,testtimestamp DESC";
                        ret.Add(station.Station , sql);
                    }
                }
            }

            return ret;
        }

        public static void UpdateProjectData(ProjectViewModels vm, Controller ctrl)
        {
            if (vm.StationList.Count > 0
                && vm.CDPTabList.Count > 0
                && vm.PNList.Count > 0)
            {
                var existdataid = ProjectTestData.RetrieveAllDataID12M(vm.ProjectKey);

                var dataidlist = new List<string>();

                var sqls = RetrieveSQLs(vm, ctrl);
                foreach (var s in sqls)
                {
                    var dbret = DBUtility.ExeCDPSqlWithRes(s.Value);
                    foreach (var item in dbret)
                    {
                        try
                        {
                            if (item[1] == null)
                            { continue; }
                            var sn = Convert.ToString(item[1]).Trim();
                            if (string.IsNullOrEmpty(sn))
                            { continue; }

                            var did = Convert.ToString(item[0]);

                            if (!existdataid.ContainsKey(did))
                            {
                                existdataid.Add(did, true);

                                var tempdata = new ProjectTestData(vm.ProjectKey, Convert.ToString(item[0]), Convert.ToString(item[1])
                                        , s.Key, Convert.ToString(item[3]), Convert.ToString(item[4])
                                        , Convert.ToString(item[5]), Convert.ToString(item[6]), Convert.ToString(item[7]));
                                tempdata.JO = Convert.ToString(item[8]);

                                tempdata.StoreProjectTestData();
                                dataidlist.Add(tempdata.DataID);
                            }
                        }
                        catch (Exception ex) { }
                    }//end foreach
                 }//end foreach

                if (dataidlist.Count > 0)
                {
                    var rawdata = ProjectTestData.RetrieveProjectTestDataByDataIDList(dataidlist);
                    foreach (var tempdata in rawdata)
                    {
                        if (string.Compare(tempdata.ErrAbbr, "PASS", true) != 0)
                        {
                            var ekey = ProjectErrorViewModels.GetUniqKey();
                            var pjerror = new ProjectErrorViewModels(vm.ProjectKey, ekey, tempdata.ErrAbbr, "", 1);
                            pjerror.Reporter = "System";
                            pjerror.Description = "";
                            pjerror.AddandUpdateProjectError();
                        }
                    }//end foreach
                }
            }//end if

        }


    }
}