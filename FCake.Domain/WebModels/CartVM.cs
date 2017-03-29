using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FCake.Domain.WebModels
{
    public class CartVM
    {
        public string Id { get; set; }
        public string PName { get; set; }//产品名称
        public string PID { get; set; }//主表id
        public string Url { get; set; }
        public string Size { get; set; }
        public string SizeTitle { get; set; }
        /// <summary>
        /// 产品类型
        /// </summary>
        public string ProductType { get; set; }
        public decimal Price { get; set; }//销售价
        public decimal? OriginalPrice { get; set; }//定价
        public int Num { get; set; }
        public string BirthdayCard { get; set; }
        public string CartID { get; set; }
        public string LinkID { get; set; }
        public string CreatedBy { get; set; }
        /// <summary>
        /// 需提前几小时，默认为系统配置（若Product表保存为null则取0，计算时取系统默认作比较，取最大值）
        /// </summary>
        public int InadvanceHours { get; set; }
    }
}