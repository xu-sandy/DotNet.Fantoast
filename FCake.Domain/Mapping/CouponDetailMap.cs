using System;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;

namespace FCake.Domain.Mapping
{
    class CouponDetailMap : EntityTypeConfiguration<Entities.CouponDetail>
    {
        public CouponDetailMap()
        {
            this.ToTable("CouponDetail");
        }
    }
}
