using FCake.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FCake.Admin.Areas.AreaPad.Models
{
    public class DistributionDetail
    {
        public string Receiver { get; set; }
        public string ReceiverAddr { get; set; }
        public string ReceiverMobile { get; set; }
        public string ReceiverTel { get; set; }
        public string Customer { get; set; }
        public YesOrNo Candle { get; set; }

        public string ProductName { get; set; }
        public string Size { get; set; }
        public string SizeTitle { get; set; }
        public int? Num { get; set; }
        public decimal? TotalPrice { get; set; }
        public string BirthdayCard { get; set; }
    }
}