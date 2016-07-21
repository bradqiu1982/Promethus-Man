using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.Models
{
    
    public class ProjectStation
    {
        public ProjectStation(string key, string stat)
        {
            ProjectKey = key;
            Station = stat;
        }

        public string ProjectKey { set; get; }
        public string Station { set; get; }
    }

    public class ProjectMembers
    {
        public ProjectMembers(string key, string sname,string srole)
        {
            ProjectKey = key;
            Name = sname;
            Role = srole;
        }

        public string ProjectKey { set; get; }
        public string Name { set; get; }
        public string Role { set; get; }
    }

    public class ProjectPn
    {
        public ProjectPn(string key, string p)
        {
            ProjectKey = key;
            Pn = p;
        }
        public string ProjectKey { set; get; }
        public string Pn { set; get; }
    }

    public class ProjectMesTable
    {
        public ProjectMesTable(string key,string stat,string table)
        {
            ProjectKey = key;
            Station = stat;
            TableName = table;
        }

        public string ProjectKey { set; get; }
        public string Station { set; get; }
        public string TableName { set; get; }
    }

    public class ProjectViewModels
    {
        public static string PMROLE = "PM";
        public static string ENGROLE = "ENG";

        public ProjectViewModels()
        {
            
        }

        public ProjectViewModels(string prokey, string proname, string startdate, decimal finshrate, string sdescription)
        {
            this.ProjectKey = prokey;
            this.ProjectName = proname;
            this.StartDate = DateTime.Parse(startdate);
            this.FinishRating = finshrate;
            this.dbDescription = sdescription;
        }

        public string ProjectKey { set; get; }

        [Display(Name = "Project")]
        [StringLength(180, MinimumLength = 6)]
        [RegularExpression("^(?!.*\').*$")]
        [Required]
        public string ProjectName { set; get; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { set; get; }


        [RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)")]
        public string PM { set; get; }

        //[StringLength(260, MinimumLength = 6)]
        //[Required]
        //[RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+).*")]
        //public string Engineers { set; get; }

        public decimal FinishRating { set; get; }


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
                if (string.IsNullOrEmpty(sDescription))
                {
                    return "";
                }
                else
                {
                    try
                    {
                        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(sDescription));
                    }
                    catch (Exception)
                    {
                        return "";
                    }
                }
                    
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    sDescription = "";
                }
                else
                {
                    try
                    {
                        sDescription = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(value));
                    }
                    catch (Exception)
                    {
                        sDescription = "";
                    }

                }
                
            }
        }

        private List<ProjectStation> lstation = new List<ProjectStation>();
        public List<ProjectStation> StationList
        {
            get
            { return lstation; }
            set
            {
                lstation.Clear();
                lstation.AddRange(value);
            }
        }

        private List<ProjectPn> lpn = new List<ProjectPn>();
        public List<ProjectPn> PNList
        {
            get
            { return lpn; }
            set
            {
                lpn.Clear();
                lpn.AddRange(value);
            }
        }

        private List<ProjectMesTable> ltab = new List<ProjectMesTable>();
        public List<ProjectMesTable> TabList
        {
            get
            { return ltab; }
            set
            {
                ltab.Clear();
                ltab.AddRange(value);
            }
        }

        private List<ProjectMembers> lmeb = new List<ProjectMembers>();
        public List<ProjectMembers> MemberList
        {
            get
            { return lmeb; }
            set
            {
                lmeb.Clear();
                lmeb.AddRange(value);
            }
        }
    }

    }
