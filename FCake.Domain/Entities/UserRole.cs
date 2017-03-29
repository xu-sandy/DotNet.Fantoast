using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FCake.Domain.Entities
{
    [Serializable]
    [DataContract(IsReference = true)]
    public partial class UserRole
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string UserId { get; set; }
        [DataMember]
        public string RoleId { get; set; }
        [DataMember]
        public virtual Role Role { get; set; }
        [DataMember]
        public virtual User User { get; set; }
    }
}
