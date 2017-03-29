using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FCake.Admin.Models;
using System.Data.Entity;
using FCake.Domain.Entities;
using FCake.Bll;
using FCake.Core.MvcCommon;
using FCake.Core.Common;
using FCake.Admin.Helper;
using FCake.Domain.Common;

namespace FCake.Admin.Controllers
{
    public class UserController : BaseController
    {
        //
        // GET: /User/
        UserService svc = new UserService();

        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult QueryData(int page = 1, int rows = 20)
        {
            var info = base.GetQueryInfo();
            int totalCount = 0;
            var pageInfo = new PageInfo() { Page = page, Rows = rows };
            var result = svc.GetUserDataBySearchInfo(info, pageInfo, out totalCount);
            GridJsonModel djson = new GridJsonModel(totalCount, result.Data);
            return Json(djson);
        }
        public JsonResult ResetPwd(string id, string password)
        {
            var result = svc.ResetPassword(id, password, Helper.UserCache.CurrentUser.Id);
            return Json(result);
        }
        public ViewResult UserEdit(string id = null)
        {
            var user = svc.GetUser(id);
            return View(user);
        }
        [HttpPost]
        public JsonResult SaveUser(User user, string Password1)
        {
            if (user.Id == null)
                user.Password = Password1;
            var result = svc.SaveUser(user, Helper.UserCache.CurrentUser.Id);
            return Json(result);
        }
        [HttpPost]
        public ActionResult CheckName(string username)
        {
            if (svc.UserNameExsist(username))
                return JqRemoteValidation.Invalid("用户名已被使用");
            else return JqRemoteValidation.Valid();
        }

        /// <summary>
        /// 用户角色选择
        /// </summary>
        /// <returns></returns>
        public ActionResult Role()
        {
            return View();
        }
        [CheckPermission(actionName = "Role", controlName = "User")]
        public ActionResult SelectRole(string detailid)
        {
            return View();
        }
        [HttpPost]
        public ActionResult SaveRoles(string id, FormCollection c)
        {
            var groupids = c["groupids[]"].IsNullOrTrimEmpty() ? "" : c["groupids[]"];
            RoleService.SaveRoles(id, groupids.Split(',').ToList());
            return Json("");
        }
        //[HttpPost]
        //public JsonResult GetRoleComboBoxJson()
        //{
        //    var list = (from roles in context.Roles
        //                where roles.IsDeleted != 1
        //                select new EasyuiCombo()
        //                {
        //                    id = roles.Id,
        //                    text = roles.Name,
        //                    desc = roles.Name,
        //                }
        //                   );
        //    List<EasyuiCombo> result = new List<EasyuiCombo>();
        //    result.Add(new EasyuiCombo(){id="",text="全部",desc="全部"});
        //    if (list.Count() > 0)
        //    {
        //        result.AddRange(list.ToList());
        //    }
        //    return Json(result);
        //}
    }
}
