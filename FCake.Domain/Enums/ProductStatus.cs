using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace FCake.Domain.Enums
{
    public enum ProductStatus:int
    {
        /// <summary>
        /// 待提交
        /// </summary>
        [Description("提交")]
        ApprovalSubmit = 0,
        /// <summary>
        /// 待审批
        /// </summary>
        [Description("待审批")]
        ApprovalPending = 1,
        /// <summary>
        /// 审批通过
        /// </summary>
        [Description("待上架")]
        ApprovalPass = 2,
        /// <summary>
        /// 上架中的产品
        /// </summary>
        [Description("待下架")]
        ApprovalUnShelf = 3,
        /// <summary>
        /// 已下架的产品
        /// </summary>
        [Description("已下架")]
        ApprovalOffShelf = 4
    }
    public enum SaleStatus : int { 
        [Description("上架")]
        SaleOn = 1,
        [Description("下架")]
        SaleOff = 2,
    }
}
