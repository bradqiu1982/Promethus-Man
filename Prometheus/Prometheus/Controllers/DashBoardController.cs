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

    }
}