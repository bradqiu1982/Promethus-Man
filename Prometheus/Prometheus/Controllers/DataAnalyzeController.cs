using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Prometheus.Models;

namespace Prometheus.Controllers
{

    public class NormalDistributionData
    {
        public double mean { set; get; }
        public double stddev { set; get; }
        public double xmin { set; get; }
        public double xmax { set; get; }
        public double ymin { set; get; }
        public double ymax { set; get; }
        public double left3sigma { set; get; }
        public double right3sigma { set; get; }
        public string YVALUES { set; get; }
        public double yrate { set; get; }
    }

    public class BoxPlotData
    {
        public double Min { set; get; }
        public double LowerQuart { set; get; }
        public double Mean { set; get; }
        public double UpperQuart { set; get; }
        public double Max { set; get; }
    }

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

        private NormalDistributionData GetNormalDistrData(List<NeoMapDataWithPos> vm,double comparerate)
        {
            var ret = new NormalDistributionData();

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

            var count = valuelist.Count;

            var xmin = valuelist[0];
            var xmax = valuelist[0];
            var ymin = ylist[0];
            var ymax = ylist[0];
            for (var idx = 0; idx < count; idx++)
            {
                if (valuelist[idx] < xmin)
                    xmin = valuelist[idx];
                if (valuelist[idx] > xmax)
                    xmax = valuelist[idx];

                if (ylist[idx] < ymin)
                    ymin = ylist[idx];
                if (ylist[idx] > ymax)
                    ymax = ylist[idx];
            }

            double rate = 1.0;

            if (comparerate != 1.0)
            {
                rate = comparerate;
            }
            else
            {
                var tempymax = ymax;
                if (tempymax > 100)
                {
                    while (tempymax > 100.0)
                    {
                        tempymax = tempymax / 10.0;
                        rate = rate * 10.0;
                    }
                }
                else if (tempymax < 1.0 && tempymax > 0.0)
                {
                    while (tempymax < 1.0)
                    {
                        tempymax = tempymax / 0.1;
                        rate = rate * 0.1;
                    }
                }
            }


            var YVALUES = string.Empty;
            for (var idx = 0; idx < count; idx++)
            {
                YVALUES = YVALUES + "[" + valuelist[idx].ToString() + "," + (ylist[idx] / rate).ToString() + "],";
            }

            YVALUES = YVALUES.Substring(0, YVALUES.Length - 1);

            ymin = ymin / rate;
            ymax = ymax / rate;

            var left3sigma = (mean - stddev * 3.0);
            var right3sigma = (mean + stddev * 3.0);

            var left4sigma = (mean - stddev * 4.0);
            var right4sigma = (mean + stddev * 4.0);

            if (left4sigma < xmin)
                xmin = left4sigma;
            if (right4sigma > xmax)
                xmax = right4sigma;

            ret.mean = mean;
            ret.stddev = stddev;
            ret.xmax = xmax;
            ret.xmin = xmin;
            ret.ymax = ymax;
            ret.ymin = ymin;
            ret.left3sigma = left3sigma;
            ret.right3sigma = right3sigma;
            ret.YVALUES = YVALUES;
            ret.yrate = rate;

            return ret;
        }

