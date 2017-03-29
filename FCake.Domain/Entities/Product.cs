using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FCake.Domain.Entities
{
    [Serializable]
    [DataContract(IsReference=true)]
    public class Product : BaseEntity
    {
        /// <summary>
        /// 产品名称
        /// </summary>
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        /// <summary>
        /// 产品英文名
        /// </summary>
        public string EnglishName { get; set; }
        /// <summary>
        /// 口味
        /// </summary>
        [DataMember]
        public string Taste { get; set; }
        /// <summary>
        /// 列表简述
        /// </summary>
        [DataMember]
        public string Brief { get; set; }
        /// <summary>
        /// 详情详述
        /// </summary>
        [DataMember]
        public string Expatiate { get; set; }
        /// <summary>
        /// 产品详情
        /// </summary>
        [DataMember]
        public string Desc { get; set; }
        /// <summary>
        /// 手机的产品详情
        /// </summary>
        [DataMember]
        public string MobileDesc { get; set; }
        /// <summary>
        /// 审批状态 待提交=0 待审批=1 待上架=2 待下架=3 已下架=4
        /// </summary>
        [DataMember]
        public int? Status { get; set; }
        /// <summary>
        /// 上架状态 上架=1 下架=2
        /// </summary>
        [DataMember]
        public int? SaleStatus { get; set; }
        /// <summary>
        /// 产品类型
        /// </summary>
        [DataMember]
        public string Type { get; set; }
        /// <summary>
        /// 缩略图外键Id
        /// </summary>
        [DataMember]
        public string MainImgId { get; set; }
        /// <summary>
        /// 产品主题
        /// </summary>
        [DataMember]
        public string Themes { get; set; }
        /// <summary>
        /// 上架时间
        /// </summary>
        [DataMember]
        public DateTime? SaleOn { get; set; }
        /// <summary>
        /// 下架时间
        /// </summary>
        [DataMember]
        public DateTime? SaleOff { get; set; }
        /// <summary>
        /// 是否推荐 1=推荐
        /// </summary>
        [DataMember]
        public int? IsRecommend { get; set; }
        /// <summary>
        /// 需提前几小时，只存int
        /// </summary>
        [DataMember]
        public int? InadvanceHours { get; set; }
        /// <summary>
        /// 产品温馨提示
        /// </summary>
        [DataMember]
        public string  WarmTips{ get; set; }
        /// <summary>
        /// 制作主要原料
        /// </summary>
        [DataMember]
        public string Material { get; set; }
        /// <summary>
        /// 排序号前台列表根据此字段降序排列
        /// </summary>
        [DataMember]
        public int? SortNo { get; set; }      
        /// <summary>
        /// 是否作为快捷按钮(0:否,1:是),当品种为其他时，是否出现在订单明细页面中,可快速添加
        /// [默认值:0]
        /// </summary>
        public int IsShortcutButton { get; set; }
        /// <summary>
        /// 快捷按钮显示名称
        /// </summary>
        public string ShortcutButtonTitle { get; set; }
        /// <summary>
        /// /缩略图一对一对象
        /// </summary>
        [DataMember]
        public virtual BaseFile BaseFile { get; set; } 
        /// <summary>
        /// 产品详细信息
        /// </summary>
        [DataMember]
        public virtual ICollection<SubProduct> SubProducts { get; set; }

       
    }
}
