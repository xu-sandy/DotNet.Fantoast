using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace FCake.Domain.Enums
{

    /// <summary>
    /// 代金卡状态
    /// </summary>
    public enum CouponsStatus
    {

        /// <summary>
        /// 生成一个暂未发行的代金卡
        /// </summary>
        [Description("未发行")]
        Init = 0,

        /// <summary>
        /// 审核不通过的已生成的代金卡信息，取消发行
        /// </summary>
        [Description("取消发行")]
        CancelPublished = 1,
        
        /// <summary>
        /// 已经发行，并且可以充值的代金卡
        /// </summary>
        [Description("已发行")]
        CanRecharge = 2,

        /// <summary>
        /// 已经被使用的代金卡
        /// </summary>
        [Description("已回收")]
        Recharged = 3
    }


    /// <summary>
    /// 代金卡使用状态
    /// </summary>
    public enum GiftCardUseStatus
    {

        /// <summary>
        /// 未使用
        /// </summary>
        [Description("未使用")]
        UnUsed = 0,

        /// <summary>
        /// 已使用
        /// </summary>
        [Description("已使用")]
        Used = 1,
                
        /// <summary>
        /// 已回收
        /// </summary>
        [Description("已回收")]
        Recharged = 2
    }
}
