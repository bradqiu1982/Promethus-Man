using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prometheus.Models
{
    public class TimeRank
    {
        public int Rank { set; get; }
        public DateTime UpdateDate { set; get; }
    }

    public class UserRankType
    {
        public static string BASE = "BASE";
        public static string ADMIRE = "ADMIRE";
        public static string VOTE = "VOTE";
        public static string ADDITIONAL = "ADDITIONAL";
        public static string SPECIAL = "SPECIAL";
    }

    public class UserReviewedItem
    {
        public string UserName { set; get; }
        public string PJKey { set; get; }
        public string Summary { set; get; }
        public string Link { set; get; }
        public string UpdateTime { set; get; }
    }

    public class UserKPIVM
    {
        public string RankKey { set; get; }
        public string UserName { set; get; }
        public string RankType { set; get; }
        public string Summary { set; get; }
        public string BackLink { set; get; }
        public int Rank { set; get; }
        public int ADMIRERank { set; get; }
        public DateTime UpdateTime { set; get; }

        public static bool ValueableAttach(string path,Controller ctrl)
        {
            var ext = System.IO.Path.GetExtension(path).Replace(".", "").ToUpper();
            var syscfg = CfgUtility.GetSysConfig(ctrl);
            try
            {
                if (syscfg["VALUEABLEATTACH"].Contains(ext))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            { return false; }
        }

        public static void AddUserDailyRank(string rankkey,string username,string ranktype,string summary,string backlink,int rank)
        {
            if (string.Compare(ranktype, UserRankType.BASE, true) == 0)
            {
                if (RetrieveRankItems(rankkey, UserRankType.BASE).Count > 0)
                {
                    return;
                }
            }

            if (string.Compare(ranktype, UserRankType.SPECIAL, true) == 0)
            {
                if (RetrieveRankItems(rankkey, UserRankType.SPECIAL).Count > 0)
                {
                    return;
                }
            }

            var sql = "insert into UserKPIVM(RankKey,UserName,RankType,Summary,BackLink,Rank,ADMIRERank,UpdateTime,databackuptm) "
                + " values('<RankKey>',N'<UserName>','<RankType>',N'<Summary>',N'<BackLink>',<Rank>,<ADMIRERank>,'<UpdateTime>','<databackuptm>')";
            sql = sql.Replace("<RankKey>", rankkey).Replace("<UserName>", username).Replace("<RankType>", ranktype).Replace("<Summary>", summary)
                .Replace("<BackLink>", backlink).Replace("<Rank>", rank.ToString()).Replace("<ADMIRERank>", "0")
                .Replace("<UpdateTime>",DateTime.Now.ToString()).Replace("<databackuptm>", DateTime.Now.ToString());
            DBUtility.ExeLocalSqlNoRes(sql);

            UpdateUserTotalRank(username, rank);
        }

        public static void AddUserAttachDailyRank(string rankkey, string username, string ranktype, string summary, string backlink, int rank,string filename,Controller ctrl)
        {
            if (!ValueableAttach(filename, ctrl))
                return;

            if (string.Compare(ranktype, UserRankType.VOTE, true) != 0)
            {
                    return;
            }

            var sql = "insert into UserKPIVM(RankKey,UserName,RankType,Summary,BackLink,Rank,ADMIRERank,UpdateTime,databackuptm) "
                + " values('<RankKey>',N'<UserName>','<RankType>',N'<Summary>',N'<BackLink>',<Rank>,<ADMIRERank>,'<UpdateTime>','<databackuptm>')";
            sql = sql.Replace("<RankKey>", rankkey).Replace("<UserName>", username).Replace("<RankType>", ranktype).Replace("<Summary>", summary)
                .Replace("<BackLink>", backlink).Replace("<Rank>", rank.ToString()).Replace("<ADMIRERank>", "0")
                .Replace("<UpdateTime>", DateTime.Now.ToString()).Replace("<databackuptm>", DateTime.Now.ToString());
            DBUtility.ExeLocalSqlNoRes(sql);

            UpdateUserTotalRank(username, rank);


            //if (!ValueableAttach(filename, ctrl))
            //    return;

            //if (string.Compare(ranktype, UserRankType.BASE, true) == 0)
            //{
            //    if (RetrieveRankItems(rankkey, UserRankType.BASE).Count > 0)
            //    {
            //        return;
            //    }
            //}

            //if (string.Compare(ranktype, UserRankType.SPECIAL, true) == 0)
            //{
            //    if (RetrieveRankItems(rankkey, UserRankType.SPECIAL).Count > 0)
            //    {
            //        return;
            //    }
            //}

            //var sql = "insert into UserKPIVM(RankKey,UserName,RankType,Summary,BackLink,Rank,ADMIRERank,UpdateTime) "
            //    + " values('<RankKey>',N'<UserName>','<RankType>',N'<Summary>',N'<BackLink>',<Rank>,<ADMIRERank>,'<UpdateTime>')";
            //sql = sql.Replace("<RankKey>", rankkey).Replace("<UserName>", username).Replace("<RankType>", ranktype).Replace("<Summary>", summary)
            //    .Replace("<BackLink>", backlink).Replace("<Rank>", rank.ToString()).Replace("<ADMIRERank>", "0")
            //    .Replace("<UpdateTime>", DateTime.Now.ToString());
            //DBUtility.ExeLocalSqlNoRes(sql);

            //UpdateUserTotalRank(username, rank);
        }

        public static List<UserKPIVM> RetrieveRankItems(string rankkey,string ranktype)
        {
            var ret = new List<UserKPIVM>();
            var sql = "select RankKey,UserName,RankType,Summary,BackLink,Rank,ADMIRERank,UpdateTime from UserKPIVM where RankKey='<RankKey>' and RankType='<RankType>'";
            sql = sql.Replace("<RankKey>", rankkey).Replace("<RankType>", ranktype);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            foreach (var line in dbret)
            {
                var tempvm = new UserKPIVM();
                tempvm.RankKey = Convert.ToString(line[0]);
                tempvm.UserName = Convert.ToString(line[1]);
                tempvm.RankType = Convert.ToString(line[2]);
                tempvm.Summary = Convert.ToString(line[3]);
                tempvm.BackLink = Convert.ToString(line[4]);
                tempvm.Rank = Convert.ToInt32(line[5]);
                tempvm.ADMIRERank = Convert.ToInt32(line[6]);
                tempvm.UpdateTime = DateTime.Parse(Convert.ToString(line[7]));
                ret.Add(tempvm);
            }
            return ret;
        }


        public static void UpdateAdmireRank(string rankkey, int admirerank,string updatetime)
        {
            var baserankitem = RetrieveRankItems(rankkey, UserRankType.BASE);
            var specialitem = RetrieveRankItems(rankkey, UserRankType.SPECIAL);
            var admirebaseitem = new List<UserKPIVM>();
            admirebaseitem.AddRange(baserankitem);
            admirebaseitem.AddRange(specialitem);
            if (admirebaseitem.Count == 0)
                return;

            var admirerankitem = RetrieveRankItems(rankkey, UserRankType.ADMIRE);
            if (admirerankitem.Count > 0)
                return;
            
            AddUserDailyRank(admirebaseitem[0].RankKey, admirebaseitem[0].UserName, UserRankType.ADMIRE, admirebaseitem[0].Summary, admirebaseitem[0].BackLink, admirerank);
            var sql = "update UserKPIVM set ADMIRERank = <ADMIRERank> where RankKey='<RankKey>' and UpdateTime = '<UpdateTime>'";
            sql = sql.Replace("<RankKey>", rankkey).Replace("<ADMIRERank>", admirerank.ToString()).Replace("<UpdateTime>",updatetime);
            DBUtility.ExeLocalSqlNoRes(sql);
            
            UpdateUserTotalRank(admirebaseitem[0].UserName, admirerank);
        }

        public static List<UserKPIVM> RetrieveRankByUserName(string UserName, string starttime)
        {
            var ret = new List<UserKPIVM>();
            var sql = "select RankKey,UserName,RankType,Summary,BackLink,Rank,ADMIRERank,UpdateTime from UserKPIVM where UserName=N'<UserName>' and UpdateTime >= '<UpdateTime>' order by UpdateTime DESC";
            sql = sql.Replace("<UserName>", UserName).Replace("<UpdateTime>", starttime);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var tempvm = new UserKPIVM();
                tempvm.RankKey = Convert.ToString(line[0]);
                tempvm.UserName = Convert.ToString(line[1]);
                tempvm.RankType = Convert.ToString(line[2]);
                tempvm.Summary = Convert.ToString(line[3]);
                tempvm.BackLink = Convert.ToString(line[4]);
                tempvm.Rank = Convert.ToInt32(line[5]);
                tempvm.ADMIRERank = Convert.ToInt32(line[6]);
                tempvm.UpdateTime = DateTime.Parse(Convert.ToString(line[7]));
                ret.Add(tempvm);
            }
            return ret;
        }

        public static List<UserKPIVM> RetrieveRank4Admire(string UserName, string starttime)
        {
            var ret = new List<UserKPIVM>();
            var sql = "select RankKey,UserName,RankType,Summary,BackLink,Rank,ADMIRERank,UpdateTime from UserKPIVM where UserName=N'<UserName>' and UpdateTime >= '<UpdateTime>' and RankType <> '<RankType1>' and RankType <> '<RankType2>'  order by UpdateTime DESC";
            sql = sql.Replace("<UserName>", UserName).Replace("<UpdateTime>", starttime).Replace("<RankType1>", UserRankType.ADMIRE).Replace("<RankType2>", UserRankType.VOTE);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var tempvm = new UserKPIVM();
                tempvm.RankKey = Convert.ToString(line[0]);
                tempvm.UserName = Convert.ToString(line[1]);
                tempvm.RankType = Convert.ToString(line[2]);
                tempvm.Summary = Convert.ToString(line[3]);
                tempvm.BackLink = Convert.ToString(line[4]);
                tempvm.Rank = Convert.ToInt32(line[5]);
                tempvm.ADMIRERank = Convert.ToInt32(line[6]);
                tempvm.UpdateTime = DateTime.Parse(Convert.ToString(line[7]));
                ret.Add(tempvm);
            }
            return ret;
        }

        public static List<UserKPIVM> RetrieveRankByDate(string starttime)
        {
            var ret = new List<UserKPIVM>();
            var sql = "select RankKey,UserName,RankType,Summary,BackLink,Rank,ADMIRERank,UpdateTime from UserKPIVM where UpdateTime >= '<UpdateTime>' and RankType <> '<RankType1>' and RankType <> '<RankType2>' order by UserName,UpdateTime DESC";
            sql = sql.Replace("<UpdateTime>", starttime).Replace("<RankType1>", UserRankType.ADMIRE).Replace("<RankType2>", UserRankType.VOTE);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var tempvm = new UserKPIVM();
                tempvm.RankKey = Convert.ToString(line[0]);
                tempvm.UserName = Convert.ToString(line[1]);
                tempvm.RankType = Convert.ToString(line[2]);
                tempvm.Summary = Convert.ToString(line[3]);
                tempvm.BackLink = Convert.ToString(line[4]);
                tempvm.Rank = Convert.ToInt32(line[5]);
                tempvm.ADMIRERank = Convert.ToInt32(line[6]);
                tempvm.UpdateTime = DateTime.Parse(Convert.ToString(line[7]));
                ret.Add(tempvm);
            }
            return ret;
        }


        private static void UpdateUserTotalRank(string username, int rank)
        {
            var sql = "select TotalRank from UserTable where UserName = N'<UserName>'";
            sql = sql.Replace("<UserName>", username);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            if (dbret.Count > 0)
            {
                var temprank = 0;
                try { temprank = Convert.ToInt32(Convert.ToString(dbret[0][0])); }
                catch (Exception ex) { temprank = 0; }
                temprank = temprank + rank;

                sql = "Update UserTable set TotalRank = '<TotalRank>' where  UserName = N'<UserName>'";
                sql = sql.Replace("<UserName>", username).Replace("<TotalRank>", Convert.ToString(temprank));
                DBUtility.ExeLocalSqlNoRes(sql);
            }
        }

        public static string RetrieveTotalRank(string username)
        {
            var sql = "select TotalRank from UserTable where UserName = N'<UserName>'";
            sql = sql.Replace("<UserName>", username);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            if (dbret.Count > 0)
            {
                var temprank = 0;
                try { temprank = Convert.ToInt32(Convert.ToString(dbret[0][0])); }
                catch (Exception ex) { temprank = 0; }
                return Convert.ToString(temprank);
            }
            else
            {
                return "0";
            }
        }

        public static void StoreUserReviewed(string UserName, string pjkey, string Summary, string Link)
        {
            var sql = "insert into UserReviewedItems(UserName,PJKey,Summary,Link,UpdateTime,databackuptm) values('<UserName>','<PJKey>',N'<Summary>',N'<Link>','<UpdateTime>','<databackuptm>')";
            sql = sql.Replace("<UserName>", UserName).Replace("<PJKey>", pjkey).Replace("<Summary>", Summary)
                .Replace("<Link>", Link).Replace("<UpdateTime>",DateTime.Now.ToString()).Replace("<databackuptm>", DateTime.Now.ToString());
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static List<UserReviewedItem> RetrieveUserReviewed(string starttime)
        {
            var ret = new List<UserReviewedItem>();
            var sql = "select UserName,PJKey,Summary,Link,UpdateTime from UserReviewedItems where UpdateTime >= '<UpdateTime>' order by UpdateTime DESC";
            sql = sql.Replace("<UpdateTime>", starttime);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);

            foreach (var line in dbret)
            {
                var tempvm = new UserReviewedItem();
                tempvm.UserName = Convert.ToString(line[0]);
                tempvm.PJKey = Convert.ToString(line[1]);
                tempvm.Summary = Convert.ToString(line[2]);
                tempvm.Link = Convert.ToString(line[3]);
                tempvm.UpdateTime = Convert.ToString(line[4]);
                ret.Add(tempvm);
            }
            return ret;
        }


    }



}