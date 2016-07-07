using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.Web.Routing;

namespace Prometheus.Controllers
{
    public class UserController : Controller
    {
        // GET: User
        public ActionResult Home()
        {
            return View();
        }

        private bool checkexistuser(string user)
        {
            return false;
        }

        private bool sendemail(List<string> tolist, string content)
        {
            try
            {
                var message = new MailMessage();
                message.From = new MailAddress("brad.qiu@finisar.com");
                foreach(var item in tolist)
                {
                    message.To.Add(item);
                }
                message.Subject = "This is a test";
                message.Body = content;

                SmtpClient client = new SmtpClient();
                client.Host = "wmail.finisar.com";
                client.EnableSsl = true;
                client.Timeout = 10000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential("brad.qiu@finisar.com", "wangle@321");
                client.Send(message);

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
                return false;
            }
            return true;
        }

        public ActionResult RegisterUser()
        {
            return View();
        }

        [HttpPost, ActionName("RegisterUser")]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterUserPOST()
        {
           if (this.checkexistuser(Request.Form["Email"]))
            {
            }

            var username = Request.Form["Email"];
            var password = Request.Form["Password"];

            //Store user Info
            string updatetime = DateTime.Now.ToString();

            var routevalue = new RouteValueDictionary();
            routevalue.Add("validatestr", Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes(username + "||" + updatetime)));
            //send validate email
            string scheme = this.Url.RequestContext.HttpContext.Request.Url.Scheme;
            string validatestr = this.Url.Action("ValidateUser","User", routevalue, scheme);

            var toaddrs = new List<string>();
            toaddrs.Add(username);
            sendemail(toaddrs, validatestr);

            return View();
        }

        public ActionResult ValidateUser(string validatestr)
        {
            if (string.IsNullOrEmpty(validatestr))
            {
                //something wrong
                return RedirectToAction("ViewAll", "DashBoard");
            }
            
            var bs = Convert.FromBase64String(validatestr);
            var val = UTF8Encoding.UTF8.GetString(bs);

            System.Windows.MessageBox.Show(val);

            return RedirectToAction("ViewAll","DashBoard");
        }

        public ActionResult LoginUser()
        {
            return View();
        }
    }
}