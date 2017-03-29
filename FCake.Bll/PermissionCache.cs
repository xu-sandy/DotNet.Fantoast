using FCake.Domain.Common;
using FCake.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FCake.Bll
{
    public class PermissionCache
    {
        public static DateTime time { get; set; }
        public static int addtime { get { return 1; } }
        private static PermissionData _permissionData = null;
        public static PermissionData PermissionData
        {
            get { return _permissionData; }
        }
        public static void ResetPermissionCache(List<Permission> permission=null,List<UserRole> userrole=null,List<RolePermission> rolepermission=null)
        {
            if (_permissionData == null)
                _permissionData = new PermissionData();
            if (_permissionData.Permissions == null)
                _permissionData.Permissions = permission.Where(a => a.IsDeleted != 1).ToList() ;
            if (_permissionData.UserRoles == null)
                _permissionData.UserRoles = userrole;
            if (_permissionData.RolePermissions == null)
                _permissionData.RolePermissions = rolepermission;
        }
        public static void Add(Permission permission)
        {
            if (_permissionData != null && _permissionData.Permissions != null&&permission.IsDeleted!=1)
                _permissionData.Permissions.Add(permission);
        }
        public static void Delete(Permission permission)
        {
            if (_permissionData != null && _permissionData.Permissions != null)
                _permissionData.Permissions.RemoveAll(a => a.Id.Equals(permission.Id));
        }
        public static void Edit(Permission permission)
        {
            Delete(permission);
            Add(permission);
        }
        public static void ResetUserRoles(string userid, List<UserRole> userroles)
        {
            if (_permissionData != null && _permissionData.UserRoles != null)
            {
                _permissionData.UserRoles.RemoveAll(a => a.UserId.Equals(userid, StringComparison.OrdinalIgnoreCase));
                _permissionData.UserRoles.AddRange(userroles);
            }
        }
        public static void ResetRolePermissions(string roleid,List<RolePermission> rolepermissions)
        {
            if (_permissionData != null && _permissionData.RolePermissions != null)
            {
                _permissionData.RolePermissions.RemoveAll(a => a.RoleId.Equals(roleid, StringComparison.OrdinalIgnoreCase));
                _permissionData.RolePermissions.AddRange(rolepermissions);
            }
        }
    }
}
