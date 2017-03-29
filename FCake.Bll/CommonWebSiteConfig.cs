using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace FCake.Bll
{
    public static partial class CommonWebSiteConfig
    {
        private static string _sysManageUrl = string.Empty;
        private static string _sysMobileUrl = string.Empty;

        /// <summary>
        /// 管理平台URL
        /// </summary>
        public static string SysManageUrl
        {
            get
            {
                if (string.IsNullOrEmpty(_sysManageUrl))
                {
                    var value = ConfigurationManager.AppSettings["BackendUrl"];
                    if (value != null)
                    {
                        _sysManageUrl = value.ToString();
                    }
                }
                return _sysManageUrl;
            }
        } 
        /// <summary>
        /// 获取管理平台URL
        /// </summary>
        /// <param name="helper"></param>
        /// <returns></returns>
        public static string GetSysManageUrl(this HtmlHelper helper)
        {
            return SysManageUrl;
        }
        /// <summary>
        /// 移动端访问地址
        /// </summary>
        public static string SysMobileUrl
        {
            get
            {
                if (string.IsNullOrEmpty(_sysMobileUrl))
                {
                    var address = ConfigurationManager.AppSettings["sys_mobiledomainaddress"];
                    _sysMobileUrl = string.IsNullOrEmpty(address) ? "http://m.fancake.cn" : address;
                }
                return _sysMobileUrl;
            }
        }
    }
}
