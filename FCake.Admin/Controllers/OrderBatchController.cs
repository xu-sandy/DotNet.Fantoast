using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FCake.Bll.Services;
using FCake.Core.MvcCommon;
using FCake.Domain.Entities;

namespace FCake.Admin.Controllers
{
    public class OrderBatchController : BaseController
    {
        //
        // GET: /OrderBatch/
        OrderBatchService obs = new OrderBatchService();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult OrderBatch()
        {
            return View();
        }
        [CheckPermission(controlName = "OrderBatch", actionName = "OrderBatch", permissionCode = "view")]
        public ActionResult BatchDetail()
        {
            return View();
        }
        [CheckPermission(controlName = "OrderBatch", actionName = "OrderBatch", permissionCode = "add")]
        public ActionResult AddBatch()
        {
            ViewBag.batchNo = FCake.Bll.CommonRules.CommonNoRules("orderbatch");
            return View();
        }
        /// <summary>
        /// 根据批次的审核状态，制作状态，查询符合条件的批次数据
        /// </summary>
        /// <param name="model"></param>
        /// <param name="page">当前页数</param>
        /// <param name="rows">查询条数</param>
        /// <returns>批次数据集合</returns>
        [CheckPermission(controlName = "OrderBatch", actionName = "OrderBatch", permissionCode = "view")]
        public ActionResult GetOrderBatchByStatus(OrderBatch model, string orderNo, int page = 1, int rows = 50)
        {
            int count = 0;//总共有符合条件的数据条数
            var result = obs.GetOrderBatchs(model, orderNo, out count, page, rows);//取得数据
            if (result == null)
            {
                result = new List<OrderBatch>();
            }
            return Json(new { total = count, rows = result });//返回Json数据
        }
        /// <summary>
        /// 根据订单批次号查询批次下的所有订单
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "OrderBatch", actionName = "OrderBatch", permissionCode = "view")]
        public ActionResult GetKitchenMakeByOrderBatch(string batch)
        {
            var result = obs.GetKitchenMakeByOrderBatch(batch);//取得数据
            return Json(result);//返回Json数据
        }
        /// <summary>
        /// 批次新增
        /// </summary>
        /// <param name="requiredTime"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "OrderBatch", actionName = "OrderBatch", permissionCode = "add")]
        public ActionResult CreatOrderBatch(DateTime requiredTime, string BatchNo)
        {
            OpResult result = new OpResult();
            result = obs.CreatOrderBatch(requiredTime, Helper.UserCache.CurrentUser.Id, BatchNo);
            //var Successed = result.Successed;
            //var Message = result.Message;

            //return Json(new { Successed = Successed, Message = Message });
            return Json(result);
        }
        /// <summary>
        /// 移除批次里的N个订单
        /// </summary>
        /// <param name="batch">批次号</param>
        /// <returns></returns>
        [CheckPermission(controlName = "OrderBatch", actionName = "OrderBatch", permissionCode = "delete")]
        public ActionResult DeleteOrderInBatch(string batchNo)
        {
            string orderNos = Request.Params["orderNos[]"].ToString();
            OpResult result = new OpResult();
            result = obs.DeleteOrderInBatch(orderNos, batchNo);
            return Json(result);
        }
        [CheckPermission(controlName = "OrderBatch", actionName = "OrderBatch", permissionCode = "add")]
        public ActionResult AddOrderInBatch(string batchNo)
        {
            return View();
        }

        /// <summary>
        /// 往批次里增加订单
        /// </summary>
        /// <param name="orderNos">订单号集合</param>
        /// <param name="batchNo">批次号</param>
        /// <returns>是否增加成功，弹窗信息</returns>
        [CheckPermission(controlName = "OrderBatch", actionName = "OrderBatch", permissionCode = "add")]
        public ActionResult AddOrder(string batch)
        {
            var orderNos = Request.Params["orderNos[]"];
            string[] nos = null;
            if (orderNos != null)
            {
                nos = orderNos.Split(',');
            }
            var result = obs.AddOrderInBatch(nos.ToList(), batch);
            //return Json(new { Successed = result.Successed, Message = result.Message });
            return Json(result);
        }
        /// <summary>
        /// 获取所有待安排生产批次的订单
        /// </summary>
        /// <returns></returns>
        [CheckPermission(controlName = "OrderBatch", actionName = "OrderBatch", permissionCode = "view")]
        public ActionResult GetOrders(string orderNo, string mobile)
        {
            var result = obs.GetOrders(orderNo, mobile);
            return Json(result);
        }
        /// <summary>
        /// 更改批次的审核状态
        /// </summary>
        /// <param name="status">状态</param>
        /// <param name="batchNo">批次号</param>
        /// <returns></returns>
        [CheckPermission(controlName = "OrderBatch", actionName = "OrderBatch", permissionCode = "edit")]
        public ActionResult UpdateStatus(int status, string batchNo)
        {
            OpResult result = obs.UpdateStatus(status, batchNo, Helper.UserCache.CurrentUser.Id);
            //return Json(new { Message = result.Message });
            return Json(result);
        }
        /// <summary>
        /// 删除批次（批次里没有订单的才可以删除）
        /// </summary>
        /// <param name="orderBatch"></param>
        /// <returns></returns>
        public ActionResult DeleteItem(string orderBatch)
        {
            OpResult result = obs.DeleteBatchByNo(orderBatch);
            return Json(result);
        }

    }
}
