using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace FCake.Domain.Enums
{
    
    public enum ActiveStatus
    {
        [Description("启用")]
        Active,
        [Description("禁用")]
        UnActive
    }
}
