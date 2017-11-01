using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
    public class UserGroupType
    {
        public static string WorkGroup = "Work Group";
        public static string ReportGroup = "Report Group";
        public static string LYTGroup = "LYT Group";
    }

    public class UserGroupVM
    {
        public UserGroupVM()
        {
            GroupID = "";
            GroupTag = "";
            GroupMember = "";
        }

        public string GroupID { set; get; }
        public string GroupTag { set; get; }
        public string GroupMember { set; get; }

        public static void AddGroup(string grouptag, string groupmember)
        {
            var sql = "insert into UserGroupVM(GroupID,GroupTag,GroupMember,TimeStamp,databackuptm) values('<GroupID>','<GroupTag>','<GroupMember>','<TimeStamp>','<databackuptm>')";
            sql = sql.Replace("<GroupID>", IssueViewModels.GetUniqKey()).Replace("<GroupTag>", grouptag)
                .Replace("<GroupMember>", groupmember.ToUpper()).Replace("<TimeStamp>", DateTime.Now.ToString()).Replace("<databackuptm>", DateTime.Now.ToString());
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static void DeleteGroup(string groupid)
        {
            var sql = "delete from UserGroupVM where GroupID = '<GroupID>'";
            sql = sql.Replace("<GroupID>", groupid);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static void EditGroup(string groupid, string groupmember)
        {
            var sql = "update UserGroupVM set GroupMember = '<GroupMember>' where GroupID = '<GroupID>'";
            sql = sql.Replace("<GroupID>", groupid).Replace("<GroupMember>",groupmember);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static UserGroupVM RetrieveOneUserGroup(string groupid)
        {
            var ret = new UserGroupVM();
            var sql = "select GroupID,GroupTag,GroupMember from UserGroupVM where GroupID = '<GroupID>'";
            sql = sql.Replace("<GroupID>", groupid);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            if (dbret.Count > 0)
            {
                ret.GroupID = Convert.ToString(dbret[0][0]);
                ret.GroupTag = Convert.ToString(dbret[0][1]);
                ret.GroupMember = Convert.ToString(dbret[0][2]);
            }
            return ret;
        }

        public static List<UserGroupVM> RetreiveAllGroup()
        {
            var ret = new List<UserGroupVM>();
            var sql = "select GroupID,GroupTag,GroupMember from UserGroupVM order by TimeStamp DESC";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            foreach (var line in dbret)
            {
                var tempgp = new UserGroupVM();
                tempgp.GroupID = Convert.ToString(line[0]);
                tempgp.GroupTag = Convert.ToString(line[1]);
                tempgp.GroupMember = Convert.ToString(line[2]);
                ret.Add(tempgp);
            }
            return ret;
        }
        
        public static string RetreiveUserGroup(string username,string grouptag)
        {
            var ret = string.Empty;

            var sql = "select GroupMember from UserGroupVM where GroupMember like '%<username>%' and GroupTag = '<GroupTag>'";
            sql = sql.Replace("<GroupTag>", grouptag).Replace("<username>", username.ToUpper());
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            var tempdict = new Dictionary<string, bool>();

            foreach (var line in dbret)
            {
                 var groupmembers = Convert.ToString(line[0]).Split(new string[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var g in groupmembers)
                {
                    if (!tempdict.ContainsKey(g.Trim()))
                    {
                        tempdict.Add(g.Trim(), true);
                    }
                }//end foreach
            }//end foreach

            foreach (var kv in tempdict)
            {
                ret = ret + ";" + kv.Key;
            }//end foreach

            return ret;
        }



    }
}