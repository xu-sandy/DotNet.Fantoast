using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FCake.Bll.Services;
using FCake.Admin.Models;
using FCake.Core.Common;
using FCake.Core.MvcCommon;
using FCake.Domain.Entities;

namespace FCake.Admin.Controllers
{
    public class ReportController : BaseController
    {
        //
        // GET: /Report/
        ReportService rs = new ReportService();
        public ActionResult Index()
        {
            return View();
        }
        #region 产品生产周期报表
        /// <summary>
        /// 产品生产周期
        /// </summary>
        /// <returns></returns>
        [CheckPermission(controlName = "Report", actionName = "ReportCycleTime", permissionCode = "view")]
        public ActionResult ReportCycleTime()
        {
            return View();
        }
        /// <summary>
        /// 产品生产周期
        /// </summary>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        [HttpPost]
        [CheckPermission(controlName = "Report", actionName = "ReportCycleTime", permissionCode = "view")]
        public ActionResult ReportCycleTime(DateTime? beginTime, DateTime? endTime)
        {
            var result = rs.GetReportByCycTime<CycleTimeVM>(beginTime, endTime);
            return Json(result);
        }
        #endregion

        #region 下单时段报表
        /// <summary>
        /// 下单时段
        /// </summary>
        /// <returns></returns>
        public ActionResult ReportOrderTime()
        {
            return View();
        }
        /// <summary>
        /// 下单时段
        /// </summary>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        [HttpPost]
        [CheckPermission(controlName = "Report", actionName = "ReportOrderTime", permissionCode = "view")]
        public ActionResult ReportOrderTime(DateTime? beginTime, DateTime? endTime, int type, string parma)
        {
            var result = rs.GetReportByOrderTime<ReportOrderTimeVModel>(beginTime, endTime, type, parma);
            return Json(result);
        }
        #endregion

        #region 销售流量报表
        public ActionResult Sales()
        {
            return View();
        }
        /// <summary>
        /// 销售流量报表
        /// </summary>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="type">类型：品种1，蛋糕2，磅数3</param>
        /// <returns></returns>
        [CheckPermission(controlName = "Report", actionName = "Sales", permissionCode = "view")]
        public ActionResult GetSalesByPosition(DateTime? beginTime, DateTime? endTime, int type)
        {
            var result = rs.GetSalesByPosition<ReportSalesVModel>(beginTime, endTime, type);
            return Json(result);
        }
        #endregion

        #region 财务报表
        public ActionResult FinancialReport()
        {
            return View();
        }
        /// <summary>
        /// 财务报表
        /// </summary>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Report", actionName = "FinancialReport", permissionCode = "view")]
        [HttpPost]
        public ActionResult FinancialReports(DateTime? beginTime, DateTime? endTime, DateTime? reqBeginTime, DateTime? reqEndTime, int? page = 1, int? orderStatus = -1, int? reviewStatus = 1, int? pageSize = 100)
        {
            int totalRecord = 0;
            var result = rs.GetFinancialReportByTime<ReportFinancialVModel>(beginTime, endTime, reqBeginTime, reqEndTime, reviewStatus, pageSize, page, orderStatus, out totalRecord);
            return Json(new { rows = result, total = totalRecord });
        }
        /// <summary>
        /// 将查询结果导出到excel
        /// </summary>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Report", actionName = "FinancialReport", permissionCode = "view")]
        public JsonResult ExportExcel(DateTime? beginTime, DateTime? endTime, DateTime? reqBeginTime, DateTime? reqEndTime, int? page = 1, int? orderStatus = -1, int? reviewStatus = 1, int? pageSize = 100)
        {
            int totalRecord = 0;
            var result = rs.GetFinancialByTime<ReportFinancialVModel>(beginTime, endTime, reqBeginTime, reqEndTime, reviewStatus, pageSize, page, orderStatus, out totalRecord);
            //if (result.Successed) {
            //    result.Data = Url+result.Data.ToString();
            //}
            return Json(result);
        }
        [CheckPermission(controlName = "Report", actionName = "ReportOrder", permissionCode = "view")]
        public JsonResult ExportOrderReportToExcel(DateTime? beginTime, DateTime? endTime, DateTime? reqBeginTime, DateTime? reqEndTime, int? page, int? orderStatus = -1, int? reviewStatus = 1, int? pageSize = 100)
        {
            int totalRecord = 0;
            var result = rs.ExportOrderReport(beginTime, endTime, reqBeginTime, reqEndTime, reviewStatus, pageSize, page, orderStatus, out totalRecord);
            return Json(result);
        }
        /// <summary>
        /// 订单，订单明细报表
        /// </summary>
        /// <returns></returns>
        [CheckPermission(controlName = "Report", actionName = "ReportOrder", permissionCode = "view")]
        public ActionResult ReportOrder()
        {
            return View();
        }
        [CheckPermission(controlName = "Report", actionName = "ReportOrder", permissionCode = "view")]
        public ActionResult GetReportOrder(DateTime? beginTime, DateTime? endTime, DateTime? reqBeginTime, DateTime? reqEndTime, int page, int orderStatus = -1, int? reviewStatus = 1, int pageSize = 100)
        {
            int totalRecord = 0;
            var result = rs.OrderReport(beginTime, endTime, reqBeginTime, reqEndTime, reviewStatus, pageSize, page, orderStatus, out totalRecord);
            return Json(new { total = totalRecord, rows = result });
        }
        /// <summary>
        /// 销售流量导出
        /// </summary>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Report", actionName = "Sales", permissionCode = "view")]
        public JsonResult ExportSalesToExcel(DateTime? beginTime, DateTime? endTime, int type)
        {
            var result = rs.ExportSales(beginTime, endTime, type);
            return Json(result);
        }
        /// <summary>
        /// 下单时段导出
        /// </summary>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="type"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Report", actionName = "Sales", permissionCode = "view")]
        public JsonResult ExportOrderTime(DateTime? beginTime, DateTime? endTime, int type, string param)
        {
            var result = rs.ExportOrderTime(beginTime, endTime, type, param);
            return Json(result);
        }
        /// <summary>
        /// 产品生产周期报表导出
        /// </summary>
        /// <param name="beginTime"></param>
        /// <param name="endtime"></param>
        /// <returns></returns>
        public JsonResult ExportCycTime(DateTime? beginTime, DateTime? endtime)
        {
            var result = rs.ExportCycTime(beginTime, endtime);
            return Json(result);
        }
        #endregion

