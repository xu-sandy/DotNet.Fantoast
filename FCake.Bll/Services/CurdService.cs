using FCake.Domain;
using FCake.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCake.Core.Common;
using FCake.Domain.Enums;
using System.Text.RegularExpressions;

namespace FCake.Bll
{
    public class CurdService
    {
        static MenuService ms = new MenuService();

        #region Menu
        public string CheckCreate(Menu source, EFDbContext context)
        {
            try
            {
                //菜单名不能为空
                if (source.MenuName.IsNullOrTrimEmpty())
                    throw new Exception("菜单名不能为空");
                if (source.ParentId.IsNullOrTrimEmpty() == false)
                {
                    source.MenuType = source.MenuType != (int)MenuType.Noshow ? 0 : (int)MenuType.Noshow;
                }
                else if (source.MenuType == 0)
                {
                    throw new Exception("请选择一个菜单类型");
                }
                //权限代码 不能重复  可以为空
                if (source.MenuCode.IsNullOrTrimEmpty() == false)
                {
                    if (context.Menus.Any(a => a.MenuCode.Equals(source.MenuCode, StringComparison.OrdinalIgnoreCase) && a.IsDeleted != 1))
                        throw new Exception("权限代码不能重复");
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }
            return "";
        }
        public string CheckEdit(Menu source, EFDbContext context)
        {
            try
            {
                //菜单名不能为空
                if (source.MenuName.IsNullOrTrimEmpty())
                    throw new Exception("菜单名不能为空");

                if (source.ParentId.IsNullOrTrimEmpty() == false)
                {
                    source.MenuType = source.MenuType != (int)MenuType.Noshow ? 0 : (int)MenuType.Noshow;
                }
                else if (source.MenuType == 0)
                {
                    throw new Exception("请选择一个菜单类型");
                }
                //权限代码 不能重复  可以为空
                if (source.MenuCode.IsNullOrTrimEmpty() == false)
                {
                    if (context.Menus.Any(a => a.Id.Equals(source.Id) == false && a.MenuCode.Equals(source.MenuCode, StringComparison.OrdinalIgnoreCase) && a.IsDeleted != 1))
                        throw new Exception("权限代码不能重复");
                }
                //父ID不能为自已
                if (source.Id.Equals(source.ParentId))
                    throw new Exception("父ID不能与自身ID一致");

                //父ID不能为子孙ID
                var temp = ms.GetChildrenTree(context.Menus.ToList(), source.Id, source.Id);
                ms.CheckMenuParentId(temp, source.ParentId);
            }
            catch (Exception e)
            {
                return e.Message;
            }
            return "";
        }
        public string CheckDelete(Menu source, EFDbContext context)
        {
            try
            {
                if (context.Menus.Any(a => a.ParentId.Equals(source.Id) && a.IsDeleted != 1))
                    throw new Exception("该目录下还有数据 不能删除");
            }
            catch (Exception e)
            {
                return e.Message;
            }
            return "";
        }


        
        
        #endregion
        #region order
        public string CheckCreate(Orders source, EFDbContext context)
        {
            try
            {
                //用户名不能为空
                if (source.No.IsNullOrTrimEmpty())
                    throw new Exception("订单号不能为空");
            }
            catch (Exception e)
            {
                return e.Message;
            }
            return "";
        }
        public string CheckEdit(Orders source, EFDbContext context)
        {
            try
            {
                //用户名不能为空
                if (source.No.IsNullOrTrimEmpty())
                    throw new Exception("订单号不能为空");
            }
            catch (Exception e)
            {
                return e.Message;
            }
            return "";
        }
        public string CheckDelete(Orders source, EFDbContext context)
        {
            try
            {

            }
            catch (Exception e)
            {
                return e.Message;
            }
            return "";
        }
        #endregion
        #region Permission
        public string CheckCreate(Permission source, EFDbContext context)
        {
            try
            {
                if (source.Name.IsNullOrTrimEmpty())
                    throw new Exception("名称不能为空");
                if (source.Code.IsNullOrTrimEmpty())
                    throw new Exception("代码不能为空");
                if (context.Permissions.Any(a => a.Code.Equals(source.Code) && a.MenuId.Equals(source.MenuId) && a.IsDeleted != 1))
                {
                    throw new Exception("代码已存在：" + source.Name);
                }
                PermissionCache.Add(source);
            }
            catch (Exception e)
            {
                return e.Message;
            }
            return "";
        }
        public string CheckEdit(Permission source, EFDbContext context)
        {
            try
            {
                if (source.Name.IsNullOrTrimEmpty())
                    throw new Exception("名称不能为空");
                if (source.Code.IsNullOrTrimEmpty())
                    throw new Exception("代码不能为空");
                if (context.Permissions.Any(a => a.Code.Equals(source.Code) && a.Id.Equals(source.Id) == false && a.MenuId.Equals(source.MenuId) && a.IsDeleted != 1))
                {
                    throw new Exception("代码已存在：" + source.Name);
                }
                PermissionCache.Edit(source);
            }
            catch (Exception e)
            {
                return e.Message;
            }
            return "";
        }
        public string CheckDelete(Permission source, EFDbContext context)
        {
            try
            {
                PermissionCache.Delete(source);
            }
            catch (Exception e)
            {
                return e.Message;
            }
            return "";
        }
        #endregion
        #region Role
        public string CheckCreate(Role source, EFDbContext context)
        {
            try
            {
                //角色名不能为空
                if (source.Name.IsNullOrTrimEmpty())
                    throw new Exception("角色名不能为空");
                //角色代码不能为空
                if (source.Name.IsNullOrTrimEmpty())
                    throw new Exception("角色代码不能为空");
                if (context.Roles.Any(a => a.Code.Equals(source.Code, StringComparison.OrdinalIgnoreCase) && a.IsDeleted != 1))
                    throw new Exception("角色代码已重复");

            }
            catch (Exception e)
            {
                return e.Message;
            }
            return "";
        }
        public string CheckEdit(Role source, EFDbContext context)
        {
            try
            {
                //角色名不能为空
                if (source.Name.IsNullOrTrimEmpty())
                    throw new Exception("角色名不能为空");
                if (context.Roles.Any(a => a.Code.Equals(source.Code, StringComparison.OrdinalIgnoreCase) && a.Id.Equals(source.Id) == false && a.IsDeleted != 1))
                    throw new Exception("角色代码已重复");
                if (source.Code.Equals("supperadmin", StringComparison.OrdinalIgnoreCase))
                    throw new Exception("supperadmin为系统默认配置，不可删除与修改。");
                if (source.Code.Equals("kitchen", StringComparison.OrdinalIgnoreCase))
                    throw new Exception("kitchen为系统默认配置，不可删除与修改。");
                if (source.Code.Equals("distribution", StringComparison.OrdinalIgnoreCase))
                    throw new Exception("distribution为系统默认配置，不可删除与修改。");
            }
            catch (Exception e)
            {
                return e.Message;
            }
            return "";
        }
        public string CheckDelete(Role source, EFDbContext context)
        {
            try
            {
                if (source.Code.Equals("supperadmin", StringComparison.OrdinalIgnoreCase))
                    throw new Exception("supperadmin为系统默认配置，不可删除与修改。");
                if (source.Code.Equals("kitchen", StringComparison.OrdinalIgnoreCase))
                    throw new Exception("kitchen为系统默认配置，不可删除与修改。");
                if (source.Code.Equals("distribution", StringComparison.OrdinalIgnoreCase))
                    throw new Exception("distribution为系统默认配置，不可删除与修改。");
            }
            catch (Exception e)
            {
                return e.Message;
            }
            return "";
        }
        #endregion
        #region SubProduct

        public string CheckCreate(SubProduct source, EFDbContext context)
        {
            try
            {
                Regex reg = new Regex(@"^_");
                if (reg.IsMatch(source.ParentId))
                    source.ParentId = source.ParentId.Substring(1);
            }
            catch (Exception e)
            {
                return e.Message;
            }
            return "";
        }
        public string CheckEdit(SubProduct source, EFDbContext context)
        {
            try
            {
                Regex reg = new Regex(@"^_");
                if (reg.IsMatch(source.ParentId))
                    source.ParentId = source.ParentId.Substring(1);
            }
            catch (Exception e)
            {
                return e.Message;
            }
            return "";
        }
        public string CheckDelete(SubProduct source, EFDbContext context)
        {
            try
            {

            }
            catch (Exception e)
            {
                return e.Message;
            }
            return "";
        }
        #endregion
        #region User
        public string CheckCreate(User source, EFDbContext context)
        {
            try
            {
                //用户名不能为空
                if (source.UserName.IsNullOrTrimEmpty())
                    throw new Exception("用户名不能为空");
            }
            catch (Exception e)
            {
                return e.Message;
            }
            return "";
        }
        public string CheckEdit(User source, EFDbContext context)
        {
            try
            {
                //用户名不能为空
                if (source.UserName.IsNullOrTrimEmpty())
                    throw new Exception("用户名不能为空");
            }
            catch (Exception e)
            {
                return e.Message;
            }
            return "";
        }
        public string CheckDelete(User source, EFDbContext context)
        {
            try
            {

            }
            catch (Exception e)
            {
                return e.Message;
            }
            return "";
        }
        #endregion




        public object GetByID(object o, string id)
        {
            return (o as dynamic).Find(id);
        }
    }
}
