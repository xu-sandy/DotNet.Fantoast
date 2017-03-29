using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FCake.Domain.Entities
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Menu : BaseEntity
    {
        [DataMember]
        public string MenuName { get; set; }
        [DataMember]
        public string MenuUrl { get; set; }
        [DataMember]
        public string ParentId { get; set; }
        [DataMember]
        public int MenuType { get; set; }
        [DataMember]
        public string MenuCode { get; set; }
        [DataMember]
        public string PMenuCode { get; set; }
        [DataMember]
        public int? ShowOrder { get; set; }
    }
}
