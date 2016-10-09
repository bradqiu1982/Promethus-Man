using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
    public class BITestDataField
    {
        public string DataID { set; get; }
        public string ChannelNum { set; get; }
        public double SLOPE { set; get; }
        public double THOLD { set; get; }
        public double PO_LD { set; get; }
        public double PO_LD_18 { set; get; }
        public double PO_LD_25 { set; get; }
        public double PO_LD_127 { set; get; }
        public double PO_Uniformity { set; get; }
        public double Delta_SLOPE { set; get; }
        public double Delta_THOLD { set; get; }
        public double Delta_PO_LD { set; get; }
        public double Delta_PO_LD_18 { set; get; }
        public double Delta_PO_LD_25 { set; get; }
        public double Delta_PO_LD_127 { set; get; }
        public double Delta_PO_Uniformity { set; get; }
    }

    public class BITestData
    {
        public string ProjectKey { set; get; }
        public string DataID { set; get; }
        public string ModuleSerialNum { set; get; }
        public string WhichTest { set; get; }
        public string ModuleType { set; get; }
        public string ErrAbbr { set; get; }
        public DateTime TestTimeStamp { set; get; }
        public string TestStation { set; get; }
        public string PN { set; get; }
        public string Wafer { set; get; }
        public string Waferpn { set; get; }

        private List<BITestDataField> df = new List<BITestDataField>();
        public List<BITestDataField> DataFields {
            get
            {
                return df;
            }}


        public void StoreBIData()
        {
            var isql = "insert into BITestData(ProjectKey,DataID,ModuleSerialNum,WhichTest,ModuleType,ErrAbbr,TestTimeStamp,TestStation,PN,UpdateTime,Wafer,Waferpn) values('<ProjectKey>','<DataID>','<ModuleSerialNum>','<WhichTest>','<ModuleType>','<ErrAbbr>','<TestTimeStamp>','<TestStation>','<PN>','<UpdateTime>','<Wafer>','<Waferpn>')";
            isql = isql.Replace("<ProjectKey>", ProjectKey).Replace("<DataID>", DataID).Replace("<ModuleSerialNum>", ModuleSerialNum)
                .Replace("<WhichTest>", WhichTest).Replace("<ModuleType>", ModuleType).Replace("<ErrAbbr>", ErrAbbr)
                .Replace("<TestTimeStamp>", TestTimeStamp.ToString()).Replace("<TestStation>", TestStation).Replace("<PN>", PN)
                .Replace("<UpdateTime>", DateTime.Now.ToString()).Replace("<Wafer>", Wafer).Replace("<Waferpn>", Waferpn);
            DBUtility.ExeLocalSqlNoRes(isql);
        }

        public static Dictionary<string, DateTime> RetrieveAllDataID(string projectkey)
        {
            var ret = new Dictionary<string, DateTime>();
            var sql = "select ModuleSerialNum,WhichTest,TestTimeStamp from BITestData where ProjectKey = '<ProjectKey>'";
            sql = sql.Replace("<ProjectKey>", projectkey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var item in dbret)
            {
                ret.Add(Convert.ToString(item[0])+"_"+ Convert.ToString(item[1]), DateTime.Parse(Convert.ToString(item[2])));
            }
            return ret;
        }

        public static string RetrieveLatestTimeOfLocalBI(string projectkey)
        {
            var sql = "select top 1 UpdateTime from BITestData where ProjectKey = '<ProjectKey>' order by TestTimeStamp DESC";
            sql = sql.Replace("<ProjectKey>", projectkey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            if (dbret.Count > 0)
            {
                return Convert.ToString(dbret[0][0]);
            }
            else
                return null;
        }
    }
}