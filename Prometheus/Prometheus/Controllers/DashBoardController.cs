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
            return View(systemeventlist);
        }

    }
}