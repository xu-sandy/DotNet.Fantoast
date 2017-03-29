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
using System.Text.RegularExpressions;
using FCake.Admin.Helper;
using FCake.Core.Common;
using FCake.Domain.Common;
using System.IO;

namespace FCake.Admin.Controllers
{
    public class ProductController : BaseController
    {
        ProductService svc = new ProductService();
        int totalCount = 0;
        PermissionService psv = new PermissionService();

        //产品页
        public ActionResult Index()
        {
            var list = psv.GetPermissionCodes("Product", "Index", UserCache.CurrentUser.Id);
            var dicName = "Status";
            if (list.Contains("review"))
            {
                dicName = "StatusReview";
            }
            ViewBag.dicName = dicName;
            return View();
        }

        /// <summary>
        /// 产品查询
        /// </summary>
        /// <param name="product"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        [HttpPost]
        [CheckPermission(isRelease = true, controlName = "Product", actionName = "Index", permissionCode = "view")]
        public ActionResult QueryData(Product product, int page = 1, int rows = 20)
        {
            var pageInfo = new PageInfo() { Page = page, Rows = rows };
            var result = svc.FilterProduct(product, UserCache.CurrentUser.Id, out totalCount, pageInfo);
            GridJsonModel djson = new GridJsonModel(totalCount, result);
            return Json(djson);
        }

        /// <summary>
        /// 产品网格页点击详细页面数据
        /// </summary>
        /// <param name="Id">ProductId</param>
        /// <returns></returns>
        [CheckPermission(controlName = "Product", actionName = "Index", permissionCode = "view")]
        public ActionResult DetailView(string Id)
        {
            var details = new ProductDetail();
            if (!Id.IsNullOrTrimEmpty())
            {
                var product = svc.GetProductAll(Id);
                if (product != null)
                {
                    details.Name = product.Name;
                    details.Taste = product.Taste;
                    details.Brief = product.Brief;
                    details.minUrl = product.BaseFile != null ? product.BaseFile.Url : null;
                    details.SubProduct = product.SubProducts.Where(sp => sp.IsDeleted != 1).ToList();
                }
            }
            return View(details);
        }
        [CheckPermission(controlName = "Product", actionName = "Index", permissionCode = "review")]
        public ActionResult ChangeSaleStatus(string id, int status)
        {
            var result = svc.ChangeSaleStatus(id, status, UserCache.CurrentUser.Id);
            return Json(result);
        }
        [CheckPermission(controlName = "Product", actionName = "Index", permissionCode = "view")]
        public ActionResult ReChangeSaleStatus(string id, int status)
        {
            var result = svc.ReChangeSaleStatus(id, status, UserCache.CurrentUser.Id);
            return Json(result);
        }
        /// <summary>
        /// 审核数据
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Product", actionName = "Index", permissionCode = "review")]
        public ActionResult ReviewStatus(string id, int status)
        {
            var result = svc.ReviewStatus(id, status, UserCache.CurrentUser.Id);
            return Json(result);
        }
        /// <summary>
        /// 提交审核
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Product", actionName = "Index", permissionCode = "view")]
        public ActionResult SubmitStatus(string id, int status)
        {
            var result = svc.ReviewStatus(id, status, UserCache.CurrentUser.Id);
            return Json(result);
        }
        /// <summary>
        /// 产品编辑
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Product", actionName = "Index", permissionCode = "add")]
        public ActionResult ProductEdit(string id)
        {
            var product = svc.GetProduct(id);
            var model = new ProductEditVM()
            {
                Product = product
            };
            Regex reg = new Regex(@"^_");
            List<BaseFile> proImgs = null;
            if (!string.IsNullOrEmpty(product.Id) && !reg.IsMatch(product.Id))
            {
                var bfService = new BaseFileService();
                proImgs = bfService.GetBaseFiles(product.Id);
            }

            ViewBag.ProductImgs = proImgs;
            return View(model);
        }

        /// <summary>
        /// 保存产品
        /// </summary>
        /// <param name="prduct"></param>
        /// <param name="subproduct"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        [CheckPermission(controlName = "Product", actionName = "Index", permissionCode = "add")]
        public ActionResult SaveProduct(Product prduct, string subproduct)
        {
            var result = svc.SaveProduct(prduct, Helper.UserCache.CurrentUser.Id, subproduct);
            return Json(result);
        }

        /// <summary>
        /// 删除产品图片
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [CheckPermission(controlName = "Product", actionName = "Index", permissionCode = "edit")]
        public ActionResult DeleteImg(string id)
        {
            svc.DeleteImg(id);
            return Json("");
        }

        /// <summary>
        /// 获取子产品(规格、价格)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        [HttpPost]
        [CheckPermission(controlName = "Product", actionName = "Index", permissionCode = "add")]
        public ActionResult GetSubProduct(string id, int page = 1, int rows = 20)
        {
            var pageInfo = new PageInfo() { Page = page, Rows = rows };
            var result = svc.GetSubProduct(id, out totalCount, pageInfo);
            return Json(new { total = totalCount, rows = result });
        }

