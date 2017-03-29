using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCake.Domain.Common;
using FCake.Domain;
using FCake.Domain.Entities;
using FCake.Framework;
using FCake.Core.MvcCommon;
using FCake.Domain.Enums;
using System.Data.SqlClient;

namespace FCake.Bll.Services
{

    public class OrderBatchService
    {
        EFDbContext context = new EFDbContext();
        #region 批次表操作相关
        /// <summary>
        /// 批次新增
        /// </summary>
        /// <param name="requiredTime">要求完成时间</param>
        /// <param name="currentUserId">当前登录用户id</param>
        /// <returns></returns>
        public OpResult CreatOrderBatch(DateTime requiredTime, string currentUserId, string BatchNo)
        {
            OpResult result = new OpResult();
            result.Successed = false;
            //if (BatchNo.Length > 14)
            //{
            //    result.Message = "新增失败：批次号格式错误！";
            //    return result;
            //}
            try
            {
                OrderBatch batch = new OrderBatch();//新增一个实体对象
                batch.BatchNo = CommonRules.CommonNoRules("orderbatch");//批次号
                batch.Id = FCake.Core.Common.DataHelper.GetSystemID();//给ID一个guid的值
                batch.RequiredTime = requiredTime;
                batch.MakeStatus = (int)FCake.Domain.Enums.OrderBatchMakeStatus.NotStart;//默认状态为0
                batch.CreatedOn = DateTime.Now;
                batch.CreatedBy = currentUserId;// Helper.UserCache.CurrentUser.Id;
                batch.IsDeleted = 0;
                batch.Status = BatchReviewStatus.ReviewPending;
                context.OrderBatchs.Add(batch);
                if (context.SaveChanges() > 0)//取得添加结果
                {
                    result.Successed = true;
                    result.Message = "新增批次成功";
                }
                else
                {
                    result.Message = "批次新增失败";
                }

            }
            catch (Exception e)
            {
                result.Message = "新增批次异常：" + e.Message;
            }
            return result;
        }
        /// <summary>
        /// 根据条件查询批次信息
        /// </summary>
        /// <param name="status">审批状态</param>
        /// <param name="kitstatus">厨房制作状态</param>
        /// <param name="sendstatus">物流配送状态</param>
        /// <param name="page"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public List<OrderBatch> GetOrderBatchs(OrderBatch model, string orderNo, out int count, int page = 1, int row = 20)
        {
            //取出对象
            var data = context.OrderBatchs.Where(p => p.IsDeleted != 1);

            if (model.Status.HasValue)//判断审核状态 
            {
                data = data.Where(p => p.Status == model.Status);
            }
            if (model.MakeStatus.HasValue)//判断厨房制作状态 
            {
                data = data.Where(p => p.MakeStatus == model.MakeStatus);
            }
            if (!string.IsNullOrEmpty(model.BatchNo))
            {
                data = data.Where(p => p.BatchNo.Contains(model.BatchNo));
            }
            if (!string.IsNullOrEmpty(orderNo))
            {
                var obj = context.KitchenMakes.Where(p => p.OrderNo.Equals(orderNo) && p.IsDeleted != 1).FirstOrDefault();
                if (obj != null)
                {
                    data = data.Where(p => p.BatchNo.Equals(obj.BatchNo) && p.IsDeleted != 1);
                }
                else
                {
                    count = 0;
                    return null;
                }
            }
            count = data.Count();//count 返回总行数
            page = page < 1 ? 1 : page;//如果行数＜1 默认=1
            var temp = data.OrderBy(p => p.Status).ThenBy(p => p.MakeStatus).ThenByDescending(p => p.RequiredTime).Skip((page - 1) * row).Take(row).ToList();//取出符合条件的集合
            return temp;
        }
        /// <summary>
        /// 改变批次的审核状态
        /// </summary>
        /// <param name="status">更改状态</param>
        /// <param name="orderNo">批次号</param>
        /// <returns>是否成功，返回信息</returns>
        public OpResult UpdateStatus(int status, string batchNo, string currentUser)
        {
            OpResult result = new OpResult();
            result.Successed = false;
            try
            {//取出对象
                var data = context.OrderBatchs.Where(p => p.BatchNo.Equals(batchNo) && p.IsDeleted != 1).First();
                string content = string.Format("批次状态由{0}改为{1}", (ReviewStatus)data.Status, (ReviewStatus)status);
                if (data.Status != BatchReviewStatus.ReviewPass)//如果是已通过的状态就返回通过状态
                {
                    data.Status = (BatchReviewStatus)status;//更改状态
                    data.ModifiedBy = currentUser;
                    data.ModifiedOn = DateTime.Now;
                    if (context.SaveChanges() > 0)
                    {
                        result.Successed = true;
                        result.Message = "操作成功：确认生产成功";
                        OperationLogService.SaveOperLog(currentUser, "OrderBatch_Status", data.Id, content);
                    }
                    else
                    {
                        result.Message = "操作失败:确认生成失败";
                    }
                }
                else
                {
                    result.Message = "该批次已是确认生产状态";
                }
            }
            catch (Exception e)
            {
                result.Message = "状态更新异常：" + e.Message;
            }
            return result;
        }
        /// <summary>
        /// 修改批次的制作状态
        /// </summary>
        /// <param name="batch">批次号</param>
        /// <param name="batch">要更改的状态</param>
        /// <returns></returns>
        public OpResult UpdateBatchMakeStatus(string batch, int makeStatus)
        {
            OpResult result = new OpResult();
            result.Successed = false;
            try
            {
                //取得该批次的数据
                var data = context.OrderBatchs.Where(p => p.BatchNo.Equals(batch) && p.IsDeleted != 1).First();
                data.MakeStatus = (OrderBatchMakeStatus)makeStatus;//更新状态
                if (context.SaveChanges() > 0)//保存数据
                {
                    result.Successed = true;
                    result.Message = "保存成功";
                }
                else
                {
                    result.Message = "保存失败";
                }
            }
            catch (Exception e)
            {
                result.Message = "更改制作状态失败：" + e.Message;
            }
            return result;
        }
        /// <summary>
        /// 删除批次
        /// </summary>
        /// <param name="batch"></param>
        /// <returns></returns>
        public OpResult DeleteBatchByNo(string batch)
        {
            OpResult result = new OpResult();
            result.Successed = false;
            try
            {
                var data = context.OrderBatchs.Single(p => p.BatchNo.Equals(batch) && p.IsDeleted != 1);
                if (data.OrderNum > 0)
                {
                    result.Message = "该批次中包含订单，不能删除";
                }
                else
                {
                    data.IsDeleted = 1;
                }
                if (context.SaveChanges() > 0)
                {
                    result.Successed = true;
                    result.Message = "删除批次成功!";
                }
            }
            catch (Exception e)
            {
                result.Message = "删除失败:" + e.Message;
            }
            return result;
        }
        #endregion

