using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
    public class ILikeType
    {
        //1: Useful 2: LogicClear 3: GoodSummary
        public static int Useful = 1;
        public static int LogicClear = 2;
        public static int GoodSummary = 3;
    }
    public class ShareStatus
    {
        public static int ValidStatus = 1;
        public static int InvalidStatus = 0;
    }
    public class VoteStatus
    {
        public static int ValidStatus = 1;
        public static int InvalidStatus = 0;
    }

    public class WeeklyReportShareData
    {
        public int ShareID { set; get; }
        public string IssueKey { set; get; }
        public string IssueSummary { set; get; }
        public string UserName { set; get; }
        public Dictionary<int, int> ILikeType { set; get; }
        public string CreateTime { set; get; }
        public string UpdateTime { set; get; }
    }

    public class WeeklyReportShareVM
    {
        public int ID { set; get; }
        public string IssueKey { set; get; }
        public string UserName { set; get; }
        public int Status { set; get; }
        public string CreateTime { set; get; }
        public string UpdateTime { set; get; }

        public static Dictionary<string, WeeklyReportShareData> GetWRSList()
        {
            var sql = @"select wrs.ID, wrs.IssueKey, i.Summary, wrs.UserName, 
                        wrsv.ILikeType, wrsv.VoteNum, wrs.CreateTime, wrs.UpdateTime
                        from WeeklyReportShare as wrs 
                        left join issue as i on wrs.IssueKey = i.IssueKey
                        left join 
	                        (select shareID, ILikeType, count(*) as VoteNum 
		                        from WeeklyReportShareVote where Status = 1 
		                        group by ShareID, ILikeType) 
                        as wrsv on wrs.ID = wrsv.ShareID order by wrs.CreateTime;";

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            var res = new Dictionary<string, WeeklyReportShareData>();
            if(dbret.Count > 0)
            {
                foreach(var item in dbret)
                {
                    if (res.ContainsKey(Convert.ToString(item[0])))
                    {
                        if (!string.IsNullOrEmpty(item[4].ToString()) && !string.IsNullOrEmpty(item[5].ToString()))
                        {
                            res[Convert.ToString(item[0])].ILikeType.Add(Convert.ToInt32(item[4]), Convert.ToInt32(item[5]));
                        }
                    }
                    else
                    {
                        var tmp = new WeeklyReportShareData();
                        tmp.ShareID = Convert.ToInt32(item[0]);
                        tmp.IssueKey = Convert.ToString(item[1]);
                        tmp.IssueSummary = Convert.ToString(item[2]);
                        tmp.UserName = Convert.ToString(item[3]);
                        tmp.CreateTime = Convert.ToDateTime(dbret[0][6]).ToString("yyyy-MM-dd HH:mm:ss");
                        tmp.UpdateTime = Convert.ToDateTime(dbret[0][7]).ToString("yyyy-MM-dd HH:mm:ss");

                        var tmpILikeType = new Dictionary<int, int>();
                        if (!string.IsNullOrEmpty(item[4].ToString()) && !string.IsNullOrEmpty(item[5].ToString()))
                        {
                            tmpILikeType.Add(Convert.ToInt32(item[4]), Convert.ToInt32(item[5]));
                        }
                        tmp.ILikeType = tmpILikeType;
                        res.Add(tmp.ShareID.ToString(), tmp);
                    }

                }
            }
            return res;

        }

        public static void OpWRS(string iKey, string uName)
        {
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var existData = GetWRS(iKey, uName);
            if(existData.ID > 0)
            {
                UpdateWRS(existData.ID, ShareStatus.ValidStatus, now);
            }
            else
            {
                AddWRS(iKey, uName, now);
            }
        }

        public static void CancelWRS(string iKey, string uName)
        {
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var existData = GetWRS(iKey, uName);
            if (existData.ID > 0)
            {
                UpdateWRS(existData.ID, ShareStatus.InvalidStatus, now);
            }
        }

        public static WeeklyReportShareVM GetWRS(string iKey, string uName)
        {
            var sql = @"select ID, IssueKey, UserName, Status, CreateTime, UpdateTime
                        from WeeklyReportShare where IssueKey = @iKey and UserName = @uName";
            var param = new Dictionary<string, string>();
                param.Add("@iKey", iKey);
                param.Add("@uName", uName);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, param);
            var ret = new WeeklyReportShareVM();
            if(dbret.Count > 0)
            {
                ret.ID = Convert.ToInt32(dbret[0][0]);
                ret.IssueKey = Convert.ToString(dbret[0][1]);
                ret.UserName = Convert.ToString(dbret[0][2]);
                ret.Status = Convert.ToInt32(dbret[0][3]);
                ret.CreateTime = Convert.ToDateTime(dbret[0][4]).ToString("yyyy-MM-dd HH:mm:ss");
                ret.UpdateTime = Convert.ToDateTime(dbret[0][5]).ToString("yyyy-MM-dd HH:mm:ss");
            }

            return ret;

        }

        public static void UpdateWRS(int id, int Status, string uTime)
        {
            var sql = @"update WeeklyReportShare set UpdateTime = @uTime,
                        Status = @Status Where ID = @ID";
            var param = new Dictionary<string, string>();
                param.Add("@uTime", uTime);
                param.Add("@Status", Status.ToString());
                param.Add("@ID", id.ToString());

            DBUtility.ExeLocalSqlNoRes(sql, param);
        }

        public static void AddWRS(string iKey, string uName, string Time)
        {
            var sql = @"insert into WeeklyReportShare(IssueKey, UserName, Status, CreateTime, UpdateTime)
                        values(@iKey, @uName, @Status, @cTime, @uTime)";
            var param = new Dictionary<string, string>();
                param.Add("@iKey", iKey);
                param.Add("@uName", uName);
                param.Add("@Status", ShareStatus.ValidStatus.ToString());
                param.Add("@cTime", Time);
                param.Add("@uTime", Time);
            DBUtility.ExeLocalSqlNoRes(sql, param);
        }
    }


    public class WeeklyReportShareVoteVM
    {
        public int ID { set; get; }
        public int ShareID { set; get; }
        public string UserName { set; get; }
        public int ILikeType { set; get; }
        public int Status { set; get; }
        public string CreateTime { set; get; }
        public string UpdateTime { set; get; }

        public static void OpVote(string sID, string uName, string iType)
        {
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var voteData = GetVote(sID, uName, iType);
            if(voteData.ID > 0)
            {
                UpdateVote(voteData.ID, VoteStatus.ValidStatus, now);
            }
            else
            {
                AddVote(sID, uName, iType, now);
            }
        }

        public static void cancelVote(string sID, string uName, string iType)
        {
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var voteData = GetVote(sID, uName, iType);
            if (voteData != null)
            {
                UpdateVote(voteData.ID, VoteStatus.InvalidStatus, now);
            }
        }

        public static WeeklyReportShareVoteVM GetVote(string sID, string uName, string iType)
        {
            var sql = @"select ID, ShareID, UserName, ILikeType, Status, CreateTime, UpdateTime
                        from WeeklyReportShareVote where ShareID = @sID 
                        and UserName = @uName and ILikeType = @iType";
            var param = new Dictionary<string, string>();
            param.Add("@sID", sID);
            param.Add("@uName", uName);
            param.Add("@iType", iType);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, param);
            var res = new WeeklyReportShareVoteVM();
            if(dbret.Count > 0)
            {
                res.ID = Convert.ToInt32(dbret[0][0]);
                res.ShareID = Convert.ToInt32(dbret[0][1]);
                res.UserName = Convert.ToString(dbret[0][2]);
                res.ILikeType = Convert.ToInt32(dbret[0][3]);
                res.Status = Convert.ToInt32(dbret[0][4]);
                res.CreateTime = Convert.ToDateTime(dbret[0][5]).ToString("yyyy-MM-dd HH:mm:ss");
                res.UpdateTime = Convert.ToDateTime(dbret[0][6]).ToString("yyyy-MM-dd HH:mm:ss");
            }

            return res;
        }

        public static void AddVote(string sID, string uName, string iType, string Time)
        {
            var sql = @"insert into WeeklyReportShareVote(ShareID, 
                    UserName, ILikeType, Status, CreateTime, UpdateTime)
                    values(@sID, @uName, @iType, @Status, @cTime, @uTime)";
            var param = new Dictionary<string, string>();
                param.Add("@sID", sID);
                param.Add("@uName", uName);
                param.Add("@iType", iType);
                param.Add("@Status", VoteStatus.ValidStatus.ToString());
                param.Add("@cTime", Time);
                param.Add("@uTime", Time);
            DBUtility.ExeLocalSqlNoRes(sql, param);
        }

        public static void UpdateVote(int id, int Status, string Time)
        {
            var sql = @"update WeeklyReportShareVote set Status = @Status,
                        UpdateTime = @uTime where ID = @id";
            var param = new Dictionary<string, string>();
                param.Add("@Status", Status.ToString());
                param.Add("@uTime", Time);
                param.Add("@id", id.ToString());
            DBUtility.ExeLocalSqlNoRes(sql, param);
        }

        public static Dictionary<int, Dictionary<int, WeeklyReportShareVoteVM>> GetVoteList(string uName)
        {
            var sql = @"select ID, ShareID, ILikeType, Status, CreateTime, UpdateTime
                        from WeeklyReportShareVote where UserName = @uName";
            var param = new Dictionary<string, string>();
                param.Add("@uName", uName);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, param);
            var res = new Dictionary<int, Dictionary<int, WeeklyReportShareVoteVM>>();

            if (dbret.Count > 0)
            {
                foreach(var item in dbret)
                {
                    var tmp = new WeeklyReportShareVoteVM();
                    tmp.ID = Convert.ToInt32(item[0]);
                    tmp.ShareID = Convert.ToInt32(item[1]);
                    tmp.ILikeType = Convert.ToInt32(item[2]);
                    tmp.Status = Convert.ToInt32(item[3]);
                    tmp.CreateTime = Convert.ToDateTime(item[4]).ToString("yyyy-MM-dd HH:mm:ss");
                    tmp.UpdateTime = Convert.ToDateTime(item[5]).ToString("yyyy-MM-dd HH:mm:ss");

                    if (res.ContainsKey(tmp.ShareID))
                    {
                        res[tmp.ShareID].Add(tmp.ILikeType, tmp);
                    }
                    else
                    {
                        var tmpList = new Dictionary<int, WeeklyReportShareVoteVM>();
                        tmpList.Add(tmp.ILikeType, tmp);
                        res.Add(tmp.ShareID, tmpList);
                    }
                }
            }

            return res;
        }

    }
}