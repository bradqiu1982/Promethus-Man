using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prometheus.Models
{
    public class ProjectFAViewModules
    {
        public ProjectFAViewModules()
        { }
        public ProjectFAViewModules(IssueViewModels im, ProjectTestData pd)
        {
            IssueData = im;
            TestData = pd;
        }

        public IssueViewModels IssueData { set; get; }
        public ProjectTestData TestData { set; get; }

        public static List<ProjectFAViewModules> RetrievePendingFAData(string pjkey, Controller ctrl)
        {
            var ret = new List<ProjectFAViewModules>();
            var issuedict = IssueViewModels.RRetrieveFAByPjkey(pjkey, Resolute.Working,50,ctrl);
            var issuedict2 = IssueViewModels.RRetrieveFAByPjkey(pjkey, Resolute.Pending, 300,ctrl);
            issuedict.AddRange(issuedict2);

            var bisndict = new Dictionary<string, string>();
            var fasndict = new Dictionary<string, string>();

            foreach (var d in issuedict)
            {
                if (d.Summary.Contains(CRITICALERRORTYPE.SECONDMATCH1) && d.Summary.Contains(CRITICALERRORTYPE.SECONDMATCH))
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(d.ModuleSN) && !string.IsNullOrEmpty(d.ErrAbbr))
                {
                    var pjdata = new ProjectTestData();
                    pjdata.ProjectKey = d.ProjectKey;
                    pjdata.ModuleSerialNum = d.ModuleSN;
                    pjdata.ErrAbbr = d.ErrAbbr;
                    ret.Add(new ProjectFAViewModules(d, pjdata));
                }
                else
                {
                    if (d.Summary.Contains("@Burn-In Step"))
                    {
                        var sn = "";

                        var pjdata = new List<ProjectTestData>();
                        if (!string.IsNullOrEmpty(d.DataID))
                        {
                            pjdata = BITestData.RetrieveProjectTestDataByDataID(d.DataID);
                        }
                        else
                        {
                            pjdata = BITestData.RetrieveProjectTestDataByDataID(d.IssueKey);
                        }

                        if (pjdata.Count > 0)
                        {
                            sn = pjdata[0].ModuleSerialNum;
                        }

                        if (!string.IsNullOrEmpty(sn) && !bisndict.ContainsKey(sn))
                        {
                            bisndict.Add(sn, d.ReportDate.ToString());
                            var pd = new ProjectTestData();
                            pd.ModuleSerialNum = sn;
                            pd.ErrAbbr = pjdata[0].ErrAbbr;
                            pd.ProjectKey = d.ProjectKey;
                            ret.Add(new ProjectFAViewModules(d, pd));
                        }
                        else if (!string.IsNullOrEmpty(sn) && bisndict.ContainsKey(sn))
                        {
                            //close automaticlly
                            IssueViewModels.CloseDupBIIssueAutomaticlly(d.ProjectKey, sn, bisndict[sn]);
                        }
                    }
                    else
                    {

                        var sn = "";
                        var pjdata = new List<ProjectTestData>();
                        if (!string.IsNullOrEmpty(d.DataID))
                        {
                            pjdata = ProjectTestData.RetrieveProjectTestData(d.DataID);
                        }
                        else
                        {
                            pjdata = ProjectTestData.RetrieveProjectTestData(d.IssueKey);
                        }

                        if (pjdata.Count > 0)
                        {
                            sn = pjdata[0].ModuleSerialNum;
                        }

                        if (!string.IsNullOrEmpty(sn) && !fasndict.ContainsKey(sn))
                        {
                            fasndict.Add(sn, d.ReportDate.ToString());
                            ret.Add(new ProjectFAViewModules(d, pjdata[0]));
                        }
                        else if (!string.IsNullOrEmpty(sn) && fasndict.ContainsKey(sn))
                        {
                            //close automaticlly
                            IssueViewModels.CloseDupIssueAutomaticlly(d.ProjectKey, sn, fasndict[sn]);
                        }

                    }
                }
            }

            return ret;
        }

        public static List<ProjectFAViewModules> RetrieveDoneFAData(string pjkey, Controller ctrl)
        {
            var ret = new List<ProjectFAViewModules>();

            var issuedict = IssueViewModels.RRetrieveFAByPjkey(pjkey, Resolute.Done, 500,ctrl);
            foreach (var d in issuedict)
            {
                    if (d.CommentList.Count == 2)
                    {
                        bool sameas = false;
                        foreach (var com in d.CommentList)
                        {
                            if (com.Comment.Contains("<p>Issue Same As <a"))
                            {
                                sameas = true;
                                break;
                            }

                            if (com.Comment.Contains("passed")
                                && string.Compare(com.Reporter, "System", true) == 0)
                            {
                                sameas = true;
                                break;
                            }
                        }
                        if (sameas)
                        {
                            continue;
                        }
                    }

                    if (d.Summary.Contains(CRITICALERRORTYPE.SECONDMATCH1) && d.Summary.Contains(CRITICALERRORTYPE.SECONDMATCH))
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(d.ModuleSN) && !string.IsNullOrEmpty(d.ErrAbbr))
                    {
                        var pjdata = new ProjectTestData();
                        pjdata.ProjectKey = d.ProjectKey;
                        pjdata.ModuleSerialNum = d.ModuleSN;
                        pjdata.ErrAbbr = d.ErrAbbr;
                        ret.Add(new ProjectFAViewModules(d, pjdata));
                    }
                    else
                    {
                        if (d.Summary.Contains("@Burn-In Step"))
                        {
                            var pjdata = BITestData.RetrieveProjectTestDataByDataID(d.IssueKey);
                            if (pjdata.Count > 0)
                            {
                                var pd = new ProjectTestData();
                                pd.ModuleSerialNum = pjdata[0].ModuleSerialNum;
                                pd.ErrAbbr = pjdata[0].ErrAbbr;
                                pd.ProjectKey = d.ProjectKey;
                                ret.Add(new ProjectFAViewModules(d, pd));
                            }
                        }
                        else
                        {
                            var pjdata = ProjectTestData.RetrieveProjectTestData(d.IssueKey);
                            if (pjdata.Count > 0)
                            {
                                ret.Add(new ProjectFAViewModules(d, pjdata[0]));
                            }
                        }
                    }
                }

            return ret;

        }

        public static int RetrieveFADataCount(string pjkey,bool pending=true)
        {
            if (pending)
            {
                return IssueViewModels.RRetrieveFAStatusByPjkey(pjkey, Resolute.Working)
                                + IssueViewModels.RRetrieveFAStatusByPjkey(pjkey, Resolute.Pending);
            }
            else
            {
                return IssueViewModels.RRetrieveFAStatusByPjkey(pjkey, Resolute.Done);
            }
        }

        public static List<ProjectFAViewModules> RetrieveFADataWithErrAbbr(string ProjectKey, string ErrAbbr, Controller ctrl)
        {
            var ret = new List<ProjectFAViewModules>();
            var vm = IssueViewModels.RRetrieveFAByErrAbbr(ProjectKey, ErrAbbr, 500, ctrl);
            foreach (var item in vm)
            {
                var pd = new ProjectTestData();
                pd.ModuleSerialNum = item.ModuleSN;
                pd.ErrAbbr = item.ErrAbbr;
                pd.ProjectKey = item.ProjectKey;
                ret.Add(new ProjectFAViewModules(item, pd));
            }
            return ret;

            //var ret = new List<ProjectFAViewModules>();
            //var pjdata = ProjectTestData.RetrieveProjectTestDataWithErrAbbr(100000, ProjectKey, ErrAbbr);
            //if (pjdata.Count == 0)
            //{
            //    pjdata = BITestData.RetrieveProjectTestDataWithErrAbbr(100000, ProjectKey, ErrAbbr);
            //}


            //foreach (var d in pjdata)
            //{
            //    var im = IssueViewModels.RetrieveIssueByIssueKey(d.DataID,ctrl);
            //    if (im != null && im.Resolution != Resolute.AutoClose)
            //    {
            //        ret.Add(new ProjectFAViewModules(im, d));
            //    }
            //}
            //return ret;
        }

        public static List<ProjectFAViewModules> RetrieveFADataWithSN(string ProjectKey, string sn, Controller ctrl)
        {
            var ret = new List<ProjectFAViewModules>();
            var vm = IssueViewModels.RRetrieveFABySN(ProjectKey, sn, ctrl);
            foreach (var item in vm)
            {
                var pd = new ProjectTestData();
                pd.ModuleSerialNum = item.ModuleSN;
                pd.ErrAbbr = item.ErrAbbr;
                pd.ProjectKey = item.ProjectKey;
                ret.Add(new ProjectFAViewModules(item, pd));
            }
            return ret;

            //var ret = new List<ProjectFAViewModules>();
            //var pjdata = ProjectTestData.RetrieveProjectTestDataWithSN(100000, ProjectKey, SN);
            //var pjdata1 = BITestData.RetrieveProjectTestDataWithSN(100000, ProjectKey, SN);
            //if (pjdata1.Count > 0) {
            //    pjdata.AddRange(pjdata1);
            //}

            ////if (pjdata.Count == 0)
            ////{
            ////    pjdata = BITestData.RetrieveProjectTestDataWithSN(100000, ProjectKey, SN);
            ////}

            //foreach (var d in pjdata)
            //{
            //    var im = IssueViewModels.RetrieveIssueByIssueKey(d.DataID,ctrl);
            //    if (im != null)
            //    {
            //        ret.Add(new ProjectFAViewModules(im, d));
            //    }
            //}
            //return ret;
        }

    }
}