using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Prometheus.Models
{
    public class ShareDocType
    {
        public static string ISSUE = "ISSUE";
        public static string DOCUMENT = "DOCUMENT";
        public static string BLOG = "BLOG";
        public static string DEBUG = "DEBUG";
    }

    public class ShareDocVM
    {
        public string BookerName { set; get; }
        public string DOCPJK { set; get; }
        public string DOCType { set; get; }
        public string DOCKey { set; get; }
        public string DOCTag {
            set
            {
                docdict.Clear();
                var tags = value.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in tags)
                {
                    if (!docdict.ContainsKey(item))
                    {
                        docdict.Add(item.Trim(), true);
                    }
                }
            }

            get
            {
                var ret = string.Empty;
                foreach (var item in docdict)
                {
                    ret = ret + item.Key.Trim()+";";
                }
                return ret;
            }
        }

        private Dictionary<string, bool> docdict = new Dictionary<string, bool>();
        public Dictionary<string, bool> DOCTagDict
        {
          get {return docdict; }
        }

        public string DOCCreator { set; get; }
        public string DOCPusher { set; get; }
        public string DOCFavor { set; get;}
        public DateTime DOCDate { set; get; }
        public int DOCFavorTimes { set; get; }

        public string Summary { set; get; }

        public string DocURL { set; get; }

        public string DocID { set; get; }

        public string BACKLink { set; get; }


        private static string UpdateDocID(string DOCPJK, string DOCKey)
        {
            var docid = IssueViewModels.GetUniqKey();
            var sql = "Update ShareDoc set APVal1 = '<DocID>' where  DOCPJK = '<DOCPJK>' and DOCKey = N'<DOCKey>'";
            sql = sql.Replace("<DocID>", docid).Replace("<DOCPJK>", DOCPJK).Replace("<DOCKey>", DOCKey);
            DBUtility.ExeLocalSqlNoRes(sql);
            sql = "Update UserLearn set APVal1 = '<DocID>' where  DOCPJK = '<DOCPJK>' and DOCKey = N'<DOCKey>'";
            sql = sql.Replace("<DocID>", docid).Replace("<DOCPJK>", DOCPJK).Replace("<DOCKey>", DOCKey);
            DBUtility.ExeLocalSqlNoRes(sql);
            return docid;
        }

        private static void UpdateTag(string tag, string DOCKey)
        {
            var sql = "Update ShareDoc set DOCTag = '<DOCTag>' where DOCKey = N'<DOCKey>'";
            sql = sql.Replace("<DOCTag>", tag).Replace("<DOCKey>", DOCKey);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static string ShareDoc(string DOCPJK, string DOCType, string DOCKey, string DOCTag, string DOCCreator, string DOCDate,string BackLink)
        {
            var sql = "select APVal1,DOCTag from ShareDoc where DOCPJK='<DOCPJK>' and DOCKey=N'<DOCKey>'";
            sql = sql.Replace("<DOCPJK>", DOCPJK).Replace("<DOCKey>", DOCKey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            if (dbret.Count > 0)
            {
                var docid1 =  Convert.ToString(dbret[0][0]);
                if (string.IsNullOrEmpty(docid1))
                {
                    docid1 = UpdateDocID(DOCPJK,DOCKey);
                }
                var newtag = DOCTag +  Convert.ToString(dbret[0][1]);
                UpdateTag(newtag, DOCKey);
                return docid1;
            }

            var docid = IssueViewModels.GetUniqKey();

            sql = "insert into ShareDoc(DOCPJK,DOCType,DOCKey,DOCTag,DOCCreator,DOCDate,APVal1,BackLink,databackuptm) values('<DOCPJK>','<DOCType>',N'<DOCKey>','<DOCTag>','<DOCCreator>','<DOCDate>','<DOCID>','<BackLink>','<databackuptm>')";
            sql = sql.Replace("<DOCPJK>", DOCPJK).Replace("<DOCType>", DOCType)
                .Replace("<DOCKey>", DOCKey).Replace("<DOCTag>", DOCTag)
                .Replace("<DOCCreator>", DOCCreator).Replace("<DOCDate>", DOCDate)
                .Replace("<DOCID>", docid).Replace("<BackLink>", BackLink).Replace("<databackuptm>", DateTime.Now.ToString());
            DBUtility.ExeLocalSqlNoRes(sql);
            return docid;
        }

        public static string RetrieveSharedDocTag(string dockey)
        {
            var ret = new List<string>();
            var sql = "select DOCTag from ShareDoc where DOCKey = '<DOCKey>'";
            sql = sql.Replace("<DOCKey>", dockey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                return Convert.ToString(line[0]).Replace(CRITICALERRORTYPE.CRITICALERRORTAG+";","");
            }
            return string.Empty;
        }

        public static string RetrieveBackLink(string Dockey)
        {
            var sql = "select BackLink from ShareDoc where DOCKey=N'<DOCKey>'";
            sql = sql.Replace("<DOCKey>", Dockey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            if (dbret.Count > 0)
            {
                var backlink = Convert.ToString(dbret[0][0]);
                return backlink;
            }
            return string.Empty;
        }

        public static void PushDoc(string BookerName, string DOCPJK, string DOCType, string DOCKey, string DOCTag, string DOCCreator, string DOCDate,string DOCPusher,string DOCID,string BackLink)
        {
            var sql = "select DOCPJK,DOCKey from UserLearn where UserName = '<BookerName>' and DOCPJK='<DOCPJK>' and DOCKey=N'<DOCKey>'";
            sql = sql.Replace("<BookerName>", BookerName).Replace("<DOCPJK>", DOCPJK).Replace("<DOCKey>", DOCKey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            if (dbret.Count > 0)
            {
                return;
            }

            sql = "insert into UserLearn(UserName,DOCPJK,DOCType,DOCKey,DOCTag,DOCCreator,DOCDate,DOCPusher,APVal1,BackLink,databackuptm) values('<BookerName>','<DOCPJK>','<DOCType>',N'<DOCKey>','<DOCTag>','<DOCCreator>','<DOCDate>','<DOCPusher>','<DOCID>','<BackLink>','<databackuptm>')";
            sql = sql.Replace("<BookerName>", BookerName).Replace("<DOCPJK>", DOCPJK).Replace("<DOCType>", DOCType)
                .Replace("<DOCKey>", DOCKey).Replace("<DOCTag>", DOCTag)
                .Replace("<DOCCreator>", DOCCreator).Replace("<DOCDate>", DOCDate).Replace("<DOCPusher>", DOCPusher)
                .Replace("<DOCID>", DOCID).Replace("<BackLink>", BackLink).Replace("<databackuptm>", DateTime.Now.ToString());
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static void SendPushDocEvent(string what, string urlstr, string towho, string pusher, Controller ctrl,string reason="")
        {
            try
            {
                var routevalue = new RouteValueDictionary();
                routevalue.Add("issuekey", "ABC");
                string scheme = ctrl.Url.RequestContext.HttpContext.Request.Url.Scheme;
                string validatestr = ctrl.Url.Action("UpdateIssue", "Issue", routevalue, scheme);
                var netcomputername = "";
                try { netcomputername = System.Net.Dns.GetHostName(); }
                catch (Exception ex) { }
                validatestr = validatestr.Replace("//localhost", "//" + netcomputername);


                validatestr = validatestr.Split(new string[] { "/Issue" }, StringSplitOptions.RemoveEmptyEntries)[0] + urlstr;

                var content = "";
                if (!string.IsNullOrEmpty(reason))
                {
                    content = "With Comment: " + reason + "\r\n\r\n";
                }
                content = content+what + " is share to you by " + pusher + ":\r\n\r\n" + validatestr;

                var toaddrs = new List<string>();
                toaddrs.Add(towho);
                EmailUtility.SendEmail(ctrl, "WUXI Engineering System - "+pusher.Split(new string[] { "@"},StringSplitOptions.RemoveEmptyEntries)[0].ToUpper()
                    , toaddrs, content);
                new System.Threading.ManualResetEvent(false).WaitOne(20);
            }
            catch (Exception ex)
            { }
        }

        public static void SendPushCommentEvent(string what, string urlstr, List<string> towho, string pusher, Controller ctrl,string commentcontent)
        {
            try
            {
                var routevalue = new RouteValueDictionary();
                routevalue.Add("issuekey", "ABC");
                string scheme = ctrl.Url.RequestContext.HttpContext.Request.Url.Scheme;
                string validatestr = ctrl.Url.Action("UpdateIssue", "Issue", routevalue, scheme);
                var netcomputername = "";
                try { netcomputername = System.Net.Dns.GetHostName(); }
                catch (Exception ex) { }
                validatestr = validatestr.Replace("//localhost", "//" + netcomputername);


                validatestr = validatestr.Split(new string[] { "/Issue" }, StringSplitOptions.RemoveEmptyEntries)[0] + urlstr;
                var content = what + " is added by " + pusher + ":\r\n\r\n" + commentcontent + "\r\n\r\n" + validatestr;

                var toaddrs = new List<string>();
                toaddrs.AddRange(towho);

                var reporter = pusher.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries)[0].Replace(".", " ");
                EmailUtility.SendEmail(ctrl, "WUXI Engineering System_"+reporter, toaddrs, content);
                new System.Threading.ManualResetEvent(false).WaitOne(20);
            }
            catch (Exception ex)
            { }
        }

        public static void IPushDoc(string DOCPJK, string DOCKey, string ToWho,string Pusher, Controller ctrl,string reason="")
        {
            var sql = "select DOCPJK,DOCType,DOCKey,DOCTag,DOCCreator,DOCDate,DOCFavorTimes,APVal1,BackLink from ShareDoc where DOCPJK = '<DOCPJK>' and DOCKey = N'<DOCKey>'";
            sql = sql.Replace("<DOCPJK>", DOCPJK).Replace("<DOCKey>", DOCKey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            foreach (var line in dbret)
            {
                var tempvm = new ShareDocVM();
                tempvm.DOCPJK = Convert.ToString(line[0]);
                tempvm.DOCType = Convert.ToString(line[1]);
                tempvm.DOCKey = Convert.ToString(line[2]);
                tempvm.DOCTag = Convert.ToString(line[3]);
                tempvm.DOCCreator = Convert.ToString(line[4]);
                tempvm.DOCDate = DateTime.Parse(Convert.ToString(line[5]));
                tempvm.DOCFavorTimes = Convert.ToInt32(line[6]);
                tempvm.DocID = Convert.ToString(line[7]);
                tempvm.BACKLink = Convert.ToString(line[8]);

                if (string.Compare(tempvm.DOCType, ShareDocType.ISSUE, true) == 0)
                {
                    var issue = IssueViewModels.RetrieveIssueByIssueKey(tempvm.DOCKey, ctrl);
                    if (issue == null)
                    {
                        continue;
                    }

                    tempvm.Summary = issue.Summary;
                    tempvm.DocURL = "/Issue/UpdateIssue?issuekey=" + tempvm.DOCKey;
                }
                else if (string.Compare(tempvm.DOCType, ShareDocType.DEBUG, true) == 0)
                {
                    var debugtree = ProjectErrorViewModels.RetrieveErrorByErrorKey(tempvm.DOCKey,ctrl);
                    tempvm.Summary = debugtree[0].ProjectKey+"-"+debugtree[0].OrignalCode;
                    tempvm.DocURL = "/Project/UpdateProjectError?ErrorKey=" + tempvm.DOCKey;
                }
                else
                {
                    tempvm.Summary = tempvm.DOCKey;
                    var tempstrs = tempvm.Summary.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                    var datestr = tempstrs[tempstrs.Length - 1].Substring(0, 8);
                    tempvm.DocURL = "/userfiles/docs/" + datestr + "/" + tempvm.DOCKey;
                }

                PushDoc(ToWho, tempvm.DOCPJK, tempvm.DOCType, tempvm.DOCKey, tempvm.DOCTag, tempvm.DOCCreator, tempvm.DOCDate.ToString(), Pusher,tempvm.DocID,tempvm.BACKLink);

                if (string.Compare(tempvm.DOCType, ShareDocType.ISSUE, true) == 0)
                {
                    SendPushDocEvent("a new Issue about "+tempvm.DOCTag, tempvm.DocURL, ToWho, Pusher, ctrl,reason);
                }
                else if (string.Compare(tempvm.DOCType, ShareDocType.DEBUG, true) == 0)
                {
                    SendPushDocEvent("a debug about " +tempvm.DOCPJK+"-"+ tempvm.Summary, tempvm.DocURL, ToWho, Pusher, ctrl,reason);
                }
                else
                {
                    SendPushDocEvent("a new document about " + tempvm.DOCTag, "/User/WebDoc?DocKey="+ HttpUtility.UrlEncode(tempvm.DOCKey) + "&Creator="+tempvm.DOCCreator, ToWho, Pusher, ctrl);
                }
            }

        }

        public static void SetUserBookTag(string UserName, string DocTag)
        {
            var sql = "Update UserTable set DocTag='<DocTag>' where UserName = '<UserName>'";
            sql = sql.Replace("<UserName>", UserName).Replace("<DocTag>", DocTag);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static ShareDocVM RetrieveUserBookedTag(string UserName)
        {
            var ret = new ShareDocVM();

            var sql = "select DocTag from UserTable where UserName = '<UserName>'";
            sql = sql.Replace("<UserName>", UserName);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            if(dbret.Count > 0)
            {
                ret.BookerName = UserName;
                ret.DOCTag = Convert.ToString(dbret[0][0]);
            }

            return ret;
        }

        public static List<ShareDocVM> RetrieveAllUserBookedTag()
        {
            var ret = new List<ShareDocVM>();

            var sql = "select UserName,DocTag from UserTable";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            foreach (var line in dbret)
            {
                if (!string.IsNullOrEmpty(Convert.ToString(line[1])))
                {
                    var tempitem = new ShareDocVM();
                    tempitem.BookerName = Convert.ToString(line[0]);
                    tempitem.DOCTag = Convert.ToString(line[1]);
                    ret.Add(tempitem);
                }
           }

            return ret;
        }

        public static List<ShareDocVM> RetrieveMyLearn(string UserName, Controller ctrl)
        {
            var ret = new List<ShareDocVM>();
            var sql = "select a.DOCPJK,a.DOCType,a.DOCKey,a.DOCTag,a.DOCCreator,a.DOCDate,a.DOCPusher,a.DOCFavor,b.DOCFavorTimes,b.APVal1,a.BackLink from UserLearn a left join ShareDoc b ON a.DOCKey = b.DOCKey where a.UserName= '<UserName>' and a.DOCType <> '<DOCType>' order by a.DOCDate DESC";
            sql = sql.Replace("<UserName>", UserName).Replace("<DOCType>", ShareDocType.BLOG);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            foreach (var line in dbret)
            {
                var tempvm = new ShareDocVM();
                tempvm.BookerName = UserName;
                tempvm.DOCPJK = Convert.ToString(line[0]);
                tempvm.DOCType = Convert.ToString(line[1]);
                tempvm.DOCKey = Convert.ToString(line[2]);
                tempvm.DOCTag = Convert.ToString(line[3]);
                tempvm.DOCCreator = Convert.ToString(line[4]);
                tempvm.DOCDate = DateTime.Parse(Convert.ToString(line[5]));
                tempvm.DOCPusher = Convert.ToString(line[6]);
                tempvm.DOCFavor = Convert.ToString(line[7]);
                tempvm.DOCFavorTimes = Convert.ToInt32(line[8]);
                tempvm.DocID = Convert.ToString(line[9]);
                tempvm.BACKLink = Convert.ToString(line[10]);

                if (string.Compare(tempvm.DOCType, ShareDocType.ISSUE, true) == 0)
                {
                    var issue = IssueViewModels.RetrieveIssueByIssueKey(tempvm.DOCKey, ctrl);
                    if (issue == null)
                    {
                        continue;
                    }

                    tempvm.Summary = issue.Summary;
                    tempvm.DocURL = "/Issue/UpdateIssue?issuekey=" + tempvm.DOCKey;
                }
                else if (string.Compare(tempvm.DOCType, ShareDocType.DEBUG, true) == 0)
                {
                    var debugtree = ProjectErrorViewModels.RetrieveErrorByErrorKey(tempvm.DOCKey, ctrl);
                    tempvm.Summary = debugtree[0].ProjectKey + "-" + debugtree[0].OrignalCode;
                    tempvm.DocURL = "/Project/UpdateProjectError?ErrorKey=" + tempvm.DOCKey;
                }
                else if (string.Compare(tempvm.DOCType, ShareDocType.DOCUMENT, true) == 0)
                {
                    tempvm.Summary = tempvm.DOCKey;
                    tempvm.DocURL = "/User/WebDoc?DocKey=" + tempvm.DOCKey + "&Creator=" + tempvm.DOCCreator;
                }

                ret.Add(tempvm);
            }

            foreach (var doc in ret)
            {
                if (string.IsNullOrEmpty(doc.DocID))
                {
                    doc.DocID = UpdateDocID(doc.DOCPJK, doc.DOCKey);
                }
            }

            var bloglist = RetrieveMyLearnBlog(UserName,ctrl);
            if (bloglist.Count > 0)
            {
                ret.AddRange(bloglist);
            }

            ret.Sort(delegate (ShareDocVM item1, ShareDocVM item2)
            {
                if (item1.DOCDate > item2.DOCDate)
                {
                    return -1;
                }
                else if (item1.DOCDate < item2.DOCDate)
                {
                    return 1;
                }
                else
                    return 0;
            });

            return ret;
        }

        public static void RemoveMyLearn(string dockey, string username)
        {
            var sql = "delete from UserLearn where DOCKey=N'<DOCKey>' and UserName='<UserName>'";
            sql = sql.Replace("<DOCKey>", dockey).Replace("<UserName>", username);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static int RetrieveBlogFavorTimes(string DOCKey, string DOCCreator)
        {
                var sql = "select DOCFavorTimes from ShareDoc where  DOCKey=N'<DOCKey>' and DOCCreator = '<DOCCreator>'";
                sql = sql.Replace("<DOCKey>", DOCKey).Replace("<DOCCreator>", DOCCreator);
                var dbret2 = DBUtility.ExeLocalSqlWithRes(sql, null);
                if (dbret2.Count > 0)
                {
                    try
                    {
                        return  Convert.ToInt32(dbret2[0][0]);
                    }
                    catch (Exception ex)
                    {
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }
        }

        private static List<ShareDocVM> RetrieveMyLearnBlog(string UserName, Controller ctrl)
        {
            var ret = new List<ShareDocVM>();
            var sql = "select a.DOCPJK,a.DOCType,a.DOCKey,a.DOCTag,a.DOCCreator,a.DOCDate,a.DOCPusher,a.DOCFavor,a.APVal1,a.BackLink from UserLearn a where a.UserName= '<UserName>' and a.DOCType = '<DOCType>' order by a.DOCDate DESC";
            sql = sql.Replace("<UserName>", UserName).Replace("<DOCType>", ShareDocType.BLOG);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            foreach (var line in dbret)
            {
                var tempvm = new ShareDocVM();
                tempvm.BookerName = UserName;
                tempvm.DOCPJK = Convert.ToString(line[0]);
                tempvm.DOCType = Convert.ToString(line[1]);
                tempvm.DOCKey = Convert.ToString(line[2]);
                tempvm.DOCTag = Convert.ToString(line[3]);
                tempvm.DOCCreator = Convert.ToString(line[4]);
                tempvm.DOCDate = DateTime.Parse(Convert.ToString(line[5]));
                tempvm.DOCPusher = Convert.ToString(line[6]);
                tempvm.DOCFavor = Convert.ToString(line[7]);
                tempvm.DOCFavorTimes = RetrieveBlogFavorTimes(tempvm.DOCKey, tempvm.DOCCreator);
                tempvm.DocID = Convert.ToString(line[8]);
                tempvm.BACKLink = Convert.ToString(line[9]);

                var blog = UserBlogVM.RetrieveBlogDoc(tempvm.DOCKey,ctrl);
                if (string.IsNullOrEmpty(blog.DocKey))
                    continue;

                tempvm.Summary = blog.Title;
                tempvm.DocURL = blog.DocURL;

                ret.Add(tempvm);
            }

            foreach (var doc in ret)
            {
                if (string.IsNullOrEmpty(doc.DocID))
                {
                    doc.DocID = UpdateDocID(doc.DOCPJK, doc.DOCKey);
                }
            }

            return ret;
        }

        public static List<ShareDocVM> RetrieveMyShare(string UserName,Controller ctrl)
        {
            var ret = new List<ShareDocVM>();
            var sql = "select a.DOCPJK,a.DOCType,a.DOCKey,a.DOCTag,a.DOCCreator,a.DOCDate,a.DOCFavorTimes,a.APVal1,a.BackLink from ShareDoc a  where a.DOCCreator= '<UserName>' order by a.DOCDate DESC";
            sql = sql.Replace("<UserName>", UserName);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            foreach (var line in dbret)
            {
                var tempvm = new ShareDocVM();
                tempvm.BookerName = UserName;
                tempvm.DOCPJK = Convert.ToString(line[0]);
                tempvm.DOCType = Convert.ToString(line[1]);
                tempvm.DOCKey = Convert.ToString(line[2]);
                tempvm.DOCTag = Convert.ToString(line[3]);
                tempvm.DOCCreator = Convert.ToString(line[4]);
                tempvm.DOCDate = DateTime.Parse(Convert.ToString(line[5]));
                tempvm.DOCFavorTimes = Convert.ToInt32(line[6]);
                tempvm.DocID = Convert.ToString(line[7]);
                tempvm.BACKLink = Convert.ToString(line[8]);

                if (string.Compare(tempvm.DOCType, ShareDocType.ISSUE, true) == 0)
                {
                    var issue = IssueViewModels.RetrieveIssueByIssueKey(tempvm.DOCKey, ctrl);
                    if (issue == null)
                    {
                        continue;
                    }

                    tempvm.Summary = issue.Summary;
                    tempvm.DocURL = "/Issue/UpdateIssue?issuekey=" + tempvm.DOCKey;
                }
                else if (string.Compare(tempvm.DOCType, ShareDocType.DEBUG, true) == 0)
                {
                    var debugtree = ProjectErrorViewModels.RetrieveErrorByErrorKey(tempvm.DOCKey, ctrl);
                    tempvm.Summary = debugtree[0].ProjectKey + "-" + debugtree[0].OrignalCode;
                    tempvm.DocURL = "/Project/UpdateProjectError?ErrorKey=" + tempvm.DOCKey;
                }
                else if (string.Compare(tempvm.DOCType, ShareDocType.BLOG, true) == 0)
                {
                    var blog = UserBlogVM.RetrieveBlogDoc(tempvm.DOCKey, ctrl);
                    if (string.IsNullOrEmpty(blog.DocKey))
                        continue;

                    tempvm.Summary = blog.Title;
                    tempvm.DocURL = blog.DocURL;
                }
                else
                {
                    tempvm.Summary = tempvm.DOCKey;
                    //var tempstrs = tempvm.Summary.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                    //var datestr = tempstrs[tempstrs.Length - 1].Substring(0, 8);
                    //tempvm.DocURL = "/userfiles/docs/" + datestr + "/" + tempvm.DOCKey;
                    tempvm.DocURL = "/User/WebDoc?DocKey=" + tempvm.DOCKey + "&Creator=" + tempvm.DOCCreator;
                }

                ret.Add(tempvm);
            }

            foreach (var doc in ret)
            {
                if (string.IsNullOrEmpty(doc.DocID))
                {
                    doc.DocID = UpdateDocID(doc.DOCPJK, doc.DOCKey);
                }
            }

            return ret;
        }

        public static List<ShareDocVM> RetrieveUserShare(string starttime, Controller ctrl)
        {
            var ret = new List<ShareDocVM>();
            var sql = "select a.DOCPJK,a.DOCType,a.DOCKey,a.DOCTag,a.DOCCreator,a.DOCDate,a.DOCFavorTimes,a.APVal1,a.BackLink from ShareDoc a  where a.DOCDate >= '<DOCDate>' order by a.DOCDate DESC";
            sql = sql.Replace("<DOCDate>", starttime);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var tempvm = new ShareDocVM();
                tempvm.BookerName = "";
                tempvm.DOCPJK = Convert.ToString(line[0]);
                tempvm.DOCType = Convert.ToString(line[1]);
                tempvm.DOCKey = Convert.ToString(line[2]);
                tempvm.DOCTag = Convert.ToString(line[3]);
                if (tempvm.DOCTag.Contains(SPECIALBLOGType.WEEKLYREPORT))
                    continue;
                tempvm.DOCCreator = Convert.ToString(line[4]);
                tempvm.DOCDate = DateTime.Parse(Convert.ToString(line[5]));
                tempvm.DOCFavorTimes = Convert.ToInt32(line[6]);
                tempvm.DocID = Convert.ToString(line[7]);
                tempvm.BACKLink = Convert.ToString(line[8]);

                if (string.Compare(tempvm.DOCType, ShareDocType.ISSUE, true) == 0)
                {
                    var issue = IssueViewModels.RetrieveIssueByIssueKey(tempvm.DOCKey, ctrl);
                    if (issue == null)
                    {
                        continue;
                    }

                    tempvm.Summary = issue.Summary;
                    tempvm.DocURL = "/Issue/UpdateIssue?issuekey=" + tempvm.DOCKey;
                }
                else if (string.Compare(tempvm.DOCType, ShareDocType.DEBUG, true) == 0)
                {
                    var debugtree = ProjectErrorViewModels.RetrieveErrorByErrorKey(tempvm.DOCKey, ctrl);
                    tempvm.Summary = debugtree[0].ProjectKey + "-" + debugtree[0].OrignalCode;
                    tempvm.DocURL = "/Project/UpdateProjectError?ErrorKey=" + tempvm.DOCKey;
                }
                else if (string.Compare(tempvm.DOCType, ShareDocType.BLOG, true) == 0)
                {
                    var blog = UserBlogVM.RetrieveBlogDoc(tempvm.DOCKey, ctrl);
                    if (string.IsNullOrEmpty(blog.DocKey))
                        continue;

                    tempvm.Summary = blog.Title;
                    tempvm.DocURL = blog.DocURL;
                }
                else
                {
                    tempvm.Summary = tempvm.DOCKey;
                    //var tempstrs = tempvm.Summary.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                    //var datestr = tempstrs[tempstrs.Length - 1].Substring(0, 8);
                    //tempvm.DocURL = "/userfiles/docs/" + datestr + "/" + tempvm.DOCKey;
                    tempvm.DocURL = "/User/WebDoc?DocKey=" + tempvm.DOCKey + "&Creator=" + tempvm.DOCCreator;
                }

                ret.Add(tempvm);
            }

            foreach (var doc in ret)
            {
                if (string.IsNullOrEmpty(doc.DocID))
                {
                    doc.DocID = UpdateDocID(doc.DOCPJK, doc.DOCKey);
                }
            }

            return ret;
        }



        public static List<ShareDocVM> RetrieveYesterdayDocs(Controller ctrl)
        {
            var ret = new List<ShareDocVM>();
            string tempdate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            var starttime = tempdate + " 00:00:01";
            var endtime = tempdate + " 23:59:59";

            var sql = "select DOCPJK,DOCType,DOCKey,DOCTag,DOCCreator,DOCDate,DOCFavorTimes,APVal1,BackLink from ShareDoc where DOCDate >= '<starttime>' and DOCDate <= '<endtime>'";
            sql = sql.Replace("<starttime>", starttime).Replace("<endtime>",endtime);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            foreach (var line in dbret)
            {
                var tempvm = new ShareDocVM();
                tempvm.DOCPJK = Convert.ToString(line[0]);
                tempvm.DOCType = Convert.ToString(line[1]);
                tempvm.DOCKey = Convert.ToString(line[2]);
                tempvm.DOCTag = Convert.ToString(line[3]);
                tempvm.DOCCreator = Convert.ToString(line[4]);
                tempvm.DOCDate = DateTime.Parse(Convert.ToString(line[5]));
                tempvm.DOCFavorTimes = Convert.ToInt32(line[6]);
                tempvm.DocID = Convert.ToString(line[7]);
                tempvm.BACKLink = Convert.ToString(line[8]);

                if (string.Compare(tempvm.DOCType, ShareDocType.ISSUE, true) == 0)
                {
                    var issue = IssueViewModels.RetrieveIssueByIssueKey(tempvm.DOCKey,ctrl);
                    if (issue == null)
                    {
                        continue;
                    }

                    tempvm.Summary = issue.Summary;
                    tempvm.DocURL = "/Issue/UpdateIssue?issuekey=" + tempvm.DOCKey;
                }
                else if (string.Compare(tempvm.DOCType, ShareDocType.DEBUG, true) == 0)
                {
                    var debugtree = ProjectErrorViewModels.RetrieveErrorByErrorKey(tempvm.DOCKey,ctrl);
                    tempvm.Summary = debugtree[0].ProjectKey + "-" + debugtree[0].OrignalCode;
                    tempvm.DocURL = "/Project/UpdateProjectError?ErrorKey=" + tempvm.DOCKey;
                }
                else
                {
                    tempvm.Summary = tempvm.DOCKey;
                    //var tempstrs = tempvm.Summary.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                    //var datestr = tempstrs[tempstrs.Length - 1].Substring(0, 8);
                    //tempvm.DocURL = "/userfiles/docs/" + datestr + "/" + tempvm.DOCKey;
                    tempvm.DocURL = "/User/WebDoc?DocKey=" + tempvm.DOCKey + "&Creator=" + tempvm.DOCCreator;
                }

                ret.Add(tempvm);
            }
            return ret;
        }

        public static List<ShareDocVM> RetrieveAllSharedDocs(Controller ctrl)
        {
            var ret = new List<ShareDocVM>();
            var sql = "select DOCPJK,DOCType,DOCKey,DOCTag,DOCCreator,DOCDate,DOCFavorTimes,APVal1,BackLink from ShareDoc order by DOCDate";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            foreach (var line in dbret)
            {
                var tempvm = new ShareDocVM();
                tempvm.DOCPJK = Convert.ToString(line[0]);
                tempvm.DOCType = Convert.ToString(line[1]);
                tempvm.DOCKey = Convert.ToString(line[2]);
                tempvm.DOCTag = Convert.ToString(line[3]);
                tempvm.DOCCreator = Convert.ToString(line[4]);
                tempvm.DOCDate = DateTime.Parse(Convert.ToString(line[5]));
                tempvm.DOCFavorTimes = Convert.ToInt32(line[6]);
                tempvm.DocID = Convert.ToString(line[7]);
                tempvm.BACKLink = Convert.ToString(line[8]);

                if (string.Compare(tempvm.DOCType, ShareDocType.ISSUE, true) == 0)
                {
                    var issue = IssueViewModels.RetrieveIssueByIssueKey(tempvm.DOCKey,ctrl);
                    if (issue == null)
                    {
                        continue;
                    }

                    tempvm.Summary = issue.Summary;
                    tempvm.DocURL = "/Issue/UpdateIssue?issuekey=" + tempvm.DOCKey;
                }
                else if (string.Compare(tempvm.DOCType, ShareDocType.DEBUG, true) == 0)
                {
                    var debugtree = ProjectErrorViewModels.RetrieveErrorByErrorKey(tempvm.DOCKey, ctrl);
                    tempvm.Summary = debugtree[0].ProjectKey + "-" + debugtree[0].OrignalCode;
                    tempvm.DocURL = "/Project/UpdateProjectError?ErrorKey=" + tempvm.DOCKey;
                }
                else if (string.Compare(tempvm.DOCType, ShareDocType.BLOG, true) == 0)
                {
                    var blog = UserBlogVM.RetrieveBlogDoc(tempvm.DOCKey, ctrl);
                    if (string.IsNullOrEmpty(blog.DocKey))
                        continue;

                    tempvm.Summary = blog.Title;
                    tempvm.DocURL = blog.DocURL;
                }
                else
                {
                    tempvm.Summary = tempvm.DOCKey;
                    //var tempstrs = tempvm.Summary.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                    //var datestr = tempstrs[tempstrs.Length - 1].Substring(0, 8);
                    //tempvm.DocURL = "/userfiles/docs/" + datestr + "/" + tempvm.DOCKey;
                    tempvm.DocURL = "/User/WebDoc?DocKey=" + tempvm.DOCKey + "&Creator=" + tempvm.DOCCreator;
                }

                ret.Add(tempvm);
            }
            return ret;
        }

        public static void LikeDoc(string DOCKey,string DOCCreator, string updater,Controller ctrl)
        {
            var sql = "select DOCFavor from UserLearn where DOCKey = N'<DOCKey>' and DOCCreator = '<DOCCreator>' and UserName = '<UserName>'";
            sql = sql.Replace("<DOCKey>", DOCKey).Replace("<DOCCreator>", DOCCreator).Replace("<UserName>", updater);
            var dbret2 = DBUtility.ExeLocalSqlWithRes(sql, null);
            if (dbret2.Count == 0)
                return;
            if (!string.IsNullOrEmpty(Convert.ToString(dbret2[0][0])))
                return;

            var ret = new List<ShareDocVM>();
            sql = "select a.DOCPJK,a.DOCType,a.DOCKey,a.DOCTag,a.DOCCreator,a.DOCDate,b.DOCPusher,b.DOCFavor,a.DOCFavorTimes,a.APVal1,a.BackLink from ShareDoc a left join UserLearn b ON a.DOCKey = b.DOCKey "
                + " where  a.DOCKey = N'<DOCKey>' and a.DOCCreator = '<DOCCreator>'  and b.UserName='<UserName>'";
            sql = sql.Replace("<DOCKey>", DOCKey).Replace("<DOCCreator>", DOCCreator).Replace("<UserName>",updater);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            foreach (var line in dbret)
            {
                var tempvm = new ShareDocVM();
                tempvm.DOCPJK = Convert.ToString(line[0]);
                tempvm.DOCType = Convert.ToString(line[1]);
                tempvm.DOCKey = Convert.ToString(line[2]);
                tempvm.DOCTag = Convert.ToString(line[3]);
                tempvm.DOCCreator = Convert.ToString(line[4]);
                tempvm.DOCDate = DateTime.Parse(Convert.ToString(line[5]));
                tempvm.DOCPusher = Convert.ToString(line[6]);
                tempvm.DOCFavor = Convert.ToString(line[7]);
                tempvm.DOCFavorTimes = Convert.ToInt32(line[8]);
                tempvm.DocID = Convert.ToString(line[9]);
                tempvm.BACKLink = Convert.ToString(line[10]);
                ret.Add(tempvm);
            }

            if (ret.Count > 0)
            {
                var sql1 = "Update ShareDoc set DOCFavorTimes = <DOCFavorTimes> where DOCKey = N'<DOCKey>' and DOCCreator = '<DOCCreator>'";
                sql1 = sql1.Replace("<DOCKey>", DOCKey).Replace("<DOCCreator>", DOCCreator).Replace("<DOCFavorTimes>", (ret[0].DOCFavorTimes + 1).ToString());
                DBUtility.ExeLocalSqlNoRes(sql1);
            }


            var sql2 = "Update UserLearn set DOCFavor='LIKE' where DOCKey = N'<DOCKey>' and DOCCreator = '<DOCCreator>' and UserName='<UserName>'";
            sql2 = sql2.Replace("<DOCKey>", DOCKey).Replace("<DOCCreator>", DOCCreator).Replace("<UserName>", updater);
            DBUtility.ExeLocalSqlNoRes(sql2);

            if (ret.Count > 0)
            {
                if (string.Compare(ret[0].DOCType, ShareDocType.ISSUE, true) == 0)
                {
                    var issue = IssueViewModels.RetrieveIssueByIssueKey(ret[0].DOCKey, ctrl);
                    if (issue != null)
                    {
                        var Summary = issue.Summary;
                        var DocURL = "/Issue/UpdateIssue?issuekey=" + ret[0].DOCKey;
                        UserKPIVM.AddUserDailyRank(ret[0].DOCKey, issue.Assignee, UserRankType.VOTE, "Like your task analyse: " + Summary, DocURL, 1);
                    }
                }
                else if (string.Compare(ret[0].DOCType, ShareDocType.DEBUG, true) == 0)
                {
                    var debugtree = ProjectErrorViewModels.RetrieveErrorByErrorKey(ret[0].DOCKey, ctrl);
                    var Summary = debugtree[0].ProjectKey + "-" + debugtree[0].OrignalCode;
                    var DocURL = "/Project/UpdateProjectError?ErrorKey=" + ret[0].DOCKey;

                    UserKPIVM.AddUserDailyRank(ret[0].DOCKey, ret[0].DOCCreator, UserRankType.VOTE, "Like your Debug Tree analyse: " + Summary, DocURL, 1);
                }
                else if (string.Compare(ret[0].DOCType, ShareDocType.DOCUMENT, true) == 0)
                {
                    var Summary = ret[0].DOCKey;

                    var DocURL = "/User/WebDoc?DocKey=" + ret[0].DOCKey + "&Creator=" + ret[0].DOCCreator;
                    if (!string.IsNullOrEmpty(ret[0].BACKLink))
                    {
                        UserKPIVM.AddUserAttachDailyRank(ret[0].DocID, ret[0].DOCCreator, UserRankType.VOTE, "Like your technical document: " + Summary, ret[0].BACKLink, 1, ret[0].DOCKey,ctrl);
                    }
                    else
                    {
                        UserKPIVM.AddUserAttachDailyRank(ret[0].DocID, ret[0].DOCCreator, UserRankType.VOTE, "Like your technical document: " + Summary, DocURL, 1, ret[0].DOCKey,ctrl);
                    }

                }
                else if (string.Compare(ret[0].DOCType, ShareDocType.BLOG, true) == 0)
                {
                    var vm = UserBlogVM.RetrieveBlogDoc(ret[0].DOCKey, ctrl);
                    var DocURL = "/User/WebDoc?DocKey=" + ret[0].DOCKey;
                    UserKPIVM.AddUserDailyRank(ret[0].DOCKey, ret[0].DOCCreator, UserRankType.VOTE, "Like your blog: " + vm.Title, DocURL, 1);
                }
            }
                
        }

        //public static void RemoveDoc(string DOCPJK, string DOCKey)
        //{
        //    var sql = "delete from ShareDoc where DOCPJK='<DOCPJK>' and DOCKey = N'<DOCKey>'";
        //    sql = sql.Replace("<DOCPJK>", DOCPJK).Replace("<DOCKey>", DOCKey);
        //    DBUtility.ExeLocalSqlNoRes(sql);

        //    sql = "delete from UserLearn where DOCPJK='<DOCPJK>' and DOCKey = N'<DOCKey>'";
        //    sql = sql.Replace("<DOCPJK>", DOCPJK).Replace("<DOCKey>", DOCKey);
        //    DBUtility.ExeLocalSqlNoRes(sql);
        //}

        public static void RemoveDoc(string DOCKey)
        {
            var sql = "delete from ShareDoc where DOCKey = N'<DOCKey>'";
            sql = sql.Replace("<DOCKey>", DOCKey);
            DBUtility.ExeLocalSqlNoRes(sql);

            sql = "delete from UserLearn where DOCKey = N'<DOCKey>'";
            sql = sql.Replace("<DOCKey>", DOCKey);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static List<ShareDocVM> Top10InTenDay(Controller ctrl)
        {
            var ret = new List<ShareDocVM>();
            var starttime = DateTime.Now.AddDays(-10).ToString();
            var sql = "select top 10 DOCPJK,DOCType,DOCKey,DOCTag,DOCCreator,DOCDate,DOCFavorTimes from ShareDoc where DOCDate >= '<starttime>' order by DOCFavorTimes";
            sql = sql.Replace("<starttime>", starttime);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            foreach (var line in dbret)
            {
                var tempvm = new ShareDocVM();
                tempvm.DOCPJK = Convert.ToString(line[0]);
                tempvm.DOCType = Convert.ToString(line[1]);
                tempvm.DOCKey = Convert.ToString(line[2]);
                tempvm.DOCTag = Convert.ToString(line[3]);
                tempvm.DOCCreator = Convert.ToString(line[4]);
                tempvm.DOCDate = DateTime.Parse(Convert.ToString(line[5]));
                tempvm.DOCFavorTimes = Convert.ToInt32(line[6]);

                if (string.Compare(tempvm.DOCType, ShareDocType.ISSUE, true) == 0)
                {
                    var issue = IssueViewModels.RetrieveIssueByIssueKey(tempvm.DOCKey,ctrl);
                    if (issue == null)
                    {
                        continue;
                    }

                    tempvm.Summary = issue.Summary;
                    tempvm.DocURL = "/Issue/UpdateIssue?issuekey=" + tempvm.DOCKey;
                }
                else if (string.Compare(tempvm.DOCType, ShareDocType.DEBUG, true) == 0)
                {
                    var debugtree = ProjectErrorViewModels.RetrieveErrorByErrorKey(tempvm.DOCKey, ctrl);
                    tempvm.Summary = debugtree[0].ProjectKey + "-" + debugtree[0].OrignalCode;
                    tempvm.DocURL = "/Project/UpdateProjectError?ErrorKey=" + tempvm.DOCKey;
                }
                else if (string.Compare(tempvm.DOCType, ShareDocType.BLOG, true) == 0)
                {
                    var blog = UserBlogVM.RetrieveBlogDoc(tempvm.DOCKey, ctrl);
                    if (string.IsNullOrEmpty(blog.DocKey))
                        continue;

                    tempvm.Summary = blog.Title;
                    tempvm.DocURL = blog.DocURL;
                }
                else
                {
                    tempvm.Summary = tempvm.DOCKey;
                    //var tempstrs = tempvm.Summary.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                    //var datestr = tempstrs[tempstrs.Length - 1].Substring(0, 8);
                    //tempvm.DocURL = "/userfiles/docs/" + datestr + "/" + tempvm.DOCKey;
                    tempvm.DocURL = "/User/WebDoc?DocKey=" + tempvm.DOCKey + "&Creator=" + tempvm.DOCCreator;
                }

                ret.Add(tempvm);
            }
            return ret;
        }

        public static void StoreShareTags(List<string> Tags)
        {
            foreach (var tag in Tags)
            {
                var sql = "delete from ShareTags where DOCTag = '<Tag>'";
                sql = sql.Replace("<Tag>", tag.Trim());
                DBUtility.ExeLocalSqlNoRes(sql);
                sql = "insert into ShareTags(DOCTag,databackuptm) values('<DOCTag>','<databackuptm>')";
                sql = sql.Replace("<DOCTag>", tag.Trim()).Replace("<databackuptm>", DateTime.Now.ToString());
                DBUtility.ExeLocalSqlNoRes(sql);
            }
        }

        public static List<string> RetrieveShareTags(Controller ctrl)
        {
            var ret = new List<string>();
            var sharetags = CfgUtility.GetSysConfig(ctrl)["SHARETAG"].Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in sharetags)
            {
                ret.Add(item.Trim());
            }
            ret.Sort();
            return ret;

            //var sql = "select DOCTag from ShareTags order by DOCTag ASC";
            //var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            //foreach (var line in dbret)
            //{ ret.Add(Convert.ToString(line[0])); }
        }

        public static List<string> RetrieveCriticalTags(Controller ctrl)
        {
            var ret = new List<string>();
            var sharetags = CfgUtility.GetSysConfig(ctrl)["CRITICALFAILURERT"].Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in sharetags)
            {
                ret.Add(item.Trim());
            }
            ret.Sort();
            return ret;
        }

        public static List<string> RetrieveCriticalSymptom(Controller ctrl)
        {
            var ret = new List<string>();
            var sharetags = CfgUtility.GetSysConfig(ctrl)["CRITICALFAILURESYMPTOM"].Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in sharetags)
            {
                ret.Add(item.Trim());
            }
            ret.Sort();
            return ret;
        }

        public static void MatchAllPostDocForUser(string username,Controller ctrl)
        {
            var allpostdoc = RetrieveAllSharedDocs(ctrl);

            var usertag = RetrieveUserBookedTag(username);
            var usertaglist = new List<string>();
            usertaglist.AddRange(usertag.DOCTagDict.Keys);

            foreach (var doc in allpostdoc)
            {
                bool push = false;
                foreach (var utag in usertaglist)
                {
                    if (doc.DOCTagDict.ContainsKey(utag)||string.Compare(doc.DOCPJK,utag,true) == 0)
                    {
                        push = true;
                        break;
                    }
                }//end foreach

                if (push)
                {
                    PushDoc(username, doc.DOCPJK, doc.DOCType, doc.DOCKey, doc.DOCTag, doc.DOCCreator, doc.DOCDate.ToString(), "",doc.DocID,doc.BACKLink);
                }
            }//end foreach
        }

        public static void MatchAllYesterdayDoc(Controller ctrl)
        {
            var userlist = RetrieveAllUserBookedTag();
            var ydoc = RetrieveYesterdayDocs(ctrl);

            foreach (var u in userlist)
            {
                var usertaglist = new List<string>();
                usertaglist.AddRange(u.DOCTagDict.Keys);
                foreach (var doc in ydoc)
                {
                    bool push = false;
                    foreach (var utag in usertaglist)
                    {
                        if (doc.DOCTagDict.ContainsKey(utag) || string.Compare(doc.DOCPJK, utag, true) == 0)
                        {
                            push = true;
                            break;
                        }
                    }//end foreach

                    if (push)
                    {
                        PushDoc(u.BookerName, doc.DOCPJK, doc.DOCType, doc.DOCKey, doc.DOCTag, doc.DOCCreator, doc.DOCDate.ToString(), "",doc.DocID,doc.BACKLink);

                        if (string.Compare(doc.DOCType, ShareDocType.ISSUE, true) == 0)
                        {
                            SendPushDocEvent("a new finished issue about " + doc.DOCTag, doc.DocURL, u.BookerName, "System", ctrl);
                        }
                        else if (string.Compare(doc.DOCType, ShareDocType.DEBUG, true) == 0)
                        {
                            SendPushDocEvent("a debug about " +doc.DOCPJK+"-"+ doc.DOCTag, doc.DocURL, u.BookerName, "System", ctrl);
                        }
                        else
                        {
                            SendPushDocEvent("a new document about " + doc.DOCTag, "/User/WebDoc?DocKey=" + doc.DOCKey + "&Creator=" + doc.DOCCreator, u.BookerName, "System", ctrl);
                        }

                           
                    }
                }//end foreach
            }//foreach
        }


    }
}