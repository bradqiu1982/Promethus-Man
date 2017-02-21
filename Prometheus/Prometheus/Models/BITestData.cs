using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prometheus.Models
{
    public class BITestDataField
    {
        public string DataID { set; get; }
        public string ChannelNum { set; get; }
        public double SLOPE { set; get; }
        public double THOLD { set; get; }
        public double PO_LD { set; get; }
        public double PO_LD_18 { set; get; }
        public double PO_LD_25 { set; get; }
        public double PO_LD_127 { set; get; }
        public double PO_Uniformity { set; get; }
        public double Delta_SLOPE { set; get; }
        public double Delta_THOLD { set; get; }
        public double Delta_PO_LD { set; get; }
        public double Delta_PO_LD_18 { set; get; }
        public double Delta_PO_LD_25 { set; get; }
        public double Delta_PO_LD_127 { set; get; }
        public double Delta_PO_Uniformity { set; get; }

        public void StoreBIDateField()
        {
            var isql = "insert into BITestDataField values('<DataID>','<ChannelNum>',<SLOPE>,<THOLD>,<PO_LD>,<PO_LD_18>,<PO_LD_25>,<PO_LD_127>,<PO_Uniformity>,<Delta_SLOPE>,<Delta_THOLD>,<Delta_PO_LD>,<Delta_PO_LD_18>,<Delta_PO_LD_25>,<Delta_PO_LD_127>,<Delta_PO_Uniformity>)";
            isql = isql.Replace("<DataID>", DataID).Replace("<ChannelNum>", ChannelNum).Replace("<SLOPE>", SLOPE.ToString()).Replace("<THOLD>", THOLD.ToString())
                .Replace("<PO_LD>", PO_LD.ToString()).Replace("<PO_LD_18>", PO_LD_18.ToString()).Replace("<PO_LD_25>", PO_LD_25.ToString()).Replace("<PO_LD_127>", PO_LD_127.ToString())
                .Replace("<PO_Uniformity>", PO_Uniformity.ToString()).Replace("<Delta_SLOPE>", Delta_SLOPE.ToString()).Replace("<Delta_THOLD>", Delta_THOLD.ToString()).Replace("<Delta_PO_LD>", Delta_PO_LD.ToString())
                .Replace("<Delta_PO_LD_18>", Delta_PO_LD_18.ToString()).Replace("<Delta_PO_LD_25>", Delta_PO_LD_25.ToString()).Replace("<Delta_PO_LD_127>", Delta_PO_LD_127.ToString()).Replace("<Delta_PO_Uniformity>", Delta_PO_Uniformity.ToString());
            DBUtility.ExeLocalSqlNoRes(isql);
        }
    }

    public class BITestData
    {
        public BITestData() { }

        public BITestData(string pk, string did, string sn, string wtest, string mt, string err, string testtime, string station, string p,string wf,string wfp)
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
            Wafer = wf;
            Waferpn = wfp;
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
        public string Wafer { set; get; }
        public string Waferpn { set; get; }

        private List<BITestDataField> df = new List<BITestDataField>();
        public List<BITestDataField> DataFields {
            get
            {
                return df;
            }}


        public void StoreBIData()
        {
            var isql = "insert into BITestData(ProjectKey,DataID,ModuleSerialNum,WhichTest,ModuleType,ErrAbbr,TestTimeStamp,TestStation,PN,UpdateTime,Wafer,Waferpn) values('<ProjectKey>','<DataID>','<ModuleSerialNum>','<WhichTest>','<ModuleType>','<ErrAbbr>','<TestTimeStamp>','<TestStation>','<PN>','<UpdateTime>','<Wafer>','<Waferpn>')";
            isql = isql.Replace("<ProjectKey>", ProjectKey).Replace("<DataID>", DataID).Replace("<ModuleSerialNum>", ModuleSerialNum)
                .Replace("<WhichTest>", WhichTest).Replace("<ModuleType>", ModuleType).Replace("<ErrAbbr>", ErrAbbr)
                .Replace("<TestTimeStamp>", TestTimeStamp.ToString()).Replace("<TestStation>", TestStation).Replace("<PN>", PN)
                .Replace("<UpdateTime>", DateTime.Now.ToString()).Replace("<Wafer>", Wafer).Replace("<Waferpn>", Waferpn);
            DBUtility.ExeLocalSqlNoRes(isql);

            //foreach (var item in DataFields)
            //{
            //    item.StoreBIDateField();
            //}
        }

        public static Dictionary<string, DateTime> RetrieveAllDataID(string projectkey)
        {
            var ret = new Dictionary<string, DateTime>();
            var sql = "select ModuleSerialNum,WhichTest,TestTimeStamp from BITestData where ProjectKey = '<ProjectKey>' order by TestTimeStamp DESC";
            sql = sql.Replace("<ProjectKey>", projectkey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            foreach (var item in dbret)
            {
                try
                {
                    if (!ret.ContainsKey(Convert.ToString(item[0]) + "_" + Convert.ToString(item[1])))
                    {
                        ret.Add(Convert.ToString(item[0])+"_"+ Convert.ToString(item[1]), DateTime.Parse(Convert.ToString(item[2])));
                    }
                }
                catch(Exception ex) { }
           }
            return ret;
        }

        public static Dictionary<string, DateTime> RetrieveAllDataIDASC(string projectkey)
        {
            var ret = new Dictionary<string, DateTime>();
            var sql = "select ModuleSerialNum,WhichTest,TestTimeStamp from BITestData where ProjectKey = '<ProjectKey>' order by TestTimeStamp ASC";
            sql = sql.Replace("<ProjectKey>", projectkey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            foreach (var item in dbret)
            {
                try
                {
                    if (!ret.ContainsKey(Convert.ToString(item[0]) + "_" + Convert.ToString(item[1])))
                    {
                        ret.Add(Convert.ToString(item[0]) + "_" + Convert.ToString(item[1]), DateTime.Parse(Convert.ToString(item[2])));
                    }
                }
                catch (Exception ex) { }
            }
            return ret;
        }

        public static string RetrieveLatestTimeOfLocalBI(string projectkey)
        {
            var sql = "select top 1 TestTimeStamp from BITestData where ProjectKey = '<ProjectKey>' order by TestTimeStamp DESC";
            sql = sql.Replace("<ProjectKey>", projectkey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            if (dbret.Count > 0)
            {
                return Convert.ToString(dbret[0][0]);
            }
            else
                return null;
        }

        public static Dictionary<string, bool> RetrieveSNBeforeDate(string projectkey, string edate)
        {
            var ret = new Dictionary<string, bool>();
            var sql = "select ModuleSerialNum,WhichTest from BITestData where ProjectKey = '<ProjectKey>' and TestTimeStamp < '<ENDDATE>'";
            sql = sql.Replace("<ProjectKey>", projectkey).Replace("<ENDDATE>", edate);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            foreach (var item in dbret)
            {
                var key = Convert.ToString(item[0]);
                var key2 = Convert.ToString(item[1]);
                if (!ret.ContainsKey(key+"-"+key2))
                {
                    ret.Add(key + "-" + key2, true);
                }
            }
            return ret;
        }

        public static List<BITestData> RetrieveProjectTestData(string projectkey, string startdate, string enddate, bool firstyield)
        {
            var ret = new List<BITestData>();
            var sql = "";
            if (firstyield)
            {
                sql = "select ProjectKey,DataID,ModuleSerialNum,WhichTest,ModuleType,ErrAbbr,TestTimeStamp,TestStation,PN,Wafer,Waferpn from BITestData where ProjectKey = '<ProjectKey>' and TestTimeStamp > '<StartDate>' and TestTimeStamp < '<EndDate>' order by ModuleSerialNum,TestTimeStamp ASC";
            }
            else
            {
                sql = "select ProjectKey,DataID,ModuleSerialNum,WhichTest,ModuleType,ErrAbbr,TestTimeStamp,TestStation,PN,Wafer,Waferpn from BITestData where ProjectKey = '<ProjectKey>' and TestTimeStamp > '<StartDate>' and TestTimeStamp < '<EndDate>' order by ModuleSerialNum,TestTimeStamp DESC";
            }

            sql = sql.Replace("<ProjectKey>", projectkey).Replace("<StartDate>", startdate).Replace("<EndDate>", enddate);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            foreach (var item in dbret)
            {
                var tempdata = new BITestData(Convert.ToString(item[0]), Convert.ToString(item[1]), Convert.ToString(item[2])
                    , Convert.ToString(item[3]), Convert.ToString(item[4]), Convert.ToString(item[5])
                    , Convert.ToString(item[6]), Convert.ToString(item[7]), Convert.ToString(item[8]), Convert.ToString(item[9]), Convert.ToString(item[10]));
                ret.Add(tempdata);
            }
            return ret;
        }

        public static List<BITestData> RetrieveProjectTestDataByWafer(string pjkey, string wafer)
        {

            var ret = new List<BITestData>();
            var sql = "select ProjectKey,DataID,ModuleSerialNum,WhichTest,ModuleType,ErrAbbr,TestTimeStamp,TestStation,PN,Wafer,Waferpn from BITestData where ProjectKey = '<ProjectKey>' and Wafer = '<Wafer>' order by ModuleSerialNum,TestTimeStamp DESC";

            sql = sql.Replace("<ProjectKey>", pjkey).Replace("<Wafer>", wafer);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            foreach (var item in dbret)
            {
                var tempdata = new BITestData(Convert.ToString(item[0]), Convert.ToString(item[1]), Convert.ToString(item[2])
                    , Convert.ToString(item[3]), Convert.ToString(item[4]), Convert.ToString(item[5])
                    , Convert.ToString(item[6]), Convert.ToString(item[7]), Convert.ToString(item[8]), Convert.ToString(item[9]), Convert.ToString(item[10]));
                ret.Add(tempdata);
            }
            return ret;
        }

        public static List<ProjectTestData> RetrieveProjectTestDataByDataID(string DataID)
        {

            var ret = new List<ProjectTestData>();
            var sql = "select ProjectKey,DataID,ModuleSerialNum,WhichTest,ModuleType,ErrAbbr,TestTimeStamp,TestStation,PN,Wafer,Waferpn from BITestData where DataID = '<DataID>'";

            sql = sql.Replace("<DataID>", DataID);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
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
            //ProjectTestData.PrePareLatestData(ProjectKey);

            var ret = new List<ProjectTestData>();
            var sql = "select top <topnum> ProjectKey,DataID,ModuleSerialNum,WhichTest,ModuleType,ErrAbbr,TestTimeStamp,TestStation,PN from BITestData where ProjectKey = '<ProjectKey>' and ErrAbbr = '<ErrAbbr>' order by TestTimeStamp DESC";
            sql = sql.Replace("<ProjectKey>", ProjectKey).Replace("<topnum>", Convert.ToString(topnum)).Replace("<ErrAbbr>", ErrAbbr);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
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
            var sql = "select top <topnum> ProjectKey,DataID,ModuleSerialNum,WhichTest,ModuleType,ErrAbbr,TestTimeStamp,TestStation,PN from BITestData where ProjectKey = '<ProjectKey>' and ModuleSerialNum = '<ModuleSerialNum>' order by TestTimeStamp DESC";
            sql = sql.Replace("<ProjectKey>", ProjectKey).Replace("<topnum>", Convert.ToString(topnum)).Replace("<ModuleSerialNum>", SN);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            foreach (var item in dbret)
            {
                var tempdata = new ProjectTestData(Convert.ToString(item[0]), Convert.ToString(item[1]), Convert.ToString(item[2])
                    , Convert.ToString(item[3]), Convert.ToString(item[4]), Convert.ToString(item[5])
                    , Convert.ToString(item[6]), Convert.ToString(item[7]), Convert.ToString(item[8]));
                ret.Add(tempdata);
            }
            return ret;
        }

        public static List<string> RetrieveAllWafer(string projectkey)
        {
            var ret = new List<string>();
            var sql = "select DISTINCT Wafer from BITestData where ProjectKey = '<ProjectKey>' order by Wafer";
            sql = sql.Replace("<ProjectKey>", projectkey);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            foreach (var item in dbret)
            {
                ret.Add(Convert.ToString(item[0]));
            }
            return ret;
        }

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

        public static void PrePareLatestData(Controller ctrl, string projectkey)
        {
            if (UpdatePJLockUsing(projectkey))
                return;

            try
            {
                var vm = ProjectViewModels.RetrieveOneProject(projectkey);
                if (vm.PNList.Count > 0)
                {
                    string lastupdatetime = BITestData.RetrieveLatestTimeOfLocalBI(projectkey);
                    if (!string.IsNullOrEmpty(lastupdatetime))
                    {
                        var vlast = DateTime.Parse(lastupdatetime);
                        vlast = vlast.AddMinutes(18);
                        if (vlast < DateTime.Now)
                        {
                            BIDataUtility.UpdateProjectData(ctrl,vm);
                        }
                    }
                    else
                    {
                        BIDataUtility.UpdateProjectData(ctrl,vm);
                    }
                }

                ResetUpdatePJLock(projectkey);
            }
            catch (Exception ex)
            {
                ResetUpdatePJLock(projectkey);
            }
        }

        public static void RetrieveWaferDataFromMes (Controller ctrl, string projectkey)
        {
            if (UpdatePJLockUsing(projectkey))
                return;

            try
            {
                var vm = ProjectViewModels.RetrieveOneProject(projectkey);
                BIDataUtility.RetrievePjWaferAllData(ctrl,vm);
                ResetUpdatePJLock(projectkey);
            }
            catch (Exception ex)
            {
                ResetUpdatePJLock(projectkey);
            }
        }

    }
}