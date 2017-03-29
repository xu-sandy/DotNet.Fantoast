using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FCake.Admin.Models
{
    public class RefundOrderVm
    {
        /// <summary>
        /// 定单号
        /// </summary>
        public string OrderNo { get; set; }
        /// <summary>
        /// 订单支付金额
        /// </summary>
        public decimal? RefundAmount { get; set; }
    }
}