        private void NEONormalDistributeCombineChart(List<NeoMapDataWithPos> lvm, List<NeoMapDataWithPos> rvm
            , string lquerycond, string rquerycond, string datafield)
        {
            var lds = GetNormalDistrData(lvm,1.0);
            var rds = GetNormalDistrData(rvm,lds.yrate);

            var xmax = lds.xmax > rds.xmax ? lds.xmax : rds.xmax;
            var xmin = lds.xmin < rds.xmin ? lds.xmin : rds.xmin;
            var ymax = lds.ymax > rds.ymax ? lds.ymax : rds.ymax;
            var ymin = lds.ymin < rds.ymin ? lds.ymin : rds.ymin;

            var DATAFIELDNAME = datafield.Replace(TXOQUERYCOND.NEOMAP, "").Replace(TXOQUERYCOND.BURNIN, "");
            var Title = DATAFIELDNAME+ " Normal Distribution";
            var ElementID = "combinenormaldistr";

            var scritpttxt = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/NormalDistributionCombine.xml"));
            scritpttxt = scritpttxt.Replace("#ElementID#", ElementID)
                    .Replace("#Title#", Title)
                    .Replace("#DATAFIELDNAME#", DATAFIELDNAME)
                    .Replace("#XMIN#", xmin.ToString())
                    .Replace("#XMAX#", xmax.ToString())
                    .Replace("#YMIN#", ymin.ToString())
                    .Replace("#YMAX#", ymax.ToString())
                    .Replace("#LMEAN#", lds.mean.ToString("f6"))
                    .Replace("#RMEAN#", rds.mean.ToString("f6"))
                    .Replace("#LStDev#", lds.stddev.ToString("f6"))
                    .Replace("#RStDev#", rds.stddev.ToString("f6"))
                    .Replace("#LStDevLeft#", lds.left3sigma.ToString())
                    .Replace("#RStDevLeft#", rds.left3sigma.ToString())
                    .Replace("#LStDevRight#", lds.right3sigma.ToString())
                    .Replace("#RStDevRight#", rds.right3sigma.ToString())
                    .Replace("#LYVALUES#", lds.YVALUES)
                    .Replace("#RYVALUES#", rds.YVALUES)
                    .Replace("#LSERIESNAME#", lquerycond)
                    .Replace("#RSERIESNAME#", rquerycond);

            ViewBag.combinenormaldistr = scritpttxt;
        }

        private void NEOBoxPlotCombine(List<NeoMapDataWithPos> lvm, List<NeoMapDataWithPos> rvm
            , string lquerycond, string rquerycond, string datafield)
        {
            //{name: '#SERIESNAME#',data: [#YVALUES#]}
            var lrawdata = new List<double>();
            foreach (var item in lvm)
            {
                lrawdata.Add(item.value);
            }
            var lboxdata = GetBoxPlotData(lrawdata);

            var rrawdata = new List<double>();
            foreach (var item in rvm)
            {
                rrawdata.Add(item.value);
            }
            var rboxdata = GetBoxPlotData(rrawdata);

            var DATAFIELDNAME = datafield.Replace(TXOQUERYCOND.NEOMAP, "").Replace(TXOQUERYCOND.BURNIN, "");
            var Title =  DATAFIELDNAME + " Box Plot";

            var LYVALUES = lboxdata.Min.ToString() + "," + lboxdata.LowerQuart.ToString() + "," + lboxdata.Mean.ToString() + "," + lboxdata.UpperQuart.ToString() + "," + lboxdata.Max.ToString();
            var RYVALUES = rboxdata.Min.ToString() + "," + rboxdata.LowerQuart.ToString() + "," + rboxdata.Mean.ToString() + "," + rboxdata.UpperQuart.ToString() + "," + rboxdata.Max.ToString();

            var min = lboxdata.Min < rboxdata.Min ? lboxdata.Min : rboxdata.Min;
            var max = lboxdata.Max > rboxdata.Max ? lboxdata.Max : rboxdata.Max;

            var lseriesstr = "{name: '#SERIESNAME#',data: [[#YVALUES#]]}";
            var rseriesstr = "{name: '#SERIESNAME#',data: [[#YVALUES#]]}";
            var mainseriesstr = lseriesstr.Replace("#SERIESNAME#", lquerycond).Replace("#YVALUES#", LYVALUES) + ","
                                + rseriesstr.Replace("#SERIESNAME#", rquerycond).Replace("#YVALUES#", RYVALUES);

            var ElementID = "combineboxplot";

            var scritpttxt = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/BoxPlot.xml"));
            scritpttxt = scritpttxt.Replace("#ElementID#", ElementID)
                    .Replace("#Title#", Title)
                    .Replace("#YMIN#", min.ToString())
                    .Replace("#YMAX#", max.ToString())
                    .Replace("#SERIESVALUE#", mainseriesstr);

            ViewBag.combineboxplot = scritpttxt;
         }

