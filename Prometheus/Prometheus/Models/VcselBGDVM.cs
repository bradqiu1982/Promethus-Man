using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Prometheus.Models
{
    public class VCSELRATE {
        public static string r25G = "25G";
        public static string r10G = "10G";
        public static string r14G = "14G";
        public static string r48G = "48G";
    }

    public class HTOLTESTTYPE {
        public static string PRE_BURN_IN = "PRE BURN IN";
        public static string POST_BURN_IN = "POST BURN IN";
        public static string POST_HTOL_BURN_IN = "POST HTOL BURN IN";
    }

    public class HTOLTESTFIELDNAME {
        public static string PO_LD = "PO_LD";
        public static string PO_UNIFORMITY = "PO_UNIFORMITY";
        public static string DELTA_PO_LD = "DELTA_PO_LD";
        public static string DELTA_PO_UNIFORMITY = "DELTA_PO_UNIFORMITY";
    }

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

        public static bool MonthDataExist(DateTime sdate,string defTab = "VcselMonthData")
        {
            var sql = "select top 1 StartDate from <defTab> where StartDate = @StartDate";
            sql = sql.Replace("<defTab>", defTab);

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

        private static void CleanMonthData(DateTime sdate, string defTab = "VcselMonthData")
        {
            var sql = "delete from <defTab> where StartDate = '<StartDate>'";
            sql = sql.Replace("<defTab>", defTab);

            sql = sql.Replace("<StartDate>", sdate.ToString("yyyy-MM-dd HH:mm:ss"));
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        private static void StoreMonthData(DateTime sdate, string vtype, string failure, string num, string defTab = "VcselMonthData") {
            var sql = "insert into <defTab>(StartDate,VTYPE,Failure,Num,UpdateTime) values(@StartDate,@VTYPE,@Failure,@Num,@UpdateTime)";
            sql = sql.Replace("<defTab>", defTab);

            var dict = new Dictionary<string, string>();
            dict.Add("@StartDate", sdate.ToString("yyyy-MM-dd HH:mm:ss"));
            dict.Add("@VTYPE", vtype);
            dict.Add("@Failure", failure);
            dict.Add("@Num", num);
            dict.Add("@UpdateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public static void UpdateMonthData(DateTime sdate, List<BITestResult> mdata, Dictionary<string, VcselPNData> VcselPNInfo, string defTab = "VcselMonthData")
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

            CleanMonthData(sdate,defTab);

            foreach (var typevaluekv in keydict)
            {
                foreach (var failurekv in typevaluekv.Value)
                {
                    StoreMonthData(sdate, typevaluekv.Key, failurekv.Key, failurekv.Value.ToString(),defTab);
                }
            }
        }
        
        //<testname,<date,<failure,cnt>>>
        public static Dictionary<string, Dictionary<string, Dictionary<string, int>>> RetrieveMonthlyData(DateTime startdate, DateTime enddate, string vtype, string defTab = "VcselMonthData")
        {
            var ret = new Dictionary<string, Dictionary<string, Dictionary<string, int>>>();

            var sql = "select StartDate,VTYPE,Failure,Num from <defTab> where VTYPE like @VTYPE and StartDate >= @sdate and StartDate < @edate";
            sql = sql.Replace("<defTab>", defTab);

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

        public static Dictionary<string, DateWaferFailureColumn> RetrieveTotleFailure(DateTime startdate, DateTime enddate, string vtype, Dictionary<string, bool> allfailure, string defTab = "VcselMonthData")
        {
            var ret = new Dictionary<string, DateWaferFailureColumn>();
                        
            var sql = "select StartDate,VTYPE,Failure,Num from <defTab> where VTYPE like @VTYPE and StartDate >= @sdate and StartDate < @edate";
            sql = sql.Replace("<defTab>", defTab);

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
                if (string.Compare(failure, "pass", true) != 0)
                {
                    if (ret.ContainsKey(sdate))
                    {
                        var tempvm = ret[sdate];
                        tempvm.total = tempvm.total + num;
                        tempvm.xkey = sdate;

                        var failureexist = false;
                        foreach (var item in tempvm.DateColSeg)
                        {
                            if (string.Compare(item.name, failure, true) == 0)
                            {
                                item.y = item.y + num;
                                failureexist = true;
                                break;
                            }
                        }

                        if (!failureexist)
                        {
                            var tempfailure = new FailureColumnSeg();
                            tempfailure.name = failure;
                            tempfailure.y = num;
                                                
                            tempvm.DateColSeg.Add(tempfailure);
                        }
                    }
                    else
                    {
                        var tempfailure = new FailureColumnSeg();
                        tempfailure.name = failure;
                        tempfailure.y = num;

                        var tempvm = new DateWaferFailureColumn();
                        tempvm.total = tempvm.total + num;
                        tempvm.DateColSeg.Add(tempfailure);
                        tempvm.xkey = sdate;

                        ret.Add(sdate, tempvm);
                    }
                }//all fail
            }//foreach

            var datelist = ret.Keys.ToList();

            

            foreach (var d in datelist)
            {
                var tempvm = ret[d];
                var tempfailurelist = tempvm.DateColSeg;
                tempfailurelist.Sort(delegate (FailureColumnSeg obj1, FailureColumnSeg obj2) {
                    return obj2.y.CompareTo(obj1.y);
                });

                tempfailurelist = VcselBGDVM.AlignmentFailureOthers(tempvm.DateColSeg, tempvm.total);
                tempvm.DateColSeg.Clear();
                tempvm.DateColSeg.AddRange(tempfailurelist);
                foreach (var item in tempfailurelist)
                {
                    if (!allfailure.ContainsKey(item.name))
                    { allfailure.Add(item.name, true); }
                }
            }

            var failurekeylist = allfailure.Keys.ToList();

            foreach (var d in datelist)
            {
                foreach (var fk in failurekeylist)
                { allfailure[fk] = true; }

                var tempvm = ret[d];
                var tempfailurelist = new List<FailureColumnSeg>();
                foreach (var item in tempvm.DateColSeg)
                {
                    allfailure[item.name] = false;
                    item.y = Math.Round((double)item.y / tempvm.total * 100.0, 3);
                    tempfailurelist.Add(item);
                }
                foreach (var kv in allfailure)
                {
                    if (kv.Value)
                    {
                        var tempf = new FailureColumnSeg();
                        tempf.name = kv.Key;
                        tempf.y = 0.0;
                        tempfailurelist.Add(tempf);
                    }
                }
                tempvm.DateColSeg.Clear();
                tempvm.DateColSeg.AddRange(tempfailurelist);
            }//end foreach

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

        public static List<VcselTimeRange> RetrieveWafer(string wafer,string defTab = "VcselTimeRange")
        {
            var ret = new List<VcselTimeRange>();
            var sql = "select WaferNo,StartDate,EndDate from <defTab> where WaferNo = @WaferNo";
            sql = sql.Replace("<defTab>", defTab);

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

        public static List<string> RetrieveWafer(DateTime starttime, DateTime endtime, string defTab = "VcselTimeRange")
        {
            var ret = new List<string>();
            var sql = "select WaferNo from <defTab> where (EndDate > @S1 and EndDate < @E1) OR (StartDate >= @S2 and EndDate <= @E2) OR (StartDate >= @S3 and StartDate < @E3)";
            sql = sql.Replace("<defTab>", defTab);

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

        private static void AddWafer(DateTime sdate, string wafer, string defTab = "VcselTimeRange")
        {
            var ret = RetrieveWafer(wafer,defTab);
            if (ret.Count == 0)
            {
                var sql = "insert into <defTab>(WaferNo,StartDate,EndDate) values(@WaferNo,@StartDate,@EndDate)";
                sql = sql.Replace("<defTab>", defTab);

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
                    sql = "update <defTab> set EndDate = @EndDate where WaferNo = @WaferNo";
                    sql = sql.Replace("<defTab>", defTab);

                    dict.Add("@EndDate", sdate.AddMonths(1).ToString("yyyy-MM-dd HH:mm:ss"));
                }
                if (sdate < ret[0].StartDate)
                {
                    sql = "update <defTab> set StartDate = @StartDate where WaferNo = @WaferNo";
                    sql = sql.Replace("<defTab>", defTab);

                    dict.Add("@StartDate", sdate.ToString("yyyy-MM-dd HH:mm:ss"));
                }
                if (!string.IsNullOrEmpty(sql))
                {
                    DBUtility.ExeLocalSqlNoRes(sql, dict);
                }
            }//end else
        }

        public static void UpateWaferTimeRange(DateTime sdate, List<BITestResult> mdata, string defTab = "VcselTimeRange")
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
                AddWafer(sdate, kv.Key,defTab);
            }
        }

        public static List<string> WaferNOList(string defTab = "VcselTimeRange")
        {
            var ret = new List<string>();
            var sql = "select distinct WaferNo from <defTab>";
            sql = sql.Replace("<defTab>", defTab);

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

        private static void StoreWaferData(string waferno, string whichtest, string vtype, string failure, string num, string dataids,string defTab = "WaferTestSum")
        {
            var sql = "insert into <defTab>(WaferNo,WhichTest,VTYPE,Failure,Num,UpdateTime,DataIDs) values(@WaferNo,@WhichTest,@VTYPE,@Failure,@Num,@UpdateTime,@DataIDs)";
            sql = sql.Replace("<defTab>", defTab);

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

        private static void CleanWaferData(string waferno, string defTab = "WaferTestSum")
        {
            var sql = "delete from <defTab> where WaferNo = @WaferNo";
            sql = sql.Replace("<defTab>", defTab);

            var dict = new Dictionary<string, string>();
            dict.Add("@WaferNo", waferno);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        //public static string DataIDCond(string waferno)
        //{
        //    var sql = "select top 1 DataIDs from WaferTestSum where WaferNo = @WaferNo";
        //    var dict = new Dictionary<string, string>();
        //    dict.Add("@WaferNo", waferno);
        //    var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
        //    foreach (var line in dbret)
        //    {
        //        return Convert.ToString(line[0]);
        //    }
        //    return string.Empty;
        //}

        public static void UpdateWaferTestData(string wafer, List<BITestResult> waferdata, Dictionary<string, VcselPNData> VcselPNInfo, string defTab = "WaferTestSum")
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

            CleanWaferData(wafer,defTab);

            foreach (var typevaluekv in keydict)
            {
                foreach (var failurekv in typevaluekv.Value)
                {
                    StoreWaferData(wafer, typevaluekv.Key, vtype, failurekv.Key, failurekv.Value.ToString(), dataids,defTab);
                }
            }
        }

        public static List<string> VcselTypeList(string defTab = "WaferTestSum")
        {
            var ret = new List<string>();
            var sql = "select distinct VTYPE from <defTab>";
            sql = sql.Replace("<defTab>", defTab);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var vt = Convert.ToString(line[0]);
                if (!string.IsNullOrEmpty(vt))
                {
                    ret.Add(vt);
                }
            }
            ret.Sort();
            return ret;
        }

        public static List<string> VcselTestList(string defTab = "WaferTestSum")
        {
            var ret = new List<string>();
            var sql = "select distinct WhichTest from <defTab>";
            sql = sql.Replace("<defTab>", defTab);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            { ret.Add(Convert.ToString(line[0])); }
            ret.Sort();
            return ret;
        }

        //<testname,<wafer,<failure,cnt>>>
        public static Dictionary<string, Dictionary<string, Dictionary<string,int>>> RetriveWaferFailure(List<string> wflist,string vtype, string defTab = "WaferTestSum")
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

            var sql = "select WaferNo,WhichTest,Failure,Num from <defTab> where WaferNo in <WAFERCOND>";
            sql = sql.Replace("<defTab>", defTab);

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


    public class WAFERFIELDDATATYPE {
        public static string ALLBOX = "ALLBOX";
        public static string PASSBOX = "PASSBOX";
        public static string ALLRAW = "ALLRAW";
        public static string PASSRAW = "PASSRAW";
    }

    public class WaferBGDField
    {
        public WaferBGDField()
        {
            WaferNo = "";
            FieldName = "";
            BoxData = "";
        }


        private static List<double> RetrieveBIFieldData(string fieldname, List<BITestResult> waferdata, Controller ctrl,bool withfaileddata)
        {
            var glbcfg = CfgUtility.GetSysConfig(ctrl);
            
            var lowlimit = Convert.ToDouble(glbcfg[fieldname + "_LIMIT"].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[0]);
            var highlimit = Convert.ToDouble(glbcfg[fieldname + "_LIMIT"].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1]);


            var rawdata = new List<double>();

            var splitdata = new List<List<BITestResult>>();
            var templist = new List<BITestResult>();
            foreach (var item in waferdata)
            {
                templist.Add(item);
                if (templist.Count >= 2000)
                {
                    splitdata.Add(templist);
                    templist = new List<BITestResult>();
                }
            }
            if (templist.Count > 0)
            {
                splitdata.Add(templist);
            }

            foreach (var sdata in splitdata)
            {
                StringBuilder sb = new StringBuilder(36 * 5010);
                sb.Append("('");
                foreach (var line in sdata)
                {
                    sb.Append(line.DataID + "','");
                }
                var tempstr = sb.ToString();
                var idcond = tempstr.Substring(0, tempstr.Length - 2) + ")";

                var sql = "select <fieldname> from BITestResultDataField where <fieldname> <> 0 and DataID in <idcond>";
                if (!withfaileddata)
                {
                    sql = @"select bf.<fieldname> from BITestResultDataField bf 
                            left join BITestResult br on bf.DataID = br.DataID
                            where bf.<fieldname> <> 0 and bf.DataID in <idcond> and br.Failure = 'Pass'";
                }

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
            }

            return rawdata;
        }

        private static List<double> RetrieveHTOLBIFieldData(string testname, string fieldname, string idcond, Controller ctrl, bool withfaileddata)
        {
            var glbcfg = CfgUtility.GetSysConfig(ctrl);

            var lowlimit = Convert.ToDouble(glbcfg[testname.Replace(" ", "_") + "*" + fieldname + "*LIMIT"].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[0]);
            var highlimit = Convert.ToDouble(glbcfg[testname.Replace(" ", "_") + "*" + fieldname + "*LIMIT"].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1]);


            var rawdata = new List<double>();
            var sql = "select <fieldname> from BIHTOLTestResultDataField where <fieldname> <> 0 and TestName = '<testname>' and DataID in <idcond>";


            if (!withfaileddata)
            {
                sql = @"select bf.<fieldname> from BIHTOLTestResultDataField bf 
                        left join BIHTOLTestResult br on bf.DataID = br.DataID
                        where bf.<fieldname> <> 0  and bf.TestName = '<testname>' and bf.DataID in <idcond> and br.Failure = 'Pass'";
            }

            sql = sql.Replace("<fieldname>", fieldname).Replace("<testname>", testname).Replace("<idcond>", idcond);
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

        private static Dictionary<string, List<double>> RetrieveBIFieldDataSNDict(string fieldname, List<BITestResult> waferdata, Controller ctrl,bool withfaileddata)
        {
            var glbcfg = CfgUtility.GetSysConfig(ctrl);
            
            var  lowlimit = Convert.ToDouble(glbcfg[fieldname + "_LIMIT"].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[0]);
            var  highlimit = Convert.ToDouble(glbcfg[fieldname + "_LIMIT"].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1]);
            
            var rawdata = new Dictionary<string, List<double>>();

            var splitdata = new List<List<BITestResult>>();
            var templist = new List<BITestResult>();
            foreach (var item in waferdata)
            {
                templist.Add(item);
                if (templist.Count >= 2000)
                {
                    splitdata.Add(templist);
                    templist = new List<BITestResult>();
                }
            }
            if (templist.Count > 0)
            {
                splitdata.Add(templist);
            }

            foreach (var sdata in splitdata)
            {
                StringBuilder sb = new StringBuilder(36 * 5010);
                sb.Append("('");
                foreach (var line in sdata)
                {
                    sb.Append(line.DataID + "','");
                }
                var tempstr = sb.ToString();
                var idcond = tempstr.Substring(0, tempstr.Length - 2) + ")";

                var sql = "select <fieldname>,SN from BITestResultDataField where <fieldname> <> 0 and DataID in <idcond>";
    
                if (!withfaileddata)
                {
                    sql = @"select bf.<fieldname>,bf.SN from BITestResultDataField bf 
                            left join BITestResult br on bf.DataID = br.DataID
                            where bf.<fieldname> <> 0 and bf.DataID in <idcond> and br.Failure = 'Pass'";
                }

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
                            var tempvallist = new List<double>();
                            tempvallist.Add(val);
                            rawdata.Add(sn, tempvallist);
                        }
                    }
                }
            }//end foreach

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

        public static CBOXData GetBoxData(List<double> rawdata,double llimit,double hlimit)
        {
            var mean = 0.0;
            var min = 0.0;
            var max = 0.0;
            var lower = 0.0;
            var upper = 0.0;
            if ((rawdata.Count % 2) == 0)
            {
                var mididx1 = rawdata.Count / 2;
                var mididx2 = mididx1 + 1;

                mean = (rawdata[mididx1] + rawdata[mididx2]) / 2.0;
                //min = rawdata[0];
                //max = rawdata[rawdata.Count - 1];

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

                lower = GetBoxMeanValue(lowlist);
                upper = GetBoxMeanValue(upperlist);


            }
            else
            {
                var mididx = (rawdata.Count + 1) / 2;
                mean = rawdata[mididx];
                //min = rawdata[0];
                //max = rawdata[rawdata.Count - 1];

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

                lower = GetBoxMeanValue(lowlist);
                upper = GetBoxMeanValue(upperlist);
            }

            var cbox = new CBOXData();
            cbox.mean = mean;
            cbox.lower = lower;
            cbox.upper = upper;
            max = upper + (upper - lower) * 1.5;
            if (max > rawdata[rawdata.Count - 1])
            { max = rawdata[rawdata.Count - 1]; }
            cbox.max = (max < hlimit)?max:hlimit;

            min = lower - (upper - lower) * 1.5;
            if (min < rawdata[0])
            { min = rawdata[0]; }
            cbox.min = (min > llimit)?min:llimit;

            return cbox;
        }

        private static void SolveNormalField(string wafer, string fieldname, List<BITestResult> waferdata, Controller ctrl,bool withfaileddata)
        {
            var rawdata = RetrieveBIFieldData(fieldname, waferdata, ctrl, withfaileddata);
            if (rawdata.Count < 30) {
                return;
            }
            //var boxdata = GetBoxData(rawdata);
            //AddData(wafer, fieldname, boxdata,withfaileddata);

            var computeddata = Newtonsoft.Json.JsonConvert.SerializeObject(rawdata);
            AddRawData(wafer, fieldname, computeddata, withfaileddata);

        }

        private static void SolveHTOLNormalField(string wafer,string testname,string fieldname,string idcond, Controller ctrl, bool withfaileddata)
        {
            var rawdata = RetrieveHTOLBIFieldData(testname,fieldname, idcond, ctrl, withfaileddata);
            if (rawdata.Count < 10)
            {
                return;
            }

            var computeddata = Newtonsoft.Json.JsonConvert.SerializeObject(rawdata);
            AddHTOLRawData(wafer, testname+"*"+fieldname, computeddata, withfaileddata);
        }

        private static void SolveComputeField(string wafer, string fieldname, List<BITestResult> waferdata, Controller ctrl,bool withfaileddata)
        {
            var rawdata = RetrieveBIFieldDataSNDict(fieldname, waferdata, ctrl, withfaileddata);
            if (rawdata.Count < 30)
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

            //var vboxdata = GetBoxData(variatelist);
            //var uboxdata = GetBoxData(uniformlist);
            //AddData(wafer, "Variation_" + fieldname, vboxdata,withfaileddata);
            //AddData(wafer, "Uniformity_" + fieldname, uboxdata,withfaileddata);



            var vcomputeddata = Newtonsoft.Json.JsonConvert.SerializeObject(variatelist);
            AddRawData(wafer, "Variation_" + fieldname, vcomputeddata,withfaileddata);

            var ucomputeddata = Newtonsoft.Json.JsonConvert.SerializeObject(uniformlist);
            AddRawData(wafer, "Uniformity_" + fieldname, ucomputeddata,withfaileddata);

        }

        public static void UpdateWaferFields(string wafer, List<BITestResult> waferdata, Controller ctrl,bool withfaileddata = true)
        {
            SolveNormalField(wafer, "Delta_PO_LD", waferdata, ctrl, withfaileddata);
            SolveNormalField(wafer, "Delta_PO_Uniformity", waferdata, ctrl, withfaileddata);
            SolveComputeField(wafer, "Delta_PO_LD", waferdata, ctrl, withfaileddata);
        }

        public static void UpdateHTOLWaferFields(string wafer, List<BITestResult> waferdata, Controller ctrl, bool withfaileddata = true)
        {
            StringBuilder sb = new StringBuilder(36 * (waferdata.Count + 1));
            sb.Append("('");
            foreach (var line in waferdata)
            {
                sb.Append(line.DataID + "','");
            }
            var tempstr = sb.ToString();
            var idcond = tempstr.Substring(0, tempstr.Length - 2) + ")";

            SolveHTOLNormalField(wafer,HTOLTESTTYPE.PRE_BURN_IN, HTOLTESTFIELDNAME.PO_LD, idcond, ctrl, withfaileddata);
            SolveHTOLNormalField(wafer, HTOLTESTTYPE.POST_BURN_IN, HTOLTESTFIELDNAME.PO_LD, idcond, ctrl, withfaileddata);
            SolveHTOLNormalField(wafer, HTOLTESTTYPE.POST_HTOL_BURN_IN, HTOLTESTFIELDNAME.PO_LD, idcond, ctrl, withfaileddata);

            SolveHTOLNormalField(wafer, HTOLTESTTYPE.PRE_BURN_IN, HTOLTESTFIELDNAME.PO_UNIFORMITY, idcond, ctrl, withfaileddata);
            SolveHTOLNormalField(wafer, HTOLTESTTYPE.POST_BURN_IN, HTOLTESTFIELDNAME.PO_UNIFORMITY, idcond, ctrl, withfaileddata);
            SolveHTOLNormalField(wafer, HTOLTESTTYPE.POST_HTOL_BURN_IN, HTOLTESTFIELDNAME.PO_UNIFORMITY, idcond, ctrl, withfaileddata);

            SolveHTOLNormalField(wafer, HTOLTESTTYPE.POST_BURN_IN, HTOLTESTFIELDNAME.DELTA_PO_LD, idcond, ctrl, withfaileddata);
            SolveHTOLNormalField(wafer, HTOLTESTTYPE.POST_BURN_IN, HTOLTESTFIELDNAME.DELTA_PO_UNIFORMITY, idcond, ctrl, withfaileddata);
            SolveHTOLNormalField(wafer, HTOLTESTTYPE.POST_HTOL_BURN_IN, HTOLTESTFIELDNAME.DELTA_PO_LD, idcond, ctrl, withfaileddata);
        }

        private static void CleanWaferData(string waferno, string fieldname,string datatype)
        {
            var sql = "delete from WaferBGDField where WaferNo = @WaferNo and FieldName = @FieldName and DataType = @DataType";

            var dict = new Dictionary<string, string>();
            dict.Add("@WaferNo", waferno);
            dict.Add("@FieldName", fieldname);
            dict.Add("@DataType", datatype);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        private static void CleanHTOLWaferData(string waferno, string fieldname, string datatype)
        {
            var sql = "delete from HTOLWaferBGDField where WaferNo = @WaferNo and FieldName = @FieldName and DataType = @DataType";

            var dict = new Dictionary<string, string>();
            dict.Add("@WaferNo", waferno);
            dict.Add("@FieldName", fieldname);
            dict.Add("@DataType", datatype);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        //private static void AddData(string wafer, string fieldname, string boxdata,bool withfaileddata)
        //{
        //    var datatype = WAFERFIELDDATATYPE.ALLBOX;
        //    if (!withfaileddata) datatype = WAFERFIELDDATATYPE.PASSBOX;

        //    CleanWaferData(wafer, fieldname, datatype);

        //    var sql = "insert into WaferBGDField(WaferNo,FieldName,BoxData,UpdateTime,DataType) values(@WaferNo,@FieldName,@BoxData,@UpdateTime,@DataType)";
        //    var dict = new Dictionary<string, string>();
        //    dict.Add("@WaferNo", wafer);
        //    dict.Add("@FieldName", fieldname);
        //    dict.Add("@BoxData", boxdata);
        //    dict.Add("@UpdateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        //    dict.Add("@DataType", datatype);
        //    DBUtility.ExeLocalSqlNoRes(sql, dict);
        //}

        private static void AddRawData(string wafer, string fieldname, string boxdata, bool withfaileddata)
        {
            var datatype = WAFERFIELDDATATYPE.ALLRAW;
            if (!withfaileddata) datatype = WAFERFIELDDATATYPE.PASSRAW;

            CleanWaferData(wafer, fieldname, datatype);

            var sql = "insert into WaferBGDField(WaferNo,FieldName,BoxData,UpdateTime,DataType) values(@WaferNo,@FieldName,@BoxData,@UpdateTime,@DataType)";
            var dict = new Dictionary<string, string>();
            dict.Add("@WaferNo", wafer);
            dict.Add("@FieldName", fieldname);
            dict.Add("@BoxData", boxdata);
            dict.Add("@UpdateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            dict.Add("@DataType", datatype);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        private static void AddHTOLRawData(string wafer, string fieldname, string boxdata, bool withfaileddata)
        {
            var datatype = WAFERFIELDDATATYPE.ALLRAW;
            if (!withfaileddata) datatype = WAFERFIELDDATATYPE.PASSRAW;

            CleanHTOLWaferData(wafer, fieldname, datatype);

            var sql = "insert into HTOLWaferBGDField(WaferNo,FieldName,BoxData,UpdateTime,DataType) values(@WaferNo,@FieldName,@BoxData,@UpdateTime,@DataType)";
            var dict = new Dictionary<string, string>();
            dict.Add("@WaferNo", wafer);
            dict.Add("@FieldName", fieldname);
            dict.Add("@BoxData", boxdata);
            dict.Add("@UpdateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            dict.Add("@DataType", datatype);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        //<fieldname,<wafer,datalist>>
        public static Dictionary<string,Dictionary<string,List<string>>> RetriveBURNINHTOLFieldData(List<string> wflist,bool withfaileddata
            ,string vtype,string defTab = "WaferBGDField" ,string defTab2 = "WaferTestSum")
        {
            var datatype  = WAFERFIELDDATATYPE.ALLRAW;
            if (!withfaileddata) datatype = WAFERFIELDDATATYPE.PASSRAW;

           var ret = new Dictionary<string, Dictionary<string, List<string>>>();

            StringBuilder sb = new StringBuilder(20 * (wflist.Count + 1));
            sb.Append("('");
            foreach (var line in wflist)
            {
                sb.Append(line + "','");
            }
            var tempstr = sb.ToString();
            var waferids = tempstr.Substring(0, tempstr.Length - 2) + ")";

            var sql = @"select w.WaferNo,w.FieldName,w.BoxData  from <defTab> w
                        left join (select distinct WaferNo,VTYPE from <defTab2> ) s on s.WaferNo = w.WaferNo
                        where w.WaferNo in <WAFERCOND> and DataType = '<DataType>'  ";

            sql = sql.Replace("<defTab>", defTab).Replace("<defTab2>", defTab2).Replace("<DataType>",datatype).Replace("<WAFERCOND>", waferids);

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
        public FailureColumnSeg()
        {
            name = string.Empty;
            y = 0.0;
            color = string.Empty;
        }
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
        public DateWaferFailureColumn()
        { total = 0.0; }

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
        public double total { set; get; }
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

    public class VXVal
    {
        public VXVal()
        {
            x = 0.0;
            ival = 0.0;
        }

        public double x { set; get; }
        public double ival { set; get; }
    }

    public class VcselBGDVM
    {
        private static List<BITestResult> RetrieveBITestData(DateTime startdate, DateTime enddate,string defTab = "BITestResult")
        {
            var ret = new List<BITestResult>();

            var sql = "select SN,TestName,Failure,Wafer,ProductName,DataID from <defTab> where TestTimeStamp >= @startdate and TestTimeStamp < @enddate and ProductName <> '' and Wafer <> '' and Appv_1 <> 'DELETE' order by TestTimeStamp desc";
            sql = sql.Replace("<defTab>", defTab);

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

        private static List<BITestResult> RetrieveBITestData(string wafer,string defTab = "BITestResult")
        {
            var ret = new List<BITestResult>();

            var sql = "select SN,TestName,Failure,Wafer,ProductName,DataID from <defTab> where ProductName <> '' and Wafer = @Wafer  and Appv_1 <> 'DELETE' order by TestTimeStamp desc";
            sql = sql.Replace("<defTab>", defTab);

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

        private static bool IsWaferSpend6Month(string wafer,DateTime endtime)
        {
            var ret = false;
            try
            {
                var sql = "select top 1 TestTimeStamp from BITestResult where Wafer = @Wafer order by TestTimeStamp asc";
                var dict = new Dictionary<string, string>();
                dict.Add("@Wafer", wafer);

                var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
                if (dbret.Count == 0) { return ret; }
                var starttime = Convert.ToDateTime(dbret[0][0]);

                if ((endtime - starttime).Days > 180)
                { ret = true; }
            }
            catch (Exception ex) { }

            return ret;
        }


        private static void SolveDataByWafer(Dictionary<string, bool> waferdict, Dictionary<string, VcselPNData> VcselPNInfo, Controller ctrl)
        {
            foreach (var kv in waferdict)
            {
                var wafertestdata = RetrieveBITestData(kv.Key);
                if (wafertestdata.Count > 20)
                {
                    WaferTestSum.UpdateWaferTestData(kv.Key, wafertestdata, VcselPNInfo);
                    WaferBGDField.UpdateWaferFields(kv.Key, wafertestdata,ctrl);
                    WaferBGDField.UpdateWaferFields(kv.Key, wafertestdata, ctrl,false);
                }
            }
        }

        private static void SolveHTOLDataByWafer(Dictionary<string, bool> waferdict, Dictionary<string, VcselPNData> VcselPNInfo, Controller ctrl)
        {
            foreach (var kv in waferdict)
            {
                var wafertestdata = RetrieveBITestData(kv.Key, "BIHTOLTestResult");
                if (wafertestdata.Count > 10)
                {
                    WaferTestSum.UpdateWaferTestData(kv.Key, wafertestdata, VcselPNInfo, "HTOLWaferTestSum");

                    WaferBGDField.UpdateHTOLWaferFields(kv.Key, wafertestdata, ctrl);
                    WaferBGDField.UpdateHTOLWaferFields(kv.Key, wafertestdata, ctrl, false);
                }
            }
        }

        

        public static void StartVcselBGDComputer(DateTime StartDate, Dictionary<string, VcselPNData> VcselPNInfo,Controller ctrl)
        {
            var EndDate = StartDate.AddMonths(1);
            var monthlytestdata = RetrieveBITestData(StartDate, EndDate);

            var waferspend6monthdict = new Dictionary<string, bool>();
            foreach (var data in monthlytestdata)
            {
                if (!waferspend6monthdict.ContainsKey(data.Wafer))
                {
                    var yesno = IsWaferSpend6Month(data.Wafer, StartDate);
                    waferspend6monthdict.Add(data.Wafer, yesno);
                }
            }

            var filteddata = new List<BITestResult>();
            foreach(var item in monthlytestdata)
            {
                if (VcselPNInfo.ContainsKey(item.ProductName) && !waferspend6monthdict[item.Wafer])
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

        public static void ClearHTOLDataWithPostBurnInFailure()
        {
            var threemonthagao = DateTime.Now.AddMonths(-3);
            var sql = @"update [NPITrace].[dbo].[BIHTOLTestResult] set Appv_1 = 'DELETE' where SN in(
                  select SN from [NPITrace].[dbo].[BIHTOLTestResult] where  (SN+'_'+  convert(varchar, TestTimeStamp, 120))  in (
                  SELECT distinct SN+'_'+  convert(varchar, MAX(TestTimeStamp), 120) as cname
                  FROM [NPITrace].[dbo].[BIHTOLTestResult] where TestName = 'Post Burn In' and TestTimeStamp > '<CLEARSTARTTIME>' group by SN)  and Failure <> 'Pass' and TestName = 'Post Burn In') and TestName = 'Post HTOL Burn In'";
            sql = sql.Replace("<CLEARSTARTTIME>", threemonthagao.ToString("yyyy-MM-dd HH:mm:ss"));
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static void StartHTOLBGDComputer(DateTime StartDate, Dictionary<string, VcselPNData> VcselPNInfo, Controller ctrl)
        {
            var EndDate = StartDate.AddMonths(1);
            var monthlytestdata = RetrieveBITestData(StartDate, EndDate, "BIHTOLTestResult");
            var filteddata = new List<BITestResult>();
            foreach (var item in monthlytestdata)
            {
                if (VcselPNInfo.ContainsKey(item.ProductName))
                {
                    filteddata.Add(item);
                }
            }

            VcselMonthData.UpdateMonthData(StartDate, filteddata, VcselPNInfo, "HTOLMonthData");
            VcselTimeRange.UpateWaferTimeRange(StartDate, filteddata, "HTOLVcselTimeRange");

            var waferdict = new Dictionary<string, bool>();
            foreach (var data in filteddata)
            {
                if (!waferdict.ContainsKey(data.Wafer))
                {
                    waferdict.Add(data.Wafer, true);
                }
            }
            SolveHTOLDataByWafer(waferdict, VcselPNInfo, ctrl);
        }
        
        //public static void TestVcselBGDComputer(DateTime StartDate, Dictionary<string, VcselPNData> VcselPNInfo, Controller ctrl)
        //{
        //    var EndDate = StartDate.AddDays(7);
        //    var monthlytestdata = RetrieveBITestData(StartDate, EndDate);
        //    var filteddata = new List<BITestResult>();
        //    foreach (var item in monthlytestdata)
        //    {
        //        if (VcselPNInfo.ContainsKey(item.ProductName))
        //        {
        //            filteddata.Add(item);
        //        }
        //    }

        //    VcselMonthData.UpdateMonthData(StartDate, filteddata, VcselPNInfo);
        //    VcselTimeRange.UpateWaferTimeRange(StartDate, filteddata);

        //    var waferdict = new Dictionary<string, bool>();
        //    foreach (var data in filteddata)
        //    {
        //        if (!waferdict.ContainsKey(data.Wafer))
        //        {
        //            waferdict.Add(data.Wafer, true);
        //        }
        //    }
        //    SolveDataByWafer(waferdict, VcselPNInfo, ctrl);
        //}

        public static List<string> VcselTypeList(string defTab = "WaferTestSum")
        {
            return WaferTestSum.VcselTypeList(defTab);
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

            var allfailure = new Dictionary<string, bool>();
            var totalret = VcselMonthData.RetrieveTotleFailure(sdate, edate, vtype, allfailure);
            foreach (var fkv in allfailure)
            {
                if (!name_colors.ContainsKey(fkv.Key))
                {
                    name_colors.Add(fkv.Key, colors[color_idx % colors.Count]);
                    color_idx++;
                }
            }
            var totledatewafercolumlist = new List<DateWaferFailureColumn>();
            var totledatelist = totalret.Keys.ToList();
            totledatelist.Sort(delegate (string obj1, string obj2)
            {
                var d1 = DateTime.Parse(obj1);
                var d2 = DateTime.Parse(obj2);
                return d1.CompareTo(d2);
            });
            foreach (var d in totledatelist)
            {
                foreach (var item in totalret[d].DateColSeg)
                {
                    item.color = name_colors[item.name];
                }
                totledatewafercolumlist.Add(totalret[d]);
            }

            ret.Add(testylist);
            ret.Add(testflist);
            ret.Add(name_colors);
            ret.Add(totledatewafercolumlist);

            return ret;
        }

        public static List<object> RetrieveHTOLMonthlyData(DateTime sdate, DateTime edate, string vtype, Dictionary<string, bool> testnamedict)
        {
            var ret = new List<object>();

            //<testname,<date,<failure,cnt>>>
            var alldata = VcselMonthData.RetrieveMonthlyData(sdate, edate, vtype, "HTOLMonthData");

            var testylist = new List<testtypeyield>();
            var testflist = new List<TestFailureColumn>();


            var name_colors = new Dictionary<string, string>();
            var colors = FMColor();
            var color_idx = 0;

            foreach (var testkv in alldata)
            {
                if (!testnamedict.ContainsKey(testkv.Key.ToUpper()))
                { continue; }
                    
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
                    double yield = (double)pass / ((double)pass + fail) * 100.0;
                    dateylist.Add(new DateYield(datekv.Key, yield, (double)(pass + fail)));

                    failseglist.Sort(delegate (FailureColumnSeg obj1, FailureColumnSeg obj2)
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
                        item.y = Math.Round(item.y / totalfailurecount * 100, 3);
                    }


                    var datefailcol = new DateWaferFailureColumn();
                    datefailcol.xkey = datekv.Key;
                    datefailcol.DateColSeg = alignmentedfailseglist;

                    dateflist.Add(datefailcol);
                }//end foreach

                dateylist.Sort(delegate (DateYield obj1, DateYield obj2) {
                    return DateTime.Parse(obj1.date).CompareTo(DateTime.Parse(obj2.date));
                });

                dateflist.Sort(delegate (DateWaferFailureColumn obj1, DateWaferFailureColumn obj2)
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

        public static List<string> WaferNOList(string defTab = "VcselTimeRange")
        {
            return VcselTimeRange.WaferNOList(defTab);
        }

        public static List<object> RetrieveBURNINWaferData(List<string> wflist,bool withfaileddata, string vtype = "")
        {
            var ret = new List<object>();
            var boxdata = WaferBGDField.RetriveBURNINHTOLFieldData(wflist, withfaileddata, vtype);

            var waferfailure =  WaferTestSum.RetriveWaferFailure(wflist,vtype);
            var name_colors = new Dictionary<string, string>();
            var colors = FMColor();
            var color_idx = 0;
            var testflist = new List<TestFailureColumn>();

            foreach (var testkv in waferfailure)
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
                    waferf.total = totalcount;

                    waferflist.Add(waferf);

                }//end foreach

                var temptestf = new TestFailureColumn();
                temptestf.TestType = testkv.Key;
                waferflist.Sort(delegate(DateWaferFailureColumn obj1, DateWaferFailureColumn obj2) {
                    return obj1.xkey.CompareTo(obj2.xkey);
                });
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
            //ret.Add(rawdata);
            return ret;
        }

        public static List<object> RetrieveHTOLWaferData(List<string> wflist, bool withfaileddata, Dictionary<string, bool> testnamedict, Dictionary<string, bool> fieldnamedict, string vtype = "")
        {
            var ret = new List<object>();

            //<fieldname,<wafer,datalist>>
            var boxdata = WaferBGDField.RetriveBURNINHTOLFieldData(wflist, withfaileddata, vtype, "HTOLWaferBGDField", "HTOLWaferTestSum");
            var newboxdata = new Dictionary<string, Dictionary<string, List<string>>>();
            foreach (var kv in boxdata)
            {
                if (fieldnamedict.ContainsKey(kv.Key))
                {
                    newboxdata.Add(kv.Key, kv.Value);
                }
            }

            //<testname,<wafer,<failure,cnt>>>
            var waferfailure = WaferTestSum.RetriveWaferFailure(wflist, vtype, "HTOLWaferTestSum");
            var name_colors = new Dictionary<string, string>();
            var colors = FMColor();
            var color_idx = 0;
            var testflist = new List<TestFailureColumn>();

            foreach (var testkv in waferfailure)
            {
                if (!testnamedict.ContainsKey(testkv.Key.ToUpper()))
                { continue; }

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
                        item.y = Math.Round(item.y / totalcount * 100, 3);
                    }

                    var waferf = new DateWaferFailureColumn();
                    waferf.xkey = waferkv.Key;
                    waferf.DateColSeg = failseglist;
                    waferf.total = totalcount;

                    waferflist.Add(waferf);

                }//end foreach

                var temptestf = new TestFailureColumn();
                temptestf.TestType = testkv.Key;
                waferflist.Sort(delegate (DateWaferFailureColumn obj1, DateWaferFailureColumn obj2) {
                    return obj1.xkey.CompareTo(obj2.xkey);
                });
                temptestf.DateColSeg = waferflist;

                testflist.Add(temptestf);
            }//end foreach

            testflist.Sort(delegate (TestFailureColumn obj1, TestFailureColumn obj2)
            {
                return obj2.TestType.CompareTo(obj1.TestType);
            });
            ret.Add(newboxdata);
            ret.Add(testflist);
            ret.Add(name_colors);

            return ret;
        }

        //public static void RetrieveBURNINHTOLDataByWafer(string wf_no, System.IO.FileStream fw, string defTab = "BITestResultDataField", string defTab2 = "BITestResult")
        //{

        //    var sql = @"SELECT bf.SN,bf.TestName,bf.TestTimeStamp,bf.PN,bf.Wafer,bf.JO,bf.Channel,bf.SLOPE,bf.PO_LD,bf.PO_Uniformity,bf.THOLD,bf.Delta_PO_LD,bf.Delta_SLOPE
        //                , bf.Delta_THOLD, bf.Delta_PO_Uniformity, bf.ProductName, br.Failure FROM <defTab> bf 
        //                 left join <defTab2> br on br.DataID = bf.DataID 
        //                 where bf.Wafer = @Wafer";

        //    sql = sql.Replace("<defTab>", defTab).Replace("<defTab2>", defTab2);

        //    var dict = new Dictionary<string, string>();
        //    dict.Add("@Wafer", wf_no);
        //    var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);

        //    var sb = new StringBuilder(120 * 10000);
        //    var title = "SN,TestName,Failure,TestTimeStamp,PN,Wafer,JO,Channel,SLOPE,PO_LD,PO_Uniformity,THOLD,Delta_PO_LD,Delta_SLOPE,Delta_THOLD, Delta_PO_Uniformity, ProductName";
        //    sb.Append(title + "\r\n");

        //    var idx = 0;
        //    foreach (var line in dbret)
        //    {
        //        var item = new BITestResultDataField(Convert.ToString(line[0]), Convert.ToString(line[1]), Convert.ToDateTime(line[2])
        //            , Convert.ToString(line[3]), Convert.ToString(line[4]), Convert.ToString(line[5])
        //            , Convert.ToString(line[6]), Convert.ToDouble(line[7]), Convert.ToDouble(line[8])
        //            , Convert.ToDouble(line[9]), Convert.ToDouble(line[10]), Convert.ToDouble(line[11])
        //            , Convert.ToDouble(line[12]), Convert.ToDouble(line[13]), Convert.ToDouble(line[14])
        //            , Convert.ToString(line[15]));
        //        var failure = Convert.ToString(line[16]);

        //        var line1 = "\"" + item.SN.ToString().Replace("\"", "") + "\"," + "\"" + item.TestName.Replace("\"", "") + "\"," + "\"" + failure.Replace("\"", "")
        //            + "\"," + "\"" + item.TestTimeStamp.ToString("yyyy-MM-dd HH:mm:ss").Replace("\"", "") + "\"," + "\"" + item.PN.Replace("\"", "") + "\","
        //            + "\"" + item.Wafer.Replace("\"", "") + "\"," + "\"" + item.JO.Replace("\"", "") + "\"," + "\"" + item.Channel.Replace("\"", "") + "\","
        //            + "\"" + item.SLOPE.ToString().Replace("\"", "") + "\"," + "\"" + item.PO_LD.ToString().Replace("\"", "") + "\"," + "\"" + item.PO_Uniformity.ToString().Replace("\"", "") + "\","
        //            + "\"" + item.THOLD.ToString().Replace("\"", "") + "\"," + "\"" + item.Delta_PO_LD.ToString().Replace("\"", "") + "\"," + "\"" + item.Delta_SLOPE.ToString().Replace("\"", "") + "\","
        //            + "\"" + item.Delta_THOLD.ToString().Replace("\"", "") + "\"," + "\"" + item.Delta_PO_Uniformity.ToString().Replace("\"", "") + "\"," + "\"" + item.ProductName.Replace("\"", "") + "\",";
        //        sb.Append(line1 + "\r\n");
        //        idx = idx + 1;

        //        if (idx > 0 && idx % 10000 == 0)
        //        {
        //            var bt = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        //            fw.Write(bt, 0, bt.Count());
        //            sb.Clear();
        //        }
        //    }
        //    var bt1 = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        //    fw.Write(bt1, 0, bt1.Count());

        //}

        public static void RetrieveBURNINHTOLDataByWaferWithFilter(string wf_no, System.IO.FileStream fw,bool filter, string defTab = "BITestResultDataField", string defTab2 = "BITestResult")
        {
            var wfs = wf_no.Split(new[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);

            var sb1 = new StringBuilder();
            sb1.Append("('");
            foreach (var w in wfs)
            {
                sb1.Append(w + "','");
            }
            var WAFERCOND = sb1.ToString();
            WAFERCOND = WAFERCOND.Substring(0, WAFERCOND.Length - 2) + ")";

            var sql = @"SELECT bf.SN,bf.TestName,bf.TestTimeStamp,bf.PN,bf.Wafer,bf.JO,bf.Channel,bf.SLOPE,bf.PO_LD,bf.PO_Uniformity,bf.THOLD,bf.Delta_PO_LD,bf.Delta_SLOPE
                        , bf.Delta_THOLD, bf.Delta_PO_Uniformity, bf.ProductName, br.Failure,bf.DataID FROM <defTab> bf 
                         left join <defTab2> br on br.DataID = bf.DataID 
                         where bf.Wafer in <WAFERCOND> and br.Appv_1 <> 'DELETE' order by bf.Wafer,bf.SN,bf.TestTimeStamp DESC";

            sql = sql.Replace("<defTab>", defTab).Replace("<defTab2>", defTab2).Replace("<WAFERCOND>", WAFERCOND);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            if (!filter)
            {
                var sb = new StringBuilder(120 * 10000);
                var title = "Wafer,SN,TestName,Failure,TestTimeStamp,PN,JO,Channel,SLOPE,PO_LD,PO_Uniformity,THOLD,Delta_PO_LD,Delta_SLOPE,Delta_THOLD, Delta_PO_Uniformity, ProductName";
                sb.Append(title + "\r\n");

                var idx = 0;
                foreach (var line in dbret)
                {
                    var item = new BITestResultDataField(Convert.ToString(line[0]), Convert.ToString(line[1]), Convert.ToDateTime(line[2])
                        , Convert.ToString(line[3]), Convert.ToString(line[4]), Convert.ToString(line[5])
                        , Convert.ToString(line[6]), Convert.ToDouble(line[7]), Convert.ToDouble(line[8])
                        , Convert.ToDouble(line[9]), Convert.ToDouble(line[10]), Convert.ToDouble(line[11])
                        , Convert.ToDouble(line[12]), Convert.ToDouble(line[13]), Convert.ToDouble(line[14])
                        , Convert.ToString(line[15]));
                    var failure = Convert.ToString(line[16]);
                    var dataid = Convert.ToString(line[17]);

                    var line1 = "\"" + item.Wafer.Replace("\"", "") + "\"," +"\"" + item.SN.ToString().Replace("\"", "") + "\"," + "\"" + item.TestName.Replace("\"", "") + "\"," + "\"" + failure.Replace("\"", "")+ "\","
                         + "\"" + item.TestTimeStamp.ToString("yyyy-MM-dd HH:mm:ss").Replace("\"", "") + "\"," + "\"" + item.PN.Replace("\"", "") + "\","
                        +  "\"" + item.JO.Replace("\"", "") + "\"," + "\"" + item.Channel.Replace("\"", "") + "\","
                        + "\"" + item.SLOPE.ToString().Replace("\"", "") + "\"," + "\"" + item.PO_LD.ToString().Replace("\"", "") + "\"," + "\"" + item.PO_Uniformity.ToString().Replace("\"", "") + "\","
                        + "\"" + item.THOLD.ToString().Replace("\"", "") + "\"," + "\"" + item.Delta_PO_LD.ToString().Replace("\"", "") + "\"," + "\"" + item.Delta_SLOPE.ToString().Replace("\"", "") + "\","
                        + "\"" + item.Delta_THOLD.ToString().Replace("\"", "") + "\"," + "\"" + item.Delta_PO_Uniformity.ToString().Replace("\"", "") + "\"," + "\"" + item.ProductName.Replace("\"", "") + "\",";
                    sb.Append(line1 + "\r\n");
                    idx = idx + 1;

                    if (idx > 0 && idx % 10000 == 0)
                    {
                        var bt = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
                        fw.Write(bt, 0, bt.Count());
                        sb.Clear();
                    }
                }
                var bt1 = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
                fw.Write(bt1, 0, bt1.Count());
            }
            else
            {
                var sndict = new Dictionary<string, bool>();
                var filterdata = new List<BITestResultDataField>();
                foreach (var line in dbret)
                {
                    var item = new BITestResultDataField(Convert.ToString(line[0]), Convert.ToString(line[1]), Convert.ToDateTime(line[2])
                        , Convert.ToString(line[3]), Convert.ToString(line[4]), Convert.ToString(line[5])
                        , Convert.ToString(line[6]), Convert.ToDouble(line[7]), Convert.ToDouble(line[8])
                        , Convert.ToDouble(line[9]), Convert.ToDouble(line[10]), Convert.ToDouble(line[11])
                        , Convert.ToDouble(line[12]), Convert.ToDouble(line[13]), Convert.ToDouble(line[14])
                        , Convert.ToString(line[15]));
                    item.Appv_3 = Convert.ToString(line[16]);
                    item.Appv_4 = Convert.ToString(line[17]);

                    if (!sndict.ContainsKey(item.SN+"::"+item.TestName))
                    {
                       sndict.Add(item.SN + "::" + item.TestName, true);
                       filterdata.Add(item);
                    }
                }

                var sb = new StringBuilder(120 * 10000);
                var title = "Wafer,SN,TestName,Failure,TestTimeStamp,PN,JO,Channel,SLOPE,PO_LD,PO_Uniformity,THOLD,Delta_PO_LD,Delta_SLOPE,Delta_THOLD, Delta_PO_Uniformity, ProductName";
                sb.Append(title + "\r\n");

                var idx = 0;
                foreach (var item in filterdata)
                {
                    var line1 = "\"" + item.Wafer.Replace("\"", "") + "\"," + "\"" + item.SN.ToString().Replace("\"", "") + "\"," + "\"" + item.TestName.Replace("\"", "") + "\"," + "\"" + item.Appv_3.Replace("\"", "") + "\","
                         + "\"" + item.TestTimeStamp.ToString("yyyy-MM-dd HH:mm:ss").Replace("\"", "") + "\"," + "\"" + item.PN.Replace("\"", "") + "\","
                        + "\"" + item.JO.Replace("\"", "") + "\"," + "\"" + item.Channel.Replace("\"", "") + "\","
                        + "\"" + item.SLOPE.ToString().Replace("\"", "") + "\"," + "\"" + item.PO_LD.ToString().Replace("\"", "") + "\"," + "\"" + item.PO_Uniformity.ToString().Replace("\"", "") + "\","
                        + "\"" + item.THOLD.ToString().Replace("\"", "") + "\"," + "\"" + item.Delta_PO_LD.ToString().Replace("\"", "") + "\"," + "\"" + item.Delta_SLOPE.ToString().Replace("\"", "") + "\","
                        + "\"" + item.Delta_THOLD.ToString().Replace("\"", "") + "\"," + "\"" + item.Delta_PO_Uniformity.ToString().Replace("\"", "") + "\"," + "\"" + item.ProductName.Replace("\"", "") + "\",";
                    sb.Append(line1 + "\r\n");
                    idx = idx + 1;

                    if (idx > 0 && idx % 10000 == 0)
                    {
                        var bt = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
                        fw.Write(bt, 0, bt.Count());
                        sb.Clear();
                    }
                }
                var bt1 = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
                fw.Write(bt1, 0, bt1.Count());

            }//end else
        }

        public static void RetrieveBURNINHTOLDataByMonth(string month,List<string> pnlist,System.IO.FileStream fw,string defTab = "BITestResultDataField",string defTab2= "BITestResult")
        {
            var pncond = "('";
            foreach (var p in pnlist)
            {
                pncond = pncond + p + "','";
            }
            pncond = pncond.Substring(0, pncond.Length - 2) + ")";

            var starttime = DateTime.Parse(month + "/01" + " 00:00:00");
            var endtime = starttime.AddMonths(1);

            var sql = @"SELECT bf.SN,bf.TestName,bf.TestTimeStamp,bf.PN,bf.Wafer,bf.JO,bf.Channel,bf.SLOPE,bf.PO_LD,bf.PO_Uniformity,bf.THOLD,bf.Delta_PO_LD,bf.Delta_SLOPE
                        , bf.Delta_THOLD, bf.Delta_PO_Uniformity, bf.ProductName, br.Failure FROM <defTab> bf 
                        left join <defTab2> br on br.DataID = bf.DataID 
                        where bf.TestTimeStamp >= @starttime and bf.TestTimeStamp < @endtime and bf.Wafer <> '' and bf.ProductName in <pncond>";

            sql = sql.Replace("<defTab>",defTab).Replace("<defTab2>", defTab2).Replace("<pncond>", pncond);
            var dict = new Dictionary<string, string>();
            dict.Add("@starttime", starttime.ToString("yyyy-MM-dd HH:mm:ss"));
            dict.Add("@endtime", endtime.ToString("yyyy-MM-dd HH:mm:ss"));

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);

            var sb = new StringBuilder(120*10000);
            var title = "SN,TestName,Failure,TestTimeStamp,PN,Wafer,JO,Channel,SLOPE,PO_LD,PO_Uniformity,THOLD,Delta_PO_LD,Delta_SLOPE,Delta_THOLD, Delta_PO_Uniformity, ProductName";
            sb.Append(title + "\r\n");

            var idx = 0;
            foreach (var line in dbret)
            {
                var item = new BITestResultDataField(Convert.ToString(line[0]), Convert.ToString(line[1]), Convert.ToDateTime(line[2])
                    , Convert.ToString(line[3]), Convert.ToString(line[4]), Convert.ToString(line[5])
                    , Convert.ToString(line[6]), Convert.ToDouble(line[7]), Convert.ToDouble(line[8])
                    , Convert.ToDouble(line[9]), Convert.ToDouble(line[10]), Convert.ToDouble(line[11])
                    , Convert.ToDouble(line[12]), Convert.ToDouble(line[13]), Convert.ToDouble(line[14])
                    , Convert.ToString(line[15]));
                var failure = Convert.ToString(line[16]);

                var line1 = "\"" + item.SN.ToString().Replace("\"", "") + "\"," + "\"" + item.TestName.Replace("\"", "") + "\"," + "\"" + failure.Replace("\"", "")
                    + "\"," + "\"" + item.TestTimeStamp.ToString("yyyy-MM-dd HH:mm:ss").Replace("\"", "") + "\"," + "\"" + item.PN.Replace("\"", "") + "\","
                    + "\"" + item.Wafer.Replace("\"", "") + "\"," + "\"" + item.JO.Replace("\"", "") + "\"," + "\"" + item.Channel.Replace("\"", "") + "\","
                    + "\"" + item.SLOPE.ToString().Replace("\"", "") + "\"," + "\"" + item.PO_LD.ToString().Replace("\"", "") + "\"," + "\"" + item.PO_Uniformity.ToString().Replace("\"", "") + "\","
                    + "\"" + item.THOLD.ToString().Replace("\"", "") + "\"," + "\"" + item.Delta_PO_LD.ToString().Replace("\"", "") + "\"," + "\"" + item.Delta_SLOPE.ToString().Replace("\"", "") + "\","
                    + "\"" + item.Delta_THOLD.ToString().Replace("\"", "") + "\"," + "\"" + item.Delta_PO_Uniformity.ToString().Replace("\"", "") + "\"," + "\"" + item.ProductName.Replace("\"", "") + "\",";
                sb.Append(line1 + "\r\n");
                idx = idx + 1;

                if (idx > 0 && idx % 10000 == 0)
                {
                    var bt = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
                    fw.Write(bt, 0, bt.Count());
                    sb.Clear();
                }
            }
            var bt1 = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            fw.Write(bt1, 0, bt1.Count());
        }


        public static List<object> CBOXFromRaw(string rawdata, double llimit, double hlimit,bool nooutlier,bool negtivedata=false)
        {
            var ret = new List<object>();
            var listdata = (List<double>)Newtonsoft.Json.JsonConvert.DeserializeObject(rawdata, (new List<double>()).GetType());
            listdata.Sort();
            if (negtivedata)
            {
                var templist = new List<double>();
                templist.AddRange(listdata);
                listdata.Clear();
                foreach (var v in templist)
                {
                    listdata.Add(0.0 - v);
                }
                listdata.Sort();
            }

            var cbox = WaferBGDField.GetBoxData(listdata, llimit, hlimit);

            var outlierlist = new List<VXVal>();

            if (!nooutlier)
            {
                var rad = new System.Random(DateTime.Now.Second);
                var idx = 0;
                foreach (var data in listdata)
                {
                    if ((data > llimit && data < cbox.min)
                        || (data < hlimit && data > cbox.max))
                    {
                        var tempdata = new VXVal();
                        tempdata.ival = data;
                        if (idx % 2 == 0)
                        { tempdata.x = rad.NextDouble() / 5.0; }
                        else
                        { tempdata.x = 0-rad.NextDouble() / 5.0; }
                        outlierlist.Add(tempdata);
                        idx = idx + 1;
                    }
                }
            }

            ret.Add(cbox);
            ret.Add(outlierlist);
            return ret;
        }

        private static string Path2ImgTag(string filename)
        {
            var ret = "";
            var splitstr = filename.Split(new string[] { "userfiles" }, StringSplitOptions.RemoveEmptyEntries);
            var url = "http://wuxinpi.china.ads.finisar.com/" + "userfiles" + splitstr[1].Replace("\\", "/");
            ret = "<div style='text-align: center;'>" + "<img src='" + url + "' style='max-width: 90%; height: auto;' /></div>";
            return ret;
        }

        public static void SendVCSELBGDReport(Controller ctrl)
        {
            var cfgdict = CfgUtility.GetSysConfig(ctrl);

            var ToWho = new List<string>();

            var links = "";
            var urls = new List<string>();

            if (cfgdict.ContainsKey("VCSELREPORTTYPE"))
            {
                var BITypes = cfgdict["VCSELREPORTTYPE"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var t in BITypes)
                {
                    var u = "http://wuxinpi.china.ads.finisar.com/DataAnalyze/MonthlyVcsel?defaulttype=" + t.Trim();
                    urls.Add(u);
                    links = links + "<p>" + u + "</p>";
                }

                ToWho = CfgUtility.GetSysConfig(ctrl)["BIBGDREPORTRECIEVER"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            if (cfgdict.ContainsKey("HTOLREPORTTYPE"))
            {
                var HTOLTypes = cfgdict["HTOLREPORTTYPE"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var t in HTOLTypes)
                {
                    var u = "http://wuxinpi.china.ads.finisar.com/DataAnalyze/MonthlyHTOL?defaulttype=" + t.Trim();
                    urls.Add(u);
                    links = links + "<p>" + u + "</p>";
                }

                ToWho = CfgUtility.GetSysConfig(ctrl)["BIBGDREPORTRECIEVER"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            var fns = new List<string>();
            foreach (var url in urls)
            {
                string datestring = DateTime.Now.ToString("yyyyMMdd");
                string imgdir = ctrl.Server.MapPath("~/userfiles") + "\\docs\\" + datestring + "\\";
                if (!System.IO.Directory.Exists(imgdir))
                {
                    System.IO.Directory.CreateDirectory(imgdir);
                }
                var fn = "HTOL_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".jpg";
                var filename = imgdir + fn;
                WebsiteToImage websiteToImage = new WebsiteToImage(url, filename);
                websiteToImage.Generate();
                fns.Add(filename);
            }

            var imgtags = new List<string>();
            foreach (var f in fns)
            {
                if (System.IO.File.Exists(f)) { imgtags.Add(Path2ImgTag(f)); }
            }

            var content = EmailUtility.CreateImgHtml("Hi Guys", "Below is an burnin report from engineering system:", links, imgtags);
            EmailUtility.SendEmail(ctrl, "WUXI ENGINEERING SYSTEM BURN-IN REPORT", ToWho, content);
            new System.Threading.ManualResetEvent(false).WaitOne(1000);
        }
    }

}