using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.Models
{
    public class ProjectMES
    {
        public string ProjectKey { set; get; }
        public string Station { set; get; }
        public string SQL { set; get; }
    }

    public class ProjectViewModels
    {
        public ProjectViewModels()
        {
            
        }

        public ProjectViewModels(string prokey, string proname, string startdate, string creator, string pm, decimal finshrate, string sdescription)
        {
            this.ProjectKey = prokey;
            this.ProjectName = proname;
            this.StartDate = DateTime.Parse(startdate);
            this.Creator = creator;
            this.PM = pm;
            this.FinishRate = finshrate;
            this.dbDescription = sdescription;
        }

        public string ProjectKey { set; get; }

        [StringLength(180, MinimumLength = 6)]
        [Required]
        public string ProjectName { set; get; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { set; get; }

        [StringLength(90, MinimumLength = 6)]
        public string Creator { set; get; }

        [StringLength(180, MinimumLength = 6)]
        [Required]
        [RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+).*")]
        public string PM { set; get; }

        [StringLength(260, MinimumLength = 6)]
        [Required]
        [RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+).*")]
        public string Engineers { set; get; }

        public decimal FinishRate { set; get; }


        private string sDescription = "";

        [StringLength(90, MinimumLength = 6)]
        [Required]
        public string Description {
            get
            {
                return sDescription;
            }

            set
            {
                sDescription = value;
            }
        }

        public string dbDescription
        {
            get
            {
                return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(sDescription));
            }

            set
            {
                sDescription = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(value));
            }
        }

        private List<ProjectMES> lmes = new List<ProjectMES>();
        public List<ProjectMES> MESList
        {
            get
            { return lmes; }
            set
            {
                lmes.Clear();
                lmes.AddRange(value);
            }
        }


    }
}