using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace Prometheus.Models
{
    public class TXNTYPENAME
    {
        public static int MoveStd = 0;
        public static int MoveNonStd = 1;
        public static int Other = 2;
    }

    public class ProjectWorkflow
    {
        public ProjectWorkflow()
        {
            Init();
        }

        private void Init()
        {
            ProjectKey = string.Empty;
            ProductId = string.Empty;
            WorkflowStepID = string.Empty;
            WorkflowStepName = string.Empty;
            WorkflowID = string.Empty;
            Sequence = 0;
            Appv_1 = string.Empty;
            Appv_2 = string.Empty;
            Appv_3 = string.Empty;
            Appv_4 = DateTime.Parse("1982-05-06 10:00:00");
            databackuptm = DateTime.Now;
        }

        public string ProjectKey { set; get; }
        public string ProductId { set; get; }
        public string WorkflowStepID { set; get; }
        public string WorkflowStepName { set; get; }
        public string WorkflowID { set; get; }
        public int Sequence { set; get; }
        public string Appv_1 { set; get; }
        public string Appv_2 { set; get; }
        public string Appv_3 { set; get; }
        public DateTime Appv_4 { set; get; }
        public DateTime databackuptm { set; get; }
    }

    public class ProjectMoveHistory
    {
        public ProjectMoveHistory()
        {
            Init();
        }

        private void Init()
        {
            ProjectKey = string.Empty;
            ProductId = string.Empty;
            WorkflowStepID = string.Empty;
            WorkflowStepName = string.Empty;
            Sequence = 0;

            MoveOutQty = 0;
            MoveInQty = 1;

            MfgOrderId = string.Empty;
            MfgOrderName = string.Empty;
            
            MoveOutTime = DateTime.Parse("1982-05-06 10:00:00");
            TxnTypeName = -1;
            ContainerName = string.Empty;
            Comments = string.Empty;

            Appv_1 = string.Empty;
            Appv_2 = string.Empty;
            Appv_3 = string.Empty;
            Appv_4 = DateTime.Parse("1982-05-06 10:00:00");
            databackuptm = DateTime.Now;
        }

        public string ProjectKey { set; get; }
        public string ProductId { set; get; }
        public string WorkflowStepID { set; get; }
        public string WorkflowStepName { set; get; }
        public int Sequence { set; get; }

        public int MoveOutQty { set; get; }
        public int MoveInQty { set; get; }

        public string MfgOrderId { set; get; }
        public string MfgOrderName { set; get; }

        public DateTime MoveOutTime { set; get; }
        public int TxnTypeName { set; get; }
        public string ContainerName { set; get; }
        public string Comments { set; get; }

        public string Appv_1 { set; get; }
        public string Appv_2 { set; get; }
        public string Appv_3 { set; get; }
        public DateTime Appv_4 { set; get; }
        public DateTime databackuptm { set; get; }
    }

    public class ProcessData
    {
        private static bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        private static string PNCondition(List<ProjectPn> pns)
        {
            var cond = string.Empty;
            foreach (var pn in pns)
            {
                if (!IsDigitsOnly(pn.Pn.Trim()))
                {
                    if (string.IsNullOrEmpty(cond))
                    {
                        cond = " p.Description like  '%" + pn.Pn.Trim() + "%' ";
                    }
                    else
                    {
                        cond = cond + " or p.Description like '%" + pn.Pn.Trim() + "%' ";
                    }
                }
            }

            return cond;
        }

        private static string ConvertString(object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }

            try
            {
                return Convert.ToString(obj);
            }
            catch (Exception ex) {
                return string.Empty;
            }
        }

        public static void LoadMesWorkflow(string PJKey,Controller ctrl)
        {
            var pvmlist = ProjectViewModels.RetrieveOneProject(PJKey);
            if (pvmlist.Count == 0) { return; }
            var pvm = pvmlist[0];

            var pncond = PNCondition(pvm.PNList);
            if (!string.IsNullOrEmpty(pncond))
            {
                var workflowlist = new List<ProjectWorkflow>();
                var productiddict = new Dictionary<string, bool>();

                var sql = "select p.ProductId,s.WorkflowStepID, s.WorkflowStepName,s.WorkflowID,s.Sequence from InsiteDB.insite.WorkflowStep s (nolock) " 
                    + " left join InsiteDB.insite.Workflow w (nolock) on s.WorkflowID = w.WorkflowID"
                    + " left join InsiteDB.insite.Product p (nolock) on w.WorkflowBaseId = p.WorkflowBaseId"
                    + " where (<pncond>) "
                    + " and p.WorkflowBaseId is not null and p.WorkflowBaseId <> '0000000000000000' ";
                sql = sql.Replace("<pncond>", pncond);
                var dbret = DBUtility.ExeMESSqlWithRes(sql);

                if (dbret.Count == 0)
                {
                    sql = "select p.ProductId,s.WorkflowStepID, s.WorkflowStepName,s.WorkflowID,s.Sequence from InsiteDB.insite.WorkflowStep s (nolock) "
                    + " left join InsiteDB.insite.Product p (nolock) on s.WorkflowID = p.WorkflowID"
                    + " where (<pncond>) "
                    + " and p.WorkflowId is not null and p.WorkflowId <> '0000000000000000' ";
                    sql = sql.Replace("<pncond>", pncond);
                    dbret = DBUtility.ExeMESSqlWithRes(sql);
                }

                foreach (var line in dbret)
                {
                    try
                    {
                        var tempflow = new ProjectWorkflow();
                        tempflow.ProjectKey = PJKey;
                        tempflow.ProductId = ConvertString(line[0]);
                        tempflow.WorkflowStepID = ConvertString(line[1]);
                        tempflow.WorkflowStepName = ConvertString(line[2]);
                        tempflow.WorkflowID = ConvertString(line[3]);
                        tempflow.Sequence = Convert.ToInt32(line[4]);
                        workflowlist.Add(tempflow);

                        if (!productiddict.ContainsKey(tempflow.ProductId))
                        {
                            productiddict.Add(tempflow.ProductId, true);
                        }
                    }
                    catch (Exception ex)
                    { }
                }//end foreach

                var productids = productiddict.Keys.ToList();
                foreach (var pid in productids)
                {
                    var csql = "delete from ProjectWorkflow where ProductId = '<ProductId>'";
                    csql = csql.Replace("<ProductId>", pid);
                    DBUtility.ExeLocalSqlNoRes(csql);
                }

                foreach (var flow in workflowlist)
                {
                    var csql = "insert into ProjectWorkflow(ProjectKey,ProductId,WorkflowStepID,WorkflowStepName,WorkflowID,Sequence,databackuptm) " 
                        + " values('<ProjectKey>','<ProductId>','<WorkflowStepID>','<WorkflowStepName>','<WorkflowID>',<Sequence>,'<databackuptm>')";
                    csql = csql.Replace("<ProjectKey>",flow.ProjectKey).Replace("<ProductId>",flow.ProductId).Replace("<WorkflowStepID>",flow.WorkflowStepID)
                        .Replace("<WorkflowStepName>",flow.WorkflowStepName).Replace("<WorkflowID>",flow.WorkflowID).Replace("<Sequence>",flow.Sequence.ToString())
                        .Replace("<databackuptm>",DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    DBUtility.ExeLocalSqlNoRes(csql);
                }
            }//end if
        }

        public static List<string> GetCurrentProjectWorkflowSteps(string PJKey)
        {
            var ret = new List<string>();

            var sql = "select distinct WorkflowStepName from ProjectWorkflow where ProjectKey = '<ProjectKey>' order by WorkflowStepName";
            sql = sql.Replace("<ProjectKey>", PJKey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                try
                {
                    ret.Add(Convert.ToString(line[0]));
                }
                catch (Exception ex) { }
            }

            return ret;
        }

        public static List<ProjectWorkflow> RetriveWorkflowByName(string PJKey,List<string> WorkflowStepNames)
        {
            var ret = new List<ProjectWorkflow>();
            var sql = string.Empty;

            if (WorkflowStepNames.Count > 0)
            {
                var namecond = "'";
                foreach (var item in WorkflowStepNames)
                {
                    namecond = namecond + item + "','";
                }
                namecond = namecond.Substring(0, namecond.Length - 2);

                sql = "select ProductId,WorkflowStepID,WorkflowStepName,Sequence from ProjectWorkflow where WorkflowStepName in (<stepnamecond>) and ProjectKey = '<ProjectKey>'";
                sql = sql.Replace("<ProjectKey>", PJKey).Replace("<stepnamecond>", namecond);
            }
            else
            {
                sql = "select ProductId,WorkflowStepID,WorkflowStepName,Sequence from ProjectWorkflow where ProjectKey = '<ProjectKey>'";
                sql = sql.Replace("<ProjectKey>", PJKey);
            }

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                try
                {
                    var tempflow = new ProjectWorkflow();
                    tempflow.ProjectKey = PJKey;
                    tempflow.ProductId = ConvertString(line[0]);
                    tempflow.WorkflowStepID = ConvertString(line[1]);
                    tempflow.WorkflowStepName = ConvertString(line[2]);
                    tempflow.Sequence = Convert.ToInt32(line[3]);
                    ret.Add(tempflow);
                }
                catch (Exception ex) { }
            }//end foreach

            return ret;
        }

        private static string RetriveProjectMoveHistoryLatetime(string PJKey,string movezerotime)
        {
            var sql = "select top 1 MoveOutTime from ProjectMoveHistory where ProjectKey = '<ProjectKey>' order by MoveOutTime DESC";
            sql = sql.Replace("<ProjectKey>", PJKey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            if (dbret.Count > 0)
            {
                try
                {
                    return Convert.ToDateTime(dbret[0][0]).ToString("yyyy-MM-dd HH:mm:ss");
                }
                catch (Exception ex) { return movezerotime; }
            }
            return movezerotime;
        }

        private static void RealMfgOrderName(List<string> mfgidlist, Dictionary<string, string> MfgOrderDict)
        {
            var mfgcond = "'";
            foreach (var item in mfgidlist)
            {
                mfgcond = mfgcond + item + "','";
            }
            mfgcond = mfgcond.Substring(0, mfgcond.Length - 2);

            var sql = "select MfgOrderId,MfgOrderName from InsiteDB.insite.MfgOrder (nolock) where MfgOrderId in (<mfgcond>)";
            sql = sql.Replace("<mfgcond>", mfgcond);
            var dbret = DBUtility.ExeMESSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var id = ConvertString(line[0]);
                var name = ConvertString(line[1]);
                if (!string.IsNullOrEmpty(id) 
                    && !string.IsNullOrEmpty(name))
                {
                    if (!MfgOrderDict.ContainsKey(id))
                    {
                        MfgOrderDict.Add(id, name);
                    }
                }//end if
            }//end foreach
        }

        private static void RetrieveMfgOrderName(List<string> mfgidlist,Dictionary<string, string> MfgOrderDict)
        {
            var snlist = mfgidlist;
            var startidx = 0;
            var totalsn = snlist.Count;

            while (true)
            {
                var done = false;
                var tempsnlist = new List<string>();
                for (var idx = startidx; idx < startidx + 20000; idx++)
                {
                    if (idx < totalsn)
                    {
                        tempsnlist.Add(snlist[idx]);
                    }
                    else
                    {
                        done = true;
                        break;
                    }
                }

                if (tempsnlist.Count > 0)
                {
                    RealMfgOrderName(tempsnlist, MfgOrderDict);
                }

                if (done)
                    break;

                startidx = startidx + 20000;
            }//end while(true)

        }

        private static List<string> RetrieveAllProductIDs(string PJKey)
        {
            var ret = new List<string>();
            var pvmlist = ProjectViewModels.RetrieveOneProject(PJKey);
            if (pvmlist.Count == 0) { return ret; }
            var pvm = pvmlist[0];
            var pncond = PNCondition(pvm.PNList);
            if (!string.IsNullOrEmpty(pncond))
            {
                var pnkey = "id_" + pncond.Replace("p.Description like", "").Replace("or", ",").Replace("'", "").Replace("%", "").Replace(" ", "").Replace(" ", "");
                var pnlist = PNDescCache.RetrievePNs(pnkey);
                if (pnlist.Count > 0)
                {
                    return pnlist;
                }

                var sql = "select distinct p.ProductId from InsiteDB.insite.Product p (nolock) where (<pncond>) ";
                sql = sql.Replace("<pncond>", pncond);
                var dbret = DBUtility.ExeMESSqlWithRes(sql);
                foreach (var line in dbret)
                {
                    var pid = ConvertString(line[0]);
                    if (!string.IsNullOrEmpty(pid))
                    {
                        ret.Add(pid);
                        PNDescCache.StorePN(pnkey, pid);
                    }
                }//end foreach
            }//end if

            return ret;
        }

        private static void _loadMESMoveHistory(string PJKey, Controller ctrl,List<string> allpids, List<ProjectWorkflow> allworkflow,string latesttime, bool isosapj)
        {

                var stepnamedict = new Dictionary<string, string>();
                foreach (var item in allworkflow)
                {
                    if (!stepnamedict.ContainsKey(item.WorkflowStepID))
                    {
                        stepnamedict.Add(item.WorkflowStepID, item.WorkflowStepName);
                    }
                }

                var stepseqdict = new Dictionary<string, int>();
                foreach (var item in allworkflow)
                {
                    if (!stepseqdict.ContainsKey(item.WorkflowStepID))
                    {
                        stepseqdict.Add(item.WorkflowStepID, item.Sequence);
                    }
                }

                var pidcond = "'";
                foreach (var item in allpids)
                {
                    pidcond = pidcond + item + "','";
                }
                pidcond = pidcond.Substring(0, pidcond.Length - 2);

                var sql = "select ProductId,WorkflowStepId,MoveOutQty,MoveInQty,MfgOrderId,MoveOutTime,TxnTypeName,ContainerName,Comments,WorkflowStepName "
                    + " from PDMSMaster.dbo.HistStepMoveSummary (nolock) where MoveOutTime > '<MoveOutTime>' and ProductId in (<pidcond>)  and MFGOrderId is not null";
                sql = sql.Replace("<pidcond>", pidcond).Replace("<MoveOutTime>", latesttime);

                var MfgOrdidDict = new Dictionary<string, bool>();
                var movelist = new List<ProjectMoveHistory>();

                var FailedSN = "'";

                var dbret = DBUtility.ExeMESReportMasterSqlWithRes(sql);
                foreach (var line in dbret)
                {
                    try
                    {
                        var tempmove = new ProjectMoveHistory();
                        tempmove.ProjectKey = PJKey;
                        tempmove.ProductId = ConvertString(line[0]);
                        tempmove.WorkflowStepID = ConvertString(line[1]);
                        tempmove.MoveOutQty = Convert.ToInt32(line[2]);
                        tempmove.MoveInQty = Convert.ToInt32(line[3]);
                        tempmove.MfgOrderId = ConvertString(line[4]);
                        tempmove.MoveOutTime = Convert.ToDateTime(line[5]);
                        tempmove.ContainerName = ConvertString(line[7]);

                        var TxnTypeName = ConvertString(line[6]);
                        if (TxnTypeName.ToUpper().Contains("MOVESTD"))
                        { tempmove.TxnTypeName = TXNTYPENAME.MoveStd; }
                        else if (TxnTypeName.ToUpper().Contains("MOVENONSTD"))
                        {
                            tempmove.TxnTypeName = TXNTYPENAME.MoveNonStd;
                            FailedSN = FailedSN + tempmove.ContainerName + "','";
                        }
                        else
                        { tempmove.TxnTypeName = TXNTYPENAME.Other; }

                        
                        tempmove.Comments = ConvertString(line[8]);
                        if (stepnamedict.ContainsKey(tempmove.WorkflowStepID))
                        {
                            tempmove.WorkflowStepName = stepnamedict[tempmove.WorkflowStepID];
                            tempmove.Sequence = stepseqdict[tempmove.WorkflowStepID];
                        }
                        else
                        {
                            tempmove.WorkflowStepName = ConvertString(line[9]);
                            tempmove.Sequence = 0;
                        }
                        movelist.Add(tempmove);

                        if (!MfgOrdidDict.ContainsKey(tempmove.MfgOrderId))
                        {
                            MfgOrdidDict.Add(tempmove.MfgOrderId, true);
                        }
                    }
                    catch (Exception ex) { }
                }//end foreach

                if (movelist.Count > 0)
                {
                    var snfailuredict = new Dictionary<string, string>();

                    var holdtime = movelist[0].MoveOutTime.AddDays(-7).ToString();
                    if (FailedSN.Length > 5)
                    {
                        FailedSN = FailedSN.Substring(0, FailedSN.Length - 2);

                        sql = "SELECT [Containername],[DefectReason] FROM [PDMS].[dbo].[HoldHistContainer] (nolock) where HoldTime > '<HoldTime>' and Containername in (<FailedSN>)";
                        sql = sql.Replace("<HoldTime>",holdtime).Replace("<FailedSN>", FailedSN);
                        dbret = DBUtility.ExeMESReportMasterSqlWithRes(sql);
                        foreach (var line in dbret)
                        {
                            try
                            {
                                if (!snfailuredict.ContainsKey(Convert.ToString(line[0])))
                                {
                                    snfailuredict.Add(Convert.ToString(line[0]), Convert.ToString(line[1]));
                                }
                            }
                            catch (Exception ex) { }
                        }//end foreach

                        foreach (var item in movelist)
                        {
                            if (item.TxnTypeName == TXNTYPENAME.MoveNonStd)
                            {
                                if (snfailuredict.ContainsKey(item.ContainerName))
                                {
                                    item.Comments = snfailuredict[item.ContainerName];
                                    if (isosapj)
                                    {
                                        if (!string.IsNullOrEmpty(item.Comments.Trim()) && !item.Comments.Trim().ToUpper().Contains("OTHER"))
                                        {
                                            var ekey = ProjectErrorViewModels.GetUniqKey();
                                            var pjerror = new ProjectErrorViewModels(PJKey, ekey,item.Comments.Trim(), "", 1);
                                            pjerror.Reporter = "System";
                                            pjerror.Description = "";
                                            pjerror.AddandUpdateProjectError();
                                        }
                                    }
                                }
                            }
                        }//end foreach
                    }//end if
                }//end if

                var orgdict = new Dictionary<string, string>();
                if (MfgOrdidDict.Count > 0)
                {
                    RetrieveMfgOrderName(MfgOrdidDict.Keys.ToList(),orgdict);
                }

                if (orgdict.Count > 0)
                {
                    foreach (var item in movelist)
                    {
                        if (orgdict.ContainsKey(item.MfgOrderId))
                        {
                            item.MfgOrderName = orgdict[item.MfgOrderId];
                        }
                    }//end foreach
                }

                StoreMoveHistory(movelist);
        }

        public static void LoadMESMoveHistory(string PJKey,Controller ctrl)
        {
            var syscfg = CfgUtility.GetSysConfig(ctrl);
            var movezerotime = syscfg["MOVEHISTORYZEROTIME"];
            var latesttime = RetriveProjectMoveHistoryLatetime(PJKey, movezerotime);

            bool isosapj = false;
            var osafailuremap = OSAFailureVM.RetrieveAllOSAFailureVM(PJKey);
            if (osafailuremap.Count > 0)
            {
                isosapj = true;
            }

            var allpids = RetrieveAllProductIDs(PJKey);
            var allworkflow = RetriveWorkflowByName(PJKey, new List<string>());
            if (allpids.Count > 0)
            {
                var temppidlistlist = new List<List<string>>();
                for (int i = 0; i < allpids.Count; i += 5)
                {
                    temppidlistlist.Add(allpids.GetRange(i, Math.Min(5, allpids.Count - i)));
                }

                foreach (var templist in temppidlistlist)
                {
                    _loadMESMoveHistory(PJKey, ctrl, templist, allworkflow, latesttime, isosapj);
                }
            }//end if
        }

        private static void StoreMoveHistory(List<ProjectMoveHistory> testresultfieldlist)
        {
            //BITestResultDataField
            var datatable = new System.Data.DataTable();
            PropertyInfo[] properties = typeof(ProjectMoveHistory).GetProperties();
            var i = 0;
            for (i = 0; i < properties.Length;)
            {
                datatable.Columns.Add(properties[i].Name, properties[i].PropertyType);
                i = i + 1;
            }

            foreach (var df in testresultfieldlist)
            {
                properties = typeof(ProjectMoveHistory).GetProperties();
                var temprow = new object[properties.Length];
                for (i = 0; i < properties.Length;)
                {
                    temprow[i] = properties[i].GetValue(df);
                    i = i + 1;
                }
                datatable.Rows.Add(temprow);
            }

            WriteDBWithTable(datatable, "ProjectMoveHistory");
        }

        private static void WriteDBWithTable(System.Data.DataTable dt, string tablename)
        {
            if (dt.Rows.Count > 0)
            {
                var targetcon = DBUtility.GetLocalConnector();
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(targetcon))
                {
                    bulkCopy.DestinationTableName = tablename;
                    bulkCopy.BulkCopyTimeout = 180;

                    try
                    {
                        for (int i = 0; i < dt.Columns.Count; i++)
                        {
                            bulkCopy.ColumnMappings.Add(dt.Columns[i].ColumnName, dt.Columns[i].ColumnName);
                        }
                        bulkCopy.WriteToServer(dt);
                        dt.Clear();
                    }
                    catch (Exception ex) { System.Windows.MessageBox.Show(ex.Message); }

                }//end using
                DBUtility.CloseConnector(targetcon);
            }//end if
        }

        private static List<ProjectMoveHistory> RetrieveProcessData(string PJKey, string MfgName)
        {
            var ret = new List<ProjectMoveHistory>();
            var sql = string.Empty;

            var bondingedprocess = ProjectViewModels.RetriveProjectProcessBonding(PJKey);
            if (bondingedprocess.Count > 0)
            {
                var items = bondingedprocess[0].Except.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                if (items.Length > 0)
                {
                    var namecond = "'";
                    foreach (var item in items)
                    {
                        namecond = namecond + item + "','";
                    }
                    namecond = namecond.Substring(0, namecond.Length - 2);

                    sql = "select MoveOutQty,MoveInQty,WorkflowStepName,TxnTypeName,ContainerName,Comments from ProjectMoveHistory where MfgOrderName like '%<MfgName>%' and WorkflowStepName in (<namecond>)  order by MoveOutTime desc";
                    sql = sql.Replace("<MfgName>", MfgName).Replace("<namecond>", namecond);
                }
                else
                {
                    sql = "select MoveOutQty,MoveInQty,WorkflowStepName,TxnTypeName,ContainerName,Comments from ProjectMoveHistory where MfgOrderName like '%<MfgName>%'  order by MoveOutTime desc";
                    sql = sql.Replace("<MfgName>", MfgName);
                }

            }
            else
            {
                sql = "select MoveOutQty,MoveInQty,WorkflowStepName,TxnTypeName,ContainerName,Comments from ProjectMoveHistory where MfgOrderName like '%<MfgName>%'  order by MoveOutTime desc";
                sql = sql.Replace("<MfgName>", MfgName);
            }

            var filterdict = new Dictionary<string, bool>();

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var tempmove = new ProjectMoveHistory();
                tempmove.ProjectKey = PJKey;
                tempmove.MoveOutQty = Convert.ToInt32(line[0]);
                tempmove.MoveInQty = Convert.ToInt32(line[1]);
                tempmove.WorkflowStepName = Convert.ToString(line[2]);
                tempmove.TxnTypeName = Convert.ToInt32(line[3]);
                tempmove.ContainerName = Convert.ToString(line[4]);
                tempmove.Comments = Convert.ToString(line[5]);

                if (!filterdict.ContainsKey(tempmove.ContainerName + "-" + tempmove.WorkflowStepName))
                {
                    filterdict.Add(tempmove.ContainerName + "-" + tempmove.WorkflowStepName, true);
                    ret.Add(tempmove);
                }
            }
            return ret;
        }

        public static List<string> RetrieveProcessWithSequence(string PJKey, string starttime, string endtime)
        {
            var ret = new List<string>();
            var uniqukey = new Dictionary<string, bool>();

            var sql = "select distinct Sequence,WorkflowStepName from ProjectMoveHistory "
            + " where  ProjectKey = '<ProjectKey>' and  MoveOutTime >= '<starttime>' and MoveOutTime <= '<endtime>' order by  Sequence asc,WorkflowStepName asc ";
            sql = sql.Replace("<ProjectKey>", PJKey).Replace("<starttime>", starttime).Replace("<endtime>", endtime);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var flowname = ConvertString(line[1]);
                if (!string.IsNullOrEmpty(flowname))
                {
                    if (!uniqukey.ContainsKey(flowname))
                    {
                        uniqukey.Add(flowname, true);
                        ret.Add(flowname);
                    }
                }
            }
            return ret;
        }

        public static List<string> RetrieveProcessWithSequenceByMfg(string PJKey, string mfg)
        {
            var ret = new List<string>();
            var uniqukey = new Dictionary<string, bool>();

            var sql = "select distinct Sequence,WorkflowStepName from ProjectMoveHistory "
            + " where  ProjectKey = '<ProjectKey>' and MfgOrderName like '%<MfgOrderName>%' order by  Sequence asc,WorkflowStepName asc ";
            sql = sql.Replace("<ProjectKey>", PJKey).Replace("<MfgOrderName>", mfg);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var flowname = ConvertString(line[1]);
                if (!string.IsNullOrEmpty(flowname))
                {
                    if (!uniqukey.ContainsKey(flowname))
                    {
                        uniqukey.Add(flowname, true);
                        ret.Add(flowname);
                    }
                }
            }
            return ret;
        }

        public static List<string> RetrieveJoList(string PJKey)
        {
            var ret = new List<string>();
            var sql = "select distinct MfgOrderName from ProjectMoveHistory where MoveOutTime > '<starttime>' and ProjectKey= '<ProjectKey>'  order by MfgOrderName";
            sql = sql.Replace("<starttime>", DateTime.Now.AddMonths(-6).ToString("yyyy-MM-dd HH:mm:ss")).Replace("<ProjectKey>", PJKey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var MfgOrderName = ConvertString(line[0]);
                if (!string.IsNullOrEmpty(MfgOrderName))
                {
                    ret.Add(MfgOrderName);
                }
            }
            return ret;
        }

        private static List<ProjectMoveHistory> RetrieveProcessDataByTime(string PJKey, string starttime,string endtime)
        {
            var ret = new List<ProjectMoveHistory>();
            var sql = string.Empty;

            var bondingedprocess = ProjectViewModels.RetriveProjectProcessBonding(PJKey);
            if (bondingedprocess.Count > 0)
            {
                var items = bondingedprocess[0].Except.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                if (items.Length > 0)
                {
                    var namecond = "'";
                    foreach (var item in items)
                    {
                        namecond = namecond + item + "','";
                    }
                    namecond = namecond.Substring(0, namecond.Length - 2);

                    sql = "select MoveOutQty,MoveInQty,WorkflowStepName,TxnTypeName,ContainerName,Comments from ProjectMoveHistory where ProjectKey = '<ProjectKey>' and  MoveOutTime >= '<starttime>' and MoveOutTime <= '<endtime>'  and WorkflowStepName in (<namecond>)  order by MoveOutTime desc";
                    sql = sql.Replace("<ProjectKey>", PJKey).Replace("<starttime>", starttime).Replace("<endtime>", endtime).Replace("<namecond>", namecond);
                }
                else
                {
                    sql = "select MoveOutQty,MoveInQty,WorkflowStepName,TxnTypeName,ContainerName,Comments from ProjectMoveHistory where  ProjectKey = '<ProjectKey>' and  MoveOutTime >= '<starttime>' and MoveOutTime <= '<endtime>'  order by MoveOutTime desc";
                    sql = sql.Replace("<ProjectKey>", PJKey).Replace("<starttime>", starttime).Replace("<endtime>", endtime);
                }

            }
            else
            {
                sql = "select MoveOutQty,MoveInQty,WorkflowStepName,TxnTypeName,ContainerName,Comments from ProjectMoveHistory where  ProjectKey = '<ProjectKey>' and  MoveOutTime >= '<starttime>' and MoveOutTime <= '<endtime>' order by MoveOutTime desc";
                sql = sql.Replace("<ProjectKey>", PJKey).Replace("<starttime>", starttime).Replace("<endtime>", endtime);
            }

            var filterdict = new Dictionary<string, bool>();

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var tempmove = new ProjectMoveHistory();
                tempmove.ProjectKey = PJKey;
                tempmove.MoveOutQty = Convert.ToInt32(line[0]);
                tempmove.MoveInQty = Convert.ToInt32(line[1]);
                tempmove.WorkflowStepName = Convert.ToString(line[2]);
                tempmove.TxnTypeName = Convert.ToInt32(line[3]);
                tempmove.ContainerName = Convert.ToString(line[4]);
                tempmove.Comments = Convert.ToString(line[5]);

                if (!filterdict.ContainsKey(tempmove.ContainerName + "-" + tempmove.WorkflowStepName))
                {
                    filterdict.Add(tempmove.ContainerName + "-" + tempmove.WorkflowStepName, true);
                    ret.Add(tempmove);
                }
            }
            return ret;
        }

        public static DateTime RetrieveLastWeek()
        {
            var currentday = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")+ " 07:30:00");

            if (currentday.DayOfWeek > DayOfWeek.Thursday)
            {
                currentday = currentday.AddDays(4-(int)currentday.DayOfWeek);
                return DateTime.Parse(currentday.ToString("yyyy-MM-dd") + " 07:30:00");
            }
            else if (currentday.DayOfWeek == DayOfWeek.Thursday)
            {
                currentday = currentday.AddDays(-7);
                return DateTime.Parse(currentday.ToString("yyyy-MM-dd") + " 07:30:00");
            }
            else
            {
                currentday = currentday.AddDays(4 - ((int)currentday.DayOfWeek+7));
                return DateTime.Parse(currentday.ToString("yyyy-MM-dd") + " 07:30:00");
            }
        }


        public static List<string> RetrieveLastWeekProcess(string PJKey)
        {
            var starttime = RetrieveLastWeek().ToString("yyyy-MM-dd") + " 07:30:00";
            var endtime = DateTime.Now.ToString("yyyy-MM-dd") + " 07:30:00";
            return RetrieveProcessWithSequence(PJKey, starttime, endtime);
        }

        public static Dictionary<string, ProjectMoveHistory> RetrieveLastWeekProcessDataByDate(string PJKey,string startdate,string enddate,Dictionary<string,List<ProjectMoveHistory>> detailinfo)
        {
            var ret = new Dictionary<string, ProjectMoveHistory>();
            var lastweekprocessdata =  RetrieveProcessDataByTime(PJKey, startdate, enddate);
            foreach (var item in lastweekprocessdata)
            {
                if (ret.ContainsKey(item.WorkflowStepName))
                {
                    if (item.TxnTypeName == TXNTYPENAME.MoveStd)
                    {
                        ret[item.WorkflowStepName].MoveOutQty = ret[item.WorkflowStepName].MoveOutQty + item.MoveOutQty;
                        ret[item.WorkflowStepName].MoveInQty = ret[item.WorkflowStepName].MoveInQty  + item.MoveOutQty;
                    }
                    else if (item.TxnTypeName == TXNTYPENAME.MoveNonStd)
                    {
                        ret[item.WorkflowStepName].MoveInQty = ret[item.WorkflowStepName].MoveInQty + item.MoveOutQty;
                        if (detailinfo != null)
                        {
                            var tempcomm = new ProjectMoveHistory();
                            tempcomm.ContainerName = item.ContainerName;
                            tempcomm.Comments = item.Comments;

                            if (!detailinfo.ContainsKey(item.WorkflowStepName))
                            {
                                var templist = new List<ProjectMoveHistory>();
                                templist.Add(tempcomm);
                                detailinfo.Add(item.WorkflowStepName, templist);
                            }
                            else
                            {
                                detailinfo[item.WorkflowStepName].Add(tempcomm);
                            }
                        }//end if
                    }
                }
                else
                {
                    var tempmove = new ProjectMoveHistory();
                    tempmove.MoveOutQty = 0;
                    tempmove.MoveInQty = 0;
                    tempmove.WorkflowStepName = item.WorkflowStepName;

                    if (item.TxnTypeName == TXNTYPENAME.MoveStd)
                    {
                        tempmove.MoveOutQty = item.MoveOutQty;
                        tempmove.MoveInQty = item.MoveOutQty;
                    }
                    else if (item.TxnTypeName == TXNTYPENAME.MoveNonStd)
                    {
                        tempmove.MoveInQty = item.MoveOutQty;
                        if (detailinfo != null)
                        {
                            var tempcomm = new ProjectMoveHistory();
                            tempcomm.ContainerName = item.ContainerName;
                            tempcomm.Comments = item.Comments;

                            if (!detailinfo.ContainsKey(item.WorkflowStepName))
                            {
                                var templist = new List<ProjectMoveHistory>();
                                templist.Add(tempcomm);
                                detailinfo.Add(item.WorkflowStepName, templist);
                            }
                            else
                            {
                                detailinfo[item.WorkflowStepName].Add(tempcomm);
                            }
                        }//end if
                    }
                    if (tempmove.MoveInQty != 0)
                    {
                        ret.Add(item.WorkflowStepName, tempmove);
                    }
                }
            }

            return ret;
        }

        public static Dictionary<string, ProjectMoveHistory> RetrieveProcessDataSummaryByTime(string PJKey,string starttime,string endtime)
        {
            var ret = new Dictionary<string, ProjectMoveHistory>();

            var lastweekprocessdata = RetrieveProcessDataByTime(PJKey, starttime, endtime);
            foreach (var item in lastweekprocessdata)
            {
                if (ret.ContainsKey(item.WorkflowStepName))
                {
                    if (item.TxnTypeName == TXNTYPENAME.MoveStd)
                    {
                        ret[item.WorkflowStepName].MoveOutQty = ret[item.WorkflowStepName].MoveOutQty + item.MoveOutQty;
                        ret[item.WorkflowStepName].MoveInQty = ret[item.WorkflowStepName].MoveInQty + item.MoveOutQty;
                    }
                    else if (item.TxnTypeName == TXNTYPENAME.MoveNonStd)
                    {
                        ret[item.WorkflowStepName].MoveInQty = ret[item.WorkflowStepName].MoveInQty + item.MoveOutQty;
                    }
                }
                else
                {
                    var tempmove = new ProjectMoveHistory();
                    tempmove.MoveOutQty = 0;
                    tempmove.MoveInQty = 0;
                    tempmove.WorkflowStepName = item.WorkflowStepName;

                    if (item.TxnTypeName == TXNTYPENAME.MoveStd)
                    {
                        tempmove.MoveOutQty = item.MoveOutQty;
                        tempmove.MoveInQty = item.MoveOutQty;
                    }
                    else if (item.TxnTypeName == TXNTYPENAME.MoveNonStd)
                    {
                        tempmove.MoveInQty = item.MoveOutQty;
                    }
                    if (tempmove.MoveInQty != 0)
                    {
                        ret.Add(item.WorkflowStepName, tempmove);
                    }
                }
            }

            return ret;
        }

        public static Dictionary<string, ProjectMoveHistory> RetrieveProcessDataByMfg(string PJKey,string mfg, Dictionary<string, List<ProjectMoveHistory>> detailinfo)
        {
            var ret = new Dictionary<string, ProjectMoveHistory>();

            var mfgprocessdata = RetrieveProcessData(PJKey, mfg);
            foreach (var item in mfgprocessdata)
            {
                if (ret.ContainsKey(item.WorkflowStepName))
                {
                    if (item.TxnTypeName == TXNTYPENAME.MoveStd)
                    {
                        ret[item.WorkflowStepName].MoveOutQty = ret[item.WorkflowStepName].MoveOutQty + item.MoveOutQty;
                        ret[item.WorkflowStepName].MoveInQty = ret[item.WorkflowStepName].MoveInQty + item.MoveOutQty;
                    }
                    else if (item.TxnTypeName == TXNTYPENAME.MoveNonStd)
                    {
                        ret[item.WorkflowStepName].MoveInQty = ret[item.WorkflowStepName].MoveInQty + item.MoveOutQty;
                        if (detailinfo != null)
                        {
                            var tempcomm = new ProjectMoveHistory();
                            tempcomm.ContainerName = item.ContainerName;
                            tempcomm.Comments = item.Comments;

                            if (!detailinfo.ContainsKey(item.WorkflowStepName))
                            {
                                var templist = new List<ProjectMoveHistory>();
                                templist.Add(tempcomm);
                                detailinfo.Add(item.WorkflowStepName, templist);
                            }
                            else
                            {
                                detailinfo[item.WorkflowStepName].Add(tempcomm);
                            }
                        }//end if
                    }
                }
                else
                {
                    var tempmove = new ProjectMoveHistory();
                    tempmove.MoveOutQty = 0;
                    tempmove.MoveInQty = 0;
                    tempmove.WorkflowStepName = item.WorkflowStepName;

                    if (item.TxnTypeName == TXNTYPENAME.MoveStd)
                    {
                        tempmove.MoveOutQty = item.MoveOutQty;
                        tempmove.MoveInQty = item.MoveOutQty;
                    }
                    else if (item.TxnTypeName == TXNTYPENAME.MoveNonStd)
                    {
                        tempmove.MoveInQty = item.MoveOutQty;
                        if (detailinfo != null)
                        {
                            var tempcomm = new ProjectMoveHistory();
                            tempcomm.ContainerName = item.ContainerName;
                            tempcomm.Comments = item.Comments;

                            if (!detailinfo.ContainsKey(item.WorkflowStepName))
                            {
                                var templist = new List<ProjectMoveHistory>();
                                templist.Add(tempcomm);
                                detailinfo.Add(item.WorkflowStepName, templist);
                            }
                            else
                            {
                                detailinfo[item.WorkflowStepName].Add(tempcomm);
                            }
                        }//end if
                    }

                    if (tempmove.MoveInQty != 0)
                    {
                        ret.Add(item.WorkflowStepName, tempmove);
                    }
                }
            }

            return ret;
        }

    }
}