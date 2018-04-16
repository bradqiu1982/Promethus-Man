using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prometheus.Models
{
    public class VcselPNData
    {
        public VcselPNData()
        {
            PN = "";
            Rate = "";
            Channel = "";
        }

        public static void UpdateVPnInfo(string pn,string rate,string ch)
        {
            var sql = "delete from VcselPNData where PN = @PN";
            var dict = new Dictionary<string, string>();
            dict.Add("@PN", pn);
            DBUtility.ExeLocalSqlNoRes(sql, dict);

            sql = "insert into VcselPNData(PN,Rate,Channel,UpdateTime) values(@PN,@Rate,@Channel,@UpdateTime)";
            dict = new Dictionary<string, string>();
            dict.Add("@PN", pn);
            dict.Add("@Rate", rate);
            dict.Add("@Channel", ch);
            dict.Add("@UpdateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public static Dictionary<string, VcselPNData> RetrieveVcselPNInfo()
        {
            var ret = new Dictionary<string, VcselPNData>();
            var sql = "select PN,Rate,Channel from VcselPNData";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var tempvm = new VcselPNData();
                tempvm.PN = Convert.ToString(line[0]);
                tempvm.Rate = Convert.ToString(line[1]);
                tempvm.Channel = Convert.ToString(line[2]);
                if (!ret.ContainsKey(tempvm.PN))
                {
                    ret.Add(tempvm.PN, tempvm);
                }
            }
            return ret;
        }

        public string PN { set; get; }
        public string Rate { set; get; }
        public string Channel { set; get; }

        //[PN]   NVARCHAR (200) DEFAULT ('') NOT NULL,
	    //[Rate]   NVARCHAR (200) DEFAULT ('') NOT NULL,
	    //[Channel]   NVARCHAR (200) DEFAULT ('') NOT NULL,
    }

    public class VcselMonthData
    {
        public VcselMonthData()
        {
            StartDate = DateTime.Parse("1982-05-06 10:00:00");
            VTYPE = "";
            Failure = "";
            Num = "1";
    }

        public static bool MonthDataExist(DateTime sdate)
        {
            var sql = "select top 1 StartDate from VcselMonthData where StartDate = @StartDate";
            var dict = new Dictionary<string, string>();
            dict.Add("@StartDate", sdate.ToString("yyyy-MM-dd HH:mm:ss"));
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
            if (dbret.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static void StoreMonthData(DateTime sdate,string vtype,string failure,string num) {
            var sql = "insert into VcselMonthData(StartDate,VTYPE,Failure,Num,UpdateTime) values(@StartDate,@VTYPE,@Failure,@Num,@UpdateTime)";
            var dict = new Dictionary<string, string>();
            dict.Add("@StartDate", sdate.ToString("yyyy-MM-dd HH:mm:ss"));
            dict.Add("@VTYPE", vtype);
            dict.Add("@Failure", failure);
            dict.Add("@Num", num);
            dict.Add("@UpdateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        } 

        public static void UpdateMonthData(DateTime sdate, List<BITestResult> mdata , Dictionary<string, VcselPNData> VcselPNInfo)
        {
            var keydict = new Dictionary<string, Dictionary<string, int>>();
            foreach (var line in mdata)
            {
                if (VcselPNInfo.ContainsKey(line.ProductName))
                {
                    var key = line.TestName + "_" + VcselPNInfo[line.ProductName].Rate + "_" + VcselPNInfo[line.ProductName].Channel;
                    if (keydict.ContainsKey(key))
                    {
                        var tempdict = keydict[key];
                        if (tempdict.ContainsKey(line.Failure))
                        {
                            tempdict[line.Failure] = tempdict[line.Failure] + 1;
                        }
                        else
                        {
                            tempdict.Add(line.Failure, 1);
                        }
                    }
                    else
                    {
                        var tempdict = new Dictionary<string, int>();
                        tempdict.Add(line.Failure,1);
                        keydict.Add(key, tempdict);
                    }
                }//check vcsel pn info
            }//loop BI test data

            foreach (var typevaluekv in keydict)
            {
                foreach (var failurekv in typevaluekv.Value)
                {
                    StoreMonthData(sdate, typevaluekv.Key, failurekv.Key, failurekv.Value.ToString());
                }
            }
        }

        public DateTime StartDate { set; get; }
        //TEST_RATE_CHANNEL
        public string VTYPE { set; get; }
        public string Failure { set; get; }
        public string Num { set; get; }
        //[StartDate]   DATETIME       DEFAULT ('1982-05-06 10:00:00') NOT NULL,
        //[VTYPE]   NVARCHAR (200) DEFAULT ('') NOT NULL,
        //[Failure]   NVARCHAR (200) DEFAULT ('') NOT NULL,
        //[Num]   NVARCHAR (126) DEFAULT ('0') NOT NULL,
    }

    public class VcselTimeRange
    {
        public VcselTimeRange()
        {
            WaferNo = "";
            StartDate = DateTime.Parse("1982-05-06 10:00:00");
            EndDate = DateTime.Parse("1982-05-06 10:00:00");
        }

        public static List<VcselTimeRange> RetrieveWafer(string wafer)
        {
            var ret = new List<VcselTimeRange>();
            var sql = "select WaferNo,StartDate,EndDate from VcselTimeRange where WaferNo = @WaferNo";
            var dict = new Dictionary<string, string>();
            dict.Add("@WaferNo", wafer);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
            foreach (var line in dbret)
            {
                var tempvm = new VcselTimeRange();
                tempvm.WaferNo = Convert.ToString(line[0]);
                tempvm.StartDate = Convert.ToDateTime(line[1]);
                tempvm.EndDate = Convert.ToDateTime(line[2]);

                ret.Add(tempvm);
            }
            return ret;
        }

        public static List<string> RetrieveWafer(DateTime starttime,DateTime endtime)
        {
            var ret = new List<string>();
            var sql = "select WaferNo from VcselTimeRange where (EndDate > @S1 and EndDate < @E1) OR (StartDate >= @S2 and EndDate <= @E2) OR (StartDate >= @S3 and StartDate < @E3)";
            var dict = new Dictionary<string, string>();
            dict.Add("@S1", starttime.ToString("yyyy-MM-dd HH:mm:ss"));
            dict.Add("@E1", endtime.ToString("yyyy-MM-dd HH:mm:ss"));
            dict.Add("@S2", starttime.ToString("yyyy-MM-dd HH:mm:ss"));
            dict.Add("@E2", endtime.ToString("yyyy-MM-dd HH:mm:ss"));
            dict.Add("@S3", starttime.ToString("yyyy-MM-dd HH:mm:ss"));
            dict.Add("@E3", endtime.ToString("yyyy-MM-dd HH:mm:ss"));
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString(line[0]));
            }
            return ret;
        }

        private static void AddWafer(DateTime sdate, string wafer)
        {
            var ret = RetrieveWafer(wafer);
            if (ret.Count == 0)
            {
                var sql = "insert into VcselTimeRange(WaferNo,StartDate,EndDate) values(@WaferNo,@StartDate,@EndDate)";
                var dict = new Dictionary<string, string>();
                dict.Add("@WaferNo", wafer);
                dict.Add("@EndDate", sdate.AddMonths(1).ToString("yyyy-MM-dd HH:mm:ss"));
                dict.Add("@StartDate", sdate.ToString("yyyy-MM-dd HH:mm:ss"));
                DBUtility.ExeLocalSqlNoRes(sql, dict);
            }
            else
            {
                var sql = "";
                var dict = new Dictionary<string, string>();
                dict.Add("@WaferNo", wafer);
                if (sdate.AddMonths(1) > ret[0].EndDate)
                {
                    sql = "update VcselTimeRange set EndDate = @EndDate where WaferNo = @WaferNo";
                    dict.Add("@EndDate",sdate.AddMonths(1).ToString("yyyy-MM-dd HH:mm:ss"));
                }
                if (sdate < ret[0].StartDate)
                {
                    sql = "update VcselTimeRange set StartDate = @StartDate where WaferNo = @WaferNo";
                    dict.Add("@StartDate", sdate.ToString("yyyy-MM-dd HH:mm:ss"));
                }
                DBUtility.ExeLocalSqlNoRes(sql, dict);
            }//end else
        }

        public static void UpateWaferTimeRange(DateTime sdate, List<BITestResult> mdata)
        {
            var waferdict = new Dictionary<string, bool>();
            foreach (var data in mdata)
            {
                if (!waferdict.ContainsKey(data.Wafer))
                {
                    waferdict.Add(data.Wafer, true);
                }
            }

            foreach (var kv in waferdict)
            {
                AddWafer(sdate, kv.Key);
            }
        }

        public string WaferNo { set; get; }
        public DateTime StartDate { set; get; }
        public DateTime EndDate { set; get; }

        //[WaferNo]   NVARCHAR (200) DEFAULT ('') NOT NULL,
        //[StartDate]   DATETIME       DEFAULT ('1982-05-06 10:00:00') NOT NULL,
        //[EndDate]   DATETIME       DEFAULT ('1982-05-06 10:00:00') NOT NULL,
    }

    public class WaferTestSum
    {
        public WaferTestSum()
        {
            WaferNo = "";
            WhichTest = "";
            VTYPE = "";
            Failure = "";
            Num = "1";
        }

        private static void StoreWaferData(string waferno, string whichtest, string vtype, string failure, string num,string dataids)
        {
            var sql = "insert into WaferTestSum(WaferNo,WhichTest,VTYPE,Failure,Num,UpdateTime,DataIDs) values(@WaferNo,@WhichTest,@VTYPE,@Failure,@Num,@UpdateTime,@DataIDs)";
            var dict = new Dictionary<string, string>();
            dict.Add("@WaferNo",waferno);
            dict.Add("@WhichTest",whichtest);
            dict.Add("@VTYPE",vtype);
            dict.Add("@Failure",failure);
            dict.Add("@Num",num);
            dict.Add("@UpdateTime",DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            dict.Add("@DataIDs", dataids);

            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        private static void CleanWaferData(string waferno)
        {
            var sql = "delete from WaferTestSum where WaferNo = @WaferNo";
            var dict = new Dictionary<string, string>();
            dict.Add("@WaferNo", waferno);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public static void UpdateWaferTestData(string wafer, List<BITestResult> waferdata, Dictionary<string, VcselPNData> VcselPNInfo)
        {
            var keydict = new Dictionary<string, Dictionary<string, int>>();
            foreach (var line in waferdata)
            {
                    var key = line.TestName;
                    if (keydict.ContainsKey(key))
                    {
                        var tempdict = keydict[key];
                        if (tempdict.ContainsKey(line.Failure))
                        {
                            tempdict[line.Failure] = tempdict[line.Failure] + 1;
                        }
                        else
                        {
                            tempdict.Add(line.Failure, 1);
                        }
                    }
                    else
                    {
                        var tempdict = new Dictionary<string, int>();
                        tempdict.Add(line.Failure, 1);
                        keydict.Add(key, tempdict);
                    }
            }//loop BI test data

            var vtype = "";
            if (VcselPNInfo.ContainsKey(waferdata[0].ProductName))
            {
                vtype =  VcselPNInfo[waferdata[0].ProductName].Rate + "_" + VcselPNInfo[waferdata[0].ProductName].Channel;
            }

            var dataids = "('";
            foreach (var line in waferdata)
            {
                dataids = dataids + line.DataID + "','";
            }
            dataids = dataids.Substring(0, dataids.Length - 2) + ")";

            CleanWaferData(wafer);

            foreach (var typevaluekv in keydict)
            {
                foreach (var failurekv in typevaluekv.Value)
                {
                    StoreWaferData(wafer,typevaluekv.Key,vtype, failurekv.Key, failurekv.Value.ToString(),dataids);
                }
            }
        }

        public string WaferNo { set; get; }
        public string WhichTest { set; get; }
        //RATE_CHANNEL
        public string VTYPE { set; get; }
        public string Failure { set; get; }
        public string Num { set; get; }
        public string DataIDs { set; get; }
        //[WaferNo]    NVARCHAR (200) DEFAULT ('') NOT NULL,
        //[WhichTest]    NVARCHAR (200) DEFAULT ('') NOT NULL,
        //[Failure]   NVARCHAR (200) DEFAULT ('') NOT NULL,
        //[Num]   NVARCHAR (126) DEFAULT ('0') NOT NULL,
    }

    public class CBOXData
    {
        public CBOXData()
        {
            mean = 0;
            min = 0;
            max = 0;
            lower = 0;
            upper = 0;
        }
        public double mean { set; get; }
        public double min { set; get; }
        public double max { set; get; }
        public double lower { set; get; }
        public double upper { set; get; }
    }

    public class WaferBGDFiled
    {
        public WaferBGDFiled()
        {
            WaferNo = "";
            FieldName = "";
            BoxData = "";
        }


        private static List<double> RetrieveBIFieldData(string fieldname, string idcond, Controller ctrl)
        {
            var glbcfg = CfgUtility.GetSysConfig(ctrl);
            bool limitexist = false;
            var lowlimit = 0.0;
            var highlimit = 0.0;
            if (glbcfg.ContainsKey(fieldname + "_LIMIT"))
            {
                limitexist = true;
                lowlimit = Convert.ToDouble(glbcfg[fieldname + "_LIMIT"].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[0]);
                highlimit = Convert.ToDouble(glbcfg[fieldname + "_LIMIT"].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1]);
            }

            var rawdata = new List<double>();
            var sql = "select <fieldname> from BITestResultDataField where <fieldname> <> 0 and DataID in <idcond>";
            sql = sql.Replace("<fieldname>", fieldname).Replace("<idcond>", idcond);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var val = Convert.ToDouble(line[0]);
                if (limitexist)
                {
                    if (val >= lowlimit && val <= highlimit)
                    {
                        rawdata.Add(val);
                    }
                }
                else
                {
                    rawdata.Add(val);
                }
            }
            return rawdata;
        }

        private static Dictionary<string,List<double>> RetrieveBIFieldDataSNDict(string fieldname, string idcond, Controller ctrl)
        {
            var glbcfg = CfgUtility.GetSysConfig(ctrl);
            bool limitexist = false;
            var lowlimit = 0.0;
            var highlimit = 0.0;
            if (glbcfg.ContainsKey(fieldname + "_LIMIT"))
            {
                limitexist = true;
                lowlimit = Convert.ToDouble(glbcfg[fieldname + "_LIMIT"].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[0]);
                highlimit = Convert.ToDouble(glbcfg[fieldname + "_LIMIT"].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1]);
            }

            var rawdata = new Dictionary<string, List<double>>();
            var sql = "select <fieldname>,SN from BITestResultDataField where <fieldname> <> 0 and DataID in <idcond>";
            sql = sql.Replace("<fieldname>", fieldname).Replace("<idcond>", idcond);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var val = Convert.ToDouble(line[0]);
                var sn = Convert.ToString(line[1]);

                var adddata = false;
                if (limitexist)
                {
                    if (val >= lowlimit && val <= highlimit)
                    {
                        adddata = true;
                    }
                }
                else
                {
                    adddata = true;
                }

                if (adddata)
                {
                    if (rawdata.ContainsKey(sn))
                    {
                        rawdata[sn].Add(val);
                    }
                    else
                    {
                        var templist = new List<double>();
                        templist.Add(val);
                        rawdata.Add(sn, templist);
                    }
                }
            }
            return rawdata;
        }

        private static double GetBoxMeanValue(List<double> rawdata)
        {
            if ((rawdata.Count % 2) == 0)
            {
                var mididx1 = rawdata.Count / 2;
                var mididx2 = mididx1 + 1;
                return (rawdata[mididx1] + rawdata[mididx2]) / 2.0;
            }
            else
            {
                var mididx = (rawdata.Count + 1) / 2;
                return rawdata[mididx];
            }
        }

        private static string GetBoxData(List<double> rawdata)
        {
            rawdata.Sort();

            if ((rawdata.Count % 2) == 0)
            {
                var mididx1 = rawdata.Count / 2;
                var mididx2 = mididx1 + 1;

                var mean = (rawdata[mididx1] + rawdata[mididx2]) / 2.0;
                var min = rawdata[0];
                var max = rawdata[rawdata.Count - 1];

                var lowlist = new List<double>();
                var upperlist = new List<double>();
                for (var idx = 0; idx < mididx2; idx++)
                {
                    lowlist.Add(rawdata[idx]);
                }
                for (var idx = mididx2; idx < rawdata.Count; idx++)
                {
                    upperlist.Add(rawdata[idx]);
                }

                var lower = GetBoxMeanValue(lowlist);
                var upper = GetBoxMeanValue(upperlist);

                object ret = new { mean = mean, min = min,max = max,lower= lower,upper = upper };
                return Newtonsoft.Json.JsonConvert.SerializeObject(ret);
            }
            else
            {
                var mididx = (rawdata.Count + 1) / 2;
                var mean = rawdata[mididx];
                var min = rawdata[0];
                var max = rawdata[rawdata.Count - 1];

                var lowlist = new List<double>();
                var upperlist = new List<double>();
                for (var idx = 0; idx < mididx; idx++)
                {
                    lowlist.Add(rawdata[idx]);
                }
                for (var idx = mididx + 1; idx < rawdata.Count; idx++)
                {
                    upperlist.Add(rawdata[idx]);
                }

                var lower = GetBoxMeanValue(lowlist);
                var upper = GetBoxMeanValue(upperlist);

                object ret = new { mean = mean, min = min, max = max, lower = lower, upper = upper };
                return Newtonsoft.Json.JsonConvert.SerializeObject(ret);
            }

        }

        private static void SolveNormalField(string wafer, string fieldname,string idcond, Controller ctrl)
        {
            var rawdata = RetrieveBIFieldData(fieldname, idcond, ctrl);
            if (rawdata.Count < 200) {
                return;
            }
            var boxdata = GetBoxData(rawdata);
            AddData(wafer, fieldname, boxdata);
        }

        private static void SolveComputerField(string wafer, string fieldname, string idcond, Controller ctrl)
        {
            var rawdata = RetrieveBIFieldDataSNDict(fieldname, idcond, ctrl);
            if (rawdata.Count < 40)
            {
                return;
            }

            var variatelist = new List<double>();
            var uniformlist = new List<double>();
            foreach (var kv in rawdata)
            {
                var v = kv.Value.Max() - kv.Value.Min();
                var av = kv.Value.Average();
                if (av != 0)
                {
                    var u = v / av;
                    variatelist.Add(v);
                    uniformlist.Add(u);
                }
            }

            var vboxdata = GetBoxData(variatelist);
            var uboxdata = GetBoxData(uniformlist);
            AddData(wafer, "Variation_" + fieldname, vboxdata);
            AddData(wafer, "Uniformity_" + fieldname, uboxdata);
        }

        public static void UpdateWaferFields(string wafer, List<BITestResult> waferdata, Controller ctrl)
        {
            var idcond = "('";
            foreach (var line in waferdata)
            {
                idcond = idcond + line.DataID + "','";
            }
            idcond = idcond.Substring(0, idcond.Length - 2)+")";

            SolveNormalField(wafer,"Delta_PO_LD",idcond,ctrl);
            SolveNormalField(wafer, "Delta_PO_Uniformity",idcond,ctrl);
            SolveComputerField(wafer, "Delta_PO_LD",idcond,ctrl);
        }

        private static void CleanWaferData(string waferno,string fieldname)
        {
            var sql = "delete from WaferBGDFiled where WaferNo = @WaferNo and FieldName = @FieldName";
            var dict = new Dictionary<string, string>();
            dict.Add("@WaferNo", waferno);
            dict.Add("@FieldName", fieldname);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        private static void AddData(string wafer,string fieldname,string boxdata)
        {
            CleanWaferData(wafer, fieldname);
            var sql = "insert into WaferBGDFiled(WaferNo,FieldName,BoxData,UpdateTime) values(@WaferNo,@FieldName,@BoxData,@UpdateTime)";
            var dict = new Dictionary<string, string>();
            dict.Add("@WaferNo",wafer);
            dict.Add("@FieldName",fieldname);
            dict.Add("@BoxData",boxdata);
            dict.Add("@UpdateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public string WaferNo { set; get; }
        public string FieldName { set; get; }
        public string BoxData { set; get; }
    }

    public class VcselBGDVM
    {
        private static List<BITestResult> RetrieveBITestData(DateTime startdate, DateTime enddate)
        {
            var ret = new List<BITestResult>();

            var sql = "select SN,TestName,Failure,Wafer,ProductName,DataID from BITestResult where TestTimeStamp >= @startdate and TestTimeStamp < @enddate and ProductName <> '' and Wafer <> '' order by TestTimeStamp desc";
            var dict = new Dictionary<string, string>();
            dict.Add("@startdate", startdate.ToString("yyyy-MM-dd HH:mm:ss"));
            dict.Add("@enddate", enddate.ToString("yyyy-MM-dd HH:mm:ss"));
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
            var dupdict = new Dictionary<string, bool>();
            foreach (var line in dbret)
            {
                var SN = Convert.ToString(line[0]);
                var TestName = Convert.ToString(line[1]);
                if (!dupdict.ContainsKey(SN + "-" + TestName))
                {
                    dupdict.Add(SN + "-" + TestName, true);

                    var tempvm = new BITestResult();
                    tempvm.SN = SN;
                    tempvm.TestName = TestName;
                    tempvm.Failure = Convert.ToString(line[2]).Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries)[0];
                    tempvm.Wafer = Convert.ToString(line[3]);
                    tempvm.ProductName = Convert.ToString(line[4]);
                    tempvm.DataID = Convert.ToString(line[5]);
                    ret.Add(tempvm);
                }
            }//end foreach
            return ret;
        }

        private static List<BITestResult> RetrieveBITestData(string wafer)
        {
            var ret = new List<BITestResult>();

            var sql = "select SN,TestName,Failure,Wafer,ProductName,DataID from BITestResult where ProductName <> '' and Wafer = @Wafer order by TestTimeStamp desc";
            var dict = new Dictionary<string, string>();
            dict.Add("@Wafer", wafer);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
            var dupdict = new Dictionary<string, bool>();
            foreach (var line in dbret)
            {
                var SN = Convert.ToString(line[0]);
                var TestName = Convert.ToString(line[1]);
                if (!dupdict.ContainsKey(SN + "-" + TestName))
                {
                    dupdict.Add(SN + "-" + TestName, true);

                    var tempvm = new BITestResult();
                    tempvm.SN = SN;
                    tempvm.TestName = TestName;
                    tempvm.Failure = Convert.ToString(line[2]).Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries)[0];
                    tempvm.Wafer = Convert.ToString(line[3]);
                    tempvm.ProductName = Convert.ToString(line[4]);
                    tempvm.DataID = Convert.ToString(line[5]);
                    ret.Add(tempvm);
                }
            }//end foreach
            return ret;
        }

        private static void SolveDataByWafer(Dictionary<string, bool> waferdict, Dictionary<string, VcselPNData> VcselPNInfo, Controller ctrl)
        {
            foreach (var kv in waferdict)
            {
                var wafertestdata = RetrieveBITestData(kv.Key);
                if (wafertestdata.Count > 200)
                {
                    WaferTestSum.UpdateWaferTestData(kv.Key, wafertestdata, VcselPNInfo);
                    WaferBGDFiled.UpdateWaferFields(kv.Key, wafertestdata,ctrl);
                }
            }
        }

        public static void StartVcselBGDComputer(DateTime StartDate, Dictionary<string, VcselPNData> VcselPNInfo,Controller ctrl)
        {
            var EndDate = StartDate.AddMonths(1);
            var monthlytestdata = RetrieveBITestData(StartDate, EndDate);
            VcselMonthData.UpdateMonthData(StartDate, monthlytestdata, VcselPNInfo);
            VcselTimeRange.UpateWaferTimeRange(StartDate, monthlytestdata);

            var waferdict = new Dictionary<string, bool>();
            foreach (var data in monthlytestdata)
            {
                if (!waferdict.ContainsKey(data.Wafer))
                {
                    waferdict.Add(data.Wafer, true);
                }
            }
            SolveDataByWafer(waferdict, VcselPNInfo,ctrl);
        }


        public static void TestVcselBGDComputer(DateTime StartDate, Dictionary<string, VcselPNData> VcselPNInfo, Controller ctrl)
        {
            var EndDate = StartDate.AddDays(7);
            var monthlytestdata = RetrieveBITestData(StartDate, EndDate);
            VcselMonthData.UpdateMonthData(StartDate, monthlytestdata, VcselPNInfo);
            VcselTimeRange.UpateWaferTimeRange(StartDate, monthlytestdata);

            var waferdict = new Dictionary<string, bool>();
            foreach (var data in monthlytestdata)
            {
                if (!waferdict.ContainsKey(data.Wafer))
                {
                    waferdict.Add(data.Wafer, true);
                }
            }
            SolveDataByWafer(waferdict, VcselPNInfo, ctrl);
        }

    }

}