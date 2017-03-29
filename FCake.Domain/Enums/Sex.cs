using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace FCake.Domain.Enums
{
    public enum Sex:int
    {
        [Description("男")]
        Male = 1,

        [Description("女")]
        Female = 2,

        [Description("保密")]
        Secrecy = 3
    }
}
