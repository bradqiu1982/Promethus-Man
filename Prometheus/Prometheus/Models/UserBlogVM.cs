using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prometheus.Models
{
    public class UserBlogContentType
    {
        public static string ATTACH = "ATTACH";
        public static string COMMENT = "COMMENT";
        public static string TAG = "TAG";
    }

    public class SPECIALBLOGType
    {
        public static string WEEKLYREPORT = "WEEKLYREPORT";
        public static string TRAINING = "TRAINING";
        public static string DEFAULT = "DEFAULT";
    }

    public class UserBlogVM
    {
        public UserBlogVM()
        {
            UserName = string.Empty;
            ContentType = string.Empty;
            DocKey = string.Empty;
            Title = string.Empty;
            Content = string.Empty;
            Tag = string.Empty;
            BackLink = string.Empty;
        }

        public string BackLink { set; get; }
        public string UserName { set; get; }
        public string ContentType { set; get; }
        public string DocKey { set; get; }

        private string sTitle = "";
        public string Title
        {
            set { sTitle = value; }
            get { return sTitle; }
        }

        private string dbTitle
        {
            get
            {
                if (string.IsNullOrEmpty(sTitle))
                {
                    return "";
                }
                else
                {
                    try
                    {
                        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(sTitle));
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
                    sTitle = "";
                }
                else
                {
                    try
                    {
                        sTitle = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(value));
                    }
                    catch (Exception)
                    {
                        sTitle = "";
                    }

                }

            }
        }

        private string sContent = "";
        public string Content
        {
            set { sContent = value; }
            get { return sContent; }
        }

        private string dbContent
        {
            get
            {
                if (string.IsNullOrEmpty(sContent))
                {
                    return "";
                }
                else
                {
                    try
                    {
                        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(sContent));
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
                    sContent = "";
                }
                else
                {
                    try
                    {
                        sContent = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(value));
                    }
                    catch (Exception)
                    {
                        sContent = "";
                    }

                }

            }
        }
        
        public string Tag { set; get; }
        public DateTime CreateDate { set; get; }

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

        public string DocURL { set; get; }

        public void StoreBlogDoc()
        {
            DocKey = GetUniqKey();

            var sql = "insert into UserBlog(UserName,APVal1,APVal2,APVal3,APVal4,APVal5,APVal9) "
                + " values('<UserName>','<APVal1>','<APVal2>','<APVal3>','<APVal4>','<APVal5>','<APVal9>')";
            sql = sql.Replace("<UserName>", UserName).Replace("<APVal1>", ContentType).Replace("<APVal2>", DocKey).Replace("<APVal3>", dbTitle)
                    .Replace("<APVal4>", dbContent).Replace("<APVal5>", Tag).Replace("<APVal9>", DateTime.Now.ToString());
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static void RemoveBlogDoc(string DOCKey)
        {
            var sql = "delete from UserBlog where APVal2='<APVal2>'";
            sql = sql.Replace("<APVal2>", DOCKey);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public void UpdateBlogDoc()
        {
            var sql = "update UserBlog set APVal4 = '<APVal4>' where APVal2 = '<APVal2>'";
            sql = sql.Replace("<APVal2>", DocKey).Replace("<APVal4>", dbContent);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static List<UserBlogVM> RetrieveAllBlogDoc(string username)
        {
            var ret = new List<UserBlogVM>();
            var sql = "select UserName,APVal1,APVal2,APVal3,APVal4,APVal5,APVal9 from UserBlog where UserName='<UserName>' and (APVal1 = '<TYPE1>' or APVal1 = '<TYPE2>') order by APVal9 DESC";
            sql = sql.Replace("<UserName>", username).Replace("<TYPE1>", UserBlogContentType.ATTACH).Replace("<TYPE2>", UserBlogContentType.COMMENT);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);

            foreach (var line in dbret)
            {
                var tempinfo = new UserBlogVM();
                tempinfo.UserName = Convert.ToString(line[0]);
                tempinfo.ContentType = Convert.ToString(line[1]);
                tempinfo.DocKey = Convert.ToString(line[2]);
                tempinfo.dbTitle = Convert.ToString(line[3]);
                tempinfo.dbContent = Convert.ToString(line[4]);
                tempinfo.Tag = Convert.ToString(line[5]);
                tempinfo.CreateDate = DateTime.Parse(Convert.ToString(line[6]));

                //if (string.Compare(tempinfo.ContentType, UserBlogContentType.COMMENT) == 0)
                //{
                    tempinfo.DocURL = "/User/WebDoc?DocKey=" + tempinfo.DocKey;
                //}

                //if (string.Compare(tempinfo.ContentType, UserBlogContentType.ATTACH) == 0)
                //{
                //    var tempstrs = tempinfo.Title.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                //    var datestr = tempstrs[tempstrs.Length - 1].Substring(0, 8);
                //    tempinfo.DocURL = "/userfiles/docs/" + datestr + "/" + tempinfo.Title;
                //}

                ret.Add(tempinfo);
            }

            return ret;
        }

        public static UserBlogVM RetrieveBlogDoc(string dockey, Controller ctrl)
        {
            var ret = new UserBlogVM();
            ret.CommentList = ProjectErrorViewModels.RetrieveErrorComments(dockey,ctrl);

            var sql = "select UserName,APVal1,APVal2,APVal3,APVal4,APVal5,APVal9 from UserBlog where APVal2='<APVal2>'";
            sql = sql.Replace("<APVal2>", dockey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);

            foreach (var line in dbret)
            {
                ret.UserName = Convert.ToString(line[0]);
                ret.ContentType = Convert.ToString(line[1]);
                ret.DocKey = Convert.ToString(line[2]);
                ret.dbTitle = Convert.ToString(line[3]);
                ret.dbContent = Convert.ToString(line[4]);
                ret.Tag = Convert.ToString(line[5]);
                ret.CreateDate = DateTime.Parse(Convert.ToString(line[6]));

                //if (string.Compare(ret.ContentType, UserBlogContentType.COMMENT) == 0)
                //{
                    ret.DocURL = "/User/WebDoc?DocKey=" + ret.DocKey;
                //}

                //if (string.Compare(ret.ContentType, UserBlogContentType.ATTACH) == 0)
                //{
                    //ret.DocURL = "/User/WebDoc?DocKey=" + ret.DocKey;
                //}
            }

            return ret;
        }

        public static void StoreUserTag(string username, string tag)
        {
            var existtag = RetrieveUserTag(username);
            var tags = tag.Split(new string[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries);
            var filtertag = string.Empty;
            foreach (var tg in tags)
            {
                if (!existtag.ToUpper().Contains(tg.Trim().ToUpper()))
                {
                    filtertag = filtertag + tg.Trim().ToUpper()+";";
                }
            }

            if (!string.IsNullOrEmpty(filtertag))
            {
                filtertag = existtag + ";" + filtertag;
            }
            else
            {
                filtertag = existtag;
            }
            

            var sql = "delete from UserBlog where UserName='<UserName>' and APVal1 = '<APVal1>'";
            sql = sql.Replace("<UserName>", username).Replace("<APVal1>", UserBlogContentType.TAG);
            DBUtility.ExeLocalSqlNoRes(sql);

            sql = "insert into UserBlog(UserName,APVal1,APVal2) "
                + " values('<UserName>','<APVal1>','<APVal2>')";
            sql = sql.Replace("<UserName>", username).Replace("<APVal1>", UserBlogContentType.TAG).Replace("<APVal2>", filtertag);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static string RetrieveUserTag(string username)
        {
            var ret = string.Empty;
            var sql = "select UserName,APVal1,APVal2 from UserBlog where UserName='<UserName>' and APVal1 = '<APVal1>'";
            sql = sql.Replace("<UserName>", username).Replace("<APVal1>", UserBlogContentType.TAG);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            if (dbret.Count > 0)
            {
                ret = Convert.ToString(dbret[0][2]);
            }
            return ret;
        }

    }

}