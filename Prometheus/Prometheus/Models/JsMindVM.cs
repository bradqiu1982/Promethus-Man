using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
    public class JsMindVM
    {
        public JsMindVM(){
        }
        public JsMindVM(string n_id, bool n_isroot, string n_topic, string n_pid, string bg_color)
        {
            id = n_id;
            isroot = n_isroot;
            topic = n_topic;
            parentid = n_pid;
            background_color = bg_color;
        }

        public string id { set; get; }
        public bool isroot { set; get; }
        public string topic { set; get; }
        public string parentid { set; get; }
        public string background_color { set; get; }
    }
}