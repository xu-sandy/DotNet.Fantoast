using FCake.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FCake.Domain.Entities
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Slide:BaseEntity
    {
        /// <summary>
        /// 链接地址
        /// </summary>
        [DataMember]
        public string LinkUrl { get; set; }
        /// <summary>
        /// 图片地址
        /// </summary>
        [DataMember]
        public string Url { get; set; }
        /// <summary>
        /// 高度
        /// </summary>
        [DataMember]
        public int Height { get; set; }
        /// <summary>
        /// 宽度
        /// </summary>
        [DataMember]
        public int Width { get; set; }
        /// <summary>
        /// 长度
        /// </summary>
        [DataMember]
        public int Length { get; set; }
       /// <summary>
       /// 适用于(2=pc;1=mobile)
       /// </summary>
        [DataMember]
        public int? Apply { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        [DataMember]
        public SlideStatus Status { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        [DataMember]
        public int? SortOrder { get; set; }
    }
}
