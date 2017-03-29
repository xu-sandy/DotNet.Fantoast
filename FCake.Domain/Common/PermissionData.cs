using FCake.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FCake.Domain.Common
{
    public class PermissionData
    {
        public List<Permission> Permissions { get; set; }
        public List<RolePermission> RolePermissions { get; set; }
        public List<UserRole> UserRoles { get; set; }
    }
}
