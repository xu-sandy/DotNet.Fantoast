using FCake.Domain;
using FCake.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;
using FCake.Core.Common;
using FCake.Core.MvcCommon;

namespace FCake.Bll
{
    public class PermissionService:BaseService
    {
        private EFDbContext context = new EFDbContext();
        /// <summary>
        /// 获取页面上的所有权限
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public List<string> GetPermissionCodes(RouteData view,string userid)
        {
            var menu = GetMenuByView(view,userid);
            if (menu == null)
                return new List<string>();

            var permissions = GetUserPermissions(userid);
            return
                permissions.Where(a => a.MenuId.Equals(menu.Id)&&a.IsDeleted!=1).Select(a => a.Code).ToList();
        }
        /// <summary>
        /// 由controlname及actionname获取权限代码
        /// </summary>
        /// <param name="controlname"></param>
        /// <param name="actionname"></param>
        /// <returns></returns>
        public List<string> GetPermissionCodes(string controlname, string actionname,string userid)
        {
            var menu = GetMenuByUrl(controlname, actionname, userid);
            if (menu == null)
                return new List<string>();

            var permissions = GetUserPermissions(userid);
            return
                permissions.Where(a => a.MenuId.Equals(menu.Id) && a.IsDeleted != 1).Select(a => a.Code).ToList();
        }


        private Menu GetMenuByView(RouteData data,string userid)
        {
            EFDbContext context = new EFDbContext();
            Menu menu = null;

            var menucode = ("/" + data.Values["controller"] + "/" + data.Values["action"]);

            menu = context.Menus.FirstOrDefault(a => a.MenuCode.Equals(menucode, StringComparison.OrdinalIgnoreCase) && a.IsDeleted != 1);

            while (menu != null && menu.PMenuCode.IsNullOrTrimEmpty() == false)
            {
                menu = context.Menus.SingleOrDefault(a => a.MenuCode.Equals(menu.PMenuCode, StringComparison.OrdinalIgnoreCase) && a.IsDeleted != 1);
            }

            return menu;
        }
        private Menu GetMenuByUrl(string controlname, string actionname,string userid)
        {
            EFDbContext context = new EFDbContext();
            Menu menu = null;

            var menucode = ("/" + controlname + "/" + actionname);

            menu = context.Menus.FirstOrDefault(a => a.MenuCode.Equals(menucode, StringComparison.OrdinalIgnoreCase) && a.IsDeleted != 1);

            while (menu != null && menu.PMenuCode.IsNullOrTrimEmpty() == false)
            {
                menu = context.Menus.SingleOrDefault(a => a.MenuCode.Equals(menu.PMenuCode, StringComparison.OrdinalIgnoreCase) && a.IsDeleted != 1);
            }

            return menu;
        }

        public List<Permission> GetUserPermissions(string userid)
        {
            if (PermissionCache.PermissionData == null)
            {
                    PermissionCache.ResetPermissionCache(context.Permissions.Where(a=>a.IsDeleted!=1).ToList(),
                        context.UserRoles.ToList(),
                        context.RolePermissions.ToList());
                
            }
            else
            {
                if (PermissionCache.time == null || PermissionCache.time.AddHours(PermissionCache.addtime) < DateTime.Now)
                {
                        PermissionCache.time = DateTime.Now;
                        PermissionCache.ResetPermissionCache(context.Permissions.Where(a => a.IsDeleted != 1).ToList(),
                            context.UserRoles.ToList(),
                            context.RolePermissions.ToList());
                    
                }
            }

            return (from x in PermissionCache.PermissionData.Permissions
                    join y in PermissionCache.PermissionData.RolePermissions on x.Id equals y.PermissionId
                    join z in PermissionCache.PermissionData.UserRoles on y.RoleId equals z.RoleId
                    join u in context.Roles on z.RoleId equals u.Id
                    where z.UserId.Equals(userid, StringComparison.OrdinalIgnoreCase)&&
                    x.IsDeleted!=1&&u.IsDeleted!=1
                    select x).Distinct().ToList();
        }

        public IQueryable<RolePermission> GetPermissionsByRoleID(string roleid)
        {
            roleid = roleid.IsNullOrTrimEmpty() ? "" : roleid.ToUpper();
            return context.RolePermissions.Where(a => a.RoleId.Equals(roleid));
        }

        public void InitBasePermissions(string menuId)
        {
            List<string> basepermissions = new List<string> { "添加", "删除", "修改", "查看" };
            var menupermissions = base.DAL.GetQuery<Permission>(p => p.MenuId.Equals(menuId));
            var add = false;
            var entity = base.DAL.Entities<Permission>();
            foreach (var x in basepermissions)
            {
                //如果找不到初始化权限则新增进去
                if (menupermissions.Any(a => a.Name.Equals(x)) == false)
                {
                    add = true;
                    entity.Add(new Permission
                    {
                        Id = FCake.Core.Common.DataHelper.GetSystemID(),
                        Name = x,
                        Code = (x == "查看" ? "view" : (x == "删除" ? "delete" : (x == "修改" ? "edit" : (x == "添加" ? "add" : "")))),
                        MenuId = menuId,
                        PermissionType = x == "查看" ? "查看" : "全部"
                    });
                }
            }
            if (add)
                base.DAL.Commit();
        }

        public dynamic GetAllPermission()
        {
            var result = (from x in context.Permissions
                         join y in context.Menus on x.MenuId equals y.Id
                         where y.MenuUrl != null && y.MenuUrl != "" && (y.PMenuCode == null || y.PMenuCode == "")
                         select new
                         {
                             Id = x.Id,
                             MenuName = y.MenuName,
                             PermissionName = x.Name,
                             PermissionCode = x.Code,
                             PermissionType = x.PermissionType,
                             MenuId = y.Id
                         }).OrderBy(a => a.MenuId);
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="permissionsStr"></param>
        /// <returns></returns>
        public object Save(string roleId, string permissionsStr)
        {
            var permissions = permissionsStr.Split(',').ToList();
            Role role = this.DAL.Find<Role>(roleId);
            if (role != null)
            {
                //删除
                var dels = context.RolePermissions.Where(a => a.RoleId.Equals(roleId) && permissions.Contains(a.PermissionId) == false);
                foreach (var x in dels)
                {
                    context.RolePermissions.Remove(x);
                }
                //添加
                var temp = context.RolePermissions.Where(a => a.RoleId.Equals(roleId)).Select(a => a.PermissionId);
                var adds = permissions.Where(a => temp.Contains(a) == false).ToList();
                adds = context.Permissions.Where(a => adds.Contains(a.Id)).Select(a => a.Id).ToList();
                foreach (var x in adds)
                {
                    context.RolePermissions.Add(new RolePermission
                    {
                        Id = FCake.Core.Common.DataHelper.GetSystemID(),
                        PermissionId = x,
                        RoleId = roleId
                    });
                }

                context.SaveChanges();
                PermissionCache.ResetRolePermissions(roleId, context.RolePermissions.Where(a => a.RoleId.Equals(roleId)).ToList());
                return OpResult.Success("数据保存成功");
            }
            return OpResult.Fail("数据保存错误");
        }
    }
}
