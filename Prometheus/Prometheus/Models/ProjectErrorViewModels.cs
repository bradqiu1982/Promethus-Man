using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
    public class PJERRORCOMMENTTYPE
    {
        public static string Description = "Description";
        public static string RootCause = "RootCause";
        public static string FailureDetail= "FailureDetail";
        public static string Result = "Result";
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

        public static List<ProjectErrorViewModels> RetrieveErrorByPJKey(string projectkey)
        {
            var ret = new List<ProjectErrorViewModels>();
            var sql = "select  ProjectKey,ErrorKey,OrignalCode,ShortDesc,ErrorCount from ProjectError where ProjectKey = '<ProjectKey>' order by ErrorCount DESC";
            sql = sql.Replace("<ProjectKey>", projectkey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);

            foreach (var line in dbret)
            {
                var temperror = new ProjectErrorViewModels(Convert.ToString(line[0]), Convert.ToString(line[1]), Convert.ToString(line[2]), Convert.ToString(line[3]),Convert.ToInt32(line[4]));
                temperror.CommentList = RetrieveErrorComments(temperror.ErrorKey);
                temperror.RetrieveAttachment(temperror.ErrorKey);
                ret.Add(temperror);
            }
            return ret;
        }

        public static string RetrieveRealError(string projectkey, string errorcode)
        {
            var vm = RetrieveErrorByPJKey(projectkey, errorcode);
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
                temperror.RetrieveAttachment(temperror.ErrorKey);
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
                temperror.RetrieveAttachment(temperror.ErrorKey);
                ret.Add(temperror);
            }
            return ret;
        }

        public static List<ErrorComments> RetrieveErrorComments(string errorkey)
        {
            var ret = new List<ErrorComments>();
            var sql = "select ErrorKey,Comment,Reporter,CommentDate,CommentType from ErrorComments where ErrorKey = '<ErrorKey>' and APVal1 <> 'delete' order by CommentDate ASC";
            sql = sql.Replace("<ErrorKey>", errorkey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);

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
            return ret;
        }

        public static void DeleteErrorComment(string ErrorKey, string CommentType, string Date)
        {
            var sql = "update ErrorComments set APVal1 = 'delete' where ErrorKey='<ErrorKey>' and CommentType='<CommentType>' and CommentDate='<CommentDate>'";
            sql = sql.Replace("<ErrorKey>", ErrorKey).Replace("<CommentType>", CommentType).Replace("<CommentDate>", Date);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static void StoreErrorAttachment(string errorkey, string attachmenturl)
        {
            var sql = "insert into PJErrorAttachment(ErrorKey,Attachment,UpdateTime) values('<ErrorKey>','<Attachment>','<UpdateTime>')";
            sql = sql.Replace("<ErrorKey>", errorkey).Replace("<Attachment>", attachmenturl).Replace("<UpdateTime>", DateTime.Now.ToString());
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public void RetrieveAttachment(string errorkey)
        {
            var ret = new List<string>();
            var csql = "select Attachment from PJErrorAttachment where ErrorKey = '<ErrorKey>' order by UpdateTime ASC";
            csql = csql.Replace("<ErrorKey>", errorkey);

            var cdbret = DBUtility.ExeLocalSqlWithRes(csql);
            foreach (var r in cdbret)
            {
                ret.Add(Convert.ToString(r[0]));
            }
            AttachList = ret;
        }

        public static void DeleteAttachment(string errorkey, string cond)
        {
            var csql = "select Attachment from PJErrorAttachment where ErrorKey = '<ErrorKey>' and Attachment like '%<cond>%'";
            csql = csql.Replace("<ErrorKey>", errorkey).Replace("<cond>", cond);
            var cdbret = DBUtility.ExeLocalSqlWithRes(csql);
            if (cdbret.Count > 0 && cdbret.Count < 3)
            {
                csql = "delete from PJErrorAttachment where ErrorKey = '<ErrorKey>' and Attachment like '%<cond>%'";
                csql = csql.Replace("<ErrorKey>", errorkey).Replace("<cond>", cond);
                DBUtility.ExeLocalSqlNoRes(csql);
            }//end if
        }

    }
}