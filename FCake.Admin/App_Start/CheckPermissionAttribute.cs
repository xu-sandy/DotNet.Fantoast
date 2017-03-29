using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FCake.Admin.Models;
using System.Text.RegularExpressions;
using FCake.Domain;
using FCake.Domain.Entities;
using FCake.Admin.Helper;
using FCake.Core.Common;
using FCake.Bll;

namespace FCake.Admin.Controllers
{
    public class CheckPermissionAttribute : ActionFilterAttribute
    {
        private string _controlName = "";
        private string _actionName = "";
        private string _permissionCode = "";
        private bool _isRelease = false;
        private bool _needMenuID = false;
        private bool _checkRegex = false;

        public string controlName { get { return _controlName; } set { _controlName = value; } }
        public string actionName { get { return _actionName; } set { _actionName = value; } }
        public string permissionCode { get { return _permissionCode; } set { _permissionCode = value; } }
        public bool isRelease { get { return _isRelease; } set { _isRelease = value; } }
        public bool needMenuID { get { return _needMenuID; } set { _needMenuID = value; } }
        public bool checkRegex { get { return _checkRegex; } set { _checkRegex = value; } }

        public bool IsBase = false;

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                if (IsBase)
                {
                    ResetStatus();
                }

                EFDbContext context = new EFDbContext();
                var permissions =new PermissionService().GetUserPermissions(UserCache.CurrentUser.Id);

                #region js查目录ID
                string checkpermissionid = filterContext.HttpContext.Request.Params["checkpermissionid"];
                if (checkpermissionid == "0")
                {
                    var m = GetCurrentPermissionMenu(filterContext);
                    HttpContext.Current.Session["ErrorCheckJson"] = (m==null?"":m.Id);
                    filterContext.Result = new RedirectResult("~/common/errorcheckjson");
                    return;
                }
                #endregion

                #region js查权限
                string checkpermission = filterContext.HttpContext.Request.Params["checkpermission"];
                if (checkpermission == "0")
                {
                    var m = GetCurrentPermissionMenu(filterContext);
                    if (m == null)
                    {
                        HttpContext.Current.Session["ErrorCheckJson"] = new { all = UserCache.CurrentUser.IsAdmin, data = new List<string> { } };
                        filterContext.Result = new RedirectResult("~/common/errorcheckjson");
                        return;
                    }

                    HttpContext.Current.Session["ErrorCheckJson"] = new { all = UserCache.CurrentUser.IsAdmin, data = permissions.Where(a => a.MenuId.Equals(m.Id)).Select(a => a.Code) };
                    filterContext.Result = new RedirectResult("~/common/errorcheckjson");
                    return;


                }
                #endregion



                if (isRelease||UserCache.CurrentUser.IsAdmin)
                    return;

                Menu menu = null;
                //根据目录ID
                if (needMenuID)
                {
                    string menuid = filterContext.HttpContext.Request.Params["permissionmenuid"] == null ? "" :
                        filterContext.HttpContext.Request.Params["permissionmenuid"].ToString();
                    if (menuid.IsNullOrTrimEmpty())
                        throw new Exception("权限不足，查无参数permissionmenuid");
                    menu = context.Menus.Find(menuid);
                    if (menu == null)
                    {
                        throw new Exception(string.Format("权限不足，查无数据：{0}", menuid));
                    }
                    var tempcode = "";
                    while (menu != null && menu.PMenuCode.IsNullOrTrimEmpty() == false)
                    {
                        tempcode = menu.PMenuCode;
                        menu = context.Menus.SingleOrDefault(a => a.MenuCode.Equals(menu.PMenuCode, StringComparison.OrdinalIgnoreCase));
                    }
                    if (menu == null)
                    {
                        throw new Exception(string.Format("权限未配置：{0}", tempcode));
                    }
                    //以正则判断
                    if (checkRegex)
                    {
                        var ps = permissions.Where(a => a.MenuId.Equals(menu.Id)&&a.PermissionRegex!=null&&a.PermissionRegex!="");
                        if(ps.Any()==false)
                            throw new Exception(string.Format("权限不足(正则判断)：{0}", menu.MenuName));
                        bool checkresult=false;
                        foreach (var x in ps)
                        {
                            var t = x.PermissionRegex.Split('&');
                            foreach (var y in t)
                            {
                                var t1 = y.Split('=');
                                if (t1.Length == 2)
                                {
                                    var ta = t1[0];
                                    var tb = t1[1];
                                    var c = filterContext.HttpContext.Request.Params[ta];
                                    if (c == null)
                                        throw new Exception("权限不足 查无参数:"+ta);
                                    Regex reg = new Regex(tb);
                                    if (reg.IsMatch(c))
                                    {
                                        return;
                                    }
                                    else
                                    {
                                        throw new Exception("权限不足 参数 " + ta+" 值为 "+c);
                                    }
                                }
                            }
                        }
                        if (checkresult == false)
                            throw new Exception("权限不足(正则无法匹配)");
                        return;
                    }
                    else
                    {
                        if (permissions.Any(a => a.MenuId.Equals(menu.Id) && a.Code.Equals(permissionCode)) == false)
                        {
                            throw new Exception(string.Format("权限不足：{0} {1} {2}", menu.MenuName, tempcode, permissionCode));
                        }
                    }
                    return;
                }





