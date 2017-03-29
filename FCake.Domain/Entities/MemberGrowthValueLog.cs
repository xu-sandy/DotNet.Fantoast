// --------------------------------------------------
// Copyright (C) 2015 版权所有
// 创 建 人：
// 创建时间：2015-12-01
// 描述信息：用于记录本系统中会员成长值的变更
// --------------------------------------------------

using System;
using System.Runtime.Serialization;

namespace FCake.Domain.Entities
{
    /// <summary>
    /// 成长值记录表
    /// </summary>
    [Serializable]
    public class MemberGrowthValueLog:BaseEntity
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
        /// 成长值变更值
        /// [长度：10]
        /// [不允许为空]
        /// </summary>
        [DataMember]
        public int ChangeGrowthValue { get; set; }

        /// <summary>
        /// 总成长值
        /// [长度：10]
        /// [不允许为空]
        /// </summary>
        [DataMember]
        public int TotalGrowthValue { get; set; }

        /// <summary>
        /// 现金消费金额
        /// [长度：19，小数位数：4]
        /// [不允许为空]
        /// </summary>
        [DataMember]
        public decimal CashAmount { get; set; }

        /// <summary>
        /// 成长值倍数
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
