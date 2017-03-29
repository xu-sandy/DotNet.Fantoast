using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace FCake.Domain.Enums
{
    public enum YesOrNo:int
    {
        [Description("是")]
        Yes=0,

        [Description("否")]
        No=1
    }
}
