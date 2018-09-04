using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
    public class CPKCache
    {
        private static void DeleteCPKParams(string PJName, string MESTab, string PNDesc, string Param)
        {
            var dict = new Dictionary<string, string>();
            dict.Add("@PJName", PJName);
            dict.Add("@MESTab", MESTab);
            dict.Add("@PNDesc", PNDesc);
            dict.Add("@Param", Param);

            var sql = "delete from CPKCache where PJName=@PJName and MESTab=@MESTab and PNDesc=@PNDesc and Param=@Param";
            DBUtility.ExeLocalSqlNoRes(sql, dict);

        }

        public static void UpdateCPKParams(string PJName, string MESTab,string PNDesc,string Param,string LowLimit,string HighLimit)
        {
            DeleteCPKParams(PJName, MESTab, PNDesc, Param);

            var dict = new Dictionary<string, string>();
            dict.Add("@PJName", PJName);
            dict.Add("@MESTab", MESTab);
            dict.Add("@PNDesc", PNDesc);
            dict.Add("@Param", Param);
            dict.Add("@LowLimit", LowLimit);
            dict.Add("@HighLimit", HighLimit);

            var sql = "insert into CPKCache(PJName,MESTab,PNDesc,Param,LowLimit,HighLimit) values(@PJName,@MESTab,@PNDesc,@Param,@LowLimit,@HighLimit)";
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public static List<string> RetrievePJList()
        {
            var ret = new List<string>();
            var sql = "select distinct PJName from CPKCache where PJName <> ''";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString(line[0]));
            }
            return ret;
        }

        public static List<string> RetrieveMESTabList(string pj)
        {
            var ret = new List<string>();
            var sql = "select distinct MESTab from CPKCache where PJName=@PJName and MESTab <> ''";
            var dict = new Dictionary<string, string>();
            dict.Add("@PJName", pj);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null,dict);
            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString(line[0]));
            }
            return ret;
        }

        public static List<string> RetrieveMESPNDesList(string pj)
        {
            var ret = new List<string>();
            var sql = "select distinct PNDesc from CPKCache where PJName=@PJName and PNDesc <> ''";
            var dict = new Dictionary<string, string>();
            dict.Add("@PJName", pj);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString(line[0]));
            }
            return ret;
        }

        public static List<string> RetrieveMESParamList(string mestab)
        {
            var ret = new List<string>();
            var sql = "select distinct Param from CPKCache where MESTab=@MESTab and Param <> ''";
            var dict = new Dictionary<string, string>();
            dict.Add("@MESTab", mestab);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString(line[0]));
            }
            return ret;
        }

        public static List<string> RetrieveMESLowLimitList(string param)
        {
            var ret = new List<string>();
            var sql = "select distinct LowLimit from CPKCache where Param=@Param and LowLimit <> ''";
            var dict = new Dictionary<string, string>();
            dict.Add("@Param", param);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString(line[0]));
            }
            return ret;
        }

        public static List<string> RetrieveMESHighLimitList(string param)
        {
            var ret = new List<string>();
            var sql = "select distinct HighLimit from CPKCache where Param=@Param and HighLimit <> ''";
            var dict = new Dictionary<string, string>();
            dict.Add("@Param", param);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString(line[0]));
            }
            return ret;
        }
    }

}