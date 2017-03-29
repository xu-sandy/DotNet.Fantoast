using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using FCake.Domain.Entities;

namespace FCake.Domain.Mapping
{
    public class PrePayInfoMap :EntityTypeConfiguration<PrePayInfo>
    {
        public PrePayInfoMap()
        {
            this.ToTable("PrePayInfo");
        }
    }
}
