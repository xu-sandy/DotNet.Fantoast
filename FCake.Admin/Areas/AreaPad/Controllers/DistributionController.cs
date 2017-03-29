using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FCake.Domain.Entities;
using FCake.Domain.Enums;
using FCake.Domain.Common;
using FCake.Bll.Services;
using FCake.Domain.Common.Pad;
using FCake.Admin.Controllers;
using FCake.Admin.Helper;
using FCake.Admin.Areas.AreaPad.Models;
using FCake.Bll;
using FCake.Domain.WebModels;

namespace FCake.Admin.Areas.AreaPad.Controllers
{
    public class DistributionController : BaseController
    {
        public DistributionService ds = new DistributionService();
        public int PageSize = 500;//页面初始化Grid行数
        public ActionResult Index()
        {
            //return View(GetDistributionData(1, PageSize, null, null, null));
            return View(GetDistributionData(1, PageSize, "today", null, null));
        }

        [HttpPost]
        [ValidateInput(false)]
        [CheckPermission(isRelease = true)]
        public ActionResult SaveOrderException(string orderNo,string msg)
        {
            var result = new DistributionService().SaveOrderException(orderNo, msg, UserCache.CurrentUser.Id);

            return Json(new { validate = result == "", msg = result });
        }
        [HttpPost]
        [CheckPermission(isRelease = true)]
        public ActionResult GetOrderDetail(string orderNo)
        {
            ViewBag.orderNo = orderNo;
            var dietributionDetail = ds.GetOrderDetail<DistributionDetail>(orderNo);
            return View(dietributionDetail);
        }

        /// <summary>
        /// 获取配送信息
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="distributionStatus">配送状态</param>
        /// <param name="dateReqFrom">要求送达时间下界</param>
        /// <param name="dateReqEnd">要求送达时间上界</param>
        /// <returns></returns>
        [CheckPermission(isRelease = true)]
        public List<DistributionModel> GetDistributionData(int? page, int? pageSize, string curDate, int? period, StatusDistribution? status)
        {
            var pager = new Pagination(page, pageSize);
            int totalCount = 0;
            var data = ds.GetData(curDate, period, status, out totalCount, pager.Page, pager.PageSize);
            pager.SetPagination(totalCount);
            ViewBag.Pager = pager;
            return data;
        }

        /// <summary>
        /// 配送信息Table分部视图
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="distributionStatus"></param>
        /// <param name="dateReqFrom"></param>
        /// <param name="dateReqEnd"></param>
        /// <returns></returns>
        [CheckPermission(isRelease = true)]
        public PartialViewResult _PartialDistribution(int? page, int? pageSize, string curDate, int? period, StatusDistribution? status)
        {
            return PartialView(GetDistributionData(page, PageSize, curDate, period, status));
        }
        /// <summary>
        /// 配送信息tbody分部视图
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="distributionStatus"></param>
        /// <param name="dateReqFrom"></param>
        /// <param name="dateReqEnd"></param>
        /// <returns></returns>
        [CheckPermission(isRelease = true)]
        public PartialViewResult _PartialDIstributionView(int? page, int? pageSize, string curDate, int? period, StatusDistribution? status)
        {
            return PartialView(GetDistributionData(page, PageSize, curDate, period, status));
        }


        /// <summary>
        /// 状态操作
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpPost]
        [CheckPermission(isRelease = true)]
        public ActionResult StatusHandle(string orderNo, StatusDistribution status, bool isSMS)
        {
            string modifyUserId = Helper.UserCache.CurrentUser.Id;
            return Json(new DistributionService().StatusHandle(orderNo, status, modifyUserId, isSMS));
        }
        [HttpPost]
        [CheckPermission(isRelease = true)]

        public ActionResult StatusRevert(string orderNo, StatusDistribution status)
        {
            string modifyUserId = Helper.UserCache.CurrentUser.Id;
            return Json(new DistributionService().StatusRevert(orderNo, status, modifyUserId));
        }
        [CheckPermission(isRelease = true)]
        public PartialViewResult _PartialReportExceptionModal()
        {
            return PartialView();
        }

        /// <summary>
        /// 批量开始上午或下午配送
        /// </summary>
        /// <param name="section"></param>
        /// <param name="isSMS"></param>
        /// <returns></returns>
        [HttpPost]
        [CheckPermission(isRelease = true)]
        public ActionResult BeginSection(string section, bool isSMS)
        {
            DateTime? startTime = null;
            DateTime? endTime = null;
            if (section == "morning")
            {
                endTime = Convert.ToDateTime("13:00:00");
            }
            if (section == "afternoon")
            {
                startTime = Convert.ToDateTime("13:00:00");
            }

            var result = ds.BegioSection(startTime, endTime, UserCache.CurrentUser.Id, isSMS);
            return Json(new { validate = result});
        }

        /// <summary>
        /// 货到付款的订单完成时要修改支付金额
        /// </summary>
        /// <param name="orderNo">订单号</param>
        /// <param name="actualPay">收到的金额</param>
        /// <returns></returns>
        [HttpPost]
        [CheckPermission(isRelease = true)]
        public ActionResult UpdateActualPayByCompateOrder(string orderNo, decimal? actualPay, decimal? giftCardPay)
        {
            OrderService os = new OrderService();
            var result = os.UpdateActualPayByCompateOrder(orderNo,Convert.ToDecimal(actualPay), Convert.ToDecimal(giftCardPay));
            return Json(result);
        }
    }
}
