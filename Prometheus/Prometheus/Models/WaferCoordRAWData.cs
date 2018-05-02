using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
    public class WaferCoordDataStatus
    {
        public static string Valid = "1";
        public static string Invalid = "2";
    }
    public class WaferCoordRAWData
    {
        public string ID { set; get; }
        public string SN { set; get; }
        public string Coord_X { set; get; }
        public string Coord_Y { set; get; }
        public string Wafer_No { set; get; }
        public string Bin { set; get; }
        public string TestTime { set; get; }
        public string SyncTime { set; get; }

        public static Dictionary<string, WaferCoordRAWData> GetWaferCoordData(string uniquekeys = "", string SN = "", string sTestTime = "", string eTestTime = "")
        {
            var sql = @"select ID, SN, Coord_X, Coord_Y, Wafer_No, Bin, TestTime, SyncTime 
                    from WaferCoordData where ApVal1 = @Status ";
            var param = new Dictionary<string, string>();
            param.Add("@Status", WaferCoordDataStatus.Valid);
            if (!string.IsNullOrEmpty(uniquekeys))
            {
                sql += " and Concat(SN, ':', Coord_X, ':', Coord_Y) in <#uniquekeys> ";
                sql = sql.Replace("<#uniquekeys>", "(" + uniquekeys + ")");
            }
            if (!string.IsNullOrEmpty(SN))
            {
                sql += " and SN in <#SN> ";
                sql = sql.Replace("<#SN>", "(" + SN + ")");
            }
            if (!string.IsNullOrEmpty(sTestTime))
            {
                sql += " and TestTime >= @sTestTime ";
                param.Add("@sTestTime", sTestTime);
            }
            if (!string.IsNullOrEmpty(eTestTime))
            {
                sql += " and TestTime <= @eTestTime ";
                param.Add("@eTestTime", eTestTime);
            }
            sql += "order by TestTime Desc, SN ";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, param);
            var res = new Dictionary<string, WaferCoordRAWData>();
            if (dbret.Count > 0)
            {
                foreach (var item in dbret)
                {
                    var tmp = new WaferCoordRAWData();
                    tmp.ID = Convert.ToString(item[0]);
                    tmp.SN = Convert.ToString(item[1]);
                    tmp.Coord_X = Convert.ToString(item[2]);
                    tmp.Coord_Y = Convert.ToString(item[3]);
                    tmp.Wafer_No = Convert.ToString(item[4]);
                    tmp.Bin = Convert.ToString(item[5]);
                    tmp.TestTime = Convert.ToString(item[6]);
                    tmp.SyncTime = Convert.ToString(item[7]);
                    if (!res.ContainsKey(tmp.SN + ":" + tmp.Coord_X + ":" + tmp.Coord_Y))
                    {
                        res.Add(tmp.SN + ":" + tmp.Coord_X + ":" + tmp.Coord_Y, tmp);
                    }
                }
            }

            return res;
        }

        public static List<WaferCoordRAWData> GetWaferInfoByKeys(Dictionary<string, WaferCoordRAWData> data)
        {
            var sns_arr = new Dictionary<string, WaferCoordRAWData>();
            foreach (var item in data)
            {
                if (!sns_arr.ContainsKey(item.Value.SN))
                {
                    sns_arr.Add(item.Value.SN, item.Value);
                }
            }
            var sns = "'" + string.Join("','", sns_arr.Keys) + "'";
            var sn_info = BIDataUtility.RealBIWaferBySN4WaferCoord(sns);
            foreach (var sn in sn_info)
            {
                if (sns_arr.ContainsKey(sn.Key))
                {
                    sns_arr[sn.Key].Wafer_No = sn.Value;
                }
            }
            var res = new Dictionary<string, WaferCoordRAWData>();
            for (var m = 1; m <= Math.Ceiling(sns_arr.Count / 10.0); m++)
            {
                var sql = @"select distinct AppV_A as Wafer, AppV_D as Coord_X, 
                AppV_E as Coord_Y, AppV_G as Bin
                from NeoMapData where 1 = 1 ";
                var cond = "";
                var idx = 0;
                foreach (var sn in sns_arr)
                {
                    if (idx >= (m - 1) * 10 && idx < m * 10)
                    {
                        if (!string.IsNullOrEmpty(sn.Value.Wafer_No))
                        {
                            cond += " (AppV_A = '" + sn.Value.Wafer_No + "' and AppV_D = '" + sn.Value.Coord_X + "' and AppV_E = '" + sn.Value.Coord_Y + "') or";
                        }
                    }
                    idx++;
                }
                if (string.IsNullOrEmpty(cond))
                {
                    continue;
                }
                else
                {
                    sql += " and (" + cond.Substring(0, cond.Length - 2) + ")";
                }
                var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
                if (dbret.Count > 0)
                {
                    foreach (var item in dbret)
                    {
                        var tmp = new WaferCoordRAWData();
                        tmp.Wafer_No = Convert.ToString(item[0]);
                        tmp.Coord_X = Convert.ToString(item[1]);
                        tmp.Coord_Y = Convert.ToString(item[2]);
                        tmp.Bin = Convert.ToString(item[3]);
                        if (!res.ContainsKey(tmp.Wafer_No + ":" + tmp.Coord_X + ":" + tmp.Coord_Y))
                        {
                            res.Add(tmp.Wafer_No + ":" + tmp.Coord_X + ":" + tmp.Coord_Y, tmp);
                        }
                    }
                }
            }
            foreach (var item in sns_arr)
            {
                var ukey = item.Value.Wafer_No + ":" + item.Value.Coord_X + ":" + item.Value.Coord_Y;
                if (res.ContainsKey(ukey))
                {
                    item.Value.Bin = res[ukey].Bin;
                }
                else
                {
                    item.Value.Bin = "";
                }
            }
            return sns_arr.Values.ToList();
        }

        public static void CreateWaferCoordData(List<WaferCoordRAWData> data)
        {
            var sql = @"insert into WaferCoordData 
                (SN, Coord_X, Coord_Y, Wafer_No, Bin, TestTime, SyncTime, APVal1, APVal4) values ";
            var param = new Dictionary<string, string>();
            var idx = 0;
            foreach (var item in data)
            {
                sql += "( @SN_" + idx + ", @Coord_X_" + idx + ", @Coord_Y_" + idx
                    + ", @Wafer_No_" + idx + ", @Bin_" + idx + ", @TestTime_" + idx
                    + ", @SyncTime_" + idx + ", @ApVal1_" + idx + ", @ApVal4_" + idx + "),";
                param.Add("@SN_" + idx, item.SN);
                param.Add("@Coord_X_" + idx, item.Coord_X);
                param.Add("@Coord_Y_" + idx, item.Coord_Y);
                param.Add("@Wafer_No_" + idx, string.IsNullOrEmpty(item.Wafer_No) ? "" : item.Wafer_No);
                param.Add("@Bin_" + idx, item.Bin);
                param.Add("@TestTime_" + idx, item.TestTime);
                param.Add("@SyncTime_" + idx, item.SyncTime);
                param.Add("@ApVal1_" + idx, WaferCoordDataStatus.Valid);
                param.Add("@ApVal4_" + idx, item.SyncTime);
                idx++;
            }
            sql = sql.Substring(0, sql.Length - 1);
            DBUtility.ExeLocalSqlNoRes(sql, param);
        }

        public static string GetSyncMinTestTime()
        {
            var sql = @"SELECT Min(TestTime) as TestTime from WaferCoordData";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            var res = "";
            if (dbret.Count > 0)
            {
                return Convert.ToString(dbret[0][0]);
            }
            return res;
        }

        public static string GetSyncMaxTestTime()
        {
            var sql = @"SELECT Max(TestTime) as TestTime from WaferCoordData";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            var res = "";
            if (dbret.Count > 0)
            {
                return Convert.ToString(dbret[0][0]);
            }
            return res;
        }

        public static void UpdateWaferCoordData(string id, string waferno, string x, string y, string bin)
        {
            var sql = @"Update WaferCoordData set 
                Bin = @bin, 
                Wafer_No = @waferno, 
                Coord_X = @x, 
                Coord_Y = @y, 
                APVal4 = @uTime 
                where ID = @id ";
            var param = new Dictionary<string, string>();
            param.Add("@bin", bin);
            param.Add("@waferno", waferno);
            param.Add("@x", x);
            param.Add("@y", y);
            param.Add("@uTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            param.Add("@id", id);

            DBUtility.ExeLocalSqlNoRes(sql, param);
        }
        public static void InvalidWaferCoordData(string id, string status)
        {
            var sql = @"Update WaferCoordData set APVal1 = @status, APVal4 = @uTime where ID = @id ";
            var param = new Dictionary<string, string>();
            param.Add("@status", status);
            param.Add("@uTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            param.Add("@id", id);

            DBUtility.ExeLocalSqlNoRes(sql, param);
        }

    }
}