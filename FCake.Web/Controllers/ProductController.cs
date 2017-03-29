using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FCake.Domain;
using FCake.Bll;
using FCake.Domain.Entities;
using System.Configuration;
using FCake.Web.Models;
using FCake.Domain.WebModels;

namespace FCake.Web.Controllers
{
    public class ProductController : BaseController
    {
        //
        // GET: /Product/
        private readonly ProductService _productService = new ProductService();

        #region 蛋糕列表
        /// <summary>
        /// 蛋糕列表
        /// </summary>
        /// <param name="type"></param>
        /// <param name="themes"></param>
        /// <returns></returns>
        public ViewResult Index(string type, string themes, int pageSize = 100, int pageIndex = 1)
        {
            var model = GetProductsByPage(type, themes, pageSize, pageIndex);

            ViewBag.themes = themes;
            ViewBag.type = type;
            return View(model);
        }
        /// <summary>
        /// 蛋糕列表分页数据
        /// </summary>
        /// <param name="type"></param>
        /// <param name="themes"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        [HttpPost]
        public PartialViewResult GetProductsPartialView(string type, string themes, int pageSize = 100, int pageIndex = 1)
        {
            var model = GetProductsByPage(type, themes, pageSize, pageIndex);
            return PartialView("_productInfo", model);
        }
        /// <summary>
        /// 获取蛋糕列表Model
        /// </summary>
        /// <param name="type"></param>
        /// <param name="themes"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        private ProductListModel GetProductsByPage(string type, string themes, int pageSize, int pageIndex)
        {
            var data = this._productService.GetProducts(type, themes, pageSize, pageIndex);
            var model = new ProductListModel()
            {
                Type = type,
                Themes = themes,
                VProductModels = data
            };
            return model;
        }
        #endregion

        #region 热卖产品
        /// <summary>
        /// 热卖产品
        /// </summary>
        /// <returns></returns>
        public ViewResult HotSale(int pageSize = 6, int pageIndex = 1)
        {
            var data = _productService.GetHotProducts(pageSize, pageIndex);
            var model = new ProductListModel();
            model.VProductModels = data;
            return View(model);
        }
        #endregion

        #region 产品详情页
        public ActionResult Detail(string id)
        {
            var data = _productService.GetProductById(id);
            if (data != null)
            {
                return View(data);
            }
            //找不到产品跳转到列表页
            return RedirectToAction("Index");
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public PartialViewResult GetShopCartHtml(string productId)
        {
            var data = _productService.GetProductById(productId);
            if (data != null)
            {
                return PartialView("_goShopCart", data);
            }
            return PartialView("_goShopCart", null);
        }

        /// <summary>
        /// 根据产品ID取得产品图片
        /// </summary>
        /// <param name="id">产品ID</param>
        /// <returns></returns>
        public ViewResult GetProductImgByProductId(string id)
        {
            var data = _productService.GetBaseFileByProductId(id);
            if (data != null)
            {
                return View(data);
            }
            return null;
        }
    }
}
