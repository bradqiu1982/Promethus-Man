using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.Web.Routing;
using System.Collections.Specialized;
using Prometheus.Models;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Threading;
using System.IO;

namespace Prometheus.Controllers
{
    public class UserController : Controller
    {

        private void IsSuper(string username)
        {
            var userdict = UserMatrixVM.RetrieveUserMatrixAuth();
            if (userdict.ContainsKey(username.ToUpper()))
            {
                if (string.Compare(userdict[username.ToUpper()].ToUpper(), USERAUTH.SUPER.ToUpper()) == 0)
                {
                    ViewBag.IsSuper = true;
                    return;
                }//end if
            }//end if
            ViewBag.IsSuper = false;
        }

        private bool checkexistuser(string user)
        {
            return false;
        }


        public ActionResult RegisterUser()
        {
            return View();
        }
         
        private void SendActiveEmail(string username,string updatetime)
        {
            var routevalue = new RouteValueDictionary();
            routevalue.Add("validatestr", Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes(username + "||" + updatetime)));
            //send validate email
            string scheme = this.Url.RequestContext.HttpContext.Request.Url.Scheme;
            string validatestr = this.Url.Action("ActiveUser", "User", routevalue, scheme);

            var netcomputername = "";
            try { netcomputername = System.Net.Dns.GetHostName(); }
            catch (Exception ex) { }
            validatestr = validatestr.Replace("//localhost", "//" + netcomputername);

            var toaddrs = new List<string>();
            toaddrs.Add(username);
            EmailUtility.SendEmail(this,"WUXI NPI System", toaddrs, validatestr);
        }

        [HttpPost, ActionName("RegisterUser")]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterUserPOST()
        {

           if (UserViewModels.CheckUserExist(Request.Form["Email"]))
            {
                var createerror = "<h3><font color=\"red\">Fail to create User: User Exist</font></h3>";
                ViewBag.CreateError = createerror;
                return View();
            }

            var username = Request.Form["Email"];
            var password = Request.Form["Password"];
            string updatetime = DateTime.Now.ToString();

            var user = new UserViewModels();
            user.Email = username.ToUpper();
            user.Password = password;
            user.UpdateDate = DateTime.Parse(updatetime);
            user.RegistUser();

            SendActiveEmail(username, updatetime);

            return RedirectToAction("ValidateNoticeA");
        }

        public ActionResult ValidateNoticeA()
        {
            return View();
        }

        public ActionResult ValidateNoticeB()
        {
            return View();
        }

        public ActionResult ResetNoticeA()
        {
            return View();
        }

        public ActionResult ResetNoticeB()
        {
            return View();
        }

        public ActionResult ActiveUser(string validatestr)
        {
            if (string.IsNullOrEmpty(validatestr))
            {
                var createerror = "<h3><font color=\"red\">Fail to active User: active string is empty</font></h3>";
                ViewBag.CreateError = createerror;
                RedirectToAction("RegisterUser");
            }
            
            var bs = Convert.FromBase64String(validatestr);
            var val = UTF8Encoding.UTF8.GetString(bs);
            var username = val.Split(new string[] { "||" },StringSplitOptions.None)[0];
            var updatetime = val.Split(new string[] { "||" }, StringSplitOptions.None)[1];
            UserViewModels.ActiveUser(username);
            return RedirectToAction("ValidateNoticeB");
        }

        public ActionResult ResetUser(string resetstr)
        {
            if (string.IsNullOrEmpty(resetstr))
            {
                var createerror = "<h3><font color=\"red\">Fail to reset User: reset string is empty</font></h3>";
                ViewBag.CreateError = createerror;
                return RedirectToAction("RegisterUser");
            }

            try
            {
                var bs = Convert.FromBase64String(resetstr);
                var val = UTF8Encoding.UTF8.GetString(bs);
                var username = val.Split(new string[] { "||" }, StringSplitOptions.None)[0];
                var updatetime = val.Split(new string[] { "||" }, StringSplitOptions.None)[1];
                var vm = new UserViewModels();
                vm.Email = username;
                vm.Password = "";
                vm.ConfirmPassword = "";
                return View(vm);
            }
            catch (Exception ex)
            {
                var createerror = "<h3><font color=\"red\">Fail to reset User: reset string is wrong</font></h3>";
                ViewBag.CreateError = createerror;
                return RedirectToAction("RegisterUser");
            }

        }

        [HttpPost, ActionName("ResetUser")]
        [ValidateAntiForgeryToken]
        public ActionResult ResetUserPOST()
        {
            var username = Request.Form["Email"];
            var password = Request.Form["Password"];
            UserViewModels.RestPwd(username, password);
            UserViewModels.ActiveUser(username);
            return RedirectToAction("ResetNoticeB");
        }


