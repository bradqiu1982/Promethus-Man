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
    public class IssueController : Controller
    {

        private List<SelectListItem> CreateSelectList(List<string> valist, string defVal)
        {
            bool selected = false;
            var pslist = new List<SelectListItem>();
            foreach (var p in valist)
            {
                var pitem = new SelectListItem();
                pitem.Text = p;
                pitem.Value = p;
                if (!string.IsNullOrEmpty(defVal) && string.Compare(defVal, p,true) == 0)
                {
                    pitem.Selected = true;
                    selected = true;
                }
                pslist.Add(pitem);
            }

            if (!selected && pslist.Count > 0)
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
            string[] tlist = { ISSUETP.Bug,ISSUETP.Task,ISSUETP.RMA,ISSUETP.OBA, ISSUETP.NewFeature
            ,ISSUETP.Improvement,ISSUETP.Document,ISSUETP.NPIPROC};

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
            string[] rlist = { Resolute.Pending, Resolute.Working, Resolute.Reopen, Resolute.Fixed, Resolute.Done, Resolute.NotFix, Resolute.Unresolved, Resolute.NotReproduce,Resolute.AutoClose };
            rsilist.AddRange(rlist);
            slist = CreateSelectList(rsilist, vm.Resolution);
            ViewBag.resolutionlist = slist;

            var asilist = UserViewModels.RetrieveAllUser();
            slist = CreateSelectList(asilist, vm.Assignee);
            ViewBag.assigneelist = slist;

            var rpilist = UserViewModels.RetrieveAllUser();
            slist = CreateSelectList(rpilist, vm.Reporter);
            ViewBag.reporterlist = slist;

            var fcilist = new List<string>();
            string[] clist = { "None", RMAFAILCODE.Cable, RMAFAILCODE.CDR, RMAFAILCODE.Contamination, RMAFAILCODE.Epoxy, RMAFAILCODE.Firmware, RMAFAILCODE.Flex,
            RMAFAILCODE.LaserDriver,RMAFAILCODE.Lens,RMAFAILCODE.MCU,RMAFAILCODE.Mechanical,RMAFAILCODE.MPD,RMAFAILCODE.NTF,RMAFAILCODE.OtherIC,RMAFAILCODE.Others,
            RMAFAILCODE.Passivecomponent,RMAFAILCODE.PCB,RMAFAILCODE.PD,RMAFAILCODE.Process,RMAFAILCODE.SMT,RMAFAILCODE.TIA,RMAFAILCODE.VCSEL,RMAFAILCODE.VMI,RMAFAILCODE.WrongEEPROM,RMAFAILCODE.Customerissue};

            fcilist.AddRange(clist);
            slist = CreateSelectList(fcilist, vm.RMAFailureCode);
            ViewBag.RMAFailureCode = slist;

            var metriallist = new List<string>();
            string[] mlist = { "None", "Scrap", "Rework", "UAI" };
            metriallist.AddRange(mlist);
            slist = CreateSelectList(metriallist, vm.MaterialDisposition);
            ViewBag.dispositionlist = slist;

            //var cmelist = new List<string>();
            //string[] clist = { COMMENTTYPE.Description,COMMENTTYPE.RootCause,COMMENTTYPE.CustomReport,COMMENTTYPE.InternalReport};
            //cmelist.AddRange(clist);
            //slist = CreateSelectList(cmelist, "");
            //ViewBag.cemtypelist = slist;


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

        private void SendTaskEvent(IssueViewModels vm, string operate)
        {
                var routevalue = new RouteValueDictionary();
                routevalue.Add("issuekey", vm.IssueKey);
                //send validate email
                string scheme = this.Url.RequestContext.HttpContext.Request.Url.Scheme;
                string validatestr = this.Url.Action("UpdateIssue", "Issue", routevalue, scheme);

            var netcomputername = "";
            try { netcomputername = System.Net.Dns.GetHostName(); }
            catch (Exception ex) { }
            validatestr = validatestr.Replace("/localhost/", "/" + netcomputername + "/");

            var content = vm.Summary + " is " + operate + " by " + vm.Reporter + " :\r\n " + validatestr;

                var toaddrs = new List<string>();
                toaddrs.Add(vm.Reporter);
                toaddrs.Add(vm.Assignee);
                if (vm.RelativePeopleList.Count > 0)
                {
                    toaddrs.AddRange(vm.RelativePeopleList);
                }
                EmailUtility.SendEmail("Parallel NPI Trace Notice", toaddrs, content);
        }

        private void SendTaskEvent(IssueViewModels vm, string operate, string Reporter, string Assignee)
        {
            var routevalue = new RouteValueDictionary();
            routevalue.Add("issuekey", vm.IssueKey);
            //send validate email
            string scheme = this.Url.RequestContext.HttpContext.Request.Url.Scheme;
            string validatestr = this.Url.Action("UpdateIssue", "Issue", routevalue, scheme);

            var netcomputername = "";
            try { netcomputername = System.Net.Dns.GetHostName(); }
            catch (Exception ex) { }
            validatestr = validatestr.Replace("/localhost/", "/" + netcomputername + "/");

            var content = vm.Summary + " is " + operate + " by " + vm.Reporter + " :\r\n " + validatestr;

            var toaddrs = new List<string>();
            toaddrs.Add(Reporter);
            toaddrs.Add(Assignee);
            if (vm.RelativePeopleList.Count > 0)
            {
                toaddrs.AddRange(vm.RelativePeopleList);
            }
            EmailUtility.SendEmail("Parallel NPI Trace Notice", toaddrs, content);
            new System.Threading.ManualResetEvent(false).WaitOne(300);
        }

        [HttpPost, ActionName("CreateIssue")]
        [ValidateAntiForgeryToken]
        public ActionResult CreateIssuePost()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

            var vm = new IssueViewModels();
            vm.ProjectKey = Request.Form["projectlist"].ToString();
            vm.IssueKey = IssueViewModels.GetUniqKey();
            vm.IssueType = Request.Form["issuetypelist"].ToString();
            vm.Summary = Request.Form["Summary"];
            vm.Priority = Request.Form["prioritylist"].ToString();
            vm.DueDate = DateTime.Parse(Request.Form["DueDate"]);
            vm.ReportDate = DateTime.Now;
            vm.Assignee = Request.Form["assigneelist"].ToString();
            vm.Reporter = updater;//Request.Form["reporterlist"].ToString();
            vm.Resolution = Request.Form["resolutionlist"].ToString();
            vm.ResolvedDate = DateTime.Parse("1982-05-06 01:01:01");
            vm.RelativePeoples = Request.Form["RPeopleAddr"];

            if (!string.IsNullOrEmpty(Request.Form["editor1"]))
            {
                vm.Description = Server.HtmlDecode(Request.Form["editor1"]);
                vm.CommentType = COMMENTTYPE.Description;
            }
            else
                vm.Description = "";

            vm.StoreIssue();

            if (!string.IsNullOrEmpty(Request.Form["attachmentupload"]))
            {
                var urls = ReceiveRMAFiles();
                var internalreportfile = Request.Form["attachmentupload"];
                var originalname = Path.GetFileNameWithoutExtension(internalreportfile)
                    .Replace(" ", "_").Replace("#", "")
                    .Replace("&", "").Replace("?", "").Replace("%", "").Replace("+", "");

                var url = "";
                foreach (var r in urls)
                {
                    if (r.Contains(originalname))
                    {
                        url = r;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(url))
                {
                    IssueViewModels.StoreIssueAttachment(vm.IssueKey, url);
                }
            }


            ProjectEvent.CreateIssueEvent(vm.ProjectKey, vm.Reporter, vm.Assignee, vm.Summary, vm.IssueKey);

            //if (vm.RelativePeopleList.Count > 0)
            //{
                SendTaskEvent(vm, "asigned to you");
            //}

            var dict = new RouteValueDictionary();
            dict.Add("issuekey", vm.IssueKey);
            return RedirectToAction("UpdateIssue", "Issue", dict);
        }

        private List<SelectListItem> CreateBIRootIssue(string pjkey,string sn)
        {
            var defval = IssueViewModels.RetrieveBIRootCause(pjkey, sn);

            var plist = new List<SelectListItem>();

            var p = new SelectListItem();
            p.Text = "NONE";
            p.Value = "NONE";
            plist.Add(p);

            string[] blist = { BIROOTCAUSE.VCSELISSUE,BIROOTCAUSE.DATAISSUE, BIROOTCAUSE.COMMISSUE, BIROOTCAUSE.QVLDISSUE, BIROOTCAUSE.PCBAISSUE, BIROOTCAUSE.VMIISSUE, BIROOTCAUSE.OTHERISSUE };
            foreach (var item in blist)
            {
                p = new SelectListItem();
                p.Text = item;
                p.Value = item;
                if (string.Compare(item, defval) == 0)
                {
                    p.Selected = true;
                }
                plist.Add(p);
            }

            if (string.IsNullOrEmpty(defval))
            {
                plist[0].Selected = true;
            }

            return plist;

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
                var ck = new Dictionary<string, string>();
                ck.Add("issuekey", issuekey);
                ck.Add("currentaction", "UpdateIssue");
                CookieUtility.SetCookie(this, ck);
                key = issuekey;
            }
            else if (ckdict.ContainsKey("issuekey") && !string.IsNullOrEmpty(ckdict["issuekey"]))
            {
                key = ckdict["issuekey"];
                var ck = new Dictionary<string, string>();
                ck.Add("currentaction", "UpdateIssue");
                CookieUtility.SetCookie(this, ck);
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
                if (string.Compare(ret.IssueType, ISSUETP.RMA) == 0)
                {
                    var dict = new RouteValueDictionary();
                    dict.Add("issuekey", key);
                    return RedirectToAction("UpdateRMA", "Issue", dict);
                }

                if (string.Compare(ret.IssueType, ISSUETP.Bug) == 0)
                {
                    var dict = new RouteValueDictionary();
                    dict.Add("issuekey", key);
                    return RedirectToAction("UpdateBug", "Issue", dict);
                }

                if (string.Compare(ret.IssueType, ISSUETP.OBA) == 0)
                {
                    var dict = new RouteValueDictionary();
                    dict.Add("issuekey", key);
                    return RedirectToAction("UpdateOBA", "Issue", dict);
                }

                var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
                ViewBag.isassignee = false;
                if (string.Compare(updater, ret.Assignee, true) == 0
                        || string.Compare(updater, ret.Reporter,true) == 0)
                {
                    ViewBag.isassignee = true;
                }

                ret.Reporter = updater;
                CreateAllLists(ret);

                if (ret.Summary.Contains(" @Burn-In Step "))
                {
                    var sn = ret.Summary.Split(new string[] { " " }, StringSplitOptions.None)[1].Trim().ToUpper();
                    ViewBag.birootcauselist = CreateBIRootIssue(ret.ProjectKey, sn);
                }
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

           if (Request.Form["deleteisu"] != null)
            {
                if (string.Compare(updater, originaldata.Reporter, true) == 0
                    || string.Compare(updater, originaldata.Assignee, true) == 0)
                {
                    IssueViewModels.RemoveIssue(issuekey);
                    var dict = new RouteValueDictionary();
                    dict.Add("ProjectKey", originaldata.ProjectKey);
                    return RedirectToAction("ProjectDetail", "Project",dict);
                }
            }

            var vm = new IssueViewModels();
            vm.IssueKey = issuekey;
            vm.Reporter = updater;
            vm.IssueType = Request.Form["issuetypelist"].ToString();
            vm.Priority = Request.Form["prioritylist"].ToString();
            vm.DueDate = DateTime.Parse(Request.Form["DueDate"]);

            if (string.Compare(originaldata.Reporter, updater, true) == 0
                || string.Compare(originaldata.Assignee, updater, true) == 0)
            {
                vm.Assignee = Request.Form["assigneelist"].ToString();
                vm.Resolution = Request.Form["resolutionlist"].ToString();
            }
            else
            {
                vm.Assignee = originaldata.Assignee;
                vm.Resolution = originaldata.Resolution;
            }

            vm.ResolvedDate = DateTime.Parse("1982-05-06 01:01:01");
            vm.RelativePeoples = Request.Form["RPeopleAddr"];

            if (!string.IsNullOrEmpty(Request.Form["editor1"]))
            {
                vm.Description = Server.HtmlDecode(Request.Form["editor1"]);
                vm.CommentType = COMMENTTYPE.Description;
                UserRankViewModel.UpdateUserRank(updater, 2);
            }
            else
                vm.Description = "";

            vm.UpdateIssue();

            if (!string.IsNullOrEmpty(Request.Form["attachmentupload"]))
            {
                var urls = ReceiveRMAFiles();
                var internalreportfile = Request.Form["attachmentupload"];
                var originalname = Path.GetFileNameWithoutExtension(internalreportfile)
                    .Replace(" ", "_").Replace("#", "")
                    .Replace("&", "").Replace("?", "").Replace("%", "").Replace("+", "");

                var url = "";
                foreach (var r in urls)
                {
                    if (r.Contains(originalname))
                    {
                        url = r;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(url))
                {
                    IssueViewModels.StoreIssueAttachment(vm.IssueKey, url);
                    UserRankViewModel.UpdateUserRank(updater, 5);
                }
            }

            ProjectEvent.OperateIssueEvent(originaldata.ProjectKey, updater, "Updated", originaldata.Summary, originaldata.IssueKey);

            if (string.Compare(originaldata.Assignee, vm.Assignee, true) != 0)
            {
                vm.UpdateIAssign();
                ProjectEvent.AssignIssueEvent(originaldata.ProjectKey, updater, vm.Assignee, originaldata.Summary, originaldata.IssueKey);
                vm.Summary = originaldata.Summary;
                SendTaskEvent(vm, "asigned to you", updater, vm.Assignee);
            }

            if (string.Compare(originaldata.Resolution, vm.Resolution, true) != 0)
            {
                if (vm.IssueClosed())
                {
                    var realissue = IssueViewModels.RetrieveIssueByIssueKey(vm.IssueKey);
                    if (realissue.AttachList.Count > 0)
                    {
                        UserRankViewModel.UpdateUserRank(updater, 5);
                    }
                    else if (realissue.CommentList.Count > 1)
                    {
                        UserRankViewModel.UpdateUserRank(updater, 2);
                    }
                    
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

            if (Request.Form["birootcauselist"] != null)
            {
                var biret = Request.Form["birootcauselist"].ToString();
                if (string.Compare(biret, "NONE") != 0)
                {
                    var sn = newdata.Summary.Split(new string[] { " " }, StringSplitOptions.None)[1].Trim().ToUpper();
                    IssueViewModels.StoreBIRootCause(newdata.ProjectKey, sn, biret);
                }
            }

            ViewBag.isassignee = false;
            if (string.Compare(updater, newdata.Assignee, true) == 0
                    || string.Compare(updater, newdata.Reporter,true) == 0)
            {
                ViewBag.isassignee = true;
            }

            CreateAllLists(newdata);

            if (newdata.Summary.Contains(" @Burn-In Step "))
            {
                var sn = newdata.Summary.Split(new string[] { " " }, StringSplitOptions.None)[1].Trim().ToUpper();
                ViewBag.birootcauselist = CreateBIRootIssue(newdata.ProjectKey, sn);
            }
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

        public ActionResult CreateSubIssue(string parentkey, string projectkey)
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
            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

            var sumpre = "";
            var vm = new IssueViewModels();
            vm.ProjectKey = Request.Form["projectlist"].ToString();
            vm.IssueKey = IssueViewModels.GetUniqKey();

            var pisk = Request.Form["HParentIssueKey"];
            if (pisk.Contains("#"))
            {
                vm.ParentIssueKey = pisk.Split(new char[] { '#' })[0];
                sumpre = pisk.Split(new char[] { '#' })[1];
            }
            else
            {
                vm.ParentIssueKey = Request.Form["HParentIssueKey"];
            }
            vm.IssueType = Request.Form["issuetypelist"].ToString();
            vm.Summary = sumpre + Request.Form["Summary"];
            vm.Priority = Request.Form["prioritylist"].ToString();
            vm.DueDate = DateTime.Parse(Request.Form["DueDate"]);
            vm.ReportDate = DateTime.Now;
            vm.Assignee = updater; //Request.Form["assigneelist"].ToString();
            vm.Reporter = updater; //Request.Form["reporterlist"].ToString();
            vm.Resolution = Request.Form["resolutionlist"].ToString();
            vm.ResolvedDate = DateTime.Parse("1982-05-06 01:01:01");
            vm.RelativePeoples = Request.Form["RPeopleAddr"];

            if (!string.IsNullOrEmpty(Request.Form["editor1"]))
            {
                vm.Description = Server.HtmlDecode(Request.Form["editor1"]);
                vm.CommentType = COMMENTTYPE.Description;
            }
            else
                vm.Description = "";

            vm.StoreSubIssue();

            if (!string.IsNullOrEmpty(Request.Form["attachmentupload"]))
            {
                var urls = ReceiveRMAFiles();
                var internalreportfile = Request.Form["attachmentupload"];
                var originalname = Path.GetFileNameWithoutExtension(internalreportfile)
                    .Replace(" ", "_").Replace("#", "")
                    .Replace("&", "").Replace("?", "").Replace("%", "").Replace("+", "");

                var url = "";
                foreach (var r in urls)
                {
                    if (r.Contains(originalname))
                    {
                        url = r;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(url))
                {
                    IssueViewModels.StoreIssueAttachment(vm.IssueKey, url);
                }
            }

            ProjectEvent.CreateIssueEvent(vm.ProjectKey, vm.Reporter, vm.Assignee, vm.Summary, vm.IssueKey);

            var dict = new RouteValueDictionary();
            dict.Add("issuekey", vm.ParentIssueKey);
            return RedirectToAction("UpdateIssue", "Issue", dict);
        }

        private List<SelectListItem> CreateSearchSelectList(List<string> valist, string defVal)
        {
            bool selected = false;
            var pslist = new List<SelectListItem>();
            var pitem = new SelectListItem();
            pitem.Text = "NONE";
            pitem.Value = "NONE";
            pslist.Add(pitem);

            foreach (var p in valist)
            {
                pitem = new SelectListItem();
                pitem.Text = p;
                pitem.Value = p;
                if (!string.IsNullOrEmpty(defVal) && string.Compare(defVal, p,true) == 0)
                {
                    pitem.Selected = true;
                    selected = true;
                }
                pslist.Add(pitem);
            }

            if (!selected && pslist.Count > 0)
            {
                pslist[0].Selected = true;
            }

            return pslist;
        }

        private void CreateAllSearchLists()
        {
            var projlist = ProjectViewModels.RetrieveAllProjectKey();
            var slist = CreateSearchSelectList(projlist, "");
            ViewBag.projectlist = slist;

            var typelist = new List<string>();
            string[] tlist = { ISSUETP.Bug, ISSUETP.Task, ISSUETP.RMA, ISSUETP.OBA, ISSUETP.NewFeature, ISSUETP.Improvement, ISSUETP.Document, ISSUETP.NPIPROC };
            typelist.AddRange(tlist);
            slist = CreateSearchSelectList(typelist, "");
            ViewBag.issuetypelist = slist;

            var rsilist = new List<string>();
            string[] rlist = { Resolute.Pending, Resolute.Working, Resolute.Reopen, Resolute.Fixed, Resolute.Done, Resolute.NotFix, Resolute.Unresolved, Resolute.NotReproduce };
            rsilist.AddRange(rlist);
            slist = CreateSearchSelectList(rsilist, "");
            ViewBag.resolutionlist = slist;

            var asilist = UserViewModels.RetrieveAllUser();
            slist = CreateSearchSelectList(asilist, "");
            ViewBag.assigneelist = slist;

        }

        public ActionResult SearchIssue()
        {
            CreateAllSearchLists();
            return View();
        }

        [HttpPost, ActionName("SearchIssue")]
        [ValidateAntiForgeryToken]
        public ActionResult SearchIssuePost()
        {
            var pj = "";
            var tp = "";
            var rs = "";
            var asn = "";
            var sd = "";
            var ed = "";
            var desp = "";

            if (string.Compare(Request.Form["projectlist"].ToString(), "NONE") != 0)
            {
                pj = Request.Form["projectlist"].ToString();
            }

            if (string.Compare(Request.Form["issuetypelist"].ToString(), "NONE") != 0)
            {
                tp = Request.Form["issuetypelist"].ToString();
            }

            if (string.Compare(Request.Form["resolutionlist"].ToString(), "NONE") != 0)
            {
                rs = Request.Form["resolutionlist"].ToString();
            }

            if (string.Compare(Request.Form["assigneelist"].ToString(), "NONE") != 0)
            {
                asn = Request.Form["assigneelist"].ToString();
            }

            if (!string.IsNullOrEmpty(Request.Form["StartDate"]))
            {
                sd = Request.Form["StartDate"];
            }

            if (!string.IsNullOrEmpty(Request.Form["EndDate"]))
            {
                ed = Request.Form["EndDate"];
            }

            if (!string.IsNullOrEmpty(Request.Form["Summary"]))
            {
                desp = Request.Form["Summary"];
            }

            if (string.IsNullOrEmpty(pj)
                && string.IsNullOrEmpty(tp)
                && string.IsNullOrEmpty(rs)
                && string.IsNullOrEmpty(asn)
                && string.IsNullOrEmpty(sd)
                && string.IsNullOrEmpty(ed)
                && string.IsNullOrEmpty(desp))
            {
                ViewBag.SearchError = "At least choose one condition to search issue";
                CreateAllSearchLists();
                return View();
            }

            var vmlist = IssueViewModels.SearchIssue(pj, tp, rs, asn, sd, ed, desp, 100);

            CreateAllSearchLists();
            return View(vmlist);
        }

        public ActionResult IssueAttach(string issuekey)
        {
            if (!string.IsNullOrEmpty(issuekey))
            {
                var vm = IssueViewModels.RetrieveIssueByIssueKey(issuekey);
                if (vm != null)
                {
                    vm.RetrieveAttachment(vm.IssueKey);
                    return View(vm);
                }
            }
            return View();
        }

        public ActionResult CreateRMA()
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
                ck.Add("logonredirectact", "CreateRMA");
                CookieUtility.SetCookie(this, ck);
                return RedirectToAction("LoginUser", "User");
            }
        }

        private void  SendRMAEvent(IssueViewModels vm, string operate,bool nocheck= false)
        {
            var alertime = vm.RetrieveAlertEmailDate(vm.IssueKey);
            if ((!string.IsNullOrEmpty(alertime) && DateTime.Parse(alertime).AddHours(24) < DateTime.Now) || nocheck)
            {
                vm.UpdateAlertEmailDate();

                var routevalue = new RouteValueDictionary();
                routevalue.Add("issuekey", vm.IssueKey);
                //send validate email
                string scheme = this.Url.RequestContext.HttpContext.Request.Url.Scheme;
                string validatestr = this.Url.Action("UpdateIssue", "Issue", routevalue, scheme);

                var netcomputername = "";
                try { netcomputername = System.Net.Dns.GetHostName(); }
                catch (Exception ex) { }
                validatestr = validatestr.Replace("/localhost/", "/" + netcomputername + "/");

                var content = vm.Summary + " is " + operate + " by " + vm.Reporter + " :\r\n " + validatestr;

                var toaddrs = new List<string>();
                toaddrs.AddRange(vm.RelativePeopleList);
                toaddrs.Add(vm.Assignee);
                toaddrs.Add(vm.Reporter);
                EmailUtility.SendEmail("Parallel NPI Trace Notice", toaddrs, content);
                new System.Threading.ManualResetEvent(false).WaitOne(300);
            }
        }


        [HttpPost, ActionName("CreateRMA")]
        [ValidateAntiForgeryToken]
        public ActionResult CreateRMAPost()
        {
            var ckdict = CookieUtility.UnpackCookie(this);

            var vm = new IssueViewModels();
            vm.ProjectKey = Request.Form["projectlist"].ToString();
            vm.IssueKey = IssueViewModels.GetUniqKey();
            vm.IssueType = ISSUETP.RMA;

            vm.FinisarRMA = Request.Form["FRMANUM"];
            vm.RMAFailureCode = "";
            vm.FVCode = "";
            vm.FinisarModel = Request.Form["FinisarModel"];
            vm.ECustomer = Request.Form["ECustomer"];
            vm.CRMANUM = Request.Form["CRMANUM"];
            vm.CReport = Request.Form["CReport"];
            vm.RelativePeoples = Request.Form["RPeopleAddr"];
            vm.ModuleSN = Request.Form["ModuleSN"];

            vm.Summary = "RMA " + vm.FinisarRMA + " for module " + vm.FinisarModel + " from " + vm.ECustomer + ": " + vm.CReport;
            if (vm.Summary.Length > 200)
            {
                vm.Summary = vm.Summary.Substring(0, 198);
            }

            vm.Priority = Request.Form["prioritylist"].ToString();
            vm.DueDate = DateTime.Parse(Request.Form["DueDate"]);
            vm.ReportDate = DateTime.Now;
            vm.Assignee = Request.Form["assigneelist"].ToString();
            vm.Reporter = ckdict["logonuser"].Split(new char[] { '|' })[0];

            vm.Resolution = Resolute.Pending;

            vm.ResolvedDate = DateTime.Parse("1982-05-06 01:01:01");

            if (!string.IsNullOrEmpty(Request.Form["editor1"]))
            {
                vm.Description = Server.HtmlDecode(Request.Form["editor1"]);
                vm.CommentType = COMMENTTYPE.Description;
            }

            vm.StoreIssue();

            if (!string.IsNullOrEmpty(Request.Form["attachmentupload"]))
            {
                var urls = ReceiveRMAFiles();
                var internalreportfile = Request.Form["attachmentupload"];
                var originalname = Path.GetFileNameWithoutExtension(internalreportfile)
                    .Replace(" ", "_").Replace("#", "")
                    .Replace("&", "").Replace("?", "").Replace("%", "").Replace("+", "");

                var url = "";
                foreach (var r in urls)
                {
                    if (r.Contains(originalname))
                    {
                        url = r;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(url))
                {
                    IssueViewModels.StoreIssueAttachment(vm.IssueKey, url);
                }
            }

            ProjectEvent.CreateIssueEvent(vm.ProjectKey, vm.Reporter, vm.Assignee, vm.Summary, vm.IssueKey);
            SendRMAEvent(vm, "created",true);

            var dict = new RouteValueDictionary();
            dict.Add("issuekey", vm.IssueKey);
            return RedirectToAction("UpdateRMA", "Issue", dict);
        }

        public ActionResult UpdateBug(string issuekey)
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
                var ck = new Dictionary<string, string>();
                ck.Add("issuekey", issuekey);
                ck.Add("currentaction", "UpdateIssue");
                CookieUtility.SetCookie(this, ck);
                key = issuekey;
            }
            else if (ckdict.ContainsKey("issuekey") && !string.IsNullOrEmpty(ckdict["issuekey"]))
            {
                key = ckdict["issuekey"];
                var ck = new Dictionary<string, string>();
                ck.Add("currentaction", "UpdateIssue");
                CookieUtility.SetCookie(this, ck);
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
                var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
                ViewBag.isassignee = false;
                if (string.Compare(updater, ret.Assignee, true) == 0
                        || string.Compare(updater, ret.Reporter, true) == 0)
                {
                    ViewBag.isassignee = true;
                }

                ret.Reporter = updater;
                CreateAllLists(ret);

                if (ret.Summary.Contains(" @Burn-In Step "))
                {
                    var sn = ret.Summary.Split(new string[] { " " }, StringSplitOptions.None)[1].Trim().ToUpper();
                    ViewBag.birootcauselist = CreateBIRootIssue(ret.ProjectKey, sn);
                }
                return View(ret);
            }
            else
            {
                var tempvm = new IssueViewModels();
                CreateAllLists(tempvm);
                return View();
            }
        }

        [HttpPost, ActionName("UpdateBug")]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateBugPost()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

            var issuekey = Request.Form["IssueKey"];
            var originaldata = IssueViewModels.RetrieveIssueByIssueKey(issuekey);

            if (Request.Form["deleteisu"] != null)
            {
                if (string.Compare(updater, originaldata.Reporter, true) == 0
                    || string.Compare(updater, originaldata.Assignee, true) == 0)
                {
                    IssueViewModels.RemoveIssue(issuekey);
                    var dict = new RouteValueDictionary();
                    dict.Add("ProjectKey", originaldata.ProjectKey);
                    return RedirectToAction("ProjectDetail", "Project", dict);
                }
            }

            var vm = new IssueViewModels();
            vm.IssueKey = issuekey;
            vm.Reporter = updater;
            vm.IssueType = Request.Form["issuetypelist"].ToString();
            vm.Priority = Request.Form["prioritylist"].ToString();
            vm.DueDate = DateTime.Parse(Request.Form["DueDate"]);

            if (string.Compare(originaldata.Reporter, updater, true) == 0
                || string.Compare(originaldata.Assignee, updater, true) == 0)
            {
                vm.Assignee = Request.Form["assigneelist"].ToString();
                vm.Resolution = Request.Form["resolutionlist"].ToString();
            }
            else
            {
                vm.Assignee = originaldata.Assignee;
                vm.Resolution = originaldata.Resolution;
            }

            vm.ResolvedDate = DateTime.Parse("1982-05-06 01:01:01");
            vm.RelativePeoples = Request.Form["RPeopleAddr"];

            if (!string.IsNullOrEmpty(Request.Form["failureeditor"]))
            {
                var failure = Server.HtmlDecode(Request.Form["failureeditor"]);
                var dbstr = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(failure));
                var commenttype = COMMENTTYPE.FailureDetail;
                IssueViewModels.StoreIssueComment(vm.IssueKey, dbstr, vm.Reporter, commenttype);
                UserRankViewModel.UpdateUserRank(updater, 2);
            }

            if (!string.IsNullOrEmpty(Request.Form["rootcauseeditor"]))
            {
                var rootcause = Server.HtmlDecode(Request.Form["rootcauseeditor"]);
                var dbstr = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(rootcause));
                var commenttype = COMMENTTYPE.RootCause;
                IssueViewModels.StoreIssueComment(vm.IssueKey, dbstr, vm.Reporter, commenttype);
                UserRankViewModel.UpdateUserRank(updater, 5);
            }

            if (!string.IsNullOrEmpty(Request.Form["resulteditor"]))
            {
                var rootcause = Server.HtmlDecode(Request.Form["resulteditor"]);
                var dbstr = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(rootcause));
                var commenttype = COMMENTTYPE.Result;
                IssueViewModels.StoreIssueComment(vm.IssueKey, dbstr, vm.Reporter, commenttype);
                UserRankViewModel.UpdateUserRank(updater,2);
            }

            if (!string.IsNullOrEmpty(Request.Form["editor1"]))
            {
                vm.Description = Server.HtmlDecode(Request.Form["editor1"]);
                vm.CommentType = COMMENTTYPE.Description;
                UserRankViewModel.UpdateUserRank(updater, 2);
            }
            else
                vm.Description = "";

            vm.UpdateIssue();

            if (!string.IsNullOrEmpty(Request.Form["attachmentupload"]))
            {
                var urls = ReceiveRMAFiles();
                var internalreportfile = Request.Form["attachmentupload"];
                var originalname = Path.GetFileNameWithoutExtension(internalreportfile)
                    .Replace(" ", "_").Replace("#", "")
                    .Replace("&", "").Replace("?", "").Replace("%", "").Replace("+", "");

                var url = "";
                foreach (var r in urls)
                {
                    if (r.Contains(originalname))
                    {
                        url = r;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(url))
                {
                    IssueViewModels.StoreIssueAttachment(vm.IssueKey, url);
                    UserRankViewModel.UpdateUserRank(updater, 5);
                }
            }

            ProjectEvent.OperateIssueEvent(originaldata.ProjectKey, updater, "Updated", originaldata.Summary, originaldata.IssueKey);

            if (string.Compare(originaldata.Assignee, vm.Assignee, true) != 0)
            {
                vm.UpdateIAssign();
                ProjectEvent.AssignIssueEvent(originaldata.ProjectKey, updater, vm.Assignee, originaldata.Summary, originaldata.IssueKey);
                vm.Summary = originaldata.Summary;
                SendTaskEvent(vm, "asigned to you", updater, vm.Assignee);
            }

            if (string.Compare(originaldata.Resolution, vm.Resolution, true) != 0)
            {
                if (vm.IssueClosed())
                {
                    var realissue = IssueViewModels.RetrieveIssueByIssueKey(vm.IssueKey);
                    if (realissue.AttachList.Count > 0)
                    {
                        UserRankViewModel.UpdateUserRank(updater, 5);
                    }
                    else if (realissue.CommentList.Count > 1)
                    {
                        UserRankViewModel.UpdateUserRank(updater, 2);
                    }

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

            if (Request.Form["birootcauselist"] != null)
            {
                var biret = Request.Form["birootcauselist"].ToString();
                if (string.Compare(biret, "NONE") != 0)
                {
                    var sn = newdata.Summary.Split(new string[] { " " }, StringSplitOptions.None)[1].Trim().ToUpper();
                    IssueViewModels.StoreBIRootCause(newdata.ProjectKey, sn, biret);
                }
            }

            ViewBag.isassignee = false;
            if (string.Compare(updater, newdata.Assignee, true) == 0
                    || string.Compare(updater, newdata.Reporter, true) == 0)
            {
                ViewBag.isassignee = true;
            }

            CreateAllLists(newdata);

            if (newdata.Summary.Contains(" @Burn-In Step "))
            {
                var sn = newdata.Summary.Split(new string[] { " " }, StringSplitOptions.None)[1].Trim().ToUpper();
                ViewBag.birootcauselist = CreateBIRootIssue(newdata.ProjectKey, sn);
            }
            return View(newdata);

        }

        public ActionResult UpdateRMA(string issuekey)
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
                ck.Add("currentaction", "UpdateIssue");
                CookieUtility.SetCookie(this, ck);
                return RedirectToAction("LoginUser", "User");
            }

            var key = "";
            if (!string.IsNullOrEmpty(issuekey))
            {
                var ck = new Dictionary<string, string>();
                ck.Add("issuekey", issuekey);
                CookieUtility.SetCookie(this, ck);
                key = issuekey;
            }
            else if (ckdict.ContainsKey("issuekey") && !string.IsNullOrEmpty(ckdict["issuekey"]))
            {
                key = ckdict["issuekey"];
                var ck = new Dictionary<string, string>();
                ck.Add("currentaction", "UpdateIssue");
                CookieUtility.SetCookie(this, ck);
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
                var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

                ViewBag.isassignee = false;
                if (string.Compare(updater, ret.Assignee, true) == 0 
                    || string.Compare(updater,ret.Reporter,true) == 0)
                {
                    ViewBag.isassignee = true;
                }
                ret.Reporter = updater;

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

        private List<string> ReceiveRMAFiles()
        {
            var ret = new List<string>();

            try
            {
                foreach (string fl in Request.Files)
                {
                    if (fl != null && Request.Files[fl].ContentLength > 0)
                    {
                        string fn = Path.GetFileName(Request.Files[fl].FileName)
                            .Replace(" ", "_").Replace("#","")
                            .Replace("&", "").Replace("?", "").Replace("%", "").Replace("+", "");

                        string datestring = DateTime.Now.ToString("yyyyMMdd");
                        string imgdir = Server.MapPath("~/userfiles") + "\\docs\\" + datestring + "\\";

                        if (!Directory.Exists(imgdir))
                        {
                            Directory.CreateDirectory(imgdir);
                        }

                        fn = Path.GetFileNameWithoutExtension(fn) + "-" + DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(fn);
                        Request.Files[fl].SaveAs(imgdir + fn);

                        var url = "/userfiles/docs/" + datestring + "/" + fn;

                        ret.Add(url);
                    }
                }

            }
            catch (Exception ex)
            { return ret; }

            return ret;
        }


        [HttpPost, ActionName("UpdateRMA")]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateRMAPost()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
            var issuekey = Request.Form["IssueKey"];

            var originaldata = IssueViewModels.RetrieveIssueByIssueKey(issuekey);

            if (Request.Form["deleterma"] != null)
            {
                if (string.Compare(updater, originaldata.Reporter, true) == 0
                    || string.Compare(updater, originaldata.Assignee, true) == 0)
                {
                    IssueViewModels.RemoveIssue(issuekey);
                    var dict = new RouteValueDictionary();
                    dict.Add("ProjectKey", originaldata.ProjectKey);
                    return RedirectToAction("ProjectDetail", "Project",dict);
                }
            }

            var vm = new IssueViewModels();
            vm.ProjectKey = Request.Form["projectlist"].ToString();
            vm.IssueKey = issuekey;
            vm.IssueType = ISSUETP.RMA;

            vm.FinisarRMA = Request.Form["FRMANUM"];
            var tempfailurecode = Request.Form["RMAFailureCode"].ToString();
            if (string.Compare(tempfailurecode, "None", true) == 0)
            {
                vm.RMAFailureCode = "";
            }
            else
            {
                vm.RMAFailureCode = tempfailurecode;
            }
            
            vm.FVCode = Request.Form["FVCode"];
            vm.FinisarModel = Request.Form["FinisarModel"];
            vm.ECustomer = Request.Form["ECustomer"];
            vm.CRMANUM = Request.Form["CRMANUM"];
            vm.CReport = Request.Form["CReport"];
            vm.RelativePeoples = Request.Form["RPeopleAddr"];
            vm.ModuleSN = Request.Form["ModuleSN"];

            vm.Summary = "RMA " + vm.FinisarRMA + " for module " + vm.FinisarModel + " from " + vm.ECustomer + ": " + vm.CReport;
            if (vm.Summary.Length > 200)
            {
                vm.Summary = vm.Summary.Substring(0, 198);
            }

            vm.Priority = Request.Form["prioritylist"].ToString();
            vm.DueDate = DateTime.Parse(Request.Form["DueDate"]);
            vm.ReportDate = DateTime.Now;
            vm.Reporter = updater;
            if (string.Compare(originaldata.Reporter, updater, true) == 0
                || string.Compare(originaldata.Assignee, updater, true) == 0)
            {
                vm.Assignee = Request.Form["assigneelist"].ToString();
                vm.Resolution = Request.Form["resolutionlist"].ToString();
            }
            else
            {
                vm.Assignee = originaldata.Assignee;
                vm.Resolution = originaldata.Resolution;
            }

            if (!string.IsNullOrEmpty(Request.Form["editor1"]))
            {
                vm.Description = Server.HtmlDecode(Request.Form["editor1"]);
                vm.CommentType = COMMENTTYPE.Description;
                UserRankViewModel.UpdateUserRank(updater, 2);
            }
            else
                vm.Description = "";

            if (!string.IsNullOrEmpty(Request.Form["rootcauseeditor"]))
            {
                var rootcause = Server.HtmlDecode(Request.Form["rootcauseeditor"]);
                var dbstr = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(rootcause));
                var commenttype = COMMENTTYPE.RootCause;
                IssueViewModels.StoreIssueComment(vm.IssueKey, dbstr, vm.Reporter, commenttype);
                UserRankViewModel.UpdateUserRank(updater, 5);
            }

            var urls = ReceiveRMAFiles();

            if (!string.IsNullOrEmpty(Request.Form["attachmentupload"]))
            {
                var attachementfile = Request.Form["attachmentupload"];
                var originalname1 = Path.GetFileNameWithoutExtension(attachementfile)
                    .Replace(" ", "_").Replace("#", "")
                    .Replace("&", "").Replace("?", "").Replace("%", "").Replace("+", "");

                var url = "";
                foreach (var r in urls)
                {
                    if (r.Contains(originalname1))
                    {
                        url = r;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(url))
                {
                    IssueViewModels.StoreIssueAttachment(vm.IssueKey, url);

                    UserRankViewModel.UpdateUserRank(updater, 5);
                }
            }

            if (!string.IsNullOrEmpty(Request.Form["customreportupload"]))
            {
                var customereportfile = Request.Form["customreportupload"];
                var originalname = Path.GetFileNameWithoutExtension(customereportfile)
                    .Replace(" ", "_").Replace("#", "")
                    .Replace("&", "").Replace("?", "").Replace("%", "").Replace("+", "");

                var url = "";
                foreach (var r in urls)
                {
                    if (r.Contains(originalname))
                    {
                        url = r;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(url))
                {
                    var linkstr = url;
                    var dbstr = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(linkstr));
                    var commenttype = COMMENTTYPE.CustomReport;
                    IssueViewModels.StoreIssueComment(vm.IssueKey, dbstr, vm.Reporter, commenttype);
                    IssueViewModels.StoreIssueAttachment(vm.IssueKey, linkstr);

                    UserRankViewModel.UpdateUserRank(updater, 5);
                }
            }

            if (!string.IsNullOrEmpty(Request.Form["internalreportupload"]))
            {
                var internalreportfile = Request.Form["internalreportupload"];
                var originalname = Path.GetFileNameWithoutExtension(internalreportfile)
                    .Replace(" ", "_").Replace("#", "")
                    .Replace("&", "").Replace("?", "").Replace("%", "").Replace("+", "");

                var url = "";
                foreach (var r in urls)
                {
                    if (r.Contains(originalname))
                    {
                        url = r;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(url))
                {
                    var linkstr = url;
                    var dbstr = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(linkstr));
                    var commenttype = COMMENTTYPE.InternalReport;
                    IssueViewModels.StoreIssueComment(vm.IssueKey, dbstr, vm.Reporter, commenttype);
                    IssueViewModels.StoreIssueAttachment(vm.IssueKey, linkstr);

                    UserRankViewModel.UpdateUserRank(updater, 10);
                }
            }

            vm.UpdateRMA();

            ProjectEvent.OperateIssueEvent(originaldata.ProjectKey, updater, "Updated", originaldata.Summary, originaldata.IssueKey);

            if (string.Compare(originaldata.Assignee, vm.Assignee, true) != 0)
            {
                vm.UpdateIAssign();
                ProjectEvent.AssignIssueEvent(originaldata.ProjectKey, updater, vm.Assignee, originaldata.Summary, originaldata.IssueKey);
                vm.Summary = originaldata.Summary;
                SendTaskEvent(vm, "asigned to you", updater, vm.Assignee);
            }

            if (string.Compare(originaldata.Resolution, vm.Resolution, true) != 0)
            {
                if (vm.IssueClosed())
                {
                    UserRankViewModel.UpdateUserRank(updater, 5);

                    ProjectEvent.OperateIssueEvent(originaldata.ProjectKey, updater, "Closed", originaldata.Summary, originaldata.IssueKey);
                    vm.CloseIssue();
                    SendRMAEvent(vm, "closed",true);
                }

                if (string.Compare(vm.Resolution, Resolute.Working) == 0)
                {
                    ProjectEvent.OperateIssueEvent(originaldata.ProjectKey, updater, "Started", originaldata.Summary, originaldata.IssueKey);
                }

                if (string.Compare(vm.Resolution, Resolute.Reopen) == 0)
                {
                    ProjectEvent.OperateIssueEvent(originaldata.ProjectKey, updater, "Reopened", originaldata.Summary, originaldata.IssueKey);
                    SendRMAEvent(vm, "reopened",true);
                }
            }

            var newdata = IssueViewModels.RetrieveIssueByIssueKey(issuekey);
            CreateAllLists(newdata);

            ViewBag.isassignee = false;
            if (string.Compare(updater, newdata.Assignee, true) == 0
                    || string.Compare(updater, newdata.Reporter,true) == 0)
            {
                ViewBag.isassignee = true;
            }

            return View(newdata);
        }


        public ActionResult UpdateOBA(string issuekey)
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
                ck.Add("currentaction", "UpdateIssue");
                CookieUtility.SetCookie(this, ck);
                return RedirectToAction("LoginUser", "User");
            }

            var key = "";
            if (!string.IsNullOrEmpty(issuekey))
            {
                var ck = new Dictionary<string, string>();
                ck.Add("issuekey", issuekey);
                CookieUtility.SetCookie(this, ck);
                key = issuekey;
            }
            else if (ckdict.ContainsKey("issuekey") && !string.IsNullOrEmpty(ckdict["issuekey"]))
            {
                key = ckdict["issuekey"];
                var ck = new Dictionary<string, string>();
                ck.Add("currentaction", "UpdateIssue");
                CookieUtility.SetCookie(this, ck);
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
                var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

                ViewBag.isassignee = false;
                if (string.Compare(updater, ret.Assignee, true) == 0
                    || string.Compare(updater, ret.Reporter, true) == 0)
                {
                    ViewBag.isassignee = true;
                }
                ret.Reporter = updater;

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

        [HttpPost, ActionName("UpdateOBA")]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateOBAPost()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
            var issuekey = Request.Form["IssueKey"];

            var originaldata = IssueViewModels.RetrieveIssueByIssueKey(issuekey);

            if (Request.Form["deleterma"] != null)
            {
                if (string.Compare(updater, originaldata.Reporter, true) == 0
                    || string.Compare(updater, originaldata.Assignee, true) == 0)
                {
                    IssueViewModels.RemoveIssue(issuekey);
                    var dict = new RouteValueDictionary();
                    dict.Add("ProjectKey", originaldata.ProjectKey);
                    return RedirectToAction("ProjectDetail", "Project", dict);
                }
            }

            var vm = new IssueViewModels();
            vm.ProjectKey = Request.Form["projectlist"].ToString();
            vm.IssueKey = issuekey;
            vm.IssueType = ISSUETP.OBA;

            vm.FinisarDMR = Request.Form["FinisarDMR"];
            var tempdispose = Request.Form["dispositionlist"].ToString();
            if (string.Compare(tempdispose, "None", true) == 0)
            {
                vm.MaterialDisposition = "";
            }
            else
            {
                vm.MaterialDisposition = tempdispose;
            }

            vm.OBAFailureRate = Request.Form["FailureRate"];

            vm.ProductType = Request.Form["ProductType"];

            vm.FVCode = Request.Form["FVCode"];
            vm.RelativePeoples = Request.Form["RPeopleAddr"];
            vm.ModuleSN = Request.Form["ModuleSN"];
            vm.Summary = originaldata.Summary;

            vm.Priority = Request.Form["prioritylist"].ToString();
            vm.DueDate = DateTime.Parse(Request.Form["DueDate"]);
            vm.ReportDate = DateTime.Now;
            vm.Reporter = updater;

            if (string.Compare(originaldata.Reporter, updater, true) == 0
                || string.Compare(originaldata.Assignee, updater, true) == 0)
            {
                vm.Assignee = Request.Form["assigneelist"].ToString();
                vm.Resolution = Request.Form["resolutionlist"].ToString();
            }
            else
            {
                vm.Assignee = originaldata.Assignee;
                vm.Resolution = originaldata.Resolution;
            }

            if (!string.IsNullOrEmpty(Request.Form["editor1"]))
            {
                vm.Description = Server.HtmlDecode(Request.Form["editor1"]);
                vm.CommentType = COMMENTTYPE.Description;
                UserRankViewModel.UpdateUserRank(updater, 2);
            }
            else
                vm.Description = "";

            if (!string.IsNullOrEmpty(Request.Form["rootcauseeditor"]))
            {
                var rootcause = Server.HtmlDecode(Request.Form["rootcauseeditor"]);
                var dbstr = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(rootcause));
                var commenttype = COMMENTTYPE.RootCause;
                IssueViewModels.StoreIssueComment(vm.IssueKey, dbstr, vm.Reporter, commenttype);
                UserRankViewModel.UpdateUserRank(updater, 5);
            }

            var urls = ReceiveRMAFiles();

            if (!string.IsNullOrEmpty(Request.Form["attachmentupload"]))
            {
                var attachementfile = Request.Form["attachmentupload"];
                var originalname1 = Path.GetFileNameWithoutExtension(attachementfile)
                    .Replace(" ", "_").Replace("#", "")
                    .Replace("&", "").Replace("?", "").Replace("%", "").Replace("+", "");

                var url = "";
                foreach (var r in urls)
                {
                    if (r.Contains(originalname1))
                    {
                        url = r;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(url))
                {
                    IssueViewModels.StoreIssueAttachment(vm.IssueKey, url);

                    UserRankViewModel.UpdateUserRank(updater, 5);
                }
            }

            vm.UpdateOBA();

            ProjectEvent.OperateIssueEvent(originaldata.ProjectKey, updater, "Updated", originaldata.Summary, originaldata.IssueKey);

            if (string.Compare(originaldata.Assignee, vm.Assignee, true) != 0)
            {
                vm.UpdateIAssign();
                ProjectEvent.AssignIssueEvent(originaldata.ProjectKey, updater, vm.Assignee, originaldata.Summary, originaldata.IssueKey);
                vm.Summary = originaldata.Summary;
                SendTaskEvent(vm, "asigned to you", updater, vm.Assignee);
            }

            if (string.Compare(originaldata.Resolution, vm.Resolution, true) != 0)
            {
                if (vm.IssueClosed())
                {
                    UserRankViewModel.UpdateUserRank(updater, 5);

                    ProjectEvent.OperateIssueEvent(originaldata.ProjectKey, updater, "Closed", originaldata.Summary, originaldata.IssueKey);
                    vm.CloseIssue();
                    SendRMAEvent(vm, "closed", true);
                }

                if (string.Compare(vm.Resolution, Resolute.Working) == 0)
                {
                    ProjectEvent.OperateIssueEvent(originaldata.ProjectKey, updater, "Started", originaldata.Summary, originaldata.IssueKey);
                }

                if (string.Compare(vm.Resolution, Resolute.Reopen) == 0)
                {
                    ProjectEvent.OperateIssueEvent(originaldata.ProjectKey, updater, "Reopened", originaldata.Summary, originaldata.IssueKey);
                    SendRMAEvent(vm, "reopened", true);
                }
            }

            var newdata = IssueViewModels.RetrieveIssueByIssueKey(issuekey);
            CreateAllLists(newdata);

            ViewBag.isassignee = false;
            if (string.Compare(updater, newdata.Assignee, true) == 0
                    || string.Compare(updater, newdata.Reporter, true) == 0)
            {
                ViewBag.isassignee = true;
            }

            return View(newdata);
        }

        public ActionResult ShowRootCause(string issuekey)
        {
            if (!string.IsNullOrEmpty(issuekey))
            {
                var vm = IssueViewModels.RetrieveIssueByIssueKey(issuekey);
                return View(vm);
            }
            return View();
        }

        public ActionResult ShowContainmentAction(string issuekey)
        {
            if (!string.IsNullOrEmpty(issuekey))
            {
                var vm = IssueViewModels.RetrieveIssueByIssueKey(issuekey);
                return View(vm.ContainmentActions);
            }
            return View();
        }

        public ActionResult ShowCorrectiveAction(string issuekey)
        {
            if (!string.IsNullOrEmpty(issuekey))
            {
                var vm = IssueViewModels.RetrieveIssueByIssueKey(issuekey);
                return View(vm.CorrectiveActions);
            }
            return View();
        }

        [HttpPost, ActionName("SameAsIssue")]
        [ValidateAntiForgeryToken]
        public ActionResult SameAsIssue()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

            var targetissuekey = Request.Form["DoneIssueList"].ToString();
            var tobeissuekey = new List<string>();
            for (var i = 0; i < 600; i++)
            {
                if (Request.Form["check" + i] != null && string.Compare(Request.Form["check" + i],"true",true) == 0)
                {
                    tobeissuekey.Add(Request.Form["HIssueKey" + i]);
                }
            }

            var targetdata = IssueViewModels.RetrieveIssueByIssueKey(targetissuekey);
            foreach (var key in tobeissuekey)
            {
                var tobedata = IssueViewModels.RetrieveIssueByIssueKey(key);
                if (string.Compare(tobedata.Resolution, Resolute.Pending) != 0
                    && string.Compare(tobedata.Resolution, Resolute.Working) != 0
                    && string.Compare(tobedata.Resolution, Resolute.Reopen) != 0)
                {
                    continue;
                }

                if (string.Compare(tobedata.Assignee, updater, true) != 0)
                {
                    continue;
                }

                tobedata.Resolution = Resolute.Done;
                tobedata.Description = "<p>Issue Same As <a href=\"/Issue/UpdateIssue?issuekey=" + targetdata.IssueKey+"\">"+targetdata.Summary +"</a></p>";
                tobedata.UpdateIssue();
                tobedata.CloseIssue();
            }
            var pjdata = ProjectTestData.RetrieveProjectTestData(targetissuekey);
            if (pjdata.Count == 0)
            {
                pjdata = BITestData.RetrieveProjectTestDataByDataID(targetissuekey);
            }

            if (pjdata.Count > 0)
            {
                var dict = new RouteValueDictionary();
                dict.Add("ProjectKey", pjdata[0].ProjectKey);
                dict.Add("ErrAbbr", pjdata[0].ErrAbbr);
                return RedirectToAction("ProjectErrAbbr", "Project",dict);
            }
            else
            {
                return RedirectToAction("ViewAll", "Project");
            }

        }

        [HttpPost, ActionName("AsignIssues")]
        [ValidateAntiForgeryToken]
        public ActionResult AsignIssues()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

            var tobeassigee = Request.Form["AllUserList"].ToString();

            var tobeissuekey = new List<string>();
            for (var i = 0; i < 600; i++)
            {
                if (Request.Form["check" + i] != null && string.Compare(Request.Form["check" + i], "true", true) == 0)
                {
                    tobeissuekey.Add(Request.Form["HIssueKey" + i]);
                }
            }

            //var targetdata = IssueViewModels.RetrieveIssueByIssueKey(targetissuekey);
            foreach (var key in tobeissuekey)
            {
                var tobedata = IssueViewModels.RetrieveIssueByIssueKey(key);
                if (string.Compare(tobedata.Assignee, updater, true) != 0)
                {
                    continue;
                }
                tobedata.Assignee = tobeassigee;
                tobedata.UpdateIssue();
            }

            if (tobeissuekey.Count > 0)
            {
                var tempvm = IssueViewModels.RetrieveIssueByIssueKey(tobeissuekey[0]);
                var dict = new RouteValueDictionary();
                dict.Add("ProjectKey", tempvm.ProjectKey);
                return RedirectToAction("ProjectFA", "Project", dict);
            }
            else
            {
                return RedirectToAction("ViewAll", "Project");
            }
        }


        private List<string> AttachCond(string filename)
        {
            var ret = new List<string>();

            var tempstrs = filename.Split(new string[] { "-", "." }, StringSplitOptions.None);
            var tempdatestr = tempstrs[tempstrs.Length - 2].Substring(0, tempstrs[tempstrs.Length - 2].Length - 6);

            var originalname = filename.Split(new string[] { "-" + tempdatestr }, StringSplitOptions.None)[0];


            var url = "/userfiles/docs/" + tempdatestr + "/" + filename;
            var linkstr1 = "<p><a href=\"" + url + "\" target=\"_blank\">[Report 4 Customer] " + originalname + "</a></p>";
            var linkstr2 = "<p><a href=\"" + url + "\" target=\"_blank\">[Internal Report] " + originalname + "</a></p>";

            ret.Add(url);
            ret.Add(linkstr1);
            ret.Add(linkstr2);
            return ret;
        }

        public ActionResult DeleteAttachment(string issuekey, string filename)
        {
            if (!string.IsNullOrEmpty(issuekey) && !string.IsNullOrEmpty(filename))
            {
                var attachcond = AttachCond(filename);
                foreach (var item in attachcond)
                {
                    IssueViewModels.DeleteAttachment(issuekey, item);
                }

                foreach (var item in attachcond)
                {
                    IssueViewModels.DeleteComment(issuekey
                        , Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(item)));
                }

                var dict = new RouteValueDictionary();
                dict.Add("issuekey", issuekey);
                return RedirectToAction("UpdateIssue", "Issue", dict);
            }

            return RedirectToAction("ViewAll", "Project");
        }

        private List<string> PrepeareRMAReport(string ProjectKey, string StartDate, string EndDate)
        {
            var lines = new List<string>();
            var list1 = IssueViewModels.RetrieveIssueTypeByProjectKey(ProjectKey, StartDate, EndDate,ISSUETP.RMA);

            var line = "FINISAR RMA,SN,STATUS,CUSTOMER,FV,RMA FAILURE CODE,ROOT CAUSE,OWENER,OPEN ISSUE DATE,CUSTOMER RMA REASON,INTERNAL REPORT,CUSTOMER REPORT";
            lines.Add(line);

            foreach (var item in list1)
            {
                var rootcause = "";
                if (item.RootCauseCommentList.Count > 0)
                {
                    rootcause = item.RootCauseCommentList[item.RootCauseCommentList.Count - 1].Comment.Replace("\"", "");
                    rootcause = System.Text.RegularExpressions.Regex.Replace(rootcause, "<.*?>", string.Empty);
                }

                var routevalue = new RouteValueDictionary();
                routevalue.Add("issuekey", "ABC");
                string scheme = this.Url.RequestContext.HttpContext.Request.Url.Scheme;
                string validatestr = this.Url.Action("UpdateIssue", "Issue", routevalue, scheme);
                validatestr = validatestr.Split(new string[] { "/Issue" }, StringSplitOptions.None)[0];

                var netcomputername = "";
                try { netcomputername = System.Net.Dns.GetHostName(); }
                catch (Exception ex) { }
                validatestr = validatestr.Replace("/localhost/", "/" + netcomputername + "/");

                var internalreport = "";
                if (item.InternalReportCommentList.Count > 0)
                {
                    if (item.InternalReportCommentList[item.InternalReportCommentList.Count - 1].Comment.Contains("<a href"))
                    {
                        internalreport = item.InternalReportCommentList[item.InternalReportCommentList.Count - 1].Comment.Split(new string[] { "<a href=\"", "\" target=" }, StringSplitOptions.None)[1];
                     }
                    else
                    {
                        internalreport = item.InternalReportCommentList[item.InternalReportCommentList.Count - 1].Comment;
                    }
                    internalreport = validatestr + internalreport;
                }


                var customerreport = "";
                if (item.Report4CustomerCommentList.Count > 0)
                {
                    if (item.Report4CustomerCommentList[item.Report4CustomerCommentList.Count - 1].Comment.Contains("<a href"))
                    {
                        customerreport = item.Report4CustomerCommentList[item.Report4CustomerCommentList.Count - 1].Comment.Split(new string[] { "<a href=\"", "\" target=" }, StringSplitOptions.None)[1];
                    }
                    else
                    {
                        customerreport = item.Report4CustomerCommentList[item.Report4CustomerCommentList.Count - 1].Comment;
                    }
                    customerreport = validatestr + customerreport;
                }


                line = string.Empty;
                line = "\""+item.FinisarRMA.Replace("\"","")+"\","+ "\""+item.ModuleSN.Replace("\"", "") + "\"," + "\"" + item.Resolution.Replace("\"", "") + "\","
                    + "\"" + item.ECustomer.Replace("\"", "") + "\"," + "\"" + item.FVCode.Replace("\"", "") + "\","+ "\""+item.RMAFailureCode.Replace("\"", "") + "\"," + "\""+ rootcause.Trim() + "\"," 
                    + "\""+item.Assignee.Replace("\"", "")+ "\"," + "\""+item.ReportDate.ToString("yyyy-MM-dd hh:mm:ss") + "\"," + "\"" + item.CReport.Replace("\"", "") + "\","
                    + "\""+ internalreport + "\"," + "\""+ customerreport + "\",";

                lines.Add(line);
            }
      
            return lines;
        }

        public ActionResult ExportRMAData(string ProjectKey, string StartDate, string EndDate)
        {
            if (!string.IsNullOrEmpty(ProjectKey))
            {
                string datestring = DateTime.Now.ToString("yyyyMMdd");
                string imgdir = Server.MapPath("~/userfiles") + "\\docs\\" + datestring + "\\";
                if (!Directory.Exists(imgdir))
                {
                    Directory.CreateDirectory(imgdir);
                }

                var fn = ProjectKey+"_RMA_Report_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
                var filename = imgdir + fn;

                var lines = PrepeareRMAReport(ProjectKey, StartDate, EndDate);

                var wholefile = "";
                foreach (var l in lines)
                {
                    wholefile = wholefile + l+"\r\n";
                }
                System.IO.File.WriteAllText(filename, wholefile);

                return File(filename, "application/vnd.ms-excel", fn);
            }
            return RedirectToAction("ViewAll", "Project");
        }

        private List<string> PrepeareAllRMAReport(string StartDate, string EndDate)
        {
            var lines = new List<string>();
            var list1 = IssueViewModels.RetrieveAllIssueTypeIssue(StartDate, EndDate,ISSUETP.RMA);

            var line = "FINISAR RMA,PROJECT,SN,STATUS,CUSTOMER,FV,RMA FAILURE CODE,ROOT CAUSE,OWENER,OPEN ISSUE DATE,CUSTOMER RMA REASON,INTERNAL REPORT,CUSTOMER REPORT";
            lines.Add(line);

            foreach (var item in list1)
            {
                var rootcause = "";
                if (item.RootCauseCommentList.Count > 0)
                {
                    rootcause = item.RootCauseCommentList[item.RootCauseCommentList.Count - 1].Comment.Replace("\"", "");
                    rootcause = System.Text.RegularExpressions.Regex.Replace(rootcause, "<.*?>", string.Empty);
                    rootcause = rootcause.Replace("&nbsp;", "");
                }

                var routevalue = new RouteValueDictionary();
                routevalue.Add("issuekey", "ABC");
                string scheme = this.Url.RequestContext.HttpContext.Request.Url.Scheme;
                string validatestr = this.Url.Action("UpdateIssue", "Issue", routevalue, scheme);
                validatestr = validatestr.Split(new string[] { "/Issue" }, StringSplitOptions.None)[0];

                var netcomputername = "";
                try { netcomputername = System.Net.Dns.GetHostName(); }
                catch (Exception ex) { }
                validatestr = validatestr.Replace("/localhost/", "/" + netcomputername + "/");

                var internalreport = "";
                if (item.InternalReportCommentList.Count > 0)
                {
                    if (item.InternalReportCommentList[item.InternalReportCommentList.Count - 1].Comment.Contains("<a href"))
                    {
                        internalreport = item.InternalReportCommentList[item.InternalReportCommentList.Count - 1].Comment.Split(new string[] { "<a href=\"", "\" target=" }, StringSplitOptions.None)[1];
                    }
                    else
                    {
                        internalreport = item.InternalReportCommentList[item.InternalReportCommentList.Count - 1].Comment;
                    }
                    internalreport = validatestr + internalreport;
                }


                var customerreport = "";
                if (item.Report4CustomerCommentList.Count > 0)
                {
                    if (item.Report4CustomerCommentList[item.Report4CustomerCommentList.Count - 1].Comment.Contains("<a href"))
                    {
                        customerreport = item.Report4CustomerCommentList[item.Report4CustomerCommentList.Count - 1].Comment.Split(new string[] { "<a href=\"", "\" target=" }, StringSplitOptions.None)[1];
                    }
                    else
                    {
                        customerreport = item.Report4CustomerCommentList[item.Report4CustomerCommentList.Count - 1].Comment;
                    }
                    customerreport = validatestr + customerreport;
                }


                line = string.Empty;
                line = "\"" + item.FinisarRMA.Replace("\"", "") + "\"," + "\"" + item.ProjectKey.Replace("\"", "") + "\"," + "\"" + item.ModuleSN.Replace("\"", "") + "\"," + "\"" + item.Resolution.Replace("\"", "") + "\","
                    + "\"" + item.ECustomer.Replace("\"", "") + "\"," + "\"" + item.FVCode.Replace("\"", "") + "\"," + "\"" + item.RMAFailureCode.Replace("\"", "") + "\"," + "\"" + rootcause.Trim() + "\","
                    + "\"" + item.Assignee.Replace("\"", "") + "\"," + "\"" + item.ReportDate.ToString("yyyy-MM-dd hh:mm:ss") + "\"," + "\"" + item.CReport.Replace("\"", "") + "\","
                    + "\"" + internalreport + "\"," + "\"" + customerreport + "\",";

                lines.Add(line);
            }

            return lines;
        }

        public ActionResult ExportAllRMAData(string StartDate, string EndDate)
        {
            string datestring = DateTime.Now.ToString("yyyyMMdd");
            string imgdir = Server.MapPath("~/userfiles") + "\\docs\\" + datestring + "\\";
            if (!Directory.Exists(imgdir))
            {
                Directory.CreateDirectory(imgdir);
            }

            var fn = "NPI_RMA_Report_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
            var filename = imgdir + fn;

            var lines = PrepeareAllRMAReport(StartDate,EndDate);

            var wholefile = "";
            foreach (var l in lines)
            {
                wholefile = wholefile + l + "\r\n";
            }
            System.IO.File.WriteAllText(filename, wholefile);

            return File(filename, "application/vnd.ms-excel", fn);
        }

        private List<string> PrepeareOBAReport(string ProjectKey, string StartDate, string EndDate)
        {
            var lines = new List<string>();
            var list1 = IssueViewModels.RetrieveIssueTypeByProjectKey(ProjectKey, StartDate, EndDate, ISSUETP.OBA);

            var line = "NO,ISSUE DATE,DMR#,Product Type,Failure Rate,Affected SN,FA Owner,OBA Description,Priority,Product Type,FV Results,ROOT CAUSE,Containment Action,Corrective Action,Material Disposition,Resolution,Attachement";
            lines.Add(line);

            var idx = 0;

            foreach (var item in list1)
            {
                idx = idx + 1;

                var index = idx.ToString();

                var rootcause = "";
                if (item.RootCauseCommentList.Count > 0)
                {
                    rootcause = item.RootCauseCommentList[item.RootCauseCommentList.Count - 1].Comment.Replace("\"", "");
                    rootcause = System.Text.RegularExpressions.Regex.Replace(rootcause, "<.*?>", string.Empty);
                }


                var routevalue = new RouteValueDictionary();
                routevalue.Add("issuekey", "ABC");
                string scheme = this.Url.RequestContext.HttpContext.Request.Url.Scheme;
                string validatestr = this.Url.Action("UpdateIssue", "Issue", routevalue, scheme);
                validatestr = validatestr.Split(new string[] { "/Issue" }, StringSplitOptions.None)[0];

                var netcomputername = "";
                try { netcomputername = System.Net.Dns.GetHostName(); }
                catch (Exception ex) { }
                validatestr = validatestr.Replace("/localhost/", "/" + netcomputername + "/");


                var internalreport = "";
                foreach (var a in item.AttachList)
                {
                    var internalreport1 = "";
                    if (a.Contains("<a href"))
                    {
                        internalreport1 = a.Split(new string[] { "<a href=\"", "\" target=" }, StringSplitOptions.None)[1];
                    }
                    else
                    {
                        internalreport1 = a;
                    }
                    internalreport = internalreport+validatestr + internalreport1 + "||";
                }

                var issuedata = item.DueDate.AddDays(-6).ToString("MM/dd/yyyy");

                var containmentaction = "";
                foreach (var c in item.ContainmentActions)
                {
                    containmentaction = containmentaction + c.Summary
                        + ":" + c.Resolution + "//";
                }

                var correctiveaction = "";
                foreach (var c in item.CorrectiveActions)
                {
                    correctiveaction = c.Summary
                        + ":" + c.Resolution + "//";
                }


                line = string.Empty;
                line = "\"" + index + "\"," + "\"" + issuedata + "\"," + "\"" + item.FinisarDMR.Replace("\"", "") + "\","
                    + "\"" + item.ProjectKey + "\"," + "\"'" + item.OBAFailureRate.Replace("\"", "") + "\"," + "\"" + item.ModuleSN.Replace("\"", "") + "\"," 
                    + "\"" + item.Assignee.Replace("\"", "") + "\","+ "\"" + item.Summary.Replace("\"", "") + "\","
                    + "\"" + item.Priority.Replace("\"", "") + "\"," + "\"" + item.ProductType.Replace("\"", "") + "\","
                    + "\"" + item.FVCode.Replace("\"", "") + "\"," + "\"" + rootcause.Replace("\"", "").Trim() + "\","
                    + "\"" + containmentaction.Replace("\"", "") + "\"," + "\"" + correctiveaction.Replace("\"", "") + "\"," 
                    + "\"" + item.MaterialDisposition + "\"," + "\"" + item.Resolution + "\"," + "\"" + internalreport + "\","
                    + "\"http://wux-app1.china.ads.finisar.com/eDMR/DMR_Edit/DMR_View.asp?DMR_ID=" + item.FinisarDMR + "\",";

                lines.Add(line);
            }

            return lines;
        }

        public ActionResult ExportOBAData(string ProjectKey, string StartDate, string EndDate)
        {
            if (!string.IsNullOrEmpty(ProjectKey))
            {
                string datestring = DateTime.Now.ToString("yyyyMMdd");
                string imgdir = Server.MapPath("~/userfiles") + "\\docs\\" + datestring + "\\";
                if (!Directory.Exists(imgdir))
                {
                    Directory.CreateDirectory(imgdir);
                }

                var fn = ProjectKey + "_OBA_Report_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
                var filename = imgdir + fn;

                var lines = PrepeareOBAReport(ProjectKey, StartDate, EndDate);

                var wholefile = "";
                foreach (var l in lines)
                {
                    wholefile = wholefile + l + "\r\n";
                }
                System.IO.File.WriteAllText(filename, wholefile);

                return File(filename, "application/vnd.ms-excel", fn);
            }
            return RedirectToAction("ViewAll", "Project");
        }

        private List<string> PrepeareAllOBAReport(string StartDate, string EndDate)
        {
            var lines = new List<string>();
            var list1 = IssueViewModels.RetrieveAllIssueTypeIssue(StartDate, EndDate, ISSUETP.OBA);

            var line = "NO,ISSUE DATE,DMR#,Product Type,Failure Rate,Affected SN,FA Owner,OBA Description,Priority,Product Type,FV Results,ROOT CAUSE,Containment Action,Corrective Action,Material Disposition,Resolution,Attachement";
            lines.Add(line);

            var idx = 0;

            foreach (var item in list1)
            {
                idx = idx + 1;

                var index = idx.ToString();

                var rootcause = "";
                if (item.RootCauseCommentList.Count > 0)
                {
                    rootcause = item.RootCauseCommentList[item.RootCauseCommentList.Count - 1].Comment.Replace("\"", "");
                    rootcause = System.Text.RegularExpressions.Regex.Replace(rootcause, "<.*?>", string.Empty);
                }


                var routevalue = new RouteValueDictionary();
                routevalue.Add("issuekey", "ABC");
                string scheme = this.Url.RequestContext.HttpContext.Request.Url.Scheme;
                string validatestr = this.Url.Action("UpdateIssue", "Issue", routevalue, scheme);
                validatestr = validatestr.Split(new string[] { "/Issue" }, StringSplitOptions.None)[0];

                var netcomputername = "";
                try { netcomputername = System.Net.Dns.GetHostName(); }
                catch (Exception ex) { }
                validatestr = validatestr.Replace("/localhost/", "/" + netcomputername + "/");


                var internalreport = "";
                foreach (var a in item.AttachList)
                {
                    var internalreport1 = "";
                    if (a.Contains("<a href"))
                    {
                        internalreport1 = a.Split(new string[] { "<a href=\"", "\" target=" }, StringSplitOptions.None)[1];
                    }
                    else
                    {
                        internalreport1 = a;
                    }
                    internalreport = internalreport + validatestr + internalreport1 + "||";
                }

                var issuedata = item.DueDate.AddDays(-6).ToString("MM/dd/yyyy");

                var containmentaction = "";
                foreach (var c in item.ContainmentActions)
                {
                    containmentaction = containmentaction + c.Summary
                        + ":" + c.Resolution+"//";
                }

                var correctiveaction = "";
                foreach (var c in item.CorrectiveActions)
                {
                    correctiveaction = c.Summary
                        + ":" + c.Resolution + "//";
                }


                line = string.Empty;
                line = "\"" + index + "\"," + "\"" + issuedata + "\"," + "\"" + item.FinisarDMR.Replace("\"", "") + "\","
                    + "\"" + item.ProjectKey + "\"," + "\"'" + item.OBAFailureRate.Replace("\"", "") + "\"," + "\"" + item.ModuleSN.Replace("\"", "") + "\","
                    + "\"" + item.Assignee.Replace("\"", "") + "\"," + "\"" + item.Summary.Replace("\"", "") + "\","
                    + "\"" + item.Priority.Replace("\"", "") + "\"," + "\"" + item.ProductType.Replace("\"", "") + "\","
                    + "\"" + item.FVCode.Replace("\"", "") + "\"," + "\"" + rootcause.Replace("\"", "").Trim() + "\","
                    + "\"" + containmentaction.Replace("\"", "") + "\"," + "\"" + correctiveaction.Replace("\"", "") + "\","
                    + "\"" + item.MaterialDisposition + "\"," + "\"" + item.Resolution + "\"," + "\"" + internalreport + "\"," 
                    + "\"http://wux-app1.china.ads.finisar.com/eDMR/DMR_Edit/DMR_View.asp?DMR_ID=" + item.FinisarDMR + "\",";

                lines.Add(line);
            }

            return lines;
        }

        public ActionResult ExportAllOBAData(string StartDate, string EndDate)
        {
            string datestring = DateTime.Now.ToString("yyyyMMdd");
            string imgdir = Server.MapPath("~/userfiles") + "\\docs\\" + datestring + "\\";
            if (!Directory.Exists(imgdir))
            {
                Directory.CreateDirectory(imgdir);
            }

            var fn = "CQE_OBA_Report_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
            var filename = imgdir + fn;

            var lines = PrepeareAllOBAReport(StartDate, EndDate);

            var wholefile = "";
            foreach (var l in lines)
            {
                wholefile = wholefile + l + "\r\n";
            }
            System.IO.File.WriteAllText(filename, wholefile);

            return File(filename, "application/vnd.ms-excel", fn);
        }

    }
}