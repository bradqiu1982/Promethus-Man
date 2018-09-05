﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
    public class CPKCache
    {
        private static void DeleteCPKParams(string PJName, string MESTab, string PNDesc, string Param,string CornerID)
        {
            var dict = new Dictionary<string, string>();
            dict.Add("@PJName", PJName);
            dict.Add("@MESTab", MESTab);
            dict.Add("@PNDesc", PNDesc);
            dict.Add("@Param", Param);
            dict.Add("@CornerID", CornerID);

            var sql = "delete from CPKCache where PJName=@PJName and MESTab=@MESTab and PNDesc=@PNDesc and Param=@Param and CornerID=@CornerID";
            DBUtility.ExeLocalSqlNoRes(sql, dict);

        }

        public static void UpdateCPKParams(string PJName, string MESTab,string PNDesc,string Param,string CornerID, string LowLimit,string HighLimit,string DataSource)
        {
            DeleteCPKParams(PJName, MESTab, PNDesc, Param, CornerID);

            var dict = new Dictionary<string, string>();
            dict.Add("@PJName", PJName);
            dict.Add("@MESTab", MESTab);
            dict.Add("@PNDesc", PNDesc);
            dict.Add("@Param", Param);
            dict.Add("@CornerID", CornerID);
            dict.Add("@LowLimit", LowLimit);
            dict.Add("@HighLimit", HighLimit);
            dict.Add("@DataSource", DataSource);

            var sql = "insert into CPKCache(PJName,MESTab,PNDesc,Param,CornerID,LowLimit,HighLimit,DataSource) values(@PJName,@MESTab,@PNDesc,@Param,@CornerID,@LowLimit,@HighLimit,@DataSource)";
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

            var pndict = new Dictionary<string, bool>();
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
            foreach (var line in dbret)
            {
                var pns = Convert.ToString(line[0]).Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var p in pns)
                {
                    if (!pndict.ContainsKey(p))
                    {
                        pndict.Add(p, true);
                        ret.Add(p);
                    }
                }
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

        public static List<string> RetrieveCornIDList(string param)
        {
            var ret = new List<string>();
            var sql = "select distinct CornerID from CPKCache where Param=@Param and CornerID <> ''";
            var dict = new Dictionary<string, string>();
            dict.Add("@Param", param);

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