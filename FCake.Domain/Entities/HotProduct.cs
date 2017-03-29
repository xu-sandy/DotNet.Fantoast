using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FCake.Domain.Entities
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class HotProduct : BaseEntity
    {
        [DataMember]
        public string ProductID { get; set; }
        [DataMember]
        public int? Num { get; set; }
    }
}
