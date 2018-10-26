using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Prometheus.Models;
using System.Text;
using System.Web.Routing;
using System.Net;
using MathNet.Numerics.Statistics;

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

        private static string resizeimg(string imgstr)
        {
            var srcidx = imgstr.IndexOf("src=\"");
            var srceidx = imgstr.IndexOf("\"", srcidx + 6);
            var srcstr = imgstr.Substring(srcidx, (srceidx + 1 - srcidx));
            return "<div style=\"text-align: center;\">" + "<img " + srcstr + " style=\"max-width: 90%; height: auto;\" /></div>";
        }
        public static string ResizeImageFromHtml(string src)
        {
            var startidx = 0;
            while (src.IndexOf("<img", startidx) != -1)
            {
                var imgsidx = src.IndexOf("<img", startidx);
                var imgeidx = src.IndexOf(">", imgsidx);
                if (imgeidx != -1)
                {
                    startidx = imgeidx;
                    imgeidx = imgeidx + 1;
                    var imgstr = src.Substring(imgsidx, (imgeidx - imgsidx));
                    var nimgstr = resizeimg(imgstr);
                    src = src.Remove(imgsidx, imgeidx - imgsidx).Insert(imgsidx, nimgstr);
                }
                else
                {
                    startidx = imgsidx + 3;
                }
            }
            return src.Replace("</img>", "");
        }

        public JsonResult RetrieveWaferReport()
        {
            var id = Request.Form["reportid"];
            var wreportlist = IssueViewModels.RetrieveWaferReport(id);
            if (wreportlist.Count == 0)
            {
                var tempvm = new IssueComments();
                tempvm.Comment = "TO BE EDIT";
                IssueViewModels.StoreIssueComment(id, tempvm.dbComment, "System", COMMENTTYPE.Description);

                var report = new
                {
                    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    reporter = "System",
                    content = "TO BE EDIT"
                };
                
                var ret = new JsonResult();
                ret.Data = new
                {
                    success = true,
                    report = report
                };
                return ret;
            }
            else
            {
                var report = new
                {
                    time = wreportlist[0].CommentDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    reporter = wreportlist[0].Reporter.ToUpper().Replace("@FINISAR.COM", ""),
                    content = ResizeImageFromHtml(wreportlist[0].Comment)
                };
                var ret = new JsonResult();
                ret.Data = new
                {
                    success = true,
                    report = report
                };
                return ret;
            }
        }

        public ActionResult ModifyWaferReport(string reportid)
        {
            var wreportlist = IssueViewModels.RetrieveWaferReport(reportid);
            if (wreportlist.Count > 0)
            {
                return View(wreportlist[0]);
            }
            return View();
        }

        [HttpPost, ActionName("ModifyWaferReport")]
        [ValidateAntiForgeryToken]
        public ActionResult ModifyWaferReportPost()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];

            var reportid = Request.Form["HIssueKey"];
            var commenttype = Request.Form["HType"];
            var commentdate = Request.Form["HDate"];


            if (!string.IsNullOrEmpty(Request.Form["editor1"]))
            {
                var tempcommment = new IssueComments();
                tempcommment.Comment = SeverHtmlDecode.Decode(this, Request.Form["editor1"]);
                IssueViewModels.UpdateWaferReport(reportid, tempcommment.dbComment,updater);
            }
            else
            {
                var tempcommment = new IssueComments();
                tempcommment.Comment = "<p>To Be Edit</p>";
                IssueViewModels.UpdateWaferReport(reportid, tempcommment.dbComment,updater);
            }

            if (reportid.Contains("HTOL_"))
            {
                var dict = new RouteValueDictionary();
                dict.Add("defaultwafer", reportid.Replace("HTOL_", ""));
                return RedirectToAction("HTOLDistribution", "DataAnalyze", dict);
            }
            else
            {
                var dict = new RouteValueDictionary();
                dict.Add("defaultwafer", reportid.Replace("BURNIN_", ""));
                return RedirectToAction("WaferDistribution", "DataAnalyze", dict);
            }
        }

        public static string DetermineCompName(string IP)
        {
            try
            {
                IPAddress myIP = IPAddress.Parse(IP);
                IPHostEntry GetIPHost = Dns.GetHostEntry(myIP);
                List<string> compName = GetIPHost.HostName.ToString().Split('.').ToList();
                return compName.First();
            }
            catch (Exception ex)
            { return string.Empty; }
        }

        public ActionResult ShipmentData()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var username = "";
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {
                username = ckdict["logonuser"].Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries)[0];
            }
            string IP = Request.UserHostName;
            string compName = DetermineCompName(IP);
            if (!MachineUserMap.IsLxEmployee(compName, username,8))
            {
                return RedirectToAction("UserCenter", "User");
            }
            LogVM.WriteLog(username.ToUpper(), "ALL", compName,
                                   "/DataAnalyze/ShipmentData", "DataAnalyze", "View", "", LogType.Default, Log4NetLevel.Warn, "ShipmentData");

            ViewBag.IsL9Employee = MachineUserMap.IsLxEmployee(compName, username, 9);
            return View();
        }

        ////<date,<customer,int>>
        private object GetShipmentChartData(Dictionary<string, Dictionary<string, double>> shipdata,Dictionary<string,int> vcselrmacntdict, Dictionary<string, int> allrmacntdict, string rate,string producttype)
        {
            var id = "shipdata_" + rate + "_id";
            var shipdatelist = shipdata.Keys.ToList();
            shipdatelist.Sort(delegate (string obj1, string obj2)
            {
                var d1 = DateTime.Parse(obj1 + "-01 00:00:00");
                var d2 = DateTime.Parse(obj2 + "-01 00:00:00");
                return d1.CompareTo(d2);
            });

            var datecntdict = new Dictionary<string, double>();
            foreach (var kv in shipdata)
            {
                var totle = 0.0;
                foreach (var nd in kv.Value)
                {
                    totle = totle + nd.Value;
                }
                datecntdict.Add(kv.Key, totle);
            }

            var colorlist = new string[] { "#161525", "#00A0E9", "#bada55", "#1D2088" ,"#00ff00", "#fca2cf", "#E60012", "#EB6100", "#E4007F"
                , "#CFDB00", "#8FC31F", "#22AC38", "#920783",  "#b5f2b0", "#F39800","#4e92d2" , "#FFF100"
                , "#1bfff5", "#4f4840", "#FCC800", "#0068B7", "#6666ff", "#009B6B", "#16ff9b" }.ToList();

            var namelist = shipdata[shipdatelist[0]].Keys.ToList();

            var custsumpair = new List<KeyValuePair<string, double>>();
            var cidx = 0;
            foreach (var name in namelist)
            {
                var namecnt = new List<double>();
                foreach (var x in shipdatelist)
                {
                    namecnt.Add(shipdata[x][name]);
                }
                custsumpair.Add(new KeyValuePair<string, double>(name, namecnt.Sum()));
            }
            custsumpair.Sort(delegate (KeyValuePair<string, double> obj1, KeyValuePair<string, double> obj2)
            {
                return obj2.Value.CompareTo(obj1.Value);
            });
            var newnamelist = new List<string>();
            foreach (var item in custsumpair)
            {
                newnamelist.Add(item.Key);
            }
            if (newnamelist.Contains("OTHERS"))
            { newnamelist.Remove("OTHERS"); newnamelist.Insert(0, "OTHERS"); }


            var lastdidx = shipdatelist.Count - 1;
            var title = shipdatelist[0] + " ~ " + shipdatelist[lastdidx] +" "+producttype+ " Shipment Distribution vs DPPM (" + rate + ")";
            var xdata = new List<string>();
            var ydata = new List<object>();

            var cussumlist = new List<double>();

            foreach (var f_item in shipdatelist)
            {
                xdata.Add(f_item);
            }
            var xAxis = new { data = xdata };

            var yAxis = new
            {
                title = "Amount"
            };

            cidx = 0;
            foreach (var name in newnamelist)
            {
                var namecnt = new List<double>();
                foreach (var x in shipdatelist)
                {
                    namecnt.Add(shipdata[x][name]);
                }

                cussumlist.Add(namecnt.Sum());

                ydata.Add(new
                {
                    name = name,
                    data = namecnt,
                    color = colorlist[cidx]
                });
                cidx += 1;
            }

            var totalship = cussumlist.Sum();
            var customerrate = new List<string>();
            cidx = 0;
            foreach (var cs in cussumlist)
            {
                customerrate.Add(newnamelist[cidx] + ":" + Math.Round(cs / totalship * 100.0, 2) + "%");
                cidx += 1;
            }
            customerrate.Add(""); customerrate.Add(""); customerrate.Add("");

            var ddata = new List<double>();
            foreach (var x in shipdatelist)
            {
                if (vcselrmacntdict.ContainsKey(x))
                {
                    ddata.Add(Math.Round((double)vcselrmacntdict[x] / datecntdict[x] * 1000000, 0));
                }
                else
                {
                    ddata.Add(0.0);
                }
            }
            ydata.Add(new
            {
                name = "VCSEL RMA DPPM",
                type = "line",
                data = ddata,
                yAxis = 1
            });

            //ddata = new List<double>();
            //foreach (var x in shipdatelist)
            //{
            //    if (allrmacntdict.ContainsKey(x))
            //    {
            //        ddata.Add(Math.Round((double)allrmacntdict[x] / datecntdict[x] * 1000000, 0));
            //    }
            //    else
            //    {
            //        ddata.Add(0.0);
            //    }
            //}
            //ydata.Add(new
            //{
            //    name = "ALL RMA DPPM",
            //    type = "line",
            //    data = ddata,
            //    yAxis = 1
            //});

            return new
            {
                id = id,
                title = title,
                xAxis = xAxis,
                yAxis = yAxis,
                data = ydata,
                rate = rate,
                customerrate = customerrate
            };
        }

        private object GetOrderQtyChartData(Dictionary<string, Dictionary<string, double>> shipdata, string rate, string producttype)
        {
            var id = "orderdata_" + rate + "_id";
            var shipdatelist = shipdata.Keys.ToList();
            shipdatelist.Sort(delegate (string obj1, string obj2)
            {
                var d1 = DateTime.Parse(obj1 + "-01 00:00:00");
                var d2 = DateTime.Parse(obj2 + "-01 00:00:00");
                return d1.CompareTo(d2);
            });

            var datecntdict = new Dictionary<string, double>();
            foreach (var kv in shipdata)
            {
                var totle = 0.0;
                foreach (var nd in kv.Value)
                {
                    totle = totle + nd.Value;
                }
                datecntdict.Add(kv.Key, totle);
            }

            var namelist = shipdata[shipdatelist[0]].Keys.ToList();

            var lastdidx = shipdatelist.Count - 1;
            var title = shipdatelist[0] + " ~ " + shipdatelist[lastdidx] + " " + producttype + " Order QTY Distribution (" + rate + ")";
            var xdata = new List<string>();
            var ydata = new List<object>();

            foreach (var f_item in shipdatelist)
            {
                xdata.Add(f_item);
            }
            var xAxis = new { data = xdata };

            var yAxis = new
            {
                title = "Amount"
            };

            foreach (var name in namelist)
            {
                var namecnt = new List<double>();
                foreach (var x in shipdatelist)
                {
                    namecnt.Add(shipdata[x][name]);
                }

                ydata.Add(new
                {
                    name = name,
                    data = namecnt
                });
            }

            //var ddata = new List<double>();
            //foreach (var x in shipdatelist)
            //{
            //    if (vcselrmacntdict.ContainsKey(x))
            //    {
            //        ddata.Add(Math.Round((double)vcselrmacntdict[x] / datecntdict[x] * 1000000, 0));
            //    }
            //    else
            //    {
            //        ddata.Add(0.0);
            //    }
            //}
            //ydata.Add(new
            //{
            //    name = "VCSEL RMA DPPM",
            //    type = "line",
            //    data = ddata,
            //    yAxis = 1
            //});


            return new
            {
                id = id,
                title = title,
                xAxis = xAxis,
                yAxis = yAxis,
                data = ydata,
                rate = rate
            };
        }


        private object GetOTDChartData(List<FsrShipData> otddata, string rate, string producttype)
        {
            var id = "otddata_" + rate + "_id";
            var title = producttype + " Monthly OTD Bills Distribution (" + rate + ")";

            var ordernumdict = new Dictionary<string, double>();
            var otdnumdict = new Dictionary<string, double>();
            var otddict = new Dictionary<string, double>();

            foreach (var data in otddata)
            {
                var orderdatekey = data.OPD.ToString("yyyy-MM");
                if (ordernumdict.ContainsKey(orderdatekey))
                { ordernumdict[orderdatekey] += 1; }
                else
                { ordernumdict.Add(orderdatekey, 1); }

                if (data.ShipDate <= data.OPD)
                {
                    if (otdnumdict.ContainsKey(orderdatekey))
                    { otdnumdict[orderdatekey] += 1; }
                    else
                    { otdnumdict.Add(orderdatekey, 1); }
                }
            }

            var orderkeys = ordernumdict.Keys.ToList();
            foreach (var o in orderkeys)
            {
                if (otdnumdict.ContainsKey(o))
                { otddict.Add(o, Math.Round(otdnumdict[o] / ordernumdict[o] * 100.0, 2)); }
                else
                { otddict.Add(o, 0.0); }
            }

            orderkeys.Sort(delegate (string obj1, string obj2)
            {
                var d1 = DateTime.Parse(obj1 + "-01 00:00:00");
                var d2 = DateTime.Parse(obj2 + "-01 00:00:00");
                return d1.CompareTo(d2);
            });

            var xdata = new List<string>();
            var ordernumchartdata = new List<double>();
            var otdchartdata = new List<double>();
            foreach (var k in orderkeys)
            {
                xdata.Add(k);
                ordernumchartdata.Add(ordernumdict[k]);
                otdchartdata.Add(otddict[k]);
            }

            var ordernumchart = new
            {
                type = "column",
                name = "orders bills",
                data = ordernumchartdata,
                yAxis = 1
            };

            var otdchart = new
            {
                type = "line",
                name = "ODT rate",
                data = otdchartdata,
                dataLabels= new {
                enabled = true
                }
            };

            var chartdata = new List<object>();
            chartdata.Add(ordernumchart);
            chartdata.Add(otdchart);

            return new {
                id = id,
                title = title,
                xdata = xdata,
                chartdata = chartdata
            };
        }

        public JsonResult ShipmentDistribution()
        {
            var ssdate = Request.Form["sdate"];
            var sedate = Request.Form["edate"];
            var startdate = DateTime.Now;
            var enddate = DateTime.Now;

            if (!string.IsNullOrEmpty(ssdate) && !string.IsNullOrEmpty(sedate))
            {
                var sdate = DateTime.Parse(Request.Form["sdate"]);
                var edate = DateTime.Parse(Request.Form["edate"]);
                if (sdate < edate)
                {
                    startdate = DateTime.Parse(sdate.ToString("yyyy-MM") + "-01 00:00:00");
                    enddate = DateTime.Parse(edate.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddSeconds(-1);
                }
                else
                {
                    startdate = DateTime.Parse(edate.ToString("yyyy-MM") + "-01 00:00:00");
                    enddate = DateTime.Parse(sdate.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddSeconds(-1);
                }
            }
            else
            {
                startdate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(-6);
                enddate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddSeconds(-1);
            }

            //<date,<customer,int>>
            var shipdata25g = FsrShipData.RetrieveShipDataByMonth(VCSELRATE.r25G, SHIPPRODTYPE.PARALLEL, startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), this);
            var shipdata14g = FsrShipData.RetrieveShipDataByMonth(VCSELRATE.r14G, SHIPPRODTYPE.PARALLEL, startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), this);
            var shipdataarray = new List<object>();
            if (shipdata25g.Count > 0)
            {
                var vcselrmacntdict = VcselRMAData.RetrieveRMACntByMonth(startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), VCSELRATE.r25G);
                //var allrmacntdict = ExternalDataCollector.RetrieveRMACntByMonth(startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), VCSELRATE.r25G);
                var allrmacntdict = new Dictionary<string, int>();
                shipdataarray.Add(GetShipmentChartData(shipdata25g, vcselrmacntdict, allrmacntdict, VCSELRATE.r25G, SHIPPRODTYPE.PARALLEL));
            }
            if (shipdata14g.Count > 0)
            {
                var vcselrmacntdict = VcselRMAData.RetrieveRMACntByMonth(startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), VCSELRATE.r14G);
                //var allrmacntdict = ExternalDataCollector.RetrieveRMACntByMonth(startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), VCSELRATE.r14G);
                var allrmacntdict = new Dictionary<string, int>();
                shipdataarray.Add(GetShipmentChartData(shipdata14g, vcselrmacntdict, allrmacntdict, "10G_14G", SHIPPRODTYPE.PARALLEL));
            }

            var orderdata25g = FsrShipData.RetrieveOrderDataByMonth(VCSELRATE.r25G, SHIPPRODTYPE.PARALLEL, startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), this);
            var orderdata14g = FsrShipData.RetrieveOrderDataByMonth(VCSELRATE.r14G, SHIPPRODTYPE.PARALLEL, startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), this);
            var orderdataarray = new List<object>();
            if (orderdata25g.Count > 0)
            {
                //var vcselrmacntdict = VcselRMAData.RetrieveRMACntByMonth(startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), VCSELRATE.r25G);
                //var allrmacntdict = ExternalDataCollector.RetrieveRMACntByMonth(startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), VCSELRATE.r25G);
                orderdataarray.Add(GetOrderQtyChartData(orderdata25g, VCSELRATE.r25G, SHIPPRODTYPE.PARALLEL));
            }
            if (orderdata14g.Count > 0)
            {
                //var vcselrmacntdict = VcselRMAData.RetrieveRMACntByMonth(startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), VCSELRATE.r14G);
                //var allrmacntdict = ExternalDataCollector.RetrieveRMACntByMonth(startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), VCSELRATE.r14G);
                orderdataarray.Add(GetOrderQtyChartData(orderdata14g, "10G_14G", SHIPPRODTYPE.PARALLEL));
            }

            var otdarray = new List<object>();
            var otd25g = FsrShipData.RetrieveOTDByMonth(VCSELRATE.r25G, SHIPPRODTYPE.PARALLEL, startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), this);
            var otd14g = FsrShipData.RetrieveOTDByMonth(VCSELRATE.r14G, SHIPPRODTYPE.PARALLEL, startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), this);
            if (otd25g.Count > 0)
            {
                otdarray.Add(GetOTDChartData(otd25g, VCSELRATE.r25G, SHIPPRODTYPE.PARALLEL));
            }
            if (otd14g.Count > 0)
            {
                otdarray.Add(GetOTDChartData(otd14g, "10G_14G", SHIPPRODTYPE.PARALLEL));
            }
            var otdall = FsrShipData.RetrieveOTDByMonth("", SHIPPRODTYPE.PARALLEL, startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), this);
            if (otdall.Count > 0)
            {
                otdarray.Add(GetOTDChartData(otdall, "ALL", SHIPPRODTYPE.PARALLEL));
            }

            var ret = new JsonResult();
            ret.Data = new
            {
                success = true,
                shipdataarray = shipdataarray,
                orderdataarray = orderdataarray,
                otdarray = otdarray
            };
            return ret;
        }

        private void PrepareShipmentData(System.IO.FileStream fw, string ssdate, string sedate)
        {
            var startdate = DateTime.Now;
            var enddate = DateTime.Now;

            if (!string.IsNullOrEmpty(ssdate) && !string.IsNullOrEmpty(sedate))
            {
                var sdate = DateTime.Parse(ssdate);
                var edate = DateTime.Parse(sedate);
                if (sdate < edate)
                {
                    startdate = DateTime.Parse(sdate.ToString("yyyy-MM") + "-01 00:00:00");
                    enddate = DateTime.Parse(edate.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddSeconds(-1);
                }
                else
                {
                    startdate = DateTime.Parse(edate.ToString("yyyy-MM") + "-01 00:00:00");
                    enddate = DateTime.Parse(sdate.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddSeconds(-1);
                }
            }
            else
            {
                startdate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(-6);
                enddate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddSeconds(-1);
            }

            var shipdata = FsrShipData.RetrieveAllShipDataByMonth(startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"), this);
            var sb = new StringBuilder(shipdata.Count*200);
            sb.Append("ShipID,ShipQty,PN,ProdDesc,MarketFamily,Configuration,ShipDate,Original Promise Date,CustomerNum,Customer1,Customer2,OrderedDate,DelieveNum,VcselType,\r\n");
            foreach (var item in shipdata)
            {
                sb.Append("\"" + item.ShipID.Replace("\"", "") + "\"," + "\"" + item.ShipQty.ToString().Replace("\"", "") + "\"," + "\"" + item.PN.Replace("\"", "") + "\","
                    + "\"" + item.ProdDesc.Replace("\"", "") + "\"," + "\"" + item.MarketFamily.Replace("\"", "") + "\"," + "\"" + item.Configuration.Replace("\"", "") + "\"," 
                    + "\"" + item.ShipDate.ToString("yyyy-MM-dd HH:mm:ss").Replace("\"", "") + "\","+ "\"" + item.OPD.ToString("yyyy-MM-dd HH:mm:ss").Replace("\"", "") + "\"," 
                    + "\"" + item.CustomerNum.Replace("\"", "") + "\"," + "\"" + item.Customer1.Replace("\"", "") + "\"," + "\"" + item.Customer2.Replace("\"", "") + "\","
                    + "\"" + item.OrderedDate.ToString("yyyy-MM-dd HH:mm:ss").Replace("\"", "") + "\"," + "\"" + item.DelieveNum.Replace("\"", "") + "\"," 
                    + "\"" + item.VcselType.Replace("\"", "") + "\",\r\n");
            }

            var bt = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            fw.Write(bt, 0, bt.Count());
            sb.Clear();
        }

        public ActionResult DownloadShipmentData(string sdate,string edate)
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var username = "";
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {
                username = ckdict["logonuser"].Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries)[0];
            }
            string IP = Request.UserHostName;
            string compName = DetermineCompName(IP);
            if (!MachineUserMap.IsLxEmployee(compName, username, 9))
            {
                return RedirectToAction("UserCenter", "User");
            }

            string datestring = DateTime.Now.ToString("yyyyMMdd");
            string imgdir = Server.MapPath("~/userfiles") + "\\docs\\" + datestring + "\\";
            if (!System.IO.Directory.Exists(imgdir))
            {
                System.IO.Directory.CreateDirectory(imgdir);
            }

            var fn = "shipment_"+sdate+"-"+edate+"_" + "_data_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
            var filename = imgdir + fn;

            var fw = System.IO.File.OpenWrite(filename);
            PrepareShipmentData(fw,sdate,edate);
            fw.Close();

            LogVM.WriteLog(username.ToUpper(), "ALL", compName,
                       "/DataAnalyze/DownloadShipmentData", "DataAnalyze", "DownLoad", "", LogType.Default, Log4NetLevel.Warn, "DownloadShipmentData");

            return File(filename, "application/vnd.ms-excel", fn);
        }

        public ActionResult VcselRMA(string rate)
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            var username = "";
            if (ckdict.ContainsKey("logonuser") && !string.IsNullOrEmpty(ckdict["logonuser"]))
            {
                username = ckdict["logonuser"].Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries)[0];
            }
            string IP = Request.UserHostName;
            string compName = DetermineCompName(IP);
            if (!MachineUserMap.IsLxEmployee(compName, username, 7))
            {
                return RedirectToAction("UserCenter", "User");
            }

            var defval = " ";
            if (!string.IsNullOrEmpty(rate))
            { defval = rate; }

            var rlist = VcselRMAData.RetrieveVcselType();
            var ratelist = new List<string>();
            ratelist.Add(" ");
            ratelist.AddRange(rlist);

            ViewBag.vcselratelist = CreateSelectList(ratelist, defval);

            LogVM.WriteLog(username.ToUpper(), "ALL", compName,
                                   "/DataAnalyze/VcselRMA", "DataAnalyze", "View", "", LogType.Default, Log4NetLevel.Warn, "VcselRMA");

            return View();
        }

        public JsonResult VcselRMADppmData()
        {
            var rate = Request.Form["rate"];
            var dppmlist = VcselRMASum.RetrieveVcselDPPM(rate);

            var xdatalist = new List<string>();
            var dppmdatalist = new List<double>();
            var shippeddatalist = new List<double>();
            foreach (var dppm in dppmlist)
            {
                xdatalist.Add(dppm.Wafer);
                dppmdatalist.Add(dppm.DPPM);
                shippeddatalist.Add(dppm.ShippedQty);
            }

            var plotbands = new List<object>();

            var latestwaferlist = VcselRMAData.RetrieveLatestWafer(rate);
            if (latestwaferlist.Count > 0)
            {
                var latestwafer = latestwaferlist[0].Wafer;
                var from = 0.0;
                var to = 0.0;
                var idx = 0.0;
                foreach (var item in xdatalist)
                {
                    if (string.Compare(item,latestwafer,true) == 0)
                    {
                        from = idx - 0.5;
                        to = idx + 0.5;
                        break;
                    }
                    idx = idx + 1.0;
                }
                plotbands.Add(
                    new {
                        color = "#00b050",
                        from = from,
                        to = to,
                    }
                    );
            }

            var xAxis = new {
                data = xdatalist
            };
            var yAxis = new {
                title = "Dppm"
            };
            var dppmdata = new {
                name = "Dppm",
                color = "#5CB85C",
                data = dppmdatalist
            };
            var shipdata = new {
                name = "Shipped",
                color = "#12CC92",
                data = shippeddatalist
            };
            var alldata = new {
                data = dppmdata,
                cdata = shipdata
            };
            var dppmline = new {
                id = "vcsel_dppm",
                title = "Vcsel RMA Dppm",
                xAxis = xAxis,
                yAxis = yAxis,
                data = alldata,
                plotbands = plotbands
            };

            var ret = new JsonResult();
            ret.Data = new {
                success = true,
                dppmline = dppmline
            };
            return ret;
        }

        public JsonResult RetrieveVcselRMARawData()
        {
            var wafer = Request.Form["wafer"];
            var waferdatalist = VcselRMAData.RetrieveWaferRawData(wafer);
            var ret = new JsonResult();
            ret.Data = new { waferdatalist = waferdatalist };
            return ret;
        }

        public JsonResult RetrieveVcselRMARawDataByMonth()
        {
            var datestr = Request.Form["datestr"];
            var rate = Request.Form["rate"];
            var sdate = datestr + "-01 00:00:00";
            var edate = DateTime.Parse(sdate).AddMonths(1).AddSeconds(-1).ToString("yyyy-MM-dd HH:mm:ss");
            var waferdatalist = VcselRMAData.RetrieveWaferRawDataByMonth(sdate, edate, rate);
            var ret = new JsonResult();
            ret.Data = new { waferdatalist = waferdatalist };
            return ret;
        }

        public JsonResult VcselRMAMileStoneData(string datetype)
        {
            var combinexdict = new Dictionary<string, bool>();
            var retdata = new List<object>();
            var id = "vcsel_milestone";
            var title = "Vcsel RMA Statistic By Build Month";
            if (!string.IsNullOrEmpty(datetype) && datetype.ToUpper().Contains("SHIPDATE"))
            {
                retdata = VcselRMASum.VcselRMAMileStoneDataByShipDate();
                id = "vcsel_milestone_ship";
                title = "Vcsel RMA Statistic By Ship Month";
            }
            else
            {
                retdata = VcselRMASum.VcselRMAMileStoneDataByBuildDate();
                id = "vcsel_milestone";
                title = "Vcsel RMA Statistic By Build Month";
            }


            //dict<month,dict<rate,count>>
            var monthlyrma = (Dictionary<string, Dictionary<string, int>>)retdata[0];
            var milestonedata = (List<EngineeringMileStone>)retdata[1];
            var vratecolordict = (Dictionary<string, string>)retdata[2];


            var vratelist = vratecolordict.Keys.ToList();
            vratelist.Sort();

            foreach (var item in monthlyrma)
            {
                if (!combinexdict.ContainsKey(item.Key))
                { combinexdict.Add(item.Key,true); }
            }

            var milestonedict = new Dictionary<string, string>();
            foreach (var item in milestonedata)
            {
                var month = item.ActionDate.ToString("yyyy-MM");
                if (!combinexdict.ContainsKey(month))
                { combinexdict.Add(month, true); }

                if (milestonedict.ContainsKey(month))
                {
                    milestonedict[month] = milestonedict[month] + "<br/>" + item.Location + "_" + item.ActionDetail;
                }
                else
                {
                    milestonedict.Add(month, item.Location + "_" + item.ActionDetail);
                }
            }


            var combinxlist = combinexdict.Keys.ToList();
            combinxlist.Sort();

            //dict<rate,countlist>
            var ratecountbydate = new Dictionary<string, List<int>>();
            var maxrmacount = 0;
            //get rate count list of each date
            foreach (var date in combinxlist)
            {
                if (monthlyrma.ContainsKey(date))
                {
                    var ratedict = monthlyrma[date];
                    var datecount = 0;
                    foreach (var ratekv in ratedict)
                    {
                        if (ratecountbydate.ContainsKey(ratekv.Key))
                        {
                            ratecountbydate[ratekv.Key].Add(ratekv.Value);
                            datecount += ratekv.Value;
                        }
                        else
                        {
                            var templist = new List<int>();
                            templist.Add(ratekv.Value);
                            ratecountbydate.Add(ratekv.Key, templist);
                            datecount += ratekv.Value;
                        }
                    }

                    if (datecount > maxrmacount) { maxrmacount = datecount; }
                }
                else
                {
                    foreach (var rate in vratelist)
                    {
                        if (ratecountbydate.ContainsKey(rate))
                        {
                            ratecountbydate[rate].Add(0);
                        }
                        else
                        {
                            var templist = new List<int>();
                            templist.Add(0);
                            ratecountbydate.Add(rate, templist);
                        }
                    }
                }
            }//end foreach

            maxrmacount = maxrmacount + 1;


            //construct milestone data array
            var idx = 0;
            var milestoneinfoarray = new List<object>();
            foreach (var date in combinxlist)
            {
                if (milestonedict.ContainsKey(date))
                {
                    milestoneinfoarray.Add(new {
                        x = idx,
                        y = maxrmacount,
                        name = milestonedict[date]
                    });
                }
                idx = idx + 1;
            }
            //construct milestone struct
            var mstmarker = new
            {
                radius = 6,
                symbol = "circle",
                fillColor = "#fff",
                lineColor = "#ffc000",
                lineWidth = 2
            };
            var msttooltip = new
            {
                useHTML = true,
                headerFormat = "<span style='font-size: 10px'>{point.x}</span><br/>",
                pointFormat = "{point.name}"
            };

            var mstseria = new
            {
                type = "scatter",
                name = "Milestone",
                marker = mstmarker,
                tooltip = msttooltip,
                data = milestoneinfoarray
            };


            //construct vcsel rate column data
            var serialarray = new List<object>();
            foreach (var rate in vratelist)
            {
                serialarray.Add(new {
                    name = rate,
                    color = vratecolordict[rate],
                    data = ratecountbydate[rate]
                });
            }
            serialarray.Add(mstseria);

            var allx = new { data = combinxlist };
            var ally = new { title = "Amount" };
            var time = new {
                name = "Milestone",
                color = "#F0AD4E",
                data = maxrmacount
            };

            var vcsel_milestone = new {
                id = id,
                title = title,
                coltype = "normal",
                xAxis = allx,
                yAxis = ally,
                time = time,
                data = serialarray
            };

            var ret = new JsonResult();
            ret.Data = new
            {
                success = true,
                vcsel_milestone = vcsel_milestone
            };
            return ret;
        }

        public static int DaysToMonth(double days)
        {
            var ret = 3;
            if (days <= 90)
                ret = 3;
            else if (days > 90 && days <= 180)
                ret = 6;
            else if (days > 180 && days <= 360)
                ret = 12;
            else if (days > 360 && days <= 540)
                ret = 18;
            else if (days > 540 && days <= 720)
                ret = 24;
            else if (days > 720 && days <= 900)
                ret = 30;
            else if (days > 900 && days <= 1080)
                ret = 36;
            else if (days > 1080)
                ret = 48;

            return ret;
        }

        public JsonResult VcselVSTimeData()
        {
            //<month,<type,count>>
            var rmavsshipdate = new Dictionary<string, Dictionary<string, int>>();
            //<type,sumlist>
            var typesumlist = new Dictionary<string, List<double>>();
            //<type,countlist>
            var typecntlist = new Dictionary<string, List<int>>();

            var alldata = VcselRMAData.RetrievAllDataASC();
            var typedict = new Dictionary<string, bool>();
            var monthxlist = (new string[] {"3","6","12","18","24","30","36","48" }).ToList();

            foreach (var item in alldata)
            {
                if (!typedict.ContainsKey(item.VcselType))
                {
                    typedict.Add(item.VcselType, true);
                }

                try
                {
                    var rdate = DateTime.Parse(item.RMAOpenDate);
                    var sdate = DateTime.Parse(item.ShipDate);
                    var vsmonth = DaysToMonth((rdate - sdate).TotalDays).ToString();
                    if (rmavsshipdate.ContainsKey(vsmonth))
                    {
                        var temptypedict = rmavsshipdate[vsmonth];
                        temptypedict["ALL_TYPE"] = temptypedict["ALL_TYPE"] + 1;
                        if (temptypedict.ContainsKey(item.VcselType))
                        {
                            temptypedict[item.VcselType] = temptypedict[item.VcselType] + 1;
                        }
                        else
                        {
                            temptypedict.Add(item.VcselType,1);
                        }
                    }
                    else
                    {
                        var temptypedict = new Dictionary<string,int>();
                        temptypedict.Add("ALL_TYPE",1);
                        temptypedict.Add(item.VcselType, 1);
                        rmavsshipdate.Add(vsmonth, temptypedict);
                    }
                }
                catch (Exception ex) { }
            }//end foreach

            typedict.Add("ALL_TYPE", true);

            //fill the whole struct
            foreach (var m in monthxlist)
            {
                if (rmavsshipdate.ContainsKey(m))
                {
                    var rmaship = rmavsshipdate[m];
                    foreach (var t in typedict)
                    {
                        if (!rmaship.ContainsKey(t.Key))
                        {
                            rmaship.Add(t.Key, 0);
                        }
                    }
                }
                else
                {
                    var temptypedict = new Dictionary<string, int>();
                    foreach (var t in typedict)
                    {
                        temptypedict.Add(t.Key, 0);
                    }
                    rmavsshipdate.Add(m, temptypedict);
                }
            }


            foreach (var m in monthxlist)
            {
                //has data of this month
                var temptypedict = rmavsshipdate[m];
                foreach (var t in typedict)
                {
                    if (typesumlist.ContainsKey(t.Key))
                    {
                        var tempsumlist = typesumlist[t.Key];
                        if (tempsumlist.Count == 0)
                        {
                            tempsumlist.Add(temptypedict[t.Key]);
                        }
                        else
                        {
                            var lastval = tempsumlist[tempsumlist.Count - 1]+ temptypedict[t.Key];
                            tempsumlist.Add(lastval);
                        }
                    }
                    else
                    {
                        var tempsumlist = new List<double>();
                        tempsumlist.Add(temptypedict[t.Key]);
                        typesumlist.Add(t.Key, tempsumlist);
                    }

                    if (!typecntlist.ContainsKey(t.Key))
                    {
                        var tempcntlist = new List<int>();
                        tempcntlist.Add(temptypedict[t.Key]);
                        typecntlist.Add(t.Key, tempcntlist);
                    }
                    else
                    {
                        typecntlist[t.Key].Add(temptypedict[t.Key]);
                    }
                }//end foreach
            }//end foreach
            
            var rmashipchartdata = new List<object>();
            foreach (var kv in typecntlist)
            {
                rmashipchartdata.Add(new {
                    name = kv.Key+" Failure",
                    data = kv.Value
                });
            }

            var rmaaccudata = new List<object>();
            foreach (var kv in typesumlist)
            {
                var last = kv.Value.Last();
                if (last != 0.0)
                {
                    var templist = new List<double>();
                    foreach (var c in kv.Value)
                    {
                        templist.Add(Math.Round(c / last * 100.0, 2));
                    }
                    rmaaccudata.Add(new
                    {
                        name = kv.Key + " accu",
                        data = templist
                    });
                }
            }

            var sxaxisdata = new List<string>();
            for (var midx=0;midx < monthxlist.Count;midx++)
            {
                if (midx == 0)
                    sxaxisdata.Add("in " + monthxlist[midx] + " M");
                else
                    sxaxisdata.Add("From " + monthxlist[midx - 1] + " To " + monthxlist[midx] + " M");
            }

            var cxaxisdata = new List<string>();
            for (var midx = 0; midx < monthxlist.Count; midx++)
            {
                cxaxisdata.Add("in " + monthxlist[midx] + " M");
            }

            var ret = new JsonResult();
            ret.Data = new {
                success = true,
                shipdatedata = new {
                    id = "rmavshipdate",
                    title = "VCSEL RMA Failure vs Ship Date",
                    xaxis = sxaxisdata,
                    data = rmashipchartdata
                },
                accumulatedata = new {
                    id = "rmaaccumulatedata",
                    title = "VCSEL RMA Accumulate Failure Rate vs Time",
                    xaxis = cxaxisdata,
                    data = rmaaccudata
                }
            };
            return ret;
        }

        public ActionResult MonthlyVcsel(string defaulttype)
        {
            var vtype = "";
            if (!string.IsNullOrEmpty(defaulttype)) { vtype = defaulttype; }

            var vcseltypelist = VcselBGDVM.VcselTypeList();
            ViewBag.vcseltypeselectlist = CreateSelectList(vcseltypelist, vtype);
            return View();
        }

        public ActionResult MonthlyHTOL(string defaulttype)
        {
            var vtype = "";
            if (!string.IsNullOrEmpty(defaulttype)) { vtype = defaulttype; }

            var vcseltypelist = VcselBGDVM.VcselTypeList("HTOLWaferTestSum");
            ViewBag.vcseltypeselectlist = CreateSelectList(vcseltypelist, vtype);
            return View();
        }

        public JsonResult MonthlyVcselYield()
        {
            var ssdate = Request.Form["sdate"];
            var sedate = Request.Form["edate"];

            var vtype = Request.Form["wf_type"];
            var startdate = DateTime.Now;
            var enddate = DateTime.Now;


            if (!string.IsNullOrEmpty(ssdate) && !string.IsNullOrEmpty(sedate))
            {
                var sdate = DateTime.Parse(Request.Form["sdate"]);
                var edate = DateTime.Parse(Request.Form["edate"]);
                if (sdate < edate)
                {
                    startdate = DateTime.Parse(sdate.ToString("yyyy-MM") + "-01 00:00:00");
                    enddate = DateTime.Parse(edate.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddSeconds(-1);
                }
                else
                {
                    startdate = DateTime.Parse(edate.ToString("yyyy-MM") + "-01 00:00:00");
                    enddate = DateTime.Parse(sdate.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddSeconds(-1);
                }
            }
            else
            {
                startdate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(-6);
                enddate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddSeconds(-1);
            }


            var yieldarray = new List<object>();
            var failurearray = new List<object>();
            var totlearray = new List<object>();

            var retdata = VcselBGDVM.RetrieveVcselMonthlyData(startdate, enddate, vtype);
            var testylist = (List<testtypeyield>)retdata[0];
            var testflist = (List<TestFailureColumn>)retdata[1];
            testylist.Sort(delegate (testtypeyield obj1, testtypeyield obj2)
            {
                if (obj1.TestType.ToUpper().Contains("PRE BURN IN"))
                {
                    if (obj2.TestType.ToUpper().Contains("POST BURN IN")
                    || obj2.TestType.ToUpper().Contains("POST HTOL BURN IN"))
                    {
                        return -1;
                    }
                }
                else if (obj1.TestType.ToUpper().Contains("POST BURN IN"))
                {
                    if (obj2.TestType.ToUpper().Contains("PRE BURN IN"))
                    {
                        return 1;
                    }

                    if (obj2.TestType.ToUpper().Contains("POST HTOL BURN IN"))
                    {
                        return -1;
                    }
                }
                else if (obj1.TestType.ToUpper().Contains("POST HTOL BURN IN"))
                {
                    if (obj2.TestType.ToUpper().Contains("PRE BURN IN")
                    || obj2.TestType.ToUpper().Contains("POST BURN IN"))
                    {
                        return 1;
                    }
                }
                return obj1.TestType.CompareTo(obj2.TestType);
            });
            testflist.Sort(delegate (TestFailureColumn obj1, TestFailureColumn obj2)
            {
                if (obj1.TestType.ToUpper().Contains("PRE BURN IN"))
                {
                    if (obj2.TestType.ToUpper().Contains("POST BURN IN")
                    || obj2.TestType.ToUpper().Contains("POST HTOL BURN IN"))
                    {
                        return -1;
                    }
                }
                else if (obj1.TestType.ToUpper().Contains("POST BURN IN"))
                {
                    if (obj2.TestType.ToUpper().Contains("PRE BURN IN"))
                    {
                        return 1;
                    }

                    if (obj2.TestType.ToUpper().Contains("POST HTOL BURN IN"))
                    {
                        return -1;
                    }
                }
                else if (obj1.TestType.ToUpper().Contains("POST HTOL BURN IN"))
                {
                    if (obj2.TestType.ToUpper().Contains("PRE BURN IN")
                    || obj2.TestType.ToUpper().Contains("POST BURN IN"))
                    {
                        return 1;
                    }
                }
                return obj1.TestType.CompareTo(obj2.TestType);
            });

            var allydata = new Dictionary<string,double>();
            var allcdata = new Dictionary<string, double>();

            foreach (var item in testylist)
            {
                if (item.DYield.Count > 0)
                {
                    var ymin = 95.0;
                    var id = "y_" + item.TestType.Replace(" ", "_") + "_id";

                    var lastdidx = item.DYield.Count - 1;
                    var title = Convert.ToDateTime(item.DYield[0].date).ToString("yyyy/MM") + " ~ " +Convert.ToDateTime(item.DYield[lastdidx].date).ToString("yyyy/MM") + " VCSEL Yield (" +item.TestType+ ")";

                    var xdata = new List<string>();
                    var ydata = new List<double>();
                    var cydata = new List<double>();
                    foreach (var dy in item.DYield)
                    {
                        var x = Convert.ToDateTime(dy.date).ToString("yyyy/MM");
                        xdata.Add(x);
                        ydata.Add(Math.Round(dy.yield, 2));
                        cydata.Add(dy.totle);

                        if (dy.yield < ymin)
                        { ymin = dy.yield; }

                        if (!allydata.ContainsKey(x))
                        {
                            allydata.Add(x, Math.Round(dy.yield, 2));
                        }
                        else
                        {
                            allydata[x] = Math.Round((allydata[x] / 100.0) * (dy.yield / 100.0) * 100.0, 2);
                        }

                        if (!allcdata.ContainsKey(x))
                        {
                            allcdata.Add(x,dy.totle);
                        }
                        else
                        {
                            if (dy.totle > allcdata[x])
                            {
                                allcdata[x] = dy.totle;
                            }
                        }
                    }

                    var xAxis = new { data = xdata };
                    var yAxis = new
                    { title = "Yield (%)",min = ymin,max = 100.0 };
                    var min = new
                    { name = "Min",color= "#F0AD4E",data=94,style= "dash" };
                    var max = new
                    { name = "Max", color = "#C9302C", data = 98, style = "solid" };
                    var data = new
                    { name = "Yield", color = "#ffa500", data = ydata };
                    var combinedata = new { min = min, max = max, data = data };
                    var cdata = new
                    { name = "Test Modules", data = cydata };

                    yieldarray.Add(new
                    {
                        id = id,
                        title = title,
                        xAxis = xAxis,
                        yAxis = yAxis,
                        data = combinedata,
                        cdata = cdata
                    });
                }
            }

            //all yield
            if (allydata.Count > 0)
            {
                var ymin = 95.0;
                var allxdata = allydata.Keys.ToList();
                allxdata.Sort(delegate (string obj1, string obj2)
                {
                    var dobj1 = DateTime.Parse(obj1 + "/01");
                    var dobj2 = DateTime.Parse(obj2 + "/01");
                    return dobj1.CompareTo(dobj2);
                });

                var realy = new List<double>();
                var realc = new List<double>();
                foreach (var x in allxdata)
                {
                    realy.Add(allydata[x]);
                    if (allydata[x] < ymin)
                    { ymin = allydata[x]; }
                    realc.Add(allcdata[x]);
                }

                var id = "y_totleyield_id";
                var title = "VCSEL TOTAL YIELD";
                var xAxis = new { data = allxdata };
                var yAxis = new
                { title = "Yield (%)", min = ymin, max = 100.0 };
                var min = new
                { name = "Min", color = "#F0AD4E", data = 94, style = "dash" };
                var max = new
                { name = "Max", color = "#C9302C", data = 98, style = "solid" };
                var data = new
                { name = "Yield", color = "#ffa500", data = realy };
                var combinedata = new
                { min = min,  max = max, data = data };
                var cdata = new
                { name = "Test Modules", data = realc };

                totlearray.Add(new
                {
                    id = id,
                    title = title,
                    xAxis = xAxis,
                    yAxis = yAxis,
                    data = combinedata,
                    cdata = cdata
                });
            }

            var errorcodedict = CfgUtility.GetBurnInErrorConfig(this);

            foreach(var item in testflist)
            {
                if(item.DateColSeg.Count > 0)
                {
                    var id = "f_" + item.TestType.Replace(" ", "_") + "_id";

                    var lastdidx = item.DateColSeg.Count - 1;
                    var title = Convert.ToDateTime(item.DateColSeg[0].xkey).ToString("yyyy/MM") + " ~ " + Convert.ToDateTime(item.DateColSeg[lastdidx].xkey).ToString("yyyy/MM") + " Faliure Mode (" + item.TestType + ")";
                    var xdata = new List<string>();
                    var ydata = new List<FailureColumnData>();
                    var count = 0;
                    foreach (var f_item in item.DateColSeg)
                    {
                        xdata.Add(Convert.ToDateTime(f_item.xkey).ToString("yyyy/MM"));
                        count = (f_item.DateColSeg.Count > count) ? f_item.DateColSeg.Count : count;
                    }
                    var num = item.DateColSeg.Count;
                    var n = 1;
                    for(var i = count - 1; i >= 0; i--)
                    {
                        var ydata_tmp = new List<FailureColumnSeg>();
                        for(var m = 0; m < num; m++)
                        {
                            if (i < item.DateColSeg[m].DateColSeg.Count)
                            {
                                ydata_tmp.Add(item.DateColSeg[m].DateColSeg[i]);
                                if (!errorcodedict.ContainsKey(item.DateColSeg[m].DateColSeg[i].name))
                                {
                                    errorcodedict.Add(item.DateColSeg[m].DateColSeg[i].name, item.DateColSeg[m].DateColSeg[i].name);
                                }
                            }
                            else
                            {
                                var tempvm = new FailureColumnSeg();
                                tempvm.name = "";
                                tempvm.y = 0;
                                tempvm.color = "#000";
                                ydata_tmp.Add(tempvm);
                            }
                        }
                        ydata.Add(new FailureColumnData() { index = n, data = ydata_tmp });
                        n++;
                    }
                    var xAxis = new { data = xdata };

                    var yAxis = new
                    {
                        title = "(%)",
                        min = 0.0,
                        max = 100.0
                    };

                    failurearray.Add(new
                    {
                        id = id,
                        coltype = "normal",
                        title = title,
                        xAxis = xAxis,
                        yAxis = yAxis,
                        data = ydata
                    });
                }
            }

            var totlefailure = (List<DateWaferFailureColumn>)retdata[3];
            if (totlefailure.Count > 0)
            {
                var id = "f_all_id";

                var lastdidx = totlefailure.Count - 1;
                var title = Convert.ToDateTime(totlefailure[0].xkey).ToString("yyyy/MM") + " ~ " + Convert.ToDateTime(totlefailure[lastdidx].xkey).ToString("yyyy/MM") + " Total Faliure Mode";
                var xdata = new List<string>();
                var ydata = new List<FailureColumnData>();
                var count = 0;
                foreach (var f_item in totlefailure)
                {
                    xdata.Add(Convert.ToDateTime(f_item.xkey).ToString("yyyy/MM"));
                    count = (f_item.DateColSeg.Count > count) ? f_item.DateColSeg.Count : count;
                }
                var num = totlefailure.Count;
                var n = 1;
                for (var i = count - 1; i >= 0; i--)
                {
                    var ydata_tmp = new List<FailureColumnSeg>();
                    for (var m = 0; m < num; m++)
                    {
                        if (i < totlefailure[m].DateColSeg.Count)
                        {
                            ydata_tmp.Add(totlefailure[m].DateColSeg[i]);
                            if (!errorcodedict.ContainsKey(totlefailure[m].DateColSeg[i].name))
                            {
                                errorcodedict.Add(totlefailure[m].DateColSeg[i].name, totlefailure[m].DateColSeg[i].name);
                            }
                        }
                        else
                        {
                            var tempvm = new FailureColumnSeg();
                            tempvm.name = "";
                            tempvm.y = 0;
                            tempvm.color = "#000";
                            ydata_tmp.Add(tempvm);
                        }
                    }
                    ydata.Add(new FailureColumnData() { index = n, data = ydata_tmp });
                    n++;
                }
                var xAxis = new { data = xdata };

                var yAxis = new
                {
                    title = "(%)",
                    min = 0.0,
                    max = 100.0
                };

                totlearray.Add(new
                {
                    id = id,
                    coltype = "normal",
                    title = title,
                    xAxis = xAxis,
                    yAxis = yAxis,
                    data = ydata
                });
            }

            var ret = new JsonResult();
            ret.Data = new { success = true,
                yieldarray = yieldarray,
                failurearray = failurearray,
                colors = retdata[2],
                totlearray = totlearray,
                errorcodedict = errorcodedict
            };
            return ret;
        }

        public JsonResult MonthlyHTOLYield()
        {
            var ssdate = Request.Form["sdate"];
            var sedate = Request.Form["edate"];

            var vtype = Request.Form["wf_type"];
            var startdate = DateTime.Now;
            var enddate = DateTime.Now;


            if (!string.IsNullOrEmpty(ssdate) && !string.IsNullOrEmpty(sedate))
            {
                var sdate = DateTime.Parse(Request.Form["sdate"]);
                var edate = DateTime.Parse(Request.Form["edate"]);
                if (sdate < edate)
                {
                    startdate = DateTime.Parse(sdate.ToString("yyyy-MM") + "-01 00:00:00");
                    enddate = DateTime.Parse(edate.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddSeconds(-1);
                }
                else
                {
                    startdate = DateTime.Parse(edate.ToString("yyyy-MM") + "-01 00:00:00");
                    enddate = DateTime.Parse(sdate.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddSeconds(-1);
                }
            }
            else
            {
                startdate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(-6);
                enddate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddSeconds(-1);
            }
            
            var testnamedict = new Dictionary<string,bool>();
            testnamedict.Add(HTOLTESTTYPE.POST_BURN_IN,true);
            testnamedict.Add(HTOLTESTTYPE.POST_HTOL_BURN_IN,true);

            var yieldarray = new List<object>();
            var failurearray = new List<object>();
 
            var retdata = VcselBGDVM.RetrieveHTOLMonthlyData(startdate, enddate, vtype,testnamedict);
            var testylist = (List<testtypeyield>)retdata[0];
            var testflist = (List<TestFailureColumn>)retdata[1];
            testylist.Sort(delegate (testtypeyield obj1, testtypeyield obj2)
            {
                if (obj1.TestType.ToUpper().Contains("PRE BURN IN"))
                {
                    if (obj2.TestType.ToUpper().Contains("POST BURN IN")
                    || obj2.TestType.ToUpper().Contains("POST HTOL BURN IN"))
                    {
                        return -1;
                    }
                }
                else if (obj1.TestType.ToUpper().Contains("POST BURN IN"))
                {
                    if (obj2.TestType.ToUpper().Contains("PRE BURN IN"))
                    {
                        return 1;
                    }

                    if (obj2.TestType.ToUpper().Contains("POST HTOL BURN IN"))
                    {
                        return -1;
                    }
                }
                else if (obj1.TestType.ToUpper().Contains("POST HTOL BURN IN"))
                {
                    if (obj2.TestType.ToUpper().Contains("PRE BURN IN")
                    || obj2.TestType.ToUpper().Contains("POST BURN IN"))
                    {
                        return 1;
                    }
                }
                return obj1.TestType.CompareTo(obj2.TestType);
            });
            testflist.Sort(delegate (TestFailureColumn obj1, TestFailureColumn obj2)
            {
                if (obj1.TestType.ToUpper().Contains("PRE BURN IN"))
                {
                    if (obj2.TestType.ToUpper().Contains("POST BURN IN")
                    || obj2.TestType.ToUpper().Contains("POST HTOL BURN IN"))
                    {
                        return -1;
                    }
                }
                else if (obj1.TestType.ToUpper().Contains("POST BURN IN"))
                {
                    if (obj2.TestType.ToUpper().Contains("PRE BURN IN"))
                    {
                        return 1;
                    }

                    if (obj2.TestType.ToUpper().Contains("POST HTOL BURN IN"))
                    {
                        return -1;
                    }
                }
                else if (obj1.TestType.ToUpper().Contains("POST HTOL BURN IN"))
                {
                    if (obj2.TestType.ToUpper().Contains("PRE BURN IN")
                    || obj2.TestType.ToUpper().Contains("POST BURN IN"))
                    {
                        return 1;
                    }
                }
                return obj1.TestType.CompareTo(obj2.TestType);
            });

            foreach (var item in testylist)
            {
                if (item.DYield.Count > 0)
                {
                    var ymin = 95.0;
                    var id = "y_" + item.TestType.Replace(" ", "_") + "_id";

                    var lastdidx = item.DYield.Count - 1;
                    var title = Convert.ToDateTime(item.DYield[0].date).ToString("yyyy/MM") + " ~ " + Convert.ToDateTime(item.DYield[lastdidx].date).ToString("yyyy/MM") + " Test Yield (" + item.TestType + ")";

                    var xdata = new List<string>();
                    var ydata = new List<double>();
                    var cydata = new List<double>();
                    foreach (var dy in item.DYield)
                    {
                        var x = Convert.ToDateTime(dy.date).ToString("yyyy/MM");
                        xdata.Add(x);
                        ydata.Add(Math.Round(dy.yield, 2));
                        cydata.Add(dy.totle);
                        if (dy.yield < ymin)
                        { ymin = dy.yield; }
                    }

                    var xAxis = new { data = xdata };
                    var yAxis = new
                    { title = "Yield (%)", min = ymin, max = 100.0 };
                    var min = new
                    { name = "Min", color = "#F0AD4E", data = 94, style = "dash" };
                    var max = new
                    { name = "Max", color = "#C9302C", data = 98, style = "solid" };
                    var data = new
                    { name = "Yield", color = "#ffa500", data = ydata };
                    var combinedata = new { min = min, max = max, data = data };
                    var cdata = new
                    { name = "Test Modules", data = cydata };

                    yieldarray.Add(new
                    {
                        id = id,
                        title = title,
                        xAxis = xAxis,
                        yAxis = yAxis,
                        data = combinedata,
                        cdata = cdata
                    });
                }
            }


            var errorcodedict = CfgUtility.GetBurnInErrorConfig(this);

            foreach (var item in testflist)
            {
                if (item.DateColSeg.Count > 0)
                {
                    var id = "f_" + item.TestType.Replace(" ", "_") + "_id";

                    var lastdidx = item.DateColSeg.Count - 1;
                    var title = Convert.ToDateTime(item.DateColSeg[0].xkey).ToString("yyyy/MM") + " ~ " + Convert.ToDateTime(item.DateColSeg[lastdidx].xkey).ToString("yyyy/MM") + " Faliure Mode (" + item.TestType + ")";
                    var xdata = new List<string>();
                    var ydata = new List<FailureColumnData>();
                    var count = 0;
                    foreach (var f_item in item.DateColSeg)
                    {
                        xdata.Add(Convert.ToDateTime(f_item.xkey).ToString("yyyy/MM"));
                        count = (f_item.DateColSeg.Count > count) ? f_item.DateColSeg.Count : count;
                    }
                    var num = item.DateColSeg.Count;
                    var n = 1;
                    for (var i = count - 1; i >= 0; i--)
                    {
                        var ydata_tmp = new List<FailureColumnSeg>();
                        for (var m = 0; m < num; m++)
                        {
                            if (i < item.DateColSeg[m].DateColSeg.Count)
                            {
                                ydata_tmp.Add(item.DateColSeg[m].DateColSeg[i]);
                                if (!errorcodedict.ContainsKey(item.DateColSeg[m].DateColSeg[i].name))
                                {
                                    errorcodedict.Add(item.DateColSeg[m].DateColSeg[i].name, item.DateColSeg[m].DateColSeg[i].name);
                                }
                            }
                            else
                            {
                                var tempvm = new FailureColumnSeg();
                                tempvm.name = "";
                                tempvm.y = 0;
                                tempvm.color = "#000";
                                ydata_tmp.Add(tempvm);
                            }
                        }
                        ydata.Add(new FailureColumnData() { index = n, data = ydata_tmp });
                        n++;
                    }
                    var xAxis = new { data = xdata };

                    var yAxis = new
                    {
                        title = "(%)",
                        min = 0.0,
                        max = 100.0
                    };

                    failurearray.Add(new
                    {
                        id = id,
                        coltype = "normal",
                        title = title,
                        xAxis = xAxis,
                        yAxis = yAxis,
                        data = ydata
                    });
                }
            }


            var ret = new JsonResult();
            ret.Data = new
            {
                success = true,
                yieldarray = yieldarray,
                failurearray = failurearray,
                colors = retdata[2],
                errorcodedict = errorcodedict
            };
            return ret;
        }

        public ActionResult WaferDistribution(string defaultwafer, string defaultdate, string defaulttype)
        {
            
            var ckdict = CookieUtility.UnpackCookie(this);
            ViewBag.Updater = "";
            if (ckdict.ContainsKey("logonuser"))
            {
                var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
                var syscfg = CfgUtility.GetSysConfig(this);
                var biteam = syscfg["BITEAM"].ToUpper();
                if (biteam.Contains(updater))
                { ViewBag.Updater = updater; }
            }

            var vcseltypelist = VcselBGDVM.VcselTypeList();

            var nvcseltypelist = new List<string>();
            nvcseltypelist.Add(" ");
            nvcseltypelist.AddRange(vcseltypelist);

            var deftype = "";
            if (!string.IsNullOrEmpty(defaulttype))
            { deftype = defaulttype; }
            ViewBag.vcseltypeselectlist = CreateSelectList(nvcseltypelist, deftype);

            var mathlist = new List<string>();
            mathlist.Add("Only Pass Data");
            mathlist.Add("Math Rectification");
            mathlist.Add("No Rectification");
            
            ViewBag.mathrectlist = CreateSelectList(mathlist, "");

            ViewBag.DefaultWafer = "";
            if (!string.IsNullOrEmpty(defaultwafer))
            { ViewBag.DefaultWafer = defaultwafer; }

            ViewBag.DefaultDate = "";
            if (!string.IsNullOrEmpty(defaultdate))
            {
                ViewBag.DefaultDate = defaultdate;
            }

            return View();
        }

        public ActionResult HTOLDistribution(string defaultwafer,string defaultdate,string defaulttype)
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            ViewBag.Updater = "";
            if (ckdict.ContainsKey("logonuser"))
            {
                var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
                var syscfg = CfgUtility.GetSysConfig(this);
                var biteam = syscfg["BITEAM"].ToUpper();
                if (biteam.Contains(updater))
                { ViewBag.Updater = updater; }
            }

            var vcseltypelist = VcselBGDVM.VcselTypeList("HTOLWaferTestSum");

            var nvcseltypelist = new List<string>();
            nvcseltypelist.Add(" ");
            nvcseltypelist.AddRange(vcseltypelist);

            var deftype = "";
            if (!string.IsNullOrEmpty(defaulttype))
            { deftype = defaulttype; }

            ViewBag.vcseltypeselectlist = CreateSelectList(nvcseltypelist, deftype);

            var mathlist = new List<string>();
            mathlist.Add("Only Pass Data");
            mathlist.Add("With Fail Data");

            ViewBag.mathrectlist = CreateSelectList(mathlist, "");

            ViewBag.DefaultWafer = "";
            if (!string.IsNullOrEmpty(defaultwafer))
            { ViewBag.DefaultWafer = defaultwafer; }

            ViewBag.DefaultDate = "";
            if (!string.IsNullOrEmpty(defaultdate))
            {
                ViewBag.DefaultDate = defaultdate;
            }
            return View();
        }

        public JsonResult WaferNumAutoCompelete()
        {
            var vlist = VcselBGDVM.WaferNOList();
            var ret = new JsonResult();
            ret.Data = new { data = vlist };
            return ret;
        }

        public JsonResult HTOLWaferNumAutoCompelete()
        {
            var vlist = VcselBGDVM.WaferNOList("HTOLVcselTimeRange");
            var ret = new JsonResult();
            ret.Data = new { data = vlist };
            return ret;
        }

        //public JsonResult WaferDistributionData() {
        //    var wf_no = Request.Form["wf_no"];
        //    var vtype = Request.Form["wf_type"].Trim();
        //    var math_rect = Request.Form["math_rect"];

        //    var wflist = new List<string>();
        //    if (!string.IsNullOrEmpty(wf_no))
        //    {
        //        wflist.AddRange(wf_no.Split(new string[] { ",",";"," " }, StringSplitOptions.RemoveEmptyEntries));
        //    }
        //    else
        //    {
        //        var sdate = DateTime.Parse(Request.Form["sdate"]);
        //        var edate = DateTime.Parse(Request.Form["edate"]);
        //        var startdate = DateTime.Now;
        //        var enddate = DateTime.Now;
        //        if (sdate < edate)
        //        {
        //            startdate = DateTime.Parse(sdate.ToString("yyyy-MM") + "-01 00:00:00");
        //            enddate = DateTime.Parse(edate.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddDays(-1);
        //        }
        //        else
        //        {
        //            startdate = DateTime.Parse(edate.ToString("yyyy-MM") + "-01 00:00:00");
        //            enddate = DateTime.Parse(sdate.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddDays(-1);
        //        }

        //        wflist.AddRange(VcselTimeRange.RetrieveWafer(startdate,enddate));
        //    }

        //    if (wflist.Count > 0)
        //    {

        //        var poldlowlimit = 0.0;
        //        var poldhighlimit = 0.0;
        //        var poulowlimit = 0.0;
        //        var pouhighlimit = 0.0;
        //        var vlowlimit = 0.0;
        //        var vhighlimit = 0.0;
        //        var ulowlimit = 0.0;
        //        var uhighlimit = 0.0;

        //        if (math_rect.ToUpper().Contains("MATH")){
        //            var glbcfg = CfgUtility.GetSysConfig(this);
        //            poldlowlimit = Convert.ToDouble(glbcfg["Plot_Delta_PO_LD_LIMIT"].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[0]);
        //            poldhighlimit = Convert.ToDouble(glbcfg["Plot_Delta_PO_LD_LIMIT"].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1]);
        //            poulowlimit = Convert.ToDouble(glbcfg["Plot_Delta_PO_Uniformity_LIMIT"].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[0]);
        //            pouhighlimit = Convert.ToDouble(glbcfg["Plot_Delta_PO_Uniformity_LIMIT"].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1]);
        //            vlowlimit = Convert.ToDouble(glbcfg["Plot_Variation_POLD_Delta_LIMIT"].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[0]);
        //            vhighlimit = Convert.ToDouble(glbcfg["Plot_Variation_POLD_Delta_LIMIT"].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1]);
        //            ulowlimit = Convert.ToDouble(glbcfg["Plot_Uniformity_POLD_Delta_LIMIT"].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[0]);
        //            uhighlimit = Convert.ToDouble(glbcfg["Plot_Uniformity_POLD_Delta_LIMIT"].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1]);
        //        }
        //        else {
        //            poldlowlimit = -9999.0;
        //            poldhighlimit = 9999.0;
        //            poulowlimit = -9999.0;
        //            pouhighlimit = 9999.0;
        //            vlowlimit = -9999.0;
        //            vhighlimit = 9999.0;
        //            ulowlimit = -9999.0;
        //            uhighlimit = 9999.0;

        //        }

        //        bool withfaileddata = true;
        //        if (math_rect.ToUpper().Contains("ONLY PASS DATA"))
        //        {
        //            withfaileddata = false;
        //        }

        //        var retdata = VcselBGDVM.RetrieveWaferData(wflist, withfaileddata, vtype,true);

        //        //fieldname,wafer,boxlist
        //        var fieldboxlist = (Dictionary<string, Dictionary<string, List<string>>>)retdata[0];
        //        var testflist = (List<TestFailureColumn>)retdata[1];

        //        //var fieldrawlist = (Dictionary<string, Dictionary<string, List<string>>>)retdata[3];

        //        var yieldarray = new List<object>();
        //        var boxarray = new List<object>();
        //        var failurearray = new List<object>();

        //        var xwaferlist = new List<string>();
        //        var xwaferdict = new Dictionary<string, bool>();

        //        foreach (var item in testflist)
        //        {
        //            if (item.DateColSeg.Count > 0)
        //            {
        //                var id = "f_yield_" + item.TestType.Replace(" ", "_") + "_id";
        //                var title = item.TestType + " Wafer Yield";

        //                var xdata = new List<string>();
        //                var ydata = new List<double>();
        //                var cydata = new List<double>();

        //                var count = 0;
        //                foreach (var f_item in item.DateColSeg)
        //                {
        //                    xdata.Add(f_item.xkey);
        //                    if (!xwaferdict.ContainsKey(f_item.xkey))
        //                    {
        //                        xwaferlist.Add(f_item.xkey);
        //                        xwaferdict.Add(f_item.xkey, true);
        //                    }

        //                    count = (f_item.DateColSeg.Count > count) ? f_item.DateColSeg.Count : count;
        //                }

        //                foreach (var wfitem in item.DateColSeg)
        //                {
        //                    var failpercent = 0.0;
        //                    foreach (var fitem in wfitem.DateColSeg)
        //                    {
        //                        failpercent = failpercent + fitem.y;
        //                    }
        //                    ydata.Add(100.0 - failpercent);
        //                    cydata.Add(wfitem.total);
        //                }


        //                var xAxis = new { data = xdata };

        //                var yAxis = new
        //                {
        //                    title = "Yield (%)",
        //                    min = 85.0,
        //                    max = 100.0
        //                };

        //                var min = new
        //                {
        //                    name = "Min",
        //                    color = "#F0AD4E",
        //                    data = 94,
        //                    style = "dash"
        //                };

        //                var max = new
        //                {
        //                    name = "Max",
        //                    color = "#C9302C",
        //                    data = 98,
        //                    style = "solid"
        //                };

        //                var data = new
        //                {
        //                    name = "Yield",
        //                    color = "#ffa500",
        //                    data = ydata
        //                };

        //                var combinedata = new
        //                {
        //                    min = min,
        //                    max = max,
        //                    data = data
        //                };

        //                var cdata = new
        //                {
        //                    name = "Test Modules",
        //                    data = cydata
        //                };

        //                yieldarray.Add(new
        //                {
        //                    id = id,
        //                    title = title,
        //                    xAxis = xAxis,
        //                    yAxis = yAxis,
        //                    data = combinedata,
        //                    cdata = cdata
        //                });
        //            }//end if
        //        }//end foreach


        //        foreach (var fieldkv in fieldboxlist)
        //        {
        //            var fieldname = fieldkv.Key;
        //            var xaxisdata = new List<string>();

        //            var ldatadata = new List<List<double>>();
        //            var llinedata = new List<double>();

        //            var rdatadata = new List<List<double>>();
        //            var rlinedata = new List<double>();

        //            //foreach (var wfkv in fieldkv.Value)
        //            foreach(var x in xwaferlist)
        //            {
        //                //var wf = wfkv.Key;
        //                //xaxisdata.Add(wf);
        //                //foreach (var box in wfkv.Value)

        //                if (!fieldkv.Value.ContainsKey(x)) continue;

        //                var wf = x;
        //                xaxisdata.Add(wf);
        //                var boxdict = fieldkv.Value[x];
        //                foreach(var box in boxdict)
        //                {
        //                    if (box.Contains("#V"))
        //                    {
        //                        var cbox = (CBOXData)Newtonsoft.Json.JsonConvert.DeserializeObject(box.Replace("#V",""), (new CBOXData()).GetType());
        //                        var tempdd = new List<double>();
        //                        tempdd.Add((cbox.min < vlowlimit) ? vlowlimit : Math.Round(cbox.min, 4));
        //                        tempdd.Add(Math.Round(cbox.lower, 4));
        //                        tempdd.Add(Math.Round(cbox.mean, 4));
        //                        tempdd.Add(Math.Round(cbox.upper, 4));
        //                        tempdd.Add((cbox.max > vhighlimit) ? vhighlimit : Math.Round(cbox.max, 4));
        //                        ldatadata.Add(tempdd);
        //                        llinedata.Add(Math.Round(cbox.mean, 4));
        //                    }
        //                    else if (box.Contains("#U"))
        //                    {
        //                        var cbox = (CBOXData)Newtonsoft.Json.JsonConvert.DeserializeObject(box.Replace("#U", ""), (new CBOXData()).GetType());
        //                        var tempdd = new List<double>();
        //                        tempdd.Add((cbox.min < ulowlimit) ? ulowlimit : Math.Round(cbox.min, 4));
        //                        tempdd.Add(Math.Round(cbox.lower, 4));
        //                        tempdd.Add(Math.Round(cbox.mean, 4));
        //                        tempdd.Add(Math.Round(cbox.upper, 4));
        //                        tempdd.Add((cbox.max > uhighlimit) ? uhighlimit : Math.Round(cbox.max, 4));
        //                        rdatadata.Add(tempdd);
        //                        rlinedata.Add(Math.Round(cbox.mean, 4));
        //                    }
        //                    else {
        //                        var cbox = (CBOXData)Newtonsoft.Json.JsonConvert.DeserializeObject(box, (new CBOXData()).GetType());
        //                        var tempdd = new List<double>();

        //                        if (fieldname.ToUpper().Contains("Delta_PO_LD".ToUpper()))
        //                        {
        //                            tempdd.Add((cbox.min < poldlowlimit) ? poldlowlimit : Math.Round(cbox.min, 4));
        //                            tempdd.Add(Math.Round(cbox.lower, 4));
        //                            tempdd.Add(Math.Round(cbox.mean, 4));
        //                            tempdd.Add(Math.Round(cbox.upper, 4));
        //                            tempdd.Add((cbox.max > poldhighlimit) ? poldhighlimit : Math.Round(cbox.max, 4));
        //                        }
        //                        else if (fieldname.ToUpper().Contains("Delta_PO_Uniformity".ToUpper()))
        //                        {
        //                            tempdd.Add((cbox.min < poulowlimit) ? poulowlimit : Math.Round(cbox.min, 4));
        //                            tempdd.Add(Math.Round(cbox.lower, 4));
        //                            tempdd.Add(Math.Round(cbox.mean, 4));
        //                            tempdd.Add(Math.Round(cbox.upper, 4));
        //                            tempdd.Add((cbox.max > pouhighlimit) ? pouhighlimit : Math.Round(cbox.max, 4));
        //                        }
        //                        else
        //                        {
        //                            tempdd.Add(Math.Round(cbox.min, 4));
        //                            tempdd.Add(Math.Round(cbox.lower, 4));
        //                            tempdd.Add(Math.Round(cbox.mean, 4));
        //                            tempdd.Add(Math.Round(cbox.upper, 4));
        //                            tempdd.Add(Math.Round(cbox.max, 4));
        //                        }
        //                        ldatadata.Add(tempdd);
        //                        llinedata.Add(Math.Round(cbox.mean, 4));
        //                    }
        //                }//end foreach
        //            }//end forea

        //            var id = fieldname.Replace(" ", "_") + "_id";
        //            var title = fieldname + " Distribution by Wafer";
        //            var xAxis = new
        //            {
        //                title = "Wafer#",
        //                data = xaxisdata
        //            };

        //            if (fieldname.Contains("variation_uniformity_pold"))
        //            {
        //                var lyAxis = new { title = "Variation_POLD_Delta" };
        //                var ryAxis = new { title = "Uniformity_POLD_Delta" };
        //                var ldata = new {
        //                    name = "Variation_POLD_Delta",
        //                    color = "#00b050",
        //                    data = ldatadata
        //                };
        //                var rdata = new
        //                {
        //                    name = "Uniformity_POLD_Delta",
        //                    color = "#0099ff",
        //                    data = rdatadata
        //                };
        //                var lline = new
        //                {
        //                    name = "Mean Line",
        //                    data = llinedata,
        //                    color = "#ffc000",
        //                    lineWidth = 1
        //                };
        //                var rline = new
        //                {
        //                    name = "Mean Line",
        //                    data = rlinedata,
        //                    color = "#C9302C",
        //                    lineWidth = 1
        //                };

        //                var left = new {
        //                    yAxis = lyAxis,
        //                    data = ldata,
        //                    line = lline
        //                };
        //                var right = new
        //                {
        //                    yAxis = ryAxis,
        //                    data = rdata,
        //                    line = rline
        //                };

        //                boxarray.Add(new
        //                {
        //                    id = id,
        //                    title = title,
        //                    xAxis = xAxis,
        //                    left = left,
        //                    right = right
        //                });
        //            }
        //            else
        //            {
        //                var yAxis = new { title = "Value" };
        //                var data = new {
        //                    name = fieldname,
        //                    data = ldatadata
        //                };
        //                var line = new
        //                {
        //                    name = "Mean Line",
        //                    data = llinedata,
        //                    color = "#ffc000",
        //                    lineWidth = 1
        //                };

        //                boxarray.Add(new
        //                {
        //                    id = id,
        //                    title = title,
        //                    xAxis = xAxis,
        //                    yAxis = yAxis,
        //                    data = data,
        //                    line = line
        //                });
        //            }
        //        }//end foreach


        //        foreach (var item in testflist)
        //        {
        //            if (item.DateColSeg.Count > 0)
        //            {
        //                var id = "f_" + item.TestType.Replace(" ", "_") + "_id";

        //                var title = "Faliure Mode (" + item.TestType + ")";

        //                var xdata = new List<string>();
        //                var ydata = new List<FailureColumnData>();
        //                var count = 0;
        //                foreach (var f_item in item.DateColSeg)
        //                {
        //                    xdata.Add(f_item.xkey);
        //                    count = (f_item.DateColSeg.Count > count) ? f_item.DateColSeg.Count : count;
        //                }

        //                var ymax = 3.0;
        //                foreach (var wfitem in item.DateColSeg)
        //                {
        //                    var tempmax = 0.0;
        //                    foreach (var fitem in wfitem.DateColSeg)
        //                    {
        //                        tempmax = tempmax + fitem.y;
        //                    }
        //                    if (tempmax > 3.0) ymax = 5.0;
        //                    if (tempmax > 5.0) ymax = 10.0;
        //                    if (tempmax > 10.0) ymax = 15.0;
        //                    if (tempmax > 15.0) ymax = 20.0;
        //                    if (tempmax > 20.0) ymax = 100.0;
        //                }

        //                var num = item.DateColSeg.Count;
        //                var n = 1;
        //                for (var i = count - 1; i >= 0; i--)
        //                {

        //                    var ydata_tmp = new List<FailureColumnSeg>();
        //                    for (var m = 0; m < num; m++)
        //                    {
        //                        if (i < item.DateColSeg[m].DateColSeg.Count)
        //                        {
        //                            item.DateColSeg[m].DateColSeg[i].y = Math.Round(item.DateColSeg[m].DateColSeg[i].y, 4);
        //                            ydata_tmp.Add(item.DateColSeg[m].DateColSeg[i]);
        //                        }
        //                        else
        //                        {
        //                            var tempvm = new FailureColumnSeg();
        //                            tempvm.name = "";
        //                            tempvm.y = 0;
        //                            tempvm.color = "#000";
        //                            ydata_tmp.Add(tempvm);
        //                        }
        //                    }
        //                    ydata.Add(new FailureColumnData() { index = n, data = ydata_tmp });
        //                    n++;
        //                }


        //                var xAxis = new { data = xdata };
        //                var yAxis = new
        //                {
        //                    title = "(%)",
        //                    min = 0,
        //                    max = ymax
        //                };

        //                failurearray.Add(new
        //                {
        //                    id = id,
        //                    coltype = "normal",
        //                    title = title,
        //                    xAxis = xAxis,
        //                    yAxis = yAxis,
        //                    data = ydata
        //                });
        //            }
        //        }

        //        var ret = new JsonResult();
        //        ret.Data = new
        //        {
        //            success = true,
        //            yieldarray = yieldarray,
        //            boxarray = boxarray,
        //            failurearray = failurearray,
        //            colors = retdata[2],
        //        };
        //        return ret;
        //    }
        //    else
        //    {
        //        var ret = new JsonResult();
        //        ret.Data = new
        //        {
        //            success = false,
        //            msg = "No wafer match query condition."
        //        };
        //        return ret;
        //    }
        //}

        public JsonResult WaferDistributionRawData()
        {
            var wf_no = Request.Form["wf_no"];
            var vtype = Request.Form["wf_type"].Trim();
            var math_rect = Request.Form["math_rect"];
            var nooutlier = true;
            var boutlier = Request.Form["outlier"];
            if (string.Compare(boutlier, "YES", true) == 0)
            { nooutlier = false; }

            var wflist = new List<string>();
            if (!string.IsNullOrEmpty(wf_no))
            {
                wflist.AddRange(wf_no.Split(new string[] { ",", ";", " " }, StringSplitOptions.RemoveEmptyEntries));
            }
            else
            {
                var sdate = DateTime.Parse(Request.Form["sdate"]);
                var edate = DateTime.Parse(Request.Form["edate"]);
                var startdate = DateTime.Now;
                var enddate = DateTime.Now;
                if (sdate < edate)
                {
                    startdate = DateTime.Parse(sdate.ToString("yyyy-MM") + "-01 00:00:00");
                    enddate = DateTime.Parse(edate.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddDays(-1);
                }
                else
                {
                    startdate = DateTime.Parse(edate.ToString("yyyy-MM") + "-01 00:00:00");
                    enddate = DateTime.Parse(sdate.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddDays(-1);
                }

                wflist.AddRange(VcselTimeRange.RetrieveWafer(startdate, enddate));
            }

            if (wflist.Count > 0)
            {

                var poldlowlimit = 0.0;
                var poldhighlimit = 0.0;
                var poulowlimit = 0.0;
                var pouhighlimit = 0.0;
                var vlowlimit = 0.0;
                var vhighlimit = 0.0;
                var ulowlimit = 0.0;
                var uhighlimit = 0.0;

                if (math_rect.ToUpper().Contains("MATH"))
                {
                    var glbcfg = CfgUtility.GetSysConfig(this);
                    poldlowlimit = Convert.ToDouble(glbcfg["Plot_Delta_PO_LD_LIMIT"].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[0]);
                    poldhighlimit = Convert.ToDouble(glbcfg["Plot_Delta_PO_LD_LIMIT"].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1]);
                    poulowlimit = Convert.ToDouble(glbcfg["Plot_Delta_PO_Uniformity_LIMIT"].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[0]);
                    pouhighlimit = Convert.ToDouble(glbcfg["Plot_Delta_PO_Uniformity_LIMIT"].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1]);
                    vlowlimit = Convert.ToDouble(glbcfg["Plot_Variation_POLD_Delta_LIMIT"].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[0]);
                    vhighlimit = Convert.ToDouble(glbcfg["Plot_Variation_POLD_Delta_LIMIT"].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1]);
                    ulowlimit = Convert.ToDouble(glbcfg["Plot_Uniformity_POLD_Delta_LIMIT"].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[0]);
                    uhighlimit = Convert.ToDouble(glbcfg["Plot_Uniformity_POLD_Delta_LIMIT"].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1]);
                }
                else
                {
                    poldlowlimit = -9999.0;
                    poldhighlimit = 9999.0;
                    poulowlimit = -9999.0;
                    pouhighlimit = 9999.0;
                    vlowlimit = -9999.0;
                    vhighlimit = 9999.0;
                    ulowlimit = -9999.0;
                    uhighlimit = 9999.0;

                }

                bool withfaileddata = true;
                if (math_rect.ToUpper().Contains("ONLY PASS DATA"))
                {
                    withfaileddata = false;
                }

                var retdata = VcselBGDVM.RetrieveBURNINWaferData(wflist, withfaileddata, vtype);

                //fieldname,wafer,boxlist
                var fieldboxlist = (Dictionary<string, Dictionary<string, List<string>>>)retdata[0];
                var fieldorderlist = fieldboxlist.Keys.ToList();
                fieldorderlist.Sort(delegate (string obj1, string obj2)
                {
                    if (obj1.ToUpper().Contains("PRE BURN IN"))
                    {
                        if (obj2.ToUpper().Contains("POST BURN IN")
                        || obj2.ToUpper().Contains("POST HTOL BURN IN"))
                        {
                            return -1;
                        }
                    }
                    else if (obj1.ToUpper().Contains("POST BURN IN"))
                    {
                        if (obj2.ToUpper().Contains("PRE BURN IN"))
                        {
                            return 1;
                        }

                        if (obj2.ToUpper().Contains("POST HTOL BURN IN"))
                        {
                            return -1;
                        }
                    }
                    else if (obj1.ToUpper().Contains("POST HTOL BURN IN"))
                    {
                        if (obj2.ToUpper().Contains("PRE BURN IN")
                        || obj2.ToUpper().Contains("POST BURN IN"))
                        {
                            return 1;
                        }
                    }

                    return obj1.CompareTo(obj2);
                });

                var testflist = (List<TestFailureColumn>)retdata[1];
                testflist.Sort(delegate (TestFailureColumn obj1, TestFailureColumn obj2)
                {
                    if (obj1.TestType.ToUpper().Contains("PRE BURN IN"))
                    {
                        if (obj2.TestType.ToUpper().Contains("POST BURN IN")
                        || obj2.TestType.ToUpper().Contains("POST HTOL BURN IN"))
                        {
                            return -1;
                        }
                    }
                    else if (obj1.TestType.ToUpper().Contains("POST BURN IN"))
                    {
                        if (obj2.TestType.ToUpper().Contains("PRE BURN IN"))
                        {
                            return 1;
                        }

                        if (obj2.TestType.ToUpper().Contains("POST HTOL BURN IN"))
                        {
                            return -1;
                        }
                    }
                    else if (obj1.TestType.ToUpper().Contains("POST HTOL BURN IN"))
                    {
                        if (obj2.TestType.ToUpper().Contains("PRE BURN IN")
                        || obj2.TestType.ToUpper().Contains("POST BURN IN"))
                        {
                            return 1;
                        }
                    }
                    return obj1.TestType.CompareTo(obj2.TestType);
                });
                //var fieldrawlist = (Dictionary<string, Dictionary<string, List<string>>>)retdata[3];

                var yieldarray = new List<object>();
                var boxarray = new List<object>();
                var failurearray = new List<object>();

                var xwaferlist = new List<string>();
                var xwaferdict = new Dictionary<string, bool>();

                foreach (var item in testflist)
                {
                    if (item.DateColSeg.Count > 0)
                    {
                        var id = "f_yield_" + item.TestType.Replace(" ", "_") + "_id";
                        var title = item.TestType + " Wafer Yield";

                        var xdata = new List<string>();
                        var ydata = new List<double>();
                        var cydata = new List<double>();
                        var ymin = 95.0;

                        var count = 0;
                        foreach (var f_item in item.DateColSeg)
                        {
                            xdata.Add(f_item.xkey);
                            if (!xwaferdict.ContainsKey(f_item.xkey))
                            {
                                xwaferlist.Add(f_item.xkey);
                                xwaferdict.Add(f_item.xkey, true);
                            }

                            count = (f_item.DateColSeg.Count > count) ? f_item.DateColSeg.Count : count;
                        }

                        foreach (var wfitem in item.DateColSeg)
                        {
                            var failpercent = 0.0;
                            foreach (var fitem in wfitem.DateColSeg)
                            {
                                failpercent = failpercent + fitem.y;
                            }
                            var tempy = 100.0 - failpercent;
                            ydata.Add(tempy);
                            if (tempy < ymin)
                            { ymin = tempy; }
                            cydata.Add(wfitem.total);
                        }


                        var xAxis = new { data = xdata };

                        var yAxis = new
                        {
                            title = "Yield (%)",
                            min = ymin,
                            max = 100.0
                        };

                        var min = new
                        {
                            name = "Min",
                            color = "#F0AD4E",
                            data = 94,
                            style = "dash"
                        };

                        var max = new
                        {
                            name = "Max",
                            color = "#C9302C",
                            data = 98,
                            style = "solid"
                        };

                        var data = new
                        {
                            name = "Yield",
                            color = "#ffa500",
                            data = ydata
                        };

                        var combinedata = new
                        {
                            min = min,
                            max = max,
                            data = data
                        };

                        var cdata = new
                        {
                            name = "Test Modules",
                            data = cydata
                        };

                        yieldarray.Add(new
                        {
                            id = id,
                            title = title,
                            xAxis = xAxis,
                            yAxis = yAxis,
                            data = combinedata,
                            cdata = cdata
                        });
                    }//end if
                }//end foreach


                //foreach (var fieldkv in fieldboxlist)
                foreach(var fname in fieldorderlist)
                {
                    var fieldname = fname;
                    var fieldval = fieldboxlist[fname];

                    var xaxisdata = new List<string>();

                    var ldatadata = new List<List<double>>();
                    var llinedata = new List<double>();

                    var rdatadata = new List<List<double>>();
                    var rlinedata = new List<double>();

                    var loutlierdata = new List<List<double>>();
                    var routlierdata = new List<List<double>>();

                    var widx = 0.0;
                    //foreach (var wfkv in fieldkv.Value)
                    foreach (var x in xwaferlist)
                    {
                        //var wf = wfkv.Key;
                        //xaxisdata.Add(wf);
                        //foreach (var box in wfkv.Value)

                        if (!fieldval.ContainsKey(x)) continue;

                        var wf = x;
                        xaxisdata.Add(wf);
                        var boxdict = fieldval[x];
                        foreach (var box in boxdict)
                        {
                            if (box.Contains("#V"))
                            {
                                var tempret = VcselBGDVM.CBOXFromRaw(box.Replace("#V", ""), vlowlimit, vhighlimit, nooutlier);
                                var cbox = (CBOXData)tempret[0];
                                var outlier = (List<VXVal>)tempret[1];

                                //var cbox = (CBOXData)Newtonsoft.Json.JsonConvert.DeserializeObject(box.Replace("#V", ""), (new CBOXData()).GetType());
                                var tempdd = new List<double>();
                                tempdd.Add((cbox.min < vlowlimit) ? vlowlimit : Math.Round(cbox.min, 4));
                                tempdd.Add(Math.Round(cbox.lower, 4));
                                tempdd.Add(Math.Round(cbox.mean, 4));
                                tempdd.Add(Math.Round(cbox.upper, 4));
                                tempdd.Add((cbox.max > vhighlimit) ? vhighlimit : Math.Round(cbox.max, 4));

                                
                                foreach (var outitem in outlier)
                                {
                                    var tempout = new List<double>();
                                    tempout.Add(Math.Round(widx + outitem.x, 4));
                                    tempout.Add(Math.Round(outitem.ival, 4));
                                    loutlierdata.Add(tempout);
                                }

                                ldatadata.Add(tempdd);
                                llinedata.Add(Math.Round(cbox.mean, 4));
                            }
                            else if (box.Contains("#U"))
                            {
                                var tempret = VcselBGDVM.CBOXFromRaw(box.Replace("#U", ""), ulowlimit, uhighlimit, nooutlier);
                                var cbox = (CBOXData)tempret[0];
                                var outlier = (List<VXVal>)tempret[1];

                                //var cbox = (CBOXData)Newtonsoft.Json.JsonConvert.DeserializeObject(box.Replace("#U", ""), (new CBOXData()).GetType());
                                var tempdd = new List<double>();
                                tempdd.Add((cbox.min < ulowlimit) ? ulowlimit : Math.Round(cbox.min, 4));
                                tempdd.Add(Math.Round(cbox.lower, 4));
                                tempdd.Add(Math.Round(cbox.mean, 4));
                                tempdd.Add(Math.Round(cbox.upper, 4));
                                tempdd.Add((cbox.max > uhighlimit) ? uhighlimit : Math.Round(cbox.max, 4));

                                foreach (var outitem in outlier)
                                {
                                    var tempout = new List<double>();
                                    tempout.Add(Math.Round(widx + outitem.x, 4));
                                    tempout.Add(Math.Round(outitem.ival, 4));
                                    routlierdata.Add(tempout);
                                }

                                rdatadata.Add(tempdd);
                                rlinedata.Add(Math.Round(cbox.mean, 4));
                            }
                            else
                            {
                                //var cbox = (CBOXData)Newtonsoft.Json.JsonConvert.DeserializeObject(box, (new CBOXData()).GetType());
                                var tempdd = new List<double>();

                                if (fieldname.ToUpper().Contains("Delta_PO_LD".ToUpper()))
                                {
                                    var tempret = VcselBGDVM.CBOXFromRaw(box, poldlowlimit, poldhighlimit, nooutlier);
                                    var cbox = (CBOXData)tempret[0];
                                    var outlier = (List<VXVal>)tempret[1];

                                    tempdd.Add((cbox.min < poldlowlimit) ? poldlowlimit : Math.Round(cbox.min, 4));
                                    tempdd.Add(Math.Round(cbox.lower, 4));
                                    tempdd.Add(Math.Round(cbox.mean, 4));
                                    tempdd.Add(Math.Round(cbox.upper, 4));
                                    tempdd.Add((cbox.max > poldhighlimit) ? poldhighlimit : Math.Round(cbox.max, 4));

                                    foreach (var outitem in outlier)
                                    {
                                        var tempout = new List<double>();
                                        tempout.Add(Math.Round(widx + outitem.x, 4));
                                        tempout.Add(Math.Round(outitem.ival, 4));
                                        loutlierdata.Add(tempout);
                                    }
                                    llinedata.Add(Math.Round(cbox.mean, 4));
                                }
                                else if (fieldname.ToUpper().Contains("Delta_PO_Uniformity".ToUpper()))
                                {
                                    var tempret = VcselBGDVM.CBOXFromRaw(box, poulowlimit, pouhighlimit, nooutlier);
                                    var cbox = (CBOXData)tempret[0];
                                    var outlier = (List<VXVal>)tempret[1];

                                    tempdd.Add((cbox.min < poulowlimit) ? poulowlimit : Math.Round(cbox.min, 4));
                                    tempdd.Add(Math.Round(cbox.lower, 4));
                                    tempdd.Add(Math.Round(cbox.mean, 4));
                                    tempdd.Add(Math.Round(cbox.upper, 4));
                                    tempdd.Add((cbox.max > pouhighlimit) ? pouhighlimit : Math.Round(cbox.max, 4));

                                    foreach (var outitem in outlier)
                                    {
                                        var tempout = new List<double>();
                                        tempout.Add(Math.Round(widx + outitem.x, 4));
                                        tempout.Add(Math.Round(outitem.ival, 4));
                                        loutlierdata.Add(tempout);
                                    }

                                    llinedata.Add(Math.Round(cbox.mean, 4));
                                }
                                else
                                {
                                    var tempret = VcselBGDVM.CBOXFromRaw(box, -99999.0, 99999.0, nooutlier);
                                    var cbox = (CBOXData)tempret[0];
                                    var outlier = (List<VXVal>)tempret[1];

                                    tempdd.Add(Math.Round(cbox.min, 4));
                                    tempdd.Add(Math.Round(cbox.lower, 4));
                                    tempdd.Add(Math.Round(cbox.mean, 4));
                                    tempdd.Add(Math.Round(cbox.upper, 4));
                                    tempdd.Add(Math.Round(cbox.max, 4));

                                    foreach (var outitem in outlier)
                                    {
                                        var tempout = new List<double>();
                                        tempout.Add(Math.Round(widx + outitem.x, 4));
                                        tempout.Add(Math.Round(outitem.ival, 4));
                                        loutlierdata.Add(tempout);
                                    }

                                    llinedata.Add(Math.Round(cbox.mean, 4));
                                }
                                ldatadata.Add(tempdd);
                                
                            }
                        }//end foreach

                        widx = widx + 1.0;
                    }//end forea

                    var id = fieldname.Replace(" ", "_") + "_id";
                    var title = fieldname + " Distribution by Wafer";
                    var xAxis = new
                    {
                        title = "Wafer#",
                        data = xaxisdata
                    };

                    if (fieldname.Contains("variation_uniformity_pold"))
                    {
                        var lyAxis = new { title = "Variation_POLD_Delta" };
                        var ryAxis = new { title = "Uniformity_POLD_Delta" };
                        var ldata = new
                        {
                            name = "Variation_POLD_Delta",
                            color = "#00b050",
                            data = ldatadata
                        };
                        var rdata = new
                        {
                            name = "Uniformity_POLD_Delta",
                            color = "#0099ff",
                            data = rdatadata
                        };
                        var lline = new
                        {
                            name = "Mean Line",
                            data = llinedata,
                            color = "#ffc000",
                            lineWidth = 1
                        };
                        var rline = new
                        {
                            name = "Mean Line",
                            data = rlinedata,
                            color = "#C9302C",
                            lineWidth = 1
                        };

                        var left = new
                        {
                            yAxis = lyAxis,
                            data = ldata,
                            line = lline
                        };
                        var right = new
                        {
                            yAxis = ryAxis,
                            data = rdata,
                            line = rline
                        };

                        boxarray.Add(new
                        {
                            id = id,
                            title = title,
                            xAxis = xAxis,
                            left = left,
                            right = right
                        });
                    }
                    else
                    {
                        var yAxis = new { title = "Value" };
                        var data = new
                        {
                            name = fieldname,
                            data = ldatadata
                        };
                        var line = new
                        {
                            name = "Mean Line",
                            data = llinedata,
                            color = "#ffc000",
                            lineWidth = 1
                        };

                        boxarray.Add(new
                        {
                            id = id,
                            title = title,
                            xAxis = xAxis,
                            yAxis = yAxis,
                            data = data,
                            line = line,
                            outlierdata = loutlierdata
                        });
                    }
                }//end foreach

                var errorcodedict = CfgUtility.GetBurnInErrorConfig(this);

                foreach (var item in testflist)
                {
                    if (item.DateColSeg.Count > 0)
                    {
                        var id = "f_" + item.TestType.Replace(" ", "_") + "_id";

                        var title = "Faliure Mode (" + item.TestType + ")";

                        var xdata = new List<string>();
                        var ydata = new List<FailureColumnData>();
                        var count = 0;
                        foreach (var f_item in item.DateColSeg)
                        {
                            xdata.Add(f_item.xkey);
                            count = (f_item.DateColSeg.Count > count) ? f_item.DateColSeg.Count : count;
                        }

                        var ymax = 3.0;
                        foreach (var wfitem in item.DateColSeg)
                        {
                            var tempmax = 0.0;
                            foreach (var fitem in wfitem.DateColSeg)
                            {
                                tempmax = tempmax + fitem.y;
                            }
                            if (tempmax > 3.0) ymax = (5.0 > ymax)?5.0:ymax;
                            if (tempmax > 5.0) ymax = (10.0 > ymax)?10.0:ymax;
                            if (tempmax > 10.0) ymax = (15.0 > ymax)?15.0:ymax;
                            if (tempmax > 15.0) ymax = (20.0 > ymax)?20.0:ymax;
                            if (tempmax > 20.0) ymax = (25.0 > ymax)?25.0:ymax;
                        }

                        var num = item.DateColSeg.Count;
                        var n = 1;
                        for (var i = count - 1; i >= 0; i--)
                        {

                            var ydata_tmp = new List<FailureColumnSeg>();
                            for (var m = 0; m < num; m++)
                            {
                                if (i < item.DateColSeg[m].DateColSeg.Count)
                                {
                                    item.DateColSeg[m].DateColSeg[i].y = Math.Round(item.DateColSeg[m].DateColSeg[i].y, 4);
                                    ydata_tmp.Add(item.DateColSeg[m].DateColSeg[i]);

                                    if (!errorcodedict.ContainsKey(item.DateColSeg[m].DateColSeg[i].name))
                                    {
                                        errorcodedict.Add(item.DateColSeg[m].DateColSeg[i].name, item.DateColSeg[m].DateColSeg[i].name);
                                    }
                                }
                                else
                                {
                                    var tempvm = new FailureColumnSeg();
                                    tempvm.name = "";
                                    tempvm.y = 0;
                                    tempvm.color = "#000";
                                    ydata_tmp.Add(tempvm);
                                }
                            }
                            ydata.Add(new FailureColumnData() { index = n, data = ydata_tmp });
                            n++;
                        }


                        var xAxis = new { data = xdata };
                        var yAxis = new
                        {
                            title = "(%)",
                            min = 0,
                            max = ymax
                        };

                        failurearray.Add(new
                        {
                            id = id,
                            coltype = "normal",
                            title = title,
                            xAxis = xAxis,
                            yAxis = yAxis,
                            data = ydata
                        });
                    }
                }

                var ret = new JsonResult();
                ret.MaxJsonLength = Int32.MaxValue;
                ret.Data = new
                {
                    success = true,
                    yieldarray = yieldarray,
                    boxarray = boxarray,
                    failurearray = failurearray,
                    colors = retdata[2],
                    errorcodedict = errorcodedict
                };
                return ret;
            }
            else
            {
                var ret = new JsonResult();
                ret.Data = new
                {
                    success = false,
                    msg = "No wafer match query condition."
                };
                return ret;
            }
        }

        public JsonResult HTOLDistributionRawData()
        {
            var wf_no = Request.Form["wf_no"];
            var vtype = Request.Form["wf_type"].Trim();
            var math_rect = Request.Form["math_rect"];
            var nooutlier = true;
            var boutlier = Request.Form["outlier"];
            if (string.Compare(boutlier, "YES", true) == 0)
            { nooutlier = false; }

            var wflist = new List<string>();
            if (!string.IsNullOrEmpty(wf_no))
            {
                wflist.AddRange(wf_no.Split(new string[] { ",", ";", " " }, StringSplitOptions.RemoveEmptyEntries));
            }
            else
            {
                var sdate = DateTime.Parse(Request.Form["sdate"]);
                var edate = DateTime.Parse(Request.Form["edate"]);
                var startdate = DateTime.Now;
                var enddate = DateTime.Now;
                if (sdate < edate)
                {
                    startdate = DateTime.Parse(sdate.ToString("yyyy-MM") + "-01 00:00:00");
                    enddate = DateTime.Parse(edate.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddDays(-1);
                }
                else
                {
                    startdate = DateTime.Parse(edate.ToString("yyyy-MM") + "-01 00:00:00");
                    enddate = DateTime.Parse(sdate.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddDays(-1);
                }

                wflist.AddRange(VcselTimeRange.RetrieveWafer(startdate, enddate, "HTOLVcselTimeRange"));
            }

            if (wflist.Count > 0)
            {

                bool withfaileddata = true;
                if (math_rect.ToUpper().Contains("ONLY PASS DATA"))
                {
                    withfaileddata = false;
                }


                var testnamedict = new Dictionary<string, bool>();
                testnamedict.Add(HTOLTESTTYPE.POST_BURN_IN, true);
                testnamedict.Add(HTOLTESTTYPE.POST_HTOL_BURN_IN, true);

                var fieldnamedict = new Dictionary<string, bool>();
                fieldnamedict.Add(HTOLTESTTYPE.PRE_BURN_IN + "*" + HTOLTESTFIELDNAME.PO_LD, true);
                fieldnamedict.Add(HTOLTESTTYPE.POST_BURN_IN + "*" + HTOLTESTFIELDNAME.DELTA_PO_LD, true);
                fieldnamedict.Add(HTOLTESTTYPE.POST_BURN_IN + "*" + HTOLTESTFIELDNAME.DELTA_PO_UNIFORMITY, true);
                fieldnamedict.Add(HTOLTESTTYPE.POST_HTOL_BURN_IN + "*" + HTOLTESTFIELDNAME.DELTA_PO_LD, true);

                var retdata = VcselBGDVM.RetrieveHTOLWaferData(wflist, withfaileddata, testnamedict, fieldnamedict, vtype);

                //fieldname,wafer,boxlist
                var fieldboxlist = (Dictionary<string, Dictionary<string, List<string>>>)retdata[0];
                var fieldorderlist = fieldboxlist.Keys.ToList();
                //fieldorderlist.Sort(); fieldorderlist.Reverse();
                fieldorderlist.Sort(delegate (string obj1, string obj2)
                {
                    if (obj1.ToUpper().Contains("PRE BURN IN"))
                    {
                        if (obj2.ToUpper().Contains("POST BURN IN")
                        || obj2.ToUpper().Contains("POST HTOL BURN IN"))
                        {
                            return -1;
                        }
                    }
                    else if (obj1.ToUpper().Contains("POST BURN IN"))
                    {
                        if (obj2.ToUpper().Contains("PRE BURN IN"))
                        {
                            return 1;
                        }

                        if (obj2.ToUpper().Contains("POST HTOL BURN IN"))
                        {
                            return -1;
                        }
                    }
                    else if (obj1.ToUpper().Contains("POST HTOL BURN IN"))
                    {
                        if (obj2.ToUpper().Contains("PRE BURN IN")
                        || obj2.ToUpper().Contains("POST BURN IN"))
                        {
                            return 1;
                        }
                    }

                    return obj1.CompareTo(obj2);
                    });

                var testflist = (List<TestFailureColumn>)retdata[1];
                testflist.Sort(delegate (TestFailureColumn obj1, TestFailureColumn obj2)
                {
                    if (obj1.TestType.ToUpper().Contains("PRE BURN IN"))
                    {
                        if (obj2.TestType.ToUpper().Contains("POST BURN IN")
                        || obj2.TestType.ToUpper().Contains("POST HTOL BURN IN"))
                        {
                            return -1;
                        }
                    }
                    else if (obj1.TestType.ToUpper().Contains("POST BURN IN"))
                    {
                        if (obj2.TestType.ToUpper().Contains("PRE BURN IN"))
                        {
                            return 1;
                        }

                        if (obj2.TestType.ToUpper().Contains("POST HTOL BURN IN"))
                        {
                            return -1;
                        }
                    }
                    else if (obj1.TestType.ToUpper().Contains("POST HTOL BURN IN"))
                    {
                        if (obj2.TestType.ToUpper().Contains("PRE BURN IN")
                        || obj2.TestType.ToUpper().Contains("POST BURN IN"))
                        {
                            return 1;
                        }
                    }

                    return obj1.TestType.CompareTo(obj2.TestType);
                });
                

                var yieldarray = new List<object>();
                var boxarray = new List<object>();
                var failurearray = new List<object>();

                var xwaferlist = new List<string>();
                var xwaferdict = new Dictionary<string, bool>();

                foreach (var item in testflist)
                {
                    if (item.DateColSeg.Count > 0)
                    {
                        var id = "f_yield_" + item.TestType.Replace(" ", "_") + "_id";
                        var title = item.TestType + " Wafer Yield";

                        var xdata = new List<string>();
                        var ydata = new List<double>();
                        var cydata = new List<double>();
                        var ymin = 95.0;

                        var count = 0;
                        foreach (var f_item in item.DateColSeg)
                        {
                            xdata.Add(f_item.xkey);
                            if (!xwaferdict.ContainsKey(f_item.xkey))
                            {
                                xwaferlist.Add(f_item.xkey);
                                xwaferdict.Add(f_item.xkey, true);
                            }

                            count = (f_item.DateColSeg.Count > count) ? f_item.DateColSeg.Count : count;
                        }

                        foreach (var wfitem in item.DateColSeg)
                        {
                            var failpercent = 0.0;
                            foreach (var fitem in wfitem.DateColSeg)
                            {
                                failpercent = failpercent + fitem.y;
                            }

                            var tempy = Math.Round(100.0 - failpercent,2);
                            ydata.Add(tempy);
                            if (tempy < ymin)
                            { ymin = tempy; }

                            cydata.Add(wfitem.total);
                        }


                        var xAxis = new { data = xdata };

                        var yAxis = new
                        {
                            title = "Yield (%)",
                            min = ((int)(ymin/10.0))*10.0,
                            max = 100.0
                        };

                        var tempmin = 94.0;
                        var tempmax = 98.0;
                        if (item.TestType.Contains("HTOL"))
                        {
                            tempmin = 98.0;
                            tempmax = 100.0;
                        }

                        var min = new
                        {
                            name = "Min",
                            color = "#F0AD4E",
                            data = tempmin,
                            style = "dash"
                        };

                        var max = new
                        {
                            name = "Max",
                            color = "#C9302C",
                            data = tempmax,
                            style = "solid"
                        };

                        var data = new
                        {
                            name = "Yield",
                            color = "#ffa500",
                            data = ydata
                        };

                        var combinedata = new
                        {
                            min = min,
                            max = max,
                            data = data
                        };

                        var cdata = new
                        {
                            name = "Test Modules",
                            data = cydata
                        };

                        yieldarray.Add(new
                        {
                            id = id,
                            title = title,
                            xAxis = xAxis,
                            yAxis = yAxis,
                            data = combinedata,
                            cdata = cdata
                        });
                    }//end if
                }//end foreach


                foreach (var fkey in fieldorderlist)
                {
                    var fieldname = fkey;
                    var waferfailuredict = fieldboxlist[fkey];

                    var xaxisdata = new List<string>();

                    var ldatadata = new List<List<double>>();
                    var llinedata = new List<double>();

                    var loutlierdata = new List<List<double>>();

                    var widx = 0.0;

                    foreach (var x in xwaferlist)
                    {

                        if (!waferfailuredict.ContainsKey(x)) continue;

                        var wf = x;
                        xaxisdata.Add(wf);
                        var boxdict = waferfailuredict[x];
                        foreach (var box in boxdict)
                        {
                                var tempdd = new List<double>();

                                var tempret = VcselBGDVM.CBOXFromRaw(box, -99999.0, 99999.0, nooutlier);
                                var cbox = (CBOXData)tempret[0];
                                var outlier = (List<VXVal>)tempret[1];

                                tempdd.Add(Math.Round(cbox.min, 4));
                                tempdd.Add(Math.Round(cbox.lower, 4));
                                tempdd.Add(Math.Round(cbox.mean, 4));
                                tempdd.Add(Math.Round(cbox.upper, 4));
                                tempdd.Add(Math.Round(cbox.max, 4));

                                foreach (var outitem in outlier)
                                {
                                    var tempout = new List<double>();
                                    tempout.Add(Math.Round(widx + outitem.x, 4));
                                    tempout.Add(Math.Round(outitem.ival, 4));
                                    loutlierdata.Add(tempout);
                                }

                                llinedata.Add(Math.Round(cbox.mean, 4));
                                
                                ldatadata.Add(tempdd);

                        }//end foreach

                        widx = widx + 1.0;
                    }//end forea

                    var id = fieldname.Replace(" ", "_") + "_id";
                    var title = fieldname + " Distribution by Wafer";
                    var xAxis = new
                    {
                        title = "Wafer#",
                        data = xaxisdata
                    };

                    var yAxis = new { title = "Value" };
                    var data = new
                    {
                        name = fieldname,
                        data = ldatadata
                    };
                    var line = new
                    {
                        name = "Mean Line",
                        data = llinedata,
                        color = "#ffc000",
                        lineWidth = 1
                    };

                    boxarray.Add(new
                    {
                        id = id,
                        title = title,
                        xAxis = xAxis,
                        yAxis = yAxis,
                        data = data,
                        line = line,
                        outlierdata = loutlierdata
                    });
                    
                }//end foreach

                var errorcodedict = CfgUtility.GetBurnInErrorConfig(this);

                foreach (var item in testflist)
                {
                    if (item.DateColSeg.Count > 0)
                    {
                        var id = "f_" + item.TestType.Replace(" ", "_") + "_id";

                        var title = "Faliure Mode (" + item.TestType + ")";

                        var xdata = new List<string>();
                        var ydata = new List<FailureColumnData>();
                        var count = 0;
                        foreach (var f_item in item.DateColSeg)
                        {
                            xdata.Add(f_item.xkey);
                            count = (f_item.DateColSeg.Count > count) ? f_item.DateColSeg.Count : count;
                        }

                        var ymax = 3.0;
                        foreach (var wfitem in item.DateColSeg)
                        {
                            var tempmax = 0.0;
                            foreach (var fitem in wfitem.DateColSeg)
                            {
                                tempmax = tempmax + fitem.y;
                            }
                            if (tempmax > 3.0) ymax = (5.0 > ymax) ? 5.0 : ymax;
                            if (tempmax > 5.0) ymax = (10.0 > ymax) ? 10.0 : ymax;
                            if (tempmax > 10.0) ymax = (15.0 > ymax) ? 15.0 : ymax;
                            if (tempmax > 15.0) ymax = (20.0 > ymax) ? 20.0 : ymax;
                            if (tempmax > 20.0) ymax = (25.0 > ymax) ? 25.0 : ymax;
                        }

                        var num = item.DateColSeg.Count;
                        var n = 1;
                        for (var i = count - 1; i >= 0; i--)
                        {

                            var ydata_tmp = new List<FailureColumnSeg>();
                            for (var m = 0; m < num; m++)
                            {
                                if (i < item.DateColSeg[m].DateColSeg.Count)
                                {
                                    item.DateColSeg[m].DateColSeg[i].y = Math.Round(item.DateColSeg[m].DateColSeg[i].y, 4);
                                    ydata_tmp.Add(item.DateColSeg[m].DateColSeg[i]);

                                    if (!errorcodedict.ContainsKey(item.DateColSeg[m].DateColSeg[i].name))
                                    {
                                        errorcodedict.Add(item.DateColSeg[m].DateColSeg[i].name, item.DateColSeg[m].DateColSeg[i].name);
                                    }
                                }
                                else
                                {
                                    var tempvm = new FailureColumnSeg();
                                    tempvm.name = "";
                                    tempvm.y = 0;
                                    tempvm.color = "#000";
                                    ydata_tmp.Add(tempvm);
                                }
                            }
                            ydata.Add(new FailureColumnData() { index = n, data = ydata_tmp });
                            n++;
                        }


                        var xAxis = new { data = xdata };
                        var yAxis = new
                        {
                            title = "(%)",
                            min = 0,
                            max = ymax
                        };

                        failurearray.Add(new
                        {
                            id = id,
                            coltype = "normal",
                            title = title,
                            xAxis = xAxis,
                            yAxis = yAxis,
                            data = ydata
                        });
                    }
                }

                var ret = new JsonResult();
                ret.MaxJsonLength = Int32.MaxValue;
                ret.Data = new
                {
                    success = true,
                    yieldarray = yieldarray,
                    boxarray = boxarray,
                    failurearray = failurearray,
                    colors = retdata[2],
                    errorcodedict = errorcodedict
                };
                return ret;
            }
            else
            {
                var ret = new JsonResult();
                ret.Data = new
                {
                    success = false,
                    msg = "No wafer match query condition."
                };
                return ret;
            }
        }

        public ActionResult DownLoadWafer(string wf_no,string withfilter)
        {
            string datestring = DateTime.Now.ToString("yyyyMMdd");
            string imgdir = Server.MapPath("~/userfiles") + "\\docs\\" + datestring + "\\";
            if (!System.IO.Directory.Exists(imgdir))
            {
                System.IO.Directory.CreateDirectory(imgdir);
            }

            var fn = "Wafer_TestData_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
            var filename = imgdir + fn;

            var fw = System.IO.File.OpenWrite(filename);

            var filter = false;
            if (!string.IsNullOrEmpty(withfilter) && string.Compare(withfilter, "true", true) == 0)
            { filter = true; }
            VcselBGDVM.RetrieveBURNINHTOLDataByWaferWithFilter(wf_no, fw,filter);
            fw.Close();

            try
            {
                var fzip = new ICSharpCode.SharpZipLib.Zip.FastZip();
                fzip.CreateZip(imgdir + fn.Replace(".csv", ".zip"), imgdir, false, fn);
                try { System.IO.File.Delete(filename); } catch (Exception ex) { }
                return File(filename.Replace(".csv", ".zip"), "application/vnd.zip", fn.Replace(".csv", ".zip"));
            }
            catch (Exception ex)
            {
                if (!System.IO.File.Exists(filename))
                { System.IO.File.WriteAllText(filename, "Fail to download data."); }
                return File(filename, "application/vnd.ms-excel", fn);
            }
        }

        public ActionResult DownLoadHTOL(string wf_no,string withfilter)
        {
            string datestring = DateTime.Now.ToString("yyyyMMdd");
            string imgdir = Server.MapPath("~/userfiles") + "\\docs\\" + datestring + "\\";
            if (!System.IO.Directory.Exists(imgdir))
            {
                System.IO.Directory.CreateDirectory(imgdir);
            }

            var fn = "HTOL_TestData_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
            var filename = imgdir + fn;

            var filter = false;
            if (!string.IsNullOrEmpty(withfilter) && string.Compare(withfilter, "true", true) == 0)
            { filter = true; }

            var fw = System.IO.File.OpenWrite(filename);
            VcselBGDVM.RetrieveBURNINHTOLDataByWaferWithFilter(wf_no, fw,filter, "BIHTOLTestResultDataField", "BIHTOLTestResult");
            fw.Close();

            try
            {
                var fzip = new ICSharpCode.SharpZipLib.Zip.FastZip();
                fzip.CreateZip(imgdir + fn.Replace(".csv", ".zip"), imgdir, false, fn);
                try { System.IO.File.Delete(filename); } catch (Exception ex) { }
                return File(filename.Replace(".csv", ".zip"), "application/vnd.zip", fn.Replace(".csv", ".zip"));
            }
            catch (Exception ex)
            {
                if (!System.IO.File.Exists(filename))
                { System.IO.File.WriteAllText(filename, "Fail to download data."); }
                return File(filename, "application/vnd.ms-excel", fn);
            }
        }
        

        public ActionResult DownLoadWaferByMonth(string month,string vtype)
        {
            string datestring = DateTime.Now.ToString("yyyyMMdd");
            string imgdir = Server.MapPath("~/userfiles") + "\\docs\\" + datestring + "\\";
            if (!System.IO.Directory.Exists(imgdir))
            {
                System.IO.Directory.CreateDirectory(imgdir);
            }
            var rate = vtype.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries)[0];
            var ch = vtype.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries)[1];
            var pninfolist = VcselPNData.RetrieveVcselPNInfo().Values.ToList();
            var pnlist = new List<string>();
            foreach (var p in pninfolist)
            {
                if (string.Compare(p.Rate, rate, true) == 0
                    && string.Compare(p.Channel, ch, true) == 0)
                {
                    pnlist.Add(p.PN);
                }
            }

            var fn = "Wafer_" + month.Replace("/","-")+"_"+vtype + "_TestData_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
            var filename = imgdir + fn;

            var fw = System.IO.File.OpenWrite(filename);
            if (pnlist.Count > 0)
            {
               VcselBGDVM.RetrieveBURNINHTOLDataByMonth(month,pnlist,fw);
            }

            fw.Close();

            try
            {
                var fzip = new ICSharpCode.SharpZipLib.Zip.FastZip();
                fzip.CreateZip(imgdir+fn.Replace(".csv", ".zip"), imgdir, false, fn);
                try { System.IO.File.Delete(filename); } catch (Exception ex) { }
                return File(filename.Replace(".csv", ".zip"), "application/vnd.zip", fn.Replace(".csv", ".zip"));
            }
            catch (Exception ex)
            {
                if (!System.IO.File.Exists(filename))
                { System.IO.File.WriteAllText(filename, "Fail to download data."); }
                return File(filename, "application/vnd.ms-excel", fn);
            }
        }

        public ActionResult DownLoadHTOLByMonth(string month, string vtype)
        {
            string datestring = DateTime.Now.ToString("yyyyMMdd");
            string imgdir = Server.MapPath("~/userfiles") + "\\docs\\" + datestring + "\\";
            if (!System.IO.Directory.Exists(imgdir))
            {
                System.IO.Directory.CreateDirectory(imgdir);
            }
            var rate = vtype.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries)[0];
            var ch = vtype.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries)[1];
            var pninfolist = VcselPNData.RetrieveVcselPNInfo().Values.ToList();
            var pnlist = new List<string>();
            foreach (var p in pninfolist)
            {
                if (string.Compare(p.Rate, rate, true) == 0
                    && string.Compare(p.Channel, ch, true) == 0)
                {
                    pnlist.Add(p.PN);
                }
            }

            var fn = "HTOL_" + month.Replace("/", "-") + "_" + vtype + "_TestData_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
            var filename = imgdir + fn;

            var fw = System.IO.File.OpenWrite(filename);
            if (pnlist.Count > 0)
            {
                VcselBGDVM.RetrieveBURNINHTOLDataByMonth(month, pnlist, fw, "BIHTOLTestResultDataField", "BIHTOLTestResult");
            }

            fw.Close();

            try
            {
                var fzip = new ICSharpCode.SharpZipLib.Zip.FastZip();
                fzip.CreateZip(imgdir + fn.Replace(".csv", ".zip"), imgdir, false, fn);
                try { System.IO.File.Delete(filename); } catch (Exception ex) { }
                return File(filename.Replace(".csv", ".zip"), "application/vnd.zip", fn.Replace(".csv", ".zip"));
            }
            catch (Exception ex)
            {
                if (!System.IO.File.Exists(filename))
                { System.IO.File.WriteAllText(filename, "Fail to download data."); }
                return File(filename, "application/vnd.ms-excel", fn);
            }
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

            var sb = new StringBuilder(30 * vm.Count);

            foreach (var item in vm)
            {
                sb.Append("[" +item.x.ToString()+","+item.y.ToString()+","+item.value.ToString()+ "],");

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

            MAPDATA = sb.ToString();
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

            NeoMapChartCache.UpdateChart(querycond, datafield, NEOMAPCHARTTYPE.HOTMAP, scritpttxt);
            
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

            var sb = new StringBuilder(30 * count);
            for (var idx = 0; idx < count; idx++)
            {
                sb.Append("[" + valuelist[idx].ToString() + "," + (ylist[idx] / rate).ToString() + "],");
            }

            YVALUES = sb.ToString();
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

            NeoMapChartCache.UpdateChart(querycond, datafield, NEOMAPCHARTTYPE.NORMALDIS, scritpttxt);

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
            var cbox = WaferBGDField.GetBoxData(filteddata.FiltedList, -9999.0, 9999.0);
            var outlierlist = new List<VXVal>();
            var rad = new System.Random(DateTime.Now.Second);
            var idx = 0;
            foreach (var data in filteddata.FiltedList)
            {
                if ((data > -9999.0 && data < cbox.min)
                    || (data < 9999.0 && data > cbox.max))
                {
                    var tempdata = new VXVal();
                    tempdata.ival = data;
                    if (idx % 2 == 0)
                    { tempdata.x = rad.NextDouble() / 5.0; }
                    else
                    { tempdata.x = 0 - rad.NextDouble() / 5.0; }
                    outlierlist.Add(tempdata);
                    idx = idx + 1;
                }
            }

            var DATAFIELDNAME = datafield.Replace(TXOQUERYCOND.NEOMAP, "").Replace(TXOQUERYCOND.BURNIN, "").Replace(TXOQUERYCOND.TEST, "");
            var Title = querycond + " " + DATAFIELDNAME + " Box Plot";

            var YVALUES = cbox.min.ToString() + "," + cbox.lower.ToString() + "," + cbox.mean.ToString() + "," + cbox.upper.ToString() + "," + cbox.max.ToString();
            var boxplotdata ="[[#YVALUES#]]";
            boxplotdata = boxplotdata.Replace("#YVALUES#", YVALUES);

            var outlierdatalist = "[#OUTLIERVAL#]";
            var outlierval = "";
            var sb = new StringBuilder(30 * outlierlist.Count);
            foreach (var item in outlierlist)
            {
                sb.Append("[" + item.x + "," + item.ival + "],");
            }
            if (outlierlist.Count > 0)
            {
                outlierval = sb.ToString();
                outlierval = outlierval.Substring(0, outlierval.Length - 1);
            }
            outlierdatalist = outlierdatalist.Replace("#OUTLIERVAL#", outlierval);

            var ElementID = "leftboxplot";
            if (!left)
            {
                ElementID = "rightboxplot";
            }

            var scritpttxt = System.IO.File.ReadAllText(Server.MapPath("~/Scripts/BoxPlotNeomap.xml"));
            scritpttxt = scritpttxt.Replace("#ElementID#", ElementID)
                    .Replace("#Title#", Title)
                    .Replace("#BOXNAME#", DATAFIELDNAME)
                    .Replace("#BOXDATA#", boxplotdata)
                    .Replace("#OUTLIERDATA#", outlierdatalist);

            NeoMapChartCache.UpdateChart(querycond, datafield, NEOMAPCHARTTYPE.BOXPLOT, scritpttxt);

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
            var chartlist = NeoMapChartCache.RetrieveCharts(querycond, datafield);
            if (chartlist.Count > 0)
            {
                foreach (var item in chartlist)
                {
                    if (item.ChartType.Contains(NEOMAPCHARTTYPE.HOTMAP))
                    {
                        if (left)
                        { ViewBag.lefthotscript = item.Chart.Replace("#right","#left");}
                        else
                        {ViewBag.righthotscript = item.Chart.Replace("#left","#right" ); }
                    }
                    if (item.ChartType.Contains(NEOMAPCHARTTYPE.NORMALDIS))
                    {
                        if (left)
                        { ViewBag.leftnormaldistr = item.Chart.Replace("#right", "#left"); }
                        else
                        { ViewBag.rightnormaldistr = item.Chart.Replace("#left", "#right"); }
                    }
                    if (item.ChartType.Contains(NEOMAPCHARTTYPE.BOXPLOT))
                    {
                        if (left)
                        { ViewBag.leftboxplot = item.Chart.Replace("#right", "#left"); }
                        else
                        { ViewBag.rightboxplot = item.Chart.Replace("#left", "#right"); }
                    }
                }
            }
            else
            {
                var vm = ExternalDataCollector.RetrieveNeoMapData(querycond, datafield);
                if (vm.Count > 500)
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


            DataSolveWithSide(leftcond, leftfield, waferdict,bijodict, true);
            DataSolveWithSide(rightcond, rightfield, waferdict, bijodict, false);

            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
            ViewBag.UserName = updater.Split(new char[] { '@' })[0];
            ViewBag.RealUserID = updater;

            waferlist.AddRange(bijolist);
            preparetxotestdata(waferlist,"", "", "", "");
            return View();
        }


        private void prepareneomapviewlist(List<string> waferlist)
        {
            var tempcondlist = new List<string>();
            tempcondlist.AddRange(waferlist);
            ViewBag.queryvallist = tempcondlist;

            var datafieldlist = ExternalDataCollector.NeoMapMainFieldNameList();
            var selectlist = new List<string>();
            selectlist.Add("Please select data field");
            selectlist.AddRange(datafieldlist);
            var selectcontrol = CreateSelectList(selectlist, "");
            selectcontrol[0].Disabled = true;
            selectcontrol[0].Selected = true;
            ViewBag.leftdatafieldlist = selectcontrol;

            selectlist = new List<string>();
            selectlist.Add("Please select data field");
            selectlist.AddRange(datafieldlist);
            selectcontrol = CreateSelectList(selectlist, "");
            selectcontrol[0].Disabled = true;
            selectcontrol[0].Selected = true;
            ViewBag.rightdatafieldlist = selectcontrol;
        }

        private void NeoMapDataSolveWithSide(string cond, string field, bool left)
        {
            if (!string.IsNullOrEmpty(cond)
            && !cond.ToUpper().Contains("PLEASE")
            && !string.IsNullOrEmpty(field))
            {
                NeoMapDataAnalysis(cond, field, left);
            }
        }

        public ActionResult NeoMapDataAnalysis()
        {
            var waferdict = ExternalDataCollector.RetrieveNeoMapWaferList();
            var waferlist = waferdict.Keys.ToList();
            waferlist.Sort();waferlist.Reverse();
            prepareneomapviewlist(waferlist);
            return View();
        }

        [HttpPost, ActionName("NeoMapDataAnalysis")]
        [ValidateAntiForgeryToken]
        public ActionResult NeoMapDataAnalysisPost()
        {
            var leftcond = Request.Form["leftquerylist"].Trim();
            var leftfield = Request.Form["leftdatafieldlist"];
            var rightcond = Request.Form["rightquerylist"].Trim();
            var rightfield = Request.Form["rightdatafieldlist"];

            if (leftfield != null)
                leftfield = leftfield.Trim();
            if (rightfield != null)
                rightfield = rightfield.Trim();

            NeoMapDataSolveWithSide(leftcond, leftfield,true);
            NeoMapDataSolveWithSide(rightcond, rightfield,false);

            var waferdict = ExternalDataCollector.RetrieveNeoMapWaferList();
            var waferlist = waferdict.Keys.ToList();
            waferlist.Sort();waferlist.Reverse();
            prepareneomapviewlist(waferlist);
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

        private void ModuleInBI(string BR,string modulevalue,bool moredata)
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

            var rawlist = BITestResultDataField.RetrieveBITestData(BR, TXOQUERYCOND.BURNIN + "PO_LD", TXOQUERYTYPE.BR, optioncond, moredata);
            if (rawlist.Count > 5)
            {
                var filteddata = GetCleanDataWithStdDev(rawlist);
                ViewBag.leftbinormaldistr =  NEONormalDistributeChart1(filteddata, BR.Replace("-", ""), "TxPower" + fieldappend, true, "leftbinormaldistr",modulevalue);
            }
        }

        private void ModuleInAlign(string BR, string modulevalue,bool moredata)
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

        private void ModuleInModule(string BR, string modulevalue,bool moredata)
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

            var rawlist = ModuleTXOData.RetrieveModuleTestData(BR, TXOQUERYCOND.TEST + "TxPower", TXOQUERYTYPE.BR, optioncond, moredata);
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

            bool moredata = false;
            if (Request.Form["moredata"] != null
                && string.Compare(Request.Form["moredata"].ToUpper(),"TRUE") == 0)
            {
                moredata = true;
            }

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
                    ModuleInBI(BR,bitestdata[bitestdata.Count-1].PO_LD.ToString(),moredata);
                }
            }

            if (aligntestdata.Count > 0)
            {
                var jo = aligntestdata[0].JO;
                var jos = jo.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                if (jos.Length == 3)
                {
                    var BR = "-" + jos[1] + "-";
                    ModuleInAlign(BR, aligntestdata[aligntestdata.Count -1].TxPower.ToString(),moredata);
                }
            }

            if (moduletestdata.Count > 0)
            {
                var jo = moduletestdata[0].JO;
                var jos = jo.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                if (jos.Length == 3)
                {
                    var BR = "-" + jos[1] + "-";
                    ModuleInModule(BR, moduletestdata[moduletestdata.Count -1].TxPower.ToString(),moredata);
                }
            }

            var ckdict = CookieUtility.UnpackCookie(this);
            var updater = ckdict["logonuser"].Split(new char[] { '|' })[0];
            ViewBag.UserName = updater.Split(new char[] { '@' })[0];
            ViewBag.RealUserID = updater;
            preparemoduletxotestdata(updater);
            return View();
        }


        public ActionResult CPKAnalysis()
        {
            var yesnolist = new List<string>();
            yesnolist.Add("NO");
            yesnolist.Add("YES");
            ViewBag.allpasslist = CreateSelectList(yesnolist, "");

            var dblist = new List<string>();
            dblist.Add("MES");
            dblist.Add("ATE");
            ViewBag.databaselist = CreateSelectList(dblist, "");

            return View();
        }

        public JsonResult GetStandardPJList()
        {
            var cfgpjdict = CfgUtility.GetStandardPJList(this);
            var inputpj = CPKCache.RetrievePJList();
            foreach (var pj in inputpj)
            {
                if (cfgpjdict.ContainsKey(pj))
                {
                    cfgpjdict.Remove(pj);
                }
            }
            var pjdata = cfgpjdict.Keys.ToList();
            pjdata.Sort();

            foreach (var pj in inputpj)
            {
                pjdata.Insert(0,pj);
            }
            
            var ret = new JsonResult();
            ret.Data = new { pjdata = pjdata };
            return ret;
        }
        
        public JsonResult GetMESTabPN()
        {
            var pj = Request.Form["pj"];
            var mestablist = CPKCache.RetrieveMESTabList(pj);
            var pnlist = CPKCache.RetrieveMESPNDesList(pj);
            var datasource = CPKCache.RetrieveDataSource(pj);

            var ret = new JsonResult();
            ret.Data = new {
                    mestablist = mestablist,
                    pnlist = pnlist,
                datasource = datasource
            };
            return ret;
        }

        public JsonResult GetMESParam()
        {
            var mestab = Request.Form["mestab"];
            var paramlist = CPKCache.RetrieveMESParamList(mestab);

            var ret = new JsonResult();
            ret.Data = new
            {
                paramlist = paramlist
            };
            return ret;
        }

        public JsonResult GetMESLimit()
        {
            var param = Request.Form["param"];
            var cornlist = CPKCache.RetrieveCornIDList(param);
            var lowlimitlist = CPKCache.RetrieveMESLowLimitList(param);
            var highlimitlist = CPKCache.RetrieveMESHighLimitList(param);
            var ret = new JsonResult();
            ret.Data = new
            {
                cornlist = cornlist,
                lowlimitlist = lowlimitlist,
                highlimitlist = highlimitlist
            };
            return ret;
        }

        private string DownLoadCPKSourceData(List<double> minlist,List<double> maxlist,string param,CPKData cpkmindata,CPKData cpkmaxdata
                    ,string datafrom,double mean,double stddev,string isnormal,double realcpk,double dppm, List<KeyValuePair<string, string>> datawithname = null)
        {
            string datestring = DateTime.Now.ToString("yyyyMMdd");
            string imgdir = Server.MapPath("~/userfiles") + "\\docs\\" + datestring + "\\";
            if (!System.IO.Directory.Exists(imgdir))
            {
                System.IO.Directory.CreateDirectory(imgdir);
            }

            var fn = param.Replace(" ","_").Replace("%","").Replace(",","").Replace(":","").Replace("#", "").Replace("'", "")
                            .Replace("&", "").Replace("?", "").Replace("+", "") + "_sourcedata_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
            var filename = imgdir + fn;

            var sb = new StringBuilder((minlist.Count + maxlist.Count + 50) * 30);
            sb.Append(param + ",\r\n");

            sb.Append("DATA FROM," + datafrom + "," + "Mean," + mean.ToString() + "," + "StdDev," + stddev.ToString()
                + "," + "Normal Distribution," + isnormal + "," + "Select CPK," + realcpk.ToString() + "," + "DPPM," + dppm.ToString() + ",\r\n");

            sb.Append("Normal Probability (min)," + cpkmindata.IsNormalProbability.ToString() + ",Normal Probability (max)," + cpkmaxdata.IsNormalProbability.ToString() + ",\r\n");
            sb.Append("General CPK (min)," + cpkmindata.Cpk_ca.ToString() + ",General CPK (max)," + cpkmaxdata.Cpk_ca.ToString() + ",\r\n");
            sb.Append("Robust CPK (min)," + cpkmindata.Cpk_robust.ToString() + ",Robust CPK (max)," + cpkmaxdata.Cpk_robust.ToString() + ",\r\n");
            sb.Append("Mean (min)," + cpkmindata.Mean.ToString() + ",Mean (max)," + cpkmaxdata.Mean.ToString() + ",\r\n");
            sb.Append("StdDev (min)," + cpkmindata.Stdev.ToString() + ",StdDev (max)," + cpkmaxdata.Stdev.ToString() + ",\r\n");
            sb.Append("CPK (min)," + cpkmindata.Cpk_final.ToString() + ",CPK (max)," + cpkmaxdata.Cpk_final.ToString() + ",\r\n");
            sb.Append("DPPM (min)," + cpkmindata.DPPM.ToString() + ",DPPM (max)," + cpkmaxdata.DPPM.ToString() + ",\r\n");

            sb.Append("Source Data,\r\n");

            if (datawithname != null)
            {
                foreach (var item in datawithname)
                {
                    sb.Append(item.Key + "," + item.Value + ",\r\n");
                }
            }
            else
            {
                foreach (var data in minlist)
                {
                    sb.Append(data.ToString() + ",min,\r\n");
                }
                foreach (var data in maxlist)
                {
                    sb.Append(data.ToString() + ",max,\r\n");
                }
            }

            var bt = System.Text.Encoding.UTF8.GetBytes(sb.ToString());

            var fw = System.IO.File.OpenWrite(filename);
            fw.Write(bt, 0, bt.Count());
            fw.Close();

            var url = "/userfiles/docs/" + datestring + "/" + fn;
            try
            {
                var fzip = new ICSharpCode.SharpZipLib.Zip.FastZip();
                fzip.CreateZip(imgdir + fn.Replace(".csv", ".zip"), imgdir, false, fn);
                url = "/userfiles/docs/" + datestring + "/" + fn.Replace(".csv", ".zip");
            }
            catch (Exception ex)
            {}
            return url;
        }

        private JsonResult MESCPK(string database, bool onlypass)
        {
            var pj = Request.Form["pj"];
            var mestab = Request.Form["mestab"].ToUpper().Trim();
            if (mestab.IndexOf("DC_") == 0)
            {
                mestab = mestab.Substring(3);
            }
            var paramliststr = Request.Form["param"];
            var cornid = Request.Form["cornid"];
            var lowlimit = Request.Form["lowlimit"].Split(new string[] { ",", ";" }, StringSplitOptions.None).ToList();
            var highlimit = Request.Form["highlimit"].Split(new string[] { ",", ";" }, StringSplitOptions.None).ToList();
            var startdate = Request.Form["startdate"];
            var enddate = Request.Form["enddate"];
            var pnlist = Request.Form["pnlist"];

            var cpkdatalist = new List<object>();
            var chartlist = new List<object>();

            var idx = 0;
            var paramlist = paramliststr.Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();

            if ((paramlist.Count != lowlimit.Count)
                || (paramlist.Count != highlimit.Count))
            {
                var ret = new JsonResult();
                ret.Data = new
                {
                    msg = "parameter list should match limit list one by one!",
                    success = false
                };
                return ret;
            }

            foreach (var param in paramlist)
            {
                var rawdata = MESUtility.GetTestData(pnlist, mestab, param, cornid, startdate, enddate, onlypass);
                if (rawdata.Count == 0)
                {
                    var ret = new JsonResult();
                    ret.Data = new
                    {
                        msg = "Fail to get test data, please check your query condition!",
                        success = false
                    };
                    return ret;
                }

                var minlist = (List<double>)rawdata[0];
                var maxlist = (List<double>)rawdata[1];

                if (minlist.Count < 100)
                {
                    var ret = new JsonResult();
                    ret.Data = new
                    {
                        msg = "No enough data is gotten,please enlarge your query period!",
                        success = false
                    };
                    return ret;
                }

                CPKCache.UpdateCPKParams(pj, mestab, pnlist, param, cornid, lowlimit[idx].Trim(), highlimit[idx].Trim(), database);

                var cpkmindata = CPKData.GetCpk(minlist, lowlimit[idx].Trim(), highlimit[idx].Trim())[0];
                var cpkmaxdata = CPKData.GetCpk(maxlist, lowlimit[idx].Trim(), highlimit[idx].Trim())[0];

                var datafrom = "MAX DATASET";
                var mean = cpkmaxdata.Mean;
                var stddev = cpkmaxdata.Stdev;
                var isnormal = cpkmaxdata.IsNormalStr;
                var dppm = cpkmaxdata.DPPM;

                var realcpk = Math.Min(cpkmindata.Cpk_final, cpkmaxdata.Cpk_final);
                if (realcpk == cpkmindata.Cpk_final)
                {
                    datafrom = "MIN DATASET";
                    mean = cpkmindata.Mean;
                    stddev = cpkmindata.Stdev;
                    isnormal = cpkmindata.IsNormalStr;
                    dppm = cpkmindata.DPPM;
                }


                var id = "min_" + param.Replace(" ", "_").ToLower() + "_id";
                var title = param + " Min Dataset Histogram";
                chartlist.Add(HistogramChart.GetChartData(id, title, lowlimit[idx].Trim(), highlimit[idx].Trim(), minlist));

                id = "max_" + param.Replace(" ", "_").ToLower() + "_id";
                title = param + " Max Dataset Histogram";
                chartlist.Add(HistogramChart.GetChartData(id, title, lowlimit[idx].Trim(), highlimit[idx].Trim(), maxlist));

                var sourcedata = DownLoadCPKSourceData(minlist, maxlist, param, cpkmindata, cpkmaxdata
                    , datafrom, mean, stddev,isnormal,realcpk,dppm);

                cpkdatalist.Add(
                    new
                    {
                        param = param,
                        datafrom = datafrom,
                        mean = Math.Round(mean, 5),
                        stddev = Math.Round(stddev, 5),
                        isnormal = isnormal,
                        realcpk = realcpk,
                        dppm = dppm,
                        probmin = cpkmindata.IsNormalProbability,
                        probmax = cpkmaxdata.IsNormalProbability,
                        gcpkmin = cpkmindata.Cpk_ca,
                        gcpkmax = cpkmaxdata.Cpk_ca,
                        rcpkmin = cpkmindata.Cpk_robust,
                        rcpkmax = cpkmaxdata.Cpk_robust,
                        meanmin = Math.Round(cpkmindata.Mean, 5),
                        meanmax = Math.Round(cpkmaxdata.Mean, 5),
                        stddevmin = Math.Round(cpkmindata.Stdev, 5),
                        stddevmax = Math.Round(cpkmaxdata.Stdev, 5),
                        rdppmmin = cpkmindata.DPPM,
                        rdppmmax = cpkmaxdata.DPPM,
                        cpkmin = cpkmindata.Cpk_final,
                        cpkmax = cpkmaxdata.Cpk_final,
                        sourcedata = sourcedata
                    });

                idx = idx + 1;
            }//end foreach
 
            var allret = new JsonResult();
            allret.Data = new
            {
                success = true,
                cpkdatalist = cpkdatalist,
                chartlist = chartlist
            };

            return allret;
        }

        private JsonResult ATECPK(string database, bool onlypass)
        {
            var pj = Request.Form["pj"];
            var mestab = Request.Form["mestab"].Trim();
            var paramliststr = Request.Form["param"];
            var cornid = Request.Form["cornid"];
            var startdate = Request.Form["startdate"];
            var enddate = Request.Form["enddate"];
            var pnlist = Request.Form["pnlist"];

            var cpkdatalist = new List<object>();
            var chartlist = new List<object>();

            var newmestab = mestab;
            if (!string.IsNullOrEmpty(cornid))
            { newmestab += "-" + cornid; }

            var paramlist = paramliststr.Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (var param in paramlist)
            {
                
                var rawdata = ATEUtility.GetTestData(pnlist, newmestab, param, startdate, enddate, onlypass);
                if (rawdata.Count == 0)
                {
                    var ret = new JsonResult();
                    ret.Data = new
                    {
                        msg = "Fail to get test data, please check your query condition!",
                        success = false
                    };
                    return ret;
                }

                var minlist = (List<double>)rawdata[0];
                var maxlist = (List<double>)rawdata[1];
                var lowlimit = (string)rawdata[2];
                var highlimit = (string)rawdata[3];

                if (minlist.Count < 100)
                {
                    var ret = new JsonResult();
                    ret.Data = new
                    {
                        msg = "No enough data is gotten,please enlarge your query period!",
                        success = false
                    };
                    return ret;
                }


                CPKCache.UpdateCPKParams(pj, mestab, pnlist, param, cornid, lowlimit, highlimit, database);

                var cpkmindata = CPKData.GetCpk(minlist, lowlimit, highlimit)[0];
                var cpkmaxdata = CPKData.GetCpk(maxlist, lowlimit, highlimit)[0];

                var datafrom = "MAX DATASET";
                var mean = cpkmaxdata.Mean;
                var stddev = cpkmaxdata.Stdev;
                var isnormal = cpkmaxdata.IsNormalStr;
                var dppm = cpkmaxdata.DPPM;

                var realcpk = Math.Min(cpkmindata.Cpk_final, cpkmaxdata.Cpk_final);
                if (realcpk == cpkmindata.Cpk_final)
                {
                    datafrom = "MIN DATASET";
                    mean = cpkmindata.Mean;
                    stddev = cpkmindata.Stdev;
                    isnormal = cpkmindata.IsNormalStr;
                    dppm = cpkmindata.DPPM;
                }

                var id = "min_" + param.Replace(" ", "_").ToLower() + "_id";
                var title = param + " Min Dataset Histogram";
                chartlist.Add(HistogramChart.GetChartData(id, title, lowlimit, highlimit, minlist));

                id = "max_" + param.Replace(" ", "_").ToLower() + "_id";
                title = param + " Max Dataset Histogram";
                chartlist.Add(HistogramChart.GetChartData(id, title, lowlimit, highlimit, maxlist));

                var datawithname = (List<KeyValuePair<string, string>>)rawdata[4];
                var sourcedata = DownLoadCPKSourceData(minlist, maxlist, param, cpkmindata, cpkmaxdata
                    , datafrom, mean, stddev, isnormal, realcpk, dppm, datawithname);

                cpkdatalist.Add(
                    new
                    {
                        param = param,
                        datafrom = datafrom,
                        mean = Math.Round(mean, 5),
                        stddev = Math.Round(stddev, 5),
                        isnormal = isnormal,
                        realcpk = realcpk,
                        dppm = dppm,
                        probmin = cpkmindata.IsNormalProbability,
                        probmax = cpkmaxdata.IsNormalProbability,
                        gcpkmin = cpkmindata.Cpk_ca,
                        gcpkmax = cpkmaxdata.Cpk_ca,
                        rcpkmin = cpkmindata.Cpk_robust,
                        rcpkmax = cpkmaxdata.Cpk_robust,
                        meanmin = Math.Round(cpkmindata.Mean, 5),
                        meanmax = Math.Round(cpkmaxdata.Mean, 5),
                        stddevmin = Math.Round(cpkmindata.Stdev, 5),
                        stddevmax = Math.Round(cpkmaxdata.Stdev, 5),
                        rdppmmin = cpkmindata.DPPM,
                        rdppmmax = cpkmaxdata.DPPM,
                        cpkmin = cpkmindata.Cpk_final,
                        cpkmax = cpkmaxdata.Cpk_final,
                        sourcedata = sourcedata
                    });
            }


            var allret = new JsonResult();
            allret.Data = new
            {
                success = true,
                cpkdatalist = cpkdatalist,
                chartlist = chartlist
            };

            return allret;
        }

        public JsonResult QueryCPK()
        {
            var pass = Request.Form["pass"];
            var database = Request.Form["database"];
            bool onlypass = false;
            if (string.Compare(pass, "YES", true) == 0)
            { onlypass = true; }

            var rawdata = new List<object>();
            if (string.Compare(database, "MES", true) == 0)
            {
                return MESCPK(database, onlypass);
            }
            else
            {
                return ATECPK(database, onlypass);
            }
        }

        public ActionResult CPKTool()
        {
            return View();
        }

        public JsonResult CPKToolData()
        {
            var marks = Request.Form["marks"];
            List<string> strdatalist = (List<string>)Newtonsoft.Json.JsonConvert.DeserializeObject(marks, (new List<string>()).GetType());
            var vallist = new List<Double>();
            foreach (var item in strdatalist)
            {
                try {
                    vallist.Add(Convert.ToDouble(item));
                } catch(Exception ex) { }
            }
            var filterlist = MESUtility.FilterStrangeValue(vallist);
            if (filterlist.Count <= 50)
            {
                var failret = new JsonResult();
                failret.Data = new {
                    success = false,
                    msg = "raw data count is too small to get a correct CPK"
                };
                return failret;
            }

            var lowlimit = Request.Form["lowlimit"];
            var highlimit = Request.Form["highlimit"];

            var cpk = CPKData.GetCpk(filterlist,lowlimit,highlimit)[0];

            var chartlist = new List<object>();
            var cpklist = new List<object>();

            var id = "raw_data_id";
            var title = "Dataset Histogram";
            chartlist.Add(HistogramChart.GetChartData(id, title, lowlimit, highlimit, filterlist));

            cpklist.Add(new
            {
                isnormal = cpk.IsNormalStr,
                mean = Math.Round(cpk.Mean,5),
                stddev = Math.Round(cpk.Stdev,5),
                gcpk = cpk.Cpk_ca,
                rcpk = cpk.Cpk_robust,
                dppm = cpk.DPPM
            });

            var ret = new JsonResult();
            ret.Data = new
            {
                success = true,
                cpklist = cpklist,
                chartlist = chartlist
            };
            return ret;
        }

        public ActionResult TestDataDistribution()
        {
            var yesnolist = new List<string>();
            yesnolist.Add("NO");
            yesnolist.Add("YES");
            ViewBag.allpasslist = CreateSelectList(yesnolist, "");
            return View();
        }

        public JsonResult QueryTestDataDist()
        {
            var pass = Request.Form["pass"];
            bool onlypass = false;
            if (string.Compare(pass, "YES", true) == 0)
            { onlypass = true; }

            var pj = Request.Form["pj"];
            var mestab = Request.Form["mestab"].ToUpper().Trim();
            if (mestab.IndexOf("DC_") == 0)
            {
                mestab = mestab.Substring(3);
            }
            var paramliststr = Request.Form["param"].Trim();
            var startdate = Request.Form["startdate"];
            var enddate = Request.Form["enddate"];
            var pnlist = Request.Form["pnlist"];


            var rawdata = MESUtility.GetTestData2(pnlist, mestab, paramliststr, startdate, enddate, onlypass);
            if (rawdata.Count == 0)
            {
                var ret = new JsonResult();
                ret.Data = new
                {
                    msg = "Fail to get test data, please check your query condition!",
                    success = false
                };
                return ret;
            }

            CPKCache.UpdateCPKParams(pj, mestab, pnlist, paramliststr, "", "", "", "MES");

            var chartdata = new List<object>();
            var labels = new List<object>();

            var plotline = new List<object>();
            var colorlist = new string[] { "#4b96f5", "#01ff00", "#e66400", "#910000", "#1aadce","#492970", "#f28f43", "#2f7ed8", "#0d233a", "#8bbc21", "#77a1e5", "#c42525", "#a6c96a"}.ToList();
            var idx = 0;
            foreach (var kv in rawdata)
            {
                chartdata.Add(HistogramChart.GetChartData(kv.Key, kv.Value,colorlist[idx]));

                chartdata.Add(NormalFit.GetChartData(kv.Key + " Fit", kv.Value, colorlist[idx]));

                var mean = Statistics.Mean(kv.Value);
                var stddev = Statistics.StandardDeviation(kv.Value);

                //var plotlabel = new {
                //    text = kv.Key + "_Mean"
                //};
                //plotline.Add(new {
                //    value= mean,
                //    color = colorlist[idx],
                //    dashStyle = "shortdash",
                //    width = 3,
                //    label = plotlabel
                //});

                labels.Add(
                    new {
                            html = "<span><font color='" + colorlist[idx] + "'>"+kv.Key+" Mean:" + Math.Round(mean, 3) + " StdDev:" + Math.Round(stddev, 3) + "</font></span>",
                            style = new {
                                left = "20px",
                                top = (15*(idx+1))+"px"
                            }
                        }
                    );

                idx = idx + 1;
            }
            
            var id = paramliststr.Replace(" ", "_").Replace(";", "_").ToLower() + "_id";
            var title = paramliststr.Replace(" ", "_").Replace(";", "_") + " Corners Distribution";
            var outdata = new
            {
                id = id,
                title = title,
                chartdata = chartdata,
                plotline = plotline,
                labels = labels
            };

            var ret1 = new JsonResult();
            ret1.Data = new
            {
                success = true,
                outdata = outdata
            };
            return ret1;
        }


    }
}