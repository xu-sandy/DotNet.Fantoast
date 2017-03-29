using FCake.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FCake.Admin.Models
{
    public class ProductDetail
    {
        public string Name { get; set; }
        public string Taste { get; set; }
        public string Brief { get; set; }
        public string minUrl { get; set; }
        public ICollection<SubProduct> SubProduct { get; set; }
    }
}