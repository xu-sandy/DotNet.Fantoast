using FCake.Core.MvcCommon;
using FCake.Domain;
using FCake.Domain.Common;
using FCake.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCake.Domain.Enums;
using FCake.Core.Common;
using FCake.API.EChi;
using FCake.Domain.WebModels;
using FCake.API;

namespace FCake.Bll.Services
{
    public class DistributionService
    {
        EFDbContext context = new EFDbContext();
        private readonly OrderService _orderService = new OrderService();
        /// <summary>
        /// 获取配送信息
        /// </summary>
        /// <param name="totalCount"></param>
        /// <param name="pageInfo"></param>
        /// <returns></returns>
        public List<Distribution> GetDistributionData(StatusDistribution? status, DateTime? dateReqFrom, DateTime? dateReqEnd, out int totalCount, int page = 1, int rows = 20)
        {
            var query = from d in context.Distribution
                        join o in context.Orders
                        on d.OrderNo equals o.No
                        where d.IsDeleted != 1 && o.IsDeleted != 1
                        select d;
            if (status.ToString() != "")
            {
                query = query.Where(d => d.Status == status);
            }
            if (dateReqFrom != null || dateReqEnd != null)
            {
                if (!dateReqFrom.HasValue)
                {
                    query = query.Where(d => d.RequiredTime <= dateReqEnd);
                }
                if (!dateReqEnd.HasValue)
                {
                    query = query.Where(d => d.RequiredTime >= dateReqFrom);
                }
                if (dateReqFrom.HasValue && dateReqEnd.HasValue)
                {
                    query = query.Where(d => d.RequiredTime >= dateReqFrom && d.RequiredTime <= dateReqEnd);
                }
            }
            totalCount = query.Count();
            var data = query.OrderBy(d => d.Status).ThenBy(d => d.RequiredTime).Skip((page - 1) * rows).Take(rows).ToList();
            return data;
        }
        public List<DistributionModel> GetData(string curDate, int? period, StatusDistribution? status, out int totalCount, int page = 1, int rows = 20)
        {
            //构造在查询时间范围内的订单数据
            var orders = context.Orders.Where(o => o.IsDeleted != 1 && o.ReviewStatus == ReviewStatus.ReviewPass);
            
            //过期未处理、今日任务、明日计划数据判断
            var yestoday = DateTime.Today.AddDays(-1);
            var today = DateTime.Today;
            var tomorrow = DateTime.Today.AddDays(1);
            var afterTomorrow = DateTime.Today.AddDays(2);
            switch (curDate)
            {
                case "today":
                    {
                        var btime = today.AddHours(13);
                        switch (period)
                        {
                            case 1:
                                orders = orders.Where(o => o.RequiredTime >= today && o.RequiredTime < btime);
                                break;
                            case 2:
                                orders = orders.Where(o => o.RequiredTime >= btime && o.RequiredTime < tomorrow);
                                break;
                            default:
                                orders = orders.Where(o => o.RequiredTime >= today && o.RequiredTime < tomorrow);
                                break;
                        }
                    }
                    break;
                case "tomorrow":
                    orders = orders.Where(o => o.RequiredTime >= tomorrow && o.RequiredTime < afterTomorrow);
                    break;
                case "expires":
                    orders = orders.Where(o => o.RequiredTime < DateTime.Today);
                    break;
                default:
                    break;
            }
            //构造返回数据集合
            var query = from o in orders
                        join d in context.Distribution
                        on o.No equals d.OrderNo into temp
                        from t in temp.DefaultIfEmpty()
                        select new DistributionModel()
                        {
                            OrderNo = o.No,
                            RequiredTime = o.RequiredTime,
                            feeType = o.FeeType,
                            Address = o.ReceiverArea + o.ReceiverAddr,
                            BeginTime = t == null ? null : t.BeginTime,
                            EndTime = t == null ? null : t.EndTime,
                            Status = t == null ? null : t.Status,
                            NeedPay = o.NeedPay,
                            RequiredTimeBucket = o.RequiredTimeBucket,
                            GiftCardPayed = o.GiftCardPay
                        };
            if (status != null)
            {
                query = query.Where(d => d.Status == status);
            }

            totalCount = query.Count();
            //过期未处理倒序排列、今日任务与明日计划数据判断正序排列
            switch (curDate)
            {
                case "today":
                    query = query.OrderByDescending(d => d.RequiredTime);
                    break;
                case "tomorrow":
                    query = query.OrderBy(d => d.RequiredTime);
                    break;
                case "expires":
                    query = query.OrderByDescending(d => d.RequiredTime);
                    break;
                default:
                    break;
            }
            var date = query.Skip((page - 1) * rows).Take(rows).ToList();
            return date;
        }

