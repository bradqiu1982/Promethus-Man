using Prometheus.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

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

            var projlist = ProjectViewModels.RetrieveAllProjectKey();
            var slist = CreateSelectList(projlist, vm.ProjectKey);
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
            string[] rlist = { "Pending","Working", "Fixed", "Done"
            ,"Will Not Fix","Unresolved","Cannot Reproduce"};
            rsilist.AddRange(rlist);
            slist = CreateSelectList(rsilist, vm.Resolution);
            ViewBag.resolutionlist = slist;

            var asilist = UserViewModels.RetrieveAllUser();
            slist = CreateSelectList(asilist, vm.Assignee);
            ViewBag.assigneelist = slist;
        }

        // GET: Issue
        public ActionResult CreateIssue()
        {
            var vm = new IssueViewModels();
            CreateAllLists(vm);

            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {
                vm.Reporter = ckdict["logonuser"].Split(new char[] { '|' })[0];
                return View(vm);
            }
            else
            {
                var ck = new Dictionary<string, string>();
                ck.Add("logonredirectctrl", "Issue");
                ck.Add("logonredirectact", "CreateIssue");
                CookieUtility.SetCookie(this, ck);
                return RedirectToAction("LoginUser", "User");
            }
        }

        [HttpPost, ActionName("CreateIssue")]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePostIssue()
        {
            var vm = new IssueViewModels();
            vm.ProjectKey = Request.Form["projectlist"].ToString();
            vm.IssueKey = IssueViewModels.GetUniqKey();
            vm.IssueType = Request.Form["issuetypelist"].ToString();
            vm.Summary = Request.Form["Summary"];
            vm.Priority = Request.Form["prioritylist"].ToString();
            vm.DueDate = DateTime.Parse(Request.Form["DueDate"]);
            vm.ReportDate = DateTime.Now;
            vm.Assignee = Request.Form["assigneelist"].ToString();
            vm.Reporter = Request.Form["Reporter"];
            vm.Resolution = Request.Form["resolutionlist"].ToString();
            vm.ResolvedDate = DateTime.Parse("1982-05-06 01:01:01");

            if (string.IsNullOrEmpty(Request.Form["editor1"]))
            {
                var createerror = "<h3><font color=\"red\">Fail to create/modify Issue: Description can not be empty</font></h3>";
                ViewBag.CreateError = createerror;
                return View(vm);
            }
            vm.Description = Server.HtmlDecode(Request.Form["editor1"]);

            vm.StoreIssue();

            ProjectEvent.CreateUserEvent(vm.ProjectKey,vm.Reporter,vm.Assignee,vm.Summary,vm.IssueKey);

            var dict = new RouteValueDictionary();
            dict.Add("issuekey", vm.IssueKey);
            return RedirectToAction("UpdateIssue", "Issue", dict);
        }

        public ActionResult UpdateIssue(string issuekey)
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {

            }
            else
            {
                var ck = new Dictionary<string, string>();
                ck.Add("logonredirectctrl", "Issue");
                ck.Add("logonredirectact", "UpdateIssue");
                ck.Add("issuekey", issuekey);
                CookieUtility.SetCookie(this, ck);
                return RedirectToAction("LoginUser", "User");
            }

            var key = "";
            if (!string.IsNullOrEmpty(issuekey))
            {
                key = issuekey;
            }
            else if (ckdict.ContainsKey("issuekey") && !string.IsNullOrEmpty(ckdict["issuekey"]))
            {
                key = ckdict["issuekey"];
            }

            if (string.IsNullOrEmpty(key))
                return View();

            var ret = IssueViewModels.RetrieveIssueByIssueKey(key);
            
            if (ret != null)
            {
                ret.Reporter = ckdict["logonuser"].Split(new char[] { '|' })[0];
                CreateAllLists(ret);
                return View(ret);
            }
            else
            {
                return View();
            }
        }

        [HttpPost, ActionName("UpdateIssue")]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateIssuePost(string key)
        {
            return View();
        }
    }
}