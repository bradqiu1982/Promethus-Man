using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Prometheus.Models;

namespace Prometheus.Controllers
{
    public class PJReportController : Controller
    {

        private List<SelectListItem> CreateSelectList(List<string> valist, string defVal)
        {
            var pslist = new List<SelectListItem>();
            foreach (var p in valist)
            {
                var pitem = new SelectListItem();
                pitem.Text = p;
                pitem.Value = p;
                if (!string.IsNullOrEmpty(defVal) && string.Compare(defVal, p) == 0)
                {
                    pitem.Selected = true;
                }
                pslist.Add(pitem);
            }

            if (string.IsNullOrEmpty(defVal) && pslist.Count > 0)
            {
                pslist[0].Selected = true;
            }

            return pslist;
        }

        // GET: PJReport
        public ActionResult IBook()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {

            }
            else
            {
                var ck = new Dictionary<string, string>();
                ck.Add("logonredirectctrl", "PJReport");
                ck.Add("logonredirectact", "IBook");
                CookieUtility.SetCookie(this, ck);
                return RedirectToAction("LoginUser", "User");
            }

            ViewBag.PJList = ProjectViewModels.RetrieveAllProjectKey();

            var temptypelist = new List<string>();
            temptypelist.Add(PJReportType.YieldTrend);
            temptypelist.Add(PJReportType.IssueTrend);
            temptypelist.Add(PJReportType.FailureTrend);
            temptypelist.Add(PJReportType.MonthlyPareto);
            ViewBag.RepTypeList = temptypelist;

            var username = ckdict["logonuser"].Split(new char[] { '|' })[0];
            var vm = PJReportViewModels.RetrieveBookReportRecord(username);

            var rsilist = new List<string>();
            string[] rlist = { ReportSelectTime.MON, ReportSelectTime.TUE, ReportSelectTime.WEN, ReportSelectTime.THU, ReportSelectTime.FRI, ReportSelectTime.SAT, ReportSelectTime.SUN, ReportSelectTime.EVE };
            rsilist.AddRange(rlist);
            if (vm != null)
            {
                ViewBag.ReportTimeList = CreateSelectList(rsilist, vm.reporttime);
            }
            else
            {
                ViewBag.ReportTimeList = CreateSelectList(rsilist,"");
            }
            
            return View(vm);
        }

        [HttpPost, ActionName("IBook")]
        [ValidateAntiForgeryToken]
        public ActionResult IBookPost()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

            var vm = new PJReportViewModels();
            vm.username = updater;

            for (var i = 0; i < 20; i++)
            {
                if (Request.Form["pckbox" + i] != null && string.Compare(Request.Form["pckbox" + i], "true", true) == 0)
                {
                    vm.ProjectList.Add(Request.Form["PJKEY" + i]);
                }
            }

            for (var i = 0; i < 20; i++)
            {
                if (Request.Form["rckbox" + i] != null && string.Compare(Request.Form["rckbox" + i], "true", true) == 0)
                {
                    vm.ReportTypeList.Add(Request.Form["RPType" + i]);
                }
            }

            vm.reporttime = Request.Form["ReportTimeList"].ToString();
            vm.lastupdatetime = DateTime.Now;
            vm.StoreBookReportRecord();


            ViewBag.PJList = ProjectViewModels.RetrieveAllProjectKey();

            var temptypelist = new List<string>();
            temptypelist.Add(PJReportType.YieldTrend);
            temptypelist.Add(PJReportType.IssueTrend);
            temptypelist.Add(PJReportType.FailureTrend);
            temptypelist.Add(PJReportType.MonthlyPareto);
            ViewBag.RepTypeList = temptypelist;

            vm = PJReportViewModels.RetrieveBookReportRecord(updater);
            var rsilist = new List<string>();
            string[] rlist = { ReportSelectTime.MON, ReportSelectTime.TUE, ReportSelectTime.WEN, ReportSelectTime.THU, ReportSelectTime.FRI, ReportSelectTime.SAT, ReportSelectTime.SUN, ReportSelectTime.EVE };
            rsilist.AddRange(rlist);
            if (vm != null)
            {
                ViewBag.ReportTimeList = CreateSelectList(rsilist, vm.reporttime);
            }
            else
            {
                ViewBag.ReportTimeList = CreateSelectList(rsilist, "");
            }

            return View(vm);
        }

        public ActionResult ViewReport()
        {

        }
    }
}