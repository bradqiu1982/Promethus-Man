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
            var projlist = new List<ProjectViewModels>();

            var pro = new ProjectViewModels();
            pro.ProjectName = "CFP4-SR4";
            var tempstr = RMSpectialCh(pro.ProjectName);
            pro.ProjectKey = tempstr.Substring(0, (tempstr.Length > 20) ? 18 : tempstr.Length).ToUpper();
            pro.StartDate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            pro.PM = "brad.qiu@finisar.com:Alex.chen@finisar.com";
            pro.FinishRating = 15;
            pro.Description = "<h3><font color =\"red\">This is a test</font></h3>";
            projlist.Add(pro);

            pro.ProjectName = "QSFP-28G";
            projlist.Add(pro);

            return View(projlist);
        }

        public ActionResult CreateProject()
        {
            return View();
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

        private bool checkExistProject(string projectkey)
        {
            return false;
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

            if (string.IsNullOrEmpty(projectmodel.PM.Trim())
                || string.IsNullOrEmpty(projectmodel.Engineers.Trim()))
            {
                ViewBag.CreateError = createerror.Replace("<ErrorMsg>", "PM or Engineer email address is empty");
                return false;
            }

            if (string.IsNullOrEmpty(projectmodel.Description.Trim()))
            {
                ViewBag.CreateError = createerror.Replace("<ErrorMsg>", "Project description is empty");
                return false;
            }

            if (checkExistProject(projectmodel.ProjectKey))
            {
                ViewBag.CreateError = createerror.Replace("<ErrorMsg>", "Project exist");
                return false;
            }

            if (!EmailAddressValidate(projectmodel.PM) || !EmailAddressValidate(projectmodel.Engineers))
            {
                ViewBag.CreateError = createerror.Replace("<ErrorMsg>", "PM or Engineer email address is wrong");
                return false;
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

        private List<string> RetrieveProjectBondingInfo(string key,int count, Controller ctrl)
        {
            var ret = new List<string>();
            try
            {
                for (var i = 0; i < count; i++)
                {
                    if (!string.IsNullOrEmpty(ctrl.Request.Form[key + i]))
                    {
                        ret.Add(ctrl.Request.Form[key + i]);
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
            var pns = RetrieveProjectBondingInfo("PN", 9, this);
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
            var stats = RetrieveProjectBondingInfo("Station", 9, this);
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

            projectmodel.PM = Request.Form["PM"];
            projectmodel.Engineers = Request.Form["Engineers"];

            RetrieveProjectDesc(projectmodel);

            if (!ProjectValidate(projectmodel))
                return View(projectmodel);


            if (!RetrieveProjectDate(projectmodel))
                return View(projectmodel);

            StoreMesConfig(projectmodel);
            RetrievePNs(projectmodel);
            RetrieveStation(projectmodel);

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

            return RedirectToAction("ViewAll");

            //var dict = new RouteValueDictionary();
            //dict.Add("ProjectKey", projectmodel.ProjectKey);
            //return RedirectToAction("ProjectDetail", dict);
        }

        public ActionResult ProjectDetail(string ProjectKey)
        {
            return View();
        }

        [HttpPost, ActionName("ProjectDetail")]
        [ValidateAntiForgeryToken]
        public ActionResult ProjectPostDetail()
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