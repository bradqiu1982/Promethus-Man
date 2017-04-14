using Prometheus.Controllers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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


            var idx = 0;
            foreach (var line in data)
            {
                if (idx != 0)
                {
                    try
                    {
                        var rmarawdatas = SplitRMAData(line); //split rma record with sn
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
                            StormRMAAttachs(rmacloseattachfolder, rmarawdatas[0].AppV_B, rmaattaches,ctrl); //retrieve rma attach and store them
                            solvedrmanum.Add(rmarawdatas[0].AppV_B,true);
                        }
                    }
                    catch (Exception ex) { }
                }//end if
                idx = idx + 1;
            }//foreach
        }

        private static void StormRMAAttachs(string rmacloseattachfolder,string RMANum, Dictionary<string, Dictionary<string, bool>> rmaattaches,Controller ctrl)
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
                            StormRMAAttach(RMANum, url);
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
                                StormRMAAttach(RMANum, url);
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
            return sb.ToString();
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

            UserController.RegisterUserAuto(vm.Assignee);

            SendRMAEvent(vm, "created",ctrl, true);
        }

        private static bool RMADataExist(RMARAWData rmadata)
        {
            var sql = "select AppV_B from RMABackupData where AppV_B='<RMANUM>' and AppV_I='<SN>'";
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

        private static void StormRMAAttach(string RMANum, string attach)
        {
            var sql = "insert into RMAMapData(AppV_A,AppV_B,AppV_C) values(N'<AppV_A>',N'<AppV_B>',N'<AppV_C>')";
            sql = sql.Replace("<AppV_A>", RMANum).Replace("<AppV_B>", attach).Replace("<AppV_C>", RMAMAPDATATYPE.ATTACH);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

    }
}