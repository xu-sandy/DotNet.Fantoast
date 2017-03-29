using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FCake.Admin.Models
{
    public class CycleTimeVM
    {

        public int ID { get; set; }
        public string ProductID { get; set; }
        public string SubProductID { get; set; }
        public string ProductName { get; set; }
        public int? Num { get; set; }
        public string Size { get; set; }
        public int MakeTimeDiff { get; set; }
        public DateTime OrderTime { get; set; }
        public string Gernre { get; set; }
        public int? Sort { get; set; }

    }
}