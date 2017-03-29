using FCake.Admin.Helper;
using FCake.Admin.Models;
using FCake.Bll;
using FCake.Core.MvcCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace FCake.Admin.Controllers
{
    [AccessAuth]
    public class AccountController : Controller
    {
        FCake.Admin.Helper.FormAuthProvider authProvider = new FCake.Admin.Helper.FormAuthProvider();
        AccountService svc = new AccountService();
        //
        // GET: /Account/
        public ViewResult Login() {
            return View();
        }
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="model"></param>
        /// <param name="hidden1"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        
        [HttpPost]
        public ActionResult Login(LoginVM model,bool hidden1,string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (authProvider.Authenticate(model, hidden1, Request.Url.Host))
                {
                    if (!UserCache.CurrentUser.IsAdmin && UserCache.CurrentUser.Roles.Where(r => r.RoleCode == "kitchen" || r.RoleCode == "distribution").Count() > 0
                        && UserCache.CurrentUser.Roles.Where(r => r.RoleCode != "kitchen" && r.RoleCode != "distribution").Count() == 0)
                        return Redirect(returnUrl ?? Url.Action("Index", "Home", new { area="AreaPad"}));
                    return Redirect(returnUrl ?? Url.Action("Index", "Home"));
                }
                else
                {
                    ModelState.AddModelError("", "用户名或密码错误！");
                    return View();
                }
            }
            else
            {
                return View();
            }
        }
        /// <summary>
        /// 退出
        /// </summary>
        /// <returns></returns>
        public ActionResult LoginOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Account");
        }
    }
}
