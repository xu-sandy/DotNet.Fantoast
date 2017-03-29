using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity.ModelConfiguration;

namespace FCake.Domain.Mapping
{
   public class KitchenBatchsMap:EntityTypeConfiguration<Entities.KitchenMake>
    {
       public KitchenBatchsMap()
       {
       this.ToTable("KitchenMakes");
       HasMany(km=>km.KitchenMakeDetails)
            .WithRequired(kmd=>kmd.KitchenMake)
            .HasForeignKey(t => t.KitchenMakeId);
       }
    }
}
