using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus
{
    public class MYStaticFileHandler : IHttpHandler
    {
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.Path.ToUpper().Contains("/USERFILES/DOCS"))
            {
                var fooCookie = context.Request.Cookies["activenpi"];
                if (fooCookie != null)
                {

                }
            }
        }
    }
}