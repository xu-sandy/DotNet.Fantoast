// --------------------------------------------------
// Copyright (C) 2015 版权所有
// 创 建 人：
// 创建时间：2015-12-02
// 描述信息：用于存放正在活动中的产品（每次发布活动中更新），加快查询速度
// --------------------------------------------------

using System;
using System.Runtime.Serialization;

namespace FCake.Domain.Entities
{
    /// <summary>
    /// 产品活动详情池
    /// </summary>
    [Serializable]
    public class ProductActivityDetailPool : BaseEntity
    {
        /// <summary>
        /// 产品Id
        /// [长度：40]
        /// [不允许为空]
        /// </summary>
       [DataMember]
        public string ProductId { get; set; }

        /// <summary>
        /// 子产品Id
        /// [长度：40]
        /// [不允许为空]
        /// </summary>
        [DataMember]
        public string SubProductId { get; set; }

        /// <summary>
        /// 产品活动Id
        /// [长度：40]
        /// [不允许为空]
        /// </summary>
        [DataMember]
        public string ProductActivityId { get; set; }

        /// <summary>
        /// 原产品销售价
        /// [长度：19，小数位数：4]
        /// [不允许为空]
        /// [默认值：((0))]
        /// </summary>
        [DataMember]
        public decimal ProductPrice { get; set; }

        /// <summary>
        /// 活动价
        /// [长度：19，小数位数：4]
        /// [不允许为空]
        /// [默认值：((0))]
        /// </summary>
        [DataMember]
        public decimal ActivityPrice { get; set; }

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
