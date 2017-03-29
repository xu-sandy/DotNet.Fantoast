using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace FCake.Domain.Enums
{
    public enum DeliveryType:int
    {
        /// <summary>
        /// 送货上门 door to door
        /// </summary>
        [Description("送货上门")]
        D2D=0,
        /// <summary>
        /// 站点自提
        /// </summary>
        [Description("站点自提")]
        FixedSite=1
    }
}
