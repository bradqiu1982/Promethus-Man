using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Prometheus.Models;
using System.Web.Routing;
using System.Text;
using System.Net.Mail;
using System.IO;
using System.Threading;
using System.Web.Caching;
using System.Net;

namespace Prometheus.Controllers
{
    public class PermissionController : BaseController
    {
        public ActionResult GroupList(int pageno = 1, string keywords = "")
        {
            var checkresult = CheckLoginAndPermit(Request, "Permission", "GroupList");
            if (checkresult.GetType() != (new Boolean()).GetType())
            {
                return (ActionResult)checkresult;
            }

            var pagesize = PageVM.PageSize;
            var GroupList = NGroupVM.GetGroups(keywords);
            var totalcnt = GroupList.Count;
            var totalpage = (int)Math.Ceiling((double)totalcnt / pagesize);
            var pageNo = totalpage < pageno ? totalpage : pageno;
            ViewBag.GroupList = (totalcnt > 0) ? GroupList.Skip((pageNo - 1) * pagesize).Take(pagesize) : null;
            ViewBag.TotalCnt = totalcnt;
            ViewBag.TotalPage = totalpage;
            ViewBag.PageNo = pageNo;
            ViewBag.Keywords = keywords;

            return View();
        }

        [HttpPost]
        public JsonResult AddGroup()
        {
            var gname = Request.Form["gname"];
            var comment = Request.Form["comment"];
            var group = new NGroupVM();
            group.Name = gname;
            group.Comment = comment;
            group.Status = RBACStatus.ValidStatus.ToString();
            group.CreateAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            group.UpdateAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            NGroupVM.AddGroup(group);
            var res = new JsonResult();
            res.Data = new { success = true };
            return res;
        }

        [HttpPost]
        public JsonResult UpdateGroup()
        {
            var gid = Convert.ToInt32(Request.Form["gid"]);
            var gname = Request.Form["gname"];
            var comment = Request.Form["comment"];
            var group = new NGroupVM();
            group.ID = gid;
            group.Name = gname;
            group.Comment = comment;
            group.UpdateAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            NGroupVM.UpdateGroup(group);
            var res = new JsonResult();
            res.Data = new { success = true };
            return res;
        }

        [HttpPost]
        public JsonResult DelGroup()
        {
            var gid = Convert.ToInt32(Request.Form["gid"]);
            var group = new NGroupVM();
            group.ID = gid;
            group.Status = RBACStatus.InValidStatus.ToString();
            group.UpdateAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            NGroupVM.UpdateGroup(group);
            var res = new JsonResult();
            res.Data = new { success = true };
            return res;
        }

        public ActionResult MemberList(int pageno = 1, string keywords = "")
        {
            var checkresult = CheckLoginAndPermit(Request, "Permission", "MemberList");
            if (checkresult.GetType() != (new Boolean()).GetType())
            {
                return (ActionResult)checkresult;
            }
            var pagesize = PageVM.PageSize;
            var MemberList = NUserVM.GetUsersList(keywords);
            var totalcnt = MemberList.Count;
            var totalpage = (int)Math.Ceiling((double)totalcnt / pagesize);
            var pageNo = totalpage < pageno ? totalpage : pageno;
            ViewBag.MemberList = (totalcnt > 0) ? MemberList.Skip((pageNo - 1) * pagesize).Take(pagesize) : null;
            ViewBag.TotalCnt = totalcnt;
            ViewBag.TotalPage = totalpage;
            ViewBag.PageNo = pageNo;
            ViewBag.Keywords = keywords;
            return View();
        }

        [HttpPost]
        public JsonResult AddMember()
        {
            var jobnum = Request.Form["jobnum"];
            var mail = Request.Form["mail"];
            var name = Request.Form["name"];
            var tel = Request.Form["tel"];
            var user = new NUserVM();
            user.JobNum = jobnum;
            user.Email = mail.ToUpper();
            user.Name = name.ToUpper();
            user.Tel = tel;
            user.Password = "abc@123";
            user.Status = RBACStatus.ValidStatus.ToString();
            user.CreateAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            user.UpdateAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            NUserVM.AddUser(user);
            var res = new JsonResult();
            res.Data = new { success = true };
            return res;
        }

