using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using log4net;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace Prometheus.Models
{
    public class LogHelper
    {
        public static void WriteLog(string logContent, Dictionary<string, string> cusProperties)
        {
            foreach(KeyValuePair<string, string> property in cusProperties)
            {
                log4net.LogicalThreadContext.Properties[property.Key] = property.Value;
            }
            WriteLog(null, logContent, Log4NetLevel.Error);
        }
        
        public static void WriteLog(string logContent, Log4NetLevel log4Level, Dictionary<string, string> cusProperties)
        {
            foreach (KeyValuePair<string, string> property in cusProperties)
            {
                log4net.LogicalThreadContext.Properties[property.Key] = property.Value;
            }
            WriteLog(null, logContent, log4Level);
        }
        
        public static void WriteLog(Type type, string logContent, Log4NetLevel log4Level)
        {
            ILog log = type == null ? LogManager.GetLogger("") : LogManager.GetLogger(type);

            switch (log4Level)
            {
                case Log4NetLevel.Warn:
                    log.Warn(logContent);
                    break;
                case Log4NetLevel.Debug:
                    log.Debug(logContent);
                    break;
                case Log4NetLevel.Info:
                    log.Info(logContent);
                    break;
                case Log4NetLevel.Fatal:
                    log.Fatal(logContent);
                    break;
                case Log4NetLevel.Error:
                    log.Error(logContent);
                    break;
            }
        }

    }

    public enum Log4NetLevel
    {
        [Description("Warning")]
        Warn = 1,
        [Description("Debug")]
        Debug = 2,
        [Description("Info")]
        Info = 3,
        [Description("Fatal")]
        Fatal = 4,
        [Description("Error")]
        Error = 5
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
        public string LogType { set; get; }
        public string LogLevel { set; get; }
        public string Message { set; get; }
        public string Date { set; get; }

        public static void WriteLog(string uName, string pKey, string machine, string url, 
                string oModule, string op, string iKey, string lType, Log4NetLevel lLevel, string msg)
        {
            var dic = new Dictionary<string, string>();
            dic.Add("uname", uName);
            dic.Add("pkey", pKey);
            dic.Add("machine", machine);
            dic.Add("url", url);
            dic.Add("module", oModule);
            dic.Add("operate", op);
            dic.Add("ikey", iKey);
            dic.Add("ltype", lType);
            LogHelper.WriteLog(msg, lLevel, dic);
        }
    }
}
