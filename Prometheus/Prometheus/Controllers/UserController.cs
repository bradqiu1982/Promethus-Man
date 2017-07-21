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

        private void UserAuth(string username)
        {
            var userdict = UserMatrixVM.RetrieveUserMatrixAuth();
            if (userdict.ContainsKey(username.ToUpper()))
            {
                if (string.Compare(userdict[username.ToUpper()].ToUpper(), USERAUTH.SUPER.ToUpper()) == 0)
                {
                    ViewBag.IsSuper = true;
                }
                else
                {
                    ViewBag.IsSuper = false;
                }

                if (string.Compare(userdict[username.ToUpper()].ToUpper(), USERAUTH.ADMIN.ToUpper()) == 0)
                {
                    ViewBag.IsAdmin = true;
                }
                else
                {
                    ViewBag.IsAdmin = false;
                }

                if (string.Compare(userdict[username.ToUpper()].ToUpper(), USERAUTH.MANAGE.ToUpper()) == 0)
                {
                    ViewBag.IsAdmin = true;
                    ViewBag.IsManage = true;
                }
                else
                {
                    ViewBag.IsManage = false;
                }
            }//end if
            else
            {
                ViewBag.IsSuper = false;
                ViewBag.IsAdmin = false;
                ViewBag.IsManage = false;
            }

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

                return RedirectToAction(logonredirectact, logonredirectctrl);
            }
            else
            {
                //verify user information
                string logonuser = username + "||" + DateTime.Now.ToString();
                var ck = new Dictionary<string, string>();
                ck.Add("logonuser", logonuser);
                CookieUtility.SetCookie(this, ck);

                return RedirectToAction("UserCenter", "User");
            }
        }

        [HttpPost, ActionName("LoginUser")]
        [ValidateAntiForgeryToken]
        public ActionResult LoginUserPOST()
        {
            var username = Request.Form["Email"].ToUpper();
            var password = Request.Form["Password"];
            if (!username.ToUpper().Contains("@FINISAR.COM"))
            {
                return View();
            }

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
            if (val.ContainsKey("logonredirectact"))
            {
                val["logonredirectact"] = "";
            }
            else
            {
                val.Add("logonredirectact", "");
            }

            CookieUtility.SetCookie(this, val);
            return RedirectToAction("UserCenter", "User");
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
                UserAuth(usernm);

                //asign to me
                var alllist = new List<IssueViewModels>();
                var pendinglist = IssueViewModels.RetrieveIssueByAssignee(usernm, Resolute.Pending, 120);
                var workinglist = IssueViewModels.RetrieveIssueByAssigneeWorking(usernm, Resolute.Working, 120);
                var donelist = IssueViewModels.RetrieveIssueByAssignee(usernm, Resolute.Done, 360);
                alllist.AddRange(pendinglist);
                alllist.AddRange(workinglist);
                alllist.AddRange(donelist);

                ViewBag.pendinglist = FilterSubIssue(pendinglist, alllist);
                ViewBag.workinglist = FilterSubIssue(workinglist, alllist);

                ViewBag.UserName = usernm.Split(new char[] { '@' })[0];
                ViewBag.RealUserID = usernm;

                if (string.IsNullOrEmpty(month))
                {
                    ViewBag.month = "";
                }
                else
                {
                    ViewBag.month = month;
                }

                return View();
            }
            else
            {
                return RedirectToAction("LoginUser", "User");
            }
        }


        private List<IssueViewModels> FilterSubIssue(List<IssueViewModels> tobefiter, List<IssueViewModels> allissue)
        {
            var alldict = new Dictionary<string, bool>();
            foreach (var item in allissue)
            {
                if (!alldict.ContainsKey(item.IssueKey))
                {
                    alldict.Add(item.IssueKey, true);
                }
            }


            var ret = new List<IssueViewModels>();
            foreach (var item in tobefiter)
            {
                if (string.IsNullOrEmpty(item.ParentIssueKey))
                {
                    ret.Add(item);
                }
                else
                {
                    if (!alldict.ContainsKey(item.ParentIssueKey))
                    {
                        ret.Add(item);
                    }
                }
            }
            return ret;
        }

        public ActionResult Assign2Me(string username)
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
            else if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {
                usernm = ckdict["logonuser"].Split(new char[] { '|' })[0];
            }

            
            if (!string.IsNullOrEmpty(usernm))
            {
                ViewBag.UserName = usernm.Split(new char[] { '@' })[0];
                ViewBag.RealUserID = usernm;

                var alllist = new List<IssueViewModels>();
                var pendinglist = IssueViewModels.RetrieveIssueByAssignee(usernm, Resolute.Pending, 120);
                var workinglist = IssueViewModels.RetrieveIssueByAssigneeWorking(usernm, Resolute.Working, 120);
                var donelist = IssueViewModels.RetrieveIssueByAssignee(usernm, Resolute.Done, 360);
                alllist.AddRange(pendinglist);
                alllist.AddRange(workinglist);
                alllist.AddRange(donelist);
                ViewBag.pendinglist = FilterSubIssue(pendinglist, alllist);
                ViewBag.workinglist = FilterSubIssue(workinglist, alllist);
                ViewBag.donelist = FilterSubIssue(donelist, alllist);

                return View();
            }
            else
            {
                return RedirectToAction("LoginUser", "User");
            }
        }

        public ActionResult IAssigned(string username)
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
            else if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {
                usernm = ckdict["logonuser"].Split(new char[] { '|' })[0];
            }


            if (!string.IsNullOrEmpty(usernm))
            {
                ViewBag.UserName = usernm.Split(new char[] { '@' })[0];
                ViewBag.RealUserID = usernm;

                var iassignissues = IssueViewModels.RetrieveIssueByCreator(usernm, 300, this);
                var wholedata = new List<List<string>>();
                var title = new List<string>();
                title.Add("Project");
                title.Add("Summary");
                title.Add("Priority");
                title.Add("TaskType");
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
                    templine.Add(item.DueDate.ToString("yy-MM-dd"));
                    templine.Add(item.IssueKey);
                    templine.Add("MAIN");
                    templine.Add(item.IssueType);

                    wholedata.Add(templine);
                    //foreach (var item1 in item.SubIssues)
                    //{
                    //    templine = new List<string>();
                    //    templine.Add(item1.ProjectKey);
                    //    templine.Add(item1.Summary);
                    //    templine.Add(item1.Priority);
                    //    templine.Add(item1.Assignee);
                    //    templine.Add(item1.Resolution);
                    //    templine.Add(item1.DueDate.ToString("MM/dd-yy"));
                    //    templine.Add(item1.IssueKey);
                    //    templine.Add("SUB");
                    //    templine.Add(item1.IssueType);

                    //    wholedata.Add(templine);
                    //}
                }

                ViewBag.iassignlist = wholedata;

                return View();
            }
            else
            {
                return RedirectToAction("LoginUser", "User");
            }
        }

        public ActionResult ICare(string username, string month)
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
            else if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {
                usernm = ckdict["logonuser"].Split(new char[] { '|' })[0];
            }


            if (!string.IsNullOrEmpty(usernm))
            {
                ViewBag.UserName = usernm.Split(new char[] { '@' })[0];
                ViewBag.RealUserID = usernm;

                if (ckdict.Count > 0 && ckdict.ContainsKey("logonuser"))
                {
                    var updater = ckdict["logonuser"].Split(new char[] { '|' })[0].ToUpper();
                    var tempmonth = 1;
                    if (string.IsNullOrEmpty(month))
                    {
                        tempmonth = 1;
                        ViewBag.month = "";
                    }
                    else
                    {
                        try { tempmonth = Convert.ToInt32(month);
                            ViewBag.month = month;
                        }
                        catch (Exception ex) { tempmonth = 1; ViewBag.month = ""; }
                    }

                    if (string.Compare(updater, usernm, true) == 0)
                    {
                        UserAuth(usernm);

                        CreateICareList();
                        ViewBag.icareissuelist = UserActionTrend.RetrieveICareUserIssue(this, updater, tempmonth);
                        ViewBag.icareranklist = UserActionTrend.RetrieveICareUserRank(this, updater, tempmonth);
                        ViewBag.icaremonth = tempmonth.ToString();
                    }
                }

                return View();
            }
            else
            {
                return RedirectToAction("LoginUser", "User");
            }
        }

        public ActionResult MyStatus(string username)
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
            else if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {
                usernm = ckdict["logonuser"].Split(new char[] { '|' })[0];
            }


            if (!string.IsNullOrEmpty(usernm))
            {
                ViewBag.UserName = usernm.Split(new char[] { '@' })[0];
                ViewBag.RealUserID = usernm;

                ViewBag.myissuesummary = UserActionTrend.RetrieveMyIssuerSummary(this, usernm, 1);
                ViewBag.myranksummary = UserActionTrend.RetrieveMyRankSummary(this, usernm, 1);

                return View();
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
                dict.Add("username", updater);
                return RedirectToAction("ICare", "User", dict);
            }

            if (Request.Form["deleteuser"] != null)
            {
                var other = Request.Form["chooseuserlist"].ToString();
                UserViewModels.RemoveICare(updater, other);
                dict.Add("month", "1");
                dict.Add("username", updater);
                return RedirectToAction("ICare", "User", dict);
            }


            dict.Add("month", Request.Form["monthlist"].ToString());
            dict.Add("username", updater);
            return RedirectToAction("ICare", "User", dict);
        }

        public ActionResult UserIssues(string username,string month)
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
            ViewBag.uname = updater.Split(new char[] { '@' })[0];
            ViewBag.RealUserID = updater;

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
                var pendingissues = IssueViewModels.RetrieveIssuePendingByUser(username, sdate.ToString("yyyy-MM-dd hh:mm:ss"));
                var workingissues = IssueViewModels.RetrieveIssueWorkingByUser(username, sdate.ToString("yyyy-MM-dd hh:mm:ss"));
                var solvedissues = IssueViewModels.RetrieveIssueDoneByUser(username, sdate.ToString("yyyy-MM-dd hh:mm:ss"), this);
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

        private List<SelectListItem> CreateSelectList2(List<string> valist)
        {
            var pslist = new List<SelectListItem>();

            foreach (var p in valist)
            {
                var pitem = new SelectListItem();
                pitem.Text = p;
                pitem.Value = p;
                pslist.Add(pitem);
            }

            return pslist;
        }

        private void createpjlist()
        {
            var projlist = ProjectViewModels.RetrieveAllProjectKey();
            var newprojlist = new List<string>();
            newprojlist.Add("Project Tag");
            newprojlist.AddRange(projlist);
            var projectlist= CreateSelectList2(newprojlist);
            projectlist[0].Disabled = true;
            projectlist[0].Selected = true;
            ViewBag.projectlist = projectlist;
        }

        private void createtaglist()
        {
            var tags = ShareDocVM.RetrieveShareTags();
            tags.Sort();
            var newtags = new List<string>();
            newtags.Add("Technical Tag");
            newtags.AddRange(tags);
            var taglist = CreateSelectList2(newtags);
            taglist[0].Disabled = true;
            taglist[0].Selected = true;
            ViewBag.taglist = taglist;
        }

        public ActionResult ITag()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {

            }
            else
            {
                var ck = new Dictionary<string, string>();
                ck.Add("logonredirectctrl", "User");
                ck.Add("logonredirectact", "ITag");
                CookieUtility.SetCookie(this, ck);
                return RedirectToAction("LoginUser", "User");
            }

            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
            ViewBag.UserName = updater.Split(new char[] { '@' })[0];
            ViewBag.RealUserID = updater;

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

            var pjtag = "NONE"; 
            if (Request.Form["projectlist"] != null)
            {
                pjtag = Request.Form["projectlist"].ToString();
            }
            var normaltag = "NONE";
            if (Request.Form["taglist"] != null)
            {
                normaltag = Request.Form["taglist"].ToString();
            }

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

            return RedirectToAction("ITag", "User");
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

            return RedirectToAction("ITag","User");
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
            ViewBag.UserName = updater.Split(new char[] { '@' })[0];
            ViewBag.RealUserID = updater;

            var asilist = UserViewModels.RetrieveAllUser();
            ViewBag.towholist = CreateSelectList(asilist,"");

            var ilearntable = new List<ShareDocVM>();
            var iweeklytable = new List<ShareDocVM>();
            var itrainingtable = new List<ShareDocVM>();

            var allileanrn =  ShareDocVM.RetrieveMyLearn(updater,this);
            foreach (var item in allileanrn)
            {
                if (item.DOCTag.ToUpper().Contains(SPECIALBLOGType.WEEKLYREPORT)
                    || item.DOCTag.ToUpper().Contains("WEEKLY REPORT"))
                {
                    iweeklytable.Add(item);
                }
                else if (item.DOCTag.ToUpper().Contains(SPECIALBLOGType.TRAINING))
                {
                    itrainingtable.Add(item);
                }
                else
                {
                    ilearntable.Add(item);
                }
            }

            ViewBag.ILearn = ilearntable;
            ViewBag.IShare = ShareDocVM.RetrieveMyShare(updater,this);
            ViewBag.IWeeklyReport = iweeklytable;
            ViewBag.ITrainingReport = itrainingtable;
            return View();
        }

        public ActionResult ReviewILearn(string DOCTYPE,string DOCKEY,string DOCCreator)
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

            if (string.Compare(DOCTYPE, ShareDocType.ISSUE, true) == 0)
            {
                var issue = IssueViewModels.RetrieveIssueByIssueKey(DOCKEY, this);
                if (issue == null)
                {
                    return RedirectToAction("ILearn", "User");
                }

                UserKPIVM.StoreUserReviewed(updater, issue.ProjectKey,"View Task: "+issue.Summary, "/Issue/UpdateIssue?issuekey=" + DOCKEY);

                var dict = new RouteValueDictionary();
                dict.Add("issuekey", DOCKEY);
                return RedirectToAction("UpdateIssue", "Issue", dict);
            }
            else if (string.Compare(DOCTYPE, ShareDocType.DEBUG, true) == 0)
            {
                var debugtree = ProjectErrorViewModels.RetrieveErrorByErrorKey(DOCKEY, this);
                var Summary = debugtree[0].ProjectKey + "-" + debugtree[0].OrignalCode;

                UserKPIVM.StoreUserReviewed(updater, debugtree[0].ProjectKey, "View DebugTree: "+Summary, "/Project/UpdateProjectError?ErrorKey=" + DOCKEY);

                var dict = new RouteValueDictionary();
                dict.Add("ErrorKey", DOCKEY);
                return RedirectToAction("UpdateProjectError", "Project", dict);
            }
            else if (string.Compare(DOCTYPE, ShareDocType.DOCUMENT, true) == 0)
            {
                var Summary = DOCKEY;
                UserKPIVM.StoreUserReviewed(updater,"ALL", "View Document: " + Summary, "/User/WebDoc?DocKey=" + DOCKEY + "&Creator=" + DOCCreator);

                var dict = new RouteValueDictionary();
                dict.Add("DocKey", DOCKEY);
                dict.Add("Creator", DOCCreator);
                return RedirectToAction("WebDoc", "User", dict);
            }
            else if (string.Compare(DOCTYPE, ShareDocType.BLOG, true) == 0)
            {
                var blog = UserBlogVM.RetrieveBlogDoc(DOCKEY, this);
                var Summary = blog.Title;

                UserKPIVM.StoreUserReviewed(updater, "ALL", "View BLOG: " + Summary, "/User/WebDoc?DocKey=" + DOCKEY);

                var dict = new RouteValueDictionary();
                dict.Add("DocKey", DOCKEY);
                return RedirectToAction("WebDoc", "User", dict);
            }

            return RedirectToAction("ILearn", "User");
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

        public ActionResult ILike(string DOCKey,string DOCCreator)
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

            if (!string.IsNullOrEmpty(DOCKey)
                && !string.IsNullOrEmpty(DOCCreator))
            {
                ShareDocVM.LikeDoc(DOCKey, DOCCreator, updater,this);
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
            ViewBag.UserName = updater.Split(new char[] { '@' })[0];
            ViewBag.RealUserID = updater;

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

            var docid = ShareDocVM.ShareDoc("ALL", ShareDocType.BLOG, DOCKey, vm.Tag, vm.UserName, vm.CreateDate.ToString(),vm.DocURL);

            var whoes = ToWho.Split(new string[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var w in whoes)
            {
                ShareDocVM.PushDoc(w.Trim().ToUpper(), "ALL", ShareDocType.BLOG, DOCKey, vm.Tag, vm.UserName, DateTime.Now.ToString(), "",docid,vm.DocURL);
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
            ViewBag.UserName = updater.Split(new char[] { '@' })[0];
            ViewBag.RealUserID = updater;

            var tobechoosetags = new List<string>();
            var usertag = UserBlogVM.RetrieveUserTag(updater);
            if (!usertag.ToUpper().Contains(SPECIALBLOGType.WEEKLYREPORT))
            {
                tobechoosetags.Add(SPECIALBLOGType.WEEKLYREPORT);
            }
            if (!usertag.ToUpper().Contains(SPECIALBLOGType.DEFAULT))
            {
                tobechoosetags.Add(SPECIALBLOGType.DEFAULT);
            }
            if (!usertag.ToUpper().Contains(SPECIALBLOGType.TRAINING))
            {
                tobechoosetags.Add(SPECIALBLOGType.TRAINING);
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
                            .Replace(" ", "_").Replace("#", "").Replace("'", "")
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

        private void DefaultActionForBlogDoc(UserBlogVM blog,string updater)
        {
            if (blog.Tag.ToUpper().Contains(SPECIALBLOGType.WEEKLYREPORT))
            {
                var reportgroup = UserGroupVM.RetreiveUserGroup(updater, UserGroupType.ReportGroup);
                if (!string.IsNullOrEmpty(reportgroup))
                {

                    var docid = ShareDocVM.ShareDoc("ALL", ShareDocType.BLOG, blog.DocKey, blog.Tag, blog.UserName, blog.CreateDate.ToString(),blog.DocURL);
                    var mbs = reportgroup.Split(new string[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var mb in mbs)
                    {
                        ShareDocVM.PushDoc(mb.ToUpper(), "ALL", ShareDocType.BLOG, blog.DocKey, blog.Tag, blog.UserName, DateTime.Now.ToString(), "",docid,blog.DocURL);
                    }
                }
            }

            if (blog.Tag.ToUpper().Contains(SPECIALBLOGType.TRAINING))
            {
                var workgroup = UserGroupVM.RetreiveUserGroup(updater, UserGroupType.WorkGroup);
                if (!string.IsNullOrEmpty(workgroup))
                {
                    var docid = ShareDocVM.ShareDoc("ALL", ShareDocType.BLOG, blog.DocKey, blog.Tag, blog.UserName, blog.CreateDate.ToString(),blog.DocURL);
                    var mbs = workgroup.Split(new string[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var mb in mbs)
                    {
                        ShareDocVM.PushDoc(mb.ToUpper(), "ALL", ShareDocType.BLOG, blog.DocKey, blog.Tag, blog.UserName, DateTime.Now.ToString(), "",docid,blog.DocURL);
                    }
                }
            }
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
                    var blog = new UserBlogVM();
                    blog.UserName = updater;
                    blog.ContentType = UserBlogContentType.ATTACH;
                    blog.Title = originalname + Path.GetExtension(internalreportfile);
                    blog.Content = url;
                    blog.Tag = tag;
                    blog.StoreBlogDoc();

                    var currentblog = UserBlogVM.RetrieveBlogDoc(blog.DocKey, this);
                    DefaultActionForBlogDoc(currentblog, updater);
                }
            }//end if


            if (!string.IsNullOrEmpty(Request.Form["docinputeditor"]))
            {
                var blog = new UserBlogVM();
                blog.UserName = updater;
                blog.ContentType = UserBlogContentType.COMMENT;
                blog.Content = SeverHtmlDecode.Decode(this,Request.Form["docinputeditor"]);

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

                var currentblog = UserBlogVM.RetrieveBlogDoc(blog.DocKey, this);
                DefaultActionForBlogDoc(currentblog, updater);
            }

            return RedirectToAction("IBLOG", "User");
        }

        public ActionResult ModifyBlogDoc(string DocKey)
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
            ViewBag.UserName = updater.Split(new char[] { '@' })[0];
            ViewBag.RealUserID = updater;

            var vm = UserBlogVM.RetrieveBlogDoc(DocKey,this);
            return View(vm);
        }

        [HttpPost, ActionName("ModifyBlogDoc")]
        [ValidateAntiForgeryToken]
        public ActionResult ModifyBlogDocPost()
        {
            var dockey = Request.Form["HDocKey"];
            var vm = UserBlogVM.RetrieveBlogDoc(dockey,this);


            if (!string.IsNullOrEmpty(Request.Form["editor1"]))
            {
                vm.Content = SeverHtmlDecode.Decode(this,Request.Form["editor1"]);
                vm.UpdateBlogDoc();
            }
            else
            {
                vm.Content = "<p>To Be Edit</p>";
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

            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
            ViewBag.UserName = updater.Split(new char[] { '@' })[0];
            ViewBag.RealUserID = updater;

            if (!string.IsNullOrEmpty(DocKey))
            {
                var vm = UserBlogVM.RetrieveBlogDoc(DocKey,this);

                if (string.IsNullOrEmpty(vm.DocKey))
                {
                    var backlink = ShareDocVM.RetrieveBackLink(DocKey);

                    var tempstrs = DocKey.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                    var datestr = tempstrs[tempstrs.Length - 1].Substring(0,8);
                    vm.ContentType = UserBlogContentType.ATTACH;
                    vm.Title = DocKey;
                    vm.Content = "/userfiles/docs/" + datestr + "/" + DocKey;
                    vm.DocKey = DocKey;
                    vm.UserName = Creator;
                    vm.DocURL = "/User/WebDoc?DocKey=" + DocKey + "&Creator=" + Creator;
                    vm.BackLink = backlink;
                }

                if (string.Compare(vm.UserName, updater, true) == 0)
                {
                    ViewBag.Owner = true;
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
                        var backlink = ShareDocVM.RetrieveBackLink(DocKey);

                        var tempstrs = tempDocKey.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                        var datestr = tempstrs[tempstrs.Length - 1].Substring(0, 8);
                        vm.ContentType = UserBlogContentType.ATTACH;
                        vm.Title = tempDocKey;
                        vm.Content = "/userfiles/docs/" + datestr + "/" + tempDocKey;
                        vm.DocKey = tempDocKey;
                        vm.UserName = Creator;
                        vm.DocURL = "/User/WebDoc?DocKey=" + DocKey + "&Creator=" + Creator;

                        vm.BackLink = backlink;
                    }

                    if (string.Compare(vm.UserName, updater, true) == 0)
                    {
                        ViewBag.Owner = true;
                    }

                    return View(vm);
                }
            }

            return RedirectToAction("IBLOG", "User");
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
                var name = namestart.Substring(1, spaceidx - 1);
                if (name.Length > 3)
                {
                    if (name.ToUpper().Contains("@FINISAR.COM"))
                    {
                        ret.Add(name.ToUpper());
                    }
                    else if (name.Contains("."))
                    {
                        ret.Add(name.ToUpper() + "@FINISAR.COM");
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

        [HttpPost, ActionName("WebDoc")]
        [ValidateAntiForgeryToken]
        public ActionResult WebDocPost()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

            var dockey = Request.Form["DocKey"];
            var docurl = Request.Form["DOCURL"];
            var doccreator = Request.Form["DOCCREATOR"];

            if (!string.IsNullOrEmpty(Request.Form["docinputeditor"]))
            {
                var com = new ErrorComments();
                com.Comment = SeverHtmlDecode.Decode(this,Request.Form["docinputeditor"]);
                if (!string.IsNullOrEmpty(com.Comment))
                {
                    var towholist = new List<string>();
                    var atlist = RetrieveUserFromComment(com.Comment);
                    if (atlist.Count > 0) towholist.AddRange(atlist);
                    towholist.Add(doccreator);

                    ProjectErrorViewModels.StoreErrorComment(dockey, com.dbComment, PJERRORCOMMENTTYPE.Description, updater, DateTime.Now.ToString());

                    var commentcontent = System.Text.RegularExpressions.Regex.Replace(com.Comment.Replace("\"", "").Replace("&nbsp;", ""), "<.*?>", string.Empty).Trim();
                    ShareDocVM.SendPushCommentEvent("a new comment", docurl, towholist, updater, this, commentcontent);
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

            if (!string.IsNullOrEmpty(Request.Form["editor1"]))
            {
                var tempcommment = new ErrorComments();
                tempcommment.Comment = SeverHtmlDecode.Decode(this,Request.Form["editor1"]);
                ProjectErrorViewModels.UpdateSPComment(errorkey, commenttype, commentdate, tempcommment.dbComment);
            }
            else
            {
                var tempcommment = new ErrorComments();
                tempcommment.Comment = "<p>To Be Edit</p>";
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

        public ActionResult IGroup()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {

            }
            else
            {
                var ck = new Dictionary<string, string>();
                ck.Add("logonredirectctrl", "User");
                ck.Add("logonredirectact", "IGroup");
                CookieUtility.SetCookie(this, ck);
                return RedirectToAction("LoginUser", "User");
            }

            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
            ViewBag.UserName = updater.Split(new char[] { '@' })[0];
            ViewBag.RealUserID = updater;

            var asilist = UserViewModels.RetrieveAllUser();
            ViewBag.towholist1 = CreateSelectList(asilist, "");

            var grouptype = new string[] { "Please choose your group type", UserGroupType.WorkGroup,UserGroupType.ReportGroup,UserGroupType.LYTGroup};
            var grouptypelist = CreateSelectList(grouptype.ToList(), "");
            grouptypelist[0].Selected = true;
            grouptypelist[0].Disabled = true;
            ViewBag.grouptypelist = grouptypelist;

            var vm = UserGroupVM.RetreiveAllGroup();
            return View(vm);
        }

        [HttpPost, ActionName("IGroup")]
        [ValidateAntiForgeryToken]
        public ActionResult IGroupPost()
        {
            var grouptype = Request.Form["grouptypelist"].ToString();
            var groupmember = Request.Form["RPeopleAddr"];
            UserGroupVM.AddGroup(grouptype, groupmember);
            return RedirectToAction("IGroup", "User");
        }

        public ActionResult AddPJCriticalError()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {

            }
            else
            {
                var ck = new Dictionary<string, string>();
                ck.Add("logonredirectctrl", "User");
                ck.Add("logonredirectact", "AddPJCriticalError");
                CookieUtility.SetCookie(this, ck);
                return RedirectToAction("LoginUser", "User");
            }

            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
            ViewBag.UserName = updater.Split(new char[] { '@' })[0];
            ViewBag.RealUserID = updater;

            var pjlist = UserViewModels.RetrieveUserProjectKeyDict(updater).Keys.ToList();
            var errordict = new Dictionary<string, bool>();
            foreach (var pj in pjlist)
            {
                var pjerrorlist = ProjectErrorViewModels.RetrieveErrorByPJKey(pj, this);
                foreach (var item in pjerrorlist)
                {
                    if (!errordict.ContainsKey(item.OrignalCode))
                    {
                        errordict.Add(item.OrignalCode,true);
                    }
                }//end foreach
            }//end foreach

            var errorlist = errordict.Keys.ToList();
            var selectlist = new List<string>();
            selectlist.Add("Please select project");
            selectlist.AddRange(pjlist);
            var selectcontrol = CreateSelectList(selectlist, "");
            selectcontrol[0].Disabled = true;
            selectcontrol[0].Selected = true;
            ViewBag.pjlist = selectcontrol;

            selectlist = new List<string>();
            selectlist.Add("Please select error code");
            selectlist.AddRange(errorlist);
            selectcontrol = CreateSelectList(selectlist, "");
            selectcontrol[0].Disabled = true;
            selectcontrol[0].Selected = true;
            ViewBag.errorlist = selectcontrol;

            selectlist = new List<string>();
            selectlist.Add("Please select temperature (Optional)");
            selectlist.Add(TestTemperatureType.Low);
            selectlist.Add(TestTemperatureType.Nomal);
            selectlist.Add(TestTemperatureType.High);
            selectcontrol = CreateSelectList(selectlist, "");
            selectcontrol[0].Disabled = true;
            selectcontrol[0].Selected = true;
            ViewBag.templist = selectcontrol;

            selectlist = new List<string>();
            selectlist.Add("Please select Channel (Optional)");
            for (var idx = 0; idx < 24; idx++)
            {
                selectlist.Add(idx.ToString());
            }
            selectcontrol = CreateSelectList(selectlist, "");
            selectcontrol[0].Disabled = true;
            selectcontrol[0].Selected = true;
            ViewBag.channellist = selectcontrol;

            selectlist = new List<string>();
            selectlist.Add("Please select algorithm");
            selectlist.Add(PJCriticalAlgorithm.UNIFORMITY);
            selectlist.Add(PJCriticalAlgorithm.STDDEV);
            selectcontrol = CreateSelectList(selectlist, "");
            selectcontrol[0].Disabled = true;
            selectcontrol[0].Selected = true;
            ViewBag.algorithmlist = selectcontrol;

            var vm = new List<ProjectCriticalErrorVM>();
            foreach (var pj in pjlist)
            {
                var tempvm = ProjectCriticalErrorVM.RetrievePJCriticalError(pj, null);
                if (tempvm.Count > 0)
                {
                    vm.AddRange(tempvm);
                }
            }

            var asilist = UserViewModels.RetrieveAllUser();
            ViewBag.towholist1 = CreateSelectList(asilist, "");

            return View(vm);
        }

        [HttpPost, ActionName("AddPJCriticalError")]
        [ValidateAntiForgeryToken]
        public ActionResult AddPJCriticalErrorPost()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

            try
            {
                var vm = new ProjectCriticalErrorVM();
                vm.Creater = updater;
                vm.ProjectKey = Request.Form["pjlist"];
                vm.ErrorCode = Request.Form["errorlist"];
                vm.TestCaseName = Request.Form["testcase"];
                vm.MatchCond = Request.Form["matchcond"];
                if (Request.Form["withlimit"] != null)
                {
                    vm.WithLimit = 1;
                    vm.LowLimit = Convert.ToDouble(Request.Form["lowlimit"]);
                    vm.HighLimit = Convert.ToDouble(Request.Form["highlimit"]);
                    if (vm.LowLimit > vm.HighLimit)
                    {
                        return RedirectToAction("AddPJCriticalError", "User");
                    }
                }
                if (Request.Form["withalgorithm"] != null)
                {
                    vm.WithAlgorithm = 1;
                    vm.Algorithm = Request.Form["algorithmlist"];
                    vm.AlgorithmParam = Request.Form["algorithmparam"];
                }

                vm.Temperature = Request.Form["templist"];
                vm.Channel = Request.Form["channellist"];
                vm.Appv_4 = Request.Form["RPeopleAddr"];
                vm.StorePJCriticalError();
            }
            catch (Exception ex) { }
            return RedirectToAction("AddPJCriticalError", "User");
        }

        public ActionResult DeletePJCriticalError(string PJKey, string ErrorCode,string TestCase,string MatchCond)
        {
            ProjectCriticalErrorVM.RemovePJCriticalError(PJKey, ErrorCode,TestCase,MatchCond);
            return RedirectToAction("AddPJCriticalError", "User");
        }

        public ActionResult Open2ndCheckFASwitch(string PJKey, string ErrorCode, string TestCase, string MatchCond)
        {
            ProjectCriticalErrorVM.Open2ndCheckFASwitch(PJKey, ErrorCode, TestCase, MatchCond);
            return RedirectToAction("AddPJCriticalError", "User");
        }

        public ActionResult Close2ndCheckFASwitch(string PJKey, string ErrorCode, string TestCase, string MatchCond)
        {
            ProjectCriticalErrorVM.Close2ndCheckFASwitch(PJKey, ErrorCode, TestCase, MatchCond);
            return RedirectToAction("AddPJCriticalError", "User");
        }

        public ActionResult DeleteGroup(string GroupID)
        {
            UserGroupVM.DeleteGroup(GroupID);
            return RedirectToAction("IGroup", "User");
        }

        public ActionResult IKPI(string month)
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {

            }
            else
            {
                var ck = new Dictionary<string, string>();
                ck.Add("logonredirectctrl", "User");
                ck.Add("logonredirectact", "IKPI");
                CookieUtility.SetCookie(this, ck);
                return RedirectToAction("LoginUser", "User");
            }

            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
            ViewBag.UserName = updater.Split(new char[] { '@' })[0];
            ViewBag.RealUserID = updater;


            var tempmonth = 1;
            if (!string.IsNullOrEmpty(month))
            {
                try
                {
                    tempmonth = Convert.ToInt32(month);
                }
                catch (Exception ex)
                { tempmonth = 1; }
            }

            var vm = UserKPIVM.RetrieveRankByUserName(updater, DateTime.Now.AddMonths(0 - tempmonth).ToString());

            var sarray = new string[] { "1", "2", "3", "4", "5", "6", "12", "18", "24","30","36" };
            var slist = new List<string>();
            slist.Add("KPI In Months");
            slist.AddRange(sarray);
            var monthlylist = CreateSelectList(slist, "");
            monthlylist[0].Selected = true;
            monthlylist[0].Disabled = true;
            ViewBag.monthlylist = monthlylist;

            ViewBag.TotalRank = UserKPIVM.RetrieveTotalRank(updater);

            return View(vm);
        }

        public ActionResult IAdmire(string month)
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {

            }
            else
            {
                var ck = new Dictionary<string, string>();
                ck.Add("logonredirectctrl", "User");
                ck.Add("logonredirectact", "IAdmire");
                CookieUtility.SetCookie(this, ck);
                return RedirectToAction("LoginUser", "User");
            }

            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
            ViewBag.UserName = updater.Split(new char[] { '@' })[0];
            ViewBag.RealUserID = updater;


            var tempmonth = 1;
            if (!string.IsNullOrEmpty(month))
            {
                try
                {
                    tempmonth = Convert.ToInt32(month);
                }
                catch (Exception ex)
                { tempmonth = 1; }
            }

            var vm = new List<UserKPIVM>();
            var usergroup = UserGroupVM.RetreiveUserGroup(updater, UserGroupType.ReportGroup);
            var userlist = usergroup.Split(new string[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var user in userlist)
            {
                var tempvm = UserKPIVM.RetrieveRank4Admire(user, DateTime.Now.AddMonths(0 - tempmonth).ToString());
                vm.AddRange(tempvm);
            }

            var sarray = new string[] { "1", "2", "3", "4", "5", "6", "12", "18", "24", "30", "36" };
            var slist = new List<string>();
            slist.Add("KPI In Months");
            slist.AddRange(sarray);
            var monthlylist = CreateSelectList(slist, "");
            monthlylist[0].Selected = true;
            monthlylist[0].Disabled = true;
            ViewBag.monthlylist = monthlylist;

            return View(vm);

        }

        [HttpPost, ActionName("IAdmire")]
        [ValidateAntiForgeryToken]
        public ActionResult IAdmirePost()
        {
            var rankcount = 0;
            if (!string.IsNullOrEmpty(Request.Form["rankcount"]))
            {
                rankcount = Convert.ToInt32(Request.Form["rankcount"]);
            }

            for (var idx = 0; idx < rankcount; idx++)
            {
                if (!string.IsNullOrEmpty(Request.Form["admirerank"+idx.ToString()]) 
                    && !string.IsNullOrEmpty(Request.Form["rankid"+idx.ToString()])
                    && !string.IsNullOrEmpty(Request.Form["updatetime" + idx.ToString()]))
                {
                    var admirerank = 0;
                    try
                    {
                        admirerank = Convert.ToInt32(Request.Form["admirerank" + idx.ToString()]);
                    }
                    catch (Exception ex)
                    { admirerank = 0; }
                    var rankid = (Request.Form["rankid" + idx.ToString()]);
                    var updatetime = Request.Form["updatetime" + idx.ToString()];
                    UserKPIVM.UpdateAdmireRank(rankid, admirerank,updatetime);
                }
            }

            return RedirectToAction("IAdmire", "User");
        }


        public ActionResult UserReviewedInfo(string month)
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {

            }
            else
            {
                var ck = new Dictionary<string, string>();
                ck.Add("logonredirectctrl", "User");
                ck.Add("logonredirectact", "IAdmire");
                CookieUtility.SetCookie(this, ck);
                return RedirectToAction("LoginUser", "User");
            }

            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
            ViewBag.UserName = updater.Split(new char[] { '@' })[0];
            ViewBag.RealUserID = updater;

            var tempmonth = 1;
            if (!string.IsNullOrEmpty(month))
            {
                try
                {
                    tempmonth = Convert.ToInt32(month);
                }
                catch (Exception ex)
                { tempmonth = 1; }
            }

            var vm = UserKPIVM.RetrieveUserReviewed(DateTime.Now.AddMonths(0 - tempmonth).ToString());

            var sarray = new string[] { "1", "2", "3", "4", "5", "6", "12", "18", "24", "30", "36" };
            var slist = new List<string>();
            slist.Add("User Reviewed In Months");
            slist.AddRange(sarray);
            var monthlylist = CreateSelectList(slist, "");
            monthlylist[0].Selected = true;
            monthlylist[0].Disabled = true;
            ViewBag.monthlylist = monthlylist;

            return View(vm);
        }

        public ActionResult UserSharedInfo(string month)
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {

            }
            else
            {
                var ck = new Dictionary<string, string>();
                ck.Add("logonredirectctrl", "User");
                ck.Add("logonredirectact", "IAdmire");
                CookieUtility.SetCookie(this, ck);
                return RedirectToAction("LoginUser", "User");
            }

            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
            ViewBag.UserName = updater.Split(new char[] { '@' })[0];
            ViewBag.RealUserID = updater;

            var tempmonth = 1;
            if (!string.IsNullOrEmpty(month))
            {
                try
                {
                    tempmonth = Convert.ToInt32(month);
                }
                catch (Exception ex)
                { tempmonth = 1; }
            }

            var vm = ShareDocVM.RetrieveUserShare(DateTime.Now.AddMonths(0 - tempmonth).ToString(),this);

            var sarray = new string[] { "1", "2", "3", "4", "5", "6", "12", "18", "24", "30", "36" };
            var slist = new List<string>();
            slist.Add("User Shared In Months");
            slist.AddRange(sarray);
            var monthlylist = CreateSelectList(slist, "");
            monthlylist[0].Selected = true;
            monthlylist[0].Disabled = true;
            ViewBag.monthlylist = monthlylist;

            return View(vm);
        }

    }
}
