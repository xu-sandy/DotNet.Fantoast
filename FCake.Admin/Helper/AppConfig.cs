using FCake.Bll;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace FCake.Admin.Helper
{
    public static class AppConfig
    {
        private static string _sysVerison = string.Empty;
        private static string _sysAllowAccessIPs = string.Empty;
        /// <summary>
        /// 后台管理允许访问的Ip地址信息[多个IP以逗号隔开,value为空为不限制访问(例子value="27.154.234.10,::1")]
        /// </summary>
        public static string SysAllowAccessIPs{
            get {
                if (string.IsNullOrEmpty(_sysAllowAccessIPs)) {
                    var allowIps = ConfigurationManager.AppSettings["sys_allowaccessips"];
                    if (allowIps != null)
                    {
                        _sysAllowAccessIPs = allowIps.ToString();
                    }
                }
                return _sysAllowAccessIPs;
            }
        } 
        public static string SysVersion {
            get {
                if (string.IsNullOrEmpty(_sysVerison)) { 
                    var commService = new CommonService();
                    _sysVerison = commService.GetVersion();
                    if (string.IsNullOrEmpty(_sysVerison))
                        _sysVerison = "--"; 
                }
                return _sysVerison;
            }
        }
    }
}