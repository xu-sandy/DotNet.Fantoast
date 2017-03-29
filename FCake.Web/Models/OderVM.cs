using FCake.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FCake.Web.Models
{
    public class OderVM
    {
        public string Receiver { get; set; }
        public DateTime Time { get; set; }
        public string Tel { get; set; }
        public FeeType FeeType { get; set; }
        public string Address { get; set; }
        public decimal Pay { get; set; }
        public OrderStatus Status { get; set; }
    }
}