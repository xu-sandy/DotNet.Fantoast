using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;

namespace FCake.Domain.Mapping
{
    public class ProductsMap : EntityTypeConfiguration<Entities.Product>
    {
        public ProductsMap() {
            this.ToTable("Products");
            HasOptional(b => b.BaseFile).WithMany().HasForeignKey(p => p.MainImgId);
            HasMany(t => t.SubProducts)
                .WithRequired(t => t.Product)
                .HasForeignKey(t => t.ParentId);
        }
    }
    
}
