using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace FCake.Domain.Enums
{
    /// <summary>
    /// 启用禁用状态
    /// </summary>
    public enum ActiveState
    {
        /// <summary>
        /// 启用
        /// </summary>
        [Description("启用")]
        Enable = 0,
        /// <summary>
        /// 禁用
        /// </summary>
        [Description("禁用")]
        Disable = 1
    }
}
