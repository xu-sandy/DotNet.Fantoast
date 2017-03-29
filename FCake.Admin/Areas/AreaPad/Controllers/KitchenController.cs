using FCake.Admin.Areas.AreaPad.Models;
using FCake.Admin.Controllers;
using FCake.Admin.Helper;
using FCake.Bll;
using FCake.Core.Common;
using FCake.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace FCake.Admin.Areas.AreaPad.Controllers
{
    public class KitchenController : BaseController
    {
        private KitchenService _kitchenService = new KitchenService();
        /// <summary>
        /// 厨房制作
        /// </summary>
        /// <param name="batchId">批次号</param>
        /// <param name="showType">展示方式 0为排产显示 1为订单显示</param>
        /// <returns></returns>
        public ActionResult Index(string batchNo, int showType = 0)
        {
            //显示方式
            ViewBag.showType = showType;
            //批次号
            ViewBag.batchNo = batchNo;
            //厨房数据
            var batchInfos = _kitchenService.GetInfoByBatchNo(batchNo);
            ViewBag.nextBatchNo = _kitchenService.GetNextBatchNo(batchNo);
            ViewBag.preBatchNo = _kitchenService.GetPreBatchNo(batchNo);
            return View(batchInfos);
        }
        [CheckPermission(isRelease=true)]
        [HttpPost]
        public ActionResult GetStatus(string id)
        {
            try
            {
                var batchInfos = _kitchenService.GetInfoByBatchNo(id);
                List<dynamic> html = new List<dynamic>();

                StringBuilder sb;

                #region head
                sb = new StringBuilder();
                if (batchInfos.Any(a => a.ProductStatus == FCake.Domain.Enums.OrderBatchMakeStatus.NotStart))
                {
                    sb.AppendFormat(@"<a class=""btn btn-info"" href=""javascript:;"" onclick=""BeginBatch('{0}',this); return false;"">开始</a> ", id);
                }
                if (batchInfos.Any(a => a.ProductStatus == FCake.Domain.Enums.OrderBatchMakeStatus.Making))
                {
                    sb.AppendFormat(@"<a class=""btn btn-danger"" href=""javascript:;"" onclick=""RescindBatch('{0}',this); return false;"">撤销</a> ", id);
                    sb.AppendFormat(@"<a class=""btn btn-success"" href=""javascript:;"" onclick=""EndBatch('{0}',this); return false;"">完成</a>", id);
                }
                html.Add(new { id = id, html = sb.ToString() });
                #endregion

                #region detail
                foreach (var x in batchInfos)
                {
                    sb = new StringBuilder();
                    sb.AppendFormat("({0}) ", EnumHelper.GetDescription(x.ProductStatus));
                    if (x.ProductStatus == OrderBatchMakeStatus.NotStart)
                    {
                        sb.AppendFormat(@"<a class=""btn btn-info"" href=""javascript:;"" onclick=""BeginProductMake('{0}','{1}',this);return false;"">开始</a> ",
                            x.KitchenMakeDetailId, Server.UrlEncode(x.ProductName));
                    }
                    else if (x.ProductStatus == OrderBatchMakeStatus.Making)
                    {
                        sb.AppendFormat(@"<a class=""btn btn-danger"" href=""javascript:;"" onclick=""RescindProductMake('{0}','{1}',this);return false;"">撤销</a> ",
                            x.KitchenMakeDetailId, Server.UrlEncode(x.ProductName));
                        sb.AppendFormat(@"<a class=""btn btn-success"" href=""javascript:;"" onclick=""EndProductMake('{0}','{1}',this);return false;"">完成</a>",
                            x.KitchenMakeDetailId, Server.UrlEncode(x.ProductName));
                    }

                    html.Add(new { id = x.KitchenMakeDetailId, html = sb.ToString() });
                }
                #endregion

                #region order
                foreach (var x in batchInfos.GroupBy(a => a.OrderNo))
                {
                    sb = new StringBuilder();
                    if (x.Any(a => a.ProductStatus != OrderBatchMakeStatus.Complete) == false)
                    {
                        sb.AppendFormat("({0}) ", EnumHelper.GetDescription(OrderBatchMakeStatus.Complete));
                    }
                    if (x.Any(a => a.ProductStatus == OrderBatchMakeStatus.Making))
                    {
                        sb.AppendFormat("({0}) ", EnumHelper.GetDescription(OrderBatchMakeStatus.Making));
                    }
                    if (x.Any(a => a.ProductStatus == OrderBatchMakeStatus.Making) == false
                        && x.Any(a => a.ProductStatus == OrderBatchMakeStatus.NotStart)
                        && x.Any(a => a.ProductStatus == OrderBatchMakeStatus.Complete))
                    {
                        sb.AppendFormat("({0}) ", EnumHelper.GetDescription(OrderBatchMakeStatus.Making));
                    }
                    if (x.Any(a => a.ProductStatus != OrderBatchMakeStatus.NotStart) == false)
                    {
                        sb.AppendFormat("({0}) ", EnumHelper.GetDescription(OrderBatchMakeStatus.NotStart));
                    }


                    if (x.Any(a => a.ProductStatus == OrderBatchMakeStatus.NotStart))
                    {
                        sb.AppendFormat(@"<a class=""btn btn-info"" href=""javascript:;"" onclick=""BeginProductMakeByOrderNo('{0}',{1},'{2}',this); return false;"">开始</a> ",
                            x.Key, x.Where(a => a.ProductStatus == FCake.Domain.Enums.OrderBatchMakeStatus.NotStart).Sum(a => a.Num), id);
                    }
                    if (x.Any(a => a.ProductStatus == OrderBatchMakeStatus.Making))
                    {
                        sb.AppendFormat(@"<a class=""btn btn-danger"" href=""javascript:;"" onclick=""RescindProductMakeByOrderNo('{0}',{1},'{2}',this); return false;"">撤销</a> ",
                            x.Key, x.Where(a => a.ProductStatus == FCake.Domain.Enums.OrderBatchMakeStatus.Making).Sum(a => a.Num), id);
                        sb.AppendFormat(@"<a class=""btn btn-success"" href=""javascript:;"" onclick=""EndProductMakeByOrderNo('{0}',{1},'{2}',this); return false;"">完成</a>",
                            x.Key, x.Where(a => a.ProductStatus == FCake.Domain.Enums.OrderBatchMakeStatus.Making).Sum(a => a.Num), id);
                    }

                    html.Add(new { id = x.Key, html = sb.ToString() });
                }
                #endregion

                #region product
                foreach (var x in batchInfos.GroupBy(a => a.ProductId))
                {
                    sb = new StringBuilder();
                    if (x.Any(a => a.ProductStatus != OrderBatchMakeStatus.Complete) == false)
                    {
                        sb.AppendFormat("({0}) ", EnumHelper.GetDescription(OrderBatchMakeStatus.Complete));
                    }
                    if (x.Any(a => a.ProductStatus == OrderBatchMakeStatus.Making))
                    {
                        sb.AppendFormat("({0}) ", EnumHelper.GetDescription(OrderBatchMakeStatus.Making));
                    }
                    if (x.Any(a => a.ProductStatus == OrderBatchMakeStatus.Making) == false
                        && x.Any(a => a.ProductStatus == OrderBatchMakeStatus.NotStart)
                        && x.Any(a => a.ProductStatus == OrderBatchMakeStatus.Complete))
                    {
                        sb.AppendFormat("({0}) ", EnumHelper.GetDescription(OrderBatchMakeStatus.Making));
                    }
                    if (x.Any(a => a.ProductStatus != OrderBatchMakeStatus.NotStart) == false)
                    {
                        sb.AppendFormat("({0}) ", EnumHelper.GetDescription(OrderBatchMakeStatus.NotStart));
                    }


                    if (x.Any(a => a.ProductStatus == OrderBatchMakeStatus.NotStart))
                    {
                        sb.AppendFormat(@"<a class=""btn btn-info"" href=""javascript:;"" onclick=""BeginProductMakeByProductId('{0}','{1}',{2},'{3}',this); return false;"">开始</a> ",
                            x.Key, Server.UrlEncode(x.First().ProductName), x.Where(a => a.ProductStatus == FCake.Domain.Enums.OrderBatchMakeStatus.NotStart).Sum(a => a.Num), id);
                    }
                    if (x.Any(a => a.ProductStatus == OrderBatchMakeStatus.Making))
                    {
                        sb.AppendFormat(@"<a class=""btn btn-danger"" href=""javascript:;"" onclick=""RescindProductMakeByProductId('{0}','{1}',{2},'{3}',this); return false;"">撤销</a> ",
                            x.Key, Server.UrlEncode(x.First().ProductName), x.Where(a => a.ProductStatus == FCake.Domain.Enums.OrderBatchMakeStatus.Making).Sum(a => a.Num), id);
                        sb.AppendFormat(@"<a class=""btn btn-success"" href=""javascript:;"" onclick=""EndProductMakeByProductId('{0}','{1}',{2},'{3}',this); return false;"">完成</a> ",
                            x.Key, Server.UrlEncode(x.First().ProductName), x.Where(a => a.ProductStatus == FCake.Domain.Enums.OrderBatchMakeStatus.Making).Sum(a => a.Num), id);
                    }

                    html.Add(new { id = x.Key, html = sb.ToString() });
                }
                #endregion

                return Json(new { validate = true, html = html });
            }
            catch (Exception ex)
            {
                return Json(new { validate = false, msg = ex.Message });
            }
        }
        /// <summary>
        /// 开始产品制作
        /// </summary>
        /// <param name="id">kitchenMakeDetailId</param>
        /// <returns></returns>
        [CheckPermission(isRelease = true)]
        [HttpPost]
        public ActionResult BeginProductMake(string id)
        {
            var result = _kitchenService.StartSubProduct(id, UserCache.CurrentUser.Id);
            return Json(new { validate = result });
        }
        /// <summary>
        /// 取消产品制作
        /// </summary>
        /// <param name="id">kitchenMakeDetailId</param>
        /// <returns></returns>
        [CheckPermission(isRelease = true)]
        [HttpPost]
        public ActionResult RescindProductMake(string id)
        {
            var result = _kitchenService.RescindSubProduct(id, UserCache.CurrentUser.Id);
            return Json(new { validate = result });
        }
        /// <summary>
        /// 完成产品制作
        /// </summary>
        /// <param name="id">kitchenMakeDetailId</param>
        /// <returns></returns>
        [CheckPermission(isRelease = true)]
        [HttpPost]
        public ActionResult EndProductMake(string id)
        {
            var result = _kitchenService.EndSubProduct(id, UserCache.CurrentUser.Id);
            return Json(new { validate = result });
        }
        /// <summary>
        /// 由产品类型开始某一产品类型的制作
        /// </summary>
        /// <param name="id"></param>
        /// <param name="batchNo"></param>
        /// <returns></returns>
        [CheckPermission(isRelease = true)]
        [HttpPost]
        public ActionResult BeginProductMakeByProductId(string id, string batchNo)
        {
            var result = _kitchenService.StartSubProductType(id, batchNo, UserCache.CurrentUser.Id);
            return Json(new { validate = result });
        }
        /// <summary>
        /// 由产品类型取消某一产品类型的制作
        /// </summary>
        /// <param name="id"></param>
        /// <param name="batchNo"></param>
        /// <returns></returns>
        [CheckPermission(isRelease = true)]
        [HttpPost]
        public ActionResult RescindProductMakeByProductId(string id, string batchNo)
        {
            var result = _kitchenService.RescindSubProductType(id, batchNo, UserCache.CurrentUser.Id);
            return Json(new { validate = result });
        }
        /// <summary>
        /// 由产品类型完成某一产品类型的制作
        /// </summary>
        /// <param name="id"></param>
        /// <param name="batchNo"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EndProductMakeByProductId(string id, string batchNo)
        {
            var result = _kitchenService.EndSubProductType(id, batchNo, UserCache.CurrentUser.Id);
            return Json(new { validate = result });
        }
        /// <summary>
        /// 由产品类型开始某一产品类型的制作
        /// </summary>
        /// <param name="id"></param>
        /// <param name="batchNo"></param>
        /// <returns></returns>
        [CheckPermission(isRelease = true)]
        [HttpPost]
        public ActionResult BeginProductMakeByOrderNo(string id)
        {
            var result = _kitchenService.StartOrder(id, UserCache.CurrentUser.Id);
            return Json(new { validate = result });
        }
        /// <summary>
        /// 由产品类型取消某一产品类型的制作
        /// </summary>
        /// <param name="id"></param>
        /// <param name="batchNo"></param>
        /// <returns></returns>
        [CheckPermission(isRelease = true)]
        [HttpPost]
        public ActionResult RescindProductMakeByOrderNo(string id)
        {
            var result = _kitchenService.RescindOrder(id, UserCache.CurrentUser.Id);
            return Json(new { validate = result });
        }
        /// <summary>
        /// 由产品类型完成某一产品类型的制作
        /// </summary>
        /// <param name="id"></param>
        /// <param name="batchNo"></param>
        /// <returns></returns>
        [CheckPermission(isRelease = true)]
        [HttpPost]
        public ActionResult EndProductMakeByOrderNo(string id)
        {
            var result = _kitchenService.EndOrder(id, UserCache.CurrentUser.Id);
            return Json(new { validate = result });
        }
        /// <summary>
        /// 由产品类型开始某一产品类型的制作
        /// </summary>
        /// <param name="id"></param>
        /// <param name="batchNo"></param>
        /// <returns></returns>
        [CheckPermission(isRelease = true)]
        [HttpPost]
        public ActionResult BeginBatch(string id)
        {
            var result = _kitchenService.StartBatch(id, UserCache.CurrentUser.Id);
            return Json(new { validate = result });
        }
        /// <summary>
        /// 由产品类型取消某一产品类型的制作
        /// </summary>
        /// <param name="id"></param>
        /// <param name="batchNo"></param>
        /// <returns></returns>
        [CheckPermission(isRelease = true)]
        [HttpPost]
        public ActionResult RescindBatch(string id)
        {
            var result = _kitchenService.RescindBatch(id, UserCache.CurrentUser.Id);
            return Json(new { validate = result });
        }
        /// <summary>
        /// 由产品类型完成某一产品类型的制作
        /// </summary>
        /// <param name="id"></param>
        /// <param name="batchNo"></param>
        /// <returns></returns>
        [CheckPermission(isRelease = true)]
        [HttpPost]
        public ActionResult EndBatch(string id)
        {
            var result = _kitchenService.EndBatch(id, UserCache.CurrentUser.Id);
            return Json(new { validate = result });
        }
    }
}
