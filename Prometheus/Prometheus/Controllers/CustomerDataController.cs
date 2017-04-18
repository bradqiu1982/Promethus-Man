using Prometheus.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Excel = Microsoft.Office.Interop.Excel;
using System.Diagnostics;

namespace Prometheus.Controllers
{


    public class CustomerDataController : Controller
    {
        public ActionResult CommitVcselData()
        {
            return View();
        }

        private static void logthdinfo(string info)
        {
            var filename = "d:\\log\\excelexception-" + DateTime.Now.ToString("yyyy-MM-dd");
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

        private static List<List<string>> RetrieveDataFromExcelWithAuth(Controller ctrl,string filename)
        {
            try
            {
                var syscfgdict = CfgUtility.GetSysConfig(ctrl);
                var folderuser = syscfgdict["SHAREFOLDERUSER"];
                var folderdomin = syscfgdict["SHAREFOLDERDOMIN"];
                var folderpwd = syscfgdict["SHAREFOLDERPWD"];

                using (NativeMethods cv = new NativeMethods(folderuser, folderdomin, folderpwd))
                {
                    return ExcelReader.RetrieveDataFromExcel(filename,null);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        [HttpPost, ActionName("CommitVcselData")]
        [ValidateAntiForgeryToken]
        public ActionResult CommitVcselDataPost()
        {
            var wholefn = "";
            try
            {
                if (!string.IsNullOrEmpty(Request.Form["VcselFileName"]))
                {
                    var customereportfile = Request.Form["VcselFileName"];
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
                        var data = RetrieveDataFromExcelWithAuth(this,wholefn);
                        if (data.Count > 0)
                        {
                            ViewBag.ROWCOUNT = data.Count;
                            ViewBag.COLCOUNT = data[0].Count;
                            return View("ConfirmVcselData",data);
                        }
                    }

                }//end if
            }
            catch (Exception ex)
            {  }

            return View();
        }

        [HttpPost, ActionName("ConfirmVcselData")]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmVcselDataPost()
        {
            if (Request.Form["confirmdata"] != null)
            {
                var rowcnt = Convert.ToInt32(Request.Form["rowcount"]);
                var colcnt = Convert.ToInt32(Request.Form["colcount"]);
                var data = new List<List<string>>();
                for (var row = 0; row < rowcnt; row++)
                {
                    if (row != 0)
                    {
                        var line = new List<string>();
                        for (var col = 0; col < colcnt; col++)
                        {
                            line.Add(Request.Form["row" + row + "col" + col]);
                        }
                        data.Add(line);
                    }
                }

                foreach (var line in data)
                {
                    VcselViewModel.StoreVcselData(line);
                }
            }

            return RedirectToAction("ReviewVcselData","CustomerData");
        }


        public ActionResult ReviewVcselData()
        {
            var data = VcselViewModel.RetrieveVcselData();
            var vm = new List<List<string>>();

            var titles = new List<string>();
            titles.Add("Module_Desc");
            titles.Add("Module_Name");
            titles.Add("Module_AgilePN");
            titles.Add("VCSEL_AgilePN");
            titles.Add("VCSEL_Description");
            titles.Add("PD_AgilePN");
            titles.Add("PD_Description");
            titles.Add("MPD_AgilePN");
            titles.Add("PBI_VCSEL_Temp");
            titles.Add("PBI_VCSEL_Bias");
            titles.Add("PBI_VCSEL_Time");
            titles.Add("Allen_PN");

            vm.Add(titles);
            foreach (var line in data)
            {
                if (string.IsNullOrEmpty(line[1].Trim()))
                {
                    continue;
                }

                var showline = new List<string>();
                for (var i = 0; i < titles.Count; i++)
                {
                   showline.Add(line[i]);
                }
                vm.Add(showline);
            }
            return View(vm);
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

        public ActionResult CommitOBAData()
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

        public ActionResult CommitQualityData()
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

        public ActionResult CommitRelData()
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
                    var originalname = Path.GetFileNameWithoutExtension(customereportfile).Replace(" ", "_").Replace("#", "").Replace("'", "")
                    .Replace("&", "").Replace("?", "").Replace("%", "").Replace("+", "");

                    foreach (string fl in Request.Files)
                    {
                        if (fl != null && Request.Files[fl].ContentLength > 0)
                        {
                            string fn = Path.GetFileName(Request.Files[fl].FileName).Replace(" ", "_").Replace("#", "").Replace("'", "")
                    .Replace("&", "").Replace("?", "").Replace("%", "").Replace("+", "");

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
                        var data = RetrieveDataFromExcelWithAuth(this,wholefn);
                        var realdata = new List<List<string>>();
                        if (data.Count > 1)
                        {
                            var templine = new List<string>();
                            templine.Add("Project");
                            templine.Add("Finisar RMA");
                            templine.Add("Finisar Model");
                            templine.Add("Model SN");
                            templine.Add("Assignee Email");
                            templine.Add("Open Date");
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

        [HttpPost, ActionName("CommitOBAData")]
        [ValidateAntiForgeryToken]
        public ActionResult CommitOBADataPost()
        {
            var projlist = ProjectViewModels.RetrieveAllProjectKey();

            var wholefn = "";
            try
            {
                if (!string.IsNullOrEmpty(Request.Form["RMAFileName"]))
                {
                    var customereportfile = Request.Form["RMAFileName"];
                    var originalname = Path.GetFileNameWithoutExtension(customereportfile).Replace(" ", "_").Replace("#", "").Replace("'", "")
                    .Replace("&", "").Replace("?", "").Replace("%", "").Replace("+", "");

                    foreach (string fl in Request.Files)
                    {
                        if (fl != null && Request.Files[fl].ContentLength > 0)
                        {
                            string fn = Path.GetFileName(Request.Files[fl].FileName).Replace(" ", "_").Replace("#", "").Replace("'", "")
                    .Replace("&", "").Replace("?", "").Replace("%", "").Replace("+", "");

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
                        var data = RetrieveDataFromExcelWithAuth(this,wholefn);
                        var realdata = new List<List<string>>();
                        if (data.Count > 1)
                        {
                            var templine = new List<string>();
                            templine.Add("Project");
                            templine.Add("DMR NUM");
                            templine.Add("Model SN");
                            templine.Add("Failure Rate");
                            templine.Add("OBA Description");
                            templine.Add("Priority");
                            templine.Add("Product Type");
                            templine.Add("Assignee Email");
                            templine.Add("Open Date");
                            realdata.Add(templine);

                            for (var idx = 0; idx < data.Count; idx++)
                            {
                                if (idx != 0)
                                {
                                    var snlist = data[idx][5].Split(new string[] { "\r", "\n", " ", ";","," }, StringSplitOptions.RemoveEmptyEntries);
                                    //var reasons = data[idx][10].Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                                    foreach (var sn in snlist)
                                    {
                                        templine = new List<string>();

                                        var trimprojectname = RMSpectialCh(data[idx][3]).ToUpper();
                                        if (trimprojectname.Length > 40)
                                        {
                                            trimprojectname = trimprojectname.Substring(0, 38);
                                        }

                                        var pjname = string.Empty;
                                        foreach (var item in projlist)
                                        {
                                            if (string.Compare(item, trimprojectname, true) == 0)
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

                                        templine.Add(data[idx][2]);
                                        templine.Add(sn);
                                        templine.Add(data[idx][4]);
                                        templine.Add(data[idx][7]);

                                        var Priority = ISSUEPR.Major;
                                        if (string.Compare(data[idx][8],ISSUEPR.Minor,true) == 0)
                                        {
                                            Priority = ISSUEPR.Minor;
                                        }
                                        if (string.Compare(data[idx][8], ISSUEPR.Critical, true) == 0)
                                        {
                                            Priority = ISSUEPR.Critical;
                                        }

                                        templine.Add(Priority);

                                        //product type
                                        var trimprojectname1 = RMSpectialCh(data[idx][9]).ToUpper();
                                        if (trimprojectname.Length > 40)
                                        {
                                            trimprojectname1 = trimprojectname1.Substring(0, 38);
                                        }
                                        templine.Add(trimprojectname1);


                                        if (!data[idx][6].Contains("@"))
                                        {
                                            templine.Add(data[idx][6].Trim().Replace(" ", ".") + "@finisar.com");
                                        }
                                        else
                                        {
                                            templine.Add(data[idx][6]);
                                        }

                                        templine.Add(data[idx][1]);

                                        realdata.Add(templine);
                                    }//end foreach
                                }//end if
                            }//end for
                        }


                        if (realdata.Count > 1)
                        {
                            ViewBag.ROWCOUNT = realdata.Count;
                            ViewBag.COLCOUNT = realdata[0].Count;
                            return View("ConfirmOBAData", realdata);
                        }
                    }

                }//end if
            }
            catch (Exception ex)
            { }

            return View();
        }

        [HttpPost, ActionName("CommitQualityData")]
        [ValidateAntiForgeryToken]
        public ActionResult CommitQualityDataPost()
        {
            var projlist = ProjectViewModels.RetrieveAllProjectKey();

            var wholefn = "";
            try
            {
                if (!string.IsNullOrEmpty(Request.Form["RMAFileName"]))
                {
                    var customereportfile = Request.Form["RMAFileName"];
                    var originalname = Path.GetFileNameWithoutExtension(customereportfile).Replace(" ", "_").Replace("#", "").Replace("'", "")
                    .Replace("&", "").Replace("?", "").Replace("%", "").Replace("+", "");

                    foreach (string fl in Request.Files)
                    {
                        if (fl != null && Request.Files[fl].ContentLength > 0)
                        {
                            string fn = Path.GetFileName(Request.Files[fl].FileName).Replace(" ", "_").Replace("#", "").Replace("'", "")
                    .Replace("&", "").Replace("?", "").Replace("%", "").Replace("+", "");

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
                        var data = RetrieveDataFromExcelWithAuth(this,wholefn);
                        var realdata = new List<List<string>>();
                        if (data.Count > 1)
                        {
                            var templine = new List<string>();
                            templine.Add("Project");
                            templine.Add("Failure Mode");
                            templine.Add("Product Type");
                            templine.Add("Affect Range");
                            templine.Add("Description");
                            templine.Add("Priority");
                            templine.Add("Assignee Email");
                            templine.Add("Open Date");
                            realdata.Add(templine);

                            for (var idx = 0; idx < data.Count; idx++)
                            {
                                if (idx != 0)
                                {
                                        templine = new List<string>();

                                        var trimprojectname = RMSpectialCh(data[idx][1]).ToUpper();
                                        if (trimprojectname.Length > 40)
                                        {
                                            trimprojectname = trimprojectname.Substring(0, 38);
                                        }

                                        var pjname = string.Empty;
                                        foreach (var item in projlist)
                                        {
                                            if (string.Compare(item, trimprojectname, true) == 0)
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

                                        templine.Add(data[idx][2]);

                                        //product type
                                        var trimprojectname1 = RMSpectialCh(data[idx][3]).ToUpper();
                                        if (trimprojectname.Length > 40)
                                        {
                                            trimprojectname1 = trimprojectname1.Substring(0, 38);
                                        }
                                        templine.Add(trimprojectname1);

                                        templine.Add(data[idx][4]);
                                        templine.Add(data[idx][5]);

                                        var Priority = ISSUEPR.Major;
                                        if (string.Compare(data[idx][6], ISSUEPR.Minor, true) == 0)
                                        {
                                            Priority = ISSUEPR.Minor;
                                        }
                                        if (string.Compare(data[idx][6], ISSUEPR.Critical, true) == 0)
                                        {
                                            Priority = ISSUEPR.Critical;
                                        }

                                        templine.Add(Priority);

                                        if (!data[idx][7].Contains("@"))
                                        {
                                            templine.Add(data[idx][7].Trim().Replace(" ", ".") + "@finisar.com");
                                        }
                                        else
                                        {
                                            templine.Add(data[idx][7]);
                                        }

                                        templine.Add(data[idx][0]);

                                        realdata.Add(templine);

                                }//end if
                            }//end for
                        }


                        if (realdata.Count > 1)
                        {
                            ViewBag.ROWCOUNT = realdata.Count;
                            ViewBag.COLCOUNT = realdata[0].Count;
                            return View("ConfirmQualityData", realdata);
                        }
                    }

                }//end if
            }
            catch (Exception ex)
            { }

            return View();
        }

        [HttpPost, ActionName("CommitRelData")]
        [ValidateAntiForgeryToken]
        public ActionResult CommitRelDataPost()
        {
            var projlist = ProjectViewModels.RetrieveAllProjectKey();

            var wholefn = "";
            try
            {
                if (!string.IsNullOrEmpty(Request.Form["RMAFileName"]))
                {
                    var customereportfile = Request.Form["RMAFileName"];
                    var originalname = Path.GetFileNameWithoutExtension(customereportfile).Replace(" ", "_").Replace("#", "").Replace("'", "")
                    .Replace("&", "").Replace("?", "").Replace("%", "").Replace("+", "");

                    foreach (string fl in Request.Files)
                    {
                        if (fl != null && Request.Files[fl].ContentLength > 0)
                        {
                            string fn = Path.GetFileName(Request.Files[fl].FileName).Replace(" ", "_").Replace("#", "").Replace("'", "")
                    .Replace("&", "").Replace("?", "").Replace("%", "").Replace("+", "");

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
                        var data = RetrieveDataFromExcelWithAuth(this,wholefn);
                        var realdata = new List<List<string>>();
                        if (data.Count > 1)
                        {
                            var templine = new List<string>();
                            templine.Add("Project");
                            templine.Add("Occurrence Date");
                            templine.Add("Qual Type");
                            templine.Add("Request ID");
                            templine.Add("Line Category");
                            templine.Add("Product");

                            templine.Add("Test Type");
                            templine.Add("Failure Interval");
                            templine.Add("SN");
                            templine.Add("Fail QTY");
                            templine.Add("Total QTY");
                            templine.Add("Analyst");
                            templine.Add("Rel Engineer");
                            templine.Add("Test Failure");
                            templine.Add("Location");
                            realdata.Add(templine);

                            for (var idx = 1; idx < data.Count; idx++)
                            {
                                if (idx != 0)
                                {
                                    templine = new List<string>();

                                    var trimprojectname = RMSpectialCh(data[idx][0]).ToUpper();
                                    if (trimprojectname.Length > 40)
                                    {
                                        trimprojectname = trimprojectname.Substring(0, 38);
                                    }

                                    var pjname = string.Empty;
                                    foreach (var item in projlist)
                                    {
                                        if (string.Compare(item, trimprojectname, true) == 0)
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

                                    templine.Add(data[idx][3]);
                                    templine.Add(data[idx][5]);
                                    templine.Add(data[idx][6]);
                                    templine.Add(data[idx][7]);

                                    //product type
                                    var trimprojectname1 = RMSpectialCh(data[idx][8]).ToUpper();
                                    if (trimprojectname.Length > 40)
                                    {
                                        trimprojectname1 = trimprojectname1.Substring(0, 38);
                                    }
                                    templine.Add(trimprojectname1);

                                    templine.Add(data[idx][9]);
                                    templine.Add(data[idx][10]);
                                    templine.Add(data[idx][11].Replace("\r\n",":").Replace("\r", ":").Replace("\n", ":"));
                                    templine.Add(data[idx][12]);
                                    templine.Add(data[idx][13]);

                                    templine.Add(data[idx][14].Split('/')[0].Split('\r')[0].Split('\n')[0].Split((char)10)[0]);
                                    templine.Add(data[idx][15].Split('/')[0].Split('\r')[0].Split('\n')[0].Split((char)10)[0]);
                                    templine.Add(data[idx][16]);
                                    templine.Add(data[idx][21]);

                                    realdata.Add(templine);

                                }//end if
                            }//end for
                        }


                        if (realdata.Count > 1)
                        {
                            ViewBag.ROWCOUNT = realdata.Count;
                            ViewBag.COLCOUNT = realdata[0].Count;
                            return View("ConfirmRelData", realdata);
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

                var netcomputername = "";
                try { netcomputername = System.Net.Dns.GetHostName(); }
                catch (Exception ex) { }
                validatestr = validatestr.Replace("//localhost", "//" + netcomputername);

                var content = vm.Summary + " is " + operate + " by " + vm.Reporter + " :\r\n " + validatestr;

                var toaddrs = new List<string>();
                toaddrs.AddRange(vm.RelativePeopleList);
                toaddrs.Add(vm.Assignee);
                toaddrs.Add(vm.Reporter);

                var reporter = vm.Reporter.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries)[0].Replace(".", " ");
                EmailUtility.SendEmail(this,"WUXI NPI System_"+reporter, toaddrs, content);
                new System.Threading.ManualResetEvent(false).WaitOne(500);
            }
        }

        private void SendOBAEvent(IssueViewModels vm, string operate, bool nocheck = false)
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

                var netcomputername = "";
                try { netcomputername = System.Net.Dns.GetHostName(); }
                catch (Exception ex) { }
                validatestr = validatestr.Replace("//localhost", "//" + netcomputername);

                var content = vm.Summary + " is " + operate + " by " + vm.Reporter + " :\r\n " + validatestr;

                var toaddrs = new List<string>();
                toaddrs.AddRange(vm.RelativePeopleList);
                toaddrs.Add(vm.Assignee);
                toaddrs.Add(vm.Reporter);
                EmailUtility.SendEmail(this,"WUXI NPI System", toaddrs, content);
                new System.Threading.ManualResetEvent(false).WaitOne(500);
            }
        }

        private void SendQualityEvent(IssueViewModels vm, string operate, bool nocheck = false)
        {
            var alertime = vm.RetrieveAlertEmailDate(vm.IssueKey);
            if (nocheck||(!string.IsNullOrEmpty(alertime) && DateTime.Parse(alertime).AddHours(24) < DateTime.Now))
            {
                vm.UpdateAlertEmailDate();

                var routevalue = new RouteValueDictionary();
                routevalue.Add("issuekey", vm.IssueKey);
                //send validate email
                string scheme = this.Url.RequestContext.HttpContext.Request.Url.Scheme;
                string validatestr = this.Url.Action("UpdateIssue", "Issue", routevalue, scheme);

                var netcomputername = "";
                try { netcomputername = System.Net.Dns.GetHostName(); }
                catch (Exception ex) { }
                validatestr = validatestr.Replace("//localhost", "//" + netcomputername);

                var content = vm.Summary + " is " + operate + " by " + vm.Reporter + " :\r\n " + validatestr;

                var toaddrs = new List<string>();
                toaddrs.AddRange(vm.RelativePeopleList);
                toaddrs.Add(vm.Assignee);
                toaddrs.Add(vm.Reporter);

                var reporter = vm.Reporter.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries)[0].Replace(".", " ");
                EmailUtility.SendEmail(this,"WUXI NPI System_"+reporter, toaddrs, content);
                new System.Threading.ManualResetEvent(false).WaitOne(500);
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

                            vm.Summary = "RMA " + vm.FinisarRMA + " for module " + vm.ModuleSN + " " + vm.FinisarModel + " from " + vm.ECustomer + ":" + vm.CReport;
                            if (vm.Summary.Length > 200)
                            {
                                vm.Summary = vm.Summary.Substring(0, 198);
                            }

                            vm.Priority = ISSUEPR.Major;
                            try
                            {
                                vm.DueDate = DateTime.Parse(data[i][5]).AddDays(12);
                            }
                            catch (Exception e) { vm.DueDate = DateTime.Now.AddDays(12); }
                            

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
                            vm.FVCode = "";
                            vm.Description = vm.Summary;
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

        [HttpPost, ActionName("ConfirmOBAData")]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmOBADataPost()
        {
            //templine.Add("Project");
            //templine.Add("DMR NUM");
            //templine.Add("Model SN");
            //templine.Add("Failure Rate");
            //templine.Add("OBA Description");
            //templine.Add("Priority");
            //templine.Add("Product Type");
            //templine.Add("Assignee Email");
            //templine.Add("Open Date");

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


                if (data.Count > 1 && data[0].Count == 9)
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
                            vm.IssueType = ISSUETP.OBA;

                            vm.FinisarDMR = data[i][1];
                            if (vm.FinisarDMR.Length > 50)
                            {
                                vm.FinisarDMR = vm.FinisarDMR.Substring(0, 48);
                            }

                            vm.ModuleSN = data[i][2];
                            if (vm.ModuleSN.Length > 50)
                            {
                                vm.ModuleSN = vm.ModuleSN.Substring(0, 48);
                            }

                            vm.OBAFailureRate = data[i][3];
                            if (vm.OBAFailureRate.Length > 50)
                            {
                                vm.OBAFailureRate = vm.OBAFailureRate.Substring(0, 48);
                            }

                            vm.RelativePeoples = "";
                            
                            vm.Summary = "OBA " + vm.FinisarDMR + " for module " + vm.ModuleSN + ":" + data[i][4];
                            if (vm.Summary.Length > 200)
                            {
                                vm.Summary = vm.Summary.Substring(0, 198);
                            }

                            vm.Priority = data[i][5];

                            vm.ProductType = data[i][6];
                            if (vm.ProductType.Length > 50)
                            {
                                vm.ProductType = vm.ProductType.Substring(0, 48);
                            }

                            vm.ReportDate = DateTime.Now;
                            vm.Assignee = data[i][7].ToUpper();
                            if (vm.Assignee.Length > 200)
                            {
                                vm.Assignee = vm.Assignee.Substring(0, 198);
                            }

                            try
                            {
                                vm.DueDate = DateTime.Parse(data[i][8]).AddDays(6);
                            }
                            catch (Exception e) { vm.DueDate = DateTime.Now.AddDays(6); }

                            vm.Reporter = updater;
                            if (vm.Reporter.Length > 200)
                            {
                                vm.Reporter = vm.Reporter.Substring(0, 198);
                            }

                            vm.Resolution = Resolute.Pending;

                            vm.ResolvedDate = DateTime.Parse("1982-05-06 01:01:01");

                            vm.Description = vm.Summary;
                            vm.CommentType = COMMENTTYPE.Description;

                            vm.DataID = "";
                            vm.ErrAbbr = "";
                            vm.FVCode = "";
                            vm.MaterialDisposition = "";

                            vm.StoreIssue();

                            UserController.RegisterUserAuto(vm.Assignee);

                            SendOBAEvent(vm, "created", true);
                        }
                    }//end for
                    return RedirectToAction("ViewAll", "Project");
                }
            }

            return View();
        }

        [HttpPost, ActionName("ConfirmQualityData")]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmQualityDataPost()
        {
            //templine.Add("Project");
            //templine.Add("Failure Mode");
            //templine.Add("Product Type");
            //templine.Add("Affect Range");
            //templine.Add("Description");
            //templine.Add("Priority");
            //templine.Add("Assignee Email");
            //templine.Add("Open Date");

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
                            vm.IssueType = ISSUETP.Quality;

                            vm.RMAFailureCode = data[i][1];
                            if (vm.RMAFailureCode.Length > 50)
                            {
                                vm.RMAFailureCode = vm.RMAFailureCode.Substring(0, 48);
                            }

                            vm.ProductType = data[i][2];
                            if (vm.ProductType.Length > 50)
                            {
                                vm.ProductType = vm.ProductType.Substring(0, 48);
                            }

                            vm.AffectRange = data[i][3];
                            if (vm.AffectRange.Length > 50)
                            {
                                vm.AffectRange = vm.AffectRange.Substring(0, 48);
                            }

                            vm.RelativePeoples = "";

                            vm.Summary = "Quality issue " + vm.RMAFailureCode + " :" + data[i][4];
                            if (vm.Summary.Length > 200)
                            {
                                vm.Summary = vm.Summary.Substring(0, 198);
                            }

                            vm.Priority = data[i][5];


                            vm.ReportDate = DateTime.Now;
                            vm.Assignee = data[i][6].ToUpper();
                            if (vm.Assignee.Length > 200)
                            {
                                vm.Assignee = vm.Assignee.Substring(0, 198);
                            }

                            try
                            {
                                vm.DueDate = DateTime.Parse(data[i][7]).AddDays(12);
                            }
                            catch (Exception e) { vm.DueDate = DateTime.Now.AddDays(12); }

                            vm.Reporter = updater;
                            if (vm.Reporter.Length > 200)
                            {
                                vm.Reporter = vm.Reporter.Substring(0, 198);
                            }

                            vm.Resolution = Resolute.Pending;

                            vm.ResolvedDate = DateTime.Parse("1982-05-06 01:01:01");

                            vm.Description = vm.Summary;
                            vm.CommentType = COMMENTTYPE.Description;

                            vm.DataID = "";
                            vm.ErrAbbr = "";
                            vm.FVCode = "";
                            vm.MaterialDisposition = "";

                            vm.StoreIssue();

                            UserController.RegisterUserAuto(vm.Assignee);

                            SendQualityEvent(vm, "created", true);
                        }
                    }//end for
                    return RedirectToAction("ViewAll", "Project");
                }
            }

            return View();
        }

        public DateTime ConvertDate(string d)
        {
            try
            {
                return DateTime.Parse(d);
            }
            catch (Exception ex)
            {
                return DateTime.Now;
            }
        }
        
        [HttpPost, ActionName("ConfirmRelData")]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmRelDataPost()
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


                if (data.Count > 1)
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

                            vm.ReportDate = ConvertDate(data[i][1]);
                            vm.QualType = data[i][2];
                            vm.RequestID = data[i][3];
                            vm.LineCategory = data[i][4];
                            vm.ProductType = data[i][5];
                            vm.TestType = data[i][6];
                            vm.FailureInterval = data[i][7];
                            vm.ModuleSN = data[i][8];
                            if (vm.ModuleSN.Length > 50)
                            {
                                vm.ModuleSN = vm.ModuleSN.Substring(0, 48);
                            }
                            vm.FailQTY = data[i][9];
                            vm.TotalQTY = data[i][10];
                            vm.Assignee = data[i][11].Split('/')[0].Split('\r')[0].Split('\n')[0].Split((char)10)[0].ToUpper();
                            if (!vm.Assignee.Contains("@"))
                            {
                                vm.Assignee = (vm.Assignee.Replace(" ", ".") + "@finisar.com").ToUpper();
                            }

                            vm.Reporter = data[i][12].Split('/')[0].Split('\r')[0].Split('\n')[0].Split((char)10)[0].ToUpper();
                            if (!vm.Reporter.Contains("@"))
                            {
                                vm.Reporter = (vm.Reporter.Replace(" ", ".") + "@finisar.com").ToUpper();
                            }

                            vm.Summary = data[i][13].Split('\r')[0].Split('\n')[0];
                            if (vm.Summary.Length > 200)
                            {
                                vm.Summary = vm.Summary.Substring(0, 198);
                            }

                            vm.Location = data[i][14];

                            UserController.RegisterUserAuto(vm.Assignee);
                            UserController.RegisterUserAuto(vm.Reporter);

                            vm.IssueKey = IssueViewModels.GetUniqKey();
                            vm.IssueType = ISSUETP.Rel;
                            vm.Priority = ISSUEPR.Major;

                            vm.RelativePeoples = "";
                            vm.FVCode = "";

                            vm.DueDate = vm.ReportDate.AddDays(21);
                            vm.Resolution = Resolute.Pending;
                            vm.ResolvedDate = DateTime.Parse("1982-05-06 01:01:01");

                            vm.Description = vm.Summary;
                            vm.CommentType = COMMENTTYPE.Description;

                            vm.StoreIssue();

                            SendQualityEvent(vm, "created", true);
                        }
                    }//end for
                    return RedirectToAction("ViewAll", "Project");
                }
            }

            return View();
        }


        private List<string> PrepeareVcselReport()
        {
            var ret = new List<string>();

            var data = VcselViewModel.RetrieveVcselData();
            var vm = new List<List<string>>();
            var titles = new List<string>();
            titles.Add("Module_Desc");
            titles.Add("Module_Name");
            titles.Add("Module_AgilePN");
            titles.Add("VCSEL_AgilePN");
            titles.Add("VCSEL_Description");
            titles.Add("PD_AgilePN");
            titles.Add("PD_Description");
            titles.Add("MPD_AgilePN");
            titles.Add("PBI_VCSEL_Temp");
            titles.Add("PBI_VCSEL_Bias");
            titles.Add("PBI_VCSEL_Time");
            titles.Add("Allen_PN");

            vm.Add(titles);
            foreach (var line in data)
            {
                if (string.IsNullOrEmpty(line[1].Trim()))
                {
                    continue;
                }

                var showline = new List<string>();
                for (var i = 0; i < titles.Count; i++)
                {
                    showline.Add(line[i]);
                }
                vm.Add(showline);
            }

            foreach (var line in vm)
            {
                var ls = "";
                foreach (var item in line)
                {
                    ls = ls + "\"" + item.Replace("\"", "").Trim() + "\",";
                }
                ret.Add(ls);
            }

            return ret;
        }

        public ActionResult ExportVcselData()
        {
            string datestring = DateTime.Now.ToString("yyyyMMdd");
            string imgdir = Server.MapPath("~/userfiles") + "\\docs\\" + datestring + "\\";
            if (!Directory.Exists(imgdir))
            {
                Directory.CreateDirectory(imgdir);
            }

            var fn = "Vcsel_Usage_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
            var filename = imgdir + fn;

            var lines = PrepeareVcselReport();

            var wholefile = "";
            foreach (var l in lines)
            {
                wholefile = wholefile + l + "\r\n";
            }
            System.IO.File.WriteAllText(filename, wholefile);

            return File(filename, "application/vnd.ms-excel", fn);
        }


        public ActionResult CommitUserMatrix()
        {
            return View();
        }

        [HttpPost, ActionName("CommitUserMatrix")]
        [ValidateAntiForgeryToken]
        public ActionResult CommitUserMatrixPost()
        {
            var wholefn = "";
            try
            {
                if (!string.IsNullOrEmpty(Request.Form["RMAFileName"]))
                {
                    var customereportfile = Request.Form["RMAFileName"];
                    var originalname = Path.GetFileNameWithoutExtension(customereportfile).Replace(" ", "_").Replace("#", "").Replace("'", "")
                    .Replace("&", "").Replace("?", "").Replace("%", "").Replace("+", "");

                    foreach (string fl in Request.Files)
                    {
                        if (fl != null && Request.Files[fl].ContentLength > 0)
                        {
                            string fn = Path.GetFileName(Request.Files[fl].FileName).Replace(" ", "_").Replace("#", "").Replace("'", "")
                    .Replace("&", "").Replace("?", "").Replace("%", "").Replace("+", "");

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

                    int idx = 0;
                    if (!string.IsNullOrEmpty(wholefn))
                    {
                        var data = RetrieveDataFromExcelWithAuth(this, wholefn);
                        if (data.Count > 1)
                        {
                            foreach (var line in data)
                            {
                                if (idx != 0)
                                {
                                    UserMatrixVM.UpdateUserMatrix(line[0], line[1], line[2]);
                                }
                                idx = idx + 1;
                            }
                        }//end if
                    }//end if

                }//end if
            }
            catch (Exception ex)
            { }

            return View();
        }


        public ActionResult ReviewRMABackupData()
        {
            var vm = ExternalDataCollector.RetrieveAllRMAData();
            return View(vm);
        }


    }
}