        [HttpPost]
        public JsonResult DelMember()
        {
            var uid = Convert.ToInt32(Request.Form["user_id"]);
            NUserVM.DeleteUser(uid);
            var res = new JsonResult();
            res.Data = new { success = true };
            return res;

        }

        [HttpPost]
        public JsonResult UpdateMember()
        {
            var uid = Convert.ToInt32(Request.Form["uid"]);
            var jobnum = Request.Form["jobnum"];
            var mail = Request.Form["mail"];
            var name = Request.Form["name"];
            var tel = Request.Form["tel"];
            var user = new NUserVM();
            user.ID = uid;
            user.JobNum = jobnum;
            user.Email = mail.ToUpper();
            user.Name = name.ToUpper();
            user.Tel = tel;
            user.UpdateAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            NUserVM.UpdateUser(user);
            var res = new JsonResult();
            res.Data = new { success = true };
            return res;

        }

        [HttpPost]
        public JsonResult GetMembers()
        {
            var gId = Convert.ToInt32(Request.Form["gId"]);
            var aMembers = NUserVM.GetUsersList();
            var gMembers = NUserGroupVM.GetUsersByGroupID(gId);
            var res = new JsonResult();
            res.Data = new { success = true, aMembers = aMembers, gMembers = gMembers };
            return res;
        }

        [HttpPost]
        public JsonResult EditGroupMember()
        {
            var gId = Convert.ToInt32(Request.Form["gId"]);
            var uIds = Request.Form["uIds[]"];
            var n_uIds = uIds.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(Int32.Parse).ToList();
            var gmembers = NUserGroupVM.GetUsersByGroupID(gId);
            var a_uIds = new List<int>();
            foreach (var member in gmembers)
            {
                a_uIds.Add(Convert.ToInt32(member.ID));
            }
            var add_uIds = n_uIds.Except(a_uIds).ToList();
            var del_uIds = a_uIds.Except(n_uIds).ToList();
            foreach (var uid in add_uIds)
            {
                NUserGroupVM.AddGroupMember(gId, Convert.ToInt32(uid));
            }
            foreach (var uid in del_uIds)
            {
                NUserGroupVM.DeleteGroupMember(gId, Convert.ToInt32(uid));
            }
            var res = new JsonResult();
            res.Data = new { success = true};
            return res;
        }

        [HttpPost]
        public JsonResult GetGroupRoles()
        {
            var gId = Convert.ToInt32(Request.Form["gId"]);
            var aRoles = NRoleVM.GetRoleList(string.Empty);
            var gRoles = NGroupRoleVM.GetGroupRoleByGroupID(gId);
            var res = new JsonResult();
            res.Data = new { success = true, aRoles = aRoles, gRoles = gRoles };
            return res;
        }

        [HttpPost]
        public JsonResult EditGroupRole()
        {
            var gId = Convert.ToInt32(Request.Form["gId"]);
            var rIds = Request.Form["rIds[]"];
            var n_rIds = rIds.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(Int32.Parse).ToList();
            var gRoles = NGroupRoleVM.GetGroupRoleByGroupID(gId);
            var a_rIds = new List<int>();
            foreach (var role in gRoles)
            {
                a_rIds.Add(Convert.ToInt32(role.RoleID));
            }
            var add_rIds = n_rIds.Except(a_rIds).ToList();
            var del_rIds = a_rIds.Except(n_rIds).ToList();
            foreach (var rid in add_rIds)
            {
                NGroupRoleVM.AddGroupRole(gId, Convert.ToInt32(rid), string.Empty);
            }
            foreach (var rid in del_rIds)
            {

                NGroupRoleVM.UpdateGroupRole(gId, Convert.ToInt32(rid), RBACStatus.InValidStatus);
            }
            var res = new JsonResult();
            res.Data = new { success = true };
            return res;
        }
        
        public ActionResult Menus(int pageno = 1, string keywords = "")
        {
            var checkresult = CheckLoginAndPermit(Request, "Permission", "Menus");
            if (checkresult.GetType() != (new Boolean()).GetType())
            {
                return (ActionResult)checkresult;
            }
            var pagesize = PageVM.PageSize;
            var Menus = NMenuVM.GetMenuList(keywords);
            var totalcnt = Menus.Count;
            var totalpage = (int)Math.Ceiling((double)totalcnt / pagesize);
            var pageNo = totalpage < pageno ? totalpage : pageno;
            ViewBag.Menus = (totalcnt > 0) ? Menus.Skip((pageNo - 1) * pagesize).Take(pagesize) : null;
            ViewBag.TotalCnt = totalcnt;
            ViewBag.TotalPage = totalpage;
            ViewBag.PageNo = pageNo;
            ViewBag.Keywords = keywords;

            return View();
        }

