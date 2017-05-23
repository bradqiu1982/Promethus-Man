using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
    public class SameAsDBTVM
    {
        public string ErrorKey { set; get; }
        public DateTime LinkTime { set; get; }
        public string OriginalIssueKey { set; get; }
        public string SameAsIssueKey { set; get; }
        public string SameAsSN { set; get; }
        public DateTime SameAsTime { set; get; }

        public SameAsDBTVM()
        {
            ErrorKey = string.Empty;
            OriginalIssueKey = string.Empty;
            SameAsIssueKey = string.Empty;
            SameAsSN = string.Empty;
        }

        public static void StoreLinkDBTIssue(string errorkey,string linktime, string originalissuekey, string sameasissuekey, string sameassn)
        {
            var sql = "insert into SameAsDBTVM(ErrorKey,LinkTime,OriginalIssueKey,SameAsIssueKey,SameAsSN,SameAsTime) values('<ErrorKey>','<LinkTime>','<OriginalIssueKey>','<SameAsIssueKey>','<SameAsSN>','<SameAsTime>')";
            sql = sql.Replace("<ErrorKey>", errorkey).Replace("<LinkTime>", linktime).Replace("<OriginalIssueKey>", originalissuekey)
                .Replace("<SameAsIssueKey>", sameasissuekey).Replace("<SameAsSN>", sameassn).Replace("<SameAsTime>",DateTime.Now.ToString());
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static void StoreSameAsIssue(string originalissuekey, string sameasissuekey, string sameassn)
        {
            var sql = "select top 1 ErrorKey,LinkTime from SameAsDBTVM where OriginalIssueKey = '<OriginalIssueKey>' order by LinkTime DESC";
            sql = sql.Replace("<OriginalIssueKey>", originalissuekey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);

            if (dbret.Count > 0)
            {
                var errorkey = Convert.ToString(dbret[0][0]);
                var linktime = Convert.ToString(dbret[0][1]);
                StoreLinkDBTIssue(errorkey, linktime, originalissuekey, sameasissuekey, sameassn);
            }
        }

        public static int SameAsIssueCount(string errorkey, string linktime)
        {
            var sql = "select count(*) from SameAsDBTVM where ErrorKey = '<ErrorKey>' and LinkTime = '<LinkTime>'";
            sql = sql.Replace("<ErrorKey>", errorkey).Replace("<LinkTime>", linktime);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);

            if (dbret.Count > 0)
            {
                return Convert.ToInt32(dbret[0][0]);
            }
            else
            {
                return 0;
            }
        }

        public static List<SameAsDBTVM>  RetriveSameAsIssues(string errorkey, string linktime)
        {
            var ret = new List<SameAsDBTVM>();

            var sql = "select originalissuekey,SameAsIssueKey,SameAsSN from SameAsDBTVM where ErrorKey = '<ErrorKey>' and LinkTime = '<LinkTime>' order by SameAsTime DESC";
            sql = sql.Replace("<ErrorKey>", errorkey).Replace("<LinkTime>", linktime);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);

            if (dbret.Count > 0)
            {
                foreach (var line in dbret)
                {
                    var tempsameasdbvm = new SameAsDBTVM();
                    tempsameasdbvm.ErrorKey = errorkey;
                    tempsameasdbvm.LinkTime = DateTime.Parse(linktime);
                    tempsameasdbvm.OriginalIssueKey = Convert.ToString(line[0]);
                    tempsameasdbvm.SameAsIssueKey = Convert.ToString(line[1]);
                    tempsameasdbvm.SameAsSN = Convert.ToString(line[2]);
                    ret.Add(tempsameasdbvm);
                }
            }
            return ret;
        }

    }
}