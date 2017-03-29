using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FCake.WebMobile.Models
{
    public class WXPayVM
    {
        public string AppId { get; set; }
        public string NonceStr { get; set; }
        public string Package { get; set; }
        public string signType { get; set; }
        public string timeStamp { get; set; }
        public string Key { get; set; }
        public string PaySign { get; set; }
    }
}