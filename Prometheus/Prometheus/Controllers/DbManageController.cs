using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Prometheus.Models;
using System.Data.SqlClient;
using System.Data;

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
                && !querystr.Contains("update")
                && !querystr.Contains("delete"))
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

        public ActionResult MoveDataBase()
        {
            var targetdb = "Server=wux-parallel;uid=NPI;pwd=NPI@IPN;Database=NPITrace;Connection Timeout=30;";
            
            var tablelist = new List<string>();
            tablelist.Add("BIError");
            tablelist.Add("BIROOTCAUSE");
            tablelist.Add("BITestData");
            tablelist.Add("BITestDataField");
            tablelist.Add("BookReportRecord");
            tablelist.Add("ErrorComments");
            tablelist.Add("Issue");
            tablelist.Add("IssueAttachment");
            tablelist.Add("IssueAttribute");
            tablelist.Add("IssueComments");
            tablelist.Add("IssueOBA");
            tablelist.Add("IssueRMA");
            tablelist.Add("PJErrorAttachment");
            tablelist.Add("Project");
            tablelist.Add("ProjectError");
            tablelist.Add("ProjectEvent");
            tablelist.Add("ProjectMembers");
            tablelist.Add("ProjectMesTable");
            tablelist.Add("ProjectModelID");
            tablelist.Add("ProjectPn");
            tablelist.Add("ProjectStation");
            tablelist.Add("ProjectSumDataSet");
            tablelist.Add("ProjectTestData");
            tablelist.Add("ShareDoc");
            tablelist.Add("ShareTags");
            tablelist.Add("UserBlog");
            tablelist.Add("UserLearn");
            tablelist.Add("UserNet");
            tablelist.Add("UserRank");
            tablelist.Add("UserTable");
            tablelist.Add("VCSELUsageTable");

            foreach (var tab in tablelist)
            {
                SqlConnection targetcon = null;
                try
                {
                    targetcon = DBUtility.GetConnector(targetdb);
                    var tempsql = "delete from " + tab;
                    DBUtility.ExeSqlNoRes(targetcon, tempsql);

                    for(int idx = 0; ;)
                    {
                        var endidx = idx + 100000;

                            //load data to memory
                            var sql = "select * from(select ROW_NUMBER() OVER(order by(select null)) as mycount, * from " + tab + ") s1 where s1.mycount > "+ idx.ToString() +" and s1.mycount <= "+endidx.ToString();
                            var dt = DBUtility.ExecuteLocalQueryReturnTable(sql);
                        if (dt.Rows.Count == 0)
                        {
                            break;
                        }

                            if (dt != null && dt.Rows.Count > 0)
                            {
                                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(targetcon))
                                {
                                    bulkCopy.DestinationTableName = tab;
                                    try
                                    {
                                        for (int i = 1; i < dt.Columns.Count; i++)
                                        {
                                            bulkCopy.ColumnMappings.Add(dt.Columns[i].ColumnName, dt.Columns[i].ColumnName);
                                        }
                                        bulkCopy.WriteToServer(dt);
                                        dt.Clear();
                                    }
                                    catch (Exception ex){}
                                }//end using
                            }//end if

                        idx = idx + 100000;
                    }//end for
                }
                catch (Exception ex)
                {
                    if (targetcon != null)
                    {
                        DBUtility.CloseConnector(targetcon);
                        targetcon = null;
                    }
                }

                if (targetcon != null)
                {
                    DBUtility.CloseConnector(targetcon);
                }
            }
            return View();
        }
    }
}