        [HttpPost]
        public JsonResult AddMenu()
        {
            var name = Request.Form["name"];
            var url = Request.Form["url"];
            var imgurl = Request.Form["imgurl"];
            var pid = string.IsNullOrEmpty(Request.Form["pid"]) ? 0 : Convert.ToInt32(Request.Form["pid"]);
            var oid = string.IsNullOrEmpty(Request.Form["oid"]) ? 0 : Convert.ToInt32(Request.Form["oid"]);
            var menu = new NMenuVM();
            menu.Name = name;
            menu.Url = url;
            menu.ImgUrl = imgurl;
            menu.ParentID = pid;
            menu.OrderID = oid;
            menu.Status = RBACStatus.ValidStatus.ToString();
            menu.CreateAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            menu.UpdateAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            NMenuVM.AddMenu(menu);
            var res = new JsonResult();
            res.Data = new { success = true };
            return res;
        }

        [HttpPost]
        public JsonResult UpdateMenu()
        {
            var mid = Convert.ToInt32(Request.Form["mid"]);
            var name = Request.Form["name"];
            var url = Request.Form["url"];
            var imgurl = Request.Form["imgurl"];
            var pid = string.IsNullOrEmpty(Request.Form["pid"]) ? 0 : Convert.ToInt32(Request.Form["pid"]);
            var oid = string.IsNullOrEmpty(Request.Form["oid"]) ? 0 : Convert.ToInt32(Request.Form["oid"]);
            var menu = new NMenuVM();
            menu.ID = mid;
            menu.Name = name;
            menu.Url = url;
            menu.ImgUrl = imgurl;
            menu.ParentID = pid;
            menu.OrderID = oid;
            menu.UpdateAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            NMenuVM.UpdateMenu(menu);
            var res = new JsonResult();
            res.Data = new { success = true };
            return res;
        }

        [HttpPost]
        public JsonResult DelMenu()
        {
            var mid = Convert.ToInt32(Request.Form["mid"]);
            var menu = new NMenuVM();
            menu.ID = mid;
            menu.Status = RBACStatus.InValidStatus.ToString();
            menu.UpdateAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            NMenuVM.UpdateMenu(menu);
            var res = new JsonResult();
            res.Data = new { success = true };
            return res;
        }

        public ActionResult Functions(int pageno = 1, string keywords = "")
        {
            var checkresult = CheckLoginAndPermit(Request, "Permission", "Functions");
            if (checkresult.GetType() != (new Boolean()).GetType())
            {
                return (ActionResult)checkresult;
            }
            var pagesize = PageVM.PageSize;
            var Funcs = NFunctionVM.GetFunctionList(keywords);
            var totalcnt = Funcs.Count;
            var totalpage = (int)Math.Ceiling((double)totalcnt / pagesize);
            var pageNo = totalpage < pageno ? totalpage : pageno;
            ViewBag.Functions = (totalcnt > 0) ? Funcs.Skip((pageNo - 1) * pagesize).Take(pagesize) : null;
            ViewBag.TotalCnt = totalcnt;
            ViewBag.TotalPage = totalpage;
            ViewBag.PageNo = pageNo;
            ViewBag.Keywords = keywords;

            return View();
        }

        [HttpPost]
        public JsonResult GetMenuList()
        {
            var type = (Request.Form["type"] == null) ? 0 : Convert.ToInt32(Request.Form["type"]);
            var Menus = NMenuVM.GetMenuList(string.Empty, 0, type);
            var res = new JsonResult();
            res.Data = new { success = true, Menus = Menus };
            return res;
        }

        [HttpPost]
        public JsonResult AddFunction()
        {
            var name = Request.Form["name"];
            var url = Request.Form["url"];
            var imgurl = Request.Form["imgurl"];
            var mid = Convert.ToInt32(Request.Form["mid"]);
            var func = new NFunctionVM();
            func.Name = name;
            func.Url = url;
            func.ImgUrl = imgurl;
            func.MenuID = mid;
            func.Status = RBACStatus.ValidStatus.ToString();
            func.CreateAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            func.UpdateAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            NFunctionVM.AddFunction(func);
            var res = new JsonResult();
            res.Data = new { success = true };
            return res;
        }

