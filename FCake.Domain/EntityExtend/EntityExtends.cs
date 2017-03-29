using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace FCake.Domain.Entities
{
    public partial class SubProduct
    {
        [NotMapped]
        public string SizeTitle { get; set; }
        /// <summary>
        /// 用于区分活动价或会员价
        /// </summary>
        [NotMapped]
        public string PriceTitle { get; set; }
    }
    public partial class ProductActivity
    {
        /// <summary>
        /// 活动的主产品id
        /// </summary>
        [NotMapped]
        public string ProductIds { get; set; }
        /// <summary>
        /// 活动的子产品id
        /// </summary>
        [NotMapped]
        public string ParentProductIds { get; set; }
        /// <summary>
        /// 活动产品原价
        /// </summary>
        [NotMapped]
        public string ProOrginPrice { get; set; }
        /// <summary>
        /// 活动产品的对应价格
        /// </summary>
        [NotMapped]
        public string ActivityProPrices { get; set; }
    }
  
}
