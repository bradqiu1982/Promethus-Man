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


                    var s  = "SELECT a.MFR_SN,d.DATASET_NAME,c.FAMILY,d.STATUS,d.END_TIME,d.STATION,a.MFR_PN,d.ROUTE_ID FROM PARTS a INNER JOIN ROUTES b ON a.OPT_INDEX = b.PART_INDEX INNER JOIN BOM_CONTEXT_ID c ON c.BOM_CONTEXT_ID = b.BOM_CONTEXT_ID INNER JOIN DATASETS d ON b.ROUTE_ID = d.ROUTE_ID WHERE <mdcond> and d.END_TIME > '<TIMECOND>'  ORDER BY d.ROUTE_ID,d.END_TIME DESC";
                    var sndict = new Dictionary<string, bool>();
                    var sql = s.Replace("<TIMECOND>", vm.StartDate.ToString("yyyyMMddhhmmss")).Replace("<mdcond>", mdcond);


                    var currentroutid = "";
                    var currentroutedsnames = new List<ProjectStation>();
                    var pjdatalist = new List<ProjectTestData>();

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

                                if (pjdatalist.Count > 0)
                                {
                                    FindErrAbbr(pjdatalist, currentroutedsnames, dsdict);

                                    foreach (var pjdata in pjdatalist)
                                    {
                                        pjdata.StoreProjectTestData();
                                    }

                                    if (string.Compare(pjdatalist[0].ErrAbbr, "PASS", true) != 0)
                                    {
                                        if (!sndict.ContainsKey(pjdatalist[0].ModuleSerialNum))
                                        {
                                            sndict.Add(pjdatalist[0].ModuleSerialNum, true);

                                            failurelist.Add(pjdatalist[0]);

                                            var ekey = ProjectErrorViewModels.GetUniqKey();
                                            var pjerror = new ProjectErrorViewModels(vm.ProjectKey, ekey, pjdatalist[0].ErrAbbr, "", 1);
                                            pjerror.Reporter = "System";
                                            pjerror.Description = "";
                                            pjerror.AddandUpdateProjectError();
                                        }
                                    }
                                    else
                                    {
                                        if (!sndict.ContainsKey(pjdatalist[0].ModuleSerialNum))
                                        {
                                            sndict.Add(pjdatalist[0].ModuleSerialNum, true);
                                        }
                                    }

                                }//end if (tempdata != null)

                                pjdatalist.Clear();
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
                                var tempdata = new ProjectTestData(vm.ProjectKey, GetUniqKey(), Convert.ToString(item[0])
                                            , Convert.ToString(item[1]), Convert.ToString(item[2]), Convert.ToString(item[3]).ToUpper()
                                            , stdtime, Convert.ToString(item[5]), Convert.ToString(item[6]));

                                pjdatalist.Add(tempdata);
                            }

                        }
                            catch (Exception ex)
                            { }
                        }//end for each

                    if (pjdatalist.Count > 0)
                    {
                        FindErrAbbr(pjdatalist, currentroutedsnames, dsdict);

                        foreach (var pjdata in pjdatalist)
                        {
                            pjdata.StoreProjectTestData();
                        }

                        if (string.Compare(pjdatalist[0].ErrAbbr, "PASS", true) != 0)
                        {
                            if (!sndict.ContainsKey(pjdatalist[0].ModuleSerialNum))
                            {
                                sndict.Add(pjdatalist[0].ModuleSerialNum, true);

                                failurelist.Add(pjdatalist[0]);

                                var ekey = ProjectErrorViewModels.GetUniqKey();
                                var pjerror = new ProjectErrorViewModels(vm.ProjectKey, ekey, pjdatalist[0].ErrAbbr, "", 1);
                                pjerror.Reporter = "System";
                                pjerror.Description = "";
                                pjerror.AddandUpdateProjectError();
                            }
                        }
                        else
                        {
                            if (!sndict.ContainsKey(pjdatalist[0].ModuleSerialNum))
                            {
                                sndict.Add(pjdatalist[0].ModuleSerialNum, true);
                            }
                        }

                    }//end if (tempdata != null)

                }// if model list > 0

                ProjectTestData.ResetUpdatePJLock(vm.ProjectKey);
            }
            catch (Exception ex)
            {
                ProjectTestData.ResetUpdatePJLock(vm.ProjectKey);
            }
        }


        public static void FindErrAbbr(List<ProjectTestData> pjdatalist, List<ProjectStation> currentroutedsnames, Dictionary<string, bool> dsdict)
        {
            var startidx = 0;
            foreach (var pjdata in pjdatalist)
            {
                if (string.Compare(pjdata.ErrAbbr, "PASS", true) == 0)
                {
                    continue;
                }

                for (var idx = startidx; idx < currentroutedsnames.Count; idx++)
                {
                    if (string.Compare(currentroutedsnames[idx].Station, "PASS", true) != 0
                        && string.Compare(currentroutedsnames[idx].Station, "INFO", true) != 0
                        && !dsdict.ContainsKey(currentroutedsnames[idx].ProjectKey))
                    {
                        pjdata.ErrAbbr = currentroutedsnames[idx].ProjectKey;
                        startidx = idx + 1;
                        break;
                    }

                    //find the stard position of next indivition dataset
                    for (var tmpidx = startidx; tmpidx < currentroutedsnames.Count; tmpidx++)
                    {
                        if (dsdict.ContainsKey(currentroutedsnames[tmpidx].ProjectKey))
                        {
                            startidx = tmpidx;
                            break;
                        }
                    }//end for
                }
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


                    var s = "SELECT a.MFR_SN,d.DATASET_NAME,c.FAMILY,d.STATUS,d.END_TIME,d.STATION,a.MFR_PN,d.ROUTE_ID FROM PARTS a INNER JOIN ROUTES b ON a.OPT_INDEX = b.PART_INDEX INNER JOIN BOM_CONTEXT_ID c ON c.BOM_CONTEXT_ID = b.BOM_CONTEXT_ID INNER JOIN DATASETS d ON b.ROUTE_ID = d.ROUTE_ID WHERE <mdcond> and d.END_TIME > '<TIMECOND>'  ORDER BY d.ROUTE_ID,d.END_TIME DESC";
                    var sndict = new Dictionary<string, bool>();
                    var sql = s.Replace("<TIMECOND>", DateTime.Parse(starttime).ToString("yyyyMMddhhmmss")).Replace("<mdcond>", mdcond);

                    var currentroutid = "";
                    var currentroutedsnames = new List<ProjectStation>();
                    var pjdatalist = new List<ProjectTestData>();

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

                                if (pjdatalist.Count > 0)
                                {
                                    FindErrAbbr(pjdatalist, currentroutedsnames, dsdict);

                                    foreach (var pjdata in pjdatalist)
                                    {
                                        pjdata.StoreProjectTestData();
                                    }

                                    if (string.Compare(pjdatalist[0].ErrAbbr, "PASS", true) != 0)
                                    {
                                        if (!sndict.ContainsKey(pjdatalist[0].ModuleSerialNum))
                                        {
                                            sndict.Add(pjdatalist[0].ModuleSerialNum, true);

                                            failurelist.Add(pjdatalist[0]);

                                            var ekey = ProjectErrorViewModels.GetUniqKey();
                                            var pjerror = new ProjectErrorViewModels(vm.ProjectKey, ekey, pjdatalist[0].ErrAbbr, "", 1);
                                            pjerror.Reporter = "System";
                                            pjerror.Description = "";
                                            pjerror.AddandUpdateProjectError();
                                        }
                                    }
                                    else
                                    {
                                        if (!sndict.ContainsKey(pjdatalist[0].ModuleSerialNum))
                                        {
                                            sndict.Add(pjdatalist[0].ModuleSerialNum, true);
                                            passlist.Add(pjdatalist[0]);
                                        }
                                    }

                                }//end if (tempdata != null)

                                pjdatalist.Clear();
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
                                var tempdata = new ProjectTestData(vm.ProjectKey, GetUniqKey(), Convert.ToString(item[0])
                                            , Convert.ToString(item[1]), Convert.ToString(item[2]), Convert.ToString(item[3]).ToUpper()
                                            , stdtime, Convert.ToString(item[5]), Convert.ToString(item[6]));

                                pjdatalist.Add(tempdata);
                            }
                        }
                        catch (Exception ex)
                        { }
                    }//end for each

                    if (pjdatalist.Count > 0)
                    {
                        FindErrAbbr(pjdatalist, currentroutedsnames, dsdict);

                        foreach (var pjdata in pjdatalist)
                        {
                            pjdata.StoreProjectTestData();
                        }

                        if (string.Compare(pjdatalist[0].ErrAbbr, "PASS", true) != 0)
                        {
                            if (!sndict.ContainsKey(pjdatalist[0].ModuleSerialNum))
                            {
                                sndict.Add(pjdatalist[0].ModuleSerialNum, true);

                                failurelist.Add(pjdatalist[0]);

                                var ekey = ProjectErrorViewModels.GetUniqKey();
                                var pjerror = new ProjectErrorViewModels(vm.ProjectKey, ekey, pjdatalist[0].ErrAbbr, "", 1);
                                pjerror.Reporter = "System";
                                pjerror.Description = "";
                                pjerror.AddandUpdateProjectError();
                            }
                        }
                        else
                        {
                            if (!sndict.ContainsKey(pjdatalist[0].ModuleSerialNum))
                            {
                                sndict.Add(pjdatalist[0].ModuleSerialNum, true);
                                passlist.Add(pjdatalist[0]);
                            }
                        }

                    }//end if (tempdata != null)

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