﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace Prometheus.Models
{
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

            Qty = 0;
            MoveInQty = 1;

            MfgOrderId = string.Empty;
            TxnDetails = string.Empty;
            TxnDate = DateTime.Parse("1982-05-06 10:00:00");
            MfgOrderName = string.Empty;

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

        public int Qty { set; get; }
        public int MoveInQty { set; get; }

        public string MfgOrderId { set; get; }
        public string TxnDetails { set; get; }
        public DateTime TxnDate { set; get; }
        public string MfgOrderName { set; get; }

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
            var pvm = ProjectViewModels.RetrieveOneProject(PJKey);
            var pncond = PNCondition(pvm.PNList);
            if (!string.IsNullOrEmpty(pncond))
            {
                var workflowlist = new List<ProjectWorkflow>();
                var productiddict = new Dictionary<string, bool>();

                var sql = "select p.ProductId,s.WorkflowStepID, s.WorkflowStepName,s.WorkflowID,s.Sequence from InsiteDB.insite.WorkflowStep s (nolock) " 
                    + " left join InsiteDB.insite.Workflow w (nolock) on s.WorkflowID = w.WorkflowID"
                    + " left join InsiteDB.insite.Product p (nolock) on w.WorkflowBaseId = p.WorkflowBaseId"
                    + " where (<pncond>) and p.Description like '%GFA%' "
                    + " and p.WorkflowBaseId is not null and p.WorkflowBaseId <> '0000000000000000' ";
                sql = sql.Replace("<pncond>", pncond);
                var dbret = DBUtility.ExeMESSqlWithRes(sql);
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
                        .Replace("<databackuptm>",DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
                    DBUtility.ExeLocalSqlNoRes(csql);
                }
            }//end if
        }

        public static List<string> GetCurrentProjectWorkflowSteps(string PJKey)
        {
            var ret = new List<string>();

            var sql = "select distinct WorkflowStepName from ProjectWorkflow where ProjectKey = '<ProjectKey>'";
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

                sql = "select ProductId,WorkflowStepID,WorkflowStepName from ProjectWorkflow where WorkflowStepName in (<stepnamecond>) and ProjectKey = '<ProjectKey>'";
                sql = sql.Replace("<ProjectKey>", PJKey).Replace("<stepnamecond>", namecond);
            }
            else
            {
                sql = "select ProductId,WorkflowStepID,WorkflowStepName from ProjectWorkflow where ProjectKey = '<ProjectKey>'";
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
                    ret.Add(tempflow);
                }
                catch (Exception ex) { }
            }//end foreach

            return ret;
        }

        private static string RetriveProjectMoveHistoryLatetime(string PJKey,string movezerotime)
        {
            var sql = "select top 1 TxnDate from ProjectMoveHistory where ProjectKey = '<ProjectKey>' order by TxnDate DESC";
            sql = sql.Replace("<ProjectKey>", PJKey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            if (dbret.Count > 0)
            {
                try
                {
                    return Convert.ToDateTime(dbret[0][0]).ToString("yyyy-MM-dd hh:mm:ss");
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

            var sql = "select MfgOrderId,MfgOrderName from InsiteDB.insite.MfgOrder where MfgOrderId in (<mfgcond>)";
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
    

        public static void LoadMESMoveHistory(string PJKey,Controller ctrl)
        {
            var allworkflow = RetriveWorkflowByName(PJKey, new List<string>());
            if (allworkflow.Count > 0)
            {
                var syscfg = CfgUtility.GetSysConfig(ctrl);
                var movezerotime = syscfg["MOVEHISTORYZEROTIME"];
                var latesttime = RetriveProjectMoveHistoryLatetime(PJKey, movezerotime);

                var pidcond = "'";
                foreach (var item in allworkflow)
                {
                    pidcond = pidcond + item.ProductId + "','";
                }
                pidcond = pidcond.Substring(0, pidcond.Length - 2);

                var sidcond = "'";
                foreach (var item in allworkflow)
                {
                    sidcond = sidcond + item.WorkflowStepID + "','";
                }
                sidcond = sidcond.Substring(0, sidcond.Length - 2);

                var stepnamedict = new Dictionary<string, string>();
                foreach (var item in allworkflow)
                {
                    if (!stepnamedict.ContainsKey(item.WorkflowStepID))
                    {
                        stepnamedict.Add(item.WorkflowStepID, item.WorkflowStepName);
                    }
                }

                var sql = "select ProductId,StepId,Qty,MoveInQty,MfgOrderId,TxnDetails,TxnDate from InsiteDB.insite.MoveHistory (nolock)  where HistoryMainlineId in ( " 
                    + " SELECT  HistoryMainlineId  FROM InsiteDB.insite.HistoryMainline (nolock) " 
                    + " where ProductId in (<pidcond>) and WorkflowStepId in (<sidcond>) and TxnDate > '<timecond>') ";
                sql = sql.Replace("<pidcond>", pidcond).Replace("<sidcond>", sidcond).Replace("<timecond>", latesttime);

                var MfgOrdidDict = new Dictionary<string, bool>();
                var movelist = new List<ProjectMoveHistory>();

                var dbret = DBUtility.ExeMESSqlWithRes(sql);
                foreach (var line in dbret)
                {
                    try
                    {
                        var tempmove = new ProjectMoveHistory();
                        tempmove.ProjectKey = PJKey;
                        tempmove.ProductId = ConvertString(line[0]);
                        tempmove.WorkflowStepID = ConvertString(line[1]);
                        tempmove.Qty = Convert.ToInt32(line[2]);
                        tempmove.MoveInQty = Convert.ToInt32(line[3]);
                        tempmove.MfgOrderId = ConvertString(line[4]);
                        tempmove.TxnDetails = ConvertString(line[5]);
                        tempmove.TxnDate = Convert.ToDateTime(line[6]);
                        tempmove.WorkflowStepName = stepnamedict[tempmove.WorkflowStepID];
                        movelist.Add(tempmove);

                        if (!MfgOrdidDict.ContainsKey(tempmove.MfgOrderId))
                        {
                            MfgOrdidDict.Add(tempmove.MfgOrderId, true);
                        }
                    }
                    catch (Exception ex) { }
                }//end foreach

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



    }
}