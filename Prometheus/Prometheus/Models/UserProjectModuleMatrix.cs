using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
    public class ProjectModuleUnitVM
    {
        public string Key { set; get; }
        public string Url { set; get; }
        public string ImgUrl { set; get; }
        public bool isTooltip { set; get; }
        public string ApVal0 { set; get; }
        public string ApVal1 { set; get; }
        public bool Visible { set; get; }
    }

    public class ProjectModuleVM
    {
        public ProjectModuleVM()
        {
            AllModules = new Dictionary<string, ProjectModuleUnitVM>();
            Yield = new ProjectModuleUnitVM();
            FA = new ProjectModuleUnitVM();
            Task = new ProjectModuleUnitVM();
            RMA = new ProjectModuleUnitVM();
            DebugTree = new ProjectModuleUnitVM();
            BurnIn = new ProjectModuleUnitVM();
            SptTask = new ProjectModuleUnitVM();
            NPI = new ProjectModuleUnitVM();
            Station = new ProjectModuleUnitVM();
            Rel = new ProjectModuleUnitVM();
            ProjectManage = new ProjectModuleUnitVM();
            MileStone = new ProjectModuleUnitVM();

            Yield.Key = "Yield";
            Yield.Url = "/Project/ProjectYieldMain";
            Yield.ImgUrl = "/Content/images/PJ/Yield.png";
            Yield.isTooltip = true;
            Yield.ApVal0 = "FirstYield";
            Yield.ApVal1 = "RetestYield";
            Yield.Visible = true;
            AllModules.Add(Yield.Key, Yield);

            FA.Key = "FA";
            FA.Url = "/Project/ProjectFA";
            FA.ImgUrl = "/Content/images/PJ/FA.png";
            FA.isTooltip = false;
            FA.ApVal0 = "FinishRating";
            FA.ApVal1 = "PendingFACount";
            FA.Visible = true;
            AllModules.Add(FA.Key, FA);

            Task.Key = "Task";
            Task.Url = "/Project/ProjectIssues";
            Task.ImgUrl = "/Content/images/PJ/Task.png";
            Task.isTooltip = true;
            Task.ApVal0 = "PendingTaskCount";
            Task.Visible = true;
            AllModules.Add(Task.Key, Task);

            RMA.Key = "RMA";
            RMA.Url = "/Project/ProjectRMAStatus";
            RMA.ImgUrl = "/Content/images/PJ/RMA.png";
            RMA.isTooltip = false;
            RMA.ApVal0 = "PendingRMACount";
            RMA.Visible = false;
            AllModules.Add(RMA.Key, RMA);

            DebugTree.Key = "DebugTree";
            DebugTree.Url = "/Project/ProjectError";
            DebugTree.ImgUrl = "/Content/images/PJ/DebugTreeicon.png";
            DebugTree.isTooltip = true;
            DebugTree.Visible = true;
            AllModules.Add(DebugTree.Key, DebugTree);

            BurnIn.Key = "BurnIn";
            BurnIn.Url = "/BurnIn/BurnInMainPage";
            BurnIn.ImgUrl = "/Content/images/PJ/BI.png";
            BurnIn.isTooltip = true;
            BurnIn.Visible = false;
            AllModules.Add(BurnIn.Key, BurnIn);
            
            SptTask.Key = "Critical Task";
            SptTask.Url = "/Project/ProjectSptTask";
            SptTask.ImgUrl = "/Content/images/PJ/Spt.png";
            SptTask.isTooltip = true;
            SptTask.Visible = true;
            AllModules.Add(SptTask.Key, SptTask);

            NPI.Key = "NPI Process";
            NPI.Url = "/Project/ProjectNPI";
            NPI.ImgUrl = "/Content/images/PJ/NPIProcess.png";
            NPI.isTooltip = true;
            NPI.Visible = false;
            AllModules.Add(NPI.Key, NPI);

            Station.Key = "Project Stations";
            Station.Url = "/Project/ProjectStations";
            Station.ImgUrl = "/Content/images/PJ/station.png";
            Station.isTooltip = true;
            Station.Visible = false;
            AllModules.Add(Station.Key, Station);

            Rel.Key = "Project Reliability";
            Rel.Url = "/Project/ProjectReliability";
            Rel.ImgUrl = "/Content/images/PJ/REL.png";
            Rel.isTooltip = true;
            Rel.Visible = false;
            AllModules.Add(Rel.Key, Rel);

            ProjectManage.Key = "Project Manage";
            ProjectManage.Url = "/Project/ProjectDash";
            ProjectManage.ImgUrl = "/Content/images/PJ/PM.png";
            ProjectManage.isTooltip = true;
            ProjectManage.Visible = false;
            AllModules.Add(ProjectManage.Key, ProjectManage);

            MileStone.Key = "Project MileStone";
            MileStone.Url = "/Project/ProjectMileStone";
            MileStone.ImgUrl  = "/Content/images/PJ/milestone.png";
            MileStone.isTooltip = true;
            MileStone.Visible = false;
            AllModules.Add(MileStone.Key, MileStone);

        }
        public Dictionary<string, ProjectModuleUnitVM> AllModules { set; get; }
        public ProjectModuleUnitVM Yield { set; get; }
        public ProjectModuleUnitVM FA { set; get; }
        public ProjectModuleUnitVM Task { set; get; }
        public ProjectModuleUnitVM RMA { set; get; }
        public ProjectModuleUnitVM DebugTree { set; get; }
        public ProjectModuleUnitVM BurnIn { set; get; }

        public ProjectModuleUnitVM SptTask { set; get; }
        public ProjectModuleUnitVM NPI { set; get; }
        public ProjectModuleUnitVM Station { set; get; }
        public ProjectModuleUnitVM Rel { set; get; }
        public ProjectModuleUnitVM ProjectManage { set; get; }
        public ProjectModuleUnitVM MileStone { set; get; }
    }

    public class UserProModuleStatus
    {
        public static string ValidStatus = "1";
        public static string InValidStatus = "2";
    }

    public class ProjectModulePositionVM
    {
        public string x { set; get; }
        public string y { set; get; }
        public string val { set; get; }
        public string visible { set; get; }

    }
    public class ProjectSortVM
    {
        public string key { set; get; }
        public string visible { set; get; }
    }
    public class UserProjectModuleMatrix
    {
        public string ID { set; get; }
        public string UserName { set; get; }
        public string ProjectKey { set; get; }
        public List<ProjectSortVM> SortData { set; get; }
        public string Status { set; get; }
        public string CreateTime { set; get; }
        public string UpdateTime { set; get; }

        public static Dictionary<string, UserProjectModuleMatrix> GetUserProjectModuleMatrix(string uName, string pKey = "", string status = "1")
        {
            var sql = @"select ID, UserName, ProjectKey, SortData, 
                    Status, CreateTime, UpdateTime
                    from UserProjectModuleMatrix
                    where UserName = @uName";
            var param = new Dictionary<string, string>();
            param.Add("@uName", uName);
            if (!string.IsNullOrEmpty(status))
            {
                sql += " and Status = @Status ";
                param.Add("@Status", status);
            }
            if (!string.IsNullOrEmpty(pKey))
            {
                sql += " and ProjectKey = @pKey ";
                param.Add("@pKey", pKey);
            }
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, param);
            var res = new Dictionary<string, UserProjectModuleMatrix>();
            if(dbret.Count > 0)
            {
                foreach(var item in dbret)
                {
                    if (!res.ContainsKey(Convert.ToString(item[2])))
                    {
                        var tmp = new UserProjectModuleMatrix();
                        tmp.ID = Convert.ToString(item[0]);
                        tmp.UserName = Convert.ToString(item[1]);
                        tmp.ProjectKey = Convert.ToString(item[2]);
                        tmp.SortData = System.Web.Helpers.Json.Decode(Convert.ToString(item[3]), (new List<ProjectSortVM>()).GetType());
                        tmp.Status = Convert.ToString(item[4]);
                        tmp.CreateTime = Convert.ToString(item[5]);
                        tmp.UpdateTime = Convert.ToString(item[6]);
                        res.Add(Convert.ToString(item[2]), tmp);
                    }
                }
            }
            return res;
        }

        public static void SaveUserProModuleMatrix(string uName, string pKey, List<ProjectSortVM> sortdata)
        {
            var existdata = GetUserProjectModuleMatrix(uName, pKey);
            if(existdata.Count > 0 && existdata.ContainsKey(pKey))
            {
                UpdateUserProModuleMatrix(existdata[pKey].ID, UserProModuleStatus.ValidStatus, sortdata);
            }
            else
            {
                AddUserProModuleMatri(uName, pKey, sortdata);
            }
        }

        public static void UpdateUserProModuleMatrix(string id, string status = "", List<ProjectSortVM> sortdata = null)
        {
            var sql = @"update UserProjectModuleMatrix set ";
            var param = new Dictionary<string, string>();
            if(!string.IsNullOrEmpty(status) || sortdata != null)
            {
                if (!string.IsNullOrEmpty(status))
                {
                    sql += " Status = @status, ";
                    param.Add("@status", status);
                }
                if(sortdata != null)
                {
                    sql += " SortData = @sortdata, ";
                    param.Add("@sortdata", System.Web.Helpers.Json.Encode(sortdata));
                }
                sql += " UpdateTime = @uTime where ID = @ID ";
                param.Add("@uTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                param.Add("@ID", id);
                DBUtility.ExeLocalSqlNoRes(sql, param);
            }
            
        }

        public static void AddUserProModuleMatri(string uName, string pKey, List<ProjectSortVM> sortdata)
        {
            var sql = @"insert into UserProjectModuleMatrix 
                (UserName, ProjectKey, SortData, Status, CreateTime, UpdateTime) 
                values (@uName, @pKey, @sortdata, @status, @cTime, @uTime)";
            var param = new Dictionary<string, string>();
            param.Add("@uName", uName);
            param.Add("@pKey", pKey);
            param.Add("@sortdata", System.Web.Helpers.Json.Encode(sortdata));
            param.Add("@status", UserProModuleStatus.ValidStatus);
            param.Add("@cTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            param.Add("@uTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            DBUtility.ExeLocalSqlNoRes(sql, param);

        }
    }
}