using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FCake.Domain.Entities
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class BaseFile:BaseEntity
    {
        [DataMember]
        public string LinkId { get; set; }
        [DataMember]
        public string NewName { get; set; }
        [DataMember]
        public string OldName { get; set; }
        [DataMember]
        public string Url { get; set; }
        [DataMember]
        public string SuffixName { get; set; }
        [DataMember]
        public int? Length { get; set; }
        [DataMember]
        public int? Sorting { get; set; }
    }
}
