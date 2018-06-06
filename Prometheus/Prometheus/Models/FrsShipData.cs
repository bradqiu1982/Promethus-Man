using System;
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
        public FsrShipData(string id, int qty, string pn, string pndesc, string family, string cfg
            , DateTime shipdate, string custnum, string cust1, string cust2, DateTime orddate, string delievenum)
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
            var sql = @"insert into FsrShipData(ShipID,ShipQty,PN,ProdDesc,MarketFamily,Configuration,VcselType,ShipDate,CustomerNum,Customer1,Customer2,OrderedDate,DelieveNum,SN,Wafer) values(
                        @ShipID,@ShipQty,@PN,@ProdDesc,@MarketFamily,@Configuration,@VcselType,@ShipDate,@CustomerNum,@Customer1,@Customer2,@OrderedDate,@DelieveNum,@SN,@Wafer)";
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
            var sql = @"select ShipQty,Customer1,Customer2,ShipDate from FsrShipData where ShipDate >= @sdate && ShipDate <= @edate and Configuration = @producttype ";
            if (string.Compare(rate, "25G", true) == 0)
            { sql = sql + " and VcselType = '25G'"; }
            else
            { sql = sql + " and VcselType <> '25G' and VcselType <> ''"; }

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

        public string ShipID { set; get; }
        public double ShipQty { set; get; }
        public string PN { set; get; }
        public string ProdDesc { set; get; }
        public string MarketFamily { set; get; }
        public string Configuration { set; get; }
        public string VcselType { set; get; }
        public DateTime ShipDate { set; get; }
        public string CustomerNum { set; get; }
        public string Customer1 { set; get; }
        public string Customer2 { set; get; }
        public DateTime OrderedDate { set; get; }
        public string DelieveNum { set; get; }
        public string SN { set; get; }
        public string Wafer { set; get; }
    }
}