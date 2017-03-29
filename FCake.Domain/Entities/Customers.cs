using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FCake.Domain.Entities
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Customers : BaseEntity
    {
        public Customers() { }
        public Customers(string userName,string password) {
            Id = NewGuid();
            CreatedOn = DateTime.Now;
            Mobile = userName;
            Password = password;
            ValidDate = DateTime.Now;
            MemberLevelValue = 1;
            CustomerType = 1;
            UpdateMemberLevelTime = DateTime.Now;//首次创建默认为当前时间
        }
        /// <summary>
        /// 会员编号（预留字段，待确定会员编号规则）
        /// </summary>
        [DataMember]
        public string MemberCode { get; set; }
        /// <summary>
        /// 会员名称
        /// </summary>
        [DataMember]
        public string FullName { get; set; }
        /// <summary>
        /// 固定电话
        /// </summary>
        [DataMember]
        public string Tel { get; set; }
        /// <summary>
        /// 移动电话 (会员唯一标识)
        /// </summary>
        [DataMember]
        public string Mobile { get; set; }
        /// <summary>
        /// 等级值
        /// [长度：10]
        /// [不允许为空]
        /// </summary>
        [DataMember]
        public int MemberLevelValue { get; set; }
        /// <summary>
        /// 成长值
        /// [长度：10]
        /// [不允许为空]
        /// </summary>
        [DataMember]
        public int GrowthValue { get; set; }
        /// <summary>
        /// 积分
        /// [长度：10]
        /// [不允许为空]
        /// </summary>
        [DataMember]
        public int Integral { get; set; }

        /// <summary>
        /// 消费总额(只统计现金支付部分)
        /// [长度：19，小数位数：4]
        /// [不允许为空]
        /// </summary>
       [DataMember]
        public decimal TotalActualRMBPay { get; set; }

        /// <summary>
        /// 生日
        /// </summary>
        [DataMember]
        public DateTime? Birthday { get; set; }
        /// <summary>
        /// 验证日期
        /// </summary>
        [DataMember]
        public DateTime? ValidDate { get; set; }
        /// <summary>
        /// 邮箱
        /// </summary>
        [DataMember]
        public string Email { get; set; }
        /// <summary>
        /// 性别(1:男 2：女 3：保密（默认：1）)
        /// </summary>
        [DataMember]
        public int? Sex { get; set; }
        /// <summary>
        /// 客户类型 （注册会员=1，电话会员=2）
        /// </summary>
        [DataMember]
        public int? CustomerType { get; set; }
        /// <summary>
        /// 用户密码
        /// </summary>
        [DataMember]
        public string Password { get; set; }
        /// <summary>
        /// 最爱蛋糕
        /// </summary>
        [DataMember]
        public string FavoriteCake { get; set; }

        /// <summary>
        /// 修改密码时间戳
        /// </summary>
        [DataMember]
        public long? ChangePasswordTimeStamp { get; set; }
        /// <summary>
        /// 是否可用【0=可用；1=禁用】
        /// </summary>
        [DataMember]
        public short IsDisabled { get; set; }
        /// <summary>
        /// 更新会员等级变更时间（作业扣除成长值、订单完成时等级发生变化都需记录）
        /// </summary>
        [DataMember]
        public DateTime UpdateMemberLevelTime { get; set; }

        [DataMember]
        public virtual ICollection<CustomerAddress> CustomerAddresses { get; set; }
    }
}
