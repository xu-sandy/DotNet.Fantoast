// --------------------------------------------------
// Copyright (C) 2015 版权所有
// 创 建 人：
// 创建时间：2015-12-02
// 描述信息：用于管理本系统中产品活动信息
// --------------------------------------------------

using System;
using System.Runtime.Serialization;

namespace FCake.Domain.Entities
{
	/// <summary>
	/// 产品活动
	/// </summary>
	[Serializable]
	public partial class ProductActivity:BaseEntity
	{
		/// <summary>
		/// 活动名称
		/// [长度：50]
		/// </summary>
		[DataMember]
        public string Title { get; set; }

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
		/// 发布状态（0：未发布，1：已发布）
		/// [长度：5]
		/// [不允许为空]
		/// [默认值：((0))]
		/// </summary>
		[DataMember]
        public short PublishStatus { get; set; }

	}
}
