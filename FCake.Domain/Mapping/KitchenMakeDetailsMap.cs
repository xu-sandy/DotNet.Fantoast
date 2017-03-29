using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity.ModelConfiguration;

namespace FCake.Domain.Mapping
{
   public class KitchenBatchDetailsMap:EntityTypeConfiguration<Entities.KitchenMakeDetail>
    {
       public KitchenBatchDetailsMap() {
           this.ToTable("KitchenMakeDetails");
       }
    }
}
