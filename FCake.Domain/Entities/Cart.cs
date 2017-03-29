using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FCake.Domain.Entities
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Cart:BaseEntity
    {
        /// <summary>
        /// 连接ID
        /// </summary>
        [DataMember]
        public string LinkID { get; set; }
        /// <summary>
        /// 产品ID
        /// </summary>
        [DataMember]
        public string SubProductID { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        [DataMember]
        public int Num { get; set; }
        /// <summary>
        /// 生日牌
        /// </summary>
        [DataMember]
        public string BirthdayCard { get; set; }
    }
}
