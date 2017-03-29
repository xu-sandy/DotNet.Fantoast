using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace FCake.Web
{
    public class AppConfig
    {
        private static string _sysMobileDomainAddress = string.Empty;
        public static string SysMobileDomainAddress
        {
            get
            {
                if (string.IsNullOrEmpty(_sysMobileDomainAddress))
                {
                    var address = ConfigurationManager.AppSettings["sys_mobiledomainaddress"];
                    _sysMobileDomainAddress = string.IsNullOrEmpty(address) ? "http://m.fancake.cn" : address;
                }
                return _sysMobileDomainAddress;
            }
        }
    }
}