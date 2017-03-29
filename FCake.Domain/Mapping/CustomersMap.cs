using FCake.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;

namespace FCake.Domain.Mapping
{
    public class CustomersMap : EntityTypeConfiguration<Customers>
    {
        public CustomersMap()
        {
            this.ToTable("Customers");
            //HasMany(customer => customer.CouponsList)
            //    .WithOptional(Coupon => Coupon.Customer);

            //客户表对客户地址表，1对多
            HasMany(c => c.CustomerAddresses)
                .WithOptional()
                .HasForeignKey(ca => ca.CustomerId);
        }
    }
}
