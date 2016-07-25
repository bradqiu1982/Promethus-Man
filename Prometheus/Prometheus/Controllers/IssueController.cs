using Prometheus.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prometheus.Controllers
{
    public class IssueController : Controller
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


        private void CreateAllLists(IssueViewModels vm)
        {
            var projlist = new List<string>();
            string[] plist = { "CFP4-SR4", "QSFP28G", "EDR25G" };
            projlist.AddRange(plist);
            var slist = CreateSelectList(projlist, vm.ProjectName);
            ViewBag.projectlist = slist;

            var typelist = new List<string>();
            string[] tlist = { "Bug", "New Feature", "Task"
            ,"Improvement","Document"};
            typelist.AddRange(tlist);
            slist = CreateSelectList(typelist, vm.IssueType);
             ViewBag.issuetypelist = slist;

            var prilist = new List<string>();
            string[] prlist = { "Major", "Blocker", "Critical"
            ,"Minor","Trivial"};
            prilist.AddRange(prlist);
            slist = CreateSelectList(prilist, vm.Priority);
            ViewBag.prioritylist = slist;

            var rsilist = new List<string>();
            string[] rlist = { "Working", "Fixed", "Done"
            ,"Will Not Fix","Unresolved","Cannot Reproduce"};
            rsilist.AddRange(rlist);
            slist = CreateSelectList(rsilist, vm.Resolution);
            ViewBag.resolutionlist = slist;

            var asilist = new List<string>();
            string[] alist = { "Brad.qiu@finisar.com", "Alex.chen@finisar.com"
                    , "Sean.Peng@finisar.com"};
            asilist.AddRange(alist);
            slist = CreateSelectList(asilist, vm.Assignee);
            ViewBag.assigneelist = slist;
        }

        // GET: Issue
        public ActionResult CreateIssue()
        {
            var vm = new IssueViewModels();
            CreateAllLists(vm);
            var ckdict = UserController.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && ckdict["logonuser"] != "")
            {
                vm.Reporter = ckdict["logonuser"].Split(new char[] { '|'})[0];
                return View(vm);
            }
            else
            {
                var ck = new Dictionary<string, string>();
                ck.Add("logonredirectctrl", "Issue");
                ck.Add("logonredirectact", "CreateIssue");
                UserController.SetCookie(this, ck);
                return RedirectToAction("LoginUser", "User");
            }
        }

        [HttpPost, ActionName("CreateIssue")]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePostIssue()
        {
             return View();
        }
    }
}