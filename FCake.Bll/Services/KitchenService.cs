
using FCake.Core.Common;
using FCake.Domain;
using FCake.Domain.Entities;
using FCake.Domain.Enums;
using FCake.Domain.WebModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FCake.Bll
{
    /// <summary>
    /// 厨房模块
    /// </summary>
    public class KitchenService
    {
        EFDbContext context = new EFDbContext();

        #region 批次状态管理
        /// <summary>
        /// 按批次号开始厨房制作
        /// </summary>
        /// <param name="id">批次号</param>
        /// <returns></returns>
        public bool StartBatch(string batchNo, string currentUserId)
        {
            //取出批次实体
            OrderBatch ob = context.OrderBatchs.SingleOrDefault(a => a.IsDeleted != 1 && a.BatchNo.Equals(batchNo));
            if (ob == null)
                return false;
            //状态为未开始
            if (ob.MakeStatus == OrderBatchMakeStatus.NotStart)
            {
                //状态设定为制作中
                ob.MakeStatus = OrderBatchMakeStatus.Making;
                ob.ModifiedBy = currentUserId;
                ob.ModifiedOn = DateTime.Now;
                ob.MakeBeginTime = DateTime.Now;
            }

            var makeDetails = (from x in context.KitchenMakeDetails
                               join y in context.KitchenMakes on x.KitchenMakeId equals y.Id
                               where x.IsDeleted != 1
                               && y.IsDeleted != 1
                               && y.BatchNo.Equals(batchNo)
                               && x.Status == OrderBatchMakeStatus.NotStart
                               select x);
            if (makeDetails.Any())
            {
                //变更子项状态
                foreach (var detail in makeDetails)
                {
                    detail.Status = OrderBatchMakeStatus.Making;
                    detail.BeginTime = DateTime.Now;
                    detail.ModifiedBy = currentUserId;
                    detail.ModifiedOn = DateTime.Now;
                }
            }

            var kitchenMakes = (from x in context.KitchenMakes
                                where x.IsDeleted != 1
                                && x.BatchNo.Equals(batchNo)
                                && x.Status == OrderBatchMakeStatus.NotStart
                                select x);
            foreach (var kitchen in kitchenMakes)
            {
                kitchen.ModifiedBy = currentUserId;
                kitchen.ModifiedOn = DateTime.Now;
                kitchen.Status = OrderBatchMakeStatus.Making;
            }

            //订单状态
            //modify:ywb,修改已完成的订单又变成制作中的bug
            var orders = (from x in context.KitchenMakes
                          join y in context.Orders on x.OrderNo equals y.No
                          where x.IsDeleted != 1 && y.IsDeleted != 1
                          && x.BatchNo == batchNo //添加了批次过滤条件
                          select y);

            foreach (var x in orders)
            {
                if (x.Status != OrderStatus.Making && x.Status != OrderStatus.MakeCompleted)
                {
                    x.Status = OrderStatus.Making;
                    x.ModifiedBy = currentUserId;
                    x.ModifiedOn = DateTime.Now;
                }
            }

            return context.SaveChanges() > 0;
        }
        /// <summary>
        /// 按批次号撤销厨房制作
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool RescindBatch(string batchNo, string currentUserId)
        {
            //取出批次实体
            OrderBatch ob = context.OrderBatchs.SingleOrDefault(a => a.IsDeleted != 1 && a.BatchNo.Equals(batchNo));
            if (ob == null)
                return false;
            //只有在制作中才能撤销制作
            if (ob.MakeStatus == OrderBatchMakeStatus.Making)
            {
                //确保没有含有已完成的
                var makeDetails = (from x in context.KitchenMakeDetails
                                   join y in context.KitchenMakes on x.KitchenMakeId equals y.Id
                                   where x.IsDeleted != 1
                                   && y.IsDeleted != 1
                                   && y.BatchNo.Equals(batchNo)
                                   select x);
                if (makeDetails.Any(a => a.Status == OrderBatchMakeStatus.Complete) == false)
                {
                    //变更状态为未开始
                    ob.MakeStatus = OrderBatchMakeStatus.NotStart;
                    ob.MakeBeginTime = null;
                    ob.MakeEndTime = null;
                    ob.ModifiedOn = DateTime.Now;
                    ob.ModifiedBy = currentUserId;
                }

                //撤销厨房状态
                var kitchens = (from x in context.KitchenMakes
                                join y in context.KitchenMakeDetails.Where(a => a.Status != OrderBatchMakeStatus.NotStart && a.IsDeleted != 1) on x.Id equals y.KitchenMakeId into details
                                where x.IsDeleted != 1
                                && x.Status == OrderBatchMakeStatus.Making
                                && details.Any(a => a.Status == OrderBatchMakeStatus.Complete) == false
                                select x);

                var orderNos = kitchens.Select(a => a.OrderNo).Distinct().ToList();
                foreach (var x in kitchens)
                {
                    x.Status = OrderBatchMakeStatus.NotStart;
                    x.ModifiedOn = DateTime.Now;
                    x.ModifiedBy = currentUserId;
                }

                //撤销子项
                foreach (var x in makeDetails)
                {
                    if (x.Status == OrderBatchMakeStatus.Making)
                    {
                        x.Status = OrderBatchMakeStatus.NotStart;
                        x.BeginTime = null;
                        x.EndTime = null;
                        x.ModifiedBy = currentUserId;
                        x.ModifiedOn = DateTime.Now;
                    }
                }

                var resultValue = context.SaveChanges();

                foreach (var x in orderNos)
                {
                    //变更订单状态
                    var order = context.Orders.SingleOrDefault(a => a.IsDeleted != 1 && a.No == x);
                    if (order == null)
                        return false;
                    order.Status = OrderStatus.Scheduled;//排产中
                    order.ModifiedBy = currentUserId;
                    order.ModifiedOn = DateTime.Now;
                }

                return (resultValue + context.SaveChanges()) > 0;
            }
            return false;
        }
        /// <summary>
        /// 按批次号结束厨房制作
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool EndBatch(string batchNo, string currentUserId)
        {
            //取出批次实体
            OrderBatch ob = context.OrderBatchs.SingleOrDefault(a => a.IsDeleted != 1 && a.BatchNo.Equals(batchNo));
            if (ob == null)
                return false;
            //只有在制作中才能撤销制作
            if (ob.MakeStatus == OrderBatchMakeStatus.Making)
            {
                //确保没有含有未开始的
                var makeDetails = (from x in context.KitchenMakeDetails
                                   join y in context.KitchenMakes on x.KitchenMakeId equals y.Id
                                   where x.IsDeleted != 1
                                   && y.IsDeleted != 1
                                   && y.BatchNo.Equals(batchNo)
                                   select x);
                if (makeDetails.Any(a => a.Status == OrderBatchMakeStatus.NotStart) == false)
                {
                    //变更状态为完成
                    ob.MakeStatus = OrderBatchMakeStatus.Complete;
                    ob.MakeEndTime = DateTime.Now; ;
                    ob.ModifiedOn = DateTime.Now;
                    ob.ModifiedBy = currentUserId;
                    //转移数据
                    var os = (from x in context.Orders
                              join y in context.KitchenMakes on x.No equals y.OrderNo
                              where y.BatchNo.Equals(batchNo)
                              && x.IsDeleted != 1
                              && y.IsDeleted != 1
                              select x).ToList();
                    InsertDistributions(os, currentUserId);
                }

                //完成厨房状态
                var kitchens = (from x in context.KitchenMakes
                                join y in context.KitchenMakeDetails.Where(a => a.Status != OrderBatchMakeStatus.Complete && a.IsDeleted != 1) on x.Id equals y.KitchenMakeId into details
                                where x.IsDeleted != 1
                                && x.Status == OrderBatchMakeStatus.Making
                                && details.Any(a => a.Status == OrderBatchMakeStatus.NotStart) == false
                                select x);

                var orderNos = kitchens.Select(a => a.OrderNo).Distinct().ToList();
                foreach (var x in kitchens)
                {
                    x.Status = OrderBatchMakeStatus.Complete;
                    x.ModifiedOn = DateTime.Now;
                    x.ModifiedBy = currentUserId;
                }

                //完成子项
                foreach (var x in makeDetails)
                {
                    if (x.Status == OrderBatchMakeStatus.Making)
                    {
                        x.Status = OrderBatchMakeStatus.Complete;
                        x.EndTime = DateTime.Now;
                        x.ModifiedBy = currentUserId;
                        x.ModifiedOn = DateTime.Now;
                    }
                }

                var resultValue = context.SaveChanges();

                foreach (var x in orderNos)
                {
                    //变更订单状态
                    var order = context.Orders.SingleOrDefault(a => a.IsDeleted != 1 && a.No == x);
                    if (order == null)
                        return false;
                    order.Status = OrderStatus.MakeCompleted;//排产中
                    order.ModifiedBy = currentUserId;
                    order.ModifiedOn = DateTime.Now;
                }

                return (resultValue + context.SaveChanges()) > 0;
            }
            return false;
        }
        #endregion

        #region 订单状态管理
        /// <summary>
        /// 按订单号开始厨房制作
        /// </summary>
        /// <param name="id">批次号</param>
        /// <returns></returns>
        public bool StartOrder(string orderNo, string currentUserId)
        {
            //取出批次实体
            var kb = context.KitchenMakes.SingleOrDefault(a => a.IsDeleted != 1 && a.OrderNo.Equals(orderNo));
            if (kb == null)
                return false;
            //状态为未开始
            if (kb.Status == OrderBatchMakeStatus.NotStart)
            {
                //状态设定为制作中
                kb.Status = OrderBatchMakeStatus.Making;
                kb.ModifiedBy = currentUserId;
                kb.ModifiedOn = DateTime.Now;
            }
            //将子状态全部设置为制作中
            var details = context.KitchenMakeDetails.Where(a => a.IsDeleted != 1
                && a.KitchenMakeId.Equals(kb.Id)
                && a.Status == OrderBatchMakeStatus.NotStart);
            foreach (var x in details)
            {
                x.Status = OrderBatchMakeStatus.Making;
                x.ModifiedBy = currentUserId;
                x.ModifiedOn = DateTime.Now;
            }
            //变更订单状态为制作中
            var order = context.Orders.SingleOrDefault(a => a.IsDeleted != 1 && a.No.Equals(kb.OrderNo));
            if (order == null)
                return false;
            if (order.Status == OrderStatus.Scheduled)
            {
                order.Status = OrderStatus.Making;//制作中
                order.ModifiedBy = currentUserId;
                order.ModifiedOn = DateTime.Now;
            }

            //将批次状态设置为制作中
            var orderBatch = context.OrderBatchs.SingleOrDefault(a => a.IsDeleted != 1 && a.BatchNo == kb.BatchNo);
            if (orderBatch == null)
                return false;
            if (orderBatch.MakeStatus == OrderBatchMakeStatus.NotStart)
            {
                //变更批次状态
                orderBatch.MakeStatus = OrderBatchMakeStatus.Making;
                orderBatch.ModifiedBy = currentUserId;
                orderBatch.ModifiedOn = DateTime.Now;
                orderBatch.MakeBeginTime = DateTime.Now;
                orderBatch.MakeEndTime = null;
            }

            return context.SaveChanges() > 0;

        }
        /// <summary>
        /// 按订单号撤销厨房制作
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool RescindOrder(string orderNo, string currentUserId)
        {
            //取出批次实体
            var kb = context.KitchenMakes.SingleOrDefault(a => a.IsDeleted != 1 && a.OrderNo.Equals(orderNo));
            if (kb == null)
                return false;
            //状态为制作中
            if (kb.Status == OrderBatchMakeStatus.Making)
            {
                //产品不能出现已完成
                if (context.KitchenMakeDetails.Any(a =>
                    a.IsDeleted != 1 &&
                    a.KitchenMakeId.Equals(kb.Id) &&
                    a.Status == OrderBatchMakeStatus.Complete
                    ) == false)
                {
                    //状态设定为未开始
                    kb.Status = 0;
                    kb.ModifiedOn = DateTime.Now;
                    kb.ModifiedBy = currentUserId;

                    //变更订单状态为未开始
                    var order = context.Orders.SingleOrDefault(a => a.IsDeleted != 1 && a.No.Equals(kb.OrderNo));
                    if (order == null)
                        return false;
                    order.Status = OrderStatus.Scheduled;//排产中
                    order.ModifiedOn = DateTime.Now;
                    order.ModifiedBy = currentUserId;

                    //将批次状态设置为未开始
                    var orderBatch = context.OrderBatchs.SingleOrDefault(a => a.IsDeleted != 1 && a.BatchNo == kb.BatchNo);
                    if (orderBatch == null)
                        return false;
                    var tempKitchenMakes = context.KitchenMakes.Where(a => a.IsDeleted != 1 && a.BatchNo.Equals(kb.BatchNo));
                    if (tempKitchenMakes.Any(a => a.Status != OrderBatchMakeStatus.NotStart && a.Id.Equals(kb.Id) == false) == false)
                    {
                        orderBatch.MakeStatus = OrderBatchMakeStatus.NotStart;
                        orderBatch.MakeBeginTime = null;
                        orderBatch.MakeEndTime = null;
                        orderBatch.ModifiedBy = currentUserId;
                        orderBatch.ModifiedOn = DateTime.Now;
                    }
                }

                //将子状态全部设置为未开始
                var details = context.KitchenMakeDetails.Where(a => a.IsDeleted != 1 && a.KitchenMakeId.Equals(kb.Id) && a.Status == OrderBatchMakeStatus.Making);
                foreach (var x in details)
                {
                    x.Status = OrderBatchMakeStatus.NotStart;
                    x.ModifiedOn = DateTime.Now;
                    x.ModifiedBy = currentUserId;
                }



                return context.SaveChanges() > 0;
            }
            return false;
        }
        /// <summary>
        /// 按订单号结束厨房制作
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool EndOrder(string orderNo, string currentUserId)
        {
            //取出批次实体
            var kb = context.KitchenMakes.SingleOrDefault(a => a.IsDeleted != 1 && a.OrderNo.Equals(orderNo));
            if (kb == null)
                return false;
            //只有在制作中才能完成制作
            if (kb.Status == OrderBatchMakeStatus.Making)
            {
                var details = context.KitchenMakeDetails.Where(a => a.IsDeleted != 1 && a.KitchenMakeId.Equals(kb.Id));
                bool allMaking = true;
                foreach (var x in details)
                {
                    //将制作中变更为完成
                    if (x.Status == OrderBatchMakeStatus.Making)
                    {
                        x.Status = OrderBatchMakeStatus.Complete;
                        x.ModifiedOn = DateTime.Now;
                        x.ModifiedBy = currentUserId;
                    }
                    if (x.Status == OrderBatchMakeStatus.NotStart)
                        allMaking = false;
                }

                //变更状态为已完成
                if (allMaking)
                {
                    kb.Status = OrderBatchMakeStatus.Complete;
                    kb.ModifiedOn = DateTime.Now;
                    kb.ModifiedBy = currentUserId;

                    //变更订单状态为制作完成
                    var order = context.Orders.SingleOrDefault(a => a.IsDeleted != 1 && a.No.Equals(kb.OrderNo));
                    if (order == null)
                        return false;
                    order.Status = OrderStatus.MakeCompleted;//制作完成
                    order.ModifiedOn = DateTime.Now;
                    order.ModifiedBy = currentUserId;

                    //将批次状态设置为完成
                    var orderBatch = context.OrderBatchs.SingleOrDefault(a => a.IsDeleted != 1 && a.BatchNo == kb.BatchNo);
                    if (orderBatch == null)
                        return false;
                    var tempKitchenMakes = context.KitchenMakes.Where(a => a.IsDeleted != 1 && a.BatchNo.Equals(kb.BatchNo));
                    if (tempKitchenMakes.Any(a => a.Status != OrderBatchMakeStatus.Complete && a.Id.Equals(kb.Id) == false) == false)
                    {
                        orderBatch.MakeStatus = OrderBatchMakeStatus.Complete;
                        orderBatch.MakeEndTime = DateTime.Now;
                        orderBatch.ModifiedBy = currentUserId;
                        orderBatch.ModifiedOn = DateTime.Now;


                        //转移数据
                        var os = (from x in context.Orders
                                  join y in context.KitchenMakes on x.No equals y.OrderNo
                                  where y.BatchNo.Equals(orderBatch.BatchNo)
                                  && x.IsDeleted != 1
                                  && y.IsDeleted != 1
                                  select x).ToList();
                        InsertDistributions(os, currentUserId);
                    }
                }


                return context.SaveChanges() > 0;
            }
            return false;
        }
        #endregion

        #region 产品状态管理
        /// <summary>
        /// 按产品ID开始厨房制作
        /// </summary>
        /// <param name="id">产品ID</param>
        /// <returns></returns>
        public bool StartSubProduct(string kitchenMakeDetailId, string currentUserId)
        {
            //取出产品实体
            var kb = context.KitchenMakeDetails.SingleOrDefault(a => a.IsDeleted != 1 && a.Id.Equals(kitchenMakeDetailId));
            if (kb == null)
                return false;
            //状态为未开始
            if (kb.Status == OrderBatchMakeStatus.NotStart)
            {
                //状态设定为制作中
                kb.Status = OrderBatchMakeStatus.Making;
                kb.ModifiedBy = currentUserId;
                kb.ModifiedOn = DateTime.Now;
                kb.BeginTime = DateTime.Now;//记录开始时间
                kb.EndTime = null;

                //将厨房订单状态也设置为制作中
                var orderMake = context.KitchenMakes.SingleOrDefault(a => a.IsDeleted != 1 && a.Id.Equals(kb.KitchenMakeId));
                if (orderMake == null)
                    return false;
                if (orderMake.Status == OrderBatchMakeStatus.NotStart)
                {
                    //变更厨房订单状态
                    orderMake.Status = OrderBatchMakeStatus.Making;
                    orderMake.ModifiedBy = currentUserId;
                    orderMake.ModifiedOn = DateTime.Now;

                    //变更订单状态
                    var order = context.Orders.SingleOrDefault(a => a.IsDeleted != 1 && a.No.Equals(orderMake.OrderNo));
                    if (order == null)
                        return false;
                    order.Status = OrderStatus.Making;//制作中
                    order.ModifiedBy = currentUserId;
                    order.ModifiedOn = DateTime.Now;
                }

                //将批次状态设置为制作中
                var orderBatch = context.OrderBatchs.SingleOrDefault(a => a.IsDeleted != 1 && a.BatchNo.Equals(orderMake.BatchNo));
                if (orderBatch == null)
                    return false;
                if (orderBatch.MakeStatus == OrderBatchMakeStatus.NotStart)
                {
                    //变更批次状态
                    orderBatch.MakeStatus = OrderBatchMakeStatus.Making;
                    orderBatch.ModifiedBy = currentUserId;
                    orderBatch.ModifiedOn = DateTime.Now;
                    orderBatch.MakeBeginTime = DateTime.Now;
                    orderBatch.MakeEndTime = null;
                }

                return context.SaveChanges() > 0;
            }
            return false;
        }
        /// <summary>
        /// 按订单号撤销厨房制作
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool RescindSubProduct(string kitchenMakeDetailId, string currentUserId)
        {
            //取出产品实体
            var kb = context.KitchenMakeDetails.SingleOrDefault(a => a.IsDeleted != 1 && a.Id.Equals(kitchenMakeDetailId));
            if (kb == null)
                return false;
            //状态为制作中
            if (kb.Status == OrderBatchMakeStatus.Making)
            {
                //状态设定为未开始
                kb.Status = OrderBatchMakeStatus.NotStart;
                kb.ModifiedBy = currentUserId;
                kb.ModifiedOn = DateTime.Now;
                kb.BeginTime = null;
                kb.EndTime = null;

                //将厨房订单状态也设置为未开始
                var orderMake = context.KitchenMakes.SingleOrDefault(a => a.IsDeleted != 1 && a.Id.Equals(kb.KitchenMakeId));
                if (orderMake == null)
                    return false;
                if (orderMake.Status == OrderBatchMakeStatus.Making)
                {
                    var makeDetails = context.KitchenMakeDetails.Where(a => a.IsDeleted != 1 && a.KitchenMakeId.Equals(orderMake.Id));
                    if (makeDetails.Any(a => a.Status != OrderBatchMakeStatus.NotStart && a.Id.Equals(kb.Id) == false) == false)
                    {
                        //变更厨房订单状态
                        orderMake.Status = OrderBatchMakeStatus.NotStart;
                        orderMake.ModifiedBy = currentUserId;
                        orderMake.ModifiedOn = DateTime.Now;

                        //变更订单状态
                        var order = context.Orders.SingleOrDefault(a => a.IsDeleted != 1 && a.No.Equals(orderMake.OrderNo));
                        if (order == null)
                            return false;
                        order.Status = OrderStatus.Scheduled;//排产中
                        order.ModifiedBy = currentUserId;
                        order.ModifiedOn = DateTime.Now;

                        //变更批次状态
                        var batch = context.OrderBatchs.SingleOrDefault(a => a.IsDeleted != 1 && a.BatchNo.Equals(orderMake.BatchNo));
                        if (batch == null)
                            return false;
                        var kitchenMakes = context.KitchenMakes.Where(a => a.IsDeleted != 1 && a.BatchNo.Equals(orderMake.BatchNo));
                        if (kitchenMakes.Any(a => a.Status != OrderBatchMakeStatus.NotStart && a.Id.Equals(orderMake.Id) == false) == false)
                        {
                            batch.MakeStatus = OrderBatchMakeStatus.NotStart;
                            batch.MakeBeginTime = null;
                            batch.MakeEndTime = null;
                            batch.ModifiedBy = currentUserId;
                            batch.ModifiedOn = DateTime.Now;
                        }
                    }

                }

                return context.SaveChanges() > 0;
            }
            return false;
        }
        /// <summary>
        /// 按产品ID结束厨房制作
        /// </summary>
        /// <param name="subProductId"></param>
        /// <param name="currentUserId"></param>
        /// <returns></returns>
        public bool EndSubProduct(string kitchenMakeDetailId, string currentUserId)
        {
            //取出产品实体
            var makeDetail = context.KitchenMakeDetails.SingleOrDefault(a => a.IsDeleted != 1 && a.Id.Equals(kitchenMakeDetailId));
            if (makeDetail == null)
                return false;
            //状态为制作中
            if (makeDetail.Status == OrderBatchMakeStatus.Making)
            {
                //状态设定完成
                makeDetail.Status = OrderBatchMakeStatus.Complete;
                makeDetail.ModifiedBy = currentUserId;
                makeDetail.ModifiedOn = DateTime.Now;
                makeDetail.EndTime = DateTime.Now;

                //将厨房订单状态也设置为完成(仅当所有产品都完成)
                var kitchenMake = context.KitchenMakes.SingleOrDefault(a => a.IsDeleted != 1 && a.Id.Equals(makeDetail.KitchenMakeId));
                if (kitchenMake == null)
                    return false;

                var makeDetails = context.KitchenMakeDetails.Where(a => a.KitchenMakeId.Equals(kitchenMake.Id) && a.IsDeleted != 1);
                if (makeDetails.Any(a => a.Status != OrderBatchMakeStatus.Complete && a.Id.Equals(makeDetail.Id) == false) == false)
                {
                    //变更厨房订单状态
                    kitchenMake.Status = OrderBatchMakeStatus.Complete;
                    kitchenMake.ModifiedBy = currentUserId;
                    kitchenMake.ModifiedOn = DateTime.Now;

                    //变更订单状态
                    var order = context.Orders.SingleOrDefault(a => a.IsDeleted != 1 && a.No.Equals(kitchenMake.OrderNo));
                    if (order == null)
                        return false;
                    order.Status = OrderStatus.MakeCompleted;//制作中
                    order.ModifiedBy = currentUserId;
                    order.ModifiedOn = DateTime.Now;

                    //变更批次状态
                    var batch = context.OrderBatchs.SingleOrDefault(a => a.IsDeleted != 1 && a.BatchNo.Equals(kitchenMake.BatchNo));
                    if (batch == null)
                        return false;
                    var kitchenMakes = context.KitchenMakes.Where(a => a.BatchNo.Equals(kitchenMake.BatchNo) && a.IsDeleted != 1 && a.Id.Equals(kitchenMake.Id) == false);
                    if (kitchenMakes.Any(a => a.Status != OrderBatchMakeStatus.Complete) == false)
                    {
                        batch.MakeEndTime = DateTime.Now;
                        batch.MakeStatus = OrderBatchMakeStatus.Complete;
                        batch.ModifiedBy = currentUserId;
                        batch.ModifiedOn = DateTime.Now;



                        //转移数据
                        var os = (from x in context.Orders
                                  join y in context.KitchenMakes on x.No equals y.OrderNo
                                  where y.BatchNo.Equals(batch.BatchNo)
                                  && x.IsDeleted != 1
                                  && y.IsDeleted != 1
                                  select x).ToList();
                        InsertDistributions(os, currentUserId);
                    }
                }

                return context.SaveChanges() > 0;
            }
            return false;
        }
        #endregion

        #region 产品类型状态管理
        /// <summary>
        /// 按产品ID开始厨房制作
        /// </summary>
        /// <param name="id">产品ID</param>
        /// <returns></returns>
        public bool StartSubProductType(string productId, string batchNo, string currentUserId)
        {
            //取出所有同批次同类型的产品
            var notStartMakeDetails = (from x in context.KitchenMakes
                                       join y in context.KitchenMakeDetails on x.Id equals y.KitchenMakeId
                                       join z in context.SubProducts on y.SubProductId equals z.Id
                                       where x.IsDeleted != 1
                                       && y.IsDeleted != 1
                                       && x.BatchNo.Equals(batchNo)
                                       && z.ParentId.Equals(productId)
                                       && y.Status == OrderBatchMakeStatus.NotStart
                                       select y);

            if (notStartMakeDetails.Any())
            {
                //变更为制作中状态
                foreach (var makeDetail in notStartMakeDetails)
                {
                    makeDetail.BeginTime = DateTime.Now;
                    makeDetail.EndTime = null;
                    makeDetail.ModifiedBy = currentUserId;
                    makeDetail.ModifiedOn = DateTime.Now;
                    makeDetail.Status = OrderBatchMakeStatus.Making;
                }

                var makeIds = notStartMakeDetails.Select(a => a.KitchenMakeId).Distinct().ToList();
                //将厨房状态改为制作中(KitchenMake)
                var kitchenMakes = (from x in context.KitchenMakes
                                    where makeIds.Contains(x.Id)
                                       && x.IsDeleted != 1
                                       && x.Status == OrderBatchMakeStatus.NotStart
                                    select x).ToList();
                //循环变更状态
                foreach (var kitchenMake in kitchenMakes)
                {
                    //变更厨房订单状态
                    kitchenMake.Status = OrderBatchMakeStatus.Making;
                    kitchenMake.ModifiedBy = currentUserId;
                    kitchenMake.ModifiedOn = DateTime.Now;

                    //变更订单状态
                    var order = context.Orders.SingleOrDefault(a => a.IsDeleted != 1 && a.No.Equals(kitchenMake.OrderNo));
                    if (order == null)
                        return false;
                    order.Status = OrderStatus.Making;//制作中
                    order.ModifiedBy = currentUserId;
                    order.ModifiedOn = DateTime.Now;
                }

                //将批次状态设置为制作中
                var orderBatch = context.OrderBatchs.SingleOrDefault(a => a.IsDeleted != 1 && a.BatchNo == batchNo);
                if (orderBatch == null)
                    return false;
                if (orderBatch.MakeStatus == OrderBatchMakeStatus.NotStart)
                {
                    //变更批次状态
                    orderBatch.MakeStatus = OrderBatchMakeStatus.Making;
                    orderBatch.ModifiedBy = currentUserId;
                    orderBatch.ModifiedOn = DateTime.Now;
                    orderBatch.MakeBeginTime = DateTime.Now;
                    orderBatch.MakeEndTime = null;
                }

                return context.SaveChanges() > 0;
            }

            return false;
        }
        /// <summary>
        /// 按订单号撤销厨房制作
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool RescindSubProductType(string productId, string batchNo, string currentUserId)
        {
            //取出所有同批次同类型的产品
            var makingDetail = (from x in context.KitchenMakes
                                join y in context.KitchenMakeDetails on x.Id equals y.KitchenMakeId
                                join z in context.SubProducts on y.SubProductId equals z.Id
                                where x.IsDeleted != 1
                                && y.IsDeleted != 1
                                && x.BatchNo.Equals(batchNo)
                                && z.ParentId.Equals(productId)
                                && y.Status == OrderBatchMakeStatus.Making
                                select y);

            if (makingDetail.Any())
            {
                //变更厨房子项状态
                foreach (var item in makingDetail)
                {
                    item.BeginTime = null;
                    item.EndTime = null;
                    item.ModifiedBy = currentUserId;
                    item.ModifiedOn = DateTime.Now;
                    item.Status = OrderBatchMakeStatus.NotStart;
                }

                var detailIds = makingDetail.Select(a => a.Id).ToList();
                var makeIds = makingDetail.Select(a => a.KitchenMakeId).Distinct().ToList();
                //将厨房状态改为未开始(KitchenMake)
                var kitchenMakes = (from x in context.KitchenMakes
                                    join y in context.KitchenMakeDetails.Where(a => a.IsDeleted != 1
                                        && a.Status != OrderBatchMakeStatus.NotStart
                                        && detailIds.Contains(a.Id) == false
                                        ) on x.Id equals y.KitchenMakeId into temp
                                    where makeIds.Contains(x.Id)
                                       && x.IsDeleted != 1
                                       && x.Status == OrderBatchMakeStatus.Making
                                       && temp.Count() == 0
                                    select x).ToList();
                //循环变更状态
                foreach (var kitchenMake in kitchenMakes)
                {
                    //变更厨房订单状态
                    kitchenMake.Status = OrderBatchMakeStatus.NotStart;
                    kitchenMake.ModifiedBy = currentUserId;
                    kitchenMake.ModifiedOn = DateTime.Now;

                    //变更订单状态
                    var order = context.Orders.SingleOrDefault(a => a.IsDeleted != 1 && a.No.Equals(kitchenMake.OrderNo));
                    if (order == null)
                        return false;
                    order.Status = OrderStatus.Scheduled;//制作中
                    order.ModifiedBy = currentUserId;
                    order.ModifiedOn = DateTime.Now;
                }

                makeIds = kitchenMakes.Select(a => a.Id).ToList();

                //将批次状态设置为未开始
                var orderBatch = context.OrderBatchs.SingleOrDefault(a => a.IsDeleted != 1 && a.BatchNo == batchNo);
                if (orderBatch == null)
                    return false;
                var tempKitchenMakes = context.KitchenMakes.Where(a => a.IsDeleted != 1 && a.BatchNo.Equals(batchNo));
                if (tempKitchenMakes.Any(a => a.Status != OrderBatchMakeStatus.NotStart && makeIds.Contains(a.Id) == false) == false)
                {
                    orderBatch.MakeStatus = OrderBatchMakeStatus.NotStart;
                    orderBatch.MakeBeginTime = null;
                    orderBatch.MakeEndTime = null;
                    orderBatch.ModifiedBy = currentUserId;
                    orderBatch.ModifiedOn = DateTime.Now;
                }

                return context.SaveChanges() > 0;
            }

            return false;
        }
        /// <summary>
        /// 按产品ID结束厨房制作
        /// </summary>
        /// <param name="subProductId"></param>
        /// <param name="currentUserId"></param>
        /// <returns></returns>
        public bool EndSubProductType(string productId, string batchNo, string currentUserId)
        {
            //取出所有同批次同类型的产品
            var makingDetail = (from x in context.KitchenMakes
                                join y in context.KitchenMakeDetails on x.Id equals y.KitchenMakeId
                                join z in context.SubProducts on y.SubProductId equals z.Id
                                where x.IsDeleted != 1
                                && y.IsDeleted != 1
                                && x.BatchNo.Equals(batchNo)
                                && z.ParentId.Equals(productId)
                                && y.Status == OrderBatchMakeStatus.Making
                                select y);

            if (makingDetail.Any())
            {
                //变更厨房子项状态
                foreach (var item in makingDetail)
                {
                    item.EndTime = DateTime.Now;
                    item.ModifiedBy = currentUserId;
                    item.ModifiedOn = DateTime.Now;
                    item.Status = OrderBatchMakeStatus.Complete;
                }

                var detailIds = makingDetail.Select(a => a.Id).ToList();
                var makeIds = makingDetail.Select(a => a.KitchenMakeId).Distinct().ToList();
                //将厨房状态改为已完成(KitchenMake)
                var kitchenMakes = (from x in context.KitchenMakes
                                    join y in context.KitchenMakeDetails.Where(a => a.IsDeleted != 1
                                        && a.Status != OrderBatchMakeStatus.Complete
                                        && detailIds.Contains(a.Id) == false
                                        ) on x.Id equals y.KitchenMakeId into temp
                                    where makeIds.Contains(x.Id)
                                       && x.IsDeleted != 1
                                       && x.Status == OrderBatchMakeStatus.Making
                                       && temp.Count() == 0
                                    select x).ToList();
                //循环变更状态
                foreach (var kitchenMake in kitchenMakes)
                {
                    //变更厨房订单状态
                    kitchenMake.Status = OrderBatchMakeStatus.Complete;
                    kitchenMake.ModifiedBy = currentUserId;
                    kitchenMake.ModifiedOn = DateTime.Now;

                    //变更订单状态
                    var order = context.Orders.SingleOrDefault(a => a.IsDeleted != 1 && a.No.Equals(kitchenMake.OrderNo));
                    if (order == null)
                        return false;
                    order.Status = OrderStatus.MakeCompleted;//制作完成
                    order.ModifiedBy = currentUserId;
                    order.ModifiedOn = DateTime.Now;
                }

                makeIds = kitchenMakes.Select(a => a.Id).ToList();

                //将批次状态设置为完成
                var orderBatch = context.OrderBatchs.SingleOrDefault(a => a.IsDeleted != 1 && a.BatchNo == batchNo);
                if (orderBatch == null)
                    return false;
                var tempKitchenMakes = context.KitchenMakes.Where(a => a.IsDeleted != 1 && a.BatchNo.Equals(batchNo));
                if (tempKitchenMakes.Any(a => a.Status != OrderBatchMakeStatus.Complete && makeIds.Contains(a.Id) == false) == false)
                {
                    orderBatch.MakeStatus = OrderBatchMakeStatus.Complete;
                    orderBatch.MakeEndTime = DateTime.Now;
                    orderBatch.ModifiedBy = currentUserId;
                    orderBatch.ModifiedOn = DateTime.Now;



                    //转移数据
                    var os = (from x in context.Orders
                              join y in context.KitchenMakes on x.No equals y.OrderNo
                              where y.BatchNo.Equals(orderBatch.BatchNo)
                              && x.IsDeleted != 1
                              && y.IsDeleted != 1
                              select x).ToList();
                    InsertDistributions(os, currentUserId);
                }

                return context.SaveChanges() > 0;
            }

            return false;
        }
        #endregion

        #region 数据转移
        public void InsertDistributions(List<Orders> orders, string currentUserId)
        {
            foreach (var x in orders)
            {
                Distribution dis = new Distribution();
                dis.Id = DataHelper.GetSystemID();
                //dis.BeginTime = DateTime.Now;
                dis.CreatedBy = currentUserId;
                dis.CreatedOn = DateTime.Now;
                dis.IsDeleted = 0;
                dis.OrderNo = x.No;
                dis.RequiredTime = x.RequiredTime;
                dis.Address = x.ReceiverArea + x.ReceiverAddr;
                dis.Status = StatusDistribution.DistributionPending;
                dis.feeType = x.FeeType;
                context.Distribution.Add(dis);
            }
        }
        #endregion

        #region 数据逻辑
        /// <summary>
        /// 由批次号获取厨房信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="batchId"></param>
        /// <returns></returns>
        public List<KitchenInfo> GetInfoByBatchNo(string batchNo)
        {
            List<KitchenInfo> resultList = new List<KitchenInfo>();
            var outType = typeof(KitchenInfo);
            var outProInfos = outType.GetProperties();
            var outNames = outProInfos.Select(a => a.Name);
            //取餐具类型
            //var otherProductType = CommonRules.OtherProductTypeDicValue;

            var result = (from kitchen in context.KitchenMakes
                          join batch in context.OrderBatchs on kitchen.BatchNo equals batch.BatchNo
                          join kitchenDetail in context.KitchenMakeDetails on kitchen.Id equals kitchenDetail.KitchenMakeId
                          join subProduct in context.SubProducts on kitchenDetail.SubProductId equals subProduct.Id
                          join order in context.Orders on kitchen.OrderNo equals order.No
                          join orderDetail in context.OrderDetails on kitchenDetail.SubProductId equals orderDetail.SubProductId
                          join product in context.Products on orderDetail.ProductId equals product.Id
                          where kitchen.IsDeleted != 1
                            && kitchenDetail.IsDeleted != 1
                            && subProduct.IsDeleted != 1
                            && order.IsDeleted != 1
                            && orderDetail.IsDeleted != 1
                            && batch.IsDeleted != 1
                            && product.IsDeleted != 1
                            && kitchen.BatchNo.Equals(batchNo)
                            && orderDetail.OrderNo.Equals(kitchen.OrderNo)
                            && batch.Status == BatchReviewStatus.ReviewPass
                          select new KitchenInfo
                          {
                              BatchNo = kitchen.BatchNo,//批次号
                              OrderNo = order.No, //订单号
                              BatchRequiredTime = batch.RequiredTime,//要求完成时间
                              OrderRequiredTime = order.RequiredTime,//订单要求送达时间
                              ProductName = product.Name,//蛋糕名称
                              Size = subProduct.Size,//磅数
                              SizeTitle = "",
                              Num = orderDetail.Num,//数量
                              OrderStatus = kitchen.Status,//订单状态
                              KitStatus = (OrderBatchMakeStatus)(batch.MakeStatus == null ? OrderBatchMakeStatus.NotStart : batch.MakeStatus),//批次状态
                              ProductStatus = kitchenDetail.Status,//产品状态
                              ProductId = product.Id,//产品ID
                              SubProductId = subProduct.Id,//子产品ID
                              KitchenMakeDetailId = kitchenDetail.Id//厨房明细ID
                          }).ToList();

            foreach (var item in result)
            {
                item.SizeTitle = new FCake.Bll.ProductService().GetSizeTitleBuySubProductId(item.SubProductId);
                KitchenInfo outItem = new KitchenInfo();
                var type = item.GetType();
                var proInfos = type.GetProperties();
                foreach (var x in proInfos.Where(p => outNames.Contains(p.Name)))
                {
                    var p = outProInfos.SingleOrDefault(a => a.Name.Equals(x.Name));
                    p.SetValue(outItem, x.GetValue(item, null), null);
                }
                resultList.Add(outItem);
            }

            return resultList;
        }
        /// <summary>
        /// 取出上一个批次号
        /// </summary>
        /// <param name="currentBatchNo"></param>
        /// <returns></returns>
        public string GetPreBatchNo(string currentBatchNo)
        {
            var result = (from x in context.OrderBatchs.OrderBy(a => a.RequiredTime)
                          where x.IsDeleted != 1
                          && x.MakeStatus != OrderBatchMakeStatus.Complete
                          && x.Status == BatchReviewStatus.ReviewPass
                          select x.BatchNo).ToList();

            var curIndex = result.IndexOf(currentBatchNo);

            //没有值
            if (result.Any() == false)
                return "";
            //如果没有当前的批次号 返回第一个
            if (curIndex == -1)
                return result[0];
            //当为第一个时
            if (curIndex == 0)
                return currentBatchNo;

            return result[curIndex - 1];
        }
        /// <summary>
        /// 取出下一个批次号
        /// </summary>
        /// <param name="nextBatchNo"></param>
        /// <returns></returns>
        public string GetNextBatchNo(string currentBatchNo)
        {
            var result = (from x in context.OrderBatchs.OrderBy(a => a.RequiredTime)
                          where x.IsDeleted != 1
                          && x.MakeStatus != OrderBatchMakeStatus.Complete
                          && x.Status == BatchReviewStatus.ReviewPass
                          select x.BatchNo).ToList();

            var curIndex = result.IndexOf(currentBatchNo);

            //没有值
            if (result.Any() == false)
                return "";
            //如果没有当前的批次号 返回第一个
            if (curIndex == -1)
                return result[0];
            //当为最后一个时
            if (curIndex == result.Count() - 1)
                return "";

            return result[curIndex + 1];
        }
        #endregion
    }
}
