using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Prometheus.Models;
using System.Web.Routing;
using System.Text;
using System.Net.Mail;

namespace Prometheus.Controllers
{
    public class ProjectController : Controller
    {
        // GET: Project
        public ActionResult ViewAll()
        {
            return View();
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

        [HttpPost, ActionName("CreateProject")]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePostProject()
        {
            var projectmodel = new ProjectViewModels();
            projectmodel.ProjectName = Request.Form["ProjectName"];
            var tempstr = RMSpectialCh(projectmodel.ProjectName);
            projectmodel.ProjectKey = tempstr.Substring(0, (tempstr.Length > 20) ? 18 : tempstr.Length).ToUpper();

            projectmodel.PM = Request.Form["PM"];
            projectmodel.Engineers = Request.Form["Engineers"];

            projectmodel.Description = Request.Form["editor1"];

            if (!ProjectValidate(projectmodel))
            {
                return View(projectmodel);
            }

            if (!string.IsNullOrEmpty(Request.Form["StartDate"].Trim()))
            {
                try
                {
                    projectmodel.StartDate = DateTime.Parse(Request.Form["StartDate"]);
                }
                catch (Exception ex)
                {
                    ViewBag.CreateError = "<h3><font color=\"red\">Fail to create project: "+ RMSpectialCh(ex.ToString())+"</font></h3>";
                    return View(projectmodel);
                }
            }
            else
            {
                ViewBag.CreateError = ViewBag.CreateError = "<h3><font color=\"red\">Fail to create project: StartDate is empty</font></h3>";
                return View(projectmodel);
            }

            return RedirectToAction("ViewAll");

            //var dict = new RouteValueDictionary();
            //dict.Add("ProjectKey", projectmodel.ProjectKey);
            //return RedirectToAction("ProjectDetail", dict);
        }


        public ActionResult ProjectDetail(string ProjectKey)
        {
            System.Windows.MessageBox.Show("get project detail: "+ ProjectKey);
            return View();
        }

        [HttpPost, ActionName("ProjectDetail")]
        [ValidateAntiForgeryToken]
        public ActionResult ProjectPostDetail()
        {
            System.Windows.MessageBox.Show("post project detail");
            return View();
        }
    }
}