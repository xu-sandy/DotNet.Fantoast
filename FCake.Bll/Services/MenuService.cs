using FCake.Core.Common;
using FCake.Domain.Entities;
using FCake.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FCake.Bll
{
    public class MenuService:BaseService
    {
        public List<Dictionary<string, object>> ToMenuTree(List<Dictionary<string, object>> source,bool isadmin,string userid)
        {
            foreach (var x in source)
            {
                ToMenuTree(x);
            }

            #region 权限设置
            //如果不是系统超级管理员  那么菜单要根据权限变化
            if (isadmin == false)
            {
                var permissions = new PermissionService().GetUserPermissions(userid).Where(a => a.Code == "view");
                List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

                CheckMenuByPermissions(permissions, source, result);
                return RemoveEmptyMenus(result);
                //var rights = DataHelper.GetUserRights().Where(a=>a.RightType==(int)RightType.Regex);
                //List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

                //CheckMenuByRights(rights,source,result);
                //return RemoveEmptyMenus(result);
            }
            #endregion

            return source;
        }
        private Dictionary<string, object> ToMenuTree(Dictionary<string, object> source)
        {
            source.Add("id", source["Id"]);
            source.Remove("Id");
            source.Add("text", source["MenuName"]);
            source.Remove("MenuName");
            foreach (var x in source["children"] as List<Dictionary<string, object>>)
            {
                ToMenuTree(x);
            }
            return source;
        }
        #region 菜单权限筛选

        public List<Dictionary<string, object>> RemoveEmptyMenus(List<Dictionary<string, object>> source)
        {
            source = source.Where(a => !(
                (a["children"] == null && a["attributes"] != null)
                ||
                ((a["children"] as List<Dictionary<string, object>>).Any() == false && a["attributes"] == null)
                )
                ).ToList();
            foreach (var x in source)
            {
                if (x["children"] != null)
                {
                    var temp = RemoveEmptyMenus((x["children"] as List<Dictionary<string, object>>));
                    x.Remove("children");
                    x.Add("children", temp);
                }
            }
            source = source.Where(a => !(
                (a["children"] == null && a["attributes"] != null)
                ||
                ((a["children"] as List<Dictionary<string, object>>).Any() == false && a["attributes"] == null)
                )
                ).ToList();
            return source;
        }

        public void CheckMenuByPermissions(IEnumerable<Permission> rights, List<Dictionary<string, object>> source, List<Dictionary<string, object>> target)
        {
            foreach (var x in source)
            {
                CheckMenuByPermissions(rights, x, target);
            }
        }

        public void CheckMenuByPermissions(IEnumerable<Permission> rights, Dictionary<string, object> source, List<Dictionary<string, object>> target)
        {
            if (CheckMenuByPermissions(rights, source))
            {
                Dictionary<string, object> temp = new Dictionary<string, object>();
                foreach (var x in source)
                {
                    if (x.Key != "children")
                        temp.Add(x.Key, x.Value);
                    else
                    {
                        var t = x.Value as List<Dictionary<string, object>>;
                        var tg = new List<Dictionary<string, object>>();

                        CheckMenuByPermissions(rights, t, tg);

                        temp.Add(x.Key, tg);
                    }
                }

                target.Add(temp);
            }

        }

        public bool CheckMenuByPermissions(IEnumerable<Permission> rights, Dictionary<string, object> source)
        {
            var id = source["id"];
            if (source.Keys.Contains("attributes") == false || source["attributes"] == null)
                return true;
            Dictionary<string, object> attr = source["attributes"] as Dictionary<string, object>;
            if (rights.Any(a => a.MenuId.Equals(id)))
                return true;
            return false;
        }
        #endregion

        public List<Dictionary<string, object>> ToTree(IQueryable<Menu> menus, int p)
        {
            //menus = menus.Where(a => a.IsDeleted != 1);
            int noshow = (int)MenuType.Noshow;
            var m = menus.Where(a => ((a.ParentId == null || a.ParentId == "") && a.MenuType == p)
                || (a.ParentId != null && a.ParentId != "" && a.MenuType != noshow)
                || p == 0
                ).OrderBy(a => a.MenuType).ThenBy(a => a.ShowOrder).ToList();

            //var m = menus.Where(a=>a.MenuType==p||p==0).OrderBy(a=>a.MenuType).ToList();


            List<EasyUI_Tree<Menu>> result = new List<EasyUI_Tree<Menu>>();
            result.AddRange(
                m.Where(a => a.ParentId == null || a.ParentId == "").Select(
                a => new EasyUI_Tree<Menu>
                {
                    value = a,
                    children = GetChildrenTree(m, a.Id, a.Id),
                    attributes = GetMenuAttributes(a)
                }
                )
                );

            return FCake.Core.Common.DataHelper.ToTreeDic<Menu>(result);
        }
        public List<EasyUI_Tree<Menu>> GetChildrenTree(List<Menu> menus, string pid, string currentid)
        {
            pid = pid.IsNullOrTrimEmpty() ? "" : pid.Trim();
            List<EasyUI_Tree<Menu>> result = new List<EasyUI_Tree<Menu>>();


            result.AddRange(menus.Where(a => a.ParentId != null && a.ParentId.Equals(pid) && a.Id.Equals(currentid) == false)
                .Select(a => new EasyUI_Tree<Menu>
                {
                    value = a,
                    children = GetChildrenTree(menus, a.Id, currentid),
                    attributes = GetMenuAttributes(a)
                })
                );

            return result;
        }
        private Dictionary<string, object> GetMenuAttributes(Menu menu)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();

            if (menu.MenuUrl.IsNullOrTrimEmpty())
                return null;

            dic.Add("url", menu.MenuUrl);

            return dic;
        }
        public void CheckMenuParentId(List<EasyUI_Tree<Menu>> menus, string pid)
        {
            foreach (var x in menus)
            {
                CheckMenuParentId(x, pid);
            }
        }
        private void CheckMenuParentId(EasyUI_Tree<Menu> menu, string pid)
        {
            if ((menu.value as Menu).Id.Equals(pid))
                throw new Exception("父ID不能为原子孙ID");
            foreach (var x in menu.children)
            {
                CheckMenuParentId(x, pid);
            }
        }
    }
}