        [HttpPost]
        public JsonResult DelFunction()
        {
            var fid = Convert.ToInt32(Request.Form["fid"]);
            var func = new NFunctionVM();
            func.ID = fid;
            func.Status = RBACStatus.InValidStatus.ToString();
            func.UpdateAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            NFunctionVM.UpdateFunction(func);
            var res = new JsonResult();
            res.Data = new { success = true };
            return res;
        }

        [HttpPost]
        public JsonResult UpdateFunction()
        {
            var fid = Convert.ToInt32(Request.Form["fid"]);
            var name = Request.Form["name"];
            var url = Request.Form["url"];
            var imgurl = Request.Form["imgurl"];
            var mid = Convert.ToInt32(Request.Form["mid"]);
            var func = new NFunctionVM();
            func.ID = fid;
            func.Name = name;
            func.Url = url;
            func.ImgUrl = imgurl;
            func.MenuID = mid;
            func.UpdateAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            NFunctionVM.UpdateFunction(func);
            var res = new JsonResult();
            res.Data = new { success = true };
            return res;
        }

        public ActionResult Operations(int pageno = 1, string keywords = "")
        {
            var checkresult = CheckLoginAndPermit(Request, "Permission", "Operations");
            if (checkresult.GetType() != (new Boolean()).GetType())
            {
                return (ActionResult)checkresult;
            }

            var pagesize = PageVM.PageSize;
            var Operations = NOperationVM.GetOperationList(keywords);
            var totalcnt = Operations.Count;
            var totalpage = (int)Math.Ceiling((double)totalcnt / pagesize);
            var pageNo = totalpage < pageno ? totalpage : pageno;
            ViewBag.Operations = (totalcnt > 0) ? Operations.Skip((pageNo - 1) * pagesize).Take(pagesize) : null;
            ViewBag.TotalCnt = totalcnt;
            ViewBag.TotalPage = totalpage;
            ViewBag.PageNo = pageNo;
            ViewBag.Keywords = keywords;

            return View();
        }

        [HttpPost]
        public JsonResult AddOperation()
        {
            var name = Request.Form["name"];
            var op = new NOperationVM();
            op.Name = name;
            op.Status = RBACStatus.ValidStatus.ToString();
            op.CreateAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            op.UpdateAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            NOperationVM.AddOperation(op);
            var res = new JsonResult();
            res.Data = new { success = true };
            return res;
        }

        [HttpPost]
        public JsonResult UpdateOperation()
        {
            var oid = Convert.ToInt32(Request.Form["oid"]);
            var name = Request.Form["name"];
            var op = new NOperationVM();
            op.ID = oid;
            op.Name = name;
            op.UpdateAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            NOperationVM.UpdateOperation(op);
            var res = new JsonResult();
            res.Data = new { success = true };
            return res;
        }

        [HttpPost]
        public JsonResult DelOperation()
        {
            var oid = Convert.ToInt32(Request.Form["oid"]);
            var op = new NOperationVM();
            op.ID = oid;
            op.Status = RBACStatus.InValidStatus.ToString();
            op.UpdateAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            NOperationVM.UpdateOperation(op);
            var res = new JsonResult();
            res.Data = new { success = true };
            return res;

        }

        public ActionResult RoleList(int pageno = 1, string keywords = "")
        {
            var checkresult = CheckLoginAndPermit(Request, "Permission", "RoleList");
            if (checkresult.GetType() != (new Boolean()).GetType())
            {
                return (ActionResult)checkresult;
            }

            var pagesize = PageVM.PageSize;
            var rolelist = NRoleVM.GetRoleList(RBACStatus.ValidStatus.ToString(), string.Empty, keywords);
            var totalcnt = rolelist.Count;
            var totalpage = (int)Math.Ceiling((double)totalcnt / pagesize);
            var pageNo = totalpage < pageno ? totalpage : pageno;

            ViewBag.Roles = (totalcnt > 0) ? rolelist.Skip((pageNo - 1) * pagesize).Take(pagesize) : null;
            ViewBag.TotalCnt = totalcnt;
            ViewBag.TotalPage = totalpage;
            ViewBag.PageNo = pageNo;
            ViewBag.Keywords = keywords;

            return View();
        }

