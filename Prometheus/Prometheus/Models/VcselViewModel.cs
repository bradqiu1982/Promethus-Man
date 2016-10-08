using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
    public class VcselViewModel
    {
        public List<string> row { set; get; }

        public static void StoreVcselData(List<string> line)
        {
            var sql = "delete from VCSELUsageTable where Module_Desc='<v0>' and Module_AgilePN='<v2>' and VCSEL_AgilePN = '<v7>'";
            sql = sql.Replace("<v0>", line[0]).Replace("<v2>", line[2]).Replace("<v7>", line[7]);
            DBUtility.ExeLocalSqlNoRes(sql);

            sql = "insert into VCSELUsageTable values (<v>)";
            var valuestring = string.Empty;
            for (var i = 0; i < 40; i++)
            {
                valuestring = valuestring + "'" + line[i] + "',";
            }
            valuestring = valuestring.Substring(0, valuestring.Length - 1);
            sql = sql.Replace("<v>", valuestring);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static List<List<string>> RetrieveVcselData()
        {
            var ret = new List<List<string>>();
            var sql = "select * from VCSELUsageTable";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var templine = new List<string>();
                foreach (var item in line)
                {
                    templine.Add(Convert.ToString(item));
                }
                ret.Add(templine);
            }
            return ret;
        }


    }
}