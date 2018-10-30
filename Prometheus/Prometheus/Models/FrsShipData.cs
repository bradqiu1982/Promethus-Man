﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prometheus.Models
{
    public class SHIPPRODTYPE {
        public static string AZNA_CML = "AZNA-CML";
        public static string U2T = "U2T";
        public static string IPX = "IPX";
        public static string INTERLEAVE = "INTERLEAVE";
        public static string SYNTUNE = "SYNTUNE";
        public static string OPTIUM = "OPTIUM";
        public static string DWDMPASS = "DWDMPASS";
        public static string RED_C = "RED-C";
        public static string CWDMPASS = "CWDMPASS";
        public static string PARALLEL = "PARALLEL";
        public static string ICDR = "ICDR";
        public static string OTHER = "OTHER";
        public static string I32X = "I32X";
        public static string SMART_LDPA = "SMART LDPA";
    }

    public class FsrShipData
    {
        public FsrShipData()
        { }

        public FsrShipData(string id, int qty, string pn, string pndesc, string family, string cfg
            , DateTime shipdate, string custnum, string cust1, string cust2, DateTime orddate, string delievenum,int orderqty,DateTime opd)
        {
            ShipID = id;
            ShipQty = qty;
            PN = pn;
            ProdDesc = pndesc;
            MarketFamily = family;
            Configuration = cfg;
            ShipDate = shipdate;
            CustomerNum = custnum;
            Customer1 = cust1;
            Customer2 = cust2;
            OrderedDate = orddate;
            DelieveNum = delievenum;
            VcselType = string.Empty;
            SN = string.Empty;
            Wafer = string.Empty;
            OrderQty = orderqty;
            OPD = opd;
            OTD = "NO";
        }

        public static Dictionary<string, bool> RetrieveAllShipID()
        {
            var ret = new Dictionary<string, bool>();
            var sql = "select distinct ShipID from FsrShipData";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            { ret.Add(Convert.ToString(line[0]), true); }
            return ret;
        }

        public void StoreShipData()
        {
            var sql = @"insert into FsrShipData(ShipID,ShipQty,PN,ProdDesc,MarketFamily,Configuration,VcselType,ShipDate,CustomerNum,Customer1,Customer2,OrderedDate,DelieveNum,SN,Wafer,Appv_1,Appv_5) values(
                        @ShipID,@ShipQty,@PN,@ProdDesc,@MarketFamily,@Configuration,@VcselType,@ShipDate,@CustomerNum,@Customer1,@Customer2,@OrderedDate,@DelieveNum,@SN,@Wafer,@OrderQty,@OPD)";
            var dict = new Dictionary<string, string>();
            dict.Add("@ShipID", ShipID);
            dict.Add("@ShipQty", ShipQty.ToString());
            dict.Add("@PN", PN);
            dict.Add("@ProdDesc", ProdDesc);
            dict.Add("@MarketFamily", MarketFamily);
            dict.Add("@Configuration", Configuration);
            dict.Add("@VcselType", VcselType);
            dict.Add("@ShipDate", ShipDate.ToString("yyyy-MM-dd HH:mm:ss"));
            dict.Add("@CustomerNum", CustomerNum);
            dict.Add("@Customer1", Customer1);
            dict.Add("@Customer2", Customer2);
            dict.Add("@OrderedDate", OrderedDate.ToString("yyyy-MM-dd HH:mm:ss"));
            dict.Add("@DelieveNum", DelieveNum);
            dict.Add("@SN", SN);
            dict.Add("@Wafer", Wafer);
            dict.Add("@OrderQty", OrderQty.ToString());
            dict.Add("@OPD", OPD.ToString("yyyy-MM-dd HH:mm:ss"));
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public static void UpdateOPD(string shipid, string OPD)
        {
            var sql = "update FsrShipData set Appv_5=@OPD where ShipID=@ShipID";
            var dict = new Dictionary<string, string>();
            dict.Add("@ShipID", shipid);
            dict.Add("@OPD", OPD);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public static void UpdateOrderQty(string shipid, double orderqty)
        {
            var sql = "update FsrShipData set Appv_1=@OrderQty where ShipID=@ShipID";
            var dict = new Dictionary<string, string>();
            dict.Add("@ShipID", shipid);
            dict.Add("@OrderQty", orderqty.ToString());
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        private static string RetrieveCustome(string cust1,string cust2,Dictionary<string,string> custdict)
        {
            var ckeylist = custdict.Keys.ToList();
            var realcust = string.Empty;
            foreach (var c in ckeylist)
            {
                if (cust1.Contains(c) || cust2.Contains(c))
                {
                    realcust = c;
                }
            }
            if (string.IsNullOrEmpty(realcust))
            {
                return "OTHERS";
            }
            else
            {
                return custdict[realcust];
            }
        }

        //<date,<customer,int>>
        public static Dictionary<string,Dictionary<string,double>> RetrieveShipDataByMonth(string rate,string producttype,string sdate,string edate, Controller ctrl)
        {
            var ret = new Dictionary<string, Dictionary<string, double>>();
            var custdict = CfgUtility.GetShipCustConfig(ctrl, producttype);
            var sql = @"select ShipQty,Customer1,Customer2,ShipDate from FsrShipData where ShipDate >= @sdate and ShipDate <= @edate and Configuration = @producttype ";
            
            if(string.Compare(rate, VCSELRATE.r14G, true) == 0)
            { sql = sql + " and ( VcselType = '"+ VCSELRATE.r14G + "' or VcselType = '"+ VCSELRATE.r10G + "')"; }
            else
            { sql = sql + " and VcselType = '"+ rate + "'"; }

            var dict = new Dictionary<string, string>();
            dict.Add("@sdate", sdate);
            dict.Add("@edate", edate);
            dict.Add("@producttype", producttype);

            var realcustdict = new Dictionary<string, bool>();
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
            foreach (var line in dbret)
            {
                var shipdate = Convert.ToDateTime(line[3]).ToString("yyyy-MM");
                var qty = Convert.ToDouble(line[0]);
                var cust1 = Convert.ToString(line[1]).ToUpper();
                var cust2 = Convert.ToString(line[2]).ToUpper();
                var realcust = RetrieveCustome(cust1, cust2, custdict);

                if (!realcustdict.ContainsKey(realcust))
                { realcustdict.Add(realcust, true); }

                if (ret.ContainsKey(shipdate))
                {
                    var shipdict = ret[shipdate];
                    if (shipdict.ContainsKey(realcust))
                    { shipdict[realcust] = shipdict[realcust] + qty; }
                    else
                    { shipdict.Add(realcust, qty); }
                }
                else
                {
                    var custcntdict = new Dictionary<string, double>();
                    custcntdict.Add(realcust, qty);
                    ret.Add(shipdate, custcntdict);
                }
            }

            var shipdatelist = ret.Keys.ToList();
            var realcustlist = realcustdict.Keys.ToList();

            foreach (var sd in shipdatelist)
            {
                var custcntdict = ret[sd];
                foreach (var c in realcustlist)
                {
                    if (!custcntdict.ContainsKey(c))
                    {
                        custcntdict.Add(c, 0.0);
                    }
                }
            }

            return ret;
        }

        public static Dictionary<string, Dictionary<string, double>> RetrieveOrderDataByMonth(string rate, string producttype, string sdate, string edate, Controller ctrl)
        {
            var ret = new Dictionary<string, Dictionary<string, double>>();
            var custdict = CfgUtility.GetShipCustConfig(ctrl, producttype);
            var sql = @"select Appv_1,Customer1,Customer2,OrderedDate from FsrShipData where OrderedDate >= @sdate and OrderedDate <= @edate and Configuration = @producttype ";

            if (string.Compare(rate, VCSELRATE.r14G, true) == 0)
            { sql = sql + " and ( VcselType = '" + VCSELRATE.r14G + "' or VcselType = '" + VCSELRATE.r10G + "')"; }
            else
            { sql = sql + " and VcselType = '" + rate + "'"; }

            var dict = new Dictionary<string, string>();
            dict.Add("@sdate", sdate);
            dict.Add("@edate", edate);
            dict.Add("@producttype", producttype);

            var realcustdict = new Dictionary<string, bool>();
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
            foreach (var line in dbret)
            {
                var shipdate = Convert.ToDateTime(line[3]).ToString("yyyy-MM");
                var qty = Convert.ToDouble(line[0]);
                var cust1 = Convert.ToString(line[1]).ToUpper();
                var cust2 = Convert.ToString(line[2]).ToUpper();
                var realcust = RetrieveCustome(cust1, cust2, custdict);

                if (!realcustdict.ContainsKey(realcust))
                { realcustdict.Add(realcust, true); }

                if (ret.ContainsKey(shipdate))
                {
                    var shipdict = ret[shipdate];
                    if (shipdict.ContainsKey(realcust))
                    { shipdict[realcust] = shipdict[realcust] + qty; }
                    else
                    { shipdict.Add(realcust, qty); }
                }
                else
                {
                    var custcntdict = new Dictionary<string, double>();
                    custcntdict.Add(realcust, qty);
                    ret.Add(shipdate, custcntdict);
                }
            }

            var shipdatelist = ret.Keys.ToList();
            var realcustlist = realcustdict.Keys.ToList();

            foreach (var sd in shipdatelist)
            {
                var custcntdict = ret[sd];
                foreach (var c in realcustlist)
                {
                    if (!custcntdict.ContainsKey(c))
                    {
                        custcntdict.Add(c, 0.0);
                    }
                }
            }

            return ret;
        }

        public static List<FsrShipData> RetrieveOTDByMonth(string rate, string producttype, string sdate, string edate, Controller ctrl)
        {
            var ret = new List<FsrShipData>();
            var sql = @"select ShipDate,Appv_5,PN,ProdDesc,Appv_1,MarketFamily,ShipID from FsrShipData where Appv_5 >= @sdate and Appv_5 <= @edate and Configuration = @producttype 
                        and Customer1  not like '%FINISAR%' and Customer2 not like  '%FINISAR%' ";

            if (string.Compare(rate, VCSELRATE.r14G, true) == 0)
            {
                sql = sql + " and ( VcselType = '" + VCSELRATE.r14G + "' or VcselType = '" + VCSELRATE.r10G + "')";
            }
            else if (string.Compare(rate, VCSELRATE.r25G, true) == 0)
            {
                sql = sql + " and VcselType = '" + rate + "'";
            }

            var dict = new Dictionary<string, string>();
            dict.Add("@sdate", sdate);
            dict.Add("@edate", edate);
            dict.Add("@producttype", producttype);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
            foreach (var line in dbret)
            {
                var tempvm = new FsrShipData();
                tempvm.ShipDate = Convert.ToDateTime(line[0]);
                tempvm.OPD = Convert.ToDateTime(line[1]);
                tempvm.PN = Convert.ToString(line[2]);
                tempvm.ProdDesc = Convert.ToString(line[3]);
                tempvm.OrderQty = Convert.ToDouble(line[4]);
                tempvm.MarketFamily = Convert.ToString(line[5]);
                tempvm.ShipID = Convert.ToString(line[6]);
                tempvm.OTD = "NO";

                if (string.Compare(tempvm.OPD.ToString("yyyy-MM"), "1982-05") == 0)
                { continue; }

                ret.Add(tempvm);
            }

            return ret;
        }



        public static List<FsrShipData> RetrieveAllShipDataByMonth(string sdate, string edate, Controller ctrl)
        {
            var ret = new List<FsrShipData>();
            var custdict = CfgUtility.GetAllCustConfig(ctrl);
            var sql = @"select ShipID,ShipQty,PN,ProdDesc,MarketFamily,Configuration,ShipDate,CustomerNum,Customer1,Customer2,OrderedDate,DelieveNum,VcselType,Appv_1,Appv_5 
                         from FsrShipData where ShipDate >= @sdate and ShipDate <= @edate order by ShipDate ASC";

            var dict = new Dictionary<string, string>();
            dict.Add("@sdate", sdate);
            dict.Add("@edate", edate);

            var realcustdict = new Dictionary<string, bool>();
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
            foreach (var line in dbret)
            {
                var cust1 = Convert.ToString(line[8]).ToUpper();
                var cust2 = Convert.ToString(line[9]).ToUpper();
                var realcust = RetrieveCustome(cust1, cust2, custdict);
                var tempvm = new FsrShipData(Convert.ToString(line[0]), Convert.ToInt32(line[1]), Convert.ToString(line[2])
                    , Convert.ToString(line[3]), Convert.ToString(line[4]), Convert.ToString(line[5])
                    , Convert.ToDateTime(line[6]), Convert.ToString(line[7]), realcust
                    , Convert.ToString(line[9]), Convert.ToDateTime(line[10]), Convert.ToString(line[11]),Convert.ToInt32(line[13]),Convert.ToDateTime(line[14]));
                tempvm.VcselType = Convert.ToString(line[12]);
                ret.Add(tempvm);
            }

            return ret;
        }

        public string ShipID { set; get; }
        public double ShipQty { set; get; }
        public string PN { set; get; }
        public string ProdDesc { set; get; }
        public string MarketFamily { set; get; }
        public string Configuration { set; get; }
        public string VcselType { set; get; }
        public DateTime ShipDate { set; get; }
        public string ShipDateStr { get { return ShipDate.ToString("yyyy-MM-dd"); } }
        public string CustomerNum { set; get; }
        public string Customer1 { set; get; }
        public string Customer2 { set; get; }
        public DateTime OrderedDate { set; get; }
        public string DelieveNum { set; get; }
        public string SN { set; get; }
        public string Wafer { set; get; }
        public double OrderQty { set; get; }
        public DateTime OPD { set; get; }
        public string OPDStr { get { return OPD.ToString("yyyy-MM-dd"); } }
        public string OTD { set; get; }
    }
}