using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Prometheus.Models;

namespace Prometheus.Controllers
{
    public class DBManageController : Controller
    {
        // GET: DbManage
        public ActionResult ExecuteSQLs()
        {
            return View();
        }

        [HttpPost, ActionName("ExecuteSQLs")]
        [ValidateAntiForgeryToken]
        public ActionResult ExecuteSQLsPost()
        {
            var querystr = Request.Form["querystring"];
            if (!querystr.Contains("select")
                && !querystr.Contains("insert")
                && !querystr.Contains("update"))
            {
                ViewBag.ExecuteRes = "invalidate sql strings";
                return View();
            }

            var ret = true;
            var sqls = querystr.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in sqls)
            {
                if (!DBUtility.ExeLocalSqlNoRes(s.Trim()))
                {
                    ret = false;
                }
            }

            if(ret)
                ViewBag.ExecuteRes = "Execute sucessfully";
            else
                ViewBag.ExecuteRes = "Sqls have error";

            return View();
        }
    }
}