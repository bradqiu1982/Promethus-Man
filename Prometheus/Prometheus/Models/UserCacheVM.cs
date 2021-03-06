﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
    public class UserCacheVM
    {

        public string UserName { set; get; }
        public string CacheInfo { set; get; }
        public DateTime UpdateTime { set; get; }

        public UserCacheVM()
        {
            UserName = string.Empty;
            CacheInfo = string.Empty;
        }

        public static void InsertCacheInfo(string username, string cacheinfo)
        {
            var sql = "delete from UserCacheInfo where UpdateTime < '<UpdateTime>'";
            sql = sql.Replace("<UpdateTime>", DateTime.Now.AddMonths(-2).ToString());
            DBUtility.ExeLocalSqlNoRes(sql);

            sql = "insert into UserCacheInfo(UserName,CacheInfo,UpdateTime,databackuptm) values('<UserName>','<CacheInfo>','<UpdateTime>','<databackuptm>')";
            sql = sql.Replace("<UserName>", username.ToUpper()).Replace("<CacheInfo>", cacheinfo).Replace("<UpdateTime>", DateTime.Now.ToString()).Replace("<databackuptm>", DateTime.Now.ToString());
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static List<UserCacheVM> RetrieveCacheInfo(string username)
        {
            var ret = new List<UserCacheVM>();

            var sql = "select top 10 UserName,CacheInfo,UpdateTime from UserCacheInfo where UserName = '<UserName>' order by UpdateTime DESC";
            sql = sql.Replace("<UserName>", username.ToUpper());
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);

            foreach (var line in dbret)
            {
                var tempinfo = new UserCacheVM();
                tempinfo.UserName = Convert.ToString(line[0]);

                string dummyData = Convert.ToString(line[1]).Trim().Replace(" ", "+");
                if (dummyData.Length % 4 > 0)
                    dummyData = dummyData.PadRight(dummyData.Length + 4 - dummyData.Length % 4, '=');

                var infobytes = Convert.FromBase64String(dummyData);
                tempinfo.CacheInfo = System.Text.Encoding.UTF8.GetString(infobytes);
                tempinfo.UpdateTime = DateTime.Parse(Convert.ToString(line[2]));
                ret.Add(tempinfo);
            }

            return ret;
        }

    }
}