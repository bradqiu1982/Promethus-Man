using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
    public class PNDescCache
    {
        public static List<string> RetrievePNs(string pnkey)
        {
            var ret = new List<string>();
            var sql = "select pn,updatetime from PNDescCache where pnkey = '<pnkey>'";
            sql = sql.Replace("<pnkey>", pnkey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            var idx = 0;
            foreach (var line in dbret)
            {
                if (idx == 0)
                {
                    var updatetime = Convert.ToDateTime(line[1]);
                    if (updatetime.AddHours(24) < DateTime.Now)
                    {
                        sql = "delete from PNDescCache where pnkey = '<pnkey>'";
                        sql = sql.Replace("<pnkey>", pnkey);
                        DBUtility.ExeLocalSqlNoRes(sql);
                        ret.Clear();
                        return ret;
                    }
                }
                ret.Add(Convert.ToString(line[0]));
                idx = idx + 1;
            }
            return ret;
        }

        public static void StorePN(string pnkey, string pn)
        {
            var sql = "insert into PNDescCache(pnkey,pn,updatetime) values('<pnkey>','<pn>','<updatetime>')";
            sql = sql.Replace("<pnkey>", pnkey).Replace("<pn>", pn).Replace("<updatetime>", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            DBUtility.ExeLocalSqlNoRes(sql);
        }


    }
}