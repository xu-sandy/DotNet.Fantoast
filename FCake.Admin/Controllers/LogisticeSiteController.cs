using FCake.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FCake.Domain.Entities;
using FCake.Bll;
using FCake.Admin.Models;
using FCake.Core.MvcCommon;

namespace FCake.Admin.Controllers
{
    public class LogisticeSiteController : BaseController
    {
        //
        // GET: /LogisticeSite/
        LogisticsSiteService service = new LogisticsSiteService();
        [CheckPermission(controlName = "LogisticeSite", actionName = "LogisticsSiteManager", permissionCode = "view")]
        public ActionResult Index()
        {
            return View();
        }

        #region 自提站点管理
        /// <summary>
        /// 自提站点管理-列表
        /// </summary>
        /// <returns></returns>
        public ActionResult LogisticsSiteManager()
        {
            return View();
        }
        /// <summary>
        /// 自提站点管理-获取列表数据信息
        /// </summary>
        /// <param name="site">站点信息</param>
        /// <returns></returns>
        [CheckPermission(controlName = "LogisticeSite", actionName = "LogisticsSiteManager", permissionCode = "view")]
        public ActionResult GetAllLogisticsSite(LogisticsSite site, int page = 1, int rows = 10)
        {
            if (site != null)
            {
                int count = 0;
                var data = service.GetLogisticsSites(site, out count, page, rows);
                if (count != 0)
                {
                    return Json(new { total = count, rows = data });
                }
            }
            return View();
        }
        #endregion
        /// <summary>
        /// 配送站点新增与编辑
        /// </summary>
        /// <returns></returns>
        public ViewResult LogisticsSiteFrom(string id = "")
        {
            var site = service.GetLogisticsSiteById(id);
            return View(site);
        }
        /// <summary>
        /// 根据区域获取所有区域自提站点
        /// </summary>
        /// <param name="area">区域名称</param>
        /// <returns></returns>
        [CheckPermission(controlName = "LogisticeSite", actionName = "LogisticsSiteManager", permissionCode = "view")]
        public ActionResult GetLogisticeSiteByArea(string city = "")
        {
            var data = new List<LogisticsSite>();
            if (city != "")
            {
                data = service.GetLogisticeSiteByArea(city);
            }
            return Json(data.ToList());
        }
        /// <summary>
        /// 根据ID取得配送站点的信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "LogisticeSite", actionName = "LogisticsSiteManager", permissionCode = "view")]
        public ActionResult GetLogisticeSiteById(string id = "")
        {
            var data = service.GetLogisticsSiteById(id);
            return Json(data);
        }
        [CheckPermission(controlName = "LogisticeSite", actionName = "LogisticsSiteManager", permissionCode = "edit")]
        public ActionResult SaveLogistics(LogisticsSite site)
        {
            OpResult result = new OpResult();

            if (site.SiteName == null || site.SiteName.Trim() == "" || site.SiteAddress == null || site.SiteAddress.Trim() == "")
            {
                result.Successed = false;
                result.Message = "站点信息不完整，保存失败！";
                return Json(result);
            }

            result = service.SaveLogisticsSite(site);
            return Json(result);
        }

        [CheckPermission(controlName = "LogisticeSite", actionName = "LogisticsSiteManager", permissionCode = "delete")]
        public ActionResult DeleteLogisSiteById(string id = "")
        {
            OpResult result = new OpResult();
            result = service.DeleteLogisSiteById(id);
            return Json(result);

        }

    }
}
