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
    public class TempOrderController : BaseController
    {
        //
        // GET: /TempOrder/
        private readonly OrderService _orderService = new OrderService();
        private readonly TempOrderService _TempOrderService = new TempOrderService();
        [CheckPermission(controlName = "TempOrder", actionName = "SearchOrder", permissionCode = "view")]
        public ActionResult SearchOrder()
        {
            return View();
        }
        [HttpPost]
        [CheckPermission(controlName = "TempOrder", actionName = "SearchOrder", permissionCode = "view")]
        public ActionResult IsExistOrder(string orderNo)
        {
            var result = OpResult.Fail();
            var order = _orderService.GetByNo(orderNo);
            if (order != null)
                result = OpResult.Success();
            return Json(result);
        }
        [CheckPermission(controlName = "TempOrder", actionName = "SearchOrder", permissionCode = "view")]
        public ActionResult EditOrder(string orderNo)
        {
            var order = _orderService.GetByNo(orderNo);
            return View(order);
        }
        [ValidateInput(false)]
        [CheckPermission(controlName = "TempOrder", actionName = "SearchOrder", permissionCode = "edit")]
        [HttpPost]
        public ActionResult ChangeOrderInfo(string orderNo, decimal actualPay = 0, decimal couponPay = 0, decimal giftCardPay = 0, int isUpdateCustomer = 1)
        {
            var result = _TempOrderService.ChangeOrderInfo(orderNo, actualPay, couponPay, giftCardPay, isUpdateCustomer);
            return Json(result);
        }

    }
}
