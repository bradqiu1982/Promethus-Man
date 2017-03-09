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

        public static void ShareDoc(string DOCPJK, string DOCType, string DOCKey, string DOCTag, string DOCCreator, string DOCDate)
        {
            var sql = "select DOCPJK,DOCKey from ShareDoc where DOCPJK='<DOCPJK>' and DOCKey=N'<DOCKey>'";
            sql = sql.Replace("<DOCPJK>", DOCPJK).Replace("<DOCKey>", DOCKey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            if (dbret.Count > 0)
            {
                return;
            }

            sql = "insert into ShareDoc(DOCPJK,DOCType,DOCKey,DOCTag,DOCCreator,DOCDate) values('<DOCPJK>','<DOCType>',N'<DOCKey>','<DOCTag>','<DOCCreator>','<DOCDate>')";
            sql = sql.Replace("<DOCPJK>", DOCPJK).Replace("<DOCType>", DOCType)
                .Replace("<DOCKey>", DOCKey).Replace("<DOCTag>", DOCTag)
                .Replace("<DOCCreator>", DOCCreator).Replace("<DOCDate>", DOCDate);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static void PushDoc(string BookerName, string DOCPJK, string DOCType, string DOCKey, string DOCTag, string DOCCreator, string DOCDate,string DOCPusher)
        {
            var sql = "select DOCPJK,DOCKey from UserLearn where UserName = '<BookerName>' and DOCPJK='<DOCPJK>' and DOCKey=N'<DOCKey>'";
            sql = sql.Replace("<BookerName>", BookerName).Replace("<DOCPJK>", DOCPJK).Replace("<DOCKey>", DOCKey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            if (dbret.Count > 0)
            {
                return;
            }

            sql = "insert into UserLearn(UserName,DOCPJK,DOCType,DOCKey,DOCTag,DOCCreator,DOCDate,DOCPusher) values('<BookerName>','<DOCPJK>','<DOCType>',N'<DOCKey>','<DOCTag>','<DOCCreator>','<DOCDate>','<DOCPusher>')";
            sql = sql.Replace("<BookerName>", BookerName).Replace("<DOCPJK>", DOCPJK).Replace("<DOCType>", DOCType)
                .Replace("<DOCKey>", DOCKey).Replace("<DOCTag>", DOCTag)
                .Replace("<DOCCreator>", DOCCreator).Replace("<DOCDate>", DOCDate).Replace("<DOCPusher>", DOCPusher);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static void SendPushDocEvent(string what, string urlstr, string towho, string pusher, Controller ctrl)
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
                var content = what + " is share to you by " + pusher + ":\r\n\r\n" + validatestr;

                var toaddrs = new List<string>();
                toaddrs.Add(towho);
                EmailUtility.SendEmail(ctrl, "WUXI NPI System", toaddrs, content);
                new System.Threading.ManualResetEvent(false).WaitOne(20);
            }
            catch (Exception ex)
            { }
        }

        public static void SendPushCommentEvent(string what, string urlstr, string towho, string pusher, Controller ctrl)
        {
            try
            {
                if (string.Compare(towho, pusher, true) == 0)
                {
                    return;
                }

                var routevalue = new RouteValueDictionary();
                routevalue.Add("issuekey", "ABC");
                string scheme = ctrl.Url.RequestContext.HttpContext.Request.Url.Scheme;
                string validatestr = ctrl.Url.Action("UpdateIssue", "Issue", routevalue, scheme);
                var netcomputername = "";
                try { netcomputername = System.Net.Dns.GetHostName(); }
                catch (Exception ex) { }
                validatestr = validatestr.Replace("//localhost", "//" + netcomputername);


                validatestr = validatestr.Split(new string[] { "/Issue" }, StringSplitOptions.RemoveEmptyEntries)[0] + urlstr;
                var content = what + " is added to your shared file by " + pusher + ":\r\n\r\n" + validatestr;

                var toaddrs = new List<string>();
                toaddrs.Add(towho);
                EmailUtility.SendEmail(ctrl, "WUXI NPI System", toaddrs, content);
                new System.Threading.ManualResetEvent(false).WaitOne(20);
            }
            catch (Exception ex)
            { }
        }

        public static void IPushDoc(string DOCPJK, string DOCKey, string ToWho,string Pusher, Controller ctrl)
        {
            var sql = "select DOCPJK,DOCType,DOCKey,DOCTag,DOCCreator,DOCDate,DOCFavorTimes from ShareDoc where DOCPJK = '<DOCPJK>' and DOCKey = N'<DOCKey>'";
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

                if (string.Compare(tempvm.DOCType, ShareDocType.ISSUE, true) == 0)
                {
                    var issue = IssueViewModels.RetrieveIssueByIssueKey(tempvm.DOCKey);
                    if (issue == null)
                    {
                        continue;
                    }

                    tempvm.Summary = issue.Summary;
                    tempvm.DocURL = "/Issue/UpdateIssue?issuekey=" + tempvm.DOCKey;
                }
                else
                {
                    tempvm.Summary = tempvm.DOCKey;
                    var tempstrs = tempvm.Summary.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                    var datestr = tempstrs[tempstrs.Length - 1].Substring(0, 8);
                    tempvm.DocURL = "/userfiles/docs/" + datestr + "/" + tempvm.DOCKey;
                }

                PushDoc(ToWho, tempvm.DOCPJK, tempvm.DOCType, tempvm.DOCKey, tempvm.DOCTag, tempvm.DOCCreator, tempvm.DOCDate.ToString(), Pusher);

                if (string.Compare(tempvm.DOCType, ShareDocType.ISSUE, true) == 0)
                {
                    SendPushDocEvent("a new Issue about "+tempvm.DOCTag, tempvm.DocURL, ToWho, Pusher, ctrl);
                }
                else
                {
                    SendPushDocEvent("a new document about " + tempvm.DOCTag, "/User/WebDoc?DocKey="+tempvm.DOCKey + "&Creator="+tempvm.DOCCreator, ToWho, Pusher, ctrl);
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

        public static List<ShareDocVM> RetrieveMyLearn(string UserName)
        {
            var ret = new List<ShareDocVM>();
            var sql = "select a.DOCPJK,a.DOCType,a.DOCKey,a.DOCTag,a.DOCCreator,a.DOCDate,a.DOCPusher,a.DOCFavor,b.DOCFavorTimes from UserLearn a left join ShareDoc b ON a.DOCKey = b.DOCKey where a.UserName= '<UserName>' and a.DOCType <> '<DOCType>' order by a.DOCDate DESC";
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

                if (string.Compare(tempvm.DOCType, ShareDocType.ISSUE, true) == 0)
                {
                    var issue = IssueViewModels.RetrieveIssueByIssueKey(tempvm.DOCKey);
                    if (issue == null)
                    {
                        continue;
                    }

                    tempvm.Summary = issue.Summary;
                    tempvm.DocURL = "/Issue/UpdateIssue?issuekey="+ tempvm.DOCKey;
                }
                else if (string.Compare(tempvm.DOCType, ShareDocType.DOCUMENT, true) == 0)
                {
                    tempvm.Summary = tempvm.DOCKey;
                    //var tempstrs = tempvm.Summary.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                    //var datestr = tempstrs[tempstrs.Length - 1].Substring(0,8);
                    //tempvm.DocURL = "/userfiles/docs/" + datestr + "/" + tempvm.DOCKey;
                    tempvm.DocURL = "/User/WebDoc?DocKey=" + tempvm.DOCKey+ "&Creator="+tempvm.DOCCreator;
                }

                ret.Add(tempvm);
            }

            var bloglist = RetrieveMyLearnBlog(UserName);
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

        private static List<ShareDocVM> RetrieveMyLearnBlog(string UserName)
        {
            var ret = new List<ShareDocVM>();
            var sql = "select a.DOCPJK,a.DOCType,a.DOCKey,a.DOCTag,a.DOCCreator,a.DOCDate,a.DOCPusher,a.DOCFavor from UserLearn a where a.UserName= '<UserName>' and a.DOCType = '<DOCType>' order by a.DOCDate DESC";
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
                tempvm.DOCFavorTimes = 0;

                var blog = UserBlogVM.RetrieveBlogDoc(tempvm.DOCKey);
                tempvm.Summary = blog.Title;
                tempvm.DocURL = blog.DocURL;

                ret.Add(tempvm);
            }

            return ret;
        }

        public static List<ShareDocVM> RetrieveMyShare(string UserName)
        {
            var ret = new List<ShareDocVM>();
            var sql = "select a.DOCPJK,a.DOCType,a.DOCKey,a.DOCTag,a.DOCCreator,a.DOCDate,a.DOCFavorTimes from ShareDoc a  where a.DOCCreator= '<UserName>' order by a.DOCDate DESC";
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

                if (string.Compare(tempvm.DOCType, ShareDocType.ISSUE, true) == 0)
                {
                    var issue = IssueViewModels.RetrieveIssueByIssueKey(tempvm.DOCKey);
                    if (issue == null)
                    {
                        continue;
                    }

                    tempvm.Summary = issue.Summary;
                    tempvm.DocURL = "/Issue/UpdateIssue?issuekey=" + tempvm.DOCKey;
                }
                else
                {
                    tempvm.Summary = tempvm.DOCKey;
                    //var tempstrs = tempvm.Summary.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                    //var datestr = tempstrs[tempstrs.Length - 1].Substring(0, 8);
                    //tempvm.DocURL = "/userfiles/docs/" + datestr + "/" + tempvm.DOCKey;
                    tempvm.DocURL = "/User/WebDoc?DocKey=" + tempvm.DOCKey + "&Creator="+tempvm.DOCCreator;
                }

                ret.Add(tempvm);
            }
            return ret;
        }



        public static List<ShareDocVM> RetrieveYesterdayDocs()
        {
            var ret = new List<ShareDocVM>();
            string tempdate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            var starttime = tempdate + " 00:00:01";
            var endtime = tempdate + " 23:59:59";

            var sql = "select DOCPJK,DOCType,DOCKey,DOCTag,DOCCreator,DOCDate,DOCFavorTimes from ShareDoc where DOCDate >= '<starttime>' and DOCDate <= '<endtime>'";
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

                if (string.Compare(tempvm.DOCType, ShareDocType.ISSUE, true) == 0)
                {
                    var issue = IssueViewModels.RetrieveIssueByIssueKey(tempvm.DOCKey);
                    if (issue == null)
                    {
                        continue;
                    }

                    tempvm.Summary = issue.Summary;
                    tempvm.DocURL = "/Issue/UpdateIssue?issuekey=" + tempvm.DOCKey;
                }
                else
                {
                    tempvm.Summary = tempvm.DOCKey;
                    var tempstrs = tempvm.Summary.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                    var datestr = tempstrs[tempstrs.Length - 1].Substring(0, 8);
                    tempvm.DocURL = "/userfiles/docs/" + datestr + "/" + tempvm.DOCKey;
                }

                ret.Add(tempvm);
            }
            return ret;
        }

        public static List<ShareDocVM> RetrieveAllSharedDocs()
        {
            var ret = new List<ShareDocVM>();
            var sql = "select DOCPJK,DOCType,DOCKey,DOCTag,DOCCreator,DOCDate,DOCFavorTimes from ShareDoc order by DOCDate";
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
                    var issue = IssueViewModels.RetrieveIssueByIssueKey(tempvm.DOCKey);
                    if (issue == null)
                    {
                        continue;
                    }

                    tempvm.Summary = issue.Summary;
                    tempvm.DocURL = "/Issue/UpdateIssue?issuekey=" + tempvm.DOCKey;
                }
                else
                {
                    tempvm.Summary = tempvm.DOCKey;
                    var tempstrs = tempvm.Summary.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                    var datestr = tempstrs[tempstrs.Length - 1].Substring(0, 8);
                    tempvm.DocURL = "/userfiles/docs/" + datestr + "/" + tempvm.DOCKey;
                }

                ret.Add(tempvm);
            }
            return ret;
        }

        public static void LikeDoc(string DOCPJK, string DOCKey,string updater)
        {
            var ret = new List<ShareDocVM>();
            var sql = "select a.DOCPJK,a.DOCType,a.DOCKey,a.DOCTag,a.DOCCreator,a.DOCDate,b.DOCPusher,b.DOCFavor,a.DOCFavorTimes from ShareDoc a left join UserLearn b ON a.DOCKey = b.DOCKey where a.DOCPJK = '<DOCPJK>' and a.DOCKey = '<DOCKey>' and b.UserName='<UserName>'";
            sql = sql.Replace("<DOCPJK>", DOCPJK).Replace("<DOCKey>", DOCKey).Replace("<UserName>",updater);
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
                ret.Add(tempvm);
            }

            if (ret.Count > 0)
            {
                var sql1 = "Update ShareDoc set DOCFavorTimes = <DOCFavorTimes> where DOCPJK = '<DOCPJK>' and DOCKey = N'<DOCKey>'";
                sql1 = sql1.Replace("<DOCPJK>", DOCPJK).Replace("<DOCKey>", DOCKey).Replace("<DOCFavorTimes>", (ret[0].DOCFavorTimes+1).ToString());
                DBUtility.ExeLocalSqlNoRes(sql1);

                sql1 = "Update UserLearn set DOCFavor='LIKE' where DOCPJK = '<DOCPJK>' and DOCKey = N'<DOCKey>' and UserName='<UserName>'";
                sql1 = sql1.Replace("<DOCPJK>", DOCPJK).Replace("<DOCKey>", DOCKey).Replace("<UserName>", updater);
                DBUtility.ExeLocalSqlNoRes(sql1);

                UserRankViewModel.UpdateUserRank(ret[0].DOCCreator, 3);
                if (!string.IsNullOrEmpty(ret[0].DOCPusher))
                {
                    UserRankViewModel.UpdateUserRank(ret[0].DOCPusher, 3);
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

        public static List<ShareDocVM> Top10InTenDay()
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
                    var issue = IssueViewModels.RetrieveIssueByIssueKey(tempvm.DOCKey);
                    if (issue == null)
                    {
                        continue;
                    }

                    tempvm.Summary = issue.Summary;
                    tempvm.DocURL = "/Issue/UpdateIssue?issuekey=" + tempvm.DOCKey;
                }
                else
                {
                    tempvm.Summary = tempvm.DOCKey;
                    var tempstrs = tempvm.Summary.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                    var datestr = tempstrs[tempstrs.Length - 1].Substring(0, 8);
                    tempvm.DocURL = "/userfiles/docs/" + datestr + "/" + tempvm.DOCKey;
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
                sql = "insert into ShareTags(DOCTag) values('<DOCTag>')";
                sql = sql.Replace("<DOCTag>", tag.Trim());
                DBUtility.ExeLocalSqlNoRes(sql);
            }
        }

        public static List<string> RetrieveShareTags()
        {
            var ret = new List<string>();
            var sql = "select DOCTag from ShareTags order by DOCTag ASC";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            foreach (var line in dbret)
            { ret.Add(Convert.ToString(line[0])); }
            return ret;
        }

        public static void MatchAllPostDocForUser(string username)
        {
            var allpostdoc = RetrieveAllSharedDocs();

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
                    PushDoc(username, doc.DOCPJK, doc.DOCType, doc.DOCKey, doc.DOCTag, doc.DOCCreator, doc.DOCDate.ToString(), "");
                }
            }//end foreach
        }

        public static void MatchAllYesterdayDoc(Controller ctrl)
        {
            var userlist = RetrieveAllUserBookedTag();
            var ydoc = RetrieveYesterdayDocs();

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
                        PushDoc(u.BookerName, doc.DOCPJK, doc.DOCType, doc.DOCKey, doc.DOCTag, doc.DOCCreator, doc.DOCDate.ToString(), "");

                        if (string.Compare(doc.DOCType, ShareDocType.ISSUE, true) == 0)
                        {
                            SendPushDocEvent("a new finished issue about " + doc.DOCTag, doc.DocURL, u.BookerName, "System", ctrl);
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