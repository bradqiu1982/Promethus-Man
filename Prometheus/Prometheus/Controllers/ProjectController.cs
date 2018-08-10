using System;
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
using System.Net;

namespace Prometheus.Controllers
{
    public class ProjectController : BaseController
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

                var sptcount = mycache.Get(item.ProjectKey + "_sptct_CUST");
                if (sptcount == null)
                {

                    var sptopen = IssueViewModels.RetrieveSptCountIssue(item.ProjectKey, ISSUESUBTYPE.CrititalFailureTask, 1);
                    var sptdone = IssueViewModels.RetrieveSptCountIssue(item.ProjectKey, ISSUESUBTYPE.CrititalFailureTask, 0);
                    var nonspt = IssueViewModels.RetrieveSptCountIssue(item.ProjectKey, ISSUESUBTYPE.NonCrititalFailureTask, 0);
                    item.PendingSptCount = (sptdone + nonspt).ToString() + "/" + (sptopen + sptdone + nonspt).ToString();
                    mycache.Insert(item.ProjectKey + "_sptct_CUST", item.PendingSptCount, null, DateTime.Now.AddHours(4), Cache.NoSlidingExpiration);
                }
                else
                {
                    item.PendingSptCount = Convert.ToString(sptcount);
                }
            }

        }


        public void SetIE8Flag(Dictionary<string,string> dict)
        {
            ViewBag.isie8 = false;
            ViewBag.showie8modal = false;
            var browse = Request.Browser;
            if (string.Compare(browse.Browser, "IE", true) == 0
                && (string.Compare(browse.Version, "7.0", true) == 0
                || string.Compare(browse.Version, "8.0", true) == 0))
            {
                ViewBag.isie8 = true;
            }

            if (ViewBag.isie8)
            {
                if (!dict.ContainsKey("showie8modal"))
                {
                    var ck = new Dictionary<string, string>();
                    ck.Add("showie8modal", "TRUE");
                    CookieUtility.SetCookie(this, ck);
                    ViewBag.showie8modal = true;
                }//END IF
            }//END IF
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
                var allprojlist = ProjectViewModels.N_RetrieveProjectInfo();
                var userpj = UserViewModels.N_RetrieveUserProjectKeyDict(updater);
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
            var uProModules = new Dictionary<string, List<ProjectSortVM>>();
            var pmvm = new ProjectModuleVM();
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

                var sptcount = mycache.Get(item.ProjectKey + "_sptct_CUST");
                if(sptcount == null)
                {

                    var sptopen = IssueViewModels.RetrieveSptCountIssue(item.ProjectKey, ISSUESUBTYPE.CrititalFailureTask, 1);
                    var sptdone = IssueViewModels.RetrieveSptCountIssue(item.ProjectKey, ISSUESUBTYPE.CrititalFailureTask, 0);
                    var nonspt = IssueViewModels.RetrieveSptCountIssue(item.ProjectKey, ISSUESUBTYPE.NonCrititalFailureTask, 0);
                    item.PendingSptCount = (sptdone + nonspt).ToString() + "/" + (sptopen + sptdone + nonspt).ToString();
                    mycache.Insert(item.ProjectKey + "_sptct_CUST", item.PendingSptCount, null, DateTime.Now.AddHours(4), Cache.NoSlidingExpiration);
                }
                else
                {
                    item.PendingSptCount = Convert.ToString(sptcount);
                }
                var user_pro_module = UserProjectModuleMatrix.GetUserProjectModuleMatrix(updater, item.ProjectKey);
                var pro_modules = new List<ProjectSortVM>();
                if (user_pro_module.Count == 0)
                {
                    foreach (var module in pmvm.AllModules)
                    {
                        if (module.Value.Visible)
                        {
                            var tmp = new ProjectSortVM();
                            tmp.key = module.Value.Key;
                            tmp.visible = module.Value.Visible ? "1" : "0";
                            pro_modules.Add(tmp);
                        }
                    }
                }
                else
                {
                    foreach (var tempvm in user_pro_module[item.ProjectKey].SortData)
                    {
                        if (pmvm.AllModules.ContainsKey(tempvm.key))
                        {
                            pro_modules.Add(tempvm);
                        }
                    }
                }
                uProModules.Add(item.ProjectKey, pro_modules);
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


            ViewBag.uProModules = uProModules;
            ViewBag.Default_Modules = pmvm.AllModules;

            SetIE8Flag(ckdict);

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

                if (Convert.ToInt32(pair1.PendingSptCount.Split(new string[] { "/" }, StringSplitOptions.None)[1])
                > Convert.ToInt32(pair2.PendingSptCount.Split(new string[] { "/" }, StringSplitOptions.None)[1]))
                {
                    return -1;
                }
                else if (Convert.ToInt32(pair1.PendingSptCount.Split(new string[] { "/" }, StringSplitOptions.None)[1])
                < Convert.ToInt32(pair2.PendingSptCount.Split(new string[] { "/" }, StringSplitOptions.None)[1]))
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
                ViewBag.AllUserList = "[\"" + string.Join("\",\"", asilist.ToArray()) + "\"]";

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
                ViewBag.AllUserList = "[\"" + string.Join("\",\"", asilist.ToArray()) + "\"]";

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
                ViewBag.AllUserList = "[\"" + string.Join("\",\"", asilist.ToArray()) + "\"]";

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
                    var vmlist = ProjectViewModels.RetrieveOneProject(realkey);
                    if (vmlist.Count == 0) { return RedirectToAction("CreateProject", "Project"); }

                    var vm = vmlist[0];
                    var asilist = UserViewModels.RetrieveAllUser();
                    ViewBag.AllUserList = "[\"" + string.Join("\",\"", asilist.ToArray()) + "\"]";

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

            var oldpjdata = ProjectViewModels.RetrieveOneProjectWithClose(projectmodel.ProjectKey);

            bool databondingchange = DataBondingChanged(oldpjdata[0], projectmodel);
            bool pnbondingchg = PNBondingChanged(oldpjdata[0], projectmodel);

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
                    projectmodel.TabList = oldpjdata[0].TabList;
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
            //var checkresult = CheckLoginAndPermit(Request, "Project", "ProjectIssues");
            //if (checkresult.GetType() != (new Boolean()).GetType())
            //{
            //    return (ActionResult)checkresult;
            //}

            if (ProjectKey != null)
            {
                ViewBag.PJKey = ProjectKey;

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

                var vmlist = ProjectViewModels.RetrieveOneProject(ProjectKey);
                if (vmlist.Count == 0) { return View(); }

                var vm = vmlist[0];
                if (vmlist.Count > 0)
                {
                    vm = vmlist[0];

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

                    var sptcount = mycache.Get(vm.ProjectKey + "_sptct_CUST");
                    if (sptcount == null)
                    {

                        var sptopen = IssueViewModels.RetrieveSptCountIssue(vm.ProjectKey, ISSUESUBTYPE.CrititalFailureTask, 1);
                        var sptdone = IssueViewModels.RetrieveSptCountIssue(vm.ProjectKey, ISSUESUBTYPE.CrititalFailureTask, 0);
                        var nonspt = IssueViewModels.RetrieveSptCountIssue(vm.ProjectKey, ISSUESUBTYPE.NonCrititalFailureTask, 0);
                        vm.PendingSptCount = (sptdone + nonspt).ToString() + "/" + (sptopen + sptdone + nonspt).ToString();
                        mycache.Insert(vm.ProjectKey + "_sptct_CUST", vm.PendingSptCount, null, DateTime.Now.AddHours(4), Cache.NoSlidingExpiration);
                    }
                    else
                    {
                        vm.PendingSptCount = Convert.ToString(sptcount);
                    }
                }

                var user_pro_module = UserProjectModuleMatrix.GetUserProjectModuleMatrix(updater, ProjectKey);
                var pro_modules = new List<ProjectSortVM>();
                var pmvm = new ProjectModuleVM();
                if (user_pro_module.Count == 0)
                {
                    foreach(var item in pmvm.AllModules)
                    {
                        var tmp = new ProjectSortVM();
                        tmp.key = item.Value.Key;
                        tmp.visible = item.Value.Visible ? "1" : "0";
                        pro_modules.Add(tmp);
                    }
                }
                else
                {
                    foreach (var item in user_pro_module[ProjectKey].SortData)
                    {
                        if (pmvm.AllModules.ContainsKey(item.key))
                        {
                            pro_modules.Add(item);
                        }
                    }
                }

                ViewBag.Default_Modules = pmvm.AllModules;
                ViewBag.Modules = pro_modules;

                return View(vm);
            }
            return View();
        }

        public ActionResult ProjectFA(string ProjectKey)
        {
            //var checkresult = CheckLoginAndPermit(Request, "Project", "ProjectFA");
            //if (checkresult.GetType() != (new Boolean()).GetType())
            //{
            //    return (ActionResult)checkresult;
            //}

            if (!string.IsNullOrEmpty(ProjectKey))
            {
                ViewBag.PJKey = ProjectKey;
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
                if (piedatadict.Count > 0)
                {
                    var namevaluepair = "";
                    var piedatadict_tmp = piedatadict.OrderByDescending(x => x.Value);
                    foreach (var item in piedatadict_tmp)
                    {
                        namevaluepair = namevaluepair + "{ name:'" + item.Key + "',y:" + item.Value.ToString() + "},";
                    }

                    namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                    var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart.xml"));
                    ViewBag.chartscript = tempscript.Replace("#ElementID#", "failurepie")
                        .Replace("#Title#", ProjectKey + " FA Realtime Failure")
                        .Replace("#SERIESNAME#", "Failure")
                        .Replace("#NAMEVALUEPAIRS#", namevaluepair);

                }
                var asilist = UserViewModels.RetrieveAllUser();
                ViewBag.AllUserList = "[\"" + string.Join("\",\"", asilist.ToArray()) + "\"]";

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
            //var checkresult = CheckLoginAndPermit(Request, "Project", "ProjectSptTask");
            //if (checkresult.GetType() != (new Boolean()).GetType())
            //{
            //    return (ActionResult)checkresult;
            //}

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

            ViewBag.PJKey = ProjectKey;

            //null is for qm, not null for parallel project
            var vm = IssueViewModels.RetrieveSptIssue(this, ProjectKey);
            ViewBag.rules = ProjectCriticalErrorVM.RetrievePJCriticalError(ProjectKey, null);

            var pjlist = ProjectViewModels.RetrieveOneProject(ProjectKey);
            var pqeFlag = false;
            if (pjlist.Count > 0)
            {
                foreach (var pqe in pjlist[0].MemberList)
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
                ViewBag.PJKey = ProjectKey;

                var vm = ProjectErrorViewModels.RetrieveErrorByPJKey(ProjectKey, this);
                var descdict = ProjectErrorViewModels.RetrieveShortDescDict(ProjectKey);


                var piedatadict = new Dictionary<string, int>();
                foreach (var item in vm)
                {
                    if (!piedatadict.ContainsKey(item.OrignalCode))
                    {
                        piedatadict.Add(item.OrignalCode, item.ErrorCount);
                    }
                }

                if (piedatadict.Count > 0)
                {
                    var namevaluepair = "";
                    var piedatadict_tmp = piedatadict.OrderByDescending(x => x.Value);

                    foreach (var item in piedatadict_tmp)
                    {
                        var pkey = descdict.ContainsKey(item.Key) ? descdict[item.Key] : item.Key;
                        namevaluepair = namevaluepair + "{ name:'" + pkey + "',y:" + item.Value.ToString() + "},";
                    }

                    namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                    var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart.xml"));
                    ViewBag.chartscript = tempscript.Replace("#ElementID#", "failurepie")
                        .Replace("#Title#", ProjectKey + " Total Failure")
                        .Replace("#SERIESNAME#", "Failure")
                        .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                }



                var keys = piedatadict.Keys;
                piedatadict = new Dictionary<string, int>();
                foreach (var item in vm)
                {
                    if (!piedatadict.ContainsKey(item.OrignalCode))
                    {
                        piedatadict.Add(item.OrignalCode, item.ErrorCount-item.AutoClosed);
                    }
                }
                if(piedatadict.Count  > 0)
                {
                    var namevaluepair = "";
                    var piedatadict_tmp = piedatadict.OrderByDescending(x => x.Value);
                    foreach(var item in piedatadict_tmp)
                    {
                        var pkey = descdict.ContainsKey(item.Key) ? descdict[item.Key] : item.Key;
                        namevaluepair = namevaluepair + "{ name:'" + pkey + "',y:" + item.Value.ToString() + "},";
                    }
                    namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                    var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart.xml"));
                    ViewBag.realchartscript = tempscript.Replace("#ElementID#", "realchartscript")
                        .Replace("#Title#", ProjectKey + " After Retest Failure")
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
                ViewBag.PJKey = ProjectKey;

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
                ViewBag.PJKey = ProjectKey;

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

            ViewBag.PJKey = projectkey;

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

            if(piedatadict.Count > 0)
            {
                var namevaluepair = "";
                var piedatadict_tmp = piedatadict.OrderByDescending(x => x.Value);
                foreach(var item in piedatadict_tmp)
                {
                    namevaluepair = namevaluepair + "{ name:'" + item.Key + "',y:" + item.Value.ToString() + "},";
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
            //var checkresult = CheckLoginAndPermit(Request, "Project", "ProjectNPI");
            //if (checkresult.GetType() != (new Boolean()).GetType())
            //{
            //    return (ActionResult)checkresult;
            //}

            if (ProjectKey != null)
            {
                ViewBag.PJKey = ProjectKey;

                var vmlist = IssueViewModels.RetrieveNPIPROCIssue(ProjectKey, this);
                var pjlist = ProjectViewModels.RetrieveOneProject(ProjectKey);
                if (pjlist.Count == 0) { return View(); }

                var pj = pjlist[0];
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

            ctrl.ViewBag.PJKey = ProjectKey;

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
            //var checkresult = CheckLoginAndPermit(Request, "Project", "ProjectYieldMain");
            //if (checkresult.GetType() != (new Boolean()).GetType())
            //{
            //    return (ActionResult)checkresult;
            //}

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

            sarray = new string[] { YIELDTYPE.BR, YIELDTYPE.JO, YIELDTYPE.PN };
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
                ViewBag.PJKey = ProjectKey;

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
                ViewBag.PJKey = ProjectKey;

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

                ViewBag.sDate = Convert.ToDateTime(sdate).ToString("yyyy-MM-dd"); ;
                ViewBag.eDate = Convert.ToDateTime(edate).ToString("yyyy-MM-dd"); ;

                var firstdatalist = new List<KeyValuePair<string, int>>();
                var retestdatalist = new List<KeyValuePair<string, int>>();
                var fytestdatalist = new List<KeyValuePair<string, int>>();

                var pvmlist = ProjectViewModels.RetrieveOneProject(ProjectKey);
                if (pvmlist.Count == 0) { return View(); }
                var pvm = pvmlist[0];
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

                    if(piedatadict.Count > 0)
                    {
                        var namevaluepair = "";
                        var piedatadict_tmp = piedatadict.OrderByDescending(x => x.Value);
                        foreach(var item in piedatadict_tmp)
                        {
                            if(item.Value > 0)
                            {
                                namevaluepair = namevaluepair + "{ name:'" + item.Key + "',y:" + item.Value.ToString() + "},";
                            }
                        }
                        namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                        var reurl = "window.location.href = '/Project/ProjectTestDataDetail?ProjectKey=" + ProjectKey + "'" + "+'&FM='+this.name";
                        reurl += "+'&StartDate=" + ViewBag.sDate + "&EndDate=" + ViewBag.eDate + "&Type=FirstFailure'";
                        var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart4FF.xml"));
                        ViewBag.fchartscript = tempscript.Replace("#ElementID#", "ffailurepie")
                            .Replace("#Title#", "First Failure")
                            .Replace("#SERIESNAME#", "FFailure")
                            .Replace("#REDIRECTURL#", reurl)
                            .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                    }
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
                    if (piedatadict.Count > 0)
                    {
                        var namevaluepair = "";
                        var piedatadict_tmp = piedatadict.OrderByDescending(x => x.Value);
                        foreach (var item in piedatadict_tmp)
                        {
                            if (item.Value > 0)
                            {
                                namevaluepair = namevaluepair + "{ name:'" + item.Key + "',y:" + item.Value.ToString() + "},";
                            }
                        }
                        namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);
                        var reurl = "window.location.href = '/Project/ProjectSNTestDataDetail?ProjectKey=" + ProjectKey + "'" + "+'&FM='+this.name";
                        reurl += "+'&StartDate=" + ViewBag.sDate + "&EndDate=" + ViewBag.eDate + "&Type=SNFailure'";
                        var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart4FF.xml"));
                        ViewBag.rchartscript = tempscript.Replace("#ElementID#", "rfailurepie")
                            .Replace("#Title#", "SN Trace Failure")
                            .Replace("#SERIESNAME#", "SNFailure")
                            .Replace("#REDIRECTURL#", reurl)
                            .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                    }
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

                    if(piedatadict.Count > 0)
                    {
                        var namevaluepair = "";
                        var piedatadict_tmp = piedatadict.OrderByDescending(x => x.Value);
                        foreach(var item in piedatadict_tmp)
                        {
                            if(item.Value > 0)
                            {
                                namevaluepair = namevaluepair + "{ name:'" + item.Key + "',y:" + item.Value.ToString() + "},";
                            }
                        }
                        namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);
                        var reurl = "window.location.href = '/Project/ProjectTestDataDetail?ProjectKey=" + ProjectKey + "'" + "+'&FM='+this.name";
                        reurl += "+'&StartDate=" + ViewBag.sDate + "&EndDate=" + ViewBag.eDate + "&Type=FinalFailure'";
                        var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart4FF.xml"));
                        ViewBag.fychartscript = tempscript.Replace("#ElementID#", "fyfailurepie")
                            .Replace("#Title#", "Final Failure")
                            .Replace("#SERIESNAME#", "RFailure")
                            .Replace("#REDIRECTURL#", reurl)
                            .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                    }
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
                ViewBag.PJKey = ProjectKey;

                var list1 = IssueViewModels.RetrieveIssueTypeByProjectKey(ProjectKey, Resolute.Pending, ISSUETP.RMA, this);
                var list2 = IssueViewModels.RetrieveIssueTypeByProjectKey(ProjectKey, Resolute.Working, ISSUETP.RMA, this);
                var list3 = IssueViewModels.RetrieveIssueTypeByProjectKey(ProjectKey, Resolute.Done, ISSUETP.RMA, this);
                list1.AddRange(list2);
                list1.AddRange(list3);
                return View(list1);
            }
            return View();
        }

        public ActionResult ProjectRMAStatus(string ProjectKey, string sDate= "", string eDate = "")
        {
            ViewBag.pKey = null;
            ViewBag.sDate = string.IsNullOrEmpty(sDate) ? DateTime.Now.AddMonths(-6).ToString("yyyy-MM-dd") : sDate;
            ViewBag.eDate = string.IsNullOrEmpty(eDate) ? DateTime.Now.ToString("yyyy-MM-dd") : eDate;
            if (ProjectKey != null)
            {
                var list1 = IssueViewModels.NRetrieveRMAByProjectKey(ProjectKey, ViewBag.sDate + " 00:00:00", ViewBag.eDate + " 23:59:59", ISSUETP.RMA, this);
                ViewBag.pKey = ProjectKey;
                ViewBag.data = list1;
                var piedatadict = new Dictionary<string, int>();
                foreach (var item in list1)
                {
                    var rmafailurecode = item.Value.RMAFailureCode.ToLower().Trim();
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

                if(piedatadict.Count > 0)
                {
                    var namevaluepair = "";
                    var piedatadict_tmp = piedatadict.OrderByDescending(x => x.Value);
                    foreach(var item in piedatadict_tmp)
                    {
                        namevaluepair = namevaluepair + "{ name:'" + item.Key + "',y:" + item.Value.ToString() + "},";
                    }
                    namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                    var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart.xml"));
                    ViewBag.chartscript = tempscript.Replace("#ElementID#", "failurepie")
                        .Replace("#Title#", ProjectKey + " RMA Realtime Failure")
                        .Replace("#SERIESNAME#", "Failure")
                        .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                }
            }
            return View();
        }

        public ActionResult ProjectReliability(string ProjectKey, string sDate = "", string eDate = "")
        {
            //var checkresult = CheckLoginAndPermit(Request, "Project", "ProjectIssues");
            //if (checkresult.GetType() != (new Boolean()).GetType())
            //{
            //    return (ActionResult)checkresult;
            //}

            ViewBag.pKey = null;
            ViewBag.sDate = string.IsNullOrEmpty(sDate) ? DateTime.Now.AddMonths(-6).ToString("yyyy-MM-dd") : sDate;
            ViewBag.eDate = string.IsNullOrEmpty(eDate) ? DateTime.Now.ToString("yyyy-MM-dd") : eDate;
            if (ProjectKey != null)
            {
                var list1 = IssueViewModels.NRetrieveRelByProjectKey(ProjectKey, ViewBag.sDate + " 00:00:00", ViewBag.eDate + " 23:59:59", ISSUETP.Rel, this);
                ViewBag.pKey = ProjectKey;
                ViewBag.data = list1;

                var piedatadict = new Dictionary<string, int>();
                foreach (var item in list1)
                {
                    var rmafailurecode = item.Value.FVCode.ToLower().Trim();
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
                if (piedatadict.Count > 0)
                {
                    var namevaluepair = "";
                    var piedatadict_tmp = piedatadict.OrderByDescending(x => x.Value);
                    foreach (var item in piedatadict_tmp)
                    {
                        namevaluepair = namevaluepair + "{ name:'" + item.Key + "',y:" + item.Value.ToString() + "},";
                    }
                    namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                    var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart.xml"));
                    ViewBag.chartscript = tempscript.Replace("#ElementID#", "failurepie")
                        .Replace("#Title#", "Reliability Failure")
                        .Replace("#SERIESNAME#", "Failure")
                        .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                }
            }
            return View();
        }

        public ActionResult ProjectOBA(string ProjectKey, string sDate="", string eDate="")
        {
            ViewBag.pKey = null;
            ViewBag.sDate = string.IsNullOrEmpty(sDate) ?DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd"): sDate;
            ViewBag.eDate = string.IsNullOrEmpty(eDate) ? DateTime.Now.ToString("yyyy-MM-dd") : eDate;
            if (ProjectKey != null)
            {
                ViewBag.pKey = ProjectKey;
                ViewBag.data = IssueViewModels.NRetrieveOBAByProjectKey(ProjectKey, ViewBag.sDate + " 00:00:00", ViewBag.eDate + " 23:59:59", ISSUETP.OBA, this);
            }
            return View();
        }

        public ActionResult ProjectIQC(string ProjectKey, string sDate="", string eDate="")
        {
            ViewBag.pKey = null;
            ViewBag.sDate = string.IsNullOrEmpty(sDate) ? DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd") : sDate;
            ViewBag.eDate = string.IsNullOrEmpty(eDate) ? DateTime.Now.ToString("yyyy-MM-dd") : eDate;
            if (ProjectKey != null)
            {
                ViewBag.pKey = ProjectKey;
                ViewBag.data = IssueViewModels.NRetrieveOtherByProjectKey(ProjectKey, ViewBag.sDate + " 00:00:00", ViewBag.eDate + " 23:59:59", ISSUETP.IQE, this);
            }
            return View();
        }

        public ActionResult ProjectQuality(string ProjectKey, string sDate="", string eDate="")
        {
            ViewBag.pKey = null;
            ViewBag.sDate = string.IsNullOrEmpty(sDate) ? DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd") : sDate;
            ViewBag.eDate = string.IsNullOrEmpty(eDate) ? DateTime.Now.ToString("yyyy-MM-dd") : eDate;
            if (ProjectKey != null)
            {
                ViewBag.pKey = ProjectKey;
                ViewBag.data = IssueViewModels.NRetrieveQualityByProjectKey(ProjectKey, ViewBag.sDate + " 00:00:00", ViewBag.eDate + " 23:59:59", ISSUETP.Quality, this);
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
                ViewBag.PJKey = ProjectKey;
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
                ViewBag.PJKey = ProjectKey;
                ViewBag.sDate = Convert.ToDateTime(StartDate).ToString("yyyy-MM-dd");
                ViewBag.eDate = Convert.ToDateTime(EndDate).ToString("yyyy-MM-dd");
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
                ViewBag.PJKey = ProjectKey;

                ViewBag.StartDate = VStartDate;
                ViewBag.EndDate = VEndDate;

                var edate = DateTime.Parse(DateTime.Parse(EndDate).AddDays(1).ToString("yyyy-MM-dd") + " 07:30:00");
                var sdate = edate.AddDays(-1);
                ViewBag.sDate = Convert.ToDateTime(sdate).ToString("yyyy-MM-dd");
                ViewBag.eDate = Convert.ToDateTime(edate).ToString("yyyy-MM-dd");
                var firstdatalist = new List<KeyValuePair<string, int>>();
                var retestdatalist = new List<KeyValuePair<string, int>>();
                var fytestdatalist = new List<KeyValuePair<string, int>>();

                var pvmlist = ProjectViewModels.RetrieveOneProject(ProjectKey);
                if (pvmlist.Count == 0) { return View(); }
                var pvm = pvmlist[0];
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

                    if (piedatadict.Count > 0)
                    {
                        var namevaluepair = "";
                        var piedatadict_tmp = piedatadict.OrderByDescending(x => x.Value);
                        foreach(var item in piedatadict_tmp)
                        {
                            if(item.Value > 0)
                            {
                                namevaluepair = namevaluepair + "{ name:'" + item.Key + "',y:" + item.Value.ToString() + "},";
                            }
                        }
                        namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                        var reurl = "window.location.href = '/Project/ProjectTestDataDetail?ProjectKey=" + ProjectKey + "'" + "+'&FM='+this.name";
                        reurl += "+'&StartDate=" + ViewBag.sDate + "&EndDate=" + ViewBag.eDate + "&Type=FirstFailure'";

                        var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart4FF.xml"));
                        ViewBag.fchartscript = tempscript.Replace("#ElementID#", "ffailurepie")
                            .Replace("#Title#", "First Failure")
                            .Replace("#SERIESNAME#", "FFailure")
                            .Replace("#REDIRECTURL#", reurl)
                            .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                    }

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

                    if(piedatadict.Count > 0)
                    {
                        var namevaluepair = "";
                        var piedatadict_tmp = piedatadict.OrderByDescending(x => x.Value);
                        foreach(var item in piedatadict_tmp)
                        {
                            if(item.Value > 0)
                            {
                                namevaluepair = namevaluepair + "{ name:'" + item.Key + "',y:" + item.Value.ToString() + "},";
                            }
                        }
                        namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                        var reurl = "window.location.href = '/Project/ProjectSNTestDataDetail?ProjectKey=" + ProjectKey + "'" + "+'&FM='+this.name";
                        reurl += "+'&StartDate=" + ViewBag.sDate + "&EndDate=" + ViewBag.eDate + "&Type=SNFailure'";

                        var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart4FF.xml"));
                        ViewBag.rchartscript = tempscript.Replace("#ElementID#", "rfailurepie")
                            .Replace("#Title#", "SN Trace Failure")
                            .Replace("#SERIESNAME#", "SNFailure")
                            .Replace("#REDIRECTURL#", reurl)
                            .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                    }
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

                    if (piedatadict.Count > 0)
                    {
                        var namevaluepair = "";
                        var piedatadict_tmp = piedatadict.OrderByDescending(x => x.Value);
                        foreach (var item in piedatadict_tmp)
                        {
                            if (item.Value > 0)
                            {
                                namevaluepair = namevaluepair + "{ name:'" + item.Key + "',y:" + item.Value.ToString() + "},";
                            }
                        }
                        namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                        var reurl = "window.location.href = '/Project/ProjectTestDataDetail?ProjectKey=" + ProjectKey + "'" + "+'&FM='+this.name";
                        reurl += "+'&StartDate=" + ViewBag.sDate + "&EndDate=" + ViewBag.eDate + "&Type=FinalFailure'";

                        var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart4FF.xml"));
                        ViewBag.fychartscript = tempscript.Replace("#ElementID#", "fyfailurepie")
                            .Replace("#Title#", "Final Failure")
                            .Replace("#SERIESNAME#", "RFailure")
                            .Replace("#REDIRECTURL#", reurl)
                            .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                    }
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
                ViewBag.PJKey = ProjectKey;

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

                ViewBag.sDate = Convert.ToDateTime(sdate).ToString("yyyy-MM-dd");
                ViewBag.eDate = Convert.ToDateTime(edate).ToString("yyyy-MM-dd");

                var firstdatalist = new List<KeyValuePair<string, int>>();
                var retestdatalist = new List<KeyValuePair<string, int>>();
                var fytestdatalist = new List<KeyValuePair<string, int>>();

                var pvmlist = ProjectViewModels.RetrieveOneProject(ProjectKey);
                if (pvmlist.Count == 0) { return View(); }
                var pvm = pvmlist[0];

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

                    if (piedatadict.Count > 0)
                    {
                        var namevaluepair = "";
                        var piedatadict_tmp = piedatadict.OrderByDescending(x => x.Value);
                        foreach (var item in piedatadict_tmp)
                        {
                            if (item.Value > 0)
                            {
                                namevaluepair = namevaluepair + "{ name:'" + item.Key + "',y:" + item.Value.ToString() + "},";
                            }
                        }

                        namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                        var reurl = "window.location.href = '/Project/ProjectTestDataDetail?ProjectKey=" + ProjectKey + "'" + "+'&FM='+this.name";
                        reurl += "+'&StartDate=" + ViewBag.sDate + "&EndDate=" + ViewBag.eDate + "&Type=FirstFailure'";
                        
                        var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart4FF.xml"));
                        ViewBag.fchartscript = tempscript.Replace("#ElementID#", "ffailurepie")
                            .Replace("#Title#", "First Failure")
                            .Replace("#SERIESNAME#", "FFailure")
                            .Replace("#REDIRECTURL#", reurl)
                            .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                    }
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
                    if (piedatadict.Count > 0)
                    {
                        var namevaluepair = "";
                        var piedatadict_tmp = piedatadict.OrderByDescending(x => x.Value);
                        foreach (var item in piedatadict_tmp)
                        {
                            if (item.Value > 0)
                            {
                                namevaluepair = namevaluepair + "{ name:'" + item.Key + "',y:" + item.Value.ToString() + "},";
                            }
                        }
                        namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                        var reurl = "window.location.href = '/Project/ProjectSNTestDataDetail?ProjectKey=" + ProjectKey + "'" + "+'&FM='+this.name";
                        reurl += "+'&StartDate=" + ViewBag.sDate + "&EndDate=" + ViewBag.eDate + "&Type=SNFailure'";
                        
                        var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart4FF.xml"));
                        ViewBag.rchartscript = tempscript.Replace("#ElementID#", "rfailurepie")
                            .Replace("#Title#", "SN Trace Failure")
                            .Replace("#SERIESNAME#", "SNFailure")
                            .Replace("#REDIRECTURL#", reurl)
                            .Replace("#NAMEVALUEPAIRS#", namevaluepair);

                    }
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

                    if (piedatadict.Count > 0)
                    {
                        var namevaluepair = "";
                        var piedatadict_tmp = piedatadict.OrderByDescending(x => x.Value);
                        foreach (var item in piedatadict_tmp)
                        {
                            if (item.Value > 0)
                            {
                                namevaluepair = namevaluepair + "{ name:'" + item.Key + "',y:" + item.Value.ToString() + "},";
                            }
                        }

                        namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                        var reurl = "window.location.href = '/Project/ProjectTestDataDetail?ProjectKey=" + ProjectKey + "'" + "+'&FM='+this.name";
                        reurl += "+'&StartDate=" + ViewBag.sDate + "&EndDate=" + ViewBag.eDate + "&Type=FinalFailure'";

                        var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart4FF.xml"));
                        ViewBag.fychartscript = tempscript.Replace("#ElementID#", "fyfailurepie")
                            .Replace("#Title#", "Final Failure")
                            .Replace("#SERIESNAME#", "RFailure")
                            .Replace("#REDIRECTURL#", reurl)
                            .Replace("#NAMEVALUEPAIRS#", namevaluepair);

                    }
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
            var pvmlist = ProjectViewModels.RetrieveOneProject(ProjectKey);
            if (pvmlist.Count == 0) { return; }
            var pvm = pvmlist[0];

            var vmlist = ProjectYieldViewModule.GetYieldByBRNum(ProjectKey, BRNUM, pvm, HttpContext.Cache, BRType);

            ViewBag.PJKey = ProjectKey;

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
                ViewBag.PJKey = ProjectKey;

                ViewBag.pjkey = ProjectKey;
                ProjectBRTypeYield(ProjectKey, BRNUM, YIELDTYPE.BR);
                return View();
            }

            return View();
        }

        public ActionResult ProjectPNYield(string ProjectKey, string BRNUM)
        {
            if (!string.IsNullOrEmpty(ProjectKey) && !string.IsNullOrEmpty(BRNUM))
            {
                ViewBag.PJKey = ProjectKey;

                ViewBag.pjkey = ProjectKey;
                ProjectBRTypeYield(ProjectKey, BRNUM, YIELDTYPE.PN);
                return View();
            }

            return View();
        }


        public ActionResult ProjectJOYield(string ProjectKey, string BRNUM)
        {
            if (!string.IsNullOrEmpty(ProjectKey) && !string.IsNullOrEmpty(BRNUM))
            {
                ViewBag.PJKey = ProjectKey;

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
                ViewBag.PJKey = ProjectKey;

                ViewBag.BRNUM = CurrentBR;
                ViewBag.BRType = BRType;

                var pvmlist = ProjectViewModels.RetrieveOneProject(ProjectKey);
                if (pvmlist.Count == 0) { return View(); }
                var pvm = pvmlist[0];

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
                        if (piedatadict.Count > 0)
                        {
                            var namevaluepair = "";
                            var piedatadict_tmp = piedatadict.OrderByDescending(x => x.Value);
                            foreach (var item in piedatadict_tmp)
                            {
                                if (item.Value > 0)
                                {
                                    namevaluepair = namevaluepair + "{ name:'" + item.Key + "',y:" + item.Value.ToString() + "},";
                                }
                            }

                            namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                            var reurl = "window.location.href = '/Project/ProjectBRTestDataDetail?ProjectKey=" + ProjectKey + "'" + "+'&FM='+this.name";
                            reurl += "+'&BRNUM=" + ViewBag.BRNUM + "&BRType=" + ViewBag.BRType + "&Type=FirstFailure'";

                            var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart4FF.xml"));
                            ViewBag.fchartscript = tempscript.Replace("#ElementID#", "ffailurepie")
                                .Replace("#Title#", "First Failure")
                                .Replace("#SERIESNAME#", "FFailure")
                                .Replace("#REDIRECTURL#", reurl)
                                .Replace("#NAMEVALUEPAIRS#", namevaluepair);

                        }
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
                        if (piedatadict.Count > 0)
                        {
                            var namevaluepair = "";
                            var piedatadict_tmp = piedatadict.OrderByDescending(x => x.Value);
                            foreach (var item in piedatadict_tmp)
                            {
                                if (item.Value > 0)
                                {
                                    namevaluepair = namevaluepair + "{ name:'" + item.Key + "',y:" + item.Value.ToString() + "},";
                                }
                            }

                            namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                            var reurl = "window.location.href = '/Project/ProjectBRTestDataDetail?ProjectKey=" + ProjectKey + "'" + "+'&FM='+this.name";
                            reurl += "+'&BRNUM=" + ViewBag.BRNUM + "&BRType=" + ViewBag.BRType + "&Type=SNFailure'";

                            var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart4FF.xml"));
                            ViewBag.rchartscript = tempscript.Replace("#ElementID#", "rfailurepie")
                                .Replace("#Title#", "SN Trace Failure")
                                .Replace("#SERIESNAME#", "SNFailure")
                                .Replace("#REDIRECTURL#", reurl)
                                .Replace("#NAMEVALUEPAIRS#", namevaluepair);

                        }
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
                        if (piedatadict.Count > 0)
                        {
                            var namevaluepair = "";
                            var piedatadict_tmp = piedatadict.OrderByDescending(x => x.Value);
                            foreach (var item in piedatadict_tmp)
                            {
                                if (item.Value > 0)
                                {
                                    namevaluepair = namevaluepair + "{ name:'" + item.Key + "',y:" + item.Value.ToString() + "},";
                                }
                            }

                            namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                            var reurl = "window.location.href = '/Project/ProjectBRTestDataDetail?ProjectKey=" + ProjectKey + "'" + "+'&FM='+this.name";
                            reurl += "+'&BRNUM=" + ViewBag.BRNUM + "&BRType=" + ViewBag.BRType + "&Type=FinalFailure'";

                            var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart4FF.xml"));
                            ViewBag.fychartscript = tempscript.Replace("#ElementID#", "fyfailurepie")
                                .Replace("#Title#", "Final Failure")
                                .Replace("#SERIESNAME#", "RFailure")
                                .Replace("#REDIRECTURL#", reurl)
                                .Replace("#NAMEVALUEPAIRS#", namevaluepair);

                        }
                    }

                    if (firstdatalist.Count > 0)
                    {
                        firsttestparetofun(firstdatalist, ProjectKey,null,null, "BR");
                    }

                    if (retestdatalist.Count > 0)
                    {
                        retestparetofun(retestdatalist, ProjectKey, null, null, "BR");
                    }

                    if (fytestdatalist.Count > 0)
                    {
                        fytparetofun(fytestdatalist, ProjectKey, null, null, "BR");
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
                ViewBag.sDate = Convert.ToDateTime(StartDate).ToString("yyyy-MM-dd");
                ViewBag.eDate = Convert.ToDateTime(EndDate).ToString("yyyy-MM-dd");
                ViewBag.PJKey = ProjectKey;

                ViewBag.pjkey = ProjectKey;

                var pvmlist = ProjectViewModels.RetrieveOneProject(ProjectKey);
                if (pvmlist.Count == 0) { return View(); }
                var pvm = pvmlist[0];

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

        private void firsttestparetofun(List<KeyValuePair<string, int>> firstdatalist, string ProjectKey, string StartDate, string EndDate, string type = "")
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
                var reurl = "";
                if (string.Compare(type, "BR", true) == 0)
                {
                    reurl = "window.location.href = '/Project/ProjectBRTestDataDetail?ProjectKey=" + ProjectKey + "'" + "+'&FM='+this.category";
                    reurl += "+'&BRNUM=" + ViewBag.BRNUM + "&BRType=" + ViewBag.BRType + "&Type=FirstFailure'";
                }
                else
                {
                    reurl = "window.location.href = '/Project/ProjectTestDataDetail?ProjectKey=" + ProjectKey + "'" + "+'&FM='+this.category";
                    reurl += "+'&StartDate=" + ViewBag.sDate + "&EndDate=" + ViewBag.eDate + "&Type=FirstFailure'";
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

        private void retestparetofun(List<KeyValuePair<string, int>> retestdatalist, string ProjectKey, string StartDate, string EndDate, string type="")
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

                var reurl = "";
                if (string.Compare(type, "BR", true) == 0)
                {
                    reurl = "window.location.href = '/Project/ProjectBRTestDataDetail?ProjectKey=" + ProjectKey + "'" + "+'&FM='+this.category";
                    reurl += "+'&BRNUM=" + ViewBag.BRNUM + "&BRType=" + ViewBag.BRType + "&Type=SNFailure'";
                }
                else
                {
                    reurl = "window.location.href = '/Project/ProjectSNTestDataDetail?ProjectKey=" + ProjectKey + "'" + "+'&FM='+this.category";
                    reurl += "+'&StartDate=" + ViewBag.sDate + "&EndDate=" + ViewBag.eDate + "&Type=SNFailure'";
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

        private void fytparetofun(List<KeyValuePair<string, int>> retestdatalist, string ProjectKey,string StartDate,string EndDate, string type="")
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

                var reurl = "";
                if (string.Compare(type, "BR", true) == 0)
                {
                    reurl = "window.location.href = '/Project/ProjectBRTestDataDetail?ProjectKey=" + ProjectKey + "'" + "+'&FM='+this.category";
                    reurl += "+'&BRNUM=" + ViewBag.BRNUM + "&BRType=" + ViewBag.BRType + "&Type=FinalFailure'";
                }
                else
                {
                    reurl = "window.location.href = '/Project/ProjectTestDataDetail?ProjectKey=" + ProjectKey + "'" + "+'&FM='+this.category";
                    reurl += "+'&StartDate=" + ViewBag.sDate + "&EndDate=" + ViewBag.eDate + "&Type=FinalFailure'";
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
                ViewBag.PJKey = ProjectKey;

                ViewBag.StartDate = StartDate;
                ViewBag.EndDate = EndDate;
                ViewBag.sDate = Convert.ToDateTime(StartDate).ToString("yyyy-MM-dd");
                ViewBag.eDate = Convert.ToDateTime(EndDate).ToString("yyyy-MM-dd");

                var firstdatalist = new List<KeyValuePair<string, int>>();
                var retestdatalist = new List<KeyValuePair<string, int>>();
                var fytestdatalist = new List<KeyValuePair<string, int>>();

                var pvmlist = ProjectViewModels.RetrieveOneProject(ProjectKey);
                if (pvmlist.Count == 0) { return View(); }
                var pvm = pvmlist[0];

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
                    if (piedatadict.Count > 0)
                    {
                        var namevaluepair = "";
                        var piedatadict_tmp = piedatadict.OrderByDescending(x => x.Value);
                        foreach (var item in piedatadict_tmp)
                        {
                            if (item.Value > 0)
                            {
                                namevaluepair = namevaluepair + "{ name:'" + item.Key + "',y:" + item.Value.ToString() + "},";
                            }
                        }

                        namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                        var reurl = "window.location.href = '/Project/ProjectTestDataDetail?ProjectKey=" + ProjectKey + "'" + "+'&FM='+this.name";
                        reurl += "+'&StartDate=" + ViewBag.sDate + "&EndDate=" + ViewBag.eDate + "&Type=FirstFailure'";


                        var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart4FF.xml"));
                        ViewBag.fchartscript = tempscript.Replace("#ElementID#", "ffailurepie")
                            .Replace("#Title#", "First Failure")
                            .Replace("#SERIESNAME#", "FFailure")
                            .Replace("#REDIRECTURL#", reurl)
                            .Replace("#NAMEVALUEPAIRS#", namevaluepair);

                    }
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
                    if (piedatadict.Count > 0)
                    {
                        var namevaluepair = "";
                        var piedatadict_tmp = piedatadict.OrderByDescending(x => x.Value);
                        foreach (var item in piedatadict_tmp)
                        {
                            if (item.Value > 0)
                            {
                                namevaluepair = namevaluepair + "{ name:'" + item.Key + "',y:" + item.Value.ToString() + "},";
                            }
                        }

                        namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                        var reurl = "window.location.href = '/Project/ProjectSNTestDataDetail?ProjectKey=" + ProjectKey + "'" + "+'&FM='+this.name";
                        reurl += "+'&StartDate=" + ViewBag.sDate + "&EndDate=" + ViewBag.eDate + "&Type=SNFailure'";
                        
                        var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart4FF.xml"));
                        ViewBag.rchartscript = tempscript.Replace("#ElementID#", "rfailurepie")
                            .Replace("#Title#", "SN Trace Failure")
                            .Replace("#SERIESNAME#", "SNFailure")
                            .Replace("#REDIRECTURL#", reurl)
                            .Replace("#NAMEVALUEPAIRS#", namevaluepair);

                    }
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
                    if (piedatadict.Count > 0)
                    {
                        var namevaluepair = "";
                        var piedatadict_tmp = piedatadict.OrderByDescending(x => x.Value);
                        foreach (var item in piedatadict_tmp)
                        {
                            if (item.Value > 0)
                            {
                                namevaluepair = namevaluepair + "{ name:'" + item.Key + "',y:" + item.Value.ToString() + "},";
                            }
                        }

                        namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                        var reurl = "window.location.href = '/Project/ProjectTestDataDetail?ProjectKey=" + ProjectKey + "'" + "+'&FM='+this.name";
                        reurl += "+'&StartDate=" + ViewBag.sDate + "&EndDate=" + ViewBag.eDate + "&Type=FinalFailure'";

                        var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart4FF.xml"));
                        ViewBag.fychartscript = tempscript.Replace("#ElementID#", "fyfailurepie")
                            .Replace("#Title#", "Final Failure")
                            .Replace("#SERIESNAME#", "RFailure")
                            .Replace("#REDIRECTURL#", reurl)
                            .Replace("#NAMEVALUEPAIRS#", namevaluepair);

                    }
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
                var AllPJMember = ProjectViewModels.RetrieveOneProjectWithClose(vm[0].ProjectKey)[0].AllPJMember;

                ViewBag.PJKey = vm[0].ProjectKey;

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
                ViewBag.AllUserList = "[\"" + string.Join("\",\"", asilist.ToArray()) + "\"]";

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

            var asilist = UserViewModels.RetrieveAllUser();
            ViewBag.AllUserList = "[\"" + string.Join("\",\"", asilist.ToArray()) + "\"]";

            if (!string.IsNullOrEmpty(key))
            {
                var vm = ProjectErrorViewModels.RetrieveErrorByErrorKey(key, this);

                ViewBag.PJKey = vm[0].ProjectKey;

                var AllPJMember = ProjectViewModels.RetrieveOneProjectWithClose(vm[0].ProjectKey)[0].AllPJMember;
                var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
                if (AllPJMember.ToUpper().Contains(updater.ToUpper()))
                {
                    ViewBag.assigee = true;
                }
                else
                {
                    ViewBag.assigee = false;
                }
                
                return View("UpdateProjectError", vm[0]);
            }

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
                var netcomputername = EmailUtility.RetrieveCurrentMachineName();
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
                var pjmems = ProjectViewModels.RetrieveOneProjectWithClose(updatevm[0].ProjectKey)[0].MemberList;
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

            var aid = IssueViewModels.GetUniqKey();

            if (!string.IsNullOrEmpty(Request.Form["analysetitle"]))
            {
                var com = new ErrorComments();
                com.Comment = Request.Form["analysetitle"];
                ProjectErrorViewModels.StoreErrorComment(vm.ErrorKey, com.dbComment, PJERRORCOMMENTTYPE.AnalyzeTitle, vm.Reporter, currenttime,aid);
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
                    ProjectErrorViewModels.StoreErrorComment(vm.ErrorKey, com.dbComment, PJERRORCOMMENTTYPE.FailureDetail, vm.Reporter, currenttime,aid);
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
                    ProjectErrorViewModels.StoreErrorComment(vm.ErrorKey, com.dbComment, PJERRORCOMMENTTYPE.Result, vm.Reporter, currenttime,aid);
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
                    ProjectErrorViewModels.StoreErrorComment(vm.ErrorKey, com.dbComment, PJERRORCOMMENTTYPE.RootCause, vm.Reporter, currenttime,aid);
                    analyseinputed = true;
                }
            }

            if (analyseinputed)
            {
                if (string.IsNullOrEmpty(failurestr))
                {
                    var com1 = new ErrorComments();
                    com1.Comment = "<p>To Be Edit</p>";
                    ProjectErrorViewModels.StoreErrorComment(vm.ErrorKey, com1.dbComment, PJERRORCOMMENTTYPE.FailureDetail, vm.Reporter, currenttime,aid);
                }

                if (string.IsNullOrEmpty(resulutstr))
                {
                    var com1 = new ErrorComments();
                    com1.Comment = "<p>To Be Edit</p>";
                    ProjectErrorViewModels.StoreErrorComment(vm.ErrorKey, com1.dbComment, PJERRORCOMMENTTYPE.Result, vm.Reporter, currenttime,aid);
                }

                if (string.IsNullOrEmpty(rootcausestr))
                {
                    var com1 = new ErrorComments();
                    com1.Comment = "<p>To Be Edit</p>";
                    ProjectErrorViewModels.StoreErrorComment(vm.ErrorKey, com1.dbComment, PJERRORCOMMENTTYPE.RootCause, vm.Reporter, currenttime,aid);
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
                var AllPJMember = ProjectViewModels.RetrieveOneProjectWithClose(tempvm[0].ProjectKey)[0].AllPJMember;
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
                            if (!ret.ContainsKey(keyvalue[0].Trim()))
                            {
                                ret.Add(keyvalue[0].Trim(), Convert.ToDouble(keyvalue[1].Trim()));
                            }
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

            //foreach (var item in pjkeylist)
            {
                //var pjval = ProjectViewModels.RetrieveOneProject(item);
                //if (string.Compare(pjval.MonitorVcsel, "False", true) == 0)
                //{
                //    continue;
                //}
                //var warningyield = Convert.ToDouble(pjval.VcselWarningYield) * 0.01;

                //var waferlist = BITestData.RetrieveAllWafer(item);

                var waferlist = BITestData.RetrieveAllWaferNoPJ();

                foreach (var w in waferlist)
                {
                    if (exceptdict.ContainsKey(w))
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(w))
                    {
                        continue;
                    }

                    var yield = ProjectBIYieldViewModule.GetYieldByWaferNoPJ(w);
                    if (yield.CorrectLastYield > 0.1 && yield.CorrectLastYield < 0.96)
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

                        if (alldict.Count < 100)
                        {
                            continue;
                        }

                        hascontent = true;
                        var tempcontent1 = "Warning: the corrective yield of  wafer " + w + " is " + (yield.CorrectLastYield * 100.0).ToString("0.00") + "% \r\n\r\n"
                            + " Total Input: " + alldict.Count.ToString() + " ,Failed Modules: " + errdict.Count.ToString() + " ,Reviewed Modules: " + cordict.Count.ToString() + " \r\n\r\n";

                        if (errdict.Count == cordict.Count)
                        {
                            tempcontent1 = "[Reviewed ] " + tempcontent1;
                        }

                        if (!emailed.ContainsKey(w))
                        {
                            tempcontent1 = "[new ] " + tempcontent1;
                            emailed.Add(w, yield.CorrectLastYield);
                        }
                        else
                        {
                            var lasttimeyield = Convert.ToDouble(emailed[w].ToString("0.0"));
                            var currentyield = Convert.ToDouble(yield.CorrectLastYield.ToString("0.0"));
                            if (currentyield > lasttimeyield)
                            {
                                tempcontent1 = "[^] " + tempcontent1;
                            }
                            else if (currentyield < lasttimeyield)
                            {
                                tempcontent1 = "[v] " + tempcontent1;
                            }
                            emailed[w] = yield.CorrectLastYield;
                        }

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

                var netcomputername = EmailUtility.RetrieveCurrentMachineName();


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
            var fn = "VCSEL_WAFER_SN" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
            var logvcsel = Server.MapPath("~/userfiles") + "\\" + fn;

            if (!System.IO.File.Exists(logvcsel))
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
            var netcomputername = EmailUtility.RetrieveCurrentMachineName();
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

        private void SendWeeklyReportNotice()
        {
            var currenttime = DateTime.Now;
            if (currenttime.DayOfWeek == DayOfWeek.Thursday && DateTime.Now.Hour > 11)
            {
                var filename = "log_weelyreport_" + DateTime.Now.ToString("yyyyMMdd");
                var wholefilename = Server.MapPath("~/userfiles") + "\\" + filename;
                if (!System.IO.File.Exists(wholefilename))
                {
                    var hello = "hello";
                    System.IO.File.WriteAllText(wholefilename, hello);

                    var updater = "WINDY.JU@FINISAR.COM";
                    var usergroup = UserGroupVM.RetreiveUserGroup(updater, UserGroupType.WorkGroup);
                    var userlist = usergroup.Split(new string[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries);
                    var greeting = "Hi, <UserName>";
                    var description = "Today is " + DateTime.Now.ToString("MM/dd/yyyy") + ", ThursDay, Please fill out your weekly report in WUXI ENGINEERING SYSTEM. ";

                    var url = getActionUrl("User", "WeeklyReportList");
                    var content = EmailUtility.CreateTableHtml(greeting, description, url, null);
                    //userlist = new string[] { "Yan.Shi@FINISAR.COM" };
                    foreach (var user in userlist)
                    {
                        var towholist = new List<string>();
                        towholist.Add(user);
                        var towho = user.Split(new char[] { '@' })[0];
                        var con = content.Replace("<UserName>", towho);
                        EmailUtility.SendEmail(this, "Weekly Report Alarm -- " + DateTime.Now.ToString("MM/dd/yyyy"), towholist, con, true);
                    }
                }
            }
        }

        private string getActionUrl(string module, string act, Dictionary<string, string> param = null)
        {
            var routevalue = new RouteValueDictionary();
            if(param != null)
            {
                foreach (var item in param)
                {
                    routevalue.Add(item.Key, item.Value);
                }
            }
            //send validate email
            string scheme = this.Url.RequestContext.HttpContext.Request.Url.Scheme;
            string validatestr = this.Url.Action(act, module, routevalue, scheme);

            var netcomputername = EmailUtility.RetrieveCurrentMachineName();
            validatestr = validatestr.Replace("//localhost", "//" + netcomputername);

            return validatestr;
        }

        private void SendTaskNotice()
        {
            var filename = "log" + DateTime.Now.ToString("yyyy-MM-dd");
            var wholefilename = Server.MapPath("~/userfiles") + "\\" + filename;
            if (!System.IO.File.Exists(wholefilename))
            {
                //var hello = "hello";
                //System.IO.File.WriteAllText(wholefilename, hello);
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

                        var netcomputername = EmailUtility.RetrieveCurrentMachineName();
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
                        var netcomputername = EmailUtility.RetrieveCurrentMachineName();
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

                        var netcomputername = EmailUtility.RetrieveCurrentMachineName();
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

                    var netcomputername = EmailUtility.RetrieveCurrentMachineName();
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

        private void SendTaskAlertEmail(string tasktype)
        {
            try
            {
                var faissues = IssueViewModels.Retrieve_Alert_TaskByProjectKey(tasktype);

                foreach (var item in faissues)
                { item.UpdateAlertEmailDate(); }

                foreach (var item in faissues)
                {
                    var routevalue = new RouteValueDictionary();
                    routevalue.Add("issuekey", item.IssueKey);
                    //send validate email
                    string scheme = this.Url.RequestContext.HttpContext.Request.Url.Scheme;
                    string validatestr = this.Url.Action("UpdateIssue", "Issue", routevalue, scheme);

                    var netcomputername = EmailUtility.RetrieveCurrentMachineName();
                    validatestr = validatestr.Replace("//localhost", "//" + netcomputername);

                    var content = "Warning: your task - " + item.Summary + " is close to its Due Date :\r\n " + validatestr;

                    var toaddrs = new List<string>();
                    toaddrs.Add(item.Reporter);
                    toaddrs.Add(item.Assignee);
                    EmailUtility.SendEmail(this, "WUXI Engineering System", toaddrs, content);
                    new System.Threading.ManualResetEvent(false).WaitOne(200);
                }
            }
            catch (Exception ex) { }
        }

        private void SendIQETaskAlertEmail()
        {
            try
            {
                //phase 0
                var sDate_1 = DateTime.Now.AddDays(-11).ToString("yyyy-MM-dd 00:00:00");
                var eDate_1 = DateTime.Now.AddDays(-10).ToString("yyyy-MM-dd 23:59:59");
                var phase_0 = IssueViewModels.Retrieve_Alert_TaskByDate(ISSUETP.IQE, sDate_1, eDate_1);
                //phase 1
                sDate_1 = DateTime.Now.AddDays(-14).ToString("yyyy-MM-dd 00:00:00");
                eDate_1 = DateTime.Now.AddDays(-12).ToString("yyyy-MM-dd 23:59:59");
                var phase_1 = IssueViewModels.Retrieve_Alert_TaskByDate(ISSUETP.IQE, sDate_1, eDate_1);
                //phase 2
                sDate_1 = DateTime.Now.AddDays(-17).ToString("yyyy-MM-dd 00:00:00");
                eDate_1 = DateTime.Now.AddDays(-15).ToString("yyyy-MM-dd 23:59:59");
                var phase_2 = IssueViewModels.Retrieve_Alert_TaskByDate(ISSUETP.IQE, sDate_1, eDate_1);

                sendIQCMail(phase_0, 1);
                sendIQCMail(phase_1, 2);
                sendIQCMail(phase_2, 3);

                phase_0.AddRange(phase_1);
                phase_0.AddRange(phase_2);

                foreach (var item in phase_0)
                {
                    item.UpdateAlertEmailDate();
                }
            }
            catch (Exception ex) { }
        }

        private void sendIQCMail(List<IssueViewModels> data, int flag)
        {
            foreach (var item in data)
            {
                var routevalue = new RouteValueDictionary();
                routevalue.Add("issuekey", item.IssueKey);
                //send validate email
                string scheme = this.Url.RequestContext.HttpContext.Request.Url.Scheme;
                string validatestr = this.Url.Action("UpdateIssue", "Issue", routevalue, scheme);

                var netcomputername = EmailUtility.RetrieveCurrentMachineName();
                validatestr = validatestr.Replace("//localhost", "//" + netcomputername);

                var date_diff = 14 - (Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd")) - Convert.ToDateTime(item.ReportDate.ToString("yyyy-MM-dd"))).Days;
                var diff_str = "";
                if (date_diff < 0)
                {
                    diff_str = " Over: " + Math.Abs(date_diff);
                }
                else
                {
                    diff_str = " Left: " + date_diff;
                }

                var content = "Warning: your task - " + item.Summary + " is close to its Due Date( " + diff_str + " ) :\r\n " + validatestr;
                var toaddrs = new List<string>();
                toaddrs.Add(item.Reporter);
                toaddrs.Add(item.Assignee);
                if(flag == 1 || flag == 2)
                {
                    toaddrs.Add(item.RelativePeopleList[0]);
                    if(flag == 2)
                    {
                        toaddrs.Add(item.RelativePeopleList[1]);
                    }
                }
                else
                {
                    toaddrs.AddRange(item.RelativePeopleList);
                }
                EmailUtility.SendEmail(this, "WUXI Engineering System", toaddrs, content);
                new System.Threading.ManualResetEvent(false).WaitOne(200);
            }
        }

        private void heartbeatlog(string msg)
        {
            try
            {
                var filename = "log" + DateTime.Now.ToString("yyyy-MM-dd");
                var wholefilename = Server.MapPath("~/userfiles") + "\\" + filename;

                var content = "";
                if (System.IO.File.Exists(wholefilename))
                {
                    content = System.IO.File.ReadAllText(wholefilename);
                }
                content = content + msg + " @ " + DateTime.Now.ToString() + "\r\n";
                System.IO.File.WriteAllText(wholefilename, content);
            }
            catch (Exception ex)
            { }
        }

        private void sundaylog(string msg)
        {
            try
            {
                var filename = "sunday-report-log" + DateTime.Now.ToString("yyyy-MM-dd");
                var wholefilename = Server.MapPath("~/userfiles") + "\\" + filename;

                var content = "";
                if (System.IO.File.Exists(wholefilename))
                {
                    content = System.IO.File.ReadAllText(wholefilename);
                }
                content = content + msg + " @ " + DateTime.Now.ToString() + "\r\n";
                System.IO.File.WriteAllText(wholefilename, content);
            }
            catch (Exception ex)
            { }
        }

        public void SundayReport()
        {
            sundaylog("Sunday Report Start....");

            var times = 0;

            var glbcfg = CfgUtility.GetSysConfig(this);
            var vcselbgdzeropoint = DateTime.Parse(glbcfg["VCSELBIGDATAZEROPOINT"]);

            var vcselpninfo = VcselPNData.RetrieveVcselPNInfo();
            if (vcselpninfo.Count == 0)
            {
                sundaylog("Sunday Report End for vcsel pn is empty....");
                return;
            }

            var thismonth = DateTime.Now.Month;

            while (true)
            {
                if (!VcselMonthData.MonthDataExist(vcselbgdzeropoint) 
                    || (thismonth == vcselbgdzeropoint.Month && DateTime.Now.Year == vcselbgdzeropoint.Year)
                    || (thismonth-1 == vcselbgdzeropoint.Month && DateTime.Now.Year == vcselbgdzeropoint.Year))
                {
                    var currenttime = DateTime.Now;
                    if (currenttime.Hour > 18 || vcselbgdzeropoint > currenttime)
                    {
                        sundaylog("Sunday Report End for time....");
                        break;
                    }

                    sundaylog("start computer BURN IN " + vcselbgdzeropoint.ToString("yyyy-MM-dd HH:mm:ss") + " Monthly data");
                    VcselBGDVM.StartVcselBGDComputer(vcselbgdzeropoint, vcselpninfo,this);
                    vcselbgdzeropoint = vcselbgdzeropoint.AddMonths(1);
                }
                else
                {
                    var currenttime = DateTime.Now;
                    if (currenttime.Hour > 18 || vcselbgdzeropoint > currenttime)
                    {
                        sundaylog("Sunday Report End for time....");
                        break;
                    }

                    //EXIST SUCH DATA
                    sundaylog(vcselbgdzeropoint.ToString("yyyy-MM-dd HH:mm:ss")+" Monthly data exist");
                    vcselbgdzeropoint = vcselbgdzeropoint.AddMonths(1);
                    continue;
                }

                times = times + 1;
                if (times > 26)
                {
                    break;
                }
            }//end while

            times = 0;
            var htolbgdzeropoint = DateTime.Parse(glbcfg["HTOLBIGDATAZEROPOINT"]);

            VcselBGDVM.ClearHTOLDataWithPostBurnInFailure();

            while (true)
            {
                if (!VcselMonthData.MonthDataExist(htolbgdzeropoint,"HTOLMonthData")
                    || (thismonth == htolbgdzeropoint.Month && DateTime.Now.Year == htolbgdzeropoint.Year)
                    || (thismonth - 1 == htolbgdzeropoint.Month && DateTime.Now.Year == htolbgdzeropoint.Year))
                {
                    var currenttime = DateTime.Now;
                    if (currenttime.Hour > 18 || htolbgdzeropoint > currenttime)
                    {
                        sundaylog("Sunday Report End for time....");
                        return;
                    }

                    sundaylog("start computer HTOL " + htolbgdzeropoint.ToString("yyyy-MM-dd HH:mm:ss") + " Monthly data");
                    VcselBGDVM.StartHTOLBGDComputer(htolbgdzeropoint, vcselpninfo, this);
                    htolbgdzeropoint = htolbgdzeropoint.AddMonths(1);
                }
                else
                {
                    var currenttime = DateTime.Now;
                    if (currenttime.Hour > 18 || htolbgdzeropoint > currenttime)
                    {
                        sundaylog("Sunday Report End for time....");
                        return;
                    }

                    //EXIST SUCH DATA
                    sundaylog(htolbgdzeropoint.ToString("yyyy-MM-dd HH:mm:ss") + " Monthly data exist");
                    htolbgdzeropoint = htolbgdzeropoint.AddMonths(1);
                    continue;
                }

                times = times + 1;
                if (times > 26)
                {
                    break;
                }
            }//end while
        }

        private void SendATETestData()
        {
            var filename = "log" + DateTime.Now.ToString("yyyy-MM-dd");
            var wholefilename = Server.MapPath("~/userfiles") + "\\" + filename;
            if (!System.IO.File.Exists(wholefilename))
            {
                ATEUtility.EmailATETestDailyData("SFP+ TUNABLE", this);
                ATEUtility.EmailATETestDailyData("XFP TUNABLE", this);
            }
        }

        public ActionResult HeartBeat()
        {
            //add files to let sundayreport have enough time to solve report
            var currenttime = DateTime.Now;
            var sundayreportStart = Server.MapPath("~/userfiles") + "\\" + "SundayReportStart" + currenttime.ToString("yyyy-MM-dd");
            var sundayreportDone = Server.MapPath("~/userfiles") + "\\" + "SundayReportDone" + currenttime.ToString("yyyy-MM-dd");

            if (System.IO.File.Exists(sundayreportStart))
            {
                return View();
            }

            if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday
                || DateTime.Now.DayOfWeek == DayOfWeek.Tuesday
                || DateTime.Now.DayOfWeek == DayOfWeek.Thursday)
            {
                if (!System.IO.File.Exists(sundayreportDone))
                {
                    System.IO.File.WriteAllText(sundayreportStart, "hello world");
                    try
                    {
                        SundayReport();
                    }
                    catch (Exception ex) { }
                    System.IO.File.WriteAllText(sundayreportDone, "hello world");
                    System.IO.File.Delete(sundayreportStart);
                    return View();
                }
            }

            //daily report start
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
                SendWeeklyReportNotice();
            }
            catch (Exception ex)
            { }

            heartbeatlog("heart beat start");

            try
            {
                SendTaskAlertEmail(ISSUETP.Task);
                SendIQETaskAlertEmail();
                SendRMAAlertEmail();
                SendOBAAlertEmail();
            }
            catch (Exception ex) { }


            heartbeatlog("ProjectTestData.PrePareMESLatestData");

            foreach (var pjkey in pjkeylist)
            {
                try
                {
                    ProjectTestData.PrePareMESLatestData(pjkey, this);
                }
                catch (Exception ex)
                { }
            }

            heartbeatlog("ProjectTestData.PrePareOSALatestData");

            foreach (var pjkey in pjkeylist)
            {
                try
                {
                    ProjectTestData.PrePareOSALatestData(pjkey, this);
                }
                catch (Exception ex)
                { }
            }

            heartbeatlog("ProjectTestData.PrePareATELatestData");

            foreach (var pjkey in pjkeylist)
            {
                try
                {
                    ProjectTestData.PrePareATELatestData(pjkey,this);
                }
                catch (Exception ex)
                { }
            }

            heartbeatlog("ExternalDataCollector.RefreshRMAData");

            try
            {
                ExternalDataCollector.RefreshRMAData(this);
                ExternalDataCollector.UpdateRMABackUPDataRate();
            }
            catch (Exception ex) { }

            heartbeatlog("ExternalDataCollector.RefreshVcselRMAData");

            try
            {
                ExternalDataCollector.RefreshVcselRMAData(this);                
            }
            catch (Exception ex) { }


            heartbeatlog("ExternalDataCollector.RefreshRELData");

            try
            {
                ExternalDataCollector.RefreshRELData(this);
            }
            catch (Exception ex) { }

            heartbeatlog("ExternalDataCollector.RefreshIQEData");

            try
            {
                ExternalDataCollector.RefreshIQEData(this);
            }
            catch (Exception ex) { }

            heartbeatlog("ExternalDataCollector.RefreshOBAFromDMR");
          
            try
            {
                ExternalDataCollector.RefreshOBAFromDMR(this);
            }
            catch (Exception ex) { }

            heartbeatlog("RefreshOQMJo");

            try {
                ExternalDataCollector.RefreshOQMJo(this);
            }
            catch (Exception ex) { }

            heartbeatlog("LoadBITestDateFromAuto");

            try
            {
                BIDataUtility.LoadBITestDateFromAuto(this);
            }
            catch (Exception ex) { }

            heartbeatlog("BITestData.LoadHTOLTestData");

            try
            {
                BIDataUtility.LoadHTOLTestData(this);
            }
            catch (Exception ex) { }

            //heartbeatlog("BITestData.PrePareLatestData");

            //foreach (var pjkey in pjkeylist)
            //{
            //    try
            //    {
            //        BITestData.PrePareLatestData(this, pjkey);
            //    }
            //    catch (Exception ex)
            //    { }
            //}

            heartbeatlog("ProcessData.LoadMesWorkflow");

            foreach (var pjkey in pjkeylist)
            {
                try
                {
                    ProcessData.LoadMesWorkflow(pjkey, this);
                }
                catch (Exception ex)
                { }
            }

            heartbeatlog("ProcessData.LoadMESMoveHistory");

            foreach (var pjkey in pjkeylist)
            {
                try
                {
                    ProcessData.LoadMESMoveHistory(pjkey, this);
                }
                catch (Exception ex)
                { }
            }

            heartbeatlog("BIDataUtility.LoadModuleTXOFromMESBackup");

            try
            {
                BIDataUtility.LoadModuleTXOFromMESBackup(this);
            }
            catch (Exception ex) { }

            heartbeatlog("BIDataUtility.LoadProcessTXOFromAuto");

            try
            {
                BIDataUtility.LoadProcessTXOFromAuto(this);
            }
            catch (Exception ex) { }

            heartbeatlog("RefreshNeoMAPData");

            try
            {
                ExternalDataCollector.RefreshNeoMAPData(this);
            }
            catch (Exception ex) { }

            heartbeatlog("ExternalDataCollector.RefreshWaferCoord");

            try
            {
                ExternalDataCollector.RefreshWaferCoord(this);
            }
            catch (Exception ex) { }

            heartbeatlog("refresh cache");

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

            heartbeatlog("Heart beat end");

            return View();
        }

        public ActionResult RefreshCache()
        {
            var pjkeylist = ProjectViewModels.RetrieveAllProjectKey();
            //heartbeatlog("refresh cache");
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

            //heartbeatlog("refresh cache end");

            return RedirectToAction("ViewAll", "Project");
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

        //private void UpdateJO()
        //{
        //    var sns = ProjectTestData.RetrieveAllSNWithNoJO();
        //    logjoinfo("get sn count from local: " + sns.Count.ToString());

        //    var dict = new Dictionary<string, string>();

        //    int i = 0;
        //    foreach (var sn in sns)
        //    {
        //        var jo = ProjectTestData.RetrieveJOFromMESSN(sn);
        //        if (!string.IsNullOrEmpty(jo))
        //        {
        //            dict.Add(sn, jo);
        //            i = i + 1;
        //        }

        //        if (i % 10000 == 0)
        //        {
        //            logjoinfo("read jo count: " + i.ToString());

        //            foreach (var kv in dict)
        //            {
        //                ProjectTestData.UpdateJO(kv.Key, kv.Value);
        //            }

        //            dict.Clear();
        //            logjoinfo("write jo ");
        //        }
        //    }

        //    logjoinfo("write last jo ");
        //    foreach (var kv in dict)
        //    {
        //        ProjectTestData.UpdateJO(kv.Key, kv.Value);
        //    }
        //    logjoinfo("finish last jo ");
        //}

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


        private void UpdateAnalyzeID()
        {
            var allpj = ProjectViewModels.RetrieveAllProjectKey();
            allpj.Add("BURNIN");

            foreach (var pj in allpj)
            {
                var allerrorvm = ProjectErrorViewModels.RetrieveErrorByPJKey(pj, this);
                foreach (var errorvm in allerrorvm)
                {
                    foreach (var errorcommentpair in errorvm.PairComments)
                    {
                        var aid = IssueViewModels.GetUniqKey();
                        foreach (var ecom in errorcommentpair)
                        {
                            ProjectErrorViewModels.UpdateErrorCommentAID(ecom.ErrorKey, ecom.CommentType, ecom.CommentDate.ToString("yyyy-MM-dd HH:mm:ss"), aid);
                        }
                    }
                }
            }
        }

        public ActionResult HeartBeat2()
        {
            ExternalDataCollector.RefreshOBAFromDMR(this);
            //var vcselpninfo = VcselPNData.RetrieveVcselPNInfo();
            //var sdate = DateTime.Parse("2017-09-01 00:00:00");
            //for (var idx = 0; idx < 10; idx++)
            //{
            //    if (!VcselMonthData.MonthDataExist(sdate, "HTOLMonthData"))
            //    {
            //        try
            //        {
            //            VcselBGDVM.StartHTOLBGDComputer(sdate, vcselpninfo, this);
            //        }
            //        catch (Exception ex)
            //        {
            //            System.Windows.MessageBox.Show(ex.Message);
            //        }
            //    }

            //    sdate = sdate.AddMonths(1);
            //}

            //BIDataUtility.LoadHTOLTestData(this);

            //BIDataUtility.BIOLDTestDateExplore(this);

            //ExternalDataCollector.UpdateRMABackUPSN();
            //ExternalDataCollector.UpdateRMABackUPDataRate();

            //ExternalDataCollector.RefreshRMAData(this);
            //ATEUtility.EmailATETestDailyData("SFP+ TUNABLE", this);

            //var pndict = new Dictionary<string, bool>();
            //ATEUtility.RetrieveATEData("COHERENT", DateTime.Parse("2018-05-10 00:00:00"), DateTime.Parse("2018-05-15 23:59:59"),pndict);

            //ExternalDataCollector.RefreshVcselRMAData(this);

            //BIDataUtility.UpdateBITestResultWaferPN(DateTime.Parse("2017-10-01 00:00:00"));

            //try
            //{
            //    var vcselpninfo = VcselPNData.RetrieveVcselPNInfo();
            //    //VcselBGDVM.TestVcselBGDComputer(DateTime.Parse("2017-12-01 00:00:00"), vcselpninfo, this);
            //    VcselBGDVM.TestVcselBGDComputer(DateTime.Parse("2018-04-01 00:00:00"), vcselpninfo, this);
            //}
            //catch (Exception ex)
            //{
            //    System.Windows.MessageBox.Show(ex.Message);
            //}

            //ExternalDataCollector.RefreshIQEData(this);

            //ProjectTestData.PrePareOSALatestData("25GWIRELESSTOSAG", this);

            //var allpjkey = ProjectViewModels.RetrieveAllProject();
            //foreach (var pjkey in allpjkey)
            //{
            //    ProjectYieldViewModule.GetYieldByWeeks(pjkey.ProjectKey, HttpContext.Cache, 4);
            //}
            //MESUtility.DebugCriticalFunction("4803b080008cde33", "OE25LPTE", this);
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
                VcselBGDVM.SendVCSELBGDReport(this);

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
            ViewBag.AllUserList = "[\"" + string.Join("\",\"", asilist.ToArray()) + "\"]";

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

            var netcomputername = EmailUtility.RetrieveCurrentMachineName();
            validatestr = validatestr.Replace("//localhost", "//" + netcomputername);
            validatestr2 = validatestr2.Replace("//localhost", "//" + netcomputername);


            var content = "Hi All,\r\n\r\nThis is a "+vm.ProjectKey+" Critical Error Alarm information. Please pay your attention to it. Thanks!";
            content = content + "\r\n\r\n[" + LYTTASK.Summary + "]  is created base on analyse of task: ";
            content = content + "\r\n\r\n" + vm.Summary;
            content = content + "\r\n\r\nCritical Error TASK LINK: " + validatestr2;
            content = content + "\r\n\r\nTRIGGER TASK LINK: " + validatestr;

            var toaddrs = new List<string>();
            toaddrs.AddRange(addrlist);
            toaddrs.Add(vm.Assignee);
            toaddrs.Add(vm.Reporter);
            toaddrs.Add(LYTTASK.Assignee);

            EmailUtility.SendEmail(this, "Project "+vm.ProjectKey+" Critical Error Alarm - WUXI Engineering System", toaddrs, content);

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
                    CreateLYTSubTask(CRITICALERRORTYPE.CONTAINMENTACTION, "Containment Action for " + comment, vm.ProjectKey, LYTTASK.IssueKey, updater, updater, DateTime.Now.AddDays(7),ISSUESUBTYPE.CONTAINMENT.ToString());
                    CreateLYTSubTask(CRITICALERRORTYPE.CORRECTIVEACTION, "Corrective Action for " + comment, vm.ProjectKey, LYTTASK.IssueKey, updater, updater, DateTime.Now.AddDays(14),ISSUESUBTYPE.CORRECTIVE.ToString());
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

        private static void CreateLYTSubTask(string presum, string sum, string pjkey, string parentkey, string analyser, string reporter, DateTime duedate,string moretag)
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

            IssueTypeVM.SaveIssueType(vm.IssueKey, ISSUESUBTYPE.CrititalFailureTask.ToString(), ISSUESUBTYPE.CrititalFailureTask.ToString());

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
            if (!ckdict.ContainsKey("logonuser"))
            {
                return RedirectToAction("LoginUser", "User");
            }

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

        private bool IsProjectMem(string pjkey)
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser"))
            {
                var updater = ckdict["logonuser"].Split(new char[] { '|' })[0].ToUpper();
                var memlist = ProjectViewModels.RetrieveProjectMembers(pjkey);
                foreach (var item in memlist)
                {
                    if (item.Name.ToUpper().Contains(updater))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public ActionResult ProjectDash(string ProjectKey)
        {
            //var checkresult = CheckLoginAndPermit(Request, "Project", "ProjectDash");
            //if (checkresult.GetType() != (new Boolean()).GetType())
            //{
            //    return (ActionResult)checkresult;
            //}

            var PJKey = ProjectKey;
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

                ViewBag.PJKey = PJKey;

                ViewBag.IsPJMember = "FALSE";
                if (IsProjectMem(PJKey))
                {
                    ViewBag.IsPJMember = "TRUE";
                }

                var memlist = ProjectViewModels.RetrieveProjectMembers(PJKey);
                var memstrlist = new List<string>();
                foreach (var m in memlist)
                {
                    memstrlist.Add(m.Name.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries)[0]);
                }
                ViewBag.pjmemlist = "[\"" + string.Join("\",\"", memstrlist.ToArray()) + "\"]";

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
            var vm = IssueViewModels.RetrievePMTask(PJKey, Resolute.Working, this);
            var list2 = IssueViewModels.RetrievePMTask(PJKey, Resolute.Done, this);
            vm.AddRange(list2);

            var donginglist = new List<object>();
            var donelist = new List<object>();

            foreach (var item in vm)
            {
                if (string.Compare(item.Resolution, Resolute.Working, true) == 0
                    || string.Compare(item.Resolution, Resolute.Reopen, true) == 0)
                {
                    donginglist.Add(
                        new
                        {
                            id = item.IssueKey,
                            title = item.Summary.Replace(CRITICALERRORTYPE.PMTASK, "").Trim(),
                            description = item.CommentList.Count > 0 ? item.CommentList[0].Comment : string.Empty,
                            dueDate = item.DueDate.ToString("yyyy-MM-dd"),
                            assignee = item.Assignee.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries)[0]
                        });
                }

                if (string.Compare(item.Resolution, Resolute.Fixed, true) == 0
                    || string.Compare(item.Resolution, Resolute.Done, true) == 0)
                {
                    donelist.Add(
                        new
                        {
                            id = item.IssueKey,
                            title = item.Summary.Replace(CRITICALERRORTYPE.PMTASK, "").Trim(),
                            description = item.CommentList.Count > 0 ? item.CommentList[0].Comment : string.Empty,
                            dueDate = item.DueDate.ToString("yyyy-MM-dd"),
                            assignee = item.Assignee.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries)[0]
                        });
                }
            }

            var mylists = new List<object>();
            //mylists.Add(
            //    new
            //    {
            //        id = "mytobelist",
            //        title = "TODO",
            //        defaultStyle = "lobilist-warning",
            //        controls = false,
            //        useCheckboxes = false,
            //        items = pendinglist
            //    });

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


        private static void SendTaskEvent(IssueViewModels vm, string comment, Controller ctrl)
        {
            var routevalue = new RouteValueDictionary();
            routevalue.Add("issuekey", vm.IssueKey);
            //send validate email
            string scheme = ctrl.Url.RequestContext.HttpContext.Request.Url.Scheme;
            string validatestr = ctrl.Url.Action("UpdateIssue", "Issue", routevalue, scheme);

            var netcomputername = EmailUtility.RetrieveCurrentMachineName();
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
            vm.Resolution = Resolute.Working;
            vm.ResolvedDate = DateTime.Parse("1982-05-06 01:01:01");
            vm.Description = desc;
            vm.StoreIssue();
            IssueTypeVM.SaveIssueType(vm.IssueKey, ISSUESUBTYPE.Task.ToString(),ISSUESUBTYPE.PMTask.ToString());
            SendTaskEvent(vm, desc, this);
            return vm;
        }

        private IssueViewModels _CreateLinkTask(string pjkey, string summary, string duedate, string asignee, string creator, string desc,string originalid)
        {
            var vm = new IssueViewModels();
            vm.ProjectKey = pjkey;
            vm.IssueKey = IssueViewModels.GetUniqKey();
            vm.IssueType = ISSUETP.Task;
            vm.Summary = summary;
            vm.Priority = ISSUEPR.Major;
            vm.DueDate = DateTime.Parse(duedate);
            vm.ReportDate = DateTime.Now;
            vm.Assignee = asignee;
            vm.Reporter = creator;
            vm.Creator = creator;
            vm.Resolution = Resolute.Working;
            vm.ResolvedDate = DateTime.Parse("1982-05-06 01:01:01");
            vm.Description = desc;
            vm.StoreIssue();
            IssueTypeVM.SaveIssueType(vm.IssueKey, ISSUESUBTYPE.Task.ToString(), ISSUESUBTYPE.Task.ToString());

            var irs = new IssueRelationShipVM();
            irs.MasterIssueKey = originalid;
            irs.SlaveIssueKey = vm.IssueKey;
            IssueRelationShipVM.UpdateIssueRelationShip(irs);

            SendTaskEvent(vm, desc, this);
            return vm;
        }

        [HttpPost]
        public JsonResult TodoListAdd()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
            var pjkey = Request.Form["pjkey"];
            var summary = Request.Form["title"];
            var desc = Request.Form["description"];
            var duedate = Request.Form["duedate"] + " 10:00:00";
            var assignee = Request.Form["assignee"]+"@FINISAR.COM";
           
            var task = CreatePMTask(pjkey, summary, duedate, assignee, updater, desc);
            var res = new JsonResult();
            res.Data = new { success = true };
            return res;
        }

        public JsonResult CreateLinkTask()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
            var pjkey = Request.Form["pjkey"];
            var summary = Request.Form["title"];
            var desc = Request.Form["description"];
            var duedate = Request.Form["duedate"] + " 10:00:00";
            var assignee = Request.Form["assignee"] + "@FINISAR.COM";
            var originalid = Request.Form["originalid"];

            var task = _CreateLinkTask(pjkey, summary, duedate, assignee, updater, desc, originalid);
            task.Summary = "<a href='/Issue/UpdateIssue?issuekey=" + task.IssueKey + "' target='_blank'>" + task.Summary + "</a>";
            task.Assignee = Request.Form["assignee"];
            var res = new JsonResult();
            res.Data = new {
                success = true,
                task = task
            };
            return res;
        }

        public JsonResult AddPMTaskRemark()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

            var id = Request.Form["id"];
            var taskremark = Request.Form["taskremark"];
            var tempcomment = new IssueComments();
            tempcomment.IssueKey = id;
            tempcomment.Comment = taskremark;
            tempcomment.Reporter = updater.Split(new char[] { '@' })[0];
            tempcomment.CommentDate = DateTime.Now;

            IssueViewModels.StoreIssueComment(id, tempcomment.dbComment, updater, COMMENTTYPE.Description);

            var res = new JsonResult();
            res.Data = new
            {
                success = true,
                remark = tempcomment
            };
            return res;
        }

        public JsonResult AddPMLinkTaskRemark()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

            var id = Request.Form["id"];
            var taskremark = Request.Form["taskremark"];
            var tempcomment = new IssueComments();
            tempcomment.IssueKey = id;
            tempcomment.Comment = taskremark;
            tempcomment.Reporter = updater.Split(new char[] { '@' })[0];
            tempcomment.CommentDate = DateTime.Now;

            IssueViewModels.StoreIssueComment(id, tempcomment.dbComment, updater, COMMENTTYPE.InternalReport);

            var res = new JsonResult();
            res.Data = new
            {
                success = true
            };
            return res;
        }

        public JsonResult ClosePMLinkTask()
        {
            var id = Request.Form["id"];
            var realissue = IssueViewModels.RetrieveIssueByIssueKey(id, this);
            if (realissue != null)
            {
                realissue.Resolution = Resolute.Done;
                realissue.UpdateIssue();
            }

            var res = new JsonResult();
            res.Data = new
            {
                success = true
            };
            return res;
        }

        //[HttpPost]
        //public JsonResult TodoListUpdate()
        //{
        //    var vm = ListOperateParse();

        //    if (string.IsNullOrEmpty(vm.Summary))
        //    {
        //        var res1 = new JsonResult();
        //        res1.Data = new { success = false };
        //        return res1;
        //    }

        //    var realissue = IssueViewModels.RetrieveIssueByIssueKey(vm.IssueKey, this);
        //    if (realissue != null)
        //    {
        //        if (!string.IsNullOrEmpty(vm.Description))
        //        {
        //            if (realissue.CommentList.Count == 0)
        //            {
        //                var ckdict = CookieUtility.UnpackCookie(this);
        //                var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
        //                var com = new IssueComments();
        //                com.Comment = vm.Description;
        //                IssueViewModels.StoreIssueComment(realissue.IssueKey, com.dbComment, updater, COMMENTTYPE.Description);
        //            }
        //            else
        //            {
        //                realissue.CommentList[0].Comment = vm.Description;
        //                IssueViewModels.UpdateSPComment(realissue.IssueKey, realissue.CommentList[0].CommentType
        //                    , realissue.CommentList[0].CommentDate.ToString("yyyy-MM-dd HH:mm:ss"), realissue.CommentList[0].dbComment);
        //            }
        //        }

        //        if (!vm.DueDate.Equals(realissue.DueDate)||!vm.Assignee.Equals(realissue.Assignee))
        //        {
        //            realissue.DueDate = vm.DueDate;
        //            realissue.Assignee = vm.Assignee;
        //            realissue.UpdateIssue();
        //        }
        //    }

        //    var res = new JsonResult();
        //    res.Data = new { success = true };
        //    return res;
        //}

        [HttpPost]
        public JsonResult TodoListDelete()
        {
            var id = Request.Form["id"];
            IssueViewModels.RemoveIssue(id, this);

            var res = new JsonResult();
            res.Data = new { success = true };
            res.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return res;
        }


        [HttpPost]
        public JsonResult TodoListMove()
        {
            var id = Request.Form["id"];
            var newlist = Request.Form["newlist"];

            var realissue = IssueViewModels.RetrieveIssueByIssueKey(id, this);
            if (realissue != null)
            {
                if (string.Compare(newlist, "DOING", true) == 0)
                {
                    realissue.Resolution = Resolute.Working;
                    realissue.UpdateIssue();
                }
                if (string.Compare(newlist, "DONE", true) == 0)
                {
                    realissue.Resolution = Resolute.Done;
                    realissue.UpdateIssue();
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
            ViewBag.AllUserList = "[\"" + string.Join("\",\"", asilist.ToArray()) + "\"]";

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
            CreateLYTSubTask(CRITICALERRORTYPE.CONTAINMENTACTION, "Containment Action for " + comment, vm.ProjectKey, vm.IssueKey, updater, updater, DateTime.Now.AddDays(7), ISSUESUBTYPE.CONTAINMENT.ToString());
            CreateLYTSubTask(CRITICALERRORTYPE.CORRECTIVEACTION, "Corrective Action for " + comment, vm.ProjectKey, vm.IssueKey, updater, updater, DateTime.Now.AddDays(14), ISSUESUBTYPE.CORRECTIVE.ToString());

            var comment1 = new IssueComments();
            comment1.Comment = comment;
            IssueViewModels.StoreIssueComment(vm.IssueKey, comment1.dbComment, updater, COMMENTTYPE.Description);
            IssueViewModels.UpdateIssueAssigneeAndResolution(vm.IssueKey, updater, Resolute.Reopen, CRITICALERRORTYPE.OCAP + vm.Summary);
            IssueTypeVM.SaveIssueType(vm.IssueKey, ISSUESUBTYPE.CrititalFailureTask.ToString(),ISSUESUBTYPE.OCAP.ToString());

            if (! String.IsNullOrEmpty(fileurl))
            {
                IssueViewModels.StoreIssueAttachment(vm.IssueKey, fileurl);
            }
        }

        private void SendOCAPEvent(List<IssueViewModels> vms, string comment, List<string> addrlist, string fileUrl)
        {
            var netcomputername = EmailUtility.RetrieveCurrentMachineName();

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

        private void SumOnePJAutoIssu(string pjkey)
        {
            var allerrorkey = ProjectErrorViewModels.RetrieveErrorByPJKey(pjkey, this);
            foreach (var err in allerrorkey)
            {
                var solvecount = IssueViewModels.RetrieveAutoCloseIssueCount(pjkey, err.OrignalCode);
                ProjectErrorViewModels.UpdateProjectAutoCloseCount(solvecount,pjkey, err.OrignalCode);
            }
        }

        public ActionResult SumAutoCloseIssue()
        {
            var pjlist = ProjectViewModels.RetrieveAllProjectKey();
            foreach (var pj in pjlist)
            {
                SumOnePJAutoIssu(pj);
            }

            return View("HeartBeat");
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
                var ekey = Request.Form["ekey"];

                var data = ProjectErrorICareVM.GetIssueIcare(pkey, ekey, updater);
                if (data.Icare == 1)
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
                var ekey = Request.Form["ekey"];

                ProjectErrorICareVM.CancelICare(pkey, ekey, updater);

                LogVM.WriteLog(updater.ToUpper(), pkey, DetermineCompName(Request.UserHostName),
                        Request.Url.ToString(), "ProjectError", "Cancel ICare", ekey, LogType.CancelICare, Log4NetLevel.Info, "");

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
                var ekey = Request.Form["ekey"];

                ProjectErrorICareVM.ICare(pkey, ekey, updater);

                LogVM.WriteLog(updater.ToUpper(), pkey, DetermineCompName(Request.UserHostName),
                        Request.Url.ToString(), "ProjectError", "ICare", ekey, LogType.ICare, Log4NetLevel.Info, "");

                ret.Data = new { success = true };
                return ret;
            }
            else
            {
                ret.Data = new { success = false };
                return ret;
            }
        }

        public ActionResult ProjectErrorMind(string ProjectKey, string OrignalCode)
        {
            ViewBag.PJKey = ProjectKey;

            ViewBag.ProjectKey = ProjectKey;
            ViewBag.OriginalCode = OrignalCode;

            ViewBag.isie8 = false;
            var browse = Request.Browser;
            if (string.Compare(browse.Browser, "IE", true) == 0
                && (string.Compare(browse.Version, "7.0", true) == 0
                || string.Compare(browse.Version, "8.0", true) == 0))
            {
                ViewBag.isie8 = true;
            }

            return View();
        }

        [HttpPost]
        public JsonResult ProjectErrorData()
        {
            var OriginalCode = Request.Form["OriginalCode"];
            var data = ProjectErrorViewModels.GetProjectErrorByOrignalCode(OriginalCode, this);

            var res = new JsonResult();
            res.Data = new
            {
                success = true,
                jm_data = data
            };

            return res;

        }

        public JsonResult RetrieveErrorCommentsByAnalyzeID()
        {
            var id = Request.Form["id"];
            var commentlist = ProjectErrorViewModels.RetrieveErrorCommentsByAnalyzeID(id, this);
            
            var failuredetail = new object();
            var rootcause = new object();
            var result = new object();
            var analysistitle = new object();
            foreach (var item in commentlist)
            {
                var commenttmp = new
                {
                    time = item.CommentDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    reporter = item.Reporter.ToUpper().Replace("@FINISAR.COM", ""),
                    content = item.Comment
                };
                if (string.Compare(item.CommentType, PJERRORCOMMENTTYPE.AnalyzeTitle) == 0)
                {
                    analysistitle = commenttmp;
                    continue;
                }
                if(string.Compare(item.CommentType, PJERRORCOMMENTTYPE.FailureDetail) == 0)
                {
                    failuredetail = commenttmp;
                    continue;
                }
                if (string.Compare(item.CommentType, PJERRORCOMMENTTYPE.RootCause) == 0)
                {
                    rootcause = commenttmp;
                    continue;
                }
                if (string.Compare(item.CommentType, PJERRORCOMMENTTYPE.Result) == 0)
                {
                    result = commenttmp;
                    continue;
                }
            }

            var ret = new JsonResult();
            ret.Data = new
            {
                title = analysistitle,
                failuredetail = failuredetail,
                rootcause = rootcause,
                result = result
            };

            return ret;
        }

        public JsonResult UpdateErrorCommentTitle()
        {
            var id = Request.Form ["id"];
            var name = Request.Form["name"];

            ProjectErrorViewModels.UpdateAnalyzeTitle(id,name);

            var ret = new JsonResult();
            ret.Data = new { success = true };
            return ret;
        }

        private string MachineScattChart(ProjectYieldViewModule yield,string station, string divid)
        {
            var failurepair = new Dictionary<string, List<KeyValuePair<string, int>>>();
            var maxamount = 0;

            var failuredict = yield.FirstYieldFailureTimeDist;
            foreach (var item in failuredict)
            {
                var date = item.Key.Split(new string[] { "##" }, StringSplitOptions.RemoveEmptyEntries)[0];
                var failure = item.Key.Split(new string[] { "##" }, StringSplitOptions.RemoveEmptyEntries)[1];
                var amount = item.Value;
                if (amount > maxamount) maxamount = amount;


                if (failurepair.ContainsKey(failure))
                {
                    failurepair[failure].Add(new KeyValuePair<string, int>(date, amount));
                }
                else
                {
                    var templist = new List<KeyValuePair<string, int>>();
                    templist.Add(new KeyValuePair<string, int>(date,amount));
                    failurepair.Add(failure, templist);
                }
            }

            var chartdata = "";
            foreach (var kv in failurepair)
            {
                var tempdata = "{name: '<name>',data:[<data>]},";
                tempdata = tempdata.Replace("<name>", kv.Key);

                var internaldata = "";
                foreach (var item in kv.Value)
                {
                    if (item.Value > 2)
                    {
                        internaldata = internaldata + "[moment('" + item.Key + "').valueOf()," + item.Value + "],";
                    }
                }

                if (internaldata.Length > 0)
                {
                    internaldata = internaldata.Substring(0, internaldata.Length - 1);
                    tempdata = tempdata.Replace("<data>", internaldata);
                    chartdata = chartdata + tempdata;
                }
            }

            if (chartdata.Length > 0)
                chartdata = chartdata.Substring(0, chartdata.Length - 1);

            var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/FailureTimeDist.xml"));
            var mscott = tempscript.Replace("#ElementID#",divid)
                .Replace("#Title#","")// station+" Failure Scatt")
                .Replace("#AmountMAX#", maxamount.ToString())
                .Replace("#DATA#", chartdata);

            return mscott;
        }

        private string MachineParetoChart(ProjectYieldViewModule yield, string station, string divid,string projectkey,string starttime,string endtime)
        {
            var piedatadict = new Dictionary<string, int>();
            var eklist = new List<string>();
            eklist.AddRange(yield.FErrorMap.Keys);

            foreach (var error in eklist)
            {
                if (string.Compare(error, "PASS", true) != 0)
                {
                    foreach (var test in yield.FirstYields)
                    {
                        var val = ProjectYieldViewModule.RetrieveErrorCount(error, test.WhichTest, yield.FErrorMap);

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

            var firstdatalist = piedatadict.ToList();

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

                var reurl = "window.location.href = '/Project/ProjectErrAbbr?ProjectKey=" + projectkey + "'" + "+'&ErrAbbr='+this.category";
                if (!string.IsNullOrEmpty(starttime) && !string.IsNullOrEmpty(endtime))
                {
                    reurl = reurl + "+'&StartDate='+'" + starttime + "'+'&EndDate='+'" + endtime + "'";
                }

                var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/ParetoChart.xml"));
                return tempscript.Replace("#ElementID#", divid)
                    .Replace("#Title#", "")
                    .Replace("#XAxisTitle#", "")
                    .Replace("#ChartxAxisValues#", ChartxAxisValues)
                    .Replace("#AmountMAX#", sum.ToString())
                    .Replace("#PCount#", pcountvalue)
                    .Replace("#ABPercent#", abpecentvalue)
                    .Replace("#PPercent#", ppecentvalue)
                    .Replace("#REDIRECTURL#", reurl);
            }
            else
            {
                return string.Empty;
            }
        }

        private  string MachineYield( string ProjectKey, Dictionary<string, ProjectYieldViewModule> machineyielddict,string divid)
        {
            if (machineyielddict.Count > 0)
            {
                var ChartxAxisValues = "";

                var ftimelist = new List<string>();
                var famountlist = new List<int>();
                var fyieldlist = new List<double>();
                var ryieldlist = new List<double>();
                var maxamout = 0;

                foreach (var item in machineyielddict)
                {
                    ftimelist.Add(item.Key);

                    fyieldlist.Add(item.Value.FirstYield * 100.0);
                    ryieldlist.Add(item.Value.LastYield * 100.0);

                    var tempfamount = 0;
                    foreach (var d in item.Value.FirstYields)
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

                var FINALTOOLTIP = "";

                for (var idx = 0; idx < fyieldlist.Count; idx++)
                {
                    FINALTOOLTIP = FINALTOOLTIP + "'<!doctype html><table>"
                        + "<tr><td><b>FPY</b></td><td>" + fyieldlist[idx].ToString("0.00") + "&#37;</td></tr>"
                        + "<tr><td><b>FY</b></td><td>" + ryieldlist[idx].ToString("0.00") + "&#37;</td></tr>";

                    foreach (var d in machineyielddict[ftimelist[idx]].LastYields)
                    {
                        FINALTOOLTIP = FINALTOOLTIP + "<tr><td><b>" + d.WhichTest + " </b></td><td>Input:</td><td>" + d.InputCount.ToString() + "</td><td> Output:</td><td>" + d.OutputCount.ToString() + "</td></tr>";
                    }

                    FINALTOOLTIP = FINALTOOLTIP + "</table>'";
                    FINALTOOLTIP = FINALTOOLTIP + ",";
                }
                FINALTOOLTIP = FINALTOOLTIP.Substring(0, FINALTOOLTIP.Length - 1);

                //rederect url
                var reurl = "";

                var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/MachineYield.xml"));
                return tempscript.Replace("#ElementID#", divid)
                    .Replace("#Title#", "Stations Yield")
                    .Replace("#ChartxAxisValues#", ChartxAxisValues)
                    .Replace("#XAxisTitle#", "")
                    .Replace("#AmountMAX#", maxamout.ToString())
                    .Replace("#FirstAmount#", famout)
                    .Replace("#FirstYield#", ftempvalue)
                    .Replace("#RetestYield#", rtempvalue)
                    .Replace("#FINALTOOLTIP#", FINALTOOLTIP)
                    .Replace("#REDIRECTURL#", reurl);
            }
            else
            {
                return string.Empty;
            }
        }

        public ActionResult ProjectStations(string ProjectKey, string whichtest,string StartTime,string EndTime)
        {
            //var checkresult = CheckLoginAndPermit(Request, "Project", "ProjectStations");
            //if (checkresult.GetType() != (new Boolean()).GetType())
            //{
            //    return (ActionResult)checkresult;
            //}

            var pjkey = ProjectKey;
            ViewBag.PJKey = pjkey;

            ViewBag.pjkey = pjkey;
            ViewBag.WhichTestList = MachineVM.RetrieveWhichTest(pjkey);
            if (!string.IsNullOrEmpty(whichtest))
            {
                ViewBag.ActiveStation = whichtest;

                var starttime = "";
                var endtime = "";
                if (!string.IsNullOrEmpty(StartTime) && !string.IsNullOrEmpty(EndTime))
                {
                    starttime = StartTime + " 00:00:00";
                    endtime = EndTime + " 23:59:59";
                }
                else
                {
                    starttime = DateTime.Now.AddMonths(-2).ToString("yyyy-MM-dd HH:mm:ss");
                    endtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }
                ViewBag.sDate = Convert.ToDateTime(starttime).ToString("yyyy-MM-dd");
                ViewBag.eDate = Convert.ToDateTime(endtime).ToString("yyyy-MM-dd");

                var dayspan = (DateTime.Parse(endtime) - DateTime.Parse(starttime)).Days;

                var testdata = MachineVM.RetrieveWhichTestData(pjkey, whichtest, starttime , endtime);
                var machineyielddict = MachineVM.RetrieveWhichTestYieldByStation(testdata, dayspan);

                var allstationkey = "AllStations";
                var allstationdata = new List<ProjectTestData>();
                foreach (var item in testdata)
                {
                    item.TestStation = allstationkey;
                    allstationdata.Add(item);
                }
                var allmachineyielddict = MachineVM.RetrieveWhichTestYieldByStation(allstationdata,dayspan);

                var keylist = machineyielddict.Keys;
                var scattchartdict = new Dictionary<string, string>();
                var idx = 1;
                foreach (var key in keylist)
                {
                    var divid = "station-scatter-" + idx;
                    scattchartdict.Add(key, MachineScattChart(machineyielddict[key],key, divid));
                    idx++;
                }

                if (allmachineyielddict.Count > 0)
                {
                    ViewBag.allstationscatter = MachineScattChart(allmachineyielddict[allstationkey], allstationkey, "allstationscatter");
                }

                var paretochartdict = new Dictionary<string, string>();
                idx = 1;
                foreach (var key in keylist)
                {
                    var divid = "station-pareto-" + idx;
                    paretochartdict.Add(key, MachineParetoChart(machineyielddict[key], key, divid, ViewBag.pjkey, starttime,endtime));
                    idx++;
                }

                if (allmachineyielddict.Count > 0)
                {
                    ViewBag.allstationpareto = MachineParetoChart(allmachineyielddict[allstationkey], allstationkey, "allstationpareto", ViewBag.pjkey, starttime, endtime);
                }

                ViewBag.StationList = new List<string>();
                ViewBag.StationList.AddRange(keylist);
                ViewBag.Scattdict = scattchartdict;
                ViewBag.paretodict = paretochartdict;

                if (allmachineyielddict.Count > 0)
                {
                    machineyielddict.Add(allstationkey, allmachineyielddict[allstationkey]);
                }
                ViewBag.MachineYield = MachineYield(ViewBag.pjkey, machineyielddict, "tester-yield");
            }
            else {
                ViewBag.ActiveStation = ViewBag.WhichTestList[0];
            }

            return View();
        }

        public ActionResult RefreshWaferCoord()
        {
            ExternalDataCollector.RefreshWaferCoord(this);
            return View();
        }

        public ActionResult ProjectTestDataDetail(string ProjectKey, string FM = "", string StartDate = "", string EndDate = "", string Type = "")
        {
            StartDate = Convert.ToDateTime(StartDate).ToString("yyyy-MM-dd 07:30:00");
            EndDate = Convert.ToDateTime(EndDate).ToString("yyyy-MM-dd 07:30:00");
            if (!string.IsNullOrEmpty(EndDate)
                    && string.Compare(Convert.ToDateTime(EndDate).ToString("yyyy-MM-dd"), DateTime.Now.ToString("yyyy-MM-dd")) == 0)
            {
                EndDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            var all_data = IssueViewModels.GetIssuesByConditions(ProjectKey, Type, FM, StartDate, EndDate);
            var all_sn_before = ProjectTestData.RetrieveSNBeforeDateWithStation_N(ProjectKey, StartDate);
            all_data.Keys.Intersect(all_sn_before.Keys).ToList().ForEach(key => all_data.Remove(key));
            if(string.Compare(FM, "Other", true) == 0)
            {
                var fm_data = new Dictionary<string, List<ProjectYieldIssueVM>>();
                foreach(var item in all_data)
                {
                    if (fm_data.ContainsKey(item.Value.ErrAbbr.ToUpper()))
                    {
                        fm_data[item.Value.ErrAbbr.ToUpper()].Add(item.Value);
                    }
                    else
                    {
                        fm_data.Add(item.Value.ErrAbbr.ToUpper(), new List<ProjectYieldIssueVM> { item.Value });
                    }
                }
                var otherper = 0.0;
                var other_data = new List<ProjectYieldIssueVM>();
                var total_cnt = all_data.Count - fm_data["PASS"].Count;
                foreach (var item in fm_data.OrderBy( x => x.Value.Count))
                {
                    if(Math.Round(otherper + Math.Round((float)item.Value.Count / total_cnt, 4), 2) <= 0.05)
                    {
                        otherper += Math.Round((float)item.Value.Count / total_cnt, 4);
                        other_data.AddRange(item.Value);
                    }
                    else
                    {
                        break;
                    }
                }
                all_data.Clear();
                foreach(var item in other_data)
                {
                    if (!all_data.ContainsKey((item.ModuleSerialNum + ":" + item.WhichTest).ToUpper()))
                    {
                        all_data.Add((item.ModuleSerialNum + ":" + item.WhichTest).ToUpper(), item);
                    }
                }
            }
            ViewBag.data = all_data;
            ViewBag.pkey = ProjectKey;
            ViewBag.fm = FM;
            ViewBag.sDate = StartDate;
            ViewBag.eDate = EndDate;
            ViewBag.type = Type;

            return View();
        }

        public ActionResult ProjectSNTestDataDetail(string ProjectKey, string FM = "", string StartDate = "", string EndDate = "", string Type = "")
        {
            StartDate = Convert.ToDateTime(StartDate).ToString("yyyy-MM-dd 07:30:00");
            EndDate = Convert.ToDateTime(EndDate).ToString("yyyy-MM-dd 07:30:00");
            if (!string.IsNullOrEmpty(EndDate) 
                    && string.Compare(Convert.ToDateTime(EndDate).ToString("yyyy-MM-dd"), DateTime.Now.ToString("yyyy-MM-dd")) == 0)
            {
                EndDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            var all_sn_data = ProjectTestData.RetrieveSNBeforeDate_N(ProjectKey, StartDate, EndDate);
            var all_sn_before = ProjectTestData.RetrieveSNBeforeDate_N(ProjectKey, "", StartDate);
            all_sn_data.Keys.Intersect(all_sn_before.Keys).ToList().ForEach(key => all_sn_data.Remove(key));
            var all_data = new Dictionary<string, ProjectYieldIssueVM>();
            for (var i = 0; i < Math.Ceiling(all_sn_data.Count / 1000.0); i++)
            {
                var sn_tmp = all_sn_data.Keys.Skip(i * 1000).Take(1000).ToList();
                var data_tmp = IssueViewModels.GetSNIssuesByConditions(ProjectKey, Type, FM, StartDate, "", String.Join("', '", sn_tmp.ToArray()));
                all_data = all_data.Concat(data_tmp).ToDictionary(x => x.Key, x => x.Value);
            }
            if (string.Compare(FM, "Other", true) == 0)
            {
                var fm_data = new Dictionary<string, List<ProjectYieldIssueVM>>();
                foreach (var item in all_data)
                {
                    if (fm_data.ContainsKey(item.Value.ErrAbbr.ToUpper()))
                    {
                        fm_data[item.Value.ErrAbbr.ToUpper()].Add(item.Value);
                    }
                    else
                    {
                        fm_data.Add(item.Value.ErrAbbr.ToUpper(), new List<ProjectYieldIssueVM> { item.Value });
                    }
                }
                var otherper = 0.0;
                var other_data = new List<ProjectYieldIssueVM>();
                var total_cnt = all_data.Count - fm_data["PASS"].Count;
                foreach (var item in fm_data.OrderBy(x => x.Value.Count))
                {
                    if (Math.Round(otherper + Math.Round((float)item.Value.Count / total_cnt, 4), 2) <= 0.05)
                    {
                        otherper += Math.Round((float)item.Value.Count / total_cnt, 4);
                        other_data.AddRange(item.Value);
                    }
                }
                all_data.Clear();
                foreach (var item in other_data)
                {
                    if (all_data.ContainsKey((item.ModuleSerialNum + ":" + item.WhichTest).ToUpper()))
                    {
                        all_data.Add((item.ModuleSerialNum + ":" + item.WhichTest).ToUpper(), item);
                    }
                }
            }

            ViewBag.data = all_data;
            ViewBag.pkey = ProjectKey;
            ViewBag.fm = FM;
            ViewBag.sDate = StartDate;
            ViewBag.eDate = EndDate;
            ViewBag.type = Type;

            return View();
        }

        public ActionResult ProjectBRTestDataDetail(string ProjectKey, string FM = "", string BRNUM = "", string BRType = "", string Type = "")
        {
            var data = IssueViewModels.GetBRIssuesByConditions(ProjectKey, Type, BRType, BRNUM, FM);
            ViewBag.data = data;
            ViewBag.pkey = ProjectKey;
            ViewBag.type = Type;
            ViewBag.BRType = BRType;
            ViewBag.BRNUM = BRNUM;
            ViewBag.fm = FM;
            return View();
        }
        public ActionResult Transfer(string ProjectKey)
        {
            var pro_info = ProjectViewModels.RetrieveOneProject(ProjectKey);
            ViewBag.MeList = string.Empty;
            if (pro_info.Count > 0)
            {
                if (pro_info[0].MemberList.Count > 0)
                {
                    var melist = new List<string>();
                    foreach (var mem in pro_info[0].MemberList)
                    {
                        if (string.Compare(mem.Role, ProjectViewModels.MEROLE, true) == 0)
                        {
                            melist.Add(mem.Name.ToUpper());
                        }
                    }
                    if (melist.Count > 0)
                    {
                        ViewBag.MeList = string.Join(";", melist);
                    }
                }
            }
            var asilist = UserViewModels.RetrieveAllUser();
            ViewBag.AllUserList = "[\"" + string.Join("\",\"", asilist.ToArray()) + "\"]";

            ViewBag.ProjectKey = ProjectKey;
            return View();
        }

        [HttpPost]
        public JsonResult SubmitTransfer()
        {
            var pkey = Request.Form["pKey"];
            var trans_to = Request.Form["trans_to"];
            var melist = Request.Form["melist"];


            var res = new JsonResult();
            if (string.IsNullOrEmpty(pkey) || string.IsNullOrEmpty(trans_to) || string.IsNullOrEmpty(melist))
            {
                res.Data = new { success = false };
            }
            else
            {
                var pro_info = ProjectViewModels.RetrieveOneProjectWithClose(pkey);
                if (string.Compare(pro_info[0].MeListStr, melist, true) != 0)
                {
                    //update project me
                    var n_melist = melist.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    ProjectViewModels.UpdateProjectMemberbyRole(pkey, ProjectViewModels.MEROLE, n_melist);
                }
                //update transfer flag
                ProjectViewModels.ProjectTransfer(pkey, trans_to);

                res.Data = new { success = true };
            }
            return res;
        }

        public ActionResult ProjectMileStone(string ProjectKey, string sDate = "", string eDate = "",string withprivate="")
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {

            }
            else
            {
                var ck = new Dictionary<string, string>();
                ck.Add("logonredirectctrl", "Project");
                ck.Add("logonredirectact", "ProjectMileStone");
                CookieUtility.SetCookie(this, ck);
                return RedirectToAction("LoginUser", "User");
            }
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

            ViewBag.actionlist = ProjectMileStonesVM.GetProjectMileStones4Owner(ProjectKey, updater);
            ViewBag.sDate = sDate;
            ViewBag.eDate = eDate;
            ViewBag.ProjectKey = ProjectKey;
            ViewBag.withprivate = "FALSE";
            if (!string.IsNullOrEmpty(withprivate))
            {
                ViewBag.withprivate = withprivate;
            }
            return View();
        }

        [HttpPost]
        public JsonResult AddProMileStone()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
            var pKey = Request.Form["pKey"];
            var actionid = Request.Form["actionid"];
            var date = Request.Form["date"];
            var action = Request.Form["action"];
            var ispublish = Request.Form["ispublish"];
            var pmvm = new ProjectMileStonesVM();
            pmvm.AddDate = date;
            pmvm.Action = action;
            pmvm.IsPublish = ispublish;
            pmvm.UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            if (string.IsNullOrEmpty(actionid))
            {
                pmvm.ProjectKey = pKey;
                pmvm.UserName = updater;
                pmvm.Status = ActionStatus.ValidStatus;
                pmvm.CreateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                ProjectMileStonesVM.AddProjectMileStones(pmvm);
            }
            else
            {
                pmvm.ID = actionid;
                ProjectMileStonesVM.UpdateProjectMileStones(pmvm);
            }

            var res = new JsonResult();
            res.Data = new { success = true };
            return res;
        }
        [HttpPost]
        public JsonResult InvalidProMileStone()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
            var actionid = Request.Form["actionid"];

            var pmvm = new ProjectMileStonesVM();
            pmvm.ID = actionid;
            pmvm.Status = ActionStatus.InValidStatus;
            pmvm.UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            ProjectMileStonesVM.UpdateProjectMileStones(pmvm);

            var res = new JsonResult();
            res.Data = new { success = true };
            return res;
        }

        [HttpPost]
        public JsonResult UpdateProMileStone()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
            var actionid = Request.Form["actionid"];
            var ispublish = Request.Form["ispublish"];

            var pmvm = new ProjectMileStonesVM();
            pmvm.ID = actionid;
            pmvm.IsPublish = string.Compare(ispublish, IsPublishStatus.mPublic) == 0 ? IsPublishStatus.mPrivate : IsPublishStatus.mPublic;
            pmvm.UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            ProjectMileStonesVM.UpdateProjectMileStones(pmvm);

            var res = new JsonResult();
            res.Data = new { success = true };
            return res;
        }

        [HttpPost]
        public JsonResult GetProMileStoneData()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
            var pKey = Request.Form["pKey"];
            var sDate = Request.Form["sDate"];
            var eDate = Request.Form["eDate"];
            var withPrivate = Request.Form["withPrivate"];

            var pvm = ProjectViewModels.RetrieveOneProjectWithClose(pKey)[0];

            var requestmonth = 18;
            var action_data = new List<ProjectMileStonesVM>();
            var yvmlist = new List<ProjectYieldViewModule>();

            if (!string.IsNullOrEmpty(sDate) && !string.IsNullOrEmpty(eDate))
            {
                if (DateTime.Parse(sDate) > DateTime.Parse(eDate))
                {
                    var tempd = sDate;
                    sDate = eDate;
                    eDate = tempd;
                }

                var mlist = ProjectYieldViewModule.RetrieveDateSpanByMonth(sDate, eDate);
                requestmonth = mlist.Count - 1;
                var s = mlist[0].ToString("yyyy-MM-dd HH:mm:ss");
                var e = mlist[mlist.Count - 1].ToString("yyyy-MM-dd HH:mm:ss");
                action_data = ProjectMileStonesVM.GetProjectMileStones(pKey, updater, withPrivate,s ,e );

                for (var yidx = 0;yidx <= mlist.Count-2;yidx++)
                {
                    var pyvm = ProjectYieldViewModule.GetYieldByDateRange(pKey, mlist[yidx].ToString("yyyy-MM-dd HH:mm:ss"), mlist[yidx + 1].AddMinutes(-1).ToString("yyyy-MM-dd HH:mm:ss"), pvm, HttpContext.Cache);
                    if (pyvm.RealTimeYields.Count > 0)
                    {
                        yvmlist.Add(pyvm);
                    }
                }
            }
            else
            {
                var s = DateTime.Now.AddMonths(-18).ToString("yyyy-MM") + "-01 00:00:00";
                var e = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                action_data = ProjectMileStonesVM.GetProjectMileStones(pKey, updater, withPrivate,s ,e);
                yvmlist = ProjectYieldViewModule.GetYieldByMonth(pKey, HttpContext.Cache, requestmonth);
            }

            var combindatedict = new Dictionary<string, DateTime>();
            var yvmdict = new Dictionary<string, ProjectYieldViewModule>();

            foreach (var item in action_data)
            {
                var mstr = DateTime.Parse(item.AddDate).ToString("yyyy-MM");
                if (!combindatedict.ContainsKey(mstr))
                {
                    combindatedict.Add(mstr, DateTime.Parse(mstr + "-01 00:00:00"));
                }
            }
            foreach (var item in yvmlist)
            {
                var mstr = item.EndDate.ToString("yyyy-MM");
                if (!combindatedict.ContainsKey(mstr))
                {
                    combindatedict.Add(mstr, DateTime.Parse(mstr + "-01 00:00:00"));
                }

                yvmdict.Add(mstr, item);
            }

            var alldatelist = combindatedict.Values.ToList();
            alldatelist.Sort();


            var xtimelist = new List<string>();
            var amountlist = new List<int>();
            var fyieldlist = new List<double>();
            var ryieldlist = new List<double>();
            var snyieldlist = new List<double>();

            var maxamout = 0;
            var xidx = 0;
            var alldatedict = new Dictionary<string, int>();
            foreach (var item in alldatelist)
            {
                var datestr = item.ToString("yyyy-MM");
                xtimelist.Add(datestr);
                if (yvmdict.ContainsKey(datestr))
                {
                    var yd = yvmdict[datestr];
                    fyieldlist.Add(Math.Round(yd.FirstYield * 100.0,2));
                    ryieldlist.Add(Math.Round(yd.LastYield * 100.0,2));
                    snyieldlist.Add(Math.Round(yd.SNYield * 100,2));
                    var tempfamount = 0;
                    foreach (var d in yd.FirstYields)
                    {
                        if (d.InputCount > tempfamount) { tempfamount = d.InputCount; }
                        if (d.InputCount > maxamout) { maxamout = d.InputCount; }
                    }
                    amountlist.Add(tempfamount);
                }
                else
                {
                    fyieldlist.Add(0.0);
                    ryieldlist.Add(0.0);
                    snyieldlist.Add(0.0);
                    amountlist.Add(0);
                }

                alldatedict.Add(datestr, xidx);
                xidx = xidx + 1;
            }


            //var plotcolor = (new string[] {"#8CC9F7","#BEEBDF","#FDEEC3","#F6B0B0","#EC88F4"
            //    , "#4FADF3","#12CC92","#FA9604","#ED6161","#EF46FC" }).ToList();
            //var labelylist = (new int[] { 50, 64, 78, 92, 106 }).ToList();

            //var cidx = 0;
            //var plotarray = new List<object>();
            //foreach (var item in action_data)
            //{
            //    var mstr = DateTime.Parse(item.AddDate).ToString("yyyy-MM");
            //    plotarray.Add(
            //        new
            //        {
            //            color = plotcolor[alldatedict[mstr] % plotcolor.Count],
            //            from = alldatedict[mstr] - 0.5,
            //            to = alldatedict[mstr] + 0.5,
            //            label = new
            //            {
            //                //text = "* " + item.Action+ "--" + item.UserName.Split(new string[] {"@"},StringSplitOptions.RemoveEmptyEntries)[0],
            //                text = "* " + item.Action,
            //                style = new
            //                {
            //                    color = "#000",
            //                    fontSize = (alldatelist.Count < 7) ? "12px" : "0px"
            //                },
            //                y = labelylist[cidx % labelylist.Count],
            //                align = "left"
            //            },
            //        });
            //    cidx = cidx + 1;
            //}

            var milestonedict = new Dictionary<string, string>();
            foreach (var item in action_data)
            {
                var mstr = DateTime.Parse(item.AddDate).ToString("yyyy-MM");
                if (milestonedict.ContainsKey(mstr))
                {
                    var info = item.Action + "--" + item.UserName.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries)[0];
                    milestonedict[mstr] = milestonedict[mstr] + "<br/>" + info;
                }
                else
                {
                    var info = item.Action + "--" + item.UserName.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries)[0];
                    milestonedict.Add(mstr,info);
                }
            }

            var milestonearray = new List<object>();
            foreach (var kv in milestonedict)
            {
                milestonearray.Add(new {
                    x = alldatedict[kv.Key],
                    y = 105,
                    name = kv.Value
                });
            }

            var yAxis = new List<object>();
            yAxis.Add(new {
                title = "Yield (%)",
                min  = 0,
                max = 100
            });
            yAxis.Add(new
            {
                title = "Amount"
            });

            var rmaylist = (new int[] {0, 5, 10, 15, 20, 25,30,35,40,45,50 }).ToList();
            var ccidx = 0;
            var allrmadata = IssueViewModels.RetrieveRMAWithTestTime(pKey);
            var rmaarray = new List<object>();
            foreach (var rma in allrmadata)
            {
                var datestr = rma.ReportDate.ToString("yyyy-MM");
                if (alldatedict.ContainsKey(datestr))
                {
                    rmaarray.Add(
                        new {
                            x = alldatedict[datestr]-0.4,
                            y = rmaylist[ccidx%rmaylist.Count],
                            date = rma.ReportDate.ToString("yyyy-MM-dd"),
                            name = "<a href='/Issue/UpdateIssue?issuekey="+rma.IssueKey+"' target='_blank'>" + rma.ModuleSN + "-" + rma.RMAFailureCode+"</a>"
                        }
                        );
                    ccidx = ccidx + 1;
                }
            }

            //#8085e9,#434348
            var res = new JsonResult();
            res.Data = new { success = true,
                             data = new {
                                 id = "pro-charts",
                                 title = "Project Detail",
                                 xAxis = new { data = xtimelist },
                                 yAxis =yAxis,
                                 data = new {
                                     fyield_data = new {
                                         name = "Final Yield",
                                         color = "#90ed7d",
                                         data = ryieldlist
                                     },
                                     fpyield_data = new
                                     {
                                         name = "First Pass Yield",
                                         data = fyieldlist
                                     },
                                     snyield_data = new
                                     {
                                         name = "SN Yield",
                                         data = snyieldlist
                                     },
                                     amount_data = new {
                                         name = "Amount",
                                         color = "#ff3399",
                                         data = amountlist
                                     },
                                     rma_data = new {
                                         name = "RMA",
                                         color = "#00b050",
                                         data = rmaarray
                                     },
                                     milestone = new {
                                         data = milestonearray
                                     }
                                     //,
                                     //plotBands = plotarray
                                 }
                             }
                            };
            return res;
        }
        [HttpPost]
        public JsonResult SavePersonalProModule()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
            var pKey = Request.Form["pKey"];

            var sortvals = System.Web.Helpers.Json.Decode(Request.Form["sortvals"], (new List<object>()).GetType());
            var data = new List<Dictionary<string, string>>();
            foreach (var item in sortvals)
            {
                var tmp = new Dictionary<string, string>();
                foreach (var line in item)
                {
                    var val = line.Value;
                    if (line.Key == "y" && (val < 0 || val < 90))
                    {
                        val = 0;
                    }
                    tmp.Add(line.Key, val.ToString());
                }
                data.Add(tmp);
            }
            var n_data = data.OrderBy(dict => dict["y"]).ThenBy(dict => dict["x"]).ToList<Dictionary<string, string>>();

            var sortdata = new List<ProjectSortVM>();
            foreach (var item in n_data)
            {
                var tmp = new ProjectSortVM();
                tmp.key = item["val"];
                tmp.visible = item["visible"];
                sortdata.Add(tmp);
            }

            UserProjectModuleMatrix.SaveUserProModuleMatrix(updater.ToUpper(), pKey, sortdata);

            var res = new JsonResult();
            res.Data = new { success = true };
            return res;
        }

        [HttpPost]
        public JsonResult CloseProject()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

            var mycache = HttpContext.Cache;
            mycache.Remove(updater + "_pjlist_CUST");

            var pKey = Request.Form["pKey"];
            ProjectViewModels.UpdateProjectStatus(pKey, ProjectStatus.Close);


            LogVM.WriteLog(updater.ToUpper(), pKey, DetermineCompName(Request.UserHostName),
                    Request.Url.ToString(), "Project", "Close Project", "", LogType.OpenOrCloseProject, Log4NetLevel.Info, "");

            var res = new JsonResult();
            res.Data = new { success = true };
            return res;
        }

        [HttpPost]
        public JsonResult ReopenProject()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

            var mycache = HttpContext.Cache;
            mycache.Remove(updater + "_pjlist_CUST");

            var pKey = Request.Form["pKey"];
            ProjectViewModels.UpdateProjectStatus(pKey, ProjectStatus.Open);

            LogVM.WriteLog(updater.ToUpper(), pKey, DetermineCompName(Request.UserHostName),
                    Request.Url.ToString(), "Project", "ReOpen Project", "", LogType.OpenOrCloseProject, Log4NetLevel.Info, "");

            var res = new JsonResult();
            res.Data = new { success = true };
            return res;
        }
        public ActionResult ClosedProjects()
        {

            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {

            }
            else
            {
                var ck = new Dictionary<string, string>();
                ck.Add("logonredirectctrl", "Project");
                ck.Add("logonredirectact", "ClosedProjects");
                CookieUtility.SetCookie(this, ck);
                return RedirectToAction("LoginUser", "User");
            }

            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

            ViewBag.data = ProjectViewModels.GetClosedProjects();

            return View();
        }
    }

}