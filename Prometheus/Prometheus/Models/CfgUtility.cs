﻿using System;
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
                    if (!ret.ContainsKey(kvpair[0].Trim()))
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
            var lines = System.IO.File.ReadAllLines(ctrl.Server.MapPath("~/Scripts/StandardPJList.txt"));
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



