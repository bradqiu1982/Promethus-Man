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
            var projlist = ProjectViewModels.RetrieveAllProject();

            foreach (var item in projlist)
            {
                var startdate = DateTime.Now.AddDays(-7);
                var enddate = DateTime.Now.ToString();
                if (startdate.DayOfWeek != DayOfWeek.Thursday)
                {
                    for (int i = 6; i > 0; i--)
                    {
                        startdate = startdate.AddDays(0 - i);
                        if (startdate.DayOfWeek == DayOfWeek.Thursday)
                        {
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

                var ivmlist = IssueViewModels.RetrieveNPIPROCIssue(item.ProjectKey);
                foreach (var iv in ivmlist)
                {
                    if (iv.Summary.Contains(" DVT"))
                    {
                        item.DVTIssueKey = iv.IssueKey;
                        item.DVTDate = iv.DueDate.ToString("yyyy-MM-dd");
                        item.DVTStatus = iv.Resolution;
                    }
                    if (iv.Summary.Contains(" MVT"))
                    {
                        item.MVTIssueKey = iv.IssueKey;
                        item.MVTDate = iv.DueDate.ToString("yyyy-MM-dd");
                        item.MVTStatus = iv.Resolution;
                    }
                }

                item.PendingTaskCount = IssueViewModels.RetrieveTaskCountByProjectKey(item.ProjectKey, Resolute.Pending);
                item.PendingFACount = ProjectFAViewModules.RetrieveFADataCount(item.ProjectKey);
                item.PendingRMACount = IssueViewModels.RetrieveRMACountByProjectKey(item.ProjectKey, Resolute.Pending);
            }

            return View(projlist);
        }

        private List<SelectListItem> CreateSelectList(List<string> valist, string defVal)
        {
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
                projectmodel.ProjectKey = tempstr.Substring(0, (tempstr.Length > 20) ? 18 : tempstr.Length).ToUpper();
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
            RetrievePorjectKey(projectmodel);

            RetrieveProjectMember(projectmodel);
            RetrieveProjectDesc(projectmodel);

            StoreMesConfig(projectmodel);
            RetrievePNs(projectmodel);
            RetrieveStation(projectmodel);

            if (!RetrieveProjectDate(projectmodel))
            {
                CreateAllUserLists(projectmodel);
                return View(projectmodel);
            }


            if (!ProjectValidate(projectmodel))
            {
                CreateAllUserLists(projectmodel);
                return View(projectmodel);
            }

            projectmodel.StoreProject();

            IssueViewModels.CreateNPIProcTasks(projectmodel.ProjectName, projectmodel.ProjectKey, projectmodel.MemberList[1].Name);

            var ckdict = CookieUtility.UnpackCookie(this);
            var who = (ckdict["logonuser"]).Split(new string[]{ "||"},StringSplitOptions.None)[0];
            ProjectEvent.CreateProjectEvent(who, projectmodel.ProjectKey, projectmodel.ProjectName);

            //new Thread(() => {
            //    try
            //    {
                    MESUtility.StartProjectBonding(projectmodel);
            //    }
            //    catch (Exception ex)
            //    { }
            //}).Start();

            return RedirectToAction("ViewAll");
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
                pndict.Add(pn.Pn, true);
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
                stationdict.Add(stat.Station, true);
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

        [HttpPost, ActionName("EditProject")]
        [ValidateAntiForgeryToken]
        public ActionResult EditProjectPost()
        {
            var projectmodel = new ProjectViewModels();
            projectmodel.ProjectName = Request.Form["ProjectName"];
            try {projectmodel.FinishRating = Convert.ToDouble(Request.Form["FinishRating"])%100; }
            catch(Exception ex) { projectmodel.FinishRating = 0; }
            RetrievePorjectKey(projectmodel);

            RetrieveProjectMember(projectmodel);
            RetrieveProjectDesc(projectmodel);

            StoreMesConfig(projectmodel);
            RetrievePNs(projectmodel);
            RetrieveStation(projectmodel);

            if (!RetrieveProjectDate(projectmodel))
            {
                CreateAllUserLists(projectmodel);
                return View(projectmodel);
            }
                

            if (!ProjectValidate(projectmodel,true))
            {
                CreateAllUserLists(projectmodel);
                return View(projectmodel);
            }

            var oldpjdata = ProjectViewModels.RetrieveOneProject(projectmodel.ProjectKey);

            bool databondingchange = DataBondingChanged(oldpjdata, projectmodel);

            projectmodel.StoreProject();

            if (projectmodel.TabList.Count == 0)
            {
                projectmodel.TabList = oldpjdata.TabList;
            }
            
                        
            var ckdict = CookieUtility.UnpackCookie(this);
            var who = (ckdict["logonuser"]).Split(new string[] { "||" }, StringSplitOptions.None)[0];
            ProjectEvent.UpdateProjectEvent(who, projectmodel.ProjectKey, projectmodel.ProjectName);

            if(databondingchange)
            {
                //new Thread(() =>
                //{
                //    try
                //    {
                        MESUtility.StartProjectBonding(projectmodel);
                //    }
                //    catch (Exception ex)
                //    { }
                //}).Start();
            }

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
                        for (int i = 6; i > 0; i--)
                        {
                            startdate = startdate.AddDays(0 - i);
                            if (startdate.DayOfWeek == DayOfWeek.Thursday)
                            {
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

                    var ivmlist = IssueViewModels.RetrieveNPIPROCIssue(vm.ProjectKey);
                    foreach (var iv in ivmlist)
                    {
                        if (iv.Summary.Contains(" DVT"))
                        {
                            vm.DVTIssueKey = iv.IssueKey;
                            vm.DVTDate = iv.DueDate.ToString("yyyy-MM-dd");
                            vm.DVTStatus = iv.Resolution;
                        }
                        if (iv.Summary.Contains(" MVT"))
                        {
                            vm.MVTIssueKey = iv.IssueKey;
                            vm.MVTDate = iv.DueDate.ToString("yyyy-MM-dd");
                            vm.MVTStatus = iv.Resolution;
                        }
                    }

                    vm.PendingTaskCount = IssueViewModels.RetrieveTaskCountByProjectKey(vm.ProjectKey, Resolute.Pending);
                    vm.PendingFACount = ProjectFAViewModules.RetrieveFADataCount(vm.ProjectKey);
                    vm.PendingRMACount = IssueViewModels.RetrieveRMACountByProjectKey(vm.ProjectKey, Resolute.Pending);
                }

                return View(vm);
            }
            return View();
        }

        public ActionResult ProjectFA(string ProjectKey)
        {
            if (!string.IsNullOrEmpty(ProjectKey))
            {
                var vm  = ProjectFAViewModules.RetrieveFAData(ProjectKey);

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
                        .Replace("#Title#", ProjectKey + "FA Realtime Failure")
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
                var vm = ProjectFAViewModules.RetrieveFADataWithErrAbbr(ProjectKey, ErrAbbr);
                return View("ProjectFA",vm);
            }
            return View();
        }

        public ActionResult ProjectNPI(string ProjectKey)
        {
            if (ProjectKey != null)
            {
                var vmlist = IssueViewModels.RetrieveNPIPROCIssue(ProjectKey);
                return View(vmlist);
            }
            return View();
        }


        public ActionResult ProjectYield(string ProjectKey)
        {
            if (ProjectKey != null)
            {
                ViewBag.pjkey = ProjectKey;

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
                    var reurl = "window.location.href = '/Project/ProjectWYieldDetail?ProjectKey='+document.getElementById(\"pjkey\").value+'&EndDate='+this.category";

                    var tempscript = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/ColumnChart.xml"));
                    ViewBag.chartscript = tempscript.Replace("#ElementID#", "weeklyyield")
                        .Replace("#ChartType#", "column")
                        .Replace("#Title#", "Weekly Yiled")
                        .Replace("#ChartxAxisValues#", ChartxAxisValues)
                        .Replace("#yAxisTitle#", "Yield Percent")
                        .Replace("#ChartSearies#", ChartSearies)
                        .Replace("#REDIRECTURL#", reurl);
                }

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
                        .Replace("#Title#", "Retest Failure")
                        .Replace("#SERIESNAME#", "RFailure")
                        .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                }

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

                    for (var i = 0; i < firstdatalist.Count; i++)
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

                    for (var i = 0; i < retestdatalist.Count; i++)
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
                        .Replace("#Title#", "Pareto of Retest Defect")
                        .Replace("#XAxisTitle#", "Defect")
                        .Replace("#ChartxAxisValues#", ChartxAxisValues)
                        .Replace("#AmountMAX#", sum.ToString())
                        .Replace("#PCount#", pcountvalue)
                        .Replace("#ABPercent#", abpecentvalue)
                        .Replace("#PPercent#", ppecentvalue);
                }

                return View(yieldvm);
            }
            return View();
        }

        public ActionResult ProjectRMA(string ProjectKey)
        {
            if (ProjectKey != null)
            {
                var list1 = IssueViewModels.RetrieveRMAByProjectKey(ProjectKey, Resolute.Pending);
                var list2 = IssueViewModels.RetrieveRMAByProjectKey(ProjectKey, Resolute.Working);
                var list3 = IssueViewModels.RetrieveRMAByProjectKey(ProjectKey, Resolute.Done);
                list1.AddRange(list2);
                list1.AddRange(list3);
                return View(list1);
            }
            return View();
        }

        public ActionResult ProjectMonthlyYield(string ProjectKey)
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
                    var reurl = "window.location.href = '/Project/ProjectMYieldDetail?ProjectKey='+document.getElementById(\"pjkey\").value+'&EndDate='+this.category";

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
                        .Replace("#Title#", "Retest Failure")
                        .Replace("#SERIESNAME#", "RFailure")
                        .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                }

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

                    for (var i = 0; i < firstdatalist.Count; i++)
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

                    for (var i = 0; i < retestdatalist.Count; i++)
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
                        .Replace("#Title#", "Pareto of Retest Defect")
                        .Replace("#XAxisTitle#", "Defect")
                        .Replace("#ChartxAxisValues#", ChartxAxisValues)
                        .Replace("#AmountMAX#", sum.ToString())
                        .Replace("#PCount#", pcountvalue)
                        .Replace("#ABPercent#", abpecentvalue)
                        .Replace("#PPercent#", ppecentvalue);
                }

                return View(yieldvm);
            }
            return View();
        }

        public ActionResult ProjectPeriodYield(string ProjectKey,string StartDate,string EndDate)
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
                        .Replace("#Title#", "Retest Failure")
                        .Replace("#SERIESNAME#", "RFailure")
                        .Replace("#NAMEVALUEPAIRS#", namevaluepair);
                }

                if (firstdatalist.Count > 0)
                {
                    var peralist = new List<ParetoData>();

                    if (firstdatalist.Count > 1)
                    {
                        firstdatalist.Sort(delegate (KeyValuePair<string, int> pair1,KeyValuePair<string, int> pair2)
                        {
                            return pair2.Value.CompareTo(pair1.Value);
                        });
                    }

                    var sum = 0;
                    for (var i = 0; i < firstdatalist.Count; i++)
                    {
                        sum = sum + firstdatalist[i].Value;
                    }

                    for (var i = 0; i < firstdatalist.Count; i++)
                    {
                        var tempperato = new ParetoData();
                        tempperato.key = firstdatalist[i].Key;
                        if (i == 0)
                        {
                            tempperato.count = firstdatalist[i].Value;
                            tempperato.percent = tempperato.count /(double)sum;
                            tempperato.sumpercent = tempperato.percent;
                            peralist.Add(tempperato);
                        }
                        else
                        {
                            tempperato.count = firstdatalist[i].Value;
                            tempperato.percent = tempperato.count / (double)sum;
                            tempperato.sumpercent = peralist[peralist.Count -1].sumpercent + tempperato.percent;
                            peralist.Add(tempperato);
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
                        ppecentvalue = ppecentvalue + (item.sumpercent*100).ToString("0.0") + ",";
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

                    for (var i = 0; i < retestdatalist.Count; i++)
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
                        .Replace("#Title#", "Pareto of Retest Defect")
                        .Replace("#XAxisTitle#", "Defect")
                        .Replace("#ChartxAxisValues#", ChartxAxisValues)
                        .Replace("#AmountMAX#", sum.ToString())
                        .Replace("#PCount#", pcountvalue)
                        .Replace("#ABPercent#", abpecentvalue)
                        .Replace("#PPercent#", ppecentvalue);
                }

                return View(yieldvm);
            }
            return View();
        }


    }
}