        public List<T> GetOrderDetail<T>(string orderNo) where T : new()
        {
            List<T> resultList = new List<T>();
            var outType = typeof(T);
            var outProInfos = outType.GetProperties();
            var outNames = outProInfos.Select(a => a.Name);

            var result = (from orderDetail in context.OrderDetails
                          join order in context.Orders on orderDetail.OrderNo equals order.No
                          join customer in context.Customers on order.CustomerId equals customer.Id
                          join product in context.Products on orderDetail.ProductId equals product.Id
                          where orderDetail.IsDeleted != 1 && order.IsDeleted != 1 && customer.IsDeleted != 1
                             && orderDetail.OrderNo.Equals(orderNo)
                          select new
                          {
                              Receiver = order.Receiver,//收货人
                              ReceiverAddr = order.ReceiverProvince + order.ReceiverCity + order.ReceiverArea + order.ReceiverAddr,//收获地址
                              ReceiverMobile = order.ReceiverMobile,//收货人手机
                              ReceiverTel = order.ReceiverTel,//收货人座机
                              Customer = customer.FullName,//订货人姓名
                              Candle = order.Candle, //是否要生日蜡烛： 0是 1否

                              ProductName = product.Name,//产品名称
                              Size = orderDetail.Size,//磅数
                              SizeTitle=orderDetail.SizeTitle,
                              Num = orderDetail.Num,//数量
                              TotalPrice = orderDetail.TotalPrice, //小计
                              BirthdayCard = orderDetail.BirthdayCard
                          }).ToList();

            foreach (var item in result)
            {
                T outItem = new T();
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
        /// 操作状态:未配送[开始Btn]->配送中[完成Btn]->配送完成；配送异常
        /// </summary>
        /// <param name="orderNo"></param>
        /// <param name="status"></param>
        /// <param name="modifyUser"></param>
        /// <returns></returns>
        public OpResult StatusHandle(string orderNo, StatusDistribution status, string modifyUser, bool isSMS)
        {
            var result = new OpResult();
            var isSendMsg = false;
            try
            {
                var distribution = context.Distribution.Where(d => d.OrderNo == orderNo && d.IsDeleted != 1).FirstOrDefault();
                var order = context.Orders.Where(o => o.No == orderNo && o.IsDeleted != 1).FirstOrDefault();
                if (distribution != null)
                {
                    if (distribution.Status != (StatusDistribution)status)
                    {
                        result.Successed = false;
                        result.Message = "数据已过期,请刷新页面";
                    }
                    else
                    {
                        if (distribution.Status == StatusDistribution.DistributionPending)
                        {
                            distribution.Status = StatusDistribution.Distributing;
                            distribution.BeginTime = DateTime.Now;
                            order.Status = OrderStatus.Delivery;
                            if (isSMS)
                            {
                                isSendMsg = true;
                            }
                        }
                        else if (distribution.Status == StatusDistribution.Distributing)
                        {
                            distribution.Status = StatusDistribution.Distributed;
                            distribution.EndTime = DateTime.Now;
                            order.Status = OrderStatus.Completed;
                        }

                        distribution.ModifiedOn = DateTime.Now;
                        distribution.ModifiedBy = modifyUser;
                        context.SaveChanges();
                        result.Successed = true;
                        result.Message = "操作成功";
                        result.Data = new Dictionary<string, string>() {
                            { "status", ((int)distribution.Status).ToString() },
                            { "beginTime", distribution.BeginTime.ToString() },
                            { "endTime", distribution.EndTime.ToString() },
                        };
                        if (order.Status == OrderStatus.Completed)
                        {//订单完成后更新会员信息
                            _orderService.CompletedOrderUpdateMemberInfo(order.No);
                        }
                    }
                }
                else
                {
                    result.Successed = false;
                    result.Message = "操作失败";
                }
            }
            catch (Exception ex)
            {
                result.Successed = false;
                result.Message = ex.Message;
            }

            //发送短信
            if (isSendMsg)
            {
                //SendBeginMessage(orderNo, new MsgTemplateService().GetMsgTempByCategory("Distribution"));
                SendBeginMessage(orderNo, DaYuConfig.BeginDeliveryTemplate);

            }
            return result;
        }

        /// <summary>
        /// 撤销物流配送回未配送状态，只当为配送中时
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="status"></param>
        /// <param name="modifyUser"></param>
        /// <returns></returns>
        public OpResult StatusRevert(string orderNo, StatusDistribution status, string modifyUser)
        {
            var result = new OpResult();
            try
            {
                var distribution = context.Distribution.Where(d => d.OrderNo == orderNo && d.IsDeleted != 1).FirstOrDefault();
                var order = context.Orders.Where(o => o.No == orderNo && o.IsDeleted != 1).FirstOrDefault();
                if (distribution != null)
                {
                    if (distribution.Status != (StatusDistribution)status)
                    {
                        result.Successed = false;
                        result.Message = "数据已过期,请刷新页面";
                    }
                    else
                    {
                        if (distribution.Status == StatusDistribution.Distributing)
                        {
                            distribution.Status = StatusDistribution.DistributionPending;
                            distribution.BeginTime = null;//开始配送时间清空
                            order.Status = OrderStatus.MakeCompleted;
                        }
                        distribution.ModifiedOn = DateTime.Now;
                        distribution.ModifiedBy = modifyUser;
                        context.SaveChanges();
                        result.Successed = true;
                        result.Message = "操作成功";
                        result.Data = distribution.Status;
                    }
                }
            }
            catch (Exception ex)
            {
                result.Successed = false;
                result.Message = ex.Message;
            }
            return result;
        }

        public string SaveOrderException(string orderNo, string msg, string currentUserId)
        {
            try
            {
                var order = context.Orders.SingleOrDefault(a => a.IsDeleted != 1 && a.No.Equals(orderNo));
                if (order == null)
                    return "查无订单信息";
                if (msg.Trim() == "")
                    return "异常描述不能为空";

                OrderException exception = new OrderException();
                exception.CreatedBy = currentUserId;
                exception.CreatedOn = DateTime.Now;
                exception.Id = DataHelper.GetSystemID();
                exception.OrderNo = orderNo;
                exception.IsDeleted = 0;
                exception.Description = msg;
                context.OrderExceptions.Add(exception);

                order.Status = OrderStatus.TradingClosed;
                order.ModifiedBy = currentUserId;
                order.ModifiedOn = DateTime.Now;

                Distribution dis = context.Distribution.SingleOrDefault(a => a.IsDeleted != 1 && a.OrderNo.Equals(orderNo));
                if (dis == null)
                    return "查无物流信息";
                dis.Status = StatusDistribution.DistributionFalse;
                dis.ModifiedBy = currentUserId;
                dis.ModifiedOn = DateTime.Now;

                context.SaveChanges();
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// 配送开始短信通知
        /// </summary>
        /// <param name="orderNo"></param>
        /// <param name="msgContent"></param>
        /// <returns></returns>
        public OpResult SendBeginMessage(string orderNo, string msgTemplateId)
        {
            OpResult result = new OpResult();
            result.Successed = false;
            var order = context.Orders.Where(p => p.No.Equals(orderNo) && p.IsDeleted != 1).SingleOrDefault();
            if (order != null)
            {
                var customer = context.Customers.Where(p => p.Id.Equals(order.CustomerId) && p.IsDeleted != 1).SingleOrDefault();
                if (customer != null)
                {
                    bool rt = false;
                    if (customer.Mobile.IsNullOrTrimEmpty() == false)
                    {
                        //rt = EChiHelper.SendSMSResult(customer.Mobile, msgContent);
                        var sendSMSErrorMsg = string.Empty;
                        rt = DaYuSMSHelper.SendNotifySMS(customer.Mobile, msgTemplateId, out sendSMSErrorMsg);
                        if (!rt)
                        {
                            SysLogService.SaveAliSMSErrorLog(sendSMSErrorMsg, customer.Mobile, msgTemplateId);
                        }

                    }
                    else
                    {
                        result.Message = orderNo + "未留手机号";
                    }

                    if (rt)
                    {
                        result.Successed = true;
                    }
                    else
                    {
                        result.Message = "发送失败";
                    }
                }
                else
                {
                    result.Message = orderNo + "：查不到客户";
                }
            }
            else
            {
                result.Message = orderNo + "：查不到订单";
            }
            return result;
        }

        /// <summary>
        /// 批量开始某段时间的配送
        /// </summary>
        /// <param name="minDate">时间段起点（默认今日开始）</param>
        /// <param name="maxDate">时间段终点（默认今日结束）</param>
        /// <returns></returns>
        public bool BegioSection(DateTime? minDate, DateTime? maxDate, string userID, bool isSMS)
        {
            var mtsv = new MsgTemplateService();

            if (minDate == null) minDate = DateTime.Today;
            if (maxDate == null) maxDate = DateTime.Today.AddDays(1);

            var dis = context.Distribution.Include("Order").Include("Order.Customers").Where(d => d.Status == StatusDistribution.DistributionPending && d.RequiredTime >= minDate && d.RequiredTime < maxDate && d.IsDeleted != 1).ToList();
            if (dis!=null && dis.Count>0)
            {
                foreach (var record in dis)
                {
                    record.Status = StatusDistribution.Distributing;
                    record.BeginTime = DateTime.Now;
                    record.ModifiedBy = userID;
                    record.ModifiedOn = DateTime.Now;
                    record.Order.Status = OrderStatus.Delivery;

                    if (isSMS)
                    {
                        try
                        {
                            //EChiHelper.SendSMS(record.Order.Customers.Mobile, string.Format(mtsv.GetMsgTempByCategory("Distribution"), record.OrderNo));
                            var sendSMSErrorMsg = string.Empty;
                            var sendResult = DaYuSMSHelper.SendNotifySMS(record.Order.Customers.Mobile, DaYuConfig.BeginDeliveryTemplate, out sendSMSErrorMsg);
                            if (!sendResult)
                            {
                                SysLogService.SaveAliSMSErrorLog(sendSMSErrorMsg, record.Order.Customers.Mobile, DaYuConfig.BeginDeliveryTemplate);
                            }
                        }
                        catch
                        {

                        }
                    }
                }
                //todo:怎么没有修改状态代码？ bug
                return context.SaveChanges() > 0;
            }
            return false;
        }
    }
}
