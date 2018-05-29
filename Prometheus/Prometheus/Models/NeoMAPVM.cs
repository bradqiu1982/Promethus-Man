using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
    public class SYSLOADFILETYPE
    {
        public static string NEOMAP = "NEOMAP";
    }

    public class NeoMAPVM
    {
        public static void UpdateNeoMAPFile(string filename)
        {
            var sql = "select AppV_A from FileLoadedData where AppV_A = N'<AppV_A>' and AppV_B=N'<AppV_B>'";
            sql = sql.Replace("<AppV_A>", filename).Replace("<AppV_B>", SYSLOADFILETYPE.NEOMAP);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            if (dbret.Count == 0)
            {
                sql = "insert into FileLoadedData(AppV_A,AppV_B,databackuptm) values(N'<AppV_A>',N'<AppV_B>','<databackuptm>')";
                sql = sql.Replace("<AppV_A>", filename).Replace("<AppV_B>", SYSLOADFILETYPE.NEOMAP).Replace("<databackuptm>", DateTime.Now.ToString());
                DBUtility.ExeLocalSqlNoRes(sql);
            }//end if
        }

        public static Dictionary<string, bool> LoadedNeoMAPFile()
        {
            var ret = new Dictionary<string, bool>();

            var sql = "select AppV_A from FileLoadedData where AppV_B=N'<AppV_B>'";
            sql = sql.Replace("<AppV_B>", SYSLOADFILETYPE.NEOMAP);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString(line[0]), true);
            }

            return ret;
        }


        public static List<Dictionary<string, string>> GetNeomapDataByConditions(string sn = "", string wafer_no = "", string coord_x = "", string coord_y = "")
        {
            var sql = @"select b.SN, a.* from NeoMapData as a 
                    left join WaferCoordData as b 
                    on (a.AppV_A = b.Wafer_No and a.AppV_D = b.Coord_X 
                    and a.AppV_E = b.Coord_Y)
                    where 1 = 1 ";
            var param = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(sn))
            {
                sql += " and b.SN like @sn ";
                param.Add("@sn", "%" + sn + "%");
            }
            if (!string.IsNullOrEmpty(wafer_no))
            {
                sql += " and a.AppV_A = @wafer_no ";
                param.Add("@wafer_no", wafer_no);
            }
            if (!string.IsNullOrEmpty(coord_x))
            {
                sql += " and a.AppV_D = @coord_x ";
                param.Add("@coord_x", coord_x);
            }
            if (!string.IsNullOrEmpty(coord_y))
            {
                sql += " and a.AppV_E = @coord_y ";
                param.Add("@coord_y", coord_y);
            }

            sql += "order by b.SN, a.AppV_A ";

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, param);
            var res = new List<Dictionary<string, string>>();
            if(dbret.Count > 0)
            {
                var neomap = ExternalDataCollector.NeoMapDBColName2RealName();
                foreach (var item in dbret)
                {
                    var tmp = new Dictionary<string, string>();
                    tmp.Add("SN", Convert.ToString(item[0]));
                    var idx = 1;
                    foreach (var line in neomap)
                    {
                        tmp.Add(line.Value, Convert.ToString(item[idx]));
                        idx++;
                    }
                    res.Add(tmp);
                }
            }
            return res;
        }
    }
}