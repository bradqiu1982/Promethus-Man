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
using System.Web.UI.WebControls;

namespace Prometheus.Models
{
    public class EmailUtility
    {
        private static void logthdinfo(string info)
        {
            try
            {
                var filename = "d:\\log\\emailexception-" + DateTime.Now.ToString("yyyy-MM-dd");
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
            catch (Exception ex)
            {
            }
        }

        public static bool SendEmail(Controller ctrl,string title, List<string> tolist, string content)
        {
            try
            {
                var syscfgdict = CfgUtility.GetSysConfig(ctrl);

                var message = new MailMessage();
                message.From = new MailAddress(syscfgdict["APPEMAILADRESS"]);
                foreach (var item in tolist)
                {
                    if (!item.Contains("@"))
                        continue;
                    try
                    {
                        if (item.Contains(";"))
                        {
                            var ts = item.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var t in ts)
                            {
                                message.To.Add(t);
                            }
                        }
                        else
                        {
                            message.To.Add(item);
                        }
                    }
                    catch (Exception e) { logthdinfo("Address exception: " + e.Message); }
                }

                message.Subject = title;
                message.Body = content;

                SmtpClient client = new SmtpClient();
                client.Host = syscfgdict["EMAILSERVER"];
                client.EnableSsl = true;
                client.Timeout = 60000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(syscfgdict["APPEMAILADRESS"], syscfgdict["APPEMAILPWD"]);

                ServicePointManager.ServerCertificateValidationCallback
                    = delegate (object s, X509Certificate certificate, X509Chain chain
                    , SslPolicyErrors sslPolicyErrors) { return true; };

                new Thread(() => {
                    try
                    {
                        client.Send(message);
                    }
                    catch (SmtpFailedRecipientsException ex)
                    {
                        logthdinfo("SmtpFailedRecipientsException exception: " + ex.Message);
                        try
                        {
                            message.To.Clear();
                            foreach (var item in tolist)
                            {
                                if (ex.Message.Contains(item))
                                {
                                    try
                                    {
                                        message.To.Add(item);
                                    }
                                    catch (Exception e) { logthdinfo("Address exception2: " + e.Message); }
                                }
                            }
                            client.Send(message);
                        }
                        catch (Exception ex1)
                        {
                            logthdinfo("nest exception1: " + ex1.Message);
                        }
                    }
                    catch (Exception ex)
                    {
                        logthdinfo("send exception: " + ex.Message);
                    }
                }).Start();
            }
            catch (Exception ex)
            {
                logthdinfo("main exception: " + ex.Message);
                return false;
            }
            return true;
        }

        public static bool SendHtmlEmail(string title, List<string> tolist, string content)
        {
            try
            {
                MailDefinition md = new MailDefinition();
                md.From = "WXNPI.Trace@finisar.com";
                md.Subject = title;
                md.IsBodyHtml = true;

                var message = md.CreateMailMessage("WXNPI.Trace@finisar.com", new Dictionary<string, string>(), content, new System.Web.UI.Control());
                SmtpClient client = new SmtpClient();
                client.Host = "wmail.finisar.com";
                client.EnableSsl = true;
                client.Timeout = 60000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential("WXNPI.Trace@finisar.com", "abc@123");

                ServicePointManager.ServerCertificateValidationCallback
                    = delegate (object s, X509Certificate certificate, X509Chain chain
                    , SslPolicyErrors sslPolicyErrors) { return true; };

                new Thread(() => {
                    try
                    {
                        client.Send(message);
                    }
                    catch (Exception ex)
                    { }
                }).Start();
            }
            catch (Exception ex)
            {
                //System.Windows.MessageBox.Show(ex.ToString());
                return false;
            }
            return true;
        }
    }
}