        [HttpPost]
        public JsonResult GetRolePermission()
        {
            var rid = Request.Form["rid"];
            var rpList = new List<NRolePermissionVM>();
            var mList = new List<NMenuVM>();
            var oList = new List<NOperationVM>();
            if (string.IsNullOrEmpty(rid))
            {
                mList = NMenuVM.GetMenuList(string.Empty, 0, 1);
                oList = NOperationVM.GetOperationList();
            }
            else
            {
                var rpList_tmp = NRolePermissionVM.GetRolePermission(rid);
                rpList = rpList_tmp[rid];
            }

            var res = new JsonResult();
            res.Data = new {
                success = true,
                rPermissions = rpList,
                mList = mList,
                oList = oList,
            };
            return res;
        }

        [HttpPost]
        public JsonResult GetMenuFunctions()
        {
            var mid = Request.Form["mid"];
            var mFunctions = NFunctionVM.GetFunctionList("", mid);

            var res = new JsonResult();
            res.Data = new {
                success = true,
                mFunctions = mFunctions,
            };
            return res;
        }

        [HttpPost]
        public JsonResult AddRole()
        {
            var rName = Request.Form["rName"];
            var mIds = Request.Form["mIds[]"];
            var comment = Request.Form["comment"];
            var res = new JsonResult();
            var exist_data = NRoleVM.GetRoleList("", rName);
            if(exist_data.Count > 0)
            {
                res.Data = new
                {
                    success = false,
                    msg = "This RoleName has already exist",
                };
                return res;
            }

            var role = new NRoleVM();
            role.Name = rName;
            role.Comment = comment;
            role.Status = RBACStatus.ValidStatus.ToString();
            NRoleVM.AddRole(role);

            exist_data = NRoleVM.GetRoleList("", rName);
            var rId = exist_data[0].ID;
            var mids_arr = mIds.Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
            var rp_list = new List<NRolePermissionVM>();
            foreach (var item in mids_arr)
            {
                var ids = item.Split(new string[] { "_"}, StringSplitOptions.RemoveEmptyEntries);
                var rpermission = new NRolePermissionVM();
                rpermission.RoleID = rId;
                rpermission.MenuID = Convert.ToInt32(ids[0]);
                rpermission.FunctionID = Convert.ToInt32(ids[1]);
                rpermission.OperationID = Convert.ToInt32(ids[2]);
                rp_list.Add(rpermission);
            }
            NRolePermissionVM.AddRolePermission(rp_list);
            res.Data = new
            {
                success = true
            };
            return res;
        }

        [HttpPost]
        public JsonResult EditRole()
        {
            var rId = Request.Form["rId"];
            var rName = Request.Form["rName"];
            var mIds = Request.Form["mIds[]"];
            var comment = Request.Form["comment"];

            var role = new NRoleVM();
            role.ID = Convert.ToInt32(rId);
            role.Name = rName;
            role.Comment = comment;
            NRoleVM.UpdateRole(role);

            NRolePermissionVM.DeleteRolePermissionByRoleID(role.ID);
            var mids_arr = mIds.Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
            var rp_list = new List<NRolePermissionVM>();
            foreach (var item in mids_arr)
            {
                var ids = item.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
                var rpermission = new NRolePermissionVM();
                rpermission.RoleID = role.ID;
                rpermission.MenuID = Convert.ToInt32(ids[0]);
                rpermission.FunctionID = Convert.ToInt32(ids[1]);
                rpermission.OperationID = Convert.ToInt32(ids[2]);
                rp_list.Add(rpermission);
            }
            NRolePermissionVM.AddRolePermission(rp_list);
            var res = new JsonResult();
            res.Data = new
            {
                success = true,
            };

            return res;
        }

        [HttpPost]
        public JsonResult DelRole()
        {
            var rid = Request.Form["rId"];
            var res = new JsonResult();
            if (!string.IsNullOrEmpty(rid))
            {
                var role = new NRoleVM();
                role.ID = Convert.ToInt32(rid);
                role.Status = RBACStatus.InValidStatus.ToString();
                NRoleVM.UpdateRole(role);
                res.Data = new { success = true };
            }
            else
            {
                res.Data = new { success = false };
            }
            return res;
        }