        #region 批次里订单相关
        /// <summary>
        /// 移除批次里的多个订单
        /// </summary>
        /// <param name="orderNos">要移除的订单号字符串，以逗号间隔</param>
        /// <param name="batchNo">要移除的订单所属的批次号</param>
        /// <returns></returns>
        public OpResult DeleteOrderInBatch(string orderNos, string batchNo)
        {
            OpResult result = OpResult.Fail("订单移除失败，没有可移除的订单");
            try
            {
                ////1 判断是否审核,未审核的订单才可以增加、移除订单；已审核的订单不可以增加移除订单
                //var batchState = context.OrderBatchs.Where(p => p.BatchNo.Equals(batchNo) && p.IsDeleted != 1).FirstOrDefault();
                //if (batchState != null)
                //{
                //    if (batchState.Status == BatchReviewStatus.ReviewPass) {
                //        result = OpResult.Fail("该批次已审核，不可移除订单");
                //        return result;
                //    }
                //}
                //else {
                //    result = OpResult.Fail("找不到该批次数据信息！");
                //    return result;
                //}

                //2 移除批次对应订单

                SqlParameter[] sqlparms = new SqlParameter[2];
                sqlparms[0] = new SqlParameter("@batchNo", batchNo);
                sqlparms[1] = new SqlParameter("@orderNos", orderNos);
                var execResult = (from p in context.Database.SqlQuery<int>("EXEC dbo.[Proc_RemoveBatchOrders] @batchNo, @orderNos", sqlparms) select p).FirstOrDefault();
                if (execResult > 0)
                {
                    result = OpResult.Success("订单移除成功");
                }
                else
                {
                    if (execResult == -2)
                    {
                        result = OpResult.Success("找不到要移除的订单，移除操作执行失败,请重新选择");
                    }
                    if (execResult == -3)
                    {
                        result = OpResult.Success("存在已完成的订单，移除操作执行失败,请重新选择");
                    }
                }
            }
            catch (Exception e)
            {
                result.Message = "订单批次更改失败:" + e.Message;
            }
            return result;
        }
        /// <summary>
        /// 往批次里增加订单
        /// </summary>
        /// <param name="orderids">要增加的订单<list>多条</param>
        /// <param name="batchNo">批次号</param>
        /// <returns>返回是否成功，返回信息</returns>
        public OpResult AddOrderInBatch(List<string> orderids, string batchNo)
        {
            OpResult result = new OpResult();
            result.Successed = false;
            try
            {//判断批次状态
                var batchState = context.OrderBatchs.Where(p => p.BatchNo.Equals(batchNo) && p.IsDeleted != 1).FirstOrDefault();
                if (batchState != null)
                {
                    if (batchState.MakeStatus == OrderBatchMakeStatus.Complete)
                    {
                        result.Message = "该批次已是完成状态，增加订单失败";
                        return result;
                    }
                }
                else
                {
                    return null;
                }


                int count = orderids.Count();//取得总更改行数 算订单数用
                int cakeCount = 0;//蛋糕数量 算蛋糕总数用
                foreach (var item in orderids)
                {
                    //插入厨房制作主表
                    var data = context.Orders.Where(p => p.No.Equals(item) && p.IsDeleted != 1).First();
                    data.Status = OrderStatus.Scheduled;
                    KitchenMake km = new KitchenMake();
                    km.Id = FCake.Core.Common.DataHelper.GetSystemID();
                    km.BatchNo = batchNo;
                    km.OrderNo = item;
                    km.Status = OrderBatchMakeStatus.NotStart;//到底是订单状态还是批次状态
                    km.IsDeleted = 0;
                    km.CreatedOn = DateTime.Now;
                    context.KitchenMakes.Add(km);
                    // context.SaveChanges();
                    //插入厨房制作从表
                    var odetail = context.OrderDetails.Where(p => p.OrderNo.Equals(item) && p.IsDeleted != 1).ToList();
                    var otherType = CommonRules.OtherProductTypeDicValue;
                    foreach (var num in odetail)
                    {
                        var product = context.Products.SingleOrDefault(p => p.Id.Equals(num.ProductId) && p.IsDeleted != 1);
                        if (product != null)
                        {
                            if (product.Type != otherType)
                                cakeCount += Convert.ToInt32(num.Num);//
                        }
                    }
                    //cakeCount = odetail.Count();
                    foreach (var p in odetail)
                    {
                        KitchenMakeDetail detail = new KitchenMakeDetail();
                        detail.Id = FCake.Core.Common.DataHelper.GetSystemID();
                        detail.KitchenMakeId = km.Id;
                        detail.Status = OrderBatchMakeStatus.NotStart;
                        detail.SubProductId = p.SubProductId;
                        detail.IsDeleted = 0;
                        detail.CreatedOn = DateTime.Now;
                        context.KitchenMakeDetails.Add(detail);
                        //context.SaveChanges();
                    }


                }
                //更改批次表的批次订单数量
                var temp = context.OrderBatchs.Single(p => p.BatchNo.Equals(batchNo) && p.IsDeleted != 1);
                temp.OrderNum += count;
                if (temp.OrderNum < 0)//订单数量不可以是负数
                    temp.OrderNum = 0;

                temp.CakeNum += cakeCount;
                if (temp.CakeNum < 0)//批次的蛋糕数量也不可以为负数
                    temp.CakeNum = 0;
                //保存数据
                if (context.SaveChanges() > 0)
                {
                    result.Successed = true;
                    result.Message = "批次新增订单成功";
                }
                else
                {
                    result.Message = "批次新增订单失败";
                }
            }
            catch (Exception e)
            {
                result.Message = "批次新增订单失败:" + e.Message;
            }
            return result;
        }

