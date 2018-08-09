﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Prometheus.Models
{
    public class TempDataSetItem
    {
        public TempDataSetItem(string ds, string status,string tempdatasetid)
        {
            DatasetName = ds;
            Status = status;
            DatasetID = tempdatasetid;
        }

        public string DatasetName { set; get; }
        public string Status { set; get; }
        public string DatasetID { set; get; }
    }

    public class ATEUtility
    {
        public static void retrieveATEData()
        {
            var sql = "SELECT d.ROUTE_ID,a.MFR_SN,d.DATASET_NAME,c.FAMILY,d.STATUS,d.END_TIME,d.STATION,a.MFR_PN FROM PARTS a INNER JOIN ROUTES b ON a.OPT_INDEX = b.PART_INDEX INNER JOIN BOM_CONTEXT_ID c ON c.BOM_CONTEXT_ID = b.BOM_CONTEXT_ID INNER JOIN DATASETS d ON b.ROUTE_ID = d.ROUTE_ID WHERE(c.MODEL_ID like 'FTLX6871%' or c.MODEL_ID like 'FTLX6872%' or c.MODEL_ID like 'FTLX6672%') and d.END_TIME > '20161119080000'  and d.DATASET_NAME IN('final', 'assembly', 'tune_rf', 'tune', 'temp_test_7up') ORDER BY d.END_TIME DESC";
            DBUtility.ExeATESqlWithRes(sql);
        }

        public static void EmailATETestDailyData(string mdtype,Controller ctrl)
        {
            var syscfg = CfgUtility.GetSysConfig(ctrl);
            var towho = syscfg["TUNABLETESTDATAOWNER"].Split(new string[] { ";",","," " }, StringSplitOptions.RemoveEmptyEntries).ToList();

            var onedayagon = DateTime.Now.AddDays(-1);
            var sdate = DateTime.Parse(onedayagon.ToString("yyyy-MM-dd") + " 00:00:00");
            var edate = DateTime.Parse(onedayagon.ToString("yyyy-MM-dd") + " 23:59:59");

            var pndict = new Dictionary<string, bool>();
            var atedatalist = ATEUtility.FilteredATEData(mdtype, sdate, edate, pndict);

            if (atedatalist.Count > 0)
            {
                var pndescdict = MESUtility.RetrievePNDescByPn(new List<string>(pndict.Keys));
                var sb = PrePareATEData(atedatalist, pndescdict);

                string datestring = DateTime.Now.ToString("yyyyMMdd");
                string imgdir = ctrl.Server.MapPath("~/userfiles") + "\\docs\\" + datestring + "\\";
                if (!Directory.Exists(imgdir))
                {
                    Directory.CreateDirectory(imgdir);
                }

                var family = atedatalist[0].ModuleType.Replace(" ", "_").Replace("#", "").Replace("'", "")
                            .Replace("&", "").Replace("?", "").Replace("%", "").Replace("+", "");

                var fn = "ATE_" + family + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
                var filename = imgdir + fn;

                var fw = System.IO.File.OpenWrite(filename);
                var CHUNK_STRING_LENGTH = 30000;
                while (sb.Length > CHUNK_STRING_LENGTH)
                {
                    var bt = System.Text.Encoding.UTF8.GetBytes(sb.ToString(0, CHUNK_STRING_LENGTH));
                    fw.Write(bt, 0, bt.Count());
                    sb.Remove(0, CHUNK_STRING_LENGTH);
                }

                var bt1 = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
                fw.Write(bt1, 0, bt1.Count());
                fw.Close();

                try
                {
                    var fzip = new ICSharpCode.SharpZipLib.Zip.FastZip();
                    fzip.CreateZip(imgdir + fn.Replace(".csv", ".zip"), imgdir, false, fn);
                    try { System.IO.File.Delete(filename); } catch (Exception ex) { }
                    EmailUtility.SendEmail(ctrl, "ATE Daily Test Data - " + mdtype, towho
                    , "Daily Test Data\r\nFYI", true, filename.Replace(".csv", ".zip"));
                }
                catch (Exception ex)
                {
                    if (!System.IO.File.Exists(filename))
                    { System.IO.File.WriteAllText(filename, "Fail to download data."); }
                    EmailUtility.SendEmail(ctrl, "ATE Daily Test Data - " + mdtype, towho
                    , "Daily Test Data\r\nFYI", true, filename);
                }
                
                new System.Threading.ManualResetEvent(false).WaitOne(500);
            }

        }

        public static StringBuilder PrePareATEData(List<ProjectTestData> atelist, Dictionary<string, string> pndescdict)
        {
            var stationdict = new Dictionary<string, double>();
            var whichtestspend = new Dictionary<string, double>();
            var whichtestcount = new Dictionary<string, int>();

            StringBuilder sb1 = new StringBuilder(300 * (atelist.Count + 1));
            sb1.Append("SN,WhichTest,Failure,TestTimestamp,Station,Module Family,PN,PN Desc,JO,Spend Time(in hour)\r\n");
            foreach (var item in atelist)
            {
                var pndesc = "";
                if (pndescdict.ContainsKey(item.PN))
                { pndesc = pndescdict[item.PN]; }

                sb1.Append("\"" + item.ModuleSerialNum.ToString().Replace("\"", "") + "\"," + "\"" + item.WhichTest.Replace("\"", "") + "\"," + "\"" + item.ErrAbbr.Replace("\"", "") + "\","
                    + "\"" + item.TestTimeStamp.ToString("yyyy-MM-dd HH:mm:ss") + "\"," + "\"" + item.TestStation.Replace("\"", "") + "\"," + "\"" + item.ModuleType.Replace("\"", "") + "\","
                    + "\"" + item.PN.Replace("\"", "") + "\"," + "\"" + pndesc.Replace("\"", "") + "\"," + "\"" + item.JO.Replace("\"", "") + "\"," + "\"" + item.CSN.Replace("\"", "") + "\",\r\n");

                var spendtime = Convert.ToDouble(item.CSN);
                if (stationdict.ContainsKey(item.TestStation))
                { stationdict[item.TestStation] = stationdict[item.TestStation] + spendtime; }
                else
                { stationdict.Add(item.TestStation, spendtime); }

                if (whichtestspend.ContainsKey(item.WhichTest))
                { whichtestspend[item.WhichTest] = whichtestspend[item.WhichTest] + spendtime;
                    whichtestcount[item.WhichTest] = whichtestcount[item.WhichTest] + 1;
                }
                else
                { whichtestspend.Add(item.WhichTest, spendtime);
                    whichtestcount.Add(item.WhichTest, 1);
                }
            }

            if (stationdict.Count > 0)
            {
                sb1.Append("\"Station\",\"Spend Time\",\r\n");
                foreach (var kv in stationdict)
                {
                    sb1.Append("\"" + kv.Key.Replace("\"", "") + "\"," + "\"" + kv.Value.ToString().Replace("\"", "") + "\",\r\n");
                }
            }

            if (whichtestspend.Count > 0)
            {
                sb1.Append("\"Which Test\",\"Spend Time\",\"Test Times\",\r\n");
                foreach (var kv in whichtestspend)
                {
                    sb1.Append("\"" + kv.Key.Replace("\"", "") + "\"," + "\"" + kv.Value.ToString().Replace("\"", "") + "\"," + "\"" + whichtestcount[kv.Key].ToString().Replace("\"", "") + "\",\r\n");
                }
            }

            return sb1;
        }

        public static List<ProjectTestData> RetrieveATEData(string family,DateTime startdate,DateTime enddate,Dictionary<string,bool> pndict)
        {
            var ret = new List<ProjectTestData>();

            var sql = @"SELECT a.MFR_SN,d.DATASET_NAME,c.FAMILY,d.STATUS,d.END_TIME,d.STATION,a.MFR_PN,d.ROUTE_ID,d.dataset_id,b.state,b.JOB_ID FROM PARTS a   
                        INNER JOIN ROUTES b ON a.OPT_INDEX = b.PART_INDEX  
                        INNER JOIN BOM_CONTEXT_ID c ON c.BOM_CONTEXT_ID = b.BOM_CONTEXT_ID 
                        INNER JOIN DATASETS d ON b.ROUTE_ID = d.ROUTE_ID   
                        WHERE c.FAMILY = '<family>' and d.END_TIME >= '<starttime>' and d.END_TIME <= '<endtime>' AND b.state <> 'GOLDEN'  and d.invalid_flag <> '1' ORDER BY a.MFR_SN,d.start_time ASC";
            sql = sql.Replace("<family>", family).Replace("<starttime>", startdate.ToString("yyyyMMddHHmmss")).Replace("<endtime>", enddate.ToString("yyyyMMddHHmmss"));

            var currentroutid = "";
            var currentstation = "";
            var currentsn = "";

            var pjdatalist = new List<ProjectTestData>();

            var dbret = DBUtility.ExeATESqlWithRes(sql);

            foreach (var item in dbret)
            {
                try
                {
                    var sn = Convert.ToString(item[0]);
                    var ds = Convert.ToString(item[1]);
                    var mdtype = Convert.ToString(item[2]);
                    var status = Convert.ToString(item[3]);

                    var spdatetime = Convert.ToString(item[4]);
                    var stdtime = spdatetime.Substring(0, 4) + "-" + spdatetime.Substring(4, 2) + "-" + spdatetime.Substring(6, 2) + " "
                                          + spdatetime.Substring(8, 2) + ":" + spdatetime.Substring(10, 2) + ":" + spdatetime.Substring(12, 2);
                    var station = Convert.ToString(item[5]);
                    var pn = Convert.ToString(item[6]);
                    if (!pndict.ContainsKey(pn))
                    { pndict.Add(pn, true); }

                    var temprouteid = Convert.ToString(item[7]);
                    var tempdatasetid = Convert.ToString(item[8]);
                    var state = Convert.ToString(item[9]);
                    var jobid = Convert.ToString(item[10]);

                    var setupflag = false;
                    if (ds.ToUpper().Contains("_SETUP"))
                    { setupflag = true; }

                    if (string.Compare(currentroutid, temprouteid) != 0 
                        || string.Compare(currentstation, station) != 0
                        || string.Compare(currentsn, sn) != 0
                        || setupflag)
                    {
                        currentroutid = temprouteid;
                        currentstation = station;
                        currentsn = sn;

                        if (pjdatalist.Count > 0)
                        {
                            var err = "";
                            var errid = "";
                            foreach (var line in pjdatalist)
                            {
                                if (string.Compare(line.ErrAbbr, "PASS", true) != 0
                                    && string.Compare(line.ErrAbbr, "INFO", true) != 0)
                                {
                                    err = line.WhichTest;
                                    errid = line.DataID;
                                    break;
                                }
                            }//end foreach

                            var hours = (double)(pjdatalist[pjdatalist.Count - 1].TestTimeStamp - pjdatalist[0].TestTimeStamp).TotalSeconds / 3600.0;
                            if (hours != 0.0)
                            {
                                if (string.IsNullOrEmpty(errid))
                                {
                                
                                    var testdata = new ProjectTestData("TEMP", pjdatalist[0].DataID, pjdatalist[0].ModuleSerialNum, pjdatalist[0].WhichTest.Replace("_setup", ""), pjdatalist[0].ModuleType
                                        , "PASS", pjdatalist[0].TestTimeStamp.ToString("yyyy-MM-dd HH:mm:ss"), pjdatalist[0].TestStation, pjdatalist[0].PN);
                                    testdata.CSN = (Math.Round(hours,3)).ToString();
                                    testdata.JO = pjdatalist[0].JO;
                                    ret.Add(testdata);
                                }
                                else
                                {
                                
                                    var testdata = new ProjectTestData("TEMP", errid, pjdatalist[0].ModuleSerialNum, pjdatalist[0].WhichTest.Replace("_setup", ""), pjdatalist[0].ModuleType
                                        , err, pjdatalist[0].TestTimeStamp.ToString("yyyy-MM-dd HH:mm:ss"), pjdatalist[0].TestStation, pjdatalist[0].PN);
                                    testdata.CSN = (Math.Round(hours, 3)).ToString();
                                    testdata.JO = pjdatalist[0].JO;
                                    ret.Add(testdata);
                                }
                            }//end if hour != 0.0
                        }//end if pjdatalist.Count > 0

                        pjdatalist.Clear();
                      
                        //(string pk, string did, string sn, string wtest, string mt, string err, string testtime, string station, string p)
                        var tempdata = new ProjectTestData("TEMP", tempdatasetid, sn, ds, mdtype, status, stdtime, station, pn);
                        tempdata.JO = jobid;
                        pjdatalist.Add(tempdata);
                    }
                    else
                    {
                        //(string pk, string did, string sn, string wtest, string mt, string err, string testtime, string station, string p)
                        var tempdata = new ProjectTestData("TEMP", tempdatasetid, sn , ds, mdtype, status, stdtime, station, pn);
                        tempdata.JO = jobid;
                        pjdatalist.Add(tempdata);
                    }

                }
                catch (Exception ex)
                { }
            }

            return ret;
        }

        public static List<ProjectTestData> FilteredATEData(string mdtype, DateTime sdate, DateTime edate, Dictionary<string, bool> pndict)
        {
            var ret = new List<ProjectTestData>();
            var rawdata = ATEUtility.RetrieveATEData(mdtype, sdate, edate, pndict);
            if (rawdata.Count > 0)
            {
                var allpndict = new Dictionary<string, bool>();
                var previousdata = ATEUtility.RetrieveATEData(mdtype, sdate.AddMonths(-2), sdate, allpndict);
                var filterdict = new Dictionary<string, bool>();
                foreach (var item in previousdata)
                {
                    if (!filterdict.ContainsKey(item.ModuleSerialNum + ":::" + item.WhichTest))
                    {
                        filterdict.Add(item.ModuleSerialNum + ":::" + item.WhichTest, true);
                    }
                }
                foreach (var item in rawdata)
                {
                    if (!filterdict.ContainsKey(item.ModuleSerialNum + ":::" + item.WhichTest))
                    {
                        ret.Add(item);
                    }
                }
            }
            return ret;
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


                    var s  = "SELECT a.MFR_SN,d.DATASET_NAME,c.FAMILY,d.STATUS,d.END_TIME,d.STATION,a.MFR_PN,d.ROUTE_ID,d.dataset_id,b.JOB_ID FROM PARTS a INNER JOIN ROUTES b ON a.OPT_INDEX = b.PART_INDEX INNER JOIN BOM_CONTEXT_ID c ON c.BOM_CONTEXT_ID = b.BOM_CONTEXT_ID INNER JOIN DATASETS d ON b.ROUTE_ID = d.ROUTE_ID WHERE <mdcond> and d.END_TIME > '<TIMECOND>'  ORDER BY d.ROUTE_ID,d.dataset_id DESC,d.END_TIME DESC";
                    var sndict = new Dictionary<string, bool>();
                    var sql = s.Replace("<TIMECOND>", vm.StartDate.ToString("yyyyMMddhhmmss")).Replace("<mdcond>", mdcond);


                    var currentroutid = "";
                    var currentroutedsnames = new List<TempDataSetItem>();
                    var pjdatalist = new List<ProjectTestData>();

                    var dbret = DBUtility.ExeATESqlWithRes(sql);

                        foreach (var item in dbret)
                        {
                            try
                            {
                                var status = Convert.ToString(item[3]);
                                var ds = Convert.ToString(item[1]);
                                var temprouteid = Convert.ToString(item[7]);
                                var tempdatasetid = Convert.ToString(item[8]);
                                var jobid = Convert.ToString(item[9]);

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
                                currentroutedsnames.Add(new TempDataSetItem(ds, status,tempdatasetid));
                            }
                            else //if (tempdata != null)
                            {
                                currentroutedsnames.Add(new TempDataSetItem(ds, status,tempdatasetid));
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
                                tempdata.DataID = tempdatasetid;
                                tempdata.JO = jobid;

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


        public static void FindErrAbbr(List<ProjectTestData> pjdatalist, List<TempDataSetItem> currentroutedsnames, Dictionary<string, bool> dsdict)
        {
            var startidx = 0;

            foreach (var pjdata in pjdatalist)
            {
                if (string.Compare(pjdata.ErrAbbr, "PASS", true) == 0)
                {
                    continue;
                }

                bool found = false;
                var maindatasetid = Convert.ToInt64(pjdata.DataID);

                for (var idx = startidx; idx < currentroutedsnames.Count; idx++)
                {
                    if (string.Compare(currentroutedsnames[idx].Status, "PASS", true) != 0
                        && string.Compare(currentroutedsnames[idx].Status, "INFO", true) != 0
                        && !dsdict.ContainsKey(currentroutedsnames[idx].DatasetName))
                    {
                        var subdatasetid = Convert.ToInt64(currentroutedsnames[idx].DatasetID);
                        if (subdatasetid < maindatasetid)
                        {
                            pjdata.ErrAbbr = currentroutedsnames[idx].DatasetName;
                            pjdata.DataID = currentroutedsnames[idx].DatasetID;
                            found = true;
                            
                        }
                    }

                    if (dsdict.ContainsKey(currentroutedsnames[idx].DatasetName))
                    {
                        var nextdatasetid = Convert.ToInt64(currentroutedsnames[idx].DatasetID);
                        if ((nextdatasetid < maindatasetid) && found)
                        {
                            startidx = idx+1;
                            break;
                        }
                    }

                    ////find the stard position of next indivition dataset
                    //for (var tmpidx = startidx; tmpidx < currentroutedsnames.Count; tmpidx++)
                    //{
                    //    if (dsdict.ContainsKey(currentroutedsnames[tmpidx].DatasetName))
                    //    {
                    //        startidx = tmpidx;
                    //        break;
                    //    }
                    //}//end for
                }
            }
        }


        public static void UpdateProjectData(ProjectViewModels vm, string starttime,Controller ctrl)
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


                    var s = "SELECT a.MFR_SN,d.DATASET_NAME,c.FAMILY,d.STATUS,d.END_TIME,d.STATION,a.MFR_PN,d.ROUTE_ID,d.dataset_id,b.JOB_ID FROM PARTS a INNER JOIN ROUTES b ON a.OPT_INDEX = b.PART_INDEX INNER JOIN BOM_CONTEXT_ID c ON c.BOM_CONTEXT_ID = b.BOM_CONTEXT_ID INNER JOIN DATASETS d ON b.ROUTE_ID = d.ROUTE_ID WHERE <mdcond> and d.END_TIME > '<TIMECOND>'  ORDER BY d.ROUTE_ID,d.dataset_id DESC,d.END_TIME DESC";
                    var sndict = new Dictionary<string, bool>();
                    var sql = s.Replace("<TIMECOND>", DateTime.Parse(starttime).ToString("yyyyMMddhhmmss")).Replace("<mdcond>", mdcond);

                    var currentroutid = "";
                    var currentroutedsnames = new List<TempDataSetItem>();
                    var pjdatalist = new List<ProjectTestData>();

                    var dbret = DBUtility.ExeATESqlWithRes(sql);

                    foreach (var item in dbret)
                    {
                        try
                        {
                            var status = Convert.ToString(item[3]);
                            var ds = Convert.ToString(item[1]);
                            var temprouteid = Convert.ToString(item[7]);
                            var tempdatasetid = Convert.ToString(item[8]);
                            var jobid = Convert.ToString(item[9]);

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
                                currentroutedsnames.Add(new TempDataSetItem(ds, status,tempdatasetid));
                            }
                            else //if (tempdata != null)
                            {
                                currentroutedsnames.Add(new TempDataSetItem(ds, status,tempdatasetid));
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
                                tempdata.DataID = tempdatasetid;
                                tempdata.JO = jobid;

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
                            IssueViewModels.CloseIssueAutomaticllyWithFailedSN(item.ProjectKey, item.ModuleSerialNum, item.WhichTest, item.TestStation, item.TestTimeStamp.ToString("yyyy-MM-dd HH:mm:ss"),ctrl);
                        }

                        CreateSystemIssues(failurelist, !string.IsNullOrEmpty(vm.TransferFlg));
                    }

                    if (vm.FinishRating < 90 && DateTime.Parse(starttime) != vm.StartDate)
                    {
                        //use pass sn cover previous failure
                        foreach (var item in passlist)
                        {
                            IssueViewModels.CloseIssueAutomaticlly(item.ProjectKey, item.ModuleSerialNum, item.WhichTest, item.TestStation, item.TestTimeStamp.ToString("yyyy-MM-dd HH:mm:ss"),ctrl);
                        }
                    }

                }// if model list > 0
            }
            catch (Exception ex)
            {
            }

        }

        private static string DataFieldStr(string datasetid)
        {
            var ret = string.Empty;
            var sql = "select data_name,data_val1,data_val2,data_val3 from route_data where dataset_id = '<DataSetID>'";
            sql = sql.Replace("<DataSetID>", datasetid);
            var dbret = DBUtility.ExeATESqlWithRes(sql);
            if (dbret.Count > 0)
            {
                ret = ret + "<hr/><table class='table table-hover'><thead><tr><th>DataSetName</th><th>V1</th><th>V2</th><th>V3</th></tr></thead><tbody>";
            }

            foreach (var line in dbret)
            {
                try
                {
                    var dsnm = Convert.ToString(line[0]);
                    var v1 = Convert.ToString(line[1]);
                    var v2 = string.Empty;
                    if (line[2] != null)
                    {
                        v2 = Convert.ToString(line[2]);
                    }
                    var v3 = string.Empty;
                    if (line[3] != null)
                    {
                        v3 = Convert.ToString(line[3]);
                    }

                    if (v2.ToUpper().Contains("PASS"))
                    {
                        ret = ret + "<tr style='color:green'><td>" + dsnm + "</td><td>"+v1+"</td><td>"+v2+"</td><td>"+v3+"</td></tr>";
                    }
                    else if (v2.ToUpper().Contains("FAIL"))
                    {
                        ret = ret + "<tr style='color:red'><td>" + dsnm + "</td><td>" + v1 + "</td><td>" + v2 + "</td><td>" + v3 + "</td></tr>";
                    }
                    else
                    {
                        ret = ret + "<tr><td>" + dsnm + "</td><td>" + v1 + "</td><td>" + v2 + "</td><td>" + v3 + "</td></tr>";
                    }

                }
                catch (Exception ex) { }
            }

            if (dbret.Count > 0)
            {
                ret = ret + "</tbody></table>";
            }

            return ret;
        }

        private static void CreateSystemIssues(List<ProjectTestData> failurelist, bool transflg = false)
        {
            if (failurelist.Count > 0)
            {
                var pjlist = ProjectViewModels.RetrieveOneProject(failurelist[0].ProjectKey);
                if (pjlist.Count == 0) { return; }
                var pj = pjlist[0];
                var firstengineer = "";
                var role = transflg ? ProjectViewModels.MEROLE : ProjectViewModels.ENGROLE;
                foreach (var m in pj.MemberList)
                {
                    if (string.Compare(m.Role, role) == 0)
                    {
                        firstengineer = m.Name;
                        break;
                    }
                }

                foreach (var item in failurelist)
                {
                    try
                    {
                        var vm = new IssueViewModels();
                        vm.ProjectKey = item.ProjectKey;
                        vm.IssueKey = GetUniqKey(); //item.DataID;
                        vm.IssueType = ISSUETP.Bug;
                        vm.Summary = "Module " + item.ModuleSerialNum + " failed for " + item.ErrAbbr + " @ " + item.WhichTest;
                        vm.Priority = ISSUEPR.Major;
                        vm.DueDate = DateTime.Now.AddDays(7);
                        vm.ReportDate = item.TestTimeStamp;
                        vm.Assignee = firstengineer;
                        vm.Reporter = "System";
                        vm.Resolution = Resolute.Pending;
                        vm.ResolvedDate = DateTime.Parse("1982-05-06 01:01:01");
                        vm.Description = "Module " + item.ModuleSerialNum + " failed for " + item.ErrAbbr + " @ " + item.WhichTest + " on tester " + item.TestStation + " " + item.TestTimeStamp.ToString("yyyy-MM-dd HH:mm:ss")+ DataFieldStr(item.DataID);
                        vm.CommentType = COMMENTTYPE.Description;
                        vm.ModuleSN = item.ModuleSerialNum;
                        vm.ErrAbbr = item.ErrAbbr;
                        vm.DataID = item.DataID;
                        //ProjectEvent.CreateIssueEvent(vm.ProjectKey, "System", vm.Assignee, vm.Summary, vm.IssueKey);
                        vm.StoreIssue();
                        IssueTypeVM.SaveIssueType(vm.IssueKey, ISSUESUBTYPE.Bug.ToString());
                    }
                    catch (Exception ex) { }
                }
            }
        }







    }
}