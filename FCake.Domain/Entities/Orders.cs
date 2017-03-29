using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FCake.Domain.Entities
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Orders : BaseEntity
    {
        //CreatedOn 成交时间

        /// <summary>
        /// 订单编号(唯一)
        /// </summary>
        [DataMember]
        public string No { get; set; }
        /// <summary>
        /// 订单来源(在线订单=1 电话订单=2)
        /// </summary>
        [DataMember]
        public Enums.OrderSource OrderSource { get; set; }
        /// <summary>
        /// 要求送达时间
        /// </summary>
        [DataMember]
        public DateTime? RequiredTime { get; set; }
        /// <summary>
        /// 支付类型(货到付现金=0,货到POS刷卡=1,支付宝支付=2,财付通支付=3,其他支付=4)
        /// </summary>
        [DataMember]
        public Enums.FeeType FeeType { get; set; }
        /// <summary>
        /// 订单状态
        /// 等待付款=0,下单成功=1,排产中=2,制作中=3,制作完成=4,配送中=5,已完成=6,已取消=7,交易关闭=90
        /// </summary>
        [DataMember]
        public Enums.OrderStatus Status { get; set; }
        /// <summary>
        /// 支付状态(未支付=0,已支付=1,已退款=2)
        /// </summary>
        [DataMember]
        public Enums.TradeStatus TradeStatus { get; set; }
        ///// <summary>
        ///// 付款时间
        ///// </summary>
        //[DataMember]
        //public DateTime TradeTime { get; set; }
        /// <summary>
        /// 顾客id
        /// </summary>
        [DataMember]
        public string CustomerId { get; set; }
        /// <summary>
        /// 收货人
        /// </summary>
        [DataMember]
        public string Receiver { get; set; }
        /// <summary>
        /// 收货省份
        /// </summary>
        [DataMember]
        public string ReceiverProvince { get; set; }
        /// <summary>
        /// 收货人城市
        /// </summary>
        [DataMember]
        public string ReceiverCity { get; set; }
        /// <summary>
        /// 收货方式，自提/配送
        /// </summary>
        [DataMember]
        public int DeliveryType { get; set; }
        /// <summary>
        /// 顾客地址id
        /// </summary>
        [DataMember]
        public string CustomerAddressId { get; set; }
        /// <summary>
        /// 自提站点id
        /// </summary>
        [DataMember]
        public string LogisticsSiteId { get; set; }
        /// <summary>
        /// 收货区域
        /// </summary>
        [DataMember]
        public string ReceiverArea { get; set; }
        /// <summary>
        /// 详细收货地址
        /// </summary>
        [DataMember]
        public string ReceiverAddr { get; set; }
        /// <summary>
        /// 收货人移动电话
        /// </summary>
        [DataMember]
        public string ReceiverMobile { get; set; }
        /// <summary>
        /// 收货人固定电话
        /// </summary>
        [DataMember]
        public string ReceiverTel { get; set; }
        /// <summary>
        /// 快递单号
        /// </summary>
        [DataMember]
        public string DeliverNo { get; set; }
        /// <summary>
        /// 买家留言
        /// </summary>
        [DataMember]
        public string DeliverMsg { get; set; }
        /// <summary>
        /// 支付交易号
        /// </summary>
        [DataMember]
        public string TradeNo { get; set; }
        /// <summary>
        /// 支付金额（用户实际支付的现金，包括支付宝、货到付款等）
        /// </summary>
        [DataMember]
        public decimal ActualPay { get; set; }
        /// <summary>
        /// 总价
        /// </summary>
        [DataMember]
        public decimal TotalPrice { get; set; }
        /// <summary>
        /// 需要支付的钱(排除代金卡支付、优惠券支付、积分支付)
        /// </summary>
        [DataMember]
        public decimal NeedPay { get; set; }
        /// <summary>
        /// 代金卡支付
        /// </summary>
        [DataMember]
        public decimal GiftCardPay { get; set; }
        /// <summary>
        /// 积分支付
        /// </summary>
        [DataMember]
        public decimal IntegralPay { get; set; }
        /// <summary>
        /// 优惠券支付
        /// </summary>
        [DataMember]
        public decimal CouponPay { get; set; }
        /// <summary>
        /// 使用的积分值（用于取消订单时退回使用的积分值）
        /// </summary>
        public int UsedIntegralVal { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [DataMember]
        public string Remark { get; set; }
        /// <summary>
        /// 审核状态 待审核=0，审核通过=1，审核未通过=2
        /// </summary>
        [DataMember]
        public Enums.ReviewStatus ReviewStatus { get; set; }
        /// <summary>
        /// 是否需要蜡烛
        /// </summary>
        [DataMember]
        public Enums.YesOrNo Candle { get; set; }
        /// <summary>
        /// 订单详细
        /// </summary>
        [DataMember]
        public virtual ICollection<OrderDetails> OrderDetails { get; set; }

        [DataMember]
        public virtual Customers Customers { get; set; }
        /// <summary>
        /// 区分网页订单和手机订单（0=电脑订单；1=手机订单;2=电话端）
        /// </summary>
        [DataMember]
        public int? OrderClient { get; set; }
        /// <summary>
        /// 要求送货时间的整个时间段
        /// </summary>
        [DataMember]
        public string RequiredTimeBucket { get; set; }
        /// <summary>
        /// 订单类型，来源数据字典（如：生日、朋友聚会、其他等）
        /// </summary>
        [DataMember]
        public string OrderType { get; set; }
        /// <summary>
        /// 订单审核人员Id
        /// </summary>
        [DataMember]
        public string ReviewUID { get;set;}

    }
}
