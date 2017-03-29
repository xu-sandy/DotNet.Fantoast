using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FCake.Domain.Entities
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class DictionaryType : BaseEntity
    {


        /// <summary>
        /// 字典名称
        /// </summary>
        [DataMember]
        public string Name { get; set; }
        /// <summary>
        /// 字典编码
        /// </summary>
        [DataMember]
        public string Code { get; set; }
    }
}
