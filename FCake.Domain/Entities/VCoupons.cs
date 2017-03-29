using FCake.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FCake.Domain.Entities
{
    /// <summary>
    /// 代金卡与客户关联数据库视图对象 代金卡信息页面
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class VCoupons : BaseEntity
    {
        /// <summary>
        /// 卡号
        /// </summary>
        [DataMember]
        public string CouponsSN { get; set; }
        /// <summary>
        /// 校验码
        /// </summary>
        [DataMember]
        public string CouponsPWD { get; set; }
        /// <summary>
        /// 面值
        /// </summary>
        [DataMember]
        public decimal FaceValue { get; set; }
        /// <summary>
        /// 余额
        /// </summary>
        [DataMember]
        public decimal Balance { get; set; }
        /// <summary>
        /// 允许使用开始日期
        /// </summary>
        [DataMember]
        public DateTime? AllowBeginDate { get; set; }
        /// <summary>
        /// 允许使用结束日期
        /// </summary>
        [DataMember]
        public DateTime? AllowEndDate { get; set; }
        /// <summary>
        /// 代金卡状态
        /// </summary>
        [DataMember]
        public CouponsStatus Status { get; set; }
        /// <summary>
        /// 销售价
        /// </summary>
        [DataMember]
        public decimal Price { get; set; }
        /// <summary>
        /// 充值日期
        /// </summary>
        [DataMember]
        public DateTime? RechargedTime { get; set; }
        /// <summary>
        /// 客户姓名
        /// </summary>
        [DataMember]
        public string FullName { get; set; }
    }
}
