using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;

namespace Prometheus.Models
{
    public class YIELDTYPE
    {
        public static string BR = "BR";
        public static string JO = "JO";
        public static string PN = "PN";
    }

    public class TestDataErrorSum
    {
        public TestDataErrorSum()
        {
            sndict = new Dictionary<string, bool>();
            whichtestdict = new Dictionary<string, int>();
        }

        public Dictionary<string, bool> sndict { set; get; }
        public Dictionary<string, int> whichtestdict { set; get; }
    }

    public class TestYield
    { 
        public string WhichTest { set; get; }
        public int InputCount { set; get; }
        public int OutputCount { set; get; }
        public double Yield
        {
            get
            {
                try
                {
                    return OutputCount / (double)(InputCount);
                }
                catch (Exception)
                { return 0.0; }
            }
        }

        public int CorrectOutputCount { set; get; }
        public double CorrectYield
        {
            get
            {
                try
                {
                    return CorrectOutputCount / (double)(InputCount);
                }
                catch (Exception)
                { return 0.0; }
            }
        }

        private Dictionary<string, bool> allsndict = new Dictionary<string, bool>();
        public Dictionary<string, bool> AllSNDict { get { return allsndict; } }


        private Dictionary<string, bool> errsndict = new Dictionary<string, bool>();
        public Dictionary<string, bool> ErrSNDict { get { return errsndict; } }

        private Dictionary<string, bool> corsndict = new Dictionary<string, bool>();
        public Dictionary<string, bool> CorSNDict { get { return corsndict; } }

        public string StartDate { set; get; }
        public string EndDate { set; get; }
    }

    public class ProjectYieldViewModule
    {
        public string ProjectKey { set; get; }
        public DateTime StartDate { set; get; }
        public DateTime EndDate { set; get; }

        public string XKey { set; get; }

        private List<TestYield> snyield = new List<TestYield>();
        public List<TestYield> SNYields { get { return snyield; } }
        public double SNYield
        {
            get
            {
                if (SNYields.Count == 0)
                    return 0.0;

                var ret = 1.0;
                foreach (var item in SNYields)
                {
                    ret = ret * item.Yield;
                }
                return ret;
            }
        }

        private List<TestYield> rltimeyield = new List<TestYield>();
        public List<TestYield> RealTimeYields { get { return rltimeyield; } }
        public double RealTimeYield
        {
            get
            {
                if (RealTimeYields.Count == 0)
                    return 0.0;

                var ret = 1.0;
                foreach (var item in RealTimeYields)
                {
                    ret = ret * item.Yield;
                }
                return ret;
            }
        }




        private List<TestYield> fyield = new List<TestYield>();
        public List<TestYield> FirstYields { get { return fyield; } }

        public double FirstYield
        {
            get
            {
                if (FirstYields.Count == 0)
                    return 0.0;

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
                if (LastYields.Count == 0)
                    return 0.0;

                var ret = 1.0;
                foreach (var item in LastYields)
                {
                    ret = ret * item.Yield;
                }
                return ret;
            }
        }

        private Dictionary<string, TestDataErrorSum> femap = new Dictionary<string, TestDataErrorSum>();
        public Dictionary<string, TestDataErrorSum> FErrorMap { get { return femap; } }

        private Dictionary<string, TestDataErrorSum> lemap = new Dictionary<string, TestDataErrorSum>();
        public Dictionary<string, TestDataErrorSum> LErrorMap { get { return lemap; } }

        private Dictionary<string, TestDataErrorSum> semap = new Dictionary<string, TestDataErrorSum>();
        public Dictionary<string, TestDataErrorSum> SNErrorMap { get { return semap; } }

        private Dictionary<string, TestDataErrorSum> rtmap = new Dictionary<string, TestDataErrorSum>();
        public Dictionary<string, TestDataErrorSum> RLTErrorMap { get { return rtmap; } }

        //public static void RegisterError(string errorcode1, string whichtest,string SN, Dictionary<string, TestDataErrorSum> emap)
        //{
        //    var errorcode = errorcode1;
        //    if (string.Compare(errorcode, "pass", true) == 0)
        //        errorcode = "PASS";

