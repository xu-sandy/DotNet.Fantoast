using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCake.Domain.Entities;
using System.Data.Entity.ModelConfiguration;


namespace FCake.Domain.Mapping
{
    public class OrderDetailsMap : EntityTypeConfiguration<OrderDetails>
    {
        public OrderDetailsMap()
        {
            this.ToTable("OrderDetails");
            //HasRequired(o => o.Order).WithMany().HasForeignKey(od => od.OrderNo);
        }
    }
}
