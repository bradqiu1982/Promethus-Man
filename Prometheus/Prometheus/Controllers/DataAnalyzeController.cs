using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Prometheus.Models;

namespace Prometheus.Controllers
{
    public class DataAnalyzeController : Controller
    {
        private List<SelectListItem> CreateSelectList(List<string> valist, string defVal)
        {
            bool selected = false;
            var pslist = new List<SelectListItem>();
            foreach (var p in valist)
            {
                var pitem = new SelectListItem();
                pitem.Text = p;
                pitem.Value = p;
                if (!string.IsNullOrEmpty(defVal) && string.Compare(defVal, p, true) == 0)
                {
                    pitem.Selected = true;
                    selected = true;
                }
                pslist.Add(pitem);
            }

            if (!selected && pslist.Count > 0)
            {
                pslist[0].Selected = true;
            }

            return pslist;
        }


        private void preparetxotestdata(string leftcond, string leftfield, string rightcond, string rightfield)
        {
            var waferlist = ExternalDataCollector.RetrieveNeoMapWaferList();
            var datafieldlist = ExternalDataCollector.NeoMapMainFieldNameList();

            var selectlist = new List<string>();
            selectlist.Add("Please select query key");
            selectlist.AddRange(waferlist);
            var selectcontrol = CreateSelectList(selectlist, leftcond);
            selectcontrol[0].Disabled = true;
            selectcontrol[0].Selected = true;
            ViewBag.leftquerylist = selectcontrol;

            selectlist = new List<string>();
            selectlist.Add("Please select compare query key");
            selectlist.AddRange(waferlist);
            selectcontrol = CreateSelectList(selectlist, rightcond);
            selectcontrol[0].Disabled = true;
            selectcontrol[0].Selected = true;
            ViewBag.rightquerylist = selectcontrol;

            selectlist = new List<string>();
            selectlist.Add("Please select data field");
            selectlist.AddRange(datafieldlist);
            selectcontrol = CreateSelectList(selectlist, leftfield);
            selectcontrol[0].Disabled = true;
            selectcontrol[0].Selected = true;
            ViewBag.leftdatafieldlist = selectcontrol;

            selectlist = new List<string>();
            selectlist.Add("Please select data field");
            selectlist.AddRange(datafieldlist);
            selectcontrol = CreateSelectList(selectlist, rightfield);
            selectcontrol[0].Disabled = true;
            selectcontrol[0].Selected = true;
            ViewBag.rightdatafieldlist = selectcontrol;
        }

        public ActionResult TXOTestData()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {

            }
            else
            {
                var ck = new Dictionary<string, string>();
                ck.Add("logonredirectctrl", "DataAnalyze");
                ck.Add("logonredirectact", "TXOTestData");
                CookieUtility.SetCookie(this, ck);
                return RedirectToAction("LoginUser", "User");
            }

            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
            ViewBag.UserName = updater.Split(new char[] { '@' })[0];
            ViewBag.RealUserID = updater;

            preparetxotestdata("","","","");

            return View();
        }

        private void HeatMap4NeoMap(List<NeoMapDataWithPos> vm, string querycond, string datafield, bool left)
        {
                var DATAFIELDNAME = datafield.Replace(TXOQUERYCOND.NEOMAP, "");
                var Title = querycond + " " + datafield.Replace(TXOQUERYCOND.NEOMAP, "") + " Heat Map";
                var ElementID = "lefthotscript";
                if (!left)
                {
                    ElementID = "righthotscript";
                }

                var MAPDATA = string.Empty;
                var xmin = vm[0].x;
                var xmax = vm[0].x;

                var ymin = vm[0].y;
                var ymax = vm[0].y;

                var VALUEMIN = vm[0].value;
                var VALUEMAX = vm[0].value;
                

                foreach (var item in vm)
                {
                    MAPDATA = MAPDATA+"[" +item.x.ToString()+","+item.y.ToString()+","+item.value.ToString()+ "],";

                    if (item.x < xmin)
                        xmin = item.x;
                    if (item.x > xmax)
                        xmax = item.x;

                    if (item.y < ymin)
                        ymin = item.y;
                    if (item.y > ymax)
                        ymax = item.y;

                    if (item.value > VALUEMAX)
                        VALUEMAX = item.value;

                    if (item.value < VALUEMIN)
                        VALUEMIN = item.value;
                }
                if (MAPDATA.Length > 6)
                    MAPDATA = MAPDATA.Substring(0, MAPDATA.Length - 1);

                var scritpttxt = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/HotMap.xml"));
                scritpttxt = scritpttxt.Replace("#ElementID#", ElementID)
                        .Replace("#Title#", Title)
                        .Replace("#XMIN#", xmin.ToString())
                        .Replace("#XMAX#", xmax.ToString())
                        .Replace("#YMIN#", ymin.ToString())
                        .Replace("#YMAX#", ymax.ToString())
                        .Replace("#DATAFIELDNAME#", DATAFIELDNAME)
                        .Replace("#MAPDATA#", MAPDATA)
                        .Replace("#VALUEMAX#", VALUEMAX.ToString())
                        .Replace("#VALUEMIN#", VALUEMIN.ToString());
                if (left)
                {
                    ViewBag.lefthotscript = scritpttxt;
                }
                else
                {
                    ViewBag.righthotscript = scritpttxt;
                }
        }

