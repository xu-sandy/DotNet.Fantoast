using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FCake.Domain.Entities
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class CustomerAddress:BaseEntity
    {
        /// <summary>
        /// 客户ID 外键
        /// </summary>
        [DataMember]
        public string CustomerId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string LogisticsSiteId { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        [DataMember]
        public string Address { get; set; }
        /// <summary>
        /// 地区
        /// </summary>
        [DataMember]
        public string Area { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string Location { get; set; }
        /// <summary>
        /// 是否默认
        /// </summary>
        [DataMember]
        public int IsDef { get; set; }
        /// <summary>
        /// 邮政编码
        /// </summary>
        [DataMember]
        public int? ZipCode { get; set; }
        /// <summary>
        /// 收货人
        /// </summary>
        [DataMember]
        public string Receiver { get; set; }
        /// <summary>
        /// 收货人固定电话
        /// </summary>
        [DataMember]
        public string ReceiverTel { get; set; }
        /// <summary>
        /// 收货人移动电话
        /// </summary>
        [DataMember]
        public string ReceiverMobile { get; set; }
        /// <summary>
        /// 省
        /// </summary>
        [DataMember]
        public string Province { get; set; }
        /// <summary>
        /// 市
        /// </summary>
        [DataMember]
        public string City { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public int DeliveryType { get; set; }

        [DataMember]
        public virtual Customers Customer { get; set; }
    }
}
