using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.Models
{
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

        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { set; get; }

        [StringLength(90, MinimumLength = 6)]
        [Required]
        [RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$")]
        public string Creator { set; get; }

        [StringLength(90, MinimumLength = 6)]
        [Required]
        [RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$")]
        public string PM { set; get; }

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
    }
}