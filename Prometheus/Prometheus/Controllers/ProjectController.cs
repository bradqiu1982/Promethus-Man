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

namespace Prometheus.Controllers
{
    public class ProjectController : Controller
    {
        // GET: Project
        public ActionResult ViewAll()
        {
            var projlist = ProjectViewModels.RetrieveAllProject();
            return View(projlist);
        }

        public ActionResult CreateProject()
        {
            var ckdict = UserController.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && ckdict["logonuser"] != "")
            {
                return View();
            }
            else
            {
                var ck = new Dictionary<string, string>();
                ck.Add("logonredirectctrl", "Project");
                ck.Add("logonredirectact", "CreateProject");
                UserController.SetCookie(this,ck);
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


        private bool ProjectValidate(ProjectViewModels projectmodel)
        {
            var createerror = "<h3><font color=\"red\">Fail to create project: <ErrorMsg></font></h3>";
            if (string.IsNullOrEmpty(projectmodel.ProjectName.Trim())
                || string.IsNullOrEmpty(projectmodel.ProjectKey.Trim()))
            {
                ViewBag.CreateError = createerror.Replace("<ErrorMsg>","Project Name is empty");
                return false;
            }

            foreach(var eg in projectmodel.MemberList)
            {
                if (string.IsNullOrEmpty(eg.Name.Trim()))
                {
                    ViewBag.CreateError = createerror.Replace("<ErrorMsg>", "PM or Engineer email address is empty");
                    return false;
                }
            }

            if (string.IsNullOrEmpty(projectmodel.Description.Trim()))
            {
                ViewBag.CreateError = createerror.Replace("<ErrorMsg>", "Project description is empty");
                return false;
            }

            if (projectmodel.CheckExistProject())
            {
                ViewBag.CreateError = createerror.Replace("<ErrorMsg>", "Project exist");
                return false;
            }

            foreach (var eg in projectmodel.MemberList)
            {
                if (!EmailAddressValidate(eg.Name.Trim()))
                {
                    ViewBag.CreateError = createerror.Replace("<ErrorMsg>", "PM or Engineer email address is wrong");
                    return false;
                }
            }
                
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
                    if (!string.IsNullOrEmpty(Request.Form[key + i]))
                    {
                        ret.Add(Request.Form[key + i]);
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

        public string PNCondition(List<ProjectPn> pns)
        {
            string ret = "('";
            foreach(var pn in pns)
            {
                ret = ret + RMSpectialCh(pn.Pn) + "','";
            }

            if (pns.Count > 0)
            {
                ret = ret.Substring(0, ret.Length - 2) + ")";
            }
            else
            {
                ret = "('')";
            }

            return ret;
        }

        private List<string> RetrieveSqlFromProjectModel(ProjectViewModels projectmodel)
        {
            var tables = new List<string>();
            foreach(var s in projectmodel.StationList)
            {
                foreach(var m in projectmodel.TabList)
                {
                    if (string.Compare(s.Station.ToUpper(),m.Station.ToUpper()) == 0)
                    {
                        tables.Add(m.TableName);
                        break;
                    }
                }
            }

            string pncond = PNCondition(projectmodel.PNList);

            var sql = "select ModuleSerialNum, WhichTest, ModuleType, ErrAbbr, TestTimeStamp, TestStation from  #DCTABLE#  where assemblypartnum in  #COND#   #TIMECOND#  order by  moduleserialnum,testtimestamp";

            var ret = new List<string>();
            foreach (var tb in tables)
            {
                ret.Add(sql.Replace("#DCTABLE#", ("insite.dc_" + tb)).Replace("#COND#", pncond));
            }
            return ret;
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
                    ViewBag.CreateError = "<h3><font color=\"red\">Fail to create project: "+ RMSpectialCh(ex.ToString())+"</font></h3>";
                    return false;
                }
            }
            else
            {
                ViewBag.CreateError = ViewBag.CreateError = "<h3><font color=\"red\">Fail to create project: StartDate is empty</font></h3>";
                return false;
            }
        }

        private void RetrievePNs(ProjectViewModels projectmodel)
        {
            var pns = RetrieveProjectInfo("PN", 9);
            if (pns.Count > 0)
            {
                var lpn = new List<ProjectPn>();
                foreach (var p in pns)
                {
                    lpn.Add(new ProjectPn(projectmodel.ProjectKey, p));
                }
                projectmodel.PNList = lpn;
            }
        }

        private void RetrieveStation(ProjectViewModels projectmodel)
        {
            var stats = RetrieveProjectInfo("Station", 9);
            if (stats.Count > 0)
            {
                var lstat = new List<ProjectStation>();
                foreach (var s in stats)
                {
                    lstat.Add(new ProjectStation(projectmodel.ProjectKey, s));
                }
                projectmodel.StationList = lstat;
            }
        }



        [HttpPost, ActionName("CreateProject")]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePostProject()
        {
            var projectmodel = new ProjectViewModels();
            projectmodel.ProjectName = Request.Form["ProjectName"];
            RetrievePorjectKey(projectmodel);

            RetrieveProjectMember(projectmodel);
            RetrieveProjectDesc(projectmodel);

            StoreMesConfig(projectmodel);
            RetrievePNs(projectmodel);
            RetrieveStation(projectmodel);

            if (!RetrieveProjectDate(projectmodel))
                return View(projectmodel);

            if (!ProjectValidate(projectmodel))
                return View(projectmodel);

            if(projectmodel.StationList.Count > 0
                && projectmodel.TabList.Count >0
                && projectmodel.PNList.Count > 0)
            {
                var sqls = RetrieveSqlFromProjectModel(projectmodel);
                foreach (var s in sqls)
                {
                    var sql = s.Replace("#TIMECOND#", "and TestTimeStamp > '"+DateTime.Parse("2016-7-18 4:00:00").ToString()+"'");
                    System.Windows.MessageBox.Show(sql);
                    var res = DBUtility.ExeMESSqlWithRes(sql);
                    foreach (var r in res)
                    {
                        var l = r;
                    }
                }
            }

            projectmodel.StoreProject();

            return RedirectToAction("ViewAll");
        }

        public ActionResult EditProject(string ProjectKey)
        {
            var ckdict = UserController.UnpackCookie(this);
            if (!string.IsNullOrEmpty(ProjectKey) || ckdict.ContainsKey("ProjectKey"))
            {
                var realkey = "";
                if (!string.IsNullOrEmpty(ProjectKey))
                    realkey = ProjectKey;
                else
                    realkey = ckdict["ProjectKey"];

                if (ckdict.ContainsKey("logonuser") && ckdict["logonuser"] != "")
                {
                    var vm = ProjectViewModels.RetrieveOneProject(realkey);
                    return View(vm);
                }
                else
                {
                    var ck = new Dictionary<string, string>();
                    ck.Add("logonredirectctrl", "Project");
                    ck.Add("logonredirectact", "EditProject");
                    ck.Add("ProjectKey", realkey);
                    UserController.SetCookie(this, ck);
                    return RedirectToAction("LoginUser", "User");
                }

            }
            else
            {
                return RedirectToAction("CreateProject","Project");
            }
        }

        [HttpPost, ActionName("EditProject")]
        [ValidateAntiForgeryToken]
        public ActionResult EditProjectPost()
        {
            if (Request.Form["update"] != null)
            {
                System.Windows.MessageBox.Show("try to update");
            }

            if (Request.Form["delete"] != null)
            {
                System.Windows.MessageBox.Show("try to delete");
            }

            return RedirectToAction("ViewAll");
        }
    }
}