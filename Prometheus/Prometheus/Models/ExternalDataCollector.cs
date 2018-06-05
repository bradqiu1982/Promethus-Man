using Prometheus.Controllers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Prometheus.Models
{

    public class FsrShipData
    {


        public FsrShipData(string id, int qty, string pn, string pndesc, string family, string cfg
            , DateTime shipdate, string custnum, string cust1, string cust2, DateTime orddate,string delievenum)
        {
            ShipID = id;
            ShipQty = qty;
            PN = pn;
            ProdDesc = pndesc;
            MarketFamily = family;
            Configuration = cfg;
            ShipDate = shipdate;
            CustomerNum = custnum;
            Customer1 = cust1;
            Customer2 = cust2;
            OrderedDate = orddate;
            VcselType = string.Empty;
            DelieveNum = delievenum;
        }

        public static Dictionary<string, bool> RetrieveAllShipID()
        {
            var ret = new Dictionary<string, bool>();
            var sql = "select distinct ShipID from FsrShipData";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            { ret.Add(Convert.ToString(line[0]),true); }
            return ret;
        }

        public void StoreShipData()
        {
            var sql = @"insert into FsrShipData(ShipID,ShipQty,PN,ProdDesc,MarketFamily,Configuration,VcselType,ShipDate,CustomerNum,Customer1,Customer2,OrderedDate,DelieveNum) values(
                        @ShipID,@ShipQty,@PN,@ProdDesc,@MarketFamily,@Configuration,@VcselType,@ShipDate,@CustomerNum,@Customer1,@Customer2,@OrderedDate,@DelieveNum)";
            var dict = new Dictionary<string, string>();
            dict.Add("@ShipID", ShipID);
            dict.Add("@ShipQty", ShipQty.ToString());
            dict.Add("@PN", PN);
            dict.Add("@ProdDesc", ProdDesc);
            dict.Add("@MarketFamily", MarketFamily);
            dict.Add("@Configuration", Configuration);
            dict.Add("@VcselType", VcselType);
            dict.Add("@ShipDate", ShipDate.ToString("yyyy-MM-dd HH:mm:ss"));
            dict.Add("@CustomerNum", CustomerNum);
            dict.Add("@Customer1", Customer1);
            dict.Add("@Customer2", Customer2);
            dict.Add("@OrderedDate", OrderedDate.ToString("yyyy-MM-dd HH:mm:ss"));
            dict.Add("@DelieveNum", DelieveNum);

            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public string ShipID { set; get; }
        public double ShipQty { set; get; }
        public string PN { set; get; }
        public string ProdDesc { set; get; }
        public string MarketFamily { set; get; }
        public string Configuration { set; get; }
        public string VcselType { set; get; }
        public DateTime ShipDate { set; get; }
        public string CustomerNum { set; get; }
        public string Customer1 { set; get; }
        public string Customer2 { set; get; }
        public DateTime OrderedDate { set; get; }
        public string DelieveNum { set; get; }
    }

    public class RMAMAPDATATYPE
    {
        public static string ATTACH = "ATTACH";
    }

    public class RMARAWData
    {
        public RMARAWData()
        {
            AppV_A = string.Empty;
            AppV_B = string.Empty;
            AppV_C = string.Empty;
            AppV_D = string.Empty;
            AppV_E = string.Empty;
            AppV_F = string.Empty;
            AppV_G = string.Empty;
            AppV_H = string.Empty;
            AppV_I = string.Empty;
            AppV_J = string.Empty;
            AppV_K = string.Empty;
            AppV_L = string.Empty;
            AppV_M = string.Empty;
            AppV_N = string.Empty;
            AppV_O = string.Empty;
            AppV_P = string.Empty;
            AppV_Q = string.Empty;
            AppV_R = string.Empty;
            AppV_S = string.Empty;
            AppV_T = string.Empty;
            AppV_U = string.Empty;
            AppV_V = string.Empty;
            AppV_W = string.Empty;
            AppV_X = string.Empty;
            AppV_Y = string.Empty;
            AppV_Z = string.Empty;
            AppV_AA = string.Empty;
            AppV_AB = string.Empty;
            AppV_AC = string.Empty;
            AppV_AD = string.Empty;
            AppV_AE = string.Empty;
            AppV_AF = string.Empty;
            AppV_AG = string.Empty;
        }

        public string AppV_A { set; get; }
        public string AppV_B { set; get; }
        public string AppV_C { set; get; }
        public string AppV_D { set; get; }
        public string AppV_E { set; get; }
        public string AppV_F { set; get; }
        public string AppV_G { set; get; }
        public string AppV_H { set; get; }
        public string AppV_I { set; get; }
        public string AppV_J { set; get; }
        public string AppV_K { set; get; }
        public string AppV_L { set; get; }
        public string AppV_M { set; get; }
        public string AppV_N { set; get; }
        public string AppV_O { set; get; }
        public string AppV_P { set; get; }
        public string AppV_Q { set; get; }
        public string AppV_R { set; get; }
        public string AppV_S { set; get; }
        public string AppV_T { set; get; }
        public string AppV_U { set; get; }
        public string AppV_V { set; get; }
        public string AppV_W { set; get; }
        public string AppV_X { set; get; }
        public string AppV_Y { set; get; }
        public string AppV_Z { set; get; }
        public string AppV_AA { set; get; }
        public string AppV_AB { set; get; }
        public string AppV_AC { set; get; }
        public string AppV_AD { set; get; }
        public string AppV_AE { set; get; }
        public string AppV_AF { set; get; }
        public string AppV_AG { set; get; }
    }

    public class NEOMAPData
    {
        public NEOMAPData()
        {
            AppV_A = string.Empty;
            AppV_B = string.Empty;
            AppV_C = string.Empty;
            AppV_D = string.Empty;
            AppV_E = string.Empty;
            AppV_F = -99999;
            AppV_G = string.Empty;
            AppV_H = -99999;
            AppV_I = -99999;
            AppV_J = -99999;
            AppV_K = -99999;
            AppV_L = -99999;
            AppV_M = -99999;
            AppV_N = -99999;
            AppV_O = -99999;
            AppV_P = -99999;
            AppV_Q = -99999;
            AppV_R = -99999;
            AppV_S = -99999;
            AppV_T = -99999;
            AppV_U = -99999;
            AppV_V = -99999;
            AppV_W = -99999;
            AppV_X = -99999;
            AppV_Y = -99999;
            AppV_Z = -99999;
            AppV_AA = -99999;
            AppV_AB = -99999;
            AppV_AC = -99999;

            AppV_AD = -99999;
            AppV_AE = -99999;
            AppV_AF = -99999;
            AppV_AG = -99999;
            AppV_AH = DateTime.Parse("1982-05-06 07:30:00");
            AppV_AI = string.Empty;
            AppV_AJ = string.Empty;

            databackuptm = DateTime.Now;
        }

        public string AppV_A { set; get; }
        public string AppV_B { set; get; }
        public string AppV_C { set; get; }
        public string AppV_D { set; get; }
        public string AppV_E { set; get; }
        public double AppV_F { set; get; }
        public string AppV_G { set; get; }
        public double AppV_H { set; get; }
        public double AppV_I { set; get; }
        public double AppV_J { set; get; }
        public double AppV_K { set; get; }
        public double AppV_L { set; get; }
        public double AppV_M { set; get; }
        public double AppV_N { set; get; }
        public double AppV_O { set; get; }
        public double AppV_P { set; get; }
        public double AppV_Q { set; get; }
        public double AppV_R { set; get; }
        public double AppV_S { set; get; }
        public double AppV_T { set; get; }
        public double AppV_U { set; get; }
        public double AppV_V { set; get; }
        public double AppV_W { set; get; }
        public double AppV_X { set; get; }
        public double AppV_Y { set; get; }
        public double AppV_Z { set; get; }
        public double AppV_AA { set; get; }
        public double AppV_AB { set; get; }
        public double AppV_AC { set; get; }

        public double AppV_AD { set; get; }
        public double AppV_AE { set; get; }
        public double AppV_AF { set; get; }
        public double AppV_AG { set; get; }
        public DateTime AppV_AH { set; get; }
        public string AppV_AI { set; get; }
        public string AppV_AJ { set; get; }

        public DateTime databackuptm { set; get; }
    }

    public class WaferRecord
    {
        public WaferRecord()
        {
            Wafer = string.Empty;
            WaferBinYield = string.Empty;
            WaferBinCount = string.Empty;
            SumCount = string.Empty;
            databackuptm = DateTime.Now;

            AppV_A = string.Empty;
            AppV_B = string.Empty;
            AppV_C = string.Empty;
            AppV_D = string.Empty;
            AppV_E = string.Empty;
            AppV_F = string.Empty;
            AppV_G = -99999;
            AppV_H = -99999;
            AppV_I = -99999;
            AppV_J = -99999;
            AppV_K = -99999;
            AppV_L = -99999;
        }

        public void StoreWaferRecord()
        {
            var sql = "delete from WaferRecord where Wafer = '<Wafer>'";
            sql = sql.Replace("<Wafer>", Wafer);
            DBUtility.ExeLocalSqlNoRes(sql);

            sql = "insert into WaferRecord(Wafer,WaferBinYield,WaferBinCount,SumCount,databackuptm) "
                +" values('<Wafer>','<WaferBinYield>','<WaferBinCount>','<SumCount>','<databackuptm>')";
            sql = sql.Replace("<Wafer>", Wafer).Replace("<WaferBinYield>", WaferBinYield).Replace("<WaferBinCount>", WaferBinCount)
                .Replace("<SumCount>", SumCount).Replace("<databackuptm>", databackuptm.ToString("yyyy-MM-dd HH:mm:ss"));
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static List<WaferRecord> RetrieveWaferRecord()
        {
            var ret = new List<WaferRecord>();
            var sql = "select Wafer,WaferBinYield,WaferBinCount,SumCount,databackuptm from WaferRecord order by databackuptm DESC";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var tempvm = new WaferRecord();
                tempvm.Wafer = Convert.ToString(line[0]);
                tempvm.WaferBinYield = Convert.ToString(line[1]);
                tempvm.WaferBinCount = Convert.ToString(line[2]);
                tempvm.SumCount = Convert.ToString(line[3]);
                tempvm.databackuptm = Convert.ToDateTime(line[4]);
                ret.Add(tempvm);
            }
            return ret;
        }

        public string Wafer { set; get; }
        public string WaferBinYield { set; get; }
        public string WaferBinCount { set; get; }
        public string SumCount { set; get; }
        public DateTime databackuptm { set; get; }

        public string AppV_A { set; get; }
        public string AppV_B { set; get; }
        public string AppV_C { set; get; }
        public string AppV_D { set; get; }
        public string AppV_E { set; get; }
        public string AppV_F { set; get; }
        public double AppV_G { set; get; }
        public double AppV_H { set; get; }
        public double AppV_I { set; get; }
        public double AppV_J { set; get; }
        public double AppV_K { set; get; }
        public double AppV_L { set; get; }
    }


    public class RELRAWData
    {
        public RELRAWData()
        {
            AppV_A = 0;
            AppV_B = string.Empty;
            AppV_C = string.Empty;
            AppV_D = string.Empty;
            AppV_E = string.Empty;
            AppV_F = string.Empty;
            AppV_G = string.Empty;
            AppV_H = string.Empty;
            AppV_I = string.Empty;
            AppV_J = string.Empty;
            AppV_K = string.Empty;
            AppV_L = string.Empty;
            AppV_M = string.Empty;
            AppV_N = string.Empty;
            AppV_O = string.Empty;
            AppV_P = string.Empty;
            AppV_Q = string.Empty;
            AppV_R = string.Empty;
            AppV_S = string.Empty;
            AppV_T = string.Empty;
            AppV_U = string.Empty;
            AppV_V = string.Empty;
            AppV_W = string.Empty;
            AppV_X = string.Empty;
            AppV_Y = string.Empty;
            AppV_Z = string.Empty;
            AppV_AA = string.Empty;
            AppV_AB = string.Empty;
            AppV_AC = string.Empty;
            AppV_AD = string.Empty;
            AppV_AE = string.Empty;
            AppV_AF = string.Empty;
            AppV_AG = string.Empty;
        }

        public int AppV_A { set; get; }
        public string AppV_B { set; get; }
        public string AppV_C { set; get; }
        public string AppV_D { set; get; }
        public string AppV_E { set; get; }
        public string AppV_F { set; get; }
        public string AppV_G { set; get; }
        public string AppV_H { set; get; }
        public string AppV_I { set; get; }
        public string AppV_J { set; get; }
        public string AppV_K { set; get; }
        public string AppV_L { set; get; }
        public string AppV_M { set; get; }
        public string AppV_N { set; get; }
        public string AppV_O { set; get; }
        public string AppV_P { set; get; }
        public string AppV_Q { set; get; }
        public string AppV_R { set; get; }
        public string AppV_S { set; get; }
        public string AppV_T { set; get; }
        public string AppV_U { set; get; }
        public string AppV_V { set; get; }
        public string AppV_W { set; get; }
        public string AppV_X { set; get; }
        public string AppV_Y { set; get; }
        public string AppV_Z { set; get; }
        public string AppV_AA { set; get; }
        public string AppV_AB { set; get; }
        public string AppV_AC { set; get; }
        public string AppV_AD { set; get; }
        public string AppV_AE { set; get; }
        public string AppV_AF { set; get; }
        public string AppV_AG { set; get; }
    }

    public class IQERAWData
    {
        public IQERAWData()
        {
            AppV_A = string.Empty;
            AppV_B = string.Empty;
            AppV_C = string.Empty;
            AppV_D = string.Empty;
            AppV_E = string.Empty;
            AppV_F = string.Empty;
            AppV_G = string.Empty;
            AppV_H = string.Empty;
            AppV_I = string.Empty;
            AppV_J = string.Empty;
            AppV_K = string.Empty;
            AppV_L = string.Empty;
            AppV_M = string.Empty;
            AppV_N = string.Empty;
            AppV_O = string.Empty;
            AppV_P = string.Empty;
            AppV_Q = string.Empty;
            Attachs = new List<string>();
        }

        public string AppV_A { set; get; }
        public string AppV_B { set; get; }
        public string AppV_C { set; get; }
        public string AppV_D { set; get; }
        public string AppV_E { set; get; }
        public string AppV_F { set; get; }
        public string AppV_G { set; get; }
        public string AppV_H { set; get; }
        public string AppV_I { set; get; }
        public string AppV_J { set; get; }
        public string AppV_K { set; get; }
        public string AppV_L { set; get; }
        public string AppV_M { set; get; }
        public string AppV_N { set; get; }
        public string AppV_O { set; get; }
        public string AppV_P { set; get; }
        public string AppV_Q { set; get; }

        public List<string> Attachs { set; get; }
    }

    public class CRITICALERRORTYPE
    {
        public static string LYTTASK1 = "[LYT TASK]";
        public static string LYTTASK = "[Critical Failure Alarm]";
        public static string LYTSUBTASK = "[Critical Failure SUB TASK]";
        public static string CONTAINMENTACTION = "[Containment]";
        public static string CORRECTIVEACTION = "[Corrective]";
        public static string SECONDMATCH1 = "[2nd Match]";
        public static string SECONDMATCH = "[Critical Failure Alarm]";
        public static string CRITICALERRORTAG = "CRITICALERROR";
        public static string PMTASK = "[Project Manage]";
        public static string OCAP = "[OCAP]";
        public static string NOTREALCRITICALISSUE = "[Not Real Critical Issue]";
    }

    public class WEEKLYREPORTMARK
    {
        public static string HighLight = "highlight";
        public static string LowLight = "lowlight";
        public static string Notable = "notable";
    }

    public class RawDMR
    {
        public RawDMR()
        {
            DMR_ID = string.Empty;
            CreateTime = string.Empty;
            Product = string.Empty;
            TotalQty = string.Empty;
            DefectQty = string.Empty;
            Summary = string.Empty;
            ModuleSN = "SN";
            Owner = string.Empty;

            RootCause = string.Empty;
            CorrectiveAction = string.Empty;
            Attachment = string.Empty;
        }

        public string DMR_ID{ set; get; }
        public string CreateTime { set; get; }
        public string Product { set; get; }
        public string TotalQty { set; get; }
        public string DefectQty { set; get; }
        public string Summary { set; get; }
        public string ModuleSN { set; get; }
        public string Owner { set; get; }

        public string RootCause { set; get; }
        public string CorrectiveAction { set; get; }
        public string Attachment { set; get; }
    }

    public class TXOQUERYCOND
    {
        public static string NEOMAP = "NEOMAP-";
        public static string BURNIN = "BURNIN-";
        public static string TEST = "MODULE-";
        public static string PROCESS = "PROCESS-";
    }

    public class TXOQUERYTYPE
    {
        public static string BR = "BR";
        public static string WAFER = "WAFER";
        public static string JO = "JO";
    }

    public class NeoMapDataWithPos
    {
        public int x { set; get; }
        public int y { set; get; }
        public double value { set; get; }
    }

    public class TraceViewData
    {
        public TraceViewData()
        {
            Temp = 25;
            CH = 0;
            Value = "";
        }

        public double Temp { set; get; }
        public int CH { set; get; }
        public double dValue { get {
                if (Value.ToUpper().Contains("0X"))
                {
                    long parsed = long.Parse(Value.ToUpper().Replace("0X", ""), NumberStyles.AllowHexSpecifier);
                    return Convert.ToDouble(parsed);
                }
                else
                {
                    return Convert.ToDouble(Value);
                }
            } }
        public string Value { set; get; }
    }

     public class ExternalDataCollector
    {

        #region FILEOPERATE

        private static List<List<string>> RetrieveDataFromExcelWithAuth(Controller ctrl, string filename,string sheetname = null, int columns = 101)
        {
            try
            {
                var syscfgdict = CfgUtility.GetSysConfig(ctrl);
                var folderuser = syscfgdict["SHAREFOLDERUSER"];
                var folderdomin = syscfgdict["SHAREFOLDERDOMIN"];
                var folderpwd = syscfgdict["SHAREFOLDERPWD"];

                using (NativeMethods cv = new NativeMethods(folderuser, folderdomin, folderpwd))
                {
                    return ExcelReader.RetrieveDataFromExcel(filename, sheetname, columns);
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

        public static string DownloadShareFile(string srcfile, Controller ctrl)
        {
            try
            {
                if (ExternalDataCollector.FileExist(ctrl, srcfile))
                {
                    var filename = System.IO.Path.GetFileName(srcfile);
                    var descfolder = ctrl.Server.MapPath("~/userfiles") + "\\docs\\ShareFile\\";
                    if (!ExternalDataCollector.DirectoryExists(ctrl, descfolder))
                        ExternalDataCollector.CreateDirectory(ctrl, descfolder);
                    var descfile = descfolder + System.IO.Path.GetFileNameWithoutExtension(srcfile) + DateTime.Now.ToString("yyyy-MM-dd") + System.IO.Path.GetExtension(srcfile);
                    ExternalDataCollector.FileCopy(ctrl, srcfile, descfile, true);
                    return descfile;
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
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

        public static void CreateDirectory(Controller ctrl, string dirname)
        {
            try
            {
                var syscfgdict = CfgUtility.GetSysConfig(ctrl);
                var folderuser = syscfgdict["SHAREFOLDERUSER"];
                var folderdomin = syscfgdict["SHAREFOLDERDOMIN"];
                var folderpwd = syscfgdict["SHAREFOLDERPWD"];

                using (NativeMethods cv = new NativeMethods(folderuser, folderdomin, folderpwd))
                {
                    Directory.CreateDirectory(dirname);
                }
            }
            catch (Exception ex)
            { }

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

        private static List<string> DirectoryEnumerateFiles(Controller ctrl, string dirname)
        {
            try
            {
                var syscfgdict = CfgUtility.GetSysConfig(ctrl);
                var folderuser = syscfgdict["SHAREFOLDERUSER"];
                var folderdomin = syscfgdict["SHAREFOLDERDOMIN"];
                var folderpwd = syscfgdict["SHAREFOLDERPWD"];

                using (NativeMethods cv = new NativeMethods(folderuser, folderdomin, folderpwd))
                {
                    var ret = new List<string>();
                    ret.AddRange(Directory.EnumerateFiles(dirname));
                    return ret;
                }
            }
            catch (Exception ex)
            {
                return new List<string>();
            }
        }

        private static string ConvertToDateStr(string datestr)
        {
            if (string.IsNullOrEmpty(datestr))
            {
                return "1982-05-06 10:00:00";
            }
            try
            {
                return DateTime.Parse(datestr).ToString();
            }
            catch (Exception ex) { return "1982-05-06 10:00:00"; }
        }

        private static double ConvertToDoubleVal(string val)
        {
            if (string.IsNullOrEmpty(val))
                return -99999;
            try
            {
                return Convert.ToDouble(val);
            }
            catch (Exception ex)
            {
                return -99999;
            }
        }

        private static void logthdinfo(string info, string file_dir = "rmabackuptrace")
        {
            try
            {
                var filename = "d:\\log\\" + file_dir + "-" + DateTime.Now.ToString("yyyy-MM-dd");
                if (File.Exists(filename))
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

        #endregion

        #region RMA

        public static void RefreshRMAData(Controller ctrl)
        {
            var syscfgdict = CfgUtility.GetSysConfig(ctrl);

            var rmasrcfolder = syscfgdict["RMASHAREFOLDER"];
            var rmasrcfiles = DirectoryEnumerateFiles(ctrl, rmasrcfolder);

            var rmaclosefilefoldr = syscfgdict["RMACLOSEATTACHES"];

            string datestring = DateTime.Now.ToString("yyyyMMdd");
            string imgdir = ctrl.Server.MapPath("~/userfiles") + "\\docs\\" + datestring + "\\";
            if (!DirectoryExists(ctrl, imgdir))
            {
                Directory.CreateDirectory(imgdir);
            }

            foreach (var srcf in rmasrcfiles)
            {
                var filename = Path.GetFileName(srcf);
                if (filename.ToUpper().Contains("RMA") 
                    && filename.ToUpper().Contains("COMPLAINT")
                    && filename.ToUpper().Contains("NEWDATA"))
                {
                    try
                    {
                        logthdinfo("\r\nStart to copy file: " + srcf);
                        var desfile = imgdir + filename;
                        FileCopy(ctrl, srcf, desfile, true);
                        if (FileExist(ctrl, desfile))
                        {
                            logthdinfo("try to get data from file: " + desfile);
                            var data = RetrieveDataFromExcelWithAuth(ctrl, desfile);
                            logthdinfo("get data count: " + data.Count.ToString());

                            SolveRMAData(data, rmaclosefilefoldr, ctrl);
                        }//copied file exist
                    }
                    catch (Exception ex) {
                        logthdinfo("SolveRMAData Exception: " + ex.Message);
                    }
                }//end if
            }//end foreach
        }

        private static RMARAWData OfferRMARAWDataValue(List<string> line)
        {
            var tempdata = new RMARAWData();
            tempdata.AppV_A = line[0];
            tempdata.AppV_B = line[1];
            tempdata.AppV_C = line[2];
            tempdata.AppV_D = line[3];
            tempdata.AppV_E = line[4];
            tempdata.AppV_F = line[5];
            tempdata.AppV_G = line[6];
            tempdata.AppV_H = line[7];
            tempdata.AppV_I = line[8];
            tempdata.AppV_J = line[9];
            tempdata.AppV_K = line[10];
            tempdata.AppV_L = line[11];
            tempdata.AppV_M = line[12];
            tempdata.AppV_N = line[13];
            tempdata.AppV_O = line[14];
            tempdata.AppV_P = ConvertToDateStr(line[15]);
            tempdata.AppV_Q = ConvertToDateStr(line[16]);
            tempdata.AppV_R = ConvertToDateStr(line[17]);
            tempdata.AppV_S = ConvertToDateStr(line[18]);
            tempdata.AppV_T = ConvertToDateStr(line[19]);
            tempdata.AppV_U = line[20];
            tempdata.AppV_V = ConvertToDateStr(line[21]);
            tempdata.AppV_W = ConvertToDateStr(line[22]);
            tempdata.AppV_X = line[23];
            tempdata.AppV_Y = line[24];
            tempdata.AppV_Z = line[25];
            tempdata.AppV_AA = line[26];
            tempdata.AppV_AB = line[27];
            tempdata.AppV_AC = ConvertToDateStr(line[28]);
            tempdata.AppV_AD = line[29];
            tempdata.AppV_AE = line[30];
            tempdata.AppV_AF = line[31];
            tempdata.AppV_AG = line[32];
            return tempdata;
        }

        private static List<RMARAWData> SplitRMAData(List<string> line)
        {
            var ret = new List<RMARAWData>();
            if (!string.IsNullOrEmpty(line[1]) && !string.IsNullOrEmpty(line[8]))
            {
                var sns = line[8].Split(new char[] { '\r','\n',',',';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var sn in sns)
                {
                    var tempdata = OfferRMARAWDataValue(line);
                    tempdata.AppV_I = sn;
                    ret.Add(tempdata);
                }
            }
            return ret;
        }

        private static void SolveRMAData(List<List<string>> data,string rmacloseattachfolder, Controller ctrl)
        {
            if (data.Count == 0)
                return;
            if (!data[0][0].ToUpper().Contains("FIELD RETURN"))
                return;

            var allrmaissue = IssueViewModels.RetrieveAllIssueTypeIssue("NONE", "NONE", ISSUETP.RMA, ctrl); //rma issues
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

            var pjkeys = ProjectViewModels.RetrieveAllProjectKey(); //all pjkey
            var allpjdict = new Dictionary<string, bool>();
            foreach (var pjk in pjkeys)
            {
                allpjdict.Add(pjk, true);
            }

            //var usermatrix = UserMatrixVM.RetrieveUserMatrixDepart(); //user depart

            var rmaattaches = RetrieveRMACloseAttach(); //all rma attach
            var solvedrmanum = new Dictionary<string, bool>();

            var sixmonthago = DateTime.Now.AddMonths(-6);

            var idx = 0;
            foreach (var line in data)
            {
                if (idx != 0)
                {
                    try
                    {
                        var rmarawdatas = SplitRMAData(line); //split rma record with sn

                        if (rmarawdatas.Count > 0)
                        {
                            
                            if (sixmonthago > DateTime.Parse(rmarawdatas[0].AppV_P))
                            {
                                continue;
                            }
                        }

                        foreach (var rawdata in rmarawdatas)
                        {
                            UpdateRMAData(rawdata);
                            Try2CreateRMA(rawdata, rmaissuedict, allpjdict,ctrl);
                        }

                        if (rmarawdatas.Count > 0)
                        {
                            if (solvedrmanum.ContainsKey(rmarawdatas[0].AppV_B))
                            {
                                continue;
                            }
                            StoreRMAAttachs(rmacloseattachfolder, rmarawdatas[0].AppV_B, rmaattaches,ctrl); //retrieve rma attach and store them
                            solvedrmanum.Add(rmarawdatas[0].AppV_B,true);
                        }
                    }
                    catch (Exception ex) { }
                }//end if
                idx = idx + 1;
            }//foreach
        }

        private static void StoreRMAAttachs(string rmacloseattachfolder,string RMANum, Dictionary<string, Dictionary<string, bool>> rmaattaches,Controller ctrl)
        {
            
            var rmanumfolder = rmacloseattachfolder + "\\" + RMANum;
            if (DirectoryExists(ctrl, rmanumfolder))
            {
                var cleanrmanum = RMANum.Replace(" ", "_").Replace("#", "").Replace("'", "")
                            .Replace("&", "").Replace("?", "").Replace("%", "").Replace("+", "");

                var imgdir = ctrl.Server.MapPath("~/userfiles") + "\\docs\\RMAATTCH\\" + cleanrmanum + "\\";

                if (!DirectoryExists(ctrl, imgdir))
                    Directory.CreateDirectory(imgdir);


                var rmaattachfiles = DirectoryEnumerateFiles(ctrl, rmanumfolder);
                foreach (var attach in rmaattachfiles)
                {
                    var filename = Path.GetFileName(attach).Replace(" ", "_").Replace("#", "").Replace("'", "")
                            .Replace("&", "").Replace("?", "").Replace("%", "").Replace("+", "");

                    if (!rmaattaches.ContainsKey(RMANum))
                    {
                        //download file and store

                        var desfile = imgdir + filename;
                        FileCopy(ctrl, attach, desfile, true);
                        if (FileExist(ctrl, desfile))
                        {
                            var url = "/userfiles/docs/RMAATTCH/"+ cleanrmanum +"/"+ filename;
                            StoreRMAAttach(RMANum, url);
                        }
                    }//not contain rmanum
                    else
                    {
                        var attachdict = rmaattaches[RMANum];
                        if (!attachdict.ContainsKey(filename))
                        {
                            var desfile = imgdir + filename;
                            FileCopy(ctrl, attach, desfile, true);
                            if (FileExist(ctrl, desfile))
                            {
                                var url = "/userfiles/docs/RMAATTCH/" + cleanrmanum + "/" + filename;
                                StoreRMAAttach(RMANum, url);
                            }
                        }//not contain attach
                    }
                }//foreach
            }//if folder exist
        }

        private static string RMSpectialCh(string str)
        {
            int idx = 0;
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                {
                    sb.Append(c);

                    idx = idx + 1;
                    if(idx > 47) return sb.ToString();
                }
            }
            return sb.ToString().ToUpper();
        }

        public static string RetrieveProjectKeyFromSN(string SN)
        {
            var sql = "select top 1 ProjectKey from ProjectTestData where ModuleSerialNum = @ModuleSerialNum";
            var dict = new Dictionary<string, string>();
            dict.Add("@ModuleSerialNum", SN);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
            if (dbret.Count > 0)
            {
                return Convert2Str(dbret[0][0]);
            }

            var pnpndes = MESUtility.RetrievePnPndescFromSN((new string[] { SN }).ToList());
            if (pnpndes.Count == 0)
            {
                return string.Empty;
            }
            
            sql = "select top 1 ProjectKey from ProjectTestData where PN = @PN";
            dict = new Dictionary<string, string>();
            dict.Add("@PN", pnpndes.ElementAt(0).Value.Key);
            dbret = DBUtility.ExeLocalSqlWithRes(sql, null, dict);
            if (dbret.Count > 0)
            {
                return Convert2Str(dbret[0][0]);
            }

            var pndes = pnpndes.ElementAt(0).Value.Value;
            var pnpjkey = ProjectViewModels.PN2PJKey();
            var pns = pnpjkey.Keys.ToList();
            foreach (var pn in pns)
            {
                if (pndes.Contains(pn))
                {
                    return pnpjkey[pn];
                }
            }

            return string.Empty;
        }

        private static void Try2CreateRMA(RMARAWData rawdata
            , Dictionary<string, string> rmaissuedict, Dictionary<string, bool> allpjdict,Controller ctrl)
        {
                    var pjk = RMSpectialCh(rawdata.AppV_N);
                    if (!allpjdict.ContainsKey(pjk))
                    {
                        pjk = RetrieveProjectKeyFromSN(rawdata.AppV_I);
                    }
                    if (string.IsNullOrEmpty(pjk))
                    {
                        pjk = "CQE";
                    }

                    if (!string.IsNullOrEmpty(pjk))
                    {
                        var oneweekago = DateTime.Now.AddDays(-7);
                        var issueopendate = DateTime.Parse(rawdata.AppV_P);
                        if (issueopendate > oneweekago)//issue open in one week
                        {
                            if (rmaissuedict.ContainsKey(rawdata.AppV_B))
                            {
                                if (rmaissuedict[rawdata.AppV_B].Contains(rawdata.AppV_I))
                                {}
                                else
                                {
                                    rmaissuedict[rawdata.AppV_B] = rmaissuedict[rawdata.AppV_B] + ":" + rawdata.AppV_I.ToUpper();
                                    //create RMA issue
                                    CreateRMAIssue(rawdata,pjk,ctrl);
                                    //reopen project
                                    ProjectViewModels.UpdateProjectStatus(pjk, ProjectStatus.Open);
                                }
                            }
                            else
                            {
                                rmaissuedict.Add(rawdata.AppV_B, rawdata.AppV_I);
                                //create RMA issue
                                CreateRMAIssue(rawdata,pjk,ctrl);
                                //reopen project
                                ProjectViewModels.UpdateProjectStatus(pjk, ProjectStatus.Open);
                            }//check whether issue exist
                        }//issue open in one week
                    }// Product type is exist in system

        }

        //private static void CreateRMASubIssue(string presum, string sum, string pjkey, string parentkey, string analyser, string reporter, DateTime duedate,string moretag)
        //{
        //    var vm = new IssueViewModels();
        //    vm.ProjectKey = pjkey;
        //    vm.IssueKey = IssueViewModels.GetUniqKey();
        //    vm.ParentIssueKey = parentkey;
        //    vm.IssueType = ISSUETP.Task;
        //    vm.Summary = presum + sum;
        //    vm.Priority = ISSUEPR.Major;
        //    vm.DueDate = duedate;
        //    vm.ReportDate = DateTime.Now;
        //    vm.Assignee = analyser;
        //    vm.Reporter = reporter;
        //    vm.Resolution = Resolute.Pending;
        //    vm.ResolvedDate = DateTime.Parse("1982-05-06 01:01:01");
        //    vm.StoreSubIssue();

        //    IssueTypeVM.SaveIssueType(vm.IssueKey, ISSUESUBTYPE.Task.ToString(),moretag);
        //}

        private static void CreateRMAIssue(RMARAWData rawdata,string pjkey,Controller ctrl)
        {
            var vm = new IssueViewModels();
            vm.ProjectKey = pjkey;
            vm.IssueKey = IssueViewModels.GetUniqKey();
            vm.IssueType = ISSUETP.RMA;

            vm.FinisarRMA = rawdata.AppV_B;
            if (vm.FinisarRMA.Length > 50)
            {
                vm.FinisarRMA = vm.FinisarRMA.Substring(0, 48);
            }

            vm.FinisarModel = rawdata.AppV_G+" "+rawdata.AppV_H;
            if (vm.FinisarModel.Length > 50)
            {
                vm.FinisarModel = vm.FinisarModel.Substring(0, 48);
            }

            vm.ECustomer = rawdata.AppV_E;
            if (vm.ECustomer.Length > 50)
            {
                vm.ECustomer = vm.ECustomer.Substring(0, 48);
            }

            vm.CRMANUM = "N/A";

            vm.CReport = rawdata.AppV_L;
            if (vm.CReport.Length > 180)
            {
                vm.CReport = vm.CReport.Substring(0, 178);
            }

            vm.RelativePeoples = "";
            vm.ModuleSN = rawdata.AppV_I;
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
                vm.DueDate = DateTime.Parse(rawdata.AppV_P).AddDays(12);
            }
            catch (Exception e) { vm.DueDate = DateTime.Now.AddDays(12); }


            vm.ReportDate = DateTime.Now;

            if (string.IsNullOrEmpty(rawdata.AppV_O)) {
                vm.Assignee = "STEVEN.QIU@FINISAR.COM";
            }
            else{
                vm.Assignee = (rawdata.AppV_O.Replace(" ", ".") + "@FINISAR.COM").ToUpper();
                if (vm.Assignee.Length > 200)
                {
                    vm.Assignee = vm.Assignee.Substring(0, 198);
                }
            }
            if (string.IsNullOrEmpty(rawdata.AppV_AG)){
                vm.Reporter = "STEVEN.QIU@FINISAR.COM";
            }
            else {
                vm.Reporter = (rawdata.AppV_AG.Replace(" ", ".") + "@FINISAR.COM").ToUpper(); ;
                if (vm.Reporter.Length > 200)
                {
                    vm.Reporter = vm.Reporter.Substring(0, 198);
                }
            }


            vm.Resolution = Resolute.Pending;

            vm.ResolvedDate = DateTime.Parse("1982-05-06 01:01:01");

            vm.RMAFailureCode = "";
            vm.FVCode = "";
            vm.Description = "RMA " + vm.FinisarRMA + " for module " + vm.ModuleSN + " " + vm.FinisarModel + " from " + vm.ECustomer + ":" + rawdata.AppV_L;
            vm.CommentType = COMMENTTYPE.Description;

            vm.StoreIssue();
            IssueTypeVM.SaveIssueType(vm.IssueKey, ISSUESUBTYPE.RMA.ToString());

            UserViewModels.RegisterUserAuto(vm.Assignee);


            //CreateRMASubIssue(RMASubIssueType.CONTAINMENTACTION, "Cotainment Action for RMA " + vm.FinisarRMA, vm.ProjectKey, vm.IssueKey, vm.Assignee, vm.Reporter, vm.DueDate.AddDays(18),ISSUESUBTYPE.CONTAINMENT.ToString());
            //CreateRMASubIssue(RMASubIssueType.CORRECTIVEACTION, "Corrective Action for RMA " + vm.FinisarRMA, vm.ProjectKey, vm.IssueKey, vm.Assignee, vm.Reporter, vm.DueDate.AddDays(48),ISSUESUBTYPE.CORRECTIVE.ToString());
            
            var comment = new IssueComments();
            comment.Comment = IssueCommentEmpty.TOBEEDIT;
            IssueViewModels.StoreIssueComment(vm.IssueKey, comment.dbComment, vm.Assignee, COMMENTTYPE.ContainmentAction);

            comment = new IssueComments();
            comment.Comment = IssueCommentEmpty.TOBEEDIT;
            IssueViewModels.StoreIssueComment(vm.IssueKey, comment.dbComment, vm.Assignee, COMMENTTYPE.RootCause);

            comment = new IssueComments();
            comment.Comment = IssueCommentEmpty.TOBEEDIT;
            IssueViewModels.StoreIssueComment(vm.IssueKey, comment.dbComment, vm.Assignee, COMMENTTYPE.CorrectiveAction);

            SendRMAEvent(vm, "created",ctrl, true);
        }

        private static bool RMADataExist(RMARAWData rmadata)
        {
            var sql = "select AppV_B from RMABackupData where AppV_B=N'<RMANUM>' and AppV_I=N'<SN>'";
            sql = sql.Replace("<RMANUM>", rmadata.AppV_B).Replace("<SN>", rmadata.AppV_I);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            if (dbret.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static void UpdateRMAData(RMARAWData rmadata)
        {
            var sql = "";
            if (RMADataExist(rmadata))
            {
                sql = "update RMABackupData set AppV_A = N'<AppV_A>',AppV_C = N'<AppV_C>',AppV_D = N'<AppV_D>'"
                    + ",AppV_E = N'<AppV_E>',AppV_F = N'<AppV_F>',AppV_G = N'<AppV_G>',AppV_H = N'<AppV_H>'"
                    + ",AppV_J = N'<AppV_J>',AppV_K = N'<AppV_K>',AppV_L = N'<AppV_L>',AppV_M = N'<AppV_M>',AppV_N = N'<AppV_N>'"
                    + ",AppV_O = N'<AppV_O>',AppV_P = '<AppV_P>',AppV_Q = '<AppV_Q>',AppV_R = '<AppV_R>',AppV_S = '<AppV_S>'"
                    + ",AppV_T = '<AppV_T>',AppV_U = N'<AppV_U>',AppV_V = '<AppV_V>',AppV_W = '<AppV_W>',AppV_X = N'<AppV_X>'"
                    + ",AppV_Y = N'<AppV_Y>',AppV_Z = N'<AppV_Z>',AppV_AA = N'<AppV_AA>',AppV_AB = N'<AppV_AB>',AppV_AC = '<AppV_AC>'"
                    + ",AppV_AD = N'<AppV_AD>',AppV_AE = N'<AppV_AE>',AppV_AF = N'<AppV_AF>',AppV_AG = N'<AppV_AG>'  where AppV_B=N'<AppV_B>' and AppV_I=N'<AppV_I>'";
            }
            else
            {
                sql = "insert into RMABackupData(AppV_A,AppV_B,AppV_C,AppV_D,AppV_E,AppV_F"
                    + ",AppV_G,AppV_H,AppV_I,AppV_J,AppV_K,AppV_L,AppV_M,AppV_N,AppV_O"
                    + ",AppV_P,AppV_Q,AppV_R,AppV_S,AppV_T,AppV_U,AppV_V,AppV_W,AppV_X"
                    + ",AppV_Y,AppV_Z,AppV_AA,AppV_AB,AppV_AC,AppV_AD,AppV_AE,AppV_AF,AppV_AG,databackuptm)"
                    + " values(N'<AppV_A>',N'<AppV_B>',N'<AppV_C>',N'<AppV_D>',N'<AppV_E>',N'<AppV_F>'"
                    + ",N'<AppV_G>',N'<AppV_H>',N'<AppV_I>',N'<AppV_J>',N'<AppV_K>',N'<AppV_L>',N'<AppV_M>',N'<AppV_N>',N'<AppV_O>'"
                    + ",'<AppV_P>','<AppV_Q>','<AppV_R>','<AppV_S>','<AppV_T>',N'<AppV_U>','<AppV_V>','<AppV_W>',N'<AppV_X>'"
                    + ",N'<AppV_Y>',N'<AppV_Z>',N'<AppV_AA>',N'<AppV_AB>','<AppV_AC>',N'<AppV_AD>',N'<AppV_AE>',N'<AppV_AF>',N'<AppV_AG>','<databackuptm>')";
            }

            sql = sql.Replace("<AppV_A>", rmadata.AppV_A).Replace("<AppV_B>", rmadata.AppV_B).Replace("<AppV_C>", rmadata.AppV_C)
                .Replace("<AppV_D>", rmadata.AppV_D).Replace("<AppV_E>", rmadata.AppV_E).Replace("<AppV_F>", rmadata.AppV_F)
                .Replace("<AppV_G>", rmadata.AppV_G).Replace("<AppV_H>", rmadata.AppV_H).Replace("<AppV_I>", rmadata.AppV_I)
                .Replace("<AppV_J>", rmadata.AppV_J).Replace("<AppV_K>", rmadata.AppV_K).Replace("<AppV_L>", rmadata.AppV_L)
                .Replace("<AppV_M>", rmadata.AppV_M).Replace("<AppV_N>", rmadata.AppV_N).Replace("<AppV_O>", rmadata.AppV_O)
                .Replace("<AppV_P>", rmadata.AppV_P).Replace("<AppV_Q>", rmadata.AppV_Q).Replace("<AppV_R>", rmadata.AppV_R)
                .Replace("<AppV_S>", rmadata.AppV_S).Replace("<AppV_T>", rmadata.AppV_T).Replace("<AppV_U>", rmadata.AppV_U)
                .Replace("<AppV_V>", rmadata.AppV_V).Replace("<AppV_W>", rmadata.AppV_W).Replace("<AppV_X>", rmadata.AppV_X)
                .Replace("<AppV_Y>", rmadata.AppV_Y).Replace("<AppV_Z>", rmadata.AppV_Z).Replace("<AppV_AA>", rmadata.AppV_AA)
                .Replace("<AppV_AB>", rmadata.AppV_AB).Replace("<AppV_AC>", rmadata.AppV_AC).Replace("<AppV_AD>", rmadata.AppV_AD)
                .Replace("<AppV_AE>", rmadata.AppV_AE).Replace("<AppV_AF>", rmadata.AppV_AF).Replace("<AppV_AG>", rmadata.AppV_AG)
                .Replace("<databackuptm>",DateTime.Now.ToString());

            DBUtility.ExeLocalSqlNoRes(sql);
        }

        private static string ConvertToDateStr2(object val)
        {
            var datestr = Convert.ToString(val);
            if (string.IsNullOrEmpty(datestr))
                return string.Empty;
            try
            {
                var date = DateTime.Parse(datestr);
                if (string.Compare(date.ToString("yyyy-MM-dd"), "1982-05-06") == 0)
                {
                    return string.Empty;
                }
                return date.ToString("yyyy-MM-dd");
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public static List<RMARAWData> RetrieveAllRMAData()
        {
            var ret = new List<RMARAWData>();

            var sql = "select AppV_A,AppV_B,AppV_C,AppV_D,AppV_E,AppV_F"
                    + ",AppV_G,AppV_H,AppV_I,AppV_J,AppV_K,AppV_L,AppV_M,AppV_N,AppV_O"
                    + ",AppV_P,AppV_Q,AppV_R,AppV_S,AppV_T,AppV_U,AppV_V,AppV_W,AppV_X"
                    + ",AppV_Y,AppV_Z,AppV_AA,AppV_AB,AppV_AC,AppV_AD,AppV_AE,AppV_AF,AppV_AG from RMABackupData";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                try
                {
                    var tempdata = new RMARAWData();
                    tempdata.AppV_A = Convert.ToString(line[0]);
                    tempdata.AppV_B = Convert.ToString(line[1]);
                    tempdata.AppV_C = Convert.ToString(line[2]);
                    tempdata.AppV_D = Convert.ToString(line[3]);
                    tempdata.AppV_E = Convert.ToString(line[4]);
                    tempdata.AppV_F = Convert.ToString(line[5]);
                    tempdata.AppV_G = Convert.ToString(line[6]);
                    tempdata.AppV_H = Convert.ToString(line[7]);
                    tempdata.AppV_I = Convert.ToString(line[8]);
                    tempdata.AppV_J = Convert.ToString(line[9]);
                    tempdata.AppV_K = Convert.ToString(line[10]);
                    tempdata.AppV_L = Convert.ToString(line[11]);
                    tempdata.AppV_M = Convert.ToString(line[12]);
                    tempdata.AppV_N = Convert.ToString(line[13]);
                    tempdata.AppV_O = Convert.ToString(line[14]);
                    tempdata.AppV_P = ConvertToDateStr2(line[15]);
                    tempdata.AppV_Q = ConvertToDateStr2(line[16]);
                    tempdata.AppV_R = ConvertToDateStr2(line[17]);
                    tempdata.AppV_S = ConvertToDateStr2(line[18]);
                    tempdata.AppV_T = ConvertToDateStr2(line[19]);
                    tempdata.AppV_U = Convert.ToString(line[20]);
                    tempdata.AppV_V = ConvertToDateStr2(line[21]);
                    tempdata.AppV_W = ConvertToDateStr2(line[22]);
                    tempdata.AppV_X = Convert.ToString(line[23]);
                    tempdata.AppV_Y = Convert.ToString(line[24]);
                    tempdata.AppV_Z = Convert.ToString(line[25]);
                    tempdata.AppV_AA = Convert.ToString(line[26]);
                    tempdata.AppV_AB = Convert.ToString(line[27]);
                    tempdata.AppV_AC = ConvertToDateStr2(line[28]);
                    tempdata.AppV_AD = Convert.ToString(line[29]);
                    tempdata.AppV_AE = Convert.ToString(line[30]);
                    tempdata.AppV_AF = Convert.ToString(line[31]);
                    tempdata.AppV_AG = Convert.ToString(line[32]);
                    ret.Add(tempdata);
                }
                catch (Exception ex) { }

            }

            return ret;
        }

        private static void SendRMAEvent(IssueViewModels vm, string operate,Controller ctrl, bool nocheck = false)
        {
            var alertime = vm.RetrieveAlertEmailDate(vm.IssueKey);
            if ((!string.IsNullOrEmpty(alertime) && DateTime.Parse(alertime).AddHours(24) < DateTime.Now) || nocheck)
            {
                vm.UpdateAlertEmailDate();

                var routevalue = new RouteValueDictionary();
                routevalue.Add("issuekey", vm.IssueKey);
                //send validate email
                string scheme = ctrl.Url.RequestContext.HttpContext.Request.Url.Scheme;
                string validatestr = ctrl.Url.Action("UpdateIssue", "Issue", routevalue, scheme);

                var netcomputername = EmailUtility.RetrieveCurrentMachineName();
                validatestr = validatestr.Replace("//localhost", "//" + netcomputername);

                var content = vm.Summary + " is " + operate + " by " + vm.Reporter + " :\r\n " + validatestr;

                var toaddrs = new List<string>();
                toaddrs.AddRange(vm.RelativePeopleList);
                toaddrs.Add(vm.Assignee);
                toaddrs.Add(vm.Reporter);

                var reporter = vm.Reporter.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries)[0].Replace(".", " ");
                EmailUtility.SendEmail(ctrl, "WUXI Engineering System_"+reporter, toaddrs, content);
                new System.Threading.ManualResetEvent(false).WaitOne(50);
            }
        }

        private static Dictionary<string, Dictionary<string, bool>> RetrieveRMACloseAttach()
        {
            var ret = new Dictionary<string, Dictionary<string, bool>>();
            var sql = "select AppV_A,AppV_B from RMAMapData where AppV_C = '<AppV_C>'";
            sql = sql.Replace("<AppV_C>", RMAMAPDATATYPE.ATTACH);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            foreach (var line in dbret)
            {
                var rmanum = Convert.ToString(line[0]);
                var attachs = Convert.ToString(line[1]).Split(new string[] { "\\","/"},StringSplitOptions.RemoveEmptyEntries);
                var attach = attachs[attachs.Length - 1];

                if (ret.ContainsKey(rmanum))
                {
                    if (!ret[rmanum].ContainsKey(attach))
                    {
                        ret[rmanum].Add(attach, true);
                    }
                }
                else
                {
                    var val = new Dictionary<string, bool>();
                    val.Add(attach, true);
                    ret.Add(rmanum, val);
                }
            }

            return ret;
        }

        private static void StoreRMAAttach(string RMANum, string attach)
        {
            var sql = "insert into RMAMapData(AppV_A,AppV_B,AppV_C,databackuptm) values(N'<AppV_A>',N'<AppV_B>',N'<AppV_C>','<databackuptm>')";
            sql = sql.Replace("<AppV_A>", RMANum).Replace("<AppV_B>", attach).Replace("<AppV_C>", RMAMAPDATATYPE.ATTACH).Replace("<databackuptm>",DateTime.Now.ToString());
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        #endregion

        #region IQC
        public static void RefreshIQEData(Controller ctrl)
        {
            var syscfgdict = CfgUtility.GetSysConfig(ctrl);

            var rmasrcfolder = syscfgdict["IQESHAREFOLDER"];
            var rmasrcfiles = DirectoryEnumerateFiles(ctrl, rmasrcfolder);

            string datestring = DateTime.Now.ToString("yyyyMMdd");
            string imgdir = ctrl.Server.MapPath("~/userfiles") + "\\docs\\" + datestring + "\\";
            if (!DirectoryExists(ctrl, imgdir))
            {
                Directory.CreateDirectory(imgdir);
            }

            foreach (var srcf in rmasrcfiles)
            {
                var filename = Path.GetFileName(srcf);

                    try
                    {
                        logthdinfo("\r\nStart to copy file: " + srcf);

                        var desfile = imgdir + filename;
                        FileCopy(ctrl, srcf, desfile, true);
                        if (FileExist(ctrl, desfile))
                        {
                            logthdinfo("try to get data from file: " + desfile);
                            var data = RetrieveDataFromExcelWithAuth(ctrl, desfile);
                            logthdinfo("get data count: " + data.Count.ToString());

                            SolveIQEData(data, rmasrcfolder, ctrl);
                        }//copied file exist
                    }
                    catch (Exception ex)
                    {
                        logthdinfo("SolveIQEData Exception: " + ex.Message);
                    }

            }//end foreach
        }

        private static void SolveIQEData(List<List<string>> data,string attfolder, Controller ctrl)
        {
            if (data.Count == 0)
                return;
            if (!data[0][0].ToUpper().Contains("ID"))
                return;
        
            var allrmaissue = IssueViewModels.RetrieveAllIssueTypeIssue("NONE", "NONE", ISSUETP.IQE, ctrl); //IQC issues
            var rmaissuedict = new Dictionary<string, bool>();
            foreach (var issue in allrmaissue)
            {
                var uniquekey = issue.ModuleSN;
                if (!rmaissuedict.ContainsKey(uniquekey))
                {
                    rmaissuedict.Add(uniquekey, true);
                }
            }//end foreach

            var allbacupdata = ExternalDataCollector.RetrieveAllIQEData();
            var allbackupdict = new Dictionary<string, string>();
            foreach (var bdata in allbacupdata)
            {
                var uniquekey =bdata.AppV_A;
                if (!allbackupdict.ContainsKey(uniquekey))
                {
                    allbackupdict.Add(uniquekey, bdata.AppV_A);
                }
            }


            var syscfgdict = CfgUtility.GetSysConfig(ctrl);
            var IQEPJKEY = RMSpectialCh(syscfgdict["IQEDEFAULTPJ"]);

            var rmaattaches = RetrieveAllIQEAttach(); //all IQE attach
            var solvediqedatanum = new Dictionary<string, bool>();

            var idx = 0;
            foreach (var line in data)
            {
                if (idx != 0)
                {
                    try
                    {
                        var rawdata = OfferIQERAWDataValue(line); //split rma record with sn
                        if (rawdata == null)
                            continue;

                        var uniquekey = rawdata.AppV_A;
                        if (solvediqedatanum.ContainsKey(uniquekey))
                        {
                            continue;
                        }

                        UpdateIQEData(rawdata, allbackupdict);

                        var attaches = StoreIQEAttachs(attfolder+ "\\attachment\\" + rawdata.AppV_A, uniquekey, rmaattaches, ctrl); //retrieve IQE attach and store them

                        Try2CreateIQE(IQEPJKEY, rawdata, rmaissuedict, ctrl,attaches);


                        solvediqedatanum.Add(uniquekey, true);

                    }
                    catch (Exception ex) { }
                }//end if
                idx = idx + 1;
            }//foreach
        }

        private static string StoreIQEAttachs(string attfolder, string combinekey, Dictionary<string, Dictionary<string, bool>> rmaattaches, Controller ctrl)
        {
            var ret = "";
            if (DirectoryExists(ctrl, attfolder))
            {
                var imgdir = ctrl.Server.MapPath("~/userfiles") + "\\docs\\IQEATTCH\\" + combinekey + "\\";

                if (!DirectoryExists(ctrl, imgdir))
                    Directory.CreateDirectory(imgdir);


                var rmaattachfiles = DirectoryEnumerateFiles(ctrl, attfolder);
                foreach (var attach in rmaattachfiles)
                {
                    var filename = Path.GetFileName(attach).Replace(" ", "_").Replace("#", "").Replace("'", "")
                            .Replace("&", "").Replace("?", "").Replace("%", "").Replace("+", "");

                    if (!rmaattaches.ContainsKey(combinekey))
                    {
                        //download file and store

                        var desfile = imgdir + filename;
                        FileCopy(ctrl, attach, desfile, true);
                        if (FileExist(ctrl, desfile))
                        {
                            var url = "/userfiles/docs/IQEATTCH/" + combinekey + "/" + filename;
                            StoreIQEAttach(combinekey, url);
                            ret = ret + url + ";";
                        }
                    }//not contain rmanum
                    else
                    {
                        var attachdict = rmaattaches[combinekey];
                        if (!attachdict.ContainsKey(filename))
                        {
                            var desfile = imgdir + filename;
                            FileCopy(ctrl, attach, desfile, true);
                            if (FileExist(ctrl, desfile))
                            {
                                var url = "/userfiles/docs/IQEATTCH/" + combinekey + "/" + filename;
                                StoreIQEAttach(combinekey, url);
                                ret = ret + url + ";";
                            }
                        }//not contain attach
                    }
                }//foreach
            }//if folder exist
            return ret;
        }

        private static void StoreIQEAttach(string combinekey, string attach)
        {
            var sql = "insert into IQEMapData(AppV_A,AppV_B,AppV_C,AppV_D,databackuptm) values(N'<AppV_A>',N'<AppV_B>',N'<AppV_C>',N'<AppV_D>','<databackuptm>')";
            sql = sql.Replace("<AppV_A>", combinekey).Replace("<AppV_B>", "").Replace("<AppV_C>", attach).Replace("<AppV_D>", RMAMAPDATATYPE.ATTACH).Replace("<databackuptm>", DateTime.Now.ToString());
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static List<string> RetrieveIQEAttach(string combinekey)
        {
            var ret = new List<string>();
            var sql = "select AppV_C from IQEMapData where AppV_A = '<AppV_A>'";
            sql = sql.Replace("<AppV_A>", combinekey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                ret.Add(Convert2Str(line[0]));
            }
            return ret;
        }

        private static void Try2CreateIQE(string IQEPJKEY, IQERAWData rawdata
                , Dictionary<string, bool> rmaissuedict, Controller ctrl,string attaches)
        {
            if (string.IsNullOrEmpty(rawdata.AppV_M) || string.IsNullOrEmpty(rawdata.AppV_N))
                return;

            if (string.Compare(rawdata.AppV_J.ToUpper(), Resolute.Working.ToUpper()) == 0)
            {
                var analyser = rawdata.AppV_M.ToUpper().Split(new string[] { "/" },StringSplitOptions.RemoveEmptyEntries)[0];
                if (!rawdata.AppV_M.Contains("@"))
                    analyser = (rawdata.AppV_M.Replace(" ", ".") + "@FINISAR.COM").ToUpper();

                var reporter = rawdata.AppV_N.ToUpper().Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries)[0];
                if (!rawdata.AppV_N.Contains("@"))
                    reporter = (rawdata.AppV_N.Replace(" ", ".") + "@FINISAR.COM").ToUpper();

                UserViewModels.RegisterUserAuto(analyser);
                UserViewModels.RegisterUserAuto(reporter);

                var uniquekey = rawdata.AppV_A;
                if (!rmaissuedict.ContainsKey(uniquekey))
                {
                    CreateIQEIssue(IQEPJKEY, analyser, reporter, rawdata, ctrl, attaches);
                    rmaissuedict.Add(rawdata.AppV_A, true);
                }//check if IQE issue exist
            }//check raw data status
        }

        private static void CreateIQEIssue(string IQEPJKEY, string analyser, string reporter, IQERAWData rawdata, Controller ctrl,string attaches)
        {
            var vm = new IssueViewModels();
            vm.ProjectKey = IQEPJKEY;
            vm.IssueKey = IssueViewModels.GetUniqKey();
            vm.IssueType = ISSUETP.IQE;

            vm.RelativePeoples = "";
            vm.Priority = ISSUEPR.Major;
            vm.Resolution = Resolute.Pending;
            vm.ReportDate = DateTime.Now;
            vm.ModuleSN = rawdata.AppV_A;

            vm.Assignee = analyser;
            vm.Reporter = reporter;

            vm.ResolvedDate = DateTime.Parse("1982-05-06 01:01:01");
            vm.DueDate = DateTime.Now.AddDays(14);

            var shortissue = ((rawdata.AppV_I.Length > 50) ? rawdata.AppV_I.Substring(0, 50) : rawdata.AppV_I);

            vm.Summary = rawdata.AppV_B + "," + rawdata.AppV_G + "(" + rawdata.AppV_F+","+rawdata.AppV_H + ")" + "," + shortissue;

            vm.Description = rawdata.AppV_B + "," + rawdata.AppV_G + "(" + rawdata.AppV_F + "," + rawdata.AppV_H + ")" + "," + rawdata.AppV_I;
            if (!string.IsNullOrEmpty(attaches))
            {
                var attachlist = attaches.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var attach in attachlist)
                {
                    var splitstrs = attach.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                    var filename = splitstrs[splitstrs.Length - 1];
                    vm.Description = vm.Description+"<p><a href='" + attach + "' target = '_blank'>" + filename + "</a></p>";
                }
            }
            
            vm.CommentType = COMMENTTYPE.Description;

            vm.StoreIssue();
            UpdateIQETaskKey(rawdata.AppV_A, vm.IssueKey);

            IssueTypeVM.SaveIssueType(vm.IssueKey, ISSUESUBTYPE.IQE.ToString());

            //CreateRMASubIssue(RMASubIssueType.CONTAINMENTACTION, "Cotainment Action for "+ shortissue
            //    , vm.ProjectKey, vm.IssueKey, vm.Assignee, vm.Reporter, vm.DueDate.AddDays(18), ISSUESUBTYPE.CONTAINMENT.ToString());
            //CreateRMASubIssue(RMASubIssueType.CORRECTIVEACTION, "Corrective Action for "+ shortissue
            //    , vm.ProjectKey, vm.IssueKey, vm.Assignee, vm.Reporter, vm.DueDate.AddDays(48), ISSUESUBTYPE.CORRECTIVE.ToString());

            var comment = new IssueComments();
            comment.Comment = IssueCommentEmpty.TOBEEDIT;
            IssueViewModels.StoreIssueComment(vm.IssueKey, comment.dbComment, vm.Assignee, COMMENTTYPE.ContainmentAction);
            comment = new IssueComments();
            comment.Comment = IssueCommentEmpty.TOBEEDIT;
            IssueViewModels.StoreIssueComment(vm.IssueKey, comment.dbComment, vm.Assignee, COMMENTTYPE.RootCause);
            comment = new IssueComments();
            comment.Comment = IssueCommentEmpty.TOBEEDIT;
            IssueViewModels.StoreIssueComment(vm.IssueKey, comment.dbComment, vm.Assignee, COMMENTTYPE.CorrectiveAction);

            if (!IsDebug())
            {
                SendRMAEvent(vm, "created", ctrl, true);
            }

        }

        private static void UpdateIQETaskKey(string combinekey,string issuekey)
        {
            var sql = "update IQEBackupData set AppV_P = N'<AppV_P>' where AppV_A=N'<AppV_A>'";
            sql = sql.Replace("<AppV_A>", combinekey).Replace("<AppV_P>",issuekey);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        private static void UpdateIQEData(IQERAWData rmadata, Dictionary<string, string> allbackupdict)
        {
            var sql = "";
            var relid = allbackupdict.ContainsKey(rmadata.AppV_A)? rmadata.AppV_A:"";
            if (!string.IsNullOrEmpty(relid))
            {
                sql = "update IQEBackupData set AppV_B = N'<AppV_B>',AppV_C = N'<AppV_C>',AppV_D = N'<AppV_D>'"
                    + ",AppV_E = N'<AppV_E>',AppV_F = N'<AppV_F>',AppV_G = N'<AppV_G>',AppV_H = N'<AppV_H>',AppV_I = N'<AppV_I>'"
                    + ",AppV_J = N'<AppV_J>',AppV_K = N'<AppV_K>',AppV_L = N'<AppV_L>',AppV_M = N'<AppV_M>',AppV_N = N'<AppV_N>'"
                    + ",AppV_O = '<AppV_O>', AppV_Q = N'<AppV_Q>'"
                    + "  where AppV_A=N'<AppV_A>'";
                sql = sql.Replace("<AppV_A>", relid);
            }
            else
            {
                sql = "insert into IQEBackupData(AppV_A,AppV_B,AppV_C,AppV_D,AppV_E,AppV_F"
                    + ",AppV_G,AppV_H,AppV_I,AppV_J,AppV_K,AppV_L,AppV_M,AppV_N,AppV_O,AppV_Q"
                    + ",databackuptm)"
                    + " values(N'<AppV_A>',N'<AppV_B>',N'<AppV_C>',N'<AppV_D>',N'<AppV_E>',N'<AppV_F>'"
                    + ",N'<AppV_G>',N'<AppV_H>',N'<AppV_I>',N'<AppV_J>',N'<AppV_K>',N'<AppV_L>',N'<AppV_M>',N'<AppV_N>','<AppV_O>',N'<AppV_Q>'"
                    + ",'<databackuptm>')";
                sql = sql.Replace("<AppV_A>", rmadata.AppV_A);

                var uniquekey = rmadata.AppV_A;
                if (!allbackupdict.ContainsKey(uniquekey))
                {
                    allbackupdict.Add(uniquekey, rmadata.AppV_A);
                }
            }

            sql = sql.Replace("<AppV_B>", rmadata.AppV_B).Replace("<AppV_C>", rmadata.AppV_C)
                .Replace("<AppV_D>", rmadata.AppV_D).Replace("<AppV_E>", rmadata.AppV_E).Replace("<AppV_F>", rmadata.AppV_F)
                .Replace("<AppV_G>", rmadata.AppV_G).Replace("<AppV_H>", rmadata.AppV_H).Replace("<AppV_I>", rmadata.AppV_I)
                .Replace("<AppV_J>", rmadata.AppV_J).Replace("<AppV_K>", rmadata.AppV_K).Replace("<AppV_L>", rmadata.AppV_L)
                .Replace("<AppV_M>", rmadata.AppV_M).Replace("<AppV_N>", rmadata.AppV_N).Replace("<AppV_O>",ConvertToDateStr(rmadata.AppV_O))
                .Replace("<AppV_Q>", rmadata.AppV_Q)
                .Replace("<databackuptm>", DateTime.Now.ToString());

            DBUtility.ExeLocalSqlNoRes(sql);
        }

        private static IQERAWData OfferIQERAWDataValue(List<string> line)
        {
            if (string.IsNullOrEmpty(line[0]))
                return null;

            var tempdata = new IQERAWData();
            tempdata.AppV_A = line[0];
            tempdata.AppV_B = line[1];
            tempdata.AppV_C = line[2];
            tempdata.AppV_D = line[3];
            tempdata.AppV_E = line[4];
            tempdata.AppV_F = line[5];
            tempdata.AppV_G = line[6];
            tempdata.AppV_H = line[7];
            tempdata.AppV_I = line[8];
            tempdata.AppV_J = line[9];
            tempdata.AppV_K = line[10];
            tempdata.AppV_L = line[11];
            tempdata.AppV_M = line[12];
            tempdata.AppV_N = line[13];
            tempdata.AppV_O = line[14];
            tempdata.AppV_Q = line[15];

            return tempdata;
        }

        private static Dictionary<string, Dictionary<string, bool>> RetrieveAllIQEAttach()
        {
            var ret = new Dictionary<string, Dictionary<string, bool>>();
            var sql = "select AppV_A,AppV_B,AppV_C from IQEMapData where AppV_D = '<AppV_D>'";
            sql = sql.Replace("<AppV_D>", RMAMAPDATATYPE.ATTACH);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var combinekey = Convert.ToString(line[0]);
                var attachs = Convert.ToString(line[2]).Split(new string[] { "\\", "/" }, StringSplitOptions.RemoveEmptyEntries);
                var attach = attachs[attachs.Length - 1];

                if (ret.ContainsKey(combinekey))
                {
                    if (!ret[combinekey].ContainsKey(attach))
                    {
                        ret[combinekey].Add(attach, true);
                    }
                }
                else
                {
                    var val = new Dictionary<string, bool>();
                    val.Add(attach, true);
                    ret.Add(combinekey, val);
                }
            }

            return ret;
        }

        public static List<IQERAWData> RetrieveAllIQEData()
        {
            var ret = new List<IQERAWData>();

            var sql = "select AppV_A,AppV_B,AppV_C,AppV_D,AppV_E,AppV_F"
                    + ",AppV_G,AppV_H,AppV_I,AppV_J,AppV_K,AppV_L,AppV_M,AppV_N,AppV_O,AppV_P, AppV_Q"
                    + " from IQEBackupData order by AppV_O DESC";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                try
                {
                    var tempdata = new IQERAWData();
                    tempdata.AppV_A = Convert.ToString(line[0]);
                    tempdata.AppV_B = Convert.ToString(line[1]);
                    tempdata.AppV_C = Convert.ToString(line[2]);
                    tempdata.AppV_D = Convert.ToString(line[3]);
                    tempdata.AppV_E = Convert.ToString(line[4]);
                    tempdata.AppV_F = Convert.ToString(line[5]);
                    tempdata.AppV_G = Convert.ToString(line[6]);
                    tempdata.AppV_H = Convert.ToString(line[7]);
                    tempdata.AppV_I = Convert.ToString(line[8]);
                    tempdata.AppV_J = Convert.ToString(line[9]);
                    tempdata.AppV_K = Convert.ToString(line[10]);
                    tempdata.AppV_L = Convert.ToString(line[11]);
                    tempdata.AppV_M = Convert.ToString(line[12]);
                    tempdata.AppV_N = Convert.ToString(line[13]);
                    tempdata.AppV_O = Convert.ToDateTime(line[14]).ToString("yyyy-MM-dd HH:mm:ss");
                    tempdata.AppV_P = Convert.ToString(line[15]);
                    tempdata.AppV_Q = Convert.ToString(line[16]);
                    tempdata.Attachs.AddRange(RetrieveIQEAttach(tempdata.AppV_A));
                    ret.Add(tempdata);
                }
                catch (Exception ex) { }

            }

            return ret;
        }

        #endregion

        #region NEOMAP
        public static Dictionary<string, string> NeoMapDBColName2RealName()
        {
            var dict = new Dictionary<string, string>();
            dict.Add("AppV_A", TXOQUERYCOND.NEOMAP + "wafer");
            dict.Add("AppV_B", TXOQUERYCOND.NEOMAP + "partfmly");
            dict.Add("AppV_C", TXOQUERYCOND.NEOMAP + "die#");
            dict.Add("AppV_D", TXOQUERYCOND.NEOMAP + "X");
            dict.Add("AppV_E", TXOQUERYCOND.NEOMAP + "Y");
            dict.Add("AppV_F", TXOQUERYCOND.NEOMAP + "radius");
            dict.Add("AppV_G", TXOQUERYCOND.NEOMAP + "bin#");
            dict.Add("AppV_H", TXOQUERYCOND.NEOMAP + "Vf2");
            dict.Add("AppV_I", TXOQUERYCOND.NEOMAP + "Vf6");
            dict.Add("AppV_J", TXOQUERYCOND.NEOMAP + "Pwr6");
            dict.Add("AppV_K", TXOQUERYCOND.NEOMAP + "SeriesR");
            dict.Add("AppV_L", TXOQUERYCOND.NEOMAP + "PkWave");
            dict.Add("AppV_M", TXOQUERYCOND.NEOMAP + "Ith");
            dict.Add("AppV_N", TXOQUERYCOND.NEOMAP + "SlopEff");
            dict.Add("AppV_O", TXOQUERYCOND.NEOMAP + "Kink2");
            dict.Add("AppV_P", TXOQUERYCOND.NEOMAP + "Delta15");
            dict.Add("AppV_Q", TXOQUERYCOND.NEOMAP + "BVr");
            dict.Add("AppV_R", TXOQUERYCOND.NEOMAP + "Pwr8");
            dict.Add("AppV_S", TXOQUERYCOND.NEOMAP + "RMSBDW");
            dict.Add("AppV_T", TXOQUERYCOND.NEOMAP + "MinPwr");
            dict.Add("AppV_U", TXOQUERYCOND.NEOMAP + "Imax");
            dict.Add("AppV_V", TXOQUERYCOND.NEOMAP + "Pmax");
            dict.Add("AppV_W", TXOQUERYCOND.NEOMAP + "_To");
            dict.Add("AppV_X", TXOQUERYCOND.NEOMAP + "_dn/dT_2");
            dict.Add("AppV_Y", TXOQUERYCOND.NEOMAP + "_Droop");
            dict.Add("AppV_Z", TXOQUERYCOND.NEOMAP + "_IthUfm");
            dict.Add("AppV_AA", TXOQUERYCOND.NEOMAP + "_PmaxUfm");
            dict.Add("AppV_AB", TXOQUERYCOND.NEOMAP + "_Vniformity");
            dict.Add("AppV_AC", TXOQUERYCOND.NEOMAP + "RadiusCalc_um");
            return dict;
        }

        public static Dictionary<string, string> NeoMapRealName2DBColName()
        {
            var dict = new Dictionary<string, string>();
            var tempdict = NeoMapDBColName2RealName();
            foreach (var kv in tempdict)
            {
                dict.Add(kv.Value, kv.Key);
            }
            return dict;
        }

        public static List<string> NeoMapMainFieldNameList()
        {
            var list = new List<string>();
            list.Add(TXOQUERYCOND.NEOMAP + "Pwr6");
            list.Add(TXOQUERYCOND.NEOMAP + "Pwr8");
            list.Add(TXOQUERYCOND.NEOMAP + "Ith");
            list.Add(TXOQUERYCOND.NEOMAP + "RMSBDW");
            list.Add(TXOQUERYCOND.NEOMAP + "SeriesR");
            list.Add(TXOQUERYCOND.NEOMAP + "SlopEff");
            list.Add(TXOQUERYCOND.NEOMAP + "Vf2");
            list.Add(TXOQUERYCOND.NEOMAP + "Vf6");
            return list;
        }

        private static string WaferWithBin(string waferwithoutbin)
        {
            var ret = string.Empty;
            var sql = "select top 1 ParamValueString from insite.dc_AOC_ManualInspection where ParamValueString like '%<wafer>%'";
            sql = sql.Replace("<wafer>", waferwithoutbin);
            var dbret = DBUtility.ExeMESSqlWithRes(sql);
            if (dbret.Count > 0)
            {
                ret = Convert.ToString(dbret[0][0]);
            }
            return ret;
        }

        public static void RefreshNeoMAPData(Controller ctrl)
        {
            var syscfgdict = CfgUtility.GetSysConfig(ctrl);
            var neodatafolder = syscfgdict["NeoMAPDataFolder"];
            var neodatafiles = DirectoryEnumerateFiles(ctrl, neodatafolder);

            string datestring = DateTime.Now.ToString("yyyyMMdd");
            string imgdir = ctrl.Server.MapPath("~/userfiles") + "\\docs\\NeoMapData\\" + datestring + "\\";
            if (!DirectoryExists(ctrl, imgdir))
            {
                Directory.CreateDirectory(imgdir);
            }

            var loadedfiles = NeoMAPVM.LoadedNeoMAPFile();

            foreach (var srcf in neodatafiles)
            {
                try
                {
                    var filename = Path.GetFileName(srcf).Replace(" ", "_").Replace("#", "").Replace("'", "")
                                .Replace("&", "").Replace("?", "").Replace("%", "").Replace("+", "");
                    if (!loadedfiles.ContainsKey(filename))
                    {
                        var des = imgdir + filename;
                        FileCopy(ctrl,srcf, des, true);

                        if (FileExist(ctrl, des))
                        {
                            var data = RetrieveDataFromExcelWithAuth(ctrl, des);
                            if (data.Count > 1)
                            {
                                var waferwithbin = WaferWithBin(data[1][0]);
                                if (!string.IsNullOrEmpty(waferwithbin))
                                {
                                    NeoMAPVM.UpdateNeoMAPFile(filename);
                                    SolveNeoMAPData(data, waferwithbin, ctrl);
                                }
                            }
                        }//end if
                    }//end if
                }
                catch (Exception ex) { }
            }//end foreach

        }

        private static void SolveNeoMAPData(List<List<string>> data,string waferwithbin, Controller ctrl)
        {
            if (data.Count == 0)
                return;
            if (!data[0][0].ToUpper().Contains("WAFER"))
                return;

            var wafers = waferwithbin.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
            var bin = wafers[wafers.Length - 1].Trim();

            var bincount = 0;
            var nonbincount = 0;
            try
            {
                var datatable = new System.Data.DataTable();
                datatable.Columns.Add("AppV_A", typeof(string));
                datatable.Columns.Add("AppV_B", typeof(string));
                datatable.Columns.Add("AppV_C", typeof(string));
                datatable.Columns.Add("AppV_D", typeof(string));
                datatable.Columns.Add("AppV_E", typeof(string));
                datatable.Columns.Add("AppV_F", typeof(double));
                datatable.Columns.Add("AppV_G", typeof(string));
                datatable.Columns.Add("AppV_H", typeof(double));
                datatable.Columns.Add("AppV_I", typeof(double));
                datatable.Columns.Add("AppV_J", typeof(double));
                datatable.Columns.Add("AppV_K", typeof(double));
                datatable.Columns.Add("AppV_L", typeof(double));
                datatable.Columns.Add("AppV_M", typeof(double));
                datatable.Columns.Add("AppV_N", typeof(double));
                datatable.Columns.Add("AppV_O", typeof(double));
                datatable.Columns.Add("AppV_P", typeof(double));
                datatable.Columns.Add("AppV_Q", typeof(double));
                datatable.Columns.Add("AppV_R", typeof(double));
                datatable.Columns.Add("AppV_S", typeof(double));
                datatable.Columns.Add("AppV_T", typeof(double));
                datatable.Columns.Add("AppV_U", typeof(double));
                datatable.Columns.Add("AppV_V", typeof(double));
                datatable.Columns.Add("AppV_W", typeof(double));
                datatable.Columns.Add("AppV_X", typeof(double));
                datatable.Columns.Add("AppV_Y", typeof(double));
                datatable.Columns.Add("AppV_Z", typeof(double));
                datatable.Columns.Add("AppV_AA", typeof(double));
                datatable.Columns.Add("AppV_AB", typeof(double));
                datatable.Columns.Add("AppV_AC", typeof(double));
                datatable.Columns.Add("AppV_AD", typeof(double));
                datatable.Columns.Add("AppV_AE", typeof(double));
                datatable.Columns.Add("AppV_AF", typeof(double));
                datatable.Columns.Add("AppV_AG", typeof(double));
                datatable.Columns.Add("AppV_AH", typeof(DateTime));
                datatable.Columns.Add("AppV_AI", typeof(string));
                datatable.Columns.Add("AppV_AJ", typeof(string));
                datatable.Columns.Add("databackuptm", typeof(DateTime));

                var idx = 0;
                foreach (var line in data)
                {
                    if (idx != 0)
                    {
                        var neodata = OfferNEOData(line);
                        if (string.Compare(neodata.AppV_G, bin) == 0)
                        {
                            neodata.AppV_AD = 1;
                            bincount++;
                        }
                        else
                        {
                            nonbincount++;
                            idx = idx + 1;
                            continue;
                            //neodata.AppV_AD = 0;
                        }

                        PropertyInfo[] properties = typeof(NEOMAPData).GetProperties();
                        var temprow = new object[properties.Length];
                        var i = 0;
                        for (i= 0;i < properties.Length;)
                        {
                            temprow[i] = properties[i].GetValue(neodata);
                            i = i + 1;
                        }
                        datatable.Rows.Add(temprow);
                    }//end if
                    idx = idx + 1;
                }//end foreach
                WriteNeoMapDBWithTable(datatable);

                if ((bincount + nonbincount) > 0)
                {
                    double binyield = bincount /(double)(bincount + nonbincount);
                    var wrecord = new WaferRecord();
                    wrecord.Wafer = waferwithbin;
                    wrecord.WaferBinYield = binyield.ToString();
                    wrecord.WaferBinCount = bincount.ToString();
                    wrecord.SumCount = (bincount + nonbincount).ToString();
                    wrecord.StoreWaferRecord();
                }
            }
            catch (Exception ex)
            {
            }
        }

        private static void WriteNeoMapDBWithTable(System.Data.DataTable dt)
        {
            if (dt.Rows.Count > 0)
            {
                var targetcon = DBUtility.GetLocalConnector();
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(targetcon))
                {
                    bulkCopy.DestinationTableName = "NeoMapData";
                    bulkCopy.BulkCopyTimeout = 120;

                    try
                    {
                        for (int i = 0; i < dt.Columns.Count; i++)
                        {
                            bulkCopy.ColumnMappings.Add(dt.Columns[i].ColumnName, dt.Columns[i].ColumnName);
                        }
                        bulkCopy.WriteToServer(dt);
                        dt.Clear();
                    }
                    catch (Exception ex) { }
                }//end using
                DBUtility.CloseConnector(targetcon);
            }//end if
        }
        

        private static NEOMAPData OfferNEOData(List<string> line)
        {
            var tempdata = new NEOMAPData();
            tempdata.AppV_A = line[0];
            tempdata.AppV_B = line[1];
            tempdata.AppV_C = line[2];
            tempdata.AppV_D = line[3];
            tempdata.AppV_E = line[4];
            tempdata.AppV_F = ConvertToDoubleVal(line[5]);
            tempdata.AppV_G = line[6];
            tempdata.AppV_H = ConvertToDoubleVal(line[7]);
            tempdata.AppV_I = ConvertToDoubleVal(line[8]);
            tempdata.AppV_J = ConvertToDoubleVal(line[9]);
            tempdata.AppV_K = ConvertToDoubleVal(line[10]);
            tempdata.AppV_L = ConvertToDoubleVal(line[11]);
            tempdata.AppV_M = ConvertToDoubleVal(line[12]);
            tempdata.AppV_N = ConvertToDoubleVal(line[13]);
            tempdata.AppV_O = ConvertToDoubleVal(line[14]);
            tempdata.AppV_P = ConvertToDoubleVal(line[15]);
            tempdata.AppV_Q = ConvertToDoubleVal(line[16]);
            tempdata.AppV_R = ConvertToDoubleVal(line[17]);
            tempdata.AppV_S = ConvertToDoubleVal(line[18]);
            tempdata.AppV_T = ConvertToDoubleVal(line[19]);
            tempdata.AppV_U = ConvertToDoubleVal(line[20]);
            tempdata.AppV_V = ConvertToDoubleVal(line[21]);
            tempdata.AppV_W = ConvertToDoubleVal(line[22]);
            tempdata.AppV_X = ConvertToDoubleVal(line[23]);
            tempdata.AppV_Y = ConvertToDoubleVal(line[24]);
            tempdata.AppV_Z = ConvertToDoubleVal(line[25]);
            tempdata.AppV_AA = ConvertToDoubleVal(line[26]);
            tempdata.AppV_AB = ConvertToDoubleVal(line[27]);
            tempdata.AppV_AC = ConvertToDoubleVal(line[28]);
            tempdata.AppV_AD = ConvertToDoubleVal(line[29]);
            tempdata.AppV_AE = ConvertToDoubleVal(line[30]);
            tempdata.AppV_AF = ConvertToDoubleVal(line[31]);
            tempdata.AppV_AG = ConvertToDoubleVal(line[32]);
            tempdata.AppV_AH = DateTime.Parse("1982-05-06 07:30:00");
            tempdata.AppV_AI = line[34]; 
            tempdata.AppV_AJ = line[35];
            return tempdata;
        }

        public static Dictionary<string,bool> RetrieveNeoMapWaferList()
        {
            var ret = new Dictionary<string, bool>();
            var sql = "select distinct AppV_A from NeoMapData";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                if (!ret.ContainsKey(Convert.ToString(line[0])))
                {
                    ret.Add(Convert.ToString(line[0]),true);
                }
            }
            return ret;
        }


        public static List<NeoMapDataWithPos> RetrieveNeoMapData(string querycond, string datafield)
        {
            var real2db = NeoMapRealName2DBColName();
            var ret = new List<NeoMapDataWithPos>();
            var sql = "select  AppV_D,AppV_E,<datafield> from NeoMapData where AppV_AD = 1 and  AppV_A = '<AppV_A>' and <datafield> <> -99999";
            sql = sql.Replace("<AppV_A>", querycond).Replace("<datafield>", real2db[datafield]);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                try
                {
                    var tempdata = new NeoMapDataWithPos();
                    tempdata.x = Convert.ToInt32(line[0]);
                    tempdata.y = Convert.ToInt32(line[1]);
                    tempdata.value = Convert.ToDouble(line[2]);
                    if (real2db[datafield].ToUpper().Contains("PWR"))
                    {
                        tempdata.value = 10*Math.Log10(tempdata.value * 1000);
                    }
                    ret.Add(tempdata);
                }
                catch (Exception ex) { }
            }
            return ret;
        }

        #endregion

        #region REL

        private static RELRAWData OfferRELRAWDataValue(List<string> line,int startidx)
        {
            if (string.IsNullOrEmpty(line[2])
                || string.IsNullOrEmpty(line[8])
                || string.IsNullOrEmpty(line[10])
                || string.IsNullOrEmpty(line[1])
                || string.IsNullOrEmpty(line[13]))
                return null;
            
            var tempdata = new RELRAWData();
            if (string.IsNullOrEmpty(line[0]))
            {
                tempdata.AppV_A = startidx;
            }
            else
            {
                try
                {
                    tempdata.AppV_A = Convert.ToInt32(line[0]);
                }
                catch(Exception ex) { tempdata.AppV_A = startidx; }
                
            }
            

            tempdata.AppV_B = line[1];
            tempdata.AppV_C = ConvertToDateStr(line[2]);
            tempdata.AppV_D = ConvertToDateStr(line[3]);
            tempdata.AppV_E = line[4];
            tempdata.AppV_F = line[5];
            tempdata.AppV_G = line[6];
            tempdata.AppV_H = line[7];
            tempdata.AppV_I = line[8];
            tempdata.AppV_J = line[9];
            tempdata.AppV_K = line[10];
            tempdata.AppV_L = line[11];
            tempdata.AppV_M = line[12];
            tempdata.AppV_N = line[13];
            tempdata.AppV_O = line[14];
            tempdata.AppV_P = line[15];
            tempdata.AppV_Q = line[16];
            tempdata.AppV_R = line[17];
            tempdata.AppV_S = line[18];
            tempdata.AppV_T = line[19];
            tempdata.AppV_U = line[20];
            tempdata.AppV_V = ConvertToDateStr(line[21]);
            tempdata.AppV_W = line[22];
            tempdata.AppV_X = line[23];
            tempdata.AppV_Y = line[24];
            tempdata.AppV_Z = line[25];
            tempdata.AppV_AA = line[26];
            tempdata.AppV_AB = line[27];
            tempdata.AppV_AC = line[28];
            tempdata.AppV_AD = line[29];
            tempdata.AppV_AE = line[30];
            tempdata.AppV_AF = line[31];
            tempdata.AppV_AG = line[32];
            return tempdata;
        }

        public static void RefreshRELData(Controller ctrl)
        {
            var syscfgdict = CfgUtility.GetSysConfig(ctrl);

            var rmasrcfolder = syscfgdict["RELSHAREFOLDER"];
            var rmasrcfiles = DirectoryEnumerateFiles(ctrl, rmasrcfolder);

            string datestring = DateTime.Now.ToString("yyyyMMdd");
            string imgdir = ctrl.Server.MapPath("~/userfiles") + "\\docs\\" + datestring + "\\";
            if (!DirectoryExists(ctrl, imgdir))
            {
                Directory.CreateDirectory(imgdir);
            }

            foreach (var srcf in rmasrcfiles)
            {
                var filename = Path.GetFileName(srcf);
                if (filename.ToUpper().Contains("REL")
                    && filename.ToUpper().Contains("SUMMARY")
                    && filename.ToUpper().Contains("NEWDATA"))
                {
                    try
                    {
                        logthdinfo("\r\nStart to copy file: " + srcf);

                        var desfile = imgdir + filename;
                        FileCopy(ctrl, srcf, desfile, true);
                        if (FileExist(ctrl, desfile))
                        {
                            logthdinfo("try to get data from file: " + desfile);
                            var data = RetrieveDataFromExcelWithAuth(ctrl, desfile);
                            logthdinfo("get data count: " + data.Count.ToString());

                            SolveRELData(data,ctrl);
                        }//copied file exist
                    }
                    catch (Exception ex)
                    {
                        logthdinfo("SolveRELData Exception: " + ex.Message);
                    }
                }//end if
            }//end foreach
        }

        private static string ReconstructSN(string originalSN)
        {
            var ret = string.Empty;
            var sns = originalSN.Split(new string[] { " ",",",";",":","\r","\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var sn in sns)
            {
                ret = ret + ";" + sn;
            }
            return ret;
        }

        private static void SolveRELData(List<List<string>> data, Controller ctrl)
        {
            if (data.Count == 0)
                return;
            if (!data[0][0].ToUpper().Contains("REL FAILURE CASE ID"))
                return;

            var allrmaissue = IssueViewModels.RetrieveAllIssueTypeIssue("NONE", "NONE", ISSUETP.Rel, ctrl); //REL issues
            var rmaissuedict = new Dictionary<string, bool>();
            foreach (var issue in allrmaissue)
            {
                var uniquekey = issue.ReportDate.ToString("yyyy-MM-dd") + "-" + issue.TestType + "-" + ReconstructSN(issue.ModuleSN);
                if (!rmaissuedict.ContainsKey(uniquekey))
                {
                    rmaissuedict.Add(uniquekey, true);
                }
            }//end foreach

            var allbacupdata = ExternalDataCollector.RetrieveAllRELData();
            var allbackupdict = new Dictionary<string, int>();
            foreach (var bdata in allbacupdata)
            {
                var uniquekey = DateTime.Parse(bdata.AppV_C).ToString("yyyy-MM-dd") + "-" + bdata.AppV_I + "-" + ReconstructSN(bdata.AppV_K);
                if (!allbackupdict.ContainsKey(uniquekey))
                {
                    allbackupdict.Add(uniquekey, bdata.AppV_A);
                }
            }

            //var pjkeys = ProjectViewModels.RetrieveAllProjectKey(); //all pjkey
            //var allpjdict = new Dictionary<string, bool>();
            //foreach (var pjk in pjkeys)
            //{
            //    allpjdict.Add(pjk, true);
            //}

            var syscfgdict = CfgUtility.GetSysConfig(ctrl);
            var RELPJKEY =  RMSpectialCh(syscfgdict["RELDEFAULTPJ"]);

            var rmaattaches = RetrieveRelAttach(); //all rel attach
            var solvedrmanum = new Dictionary<string, bool>();

            var caseid = RetrieveLatestCaseID();
            
            var idx = 0;
            foreach (var line in data)
            {
                if (idx != 0)
                {
                    try
                    {
                        var rawdata = OfferRELRAWDataValue(line, caseid); //split rma record with sn
                        if (rawdata == null)
                            continue;

                        if (rawdata.AppV_A == caseid)
                        {
                            caseid = caseid + 1;
                        }

                        UpdateRELData(rawdata, allbackupdict);

                        //if (allpjdict.ContainsKey(RELPJKEY))
                        //{
                            Try2CreateREL(RELPJKEY,rawdata, rmaissuedict, ctrl);
                        //}

                        var uniquekey = DateTime.Parse(rawdata.AppV_C).ToString("yyyy-MM-dd") + "-" + rawdata.AppV_I + "-" + ReconstructSN(rawdata.AppV_K);
                        if (solvedrmanum.ContainsKey(uniquekey))
                        {
                            continue;
                        }
                        StoreRELAttachs(rawdata.AppV_Y,uniquekey, rawdata.AppV_A.ToString(), rmaattaches, ctrl); //retrieve rma attach and store them
                        solvedrmanum.Add(uniquekey, true);

                    }
                    catch (Exception ex) { }
                }//end if
                idx = idx + 1;
            }//foreach
        }

        public static int RetrieveLatestCaseID()
        {
            var sql = "select top 1 AppV_A from RELBackupData order by AppV_A DESC";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            if (dbret.Count > 0)
            {
                try
                {
                    var val = Convert.ToInt32(dbret[0][0]);
                    if (val < 2000)
                    {
                        return 2000;
                    }
                    else
                    {
                        return val+1;
                    }
                }
                catch (Exception ex)
                {
                    return 8000;
                }
            }
            else
            {
                return 2000;
            }
        }

        private static string RELDataExist(RELRAWData rmadata, Dictionary<string, int> allbackupdict)
        {
            var uniquekey = DateTime.Parse(rmadata.AppV_C).ToString("yyyy-MM-dd") + "-" + rmadata.AppV_I + "-" + ReconstructSN(rmadata.AppV_K);
            var ret = string.Empty;
            if (allbackupdict.ContainsKey(uniquekey))
            {
                ret = allbackupdict[uniquekey].ToString();
            }
            return ret;
            //var sql = "select AppV_A from RELBackupData where AppV_C=N'<AppV_C>' and AppV_I=N'<AppV_I>' and AppV_K=N'<AppV_K>'";
            //sql = sql.Replace("<AppV_C>", rmadata.AppV_C).Replace("<AppV_I>", rmadata.AppV_I).Replace("<AppV_K>", rmadata.AppV_K);
            //var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            //if (dbret.Count > 0)
            //{
            //    return true;
            //}
            //else
            //{
            //    return false;
            //}
        }

        private static void UpdateRELData(RELRAWData rmadata,Dictionary<string,int> allbackupdict)
        {
            var sql = "";
            var relid = RELDataExist(rmadata, allbackupdict);
            if (!string.IsNullOrEmpty(relid))
            {
                sql = "update RELBackupData set AppV_B = N'<AppV_B>',AppV_D = N'<AppV_D>'"
                    + ",AppV_E = N'<AppV_E>',AppV_F = N'<AppV_F>',AppV_G = N'<AppV_G>',AppV_H = N'<AppV_H>'"
                    + ",AppV_J = N'<AppV_J>',AppV_L = N'<AppV_L>',AppV_M = N'<AppV_M>',AppV_N = N'<AppV_N>'"
                    + ",AppV_O = N'<AppV_O>',AppV_P = '<AppV_P>',AppV_Q = '<AppV_Q>',AppV_R = '<AppV_R>',AppV_S = '<AppV_S>'"
                    + ",AppV_T = '<AppV_T>',AppV_U = N'<AppV_U>',AppV_V = '<AppV_V>',AppV_W = '<AppV_W>',AppV_X = N'<AppV_X>'"
                    + ",AppV_Y = N'<AppV_Y>',AppV_Z = N'<AppV_Z>',AppV_AA = N'<AppV_AA>',AppV_AB = N'<AppV_AB>',AppV_AC = '<AppV_AC>'"
                    + ",AppV_AD = N'<AppV_AD>',AppV_AE = N'<AppV_AE>',AppV_AF = N'<AppV_AF>',AppV_AG = N'<AppV_AG>'  where AppV_A=<AppV_A>";
                sql = sql.Replace("<AppV_A>", relid);
            }
            else
            {
                sql = "insert into RELBackupData(AppV_A,AppV_B,AppV_C,AppV_D,AppV_E,AppV_F"
                    + ",AppV_G,AppV_H,AppV_I,AppV_J,AppV_K,AppV_L,AppV_M,AppV_N,AppV_O"
                    + ",AppV_P,AppV_Q,AppV_R,AppV_S,AppV_T,AppV_U,AppV_V,AppV_W,AppV_X"
                    + ",AppV_Y,AppV_Z,AppV_AA,AppV_AB,AppV_AC,AppV_AD,AppV_AE,AppV_AF,AppV_AG,databackuptm)"
                    + " values(<AppV_A>,N'<AppV_B>',N'<AppV_C>',N'<AppV_D>',N'<AppV_E>',N'<AppV_F>'"
                    + ",N'<AppV_G>',N'<AppV_H>',N'<AppV_I>',N'<AppV_J>',N'<AppV_K>',N'<AppV_L>',N'<AppV_M>',N'<AppV_N>',N'<AppV_O>'"
                    + ",'<AppV_P>','<AppV_Q>','<AppV_R>','<AppV_S>','<AppV_T>',N'<AppV_U>','<AppV_V>','<AppV_W>',N'<AppV_X>'"
                    + ",N'<AppV_Y>',N'<AppV_Z>',N'<AppV_AA>',N'<AppV_AB>','<AppV_AC>',N'<AppV_AD>',N'<AppV_AE>',N'<AppV_AF>',N'<AppV_AG>','<databackuptm>')";
                sql = sql.Replace("<AppV_A>", rmadata.AppV_A.ToString());

                var uniquekey = DateTime.Parse(rmadata.AppV_C).ToString("yyyy-MM-dd") + "-" + rmadata.AppV_I + "-" + ReconstructSN(rmadata.AppV_K);
                if (!allbackupdict.ContainsKey(uniquekey))
                {
                    allbackupdict.Add(uniquekey, rmadata.AppV_A);
                }
            }

            sql = sql.Replace("<AppV_B>", rmadata.AppV_B).Replace("<AppV_C>", rmadata.AppV_C)
                .Replace("<AppV_D>", rmadata.AppV_D).Replace("<AppV_E>", rmadata.AppV_E).Replace("<AppV_F>", rmadata.AppV_F)
                .Replace("<AppV_G>", rmadata.AppV_G).Replace("<AppV_H>", rmadata.AppV_H).Replace("<AppV_I>", rmadata.AppV_I)
                .Replace("<AppV_J>", rmadata.AppV_J).Replace("<AppV_K>", ReconstructSN(rmadata.AppV_K)).Replace("<AppV_L>", rmadata.AppV_L)
                .Replace("<AppV_M>", rmadata.AppV_M).Replace("<AppV_N>", rmadata.AppV_N).Replace("<AppV_O>", rmadata.AppV_O)
                .Replace("<AppV_P>", rmadata.AppV_P).Replace("<AppV_Q>", rmadata.AppV_Q).Replace("<AppV_R>", rmadata.AppV_R)
                .Replace("<AppV_S>", rmadata.AppV_S).Replace("<AppV_T>", rmadata.AppV_T).Replace("<AppV_U>", rmadata.AppV_U)
                .Replace("<AppV_V>", rmadata.AppV_V).Replace("<AppV_W>", rmadata.AppV_W).Replace("<AppV_X>", rmadata.AppV_X)
                .Replace("<AppV_Y>", rmadata.AppV_Y).Replace("<AppV_Z>", rmadata.AppV_Z).Replace("<AppV_AA>", rmadata.AppV_AA)
                .Replace("<AppV_AB>", rmadata.AppV_AB).Replace("<AppV_AC>", rmadata.AppV_AC).Replace("<AppV_AD>", rmadata.AppV_AD)
                .Replace("<AppV_AE>", rmadata.AppV_AE).Replace("<AppV_AF>", rmadata.AppV_AF).Replace("<AppV_AG>", rmadata.AppV_AG).Replace("<databackuptm>", DateTime.Now.ToString());

            DBUtility.ExeLocalSqlNoRes(sql);
        }

        private static void Try2CreateREL(string RELPJKEY,RELRAWData rawdata
            , Dictionary<string, bool> rmaissuedict, Controller ctrl)
        {
            if (string.IsNullOrEmpty(rawdata.AppV_N) || string.IsNullOrEmpty(rawdata.AppV_O))
                return;

            if (string.Compare(rawdata.AppV_B.ToUpper(), Resolute.Working.ToUpper()) == 0)
            {
                var analyser = rawdata.AppV_N.ToUpper();
                if (!rawdata.AppV_N.Contains("@"))
                    analyser = (rawdata.AppV_N.Replace(" ", ".") + "@FINISAR.COM").ToUpper();

                var reporter = rawdata.AppV_O.ToUpper();
                if (!rawdata.AppV_O.Contains("@"))
                    reporter = (rawdata.AppV_O.Replace(" ", ".") + "@FINISAR.COM").ToUpper();
                UserViewModels.RegisterUserAuto(analyser);
                UserViewModels.RegisterUserAuto(reporter);

                var uniquekey = DateTime.Parse(rawdata.AppV_C).ToString("yyyy-MM-dd") + "-" + rawdata.AppV_I + "-" + ReconstructSN(rawdata.AppV_K);
                if (!rmaissuedict.ContainsKey(uniquekey))
                {
                    CreateRELssue(RELPJKEY, analyser, reporter, rawdata, ctrl);
                }//check if rel issue exist
            }//check raw data status
        }

        public static bool IsDebug()
        {
            bool debugging = false;
#if DEBUG
            debugging = true;
#else
            debugging = false;
#endif
            return debugging;
        }

        private static void CreateRELssue(string RELPJKEY,string analyser,string reporter, RELRAWData rawdata,Controller ctrl)
        {
            var vm = new IssueViewModels();
            vm.ProjectKey = RELPJKEY;
            vm.IssueKey = IssueViewModels.GetUniqKey();
            vm.IssueType = ISSUETP.Rel;

            vm.RelativePeoples = "";
            vm.Priority = ISSUEPR.Major;
            vm.Resolution = Resolute.Pending;
            vm.FVCode = "";
            vm.ReportDate = DateTime.Parse(rawdata.AppV_C);

            vm.CaseID = rawdata.AppV_A.ToString();
            vm.ProductType = rawdata.AppV_H;
            vm.LineCategory = rawdata.AppV_G;
            vm.QualType = rawdata.AppV_E;
            vm.TestType = rawdata.AppV_I;
            vm.FailureInterval = rawdata.AppV_J;
            vm.TestFailure = rawdata.AppV_P;
            vm.Location = rawdata.AppV_U;
            vm.RequestID = rawdata.AppV_F;
            vm.ModuleSN = rawdata.AppV_K;
            vm.FailQTY = rawdata.AppV_L;
            vm.TotalQTY = rawdata.AppV_M;

            vm.Assignee = analyser;
            vm.Reporter = reporter;

            vm.ResolvedDate = DateTime.Parse("1982-05-06 01:01:01");
            vm.DueDate = DateTime.Parse(rawdata.AppV_C).AddDays(23);

            vm.Summary = "REL CaseID "+vm.CaseID+": "+vm.ModuleSN+" failed for "+ vm.TestType+" test with failure "+ vm.TestFailure;
            vm.Description = vm.Summary;
            vm.CommentType = COMMENTTYPE.Description;

            vm.StoreIssue();

            IssueTypeVM.SaveIssueType(vm.IssueKey, ISSUESUBTYPE.REL.ToString());

            //CreateRelSubIssue(RELSubIssueType.FAILVERIFYACTION, "Failure Verify for CaseID " + vm.CaseID, RELPJKEY, vm.IssueKey, analyser, reporter, DateTime.Parse(rawdata.AppV_C).AddDays(2),ISSUESUBTYPE.FAILVERIFY.ToString());
            //CreateRelSubIssue(RELSubIssueType.CONTAINMENTACTION, "Cotainment Action for CaseID " + vm.CaseID, RELPJKEY, vm.IssueKey, analyser, reporter, DateTime.Parse(rawdata.AppV_C).AddDays(30),ISSUESUBTYPE.CONTAINMENT.ToString());
            //CreateRelSubIssue(RELSubIssueType.CORRECTIVEACTION, "Corrective/PreVentive Action for CaseID " + vm.CaseID, RELPJKEY, vm.IssueKey, analyser, reporter, DateTime.Parse(rawdata.AppV_C).AddDays(60),ISSUESUBTYPE.CORRECTIVE.ToString());
            //CreateRelSubIssue(RELSubIssueType.VERIFYCORRECTIVEACTION, "Verify Corrective/PreVentive Action for CaseID " + vm.CaseID, RELPJKEY, vm.IssueKey, reporter, reporter, DateTime.Parse(rawdata.AppV_C).AddDays(75),ISSUESUBTYPE.CORRECTIVEVERIFY.ToString());

            var comment = new IssueComments();
            comment.Comment = IssueCommentEmpty.TOBEEDIT;
            IssueViewModels.StoreIssueComment(vm.IssueKey, comment.dbComment, analyser, COMMENTTYPE.ContainmentAction);
            comment = new IssueComments();
            comment.Comment = IssueCommentEmpty.TOBEEDIT;
            IssueViewModels.StoreIssueComment(vm.IssueKey, comment.dbComment, analyser, COMMENTTYPE.RootCause);
            comment = new IssueComments();
            comment.Comment = IssueCommentEmpty.TOBEEDIT;
            IssueViewModels.StoreIssueComment(vm.IssueKey, comment.dbComment, analyser, COMMENTTYPE.CorrectiveAction);

            if (!IsDebug())
            {
                SendRMAEvent(vm, "created", ctrl, true);
            }

        }

        private static void CreateRelSubIssue(string presum, string sum, string pjkey, string parentkey, string analyser, string reporter, DateTime duedate,string moretag)
        {
            var vm = new IssueViewModels();
            vm.ProjectKey = pjkey;
            vm.IssueKey = IssueViewModels.GetUniqKey();
            vm.ParentIssueKey = parentkey;
            vm.IssueType = ISSUETP.Task;
            vm.Summary = presum + sum;
            vm.Priority = ISSUEPR.Major;
            vm.DueDate = duedate;
            vm.ReportDate = DateTime.Now;
            vm.Assignee = analyser;
            vm.Reporter = reporter;
            vm.Resolution = Resolute.Pending;
            vm.ResolvedDate = DateTime.Parse("1982-05-06 01:01:01");
            vm.StoreSubIssue();

            IssueTypeVM.SaveIssueType(vm.IssueKey, ISSUESUBTYPE.Task.ToString(),moretag);
        }

        private static Dictionary<string, Dictionary<string, bool>> RetrieveRelAttach()
        {
            var ret = new Dictionary<string, Dictionary<string, bool>>();
            var sql = "select AppV_A,AppV_B,AppV_C from RELMapData where AppV_D = '<AppV_D>'";
            sql = sql.Replace("<AppV_D>", RMAMAPDATATYPE.ATTACH);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var combinekey = Convert.ToString(line[0]);
                var attachs = Convert.ToString(line[2]).Split(new string[] { "\\", "/" }, StringSplitOptions.RemoveEmptyEntries);
                var attach = attachs[attachs.Length - 1];

                if (ret.ContainsKey(combinekey))
                {
                    if (!ret[combinekey].ContainsKey(attach))
                    {
                        ret[combinekey].Add(attach, true);
                    }
                }
                else
                {
                    var val = new Dictionary<string, bool>();
                    val.Add(attach, true);
                    ret.Add(combinekey, val);
                }
            }

            return ret;
        }

        private static void StoreRELAttachs(string relfolder, string combinekey,string CaseID, Dictionary<string, Dictionary<string, bool>> rmaattaches, Controller ctrl)
        {

            if (DirectoryExists(ctrl, relfolder))
            {
                var imgdir = ctrl.Server.MapPath("~/userfiles") + "\\docs\\RELATTCH\\" + CaseID + "\\";

                if (!DirectoryExists(ctrl, imgdir))
                    Directory.CreateDirectory(imgdir);


                var rmaattachfiles = DirectoryEnumerateFiles(ctrl, relfolder);
                foreach (var attach in rmaattachfiles)
                {
                    var filename = Path.GetFileName(attach).Replace(" ", "_").Replace("#", "").Replace("'", "")
                            .Replace("&", "").Replace("?", "").Replace("%", "").Replace("+", "");

                    if (!rmaattaches.ContainsKey(combinekey))
                    {
                        //download file and store

                        var desfile = imgdir + filename;
                        FileCopy(ctrl, attach, desfile, true);
                        if (FileExist(ctrl, desfile))
                        {
                            var url = "/userfiles/docs/RELATTCH/" + CaseID + "/" + filename;
                            StoreRELAttach( combinekey, CaseID, url);
                        }
                    }//not contain rmanum
                    else
                    {
                        var attachdict = rmaattaches[combinekey];
                        if (!attachdict.ContainsKey(filename))
                        {
                            var desfile = imgdir + filename;
                            FileCopy(ctrl, attach, desfile, true);
                            if (FileExist(ctrl, desfile))
                            {
                                var url = "/userfiles/docs/RELATTCH/" + CaseID + "/" + filename;
                                StoreRELAttach( combinekey, CaseID, url);
                            }
                        }//not contain attach
                    }
                }//foreach
            }//if folder exist
        }

        private static void StoreRELAttach(string combinekey, string CaseID, string attach)
        {
            var sql = "insert into RELMapData(AppV_A,AppV_B,AppV_C,AppV_D,databackuptm) values(N'<AppV_A>',N'<AppV_B>',N'<AppV_C>',N'<AppV_D>','<databackuptm>')";
            sql = sql.Replace("<AppV_A>", combinekey).Replace("<AppV_B>", CaseID).Replace("<AppV_C>", attach).Replace("<AppV_D>", RMAMAPDATATYPE.ATTACH).Replace("<databackuptm>", DateTime.Now.ToString());
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static List<RELRAWData> RetrieveAllRELData()
        {
            var ret = new List<RELRAWData>();

            var sql = "select AppV_A,AppV_B,AppV_C,AppV_D,AppV_E,AppV_F"
                    + ",AppV_G,AppV_H,AppV_I,AppV_J,AppV_K,AppV_L,AppV_M,AppV_N,AppV_O"
                    + ",AppV_P,AppV_Q,AppV_R,AppV_S,AppV_T,AppV_U,AppV_V,AppV_W,AppV_X"
                    + ",AppV_Y,AppV_Z,AppV_AA,AppV_AB,AppV_AC,AppV_AD,AppV_AE,AppV_AF,AppV_AG from RELBackupData";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                try
                {
                    var tempdata = new RELRAWData();
                    tempdata.AppV_A = Convert.ToInt32(line[0]);
                    tempdata.AppV_B = Convert.ToString(line[1]);
                    tempdata.AppV_C = ConvertToDateStr2(line[2]);
                    tempdata.AppV_D = ConvertToDateStr2(line[3]);
                    tempdata.AppV_E = Convert.ToString(line[4]);
                    tempdata.AppV_F = Convert.ToString(line[5]);
                    tempdata.AppV_G = Convert.ToString(line[6]);
                    tempdata.AppV_H = Convert.ToString(line[7]);
                    tempdata.AppV_I = Convert.ToString(line[8]);
                    tempdata.AppV_J = Convert.ToString(line[9]);
                    tempdata.AppV_K = Convert.ToString(line[10]);
                    tempdata.AppV_L = Convert.ToString(line[11]);
                    tempdata.AppV_M = Convert.ToString(line[12]);
                    tempdata.AppV_N = Convert.ToString(line[13]);
                    tempdata.AppV_O = Convert.ToString(line[14]);
                    tempdata.AppV_P = Convert.ToString(line[15]);
                    tempdata.AppV_Q = Convert.ToString(line[16]);
                    tempdata.AppV_R = Convert.ToString(line[17]);
                    tempdata.AppV_S = Convert.ToString(line[18]);
                    tempdata.AppV_T = Convert.ToString(line[19]);
                    tempdata.AppV_U = Convert.ToString(line[20]);
                    tempdata.AppV_V = ConvertToDateStr2(line[21]);
                    tempdata.AppV_W = Convert.ToString(line[22]);
                    tempdata.AppV_X = Convert.ToString(line[23]);
                    tempdata.AppV_Y = Convert.ToString(line[24]);
                    tempdata.AppV_Z = Convert.ToString(line[25]);
                    tempdata.AppV_AA = Convert.ToString(line[26]);
                    tempdata.AppV_AB = Convert.ToString(line[27]);
                    tempdata.AppV_AC = Convert.ToString(line[28]);
                    tempdata.AppV_AD = Convert.ToString(line[29]);
                    tempdata.AppV_AE = Convert.ToString(line[30]);
                    tempdata.AppV_AF = Convert.ToString(line[31]);
                    tempdata.AppV_AG = Convert.ToString(line[32]);
                    ret.Add(tempdata);
                }
                catch (Exception ex) { }

            }

            return ret;
        }

        #endregion

        #region OBA

        private static string RetrieveOBAUpdateTime()
        {
            var OBAUpdateTime = DateTime.Parse("2017-05-23 01:00:00");

            var sql = "select top 1 ReportDate from Issue where IssueType = '"+ISSUETP.OBA+"' order by ReportDate DESC";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            if (dbret.Count > 0)
            {
                try
                {
                    OBAUpdateTime = DateTime.Parse(Convert.ToString(dbret[0][0]));
                }
                catch (Exception ex)
                {
                    OBAUpdateTime = DateTime.Parse("2018-05-23 01:00:00");
                }
            }

            return OBAUpdateTime.ToString();
        }

        private static Dictionary<string, IssueViewModels> RetrieveExistDMRDict(Controller ctrl,string sdate,string enddate)
        {
            var ret = new Dictionary<string, IssueViewModels>();
            var obalist = IssueViewModels.RetrieveAllIssueTypeIssue(sdate, enddate, ISSUETP.OBA, ctrl);
            foreach (var item in obalist)
            {
                if (!string.IsNullOrEmpty(item.FinisarDMR.Trim()) && !ret.ContainsKey(item.FinisarDMR.Trim()))
                {
                    ret.Add(item.FinisarDMR.Trim(), item);
                }
            }
            return ret;
        }

        private static string Convert2Str(object obj)
        {
            try
            {
                return Convert.ToString(obj);
            }
            catch (Exception ex) { return string.Empty; }
        }

        private static void UpdateOBARootCause(IssueViewModels obaissue,string rootcause)
        {
            if (obaissue.RootCauseCommentList.Count > 0)
            {
                var rootcomment = obaissue.RootCauseCommentList[0];

                var temprootcomment = new IssueComments();
                temprootcomment.Comment = rootcause;
                IssueViewModels.UpdateSPComment(obaissue.IssueKey, rootcomment.CommentType, rootcomment.CommentDate.ToString(), temprootcomment.dbComment);
                obaissue.RootCauseCommentList.Clear();
                obaissue.RootCauseCommentList.Add(temprootcomment);

            }
            else
            {
                var rootcomment = new IssueComments();
                rootcomment.Comment = rootcause;
                rootcomment.CommentType = COMMENTTYPE.RootCause;
                rootcomment.CommentDate = DateTime.Now;
                rootcomment.Reporter = obaissue.Assignee;
                rootcomment.IssueKey = obaissue.IssueKey;
                IssueViewModels.StoreIssueComment(rootcomment.IssueKey, rootcomment.dbComment, rootcomment.Reporter, rootcomment.CommentType);
                obaissue.RootCauseCommentList.Add(rootcomment);
            }
            
        }

        private static void UpdateOBAAttachement(IssueViewModels obaissue, string attachment, Controller ctrl)
        {
            var srcfilename = Path.GetFileNameWithoutExtension(attachment);
            bool attachementexist = false;
            foreach (var att in obaissue.AttachList)
            {
                if (att.Contains(srcfilename))
                {
                    attachementexist = true;
                    break;
                }
            }

            if (!attachementexist)
            {
                string datestring = DateTime.Now.ToString("yyyyMMdd");
                string imgdir = ctrl.Server.MapPath("~/userfiles") + "\\docs\\" + datestring + "\\";
                if (!DirectoryExists(ctrl, imgdir))
                    Directory.CreateDirectory(imgdir);

                var desfilename = Path.GetFileNameWithoutExtension(attachment) 
                    + DateTime.Now.ToString("-yyyyMMddhhmmss") + Path.GetExtension(attachment);
                var attpath = imgdir + desfilename;
                var url = "/userfiles/docs/" + datestring + "/" + desfilename;

                FileCopy(ctrl, attachment, attpath, true);
                IssueViewModels.StoreIssueAttachment(obaissue.IssueKey, url);
                obaissue.AttachList.Add(url);
            }
        }

        private static List<RawDMR> RetrieveDMRFromITDB(string OBAUpdateTime, Dictionary<string, IssueViewModels> DMRDict, Controller ctrl)
        {
            var ret = new List<RawDMR>();
            var sql = @"select a.DMR_ID,Created_at,Prod_Line,Defect_Qty,Inspected_Qty,Actual_Problem,Justification,a.Remark,File_URL from dbo.DMR_Detail_List_View a (nolock) 
                        left join View_OQC_Function_60_Approval_Remark b with (nolock) on b.DMR_ID = a.DMR_ID 
                         where a.Created_at > '<CreateTime>' and b.Remark like '%[RCCA]%'";

            sql = sql.Replace("<CreateTime>", OBAUpdateTime);
            var dbret = DBUtility.ExeFAISqlWithRes(sql);

            foreach (var line in dbret)
            {
                var dmrid = Convert.ToString(line[0]).Trim();
                if (!string.IsNullOrEmpty(dmrid) && !DMRDict.ContainsKey(dmrid))
                {
                    try
                    {
                        var tempdmr = new RawDMR();
                        tempdmr.DMR_ID = dmrid;
                        tempdmr.CreateTime = ConvertToDateStr(Convert.ToString(line[1]));
                        tempdmr.Product = Convert.ToString(line[2]);
                        tempdmr.DefectQty = Convert.ToString(line[3]);
                        tempdmr.TotalQty = Convert.ToString(line[4]);
                        tempdmr.Summary = Convert.ToString(line[5]);
                        if (tempdmr.Summary.Contains("SN:"))
                        {
                            var idx = tempdmr.Summary.IndexOf("SN:");
                            var splitstr = tempdmr.Summary.Substring(idx + 3).Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            tempdmr.ModuleSN = splitstr[0];
                        }
                        else if (tempdmr.Summary.Contains("SN :"))
                        {
                            var idx = tempdmr.Summary.IndexOf("SN :");
                            var splitstr = tempdmr.Summary.Substring(idx + 4).Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            tempdmr.ModuleSN = splitstr[0];
                        }
                        ret.Add(tempdmr);
                    }
                    catch (Exception ex) { }
                }//end if
                //else if (!string.IsNullOrEmpty(dmrid) && DMRDict.ContainsKey(dmrid))
                //{
                //    var obaissue = DMRDict[dmrid];
                //    var tempdmr = new RawDMR();
                //    tempdmr.DMR_ID = dmrid;
                //    tempdmr.RootCause = Conver2Str(line[6]);
                //    tempdmr.CorrectiveAction = Conver2Str(line[7]);
                //    tempdmr.Attachment = Conver2Str(line[8]);

                //    if (!string.IsNullOrEmpty(tempdmr.RootCause))
                //    {
                //        UpdateOBARootCause(obaissue, tempdmr.RootCause);
                //    }

                //    if (!string.IsNullOrEmpty(tempdmr.Attachment) && File.Exists(tempdmr.Attachment))
                //    {
                //        UpdateOBAAttachement(obaissue, tempdmr.Attachment, ctrl);
                //    }
                //}
            }//end foreach

            return ret;
        }

        private static void CreateOBATask(RawDMR dmr, string OBAAdmin,string defaultpj, Dictionary<string, string> OBARelateDict,Controller ctrl)
        {
            var vm = new IssueViewModels();
            vm.ProjectKey = defaultpj;
            vm.IssueKey = IssueViewModels.GetUniqKey();
            vm.IssueType = ISSUETP.OBA;

            vm.FinisarDMR = dmr.DMR_ID;
            if (vm.FinisarDMR.Length > 50)
            {
                vm.FinisarDMR = vm.FinisarDMR.Substring(0, 48);
            }

            vm.ModuleSN = dmr.ModuleSN;

            vm.OBAFailureRate =dmr.DefectQty+"/"+dmr.TotalQty;
            if (vm.OBAFailureRate.Length > 50)
            {
                vm.OBAFailureRate = vm.OBAFailureRate.Substring(0, 48);
            }

            vm.RelativePeoples = "";

            vm.Summary = "OBA " + vm.FinisarDMR + " " +dmr.Summary;
            if (vm.Summary.Length > 200)
            {
                vm.Summary = vm.Summary.Substring(0, 198);
            }

            vm.Priority = ISSUEPR.Major;

            vm.ProductType = dmr.Product;
            if (vm.ProductType.Length > 50)
            {
                vm.ProductType = vm.ProductType.Substring(0, 48);
            }

            vm.ReportDate = DateTime.Parse(dmr.CreateTime);


            if (OBARelateDict.ContainsKey(dmr.Product.ToUpper()))
            {
                vm.Assignee = OBARelateDict[dmr.Product.ToUpper()];
            }
            else
            {
                vm.Assignee = OBAAdmin;
            }
            if (!vm.Assignee.Contains("@"))
            {
                vm.Assignee = vm.Assignee+"@finisar.com";
            }
            vm.Assignee = vm.Assignee.ToUpper();
            if (vm.Assignee.Length > 200)
            {
                vm.Assignee = vm.Assignee.Substring(0, 198);
            }
            vm.Reporter = vm.Assignee;


            try
            {
                vm.DueDate = DateTime.Parse(dmr.CreateTime).AddDays(6);
            }
            catch (Exception e) { vm.DueDate = DateTime.Now.AddDays(6); }

            vm.Resolution = Resolute.Pending;
            vm.ResolvedDate = DateTime.Parse("1982-05-06 01:01:01");

            vm.Description = vm.Summary;
            vm.CommentType = COMMENTTYPE.Description;

            vm.DataID = "";
            vm.ErrAbbr = "";
            vm.FVCode = "";
            vm.MaterialDisposition = "";

            vm.StoreIssue();
            IssueTypeVM.SaveIssueType(vm.IssueKey, ISSUESUBTYPE.OBA.ToString());

            UserViewModels.RegisterUserAuto(vm.Assignee);
            SendOBAEvent(vm, "created",ctrl, true);

            //CreateRMASubIssue(RMASubIssueType.CONTAINMENTACTION, "Cotainment Action for OBA " + vm.FinisarDMR, vm.ProjectKey, vm.IssueKey, vm.Assignee, vm.Reporter, vm.DueDate.AddDays(14),ISSUESUBTYPE.CONTAINMENT.ToString());
            //CreateRMASubIssue(RMASubIssueType.CORRECTIVEACTION, "Corrective Action for OBA " + vm.FinisarDMR, vm.ProjectKey, vm.IssueKey, vm.Assignee, vm.Reporter, vm.DueDate.AddDays(28),ISSUESUBTYPE.CORRECTIVE.ToString());

            var comment = new IssueComments();
            comment.Comment = IssueCommentEmpty.TOBEEDIT;
            IssueViewModels.StoreIssueComment(vm.IssueKey, comment.dbComment, vm.Assignee, COMMENTTYPE.ContainmentAction);
            comment = new IssueComments();
            comment.Comment = IssueCommentEmpty.TOBEEDIT;
            IssueViewModels.StoreIssueComment(vm.IssueKey, comment.dbComment, vm.Assignee, COMMENTTYPE.RootCause);
            comment = new IssueComments();
            comment.Comment = IssueCommentEmpty.TOBEEDIT;
            IssueViewModels.StoreIssueComment(vm.IssueKey, comment.dbComment, vm.Assignee, COMMENTTYPE.CorrectiveAction);
        }

        private static void SendOBAEvent(IssueViewModels vm, string operate, Controller ctrl, bool nocheck = false)
        {
            var alertime = vm.RetrieveAlertEmailDate(vm.IssueKey);
            if ((!string.IsNullOrEmpty(alertime) && DateTime.Parse(alertime).AddHours(24) < DateTime.Now) || nocheck)
            {
                vm.UpdateAlertEmailDate();

                var routevalue = new RouteValueDictionary();
                routevalue.Add("issuekey", vm.IssueKey);
                //send validate email
                string scheme = ctrl.Url.RequestContext.HttpContext.Request.Url.Scheme;
                string validatestr = ctrl.Url.Action("UpdateIssue", "Issue", routevalue, scheme);

                var netcomputername = EmailUtility.RetrieveCurrentMachineName();
                validatestr = validatestr.Replace("//localhost", "//" + netcomputername);

                var content = vm.Summary + " is " + operate + " by " + vm.Reporter + " :\r\n " + validatestr;

                var toaddrs = new List<string>();
                toaddrs.Add(vm.Assignee);
                toaddrs.Add(vm.Reporter);
                EmailUtility.SendEmail(ctrl, "WUXI Engineering System", toaddrs, content);
                new System.Threading.ManualResetEvent(false).WaitOne(50);
            }
        }

        public static void RefreshOBAFromDMR(Controller ctrl)
        {
            var syscfgdict = CfgUtility.GetSysConfig(ctrl);
            var OBAAdmin = syscfgdict["OBAADMIN"];
            var OBADefaultPJ = syscfgdict["OBADEFAULTPJ"];

            var OBARelateDict = new Dictionary<string, string>();
            foreach (var kv in syscfgdict)
            {
                if (kv.Key.Contains("OBA--"))
                {
                    OBARelateDict.Add(kv.Key.Replace("OBA--", "").ToUpper(), kv.Value);
                }
            }

            var OBAUpdateTime = RetrieveOBAUpdateTime();
            var DMRDict = RetrieveExistDMRDict(ctrl,DateTime.Now.AddDays(-14).ToString("yyyy-MM-dd HH:mm:ss"),DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            var dmrlist = RetrieveDMRFromITDB(OBAUpdateTime, DMRDict,ctrl);
            var currentdbdmrfilter = new Dictionary<string, bool>();
            foreach (var dmr in dmrlist)
            {
                if (!currentdbdmrfilter.ContainsKey(dmr.DMR_ID))
                {
                    CreateOBATask(dmr, OBAAdmin, OBADefaultPJ, OBARelateDict, ctrl);
                    currentdbdmrfilter.Add(dmr.DMR_ID, true);
                }
            }
        }

        #endregion

        #region TraceView

        private static string RetrieveTimeFromTraceViewName(string filename)
        {
            var tracestrs = filename.Split(new string[] { "_TRACEVIEW_" }, StringSplitOptions.RemoveEmptyEntries);
            if (tracestrs.Length > 1)
            {
                var dutstrs = tracestrs[1].Split(new string[] { "_DUT" }, StringSplitOptions.RemoveEmptyEntries);
                var timestr = dutstrs[0].Replace("_", "").Replace(" ", "");
                return timestr.Substring(0, 4) + "-" + timestr.Substring(4, 2) + "-" + timestr.Substring(6, 2) + " " + timestr.Substring(8, 2) + ":" + timestr.Substring(10, 2) + ":" + timestr.Substring(12, 2);
            }
            return null;
        }

        private static List<TraceViewData> RetrieveTestDataFromTraceView_DUTORDERED(ProjectCriticalErrorVM pjerror,string filename, string testcase, string datafield)
        {
            var ret = new List<TraceViewData>();

            if (!File.Exists(filename))
                return ret;

            var allline = System.IO.File.ReadAllLines(filename);
            var crttemp = 25.0;
            var crtch = 0;

            var entertestcase = false;

            foreach (var line in allline)
            {
                var uline = line.ToUpper();

                if ((string.Compare(testcase,"ALL",true) == 0) 
                    || (uline.Contains(testcase.ToUpper()) && uline.Contains("STARTED")))
                {
                    entertestcase = true;
                    if (uline.Contains("C ---") && uline.Contains("@"))
                    {
                        var head = uline.Split(new string[] { "C ---" }, StringSplitOptions.RemoveEmptyEntries);
                        var tempstrs = head[0].Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries);
                        if (tempstrs.Length > 1)
                        {
                            try
                            {
                                crttemp = Convert.ToDouble(tempstrs[1].Trim());
                            }
                            catch (Exception ex) { }
                        }
                    }
                }//end if

                if (uline.Contains(testcase.ToUpper()) && uline.Contains("COMPLETED"))
                {
                    entertestcase = false;
                }

                if (string.Compare(testcase, "ALL", true) == 0)
                {
                    entertestcase = true;
                }

                if (pjerror.WithWildMatch == 1)
                {
                    if (entertestcase && uline.Contains(datafield.ToUpper()))
                    {
                        var fields = uline.Split(new string[] {  datafield.ToUpper() }, StringSplitOptions.RemoveEmptyEntries);
                        if (fields.Length > 1)
                        {
                            var chstr = fields[0].Replace("\t", "").Replace(" ", "").Replace("[", "").Replace("]", "").Replace("-", "");
                            try
                            { crtch = Convert.ToInt32(chstr); }
                            catch (Exception ex) { }

                            var tmpvaluestr = fields[1].Trim();
                            var tmpvals = tmpvaluestr.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            try
                            {
                                if (!tmpvals[0].Contains("<NM>"))
                                {
                                    var temptraceviewdata = new TraceViewData();
                                    temptraceviewdata.Temp = crttemp;
                                    temptraceviewdata.CH = crtch;
                                    temptraceviewdata.Value = tmpvals[0].Replace("<","").Replace(">", "");
                                    ret.Add(temptraceviewdata);
                                }
                            }
                            catch (Exception ex) { }
                        }
                    }
                }
                else
                {
                    if (entertestcase && line.Contains("] --- ") && uline.Contains(" "+datafield.ToUpper()+" "))
                    {
                        var fields = uline.Split(new string[] { " " + datafield.ToUpper() + " " }, StringSplitOptions.RemoveEmptyEntries);
                        if (fields.Length > 1)
                        {
                            var chstr = fields[0].Replace("\t", "").Replace(" ", "").Replace("[", "").Replace("]", "").Replace("-", "");
                            try
                            { crtch = Convert.ToInt32(chstr); }
                            catch (Exception ex) { }

                            var tmpvaluestr = fields[1].Trim();
                            var tmpvals = tmpvaluestr.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            try
                            {
                                if (!tmpvals[0].Contains("<NM>"))
                                {
                                    var tempval = Convert.ToDouble(tmpvals[0]);

                                    var temptraceviewdata = new TraceViewData();
                                    temptraceviewdata.Temp = crttemp;
                                    temptraceviewdata.CH = crtch;
                                    temptraceviewdata.Value = tmpvals[0];
                                    ret.Add(temptraceviewdata);
                                }
                            }
                            catch (Exception ex) { }
                        }//end if
                    }//en if
                }


            }//end foreach
            return ret;
        }

        private static List<TraceViewData> RetrieveTestDataFromTraceView_DUTx(ProjectCriticalErrorVM pjerror, string filename, string testcase, string datafield)
        {
            var ret = new List<TraceViewData>();

            if (!File.Exists(filename))
                return ret;

            var allline = System.IO.File.ReadAllLines(filename);
            var crttemp = 25.0;
            var crtch = 0;

            var entertestcase = false;

            foreach (var line in allline)
            {
                var uline = line.ToUpper();

                if ((string.Compare(testcase, "ALL", true) == 0)
                    || (uline.Contains(testcase.ToUpper()) && uline.Contains("STARTED")))
                {
                    entertestcase = true;
                    if (uline.Contains("C ---") && uline.Contains("@"))
                    {
                        var head = uline.Split(new string[] { "C ---" }, StringSplitOptions.RemoveEmptyEntries);
                        var tempstrs = head[0].Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries);
                        if (tempstrs.Length > 1)
                        {
                            try
                            {
                                crttemp = Convert.ToDouble(tempstrs[1].Trim());
                            }
                            catch (Exception ex) { }
                        }
                    }
                }//end if

                if (uline.Contains(testcase.ToUpper()) && uline.Contains("COMPLETED"))
                {
                    entertestcase = false;
                }

                if (string.Compare(testcase, "ALL", true) == 0)
                {
                    entertestcase = true;
                }

                if (pjerror.WithWildMatch == 1)
                {
                    if (entertestcase && uline.Contains(datafield.ToUpper()))
                    {
                        var fields = uline.Split(new string[] { datafield.ToUpper() }, StringSplitOptions.RemoveEmptyEntries);
                        if (fields.Length > 1)
                        {
                            var chstr = fields[0].Replace("\t", "").Replace(" ", "").Replace("[", "").Replace("]", "").Replace("-", "");
                            try
                            { crtch = Convert.ToInt32(chstr); }
                            catch (Exception ex) { }

                            var tmpvaluestr = fields[1].Trim();
                            var tmpvals = tmpvaluestr.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            try
                            {
                                if (!tmpvals[0].Contains("<NM>"))
                                {
                                    var temptraceviewdata = new TraceViewData();
                                    temptraceviewdata.Temp = crttemp;
                                    temptraceviewdata.CH = crtch;
                                    temptraceviewdata.Value = tmpvals[0].Replace("<", "").Replace(">", "");
                                    ret.Add(temptraceviewdata);
                                }
                            }
                            catch (Exception ex) { }
                        }
                    }
                }
                else
                {
                    if (entertestcase && line.Contains("--- ") && uline.Contains(" " + datafield.ToUpper() + " "))
                    {
                        var fields = uline.Split(new string[] { " " + datafield.ToUpper() + " " }, StringSplitOptions.RemoveEmptyEntries);
                        if (fields.Length > 1)
                        {
                            //var chstr = fields[0].Replace("\t", "").Replace(" ", "").Replace("[", "").Replace("]", "").Replace("-", "");
                            //try
                            //{ crtch = Convert.ToInt32(chstr); }
                            //catch (Exception ex) { }

                            var tmpvaluestr = fields[1].Trim();
                            var tmpvals = tmpvaluestr.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            try
                            {
                                if (!tmpvals[0].Contains("<NM>"))
                                {
                                    var tempval = Convert.ToDouble(tmpvals[0]);
                                    var temptraceviewdata = new TraceViewData();
                                    temptraceviewdata.Temp = crttemp;
                                    temptraceviewdata.CH = crtch;
                                    temptraceviewdata.Value = tmpvals[0];
                                    ret.Add(temptraceviewdata);
                                }
                            }
                            catch (Exception ex) { }
                        }//end if
                    }//en if
                }

            }//end foreach
            return ret;
        }


        public static List<TraceViewData> RetrieveTestDataFromTraceView(ProjectCriticalErrorVM pjerror, string filename,string testcase,string datafield)
        {
            if (filename.ToUpper().Contains("_DUTORDERED_"))
            {
                return RetrieveTestDataFromTraceView_DUTORDERED(pjerror, filename, testcase, datafield);
            }
            else
            {
                return RetrieveTestDataFromTraceView_DUTx(pjerror, filename, testcase, datafield);
            }
        }

        public static List<string> LoadTraceView2Local(string tester, string sn, string whichtest, string dbtimestr, Controller ctrl)
        {
            var ret = new List<string>();
            try
            {
                var syscfgdict = CfgUtility.GetSysConfig(ctrl);
                var traceviewscrfolder = syscfgdict["TRACEVIEWFOLDER"]+"\\"+tester;
                var allsrcfiles = DirectoryEnumerateFiles(ctrl, traceviewscrfolder);

                string imgdir = ctrl.Server.MapPath("~/userfiles") + "\\docs\\TraceView\\";
                if (!DirectoryExists(ctrl, imgdir))
                {
                    Directory.CreateDirectory(imgdir);
                }

                foreach (var srcf in allsrcfiles)
                {
                    var filename = Path.GetFileName(srcf).ToUpper();

                    if (filename.Contains(sn.ToUpper())
                        && filename.Contains(whichtest.ToUpper())
                        && filename.Contains("_DUTORDERED_")
                        && filename.Contains("_TRACEVIEW_"))
                    {
                        var traceviewtimestr = RetrieveTimeFromTraceViewName(filename);
                        if (traceviewtimestr == null)
                            continue;

                        try
                        {
                            var traceviewtime = DateTime.Parse(traceviewtimestr);
                            var dbtime = DateTime.Parse(dbtimestr);
                            if (traceviewtime > dbtime.AddSeconds(-5) && traceviewtime < dbtime.AddSeconds(5))
                            {
                                logthdinfo("\r\nStart to copy file: " + srcf);
                                var desfile = imgdir + filename;
                                FileCopy(ctrl, srcf, desfile, true);
                                if (FileExist(ctrl, desfile))
                                {
                                    logthdinfo("try to add data from file: " + desfile);
                                    ret.Add(desfile);
                                }//copied file exist
                            }
                        }
                        catch (Exception ex)
                        {
                            logthdinfo("LoadTraceView2Local Exception: " + ex.Message);
                        }
                    }//end if
                }//end foreach

                if (ret.Count == 0)
                {
                    foreach (var srcf in allsrcfiles)
                    {
                        var filename = Path.GetFileName(srcf).ToUpper();

                        if (filename.Contains(sn.ToUpper())
                            && filename.Contains(whichtest.ToUpper())
                            && filename.Contains("_DUT")
                            && filename.Contains("_TRACEVIEW_"))
                        {
                            var traceviewtimestr = RetrieveTimeFromTraceViewName(filename);
                            if (traceviewtimestr == null)
                                continue;

                            try
                            {
                                var traceviewtime = DateTime.Parse(traceviewtimestr);
                                var dbtime = DateTime.Parse(dbtimestr);
                                if (traceviewtime > dbtime.AddSeconds(-5) && traceviewtime < dbtime.AddSeconds(5))
                                {
                                    logthdinfo("\r\nStart to copy file: " + srcf);
                                    var desfile = imgdir + filename;
                                    FileCopy(ctrl, srcf, desfile, true);
                                    if (FileExist(ctrl, desfile))
                                    {
                                        logthdinfo("try to add data from file: " + desfile);
                                        ret.Add(desfile);
                                    }//copied file exist
                                }
                            }
                            catch (Exception ex)
                            {
                                logthdinfo("LoadTraceView2Local Exception: " + ex.Message);
                            }
                        }//end if
                    }//end foreach
                }//end if ret == 0
            }
            catch (Exception e) { }


            return ret;
        }
        #endregion

        #region WAFERCOOR
        public static void RefreshWaferCoordByDate(Controller ctrl, string date)
        {
            var syscfgdict = CfgUtility.GetSysConfig(ctrl);
            string datestring = date;

            var coordsrcfolder = syscfgdict["WAFERCOORD"];
            
            var coordsrcfiles = DirectoryEnumerateFiles(ctrl, coordsrcfolder+"\\"+ datestring);
            if(coordsrcfiles.Count > 0)
            {
                string imgdir = ctrl.Server.MapPath("~/userfiles") + "\\docs\\" + datestring + "\\";
                if (!DirectoryExists(ctrl, imgdir))
                {
                    Directory.CreateDirectory(imgdir);
                }
                var new_data = new Dictionary<string, WaferCoordRAWData>();
                foreach (var srcf in coordsrcfiles)
                {
                    var filename = Path.GetFileName(srcf);
                    var testtime = File.GetLastWriteTime(srcf).ToString("yyyy-MM-dd HH:mm:ss");
                    try
                    {
                        var desfile = imgdir + filename;
                        FileCopy(ctrl, srcf, desfile, true);
                        if (FileExist(ctrl, desfile))
                        {
                            var data = RetrieveDataFromExcelWithAuth(ctrl, desfile,null, 9);
                            var tmp = SaveWaferCoordData(data, testtime, coordsrcfolder, ctrl);
                            new_data.Intersect(tmp).ToList().ForEach(x => new_data.Remove(x.Key));
                            new_data = new_data.Concat(tmp).ToDictionary(x => x.Key, x => x.Value);
                        }
                    }
                    catch (Exception ex)
                    {
                        var logInfo = "\r\nSolveWaferCoordData Exception: " + ex.Message;
                        logthdinfo(logInfo, "wafercoordtrace");
                    }
                }
                if (new_data.Count > 0)
                {
                    var wafer_coords = WaferCoordRAWData.GetWaferInfoByKeys(new_data);
                    for (var i = 1; i <= Math.Ceiling(wafer_coords.Count / 10.0); i++)
                    {
                        var tmp = wafer_coords.Skip((i - 1) * 10).Take(10).ToList();
                        WaferCoordRAWData.CreateWaferCoordData(tmp);
                    }
                }
            }
        }

        public static Dictionary<string, WaferCoordRAWData> SaveWaferCoordData(List<List<string>> data, string date, string attfolder, Controller ctrl)
        {
            var res = new Dictionary<string, WaferCoordRAWData>();
            if (data.Count == 0)
                return res;
            if (!data[0][0].ToUpper().Contains("LOCATION"))
                return res;
            var test_time = date;
            var idx = 0;
            var sn_arr = new List<string>();
            var coord_arr = new List<string>();
            var all_coords = new Dictionary<string, WaferCoordRAWData>();
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            foreach(var line in data[1])
            {
                if (!string.IsNullOrEmpty(line))
                {
                    if(data[2][idx].Split(new char[] { ',' }).Length >= 2)
                    {
                        var tmp = new WaferCoordRAWData();
                        tmp.SN = line;
                        tmp.Coord_X = Convert.ToInt32(data[2][idx].Split(new char[] { ',' })[0]).ToString();
                        tmp.Coord_Y = Convert.ToInt32(data[2][idx].Split(new char[] { ',' })[1]).ToString();
                        tmp.TestTime = test_time;
                        tmp.SyncTime = now;
                        if (!all_coords.ContainsKey(tmp.SN + ":" + tmp.Coord_X + ":" + tmp.Coord_Y))
                        {
                            all_coords.Add(tmp.SN + ":" + tmp.Coord_X + ":" + tmp.Coord_Y, tmp);
                        }
                        idx++;
                    }
                }
            }
            if(all_coords.Count > 0)
            {
                res = SolveWaferCoordData(all_coords);
            }
            return res;
        }

        public static Dictionary<string, WaferCoordRAWData> SolveWaferCoordData(Dictionary<string, WaferCoordRAWData> data)
        {
            var res = new List<WaferCoordRAWData>();
            var exist_data = WaferCoordRAWData.GetWaferCoordData("'" + string.Join("','", data.Keys) + "'");
            exist_data.Keys.ToList().ForEach(key => data.Remove(key));
            return data;
        }

        public static void RefreshWaferCoord(Controller ctrl)
        {
            //pre
            var zero_day = Convert.ToDateTime("2018-04-23 00:00:00");
            var end_day = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59"));
            var min_testtime = WaferCoordRAWData.GetSyncMinTestTime();
            if (!string.IsNullOrEmpty(min_testtime))
            {
                end_day = Convert.ToDateTime(min_testtime).AddDays(-1);
            }
            var tmp_day = Convert.ToDateTime(zero_day);
            while(tmp_day <= end_day)
            {
                RefreshWaferCoordByDate(ctrl, tmp_day.ToString("yyyyMMdd"));
                tmp_day = Convert.ToDateTime(tmp_day).AddDays(+1);
            }
            //next
            var max_testtime = WaferCoordRAWData.GetSyncMaxTestTime();
            zero_day = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));
            end_day = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59"));
            if (!string.IsNullOrEmpty(max_testtime))
            {
                zero_day = Convert.ToDateTime(max_testtime);
            }
            tmp_day = Convert.ToDateTime(zero_day);
            while (tmp_day <= end_day)
            {
                RefreshWaferCoordByDate(ctrl, tmp_day.ToString("yyyyMMdd"));
                tmp_day = Convert.ToDateTime(tmp_day).AddDays(+1);
            }
        }

        #endregion

        #region VCSELRMA
        private static void SolveVcselRMAData(Controller ctrl,string vcselrmafile)
        {
                var existrmasn = VcselRMAData.GetAllVcselRMASN();
                var vcselpndict = VcselPNData.RetrieveVcselPNInfo();

                var idx = 0;
                var data = RetrieveDataFromExcelWithAuth(ctrl, vcselrmafile, "Master list");
                var vcselrmalist = new List<VcselRMAData>();

                foreach (var line in data)
                {
                    if (idx == 0)
                    {
                        idx = idx + 1;
                        continue;
                    }

                    var sn = line[10];
                    if (!existrmasn.ContainsKey(sn))
                    {
                        existrmasn.Add(sn, true);

                        var tempvm = new VcselRMAData();
                        tempvm.SN = sn;
                        tempvm.PN = line[8];
                        tempvm.PNDesc = line[9];
                        tempvm.VcselPN = line[11];
                        if (vcselpndict.ContainsKey(tempvm.VcselPN))
                        {
                            tempvm.VcselType = vcselpndict[tempvm.VcselPN].Rate;
                        }
                        else
                        {
                            tempvm.VcselType = "OTHERS";
                        }
                        tempvm.RMANum = line[0];
                        tempvm.Customer = line[1];
                        tempvm.ProductType = line[2];
                        tempvm.ShipDate = line[3];
                        tempvm.RMAOpenDate = line[4];

                        vcselrmalist.Add(tempvm);
                    }//not exist
                }//end foreach

                if (vcselrmalist.Count > 0)
                {
                    var snlist = new List<string>();
                    foreach (var item in vcselrmalist)
                    {
                        snlist.Add(item.SN);
                    }

                    var snwaferdict = BIDataUtility.RetrieveBIWaferBySN_SNDict(snlist);
                    foreach (var item in vcselrmalist)
                    {
                        if (snwaferdict.ContainsKey(item.SN))
                        {
                            item.Wafer = snwaferdict[item.SN].Key;
                            item.BuildDate = DateTime.Parse(snwaferdict[item.SN].Value);

                            item.StoreVcselRMA();
                        }
                    }//end foreach
                }

                foreach (var item in vcselrmalist)
                {
                    if (!string.IsNullOrEmpty(item.Wafer))
                    {
                        WaferSNMap.UpdateWaferInfo(item.Wafer);
                    }
                }
        }

        private static void SolveMailStoneData(Controller ctrl, string vcselrmafile)
        {
            var idx = 0;
            var data = RetrieveDataFromExcelWithAuth(ctrl, vcselrmafile, "Changes milestone");
            var milestonelist = new List<EngineeringMileStone>();

            foreach (var line in data)
            {
                if (idx == 0)
                {
                    idx = idx + 1;
                    continue;
                }

                var tempvm = new EngineeringMileStone();
                tempvm.ActionDate = DateTime.Parse(line[2]);
                tempvm.Location = line[3];
                tempvm.ActionDetail = line[0] + " # " + line[4];
                tempvm.AppendInfo = line[0];
                if (string.IsNullOrEmpty(tempvm.AppendInfo))
                {
                    tempvm.AppendInfo = "OTHERS";
                }
                milestonelist.Add(tempvm);
            }

            EngineeringMileStone.UpdateVcselMileStone(milestonelist);
        }

        public static void RefreshVcselRMAData(Controller ctrl)
        {
            var syscfg = CfgUtility.GetSysConfig(ctrl);
            var vcselrmafile = ExternalDataCollector.DownloadShareFile(syscfg["VCSELRMASHARE"], ctrl);
            if (vcselrmafile != null && ExternalDataCollector.FileExist(ctrl, vcselrmafile))
            {
                SolveVcselRMAData(ctrl, vcselrmafile);
                SolveMailStoneData(ctrl, vcselrmafile);
            }//end if
        }

        #endregion

        #region MONITOROQMJO
        private static Dictionary<string, bool> RetrieveOpeningOQMJo()
        {
            var oqmlist = new Dictionary<string, bool>();
            var sql = "select JONumber from JOBaseInfo where BRKey = 'OQM' and JORealStatus = 'OPEN'";
            var dbret = DBUtility.ExeNebulaSqlWithRes(sql, null);
            {
                foreach (var line in dbret)
                {
                    var jo = Convert2Str(line[0]);
                    if (!string.IsNullOrEmpty(jo) && !oqmlist.ContainsKey(jo))
                    { oqmlist.Add(jo, true); }
                }
            }
            return oqmlist;
        }

        public static Dictionary<string, string> RetrieveSNFromJo(Dictionary<string, bool> oqmjolist)
        {
            var snlist = new Dictionary<string, string>();

            StringBuilder sb1 = new StringBuilder(10 * (snlist.Count + 5));
            sb1.Append("('");
            foreach (var line in oqmjolist)
            {
                sb1.Append(line.Key + "','");
            }
            var tempstr1 = sb1.ToString();
            var jocond = tempstr1.Substring(0, tempstr1.Length - 2) + ")";

            var sql = @" select c.ContainerName,m.MfgOrderName from [InsiteDB].[insite].[Container] c (nolock) 
                            left join[InsiteDB].[insite].[MfgOrder] m(nolock) on m.MfgOrderId = c.MfgOrderId
                            where m.MfgOrderName  in <jocond>";
            sql = sql.Replace("<jocond>", jocond);
            var dbret = DBUtility.ExeRealMESSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var sn = Convert2Str(line[0]);
                var jo = Convert2Str(line[1]);
                if (!snlist.ContainsKey(sn) && sn.Length <= 7)
                {
                    snlist.Add(sn, jo);
                }
            }
            return snlist;
        }

        public static List<string> RetrieveDCTableFromSn(string sn)
        {
            var ret = new List<string>();
            var dctabledict = new Dictionary<string, bool>();

            var sql = @" select ddr.DataCollectionDefName from insitedb.insite.DataCollectionDefBase ddr  (nolock)
	                    inner join insitedb.insite.TxnMap tm with(noloCK) ON tm.DataCollectionDefinitionBaseId = ddr.DataCollectionDefBaseId
	                    inner join insitedb.insite.spec sp with(nolock) on sp.specid =  tm.specid
	                    inner join InsiteDB.insite.WorkflowStep ws (nolock)on  ws.specbaseid = sp.specbaseid
	                    inner join InsiteDB.insite.Workflow w (nolock)on w.WorkflowID = ws.WorkflowID
                        inner join InsiteDB.insite.Product p(nolock) on w.WorkflowBaseId = p.WorkflowBaseId
	                    inner join [InsiteDB].[insite].[Container] c(nolock) on c.ProductId = p.ProductId
                        where c.ContainerName = '<ContainerName>' and ddr.DataCollectionDefName is not null";
            sql = sql.Replace("<ContainerName>", sn);
            var dbret = DBUtility.ExeRealMESSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var dc = Convert2Str(line[0]).ToUpper();
                if (dc.Length > 4 && dc.Substring(0, 4).Contains("DCD_"))
                {
                    var realdc = "";
                    if (dc.Contains("DCD_Module_Initialization_0811".ToUpper()))
                    { realdc = "dc_initial"; }
                    else
                    { realdc = "dc_" + dc.Substring(4); }

                    if (!dctabledict.ContainsKey(realdc)) {
                        dctabledict.Add(realdc, true);
                    }
                }//end if
            }//end foreach
            ret.AddRange(dctabledict.Keys);
            return ret;
        }

        public static List<ProjectTestData> RetrieveLatestSNTestResult(string sn,string defaultpj)
        {
            var dctablelist = RetrieveDCTableFromSn(sn);
            var testdatalist = new List<ProjectTestData>();

            foreach (var dctable in dctablelist)
            {
                var sql = @"select top 1 a.<DCTABLE>HistoryId,a.ModuleSerialNum, a.WhichTest, a.ModuleType, a.ErrAbbr, a.TestTimeStamp, a.TestStation,a.assemblypartnum 
                               from insite.<DCTABLE> a (nolock) where a.ModuleSerialNum = '<ModuleSerialNum>' order by  testtimestamp DESC";
                sql = sql.Replace("<DCTABLE>", dctable).Replace("<ModuleSerialNum>", sn);
                var dbret = DBUtility.ExeRealMESSqlWithRes(sql);
                foreach (var item in dbret)
                {
                    var tempdata = new ProjectTestData(defaultpj, Convert.ToString(item[0]), Convert.ToString(item[1])
                                        ,Convert.ToString(item[2]), Convert.ToString(item[3]), Convert.ToString(item[4])
                                        , Convert.ToString(item[5]), Convert.ToString(item[6]), Convert.ToString(item[7]));
                    testdatalist.Add(tempdata);
                }
            }

            testdatalist.Sort(delegate (ProjectTestData obj1, ProjectTestData obj2)
            {
                return obj2.TestTimeStamp.CompareTo(obj1.TestTimeStamp);
            });

            var ret = new List<ProjectTestData>();
            ret.Add(testdatalist[0]);
            return ret;
        }

        public static void SolveOQMSN(string sn, string jo,string asignee, string defaultpj, Controller ctrl)
        {

            var testdata = RetrieveLatestSNTestResult(sn,defaultpj);
            if (testdata.Count > 0)
            {
                var td = testdata[0];

                if (string.Compare(td.ErrAbbr, "PASS", true) == 0)
                {
                    var desc = "Module " + td.ModuleSerialNum + " latest status is: pass on " + td.WhichTest + " @ " + td.TestTimeStamp.ToString("yyyy-MM-dd HH:mm:ss");
                    IssueViewModels.CloseIssueAutomaticlly(td.ModuleSerialNum, desc, ctrl);
                }
                else
                {
                    var issues = IssueViewModels.RetrieveIssueBySN(td.ModuleSerialNum, ctrl);
                    foreach (var item in issues)
                    {
                        if (item.ReportDate.Equals(td.TestTimeStamp))
                        { return; }
                    }

                    var desc = "Module " + td.ModuleSerialNum + " latest status is: failed on " + td.WhichTest + " @ " + td.TestTimeStamp.ToString("yyyy-MM-dd HH:mm:ss");
                    IssueViewModels.CloseIssueAutomaticlly(td.ModuleSerialNum, desc, ctrl);

                    var now = DateTime.Now;
                    var vm = new IssueViewModels();
                    vm.ProjectKey = defaultpj;
                    vm.IssueKey = td.DataID;
                    vm.IssueType = ISSUETP.Task;
                    vm.Summary = "OQM Task: "+ td.ModuleSerialNum + " failed for " + td.ErrAbbr + " @ " + td.WhichTest + " "+ td.TestTimeStamp.ToString("yyyy-MM-dd HH:mm:ss");
                    vm.ModuleSN = td.ModuleSerialNum;
                    vm.ErrAbbr = td.ErrAbbr;
                    vm.Priority = ISSUEPR.Major;
                    vm.DueDate = td.TestTimeStamp.AddDays(7);
                    vm.ReportDate = td.TestTimeStamp;
                    if (!asignee.Contains("@"))
                    { asignee = asignee + "@finisar.com"; }

                    vm.Assignee = asignee;
                    vm.Reporter = asignee;
                    vm.Creator = asignee;
                    vm.Resolution = Resolute.Pending;
                    vm.ResolvedDate = DateTime.Parse("1982-05-06 01:01:01");
                    vm.RelativePeoples = "";
                    vm.StoreIssue();

                    var comment = new IssueComments();
                    comment.Comment = "JO Distribution: http://wuxinpi.china.ads.finisar.com:8082/BRTrace/JODetail?JONum="+jo+"&Step=3";
                    IssueViewModels.StoreIssueComment(vm.IssueKey, comment.dbComment, "system", COMMENTTYPE.Description);

                    SendOBAEvent(vm, "created", ctrl, true);
                }
            }
        }

        public static void RefreshOQMJo(Controller ctrl)
        {
            var oqmlist = RetrieveOpeningOQMJo();
            if (oqmlist.Count > 0)
            {
                var snlist = RetrieveSNFromJo(oqmlist);
                if(snlist.Count > 0)
                {
                    var syscfgdict = CfgUtility.GetSysConfig(ctrl);
                    var OBAAdmin = syscfgdict["OBAADMIN"];
                    var OBADefaultPJ = syscfgdict["OBADEFAULTPJ"];

                    foreach (var snjo in snlist)
                    {
                        try
                        {
                            SolveOQMSN(snjo.Key, snjo.Value, OBAAdmin, OBADefaultPJ, ctrl);
                        }
                        catch (Exception ex) { }
                    }
                }//end if
            }//end if
        }

        #endregion

        #region SHIP-DATA

        private static Dictionary<string, string> CableSN2RealSN(Dictionary<string, string> pnsndict)
        {
            var newpnsndict = new Dictionary<string, string>();

            var sndict = new Dictionary<string, string>();

            var sb = new StringBuilder();
            sb.Append("('");
            foreach (var kv in pnsndict)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    sb.Append(kv.Value + "','");
                }
            }
            var sncond = sb.ToString(0, sb.Length - 2) + ")";

            var sql = @"SELECT  max([FromContainer]),ToContainer
                        FROM [PDMS].[dbo].[ComponentIssueSummary] where ToContainer in <sncond> and len([FromContainer]) = 7 group by ToContainer";
            sql = sql.Replace("<sncond>", sncond);
            var dbret = DBUtility.ExeMESReportSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var tosn = Convert.ToString(line[1]);
                var fromsn = Convert.ToString(line[0]);
                if (!sndict.ContainsKey(tosn))
                {
                    sndict.Add(tosn, fromsn);
                }
            }

            foreach (var kv in pnsndict)
            {
                if (sndict.ContainsKey(kv.Value))
                {
                    newpnsndict.Add(kv.Key, sndict[kv.Value]);
                }
                else
                {
                    newpnsndict.Add(kv.Key, kv.Value);
                }
            }
            return newpnsndict;
        }

        public static Dictionary<string,string> PN2MPn(Dictionary<string, string> pnsndict)
        {
            var sb = new StringBuilder();
            sb.Append("('");
            foreach (var kv in pnsndict)
            {
                sb.Append(kv.Key + "','");
            }
            var pncond = sb.ToString(0, sb.Length - 2) + ")";

            var sql = @" select max(c.ContainerName),pb.productname from InsiteDB.insite.container  c (nolock) 
	                     left join InsiteDB.insite.product p on c.productId = p.productId 
	                     left join InsiteDB.insite.productbase pb on pb.productbaseid = p.productbaseid 
	                     where pb.productname in <pncond> and len(c.ContainerName) = 7 group by pb.productname";
            sql = sql.Replace("<pncond>", pncond);
            var dbret = DBUtility.ExeMESSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var sn = Convert.ToString(line[0]);
                var pn = Convert.ToString(line[1]);
                if (pnsndict.ContainsKey(pn))
                {
                    pnsndict[pn] = sn;
                }
            }//end foreach

            var newpnsndict =  CableSN2RealSN(pnsndict);

            sb = new StringBuilder();
            sb.Append("('");
            foreach (var kv in newpnsndict)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    sb.Append(kv.Value + "','");
                }
            }
            var sncond = sb.ToString(0, sb.Length - 2) + ")";

            var snmpndict = BIDataUtility.RetrieveSNMaterial(sncond);

            var ret = new Dictionary<string, string>();
            foreach (var pnsn in newpnsndict)
            {
                if (!string.IsNullOrEmpty(pnsn.Value) && snmpndict.ContainsKey(pnsn.Value))
                {
                    if (!ret.ContainsKey(pnsn.Key))
                    {
                        ret.Add(pnsn.Key, snmpndict[pnsn.Value]);
                    }
                }
            }
            return ret;
        }

        public static Dictionary<string, string> PN2VType(Dictionary<string, string> pn2mpndict)
        {
            var ret = new Dictionary<string, string>();
            var mpnvdict = VcselPNData.RetrieveVcselPNInfo();
            var pnvdict = new Dictionary<string, string>();
            foreach (var pnmpn in pn2mpndict)
            {
                if (!string.IsNullOrEmpty(pnmpn.Value) && mpnvdict.ContainsKey(pnmpn.Value))
                {
                    if (!ret.ContainsKey(pnmpn.Key))
                    {
                        ret.Add(pnmpn.Key, mpnvdict[pnmpn.Value].Rate);
                    }
                }
            }
            return ret;
        }

        public static void RefreshShipData(Controller ctrl)
        {
            var syscfg = CfgUtility.GetSysConfig(ctrl);
            var shipsrcfile = syscfg["FINISARSHIPDATA"];
            var shipdesfile = DownloadShareFile(shipsrcfile,ctrl);

            if (!string.IsNullOrEmpty(shipdesfile))
            {
                var shipiddict = FsrShipData.RetrieveAllShipID();
                var data = RetrieveDataFromExcelWithAuth(ctrl, shipdesfile);
                var shipdatalist = new List<FsrShipData>();
                var pnsndict = new Dictionary<string, string>();

                foreach (var line in data)
                {
                    try {
                        var shipid = Convert2Str(line[8]) +"-"+ Convert2Str(line[9]);
                        if (!shipiddict.ContainsKey(shipid))
                        {
                            var cpo = Convert2Str(line[5]).ToUpper();
                            var makebuy = Convert2Str(line[27]).ToUpper();
                            var family = Convert2Str(line[30]);
                            var shipqty = Convert.ToInt32(line[14]);
                            var pn = Convert2Str(line[10]);

                            if (!cpo.Contains("RMA") && !cpo.Contains("STOCK")
                                && makebuy.Contains("MAKE") && !string.IsNullOrEmpty(family)
                                && shipqty > 0 && !string.IsNullOrEmpty(pn))
                            {
                                var ordereddate = Convert.ToDateTime(line[2]);
                                var customernum = Convert2Str(line[3]);
                                var customer1 = Convert2Str(line[4]);
                                var customer2 = Convert2Str(line[6]);
                                var pndesc = Convert2Str(line[12]);
                                var shipdate = Convert.ToDateTime(line[19]);
                                var cfg = Convert2Str(line[32]);
                                var delievenum = Convert2Str(line[24]);

                                if (!pnsndict.ContainsKey(pn))
                                {
                                    pnsndict.Add(pn, string.Empty);
                                }

                                shipdatalist.Add(new FsrShipData(shipid,shipqty,pn,pndesc,family,cfg
                                    ,shipdate,customernum,customer1,customer2,ordereddate, delievenum));

                            }//end if
                        }//end if
                    } catch (Exception ex) { }
                }//end foreach

                var pn_mpn_dict = PN2MPn(pnsndict);
                var pn_vtype_dict = PN2VType(pn_mpn_dict);
                foreach (var item in shipdatalist)
                {
                    if (pn_vtype_dict.ContainsKey(item.PN))
                    {
                        item.VcselType = pn_vtype_dict[item.PN];
                    }
                }

                foreach (var item in shipdatalist)
                {
                    item.StoreShipData();
                }
            }//end if
        }
        #endregion
    }
}