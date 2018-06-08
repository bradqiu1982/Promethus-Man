using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mail;
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
            var sql = "insert into UserTable(UserName,PassWD,UpdateDate,databackuptm) values(N'<UserName>','<PassWD>','<UpdateDate>','<databackuptm>')";
            sql = sql.Replace("<UserName>", Email.ToUpper()).Replace("<PassWD>", Password).Replace("<UpdateDate>", UpdateDate.ToString()).Replace("<databackuptm>", DateTime.Now.ToString());
            DBUtility.ExeLocalSqlNoRes(sql);

            //insert into n_user
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            sql = @"insert into n_User(Name, Password, Email, Tel, JobNum, 
                Status, CreateAt, UpdateAt) values(@Name, @Password, @Email,
                @Tel, @JobNum, @Status, @CreateAt, @UpdateAt)";

            var param = new Dictionary<string, string>();
            param.Add("@Name", Email.ToUpper().Split(new string[] {"@"}, StringSplitOptions.RemoveEmptyEntries)[0].Replace(".", " "));
            param.Add("@Password", Password);
            param.Add("@Email", Email.ToUpper());
            param.Add("@Tel", "");
            param.Add("@JobNum", "1");
            param.Add("@Status", RBACStatus.ValidStatus.ToString());
            param.Add("@CreateAt", now);
            param.Add("@UpdateAt", now);

            DBUtility.ExeLocalSqlNoRes(sql, param);
        }

        public static UserViewModels RetrieveUser(string username)
        {
            var sql = "select PassWD,Validated,Priority,UpdateDate from UserTable where UserName = N'<UserName>'";
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
            var sql = "update UserTable set Validated = 1 where UserName = N'<UserName>'";
            sql = sql.Replace("<UserName>", username.ToUpper());
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static void ValidateUserWithDate(string username, DateTime date)
        {
            var sql = "update UserTable set Validated = 1 where UserName = N'<UserName>' and UpdateDate = '<UpdateDate>'";
            sql = sql.Replace("<UpdateDate>", date.ToString()).Replace("<UserName>", username.ToUpper());
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static void UpdateUserTime(string username,DateTime date)
        {
            var sql = "update UserTable set UpdateDate = '<UpdateDate>' where UserName = N'<UserName>'";
            sql = sql.Replace("<UpdateDate>", date.ToString()).Replace("<UserName>", username.ToUpper());
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static void RestPwd(string username, string pwd)
        {
            var sql = "update UserTable set PassWD = '<PassWD>' where UserName = N'<UserName>'";
            sql = sql.Replace("<UserName>", username.ToUpper()).Replace("<PassWD>", pwd);
            DBUtility.ExeLocalSqlNoRes(sql);

            //update n_user Password
            sql = @"update n_User set Password = @pwd where Email = @Email ";
            var param = new Dictionary<string, string>();
            param.Add("@pwd", pwd);
            param.Add("@Email", username.ToUpper());
            DBUtility.ExeLocalSqlNoRes(sql, param);
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
            var sql = "insert into UserNet(ME,OTHER,databackuptm) values('<ME>','<OTHER>','<databackuptm>')";
            sql = sql.Replace("<ME>", me.ToUpper()).Replace("<OTHER>", other.ToUpper()).Replace("<databackuptm>", DateTime.Now.ToString());
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

        public static bool IsEmaileValid(string emailaddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailaddress);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }


        public static void RegisterUserAuto(string name)
        {
            var dbret = UserViewModels.RetrieveUser(name);
            if (dbret == null)
            {
                var tempname = name.ToUpper();
                if (!name.Contains("@"))
                {
                    tempname = (name.Replace(" ", ".") + "@finisar.com").ToUpper();
                }

                if (!IsEmaileValid(tempname))
                {
                    return;
                }

                var user = new UserViewModels();
                user.Email = tempname;
                user.Password = "abc@123";
                user.UpdateDate = DateTime.Now;
                user.RegistUser();
                UserViewModels.ActiveUser(user.Email);
            }
        }

        public static Dictionary<string, bool> RetrieveUserProjectKeyDict(string username)
        {
            var dict = new Dictionary<string, bool>();
            var us = RetrieveUserProjectKeyStr(username);

            if (!string.IsNullOrEmpty(us))
            {
                var pjs = us.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var pj in pjs)
                {
                    var onepj = ProjectViewModels.RetrieveOneProject(pj);
                    if (onepj!=null && !dict.ContainsKey(pj))
                    {
                        dict.Add(pj, true);
                    }
                }//end foreach
            }//end if

            return dict;
        }

        public static Dictionary<string, bool> N_RetrieveUserProjectKeyDict(string username)
        {
            var dict = new Dictionary<string, bool>();
            var us = RetrieveUserProjectKeyStr(username);

            if (!string.IsNullOrEmpty(us))
            {
                var pjs = us.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var pj in pjs)
                {
                    var onepj = ProjectViewModels.N_RetrieveProjectInfo(pj);
                    if (onepj != null && !dict.ContainsKey(pj))
                    {
                        dict.Add(pj, true);
                    }
                }//end foreach
            }//end if

            return dict;
        }

        public static string RetrieveUserProjectKeyStr(string username)
        {
            var sql = "select APVal1 from UserTable where UserName = N'<UserName>'";
            sql = sql.Replace("<UserName>", username.ToUpper());
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            if (dbret.Count > 0)
            {
                return Convert.ToString(dbret[0][0]);
            }
            return string.Empty;
        }

        public static void UpdateUserProject(string username, string projectkey)
        {
            var us = RetrieveUserProjectKeyStr(username);
            if (string.IsNullOrEmpty(us))
            {
                var sql = "update UserTable set APVal1 = '<APVal1>' where UserName = N'<UserName>'";
                sql = sql.Replace("<UserName>", username.ToUpper()).Replace("<APVal1>", projectkey);
                DBUtility.ExeLocalSqlNoRes(sql);
            }
            else
            {
                var dict = new Dictionary<string, bool>();
                var pjs = us.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                if (!pjs.Contains(projectkey))
                {
                    var sql = "update UserTable set APVal1 = '<APVal1>' where UserName = N'<UserName>'";
                    sql = sql.Replace("<UserName>", username.ToUpper()).Replace("<APVal1>", us+";"+projectkey);
                    DBUtility.ExeLocalSqlNoRes(sql);
                }//end if
            }
        }

        public static void RemovePJfromUser(string username, string projectkey)
        {
            var us = RetrieveUserProjectKeyStr(username);
            if (!string.IsNullOrEmpty(us))
            {
                var newpj = us.Replace(projectkey+";", "");
                newpj = newpj.Replace(";"+projectkey, "");
                newpj = newpj.Replace(projectkey, "");

                var sql = "update UserTable set APVal1 = '<APVal1>' where UserName = N'<UserName>'";
                sql = sql.Replace("<UserName>", username.ToUpper()).Replace("<APVal1>", newpj);
                DBUtility.ExeLocalSqlNoRes(sql);
            }
        }

        public static Dictionary<string, ProjectViewModels> GetUserProjects(string uName, List<string> pKeys)
        {
            var sql1 = @"select distinct i.ProjectKey, p.APVal4 from issue as i 
                    inner join Project as p on i.ProjectKey = p.ProjectKey 
                    where i.Assignee = @Username and i.APVal1 != 'delete' and p.APVal5 != 'Close' ";
            var sql2 = "";
            if(pKeys.Count > 0)
            {
                sql2 += @" union all
                    select ProjectKey, APVal4 from project where projectkey
					in (<#projectkey>)  and APVal5 != 'Close'";
                sql2 = sql2.Replace("<#projectkey>", "'" + string.Join("','", pKeys) + "'");
            }
            var sql = string.IsNullOrEmpty(sql2) ? sql1 
                        : ("select distinct ProjectKey, APVal4 from ( " + sql1 + sql2 + " ) as tmp_a");
            var param = new Dictionary<string, string>();
            param.Add("@Username", uName);

            var data = DBUtility.ExeLocalSqlWithRes(sql, null, param);
            var res = new Dictionary<string, ProjectViewModels>();
            if(data.Count > 0)
            {
                foreach(var item in data)
                {
                    var tmp = new ProjectViewModels();
                    tmp.ProjectKey = Convert.ToString(item[0]);
                    tmp.TransferFlg = Convert.ToString(item[1]);
                    res.Add(Convert.ToString(item[0]), tmp);
                }
            }
            return res;
        }


    }

}