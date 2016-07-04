using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
    public class HtmlRaw
    {
        public string MesgContent {
            set { sMesgContent=value; }
            get {return sMesgContent; }
        }

        private string sMesgContent = "";
        public string dbMesgContent
        {
            get
            {
                return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(sMesgContent));
            }

            set
            {
                sMesgContent = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(value));
            }
        }
    }
    
    public class DashBoardViewModels
    {

            private List<HtmlRaw> activeInfo = new List<HtmlRaw>();
            public List<HtmlRaw> ActiveInfoList
            {
                get
                { return activeInfo; }
                set
                {
                    activeInfo.Clear();
                    activeInfo.AddRange(value);
                }
            }

            private List<HtmlRaw> asignInfo = new List<HtmlRaw>();
            public List<HtmlRaw> AsignInfoList
            {
                get
                { return asignInfo; }
                set
                {
                    asignInfo.Clear();
                    asignInfo.AddRange(value);
                }
            }
    }
}