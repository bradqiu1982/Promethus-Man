using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
    public class IssueComments
    {
        public string IssueKey { set; get; }

        private string sComment = "";
        public string Comment
        {
            set { sComment = value; }
            get { return sComment; }
        }

        public string dbComment
        {
            get
            {
                if (string.IsNullOrEmpty(sComment))
                {
                    return "";
                }
                else
                {
                    try
                    {
                        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(sComment));
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
                    sComment = "";
                }
                else
                {
                    try
                    {
                        sComment = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(value));
                    }
                    catch (Exception)
                    {
                        sComment = "";
                    }

                }

            }
        }

        public DateTime CommitDate { set; get; }
    }

    public class IssueViewModels
    {
        public string ProjectKey
        {
            set;get;
        }

        public string IssueType { set; get; }
        public string Summary { set; get; }
        public string Priority { set; get; }
        public DateTime DueDate { set; get; }
        public DateTime ResolvedDate { set; get; }
        public string Assignee { set; get; }
        public string Reporter { set; get; }
        public string Resolution { set; get; }

        private string sDescription = "";
        public string Description {
            set { sDescription = value; }
            get { return sDescription; }
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


        private List<IssueComments> cemlist = new List<IssueComments>();
        public List<IssueComments> CommentList
        {
            set
            {
                cemlist.Clear();
                cemlist.AddRange(value);
            }
            get
            {
                return cemlist;
            }
        }
    }
}