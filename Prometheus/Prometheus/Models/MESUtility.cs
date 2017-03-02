using Prometheus.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Prometheus.Models
{
    public class MESUtility
    {
        private static bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

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
            var cond = "";
            foreach (var pn in pns)
            {
                if (!IsDigitsOnly(pn.Pn.Trim()))
                {
                    if (string.IsNullOrEmpty(cond))
                    {
                        cond = " c.Description like '%" + pn.Pn.Trim() + "%' ";
                    }
                    else
                    {
                        cond = cond + " or c.Description like '%" + pn.Pn.Trim() + "%' ";
                    }
                }
            }

            if (string.IsNullOrEmpty(cond))
            {
                return string.Empty;
            }
            else
            {
                var ret = "select DISTINCT p.ProductName from insite.Product c (nolock) left join insite.ProductBase p on c.ProductBaseId = p.ProductBaseId where " + cond;
                return ret;
            }
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
                        if (!tables.ContainsKey(s.Station)) { 
                        tables.Add(s.Station,m.TableName);
                        }
                        break;
                    }
                }
            }

            string pncond = PNCondition(projectmodel.PNList);

            var joinstr = " LEFT JOIN Insite.Container b WITH (NOLOCK) ON b.containername = a.ModuleSerialNum LEFT JOIN Insite.MfgOrder d WITH(NOLOCK) ON d.MfgOrderId = b.MfgOrderId ";
            var sql = "select a.dc_<DCTABLE>HistoryId,a.ModuleSerialNum, a.WhichTest, a.ModuleType, a.ErrAbbr, a.TestTimeStamp, a.TestStation,a.assemblypartnum ,d.MfgOrderName from "
                + " insite.dc_<DCTABLE> a (nolock) "+ joinstr + " where assemblypartnum in  (<PNCOND>)  <TIMECOND>  order by  moduleserialnum,testtimestamp DESC";

            var ret = new Dictionary<string, string>();
            foreach (var tb in tables)
            {
                if (string.IsNullOrEmpty(pncond))
                {
                    ret.Add(tb.Key,string.Empty);
                }
                else
                {
                     ret.Add(tb.Key,sql.Replace("<DCTABLE>",tb.Value).Replace("<PNCOND>", pncond));
                }
                
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
                    vm.Description = "Module " + item.ModuleSerialNum + " failed for " + item.ErrAbbr + " @ " + item.WhichTest +" on tester "+item.TestStation + " "+item.TestTimeStamp.ToString("yyyy-MM-dd hh:mm:ss");
                    vm.CommentType = COMMENTTYPE.Description;
                    vm.ModuleSN = item.ModuleSerialNum;
                    vm.ErrAbbr = item.ErrAbbr;
                    vm.DataID = item.DataID;
                    //ProjectEvent.CreateIssueEvent(vm.ProjectKey, "System", vm.Assignee, vm.Summary, vm.IssueKey);
                    vm.StoreIssue();
                }
            }
        }

        public static void StartProjectBonding(ProjectViewModels vm)
        {
            try
            {
                if (ProjectTestData.UpdatePJLockUsing(vm.ProjectKey))
                    return;


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
                        if (string.IsNullOrEmpty(s.Value))
                        {
                            continue;
                        }

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
                                tempdata.JO = Convert.ToString(item[8]);

                                if (!bondinged)
                                {
                                    tempdata.StoreProjectTestData();

                                    if (!sndict.ContainsKey(tempdata.ModuleSerialNum))
                                    {
                                        sndict.Add(tempdata.ModuleSerialNum, true);
                                        if (string.Compare(tempdata.ErrAbbr, "PASS", true) != 0)
                                        {
                                            var ekey = ProjectErrorViewModels.GetUniqKey();
                                            var pjerror = new ProjectErrorViewModels(vm.ProjectKey, ekey, tempdata.ErrAbbr, "", 1);
                                            pjerror.Reporter = "System";
                                            pjerror.Description = "";
                                            pjerror.AddandUpdateProjectError();

                                            failurelist.Add(tempdata);
                                        }
                                    }
                                }
                                else
                                {
                                    if (!bondingeddatadict.ContainsKey(tempdata.DataID))
                                    {
                                        bondingeddatadict.Add(tempdata.DataID,true);
                                        tempdata.StoreProjectTestData();

                                    if (!sndict.ContainsKey(tempdata.ModuleSerialNum))
                                        {
                                            sndict.Add(tempdata.ModuleSerialNum, true);
                                            if (string.Compare(tempdata.ErrAbbr, "PASS", true) != 0)
                                            {
                                                var ekey = ProjectErrorViewModels.GetUniqKey();
                                                var pjerror = new ProjectErrorViewModels(vm.ProjectKey, ekey, tempdata.ErrAbbr, "", 1);
                                                pjerror.Reporter = "System";
                                                pjerror.Description = "";
                                                pjerror.AddandUpdateProjectError();

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

                    //if (vm.FinishRating < 90)
                    //{
                    //    CreateSystemIssues(failurelist);
                    //}
                }

                ProjectTestData.ResetUpdatePJLock(vm.ProjectKey);
            }
            catch (Exception ex)
            {
                ProjectTestData.ResetUpdatePJLock(vm.ProjectKey);
            }

            
        }


        public static void UpdateProjectData(ProjectViewModels vm,string starttime,string endtime)
        {
            try
            {

                if (vm.StationList.Count > 0
                && vm.TabList.Count > 0
                && vm.PNList.Count > 0)
            {

                var failurelist = new List<ProjectTestData>();
                var passlist = new List<ProjectTestData>();

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
                    if (string.IsNullOrEmpty(s.Value))
                    {
                        continue;
                    }

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
                                tempdata.JO = Convert.ToString(item[8]);

                            if (!bondinged)
                            {
                                tempdata.StoreProjectTestData();

                                if (!sndict.ContainsKey(tempdata.ModuleSerialNum))
                                {
                                    sndict.Add(tempdata.ModuleSerialNum, true);
                                    if (string.Compare(tempdata.ErrAbbr, "PASS", true) != 0)
                                    {
                                        var ekey = ProjectErrorViewModels.GetUniqKey();
                                        var pjerror = new ProjectErrorViewModels(vm.ProjectKey, ekey, tempdata.ErrAbbr, "", 1);
                                        pjerror.Reporter = "System";
                                        pjerror.Description = "";
                                        pjerror.AddandUpdateProjectError();

                                        failurelist.Add(tempdata);
                                    }
                                    else
                                    {
                                        passlist.Add(tempdata);
                                    }
                                }
                            }
                            else
                            {
                                if (!bondingeddatadict.ContainsKey(tempdata.DataID))
                                {
                                    bondingeddatadict.Add(tempdata.DataID,true);

                                    tempdata.StoreProjectTestData();

                                    if (!sndict.ContainsKey(tempdata.ModuleSerialNum))
                                    {
                                        sndict.Add(tempdata.ModuleSerialNum, true);
                                        if (string.Compare(tempdata.ErrAbbr, "PASS", true) != 0)
                                        {
                                            var ekey = ProjectErrorViewModels.GetUniqKey();
                                            var pjerror = new ProjectErrorViewModels(vm.ProjectKey, ekey, tempdata.ErrAbbr, "", 1);
                                            pjerror.Reporter = "System";
                                            pjerror.Description = "";
                                            pjerror.AddandUpdateProjectError();

                                            failurelist.Add(tempdata);
                                        }
                                        else
                                        {
                                            passlist.Add(tempdata);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        { }
                    }
                }

                if (vm.FinishRating < 90 && DateTime.Parse(starttime) != vm.StartDate)
                {
                        //use latest failure cover previous failure
                        foreach (var item in failurelist)
                        {
                            IssueViewModels.CloseIssueAutomaticllyWithFailedSN(item.ProjectKey, item.ModuleSerialNum, item.WhichTest, item.TestStation, item.TestTimeStamp.ToString("yyyy-MM-dd hh:mm:ss"));
                        }

                        CreateSystemIssues(failurelist);
                }

                if (vm.FinishRating < 90 && DateTime.Parse(starttime) != vm.StartDate)
                {
                        //use pass sn cover previous failure
                        foreach (var item in passlist)
                    {
                        IssueViewModels.CloseIssueAutomaticlly(item.ProjectKey,item.ModuleSerialNum, item.WhichTest, item.TestStation, item.TestTimeStamp.ToString("yyyy-MM-dd hh:mm:ss"));
                    }
                }
                
            }


            }
            catch (Exception ex)
            {
                
            }

        }

        public static List<string> RetrieveAllPN(List<string> pndeslist)
        {
            var cond = "";
            foreach (var pn in pndeslist)
            {
                if (!IsDigitsOnly(pn.Trim()))
                {
                    if (string.IsNullOrEmpty(cond))
                    {
                        cond = " c.Description like '%" + pn.Trim() + "%' ";
                    }
                    else
                    {
                        cond = cond + " or c.Description like '%" + pn.Trim() + "%' ";
                    }
                }
            }

            if (string.IsNullOrEmpty(cond))
            {
                return new List<string>();
            }
            else
            {
                var ret = new List<string>();
                var sql = "select DISTINCT p.ProductName from insite.Product c (nolock) left join insite.ProductBase p on c.ProductBaseId = p.ProductBaseId where " + cond;
                var dbret = DBUtility.ExeMESSqlWithRes(sql);
                foreach (var item in dbret)
                {
                    ret.Add(Convert.ToString(item[0]));
                }
                return ret;
            }
        }


    }
}