        public ActionResult ApplyForPermission(string url)
        {
            var checkresult = CheckLoginAndPermit(Request, "Permission", "ApplyForPermission");
            if (checkresult.GetType() != (new Boolean()).GetType())
            {
                return (ActionResult)checkresult;
            }

            if (! string.IsNullOrEmpty(url))
            {
                var uri = url.Split(new string[] { "?" }, StringSplitOptions.RemoveEmptyEntries)[0];
                var cur_uri = Request.RawUrl.Split(new string[] { "?" }, StringSplitOptions.RemoveEmptyEntries)[0];
                ViewBag.func = (uri == cur_uri) ? null : NFunctionVM.GetFunctionInfoByUrl(uri);
            }

            return View();
        }

        public ActionResult PermissionRequest(int pageno = 1, string keywords = "")
        {
            var checkresult = CheckLoginAndPermit(Request, "Permission", "PermissionRequest");
            if (checkresult.GetType() != (new Boolean()).GetType())
            {
                return (ActionResult)checkresult;
            }

            var pagesize = PageVM.PageSize;

            ViewBag.Approve_Flg = HasPermission(ViewBag.UserData.ID, PermissionOperateUrl.Approve);
            ViewBag.Deny_Flg = HasPermission(ViewBag.UserData.ID, PermissionOperateUrl.Deny);
            ViewBag.Complete_Flg = HasPermission(ViewBag.UserData.ID, PermissionOperateUrl.Complete);

            var search_uid = ViewBag.UserData.ID;
            if(ViewBag.Approve_Flg || ViewBag.Deny_Flg || ViewBag.Complete_Flg)
            {
                search_uid = 0;
            }
            var prList = (List<NUserPermissionRequestVM>)NUserPermissionRequestVM.GetUserPermissionRequest(search_uid, 0, 0, 0, 0, 0, keywords);
            var totalcnt = prList.Count;
            var totalpage = (int)Math.Ceiling((double)totalcnt / pagesize);
            var pageNo = totalpage < pageno ? totalpage : pageno;


            ViewBag.Data = (totalcnt > 0) ? prList.Skip((pageNo - 1) * pagesize).Take(pagesize) : null;
            ViewBag.TotalCnt = totalcnt;
            ViewBag.TotalPage = totalpage;
            ViewBag.PageNo = pageNo;
            ViewBag.Keywords = keywords;

            return View();
        }

        private bool HasPermission(int uId, string url)
        {
            var func_info = NFunctionVM.GetFunctionInfoByUrl(url.Split(new string[] { "?" }, StringSplitOptions.RemoveEmptyEntries)[0]);
            var rp_info = new List<UserGroupRolePermissionVM>();
            if (func_info.ID != 0)
            {
                rp_info = UserGroupRolePermissionVM.GetUserGroupRolePermission(uId, func_info.MenuID, func_info.ID);
                if (rp_info.Count > 0)
                {
                    return true;
                }
            }
            return false;
        }

        [HttpPost]
        public JsonResult AddUserPermissionRequest()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var uName = ckdict["logonuser"].Split(new char[] { '|' })[0];
            var exist_data = NUserVM.GetUserInfo(uName);
            var res = new JsonResult();
            var success_flg = false;
            if (exist_data.ID != 0)
            {
                var Ids = Request.Form["Ids[]"];
                var comment = Request.Form["comment"];

                var ids_arr = Ids.Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
                var upr_list = new List<NUserPermissionRequestVM>();
                foreach (var item in ids_arr)
                {
                    var ids = item.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
                    var upr = new NUserPermissionRequestVM();
                    upr.UserID = exist_data.ID;
                    upr.MenuID = Convert.ToInt32(ids[0]);
                    upr.FunctionID = Convert.ToInt32(ids[1]);
                    upr.OperationID = Convert.ToInt32(ids[2]);
                    upr.Comment = comment;
                    upr_list.Add(upr);
                }
                NUserPermissionRequestVM.AddUserPermissionRequest(upr_list);
                success_flg = true;
            }

            res.Data = new { success = success_flg };
            return res;
        }

