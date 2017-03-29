using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FCake.Admin.Models;
using FCake.Domain;
using FCake.Admin.Helper;
using FCake.Bll;
using FCake.Bll.Services;
using FCake.Core.Common;
using FCake.Domain.Entities;

namespace FCake.Admin.Controllers
{
    [Login]
    [CheckPermission(isRelease = true)]
    public class HomeController : BaseController
    {
        public readonly CommonService _commService = new CommonService();
        private readonly ReportIndexService _reportIndexService = new ReportIndexService();
        private readonly ReportService _reportService = new ReportService();
        [CheckPermission(controlName = "Home", actionName = "Index", permissionCode = "view")]
        public ActionResult Index()
        {
            ViewBag.isCustomerServiceRole = IsCustomerServiceRole();

            //获取版本号
            var version = AppConfig.SysVersion;
            ViewBag.Version = version;
            #region 首页报表数据
            DateTime curDayBegin = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            DateTime curDayEnd = curDayBegin.AddDays(1);
            DateTime curMonthBegin = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01"));
            DateTime curMonthEnd = curMonthBegin.AddMonths(1);
            List<Orders> curMonthCompletedOrderList = _reportIndexService.GetCompletedOrderByTime(curMonthBegin, curMonthEnd);
            List<Orders> curMonthReviewPassOrdersList = _reportIndexService.GetReviewPassOrderByTime(curMonthBegin, curMonthEnd);

            //新增会员
            ViewBag.curDayMemberQuantity = _reportIndexService.GetMemberQuantity(curDayBegin, curDayEnd);
            ViewBag.curMonthMemberQuantity = _reportIndexService.GetMemberQuantity(curMonthBegin, curMonthEnd);

            //昨日数据
            ViewBag.lastDayReviewPassOrderList = curMonthReviewPassOrdersList.Where(o => o.CreatedOn >= curDayBegin.AddDays(-1) && o.CreatedOn <= curDayEnd.AddDays(-1)).ToList();
            ViewBag.lastDayCompletedOrderList = curMonthCompletedOrderList.Where(o => o.CreatedOn >= curDayBegin.AddDays(-1) && o.CreatedOn <= curDayEnd.AddDays(-1)).ToList();

            //今日数据
            ViewBag.todayReviewPassOrderList = curMonthReviewPassOrdersList.Where(o => o.CreatedOn >= curDayBegin && o.CreatedOn <= curDayEnd).ToList();
            ViewBag.todayCompletedOrderList = curMonthCompletedOrderList.Where(o => o.CreatedOn >= curDayBegin && o.CreatedOn <= curDayEnd).ToList();

            //本月数据
            ViewBag.curMonthReviewPassOrderList = curMonthReviewPassOrdersList.ToList();
            ViewBag.curMonthCompletedOrderList = curMonthCompletedOrderList.ToList();



            //月销售额
            List<string> monthDayTitleList = new List<string>();
            List<decimal> monthDayActualPayList = new List<decimal>();
            List<decimal> monthDayTotalPriceList = new List<decimal>();
            _reportIndexService.MonthSalesTotal(curMonthBegin, out monthDayTitleList, out monthDayActualPayList, out monthDayTotalPriceList);
            ViewBag.monthDayTitleList = monthDayTitleList.ToJson();
            ViewBag.monthDayActualPayList = monthDayActualPayList.ToJson();
            ViewBag.monthDayTotalPriceList = monthDayTotalPriceList.ToJson();

            //近7天订单量
            var day7TitleList = new List<string>();
            var day7SaleNumberList = new List<decimal>();
            for (int i = 6; i >= 0; i--)
            {
                var time1 = DateTime.Parse(DateTime.Now.AddDays(0 - i).ToString("yyyy-MM-dd"));
                var time2 = DateTime.Parse(DateTime.Now.AddDays(0 - i + 1).ToString("yyyy-MM-dd"));
                var day7saleOrderList = _reportIndexService.GetReviewPassOrderByTime(time1, time2);
                day7TitleList.Add(int.Parse(DateTime.Now.AddDays(0 - i).ToString("dd")) + "日");
                day7SaleNumberList.Add(day7saleOrderList.Count());
            }
            ViewBag.day7TitleList = day7TitleList.ToJson();
            ViewBag.day7SaleNumberList = day7SaleNumberList.ToJson();

            //本月销售前10
            var topProductNameList = new List<string>();
            var topProductSaleNumList = new List<int>();
            _reportService.SaleTopProduct(curMonthBegin, curMonthEnd, out topProductNameList, out topProductSaleNumList);
            ViewBag.topProductNameList = topProductNameList.ToJson();
            ViewBag.topProductSaleNumList = topProductSaleNumList.ToJson();
            #endregion


            return View();
        }
        [HttpPost]
        [CheckPermission(controlName = "Home", actionName = "Index", permissionCode = "view")]
        public ActionResult GetMonthSalesTotal(string month)
        {
            DateTime monthBegin = DateTime.Parse(month + "-01");
            List<string> monthDayTitleList = new List<string>();
            List<decimal> monthDayActualPayList = new List<decimal>();
            List<decimal> monthDayTotalPriceList = new List<decimal>();
            _reportIndexService.MonthSalesTotal(monthBegin, out monthDayTitleList, out monthDayActualPayList, out monthDayTotalPriceList);
            string chartTitle = monthBegin.ToString("yyyy年MM月");
            return Json(new { chartTitle = chartTitle, monthDayTitleList = monthDayTitleList, monthDayActualPayList = monthDayActualPayList, monthDayTotalPriceList = monthDayTotalPriceList });
        }

    }
}
