using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCake.Domain.Entities;
using FCake.Domain;
using System.Data.SqlClient;
using System.Data;
using System.Web;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.HPSF;
using System.Reflection;
using System.Collections;
using System.Data.Entity;
using FCake.Domain.Enums;
using FCake.Core.Common;
using FCake.Core.MvcCommon;

namespace FCake.Bll.Services
{
    /// <summary>
    /// 首页销售数据报表
    /// </summary>
    public class ReportIndexService
    {
        private readonly EFDbContext _context = new EFDbContext();
        /// <summary>
        /// 根据时间获得已完成的订单
        /// </summary>
        /// <param name="beginTime"></param>
        /// <param name="EndTime"></param>
        /// <returns></returns>
        public List<Orders> GetCompletedOrderByTime(DateTime beginTime, DateTime EndTime)
        {
            var result = _context.Orders.Where(o => o.CreatedOn >= beginTime && o.CreatedOn <= EndTime && o.Status == OrderStatus.Completed && o.IsDeleted != 1).ToList();
            return result;
        }
        /// <summary>
        /// 根据时间获得审核通过的订单
        /// </summary>
        /// <param name="beginTime"></param>
        /// <param name="EndTime"></param>
        /// <returns></returns>
        public List<Orders> GetReviewPassOrderByTime(DateTime beginTime, DateTime EndTime)
        {
            var result = _context.Orders.Where(o => o.CreatedOn >= beginTime && o.CreatedOn <= EndTime && o.ReviewStatus == ReviewStatus.ReviewPass && o.IsDeleted != 1).ToList();
            return result;
        }
        /// <summary>
        /// 根据时间月销售额数据
        /// </summary>
        /// <param name="monthBegin">月起始时间如：2015-12-01</param>
        /// <param name="topProductName"></param>
        /// <param name="topProductSaleNum"></param>
        public void MonthSalesTotal(DateTime monthBegin, out List<string> monthDayTitleList, out List<decimal> monthDayActualPayList, out List<decimal> monthDayTotalPriceList)
        {
            monthDayTitleList = new List<string>();
            monthDayActualPayList = new List<decimal>();
            monthDayTotalPriceList = new List<decimal>();
            DateTime monthEnd = monthBegin.AddMonths(1);
            DateTime date = monthBegin;
            var monthCompletedOrderList = GetCompletedOrderByTime(monthBegin, monthEnd);
            while (date < monthEnd)
            {
                var time1 = date;
                var time2 = date.AddDays(1);
                var daySalesList = monthCompletedOrderList.Where(o => o.CreatedOn >= time1 && o.CreatedOn <= time2).ToList();
                monthDayTitleList.Add(int.Parse(time1.ToString("dd")) + "日");
                monthDayActualPayList.Add(daySalesList.Sum(o => o.ActualPay));
                monthDayTotalPriceList.Add(daySalesList.Sum(o => o.TotalPrice));
                date = date.AddDays(1);
            }
        }

        public int GetMemberQuantity(DateTime begin, DateTime end)
        {
            int result = _context.Customers.Where(o => o.CreatedOn >= begin && o.CreatedOn <= end && o.IsDeleted != 1).ToList().Count();
            return result;
        }

    }
}
