using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prometheus.Models
{
    public class MINDCOLOR
    {
        public static string RootColor = "#00539F";
        public static string ProColor = "#00CC00";
        public static string AnalysisColor = "#FF6200";
    }
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
        public ErrorComments()
        {
            Reporter = "";
            CommentType = "";
            ResultClosed = "";
            AnalyzeID = "";
        }

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

        public string ResultClosed { set; get; }

        public string AnalyzeID { set; get; }
    }

    public class ProjectErrorViewModels
    {
        public ProjectErrorViewModels()
        {
            ProjectKey = "";
            ErrorKey = "";
            OrignalCode = "";
            ShortDesc = "";
            ErrorCount = 0;
            SolvedCount = 0;
            AutoClosed = 0;
        }

        public static string BURNIN = "BURNIN";

        public ProjectErrorViewModels(string pjkey,string ekey,string ocode,string sdesc,int count,int solvedcount = 0)
        {
            ProjectKey = pjkey;
            ErrorKey = ekey;
            OrignalCode = ocode;
            ShortDesc = sdesc;
            ErrorCount = count;
            SolvedCount = solvedcount;
            AutoClosed = 0;
        }

        public string ProjectKey { set; get; }

        public string ErrorKey { set; get; }

        public string OrignalCode { set; get; }

        public string ShortDesc { set; get; }

        public string Reporter { set; get; }

        public int ErrorCount { set; get; }

        public int SolvedCount { set; get; }

        public int AutoClosed { set; get; }

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
                        //if (r.CommentDate > starttime && r.CommentDate < endtime)
                        if(string.Compare(item.AnalyzeID,r.AnalyzeID) == 0)
                        {
                            temppitem.Add(r);
                        }
                    }//end foreach
                    foreach (var r in resultcommentlist)
                    {
                        //if (r.CommentDate > starttime && r.CommentDate < endtime)
                        if (string.Compare(item.AnalyzeID, r.AnalyzeID) == 0)
                        {
                            var sameascount = SameAsDBTVM.SameAsIssueCount(r.ErrorKey, item.CommentDate.ToString());

                            if (sameascount == 0)
                            {
                                var originalissuekey = string.Empty;
                                var originalmodulesn = string.Empty;
                                if (item.Comment.Contains("<a href='/Issue/UpdateIssue?issuekey=")
                                    && item.Comment.Contains("' target='_blank'>")
                                    && item.Comment.Contains(" Issue Link</a>"))
                                {
                                    var splitstr = item.Comment.Split(new string[] { "<a href='/Issue/UpdateIssue?issuekey=" , "' target='_blank'>" , " Issue Link</a>" }, StringSplitOptions.RemoveEmptyEntries);
                                    if (splitstr.Length > 2)
                                    {
                                        originalissuekey = splitstr[1].Trim();
                                        originalmodulesn = splitstr[2].Trim();
                                        if (!string.IsNullOrEmpty(originalissuekey)
                                            && !string.IsNullOrEmpty(originalmodulesn))
                                        {
                                            SameAsDBTVM.StoreLinkDBTIssue(item.ErrorKey, item.CommentDate.ToString(), originalissuekey, originalissuekey, originalmodulesn);
                                        }//end if
                                    }//end if
                                }//end if
                            }//end if

                            if (sameascount > 1)
                            {
                                r.Comment = r.Comment + "<p><a href='/Project/SameAsModules?ErrorKey=" + r.ErrorKey+"&LinkTime="+item.CommentDate.ToString() + "' target='_blank'>" + sameascount.ToString()+" Modules Same As This Module" + "</a></p>";
                            }
                            temppitem.Add(r);
                        }
                    }//end foreach
                    foreach (var r in titlecommentlist)
                    {
                        //if (r.CommentDate > starttime && r.CommentDate < endtime)
                        if (string.Compare(item.AnalyzeID, r.AnalyzeID) == 0)
                        {
                            var sameascount = SameAsDBTVM.SameAsIssueCount(r.ErrorKey, item.CommentDate.ToString());
                            if (sameascount > 1)
                            {
                                r.Comment = r.Comment + "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href='/Project/SameAsModules?ErrorKey=" + r.ErrorKey + "&LinkTime=" + item.CommentDate.ToString() + "' target='_blank' style='Color:white !important;text-decoration: none !important;'><strong>  QTY:" + sameascount.ToString() + "</strong></a>";
                            }
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
            var sql = "select  ProjectKey,ErrorKey,OrignalCode,ShortDesc,ErrorCount,SolvedCount from ProjectError where ProjectKey = '<ProjectKey>' and OrignalCode = N'<OrignalCode>'";
            sql = sql.Replace("<ProjectKey>", ProjectKey).Replace("<OrignalCode>", OrignalCode);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);

            if (dbret.Count > 0)
            {
                sql = "update ProjectError set ErrorCount = <ErrorCount>  where ProjectKey = '<ProjectKey>' and OrignalCode = N'<OrignalCode>'";
                sql = sql.Replace("<ProjectKey>", ProjectKey).Replace("<OrignalCode>", OrignalCode)
                        .Replace("<ErrorCount>", Convert.ToString(Convert.ToUInt32(dbret[0][4])+1));
                DBUtility.ExeLocalSqlNoRes(sql);
            }
            else
            {
                sql = "insert into ProjectError(ProjectKey,ErrorKey,OrignalCode,ShortDesc,ErrorCount,databackuptm) values('<ProjectKey>','<ErrorKey>',N'<OrignalCode>',N'<ShortDesc>',1,'<databackuptm>')";
                sql = sql.Replace("<ProjectKey>", ProjectKey).Replace("<ErrorKey>", ErrorKey)
                    .Replace("<OrignalCode>", OrignalCode).Replace("<ShortDesc>", ShortDesc).Replace("<databackuptm>", DateTime.Now.ToString());
                DBUtility.ExeLocalSqlNoRes(sql);
            }

        }

        public static void UpdateProjectAutoCloseCount(int count,string pjkey,string errorcode)
        {
            var sql = "select AutoClosed from ProjectError where ProjectKey = '<ProjectKey>' and OrignalCode = N'<OrignalCode>'";
            sql = sql.Replace("<ProjectKey>", pjkey).Replace("<OrignalCode>", errorcode);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);

            if (dbret.Count > 0)
            {
                sql = "update ProjectError set AutoClosed = <AutoClosed>  where ProjectKey = '<ProjectKey>' and OrignalCode = N'<OrignalCode>'";
                sql = sql.Replace("<ProjectKey>", pjkey).Replace("<OrignalCode>", errorcode)
                        .Replace("<AutoClosed>", Convert.ToString(Convert.ToUInt32(dbret[0][0]) + count));
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

        public static void StoreErrorComment(string ErrorKey, string dbComment,string CommentType, string Reporter, string CommentDate,string aid = "")
        {
            var sql = "insert into ErrorComments(ErrorKey,Comment,Reporter,CommentDate,CommentType,databackuptm,AnalyzeID) values('<ErrorKey>','<Comment>','<Reporter>','<CommentDate>','<CommentType>','<databackuptm>','<AnalyzeID>')";
            sql = sql.Replace("<ErrorKey>", ErrorKey).Replace("<Comment>", dbComment)
                .Replace("<Reporter>", Reporter).Replace("<CommentDate>", CommentDate).Replace("<CommentType>", CommentType).Replace("<databackuptm>", DateTime.Now.ToString()).Replace("<AnalyzeID>", aid);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static List<ProjectErrorViewModels> RetrieveErrorByPJKey(string projectkey, Controller ctrl)
        {
            var ret = new List<ProjectErrorViewModels>();
            var sql = "select  ProjectKey,ErrorKey,OrignalCode,ShortDesc,ErrorCount,SolvedCount,AutoClosed from ProjectError where ProjectKey = '<ProjectKey>' order by ErrorCount DESC";
            sql = sql.Replace("<ProjectKey>", projectkey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);

            foreach (var line in dbret)
            {
                var temperror = new ProjectErrorViewModels(Convert.ToString(line[0]), Convert.ToString(line[1]), Convert.ToString(line[2]), Convert.ToString(line[3]),Convert.ToInt32(line[4]),Convert.ToInt32(line[5]));
                temperror.AutoClosed = Convert.ToInt32(line[6]);

                temperror.CommentList = RetrieveErrorComments(temperror.ErrorKey,ctrl);
                temperror.RetrieveAttachment(temperror.ErrorKey);
                ret.Add(temperror);
            }
            return ret;
        }

        public static Dictionary<string,string> RetrieveShortDescDict(string projectkey)
        {
            var ret = new Dictionary<string, string>();
            var dupdict = new Dictionary<string, bool>();

            var sql = "select  OrignalCode,ShortDesc from ProjectError where ProjectKey = '<ProjectKey>' order by ErrorCount DESC";
            sql = sql.Replace("<ProjectKey>", projectkey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);

            foreach (var line in dbret)
            {
                if (!string.IsNullOrEmpty(Convert.ToString(line[1])) 
                    && !ret.ContainsKey(Convert.ToString(line[0]))
                    && !dupdict.ContainsKey(Convert.ToString(line[1]).ToUpper()))
                {
                    dupdict.Add(Convert.ToString(line[1]).ToUpper(), true);
                    ret.Add(Convert.ToString(line[0]), Convert.ToString(line[1]));
                }
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
            var sql = "select  ProjectKey,ErrorKey,OrignalCode,ShortDesc,ErrorCount,SolvedCount,AutoClosed from ProjectError where ProjectKey = '<ProjectKey>' and OrignalCode = '<OrignalCode>'";
            sql = sql.Replace("<ProjectKey>", projectkey).Replace("<OrignalCode>", errorcode);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);

            foreach (var line in dbret)
            {
                var temperror = new ProjectErrorViewModels(Convert.ToString(line[0]), Convert.ToString(line[1]), Convert.ToString(line[2]), Convert.ToString(line[3]), Convert.ToInt32(line[4]),Convert.ToInt32(line[5]));
                temperror.AutoClosed = Convert.ToInt32(line[6]);

                temperror.CommentList = RetrieveErrorComments(temperror.ErrorKey,ctrl);
                temperror.RetrieveAttachment(temperror.ErrorKey);
                ret.Add(temperror);
            }
            return ret;
        }

        public static List<ProjectErrorViewModels> RetrieveErrorByErrorKey(string ekey, Controller ctrl)
        {
            var ret = new List<ProjectErrorViewModels>();
            var sql = "select  ProjectKey,ErrorKey,OrignalCode,ShortDesc,ErrorCount,SolvedCount,AutoClosed from ProjectError where ErrorKey = '<ErrorKey>'";
            sql = sql.Replace("<ErrorKey>", ekey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);

            foreach (var line in dbret)
            {
                var temperror = new ProjectErrorViewModels(Convert.ToString(line[0]), Convert.ToString(line[1]), Convert.ToString(line[2]), Convert.ToString(line[3]), Convert.ToInt32(line[4]),Convert.ToInt32(line[5]));
                temperror.AutoClosed = Convert.ToInt32(line[6]);

                temperror.CommentList = RetrieveErrorComments(temperror.ErrorKey,ctrl);
                temperror.RetrieveAttachment(temperror.ErrorKey);
                ret.Add(temperror);
            }
            return ret;
        }

        public static List<ErrorComments> RetrieveErrorComments(string errorkey,Controller ctrl)
        {
            var ret = new List<ErrorComments>();
            var sql = "select ErrorKey,Comment,Reporter,CommentDate,CommentType,APVal2,AnalyzeID from ErrorComments where ErrorKey = '<ErrorKey>' and APVal1 <> 'delete' order by CommentDate ASC";
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
                tempcomment.ResultClosed = Convert.ToString(r[5]);
                tempcomment.AnalyzeID = Convert.ToString(r[6]);
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

        public static void DeleteAnalyse(string ErrorKey, string Date)
        {
            var sql = "update ErrorComments set APVal1 = 'delete' where ErrorKey='<ErrorKey>' and CommentDate > '<CommentDate1>' and CommentDate < '<CommentDate2>'";
            sql = sql.Replace("<ErrorKey>", ErrorKey).Replace("<CommentDate1>",DateTime.Parse(Date).AddSeconds(-3).ToString("yyyy-MM-dd HH:mm:ss"))
                .Replace("<CommentDate2>", DateTime.Parse(Date).AddSeconds(3).ToString("yyyy-MM-dd HH:mm:ss"));

            DBUtility.ExeLocalSqlNoRes(sql);
        }


        public static ErrorComments RetrieveSPComment(string ErrorKey, string CommentType, string Date)
        {
            var tempcomment = new ErrorComments();
            var csql = "select ErrorKey,Comment,Reporter,CommentDate,CommentType,APVal2,AnalyzeID from ErrorComments  where ErrorKey='<ErrorKey>' and CommentType='<CommentType>' and CommentDate='<CommentDate>'";
            csql = csql.Replace("<ErrorKey>", ErrorKey).Replace("<CommentType>", CommentType).Replace("<CommentDate>", Date);
            var cdbret = DBUtility.ExeLocalSqlWithRes(csql, null);
            foreach (var r in cdbret)
            {
                tempcomment.ErrorKey = Convert.ToString(r[0]);
                tempcomment.dbComment = Convert.ToString(r[1]);
                tempcomment.Reporter = Convert.ToString(r[2]);
                tempcomment.CommentDate = DateTime.Parse(Convert.ToString(r[3]));
                tempcomment.CommentType = Convert.ToString(r[4]);
                tempcomment.ResultClosed = Convert.ToString(r[5]);
                tempcomment.AnalyzeID = Convert.ToString(r[6]);
            }

            return tempcomment;
        }

        public static void UpdateSPComment(string ErrorKey, string CommentType, string Date, string dbcomment)
        {
            var csql = "update ErrorComments set Comment = '<Comment>'  where ErrorKey='<ErrorKey>' and CommentType='<CommentType>' and CommentDate='<CommentDate>'";
            csql = csql.Replace("<ErrorKey>", ErrorKey).Replace("<CommentType>", CommentType).Replace("<CommentDate>", Date).Replace("<Comment>", dbcomment);
            DBUtility.ExeLocalSqlNoRes(csql);
        }

        public static void UpdateErrorCommentAID(string ErrorKey, string CommentType, string Date, string aid)
        {
            var csql = "update ErrorComments set AnalyzeID = '<AnalyzeID>'  where ErrorKey='<ErrorKey>' and CommentType='<CommentType>' and CommentDate='<CommentDate>'";
            csql = csql.Replace("<ErrorKey>", ErrorKey).Replace("<CommentType>", CommentType).Replace("<CommentDate>", Date).Replace("<AnalyzeID>", aid);
            DBUtility.ExeLocalSqlNoRes(csql);
        }

        public static void CloseSPResultComment(string ErrorKey, string CommentType, string Date)
        {
            var csql = "update ErrorComments set APVal2 = 'CLOSED'  where ErrorKey='<ErrorKey>' and CommentType='<CommentType>' and CommentDate='<CommentDate>'";
            csql = csql.Replace("<ErrorKey>", ErrorKey).Replace("<CommentType>", CommentType).Replace("<CommentDate>", Date);
            DBUtility.ExeLocalSqlNoRes(csql);
        }

        public static void StoreErrorAttachment(string errorkey, string attachmenturl)
        {
            var sql = "insert into PJErrorAttachment(ErrorKey,Attachment,UpdateTime,databackuptm) values('<ErrorKey>',N'<Attachment>','<UpdateTime>','<databackuptm>')";
            sql = sql.Replace("<ErrorKey>", errorkey).Replace("<Attachment>", attachmenturl).Replace("<UpdateTime>", DateTime.Now.ToString()).Replace("<databackuptm>", DateTime.Now.ToString());
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

        public static void UpdateSolvedIssueCount(string pjkey, string errabbr, int count)
        {
            var sql = "update ProjectError set SolvedCount = <SolvedCount> where ProjectKey = '<ProjectKey>' and OrignalCode = N'<OrignalCode>'";
            sql = sql.Replace("<ProjectKey>", pjkey).Replace("<OrignalCode>", errabbr).Replace("<SolvedCount>", count.ToString());
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static void UpdateSovledIssueCount(string pjkey, string errabbr)
        {
            var sql = "select SolvedCount from ProjectError where  ProjectKey = '<ProjectKey>' and OrignalCode = N'<OrignalCode>'";
            sql = sql.Replace("<ProjectKey>", pjkey).Replace("<OrignalCode>", errabbr);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            if (dbret.Count > 0)
            {
                var newcount = Convert.ToInt32(dbret[0][0]) + 1;
                UpdateSolvedIssueCount(pjkey, errabbr, newcount);
            }
        }
        
        public static List<ProjectErrorViewModels> RetrieveWeeklyErrorByPJKey(string projectkey, string sDate, string eDate, Controller ctrl)
        {
            var ret = new List<ProjectErrorViewModels>();
            var sql = "select ProjectKey, IssueKey as ErrorKey, "
                        + "OperateModule as OrignalCode, Operate as ShortDesc, "
                        + "count(*) as Count "
                        + "from Log "
                        + "where LogType = '<LogType>' and ProjectKey = '<ProjectKey>' "
                        + "and Date between '<sDate>' and '<eDate>' "
                        + "group by IssueKey, ProjectKey, OperateModule, Operate "
                        + "order by Count DESC; ";
            sql = sql.Replace("<LogType>", LogType.DebugTree.ToString())
                    .Replace("<ProjectKey>", projectkey)
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
                temperror.CommentList = RetrieveErrorComments(temperror.ErrorKey, ctrl);
                temperror.RetrieveAttachment(temperror.ErrorKey);
                ret.Add(temperror);
            }
            return ret;
        }

        public static List<JsMindVM> GetProjectErrorByOrignalCode(string original_code, Controller ctrl)
        {

            var sql = @"select pe.ProjectKey, ec.ErrorKey, ec.Comment, ec.Reporter, 
                ec.CommentDate, ec.CommentType, ec.APVal2, ec.AnalyzeID from ErrorComments as ec 
                inner join ProjectError as pe on ec.ErrorKey = pe.ErrorKey 
                where pe.OrignalCode = @OrignalCode and ec.APVal1 <> 'delete' and ec.AnalyzeID <> '';";
            var param = new Dictionary<string, string>();
            param.Add("@OrignalCode", original_code);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, param);
            var comments = new Dictionary<string, List<ErrorComments>>();
            var proErrList = new Dictionary<string, Dictionary<string, List<ErrorComments>>>();
            foreach (var r in dbret)
            {
                var tempcomment = new ErrorComments();
                tempcomment.ErrorKey = Convert.ToString(r[1]);
                tempcomment.dbComment = Convert.ToString(r[2]);
                tempcomment.Reporter = Convert.ToString(r[3]);
                tempcomment.CommentDate = DateTime.Parse(Convert.ToString(r[4]));
                tempcomment.CommentType = Convert.ToString(r[5]);
                tempcomment.ResultClosed = Convert.ToString(r[6]);

                var tmpCommentList = new List<ErrorComments>();
                tmpCommentList.Add(tempcomment);
                var tmpcom = RepairBase64Image4IE(tmpCommentList, ctrl);

                var projectkey = Convert.ToString(r[0]);
                var analysiskey = Convert.ToString(r[7]);

                if (proErrList.ContainsKey(projectkey))
                {
                    if (proErrList[projectkey].ContainsKey(analysiskey))
                    {
                        proErrList[projectkey][analysiskey].Add(tmpcom[0]);
                    }
                    else
                    {
                        proErrList[projectkey].Add(analysiskey, tmpcom);
                    }
                    
                }
                else
                {
                    var tmpcomList = new Dictionary<string, List<ErrorComments>>();
                    tmpcomList.Add(analysiskey, tmpcom);
                    proErrList.Add(projectkey, tmpcomList);
                }
            }

            var res = new List<JsMindVM>();
            var root = new JsMindVM("1", true, original_code, string.Empty, MINDCOLOR.RootColor);
            res.Add(root);
            foreach(var pro in proErrList)
            {
                res.Add(new JsMindVM(pro.Key, false, pro.Key, "1", MINDCOLOR.ProColor));
                var idx = 1;
                foreach(var analysis in pro.Value)
                {
                    if (analysis.Value.Count < 3)
                    {
                        continue;
                    }
                    var topic = string.Empty;
                    foreach(var item in analysis.Value)
                    {
                        if(item.CommentType == PJERRORCOMMENTTYPE.AnalyzeTitle)
                        {
                            topic = item.Comment;
                            break;
                        }
                    }
                    if (string.IsNullOrEmpty(topic))
                    {
                        topic = "Analysis " + idx;
                        idx++;
                    }
                    res.Add(new JsMindVM(analysis.Key, false, topic, pro.Key, MINDCOLOR.AnalysisColor));
                }
            }

            return res;
        }

        public static List<ErrorComments> RetrieveErrorCommentsByAnalyzeID(string AnalyzeID, Controller ctrl)
        {
            var ret = new List<ErrorComments>();
            var sql = "select ErrorKey,Comment,Reporter,CommentDate,CommentType,APVal2,AnalyzeID from ErrorComments where AnalyzeID = '<AnalyzeID>' and APVal1 <> 'delete' order by CommentDate ASC";
            sql = sql.Replace("<AnalyzeID>", AnalyzeID);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);

            foreach (var r in dbret)
            {
                var tempcomment = new ErrorComments();
                tempcomment.ErrorKey = Convert.ToString(r[0]);
                tempcomment.dbComment = Convert.ToString(r[1]);
                tempcomment.Reporter = Convert.ToString(r[2]);
                tempcomment.CommentDate = DateTime.Parse(Convert.ToString(r[3]));
                tempcomment.CommentType = Convert.ToString(r[4]);
                tempcomment.ResultClosed = Convert.ToString(r[5]);
                tempcomment.AnalyzeID = Convert.ToString(r[6]);
                ret.Add(tempcomment);
            }

            return RepairBase64Image4IE(ret, ctrl);
        }

        public static void UpdateAnalyzeTitle(string AnalyzeID, string title)
        {
            var errorcom = new ErrorComments();
            errorcom.Comment = title;

            var sql = "select ErrorKey,Comment,Reporter,CommentDate,CommentType,APVal2 from ErrorComments where AnalyzeID = '<AnalyzeID>' and CommentType = '<CommentType>' and APVal1 <> 'delete' order by CommentDate ASC";
            sql = sql.Replace("<AnalyzeID>", AnalyzeID).Replace("<CommentType>",PJERRORCOMMENTTYPE.AnalyzeTitle);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            if (dbret.Count > 0)
            {
                sql = "update ErrorComments set Comment = '<Comment>' where AnalyzeID = '<AnalyzeID>' and CommentType = '<CommentType>' and APVal1 <> 'delete' ";
                sql = sql.Replace("<AnalyzeID>", AnalyzeID).Replace("<CommentType>", PJERRORCOMMENTTYPE.AnalyzeTitle).Replace("<Comment>", errorcom.dbComment);
                DBUtility.ExeLocalSqlNoRes(sql);
            }
            else
            {
                sql = "select ErrorKey,Reporter,CommentDate from ErrorComments where AnalyzeID = '<AnalyzeID>' and CommentType = '<CommentType>' and APVal1 <> 'delete' order by CommentDate ASC";
                sql = sql.Replace("<AnalyzeID>", AnalyzeID).Replace("<CommentType>", PJERRORCOMMENTTYPE.RootCause);
                dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
                if (dbret.Count > 0)
                {
                    var ErrorKey = Convert.ToString(dbret[0][0]);
                    var Reporter = Convert.ToString(dbret[0][1]);
                    var CommentDate = Convert.ToDateTime(dbret[0][2]).ToString("yyyy-MM-dd HH:mm:ss");
                    StoreErrorComment(ErrorKey, errorcom.dbComment, PJERRORCOMMENTTYPE.AnalyzeTitle, Reporter, CommentDate, AnalyzeID);
                }//HAS ROOTCAUSE
            }
        }


    }
}