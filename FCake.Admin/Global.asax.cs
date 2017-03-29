using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using FCake.Admin.Models;
using FCake.Domain;
using FCake.Bll;

namespace FCake.Admin
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

            log4net.Config.XmlConfigurator.Configure(new FileInfo(Server.MapPath("~/log4net.config")));

            //初始化全局配置缓存信息
            SysConfigsCache.Register();

            //权限缓存
            using (EFDbContext context = new EFDbContext())
            {
                PermissionCache.time = DateTime.Now;
                PermissionCache.ResetPermissionCache(
                    context.Permissions.ToList(),
                    context.UserRoles.ToList(),
                    context.RolePermissions.ToList());
            }
        }
    }
}