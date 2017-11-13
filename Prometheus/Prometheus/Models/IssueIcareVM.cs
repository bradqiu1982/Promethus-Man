using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
    public class IssueIcareStatus
    {
        public static int Icare = 1;
        public static int CancelIcare = 0;
    }

    public class IssueIcareVM
    {
        public IssueIcareVM()
        {
            ID = "";
            ProjectKey = "";
            IssueKey = "";
            Icare = 0;
            CreateTime = DateTime.Now;
            UpdateTime = DateTime.Now;
        }

        public IssueIcareVM(string id, string pKey, string iKey, int icare, DateTime cTime, DateTime uTime)
        {
            ID = id;
            ProjectKey = pKey;
            IssueKey = iKey;
            Icare = icare;
            CreateTime = cTime;
            UpdateTime = uTime;
        }

        public string ID { set; get; }
        public string ProjectKey { set; get; }
        public string IssueKey { set; get; }
        public int Icare { set; get; }
        public DateTime CreateTime { set; get; }
        public DateTime UpdateTime { set; get; }

        public static IssueIcareVM GetIssueIcare(string pKey, string iKey)
        {
            var sql = "select ID, ProjectKey, IssueKey, Icare, CreateTime, UpdateTime "
                    + "from IssueIcare "
                    + "where ProjectKey = '<ProjectKey>' "
                    + "and IssueKey = '<IssueKey>' ";
            sql = sql.Replace("<ProjectKey>", pKey)
                    .Replace("<IssueKey>", iKey);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            var ret = new IssueIcareVM();
            foreach (var line in dbret)
            {
                ret.ID = Convert.ToString(line[0]);
                ret.ProjectKey = Convert.ToString(line[1]);
                ret.IssueKey = Convert.ToString(line[2]);
                ret.Icare = Convert.ToInt32(line[3]);
                ret.CreateTime = Convert.ToDateTime(line[4]);
                ret.UpdateTime = Convert.ToDateTime(line[5]);
            }

            return ret;
        }

        public static void AddIcare(string pKey, string iKey)
        {
            var insertsql = "insert into IssueIcare (ProjectKey, IssueKey, "
                        + "Icare, CreateTime, UpdateTime) values "
                        + "('<ProjectKey>', '<IssueKey>', '<Icare>', '<CreateTime>', '<UpdateTime>')";
            insertsql = insertsql.Replace("<ProjectKey>", pKey)
                    .Replace("<IssueKey>", iKey)
                    .Replace("<Icare>", IssueIcareStatus.Icare.ToString())
                    .Replace("<CreateTime>", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                    .Replace("<UpdateTime>", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            DBUtility.ExeLocalSqlNoRes(insertsql);
        }

        public static void UpdateIcare(string pKey, string iKey, int status)
        {
            var insertsql = "update IssueIcare set Icare = '<Icare>', "
                    + "UpdateTime = '<UpdateTime>' "
                    + "where ProjectKey = '<ProjectKey>' "
                    + "and IssueKey = '<IssueKey>'";
            insertsql = insertsql.Replace("<ProjectKey>", pKey)
                    .Replace("<IssueKey>", iKey)
                    .Replace("<Icare>", status.ToString())
                    .Replace("<UpdateTime>", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            DBUtility.ExeLocalSqlNoRes(insertsql);
        }

        public static void ICare(string pKey, string iKey)
        {
            var data = GetIssueIcare(pKey, iKey);
            if (string.IsNullOrEmpty(data.ID))
            {
                AddIcare(pKey, iKey);
            }
            else if(data.Icare == 0)
            {
                UpdateIcare(pKey, iKey, IssueIcareStatus.Icare);
            }
        }

        public static void CancelICare(string pKey, string iKey)
        {
            var data = GetIssueIcare(pKey, iKey);
            if ( ! string.IsNullOrEmpty(data.ID) && data.Icare == 1)
            {
                UpdateIcare(pKey, iKey, IssueIcareStatus.CancelIcare);
            }
        }
    }
}