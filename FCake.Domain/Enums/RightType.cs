using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace FCake.Domain.Enums
{
    /// <summary>
    /// 权限类型
    /// </summary>
    public enum RightType : int
    {
        [Description("正则")]
        Regex = 1,
        [Description("字符串")]
        String = 2,
        [Description("参数正则")]
        RequestRegex = 3
    }
}
