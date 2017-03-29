using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FCake.Domain.Entities
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class DictionaryData : BaseEntity
    {
        
        /// <summary>
        /// 关联主表DictionaryType  ID
        /// </summary>
        [DataMember]
        public string DicCode { get; set; }
        /// <summary>
        /// 字典分组
        /// </summary>
        [DataMember]
        public string GroupName { get; set; }
        /// <summary>
        /// 字典值名称
        /// </summary>
        [DataMember]
        public string Name { get; set; }
        /// <summary>
        /// 字典值value
        /// </summary>
        [DataMember]
        public string Value { get; set; }
        /// <summary>
        /// 字典值子项
        /// </summary>
        [DataMember]
        public string ParentId { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        [DataMember]
        public double? Sorting { get; set; }

        /// <summary>
        /// 状态（0:启用，1：禁用）默认为0
        /// </summary>
        [DataMember]
        public int State { get; set; }
        [DataMember]
        /// <summary>
        /// 数据字典描述
        /// </summary>
        public string Description { get; set; }
    }
}
