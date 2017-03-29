using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FCake.Domain.Entities
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class MsgTemplate : BaseEntity
    {
        /// <summary>
        /// 类别（用来区分，审核，注册，配送）
        /// </summary>
        [DataMember]
        public string Category { get; set; }
        /// <summary>
        /// 类别中文名
        /// </summary>
        [DataMember]
        public string CategoryName { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        [DataMember]
        public string content { get; set; }
    }
}
