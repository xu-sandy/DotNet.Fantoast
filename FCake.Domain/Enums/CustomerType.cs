using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace FCake.Domain.Enums
{
    public enum CustomerType : int
    {
        [Description("注册会员")]
        RegediteredCustomer = 1,

        [Description("电话会员")]
        PhoneCustomer = 2
    }
}
