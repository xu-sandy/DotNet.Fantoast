using FCake.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;

namespace FCake.Domain.Mapping
{
    public class UsersMap : EntityTypeConfiguration<User>
    {
        public UsersMap()
        {
            this.ToTable("Users");
            HasMany(u => u.UserRoles).WithOptional(ur => ur.User).HasForeignKey(r => r.UserId);
        }
    }
}
