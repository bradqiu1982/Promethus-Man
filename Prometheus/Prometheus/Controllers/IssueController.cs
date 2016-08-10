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
            string[] tlist = { ISSUETP.Bug, ISSUETP.NewFeature, ISSUETP.Task
            ,ISSUETP.Improvement,ISSUETP.Document};

            typelist.AddRange(tlist);
            slist = CreateSelectList(typelist, vm.IssueType);
            ViewBag.issuetypelist = slist;

            var prilist = new List<string>();
            string[] prlist = { ISSUEPR.Major, ISSUEPR.Blocker,ISSUEPR.Critical
            ,ISSUEPR.Minor,ISSUEPR.Trivial};

            prilist.AddRange(prlist);
            slist = CreateSelectList(prilist, vm.Priority);
            ViewBag.prioritylist = slist;

            var rsilist = new List<string>();
            string[] rlist = { Resolute.Pending,Resolute.Working,Resolute.Reopen,Resolute.Fixed,Resolute.Done,Resolute.NotFix,Resolute.Unresolved, Resolute.NotReproduce};
            rsilist.AddRange(rlist);
            slist = CreateSelectList(rsilist, vm.Resolution);
            ViewBag.resolutionlist = slist;

            var asilist = UserViewModels.RetrieveAllUser();
            slist = CreateSelectList(asilist, vm.Assignee);
            ViewBag.assigneelist = slist;

            var rpilist = UserViewModels.RetrieveAllUser();
            slist = CreateSelectList(rpilist, vm.Reporter);
            ViewBag.reporterlist = slist;
        }

        // GET: Issue
        public ActionResult CreateIssue()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {
                var vm = new IssueViewModels();
                vm.Reporter = ckdict["logonuser"].Split(new char[] { '|' })[0];
                CreateAllLists(vm);
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
            vm.Reporter = Request.Form["reporterlist"].ToString();
            vm.Resolution = Request.Form["resolutionlist"].ToString();
            vm.ResolvedDate = DateTime.Parse("1982-05-06 01:01:01");

            if (string.IsNullOrEmpty(Request.Form["editor1"]))
            {
                var createerror = "<h3><font color=\"red\">Fail to create/modify Issue: Description can not be empty</font></h3>";
                ViewBag.CreateError = createerror;
                CreateAllLists(vm);
                return View(vm);
            }
            vm.Description = Server.HtmlDecode(Request.Form["editor1"]);

            vm.StoreIssue();

            ProjectEvent.CreateIssueEvent(vm.ProjectKey,vm.Reporter,vm.Assignee,vm.Summary,vm.IssueKey);

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
            {
                var tempvm = new IssueViewModels();
                CreateAllLists(tempvm);
                return View();
            }

            var ret = IssueViewModels.RetrieveIssueByIssueKey(key);
            
            if (ret != null)
            {
                ret.Reporter = ckdict["logonuser"].Split(new char[] { '|' })[0];
                CreateAllLists(ret);
                return View(ret);
            }
            else
            {
                var tempvm = new IssueViewModels();
                CreateAllLists(tempvm);
                return View();
            }
        }

        [HttpPost, ActionName("UpdateIssue")]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateIssuePost()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

            var issuekey = Request.Form["IssueKey"];
            var originaldata = IssueViewModels.RetrieveIssueByIssueKey(issuekey);

            var vm = new IssueViewModels();
            vm.IssueKey = issuekey;
            vm.Reporter = updater;
            vm.IssueType = Request.Form["issuetypelist"].ToString();
            vm.Priority = Request.Form["prioritylist"].ToString();
            vm.DueDate = DateTime.Parse(Request.Form["DueDate"]);
            vm.Assignee = Request.Form["assigneelist"].ToString();
            vm.Resolution = Request.Form["resolutionlist"].ToString();
            vm.ResolvedDate = DateTime.Parse("1982-05-06 01:01:01");

            if (!string.IsNullOrEmpty(Request.Form["editor1"]))
            {
                vm.Description = Server.HtmlDecode(Request.Form["editor1"]);
            }
            else
                vm.Description = "";

            vm.UpdateIssue();

            ProjectEvent.OperateIssueEvent(originaldata.ProjectKey, updater, "Updated", originaldata.Summary, originaldata.IssueKey);

            if (string.Compare(originaldata.Assignee, vm.Assignee, true) != 0)
            {
                vm.UpdateIAssign();
                ProjectEvent.AssignIssueEvent(originaldata.ProjectKey, updater, vm.Assignee, originaldata.Summary, originaldata.IssueKey);
            }

            if (string.Compare(originaldata.Resolution, vm.Resolution, true) != 0)
            {
                if (vm.IssueClosed())
                {
                    ProjectEvent.OperateIssueEvent(originaldata.ProjectKey, updater, "Closed", originaldata.Summary, originaldata.IssueKey);
                    vm.CloseIssue();
                }

                if (string.Compare(vm.Resolution, Resolute.Working) == 0)
                {
                    ProjectEvent.OperateIssueEvent(originaldata.ProjectKey, updater, "Started", originaldata.Summary, originaldata.IssueKey);
                }

                if (string.Compare(vm.Resolution, Resolute.Reopen) == 0)
                {
                    ProjectEvent.OperateIssueEvent(originaldata.ProjectKey, updater, "Reopened", originaldata.Summary, originaldata.IssueKey);
                }
            }

            var newdata = IssueViewModels.RetrieveIssueByIssueKey(issuekey);
            CreateAllLists(newdata);
            return View(newdata);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PrepareSubIssue()
        {
            var parentkey = Request.Form["HIssueKey"];
            var projectkey = Request.Form["HProjectKey"];
            var dict = new RouteValueDictionary();
            dict.Add("parentkey", parentkey);
            dict.Add("projectkey", projectkey);
            return RedirectToAction("CreateSubIssue", dict);
        }

        public ActionResult CreateSubIssue(string parentkey,string projectkey)
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var vm = new IssueViewModels();
            vm.Reporter = ckdict["logonuser"].Split(new char[] { '|' })[0];
            vm.ParentIssueKey = parentkey;
            vm.ProjectKey = projectkey;
            CreateAllLists(vm);
            return View(vm);
        }

        [HttpPost, ActionName("CreateSubIssue")]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePostSubIssue()
        {
            var vm = new IssueViewModels();
            vm.ProjectKey = Request.Form["projectlist"].ToString();
            vm.IssueKey = IssueViewModels.GetUniqKey();
            vm.ParentIssueKey = Request.Form["HParentIssueKey"];
            vm.IssueType = Request.Form["issuetypelist"].ToString();
            vm.Summary = Request.Form["Summary"];
            vm.Priority = Request.Form["prioritylist"].ToString();
            vm.DueDate = DateTime.Parse(Request.Form["DueDate"]);
            vm.ReportDate = DateTime.Now;
            vm.Assignee = Request.Form["assigneelist"].ToString();
            vm.Reporter = Request.Form["reporterlist"].ToString();
            vm.Resolution = Request.Form["resolutionlist"].ToString();
            vm.ResolvedDate = DateTime.Parse("1982-05-06 01:01:01");

            if (string.IsNullOrEmpty(Request.Form["editor1"]))
            {
                var createerror = "<h3><font color=\"red\">Fail to create/modify Issue: Description can not be empty</font></h3>";
                ViewBag.CreateError = createerror;
                CreateAllLists(vm);
                return View(vm);
            }
            vm.Description = Server.HtmlDecode(Request.Form["editor1"]);

            vm.StoreSubIssue();

            ProjectEvent.CreateIssueEvent(vm.ProjectKey, vm.Reporter, vm.Assignee, vm.Summary, vm.IssueKey);

            var dict = new RouteValueDictionary();
            dict.Add("issuekey", vm.ParentIssueKey);
            return RedirectToAction("UpdateIssue", "Issue", dict);
        }

    }
}