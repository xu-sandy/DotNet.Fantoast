using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FCake.Domain.Entities
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class OrderHist : BaseEntity
    {
        [DataMember]
        public string No { get; set; }
        [DataMember]
        public Enums.OrderSource OrderSource { get; set; }
        [DataMember]
        public DateTime? RequiredTime { get; set; }
        [DataMember]
        public Enums.FeeType FeeType { get; set; }
        [DataMember]
        public Enums.OrderStatus Status { get; set; }
        [DataMember]
        public string CustomerId { get; set; }
        [DataMember]
        public string Receiver { get; set; }
        [DataMember]
        public string ReceiverProvince { get; set; }
        [DataMember]
        public string ReceiverCity { get; set; }
        [DataMember]
        public int DeliveryType { get; set; }
        [DataMember]
        public string CustomerAddressId { get; set; }
        [DataMember]
        public string LogisticsSiteId { get; set; }
        [DataMember]
        public string ReceiverArea { get; set; }
        [DataMember]
        public string ReceiverAddr { get; set; }
        [DataMember]
        public string ReceiverMobile { get; set; }
        [DataMember]
        public string ReceiverTel { get; set; }
        [DataMember]
        public string DeliverNo { get; set; }
        [DataMember]
        public DateTime? DeliverTime { get; set; }
        [DataMember]
        public string DeliverMsg { get; set; }
        [DataMember]
        public int? DeliverStatus { get; set; }
        [DataMember]
        public string TradeNo { get; set; }
        [DataMember]
        public decimal? ActualPay { get; set; }
        [DataMember]
        public Enums.TradeStatus TradeStatus { get; set; }
        [DataMember]
        public decimal? TotalPrice { get; set; }
        /// <summary>
        /// 需要支付的钱(排除钱包的钱)
        /// </summary>
        [DataMember]
        public decimal? NeedPay { get; set; }
        /// <summary>
        /// 优惠券支付
        /// </summary>
        [DataMember]
        public decimal CouponPay { get; set; }
        [DataMember]
        public string Remark { get; set; }
        [DataMember]
        public string OrderId { get; set; }
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
    }
}
