using FCake.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;

namespace FCake.Domain.Mapping
{
    public class ReviewStatusLogMap : EntityTypeConfiguration<ReviewStatusLog>
    {
        public ReviewStatusLogMap()
        {
            //HasKey<string>(r => r.Id);
            //Property(r=>r.Id)
            //    .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None)
            //    .IsRequired();

            //Property(r => r.CreatedBy)
            //    .IsRequired()
            //    .HasMaxLength(50);
            //Property(r => r.CreatedOn)
            //    .IsRequired();
            //Property(r => r.ModifiedBy)
            //    .IsRequired()
            //    .HasMaxLength(50);
            //Property(r => r.ModifiedOn)
            //    .IsRequired();
            //Property(r => r.IsDeleted);
            //Property(r => r.TableName)
            //    .IsRequired()
            //    .HasMaxLength(50);
            //Property(r => r.TableNameId)
            //    .IsRequired()
            //    .HasMaxLength(50);
            //Property(r => r.Status);
            //Property(r => r.Remark);
        }
    }
}
