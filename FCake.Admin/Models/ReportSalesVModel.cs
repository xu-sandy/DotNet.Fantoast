using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FCake.Admin.Models
{
    public class ReportSalesVModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string GroupId { get; set; }
        public string GroupName { get; set; }
        public decimal? TotalPrice { get; set; }
        public int? Num { get; set; }
        public int? IsData { get; set; }
        public double? Sorting { get; set; }
    }
}