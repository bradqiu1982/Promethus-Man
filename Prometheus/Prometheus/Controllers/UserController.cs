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

namespace Prometheus.Controllers
{
    public class UserController : Controller
    {

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
            user.Email = username;
            user.Password = password;
            user.UpdateDate = DateTime.Parse(updatetime);
            user.RegistUser();

            var routevalue = new RouteValueDictionary();
            routevalue.Add("validatestr", Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes(username + "||" + updatetime)));
            //send validate email
            string scheme = this.Url.RequestContext.HttpContext.Request.Url.Scheme;
            string validatestr = this.Url.Action("ValidateUser","User", routevalue, scheme);

            var toaddrs = new List<string>();
            toaddrs.Add(username);
            sendemail(toaddrs, validatestr);

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

        public ActionResult ValidateUser(string validatestr)
        {
            if (string.IsNullOrEmpty(validatestr))
            {
                var createerror = "<h3><font color=\"red\">Fail to active User: active string is empty</font></h3>";
                ViewBag.CreateError = createerror;
                RedirectToAction("RegisterUser");
            }
            
            var bs = Convert.FromBase64String(validatestr);
            var val = UTF8Encoding.UTF8.GetString(bs);
            var username = val.Split(new char[] { '|' })[0];
            var updatetime = val.Split(new char[] { '|' })[1];
            UserViewModels.ValidateUser(username);
            return RedirectToAction("ValidateNoticeB");
        }

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
            return UnpackCookie(ctrl.Request);
        }

        public static Dictionary<string, string> UnpackCookie(HttpRequestBase req)
        {

            var ret = new Dictionary<string, string>();

            if (req.Cookies["activenpi"] != null)
            {
                try
                {
                    var ck = req.Cookies["activenpi"];
                    foreach (var key in ck.Values.AllKeys)
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

        public ActionResult LoginOutUser(string ctrl, string action)
        {
            var val = UnpackCookie(this);
            val["logonuser"] = "";
            SetCookie(this, val);
            return RedirectToAction("ViewAll", "DashBoard");
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