        private void NormalDistributeChart(List<NeoMapDataWithPos> vm, string querycond, string datafield, bool left)
        {
            var ylist = new List<double>();
                var valuelist = new List<double>();
                foreach (var item in vm)
                {
                    valuelist.Add(item.value);
                }
                valuelist.Sort();

                var mean = NormalDistributeAlg.Mean(valuelist);
                var stddev = NormalDistributeAlg.StandardDeviation(valuelist, mean);
                foreach (var item in valuelist)
                {
                    ylist.Add(NormalDistributeAlg.getY(item, stddev, mean));
                }

            var DATAFIELDNAME = datafield.Replace(TXOQUERYCOND.NEOMAP, "").Replace(TXOQUERYCOND.BURNIN,"");
            var Title = querycond +" "+ datafield.Replace(TXOQUERYCOND.NEOMAP, "").Replace(TXOQUERYCOND.BURNIN, "") 
                + " Normal Distribution";

            var ElementID = "leftnormaldistr";
            if (!left)
            {
                ElementID = "rightnormaldistr";
            }
            var sumy = 0.0;


            var count = valuelist.Count;
            for (var idx = 0; idx < count; idx++)
            {
                sumy = sumy + ylist[idx]/100.0;
            }

            var xmin = valuelist[0];
            var xmax = valuelist[0];

            var ymin = ylist[0]/sumy;
            var ymax = ylist[0]/sumy;

            var YVALUES = string.Empty;

            for (var idx = 0; idx < count; idx++)
            {
                if (valuelist[idx] < xmin)
                    xmin = valuelist[idx];
                if (valuelist[idx] > xmax)
                    xmax = valuelist[idx];

                if (ylist[idx]/sumy < ymin)
                    ymin = ylist[idx]/sumy;
                if (ylist[idx]/sumy > ymax)
                    ymax = ylist[idx]/sumy;

                YVALUES = YVALUES + "[" + valuelist[idx].ToString() + "," + ((ylist[idx]/sumy)*100).ToString() + "],";
            }

            YVALUES = YVALUES.Substring(0, YVALUES.Length - 1);

            var left3sigma = (mean - stddev * 3.0);
            var right3sigma = (mean + stddev * 3.0);
            if (left3sigma < xmin)
                xmin = left3sigma;
            if (right3sigma > xmax)
                xmax = right3sigma;

            var scritpttxt = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/NormalDistribution.xml"));
            scritpttxt = scritpttxt.Replace("#ElementID#", ElementID)
                    .Replace("#Title#", Title)
                    .Replace("#XMIN#", xmin.ToString())
                    .Replace("#XMAX#", xmax.ToString())
                    .Replace("#YMIN#", (ymin*100).ToString())
                    .Replace("#YMAX#", (ymax*100).ToString())
                    .Replace("#DATAFIELDNAME#", DATAFIELDNAME)
                    .Replace("#MEAN#", mean.ToString("f6"))
                    .Replace("#StDev#", stddev.ToString("f6"))
                    .Replace("#StDevLeft#", left3sigma.ToString())
                    .Replace("#StDevRight#", right3sigma.ToString())
                    .Replace("#YVALUES#", YVALUES);

            if (left)
            {
                ViewBag.leftnormaldistr = scritpttxt;
            }
            else
            {
                ViewBag.rightnormaldistr = scritpttxt;
            }
        }

        private void NeoMapDataAnalysis(string querycond, string datafield,bool left =true)
        {
            var vm = ExternalDataCollector.RetrieveNeoMapData(querycond, datafield);
            if (vm.Count > 2)
            {
                HeatMap4NeoMap(vm, querycond, datafield, left);
                NormalDistributeChart(vm, querycond, datafield, left);
            }
        }

        [HttpPost, ActionName("TXOTestData")]
        [ValidateAntiForgeryToken]
        public ActionResult TXOTestDataPost()
        {
            var leftcond = string.Empty;
            var leftfield = string.Empty;
            var rightcond = string.Empty;
            var rightfield = string.Empty;

            if (!string.IsNullOrEmpty(Request.Form["leftquerylist"])
                && !string.IsNullOrEmpty(Request.Form["leftdatafieldlist"]))
            {
                var querycond = Request.Form["leftquerylist"].ToString();
                var datafield = Request.Form["leftdatafieldlist"];
                leftcond = querycond;
                leftfield = datafield;
                if (datafield.Contains(TXOQUERYCOND.NEOMAP))
                {
                    NeoMapDataAnalysis(querycond, datafield);
                }
            }

            if (!string.IsNullOrEmpty(Request.Form["rightquerylist"])
                && !string.IsNullOrEmpty(Request.Form["rightdatafieldlist"]))
            {
                var querycond = Request.Form["rightquerylist"].ToString();
                var datafield = Request.Form["rightdatafieldlist"];
                rightcond = querycond;
                rightfield = datafield;
                if (datafield.Contains(TXOQUERYCOND.NEOMAP))
                {
                    NeoMapDataAnalysis(querycond, datafield,false);
                }
            }

            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
            ViewBag.UserName = updater.Split(new char[] { '@' })[0];
            ViewBag.RealUserID = updater;
            preparetxotestdata("", "", "", "");
            return View();
        }
    }
}