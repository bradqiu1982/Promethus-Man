using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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
                            return View();
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
            return View();
        }

    }
}