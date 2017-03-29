﻿using FCake.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;

namespace FCake.Domain.Mapping
{
    public class SysConfigMap : EntityTypeConfiguration<SysConfig>
    {
        public SysConfigMap()
        {
            this.ToTable("SysConfigs");
        }
    }
}
