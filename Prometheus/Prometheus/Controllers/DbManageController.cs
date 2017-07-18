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
            //var sourcedb = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=" + System.IO.Path.Combine(HttpRuntime.AppDomainAppPath, "App_Data\\Prometheus.mdf") + ";Integrated Security=True;";
            //var targetdb = "Server=SHG-L80003583;User ID=dbg;Password=dbgpwd;Database=DebugDB;Connection Timeout=30;";
            var targetdb = "Server=SHG-L80003583;User ID=NPI;Password=NPI@NPI;Database=NPITrace;Connection Timeout=30;";
            //var targetdb = "Server=WUX-D80008792;User ID=dbg;Password=dbgpwd;Database=DebugDB;Connection Timeout=30;";
            var sourcedb = "Server=WUX-D80008792;User ID=NPI;Password=NPI@NPI;Database=NPITrace;Connection Timeout=30;";

            var tablelist = new List<string>();
            tablelist.Add("AlignmentPower");
            tablelist.Add("BIError");
            tablelist.Add("BIROOTCAUSE");
            tablelist.Add("BITestData");
            tablelist.Add("BITestDataField");
            tablelist.Add("BITestResult");
            tablelist.Add("BITestResultDataField");
            tablelist.Add("BookReportRecord");
            tablelist.Add("ErrorComments");
            tablelist.Add("FileLoadedData");
            tablelist.Add("Issue");
            tablelist.Add("IssueAttachment");
            tablelist.Add("IssueAttribute");
            tablelist.Add("IssueComments");
            tablelist.Add("IssueOBA");
            tablelist.Add("IssueRMA");
            tablelist.Add("ModuleTXOData");
            tablelist.Add("NeoMapData");
            tablelist.Add("PJErrorAttachment");
            tablelist.Add("Project");
            tablelist.Add("ProjectCriticalError");
            tablelist.Add("ProjectError");
            tablelist.Add("ProjectEvent");
            tablelist.Add("ProjectException");
            tablelist.Add("ProjectMembers");
            tablelist.Add("ProjectMesTable");
            tablelist.Add("ProjectModelID");
            tablelist.Add("ProjectPn");
            tablelist.Add("ProjectStation");
            tablelist.Add("ProjectSumDataSet");
            tablelist.Add("ProjectTestData");
            tablelist.Add("RELBackupData");
            tablelist.Add("RELMapData");
            tablelist.Add("RMABackupData");
            tablelist.Add("RMAMapData");
            tablelist.Add("SameAsDBTVM");
            tablelist.Add("ShareDoc");
            tablelist.Add("ShareTags");
            tablelist.Add("UserBlog");
            tablelist.Add("UserCacheInfo");
            tablelist.Add("UserGroupVM");
            tablelist.Add("UserKPIVM");
            tablelist.Add("UserLearn");
            tablelist.Add("UserMatrix");
            tablelist.Add("UserNet");
            tablelist.Add("UserRank");
            tablelist.Add("UserReviewedItems");
            tablelist.Add("UserTable");
            tablelist.Add("VCSELUsageTable");


            foreach (var tab in tablelist)
            {
                SqlConnection sourcecon = null;
                SqlConnection targetcon = null;

                try
                {
                    sourcecon = DBUtility.GetConnector(sourcedb);

                    targetcon = DBUtility.GetConnector(targetdb);
                    var tempsql = "delete from " + tab;
                    DBUtility.ExeSqlNoRes(targetcon, tempsql);

                    for (int idx = 0; ;)
                    {
                        var endidx = idx + 100000;

                        //load data to memory
                        var sql = "select * from(select ROW_NUMBER() OVER(order by(select null)) as mycount, * from " + tab + ") s1 where s1.mycount > " + idx.ToString() + " and s1.mycount <= " + endidx.ToString();
                        var dt = DBUtility.ExecuteSqlReturnTable(sourcecon, sql);
                        if (dt.Rows.Count == 0)
                        {
                            break;
                        }

                        if (dt != null && dt.Rows.Count > 0)
                        {
                            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(targetcon))
                            {
                                bulkCopy.DestinationTableName = tab;
                                bulkCopy.BulkCopyTimeout = 120;

                                try
                                {
                                    for (int i = 1; i < dt.Columns.Count; i++)
                                    {
                                        bulkCopy.ColumnMappings.Add(dt.Columns[i].ColumnName, dt.Columns[i].ColumnName);
                                    }
                                    bulkCopy.WriteToServer(dt);
                                    dt.Clear();
                                }
                                catch (Exception ex) { logthdinfo("Move db Exception: " + ex.Message + "\r\n"); }
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

                    if (sourcecon != null)
                    {
                        DBUtility.CloseConnector(sourcecon);
                        sourcecon = null;
                    }
                }

                if (targetcon != null)
                {
                    DBUtility.CloseConnector(targetcon);
                }

                if (sourcecon != null)
                {
                    DBUtility.CloseConnector(sourcecon);
                }
            }
            return View();
        }

        private void logthdinfo(string info)
        {
            try
            {
                var filename = "d:\\log\\sqlexception-" + DateTime.Now.ToString("yyyy-MM-dd");
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
            { }

        }

        public ActionResult SmartBackup()
        {
            var syscfgdict = CfgUtility.GetSysConfig(this);
            var targetmachine = syscfgdict["BAKTARGETMACHINE"];
            var srcmachine = syscfgdict["BAKSRCMACHINE"];

            var targetdb = "Server="+ targetmachine + ";User ID=NPI;Password=NPI@NPI;Database=NPITrace;Connection Timeout=60;";
            var sourcedb = "Server="+ srcmachine + ";User ID=NPI;Password=NPI@NPI;Database=NPITrace;Connection Timeout=60;";

            var tablelist = new List<string>();
            tablelist.Add("AlignmentPower");
            tablelist.Add("BIError");
            tablelist.Add("BIROOTCAUSE");
            tablelist.Add("BITestData");
            tablelist.Add("BITestDataField");
            tablelist.Add("BITestResult");
            tablelist.Add("BITestResultDataField");
            tablelist.Add("BookReportRecord");
            tablelist.Add("ErrorComments");
            tablelist.Add("FileLoadedData");
            tablelist.Add("Issue");
            tablelist.Add("IssueAttachment");
            tablelist.Add("IssueAttribute");
            tablelist.Add("IssueComments");
            tablelist.Add("IssueOBA");
            tablelist.Add("IssueRMA");
            tablelist.Add("ModuleTXOData");
            tablelist.Add("NeoMapData");
            tablelist.Add("PJErrorAttachment");
            tablelist.Add("Project");
            tablelist.Add("ProjectCriticalError");
            tablelist.Add("ProjectError");
            tablelist.Add("ProjectEvent");
            tablelist.Add("ProjectException");
            tablelist.Add("ProjectMembers");
            tablelist.Add("ProjectMesTable");
            tablelist.Add("ProjectModelID");
            tablelist.Add("ProjectPn");
            tablelist.Add("ProjectStation");
            tablelist.Add("ProjectSumDataSet");
            tablelist.Add("ProjectTestData");
            tablelist.Add("RELBackupData");
            tablelist.Add("RELMapData");
            tablelist.Add("RMABackupData");
            tablelist.Add("RMAMapData");
            tablelist.Add("SameAsDBTVM");
            tablelist.Add("ShareDoc");
            tablelist.Add("ShareTags");
            tablelist.Add("UserBlog");
            tablelist.Add("UserCacheInfo");
            tablelist.Add("UserGroupVM");
            tablelist.Add("UserKPIVM");
            tablelist.Add("UserLearn");
            tablelist.Add("UserMatrix");
            tablelist.Add("UserNet");
            tablelist.Add("UserRank");
            tablelist.Add("UserReviewedItems");
            tablelist.Add("UserTable");
            tablelist.Add("VCSELUsageTable");


            foreach (var tab in tablelist)
            {
                SqlConnection sourcecon = null;
                SqlConnection targetcon = null;

                try
                {
                    sourcecon = DBUtility.GetConnector(sourcedb);
                    targetcon = DBUtility.GetConnector(targetdb);

                    var targetbackuptime = string.Empty;
                    var tempsql = "select top 1 databackuptm from " + tab+ " order by databackuptm DESC";
                    var tempdbret = DBUtility.ExeSqlWithRes(targetcon, tempsql);
                    if (tempdbret.Count > 0)
                    {
                        try
                        {
                            targetbackuptime = DateTime.Parse(Convert.ToString(tempdbret[0][0])).ToString("yyyy-MM-dd hh:mm:ss");
                        }
                        catch (Exception ex) { targetbackuptime = string.Empty; }
                    }

                    for (int idx = 0; ;)
                    {
                        var endidx = idx + 100000;

                        //load data to memory
                        var sql = string.Empty;
                        if (!string.IsNullOrEmpty(targetbackuptime))
                        {
                            sql = "select * from (select ROW_NUMBER() OVER(order by(select null)) as mycount, * from " + tab + " where databackuptm > '"+ targetbackuptime + "') s1 where s1.mycount > " + idx.ToString() + " and s1.mycount <= " + endidx.ToString();
                        }
                        else
                        {
                            sql = "select * from (select ROW_NUMBER() OVER(order by(select null)) as mycount, * from " + tab + ") s1 where s1.mycount > " + idx.ToString() + " and s1.mycount <= " + endidx.ToString();
                        }
                        
                        var dt = DBUtility.ExecuteSqlReturnTable(sourcecon, sql);
                        if (dt.Rows.Count == 0)
                        {
                            break;
                        }

                        if (dt != null && dt.Rows.Count > 0)
                        {
                            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(targetcon))
                            {
                                bulkCopy.DestinationTableName = tab;
                                bulkCopy.BulkCopyTimeout = 120;

                                try
                                {
                                    for (int i = 1; i < dt.Columns.Count; i++)
                                    {
                                        bulkCopy.ColumnMappings.Add(dt.Columns[i].ColumnName, dt.Columns[i].ColumnName);
                                    }
                                    bulkCopy.WriteToServer(dt);
                                    dt.Clear();
                                }
                                catch (Exception ex) { logthdinfo("BACK-UP Exception: " + ex.Message + "\r\n"); }
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

                    if (sourcecon != null)
                    {
                        DBUtility.CloseConnector(sourcecon);
                        sourcecon = null;
                    }
                }

                if (targetcon != null)
                {
                    DBUtility.CloseConnector(targetcon);
                }

                if (sourcecon != null)
                {
                    DBUtility.CloseConnector(sourcecon);
                }
            }

            return View("MoveDataBase");
        }

    }
}