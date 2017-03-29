using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace FCake.Domain.Enums
{
    /// <summary>
    /// 批次管理-审核状态
    /// </summary>
    public enum BatchReviewStatus
    {
        /// <summary>
        /// 待审核=0
        /// </summary>
        [Description("待审核")]
        ReviewPending = 0,
        /// <summary>
        /// 已审核=1
        /// </summary>
        [Description("已审核")]
        ReviewPass = 1
    }

    /// <summary>
    /// 批次管理-制作状态
    /// </summary>
    public enum OrderBatchMakeStatus : int
    {
        /// <summary>
        /// 未开始=0
        /// </summary>
        [Description("未开始")]
        NotStart = 0,

        /// <summary>
        /// 制作中=1
        /// </summary>
        [Description("制作中")]
        Making = 1,

        /// <summary>
        /// 已完成=2
        /// </summary>
        [Description("已完成")]
        Complete = 2,
    }
}
