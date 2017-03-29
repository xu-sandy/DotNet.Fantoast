using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FCake.Domain.Entities
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Invoice : BaseEntity
    {
        /// <summary>
        /// 订单ID
        /// </summary>
        [DataMember]
        public string OrderId { get; set; }
        /// <summary>
        /// 发票类型
        /// </summary>
        [DataMember]
        public string InvoiceType { get; set; }
        /// <summary>
        /// 发票抬头
        /// </summary>
        [DataMember]
        public string InvoiceTitle { get; set; }
    }
}
