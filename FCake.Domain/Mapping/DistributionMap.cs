using FCake.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;

namespace FCake.Domain.Mapping
{
    public class DistributionMap : EntityTypeConfiguration<Distribution>
    {
        public DistributionMap(){
            this.ToTable("Distribution");
            HasOptional(d => d.Order).WithMany().HasForeignKey(d => d.OrderNo);
        }
    }
}
