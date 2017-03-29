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
    public class MemberLevelController : BaseController
    {
        //
        // GET: /MemberLevel/
        private readonly MemberLevelService _memberLevelService = new MemberLevelService();
        [CheckPermission(controlName = "MemberLevel", actionName = "Index", permissionCode = "view")]
        public ActionResult Index()
        {
            return View();
        }
        [CheckPermission(controlName = "MemberLevel", actionName = "Index", permissionCode = "view")]
        [HttpPost]
        public ActionResult GetMemberLevelPaging(int page = 1, int rows = 20)
        {
            int totalCount = 0;
            var pageInfo = new PageInfo() { Page = page, Rows = rows };
            var result = _memberLevelService.GetMemberLevelPaging(out totalCount, pageInfo);
            GridJsonModel djson = new GridJsonModel(totalCount, result);
            return Json(djson);
        }
        [CheckPermission(controlName = "MemberLevel", actionName = "Index", permissionCode = "edit")]
        public ActionResult Edit(string memberLevelId)
        {
            MemberLevel memberLevel = new MemberLevel();
            if (!string.IsNullOrEmpty(memberLevelId))
            {
                var ml = _memberLevelService.Find<MemberLevel>(memberLevelId);
                if (ml != null)
                    memberLevel = ml;
            }
            return View(memberLevel);
        }
        [CheckPermission(controlName = "MemberLevel", actionName = "Index", permissionCode = "edit")]
        [HttpPost]
        public ActionResult SaveOrUpdate(MemberLevel memberLevel)
        {
            var result = _memberLevelService.SaveOrUpdate(memberLevel, UserCache.CurrentUser.Id);
            return Json(result);
        }
        /// <summary>
        /// 获取所有的用户等级
        /// </summary>
        /// <returns></returns>
       [CheckPermission(isRelease = true)]
        public ActionResult GetAllMemberLevelBindCombobox(bool isAddAll)
        {
            List<MemberLevel> result = new List<MemberLevel>();
            //添加全部选项
            if (isAddAll)
            {
                MemberLevel level = new MemberLevel();
                level.Title = "全部";
                level.MemberLevelValue = -1;
                result.Add(level);
            }
            //将数据插入
            result.AddRange(_memberLevelService.GetAllMemberLevel());
            return Json(result);
        }

    }
}
