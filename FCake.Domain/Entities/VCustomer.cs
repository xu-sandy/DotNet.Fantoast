using FCake.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FCake.Domain.Entities
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class VCustomer : BaseEntity
    {
        /// <summary>
        /// 用户姓名
        /// </summary>
        [DataMember]
        public string FullName { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        [DataMember]
        public int? Sex { get; set; }
        /// <summary>
        /// 固定电话
        /// </summary>
        [DataMember]
        public string Tel { get; set; }
        /// <summary>
        /// 移动电话
        /// </summary>
        [DataMember]
        public string Mobile { get; set; }
        /// <summary>
        /// 邮箱
        /// </summary>
        [DataMember]
        public string Email { get; set; }
        ///// <summary>
        ///// 积分
        ///// </summary>
        //[DataMember]
        //public int Integral { get; set; }
        ///// <summary>
        ///// 成长值
        ///// </summary>
        //[DataMember]
        //public int GrowthValue { get; set; }
        /// <summary>
        /// 状态【0=启用 1=禁用】
        /// </summary>
        [DataMember]
        public short IsDisabled { get; set; }
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
        /// 区域
        /// </summary>
        [DataMember]
        public string Area { get; set; }
        /// <summary>
        /// 街道地址
        /// </summary>
        [DataMember]
        public string Address { get; set; }
    }
}
