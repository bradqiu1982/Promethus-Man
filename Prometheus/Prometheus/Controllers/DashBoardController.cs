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
        public ActionResult ViewAll()
        {
            DashBoardViewModels dv = new DashBoardViewModels();

            List<HtmlRaw> activeInfo = new List<HtmlRaw>();
            var info = new HtmlRaw();
            info.MesgContent = "<a href=\"brad.qiu@finisar.com\">Brad</a> modified file: <a href=\"~/userfiles/docs/file\">CFP4-SR4 DESIGN</a>";
            activeInfo.Add(info);

            info = new HtmlRaw();
            info.MesgContent = "<a href=\"alex.chen@finisar.com\">Alex</a> modified file: <a href=\"~/userfiles/docs/file\">QSFP28G DESIGN</a>";
            activeInfo.Add(info);

            dv.ActiveInfoList = activeInfo;
            return View(dv);
        }

    }
}