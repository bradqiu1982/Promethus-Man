using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
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


        public void StorePJCriticalError()
        {
            var sql = "insert into ProjectCriticalError(ProjectKey,ErrorCode,TestCaseName,MatchCond,WithLimit,LowLimit,HighLimit,WithAlgorithm,Algorithm,AlgorithmParam,Creater,Temperature,Channel) "
                + " values('<ProjectKey>','<ErrorCode>','<TestCaseName>','<MatchCond>',<WithLimit>,<LowLimit>,<HighLimit>,<WithAlgorithm>,'<Algorithm>','<AlgorithmParam>','<Creater>','<Temperature>','<Channel>')";

            sql = sql.Replace("<ProjectKey>", ProjectKey).Replace("<ErrorCode>", ErrorCode).Replace("<MatchCond>", MatchCond).Replace("<TestCaseName>", TestCaseName)
                .Replace("<WithLimit>", WithLimit.ToString()).Replace("<LowLimit>", LowLimit.ToString()).Replace("<HighLimit>", HighLimit.ToString())
                .Replace("<WithAlgorithm>", WithAlgorithm.ToString()).Replace("<Algorithm>", Algorithm).Replace("<AlgorithmParam>", AlgorithmParam)
                .Replace("<Creater>", Creater).Replace("<Temperature>", Temperature).Replace("<Channel>", Channel);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static void RemovePJCriticalError(string pjkey, string errorcode)
        {
            var sql = "delete from ProjectCriticalError where ProjectKey='<ProjectKey>' and ErrorCode='<ErrorCode>'";
            sql = sql.Replace("<ProjectKey>", pjkey).Replace("<ErrorCode>", errorcode);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static List<ProjectCriticalErrorVM> RetrievePJCriticalError(string pjkey,string errorcode)
        {
            var ret = new List<ProjectCriticalErrorVM>();
            var sql = string.Empty;
            if (!string.IsNullOrEmpty(errorcode))
            {
                sql = "select ProjectKey,ErrorCode,TestCaseName,MatchCond,WithLimit,LowLimit,HighLimit,WithAlgorithm,Algorithm,AlgorithmParam,Creater,Temperature,Channel from ProjectCriticalError where ProjectKey='<ProjectKey>' and ErrorCode='<ErrorCode>'";
                sql = sql.Replace("<ProjectKey>", pjkey).Replace("<ErrorCode>", errorcode);
            }
            else
            {
                sql = "select ProjectKey,ErrorCode,TestCaseName,MatchCond,WithLimit,LowLimit,HighLimit,WithAlgorithm,Algorithm,AlgorithmParam,Creater,Temperature,Channel from ProjectCriticalError where ProjectKey='<ProjectKey>'";
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
                ret.Add(tempvm);
            }
            return ret;
        }

    }
}