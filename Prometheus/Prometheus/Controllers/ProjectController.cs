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


namespace Prometheus.Controllers
{
    public class ProjectController : Controller
    {
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

            var projlist = ProjectViewModels.RetrieveAllProject();

            var filterlist = new List<SelectListItem>();
            var filteritem = new SelectListItem();
            filteritem.Text = "NONE";
            filteritem.Value = "NONE";
            filterlist.Add(filteritem);

            foreach (var item in projlist)
            {
                filteritem = new SelectListItem();
                filteritem.Text = item.ProjectKey;
                filteritem.Value = item.ProjectKey;
                filterlist.Add(filteritem);

                var startdate = DateTime.Now.AddDays(-7);
                var enddate = DateTime.Now.ToString();
                if (startdate.DayOfWeek != DayOfWeek.Thursday)
                {
                    for (int i = 7; i > 0; i--)
                    {
                        if (DateTime.Now.AddDays(0-i).DayOfWeek == DayOfWeek.Thursday)
                        {
                            startdate = DateTime.Now.AddDays(0 - i);
                            break;
                        }
                    }
                }

                var yvm = ProjectYieldViewModule.GetYieldByDateRange(item.ProjectKey, startdate.ToString(), enddate, item);
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

                NPIInfo(item);

                item.PendingTaskCount = IssueViewModels.RetrieveTaskCountByProjectKey(item.ProjectKey, Resolute.Pending).ToString()
                    + "/" + IssueViewModels.RetrieveTaskCountByProjectKey(item.ProjectKey, Resolute.Done).ToString();
                item.PendingFACount = ProjectFAViewModules.RetrieveFADataCount(item.ProjectKey).ToString()
                    +"/"+ ProjectFAViewModules.RetrieveFADataCount(item.ProjectKey,false).ToString();
                item.PendingRMACount = IssueViewModels.RetrieveRMACountByProjectKey(item.ProjectKey, Resolute.Pending).ToString()
                    + "/" + IssueViewModels.RetrieveRMACountByProjectKey(item.ProjectKey, Resolute.Done).ToString();
            }

            filterlist[0].Selected = true;
            ViewBag.pjfilterlist = filterlist;

            SortPJ(projlist);
            return View(projlist);
        }

        private void NPIInfo(ProjectViewModels item)
        {
                var ivmlist = IssueViewModels.RetrieveNPIPROCIssue(item.ProjectKey);
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
                else if(pair1.FirstYield <= 0 && pair2.FirstYield > 0)
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

            foreach (var pm in vm.MemberList)
            {
                if (string.Compare(pm.Role, Prometheus.Models.ProjectViewModels.PMROLE) == 0)
                {
                    pmlist.Add(pm);
                }
            }

            foreach (var eg in vm.MemberList)
            {
                if (string.Compare(eg.Role, Prometheus.Models.ProjectViewModels.ENGROLE) == 0)
                {
                    eglist.Add(eg);
                }
            }


            var asilist = UserViewModels.RetrieveAllUser();
            if (pmlist.Count > 0)
                ViewBag.PM1 = CreateSelectList(asilist, pmlist[0].Name);
            else
                ViewBag.PM1 = CreateSelectList(asilist,"");

            if (pmlist.Count > 1)
                ViewBag.PM2 = CreateSelectList(asilist, pmlist[1].Name);
            else
                ViewBag.PM2 = CreateSelectList(asilist, "");

            if (eglist.Count > 0)
                ViewBag.Engineer1 = CreateSelectList(asilist, eglist[0].Name);
            else
                ViewBag.Engineer1 = CreateSelectList(asilist, "");

            if (eglist.Count > 1)
                ViewBag.Engineer2 = CreateSelectList(asilist, eglist[1].Name);
            else
                ViewBag.Engineer2 = CreateSelectList(asilist, "");

            if (eglist.Count > 2)
                ViewBag.Engineer3 = CreateSelectList(asilist, eglist[2].Name);
            else
                ViewBag.Engineer3 = CreateSelectList(asilist, "");

            if (eglist.Count > 3)
                ViewBag.Engineer4 = CreateSelectList(asilist, eglist[3].Name);
            else
                ViewBag.Engineer4 = CreateSelectList(asilist, "");

            if (eglist.Count > 4)
                ViewBag.Engineer5 = CreateSelectList(asilist, eglist[4].Name);
            else
                ViewBag.Engineer5 = CreateSelectList(asilist, "");

            if (eglist.Count > 5)
                ViewBag.Engineer6 = CreateSelectList(asilist, eglist[5].Name);
            else
                ViewBag.Engineer6 = CreateSelectList(asilist, "");

        }

