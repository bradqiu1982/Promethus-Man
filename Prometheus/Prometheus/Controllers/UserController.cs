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
                client.Credentials = new NetworkCredential("brad.qiu@finisar.com", "wangle@432");
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

        //public ActionResult LoginUser()
        //{
        //    return View();
        //}

        public ActionResult LoginUser(string ctrl,string action)
        {
            return View();
        }

        [HttpPost, ActionName("LoginUser")]
        [ValidateAntiForgeryToken]
        public ActionResult LoginUserPOST()
        {
            var username = Request.Form["Email"];
            var password = Request.Form["Password"];

            

            var ckdict = UnpackCookie(this);
            if (ckdict.ContainsKey("logonredirectctrl") && ckdict.ContainsKey("logonredirectact"))
            {
                //verify user information
                string logonuser = username + "||" + DateTime.Now.ToString();
                var ck = new Dictionary<string, string>();
                ck.Add("logonuser", logonuser);
                SetCookie(this, ck);
                return RedirectToAction(ckdict["logonredirectact"], ckdict["logonredirectctrl"]);
            }
            else
            {
                //verify user information
                string logonuser = username +"||"+ DateTime.Now.ToString();
                var ck = new Dictionary<string, string>();
                ck.Add("logonuser", logonuser);
                SetCookie(this, ck);
                return RedirectToAction("UserCenter", "User");
            }
           
        }

        public static bool SetCookie(Controller ctrl, Dictionary<string, string> values)
        {
            try
            {
                HttpCookie ck = null;

                if (ctrl.Request.Cookies["activenpi"] != null)
                {
                    ck = ctrl.Request.Cookies["activenpi"];
                    foreach (var item in values)
                    {
                        ck.Values[item.Key] = Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes(item.Value));
                    }

                    if (ctrl.Response.Cookies["activenpi"] != null)
                    {
                        ctrl.Response.SetCookie(ck);
                    }
                    else
                    {
                        ctrl.Response.AppendCookie(ck);
                    }
                    
                }
                else
                {
                    ck = new HttpCookie("activenpi");
                    foreach (var item in values)
                    {
                        ck.Values[item.Key] = Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes(item.Value));
                    }

                    if (ctrl.Response.Cookies["activenpi"] != null)
                    {
                        ctrl.Response.SetCookie(ck);
                    }
                    else
                    {
                        ctrl.Response.AppendCookie(ck);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static Dictionary<string,string> UnpackCookie(Controller ctrl)
        {

            var ret = new Dictionary<string, string>();

            if (ctrl.Request.Cookies["activenpi"] != null)
            {
                try
                {
                    var ck = ctrl.Request.Cookies["activenpi"];
                    foreach(var key in ck.Values.AllKeys)
                    {
                        ret.Add(key, UTF8Encoding.UTF8.GetString(Convert.FromBase64String(ck.Values[key])));
                    }
                    return ret;
                }
                catch (Exception ex)
                {
                    ret.Clear();
                    return ret;
                }
            }
            else
            {
                ret.Clear();
                return ret;
            }
        }

        public ActionResult UserCenter()
        {
            var val = UnpackCookie(this);
            if(val != null)
            { 
                System.Windows.MessageBox.Show(val["logonuser"]);
            }
            return View();
        }

    }
}