        private void NEONormalDistributeChart(List<NeoMapDataWithPos> vm, string querycond, string datafield, bool left)
        {
            var ds = GetNormalDistrData(vm,1.0);

            var DATAFIELDNAME = datafield.Replace(TXOQUERYCOND.NEOMAP, "").Replace(TXOQUERYCOND.BURNIN,"");
            var Title = querycond +" "+ DATAFIELDNAME + " Normal Distribution";

            var ElementID = "leftnormaldistr";
            if (!left)
            {
                ElementID = "rightnormaldistr";
            }
            
            var scritpttxt = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/NormalDistribution.xml"));
            scritpttxt = scritpttxt.Replace("#ElementID#", ElementID)
                    .Replace("#Title#", Title)
                    .Replace("#XMIN#", ds.xmin.ToString())
                    .Replace("#XMAX#", ds.xmax.ToString())
                    .Replace("#YMIN#", ds.ymin.ToString())
                    .Replace("#YMAX#", ds.ymax.ToString())
                    .Replace("#DATAFIELDNAME#", DATAFIELDNAME)
                    .Replace("#MEAN#", ds.mean.ToString("f6"))
                    .Replace("#StDev#", ds.stddev.ToString("f6"))
                    .Replace("#StDevLeft#", ds.left3sigma.ToString())
                    .Replace("#StDevRight#", ds.right3sigma.ToString())
                    .Replace("#YVALUES#", ds.YVALUES)
                    .Replace("#SERIESNAME#", datafield);

            if (left)
            {
                ViewBag.leftnormaldistr = scritpttxt;
            }
            else
            {
                ViewBag.rightnormaldistr = scritpttxt;
            }
        }

        private double GetBoxMeanValue(List<double> rawdata)
        {
            if ((rawdata.Count % 2) == 0)
            {
                var mididx1 = rawdata.Count / 2;
                var mididx2 = mididx1 + 1;
                return (rawdata[mididx1] + rawdata[mididx2]) / 2.0;
            }
            else
            {
                var mididx = (rawdata.Count + 1) / 2;
                return rawdata[mididx];
            }
        }

        private BoxPlotData GetBoxPlotData(List<double> rawdata)
        {
            rawdata.Sort();
            var ret = new BoxPlotData();
            if ((rawdata.Count % 2) == 0)
            {
                var mididx1 = rawdata.Count / 2;
                var mididx2 = mididx1 + 1;

                ret.Mean = (rawdata[mididx1]+rawdata[mididx2]) / 2.0;
                ret.Min = rawdata[0];
                ret.Max = rawdata[rawdata.Count - 1];

                var lowlist = new List<double>();
                var upperlist = new List<double>();
                for (var idx = 0; idx < mididx2; idx++)
                {
                    lowlist.Add(rawdata[idx]);
                }
                for (var idx = mididx2; idx < rawdata.Count; idx++)
                {
                    upperlist.Add(rawdata[idx]);
                }

                ret.LowerQuart = GetBoxMeanValue(lowlist);
                ret.UpperQuart = GetBoxMeanValue(upperlist);
            }
            else
            {
                var mididx = (rawdata.Count + 1) / 2;
                ret.Mean = rawdata[mididx];
                ret.Min = rawdata[0];
                ret.Max = rawdata[rawdata.Count - 1];

                var lowlist = new List<double>();
                var upperlist = new List<double>();
                for (var idx = 0; idx < mididx; idx++)
                {
                    lowlist.Add(rawdata[idx]);
                }
                for (var idx = mididx+1; idx < rawdata.Count; idx++)
                {
                    upperlist.Add(rawdata[idx]);
                }

                ret.LowerQuart = GetBoxMeanValue(lowlist);
                ret.UpperQuart = GetBoxMeanValue(upperlist);
            }

            return ret;
        }

