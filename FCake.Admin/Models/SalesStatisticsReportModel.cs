using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FCake.Admin.Models
{
    public class SalesStatisticsReportModel
    {
        public string OMonth { get; set; }
        public string OrderClient { get; set; }
        public int OrderQuantity { get; set; }
        public decimal ActualPay { get; set; }
        public decimal CouponPay { get; set; }
        public decimal GiftCardPay { get; set; }
        public decimal IntegralPay { get; set; }
        public decimal TotalPrice { get; set; }





    }
}