using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FCake.Admin.Models
{
    public class SlideVM
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string LinkUrl { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public int Length { get; set; }
        public int Status { get; set; }
        public int Apply { get; set; }
        public int? SortOrder { get; set; }
    }
}