using FCake.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FCake.Domain.Entities
{
    [Serializable]
    [DataContract(IsReference=true)]
    public partial class SubProduct : BaseEntity
    {
        [DataMember]
        public string ParentId { get; set; }
        [DataMember]
        public string Size { get; set; }
        /// <summary>
        /// 原定价字段，废弃，但不删除，存储时存储与Price相同的值
        /// </summary>
        [DataMember]
        public decimal? OriginalPrice { get; set; }

        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string ImgUrl { get; set; }
        [DataMember]
        public string Desc { get; set; }
        /// <summary>
        /// 原销售价字段，现展示为定价
        /// </summary>
        [DataMember]
        public decimal? Price { get; set; }
        [DataMember]
        public virtual Product Product { get; set; }
        [DataMember]
        public int? Status { get; set; }
    }
}
