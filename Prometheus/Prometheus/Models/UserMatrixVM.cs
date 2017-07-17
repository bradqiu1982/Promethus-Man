using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
    public class USERAUTH
    {
        public static string SUPER = "Super";
        public static string MANAGE = "Manage";
        public static string ADMIN = "Admin";
        public static string GENERAL = "General";
        public static string NONE = "None";
    }

    public class USERDEPART
    {
        public static string NPI = "NPI";
        public static string OTHER = "Other";
    }

    public class UserMatrixVM
    {

        public static void UpdateUserMatrix(string username, string auth, string depart)
        {
            var sql = "delete from UserMatrix where UserName = '<UserName>'";
            sql = sql.Replace("<UserName>", username);
            DBUtility.ExeLocalSqlNoRes(sql);

            sql = "insert into UserMatrix(UserName,Auth,Depart,databackuptm) values('<UserName>','<Auth>','<Depart>','<databackuptm>')";
            sql = sql.Replace("<UserName>", username).Replace("<Auth>", auth).Replace("<Depart>", depart).Replace("<databackuptm>", DateTime.Now.ToString());
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static Dictionary<string, string> RetrieveUserMatrixAuth()
        {
            var ret = new Dictionary<string, string>();
            var sql = "select UserName,Auth,Depart from UserMatrix";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            foreach (var line in dbret)
            {
                var username = Convert.ToString(line[0]);
                var auth = Convert.ToString(line[1]);
                if (!ret.ContainsKey(username))
                {
                    ret.Add(username, auth);
                }
            }
            return ret;
        }

        public static Dictionary<string, string> RetrieveUserMatrixDepart()
        {
            var ret = new Dictionary<string, string>();
            var sql = "select UserName,Auth,Depart from UserMatrix";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var username = Convert.ToString(line[0]);
                var depart = Convert.ToString(line[2]);
                if (!ret.ContainsKey(username))
                {
                    ret.Add(username, depart);
                }
            }
            return ret;
        }

    }
}