using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace FCake.Domain.Enums
{
    /// <summary>
    /// 幻灯片状态
    /// </summary>
    public enum SlideStatus:int
    {
        /// <summary>
        /// 启用
        /// </summary>
        [Description("启用")]
        Active = 0,
        /// <summary>
        /// 禁用
        /// </summary>
        [Description("禁用")]
        NoActive=1
    }
}
