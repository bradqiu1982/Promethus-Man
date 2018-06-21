using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Prometheus.Models
{
    public class Log4NetLevel
    {
        public static string Warn = "WARNING";
        public static string Debug = "DEBUG";
        public static string Info = "INFO";
        public static string Fatal = "FATAL";
        public static string Error = "ERROR";
    }
    
    public class LogVM
    {
        public string ID { set; get; }
        public string UserName { set; get; }
        public string ProjectKey { set; get; }
        public string Machine { set; get; }
        public string Url { set; get; }
        public string OperateModule { set; get; }
        public string Operate { set; get; }
        public string IssueKey { set; get; }
        public int LogType { set; get; }
        public string LogLevel { set; get; }
        public string Message { set; get; }
        public string Date { set; get; }

        public static bool IsDebug()
        {
            bool debugging = false;
#if DEBUG
            debugging = true;
#else
            debugging = false;
#endif
            return debugging;
        }

        public static void WriteLog(string uName, string pKey, string machine, string url, 
                string oModule, string op, string iKey, int lType, string lLevel, string msg)
        {
            try
            {
                if (!IsDebug())
                {
                    var dic = new Dictionary<string, string>();
                    dic.Add("@uname", uName);
                    dic.Add("@pkey", pKey);
                    dic.Add("@machine", machine);
                    dic.Add("@url", url);
                    dic.Add("@module", oModule);
                    dic.Add("@operate", op);
                    dic.Add("@ikey", iKey);
                    dic.Add("@ltype", lType.ToString());
                    dic.Add("@llevel", lLevel);
                    dic.Add("@msg", msg);
                    dic.Add("@date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    dic.Add("@apval1","" );
                    dic.Add("@apval2", "");
                    dic.Add("@apval3", "");

                    var sql = @"INSERT INTO Log ([UserName],[ProjectKey],[Machine],[Url],[OperateModule],[Operate],[IssueKey],[LogType],[LogLevel],[Message],[Date],[APVal1],[APVal2],[APVal3])  
                                  VALUES(@uname, @pkey, @machine, @url, @module, @operate, @ikey, @ltype, @llevel, @msg, @date, @apval1, @apval2, @apval3)";

                    DBUtility.ExeLocalSqlNoRes(sql, dic);
                }
            }
            catch (Exception ex) { }
        }

        public static int GetChangeDueDateLogCnt(string iKey, int lType)
        {
            string sql = "select count(*) from Log "
                        + "where IssueKey = '<IssueKey>' "
                        + "and LogType = '<LogType>' ";
            sql = sql.Replace("<IssueKey>", iKey)
                    .Replace("<LogType>", lType.ToString());

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);

            return Convert.ToInt32(dbret[0][0]);
        }
    }

    public class LogType
    {
        public static int Default = 0;
        public static int Task = 1;
        public static int DebugTree = 2;
        public static int ICare = 3;
        public static int CancelICare = 4;
        public static int ModifyDueDate = 5;
        public static int OpenOrCloseProject = 6;
        public static int Login = 7;
    }

    public class ModifyDueDateConstant
    {
        public static int EditCnt = 3;
    }

}