        public ActionResult CreateProject()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {
                var vm = new ProjectViewModels();
                CreateAllUserLists(vm);
                CreateProjectTypeList(vm);
                return View();
            }
            else
            {
                var ck = new Dictionary<string, string>();
                ck.Add("logonredirectctrl", "Project");
                ck.Add("logonredirectact", "CreateProject");
                CookieUtility.SetCookie(this,ck);
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
            return sb.ToString();
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


        private bool ProjectValidate(ProjectViewModels projectmodel,bool updateproject = false)
        {
            var createerror = "<h3><font color=\"red\">Fail to create/modify project: <ErrorMsg></font></h3>";
            if (string.IsNullOrEmpty(projectmodel.ProjectName.Trim())
                || string.IsNullOrEmpty(projectmodel.ProjectKey.Trim()))
            {
                ViewBag.CreateError = createerror.Replace("<ErrorMsg>","Project Name is empty");
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

            if (string.IsNullOrEmpty(projectmodel.Description.Trim()))
            {
                ViewBag.CreateError = createerror.Replace("<ErrorMsg>", "Project description is empty");
                return false;
            }

            if(!updateproject)
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

            if(eglist.Count == 0)
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
                            ret.Add(new ProjectMesTable("",line.Split(new char[] { '=' })[0].Trim(), line.Split(new char[] { '=' })[1].Trim().Replace("\"", "")));
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

        private List<string> RetrieveProjectInfo(string key,int count)
        {
            var ret = new List<string>();
            try
            {
                for (var i = 1; i < count+1; i++)
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

        private void RetrieveProjectMember(ProjectViewModels projectmodel)
        {
            var pns = RetrieveProjectInfo("PM", 2);
            var lpm = new List<ProjectMembers>();

            if (pns.Count > 0)
            {
                foreach (var p in pns)
                {
                    lpm.Add(new ProjectMembers(projectmodel.ProjectKey, p, ProjectViewModels.PMROLE));
                }
            }

            pns = RetrieveProjectInfo("Engineer", 6);
            if (pns.Count > 0)
            {
                foreach (var p in pns)
                {
                    lpm.Add(new ProjectMembers(projectmodel.ProjectKey, p, ProjectViewModels.ENGROLE));
                }
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
                projectmodel.Description = Server.HtmlDecode(temphtml);
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
                    ViewBag.CreateError = "<h3><font color=\"red\">Fail to create/modify project: " + RMSpectialCh(ex.ToString())+"</font></h3>";
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
            //var pns = RetrieveProjectInfo("PN", 9);
            //if (pns.Count > 0)
            //{
            //    var lpn = new List<ProjectPn>();
            //    foreach (var p in pns)
            //    {
            //        lpn.Add(new ProjectPn(projectmodel.ProjectKey, p));
            //    }
            //    projectmodel.PNList = lpn;
            //}
            projectmodel.PNs = Request.Form["PNs"];
        }

        private void RetrieveStation(ProjectViewModels projectmodel)
        {
            //var stats = RetrieveProjectInfo("Station", 9);
            //if (stats.Count > 0)
            //{
            //    var lstat = new List<ProjectStation>();
            //    foreach (var s in stats)
            //    {
            //        lstat.Add(new ProjectStation(projectmodel.ProjectKey, s));
            //    }
            //    projectmodel.StationList = lstat;
            //}
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

            RetrieveProjectMember(projectmodel);
            RetrieveProjectDesc(projectmodel);

            StoreMesConfig(projectmodel);
            RetrievePNs(projectmodel);
            RetrieveStation(projectmodel);

            projectmodel.ModelIDs = Request.Form["ModelIDs"];
            projectmodel.SumDatasets = Request.Form["SumDatasets"];

            if (!RetrieveProjectDate(projectmodel))
            {
                CreateAllUserLists(projectmodel);
                CreateProjectTypeList(projectmodel);
                return View(projectmodel);
            }


            if (!ProjectValidate(projectmodel))
            {
                CreateAllUserLists(projectmodel);
                CreateProjectTypeList(projectmodel);
                return View(projectmodel);
            }

            projectmodel.StoreProject();

            IssueViewModels.CreateNPIProcTasks(projectmodel.ProjectName, projectmodel.ProjectKey, projectmodel.MemberList[0].Name);

            var ckdict = CookieUtility.UnpackCookie(this);
            var who = (ckdict["logonuser"]).Split(new string[]{ "||"},StringSplitOptions.None)[0];
            //ProjectEvent.CreateProjectEvent(who, projectmodel.ProjectKey, projectmodel.ProjectName);

            MESUtility.StartProjectBonding(projectmodel);
            BIDataUtility.StartProjectBonding(projectmodel);
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

            ViewBag.updateissuelist =pslist;
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
                    || string.Compare(vm.MonitorVcsel,"True",true) == 0)
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
            pjtypelist.Add(ProjectTypeInf.Parallel);
            pjtypelist.Add(ProjectTypeInf.Tunable);
            pjtypelist.Add(ProjectTypeInf.OSA);
            pjtypelist.Add(ProjectTypeInf.LineCard);
            pjtypelist.Add(ProjectTypeInf.QM);
            pjtypelist.Add(ProjectTypeInf.Others);

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
                    CreateAllUserLists(vm);
                    CreateUpdateIssueList(vm);
                    CreateMonitorVcselList(vm);
                    CreateProjectTypeList(vm);
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
                return RedirectToAction("CreateProject","Project");
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

            RetrieveProjectMember(projectmodel);
            RetrieveProjectDesc(projectmodel);

            StoreMesConfig(projectmodel);
            RetrievePNs(projectmodel);
            RetrieveStation(projectmodel);
            projectmodel.ModelIDs = Request.Form["ModelIDs"];
            projectmodel.SumDatasets = Request.Form["SumDatasets"];

            if (!RetrieveProjectDate(projectmodel))
            {
                CreateAllUserLists(projectmodel);
                CreateUpdateIssueList(projectmodel);
                CreateMonitorVcselList(projectmodel);
                CreateProjectTypeList(projectmodel);
                return View(projectmodel);
            }
                

            if (!ProjectValidate(projectmodel,true))
            {
                CreateAllUserLists(projectmodel);
                CreateUpdateIssueList(projectmodel);
                CreateMonitorVcselList(projectmodel);
                CreateProjectTypeList(projectmodel);
                return View(projectmodel);
            }

            var oldpjdata = ProjectViewModels.RetrieveOneProject(projectmodel.ProjectKey);

            bool databondingchange = DataBondingChanged(oldpjdata, projectmodel);
            bool pnbondingchg = PNBondingChanged(oldpjdata, projectmodel);

            projectmodel.StoreProject();

            if (projectmodel.TabList.Count == 0)
            {
                projectmodel.TabList = oldpjdata.TabList;
            }
            
                        
            var ckdict = CookieUtility.UnpackCookie(this);
            var who = (ckdict["logonuser"]).Split(new string[] { "||" }, StringSplitOptions.None)[0];
            //ProjectEvent.UpdateProjectEvent(who, projectmodel.ProjectKey, projectmodel.ProjectName);

            if(databondingchange)
            {
                MESUtility.StartProjectBonding(projectmodel);
            }

            if (pnbondingchg)
            {
                BIDataUtility.StartProjectBonding(projectmodel);
            }

            ProjectTestData.PrePareATELatestData(projectmodel.ProjectKey);

            return RedirectToAction("ViewAll");
        }

        public ActionResult ProjectIssues(string ProjectKey)
        {
            if(ProjectKey != null)
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

                    var yvm = ProjectYieldViewModule.GetYieldByDateRange(vm.ProjectKey, startdate.ToString(), enddate, vm);
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

                    NPIInfo(vm);

                    vm.PendingTaskCount = IssueViewModels.RetrieveTaskCountByProjectKey(vm.ProjectKey, Resolute.Pending).ToString()
                        + "/" + IssueViewModels.RetrieveTaskCountByProjectKey(vm.ProjectKey, Resolute.Done).ToString(); 
                    vm.PendingFACount = ProjectFAViewModules.RetrieveFADataCount(vm.ProjectKey).ToString()
                        + "/" + ProjectFAViewModules.RetrieveFADataCount(vm.ProjectKey, false).ToString();
                    vm.PendingRMACount = IssueViewModels.RetrieveRMACountByProjectKey(vm.ProjectKey, Resolute.Pending).ToString()
                         + "/" + IssueViewModels.RetrieveRMACountByProjectKey(vm.ProjectKey, Resolute.Done).ToString();
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

                var vm  = ProjectFAViewModules.RetrievePendingFAData(ProjectKey);

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
                        namevaluepair = namevaluepair+"{ name:'"+k+"',y:"+ piedatadict[k].ToString()+ "},";
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

        public ActionResult ProjectError(string ProjectKey)
        {
            if (!string.IsNullOrEmpty(ProjectKey))
            {
                var vm = ProjectErrorViewModels.RetrieveErrorByPJKey(ProjectKey);
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
        
        public ActionResult ProjectErrAbbr(string ProjectKey, string ErrAbbr)
        {
            if (!string.IsNullOrEmpty(ProjectKey) && !string.IsNullOrEmpty(ErrAbbr))
            {
                ViewBag.projectkey = ProjectKey;

                var vm = ProjectFAViewModules.RetrieveFADataWithErrAbbr(ProjectKey, ErrAbbr);

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
                return View("ProjectFA", pendingitems);
            }
            return RedirectToAction("ViewAll","Project");
        }

        public ActionResult SNIssue(string ProjectKey, string SN)
        {
            if (!string.IsNullOrEmpty(ProjectKey) && !string.IsNullOrEmpty(SN))
            {
                ViewBag.projectkey = ProjectKey;

                var vm = ProjectFAViewModules.RetrieveFADataWithSN(ProjectKey, SN);

                var asilist = UserViewModels.RetrieveAllUser();
                ViewBag.AllUserList = CreateSelectList1(asilist, "");
                var ckdict = CookieUtility.UnpackCookie(this);
                if (ckdict.ContainsKey("logonuser"))
                {
                    ViewBag.logined = true;
                }
                return View("ProjectFA", vm);
            }
            return RedirectToAction("ViewAll", "Project");
        }

        [HttpPost, ActionName("ProjectDoneFA")]
        [ValidateAntiForgeryToken]
        public ActionResult ProjectDoneFA()
        {
            var projectkey = Request.Form["HProjectKey"];
            ViewBag.projectkey = projectkey;

            var vm = ProjectFAViewModules.RetrieveDoneFAData(projectkey);

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

            return View("ProjectFA", vm);
        }

        public ActionResult ProjectNPI(string ProjectKey)
        {
            if (ProjectKey != null)
            {
                var vmlist = IssueViewModels.RetrieveNPIPROCIssue(ProjectKey);
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


        /*public static void ProjectWeeklyTrend(Controller ctrl, string ProjectKey)
        {
                var vmlist = ProjectYieldViewModule.GetYieldByWeeks(ProjectKey);
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
                    var reurl = "window.location.href = '/Project/ProjectWYieldDetail?ProjectKey="+ProjectKey+"'"+ "+'&EndDate='+this.category";

                    var tempscript = System.IO.File.ReadAllText(ctrl.Server.MapPath("~/Scripts/ColumnChart.xml"));
                    ctrl.ViewBag.chartscript = tempscript.Replace("#ElementID#", "weeklyyield")
                        .Replace("#ChartType#", "column")
                        .Replace("#Title#", "Weekly Yiled Trend")
                        .Replace("#ChartxAxisValues#", ChartxAxisValues)
                        .Replace("#yAxisTitle#", "Yield Percent")
                        .Replace("#ChartSearies#", ChartSearies)
                        .Replace("#REDIRECTURL#", reurl);
                }
        }*/

        public static void ProjectWeeklyTrend(Controller ctrl, string ProjectKey)
        {
            var vmlist = ProjectYieldViewModule.GetYieldByWeeks(ProjectKey);
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
                    FINALTOOLTIP = FINALTOOLTIP+ "'<!doctype html><table>"
                        + "<tr><td><b>FPY</b></td><td>" + fyieldlist[idx].ToString("0.00") + "&#37;</td></tr>"
                        + "<tr><td><b>FY</b></td><td>" + ryieldlist[idx].ToString("0.00") + "&#37;</td></tr>";

                    foreach (var d in vmlist[idx].LastYields)
                    {
                        FINALTOOLTIP = FINALTOOLTIP + "<tr><td><b>" + d.WhichTest + "</b></td><td>Input:</td><td>" + d.InputCount.ToString() + "</td><td>Output:</td><td>" + d.OutputCount.ToString() + "</td></tr>";
                    }

                    FINALTOOLTIP =  FINALTOOLTIP + "</table>'";
                    FINALTOOLTIP = FINALTOOLTIP + ",";
                }
                FINALTOOLTIP = FINALTOOLTIP.Substring(0, FINALTOOLTIP.Length - 1);


                for (var idx = 0; idx < rtyieldlist.Count; idx++)
                {
                    REALTIMETOOLTIP = REALTIMETOOLTIP+ "'<!doctype html><table>"
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
                var reurl = "window.location.href = '/Project/ProjectWYieldDetail?ProjectKey=" + ProjectKey + "'" + "+'&EndDate='+this.category";

                var tempscript = System.IO.File.ReadAllText(ctrl.Server.MapPath("~/Scripts/SuperYield.xml"));
                ctrl.ViewBag.chartscript = tempscript.Replace("#ElementID#", "weeklyyield")
                    .Replace("#Title#", "Weekly Yiled Trend")
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

        public ActionResult ProjectYield(string ProjectKey)
        {
            if (ProjectKey != null)
            {
                ViewBag.pjkey = ProjectKey;

                ProjectWeeklyTrend(this, ProjectKey);

                return View();
            }
            return View();
        }

        public ActionResult ProjectWYieldDetail(string ProjectKey, string EndDate)
        {
            if (!string.IsNullOrEmpty(ProjectKey) && !string.IsNullOrEmpty(EndDate))
            {
                var edate = DateTime.Parse(DateTime.Parse(EndDate).ToString("yyyy-MM-dd") + " 07:30:00");
                var sdate = edate.AddDays(-7);
                if (sdate.DayOfWeek != DayOfWeek.Thursday)
                {
                    for (int i = 6; i > 0; i--)
                    {
                        sdate = edate.AddDays(0-i);
                        if (sdate.DayOfWeek == DayOfWeek.Thursday)
                        {
                            break;
                        }
                    }
                }

                var firstdatalist = new List<KeyValuePair<string, int>>();
                var retestdatalist = new List<KeyValuePair<string, int>>();

                var pvm = ProjectViewModels.RetrieveOneProject(ProjectKey);
                var yieldvm = ProjectYieldViewModule.GetYieldByDateRange(ProjectKey, sdate.ToString(), edate.ToString(),pvm);

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
                        if(piedatadict[k] > 0)
                            namevaluepair = namevaluepair + "{ name:'" + k + "',y:" + piedatadict[k].ToString() + "},";
                    }

                    namevaluepair = namevaluepair.Substring(0, namevaluepair.Length - 1);

                    var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/PieChart.xml"));
                    ViewBag.fchartscript = tempscript.Replace("#ElementID#", "ffailurepie")
                        .Replace("#Title#","First Failure")
                        .Replace("#SERIESNAME#", "FFailure")
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

                    retestdatalist = piedatadict.ToList();

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
                    ViewBag.rchartscript = tempscript.Replace("#ElementID#", "rfailurepie")
                        .Replace("#Title#", "Final Failure")
                        .Replace("#SERIESNAME#", "RFailure")
                        .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                }

                if (firstdatalist.Count > 0)
                {
                    firsttestparetofun(firstdatalist);
                }

                if (retestdatalist.Count > 0)
                {
                    retestparetofun(retestdatalist);
                }

                return View(yieldvm);
            }
            return View();
        }

        public ActionResult ProjectRMA(string ProjectKey)
        {
            if (ProjectKey != null)
            {
                var list1 = IssueViewModels.RetrieveIssueTypeByProjectKey(ProjectKey, Resolute.Pending,ISSUETP.RMA);
                var list2 = IssueViewModels.RetrieveIssueTypeByProjectKey(ProjectKey, Resolute.Working,ISSUETP.RMA);
                var list3 = IssueViewModels.RetrieveIssueTypeByProjectKey(ProjectKey, Resolute.Done,ISSUETP.RMA);
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
                var list1 = IssueViewModels.RetrieveIssueTypeByProjectKey(ProjectKey, Resolute.Pending, ISSUETP.RMA);
                var list2 = IssueViewModels.RetrieveIssueTypeByProjectKey(ProjectKey, Resolute.Working, ISSUETP.RMA);
                var list3 = IssueViewModels.RetrieveIssueTypeByProjectKey(ProjectKey, Resolute.Done, ISSUETP.RMA);
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

        public ActionResult ProjectOBA(string ProjectKey)
        {
            if (ProjectKey != null)
            {
                var list1 = IssueViewModels.RetrieveIssueTypeByProjectKey(ProjectKey, Resolute.Pending, ISSUETP.OBA);
                var list2 = IssueViewModels.RetrieveIssueTypeByProjectKey(ProjectKey, Resolute.Working, ISSUETP.OBA);
                var list3 = IssueViewModels.RetrieveIssueTypeByProjectKey(ProjectKey, Resolute.Done, ISSUETP.OBA);
                list1.AddRange(list2);
                list1.AddRange(list3);

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
                var list1 = IssueViewModels.RetrieveIssueTypeByProjectKey(ProjectKey, Resolute.Pending, ISSUETP.Quality);
                var list2 = IssueViewModels.RetrieveIssueTypeByProjectKey(ProjectKey, Resolute.Working, ISSUETP.Quality);
                var list3 = IssueViewModels.RetrieveIssueTypeByProjectKey(ProjectKey, Resolute.Done, ISSUETP.Quality);
                list1.AddRange(list2);
                list1.AddRange(list3);

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
                        .Replace("#Title#", "Monthly Yiled")
                        .Replace("#ChartxAxisValues#", ChartxAxisValues)
                        .Replace("#yAxisTitle#", "Yield Percent")
                        .Replace("#ChartSearies#", ChartSearies)
                        .Replace("#REDIRECTURL#", reurl);
                }

                return View();
            }
            return View();

        }*/

        public ActionResult ProjectMonthlyYield(string ProjectKey)
        {
            if (ProjectKey != null)
            {
                ViewBag.pjkey = ProjectKey;

                var vmlist = ProjectYieldViewModule.GetYieldByMonth(ProjectKey);
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
                    var reurl = "window.location.href = '/Project/ProjectMYieldDetail?ProjectKey=" + ProjectKey + "'" + "+'&EndDate='+this.category";

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
                ViewBag.pjkey = ProjectKey;
                var vmlist = ProjectYieldViewModule.GetYieldByDay(ProjectKey, StartDate, EndDate);
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
                    var reurl = "window.location.href = '/Project/ProjectDYieldDetail?ProjectKey=" + ProjectKey + "'" + "+'&EndDate='+this.category+'&VStartDate='+'" + StartDate+ "'+'&VEndDate='+'"+EndDate+"'";

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

        public ActionResult ProjectDYieldDetail(string ProjectKey, string EndDate,string VStartDate,string VEndDate)
        {
            if (!string.IsNullOrEmpty(ProjectKey) && !string.IsNullOrEmpty(EndDate))
            {

                ViewBag.StartDate = VStartDate;
                ViewBag.EndDate = VEndDate;

                var edate = DateTime.Parse(DateTime.Parse(EndDate).AddDays(1).ToString("yyyy-MM-dd") + " 07:30:00");
                var sdate = edate.AddDays(-1);
                var firstdatalist = new List<KeyValuePair<string, int>>();
                var retestdatalist = new List<KeyValuePair<string, int>>();

                var pvm = ProjectViewModels.RetrieveOneProject(ProjectKey);
                var yieldvm = ProjectYieldViewModule.GetYieldByDateRange(ProjectKey, sdate.ToString(), edate.ToString(), pvm);

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

                    retestdatalist = piedatadict.ToList();

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
                    ViewBag.rchartscript = tempscript.Replace("#ElementID#", "rfailurepie")
                        .Replace("#Title#", "Final Failure")
                        .Replace("#SERIESNAME#", "RFailure")
                        .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                }

                if (firstdatalist.Count > 0)
                {
                    firsttestparetofun(firstdatalist);
                }

                if (retestdatalist.Count > 0)
                {
                    retestparetofun(retestdatalist);
                }

                return View(yieldvm);
            }
            return View();
        }

        public ActionResult ProjectMYieldDetail(string ProjectKey, string EndDate)
        {
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

                var pvm = ProjectViewModels.RetrieveOneProject(ProjectKey);
                var yieldvm = ProjectYieldViewModule.GetYieldByDateRange(ProjectKey, sdate.ToString(), edate.ToString(), pvm);

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

                    retestdatalist = piedatadict.ToList();

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
                    ViewBag.rchartscript = tempscript.Replace("#ElementID#", "rfailurepie")
                        .Replace("#Title#", "Final Failure")
                        .Replace("#SERIESNAME#", "RFailure")
                        .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                }

                if (firstdatalist.Count > 0)
                {
                    firsttestparetofun(firstdatalist);
                }

                if (retestdatalist.Count > 0)
                {
                    retestparetofun(retestdatalist);
                }

                return View(yieldvm);
            }
            return View();
        }

        /*public ActionResult ProjectPeriodYield(string ProjectKey,string StartDate,string EndDate)
        {
            if (!string.IsNullOrEmpty(ProjectKey) && !string.IsNullOrEmpty(StartDate) && !string.IsNullOrEmpty(EndDate))
            {
                ViewBag.pjkey = ProjectKey;

                var pvm = ProjectViewModels.RetrieveOneProject(ProjectKey);

                var vmlist = new List<ProjectYieldViewModule>();
                var tempret = ProjectYieldViewModule.GetYieldByDateRange(ProjectKey, StartDate, EndDate, pvm);
                if (tempret.FirstYields.Count > 0)
                {
                    vmlist.Add(tempret);
                }

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
                    var tempurl = "/Project/ProjectPYieldDetail?ProjectKey="+ ProjectKey + "&StartDate="+ StartDate + "&EndDate=" + EndDate;
                    var reurl = "window.location.href = '"+ tempurl+"'";

                    var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/ColumnChart.xml"));
                    ViewBag.chartscript = tempscript.Replace("#ElementID#", "periodyield")
                        .Replace("#ChartType#", "column")
                        .Replace("#Title#", "Period Yiled")
                        .Replace("#ChartxAxisValues#", ChartxAxisValues)
                        .Replace("#yAxisTitle#", "Yield Percent")
                        .Replace("#ChartSearies#", ChartSearies)
                        .Replace("#REDIRECTURL#", reurl);
                }

                return View();
            }
            return View();
        }*/

        public ActionResult ProjectPeriodYield(string ProjectKey, string StartDate, string EndDate)
        {
            if (!string.IsNullOrEmpty(ProjectKey) && !string.IsNullOrEmpty(StartDate) && !string.IsNullOrEmpty(EndDate))
            {
                ViewBag.pjkey = ProjectKey;

                var pvm = ProjectViewModels.RetrieveOneProject(ProjectKey);

                var vmlist = new List<ProjectYieldViewModule>();
                var tempret = ProjectYieldViewModule.GetYieldByDateRange(ProjectKey, StartDate, EndDate, pvm);
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
                            if (d.InputCount > tempfamount){ tempfamount = d.InputCount;}
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

        private void firsttestparetofun(List<KeyValuePair<string, int>> firstdatalist)
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

                var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/ParetoChart.xml"));
                ViewBag.fparetoscript = tempscript.Replace("#ElementID#", "fparetochart")
                    .Replace("#Title#", "Pareto of First Test Defect")
                    .Replace("#XAxisTitle#", "Defect")
                    .Replace("#ChartxAxisValues#", ChartxAxisValues)
                    .Replace("#AmountMAX#", sum.ToString())
                    .Replace("#PCount#", pcountvalue)
                    .Replace("#ABPercent#", abpecentvalue)
                    .Replace("#PPercent#", ppecentvalue);
            }
        }

        private void retestparetofun(List<KeyValuePair<string, int>> retestdatalist)
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

                var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/ParetoChart.xml"));
                ViewBag.rparetoscript = tempscript.Replace("#ElementID#", "rparetochart")
                    .Replace("#Title#", "Pareto of Final Defect")
                    .Replace("#XAxisTitle#", "Defect")
                    .Replace("#ChartxAxisValues#", ChartxAxisValues)
                    .Replace("#AmountMAX#", sum.ToString())
                    .Replace("#PCount#", pcountvalue)
                    .Replace("#ABPercent#", abpecentvalue)
                    .Replace("#PPercent#", ppecentvalue);
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

                var pvm = ProjectViewModels.RetrieveOneProject(ProjectKey);
                var yieldvm = ProjectYieldViewModule.GetYieldByDateRange(ProjectKey, StartDate, EndDate, pvm);

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

                    retestdatalist = piedatadict.ToList();

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
                    ViewBag.rchartscript = tempscript.Replace("#ElementID#", "rfailurepie")
                        .Replace("#Title#", "Final Failure")
                        .Replace("#SERIESNAME#", "RFailure")
                        .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                }

                if (firstdatalist.Count > 0)
                {
                    firsttestparetofun(firstdatalist);
                }

                if (retestdatalist.Count > 0)
                {
                    retestparetofun(retestdatalist);
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
                var vm = ProjectErrorViewModels.RetrieveErrorByErrorKey(key);
                var FirstEngineer = ProjectViewModels.RetrieveOneProject(vm[0].ProjectKey).FirstEngineer;
                var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
                if (string.Compare(FirstEngineer, updater, true) == 0)
                {
                    ViewBag.assigee = true;
                }
                else
                {
                    ViewBag.assigee = false;
                }
                return View(vm[0]);
            }
            
            return View();
        }

        public ActionResult DeleteErrorComment(string ErrorKey, string CommentType, string Date)
        {
            if (!string.IsNullOrEmpty(ErrorKey) && !string.IsNullOrEmpty(CommentType) && !string.IsNullOrEmpty(Date) )
            {
                ProjectErrorViewModels.DeleteErrorComment(ErrorKey, CommentType, Date);
                var dict = new RouteValueDictionary();
                dict.Add("ErrorKey", ErrorKey);
                return RedirectToAction("UpdateProjectError", "Project",dict);
            }
            return RedirectToAction("ViewAll", "Project");
        }

        public ActionResult UpdateProjectError2(string ProjectKey,string ErrorCode)
        {
            var tempvm = ProjectErrorViewModels.RetrieveErrorByPJKey(ProjectKey, ErrorCode);
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
                var vm = ProjectErrorViewModels.RetrieveErrorByErrorKey(key);
                var FirstEngineer = ProjectViewModels.RetrieveOneProject(vm[0].ProjectKey).FirstEngineer;
                var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
                if (string.Compare(FirstEngineer, updater, true) == 0)
                {
                    ViewBag.assigee = true;
                }
                else
                {
                    ViewBag.assigee = false;
                }

                return View("UpdateProjectError",vm[0]);
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

        [HttpPost, ActionName("UpdateProjectError")]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProjectErrorPost()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

            var vm = new ProjectErrorViewModels();
            vm.ErrorKey = Request.Form["ErrorKey"];
            vm.ShortDesc = Request.Form["ShortDesc"];
            vm.Reporter = ckdict["logonuser"].Split(new char[] { '|' })[0];            

            vm.UpdateShortDesc();

            var temphtml = Request.Form["editor1"];
            if (!string.IsNullOrEmpty(temphtml))
            {
                vm.Description = Server.HtmlDecode(temphtml);
                ProjectErrorViewModels.StoreErrorComment(vm.ErrorKey,vm.dbDescription,PJERRORCOMMENTTYPE.Description,vm.Reporter,DateTime.Now.ToString());
                UserRankViewModel.UpdateUserRank(updater, 2);
            }

            if (Request.Form["editor2"] != null)
            {
                var com = new ErrorComments();
                com.Comment = Server.HtmlDecode(Request.Form["editor2"]);
                if (!string.IsNullOrEmpty(com.Comment))
                {
                    ProjectErrorViewModels.StoreErrorComment(vm.ErrorKey, com.dbComment, PJERRORCOMMENTTYPE.RootCause, vm.Reporter, DateTime.Now.ToString());
                    UserRankViewModel.UpdateUserRank(updater, 5);
                }
            }

            if (Request.Form["editor3"] != null)
            {
                var com = new ErrorComments();
                com.Comment = Server.HtmlDecode(Request.Form["editor3"]);
                if (!string.IsNullOrEmpty(com.Comment))
                {
                    if (!string.IsNullOrEmpty(com.Comment))
                    {
                        ProjectErrorViewModels.StoreErrorComment(vm.ErrorKey, com.dbComment, PJERRORCOMMENTTYPE.FailureDetail, vm.Reporter, DateTime.Now.ToString());
                        UserRankViewModel.UpdateUserRank(updater, 2);
                    }
                }
            }

            if (Request.Form["resulteditor"] != null)
            {
                var com = new ErrorComments();
                com.Comment = Server.HtmlDecode(Request.Form["resulteditor"]);
                if (!string.IsNullOrEmpty(com.Comment))
                {
                    if (!string.IsNullOrEmpty(com.Comment))
                    {
                        ProjectErrorViewModels.StoreErrorComment(vm.ErrorKey, com.dbComment, PJERRORCOMMENTTYPE.Result, vm.Reporter, DateTime.Now.ToString());
                        UserRankViewModel.UpdateUserRank(updater, 2);
                    }
                }
            }

            if (!string.IsNullOrEmpty(Request.Form["attachmentupload"]))
            {
                var urls = ReceiveAttachFiles();
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
                    ProjectErrorViewModels.StoreErrorAttachment(vm.ErrorKey, url);
                    UserRankViewModel.UpdateUserRank(updater, 5);
                }
            }

            var tempvm = ProjectErrorViewModels.RetrieveErrorByErrorKey(vm.ErrorKey);
            var FirstEngineer = ProjectViewModels.RetrieveOneProject(tempvm[0].ProjectKey).FirstEngineer;
            
            if (string.Compare(FirstEngineer, updater, true) == 0)
            {
                ViewBag.assigee = true;
            }
            else
            {
                ViewBag.assigee = false;
            }

            return View(tempvm[0]);
        }

        public ActionResult ErrorAttach(string ErrorKey)
        {
            if (!string.IsNullOrEmpty(ErrorKey))
            {
                var tempvm = ProjectErrorViewModels.RetrieveErrorByErrorKey(ErrorKey);
                return View(tempvm[0]);
            }
            return RedirectToAction("ViewAll", "Project");
        }

        public ActionResult DeleteErrorAttachment(string errorkey,string filename)
        {
            if (!string.IsNullOrEmpty(errorkey) && !string.IsNullOrEmpty(filename))
            {
                var tempvm = ProjectErrorViewModels.RetrieveErrorByErrorKey(errorkey);
                var FirstEngineer = ProjectViewModels.RetrieveOneProject(tempvm[0].ProjectKey).FirstEngineer;
                var ckdict = CookieUtility.UnpackCookie(this);
                var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

                if (string.Compare(FirstEngineer, updater, true) == 0)
                {
                    ProjectErrorViewModels.DeleteAttachment(errorkey,filename);
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

                foreach (var item in pjkeylist)
                {
                    var pjval = ProjectViewModels.RetrieveOneProject(item);
                    if (string.Compare(pjval.MonitorVcsel,"False",true) == 0)
                    {
                        continue;
                    }
                    var warningyield = Convert.ToDouble(pjval.VcselWarningYield) * 0.01;


                    var waferlist = BITestData.RetrieveAllWafer(item);

                    
                    foreach (var w in waferlist)
                    {
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
                                + " Total Input: "+ alldict.Count.ToString() + " ,Failed Modules: " + errdict.Count.ToString()+" ,Reviewed Modules: " + cordict.Count.ToString() + " \r\n\r\n";

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
                                logcontent = logcontent + sn.Key+"\r\n";
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
                    validatestr = validatestr.Split(new string[] { "/Issue" }, StringSplitOptions.None)[0]+ url;
                    validatestr = validatestr.Replace("//localhost", "//" + netcomputername);

                    var toaddrs = new List<string>();
                    toaddrs.Add("windy.ju@finisar.com");
                    toaddrs.Add("daly.li@finisar.com");
                    toaddrs.Add("tyler.zhang@finisar.com");
                    toaddrs.Add("tony.lv@finisar.com");
                    toaddrs.Add("Zhongxi.Yu@finisar.com");
                    toaddrs.Add("Zhijun.Chen@finisar.com");
                    //toaddrs.Add("brad.qiu@finisar.com");
                    EmailUtility.SendEmail("VCSEL WAFER YIELD WARNING", toaddrs, content1 + "\r\nWafer SN File: " + validatestr);
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
                        try{ netcomputername = System.Net.Dns.GetHostName();}
                        catch (Exception ex) { }
                        validatestr = validatestr.Replace("//localhost", "//" + netcomputername);

                        var content = "Click below link to review the Report which you booked :\r\n " + validatestr;

                        var toaddrs = new List<string>();
                        toaddrs.Add(record.username);
                        EmailUtility.SendEmail("Parallel NPI Trace Notice", toaddrs, content);
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
                try {
                        var fvissues = IssueViewModels.Retrieve_Alert_RMAByProjectKey(pjkey, true);

                        foreach (var item in fvissues)
                        { item.UpdateAlertEmailDate();}
                            
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

                        var content = "RMA FA of "+item.ModuleSN+" must finished today :\r\n " + validatestr;

                            var toaddrs = new List<string>();
                            toaddrs.Add(item.Reporter);
                            toaddrs.Add(item.Assignee);
                            EmailUtility.SendEmail("RMA Trace Notice", toaddrs, content);
                            new System.Threading.ManualResetEvent(false).WaitOne(300);
                    }
                } catch (Exception ex) { }


                try {
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
                            EmailUtility.SendEmail("RMA Trace Notice", toaddrs, content);
                            new System.Threading.ManualResetEvent(false).WaitOne(300);
                    }
                } catch (Exception ex) { }

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
                    EmailUtility.SendEmail("OBA Trace Notice", toaddrs, content);
                    new System.Threading.ManualResetEvent(false).WaitOne(300);
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
                SendBookedReportNotice();
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
                SendRMAAlertEmail();
                SendOBAAlertEmail();
            }
            catch (Exception ex) { }

            
             
            foreach (var pjkey in pjkeylist)
            {
                try
                {
                    ProjectTestData.PrePareMESLatestData(pjkey);
                }
                catch (Exception ex)
                { }
            }

            foreach (var pjkey in pjkeylist)
            {
                try
                {
                    BITestData.PrePareLatestData(pjkey);
                }
                catch (Exception ex)
                { }
            }


            foreach (var pjkey in pjkeylist)
            {
                try
                {
                    ProjectTestData.PrePareATELatestData(pjkey);
                }
                catch (Exception ex)
                { }
            }

            try
            {
                var filename = "log" + DateTime.Now.ToString("yyyy-MM-dd");
                var wholefilename = Server.MapPath("~/userfiles") + "\\" + filename;

                var content = System.IO.File.ReadAllText(wholefilename);
                content = content+ "heart beat end @ "+DateTime.Now.ToString()+"\r\n";
                System.IO.File.WriteAllText(wholefilename, content);
            }
            catch (Exception ex)
            { }

            return View();
        }

        public ActionResult HeartBeat2()
        {
            //var pjkeylist = ProjectViewModels.RetrieveAllProjectKey();
            //foreach (var pjkey in pjkeylist)
            //{
            //    try
            //    {
            //        ProjectTestData.PrePareATELatestData(pjkey);
            //    }
            //    catch (Exception ex)
            //    { }
            //}
            ShareDocVM.MatchAllYesterdayDoc(this);
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
            
            var title = new List<string>();
            title.Add("Part Num");
            vm.Add(title);

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
                    var partnums = MESUtility.RetrieveAllPN(pndeslist);
                    foreach (var pt in partnums)
                    {
                        var templine = new List<string>();
                        templine.Add(pt);
                        vm.Add(templine);
                    }
                }
            }

            return View(vm);
        }

        public ActionResult RetrieveWaferData(string pjkey)
        {
            if (!string.IsNullOrEmpty(pjkey))
            {
                try
                {
                    BITestData.RetrieveWaferDataFromMes(pjkey);
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

            var ret = IssueViewModels.RetrieveIssueByIssueKey(key);
            if (ret != null)
            {
                return View(ret);
            }
            else
            { return View(); }
        }


        private void SendTaskEvent(IssueViewModels vm, string comment,List<string> addrlist)
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

            var content = "Hi All,\r\n\r\nThis is a LYT(low yield trigger) information. Please pay your attention to it. Thanks!\r\n\r\nFailure mode: " + vm.Summary + "\r\n\r\nLYT Reason:\r\n\r\n" + comment + " \r\n\r\nDetail Information: " + validatestr;

            var toaddrs = new List<string>();
            toaddrs.AddRange(addrlist);
            toaddrs.Add(vm.Assignee);
            EmailUtility.SendEmail("Parallel Project LYT", toaddrs, content);
            IssueViewModels.UpdateLYT(vm.IssueKey);
            new System.Threading.ManualResetEvent(false).WaitOne(2000);
        }

        [HttpPost, ActionName("LowYieldWarn")]
        [ValidateAntiForgeryToken]
        public ActionResult LowYieldWarnPost()
        {
            var issuekey = Request.Form["IssueKey"];
            var vm = IssueViewModels.RetrieveIssueByIssueKey(issuekey);
            if (vm != null)
            {
                var ckdict = CookieUtility.UnpackCookie(this);
                var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

                if (string.Compare(vm.Assignee, updater, true) == 0
                    && Request.Form["sendisu"] != null)
                {
                    var comment = Request.Form["commentcontent"];
                    var addrs = Request.Form["RPeopleAddr"].Split(new string[] {";"},StringSplitOptions.RemoveEmptyEntries);
                    var addrlist = new List<string>();
                    addrlist.AddRange(addrs);

                    SendTaskEvent(vm, comment, addrlist);

                    var dict = new RouteValueDictionary();
                    dict.Add("ProjectKey", vm.ProjectKey);
                    return RedirectToAction("ProjectDetail", "Project", dict);
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

    }
}