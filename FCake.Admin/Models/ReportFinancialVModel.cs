using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FCake.Admin.Models
{
    public class ReportFinancialVModel
    {
        public string OrderNo { get; set; }
        public string CreatedOn { get; set; }
        public string FeeType { get; set; }
        public decimal? TotalPrice { get; set; }
        public decimal? RealPay { get; set; }
        public decimal? CouponPay { get; set; }
        public decimal? TotalMoney { get; set; }
        public int? sorting { get; set; }

    }
}