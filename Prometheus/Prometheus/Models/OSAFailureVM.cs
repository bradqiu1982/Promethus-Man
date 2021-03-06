﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
    public class OSAFAILUREPRIORITY
    {
        public static string NORMAL = "NORMAL";
        public static string CRITICAL = "CRITICAL";
    }

    public class OSAFailureVM
    {
        public OSAFailureVM()
        {
            ProjectKey = "";
            ParameterName = "";
            WhichTest = "";
            ErrorPriority = OSAFAILUREPRIORITY.NORMAL;
            LowLimit = -99999;
            HighLimit = 99999;
            FailureCode = "";
            FailureMode = "";
        }

        public OSAFailureVM(string pjkey,string param,string whichtest,string priority,double low,double high,string failcode,string failmode)
        {
            ProjectKey = pjkey;
            ParameterName = param;
            WhichTest = whichtest;
            if (string.Compare(priority.ToUpper(), OSAFAILUREPRIORITY.CRITICAL) == 0)
            {
                ErrorPriority = OSAFAILUREPRIORITY.CRITICAL;
            }
            else
            {
                ErrorPriority = OSAFAILUREPRIORITY.NORMAL;
            }
            LowLimit = low;
            HighLimit = high;
            FailureCode = failcode;
            FailureMode = failmode;
        }

        public static bool UpdateOSAFailureVM(List<OSAFailureVM> osafailurelist)
        {
            if (osafailurelist.Count > 0)
            {
                var sql = "delete from OSAFailureVM where ProjectKey = '<ProjectKey>'";
                sql = sql.Replace("<ProjectKey>", osafailurelist[0].ProjectKey);
                DBUtility.ExeLocalSqlNoRes(sql);

                var currenttime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                sql = "insert into OSAFailureVM(ProjectKey,ParameterName,WhichTest,ErrorPriority,LowLimit,HighLimit,FailureCode,FailureMode,databackuptm) values";
                var val = "('<ProjectKey>',N'<ParameterName>',N'<WhichTest>','<ErrorPriority>',<LowLimit>,<HighLimit>,N'<FailureCode>',N'<FailureMode>','<databackuptm>'),";
                foreach(var item in osafailurelist)
                {
                    sql = sql + val.Replace("<ProjectKey>", item.ProjectKey).Replace("<ParameterName>", item.ParameterName).Replace("<WhichTest>", item.WhichTest)
                        .Replace("<ErrorPriority>", item.ErrorPriority).Replace("<LowLimit>", item.LowLimit.ToString("0.000")).Replace("<HighLimit>", item.HighLimit.ToString("0.000"))
                        .Replace("<FailureCode>", item.FailureCode).Replace("<FailureMode>", item.FailureMode).Replace("<databackuptm>", currenttime);
                }
                sql = sql.Substring(0, sql.Length - 1);
                return DBUtility.ExeLocalSqlNoRes(sql);
            }

            return true;
        }

        public static Dictionary<string,OSAFailureVM> RetrieveAllOSAFailureVM(string ProjectKey)
        {
            var ret = new Dictionary<string, OSAFailureVM>();

            var sql = "select ProjectKey,ParameterName,WhichTest,ErrorPriority,LowLimit,HighLimit,FailureCode,FailureMode from OSAFailureVM where ProjectKey = '<ProjectKey>'";
            sql = sql.Replace("<ProjectKey>", ProjectKey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            foreach (var line in dbret)
            {
                try
                {
                    var tempvm = new OSAFailureVM();
                    tempvm.ProjectKey = Convert.ToString(line[0]);
                    tempvm.ParameterName = Convert.ToString(line[1]);
                    tempvm.WhichTest = Convert.ToString(line[2]);
                    tempvm.ErrorPriority = Convert.ToString(line[3]);
                    tempvm.LowLimit = Convert.ToDouble(line[4]);
                    tempvm.HighLimit = Convert.ToDouble(line[5]);
                    tempvm.FailureCode = Convert.ToString(line[6]);
                    tempvm.FailureMode = Convert.ToString(line[7]);

                    if (!ret.ContainsKey(tempvm.ParameterName+"_"+tempvm.WhichTest))
                    {
                        ret.Add((tempvm.ParameterName + "_" + tempvm.WhichTest).ToUpper(), tempvm);
                    }
                }
                catch (Exception ex) { }
            }

            return ret;
        }

        public string ProjectKey { set; get; }
        public string ParameterName { set; get; }
        public string WhichTest { set; get; }
        public string ErrorPriority { set; get; }
        public double LowLimit { set; get; }
        public double HighLimit { set; get; }
        public string FailureCode { set; get; }
        public string FailureMode { set; get; }
    }
}