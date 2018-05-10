﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
    public class EngineeringMileStone
    {
        public EngineeringMileStone()
        {
            PJKey = "";
            ActionDate = DateTime.Parse("1982-05-06 10:00:00");
            Location = "";
            ActionDetail = "";
        }


        private static void CleanMileStone(string pjkey)
        {
            var sql = "delete from EngineeringMileStone where  PJKey = @PJKey";
            var dict = new Dictionary<string, string>();
            dict.Add("@PJKey", pjkey);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public static void UpdateVcselMileStone(List<EngineeringMileStone> elist)
        {
            var pjkey = "VCSEL";
            CleanMileStone(pjkey);

            foreach (var item in elist)
            {
                var sql = "insert into EngineeringMileStone(PJKey,ActionDate,Location,ActionDetail) values(@PJKey,@ActionDate,@Location,@ActionDetail)";
                var dict = new Dictionary<string, string>();
                dict.Add("@PJKey",pjkey);
                dict.Add("@ActionDate",item.ActionDate.ToString("yyyy-MM-dd HH:mm:ss"));
                dict.Add("@Location",item.Location);
                dict.Add("@ActionDetail",item.ActionDetail);
                DBUtility.ExeLocalSqlNoRes(sql, dict);
            }
        }

        public static List<EngineeringMileStone> RetrieveVcselMileStone()
        {
            var ret = new List<EngineeringMileStone>();

            var pjkey = "VCSEL";
            var sql = "select PJKey,ActionDate,Location,ActionDetail from EngineeringMileStone where PJKey = '<PJKey>'";
            sql = sql.Replace("<PJKey>", pjkey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var tempvm = new EngineeringMileStone();
                tempvm.PJKey = pjkey;
                tempvm.ActionDate = Convert.ToDateTime(line[1]);
                tempvm.Location = Convert.ToString(line[2]);
                tempvm.ActionDetail = Convert.ToString(line[3]);
                ret.Add(tempvm);
            }

            return ret;
        }

        public string PJKey { set; get; }
        public DateTime ActionDate { set; get; }
        public string Location { set; get; }
        public string ActionDetail { set; get; }
    }
}