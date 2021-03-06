﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Prometheus.Models;
using System.Web.Routing;
using System.Text;
using System.Net.Mail;
using System.IO;
using System.Threading;
using System.Web.Caching;

namespace Prometheus.Controllers
{
    public class ProjectController : Controller
    {
        private static void logthdinfo(string info)
        {
            //var filename = "d:\\log\\spendtime-" + DateTime.Now.ToString("yyyy-MM-dd");
            //if (System.IO.File.Exists(filename))
            //{
            //    var content = System.IO.File.ReadAllText(filename);
            //    content = content + "\r\n" + DateTime.Now.ToString() + " : " + info;
            //    System.IO.File.WriteAllText(filename, content);
            //}
            //else
            //{
            //    System.IO.File.WriteAllText(filename, DateTime.Now.ToString() + " : " + info);
            //}
        }

        private void UserAuth(string username)
        {
            var userdict = new Dictionary<string, string>();

            var mycache = HttpContext.Cache;
            var tempuserdict = mycache.Get("usermatrixvm_CUST");
            if (tempuserdict == null)
            {
                userdict = UserMatrixVM.RetrieveUserMatrixAuth();
                mycache.Insert("usermatrixvm_CUST", userdict, null, DateTime.Now.AddHours(2), Cache.NoSlidingExpiration);
            }
            else
            {
                userdict = (Dictionary<string, string>)tempuserdict;
            }

            if (userdict.ContainsKey(username.ToUpper()))
            {
                ViewBag.IsOutSide = false;

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
                ViewBag.IsOutSide = true;
                var syscfg = CfgUtility.GetSysConfig(this);
                var outsideauth = syscfg["OUTSIDEPEOPLEAUTH"];
                if (string.Compare(outsideauth, "TRUE", true) == 0)
                {
                    ViewBag.IsOutSide = false;
                }
            }

        }

        private void _viewallprivate()
        {
            var mycache = HttpContext.Cache;

            var projlist = ProjectViewModels.RetrieveAllProject();
            if (projlist.Count == 0)
            {
                return;
            }

            var filterlist = new List<SelectListItem>();
            var filteritem = new SelectListItem();
            filteritem.Text = "Find Project";
            filteritem.Value = "Find Project";
            filterlist.Add(filteritem);

            var startdate = DateTime.Now.AddDays(-7);
            var enddate = DateTime.Now.ToString();
            if (startdate.DayOfWeek != DayOfWeek.Thursday)
            {
                for (int i = 7; i > 0; i--)
                {
                    if (DateTime.Now.AddDays(0 - i).DayOfWeek == DayOfWeek.Thursday)
                    {
                        startdate = DateTime.Now.AddDays(0 - i);
                        break;
                    }
                }
            }

            foreach (var item in projlist)
            {
                filteritem = new SelectListItem();
                filteritem.Text = item.ProjectKey;
                filteritem.Value = item.ProjectKey;
                filterlist.Add(filteritem);

                //now = DateTime.Now;
                //msec1 = now.Hour * 60 * 60 * 1000 + now.Minute * 60 * 1000 + now.Second * 1000 + now.Millisecond;
                var firstyield = mycache.Get(item.ProjectKey + "_FPY_CUST");
                if (firstyield == null)
                {
                    var yvm = ProjectYieldViewModule.GetYieldByDateRange(item.ProjectKey, startdate.ToString(), enddate, item, HttpContext.Cache);

                    if (yvm.FirstYields.Count > 0)
                    {
                        item.FirstYield = yvm.FirstYield;
                        item.RetestYield = yvm.LastYield;
                    }
                    else
                    {
                        item.FirstYield = -1.0;
                        item.RetestYield = -1.0;
                    }
                    mycache.Insert(item.ProjectKey + "_FPY_CUST", item.FirstYield, null, DateTime.Now.AddHours(2), Cache.NoSlidingExpiration);
                    mycache.Insert(item.ProjectKey + "_FY_CUST", item.RetestYield, null, DateTime.Now.AddHours(2), Cache.NoSlidingExpiration);
                }
                else
                {
                    item.FirstYield = Convert.ToDouble(mycache.Get(item.ProjectKey + "_FPY_CUST"));
                    item.RetestYield = Convert.ToDouble(mycache.Get(item.ProjectKey + "_FY_CUST"));
                }


                NPIInfo(item);

                var taskcount = mycache.Get(item.ProjectKey + "_taskct_CUST");
                if (taskcount == null)
                {
                    var taskdone = IssueViewModels.RetrieveTaskCountByProjectKey(item.ProjectKey, Resolute.Done);
                    var tasktotal = taskdone + IssueViewModels.RetrieveTaskCountByProjectKey(item.ProjectKey, Resolute.Pending)
                        + IssueViewModels.RetrieveTaskCountByProjectKey(item.ProjectKey, Resolute.Working);
                    item.PendingTaskCount = taskdone.ToString() + "/" + tasktotal.ToString();
                    mycache.Insert(item.ProjectKey + "_taskct_CUST", item.PendingTaskCount, null, DateTime.Now.AddHours(4), Cache.NoSlidingExpiration);
                }
                else
                {
                    item.PendingTaskCount = Convert.ToString(taskcount);
                }


                var facount = mycache.Get(item.ProjectKey + "_fact_CUST");
                if (facount == null)
                {
                    var fadone = ProjectFAViewModules.RetrieveFADataCount(item.ProjectKey, false);
                    var fatotal = fadone + ProjectFAViewModules.RetrieveFADataCount(item.ProjectKey);
                    item.PendingFACount = fadone.ToString() + "/" + fatotal.ToString();
                    mycache.Insert(item.ProjectKey + "_fact_CUST", item.PendingFACount, null, DateTime.Now.AddHours(4), Cache.NoSlidingExpiration);
                }
                else
                {
                    item.PendingFACount = Convert.ToString(facount);
                }


                var rmacount = mycache.Get(item.ProjectKey + "_rmact_CUST");
                if (rmacount == null)
                {
                    var rmadone = IssueViewModels.RetrieveRMACountByProjectKey(item.ProjectKey, Resolute.Done);
                    var rmatotal = rmadone + IssueViewModels.RetrieveRMACountByProjectKey(item.ProjectKey, Resolute.Pending)
                        + IssueViewModels.RetrieveRMACountByProjectKey(item.ProjectKey, Resolute.Working);
                    item.PendingRMACount = rmadone.ToString() + "/" + rmatotal.ToString();
                    mycache.Insert(item.ProjectKey + "_rmact_CUST", item.PendingRMACount, null, DateTime.Now.AddHours(4), Cache.NoSlidingExpiration);
                }
                else
                {
                    item.PendingRMACount = Convert.ToString(rmacount);
                }

            }

        }

        // GET: Project
        public ActionResult ViewAll()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {

            }
            else
            {
                var ck = new Dictionary<string, string>();
                ck.Add("logonredirectctrl", "Project");
                ck.Add("logonredirectact", "ViewAll");
                CookieUtility.SetCookie(this, ck);
                return RedirectToAction("LoginUser", "User");
            }

            //now = DateTime.Now;
            //var msec2 = now.Hour * 60 * 60 * 1000 + now.Minute * 60 * 1000 + now.Second * 1000 + now.Millisecond;
            //logthdinfo("cookie  " + (msec2 - msec1).ToString());

            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

            var mycache = HttpContext.Cache;

            UserAuth(updater);

            var projlist = new List<ProjectViewModels>();
            var cachepjlist = mycache.Get(updater + "_pjlist_CUST");
            if (cachepjlist == null)
            {
                var allprojlist = ProjectViewModels.RetrieveAllProject();
                var userpj = UserViewModels.RetrieveUserProjectKeyDict(updater);
                foreach (var pjk in allprojlist)
                {
                    if (userpj.ContainsKey(pjk.ProjectKey) || (ViewBag.IsSuper != null && ViewBag.IsSuper))
                    {
                        projlist.Add(pjk);
                    }
                }//end foreach

                if (projlist.Count == 0)
                {
                    return RedirectToAction("LoadProjects", "Project");
                }

                mycache.Insert(updater + "_pjlist_CUST", projlist, null, DateTime.Now.AddHours(2), Cache.NoSlidingExpiration);
            }
            else
            {
                projlist.AddRange((List<ProjectViewModels>)cachepjlist);
            }

            if (projlist.Count == 0)
            {
                return RedirectToAction("LoadProjects", "Project");
            }

            var filterlist = new List<SelectListItem>();
            var filteritem = new SelectListItem();
            filteritem.Text = "Find Project";
            filteritem.Value = "Find Project";
            filterlist.Add(filteritem);

            var startdate = DateTime.Now.AddDays(-7);
            var enddate = DateTime.Now.ToString();
            if (startdate.DayOfWeek != DayOfWeek.Thursday)
            {
                for (int i = 7; i > 0; i--)
                {
                    if (DateTime.Now.AddDays(0 - i).DayOfWeek == DayOfWeek.Thursday)
                    {
                        startdate = DateTime.Now.AddDays(0 - i);
                        break;
                    }
                }
            }

            foreach (var item in projlist)
            {
                filteritem = new SelectListItem();
                filteritem.Text = item.ProjectKey;
                filteritem.Value = item.ProjectKey;
                filterlist.Add(filteritem);

                //now = DateTime.Now;
                //msec1 = now.Hour * 60 * 60 * 1000 + now.Minute * 60 * 1000 + now.Second * 1000 + now.Millisecond;
                var firstyield = mycache.Get(item.ProjectKey + "_FPY_CUST");
                if (firstyield == null)
                {
                    var yvm = ProjectYieldViewModule.GetYieldByDateRange(item.ProjectKey, startdate.ToString(), enddate, item, HttpContext.Cache);

                    if (yvm.FirstYields.Count > 0)
                    {
                        item.FirstYield = yvm.FirstYield;
                        item.RetestYield = yvm.LastYield;
                    }
                    else
                    {
                        item.FirstYield = -1.0;
                        item.RetestYield = -1.0;
                    }
                    mycache.Insert(item.ProjectKey + "_FPY_CUST", item.FirstYield, null, DateTime.Now.AddHours(2), Cache.NoSlidingExpiration);
                    mycache.Insert(item.ProjectKey + "_FY_CUST", item.RetestYield, null, DateTime.Now.AddHours(2), Cache.NoSlidingExpiration);
                }
                else
                {
                    item.FirstYield = Convert.ToDouble(mycache.Get(item.ProjectKey + "_FPY_CUST"));
                    item.RetestYield = Convert.ToDouble(mycache.Get(item.ProjectKey + "_FY_CUST"));
                }


                NPIInfo(item);

                var taskcount = mycache.Get(item.ProjectKey + "_taskct_CUST");
                if (taskcount == null)
                {
                    var taskdone = IssueViewModels.RetrieveTaskCountByProjectKey(item.ProjectKey, Resolute.Done);
                    var tasktotal = taskdone + IssueViewModels.RetrieveTaskCountByProjectKey(item.ProjectKey, Resolute.Pending)
                        + IssueViewModels.RetrieveTaskCountByProjectKey(item.ProjectKey, Resolute.Working);
                    item.PendingTaskCount = taskdone.ToString() + "/" + tasktotal.ToString();
                    mycache.Insert(item.ProjectKey + "_taskct_CUST", item.PendingTaskCount, null, DateTime.Now.AddHours(4), Cache.NoSlidingExpiration);
                }
                else
                {
                    item.PendingTaskCount = Convert.ToString(taskcount);
                }


                var facount = mycache.Get(item.ProjectKey + "_fact_CUST");
                if (facount == null)
                {
                    var fadone = ProjectFAViewModules.RetrieveFADataCount(item.ProjectKey, false);
                    var fatotal = fadone + ProjectFAViewModules.RetrieveFADataCount(item.ProjectKey);
                    item.PendingFACount = fadone.ToString() + "/" + fatotal.ToString();
                    mycache.Insert(item.ProjectKey + "_fact_CUST", item.PendingFACount, null, DateTime.Now.AddHours(4), Cache.NoSlidingExpiration);
                }
                else
                {
                    item.PendingFACount = Convert.ToString(facount);
                }


                var rmacount = mycache.Get(item.ProjectKey + "_rmact_CUST");
                if (rmacount == null)
                {
                    var rmadone = IssueViewModels.RetrieveRMACountByProjectKey(item.ProjectKey, Resolute.Done);
                    var rmatotal = rmadone + IssueViewModels.RetrieveRMACountByProjectKey(item.ProjectKey, Resolute.Pending)
                        + IssueViewModels.RetrieveRMACountByProjectKey(item.ProjectKey, Resolute.Working);
                    item.PendingRMACount = rmadone.ToString() + "/" + rmatotal.ToString();
                    mycache.Insert(item.ProjectKey + "_rmact_CUST", item.PendingRMACount, null, DateTime.Now.AddHours(4), Cache.NoSlidingExpiration);
                }
                else
                {
                    item.PendingRMACount = Convert.ToString(rmacount);
                }

            }

            filterlist[0].Disabled = true;
            filterlist[0].Selected = true;
            ViewBag.pjfilterlist = filterlist;
            SortPJ(projlist);

            ViewBag.pjtpdict = new Dictionary<string, bool>();
            ViewBag.pjtypelist = new List<string>();

            var syscfg = CfgUtility.GetSysConfig(this);
            var pjtylist = syscfg["PROJECTTYPE"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var pjt in pjtylist)
            {
                ViewBag.pjtypelist.Add(pjt);
            }

            foreach (var item in projlist)
            {
                var tpstr = "";
                if (string.IsNullOrEmpty(item.ProjectType))
                {
                    tpstr = "Parallel";
                    item.ProjectType = "Parallel";
                }
                else
                {
                    tpstr = item.ProjectType;
                }

                if (!ViewBag.pjtpdict.ContainsKey(tpstr))
                {
                    ViewBag.pjtpdict.Add(tpstr, true);
                }
            }

            return View(projlist);
        }

        private void NPIInfo(ProjectViewModels item)
        {
            var ivmlist = new List<IssueViewModels>();

            var mycache = HttpContext.Cache;
            var templist = mycache.Get(item.ProjectKey + "_npilist_CUST");
            if (templist == null)
            {
                ivmlist = IssueViewModels.RetrieveNPIPROCIssue(item.ProjectKey, this);
                mycache.Insert(item.ProjectKey + "_npilist_CUST", ivmlist, null, DateTime.Now.AddHours(2), Cache.NoSlidingExpiration);
            }
            else
            {
                ivmlist.AddRange((List<IssueViewModels>)templist);
            }
            foreach (var iv in ivmlist)
            {
                if (iv.Summary.Contains("PIP1"))
                {
                    item.PIP1Date = iv.DueDate.ToString("yyyy-MM-dd");
                }

                if (iv.Summary.Contains("MVT"))
                {
                    item.MVTDate = iv.DueDate.ToString("yyyy-MM-dd");
                }

                if (iv.Resolution == Resolute.Working)
                {
                    if (iv.Summary.Contains("PIP1"))
                    {
                        item.CurrentNPIProc = "PIP1";
                    }
                    if (iv.Summary.Contains("EVT"))
                    {
                        item.CurrentNPIProc = "EVT";
                    }
                    if (iv.Summary.Contains("DVT"))
                    {
                        item.CurrentNPIProc = "DVT";
                    }
                    if (iv.Summary.Contains("MVT"))
                    {
                        item.CurrentNPIProc = "MVT";
                    }
                    if (iv.Summary.Contains("MP"))
                    {
                        item.CurrentNPIProc = "MP";
                    }
                    item.CurrentNPIProcKey = iv.IssueKey;
                }
            }
        }

        private void SortPJ(List<ProjectViewModels> projlist)
        {
            projlist.Sort(delegate (ProjectViewModels pair1, ProjectViewModels pair2)
            {
                if (pair1.FirstYield > 0 && pair2.FirstYield <= 0)
                {
                    return -1;
                }
                else if (pair1.FirstYield <= 0 && pair2.FirstYield > 0)
                {
                    return 1;
                }
                else
                {

                    if (pair1.RetestYield > 0 && pair2.RetestYield <= 0)
                    {
                        return -1;
                    }
                    else if (pair1.RetestYield <= 0 && pair2.RetestYield > 0)
                    {
                        return 1;
                    }
                    else
                    {
                        if (pair1.FinishRating < pair2.FinishRating)
                        {
                            return -1;
                        }
                        else if (pair1.FinishRating > pair2.FinishRating)
                        {
                            return 1;
                        }
                    }
                }



                if (Convert.ToInt32(pair1.PendingFACount.Split(new string[] { "/" }, StringSplitOptions.None)[1])
                > Convert.ToInt32(pair2.PendingFACount.Split(new string[] { "/" }, StringSplitOptions.None)[1]))
                {
                    return -1;
                }
                else if (Convert.ToInt32(pair1.PendingFACount.Split(new string[] { "/" }, StringSplitOptions.None)[1])
                < Convert.ToInt32(pair2.PendingFACount.Split(new string[] { "/" }, StringSplitOptions.None)[1]))
                {
                    return 1;
                }

                if (Convert.ToInt32(pair1.PendingRMACount.Split(new string[] { "/" }, StringSplitOptions.None)[1])
                > Convert.ToInt32(pair2.PendingRMACount.Split(new string[] { "/" }, StringSplitOptions.None)[1]))
                {
                    return -1;
                }
                else if (Convert.ToInt32(pair1.PendingRMACount.Split(new string[] { "/" }, StringSplitOptions.None)[1])
                < Convert.ToInt32(pair2.PendingRMACount.Split(new string[] { "/" }, StringSplitOptions.None)[1]))
                {
                    return 1;
                }

                if (Convert.ToInt32(pair1.PendingTaskCount.Split(new string[] { "/" }, StringSplitOptions.None)[1])
                > Convert.ToInt32(pair2.PendingTaskCount.Split(new string[] { "/" }, StringSplitOptions.None)[1]))
                {
                    return -1;
                }
                else if (Convert.ToInt32(pair1.PendingTaskCount.Split(new string[] { "/" }, StringSplitOptions.None)[1])
                < Convert.ToInt32(pair2.PendingTaskCount.Split(new string[] { "/" }, StringSplitOptions.None)[1]))
                {
                    return 1;
                }

                return 0;
            });
        }

        private List<SelectListItem> CreateSelectList(List<string> valist, string defVal)
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

        private List<SelectListItem> CreateSelectList1(List<string> valist, string defVal)
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

        private void CreateAllUserLists(ProjectViewModels vm)
        {
            var pmlist = new List<ProjectMembers>();
            var eglist = new List<ProjectMembers>();
            var pelist = new List<ProjectMembers>();
            var melist = new List<ProjectMembers>();
            var pqelist = new List<ProjectMembers>();

            foreach (var pm in vm.MemberList)
            {
                if (string.Compare(pm.Role, Prometheus.Models.ProjectViewModels.PMROLE) == 0)
                {
                    pmlist.Add(pm);
                    vm.PMListStr = vm.PMListStr + ";" + pm.Name;
                }
            }

            foreach (var eg in vm.MemberList)
            {
                if (string.Compare(eg.Role, Prometheus.Models.ProjectViewModels.ENGROLE) == 0)
                {
                    eglist.Add(eg);
                    vm.EngListStr = vm.EngListStr + ";" + eg.Name;
                }
            }

            foreach (var pe in vm.MemberList)
            {
                if (string.Compare(pe.Role, Prometheus.Models.ProjectViewModels.PEROLE) == 0)
                {
                    pelist.Add(pe);
                    vm.PeListStr = vm.PeListStr + ";" + pe.Name;
                }
            }

            foreach (var me in vm.MemberList)
            {
                if (string.Compare(me.Role, Prometheus.Models.ProjectViewModels.MEROLE) == 0)
                {
                    melist.Add(me);
                    vm.MeListStr = vm.MeListStr + ";" + me.Name;
                }
            }

            foreach (var pqe in vm.MemberList)
            {
                if (string.Compare(pqe.Role, Prometheus.Models.ProjectViewModels.PQEROLE) == 0)
                {
                    pqelist.Add(pqe);
                    vm.PqeListStr = vm.PqeListStr + ";" + pqe.Name;
                }
            }
        }

