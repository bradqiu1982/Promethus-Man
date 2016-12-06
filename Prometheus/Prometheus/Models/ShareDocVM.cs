using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
    public class ShareDocType
    {
        public static string ISSUE = "ISSUE";
        public static string DOCUMENT = "DOCUMENT";
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

        public Dictionary<string, bool> docdict = new Dictionary<string, bool>();
        public Dictionary<string, bool> DOCTagDict
        {
          get {return docdict; }
        }

        public string DOCCreator { set; get; }
        public string DOCPusher { set; get; }
        public string DOCFavor { set; get;}
        public DateTime DOCDate { set; get; }
        public int DOCFavorTimes { set; get; }

        public static void ShareDoc(string DOCPJK, string DOCType, string DOCKey, string DOCTag, string DOCCreator, string DOCDate)
        {
            var sql = "delete from ShareDoc where DOCPJK='<DOCPJK>' and DOCKey='<DOCKey>'";
            sql = sql.Replace("<DOCPJK>", DOCPJK).Replace("<DOCKey>", DOCKey);
            DBUtility.ExeLocalSqlNoRes(sql);

            sql = "insert into ShareDoc(DOCPJK,DOCType,DOCKey,DOCTag,DOCCreator,DOCDate) values('<DOCPJK>','<DOCType>','<DOCKey>','<DOCTag>','<DOCCreator>','<DOCDate>')";
            sql = sql.Replace("<DOCPJK>", DOCPJK).Replace("<DOCType>", DOCType)
                .Replace("<DOCKey>", DOCKey).Replace("<DOCTag>", DOCTag)
                .Replace("<DOCCreator>", DOCCreator).Replace("<DOCDate>", DOCDate);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static void PushDoc(string BookerName, string DOCPJK, string DOCType, string DOCKey, string DOCTag, string DOCCreator, string DOCDate,string DOCPusher)
        {
            var sql = "delete from UserLearn where UserName = '<BookerName>' and DOCPJK='<DOCPJK>' and DOCKey='<DOCKey>'";
            sql = sql.Replace("<BookerName>", BookerName).Replace("<DOCPJK>", DOCPJK).Replace("<DOCKey>", DOCKey);
            DBUtility.ExeLocalSqlNoRes(sql);

            sql = "insert into UserLearn(UserName,DOCPJK,DOCType,DOCKey,DOCTag,DOCCreator,DOCDate,DOCPusher) values('<BookerName>','<DOCPJK>','<DOCType>','<DOCKey>','<DOCTag>','<DOCCreator>','<DOCDate>','<DOCPusher>')";
            sql = sql.Replace("<BookerName>", BookerName).Replace("<DOCPJK>", DOCPJK).Replace("<DOCType>", DOCType)
                .Replace("<DOCKey>", DOCKey).Replace("<DOCTag>", DOCTag)
                .Replace("<DOCCreator>", DOCCreator).Replace("<DOCDate>", DOCDate).Replace("<DOCPusher>", DOCPusher);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static void SetUserBookTag(string UserName, string DocTag)
        {
            var sql = "Update UserTable set DocTag='<DocTag>' where UserName = '<UserName>'";
            sql = sql.Replace("<UserName>", UserName).Replace("<DocTag>", DocTag);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static ShareDocVM RetrieveUserBookTag(string UserName)
        {
            var ret = new ShareDocVM();

            var sql = "select DocTag from UserTable where UserName = '<UserName>'";
            sql = sql.Replace("<UserName>", UserName);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            if(dbret.Count > 0)
            {
                ret.BookerName = UserName;
                ret.DOCTag = Convert.ToString(dbret[0][0]);
            }

            return ret;
        }

        public List<ShareDocVM> RetrieveMyLearn(string UserName)
        {
            var ret = new List<ShareDocVM>();
            var sql = "select a.DOCPJK,a.DOCType,a.DOCKey,a.DOCTag,a.DOCCreator,a.DOCDate,a.DOCPusher,a.DOCFavor,b.DOCFavorTimes from UserLearn a left join ShareDoc b ON a.DOCKey = b.DOCKey where a.UserName= '<UserName>' order by a.DOCDate";
            sql = sql.Replace("<UserName>", UserName);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
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
                ret.Add(tempvm);
            }
            return ret;
        }

        public List<ShareDocVM> RetrieveYesterdayDocs()
        {
            var ret = new List<ShareDocVM>();
            string tempdate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            var starttime = tempdate + " 00:00:01";
            var endtime = tempdate + " 23:59:59";

            var sql = "select DOCPJK,DOCType,DOCKey,DOCTag,DOCCreator,DOCDate,DOCFavorTimes from ShareDoc where DOCDate >= '<starttime>' and DOCDate <= '<endtime>'";
            sql = sql.Replace("<starttime>", starttime).Replace("<endtime>",endtime);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
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
                ret.Add(tempvm);
            }
            return ret;
        }

        public static void LikeDoc(string DOCPJK, string DOCKey,string updater)
        {
            var ret = new List<ShareDocVM>();
            var sql = "select a.DOCPJK,a.DOCType,a.DOCKey,a.DOCTag,a.DOCCreator,a.DOCDate,a.DOCPusher,a.DOCFavor,b.DOCFavorTimes,a.DOCPusher from UserLearn a left join ShareDoc b ON a.DOCKey = b.DOCKey where DOCPJK = '<DOCPJK>' and DOCKey = '<DOCKey>' and UserName='<UserName>'";
            sql = sql.Replace("<DOCPJK>", DOCPJK).Replace("<DOCKey>", DOCKey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
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
                tempvm.DOCPusher = Convert.ToString(line[9]);
                ret.Add(tempvm);
            }

            if (ret.Count > 0)
            {
                var sql1 = "Update ShareDoc set DOCFavorTimes = <DOCFavorTimes> where DOCPJK = '<DOCPJK>' and DOCKey = '<DOCKey>'";
                sql1.Replace("<DOCPJK>", DOCPJK).Replace("<DOCKey>", DOCKey).Replace("<DOCFavorTimes>", (ret[0].DOCFavorTimes+1).ToString());
                DBUtility.ExeLocalSqlNoRes(sql1);

                sql1 = "Update UserLearn set DOCFavor='LIKE' where DOCPJK = '<DOCPJK>' and DOCKey = '<DOCKey>' and UserName='<UserName>'";
                sql1.Replace("<DOCPJK>", DOCPJK).Replace("<DOCKey>", DOCKey).Replace("<UserName>", updater);
                DBUtility.ExeLocalSqlNoRes(sql1);

                UserRankViewModel.UpdateUserRank(ret[0].DOCCreator, 3);
                if (!string.IsNullOrEmpty(ret[0].DOCPusher))
                {
                    UserRankViewModel.UpdateUserRank(ret[0].DOCPusher, 3);
                }
            }
        }

        public static List<ShareDocVM> Top10InTenDay()
        {
            var ret = new List<ShareDocVM>();
            var starttime = DateTime.Now.AddDays(-10).ToString();
            var sql = "select top 10 DOCPJK,DOCType,DOCKey,DOCTag,DOCCreator,DOCDate,DOCFavorTimes from ShareDoc where DOCDate >= '<starttime>' order by DOCFavorTimes";
            sql = sql.Replace("<starttime>", starttime);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
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
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            { ret.Add(Convert.ToString(line[0])); }
            return ret;
        }


    }
}