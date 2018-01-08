using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Prometheus.Models;

namespace Prometheus.Controllers
{
    public class DashBoardController : Controller
    {
        // GET: DashBoard
        public ActionResult SystemBoard()
        {
            var systemeventlist = ProjectEvent.RetrieveEvent(30);

            ViewBag.assignedtasklist = new List<IssueViewModels>();

            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {
                var usernm = ckdict["logonuser"].Split(new char[] { '|' })[0];
                ViewBag.assignedtasklist = IssueViewModels.RetrieveIssueByAssignee(usernm, Resolute.Pending, 60);
            }
            
            return View(systemeventlist);
        }
        
        public ActionResult DebugTree()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {
            }
            else
            {
                return RedirectToAction("LoginUser", "User");
            }
            ViewBag.isMe = false;
            var usernm = ckdict["logonuser"].Split(new char[] { '|' })[0];
            var syscfg = CfgUtility.GetSysConfig(this);
            var melist = syscfg["ME"].ToUpper().Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            var uname = usernm.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries)[0].ToUpper().ToString();
            if (melist.Contains(uname))
            {
                ViewBag.isMe = true;
            }
            ViewBag.List = ProjectErrorViewModels.GetAllOrignalCode();

            ViewBag.isie8 = false;
            var browse = Request.Browser;
            if (string.Compare(browse.Browser, "IE", true) == 0
                && (string.Compare(browse.Version, "7.0", true) == 0
                || string.Compare(browse.Version, "8.0", true) == 0))
            {
                ViewBag.isie8 = true;
            }

            return View();
        }
    }
}