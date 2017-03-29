using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FCake.Admin.Models;
using FCake.Domain.Entities;
using FCake.Bll;
using FCake.Core.Common;
using FCake.Domain;

namespace FCake.Admin.Controllers
{
    public class PermissionController : BaseController
    {
        private readonly BaseService _baseSvc = new BaseService();
        private readonly PermissionService _permissionSvc = new PermissionService();
        /// <summary>
        /// 单个菜单下的权限管理
        /// </summary>
        /// <returns></returns>
        [CheckPermission(controlName = "Permission", actionName = "MenuPermission",permissionCode="view")]
        public ActionResult Menu(string id)
        {
            ViewBag.id = id;
            Menu menu = this._baseSvc.Find<Menu>(id);
            if (menu != null)
            { 
                //如果没有任何权限 则初始化
                if (this._baseSvc.Any<Permission>(a => a.MenuId.Equals(id)) == false && menu.PMenuCode.IsNullOrTrimEmpty())
                {
                    //初始化权限
                    _permissionSvc.InitBasePermissions(id);
                }
            }
            return View(menu);
        }

        [CheckPermission(controlName = "Permission", actionName = "MenuPermission", permissionCode = "view")]
        [HttpPost]
        public ActionResult Menu_GetData(string id)
        {
            var result = this._baseSvc.GetQuery<Permission>(a=>a.MenuId.Equals(id)&&a.IsDeleted!=1);
            return Json(new { total = result.Count(), rows = result.OrderBy(a=>a.Name) });
        }

        /// <summary>
        /// 菜单权限分配
        /// </summary>
        /// <returns></returns>
        public ActionResult MenuPermission()
        {
            return View();
        }

        /// <summary>
        /// 角色权限分配
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns></returns>
        [CheckPermission(controlName = "Permission", actionName = "RoleSet", permissionCode = "edit")]
        public ActionResult RolePermission(string id)
        {
            ViewBag.id = id;
            return View();
        }

        public ActionResult RoleSet()
        {
            return View();
        }

        /// <summary>
        /// 获取某个角色下所有权限
        /// </summary>
        /// <param name="id">角色</param>
        /// <returns></returns>
        [CheckPermission(controlName = "Permission", actionName = "RoleSet", permissionCode = "view")]
        [HttpPost]
        public ActionResult GetAllPermission()
        {
            var result = this._permissionSvc.GetAllPermission() as IQueryable<dynamic>;
            
            return Json(new { total = result.Count(), rows = result });
        }

        [HttpPost]
        [CheckPermission(controlName = "Permission", actionName = "RoleSet", permissionCode = "edit")]
        public ActionResult SavePermission(string id, FormCollection c)
        {
            var rights = c["rightids[]"].IsNullOrTrimEmpty() ? "" : c["rightids[]"];
            var result = this._permissionSvc.Save(id,rights);
            return Json("");
        }
    }
}
