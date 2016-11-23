using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
    public class ATEUtility
    {
        public static void retrieveATEData()
        {
            var sql = "SELECT d.ROUTE_ID,a.MFR_SN,d.DATASET_NAME,c.FAMILY,d.STATUS,d.END_TIME,d.STATION,a.MFR_PN FROM PARTS a INNER JOIN ROUTES b ON a.OPT_INDEX = b.PART_INDEX INNER JOIN BOM_CONTEXT_ID c ON c.BOM_CONTEXT_ID = b.BOM_CONTEXT_ID INNER JOIN DATASETS d ON b.ROUTE_ID = d.ROUTE_ID WHERE(c.MODEL_ID like 'FTLX6871%' or c.MODEL_ID like 'FTLX6872%' or c.MODEL_ID like 'FTLX6672%') and d.END_TIME > '20161119080000'  and d.DATASET_NAME IN('final', 'assembly', 'tune_rf', 'tune', 'temp_test_7up') ORDER BY d.END_TIME DESC";
            DBUtility.ExeATESqlWithRes(sql);
        }

        private static string GetUniqKey()
        {
            return Guid.NewGuid().ToString("N");
        }

        public static void StartProjectBonding(ProjectViewModels vm)
        {
            try
            {
                if (ProjectTestData.UpdatePJLockUsing(vm.ProjectKey))
                    return;


                if (vm.MDIDList.Count > 0
                    && vm.SumDatasetList.Count > 0)
                {
                    var dsdict = new Dictionary<string, bool>();
                    foreach (var item in vm.SumDatasetList)
                    {
                        dsdict.Add(item.Station, true);
                    }

                    var failurelist = new List<ProjectTestData>();
                    

                    //(c.MODEL_ID like 'FTLX6871%' or c.MODEL_ID like 'FTLX6872%' or c.MODEL_ID like 'FTLX6672%')
                    var mdcond = "(";
                    foreach (var w in vm.MDIDList)
                    {
                        mdcond = mdcond + "c.MODEL_ID like '"+w.Pn.Trim()+"%' or ";
                    }
                    mdcond = mdcond.Substring(0, mdcond.Length - 4) + ")";


                    var s  = "SELECT a.MFR_SN,d.DATASET_NAME,c.FAMILY,d.STATUS,d.END_TIME,d.STATION,a.MFR_PN,d.ROUTE_ID FROM PARTS a INNER JOIN ROUTES b ON a.OPT_INDEX = b.PART_INDEX INNER JOIN BOM_CONTEXT_ID c ON c.BOM_CONTEXT_ID = b.BOM_CONTEXT_ID INNER JOIN DATASETS d ON b.ROUTE_ID = d.ROUTE_ID WHERE <mdcond> and d.END_TIME > '<TIMECOND>'  ORDER BY d.END_TIME DESC,d.ROUTE_ID";
                    var sndict = new Dictionary<string, bool>();
                    var sql = s.Replace("<TIMECOND>", vm.StartDate.ToString("yyyyMMddhhmmss")).Replace("<mdcond>", mdcond);


                    var currentroutid = "";
                    var currentroutedsnames = new List<ProjectStation>();
                    ProjectTestData tempdata = null;

                    var dbret = DBUtility.ExeATESqlWithRes(sql);

                        foreach (var item in dbret)
                        {
                            try
                            {
                                var status = Convert.ToString(item[3]);
                                var ds = Convert.ToString(item[1]);
                                var temprouteid = Convert.ToString(item[7]);

                                if (string.Compare(currentroutid, temprouteid) != 0)
                                {
                                    currentroutid = temprouteid;

                                    if (tempdata != null)
                                    {
                                        if (string.Compare(tempdata.ErrAbbr, "PASS", true) != 0)
                                        {
                                            foreach (var dsstatus in currentroutedsnames)
                                            {
                                                if (string.Compare(dsstatus.Station, "PASS", true) != 0
                                                && string.Compare(dsstatus.Station, "INFO", true) != 0
                                                && !dsdict.ContainsKey(dsstatus.ProjectKey))
                                                {
                                                    tempdata.ErrAbbr = dsstatus.ProjectKey;
                                                    break;
                                                }
                                            }

                                            tempdata.StoreProjectTestData();

                                            if (!sndict.ContainsKey(tempdata.ModuleSerialNum))
                                            {
                                                sndict.Add(tempdata.ModuleSerialNum, true);

                                                var ekey = ProjectErrorViewModels.GetUniqKey();
                                                var pjerror = new ProjectErrorViewModels(vm.ProjectKey, ekey, tempdata.ErrAbbr, "", 1);
                                                pjerror.Reporter = "System";
                                                pjerror.Description = "";
                                                pjerror.AddandUpdateProjectError();

                                                failurelist.Add(tempdata);
                                            }
                                        }
                                        else //error is pass
                                        {
                                                tempdata.StoreProjectTestData();
                                                if (!sndict.ContainsKey(tempdata.ModuleSerialNum))
                                                {
                                                    sndict.Add(tempdata.ModuleSerialNum, true);
                                                }
                                        }

                                        tempdata = null;
                                    }//end if (tempdata != null)

                                    currentroutedsnames.Clear();
                                    currentroutedsnames.Add(new ProjectStation(ds, status));
                                }
                                else //if (tempdata != null)
                                {
                                    currentroutedsnames.Add(new ProjectStation(ds, status));
                                }

                                if (dsdict.ContainsKey(ds))
                                {
                                //(string pk, string did, string sn, string wtest, string mt, string err, string testtime, string station, string p)
                                var spdatetime = Convert.ToString(item[4]);
                                var stdtime = spdatetime.Substring(0, 4) + "-" + spdatetime.Substring(4, 2) + "-" + spdatetime.Substring(6, 2) + " "
                                              + spdatetime.Substring(8, 2) + ":" + spdatetime.Substring(10, 2) + ":" + spdatetime.Substring(12, 2);
                                tempdata = new ProjectTestData(vm.ProjectKey, GetUniqKey(), Convert.ToString(item[0])
                                            , Convert.ToString(item[1]), Convert.ToString(item[2]), Convert.ToString(item[3]).ToUpper()
                                            , stdtime, Convert.ToString(item[5]), Convert.ToString(item[6]));
                                }
                            
                            }
                            catch (Exception ex)
                            { }
                        }//end for each

                        if (tempdata != null)
                        {
                            if (string.Compare(tempdata.ErrAbbr, "PASS", true) != 0)
                            {
                                foreach (var dsstatus in currentroutedsnames)
                                {
                                    if (string.Compare(dsstatus.Station, "PASS", true) != 0
                                    && string.Compare(dsstatus.Station, "INFO", true) != 0
                                    && !dsdict.ContainsKey(dsstatus.ProjectKey))
                                    {
                                        tempdata.ErrAbbr = dsstatus.ProjectKey;
                                        break;
                                    }
                                }

                                tempdata.StoreProjectTestData();

                                if (!sndict.ContainsKey(tempdata.ModuleSerialNum))
                                {
                                    sndict.Add(tempdata.ModuleSerialNum, true);

                                    var ekey = ProjectErrorViewModels.GetUniqKey();
                                    var pjerror = new ProjectErrorViewModels(vm.ProjectKey, ekey, tempdata.ErrAbbr, "", 1);
                                    pjerror.Reporter = "System";
                                    pjerror.Description = "";
                                    pjerror.AddandUpdateProjectError();

                                    failurelist.Add(tempdata);
                                }
                            }
                            else //error is pass
                            {
                                tempdata.StoreProjectTestData();
                                if (!sndict.ContainsKey(tempdata.ModuleSerialNum))
                                {
                                    sndict.Add(tempdata.ModuleSerialNum, true);
                                }
                            }

                            tempdata = null;
                        }//end if (tempdata != null)

                }// if model list > 0

                ProjectTestData.ResetUpdatePJLock(vm.ProjectKey);
            }
            catch (Exception ex)
            {
                ProjectTestData.ResetUpdatePJLock(vm.ProjectKey);
            }

        }

        public static void UpdateProjectData(ProjectViewModels vm, string starttime)
        {
            try
            {
               if (vm.MDIDList.Count > 0
                    && vm.SumDatasetList.Count > 0)
                {
                    var dsdict = new Dictionary<string, bool>();
                    foreach (var item in vm.SumDatasetList)
                    {
                        dsdict.Add(item.Station, true);
                    }

                    var failurelist = new List<ProjectTestData>();
                    var passlist = new List<ProjectTestData>();

                    //(c.MODEL_ID like 'FTLX6871%' or c.MODEL_ID like 'FTLX6872%' or c.MODEL_ID like 'FTLX6672%')
                    var mdcond = "(";
                    foreach (var w in vm.MDIDList)
                    {
                        mdcond = mdcond + "c.MODEL_ID like '" + w.Pn.Trim() + "%' or ";
                    }
                    mdcond = mdcond.Substring(0, mdcond.Length - 4) + ")";


                    var s = "SELECT a.MFR_SN,d.DATASET_NAME,c.FAMILY,d.STATUS,d.END_TIME,d.STATION,a.MFR_PN,d.ROUTE_ID FROM PARTS a INNER JOIN ROUTES b ON a.OPT_INDEX = b.PART_INDEX INNER JOIN BOM_CONTEXT_ID c ON c.BOM_CONTEXT_ID = b.BOM_CONTEXT_ID INNER JOIN DATASETS d ON b.ROUTE_ID = d.ROUTE_ID WHERE <mdcond> and d.END_TIME > '<TIMECOND>'  ORDER BY d.END_TIME DESC,d.ROUTE_ID";
                    var sndict = new Dictionary<string, bool>();
                    var sql = s.Replace("<TIMECOND>", DateTime.Parse(starttime).ToString("yyyyMMddhhmmss")).Replace("<mdcond>", mdcond);

                    var currentroutid = "";
                    var currentroutedsnames = new List<ProjectStation>();
                    ProjectTestData tempdata = null;

                    var dbret = DBUtility.ExeATESqlWithRes(sql);

                    foreach (var item in dbret)
                    {
                        try
                        {
                            var status = Convert.ToString(item[3]);
                            var ds = Convert.ToString(item[1]);
                            var temprouteid = Convert.ToString(item[7]);

                            if (string.Compare(currentroutid, temprouteid) != 0)
                            {
                                currentroutid = temprouteid;

                                if (tempdata != null)
                                {
                                    if (string.Compare(tempdata.ErrAbbr, "PASS", true) != 0)
                                    {
                                        foreach (var dsstatus in currentroutedsnames)
                                        {
                                            if (string.Compare(dsstatus.Station, "PASS", true) != 0
                                            && string.Compare(dsstatus.Station, "INFO", true) != 0
                                            && !dsdict.ContainsKey(dsstatus.ProjectKey))
                                            {
                                                tempdata.ErrAbbr = dsstatus.ProjectKey;
                                                break;
                                            }
                                        }

                                        tempdata.StoreProjectTestData();

                                        if (!sndict.ContainsKey(tempdata.ModuleSerialNum))
                                        {
                                            sndict.Add(tempdata.ModuleSerialNum, true);

                                            var ekey = ProjectErrorViewModels.GetUniqKey();
                                            var pjerror = new ProjectErrorViewModels(vm.ProjectKey, ekey, tempdata.ErrAbbr, "", 1);
                                            pjerror.Reporter = "System";
                                            pjerror.Description = "";
                                            pjerror.AddandUpdateProjectError();

                                            failurelist.Add(tempdata);
                                        }
                                    }
                                    else if (string.Compare(tempdata.ErrAbbr, "PASS", true) == 0)//error is pass
                                    {
                                        tempdata.StoreProjectTestData();

                                        if (!sndict.ContainsKey(tempdata.ModuleSerialNum))
                                        {
                                            passlist.Add(tempdata);
                                            sndict.Add(tempdata.ModuleSerialNum, true);
                                        }
                                    }

                                    tempdata = null;
                                }//end if (tempdata != null)

                                currentroutedsnames.Clear();
                                currentroutedsnames.Add(new ProjectStation(ds, status));
                            }
                            else //if (tempdata != null)
                            {
                                currentroutedsnames.Add(new ProjectStation(ds, status));
                            }

                            if (dsdict.ContainsKey(ds))
                            {
                                var spdatetime = Convert.ToString(item[4]);
                                var stdtime = spdatetime.Substring(0, 4) + "-" + spdatetime.Substring(4, 2) + "-" + spdatetime.Substring(6, 2) + " "
                                              + spdatetime.Substring(8, 2) + ":" + spdatetime.Substring(10, 2) + ":" + spdatetime.Substring(12, 2);

                                //(string pk, string did, string sn, string wtest, string mt, string err, string testtime, string station, string p)
                                tempdata = new ProjectTestData(vm.ProjectKey, GetUniqKey(), Convert.ToString(item[0])
                                        , Convert.ToString(item[1]), Convert.ToString(item[2]), Convert.ToString(item[3]).ToUpper()
                                        , stdtime, Convert.ToString(item[5]), Convert.ToString(item[6]));
                            }

                        }
                        catch (Exception ex)
                        { }
                    }//end for each

                    if (tempdata != null)
                    {
                        if (string.Compare(tempdata.ErrAbbr, "PASS", true) != 0)
                        {
                            foreach (var dsstatus in currentroutedsnames)
                            {
                                if (string.Compare(dsstatus.Station, "PASS", true) != 0
                                && string.Compare(dsstatus.Station, "INFO", true) != 0
                                && !dsdict.ContainsKey(dsstatus.ProjectKey))
                                {
                                    tempdata.ErrAbbr = dsstatus.ProjectKey;
                                    break;
                                }
                            }

                            tempdata.StoreProjectTestData();

                            if (!sndict.ContainsKey(tempdata.ModuleSerialNum))
                            {
                                sndict.Add(tempdata.ModuleSerialNum, true);

                                var ekey = ProjectErrorViewModels.GetUniqKey();
                                var pjerror = new ProjectErrorViewModels(vm.ProjectKey, ekey, tempdata.ErrAbbr, "", 1);
                                pjerror.Reporter = "System";
                                pjerror.Description = "";
                                pjerror.AddandUpdateProjectError();

                                failurelist.Add(tempdata);
                            }
                        }
                        else if (string.Compare(tempdata.ErrAbbr, "PASS", true) == 0)//error is pass
                        {
                            tempdata.StoreProjectTestData();
                            
                            if (!sndict.ContainsKey(tempdata.ModuleSerialNum))
                            {
                                passlist.Add(tempdata);
                                sndict.Add(tempdata.ModuleSerialNum, true);
                            }
                        }

                        tempdata = null;
                    }//end if (tempdata != null)

                    if (vm.FinishRating < 90 && DateTime.Parse(starttime) != vm.StartDate)
                    {
                        CreateSystemIssues(failurelist);
                    }

                    if (vm.FinishRating < 90 && DateTime.Parse(starttime) != vm.StartDate)
                    {
                        foreach (var item in passlist)
                        {
                            IssueViewModels.CloseIssueAutomaticlly(item.ProjectKey, item.ModuleSerialNum, item.WhichTest, item.TestStation, item.TestTimeStamp.ToString("yyyy-MM-dd hh:mm:ss"));
                        }
                    }


                }// if model list > 0
            }
            catch (Exception ex)
            {
            }

        }

        private static void CreateSystemIssues(List<ProjectTestData> failurelist)
        {
            if (failurelist.Count > 0)
            {
                var pj = ProjectViewModels.RetrieveOneProject(failurelist[0].ProjectKey);
                var firstengineer = "";
                foreach (var m in pj.MemberList)
                {
                    if (string.Compare(m.Role, ProjectViewModels.ENGROLE) == 0)
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
                    vm.Summary = "Module " + item.ModuleSerialNum + " failed for " + item.ErrAbbr + " @ " + item.WhichTest;
                    vm.Priority = ISSUEPR.Major;
                    vm.DueDate = DateTime.Now.AddDays(7);
                    vm.ReportDate = item.TestTimeStamp;
                    vm.Assignee = firstengineer;
                    vm.Reporter = "System";
                    vm.Resolution = Resolute.Pending;
                    vm.ResolvedDate = DateTime.Parse("1982-05-06 01:01:01");
                    vm.Description = "Module " + item.ModuleSerialNum + " failed for " + item.ErrAbbr + " @ " + item.WhichTest + " on tester " + item.TestStation + " " + item.TestTimeStamp.ToString("yyyy-MM-dd hh:mm:ss");
                    vm.CommentType = COMMENTTYPE.Description;
                    vm.ModuleSN = item.ModuleSerialNum;
                    vm.ErrAbbr = item.ErrAbbr;
                    vm.DataID = item.DataID;
                    ProjectEvent.CreateIssueEvent(vm.ProjectKey, "System", vm.Assignee, vm.Summary, vm.IssueKey);
                    vm.StoreIssue();
                }
            }
        }







    }
}