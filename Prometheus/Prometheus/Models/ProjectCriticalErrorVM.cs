using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
    public class TestTemperatureType
    {
        public static string Low = "Low";
        public static string High = "High";
        public static string Nomal = "Nomal";
    }

    public class PJCriticalAlgorithm
    {
        public static string MAXDELTA = "MAX DELTA";
        public static string UNIFORMITY = "UNIFORMITY";
        public static string STDDEV = "STDDEV";
    }

    public class ProjectCriticalErrorVM
    {
        public ProjectCriticalErrorVM()
        {
            ProjectKey = string.Empty;
            ErrorCode = string.Empty;
            TestCaseName = string.Empty;
            MatchCond = string.Empty;

            WithLimit = 0;
            LowLimit = -9999.0;
            HighLimit = 9999.0;
            WithAlgorithm = 0;

            Algorithm = string.Empty;
            AlgorithmParam = string.Empty;
            Creater = string.Empty;

            Temperature = string.Empty;
            Channel = string.Empty;

            Appv_1 = 0;
            Appv_2 = 0;
            Appv_3 = string.Empty;
            Appv_4 = string.Empty;

            SettingReason = string.Empty;
        }

        public string ProjectKey { set; get; }
        public string ErrorCode { set; get; }
        public string TestCaseName { set; get; }
        public string MatchCond { set; get; }
        public int WithLimit { set; get; } 
        public double LowLimit { set; get; }
        public double HighLimit { set; get; }
        public int WithAlgorithm { set; get; }
        public string Algorithm { set; get; }
        public string AlgorithmParam { set; get; }
        public string Creater { set; get; }
        public string Temperature { set; get; }
        public string Channel { set; get; }
        public double Appv_1 { set; get; }
        public double Appv_2 { set; get; }
        public string Appv_3 { set; get; }
        public string Appv_4 { set; get; }
        public DateTime Appv_5 { set; get; }
        public string SettingReason { set; get; }
        public string RuleID { set; get; }

        public void StorePJCriticalError()
        {
            if (RuleExist(ProjectKey,ErrorCode.Trim(),TestCaseName.Trim(),MatchCond.Trim(),LowLimit,HighLimit,Algorithm.Trim(), AlgorithmParam.Trim()))
            {
                return;
            }

            var sql = "insert into ProjectCriticalError(ProjectKey,ErrorCode,TestCaseName,MatchCond,WithLimit,LowLimit,HighLimit,WithAlgorithm,Algorithm,AlgorithmParam,Creater,Temperature,Channel,Appv_4,databackuptm,SettingReason,RuleID) "
                + " values('<ProjectKey>','<ErrorCode>','<TestCaseName>','<MatchCond>',<WithLimit>,<LowLimit>,<HighLimit>,<WithAlgorithm>,'<Algorithm>','<AlgorithmParam>','<Creater>','<Temperature>','<Channel>','<Appv_4>','<databackuptm>','<SettingReason>','<RuleID>')";

            sql = sql.Replace("<ProjectKey>", ProjectKey).Replace("<ErrorCode>", ErrorCode.Trim()).Replace("<MatchCond>", MatchCond.Trim()).Replace("<TestCaseName>", TestCaseName.Trim())
                .Replace("<WithLimit>", WithLimit.ToString()).Replace("<LowLimit>", LowLimit.ToString()).Replace("<HighLimit>", HighLimit.ToString())
                .Replace("<WithAlgorithm>", WithAlgorithm.ToString()).Replace("<Algorithm>", Algorithm.Trim()).Replace("<AlgorithmParam>", AlgorithmParam.Trim())
                .Replace("<Creater>", Creater).Replace("<Temperature>", Temperature).Replace("<Channel>", Channel).Replace("<Appv_4>", Appv_4)
                .Replace("<databackuptm>", DateTime.Now.ToString()).Replace("<SettingReason>", SettingReason.Trim()).Replace("<RuleID>",IssueViewModels.GetUniqKey());
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        private static bool RuleExist(string pjkey, string errorcode, string testcasename, string matchcond, double lowlimit, double highlimit, string Algorithm, string algorithmparam)
        {
            var sql = "select RuleID from ProjectCriticalError where ProjectKey = '<ProjectKey>' and ErrorCode = '<ErrorCode>' and TestCaseName = '<TestCaseName>' and MatchCond = '<MatchCond>' and LowLimit = <LowLimit> and HighLimit = <HighLimit> and Algorithm = '<Algorithm>' and AlgorithmParam = '<AlgorithmParam>'";
            sql = sql.Replace("<ProjectKey>", pjkey).Replace("<ErrorCode>", errorcode).Replace("<TestCaseName>", testcasename)
                .Replace("<MatchCond>", matchcond).Replace("<LowLimit>", lowlimit.ToString()).Replace("<HighLimit>", highlimit.ToString())
                .Replace("<Algorithm>", Algorithm).Replace("<AlgorithmParam>", algorithmparam);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            if (dbret.Count > 0)
            {
                return true;
            }

            return false;
        }

        public static void RemovePJCriticalError(string RuleID)
        {
            var sql = "delete from ProjectCriticalError where RuleID='<RuleID>'";
            sql = sql.Replace("<RuleID>", RuleID);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static void Open2ndCheckFASwitch(string RuleID)
        {
            var sql = "update ProjectCriticalError set Appv_1 = -99999 where RuleID='<RuleID>'";
            sql = sql.Replace("<RuleID>", RuleID);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static void Close2ndCheckFASwitch(string RuleID)
        {
            var sql = "update ProjectCriticalError set Appv_1 = 1  where RuleID='<RuleID>'";
            sql = sql.Replace("<RuleID>", RuleID);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static List<ProjectCriticalErrorVM> RetrievePJCriticalError(string pjkey,string errorcode)
        {
            var ret = new List<ProjectCriticalErrorVM>();
            if (string.IsNullOrEmpty(pjkey))
            {
                return ret;
            }

            var sql = string.Empty;
            if (!string.IsNullOrEmpty(errorcode))
            {
                sql = "select ProjectKey,ErrorCode,TestCaseName,MatchCond,WithLimit,LowLimit,HighLimit,WithAlgorithm,Algorithm,AlgorithmParam,Creater,Temperature,Channel,Appv_5,Appv_4,Appv_3,Appv_1,SettingReason,RuleID from ProjectCriticalError where ProjectKey='<ProjectKey>' and ErrorCode='<ErrorCode>'";
                sql = sql.Replace("<ProjectKey>", pjkey).Replace("<ErrorCode>", errorcode);
            }
            else
            {
                sql = "select ProjectKey,ErrorCode,TestCaseName,MatchCond,WithLimit,LowLimit,HighLimit,WithAlgorithm,Algorithm,AlgorithmParam,Creater,Temperature,Channel,Appv_5,Appv_4,Appv_3,Appv_1,SettingReason,RuleID from ProjectCriticalError where ProjectKey='<ProjectKey>'";
                sql = sql.Replace("<ProjectKey>", pjkey);
            }

            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            foreach (var line in dbret)
            {
                var tempvm = new ProjectCriticalErrorVM();
                tempvm.ProjectKey = Convert.ToString(line[0]);
                tempvm.ErrorCode = Convert.ToString(line[1]);
                tempvm.TestCaseName = Convert.ToString(line[2]);
                tempvm.MatchCond = Convert.ToString(line[3]);
                tempvm.WithLimit = Convert.ToInt32(line[4]);
                tempvm.LowLimit = Convert.ToDouble(line[5]);
                tempvm.HighLimit = Convert.ToDouble(line[6]);
                tempvm.WithAlgorithm = Convert.ToInt32(line[7]);
                tempvm.Algorithm = Convert.ToString(line[8]);
                tempvm.AlgorithmParam = Convert.ToString(line[9]);
                tempvm.Creater = Convert.ToString(line[10]);
                tempvm.Temperature = Convert.ToString(line[11]);
                tempvm.Channel = Convert.ToString(line[12]);
                tempvm.Appv_5 = Convert.ToDateTime(line[13]);
                tempvm.Appv_4 = Convert.ToString(line[14]);
                tempvm.Appv_3 = Convert.ToString(line[15]);
                tempvm.Appv_1 = Convert.ToDouble(line[16]);
                tempvm.SettingReason = Convert.ToString(line[17]);
                tempvm.RuleID = Convert.ToString(line[18]);

                if (string.IsNullOrEmpty(tempvm.RuleID))
                {
                    tempvm.RuleID = IssueViewModels.GetUniqKey();
                    UpdateRuleID(tempvm.RuleID, tempvm.ProjectKey, tempvm.ErrorCode
                        , tempvm.TestCaseName, tempvm.MatchCond, tempvm.LowLimit
                        , tempvm.HighLimit, tempvm.Algorithm, tempvm.AlgorithmParam);
                }

                ret.Add(tempvm);
            }
            return ret;
        }


        private static void UpdateRuleID(string ruleid,string pjkey, string errorcode, string testcasename, string matchcond, double lowlimit, double highlimit,string Algorithm, string algorithmparam)
        {
            var sql = "update ProjectCriticalError set RuleID = '<RuleID>' where ProjectKey = '<ProjectKey>' and ErrorCode = '<ErrorCode>' and TestCaseName = '<TestCaseName>' and MatchCond = '<MatchCond>' and LowLimit = <LowLimit> and HighLimit = <HighLimit> and Algorithm = '<Algorithm>' and AlgorithmParam = '<AlgorithmParam>'";
            sql = sql.Replace("<RuleID>",ruleid).Replace("<ProjectKey>",pjkey).Replace("<ErrorCode>", errorcode).Replace("<TestCaseName>", testcasename)
                .Replace("<MatchCond>", matchcond).Replace("<LowLimit>",lowlimit.ToString()).Replace("<HighLimit>", highlimit.ToString())
                .Replace("<Algorithm>", Algorithm).Replace("<AlgorithmParam>", algorithmparam);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public void UpdateMatchDateandTaskKey()
        {
            var sql = "update ProjectCriticalError set Appv_5 = '<MatchDate>',Appv_3 = '<issuekey>' where RuleID='<RuleID>'";
            sql = sql.Replace("<RuleID>", RuleID).Replace("<MatchDate>",Appv_5.ToString("yyyy-MM-dd hh:mm:ss")).Replace("<issuekey>", Appv_3);
            DBUtility.ExeLocalSqlNoRes(sql);
        }
    }
}