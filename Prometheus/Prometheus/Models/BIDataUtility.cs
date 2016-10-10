using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Prometheus.Models
{
    public class BIDataUtility
    {
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

        private static string PNCondition(List<ProjectPn> pns)
        {
            string ret = "('";
            foreach (var pn in pns)
            {
                ret = ret + RMSpectialCh(pn.Pn) + "','";
            }

            if (pns.Count > 0)
            {
                ret = ret.Substring(0, ret.Length - 2) + ")";
            }
            else
            {
                ret = "('')";
            }

            return ret;
        }

        private static string GetUniqKey()
        {
            return Guid.NewGuid().ToString("N");
        }

        private static double ConvertDouble(object value)
        {
            if (value != null)
            {
                try
                {
                    return Convert.ToDouble(value);
                }
                catch (Exception ex)
                {
                    return 0.0;
                }
            }
            else
            {
                return 0.0;
            }
        }

        private static string ConvertString(object value)
        {
            if (value != null)
            {
                try
                {
                    return Convert.ToString(value);
                }
                catch (Exception ex)
                {
                    return string.Empty;
                }
            }
            else
            {
                return string.Empty;
            }
        }

        public static void StartProjectBonding(ProjectViewModels vm)
        {
            try
            {
                if (BITestData.UpdatePJLockUsing(vm.ProjectKey))
                    return;

                    if (vm.PNList.Count > 0)
                    {
                        var bondeddatadict = BITestData.RetrieveAllDataID(vm.ProjectKey);

                        var failurelist = new List<BITestData>();
                        var sql = "select ModuleSerialNum,Step,ModuleType,ErrAbbr,TestTimeStamp,TestStation,ModulePartNum,wafer,waferpn,ChannelNum,SLOPE,THOLD,PO_LD,PO_LD_18,PO_LD_25,PO_LD_127,PO_Uniformity,Delta_SLOPE,Delta_THOLD,Delta_PO_LD,Delta_PO_LD_18,Delta_PO_LD_25,Delta_PO_LD_127,Delta_PO_Uniformity from dbo.PRLL_VcselInfoSummary_2016(nolock) where wafer <> 'NULL' and waferpn  <> 'NULL' and ErrAbbr  <> 'NULL' and ModulePartNum in <PNCOND> and TestTimeStamp > '<TIMECOND>' order by TestTimeStamp Desc,ModuleSerialNum";
                        var pncond = PNCondition(vm.PNList);
                        sql = sql.Replace("<PNCOND>",pncond).Replace("<TIMECOND>", vm.StartDate.ToString());

                        var tempdataiddict = new Dictionary<string, BITestData>();
                        var dbret = DBUtility.ExePRLSqlWithRes(sql);
                        foreach (var line in dbret)
                        {
                            var tempid = ConvertString(line[0])+ "_" + ConvertString(line[1]);

                            if (bondeddatadict.ContainsKey(tempid))
                            {
                                if (DateTime.Parse(ConvertString(line[4])) <= bondeddatadict[tempid])
                                {
                                    continue;
                                }
                            }

                            if (tempdataiddict.ContainsKey(tempid))
                            {
                                if (DateTime.Parse(ConvertString(line[4])) == tempdataiddict[tempid].TestTimeStamp)
                                {
                                    if (string.Compare(ConvertString(line[3]), "Pass", true) != 0)
                                    {
                                        tempdataiddict[tempid].ErrAbbr = ConvertString(line[3]);
                                    }

                                    var tempdf = new BITestDataField();
                                    tempdf.DataID = tempdataiddict[tempid].DataID;
                                    tempdf.ChannelNum = ConvertString(line[9]);
                                    tempdf.SLOPE = ConvertDouble(line[10]);
                                    tempdf.THOLD = ConvertDouble(line[11]);
                                    tempdf.PO_LD = ConvertDouble(line[12]);
                                    tempdf.PO_LD_18 = ConvertDouble(line[13]);
                                    tempdf.PO_LD_25 = ConvertDouble(line[14]);
                                    tempdf.PO_LD_127 = ConvertDouble(line[15]);
                                    tempdf.PO_Uniformity = ConvertDouble(line[16]);
                                    tempdf.Delta_SLOPE = ConvertDouble(line[17]);
                                    tempdf.Delta_THOLD = ConvertDouble(line[18]);
                                    tempdf.Delta_PO_LD = ConvertDouble(line[19]);
                                    tempdf.Delta_PO_LD_18 = ConvertDouble(line[20]);
                                    tempdf.Delta_PO_LD_25 = ConvertDouble(line[21]);
                                    tempdf.Delta_PO_LD_127 = ConvertDouble(line[22]);
                                    tempdf.Delta_PO_Uniformity = ConvertDouble(line[23]);
                                    tempdataiddict[tempid].DataFields.Add(tempdf);
                                }//same time
                            }
                            else
                            {
                                var tempdata = new BITestData();
                                tempdata.DataID = GetUniqKey();
                                tempdata.ProjectKey = vm.ProjectKey;
                                tempdata.ModuleSerialNum = ConvertString(line[0]);
                                tempdata.WhichTest = ConvertString(line[1]);
                                tempdata.ModuleType = ConvertString(line[2]);
                                tempdata.ErrAbbr = ConvertString(line[3]);
                                tempdata.TestTimeStamp = DateTime.Parse(ConvertString(line[4]));
                                tempdata.TestStation = ConvertString(line[5]);
                                tempdata.PN = ConvertString(line[6]);
                                tempdata.Wafer = ConvertString(line[7]);
                                tempdata.Waferpn = ConvertString(line[8]);

                                var tempdf = new BITestDataField();
                                tempdf.DataID = tempdata.DataID;
                                tempdf.ChannelNum = ConvertString(line[9]);
                                tempdf.SLOPE = ConvertDouble(line[10]);
                                tempdf.THOLD = ConvertDouble(line[11]);
                                tempdf.PO_LD = ConvertDouble(line[12]);
                                tempdf.PO_LD_18 = ConvertDouble(line[13]);
                                tempdf.PO_LD_25 = ConvertDouble(line[14]);
                                tempdf.PO_LD_127 = ConvertDouble(line[15]);
                                tempdf.PO_Uniformity = ConvertDouble(line[16]);
                                tempdf.Delta_SLOPE = ConvertDouble(line[17]);
                                tempdf.Delta_THOLD = ConvertDouble(line[18]);
                                tempdf.Delta_PO_LD = ConvertDouble(line[19]);
                                tempdf.Delta_PO_LD_18 = ConvertDouble(line[20]);
                                tempdf.Delta_PO_LD_25 = ConvertDouble(line[21]);
                                tempdf.Delta_PO_LD_127 = ConvertDouble(line[22]);
                                tempdf.Delta_PO_Uniformity = ConvertDouble(line[23]);

                                tempdata.DataFields.Add(tempdf);

                                tempdataiddict.Add(tempid, tempdata);
                            }
                        }//end foreach

                        foreach (var kv in tempdataiddict)
                        {
                            kv.Value.StoreBIData();
                        }//end foreach

                    }

                BITestData.ResetUpdatePJLock(vm.ProjectKey);
            }
            catch (Exception ex)
            {
                BITestData.ResetUpdatePJLock(vm.ProjectKey);
            }


        }


        public static void UpdateProjectData(ProjectViewModels vm)
        {
            if (vm.PNList.Count > 0)
            {
                var starttime = BITestData.RetrieveLatestTimeOfLocalBI(vm.ProjectKey);
                if (string.IsNullOrEmpty(starttime))
                {
                    starttime = vm.StartDate.ToString();
                }

                var sql = "select ModuleSerialNum,Step,ModuleType,ErrAbbr,TestTimeStamp,TestStation,ModulePartNum,wafer,waferpn,ChannelNum,SLOPE,THOLD,PO_LD,PO_LD_18,PO_LD_25,PO_LD_127,PO_Uniformity,Delta_SLOPE,Delta_THOLD,Delta_PO_LD,Delta_PO_LD_18,Delta_PO_LD_25,Delta_PO_LD_127,Delta_PO_Uniformity from dbo.PRLL_VcselInfoSummary_2016(nolock) where wafer <> 'NULL' and waferpn  <> 'NULL' and ErrAbbr  <> 'NULL' and ModulePartNum in <PNCOND> and TestTimeStamp > '<TIMECOND>' order by TestTimeStamp Desc,ModuleSerialNum";
                var pncond = PNCondition(vm.PNList);
                sql = sql.Replace("<PNCOND>", pncond).Replace("<TIMECOND>", starttime);

                var tempdataiddict = new Dictionary<string, BITestData>();
                var dbret = DBUtility.ExePRLSqlWithRes(sql);
                foreach (var line in dbret)
                {
                    var tempid = ConvertString(line[0]) + "_" + ConvertString(line[1]);
                    if (tempdataiddict.ContainsKey(tempid))
                    {
                        if (DateTime.Parse(ConvertString(line[4])) == tempdataiddict[tempid].TestTimeStamp)
                        {
                            if (string.Compare(ConvertString(line[3]), "Pass", true) != 0)
                            {
                                tempdataiddict[tempid].ErrAbbr = ConvertString(line[3]);
                            }

                            var tempdf = new BITestDataField();
                            tempdf.DataID = tempdataiddict[tempid].DataID;
                            tempdf.ChannelNum = ConvertString(line[9]);
                            tempdf.SLOPE = ConvertDouble(line[10]);
                            tempdf.THOLD = ConvertDouble(line[11]);
                            tempdf.PO_LD = ConvertDouble(line[12]);
                            tempdf.PO_LD_18 = ConvertDouble(line[13]);
                            tempdf.PO_LD_25 = ConvertDouble(line[14]);
                            tempdf.PO_LD_127 = ConvertDouble(line[15]);
                            tempdf.PO_Uniformity = ConvertDouble(line[16]);
                            tempdf.Delta_SLOPE = ConvertDouble(line[17]);
                            tempdf.Delta_THOLD = ConvertDouble(line[18]);
                            tempdf.Delta_PO_LD = ConvertDouble(line[19]);
                            tempdf.Delta_PO_LD_18 = ConvertDouble(line[20]);
                            tempdf.Delta_PO_LD_25 = ConvertDouble(line[21]);
                            tempdf.Delta_PO_LD_127 = ConvertDouble(line[22]);
                            tempdf.Delta_PO_Uniformity = ConvertDouble(line[23]);
                            tempdataiddict[tempid].DataFields.Add(tempdf);
                        }//same time
                    }
                    else
                    {
                        var tempdata = new BITestData();
                        tempdata.DataID = GetUniqKey();
                        tempdata.ProjectKey = vm.ProjectKey;
                        tempdata.ModuleSerialNum = ConvertString(line[0]);
                        tempdata.WhichTest = ConvertString(line[1]);
                        tempdata.ModuleType = ConvertString(line[2]);
                        tempdata.ErrAbbr = ConvertString(line[3]);
                        tempdata.TestTimeStamp = DateTime.Parse(ConvertString(line[4]));
                        tempdata.TestStation = ConvertString(line[5]);
                        tempdata.PN = ConvertString(line[6]);
                        tempdata.Wafer = ConvertString(line[7]);
                        tempdata.Waferpn = ConvertString(line[8]);

                        var tempdf = new BITestDataField();
                        tempdf.DataID = tempdata.DataID;
                        tempdf.ChannelNum = ConvertString(line[9]);
                        tempdf.SLOPE = ConvertDouble(line[10]);
                        tempdf.THOLD = ConvertDouble(line[11]);
                        tempdf.PO_LD = ConvertDouble(line[12]);
                        tempdf.PO_LD_18 = ConvertDouble(line[13]);
                        tempdf.PO_LD_25 = ConvertDouble(line[14]);
                        tempdf.PO_LD_127 = ConvertDouble(line[15]);
                        tempdf.PO_Uniformity = ConvertDouble(line[16]);
                        tempdf.Delta_SLOPE = ConvertDouble(line[17]);
                        tempdf.Delta_THOLD = ConvertDouble(line[18]);
                        tempdf.Delta_PO_LD = ConvertDouble(line[19]);
                        tempdf.Delta_PO_LD_18 = ConvertDouble(line[20]);
                        tempdf.Delta_PO_LD_25 = ConvertDouble(line[21]);
                        tempdf.Delta_PO_LD_127 = ConvertDouble(line[22]);
                        tempdf.Delta_PO_Uniformity = ConvertDouble(line[23]);

                        tempdata.DataFields.Add(tempdf);

                        tempdataiddict.Add(tempid, tempdata);
                    }
                }//end foreach

                foreach (var kv in tempdataiddict)
                {
                    kv.Value.StoreBIData();
                }//end foreach

            }
        }


    }
}