        public ActionResult CreateProject()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {
                var vm = new ProjectViewModels();
                CreateAllUserLists(vm);
                CreateProjectTypeList(vm);

                var asilist = UserViewModels.RetrieveAllUser();
                ViewBag.towholist = CreateSelectList(asilist, "");

                var cfgpjlist = CfgUtility.GetStandardPJList(this);
                var suggestpjlist = new List<string>();
                suggestpjlist.AddRange(cfgpjlist.Values);
                ViewBag.ProjectStandardList = Newtonsoft.Json.JsonConvert.SerializeObject(suggestpjlist.ToArray());

                return View();
            }
            else
            {
                var ck = new Dictionary<string, string>();
                ck.Add("logonredirectctrl", "Project");
                ck.Add("logonredirectact", "CreateProject");
                CookieUtility.SetCookie(this, ck);
                return RedirectToAction("LoginUser", "User");
            }
        }

        private static string RMSpectialCh(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                {
                    sb.Append(c);
                }
            }
            return sb.ToString().ToUpper();
        }

        private bool EmailAddressValidate(string emailaddress)
        {
            try
            {
                var eaddrs = emailaddress.Split(new char[] { ':' });
                foreach (var item in eaddrs)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        var m = new MailAddress(emailaddress);
                    }
                }

                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }


        private bool ProjectValidate(ProjectViewModels projectmodel, bool updateproject = false)
        {
            var createerror = "<h3><font color=\"red\">Fail to create/modify project: <ErrorMsg></font></h3>";
            if (string.IsNullOrEmpty(projectmodel.ProjectName.Trim())
                || string.IsNullOrEmpty(projectmodel.ProjectKey.Trim()))
            {
                ViewBag.CreateError = createerror.Replace("<ErrorMsg>", "Project Name is empty");
                return false;
            }

            //foreach(var eg in projectmodel.MemberList)
            //{
            //    if (string.IsNullOrEmpty(eg.Name.Trim()))
            //    {
            //        ViewBag.CreateError = createerror.Replace("<ErrorMsg>", "PM or Engineer email address is empty");
            //        return false;
            //    }
            //}

            //if (string.IsNullOrEmpty(projectmodel.Description.Trim()))
            //{
            //    ViewBag.CreateError = createerror.Replace("<ErrorMsg>", "Project description is empty");
            //    return false;
            //}

            if (!updateproject)
            {
                if (projectmodel.CheckExistProject())
                {
                    ViewBag.CreateError = createerror.Replace("<ErrorMsg>", "Project exist");
                    return false;
                }
            }

            var pmlist = new List<ProjectMembers>();
            var eglist = new List<ProjectMembers>();

            foreach (var pm in projectmodel.MemberList)
            {
                if (string.Compare(pm.Role, Prometheus.Models.ProjectViewModels.PMROLE) == 0)
                {
                    pmlist.Add(pm);
                }
            }

            foreach (var eg in projectmodel.MemberList)
            {
                if (string.Compare(eg.Role, Prometheus.Models.ProjectViewModels.ENGROLE) == 0)
                {
                    eglist.Add(eg);
                }
            }

            if (pmlist.Count == 0)
            {
                ViewBag.CreateError = createerror.Replace("<ErrorMsg>", "At least one PM should be choosed");
                return false;
            }

            if (eglist.Count == 0)
            {
                ViewBag.CreateError = createerror.Replace("<ErrorMsg>", "At least one Engineer should be choosed");
                return false;
            }

            if (projectmodel.StationList.Count > 0 && projectmodel.SumDatasetList.Count > 0)
            {
                ViewBag.CreateError = createerror.Replace("<ErrorMsg>", "A project can not bonding MES and ATE at same time");
                return false;
            }

            //foreach (var eg in projectmodel.MemberList)
            //{
            //    if (!EmailAddressValidate(eg.Name.Trim()))
            //    {
            //        ViewBag.CreateError = createerror.Replace("<ErrorMsg>", "PM or Engineer email address is wrong");
            //        return false;
            //    }
            //}

            return true;
        }

        private List<ProjectMesTable> RetriveMesTables(string mesfilepath)
        {
            var ret = new List<ProjectMesTable>();
            try
            {
                if (System.IO.File.Exists(mesfilepath))
                {
                    string[] lines = System.IO.File.ReadAllLines(mesfilepath);
                    bool tableseg = false;
                    foreach (var line in lines)
                    {
                        if (line.Contains(";"))
                        {
                            continue;
                        }

                        if (line.ToUpper().Contains("[MESTABLENAME]"))
                        {
                            tableseg = true;
                            continue;
                        }

                        if (tableseg && line.Contains("[") && line.Contains("]"))
                        {
                            tableseg = false;
                        }

                        if (tableseg && line.Contains("="))
                        {
                            ret.Add(new ProjectMesTable("", line.Split(new char[] { '=' })[0].Trim(), line.Split(new char[] { '=' })[1].Trim().Replace("\"", "")));
                        }
                    }
                }

                return ret;
            }
            catch (Exception ex)
            {
                ret.Clear();
                return ret;
            }
        }

        private bool StoreMesConfig(ProjectViewModels projectmodel)
        {
            try
            {
                foreach (string fl in Request.Files)
                {
                    if (fl != null && Request.Files[fl].ContentLength > 0
                        && string.Compare(Path.GetExtension(Path.GetFileName(Request.Files[fl].FileName)), ".ini", true) == 0)
                    {
                        string fn = System.IO.Path.GetFileName(Request.Files[fl].FileName);
                        string datestring = DateTime.Now.ToString("yyyyMMdd");
                        string imgdir = Server.MapPath("~/userfiles") + "\\docs\\" + datestring + "\\";

                        if (!Directory.Exists(imgdir))
                        {
                            Directory.CreateDirectory(imgdir);
                        }

                        fn = Path.GetFileNameWithoutExtension(fn) + "-" + DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(fn);
                        Request.Files[fl].SaveAs(imgdir + fn);

                        var ret = RetriveMesTables(imgdir + fn);
                        if (ret.Count > 0)
                        {
                            foreach (var tb in ret)
                            {
                                tb.ProjectKey = projectmodel.ProjectKey;
                            }

                            projectmodel.TabList = ret;
                            return true;
                        }
                        else
                            return false;
                    }
                }
                return false;

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private List<string> RetrieveProjectInfo(string key, int count)
        {
            var ret = new List<string>();
            try
            {
                for (var i = 1; i < count + 1; i++)
                {
                    if (!string.IsNullOrEmpty(Request.Form[key + i].ToString())
                        && string.Compare(Request.Form[key + i].ToString(), "NONE") != 0)
                    {
                        ret.Add(Request.Form[key + i].ToString());
                    }
                }
                return ret;
            }
            catch (Exception ex)
            {
                ret.Clear();
                return ret;
            }
        }

        private void RetrievePorjectKey(ProjectViewModels projectmodel)
        {
            if (!string.IsNullOrEmpty(projectmodel.ProjectName))
            {
                var tempstr = RMSpectialCh(projectmodel.ProjectName);
                projectmodel.ProjectKey = tempstr.Substring(0, (tempstr.Length > 40) ? 38 : tempstr.Length).ToUpper();
            }
            else
                projectmodel.ProjectKey = "";
        }

        private void RetrieveProjectMember2(ProjectViewModels projectmodel)
        {

            var lpm = new List<ProjectMembers>();
            var pns = Request.Form["PMAddr"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in pns)
            {
                lpm.Add(new ProjectMembers(projectmodel.ProjectKey, p, ProjectViewModels.PMROLE));
            }

            pns = Request.Form["ENGAddr"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in pns)
            {
                lpm.Add(new ProjectMembers(projectmodel.ProjectKey, p, ProjectViewModels.ENGROLE));
            }

            pns = Request.Form["PEAddr"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in pns)
            {
                lpm.Add(new ProjectMembers(projectmodel.ProjectKey, p, ProjectViewModels.PEROLE));
            }

            pns = Request.Form["MEAddr"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in pns)
            {
                lpm.Add(new ProjectMembers(projectmodel.ProjectKey, p, ProjectViewModels.MEROLE));
            }

            pns = Request.Form["PQEAddr"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in pns)
            {
                lpm.Add(new ProjectMembers(projectmodel.ProjectKey, p, ProjectViewModels.PQEROLE));
            }

            projectmodel.MemberList = lpm;
        }

        private void RetrieveProjectDesc(ProjectViewModels projectmodel)
        {
            var temphtml = Request.Form["editor1"];
            if (string.IsNullOrEmpty(temphtml))
            {
                projectmodel.Description = "";
            }
            else
            {
                projectmodel.Description = SeverHtmlDecode.Decode(this, temphtml);
            }
        }

        private bool RetrieveProjectDate(ProjectViewModels projectmodel)
        {
            if (!string.IsNullOrEmpty(Request.Form["StartDate"].Trim()))
            {
                try
                {
                    projectmodel.StartDate = DateTime.Parse(Request.Form["StartDate"]);
                    return true;
                }
                catch (Exception ex)
                {
                    ViewBag.CreateError = "<h3><font color=\"red\">Fail to create/modify project: " + RMSpectialCh(ex.ToString()) + "</font></h3>";
                    return false;
                }
            }
            else
            {
                ViewBag.CreateError = ViewBag.CreateError = "<h3><font color=\"red\">Fail to create/modify project: StartDate is empty</font></h3>";
                return false;
            }
        }

        private void RetrievePNs(ProjectViewModels projectmodel)
        {
            projectmodel.PNs = Request.Form["PNs"];
            if (!string.IsNullOrEmpty(Request.Form["OSAPNs"]))
            {
                projectmodel.PNs = Request.Form["OSAPNs"];
            }
        }

        private void RetrieveStation(ProjectViewModels projectmodel)
        {
            projectmodel.Stations = Request.Form["Stations"];
        }

        [HttpPost, ActionName("CreateProject")]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePostProject()
        {
            var projectmodel = new ProjectViewModels();
            projectmodel.ProjectName = Request.Form["ProjectName"];
            projectmodel.FinishRating = 0;

            projectmodel.ProjectType = Request.Form["projecttypelist"].ToString();


            RetrievePorjectKey(projectmodel);

            RetrieveProjectMember2(projectmodel);
            RetrieveProjectDesc(projectmodel);

            StoreMesConfig(projectmodel);
            RetrievePNs(projectmodel);
            RetrieveStation(projectmodel);



            projectmodel.ModelIDs = Request.Form["ModelIDs"];
            projectmodel.SumDatasets = Request.Form["SumDatasets"];

            if (!RetrieveProjectDate(projectmodel))
            {
                var asilist = UserViewModels.RetrieveAllUser();
                ViewBag.towholist = CreateSelectList(asilist, "");

                CreateAllUserLists(projectmodel);
                CreateProjectTypeList(projectmodel);

                var cfgpjlist = CfgUtility.GetStandardPJList(this);
                var suggestpjlist = new List<string>();
                suggestpjlist.AddRange(cfgpjlist.Values);
                ViewBag.ProjectStandardList = Newtonsoft.Json.JsonConvert.SerializeObject(suggestpjlist.ToArray());

                return View(projectmodel);
            }


            if (!ProjectValidate(projectmodel))
            {
                var asilist = UserViewModels.RetrieveAllUser();
                ViewBag.towholist = CreateSelectList(asilist, "");

                CreateAllUserLists(projectmodel);
                CreateProjectTypeList(projectmodel);

                var cfgpjlist = CfgUtility.GetStandardPJList(this);
                var suggestpjlist = new List<string>();
                suggestpjlist.AddRange(cfgpjlist.Values);
                ViewBag.ProjectStandardList = Newtonsoft.Json.JsonConvert.SerializeObject(suggestpjlist.ToArray());

                return View(projectmodel);
            }

            projectmodel.StoreProject();

            //Store OSA failured code map
            StoreOSAFailuredCodeMap(projectmodel);

            var bondingprocess = Request.Form["ChoosedProcess"];
            if (!string.IsNullOrEmpty(bondingprocess)
                && !bondingprocess.ToUpper().Contains("PLEASE"))
            {
                ProjectViewModels.StoreProjectProcessBonding(projectmodel.ProjectKey, bondingprocess);
            }


            IssueViewModels.CreateNPIProcTasks(projectmodel.ProjectName, projectmodel.ProjectKey, projectmodel.MemberList[0].Name);

            var ckdict = CookieUtility.UnpackCookie(this);
            var who = (ckdict["logonuser"]).Split(new string[] { "||" }, StringSplitOptions.None)[0];
            //ProjectEvent.CreateProjectEvent(who, projectmodel.ProjectKey, projectmodel.ProjectName);

            if (projectmodel.OSATabList.Count > 0)
            {
                MESUtility.StartOSAProjectBonding(projectmodel);
            }
            else
            {
                MESUtility.StartProjectBonding(projectmodel);
            }

            BIDataUtility.StartProjectBonding(this, projectmodel);
            ATEUtility.StartProjectBonding(projectmodel);


            
            return RedirectToAction("ViewAll");
        }

        private void CreateUpdateIssueList(ProjectViewModels vm)
        {
            var pslist = new List<SelectListItem>();

            var psitem = new SelectListItem();
            psitem.Text = "True";
            psitem.Value = "True";
            pslist.Add(psitem);

            psitem = new SelectListItem();
            psitem.Text = "False";
            psitem.Value = "False";
            pslist.Add(psitem);

            if (vm != null)
            {
                if (vm.FinishRating < 90)
                {
                    pslist[0].Selected = true;
                }
                else
                {
                    pslist[1].Selected = true;
                }
            }
            else
            {
                pslist[0].Selected = true;
            }

            ViewBag.updateissuelist = pslist;
        }

        private void CreateMonitorVcselList(ProjectViewModels vm)
        {
            var pslist = new List<SelectListItem>();

            var psitem = new SelectListItem();
            psitem.Text = "True";
            psitem.Value = "True";
            pslist.Add(psitem);

            psitem = new SelectListItem();
            psitem.Text = "False";
            psitem.Value = "False";
            pslist.Add(psitem);

            if (vm != null)
            {
                if (string.IsNullOrEmpty(vm.MonitorVcsel)
                    || string.Compare(vm.MonitorVcsel, "True", true) == 0)
                {
                    pslist[0].Selected = true;
                }
                else
                {
                    pslist[1].Selected = true;
                }
            }
            else
            {
                pslist[0].Selected = true;
            }

            ViewBag.monitorvcsellist = pslist;
        }

        private void CreateProjectTypeList(ProjectViewModels vm)
        {
            var pjtypelist = new List<string>();
            var syscfg = CfgUtility.GetSysConfig(this);
            var pjtylist = syscfg["PROJECTTYPE"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var pjt in pjtylist)
            {
                pjtypelist.Add(pjt);
            }
            //pjtypelist.Add(ProjectTypeInf.Parallel);
            //pjtypelist.Add(ProjectTypeInf.Tunable);
            //pjtypelist.Add(ProjectTypeInf.OSA);
            //pjtypelist.Add(ProjectTypeInf.LineCard);
            //pjtypelist.Add(ProjectTypeInf.QM);
            //pjtypelist.Add(ProjectTypeInf.Others);

            bool selected = false;
            var pslist = new List<SelectListItem>();
            foreach (var item in pjtypelist)
            {
                var psitem = new SelectListItem();
                psitem.Text = item;
                psitem.Value = item;
                if (vm != null && string.Compare(vm.ProjectType, item, true) == 0)
                {
                    psitem.Selected = true;
                    selected = true;
                }
                pslist.Add(psitem);
            }

            if (!selected)
            {
                pslist[0].Selected = true;
            }

            ViewBag.projecttypelist = pslist;
        }

        public ActionResult EditProject(string ProjectKey)
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            if (!string.IsNullOrEmpty(ProjectKey) || ckdict.ContainsKey("ProjectKey"))
            {
                var realkey = "";
                if (!string.IsNullOrEmpty(ProjectKey))
                    realkey = ProjectKey;
                else
                    realkey = ckdict["ProjectKey"];

                if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
                {
                    var vm = ProjectViewModels.RetrieveOneProject(realkey);

                    var asilist = UserViewModels.RetrieveAllUser();
                    ViewBag.towholist = CreateSelectList(asilist, "");

                    CreateAllUserLists(vm);
                    CreateUpdateIssueList(vm);
                    CreateMonitorVcselList(vm);
                    CreateProjectTypeList(vm);

                    var pjexcept = ProjectViewModels.RetrieveProjectExcept(realkey, ProjectExceptType.WAFERYIELDEXCEPT);
                    vm.WaferYieldExceptList = pjexcept.Except;

                    var bondingedprocess = ProjectViewModels.RetriveProjectProcessBonding(realkey);
                    if (bondingedprocess.Count > 0)
                    {
                        ViewBag.ChoosedProcessData = bondingedprocess[0].Except;
                    }

                    var projectprocesslist = ProcessData.GetCurrentProjectWorkflowSteps(realkey);
                    if (projectprocesslist.Count > 0)
                    {
                        ViewBag.ProcessList = CreateSelectList(projectprocesslist, "");
                    }

                    return View(vm);
                }
                else
                {
                    var ck = new Dictionary<string, string>();
                    ck.Add("logonredirectctrl", "Project");
                    ck.Add("logonredirectact", "EditProject");
                    ck.Add("ProjectKey", realkey);
                    CookieUtility.SetCookie(this, ck);
                    return RedirectToAction("LoginUser", "User");
                }

            }
            else
            {
                return RedirectToAction("CreateProject", "Project");
            }
        }

        private bool DataBondingChanged(ProjectViewModels oldpjdata, ProjectViewModels newpjdata)
        {
            var pndict = new Dictionary<string, bool>();
            foreach (var pn in oldpjdata.PNList)
            {
                if (!pndict.ContainsKey(pn.Pn))
                {
                    pndict.Add(pn.Pn, true);
                }

            }

            bool changed = false;

            foreach (var pn in newpjdata.PNList)
            {
                if (!pndict.ContainsKey(pn.Pn))
                {
                    changed = true;
                    break;
                }
            }

            if (changed)
                return changed;

            var stationdict = new Dictionary<string, bool>();
            foreach (var stat in oldpjdata.StationList)
            {
                if (!stationdict.ContainsKey(stat.Station))
                {
                    stationdict.Add(stat.Station, true);
                }
            }

            foreach (var stat in newpjdata.StationList)
            {
                if (!stationdict.ContainsKey(stat.Station))
                {
                    changed = true;
                    break;
                }
            }

            return changed;
        }

        private bool PNBondingChanged(ProjectViewModels oldpjdata, ProjectViewModels newpjdata)
        {
            var pndict = new Dictionary<string, bool>();
            foreach (var pn in oldpjdata.PNList)
            {
                if (!pndict.ContainsKey(pn.Pn))
                {
                    pndict.Add(pn.Pn, true);
                }
            }

            bool changed = false;

            foreach (var pn in newpjdata.PNList)
            {
                if (!pndict.ContainsKey(pn.Pn))
                {
                    changed = true;
                    break;
                }
            }

            return changed;
        }

        [HttpPost, ActionName("EditProject")]
        [ValidateAntiForgeryToken]
        public ActionResult EditProjectPost()
        {
            var projectmodel = new ProjectViewModels();
            projectmodel.ProjectName = Request.Form["ProjectName"];

            projectmodel.ProjectType = Request.Form["projecttypelist"].ToString();

            //try {projectmodel.FinishRating = Convert.ToDouble(Request.Form["FinishRating"])%100; }
            //catch(Exception ex) { projectmodel.FinishRating = 0; }
            if (string.Compare(Request.Form["updateissuelist"].ToString(), "True", true) == 0)
            {
                projectmodel.FinishRating = 0;
            }
            else
            {
                projectmodel.FinishRating = 99;
            }

            projectmodel.MonitorVcsel = Request.Form["monitorvcsellist"].ToString();

            if (string.IsNullOrEmpty(Request.Form["VcselWarningYield"]))
            {
                projectmodel.VcselWarningYield = "98";
            }
            else
            {
                var tempval = 98.0;
                try
                { tempval = Convert.ToDouble(Request.Form["VcselWarningYield"]); }
                catch (Exception ex) { tempval = 98.0; }
                projectmodel.VcselWarningYield = (tempval % 100.0).ToString("0.0");
            }

            RetrievePorjectKey(projectmodel);

            RetrieveProjectMember2(projectmodel);
            RetrieveProjectDesc(projectmodel);

            StoreMesConfig(projectmodel);
            RetrievePNs(projectmodel);
            RetrieveStation(projectmodel);
            projectmodel.ModelIDs = Request.Form["ModelIDs"];
            projectmodel.SumDatasets = Request.Form["SumDatasets"];

            if (!RetrieveProjectDate(projectmodel) || !ProjectValidate(projectmodel, true))
            {
                var dict1 = new RouteValueDictionary();
                dict1.Add("ProjectKey", projectmodel.ProjectKey);
                return RedirectToAction("ProjectDetail", "Project", dict1);
            }

            var waferyieldexceptlist = Request.Form["WaferYieldExceptList"];
            ProjectViewModels.UpdateProjectExcept(projectmodel.ProjectKey, waferyieldexceptlist, ProjectExceptType.WAFERYIELDEXCEPT);

            var oldpjdata = ProjectViewModels.RetrieveOneProject(projectmodel.ProjectKey);

            bool databondingchange = DataBondingChanged(oldpjdata, projectmodel);
            bool pnbondingchg = PNBondingChanged(oldpjdata, projectmodel);

            projectmodel.StoreProject();

            var bondingprocess = Request.Form["ChoosedProcess"];
            ProjectViewModels.StoreProjectProcessBonding(projectmodel.ProjectKey, bondingprocess);

            //Store OSA failured code map
            StoreOSAFailuredCodeMap(projectmodel);

            if (projectmodel.OSATabList.Count > 0)
            {
                //do nothing
                //ProjectTestData.PrePareOSALatestData(projectmodel.ProjectKey, this);
            }
            else
            {
                if (projectmodel.TabList.Count == 0)
                {
                    projectmodel.TabList = oldpjdata.TabList;
                }


                //var ckdict = CookieUtility.UnpackCookie(this);
                //var who = (ckdict["logonuser"]).Split(new string[] { "||" }, StringSplitOptions.None)[0];
                //ProjectEvent.UpdateProjectEvent(who, projectmodel.ProjectKey, projectmodel.ProjectName);

                if (databondingchange)
                {
                    MESUtility.StartProjectBonding(projectmodel);
                }

                if (pnbondingchg)
                {
                    BIDataUtility.StartProjectBonding(this, projectmodel);
                }
            }

            ProjectTestData.PrePareATELatestData(projectmodel.ProjectKey,this);

            var dict = new RouteValueDictionary();
            dict.Add("ProjectKey", projectmodel.ProjectKey);
            return RedirectToAction("ProjectDetail", "Project", dict);

        }

        public ActionResult ProjectIssues(string ProjectKey)
        {
            if (ProjectKey != null)
            {
                var list1 = IssueViewModels.RetrieveTaskByProjectKey(ProjectKey, Resolute.Pending);
                var list2 = IssueViewModels.RetrieveTaskByProjectKey(ProjectKey, Resolute.Working);
                var list3 = IssueViewModels.RetrieveTaskByProjectKey(ProjectKey, Resolute.Done);
                list1.AddRange(list2);
                list1.AddRange(list3);

                return View(list1);
            }

            return View();
        }

        public ActionResult ProjectDetail(string ProjectKey)
        {
            if (ProjectKey != null)
            {
                var ckdict = CookieUtility.UnpackCookie(this);
                if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
                {

                }
                else
                {
                    var ck = new Dictionary<string, string>();
                    ck.Add("logonredirectctrl", "Project");
                    ck.Add("logonredirectact", "ViewAll");
                    CookieUtility.SetCookie(this, ck);
                    return RedirectToAction("LoginUser", "User");
                }
                var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
                UserAuth(updater);

                var vm = ProjectViewModels.RetrieveOneProject(ProjectKey);

                if (vm != null)
                {
                    var startdate = DateTime.Now.AddDays(-7);
                    var enddate = DateTime.Now.ToString();
                    if (startdate.DayOfWeek != DayOfWeek.Thursday)
                    {
                        for (int i = 7; i > 0; i--)
                        {
                            if (DateTime.Now.AddDays(0 - i).DayOfWeek == DayOfWeek.Thursday)
                            {
                                startdate = DateTime.Now.AddDays(0 - i);
                                break;
                            }
                        }
                    }

                    var mycache = HttpContext.Cache;
                    var firstyield = mycache.Get(vm.ProjectKey + "_FPY_CUST");
                    if (firstyield == null)
                    {
                        var yvm = ProjectYieldViewModule.GetYieldByDateRange(vm.ProjectKey, startdate.ToString(), enddate, vm, HttpContext.Cache);
                        if (yvm.FirstYields.Count > 0)
                        {
                            vm.FirstYield = yvm.FirstYield;
                            vm.RetestYield = yvm.LastYield;
                        }
                        else
                        {
                            vm.FirstYield = -1.0;
                            vm.RetestYield = -1.0;
                        }
                        mycache.Insert(vm.ProjectKey + "_FPY_CUST", vm.FirstYield, null, DateTime.Now.AddHours(2), Cache.NoSlidingExpiration);
                        mycache.Insert(vm.ProjectKey + "_FY_CUST", vm.RetestYield, null, DateTime.Now.AddHours(2), Cache.NoSlidingExpiration);
                    }
                    else
                    {
                        vm.FirstYield = Convert.ToDouble(mycache.Get(vm.ProjectKey + "_FPY_CUST"));
                        vm.RetestYield = Convert.ToDouble(mycache.Get(vm.ProjectKey + "_FY_CUST"));
                    }


                    NPIInfo(vm);

                    var taskcount = mycache.Get(vm.ProjectKey + "_taskct_CUST");
                    if (taskcount == null)
                    {
                        var taskdone = IssueViewModels.RetrieveTaskCountByProjectKey(vm.ProjectKey, Resolute.Done);
                        var tasktotal = taskdone + IssueViewModels.RetrieveTaskCountByProjectKey(vm.ProjectKey, Resolute.Pending)
                            + IssueViewModels.RetrieveTaskCountByProjectKey(vm.ProjectKey, Resolute.Working);
                        vm.PendingTaskCount = taskdone.ToString() + "/" + tasktotal.ToString();
                        mycache.Insert(vm.ProjectKey + "_taskct_CUST", vm.PendingTaskCount, null, DateTime.Now.AddHours(4), Cache.NoSlidingExpiration);
                    }
                    else
                    {
                        vm.PendingTaskCount = Convert.ToString(taskcount);
                    }

                    var facount = mycache.Get(vm.ProjectKey + "_fact_CUST");
                    if (facount == null)
                    {
                        var fadone = ProjectFAViewModules.RetrieveFADataCount(vm.ProjectKey, false);
                        var fatotal = fadone + ProjectFAViewModules.RetrieveFADataCount(vm.ProjectKey);
                        vm.PendingFACount = fadone.ToString() + "/" + fatotal.ToString();
                        mycache.Insert(vm.ProjectKey + "_fact_CUST", vm.PendingFACount, null, DateTime.Now.AddHours(4), Cache.NoSlidingExpiration);
                    }
                    else
                    {
                        vm.PendingFACount = Convert.ToString(facount);
                    }

                    var rmacount = mycache.Get(vm.ProjectKey + "_rmact_CUST");
                    if (rmacount == null)
                    {
                        var rmadone = IssueViewModels.RetrieveRMACountByProjectKey(vm.ProjectKey, Resolute.Done);
                        var rmatotal = rmadone + IssueViewModels.RetrieveRMACountByProjectKey(vm.ProjectKey, Resolute.Pending)
                            + IssueViewModels.RetrieveRMACountByProjectKey(vm.ProjectKey, Resolute.Working);
                        vm.PendingRMACount = rmadone.ToString() + "/" + rmatotal.ToString();
                        mycache.Insert(vm.ProjectKey + "_rmact_CUST", vm.PendingRMACount, null, DateTime.Now.AddHours(4), Cache.NoSlidingExpiration);
                    }
                    else
                    {
                        vm.PendingRMACount = Convert.ToString(rmacount);
                    }

                }

                return View(vm);
            }
            return View();
        }

        public ActionResult ProjectFA(string ProjectKey)
        {
            if (!string.IsNullOrEmpty(ProjectKey))
            {
                ViewBag.projectkey = ProjectKey;

                var vm = ProjectFAViewModules.RetrievePendingFAData(ProjectKey, this);

                var piedatadict = new Dictionary<string, int>();
                foreach (var item in vm)
                {
                    if (piedatadict.ContainsKey(item.TestData.ErrAbbr))
                    {
                        var preval = piedatadict[item.TestData.ErrAbbr];
                        piedatadict[item.TestData.ErrAbbr] = preval + 1;
                    }
                    else
                    {
                        piedatadict.Add(item.TestData.ErrAbbr, 1);
                    }
                }

                var keys = piedatadict.Keys;
                if (keys.Count > 0)
                {
                    var namevaluepair = "";
                    foreach (var k in keys)
                    {
                        namevaluepair = namevaluepair + "{ name:'" + k + "',y:" + piedatadict[k].ToString() + "},";
                    }

                    namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                    var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart.xml"));
                    ViewBag.chartscript = tempscript.Replace("#ElementID#", "failurepie")
                        .Replace("#Title#", ProjectKey + " FA Realtime Failure")
                        .Replace("#SERIESNAME#", "Failure")
                        .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                }

                var asilist = UserViewModels.RetrieveAllUser();
                ViewBag.AllUserList = CreateSelectList1(asilist, "");

                var ckdict = CookieUtility.UnpackCookie(this);
                if (ckdict.ContainsKey("logonuser"))
                {
                    ViewBag.logined = true;
                    ViewBag.UpdaterName = ckdict["logonuser"].Split(new char[] { '|' })[0].ToUpper();
                }

                return View(vm);
            }
            return View();
        }

        public ActionResult ProjectSptTask(string ProjectKey)
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {

            }
            else
            {
                var ck = new Dictionary<string, string>();
                ck.Add("logonredirectctrl", "Project");
                ck.Add("logonredirectact", "ProjectSptTask");
                ck.Add("sptpjkey", ProjectKey);
                CookieUtility.SetCookie(this, ck);
                return RedirectToAction("LoginUser", "User");
            }

            if (string.IsNullOrEmpty(ProjectKey))
            {
                if (ckdict.ContainsKey("sptpjkey"))
                {
                    ProjectKey = ckdict["sptpjkey"];
                }
                else
                {
                    return RedirectToAction("ViewAll", "Project");
                }
            }

            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

            //null is for qm, not null for parallel project
            var vm = IssueViewModels.RetrieveSptIssue(this, ProjectKey);
            ViewBag.rules = ProjectCriticalErrorVM.RetrievePJCriticalError(ProjectKey, null);

            var ProjectInfo = ProjectViewModels.RetrieveOneProject(ProjectKey);
            var pqeFlag = false;
            if (ProjectInfo.MemberList.Count > 0)
            {
                foreach (var pqe in ProjectInfo.MemberList)
                {
                    if (string.Compare(pqe.Role, Prometheus.Models.ProjectViewModels.PQEROLE) == 0)
                    {
                        if (string.Compare(updater, pqe.Name, true) == 0)
                        {
                            pqeFlag = true;
                            break;
                        }
                    }
                }
            }

            ViewBag.pqeFlag = pqeFlag;
            return View(vm);
        }

        public ActionResult ProjectError(string ProjectKey)
        {
            if (!string.IsNullOrEmpty(ProjectKey))
            {
                var vm = ProjectErrorViewModels.RetrieveErrorByPJKey(ProjectKey, this);
                var piedatadict = new Dictionary<string, int>();
                foreach (var item in vm)
                {
                    if (!piedatadict.ContainsKey(item.OrignalCode))
                    {
                        piedatadict.Add(item.OrignalCode, item.ErrorCount);
                    }
                }

                var keys = piedatadict.Keys;
                if (keys.Count > 0)
                {
                    var namevaluepair = "";
                    foreach (var k in keys)
                    {
                        namevaluepair = namevaluepair + "{ name:'" + k + "',y:" + piedatadict[k].ToString() + "},";
                    }

                    namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                    var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart.xml"));
                    ViewBag.chartscript = tempscript.Replace("#ElementID#", "failurepie")
                        .Replace("#Title#", ProjectKey + " Realtime Failure")
                        .Replace("#SERIESNAME#", "Failure")
                        .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                }

                return View(vm);
            }
            return View();
        }

        public ActionResult ProjectErrAbbr(string ProjectKey, string ErrAbbr, string StartDate = null, string EndDate = null)
        {
            if (!string.IsNullOrEmpty(ProjectKey) && !string.IsNullOrEmpty(ErrAbbr))
            {
                ViewBag.projectkey = ProjectKey;

                var vm = ProjectFAViewModules.RetrieveFADataWithErrAbbr(ProjectKey, ErrAbbr, this);

                var pendingitems = new List<ProjectFAViewModules>();
                var tempitems = new List<ProjectFAViewModules>();

                foreach (var item in vm)
                {
                    if (item.IssueData.Resolution != Resolute.Pending
                        && item.IssueData.Resolution != Resolute.Working
                        && item.IssueData.Resolution != Resolute.Reopen)
                    {

                        if (string.Compare(item.IssueData.Resolution, Resolute.AutoClose, true) == 0)
                        {
                            continue;
                        }

                        if (item.IssueData.CommentList.Count == 2)
                        {
                            var removesameasissue = false;
                            foreach (var c in item.IssueData.CommentList)
                            {
                                if (c.Comment.Contains("<p>Issue Same As <a"))
                                {
                                    removesameasissue = true;
                                    break;
                                }

                                if (c.Comment.Contains("passed")
                                    && string.Compare(c.Reporter, "System", true) == 0)
                                {
                                    removesameasissue = true;
                                    break;
                                }
                            }
                            if (removesameasissue)
                            {
                                continue;
                            }
                        }

                        tempitems.Add(item);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(StartDate) && !string.IsNullOrEmpty(EndDate))
                        {
                            try
                            {
                                var startdate = DateTime.Parse(StartDate).AddHours(-12);
                                var enddate = DateTime.Parse(EndDate).AddHours(12);
                                if (item.IssueData.ReportDate > enddate || item.IssueData.ReportDate < startdate)
                                {
                                    continue;
                                }
                            }
                            catch (Exception ex) { }
                        }//end if

                        pendingitems.Add(item);
                    }
                }

                if (tempitems.Count > 0)
                {
                    var pslist = new List<SelectListItem>();
                    foreach (var item in tempitems)
                    {
                        var pitem = new SelectListItem();
                        pitem.Text = item.IssueData.Summary;
                        pitem.Value = item.IssueData.IssueKey;
                        pslist.Add(pitem);
                    }
                    ViewBag.DoneIssueList = pslist;
                }

                var asilist = UserViewModels.RetrieveAllUser();
                ViewBag.AllUserList = CreateSelectList1(asilist, "");
                var ckdict = CookieUtility.UnpackCookie(this);
                if (ckdict.ContainsKey("logonuser"))
                {
                    ViewBag.logined = true;
                }


                pendingitems.AddRange(tempitems);

                return View(pendingitems);
            }
            return RedirectToAction("ViewAll", "Project");
        }

        public ActionResult SNIssue(string ProjectKey, string SN)
        {
            if (!string.IsNullOrEmpty(ProjectKey) && !string.IsNullOrEmpty(SN))
            {
                ViewBag.projectkey = ProjectKey;

                var vm = ProjectFAViewModules.RetrieveFADataWithSN(ProjectKey, SN, this);

                var asilist = UserViewModels.RetrieveAllUser();
                ViewBag.AllUserList = CreateSelectList1(asilist, "");
                var ckdict = CookieUtility.UnpackCookie(this);
                if (ckdict.ContainsKey("logonuser"))
                {
                    ViewBag.logined = true;
                }
                return View(vm);
            }
            return RedirectToAction("ViewAll", "Project");
        }

        [HttpPost, ActionName("ProjectDoneFA")]
        [ValidateAntiForgeryToken]
        public ActionResult ProjectDoneFA()
        {
            var projectkey = Request.Form["HProjectKey"];
            ViewBag.projectkey = projectkey;

            var vm = ProjectFAViewModules.RetrieveDoneFAData(projectkey, this);

            var piedatadict = new Dictionary<string, int>();
            foreach (var item in vm)
            {
                if (piedatadict.ContainsKey(item.TestData.ErrAbbr))
                {
                    var preval = piedatadict[item.TestData.ErrAbbr];
                    piedatadict[item.TestData.ErrAbbr] = preval + 1;
                }
                else
                {
                    piedatadict.Add(item.TestData.ErrAbbr, 1);
                }
            }

            var keys = piedatadict.Keys;
            if (keys.Count > 0)
            {
                var namevaluepair = "";
                foreach (var k in keys)
                {
                    namevaluepair = namevaluepair + "{ name:'" + k + "',y:" + piedatadict[k].ToString() + "},";
                }

                namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart.xml"));
                ViewBag.chartscript = tempscript.Replace("#ElementID#", "failurepie")
                    .Replace("#Title#", projectkey + " Done FA Failure")
                    .Replace("#SERIESNAME#", "Failure")
                    .Replace("#NAMEVALUEPAIRS#", namevaluepair);
            }

            var asilist = UserViewModels.RetrieveAllUser();
            ViewBag.AllUserList = CreateSelectList1(asilist, "");
            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser"))
            {
                ViewBag.logined = true;
            }

            return View(vm);
        }

        public ActionResult ProjectNPI(string ProjectKey)
        {
            if (ProjectKey != null)
            {
                var vmlist = IssueViewModels.RetrieveNPIPROCIssue(ProjectKey, this);
                var pj = ProjectViewModels.RetrieveOneProject(ProjectKey);

                var ckdict = CookieUtility.UnpackCookie(this);
                var updater = "";
                if (ckdict.ContainsKey("logonuser"))
                {
                    updater = ckdict["logonuser"].Split(new char[] { '|' })[0].ToUpper();
                }

                ViewBag.authrized = false;
                foreach (var item in pj.MemberList)
                {
                    if (string.Compare(item.Name, updater, true) == 0)
                    {
                        ViewBag.authrized = true;
                        break;
                    }
                }

                return View(vmlist);
            }
            return View();
        }

        public static void ProjectWeeklyTrend(Controller ctrl, string ProjectKey, int weeks)
        {
            ctrl.ViewBag.Weeks = weeks.ToString();

            var vmlist = ProjectYieldViewModule.GetYieldByWeeks(ProjectKey, ctrl.HttpContext.Cache, weeks);
            if (vmlist.Count > 0)
            {
                var ChartxAxisValues = "";
                //var ChartSearies = "";

                var ftimelist = new List<string>();
                var famountlist = new List<int>();
                var fyieldlist = new List<double>();
                var ryieldlist = new List<double>();
                var rtyieldlist = new List<double>();
                var snyieldlist = new List<double>();
                var maxamout = 0;

                foreach (var item in vmlist)
                {
                    ftimelist.Add(item.EndDate.ToString("yyyy-MM-dd"));

                    fyieldlist.Add(item.FirstYield * 100.0);
                    ryieldlist.Add(item.LastYield * 100.0);
                    rtyieldlist.Add(item.RealTimeYield * 100.0);
                    snyieldlist.Add(item.SNYield * 100);

                    var tempfamount = 0;
                    foreach (var d in item.FirstYields)
                    {
                        if (d.InputCount > tempfamount) { tempfamount = d.InputCount; }
                        if (d.InputCount > maxamout) { maxamout = d.InputCount; }
                    }
                    famountlist.Add(tempfamount);
                }

                //xaxis
                foreach (var item in ftimelist)
                {
                    ChartxAxisValues = ChartxAxisValues + "'" + item + "',";
                }
                ChartxAxisValues = ChartxAxisValues.Substring(0, ChartxAxisValues.Length - 1);


                //yaxis
                //ChartSearies = "{name:'First Yield',data:[<fvalue>]},{name:'Retest Yield',data:[<cvalue>]}";

                var famout = "";
                foreach (var item in famountlist)
                {
                    famout = famout + item.ToString() + ",";
                }
                famout = famout.Substring(0, famout.Length - 1);

                var ftempvalue = "";
                foreach (var item in fyieldlist)
                {
                    ftempvalue = ftempvalue + item.ToString("0.00") + ",";
                }
                ftempvalue = ftempvalue.Substring(0, ftempvalue.Length - 1);
                //ChartSearies = ChartSearies.Replace("<fvalue>", tempvalue);

                var rttempvalue = "";
                foreach (var item in rtyieldlist)
                {
                    rttempvalue = rttempvalue + item.ToString("0.00") + ",";
                }
                rttempvalue = rttempvalue.Substring(0, rttempvalue.Length - 1);

                var sntempvalue = "";
                foreach (var item in snyieldlist)
                {
                    sntempvalue = sntempvalue + item.ToString("0.00") + ",";
                }
                sntempvalue = sntempvalue.Substring(0, sntempvalue.Length - 1);

                var rtempvalue = "";
                foreach (var item in ryieldlist)
                {
                    rtempvalue = rtempvalue + item.ToString("0.00") + ",";
                }
                rtempvalue = rtempvalue.Substring(0, rtempvalue.Length - 1);
                //ChartSearies = ChartSearies.Replace("<cvalue>", tempvalue);

                var FINALTOOLTIP = "";
                var REALTIMETOOLTIP = "";


                for (var idx = 0; idx < rtyieldlist.Count; idx++)
                {
                    FINALTOOLTIP = FINALTOOLTIP + "'<!doctype html><table>"
                        + "<tr><td><b>FPY</b></td><td>" + fyieldlist[idx].ToString("0.00") + "&#37;</td></tr>"
                        + "<tr><td><b>FY</b></td><td>" + ryieldlist[idx].ToString("0.00") + "&#37;</td></tr>";

                    foreach (var d in vmlist[idx].LastYields)
                    {
                        FINALTOOLTIP = FINALTOOLTIP + "<tr><td><b>" + d.WhichTest + "</b></td><td>Input:</td><td>" + d.InputCount.ToString() + "</td><td>Output:</td><td>" + d.OutputCount.ToString() + "</td></tr>";
                    }

                    FINALTOOLTIP = FINALTOOLTIP + "</table>'";
                    FINALTOOLTIP = FINALTOOLTIP + ",";
                }
                FINALTOOLTIP = FINALTOOLTIP.Substring(0, FINALTOOLTIP.Length - 1);


                for (var idx = 0; idx < rtyieldlist.Count; idx++)
                {
                    REALTIMETOOLTIP = REALTIMETOOLTIP + "'<!doctype html><table>"
                        + "<tr><td><b>Realtime Yield</b></td><td>" + rtyieldlist[idx].ToString("0.00") + "&#37;</td></tr>";
                    foreach (var d in vmlist[idx].RealTimeYields)
                    {
                        REALTIMETOOLTIP = REALTIMETOOLTIP + "<tr><td><b>" + d.WhichTest + "</b></td><td>Input:</td><td>" + d.InputCount.ToString() + "</td><td>Output:</td><td>" + d.OutputCount.ToString() + "</td></tr>";
                    }
                    REALTIMETOOLTIP = REALTIMETOOLTIP + "</table>'";
                    REALTIMETOOLTIP = REALTIMETOOLTIP + ",";
                }
                REALTIMETOOLTIP = REALTIMETOOLTIP.Substring(0, REALTIMETOOLTIP.Length - 1);

                var SNTOOLTIP = "";
                for (var idx = 0; idx < snyieldlist.Count; idx++)
                {
                    SNTOOLTIP = SNTOOLTIP + "'<!doctype html><table>"
                        + "<tr><td><b>SN Trace Yield</b></td><td>" + snyieldlist[idx].ToString("0.00") + "&#37;</td></tr>";
                    foreach (var d in vmlist[idx].SNYields)
                    {
                        SNTOOLTIP = SNTOOLTIP + "<tr><td><b>" + d.WhichTest + "</b></td><td>Input:</td><td>" + d.InputCount.ToString() + "</td><td>Output:</td><td>" + d.OutputCount.ToString() + "</td></tr>";
                    }
                    SNTOOLTIP = SNTOOLTIP + "</table>'";
                    SNTOOLTIP = SNTOOLTIP + ",";
                }
                SNTOOLTIP = SNTOOLTIP.Substring(0, SNTOOLTIP.Length - 1);

                //rederect url
                var reurl = "window.location.href = '/Project/ProjectWYieldDetail?ProjectKey=" + ProjectKey + "'" + "+'&EndDate='+this.category" + "+'&Weeks='+'" + weeks.ToString() + "'";

                var tempscript = System.IO.File.ReadAllText(ctrl.Server.MapPath("~/Scripts/SuperYield.xml"));
                ctrl.ViewBag.chartscript = tempscript.Replace("#ElementID#", "weeklyyield")
                    .Replace("#Title#", "Weekly Yield Trend")
                    .Replace("#ChartxAxisValues#", ChartxAxisValues)
                    .Replace("#XAxisTitle#", "Date")
                    .Replace("#AmountMAX#", maxamout.ToString())
                    .Replace("#FirstAmount#", famout)
                    .Replace("#FirstYield#", ftempvalue)
                    .Replace("#RetestYield#", rtempvalue)
                    .Replace("#RealTimeYield#", rttempvalue)
                    .Replace("#SNYield#", sntempvalue)
                    .Replace("#FINALTOOLTIP#", FINALTOOLTIP)
                    .Replace("#REALTIMETOOLTIP#", REALTIMETOOLTIP)
                    .Replace("#SNTOOLTIP#", SNTOOLTIP)
                    .Replace("#REDIRECTURL#", reurl);

            }
        }

        public ActionResult ProjectYield(string ProjectKey, int Weeks)
        {
            if (ProjectKey != null)
            {
                ViewBag.pjkey = ProjectKey;

                ProjectWeeklyTrend(this, ProjectKey, Weeks);

                return View();
            }
            return View();
        }

        public ActionResult ProjectYieldMain(string ProjectKey)
        {
            var sarray = new string[] { "8", "16", "24", "32", "40", "48", "56" };
            var slist = new List<string>();
            slist.Add("Weekly Yield Trend");
            slist.AddRange(sarray);
            var weeklylist = CreateSelectList1(slist, "");
            weeklylist[0].Selected = true;
            weeklylist[0].Disabled = true;
            ViewBag.weeklylist = weeklylist;

            sarray = new string[] { "1", "2", "3", "4", "5", "6", "12", "18", "24" };
            slist = new List<string>();
            slist.Add("Monthly Yield Trend");
            slist.AddRange(sarray);
            var monthlylist = CreateSelectList1(slist, "");
            monthlylist[0].Selected = true;
            monthlylist[0].Disabled = true;
            ViewBag.monthlylist = monthlylist;

            sarray = new string[] { YIELDTYPE.BR, YIELDTYPE.JO };
            slist = new List<string>();
            slist.AddRange(sarray);
            ViewBag.yieldtypelist = CreateSelectList1(slist, "");

            if (ProjectKey != null)
            {
                ViewBag.pjkey = ProjectKey;
                ProjectWeeklyTrend(this, ProjectKey, 4);

                var allprocname = new List<string>();
                var bondingedprocess = ProjectViewModels.RetriveProjectProcessBonding(ProjectKey);
                if (bondingedprocess.Count > 0)
                {
                    allprocname = bondingedprocess[0].Except.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }
                else
                {
                    allprocname = ProcessData.RetrieveLastWeekProcess(ProjectKey);
                }

                var detailinfo = new Dictionary<string, List<ProjectMoveHistory>>();
                var procdata = ProcessData.RetrieveLastWeekProcessData(ProjectKey, detailinfo);
                var processdatatable = new List<ProjectMoveHistory>();
                foreach (var proc in allprocname)
                {
                    if (procdata.ContainsKey(proc))
                    {
                        processdatatable.Add(procdata[proc]);
                    }
                }
                if (detailinfo.Count > 0)
                    ViewBag.detailinfo = detailinfo;
                if (processdatatable.Count > 0)
                    ViewBag.processdatatable = processdatatable;

                var jolist = new List<string>();
                jolist.Add("Please select JO");
                var templist = ProcessData.RetrieveJoList(ProjectKey);
                jolist.AddRange(templist);
                var selectcontrol = CreateSelectList1(jolist, "");
                selectcontrol[0].Disabled = true;
                selectcontrol[0].Selected = true;
                ViewBag.procjolist = selectcontrol;

                return View();
            }

            return View();
        }

        public ActionResult ProjectProcessYield(string ProjectKey, string PBRNUM)
        {
            if (!string.IsNullOrEmpty(ProjectKey)
                && !string.IsNullOrEmpty(PBRNUM))
            {
                ViewBag.pjkey = ProjectKey;
                ViewBag.brnum = PBRNUM;

                var allprocname = new List<string>();
                var bondingedprocess = ProjectViewModels.RetriveProjectProcessBonding(ProjectKey);
                if (bondingedprocess.Count > 0)
                {
                    allprocname = bondingedprocess[0].Except.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }
                else
                {
                    allprocname = ProcessData.RetrieveProcessWithSequenceByMfg(ProjectKey, PBRNUM);
                }

                var detailinfo = new Dictionary<string, List<ProjectMoveHistory>>();
                var procdata = ProcessData.RetrieveProcessDataByMfg(ProjectKey, PBRNUM, detailinfo);
                var processdatatable = new List<ProjectMoveHistory>();
                foreach (var proc in allprocname)
                {
                    if (procdata.ContainsKey(proc))
                    {
                        processdatatable.Add(procdata[proc]);
                    }
                }
                if (detailinfo.Count > 0)
                    ViewBag.detailinfo = detailinfo;
                if (processdatatable.Count > 0)
                    ViewBag.processdatatable = processdatatable;

                //var jolist = new List<string>();
                //jolist.Add("Please select JO");
                //var templist = ProcessData.RetrieveJoList(ProjectKey);
                //jolist.AddRange(templist);
                //var selectcontrol = CreateSelectList1(jolist, "");
                //selectcontrol[0].Disabled = true;
                //selectcontrol[0].Selected = true;
                //ViewBag.procjolist = selectcontrol;

                return View();
            }
            return View();
        }

        public ActionResult ProjectWYieldDetail(string ProjectKey, string EndDate, string Weeks)
        {
            ViewBag.Weeks = Weeks;

            if (!string.IsNullOrEmpty(ProjectKey) && !string.IsNullOrEmpty(EndDate))
            {
                var edate = DateTime.Parse(DateTime.Parse(EndDate).ToString("yyyy-MM-dd") + " 07:30:00");
                var sdate = edate.AddDays(-7);
                if (sdate.DayOfWeek != DayOfWeek.Thursday)
                {
                    for (int i = 6; i > 0; i--)
                    {
                        sdate = edate.AddDays(0 - i);
                        if (sdate.DayOfWeek == DayOfWeek.Thursday)
                        {
                            break;
                        }
                    }
                }

                var firstdatalist = new List<KeyValuePair<string, int>>();
                var retestdatalist = new List<KeyValuePair<string, int>>();
                var fytestdatalist = new List<KeyValuePair<string, int>>();

                var pvm = ProjectViewModels.RetrieveOneProject(ProjectKey);
                var yieldvm = ProjectYieldViewModule.GetYieldByDateRange(ProjectKey, sdate.ToString(), edate.ToString(), pvm, HttpContext.Cache);

                if (yieldvm.FirstYields.Count > 0)
                {
                    var piedatadict = new Dictionary<string, int>();
                    var eklist = new List<string>();
                    foreach (var error in yieldvm.FErrorMap.Keys)
                    {
                        eklist.Add(error);
                    }

                    foreach (var error in eklist)
                    {
                        if (string.Compare(error, "PASS", true) != 0)
                        {
                            foreach (var test in yieldvm.FirstYields)
                            {
                                var val = ProjectYieldViewModule.RetrieveErrorCount(error, test.WhichTest, yieldvm.FErrorMap);

                                if (piedatadict.ContainsKey(error))
                                {
                                    var preval = piedatadict[error];
                                    piedatadict[error] = preval + val;
                                }
                                else
                                {
                                    piedatadict.Add(error, val);
                                }
                            }
                        }
                    }

                    firstdatalist = piedatadict.ToList();

                    piedatadict["PASS"] = ProjectYieldViewModule.RetrieveErrorCount("PASS", yieldvm.FirstYields[yieldvm.FirstYields.Count - 1].WhichTest, yieldvm.FErrorMap);

                    var keys = piedatadict.Keys;
                    var namevaluepair = "";
                    foreach (var k in keys)
                    {
                        if (piedatadict[k] > 0)
                            namevaluepair = namevaluepair + "{ name:'" + k + "',y:" + piedatadict[k].ToString() + "},";
                    }

                    namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                    var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart.xml"));
                    ViewBag.fchartscript = tempscript.Replace("#ElementID#", "ffailurepie")
                        .Replace("#Title#", "First Failure")
                        .Replace("#SERIESNAME#", "FFailure")
                        .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                }


                if (yieldvm.SNYields.Count > 0)
                {
                    var piedatadict = new Dictionary<string, int>();
                    var eklist = new List<string>();
                    foreach (var error in yieldvm.SNErrorMap.Keys)
                    {
                        eklist.Add(error);
                    }

                    foreach (var error in eklist)
                    {
                        if (string.Compare(error, "PASS", true) != 0)
                        {
                            foreach (var test in yieldvm.SNYields)
                            {
                                var val = ProjectYieldViewModule.RetrieveErrorCount(error, test.WhichTest, yieldvm.SNErrorMap);

                                if (piedatadict.ContainsKey(error))
                                {
                                    var preval = piedatadict[error];
                                    piedatadict[error] = preval + val;
                                }
                                else
                                {
                                    piedatadict.Add(error, val);
                                }
                            }
                        }
                    }

                    retestdatalist = piedatadict.ToList();

                    piedatadict["PASS"] = ProjectYieldViewModule.RetrieveErrorCount("PASS", yieldvm.SNYields[yieldvm.SNYields.Count - 1].WhichTest, yieldvm.SNErrorMap);

                    var keys = piedatadict.Keys;
                    var namevaluepair = "";
                    foreach (var k in keys)
                    {
                        if (piedatadict[k] > 0)
                            namevaluepair = namevaluepair + "{ name:'" + k + "',y:" + piedatadict[k].ToString() + "},";
                    }

                    namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                    var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart.xml"));
                    ViewBag.rchartscript = tempscript.Replace("#ElementID#", "rfailurepie")
                        .Replace("#Title#", "SN Trace Failure")
                        .Replace("#SERIESNAME#", "SNFailure")
                        .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                }

                if (yieldvm.LastYields.Count > 0)
                {
                    var piedatadict = new Dictionary<string, int>();
                    var eklist = new List<string>();
                    foreach (var error in yieldvm.LErrorMap.Keys)
                    {
                        eklist.Add(error);
                    }

                    foreach (var error in eklist)
                    {
                        if (string.Compare(error, "PASS", true) != 0)
                        {
                            foreach (var test in yieldvm.LastYields)
                            {
                                var val = ProjectYieldViewModule.RetrieveErrorCount(error, test.WhichTest, yieldvm.LErrorMap);

                                if (piedatadict.ContainsKey(error))
                                {
                                    var preval = piedatadict[error];
                                    piedatadict[error] = preval + val;
                                }
                                else
                                {
                                    piedatadict.Add(error, val);
                                }
                            }
                        }
                    }

                    fytestdatalist = piedatadict.ToList();
                    piedatadict["PASS"] = ProjectYieldViewModule.RetrieveErrorCount("PASS", yieldvm.LastYields[yieldvm.LastYields.Count - 1].WhichTest, yieldvm.LErrorMap);

                    var keys = piedatadict.Keys;
                    var namevaluepair = "";
                    foreach (var k in keys)
                    {
                        if (piedatadict[k] > 0)
                            namevaluepair = namevaluepair + "{ name:'" + k + "',y:" + piedatadict[k].ToString() + "},";
                    }

                    namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                    var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart.xml"));
                    ViewBag.fychartscript = tempscript.Replace("#ElementID#", "fyfailurepie")
                        .Replace("#Title#", "Final Failure")
                        .Replace("#SERIESNAME#", "RFailure")
                        .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                }

                if (firstdatalist.Count > 0)
                {
                    firsttestparetofun(firstdatalist, ProjectKey, sdate.ToString(), edate.ToString());
                }

                if (retestdatalist.Count > 0)
                {
                    retestparetofun(retestdatalist, ProjectKey, sdate.ToString(), edate.ToString());
                }

                if (fytestdatalist.Count > 0)
                {
                    fytparetofun(fytestdatalist, ProjectKey, sdate.ToString(), edate.ToString());
                }

                return View(yieldvm);
            }
            return View();
        }

        public ActionResult ProjectRMA(string ProjectKey)
        {
            if (ProjectKey != null)
            {
                var list1 = IssueViewModels.RetrieveIssueTypeByProjectKey(ProjectKey, Resolute.Pending, ISSUETP.RMA, this);
                var list2 = IssueViewModels.RetrieveIssueTypeByProjectKey(ProjectKey, Resolute.Working, ISSUETP.RMA, this);
                var list3 = IssueViewModels.RetrieveIssueTypeByProjectKey(ProjectKey, Resolute.Done, ISSUETP.RMA, this);
                list1.AddRange(list2);
                list1.AddRange(list3);
                return View(list1);
            }
            return View();
        }

        public ActionResult ProjectRMAStatus(string ProjectKey)
        {
            if (ProjectKey != null)
            {
                var list1 = IssueViewModels.RetrieveIssueTypeByProjectKey(ProjectKey, Resolute.Pending, ISSUETP.RMA, this);
                var list2 = IssueViewModels.RetrieveIssueTypeByProjectKey(ProjectKey, Resolute.Working, ISSUETP.RMA, this);
                var list3 = IssueViewModels.RetrieveIssueTypeByProjectKey(ProjectKey, Resolute.Done, ISSUETP.RMA, this);
                list1.AddRange(list2);
                list1.AddRange(list3);

                var piedatadict = new Dictionary<string, int>();
                foreach (var item in list1)
                {
                    var rmafailurecode = item.RMAFailureCode.ToLower().Trim();
                    if (!string.IsNullOrEmpty(rmafailurecode))
                    {
                        if (piedatadict.ContainsKey(rmafailurecode))
                        {
                            var preval = piedatadict[rmafailurecode];
                            piedatadict[rmafailurecode] = preval + 1;
                        }
                        else
                        {
                            piedatadict.Add(rmafailurecode, 1);
                        }
                    }
                }

                var keys = piedatadict.Keys;
                if (keys.Count > 0)
                {
                    var namevaluepair = "";
                    foreach (var k in keys)
                    {
                        namevaluepair = namevaluepair + "{ name:'" + k + "',y:" + piedatadict[k].ToString() + "},";
                    }

                    namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                    var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart.xml"));
                    ViewBag.chartscript = tempscript.Replace("#ElementID#", "failurepie")
                        .Replace("#Title#", ProjectKey + " RMA Realtime Failure")
                        .Replace("#SERIESNAME#", "Failure")
                        .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                }

                return View(list1);
            }
            return View();
        }

        public ActionResult ProjectReliability(string ProjectKey)
        {
            if (ProjectKey != null)
            {
                var list1 = IssueViewModels.RetrieveIssueTypeByProjectKey(ProjectKey, Resolute.Working, ISSUETP.Rel, this);
                var list2 = IssueViewModels.RetrieveIssueTypeByProjectKey(ProjectKey, Resolute.Pending, ISSUETP.Rel, this);
                var list3 = IssueViewModels.RetrieveIssueTypeByProjectKey(ProjectKey, Resolute.Done, ISSUETP.Rel, this);
                list1.AddRange(list2);
                list1.AddRange(list3);

                var piedatadict = new Dictionary<string, int>();
                foreach (var item in list1)
                {
                    var rmafailurecode = item.FVCode.ToLower().Trim();
                    if (!string.IsNullOrEmpty(rmafailurecode))
                    {
                        if (piedatadict.ContainsKey(rmafailurecode))
                        {
                            var preval = piedatadict[rmafailurecode];
                            piedatadict[rmafailurecode] = preval + 1;
                        }
                        else
                        {
                            piedatadict.Add(rmafailurecode, 1);
                        }
                    }
                }

                var keys = piedatadict.Keys;
                if (keys.Count > 0)
                {
                    var namevaluepair = "";
                    foreach (var k in keys)
                    {
                        namevaluepair = namevaluepair + "{ name:'" + k + "',y:" + piedatadict[k].ToString() + "},";
                    }

                    namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                    var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart.xml"));
                    ViewBag.chartscript = tempscript.Replace("#ElementID#", "failurepie")
                        .Replace("#Title#", "Reliability Failure")
                        .Replace("#SERIESNAME#", "Failure")
                        .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                }

                return View(list1);
            }
            return View();
        }

        public ActionResult ProjectOBA(string ProjectKey)
        {
            if (ProjectKey != null)
            {
                //var list1 = IssueViewModels.RetrieveIssueTypeByProjectKey(ProjectKey, Resolute.Pending, ISSUETP.OBA, this);
                //var list2 = IssueViewModels.RetrieveIssueTypeByProjectKey(ProjectKey, Resolute.Working, ISSUETP.OBA, this);
                //var list3 = IssueViewModels.RetrieveIssueTypeByProjectKey(ProjectKey, Resolute.Done, ISSUETP.OBA, this);
                //list1.AddRange(list2);
                //list1.AddRange(list3);

                var list1 = IssueViewModels.RetrieveIssueTypeByProjectKey(ProjectKey, "NONE", "NONE", ISSUETP.OBA, this);

                //var piedatadict = new Dictionary<string, int>();
                //foreach (var item in list1)
                //{
                //    var rmafailurecode = item.RMAFailureCode.ToLower().Trim();
                //    if (!string.IsNullOrEmpty(rmafailurecode))
                //    {
                //        if (piedatadict.ContainsKey(rmafailurecode))
                //        {
                //            var preval = piedatadict[rmafailurecode];
                //            piedatadict[rmafailurecode] = preval + 1;
                //        }
                //        else
                //        {
                //            piedatadict.Add(rmafailurecode, 1);
                //        }
                //    }
                //}

                //var keys = piedatadict.Keys;
                //if (keys.Count > 0)
                //{
                //    var namevaluepair = "";
                //    foreach (var k in keys)
                //    {
                //        namevaluepair = namevaluepair + "{ name:'" + k + "',y:" + piedatadict[k].ToString() + "},";
                //    }

                //    namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                //    var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart.xml"));
                //    ViewBag.chartscript = tempscript.Replace("#ElementID#", "failurepie")
                //        .Replace("#Title#", ProjectKey + " RMA Realtime Failure")
                //        .Replace("#SERIESNAME#", "Failure")
                //        .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                //}

                return View(list1);
            }
            return View();
        }

        public ActionResult ProjectQuality(string ProjectKey)
        {
            if (ProjectKey != null)
            {
                //var list1 = IssueViewModels.RetrieveIssueTypeByProjectKey(ProjectKey, Resolute.Pending, ISSUETP.Quality, this);
                //var list2 = IssueViewModels.RetrieveIssueTypeByProjectKey(ProjectKey, Resolute.Working, ISSUETP.Quality, this);
                //var list3 = IssueViewModels.RetrieveIssueTypeByProjectKey(ProjectKey, Resolute.Done, ISSUETP.Quality, this);
                //list1.AddRange(list2);
                //list1.AddRange(list3);

                var list1 = IssueViewModels.RetrieveIssueTypeByProjectKey(ProjectKey, "NONE", "NONE", ISSUETP.Quality, this);

                //var piedatadict = new Dictionary<string, int>();
                //foreach (var item in list1)
                //{
                //    var rmafailurecode = item.RMAFailureCode.ToLower().Trim();
                //    if (!string.IsNullOrEmpty(rmafailurecode))
                //    {
                //        if (piedatadict.ContainsKey(rmafailurecode))
                //        {
                //            var preval = piedatadict[rmafailurecode];
                //            piedatadict[rmafailurecode] = preval + 1;
                //        }
                //        else
                //        {
                //            piedatadict.Add(rmafailurecode, 1);
                //        }
                //    }
                //}

                //var keys = piedatadict.Keys;
                //if (keys.Count > 0)
                //{
                //    var namevaluepair = "";
                //    foreach (var k in keys)
                //    {
                //        namevaluepair = namevaluepair + "{ name:'" + k + "',y:" + piedatadict[k].ToString() + "},";
                //    }

                //    namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                //    var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart.xml"));
                //    ViewBag.chartscript = tempscript.Replace("#ElementID#", "failurepie")
                //        .Replace("#Title#", ProjectKey + " RMA Realtime Failure")
                //        .Replace("#SERIESNAME#", "Failure")
                //        .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                //}

                return View(list1);
            }
            return View();
        }

        /*public ActionResult ProjectMonthlyYield(string ProjectKey)
        {
            if (ProjectKey != null)
            {
                ViewBag.pjkey = ProjectKey;

                var vmlist = ProjectYieldViewModule.GetYieldByMonth(ProjectKey);
                if (vmlist.Count > 0)
                {
                    var ChartxAxisValues = "";
                    var ChartSearies = "";

                    var ftimelist = new List<string>();
                    var fvalues = new List<double>();
                    var cvalues = new List<double>();

                    foreach (var item in vmlist)
                    {
                        ftimelist.Add(item.EndDate.ToString("yyyy-MM-dd"));
                        fvalues.Add(item.FirstYield * 100.0);
                        cvalues.Add(item.LastYield * 100.0);
                    }

                    //xaxis
                    foreach (var item in ftimelist)
                    {
                        ChartxAxisValues = ChartxAxisValues + "'" + item + "',";
                    }
                    ChartxAxisValues = ChartxAxisValues.Substring(0, ChartxAxisValues.Length - 1);


                    //yaxis
                    ChartSearies = "{name:'First Yield',data:[<fvalue>]},{name:'Retest Yield',data:[<cvalue>]}";

                    var tempvalue = "";
                    foreach (var item in fvalues)
                    {
                        tempvalue = tempvalue + item.ToString("0.00") + ",";
                    }
                    tempvalue = tempvalue.Substring(0, tempvalue.Length - 1);
                    ChartSearies = ChartSearies.Replace("<fvalue>", tempvalue);

                    tempvalue = "";
                    foreach (var item in cvalues)
                    {
                        tempvalue = tempvalue + item.ToString("0.00") + ",";
                    }
                    tempvalue = tempvalue.Substring(0, tempvalue.Length - 1);
                    ChartSearies = ChartSearies.Replace("<cvalue>", tempvalue);

                    //rederect url
                    var reurl = "window.location.href = '/Project/ProjectMYieldDetail?ProjectKey=" + ProjectKey + "'" + "+'&EndDate='+this.category";

                    var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/ColumnChart.xml"));
                    ViewBag.chartscript = tempscript.Replace("#ElementID#", "monthlyyield")
                        .Replace("#ChartType#", "column")
                        .Replace("#Title#", "Monthly Yield")
                        .Replace("#ChartxAxisValues#", ChartxAxisValues)
                        .Replace("#yAxisTitle#", "Yield Percent")
                        .Replace("#ChartSearies#", ChartSearies)
                        .Replace("#REDIRECTURL#", reurl);
                }

                return View();
            }
            return View();

        }*/

        public ActionResult ProjectMonthlyYield(string ProjectKey, int Months)
        {
            if (ProjectKey != null)
            {
                ViewBag.pjkey = ProjectKey;

                var vmlist = ProjectYieldViewModule.GetYieldByMonth(ProjectKey, HttpContext.Cache, Months);
                if (vmlist.Count > 0)
                {
                    var ChartxAxisValues = "";
                    //var ChartSearies = "";

                    var ftimelist = new List<string>();
                    var famountlist = new List<int>();
                    var fyieldlist = new List<double>();
                    var ryieldlist = new List<double>();
                    var rtyieldlist = new List<double>();
                    var snyieldlist = new List<double>();
                    var maxamout = 0;

                    foreach (var item in vmlist)
                    {
                        ftimelist.Add(item.EndDate.ToString("yyyy-MM-dd"));
                        fyieldlist.Add(item.FirstYield * 100.0);
                        ryieldlist.Add(item.LastYield * 100.0);
                        rtyieldlist.Add(item.RealTimeYield * 100.0);
                        snyieldlist.Add(item.SNYield * 100);

                        var tempfamount = 0;
                        foreach (var d in item.FirstYields)
                        {
                            if (d.InputCount > tempfamount) { tempfamount = d.InputCount; }
                            if (d.InputCount > maxamout) { maxamout = d.InputCount; }
                        }
                        famountlist.Add(tempfamount);
                    }

                    //xaxis
                    foreach (var item in ftimelist)
                    {
                        ChartxAxisValues = ChartxAxisValues + "'" + item + "',";
                    }
                    ChartxAxisValues = ChartxAxisValues.Substring(0, ChartxAxisValues.Length - 1);


                    //yaxis
                    //ChartSearies = "{name:'First Yield',data:[<fvalue>]},{name:'Retest Yield',data:[<cvalue>]}";

                    var famout = "";
                    foreach (var item in famountlist)
                    {
                        famout = famout + item.ToString() + ",";
                    }
                    famout = famout.Substring(0, famout.Length - 1);

                    var ftempvalue = "";
                    foreach (var item in fyieldlist)
                    {
                        ftempvalue = ftempvalue + item.ToString("0.00") + ",";
                    }
                    ftempvalue = ftempvalue.Substring(0, ftempvalue.Length - 1);
                    //ChartSearies = ChartSearies.Replace("<fvalue>", tempvalue);

                    var rttempvalue = "";
                    foreach (var item in rtyieldlist)
                    {
                        rttempvalue = rttempvalue + item.ToString("0.00") + ",";
                    }
                    rttempvalue = rttempvalue.Substring(0, rttempvalue.Length - 1);

                    var sntempvalue = "";
                    foreach (var item in snyieldlist)
                    {
                        sntempvalue = sntempvalue + item.ToString("0.00") + ",";
                    }
                    sntempvalue = sntempvalue.Substring(0, sntempvalue.Length - 1);

                    var rtempvalue = "";
                    foreach (var item in ryieldlist)
                    {
                        rtempvalue = rtempvalue + item.ToString("0.00") + ",";
                    }
                    rtempvalue = rtempvalue.Substring(0, rtempvalue.Length - 1);
                    //ChartSearies = ChartSearies.Replace("<cvalue>", tempvalue);

                    var FINALTOOLTIP = "";
                    var REALTIMETOOLTIP = "";

                    for (var idx = 0; idx < rtyieldlist.Count; idx++)
                    {
                        FINALTOOLTIP = FINALTOOLTIP + "'<!doctype html><table>"
                            + "<tr><td><b>FPY</b></td><td>" + fyieldlist[idx].ToString("0.00") + "&#37;</td></tr>"
                            + "<tr><td><b>FY</b></td><td>" + ryieldlist[idx].ToString("0.00") + "&#37;</td></tr>";

                        foreach (var d in vmlist[idx].LastYields)
                        {
                            FINALTOOLTIP = FINALTOOLTIP + "<tr><td><b>" + d.WhichTest + "</b></td><td>Input:</td><td>" + d.InputCount.ToString() + "</td><td>Output:</td><td>" + d.OutputCount.ToString() + "</td></tr>";
                        }

                        FINALTOOLTIP = FINALTOOLTIP + "</table>'";
                        FINALTOOLTIP = FINALTOOLTIP + ",";
                    }
                    FINALTOOLTIP = FINALTOOLTIP.Substring(0, FINALTOOLTIP.Length - 1);


                    for (var idx = 0; idx < rtyieldlist.Count; idx++)
                    {
                        REALTIMETOOLTIP = REALTIMETOOLTIP + "'<!doctype html><table>"
                            + "<tr><td><b>Realtime Yield</b></td><td>" + rtyieldlist[idx].ToString("0.00") + "&#37;</td></tr>";
                        foreach (var d in vmlist[idx].RealTimeYields)
                        {
                            REALTIMETOOLTIP = REALTIMETOOLTIP + "<tr><td><b>" + d.WhichTest + "</b></td><td>Input:</td><td>" + d.InputCount.ToString() + "</td><td>Output:</td><td>" + d.OutputCount.ToString() + "</td></tr>";
                        }
                        REALTIMETOOLTIP = REALTIMETOOLTIP + "</table>'";
                        REALTIMETOOLTIP = REALTIMETOOLTIP + ",";
                    }
                    REALTIMETOOLTIP = REALTIMETOOLTIP.Substring(0, REALTIMETOOLTIP.Length - 1);

                    var SNTOOLTIP = "";
                    for (var idx = 0; idx < snyieldlist.Count; idx++)
                    {
                        SNTOOLTIP = SNTOOLTIP + "'<!doctype html><table>"
                            + "<tr><td><b>SN Trace Yield</b></td><td>" + snyieldlist[idx].ToString("0.00") + "&#37;</td></tr>";
                        foreach (var d in vmlist[idx].SNYields)
                        {
                            SNTOOLTIP = SNTOOLTIP + "<tr><td><b>" + d.WhichTest + "</b></td><td>Input:</td><td>" + d.InputCount.ToString() + "</td><td>Output:</td><td>" + d.OutputCount.ToString() + "</td></tr>";
                        }
                        SNTOOLTIP = SNTOOLTIP + "</table>'";
                        SNTOOLTIP = SNTOOLTIP + ",";
                    }
                    SNTOOLTIP = SNTOOLTIP.Substring(0, SNTOOLTIP.Length - 1);

                    //rederect url
                    var reurl = "window.location.href = '/Project/ProjectMYieldDetail?ProjectKey=" + ProjectKey + "'" + "+'&EndDate='+this.category" + "+'&Months='+'" + Months.ToString() + "'";

                    var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/SuperYield.xml"));
                    ViewBag.chartscript = tempscript.Replace("#ElementID#", "monthlyyield")
                        .Replace("#Title#", "Monthly Yield")
                        .Replace("#ChartxAxisValues#", ChartxAxisValues)
                        .Replace("#XAxisTitle#", "Date")
                        .Replace("#AmountMAX#", maxamout.ToString())
                        .Replace("#FirstAmount#", famout)
                        .Replace("#FirstYield#", ftempvalue)
                        .Replace("#RetestYield#", rtempvalue)
                        .Replace("#RealTimeYield#", rttempvalue)
                        .Replace("#FINALTOOLTIP#", FINALTOOLTIP)
                        .Replace("#REALTIMETOOLTIP#", REALTIMETOOLTIP)
                        .Replace("#SNYield#", sntempvalue)
                        .Replace("#SNTOOLTIP#", SNTOOLTIP)
                        .Replace("#REDIRECTURL#", reurl);
                }

                return View();
            }
            return View();

        }

        public ActionResult ProjectDailyYield(string ProjectKey, string StartDate, string EndDate)
        {
            if (ProjectKey != null && StartDate != null && EndDate != null)
            {
                var EndDate2 = DateTime.Parse(EndDate).AddDays(1).ToString();
                ViewBag.pjkey = ProjectKey;
                var vmlist = ProjectYieldViewModule.GetYieldByDay(ProjectKey, StartDate, EndDate2, HttpContext.Cache);
                if (vmlist.Count > 0)
                {
                    var ChartxAxisValues = "";
                    //var ChartSearies = "";

                    var ftimelist = new List<string>();
                    var famountlist = new List<int>();
                    var fyieldlist = new List<double>();
                    var ryieldlist = new List<double>();
                    var rtyieldlist = new List<double>();
                    var snyieldlist = new List<double>();
                    var maxamout = 0;

                    foreach (var item in vmlist)
                    {
                        ftimelist.Add(item.StartDate.ToString("yyyy-MM-dd"));
                        fyieldlist.Add(item.FirstYield * 100.0);
                        ryieldlist.Add(item.LastYield * 100.0);
                        rtyieldlist.Add(item.RealTimeYield * 100.0);
                        snyieldlist.Add(item.SNYield * 100);

                        var tempfamount = 0;
                        foreach (var d in item.FirstYields)
                        {
                            if (d.InputCount > tempfamount) { tempfamount = d.InputCount; }
                            if (d.InputCount > maxamout) { maxamout = d.InputCount; }
                        }
                        famountlist.Add(tempfamount);
                    }

                    //xaxis
                    foreach (var item in ftimelist)
                    {
                        ChartxAxisValues = ChartxAxisValues + "'" + item + "',";
                    }
                    ChartxAxisValues = ChartxAxisValues.Substring(0, ChartxAxisValues.Length - 1);


                    //yaxis
                    //ChartSearies = "{name:'First Yield',data:[<fvalue>]},{name:'Retest Yield',data:[<cvalue>]}";

                    var famout = "";
                    foreach (var item in famountlist)
                    {
                        famout = famout + item.ToString() + ",";
                    }
                    famout = famout.Substring(0, famout.Length - 1);

                    var ftempvalue = "";
                    foreach (var item in fyieldlist)
                    {
                        ftempvalue = ftempvalue + item.ToString("0.00") + ",";
                    }
                    ftempvalue = ftempvalue.Substring(0, ftempvalue.Length - 1);
                    //ChartSearies = ChartSearies.Replace("<fvalue>", tempvalue);

                    var rttempvalue = "";
                    foreach (var item in rtyieldlist)
                    {
                        rttempvalue = rttempvalue + item.ToString("0.00") + ",";
                    }
                    rttempvalue = rttempvalue.Substring(0, rttempvalue.Length - 1);

                    var sntempvalue = "";
                    foreach (var item in snyieldlist)
                    {
                        sntempvalue = sntempvalue + item.ToString("0.00") + ",";
                    }
                    sntempvalue = sntempvalue.Substring(0, sntempvalue.Length - 1);

                    var rtempvalue = "";
                    foreach (var item in ryieldlist)
                    {
                        rtempvalue = rtempvalue + item.ToString("0.00") + ",";
                    }
                    rtempvalue = rtempvalue.Substring(0, rtempvalue.Length - 1);
                    //ChartSearies = ChartSearies.Replace("<cvalue>", tempvalue);

                    var FINALTOOLTIP = "";
                    var REALTIMETOOLTIP = "";

                    for (var idx = 0; idx < rtyieldlist.Count; idx++)
                    {
                        FINALTOOLTIP = FINALTOOLTIP + "'<!doctype html><table>"
                            + "<tr><td><b>FPY</b></td><td>" + fyieldlist[idx].ToString("0.00") + "&#37;</td></tr>"
                            + "<tr><td><b>FY</b></td><td>" + ryieldlist[idx].ToString("0.00") + "&#37;</td></tr>";

                        foreach (var d in vmlist[idx].LastYields)
                        {
                            FINALTOOLTIP = FINALTOOLTIP + "<tr><td><b>" + d.WhichTest + "</b></td><td>Input:</td><td>" + d.InputCount.ToString() + "</td><td>Output:</td><td>" + d.OutputCount.ToString() + "</td></tr>";
                        }

                        FINALTOOLTIP = FINALTOOLTIP + "</table>'";
                        FINALTOOLTIP = FINALTOOLTIP + ",";
                    }
                    FINALTOOLTIP = FINALTOOLTIP.Substring(0, FINALTOOLTIP.Length - 1);


                    for (var idx = 0; idx < rtyieldlist.Count; idx++)
                    {
                        REALTIMETOOLTIP = REALTIMETOOLTIP + "'<!doctype html><table>"
                            + "<tr><td><b>Realtime Yield</b></td><td>" + rtyieldlist[idx].ToString("0.00") + "&#37;</td></tr>";
                        foreach (var d in vmlist[idx].RealTimeYields)
                        {
                            REALTIMETOOLTIP = REALTIMETOOLTIP + "<tr><td><b>" + d.WhichTest + "</b></td><td>Input:</td><td>" + d.InputCount.ToString() + "</td><td>Output:</td><td>" + d.OutputCount.ToString() + "</td></tr>";
                        }
                        REALTIMETOOLTIP = REALTIMETOOLTIP + "</table>'";
                        REALTIMETOOLTIP = REALTIMETOOLTIP + ",";
                    }
                    REALTIMETOOLTIP = REALTIMETOOLTIP.Substring(0, REALTIMETOOLTIP.Length - 1);

                    var SNTOOLTIP = "";
                    for (var idx = 0; idx < snyieldlist.Count; idx++)
                    {
                        SNTOOLTIP = SNTOOLTIP + "'<!doctype html><table>"
                            + "<tr><td><b>SN Trace Yield</b></td><td>" + snyieldlist[idx].ToString("0.00") + "&#37;</td></tr>";
                        foreach (var d in vmlist[idx].SNYields)
                        {
                            SNTOOLTIP = SNTOOLTIP + "<tr><td><b>" + d.WhichTest + "</b></td><td>Input:</td><td>" + d.InputCount.ToString() + "</td><td>Output:</td><td>" + d.OutputCount.ToString() + "</td></tr>";
                        }
                        SNTOOLTIP = SNTOOLTIP + "</table>'";
                        SNTOOLTIP = SNTOOLTIP + ",";
                    }
                    SNTOOLTIP = SNTOOLTIP.Substring(0, SNTOOLTIP.Length - 1);

                    //rederect url
                    var reurl = "window.location.href = '/Project/ProjectDYieldDetail?ProjectKey=" + ProjectKey + "'" + "+'&EndDate='+this.category+'&VStartDate='+'" + StartDate + "'+'&VEndDate='+'" + EndDate2 + "'";

                    var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/SuperYield.xml"));
                    ViewBag.chartscript = tempscript.Replace("#ElementID#", "dailyyield")
                        .Replace("#Title#", "Daily Yield")
                        .Replace("#ChartxAxisValues#", ChartxAxisValues)
                        .Replace("#XAxisTitle#", "Date")
                        .Replace("#AmountMAX#", maxamout.ToString())
                        .Replace("#FirstAmount#", famout)
                        .Replace("#FirstYield#", ftempvalue)
                        .Replace("#RetestYield#", rtempvalue)
                        .Replace("#RealTimeYield#", rttempvalue)
                        .Replace("#FINALTOOLTIP#", FINALTOOLTIP)
                        .Replace("#REALTIMETOOLTIP#", REALTIMETOOLTIP)
                        .Replace("#SNYield#", sntempvalue)
                        .Replace("#SNTOOLTIP#", SNTOOLTIP)
                        .Replace("#REDIRECTURL#", reurl);
                }

                return View();
            }

            return View();
        }

        public ActionResult ProjectDYieldDetail(string ProjectKey, string EndDate, string VStartDate, string VEndDate)
        {
            if (!string.IsNullOrEmpty(ProjectKey) && !string.IsNullOrEmpty(EndDate))
            {

                ViewBag.StartDate = VStartDate;
                ViewBag.EndDate = VEndDate;

                var edate = DateTime.Parse(DateTime.Parse(EndDate).AddDays(1).ToString("yyyy-MM-dd") + " 07:30:00");
                var sdate = edate.AddDays(-1);
                var firstdatalist = new List<KeyValuePair<string, int>>();
                var retestdatalist = new List<KeyValuePair<string, int>>();
                var fytestdatalist = new List<KeyValuePair<string, int>>();

                var pvm = ProjectViewModels.RetrieveOneProject(ProjectKey);
                var yieldvm = ProjectYieldViewModule.GetYieldByDateRange(ProjectKey, sdate.ToString(), edate.ToString(), pvm, HttpContext.Cache);

                if (yieldvm.FirstYields.Count > 0)
                {
                    var piedatadict = new Dictionary<string, int>();
                    var eklist = new List<string>();
                    foreach (var error in yieldvm.FErrorMap.Keys)
                    {
                        eklist.Add(error);
                    }

                    foreach (var error in eklist)
                    {
                        if (string.Compare(error, "PASS", true) != 0)
                        {
                            foreach (var test in yieldvm.FirstYields)
                            {
                                var val = ProjectYieldViewModule.RetrieveErrorCount(error, test.WhichTest, yieldvm.FErrorMap);

                                if (piedatadict.ContainsKey(error))
                                {
                                    var preval = piedatadict[error];
                                    piedatadict[error] = preval + val;
                                }
                                else
                                {
                                    piedatadict.Add(error, val);
                                }
                            }
                        }
                    }

                    firstdatalist = piedatadict.ToList();

                    piedatadict["PASS"] = ProjectYieldViewModule.RetrieveErrorCount("PASS", yieldvm.FirstYields[yieldvm.FirstYields.Count - 1].WhichTest, yieldvm.FErrorMap);

                    var keys = piedatadict.Keys;
                    var namevaluepair = "";
                    foreach (var k in keys)
                    {
                        if (piedatadict[k] > 0)
                            namevaluepair = namevaluepair + "{ name:'" + k + "',y:" + piedatadict[k].ToString() + "},";
                    }

                    namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                    var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart.xml"));
                    ViewBag.fchartscript = tempscript.Replace("#ElementID#", "ffailurepie")
                        .Replace("#Title#", "First Failure")
                        .Replace("#SERIESNAME#", "FFailure")
                        .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                }


                //if (yieldvm.LastYields.Count > 0)
                //{
                //    var piedatadict = new Dictionary<string, int>();
                //    var eklist = new List<string>();
                //    foreach (var error in yieldvm.LErrorMap.Keys)
                //    {
                //        eklist.Add(error);
                //    }

                //    foreach (var error in eklist)
                //    {
                //        if (string.Compare(error, "PASS", true) != 0)
                //        {
                //            foreach (var test in yieldvm.LastYields)
                //            {
                //                var val = ProjectYieldViewModule.RetrieveErrorCount(error, test.WhichTest, yieldvm.LErrorMap);

                //                if (piedatadict.ContainsKey(error))
                //                {
                //                    var preval = piedatadict[error];
                //                    piedatadict[error] = preval + val;
                //                }
                //                else
                //                {
                //                    piedatadict.Add(error, val);
                //                }
                //            }
                //        }
                //    }

                //    retestdatalist = piedatadict.ToList();

                //    piedatadict["PASS"] = ProjectYieldViewModule.RetrieveErrorCount("PASS", yieldvm.LastYields[yieldvm.LastYields.Count - 1].WhichTest, yieldvm.LErrorMap);

                //    var keys = piedatadict.Keys;
                //    var namevaluepair = "";
                //    foreach (var k in keys)
                //    {
                //        if (piedatadict[k] > 0)
                //            namevaluepair = namevaluepair + "{ name:'" + k + "',y:" + piedatadict[k].ToString() + "},";
                //    }

                //    namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                //    var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart.xml"));
                //    ViewBag.rchartscript = tempscript.Replace("#ElementID#", "rfailurepie")
                //        .Replace("#Title#", "Final Failure")
                //        .Replace("#SERIESNAME#", "RFailure")
                //        .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                //}

                if (yieldvm.SNYields.Count > 0)
                {
                    var piedatadict = new Dictionary<string, int>();
                    var eklist = new List<string>();
                    foreach (var error in yieldvm.SNErrorMap.Keys)
                    {
                        eklist.Add(error);
                    }

                    foreach (var error in eklist)
                    {
                        if (string.Compare(error, "PASS", true) != 0)
                        {
                            foreach (var test in yieldvm.SNYields)
                            {
                                var val = ProjectYieldViewModule.RetrieveErrorCount(error, test.WhichTest, yieldvm.SNErrorMap);

                                if (piedatadict.ContainsKey(error))
                                {
                                    var preval = piedatadict[error];
                                    piedatadict[error] = preval + val;
                                }
                                else
                                {
                                    piedatadict.Add(error, val);
                                }
                            }
                        }
                    }

                    retestdatalist = piedatadict.ToList();

                    piedatadict["PASS"] = ProjectYieldViewModule.RetrieveErrorCount("PASS", yieldvm.SNYields[yieldvm.SNYields.Count - 1].WhichTest, yieldvm.SNErrorMap);

                    var keys = piedatadict.Keys;
                    var namevaluepair = "";
                    foreach (var k in keys)
                    {
                        if (piedatadict[k] > 0)
                            namevaluepair = namevaluepair + "{ name:'" + k + "',y:" + piedatadict[k].ToString() + "},";
                    }

                    namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                    var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart.xml"));
                    ViewBag.rchartscript = tempscript.Replace("#ElementID#", "rfailurepie")
                        .Replace("#Title#", "SN Trace Failure")
                        .Replace("#SERIESNAME#", "SNFailure")
                        .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                }

                if (yieldvm.LastYields.Count > 0)
                {
                    var piedatadict = new Dictionary<string, int>();
                    var eklist = new List<string>();
                    foreach (var error in yieldvm.LErrorMap.Keys)
                    {
                        eklist.Add(error);
                    }

                    foreach (var error in eklist)
                    {
                        if (string.Compare(error, "PASS", true) != 0)
                        {
                            foreach (var test in yieldvm.LastYields)
                            {
                                var val = ProjectYieldViewModule.RetrieveErrorCount(error, test.WhichTest, yieldvm.LErrorMap);

                                if (piedatadict.ContainsKey(error))
                                {
                                    var preval = piedatadict[error];
                                    piedatadict[error] = preval + val;
                                }
                                else
                                {
                                    piedatadict.Add(error, val);
                                }
                            }
                        }
                    }

                    fytestdatalist = piedatadict.ToList();
                    piedatadict["PASS"] = ProjectYieldViewModule.RetrieveErrorCount("PASS", yieldvm.LastYields[yieldvm.LastYields.Count - 1].WhichTest, yieldvm.LErrorMap);

                    var keys = piedatadict.Keys;
                    var namevaluepair = "";
                    foreach (var k in keys)
                    {
                        if (piedatadict[k] > 0)
                            namevaluepair = namevaluepair + "{ name:'" + k + "',y:" + piedatadict[k].ToString() + "},";
                    }

                    namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                    var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart.xml"));
                    ViewBag.fychartscript = tempscript.Replace("#ElementID#", "fyfailurepie")
                        .Replace("#Title#", "Final Failure")
                        .Replace("#SERIESNAME#", "RFailure")
                        .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                }

                if (firstdatalist.Count > 0)
                {
                    firsttestparetofun(firstdatalist, ProjectKey, sdate.ToString(), edate.ToString());
                }

                if (retestdatalist.Count > 0)
                {
                    retestparetofun(retestdatalist, ProjectKey, sdate.ToString(), edate.ToString());
                }

                if (fytestdatalist.Count > 0)
                {
                    fytparetofun(fytestdatalist, ProjectKey, sdate.ToString(), edate.ToString());
                }

                return View(yieldvm);
            }
            return View();
        }

        public ActionResult ProjectMYieldDetail(string ProjectKey, string EndDate, int Months)
        {
            ViewBag.Months = Months.ToString();

            if (!string.IsNullOrEmpty(ProjectKey) && !string.IsNullOrEmpty(EndDate))
            {
                var edate = DateTime.Parse(DateTime.Parse(EndDate).ToString("yyyy-MM-dd") + " 07:30:00");
                var datestr = EndDate.Split(new char[] { '-', ' ' })[2];

                var sdate = DateTime.Parse(EndDate);
                if (Convert.ToInt32(datestr) != 1)
                {
                    sdate = DateTime.Parse(DateTime.Parse(EndDate).ToString("yyyy-MM") + "-01 07:30:00");
                }
                else
                {
                    sdate = sdate.AddMonths(-1);
                }

                var firstdatalist = new List<KeyValuePair<string, int>>();
                var retestdatalist = new List<KeyValuePair<string, int>>();
                var fytestdatalist = new List<KeyValuePair<string, int>>();

                var pvm = ProjectViewModels.RetrieveOneProject(ProjectKey);
                var yieldvm = ProjectYieldViewModule.GetYieldByDateRange(ProjectKey, sdate.ToString(), edate.ToString(), pvm, HttpContext.Cache);

                if (yieldvm.FirstYields.Count > 0)
                {
                    var piedatadict = new Dictionary<string, int>();
                    var eklist = new List<string>();
                    foreach (var error in yieldvm.FErrorMap.Keys)
                    {
                        eklist.Add(error);
                    }

                    foreach (var error in eklist)
                    {
                        if (string.Compare(error, "PASS", true) != 0)
                        {
                            foreach (var test in yieldvm.FirstYields)
                            {
                                var val = ProjectYieldViewModule.RetrieveErrorCount(error, test.WhichTest, yieldvm.FErrorMap);

                                if (piedatadict.ContainsKey(error))
                                {
                                    var preval = piedatadict[error];
                                    piedatadict[error] = preval + val;
                                }
                                else
                                {
                                    piedatadict.Add(error, val);
                                }
                            }
                        }
                    }

                    firstdatalist = piedatadict.ToList();

                    piedatadict["PASS"] = ProjectYieldViewModule.RetrieveErrorCount("PASS", yieldvm.FirstYields[yieldvm.FirstYields.Count - 1].WhichTest, yieldvm.FErrorMap);

                    var keys = piedatadict.Keys;
                    var namevaluepair = "";
                    foreach (var k in keys)
                    {
                        if (piedatadict[k] > 0)
                            namevaluepair = namevaluepair + "{ name:'" + k + "',y:" + piedatadict[k].ToString() + "},";
                    }

                    namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                    var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart.xml"));
                    ViewBag.fchartscript = tempscript.Replace("#ElementID#", "ffailurepie")
                        .Replace("#Title#", "First Failure")
                        .Replace("#SERIESNAME#", "FFailure")
                        .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                }


                //if (yieldvm.LastYields.Count > 0)
                //{
                //    var piedatadict = new Dictionary<string, int>();
                //    var eklist = new List<string>();
                //    foreach (var error in yieldvm.LErrorMap.Keys)
                //    {
                //        eklist.Add(error);
                //    }

                //    foreach (var error in eklist)
                //    {
                //        if (string.Compare(error, "PASS", true) != 0)
                //        {
                //            foreach (var test in yieldvm.LastYields)
                //            {
                //                var val = ProjectYieldViewModule.RetrieveErrorCount(error, test.WhichTest, yieldvm.LErrorMap);

                //                if (piedatadict.ContainsKey(error))
                //                {
                //                    var preval = piedatadict[error];
                //                    piedatadict[error] = preval + val;
                //                }
                //                else
                //                {
                //                    piedatadict.Add(error, val);
                //                }
                //            }
                //        }
                //    }

                //    retestdatalist = piedatadict.ToList();

                //    piedatadict["PASS"] = ProjectYieldViewModule.RetrieveErrorCount("PASS", yieldvm.LastYields[yieldvm.LastYields.Count - 1].WhichTest, yieldvm.LErrorMap);

                //    var keys = piedatadict.Keys;
                //    var namevaluepair = "";
                //    foreach (var k in keys)
                //    {
                //        if (piedatadict[k] > 0)
                //            namevaluepair = namevaluepair + "{ name:'" + k + "',y:" + piedatadict[k].ToString() + "},";
                //    }

                //    namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                //    var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart.xml"));
                //    ViewBag.rchartscript = tempscript.Replace("#ElementID#", "rfailurepie")
                //        .Replace("#Title#", "Final Failure")
                //        .Replace("#SERIESNAME#", "RFailure")
                //        .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                //}

                if (yieldvm.SNYields.Count > 0)
                {
                    var piedatadict = new Dictionary<string, int>();
                    var eklist = new List<string>();
                    foreach (var error in yieldvm.SNErrorMap.Keys)
                    {
                        eklist.Add(error);
                    }

                    foreach (var error in eklist)
                    {
                        if (string.Compare(error, "PASS", true) != 0)
                        {
                            foreach (var test in yieldvm.SNYields)
                            {
                                var val = ProjectYieldViewModule.RetrieveErrorCount(error, test.WhichTest, yieldvm.SNErrorMap);

                                if (piedatadict.ContainsKey(error))
                                {
                                    var preval = piedatadict[error];
                                    piedatadict[error] = preval + val;
                                }
                                else
                                {
                                    piedatadict.Add(error, val);
                                }
                            }
                        }
                    }

                    retestdatalist = piedatadict.ToList();

                    piedatadict["PASS"] = ProjectYieldViewModule.RetrieveErrorCount("PASS", yieldvm.SNYields[yieldvm.SNYields.Count - 1].WhichTest, yieldvm.SNErrorMap);

                    var keys = piedatadict.Keys;
                    var namevaluepair = "";
                    foreach (var k in keys)
                    {
                        if (piedatadict[k] > 0)
                            namevaluepair = namevaluepair + "{ name:'" + k + "',y:" + piedatadict[k].ToString() + "},";
                    }

                    namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                    var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart.xml"));
                    ViewBag.rchartscript = tempscript.Replace("#ElementID#", "rfailurepie")
                        .Replace("#Title#", "SN Trace Failure")
                        .Replace("#SERIESNAME#", "SNFailure")
                        .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                }

                if (yieldvm.LastYields.Count > 0)
                {
                    var piedatadict = new Dictionary<string, int>();
                    var eklist = new List<string>();
                    foreach (var error in yieldvm.LErrorMap.Keys)
                    {
                        eklist.Add(error);
                    }

                    foreach (var error in eklist)
                    {
                        if (string.Compare(error, "PASS", true) != 0)
                        {
                            foreach (var test in yieldvm.LastYields)
                            {
                                var val = ProjectYieldViewModule.RetrieveErrorCount(error, test.WhichTest, yieldvm.LErrorMap);

                                if (piedatadict.ContainsKey(error))
                                {
                                    var preval = piedatadict[error];
                                    piedatadict[error] = preval + val;
                                }
                                else
                                {
                                    piedatadict.Add(error, val);
                                }
                            }
                        }
                    }

                    fytestdatalist = piedatadict.ToList();
                    piedatadict["PASS"] = ProjectYieldViewModule.RetrieveErrorCount("PASS", yieldvm.LastYields[yieldvm.LastYields.Count - 1].WhichTest, yieldvm.LErrorMap);

                    var keys = piedatadict.Keys;
                    var namevaluepair = "";
                    foreach (var k in keys)
                    {
                        if (piedatadict[k] > 0)
                            namevaluepair = namevaluepair + "{ name:'" + k + "',y:" + piedatadict[k].ToString() + "},";
                    }

                    namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                    var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart.xml"));
                    ViewBag.fychartscript = tempscript.Replace("#ElementID#", "fyfailurepie")
                        .Replace("#Title#", "Final Failure")
                        .Replace("#SERIESNAME#", "RFailure")
                        .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                }

                if (firstdatalist.Count > 0)
                {
                    firsttestparetofun(firstdatalist, ProjectKey, sdate.ToString(), edate.ToString());
                }

                if (retestdatalist.Count > 0)
                {
                    retestparetofun(retestdatalist, ProjectKey, sdate.ToString(), edate.ToString());
                }

                if (fytestdatalist.Count > 0)
                {
                    fytparetofun(fytestdatalist, ProjectKey, sdate.ToString(), edate.ToString());
                }

                return View(yieldvm);
            }
            return View();
        }

        private void ProjectBRTypeYield(string ProjectKey, string BRNUM, string BRType)
        {
            var pvm = ProjectViewModels.RetrieveOneProject(ProjectKey);
            var vmlist = ProjectYieldViewModule.GetYieldByBRNum(ProjectKey, BRNUM, pvm, HttpContext.Cache, BRType);

            if (vmlist.Count > 0)
            {
                var ChartxAxisValues = "";
                //var ChartSearies = "";

                var ftimelist = new List<string>();
                var famountlist = new List<int>();
                var fyieldlist = new List<double>();
                var ryieldlist = new List<double>();

                var maxamout = 0;

                foreach (var item in vmlist)
                {
                    ftimelist.Add(item.XKey);

                    fyieldlist.Add(item.FirstYield * 100.0);
                    ryieldlist.Add(item.LastYield * 100.0);

                    var tempfamount = 0;
                    foreach (var d in item.FirstYields)
                    {
                        if (d.InputCount > tempfamount) { tempfamount = d.InputCount; }
                        if (d.InputCount > maxamout) { maxamout = d.InputCount; }
                    }
                    famountlist.Add(tempfamount);
                }

                //xaxis
                foreach (var item in ftimelist)
                {
                    ChartxAxisValues = ChartxAxisValues + "'" + item + "',";
                }
                ChartxAxisValues = ChartxAxisValues.Substring(0, ChartxAxisValues.Length - 1);


                //yaxis
                //ChartSearies = "{name:'First Yield',data:[<fvalue>]},{name:'Retest Yield',data:[<cvalue>]}";

                var famout = "";
                foreach (var item in famountlist)
                {
                    famout = famout + item.ToString() + ",";
                }
                famout = famout.Substring(0, famout.Length - 1);

                var ftempvalue = "";
                foreach (var item in fyieldlist)
                {
                    ftempvalue = ftempvalue + item.ToString("0.00") + ",";
                }
                ftempvalue = ftempvalue.Substring(0, ftempvalue.Length - 1);


                var rtempvalue = "";
                foreach (var item in ryieldlist)
                {
                    rtempvalue = rtempvalue + item.ToString("0.00") + ",";
                }
                rtempvalue = rtempvalue.Substring(0, rtempvalue.Length - 1);
                //ChartSearies = ChartSearies.Replace("<cvalue>", tempvalue);

                var FINALTOOLTIP = "";

                for (var idx = 0; idx < fyieldlist.Count; idx++)
                {
                    FINALTOOLTIP = FINALTOOLTIP + "'<!doctype html><table>"
                        + "<tr><td><b>FPY</b></td><td>" + fyieldlist[idx].ToString("0.00") + "&#37;</td></tr>"
                        + "<tr><td><b>FY</b></td><td>" + ryieldlist[idx].ToString("0.00") + "&#37;</td></tr>";

                    foreach (var d in vmlist[idx].LastYields)
                    {
                        FINALTOOLTIP = FINALTOOLTIP + "<tr><td><b>" + d.WhichTest + "</b></td><td>Input:</td><td>" + d.InputCount.ToString() + "</td><td>Output:</td><td>" + d.OutputCount.ToString() + "</td></tr>";
                    }

                    FINALTOOLTIP = FINALTOOLTIP + "</table>'";
                    FINALTOOLTIP = FINALTOOLTIP + ",";
                }
                FINALTOOLTIP = FINALTOOLTIP.Substring(0, FINALTOOLTIP.Length - 1);

                //rederect url
                var tempurl = "/Project/ProjectBRYieldDetail?ProjectKey=" + ProjectKey + "'+'&CurrentBR='+this.category+'" + "&WholeBRNUM=" + BRNUM + "&BRType=" + BRType;
                var reurl = "window.location.href = '" + tempurl + "'";

                var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/BaseYield.xml"));
                ViewBag.chartscript = tempscript.Replace("#ElementID#", "bryield")
                    .Replace("#Title#", BRNUM + " " + BRType + " Yield")
                    .Replace("#ChartxAxisValues#", ChartxAxisValues)
                    .Replace("#XAxisTitle#", BRType)
                    .Replace("#AmountMAX#", maxamout.ToString())
                    .Replace("#FirstAmount#", famout)
                    .Replace("#FirstYield#", ftempvalue)
                    .Replace("#RetestYield#", rtempvalue)
                    .Replace("#FINALTOOLTIP#", FINALTOOLTIP)
                    .Replace("#REDIRECTURL#", reurl);
            }

        }



        public ActionResult ProjectBRYield(string ProjectKey, string BRNUM)
        {
            if (!string.IsNullOrEmpty(ProjectKey) && !string.IsNullOrEmpty(BRNUM))
            {
                ViewBag.pjkey = ProjectKey;
                ProjectBRTypeYield(ProjectKey, BRNUM, YIELDTYPE.BR);
                return View();
            }

            return View();
        }



        public ActionResult ProjectJOYield(string ProjectKey, string BRNUM)
        {
            if (!string.IsNullOrEmpty(ProjectKey) && !string.IsNullOrEmpty(BRNUM))
            {
                ViewBag.pjkey = ProjectKey;
                ProjectBRTypeYield(ProjectKey, BRNUM, YIELDTYPE.JO);
                return View();
            }

            return View();
        }
        public ActionResult ProjectBRYieldDetail(string ProjectKey, string CurrentBR, string WholeBRNUM, string BRType)
        {
            if (!string.IsNullOrEmpty(ProjectKey)
                && !string.IsNullOrEmpty(CurrentBR)
                && !string.IsNullOrEmpty(WholeBRNUM)
                && !string.IsNullOrEmpty(BRType))
            {
                ViewBag.BRNUM = WholeBRNUM;
                ViewBag.BRType = BRType;

                var pvm = ProjectViewModels.RetrieveOneProject(ProjectKey);
                var vmlist = ProjectYieldViewModule.GetYieldByBRNum(ProjectKey, CurrentBR, pvm, HttpContext.Cache, BRType);
                if (vmlist.Count > 0)
                {
                    var firstdatalist = new List<KeyValuePair<string, int>>();
                    var retestdatalist = new List<KeyValuePair<string, int>>();
                    var fytestdatalist = new List<KeyValuePair<string, int>>();

                    var yieldvm = vmlist[0];

                    if (yieldvm.FirstYields.Count > 0)
                    {
                        var piedatadict = new Dictionary<string, int>();
                        var eklist = new List<string>();
                        foreach (var error in yieldvm.FErrorMap.Keys)
                        {
                            eklist.Add(error);
                        }

                        foreach (var error in eklist)
                        {
                            if (string.Compare(error, "PASS", true) != 0)
                            {
                                foreach (var test in yieldvm.FirstYields)
                                {
                                    var val = ProjectYieldViewModule.RetrieveErrorCount(error, test.WhichTest, yieldvm.FErrorMap);

                                    if (piedatadict.ContainsKey(error))
                                    {
                                        var preval = piedatadict[error];
                                        piedatadict[error] = preval + val;
                                    }
                                    else
                                    {
                                        piedatadict.Add(error, val);
                                    }
                                }
                            }
                        }

                        firstdatalist = piedatadict.ToList();

                        piedatadict["PASS"] = ProjectYieldViewModule.RetrieveErrorCount("PASS", yieldvm.FirstYields[yieldvm.FirstYields.Count - 1].WhichTest, yieldvm.FErrorMap);

                        var keys = piedatadict.Keys;
                        var namevaluepair = "";
                        foreach (var k in keys)
                        {
                            if (piedatadict[k] > 0)
                                namevaluepair = namevaluepair + "{ name:'" + k + "',y:" + piedatadict[k].ToString() + "},";
                        }

                        namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                        var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart.xml"));
                        ViewBag.fchartscript = tempscript.Replace("#ElementID#", "ffailurepie")
                            .Replace("#Title#", "First Failure")
                            .Replace("#SERIESNAME#", "FFailure")
                            .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                    }


                    //if (yieldvm.LastYields.Count > 0)
                    //{
                    //    var piedatadict = new Dictionary<string, int>();
                    //    var eklist = new List<string>();
                    //    foreach (var error in yieldvm.LErrorMap.Keys)
                    //    {
                    //        eklist.Add(error);
                    //    }

                    //    foreach (var error in eklist)
                    //    {
                    //        if (string.Compare(error, "PASS", true) != 0)
                    //        {
                    //            foreach (var test in yieldvm.LastYields)
                    //            {
                    //                var val = ProjectYieldViewModule.RetrieveErrorCount(error, test.WhichTest, yieldvm.LErrorMap);

                    //                if (piedatadict.ContainsKey(error))
                    //                {
                    //                    var preval = piedatadict[error];
                    //                    piedatadict[error] = preval + val;
                    //                }
                    //                else
                    //                {
                    //                    piedatadict.Add(error, val);
                    //                }
                    //            }
                    //        }
                    //    }

                    //    retestdatalist = piedatadict.ToList();

                    //    piedatadict["PASS"] = ProjectYieldViewModule.RetrieveErrorCount("PASS", yieldvm.LastYields[yieldvm.LastYields.Count - 1].WhichTest, yieldvm.LErrorMap);

                    //    var keys = piedatadict.Keys;
                    //    var namevaluepair = "";
                    //    foreach (var k in keys)
                    //    {
                    //        if (piedatadict[k] > 0)
                    //            namevaluepair = namevaluepair + "{ name:'" + k + "',y:" + piedatadict[k].ToString() + "},";
                    //    }

                    //    namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                    //    var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart.xml"));
                    //    ViewBag.rchartscript = tempscript.Replace("#ElementID#", "rfailurepie")
                    //        .Replace("#Title#", "Final Failure")
                    //        .Replace("#SERIESNAME#", "RFailure")
                    //        .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                    //}

                    if (yieldvm.SNYields.Count > 0)
                    {
                        var piedatadict = new Dictionary<string, int>();
                        var eklist = new List<string>();
                        foreach (var error in yieldvm.SNErrorMap.Keys)
                        {
                            eklist.Add(error);
                        }

                        foreach (var error in eklist)
                        {
                            if (string.Compare(error, "PASS", true) != 0)
                            {
                                foreach (var test in yieldvm.SNYields)
                                {
                                    var val = ProjectYieldViewModule.RetrieveErrorCount(error, test.WhichTest, yieldvm.SNErrorMap);

                                    if (piedatadict.ContainsKey(error))
                                    {
                                        var preval = piedatadict[error];
                                        piedatadict[error] = preval + val;
                                    }
                                    else
                                    {
                                        piedatadict.Add(error, val);
                                    }
                                }
                            }
                        }

                        retestdatalist = piedatadict.ToList();

                        piedatadict["PASS"] = ProjectYieldViewModule.RetrieveErrorCount("PASS", yieldvm.SNYields[yieldvm.SNYields.Count - 1].WhichTest, yieldvm.SNErrorMap);

                        var keys = piedatadict.Keys;
                        var namevaluepair = "";
                        foreach (var k in keys)
                        {
                            if (piedatadict[k] > 0)
                                namevaluepair = namevaluepair + "{ name:'" + k + "',y:" + piedatadict[k].ToString() + "},";
                        }

                        namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                        var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart.xml"));
                        ViewBag.rchartscript = tempscript.Replace("#ElementID#", "rfailurepie")
                            .Replace("#Title#", "SN Trace Failure")
                            .Replace("#SERIESNAME#", "SNFailure")
                            .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                    }

                    if (yieldvm.LastYields.Count > 0)
                    {
                        var piedatadict = new Dictionary<string, int>();
                        var eklist = new List<string>();
                        foreach (var error in yieldvm.LErrorMap.Keys)
                        {
                            eklist.Add(error);
                        }

                        foreach (var error in eklist)
                        {
                            if (string.Compare(error, "PASS", true) != 0)
                            {
                                foreach (var test in yieldvm.LastYields)
                                {
                                    var val = ProjectYieldViewModule.RetrieveErrorCount(error, test.WhichTest, yieldvm.LErrorMap);

                                    if (piedatadict.ContainsKey(error))
                                    {
                                        var preval = piedatadict[error];
                                        piedatadict[error] = preval + val;
                                    }
                                    else
                                    {
                                        piedatadict.Add(error, val);
                                    }
                                }
                            }
                        }

                        fytestdatalist = piedatadict.ToList();

                        piedatadict["PASS"] = ProjectYieldViewModule.RetrieveErrorCount("PASS", yieldvm.LastYields[yieldvm.LastYields.Count - 1].WhichTest, yieldvm.LErrorMap);

                        var keys = piedatadict.Keys;
                        var namevaluepair = "";
                        foreach (var k in keys)
                        {
                            if (piedatadict[k] > 0)
                                namevaluepair = namevaluepair + "{ name:'" + k + "',y:" + piedatadict[k].ToString() + "},";
                        }

                        namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                        var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart.xml"));
                        ViewBag.fychartscript = tempscript.Replace("#ElementID#", "fyfailurepie")
                            .Replace("#Title#", "Final Failure")
                            .Replace("#SERIESNAME#", "RFailure")
                            .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                    }

                    if (firstdatalist.Count > 0)
                    {
                        firsttestparetofun(firstdatalist, ProjectKey,null,null);
                    }

                    if (retestdatalist.Count > 0)
                    {
                        retestparetofun(retestdatalist, ProjectKey, null, null);
                    }

                    if (fytestdatalist.Count > 0)
                    {
                        fytparetofun(fytestdatalist, ProjectKey, null, null);
                    }

                    return View(yieldvm);

                }

            }
            return View();
        }

        public ActionResult ProjectPeriodYield(string ProjectKey, string StartDate, string EndDate)
        {
            if (!string.IsNullOrEmpty(ProjectKey) && !string.IsNullOrEmpty(StartDate) && !string.IsNullOrEmpty(EndDate))
            {
                ViewBag.pjkey = ProjectKey;

                var pvm = ProjectViewModels.RetrieveOneProject(ProjectKey);

                var vmlist = new List<ProjectYieldViewModule>();
                var tempret = ProjectYieldViewModule.GetYieldByDateRange(ProjectKey, StartDate, EndDate, pvm, HttpContext.Cache);
                if (tempret.RealTimeYields.Count > 0)
                {
                    vmlist.Add(tempret);
                }

                if (vmlist.Count > 0)
                {
                    var ChartxAxisValues = "";
                    //var ChartSearies = "";

                    var ftimelist = new List<string>();
                    var famountlist = new List<int>();
                    var fyieldlist = new List<double>();
                    var ryieldlist = new List<double>();
                    var rtyieldlist = new List<double>();
                    var snyieldlist = new List<double>();
                    var maxamout = 0;

                    foreach (var item in vmlist)
                    {
                        ftimelist.Add(item.EndDate.ToString("yyyy-MM-dd"));
                        fyieldlist.Add(item.FirstYield * 100.0);
                        ryieldlist.Add(item.LastYield * 100.0);
                        rtyieldlist.Add(item.RealTimeYield * 100.0);
                        snyieldlist.Add(item.SNYield * 100);

                        var tempfamount = 0;
                        foreach (var d in item.FirstYields)
                        {
                            if (d.InputCount > tempfamount) { tempfamount = d.InputCount; }
                            if (d.InputCount > maxamout) { maxamout = d.InputCount; }
                        }
                        famountlist.Add(tempfamount);
                    }

                    //xaxis
                    foreach (var item in ftimelist)
                    {
                        ChartxAxisValues = ChartxAxisValues + "'" + item + "',";
                    }
                    ChartxAxisValues = ChartxAxisValues.Substring(0, ChartxAxisValues.Length - 1);


                    //yaxis
                    //ChartSearies = "{name:'First Yield',data:[<fvalue>]},{name:'Retest Yield',data:[<cvalue>]}";

                    var famout = "";
                    foreach (var item in famountlist)
                    {
                        famout = famout + item.ToString() + ",";
                    }
                    famout = famout.Substring(0, famout.Length - 1);

                    var ftempvalue = "";
                    foreach (var item in fyieldlist)
                    {
                        ftempvalue = ftempvalue + item.ToString("0.00") + ",";
                    }
                    ftempvalue = ftempvalue.Substring(0, ftempvalue.Length - 1);
                    //ChartSearies = ChartSearies.Replace("<fvalue>", tempvalue);

                    var rttempvalue = "";
                    foreach (var item in rtyieldlist)
                    {
                        rttempvalue = rttempvalue + item.ToString("0.00") + ",";
                    }
                    rttempvalue = rttempvalue.Substring(0, rttempvalue.Length - 1);

                    var sntempvalue = "";
                    foreach (var item in snyieldlist)
                    {
                        sntempvalue = sntempvalue + item.ToString("0.00") + ",";
                    }
                    sntempvalue = sntempvalue.Substring(0, sntempvalue.Length - 1);

                    var rtempvalue = "";
                    foreach (var item in ryieldlist)
                    {
                        rtempvalue = rtempvalue + item.ToString("0.00") + ",";
                    }
                    rtempvalue = rtempvalue.Substring(0, rtempvalue.Length - 1);
                    //ChartSearies = ChartSearies.Replace("<cvalue>", tempvalue);

                    var FINALTOOLTIP = "";
                    var REALTIMETOOLTIP = "";

                    for (var idx = 0; idx < rtyieldlist.Count; idx++)
                    {
                        FINALTOOLTIP = FINALTOOLTIP + "'<!doctype html><table>"
                            + "<tr><td><b>FPY</b></td><td>" + fyieldlist[idx].ToString("0.00") + "&#37;</td></tr>"
                            + "<tr><td><b>FY</b></td><td>" + ryieldlist[idx].ToString("0.00") + "&#37;</td></tr>";

                        foreach (var d in vmlist[idx].LastYields)
                        {
                            FINALTOOLTIP = FINALTOOLTIP + "<tr><td><b>" + d.WhichTest + "</b></td><td>Input:</td><td>" + d.InputCount.ToString() + "</td><td>Output:</td><td>" + d.OutputCount.ToString() + "</td></tr>";
                        }

                        FINALTOOLTIP = FINALTOOLTIP + "</table>'";
                        FINALTOOLTIP = FINALTOOLTIP + ",";
                    }
                    FINALTOOLTIP = FINALTOOLTIP.Substring(0, FINALTOOLTIP.Length - 1);


                    for (var idx = 0; idx < rtyieldlist.Count; idx++)
                    {
                        REALTIMETOOLTIP = REALTIMETOOLTIP + "'<!doctype html><table>"
                            + "<tr><td><b>Realtime Yield</b></td><td>" + rtyieldlist[idx].ToString("0.00") + "&#37;</td></tr>";
                        foreach (var d in vmlist[idx].RealTimeYields)
                        {
                            REALTIMETOOLTIP = REALTIMETOOLTIP + "<tr><td><b>" + d.WhichTest + "</b></td><td>Input:</td><td>" + d.InputCount.ToString() + "</td><td>Output:</td><td>" + d.OutputCount.ToString() + "</td></tr>";
                        }
                        REALTIMETOOLTIP = REALTIMETOOLTIP + "</table>'";
                        REALTIMETOOLTIP = REALTIMETOOLTIP + ",";
                    }
                    REALTIMETOOLTIP = REALTIMETOOLTIP.Substring(0, REALTIMETOOLTIP.Length - 1);

                    var SNTOOLTIP = "";
                    for (var idx = 0; idx < snyieldlist.Count; idx++)
                    {
                        SNTOOLTIP = SNTOOLTIP + "'<!doctype html><table>"
                            + "<tr><td><b>SN Trace Yield</b></td><td>" + snyieldlist[idx].ToString("0.00") + "&#37;</td></tr>";
                        foreach (var d in vmlist[idx].SNYields)
                        {
                            SNTOOLTIP = SNTOOLTIP + "<tr><td><b>" + d.WhichTest + "</b></td><td>Input:</td><td>" + d.InputCount.ToString() + "</td><td>Output:</td><td>" + d.OutputCount.ToString() + "</td></tr>";
                        }
                        SNTOOLTIP = SNTOOLTIP + "</table>'";
                        SNTOOLTIP = SNTOOLTIP + ",";
                    }
                    SNTOOLTIP = SNTOOLTIP.Substring(0, SNTOOLTIP.Length - 1);

                    //rederect url
                    var tempurl = "/Project/ProjectPYieldDetail?ProjectKey=" + ProjectKey + "&StartDate=" + StartDate + "&EndDate=" + EndDate;
                    var reurl = "window.location.href = '" + tempurl + "'";

                    var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/SuperYield.xml"));
                    ViewBag.chartscript = tempscript.Replace("#ElementID#", "periodyield")
                        .Replace("#Title#", "Period Yield")
                        .Replace("#ChartxAxisValues#", ChartxAxisValues)
                        .Replace("#XAxisTitle#", "Date")
                        .Replace("#AmountMAX#", maxamout.ToString())
                        .Replace("#FirstAmount#", famout)
                        .Replace("#FirstYield#", ftempvalue)
                        .Replace("#RetestYield#", rtempvalue)
                        .Replace("#RealTimeYield#", rttempvalue)
                        .Replace("#FINALTOOLTIP#", FINALTOOLTIP)
                        .Replace("#REALTIMETOOLTIP#", REALTIMETOOLTIP)
                        .Replace("#SNYield#", sntempvalue)
                        .Replace("#SNTOOLTIP#", SNTOOLTIP)
                        .Replace("#REDIRECTURL#", reurl);
                }

                return View();
            }
            return View();
        }

        private void firsttestparetofun(List<KeyValuePair<string, int>> firstdatalist, string ProjectKey, string StartDate, string EndDate)
        {
            if (firstdatalist.Count > 0)
            {
                var peralist = new List<ParetoData>();

                if (firstdatalist.Count > 1)
                {
                    firstdatalist.Sort(delegate (KeyValuePair<string, int> pair1, KeyValuePair<string, int> pair2)
                    {
                        return pair2.Value.CompareTo(pair1.Value);
                    });
                }

                var sum = 0;
                for (var i = 0; i < firstdatalist.Count; i++)
                {
                    sum = sum + firstdatalist[i].Value;
                }

                var otherpercent = 0.0;

                for (var i = 0; i < firstdatalist.Count; i++)
                {
                    if (firstdatalist.Count > 5 && peralist.Count > 0 && peralist[peralist.Count - 1].sumpercent > 0.95)
                    {
                        otherpercent = otherpercent + firstdatalist[i].Value / (double)sum;
                        if (i == (firstdatalist.Count - 1))
                        {
                            var tempperato = new ParetoData();
                            tempperato.key = "Other";
                            tempperato.count = (int)(otherpercent * sum);
                            tempperato.percent = otherpercent;
                            tempperato.sumpercent = 1.0;
                            peralist.Add(tempperato);
                        }
                    }
                    else
                    {
                        var tempperato = new ParetoData();
                        tempperato.key = firstdatalist[i].Key;
                        if (i == 0)
                        {
                            tempperato.count = firstdatalist[i].Value;
                            tempperato.percent = tempperato.count / (double)sum;
                            tempperato.sumpercent = tempperato.percent;
                            peralist.Add(tempperato);
                        }
                        else
                        {
                            tempperato.count = firstdatalist[i].Value;
                            tempperato.percent = tempperato.count / (double)sum;
                            tempperato.sumpercent = peralist[peralist.Count - 1].sumpercent + tempperato.percent;
                            peralist.Add(tempperato);
                        }
                    }
                }

                //xaxis
                var ChartxAxisValues = "";

                foreach (var item in peralist)
                {
                    ChartxAxisValues = ChartxAxisValues + "'" + item.key + "',";
                }
                ChartxAxisValues = ChartxAxisValues.Substring(0, ChartxAxisValues.Length - 1);


                //yaxis
                //var ChartSearies = "{name:'Defect',data:[<fvalue>]}";

                var pcountvalue = "";
                foreach (var item in peralist)
                {
                    pcountvalue = pcountvalue + item.count.ToString() + ",";
                }
                pcountvalue = pcountvalue.Substring(0, pcountvalue.Length - 1);

                var ppecentvalue = "";
                foreach (var item in peralist)
                {
                    ppecentvalue = ppecentvalue + (item.sumpercent * 100).ToString("0.0") + ",";
                }
                ppecentvalue = ppecentvalue.Substring(0, ppecentvalue.Length - 1);

                var abpecentvalue = "";
                foreach (var item in peralist)
                {
                    abpecentvalue = abpecentvalue + (item.percent * 100).ToString("0.0") + ",";
                }
                abpecentvalue = abpecentvalue.Substring(0, abpecentvalue.Length - 1);

                //ChartSearies = ChartSearies.Replace("<fvalue>", tempvalue);

                var reurl = "window.location.href = '/Project/ProjectErrAbbr?ProjectKey=" + ProjectKey + "'" + "+'&ErrAbbr='+this.category";
                if (!string.IsNullOrEmpty(StartDate) && !string.IsNullOrEmpty(EndDate))
                {
                    reurl = reurl + "+'&StartDate='+'"+ StartDate + "'+'&EndDate='+'"+ EndDate + "'";
                }

                var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/ParetoChart.xml"));
                ViewBag.fparetoscript = tempscript.Replace("#ElementID#", "fparetochart")
                    .Replace("#Title#", "Pareto of First Test Defect")
                    .Replace("#XAxisTitle#", "Defect")
                    .Replace("#ChartxAxisValues#", ChartxAxisValues)
                    .Replace("#AmountMAX#", sum.ToString())
                    .Replace("#PCount#", pcountvalue)
                    .Replace("#ABPercent#", abpecentvalue)
                    .Replace("#PPercent#", ppecentvalue)
                    .Replace("#REDIRECTURL#", reurl);
            }
        }

        private void retestparetofun(List<KeyValuePair<string, int>> retestdatalist, string ProjectKey, string StartDate, string EndDate)
        {
            if (retestdatalist.Count > 0)
            {
                var peralist = new List<ParetoData>();

                if (retestdatalist.Count > 1)
                {
                    retestdatalist.Sort(delegate (KeyValuePair<string, int> pair1, KeyValuePair<string, int> pair2)
                    {
                        return pair2.Value.CompareTo(pair1.Value);
                    });
                }

                var sum = 0;
                for (var i = 0; i < retestdatalist.Count; i++)
                {
                    sum = sum + retestdatalist[i].Value;
                }

                var otherpercent = 0.0;

                for (var i = 0; i < retestdatalist.Count; i++)
                {
                    if (retestdatalist.Count > 5 && peralist.Count > 0 && peralist[peralist.Count - 1].sumpercent > 0.95)
                    {
                        otherpercent = otherpercent + retestdatalist[i].Value / (double)sum;
                        if (i == (retestdatalist.Count - 1))
                        {
                            var tempperato = new ParetoData();
                            tempperato.key = "Other";
                            tempperato.count = (int)(otherpercent * sum);
                            tempperato.percent = otherpercent;
                            tempperato.sumpercent = 1.0;
                            peralist.Add(tempperato);
                        }
                    }
                    else
                    {
                        var tempperato = new ParetoData();
                        tempperato.key = retestdatalist[i].Key;
                        if (i == 0)
                        {
                            tempperato.count = retestdatalist[i].Value;
                            tempperato.percent = tempperato.count / (double)sum;
                            tempperato.sumpercent = tempperato.percent;
                            peralist.Add(tempperato);
                        }
                        else
                        {
                            tempperato.count = retestdatalist[i].Value;
                            tempperato.percent = tempperato.count / (double)sum;
                            tempperato.sumpercent = peralist[peralist.Count - 1].sumpercent + tempperato.percent;
                            peralist.Add(tempperato);
                        }
                    }
                }

                //xaxis
                var ChartxAxisValues = "";

                foreach (var item in peralist)
                {
                    ChartxAxisValues = ChartxAxisValues + "'" + item.key + "',";
                }
                ChartxAxisValues = ChartxAxisValues.Substring(0, ChartxAxisValues.Length - 1);


                //yaxis
                //var ChartSearies = "{name:'Defect',data:[<fvalue>]}";

                var pcountvalue = "";
                foreach (var item in peralist)
                {
                    pcountvalue = pcountvalue + item.count.ToString() + ",";
                }
                pcountvalue = pcountvalue.Substring(0, pcountvalue.Length - 1);

                var ppecentvalue = "";
                foreach (var item in peralist)
                {
                    ppecentvalue = ppecentvalue + (item.sumpercent * 100).ToString("0.0") + ",";
                }
                ppecentvalue = ppecentvalue.Substring(0, ppecentvalue.Length - 1);

                var abpecentvalue = "";
                foreach (var item in peralist)
                {
                    abpecentvalue = abpecentvalue + (item.percent * 100).ToString("0.0") + ",";
                }
                abpecentvalue = abpecentvalue.Substring(0, abpecentvalue.Length - 1);

                //ChartSearies = ChartSearies.Replace("<fvalue>", tempvalue);

                var reurl = "window.location.href = '/Project/ProjectErrAbbr?ProjectKey=" + ProjectKey + "'" + "+'&ErrAbbr='+this.category";
                if (!string.IsNullOrEmpty(StartDate) && !string.IsNullOrEmpty(EndDate))
                {
                    reurl = reurl + "+'&StartDate='+'" + StartDate + "'+'&EndDate='+'" + EndDate + "'";
                }

                var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/ParetoChart.xml"));
                ViewBag.rparetoscript = tempscript.Replace("#ElementID#", "rparetochart")
                    .Replace("#Title#", "Pareto of SN Trace Defect")
                    .Replace("#XAxisTitle#", "Defect")
                    .Replace("#ChartxAxisValues#", ChartxAxisValues)
                    .Replace("#AmountMAX#", sum.ToString())
                    .Replace("#PCount#", pcountvalue)
                    .Replace("#ABPercent#", abpecentvalue)
                    .Replace("#PPercent#", ppecentvalue)
                    .Replace("#REDIRECTURL#", reurl);
            }
        }

        private void fytparetofun(List<KeyValuePair<string, int>> retestdatalist, string ProjectKey,string StartDate,string EndDate)
        {
            if (retestdatalist.Count > 0)
            {
                var peralist = new List<ParetoData>();

                if (retestdatalist.Count > 1)
                {
                    retestdatalist.Sort(delegate (KeyValuePair<string, int> pair1, KeyValuePair<string, int> pair2)
                    {
                        return pair2.Value.CompareTo(pair1.Value);
                    });
                }

                var sum = 0;
                for (var i = 0; i < retestdatalist.Count; i++)
                {
                    sum = sum + retestdatalist[i].Value;
                }

                var otherpercent = 0.0;

                for (var i = 0; i < retestdatalist.Count; i++)
                {
                    if (retestdatalist.Count > 5 && peralist.Count > 0 && peralist[peralist.Count - 1].sumpercent > 0.95)
                    {
                        otherpercent = otherpercent + retestdatalist[i].Value / (double)sum;
                        if (i == (retestdatalist.Count - 1))
                        {
                            var tempperato = new ParetoData();
                            tempperato.key = "Other";
                            tempperato.count = (int)(otherpercent * sum);
                            tempperato.percent = otherpercent;
                            tempperato.sumpercent = 1.0;
                            peralist.Add(tempperato);
                        }
                    }
                    else
                    {
                        var tempperato = new ParetoData();
                        tempperato.key = retestdatalist[i].Key;
                        if (i == 0)
                        {
                            tempperato.count = retestdatalist[i].Value;
                            tempperato.percent = tempperato.count / (double)sum;
                            tempperato.sumpercent = tempperato.percent;
                            peralist.Add(tempperato);
                        }
                        else
                        {
                            tempperato.count = retestdatalist[i].Value;
                            tempperato.percent = tempperato.count / (double)sum;
                            tempperato.sumpercent = peralist[peralist.Count - 1].sumpercent + tempperato.percent;
                            peralist.Add(tempperato);
                        }
                    }
                }

                //xaxis
                var ChartxAxisValues = "";

                foreach (var item in peralist)
                {
                    ChartxAxisValues = ChartxAxisValues + "'" + item.key + "',";
                }
                ChartxAxisValues = ChartxAxisValues.Substring(0, ChartxAxisValues.Length - 1);


                //yaxis
                //var ChartSearies = "{name:'Defect',data:[<fvalue>]}";

                var pcountvalue = "";
                foreach (var item in peralist)
                {
                    pcountvalue = pcountvalue + item.count.ToString() + ",";
                }
                pcountvalue = pcountvalue.Substring(0, pcountvalue.Length - 1);

                var ppecentvalue = "";
                foreach (var item in peralist)
                {
                    ppecentvalue = ppecentvalue + (item.sumpercent * 100).ToString("0.0") + ",";
                }
                ppecentvalue = ppecentvalue.Substring(0, ppecentvalue.Length - 1);

                var abpecentvalue = "";
                foreach (var item in peralist)
                {
                    abpecentvalue = abpecentvalue + (item.percent * 100).ToString("0.0") + ",";
                }
                abpecentvalue = abpecentvalue.Substring(0, abpecentvalue.Length - 1);

                //ChartSearies = ChartSearies.Replace("<fvalue>", tempvalue);

                var reurl = "window.location.href = '/Project/ProjectErrAbbr?ProjectKey=" + ProjectKey + "'" + "+'&ErrAbbr='+this.category";
                if (!string.IsNullOrEmpty(StartDate) && !string.IsNullOrEmpty(EndDate))
                {
                    reurl = reurl + "+'&StartDate='+'" + StartDate + "'+'&EndDate='+'" + EndDate + "'";
                }

                var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/ParetoChart.xml"));
                ViewBag.fyparetoscript = tempscript.Replace("#ElementID#", "fyparetochart")
                    .Replace("#Title#", "Pareto of Final Yield Defect")
                    .Replace("#XAxisTitle#", "Defect")
                    .Replace("#ChartxAxisValues#", ChartxAxisValues)
                    .Replace("#AmountMAX#", sum.ToString())
                    .Replace("#PCount#", pcountvalue)
                    .Replace("#ABPercent#", abpecentvalue)
                    .Replace("#PPercent#", ppecentvalue)
                    .Replace("#REDIRECTURL#", reurl);
            }
        }

        public ActionResult ProjectPYieldDetail(string ProjectKey, string StartDate, string EndDate)
        {
            if (!string.IsNullOrEmpty(ProjectKey) && !string.IsNullOrEmpty(StartDate) && !string.IsNullOrEmpty(EndDate))
            {
                ViewBag.StartDate = StartDate;
                ViewBag.EndDate = EndDate;

                var firstdatalist = new List<KeyValuePair<string, int>>();
                var retestdatalist = new List<KeyValuePair<string, int>>();
                var fytestdatalist = new List<KeyValuePair<string, int>>();

                var pvm = ProjectViewModels.RetrieveOneProject(ProjectKey);
                var yieldvm = ProjectYieldViewModule.GetYieldByDateRange(ProjectKey, StartDate, EndDate, pvm, HttpContext.Cache);

                if (yieldvm.FirstYields.Count > 0)
                {
                    var piedatadict = new Dictionary<string, int>();
                    var eklist = new List<string>();
                    foreach (var error in yieldvm.FErrorMap.Keys)
                    {
                        eklist.Add(error);
                    }

                    foreach (var error in eklist)
                    {
                        if (string.Compare(error, "PASS", true) != 0)
                        {
                            foreach (var test in yieldvm.FirstYields)
                            {
                                var val = ProjectYieldViewModule.RetrieveErrorCount(error, test.WhichTest, yieldvm.FErrorMap);

                                if (piedatadict.ContainsKey(error))
                                {
                                    var preval = piedatadict[error];
                                    piedatadict[error] = preval + val;
                                }
                                else
                                {
                                    piedatadict.Add(error, val);
                                }
                            }
                        }
                    }

                    firstdatalist = piedatadict.ToList();

                    piedatadict["PASS"] = ProjectYieldViewModule.RetrieveErrorCount("PASS", yieldvm.FirstYields[yieldvm.FirstYields.Count - 1].WhichTest, yieldvm.FErrorMap);

                    var keys = piedatadict.Keys;
                    var namevaluepair = "";
                    foreach (var k in keys)
                    {
                        if (piedatadict[k] > 0)
                            namevaluepair = namevaluepair + "{ name:'" + k + "',y:" + piedatadict[k].ToString() + "},";
                    }

                    namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                    var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart.xml"));
                    ViewBag.fchartscript = tempscript.Replace("#ElementID#", "ffailurepie")
                        .Replace("#Title#", "First Failure")
                        .Replace("#SERIESNAME#", "FFailure")
                        .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                }


                //if (yieldvm.LastYields.Count > 0)
                //{
                //    var piedatadict = new Dictionary<string, int>();
                //    var eklist = new List<string>();
                //    foreach (var error in yieldvm.LErrorMap.Keys)
                //    {
                //        eklist.Add(error);
                //    }

                //    foreach (var error in eklist)
                //    {
                //        if (string.Compare(error, "PASS", true) != 0)
                //        {
                //            foreach (var test in yieldvm.LastYields)
                //            {
                //                var val = ProjectYieldViewModule.RetrieveErrorCount(error, test.WhichTest, yieldvm.LErrorMap);

                //                if (piedatadict.ContainsKey(error))
                //                {
                //                    var preval = piedatadict[error];
                //                    piedatadict[error] = preval + val;
                //                }
                //                else
                //                {
                //                    piedatadict.Add(error, val);
                //                }
                //            }
                //        }
                //    }

                //    retestdatalist = piedatadict.ToList();

                //    piedatadict["PASS"] = ProjectYieldViewModule.RetrieveErrorCount("PASS", yieldvm.LastYields[yieldvm.LastYields.Count - 1].WhichTest, yieldvm.LErrorMap);

                //    var keys = piedatadict.Keys;
                //    var namevaluepair = "";
                //    foreach (var k in keys)
                //    {
                //        if (piedatadict[k] > 0)
                //            namevaluepair = namevaluepair + "{ name:'" + k + "',y:" + piedatadict[k].ToString() + "},";
                //    }

                //    namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                //    var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart.xml"));
                //    ViewBag.rchartscript = tempscript.Replace("#ElementID#", "rfailurepie")
                //        .Replace("#Title#", "Final Failure")
                //        .Replace("#SERIESNAME#", "RFailure")
                //        .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                //}

                if (yieldvm.SNYields.Count > 0)
                {
                    var piedatadict = new Dictionary<string, int>();
                    var eklist = new List<string>();
                    foreach (var error in yieldvm.SNErrorMap.Keys)
                    {
                        eklist.Add(error);
                    }

                    foreach (var error in eklist)
                    {
                        if (string.Compare(error, "PASS", true) != 0)
                        {
                            foreach (var test in yieldvm.SNYields)
                            {
                                var val = ProjectYieldViewModule.RetrieveErrorCount(error, test.WhichTest, yieldvm.SNErrorMap);

                                if (piedatadict.ContainsKey(error))
                                {
                                    var preval = piedatadict[error];
                                    piedatadict[error] = preval + val;
                                }
                                else
                                {
                                    piedatadict.Add(error, val);
                                }
                            }
                        }
                    }

                    retestdatalist = piedatadict.ToList();

                    piedatadict["PASS"] = ProjectYieldViewModule.RetrieveErrorCount("PASS", yieldvm.SNYields[yieldvm.SNYields.Count - 1].WhichTest, yieldvm.SNErrorMap);

                    var keys = piedatadict.Keys;
                    var namevaluepair = "";
                    foreach (var k in keys)
                    {
                        if (piedatadict[k] > 0)
                            namevaluepair = namevaluepair + "{ name:'" + k + "',y:" + piedatadict[k].ToString() + "},";
                    }

                    namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                    var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart.xml"));
                    ViewBag.rchartscript = tempscript.Replace("#ElementID#", "rfailurepie")
                        .Replace("#Title#", "SN Trace Failure")
                        .Replace("#SERIESNAME#", "SNFailure")
                        .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                }

                if (yieldvm.LastYields.Count > 0)
                {
                    var piedatadict = new Dictionary<string, int>();
                    var eklist = new List<string>();
                    foreach (var error in yieldvm.LErrorMap.Keys)
                    {
                        eklist.Add(error);
                    }

                    foreach (var error in eklist)
                    {
                        if (string.Compare(error, "PASS", true) != 0)
                        {
                            foreach (var test in yieldvm.LastYields)
                            {
                                var val = ProjectYieldViewModule.RetrieveErrorCount(error, test.WhichTest, yieldvm.LErrorMap);

                                if (piedatadict.ContainsKey(error))
                                {
                                    var preval = piedatadict[error];
                                    piedatadict[error] = preval + val;
                                }
                                else
                                {
                                    piedatadict.Add(error, val);
                                }
                            }
                        }
                    }

                    fytestdatalist = piedatadict.ToList();

                    piedatadict["PASS"] = ProjectYieldViewModule.RetrieveErrorCount("PASS", yieldvm.LastYields[yieldvm.LastYields.Count - 1].WhichTest, yieldvm.LErrorMap);

                    var keys = piedatadict.Keys;
                    var namevaluepair = "";
                    foreach (var k in keys)
                    {
                        if (piedatadict[k] > 0)
                            namevaluepair = namevaluepair + "{ name:'" + k + "',y:" + piedatadict[k].ToString() + "},";
                    }

                    namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                    var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart.xml"));
                    ViewBag.fychartscript = tempscript.Replace("#ElementID#", "fyfailurepie")
                        .Replace("#Title#", "Final Failure")
                        .Replace("#SERIESNAME#", "RFailure")
                        .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                }

                if (firstdatalist.Count > 0)
                {
                    firsttestparetofun(firstdatalist, ProjectKey,StartDate,EndDate);
                }

                if (retestdatalist.Count > 0)
                {
                    retestparetofun(retestdatalist, ProjectKey, StartDate, EndDate);
                }

                if (fytestdatalist.Count > 0)
                {
                    fytparetofun(fytestdatalist, ProjectKey, StartDate, EndDate);
                }

                return View(yieldvm);
            }
            return View();
        }

        public ActionResult UpdateProjectError(string ErrorKey)
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {

            }
            else
            {
                var ck = new Dictionary<string, string>();
                ck.Add("logonredirectctrl", "Project");
                ck.Add("logonredirectact", "UpdateProjectError");
                ck.Add("errorkey", ErrorKey);
                CookieUtility.SetCookie(this, ck);
                return RedirectToAction("LoginUser", "User");
            }

            var key = "";
            if (!string.IsNullOrEmpty(ErrorKey))
            {
                var ck = new Dictionary<string, string>();
                ck.Add("errorkey", ErrorKey);
                ck.Add("currentaction", "UpdateProjectError");
                CookieUtility.SetCookie(this, ck);
                key = ErrorKey;
            }
            else if (ckdict.ContainsKey("errorkey") && !string.IsNullOrEmpty(ckdict["errorkey"]))
            {
                key = ckdict["errorkey"];
                var ck = new Dictionary<string, string>();
                ck.Add("currentaction", "UpdateProjectError");
                CookieUtility.SetCookie(this, ck);
            }

            if (!string.IsNullOrEmpty(key))
            {
                var vm = ProjectErrorViewModels.RetrieveErrorByErrorKey(key, this);
                var AllPJMember = ProjectViewModels.RetrieveOneProject(vm[0].ProjectKey).AllPJMember;
                var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
                if (AllPJMember.ToUpper().Contains(updater.ToUpper()))
                {
                    ViewBag.assigee = true;
                }
                else
                {
                    ViewBag.assigee = false;
                }

                var asilist = UserViewModels.RetrieveAllUser();
                ViewBag.towholist = CreateSelectList(asilist, "");

                return View(vm[0]);
            }

            return View();
        }

        public ActionResult DeleteErrorComment(string ErrorKey, string CommentType, string Date)
        {
            if (!string.IsNullOrEmpty(ErrorKey) && !string.IsNullOrEmpty(CommentType) && !string.IsNullOrEmpty(Date))
            {
                ProjectErrorViewModels.DeleteErrorComment(ErrorKey, CommentType, Date);
                var dict = new RouteValueDictionary();
                dict.Add("ErrorKey", ErrorKey);
                return RedirectToAction("UpdateProjectError", "Project", dict);
            }
            return RedirectToAction("ViewAll", "Project");
        }

        public ActionResult DeleteAnalyse(string ErrorKey, string Date)
        {
            if (!string.IsNullOrEmpty(ErrorKey) && !string.IsNullOrEmpty(Date))
            {
                ProjectErrorViewModels.DeleteAnalyse(ErrorKey, Date);

                var dict = new RouteValueDictionary();
                dict.Add("ErrorKey", ErrorKey);
                return RedirectToAction("UpdateProjectError", "Project", dict);
            }
            return RedirectToAction("ViewAll", "Project");
        }


        public ActionResult UpdateProjectError2(string ProjectKey, string ErrorCode)
        {
            var tempvm = ProjectErrorViewModels.RetrieveErrorByPJKey(ProjectKey, ErrorCode, this);
            var ErrorKey = "";
            if (tempvm.Count > 0)
            {
                ErrorKey = tempvm[0].ErrorKey;
            }

            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {

            }
            else
            {
                var ck = new Dictionary<string, string>();
                ck.Add("logonredirectctrl", "Project");
                ck.Add("logonredirectact", "UpdateProjectError");
                ck.Add("errorkey", ErrorKey);
                CookieUtility.SetCookie(this, ck);
                return RedirectToAction("LoginUser", "User");
            }

            var key = "";
            if (!string.IsNullOrEmpty(ErrorKey))
            {
                var ck = new Dictionary<string, string>();
                ck.Add("errorkey", ErrorKey);
                ck.Add("currentaction", "UpdateProjectError");
                CookieUtility.SetCookie(this, ck);
                key = ErrorKey;
            }
            else if (ckdict.ContainsKey("errorkey") && !string.IsNullOrEmpty(ckdict["errorkey"]))
            {
                key = ckdict["errorkey"];
                var ck = new Dictionary<string, string>();
                ck.Add("currentaction", "UpdateProjectError");
                CookieUtility.SetCookie(this, ck);
            }

            if (!string.IsNullOrEmpty(key))
            {
                var vm = ProjectErrorViewModels.RetrieveErrorByErrorKey(key, this);
                var AllPJMember = ProjectViewModels.RetrieveOneProject(vm[0].ProjectKey).AllPJMember;
                var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
                if (AllPJMember.ToUpper().Contains(updater.ToUpper()))
                {
                    ViewBag.assigee = true;
                }
                else
                {
                    ViewBag.assigee = false;
                }

                var asilist1 = UserViewModels.RetrieveAllUser();
                ViewBag.towholist = CreateSelectList(asilist1, "");
                return View("UpdateProjectError", vm[0]);
            }

            var asilist = UserViewModels.RetrieveAllUser();
            ViewBag.towholist = CreateSelectList(asilist, "");
            return View("UpdateProjectError");
        }


        private List<string> ReceiveAttachFiles()
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

        private void SendDBGCommentEvent(string what, string urlstr, List<string> towho, string pusher, string commentcontent)
        {
            try
            {
                var routevalue = new RouteValueDictionary();
                routevalue.Add("issuekey", "ABC");
                string scheme = Url.RequestContext.HttpContext.Request.Url.Scheme;
                string validatestr = Url.Action("UpdateIssue", "Issue", routevalue, scheme);
                var netcomputername = "";
                try { netcomputername = System.Net.Dns.GetHostName(); }
                catch (Exception ex) { }
                validatestr = validatestr.Replace("//localhost", "//" + netcomputername);


                validatestr = validatestr.Split(new string[] { "/Issue" }, StringSplitOptions.RemoveEmptyEntries)[0] + urlstr;
                var content = what + " is added to your debug tree by " + pusher + ":\r\n\r\n" + validatestr + "\r\n\r\n" + commentcontent;

                var toaddrs = new List<string>();
                toaddrs.AddRange(towho);

                var reporter = pusher.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries)[0].Replace(".", " ");
                EmailUtility.SendEmail(this, "WUXI Engineering System_" + reporter, toaddrs, content);
                new System.Threading.ManualResetEvent(false).WaitOne(20);
            }
            catch (Exception ex)
            { }
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

        [HttpPost, ActionName("UpdateProjectError")]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProjectErrorPost()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

            var currenttime = DateTime.Now.ToString();

            var vm = new ProjectErrorViewModels();
            vm.ErrorKey = Request.Form["ErrorKey"];
            vm.ShortDesc = Request.Form["ShortDesc"];
            vm.Reporter = ckdict["logonuser"].Split(new char[] { '|' })[0];

            vm.UpdateShortDesc();


            var temphtml = Request.Form["editor1"];
            if (!string.IsNullOrEmpty(temphtml))
            {
                vm.Description = SeverHtmlDecode.Decode(this, temphtml);
                ProjectErrorViewModels.StoreErrorComment(vm.ErrorKey, vm.dbDescription, PJERRORCOMMENTTYPE.Description, vm.Reporter, currenttime);

                var updatevm = ProjectErrorViewModels.RetrieveErrorByErrorKey(vm.ErrorKey, this);
                var pjmems = ProjectViewModels.RetrieveOneProject(updatevm[0].ProjectKey).MemberList;
                var towho = new List<string>();
                foreach (var w in pjmems)
                {
                    towho.Add(w.Name);
                }
                var atlist = RetrieveUserFromComment(vm.Description);
                if (atlist.Count > 0) towho.AddRange(atlist);

                var commentcontent = System.Text.RegularExpressions.Regex.Replace(vm.Description.Replace("\"", "").Replace("&nbsp;", ""), "<.*?>", string.Empty).Trim();
                SendDBGCommentEvent("A mew comment", "/Project/UpdateProjectError?ErrorKey=" + vm.ErrorKey, towho, updater, commentcontent);
            }

            var urls = ReceiveAttachFiles();
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
                    ProjectErrorViewModels.StoreErrorAttachment(vm.ErrorKey, url);

                    var tempkeys = url.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                    var dockey = tempkeys[tempkeys.Length - 1];
                    UserKPIVM.AddUserAttachDailyRank(vm.ErrorKey, updater, UserRankType.ADDITIONAL
                        , "Add attachment to " + vm.ProjectKey + " " + vm.OrignalCode, "/Project/UpdateProjectError?ErrorKey=" + vm.ErrorKey, 2, dockey, this);
                }
            }

            if (!string.IsNullOrEmpty(Request.Form["analysetitle"]))
            {
                var com = new ErrorComments();
                com.Comment = Request.Form["analysetitle"];
                ProjectErrorViewModels.StoreErrorComment(vm.ErrorKey, com.dbComment, PJERRORCOMMENTTYPE.AnalyzeTitle, vm.Reporter, currenttime);
            }

            bool analyseinputed = false;
            var failurestr = string.Empty;
            var resulutstr = string.Empty;
            var rootcausestr = string.Empty;

            if (Request.Form["editor3"] != null)
            {
                var com = new ErrorComments();
                com.Comment = SeverHtmlDecode.Decode(this, Request.Form["editor3"]);
                failurestr = com.Comment;
                if (!string.IsNullOrEmpty(com.Comment))
                {
                    ProjectErrorViewModels.StoreErrorComment(vm.ErrorKey, com.dbComment, PJERRORCOMMENTTYPE.FailureDetail, vm.Reporter, currenttime);
                    analyseinputed = true;
                }
            }

            if (Request.Form["resulteditor"] != null)
            {
                var com = new ErrorComments();
                com.Comment = SeverHtmlDecode.Decode(this, Request.Form["resulteditor"]);
                resulutstr = com.Comment;
                if (!string.IsNullOrEmpty(com.Comment))
                {
                    ProjectErrorViewModels.StoreErrorComment(vm.ErrorKey, com.dbComment, PJERRORCOMMENTTYPE.Result, vm.Reporter, currenttime);
                    analyseinputed = true;
                }
            }

            if (Request.Form["editor2"] != null)
            {
                var com = new ErrorComments();
                com.Comment = SeverHtmlDecode.Decode(this, Request.Form["editor2"]);
                rootcausestr = com.Comment;
                if (!string.IsNullOrEmpty(com.Comment))
                {
                    ProjectErrorViewModels.StoreErrorComment(vm.ErrorKey, com.dbComment, PJERRORCOMMENTTYPE.RootCause, vm.Reporter, currenttime);
                    analyseinputed = true;
                }
            }

            if (analyseinputed)
            {
                if (string.IsNullOrEmpty(failurestr))
                {
                    var com1 = new ErrorComments();
                    com1.Comment = "<p>To Be Edit</p>";
                    ProjectErrorViewModels.StoreErrorComment(vm.ErrorKey, com1.dbComment, PJERRORCOMMENTTYPE.FailureDetail, vm.Reporter, currenttime);
                }

                if (string.IsNullOrEmpty(resulutstr))
                {
                    var com1 = new ErrorComments();
                    com1.Comment = "<p>To Be Edit</p>";
                    ProjectErrorViewModels.StoreErrorComment(vm.ErrorKey, com1.dbComment, PJERRORCOMMENTTYPE.Result, vm.Reporter, currenttime);
                }

                if (string.IsNullOrEmpty(rootcausestr))
                {
                    var com1 = new ErrorComments();
                    com1.Comment = "<p>To Be Edit</p>";
                    ProjectErrorViewModels.StoreErrorComment(vm.ErrorKey, com1.dbComment, PJERRORCOMMENTTYPE.RootCause, vm.Reporter, currenttime);
                }
                else
                {
                    UserKPIVM.AddUserDailyRank(vm.ErrorKey, updater, UserRankType.ADDITIONAL
                        , "Add analyse to " + vm.ProjectKey + " " + vm.OrignalCode, "/Project/UpdateProjectError?ErrorKey=" + vm.ErrorKey, 6);
                }
            }

            var dict = new RouteValueDictionary();
            dict.Add("ErrorKey", vm.ErrorKey);
            return RedirectToAction("UpdateProjectError", "Project", dict);
        }

        public ActionResult ErrorAttach(string ErrorKey)
        {
            if (!string.IsNullOrEmpty(ErrorKey))
            {
                var tempvm = ProjectErrorViewModels.RetrieveErrorByErrorKey(ErrorKey, this);
                return View(tempvm[0]);
            }
            return RedirectToAction("ViewAll", "Project");
        }

        public ActionResult DeleteErrorAttachment(string errorkey, string filename)
        {
            if (!string.IsNullOrEmpty(errorkey) && !string.IsNullOrEmpty(filename))
            {
                var tempvm = ProjectErrorViewModels.RetrieveErrorByErrorKey(errorkey, this);
                var AllPJMember = ProjectViewModels.RetrieveOneProject(tempvm[0].ProjectKey).AllPJMember;
                var ckdict = CookieUtility.UnpackCookie(this);
                var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

                if (AllPJMember.ToUpper().Contains(updater.ToUpper()))
                {
                    ProjectErrorViewModels.DeleteAttachment(errorkey, filename);
                }

                var dict = new RouteValueDictionary();
                dict.Add("ErrorKey", errorkey);
                return RedirectToAction("UpdateProjectError", "Project", dict);

            }
            else if (!string.IsNullOrEmpty(errorkey))
            {
                var dict = new RouteValueDictionary();
                dict.Add("ErrorKey", errorkey);
                return RedirectToAction("UpdateProjectError", "Project", dict);
            }
            else
            {
                return RedirectToAction("ViewAll", "Project");
            }
        }

        private Dictionary<string, double> VcselEmailCheck()
        {
            try
            {
                var filename = "VCSEL-EMAIL-CHECK";
                var wholefilename = Server.MapPath("~/userfiles") + "\\" + filename;
                if (System.IO.File.Exists(wholefilename))
                {
                    var ret = new Dictionary<string, double>();
                    var content = System.IO.File.ReadAllText(wholefilename);
                    if (content.Length > 3)
                    {
                        var lines = content.Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var item in lines)
                        {
                            var keyvalue = item.Split(new string[] { "<>" }, StringSplitOptions.RemoveEmptyEntries);
                            ret.Add(keyvalue[0].Trim(), Convert.ToDouble(keyvalue[1].Trim()));
                        }
                    }
                    return ret;
                }
                else
                {
                    return new Dictionary<string, double>();
                }
            }
            catch (Exception ex)
            { return new Dictionary<string, double>(); }
        }

        private void VcselEmailStore(Dictionary<string, double> emailed)
        {
            try
            {
                var filename = "VCSEL-EMAIL-CHECK";
                var wholefilename = Server.MapPath("~/userfiles") + "\\" + filename;
                var content = "";
                foreach (var item in emailed)
                {
                    content = content + item.Key + "<>" + item.Value.ToString("0.00000") + "\r\n";
                }
                System.IO.File.WriteAllText(wholefilename, content);
            }
            catch (Exception ex)
            { }

        }

        private void RealCheckVcselYieldByWafer(List<string> pjkeylist)
        {
            var emailed = VcselEmailCheck();
            //var content = "<!DOCTYPE html><table>";
            var content1 = string.Empty;
            var logcontent = string.Empty;
            bool hascontent = false;

            //var tempcontent2 = "<tr>" + "<th>Project</th>" + "<th>Wafer</th>" + "<th>Corrective Yield</th>" + "<th>Input</th>" + "<th>Output</th>" + "</tr>";
            //content = content + tempcontent2;

            var waferexceptlist = ProjectViewModels.RetrieveProjectAllExcept(ProjectExceptType.WAFERYIELDEXCEPT);
            var exceptdict = new Dictionary<string, bool>();

            foreach (var e in waferexceptlist)
            {
                var waferexcepts = e.Except.Split(new string[] { ";", ",", " " }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var w in waferexcepts)
                {
                    if (!exceptdict.ContainsKey(w))
                    {
                        exceptdict.Add(w, true);
                    }
                }
            }

            foreach (var item in pjkeylist)
            {
                var pjval = ProjectViewModels.RetrieveOneProject(item);
                if (string.Compare(pjval.MonitorVcsel, "False", true) == 0)
                {
                    continue;
                }
                var warningyield = Convert.ToDouble(pjval.VcselWarningYield) * 0.01;

                var waferlist = BITestData.RetrieveAllWafer(item);

                foreach (var w in waferlist)
                {
                    if (exceptdict.ContainsKey(w))
                    {
                        continue;
                    }


                    var yield = ProjectBIYieldViewModule.GetYieldByWafer(item, w);
                    if (yield.CorrectLastYield > 0.1 && yield.CorrectLastYield < warningyield)
                    {

                        var alldict = new Dictionary<string, bool>();
                        var errdict = new Dictionary<string, bool>();
                        var cordict = new Dictionary<string, bool>();

                        foreach (var yitem in yield.LastYields)
                        {
                            foreach (var snitem in yitem.AllSNDict)
                            {
                                if (!alldict.ContainsKey(snitem.Key))
                                {
                                    alldict.Add(snitem.Key, true);
                                }
                            }

                            foreach (var snitem in yitem.ErrSNDict)
                            {
                                if (!errdict.ContainsKey(snitem.Key))
                                {
                                    errdict.Add(snitem.Key, true);
                                }
                            }

                            foreach (var snitem in yitem.CorSNDict)
                            {
                                if (!cordict.ContainsKey(snitem.Key))
                                {
                                    cordict.Add(snitem.Key, true);
                                }
                            }
                        }

                        if (alldict.Count < 80)
                        {
                            continue;
                        }

                        hascontent = true;
                        var tempcontent1 = "Warning: the corrective yield of " + item + " wafer " + w + " is " + (yield.CorrectLastYield * 100.0).ToString("0.00") + "% \r\n\r\n"
                            + " Total Input: " + alldict.Count.ToString() + " ,Failed Modules: " + errdict.Count.ToString() + " ,Reviewed Modules: " + cordict.Count.ToString() + " \r\n\r\n";

                        if (errdict.Count == cordict.Count)
                        {
                            tempcontent1 = "[Reviewed ] " + tempcontent1;
                        }

                        if (!emailed.ContainsKey(item + "-" + w))
                        {
                            tempcontent1 = "[new ] " + tempcontent1;
                            emailed.Add(item + "-" + w, yield.CorrectLastYield);
                        }
                        else
                        {
                            var lasttimeyield = Convert.ToDouble(emailed[item + "-" + w].ToString("0.0"));
                            var currentyield = Convert.ToDouble(yield.CorrectLastYield.ToString("0.0"));
                            if (currentyield > lasttimeyield)
                            {
                                tempcontent1 = "[^] " + tempcontent1;
                            }
                            else if (currentyield < lasttimeyield)
                            {
                                tempcontent1 = "[v] " + tempcontent1;
                            }
                            emailed[item + "-" + w] = yield.CorrectLastYield;
                        }
                        //tempcontent2 = "<tr>"+ "<td>"+item+"</td>" + "<td>"+w+"</td>" + "<td>"+ (yield.CorrectLastYield * 100.0).ToString("0.00") + "%</td>" + "<td>"+ alldict.Count.ToString() + "</td>" + "<td>"+ ((int)(alldict.Count* yield.CorrectLastYield)).ToString() + "</td>" + "</tr>";
                        //content = content + tempcontent2;

                        content1 = content1 + tempcontent1;

                        logcontent = logcontent + tempcontent1;
                        logcontent = logcontent + "All SN:\r\n";
                        foreach (var sn in alldict)
                        {
                            logcontent = logcontent + sn.Key + "\r\n";
                        }
                        logcontent = logcontent + "Error SN:\r\n";
                        foreach (var sn in errdict)
                        {
                            logcontent = logcontent + sn.Key + "\r\n";
                        }

                    }
                }//end foreach
            }//end foreach


            VcselEmailStore(emailed);

            //content = content + "</table>";

            if (hascontent)
            {
                var fn = "VCSEL_WAFER_SN" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
                var logvcsel = Server.MapPath("~/userfiles") + "\\" + fn;
                System.IO.File.WriteAllText(logvcsel, logcontent);

                var netcomputername = "";
                try
                {
                    netcomputername = System.Net.Dns.GetHostName();
                }
                catch (Exception ex) { }


                var url = "/userfiles/" + fn;
                var routevalue = new RouteValueDictionary();
                routevalue.Add("issuekey", "ABC");
                string scheme = this.Url.RequestContext.HttpContext.Request.Url.Scheme;
                string validatestr = this.Url.Action("UpdateIssue", "Issue", routevalue, scheme);
                validatestr = validatestr.Split(new string[] { "/Issue" }, StringSplitOptions.None)[0] + url;
                validatestr = validatestr.Replace("//localhost", "//" + netcomputername);

                var toaddrs = new List<string>();
                var syscfgdict = CfgUtility.GetSysConfig(this);
                var bimembs = syscfgdict["BITEAM"].Split(new string[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var bm in bimembs)
                {
                    toaddrs.Add(bm.ToUpper());
                }

                EmailUtility.SendEmail(this, "VCSEL WAFER YIELD WARNING - WUXI Engineering System", toaddrs, content1 + "\r\nWafer SN File: " + validatestr);
                new System.Threading.ManualResetEvent(false).WaitOne(5000);
            }
        }

        private void CheckVecselYieldByWafer(List<string> pjkeylist)
        {
            var filename = "log" + DateTime.Now.ToString("yyyy-MM-dd");
            var wholefilename = Server.MapPath("~/userfiles") + "\\" + filename;

            if (!System.IO.File.Exists(wholefilename))
            {
                RealCheckVcselYieldByWafer(pjkeylist);
            }
        }

        private void PushShareDoc(Controller ctrl)
        {
            var filename = "log" + DateTime.Now.ToString("yyyy-MM-dd");
            var wholefilename = Server.MapPath("~/userfiles") + "\\" + filename;

            if (!System.IO.File.Exists(wholefilename))
            {
                ShareDocVM.MatchAllYesterdayDoc(ctrl);
            }
        }

        private string IssueURL(string IssueKey)
        {
            var routevalue = new RouteValueDictionary();
            routevalue.Add("issuekey", IssueKey);
            string scheme = this.Url.RequestContext.HttpContext.Request.Url.Scheme;
            string validatestr = this.Url.Action("UpdateIssue", "Issue", routevalue, scheme);
            var netcomputername = "";
            try { netcomputername = System.Net.Dns.GetHostName(); }
            catch (Exception ex) { }
            return validatestr.Replace("//localhost", "//" + netcomputername);
        }

        private string SendIAssignPendingIssue(string username)
        {
            var wissues = IssueViewModels.RetrieveIssueByCreator(username, Resolute.Working, this);
            var pissues = IssueViewModels.RetrieveIssueByCreator(username, Resolute.Pending, this);

            var ret = string.Empty;
            foreach (var i in wissues)
            {
                if (i.DueDate > DateTime.Now)
                    continue;

                ret = ret + "WORKING    Expire-Days: " + (DateTime.Now - i.DueDate).Days.ToString() + "    " + i.Summary + "   " + i.Assignee + " Due Date: " + i.DueDate.ToString() + ":\r\n";
                ret = ret + IssueURL(i.IssueKey) + "\r\n\r\n";
            }

            foreach (var i in pissues)
            {
                if (i.DueDate > DateTime.Now)
                    continue;

                ret = ret + "PENDING    Expire-Days: " + (DateTime.Now - i.DueDate).Days.ToString() + "    " + i.Summary + "   " + i.Assignee + " Due Date: " + i.DueDate.ToString() + ":\r\n";
                ret = ret + IssueURL(i.IssueKey) + "\r\n\r\n";
            }
            return ret;
        }

        private string SendIAssignDoneIssue(string username)
        {
            var dissues = IssueViewModels.RetrieveIssueByCreator(username, Resolute.Done, this);
            var ret = string.Empty;
            foreach (var i in dissues)
            {
                if (i.ResolvedDate > DateTime.Now.AddDays(-7))
                {
                    ret = ret + "Done    " + i.Summary + "   " + i.Assignee + ":\r\n";
                    ret = ret + IssueURL(i.IssueKey) + "\r\n\r\n";
                }
            }
            return ret;
        }

        private string SendAssign2MePendingIssue(string username)
        {

            var wissues = IssueViewModels.RetrieveIssueByAssigneeWorking(username, Resolute.Working, 60);
            var pissues = IssueViewModels.RetrieveIssueByAssignee(username, Resolute.Pending, 60);


            var ret = string.Empty;

            foreach (var i in wissues)
            {
                ret = ret + "WORKING    Pending-Days: " + (DateTime.Now - i.ReportDate).Days.ToString() + "    " + i.Summary + "   " + i.Reporter + " Due Date: " + i.DueDate.ToString() + ":\r\n";
                ret = ret + IssueURL(i.IssueKey) + "\r\n\r\n";
            }

            foreach (var i in pissues)
            {
                ret = ret + "PENDING    Pending-Days: " + (DateTime.Now - i.ReportDate).Days.ToString() + "    " + i.Summary + "   " + i.Reporter + " Due Date: " + i.DueDate.ToString() + ":\r\n";
                ret = ret + IssueURL(i.IssueKey) + "\r\n\r\n";
            }

            return ret;
        }

        private string SendAssign2MeDoneIssue(string username)
        {
            var dissues = IssueViewModels.RetrieveIssueByAssignee(username, Resolute.Done, 200);
            var ret = string.Empty;
            foreach (var i in dissues)
            {
                if (i.ResolvedDate > DateTime.Now.AddDays(-7))
                {
                    ret = ret + "Done    " + i.Summary + "   " + i.Reporter + ":\r\n";
                    ret = ret + IssueURL(i.IssueKey) + "\r\n\r\n";
                }
            }
            return ret;
        }

        private void SendTaskNotice()
        {
            var filename = "log" + DateTime.Now.ToString("yyyy-MM-dd");
            var wholefilename = Server.MapPath("~/userfiles") + "\\" + filename;
            if (!System.IO.File.Exists(wholefilename))
            {
                var hello = "hello";
                System.IO.File.WriteAllText(wholefilename, hello);
                var currenttime = DateTime.Now;

                var alluser = UserViewModels.RetrieveAllUser();
                foreach (var u in alluser)
                {
                    if (currenttime.DayOfWeek == DayOfWeek.Monday)
                    {
                        var Summary = string.Empty;
                        var assign2mepending = SendAssign2MePendingIssue(u);
                        if (!string.IsNullOrEmpty(assign2mepending))
                        {
                            Summary = Summary + "Assign To Me Pending:" + "\r\n\r\n";
                            Summary = Summary + assign2mepending;
                        }

                        var iassignpending = SendIAssignPendingIssue(u);
                        if (!string.IsNullOrEmpty(iassignpending))
                        {
                            Summary = Summary + "I Assign Pending:" + "\r\n\r\n";
                            Summary = Summary + iassignpending;
                        }

                        if (!string.IsNullOrEmpty(Summary))
                        {
                            var tolist = new List<string>();
                            tolist.Add(u);
                            EmailUtility.SendEmail(this, "Task Notice - WUXI Engineering System", tolist, Summary);
                        }
                    }//end if

                    if (currenttime.DayOfWeek == DayOfWeek.Thursday)
                    {
                        var Summary = string.Empty;
                        var assign2medone = SendAssign2MeDoneIssue(u);
                        if (!string.IsNullOrEmpty(assign2medone))
                        {
                            Summary = Summary + "Assign To Me Done:(in recent one week)" + "\r\n\r\n";
                            Summary = Summary + assign2medone;
                        }

                        var iassigndone = SendIAssignDoneIssue(u);
                        if (!string.IsNullOrEmpty(iassigndone))
                        {
                            Summary = Summary + "I Assign Done(in recent one week):" + "\r\n\r\n";
                            Summary = Summary + iassigndone;
                        }

                        var assign2mepending = SendAssign2MePendingIssue(u);
                        if (!string.IsNullOrEmpty(assign2mepending))
                        {
                            Summary = Summary + "Assign To Me Pending:" + "\r\n\r\n";
                            Summary = Summary + assign2mepending;
                        }

                        var iassignpending = SendIAssignPendingIssue(u);
                        if (!string.IsNullOrEmpty(iassignpending))
                        {
                            Summary = Summary + "I Assign Pending:" + "\r\n\r\n";
                            Summary = Summary + iassignpending;
                        }

                        if (!string.IsNullOrEmpty(Summary))
                        {
                            var tolist = new List<string>();
                            tolist.Add(u);
                            EmailUtility.SendEmail(this, "Task Notice - WUXI Engineering System", tolist, Summary);
                        }
                    }//end if
                }//end foreach
            }//end if
        }

        private void SendBookedReportNotice()
        {
            var filename = "log" + DateTime.Now.ToString("yyyy-MM-dd");
            var wholefilename = Server.MapPath("~/userfiles") + "\\" + filename;
            if (!System.IO.File.Exists(wholefilename))
            {
                var hello = "hello";
                System.IO.File.WriteAllText(wholefilename, hello);
                var bookrecords = PJReportViewModels.RetrieveBookReportRecord();
                foreach (var record in bookrecords)
                {
                    var wkday = ReportSelectTime.GetWeekDay(record.reporttime);
                    if (wkday == 8 || wkday == (int)DateTime.Now.DayOfWeek)
                    {
                        var routevalue = new RouteValueDictionary();
                        routevalue.Add("username", record.username);
                        //send validate email
                        string scheme = this.Url.RequestContext.HttpContext.Request.Url.Scheme;
                        string validatestr = this.Url.Action("UserBookedReport", "PJReport", routevalue, scheme);

                        var netcomputername = "";
                        try { netcomputername = System.Net.Dns.GetHostName(); }
                        catch (Exception ex) { }
                        validatestr = validatestr.Replace("//localhost", "//" + netcomputername);

                        var content = "Click below link to review the Report which you booked :\r\n " + validatestr;

                        var toaddrs = new List<string>();
                        toaddrs.Add(record.username);
                        EmailUtility.SendEmail(this, "WUXI Engineering System", toaddrs, content);
                        new System.Threading.ManualResetEvent(false).WaitOne(500);
                    }
                }//end foreach
            }//end if

        }

        private void SendRMAAlertEmail()
        {
            var pjlist = ProjectViewModels.RetrieveAllProjectKey();
            foreach (var pjkey in pjlist)
            {
                try
                {
                    var fvissues = IssueViewModels.Retrieve_Alert_RMAByProjectKey(pjkey, true);

                    foreach (var item in fvissues)
                    { item.UpdateAlertEmailDate(); }

                    foreach (var item in fvissues)
                    {
                        var routevalue = new RouteValueDictionary();
                        routevalue.Add("issuekey", item.IssueKey);
                        //send validate email
                        string scheme = this.Url.RequestContext.HttpContext.Request.Url.Scheme;
                        string validatestr = this.Url.Action("UpdateIssue", "Issue", routevalue, scheme);
                        var netcomputername = "";
                        try { netcomputername = System.Net.Dns.GetHostName(); }
                        catch (Exception ex) { }
                        validatestr = validatestr.Replace("//localhost", "//" + netcomputername);

                        var content = "RMA FA of " + item.ModuleSN + " must finished today :\r\n " + validatestr;

                        var toaddrs = new List<string>();
                        toaddrs.Add(item.Reporter);
                        toaddrs.Add(item.Assignee);
                        EmailUtility.SendEmail(this, "WUXI Engineering System", toaddrs, content);
                        new System.Threading.ManualResetEvent(false).WaitOne(300);
                    }
                }
                catch (Exception ex) { }


                try
                {
                    var faissues = IssueViewModels.Retrieve_Alert_RMAByProjectKey(pjkey, false);

                    foreach (var item in faissues)
                    { item.UpdateAlertEmailDate(); }

                    foreach (var item in faissues)
                    {
                        var routevalue = new RouteValueDictionary();
                        routevalue.Add("issuekey", item.IssueKey);
                        //send validate email
                        string scheme = this.Url.RequestContext.HttpContext.Request.Url.Scheme;
                        string validatestr = this.Url.Action("UpdateIssue", "Issue", routevalue, scheme);

                        var netcomputername = "";
                        try { netcomputername = System.Net.Dns.GetHostName(); }
                        catch (Exception ex) { }
                        validatestr = validatestr.Replace("//localhost", "//" + netcomputername);

                        var content = "RMA report of " + item.ModuleSN + " must finished today :\r\n " + validatestr;

                        var toaddrs = new List<string>();
                        toaddrs.Add(item.Reporter);
                        toaddrs.Add(item.Assignee);
                        EmailUtility.SendEmail(this, "WUXI Engineering System", toaddrs, content);
                        new System.Threading.ManualResetEvent(false).WaitOne(200);
                    }
                }
                catch (Exception ex) { }

            }//end foreach
        }

        private void SendOBAAlertEmail()
        {
            try
            {
                var faissues = IssueViewModels.Retrieve_Alert_OBAByProjectKey("PQE");

                foreach (var item in faissues)
                { item.UpdateAlertEmailDate(); }

                foreach (var item in faissues)
                {
                    var routevalue = new RouteValueDictionary();
                    routevalue.Add("issuekey", item.IssueKey);
                    //send validate email
                    string scheme = this.Url.RequestContext.HttpContext.Request.Url.Scheme;
                    string validatestr = this.Url.Action("UpdateIssue", "Issue", routevalue, scheme);

                    var netcomputername = "";
                    try { netcomputername = System.Net.Dns.GetHostName(); }
                    catch (Exception ex) { }
                    validatestr = validatestr.Replace("//localhost", "//" + netcomputername);

                    var content = "OBA " + item.FinisarDMR + " analyse  must finished tomorrow :\r\n " + validatestr;

                    var toaddrs = new List<string>();
                    toaddrs.Add(item.Reporter);
                    toaddrs.Add(item.Assignee);
                    EmailUtility.SendEmail(this, "WUXI Engineering System", toaddrs, content);
                    new System.Threading.ManualResetEvent(false).WaitOne(200);
                }
            }
            catch (Exception ex) { }
        }

        private void SendTaskAlertEmail()
        {
            try
            {
                var faissues = IssueViewModels.Retrieve_Alert_TaskByProjectKey();

                foreach (var item in faissues)
                { item.UpdateAlertEmailDate(); }

                foreach (var item in faissues)
                {
                    var routevalue = new RouteValueDictionary();
                    routevalue.Add("issuekey", item.IssueKey);
                    //send validate email
                    string scheme = this.Url.RequestContext.HttpContext.Request.Url.Scheme;
                    string validatestr = this.Url.Action("UpdateIssue", "Issue", routevalue, scheme);

                    var netcomputername = "";
                    try { netcomputername = System.Net.Dns.GetHostName(); }
                    catch (Exception ex) { }
                    validatestr = validatestr.Replace("//localhost", "//" + netcomputername);

                    var content = "Warning: Assigned to you task - " + item.Summary + " is close to its Due Date :\r\n " + validatestr;

                    var toaddrs = new List<string>();
                    //toaddrs.Add(item.Reporter);
                    toaddrs.Add(item.Assignee);
                    EmailUtility.SendEmail(this, "WUXI Engineering System", toaddrs, content);
                    new System.Threading.ManualResetEvent(false).WaitOne(200);
                }
            }
            catch (Exception ex) { }
        }


        public ActionResult HeartBeat()
        {
            var starttime = DateTime.Now.ToString();

            var pjkeylist = ProjectViewModels.RetrieveAllProjectKey();
            try
            {
                CheckVecselYieldByWafer(pjkeylist);
            }
            catch (Exception ex)
            { }

            try
            {
                PushShareDoc(this);
            }
            catch (Exception ex)
            { }

            try
            {
                SendTaskNotice();
                //SendBookedReportNotice();
            }
            catch (Exception ex)
            { }


            try
            {
                var filename = "log" + DateTime.Now.ToString("yyyy-MM-dd");
                var wholefilename = Server.MapPath("~/userfiles") + "\\" + filename;

                var content = System.IO.File.ReadAllText(wholefilename);
                content = content + "heart beat start @ " + starttime + "\r\n";
                System.IO.File.WriteAllText(wholefilename, content);
            }
            catch (Exception ex)
            { }

            try
            {
                SendTaskAlertEmail();
                SendRMAAlertEmail();
                SendOBAAlertEmail();
            }
            catch (Exception ex) { }



            foreach (var pjkey in pjkeylist)
            {
                try
                {
                    ProjectTestData.PrePareMESLatestData(pjkey, this);
                }
                catch (Exception ex)
                { }
            }

            foreach (var pjkey in pjkeylist)
            {
                try
                {
                    ProjectTestData.PrePareOSALatestData(pjkey, this);
                }
                catch (Exception ex)
                { }
            }

            foreach (var pjkey in pjkeylist)
            {
                try
                {
                    ProjectTestData.PrePareATELatestData(pjkey,this);
                }
                catch (Exception ex)
                { }
            }

            try
            {
                ExternalDataCollector.RefreshRMAData(this);
            }
            catch (Exception ex) { }

            try
            {
                ExternalDataCollector.RefreshRELData(this);
            }
            catch (Exception ex) { }

            try
            {
                ExternalDataCollector.RefreshOBAFromDMR(this);
            }
            catch (Exception ex) { }

            try
            {
                BIDataUtility.LoadBITestDateFromAuto(this);
            }
            catch (Exception ex) { }

            foreach (var pjkey in pjkeylist)
            {
                try
                {
                    BITestData.PrePareLatestData(this, pjkey);
                }
                catch (Exception ex)
                { }
            }

            foreach (var pjkey in pjkeylist)
            {
                try
                {
                    ProcessData.LoadMesWorkflow(pjkey, this);
                }
                catch (Exception ex)
                { }
            }

            foreach (var pjkey in pjkeylist)
            {
                try
                {
                    ProcessData.LoadMESMoveHistory(pjkey, this);
                }
                catch (Exception ex)
                { }
            }

            try
            {
                BIDataUtility.LoadModuleTXOFromMESBackup(this);
            }
            catch (Exception ex) { }

            try
            {
                BIDataUtility.LoadProcessTXOFromAuto(this);
            }
            catch (Exception ex) { }

            try
            {
                ExternalDataCollector.RefreshNeoMAPData(this);
            }
            catch (Exception ex) { }

            try
            {
                var filename = "log" + DateTime.Now.ToString("yyyy-MM-dd");
                var wholefilename = Server.MapPath("~/userfiles") + "\\" + filename;

                var content = System.IO.File.ReadAllText(wholefilename);
                content = content + "heart beat end @ " + DateTime.Now.ToString() + "\r\n";
                System.IO.File.WriteAllText(wholefilename, content);
            }
            catch (Exception ex)
            { }

            try
            {
                var ckeylist = new List<string>();

                var mycache = HttpContext.Cache;
                var cacheitem = mycache.GetEnumerator();
                while (cacheitem.MoveNext())
                {
                    var ckey = Convert.ToString(cacheitem.Key);
                    ckeylist.Add(ckey);
                }

                foreach (var ckey in ckeylist)
                {
                    if (ckey.Contains("_CUST"))
                    {
                        mycache.Remove(ckey);
                    }
                }

                foreach (var pjkey in pjkeylist)
                {
                    ProjectYieldViewModule.GetYieldByWeeks(pjkey, mycache, 4);
                }
                _viewallprivate();
            }
            catch (Exception ex)
            { }

            return View();
        }

        private static void logjoinfo(string info)
        {
            var filename = "d:\\log\\updatejo-" + DateTime.Now.ToString("yyyy-MM-dd");
            if (System.IO.File.Exists(filename))
            {
                var content = System.IO.File.ReadAllText(filename);
                content = content + "\r\n" + DateTime.Now.ToString() + " : " + info;
                System.IO.File.WriteAllText(filename, content);
            }
            else
            {
                System.IO.File.WriteAllText(filename, DateTime.Now.ToString() + " : " + info);
            }
        }

        private void UpdateJO()
        {
            var sns = ProjectTestData.RetrieveAllSNWithNoJO();
            logjoinfo("get sn count from local: " + sns.Count.ToString());

            var dict = new Dictionary<string, string>();

            int i = 0;
            foreach (var sn in sns)
            {
                var jo = ProjectTestData.RetrieveJOFromMESSN(sn);
                if (!string.IsNullOrEmpty(jo))
                {
                    dict.Add(sn, jo);
                    i = i + 1;
                }

                if (i % 10000 == 0)
                {
                    logjoinfo("read jo count: " + i.ToString());

                    foreach (var kv in dict)
                    {
                        ProjectTestData.UpdateJO(kv.Key, kv.Value);
                    }

                    dict.Clear();
                    logjoinfo("write jo ");
                }
            }

            logjoinfo("write last jo ");
            foreach (var kv in dict)
            {
                ProjectTestData.UpdateJO(kv.Key, kv.Value);
            }
            logjoinfo("finish last jo ");
        }

        private void AssignPJ2User()
        {
            var allpjkey = ProjectViewModels.RetrieveAllProject();
            foreach (var pjkey in allpjkey)
            {
                var pjmembers = ProjectViewModels.RetrieveProjectMembers(pjkey.ProjectKey);
                foreach (var member in pjmembers)
                {
                    UserViewModels.UpdateUserProject(member.Name.ToUpper(), pjkey.ProjectKey);
                }
            }

        }

        public ActionResult HeartBeat2()
        {
            //ProjectTestData.PrePareOSALatestData("25GWIRELESSTOSAG", this);
            
            //var allpjkey = ProjectViewModels.RetrieveAllProject();
            //foreach (var pjkey in allpjkey)
            //{
            //    ProjectYieldViewModule.GetYieldByWeeks(pjkey.ProjectKey, HttpContext.Cache, 4);
            //}
            //MESUtility.DebugCriticalFunction("4803ad800037d81c", "QSFPSR4GEN4", this);
            //try
            //{
            //    var ckeylist = new List<string>();

            //    var mycache = HttpContext.Cache;
            //    var cacheitem = mycache.GetEnumerator();
            //    while (cacheitem.MoveNext())
            //    {
            //        var ckey = Convert.ToString(cacheitem.Key);
            //        ckeylist.Add(ckey);
            //    }

            //    foreach (var ckey in ckeylist)
            //    {
            //        if (ckey.Contains("_CUST"))
            //        {
            //            mycache.Remove(ckey);
            //        }
            //    }

            //    _viewallprivate();
            //}
            //catch (Exception ex)
            //{ }

            //var traceviewlist = ExternalDataCollector.LoadTraceView2Local("IPH_TEST322", "XXC00RT", "ER Setup", "2017-07-01 12:23:20 AM", this);
            //foreach (var item in traceviewlist)
            //{
            //    ExternalDataCollector.RetrieveTestDataFromTraceView(item, "Test_Base_TxPower", "TxPower");
            //}

            //var traceviewlist = ExternalDataCollector.LoadTraceView2Local("IPH_TEST322", "XXC00QN", "ER Setup", "2017-07-04 08:33:06", this);
            //foreach (var item in traceviewlist)
            //{
            //    ExternalDataCollector.RetrieveTestDataFromTraceView(item, "Test_MultiChannel_DCSetup_2", "LDI");
            //}

            //AssignPJ2User();

            //ExternalDataCollector.RefreshRMAData(this);
            //ExternalDataCollector.RefreshRELData(this);
            //ExternalDataCollector.RefreshNeoMAPData(this);
            //ExternalDataCollector.RefreshOBAFromDMR(this);
            //BIDataUtility.LoadBITestDateFromAuto(this);
            //BIDataUtility.LoadModuleTXOFromMESBackup(this);
            //BIDataUtility.LoadProcessTXOFromAuto(this);
            //ExternalDataCollector.RefreshOBAFromDMR(this);

            //ProcessData.LoadMESMoveHistory("EDRLP", this);

            //var pjkeylist = ProjectViewModels.RetrieveAllProjectKey();
            //foreach (var pjkey in pjkeylist)
            //{
            //    try
            //    {
            //ProcessData.LoadMESMoveHistory("EDR", this);
            //}
            //catch (Exception ex)
            //{ }
            //}
            //foreach (var pjkey in pjkeylist)
            //{
            //try
            //{
            //    ProjectTestData.PrePareMESLatestData("EDRLP", this);
            //}
            //catch (Exception ex)
            //{ }
            //}
            //var pjkeylist = ProjectViewModels.RetrieveAllProjectKey();
            //foreach (var pjkey in pjkeylist)
            //{
            //    try
            //    {
            //        //UpdateJO();
            //        //SendTaskNotice();
            //        //BITestData.PrePareLatestData(this,pjkey);
            //    }
            //    catch (Exception ex)
            //    { }
            //}
            return View("HeartBeat");
        }

        public ActionResult HeartBeat3()
        {

            try
            {
                //ExternalDataCollector.RefreshOBAFromDMR(this);
                //ExternalDataCollector.RefreshNeoMAPData(this);
            }
            catch (Exception ex)
            { }

            return View("HeartBeat");
        }

        private static bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        public ActionResult TestPNDesc(string PNs)
        {
            var vm = new List<List<string>>();

            var partnums = new List<string>();
            if (!string.IsNullOrEmpty(PNs))
            {
                var ps = PNs.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

                var pndeslist = new List<string>();
                foreach (var p in ps)
                {
                    if (!IsDigitsOnly(p.Trim()))
                    {
                        pndeslist.Add(p.Trim());
                    }
                }

                if (pndeslist.Count > 0)
                {
                    partnums = MESUtility.RetrieveAllPN(pndeslist);
                }
            }
            var title = new List<string>();

            var colSize = 8;
            for(var i = 1; i <= colSize; i++)
            {
                title.Add("Part Num " + i);
            }
            ViewBag.colSize = colSize;
            ViewBag.tHead = title;
            ViewBag.pnList = partnums;

            return View(vm);
        }

        public ActionResult RetrieveWaferData(string pjkey)
        {
            if (!string.IsNullOrEmpty(pjkey))
            {
                try
                {
                    BITestData.RetrieveWaferDataFromMes(this, pjkey);
                    var pjkeylist = ProjectViewModels.RetrieveAllProjectKey();
                    RealCheckVcselYieldByWafer(pjkeylist);
                }
                catch (Exception ex)
                { }

            }

            return RedirectToAction("ViewAll", "Project");
        }

        public ActionResult LowYieldWarn(string IssueKey)
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {

            }
            else
            {
                var ck = new Dictionary<string, string>();
                ck.Add("logonredirectctrl", "Project");
                ck.Add("logonredirectact", "LowYieldWarn");
                ck.Add("issuekey", IssueKey);
                CookieUtility.SetCookie(this, ck);
                return RedirectToAction("LoginUser", "User");
            }

            var key = "";
            if (!string.IsNullOrEmpty(IssueKey))
            {
                var ck = new Dictionary<string, string>();
                ck.Add("issuekey", IssueKey);
                ck.Add("currentaction", "LowYieldWarn");
                CookieUtility.SetCookie(this, ck);
                key = IssueKey;
            }
            else if (ckdict.ContainsKey("issuekey") && !string.IsNullOrEmpty(ckdict["issuekey"]))
            {
                key = ckdict["issuekey"];
                var ck = new Dictionary<string, string>();
                ck.Add("currentaction", "LowYieldWarn");
                CookieUtility.SetCookie(this, ck);
            }

            if (string.IsNullOrEmpty(key))
            {
                return View();
            }

            var asilist = UserViewModels.RetrieveAllUser();
            ViewBag.towholist1 = CreateSelectList(asilist, "");

            var ret = IssueViewModels.RetrieveIssueByIssueKey(key, this);
            if (ret != null)
            {
                var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
                var defaultlytteam = UserGroupVM.RetreiveUserGroup(updater, UserGroupType.LYTGroup);
                if (!string.IsNullOrEmpty(defaultlytteam))
                {
                    ViewBag.defaultlytteam = defaultlytteam;
                }
                return View(ret);
            }
            else
            { return View(); }
        }


        private void SendLYTEvent(IssueViewModels LYTTASK, IssueViewModels vm, string comment, List<string> addrlist)
        {
            var routevalue = new RouteValueDictionary();
            routevalue.Add("issuekey", vm.IssueKey);
            //send validate email
            string scheme = this.Url.RequestContext.HttpContext.Request.Url.Scheme;
            string validatestr = this.Url.Action("UpdateIssue", "Issue", routevalue, scheme);

            routevalue["issuekey"] = LYTTASK.IssueKey;
            string validatestr2 = this.Url.Action("UpdateIssue", "Issue", routevalue, scheme);

            var netcomputername = "";
            try { netcomputername = System.Net.Dns.GetHostName(); }
            catch (Exception ex) { }
            validatestr = validatestr.Replace("//localhost", "//" + netcomputername);
            validatestr2 = validatestr2.Replace("//localhost", "//" + netcomputername);


            var content = "Hi All,\r\n\r\nThis is a Critical Error Alarm information. Please pay your attention to it. Thanks!";
            content = content + "\r\n\r\n[" + LYTTASK.Summary + "]  is created base on analyse of task: ";
            content = content + "\r\n\r\n" + vm.Summary;
            content = content + "\r\n\r\nCritical Error TASK LINK: " + validatestr2;
            content = content + "\r\n\r\nTRIGGER TASK LINK: " + validatestr;

            var toaddrs = new List<string>();
            toaddrs.AddRange(addrlist);
            toaddrs.Add(vm.Assignee);
            toaddrs.Add(vm.Reporter);
            toaddrs.Add(LYTTASK.Assignee);

            EmailUtility.SendEmail(this, "Project Critical Error Alarm - WUXI Engineering System", toaddrs, content);

            IssueViewModels.UpdateLYT(vm.IssueKey);
            new System.Threading.ManualResetEvent(false).WaitOne(500);
        }

        [HttpPost, ActionName("LowYieldWarn")]
        [ValidateAntiForgeryToken]
        public ActionResult LowYieldWarnPost()
        {
            var issuekey = Request.Form["IssueKey"];
            var vm = IssueViewModels.RetrieveIssueByIssueKey(issuekey, this);
            if (vm != null)
            {
                var ckdict = CookieUtility.UnpackCookie(this);
                var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

                if (string.Compare(vm.Assignee, updater, true) == 0
                    && Request.Form["sendisu"] != null)
                {
                    var comment = Request.Form["commentcontent"];
                    var addrs = Request.Form["RPeopleAddr"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

                    var LYTTASK = CreateLYTTask(CRITICALERRORTYPE.LYTTASK, comment, vm.ProjectKey, updater, updater, DateTime.Now.AddDays(14), vm, Request.Form["RPeopleAddr"]);
                    //CreateLYTSubTask(CRITICALERRORTYPE.LYTSUBTASK, "Stop Product Line for " + comment, vm.ProjectKey, LYTTASK.IssueKey, updater, updater, DateTime.Now.AddDays(1));
                    CreateLYTSubTask(CRITICALERRORTYPE.CONTAINMENTACTION, "Containment Action for " + comment, vm.ProjectKey, LYTTASK.IssueKey, updater, updater, DateTime.Now.AddDays(7));
                    CreateLYTSubTask(CRITICALERRORTYPE.CORRECTIVEACTION, "Corrective Action for " + comment, vm.ProjectKey, LYTTASK.IssueKey, updater, updater, DateTime.Now.AddDays(14));
                    //CreateLYTSubTask(CRITICALERRORTYPE.LYTSUBTASK, "Restart Product Line for " + comment, vm.ProjectKey, LYTTASK.IssueKey, updater, updater, DateTime.Now.AddDays(14));

                    var comment1 = new IssueComments();
                    comment1.Comment = "<p>" + comment + "</p><p><a href='/Issue/UpdateIssue?issuekey=" + vm.IssueKey + "'><strong>Trigger Task</strong></a></p>";
                    if (vm.FailureDetailCommentList.Count > 0)
                    {
                        comment1.Comment = comment1.Comment + "<p><h3>Trigger Failure Detail</h3></p><p>" + vm.FailureDetailCommentList[0].Comment + "</p>";
                    }
                    if (vm.RootCauseCommentList.Count > 0)
                    {
                        comment1.Comment = comment1.Comment + "<p><h3>Trigger Root Cause</h3></p><p>" + vm.RootCauseCommentList[0].Comment + "</p>";
                    }
                    IssueViewModels.StoreIssueComment(LYTTASK.IssueKey, comment1.dbComment, vm.Assignee, COMMENTTYPE.Description);

                    foreach (var attach in vm.AttachList)
                    {
                        IssueViewModels.StoreIssueAttachment(LYTTASK.IssueKey, attach);
                    }

                    var originalcomment = new IssueComments();
                    originalcomment.Comment = "<p>A LYT task is create base on this task:</p>" + "<p><a href='/Issue/UpdateIssue?issuekey=" + LYTTASK.IssueKey + "'>LYT TASK</a></p>";
                    IssueViewModels.StoreIssueComment(vm.IssueKey, originalcomment.dbComment, vm.Assignee, COMMENTTYPE.Description);

                    var addrlist = new List<string>();
                    addrlist.AddRange(addrs);
                    SendLYTEvent(LYTTASK, vm, comment, addrlist);

                    UserKPIVM.AddUserDailyRank(LYTTASK.IssueKey, updater, UserRankType.SPECIAL, "Create LYT Task: " + comment, "/Issue/UpdateIssue?issuekey=" + LYTTASK.IssueKey, 4);

                    var dict = new RouteValueDictionary();
                    dict.Add("issuekey", LYTTASK.IssueKey);
                    return RedirectToAction("UpdateIssue", "Issue", dict);
                }
                else
                {
                    var dict = new RouteValueDictionary();
                    dict.Add("ProjectKey", vm.ProjectKey);
                    return RedirectToAction("ProjectDetail", "Project", dict);
                }
            }
            return RedirectToAction("ViewAll", "Project");
        }

        private static void CreateLYTSubTask(string presum, string sum, string pjkey, string parentkey, string analyser, string reporter, DateTime duedate)
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

            var comment1 = new IssueComments();
            comment1.Comment = vm.Summary;
            IssueViewModels.StoreIssueComment(vm.IssueKey, comment1.dbComment, vm.Assignee, COMMENTTYPE.Description);
        }

        private static IssueViewModels CreateLYTTask(string presum, string sum, string pjkey, string analyser, string reporter, DateTime duedate, IssueViewModels trigglevm,string relatedaddrs)
        {
            var vm = new IssueViewModels();
            vm.ProjectKey = pjkey;
            vm.IssueKey = IssueViewModels.GetUniqKey();
            vm.IssueType = ISSUETP.Task;
            vm.Summary = presum + sum;
            vm.Priority = ISSUEPR.Major;
            vm.DueDate = duedate;
            vm.ReportDate = DateTime.Now;
            vm.Assignee = analyser;
            vm.Reporter = reporter;
            vm.Resolution = Resolute.Pending;
            vm.ResolvedDate = DateTime.Parse("1982-05-06 01:01:01");
            vm.ModuleSN = trigglevm.ModuleSN;
            vm.ErrAbbr = trigglevm.ErrAbbr;
            vm.RelativePeoples = relatedaddrs;
            vm.StoreSubIssue();

            return vm;
        }

        public ActionResult CloseResultComment(string ErrorKey, string CommentType, string Date)
        {
            if (!string.IsNullOrEmpty(ErrorKey) && !string.IsNullOrEmpty(CommentType) && !string.IsNullOrEmpty(Date))
            {
                ProjectErrorViewModels.CloseSPResultComment(ErrorKey, CommentType, Date);
                var dict = new RouteValueDictionary();
                dict.Add("ErrorKey", ErrorKey);
                return RedirectToAction("UpdateProjectError", "Project", dict);
            }

            return RedirectToAction("ViewAll", "Project");
        }

        public ActionResult UpdateErrorComment(string ErrorKey, string CommentType, string Date)
        {
            if (!string.IsNullOrEmpty(ErrorKey) && !string.IsNullOrEmpty(CommentType) && !string.IsNullOrEmpty(Date))
            {
                var errorcomment = ProjectErrorViewModels.RetrieveSPComment(ErrorKey, CommentType, Date);
                return View(errorcomment);
            }

            return RedirectToAction("ViewAll", "Project");
        }

        [HttpPost, ActionName("UpdateErrorComment")]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateErrorCommentPost()
        {
            var errorkey = Request.Form["HErrorKey"];
            var commenttype = Request.Form["HType"];
            var commentdate = Request.Form["HDate"];

            if (!string.IsNullOrEmpty(Request.Form["editor1"]))
            {
                var tempcommment = new ErrorComments();
                tempcommment.Comment = SeverHtmlDecode.Decode(this, Request.Form["editor1"]);
                ProjectErrorViewModels.UpdateSPComment(errorkey, commenttype, commentdate, tempcommment.dbComment);
            }
            else
            {
                var tempcommment = new ErrorComments();
                tempcommment.Comment = "<p>To Be Edit</p>";
                ProjectErrorViewModels.UpdateSPComment(errorkey, commenttype, commentdate, tempcommment.dbComment);
            }

            var dict = new RouteValueDictionary();
            dict.Add("ErrorKey", errorkey);
            return RedirectToAction("UpdateProjectError", "Project", dict);
        }

        public ActionResult IPushDebug(string ErrorKey, string ToWho, string Reason)
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
                var commenttype = PJERRORCOMMENTTYPE.Description;
                ProjectErrorViewModels.StoreErrorComment(ErrorKey, dbstr, commenttype, updater, DateTime.Now.ToString());
                tempreason = tempreason.Replace("<p>", "").Replace("</p>", "\r\n");
            }

            var debugtree = ProjectErrorViewModels.RetrieveErrorByErrorKey(ErrorKey, this);
            ShareDocVM.ShareDoc(debugtree[0].ProjectKey, ShareDocType.DEBUG, debugtree[0].ErrorKey, debugtree[0].OrignalCode, updater, DateTime.Now.ToString(), "/Project/UpdateProjectError?ErrorKey=" + debugtree[0].ErrorKey);

            var whoes = ToWho.Split(new string[] { ";", ",", " " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var w in whoes)
            {
                ShareDocVM.IPushDoc(debugtree[0].ProjectKey, debugtree[0].ErrorKey, w, updater, this, tempreason);
            }

            ShareDocVM.SendPushDocEvent("a debug about " + debugtree[0].ProjectKey + "-" + debugtree[0].ErrorKey
                , "/Project/UpdateProjectError?ErrorKey=" + debugtree[0].ErrorKey, updater, updater, this, tempreason);

            var dict1 = new RouteValueDictionary();
            dict1.Add("ErrorKey", ErrorKey);
            return RedirectToAction("UpdateProjectError", "Project", dict1);
        }

        public ActionResult LoadProjects()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
            var allprojlist = ProjectViewModels.RetrieveAllProject();

            ViewBag.pjtpdict = new Dictionary<string, bool>();
            ViewBag.pjtypelist = new List<string>();
            var syscfg = CfgUtility.GetSysConfig(this);
            var pjtylist = syscfg["PROJECTTYPE"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var pjt in pjtylist)
            {
                ViewBag.pjtypelist.Add(pjt);
            }

            foreach (var item in allprojlist)
            {
                var tpstr = "";
                if (string.IsNullOrEmpty(item.ProjectType))
                {
                    tpstr = "Parallel";
                    item.ProjectType = "Parallel";
                }
                else
                {
                    tpstr = item.ProjectType;
                }

                if (!ViewBag.pjtpdict.ContainsKey(tpstr))
                {
                    ViewBag.pjtpdict.Add(tpstr, true);
                }
            }

            ViewBag.userpjkey = UserViewModels.RetrieveUserProjectKeyDict(updater);

            return View(allprojlist);
        }

        [HttpPost, ActionName("LoadProjects")]
        [ValidateAntiForgeryToken]
        public ActionResult LoadProjectsPost()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
            var olduserpjkeydict = UserViewModels.RetrieveUserProjectKeyDict(updater);
            var olduserpjkeylist = olduserpjkeydict.Keys;

            var allprojlist = ProjectViewModels.RetrieveAllProject();
            var pjcount = allprojlist.Count;

            var choosedpj = new List<string>();
            var choosedpjdict = new Dictionary<string, bool>();
            for (var idx = 0; idx < pjcount; idx++)
            {
                if (Request.Form["projectkey" + idx] != null)
                {
                    choosedpj.Add(Request.Form["projectkey" + idx]);
                    choosedpjdict.Add(Request.Form["projectkey" + idx], true);
                }
            }

            foreach (var okey in olduserpjkeylist)
            {
                if (!choosedpjdict.ContainsKey(okey))
                {
                    UserViewModels.RemovePJfromUser(updater, okey);
                }
            }

            foreach (var newkey in choosedpj)
            {
                UserViewModels.UpdateUserProject(updater, newkey);
            }

            var mycache = HttpContext.Cache;
            mycache.Remove(updater + "_pjlist_CUST");

            return RedirectToAction("ViewAll", "Project");
        }

        public ActionResult SameAsModules(string ErrorKey, string LinkTime)
        {
            if (!string.IsNullOrEmpty(ErrorKey) && !string.IsNullOrEmpty(LinkTime))
            {
                var vm = SameAsDBTVM.RetriveSameAsIssues(ErrorKey, LinkTime);
                return View(vm);
            }
            return RedirectToAction("ViewAll", "Project");
        }

        public ActionResult DashDemo()
        {
            return View();
        }

        public ActionResult ProjectDash(string PJKey)
        {
            if (!string.IsNullOrEmpty(PJKey))
            {
                var ckdict = CookieUtility.UnpackCookie(this);
                if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
                {

                }
                else
                {
                    var ck = new Dictionary<string, string>();
                    ck.Add("logonredirectctrl", "Project");
                    ck.Add("logonredirectact", "ProjectDash");
                    ck.Add("PJKey", PJKey);
                    CookieUtility.SetCookie(this, ck);
                    return RedirectToAction("LoginUser", "User");
                }

                var ck1 = new Dictionary<string, string>();
                ck1.Add("PJKey", PJKey);
                CookieUtility.SetCookie(this, ck1);
                ViewBag.PJKey = PJKey;

                var reportmark = new List<string>();
                reportmark.Add("Please select report mark");
                reportmark.Add(WEEKLYREPORTMARK.HighLight);
                reportmark.Add(WEEKLYREPORTMARK.LowLight);
                reportmark.Add(WEEKLYREPORTMARK.Notable);
                var tempcontrol = CreateSelectList1(reportmark, "");
                tempcontrol[0].Disabled = true;
                tempcontrol[0].Selected = true;
                ViewBag.wkrptmarklist = tempcontrol;

            }
            return View();
        }

        [HttpGet]
        public JsonResult InitPJMGTask(string PJKey)
        {
            var vm = IssueViewModels.RetrievePMTask(PJKey, Resolute.Pending, this);
            var list1 = IssueViewModels.RetrievePMTask(PJKey, Resolute.Working, this);
            var list2 = IssueViewModels.RetrievePMTask(PJKey, Resolute.Done, this);
            vm.AddRange(list1);
            vm.AddRange(list2);

            var pendinglist = new List<object>();
            var donginglist = new List<object>();
            var donelist = new List<object>();

            foreach (var item in vm)
            {
                if (string.Compare(item.Resolution, Resolute.Pending, true) == 0)
                {
                    pendinglist.Add(
                        new
                        {
                            id = item.IssueKey,
                            title = item.Summary.Replace(CRITICALERRORTYPE.PMTASK, "").Trim() + "  <a href='/Issue/UpdateIssue?issuekey=" + item.IssueKey + "' target='_blank'>Detail</a>",
                            description = item.CommentList.Count > 0 ? item.CommentList[0].Comment : string.Empty,
                            dueDate = item.DueDate.ToString("yyyy-MM-dd")
                        });
                }

                if (string.Compare(item.Resolution, Resolute.Working, true) == 0
                    || string.Compare(item.Resolution, Resolute.Reopen, true) == 0)
                {
                    donginglist.Add(
                        new
                        {
                            id = item.IssueKey,
                            title = item.Summary.Replace(CRITICALERRORTYPE.PMTASK, "").Trim() + "  <a href='/Issue/UpdateIssue?issuekey=" + item.IssueKey + "' target='_blank'>Detail</a>",
                            description = item.CommentList.Count > 0 ? item.CommentList[0].Comment : string.Empty,
                            dueDate = item.DueDate.ToString("yyyy-MM-dd")
                        });
                }

                if (string.Compare(item.Resolution, Resolute.Fixed, true) == 0
                    || string.Compare(item.Resolution, Resolute.Done, true) == 0)
                {
                    donelist.Add(
                        new
                        {
                            id = item.IssueKey,
                            title = item.Summary.Replace(CRITICALERRORTYPE.PMTASK, "").Trim() + "  <a href='/Issue/UpdateIssue?issuekey=" + item.IssueKey + "' target='_blank'>Detail</a>",
                            description = item.CommentList.Count > 0 ? item.CommentList[0].Comment : string.Empty,
                            dueDate = item.DueDate.ToString("yyyy-MM-dd")
                        });
                }
            }

            var mylists = new List<object>();
            mylists.Add(
                new
                {
                    id = "mytobelist",
                    title = "TODO",
                    defaultStyle = "lobilist-warning",
                    controls = false,
                    useCheckboxes = false,
                    items = pendinglist
                });

            mylists.Add(
                new
                {
                    id = "mydoinglist",
                    title = "DOING",
                    defaultStyle = "lobilist-info",
                    controls = false,
                    useCheckboxes = false,
                    items = donginglist
                });

            mylists.Add(
                new
                {
                    id = "mydonelist",
                    title = "DONE",
                    defaultStyle = "lobilist-success",
                    controls = false,
                    useCheckboxes = false,
                    enableTodoRemove = false,
                    items = donelist
                });

            var res = new JsonResult();
            res.Data = new { lists = mylists };
            res.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return res;
        }

        private IssueViewModels ListOperateParse()
        {
            var ret = new IssueViewModels();
            var jsonStringData = new System.IO.StreamReader(Request.InputStream).ReadToEnd();
            var items = jsonStringData.Split(new string[] { "&" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var it in items)
            {
                if (it.Contains("id="))
                {
                    ret.IssueKey = it.Replace("id=", "").Trim();
                }

                if (it.Contains("title="))
                {
                    ret.Summary = SeverHtmlDecode.Decode(this, it.Replace("title=", "")).Replace("'", "").Replace("+", " ").Trim();
                }

                if (it.Contains("description="))
                {
                    if (string.IsNullOrEmpty(it.Replace("description=", "")))
                    {
                        ret.Description = string.Empty;
                    }
                    else
                    {
                        ret.Description = SeverHtmlDecode.Decode(this, it.Replace("description=", "")).Replace("'", "").Replace("+", " ").Trim();
                    }
                }

                if (it.Contains("dueDate="))
                {
                    try
                    {
                        ret.DueDate = DateTime.Parse(SeverHtmlDecode.Decode(this, it.Replace("dueDate=", "")).Replace("'", "").Trim() + " 10:00:00");
                    }
                    catch (Exception ex) { ret.Summary = string.Empty; }
                }

                if (it.Contains("listId="))
                {
                    ret.DataID = SeverHtmlDecode.Decode(this, it.Replace("listId=", "")).Replace("'", "").Trim();
                }

            }
            return ret;
        }

        private static void SendTaskEvent(IssueViewModels vm, string comment, Controller ctrl)
        {
            var routevalue = new RouteValueDictionary();
            routevalue.Add("issuekey", vm.IssueKey);
            //send validate email
            string scheme = ctrl.Url.RequestContext.HttpContext.Request.Url.Scheme;
            string validatestr = ctrl.Url.Action("UpdateIssue", "Issue", routevalue, scheme);

            var netcomputername = "";
            try { netcomputername = System.Net.Dns.GetHostName(); }
            catch (Exception ex) { }
            validatestr = validatestr.Replace("//localhost", "//" + netcomputername);

            var content = vm.Summary + " is created :\r\n " + validatestr;
            content = content + "\r\n\r\n" + comment;

            var toaddrs = new List<string>();
            toaddrs.Add(vm.Assignee);
            toaddrs.Add(vm.Reporter);
            if (vm.RelativePeopleList.Count > 0)
            {
                toaddrs.AddRange(vm.RelativePeopleList);
            }

            var reporter = vm.Reporter.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries)[0].Replace(".", " ");
            EmailUtility.SendEmail(ctrl, "WUXI Engineering System_" + reporter, toaddrs, content);
            new System.Threading.ManualResetEvent(false).WaitOne(30);
        }

        private IssueViewModels CreatePMTask(string pjkey, string summary, string duedate, string asignee, string creator, string desc)
        {
            var vm = new IssueViewModels();
            vm.ProjectKey = pjkey;
            vm.IssueKey = IssueViewModels.GetUniqKey();
            vm.IssueType = ISSUETP.Task;
            vm.Summary = CRITICALERRORTYPE.PMTASK + " " + summary;
            vm.Priority = ISSUEPR.Major;
            vm.DueDate = DateTime.Parse(duedate);
            vm.ReportDate = DateTime.Now;
            vm.Assignee = asignee;
            vm.Reporter = creator;
            vm.Creator = creator;
            vm.Resolution = Resolute.Pending;
            vm.ResolvedDate = DateTime.Parse("1982-05-06 01:01:01");
            vm.Description = desc;
            vm.StoreIssue();
            SendTaskEvent(vm, desc, this);
            return vm;
        }

        [HttpPost]
        public JsonResult TodoListAdd()
        {
            var vm = ListOperateParse();
            if (string.IsNullOrEmpty(vm.Summary))
            {
                var res1 = new JsonResult();
                res1.Data = new { success = false };
                return res1;
            }

            if (string.Compare(vm.DataID, "mytobelist", true) != 0)
            {
                var res1 = new JsonResult();
                res1.Data = new { success = false };
                return res1;
            }

            var ckdict = CookieUtility.UnpackCookie(this);
            if (!ckdict.ContainsKey("PJKey"))
            {
                var res1 = new JsonResult();
                res1.Data = new { success = false };
                return res1;
            }

            var PJKey = ckdict["PJKey"];
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
            var pj = ProjectViewModels.RetrieveOneProject(PJKey);
            var pm = "";
            foreach (var m in pj.MemberList)
            {
                if (string.Compare(m.Role, ProjectViewModels.PMROLE) == 0)
                {
                    pm = m.Name;
                    break;
                }
            }

            var task = CreatePMTask(PJKey, vm.Summary, vm.DueDate.ToString(), pm, updater, vm.Description);
            var res = new JsonResult();
            res.Data = new { success = true, id = task.IssueKey };
            return res;
        }

        [HttpPost]
        public JsonResult TodoListUpdate()
        {
            var vm = ListOperateParse();
            if (string.IsNullOrEmpty(vm.Summary))
            {
                var res1 = new JsonResult();
                res1.Data = new { success = false };
                return res1;
            }

            var realissue = IssueViewModels.RetrieveIssueByIssueKey(vm.IssueKey, this);
            if (realissue != null)
            {
                if (!string.IsNullOrEmpty(vm.Description))
                {
                    if (realissue.CommentList.Count == 0)
                    {
                        var ckdict = CookieUtility.UnpackCookie(this);
                        var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
                        var com = new IssueComments();
                        com.Comment = vm.Description;
                        IssueViewModels.StoreIssueComment(realissue.IssueKey, com.dbComment, updater, COMMENTTYPE.Description);
                    }
                    else
                    {
                        realissue.CommentList[0].Comment = vm.Description;
                        IssueViewModels.UpdateSPComment(realissue.IssueKey, realissue.CommentList[0].CommentType
                            , realissue.CommentList[0].CommentDate.ToString("yyyy-MM-dd HH:mm:ss"), realissue.CommentList[0].dbComment);
                    }
                }

                if (!vm.DueDate.Equals(realissue.DueDate))
                {
                    realissue.DueDate = vm.DueDate;
                    realissue.UpdateIssue();
                }
            }

            var res = new JsonResult();
            res.Data = new { success = true };
            return res;
        }

        [HttpPost]
        public JsonResult TodoListDelete()
        {
            var vm = ListOperateParse();
            if (string.IsNullOrEmpty(vm.Summary))
            {
                var res1 = new JsonResult();
                res1.Data = new { success = false };
                return res1;
            }

            IssueViewModels.RemoveIssue(vm.IssueKey, this);

            var res = new JsonResult();
            res.Data = new { success = true };
            res.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return res;
        }

        private List<string> MoveOperateParse()
        {
            var ret = new List<string>();
            var jsonStringData = new System.IO.StreamReader(Request.InputStream).ReadToEnd();
            var items = jsonStringData.Split(new string[] { "&" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var it in items)
            {
                if (it.Contains("id="))
                {
                    ret.Add(it.Replace("id=", "").Trim());
                }

                if (it.Contains("oldlist="))
                {
                    ret.Add(SeverHtmlDecode.Decode(this, it.Replace("oldlist=", "")).Replace("'", "").Trim());
                }

                if (it.Contains("newlist="))
                {
                    ret.Add(SeverHtmlDecode.Decode(this, it.Replace("newlist=", "")).Replace("'", "").Trim());
                }
                if (it.Contains("reportmark="))
                {
                    ret.Add(it.Replace("reportmark=", "").Trim());
                }
            }
            return ret;
        }

        [HttpPost]
        public JsonResult TodoListMove()
        {
            var ret = MoveOperateParse();
            if (ret.Count >= 3
                && !string.IsNullOrEmpty(ret[0])
                && !string.IsNullOrEmpty(ret[2]))
            {
                var realissue = IssueViewModels.RetrieveIssueByIssueKey(ret[0], this);
                if (realissue != null)
                {
                    if (string.Compare(ret[2], "TODO", true) == 0)
                    {
                        realissue.Resolution = Resolute.Pending;
                        realissue.UpdateIssue();
                    }
                    if (string.Compare(ret[2], "DOING", true) == 0)
                    {
                        realissue.Resolution = Resolute.Working;
                        realissue.UpdateIssue();
                    }
                    if (string.Compare(ret[2], "DONE", true) == 0)
                    {
                        realissue.Resolution = Resolute.Done;
                        realissue.UpdateIssue();
                    }
                }
            }

            var res = new JsonResult();
            res.Data = new { success = true };
            return res;
        }

        public ActionResult OcapWarn(string IssueKeys)
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {

            }
            else
            {
                var ck = new Dictionary<string, string>();
                ck.Add("logonredirectctrl", "Project");
                ck.Add("logonredirectact", "OcapWarn");
                ck.Add("issuekeys", IssueKeys);
                CookieUtility.SetCookie(this, ck);
                return RedirectToAction("LoginUser", "User");
            }

            var keys = "";
            if (!string.IsNullOrEmpty(IssueKeys))
            {
                var ck = new Dictionary<string, string>();
                ck.Add("issuekeys", IssueKeys);
                ck.Add("currentaction", "OcapWarn");
                CookieUtility.SetCookie(this, ck);
                keys = IssueKeys;
            }
            else if (ckdict.ContainsKey("issuekeys") && !string.IsNullOrEmpty(ckdict["issuekeys"]))
            {
                keys = ckdict["issuekeys"];
                var ck = new Dictionary<string, string>();
                ck.Add("currentaction", "OcapWarn");
                CookieUtility.SetCookie(this, ck);
            }

            if (string.IsNullOrEmpty(keys))
            {
                return RedirectToAction("ViewAll", "Project");
            }

            var asilist = UserViewModels.RetrieveAllUser();
            ViewBag.towholist = CreateSelectList(asilist, "");

            var issuekeybits = Convert.FromBase64String(keys);
            var issuekeystr = System.Text.Encoding.UTF8.GetString(issuekeybits);
            var issueKeyList = issuekeystr.Split(new string[] { ",", ";"}, StringSplitOptions.RemoveEmptyEntries);
            var vmList = new List<IssueViewModels>();
            var summaryList = new List<String>();
            foreach (var key in issueKeyList)
            {
                var vm = IssueViewModels.RetrieveIssueByIssueKey(key, this);
                if (vm != null)
                {
                    vmList.Add(vm);
                    summaryList.Add(vm.Summary);
                }  
            }
            if(vmList.Count > 0)
            {
                var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
                var defaultlytteam = UserGroupVM.RetreiveUserGroup(updater, UserGroupType.LYTGroup);
                if (!string.IsNullOrEmpty(defaultlytteam))
                {
                    ViewBag.defaultlytteam = defaultlytteam;
                }
                ViewBag.IssueKeys = IssueKeys;
                ViewBag.SummaryList = summaryList;
                return View(vmList);
            }
            else
            {
                return RedirectToAction("ViewAll", "Project");
            }
        }

        [HttpPost, ActionName("OcapWarn")]
        [ValidateAntiForgeryToken]
        public ActionResult OcapWarnPost()
        {
            var issuekeybits = Convert.FromBase64String(Request.Form["IssueKeys"]);
            var issuekeystr = System.Text.Encoding.UTF8.GetString(issuekeybits);

            if ( ! String.IsNullOrEmpty(issuekeystr))
            {
                var issuekeys = issuekeystr.Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
                var ckdict = CookieUtility.UnpackCookie(this);
                var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
                ////for test
                //updater = "YAN.SHI@FINISAR.COM";
                var vms = new List<IssueViewModels>();
                var ocapNum = Request.Form["ocap_id"];
                var comment = updater.ToUpper().Replace("@FINISAR.COM", "") + " start OCAP Num [ " + ocapNum + " ] at " + DateTime.Now.ToString("MM/dd/yyyy") + ". ";
                comment += "</p><p>Comment: " + Request.Form["commentcontent"];
                var comment1 = comment.Replace("</p><p>", "");
                var files_ret = ReceiveAttachFiles();
                var fileurl = "";
                if(files_ret.Count > 0)
                {
                    fileurl = files_ret[0];
                }

                foreach (var issuekey in issuekeys)
                {
                    var vm = IssueViewModels.RetrieveIssueByIssueKey(issuekey.ToString(), this);
                    if (vm != null)
                    {
                        vms.Add(vm);
                        OcapSingletonOperate(vm, updater, comment1, fileurl);
                    }
                }
                var addrs = Request.Form["RPeopleAddr"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                var addrlist = new List<string>();
                addrlist.AddRange(addrs);
                SendOCAPEvent(vms, comment, addrlist, fileurl);
                if (vms.Count == 0)
                {
                    return RedirectToAction("ViewAll", "Project");
                }
                else if (vms.Count == 1)
                {
                    var dict = new RouteValueDictionary();
                    dict.Add("issuekey", vms[0].IssueKey);
                    return RedirectToAction("UpdateIssue", "Issue", dict);
                }
                else
                {
                    var dict = new RouteValueDictionary();
                    dict.Add("ProjectKey", vms[0].ProjectKey);
                    return RedirectToAction("ProjectSptTask", "Project", dict);
                }
            }

            return RedirectToAction("ViewAll", "Project");
        }

        private bool StoreOSAFailuredCodeMap(ProjectViewModels projectmodel)
        {
            try
            {
                string fl = "osafile";
                if (Request.Files[fl] != null)
                {
                    string fn = System.IO.Path.GetFileName(Request.Files[fl].FileName);
                    string datestring = DateTime.Now.ToString("yyyyMMdd");
                    string imgdir = Server.MapPath("~/userfiles") + "\\docs\\" + datestring + "\\";

                    if (!Directory.Exists(imgdir))
                    {
                        Directory.CreateDirectory(imgdir);
                    }

                    fn = Path.GetFileNameWithoutExtension(fn) + "-" + DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(fn);
                    Request.Files[fl].SaveAs(imgdir + fn);
                    var ret = RetriveOSATables(projectmodel.ProjectKey,imgdir + fn);

                    //store test station
                    var stationlist = new List<string>();
                    var stationdict = new Dictionary<string, bool>();
                    foreach (var item in ret)
                    {
                        if (!stationdict.ContainsKey(item.WhichTest))
                        {
                            stationdict.Add(item.WhichTest,true);
                            stationlist.Add(item.WhichTest);
                        }
                    }
                    if (stationlist.Count > 0)
                    {
                        ProjectViewModels.StoreOSAStation(projectmodel.ProjectKey,stationlist);
                    }

                    if (ret.Count > 0)
                    {
                        projectmodel.OSATabList = ret;
                        return OSAFailureVM.UpdateOSAFailureVM(ret);
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private List<OSAFailureVM> RetriveOSATables(string pjkey,string filename)
        {
            var ret = new List<OSAFailureVM>();
            try
            {
                if (System.IO.File.Exists(filename))
                {
                    var idx = 0;
                    var data = ExcelReader.RetrieveDataFromExcel(filename, null);
                    foreach (var line in data)
                    {
                        if (idx == 0)
                        {
                            idx++;
                            continue;
                        }

                        var lowlimit = -99999.0;
                        if (!string.IsNullOrEmpty(line[4]))
                        {
                            try
                            {
                                lowlimit = Convert.ToDouble(line[4]);
                            }
                            catch (Exception ex)
                            {
                                continue;
                            }
                        }

                        var highlimit = 99999.0;
                        if (!string.IsNullOrEmpty(line[5]))
                        {
                            try
                            {
                                highlimit = Convert.ToDouble(line[5]);
                            }
                            catch (Exception ex)
                            {
                                continue;
                            }
                        }

                        var tempvm = new OSAFailureVM(pjkey, line[1], line[2], line[3], lowlimit, highlimit, line[6], line[7]);
                        ret.Add(tempvm);
                    }
                    
                }

                return ret;
            }
            catch (Exception ex)
            {
                ret.Clear();
                return ret;
            }
        }
        private void OcapSingletonOperate(IssueViewModels vm, string updater, string comment, string fileurl)
        {
            CreateLYTSubTask(CRITICALERRORTYPE.CONTAINMENTACTION, "Containment Action for " + comment, vm.ProjectKey, vm.IssueKey, updater, updater, DateTime.Now.AddDays(7));
            CreateLYTSubTask(CRITICALERRORTYPE.CORRECTIVEACTION, "Corrective Action for " + comment, vm.ProjectKey, vm.IssueKey, updater, updater, DateTime.Now.AddDays(14));

            var comment1 = new IssueComments();
            comment1.Comment = comment;
            IssueViewModels.StoreIssueComment(vm.IssueKey, comment1.dbComment, updater, COMMENTTYPE.Description);
            IssueViewModels.UpdateIssueAssigneeAndResolution(vm.IssueKey, updater, Resolute.Reopen, CRITICALERRORTYPE.OCAP + vm.Summary);
            if (! String.IsNullOrEmpty(fileurl))
            {
                IssueViewModels.StoreIssueAttachment(vm.IssueKey, fileurl);
            }
        }

        private void SendOCAPEvent(List<IssueViewModels> vms, string comment, List<string> addrlist, string fileUrl)
        {
            var netcomputername = "";
            try {
                netcomputername = System.Net.Dns.GetHostName();
            }
            catch (Exception ex)
            { }

            var toaddrs = new Dictionary<string, bool>();
            var body = new List<List<string>>();
            var tmpList = new List<string>();
            tmpList.Add("Project");
            tmpList.Add("ModuleSN");
            tmpList.Add("RootCause");
            tmpList.Add("Failure Description");
            tmpList.Add("Task Link");
            body.Add(tmpList);
            foreach (var vm in vms) {
                var routevalue = new RouteValueDictionary();
                routevalue.Add("issuekey", vm.IssueKey);
                //send validate email
                string scheme = this.Url.RequestContext.HttpContext.Request.Url.Scheme;
                string validatestr = this.Url.Action("UpdateIssue", "Issue", routevalue, scheme);
                validatestr = validatestr.Replace("//localhost", "//" + netcomputername);
                
                tmpList = new List<string>();
                tmpList.Add(vm.ProjectKey);
                tmpList.Add(vm.ModuleSN);
                tmpList.Add(vm.SharedTag);
                tmpList.Add(vm.Summary.Replace(CRITICALERRORTYPE.SECONDMATCH, ""));
                tmpList.Add("<a href=" + validatestr + ">Detail</a>");
                body.Add(tmpList);
                if (!toaddrs.ContainsKey(vm.Assignee))
                {
                    toaddrs.Add(vm.Assignee, true);
                }
                if (!toaddrs.ContainsKey(vm.Reporter))
                {
                    toaddrs.Add(vm.Reporter, true);
                }
            }

            var towho = new List<string>();
            towho.AddRange(toaddrs.Keys);
            towho.AddRange(addrlist);

            var greeting = "Dear All";
            var description = "Attachment is Wuxi OCAP file from PQE by WUXI ENGINEERING SYSTEM -- " + DateTime.Now.ToString("MM/dd/yyyy") + ". ";
            description = description + "Please kindly view it";
            var content = EmailUtility.CreateTableHtml(greeting, description, comment, body);

            fileUrl = fileUrl.Replace("/userfiles", Server.MapPath("~/userfiles")).Replace("/", "\\");

            EmailUtility.SendEmail(this, "Wuxi OCAP Task -- " + DateTime.Now.ToString("MM/dd/yyyy"), towho, content, true, fileUrl);
            
            new System.Threading.ManualResetEvent(false).WaitOne(500);
        }


        //private void SumOnePJSolvedIssu(string pjkey)
        //{
        //    var allerrorkey = ProjectErrorViewModels.RetrieveErrorByPJKey(pjkey, this);
        //    foreach (var err in allerrorkey)
        //    {
        //        var solvecount = IssueViewModels.RetrieveSolveIssueCount(pjkey,err.OrignalCode);
        //        ProjectErrorViewModels.UpdateSolvedIssueCount(pjkey,err.OrignalCode,solvecount);
        //    }
        //}

        //public ActionResult SumSolvedIssue()
        //{
        //    var pjlist = ProjectViewModels.RetrieveAllProjectKey();
        //    foreach (var pj in pjlist)
        //    {
        //        SumOnePJSolvedIssu(pj);
        //    }

        //    return View("HeartBeat");
        //}

    }
    
}