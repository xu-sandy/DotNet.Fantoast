using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FCake.Domain.Entities
{
    public class PayLog
    {
        public string ID { get; set; }
        public string OrderNo { get; set; }
        public DateTime LogTime { get; set; }
        public string Message { get; set; }
    }
}
