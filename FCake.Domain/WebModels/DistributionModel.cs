using FCake.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FCake.Domain.WebModels
{
    public class DistributionModel
    {
        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderNo { get; set; }

        /// <summary>
        /// 要求送达时间
        /// </summary>
        public DateTime? RequiredTime { get; set; }
        /// <summary>
        /// 支付类型
        /// </summary>
        public FeeType feeType { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? BeginTime { get; set; }

        /// <summary>
        /// 完成时间
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 制作状态（未开始=0,配送中=1,完成=2）
        /// </summary>
        public StatusDistribution? Status { get; set; }

        /// <summary>
        /// 收货地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 还需支付
        /// </summary>
        public decimal? NeedPay { get; set; }
        /// <summary>
        /// 要求送货时间段
        /// </summary>
        public string RequiredTimeBucket { get; set; }
        /// <summary>
        /// 已使用代金卡抵扣金额
        /// </summary>
        public decimal GiftCardPayed { get; set; }
    }
}
