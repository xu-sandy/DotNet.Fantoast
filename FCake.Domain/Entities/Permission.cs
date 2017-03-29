using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FCake.Domain.Entities
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Permission:BaseEntity
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Code { get; set; }
        [DataMember]
        public string MenuId { get; set; }
        [DataMember]
        public string PermissionType { get; set; }
        [DataMember]
        public string PermissionRegex { get; set; } 
    }
}
