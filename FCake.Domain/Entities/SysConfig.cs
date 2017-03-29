using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FCake.Domain.Entities
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class SysConfig
    {

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string Id { get; set; }
        /// <summary>
        /// 配置标识
        /// </summary>
        [DataMember]
        public string Code { get; set; }
        /// <summary>
        /// 配置值
        /// </summary>
        [DataMember]
        public string Value { get; set; }
        /// <summary>
        /// 父级配置标识
        /// </summary>
        [DataMember]
        public string PCode { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [DataMember]
        public string Note { get; set; }
    }
}
