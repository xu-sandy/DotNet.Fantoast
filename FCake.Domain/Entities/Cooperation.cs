using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FCake.Domain.Entities
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Cooperation : BaseEntity
    {
        /// <summary>
        /// 公司名称
        /// </summary>
        [DataMember]
        public string CompanyName { get; set; }
        /// <summary>
        /// 公司地址
        /// </summary>
        [DataMember]
        public string CompanyAddress { get; set; }
        /// <summary>
        /// 公司人数
        /// </summary>
        [DataMember]
        public int? CompanyPopulation { get; set; }
        /// <summary>
        /// 联系人名字
        /// </summary>
        [DataMember]
        public string Name { get; set; }
        /// <summary>
        /// 联系人电话
        /// </summary>
        [DataMember]
        public string Phone { get; set; }
        /// <summary>
        /// 联系人电邮
        /// </summary>
        [DataMember]
        public string Email { get; set; }
        /// <summary>
        /// 申请人ID
        /// </summary>
        [DataMember]
        public string CustomerId { get; set; }
        /// <summary>
        /// 申请时间
        /// </summary>
        [DataMember]
        public DateTime? ApplyForTime { get; set; }
        /// <summary>
        /// 申请状态 0=待审核 1=同意 2=不同意
        /// </summary>
        [DataMember]
        public int? Status { get; set; }
        /// <summary>
        /// 申请结果 0=不同意  1=同意
        /// </summary>
        //[DataMember]
        //public int? Result { get; set; }
        /// <summary>
        /// 备注说明
        /// </summary>
        [DataMember]
        public string Description { get; set; }
    }
}
