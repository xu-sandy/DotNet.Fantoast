using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace FCake.Domain.Enums
{
    public enum BoolType:int
    {
        [Description("是")]
        True = 1,

        [Description("否")]
        False = 0
    }
}
