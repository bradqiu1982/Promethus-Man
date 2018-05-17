using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
    public class JOMesStatus
    {
        public JOMesStatus() {
            JO = "";
            SN = "";
            WorkFlowStep = "";
            LastMoveDate = DateTime.Parse("1982-05-06 10:00:00");
        }

        public static List<JOMesStatus> RetrieveJOMesStaus(string jo)
        {
            var ret = new List<JOMesStatus>();

            var sql = @" select c.ContainerName,w.WorkflowStepName,cs.LastMoveDate from [InsiteDB].[insite].[Container] c (nolock) 
                         left join[InsiteDB].[insite].[MfgOrder] m(nolock) on m.MfgOrderId = c.MfgOrderId 
                         left join InsiteDB.insite.CurrentStatus cs (nolock) on cs.CurrentStatusId = c.CurrentStatusId 
                         left join InsiteDB.insite.WorkflowStep w (nolock) on w.WorkflowStepId = cs.WorkflowStepId 
                         where m.MfgOrderName = @JOCond order by w.Sequence ASC";
            var dict = new Dictionary<string, string>();
            dict.Add("@JOCond", jo);
            var dbret = DBUtility.ExeRealMESSqlWithRes(sql,dict);
            foreach (var line in dbret)
            {
                var tempvm = new JOMesStatus();
                tempvm.JO = jo;
                tempvm.SN = Convert.ToString(line[0]);
                tempvm.WorkFlowStep = Convert.ToString(line[1]);
                tempvm.LastMoveDate = Convert.ToDateTime(line[2]);
                ret.Add(tempvm);
            }
            return ret;
        }

        public string JO { set; get; }
        public string SN { set; get; }
        public string WorkFlowStep { set; get; }
        public DateTime LastMoveDate { set; get; }
        public string LastMoveDateStr { get { return LastMoveDate.ToString("yyyy-MM-dd HH:mm:ss"); } }
    }
}