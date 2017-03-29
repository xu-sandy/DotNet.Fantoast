using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace FCake.Domain.Enums
{
    public enum Status : int
    {
        [Description("待审批")]
        ApprovalPending = 1,

        [Description("审批通过")]
        ApprovalPass = 2,

        [Description("审批不通过")]
        ApprovalReject = 3
    }

    public enum StatusDistribution : int
    {
        /// <summary>
        /// 未配送
        /// </summary>
        [Description("未配送")]
        DistributionPending = 0,
        /// <summary>
        /// 配送中
        /// </summary>
        [Description("配送中")]
        Distributing = 1,
        /// <summary>
        /// 配送完成
        /// </summary>
        [Description("配送完成")]
        Distributed = 2,
        /// <summary>
        /// 配送异常
        /// </summary>
        [Description("配送异常")]
        DistributionFalse = 3
    }
    /// <summary>
    /// 代金卡审核状态
    /// </summary>
    public enum GiftCardReviewStatus
    {
        /// <summary>
        /// 待审核=1
        /// </summary>
        [Description("待审核")]
        ReviewPending = 1,
        /// <summary>
        /// 审核通过=2
        /// </summary>
        [Description("审核通过")]
        ReviewPass = 2,
        /// <summary>
        /// 审核未通过=3
        /// </summary>
        [Description("审核未通过")]
        ReviewReject = 3
    }
}
