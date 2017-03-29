using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FCake.Admin.Models
{
    /// <summary>
    /// 财务报表，订单，订单明细
    /// </summary>
    public class ReportOrder
    {
        /// <summary>
        /// 订单来源（在线或者电话）
        /// </summary>
        public string OrderSource { get; set; }
        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderNo { get; set; }
        /// <summary>
        /// 下单日期
        /// </summary>
        public string CreateOn { get; set; }
        /// <summary>
        /// 送货日期
        /// </summary>
        public string RequestTime { get; set; }
        /// <summary>
        /// 产品名
        /// </summary>
        public string  Name { get; set; }
        /// <summary>
        /// 规格(磅)
        /// </summary>
        public int Size { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int Num { get; set; }
        /// <summary>
        /// 单价
        /// </summary>
        public decimal? Price { get; set; }
        /// <summary>
        /// 优惠券支付
        /// </summary>
        public decimal? CouponPay { get; set; }
        /// <summary>
        /// 总价
        /// </summary>
        public decimal? TotalPrice { get; set; }
        /// <summary>
        /// 支付方式
        /// </summary>
        public string FeeType { get; set; }
    }
}