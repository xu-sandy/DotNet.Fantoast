using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FCake.Domain.Entities
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class RolePermission:BaseEntity
    {
        [DataMember]
        public string RoleId { get; set; }
        [DataMember]
        public string PermissionId { get; set; }
    }
}
