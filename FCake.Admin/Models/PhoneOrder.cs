using FCake.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FCake.Admin.Models
{
    public class PhoneOrder
    {
        /// <summary>
        /// 订单号
        /// </summary>
        public string NO { get; set; }
        /// <summary>
        /// 客户ID
        /// </summary>
        public string CustomerId { get; set; }
        /// <summary>
        /// 地址类型
        /// 0=送货上门
        /// 1=站点自提
        /// </summary>
        public int AddressType { get; set; }
        /// <summary>
        /// 地址ID
        /// </summary>
        public string AddressId { get; set; }
        /// <summary>
        /// 产品及对应的数量
        /// </summary>
        public List<CollectionIDNum> Products { get; set; }
        /// <summary>
        /// 接收时间
        /// </summary>
        public DateTime ReceiveTime { get; set; }
        /// <summary>
        /// 发票类型
        /// </summary>
        public InvoiceType? InvoiceType { get; set; }
        /// <summary>
        /// 发票抬头
        /// </summary>
        public string InvoiceTitle { get; set; }
        /// <summary>
        /// 是否需要蜡烛
        /// </summary>
        public YesOrNo Candle { get; set; }
        /// <summary>
        /// 生日卡片
        /// </summary>
        public string BirthdayCard { get; set; }
        /// <summary>
        /// 客户留言
        /// </summary>
        public string DeliverMsg { get; set; }
        /// <summary>
        /// 订单备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 要求送货时间段
        /// </summary>
        public string TimeBucket { get; set; }
        /// <summary>
        /// 自提人名称
        /// </summary>
        public string RevierName { get; set; }
        /// <summary>
        /// 自提人电话
        /// </summary>
        public string RevierPhone { get; set; }

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



        public PhoneOrder()
        {
            NO = "";
            CustomerId = "";
            AddressId = "";
            Products = new List<CollectionIDNum>();
            InvoiceTitle = "";
            BirthdayCard = "";
            DeliverMsg = "";
        }
    }

    /// <summary>
    /// 记录ID与其对应数量
    /// </summary>
    public class CollectionIDNum
    {
        public string ID { get; set; }
        public int Num { get; set; }
        public string BirthdayCard { get; set; }
    }
}