        //    if (emap.ContainsKey(errorcode))
        //    {
        //        var tempdict = emap[errorcode];
        //        if (tempdict.whichtestdict.ContainsKey(whichtest))
        //        {
        //            tempdict.whichtestdict[whichtest] = tempdict.whichtestdict[whichtest] + 1;
        //        }
        //        else
        //        {
        //            tempdict.whichtestdict.Add(whichtest, 1);
        //        }

        //        if (!tempdict.sndict.ContainsKey(SN))
        //        {
        //            tempdict.sndict.Add(SN, true);
        //        }
        //    }
        //    else
        //    {
        //        var errsum = new TestDataErrorSum();
        //        errsum.whichtestdict.Add(whichtest, 1);
        //        emap.Add(errorcode, errsum);

        //        if (!errsum.sndict.ContainsKey(SN))
        //        {
        //            errsum.sndict.Add(SN, true);
        //        }
        //    }
        //}

        public static void RegisterError(ProjectTestData testdata, Dictionary<string, TestDataErrorSum> emap)
        {
            string errorcode1 = testdata.ErrAbbr;
            string whichtest = testdata.WhichTest;
            string SN = testdata.ModuleSerialNum;

            var errorcode = errorcode1;
            if (string.Compare(errorcode, "pass", true) == 0)
                errorcode = "PASS";

            if (emap.ContainsKey(errorcode))
            {
                var tempdict = emap[errorcode];
                if (tempdict.whichtestdict.ContainsKey(whichtest))
                {
                    tempdict.whichtestdict[whichtest] = tempdict.whichtestdict[whichtest] + 1;
                }
                else
                {
                    tempdict.whichtestdict.Add(whichtest, 1);
                }

                if (!tempdict.sndict.ContainsKey(SN))
                {
                    tempdict.sndict.Add(SN, true);
                }
            }
            else
            {
                var errsum = new TestDataErrorSum();
                errsum.whichtestdict.Add(whichtest, 1);
                emap.Add(errorcode, errsum);

                if (!errsum.sndict.ContainsKey(SN))
                {
                    errsum.sndict.Add(SN, true);
                }
            }
        }

        public static int RetrieveErrorCount(string errorcode, string whichtest, Dictionary<string, TestDataErrorSum> emap)
        {
            if (emap.ContainsKey(errorcode))
            {
                var tempdict = emap[errorcode];
                if (tempdict.whichtestdict.ContainsKey(whichtest))
                {
                    return tempdict.whichtestdict[whichtest];
                }
                return 0;
            }

            return 0;
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

        private static List<DateTime> _RetrieveDateSpanByWeek(string startdate, string enddate)
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
                    if (DateTime.Parse(enddate).DayOfWeek == DayOfWeek.Thursday)
                    {
                        ret.Add(DateTime.Parse(DateTime.Parse(enddate).ToString("yyyy-MM-dd") + " 07:30:00"));
                    }
                    else
                    {
                        ret.Add(DateTime.Parse(enddate));
                    }

                    return ret;
                }
                else
                {
                    ret.Add(DateTime.Parse(temptimepoint.ToString("yyyy-MM-dd") + " 07:30:00"));
                }
            }

