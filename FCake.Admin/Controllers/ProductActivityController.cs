using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FCake.Admin.Models;
using System.Data.Entity;
using FCake.Domain.Entities;
using FCake.Bll;
using FCake.Core.MvcCommon;
using FCake.Core.Common;
using FCake.Admin.Helper;
using FCake.Domain.Common;
using FCake.Bll.Services;

namespace FCake.Admin.Controllers
{
    public class ProductActivityController : BaseController
    {
        //
        // GET: /ProductActivity/
        private readonly ProductActivityService _productActivityService = new ProductActivityService();

        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [CheckPermission(controlName = "ProductActivity", actionName = "Index", permissionCode = "view")]
        public ActionResult GetProductActivityPaging(int page = 1, int rows = 20)
        {
            int totalCount = 0;
            var pageInfo = new PageInfo() { Page = page, Rows = rows };
            var result = _productActivityService.GetProductActivityPaging(out totalCount, pageInfo);
            GridJsonModel djson = new GridJsonModel(totalCount, result);
            return Json(djson);
        }
        [CheckPermission(controlName = "ProductActivity", actionName = "Index", permissionCode = "edit")]
        public ActionResult Edit(string id)
        {
            ProductActivity productActivity = new ProductActivity();
            if (!string.IsNullOrEmpty(id))
            {
                var pa = _productActivityService.Find<ProductActivity>(id);
                if (pa != null)
                    productActivity = pa;
            }
            return View(productActivity);
        }


        /// <summary>
        /// 根据产品名称和类型查找产品
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "ProductActivity", actionName = "Index", permissionCode = "view")]
        public ActionResult SearchProducts(string name, string type, int page = 1, int rows = 10)
        {
            var data = new ProductService().SearchProductByPhoneOrder(name, null, type, page, rows);
            return Json(data);
        }
        /// <summary>
        /// 根据产品Id 获得其所有子产品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [CheckPermission(isRelease = true)]
        public ActionResult GetSubProducts(string id)
        {
            //返回数据有ID Price Size
            Product p = new ProductService().GetProduct(id, false);
            return View(p);
        }
        /// <summary>
        /// 创建产品活动
        /// </summary>
        /// <param name="productActivity"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "ProductActivity", actionName = "Index", permissionCode = "add")]
        [HttpPost]
        public ActionResult CreateActivity(ProductActivity productActivity)
        {
            var currentUser = UserCache.CurrentUser.Id;
            var result = _productActivityService.CreateProductActivity(productActivity, currentUser);
            return Json(result);
        }
        /// <summary>
        /// 未发布活动发布
        /// </summary>
        /// <param name="activityId"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "ProductActivity", actionName = "Index", permissionCode = "add")]
        [HttpPost]
        public ActionResult PushActicvity(string activityId)
        {
            OpResult result = new OpResult();
            var currentUser = UserCache.CurrentUser.Id;
            result = _productActivityService.PushProductActivity(activityId, currentUser);
            return Json(result);
        }

        /// <summary>
        /// 删除产品活动数据
        /// </summary>
        /// <param name="activityId"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "ProductActivity", actionName = "Index", permissionCode = "edit")]
        [HttpPost]
        public ActionResult DeleteActivity(string activityId)
        {
            var result = _productActivityService.DeleteActivity(activityId);
            return Json(result);
        }

        /// <summary>
        /// 获取活动用的子产品信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        [HttpPost]
        [CheckPermission(isRelease = true)]
        public ActionResult GetSubProductsById(string id)
        {
            //返回数据有ID Price Size
            var result = new ProductService().GetSubProducts(id);
            return Json(result);
        }
        /// <summary>
        /// 编辑活动绑定产品
        /// </summary>
        /// <param name="activityId"></param>
        /// <returns></returns>
        [CheckPermission(isRelease = true)]
        public ActionResult GetActivityProByProId(string activityId)
        {
            var data = _productActivityService.GetActivityProInfoByActId(activityId);
            return Json(new { rows = data });
        }
    }
}
