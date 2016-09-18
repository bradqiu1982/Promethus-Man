using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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

        public static List<ProjectFAViewModules> RetrievePendingFAData(string pjkey)
        {
            var ret = new List<ProjectFAViewModules>();
            var pjdata = ProjectTestData.RetrieveProjectFailedTestData(100000,pjkey);

            var i = 0;
            var issuedict = IssueViewModels.RRetrieveFAByPjkey(pjkey, Resolute.Working);
            foreach (var d in pjdata)
            {
                if (issuedict.ContainsKey(d.DataID))
                {
                    ret.Add(new ProjectFAViewModules(issuedict[d.DataID], d));
                    if (i > 50)
                        break;
                    i = i + 1;
                }
            }

            issuedict = IssueViewModels.RRetrieveFAByPjkey(pjkey, Resolute.Pending);
            foreach (var d in pjdata)
            {
                if (issuedict.ContainsKey(d.DataID))
                {
                    ret.Add(new ProjectFAViewModules(issuedict[d.DataID], d));
                    if (i > 500)
                        break;
                    i = i + 1;
                }
            }

            return ret;
        }

        public static List<ProjectFAViewModules> RetrieveDoneFAData(string pjkey)
        {
            var ret = new List<ProjectFAViewModules>();
            var pjdata = ProjectTestData.RetrieveProjectFailedTestData(100000, pjkey);

            var issuedict = IssueViewModels.RRetrieveFAByPjkey(pjkey, Resolute.Done);
            foreach (var d in pjdata)
            {
                if (issuedict.ContainsKey(d.DataID))
                {
                    ret.Add(new ProjectFAViewModules(issuedict[d.DataID], d));
                }
            }

            return ret;
        }

        public static int RetrieveFADataCount(string pjkey)
        {

            return IssueViewModels.RRetrieveFAStatusByPjkey(pjkey, Resolute.Working)
                + IssueViewModels.RRetrieveFAStatusByPjkey(pjkey, Resolute.Pending);
        }

        public static List<ProjectFAViewModules> RetrieveFADataWithErrAbbr(string ProjectKey, string ErrAbbr)
        {
            var ret = new List<ProjectFAViewModules>();
            var pjdata = ProjectTestData.RetrieveProjectTestDataWithErrAbbr(100000, ProjectKey, ErrAbbr);
            foreach (var d in pjdata)
            {
                var im = IssueViewModels.RetrieveIssueByIssueKey(d.DataID);
                if (im != null)
                {
                    ret.Add(new ProjectFAViewModules(im, d));
                }
            }
            return ret;
        }

    }
}