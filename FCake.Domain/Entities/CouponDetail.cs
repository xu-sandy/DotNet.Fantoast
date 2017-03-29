// --------------------------------------------------
// Copyright (C) 2015 版权所有
// 创 建 人：
// 创建时间：2015-11-30
// 描述信息：用于管理本系统中优惠券明细信息（主表：Coupons）
// --------------------------------------------------

using System;
using System.Runtime.Serialization;

namespace FCake.Domain.Entities
{
	/// <summary>
	/// 优惠券明细表
	/// </summary>
	[Serializable]
    [DataContract(IsReference = true)]
	public class CouponDetail:BaseEntity
	{

		/// <summary>
		/// 优惠券Id
		/// [长度：40]
		/// [不允许为空]
		/// </summary>
        [DataMember]
        public string CouponId { get; set; }
        /// <summary>
        /// 优惠券批次号
        /// 【长度：40】
        /// 【不允许空】
        /// 2015-12-24
        /// </summary>
        [DataMember]
        public string CouponBatch { get; set; }
		/// <summary>
		/// 优惠券号
		/// [长度：40]
		/// [不允许为空]
		/// </summary>
        [DataMember]
        public string CouponSN { get; set; }

		/// <summary>
		/// 拥有此券的会员Id
		/// [长度：40]
		/// </summary>
        [DataMember]
        public string MemberId { get; set; }

		/// <summary>
		/// 使用状态（0：未使用，1：已使用）
		/// [长度：5]
		/// [不允许为空]
		/// </summary>
        [DataMember]
        public short UseState { get; set; }

		/// <summary>
		/// 使用日期
		/// [长度：23，小数位数：3]
		/// </summary>
        [DataMember]
        public DateTime? UseDate { get; set; }

		/// <summary>
		/// 使用会员Id
		/// [长度：40]
		/// </summary>
        [DataMember]
        public string UseMemberId { get; set; }

        /// <summary>
        /// 使用订单编号
        /// [长度：40]
        /// </summary>
        [DataMember]
        public string UseOrderSN { get; set; }

		/// <summary>
		/// 券名
		/// [长度：50]
		/// </summary>
        [DataMember]
        public string Title { get; set; }

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

	
	}
}
