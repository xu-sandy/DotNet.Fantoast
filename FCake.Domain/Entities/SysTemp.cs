using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FCake.Domain.Entities
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class SysTemp
    {
        public SysTemp() { 
            Id= Guid.NewGuid().ToString("N").ToUpper();
            Code = Guid.NewGuid().ToString("N").ToUpper();
            CreatedOn = DateTime.Now;
        }
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public virtual string Code { get; set; }
        [DataMember]
        public DateTime CreatedOn { get; set; }
    }
}
