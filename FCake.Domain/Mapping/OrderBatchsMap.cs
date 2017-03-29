using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;

namespace FCake.Domain.Mapping
{
    public class OrderBatchsMap : EntityTypeConfiguration<Entities.OrderBatch>
    {
        public OrderBatchsMap()
        {
            this.ToTable("OrderBatchs");
            HasMany(ob => ob.KitchenMakes)
                .WithRequired(k => k.OrderBatch)
                .HasForeignKey(t => t.BatchNo);
        }
    }
}
