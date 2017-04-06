using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prometheus.Models
{
    public class ExternalDataCollector
    {
        private static List<List<string>> RetrieveDataFromExcelWithAuth(Controller ctrl, string filename)
        {
            try
            {
                var syscfgdict = CfgUtility.GetSysConfig(ctrl);
                var folderuser = syscfgdict["SHAREFOLDERUSER"];
                var folderdomin = syscfgdict["SHAREFOLDERDOMIN"];
                var folderpwd = syscfgdict["SHAREFOLDERPWD"];

                using (NativeMethods cv = new NativeMethods(folderuser, folderdomin, folderpwd))
                {
                    return ExcelReader.RetrieveDataFromExcel(filename, null);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static bool FileExist(Controller ctrl, string filename)
        {
            try
            {
                var syscfgdict = CfgUtility.GetSysConfig(ctrl);
                var folderuser = syscfgdict["SHAREFOLDERUSER"];
                var folderdomin = syscfgdict["SHAREFOLDERDOMIN"];
                var folderpwd = syscfgdict["SHAREFOLDERPWD"];

                using (NativeMethods cv = new NativeMethods(folderuser, folderdomin, folderpwd))
                {
                    return File.Exists(filename);
                }
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        private static void FileCopy(Controller ctrl, string src, string des, bool overwrite, bool checklocal = false)
        {
            try
            {


                var syscfgdict = CfgUtility.GetSysConfig(ctrl);
                var folderuser = syscfgdict["SHAREFOLDERUSER"];
                var folderdomin = syscfgdict["SHAREFOLDERDOMIN"];
                var folderpwd = syscfgdict["SHAREFOLDERPWD"];

                using (NativeMethods cv = new NativeMethods(folderuser, folderdomin, folderpwd))
                {
                    if (checklocal)
                    {
                        if (File.Exists(des))
                        {
                            return;
                        }
                    }

                    File.Copy(src, des, overwrite);
                }
            }
            catch (Exception ex)
            {
            }
        }

        private static bool DirectoryExists(Controller ctrl, string dirname)
        {
            try
            {
                var syscfgdict = CfgUtility.GetSysConfig(ctrl);
                var folderuser = syscfgdict["SHAREFOLDERUSER"];
                var folderdomin = syscfgdict["SHAREFOLDERDOMIN"];
                var folderpwd = syscfgdict["SHAREFOLDERPWD"];

                using (NativeMethods cv = new NativeMethods(folderuser, folderdomin, folderpwd))
                {
                    return Directory.Exists(dirname);
                }
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        private static string ConvertToDate(string datestr)
        {
            if (string.IsNullOrEmpty(datestr))
            {
                return string.Empty;
            }
            try
            {
                return DateTime.Parse(datestr).ToString();
            }
            catch (Exception ex) { return string.Empty; }
        }

        public static void RefreshRMAData(Controller ctrl)
        {
            var syscfgdict = CfgUtility.GetSysConfig(ctrl);
            var rmasrcfile = syscfgdict["RMASHAREFILE"];

            if (FileExist(ctrl, rmasrcfile))
            {
                string datestring = DateTime.Now.ToString("yyyyMMdd");
                string imgdir = ctrl.Server.MapPath("~/userfiles") + "\\docs\\" + datestring + "\\";
                if (!DirectoryExists(ctrl, imgdir))
                {
                    Directory.CreateDirectory(imgdir);
                }

                var desfile = imgdir + Path.GetFileName(rmasrcfile);
                FileCopy(ctrl, rmasrcfile, desfile, true);

                if (FileExist(ctrl, desfile))
                {
                    var data = RetrieveDataFromExcelWithAuth(ctrl, desfile);
                }//copied file exist
            }//end if src file exist
        }

        private static void SolveRMAData(List<List<string>> data, Controller ctrl)
        {
            var allrmaissue = IssueViewModels.RetrieveAllIssueTypeIssue("NONE", "NONE", ISSUETP.RMA, ctrl);
            var rmaissuedict = new Dictionary<string, string>();
            foreach (var issue in allrmaissue)
            {
                if (rmaissuedict.ContainsKey(issue.FinisarRMA))
                {
                    rmaissuedict[issue.FinisarRMA] = rmaissuedict[issue.FinisarRMA] + ":" + issue.ModuleSN.ToUpper();
                }
                else
                {
                    rmaissuedict.Add(issue.FinisarRMA, issue.ModuleSN.ToUpper());
                }
            }//end foreach

            var usermatrix = UserMatrixVM.RetrieveUserMatrixDepart();

        }

    }
}