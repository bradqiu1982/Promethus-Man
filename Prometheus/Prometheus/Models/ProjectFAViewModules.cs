﻿using System;
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
            var issuedict = IssueViewModels.RRetrieveFAByPjkey(pjkey, Resolute.Working, 50, ctrl);
            var issuedict2 = IssueViewModels.RRetrieveFAByPjkey(pjkey, Resolute.Pending, 300, ctrl);
            issuedict.AddRange(issuedict2);

            var bisndict = new Dictionary<string, string>();
            var fasndict = new Dictionary<string, string>();

            foreach (var d in issuedict)
            {
                if (d.Summary.Contains(CRITICALERRORTYPE.SECONDMATCH1) || d.Summary.Contains(CRITICALERRORTYPE.SECONDMATCH))
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(d.ModuleSN) && !string.IsNullOrEmpty(d.ErrAbbr))
                {
                    var pd = new ProjectTestData();
                    pd.ModuleSerialNum = d.ModuleSN;
                    pd.ErrAbbr = d.ErrAbbr;
                    pd.ProjectKey = d.ProjectKey;
                    pd.TestStation = string.IsNullOrEmpty(d.TestStation) ? string.Empty : d.TestStation;
                    ret.Add(new ProjectFAViewModules(d, pd));
                }
                else
                {
                    if (d.Summary.Contains("@Burn-In Step"))
                    {
                        var sn = string.IsNullOrEmpty(d.ModuleSerialNum) ? string.Empty : d.ModuleSerialNum;

                        if (!string.IsNullOrEmpty(sn))
                        {
                            if (!bisndict.ContainsKey(sn))
                            {
                                bisndict.Add(sn, d.ReportDate.ToString());
                                var pd = new ProjectTestData();
                                pd.ModuleSerialNum = sn;
                                pd.ErrAbbr = d.tErrAbbr;
                                pd.ProjectKey = d.ProjectKey;
                                pd.TestStation = string.IsNullOrEmpty(d.TestStation) ? string.Empty : d.TestStation; ;
                                ret.Add(new ProjectFAViewModules(d, pd));
                            }
                            else
                            {
                                //close automaticlly
                                IssueViewModels.CloseDupBIIssueAutomaticlly(d.ProjectKey, sn, bisndict[sn]);
                            }
                        }
                    }
                    else
                    {
                        var sn = string.IsNullOrEmpty(d.ModuleSerialNum) ? string.Empty : d.ModuleSerialNum;
                        if (!string.IsNullOrEmpty(sn))
                        {
                            if (!fasndict.ContainsKey(sn))
                            {
                                fasndict.Add(sn, d.ReportDate.ToString());
                                var pd = new ProjectTestData();
                                pd.ModuleSerialNum = sn;
                                pd.ErrAbbr = d.ErrAbbr;
                                pd.ProjectKey = d.ProjectKey;
                                pd.TestStation = string.IsNullOrEmpty(d.TestStation) ? string.Empty : d.TestStation;
                                ret.Add(new ProjectFAViewModules(d, pd));
                            }
                            else
                            {
                                //close automaticlly
                                IssueViewModels.CloseDupIssueAutomaticlly(d.ProjectKey, sn, fasndict[sn]);
                            }
                        }
                    }
                }
            }
            return ret;
        }

        public static List<ProjectFAViewModules> RetrieveDoneFAData(string pjkey, Controller ctrl)
        {
            var ret = new List<ProjectFAViewModules>();

            var issuedict = IssueViewModels.RRetrieveFAByPjkey(pjkey, Resolute.Done, 500, ctrl);
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

                if (d.Summary.Contains(CRITICALERRORTYPE.SECONDMATCH1) || d.Summary.Contains(CRITICALERRORTYPE.SECONDMATCH))
                {
                    continue;
                }

                var sn = string.IsNullOrEmpty(d.ModuleSerialNum) ? string.Empty : d.ModuleSerialNum;
                var testStation = string.IsNullOrEmpty(d.TestStation) ? string.Empty : d.TestStation;

                var pd = new ProjectTestData();
                pd.ModuleSerialNum = !string.IsNullOrEmpty(d.ModuleSN) ? d.ModuleSN : sn;
                pd.ErrAbbr = !string.IsNullOrEmpty(d.ErrAbbr) ? d.ErrAbbr : d.TestStation;
                pd.ProjectKey = d.ProjectKey;
                pd.TestStation = testStation;
                ret.Add(new ProjectFAViewModules(d, pd));
            }

            return ret;

        }

        public static int RetrieveFADataCount(string pjkey, bool pending = true)
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
                var testStation = "";
                var pjdata = ProjectTestData.RetrieveProjectTestData(item.IssueKey);
                if (pjdata.Count > 0)
                {
                    testStation = pjdata[0].TestStation;
                }

                var pd = new ProjectTestData();
                pd.ModuleSerialNum = item.ModuleSN;
                pd.ErrAbbr = item.ErrAbbr;
                pd.ProjectKey = item.ProjectKey;
                pd.TestStation = testStation;
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
                var testStation = "";
                var pjdata = ProjectTestData.RetrieveProjectTestData(item.IssueKey);
                if (pjdata.Count > 0)
                {
                    testStation = pjdata[0].TestStation;
                }

                var pd = new ProjectTestData();
                pd.ModuleSerialNum = item.ModuleSN;
                pd.ErrAbbr = item.ErrAbbr;
                pd.ProjectKey = item.ProjectKey;
                pd.TestStation = testStation;
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