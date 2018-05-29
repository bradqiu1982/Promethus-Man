using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RestSharp;

namespace Prometheus.Models
{
    public class IMGTYPE
    {
        public string PNG = "png";
        public string JPG = "jpg";
        public string SVG = "svg";
    }

    public class HighChartsByNode
    {
        private static string GetImgName(Controller ctrl,string ext)
        {
            string imgdir = ctrl.Server.MapPath("~/userfiles") + "\\docs\\HighChart\\";
            if (!Directory.Exists(imgdir))
            {
                Directory.CreateDirectory(imgdir);
            }

            return imgdir + Guid.NewGuid().ToString("N") + "."+ext.Replace(".","");
        }

        public static string GenerateHighChartFile(Controller ctrl, object option, string imgext="png")
        {
            try
            {
                var fn = GetImgName(ctrl, imgext);
                var client = new RestClient("http://wuxinpi.china.ads.finisar.com:9999/");

                var request = new RestRequest(Method.POST);
                request.AddParameter("b64", "true");
                request.AddParameter("type", "png");
                request.AddParameter("width", "800");
                request.AddParameter("options", Newtonsoft.Json.JsonConvert.SerializeObject(option));

                var response = client.Execute(request);
                if (response.IsSuccessful)
                {
                    var responseString = response.Content;
                    var imgbytes = Convert.FromBase64String(responseString);
                    var fs = File.Create(fn);
                    fs.Write(imgbytes, 0, imgbytes.Length);
                    fs.Close();
                }
                client.ClearHandlers();
                return fn;
            }
            catch (Exception ex) { }
            return string.Empty;
        }

        public static string GenerateHighChartBase64(Controller ctrl, object option, string imgext = "png")
        {
            try
            {
                var client = new RestClient("http://wuxinpi.china.ads.finisar.com:9999/");
                var request = new RestRequest(Method.POST);
                request.AddParameter("b64", "true");
                request.AddParameter("type", "png");
                request.AddParameter("width", "800");
                request.AddParameter("options", Newtonsoft.Json.JsonConvert.SerializeObject(option));

                var response = client.Execute(request);
                if (response.IsSuccessful)
                {
                    var responseString = response.Content;
                    return responseString;
                }
                client.ClearHandlers();
            }
            catch (Exception ex) { }
            return string.Empty;
        }



    }
}