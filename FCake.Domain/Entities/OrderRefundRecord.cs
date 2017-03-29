using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FCake.Domain.Entities
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class OrderRefundRecord : BaseEntity
    {
        /// <summary>
        /// 退款操作人
        /// </summary>
        [DataMember]
        public string OperUserId { get; set; }
        /// <summary>
        /// 订单ID
        /// </summary>
        [DataMember]
        public string OrderNo { get; set; }
        /// <summary>
        /// 买家支付宝财付通银行账户等
        /// </summary>
        [DataMember]
        public string CustomerAccount { get; set; }
        /// <summary>
        /// 退款金额
        /// </summary>
        [DataMember]
        public decimal RefundAmount { get; set; }
    }
}
