using FCake.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace FCake.Domain.Entities
{
    [Serializable]
    [DataContract(IsReference=true)]
    public class Role : BaseEntity
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Code { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        [DataMember]
        public ActiveState Status { get; set; }
        [DataMember]
        public string Remark { get; set; }
    }
}