        [HttpPost]
        public JsonResult ApprovePermissionRequest()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var uName = ckdict["logonuser"].Split(new char[] { '|' })[0];
            var exist_data = NUserVM.GetUserInfo(uName);
            var res = new JsonResult();
            var success_flg = false;
            if (exist_data.ID != 0)
            {
                var id = Convert.ToInt32(Request.Form["id"]);
                var permit_data = NUserPermissionRequestVM.GetUserPermissionRequest(0, 0, id);
                if (permit_data.Count > 0)
                {
                    NUserPermissionRequestVM.UpdateUserPermissionRequestStatus(id, PermissionRequestStatus.Approved, exist_data.ID);
                    //search operate user
                    var func_info = NFunctionVM.GetFunctionInfoByUrl(PermissionOperateUrl.Complete);
                    var permit_list = UserGroupRolePermissionVM.GetUserGroupRolePermission(0, func_info.MenuID, func_info.ID);
                    var operator_list = new List<string>();
                    if (permit_list.Count > 0)
                    {
                        foreach(var item in permit_list)
                        {
                            if (!operator_list.Contains(item.Email))
                            {
                                operator_list.Add(item.Email);
                            }
                        }
                    }
                    //send alert email
                    SendAlertEmail("Approved", uName, permit_data[0].Email, operator_list);
                    success_flg = true;
                }
            }
            res.Data = new { success = success_flg };
            return res;
        }

        [HttpPost]
        public JsonResult DenyPermissionRequest()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var uName = ckdict["logonuser"].Split(new char[] { '|' })[0];
            var exist_data = NUserVM.GetUserInfo(uName);
            var res = new JsonResult();
            var success_flg = false;
            if (exist_data.ID != 0)
            {
                var id = Convert.ToInt32(Request.Form["id"]);
                var permit_data = NUserPermissionRequestVM.GetUserPermissionRequest(0, 0, id);
                if(permit_data.Count > 0)
                {
                    NUserPermissionRequestVM.UpdateUserPermissionRequestStatus(id, PermissionRequestStatus.Deny, exist_data.ID);
                    //send alert email
                    SendAlertEmail("Denied", uName, permit_data[0].Email);
                    success_flg = true;
                }
            }
            res.Data = new { success = success_flg };
            return res;
        }

        [HttpPost]
        public JsonResult CompletePermissionRequest()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var uName = ckdict["logonuser"].Split(new char[] { '|' })[0];
            var exist_data = NUserVM.GetUserInfo(uName);
            var res = new JsonResult();
            var success_flg = false;
            if (exist_data.ID != 0)
            {
                var id = Convert.ToInt32(Request.Form["id"]);
                var permit_data = NUserPermissionRequestVM.GetUserPermissionRequest(0, 0, id);
                if(permit_data.Count > 0)
                {
                    var permit_exist = UserGroupRolePermissionVM.GetUserGroupRolePermission(permit_data[0].UserID, permit_data[0].MenuID, permit_data[0].FunctionID);
                    if (permit_exist.Count > 0)
                    {
                        NUserPermissionRequestVM.UpdateUserPermissionRequestStatus(id, PermissionRequestStatus.Complete, exist_data.ID);
                        //send alert email
                        SendAlertEmail("Completed", uName, permit_data[0].Email);
                        success_flg = true;
                    }
                }
            }
            res.Data = new { success = success_flg };

            return res;
        }

        private void SendAlertEmail(string type, string updater, string requester, List<string> n_operator=null)
        {
            string scheme = Url.RequestContext.HttpContext.Request.Url.Scheme;
            string validatestr = Url.Action("ApplyForPermission", "Permission", null, scheme);

            var netcomputername = EmailUtility.RetrieveCurrentMachineName();

            validatestr = validatestr.Replace("//localhost", "//" + netcomputername);

            var content = "This request has been " + type + " by " + updater + " :\r\n " + validatestr;

            var toaddrs = new List<string>();
            toaddrs.Add(updater);
            toaddrs.Add(requester);
            if(n_operator != null && n_operator.Count > 0)
            {
                toaddrs.AddRange(n_operator);
            }

            toaddrs = new List<string> { "yan.shi@finisar.com" };

            EmailUtility.SendEmail(this, "【" + type + "】Permission Request", toaddrs, content);
            new System.Threading.ManualResetEvent(false).WaitOne(200);
        }
    }
}