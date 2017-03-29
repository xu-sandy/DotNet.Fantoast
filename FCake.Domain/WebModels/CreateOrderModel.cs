using FCake.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FCake.Domain.WebModels
{
    public class CreateOrderModel
    {
        /// <summary>
        /// 是否需要生日蜡烛
        /// </summary>
        public YesOrNo Candle { get; set; }
        /// <summary>
        /// 收货地址Id
        /// </summary>
        public string AddressId { get; set; }
        /// <summary>
        /// 配送方式
        /// </summary>
        public DeliveryType RdType { get; set; }
        /// <summary>
        /// 配送时间
        /// </summary>
        public DateTime Time { get; set; }
        /// <summary>
        /// 自提站点Id
        /// </summary>
        public string LogistId { get; set; }
        /// <summary>
        /// 代金卡支付
        /// </summary>
        public decimal GiftCardPay { get; set; }
        /// <summary>
        /// 积分支付
        /// </summary>
        public decimal IntegralPay { get; set; }
        /// <summary>
        /// 优惠券支付
        /// </summary>
        public decimal CouponPay { get; set; }
        /// <summary>
        /// 使用的积分值（用于取消订单时退回使用的积分值）
        /// </summary>
        public int UsedIntegralVal { get; set; }
        /// <summary>
        /// 支付方式
        /// </summary>
        public FeeType FeeType { get; set; }
        /// <summary>
        /// 收货人姓名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 收货人手机号
        /// </summary>
        public string Mobile { get; set; }
        /// <summary>
        /// 留言
        /// </summary>
        public string DeliverMsg { get; set; }
        /// <summary>
        /// 配送时间段
        /// </summary>
        public string TimeBucket { get; set; }
        /// <summary>
        /// 是否已支付
        /// </summary>
        public int IsPay { get; set; }
        /// <summary>
        /// 优惠券Ids,用逗号隔开（如Id,Id,....,）
        /// </summary>
        public string CouponDetailIds { get; set; }
        /// <summary>
        /// 代金卡Ids,用逗号隔开（如Id,Id,....,）
        /// </summary>
        public string GiftCardDetailIds { get; set; }
        /// <summary>
        /// 订单总金额（所有商品（商品销售价或活动价或会员价）* 数量），用于判断订单总额在后台下单时是否发生改变
        /// </summary>
        public decimal OrderTotalAmount { get; set; }
    }
}