using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
    public class MachineUserMap
    {
        public MachineUserMap()
        {
            machine = "";
            username = "";
        }

        public static void AddMachineUserMap(string machine, string username,string level,string leader)
        {
            var sql = "delete from machineusermap where machine = '<machine>'";
            sql = sql.Replace("<machine>", machine);
            DBUtility.ExeLocalSqlNoRes(sql);

            var tempname = username.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries)[0].ToUpper().Trim();

            sql = "insert into machineusermap(machine,username,level,leader) values(@machine,@username,@level,@leader)";
            var param = new Dictionary<string, string>();
            param.Add("@machine", machine.ToUpper().Trim());
            param.Add("@username", tempname.ToUpper().Trim());
            param.Add("@level", level);
            param.Add("@leader", leader);

            DBUtility.ExeLocalSqlNoRes(sql, param);
        }

        public static Dictionary<string, string> RetrieveUserMap(string machine = "", string username = "")
        {
            var ret = new Dictionary<string, string>();

            var sql = "select machine,username from machineusermap where 1 = 1";
            if (!string.IsNullOrEmpty(machine))
            {
                sql = sql + " and machine = '<machine>'";
                sql = sql.Replace("<machine>", machine);
            }
            if (!string.IsNullOrEmpty(username))
            {
                sql = sql + " and username = '<username>'";
                sql = sql.Replace("<username>", username);
            }

            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            foreach (var line in dbret)
            {
                var tempvm = new MachineUserMap();
                tempvm.machine = Convert.ToString(line[0]);
                tempvm.username = Convert.ToString(line[1]);
                if (!ret.ContainsKey(tempvm.machine))
                {
                    ret.Add(tempvm.machine, tempvm.username);
                }
            }
            return ret;
        }

        public static Dictionary<string, KeyValuePair<string,string>> LoadUserLevel(string emailcond)
        {
            var ret = new Dictionary<string, KeyValuePair<string, string>>();

            var sql = "select Email_Address,Job_Level,Manager FROM [Oracle_Data].[dbo].[ALL_Active_User_List] where Email_Address in <emailcond> and Email_Address is not null and Job_Level is not null";
            sql = sql.Replace("<emailcond>", emailcond);
            var dbret = DBUtility.ExeFAISqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var usename = Convert.ToString(line[0]).Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries)[0].ToUpper();
                var level = Convert.ToString(line[1]);
                var manager = "";
                if (line[2] != null)
                {
                    manager = Convert.ToString(line[2]);
                }

                if (!ret.ContainsKey(usename))
                {
                    ret.Add(usename, new KeyValuePair<string, string>(level,manager));
                }
            }

            return ret;
        }

        public static bool IsSeniorEmployee(string machine, string username)
        {
            var sql = "select machine,username,level from machineusermap where machine = '<machine>' ";
            sql = sql.Replace("<machine>", machine);

            if (!string.IsNullOrEmpty(username))
            {
                sql = sql + " or username = '<username>'";
                sql = sql.Replace("<username>", username.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries)[0].ToUpper());
            }

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            if (dbret.Count == 0)
            {
                return true;
            }

            try
            {
                foreach (var line in dbret)
                {
                    var level = Convert.ToString(line[2]);
                    if (!string.IsNullOrEmpty(level) && level.Length > 1)
                    {
                        var lv = Convert.ToInt32(level.Substring(1));
                        if (lv >= 7 || lv == 0)
                        {
                            return true;
                        }
                    }//end if
                }//en foreach
            }
            catch (Exception ex) { }

            return false;
        }

        public string machine { set; get; }
        public string username { set; get; }

    }

}