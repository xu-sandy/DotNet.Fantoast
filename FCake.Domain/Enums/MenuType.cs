using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace FCake.Domain.Enums
{
    /// <summary>
    /// 菜单类型
    /// </summary>
    public enum MenuType : int
    {
        [Description("菜单")]
        System = 1,
        //[Description("网站设置")]
        //Web = 2,
        [Description("不显示")]
        Noshow = 3
    }
}
