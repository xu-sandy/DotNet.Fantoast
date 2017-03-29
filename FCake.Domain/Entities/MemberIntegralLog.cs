// --------------------------------------------------
// Copyright (C) 2015 版权所有
// 创 建 人：
// 创建时间：2015-12-01
// 描述信息：用于记录本系统中会员积分的变更
// --------------------------------------------------

using System;
using System.Runtime.Serialization;

namespace FCake.Domain.Entities
{
	/// <summary>
	/// 积分记录表
	/// </summary>
	[Serializable]
	public class MemberIntegralLog:BaseEntity
	{
		/// <summary>
		/// 会员Id
		/// [长度：40]
		/// [不允许为空]
		/// </summary>
		[DataMember]
        public string MemberId { get; set; }

		/// <summary>
		/// 订单编号
		/// [长度：40]
		/// [不允许为空]
		/// </summary>
		[DataMember]
        public string OrderSN { get; set; }

		/// <summary>
		/// 积分变更值
		/// [长度：10]
		/// [不允许为空]
		/// </summary>
		[DataMember]
        public int ChangeIntegral { get; set; }

		/// <summary>
		/// 总积分值
		/// [长度：10]
		/// [不允许为空]
		/// </summary>
		[DataMember]
        public int TotalIntegral { get; set; }

		/// <summary>
		/// 现金消费金额
		/// [长度：19，小数位数：4]
		/// [不允许为空]
		/// </summary>
		[DataMember]
        public decimal CashAmount { get; set; }

		/// <summary>
		/// 积分倍数
		/// [长度：19，小数位数：4]
		/// [不允许为空]
		/// </summary>
		[DataMember]
        public decimal Multiple { get; set; }

        /// <summary>
        /// 备注
        /// [长度：40]
        /// </summary>
        [DataMember]
        public string Remark { get; set; }

	}
}