                //默认查看权限
                permissionCode = permissionCode.IsNullOrTrimEmpty() ? "view" : permissionCode.Trim();

                string userid = UserCache.CurrentUser.Id;
                if (controlName.IsNullOrTrimEmpty() || actionName.IsNullOrTrimEmpty())
                {
                    controlName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
                    actionName = filterContext.ActionDescriptor.ActionName;
                }
                var menucode=("/"+controlName+"/"+actionName).ToLower();

                menu = context.Menus.FirstOrDefault(a => a.MenuCode.ToLower().Equals(menucode));
                if (menu == null)
                {
                    throw new Exception(string.Format("权限未配置：{0}",menucode));
                }
                while (menu != null && menu.PMenuCode.IsNullOrTrimEmpty() == false)
                {
                    menu = context.Menus.SingleOrDefault(a => a.MenuCode.Equals(menu.PMenuCode, StringComparison.OrdinalIgnoreCase));
                }
                if (menu == null)
                {
                    throw new Exception(string.Format("权限未配置：{0}", menucode));
                }

                if (permissions.Any(a => a.MenuId.Equals(menu.Id) && a.Code.Equals(permissionCode)) == false)
                {
                    throw new Exception(string.Format("权限不足：{0} {1} {2}", menu.MenuName, menucode, permissionCode));
                }
            }
            catch (Exception ex)
            {
                if (filterContext.RequestContext.HttpContext.Request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
                {
                    base.OnActionExecuting(filterContext);
                    HttpContext.Current.Session["ErrorMsg"] = ex.Message;
                    filterContext.Result = new RedirectResult("~/common/error");
                }
                else
                {
                    HttpContext.Current.Session["ErrorJson"] = new { validate = false, msg = ex.Message };
                    filterContext.Result = new RedirectResult("~/common/errorjson");
                }
            }
        }

        private Menu GetCurrentPermissionMenu(ActionExecutingContext filterContext)
        {
            EFDbContext context = new EFDbContext();
            Menu menu = null;
            //
            if (needMenuID)
            {
                string menuid = filterContext.HttpContext.Request.Params["permissionmenuid"] == null ? "" :
                        filterContext.HttpContext.Request.Params["permissionmenuid"].ToString();
                if (menuid.IsNullOrTrimEmpty())
                    return null;
                menu = context.Menus.Find(menuid);
                if (menu == null)
                    return null;
                var tempcode = "";
                while (menu != null && menu.PMenuCode.IsNullOrTrimEmpty() == false)
                {
                    tempcode = menu.PMenuCode;
                    menu = context.Menus.SingleOrDefault(a => a.MenuCode.Equals(menu.PMenuCode, StringComparison.OrdinalIgnoreCase));
                }
                return menu;
            }

            string userid = UserCache.CurrentUser.Id;
            if (controlName.IsNullOrTrimEmpty() || actionName.IsNullOrTrimEmpty())
            {
                controlName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
                actionName = filterContext.ActionDescriptor.ActionName;
            }
            var menucode = ("/" + controlName + "/" + actionName);

            menu = context.Menus.FirstOrDefault(a => a.MenuCode.Equals(menucode,StringComparison.OrdinalIgnoreCase));

            while (menu != null && menu.PMenuCode.IsNullOrTrimEmpty() == false)
            {
                menu = context.Menus.SingleOrDefault(a => a.MenuCode.Equals(menu.PMenuCode, StringComparison.OrdinalIgnoreCase));
            }

            return menu;
        }

        private void ResetStatus()
        {
            _controlName = "";
            _actionName = "";
            _permissionCode = "";
            _isRelease = false;
            _needMenuID = false;
            _checkRegex = false;
        }
    }
}