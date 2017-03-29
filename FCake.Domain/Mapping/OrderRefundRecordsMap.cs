using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCake.Domain.Entities;
using System.Data.Entity.ModelConfiguration;

namespace FCake.Domain.Mapping
{
    class OrderRefundRecordsMap : EntityTypeConfiguration<OrderRefundRecord>
    {
        public OrderRefundRecordsMap()
        {
            this.ToTable("OrderRefundRecords");
            //HasOptional(c => c.Customer).WithMany().HasForeignKey(p => p.CustomerId);
        }
    }
}
