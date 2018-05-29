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
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Threading;

namespace Prometheus.Models
{
    public class CookieUtility
    {

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
                    ck.Expires = DateTime.Now.AddDays(7);
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

        public static Dictionary<string, string> UnpackCookie(Controller ctrl)
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

        public static bool RemoveCookie(Controller ctrl)
        {
            ctrl.Response.Cookies["activenpi"].Expires = DateTime.Now.AddDays(-1000);
            return true;
        }
    }
}