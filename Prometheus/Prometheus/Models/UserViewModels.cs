using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
    public class UserViewModels
    {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }


        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public int Validated { get; set; }

        public int Priority { get; set; }

        public DateTime UpdateDate { get; set; }

        public static bool CheckUserExist(string username)
        {
            var dbret = RetrieveUser(username);
            if (dbret != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void RegistUser()
        {
            var sql = "insert into UserTable(UserName,PassWD,UpdateDate) values('<UserName>','<PassWD>','<UpdateDate>')";
            sql = sql.Replace("<UserName>", Email.ToUpper()).Replace("<PassWD>", Password).Replace("<UpdateDate>", UpdateDate.ToString());
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static UserViewModels RetrieveUser(string username)
        {
            var sql = "select PassWD,Validated,Priority,UpdateDate from UserTable where UserName = '<UserName>'";
            sql = sql.Replace("<UserName>", username.ToUpper());
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            if (dbret.Count > 0)
            {
                var ret = new UserViewModels();
                ret.Email = username.ToUpper();
                ret.Password = Convert.ToString(dbret[0][0]);
                ret.Validated = Convert.ToInt32(dbret[0][1]);
                ret.Priority = Convert.ToInt32(dbret[0][2]);
                ret.UpdateDate = DateTime.Parse(Convert.ToString(dbret[0][3]));
                return ret;
            }
            return null;
        }

        public static void ActiveUser(string username)
        {
            var sql = "update UserTable set Validated = 1 where UserName = '<UserName>'";
            sql = sql.Replace("<UserName>", username.ToUpper());
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static void ValidateUserWithDate(string username, DateTime date)
        {
            var sql = "update UserTable set Validated = 1 where UserName = '<UserName>' and UpdateDate = '<UpdateDate>'";
            sql = sql.Replace("<UpdateDate>", date.ToString()).Replace("<UserName>", username.ToUpper());
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static void UpdateUserTime(string username,DateTime date)
        {
            var sql = "update UserTable set UpdateDate = '<UpdateDate>' where UserName = '<UserName>'";
            sql = sql.Replace("<UpdateDate>", date.ToString()).Replace("<UserName>", username.ToUpper());
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static void RestPwd(string username, string pwd)
        {
            var sql = "update UserTable set PassWD = '<PassWD>' where UserName = '<UserName>'";
            sql = sql.Replace("<UserName>", username.ToUpper()).Replace("<PassWD>", pwd);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static List<string> RetrieveAllUser()
        {
            var sql = "select UserName from UserTable";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            var ret = new List<string>();

            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString(line[0]));
            }

            ret.Sort();
            return ret;
        }

        public static void AddICare(string me,string other)
        {
            RemoveICare(me, other);
            var sql = "insert into UserNet(ME,OTHER) values('<ME>','<OTHER>')";
            sql = sql.Replace("<ME>", me.ToUpper()).Replace("<OTHER>", other.ToUpper());
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static void RemoveICare(string me, string other)
        {
            var sql = "delete from UserNet where ME='<ME>' and OTHER='<OTHER>'";
            sql = sql.Replace("<ME>", me.ToUpper()).Replace("<OTHER>", other.ToUpper());
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static List<string> RetrieveICare(string me)
        {
            var sql = "select OTHER from UserNet where  ME='<ME>'";
            sql = sql.Replace("<ME>", me);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            var ret = new List<string>();

            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString(line[0]));
            }

            ret.Sort();
            return ret;
        }

    }

}