using FCake.Admin.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FCake.Admin
{
    public class UserAuthAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
            
            if (filterContext.HttpContext != null) {
                if (HttpContext.Current.User.Identity.IsAuthenticated) {
                    if (FCake.Admin.Helper.UserCache.CurrentUser == null)
                    {
                        filterContext.Result = new RedirectResult("~/Account/Login");
                    }
                }
                else
                {
                    filterContext.Result = new RedirectResult("~/Account/Login");
                }
            }
        }
    }

    public class AccessAuthAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            string allowIps = AppConfig.SysAllowAccessIPs;
            var addressIP = System.Web.HttpContext.Current.Request.UserHostAddress;
            if (!string.IsNullOrEmpty(allowIps))
            {
                var ips = allowIps.Split(',');
                var allow = ips.Any(s => s == addressIP);
                if(allow)
                    base.OnAuthorization(filterContext);
                else
                    filterContext.Result = new RedirectResult("http://www.fancake.cn");
            }
        }
    }
}