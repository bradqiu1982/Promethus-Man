using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Prometheus.Models
{
    public class MESUtility
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
            return sb.ToString();
        }

        private static string PNCondition(List<ProjectPn> pns)
        {
            string ret = "('";
            foreach (var pn in pns)
            {
                ret = ret + RMSpectialCh(pn.Pn) + "','";
            }

            if (pns.Count > 0)
            {
                ret = ret.Substring(0, ret.Length - 2) + ")";
            }
            else
            {
                ret = "('')";
            }

            return ret;
        }


        private static Dictionary<string, string> RetrieveSqlFromProjectModel(ProjectViewModels projectmodel)
        {
            var tables = new Dictionary<string,string>();

            foreach (var s in projectmodel.StationList)
            {
                foreach (var m in projectmodel.TabList)
                {
                    if (string.Compare(s.Station.ToUpper(), m.Station.ToUpper()) == 0)
                    {
                        tables.Add(s.Station,m.TableName);
                        break;
                    }
                }
            }

            string pncond = PNCondition(projectmodel.PNList);

            var sql = "select dc_<DCTABLE>HistoryId,ModuleSerialNum, WhichTest, ModuleType, ErrAbbr, TestTimeStamp, TestStation,assemblypartnum from  insite.dc_<DCTABLE>  where assemblypartnum in  <PNCOND>   <TIMECOND>  order by  moduleserialnum,testtimestamp DESC";

            var ret = new Dictionary<string, string>();
            foreach (var tb in tables)
            {
                ret.Add(tb.Key,sql.Replace("<DCTABLE>",tb.Value).Replace("<PNCOND>", pncond));
            }
            return ret;
        }

        private static void CreateSystemIssues(List<ProjectTestData> failurelist)
        {
            if (failurelist.Count > 0)
            {
                var pj = ProjectViewModels.RetrieveOneProject(failurelist[0].ProjectKey);
                var firstengineer = "";
                foreach (var m in pj.MemberList)
                {
                    if (string.Compare(m.Role,ProjectViewModels.ENGROLE) == 0)
                    {
                        firstengineer = m.Name;
                        break;
                    }
                }

                foreach (var item in failurelist)
                {
                    var vm = new IssueViewModels();
                    vm.ProjectKey = item.ProjectKey;
                    vm.IssueKey = item.DataID;
                    vm.IssueType = ISSUETP.Bug;
                    vm.Summary = "Module " + item.ModuleSerialNum + " failed for "+item.ErrAbbr + " @ "+item.WhichTest;
                    vm.Priority = ISSUEPR.Major;
                    vm.DueDate = DateTime.Now.AddDays(7);
                    vm.ReportDate = item.TestTimeStamp;
                    vm.Assignee = firstengineer;
                    vm.Reporter = "System";
                    vm.Resolution = Resolute.Pending;
                    vm.ResolvedDate = DateTime.Parse("1982-05-06 01:01:01");
                    vm.Description = "";
                    ProjectEvent.CreateIssueEvent(vm.ProjectKey, "System", vm.Assignee, vm.Summary, vm.IssueKey);
                    vm.StoreIssue();
                }
            }
        }

        public static void StartProjectBonding(ProjectViewModels vm)
        {
            
                if (vm.StationList.Count > 0
                    && vm.TabList.Count > 0
                    && vm.PNList.Count > 0)
                {

                    var failurelist = new List<ProjectTestData>();
                    

                    bool bondinged = false;
                    if (ProjectTestData.RetrieveLatestTimeOfLocalProject(vm.ProjectKey) != null)
                    {
                        bondinged = true;
                    }

                    var bondingeddatadict = new Dictionary<string, bool>();
                    if (bondinged)
                    {
                        bondingeddatadict = ProjectTestData.RetrieveAllDataID(vm.ProjectKey);
                    }
                    
                    var sqls = RetrieveSqlFromProjectModel(vm);
                    foreach (var s in sqls)
                    {
                        var sndict = new Dictionary<string, bool>();
                        var sql = s.Value.Replace("<TIMECOND>", "and TestTimeStamp > '" + vm.StartDate.ToString() + "'");
                        var dbret = DBUtility.ExeMESSqlWithRes(sql);
                        foreach (var item in dbret)
                        {
                            try
                            {

                                var tempdata = new ProjectTestData(vm.ProjectKey, Convert.ToString(item[0]), Convert.ToString(item[1])
                                        ,s.Key, Convert.ToString(item[3]), Convert.ToString(item[4])
                                        , Convert.ToString(item[5]), Convert.ToString(item[6]), Convert.ToString(item[7]));


                                if (!bondinged)
                                {
                                    tempdata.StoreProjectTestData();

                                    if (!sndict.ContainsKey(tempdata.ModuleSerialNum))
                                    {
                                        sndict.Add(tempdata.ModuleSerialNum, true);
                                        if (string.Compare(tempdata.ErrAbbr, "PASS", true) != 0)
                                        {
                                            failurelist.Add(tempdata);
                                        }
                                    }
                                }
                                else
                                {
                                    if (!bondingeddatadict.ContainsKey(tempdata.DataID))
                                    {
                                        tempdata.StoreProjectTestData();
                                        if (!sndict.ContainsKey(tempdata.ModuleSerialNum))
                                        {
                                            sndict.Add(tempdata.ModuleSerialNum, true);
                                            if (string.Compare(tempdata.ErrAbbr, "PASS", true) != 0)
                                            {
                                                failurelist.Add(tempdata);
                                            }
                                        }
                                }
                                }
                            }
                            catch (Exception ex)
                            { }
                        }
                    }

                    CreateSystemIssues(failurelist);
                }
            
        }


        public static void UpdateProjectData(ProjectViewModels vm,string starttime,string endtime)
        {

            if (vm.StationList.Count > 0
                && vm.TabList.Count > 0
                && vm.PNList.Count > 0)
            {

                var failurelist = new List<ProjectTestData>();

                bool bondinged = false;
                if (ProjectTestData.RetrieveLatestTimeOfLocalProject(vm.ProjectKey) != null)
                {
                    bondinged = true;
                }

                var bondingeddatadict = new Dictionary<string, bool>();
                if (bondinged)
                {
                    bondingeddatadict = ProjectTestData.RetrieveAllDataID(vm.ProjectKey);
                }

                var sqls = RetrieveSqlFromProjectModel(vm);
                foreach (var s in sqls)
                {
                    var sndict = new Dictionary<string, bool>();
                    var sql = s.Value.Replace("<TIMECOND>", "and TestTimeStamp > '" + starttime + "' and TestTimeStamp < '"+ endtime + "'");
                    var dbret = DBUtility.ExeMESSqlWithRes(sql);
                    foreach (var item in dbret)
                    {
                        try
                        {

                            var tempdata = new ProjectTestData(vm.ProjectKey, Convert.ToString(item[0]), Convert.ToString(item[1])
                                    ,s.Key, Convert.ToString(item[3]), Convert.ToString(item[4])
                                    , Convert.ToString(item[5]), Convert.ToString(item[6]), Convert.ToString(item[7]));


                            if (!bondinged)
                            {
                                tempdata.StoreProjectTestData();

                                if (!sndict.ContainsKey(tempdata.ModuleSerialNum))
                                {
                                    sndict.Add(tempdata.ModuleSerialNum, true);
                                    if (string.Compare(tempdata.ErrAbbr, "PASS", true) != 0)
                                    {
                                        failurelist.Add(tempdata);
                                    }
                                }
                            }
                            else
                            {
                                if (!bondingeddatadict.ContainsKey(tempdata.DataID))
                                {
                                    tempdata.StoreProjectTestData();
                                    if (!sndict.ContainsKey(tempdata.ModuleSerialNum))
                                    {
                                        sndict.Add(tempdata.ModuleSerialNum, true);
                                        if (string.Compare(tempdata.ErrAbbr, "PASS", true) != 0)
                                        {
                                            failurelist.Add(tempdata);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        { }
                    }
                }

                CreateSystemIssues(failurelist);
            }

        }





    }
}