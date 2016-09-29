using Prometheus.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Excel = Microsoft.Office.Interop.Excel;

namespace Prometheus.Controllers
{
    public class CustomerDataController : Controller
    {
        public ActionResult CommitVecselData()
        {
            return View();
        }

        private static Excel.Workbook OpenBook(Excel.Application excelInstance, string fileName, bool readOnly, bool editable,
        bool updateLinks)
        {
            Excel.Workbook book = excelInstance.Workbooks.Open(
                fileName, updateLinks, readOnly,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                Type.Missing, editable, Type.Missing, Type.Missing, Type.Missing,
                Type.Missing, Type.Missing);
            return book;
        }

        private static void ReleaseRCM(object o)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(o);
            }
            catch
            {
            }
            finally
            {
                o = null;
            }
        }

        private bool WholeLineEmpty(List<string> line)
        {
            bool ret = true;
            foreach (var item in line)
            {
                if (!string.IsNullOrEmpty(item))
                {
                    ret = false;
                }
            }
            return ret;
        }

        private List<List<string>> RetrieveDataFromExcel(string wholefn)
        {
            var data = new List<List<string>>();
            Excel.Application excel = null;
            Excel.Workbook wkb = null;

            try
            {
                excel = new Excel.Application();
                wkb = OpenBook(excel, wholefn,true,false,false);
                Excel.Worksheet sheet = wkb.Sheets[1] as Excel.Worksheet;

                var excelRange = sheet.UsedRange;
                object[,] valueArray = (object[,])excelRange.get_Value(
                Excel.XlRangeValueDataType.xlRangeValueDefault);

                for (int row = 1; row <= sheet.UsedRange.Rows.Count; ++row)
                {
                    var line = new List<string>();
                    for (int col = 1; col <= sheet.UsedRange.Columns.Count; ++col)
                    {
                        if (valueArray[row, col] == null)
                        {
                            line.Add(string.Empty);
                        }
                        else
                        {
                            line.Add(valueArray[row, col].ToString());
                        }
                    }
                    if (!WholeLineEmpty(line))
                    {
                        data.Add(line);
                    }
                }

                return data;
            }
            catch (Exception ex)
            {
                data.Clear();
                return data;
            }
            finally
            {
                if (wkb != null)
                    ReleaseRCM(wkb);
                if (excel != null)
                    ReleaseRCM(excel);
            }
        }



        [HttpPost, ActionName("CommitVecselData")]
        [ValidateAntiForgeryToken]
        public ActionResult CommitVecselDataPost()
        {
            var wholefn = "";
            try
            {
                if (!string.IsNullOrEmpty(Request.Form["VecselFileName"]))
                {
                    var customereportfile = Request.Form["VecselFileName"];
                    var originalname = Path.GetFileNameWithoutExtension(customereportfile).Replace(" ", "_");

                    foreach (string fl in Request.Files)
                    {
                        if (fl != null && Request.Files[fl].ContentLength > 0)
                        {
                            string fn = Path.GetFileName(Request.Files[fl].FileName).Replace(" ", "_");

                            string datestring = DateTime.Now.ToString("yyyyMMdd");
                            string imgdir = Server.MapPath("~/userfiles") + "\\docs\\" + datestring + "\\";

                            if (!Directory.Exists(imgdir))
                            {
                                Directory.CreateDirectory(imgdir);
                            }

                            fn = Path.GetFileNameWithoutExtension(fn) + "-" + DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(fn);
                            Request.Files[fl].SaveAs(imgdir + fn);

                            if (fn.Contains(originalname))
                            {
                                wholefn = imgdir + fn;
                                break;
                            }
                        }//end if
                    }//end foreach

                    if (!string.IsNullOrEmpty(wholefn))
                    {
                        var data = RetrieveDataFromExcel(wholefn);
                        if (data.Count > 0)
                        {
                            ViewBag.ROWCOUNT = data.Count;
                            ViewBag.COLCOUNT = data[0].Count;
                            return View("ConfirmVecselData",data);
                        }
                    }

                }//end if
            }
            catch (Exception ex)
            {  }

            return View();
        }

        [HttpPost, ActionName("ConfirmVecselData")]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmVecselDataPost()
        {
            if (Request.Form["confirmdata"] != null)
            {
                var rowcnt = Convert.ToInt32(Request.Form["rowcount"]);
                var colcnt = Convert.ToInt32(Request.Form["colcount"]);
                var data = new List<List<string>>();
                for (var row = 0; row < rowcnt; row++)
                {
                    var line = new List<string>();
                    for (var col = 0; col < colcnt; col++)
                    {
                        line.Add(Request.Form["row" + row + "col" + col]);
                    }
                    data.Add(line);
                }
            }

            return View();
        }


        public ActionResult CommitRMAData()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {

            }
            else
            {
                var ck = new Dictionary<string, string>();
                ck.Add("logonredirectctrl", "CustomerData");
                ck.Add("logonredirectact", "CommitRMAData");
                ck.Add("currentaction", "CommitRMAData");
                CookieUtility.SetCookie(this, ck);
                return RedirectToAction("LoginUser", "User");
            }

            return View();
        }

        private static string RMSpectialCh(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        [HttpPost, ActionName("CommitRMAData")]
        [ValidateAntiForgeryToken]
        public ActionResult CommitRMADataPost()
        {
            var projlist = ProjectViewModels.RetrieveAllProjectKey();

            var wholefn = "";
            try
            {
                if (!string.IsNullOrEmpty(Request.Form["RMAFileName"]))
                {
                    var customereportfile = Request.Form["RMAFileName"];
                    var originalname = Path.GetFileNameWithoutExtension(customereportfile).Replace(" ", "_");

                    foreach (string fl in Request.Files)
                    {
                        if (fl != null && Request.Files[fl].ContentLength > 0)
                        {
                            string fn = Path.GetFileName(Request.Files[fl].FileName).Replace(" ", "_");

                            string datestring = DateTime.Now.ToString("yyyyMMdd");
                            string imgdir = Server.MapPath("~/userfiles") + "\\docs\\" + datestring + "\\";

                            if (!Directory.Exists(imgdir))
                            {
                                Directory.CreateDirectory(imgdir);
                            }

                            fn = Path.GetFileNameWithoutExtension(fn) + "-" + DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(fn);
                            Request.Files[fl].SaveAs(imgdir + fn);

                            if (fn.Contains(originalname))
                            {
                                wholefn = imgdir + fn;
                                break;
                            }
                        }//end if
                    }//end foreach

                    if (!string.IsNullOrEmpty(wholefn))
                    {
                        var data = RetrieveDataFromExcel(wholefn);
                        var realdata = new List<List<string>>();
                        if (data.Count > 1)
                        {
                            var templine = new List<string>();
                            templine.Add("Project");
                            templine.Add("Finisar RMA");
                            templine.Add("Finisar Model");
                            templine.Add("Model SN");
                            templine.Add("Assignee Email");
                            templine.Add("Due Date");
                            templine.Add("End Customer");
                            templine.Add("Customer RMA Reason");
                            realdata.Add(templine);

                            for (var idx = 0;idx < data.Count;idx++)
                            {
                                if (idx != 0)
                                {
                                    var snlist = data[idx][8].Split(new string[] { "\r","\n", " ", ";" }, StringSplitOptions.RemoveEmptyEntries);
                                    var reasons = data[idx][10].Split(new string[] { "\r", "\n"}, StringSplitOptions.RemoveEmptyEntries);
                                    foreach (var sn in snlist)
                                    {
                                        templine = new List<string>();

                                        var trimprojectname = RMSpectialCh(data[idx][12]).ToUpper();
                                        if (trimprojectname.Length > 40)
                                        {
                                            trimprojectname = trimprojectname.Substring(0, 38);
                                        }

                                        var pjname = string.Empty;
                                        foreach (var item in projlist)
                                        {
                                            if (string.Compare(item, trimprojectname,true) == 0)
                                            {
                                                pjname = item;
                                                break;
                                            }
                                        }

                                        if (!string.IsNullOrEmpty(pjname))
                                        {
                                            templine.Add(pjname);
                                        }
                                        else
                                        {
                                            templine.Add("N/A");
                                        }

                                        templine.Add(data[idx][1]);
                                        templine.Add(data[idx][6]+" "+ data[idx][7]);
                                        templine.Add(sn);
                                        if (!data[idx][13].Contains("@"))
                                        {
                                            templine.Add(data[idx][13].Trim().Replace(" ",".")+"@finisar.com");
                                        }
                                        else
                                        {
                                            templine.Add(data[idx][13]);
                                        }
                                        templine.Add(data[idx][14]);
                                        templine.Add(data[idx][4]);

                                        var rs = "N/A";
                                        foreach (var r in reasons)
                                        {
                                            if (r.Contains(sn))
                                            {
                                                rs = r;
                                                break;
                                            }
                                        }

                                        if (rs == "N/A" && reasons.Count() > 0)
                                        {
                                            rs = reasons[0];
                                        }

                                        templine.Add(rs);

                                        realdata.Add(templine);
                                    }//end foreach
                                }//end if
                            }//end for
                        }


                        if (realdata.Count > 1)
                        {
                            ViewBag.ROWCOUNT = realdata.Count;
                            ViewBag.COLCOUNT = realdata[0].Count;
                            return View("ConfirmRMAData", realdata);
                        }
                    }

                }//end if
            }
            catch (Exception ex)
            { }

            return View();
        }


        private void SendRMAEvent(IssueViewModels vm, string operate, bool nocheck = false)
        {
            var alertime = vm.RetrieveAlertEmailDate(vm.IssueKey);
            if ((!string.IsNullOrEmpty(alertime) && DateTime.Parse(alertime).AddHours(24) < DateTime.Now) || nocheck)
            {
                vm.UpdateAlertEmailDate();

                var routevalue = new RouteValueDictionary();
                routevalue.Add("issuekey", vm.IssueKey);
                //send validate email
                string scheme = this.Url.RequestContext.HttpContext.Request.Url.Scheme;
                string validatestr = this.Url.Action("UpdateIssue", "Issue", routevalue, scheme);

                var content = vm.Summary + " is " + operate + " by " + vm.Reporter + " :\r\n " + validatestr;

                var toaddrs = new List<string>();
                toaddrs.AddRange(vm.RelativePeopleList);
                toaddrs.Add(vm.Assignee);
                toaddrs.Add(vm.Reporter);
                EmailUtility.SendEmail("Parallel NPI Trace Notice", toaddrs, content);

            }
        }


        [HttpPost, ActionName("ConfirmRMAData")]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmRMADataPost()
        {
            if (Request.Form["confirmdata"] != null)
            {
                var ckdict = CookieUtility.UnpackCookie(this);
                var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

                var rowcnt = Convert.ToInt32(Request.Form["rowcount"]);
                var colcnt = Convert.ToInt32(Request.Form["colcount"]);
                var data = new List<List<string>>();
                for (var row = 0; row < rowcnt; row++)
                {
                    var line = new List<string>();
                    for (var col = 0; col < colcnt; col++)
                    {
                        line.Add(Request.Form["row" + row + "col" + col]);
                    }
                    data.Add(line);
                }


                if (data.Count > 1 && data[0].Count == 8)
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        if (i != 0 && string.Compare(data[i][0], "N/A") != 0)
                        {
                            var vm = new IssueViewModels();
                            vm.ProjectKey = data[i][0];
                            if (vm.ProjectKey.Length > 50)
                            {
                                vm.ProjectKey = vm.ProjectKey.Substring(0, 48);
                            }

                            vm.IssueKey = IssueViewModels.GetUniqKey();
                            vm.IssueType = ISSUETP.RMA;

                            vm.FinisarRMA = data[i][1];
                            if (vm.FinisarRMA.Length > 50)
                            {
                                vm.FinisarRMA = vm.FinisarRMA.Substring(0, 48);
                            }

                            vm.FinisarModel = data[i][2];
                            if (vm.FinisarModel.Length > 50)
                            {
                                vm.FinisarModel = vm.FinisarModel.Substring(0, 48);
                            }

                            vm.ECustomer = data[i][6];
                            if (vm.ECustomer.Length > 50)
                            {
                                vm.ECustomer = vm.ECustomer.Substring(0, 48);
                            }


                            vm.CRMANUM = "N/A";

                            vm.CReport = data[i][7];
                            if (vm.CReport.Length > 180)
                            {
                                vm.CReport = vm.CReport.Substring(0, 178);
                            }

                            vm.RelativePeoples = "";
                            vm.ModuleSN = data[i][3];
                            if (vm.ModuleSN.Length > 50)
                            {
                                vm.ModuleSN = vm.ModuleSN.Substring(0, 48);
                            }

                            vm.Summary = "RMA " + vm.FinisarRMA + " for module " + vm.ModuleSN + " " + vm.FinisarModel + " from " + vm.ECustomer + ":" + vm.CReport.Substring(0, vm.CReport.Length > 50 ? 50 : vm.CReport.Length);
                            if (vm.Summary.Length > 200)
                            {
                                vm.Summary = vm.Summary.Substring(0, 198);
                            }

                            vm.Priority = ISSUEPR.Major;
                            vm.DueDate = DateTime.Parse(data[i][5]);
                            vm.ReportDate = DateTime.Now;
                            vm.Assignee = data[i][4].ToUpper();
                            if (vm.Assignee.Length > 200)
                            {
                                vm.Assignee = vm.Assignee.Substring(0, 198);
                            }

                            vm.Reporter = updater;
                            if (vm.Reporter.Length > 200)
                            {
                                vm.Reporter = vm.Reporter.Substring(0, 198);
                            }

                            vm.Resolution = Resolute.Pending;

                            vm.ResolvedDate = DateTime.Parse("1982-05-06 01:01:01");

                            vm.RMAFailureCode = "";

                            vm.Description = "";
                            vm.CommentType = COMMENTTYPE.Description;

                            vm.StoreIssue();
                            UserController.RegisterUserAuto(vm.Assignee);
                            SendRMAEvent(vm, "created",true);
                        }
                    }//end for
                    return RedirectToAction("ViewAll", "Project");
                }
            }

            return View();
        }

    }
}