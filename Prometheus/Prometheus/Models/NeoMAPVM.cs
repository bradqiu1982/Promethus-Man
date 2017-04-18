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
            var sql = "select AppV_A from LoadedData where AppV_A = N'<AppV_A>' and AppV_B=N'<AppV_B>'";
            sql = sql.Replace("<AppV_A>", filename).Replace("<AppV_B>", SYSLOADFILETYPE.NEOMAP);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            if (dbret.Count == 0)
            {
                sql = "insert into LoadedData(AppV_A,AppV_B) values(N'<AppV_A>',N'<AppV_B>')";
                sql = sql.Replace("<AppV_A>", filename).Replace("<AppV_B>", SYSLOADFILETYPE.NEOMAP);
                DBUtility.ExeLocalSqlNoRes(sql);
            }//end if
        }

        public static Dictionary<string, bool> LoadedNeoMAPFile()
        {
            var ret = new Dictionary<string, bool>();

            var sql = "select AppV_A from LoadedData where AppV_B=N'<AppV_B>'";
            sql = sql.Replace("<AppV_B>", SYSLOADFILETYPE.NEOMAP);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString(line[0]), true);
            }

            return ret;
        }



    }
}