        private void NEOBoxPlot(List<NeoMapDataWithPos> vm, string querycond, string datafield, bool left)
        {
            //{name: '#SERIESNAME#',data: [#YVALUES#]}
            var rawdata = new List<double>();
            foreach (var item in vm)
            {
                rawdata.Add(item.value);
            }
            var boxdata = GetBoxPlotData(rawdata);
            var DATAFIELDNAME = datafield.Replace(TXOQUERYCOND.NEOMAP, "").Replace(TXOQUERYCOND.BURNIN, "");
            var Title = querycond + " " + DATAFIELDNAME + " Box Plot";

            var YVALUES = boxdata.Min.ToString() + "," + boxdata.LowerQuart.ToString() + "," + boxdata.Mean.ToString() + "," + boxdata.UpperQuart.ToString() + "," + boxdata.Max.ToString();
            var seriesstr = "{name: '#SERIESNAME#',data: [[#YVALUES#]]}";
            seriesstr = seriesstr.Replace("#SERIESNAME#", DATAFIELDNAME).Replace("#YVALUES#", YVALUES);

            var ElementID = "leftboxplot";
            if (!left)
            {
                ElementID = "rightboxplot";
            }

            var scritpttxt = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/BoxPlot.xml"));
            scritpttxt = scritpttxt.Replace("#ElementID#", ElementID)
                    .Replace("#Title#", Title)
                    .Replace("#YMIN#", boxdata.Min.ToString())
                    .Replace("#YMAX#", boxdata.Max.ToString())
                    .Replace("#SERIESVALUE#", seriesstr);

            if (left)
            {
                ViewBag.leftboxplot = scritpttxt;
            }
            else
            {
                ViewBag.rightboxplot = scritpttxt;
            }
        }

        private void NeoMapDataAnalysis(string querycond, string datafield,bool left)
        {
            var vm = ExternalDataCollector.RetrieveNeoMapData(querycond, datafield);
            if (vm.Count > 2)
            {
                HeatMap4NeoMap(vm, querycond, datafield, left);
                NEONormalDistributeChart(vm, querycond, datafield, left);
            }

            if (vm.Count > 5)
            {
                NEOBoxPlot(vm, querycond, datafield,left);
            }
        }

        private void NeoMapDataAnalysisCombine(string lquerycond, string ldatafield, string rquerycond, string rdatafield)
        {
            var lvm = ExternalDataCollector.RetrieveNeoMapData(lquerycond, ldatafield);
            var rvm = ExternalDataCollector.RetrieveNeoMapData(rquerycond, rdatafield);
            if (lvm.Count > 2)
            {
                HeatMap4NeoMap(lvm, lquerycond, ldatafield,true);
            }

            if (rvm.Count > 2)
            {
                HeatMap4NeoMap(rvm, rquerycond, rdatafield, false);
            }
            if (rvm.Count > 2 && lvm.Count > 2)
            {
                NEONormalDistributeCombineChart(lvm, rvm, lquerycond, rquerycond, ldatafield);
            }

            if (rvm.Count > 5 && lvm.Count > 5)
            {
                NEOBoxPlotCombine(lvm, rvm, lquerycond, rquerycond, ldatafield);
            }
        }


        [HttpPost, ActionName("TXOTestData")]
        [ValidateAntiForgeryToken]
        public ActionResult TXOTestDataPost()
        {
            var leftcond = Request.Form["leftquerylist"];
            var leftfield = Request.Form["leftdatafieldlist"];
            var rightcond = Request.Form["rightquerylist"];
            var rightfield = Request.Form["rightdatafieldlist"];

            if (!string.IsNullOrEmpty(leftcond)
                    && !string.IsNullOrEmpty(leftfield)
                    && !string.IsNullOrEmpty(rightcond)
                    && !string.IsNullOrEmpty(rightfield)
                    && string.Compare(leftfield, rightfield) == 0)
            {
                if (leftfield.Contains(TXOQUERYCOND.NEOMAP))
                {
                    NeoMapDataAnalysisCombine(leftcond, leftfield, rightcond, rightfield);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(leftcond)
                    && !string.IsNullOrEmpty(leftfield))
                {
                
                    if (leftfield.Contains(TXOQUERYCOND.NEOMAP))
                    {
                        NeoMapDataAnalysis(leftcond, leftfield,true);
                    }
                }

                if (!string.IsNullOrEmpty(rightcond)
                    && !string.IsNullOrEmpty(rightfield))
                {
                    if (rightfield.Contains(TXOQUERYCOND.NEOMAP))
                    {
                        NeoMapDataAnalysis(rightcond, rightfield, false);
                    }
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