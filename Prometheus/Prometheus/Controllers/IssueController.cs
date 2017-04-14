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
            string[] mlist = { "None", "Scrap", "Rework", "UAI","Sorting","Purge" };
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

                ViewBag.tobechoosetags = ShareDocVM.RetrieveShareTags();

                var asilist = UserViewModels.RetrieveAllUser();
                ViewBag.towholist = CreateSelectList(asilist, "");

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
            validatestr = validatestr.Replace("//localhost", "//" + netcomputername);

            var content = vm.Summary + " is " + operate + " by " + vm.Reporter + " :\r\n " + validatestr;

                var toaddrs = new List<string>();
                toaddrs.Add(vm.Reporter);
                toaddrs.Add(vm.Assignee);
                if (vm.RelativePeopleList.Count > 0)
                {
                    toaddrs.AddRange(vm.RelativePeopleList);
                }

            var reporter = vm.Reporter.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries)[0].Replace(".", " ");
            EmailUtility.SendEmail(this,"WUXI NPI System_"+ reporter, toaddrs, content);
            new System.Threading.ManualResetEvent(false).WaitOne(300);
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
            validatestr = validatestr.Replace("//localhost", "//" + netcomputername);

            var content = vm.Summary + " is " + operate + " by " + vm.Reporter + " :\r\n " + validatestr;

            var toaddrs = new List<string>();
            toaddrs.Add(Reporter);
            toaddrs.Add(Assignee);
            if (vm.RelativePeopleList.Count > 0)
            {
                toaddrs.AddRange(vm.RelativePeopleList);
            }

            var report = Reporter.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries)[0].Replace(".", " ");
            EmailUtility.SendEmail(this,"WUXI NPI System_"+report, toaddrs, content);
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
                vm.Description = SeverHtmlDecode.Decode(this,Request.Form["editor1"]);
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
                    .Replace(" ", "_").Replace("#", "").Replace("'", "")
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

                    var attachtag = string.Empty;
                    for (var i = 0; i < 200; i++)
                    {
                        if (Request.Form["attachtagcheck" + i] != null)
                        {
                            attachtag = attachtag + Request.Form["attachtagcheck" + i] + ";";
                        }
                    }

                    if (!string.IsNullOrEmpty(attachtag))
                    {
                        var tempkeys = url.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                        var dockey = tempkeys[tempkeys.Length - 1];
                        ShareDocVM.ShareDoc(vm.ProjectKey, ShareDocType.DOCUMENT, dockey, attachtag, updater, DateTime.Now.ToString());
                    }
                }
            }


            //ProjectEvent.CreateIssueEvent(vm.ProjectKey, vm.Reporter, vm.Assignee, vm.Summary, vm.IssueKey);

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
                var asilist = UserViewModels.RetrieveAllUser();
                ViewBag.towholist = CreateSelectList(asilist, "");
                ViewBag.towholist1 = CreateSelectList(asilist, "");

                var tempvm = new IssueViewModels();
                CreateAllLists(tempvm);
                return View();
            }

            var ret = IssueViewModels.RetrieveIssueByIssueKey(key,this);

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

                if (string.Compare(ret.IssueType, ISSUETP.Quality) == 0)
                {
                    var dict = new RouteValueDictionary();
                    dict.Add("issuekey", key);
                    return RedirectToAction("UpdateQuality", "Issue", dict);
                }

                if (string.Compare(ret.IssueType, ISSUETP.Rel) == 0)
                {
                    var dict = new RouteValueDictionary();
                    dict.Add("issuekey", key);
                    return RedirectToAction("UpdateRel", "Issue", dict);
                }

                var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
                ViewBag.isassignee = false;
                if (string.Compare(updater, ret.Assignee, true) == 0
                        || string.Compare(updater, ret.Reporter,true) == 0
                        || string.Compare(updater, ret.Creator, true) == 0)
                {
                    ViewBag.isassignee = true;
                }

                ViewBag.authrized = true;
                if (string.Compare(ret.IssueType, ISSUETP.NPIPROC) == 0)
                {
                    ViewBag.authrized = false;
                }
                var pj = ProjectViewModels.RetrieveOneProject(ret.ProjectKey);
                foreach (var item in pj.MemberList)
                {
                    if (string.Compare(item.Name, updater, true) == 0)
                    {
                        ViewBag.authrized = true;
                        break;
                    }
                }

                //ret.Reporter = updater;
                CreateAllLists(ret);

                if (ret.Summary.Contains(" @Burn-In Step "))
                {
                    var sn = ret.Summary.Split(new string[] { " " }, StringSplitOptions.None)[1].Trim().ToUpper();
                    ViewBag.birootcauselist = CreateBIRootIssue(ret.ProjectKey, sn);
                }


                var asilist = UserViewModels.RetrieveAllUser();
                ViewBag.towholist = CreateSelectList(asilist, "");
                ViewBag.towholist1 = CreateSelectList(asilist, "");

                ViewBag.tobechoosetags = ShareDocVM.RetrieveShareTags();
                return View(ret);
            }
            else
            {
                var asilist = UserViewModels.RetrieveAllUser();
                ViewBag.towholist = CreateSelectList(asilist, "");
                ViewBag.towholist1 = CreateSelectList(asilist, "");

                var tempvm = new IssueViewModels();
                CreateAllLists(tempvm);
                return View();
            }
        }

        private void SendTaskCommentEmail(string issuekey,string summary,string commenter,List<string> towho,string commentcontent)
        {
            var routevalue = new RouteValueDictionary();
            routevalue.Add("issuekey", issuekey);
            //send validate email
            string scheme = this.Url.RequestContext.HttpContext.Request.Url.Scheme;
            string validatestr = this.Url.Action("UpdateIssue", "Issue", routevalue, scheme);

            var netcomputername = "";
            try { netcomputername = System.Net.Dns.GetHostName(); }
            catch (Exception ex) { }
            validatestr = validatestr.Replace("//localhost", "//" + netcomputername);
            var content = commenter + " add a new comment on issue: " + summary + "\r\n\r\n" + commentcontent + "\r\n\r\nISSUE LINK:\r\n\r\n"+ validatestr;
            EmailUtility.SendEmail(this, "WUXI NPI System_"+commenter, towho, content);
            new System.Threading.ManualResetEvent(false).WaitOne(50);
        }

        [HttpPost, ActionName("UpdateIssue")]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateIssuePost()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

            var issuekey = Request.Form["IssueKey"];
            var originaldata = IssueViewModels.RetrieveIssueByIssueKey(issuekey,this);

           if (Request.Form["deleteisu"] != null)
            {
                if (string.Compare(updater, originaldata.Reporter, true) == 0
                    || string.Compare(updater, originaldata.Assignee, true) == 0
                    || string.Compare(updater, originaldata.Creator, true) == 0)
                {
                    IssueViewModels.RemoveIssue(issuekey,this);
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
                || string.Compare(originaldata.Assignee, updater, true) == 0
                || string.Compare(originaldata.Creator, updater, true) == 0)
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
                vm.Description = SeverHtmlDecode.Decode(this,Request.Form["editor1"]);
                vm.CommentType = COMMENTTYPE.Description;

                if (string.Compare(updater.ToUpper(), originaldata.Assignee) != 0)
                {
                    var commenter = updater.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries)[0].Replace("."," ");
                    var towho = new List<string>();
                    towho.Add(vm.Assignee);
                    if (originaldata.Reporter.Contains("@"))
                        towho.Add(originaldata.Reporter);

                    var commentcontent = System.Text.RegularExpressions.Regex.Replace(vm.Description.Replace("\"", "").Replace("&nbsp;", ""), "<.*?>", string.Empty).Trim();
                    SendTaskCommentEmail(originaldata.IssueKey,originaldata.Summary,commenter,towho, commentcontent);
                }

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
                    .Replace(" ", "_").Replace("#", "").Replace("'", "")
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

                    var attachtag = string.Empty;
                    for (var i = 0; i < 200; i++)
                    {
                        if (Request.Form["attachtagcheck" + i] != null)
                        {
                            attachtag = attachtag + Request.Form["attachtagcheck" + i] + ";";
                        }
                    }
                    if (!string.IsNullOrEmpty(attachtag))
                    {
                        var tempkeys = url.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                        var dockey = tempkeys[tempkeys.Length - 1];
                        ShareDocVM.ShareDoc(originaldata.ProjectKey, ShareDocType.DOCUMENT, dockey, attachtag, updater, DateTime.Now.ToString());
                    }
                }
            }

            //ProjectEvent.OperateIssueEvent(originaldata.ProjectKey, updater, "Updated", originaldata.Summary, originaldata.IssueKey);

            if (string.Compare(originaldata.Assignee, vm.Assignee, true) != 0)
            {
                vm.UpdateIAssign();
                //ProjectEvent.AssignIssueEvent(originaldata.ProjectKey, updater, vm.Assignee, originaldata.Summary, originaldata.IssueKey);
                vm.Summary = originaldata.Summary;
                SendTaskEvent(vm, "asigned to you", updater, vm.Assignee);
            }

            if (string.Compare(originaldata.Resolution, vm.Resolution, true) != 0)
            {
                if (vm.IssueClosed())
                {
                    var realissue = IssueViewModels.RetrieveIssueByIssueKey(vm.IssueKey,this);
                    if (realissue.AttachList.Count > 0)
                    {
                        UserRankViewModel.UpdateUserRank(updater, 5);
                    }
                    else if (realissue.CommentList.Count > 1)
                    {
                        UserRankViewModel.UpdateUserRank(updater, 2);
                    }
                    
                    //ProjectEvent.OperateIssueEvent(originaldata.ProjectKey, updater, "Closed", originaldata.Summary, originaldata.IssueKey);
                    vm.CloseIssue();
                }

                if (string.Compare(vm.Resolution, Resolute.Working) == 0)
                {
                    //ProjectEvent.OperateIssueEvent(originaldata.ProjectKey, updater, "Started", originaldata.Summary, originaldata.IssueKey);
                }

                if (string.Compare(vm.Resolution, Resolute.Reopen) == 0)
                {
                    //ProjectEvent.OperateIssueEvent(originaldata.ProjectKey, updater, "Reopened", originaldata.Summary, originaldata.IssueKey);
                }
            }

            //var newdata = IssueViewModels.RetrieveIssueByIssueKey(issuekey);

            if (Request.Form["birootcauselist"] != null)
            {
                var biret = Request.Form["birootcauselist"].ToString();
                if (string.Compare(biret, "NONE") != 0)
                {
                    var sn = originaldata.Summary.Split(new string[] { " " }, StringSplitOptions.None)[1].Trim().ToUpper();
                    IssueViewModels.StoreBIRootCause(originaldata.ProjectKey, sn, biret);
                }
            }

            //ViewBag.isassignee = false;
            //if (string.Compare(updater, newdata.Assignee, true) == 0
            //        || string.Compare(updater, newdata.Reporter,true) == 0
            //        || string.Compare(updater, newdata.Creator, true) == 0)
            //{
            //    ViewBag.isassignee = true;
            //}

            //ViewBag.authrized = true;
            //if (string.Compare(newdata.IssueType, ISSUETP.NPIPROC) == 0)
            //{
            //    ViewBag.authrized = false;
            //}
            //var pj = ProjectViewModels.RetrieveOneProject(newdata.ProjectKey);
            //foreach (var item in pj.MemberList)
            //{
            //    if (string.Compare(item.Name, updater, true) == 0)
            //    {
            //        ViewBag.authrized = true;
            //        break;
            //    }
            //}

            //CreateAllLists(newdata);

            //if (newdata.Summary.Contains(" @Burn-In Step "))
            //{
            //    var sn = newdata.Summary.Split(new string[] { " " }, StringSplitOptions.None)[1].Trim().ToUpper();
            //    ViewBag.birootcauselist = CreateBIRootIssue(newdata.ProjectKey, sn);
            //}


            //ViewBag.tobechoosetags = ShareDocVM.RetrieveShareTags();
            //return View(newdata);

            var dict1 = new RouteValueDictionary();
            dict1.Add("issuekey", originaldata.IssueKey);
            return RedirectToAction("UpdateIssue", "Issue", dict1);
        }

        public ActionResult IPush(string IssueKey, string ToWho,string Reason)
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

            var tempreason = "";
            if (!string.IsNullOrEmpty(Reason))
            {
                try
                {
                    string dummyData = Reason.Trim().Replace(" ", "+");
                    if (dummyData.Length % 4 > 0)
                        dummyData = dummyData.PadRight(dummyData.Length + 4 - dummyData.Length % 4, '=');

                    var bytes = Convert.FromBase64String(dummyData);
                    tempreason = System.Text.Encoding.UTF8.GetString(bytes);
                }
                catch (Exception ex) { }
            }

            if (tempreason.Contains("WITHCOMMENT:"))
            {
                tempreason = tempreason.Replace("WITHCOMMENT:", "");
                var dbstr = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(tempreason));
                var commenttype = COMMENTTYPE.Description;
                IssueViewModels.StoreIssueComment(IssueKey, dbstr, updater, commenttype);

                tempreason = tempreason.Replace("<p>", "").Replace("</p>", "\r\n");
            }

            var issue = IssueViewModels.RetrieveIssueByIssueKey(IssueKey,this);
            ShareDocVM.ShareDoc(issue.ProjectKey, ShareDocType.ISSUE, issue.IssueKey, issue.Summary, updater, DateTime.Now.ToString());

            var whoes = ToWho.Split(new string[] { ";", ",", " " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var w in whoes)
            {
                ShareDocVM.IPushDoc(issue.ProjectKey, issue.IssueKey,w,updater,this, tempreason);
            }

            ShareDocVM.SendPushDocEvent("a new Issue about " + issue.Summary
                , "/Issue/UpdateIssue?issuekey="+ issue.IssueKey, updater, updater, this, tempreason);

            if (issue.Reporter.Contains("@"))
            {
                ShareDocVM.SendPushDocEvent("a new Issue about " + issue.Summary
                    , "/Issue/UpdateIssue?issuekey=" + issue.IssueKey, issue.Reporter, updater, this, tempreason);
            }

            var dict1 = new RouteValueDictionary();
            dict1.Add("issuekey", IssueKey);
            return RedirectToAction("UpdateIssue", "Issue", dict1);
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
            ViewBag.tobechoosetags = ShareDocVM.RetrieveShareTags();

            var asilist = UserViewModels.RetrieveAllUser();
            ViewBag.towholist = CreateSelectList(asilist, "");

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
            vm.Assignee = Request.Form["assigneelist"].ToString();
            vm.Reporter = updater; //Request.Form["reporterlist"].ToString();
            vm.Resolution = Request.Form["resolutionlist"].ToString();
            vm.ResolvedDate = DateTime.Parse("1982-05-06 01:01:01");
            vm.RelativePeoples = Request.Form["RPeopleAddr"];

            if (!string.IsNullOrEmpty(Request.Form["editor1"]))
            {
                vm.Description = SeverHtmlDecode.Decode(this,Request.Form["editor1"]);
                vm.CommentType = COMMENTTYPE.Description;
            }
            else
                vm.Description = "";

            vm.StoreSubIssue();

            SendTaskEvent(vm, "asigned to you", vm.Reporter, vm.Assignee);

            if (!string.IsNullOrEmpty(Request.Form["attachmentupload"]))
            {
                var urls = ReceiveRMAFiles();
                var internalreportfile = Request.Form["attachmentupload"];
                var originalname = Path.GetFileNameWithoutExtension(internalreportfile)
                    .Replace(" ", "_").Replace("#", "").Replace("'", "")
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

                    var attachtag = string.Empty;
                    for (var i = 0; i < 200; i++)
                    {
                        if (Request.Form["attachtagcheck" + i] != null)
                        {
                            attachtag = attachtag + Request.Form["attachtagcheck" + i] + ";";
                        }
                    }
                    if (!string.IsNullOrEmpty(attachtag))
                    {
                        var tempkeys = url.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                        var dockey = tempkeys[tempkeys.Length - 1];
                        ShareDocVM.ShareDoc(vm.ProjectKey, ShareDocType.DOCUMENT, dockey, attachtag, updater, DateTime.Now.ToString());
                    }
                }
            }

            //ProjectEvent.CreateIssueEvent(vm.ProjectKey, vm.Reporter, vm.Assignee, vm.Summary, vm.IssueKey);

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
                var vm = IssueViewModels.RetrieveIssueByIssueKey(issuekey,this);
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

                ViewBag.tobechoosetags = ShareDocVM.RetrieveShareTags();
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
                validatestr = validatestr.Replace("//localhost", "//" + netcomputername);

                var content = vm.Summary + " is " + operate + " by " + vm.Reporter + " :\r\n " + validatestr;

                var toaddrs = new List<string>();
                toaddrs.AddRange(vm.RelativePeopleList);
                toaddrs.Add(vm.Assignee);
                toaddrs.Add(vm.Reporter);

                var reporter = vm.Reporter.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries)[0].Replace(".", " ");
                EmailUtility.SendEmail(this,"WUXI NPI System_"+reporter, toaddrs, content);
                new System.Threading.ManualResetEvent(false).WaitOne(300);
            }
        }

        private void SendIssueEvent(IssueViewModels vm, string operate,string issuetype, bool nocheck = false)
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
                validatestr = validatestr.Replace("//localhost", "//" + netcomputername);

                var content = vm.Summary + " is " + operate + " by " + vm.Reporter + " :\r\n " + validatestr;

                var toaddrs = new List<string>();
                toaddrs.AddRange(vm.RelativePeopleList);
                toaddrs.Add(vm.Assignee);
                toaddrs.Add(vm.Reporter);

                var reporter = vm.Reporter.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries)[0].Replace(".", " ");
                EmailUtility.SendEmail(this,"WUXI NPI System_"+reporter, toaddrs, content);
                new System.Threading.ManualResetEvent(false).WaitOne(300);
            }
        }

        [HttpPost, ActionName("CreateRMA")]
        [ValidateAntiForgeryToken]
        public ActionResult CreateRMAPost()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

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
                vm.Description = SeverHtmlDecode.Decode(this,Request.Form["editor1"]);
                vm.CommentType = COMMENTTYPE.Description;
            }

            vm.StoreIssue();

            if (!string.IsNullOrEmpty(Request.Form["attachmentupload"]))
            {
                var urls = ReceiveRMAFiles();
                var internalreportfile = Request.Form["attachmentupload"];
                var originalname = Path.GetFileNameWithoutExtension(internalreportfile)
                    .Replace(" ", "_").Replace("#", "").Replace("'", "")
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

                    var attachtag = string.Empty;
                    for (var i = 0; i < 200; i++)
                    {
                        if (Request.Form["attachtagcheck" + i] != null)
                        {
                            attachtag = attachtag + Request.Form["attachtagcheck" + i] + ";";
                        }
                    }
                    if (!string.IsNullOrEmpty(attachtag))
                    {
                        var tempkeys = url.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                        var dockey = tempkeys[tempkeys.Length - 1];
                        ShareDocVM.ShareDoc(vm.ProjectKey, ShareDocType.DOCUMENT, dockey, attachtag, updater, DateTime.Now.ToString());
                    }
                }
            }

            //ProjectEvent.CreateIssueEvent(vm.ProjectKey, vm.Reporter, vm.Assignee, vm.Summary, vm.IssueKey);
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
                var asilist = UserViewModels.RetrieveAllUser();
                ViewBag.towholist = CreateSelectList(asilist, "");
                ViewBag.towholist1 = CreateSelectList(asilist, "");

                var tempvm = new IssueViewModels();
                CreateAllLists(tempvm);

                ViewBag.tobechoosetags = ShareDocVM.RetrieveShareTags();
                return View();
            }

            var ret = IssueViewModels.RetrieveIssueByIssueKey(key,this);

            if (ret != null)
            {
                var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
                ViewBag.isassignee = false;
                if (string.Compare(updater, ret.Assignee, true) == 0
                        || string.Compare(updater, ret.Reporter, true) == 0
                        || string.Compare(updater, ret.Creator, true) == 0)
                {
                    ViewBag.isassignee = true;
                }

                //ret.Reporter = updater;
                CreateAllLists(ret);

                if (ret.Summary.Contains(" @Burn-In Step "))
                {
                    var sn = ret.Summary.Split(new string[] { " " }, StringSplitOptions.None)[1].Trim().ToUpper();
                    ViewBag.birootcauselist = CreateBIRootIssue(ret.ProjectKey, sn);
                }

                var asilist = UserViewModels.RetrieveAllUser();
                ViewBag.towholist = CreateSelectList(asilist, "");
                ViewBag.towholist1 = CreateSelectList(asilist, "");

                ViewBag.tobechoosetags = ShareDocVM.RetrieveShareTags();
                return View(ret);
            }
            else
            {
                var asilist = UserViewModels.RetrieveAllUser();
                ViewBag.towholist = CreateSelectList(asilist, "");
                ViewBag.towholist1 = CreateSelectList(asilist, "");

                var tempvm = new IssueViewModels();
                CreateAllLists(tempvm);

                ViewBag.tobechoosetags = ShareDocVM.RetrieveShareTags();
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
            var originaldata = IssueViewModels.RetrieveIssueByIssueKey(issuekey,this);

            if (Request.Form["deleteisu"] != null)
            {
                if (string.Compare(updater, originaldata.Reporter, true) == 0
                    || string.Compare(updater, originaldata.Assignee, true) == 0
                    || string.Compare(updater, originaldata.Creator, true) == 0)
                {
                    IssueViewModels.RemoveIssue(issuekey,this);
                    var dict = new RouteValueDictionary();
                    dict.Add("ProjectKey", originaldata.ProjectKey);
                    return RedirectToAction("ProjectDetail", "Project", dict);
                }
            }


            if (Request.Form["linkisu"] != null)
            {
                if (string.Compare(updater, originaldata.Reporter, true) == 0
                    || string.Compare(updater, originaldata.Assignee, true) == 0)
                {
                    var errabbr = "";
                    if (!string.IsNullOrEmpty(originaldata.ErrAbbr))
                    {
                        errabbr = originaldata.ErrAbbr;
                    }
                    else
                    {
                        try {
                            errabbr = originaldata.Summary.Split(new string[] { "failed for", "@" }, StringSplitOptions.None)[1].Trim();
                        } catch (Exception ex) { errabbr = ""; }
                        
                    }

                    if (!string.IsNullOrEmpty(errabbr))
                    {
                        if (!originaldata.Summary.Contains("@Burn-In Step"))
                        {
                            var perrlist = ProjectErrorViewModels.RetrieveErrorByPJKey(originaldata.ProjectKey, errabbr,this);
                            var testdata = ProjectTestData.RetrieveProjectTestData(originaldata.IssueKey);

                            if (perrlist.Count > 0)
                            {
                                var linktime = DateTime.Now.ToString();

                                if (!string.IsNullOrEmpty(Request.Form["analysetitle"]))
                                {
                                    var com = new ErrorComments();
                                    com.Comment = Request.Form["analysetitle"];
                                    ProjectErrorViewModels.StoreErrorComment(perrlist[0].ErrorKey, com.dbComment, PJERRORCOMMENTTYPE.AnalyzeTitle, updater, linktime);
                                }

                                foreach (var item in originaldata.FailureDetailCommentList)
                                {
                                    var newcomment = new IssueComments();
                                    if (testdata.Count > 0)
                                    {
                                        newcomment.Comment = "<p>" + testdata[0].ModuleSerialNum + "  -  " + testdata[0].TestStation + "  -  " + testdata[0].WhichTest + "  -  " + testdata[0].TestTimeStamp.ToString() + "</p>"
                                            + "<p><a href='/Issue/UpdateIssue?issuekey="+originaldata.IssueKey+"' target='_blank'>"+originaldata.ModuleSN+" Issue Link"+"</a></p>"
                                            + "<p><a href='http://cn-traceview/?search=" + originaldata.ModuleSN+"_"+ testdata[0].WhichTest + "' target='_blank'>" + originaldata.ModuleSN + " TraceView Link" + "</a></p>"
                                                     + item.Comment;
                                    }
                                    else
                                    {
                                        newcomment.Comment = item.Comment;
                                    }
                                
                                    ProjectErrorViewModels.StoreErrorComment(perrlist[0].ErrorKey, newcomment.dbComment, PJERRORCOMMENTTYPE.FailureDetail,updater, linktime);
                                }

                                foreach (var item in originaldata.RootCauseCommentList)
                                {
                                    ProjectErrorViewModels.StoreErrorComment(perrlist[0].ErrorKey, item.dbComment, PJERRORCOMMENTTYPE.RootCause, updater, linktime);
                                }
                                if (originaldata.ResultCommentList.Count > 0)
                                {
                                    foreach (var item in originaldata.ResultCommentList)
                                    {
                                        ProjectErrorViewModels.StoreErrorComment(perrlist[0].ErrorKey, item.dbComment, PJERRORCOMMENTTYPE.Result, updater, linktime);
                                    }
                                }
                                else
                                {
                                    var newcomment = new IssueComments();
                                    newcomment.Comment = "<p>To Be Edit</p>";
                                    ProjectErrorViewModels.StoreErrorComment(perrlist[0].ErrorKey, newcomment.dbComment, PJERRORCOMMENTTYPE.Result, updater, linktime);
                                }

                                foreach (var item in originaldata.AttachList)
                                {
                                    ProjectErrorViewModels.StoreErrorAttachment(perrlist[0].ErrorKey, item);
                                }

                                UserRankViewModel.UpdateUserRank(updater, 10);

                                var dict = new RouteValueDictionary();
                                dict.Add("ErrorKey", perrlist[0].ErrorKey);
                                return RedirectToAction("UpdateProjectError", "Project", dict);
                            }//end if
                        }
                        else
                        {
                            var perrlist = ProjectErrorViewModels.RetrieveErrorByPJKey(ProjectErrorViewModels.BURNIN, errabbr,this);
                            var testdata = BITestData.RetrieveProjectTestDataByDataID(originaldata.IssueKey);
                            if (perrlist.Count > 0)
                            {
                                var linktime = DateTime.Now.ToString();
                                foreach (var item in originaldata.FailureDetailCommentList)
                                {
                                    var newcomment = new IssueComments();
                                    if (testdata.Count > 0)
                                    {
                                        newcomment.Comment = "<p>" + testdata[0].ModuleSerialNum + "  -  " + testdata[0].TestStation + "  -  " + testdata[0].WhichTest + "  -  " + testdata[0].TestTimeStamp.ToString() + "</p>"
                                            + "<p><a href='/Issue/UpdateIssue?issuekey=" + originaldata.IssueKey + "' target='_blank'>" + originaldata.ModuleSN + " link" + "</a></p>"
                                            + item.Comment;
                                    }
                                    else
                                    {
                                        newcomment.Comment = item.Comment;
                                    }

                                    ProjectErrorViewModels.StoreErrorComment(perrlist[0].ErrorKey, newcomment.dbComment, PJERRORCOMMENTTYPE.FailureDetail, updater, linktime);
                                }

                                foreach (var item in originaldata.RootCauseCommentList)
                                {
                                    ProjectErrorViewModels.StoreErrorComment(perrlist[0].ErrorKey, item.dbComment, PJERRORCOMMENTTYPE.RootCause, updater, linktime);
                                }

                                if (originaldata.ResultCommentList.Count > 0)
                                {
                                    foreach (var item in originaldata.ResultCommentList)
                                    {
                                        ProjectErrorViewModels.StoreErrorComment(perrlist[0].ErrorKey, item.dbComment, PJERRORCOMMENTTYPE.Result, updater, linktime);
                                    }
                                }
                                else
                                {
                                    var newcomment = new IssueComments();
                                    newcomment.Comment = "<p>To Be Edit</p>";
                                    ProjectErrorViewModels.StoreErrorComment(perrlist[0].ErrorKey, newcomment.dbComment, PJERRORCOMMENTTYPE.Result, updater, linktime);
                                }

                                foreach (var item in originaldata.AttachList)
                                {
                                    ProjectErrorViewModels.StoreErrorAttachment(perrlist[0].ErrorKey, item);
                                }

                                UserRankViewModel.UpdateUserRank(updater, 10);

                                var dict = new RouteValueDictionary();
                                dict.Add("ErrorKey", perrlist[0].ErrorKey);
                                return RedirectToAction("UpdateBIError", "BurnIn", dict);
                            }//end if
                        }


                    }//end if
                }
            }

            var vm = new IssueViewModels();
            vm.IssueKey = issuekey;
            vm.Reporter = updater;
            vm.IssueType = Request.Form["issuetypelist"].ToString();
            vm.Priority = Request.Form["prioritylist"].ToString();
            vm.DueDate = DateTime.Parse(Request.Form["DueDate"]);

            if (string.Compare(originaldata.Reporter, updater, true) == 0
                || string.Compare(originaldata.Assignee, updater, true) == 0
                || string.Compare(originaldata.Creator, updater, true) == 0)
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
                var failure = SeverHtmlDecode.Decode(this,Request.Form["failureeditor"]);
                var dbstr = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(failure));
                var commenttype = COMMENTTYPE.FailureDetail;
                IssueViewModels.StoreIssueComment(vm.IssueKey, dbstr, vm.Reporter, commenttype);
                UserRankViewModel.UpdateUserRank(updater, 2);
            }

            if (!string.IsNullOrEmpty(Request.Form["rootcauseeditor"]))
            {
                var rootcause = SeverHtmlDecode.Decode(this,Request.Form["rootcauseeditor"]);
                var dbstr = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(rootcause));
                var commenttype = COMMENTTYPE.RootCause;
                IssueViewModels.StoreIssueComment(vm.IssueKey, dbstr, vm.Reporter, commenttype);
                UserRankViewModel.UpdateUserRank(updater, 5);
            }

            if (!string.IsNullOrEmpty(Request.Form["resulteditor"]))
            {
                var rootcause = SeverHtmlDecode.Decode(this,Request.Form["resulteditor"]);
                var dbstr = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(rootcause));
                var commenttype = COMMENTTYPE.Result;
                IssueViewModels.StoreIssueComment(vm.IssueKey, dbstr, vm.Reporter, commenttype);
                UserRankViewModel.UpdateUserRank(updater,2);
            }

            if (!string.IsNullOrEmpty(Request.Form["editor1"]))
            {
                vm.Description = SeverHtmlDecode.Decode(this,Request.Form["editor1"]);
                vm.CommentType = COMMENTTYPE.Description;

                if (string.Compare(updater.ToUpper(), originaldata.Assignee) != 0)
                {
                    var commenter = updater.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries)[0].Replace(".", " ");
                    var towho = new List<string>();
                    towho.Add(vm.Assignee);
                    if (originaldata.Reporter.Contains("@"))
                        towho.Add(originaldata.Reporter);

                    var commentcontent = System.Text.RegularExpressions.Regex.Replace(vm.Description.Replace("\"", "").Replace("&nbsp;", ""), "<.*?>", string.Empty).Trim();
                    SendTaskCommentEmail(originaldata.IssueKey, originaldata.Summary, commenter, towho, commentcontent);
                }

                UserRankViewModel.UpdateUserRank(updater, 2);
            }
            else
                vm.Description = "";

            vm.UpdateIssue();

            var issuetag = string.Empty;
            for (var i = 0; i < 200; i++)
            {
                if (Request.Form["issuetagcheck" + i] != null)
                {
                    issuetag = issuetag + Request.Form["issuetagcheck" + i] + ";";
                }
            }


            if (!string.IsNullOrEmpty(Request.Form["attachmentupload"]))
            {
                var urls = ReceiveRMAFiles();
                var internalreportfile = Request.Form["attachmentupload"];
                var originalname = Path.GetFileNameWithoutExtension(internalreportfile)
                    .Replace(" ", "_").Replace("#", "").Replace("'", "")
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

                    var attachtag = string.Empty;
                    for (var i = 0; i < 200; i++)
                    {
                        if (Request.Form["attachtagcheck" + i] != null)
                        {
                            attachtag = attachtag + Request.Form["attachtagcheck" + i] + ";";
                        }
                    }
                    if (!string.IsNullOrEmpty(attachtag))
                    {
                        var tempkeys = url.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                        var dockey = tempkeys[tempkeys.Length - 1];
                        ShareDocVM.ShareDoc(originaldata.ProjectKey, ShareDocType.DOCUMENT, dockey, attachtag, updater, DateTime.Now.ToString());
                    }
                }
            }

            //ProjectEvent.OperateIssueEvent(originaldata.ProjectKey, updater, "Updated", originaldata.Summary, originaldata.IssueKey);

            if (string.Compare(originaldata.Assignee, vm.Assignee, true) != 0)
            {
                vm.UpdateIAssign();
                //ProjectEvent.AssignIssueEvent(originaldata.ProjectKey, updater, vm.Assignee, originaldata.Summary, originaldata.IssueKey);
                vm.Summary = originaldata.Summary;
                SendTaskEvent(vm, "asigned to you", updater, vm.Assignee);
            }

            if (string.Compare(originaldata.Resolution, vm.Resolution, true) != 0)
            {
                if (vm.IssueClosed())
                {
                    var realissue = IssueViewModels.RetrieveIssueByIssueKey(vm.IssueKey,this);
                    if (realissue.AttachList.Count > 0)
                    {
                        UserRankViewModel.UpdateUserRank(updater, 5);
                    }
                    else if (realissue.CommentList.Count > 1)
                    {
                        UserRankViewModel.UpdateUserRank(updater, 2);
                    }

                    //ProjectEvent.OperateIssueEvent(originaldata.ProjectKey, updater, "Closed", originaldata.Summary, originaldata.IssueKey);
                    vm.CloseIssue();

                    if (!string.IsNullOrEmpty(issuetag))
                    {
                        ShareDocVM.ShareDoc(originaldata.ProjectKey, ShareDocType.ISSUE, originaldata.IssueKey, issuetag, updater, DateTime.Now.ToString());
                    }
                }

                if (string.Compare(vm.Resolution, Resolute.Working) == 0)
                {
                    //ProjectEvent.OperateIssueEvent(originaldata.ProjectKey, updater, "Started", originaldata.Summary, originaldata.IssueKey);
                }

                if (string.Compare(vm.Resolution, Resolute.Reopen) == 0)
                {
                    //ProjectEvent.OperateIssueEvent(originaldata.ProjectKey, updater, "Reopened", originaldata.Summary, originaldata.IssueKey);
                }
            }

            //var newdata = IssueViewModels.RetrieveIssueByIssueKey(issuekey);

            if (Request.Form["birootcauselist"] != null)
            {
                var biret = Request.Form["birootcauselist"].ToString();
                if (string.Compare(biret, "NONE") != 0)
                {
                    var sn = originaldata.Summary.Split(new string[] { " " }, StringSplitOptions.None)[1].Trim().ToUpper();
                    IssueViewModels.StoreBIRootCause(originaldata.ProjectKey, sn, biret);
                }
            }

            //ViewBag.isassignee = false;
            //if (string.Compare(updater, newdata.Assignee, true) == 0
            //        || string.Compare(updater, newdata.Reporter, true) == 0
            //        || string.Compare(updater, newdata.Creator, true) == 0)
            //{
            //    ViewBag.isassignee = true;
            //}

            //CreateAllLists(newdata);

            //if (newdata.Summary.Contains(" @Burn-In Step "))
            //{
            //    var sn = newdata.Summary.Split(new string[] { " " }, StringSplitOptions.None)[1].Trim().ToUpper();
            //    ViewBag.birootcauselist = CreateBIRootIssue(newdata.ProjectKey, sn);
            //}

            //ViewBag.tobechoosetags = ShareDocVM.RetrieveShareTags();
            //return View(newdata);

            var dict1 = new RouteValueDictionary();
            dict1.Add("issuekey", originaldata.IssueKey);
            return RedirectToAction("UpdateBug", "Issue", dict1);
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

            var ret = IssueViewModels.RetrieveIssueByIssueKey(key,this);
            if (ret != null)
            {
                var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

                ViewBag.isassignee = false;
                if (string.Compare(updater, ret.Assignee, true) == 0 
                    || string.Compare(updater,ret.Reporter,true) == 0
                    || string.Compare(updater, ret.Creator, true) == 0)
                {
                    ViewBag.isassignee = true;
                }
                //ret.Reporter = updater;

                ViewBag.tobechoosetags = ShareDocVM.RetrieveShareTags();
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

            var originaldata = IssueViewModels.RetrieveIssueByIssueKey(issuekey,this);

            if (Request.Form["deleterma"] != null)
            {
                if (string.Compare(updater, originaldata.Reporter, true) == 0
                    || string.Compare(updater, originaldata.Assignee, true) == 0
                    || string.Compare(updater, originaldata.Creator, true) == 0)
                {
                    IssueViewModels.RemoveIssue(issuekey,this);
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
                || string.Compare(originaldata.Assignee, updater, true) == 0
                || string.Compare(originaldata.Creator, updater, true) == 0)
            {
                vm.Assignee = Request.Form["assigneelist"].ToString();
                vm.Resolution = Request.Form["resolutionlist"].ToString();
                vm.RelativePeoples = Request.Form["RPeopleAddr"];
            }
            else
            {
                vm.Assignee = originaldata.Assignee;
                vm.Resolution = originaldata.Resolution;
                vm.RelativePeoples = originaldata.RelativePeoples;
            }

            if (!string.IsNullOrEmpty(Request.Form["editor1"]))
            {
                vm.Description = SeverHtmlDecode.Decode(this,Request.Form["editor1"]);
                vm.CommentType = COMMENTTYPE.Description;
                UserRankViewModel.UpdateUserRank(updater, 2);
            }
            else
                vm.Description = "";

            if (!string.IsNullOrEmpty(Request.Form["rootcauseeditor"]))
            {
                var rootcause = SeverHtmlDecode.Decode(this,Request.Form["rootcauseeditor"]);
                var dbstr = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(rootcause));
                var commenttype = COMMENTTYPE.RootCause;
                IssueViewModels.StoreIssueComment(vm.IssueKey, dbstr, vm.Reporter, commenttype);
                UserRankViewModel.UpdateUserRank(updater, 5);
            }

            var issuetag = string.Empty;
            for (var i = 0; i < 200; i++)
            {
                if (Request.Form["issuetagcheck" + i] != null)
                {
                    issuetag = issuetag + Request.Form["issuetagcheck" + i] + ";";
                }
            }

            var customertag = string.Empty;
            for (var i = 0; i < 200; i++)
            {
                if (Request.Form["customertagcheck" + i] != null)
                {
                    customertag = customertag + Request.Form["customertagcheck" + i] + ";";
                }
            }

            var internaltag = string.Empty;
            for (var i = 0; i < 200; i++)
            {
                if (Request.Form["internaltagcheck" + i] != null)
                {
                    internaltag = internaltag + Request.Form["internaltagcheck" + i] + ";";
                }
            }

            var attachtag = string.Empty;
            for (var i = 0; i < 200; i++)
            {
                if (Request.Form["attachtagcheck" + i] != null)
                {
                    attachtag = attachtag + Request.Form["attachtagcheck" + i] + ";";
                }
            }

            var urls = ReceiveRMAFiles();

            if (!string.IsNullOrEmpty(Request.Form["attachmentupload"]))
            {
                var attachementfile = Request.Form["attachmentupload"];
                var originalname1 = Path.GetFileNameWithoutExtension(attachementfile)
                    .Replace(" ", "_").Replace("#", "").Replace("'", "")
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


                    if (!string.IsNullOrEmpty(attachtag))
                    {
                        var tempkeys = url.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                        var dockey = tempkeys[tempkeys.Length - 1];
                        ShareDocVM.ShareDoc(originaldata.ProjectKey, ShareDocType.DOCUMENT, dockey, attachtag, updater, DateTime.Now.ToString());
                    }
                    UserRankViewModel.UpdateUserRank(updater, 5);
                }
            }

            if (!string.IsNullOrEmpty(Request.Form["customreportupload"]))
            {
                var customereportfile = Request.Form["customreportupload"];
                var originalname = Path.GetFileNameWithoutExtension(customereportfile)
                    .Replace(" ", "_").Replace("#", "").Replace("'", "")
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
                    if (!string.IsNullOrEmpty(customertag))
                    {
                        var tempkeys = url.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                        var dockey = tempkeys[tempkeys.Length - 1];
                        ShareDocVM.ShareDoc(originaldata.ProjectKey, ShareDocType.DOCUMENT, dockey, customertag, updater, DateTime.Now.ToString());
                    }
                    UserRankViewModel.UpdateUserRank(updater, 5);
                }
            }

            if (!string.IsNullOrEmpty(Request.Form["internalreportupload"]))
            {
                var internalreportfile = Request.Form["internalreportupload"];
                var originalname = Path.GetFileNameWithoutExtension(internalreportfile)
                    .Replace(" ", "_").Replace("#", "").Replace("'", "")
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
                    if (!string.IsNullOrEmpty(internaltag))
                    {
                        var tempkeys = url.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                        var dockey = tempkeys[tempkeys.Length - 1];
                        ShareDocVM.ShareDoc(originaldata.ProjectKey, ShareDocType.DOCUMENT, dockey, internaltag, updater, DateTime.Now.ToString());
                    }
                    UserRankViewModel.UpdateUserRank(updater, 10);
                }
            }

            vm.UpdateRMA();

            //ProjectEvent.OperateIssueEvent(originaldata.ProjectKey, updater, "Updated", originaldata.Summary, originaldata.IssueKey);

            if (string.Compare(originaldata.Assignee, vm.Assignee, true) != 0)
            {
                vm.UpdateIAssign();
                //ProjectEvent.AssignIssueEvent(originaldata.ProjectKey, updater, vm.Assignee, originaldata.Summary, originaldata.IssueKey);
                vm.Summary = originaldata.Summary;
                SendTaskEvent(vm, "asigned to you", updater, vm.Assignee);
            }

            if (string.Compare(originaldata.Resolution, vm.Resolution, true) != 0)
            {
                if (vm.IssueClosed())
                {
                    UserRankViewModel.UpdateUserRank(updater, 5);
                    if (!string.IsNullOrEmpty(issuetag))
                    {
                        ShareDocVM.ShareDoc(originaldata.ProjectKey, ShareDocType.ISSUE, originaldata.IssueKey, issuetag, updater, DateTime.Now.ToString());
                    }
                    //ProjectEvent.OperateIssueEvent(originaldata.ProjectKey, updater, "Closed", originaldata.Summary, originaldata.IssueKey);
                    vm.CloseIssue();
                    SendRMAEvent(vm, "closed",true);
                }

                if (string.Compare(vm.Resolution, Resolute.Working) == 0)
                {
                    //ProjectEvent.OperateIssueEvent(originaldata.ProjectKey, updater, "Started", originaldata.Summary, originaldata.IssueKey);
                }

                if (string.Compare(vm.Resolution, Resolute.Reopen) == 0)
                {
                    //ProjectEvent.OperateIssueEvent(originaldata.ProjectKey, updater, "Reopened", originaldata.Summary, originaldata.IssueKey);
                    SendRMAEvent(vm, "reopened",true);
                }
            }

            //var newdata = IssueViewModels.RetrieveIssueByIssueKey(issuekey);
            //CreateAllLists(newdata);

            //ViewBag.isassignee = false;
            //if (string.Compare(updater, newdata.Assignee, true) == 0
            //        || string.Compare(updater, newdata.Reporter,true) == 0
            //        || string.Compare(updater, newdata.Creator, true) == 0)
            //{
            //    ViewBag.isassignee = true;
            //}

            //ViewBag.tobechoosetags = ShareDocVM.RetrieveShareTags();
            //return View(newdata);

            var dict1 = new RouteValueDictionary();
            dict1.Add("issuekey", originaldata.IssueKey);
            return RedirectToAction("UpdateRMA", "Issue", dict1);
        }

        public ActionResult UpdateRel(string issuekey)
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

            var ret = IssueViewModels.RetrieveIssueByIssueKey(key,this);
            if (ret != null)
            {
                var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

                ViewBag.isassignee = false;
                if (string.Compare(updater, ret.Assignee, true) == 0
                    || string.Compare(updater, ret.Reporter, true) == 0
                    || string.Compare(updater, ret.Creator, true) == 0)
                {
                    ViewBag.isassignee = true;
                }

                ViewBag.tobechoosetags = ShareDocVM.RetrieveShareTags();
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

        [HttpPost, ActionName("UpdateRel")]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateRelPost()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
            var issuekey = Request.Form["IssueKey"];

            var originaldata = IssueViewModels.RetrieveIssueByIssueKey(issuekey,this);

            if (Request.Form["deleterma"] != null)
            {
                if (string.Compare(updater, originaldata.Reporter, true) == 0
                    || string.Compare(updater, originaldata.Assignee, true) == 0
                    || string.Compare(updater, originaldata.Creator, true) == 0)
                {
                    IssueViewModels.RemoveIssue(issuekey,this);
                    var dict1 = new RouteValueDictionary();
                    dict1.Add("ProjectKey", originaldata.ProjectKey);
                    return RedirectToAction("ProjectDetail", "Project", dict1);
                }
            }

            if(!string.IsNullOrEmpty(Request.Form["LineCategory"]))
                originaldata.LineCategory = Request.Form["LineCategory"];

            if (!string.IsNullOrEmpty(Request.Form["ProductType"]))
                originaldata.ProductType = Request.Form["ProductType"];

            if (!string.IsNullOrEmpty(Request.Form["TestType"]))
                originaldata.TestType = Request.Form["TestType"];

            if (!string.IsNullOrEmpty(Request.Form["QualType"]))
                originaldata.QualType = Request.Form["QualType"];

            if (!string.IsNullOrEmpty(Request.Form["RequestID"]))
                originaldata.RequestID = Request.Form["RequestID"];

            originaldata.FVCode = Request.Form["FVCode"];
            
            originaldata.Priority = Request.Form["prioritylist"].ToString();
            originaldata.DueDate = DateTime.Parse(Request.Form["DueDate"]);

            var originalassignee = originaldata.Assignee;
            var originaldataresolution = originaldata.Resolution;
            if (string.Compare(originaldata.Reporter, updater, true) == 0
                || string.Compare(originaldata.Assignee, updater, true) == 0
                || string.Compare(originaldata.Creator, updater, true) == 0)
            {
                originaldata.Assignee = Request.Form["assigneelist"].ToString();
                originaldata.Resolution = Request.Form["resolutionlist"].ToString();
                originaldata.RelativePeoples = Request.Form["RPeopleAddr"];
            }

            if (!string.IsNullOrEmpty(Request.Form["editor1"]))
            {
                originaldata.Description = SeverHtmlDecode.Decode(this,Request.Form["editor1"]);
                originaldata.CommentType = COMMENTTYPE.Description;
                UserRankViewModel.UpdateUserRank(updater, 2);
            }
            else
                originaldata.Description = "";

            if (!string.IsNullOrEmpty(Request.Form["rootcauseeditor"]))
            {
                var rootcause = SeverHtmlDecode.Decode(this,Request.Form["rootcauseeditor"]);
                var dbstr = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(rootcause));
                var commenttype = COMMENTTYPE.RootCause;
                IssueViewModels.StoreIssueComment(originaldata.IssueKey, dbstr, updater, commenttype);
                UserRankViewModel.UpdateUserRank(updater, 5);
            }

            var issuetag = string.Empty;
            //for (var i = 0; i < 200; i++)
            //{
            //    if (Request.Form["issuetagcheck" + i] != null)
            //    {
            //        issuetag = issuetag + Request.Form["issuetagcheck" + i] + ";";
            //    }
            //}

            var attachtag = string.Empty;
            for (var i = 0; i < 200; i++)
            {
                if (Request.Form["attachtagcheck" + i] != null)
                {
                    attachtag = attachtag + Request.Form["attachtagcheck" + i] + ";";
                }
            }

            var urls = ReceiveRMAFiles();

            if (!string.IsNullOrEmpty(Request.Form["attachmentupload"]))
            {
                var attachementfile = Request.Form["attachmentupload"];
                var originalname1 = Path.GetFileNameWithoutExtension(attachementfile)
                    .Replace(" ", "_").Replace("#", "").Replace("'", "")
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
                    IssueViewModels.StoreIssueAttachment(originaldata.IssueKey, url);
                    if (!string.IsNullOrEmpty(attachtag))
                    {
                        var tempkeys = url.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                        var dockey = tempkeys[tempkeys.Length - 1];
                        ShareDocVM.ShareDoc(originaldata.ProjectKey, ShareDocType.DOCUMENT, dockey, attachtag, updater, DateTime.Now.ToString());
                    }
                    UserRankViewModel.UpdateUserRank(updater, 5);
                }
            }


            originaldata.UpdateRel();

            if (string.Compare(originaldata.Assignee, originalassignee, true) != 0)
            {
                originaldata.UpdateIAssign();
                SendTaskEvent(originaldata, "asigned to you", updater, originaldata.Assignee);
            }

            if (string.Compare(originaldataresolution, originaldata.Resolution, true) != 0)
            {
                if (originaldata.IssueClosed())
                {
                    UserRankViewModel.UpdateUserRank(updater, 5);
                    if (!string.IsNullOrEmpty(issuetag))
                    {
                        ShareDocVM.ShareDoc(originaldata.ProjectKey, ShareDocType.ISSUE, originaldata.IssueKey, issuetag, updater, DateTime.Now.ToString());
                    }
                    //ProjectEvent.OperateIssueEvent(originaldata.ProjectKey, updater, "Closed", originaldata.Summary, originaldata.IssueKey);
                    originaldata.CloseIssue();
                    SendRMAEvent(originaldata, "closed", true);
                }

                if (string.Compare(originaldata.Resolution, Resolute.Reopen) == 0)
                {
                    //ProjectEvent.OperateIssueEvent(originaldata.ProjectKey, updater, "Reopened", originaldata.Summary, originaldata.IssueKey);
                    SendRMAEvent(originaldata, "reopened", true);
                }
            }

            var dict = new RouteValueDictionary();
            dict.Add("issuekey", originaldata.IssueKey);
            return RedirectToAction("UpdateRel", "Issue", dict);
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

            var ret = IssueViewModels.RetrieveIssueByIssueKey(key,this);
            if (ret != null)
            {
                var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

                ViewBag.isassignee = false;
                if (string.Compare(updater, ret.Assignee, true) == 0
                    || string.Compare(updater, ret.Reporter, true) == 0
                    || string.Compare(updater, ret.Creator, true) == 0)
                {
                    ViewBag.isassignee = true;
                }
                //ret.Reporter = updater;

                ViewBag.tobechoosetags = ShareDocVM.RetrieveShareTags();
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

            var originaldata = IssueViewModels.RetrieveIssueByIssueKey(issuekey,this);

            if (Request.Form["deleterma"] != null)
            {
                if (string.Compare(updater, originaldata.Reporter, true) == 0
                    || string.Compare(updater, originaldata.Assignee, true) == 0
                    || string.Compare(updater, originaldata.Creator, true) == 0)
                {
                    IssueViewModels.RemoveIssue(issuekey,this);
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
                || string.Compare(originaldata.Assignee, updater, true) == 0
                || string.Compare(originaldata.Creator, updater, true) == 0)
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
                vm.Description = SeverHtmlDecode.Decode(this,Request.Form["editor1"]);
                vm.CommentType = COMMENTTYPE.Description;
                UserRankViewModel.UpdateUserRank(updater, 2);
            }
            else
                vm.Description = "";

            if (!string.IsNullOrEmpty(Request.Form["rootcauseeditor"]))
            {
                var rootcause = SeverHtmlDecode.Decode(this,Request.Form["rootcauseeditor"]);
                var dbstr = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(rootcause));
                var commenttype = COMMENTTYPE.RootCause;
                IssueViewModels.StoreIssueComment(vm.IssueKey, dbstr, vm.Reporter, commenttype);
                UserRankViewModel.UpdateUserRank(updater, 5);
            }


            var issuetag = string.Empty;
            //for (var i = 0; i < 200; i++)
            //{
            //    if (Request.Form["issuetagcheck" + i] != null)
            //    {
            //        issuetag = issuetag + Request.Form["issuetagcheck" + i] + ";";
            //    }
            //}

            var attachtag = string.Empty;
            for (var i = 0; i < 200; i++)
            {
                if (Request.Form["attachtagcheck" + i] != null)
                {
                    attachtag = attachtag + Request.Form["attachtagcheck" + i] + ";";
                }
            }

            var urls = ReceiveRMAFiles();

            if (!string.IsNullOrEmpty(Request.Form["attachmentupload"]))
            {
                var attachementfile = Request.Form["attachmentupload"];
                var originalname1 = Path.GetFileNameWithoutExtension(attachementfile)
                    .Replace(" ", "_").Replace("#", "").Replace("'", "")
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
                    UserRankViewModel.UpdateUserRank(updater, 3);
                    if (!string.IsNullOrEmpty(attachtag))
                    {
                        var tempkeys = url.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                        var dockey = tempkeys[tempkeys.Length - 1];
                        ShareDocVM.ShareDoc(originaldata.ProjectKey, ShareDocType.DOCUMENT, dockey, attachtag, updater, DateTime.Now.ToString());
                    }
                }
            }

            vm.UpdateOBA();

            //ProjectEvent.OperateIssueEvent(originaldata.ProjectKey, updater, "Updated", originaldata.Summary, originaldata.IssueKey);

            if (string.Compare(originaldata.Assignee, vm.Assignee, true) != 0)
            {
                vm.UpdateIAssign();
                //ProjectEvent.AssignIssueEvent(originaldata.ProjectKey, updater, vm.Assignee, originaldata.Summary, originaldata.IssueKey);
                vm.Summary = originaldata.Summary;
                SendTaskEvent(vm, "asigned to you", updater, vm.Assignee);
            }

            if (string.Compare(originaldata.Resolution, vm.Resolution, true) != 0)
            {
                if (vm.IssueClosed())
                {
                    UserRankViewModel.UpdateUserRank(updater, 5);

                    if (!string.IsNullOrEmpty(issuetag))
                    {
                        ShareDocVM.ShareDoc(originaldata.ProjectKey, ShareDocType.ISSUE, originaldata.IssueKey, issuetag, updater, DateTime.Now.ToString());
                    }
                    //ProjectEvent.OperateIssueEvent(originaldata.ProjectKey, updater, "Closed", originaldata.Summary, originaldata.IssueKey);
                    vm.CloseIssue();
                    SendIssueEvent(vm, "closed",ISSUETP.OBA, true);
                }

                if (string.Compare(vm.Resolution, Resolute.Working) == 0)
                {
                    //ProjectEvent.OperateIssueEvent(originaldata.ProjectKey, updater, "Started", originaldata.Summary, originaldata.IssueKey);
                }

                if (string.Compare(vm.Resolution, Resolute.Reopen) == 0)
                {
                    //ProjectEvent.OperateIssueEvent(originaldata.ProjectKey, updater, "Reopened", originaldata.Summary, originaldata.IssueKey);
                    SendIssueEvent(vm, "reopened", ISSUETP.OBA, true);
                }
            }

            //var newdata = IssueViewModels.RetrieveIssueByIssueKey(issuekey);
            //CreateAllLists(newdata);

            //ViewBag.isassignee = false;
            //if (string.Compare(updater, newdata.Assignee, true) == 0
            //        || string.Compare(updater, newdata.Reporter, true) == 0
            //        || string.Compare(updater, newdata.Creator, true) == 0)
            //{
            //    ViewBag.isassignee = true;
            //}
            //ViewBag.tobechoosetags = ShareDocVM.RetrieveShareTags();
            //return View(newdata);

            var dict1 = new RouteValueDictionary();
            dict1.Add("issuekey", originaldata.IssueKey);
            return RedirectToAction("UpdateOBA", "Issue", dict1);
        }

        public ActionResult UpdateQuality(string issuekey)
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

            var ret = IssueViewModels.RetrieveIssueByIssueKey(key,this);
            if (ret != null)
            {
                var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

                ViewBag.isassignee = false;
                if (string.Compare(updater, ret.Assignee, true) == 0
                    || string.Compare(updater, ret.Reporter, true) == 0
                    || string.Compare(updater, ret.Creator, true) == 0)
                {
                    ViewBag.isassignee = true;
                }
                //ret.Reporter = updater;

                CreateAllLists(ret);
                ViewBag.tobechoosetags = ShareDocVM.RetrieveShareTags();

                return View(ret);
            }
            else
            {
                var tempvm = new IssueViewModels();
                CreateAllLists(tempvm);
                return View();
            }
        }

        [HttpPost, ActionName("UpdateQuality")]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateQualityPost()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
            var issuekey = Request.Form["IssueKey"];

            var originaldata = IssueViewModels.RetrieveIssueByIssueKey(issuekey,this);

            if (Request.Form["deleterma"] != null)
            {
                if (string.Compare(updater, originaldata.Reporter, true) == 0
                    || string.Compare(updater, originaldata.Assignee, true) == 0
                    || string.Compare(updater, originaldata.Creator, true) == 0)
                {
                    IssueViewModels.RemoveIssue(issuekey,this);
                    var dict = new RouteValueDictionary();
                    dict.Add("ProjectKey", originaldata.ProjectKey);
                    return RedirectToAction("ProjectDetail", "Project", dict);
                }
            }

            var vm = new IssueViewModels();
            vm.ProjectKey = Request.Form["projectlist"].ToString();
            vm.IssueKey = issuekey;
            vm.IssueType = ISSUETP.Quality;

            vm.RMAFailureCode = Request.Form["FailureMode"];

            var tempdispose = Request.Form["dispositionlist"].ToString();
            if (string.Compare(tempdispose, "None", true) == 0)
            {
                vm.MaterialDisposition = "";
            }
            else
            {
                vm.MaterialDisposition = tempdispose;
            }

            vm.AffectRange = Request.Form["AffectRange"];

            vm.ProductType = Request.Form["ProductType"];

            vm.RelativePeoples = Request.Form["RPeopleAddr"];

            vm.ModuleSN = originaldata.ModuleSN;
            vm.Summary = originaldata.Summary;

            vm.Priority = Request.Form["prioritylist"].ToString();
            vm.DueDate = DateTime.Parse(Request.Form["DueDate"]);
            vm.ReportDate = DateTime.Now;
            vm.Reporter = updater;

            if (string.Compare(originaldata.Reporter, updater, true) == 0
                || string.Compare(originaldata.Assignee, updater, true) == 0
                || string.Compare(originaldata.Creator, updater, true) == 0)
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
                vm.Description = SeverHtmlDecode.Decode(this,Request.Form["editor1"]);
                vm.CommentType = COMMENTTYPE.Description;
                UserRankViewModel.UpdateUserRank(updater, 2);
            }
            else
                vm.Description = "";

            if (!string.IsNullOrEmpty(Request.Form["analysiseditor"]))
            {
                var analysis = SeverHtmlDecode.Decode(this,Request.Form["analysiseditor"]);
                var dbstr = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(analysis));
                var commenttype = COMMENTTYPE.Analysis;
                IssueViewModels.StoreIssueComment(vm.IssueKey, dbstr, vm.Reporter, commenttype);
                UserRankViewModel.UpdateUserRank(updater, 3);
            }

            if (!string.IsNullOrEmpty(Request.Form["rootcauseeditor"]))
            {
                var rootcause = SeverHtmlDecode.Decode(this,Request.Form["rootcauseeditor"]);
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
                    .Replace(" ", "_").Replace("#", "").Replace("'", "")
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

                    UserRankViewModel.UpdateUserRank(updater, 3);

                    var attachtag = string.Empty;
                    for (var i = 0; i < 200; i++)
                    {
                        if (Request.Form["attachtagcheck" + i] != null)
                        {
                            attachtag = attachtag + Request.Form["attachtagcheck" + i] + ";";
                        }
                    }
                    if (!string.IsNullOrEmpty(attachtag))
                    {
                        var tempkeys = url.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                        var dockey = tempkeys[tempkeys.Length - 1];
                        ShareDocVM.ShareDoc(originaldata.ProjectKey, ShareDocType.DOCUMENT, dockey, attachtag, updater, DateTime.Now.ToString());
                    }
                }
            }

            vm.UpdateQuality();

            //ProjectEvent.OperateIssueEvent(originaldata.ProjectKey, updater, "Updated", originaldata.Summary, originaldata.IssueKey);

            if (string.Compare(originaldata.Assignee, vm.Assignee, true) != 0)
            {
                vm.UpdateIAssign();
                //ProjectEvent.AssignIssueEvent(originaldata.ProjectKey, updater, vm.Assignee, originaldata.Summary, originaldata.IssueKey);
                vm.Summary = originaldata.Summary;
                SendTaskEvent(vm, "asigned to you", updater, vm.Assignee);
            }

            if (string.Compare(originaldata.Resolution, vm.Resolution, true) != 0)
            {
                if (vm.IssueClosed())
                {
                    UserRankViewModel.UpdateUserRank(updater, 5);

                    //ProjectEvent.OperateIssueEvent(originaldata.ProjectKey, updater, "Closed", originaldata.Summary, originaldata.IssueKey);
                    vm.CloseIssue();
                    SendIssueEvent(vm, "closed",ISSUETP.Quality, true);
                }

                if (string.Compare(vm.Resolution, Resolute.Working) == 0)
                {
                    //ProjectEvent.OperateIssueEvent(originaldata.ProjectKey, updater, "Started", originaldata.Summary, originaldata.IssueKey);
                }

                if (string.Compare(vm.Resolution, Resolute.Reopen) == 0)
                {
                    //ProjectEvent.OperateIssueEvent(originaldata.ProjectKey, updater, "Reopened", originaldata.Summary, originaldata.IssueKey);
                    SendIssueEvent(vm, "reopened", ISSUETP.Quality, true);
                }
            }

            //var newdata = IssueViewModels.RetrieveIssueByIssueKey(issuekey);
            //CreateAllLists(newdata);

            //ViewBag.isassignee = false;
            //if (string.Compare(updater, newdata.Assignee, true) == 0
            //        || string.Compare(updater, newdata.Reporter, true) == 0
            //        || string.Compare(updater, newdata.Creator, true) == 0)
            //{
            //    ViewBag.isassignee = true;
            //}

            //return View(newdata);

            var dict1 = new RouteValueDictionary();
            dict1.Add("issuekey", originaldata.IssueKey);
            return RedirectToAction("UpdateQuality", "Issue", dict1);
        }

        public ActionResult ShowRootCause(string issuekey)
        {
            if (!string.IsNullOrEmpty(issuekey))
            {
                var vm = IssueViewModels.RetrieveIssueByIssueKey(issuekey,this);
                return View(vm);
            }
            return View();
        }

        public ActionResult ShowAnalysis(string issuekey)
        {
            if (!string.IsNullOrEmpty(issuekey))
            {
                var vm = IssueViewModels.RetrieveIssueByIssueKey(issuekey,this);
                return View(vm);
            }
            return View();
        }

        public ActionResult ShowContainmentAction(string issuekey)
        {
            if (!string.IsNullOrEmpty(issuekey))
            {
                var vm = IssueViewModels.RetrieveIssueByIssueKey(issuekey,this);
                return View(vm.ContainmentActions);
            }
            return View();
        }

        public ActionResult ShowCorrectiveAction(string issuekey)
        {
            if (!string.IsNullOrEmpty(issuekey))
            {
                var vm = IssueViewModels.RetrieveIssueByIssueKey(issuekey,this);
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

            var targetdata = IssueViewModels.RetrieveIssueByIssueKey(targetissuekey,this);
            foreach (var key in tobeissuekey)
            {
                var tobedata = IssueViewModels.RetrieveIssueByIssueKey(key,this);
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
                var tobedata = IssueViewModels.RetrieveIssueByIssueKey(key,this);
                if (string.Compare(tobedata.Assignee, updater, true) != 0)
                {
                    continue;
                }
                tobedata.Assignee = tobeassigee;
                tobedata.UpdateIssue();
            }

            if (tobeissuekey.Count > 0)
            {
                var tempvm = IssueViewModels.RetrieveIssueByIssueKey(tobeissuekey[0],this);
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
                    IssueViewModels.DeleteAttachComment(issuekey
                        , Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(item)));
                }

                var dict = new RouteValueDictionary();
                dict.Add("issuekey", issuekey);
                return RedirectToAction("UpdateIssue", "Issue", dict);
            }

            return RedirectToAction("ViewAll", "Project");
        }

        public ActionResult DeleteSPIssueComment(string IssueKey,string CommentType, string Date)
        {
            if (!string.IsNullOrEmpty(IssueKey) && !string.IsNullOrEmpty(CommentType) && !string.IsNullOrEmpty(Date))
            {
                IssueViewModels.DeleteSPComment(IssueKey, CommentType, Date);

                var dict = new RouteValueDictionary();
                dict.Add("issuekey", IssueKey);
                return RedirectToAction("UpdateIssue", "Issue", dict);
            }

            return RedirectToAction("ViewAll", "Project");
        }

        public ActionResult ModifyIssueComment(string IssueKey, string CommentType, string Date)
        {
            if (!string.IsNullOrEmpty(IssueKey) && !string.IsNullOrEmpty(CommentType) && !string.IsNullOrEmpty(Date))
            {
                var issuecomment  = IssueViewModels.RetrieveSPComment(IssueKey, CommentType, Date);
                return View(issuecomment);
            }

            return RedirectToAction("ViewAll", "Project");
        }

        [HttpPost, ActionName("ModifyIssueComment")]
        [ValidateAntiForgeryToken]
        public ActionResult ModifyIssueCommentPost()
        {
            var issuekey = Request.Form["HIssueKey"];
            var commenttype = Request.Form["HType"];
            var commentdate = Request.Form["HDate"];

            var urls = ReceiveRMAFiles();
            var contenturl = string.Empty;
            var contentreffile = string.Empty;
            if (!string.IsNullOrEmpty(Request.Form["contentattach"]))
            {
                var internalreportfile = Request.Form["contentattach"];
                var originalname = Path.GetFileNameWithoutExtension(internalreportfile)
                    .Replace(" ", "_").Replace("#", "").Replace("'", "")
                    .Replace("&", "").Replace("?", "").Replace("%", "").Replace("+", "");

                foreach (var r in urls)
                {
                    if (r.Contains(originalname))
                    {
                        contentreffile = originalname;
                        contenturl = r;
                        break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(Request.Form["editor1"]))
            {
                var tempcommment = new IssueComments();
                tempcommment.Comment = SeverHtmlDecode.Decode(this,Request.Form["editor1"]);
                if (!string.IsNullOrEmpty(contenturl))
                {
                    tempcommment.Comment = tempcommment.Comment + "<p><a href='" + contenturl + "' target='_blank'>Reference File: " + contentreffile + " " + "</a></p>";
                }

                IssueViewModels.UpdateSPComment(issuekey, commenttype, commentdate, tempcommment.dbComment);
            }
            else
            {
                var tempcommment = new IssueComments();
                tempcommment.Comment = "<p>To Be Edit</p>";
                if (!string.IsNullOrEmpty(contenturl))
                {
                    tempcommment.Comment = tempcommment.Comment + "<p><a href='" + contenturl + "' target='_blank'>Reference File: " + contentreffile + " " + "</a></p>";
                }

                IssueViewModels.UpdateSPComment(issuekey, commenttype, commentdate, tempcommment.dbComment);
            }

            var dict = new RouteValueDictionary();
            dict.Add("issuekey", issuekey);
            return RedirectToAction("UpdateIssue", "Issue", dict);
        }

        private List<string> PrepeareRMAReport(string ProjectKey, string StartDate, string EndDate)
        {
            var lines = new List<string>();
            var list1 = IssueViewModels.RetrieveIssueTypeByProjectKey(ProjectKey, StartDate, EndDate,ISSUETP.RMA,this);

            var line = "FINISAR RMA,SN,STATUS,CUSTOMER,FV,RMA FAILURE CODE,ROOT CAUSE,OWENER,CREATOR,OPEN ISSUE DATE,CUSTOMER RMA REASON,INTERNAL REPORT,CUSTOMER REPORT,Containment Action,Corrective Action,Attachment";
            lines.Add(line);

            foreach (var item in list1)
            {
                var rootcause = "";
                foreach (var r in item.RootCauseCommentList)
                {
                    rootcause = rootcause + System.Text.RegularExpressions.Regex.Replace(r.Comment.Replace("\"", "").Replace("&nbsp;", ""), "<.*?>", string.Empty).Trim() + "||";
                }

                var containmentaction = "";
                foreach (var c in item.ContainmentActions)
                {
                    containmentaction = containmentaction + c.Summary
                        + ":" + c.Resolution + "//";
                }

                var correctiveaction = "";
                foreach (var c in item.CorrectiveActions)
                {
                    correctiveaction = correctiveaction + c.Summary
                        + ":" + c.Resolution + "//";
                }

                

                var routevalue = new RouteValueDictionary();
                routevalue.Add("issuekey", "ABC");
                string scheme = this.Url.RequestContext.HttpContext.Request.Url.Scheme;
                string validatestr = this.Url.Action("UpdateIssue", "Issue", routevalue, scheme);
                validatestr = validatestr.Split(new string[] { "/Issue" }, StringSplitOptions.None)[0];

                var netcomputername = "";
                try { netcomputername = System.Net.Dns.GetHostName(); }
                catch (Exception ex) { }
                validatestr = validatestr.Replace("//localhost", "//" + netcomputername);

                var attach = "";
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
                    attach = attach + validatestr + internalreport1 + "||";
                }

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
                    + "\"" + item.ECustomer.Replace("\"", "") + "\"," + "\"" + item.FVCode.Replace("\"", "") + "\","+ "\""+item.RMAFailureCode.Replace("\"", "") + "\"," 
                    + "\""+ rootcause.Trim() + "\"," + "\""+item.Assignee.Replace("\"", "")+ "\"," + "\"" + item.Reporter.Replace("\"", "") + "\"," 
                    + "\""+item.ReportDate.ToString("yyyy-MM-dd hh:mm:ss") + "\"," 
                    + "\"" + item.CReport.Replace("\"", "") + "\","+ "\""+ internalreport + "\"," + "\""+ customerreport + "\","
                    + "\"" + containmentaction.Replace("\"", "") + "\"," + "\"" + correctiveaction.Replace("\"", "") + "\"," + "\"" + attach.Replace("\"", "") + "\",";

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
            var list1 = IssueViewModels.RetrieveAllIssueTypeIssue(StartDate, EndDate,ISSUETP.RMA,this);

            var line = "FINISAR RMA,PROJECT,SN,STATUS,CUSTOMER,FV,RMA FAILURE CODE,ROOT CAUSE,OWENER,CREATOR,OPEN ISSUE DATE,CUSTOMER RMA REASON,INTERNAL REPORT,CUSTOMER REPORT,Containment Action,Corrective Action,Attachment";
            lines.Add(line);

            foreach (var item in list1)
            {
                var rootcause = "";
                foreach (var r in item.RootCauseCommentList)
                {
                    rootcause = rootcause + System.Text.RegularExpressions.Regex.Replace(r.Comment.Replace("\"", "").Replace("&nbsp;", ""), "<.*?>", string.Empty).Trim() + "||";
                }

                var containmentaction = "";
                foreach (var c in item.ContainmentActions)
                {
                    containmentaction = containmentaction + c.Summary
                        + ":" + c.Resolution + "//";
                }

                var correctiveaction = "";
                foreach (var c in item.CorrectiveActions)
                {
                    correctiveaction = correctiveaction + c.Summary
                        + ":" + c.Resolution + "//";
                }

                var routevalue = new RouteValueDictionary();
                routevalue.Add("issuekey", "ABC");
                string scheme = this.Url.RequestContext.HttpContext.Request.Url.Scheme;
                string validatestr = this.Url.Action("UpdateIssue", "Issue", routevalue, scheme);
                validatestr = validatestr.Split(new string[] { "/Issue" }, StringSplitOptions.None)[0];

                var netcomputername = "";
                try { netcomputername = System.Net.Dns.GetHostName(); }
                catch (Exception ex) { }
                validatestr = validatestr.Replace("//localhost", "//" + netcomputername);

                var attach = "";
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
                    attach = attach + validatestr + internalreport1 + "||";
                }

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
                    + "\"" + item.Assignee.Replace("\"", "") + "\"," + "\"" + item.Reporter.Replace("\"", "") + "\"," + "\"" + item.ReportDate.ToString("yyyy-MM-dd hh:mm:ss") + "\"," + "\"" + item.CReport.Replace("\"", "") + "\","
                    + "\"" + internalreport + "\"," + "\"" + customerreport + "\","
                    + "\"" + containmentaction.Replace("\"", "") + "\"," + "\"" + correctiveaction.Replace("\"", "") + "\"," + "\"" + attach.Replace("\"", "") + "\",";

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

            var fn = "CQE_RMA_Report_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
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

        //private List<string> PrepeareOBAReport(string ProjectKey, string StartDate, string EndDate)
        //{
        //    var lines = new List<string>();
        //    var list1 = IssueViewModels.RetrieveIssueTypeByProjectKey(ProjectKey, StartDate, EndDate, ISSUETP.OBA);

        //    var line = "NO,ISSUE DATE,DMR#,Project Name,Failure Rate,Affected SN,FA Owner,OBA Description,Priority,Product Type,FV Results,ROOT CAUSE,Containment Action,Corrective Action,Material Disposition,Resolution,Attachement";
        //    lines.Add(line);

        //    var idx = 0;

        //    foreach (var item in list1)
        //    {
        //        idx = idx + 1;

        //        var index = idx.ToString();

        //        var rootcause = "";
        //        foreach (var r in item.RootCauseCommentList)
        //        {
        //            rootcause = rootcause + System.Text.RegularExpressions.Regex.Replace(r.Comment.Replace("\"", "").Replace("&nbsp;", ""), "<.*?>", string.Empty) + "||";
        //        }


        //        var routevalue = new RouteValueDictionary();
        //        routevalue.Add("issuekey", "ABC");
        //        string scheme = this.Url.RequestContext.HttpContext.Request.Url.Scheme;
        //        string validatestr = this.Url.Action("UpdateIssue", "Issue", routevalue, scheme);
        //        validatestr = validatestr.Split(new string[] { "/Issue" }, StringSplitOptions.None)[0];

        //        var netcomputername = "";
        //        try { netcomputername = System.Net.Dns.GetHostName(); }
        //        catch (Exception ex) { }
        //        validatestr = validatestr.Replace("/localhost/", "/" + netcomputername + "/");


        //        var internalreport = "";
        //        foreach (var a in item.AttachList)
        //        {
        //            var internalreport1 = "";
        //            if (a.Contains("<a href"))
        //            {
        //                internalreport1 = a.Split(new string[] { "<a href=\"", "\" target=" }, StringSplitOptions.None)[1];
        //            }
        //            else
        //            {
        //                internalreport1 = a;
        //            }
        //            internalreport = internalreport+validatestr + internalreport1 + "||";
        //        }

        //        var issuedata = item.DueDate.AddDays(-6).ToString("MM/dd/yyyy");

        //        var containmentaction = "";
        //        foreach (var c in item.ContainmentActions)
        //        {
        //            containmentaction = containmentaction + c.Summary
        //                + ":" + c.Resolution + "//";
        //        }

        //        var correctiveaction = "";
        //        foreach (var c in item.CorrectiveActions)
        //        {
        //            correctiveaction = c.Summary
        //                + ":" + c.Resolution + "//";
        //        }


        //        line = string.Empty;
        //        line = "\"" + index + "\"," + "\"" + issuedata + "\"," + "\"" + item.FinisarDMR.Replace("\"", "") + "\","
        //            + "\"" + item.ProjectKey + "\"," + "\"'" + item.OBAFailureRate.Replace("\"", "") + "\"," + "\"" + item.ModuleSN.Replace("\"", "") + "\"," 
        //            + "\"" + item.Assignee.Replace("\"", "") + "\","+ "\"" + item.Summary.Replace("\"", "") + "\","
        //            + "\"" + item.Priority.Replace("\"", "") + "\"," + "\"" + item.ProductType.Replace("\"", "") + "\","
        //            + "\"" + item.FVCode.Replace("\"", "") + "\"," + "\"" + rootcause.Replace("\"", "").Trim() + "\","
        //            + "\"" + containmentaction.Replace("\"", "") + "\"," + "\"" + correctiveaction.Replace("\"", "") + "\"," 
        //            + "\"" + item.MaterialDisposition + "\"," + "\"" + item.Resolution + "\"," + "\"" + internalreport + "\","
        //            + "\"http://wux-app1.china.ads.finisar.com/eDMR/DMR_Edit/DMR_View.asp?DMR_ID=" + item.FinisarDMR + "\",";

        //        lines.Add(line);
        //    }

        //    return lines;
        //}

        //public ActionResult ExportOBAData(string ProjectKey, string StartDate, string EndDate)
        //{
        //    if (!string.IsNullOrEmpty(ProjectKey))
        //    {
        //        string datestring = DateTime.Now.ToString("yyyyMMdd");
        //        string imgdir = Server.MapPath("~/userfiles") + "\\docs\\" + datestring + "\\";
        //        if (!Directory.Exists(imgdir))
        //        {
        //            Directory.CreateDirectory(imgdir);
        //        }

        //        var fn = ProjectKey + "_OBA_Report_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
        //        var filename = imgdir + fn;

        //        var lines = PrepeareOBAReport(ProjectKey, StartDate, EndDate);

        //        var wholefile = "";
        //        foreach (var l in lines)
        //        {
        //            wholefile = wholefile + l + "\r\n";
        //        }
        //        System.IO.File.WriteAllText(filename, wholefile);

        //        return File(filename, "application/vnd.ms-excel", fn);
        //    }
        //    return RedirectToAction("ViewAll", "Project");
        //}

        private List<string> PrepeareAllOBAReport(string StartDate, string EndDate)
        {
            var lines = new List<string>();
            var list1 = IssueViewModels.RetrieveAllIssueTypeIssue(StartDate, EndDate, ISSUETP.OBA,this);

            var line = "NO,ISSUE DATE,DMR#,Project Name,Failure Rate,Affected SN,FA Owner,OBA Description,Priority,Product Type,FV Results,ROOT CAUSE,Containment Action,Corrective Action,Material Disposition,Resolution,Attachement";
            lines.Add(line);

            var idx = 0;

            foreach (var item in list1)
            {
                idx = idx + 1;

                var index = idx.ToString();

                var rootcause = "";
                foreach (var r in item.RootCauseCommentList)
                {
                    rootcause = rootcause + System.Text.RegularExpressions.Regex.Replace(r.Comment.Replace("\"", "").Replace("&nbsp;", ""), "<.*?>", string.Empty).Trim() + "||";
                }

                var routevalue = new RouteValueDictionary();
                routevalue.Add("issuekey", "ABC");
                string scheme = this.Url.RequestContext.HttpContext.Request.Url.Scheme;
                string validatestr = this.Url.Action("UpdateIssue", "Issue", routevalue, scheme);
                validatestr = validatestr.Split(new string[] { "/Issue" }, StringSplitOptions.None)[0];

                var netcomputername = "";
                try { netcomputername = System.Net.Dns.GetHostName(); }
                catch (Exception ex) { }
                validatestr = validatestr.Replace("//localhost", "//" + netcomputername);


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
                    correctiveaction = correctiveaction + c.Summary
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

            var fn = "PQE_OBA_Report_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
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


        private List<string> PrepeareAllQualityReport( string StartDate, string EndDate)
        {
            var lines = new List<string>();
            var list1 = IssueViewModels.RetrieveAllIssueTypeIssue(StartDate, EndDate, ISSUETP.Quality,this);

            var line = "DATE,Project Name,Failure Mode,Product Type,Affected Range,Description,Priority,Owner,Analysis,ROOT CAUSE,Containment Action,Corrective Action,Product Disposal,Resolution,Attachement";
            lines.Add(line);

            foreach (var item in list1)
            {

                var rootcause = "";
                foreach (var r in item.RootCauseCommentList)
                {
                    rootcause = rootcause + System.Text.RegularExpressions.Regex.Replace(r.Comment.Replace("\"", "").Replace("&nbsp;", ""), "<.*?>", string.Empty).Trim() + "||";
                }

                var analysis = "";
                foreach (var r in item.AnalysisCommentList)
                {
                    analysis = analysis + System.Text.RegularExpressions.Regex.Replace(r.Comment.Replace("\"", "").Replace("&nbsp;", ""), "<.*?>", string.Empty).Trim() + "||";
                }

                var routevalue = new RouteValueDictionary();
                routevalue.Add("issuekey", "ABC");
                string scheme = this.Url.RequestContext.HttpContext.Request.Url.Scheme;
                string validatestr = this.Url.Action("UpdateIssue", "Issue", routevalue, scheme);
                validatestr = validatestr.Split(new string[] { "/Issue" }, StringSplitOptions.None)[0];

                var netcomputername = "";
                try { netcomputername = System.Net.Dns.GetHostName(); }
                catch (Exception ex) { }
                validatestr = validatestr.Replace("//localhost", "//" + netcomputername);


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

                var issuedata = item.DueDate.AddDays(-12).ToString("MM/dd/yyyy");

                var containmentaction = "";
                foreach (var c in item.ContainmentActions)
                {
                    containmentaction = containmentaction + c.Summary
                        + ":" + c.Resolution + "//";
                }

                var correctiveaction = "";
                foreach (var c in item.CorrectiveActions)
                {
                    correctiveaction = correctiveaction + c.Summary
                        + ":" + c.Resolution + "//";
                }


                line = string.Empty;
                line = "\"" + issuedata + "\"," + "\"" + item.ProjectKey + "\"," + "\"" + item.RMAFailureCode.Replace("\"", "") + "\","
                    + "\"" + item.ProductType + "\"," + "\"" + item.AffectRange.Replace("\"", "") + "\"," + "\"" + item.Summary.Replace("\"", "") + "\","
                    + "\"" + item.Priority.Replace("\"", "") + "\"," + "\"" + item.Assignee.Replace("\"", "") + "\","
                    + "\"" + analysis.Replace("\"", "") + "\"," + "\"" + rootcause.Replace("\"", "") + "\","
                    + "\"" + containmentaction.Replace("\"", "") + "\"," + "\"" + correctiveaction.Replace("\"", "") + "\","
                    + "\"" + item.MaterialDisposition + "\"," + "\"" + item.Resolution + "\"," + "\"" + internalreport + "\",";

                lines.Add(line);
            }

            return lines;
        }

        public ActionResult ExportAllQualityData(string StartDate, string EndDate)
        {
                string datestring = DateTime.Now.ToString("yyyyMMdd");
                string imgdir = Server.MapPath("~/userfiles") + "\\docs\\" + datestring + "\\";
                if (!Directory.Exists(imgdir))
                {
                    Directory.CreateDirectory(imgdir);
                }

                var fn = "PQE_Quality_Report_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
                var filename = imgdir + fn;

                var lines = PrepeareAllQualityReport(StartDate, EndDate);

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