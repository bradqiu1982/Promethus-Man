using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
    public class ProjectTestData
    {
        public ProjectTestData()
        { }

        public ProjectTestData(string pk,string did,string sn,string wtest,string mt,string err,string testtime,string station,string p)
        {
            ProjectKey = pk;
            DataID = did;
            ModuleSerialNum = sn;
            WhichTest = wtest;
            ModuleType = mt;
            ErrAbbr = err;
            TestTimeStamp = DateTime.Parse(testtime);
            TestStation = station;
            PN = p;
        }

        public string ProjectKey { set; get; }
        public string DataID { set; get; }
        public string ModuleSerialNum { set; get; }
        public string  WhichTest{ set; get; }
        public string ModuleType { set; get; }
        public string ErrAbbr { set; get; }
        public DateTime TestTimeStamp { set; get; }
        public string TestStation { set; get; }
        public string PN { set; get; }
    }
}