            return ret;
        }

        private static List<DateTime> RetrieveDateSpanByWeek(string startdate, string enddate)
        {
            var ret = _RetrieveDateSpanByWeek(startdate, enddate);
            if(ret.Count > 2)
            {
                var end1 = ret[ret.Count - 2];
                var end2 = ret[ret.Count - 1];
                if (string.Compare(end1.ToString("yyyy-MM-dd"), end2.ToString("yyyy-MM-dd")) == 0)
                {
                    var lastidx = ret.Count - 1;
                    ret.RemoveAt(lastidx);
                }
            }
            return ret;
        }

        public static List<DateTime> RetrieveDateSpanByMonth(string startdate,string enddate)
        {
            var ret = new List<DateTime>();
            var sdate = DateTime.Parse(DateTime.Parse(startdate).ToString("yyyy-MM") + "-01 00:00:00");
            
            var temptimepoint = sdate;
            var edate = DateTime.Parse(DateTime.Parse(enddate).ToString("yyyy-MM-dd") + " 23:59:59");

            ret.Add(sdate);

            while (true)
            {
                temptimepoint = temptimepoint.AddMonths(1);
                if (temptimepoint > edate)
                {
                    ret.Add(edate);
                    break;
                }
                ret.Add(temptimepoint);
            }

            return ret;
        }

        private static void RetrieveRealTimeYield(ProjectYieldViewModule pyvm, List<ProjectTestData> plist, ProjectViewModels pvm)
        {

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
                        RegisterError(p, pyvm.RLTErrorMap);
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

                        yielddict.Add(p.WhichTest, tempyield);
                        RegisterError(p, pyvm.RLTErrorMap);
                    }
                }
            }

            if (pvm.SumDatasetList.Count > 0)
            {
                foreach (var s in pvm.SumDatasetList)
                {
                    if (yielddict.ContainsKey(s.Station) && yielddict[s.Station].InputCount > 1)
                    {
                        pyvm.RealTimeYields.Add(yielddict[s.Station]);
                    }
                }
            }
            else if (pvm.StationList.Count > 0)
            {
                foreach (var s in pvm.StationList)
                {
                    if (yielddict.ContainsKey(s.Station) && yielddict[s.Station].InputCount > 4)
                    {
                        pyvm.RealTimeYields.Add(yielddict[s.Station]);
                    }
                }
            }

        }

        private static void RetrieveSNYield(ProjectYieldViewModule pyvm, List<ProjectTestData> plist, ProjectViewModels pvm)
        {

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
                        RegisterError(p, pyvm.SNErrorMap);
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

                        yielddict.Add(p.WhichTest, tempyield);
                        RegisterError(p, pyvm.SNErrorMap);
                    }
                }
            }

            if (pvm.SumDatasetList.Count > 0)
            {
                foreach (var s in pvm.SumDatasetList)
                {
                    if (yielddict.ContainsKey(s.Station) && yielddict[s.Station].InputCount > 1)
                    {
                        pyvm.SNYields.Add(yielddict[s.Station]);
                    }
                }
            }
            else if (pvm.StationList.Count > 0)
            {
                foreach (var s in pvm.StationList)
                {
                    if (yielddict.ContainsKey(s.Station) && yielddict[s.Station].InputCount > 4)
                    {
                        pyvm.SNYields.Add(yielddict[s.Station]);
                    }
                }
            }


        }

        private static void RetrieveFirstYield(ProjectYieldViewModule pyvm, List<ProjectTestData> plist, ProjectViewModels pvm,string sdate = null,string edate = null)
        {

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
                        RegisterError(p,pyvm.FErrorMap);
                    }
                    else
                    {
                        var tempyield = new TestYield();
                        tempyield.StartDate = sdate;
                        tempyield.EndDate = edate;

                        tempyield.InputCount = 1;
                        if (string.Compare(p.ErrAbbr, "PASS", true) == 0)
                            tempyield.OutputCount = 1;
                        else
                            tempyield.OutputCount = 0;
                        tempyield.WhichTest = p.WhichTest;

                        RegisterError(p, pyvm.FErrorMap);
                        yielddict.Add(p.WhichTest, tempyield);
                    }
                }
            }

            if (pvm.SumDatasetList.Count > 0)
            {
                foreach (var s in pvm.SumDatasetList)
                {
                    if (yielddict.ContainsKey(s.Station) && yielddict[s.Station].InputCount > 1)
                    {
                        pyvm.FirstYields.Add(yielddict[s.Station]);
                    }
                }
            }
            else if (pvm.StationList.Count > 0)
            {
                foreach (var s in pvm.StationList)
                {
                    if (yielddict.ContainsKey(s.Station) && yielddict[s.Station].InputCount > 4)
                    {
                        pyvm.FirstYields.Add(yielddict[s.Station]);
                    }
                }
            }


        }

        private Dictionary<string, int> ffailuretimedist = new Dictionary<string, int>();
        public Dictionary<string, int> FirstYieldFailureTimeDist { get { return ffailuretimedist; } }

        private static void RetrieveFirstYield(ProjectYieldViewModule pyvm, List<ProjectTestData> plist)
        {

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
                        {
                            yielddict[p.WhichTest].OutputCount = yielddict[p.WhichTest].OutputCount + 1;
                        }
                        else
                        {
                            var key = p.TestTimeStamp.ToString("yyyy-MM-dd") + "##" + p.ErrAbbr;
                            var tempdict = pyvm.FirstYieldFailureTimeDist;
                            if (tempdict.ContainsKey(key))
                            {
                                tempdict[key] = tempdict[key] + 1;
                            }
                            else
                            {
                                tempdict.Add(key, 1);
                            }
                        }

                        RegisterError(p, pyvm.FErrorMap);
                    }
                    else
                    {
                        var tempyield = new TestYield();
                        tempyield.InputCount = 1;
                        if (string.Compare(p.ErrAbbr, "PASS", true) == 0)
                        {
                            tempyield.OutputCount = 1;
                        }
                        else
                        { 
                            tempyield.OutputCount = 0;

                            var key = p.TestTimeStamp.ToString("yyyy-MM-dd") + "##" + p.ErrAbbr;
                            var tempdict = pyvm.FirstYieldFailureTimeDist;
                            if (tempdict.ContainsKey(key))
                            {
                                tempdict[key] = tempdict[key] + 1;
                            }
                            else
                            {
                                tempdict.Add(key, 1);
                            }
                        }

                        tempyield.WhichTest = p.WhichTest;

                        RegisterError(p, pyvm.FErrorMap);
                        yielddict.Add(p.WhichTest, tempyield);
                    }
                }
            }

            foreach (var kv in yielddict)
            {
                pyvm.FirstYields.Add(kv.Value);
            }

        }

        private static void RetrieveCummYield(ProjectYieldViewModule pyvm, List<ProjectTestData> plist, ProjectViewModels pvm, string sdate = null, string edate = null)
        {
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
                        RegisterError(p, pyvm.LErrorMap);
                    }
                    else
                    {
                        var tempyield = new TestYield();
                        tempyield.StartDate = sdate;
                        tempyield.EndDate = edate;

                        tempyield.InputCount = 1;
                        if (string.Compare(p.ErrAbbr, "PASS", true) == 0)
                            tempyield.OutputCount = 1;
                        else
                            tempyield.OutputCount = 0;
                        tempyield.WhichTest = p.WhichTest;

                        RegisterError(p, pyvm.LErrorMap);
                        yielddict.Add(p.WhichTest, tempyield);
                    }
                }
            }

            if (pvm.SumDatasetList.Count > 0)
            {
                foreach (var s in pvm.SumDatasetList)
                {
                    if (yielddict.ContainsKey(s.Station) && yielddict[s.Station].InputCount > 1)
                    {
                        pyvm.LastYields.Add(yielddict[s.Station]);
                    }
                }
            }
            else if (pvm.StationList.Count > 0)
            {
                foreach (var s in pvm.StationList)
                {
                    if (yielddict.ContainsKey(s.Station) && yielddict[s.Station].InputCount > 4)
                    {
                        pyvm.LastYields.Add(yielddict[s.Station]);
                    }
                }
            }
        }

        private Dictionary<string,int> flfailuretimedist = new Dictionary<string, int>();
        public Dictionary<string, int> FinalYieldFailureTimeDist { get { return flfailuretimedist; } }

        private static void RetrieveCummYield(ProjectYieldViewModule pyvm, List<ProjectTestData> plist)
        {
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
                        {
                            yielddict[p.WhichTest].OutputCount = yielddict[p.WhichTest].OutputCount + 1;
                        }
                        else
                        {
                            var key = p.TestTimeStamp.ToString("yyyy-MM-dd") + "##" + p.ErrAbbr;
                            var tempdict = pyvm.FinalYieldFailureTimeDist;
                            if (tempdict.ContainsKey(key))
                            {
                                tempdict[key] = tempdict[key] + 1;
                            }
                            else
                            {
                                tempdict.Add(key, 1);
                            }
                        }

                        RegisterError(p, pyvm.LErrorMap);
                    }
                    else
                    {
                        var tempyield = new TestYield();
                        tempyield.InputCount = 1;
                        if (string.Compare(p.ErrAbbr, "PASS", true) == 0) { 
                            tempyield.OutputCount = 1;
                        }
                        else{ 
                            tempyield.OutputCount = 0;

                            var key = p.TestTimeStamp.ToString("yyyy-MM-dd") + "##" + p.ErrAbbr;
                            var tempdict = pyvm.FinalYieldFailureTimeDist;
                            if (tempdict.ContainsKey(key))
                            {
                                tempdict[key] = tempdict[key] + 1;
                            }
                            else
                            {
                                tempdict.Add(key, 1);
                            }
                        }

                        tempyield.WhichTest = p.WhichTest;
                        RegisterError(p, pyvm.LErrorMap);
                        yielddict.Add(p.WhichTest, tempyield);
                    }
                }
            }

            foreach (var kv in yielddict)
            {
                pyvm.LastYields.Add(kv.Value);
            }
        }

        public static ProjectYieldViewModule GetYieldByDateRange(string pjkey, string sdate, string edate, ProjectViewModels pvm,Cache mycache)
        {
            if (mycache != null)
            {
                var ckey = pjkey + "_" + DateTime.Parse(sdate).ToString("yyyy-MM-dd HH:mm:ss") + "_" + DateTime.Parse(edate).ToString("yyyy-MM-dd HH:mm:ss") + "_CUST";
                var vm = mycache.Get(ckey);
                if (vm != null)
                {
                    return (ProjectYieldViewModule)vm;
                }
            }

            var ret = new ProjectYieldViewModule();
            ret.ProjectKey = pjkey;
            ret.StartDate = DateTime.Parse(sdate);
            ret.EndDate = DateTime.Parse(edate);

            var startdate = DateTime.Parse(DateTime.Parse(sdate).ToString("yyyy-MM-dd") + " 07:30:00").ToString();
            var enddate = DateTime.Parse(edate).ToString("yyyy-MM-dd HH:mm:ss");

            if (string.Compare(startdate,enddate) == 0)
            {
                return ret;
            }

            var datatfromstart = ProjectTestData.RetrieveProjectTestData(pjkey, startdate, DateTime.Parse(enddate).AddYears(5).ToString("yyyy-MM-dd HH:mm:ss"), false,null);
            if (datatfromstart.Count == 0)
            {
                return ret;
            }


            var datawithstartend = new List<ProjectTestData>();
            var enddatet = DateTime.Parse(enddate);
            foreach (var item in datatfromstart)
            {
                if (item.TestTimeStamp <= enddatet)
                {
                    datawithstartend.Add(item);
                }
            }
            RetrieveRealTimeYield(ret, datawithstartend, pvm);

            
            var reversedatawithstartend = new List<ProjectTestData>();
            var datacount = datawithstartend.Count - 1;
            for (int idx = datacount; idx >= 0; idx--)
            {
                reversedatawithstartend.Add(datawithstartend[idx]);
            }

            var previoussnstationdict = ProjectTestData.RetrieveSNBeforeDateWithStation(pjkey, startdate,mycache);

            var sndict = new Dictionary<string, bool>();
            foreach (var kvpair in previoussnstationdict)
            {
                var endindex = kvpair.Key.LastIndexOf(':');
                var sn = kvpair.Key.Substring(0, endindex);
                if (!sndict.ContainsKey(sn))
                {
                    sndict.Add(sn, true);
                }
            }
            var validatedict4snyield = new Dictionary<string, bool>();
            foreach (var item in reversedatawithstartend)
            {
                if (!sndict.ContainsKey(item.ModuleSerialNum))
                {
                    if (!validatedict4snyield.ContainsKey(item.ModuleSerialNum))
                    {
                        validatedict4snyield.Add(item.ModuleSerialNum, true);
                    }
                }
            }
            var filteredPjData2 = new List<ProjectTestData>();
            foreach (var item in datatfromstart)
            {
                if (validatedict4snyield.ContainsKey(item.ModuleSerialNum))
                {
                        filteredPjData2.Add(item);
                }
            }
            RetrieveSNYield(ret, filteredPjData2, pvm);
            filteredPjData2.Clear();



            var filteredPjData = new List<ProjectTestData>();
            foreach (var item in reversedatawithstartend)
            {
                if (!previoussnstationdict.ContainsKey(item.ModuleSerialNum+":"+item.WhichTest))
                {
                    filteredPjData.Add(item);
                }
            }
            RetrieveFirstYield(ret, filteredPjData, pvm,startdate, enddate);


            filteredPjData2 = new List<ProjectTestData>();
            foreach (var item in datawithstartend)
            {
                if (!previoussnstationdict.ContainsKey(item.ModuleSerialNum + ":" + item.WhichTest))
                {
                    filteredPjData2.Add(item);
                }
            }
            RetrieveCummYield(ret, filteredPjData2, pvm, startdate, enddate);

            datatfromstart.Clear();
            previoussnstationdict.Clear();
            sndict.Clear();
            validatedict4snyield.Clear();
            GC.Collect();
            GC.WaitForPendingFinalizers();

            if (mycache != null)
            {
                var ckey = pjkey + "_" + DateTime.Parse(sdate).ToString("yyyy-MM-dd HH:mm:ss") + "_" + DateTime.Parse(edate).ToString("yyyy-MM-dd HH:mm:ss") + "_CUST";
                var vm = mycache.Get(ckey);
                if (vm == null)
                {
                    mycache.Insert(ckey, ret);//, null, DateTime.Now.AddHours(4), Cache.NoSlidingExpiration);
                }
            }

            return ret;
        }

        public static StringBuilder ProjectRawData(string pjkey, string DataType, string StartDate, string EndDate)
        {
            var startdate = DateTime.Parse(DateTime.Parse(StartDate).ToString("yyyy-MM-dd") + " 07:30:00").ToString();
            var enddate = DateTime.Parse(EndDate).ToString("yyyy-MM-dd HH:mm:ss");

            var datatfromstart = ProjectTestData.RetrieveProjectTestData(pjkey, startdate, DateTime.Parse(enddate).AddYears(5).ToString("yyyy-MM-dd HH:mm:ss"), false, null);
            if (datatfromstart.Count == 0)
            {
                return new StringBuilder();
            }

            var trawdata = new List<ProjectTestData>();

            var datawithstartend = new List<ProjectTestData>();
            var enddatet = DateTime.Parse(enddate);
            foreach (var item in datatfromstart)
            {
                if (item.TestTimeStamp <= enddatet)
                {
                    datawithstartend.Add(item);
                }
            }

            var reversedatawithstartend = new List<ProjectTestData>();
            var datacount = datawithstartend.Count - 1;
            for (int idx = datacount; idx >= 0; idx--)
            {
                reversedatawithstartend.Add(datawithstartend[idx]);
            }

            var previoussnstationdict = ProjectTestData.RetrieveSNBeforeDateWithStation(pjkey, startdate, null);

            if(string.Compare(DataType,"SN") == 0)
            {
                var sndict = new Dictionary<string, bool>();
                foreach (var kvpair in previoussnstationdict)
                {
                    var endindex = kvpair.Key.LastIndexOf(':');
                    var sn = kvpair.Key.Substring(0, endindex);
                    if (!sndict.ContainsKey(sn))
                    {
                        sndict.Add(sn, true);
                    }
                }
                var validatedict4snyield = new Dictionary<string, bool>();
                foreach (var item in reversedatawithstartend)
                {
                    if (!sndict.ContainsKey(item.ModuleSerialNum))
                    {
                        if (!validatedict4snyield.ContainsKey(item.ModuleSerialNum))
                        {
                            validatedict4snyield.Add(item.ModuleSerialNum, true);
                        }
                    }
                }
                var filteredPjData2 = new List<ProjectTestData>();
                foreach (var item in datatfromstart)
                {
                    if (validatedict4snyield.ContainsKey(item.ModuleSerialNum))
                    {
                        filteredPjData2.Add(item);
                    }
                }

                trawdata = filteredPjData2;
            }

            if (string.Compare(DataType, "FPY") == 0)
            {
                var filteredPjData = new List<ProjectTestData>();
                foreach (var item in reversedatawithstartend)
                {
                    if (!previoussnstationdict.ContainsKey(item.ModuleSerialNum + ":" + item.WhichTest))
                    {
                        filteredPjData.Add(item);
                    }
                }
                trawdata = filteredPjData;
            }

            if (string.Compare(DataType, "FY") == 0)
            {
                var filteredPjData2 = new List<ProjectTestData>();
                foreach (var item in datawithstartend)
                {
                    //if (!previoussnstationdict.ContainsKey(item.ModuleSerialNum + ":" + item.WhichTest))
                    {
                        filteredPjData2.Add(item);
                    }
                }
                trawdata = filteredPjData2;
            }

            var rawdata = new List<ProjectTestData>();
            var sntestdict = new Dictionary<string, bool>();
            foreach (var item in trawdata)
            {
                if (!sntestdict.ContainsKey(item.ModuleSerialNum + ":" + item.WhichTest))
                {
                    sntestdict.Add(item.ModuleSerialNum + ":" + item.WhichTest, true);
                    rawdata.Add(item);
                }
            }

            var sb = new StringBuilder((rawdata.Count + 1) * 160);
            foreach (var item in rawdata)
            {
                sb.Append("\""+item.ProjectKey+"\",\""+item.ModuleSerialNum+"\",\""+item.WhichTest+"\",\""+item.ErrAbbr+"\",\""+item.TestTimeStamp.ToString("yyyy-MM-dd HH:mm:ss")+"\",\""+item.TestStation+"\",\""+item.PN+"\",\""+item.JO+"\",\r\n");
            }
            return sb;

        }

        public static ProjectYieldViewModule GetYieldByTestData(List<ProjectTestData> testdata)
        {
            var ret = new ProjectYieldViewModule();

            RetrieveCummYield(ret, testdata);

            var reversedata = new List<ProjectTestData>();
            var datacount = testdata.Count - 1;
            for (int idx = datacount; idx >= 0; idx--)
            {
                reversedata.Add(testdata[idx]);
            }
            RetrieveFirstYield(ret, reversedata);

            return ret;
        }

        public static List<ProjectYieldViewModule> GetBRYieldByTime(string pjkey, string startdate, string enddate, ProjectViewModels pvm,Cache cache)
        {

            var ret = new List<ProjectYieldViewModule>();

            var brdict = new Dictionary<string,bool>();
            var sql = "select distinct APPV1 from ProjectTestData where ProjectKey = @ProjectKey and TestTimeStamp >= @startdate and TestTimeStamp <= @enddate and APPV1 like '%-SBR%'";
            var param = new Dictionary<string, string>();
            param.Add("@ProjectKey", pjkey);
            param.Add("@startdate", startdate);
            param.Add("@enddate", enddate);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, param);
            foreach (var line in dbret)
            {
                var jo = Convert.ToString(line[0]);
                var josplit = jo.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (josplit.Count >= 3 && josplit[1].ToUpper().Contains("SBR"))
                {
                    var sbr = josplit[1].Trim().ToUpper();
                    if (!brdict.ContainsKey(sbr))
                    { brdict.Add(sbr,true); }
                }
            }

            if (brdict.Count > 0)
            {
                var brlist = brdict.Keys.ToList();
                brlist.Sort();

                foreach (var br in brlist)
                {
                    ret.AddRange(GetYieldByBRNum(pjkey,br,pvm, cache, YIELDTYPE.BR));
                }
            }

            return ret;
        }

        public static List<ProjectYieldViewModule> GetYieldByBRNum(string pjkey, string BRNUM, ProjectViewModels pvm, Cache mycache, string yieldtype)
        {
            var retlist = new List<ProjectYieldViewModule>();

            var brs = BRNUM.Split(new string[] { ";", ",", " " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var br in brs)
            {

                var ret = new ProjectYieldViewModule();
                ret.ProjectKey = pjkey;

                var plist = ProjectTestData.RetrieveProjectTestDataByBR(pjkey, br, yieldtype, false, mycache);

                var tplist = new List<ProjectTestData>();
                var datacount = plist.Count - 1;
                for (int idx = datacount; idx >= 0; idx--)
                {
                    tplist.Add(plist[idx]);
                }

                RetrieveFirstYield(ret, tplist, pvm);

                RetrieveCummYield(ret, plist, pvm);

                RetrieveSNYield(ret, plist, pvm);

                if (ret.FirstYields.Count > 0)
                {
                    ret.XKey = br;
                    retlist.Add(ret);
                }
            }

            return retlist;
        }

        public static StringBuilder ProjectBRRawData(string pjkey, string DataType, string BRNUM, string BRType)
        {
            var rawdata = new List<ProjectTestData>();

            var plist = ProjectTestData.RetrieveProjectTestDataByBR(pjkey, BRNUM, BRType, false, null);

            var sndict = new Dictionary<string, bool>();
            if (string.Compare(DataType, "FPY") == 0)
            {
                plist.Reverse();
            }

            foreach (var item in plist)
            {
                if (!sndict.ContainsKey(item.ModuleSerialNum + ":" + item.WhichTest))
                {
                    sndict.Add(item.ModuleSerialNum + ":" + item.WhichTest, true);
                    rawdata.Add(item);
                }
            }

            var sb = new StringBuilder((rawdata.Count + 1) * 160);
            foreach (var item in rawdata)
            {
                sb.Append("\"" + item.ProjectKey + "\",\"" + item.ModuleSerialNum + "\",\"" + item.WhichTest + "\",\"" + item.ErrAbbr + "\",\"" + item.TestTimeStamp.ToString("yyyy-MM-dd HH:mm:ss") + "\",\"" + item.TestStation + "\",\"" + item.PN + "\",\"" + item.JO + "\",\r\n");
            }
            return sb;
        }

        public static List<ProjectYieldViewModule> GetYieldByWeeks(string pjkey,Cache mycache,int weeks)
        {
            var ret = new List<ProjectYieldViewModule>();

            var pvmlist = ProjectViewModels.RetrieveOneProject(pjkey);
            if (pvmlist.Count == 0)
            { return ret; }
            var pvm = pvmlist[0];
            var  ldate = RetrieveDateSpanByWeek(pvm.StartDate.ToString(), DateTime.Now.ToString());

            var startidx = 0;
            if (ldate.Count > weeks)
            {
                startidx = ldate.Count - weeks - 1;
            }

            for(int idx  = startidx; idx < ldate.Count -1;idx++)
            {
                var temp  = GetYieldByDateRange(pjkey, ldate[idx].ToString(), ldate[idx+1].ToString(),pvm,mycache);
                if (temp.RealTimeYields.Count > 0)
                {
                    ret.Add(temp);
                }
            }
            return ret;
        }

        public static List<ProjectYieldViewModule> GetYieldByWeeks(string pjkey,string startdate,string enddate, Cache mycache)
        {
            var ret = new List<ProjectYieldViewModule>();

            var pvmlist = ProjectViewModels.RetrieveOneProject(pjkey);
            if (pvmlist.Count == 0)
            { return ret; }
            var pvm = pvmlist[0];
            var ldate = RetrieveDateSpanByWeek(startdate, enddate);

            var startidx = 0;
            for (int idx = startidx; idx < ldate.Count - 1; idx++)
            {
                var temp = GetYieldByDateRange(pjkey, ldate[idx].ToString(), ldate[idx + 1].ToString(), pvm, mycache);
                if (temp.RealTimeYields.Count > 0)
                {
                    ret.Add(temp);
                }
            }
            return ret;
        }

        public static List<ProjectYieldViewModule> GetYieldByMonth(string pjkey, Cache mycache,int Months)
        {
            var ret = new List<ProjectYieldViewModule>();

            var pvmlist = ProjectViewModels.RetrieveOneProject(pjkey);
            if (pvmlist.Count == 0)
            { return ret; }
            var pvm = pvmlist[0];

            var ldate = RetrieveDateSpanByMonth(pvm.StartDate.ToString(), DateTime.Now.ToString());

            var startidx = 0;
            if (ldate.Count > Months)
            {
                startidx = ldate.Count - Months-1;
            }

            for (int idx = startidx; idx < ldate.Count - 1; idx++)
            {
                var temp = GetYieldByDateRange(pjkey, ldate[idx].ToString(), ldate[idx + 1].AddMinutes(-1).ToString(), pvm,mycache);
                if (temp.RealTimeYields.Count > 0)
                {
                    ret.Add(temp);
                }
            }
            return ret;
        }

        public static List<ProjectYieldViewModule> GetYieldByDay(string pjkey,string starttime,string endtime, Cache mycache)
        {
            var ret = new List<ProjectYieldViewModule>();
            var pvmlist = ProjectViewModels.RetrieveOneProject(pjkey);
            if (pvmlist.Count == 0)
            { return ret; }
            var pvm = pvmlist[0];

            var stime = DateTime.Parse(starttime);
            var etime = DateTime.Parse(endtime);

            var ldate = new List<DateTime>();
            for (var item = stime; item <= etime;)
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
                var temp = GetYieldByDateRange(pjkey, ldate[idx].ToString(), ldate[idx + 1].ToString(), pvm,mycache);
                if (temp.RealTimeYields.Count > 0)
                {
                    ret.Add(temp);
                }
            }
            return ret;
        }

    }
}