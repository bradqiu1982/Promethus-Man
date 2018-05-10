using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
    public class VcselRMAData
    {
        public VcselRMAData()
        {
            SN = "";
            BuildDate = DateTime.Parse("1982-05-06 10:00:00");
            Wafer = "";

            PN = "";
            PNDesc = "";
            VcselPN = "";
            VcselType = "";

            ProductType = "";
            ShipDate = "";
            RMAOpenDate = "";
            RMANum = "";
            Customer = "";
        }

        public static Dictionary<string, bool> GetAllVcselRMASN()
        {
            var ret = new Dictionary<string, bool>();
            var sql = "select distinct SN from VcselRMAData";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString(line[0]), true);
            }
            return ret;
        }

        public void StoreVcselRMA()
        {
            var sql = @"insert into VcselRMAData(SN,BuildDate,Wafer,PN,PNDesc,VcselPN,VcselType,ProductType,ShipDate,RMAOpenDate,RMANum,Customer)  
                        values(@SN,@BuildDate,@Wafer,@PN,@PNDesc,@VcselPN,@VcselType,@ProductType,@ShipDate,@RMAOpenDate,@RMANum,@Customer)";
            var dict = new Dictionary<string, string>();
            dict.Add("@SN", SN); dict.Add("@BuildDate", BuildDate.ToString("yyyy-MM-dd HH:mm:ss")); dict.Add("@Wafer", Wafer);
            dict.Add("@PN", PN); dict.Add("@PNDesc", PNDesc); dict.Add("@VcselPN", VcselPN);
            dict.Add("@VcselType", VcselType); dict.Add("@ProductType", ProductType); dict.Add("@ShipDate", ShipDate);
            dict.Add("@RMAOpenDate", RMAOpenDate); dict.Add("@RMANum", RMANum); dict.Add("@Customer", Customer);

            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public string SN { set; get; }
        public DateTime BuildDate { set; get; }
        public string Wafer { set; get; }


        public string PN { set; get; }
        public string PNDesc { set; get; }
        public string VcselPN { set; get; }
        public string VcselType { set; get; }

        public string ProductType { set; get; }
        public string ShipDate { set; get; }
        public string RMAOpenDate { set; get; }
        public string RMANum { set; get; }
        public string Customer { set; get; }
    }

    public class WaferSNMap
    {
        public WaferSNMap()
        {
            Wafer = "";
            SN = "";
            CurrentWorkStep = "";
            LastMoveDate = DateTime.Parse("1982-05-06 10:00:00");
        }

        public static bool WaferExist(string wafer)
        {
            var sql = "select top 1 Wafer from WaferSNMap where Wafer = @Wafer";
            var dict = new Dictionary<string, string>();
            dict.Add("@Wafer", wafer);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
            if (dbret.Count > 0)
            {
                return true;
            }
            return false;
        }

        public static void UpdateWaferInfo(string wafer)
        {
            if (!WaferExist(wafer))
            {
                var wafersnlist = BIDataUtility.RetrieveSNByWafer(wafer);
                foreach (var item in wafersnlist)
                {
                    item.StoreData();
                }
            }
        }

        public void StoreData()
        {
            var sql = "insert into WaferSNMap(Wafer,SN,CurrentWorkStep,LastMoveDate) values(@Wafer,@SN,@CurrentWorkStep,@LastMoveDate)";
            var dict = new Dictionary<string, string>();
            dict.Add("@Wafer",Wafer);
            dict.Add("@SN",SN);
            dict.Add("@CurrentWorkStep", CurrentWorkStep);
            dict.Add("@LastMoveDate", LastMoveDate.ToString("yyyy-MM-dd HH:mm:ss"));
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public string Wafer { set; get; }
        public string SN { set; get; }
        public string CurrentWorkStep { set; get; }
        public DateTime LastMoveDate { set; get; }
    } 



}