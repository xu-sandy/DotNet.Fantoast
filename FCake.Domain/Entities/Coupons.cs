// --------------------------------------------------
// Copyright (C) 2015 版权所有
// 创 建 人：
// 创建时间：2015-11-30
// 描述信息：用于管理本系统中的优惠券信息
// --------------------------------------------------

using System;
using System.Runtime.Serialization;

namespace FCake.Domain.Entities
{
	/// <summary>
	/// 优惠券
	/// </summary>
	[Serializable]
    [DataContract(IsReference = true)]
	public class Coupons:BaseEntity
	{

		/// <summary>
		/// 券名
		/// [长度：50]
		/// </summary>
        [DataMember]
        public string Title { get; set; }

        /// <summary>
        /// 优惠券批次号
        /// 【长度：40】
        /// 【不允许空】
        /// 2015-12-24
        /// </summary>
        [DataMember]
        public string CouponBatch { get; set; }
		/// <summary>
		/// 面额
		/// [长度：19，小数位数：4]
		/// [不允许为空]
		/// </summary>
        [DataMember]
        public decimal Denomination { get; set; }

		/// <summary>
		/// 销售金额
		/// [长度：19，小数位数：4]
		/// [不允许为空]
		/// [默认值：((0))]
		/// </summary>
        [DataMember]
        public decimal SalesMoney { get; set; }

		/// <summary>
		/// 优惠券发放数量
		/// [长度：10]
		/// [不允许为空]
		/// </summary>
        [DataMember]
        public int Quantity { get; set; }

		/// <summary>
		/// 使用条件（0：无条件使用，大于0：满x元使用）
		/// [长度：19，小数位数：4]
		/// [不允许为空]
		/// </summary>
        [DataMember]
        public decimal ConditionMoney { get; set; }

		/// <summary>
		/// 有效期开始时间
		/// [长度：23，小数位数：3]
		/// [不允许为空]
		/// </summary>
        [DataMember]
        public DateTime BeginValidDate { get; set; }

		/// <summary>
		/// 有效期截至时间
		/// [长度：23，小数位数：3]
		/// [不允许为空]
		/// </summary>
        [DataMember]
        public DateTime EndValidDate { get; set; }

		/// <summary>
		/// 赠送方式（1：绑定用户，2：卡券发放）
		/// [长度：5]
		/// [不允许为空]
		/// </summary>
        [DataMember]
        public short GiveWay { get; set; }

		/// <summary>
		/// 赠送对象类型（1：全部，2：会员类型，3：指定用户）
		/// [长度：5]
		/// [不允许为空]
		/// </summary>
        [DataMember]
        public short GivenObjectType { get; set; }

		/// <summary>
        /// 赠送对象Id（赠送对象类型为1：null，赠送对象类型为2：会员等级值,会员等级值,……，赠送对象类型为3：会员Id,会员Id,……）
		/// [长度：200]
		/// </summary>
        [DataMember]
        public string GivenObjectIds { get; set; }

		/// <summary>
		/// 是否发送短信（0：否，1：是）
		/// [长度：5]
		/// [不允许为空]
		/// </summary>
        [DataMember]
        public short IsSendSMS { get; set; }

		/// <summary>
		/// 短信内容
		/// [长度：200]
		/// </summary>
        [DataMember]
        public string SMSContent { get; set; }

		/// <summary>
		/// 状态（0：未生成，1：已生成）
		/// [长度：5]
		/// [不允许为空]
		/// </summary>
        [DataMember]
        public short Status { get; set; }

        /// <summary>
        /// 发放类型（0：其他，1：注册营销），默认0
        /// [长度：5]
        /// [不允许为空]
        /// </summary>
        [DataMember]
        public short DistributingType { get; set; }
        /// <summary>
        /// 短信发送状态（0:未发送，1：已发送）默认为0
        /// </summary>
        [DataMember]
        public short SendSMSStatus { get; set; } 

	}
}
