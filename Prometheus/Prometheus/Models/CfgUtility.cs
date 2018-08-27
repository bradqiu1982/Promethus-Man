using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Runtime.InteropServices;

namespace Prometheus.Models
{
    public class NativeMethods : IDisposable
    {

        // obtains user token  

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]

        static extern bool LogonUser(string pszUsername, string pszDomain, string pszPassword,

            int dwLogonType, int dwLogonProvider, ref IntPtr phToken);



        // closes open handes returned by LogonUser  

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]

        extern static bool CloseHandle(IntPtr handle);
        [DllImport("Advapi32.DLL")]
        static extern bool ImpersonateLoggedOnUser(IntPtr hToken);
        [DllImport("Advapi32.DLL")]
        static extern bool RevertToSelf();
        const int LOGON32_PROVIDER_DEFAULT = 0;
        const int LOGON32_LOGON_NEWCREDENTIALS = 2;

        private bool disposed;

        public NativeMethods(string sUsername, string sDomain, string sPassword)
        {

            // initialize tokens  

            IntPtr pExistingTokenHandle = new IntPtr(0);
            IntPtr pDuplicateTokenHandle = new IntPtr(0);
            try
            {
                // get handle to token  
                bool bImpersonated = LogonUser(sUsername, sDomain, sPassword,

                    LOGON32_LOGON_NEWCREDENTIALS, LOGON32_PROVIDER_DEFAULT, ref pExistingTokenHandle);
                if (true == bImpersonated)
                {

                    if (!ImpersonateLoggedOnUser(pExistingTokenHandle))
                    {
                        int nErrorCode = Marshal.GetLastWin32Error();
                        throw new Exception("ImpersonateLoggedOnUser error;Code=" + nErrorCode);
                    }
                }
                else
                {
                    int nErrorCode = Marshal.GetLastWin32Error();
                    throw new Exception("LogonUser error;Code=" + nErrorCode);
                }

            }

            finally
            {
                // close handle(s)  
                if (pExistingTokenHandle != IntPtr.Zero)
                    CloseHandle(pExistingTokenHandle);
                if (pDuplicateTokenHandle != IntPtr.Zero)
                    CloseHandle(pDuplicateTokenHandle);
            }

        }

        protected virtual void Dispose(bool disposing)
        {

            if (!disposed)
            {
                RevertToSelf();
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }

    public class LIMITVAL
    {
        public LIMITVAL(double l, double h)
        {
            low = l;
            high = h;
        }

        public double low { set; get; }
        public double high { set; get; }
    }

    public class CfgUtility
    {
        public static Dictionary<string, string> GetSysConfig(Controller ctrl)
        {
            var lines = System.IO.File.ReadAllLines(ctrl.Server.MapPath("~/Scripts/PromethusCfg.txt"));
            var ret = new Dictionary<string, string>();
            foreach (var line in lines)
            {
                if (line.Contains("##"))
                {
                    continue;
                }

                if (line.Contains(":::"))
                {
                    var kvpair = line.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
                    if (!ret.ContainsKey(kvpair[0].Trim()) && kvpair.Length > 1)
                    {
                        ret.Add(kvpair[0].Trim(), kvpair[1].Trim());
                    }
                }//end if
            }//end foreach
            return ret;
        }

        public static Dictionary<string, string> GetBurnInErrorConfig(Controller ctrl)
        {
            var lines = System.IO.File.ReadAllLines(ctrl.Server.MapPath("~/Scripts/burnin-errorcode.cfg"));
            var ret = new Dictionary<string, string>();
            foreach (var line in lines)
            {
                if (line.Contains("##"))
                {
                    continue;
                }

                if (line.Contains(":::"))
                {
                    var kvpair = line.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
                    if (!ret.ContainsKey(kvpair[0].Trim()))
                    {
                        ret.Add(kvpair[0].Trim(), kvpair[1].Trim());
                    }
                }//end if
            }//end foreach
            return ret;
        }

        public static Dictionary<string, string> GetShipCustConfig(Controller ctrl, string producttype)
        {
            var lines = System.IO.File.ReadAllLines(ctrl.Server.MapPath("~/Scripts/ShipCustomer.cfg"));
            var ret = new Dictionary<string, string>();
            foreach (var line in lines)
            {
                if (line.Contains("##"))
                {
                    continue;
                }

                if (line.Contains(":::"))
                {
                    var kvpair = line.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
                    if (kvpair[0].Contains(producttype + "_"))
                    {
                        var key = kvpair[0].Replace(producttype + "_", "").ToUpper();
                        var val = kvpair[1].Trim().ToUpper();
                        if (!ret.ContainsKey(key))
                        { ret.Add(key,val); }
                    }
                }//end if
            }//end foreach
            return ret;
        }

        public static Dictionary<string, string> GetAllCustConfig(Controller ctrl)
        {
            var lines = System.IO.File.ReadAllLines(ctrl.Server.MapPath("~/Scripts/ShipCustomer.cfg"));
            var ret = new Dictionary<string, string>();
            foreach (var line in lines)
            {
                if (line.Contains("##"))
                {
                    continue;
                }

                if (line.Contains(":::"))
                {
                    var kvpair = line.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
                    var producttype = kvpair[0].Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries)[0];

                    var key = kvpair[0].Replace(producttype + "_", "").ToUpper();
                    var val = kvpair[1].Trim().ToUpper();
                    if (!ret.ContainsKey(key))
                    { ret.Add(key, val); }

                }//end if
            }//end foreach
            return ret;
        }

        public static Dictionary<string, string> GetStandardPJList(Controller ctrl)
        {
            var ret = new Dictionary<string, string>();
            var sql = "select distinct ProjectName from [NebulaTrace].[dbo].[ProjectVM]";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                try {
                    var pjname = Convert.ToString(line[0]);
                    if (pjname.Contains("/"))
                    {
                        var idx = pjname.IndexOf("/") + 1;
                        pjname = pjname.Substring(idx);                        
                    }

                    if (!ret.ContainsKey(pjname))
                    {
                        ret.Add(pjname, pjname);
                    }

                } catch (Exception ex) { }
            }

            return ret;

            //var lines = System.IO.File.ReadAllLines(ctrl.Server.MapPath("~/Scripts/StandardPJList.txt"));
            //var ret = new Dictionary<string, string>();
            //foreach (var line in lines)
            //{
            //    if (line.Contains("##"))
            //    {
            //        continue;
            //    }

            //    if (line.Contains(":::"))
            //    {
            //        var kvpair = line.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
            //        if (!ret.ContainsKey(kvpair[0].Trim()))
            //        {
            //            ret.Add(kvpair[0].Trim(), kvpair[1].Trim());
            //        }
            //    }//end if
            //}//end foreach
            //return ret;
        }

        public static Dictionary<string, string> GetNPIMachine(Controller ctrl)
        {
            var lines = System.IO.File.ReadAllLines(ctrl.Server.MapPath("~/Scripts/npidepartmentmachine.cfg"));
            var ret = new Dictionary<string, string>();
            foreach (var line in lines)
            {
                if (line.Contains("##"))
                {
                    continue;
                }

                if (line.Contains(":::"))
                {
                    var kvpair = line.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
                    if (!ret.ContainsKey(kvpair[0].Trim()))
                    {
                        ret.Add(kvpair[0].Trim().ToUpper(), kvpair[1].Trim());
                    }
                }//end if
            }//end foreach
            return ret;
        }

        public static Dictionary<string,Dictionary<string, LIMITVAL>> GetATETestTimeLimit(Controller ctrl)
        {
            var lines = System.IO.File.ReadAllLines(ctrl.Server.MapPath("~/Scripts/ATETestTimeBaseLine.cfg"));
            var tempdict = new Dictionary<string, string>();
            foreach (var line in lines)
            {
                if (line.Contains("##"))
                {
                    continue;
                }

                if (line.Contains(":::"))
                {
                    var kvpair = line.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
                    if (!tempdict.ContainsKey(kvpair[0].Trim()))
                    {
                        tempdict.Add(kvpair[0].Trim().ToUpper(), kvpair[1].Trim());
                    }
                }//end if
            }//end foreach

            var ret = new Dictionary<string, Dictionary<string, LIMITVAL>>();
            var sectionlist = tempdict["SECTION"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (var mdt in sectionlist)
            {
                if (tempdict.ContainsKey(mdt))
                {
                    var tempkey = tempdict[mdt];
                    var limitdict = new Dictionary<string, LIMITVAL>();

                    foreach (var line in lines)
                    {
                        if (line.Contains(tempkey + ":::"))
                        {
                            var limitlist = line.Replace(tempkey + ":::", "").Split(new string[] { "#" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                            if (limitlist.Count == 3)
                            {
                                try
                                {
                                    limitdict.Add(limitlist[0], new LIMITVAL(Convert.ToDouble(limitlist[1]), Convert.ToDouble(limitlist[2])));
                                }
                                catch (Exception ex) { }
                            }//end if
                        }//end contains
                    }//end foreach
                    if (!ret.ContainsKey(mdt))
                    {
                        ret.Add(mdt, limitdict);
                    }
                }//end if mdt
            }//end foreach

            return ret;
        }

    }

    public class SeverHtmlDecode
        {
            public static string Decode(Controller ctrl, string src)
            {
                var ret = ctrl.Server.HtmlDecode(src).Replace("border=\"0\"", "border=\"2\"");
                ret = System.Text.RegularExpressions.Regex.Replace(ret, "<div.*?>", string.Empty).Trim();
                ret = ret.Replace("</div>", "");
                return ret;
            }
        }

}



