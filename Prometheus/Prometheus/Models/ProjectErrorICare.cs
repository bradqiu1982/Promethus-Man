using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prometheus.Models
{
    public class ProjectErrorICareStatus
    {
        public static int Icare = 1;
        public static int CancelIcare = 0;
    }
    public class ProjectErrorICareVM
    {
        public ProjectErrorICareVM()
        {
            ID = "";
            ProjectKey = "";
            UserName = "";
            ErrorKey = "";
            Icare = 0;
            CreateTime = DateTime.Now;
            UpdateTime = DateTime.Now;
        }

        public ProjectErrorICareVM(string id, string pKey, string uName, string eKey, int icare, DateTime cTime, DateTime uTime)
        {
            ID = id;
            ProjectKey = pKey;
            UserName = uName;
            ErrorKey = eKey;
            Icare = icare;
            CreateTime = cTime;
            UpdateTime = uTime;
        }

        public string ID { set; get; }
        public string ProjectKey { set; get; }
        public string UserName { set; get; }
        public string ErrorKey { set; get; }
        public int Icare { set; get; }
        public DateTime CreateTime { set; get; }
        public DateTime UpdateTime { set; get; }

        public static ProjectErrorICareVM GetIssueIcare(string pKey, string eKey, string uName)
        {
            var sql = "select ID, ProjectKey, UserName, ErrorKey, Icare, CreateTime, UpdateTime "
                    + "from ProjectErrorICare "
                    + "where ProjectKey = '<ProjectKey>' "
                    + "and ErrorKey = '<ErrorKey>' "
                    + "and UserName = '<UserName>' ";
            sql = sql.Replace("<ProjectKey>", pKey)
                    .Replace("<ErrorKey>", eKey)
                    .Replace("<UserName>", uName);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            var ret = new ProjectErrorICareVM();
            foreach (var line in dbret)
            {
                ret.ID = Convert.ToString(line[0]);
                ret.ProjectKey = Convert.ToString(line[1]);
                ret.UserName = Convert.ToString(line[2]);
                ret.ErrorKey = Convert.ToString(line[3]);
                ret.Icare = Convert.ToInt32(line[4]);
                ret.CreateTime = Convert.ToDateTime(line[5]);
                ret.UpdateTime = Convert.ToDateTime(line[6]);
            }

            return ret;
        }

        public static void AddIcare(string pKey, string eKey, string uName)
        {
            var insertsql = "insert into ProjectErrorICare (ProjectKey, UserName, ErrorKey, "
                        + "Icare, CreateTime, UpdateTime) values "
                        + "('<ProjectKey>', '<UserName>', '<ErrorKey>', '<Icare>', '<CreateTime>', '<UpdateTime>')";
            insertsql = insertsql.Replace("<ProjectKey>", pKey)
                    .Replace("<UserName>", uName)
                    .Replace("<ErrorKey>", eKey)
                    .Replace("<Icare>", ProjectErrorICareStatus.Icare.ToString())
                    .Replace("<CreateTime>", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                    .Replace("<UpdateTime>", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            DBUtility.ExeLocalSqlNoRes(insertsql);
        }

        public static void UpdateIcare(string pKey, string eKey, int status, string uName)
        {
            var insertsql = "update ProjectErrorICare set Icare = '<Icare>', "
                    + "UpdateTime = '<UpdateTime>' "
                    + "where ProjectKey = '<ProjectKey>' "
                    + "and ErrorKey = '<ErrorKey>'"
                    + "and UserName = '<UserName>'";
            insertsql = insertsql.Replace("<ProjectKey>", pKey)
                    .Replace("<ErrorKey>", eKey)
                    .Replace("<UserName>", uName)
                    .Replace("<Icare>", status.ToString())
                    .Replace("<UpdateTime>", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            DBUtility.ExeLocalSqlNoRes(insertsql);
        }

        public static void ICare(string pKey, string eKey, string uName)
        {
            var data = GetIssueIcare(pKey, eKey, uName);
            if (string.IsNullOrEmpty(data.ID))
            {
                AddIcare(pKey, eKey, uName);
            }
            else if (data.Icare == 0)
            {
                UpdateIcare(pKey, eKey, ProjectErrorICareStatus.Icare, uName);
            }
        }

        public static void CancelICare(string pKey, string eKey, string uName)
        {
            var data = GetIssueIcare(pKey, eKey, uName);
            if (!string.IsNullOrEmpty(data.ID) && data.Icare == 1)
            {
                UpdateIcare(pKey, eKey, ProjectErrorICareStatus.CancelIcare, uName);
            }
        }

        public static List<ProjectErrorViewModels> GetProjectErrorICareList(string uName, string pKey, string sDate, string eDate, Controller ctrl)
        {
            var ret = new List<ProjectErrorViewModels>();
            var sql = "select pi.ProjectKey, pi.ErrorKey, log.OperateModule as OrignalCode, "
                    + "log.Operate as ShortDesc, count(log.IssueKey) as Count "
                    + "from ProjectErrorICare as pi "
                    + "left join log on pi.ErrorKey = log.IssueKey "
                    + "where pi.ICare = '<Status>' and log.LogType = '<LogType>' "
                    + "and pi.ProjectKey = '<ProjectKey>' "
                    + "and log.Date between '<sDate>' and '<eDate>' "
                    + "and pi.UserName = '<UserName>' "
                    + "group by pi.ProjectKey, pi.errorkey, "
                    + "log.OperateModule, log.Operate, log.issuekey "
                    + "order by count desc; ";
            sql = sql.Replace("<Status>", ProjectErrorICareStatus.Icare.ToString())
                    .Replace("<LogType>", LogType.DebugTree.ToString())
                    .Replace("<ProjectKey>", pKey)
                    .Replace("<UserName>", uName)
                    .Replace("<sDate>", sDate)
                    .Replace("<eDate>", eDate);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var temperror = new ProjectErrorViewModels(
                    Convert.ToString(line[0]),
                    Convert.ToString(line[1]),
                    Convert.ToString(line[2]),
                    Convert.ToString(line[3]),
                    Convert.ToInt32(line[4]), 0);
                temperror.CommentList = ProjectErrorViewModels.RetrieveErrorComments(temperror.ErrorKey, ctrl);
                temperror.RetrieveAttachment(temperror.ErrorKey);
                ret.Add(temperror);
            }
            return ret;
        }
    }
}