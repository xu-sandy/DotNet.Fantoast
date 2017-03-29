using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FCake.Domain.Entities
{
    /// <summary>
    /// 微信预支付表
    /// 针对不接受同一订单
    /// </summary>
    public class PrePayInfo
    {
        public int Id { get; set; }
        /// <summary>
        /// 关联订单号
        /// </summary>
        public string OrderNo { get; set; }
        /// <summary>
        /// 预支付号
        /// </summary>
        public string PrePayNo { get; set; }
        /// <summary>
        /// 需要支付
        /// </summary>
        public int NeedPay { get; set; }
        /// <summary>
        /// 记录时间
        /// </summary>
        public DateTime? LogingTime { get; set; }
    }
}
