﻿using System;
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
        public int AmountMAX { set; get; }
        public string YVALUES2 { set; get; }
    }

    public class FrequenceDataRange
    {
        public double lowval { set; get; }
        public double highval { set; get; }
    }

    public class BoxPlotData
    {
        public double Min { set; get; }
        public double LowerQuart { set; get; }
        public double Mean { set; get; }
        public double UpperQuart { set; get; }
        public double Max { set; get; }

        private List<double> outlierlist = new List<double>();
        public List<double> OutLierList
        {
            set {
                outlierlist.Clear();
                outlierlist.AddRange(value);
            }
            get { return outlierlist; }
        }
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


        private void preparetxotestdata(List<string> waferlist,string leftcond, string leftfield, string rightcond, string rightfield)
        {
            
            var datafieldlist = ExternalDataCollector.NeoMapMainFieldNameList();
            datafieldlist.AddRange(BITestResultDataField.BIMainFieldNameList());
            datafieldlist.AddRange(ModuleTXOData.ModuleFieldNameList());
            datafieldlist.AddRange(AlignmentPower.ProcessFieldNameList());

            var selectlist = new List<string>();
            //selectlist.Add("Please select query key");
            //selectlist.AddRange(waferlist);
            //var selectcontrol = CreateSelectList(selectlist, leftcond);
            //selectcontrol[0].Disabled = true;
            //selectcontrol[0].Selected = true;
            //ViewBag.leftquerylist = selectcontrol;

            var tempcondlist = new List<string>();
            tempcondlist.AddRange(waferlist);
            ViewBag.queryvallist = tempcondlist;

            //selectlist = new List<string>();
            //selectlist.Add("Please select compare query key");
            //selectlist.AddRange(waferlist);
            //selectcontrol = CreateSelectList(selectlist, rightcond);
            //selectcontrol[0].Disabled = true;
            //selectcontrol[0].Selected = true;
            //ViewBag.rightquerylist = selectcontrol;

            selectlist = new List<string>();
            selectlist.Add("Please select data field");
            selectlist.AddRange(datafieldlist);
            var selectcontrol = CreateSelectList(selectlist, leftfield);
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

            selectlist = new List<string>();
            selectlist.Add("Please select BI station");
            selectlist.Add("Pre Burn In");
            selectlist.Add("Post Burn In");
            selectcontrol = CreateSelectList(selectlist, leftfield);
            selectcontrol[0].Disabled = true;
            selectcontrol[0].Selected = true;
            ViewBag.leftbistationlist = selectcontrol;

            selectlist = new List<string>();
            selectlist.Add("Please select BI station");
            selectlist.Add("Pre Burn In");
            selectlist.Add("Post Burn In");
            selectcontrol = CreateSelectList(selectlist, leftfield);
            selectcontrol[0].Disabled = true;
            selectcontrol[0].Selected = true;
            ViewBag.rightbistationlist = selectcontrol;

            selectlist = new List<string>();
            selectlist.Add("Please select BI channel");
            for (var idx = 0; idx < 24; idx++)
            {
                selectlist.Add(idx.ToString());
            }
            selectcontrol = CreateSelectList(selectlist, leftfield);
            selectcontrol[0].Disabled = true;
            selectcontrol[0].Selected = true;
            ViewBag.leftbichannellist = selectcontrol;

            selectlist = new List<string>();
            selectlist.Add("Please select BI channel");
            for (var idx = 0; idx < 24; idx++)
            {
                selectlist.Add(idx.ToString());
            }
            selectcontrol = CreateSelectList(selectlist, leftfield);
            selectcontrol[0].Disabled = true;
            selectcontrol[0].Selected = true;
            ViewBag.rightbichannellist = selectcontrol;

            selectlist = new List<string>();
            selectlist.Add("Please select module station");
            selectlist.Add("ersetup");
            selectlist.Add("ertempcomp");
            selectcontrol = CreateSelectList(selectlist, leftfield);
            selectcontrol[0].Disabled = true;
            selectcontrol[0].Selected = true;
            ViewBag.leftmdstationlist = selectcontrol;

            selectlist = new List<string>();
            selectlist.Add("Please select module station");
            selectlist.Add("ersetup");
            selectlist.Add("ertempcomp");
            selectcontrol = CreateSelectList(selectlist, leftfield);
            selectcontrol[0].Disabled = true;
            selectcontrol[0].Selected = true;
            ViewBag.rightmdstationlist = selectcontrol;

            selectlist = new List<string>();
            selectlist.Add("Please select module channel");
            for (var idx = 0; idx < 24; idx++)
            {
                selectlist.Add(idx.ToString());
            }
            selectcontrol = CreateSelectList(selectlist, leftfield);
            selectcontrol[0].Disabled = true;
            selectcontrol[0].Selected = true;
            ViewBag.leftmdchannellist = selectcontrol;

            selectlist = new List<string>();
            selectlist.Add("Please select module channel");
            for (var idx = 0; idx < 24; idx++)
            {
                selectlist.Add(idx.ToString());
            }
            selectcontrol = CreateSelectList(selectlist, leftfield);
            selectcontrol[0].Disabled = true;
            selectcontrol[0].Selected = true;
            ViewBag.rightmdchannellist = selectcontrol;

            selectlist = new List<string>();
            selectlist.Add("Please select test temperature");
            selectlist.Add(TestTemperatureType.Low);
            selectlist.Add(TestTemperatureType.Nomal);
            selectlist.Add(TestTemperatureType.High);
            selectcontrol = CreateSelectList(selectlist, leftfield);
            selectcontrol[0].Disabled = true;
            selectcontrol[0].Selected = true;
            ViewBag.leftmdtemplist = selectcontrol;

            selectlist = new List<string>();
            selectlist.Add("Please select test temperature");
            selectlist.Add(TestTemperatureType.Low);
            selectlist.Add(TestTemperatureType.Nomal);
            selectlist.Add(TestTemperatureType.High);
            selectcontrol = CreateSelectList(selectlist, leftfield);
            selectcontrol[0].Disabled = true;
            selectcontrol[0].Selected = true;
            ViewBag.rightmdtemplist = selectcontrol;

            selectlist = new List<string>();
            selectlist.Add("Please select alignment test");
            selectlist.Add(AlignmentPowerType.AlignmentPower);
            selectlist.Add(AlignmentPowerType.PowerCheck);
            selectcontrol = CreateSelectList(selectlist, leftfield);
            selectcontrol[0].Disabled = true;
            selectcontrol[0].Selected = true;
            ViewBag.leftaligntestlist = selectcontrol;

            selectlist = new List<string>();
            selectlist.Add("Please select alignment test");
            selectlist.Add(AlignmentPowerType.AlignmentPower);
            selectlist.Add(AlignmentPowerType.PowerCheck);
            selectcontrol = CreateSelectList(selectlist, leftfield);
            selectcontrol[0].Disabled = true;
            selectcontrol[0].Selected = true;
            ViewBag.rightaligntestlist = selectcontrol;

            selectlist = new List<string>();
            selectlist.Add("Please select alignment channel");
            for (var idx = 0; idx < 4; idx++)
            {
                selectlist.Add(idx.ToString());
            }
            selectcontrol = CreateSelectList(selectlist, leftfield);
            selectcontrol[0].Disabled = true;
            selectcontrol[0].Selected = true;
            ViewBag.leftalignchannellist = selectcontrol;

            selectlist = new List<string>();
            selectlist.Add("Please select alignment channel");
            for (var idx = 0; idx < 4; idx++)
            {
                selectlist.Add(idx.ToString());
            }
            selectcontrol = CreateSelectList(selectlist, leftfield);
            selectcontrol[0].Disabled = true;
            selectcontrol[0].Selected = true;
            ViewBag.rightalignchannellist = selectcontrol;
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

            var waferdict = ExternalDataCollector.RetrieveNeoMapWaferList();
            var biwaferdict = BITestResultDataField.RetrieveBIWaferDict();
            foreach (var kv in biwaferdict)
            {
                if (!waferdict.ContainsKey(kv.Key))
                { waferdict.Add(kv.Key, true); }
            }

            var bijodict = BITestResultDataField.RetrieveBIJOList();
            var waferlist = waferdict.Keys.ToList();
            var bijolist = bijodict.Keys.ToList();

            waferlist.AddRange(bijolist);

            preparetxotestdata(waferlist,"","","","");

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

        private NormalDistributionData GetNormalDistrData(CleanDataWithStdDev rawdata,double comparerate)
        {
            var ret = new NormalDistributionData();
            var ylist = new List<double>();
            var valuelist = rawdata.FiltedList;
            var mean = rawdata.Mean;
            var stddev = rawdata.StdDev;

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

            var freqdict = rawdata.FrequenceDict;
            var keylist = freqdict.Keys.ToList();
            keylist.Sort();
            var amount = 0;
            var YVALUES2 = string.Empty;
            foreach (var key in keylist)
            {
                YVALUES2 = YVALUES2 + "[" + key.ToString() + "," + (freqdict[key]).ToString() + "],";
                if (freqdict[key] > amount)
                    amount = freqdict[key];
            }
            YVALUES2 = YVALUES2.Substring(0, YVALUES2.Length - 1);

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

            ret.AmountMAX = amount;
            ret.YVALUES2 = YVALUES2;

            return ret;
        }

        private Dictionary<double, int> RetrieveStdDevFrequence(double mean, double stddev, List<double> filterlist)
        {
            var ret = new Dictionary<double, int>();

            var xlist = new List<double>();
            var valrange = new List<FrequenceDataRange>();

            for (var idx = -8; idx <= 8; idx++)
            {
                var tempval = mean + 0.5 * idx * stddev;
                xlist.Add(tempval);

                var temprang = new FrequenceDataRange();
                if (idx == -8)
                {
                    temprang.lowval = filterlist[0] - stddev;
                    temprang.highval = tempval + 0.25 * stddev;
                }
                else if (idx == 8)
                {
                    temprang.lowval = tempval - 0.25 * stddev;
                    temprang.highval = filterlist[filterlist.Count - 1] + stddev;
                }
                else
                {
                    temprang.lowval = tempval - 0.25 * stddev;
                    temprang.highval = tempval + 0.25 * stddev;
                }
                valrange.Add(temprang);
            }

            foreach (var item in filterlist)
            {
                for (var idx = 0; idx < valrange.Count; idx++)
                {
                    if (item > valrange[idx].lowval && item <= valrange[idx].highval)
                    {
                        if (ret.ContainsKey(xlist[idx]))
                        {
                            ret[xlist[idx]] = ret[xlist[idx]] + 1;
                        }
                        else
                        {
                            ret.Add(xlist[idx], 1);
                        }
                        break;
                    }//end if
                }//end for
            }//end foreach

            return ret;
        }

        private CleanDataWithStdDev GetCleanDataWithStdDev(List<double> rawdata)
        {
            var ret = new CleanDataWithStdDev();

            rawdata.Sort();
            var mean = NormalDistributeAlg.Mean(rawdata);
            var stddev = NormalDistributeAlg.StandardDeviation(rawdata, mean);
            ret.Mean = mean;
            ret.StdDev = stddev;

            var startvalue = mean - 6 * stddev;
            var endvalue = mean + 6 * stddev;
            foreach (var item in rawdata)
            {
                if (item >= startvalue && item <= endvalue)
                {
                    ret.FiltedList.Add(item);
                }
                else
                {
                    ret.OutlierList.Add(item);
                }
            }
            ret.FrequenceDict = RetrieveStdDevFrequence(mean, stddev, ret.FiltedList);
            return ret;
        }


        private void NEONormalDistributeCombineChart(CleanDataWithStdDev lfilteddata, CleanDataWithStdDev rfilteddata
            , string lquerycond, string rquerycond, string datafield)
        {
            var lds = GetNormalDistrData(lfilteddata, 1.0);
            var rds = GetNormalDistrData(rfilteddata, lds.yrate);

            var xmax = lds.xmax > rds.xmax ? lds.xmax : rds.xmax;
            var xmin = lds.xmin < rds.xmin ? lds.xmin : rds.xmin;
            var ymax = lds.ymax > rds.ymax ? lds.ymax : rds.ymax;
            var ymin = lds.ymin < rds.ymin ? lds.ymin : rds.ymin;
            var amount = lds.AmountMAX > rds.AmountMAX ? lds.AmountMAX : rds.AmountMAX;

            var DATAFIELDNAME = datafield.Replace(TXOQUERYCOND.NEOMAP, "").Replace(TXOQUERYCOND.BURNIN, "").Replace(TXOQUERYCOND.TEST, "");
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
                    .Replace("#RSERIESNAME#", rquerycond)
                    .Replace("#AmountMAX#", amount.ToString())
                    .Replace("#LYVALUES2#", lds.YVALUES2)
                    .Replace("#RYVALUES2#", rds.YVALUES2)
                    .Replace("#LSERIESNAME2#", lquerycond + "-freq")
                    .Replace("#RSERIESNAME2#", rquerycond + "-freq");

            ViewBag.combinenormaldistr = scritpttxt;
        }

        private void NEOBoxPlotCombine(CleanDataWithStdDev lvm, CleanDataWithStdDev rvm
            , string lquerycond, string rquerycond, string datafield)
        {
            //{name: '#SERIESNAME#',data: [#YVALUES#]}

            var lboxdata = GetBoxPlotData(lvm);
            var rboxdata = GetBoxPlotData(rvm);

            var DATAFIELDNAME = datafield.Replace(TXOQUERYCOND.NEOMAP, "").Replace(TXOQUERYCOND.BURNIN, "").Replace(TXOQUERYCOND.TEST, "");
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

        private void NEONormalDistributeChart(CleanDataWithStdDev filteddata, string querycond, string datafield, bool left)
        {
            var ds = GetNormalDistrData(filteddata, 1.0);

            var DATAFIELDNAME = datafield.Replace(TXOQUERYCOND.NEOMAP, "").Replace(TXOQUERYCOND.BURNIN,"").Replace(TXOQUERYCOND.TEST, "");
            var Title = querycond +" "+ DATAFIELDNAME + " Normal Distribution";

            var ElementID = "leftnormaldistr";
            if (!left)
            {
                ElementID = "rightnormaldistr";
            }

            var SERIESNAME2 = datafield + "-freq";

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
                    .Replace("#SERIESNAME#", datafield)
                    .Replace("#AmountMAX#", ds.AmountMAX.ToString())
                    .Replace("#SERIESNAME2#", SERIESNAME2)
                    .Replace("#YVALUES2#", ds.YVALUES2);

            if (left)
            {
                ViewBag.leftnormaldistr = scritpttxt;
            }
            else
            {
                ViewBag.rightnormaldistr = scritpttxt;
            }
        }

        private string NEONormalDistributeChart1(CleanDataWithStdDev filteddata, string querycond, string datafield, bool left,string ElementID,string modulevalue)
        {
            var ds = GetNormalDistrData(filteddata, 1.0);

            var DATAFIELDNAME = datafield.Replace(TXOQUERYCOND.NEOMAP, "").Replace(TXOQUERYCOND.BURNIN, "").Replace(TXOQUERYCOND.TEST, "");
            var Title = querycond + " " + DATAFIELDNAME + " Normal Distribution";

            var SERIESNAME2 = datafield + "-freq";

            var scritpttxt = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/NormalDistribution1.xml"));
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
                    .Replace("#SERIESNAME#", datafield)
                    .Replace("#AmountMAX#", ds.AmountMAX.ToString())
                    .Replace("#SERIESNAME2#", SERIESNAME2)
                    .Replace("#YVALUES2#", ds.YVALUES2)
                    .Replace("#ModuleValue#", modulevalue);

            return scritpttxt;
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

        private BoxPlotData GetBoxPlotData(CleanDataWithStdDev filteddata)
        {
            var rawdata = filteddata.FiltedList;
            var ret = new BoxPlotData();
            ret.OutLierList = filteddata.OutlierList;

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

        private void NEOBoxPlot(CleanDataWithStdDev filteddata, string querycond, string datafield, bool left)
        {
            //{name: '#SERIESNAME#',data: [#YVALUES#]}
            var boxdata = GetBoxPlotData(filteddata);

            var DATAFIELDNAME = datafield.Replace(TXOQUERYCOND.NEOMAP, "").Replace(TXOQUERYCOND.BURNIN, "").Replace(TXOQUERYCOND.TEST, "");
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
            if (vm.Count > 5)
            {
                HeatMap4NeoMap(vm, querycond, datafield, left);

                var rawlist = new List<double>();
                foreach (var item in vm)
                {
                    rawlist.Add(item.value);
                }
                var filteddata = GetCleanDataWithStdDev(rawlist);

                NEONormalDistributeChart(filteddata, querycond, datafield, left);
                NEOBoxPlot(filteddata, querycond, datafield,left);
            }
        }

        private void BITestDataAnalysis(string cond, string field, string condtype, bool left)
        {
            var station = string.Empty;
            var channel = string.Empty;
            if (left)
            {
                station = Request.Form["leftbistationlist"];
                channel = Request.Form["leftbichannellist"];
            }
            else
            {
                station = Request.Form["rightbistationlist"];
                channel = Request.Form["rightbichannellist"];
            }

            var optioncond = string.Empty;
            var fieldappend = string.Empty;
            if (!string.IsNullOrEmpty(station))
            {
                optioncond = " and TestName ='" + station + "' ";
                fieldappend = "-" + station;
            }
            if (!string.IsNullOrEmpty(channel))
            {
                optioncond = optioncond + " and Channel ='" + (Convert.ToInt32(channel) + 1).ToString() + "' ";
                fieldappend = fieldappend + "-CH" + channel;
            }

            var rawlist = BITestResultDataField.RetrieveBITestData(cond, field, condtype, optioncond);
            if (rawlist.Count > 5)
            {
                var filteddata = GetCleanDataWithStdDev(rawlist);
                NEONormalDistributeChart(filteddata, cond, field+ fieldappend, left);
                NEOBoxPlot(filteddata, cond, field+ fieldappend, left);
            }
        }

        private void ModuleTestDataAnalysis(string cond, string field, string condtype, bool left)
        {
            var station = string.Empty;
            var channel = string.Empty;
            var temp = string.Empty;

            if (left)
            {
                station = Request.Form["leftmdstationlist"];
                channel = Request.Form["leftmdchannellist"];
                temp = Request.Form["leftmdtemplist"];
            }
            else
            {
                station = Request.Form["rightmdstationlist"];
                channel = Request.Form["rightmdchannellist"];
                temp = Request.Form["rightmdtemplist"];
            }

            var optioncond = string.Empty;
            var fieldappend = string.Empty;
            if (!string.IsNullOrEmpty(station))
            {
                optioncond = " and TestName ='" + station + "' ";
                fieldappend = "-" + station;
            }
            if (!string.IsNullOrEmpty(channel))
            {
                optioncond = optioncond + " and Channel ='" + channel + "' ";
                fieldappend = fieldappend + "-CH" + channel;
            }

            if (!string.IsNullOrEmpty(temp))
            {
                if (string.Compare(temp, TestTemperatureType.Low, true) == 0)
                {
                    optioncond = optioncond + " and Temperature < 15 ";
                }
                else if (string.Compare(temp, TestTemperatureType.High, true) == 0)
                {
                    optioncond = optioncond + " and Temperature > 45 ";
                }
                else
                {
                    optioncond = optioncond + " and Temperature > 15 and Temperature < 45 ";
                }
                fieldappend = fieldappend+"-" + temp;
            }

            var rawlist = ModuleTXOData.RetrieveModuleTestData(cond, field, condtype, optioncond);
            if (rawlist.Count > 5)
            {
                var filteddata = GetCleanDataWithStdDev(rawlist);
                NEONormalDistributeChart(filteddata, cond, field+ fieldappend, left);
                NEOBoxPlot(filteddata, cond, field + fieldappend, left);
            }
        }

        private void AlignTestDataAnalysis(string cond, string field, string condtype, bool left)
        {
            var station = string.Empty;
            var channel = string.Empty;
            if (left)
            {
                station = Request.Form["leftaligntestlist"];
                channel = Request.Form["leftalignchannellist"];
            }
            else
            {
                station = Request.Form["rightaligntestlist"];
                channel = Request.Form["rightalignchannellist"];
            }

            var optioncond = string.Empty;
            var fieldappend = string.Empty;
            if (!string.IsNullOrEmpty(station))
            {
                optioncond = " and TestName ='" + station + "' ";
                fieldappend = "-" + station;
            }
            if (!string.IsNullOrEmpty(channel))
            {
                optioncond = optioncond + " and Channel ='" + channel + "' ";
                fieldappend = fieldappend + "-CH" + channel;
            }

            var rawlist = AlignmentPower.RetrieveAlignmentTestData(cond, field, condtype, optioncond);
            if (rawlist.Count > 5)
            {
                var filteddata = GetCleanDataWithStdDev(rawlist);
                NEONormalDistributeChart(filteddata, cond, field + fieldappend, left);
                NEOBoxPlot(filteddata, cond, field + fieldappend, left);
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
            if (rvm.Count > 5 && lvm.Count > 5)
            {
                var lrawlist = new List<double>();
                var rrawlist = new List<double>();
                foreach (var item in lvm)
                {
                    lrawlist.Add(item.value);
                }
                foreach (var item in rvm)
                {
                    rrawlist.Add(item.value);
                }

                var lfilteddata = GetCleanDataWithStdDev(lrawlist);
                var rfilteddata = GetCleanDataWithStdDev(rrawlist);

                NEONormalDistributeCombineChart(lfilteddata, rfilteddata, lquerycond, rquerycond, ldatafield);
                NEOBoxPlotCombine(lfilteddata, rfilteddata, lquerycond, rquerycond, ldatafield);
            }
        }

        private void DataSolveWithSide(string cond,string field,Dictionary<string,bool> waferdict, Dictionary<string, bool> bijodict,bool left) 
        {
            if (!string.IsNullOrEmpty(cond)
            && !cond.ToUpper().Contains("PLEASE")
            && !string.IsNullOrEmpty(field))
            {
                var condtype = TXOQUERYTYPE.BR;
                if (waferdict.ContainsKey(cond))
                {
                    condtype = TXOQUERYTYPE.WAFER;
                }
                else if (bijodict.ContainsKey(cond))
                {
                    condtype = TXOQUERYTYPE.JO;
                }

                if (field.Contains(TXOQUERYCOND.NEOMAP))
                {
                    if (condtype.Contains(TXOQUERYTYPE.WAFER))
                    {
                        NeoMapDataAnalysis(cond, field, left);
                    }
                }
                else
                {
                    if (condtype.Contains(TXOQUERYTYPE.BR))
                    {
                        if (cond.Length > 3)
                        {
                            if (field.Contains(TXOQUERYCOND.BURNIN))
                            {
                                BITestDataAnalysis(cond, field, condtype, left);
                            }
                            if (field.Contains(TXOQUERYCOND.TEST))
                            {
                                ModuleTestDataAnalysis(cond, field, condtype, left);
                            }
                            if (field.Contains(TXOQUERYCOND.PROCESS))
                            {
                                AlignTestDataAnalysis(cond, field, condtype, left);
                            }
                        }//end if
                    }//end if
                    else
                    {
                        if (field.Contains(TXOQUERYCOND.BURNIN))
                        {
                            BITestDataAnalysis(cond, field, condtype,left);
                        }
                        if (field.Contains(TXOQUERYCOND.TEST))
                        {
                            ModuleTestDataAnalysis(cond, field, condtype, left);
                        }
                        if (field.Contains(TXOQUERYCOND.PROCESS))
                        {
                            AlignTestDataAnalysis(cond, field, condtype, left);
                        }
                    }//end else
                }//end else
            }
        }

        private void DataSolveCombine(string leftcond, string leftfield
            , string rightcond, string rightfield, Dictionary<string, bool> waferdict, Dictionary<string, bool> bijodict)
        {
                var lcondtype = TXOQUERYTYPE.BR;
                var rcondtype = TXOQUERYTYPE.BR;

                if (waferdict.ContainsKey(leftcond))
                {
                    lcondtype = TXOQUERYTYPE.WAFER;
                }
                else if (bijodict.ContainsKey(leftcond))
                {
                    lcondtype = TXOQUERYTYPE.JO;
                }

                if (waferdict.ContainsKey(rightcond))
                {
                    rcondtype = TXOQUERYTYPE.WAFER;
                }
                else if (bijodict.ContainsKey(rightcond))
                {
                    rcondtype = TXOQUERYTYPE.JO;
                }

            if (string.Compare(lcondtype, rcondtype) == 0)
            {
                if (lcondtype.Contains(TXOQUERYTYPE.BR))
                {

                }
                else if (lcondtype.Contains(TXOQUERYTYPE.JO))
                {

                }
                else if (lcondtype.Contains(TXOQUERYTYPE.WAFER))
                {
                    if (leftfield.Contains(TXOQUERYCOND.NEOMAP))
                    {
                        NeoMapDataAnalysisCombine(leftcond, leftfield, rightcond, rightfield);
                    }
                }
            }
            else
            {
                if (lcondtype.Contains(TXOQUERYTYPE.BR))
                {
                    
                }
                else if (lcondtype.Contains(TXOQUERYTYPE.JO))
                {

                }
                else if (lcondtype.Contains(TXOQUERYTYPE.WAFER))
                {
                    
                }
            }
        }

        [HttpPost, ActionName("TXOTestData")]
        [ValidateAntiForgeryToken]
        public ActionResult TXOTestDataPost()
        {
            var leftcond = Request.Form["leftquerylist"].Trim();
            var leftfield = Request.Form["leftdatafieldlist"];
            var rightcond = Request.Form["rightquerylist"].Trim();
            var rightfield = Request.Form["rightdatafieldlist"];

            if (leftfield != null)
                leftfield = leftfield.Trim();
            if (rightfield != null)
                rightfield = rightfield.Trim();

            var waferdict = ExternalDataCollector.RetrieveNeoMapWaferList();
            var biwaferdict = BITestResultDataField.RetrieveBIWaferDict();
            foreach (var kv in biwaferdict)
            {
                if (!waferdict.ContainsKey(kv.Key))
                { waferdict.Add(kv.Key, true); }
            }
            var bijodict = BITestResultDataField.RetrieveBIJOList();
            var waferlist = waferdict.Keys.ToList();
            var bijolist = bijodict.Keys.ToList();

            //if (!string.IsNullOrEmpty(leftcond)
            //        && !string.IsNullOrEmpty(leftfield)
            //        && !string.IsNullOrEmpty(rightcond)
            //        && !string.IsNullOrEmpty(rightfield)
            //        && !leftcond.ToUpper().Contains("PLEASE")
            //        && !rightcond.ToUpper().Contains("PLEASE")
            //        && string.Compare(leftfield, rightfield) == 0)
            //{
            //    DataSolveCombine(leftcond, leftfield
            //        , rightcond, rightfield, waferdict, bijodict);
            //}
            //else
            //{
                DataSolveWithSide(leftcond, leftfield, waferdict,bijodict, true);
                DataSolveWithSide(rightcond, rightfield, waferdict, bijodict, false);
            //}

            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
            ViewBag.UserName = updater.Split(new char[] { '@' })[0];
            ViewBag.RealUserID = updater;

            waferlist.AddRange(bijolist);
            preparetxotestdata(waferlist,"", "", "", "");
            return View();
        }

        private void preparemoduletxotestdata(string UserName)
        {
            ViewBag.modulequeryvallist = ProjectTestData.Last300FailedModuleForUser(UserName);
            
            var selectlist = new List<string>();
            selectlist.Add("Please select module station");
            selectlist.Add("ersetup");
            selectlist.Add("ertempcomp");
            var selectcontrol = CreateSelectList(selectlist,"");
            selectcontrol[0].Disabled = true;
            selectcontrol[0].Selected = true;
            ViewBag.leftmdstationlist = selectcontrol;


            selectlist = new List<string>();
            selectlist.Add("Please select module channel");
            for (var idx = 0; idx < 24; idx++)
            {
                selectlist.Add(idx.ToString());
            }
            selectcontrol = CreateSelectList(selectlist, "");
            selectcontrol[0].Disabled = true;
            selectcontrol[0].Selected = true;
            ViewBag.leftmdchannellist = selectcontrol;

            selectlist = new List<string>();
            selectlist.Add("Please select test temperature");
            selectlist.Add(TestTemperatureType.Low);
            selectlist.Add(TestTemperatureType.Nomal);
            selectlist.Add(TestTemperatureType.High);
            selectcontrol = CreateSelectList(selectlist, "");
            selectcontrol[0].Disabled = true;
            selectcontrol[0].Selected = true;
            ViewBag.leftmdtemplist = selectcontrol;

            selectlist = new List<string>();
            selectlist.Add("Please select alignment test");
            selectlist.Add(AlignmentPowerType.AlignmentPower);
            selectlist.Add(AlignmentPowerType.PowerCheck);
            selectcontrol = CreateSelectList(selectlist, "");
            selectcontrol[0].Disabled = true;
            selectcontrol[0].Selected = true;
            ViewBag.leftaligntestlist = selectcontrol;

            selectlist = new List<string>();
            selectlist.Add("Please select BI station");
            selectlist.Add("Pre Burn In");
            selectlist.Add("Post Burn In");
            selectcontrol = CreateSelectList(selectlist, "");
            selectcontrol[0].Disabled = true;
            selectcontrol[0].Selected = true;
            ViewBag.leftbistationlist = selectcontrol;
        }

        public ActionResult ModuleTestData()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {

            }
            else
            {
                var ck = new Dictionary<string, string>();
                ck.Add("logonredirectctrl", "DataAnalyze");
                ck.Add("logonredirectact", "ModuleTestData");
                CookieUtility.SetCookie(this, ck);
                return RedirectToAction("LoginUser", "User");
            }
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
            ViewBag.UserName = updater.Split(new char[] { '@' })[0];
            ViewBag.RealUserID = updater;
            preparemoduletxotestdata(updater);
            return View();
        }

        private void ModuleInBI(string BR,string modulevalue)
        {
            var channel = Request.Form["leftmdchannellist"];
            var bitestname = Request.Form["leftbistationlist"];

            var optioncond = string.Empty;
            var fieldappend = string.Empty;
            if (!string.IsNullOrEmpty(bitestname))
            {
                optioncond = " and TestName ='" + bitestname + "' ";
                fieldappend = "-" + bitestname;
            }
            if (!string.IsNullOrEmpty(channel))
            {
                optioncond = optioncond + " and Channel ='" + (Convert.ToInt32(channel)+1).ToString() + "' ";
                fieldappend = fieldappend + "-CH" + channel;
            }

            var rawlist = BITestResultDataField.RetrieveBITestData(BR, TXOQUERYCOND.BURNIN + "PO_LD", TXOQUERYTYPE.BR, optioncond);
            if (rawlist.Count > 5)
            {
                var filteddata = GetCleanDataWithStdDev(rawlist);
                ViewBag.leftbinormaldistr =  NEONormalDistributeChart1(filteddata, BR.Replace("-", ""), "TxPower" + fieldappend, true, "leftbinormaldistr",modulevalue);
            }
        }

        private void ModuleInAlign(string BR, string modulevalue)
        {
            var channel = Request.Form["leftmdchannellist"];
            var aligntestname = Request.Form["leftaligntestlist"];

            var optioncond = string.Empty;
            var fieldappend = string.Empty;
            if (!string.IsNullOrEmpty(aligntestname))
            {
                optioncond = " and TestName ='" + aligntestname + "' ";
                fieldappend = "-" + aligntestname;
            }
            if (!string.IsNullOrEmpty(channel))
            {
                optioncond = optioncond + " and Channel ='" + channel + "' ";
                fieldappend = fieldappend + "-CH" + channel;
            }

            var rawlist = AlignmentPower.RetrieveAlignmentTestData(BR, TXOQUERYCOND.PROCESS + "TxPower", TXOQUERYTYPE.BR, optioncond);
            if (rawlist.Count > 5)
            {
                var filteddata = GetCleanDataWithStdDev(rawlist);
                ViewBag.alignnormaldistr = NEONormalDistributeChart1(filteddata, BR.Replace("-", ""), "TxPower" + fieldappend, true, "alignnormaldistr", modulevalue);
            }
        }

        private void ModuleInModule(string BR, string modulevalue)
        {
            var channel = Request.Form["leftmdchannellist"];
            var temperature = Request.Form["leftmdtemplist"];
            var moduletestname = Request.Form["leftmdstationlist"];

            var optioncond = string.Empty;
            var fieldappend = string.Empty;
            if (!string.IsNullOrEmpty(moduletestname))
            {
                optioncond = " and TestName ='" + moduletestname + "' ";
                fieldappend = "-" + moduletestname;
            }
            if (!string.IsNullOrEmpty(channel))
            {
                optioncond = optioncond + " and Channel ='" + channel + "' ";
                fieldappend = fieldappend + "-CH" + channel;
            }

            if (!string.IsNullOrEmpty(temperature))
            {
                if (string.Compare(temperature, TestTemperatureType.Low, true) == 0)
                {
                    optioncond = optioncond + " and Temperature < 15 ";
                }
                else if (string.Compare(temperature, TestTemperatureType.High, true) == 0)
                {
                    optioncond = optioncond + " and Temperature > 45 ";
                }
                else
                {
                    optioncond = optioncond + " and Temperature > 15 and Temperature < 45 ";
                }
                fieldappend = fieldappend + "-" + temperature;
            }

            var rawlist = ModuleTXOData.RetrieveModuleTestData(BR, TXOQUERYCOND.TEST + "TxPower", TXOQUERYTYPE.BR, optioncond);
            if (rawlist.Count > 5)
            {
                var filteddata = GetCleanDataWithStdDev(rawlist);
                ViewBag.modulenormaldistr = NEONormalDistributeChart1(filteddata, BR.Replace("-", ""), "TxPower" + fieldappend, true, "modulenormaldistr", modulevalue);
            }
        }

        private List<BITestResultDataField> RetrieveModuleTestDataBySN_BI(string ModuleSn)
        {
            var channel = Request.Form["leftmdchannellist"];
            var bitestname = Request.Form["leftbistationlist"];

            var optioncond = string.Empty;
            if (!string.IsNullOrEmpty(bitestname))
            {
                optioncond = " and TestName ='" + bitestname + "' ";
            }
            if (!string.IsNullOrEmpty(channel))
            {
                optioncond = optioncond + " and Channel ='" + (Convert.ToInt32(channel) + 1).ToString() + "' ";
            }

            return BITestResultDataField.RetrieveAllDataFieldBySN(ModuleSn, optioncond);
        }

        private List<AlignmentPower> RetrieveModuleTestDataBySN_AG(string ModuleSn)
        {
            var channel = Request.Form["leftmdchannellist"];
            var aligntestname = Request.Form["leftaligntestlist"];

            var optioncond = string.Empty;
            if (!string.IsNullOrEmpty(aligntestname))
            {
                optioncond = " and TestName ='" + aligntestname + "' ";
            }
            if (!string.IsNullOrEmpty(channel))
            {
                optioncond = optioncond + " and Channel ='" + channel + "' ";
            }

            return AlignmentPower.RetrieveAllDataFieldBySN(ModuleSn, optioncond);
        }

        private List<ModuleTXOData> RetrieveModuleTestDataBySN_MD(string ModuleSn)
        {
            var channel = Request.Form["leftmdchannellist"];
            var temperature = Request.Form["leftmdtemplist"];
            var moduletestname = Request.Form["leftmdstationlist"];

            var optioncond = string.Empty;
            if (!string.IsNullOrEmpty(moduletestname))
            {
                optioncond = " and TestName ='" + moduletestname + "' ";
            }
            if (!string.IsNullOrEmpty(channel))
            {
                optioncond = optioncond + " and Channel ='" + channel + "' ";
            }

            if (!string.IsNullOrEmpty(temperature))
            {
                if (string.Compare(temperature, TestTemperatureType.Low, true) == 0)
                {
                    optioncond = optioncond + " and Temperature < 15 ";
                }
                else if (string.Compare(temperature, TestTemperatureType.High, true) == 0)
                {
                    optioncond = optioncond + " and Temperature > 45 ";
                }
                else
                {
                    optioncond = optioncond + " and Temperature > 15 and Temperature < 45 ";
                }
            }

            return ModuleTXOData.RetrieveAllDataFieldBySN(ModuleSn, optioncond);
        }


        [HttpPost, ActionName("ModuleTestData")]
        [ValidateAntiForgeryToken]
        public ActionResult ModuleTestDataPost()
        {
            var ModuleSn = Request.Form["ModuleSNList"];
            ViewBag.ModuleSn = ModuleSn;

            var bitestdata = RetrieveModuleTestDataBySN_BI(ModuleSn);
            var aligntestdata = RetrieveModuleTestDataBySN_AG(ModuleSn);
            var moduletestdata = RetrieveModuleTestDataBySN_MD(ModuleSn);

            ViewBag.bitestdata = bitestdata;
            ViewBag.aligntestdata = aligntestdata;
            ViewBag.moduletestdata = moduletestdata;
            if (bitestdata.Count > 0 || aligntestdata.Count > 0 || moduletestdata.Count > 0)
                ViewBag.hasdata = true;

            if (bitestdata.Count > 0)
            {
                var jo = bitestdata[0].JO;
                var jos = jo.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                if (jos.Length == 3)
                {
                    var BR = "-" + jos[1] + "-";
                    ModuleInBI(BR,bitestdata[bitestdata.Count-1].PO_LD.ToString());
                }
            }

            if (aligntestdata.Count > 0)
            {
                var jo = aligntestdata[0].JO;
                var jos = jo.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                if (jos.Length == 3)
                {
                    var BR = "-" + jos[1] + "-";
                    ModuleInAlign(BR, aligntestdata[aligntestdata.Count -1].TxPower.ToString());
                }
            }

            if (moduletestdata.Count > 0)
            {
                var jo = moduletestdata[0].JO;
                var jos = jo.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                if (jos.Length == 3)
                {
                    var BR = "-" + jos[1] + "-";
                    ModuleInModule(BR, moduletestdata[moduletestdata.Count -1].TxPower.ToString());
                }
            }

            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
            ViewBag.UserName = updater.Split(new char[] { '@' })[0];
            ViewBag.RealUserID = updater;
            preparemoduletxotestdata(updater);
            return View();
        }

    }
}