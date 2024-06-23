// Copyright 2010 Nejat Dilek  <imruon@gmail.com>
// Licensed under the terms of the GNU GPL, version 2
// http://www.gnu.org/licenses/old-licenses/gpl-2.0.txt
using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Net;

namespace WiiGSC
{
    public class AppUtils
    {
        public static WebProxy GetProxyIfRequired()
        {
            string useProxy = ConfigurationSettings.AppSettings["UseProxy"];
            if (useProxy == "on")
            {
                WebProxy myProxy = new WebProxy();
                string proxyAddress = ConfigurationSettings.AppSettings["ProxyServer"];
                string proxyUser = ConfigurationSettings.AppSettings["ProxyUser"];
                string proxyPass = ConfigurationSettings.AppSettings["ProxyPass"];
                Uri newUri = new Uri(proxyAddress);
                myProxy.Address = newUri;
                myProxy.Credentials = new NetworkCredential(proxyUser, proxyPass);
                return myProxy;
            }
            return null;
        }

    }
}
