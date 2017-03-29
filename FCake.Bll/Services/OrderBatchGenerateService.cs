using FCake.Core.Common;
using FCake.Domain;
using FCake.Domain.Entities;
using FCake.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FCake.Bll.Services
{
    public class OrderBatchGenerateService
    {
        EFDbContext context = new EFDbContext();
        public OrderBatchGenerateService() {
        }
        public OrderBatchGenerateService(double earlyHours, int maxOrdersNum, DateTime calculatingTime, string userId)
        {
            EarlyHours = earlyHours;
            MaxOrdersNum = maxOrdersNum;
            MaxRequiredTime = DateTime.Now.AddHours(earlyHours);
            ExecUserId = userId;
        }
        private DateTime _nowTime = DateTime.Now;
        /// <summary>
        /// 提前生成批次时间参数(单位小时)
        /// </summary>
        private double _earlyHours = 12;

        public double EarlyHours
        {
            get { return _earlyHours; }
            set { _earlyHours = value; }
        }

        /// <summary>
        /// 每个批次允许的最多蛋糕数量(=-1不限制批次的蛋糕数量)
        /// </summary>
        private int _maxOrdersNum = -1;

        public int MaxOrdersNum
        {
            get { return _maxOrdersNum; }
            set { _maxOrdersNum = value; }
        }

        /// <summary>
        /// 本批次订单最迟要求送达时间
        /// </summary>
        private DateTime _maxRequiredTime = DateTime.Now;

        public DateTime MaxRequiredTime
        {
            get { return _maxRequiredTime; }
            set { _maxRequiredTime = value; }
        }
        /// <summary>
        /// 执行人
        /// </summary>
        private string _execUserId = string.Empty;

        public string ExecUserId
        {
            get { return _execUserId; }
            set { _execUserId = value; }
        }

        public void ExecuteGenerate()
        {
            _nowTime = DateTime.Now;
            //获取最新的未生成批次的订单数据列表
            var orderList = GetOrderListNotBatch();
            if (orderList != null)
            {
                //生成生产批次数据
                var batch = CreateBatch(orderList);
                context.OrderBatchs.Add(batch);
                //修改订单状态
                orderList.ForEach(order =>
                {
                    order.Status = OrderStatus.Scheduled;
                });
                context.SaveChanges();
            }
        }

        public List<Orders> GetOrderListNotBatch()
        {
            var maxTime = MaxRequiredTime;
            //根据过滤条件获得本次需要进入排产批次的订单（订单要求送达时间在本批次范围内;必须是审核通过的;且状态为若在线订单则必须是已支付的,若电话订单则是未支付的;未被删除的订单）
            
            var orders = context.Orders.Include("OrderDetails").Where(o => o.ReviewStatus == ReviewStatus.ReviewPass
                && (o.RequiredTime <= maxTime)
                //todo:delete 更改订单状态
                //&& ((o.OrderSource == OrderSource.OnlineOrder && o.Status == OrderStatus.HadPaid) || (o.OrderSource == OrderSource.TelOrder && o.Status == OrderStatus.NotPay))
                && o.Status==OrderStatus.HadPaid
                && o.IsDeleted != 1).OrderBy(o => o.RequiredTime);
            var orderList = orders.ToList();
            return orderList;
        }

        public OrderBatch CreateBatch(List<Orders> orders)
        {
            var batchNo = GetBatchNo(_nowTime);
            //取餐具类型
            var otherProductType = CommonRules.OtherProductTypeDicValue;
            var batch = context.OrderBatchs.Where(ob => ob.BatchNo == batchNo).FirstOrDefault();
            if (batch != null)
            {
                batchNo = GetBatchNo(_nowTime.AddMilliseconds(1));
            }
            var cakeNum = orders.Select(o => o.OrderDetails).Count();
            //过滤掉餐具
            if (cakeNum > 0)
            {
                foreach (var item in orders)
                {
                    foreach (var i in item.OrderDetails)
                    {
                        var product = context.Products.SingleOrDefault(p => p.IsDeleted != 1 && p.Id.Equals(i.ProductId));
                        if (product != null)
                        {
                            if (product.Type == otherProductType)
                            {
                                cakeNum -= i.Num;
                            }
                        }
                    }
                }
            }
            if (cakeNum < 1)
            {
                cakeNum = 0;
            }

            var newBatch = new OrderBatch(DataHelper.GetSystemID(), ExecUserId, batchNo, orders.Count(), cakeNum);
            var kitchenMakes = CreateKitchenMakes(newBatch, orders);
            return newBatch;
        }

        public List<KitchenMake> CreateKitchenMakes(OrderBatch orderBatch, List<Orders> orders)
        {
            List<KitchenMake> makes = new List<KitchenMake>();
            foreach (var item in orders)
            {
                var make = new KitchenMake(DataHelper.GetSystemID(), orderBatch.BatchNo, item.No, ExecUserId);
                makes.Add(make);
                if (item.OrderDetails != null)
                {
                    var details = CreateKitchenMakeDetails(make, item.OrderDetails);
                    context.KitchenMakeDetails.AddRange(details);
                }
            }
            context.KitchenMakes.AddRange(makes);
            return makes;
        }

        public List<KitchenMakeDetail> CreateKitchenMakeDetails(KitchenMake make, ICollection<OrderDetails> orderDetails)
        {
            List<KitchenMakeDetail> makeDetails = null;
            makeDetails = new List<KitchenMakeDetail>();
            foreach (var detailItem in orderDetails)
            {
                var makeDetail = new KitchenMakeDetail(DataHelper.GetSystemID(), ExecUserId, make.Id, detailItem.SubProductId);
                makeDetails.Add(makeDetail);
            }
            return makeDetails;
        }

        public string GetBatchNo(DateTime calculatingTime)
        {
            return calculatingTime.ToString("yyyyMMddHHmmssfff");
        }
    }
}
