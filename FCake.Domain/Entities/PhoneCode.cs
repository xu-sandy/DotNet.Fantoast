using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FCake.Domain.Entities
{
    public class PhoneCode : BaseEntity
    {
        /// <summary>
        /// 手机号
        /// </summary>
        [DataMember]
        public string Mobile { get; set; }
        /// <summary>
        /// 验证码
        /// </summary>
        [DataMember]
        public string Code { get; set; }
        /// <summary>
        /// 发送时间
        /// </summary>
        [DataMember]
        public DateTime SendTime { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
        [DataMember]
        public string CustomerID { get; set; }

        public virtual Customers Customer { get; set; }
    }
}
