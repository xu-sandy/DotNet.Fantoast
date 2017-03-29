using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace FCake.Web
{
    public class DeviceFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var httpContext = System.Web.HttpContext.Current;
            var myBrowserCaps = httpContext.Request.Browser;
            var isMobile = myBrowserCaps.IsMobileDevice ? 1 : 0;
            if (isMobile == 1)
            {
                httpContext.Response.Redirect(AppConfig.SysMobileDomainAddress);
            }
            base.OnActionExecuting(filterContext);
        }
    }
}