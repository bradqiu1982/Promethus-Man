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

namespace Prometheus.Controllers
{
    public class UserController : Controller
    {

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
            EmailUtility.SendEmail("NPI Website Active Link",toaddrs, validatestr);
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
            EmailUtility.SendEmail("NPI Website Active Link", toaddrs, validatestr);
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
                var user = new UserViewModels();
                user.Email = name.ToUpper();
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
                var iassignissues= IssueViewModels.RetrieveIssueByCreator(usernm, 300);
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
                var solvedissues = IssueViewModels.RetrieveIssueDoneByUser(username, sdate.ToString());
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
            var usertagdict = ShareDocVM.RetrieveUserBookTag(updater).DOCTagDict;
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
                ck.Add("logonredirectact", "IBook");
                CookieUtility.SetCookie(this, ck);
                return RedirectToAction("LoginUser", "User");
            }

            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
            var usertaglist = new List<string>();
            var usertagdict = ShareDocVM.RetrieveUserBookTag(updater).DOCTagDict;
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

            return RedirectToAction("IBook");
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
                ck.Add("logonredirectact", "IBook");
                CookieUtility.SetCookie(this, ck);
                return RedirectToAction("LoginUser", "User");
            }

            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
            var tags = string.Empty;
            for (var i = 0; i < 600; i++)
            {
                if (Request.Form["check" + i] != null)
                {
                    tags = tags + Request.Form["check" + i] + ";";
                }
            }
            ShareDocVM.SetUserBookTag(updater,tags);

            return RedirectToAction("IBook");
        }

    }
}