using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FCake.Domain.Entities
{
    /// <summary>
    /// 客户与订单信息数据库视图对象 客户管理页面
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class VCustomOrders : BaseEntity
    {
        //[DataMember]
        //public string Id { get; set; }
        /// <summary>
        /// 客户姓名
        /// </summary>
        [DataMember]
        public string FullName { get; set; }
        /// <summary>
        /// 手机
        /// </summary>
        [DataMember]
        public string Mobile { get; set; }
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
        /// <summary>
        /// 订单号
        /// </summary>
        [DataMember]
        public string No { get; set; }
    }
}