        private ActionResult SendResetEmail(string username)
        {
            string updatetime = DateTime.Now.ToString();
            var routevalue = new RouteValueDictionary();
            routevalue.Add("resetstr", Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes(username + "||" + updatetime)));
            //send validate email
            string scheme = this.Url.RequestContext.HttpContext.Request.Url.Scheme;
            string validatestr = this.Url.Action("ResetUser", "User", routevalue, scheme);
            var netcomputername = "";
            try { netcomputername = System.Net.Dns.GetHostName(); }
            catch (Exception ex) { }
            validatestr = validatestr.Replace("//localhost", "//" + netcomputername);

            var toaddrs = new List<string>();
            toaddrs.Add(username);
            EmailUtility.SendEmail(this,"WUXI NPI System", toaddrs, validatestr);
            return RedirectToAction("ResetNoticeA");
        }

        public ActionResult LoginUser()
        {
            return View();
        }

        private ActionResult NormalLogin(string username,string dbpwd,string inputpwd)
        {
            if (string.Compare(dbpwd, inputpwd) != 0)
            {
                var loginerror = "<h3><font color=\"red\">Fail to login: password not correct</font></h3>";
                ViewBag.loginerror = loginerror;
                return View("LoginUser");
            }

            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonredirectctrl") 
                && ckdict.ContainsKey("logonredirectact")
                && !string.IsNullOrEmpty(ckdict["logonredirectact"])
                && !string.IsNullOrEmpty(ckdict["logonredirectctrl"]))
            {
                var logonredirectact = ckdict["logonredirectact"];
                var logonredirectctrl = ckdict["logonredirectctrl"];

                //verify user information
                string logonuser = username + "||" + DateTime.Now.ToString();
                var ck = new Dictionary<string, string>();
                ck.Add("logonuser", logonuser);
                ck.Add("logonredirectact","");
                ck.Add("logonredirectctrl","");
                CookieUtility.SetCookie(this, ck);

                UserRankViewModel.UpdateUserRank(username,1);

                return RedirectToAction(logonredirectact, logonredirectctrl);
            }
            else
            {
                //verify user information
                string logonuser = username + "||" + DateTime.Now.ToString();
                var ck = new Dictionary<string, string>();
                ck.Add("logonuser", logonuser);
                CookieUtility.SetCookie(this, ck);

                UserRankViewModel.UpdateUserRank(username, 1);

                return RedirectToAction("UserCenter", "User");
            }
        }

        public static void RegisterUserAuto(string name)
        {
            var dbret = UserViewModels.RetrieveUser(name);
            if (dbret == null)
            {
                var tempname = name.ToUpper();
                if (!name.Contains("@"))
                {
                    tempname = (name.Replace(" ", ".") + "@finisar.com").ToUpper();
                }

                var user = new UserViewModels();
                user.Email = tempname;
                user.Password = "abc@123";
                user.UpdateDate = DateTime.Now;
                user.RegistUser();
                UserViewModels.ActiveUser(user.Email);
            }
        }

        [HttpPost, ActionName("LoginUser")]
        [ValidateAntiForgeryToken]
        public ActionResult LoginUserPOST()
        {
            var username = Request.Form["Email"].ToUpper();
            var password = Request.Form["Password"];

            var dbret = UserViewModels.RetrieveUser(username);
            if (dbret == null)
            {
                if (string.Compare(password, "abc@123", true) == 0)
                {
                    var user = new UserViewModels();
                    user.Email = username.ToUpper();
                    user.Password = password;
                    user.UpdateDate = DateTime.Now;
                    user.RegistUser();
                    UserViewModels.ActiveUser(user.Email);

                    dbret = UserViewModels.RetrieveUser(username);
                }
                else
                {
                    var loginerror = "<h3><font color=\"red\">Fail to login: user not exist</font></h3>";
                    ViewBag.loginerror = loginerror;
                    return View();
                }
            }

            if (dbret.Validated == 0)
            {
                var loginerror = "<h3><font color=\"red\">Fail to login: user is not actived</font></h3>";
                ViewBag.loginerror = loginerror;
                string updatetime = DateTime.Now.ToString();
                UserViewModels.UpdateUserTime(username, DateTime.Parse(updatetime));
                SendActiveEmail(username, updatetime);
                return RedirectToAction("ValidateNoticeA");
            }

            if (Request.Form["Login"] != null)
            {
                return NormalLogin(username, dbret.Password, password);
            }
            else if (Request.Form["ForgetPassword"] != null)
            {
                return SendResetEmail(username);
            }
            else
            {
                return View();
            }
        }


        public ActionResult LoginOutUser(string ctrl, string action)
        {
            var val = CookieUtility.UnpackCookie(this);
            val["logonuser"] = "";
            CookieUtility.SetCookie(this, val);
            return RedirectToAction("ViewAll", "Project");
        }

        private List<SelectListItem> CreateSelectList(List<string> valist, string defVal)
        {
            bool selected = false;
            var pslist = new List<SelectListItem>();
            foreach (var p in valist)
            {
                var pitem = new SelectListItem();
                pitem.Text = p;
                pitem.Value = p;
                if (!string.IsNullOrEmpty(defVal) && string.Compare(defVal, p, true) == 0)
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

        private void CreateICareList()
        {
            var asilist = UserViewModels.RetrieveAllUser();
            var slist = CreateSelectList(asilist, "");
            ViewBag.chooseuserlist = slist;

            var months = new string[] {"1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "18" };
            asilist = new List<string>();
            asilist.AddRange(months);
            slist = CreateSelectList(asilist, "");
            ViewBag.monthlist = slist;
        }


        public ActionResult UserCenter(string username,string month)
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {

            }
            else
            {
                var ck = new Dictionary<string, string>();
                ck.Add("logonredirectctrl", "User");
                ck.Add("logonredirectact", "UserCenter");
                CookieUtility.SetCookie(this, ck);
                return RedirectToAction("LoginUser", "User");
            }

            var usernm = "";
            if (!string.IsNullOrEmpty(username))
            {
                usernm = username;
            }
            else if(ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {
                usernm = ckdict["logonuser"].Split(new char[] { '|' })[0];
            }

            
            if (!string.IsNullOrEmpty(usernm))
            {
                //my blog

                //if (ckdict.Count > 0 && ckdict.ContainsKey("logonuser"))
                //{
                //    var updater = ckdict["logonuser"].Split(new char[] { '|' })[0].ToUpper();
                //    if (string.Compare(updater, usernm, true) == 0)
                //    {
                        ViewBag.myissuesummary = UserActionTrend.RetrieveMyIssuerSummary(this, usernm, 1);
                        ViewBag.myranksummary = UserActionTrend.RetrieveMyRankSummary(this,usernm,1);
                //    }
                    
                //}

                //asign to me
                var list1 = IssueViewModels.RetrieveIssueByAssignee(usernm, Resolute.Pending, 60);
                var list2 = IssueViewModels.RetrieveIssueByAssigneeWorking(usernm, Resolute.Working, 60);
                var list3 = IssueViewModels.RetrieveIssueByAssignee(usernm, Resolute.Done, 200);
                list1.AddRange(list2);
                list1.AddRange(list3);

                ViewBag.UserName = usernm.Split(new char[] { '@' })[0];

                //I assign
                var iassignissues= IssueViewModels.RetrieveIssueByCreator(usernm, 300, this);
                var wholedata = new List<List<string>>();
                var title = new List<string>();
                title.Add("Project");
                title.Add("Summary");
                title.Add("Priority");
                title.Add("IssueType");
                title.Add("Assignee");
                title.Add("Status");
                title.Add("Due Date");
                wholedata.Add(title);

                foreach (var item in iassignissues)
                {
                    var templine = new List<string>();
                    templine.Add(item.ProjectKey);
                    templine.Add(item.Summary);
                    templine.Add(item.Priority);
                    templine.Add(item.Assignee);
                    templine.Add(item.Resolution);
                    templine.Add(item.DueDate.ToString("MM/dd-yy"));
                    templine.Add(item.IssueKey);
                    templine.Add("MAIN");
                    templine.Add(item.IssueType);

                    wholedata.Add(templine);
                    foreach (var item1 in item.SubIssues)
                    {
                        templine = new List<string>();
                        templine.Add(item1.ProjectKey);
                        templine.Add(item1.Summary);
                        templine.Add(item1.Priority);
                        templine.Add(item1.Assignee);
                        templine.Add(item1.Resolution);
                        templine.Add(item1.DueDate.ToString("MM/dd-yy"));
                        templine.Add(item1.IssueKey);
                        templine.Add("SUB");
                        templine.Add(item1.IssueType);

                        wholedata.Add(templine);
                    }
                }

                ViewBag.iassignlist = wholedata;

                //I care
                if (ckdict.Count > 0 && ckdict.ContainsKey("logonuser"))
                {
               
                    var updater = ckdict["logonuser"].Split(new char[] { '|' })[0].ToUpper();
                    var tempmonth = 1;
                    if (string.IsNullOrEmpty(month))
                    {
                        tempmonth = 1;
                    }
                    else
                    {
                        try { tempmonth = Convert.ToInt32(month); }
                        catch (Exception ex) { tempmonth = 1; }

                    }

                    if (string.Compare(updater, usernm, true) == 0)
                    {
                        IsSuper(usernm);

                        CreateICareList();
                        ViewBag.icareissuelist = UserActionTrend.RetrieveICareUserIssue(this, updater, tempmonth);
                        ViewBag.icareranklist = UserActionTrend.RetrieveICareUserRank(this, updater, tempmonth);
                        ViewBag.icaremonth = tempmonth.ToString();
                    }
                }

                return View(list1);
            }
            else
            {
                return RedirectToAction("LoginUser", "User");
            }
        }

        [HttpPost, ActionName("OperateICare")]
        [ValidateAntiForgeryToken]
        public ActionResult OperateICare()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

            var dict = new RouteValueDictionary();
            if (Request.Form["adduser"] != null)
            {
                var other = Request.Form["chooseuserlist"].ToString();
                UserViewModels.AddICare(updater, other);
                dict.Add("month", "1");
            }

            if (Request.Form["deleteuser"] != null)
            {
                var other = Request.Form["chooseuserlist"].ToString();
                UserViewModels.RemoveICare(updater, other);
                dict.Add("month", "1");
            }

            if (Request.Form["viewuser"] != null)
            {
                dict.Add("month", Request.Form["monthlist"].ToString());
            }

            dict.Add("username", updater);
            
            return RedirectToAction("UserCenter", "User", dict);
        }

        public ActionResult UserIssues(string username,string month)
        {
            if (!string.IsNullOrEmpty(username))
            {
                ViewBag.UserName = username.Split(new char[] { '@' })[0];

                var tempmonth = 1;
                if (string.IsNullOrEmpty(month))
                {
                    tempmonth = 1;
                }
                else
                {
                    try { tempmonth = Convert.ToInt32(month); }
                    catch (Exception ex) { tempmonth = 1; }
                }

                var edate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " 07:30:00");
                var sdate = edate.AddMonths(0 - tempmonth);
                var pendingissues = IssueViewModels.RetrieveIssuePendingByUser(username, sdate.ToString());
                var workingissues = IssueViewModels.RetrieveIssueWorkingByUser(username, sdate.ToString());
                var solvedissues = IssueViewModels.RetrieveIssueDoneByUser(username, sdate.ToString(), this);
                workingissues.AddRange(solvedissues);
                workingissues.AddRange(pendingissues);

                var wholedata = new List<List<string>>();
                var title = new List<string>();
                title.Add("Project");
                title.Add("Summary");
                title.Add("Priority");
                title.Add("IssueType");
                title.Add("Status");
                title.Add("Due Date");
                title.Add("Attachment");
                wholedata.Add(title);

                foreach (var item in workingissues)
                {
                    var templine = new List<string>();
                    templine.Add(item.ProjectKey);
                    templine.Add(item.Summary);
                    templine.Add(item.Priority);
                    templine.Add(item.IssueType);
                    templine.Add(item.Resolution);
                    templine.Add(item.DueDate.ToString("MM/dd-yy"));
                    if (item.AttachList.Count > 0)
                    {
                        templine.Add("has");
                    }
                    else
                    {
                        templine.Add("");
                    }
                    templine.Add(item.IssueKey);

                    wholedata.Add(templine);
                }

                ViewBag.userissuelist = wholedata;

                return View();
            }

            return View();
        }


        public ActionResult AddShareTag()
        {
            var tagstr = string.Empty;
            var tags = ShareDocVM.RetrieveShareTags();

            foreach (var tag in tags)
            {
                tagstr = tagstr + tag + ";";
            }

            ViewBag.tagstr = tagstr;

            return View();
        }

        [HttpPost, ActionName("AddShareTag")]
        [ValidateAntiForgeryToken]
        public ActionResult AddShareTagPost()
        {
            var tagstr = Request.Form["querystring"];
            var tags = tagstr.Split(new string[] { ";","," }, StringSplitOptions.RemoveEmptyEntries);
            var taglist = new List<string>();
            taglist.AddRange(tags);
            ShareDocVM.StoreShareTags(taglist);

            var tags1 = ShareDocVM.RetrieveShareTags();
            var tagstr1 = string.Empty;
            foreach (var tag in tags1)
            {
                tagstr1 = tagstr1 + tag + ";";
            }

            ViewBag.tagstr = tagstr1;

            return View();
        }

        private List<SelectListItem> CreateSelectList(List<string> valist)
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
                pslist.Add(pitem);
            }

            if (!selected && pslist.Count > 0)
            {
                pslist[0].Selected = true;
            }

            return pslist;
        }

        private void createpjlist()
        {
            var projlist = ProjectViewModels.RetrieveAllProjectKey();
            ViewBag.projectlist= CreateSelectList(projlist);
        }

        private void createtaglist()
        {
            var tags = ShareDocVM.RetrieveShareTags();
            tags.Sort();
            ViewBag.taglist = CreateSelectList(tags);
        }

        public ActionResult IBook()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {

            }
            else
            {
                var ck = new Dictionary<string, string>();
                ck.Add("logonredirectctrl", "User");
                ck.Add("logonredirectact", "IBook");
                CookieUtility.SetCookie(this, ck);
                return RedirectToAction("LoginUser", "User");
            }

            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
            createpjlist();
            createtaglist();

            var usertaglist = new List<string>();
            var usertagdict = ShareDocVM.RetrieveUserBookedTag(updater).DOCTagDict;
            usertaglist.AddRange(usertagdict.Keys);
            if (usertaglist.Count > 0)
            {
                usertaglist.Sort();
                ViewBag.usertaglist = usertaglist;
            }
            return View();
        }


        [HttpPost, ActionName("AddUserShareTag")]
        [ValidateAntiForgeryToken]
        public ActionResult AddUserShareTag()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {

            }
            else
            {
                var ck = new Dictionary<string, string>();
                ck.Add("logonredirectctrl", "User");
                ck.Add("logonredirectact", "AddUserShareTag");
                CookieUtility.SetCookie(this, ck);
                return RedirectToAction("LoginUser", "User");
            }

            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
            var usertaglist = new List<string>();
            var usertagdict = ShareDocVM.RetrieveUserBookedTag(updater).DOCTagDict;
            usertaglist.AddRange(usertagdict.Keys);

            var pjtag = Request.Form["projectlist"].ToString();
            var normaltag = Request.Form["taglist"].ToString();

            if (string.Compare(pjtag, "NONE", true) != 0)
            {
                usertaglist.Add(pjtag);
            }

            if (string.Compare(normaltag, "NONE", true) != 0)
            {
                usertaglist.Add(normaltag);
            }

            var usertag = string.Empty;
            foreach (var tag in usertaglist)
            {
                usertag = usertag + tag + ";";
            }
            ShareDocVM.SetUserBookTag(updater, usertag);

            return RedirectToAction("IBook", "User");
        }

        [HttpPost, ActionName("UpdateUserShareTag")]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateUserShareTag()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {

            }
            else
            {
                var ck = new Dictionary<string, string>();
                ck.Add("logonredirectctrl", "User");
                ck.Add("logonredirectact", "UpdateUserShareTag");
                CookieUtility.SetCookie(this, ck);
                return RedirectToAction("LoginUser", "User");
            }

            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

            if (Request.Form["matchpostfile"] != null)
            {
                ShareDocVM.MatchAllPostDocForUser(updater,this);
                return RedirectToAction("ILearn");
            }
            
            var tags = string.Empty;
            for (var i = 0; i < 600; i++)
            {
                if (Request.Form["check" + i] != null)
                {
                    tags = tags + Request.Form["check" + i] + ";";
                }
            }
            ShareDocVM.SetUserBookTag(updater,tags);

            return RedirectToAction("IBook","User");
        }

        public ActionResult ILearn()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {

            }
            else
            {
                var ck = new Dictionary<string, string>();
                ck.Add("logonredirectctrl", "User");
                ck.Add("logonredirectact", "ILearn");
                CookieUtility.SetCookie(this, ck);
                return RedirectToAction("LoginUser", "User");
            }

            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
            var asilist = UserViewModels.RetrieveAllUser();
            ViewBag.towholist = CreateSelectList(asilist,"");

            var ilearntable = new List<ShareDocVM>();
            var iweeklytable = new List<ShareDocVM>();
            var allileanrn =  ShareDocVM.RetrieveMyLearn(updater,this);
            foreach (var item in allileanrn)
            {
                if (item.DOCTag.ToUpper().Contains("WEEKLYREPORT")
                    || item.DOCTag.ToUpper().Contains("WEEKLY REPORT"))
                {
                    iweeklytable.Add(item);
                }
                else
                {
                    ilearntable.Add(item);
                }
            }

            ViewBag.ILearn = ilearntable;
            ViewBag.IShare = ShareDocVM.RetrieveMyShare(updater,this);
            ViewBag.IWeeklyReport = iweeklytable;
            return View();
        }

        [HttpPost, ActionName("ILearnOperate")]
        [ValidateAntiForgeryToken]
        public ActionResult ILearnOperate()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

            var tobedockey = new List<string>();
            for (var i = 0; i < 600; i++)
            {
                if (Request.Form["check" + i] != null && string.Compare(Request.Form["check" + i], "true", true) == 0)
                {
                    tobedockey.Add(Request.Form["HDocKey" + i]);
                }
            }

            foreach (var key in tobedockey)
            {
                ShareDocVM.RemoveMyLearn(key, updater);
            }

            return RedirectToAction("ILearn", "User");
        }

        public ActionResult ILike(string DOCPJK, string DOCKey)
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {

            }
            else
            {
                var ck = new Dictionary<string, string>();
                ck.Add("logonredirectctrl", "User");
                ck.Add("logonredirectact", "ILearn");
                CookieUtility.SetCookie(this, ck);
                return RedirectToAction("LoginUser", "User");
            }

            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

            if (!string.IsNullOrEmpty(DOCPJK)
                && !string.IsNullOrEmpty(DOCKey))
            {
                ShareDocVM.LikeDoc(DOCPJK, DOCKey, updater);
            }
            return RedirectToAction("ILearn", "User");
        }

        public ActionResult RemoveBlogDoc(string DOCKey)
        {
            UserBlogVM.RemoveBlogDoc(DOCKey);
            ShareDocVM.RemoveDoc(DOCKey);
            return RedirectToAction("IBLOG", "User");
        }

        public ActionResult RemoveSharedDoc(string DOCPJK, string DOCKey)
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {

            }
            else
            {
                var ck = new Dictionary<string, string>();
                ck.Add("logonredirectctrl", "User");
                ck.Add("logonredirectact", "ILearn");
                CookieUtility.SetCookie(this, ck);
                return RedirectToAction("LoginUser", "User");
            }

            if (!string.IsNullOrEmpty(DOCPJK)
                && !string.IsNullOrEmpty(DOCKey))
            {
                ShareDocVM.RemoveDoc(DOCKey);
            }

            return RedirectToAction("ILearn", "User");
        }

        public ActionResult IPush(string DOCPJK, string DOCKey,string ToWho)
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {

            }
            else
            {
                var ck = new Dictionary<string, string>();
                ck.Add("logonredirectctrl", "User");
                ck.Add("logonredirectact", "ILearn");
                CookieUtility.SetCookie(this, ck);
                return RedirectToAction("LoginUser", "User");
            }
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

            var whoes = ToWho.Split(new string[] { ";", ","," " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var w in whoes)
            {
                ShareDocVM.IPushDoc(DOCPJK, DOCKey, w.Trim().ToUpper(),updater,this);
            }

            ShareDocVM.SendPushDocEvent("a new document about " + DOCKey, "/User/WebDoc?DocKey=" + HttpUtility.UrlEncode(DOCKey) + "&Creator=" + updater.ToUpper(), updater, updater, this);

            return RedirectToAction("ILearn", "User");
        }

        public ActionResult IBLOG()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {

            }
            else
            {
                var ck = new Dictionary<string, string>();
                ck.Add("logonredirectctrl", "User");
                ck.Add("logonredirectact", "IBLOG");
                CookieUtility.SetCookie(this, ck);
                return RedirectToAction("LoginUser", "User");
            }

            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

            var users = UserViewModels.RetrieveAllUser();
            ViewBag.towholist = CreateSelectList(users);

            var vm = UserBlogVM.RetrieveAllBlogDoc(updater);
            return View(vm);
        }

        public ActionResult IBLogPush(string DOCKey, string ToWho,string Reason)
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

            var vm = UserBlogVM.RetrieveBlogDoc(DOCKey,this);

            var whoes = ToWho.Split(new string[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var w in whoes)
            {
                //ShareDocVM.PushDoc(w.Trim().ToUpper(), "ALL",ShareDocType.BLOG, DOCKey,vm.Tag,vm.UserName,vm.CreateDate.ToString(), "");
                ShareDocVM.PushDoc(w.Trim().ToUpper(), "ALL", ShareDocType.BLOG, DOCKey, vm.Tag, vm.UserName, DateTime.Now.ToString(), "");
                ShareDocVM.SendPushDocEvent("a new document about " + vm.Tag, vm.DocURL, ToWho, updater.ToUpper(), this, tempreason);
            }

            ShareDocVM.SendPushDocEvent("a new document about " + vm.Tag, vm.DocURL, updater.ToUpper(), updater.ToUpper(), this, tempreason);

            return RedirectToAction("IBLOG", "User");
        }

        public ActionResult AddBlogDoc()
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

            var tobechoosetags = new List<string>();
            var usertag = UserBlogVM.RetrieveUserTag(updater);
            if (!usertag.ToUpper().Contains("WeeklyReport".ToUpper()))
            {
                tobechoosetags.Add("WeeklyReport".ToUpper());
            }
            if (!usertag.ToUpper().Contains("Default".ToUpper()))
            {
                tobechoosetags.Add("Default".ToUpper());
            }
            var usertags = usertag.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            if (usertags.Length > 0)
            {
                tobechoosetags.AddRange(usertags);
            }
            ViewBag.tobechoosetags = tobechoosetags;

            return View();
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
                            .Replace(" ", "_").Replace("#", "")
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


        [HttpPost, ActionName("AddBlogDoc")]
        [ValidateAntiForgeryToken]
        public ActionResult AddBlogDocPost()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

            var tag = Request.Form["Tag"];
            if (string.IsNullOrEmpty(tag))
            {
                tag = "Default;".ToUpper();
            }

            UserBlogVM.StoreUserTag(updater, tag);

            var urls = ReceiveRMAFiles();

            if (!string.IsNullOrEmpty(Request.Form["attachmentupload"]))
            {
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
                    var blog = new UserBlogVM();
                    blog.UserName = updater;
                    blog.ContentType = UserBlogContentType.ATTACH;
                    blog.Title = originalname + Path.GetExtension(internalreportfile);
                    blog.Content = url;
                    blog.Tag = tag;
                    blog.StoreBlogDoc();
                }
            }//end if

            var contenturl = string.Empty;
            var contentreffile = string.Empty;
            if (!string.IsNullOrEmpty(Request.Form["contentattach"]))
            {
                var internalreportfile = Request.Form["contentattach"];
                var originalname = Path.GetFileNameWithoutExtension(internalreportfile)
                    .Replace(" ", "_").Replace("#", "")
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

            if (!string.IsNullOrEmpty(Request.Form["docinputeditor"]))
            {
                var blog = new UserBlogVM();
                blog.UserName = updater;
                blog.ContentType = UserBlogContentType.COMMENT;
                blog.Content = SeverHtmlDecode.Decode(this,Request.Form["docinputeditor"]);

                if (!string.IsNullOrEmpty(contenturl))
                {
                    blog.Content = blog.Content + "<hr/><p><a href='" + contenturl + "' target='_blank'>Reference File: " + contentreffile + " " + "</a></p>";
                }

                blog.Tag = tag;
                if (string.IsNullOrEmpty(Request.Form["DocTitle"]))
                {
                    var drycontent = System.Text.RegularExpressions.Regex.Replace(blog.Content.Replace("\"", "").Replace("&nbsp;", ""), "<.*?>", string.Empty).Trim();
                    blog.Title = drycontent.Substring(0, 45);
                }
                else
                {
                    blog.Title = Request.Form["DocTitle"];
                }
                blog.StoreBlogDoc();
            }

            return RedirectToAction("IBLOG", "User");
        }

        public ActionResult ModifyBlogDoc(string DocKey)
        {
            var vm = UserBlogVM.RetrieveBlogDoc(DocKey,this);
            return View(vm);
        }

        [HttpPost, ActionName("ModifyBlogDoc")]
        [ValidateAntiForgeryToken]
        public ActionResult ModifyBlogDocPost()
        {
            var dockey = Request.Form["HDocKey"];
            var vm = UserBlogVM.RetrieveBlogDoc(dockey,this);


            var urls = ReceiveRMAFiles();
            var contenturl = string.Empty;
            var contentreffile = string.Empty;
            if (!string.IsNullOrEmpty(Request.Form["contentattach"]))
            {
                var internalreportfile = Request.Form["contentattach"];
                var originalname = Path.GetFileNameWithoutExtension(internalreportfile)
                    .Replace(" ", "_").Replace("#", "")
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
                vm.Content = SeverHtmlDecode.Decode(this,Request.Form["editor1"]);

                if (!string.IsNullOrEmpty(contenturl))
                {
                    vm.Content = vm.Content + "<p><a href='" + contenturl + "' target='_blank'>Reference File: " + contentreffile + " " + "</a></p>";
                }
                vm.UpdateBlogDoc();
            }
            else
            {
                vm.Content = "<p>To Be Edit</p>";

                if (!string.IsNullOrEmpty(contenturl))
                {
                    vm.Content = vm.Content + "<p><a href='" + contenturl + "' target='_blank'>Reference File: " + contentreffile + " " + "</a></p>";
                }
                vm.UpdateBlogDoc();
            }

            var dict = new RouteValueDictionary();
            dict.Add("DocKey", vm.DocKey);
            dict.Add("Creator", vm.UserName);
            return RedirectToAction("WebDoc", "User", dict);
        }

        public ActionResult WebDoc(string DocKey,string Creator = "")
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {

            }
            else
            {
                var ck = new Dictionary<string, string>();
                ck.Add("logonredirectctrl", "User");
                ck.Add("logonredirectact", "WebDoc");
                ck.Add("DocKey", DocKey);
                ck.Add("Creator", Creator);
                CookieUtility.SetCookie(this, ck);
                return RedirectToAction("LoginUser", "User");
            }

            if (!string.IsNullOrEmpty(DocKey))
            {
                var vm = UserBlogVM.RetrieveBlogDoc(DocKey,this);

                if (string.IsNullOrEmpty(vm.DocKey))
                {
                    var tempstrs = DocKey.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                    var datestr = tempstrs[tempstrs.Length - 1].Substring(0,8);
                    vm.ContentType = UserBlogContentType.ATTACH;
                    vm.Title = DocKey;
                    vm.Content = "/userfiles/docs/" + datestr + "/" + DocKey;
                    vm.DocKey = DocKey;
                    vm.UserName = Creator;
                    vm.DocURL = "/User/WebDoc?DocKey=" + DocKey + "&Creator=" + Creator;
                }

                return View(vm);
            }
            else
            {
                if (ckdict.ContainsKey("DocKey") && !string.IsNullOrEmpty(ckdict["DocKey"]))
                {
                    var vm = UserBlogVM.RetrieveBlogDoc(ckdict["DocKey"],this);

                    var tempDocKey = ckdict["DocKey"];
                    if (string.IsNullOrEmpty(vm.DocKey))
                    {
                        var tempstrs = tempDocKey.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                        var datestr = tempstrs[tempstrs.Length - 1].Substring(0, 8);
                        vm.ContentType = UserBlogContentType.ATTACH;
                        vm.Title = tempDocKey;
                        vm.Content = "/userfiles/docs/" + datestr + "/" + tempDocKey;
                        vm.DocKey = tempDocKey;
                        vm.UserName = Creator;
                        vm.DocURL = "/User/WebDoc?DocKey=" + DocKey + "&Creator=" + Creator;
                    }

                    return View(vm);
                }
            }

            return RedirectToAction("IBLOG", "User");
        }

        [HttpPost, ActionName("WebDoc")]
        [ValidateAntiForgeryToken]
        public ActionResult WebDocPost()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

            var dockey = Request.Form["DocKey"];
            var docurl = Request.Form["DOCURL"];
            var doccreator = Request.Form["DOCCREATOR"];

            var urls = ReceiveRMAFiles();
            var contenturl = string.Empty;
            var contentreffile = string.Empty;
            if (!string.IsNullOrEmpty(Request.Form["contentattach"]))
            {
                var internalreportfile = Request.Form["contentattach"];
                var originalname = Path.GetFileNameWithoutExtension(internalreportfile)
                    .Replace(" ", "_").Replace("#", "")
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

            if (!string.IsNullOrEmpty(Request.Form["docinputeditor"]))
            {
                var com = new ErrorComments();
                com.Comment = SeverHtmlDecode.Decode(this,Request.Form["docinputeditor"]);
                if (!string.IsNullOrEmpty(com.Comment))
                {
                    if (!string.IsNullOrEmpty(contenturl))
                    {
                        com.Comment = com.Comment + "<hr/><p><a href='" + contenturl + "' target='_blank'>Reference File: " + contentreffile + " " + "</a></p>";
                    }

                    ProjectErrorViewModels.StoreErrorComment(dockey, com.dbComment, PJERRORCOMMENTTYPE.Description, updater, DateTime.Now.ToString());
                    //send comment mesage
                    ShareDocVM.SendPushCommentEvent("a new comment", docurl, doccreator, updater, this);
                }
            }

            var dict = new RouteValueDictionary();
            dict.Add("DocKey", dockey);
            dict.Add("Creator", doccreator);
            return RedirectToAction("WebDoc", "User", dict);

            //var vm = UserBlogVM.RetrieveBlogDoc(dockey);
            //return View(vm);
        }

        public ActionResult UpdateWebDocComment(string ErrorKey, string CommentType, string Date,string Creator)
        {
            if (!string.IsNullOrEmpty(ErrorKey) && !string.IsNullOrEmpty(CommentType) && !string.IsNullOrEmpty(Date))
            {
                var ckdict = CookieUtility.UnpackCookie(this);
                var updater = ckdict["logonuser"].Split(new char[] { '|' })[0].ToUpper();

                var errorcomment = ProjectErrorViewModels.RetrieveSPComment(ErrorKey, CommentType, Date);
                if (string.Compare(updater, errorcomment.Reporter, true) != 0)
                {
                    var dict = new RouteValueDictionary();
                    dict.Add("DocKey", ErrorKey);
                    dict.Add("Creator", Creator);
                    return RedirectToAction("WebDoc", "User", dict);
                }

                ViewBag.Creator = Creator;
                return View(errorcomment);
            }

            return RedirectToAction("ViewAll", "Project");
        }

        [HttpPost, ActionName("UpdateWebDocComment")]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateWebDocCommentPost()
        {
            var errorkey = Request.Form["HErrorKey"];
            var commenttype = Request.Form["HType"];
            var commentdate = Request.Form["HDate"];
            var creator = Request.Form["HCreator"];

            var urls = ReceiveRMAFiles();
            var contenturl = string.Empty;
            var contentreffile = string.Empty;
            if (!string.IsNullOrEmpty(Request.Form["contentattach"]))
            {
                var internalreportfile = Request.Form["contentattach"];
                var originalname = Path.GetFileNameWithoutExtension(internalreportfile)
                    .Replace(" ", "_").Replace("#", "")
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
                var tempcommment = new ErrorComments();
                tempcommment.Comment = SeverHtmlDecode.Decode(this,Request.Form["editor1"]);

                if (!string.IsNullOrEmpty(contenturl))
                {
                    tempcommment.Comment = tempcommment.Comment + "<hr/><p><a href='" + contenturl + "' target='_blank'>Reference File: " + contentreffile + " " + "</a></p>";
                }
                ProjectErrorViewModels.UpdateSPComment(errorkey, commenttype, commentdate, tempcommment.dbComment);
            }
            else
            {
                var tempcommment = new ErrorComments();
                tempcommment.Comment = "<p>To Be Edit</p>";

                if (!string.IsNullOrEmpty(contenturl))
                {
                    tempcommment.Comment = tempcommment.Comment + "<hr/><p><a href='" + contenturl + "' target='_blank'>Reference File: " + contentreffile + " " + "</a></p>";
                }
                ProjectErrorViewModels.UpdateSPComment(errorkey, commenttype, commentdate, tempcommment.dbComment);
            }

            var dict = new RouteValueDictionary();
            dict.Add("DocKey", errorkey);
            dict.Add("Creator", creator);
            return RedirectToAction("WebDoc", "User", dict);
        }


        [HttpPost, ActionName("SaveCacheInfo")]
        public string SaveCacheInfo()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

            foreach (var key in Request.Form.Keys)
            {
                UserCacheVM.InsertCacheInfo(updater, Request.Form.Get(key.ToString()));
            }

            return "SAVED";
        }

        public ActionResult UserCachedInfo()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {

            }
            else
            {
                var ck = new Dictionary<string, string>();
                ck.Add("logonredirectctrl", "User");
                ck.Add("logonredirectact", "UserCachedInfo");
                CookieUtility.SetCookie(this, ck);
                return RedirectToAction("LoginUser", "User");
            }

            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
            var vm = UserCacheVM.RetrieveCacheInfo(updater);

            return View(vm);
        }

    }
}