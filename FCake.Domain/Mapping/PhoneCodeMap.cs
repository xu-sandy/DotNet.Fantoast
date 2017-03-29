using FCake.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;

namespace FCake.Domain.Mapping
{
    public class PhoneCodeMap : EntityTypeConfiguration<PhoneCode>
    {
        public PhoneCodeMap()
        {
            this.ToTable("PhoneCodes");
            HasOptional(t => t.Customer).WithMany().HasForeignKey(t => t.CustomerID);
        }
    }
}
