using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;

namespace FCake.Domain.Mapping
{
    public class CouponsMap : EntityTypeConfiguration<Entities.Coupons>
    {
        public CouponsMap()
        {
            this.ToTable("Coupons");
        }
    }
}
