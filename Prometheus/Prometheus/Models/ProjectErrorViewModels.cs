using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prometheus.Models
{
    public class PJERRORCOMMENTTYPE
    {
        public static string Description = "Description";
        public static string RootCause = "RootCause";
        public static string FailureDetail= "FailureDetail";
        public static string Result = "Result";
        public static string AnalyzeTitle = "AnalyzeTitle";
    }

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

        public string CommentType { set; get; }
    }

    public class ProjectErrorViewModels
    {
        public ProjectErrorViewModels()
        { }

        public static string BURNIN = "BURNIN";

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


        private List<ErrorComments> generalcommentlist = new List<ErrorComments>();
        private List<ErrorComments> rootcausecommentlist = new List<ErrorComments>();
        private List<ErrorComments> failuredetailcommentlist = new List<ErrorComments>();
        private List<ErrorComments> resultcommentlist = new List<ErrorComments>();
        private List<ErrorComments> titlecommentlist = new List<ErrorComments>();

        public List<ErrorComments> GeneralCommentList
        {
            get { return generalcommentlist; }
        }

        public List<ErrorComments> RootCauseCommentList
        {
            get { return rootcausecommentlist; }
        }

        public List<ErrorComments> ResultCommentList
        {
            get { return resultcommentlist; }
        }

        public List<ErrorComments> FailureDetailCommentList
        {
            get { return failuredetailcommentlist; }
        }

        public List<ErrorComments> AnalyzeTitleCommentList
        {
            get { return titlecommentlist; }
        }


        private List<List<ErrorComments>> pcomments = new List<List<ErrorComments>>();
        public List<List<ErrorComments>> PairComments
        {
            get { return pcomments; }
        }


        private List<ErrorComments> cemlist = new List<ErrorComments>();
        public List<ErrorComments> CommentList
        {
            set
            {
                cemlist.Clear();
                cemlist.AddRange(value);
                generalcommentlist.Clear();
                rootcausecommentlist.Clear();
                failuredetailcommentlist.Clear();
                resultcommentlist.Clear();
                titlecommentlist.Clear();

                foreach (var item in cemlist)
                {
                    if (string.Compare(item.CommentType, PJERRORCOMMENTTYPE.Description) == 0
                        ||string.IsNullOrEmpty(item.CommentType))
                    {
                        generalcommentlist.Add(item);
                    }
                    if (string.Compare(item.CommentType, PJERRORCOMMENTTYPE.RootCause) == 0)
                    {
                        rootcausecommentlist.Add(item);
                    }
                    if (string.Compare(item.CommentType, PJERRORCOMMENTTYPE.FailureDetail) == 0)
                    {
                        failuredetailcommentlist.Add(item);
                    }
                    if (string.Compare(item.CommentType, PJERRORCOMMENTTYPE.Result) == 0)
                    {
                        resultcommentlist.Add(item);
                    }
                    if (string.Compare(item.CommentType, PJERRORCOMMENTTYPE.AnalyzeTitle) == 0)
                    {
                        titlecommentlist.Add(item);
                    }
                }

                foreach (var item in failuredetailcommentlist)
                {
                    var temppitem = new List<ErrorComments>();
                    temppitem.Add(item);

                    var starttime = item.CommentDate.AddSeconds(-3);
                    var endtime = item.CommentDate.AddSeconds(3);

                    foreach (var r in rootcausecommentlist)
                    {
                        if (r.CommentDate > starttime && r.CommentDate < endtime)
                        {
                            temppitem.Add(r);
                        }
                    }//end foreach
                    foreach (var r in resultcommentlist)
                    {
                        if (r.CommentDate > starttime && r.CommentDate < endtime)
                        {
                            temppitem.Add(r);
                        }
                    }//end foreach
                    foreach (var r in titlecommentlist)
                    {
                        if (r.CommentDate > starttime && r.CommentDate < endtime)
                        {
                            temppitem.Add(r);
                        }
                    }//end foreach
                    pcomments.Add(temppitem);
                }//end foreach
            }

            get
            {
                return cemlist;
            }
        }

        private List<string> attachlist = new List<string>();
        public List<string> AttachList
        {
            set
            {
                attachlist.Clear();
                attachlist.AddRange(value);
            }
            get
            {
                return attachlist;
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
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);

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

        public void UpdateShortDesc()
        {
            var sql = "update ProjectError set ShortDesc = '<ShortDesc>'  where ErrorKey = '<ErrorKey>'";
            sql = sql.Replace("<ErrorKey>", ErrorKey).Replace("<ShortDesc>", ShortDesc);
            DBUtility.ExeLocalSqlNoRes(sql);

            //StoreErrorComment(DateTime.Now.ToString());
        }

        //private void StoreErrorComment(string CommentDate)
        //{
        //    if (!string.IsNullOrEmpty(Description))
        //    {
        //        var sql = "insert into ErrorComments(ErrorKey,Comment,Reporter,CommentDate) values('<ErrorKey>','<Comment>','<Reporter>','<CommentDate>')";
        //        sql = sql.Replace("<ErrorKey>", ErrorKey).Replace("<Comment>", dbDescription)
        //            .Replace("<Reporter>", Reporter).Replace("<CommentDate>", CommentDate);
        //        DBUtility.ExeLocalSqlNoRes(sql);
        //    }
        //}

        public static void StoreErrorComment(string ErrorKey, string dbComment,string CommentType, string Reporter, string CommentDate)
        {
            var sql = "insert into ErrorComments(ErrorKey,Comment,Reporter,CommentDate,CommentType) values('<ErrorKey>','<Comment>','<Reporter>','<CommentDate>','<CommentType>')";
            sql = sql.Replace("<ErrorKey>", ErrorKey).Replace("<Comment>", dbComment)
                .Replace("<Reporter>", Reporter).Replace("<CommentDate>", CommentDate).Replace("<CommentType>", CommentType);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static List<ProjectErrorViewModels> RetrieveErrorByPJKey(string projectkey, Controller ctrl)
        {
            var ret = new List<ProjectErrorViewModels>();
            var sql = "select  ProjectKey,ErrorKey,OrignalCode,ShortDesc,ErrorCount from ProjectError where ProjectKey = '<ProjectKey>' order by ErrorCount DESC";
            sql = sql.Replace("<ProjectKey>", projectkey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);

            foreach (var line in dbret)
            {
                var temperror = new ProjectErrorViewModels(Convert.ToString(line[0]), Convert.ToString(line[1]), Convert.ToString(line[2]), Convert.ToString(line[3]),Convert.ToInt32(line[4]));
                temperror.CommentList = RetrieveErrorComments(temperror.ErrorKey,ctrl);
                temperror.RetrieveAttachment(temperror.ErrorKey);
                ret.Add(temperror);
            }
            return ret;
        }

        public static string RetrieveRealError(string projectkey, string errorcode, Controller ctrl)
        {
            var vm = RetrieveErrorByPJKey(projectkey, errorcode,ctrl);
            if (vm.Count == 0)
            {
                return errorcode;
            }

            if (string.IsNullOrEmpty(vm[0].ShortDesc))
            {
                return errorcode;
            }

            return vm[0].ShortDesc;
        }

        public static List<ProjectErrorViewModels> RetrieveErrorByPJKey(string projectkey,string errorcode, Controller ctrl)
        {
            var ret = new List<ProjectErrorViewModels>();
            var sql = "select  ProjectKey,ErrorKey,OrignalCode,ShortDesc,ErrorCount from ProjectError where ProjectKey = '<ProjectKey>' and OrignalCode = '<OrignalCode>'";
            sql = sql.Replace("<ProjectKey>", projectkey).Replace("<OrignalCode>", errorcode);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);

            foreach (var line in dbret)
            {
                var temperror = new ProjectErrorViewModels(Convert.ToString(line[0]), Convert.ToString(line[1]), Convert.ToString(line[2]), Convert.ToString(line[3]), Convert.ToInt32(line[4]));
                temperror.CommentList = RetrieveErrorComments(temperror.ErrorKey,ctrl);
                temperror.RetrieveAttachment(temperror.ErrorKey);
                ret.Add(temperror);
            }
            return ret;
        }

        public static List<ProjectErrorViewModels> RetrieveErrorByErrorKey(string ekey, Controller ctrl)
        {
            var ret = new List<ProjectErrorViewModels>();
            var sql = "select  ProjectKey,ErrorKey,OrignalCode,ShortDesc,ErrorCount from ProjectError where ErrorKey = '<ErrorKey>'";
            sql = sql.Replace("<ErrorKey>", ekey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);

            foreach (var line in dbret)
            {
                var temperror = new ProjectErrorViewModels(Convert.ToString(line[0]), Convert.ToString(line[1]), Convert.ToString(line[2]), Convert.ToString(line[3]), Convert.ToInt32(line[4]));
                temperror.CommentList = RetrieveErrorComments(temperror.ErrorKey,ctrl);
                temperror.RetrieveAttachment(temperror.ErrorKey);
                ret.Add(temperror);
            }
            return ret;
        }

        public static List<ErrorComments> RetrieveErrorComments(string errorkey,Controller ctrl)
        {
            var ret = new List<ErrorComments>();
            var sql = "select ErrorKey,Comment,Reporter,CommentDate,CommentType from ErrorComments where ErrorKey = '<ErrorKey>' and APVal1 <> 'delete' order by CommentDate ASC";
            sql = sql.Replace("<ErrorKey>", errorkey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);

            foreach (var r in dbret)
            {
                var tempcomment = new ErrorComments();
                tempcomment.ErrorKey = Convert.ToString(r[0]);
                tempcomment.dbComment = Convert.ToString(r[1]);
                tempcomment.Reporter = Convert.ToString(r[2]);
                tempcomment.CommentDate = DateTime.Parse(Convert.ToString(r[3]));
                tempcomment.CommentType = Convert.ToString(r[4]);
                ret.Add(tempcomment);
            }

            return RepairBase64Image4IE(ret,ctrl);
        }



        private static string WriteBase64ImgFile(string commentcontent, Controller ctrl)
        {
            try
            {
                var idx = commentcontent.IndexOf("<img alt=\"\" src=\"data:image/png;base64");
                var base64idx = commentcontent.IndexOf("data:image/png;base64,", idx)+22;
                var base64end = commentcontent.IndexOf("\"", base64idx);
                var imgstrend = commentcontent.IndexOf("/>", base64end)+2;
                var base64img = commentcontent.Substring(base64idx, base64end - base64idx);
                var imgbytes = Convert.FromBase64String(base64img);

                var imgkey = Guid.NewGuid().ToString("N");
                string datestring = DateTime.Now.ToString("yyyyMMdd");
                string imgdir = ctrl.Server.MapPath("~/userfiles") + "\\images\\" + datestring + "\\";

                if (!Directory.Exists(imgdir))
                {
                    Directory.CreateDirectory(imgdir);
                }
                var realpath = imgdir + imgkey + ".jpg";
            
                var fs = File.Create(realpath);
                fs.Write(imgbytes, 0, imgbytes.Length);
                fs.Close();


                var url = "/userfiles/images/" + datestring + "/" + imgkey+".jpg";
                var ret = commentcontent;
                ret = ret.Remove(idx, imgstrend - idx);
                ret = ret.Insert(idx, "<img src='" + url + "'/>");

                return ret;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }

        }

        private static string ReplaceBase64data2File(string commentcontent, Controller ctrl)
        {
            var ret = commentcontent;
            if (commentcontent.Contains("<img alt=\"\" src=\"data:image/png;base64"))
            {
                while (ret.Contains("<img alt=\"\" src=\"data:image/png;base64"))
                {
                    ret = WriteBase64ImgFile(ret,ctrl);
                    if (string.IsNullOrEmpty(ret))
                    {
                        ret = commentcontent;
                        break;
                    }
                }
            }

            return ret;
        }

        public static List<ErrorComments> RepairBase64Image4IE(List<ErrorComments> coments, Controller ctrl)
        {
            foreach (var com in coments)
            {
                var newcomment = ReplaceBase64data2File(com.Comment, ctrl);
                if (newcomment.Length != com.Comment.Length)
                {
                    com.Comment = newcomment;
                    UpdateSPComment(com.ErrorKey, com.CommentType, com.CommentDate.ToString(), com.dbComment);
                }
            }
            return coments;
        }

        public static void DeleteErrorComment(string ErrorKey, string CommentType, string Date)
        {
            var sql = "update ErrorComments set APVal1 = 'delete' where ErrorKey='<ErrorKey>' and CommentType='<CommentType>' and CommentDate='<CommentDate>'";
            sql = sql.Replace("<ErrorKey>", ErrorKey).Replace("<CommentType>", CommentType).Replace("<CommentDate>", Date);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static ErrorComments RetrieveSPComment(string ErrorKey, string CommentType, string Date)
        {
            var tempcomment = new ErrorComments();
            var csql = "select ErrorKey,Comment,Reporter,CommentDate,CommentType from ErrorComments  where ErrorKey='<ErrorKey>' and CommentType='<CommentType>' and CommentDate='<CommentDate>'";
            csql = csql.Replace("<ErrorKey>", ErrorKey).Replace("<CommentType>", CommentType).Replace("<CommentDate>", Date);
            var cdbret = DBUtility.ExeLocalSqlWithRes(csql, null);
            foreach (var r in cdbret)
            {
                tempcomment.ErrorKey = Convert.ToString(r[0]);
                tempcomment.dbComment = Convert.ToString(r[1]);
                tempcomment.Reporter = Convert.ToString(r[2]);
                tempcomment.CommentDate = DateTime.Parse(Convert.ToString(r[3]));
                tempcomment.CommentType = Convert.ToString(r[4]);
            }

            return tempcomment;
        }

        public static void UpdateSPComment(string ErrorKey, string CommentType, string Date, string dbcomment)
        {
            var csql = "update ErrorComments set Comment = '<Comment>'  where ErrorKey='<ErrorKey>' and CommentType='<CommentType>' and CommentDate='<CommentDate>'";
            csql = csql.Replace("<ErrorKey>", ErrorKey).Replace("<CommentType>", CommentType).Replace("<CommentDate>", Date).Replace("<Comment>", dbcomment);
            DBUtility.ExeLocalSqlNoRes(csql);
        }

        public static void StoreErrorAttachment(string errorkey, string attachmenturl)
        {
            var sql = "insert into PJErrorAttachment(ErrorKey,Attachment,UpdateTime) values('<ErrorKey>',N'<Attachment>','<UpdateTime>')";
            sql = sql.Replace("<ErrorKey>", errorkey).Replace("<Attachment>", attachmenturl).Replace("<UpdateTime>", DateTime.Now.ToString());
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public void RetrieveAttachment(string errorkey)
        {
            var ret = new List<string>();
            var csql = "select Attachment from PJErrorAttachment where ErrorKey = '<ErrorKey>' order by UpdateTime ASC";
            csql = csql.Replace("<ErrorKey>", errorkey);

            var cdbret = DBUtility.ExeLocalSqlWithRes(csql,null);
            foreach (var r in cdbret)
            {
                ret.Add(Convert.ToString(r[0]));
            }
            AttachList = ret;
        }

        public static void DeleteAttachment(string errorkey, string cond)
        {
            var csql = "select Attachment from PJErrorAttachment where ErrorKey = '<ErrorKey>' and Attachment like N'%<cond>%'";
            csql = csql.Replace("<ErrorKey>", errorkey).Replace("<cond>", cond);
            var cdbret = DBUtility.ExeLocalSqlWithRes(csql,null);
            if (cdbret.Count > 0 && cdbret.Count < 3)
            {
                csql = "delete from PJErrorAttachment where ErrorKey = '<ErrorKey>' and Attachment like N'%<cond>%'";
                csql = csql.Replace("<ErrorKey>", errorkey).Replace("<cond>", cond);
                DBUtility.ExeLocalSqlNoRes(csql);
            }//end if
        }

    }
}