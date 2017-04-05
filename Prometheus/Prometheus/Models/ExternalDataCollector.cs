using System;
using System.Collections.Generic;
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


        //public static List<List<string>> RefreshRMAData(Controller ctrl)
        //{
        //    var syscfgdict = CfgUtility.GetSysConfig(ctrl);

        //}


    }
}