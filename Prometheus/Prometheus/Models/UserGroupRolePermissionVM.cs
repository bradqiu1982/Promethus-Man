using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
    public class PageVM
    {
        public static int PageSize = 10;
    }
    public class RBACStatus
    {
        public static int InValidStatus = 0;
        public static int ValidStatus = 1;
        public static int NoVerifiedStatus = 2;
    }

    public class PermissionRequestStatus
    {
        public static int Invalid = 0;
        public static int Pending = 1;
        public static int Approved = 2;
        public static int Complete = 3;
        public static int Deny = 4;
        public static int Cancel = 5;
        public static Dictionary<int, string> Msg = new Dictionary<int, string>()
        {
            { 0, "Invalid"},
            { 1, "Pending"},
            { 2, "Approved"},
            { 3, "Complete"},
            { 4, "Deny"},
            { 5, "Cancel"},
        };
    }

    public class PermissionOperateUrl
    {
        public static string Approve = "/Permission/ApprovePermissionRequest";
        public static string Deny = "/Permission/DenyPermissionRequest";
        public static string Complete = "/Permission/CompletePermissionRequest";
    }

    public class UserGroupRolePermissionVM
    {
        public int UserID { set; get; }
        public string Email { set; get; }
        public int GroupID { set; get; }
        public int RoleID { set; get; }
        public int MenuID { set; get; }
        public int FunctionID { set; get; }
        public int OperationID { set; get; }
        public int PermissionID { set; get; }
        public string MenuUrl { set; get; }
        public string FunctionUrl { set; get; }

        public string MenuName { set; get; }
        public int ParentID { set; get; }

        public static List<UserGroupRolePermissionVM> GetUserGroupRolePermission(int uId = 0, int mId = 0, int fId = 0, string Email = "")
        {
            var res = new List<UserGroupRolePermissionVM>();
            //if (uId == 0 && string.IsNullOrEmpty(Email))
            //{
            //    return res;
            //}
            var sql = @"select nu.ID as UserID, nu.Email, ngr.GroupID,
                    nrp.RoleID, nrp.MenuID, nrp.FunctionID,
                    nrp.OperationID, nrp.ID as PermissionID,
                    nm.Url as MenuUrl, nf.Url as FunctionUrl, 
                    nm.Name as MenuName, nm.ParentID
                    from n_User as nu 
                    left join n_UserGroup as nug on (nu.ID = nug.UserID and nug.Status = 1)
                    left join n_GroupRole as ngr on (nug.GroupID = ngr.GroupID and ngr.Status = 1)
                    left join n_RolePermission as nrp on nrp.RoleID = ngr.RoleID
                    left join n_Menu as nm on (nm.Status = 1 and nrp.MenuID = nm.ID)
                    left join n_Function as nf on (nf.Status = 1 and nrp.FunctionID = nf.ID)
                    left join n_Operation as no on (no.Status = 1 and nrp.OperationID = no.ID)
                    where 1 = 1 ";
            var param = new Dictionary<string, string>();
            if(uId != 0)
            {
                sql += " and nu.ID = @uId";
                param.Add("@uId", uId.ToString());
            }
            if (!string.IsNullOrEmpty(Email))
            {
                sql += " and nu.Email = @Email";
                param.Add("@Email", Email);
            }
            if(mId != 0)
            {
                sql += " and nrp.MenuID = @mId ";
                param.Add("@mId", mId.ToString());
            }
            if(fId != 0)
            {
                sql += " and nrp.FunctionID = @fId ";
                param.Add("@fId", fId.ToString());
            }
            sql += " order by nm.ParentID, nm.OrderID ";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, param);
            if(dbret.Count > 0)
            {
                foreach(var item in dbret)
                {
                    var tmp = new UserGroupRolePermissionVM();
                    tmp.UserID = Convert.ToInt32(item[0]);
                    tmp.Email = Convert.ToString(item[1]);
                    tmp.GroupID = !string.IsNullOrEmpty(Convert.ToString(item[2])) ? Convert.ToInt32(item[2]) : 0;
                    tmp.RoleID = !string.IsNullOrEmpty(Convert.ToString(item[3])) ? Convert.ToInt32(item[3]) : 0;
                    tmp.MenuID = !string.IsNullOrEmpty(Convert.ToString(item[4])) ? Convert.ToInt32(item[4]) : 0;
                    tmp.FunctionID = !string.IsNullOrEmpty(Convert.ToString(item[5])) ? Convert.ToInt32(item[5]) : 0;
                    tmp.OperationID = !string.IsNullOrEmpty(Convert.ToString(item[6])) ? Convert.ToInt32(item[6]) : 0;
                    tmp.PermissionID = !string.IsNullOrEmpty(Convert.ToString(item[7])) ? Convert.ToInt32(item[7]) : 0;
                    tmp.MenuUrl = Convert.ToString(item[8]);
                    tmp.FunctionUrl = Convert.ToString(item[9]);
                    tmp.MenuName = Convert.ToString(item[10]);
                    tmp.ParentID = Convert.ToInt32(item[11]);
                    res.Add(tmp);
                }
            }
            return res;
        }
    }

    public class NUserVM
    {
        public int ID { set; get; }
        public string Name { set; get; }
        public string Password { set; get; }
        public string Email { set; get; }
        public string Tel { set; get; }
        public string JobNum { set; get; }
        public string Status { set; get; }
        public string CreateAt { set; get; }
        public string UpdateAt { set; get; }

        public static NUserVM GetUserInfo(string email)
        {
            var sql = @"select ID, Name, Password, Email, 
                    Tel, JobNum, Status, CreateAt, UpdateAt
                    from n_User where Email = @Email";
            var param = new Dictionary<string, string>();
            param.Add("@Email", email);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, param);
            var res = new NUserVM();
            if (dbret.Count > 0)
            {
                res.ID = Convert.ToInt32(dbret[0][0]);
                res.Name = Convert.ToString(dbret[0][1]);
                res.Password = Convert.ToString(dbret[0][2]);
                res.Email = Convert.ToString(dbret[0][3]);
                res.Tel = Convert.ToString(dbret[0][4]);
                res.JobNum = Convert.ToString(dbret[0][5]);
                res.Status = Convert.ToString(dbret[0][6]);
                res.CreateAt = Convert.ToString(dbret[0][7]);
                res.UpdateAt = Convert.ToString(dbret[0][8]);
            }

            return res;
        }

        public static void AddUser(NUserVM user)
        {
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var sql = @"insert into n_User (Name, Password, Email, Tel,
                        JobNum, Status, CreateAt, UpdateAt) values(
                        @Name, @Pwd, @Email, @Tel, @JobNum, @Status,
                        @CreateAt, @UpdateAt)";
            var param = new Dictionary<string, string>();
            param.Add("@Name", user.Name);
            param.Add("@Pwd", user.Password);
            param.Add("@Email", user.Email);
            param.Add("@Tel", user.Tel);
            param.Add("@JobNum", user.JobNum);
            param.Add("@Status", user.Status);
            param.Add("@CreateAt", now);
            param.Add("@UpdateAt", now);

            DBUtility.ExeLocalSqlNoRes(sql, param);
        }

        public static void UpdateUser(NUserVM user)
        {
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var sql = @"update n_User set ";
            var param = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(user.Name))
            {
                sql += "Name = @Name, ";
                param.Add("@Name", user.Name);
            }
            if (!string.IsNullOrEmpty(user.Password))
            {
                sql += "Password = @Password, ";
                param.Add("@Password", user.Password);
            }
            if (!string.IsNullOrEmpty(user.Email))
            {
                sql += "Email = @Email, ";
                param.Add("@Email", user.Email);
            }
            if (!string.IsNullOrEmpty(user.Tel))
            {
                sql += "Tel = @Tel, ";
                param.Add("@Tel", user.Tel);
            }
            if (!string.IsNullOrEmpty(user.JobNum))
            {
                sql += "JobNum = @JobNum, ";
                param.Add("@JobNum", user.JobNum);
            }
            if (!string.IsNullOrEmpty(user.Status))
            {
                sql += "Status = @Status, ";
                param.Add("@Status", user.Status);
            }

            sql += "UpdateAt = @UpdateAt ";
            param.Add("@UpdateAt", now);

            sql += "where ID = @ID";
            param.Add("@ID", user.ID.ToString());

            DBUtility.ExeLocalSqlNoRes(sql, param);
        }

        public static void DeleteUser(int Id)
        {
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var param = new Dictionary<string, string>();
            var sql = @"update n_User set Status = @Status,
                UpdateAt = @UpdateAt where ID = @ID";

            param.Add("@Status", RBACStatus.InValidStatus.ToString());
            param.Add("@UpdateAt", now);
            param.Add("@ID", Id.ToString());

            DBUtility.ExeLocalSqlNoRes(sql, param);
        }


        public static List<NUserVM> GetUsersList(string keywords = "")
        {
            var sql = @"select nu.ID, nu.Name, nu.Email, nu.JobNum, nu.Tel,
                nu.CreateAt, nu.UpdateAt
                from n_User as nu 
                where nu.Status = @ValidStatus";
            var param = new Dictionary<string, string>();
            param.Add("@ValidStatus", RBACStatus.ValidStatus.ToString());
            if (!string.IsNullOrEmpty(keywords))
            {
                sql += " and (nu.Name like @Keywords or nu.JobNum like @Keywords or nu.Email like @Keywords)";
                param.Add("@Keywords", "%"+keywords+"%");
            }
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, param);
            var res = new List<NUserVM>();
            if (dbret.Count > 0)
            {
                foreach (var item in dbret)
                {
                    var tmp = new NUserVM();
                    tmp.ID = Convert.ToInt32(item[0]);
                    tmp.Name = Convert.ToString(item[1]);
                    tmp.Email = Convert.ToString(item[2]);
                    tmp.JobNum = Convert.ToString(item[3]);
                    tmp.Tel = Convert.ToString(item[4]);
                    tmp.CreateAt = Convert.ToString(item[5]);
                    tmp.UpdateAt = Convert.ToString(item[6]);
                    res.Add(tmp);
                }
            }
            return res;
        }

    }

    public class NGroupVM
    {
        public int ID { set; get; }
        public string Name { set; get; }
        public string Status { set; get; }
        public string Comment { set; get; }
        public string Members { set; get; }
        public string CreateAt { set; get; }
        public string UpdateAt { set; get; }

        public static List<NGroupVM> GetGroups(string keywords = "", int Id = 0)
        {
            var sql = @"select ng.ID, ng.Name, ng.Status, ng.Comment, 
                    ng.CreateAt, ng.UpdateAt, ISNULL(nu.Members, 0) as Members
                    from n_Group  as ng 
                    left join(select GroupID, count(*) as Members 
                    from n_UserGroup where Status = @ValidStatus group by GroupID) 
                    as nu on ng.Id = nu.GroupID where ng.Status = @ValidStatus";
            var param = new Dictionary<string, string>();
            param.Add("@ValidStatus", RBACStatus.ValidStatus.ToString());
            if (!string.IsNullOrEmpty(keywords))
            {
                sql += " and (ng.Name like @Keywords)";
                param.Add("@Keywords", "%"+keywords+"%");
            }
            if (Id > 0)
            {
                sql += " and ng.ID = @Id";
                param.Add("@Id", Id.ToString());
            }
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, param);
            var res = new List<NGroupVM>();
            if(dbret.Count > 0)
            {
                foreach(var item in dbret)
                {
                    var tmp = new NGroupVM();
                    tmp.ID = Convert.ToInt32(item[0]);
                    tmp.Name = Convert.ToString(item[1]);
                    tmp.Status = Convert.ToString(item[2]);
                    tmp.Comment = Convert.ToString(item[3]);
                    tmp.CreateAt = Convert.ToString(item[4]);
                    tmp.UpdateAt = Convert.ToString(item[5]);
                    tmp.Members = Convert.ToString(item[6]);
                    res.Add(tmp);
                }
            }
            return res;
        }

        public static void UpdateGroup(NGroupVM group)
        {
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var param = new Dictionary<string, string>();
            var sql = @"update n_Group set ";
            if (!string.IsNullOrEmpty(group.Name))
            {
                sql += "Name = @Name, ";
                param.Add("@Name", group.Name);
            }
            if (!string.IsNullOrEmpty(group.Status))
            {
                sql += "Status = @Status, ";
                param.Add("@Status", group.Status);
            }
            if (!string.IsNullOrEmpty(group.Comment))
            {
                sql += "Comment = @Comment, ";
                param.Add("@Comment", group.Comment);
            }
            sql += "UpdateAt = @UpdateAt ";
            param.Add("@UpdateAt", now);

            sql += " where ID = @ID ";
            param.Add("@ID", group.ID.ToString());

            DBUtility.ExeLocalSqlNoRes(sql, param);
        }

        public static void AddGroup(NGroupVM group)
        {
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var sql = @"insert into n_Group (Name, Status, Comment,
                    CreateAt, UpdateAt) values(
                    @Name, @Status, @Comment, @CreateAt, @UpdateAt)";
            var param = new Dictionary<string, string>();
            param.Add("@Name", group.Name);
            param.Add("@Status", group.Status);
            param.Add("@Comment", group.Comment);
            param.Add("@CreateAt", now);
            param.Add("@UpdateAt", now);

            DBUtility.ExeLocalSqlNoRes(sql, param);
        }
    }

    public class NUserGroupVM
    {
        public int ID { set; get; }
        public int UserID { set; get; }
        public int GroupID { set; get; }
        public string Status { set; get; }
        public string CreateAt { set; get; }
        public string UpdateAt { set; get; }
        public string GroupName { set; get; }

        public static List<NUserVM> GetUsersByGroupID(int gId = 0)
        {
            var sql = @"select nu.ID, nu.Name, nu.Email, nu.JobNum, nu.Tel
                from n_UserGroup as nug left join n_User as nu
                on nug.UserID = nu.ID where nug.Status = @ValidStatus 
                and nu.Status = @ValidStatus";
            var param = new Dictionary<string, string>();
            param.Add("@ValidStatus", RBACStatus.ValidStatus.ToString());
            if(gId != 0)
            {
                sql += " and nug.GroupID = @GroupID";
                param.Add("@GroupID", gId.ToString());
            }
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, param);
            var res = new List<NUserVM>();
            if(dbret.Count > 0)
            {
                foreach(var item in dbret)
                {
                    var tmp = new NUserVM();
                    tmp.ID = Convert.ToInt32(item[0]);
                    tmp.Name = Convert.ToString(item[1]);
                    tmp.Email = Convert.ToString(item[2]);
                    tmp.JobNum = Convert.ToString(item[3]);
                    tmp.Tel = Convert.ToString(item[4]);
                    res.Add(tmp);
                }
            }
            return res;
        }
        public static void AddGroupMember(int gId, int uId)
        {
            var gm_data = GetGroupMember(gId, uId);
            if(gm_data.ID > 0)
            {
                if(string.Compare(gm_data.Status, RBACStatus.InValidStatus.ToString()) == 0)
                {
                    UpdateGroupMember(gId, uId, RBACStatus.ValidStatus.ToString());
                }
            }
            else
            {
                InsertGroupMember(gId, uId);
            }
        }
        public static NUserGroupVM GetGroupMember(int gId, int uId)
        {
            var sql = @"select ID, Status from n_UserGroup 
                    Where GroupID = @gId and UserID = @uId";
            var param = new Dictionary<string, string>();
            param.Add("@gId", gId.ToString());
            param.Add("@uId", uId.ToString());
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, param);
           
            var res = new NUserGroupVM();
            if(dbret.Count > 0)
            {
                res.ID = Convert.ToInt32(dbret[0][0]);
                res.Status = Convert.ToString(dbret[0][1]);
            }

            return res;

        }
        public static void DeleteGroupMember(int gId, int uId)
        {
            UpdateGroupMember(gId, uId, RBACStatus.InValidStatus.ToString());
        }
        public static void UpdateGroupMember(int gId, int uId, string Status)
        {
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var sql = @"update n_UserGroup set Status = @Status,
                    UpdateAt = @uTime
                    where GroupID = @gId and UserID = @uId";
            var param = new Dictionary<string, string>();
            param.Add("@Status", Status.ToString());
            param.Add("@gId", gId.ToString());
            param.Add("@uId", uId.ToString());
            param.Add("@uTime", now);

            DBUtility.ExeLocalSqlNoRes(sql, param);
        }
        public static void InsertGroupMember(int gId, int uId)
        {
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var sql = @"insert into n_UserGroup (GroupID, UserID, Status, CreateAt, UpdateAt) 
                    values(@gId, @uId, @Status, @cTime, @uTime)";
            var param = new Dictionary<string, string>();
            param.Add("@gId", gId.ToString());
            param.Add("@uId", uId.ToString());
            param.Add("@Status", RBACStatus.ValidStatus.ToString());
            param.Add("@cTime", now);
            param.Add("@uTime", now);
            DBUtility.ExeLocalSqlNoRes(sql, param);

        }
        public static List<NUserGroupVM> GetMemberGroup(int uId)
        {
            var sql = @"select nug.ID, nug.UserID, nug.GroupID, nug.Status, 
                nug.CreateAt, nug.UpdateAt, ng.Name as GroupName
                from n_UserGroup as nug 
                left join n_Group as ng on nug.GroupID = ng.ID
                where nug.UserID = @uId 
                and nug.Status = 1 and ng.Status = 1";
            var param = new Dictionary<string, string>();
            param.Add("@uId", uId.ToString());
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, param);
            var res = new List<NUserGroupVM>();
            if (dbret.Count > 0)
            {
                foreach (var item in dbret)
                {
                    var tmp = new NUserGroupVM();
                    tmp.ID = Convert.ToInt32(item[0]);
                    tmp.UserID = Convert.ToInt32(item[1]);
                    tmp.GroupID = Convert.ToInt32(item[2]);
                    tmp.Status = Convert.ToString(item[3]);
                    tmp.CreateAt = Convert.ToString(item[4]);
                    tmp.UpdateAt = Convert.ToString(item[5]);
                    tmp.GroupName = Convert.ToString(item[6]);
                    res.Add(tmp);
                }
            }
            return res;
        }
    }

    public class NRoleVM
    {
        public int ID { set; get; }
        public string Name { set; get; }
        public string Status { set; get; }
        public string Comment { set; get; }
        public string CreateAt { set; get; }
        public string UpdateAt { set; get; }

        public static List<NRoleVM> GetRoleList(string status, string rName = "", string keywords = "")
        {
            var sql = @"select ID, Name, Status, Comment, CreateAt, UpdateAt 
                        from n_Role where Status = @Status";
            var param = new Dictionary<string, string>();
            param.Add("@Status", string.IsNullOrEmpty(status) ? RBACStatus.ValidStatus.ToString() : status);
            if (!string.IsNullOrEmpty(keywords))
            {
                sql += " and (Name like @keywords or Comment like @keywords) ";
                param.Add("@keywords", keywords);
            }
            if (!string.IsNullOrEmpty(rName))
            {
                sql += " and Name = @Name ";
                param.Add("@Name", rName);
            }
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, param);
            var res = new List<NRoleVM>();
            if(dbret.Count > 0)
            {
                foreach (var item in dbret) 
                {
                    var tmp = new NRoleVM();
                    tmp.ID = Convert.ToInt32(item[0]);
                    tmp.Name = Convert.ToString(item[1]);
                    tmp.Status = Convert.ToString(item[2]);
                    tmp.Comment = Convert.ToString(item[3]);
                    tmp.CreateAt = Convert.ToString(item[4]);
                    tmp.UpdateAt = Convert.ToString(item[5]);
                    res.Add(tmp);
                }
            }
            
            return res;
        }

        public static void UpdateRole(NRoleVM role)
        {
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var param = new Dictionary<string, string>();
            var sql = @"update n_Role set ";
            if (!string.IsNullOrEmpty(role.Name))
            {
                sql += "Name = @Name, ";
                param.Add("@Name", role.Name);
            }
            if (!string.IsNullOrEmpty(role.Status))
            {
                sql += "Status = @Status, ";
                param.Add("@Status", role.Status);
            }
            if (!string.IsNullOrEmpty(role.Comment))
            {
                sql += "Comment = @Comment, ";
                param.Add("@Comment", role.Comment);
            }
            sql += "UpdateAt = @UpdateAt where ID = @ID";
            param.Add("@UpdateAt", now);
            param.Add("@ID", role.ID.ToString());

            DBUtility.ExeLocalSqlNoRes(sql, param);
        }

        public static void AddRole(NRoleVM role)
        {
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var sql = @"insert into n_Role (Name, Status, Comment,
                    CreateAt, UpdateAt) values(
                    @Name, @Status, @Comment, @CreateAt, @UpdateAt)";
            var param = new Dictionary<string, string>();
            param.Add("@Name", role.Name);
            param.Add("@Status", role.Status);
            param.Add("@Comment", role.Comment);
            param.Add("@CreateAt", now);
            param.Add("@UpdateAt", now);

            DBUtility.ExeLocalSqlNoRes(sql, param);
        }
    }

    public class NGroupRoleVM
    {
        public int ID { set; get; }
        public int GroupID { set; get; }
        public int RoleID { set; get; }
        public string Status { set; get; }
        public string Comment { set; get; }
        public string CreateAt { set; get; }
        public string UpdateAt { set; get; }
        public string GroupName { set; get; }
        public string RoleName { set; get; }

        public static void AddGroupRole(int gId, int rId, string comment)
        {
            var gr_data = GetGroupRole(gId, rId);
            if (gr_data.ID > 0)
            {
                if (string.Compare(gr_data.Status, RBACStatus.InValidStatus.ToString()) == 0)
                {
                    UpdateGroupRole(gId, rId, RBACStatus.ValidStatus);
                }
            }
            else
            {
                InsertGroupRole(gId, rId, comment);
            }
        }

        public static NGroupRoleVM GetGroupRole(int gId, int rId)
        {

            var sql = @"select ID, Status from n_GroupRole 
                    Where GroupID = @gId and RoleID = @rId";
            var param = new Dictionary<string, string>();
            param.Add("@gId", gId.ToString());
            param.Add("@rId", rId.ToString());
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, param);

            var res = new NGroupRoleVM();
            if (dbret.Count > 0)
            {
                res.ID = Convert.ToInt32(dbret[0][0]);
                res.Status = Convert.ToString(dbret[0][1]);
            }

            return res;
        }

        public static List<NGroupRoleVM> GetGroupRoleByGroupID(int gId)
        {
            var sql = @"select gr.ID, gr.GroupID, gr.RoleID, gr.Status,
                    gr.Comment, gr.CreateAt, gr.UpdateAt, ng.Name as GroupName,
                    nr.Name as RoleName from n_GroupRole as gr 
                    left join n_Group as ng on ng.ID = gr.GroupID
                    left join n_Role as nr on nr.ID = gr.RoleID 
                    where gr.GroupID = @gId and gr.Status = @Status
                    and ng.Status = @Status and nr.Status = @Status";
            var param = new Dictionary<string, string>();
            param.Add("@gId", gId.ToString());
            param.Add("@Status", RBACStatus.ValidStatus.ToString());
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, param);
            var res = new List<NGroupRoleVM>();
            if(dbret.Count > 0)
            {
                foreach(var item in dbret)
                {
                    var tmp = new NGroupRoleVM();
                    tmp.ID = Convert.ToInt32(item[0]);
                    tmp.GroupID = Convert.ToInt32(item[1]);
                    tmp.RoleID = Convert.ToInt32(item[2]);
                    tmp.Status = Convert.ToString(item[3]);
                    tmp.Comment = Convert.ToString(item[4]);
                    tmp.CreateAt = Convert.ToString(item[5]);
                    tmp.UpdateAt = Convert.ToString(item[6]);
                    tmp.GroupName = Convert.ToString(item[7]);
                    tmp.RoleName = Convert.ToString(item[8]);
                    res.Add(tmp);
                }
            }
            return res;
        }

        public static void UpdateGroupRole(int gId, int rId, int status)
        {
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var sql = @"update n_GroupRole set Status = @Status,
                    UpdateAt = @uTime
                    where GroupID = @gId and RoleID = @rId";
            var param = new Dictionary<string, string>();
            param.Add("@Status", status.ToString());
            param.Add("@gId", gId.ToString());
            param.Add("@rId", rId.ToString());
            param.Add("@uTime", now);

            DBUtility.ExeLocalSqlNoRes(sql, param);
        }

        public static void InsertGroupRole(int gId, int rId, string comment)
        {
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var sql = @"insert into n_GroupRole (GroupID, RoleID, Status, Comment, CreateAt, UpdateAt) 
                    values(@gId, @rId, @Status, @Comment, @cTime, @uTime)";
            var param = new Dictionary<string, string>();
            param.Add("@gId", gId.ToString());
            param.Add("@rId", rId.ToString());
            param.Add("@Status", RBACStatus.ValidStatus.ToString());
            param.Add("@Comment", comment);
            param.Add("@cTime", now);
            param.Add("@uTime", now);
            DBUtility.ExeLocalSqlNoRes(sql, param);

        }
    }

    public class NMenuVM
    {
        public int ID { set; get; }
        public string Name { set; get; }
        public string Url { set; get; }
        public string ImgUrl { set; get; }
        public int ParentID { set; get; }
        public int OrderID { set; get; }
        public string Status { set; get; }
        public string CreateAt { set; get; }
        public string UpdateAt { set; get; }
        public string ParentMenu { set; get; }
        public static void AddMenu(NMenuVM menu)
        {
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var sql = @"insert into n_Menu (Name, Url, ImgUrl, ParentID, OrderID, 
                    Status, CreateAt, UpdateAt) values(
                    @Name, @Url, @ImgUrl, @ParentID, @OrderID, @Status, @CreateAt, @UpdateAt)";
            var param = new Dictionary<string, string>();
            param.Add("@Name", menu.Name);
            param.Add("@Url", menu.Url);
            param.Add("@ImgUrl", menu.ImgUrl);
            param.Add("@ParentID", menu.ParentID.ToString());
            param.Add("@OrderID", menu.OrderID.ToString());
            param.Add("@Status", menu.Status);
            param.Add("@CreateAt", now);
            param.Add("@UpdateAt", now);

            DBUtility.ExeLocalSqlNoRes(sql, param);
        }
        public static void UpdateMenu(NMenuVM menu)
        {
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var param = new Dictionary<string, string>();
            var sql = @"update n_Menu set ";
            if (!string.IsNullOrEmpty(menu.Name))
            {
                sql += "Name = @Name, ";
                param.Add("@Name", menu.Name);
            }
            if (!string.IsNullOrEmpty(menu.Url))
            {
                sql += "Url = @Url, ";
                param.Add("@Url", menu.Url);
            }
            if (!string.IsNullOrEmpty(menu.ImgUrl))
            {
                sql += "ImgUrl = @ImgUrl, ";
                param.Add("@ImgUrl", menu.ImgUrl);
            }
            if (menu.ParentID != 0)
            {
                sql += "ParentID = @ParentID, ";
                param.Add("@ParentID", menu.ParentID.ToString());
            }
            if (menu.OrderID != 0)
            {
                sql += "OrderID = @OrderID, ";
                param.Add("@OrderID", menu.OrderID.ToString());
            }
            if (!string.IsNullOrEmpty(menu.Status))
            {
                sql += "Status = @Status, ";
                param.Add("@Status", menu.Status);
            }
            sql += "UpdateAt = @UpdateAt where ID = @ID ";
            param.Add("@UpdateAt", now);
            param.Add("@ID", menu.ID.ToString());

            DBUtility.ExeLocalSqlNoRes(sql, param);
        }
        public static List<NMenuVM> GetMenuList(string keywords = "", int mId = 0, int type = 0)
        {
            var sql = @"select m1.ID, m1.Name, m1.Url, m1.ParentID, 
                    m1.OrderID, m1.Status, m1.CreateAt, m1.UpdateAt, 
                    m2.Name as ParentMenu, m1.ImgUrl
                    from n_Menu as m1 left join n_Menu as m2 
                    on m1.ParentID = m2.ID
                    where m1.Status = @Status ";
            var param = new Dictionary<string, string>();
            param.Add("@Status", RBACStatus.ValidStatus.ToString());
            if (!string.IsNullOrEmpty(keywords))
            {
                sql += " and (m1.Name like @Keywords or m2.Name like @Keywords)";
                param.Add("@Keywords", "%" + keywords + "%");
            }
            if(mId != 0)
            {
                sql += " and ID = @mId";
                param.Add("@mId", mId.ToString());
            }
            if(type == 1)
            {
                sql += " and m1.ParentID != '0' ";
            }
            sql += " order by m1.ParentID, m1.OrderID";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, param);
            var res = new List<NMenuVM>();
            if(dbret.Count > 0)
            {
                foreach(var item in dbret)
                {
                    var tmp = new NMenuVM();
                    tmp.ID = Convert.ToInt32(item[0]);
                    tmp.Name = Convert.ToString(item[1]);
                    tmp.Url = Convert.ToString(item[2]);
                    tmp.ParentID = Convert.ToInt32(item[3]);
                    tmp.OrderID = Convert.ToInt32(item[4]);
                    tmp.Status = Convert.ToString(item[5]);
                    tmp.CreateAt = Convert.ToString(item[6]);
                    tmp.UpdateAt = Convert.ToString(item[7]);
                    tmp.ParentMenu = Convert.ToString(item[8]);
                    tmp.ImgUrl = Convert.ToString(item[9]);
                    res.Add(tmp);
                }
            }

            return res;
        }

        public static NMenuVM GetMenuInfoByUrl(string mUrl)
        {
            var sql = @"select ID, Name, Url, ParentID, OrderID, Status, 
                    CreateAt, UpdateAt, ImgUrl 
                    from n_Menu where Url = @mUrl and Status = @Status";
            var param = new Dictionary<string, string>();
            param.Add("@mUrl", mUrl);
            param.Add("@Status", RBACStatus.ValidStatus.ToString());
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, param);
            var res = new NMenuVM();
            if(dbret.Count > 0)
            {
                var item = dbret[0];
                res.ID = Convert.ToInt32(item[0]);
                res.Name = Convert.ToString(item[1]);
                res.Url = Convert.ToString(item[2]);
                res.ParentID = Convert.ToInt32(item[3]);
                res.OrderID = Convert.ToInt32(item[4]);
                res.Status = Convert.ToString(item[5]);
                res.CreateAt = Convert.ToString(item[6]);
                res.UpdateAt = Convert.ToString(item[7]);
                res.ImgUrl = Convert.ToString(item[8]);
            }
            return res;
        }

        public static List<NMenuVM> GetMenuListByFuncUrl(string fUrl)
        {
            var sql = @"select nm1.ID, nm1.Name, nm1.Url, nm1.ImgUrl, 
                    nm1.ParentID, nm1.OrderID, nm1.Status, nm1.CreateAt, nm1.UpdateAt
                    from n_function as nf 
                    inner join n_Menu as nm on nf.MenuID = nm.ID
                    inner join n_Menu as nm1 on nm.ParentID = nm1.ParentID
                    where nf.Url = @fUrl and nm1.Status = @Status
                    order by nm1.OrderID ";
            var param = new Dictionary<string, string>();
            param.Add("@fUrl", fUrl);
            param.Add("@Status", RBACStatus.ValidStatus.ToString());

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, param);
            var res = new List<NMenuVM>();
            if(dbret.Count > 0)
            {
                foreach(var item in dbret)
                {
                    var tmp = new NMenuVM();
                    tmp.ID = Convert.ToInt32(item[0]);
                    tmp.Name = Convert.ToString(item[1]);
                    tmp.Url = Convert.ToString(item[2]);
                    tmp.ImgUrl = Convert.ToString(item[3]);
                    tmp.ParentID = Convert.ToInt32(item[4]);
                    tmp.OrderID = Convert.ToInt32(item[5]);
                    tmp.Status = Convert.ToString(item[6]);
                    tmp.CreateAt = Convert.ToString(item[7]);
                    tmp.UpdateAt = Convert.ToString(item[8]);
                    res.Add(tmp);
                }
            }
            return res;
        }
    }

    public class NFunctionVM
    {
        public int ID { set; get; }
        public string Name { set; get; }
        public int MenuID { set; get; }
        public string Url { set; get; }
        public string ImgUrl { set; get; }
        public string Status { set; get; }
        public string CreateAt { set; get; }
        public string UpdateAt { set; get; }
        public string MenuName { set; get; }

        public static void AddFunction(NFunctionVM fun)
        {
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var sql = @"insert into n_Function (Name, MenuID, Url, ImgUrl,
                    Status, CreateAt, UpdateAt) values(
                    @Name, @MenuID, @Url, @ImgUrl, @Status, @CreateAt, @UpdateAt)";
            var param = new Dictionary<string, string>();
            param.Add("@Name", fun.Name);
            param.Add("@MenuID", fun.MenuID.ToString());
            param.Add("@Url", fun.Url);
            param.Add("@ImgUrl", fun.ImgUrl);
            param.Add("@Status", fun.Status);
            param.Add("@CreateAt", now);
            param.Add("@UpdateAt", now);

            DBUtility.ExeLocalSqlNoRes(sql, param);
        } 

        public static void UpdateFunction(NFunctionVM fun)
        {
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var param = new Dictionary<string, string>();
            var sql = @"update n_Function set ";
            if (!string.IsNullOrEmpty(fun.Name))
            {
                sql += "Name = @Name, ";
                param.Add("@Name", fun.Name);
            }
            if (fun.MenuID != 0)
            {
                sql += "MenuID = @MenuID, ";
                param.Add("@MenuID", fun.MenuID.ToString());
            }
            if (!string.IsNullOrEmpty(fun.Url))
            {
                sql += "Url = @Url, ";
                param.Add("@Url", fun.Url);
            }
            if (!string.IsNullOrEmpty(fun.ImgUrl))
            {
                sql += "ImgUrl = @ImgUrl, ";
                param.Add("@ImgUrl", fun.ImgUrl);
            }
            if (!string.IsNullOrEmpty(fun.Status))
            {
                sql += "Status = @Status, ";
                param.Add("@Status", fun.Status);
            }
            sql += "UpdateAt = @UpdateAt where ID = @ID ";
            param.Add("@UpdateAt", now);
            param.Add("@ID", fun.ID.ToString());

            DBUtility.ExeLocalSqlNoRes(sql, param);
        }

        public static List<NFunctionVM> GetFunctionList(string keywords = "", string mId = "", string fId = "")
        {
            var sql = @"select nf.ID, nf.Name, nf.MenuID, nf.Url, nf.Status, 
                    nf.CreateAt, nf.UpdateAt, nm.Name as MenuName, nf.ImgUrl
                    from n_Function as nf left join n_Menu as nm on nf.MenuID = nm.ID
                    where nf.Status = @Status";
            var param = new Dictionary<string, string>();
            param.Add("@Status", RBACStatus.ValidStatus.ToString());
            if (!string.IsNullOrEmpty(keywords))
            {
                sql += " and (nf.Name like @Keywords or nm.Name like @Keywords)";
                param.Add("@Keywords", "%" + keywords + "%");
            }
            if(!string.IsNullOrEmpty(fId))
            {
                sql += " and nf.ID = @fId";
                param.Add("@fId", fId);
            }
            if (!string.IsNullOrEmpty(mId))
            {
                sql += " and nf.MenuID = @mId";
                param.Add("@mId", mId);
            }
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, param);
            var res = new List<NFunctionVM>();
            if(dbret.Count > 0)
            {
                foreach (var item in dbret)
                {
                    var tmp = new NFunctionVM();
                    tmp.ID = Convert.ToInt32(item[0]);
                    tmp.Name = Convert.ToString(item[1]);
                    tmp.MenuID = Convert.ToInt32(item[2]);
                    tmp.Url = Convert.ToString(item[3]);
                    tmp.Status = Convert.ToString(item[4]);
                    tmp.CreateAt = Convert.ToString(item[5]);
                    tmp.UpdateAt = Convert.ToString(item[6]);
                    tmp.MenuName = Convert.ToString(item[7]);
                    tmp.ImgUrl = Convert.ToString(item[8]);
                    res.Add(tmp);
                }
            }
            return res;
        }
        public static NFunctionVM GetFunctionInfoByUrl(string fUrl)
        {
            var sql = @"select nf.ID, nf.Name, nf.MenuID, nf.Url, nf.Status, 
                    nf.CreateAt, nf.UpdateAt, nf.ImgUrl, nm.Name as MenuName 
                    from n_Function as nf
                    left join n_Menu as nm on nf.MenuID = nm.ID
                    where nf.Url = @fUrl and nf.Status = @Status";
            var param = new Dictionary<string, string>();
            param.Add("@fUrl", fUrl);
            param.Add("@Status", RBACStatus.ValidStatus.ToString());
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, param);
            var res = new NFunctionVM();
            if (dbret.Count > 0)
            {
                var item = dbret[0];
                res.ID = Convert.ToInt32(item[0]);
                res.Name = Convert.ToString(item[1]);
                res.MenuID = Convert.ToInt32(item[2]);
                res.Url = Convert.ToString(item[3]);
                res.Status = Convert.ToString(item[4]);
                res.CreateAt = Convert.ToString(item[5]);
                res.UpdateAt = Convert.ToString(item[6]);
                res.ImgUrl = Convert.ToString(item[7]);
                res.MenuName = Convert.ToString(item[8]);
            }
            return res;
        }
    }

    public class NOperationVM
    {
        public int ID { set; get; }
        public string Name { set; get; }
        public string Status { set; get; }
        public string CreateAt { set; get; }
        public string UpdateAt { set; get; }

        public static List<NOperationVM> GetOperationList(string keywords = "", int oId = 0)
        {
            var sql = @"select ID, Name, Status, CreateAt, UpdateAt
                    from n_Operation where Status = @Status";
            var param = new Dictionary<string, string>();
            param.Add("@Status", RBACStatus.ValidStatus.ToString());
            if (!string.IsNullOrEmpty(keywords))
            {
                sql += " and Name like @Keywords";
                param.Add("@Keywords", "%" + keywords + "%");
            }
            if(oId != 0)
            {
                sql += " and ID = @oId";
                param.Add("@oId", oId.ToString());
            }
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, param);
            var res = new List<NOperationVM>();
            if(dbret.Count > 0)
            {
                foreach(var item in dbret)
                {
                    var tmp = new NOperationVM();
                    tmp.ID = Convert.ToInt32(item[0]);
                    tmp.Name = Convert.ToString(item[1]);
                    tmp.Status = Convert.ToString(item[2]);
                    tmp.CreateAt = Convert.ToString(item[3]);
                    tmp.UpdateAt = Convert.ToString(item[4]);
                    res.Add(tmp);
                }
            }
            return res;
        }

        public static void AddOperation(NOperationVM op)
        {
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var sql = @"insert into n_Operation (Name, Status, CreateAt, UpdateAt) 
                values(@Name, @Status, @CreateAt, @UpdateAt)";
            var param = new Dictionary<string, string>();
            param.Add("@Name", op.Name);
            param.Add("@Status", op.Status);
            param.Add("@CreateAt", now);
            param.Add("@UpdateAt", now);

            DBUtility.ExeLocalSqlNoRes(sql, param);
        }
        public static void UpdateOperation(NOperationVM op)
        {
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var param = new Dictionary<string, string>();
            var sql = @"update n_Operation set ";
            if (!string.IsNullOrEmpty(op.Name))
            {
                sql += "Name = @Name, ";
                param.Add("@Name", op.Name);
            }
            if (!string.IsNullOrEmpty(op.Status))
            {
                sql += "Status = @Status, ";
                param.Add("@Status", op.Status);
            }
            sql += "UpdateAt = @UpdateAt where ID = @ID";
            param.Add("@UpdateAt", now);
            param.Add("@ID", op.ID.ToString());

            DBUtility.ExeLocalSqlNoRes(sql, param);

        }

    }

    public class NRolePermissionVM
    {
        public int ID { set; get; }
        public int RoleID { set; get; }
        public int MenuID { set; get; }
        public int FunctionID { set; get; }
        public int OperationID { set; get; }
        public string CreateAt { set; get; }
        public string UpdateAt { set; get; }
        public string RoleName { set; get; }
        public string MenuName { set; get; }
        public string FunctionName { set; get; }
        public string OperationName { set; get; }
        public static Dictionary<string, List<NRolePermissionVM>> GetRolePermission(string rId = "")
        {
            var sql = @"select nrp.ID, nrp.RoleID, nrp.MenuID, nrp.FunctionID,
                nrp.OperationID, nrp.CreateAt, nrp.UpdateAt, nr.Name as RoleName,
                nm.Name as MenuName, nf.Name as FunctionName, no.Name as OperationName 
                from n_RolePermission as nrp 
                left join n_Role as nr on (nr.Status = 1 and nrp.RoleID = nr.ID)
                left join n_Menu as nm on (nm.Status = 1 and nm.ID = nrp.MenuID)
                left join n_Function as nf on (nf.Status = 1 and nrp.FunctionID = nf.ID)
                left join n_Operation as no on (no.Status = 1 and nrp.OperationID = no.ID)
                where 1 = 1";
            var param = new Dictionary<string, string>();
            if(!string.IsNullOrEmpty(rId))
            {
                sql += " and nrp.RoleID = @rId ";
                param.Add("@rId", rId);
            }
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, param);
            var res = new Dictionary<string, List<NRolePermissionVM>>();
            if(dbret.Count > 0)
            {
                foreach (var item in dbret)
                {
                    var tmp = new NRolePermissionVM();
                    tmp.ID = Convert.ToInt32(item[0]);
                    tmp.RoleID = Convert.ToInt32(item[1]);
                    tmp.MenuID = Convert.ToInt32(item[2]);
                    tmp.FunctionID = Convert.ToInt32(item[3]);
                    tmp.OperationID = Convert.ToInt32(item[4]);
                    tmp.CreateAt = Convert.ToString(item[5]);
                    tmp.UpdateAt = Convert.ToString(item[6]);
                    tmp.RoleName = Convert.ToString(item[7]);
                    tmp.MenuName = Convert.ToString(item[8]);
                    tmp.FunctionName = Convert.ToString(item[9]);
                    tmp.OperationName = Convert.ToString(item[10]);

                    if (res.ContainsKey(Convert.ToString(item[1])))
                    {
                        res[Convert.ToString(item[1])].Add(tmp);
                    }
                    else
                    { 
                        var list_tmp = new List<NRolePermissionVM>();
                        list_tmp.Add(tmp);
                        res.Add(Convert.ToInt32(item[1]).ToString(), list_tmp);
                    }

                }
            }
            return res;
        }
        public static void AddRolePermission(List<NRolePermissionVM> rolepermissions)
        {
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var sql = @"insert into n_RolePermission (RoleID, MenuID, FunctionID, OperationID,
                        CreateAt, UpdateAt) values ";
            var param = new Dictionary<string, string>();
            var idx = 0;
            foreach(var item in rolepermissions)
            {
                sql += "(@RoleID_"+idx+ ", @MenuID_" + idx + ", @FunctionID_" + idx + ", @OperationID_" + idx + ", @CreateAt_" + idx + ", @UpdateAt_" + idx + "),";
                param.Add("@RoleID_" + idx, item.RoleID.ToString());
                param.Add("@MenuID_" + idx, item.MenuID.ToString());
                param.Add("@FunctionID_" + idx, item.FunctionID.ToString());
                param.Add("@OperationID_" + idx, item.OperationID.ToString());
                param.Add("@CreateAt_" + idx, now);
                param.Add("@UpdateAt_" + idx, now);
                idx++;
            }
            sql = sql.Substring(0, sql.Length - 1);
            DBUtility.ExeLocalSqlNoRes(sql, param);
        }
        public static void DeleteRolePermissionByRoleID(int rId)
        {
            var sql = @"delete from n_RolePermission where RoleID = @rId";
            var param = new Dictionary<string, string>();
            param.Add("@rId", rId.ToString());
            DBUtility.ExeLocalSqlNoRes(sql, param);
        }
        public static void DeleteRolePermission(string ids)
        {
            var sql = @"delete from n_RolePermission where ID in (<ids>)";
            sql.Replace("<ids>", ids);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

    }

    public class NUserPermissionRequestVM
    {
        public int ID { set; get; }
        public int UserID { set; get; }
        public int MenuID { set; get; }
        public int FunctionID { set; get; }
        public int OperationID { set; get; }
        public string Comment { set; get; }
        public int Status { set; get; }
        public int Approver { set; get; }
        public int Operator { set; get; }
        public string CreateAt { set; get; }
        public string UpdateAt { set; get; }
        public string Email { set; get; }
        public string UserName { set; get; }
        public string MenuName { set; get; }
        public string FunctionName { set; get; }
        public string OperationName { set; get; }

        public static List<NUserPermissionRequestVM> GetUserPermissionRequest(int uId = 0, int Status = 0, int id = 0, int mId = 0, int fId = 0, int oId = 0, string keywords = "")
        {
            var sql = @"select upr.ID, upr.UserID, upr.MenuID, upr.FunctionID, upr.OperationID, 
                    upr.Status, upr.Comment, upr.CreateAt, upr.UpdateAt, nu.Name as UserName,
                    nm.Name as MenuName, nf.Name as FunctionName, no.Name as OperationName, nu.Email
                    from n_UserPermissionRequest as upr 
                    left join n_User as nu on nu.ID = upr.UserID
                    left join n_Menu as nm on nm.ID = upr.MenuID
                    left join n_Function as nf on nf.ID = upr.FunctionID
                    left join n_Operation as no on no.ID = upr.OperationID
                    where 1 = 1 ";
            var param = new Dictionary<string, string>();
            if(uId != 0)
            {
                sql += " and upr.UserId = @uId ";
                param.Add("@uId", uId.ToString());
            }
            if(Status != 0)
            {
                sql += " and upr.Status = @Status ";
                param.Add("@Status", Status.ToString());
            }
            if(id != 0)
            {
                sql += " and upr.ID = @Id";
                param.Add("@Id", id.ToString());
            }
            if (mId != 0)
            {
                sql += " and upr.MenuID = @mId";
                param.Add("@mId", mId.ToString());
            }
            if (fId != 0)
            {
                sql += " and upr.FunctionID = @fId";
                param.Add("@fId", fId.ToString());
            }
            if (oId != 0)
            {
                sql += " and upr.OperationID = @oId";
                param.Add("@oId", oId.ToString());
            }
            if (!string.IsNullOrEmpty(keywords))
            {
                if(string.Compare(keywords, PermissionRequestStatus.Msg[1], true) == 0)
                {
                    sql += " and upr.Status = 1 ";
                }
                else if(string.Compare(keywords, PermissionRequestStatus.Msg[2], true) == 0)
                {
                    sql += " and upr.Status = 2 ";
                }
                else if (string.Compare(keywords, PermissionRequestStatus.Msg[3], true) == 0)
                {
                    sql += " and upr.Status = 3 ";
                }
                else if (string.Compare(keywords, PermissionRequestStatus.Msg[4], true) == 0)
                {
                    sql += " and upr.Status = 4 ";
                }
                else if (string.Compare(keywords, PermissionRequestStatus.Msg[5], true) == 0)
                {
                    sql += " and upr.Status = 5 ";
                }
                else
                {
                    sql += @" and (nu.Name like @Keywords or nm.Name like @Keywords or nf.Name like @Keywords ) ";
                    param.Add("@Keywords", "%" + keywords + "%");
                }
            }
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, param);
            var res = new List<NUserPermissionRequestVM>();
            if(dbret.Count > 0)
            {
                foreach(var item in dbret)
                {
                    var tmp = new NUserPermissionRequestVM();
                    tmp.ID = Convert.ToInt32(item[0]);
                    tmp.UserID = Convert.ToInt32(item[1]);
                    tmp.MenuID = Convert.ToInt32(item[2]);
                    tmp.FunctionID = Convert.ToInt32(item[3]);
                    tmp.OperationID = Convert.ToInt32(item[4]);
                    tmp.Status = Convert.ToInt32(item[5]);
                    tmp.Comment = Convert.ToString(item[6]);
                    tmp.CreateAt = Convert.ToString(item[7]);
                    tmp.UpdateAt = Convert.ToString(item[8]);
                    tmp.UserName = Convert.ToString(item[9]);
                    tmp.MenuName = Convert.ToString(item[10]);
                    tmp.FunctionName = Convert.ToString(item[11]);
                    tmp.OperationName = Convert.ToString(item[12]);
                    tmp.Email = Convert.ToString(item[13]);
                    res.Add(tmp);
                }
            }
            return res;
        }

        public static void UpdateUserPermissionRequestStatus(int Id, int Status, int uId)
        {
            var sql = @"update n_UserPermissionRequest set Status = @Status,
                        ";
            var param = new Dictionary<string, string>();
            param.Add("@Status", Status.ToString());
            if(Status == PermissionRequestStatus.Approved 
                || Status == PermissionRequestStatus.Deny)
            {
                sql += " Approver = @Approver, ";
                param.Add("@Approver", uId.ToString());
            }
            else if (Status == PermissionRequestStatus.Complete)
            {
                sql += " Operator = @Operator, ";
                param.Add("@Operator", uId.ToString());
            }
            sql += "UpdateAt = @uTime where ID = @Id ";
            param.Add("@uTime", DateTime.Now.ToString("yyyy-MM-dd"));
            param.Add("@Id", Id.ToString());


            DBUtility.ExeLocalSqlNoRes(sql, param);
        }

        public static void AddUserPermissionRequest(List<NUserPermissionRequestVM> upr_list)
        {
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var sql = @"insert into n_UserPermissionRequest(UserID, MenuID, 
                FunctionID, OperationID, Status, Comment, Approver, 
                Operator, CreateAt, UpdateAt) values ";
            var param = new Dictionary<string, string>();
            var idx = 0;
            foreach (var item in upr_list)
            {
                var exist = GetUserPermissionRequest(item.UserID, 0, 0, item.MenuID, item.FunctionID, item.OperationID);
                if(exist.Count > 0)
                {
                    continue;
                }
                var hasPermit = UserGroupRolePermissionVM.GetUserGroupRolePermission(item.UserID, item.MenuID, item.FunctionID);
                if(hasPermit.Count > 0)
                {
                    continue;
                }
                sql += "(@UserID_" + idx + ", @MenuID_" + idx + ", @FunctionID_" 
                    + idx + ", @OperationID_" + idx + ",  @Status_" + idx + ",  @Comment_" 
                    + idx + ", @Approver_" + idx + ",  @Operator_" + idx + ",  @CreateAt_" 
                    + idx + ", @UpdateAt_" + idx + "),";
                param.Add("@UserID_" + idx, item.UserID.ToString());
                param.Add("@MenuID_" + idx, item.MenuID.ToString());
                param.Add("@FunctionID_" + idx, item.FunctionID.ToString());
                param.Add("@OperationID_" + idx, item.OperationID.ToString());
                param.Add("@Status_" + idx, PermissionRequestStatus.Pending.ToString());
                param.Add("@Comment_" + idx, item.Comment);
                param.Add("@Approver_" + idx, "0");
                param.Add("@Operator_" + idx, "0");
                param.Add("@CreateAt_" + idx, now);
                param.Add("@UpdateAt_" + idx, now);
                idx++;
            }
            sql = sql.Substring(0, sql.Length - 1);
            DBUtility.ExeLocalSqlNoRes(sql, param);

        }

        
    }

}