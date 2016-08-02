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
            var systemeventlist = ProjectEvent.RetrieveSystemEvent(30);

            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {
                var usernm = ckdict["logonuser"].Split(new char[] { '|' })[0];
                var userpendinglist = ProjectEvent.RetrieveUserEvent(usernm, ProjectEvent.Pending, 100);
                var userworkinglist = ProjectEvent.RetrieveUserEvent(usernm, ProjectEvent.Pending, 100);
                systemeventlist.AddRange(userpendinglist);
                systemeventlist.AddRange(userworkinglist);
            }

            return View(systemeventlist);
        }

    }
}