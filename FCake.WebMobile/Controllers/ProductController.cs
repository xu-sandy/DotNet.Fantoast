using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FCake.Domain;
using FCake.Bll;
using FCake.Domain.WebModels;

namespace FCake.WebMobile.Controllers
{
    public class ProductController : Controller
    {
        //
        // GET: /Product/
        ProductService _productService = new ProductService();
        #region 产品列表
        /// <summary>
        /// 产品列表
        /// </summary>
        /// <param name="type"></param>
        /// <param name="themes"></param>
        /// <returns></returns>
        public ViewResult Index(string type, string themes, int pageSize = 100, int pageIndex = 1)
        {
            var data = this._productService.GetProducts(type, themes, pageSize, pageIndex);
            var model = new ProductListModel();
            model.Type = type;
            model.Themes = themes;
            model.VProductModels = data;
            return View(model);
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
            var model = new ProductListModel() { VProductModels = data };
            return View(model);
        }
        #endregion

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
        /// <summary>
        /// 获取一个产品信息，返回实体product
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public FCake.Domain.Entities.Product GetProductById(string id)
        {
            var data = _productService.GetProductById(id);
            if (data != null)
            {
                return data;
            }
            return null;
        }
    }
}
