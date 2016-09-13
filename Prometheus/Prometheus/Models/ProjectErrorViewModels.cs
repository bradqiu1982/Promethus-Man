using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
    public class ErrorComments
    {
        public string ErrorKey { set; get; }

        private string sComment = "";
        public string Comment
        {
            set { sComment = value; }
            get { return sComment; }
        }

        public string dbComment
        {
            get
            {
                if (string.IsNullOrEmpty(sComment))
                {
                    return "";
                }
                else
                {
                    try
                    {
                        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(sComment));
                    }
                    catch (Exception)
                    {
                        return "";
                    }
                }

            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    sComment = "";
                }
                else
                {
                    try
                    {
                        sComment = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(value));
                    }
                    catch (Exception)
                    {
                        sComment = "";
                    }

                }

            }
        }

        public string Reporter { set; get; }

        public DateTime CommentDate { set; get; }
    }

    public class ProjectErrorViewModels
    {
        public ProjectErrorViewModels()
        { }

        public ProjectErrorViewModels(string pjkey,string ekey,string ocode,string sdesc,int count)
        {
            ProjectKey = pjkey;
            ErrorKey = ekey;
            OrignalCode = ocode;
            ShortDesc = sdesc;
            ErrorCount = count;
        }

        public string ProjectKey { set; get; }

        public string ErrorKey { set; get; }

        public string OrignalCode { set; get; }

        public string ShortDesc { set; get; }

        public string Reporter { set; get; }

        public int ErrorCount { set; get; }

        private string sDescription = "";
        public string Description
        {
            set { sDescription = value; }
            get { return sDescription; }
        }

        public string dbDescription
        {
            get
            {
                if (string.IsNullOrEmpty(sDescription))
                {
                    return "";
                }
                else
                {
                    try
                    {
                        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(sDescription));
                    }
                    catch (Exception)
                    {
                        return "";
                    }
                }

            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    sDescription = "";
                }
                else
                {
                    try
                    {
                        sDescription = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(value));
                    }
                    catch (Exception)
                    {
                        sDescription = "";
                    }

                }

            }
        }

        private List<ErrorComments> cemlist = new List<ErrorComments>();
        public List<ErrorComments> CommentList
        {
            set
            {
                cemlist.Clear();
                cemlist.AddRange(value);
            }
            get
            {
                return cemlist;
            }
        }

        public static string GetUniqKey()
        {
            return Guid.NewGuid().ToString("N");
        }

        public void AddandUpdateProjectError()
        {
            var sql = "select  ProjectKey,ErrorKey,OrignalCode,ShortDesc,ErrorCount from ProjectError where ProjectKey = '<ProjectKey>' and OrignalCode = '<OrignalCode>'";
            sql = sql.Replace("<ProjectKey>", ProjectKey).Replace("<OrignalCode>", OrignalCode);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);

            if (dbret.Count > 0)
            {
                sql = "update ProjectError set ErrorCount = <ErrorCount>  where ProjectKey = '<ProjectKey>' and OrignalCode = '<OrignalCode>'";
                sql = sql.Replace("<ProjectKey>", ProjectKey).Replace("<OrignalCode>", OrignalCode)
                        .Replace("<ErrorCount>", Convert.ToString(Convert.ToUInt32(dbret[0][4])+1));
                DBUtility.ExeLocalSqlNoRes(sql);
            }
            else
            {
                sql = "insert into ProjectError(ProjectKey,ErrorKey,OrignalCode,ShortDesc,ErrorCount) values('<ProjectKey>','<ErrorKey>','<OrignalCode>','<ShortDesc>',1)";
                sql = sql.Replace("<ProjectKey>", ProjectKey).Replace("<ErrorKey>", ErrorKey)
                    .Replace("<OrignalCode>", OrignalCode).Replace("<ShortDesc>", ShortDesc);
                DBUtility.ExeLocalSqlNoRes(sql);
            }
        }

        public void UpdateProjectError()
        {
            var sql = "update ProjectError set ShortDesc = '<ShortDesc>'  where ErrorKey = '<ErrorKey>'";
            sql = sql.Replace("<ErrorKey>", ErrorKey).Replace("<ShortDesc>", ShortDesc);
            DBUtility.ExeLocalSqlNoRes(sql);

            StoreErrorComment(DateTime.Now.ToString());
        }

        private void StoreErrorComment(string CommentDate)
        {
            if (!string.IsNullOrEmpty(Description))
            {
                var sql = "insert into ErrorComments(ErrorKey,Comment,Reporter,CommentDate) values('<ErrorKey>','<Comment>','<Reporter>','<CommentDate>')";
                sql = sql.Replace("<ErrorKey>", ErrorKey).Replace("<Comment>", dbDescription)
                    .Replace("<Reporter>", Reporter).Replace("<CommentDate>", CommentDate);
                DBUtility.ExeLocalSqlNoRes(sql);
            }
        }

        public static List<ProjectErrorViewModels> RetrieveErrorByPJKey(string projectkey)
        {
            var ret = new List<ProjectErrorViewModels>();
            var sql = "select  ProjectKey,ErrorKey,OrignalCode,ShortDesc,ErrorCount from ProjectError where ProjectKey = '<ProjectKey>'";
            sql = sql.Replace("<ProjectKey>", projectkey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);

            foreach (var line in dbret)
            {
                var temperror = new ProjectErrorViewModels(Convert.ToString(line[0]), Convert.ToString(line[1]), Convert.ToString(line[2]), Convert.ToString(line[3]),Convert.ToInt32(line[4]));
                temperror.CommentList = RetrieveErrorComments(temperror.ErrorKey);
                ret.Add(temperror);
            }
            return ret;
        }

        public static List<ProjectErrorViewModels> RetrieveErrorByPJKey(string projectkey,string errorcode)
        {
            var ret = new List<ProjectErrorViewModels>();
            var sql = "select  ProjectKey,ErrorKey,OrignalCode,ShortDesc,ErrorCount from ProjectError where ProjectKey = '<ProjectKey>' and OrignalCode = '<OrignalCode>'";
            sql = sql.Replace("<ProjectKey>", projectkey).Replace("<OrignalCode>", errorcode);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);

            foreach (var line in dbret)
            {
                var temperror = new ProjectErrorViewModels(Convert.ToString(line[0]), Convert.ToString(line[1]), Convert.ToString(line[2]), Convert.ToString(line[3]), Convert.ToInt32(line[4]));
                temperror.CommentList = RetrieveErrorComments(temperror.ErrorKey);
                ret.Add(temperror);
            }
            return ret;
        }

        public static List<ProjectErrorViewModels> RetrieveErrorByErrorKey(string ekey)
        {
            var ret = new List<ProjectErrorViewModels>();
            var sql = "select  ProjectKey,ErrorKey,OrignalCode,ShortDesc,ErrorCount from ProjectError where ErrorKey = '<ErrorKey>'";
            sql = sql.Replace("<ErrorKey>", ekey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);

            foreach (var line in dbret)
            {
                var temperror = new ProjectErrorViewModels(Convert.ToString(line[0]), Convert.ToString(line[1]), Convert.ToString(line[2]), Convert.ToString(line[3]), Convert.ToInt32(line[4]));
                temperror.CommentList = RetrieveErrorComments(temperror.ErrorKey);
                ret.Add(temperror);
            }
            return ret;
        }

        private static List<ErrorComments> RetrieveErrorComments(string errorkey)
        {
            var ret = new List<ErrorComments>();
            var sql = "select ErrorKey,Comment,Reporter,CommentDate from ErrorComments where ErrorKey = '<ErrorKey>'";
            sql = sql.Replace("<ErrorKey>", errorkey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);

            foreach (var r in dbret)
            {
                var tempcomment = new ErrorComments();
                tempcomment.ErrorKey = Convert.ToString(r[0]);
                tempcomment.dbComment = Convert.ToString(r[1]);
                tempcomment.Reporter = Convert.ToString(r[2]);
                tempcomment.CommentDate = DateTime.Parse(Convert.ToString(r[3]));
                ret.Add(tempcomment);
            }
            return ret;
        }
    }
}