using FCake.Core.Common;
using FCake.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace FCake.Domain.WebModels
{
    public class ProductListModel
    {
        //产品列表信息
        public PageList<Product> ProductPageList { get; set; }
        public PageList<VProductModel> VProductModels { get; set; }
        /// <summary>
        /// 当前搜索的产品类型
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 当前搜索的产品主题
        /// </summary>
        public string Themes { get; set; }
        public string ImgFlag { get; set; }
    }

    /// <summary>
    /// 产品列表，产品热卖Model
    /// </summary>
    public class VProductModel
    {
        /// <summary>
        /// ProductId
        /// </summary>
        public string ProductId { get; set; }
        /// <summary>
        /// 子产品Id
        /// </summary>
        public string SubProductId { get; set; }
        /// <summary>
        /// 产品名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 产品英文名
        /// </summary>
        public string EnglishName { get; set; }

        /// <summary>
        /// 产品类型
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 缩略图外键Id
        /// </summary>
        public string ImgUrl { get; set; }
        /// <summary>
        /// 产品主题
        /// </summary>
        public string Themes { get; set; }
        /// <summary>
        /// 上架时间
        /// </summary>
        public DateTime? SaleOn { get; set; }
        /// <summary>
        /// 是否推荐 1=推荐
        /// </summary>
        public int? IsRecommend { get; set; }
        /// <summary>
        /// 排序号前台列表根据此字段降序排列
        /// </summary>
        public int? SortNo { get; set; }
        /// <summary>
        /// 定价
        /// </summary>
        public decimal? OriginalPrice { get; set; }
        /// <summary>
        /// 销售价
        /// </summary>
        public decimal? Price { get; set; }
        /// <summary>
        /// 列表简述
        /// </summary>
        public string Brief { get; set; }/// <summary>
        /// 制作主要原料
        /// </summary>
        public string Material { get; set; }
        public string Size { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreatedOn { get; set; }
        /// <summary>
        /// 产品温馨提示
        /// </summary>
        public string WarmTips { get; set; }
        /// <summary>
        /// 产品规格单位字典值名称
        /// </summary>
        public string SizeTitle { get; set; }
        /// <summary>
        /// 扩展属性：是否存在活动(0:否，1：是)
        /// </summary>
        [NotMapped]
        public int isHasActivity { get; set; }
        /// <summary>
        /// 活动价
        /// </summary>
        [NotMapped]
        public decimal? ActivityPrice { get; set; }
    }
}