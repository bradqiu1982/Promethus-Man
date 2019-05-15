using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
    public class UT
    {
        public static List<List<T>> SplitList<T>(List<T> me, int size = 5000)
        {
            var list = new List<List<T>>();
            try
            {
                for (int i = 0; i < me.Count; i += size)
                {
                    list.Add(me.GetRange(i, Math.Min(size, me.Count - i)));
                }
            }
            catch (Exception ex) { }
            return list;
        }

        public static DateTime O2T(object obj)
        {
            if (obj == null || string.IsNullOrEmpty(obj.ToString()))
            { return DateTime.Parse("1982-05-06 10:00:00"); }

            try
            {
                return Convert.ToDateTime(obj);
            }
            catch (Exception ex) { return DateTime.Parse("1982-05-06 10:00:00"); }
        }

    }
}