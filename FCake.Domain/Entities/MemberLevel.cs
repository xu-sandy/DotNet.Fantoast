// --------------------------------------------------
// Copyright (C) 2015 版权所有
// 创 建 人：
// 创建时间：2015-12-01
// 描述信息：用于管理本系统中的会员等级信息
// --------------------------------------------------

using System;
using System.Runtime.Serialization;

namespace FCake.Domain.Entities
{
	/// <summary>
	/// 会员等级表
	/// </summary>
	[Serializable]
	public class MemberLevel:BaseEntity
	{

		/// <summary>
		/// 名称
		/// [长度：50]
		/// </summary>
		[DataMember]
        public string Title { get; set; }

		/// <summary>
		/// 等级值
		/// [长度：10]
		/// [不允许为空]
		/// </summary>
        [DataMember]
        public int MemberLevelValue { get; set; }

		/// <summary>
		/// 最小成长值
		/// [长度：10]
		/// [不允许为空]
		/// </summary>
        [DataMember]
        public int MinGrowthValue { get; set; }

		/// <summary>
		/// 最大成长值
		/// [长度：10]
		/// [不允许为空]
		/// </summary>
		[DataMember]
        public int MaxGrowthValue { get; set; }

		/// <summary>
		/// 折扣率
		/// [长度：19，小数位数：4]
		/// [不允许为空]
		/// </summary>
		[DataMember]
        public decimal DiscountRate { get; set; }

		/// <summary>
		/// 积分倍数
		/// [长度：19，小数位数：4]
		/// [不允许为空]
		/// </summary>
		[DataMember]
        public decimal IntegralMultiples { get; set; }

		/// <summary>
		/// 成长值倍数
        /// [长度：19，小数位数：4]
		/// [不允许为空]
		/// </summary>
		[DataMember]
        public decimal GrowthValueMultiples { get; set; }

		/// <summary>
		/// 描述
		/// [长度：50]
		/// </summary>
		[DataMember]
        public string Remark { get; set; }
        /// <summary>
        /// 每年扣除的成长值
        /// </summary>
        [DataMember]
        public int YearDeductGrowthValue { get; set; }

	}
}