        /// <summary>
        /// 根据批次查询批此下的所有订单
        /// </summary>
        /// <param name="batch">批次号</param>
        /// <param name="page">页数</param>
        /// <param name="rows">查询行数</param>
        /// <returns>定的那详情集合</returns>
        public dynamic GetKitchenMakeByOrderBatch(string batch)
        {
            //根据批次号查出符合条件的数据
            var data = context.KitchenMakes.OrderByDescending(p => p.CreatedOn).Where(p => p.BatchNo.Equals(batch) && p.IsDeleted != 1);
            var temp = from k in data
                       join o in context.Orders
                       on k.OrderNo equals o.No
                       join c in context.Customers on o.CustomerId equals c.Id
                       select new
                       {
                           BatchNo = k.BatchNo,
                           OrderNo = o.No,
                           RequiredTime = o.RequiredTime,
                           Status = k.Status,
                           Customer = c.Mobile
                       };
            //返回分页后的数据集合
            return temp.ToList();
        }
        #endregion

        /// <summary>
        /// 获取所有待安排批次的订单
        /// </summary>
        /// <returns></returns>
        public dynamic GetOrders(string orderNo, string mobile)
        {
            //todo:delete更改订单状态
            //var data = context.Orders.Include("Customers").Where(p => p.ReviewStatus == ReviewStatus.ReviewPass && (p.Status == OrderStatus.HadPaid || p.Status == OrderStatus.NotPay) && p.IsDeleted != 1);
            var data = context.Orders.Include("Customers").Where(p => p.ReviewStatus == ReviewStatus.ReviewPass && p.Status == OrderStatus.HadPaid && p.IsDeleted != 1);
            if (orderNo != "" && orderNo != null)
            {
                data = data.Where(p => p.No.Contains(orderNo));
            }
            if (mobile != "" && mobile != null)
            {
                data = data.Where(p => p.Customers.Mobile.Contains(mobile));//用客户的联系电话还是收货人的联系电话
            }
            data = data.OrderBy(p => p.RequiredTime).ThenByDescending(p => p.No).ThenBy(p => p.RequiredTime);
            var temp = from o in data
                       join c in context.Customers
                       on o.CustomerId equals c.Id
                       orderby o.RequiredTime ascending
                       select new
                       {
                           No = o.No,
                           RequiredTime = o.RequiredTime,
                           state = o.Status,
                           Customer = c.Mobile,
                           RequiredTimeBucket = o.RequiredTimeBucket
                       };
            return temp.ToList();
        }
    }
}