        #region 销售统计报表
        public ActionResult SalesStatisticsReport()
        {
            return View();
        }
        public ActionResult GetSalesStatisticsReportData(int year)
        {
            int totalRecord = 0;
            var result = rs.GetSalesStatisticsReportData<SalesStatisticsReportModel>(year, out totalRecord);
            return Json(new { rows = result, total = totalRecord });
        }
        public JsonResult ExportSalesStatisticsReport(int year)
        {
            int totalRecord = 0;
            var dt = rs.GetSalesStatisticsReportData<SalesStatisticsReportModel>(year, out totalRecord);
            string[] fields = { "OMonth", "OrderClient", "OrderQuantity", "ActualPay", "CouponPay", "GiftCardPay", "IntegralPay", "TotalPrice"};
            string[] names = { "月份", "来源", "订单量", "现金支付", "优惠券抵扣", "代金卡抵扣", "积分抵扣", "总销售额" };

            int[] merger = { 0};
            string fileName = "销售统计报表_" + DateTime.Now.ToString("yyyyMMddhhmmss"); 
            FCake.Core.Common.ExportExcel excelExport = new ExportExcel();
            string fileUrl = excelExport.ToExcel(fileName, dt, fields, names, merger);
            return Json(OpResult.Success("导出成功", null, fileUrl));
        }
        #endregion

        #region 统计分析报表
        [CheckPermission(controlName = "Report", actionName = "StatisticalAnalysis", permissionCode = "view")]
        public ActionResult StatisticalAnalysis()
        {
            DateTime beginTime = DateTime.Parse(DateTime.Now.ToString("yyyy-01-01"));
            DateTime endTime = beginTime.AddYears(1);

            //蛋糕类型销售占比（订单总额）
            List<string> productTypeTitleList = new List<string>();
            ViewBag.ProductTypeTotalPriceData = rs.StatisticalAnalysis(beginTime,endTime, 1, out productTypeTitleList).ToJson();
            ViewBag.ProductTypeTitleList = productTypeTitleList.ToJson();

            //订单用途（订单总额）
            List<string> orderTypeTitle = new List<string>();
            ViewBag.OrderTypeTotalPriceData = rs.StatisticalAnalysis(beginTime, endTime, 3, out orderTypeTitle).ToJson();
            ViewBag.OrderTypeTitle = orderTypeTitle.ToJson();

            //订单来源（订单总额）
            List<string> orderClientTitle = new List<string>();
            ViewBag.OrderClientTotalPriceData = rs.StatisticalAnalysis(beginTime, endTime, 5, out orderClientTitle).ToJson();
            ViewBag.OrderClientTitle = orderClientTitle.ToJson();

            return View();
        }
        /// <summary>
        /// 用于异步获取统计分析中数据
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [CheckPermission(controlName = "Report", actionName = "StatisticalAnalysis", permissionCode = "view")]
        public JsonResult GetStatisticalAnalysisData(string year,int type)
        {
            DateTime beginTime = DateTime.Parse(year + "-01-01");
            DateTime endTime = beginTime.AddYears(1);
            List<string> legendData = new List<string>();
            var chartData = rs.StatisticalAnalysis(beginTime, endTime, type, out legendData).ToJson();

            return Json(new{ legendData = legendData, chartData = chartData});
        }
        #endregion

    }
}
