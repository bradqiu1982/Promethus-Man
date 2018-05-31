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
            IssueKey = "";
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

        public static List<VcselRMAData> RetrieveWaferRawData(string wafer)
        {
            var ret = new List<VcselRMAData>();
            var sql = "select SN,BuildDate,Wafer,PN,PNDesc,VcselPN,VcselType,ProductType,ShipDate,RMAOpenDate,RMANum,Customer from VcselRMAData where Wafer = @Wafer";
            var dict = new Dictionary<string, string>();
            dict.Add("@Wafer", wafer);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
            foreach (var line in dbret)
            {
                var tempvm = new VcselRMAData();
                tempvm.SN = Convert.ToString(line[0]);
                tempvm.BuildDate = Convert.ToDateTime(line[1]);
                tempvm.Wafer = Convert.ToString(line[2]);

                tempvm.PN = Convert.ToString(line[3]);
                tempvm.PNDesc = Convert.ToString(line[4]);
                tempvm.VcselPN = Convert.ToString(line[5]);
                tempvm.VcselType = Convert.ToString(line[6]);

                tempvm.ProductType = Convert.ToString(line[7]);
                tempvm.ShipDate = Convert.ToString(line[8]);
                tempvm.RMAOpenDate = Convert.ToString(line[9]);
                tempvm.RMANum = Convert.ToString(line[10]);
                tempvm.Customer = Convert.ToString(line[11]);

                ret.Add(tempvm);
            }

            if (ret.Count > 0)
            {
                var sncond = "('";
                foreach (var item in ret)
                {
                    if (!string.IsNullOrEmpty(item.SN))
                    {
                        sncond = sncond + item.SN + "','";
                    }
                }
                sncond = sncond.Substring(0, sncond.Length - 2) + ")";
                var snkeydict = IssueViewModels.RetrieveIssueBySNs(sncond);
                foreach (var item in ret)
                {
                    if (snkeydict.ContainsKey(item.SN))
                    {
                        item.IssueKey = snkeydict[item.SN];
                    }
                }
            }

            return ret;
        }

        public static Dictionary<string, int> RetriveWaferCountDict()
        {
            var ret = new Dictionary<string, int>();
            var sql = "select count(*) as cnt,Wafer from VcselRMAData group by Wafer";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var wafer = Convert.ToString(line[1]);
                var cnt = Convert.ToInt32(line[0]);
                if (!ret.ContainsKey(wafer))
                {
                    ret.Add(wafer, cnt);
                }
            }
            return ret;
        }

        public static List<VcselRMAData> RetrieveDistinctWaferListASC(string rate)
        {
            var ret = new List<VcselRMAData>();
            var wdict = new Dictionary<string, bool>();
            var sql = "select Wafer,BuildDate,VcselType from VcselRMAData ";
            if (!string.IsNullOrEmpty(rate.Trim()))
            {
                sql = sql + "  where VcselType = '<VcselType>'  ";
                sql = sql.Replace("<VcselType>", rate);
            }
            sql = sql+" order by BuildDate ASC";

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var w = Convert.ToString(line[0]);
                if (!wdict.ContainsKey(w))
                {
                    wdict.Add(w, true);
                    var tempvm = new VcselRMAData();
                    tempvm.Wafer = w;
                    tempvm.BuildDate = Convert.ToDateTime(line[1]);
                    tempvm.VcselType = Convert.ToString(line[2]);
                    ret.Add(tempvm);
                }
            }
            return ret;
        }

        public static List<VcselRMAData> RetrievAllDataASC()
        {
            var ret = new List<VcselRMAData>();
            var sql = "select Wafer,BuildDate,VcselType,ShipDate,RMAOpenDate,SN from VcselRMAData order by BuildDate ASC";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var tempvm = new VcselRMAData();
                tempvm.Wafer = Convert.ToString(line[0]);
                tempvm.BuildDate = Convert.ToDateTime(line[1]);
                tempvm.VcselType = Convert.ToString(line[2]);
                tempvm.ShipDate = Convert.ToString(line[3]);
                tempvm.RMAOpenDate = Convert.ToString(line[4]);
                tempvm.SN = Convert.ToString(line[5]);
                ret.Add(tempvm);
            }
            return ret;
        }
        
        //return dict<month,dict<rate,count>>
        public static Dictionary<string,Dictionary<string,int>> RetrieveRMACountByBuildMonth(Dictionary<string, string> vtypedict)
        {
            var ret = new Dictionary<string, Dictionary<string, int>>();
            var vlist = RetrievAllDataASC();
            foreach (var v in vlist)
            {
                var month = v.BuildDate.ToString("yyyy-MM");
                if (!vtypedict.ContainsKey(v.VcselType))
                { vtypedict.Add(v.VcselType,""); }

                if (ret.ContainsKey(month))
                {
                    var ratedict = ret[month];
                    if (ratedict.ContainsKey(v.VcselType))
                    {
                        ratedict[v.VcselType] = ratedict[v.VcselType] + 1;
                    }
                    else
                    {
                        ratedict.Add(v.VcselType, 1);
                    }
                }
                else
                {
                    var ratedict = new Dictionary<string, int>();
                    ratedict.Add(v.VcselType, 1);
                    ret.Add(month, ratedict);
                }
            }

            //add type to all month 
            foreach (var r in ret)
            {
                foreach (var vt in vtypedict)
                {
                    if (!r.Value.ContainsKey(vt.Key))
                    {
                        r.Value.Add(vt.Key, 0);
                    }
                }
            }

            return ret;
        }

        public static Dictionary<string, Dictionary<string, int>> RetrieveRMACountByShipMonth(Dictionary<string, string> vtypedict)
        {
            var ret = new Dictionary<string, Dictionary<string, int>>();
            var vlist = RetrievAllDataASC();
            foreach (var v in vlist)
            {
                var month = DateTime.Parse(v.ShipDate).ToString("yyyy-MM");
                if (!vtypedict.ContainsKey(v.VcselType))
                { vtypedict.Add(v.VcselType, ""); }

                if (ret.ContainsKey(month))
                {
                    var ratedict = ret[month];
                    if (ratedict.ContainsKey(v.VcselType))
                    {
                        ratedict[v.VcselType] = ratedict[v.VcselType] + 1;
                    }
                    else
                    {
                        ratedict.Add(v.VcselType, 1);
                    }
                }
                else
                {
                    var ratedict = new Dictionary<string, int>();
                    ratedict.Add(v.VcselType, 1);
                    ret.Add(month, ratedict);
                }
            }

            //add type to all month 
            foreach (var r in ret)
            {
                foreach (var vt in vtypedict)
                {
                    if (!r.Value.ContainsKey(vt.Key))
                    {
                        r.Value.Add(vt.Key, 0);
                    }
                }
            }

            return ret;
        }

        public static List<string> RetrieveVcselType()
        {
            var sql = "select distinct VcselType from VcselRMAData where VcselType <> '' order by VcselType";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            var ret = new List<string>();
            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString(line[0]));
            }
            return ret;
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
        public string IssueKey { set; get; }
    }

    public class WaferSNMap
    {
        public WaferSNMap()
        {
            Wafer = "";
            SN = "";
            CurrentWorkStep = "";
            LastMoveDate = DateTime.Parse("1982-05-06 10:00:00");
            PN = "";
            PNDesc = "";
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
            var sql = "insert into WaferSNMap(Wafer,SN,CurrentWorkStep,LastMoveDate,PN,PNDesc) values(@Wafer,@SN,@CurrentWorkStep,@LastMoveDate,@PN,@PNDesc)";
            var dict = new Dictionary<string, string>();
            dict.Add("@Wafer", Wafer);
            dict.Add("@SN", SN);
            dict.Add("@CurrentWorkStep", CurrentWorkStep);
            dict.Add("@LastMoveDate", LastMoveDate.ToString("yyyy-MM-dd HH:mm:ss"));
            dict.Add("@PN", PN);
            dict.Add("@PNDesc", PNDesc);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public static Dictionary<string, int> RetriveWaferCountDict()
        {
            var ret = new Dictionary<string, int>();
            var sql = "select cnt,Wafer from (select count(*) as cnt,Wafer from WaferSNMap group by Wafer) as subquery where cnt >=50";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var wafer = Convert.ToString(line[1]);
                var cnt = Convert.ToInt32(line[0]);
                if (!ret.ContainsKey(wafer))
                {
                    ret.Add(wafer, cnt);
                }
            }
            return ret;
        }

        public string Wafer { set; get; }
        public string SN { set; get; }
        public string CurrentWorkStep { set; get; }
        public DateTime LastMoveDate { set; get; }
        public string PN { set; get; }
        public string PNDesc { set; get; }
    }

    public class VcselRMADPPM
    {
        public VcselRMADPPM()
        {
            Wafer = "";
            DPPM = 0.0;
            ShippedQty = 0.0;
        }

        public string Wafer { set; get; }
        public double DPPM { set; get; }
        public double ShippedQty { set; get; }
    }

    public class VcselRMASum {
        public static List<string> FMColor()
        {
            return new List<string> {
                "#ff3399","#0099ff","#00b050","#7030a0",
                "#105D9C","#23735D","#A55417","#821A08","#7030A0",
                "#0C779D","#34AC8B","#D85C00","#CC044D","#B925A7",
                "#4FADF3","#12CC92","#FA9604","#ED6161","#EF46FC",
                "#8CC9F7","#BEEBDF","#FDEEC3","#F6B0B0","#EC88F4"
            };
        }

        public static List<VcselRMADPPM> RetrieveVcselDPPM(string rate)
        {
            var ret = new List<VcselRMADPPM>();

            var wlist = VcselRMAData.RetrieveDistinctWaferListASC(rate);
            var rmacntdict = VcselRMAData.RetriveWaferCountDict();
            var wafercntdict = WaferSNMap.RetriveWaferCountDict();
            foreach (var w in wlist)
            {
                if (rmacntdict.ContainsKey(w.Wafer) && wafercntdict.ContainsKey(w.Wafer))
                {
                    var tempvm = new VcselRMADPPM();
                    tempvm.Wafer = w.Wafer;
                    tempvm.ShippedQty = wafercntdict[w.Wafer];
                    tempvm.DPPM = Math.Round((double)rmacntdict[w.Wafer] /(double)wafercntdict[w.Wafer] * 1000000,0);
                    ret.Add(tempvm);
                }
            }//end foreach
            return ret;
        }

        public static List<object> VcselRMAMileStoneDataByBuildDate()
        {
            var ret = new List<object>();
            var vtypedict = new Dictionary<string, string>();
            var rmacountdata = VcselRMAData.RetrieveRMACountByBuildMonth(vtypedict);
            ret.Add(rmacountdata);

            var milelist = EngineeringMileStone.RetrieveVcselMileStone();
            ret.Add(milelist);

            var colorlist = VcselRMASum.FMColor();
            var vtypekeylist = vtypedict.Keys.ToList();
            var cidx = 0;
            foreach (var vk in vtypekeylist)
            {
                vtypedict[vk] = colorlist[cidx % colorlist.Count];
                cidx = cidx + 1;
            }
            ret.Add(vtypedict);

            return ret;
        }

        public static List<object> VcselRMAMileStoneDataByShipDate()
        {
            var ret = new List<object>();
            var vtypedict = new Dictionary<string, string>();
            var rmacountdata = VcselRMAData.RetrieveRMACountByShipMonth(vtypedict);
            ret.Add(rmacountdata);

            var milelist = EngineeringMileStone.RetrieveVcselMileStone();
            ret.Add(milelist);

            var colorlist = VcselRMASum.FMColor();
            var vtypekeylist = vtypedict.Keys.ToList();
            var cidx = 0;
            foreach (var vk in vtypekeylist)
            {
                vtypedict[vk] = colorlist[cidx % colorlist.Count];
                cidx = cidx + 1;
            }
            ret.Add(vtypedict);

            return ret;
        }
    }

}