        /// <summary>
        /// 由订单号获取订单中的所有产品
        /// </summary>
        /// <param name="orderid"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Product", actionName = "Index", permissionCode = "view")]
        public ActionResult GetAllProductByOrderid(string orderid)
        {
            var result = svc.GetAllProductByOrderid(orderid);
            return Json(result);
        }
        #region  产品编辑页上传图片
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CheckPermission(controlName = "Product", actionName = "Index", permissionCode = "view")]
        public ActionResult UploadImgs(string productId)
        {
            OpResult result = null;
            productId = ReplaceProductId(productId);
            var files = Request.Files;
            //var maxFile = 1024 * 1024;
            var dir = "/Files/" + DateTime.Now.ToString("yyyyMMdd") + "/";
            var dirPath = Server.MapPath(dir);
            if (Directory.Exists(dirPath) == false)
                Directory.CreateDirectory(dirPath);
            if (files.Count > 0)
            {
                var entities = new List<BaseFile>();
                var bfService = new BaseFileService();
                foreach (var fileKey in files.AllKeys)
                {
                    var index = fileKey.Split('_')[1].ToInt32();
                    var file = files[fileKey];
                    BaseFile entity = null;
                    if (file != null)
                    {
                        // Verify that the user selected a file
                        if (file != null && file.ContentLength > 0)
                        {
                            entity = new BaseFile();
                            // extract only the fielname
                            entity.OldName = Path.GetFileName(file.FileName);
                            entity.SuffixName = GetSuffix(entity.OldName);
                            entity.NewName = DataHelper.GetSystemID() + "." + entity.SuffixName;
                            entity.Url = dir + entity.NewName;
                            entity.Sorting = index;
                            string filePath = dirPath + entity.NewName;
                            var minFilePath = filePath + "_min" + ".jpg";
                            var middleFilePath = filePath + "_middle" + ".jpg";
                            // TODO: need to define destination
                            //var path = Path.Combine(Server.MapPath("~/Files"), fileName);
                            file.SaveAs(filePath);
                            PictureHelper.MakeThumbnail(filePath, minFilePath, 200, 200, "Cut");
                            PictureHelper.MakeThumbnail(filePath, middleFilePath, 280, 280, "Cut");
                        }
                        entities.Add(entity);
                    }
                }
                var result1 = bfService.SaveBaseFiles(entities, productId, UserCache.CurrentUser.Id);
                if (result1 != null)
                    result = OpResult.Success("上传成功", "", result1);
                else
                    result = OpResult.Fail("上传失败");
            }
            else
            {
                result = OpResult.Fail("没有可上传的文件");
            }
            return Json(result);
        }
        private string GetSuffix(string filename)
        {
            try
            {
                return filename.Substring(filename.LastIndexOf(".") + 1);

            }
            catch
            {
                return "";
            }
        }
        private string ReplaceProductId(string productId)
        {
            var result = productId;
            if (productId.Length > 0 && productId.Contains("_"))
                result = productId.Substring(1);
            return result;
        }
        #endregion


        /// <summary>
        /// 后台用户下单时刷新价格
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="subProductId"></param>
        /// <param name="orginPrice"></param>
        /// <returns></returns>
        [CheckPermission(isRelease = true)]
        public ActionResult GetSubProductPriceByCustomerId(string customerId, string subProductId, string ParentId, decimal orginPrice)
        {
            var product = new ProductService().GetProduct(ParentId, false);
            var result = new Bll.ProductService().GetLastProductPrice(subProductId, orginPrice, product.Type, true, customerId);
            return Json(result);
        }
        /// <summary>
        /// 批量更新产品价格
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="subProductId"></param>
        /// <param name="ParentId"></param>
        /// <param name="orginPrice"></param>
        /// <returns></returns>
        [CheckPermission(isRelease = true)]
        public ActionResult UpdateProductPrice(string customerId, string subProductId, string ParentId, string orginPrice)
        {
            if (customerId == null || subProductId == null || ParentId.Replace(",", "") == "" || subProductId.Replace(",", "") == "")
            {
                return Json("");
            }
            //集合产品信息
            List<string> subProducts = new List<string>();//所有子产品id
            List<string> parentIds = new List<string>();//所有子产品对应的父产品id
            List<decimal> orginPrices = new List<decimal>();//所有子产品的原价
            //判断产品是单个还是多个
            if (subProductId.Contains(","))
            {
                //多个产品
                var subIds = subProductId.Split(',');
                var pIds = ParentId.Split(',');
                var prices = orginPrice.Split(',');
                for (int i = 0; i < subIds.Count(); i++)
                {
                    subProducts.Add(subIds[i]);
                    parentIds.Add(pIds[i]);
                    orginPrices.Add(Convert.ToDecimal(prices[i]));
                }
            }
            else
            {
                //单个产品
                subProducts.Add(subProductId);
                parentIds.Add(ParentId);
                orginPrices.Add(Convert.ToDecimal(orginPrice));
            }
            //创建返回集合
            Dictionary<string, decimal> productPrice = new Dictionary<string, decimal>();
            //取产品当前价格 并加入到返回集合中
            for (int i = 0; i < subProducts.Count; i++)
            {
                var product = new ProductService().GetProduct(parentIds[i], false);
                var resultPrice = new Bll.ProductService().GetLastProductPrice(subProducts[i], orginPrices[i], product.Type, true, customerId);
                productPrice.Add(subProducts[i], resultPrice);
            }
            return Json(productPrice);
        }

        [HttpPost]
        public ActionResult DelProductImg(string baseFileId)
        {
            var result = svc.DelProductImg(baseFileId);
            return Json(result);
        }
    }
}
