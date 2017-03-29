using FCake.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;

namespace FCake.Domain.Mapping
{
    public class OperationLogMap : EntityTypeConfiguration<OperationLog>
    {
        public OperationLogMap()
        {
            ToTable("OperationLogs");
        }
    }
}
