using Prometheus.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Prometheus.Controllers
{
    public class BaseController: Controller
    {
        public string RedirectTo(HttpRequestBase request)
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {
                var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
                var exist_data = NUserVM.GetUserInfo(updater);
                if (exist_data.ID != 0)
                {
                    var func_info = NFunctionVM.GetFunctionInfoByUrl(request.RawUrl.Split(new string[] { "?" }, StringSplitOptions.RemoveEmptyEntries)[0]);
                    var rp_info = new List<UserGroupRolePermissionVM>();
                    if (func_info.ID != 0)
                    {
                        rp_info = UserGroupRolePermissionVM.GetUserGroupRolePermission(exist_data.ID, func_info.MenuID, func_info.ID);
                        if (rp_info.Count > 0)
                        {
                            return request.RawUrl;
                        }
                    }
                    return "/Permission/ApplyForPermission?url=" + request.RawUrl;
                }
            }

            return request.RawUrl;
        }
        
        public void UserPermit()
        {
            ViewBag.UserData = null;
            ViewBag.PermitData = null;
            ViewBag.Login = false;
            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {
                ViewBag.Login = true;
                var uName = ckdict["logonuser"].Split(new char[] { '|' })[0];
                var u_info = NUserVM.GetUserInfo(uName);
                if (u_info.ID != 0)
                {
                    ViewBag.UserData = u_info;
                    ViewBag.PermitData = UserGroupRolePermissionVM.GetUserGroupRolePermission(u_info.ID);
                    var allMenuList = NMenuVM.GetMenuListByFuncUrl(Request.RawUrl.Split(new string[] { "?" }, StringSplitOptions.RemoveEmptyEntries)[0]);
                    var parentMenuId = allMenuList[0].ParentID;
                    var MenuList = new Dictionary<int, UserGroupRolePermissionVM>();
                    foreach (var item in ViewBag.PermitData)
                    {
                        if (item.ParentID == parentMenuId)
                        {
                            if (!MenuList.ContainsKey(item.MenuID))
                            {
                                if (item.FunctionUrl == Request.RawUrl.Split(new string[] { "?" }, StringSplitOptions.RemoveEmptyEntries)[0])
                                {
                                    ViewBag.CurrentMenu = item.MenuID;
                                }
                                MenuList.Add(item.MenuID, item);
                            }
                        }
                    }
                    ViewBag.MenuList = MenuList.Values;
                }
            }
        }

        public ActionResult RedirectToLogin(string ctrl, string method)
        {
            var ck = new Dictionary<string, string>();
            ck.Add("logonredirectctrl", ctrl);
            ck.Add("logonredirectact", method);
            CookieUtility.SetCookie(this, ck);
            return RedirectToAction("LoginUser", "User");
        }

        public object CheckLoginAndPermit(HttpRequestBase request, string ctrl, string method)
        {
            UserPermit();
            
            if (!ViewBag.Login)
            {
                return RedirectToLogin(ctrl, method);
            }

            var redirecturl = RedirectTo(request);
            if (redirecturl != Request.RawUrl)
            {
                return Redirect(redirecturl);
            }

            return true;
        }
    }
}