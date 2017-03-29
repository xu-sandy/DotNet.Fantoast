using FCake.Bll.Services;
using FCake.Domain.Common;
using FCake.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FCake.Admin.Controllers
{
    public class CooperationController : BaseController
    {
        CooperationService cs = new CooperationService();

        [CheckPermission(controlName = "Cooperation", actionName = "Index", permissionCode = "view")]
        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// 获取所有合作公司信息
        /// </summary>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Cooperation", actionName = "Index", permissionCode = "view")]
        public ActionResult GetCooperationsAll(int page = 1, int rows = 20)
        {
            var totalCount = 0;
            var pageInfo = new PageInfo() { Page = page, Rows = rows };
            var result = cs.GetCooperationsAll(out totalCount, pageInfo);
            return Json(new { total = totalCount, rows = result });
        }
        [CheckPermission(controlName = "Cooperation", actionName = "Index", permissionCode = "view")]
        public ActionResult CooperationCheck(string id)
        {
            Cooperation cooper = new CooperationService().GetCooperationByWhere(a => a.Id.Equals(id) && a.IsDeleted != 1);
            if (cooper != null)
            {
                var customer = new FCake.Bll.CustomersService().GetById(cooper.CustomerId);
                cooper.CustomerId = customer.Mobile;
                return View(cooper);
            }
            return View();
        }
        /// <summary>
        /// 审核企业试吃申请
        /// </summary>
        /// <param name="id"></param>
        /// <param name="description"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Cooperation", actionName = "Index", permissionCode = "view")]
        public JsonResult UpdateCooperation(string id, string description, int result)
        {
            CooperationService cs = new CooperationService();
            var res = cs.UpdateCooperation(id, description, result);
            return Json(res);
        }

    }
}
