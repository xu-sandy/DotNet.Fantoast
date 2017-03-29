using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FCake.Domain.Entities
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class User : BaseEntity
    {
        public User() { }
        public void InitUser(string createdBy, string pwd)
        {
            Id = NewGuid();
            CreatedBy = createdBy;
            CreatedOn = DateTime.Now;
            Password = pwd;
        }
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public string FullName { get; set; }
        [DataMember]
        public string OfficeId { get; set; }
        [DataMember]
        public string Tel { get; set; }
        [DataMember]
        public string Mobile { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public int? Sex { get; set; }
        [DataMember]
        public int? IsDisabled { get; set; }
        [DataMember]
        public int? IsAdmin { get; set; }
        [DataMember]
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}