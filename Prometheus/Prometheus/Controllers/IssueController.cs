using Prometheus.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
            string[] tlist = { ISSUETP.Task,ISSUETP.Bug,ISSUETP.RMA,ISSUETP.OBA,ISSUETP.NPIPROC};

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

                ViewBag.tobechoosetags = ShareDocVM.RetrieveShareTags(this);

                var asilist = UserViewModels.RetrieveAllUser();
                ViewBag.towholist = CreateSelectList(asilist, "");


                var typelist = new List<string>();
                string[] tlist = { ISSUETP.Task, ISSUETP.Bug, ISSUETP.NPIPROC };
                typelist.AddRange(tlist);
                ViewBag.issuetypelist = CreateSelectList(typelist, "");

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
            EmailUtility.SendEmail(this,"WUXI Engineering System_"+ reporter, toaddrs, content);
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
            //if (vm.RelativePeopleList.Count > 0)
            //{
            //    toaddrs.AddRange(vm.RelativePeopleList);
            //}

            var report = Reporter.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries)[0].Replace(".", " ");
            EmailUtility.SendEmail(this,"WUXI Engineering System_"+report, toaddrs, content);
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

            if (vm.IssueType.Contains(ISSUETP.Task))
            {
                IssueTypeVM.SaveIssueType(vm.IssueKey, ISSUESUBTYPE.Task.ToString());
            }
            else if (vm.IssueType.Contains(ISSUETP.Bug))
            {
                IssueTypeVM.SaveIssueType(vm.IssueKey, ISSUESUBTYPE.Bug.ToString());
            }
            else if (vm.IssueType.Contains(ISSUETP.NPIPROC))
            {
                IssueTypeVM.SaveIssueType(vm.IssueKey, ISSUESUBTYPE.NPIProcess.ToString());
            }
            else
            {
                IssueTypeVM.SaveIssueType(vm.IssueKey, ISSUESUBTYPE.Other.ToString());
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
                        ShareDocVM.ShareDoc(vm.ProjectKey, ShareDocType.DOCUMENT, dockey, attachtag, updater, DateTime.Now.ToString(), "/Issue/UpdateIssue?issuekey="+vm.IssueKey);
                    }
                }
            }


            //ProjectEvent.CreateIssueEvent(vm.ProjectKey, vm.Reporter, vm.Assignee, vm.Summary, vm.IssueKey);

            //if (vm.RelativePeopleList.Count > 0)
            //{
                SendTaskEvent(vm, "Created");
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
                ViewBag.tobechoosetags = ShareDocVM.RetrieveShareTags(this);
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

                if (ret.Summary.Contains(CRITICALERRORTYPE.LYTTASK) || ret.Summary.Contains(CRITICALERRORTYPE.LYTTASK1))
                {
                    var templist = new List<string>();
                    templist.Add("YES");
                    templist.Add("NO");
                    ViewBag.iscriticaltasklist = CreateSelectList(templist, "");

                    ViewBag.tobechoosetags = ShareDocVM.RetrieveCriticalTags(this);
                }
                else
                {
                    ViewBag.tobechoosetags = ShareDocVM.RetrieveShareTags(this);
                }

                var asilist = UserViewModels.RetrieveAllUser();
                ViewBag.towholist = CreateSelectList(asilist, "");
                ViewBag.towholist1 = CreateSelectList(asilist, "");
                
                return View(ret);
            }
            else
            {
                var asilist = UserViewModels.RetrieveAllUser();
                ViewBag.towholist = CreateSelectList(asilist, "");
                ViewBag.towholist1 = CreateSelectList(asilist, "");

                var tempvm = new IssueViewModels();
                CreateAllLists(tempvm);
                ViewBag.tobechoosetags = ShareDocVM.RetrieveShareTags(this);
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
            EmailUtility.SendEmail(this, "WUXI Engineering System_"+commenter, towho, content);
            new System.Threading.ManualResetEvent(false).WaitOne(50);
        }

        private List<string> RetrieveUserFromComment(string comment)
        {
            var ret = new List<string>();
            var startidx = 0;
            while (comment.IndexOf("@", startidx) != -1)
            {
                var namestartidx = comment.IndexOf("@", startidx);
                var namestart = comment.Substring(namestartidx);
                var spaceidx = namestart.IndexOf(" ");
                if (spaceidx == -1)
                    break;
                var name = namestart.Substring(1, spaceidx-1);
                if (name.Length > 3)
                {
                    if (name.ToUpper().Contains("@FINISAR.COM"))
                    {
                        ret.Add(name.ToUpper());
                    }
                    else if (name.Contains("."))
                    {
                        ret.Add(name.ToUpper()+ "@FINISAR.COM");
                    }
                    startidx = spaceidx + 1;
                }
                else
                {
                    startidx = startidx + 1;
                }
            }
            return ret;
        }

        private List<string> PJPQE(IssueViewModels vm)
        {
            var pj = ProjectViewModels.RetrieveOneProject(vm.ProjectKey);
            var ret = new List<string>();
            foreach (var item in pj.MemberList)
            {
                if (string.Compare(item.Role, ProjectViewModels.PQEROLE, true) == 0)
                {
                    ret.Add(item.Name);
                }
            }
            return ret;
        }

        private void SendPDMSEvent(IssueViewModels vm, string comment,string rootcause)
        {
            var netcomputername = "";
            try
            {
                netcomputername = System.Net.Dns.GetHostName();
            }
            catch (Exception ex)
            { }

            var body = new List<List<string>>();
            var tmpList = new List<string>();
            tmpList.Add("Project");
            tmpList.Add("ModuleSN");
            tmpList.Add("RootCause");
            tmpList.Add("Failure Description");
            tmpList.Add("Task Link");
            body.Add(tmpList);

                var routevalue = new RouteValueDictionary();
                routevalue.Add("issuekey", vm.IssueKey);
                //send validate email
                string scheme = this.Url.RequestContext.HttpContext.Request.Url.Scheme;
                string validatestr = this.Url.Action("UpdateIssue", "Issue", routevalue, scheme);
                validatestr = validatestr.Replace("//localhost", "//" + netcomputername);

                tmpList = new List<string>();
                tmpList.Add(vm.ProjectKey);
                tmpList.Add(vm.ModuleSN);
                tmpList.Add(rootcause);
                tmpList.Add(vm.Summary.Replace(CRITICALERRORTYPE.SECONDMATCH, ""));
                tmpList.Add("<a href=" + validatestr + ">Detail</a>");
                body.Add(tmpList);

            var towho = new List<string>();
            towho.Add(vm.Assignee);
            towho.Add(vm.Reporter);
            towho.AddRange(PJPQE(vm));
            if (vm.RelativePeopleList.Count > 0)
            {
                towho.AddRange(vm.RelativePeopleList);
            }

            var greeting = "Hi All";
            var description = "Below is Parallel Test Critical Alarm base on WUXI ENGINEERING SYSTEM (Human Reviewed) --" + DateTime.Now.ToString("MM/dd/yyyy");
            var content = EmailUtility.CreateTableHtml(greeting, description, comment, body);

            EmailUtility.SendEmail(this, "Parallel Test Critical Failure Alarm Report -- " + DateTime.Now.ToString("MM/dd/yyyy"), towho, content, true);

            new System.Threading.ManualResetEvent(false).WaitOne(500);
        }

        [HttpPost, ActionName("UpdateIssue")]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateIssuePost()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

            var issuekey = Request.Form["IssueKey"];
            var originaldata = IssueViewModels.RetrieveIssueByIssueKey(issuekey,this);

            //write log
            LogVM.WriteLog(updater.ToUpper(), originaldata.ProjectKey, DetermineCompName(Request.UserHostName),
                Request.Url.ToString(), "Issue", "Update", issuekey, LogType.Task, Log4NetLevel.Info, "");

            if (string.Compare(updater, originaldata.Assignee, true) == 0
            || string.Compare(updater, originaldata.Reporter, true) == 0
            || string.Compare(updater, originaldata.Creator, true) == 0)
            { }
            else
            {
                if (!string.IsNullOrEmpty(Request.Form["editor1"]))
                {
                    var issuecomment = new IssueComments();
                    issuecomment.Comment = SeverHtmlDecode.Decode(this, Request.Form["editor1"]);
                    IssueViewModels.StoreIssueComment(originaldata.IssueKey, issuecomment.dbComment, updater, COMMENTTYPE.Description);
                }

                var dict2 = new RouteValueDictionary();
                dict2.Add("issuekey", originaldata.IssueKey);
                return RedirectToAction("UpdateIssue", "Issue", dict2);
            }

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

                
                //if (string.Compare(updater.ToUpper(), originaldata.Assignee) != 0)
                //{
                var commenter = updater.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries)[0].Replace("."," ");

                var towho = new List<string>();
                towho.Add(vm.Assignee);
                if (originaldata.Reporter.Contains("@"))
                    towho.Add(originaldata.Reporter);
                towho.Add(updater);

                if(vm.RelativePeopleList.Count > 0) towho.AddRange(vm.RelativePeopleList);
                var atlist = RetrieveUserFromComment(vm.Description);
                if(atlist.Count > 0) towho.AddRange(atlist);

                var commentcontent = System.Text.RegularExpressions.Regex.Replace(vm.Description.Replace("\"", "").Replace("&nbsp;", ""), "<.*?>", string.Empty).Trim();
                SendTaskCommentEmail(originaldata.IssueKey,originaldata.Summary,commenter,towho, commentcontent);
                //}

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
                        ShareDocVM.ShareDoc(originaldata.ProjectKey, ShareDocType.DOCUMENT, dockey, attachtag, updater, DateTime.Now.ToString(), "/Issue/UpdateIssue?issuekey=" + originaldata.IssueKey);

                        UserKPIVM.AddUserAttachDailyRank(vm.IssueKey, updater, UserRankType.ADDITIONAL
                        , "Add attachement with tag to task: " + dockey, "/Issue/UpdateIssue?issuekey=" + vm.IssueKey, 2,dockey,this);
                    }
                    else
                    {
                        var tempkeys = url.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                        var dockey = tempkeys[tempkeys.Length - 1];

                        UserKPIVM.AddUserAttachDailyRank(vm.IssueKey, updater, UserRankType.ADDITIONAL
                        , "Add attachement to task: " + dockey, "/Issue/UpdateIssue?issuekey=" + vm.IssueKey, 1, dockey, this);
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
                    if (realissue.Summary.Contains(CRITICALERRORTYPE.LYTTASK)|| realissue.Summary.Contains(CRITICALERRORTYPE.LYTTASK1))
                    {
                        UserKPIVM.AddUserDailyRank(realissue.IssueKey, realissue.Assignee, UserRankType.BASE
                            , "Close CRITICAL ERROR Task: " + realissue.Summary, "/Issue/UpdateIssue?issuekey=" + realissue.IssueKey, 4);

                        var isrealcritical = Request.Form["iscriticaltasklist"];
                        if (isrealcritical != null)
                        {
                            if (string.Compare(isrealcritical, "NO", true) == 0)
                            {
                                realissue.UpdateSummary(CRITICALERRORTYPE.NOTREALCRITICALISSUE + realissue.Summary.Replace(CRITICALERRORTYPE.LYTTASK, ""));
                                IssueTypeVM.SaveIssueType(vm.IssueKey, ISSUESUBTYPE.NonCrititalFailureTask.ToString(), ISSUESUBTYPE.NonCrititalFailureTask.ToString());
                            }
                            else
                            {
                                var tmpcmt = updater.ToUpper().Replace("@FINISAR.COM", "") + " has confirmed this is a real critical failure.";
                                SendPDMSEvent(realissue,tmpcmt,issuetag);
                            }
                        }

                        //if (!issuetag.Contains(CRITICALERRORTYPE.CRITICALERRORTAG))
                        //{
                        //    issuetag = issuetag + CRITICALERRORTYPE.CRITICALERRORTAG + ";";
                        //}
                    }
                    else
                    {
                        if (string.Compare(realissue.IssueType, ISSUETP.NPIPROC) != 0)
                        {
                            if (string.IsNullOrEmpty(realissue.ParentIssueKey))
                            {
                                UserKPIVM.AddUserDailyRank(realissue.IssueKey, realissue.Assignee, UserRankType.BASE
                                    , "Close General Task: " + realissue.Summary, "/Issue/UpdateIssue?issuekey=" + realissue.IssueKey, 4);
                            }
                            else
                            {
                                var parentissue = IssueViewModels.RetrieveIssueByIssueKey(realissue.ParentIssueKey, this);
                                if (string.Compare(parentissue.Assignee, realissue.Assignee, true) != 0)
                                {
                                    UserKPIVM.AddUserDailyRank(realissue.IssueKey, realissue.Assignee, UserRankType.BASE
                                    , "Close General Task: " + realissue.Summary, "/Issue/UpdateIssue?issuekey=" + realissue.IssueKey, 3);
                                }
                            }
                        }//end if

                    }

                    if (!string.IsNullOrEmpty(issuetag))
                    {
                        ShareDocVM.ShareDoc(originaldata.ProjectKey, ShareDocType.ISSUE, originaldata.IssueKey, issuetag, updater, DateTime.Now.ToString(), "/Issue/UpdateIssue?issuekey=" + originaldata.IssueKey);
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
            ShareDocVM.ShareDoc(issue.ProjectKey, ShareDocType.ISSUE, issue.IssueKey, issue.Summary, updater, DateTime.Now.ToString(),"/Issue/UpdateIssue?issuekey=" + IssueKey);

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
            ViewBag.tobechoosetags = ShareDocVM.RetrieveShareTags(this);

            var asilist = UserViewModels.RetrieveAllUser();
            ViewBag.towholist = CreateSelectList(asilist, "");

            var typelist = new List<string>();
            string[] tlist = { ISSUETP.Task,ISSUETP.Bug};
            typelist.AddRange(tlist);
            ViewBag.issuetypelist = CreateSelectList(typelist, vm.IssueType);

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

            if (vm.IssueType.Contains(ISSUETP.Task))
            {
                IssueTypeVM.SaveIssueType(vm.IssueKey, ISSUESUBTYPE.Task.ToString());
            }
            else if (vm.IssueType.Contains(ISSUETP.Bug))
            {
                IssueTypeVM.SaveIssueType(vm.IssueKey, ISSUESUBTYPE.Bug.ToString());
            }
            else if (vm.IssueType.Contains(ISSUETP.NPIPROC))
            {
                IssueTypeVM.SaveIssueType(vm.IssueKey, ISSUESUBTYPE.NPIProcess.ToString());
            }
            else
            {
                IssueTypeVM.SaveIssueType(vm.IssueKey, ISSUESUBTYPE.Other.ToString());
            }

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
                        ShareDocVM.ShareDoc(vm.ProjectKey, ShareDocType.DOCUMENT, dockey, attachtag, updater, DateTime.Now.ToString(), "/Issue/UpdateIssue?issuekey=" + vm.IssueKey);

                        UserKPIVM.AddUserAttachDailyRank(vm.IssueKey, updater, UserRankType.ADDITIONAL
                        , "Add attachement with tag to task: " + dockey, "/Issue/UpdateIssue?issuekey=" + vm.IssueKey, 2, dockey, this);
                    }
                    else
                    {
                        var tempkeys = url.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                        var dockey = tempkeys[tempkeys.Length - 1];

                        UserKPIVM.AddUserAttachDailyRank(vm.IssueKey, updater, UserRankType.ADDITIONAL
                        , "Add attachement to task: " + dockey, "/Issue/UpdateIssue?issuekey=" + vm.IssueKey, 1, dockey, this);
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

        private void CreateSearchParams(string pj, string tp, string rs, string asn, string sd, string ed, string desp)
        {
            ViewBag.pj = pj;
            ViewBag.tp = tp;
            ViewBag.rs = rs;
            ViewBag.asn = asn;
            ViewBag.sd = sd;
            ViewBag.ed = ed;
            ViewBag.desp = desp;
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

            if (string.Compare(Request.Form["ProjectKey"].ToString(), "NONE") != 0)
            {
                pj = Request.Form["ProjectKey"].ToString();
            }

            if (string.Compare(Request.Form["IssueType"].ToString(), "NONE") != 0)
            {
                tp = Request.Form["IssueType"].ToString();
            }

            if (string.Compare(Request.Form["Resolution"].ToString(), "NONE") != 0)
            {
                rs = Request.Form["Resolution"].ToString();
            }

            if (string.Compare(Request.Form["Assignee"].ToString(), "NONE") != 0)
            {
                asn = Request.Form["Assignee"].ToString();
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
                ViewBag.SearchError = "At least choose one condition to search Task";
                CreateAllSearchLists();
                return View();
            }

            var vmlist = IssueViewModels.SearchIssue(pj, tp, rs, asn, sd, ed, desp, 100);
            CreateSearchParams(pj, tp, rs, asn, sd, ed, desp);
            CreateAllSearchLists();
            return View(vmlist);
        }

        public ActionResult SearachIssue2(string pj, string desp)
        {

            var tp = "";
            var rs = "";
            var asn = "";
            var sd = "";
            var ed = "";
            
            var vmlist = IssueViewModels.SearchIssue(pj, tp, rs, asn, sd, ed, desp, 100);
            CreateSearchParams(pj, tp, rs, asn, sd, ed, desp);
            CreateAllSearchLists();
            return View("SearchIssue", vmlist);
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

                ViewBag.tobechoosetags = ShareDocVM.RetrieveShareTags(this);
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
                EmailUtility.SendEmail(this,"WUXI Engineering System_"+reporter, toaddrs, content);
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
                EmailUtility.SendEmail(this,"WUXI Engineering System_"+reporter, toaddrs, content);
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

            IssueTypeVM.SaveIssueType(vm.IssueKey, ISSUESUBTYPE.RMA.ToString());

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
                        ShareDocVM.ShareDoc(vm.ProjectKey, ShareDocType.DOCUMENT, dockey, attachtag, updater, DateTime.Now.ToString(), "/Issue/UpdateIssue?issuekey=" + vm.IssueKey);

                        UserKPIVM.AddUserAttachDailyRank(vm.IssueKey, updater, UserRankType.ADDITIONAL
                        , "Add attachement with tag to task: " + dockey, "/Issue/UpdateIssue?issuekey=" + vm.IssueKey, 2, dockey, this);
                    }
                    else
                    {
                        var tempkeys = url.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                        var dockey = tempkeys[tempkeys.Length - 1];

                        UserKPIVM.AddUserAttachDailyRank(vm.IssueKey, updater, UserRankType.ADDITIONAL
                        , "Add attachement to task: " + dockey, "/Issue/UpdateIssue?issuekey=" + vm.IssueKey, 1, dockey, this);
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

                ViewBag.tobechoosetags = ShareDocVM.RetrieveShareTags(this);
                return View();
            }

            var ret = IssueViewModels.RetrieveIssueByIssueKey(key,this);

            if (ret != null)
            {
                var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
                ViewBag.isassignee = false;

                var pj = ProjectViewModels.RetrieveOneProject(ret.ProjectKey);
                if (pj != null) {
                    foreach (var item in pj.MemberList)
                    {
                        if (string.Compare(updater, item.Name, true) == 0)
                        {
                            ViewBag.isassignee = true;
                        }
                    }
                }

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

                if (ret.Summary.Contains(CRITICALERRORTYPE.LYTTASK) || ret.Summary.Contains(CRITICALERRORTYPE.LYTTASK1))
                {
                    var templist = new List<string>();
                    templist.Add("YES");
                    templist.Add("NO");
                    ViewBag.iscriticaltasklist = CreateSelectList(templist,"");
                    ViewBag.tobechoosetags = ShareDocVM.RetrieveCriticalTags(this);
                }
                else
                {
                    ViewBag.tobechoosetags = ShareDocVM.RetrieveShareTags(this);
                }

                return View(ret);
            }
            else
            {
                var asilist = UserViewModels.RetrieveAllUser();
                ViewBag.towholist = CreateSelectList(asilist, "");
                ViewBag.towholist1 = CreateSelectList(asilist, "");

                var tempvm = new IssueViewModels();
                CreateAllLists(tempvm);

                ViewBag.tobechoosetags = ShareDocVM.RetrieveShareTags(this);
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
            //write log
            LogVM.WriteLog(updater.ToUpper(), originaldata.ProjectKey, DetermineCompName(Request.UserHostName),
                Request.Url.ToString(), "Bug", "Update", issuekey, LogType.Task, Log4NetLevel.Info, "");

            var pjmemauth = false;
            var pj = ProjectViewModels.RetrieveOneProject(originaldata.ProjectKey);
            if (pj != null)
            {
                foreach (var item in pj.MemberList)
                {
                    if (string.Compare(updater, item.Name, true) == 0)
                    {
                        pjmemauth = true;
                    }
                }
            }

            if (pjmemauth || string.Compare(updater, originaldata.Assignee, true) == 0
                || string.Compare(updater, originaldata.Reporter, true) == 0
                || string.Compare(updater, originaldata.Creator, true) == 0)
            { }
            else
            {
                    if ( !string.IsNullOrEmpty(Request.Form["editor1"]))
                    {
                        var issuecomment = new IssueComments();
                        issuecomment.Comment = SeverHtmlDecode.Decode(this, Request.Form["editor1"]);
                        IssueViewModels.StoreIssueComment(originaldata.IssueKey, issuecomment.dbComment, updater, COMMENTTYPE.Description);
                    }

                    var dict2 = new RouteValueDictionary();
                    dict2.Add("issuekey", originaldata.IssueKey);
                    return RedirectToAction("UpdateIssue", "Issue", dict2);
            }

            if (Request.Form["deleteisu"] != null)
            {
                if (pjmemauth || string.Compare(updater, originaldata.Reporter, true) == 0
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
                if (pjmemauth || string.Compare(updater, originaldata.Reporter, true) == 0
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

                                if (testdata.Count > 0)
                                {
                                    SameAsDBTVM.StoreLinkDBTIssue(perrlist[0].ErrorKey, linktime, originaldata.IssueKey, originaldata.IssueKey, testdata[0].ModuleSerialNum);
                                }

                                UserKPIVM.AddUserDailyRank(originaldata.IssueKey, updater, UserRankType.SPECIAL, "Link To "+originaldata.ProjectKey+" Debug Tree "+ errabbr
                                    , "/Issue/UpdateIssue?issuekey=" + originaldata.IssueKey, 6);

                                //write log
                                LogVM.WriteLog(updater.ToUpper(), originaldata.ProjectKey, DetermineCompName(Request.UserHostName),
                                        Request.Url.ToString(), perrlist[0].OrignalCode, perrlist[0].ShortDesc, perrlist[0].ErrorKey, LogType.DebugTree, Log4NetLevel.Info, "");
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

                                if (testdata.Count > 0)
                                {
                                    SameAsDBTVM.StoreLinkDBTIssue(perrlist[0].ErrorKey, linktime, originaldata.IssueKey, originaldata.IssueKey, testdata[0].ModuleSerialNum);
                                }

                                UserKPIVM.AddUserDailyRank(originaldata.IssueKey, updater, UserRankType.SPECIAL, "Link To Burn-In Debug Tree " + errabbr
                                    , "/Issue/UpdateIssue?issuekey=" + originaldata.IssueKey, 6);

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

            if (pjmemauth || string.Compare(originaldata.Reporter, updater, true) == 0
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

            bool analyzeinputed = false;

            if (!string.IsNullOrEmpty(Request.Form["failureeditor"]))
            {
                var failure = SeverHtmlDecode.Decode(this,Request.Form["failureeditor"]);
                var dbstr = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(failure));
                var commenttype = COMMENTTYPE.FailureDetail;
                IssueViewModels.StoreIssueComment(vm.IssueKey, dbstr, vm.Reporter, commenttype);

                analyzeinputed = true;
            }

            if (!string.IsNullOrEmpty(Request.Form["rootcauseeditor"]))
            {
                var rootcause = SeverHtmlDecode.Decode(this,Request.Form["rootcauseeditor"]);
                var dbstr = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(rootcause));
                var commenttype = COMMENTTYPE.RootCause;
                IssueViewModels.StoreIssueComment(vm.IssueKey, dbstr, vm.Reporter, commenttype);

                analyzeinputed = true;
            }

            if (!string.IsNullOrEmpty(Request.Form["resulteditor"]))
            {
                var rootcause = SeverHtmlDecode.Decode(this,Request.Form["resulteditor"]);
                var dbstr = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(rootcause));
                var commenttype = COMMENTTYPE.Result;
                IssueViewModels.StoreIssueComment(vm.IssueKey, dbstr, vm.Reporter, commenttype);

                analyzeinputed = true;
            }

            if (analyzeinputed)
            {
                if (string.IsNullOrEmpty(Request.Form["failureeditor"]) && originaldata.FailureDetailCommentList.Count == 0)
                {
                    var dbstr = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("<p>To Be Edit</p>"));
                    var commenttype = COMMENTTYPE.FailureDetail;
                    IssueViewModels.StoreIssueComment(vm.IssueKey, dbstr, vm.Reporter, commenttype);
                }
                if (string.IsNullOrEmpty(Request.Form["rootcauseeditor"]) && originaldata.RootCauseCommentList.Count == 0)
                {
                    var dbstr = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("<p>To Be Edit</p>"));
                    var commenttype = COMMENTTYPE.RootCause;
                    IssueViewModels.StoreIssueComment(vm.IssueKey, dbstr, vm.Reporter, commenttype);
                }
                if (string.IsNullOrEmpty(Request.Form["resulteditor"]) && originaldata.ResultCommentList.Count == 0)
                {
                    var dbstr = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("<p>To Be Edit</p>"));
                    var commenttype = COMMENTTYPE.Result;
                    IssueViewModels.StoreIssueComment(vm.IssueKey, dbstr, vm.Reporter, commenttype);
                }
            }

            if (!string.IsNullOrEmpty(Request.Form["editor1"]))
            {
                vm.Description = SeverHtmlDecode.Decode(this,Request.Form["editor1"]);
                vm.CommentType = COMMENTTYPE.Description;

                //if (string.Compare(updater.ToUpper(), originaldata.Assignee) != 0)
                //{
                var commenter = updater.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries)[0].Replace(".", " ");
                var towho = new List<string>();
                towho.Add(vm.Assignee);
                if (originaldata.Reporter.Contains("@"))
                    towho.Add(originaldata.Reporter);
                towho.Add(updater);

                if (vm.RelativePeopleList.Count > 0) towho.AddRange(vm.RelativePeopleList);
                var atlist = RetrieveUserFromComment(vm.Description);
                if (atlist.Count > 0) towho.AddRange(atlist);

                var commentcontent = System.Text.RegularExpressions.Regex.Replace(vm.Description.Replace("\"", "").Replace("&nbsp;", ""), "<.*?>", string.Empty).Trim();
                SendTaskCommentEmail(originaldata.IssueKey, originaldata.Summary, commenter, towho, commentcontent);
                //}

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
                        ShareDocVM.ShareDoc(originaldata.ProjectKey, ShareDocType.DOCUMENT, dockey, attachtag, updater, DateTime.Now.ToString(), "/Issue/UpdateIssue?issuekey=" + vm.IssueKey);

                        UserKPIVM.AddUserAttachDailyRank(vm.IssueKey, updater, UserRankType.ADDITIONAL
                        , "Add attachement with tag to task: " + dockey, "/Issue/UpdateIssue?issuekey=" + vm.IssueKey, 2, dockey, this);
                    }
                    else
                    {
                        var tempkeys = url.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                        var dockey = tempkeys[tempkeys.Length - 1];

                        UserKPIVM.AddUserAttachDailyRank(vm.IssueKey, updater, UserRankType.ADDITIONAL
                        , "Add attachement to task: " + dockey, "/Issue/UpdateIssue?issuekey=" + vm.IssueKey, 1, dockey, this);
                    }
                }
            }

            //ProjectEvent.OperateIssueEvent(originaldata.ProjectKey, updater, "Updated", originaldata.Summary, originaldata.IssueKey);

            if (pjmemauth || string.Compare(originaldata.Assignee, vm.Assignee, true) != 0)
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
                    realissue.CloseIssue();
                    ProjectErrorViewModels.UpdateSovledIssueCount(realissue.ProjectKey, realissue.ErrAbbr);

                    if (realissue.Summary.Contains(CRITICALERRORTYPE.LYTTASK)|| realissue.Summary.Contains(CRITICALERRORTYPE.LYTTASK1))
                    {
                        var isrealcritical = Request.Form["iscriticaltasklist"];
                        if (isrealcritical != null)
                        {
                            if (string.Compare(isrealcritical, "NO", true) == 0)
                            {
                                realissue.UpdateSummary(CRITICALERRORTYPE.NOTREALCRITICALISSUE + realissue.Summary.Replace(CRITICALERRORTYPE.LYTTASK, ""));
                                IssueTypeVM.SaveIssueType(vm.IssueKey, ISSUESUBTYPE.NonCrititalFailureTask.ToString(), ISSUESUBTYPE.NonCrititalFailureTask.ToString());
                            }
                            else
                            {
                                var tmpcmt = updater.ToUpper().Replace("@FINISAR.COM", "") + " has confirmed this is a real critical failure.";
                                SendPDMSEvent(realissue, tmpcmt, issuetag);
                            }
                        }
                        UserKPIVM.AddUserDailyRank(realissue.IssueKey, realissue.Assignee, UserRankType.BASE
                            , "Close CRITICAL ERROR Task: " + realissue.Summary, "/Issue/UpdateIssue?issuekey=" + realissue.IssueKey, 4);
                    }

                    if (!string.IsNullOrEmpty(issuetag))
                    {
                        ShareDocVM.ShareDoc(originaldata.ProjectKey, ShareDocType.ISSUE, originaldata.IssueKey, issuetag, updater, DateTime.Now.ToString(), "/Issue/UpdateIssue?issuekey=" + originaldata.IssueKey);
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


            var dict1 = new RouteValueDictionary();
            dict1.Add("issuekey", originaldata.IssueKey);
            return RedirectToAction("UpdateBug", "Issue", dict1);
        }

        private static void CreateRMASubIssue(string presum, string sum, string pjkey, string parentkey, string analyser, string reporter, DateTime duedate,string moretag)
        {
            var vm = new IssueViewModels();
            vm.ProjectKey = pjkey;
            vm.IssueKey = IssueViewModels.GetUniqKey();
            vm.ParentIssueKey = parentkey;
            vm.IssueType = ISSUETP.Task;
            vm.Summary = presum + sum;
            vm.Priority = ISSUEPR.Major;
            vm.DueDate = duedate;
            vm.ReportDate = DateTime.Now;
            vm.Assignee = analyser;
            vm.Reporter = reporter;
            vm.Resolution = Resolute.Pending;
            vm.ResolvedDate = DateTime.Parse("1982-05-06 01:01:01");
            vm.StoreSubIssue();

            IssueTypeVM.SaveIssueType(vm.IssueKey, ISSUESUBTYPE.Task.ToString(),moretag);
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
                var asilist = UserViewModels.RetrieveAllUser();
                ViewBag.towholist = CreateSelectList(asilist, "");
                ViewBag.towholist1 = CreateSelectList(asilist, "");
                return View();
            }

            var ret = IssueViewModels.RetrieveIssueByIssueKey(key,this);
            if (ret != null)
            {
                var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

                if (ret.ContainmentActions.Count == 0)
                {
                    CreateRMASubIssue(RMASubIssueType.CONTAINMENTACTION, "Cotainment Action for RMA " + ret.FinisarRMA, ret.ProjectKey, ret.IssueKey, ret.Assignee, ret.Reporter, ret.DueDate.AddDays(18),ISSUESUBTYPE.CONTAINMENT.ToString());
                    ret = IssueViewModels.RetrieveIssueByIssueKey(key, this);
                }

                if (ret.CorrectiveActions.Count == 0)
                {
                    CreateRMASubIssue(RMASubIssueType.CORRECTIVEACTION, "Corrective Action for RMA " + ret.FinisarRMA, ret.ProjectKey, ret.IssueKey, ret.Assignee, ret.Reporter, ret.DueDate.AddDays(48),ISSUESUBTYPE.CORRECTIVE.ToString());
                    ret = IssueViewModels.RetrieveIssueByIssueKey(key, this);
                }

                ViewBag.isassignee = false;
                if (string.Compare(updater, ret.Assignee, true) == 0 
                    || string.Compare(updater,ret.Reporter,true) == 0
                    || string.Compare(updater, ret.Creator, true) == 0)
                {
                    ViewBag.isassignee = true;
                }
                //ret.Reporter = updater;

                ViewBag.tobechoosetags = ShareDocVM.RetrieveShareTags(this);
                CreateAllLists(ret);
                var asilist = UserViewModels.RetrieveAllUser();
                ViewBag.towholist = CreateSelectList(asilist, "");
                ViewBag.towholist1 = CreateSelectList(asilist, "");
                return View(ret);
            }
            else
            {
                var tempvm = new IssueViewModels();
                CreateAllLists(tempvm);
                var asilist = UserViewModels.RetrieveAllUser();
                ViewBag.towholist = CreateSelectList(asilist, "");
                ViewBag.towholist1 = CreateSelectList(asilist, "");
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

            //write log
            LogVM.WriteLog(updater.ToUpper(), originaldata.ProjectKey, DetermineCompName(Request.UserHostName),
                Request.Url.ToString(), "RMA", "Update", issuekey, LogType.Task, Log4NetLevel.Info, "");

            if (string.Compare(updater, originaldata.Assignee, true) == 0
                || string.Compare(updater, originaldata.Reporter, true) == 0
                || string.Compare(updater, originaldata.Creator, true) == 0)
            { }
            else
            {
                if (!string.IsNullOrEmpty(Request.Form["editor1"]))
                {
                    var issuecomment = new IssueComments();
                    issuecomment.Comment = SeverHtmlDecode.Decode(this, Request.Form["editor1"]);
                    IssueViewModels.StoreIssueComment(originaldata.IssueKey, issuecomment.dbComment, updater, COMMENTTYPE.Description);
                }

                var dict2 = new RouteValueDictionary();
                dict2.Add("issuekey", originaldata.IssueKey);
                return RedirectToAction("UpdateIssue", "Issue", dict2);
            }

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
                var commenter = updater.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries)[0].Replace(".", " ");

                var towho = new List<string>();
                towho.Add(vm.Assignee);
                if (originaldata.Reporter.Contains("@"))
                    towho.Add(originaldata.Reporter);
                towho.AddRange(vm.RelativePeopleList);
                towho.Add(updater);
                if (vm.RelativePeopleList.Count > 0) towho.AddRange(vm.RelativePeopleList);
                var atlist = RetrieveUserFromComment(vm.Description);
                if (atlist.Count > 0) towho.AddRange(atlist);

                var commentcontent = System.Text.RegularExpressions.Regex.Replace(vm.Description.Replace("\"", "").Replace("&nbsp;", ""), "<.*?>", string.Empty).Trim();
                SendTaskCommentEmail(originaldata.IssueKey, vm.Summary, commenter, towho, commentcontent);
            }
            else
                vm.Description = "";

            if (!string.IsNullOrEmpty(Request.Form["rootcauseeditor"]))
            {
                var rootcause = SeverHtmlDecode.Decode(this,Request.Form["rootcauseeditor"]);
                var dbstr = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(rootcause));
                var commenttype = COMMENTTYPE.RootCause;
                IssueViewModels.StoreIssueComment(vm.IssueKey, dbstr, vm.Reporter, commenttype);
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
                        ShareDocVM.ShareDoc(originaldata.ProjectKey, ShareDocType.DOCUMENT, dockey, attachtag, updater, DateTime.Now.ToString(), "/Issue/UpdateIssue?issuekey=" + vm.IssueKey);

                        UserKPIVM.AddUserAttachDailyRank(vm.IssueKey, updater, UserRankType.ADDITIONAL
                        , "Add attachement with tag to task: " + dockey, "/Issue/UpdateIssue?issuekey=" + vm.IssueKey, 2, dockey, this);
                    }
                    else
                    {
                        var tempkeys = url.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                        var dockey = tempkeys[tempkeys.Length - 1];

                        UserKPIVM.AddUserAttachDailyRank(vm.IssueKey, updater, UserRankType.ADDITIONAL
                        , "Add attachement to task: " + dockey, "/Issue/UpdateIssue?issuekey=" + vm.IssueKey, 1, dockey, this);
                    }
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

                    IssueViewModels.StoreIssueAttachment(vm.IssueKey, linkstr,ISSUEATTACHTYPE.CustomRMA);
                    if (!string.IsNullOrEmpty(customertag))
                    {
                        var tempkeys = url.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                        var dockey = tempkeys[tempkeys.Length - 1];
                        ShareDocVM.ShareDoc(originaldata.ProjectKey, ShareDocType.DOCUMENT, dockey, customertag, updater, DateTime.Now.ToString(), "/Issue/UpdateIssue?issuekey=" + vm.IssueKey);

                        UserKPIVM.AddUserAttachDailyRank(vm.IssueKey, updater, UserRankType.ADDITIONAL
                        , "Add attachement with tag to task: " + dockey, "/Issue/UpdateIssue?issuekey=" + vm.IssueKey, 2, dockey, this);
                    }
                    else
                    {
                        var tempkeys = url.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                        var dockey = tempkeys[tempkeys.Length - 1];
                        UserKPIVM.AddUserAttachDailyRank(vm.IssueKey, updater, UserRankType.ADDITIONAL
                        , "Add attachement to task: " + dockey, "/Issue/UpdateIssue?issuekey=" + vm.IssueKey, 2, dockey, this);
                    }
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

                    IssueViewModels.StoreIssueAttachment(vm.IssueKey, linkstr,ISSUEATTACHTYPE.InternalRMA);
                    if (!string.IsNullOrEmpty(internaltag))
                    {
                        var tempkeys = url.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                        var dockey = tempkeys[tempkeys.Length - 1];
                        ShareDocVM.ShareDoc(originaldata.ProjectKey, ShareDocType.DOCUMENT, dockey, internaltag, updater, DateTime.Now.ToString(), "/Issue/UpdateIssue?issuekey=" + vm.IssueKey);

                        UserKPIVM.AddUserAttachDailyRank(vm.IssueKey, updater, UserRankType.ADDITIONAL
                        , "Add attachement with tag to task: " + dockey, "/Issue/UpdateIssue?issuekey=" + vm.IssueKey, 2, dockey, this);
                    }
                    else
                    {
                        var tempkeys = url.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                        var dockey = tempkeys[tempkeys.Length - 1];

                        UserKPIVM.AddUserAttachDailyRank(vm.IssueKey, updater, UserRankType.ADDITIONAL
                        , "Add attachement to task: " + dockey, "/Issue/UpdateIssue?issuekey=" + vm.IssueKey, 2, dockey, this);
                    }
                }
            }

            vm.UpdateRMA();

            //ProjectEvent.OperateIssueEvent(originaldata.ProjectKey, updater, "Updated", originaldata.Summary, originaldata.IssueKey);

            if (string.Compare(originaldata.Assignee, vm.Assignee, true) != 0)
            {
                vm.UpdateIAssign();
                //ProjectEvent.AssignIssueEvent(originaldata.ProjectKey, updater, vm.Assignee, originaldata.Summary, originaldata.IssueKey);
                vm.Summary = originaldata.Summary;
                SendTaskEvent(vm, "changed", updater, vm.Assignee);
            }

            if (string.Compare(originaldata.Resolution, vm.Resolution, true) != 0)
            {
                if (vm.IssueClosed())
                {
                    UserKPIVM.AddUserDailyRank(vm.IssueKey, vm.Assignee, UserRankType.BASE
                        , "Close RMA Task: " + vm.Summary, "/Issue/UpdateIssue?issuekey=" + vm.IssueKey, 4);

                    if (!string.IsNullOrEmpty(issuetag))
                    {
                        ShareDocVM.ShareDoc(originaldata.ProjectKey, ShareDocType.ISSUE, originaldata.IssueKey, issuetag, updater, DateTime.Now.ToString(), "/Issue/UpdateIssue?issuekey=" + vm.IssueKey);
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

                var asilist = UserViewModels.RetrieveAllUser();
                ViewBag.towholist = CreateSelectList(asilist, "");
                ViewBag.towholist1 = CreateSelectList(asilist, "");

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

                ViewBag.tobechoosetags = ShareDocVM.RetrieveShareTags(this);
                CreateAllLists(ret);

                var asilist = UserViewModels.RetrieveAllUser();
                ViewBag.towholist = CreateSelectList(asilist, "");
                ViewBag.towholist1 = CreateSelectList(asilist, "");

                return View(ret);
            }
            else
            {
                var tempvm = new IssueViewModels();
                CreateAllLists(tempvm);

                var asilist = UserViewModels.RetrieveAllUser();
                ViewBag.towholist = CreateSelectList(asilist, "");
                ViewBag.towholist1 = CreateSelectList(asilist, "");

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

            //write log
            LogVM.WriteLog(updater.ToUpper(), originaldata.ProjectKey, DetermineCompName(Request.UserHostName),
                Request.Url.ToString(), "REL", "Update", issuekey, LogType.Task, Log4NetLevel.Info, "");

            if (string.Compare(updater, originaldata.Assignee, true) == 0
                || string.Compare(updater, originaldata.Reporter, true) == 0
                || string.Compare(updater, originaldata.Creator, true) == 0)
            { }
            else
            {
                if (!string.IsNullOrEmpty(Request.Form["editor1"]))
                {
                    var issuecomment = new IssueComments();
                    issuecomment.Comment = SeverHtmlDecode.Decode(this, Request.Form["editor1"]);
                    IssueViewModels.StoreIssueComment(originaldata.IssueKey, issuecomment.dbComment, updater, COMMENTTYPE.Description);
                }

                var dict2 = new RouteValueDictionary();
                dict2.Add("issuekey", originaldata.IssueKey);
                return RedirectToAction("UpdateIssue", "Issue", dict2);
            }

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




            var originalassignee = originaldata.Assignee;
            var originaldataresolution = originaldata.Resolution;
            if (string.Compare(originaldata.Reporter, updater, true) == 0
                || string.Compare(originaldata.Assignee, updater, true) == 0
                || string.Compare(originaldata.Creator, updater, true) == 0)
            {
                originaldata.Assignee = Request.Form["assigneelist"].ToString();
                originaldata.Resolution = Request.Form["resolutionlist"].ToString();
                originaldata.RelativePeoples = Request.Form["RPeopleAddr"];
                originaldata.FVCode = Request.Form["FVCode"];
                originaldata.Priority = Request.Form["prioritylist"].ToString();
                originaldata.DueDate = DateTime.Parse(Request.Form["DueDate"]);
            }

            if (!string.IsNullOrEmpty(Request.Form["editor1"]))
            {
                originaldata.Description = SeverHtmlDecode.Decode(this,Request.Form["editor1"]);
                originaldata.CommentType = COMMENTTYPE.Description;
            }
            else
                originaldata.Description = "";


            var issuetag = string.Empty;
            for (var i = 0; i < 200; i++)
            {
                if (Request.Form["issuetagcheck" + i] != null)
                {
                    issuetag = issuetag + Request.Form["issuetagcheck" + i] + ";";
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
                    IssueViewModels.StoreIssueAttachment(originaldata.IssueKey, url);
                    if (!string.IsNullOrEmpty(attachtag))
                    {
                        var tempkeys = url.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                        var dockey = tempkeys[tempkeys.Length - 1];
                        ShareDocVM.ShareDoc(originaldata.ProjectKey, ShareDocType.DOCUMENT, dockey, attachtag, updater, DateTime.Now.ToString(), "/Issue/UpdateIssue?issuekey=" + originaldata.IssueKey);

                        UserKPIVM.AddUserAttachDailyRank(originaldata.IssueKey, updater, UserRankType.ADDITIONAL
                        , "Add attachement with tag to task: " + dockey, "/Issue/UpdateIssue?issuekey=" + originaldata.IssueKey, 2, dockey, this);
                    }
                    else
                    {
                        var tempkeys = url.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                        var dockey = tempkeys[tempkeys.Length - 1];

                        UserKPIVM.AddUserAttachDailyRank(originaldata.IssueKey, updater, UserRankType.ADDITIONAL
                        , "Add attachement to task: " + dockey, "/Issue/UpdateIssue?issuekey=" + originaldata.IssueKey, 1, dockey, this);
                    }
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
                    UserKPIVM.AddUserDailyRank(originaldata.IssueKey, originaldata.Assignee, UserRankType.BASE
                        , "Close REL Task: " + originaldata.Summary, "/Issue/UpdateIssue?issuekey=" + originaldata.IssueKey, 4);

                    if (!string.IsNullOrEmpty(issuetag))
                    {
                        ShareDocVM.ShareDoc(originaldata.ProjectKey, ShareDocType.ISSUE, originaldata.IssueKey, issuetag, updater, DateTime.Now.ToString(), "/Issue/UpdateIssue?issuekey=" + originaldata.IssueKey);
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
                var asilist = UserViewModels.RetrieveAllUser();
                ViewBag.towholist = CreateSelectList(asilist, "");
                ViewBag.towholist1 = CreateSelectList(asilist, "");
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

                ViewBag.tobechoosetags = ShareDocVM.RetrieveShareTags(this);
                CreateAllLists(ret);
                var asilist = UserViewModels.RetrieveAllUser();
                ViewBag.towholist = CreateSelectList(asilist, "");
                ViewBag.towholist1 = CreateSelectList(asilist, "");
                return View(ret);
            }
            else
            {
                var tempvm = new IssueViewModels();
                CreateAllLists(tempvm);
                var asilist = UserViewModels.RetrieveAllUser();
                ViewBag.towholist = CreateSelectList(asilist, "");
                ViewBag.towholist1 = CreateSelectList(asilist, "");
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

            //write log
            LogVM.WriteLog(updater.ToUpper(), originaldata.ProjectKey, DetermineCompName(Request.UserHostName),
                Request.Url.ToString(), "OBA", "Update", issuekey, LogType.Task, Log4NetLevel.Info, "");

            if (string.Compare(updater, originaldata.Assignee, true) == 0
                || string.Compare(updater, originaldata.Reporter, true) == 0
                || string.Compare(updater, originaldata.Creator, true) == 0)
            { }
            else
            {
                    if (!string.IsNullOrEmpty(Request.Form["editor1"]))
                    {
                        var issuecomment = new IssueComments();
                        issuecomment.Comment = SeverHtmlDecode.Decode(this, Request.Form["editor1"]);
                        IssueViewModels.StoreIssueComment(originaldata.IssueKey, issuecomment.dbComment, updater, COMMENTTYPE.Description);
                    }

                    var dict2 = new RouteValueDictionary();
                    dict2.Add("issuekey", originaldata.IssueKey);
                    return RedirectToAction("UpdateIssue", "Issue", dict2);
            }

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
            }
            else
                vm.Description = "";

            if (!string.IsNullOrEmpty(Request.Form["rootcauseeditor"]))
            {
                var rootcause = SeverHtmlDecode.Decode(this,Request.Form["rootcauseeditor"]);
                var dbstr = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(rootcause));
                var commenttype = COMMENTTYPE.RootCause;
                IssueViewModels.StoreIssueComment(vm.IssueKey, dbstr, vm.Reporter, commenttype);
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
                    if (!string.IsNullOrEmpty(attachtag))
                    {
                        var tempkeys = url.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                        var dockey = tempkeys[tempkeys.Length - 1];
                        ShareDocVM.ShareDoc(originaldata.ProjectKey, ShareDocType.DOCUMENT, dockey, attachtag, updater, DateTime.Now.ToString(), "/Issue/UpdateIssue?issuekey=" + vm.IssueKey);
                        UserKPIVM.AddUserAttachDailyRank(vm.IssueKey, updater, UserRankType.ADDITIONAL
                       , "Add attachement with tag to task: " + dockey, "/Issue/UpdateIssue?issuekey=" + vm.IssueKey, 2, dockey, this);
                    }
                    else
                    {
                        var tempkeys = url.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                        var dockey = tempkeys[tempkeys.Length - 1];
                        UserKPIVM.AddUserAttachDailyRank(vm.IssueKey, updater, UserRankType.ADDITIONAL
                       , "Add attachement to task: " + dockey, "/Issue/UpdateIssue?issuekey=" + vm.IssueKey, 1, dockey, this);
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

                foreach (var subissue in originaldata.SubIssues)
                {
                    subissue.Assignee = vm.Assignee;
                    subissue.UpdateIssue();
                }
            }

            if (string.Compare(originaldata.Resolution, vm.Resolution, true) != 0)
            {
                if (vm.IssueClosed())
                {
                    UserKPIVM.AddUserDailyRank(vm.IssueKey, vm.Assignee, UserRankType.BASE
                        , "Close OBA Task: " + vm.Summary, "/Issue/UpdateIssue?issuekey=" + vm.IssueKey, 4);

                    if (!string.IsNullOrEmpty(issuetag))
                    {
                        ShareDocVM.ShareDoc(originaldata.ProjectKey, ShareDocType.ISSUE, originaldata.IssueKey, issuetag, updater, DateTime.Now.ToString(), "/Issue/UpdateIssue?issuekey=" + originaldata.IssueKey);
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
            //ViewBag.tobechoosetags = ShareDocVM.RetrieveShareTags(this);
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
                ViewBag.tobechoosetags = ShareDocVM.RetrieveShareTags(this);

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

            //write log
            LogVM.WriteLog(updater.ToUpper(), originaldata.ProjectKey, DetermineCompName(Request.UserHostName),
                Request.Url.ToString(), "Quality", "Update", issuekey, LogType.Task, Log4NetLevel.Info, "");

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
            }
            else
                vm.Description = "";

            if (!string.IsNullOrEmpty(Request.Form["analysiseditor"]))
            {
                var analysis = SeverHtmlDecode.Decode(this,Request.Form["analysiseditor"]);
                var dbstr = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(analysis));
                var commenttype = COMMENTTYPE.Analysis;
                IssueViewModels.StoreIssueComment(vm.IssueKey, dbstr, vm.Reporter, commenttype);
            }

            if (!string.IsNullOrEmpty(Request.Form["rootcauseeditor"]))
            {
                var rootcause = SeverHtmlDecode.Decode(this,Request.Form["rootcauseeditor"]);
                var dbstr = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(rootcause));
                var commenttype = COMMENTTYPE.RootCause;
                IssueViewModels.StoreIssueComment(vm.IssueKey, dbstr, vm.Reporter, commenttype);
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
                        ShareDocVM.ShareDoc(originaldata.ProjectKey, ShareDocType.DOCUMENT, dockey, attachtag, updater, DateTime.Now.ToString(), "/Issue/UpdateIssue?issuekey=" + vm.IssueKey);

                        UserKPIVM.AddUserAttachDailyRank(vm.IssueKey, updater, UserRankType.ADDITIONAL
                        , "Add attachement with tag to task: " + dockey, "/Issue/UpdateIssue?issuekey=" + vm.IssueKey, 2, dockey, this);
                    }
                    else
                    {
                        var tempkeys = url.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                        var dockey = tempkeys[tempkeys.Length - 1];

                        UserKPIVM.AddUserAttachDailyRank(vm.IssueKey, updater, UserRankType.ADDITIONAL
                        , "Add attachement to task: " + dockey, "/Issue/UpdateIssue?issuekey=" + vm.IssueKey, 1, dockey, this);
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
                    UserKPIVM.AddUserDailyRank(vm.IssueKey, vm.Assignee, UserRankType.BASE
                        , "Close Quaulity Task: " + vm.Summary, "/Issue/UpdateIssue?issuekey="+vm.IssueKey, 2);

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

        public ActionResult ShowVerifyAction(string verifytype, string issuekey)
        {
            if (!string.IsNullOrEmpty(issuekey))
            {
                if (string.Compare(verifytype, RELSubIssueType.FAILVERIFYACTION) == 0)
                {
                    var vm = IssueViewModels.RetrieveIssueByIssueKey(issuekey, this);
                    ViewBag.pagetitle = "Failure Verification";
                    return View(vm.FailureVerifyActions);
                }
                else
                {
                    var vm = IssueViewModels.RetrieveIssueByIssueKey(issuekey, this);
                    ViewBag.pagetitle = "Verification 4 Corrective";
                    return View(vm.CorrectiveVerifyActions);
                }
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

                if (!string.IsNullOrEmpty(tobedata.ModuleSN))
                {
                    SameAsDBTVM.StoreSameAsIssue(targetdata.IssueKey, tobedata.IssueKey, tobedata.ModuleSN);
                }

                if (targetdata.CommentList.Count > 0)
                {
                    var targetcomment = targetdata.CommentList[0];
                    if (!targetcomment.Comment.Contains("<p>"))
                    {
                        targetcomment.Comment = targetcomment.Comment + "<p>&nbsp;</p>";
                    }
                    targetcomment.Comment = targetcomment.Comment + "<div class=\"col-lg-2\">" + "<a href=\"/Issue/UpdateIssue?issuekey=" + key + "\">" + tobedata.ModuleSN + "</a>" + "</div>";
                    IssueViewModels.UpdateSPComment(targetcomment.IssueKey, targetcomment.CommentType, targetcomment.CommentDate.ToString(), targetcomment.dbComment);
                }
                
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

            ret.Add(url);
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


            if (!string.IsNullOrEmpty(Request.Form["editor1"]))
            {
                var tempcommment = new IssueComments();
                tempcommment.Comment = SeverHtmlDecode.Decode(this,Request.Form["editor1"]);
                IssueViewModels.UpdateSPComment(issuekey, commenttype, commentdate, tempcommment.dbComment);
            }
            else
            {
                var tempcommment = new IssueComments();
                tempcommment.Comment = "<p>To Be Edit</p>";
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
                    + "\""+item.ReportDate.ToString("yyyy-MM-dd HH:mm:ss") + "\"," 
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
                    + "\"" + item.Assignee.Replace("\"", "") + "\"," + "\"" + item.Reporter.Replace("\"", "") + "\"," + "\"" + item.ReportDate.ToString("yyyy-MM-dd HH:mm:ss") + "\"," + "\"" + item.CReport.Replace("\"", "") + "\","
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

        public ActionResult MoveTask2Working(string issuekey,string myaction)
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {

            }
            else
            {
                var ck = new Dictionary<string, string>();
                ck.Add("logonredirectctrl", "User");
                ck.Add("logonredirectact", "AddBlogDoc");
                CookieUtility.SetCookie(this, ck);
                return RedirectToAction("LoginUser", "User");
            }
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

            var originalissue = IssueViewModels.RetrieveIssueByIssueKey(issuekey, this);
            if (string.Compare(originalissue.Reporter.ToUpper(), updater.ToUpper()) == 0
                || string.Compare(originalissue.Assignee.ToUpper(), updater.ToUpper()) == 0)
            {
                originalissue.Resolution = Resolute.Working;
                originalissue.UpdateIssue();
            }

            if (!string.IsNullOrEmpty(myaction)
                && string.Compare(myaction, "Assign2Me") == 0)
            {
                var dict = new RouteValueDictionary();
                dict.Add("username", updater);
                return RedirectToAction("Assign2Me", "User", dict);
            }
            else
            {
                var dict = new RouteValueDictionary();
                dict.Add("ProjectKey", originalissue.ProjectKey);
                return RedirectToAction("ProjectIssues", "Project", dict);
            }

        }

        public static string DetermineCompName(string IP)
        {
            try
            {
                IPAddress myIP = IPAddress.Parse(IP);
                IPHostEntry GetIPHost = Dns.GetHostEntry(myIP);
                List<string> compName = GetIPHost.HostName.ToString().Split('.').ToList();
                return compName.First();
            }
            catch (Exception ex)
            { return string.Empty; }
        }

        [HttpPost]
        public JsonResult GetICare()
        {
            var ret = new JsonResult();
            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {
                var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
                var pkey = Request.Form["pkey"];
                var ikey = Request.Form["ikey"];

                var data = IssueIcareVM.GetIssueIcare(pkey, ikey, updater);
                if(data.Icare == 1)
                {
                    ret.Data = new { success = true };
                    return ret;
                }
            }

            ret.Data = new { success = false };
            return ret;
        }

        [HttpPost]
        public JsonResult CancelICare()
        {
            var ret = new JsonResult();
            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {
                var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
                var pkey = Request.Form["pkey"];
                var ikey = Request.Form["ikey"];

                IssueIcareVM.CancelICare(pkey, ikey, updater);

                LogVM.WriteLog(updater.ToUpper(), pkey, DetermineCompName(Request.UserHostName),
                        Request.Url.ToString(), "Issue", "Cancel ICare", ikey, LogType.CancelICare, Log4NetLevel.Info, "");

                ret.Data = new { success = true };
                return ret;
            }
            else
            {
                ret.Data = new { success = false };
                return ret;
            }
        }

        [HttpPost]
        public JsonResult ICare()
        {
            var ret = new JsonResult();
            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {
                var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
                var pkey = Request.Form["pkey"];
                var ikey = Request.Form["ikey"];

                IssueIcareVM.ICare(pkey, ikey, updater);

                LogVM.WriteLog(updater.ToUpper(), pkey, DetermineCompName(Request.UserHostName),
                        Request.Url.ToString(), "Issue", "ICare", ikey, LogType.ICare, Log4NetLevel.Info, "");

                ret.Data = new { success = true };
                return ret;
            }
            else
            {
                ret.Data = new { success = false };
                return ret;
            }
        }
    }
}