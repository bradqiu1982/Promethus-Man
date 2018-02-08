using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Prometheus.Models;

namespace Prometheus.Controllers
{
    public class PermissionController : Controller
    {
        // GET: Permission
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GroupList(int pageno = 1, string keywords = "")
        {
            var pagesize = 2;
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
            var pagesize = 2;
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
                a_rIds.Add(Convert.ToInt32(role.ID));
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
            var pagesize = 2;
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
            var pid = string.IsNullOrEmpty(Request.Form["pid"]) ? 0 : Convert.ToInt32(Request.Form["pid"]);
            var oid = string.IsNullOrEmpty(Request.Form["oid"]) ? 0 : Convert.ToInt32(Request.Form["oid"]);
            var menu = new NMenuVM();
            menu.Name = name;
            menu.Url = url;
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
            var pid = string.IsNullOrEmpty(Request.Form["pid"]) ? 0 : Convert.ToInt32(Request.Form["pid"]);
            var oid = string.IsNullOrEmpty(Request.Form["oid"]) ? 0 : Convert.ToInt32(Request.Form["oid"]);
            var menu = new NMenuVM();
            menu.ID = mid;
            menu.Name = name;
            menu.Url = url;
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
            var pagesize = 2;
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

            var Menus = NMenuVM.GetMenuList();
            var res = new JsonResult();
            res.Data = new { success = true, Menus = Menus };
            return res;
        }

        [HttpPost]
        public JsonResult AddFunction()
        {
            var name = Request.Form["name"];
            var url = Request.Form["url"];
            var mid = Convert.ToInt32(Request.Form["mid"]);
            var func = new NFunctionVM();
            func.Name = name;
            func.Url = url;
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
            var mid = Convert.ToInt32(Request.Form["mid"]);
            var func = new NFunctionVM();
            func.ID = fid;
            func.Name = name;
            func.Url = url;
            func.MenuID = mid;
            func.UpdateAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            NFunctionVM.UpdateFunction(func);
            var res = new JsonResult();
            res.Data = new { success = true };
            return res;
        }

        public ActionResult Operations(int pageno = 1, string keywords = "")
        {
            var pagesize = 2;
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
            return View();
        }
    }
}