using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using FCake.Domain.Enums;


namespace FCake.Domain.Entities
{
    /// <summary>
    /// 物流配送
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class Distribution : BaseEntity
    {
        /// <summary>
        /// 订单号
        /// </summary>
        [DataMember]
        public string OrderNo { get; set; }

        /// <summary>
        /// 要求送达时间
        /// </summary>
        [DataMember]
        public DateTime? RequiredTime { get; set; }
        /// <summary>
        /// 支付类型
        /// </summary>
        [DataMember]
        public FeeType feeType { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        [DataMember]
        public DateTime? BeginTime { get; set; }

        /// <summary>
        /// 完成时间
        /// </summary>
        [DataMember]
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 制作状态（未开始=0,配送中=1,完成=2）
        /// </summary>
        [DataMember]
        public StatusDistribution? Status { get; set; }

        /// <summary>
        /// 收货地址
        /// </summary>
        [DataMember]
        public string Address { get; set; }

        /// <summary>
        /// 订单
        /// </summary>
        [DataMember]
        public virtual Orders Order { get; set; }
    }
}
