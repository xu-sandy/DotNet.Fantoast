using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCake.Domain.Entities;
using System.Data.Entity.ModelConfiguration;


namespace FCake.Domain.Mapping
{
    public class OrdersMap : EntityTypeConfiguration<Orders>
    {
        public OrdersMap()
        {
            this.ToTable("Orders");
            HasKey(o => o.No);
            Property(p => p.No).IsRequired();
            HasMany(o => o.OrderDetails).WithRequired(o => o.Order).HasForeignKey(od => od.OrderNo);
            HasRequired(b => b.Customers).WithMany().HasForeignKey(p => p.CustomerId);
        }
    }
}
