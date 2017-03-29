using FCake.Admin.Controllers;
using FCake.Bll;
using FCake.Bll.Services;
using FCake.Domain.Common;
using FCake.Domain.Common.Pad;
using FCake.Domain.Entities;
using FCake.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FCake.Admin.Areas.AreaPad.Controllers
{
    public class OrderBatchController : BaseController
    {
        public OrderBatchService obs = new OrderBatchService();
        public int PageSize = 10;//页面初始化Grid行数
        [CheckPermission(actionName = "OrderBatch", controlName = "OrderBatch")]
        public ActionResult Index()
        {
            return View(GetQueryOrderBatchData(1, PageSize, new OrderBatch() { MakeStatus=OrderBatchMakeStatus.NotStart}));
        }
        /// <summary>
        /// 生产批次Table分部视图
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [CheckPermission(isRelease = true)]
        public PartialViewResult _PartialOrderBatch(int? page, int? pagesize, OrderBatch model)
        {
            return PartialView(GetQueryOrderBatchData(page, PageSize, model));
        }
        /// <summary>
        /// 生产批次tbody分部视图
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [CheckPermission(isRelease = true)]
        public PartialViewResult _PartialOrderBatchView(int? page, int? pagesize, OrderBatch model)
        {
            return PartialView(GetQueryOrderBatchData(page, PageSize, model));
        }
        /// <summary>
        /// 生产批次获取Grid数据
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [CheckPermission(isRelease = true)]
        private List<OrderBatch> GetQueryOrderBatchData(int? page, int? pagesize, OrderBatch model)
        {
            var pager = new Pagination(page, pagesize);
            int totalCount = 0;
            model.Status = BatchReviewStatus.ReviewPass;
            var data = obs.GetOrderBatchs(model,null, out totalCount, pager.Page, pager.PageSize);//取得数据
            pager.SetPagination(totalCount);
            ViewBag.Pager = pager;
            return data;
        }
    }
}
