using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
    public class ActionStatus
    {
        public static string ValidStatus = "1";
        public static string InValidStatus ="2";
    }
    public class IsPublishStatus
    {
        public static string mPublic = "1";
        public static string mPrivate = "2";
    }
    public class ProjectMileStonesVM
    {
        public string ID { set; get; }
        public string ProjectKey { set; get; }
        public string UserName { set; get; }
        public string Action { set; get; }
        public string IsPublish { set; get; }
        public string AddDate { set; get; }
        public string Status { set; get; }
        public string CreateTime { set; get; }
        public string UpdateTime { set; get; }

        public static List<ProjectMileStonesVM> GetProjectMileStones(string pKey, string uName, bool ispublish = false, string sDate = "", string eDate = "")
        {
            var sql = @"select ID, ProjectKey, UserName, Action, IsPublish, Status, AddDate, CreateTime, UpdateTime 
                        from ProjectMileStones as pm 
                        where Status = @Status and ProjectKey = @pKey ";
            var param = new Dictionary<string, string>();
            param.Add("@pKey", pKey);
            param.Add("@Status", ActionStatus.ValidStatus);
            param.Add("@uName", uName);
            if (ispublish)
            {
                sql += " and (IsPublish = @mPublic or (UserName = @uName and IsPublish = @mPrivate)) ";
                param.Add("@mPublic", IsPublishStatus.mPublic);
                param.Add("@mPrivate", IsPublishStatus.mPrivate);
            }
            else
            {
                sql += " and UserName = @uName ";
            }
            if (!string.IsNullOrEmpty(sDate))
            {
                sql += " and AddDate >= @sDate ";
                param.Add("@sDate", sDate);
            }
            if (!string.IsNullOrEmpty(eDate))
            {
                sql += " and AddDate <= @eDate ";
                param.Add("@eDate", eDate);
            }
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null, param);
            var res = new List<ProjectMileStonesVM>();
            if(dbret.Count > 0)
            {
                foreach(var item in dbret)
                {
                    var tmp = new ProjectMileStonesVM();
                    tmp.ID = Convert.ToString(item[0]);
                    tmp.ProjectKey = Convert.ToString(item[1]);
                    tmp.UserName = Convert.ToString(item[2]);
                    tmp.Action = Convert.ToString(item[3]);
                    tmp.IsPublish = Convert.ToString(item[4]);
                    tmp.Status = Convert.ToString(item[5]);
                    tmp.AddDate = Convert.ToString(item[6]);
                    tmp.CreateTime = Convert.ToString(item[7]);
                    tmp.UpdateTime = Convert.ToString(item[8]);
                    res.Add(tmp);
                }
            }
            return res;
        }
        
        public static void UpdateProjectMileStones(ProjectMileStonesVM pmvm)
        {
            var sql = @"update ProjectMileStones set ";
            var param = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(pmvm.Action))
            {
                sql += " Action = @Action, ";
                param.Add("@Action", pmvm.Action);
            }
            if (!string.IsNullOrEmpty(pmvm.AddDate))
            {
                sql += " AddDate = @AddDate, ";
                param.Add("@AddDate", pmvm.AddDate);
            }
            if (!string.IsNullOrEmpty(pmvm.IsPublish))
            {
                sql += " IsPublish = @IsPublish, ";
                param.Add("@IsPublish", pmvm.IsPublish);
            }
            if (!string.IsNullOrEmpty(pmvm.Status))
            {
                sql += " Status = @Status, ";
                param.Add("@Status", pmvm.Status);
            }

            sql += " UpdateTime = @uTime where ID = @ID ";
            param.Add("@uTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            param.Add("@ID", pmvm.ID);

            DBUtility.ExeLocalSqlNoRes(sql, param);

        }

        public static void AddProjectMileStones(ProjectMileStonesVM pmvm)
        {
            var sql = @"insert into ProjectMileStones(ProjectKey, UserName, Action, 
                    IsPublish, Status, AddDate, CreateTime, UpdateTime)
                    values(@ProjectKey, @UserName, @Action, @IsPublish, @Status, @AddDate, 
                    @CreateTime, @UpdateTime)";
            var param = new Dictionary<string, string>();
                param.Add("@ProjectKey", pmvm.ProjectKey);
                param.Add("@UserName", pmvm.UserName);
                param.Add("@Action", pmvm.Action);
                param.Add("@IsPublish", pmvm.IsPublish);
                param.Add("@Status", ActionStatus.ValidStatus);
                param.Add("@AddDate", pmvm.AddDate);
                param.Add("@CreateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                param.Add("@UpdateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            DBUtility.ExeLocalSqlNoRes(sql, param);
        }
    }
}