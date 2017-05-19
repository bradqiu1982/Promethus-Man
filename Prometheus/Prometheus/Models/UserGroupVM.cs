using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
    public class UserGroupType
    {
        public static string WorkGroup = "WorkGroup";
        public static string ReportGroup = "ReportGroup";
        public static string LYTGroup = "LYTGroup";
    }

    public class UserGroupVM
    {
        public string GroupID { set; get; }
        public string GroupTag { set; get; }
        public string GroupMember { set; get; }

        public static void AddGroup(string grouptag, string groupmember)
        {
            var sql = "insert into UserGroupVM(GroupID,GroupTag,GroupMember,TimeStamp) values('<GroupID>','<GroupTag>','<GroupMember>','<TimeStamp>')";
            sql = sql.Replace("<GroupID>", IssueViewModels.GetUniqKey()).Replace("<GroupTag>", grouptag).Replace("<GroupMember>", groupmember.ToUpper()).Replace("<TimeStamp>", DateTime.Now.ToString());
            DBUtility.ExeLocalSqlNoRes(sql);
        }
        
        public static void DeleteGroup(string groupid)
        {
            var sql = "delete from UserGroupVM where GroupID = '<GroupID>'";
            sql = sql.Replace("<GroupID>", groupid);
            DBUtility.ExeLocalSqlNoRes(sql);
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
                ret = ret + ";" + kv.Value;
            }//end foreach

            return ret;
        }



    }
}