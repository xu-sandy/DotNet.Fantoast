using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace FCake.Domain.Enums
{
    /// <summary>
    /// 审核状态
    /// </summary>
    public enum ReviewStatus
    {
        /// <summary>
        /// 待审核=0
        /// </summary>
        [Description("待审核")]
        ReviewPending = 0,
        /// <summary>
        /// 审核通过=1
        /// </summary>
        [Description("审核通过")]
        ReviewPass = 1,
        /// <summary>
        /// 审核未通过=2
        /// </summary>
        [Description("审核未通过")]
        ReviewReject = 2,
        /// <summary>
        /// 在线未支付=3
        /// </summary>
        [Description("在线未支付")]
        ReviewOnLineNoPay = 3,
        /// <summary>
        /// 已取消=99
        /// </summary>
        [Description("已取消")]
        Canceled = 99
    }
}
