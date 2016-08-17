using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{

    public class TestYield
    { 
        public string WhichTest { set; get; }
        public int InputCount { set; get; }
        public int OutputCount { set; get; }
        public double Yield
        {
            get
            {
                return OutputCount / (double)(InputCount);
            }
        }
    }

    public class ProjectYieldViewModule
    {
        public string ProjectKey { set; get; }
        public DateTime StartDate { set; get; }
        public DateTime EndDate { set; get; }

        private List<TestYield> fyield = new List<TestYield>();
        public List<TestYield> FirstYields { get { return fyield; } }

        public double FirstYield
        {
            get
            {
                var ret = 1.0;
                foreach (var item in FirstYields)
                {
                    ret = ret * item.Yield;
                }
                return ret;
            }
        }

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

        private Dictionary<string, Dictionary<string, int>> femap = new Dictionary<string, Dictionary<string, int>>();
        public Dictionary<string, Dictionary<string, int>> FErrorMap { get { return femap; } }

        private Dictionary<string, Dictionary<string, int>> lemap = new Dictionary<string, Dictionary<string, int>>();
        public Dictionary<string, Dictionary<string, int>> LErrorMap { get { return lemap; } }

        public static void RegisterError(string errorcode, string whichtest, Dictionary<string, Dictionary<string, int>> emap)
        {
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

        private static DateTime RetrieveFirstWeek(string startdate)
        {
            var sdate = DateTime.Parse(DateTime.Parse(startdate).ToString("yyyy-MM-dd") + " 00:00:01");
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
            var sdate = DateTime.Parse(DateTime.Parse(startdate).ToString("yyyy-MM-dd") + " 00:00:01");
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

        private List<DateTime> RetrieveDateSpanByMonth(string startdate,string enddate)
        {
            var ret = new List<DateTime>();
            var sdate = DateTime.Parse(DateTime.Parse(startdate).ToString("yyyy-MM-dd") + " 00:00:01");
            ret.Add(sdate);

            var temptimepoint = sdate;
            var edate = DateTime.Parse(enddate);
            while (temptimepoint < edate)
            {
                temptimepoint = temptimepoint.AddMonths(1);
                if (temptimepoint > edate)
                {
                    ret.Add(DateTime.Parse(DateTime.Parse(enddate).ToString("yyyy-MM-dd") + " 23:59:59"));
                    return ret;
                }
                else
                {
                    ret.Add(DateTime.Parse(temptimepoint.ToString("yyyy-MM-dd") + " 23:59:59"));
                }
            }
            return ret;
        }

        private static void RetrieveFirstYield(ProjectYieldViewModule pyvm, string pjkey, string startdate, string enddate, ProjectViewModels pvm)
        {
            var plist = ProjectTestData.RetrieveProjectTestData(pjkey, startdate, enddate,true);
            var yielddict = new Dictionary<string, TestYield>();
            var sndict = new Dictionary<string, bool>();
            foreach (var p in plist)
            {
                if (!sndict.ContainsKey(p.WhichTest+":"+p.ModuleSerialNum))
                {
                    sndict.Add(p.WhichTest + ":" + p.ModuleSerialNum, true);
                    if (yielddict.ContainsKey(p.WhichTest))
                    {
                        yielddict[p.WhichTest].InputCount = yielddict[p.WhichTest].InputCount + 1;
                        if (string.Compare(p.ErrAbbr, "PASS", true) == 0)
                            yielddict[p.WhichTest].OutputCount = yielddict[p.WhichTest].OutputCount + 1;
                        RegisterError(p.ErrAbbr, p.WhichTest,pyvm.FErrorMap);
                    }
                    else
                    {
                        var tempyield = new TestYield();
                        tempyield.InputCount = 1;
                        if (string.Compare(p.ErrAbbr, "PASS", true) == 0)
                            tempyield.OutputCount = 1;
                        else
                            tempyield.OutputCount = 0;
                        tempyield.WhichTest = p.WhichTest;

                        RegisterError(p.ErrAbbr, p.WhichTest, pyvm.FErrorMap);
                        yielddict.Add(p.WhichTest, tempyield);
                    }
                }
            }

            foreach (var s in pvm.StationList)
            {
                if (yielddict.ContainsKey(s.Station))
                {
                    pyvm.FirstYields.Add(yielddict[s.Station]);
                }
            }
        }

        private static void RetrieveCummYield(ProjectYieldViewModule pyvm, string pjkey, string startdate, string enddate, ProjectViewModels pvm)
        {
            var plist = ProjectTestData.RetrieveProjectTestData(pjkey, startdate, enddate, false);
            var yielddict = new Dictionary<string, TestYield>();
            var sndict = new Dictionary<string, bool>();
            foreach (var p in plist)
            {
                if (!sndict.ContainsKey(p.WhichTest + ":" + p.ModuleSerialNum))
                {
                    sndict.Add(p.WhichTest + ":" + p.ModuleSerialNum, true);
                    if (yielddict.ContainsKey(p.WhichTest))
                    {
                        yielddict[p.WhichTest].InputCount = yielddict[p.WhichTest].InputCount + 1;
                        if (string.Compare(p.ErrAbbr, "PASS", true) == 0)
                            yielddict[p.WhichTest].OutputCount = yielddict[p.WhichTest].OutputCount + 1;
                        RegisterError(p.ErrAbbr, p.WhichTest, pyvm.LErrorMap);
                    }
                    else
                    {
                        var tempyield = new TestYield();
                        tempyield.InputCount = 1;
                        if (string.Compare(p.ErrAbbr, "PASS", true) == 0)
                            tempyield.OutputCount = 1;
                        else
                            tempyield.OutputCount = 0;
                        tempyield.WhichTest = p.WhichTest;

                        RegisterError(p.ErrAbbr, p.WhichTest, pyvm.LErrorMap);
                        yielddict.Add(p.WhichTest, tempyield);
                    }
                }
            }

            foreach (var s in pvm.StationList)
            {
                if (yielddict.ContainsKey(s.Station))
                {
                    pyvm.LastYields.Add(yielddict[s.Station]);
                }
            }
        }

        private static ProjectYieldViewModule GetYieldByDateRange(string pjkey, string startdate, string enddate, ProjectViewModels pvm)
        {
            var ret = new ProjectYieldViewModule();
            ret.ProjectKey = pjkey;
            ret.StartDate = DateTime.Parse(startdate);
            ret.EndDate = DateTime.Parse(enddate);
            RetrieveFirstYield(ret, pjkey, startdate, enddate, pvm);
            RetrieveCummYield(ret, pjkey, startdate, enddate, pvm);
            return ret;
        }

        public static List<ProjectYieldViewModule> GetYieldByWeeks(string pjkey)
        {
            var ret = new List<ProjectYieldViewModule>();

            var pvm = ProjectViewModels.RetrieveOneProject(pjkey);
            var  ldate = RetrieveDateSpanByWeek(pvm.StartDate.ToString(), DateTime.Now.ToString());
            for(int idx  = 0;idx < ldate.Count -1;idx++)
            {
                var temp  = GetYieldByDateRange(pjkey, ldate[idx].ToString(), ldate[idx+1].ToString(),pvm);
                if (temp.FirstYields.Count > 0)
                {
                    ret.Add(temp);
                }
            }
            return ret;
        }

        //public List<ProjectYieldViewModule> GetYieldByWeeks(string pjkey, string startdate, string enddate)
        //{

        //}

        //public List<ProjectYieldViewModule> GetYieldByMonth(string pjkey, string startdate, string enddate)
        //{

        //}
    }
}