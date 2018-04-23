using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public static void UpdateVPnInfo(string pn, string rate, string ch)
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

        private static void StoreMonthData(DateTime sdate, string vtype, string failure, string num) {
            var sql = "insert into VcselMonthData(StartDate,VTYPE,Failure,Num,UpdateTime) values(@StartDate,@VTYPE,@Failure,@Num,@UpdateTime)";
            var dict = new Dictionary<string, string>();
            dict.Add("@StartDate", sdate.ToString("yyyy-MM-dd HH:mm:ss"));
            dict.Add("@VTYPE", vtype);
            dict.Add("@Failure", failure);
            dict.Add("@Num", num);
            dict.Add("@UpdateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public static void UpdateMonthData(DateTime sdate, List<BITestResult> mdata, Dictionary<string, VcselPNData> VcselPNInfo)
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
                        tempdict.Add(line.Failure, 1);
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

        public static Dictionary<string, Dictionary<string, Dictionary<string, int>>> RetrieveMonthlyData(DateTime startdate, DateTime enddate, string vtype)
        {
            var ret = new Dictionary<string, Dictionary<string, Dictionary<string, int>>>();

            var sql = "select StartDate,VTYPE,Failure,Num from VcselMonthData where VTYPE like @VTYPE and StartDate >= @sdate and StartDate < @edate";
            var dict = new Dictionary<string, string>();
            dict.Add("@VTYPE", "%_" + vtype);
            dict.Add("@sdate", startdate.ToString("yyyy-MM-dd HH:mm:ss"));
            dict.Add("@edate", enddate.ToString("yyyy-MM-dd HH:mm:ss"));
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
            foreach (var line in dbret)
            {
                var sdate = Convert.ToDateTime(line[0]).ToString("yyyy-MM-dd");
                var testtype = Convert.ToString(line[1]).Replace("_" + vtype, "");
                var failure = Convert.ToString(line[2]);
                var num = Convert.ToInt32(line[3]);

                if (ret.ContainsKey(testtype))
                {
                    if (ret[testtype].ContainsKey(sdate))
                    {
                        if (!ret[testtype][sdate].ContainsKey(failure))
                        {
                            ret[testtype][sdate].Add(failure, num);
                        }
                    }
                    else
                    {
                        var sub2dict = new Dictionary<string, int>();
                        sub2dict.Add(failure, num);
                        ret[testtype].Add(sdate, sub2dict);
                    }
                }
                else
                {
                    var sub1dict = new Dictionary<string, Dictionary<string, int>>();
                    var sub2dict = new Dictionary<string, int>();
                    sub2dict.Add(failure, num);
                    sub1dict.Add(sdate, sub2dict);
                    ret.Add(testtype, sub1dict);
                }
            }

            return ret;
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

        public static List<string> RetrieveWafer(DateTime starttime, DateTime endtime)
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
                    dict.Add("@EndDate", sdate.AddMonths(1).ToString("yyyy-MM-dd HH:mm:ss"));
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

        public static List<string> WaferNOList()
        {
            var ret = new List<string>();
            var sql = "select distinct WaferNo from VcselTimeRange";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            { ret.Add(Convert.ToString(line[0])); }
            ret.Sort();
            return ret;
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

        private static void StoreWaferData(string waferno, string whichtest, string vtype, string failure, string num, string dataids)
        {
            var sql = "insert into WaferTestSum(WaferNo,WhichTest,VTYPE,Failure,Num,UpdateTime,DataIDs) values(@WaferNo,@WhichTest,@VTYPE,@Failure,@Num,@UpdateTime,@DataIDs)";
            var dict = new Dictionary<string, string>();
            dict.Add("@WaferNo", waferno);
            dict.Add("@WhichTest", whichtest);
            dict.Add("@VTYPE", vtype);
            dict.Add("@Failure", failure);
            dict.Add("@Num", num);
            dict.Add("@UpdateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
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

        public static string DataIDCond(string waferno)
        {
            var sql = "select top 1 DataIDs from WaferTestSum where WaferNo = @WaferNo";
            var dict = new Dictionary<string, string>();
            dict.Add("@WaferNo", waferno);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
            foreach (var line in dbret)
            {
                return Convert.ToString(line[0]);
            }
            return string.Empty;
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
                vtype = VcselPNInfo[waferdata[0].ProductName].Rate + "_" + VcselPNInfo[waferdata[0].ProductName].Channel;
            }

            StringBuilder sb = new StringBuilder(36 * (waferdata.Count + 1));
            sb.Append("('");
            foreach (var line in waferdata)
            {
                sb.Append(line.DataID + "','");
            }
            var tempstr = sb.ToString();
            var dataids = tempstr.Substring(0, tempstr.Length - 2) + ")";

            //var dataids = "('";
            //foreach (var line in waferdata)
            //{
            //    dataids = dataids + line.DataID + "','";
            //}
            //dataids = dataids.Substring(0, dataids.Length - 2) + ")";

            CleanWaferData(wafer);

            foreach (var typevaluekv in keydict)
            {
                foreach (var failurekv in typevaluekv.Value)
                {
                    StoreWaferData(wafer, typevaluekv.Key, vtype, failurekv.Key, failurekv.Value.ToString(), dataids);
                }
            }
        }

        public static List<string> VcselTypeList()
        {
            var ret = new List<string>();
            var sql = "select distinct VTYPE from WaferTestSum";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            { ret.Add(Convert.ToString(line[0])); }
            ret.Sort();
            return ret;
        }

        public static List<string> VcselTestList()
        {
            var ret = new List<string>();
            var sql = "select distinct WhichTest from WaferTestSum";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            { ret.Add(Convert.ToString(line[0])); }
            ret.Sort();
            return ret;
        }

        public static Dictionary<string, Dictionary<string, Dictionary<string,int>>> RetriveWaferFailure(List<string> wflist,string vtype)
        {
            var ret = new Dictionary<string, Dictionary<string, Dictionary<string, int>>>();

            StringBuilder sb = new StringBuilder(20 * (wflist.Count + 1));
            sb.Append("('");
            foreach (var line in wflist)
            {
                sb.Append(line + "','");
            }
            var tempstr = sb.ToString();
            var waferids = tempstr.Substring(0, tempstr.Length - 2) + ")";

            var sql = "select WaferNo,WhichTest,Failure,Num from WaferTestSum where WaferNo in <WAFERCOND>";
            sql = sql.Replace("<WAFERCOND>", waferids);
            if (!string.IsNullOrEmpty(vtype))
            {
                sql = sql + " and VTYPE = '<VTYPE>'";
                sql = sql.Replace("<VTYPE>", vtype);
            }

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var wafer = Convert.ToString(line[0]);
                var testtype = Convert.ToString(line[1]);
                var failure = Convert.ToString(line[2]);
                var num = Convert.ToInt32(line[3]);

                if (ret.ContainsKey(testtype))
                {
                    if (ret[testtype].ContainsKey(wafer))
                    {
                        if (!ret[testtype][wafer].ContainsKey(failure))
                        {
                            ret[testtype][wafer].Add(failure, num);
                        }
                    }
                    else
                    {
                        var sub2dict = new Dictionary<string, int>();
                        sub2dict.Add(failure, num);
                        ret[testtype].Add(wafer, sub2dict);
                    }
                }
                else
                {
                    var sub1dict = new Dictionary<string, Dictionary<string, int>>();
                    var sub2dict = new Dictionary<string, int>();
                    sub2dict.Add(failure, num);
                    sub1dict.Add(wafer, sub2dict);
                    ret.Add(testtype, sub1dict);
                }
            }

            return ret;
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
            
            var lowlimit = Convert.ToDouble(glbcfg[fieldname + "_LIMIT"].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[0]);
            var highlimit = Convert.ToDouble(glbcfg[fieldname + "_LIMIT"].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1]);


            var rawdata = new List<double>();
            var sql = "select <fieldname> from BITestResultDataField where <fieldname> <> 0 and DataID in <idcond>";
            sql = sql.Replace("<fieldname>", fieldname).Replace("<idcond>", idcond);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var val = Convert.ToDouble(line[0]);
                if (val > lowlimit && val < highlimit)
                {
                    rawdata.Add(val);
                }
            }
            return rawdata;
        }

        private static Dictionary<string, List<double>> RetrieveBIFieldDataSNDict(string fieldname, string idcond, Controller ctrl)
        {
            var glbcfg = CfgUtility.GetSysConfig(ctrl);
            
            var  lowlimit = Convert.ToDouble(glbcfg[fieldname + "_LIMIT"].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[0]);
            var  highlimit = Convert.ToDouble(glbcfg[fieldname + "_LIMIT"].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1]);
            
            var rawdata = new Dictionary<string, List<double>>();
            var sql = "select <fieldname>,SN from BITestResultDataField where <fieldname> <> 0 and DataID in <idcond>";
            sql = sql.Replace("<fieldname>", fieldname).Replace("<idcond>", idcond);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var val = Convert.ToDouble(line[0]);
                var sn = Convert.ToString(line[1]);

                if (val > lowlimit && val < highlimit)
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

                object ret = new { mean = mean, min = min, max = max, lower = lower, upper = upper };
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

        private static void SolveNormalField(string wafer, string fieldname, string idcond, Controller ctrl)
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

            var glbcfg = CfgUtility.GetSysConfig(ctrl);
            var vlowlimit = Convert.ToDouble(glbcfg["Variation_POLD_Delta_LIMIT"].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[0]);
            var vhighlimit = Convert.ToDouble(glbcfg["Variation_POLD_Delta_LIMIT"].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1]);
            var ulowlimit = Convert.ToDouble(glbcfg["Uniformity_POLD_Delta_LIMIT"].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[0]);
            var uhighlimit = Convert.ToDouble(glbcfg["Uniformity_POLD_Delta_LIMIT"].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1]);

            var variatelist = new List<double>();
            var uniformlist = new List<double>();
            foreach (var kv in rawdata)
            {
                var v = kv.Value.Max() - kv.Value.Min();
                var av = kv.Value.Average();
                if (av != 0)
                {
                    var u = v / av;

                    if (v > vlowlimit && v < vhighlimit)
                    { variatelist.Add(v); }

                    if (u > ulowlimit && u < uhighlimit)
                    { uniformlist.Add(u); }
                }
            }

            var vboxdata = GetBoxData(variatelist);
            var uboxdata = GetBoxData(uniformlist);
            AddData(wafer, "Variation_" + fieldname, vboxdata);
            AddData(wafer, "Uniformity_" + fieldname, uboxdata);
        }

        public static void UpdateWaferFields(string wafer, List<BITestResult> waferdata, Controller ctrl)
        {
            StringBuilder sb = new StringBuilder(36 * (waferdata.Count + 1));
            sb.Append("('");
            foreach (var line in waferdata)
            {
                sb.Append(line.DataID + "','");
            }
            var tempstr = sb.ToString();
            var idcond = tempstr.Substring(0, tempstr.Length - 2) + ")";

            SolveNormalField(wafer, "Delta_PO_LD", idcond, ctrl);
            SolveNormalField(wafer, "Delta_PO_Uniformity", idcond, ctrl);
            SolveComputerField(wafer, "Delta_PO_LD", idcond, ctrl);
        }

        private static void CleanWaferData(string waferno, string fieldname)
        {
            var sql = "delete from WaferBGDFiled where WaferNo = @WaferNo and FieldName = @FieldName";
            var dict = new Dictionary<string, string>();
            dict.Add("@WaferNo", waferno);
            dict.Add("@FieldName", fieldname);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        private static void AddData(string wafer, string fieldname, string boxdata)
        {
            CleanWaferData(wafer, fieldname);
            var sql = "insert into WaferBGDFiled(WaferNo,FieldName,BoxData,UpdateTime) values(@WaferNo,@FieldName,@BoxData,@UpdateTime)";
            var dict = new Dictionary<string, string>();
            dict.Add("@WaferNo", wafer);
            dict.Add("@FieldName", fieldname);
            dict.Add("@BoxData", boxdata);
            dict.Add("@UpdateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public static Dictionary<string,Dictionary<string,List<string>>> RetriveWaferBoxData(List<string> wflist,string vtype)
        {
            var ret = new Dictionary<string, Dictionary<string, List<string>>>();

            StringBuilder sb = new StringBuilder(20 * (wflist.Count + 1));
            sb.Append("('");
            foreach (var line in wflist)
            {
                sb.Append(line + "','");
            }
            var tempstr = sb.ToString();
            var waferids = tempstr.Substring(0, tempstr.Length - 2) + ")";

            var sql = @"select w.WaferNo,w.FieldName,w.BoxData  from WaferBGDFiled w
                        left join (select distinct WaferNo,VTYPE from WaferTestSum ) s on s.WaferNo = w.WaferNo
                        where w.WaferNo in <WAFERCOND> ";
            sql = sql.Replace("<WAFERCOND>", waferids);

            if (!string.IsNullOrEmpty(vtype))
            {
                sql = sql + " and s.VTYPE = '<VTYPE>'";
                sql = sql.Replace("<VTYPE>", vtype);
            }

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                
                var wf = Convert.ToString(line[0]);
                var name = Convert.ToString(line[1]);
                var suffix = "";
                if (name.ToUpper().Contains("Variation_Delta_PO_LD".ToUpper()))
                {
                    suffix = "#V";
                }
                if (name.ToUpper().Contains("Uniformity_Delta_PO_LD".ToUpper()))
                {
                    suffix = "#U";
                }

                var data = Convert.ToString(line[2])+suffix;

                if (name.ToUpper().Contains("Variation_Delta_PO_LD".ToUpper())
                    || name.ToUpper().Contains("Uniformity_Delta_PO_LD".ToUpper()))
                {
                    name = "variation_uniformity_pold";
                }

                if (ret.ContainsKey(name))
                {
                    if (ret[name].ContainsKey(wf))
                    {
                        ret[name][wf].Add(data);
                    }
                    else
                    {
                        var sublist = new List<string>();
                        sublist.Add(data);
                        ret[name].Add(wf, sublist);
                    }
                }
                else
                {
                    var sublist = new List<string>();
                    sublist.Add(data);
                    var subdict = new Dictionary<string, List<string>>();
                    subdict.Add(wf, sublist);
                    ret.Add(name, subdict);
                }
            }

            return ret;
        }

        public string WaferNo { set; get; }
        public string FieldName { set; get; }
        public string BoxData { set; get; }
    }

    public class DateYield {
        public DateYield(string d, double y,double tt)
        {
            date = d;
            yield = y;
            totle = tt;
        }
        public string date { set; get; }
        public double yield { set; get; }
        public double totle { set; get; }
    }

    public class testtypeyield {
        public string TestType { set; get; }
        private List<DateYield> dateyield = new List<DateYield>();
        public List<DateYield> DYield {
            set {
                dateyield.Clear();
                dateyield.AddRange(value);
            }
            get {
                return dateyield;
            }
        }
    }

    public class FailureColumnSeg{
        public string name { set; get; }
        public double y { set; get; }
        public string color { set; get; }
    }

    public class FailureColumnData
    {
        public int index { set; get; }
        public List<FailureColumnSeg> data { set; get; }
    }

    public class DateWaferFailureColumn
    {
        public string xkey { set; get; }

        private List<FailureColumnSeg> colseglist = new List<FailureColumnSeg>();
        public List<FailureColumnSeg> DateColSeg {
            set {
                colseglist.Clear();
                colseglist.AddRange(value);
            }
            get {
                return colseglist;
            }
        }
    }

    public class TestFailureColumn
    {
        public string TestType { set; get; }
        private List<DateWaferFailureColumn> colseglist = new List<DateWaferFailureColumn>();
        public List<DateWaferFailureColumn> DateColSeg
        {
            set
            {
                colseglist.Clear();
                colseglist.AddRange(value);
            }
            get
            {
                return colseglist;
            }
        }
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
            var filteddata = new List<BITestResult>();
            foreach(var item in monthlytestdata)
            {
                if (VcselPNInfo.ContainsKey(item.ProductName))
                {
                    filteddata.Add(item);
                }
            }

            VcselMonthData.UpdateMonthData(StartDate, filteddata, VcselPNInfo);
            VcselTimeRange.UpateWaferTimeRange(StartDate, filteddata);

            var waferdict = new Dictionary<string, bool>();
            foreach (var data in filteddata)
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
            var filteddata = new List<BITestResult>();
            foreach (var item in monthlytestdata)
            {
                if (VcselPNInfo.ContainsKey(item.ProductName))
                {
                    filteddata.Add(item);
                }
            }

            VcselMonthData.UpdateMonthData(StartDate, filteddata, VcselPNInfo);
            VcselTimeRange.UpateWaferTimeRange(StartDate, filteddata);

            var waferdict = new Dictionary<string, bool>();
            foreach (var data in filteddata)
            {
                if (!waferdict.ContainsKey(data.Wafer))
                {
                    waferdict.Add(data.Wafer, true);
                }
            }
            SolveDataByWafer(waferdict, VcselPNInfo, ctrl);
        }

        public static List<string> VcselTypeList()
        {
            return WaferTestSum.VcselTypeList();
        }

        public static List<string> VcselTestList()
        {
            return WaferTestSum.VcselTestList();
        }

        public static List<string> FMColor()
        {
            //return new List<string> {
            //    "#f2debd","#b7d28d","#dcff93",
            //    "#ff9b6a","#f1b8e4","#d9b8f1",
            //    "#f1ccb8","#f1f1b8","#b8f1ed",
            //    "#b8f1cc","#e7dac9","#c86f67",
            //    "#fecf45","#ef5464"
            //};
            return new List<string> {
                "#105D9C","#23735D","#A55417","#821A08","#7030A0",
                "#0C779D","#34AC8B","#D85C00","#CC044D","#B925A7",
                "#4FADF3","#12CC92","#FA9604","#ED6161","#EF46FC",
                "#8CC9F7","#BEEBDF","#FDEEC3","#F6B0B0","#EC88F4"
            };
        }

        public static List<FailureColumnSeg> AlignmentFailureOthers(List<FailureColumnSeg> failseglist, double totalfailurecount)
        {
            var newfailseglist = new List<FailureColumnSeg>();
            var overed = false;
            var others = 0.0;
            var tempsum = 0.0;
            for (var idx = 0; idx < failseglist.Count; idx++)
            {
                tempsum = tempsum + failseglist[idx].y;
                if ((double)tempsum / (double)totalfailurecount > 0.9 && !overed)
                {
                    overed = true;
                    newfailseglist.Add(failseglist[idx]);
                }
                else if (!overed)
                {
                    newfailseglist.Add(failseglist[idx]);
                }
                else
                {
                    others = others + failseglist[idx].y;
                }
            }

            if (others > 0)
            {
                var tempfailcolseg1 = new FailureColumnSeg();
                tempfailcolseg1.name = "OTHERS";
                tempfailcolseg1.y = others;
                newfailseglist.Add(tempfailcolseg1);
            }

            return newfailseglist;
        }

        public static List<object> RetrieveVcselMonthlyData(DateTime sdate, DateTime edate, string vtype)
        {
            var ret = new List<object>();

            var alldata = VcselMonthData.RetrieveMonthlyData(sdate, edate, vtype);

            var testylist = new List<testtypeyield>();
            var testflist = new List<TestFailureColumn>();


            var name_colors = new Dictionary<string, string>();
            var colors = FMColor();
            var color_idx = 0;

            foreach (var testkv in alldata)
            {
                var dateylist = new List<DateYield>();
                var dateflist = new List<DateWaferFailureColumn>();

                foreach (var datekv in testkv.Value)
                {
                    var pass = 0;
                    var fail = 0;

                    var failseglist = new List<FailureColumnSeg>();
                    var totalfailurecount = 0.0;

                    foreach (var failurekv in datekv.Value)
                    {
                        if (failurekv.Key.ToUpper().Contains("PASS"))
                        {
                            pass = failurekv.Value;
                        }
                        else
                        {
                            fail = fail + failurekv.Value;

                            var tempfailcolseg = new FailureColumnSeg();
                            tempfailcolseg.name = failurekv.Key;
                            tempfailcolseg.y = failurekv.Value;
                            //tempfailcolseg.color = (name_colors.ContainsKey(failurekv.Key)?name_colors[failurekv.Key]:"#000");
                            failseglist.Add(tempfailcolseg);
                            totalfailurecount = totalfailurecount + tempfailcolseg.y;
                        }
                    }//end foreach
                    double yield = (double)pass / ((double)pass + fail)*100.0;
                    dateylist.Add(new DateYield(datekv.Key,yield,(double)(pass+fail)));

                    failseglist.Sort(delegate (FailureColumnSeg obj1,FailureColumnSeg obj2)
                    {
                        return obj2.y.CompareTo(obj1.y);
                    });

                    var alignmentedfailseglist = AlignmentFailureOthers(failseglist, totalfailurecount);
                    foreach (var item in alignmentedfailseglist)
                    {
                        if (!name_colors.ContainsKey(item.name))
                        {
                            name_colors.Add(item.name, colors[color_idx % colors.Count]);
                            item.color = colors[color_idx % colors.Count];
                            color_idx++;
                        }
                        else
                        {
                            item.color = name_colors[item.name];
                        }
                        item.y = Math.Round(item.y / totalfailurecount * 100,3);
                    }


                    var datefailcol = new DateWaferFailureColumn();
                    datefailcol.xkey = datekv.Key;
                    datefailcol.DateColSeg = alignmentedfailseglist;

                    dateflist.Add(datefailcol);
                }//end foreach

                dateylist.Sort(delegate (DateYield obj1, DateYield obj2) {
                    return DateTime.Parse(obj1.date).CompareTo(DateTime.Parse(obj2.date));
                });

                dateflist.Sort(delegate (DateWaferFailureColumn obj1,DateWaferFailureColumn obj2)
                {
                    return DateTime.Parse(obj1.xkey).CompareTo(DateTime.Parse(obj2.xkey));
                });

                var temptesty = new testtypeyield();
                temptesty.TestType = testkv.Key;
                temptesty.DYield = dateylist;

                testylist.Add(temptesty);

                var temptestf = new TestFailureColumn();
                temptestf.TestType = testkv.Key;
                temptestf.DateColSeg = dateflist;

                testflist.Add(temptestf);
            }//end foreach
            
            ret.Add(testylist);
            ret.Add(testflist);
            ret.Add(name_colors);
            return ret;
        }

        public static List<string> WaferNOList()
        {
            return VcselTimeRange.WaferNOList();
        }

        public static List<object> RetrieveWaferData(List<string> wflist,string vtype = "")
        {
            var ret = new List<object>();
            var boxdata = WaferBGDFiled.RetriveWaferBoxData(wflist,vtype);
            
            var waferfaile =  WaferTestSum.RetriveWaferFailure(wflist,vtype);
            var name_colors = new Dictionary<string, string>();
            var colors = FMColor();
            var color_idx = 0;
            var testflist = new List<TestFailureColumn>();

            foreach (var testkv in waferfaile)
            {
                var waferflist = new List<DateWaferFailureColumn>();
                foreach (var waferkv in testkv.Value)
                {
                    var failseglist = new List<FailureColumnSeg>();
                    var totalcount = 0.0;

                    foreach (var failurekv in waferkv.Value)
                    {
                        if (failurekv.Key.ToUpper().Contains("PASS"))
                        {
                            totalcount = totalcount + failurekv.Value;
                        }
                        else
                        {
                            var tempfailcolseg = new FailureColumnSeg();
                            tempfailcolseg.name = failurekv.Key;
                            tempfailcolseg.y = failurekv.Value;
                            failseglist.Add(tempfailcolseg);
                            totalcount = totalcount + tempfailcolseg.y;
                        }
                    }//end foreach
                    failseglist.Sort(delegate (FailureColumnSeg obj1, FailureColumnSeg obj2)
                    {
                        return obj2.y.CompareTo(obj1.y);
                    });

                    foreach (var item in failseglist)
                    {
                        if (!name_colors.ContainsKey(item.name))
                        {
                            name_colors.Add(item.name, colors[color_idx % colors.Count]);
                            item.color = colors[color_idx % colors.Count];
                            color_idx++;
                        }
                        else
                        {
                            item.color = name_colors[item.name];
                        }
                        item.y = Math.Round(item.y / totalcount * 100,3);
                    }

                    var waferf = new DateWaferFailureColumn();
                    waferf.xkey = waferkv.Key;
                    waferf.DateColSeg = failseglist;

                    waferflist.Add(waferf);

                }//end foreach

                var temptestf = new TestFailureColumn();
                temptestf.TestType = testkv.Key;
                temptestf.DateColSeg = waferflist;

                testflist.Add(temptestf);
            }//end foreach
            
            testflist.Sort(delegate (TestFailureColumn obj1, TestFailureColumn obj2)
            {
                return obj2.TestType.CompareTo(obj1.TestType);
            });
            ret.Add(boxdata);
            ret.Add(testflist);
            ret.Add(name_colors);
            return ret;
        }

        public static List<BITestResultDataField> RetrieveWaferData(string wf_no)
        {
            var ret = new List<BITestResultDataField>();
            var sql = @"SELECT SN,TestName,TestTimeStamp,PN,Wafer,JO,Channel,SLOPE,PO_LD,PO_Uniformity,THOLD,Delta_PO_LD,Delta_SLOPE
                        , Delta_THOLD, Delta_PO_Uniformity, ProductName FROM BITestResultDataField where Wafer = @Wafer";
            var dict = new Dictionary<string, string>();
            dict.Add("@Wafer", wf_no);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
            foreach (var line in dbret)
            {
                var tempvm = new BITestResultDataField(Convert.ToString(line[0]), Convert.ToString(line[1]), Convert.ToDateTime(line[2])
                    , Convert.ToString(line[3]), Convert.ToString(line[4]), Convert.ToString(line[5])
                    , Convert.ToString(line[6]), Convert.ToDouble(line[7]), Convert.ToDouble(line[8])
                    , Convert.ToDouble(line[9]), Convert.ToDouble(line[10]), Convert.ToDouble(line[11])
                    , Convert.ToDouble(line[12]), Convert.ToDouble(line[13]), Convert.ToDouble(line[14])
                    , Convert.ToString(line[15]));
                ret.Add(tempvm);
            }
            return ret;
        }


    }

}