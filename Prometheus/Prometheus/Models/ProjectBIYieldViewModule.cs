using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
    public class ProjectBIYieldViewModule
    {
        public string ProjectKey { set; get; }
        public DateTime StartDate { set; get; }
        public DateTime EndDate { set; get; }

        private List<TestYield> lyield = new List<TestYield>();
        public List<TestYield> LastYields { get { return lyield; } }
        public double LastYield
        {
            get
            {
                var ret = 1.0;
                foreach (var item in LastYields)
                {
                    ret = ret * item.Yield;
                }
                return ret;
            }
        }

        public double CorrectLastYield
        {
            get
            {
                var ret = 1.0;
                foreach (var item in LastYields)
                {
                    ret = ret * item.CorrectYield;
                }
                return ret;
            }
        }

        private Dictionary<string, Dictionary<string, int>> lemap = new Dictionary<string, Dictionary<string, int>>();
        public Dictionary<string, Dictionary<string, int>> LErrorMap { get { return lemap; } }


        private static void RegisterError(string errorcode1, string whichtest, Dictionary<string, Dictionary<string, int>> emap)
        {
            var errorcode = errorcode1;
            if (string.Compare(errorcode, "pass", true) == 0)
                errorcode = "PASS";

            if (emap.ContainsKey(errorcode))
            {
                var tempdict = emap[errorcode];
                if (tempdict.ContainsKey(whichtest))
                {
                    tempdict[whichtest] = tempdict[whichtest] + 1;
                }
                else
                {
                    tempdict.Add(whichtest, 1);
                }
            }
            else
            {
                var tempdict = new Dictionary<string, int>();
                tempdict.Add(whichtest, 1);
                emap.Add(errorcode, tempdict);
            }
        }

        private static void RetrieveCummYield(ProjectBIYieldViewModule pyvm, List<BITestData> plist)
        {
            var yielddict = new Dictionary<string, TestYield>();
            var sndict = new Dictionary<string, bool>();

            var correctbidict = IssueViewModels.RetrieveAllBIRootCause(pyvm.ProjectKey);

            foreach (var p in plist)
            {
                if (!sndict.ContainsKey(p.WhichTest + ":" + p.ModuleSerialNum))
                {
                    sndict.Add(p.WhichTest + ":" + p.ModuleSerialNum, true);
                    if (yielddict.ContainsKey(p.WhichTest))
                    {
                        if (!yielddict[p.WhichTest].AllSNDict.ContainsKey(p.ModuleSerialNum))
                        { yielddict[p.WhichTest].AllSNDict.Add(p.ModuleSerialNum, true); }

                        yielddict[p.WhichTest].InputCount = yielddict[p.WhichTest].InputCount + 1;
                        if (string.Compare(p.ErrAbbr, "PASS", true) == 0)
                        {
                            yielddict[p.WhichTest].OutputCount = yielddict[p.WhichTest].OutputCount + 1;
                            yielddict[p.WhichTest].CorrectOutputCount = yielddict[p.WhichTest].CorrectOutputCount + 1;
                        }
                        else
                        {
                            if (correctbidict.ContainsKey(p.ModuleSerialNum)
                                && string.Compare(correctbidict[p.ModuleSerialNum], BIROOTCAUSE.VCSELISSUE) != 0)
                            {
                                yielddict[p.WhichTest].CorrectOutputCount = yielddict[p.WhichTest].CorrectOutputCount + 1;
                            }
                            else
                            {
                                if (!yielddict[p.WhichTest].ErrSNDict.ContainsKey(p.ModuleSerialNum))
                                { yielddict[p.WhichTest].ErrSNDict.Add(p.ModuleSerialNum, true); }
                            }
                        }
                            
                        RegisterError(p.ErrAbbr, p.WhichTest, pyvm.LErrorMap);
                    }
                    else
                    {
                        var tempyield = new TestYield();
                        if (!tempyield.AllSNDict.ContainsKey(p.ModuleSerialNum))
                        { tempyield.AllSNDict.Add(p.ModuleSerialNum, true); }


                        tempyield.InputCount = 1;
                        if (string.Compare(p.ErrAbbr, "PASS", true) == 0)
                        {
                            tempyield.OutputCount = 1;
                            tempyield.CorrectOutputCount = 1;
                        }
                        else
                        {
                            tempyield.OutputCount = 0;
                            tempyield.CorrectOutputCount = 0;
                            if (correctbidict.ContainsKey(p.ModuleSerialNum)
                                && string.Compare(correctbidict[p.ModuleSerialNum], BIROOTCAUSE.VCSELISSUE) != 0)
                            {
                                tempyield.CorrectOutputCount = 1;
                            }
                            else
                            {
                                if (!tempyield.ErrSNDict.ContainsKey(p.ModuleSerialNum))
                                { tempyield.ErrSNDict.Add(p.ModuleSerialNum, true); }
                            }
                        }

                        tempyield.WhichTest = p.WhichTest;

                        RegisterError(p.ErrAbbr, p.WhichTest, pyvm.LErrorMap);
                        yielddict.Add(p.WhichTest, tempyield);
                    }
                }
            }

            foreach (var s in yielddict)
            {
               pyvm.LastYields.Add(s.Value);
            }

        }


        public static ProjectBIYieldViewModule GetYieldByDateRange(string pjkey, string sdate, string edate, ProjectViewModels pvm)
        {
            var ret = new ProjectBIYieldViewModule();
            ret.ProjectKey = pjkey;
            ret.StartDate = DateTime.Parse(sdate);
            ret.EndDate = DateTime.Parse(edate);

            var startdate = DateTime.Parse(DateTime.Parse(sdate).ToString("yyyy-MM-dd") + " 07:30:00").ToString();
            var enddate = DateTime.Parse(DateTime.Parse(edate).ToString("yyyy-MM-dd") + " 07:30:00").ToString();

            if (startdate == enddate)
            {
                return ret;
            }

            var plist = BITestData.RetrieveProjectTestData(pjkey, startdate, enddate, true);
            var snlist = BITestData.RetrieveSNBeforeDate(pjkey, startdate);
            var validatedict = new Dictionary<string, bool>();

            foreach (var item in plist)
            {
                if (!snlist.ContainsKey(item.ModuleSerialNum + "-" + item.WhichTest))
                {

                    if (!validatedict.ContainsKey(item.ModuleSerialNum))
                    {
                        validatedict.Add(item.ModuleSerialNum, true);
                    }
                }
            }

            plist = BITestData.RetrieveProjectTestData(pjkey, startdate, DateTime.Parse(enddate).AddYears(5).ToString(), false);
            var filteredPjData2 = new List<BITestData>();
            foreach (var item in plist)
            {
                if (validatedict.ContainsKey(item.ModuleSerialNum))
                {
                    filteredPjData2.Add(item);
                }
            }

            RetrieveCummYield(ret, filteredPjData2);

            return ret;
        }

        private static DateTime RetrieveFirstWeek(string startdate)
        {
            var sdate = DateTime.Parse(DateTime.Parse(startdate).ToString("yyyy-MM-dd") + " 07:30:00");
            if (sdate.DayOfWeek < DayOfWeek.Thursday)
            {
                sdate = sdate.AddDays(4 - (int)sdate.DayOfWeek);
                return DateTime.Parse(sdate.ToString("yyyy-MM-dd") + " 07:30:00");
            }
            else if (sdate.DayOfWeek == DayOfWeek.Thursday)
            {
                return DateTime.Parse(sdate.ToString("yyyy-MM-dd") + " 07:30:00");
            }
            else
            {
                sdate = sdate.AddDays(11 - (int)sdate.DayOfWeek);
                return DateTime.Parse(sdate.ToString("yyyy-MM-dd") + " 07:30:00");
            }
        }

        private static List<DateTime> RetrieveDateSpanByWeek(string startdate, string enddate)
        {
            var ret = new List<DateTime>();
            var sdate = DateTime.Parse(DateTime.Parse(startdate).ToString("yyyy-MM-dd") + " 07:30:00");
            ret.Add(sdate);
            var edate = DateTime.Parse(enddate);
            var firstweekend = RetrieveFirstWeek(startdate);
            if (firstweekend > edate)
            {
                ret.Add(DateTime.Parse(edate.ToString("yyyy-MM-dd") + " 07:30:00"));
                return ret;
            }

            ret.Add(firstweekend);


            var temptimepoint = firstweekend;
            while (temptimepoint < edate)
            {
                temptimepoint = temptimepoint.AddDays(7);
                if (temptimepoint > edate)
                {
                    ret.Add(DateTime.Parse(DateTime.Parse(enddate).ToString("yyyy-MM-dd") + " 07:30:00"));
                    return ret;
                }
                else
                {
                    ret.Add(DateTime.Parse(temptimepoint.ToString("yyyy-MM-dd") + " 07:30:00"));
                }
            }
            return ret;
        }

        public static List<ProjectBIYieldViewModule> GetYieldByWeeks(string pjkey)
        {
            var ret = new List<ProjectBIYieldViewModule>();

            var pvm = ProjectViewModels.RetrieveOneProject(pjkey);
            var ldate = RetrieveDateSpanByWeek(pvm.StartDate.ToString(), DateTime.Now.ToString());

            var startidx = 0;
            if (ldate.Count > 28)
            {
                startidx = ldate.Count - 28;
            }

            for (int idx = startidx; idx < ldate.Count - 1; idx++)
            {
                var temp = GetYieldByDateRange(pjkey, ldate[idx].ToString(), ldate[idx + 1].ToString(), pvm);
                if (temp.LastYields.Count > 0)
                {
                    ret.Add(temp);
                }
            }
            return ret;
        }

        public static int RetrieveErrorCount(string errorcode, string whichtest, Dictionary<string, Dictionary<string, int>> emap)
        {
            if (emap.ContainsKey(errorcode))
            {
                var tempdict = emap[errorcode];
                if (tempdict.ContainsKey(whichtest))
                {
                    return tempdict[whichtest];
                }
                return 0;
            }

            return 0;
        }


        public static ProjectBIYieldViewModule GetYieldByWafer(string pjkey, string wafer)
        {
            var ret = new ProjectBIYieldViewModule();
            ret.ProjectKey = pjkey;
            ret.ProjectKey = pjkey;
            ret.StartDate = DateTime.Now;
            ret.EndDate = DateTime.Now;

            var plist = BITestData.RetrieveProjectTestDataByWafer(pjkey, wafer);
            RetrieveCummYield(ret, plist);

            return ret;
        }

        public static List<ProjectBIYieldViewModule> GetYieldByDay(string pjkey, string starttime, string endtime)
        {
            var ret = new List<ProjectBIYieldViewModule>();
            var pvm = ProjectViewModels.RetrieveOneProject(pjkey);

            var stime = DateTime.Parse(starttime);
            var etime = DateTime.Parse(endtime);

            var ldate = new List<DateTime>();
            for (var item = stime; item < etime;)
            {
                ldate.Add(item);
                item = item.AddDays(1);
            }

            var startidx = 0;
            if (ldate.Count > 30)
            {
                startidx = ldate.Count - 30;
            }

            for (int idx = startidx; idx < ldate.Count - 1; idx++)
            {
                var temp = GetYieldByDateRange(pjkey, ldate[idx].ToString(), ldate[idx + 1].ToString(), pvm);
                if (temp.LastYields.Count > 0)
                {
                    ret.Add(temp);
                }
            }
            return ret;
        }
    }
}