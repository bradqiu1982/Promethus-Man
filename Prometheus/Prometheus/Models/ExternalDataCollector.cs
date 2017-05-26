using Prometheus.Controllers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Prometheus.Models
{

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

    public class RELSubIssueType
    {
        public static string CONTAINMENTACTION = "[CONTAINMENTACTION]";
        public static string CORRECTIVEACTION = "[CORRECTIVEACTION]";
        public static string FAILVERIFYACTION = "[FAILVERIFYACTION]";
        public static string VERIFYCORRECTIVEACTION = "[VERIFYCORRECTIVEACTION]";
    }

    public class RMASubIssueType
    {
        public static string CONTAINMENTACTION = "[Containment]";
        public static string CORRECTIVEACTION = "[Corrective]";
    }

    public class LYTTASKType
    {
        public static string LYTTASK = "[LYT TASK]";
        public static string LYTSUBTASK = "[LYT SUB TASK]";
        public static string CONTAINMENTACTION = "[Containment]";
        public static string CORRECTIVEACTION = "[Corrective]";
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
        }

        public string DMR_ID{ set; get; }
        public string CreateTime { set; get; }
        public string Product { set; get; }
        public string TotalQty { set; get; }
        public string DefectQty { set; get; }
        public string Summary { set; get; }
        public string ModuleSN { set; get; }
        public string Owner { set; get; }
    }

    public class ExternalDataCollector
    {

        #region FILEOPERATE

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
                return null;
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

        private static void logthdinfo(string info)
        {
            try
            {
                var filename = "d:\\log\\rmabackuptrace-" + DateTime.Now.ToString("yyyy-MM-dd");
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

            var usermatrix = UserMatrixVM.RetrieveUserMatrixDepart(); //user depart

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
                            Try2CreateRMA(rawdata, usermatrix, rmaissuedict, allpjdict,ctrl);
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

        private static void Try2CreateRMA(RMARAWData rawdata,Dictionary<string,string> usermatrix
            , Dictionary<string, string> rmaissuedict, Dictionary<string, bool> allpjdict,Controller ctrl)
        {
            var analyser = rawdata.AppV_O.ToUpper();
            if(!rawdata.AppV_O.Contains("@"))
                analyser = (rawdata.AppV_O.Replace(" ", ".") + "@FINISAR.COM").ToUpper();

            if (usermatrix.ContainsKey(analyser))
            {
                if (string.Compare(usermatrix[analyser], USERDEPART.NPI, true) == 0)
                {
                    var pjk = RMSpectialCh(rawdata.AppV_N);
                    if (allpjdict.ContainsKey(pjk))
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
                                    CreateRMAIssue(rawdata,ctrl);
                                }
                            }
                            else
                            {
                                rmaissuedict.Add(rawdata.AppV_B, rawdata.AppV_I);
                                //create RMA issue
                                CreateRMAIssue(rawdata,ctrl);
                            }//check whether issue exist
                        }//issue open in one week
                    }// Product type is exist in system
                }//analyse is in NPI Department
            }//analyser in usermatrisx
        }

        private static void CreateRMASubIssue(string presum, string sum, string pjkey, string parentkey, string analyser, string reporter, DateTime duedate)
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
        }

        private static void CreateRMAIssue(RMARAWData rawdata,Controller ctrl)
        {
            var vm = new IssueViewModels();
            vm.ProjectKey = RMSpectialCh(rawdata.AppV_N);
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
            vm.Assignee = (rawdata.AppV_O.Replace(" ", ".") + "@FINISAR.COM").ToUpper();
            if (vm.Assignee.Length > 200)
            {
                vm.Assignee = vm.Assignee.Substring(0, 198);
            }

            vm.Reporter = (rawdata.AppV_AG.Replace(" ", ".") + "@FINISAR.COM").ToUpper(); ;
            if (vm.Reporter.Length > 200)
            {
                vm.Reporter = vm.Reporter.Substring(0, 198);
            }

            vm.Resolution = Resolute.Pending;

            vm.ResolvedDate = DateTime.Parse("1982-05-06 01:01:01");

            vm.RMAFailureCode = "";
            vm.FVCode = "";
            vm.Description = "RMA " + vm.FinisarRMA + " for module " + vm.ModuleSN + " " + vm.FinisarModel + " from " + vm.ECustomer + ":" + rawdata.AppV_L;
            vm.CommentType = COMMENTTYPE.Description;

            vm.StoreIssue();

            UserViewModels.RegisterUserAuto(vm.Assignee);

            CreateRMASubIssue(RMASubIssueType.CONTAINMENTACTION, "Cotainment Action for RMA " + vm.FinisarRMA, vm.ProjectKey, vm.IssueKey, vm.Assignee, vm.Reporter, vm.DueDate.AddDays(18));
            CreateRMASubIssue(RMASubIssueType.CORRECTIVEACTION, "Corrective Action for RMA " + vm.FinisarRMA, vm.ProjectKey, vm.IssueKey, vm.Assignee, vm.Reporter, vm.DueDate.AddDays(48));
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
                    + ",AppV_Y,AppV_Z,AppV_AA,AppV_AB,AppV_AC,AppV_AD,AppV_AE,AppV_AF,AppV_AG)"
                    + " values(N'<AppV_A>',N'<AppV_B>',N'<AppV_C>',N'<AppV_D>',N'<AppV_E>',N'<AppV_F>'"
                    + ",N'<AppV_G>',N'<AppV_H>',N'<AppV_I>',N'<AppV_J>',N'<AppV_K>',N'<AppV_L>',N'<AppV_M>',N'<AppV_N>',N'<AppV_O>'"
                    + ",'<AppV_P>','<AppV_Q>','<AppV_R>','<AppV_S>','<AppV_T>',N'<AppV_U>','<AppV_V>','<AppV_W>',N'<AppV_X>'"
                    + ",N'<AppV_Y>',N'<AppV_Z>',N'<AppV_AA>',N'<AppV_AB>','<AppV_AC>',N'<AppV_AD>',N'<AppV_AE>',N'<AppV_AF>',N'<AppV_AG>')";
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
                .Replace("<AppV_AE>", rmadata.AppV_AE).Replace("<AppV_AF>", rmadata.AppV_AF).Replace("<AppV_AG>", rmadata.AppV_AG);

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
                EmailUtility.SendEmail(ctrl, "WUXI NPI System_"+reporter, toaddrs, content);
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
            var sql = "insert into RMAMapData(AppV_A,AppV_B,AppV_C) values(N'<AppV_A>',N'<AppV_B>',N'<AppV_C>')";
            sql = sql.Replace("<AppV_A>", RMANum).Replace("<AppV_B>", attach).Replace("<AppV_C>", RMAMAPDATATYPE.ATTACH);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        #endregion

        #region NEOMAP

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

                var idx = 0;
                foreach (var line in data)
                {
                    if (idx != 0)
                    {
                        var neodata = OfferNEOData(line);
                        if (string.Compare(neodata.AppV_G, bin) == 0)
                        {
                            neodata.AppV_AD = 1;
                        }
                        else
                        {
                            neodata.AppV_AD = 0;
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
                    try
                    {
                        for (int i = 1; i < dt.Columns.Count; i++)
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

        //private static bool NEOMAPDataExist(NEOMAPData neomapdata)
        //{
        //    var sql = "select AppV_A from NeoMapData where AppV_A='<AppV_A>' and AppV_C='<AppV_C>' and AppV_D='<AppV_D>' and AppV_E='<AppV_E>'";
        //    sql = sql.Replace("<AppV_A>", neomapdata.AppV_A).Replace("<AppV_C>", neomapdata.AppV_C)
        //        .Replace("<AppV_D>", neomapdata.AppV_D).Replace("<AppV_E>", neomapdata.AppV_E);

        //    var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
        //    if (dbret.Count > 0)
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        //private static void UpdateNeoMapData(List<string> rawdata)
        //{
        //    var neodata = OfferNEOData(rawdata);
        //    if (!NEOMAPDataExist(neodata))
        //    {
        //        var sql = "insert into NeoMapData(AppV_A,AppV_B,AppV_C,AppV_D,AppV_E,AppV_F"
        //            + ",AppV_G,AppV_H,AppV_I,AppV_J,AppV_K,AppV_L,AppV_M,AppV_N,AppV_O"
        //            + ",AppV_P,AppV_Q,AppV_R,AppV_S,AppV_T,AppV_U,AppV_V,AppV_W,AppV_X"
        //            + ",AppV_Y,AppV_Z,AppV_AA,AppV_AB,AppV_AC)"
        //            + " values(N'<AppV_A>',N'<AppV_B>',N'<AppV_C>',N'<AppV_D>',N'<AppV_E>',<AppV_F>"
        //            + ",N'<AppV_G>',<AppV_H>,<AppV_I>,<AppV_J>,<AppV_K>,<AppV_L>,<AppV_M>,<AppV_N>,<AppV_O>"
        //            + ",<AppV_P>,<AppV_Q>,<AppV_R>,<AppV_S>,<AppV_T>,<AppV_U>,<AppV_V>,<AppV_W>,<AppV_X>"
        //            + ",<AppV_Y>,<AppV_Z>,<AppV_AA>,<AppV_AB>,<AppV_AC>)";


        //        sql = sql.Replace("<AppV_A>", neodata.AppV_A).Replace("<AppV_B>", neodata.AppV_B).Replace("<AppV_C>", neodata.AppV_C)
        //            .Replace("<AppV_D>", neodata.AppV_D).Replace("<AppV_E>", neodata.AppV_E).Replace("<AppV_F>", neodata.AppV_F.ToString())
        //            .Replace("<AppV_G>", neodata.AppV_G).Replace("<AppV_H>", neodata.AppV_H.ToString()).Replace("<AppV_I>", neodata.AppV_I.ToString())
        //            .Replace("<AppV_J>", neodata.AppV_J.ToString()).Replace("<AppV_K>", neodata.AppV_K.ToString()).Replace("<AppV_L>", neodata.AppV_L.ToString())
        //            .Replace("<AppV_M>", neodata.AppV_M.ToString()).Replace("<AppV_N>", neodata.AppV_N.ToString()).Replace("<AppV_O>", neodata.AppV_O.ToString())
        //            .Replace("<AppV_P>", neodata.AppV_P.ToString()).Replace("<AppV_Q>", neodata.AppV_Q.ToString()).Replace("<AppV_R>", neodata.AppV_R.ToString())
        //            .Replace("<AppV_S>", neodata.AppV_S.ToString()).Replace("<AppV_T>", neodata.AppV_T.ToString()).Replace("<AppV_U>", neodata.AppV_U.ToString())
        //            .Replace("<AppV_V>", neodata.AppV_V.ToString()).Replace("<AppV_W>", neodata.AppV_W.ToString()).Replace("<AppV_X>", neodata.AppV_X.ToString())
        //            .Replace("<AppV_Y>", neodata.AppV_Y.ToString()).Replace("<AppV_Z>", neodata.AppV_Z.ToString()).Replace("<AppV_AA>", neodata.AppV_AA.ToString())
        //            .Replace("<AppV_AB>", neodata.AppV_AB.ToString()).Replace("<AppV_AC>", neodata.AppV_AC.ToString());

        //        DBUtility.ExeLocalSqlNoRes(sql);
        //    }
        //}
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
                var uniquekey = issue.ReportDate.ToString("yyyy-MM-dd") + "-" + issue.TestType + "-" + issue.ModuleSN;
                if (!rmaissuedict.ContainsKey(uniquekey))
                {
                    rmaissuedict.Add(uniquekey, true);
                }
            }//end foreach

            var pjkeys = ProjectViewModels.RetrieveAllProjectKey(); //all pjkey
            var allpjdict = new Dictionary<string, bool>();
            foreach (var pjk in pjkeys)
            {
                allpjdict.Add(pjk, true);
            }

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

                        UpdateRELData(rawdata);

                        //if (allpjdict.ContainsKey(RELPJKEY))
                        //{
                            Try2CreateREL(RELPJKEY,rawdata, rmaissuedict, ctrl);
                        //}

                        var uniquekey = DateTime.Parse(rawdata.AppV_C).ToString("yyyy-MM-dd") + "-" + rawdata.AppV_I + "-" + rawdata.AppV_K;
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

        private static bool RELDataExist(RELRAWData rmadata)
        {
            var sql = "select AppV_A from RELBackupData where AppV_C=N'<AppV_C>' and AppV_I=N'<AppV_I>' and AppV_K=N'<AppV_K>'";
            sql = sql.Replace("<AppV_C>", rmadata.AppV_C).Replace("<AppV_I>", rmadata.AppV_I).Replace("<AppV_K>", rmadata.AppV_K);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            if (dbret.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static void UpdateRELData(RELRAWData rmadata)
        {
            var sql = "";
            if (RELDataExist(rmadata))
            {
                sql = "update RELBackupData set AppV_B = N'<AppV_B>',AppV_D = N'<AppV_D>'"
                    + ",AppV_E = N'<AppV_E>',AppV_F = N'<AppV_F>',AppV_G = N'<AppV_G>',AppV_H = N'<AppV_H>'"
                    + ",AppV_J = N'<AppV_J>',AppV_L = N'<AppV_L>',AppV_M = N'<AppV_M>',AppV_N = N'<AppV_N>'"
                    + ",AppV_O = N'<AppV_O>',AppV_P = '<AppV_P>',AppV_Q = '<AppV_Q>',AppV_R = '<AppV_R>',AppV_S = '<AppV_S>'"
                    + ",AppV_T = '<AppV_T>',AppV_U = N'<AppV_U>',AppV_V = '<AppV_V>',AppV_W = '<AppV_W>',AppV_X = N'<AppV_X>'"
                    + ",AppV_Y = N'<AppV_Y>',AppV_Z = N'<AppV_Z>',AppV_AA = N'<AppV_AA>',AppV_AB = N'<AppV_AB>',AppV_AC = '<AppV_AC>'"
                    + ",AppV_AD = N'<AppV_AD>',AppV_AE = N'<AppV_AE>',AppV_AF = N'<AppV_AF>',AppV_AG = N'<AppV_AG>'  where AppV_C=N'<AppV_C>' and AppV_I=N'<AppV_I>' and AppV_K=N'<AppV_K>'";
            }
            else
            {
                sql = "insert into RELBackupData(AppV_A,AppV_B,AppV_C,AppV_D,AppV_E,AppV_F"
                    + ",AppV_G,AppV_H,AppV_I,AppV_J,AppV_K,AppV_L,AppV_M,AppV_N,AppV_O"
                    + ",AppV_P,AppV_Q,AppV_R,AppV_S,AppV_T,AppV_U,AppV_V,AppV_W,AppV_X"
                    + ",AppV_Y,AppV_Z,AppV_AA,AppV_AB,AppV_AC,AppV_AD,AppV_AE,AppV_AF,AppV_AG)"
                    + " values(<AppV_A>,N'<AppV_B>',N'<AppV_C>',N'<AppV_D>',N'<AppV_E>',N'<AppV_F>'"
                    + ",N'<AppV_G>',N'<AppV_H>',N'<AppV_I>',N'<AppV_J>',N'<AppV_K>',N'<AppV_L>',N'<AppV_M>',N'<AppV_N>',N'<AppV_O>'"
                    + ",'<AppV_P>','<AppV_Q>','<AppV_R>','<AppV_S>','<AppV_T>',N'<AppV_U>','<AppV_V>','<AppV_W>',N'<AppV_X>'"
                    + ",N'<AppV_Y>',N'<AppV_Z>',N'<AppV_AA>',N'<AppV_AB>','<AppV_AC>',N'<AppV_AD>',N'<AppV_AE>',N'<AppV_AF>',N'<AppV_AG>')";
            }

            sql = sql.Replace("<AppV_A>", rmadata.AppV_A.ToString()).Replace("<AppV_B>", rmadata.AppV_B).Replace("<AppV_C>", rmadata.AppV_C)
                .Replace("<AppV_D>", rmadata.AppV_D).Replace("<AppV_E>", rmadata.AppV_E).Replace("<AppV_F>", rmadata.AppV_F)
                .Replace("<AppV_G>", rmadata.AppV_G).Replace("<AppV_H>", rmadata.AppV_H).Replace("<AppV_I>", rmadata.AppV_I)
                .Replace("<AppV_J>", rmadata.AppV_J).Replace("<AppV_K>", rmadata.AppV_K).Replace("<AppV_L>", rmadata.AppV_L)
                .Replace("<AppV_M>", rmadata.AppV_M).Replace("<AppV_N>", rmadata.AppV_N).Replace("<AppV_O>", rmadata.AppV_O)
                .Replace("<AppV_P>", rmadata.AppV_P).Replace("<AppV_Q>", rmadata.AppV_Q).Replace("<AppV_R>", rmadata.AppV_R)
                .Replace("<AppV_S>", rmadata.AppV_S).Replace("<AppV_T>", rmadata.AppV_T).Replace("<AppV_U>", rmadata.AppV_U)
                .Replace("<AppV_V>", rmadata.AppV_V).Replace("<AppV_W>", rmadata.AppV_W).Replace("<AppV_X>", rmadata.AppV_X)
                .Replace("<AppV_Y>", rmadata.AppV_Y).Replace("<AppV_Z>", rmadata.AppV_Z).Replace("<AppV_AA>", rmadata.AppV_AA)
                .Replace("<AppV_AB>", rmadata.AppV_AB).Replace("<AppV_AC>", rmadata.AppV_AC).Replace("<AppV_AD>", rmadata.AppV_AD)
                .Replace("<AppV_AE>", rmadata.AppV_AE).Replace("<AppV_AF>", rmadata.AppV_AF).Replace("<AppV_AG>", rmadata.AppV_AG);

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
                if (!rawdata.AppV_N.Contains("@"))
                    reporter = (rawdata.AppV_O.Replace(" ", ".") + "@FINISAR.COM").ToUpper();
                UserViewModels.RegisterUserAuto(analyser);
                UserViewModels.RegisterUserAuto(reporter);

                var uniquekey = DateTime.Parse(rawdata.AppV_C).ToString("yyyy-MM-dd") + "-" + rawdata.AppV_I + "-" + rawdata.AppV_K;
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

            CreateRelSubIssue(RELSubIssueType.FAILVERIFYACTION, "Failure Verify for CaseID " + vm.CaseID, RELPJKEY, vm.IssueKey, analyser, reporter, DateTime.Parse(rawdata.AppV_C).AddDays(2));
            CreateRelSubIssue(RELSubIssueType.CONTAINMENTACTION, "Cotainment Action for CaseID " + vm.CaseID, RELPJKEY, vm.IssueKey, analyser, reporter, DateTime.Parse(rawdata.AppV_C).AddDays(30));
            CreateRelSubIssue(RELSubIssueType.CORRECTIVEACTION, "Corrective/PreVentive Action for CaseID " + vm.CaseID, RELPJKEY, vm.IssueKey, analyser, reporter, DateTime.Parse(rawdata.AppV_C).AddDays(60));
            CreateRelSubIssue(RELSubIssueType.VERIFYCORRECTIVEACTION, "Verify Corrective/PreVentive Action for CaseID " + vm.CaseID, RELPJKEY, vm.IssueKey, reporter, reporter, DateTime.Parse(rawdata.AppV_C).AddDays(75));

            var comment = new IssueComments();
            comment.Comment = "ROOTCAUSE: to be edited";
            IssueViewModels.StoreIssueComment(vm.IssueKey, comment.dbComment, analyser, COMMENTTYPE.RootCause);

            if (!IsDebug())
            {
                SendRMAEvent(vm, "created", ctrl, true);
            }

        }

        private static void CreateRelSubIssue(string presum, string sum, string pjkey, string parentkey, string analyser, string reporter, DateTime duedate)
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
            var sql = "insert into RELMapData(AppV_A,AppV_B,AppV_C,AppV_D) values(N'<AppV_A>',N'<AppV_B>',N'<AppV_C>',N'<AppV_D>')";
            sql = sql.Replace("<AppV_A>", combinekey).Replace("<AppV_B>", CaseID).Replace("<AppV_C>", attach).Replace("<AppV_D>", RMAMAPDATATYPE.ATTACH);
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
                    OBAUpdateTime = DateTime.Parse("2017-05-23 01:00:00");
                }
            }

            return OBAUpdateTime.ToString();
        }

        private static Dictionary<string, bool> RetrieveExistDMRDict(Controller ctrl)
        {
            var ret = new Dictionary<string, bool>();
            var obalist = IssueViewModels.RetrieveAllIssueTypeIssue("NONE", "NONE", ISSUETP.OBA, ctrl);
            foreach (var item in obalist)
            {
                if (!string.IsNullOrEmpty(item.FinisarDMR.Trim()) && !ret.ContainsKey(item.FinisarDMR.Trim()))
                {
                    ret.Add(item.FinisarDMR.Trim(), true);
                }
            }
            return ret;
        }

        private static List<RawDMR> RetrieveDMRFromITDB(string OBAUpdateTime, Dictionary<string, bool> DMRDict)
        {
            var ret = new List<RawDMR>();

            var sql = "select DMR_ID,Created_at,Prod_Line,Defect_Qty,Inspected_Qty,Actual_Problem from dbo.DMR_Detail_List_View where Created_at > '<CreateTime>'";
            sql = sql.Replace("<CreateTime>", OBAUpdateTime);
            var dbret = DBUtility.ExeFAISqlWithRes(sql);

            foreach (var line in dbret)
            {
                var dmrid = Convert.ToString(line[0]).Trim();
                if (!string.IsNullOrEmpty(dmrid) && !DMRDict.ContainsKey(dmrid))
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
                        var splitstr = tempdmr.Summary.Substring(idx + 3).Split(new string[] { " "},StringSplitOptions.RemoveEmptyEntries);
                        tempdmr.ModuleSN = splitstr[0];
                    }
                    else if (tempdmr.Summary.Contains("SN :"))
                    {
                        var idx = tempdmr.Summary.IndexOf("SN :");
                        var splitstr = tempdmr.Summary.Substring(idx + 4).Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                        tempdmr.ModuleSN = splitstr[0];
                    }
                    ret.Add(tempdmr);
                }//end if
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

            UserViewModels.RegisterUserAuto(vm.Assignee);
            SendOBAEvent(vm, "created",ctrl, true);

            CreateRMASubIssue(RMASubIssueType.CONTAINMENTACTION, "Cotainment Action for OBA " + vm.FinisarDMR, vm.ProjectKey, vm.IssueKey, vm.Assignee, vm.Reporter, vm.DueDate.AddDays(14));
            CreateRMASubIssue(RMASubIssueType.CORRECTIVEACTION, "Corrective Action for OBA " + vm.FinisarDMR, vm.ProjectKey, vm.IssueKey, vm.Assignee, vm.Reporter, vm.DueDate.AddDays(28));

            var comment = new IssueComments();
            comment.Comment = "ROOTCAUSE: to be edited";
            IssueViewModels.StoreIssueComment(vm.IssueKey, comment.dbComment, vm.Assignee, COMMENTTYPE.RootCause);
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

                var netcomputername = "";
                try { netcomputername = System.Net.Dns.GetHostName(); }
                catch (Exception ex) { }
                validatestr = validatestr.Replace("//localhost", "//" + netcomputername);

                var content = vm.Summary + " is " + operate + " by " + vm.Reporter + " :\r\n " + validatestr;

                var toaddrs = new List<string>();
                toaddrs.Add(vm.Assignee);
                toaddrs.Add(vm.Reporter);
                EmailUtility.SendEmail(ctrl, "WUXI NPI System", toaddrs, content);
                new System.Threading.ManualResetEvent(false).WaitOne(200);
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
            var DMRDict = RetrieveExistDMRDict(ctrl);
            var dmrlist = RetrieveDMRFromITDB(OBAUpdateTime, DMRDict);
            foreach (var dmr in dmrlist)
            {
                CreateOBATask(dmr, OBAAdmin, OBADefaultPJ, OBARelateDict,ctrl);
            }
        }

        #endregion
    }
}