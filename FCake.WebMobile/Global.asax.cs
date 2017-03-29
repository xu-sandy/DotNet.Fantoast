using FCake.Bll;
using FCake.Bll.MemberAuth;
using FCake.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace FCake.WebMobile
{
    // 注意: 有关启用 IIS6 或 IIS7 经典模式的说明，
    // 请访问 http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //初始化全局配置缓存信息
            SysConfigsCache.Register();
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            var linkID = CookieHelper.GetCookieValue(Constants.UNIQUE_VISITOR_ID);
            if (linkID == string.Empty)
                CookieHelper.SetCookie(Constants.UNIQUE_VISITOR_ID, Guid.NewGuid().ToString("N"));
        }
    }
}