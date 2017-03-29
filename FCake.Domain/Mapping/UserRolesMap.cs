using FCake.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;

namespace FCake.Domain.Mapping
{
    public class UserRolesMap : EntityTypeConfiguration<UserRole>
    {
        public UserRolesMap()
        {
            this.ToTable("UserRoles");
            HasOptional(ur => ur.Role).WithMany().HasForeignKey(u => u.RoleId);
        }
    }
}
