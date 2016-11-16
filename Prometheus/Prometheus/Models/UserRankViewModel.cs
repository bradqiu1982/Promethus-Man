using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
    public class TimeRank
    {
        public int Rank { set; get; }
        public DateTime UpdateDate { set; get; }
    }

    public class UserRankViewModel
    {

        private static void UpdateUserTotalRank(string username, int rank)
        {
            var sql = "select TotalRank from UserTable where UserName = '<UserName>'";
            sql = sql.Replace("<UserName>", username);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            if (dbret.Count > 0)
            {
                var temprank = 0;
                try { temprank = Convert.ToInt32(Convert.ToString(dbret[0][0])); }
                catch (Exception ex) { temprank = 0; }
                temprank = temprank + rank;

                sql = "Update UserTable set TotalRank = '<TotalRank>' where  UserName = '<UserName>'";
                sql = sql.Replace("<UserName>", username).Replace("<TotalRank>", Convert.ToString(temprank));
                DBUtility.ExeLocalSqlNoRes(sql);
            }
        }

        private static void UpdateUserDailyRank(string username, int rank)
        {
            var updatetime = DateTime.Now.ToString("yyyy-MM-dd") + " 07:30:00";

            var sql = "select Rank from UserRank where UserName = '<UserName>' and UpdateDate = '<UpdateDate>'";
            sql = sql.Replace("<UserName>", username).Replace("<UpdateDate>", updatetime);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            if (dbret.Count > 0)
            {
                var temprank = 0;
                try { temprank = Convert.ToInt32(Convert.ToString(dbret[0][0])); }
                catch (Exception ex) { temprank = 0; }
                temprank = temprank + rank;

                sql = "Update UserRank set Rank = '<Rank>' where  UserName = '<UserName>' and UpdateDate = '<UpdateDate>'";
                sql = sql.Replace("<UserName>", username).Replace("<UpdateDate>", updatetime).Replace("<Rank>", Convert.ToString(temprank));
                DBUtility.ExeLocalSqlNoRes(sql);
            }
            else
            {
                sql = "Insert into UserRank(UserName,UpdateDate,Rank) values('<UserName>','<UpdateDate>','<Rank>')";
                sql = sql.Replace("<UserName>", username).Replace("<UpdateDate>", updatetime).Replace("<Rank>", Convert.ToString(rank));
                DBUtility.ExeLocalSqlNoRes(sql);
            }
        }

        public static void UpdateUserRank(string username, int rank)
        {
            UpdateUserTotalRank(username.ToUpper(), rank);
            UpdateUserDailyRank(username.ToUpper(), rank);
        }

        public static string RetrieveTotalRank(string username)
        {
            var sql = "select TotalRank from UserTable where UserName = '<UserName>'";
            sql = sql.Replace("<UserName>", username);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
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

        public static List<TimeRank> RetrieveRankByMonth(string username,int month)
        {
            var endtime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " 07:30:00");
            var stattime = endtime.AddMonths(0 - month);
            stattime = stattime.AddHours(-1);

            var ret = new List<TimeRank>();

            for (var temptime = stattime; temptime < endtime;)
            {
                var sql = "select Rank,UpdateDate from UserRank where UserName = '<UserName>' and UpdateDate > '<UpdateDate1>' and UpdateDate < '<UpdateDate2>'";
                sql = sql.Replace("<UserName>", username)
                    .Replace("<UpdateDate1>", temptime.ToString("yyyy-MM-dd") + " 06:30:00")
                    .Replace("<UpdateDate2>", temptime.ToString("yyyy-MM-dd") + " 08:30:00");

                var dbret = DBUtility.ExeLocalSqlWithRes(sql);
                if (dbret.Count > 0)
                {
                    var temprank = new TimeRank();
                    try
                    { temprank.Rank = Convert.ToInt32(dbret[0][0]); }
                    catch (Exception ex)
                    { temprank.Rank = 0; }
                    temprank.UpdateDate = DateTime.Parse(Convert.ToString(dbret[0][1]));
                    ret.Add(temprank);
                }
                else
                {
                    var temprank = new TimeRank();
                    temprank.Rank = 0;
                    temprank.UpdateDate = DateTime.Parse(temptime.ToString("yyyy-MM-dd") + " 07:30:00");
                    ret.Add(temprank);
                }

                temptime = temptime.AddDays(1);
            }

            return ret;
        }

    }
}