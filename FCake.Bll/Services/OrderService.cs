using FCake.Domain;
using FCake.Domain.Entities;
using FCake.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCake.Core.Common;
using System.Web.Mvc;
using LinqKit;
using FCake.Core.MvcCommon;
using FCake.API.EChi;
using System.Linq.Expressions;
using System.Configuration;
using FCake.Domain.WebModels;
using FCake.Bll.Services;
using System.Data.SqlClient;
using System.Data;
using FCake.API;

namespace FCake.Bll
{
    public partial class OrderService : BaseService
    {
        public EFDbContext context = new EFDbContext();
        private readonly CommonService _commonService = new CommonService();
        private readonly CouponsService _couponsService = new CouponsService();
        private readonly GiftCardDetailService _giftCardDetailService = new GiftCardDetailService();
        private readonly CustomersService _customersService = new CustomersService();
        private readonly ProductService _productService = new ProductService();

        #region redo
        /// <summary>
        /// 创建订单
        /// </summary>
        /// <param name="getCustomer">获取客户</param>
        /// <param name="getCustomerAddress">获取客户地址</param>
        /// <param name="getDetails">获取订单产品详细</param>
        /// <param name="invoice">发票数据</param>
        /// <param name="setOrder">初始化订单数据</param>
        /// <param name="currentUserID">当前管理用户</param>
        /// <returns></returns>
        public OpResult CreateOrder(
            Expression<Func<Customers, bool>> getCustomer,
            Action<Orders> setOrderAddress,
            Func<IQueryable<SubProduct>, IEnumerable<OrderDetails>> getDetails,
            Invoice invoice,
            Action<Orders> setOrder,
            string currentUserID, int orderClient, string timeBucket,
            List<string> couponDetailIdList, List<string> giftCardDetailIdList,
            decimal orderTotalAmount)
        {
            Customers customer = null;
            Orders order = new Orders();
            IEnumerable<OrderDetails> orderDetails = new List<OrderDetails>();

            #region 取客户信息 如果没有返回NULL
            //取客户
            customer = SingleOrDefault(getCustomer);
            //如果客户为NULL  返回NULL
            if (customer == null)
                return new OpResult { Successed = false, Message = "找不到客户信息" };
            #endregion

            #region 如果没有产品 那就不用处理 直接返回NULL
            //取出订单产品明细
            orderDetails = getDetails(GetQuery<SubProduct>());
            if (orderDetails.Any() == false)
                return new OpResult { Successed = false, Message = "找不到订单产品数据" };
            #endregion

            #region 添加订单数据 并验证订单数据
            setOrder(order);
            //不能事先设置订单ID
            order.Id = "";
            //创建订单号
            order.No = CommonRules.CommonNoRules("orderno");
            //客户ID
            order.CustomerId = customer.Id;
            //未支付
            //todo:delete
            //order.Status = OrderStatus.NotPay;
            order.Status = CreateOrderStatusByFeeType(order);
            order.TradeStatus = TradeStatus.NotPay;
            //必须要有接收时间且大于当前时间
            if (order.RequiredTime == null || order.RequiredTime <= DateTime.Now)
                return new OpResult { Successed = false, Message = "接收时间错误" };
            #endregion

            #region 地址变更
            if (setOrderAddress == null)
                return new OpResult { Successed = false, Message = "找不到客户地址数据" };
            //回写地址信息到订单表
            setOrderAddress(order);
            DAL.AddOrModify(order, currentUserID);
            #endregion

            #region 添加订单产品信息 更改订单中相关数据
            decimal allowFavorableAmount = 0;//允许使用优惠券、代金卡部分，排除产品类型为其他（餐具、蜡烛等）
            decimal favorablePay = 0;//使用优惠合计
            decimal tempOrderTotalAmount = 0;//订单总额
            favorablePay = order.CouponPay + order.GiftCardPay + order.IntegralPay;
            foreach (var x in orderDetails)
            {
                x.OrderNo = order.No;
                DAL.AddOrModify(x, currentUserID);
                //下面这个是奇葩的东西
                x.No = x.Id;
                //更新SizeTitle
                var product = context.Products.Where(o => o.Id == x.ProductId).FirstOrDefault();
                if (product != null)
                {
                    //更新子产品的价格
                    x.Price = _productService.GetLastProductPrice(x.SubProductId, x.Price, product.Type, true, order.CustomerId);
                    x.TotalPrice = x.Price * x.Num;
                    tempOrderTotalAmount += x.TotalPrice;
                    //更新子产品SizeTitle
                    x.SizeTitle = product.Type == CommonRules.OtherProductTypeDicValue ? _commonService.GetDictionaryName("ProductUnitOther", x.Size) : _commonService.GetDictionaryName("ProductUnitCake", x.Size);
                    if (product.Type != CommonRules.OtherProductTypeDicValue)
                    {
                        allowFavorableAmount += x.TotalPrice;
                    }
                }

            }

            order.TotalPrice = tempOrderTotalAmount;
            order.NeedPay = order.TotalPrice - favorablePay;
            #region 验证订单金额部分
            if (order.TotalPrice != orderTotalAmount)
            {
                return new OpResult { Successed = false, Message = "对不起，订单总额数据异常，请刷新后重试" };
            }
            if (favorablePay > allowFavorableAmount)
            {
                return new OpResult { Successed = false, Message = "使用优惠部分不能大于允许使用优惠部分" };
            }
            if (order.NeedPay < 0)
            {
                return new OpResult { Successed = false, Message = "需要支付金额部分不能小于0" };
            }
            var integralDeductionCashRate = CommonRules.IntegralDeductionCashRate;//积分抵扣现金比例
            if (Math.Round(order.UsedIntegralVal / integralDeductionCashRate, 2, MidpointRounding.AwayFromZero) != Math.Round(order.IntegralPay, 2, MidpointRounding.AwayFromZero))
            {
                return new OpResult { Successed = false, Message = "使用积分支付数据异常，请刷新后重试" };
            }
            if (order.NeedPay == 0)
            {
                order.TradeStatus = TradeStatus.HadPaid;
                order.Status = OrderStatus.HadPaid;
            }
            #endregion

            order.ActualPay = 0;
            order.OrderClient = orderClient;
            order.RequiredTimeBucket = timeBucket;

            #endregion

            #region 添加发票信息
            if (invoice != null)
            {
                invoice.OrderId = order.Id;
                DAL.AddOrModify(invoice, currentUserID);
            }
            #endregion

            #region 审核状态
            if (order.FeeType == FeeType.ALiPay || order.FeeType == FeeType.TenPay || order.FeeType == FeeType.WXPay)
            {
                order.ReviewStatus = ReviewStatus.ReviewOnLineNoPay;
                if (order.NeedPay == 0)
                    order.ReviewStatus = ReviewStatus.ReviewPending;
            }
            #endregion

            #region 设置已使用优惠券、代金卡、积分状态
            var setResult = SetCouponDetailsToUsed(couponDetailIdList, order.No, order.CustomerId, false);
            if (setResult.Successed == false)
                return setResult;
            setResult = SetGiftCardDetailToUsed(giftCardDetailIdList, order.No, order.CustomerId, false);
            if (setResult.Successed == false)
                return setResult;
            setResult = DeductMemberIntegral(order.CustomerId, order.UsedIntegralVal, order.No, order.ActualPay, currentUserID, false);
            if (setResult.Successed == false)
                return setResult;

            #endregion

            DAL.Commit();
            return new OpResult { Data = order, Successed = true };
        }
        /// <summary>
        /// 编辑订单
        /// </summary>
        /// <param name="getCustomerAddress">获取客户地址</param>
        /// <param name="getDetails">获取订单产品详细</param>
        /// <param name="invoice">发票数据</param>
        /// <param name="setOrder">初始化订单数据</param>
        /// <param name="currentUserID">当前管理用户</param>
        /// <returns></returns>
        public OpResult EditOrder(
            Action<Orders> setOrderAddress,
            Func<IQueryable<SubProduct>, IEnumerable<OrderDetails>> getDetails,
            Invoice invoice,
            Action<Orders> setOrder,
            string currentUserID)
        {
            Customers customer = null;
            Orders temporder = new Orders();
            Orders order = null;
            IEnumerable<OrderDetails> orderDetails = new List<OrderDetails>();
            //decimal preWalletPay = 0;

            #region 如果没有产品 那就不用处理 直接返回NULL
            //取出订单产品明细
            orderDetails = getDetails(GetQuery<SubProduct>());
            if (orderDetails.Any() == false)
                return new OpResult { Successed = false, Message = "找不到订单产品数据" };
            #endregion

            #region 取出回传的订单数据
            setOrder(temporder);
            order = SingleOrDefault<Orders>(a => a.No.Equals(temporder.No, StringComparison.OrdinalIgnoreCase));
            if (order == null)
                return new OpResult { Successed = false, Message = "找不到订单数据" };
            //保存订单历史
            #region
            var hist = new OrderHist();
            hist.CopyProperty(order);
            hist.OrderId = order.Id;
            DAL.AddOrModify(hist, currentUserID);
            hist.CreatedBy = order.CreatedBy;
            hist.ModifiedBy = order.ModifiedBy;
            hist.CreatedOn = order.CreatedOn;
            hist.ModifiedOn = order.ModifiedOn;
            #endregion
            //钱包 如果为NULL 改为0 且钱包支付不能大于总价
            //order.CouponPay = order.CouponPay;
            //preWalletPay = order.CouponPay;

            setOrder(order);
            //必须要有接收时间且大于当前时间
            if (order.RequiredTime == null || order.RequiredTime <= DateTime.Now)
                return new OpResult { Successed = false, Message = "收货时间有误!" };
            #endregion

            #region 如果客户为NULL  返回NULL
            string customerID = order.CustomerId;
            //取客户
            customer = SingleOrDefault<Customers>(a => a.Id.Equals(customerID));
            //如果客户为NULL  返回NULL
            if (customer == null)
                return new OpResult { Successed = false, Message = "找不到客户信息" };
            #endregion

            #region 地址变更
            if (setOrderAddress == null)
                return new OpResult { Successed = false, Message = "找不到客户地址数据" };
            //回写地址信息到订单表
            setOrderAddress(order);
            DAL.AddOrModify(order, currentUserID);
            #endregion
            string no = order.No;
            //订单只产品
            var details = GetQuery<OrderDetails>(a => a.OrderNo.Equals(no, StringComparison.OrdinalIgnoreCase));

            #region 变更生日牌
            //请卡控条件 
            if (true)
            {
                details.ForEach(a =>
                {
                    var temp = orderDetails.SingleOrDefault(o => o.SubProductId.Equals(a.SubProductId));
                    a.BirthdayCard = temp == null ? "" : temp.BirthdayCard;
                });
            }
            #endregion

            #region 变更子产品数据
            //todo:delete 更改订单状态
            //if (order.OrderSource == OrderSource.TelOrder && order.Status == OrderStatus.NotPay)
            if (order.TradeStatus == TradeStatus.NotPay)
            {
                //保存历史
                foreach (var x in details)
                {
                    var detailhist = new OrderDetailHist();
                    detailhist.CopyProperty(x);
                    detailhist.OrderDetailId = x.Id;
                    DAL.AddOrModify(detailhist, currentUserID);
                    detailhist.CreatedBy = x.CreatedBy;
                    detailhist.ModifiedBy = x.ModifiedBy;
                    detailhist.CreatedOn = x.CreatedOn;
                    detailhist.ModifiedOn = x.ModifiedOn;
                }
                //删除所有子产品
                DAL.DeleteRange<OrderDetails>(details, currentUserID);
                //添加子产品
                decimal tempOrderTotalAmount = 0;//订单总额
                foreach (var x in orderDetails)
                {
                    x.OrderNo = order.No;
                    DAL.AddOrModify(x, currentUserID);
                    var prod = new Bll.ProductService().GetProductById(x.ProductId);
                    x.Price = new Bll.ProductService().GetLastProductPrice(x.SubProductId, x.Price, prod.Type, true, customer.Id);
                    x.TotalPrice = x.Price * x.Num;
                    tempOrderTotalAmount += x.TotalPrice;
                    //下面这个是奇葩的东西
                    x.No = x.Id;
                    //更新SizeTitle
                    var product = context.Products.Where(o => o.Id == x.ProductId).FirstOrDefault();
                    if (product != null)
                    {
                        x.SizeTitle = product.Type == CommonRules.OtherProductTypeDicValue ? _commonService.GetDictionaryName("ProductUnitOther", x.Size) : _commonService.GetDictionaryName("ProductUnitCake", x.Size);
                    }
                }
                order.TotalPrice = tempOrderTotalAmount;
                order.NeedPay = order.TotalPrice - (order.CouponPay + order.GiftCardPay + order.IntegralPay);
                if (order.NeedPay < 0)
                {
                    order.NeedPay = 0;
                }
            }
            #endregion

            #region 变更发票信息
            string oid = order.Id;
            var tempinvoice = SingleOrDefault<Invoice>(a => a.OrderId.Equals(oid));
            if (invoice != null && tempinvoice == null)
            {
                invoice.OrderId = order.Id;
                DAL.AddOrModify(invoice, currentUserID);
            }
            if (tempinvoice != null)
            {
                if (invoice == null)
                    DAL.Delete(tempinvoice, currentUserID);
                else
                {
                    tempinvoice.InvoiceTitle = invoice.InvoiceTitle;
                    tempinvoice.InvoiceType = invoice.InvoiceType;
                    DAL.AddOrModify(tempinvoice, currentUserID);
                }
            }
            #endregion

            #region 审核状态
            if (order.TradeStatus == TradeStatus.NotPay && (order.FeeType == FeeType.ALiPay || order.FeeType == FeeType.TenPay || order.FeeType == FeeType.WXPay))
            {
                order.ReviewStatus = ReviewStatus.ReviewOnLineNoPay;
            }
            #endregion

            DAL.Commit();

            return new OpResult { Data = order, Successed = true };
        }

        /// <summary>
        /// 由购物车取出暂未添加的订单明细
        /// </summary>
        /// <param name="subProducts"></param>
        /// <param name="cartIDs"></param>
        /// <returns></returns>
        public IEnumerable<OrderDetails> GetOrderDetailsByCarts(IEnumerable<SubProduct> subProducts, List<string> cartIDs)
        {
            return from sub in subProducts
                   join cart in GetQuery<Cart>() on sub.Id equals cart.SubProductID
                   join product in GetQuery<Product>() on sub.ParentId equals product.Id
                   where product.SaleStatus == 1
                   && cartIDs.Contains(cart.Id)
                   select new OrderDetails
                   {
                       ProductId = sub.ParentId,
                       SubProductId = sub.Id,
                       Size = sub.Size,
                       BirthdayCard = cart.BirthdayCard,
                       Price = (decimal)(sub.Price == null ? 0 : sub.Price),
                       Num = cart.Num,
                       TotalPrice = (decimal)(sub.Price == null ? 0 : sub.Price) * cart.Num
                   };
        }
        /// <summary>
        /// 取出设置订单地址的委托
        /// </summary>
        /// <param name="getCustomerAddress"></param>
        /// <param name="getLogistic"></param>
        /// <param name="name"></param>
        /// <param name="mobile"></param>
        /// <returns></returns>
        public Action<Orders> SetOrderAddress(Expression<Func<CustomerAddress, bool>> getCustomerAddress,
            Expression<Func<LogisticsSite, bool>> getLogistic = null, string name = "", string mobile = "")
        {
            if (getCustomerAddress != null)
            {
                var address = SingleOrDefault<CustomerAddress>(getCustomerAddress);
                if (address == null)
                    return null;
                return a =>
                {
                    a.DeliveryType = address.DeliveryType;
                    a.LogisticsSiteId = address.LogisticsSiteId;
                    a.CustomerAddressId = address.Id;
                    a.Receiver = address.Receiver;
                    a.ReceiverAddr = address.Address;
                    a.ReceiverArea = address.Area;
                    a.ReceiverCity = address.City;
                    a.ReceiverProvince = address.Province;
                    a.ReceiverMobile = address.ReceiverMobile;
                    a.ReceiverTel = address.ReceiverTel;
                };
            }

            if (getLogistic != null)
            {
                var logistic = SingleOrDefault<LogisticsSite>(getLogistic);
                if (logistic == null)
                    return null;
                return a =>
                {
                    a.DeliveryType = (int)DeliveryType.FixedSite;
                    a.LogisticsSiteId = logistic.Id;
                    a.ReceiverProvince = logistic.SiteProvince;
                    a.ReceiverCity = logistic.SiteCity;
                    a.ReceiverArea = logistic.SiteArea;
                    a.ReceiverAddr = logistic.SiteAddress;
                    a.Receiver = name;
                    a.ReceiverMobile = mobile;
                };
            }

            return null;
        }
        #endregion

        public Orders GetByNo(string orderno)
        {
            return context.Orders.SingleOrDefault(a => a.No.Equals(orderno) && a.IsDeleted != 1);
        }

        public Orders GetByNo(string orderNo, string currentUserID)
        {
            var order = context.Orders.SingleOrDefault(a => a.IsDeleted != 1 && a.No.Equals(orderNo) && a.CustomerId.Equals(currentUserID));
            if (order == null)
                order = new Orders();
            return order;
        }

        public Orders GetByNo(string orderNo, bool includeCustomer = true)
        {
            return context.Orders.Include("Customers").SingleOrDefault(a => a.No.Equals(orderNo) && a.IsDeleted != 1);
        }

        public List<T> GetDetailByNo<T>(string orderNo) where T : new()
        {
            List<T> result = new List<T>();
            var outType = typeof(T);
            var outProInfos = outType.GetProperties();
            var outNames = outProInfos.Select(a => a.Name);

            var details = from x in context.OrderDetails
                          join y in context.Products on x.ProductId equals y.Id
                          join z in context.BaseFiles on y.MainImgId equals z.Id
                          where x.IsDeleted != 1
                          && y.IsDeleted != 1
                          && z.IsDeleted != 1
                          && x.OrderNo.Equals(orderNo)
                          select new
                          {
                              z.Url,
                              x.Size,
                              x.SizeTitle,
                              x.Price,
                              x.Num,
                              x.BirthdayCard,
                              PName = y.Name,
                              ProductType = y.Type
                          };

            foreach (var detail in details)
            {
                T outItem = new T();
                var type = detail.GetType();
                var proInfos = type.GetProperties();
                foreach (var x in proInfos.Where(p => outNames.Contains(p.Name)))
                {
                    var p = outProInfos.SingleOrDefault(a => a.Name.Equals(x.Name));
                    p.SetValue(outItem, x.GetValue(detail, null), null);
                }
                result.Add(outItem);
            }
            return result;
        }

        /// <summary>
        /// 分页查询订单 
        /// </summary>
        /// <returns></returns>
        public object GetQueryData(int page, int rows, string orderNo)
        {
            var result = context.Orders;
            if (orderNo.IsNullOrTrimEmpty() == false)
                return new
                {
                    total = result.Count(),
                    rows = result.Where(a => a.No.Equals(orderNo) && a.IsDeleted != 1).OrderBy(a => a.No).Skip((page - 1) * rows).Take(rows)
                };
            return new
                {
                    total = result.Count(),
                    rows = result.Where(a => a.IsDeleted != 1).OrderBy(a => a.No).Skip((page - 1) * rows).Take(rows)
                };
        }

        public object GetOrdersByCustomerId(int page, int rows, string customerId)
        {
            var orders = context.Orders.Where(a => a.IsDeleted != 1);
            if (customerId.IsNullOrTrimEmpty() == false)
            {
                orders = orders.Where(a => a.CustomerId.Equals(customerId));
            }
            return new
            {
                total = orders.Count(),
                rows = orders.OrderBy(a => a.No).Skip((page - 1) * rows).Take(rows)
            };
        }

        /// <summary>
        /// 获取订单及详细
        /// </summary>
        /// <returns></returns>
        public Orders GetOrderWithDetails(string orderNo, string customerId)
        {
            var orderInfo = context.Orders.Include("OrderDetails").Where(p => p.No == orderNo && p.IsDeleted != 1).FirstOrDefault();
            if (orderInfo == null)
                orderInfo = new Orders
                {
                    No = CommonRules.CommonNoRules("orderno"),
                    CustomerId = customerId
                };
            return orderInfo;
        }
        /// <summary>
        /// 生成订单号
        /// </summary>
        /// <returns></returns>
        public string CreateOrderNo()
        {
            string maxid = GetMaxOrderIdInToday();
            int maxorder;
            int.TryParse(maxid, out maxorder);
        roclback:
            maxorder = maxorder + 1;
            System.Text.StringBuilder currentOrderId = new System.Text.StringBuilder();
            string id = string.Format("{0:D5}", maxorder);
            currentOrderId.Append("FC").Append(DateTime.Now.ToString("yyMMdd")).Append(id);

            string no = currentOrderId.ToString();

            var results = (context.Orders.Where(p => p.No == no && p.IsDeleted != 1)).ToList();
            if (results.Count() > 0)
            {
                goto roclback;
            }

            return no;
        }

        /// <summary>
        /// 查询当日最大订单号
        /// </summary>
        /// <returns></returns>
        public string GetMaxOrderIdInToday()
        {
            string time1 = DateTime.Now.ToString("yyyy-MM-dd") + " 00:00:00";
            string time2 = DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59";
            DateTime beginDate = new DateTime();
            DateTime.TryParse(time1, out beginDate);
            DateTime endDate = new DateTime();
            DateTime.TryParse(time2, out endDate);
            var maxorderid = context.Orders.Where(p => p.IsDeleted != 1 && p.CreatedOn > beginDate && p.CreatedOn < endDate).OrderByDescending(p => p.CreatedOn).FirstOrDefault();
            if (maxorderid == null)
                return "0000";
            else
                return maxorderid.No.ToString().Remove(0, 8);
        }

        public List<Product> GetOrderProductByOrderNo(string orderNo)
        {
            var result = (from x in context.Orders
                          join y in context.OrderDetails on x.No equals y.OrderNo
                          join z in context.Products on y.ProductId equals z.Id
                          where x.No.Equals(orderNo, StringComparison.OrdinalIgnoreCase)
                          select z).ToList();
            return result;
        }

        public object UpdateOrderByOrderNo(Orders order, FormCollection c, string currentUserId)
        {
            var products = c["selectedproductids"].Split(',').Where(a => a.IsNullOrTrimEmpty() == false).ToList();
            //edit
            if (!order.Id.IsNullOrTrimEmpty())
            {
                if (order.No.IsNullOrTrimEmpty() == false)
                {
                    var ord = context.Orders.Single(p => p.IsDeleted != 1 && p.No == order.No);
                    ord.Receiver = order.Receiver;
                    ord.ReceiverArea = order.ReceiverArea;
                    ord.ReceiverAddr = order.ReceiverAddr;
                    // ord.DeliverTime = order.DeliverTime;
                    ord.DeliverMsg = order.DeliverMsg;
                    ord.TotalPrice = order.TotalPrice;
                    ord.ModifiedBy = currentUserId;
                    ord.ModifiedOn = DateTime.Now;
                    ord.IsDeleted = 0;
                    //context.SaveChanges();

                    string no = order.No;

                    //删除
                    var dels = context.OrderDetails.Where(a => a.OrderNo.Equals(no) && products.Contains(a.ProductId) == false);
                    foreach (var x in dels)
                    {
                        context.OrderDetails.Remove(x);
                    }
                    //添加
                    var temp = context.OrderDetails.Where(a => a.OrderNo.Equals(no)).Select(a => a.ProductId);
                    var adds = products.Where(a => temp.Contains(a) == false).ToList();
                    adds = context.Products.Where(a => adds.Contains(a.Id)).Select(a => a.Id).ToList();
                    foreach (var x in adds)
                    {
                        var id = FCake.Core.Common.DataHelper.GetSystemID();
                        context.OrderDetails.Add(new OrderDetails
                        {
                            Id = id,
                            No = id,
                            ProductId = x,
                            Price = Convert.ToDecimal(c["Price"]),
                            Num = Convert.ToInt32(c["Num"]),
                            TotalPrice = Convert.ToDecimal(c["TotalPrice"]),
                            OrderNo = order.No,
                            Size = c["Size"],
                            CreatedBy = currentUserId,
                            CreatedOn = DateTime.Now,
                            IsDeleted = 0,
                        });
                    }


                    context.SaveChanges();
                }

                return new { Successed = true, Message = "订单信息修改成功！" };
            }
            else
            {
                if (order.No.IsNullOrTrimEmpty())
                {
                    order.No = CommonRules.CommonNoRules("orderno");
                }
                Orders orderinfo = new Orders();
                orderinfo.Id = Guid.NewGuid().ToString("N").ToUpper();
                orderinfo.No = order.No;
                orderinfo.OrderSource = order.OrderSource;
                orderinfo.CreatedBy = currentUserId;
                orderinfo.CreatedOn = DateTime.Now;
                orderinfo.RequiredTime = DateTime.Now;
                orderinfo.FeeType = order.FeeType;
                orderinfo.Status = OrderStatus.HadPaid;
                orderinfo.CustomerId = order.CustomerId;
                orderinfo.Receiver = order.Receiver;
                orderinfo.ReceiverArea = order.ReceiverArea;
                orderinfo.ReceiverMobile = order.ReceiverMobile;
                orderinfo.ReceiverAddr = order.ReceiverAddr;
                //orderinfo.DeliverTime = order.DeliverTime;
                orderinfo.DeliverMsg = order.DeliverMsg;
                // orderinfo.DeliverStatus = order.DeliverStatus;
                orderinfo.TradeNo = order.TradeNo;
                orderinfo.ActualPay = order.ActualPay;
                orderinfo.TotalPrice = order.TotalPrice;
                orderinfo.IsDeleted = 0;
                context.Orders.Add(orderinfo);
                //context.SaveChanges();



                foreach (var x in products)
                {
                    var id = FCake.Core.Common.DataHelper.GetSystemID();
                    context.OrderDetails.Add(new OrderDetails
                    {
                        Id = id,
                        No = id,
                        ProductId = x,
                        Price = Convert.ToDecimal(c["Price"]),
                        Num = Convert.ToInt32(c["Num"]),
                        TotalPrice = Convert.ToDecimal(c["TotalPrice"]),
                        OrderNo = order.No,
                        Size = c["Size"],
                        CreatedBy = currentUserId,
                        CreatedOn = DateTime.Now,
                        IsDeleted = 0,
                    });
                }

                context.SaveChanges();
                return new { Successed = true, Message = "订单信息新增成功！" };
            }
        }

        public Dictionary<string, object> GetOrderDetailByOrderNo(string orderNo)
        {
            var order = context.Orders.SingleOrDefault(a => a.IsDeleted != 1 && a.No.Equals(orderNo));
            //var count=context.OrderDetails.Where(a=>a.IsDeleted!=1&&a.OrderNo.Equals(orderNo)).Sum(a=>a.Num);
            var invoice = context.Invoices.SingleOrDefault(a => a.OrderId.Equals(orderNo) && a.IsDeleted != 1);
            var logistic = order == null ? null : context.LogisticsSite.SingleOrDefault(a => a.Id.Equals(order.LogisticsSiteId) && a.IsDeleted != 1);
            Dictionary<string, object> dic = new Dictionary<string, object>();
            if (order == null)
            {
                //dic.Add("Count", "");
                dic.Add("Status", "");
                dic.Add("OrderSourceStr", "");
                dic.Add("Price", "");
                dic.Add("Invoice", "");
                dic.Add("InvoiceType", "");
                dic.Add("InvoiceTitle", "");
                dic.Add("DeliveryType", "");
                dic.Add("Address", "");
                dic.Add("Receiver", "");
                dic.Add("ReceiverConection", "");
                dic.Add("CreatedOn", "");
                dic.Add("RequiredTime", "");
                dic.Add("OrderSource", "");
                dic.Add("Candle", "");
                dic.Add("RequiredTimeBucket", "");
                dic.Add("CouponPay", "");
                dic.Add("IntegralPay", "");
                dic.Add("GiftCardPay", "");
                dic.Add("NeedPay", "");
                dic.Add("ActualPay", "");
                dic.Add("ReviewStatus", "");
                dic.Add("OrderType", "");
                dic.Add("DeliverMsg", "");
                dic.Add("Remark", "");
                dic.Add("ReviewUID", "");
            }
            else
            {
                //dic.Add("Count", count);
                dic.Add("Status", EnumHelper.GetDescription(order.Status));
                dic.Add("OrderSourceStr", EnumHelper.GetDescription(order.OrderSource));
                dic.Add("Price", order.TotalPrice.ToString("N2"));
                dic.Add("Invoice", invoice == null ? "不需要" : "需要");
                dic.Add("InvoiceType", invoice == null ? "" : invoice.InvoiceType == "person" ? "个人发票" : "公司发票");
                dic.Add("InvoiceTitle", invoice == null ? "" : invoice.InvoiceTitle);
                dic.Add("DeliveryType", order.DeliveryType == (int)DeliveryType.D2D ? "送货上门" : "站点自提");
                dic.Add("Address", logistic != null ? (logistic.SiteProvince + logistic.SiteCity + logistic.SiteArea + logistic.SiteAddress) : (order.ReceiverProvince + order.ReceiverCity + order.ReceiverArea + order.ReceiverAddr));
                dic.Add("Receiver", order.Receiver);
                dic.Add("ReceiverConection", order.ReceiverMobile + " " + order.ReceiverTel);
                dic.Add("CreatedOn", order.CreatedOn);
                dic.Add("RequiredTime", order.RequiredTime);
                dic.Add("OrderSource", order.OrderSource);
                dic.Add("Candle", EnumHelper.GetDescription(order.Candle));
                dic.Add("RequiredTimeBucket", order.RequiredTimeBucket);
                dic.Add("CouponPay", order.CouponPay.ToString("N2"));
                dic.Add("IntegralPay", order.IntegralPay.ToString("N2"));
                dic.Add("GiftCardPay", order.GiftCardPay.ToString("N2"));
                dic.Add("NeedPay", order.NeedPay.ToString("N2"));
                dic.Add("ActualPay", order.ActualPay.ToString("N2"));
                dic.Add("ReviewStatus", (int)order.ReviewStatus);
                dic.Add("OrderType", order.OrderType);
                dic.Add("DeliverMsg", order.DeliverMsg);
                dic.Add("Remark", order.Remark);
                dic.Add("ReviewUID", order.ReviewUID);
            }
            return dic;
        }
        /// <summary>
        /// 获取单个客户购物历史 by cloud
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public List<Orders> GetShoppingHistory(string customerId)
        {
            var history = context.Orders.Where(o => o.IsDeleted != 1 && o.CustomerId == customerId).OrderByDescending(o => o.CreatedOn).ToList();
            return history;
        }
        /// <summary>
        /// 获取订单详情 by cloud
        /// </summary>
        /// <param name="orderNo"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetOrderDetails(string orderNo)
        {
            var orderDetails = context.OrderDetails.Where(od => od.IsDeleted != 1 && od.OrderNo == orderNo);
            var temp = (from details in orderDetails
                        join product in context.Products on details.ProductId equals product.Id
                        select new
                        {
                            Name = product.Name,
                            Size = details.Size,
                            Price = details.Price,
                            Num = details.Num,
                            TotalPrice = details.TotalPrice
                        }).ToList();
            var dic = FCake.Core.Common.DataHelper.ToDic(temp);
            return dic;
        }

        /// <summary>
        /// 订单报表数据
        /// </summary>
        /// <param name="orderid">订单号</param>
        /// <param name="status">订单状态</param>
        /// <returns></returns>
        public dynamic GetOrders(string orderid, string keyWord, DateTime? beginTime, DateTime? endTime, DateTime? reqBeginTime, DateTime? reqEndTime, out int count, int? status, int? reviewStatus, int page = 1, int rows = 20)
        {

            var dquery = context.Orders.Include("Customers").Where(p => p.IsDeleted != 1);
            if (orderid != "" && orderid != null)
            {
                orderid = orderid.ToUpper();
                dquery = dquery.Where(p => p.No.Contains(orderid));
            }
            if (status != null)
            {
                dquery = dquery.Where(p => p.Status == (OrderStatus)status);
            }
            if (reviewStatus != null)
            {
                dquery = dquery.Where(p => p.ReviewStatus == (ReviewStatus)reviewStatus && p.Status != OrderStatus.Canceled);
            }
            if (reviewStatus == null)
            {//订单管理中不显示待审核订单
                dquery = dquery.Where(p => p.ReviewStatus != ReviewStatus.ReviewPending);
            }
            if (!string.IsNullOrEmpty(keyWord))
            {
                dquery = dquery.Where(p => p.Customers.FullName.Contains(keyWord) || p.Customers.Mobile.Contains(keyWord) || p.Customers.Tel.Contains(keyWord));
            }
            if (beginTime != null || endTime != null)
            {
                if (beginTime != null && endTime != null)
                {
                    dquery = dquery.Where(p => p.CreatedOn >= beginTime && p.CreatedOn <= endTime);
                }
                else if (beginTime != null)
                {
                    dquery = dquery.Where(p => p.CreatedOn >= beginTime);
                }
                else if (endTime != null)
                {
                    dquery = dquery.Where(p => p.CreatedOn <= endTime);
                }
            }
            if (reqBeginTime != null || reqEndTime != null)
            {
                if (reqBeginTime != null && reqEndTime != null)
                {
                    dquery = dquery.Where(p => p.RequiredTime >= reqBeginTime && p.RequiredTime <= reqEndTime);
                }
                else if (reqBeginTime != null)
                {
                    dquery = dquery.Where(p => p.RequiredTime >= reqBeginTime);
                }
                else if (reqEndTime != null)
                {
                    dquery = dquery.Where(p => p.RequiredTime <= reqEndTime);
                }
            }
            dquery = dquery.OrderBy(p => p.ReviewStatus).ThenByDescending(p => p.CreatedOn);
            count = dquery.Count();
            page = page < 1 ? 1 : page;
            var temp = from o in dquery.Skip((page - 1) * rows).Take(rows)
                       join c in context.Customers on o.CustomerId equals c.Id
                       select new
                       {
                           Id = o.Id,
                           No = o.No,
                           Customer = c.FullName,
                           CreatedOn = o.CreatedOn,
                           FreeType = o.FeeType,
                           state = o.Status,
                           //Receiver = o.Receiver,
                           //ReceiverMobile = o.ReceiverMobile,
                           ReceiverAddr = o.ReceiverAddr,
                           ReceiverArea = o.ReceiverArea,
                           Mobile = o.Customers.Mobile,
                           Tel = o.Customers.Tel,
                           Remark = o.Remark,
                           RequiredTime = o.RequiredTime,
                           TotalPrice = o.TotalPrice,
                           reviewstate = o.ReviewStatus,
                           DeliverMsg = o.DeliverMsg,
                           OrderClient = o.OrderClient,
                           TradeStatus = o.TradeStatus,
                           ActualPay = o.ActualPay,
                           CouponPay = o.CouponPay,
                           o.NeedPay,
                           o.GiftCardPay,
                           o.IntegralPay,
                           o.UsedIntegralVal,
                           RequiredTimeBucket = o.RequiredTimeBucket
                       };

            //result = context.Orders.Where(dquery.Compile()).Skip((page - 1) * rows).Take(rows).ToList();

            return temp;
        }

        public OpResult UpdateViewStateByNo(string orderNo, int? viewState, string remark, string orderType, string reviewUID, string deliverMsg)
        {
            OpResult result = new OpResult();
            result.Successed = false;
            try
            {
                var data = context.Orders.Where(p => p.No.Equals(orderNo)).First();
                if (data != null)
                {
                    if (data.ReviewStatus == ReviewStatus.ReviewPass)
                    {
                        result.Message = "该订单已经审批通过";
                        return result;
                    }
                    data.ReviewStatus = (ReviewStatus)viewState;
                    data.Remark = remark;
                    if (viewState == 2)
                    {
                        data.Status = OrderStatus.TradingClosed;//如果订单审核不通过的话订单状态变为关闭
                    }
                    data.OrderType = orderType;
                    data.ReviewUID = reviewUID;
                    data.DeliverMsg = deliverMsg;
                    if (context.SaveChanges() > 0)
                    {
                        result.Successed = true;
                        if ((ReviewStatus)viewState == ReviewStatus.ReviewReject)
                        {
                            result.Message = "操作成功！";
                        }
                        if ((ReviewStatus)viewState == ReviewStatus.ReviewPass)
                        {
                            result.Message = "操作成功！";
                        }
                        //TODO:预留短信发送区域(手机号码，内容)
                    }
                    return result;
                }
                return null;
            }
            catch (Exception e)
            {
                result.Message = "操作发生异常：" + e.Message;
                return result;
            }
        }

        /// <summary>
        /// 由客户Id获取其订单信息——前台<我的账户> by cloud
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public List<Orders> GetOrdersByCustomerId(string customerId, out int count, int pageSize = 10, int pageIndex = 1)
        {
            var query = context.Orders.Where(o => o.IsDeleted != 1 && o.CustomerId == customerId);
            List<Orders> result = null;
            count = query.Count();
            if (query.Count() != 0)
            {
                query = query.OrderByDescending(o => o.CreatedOn);
                result = query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            }
            else
            {
                result = query.ToList();
            }
            return result;
        }
        /// <summary>
        /// 取消订单
        /// </summary>
        /// <param name="orderNo"></param>
        /// <param name="requestFrom">请求来源（0：后台，1：PC，2：手机）</param>
        /// <returns></returns>
        public OpResult CancelOrderByNo(string orderNo, int requestFrom = 0)
        {

            OpResult result = new OpResult();
            result.Successed = false;
            try
            {
                //更改订单状态
                var data = context.Orders.SingleOrDefault(p => p.No.Equals(orderNo) && p.IsDeleted != 1);
                if (data != null)
                {
                    if (requestFrom != 0 && data.TradeStatus != TradeStatus.NotPay)
                    {
                        return OpResult.Fail("该订单已支付，无法取消");
                    }
                    if (data.Status != OrderStatus.Canceled)
                    {
                        if (data.Status == OrderStatus.NotPay || data.Status == OrderStatus.HadPaid)
                        {//等待付款或下单成功状态，允许退还优惠券、代金卡、积分
                            SendBackCouponDetail(data.No, false);
                            SendBackGiftCardDetail(data.No, false);
                            SendBackIntegral(data.No, false);
                        }

                        data.Status = OrderStatus.Canceled;
                        data.ReviewStatus = ReviewStatus.Canceled;

                    }
                }
            }
            catch
            {
                result.Message = "服务器异常，请稍后重试！";
            }
            if (context.SaveChanges() > 0)
                result.Successed = true;
            return result;
        }
        /// <summary>
        /// 短信发送
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="msgTemplateId"></param>
        /// <returns></returns>
        public OpResult SendMessage(string orderNo, string msgTemplateId)
        {
            OpResult result = new OpResult();
            result.Successed = false;
            var order = context.Orders.Where(p => p.No.Equals(orderNo) && p.IsDeleted != 1).SingleOrDefault();
            if (order != null)
            {
                var customer = context.Customers.Where(p => p.Id.Equals(order.CustomerId) && p.IsDeleted != 1).SingleOrDefault();
                if (customer != null)
                {
                    //var rt = EChiHelper.SendSMSResult(customer.Mobile, msgContent);
                    var sendSMSErrorMsg = string.Empty;
                    var rt = DaYuSMSHelper.SendNotifySMS(customer.Mobile, msgTemplateId, out sendSMSErrorMsg);
                    if (rt)
                    {
                        result.Successed = true;
                    }
                    else
                    {
                        result.Message = "发送失败";
                        SysLogService.SaveAliSMSErrorLog(sendSMSErrorMsg, customer.Mobile, msgTemplateId);
                    }
                }
                else
                {
                    result.Message = "查不到该客户";
                }
            }
            else
            {
                result.Message = "查不到该订单";
            }
            return result;
        }
        public int GetNucheckOrder()
        {
            var count = context.Orders.Where(p => p.ReviewStatus == ReviewStatus.ReviewPending && p.IsDeleted != 1).Count();
            return count;
        }
        /// <summary>
        /// 货到付款的订单完成时要修改支付金额
        /// </summary>
        /// <param name="orderNo">订单号</param>
        /// <param name="actualPay">收到的金额</param>
        /// <returns></returns>
        public OpResult UpdateActualPayByCompateOrder(string orderNo, decimal actualPay, decimal giftCardPay)
        {
            OpResult result = new OpResult();
            result.Successed = false;
            try
            {
                var order = context.Orders.SingleOrDefault(p => p.No.Equals(orderNo));
                var distribution = context.Distribution.Where(d => d.OrderNo == orderNo && d.IsDeleted != 1).FirstOrDefault();
                if (order != null)
                {
                    //是货到付款的才改
                    if (order.TradeStatus == TradeStatus.NotPay && (order.FeeType == FeeType.Cash || order.FeeType == FeeType.POS))
                    {
                        order.ActualPay = actualPay;
                        //更新代金卡支付数据
                        if (giftCardPay > 0)
                        {
                            order.GiftCardPay += giftCardPay;
                            order.NeedPay = order.TotalPrice - (order.CouponPay + order.GiftCardPay + order.IntegralPay);
                        }
                        order.Status = OrderStatus.Completed;
                        order.TradeStatus = TradeStatus.HadPaid;
                        distribution.Status = StatusDistribution.Distributed;
                        distribution.EndTime = DateTime.Now;

                    }
                    else
                    {
                        order.Status = OrderStatus.Completed;
                        distribution.Status = StatusDistribution.Distributed;
                        distribution.EndTime = DateTime.Now;
                    }
                }
                context.SaveChanges();

                result.Successed = true;
                if (order.Status == OrderStatus.Completed)
                {//订单完成后更新会员信息
                    CompletedOrderUpdateMemberInfo(order.No);
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }
            return result;
        }

        #region private
        /// <summary>
        /// 如果是货到付款，POS支付的，电话订单的，订单状态直接是下单成功
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        private OrderStatus CreateOrderStatusByFeeType(Orders order)
        {
            OrderStatus oStatus = order.Status;
            switch (order.FeeType)
            {
                case FeeType.Cash:
                case FeeType.POS:
                    oStatus = OrderStatus.HadPaid;
                    break;
                case FeeType.ALiPay:
                    break;
                case FeeType.TenPay:
                    break;
                case FeeType.OtherPay:
                    break;
                default:
                    break;
            }
            return oStatus;
        }
        #endregion

        #region 退款管理
        /// <summary>
        /// 查询所有退款记录
        /// </summary>
        /// <param name="orderNo">订单号</param>
        /// <param name="totalRecord">总记录数</param>
        /// <returns>退款记录集合</returns>
        public List<OrderRefundRecord> GetAllRefundOrder(string orderNo, out int totalRecord)
        {
            List<OrderRefundRecord> refundOrderList = new List<OrderRefundRecord>();
            if (orderNo == "" || orderNo == null)
            {
                refundOrderList = context.OrderRefundRecords.Where(p => p.IsDeleted != 1).ToList();
                totalRecord = refundOrderList.Count;
            }
            else
            {
                refundOrderList = context.OrderRefundRecords.Where(p => p.IsDeleted != 1 && p.OrderNo.Equals(orderNo)).ToList();
                totalRecord = refundOrderList.Count;
            }
            return refundOrderList;
        }
        /// <summary>
        /// 获取所有可以退款的订单
        /// </summary>
        /// <returns>订单集合</returns>
        public List<Orders> GetCanChangesOrder(string orderNo)
        {
            List<Orders> result = new List<Orders>();
            //订单状态=0 || =1 未生产的订单可以退
            if (orderNo != null && orderNo != "")
            {
                result = context.Orders.Where(p => p.No.Equals(orderNo) && p.TradeStatus == TradeStatus.HadPaid && (int)p.Status < 2 && p.IsDeleted != 1).ToList();
            }
            else
            {
                result = context.Orders.Where(p => (int)p.Status < 2 && p.TradeStatus == TradeStatus.HadPaid && p.IsDeleted != 1).ToList();
            }
            return result;
        }
        /// <summary>
        /// 添加订单退款记录
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns></returns>
        public OpResult AddRefundOrder(OrderRefundRecord entity)
        {
            OpResult result = new OpResult();
            result.Successed = false;
            try
            {
                if (entity.RefundAmount == 0 || entity.CustomerAccount == null || entity.CustomerAccount == "")
                {
                    result.Message = "信息填写不完整!";
                    return result;
                }
                if (entity.OrderNo != null && entity.OrderNo != "")
                {

                    var order = context.Orders.SingleOrDefault(p => p.No.Equals(entity.OrderNo));
                    if (order != null)
                    {
                        if ((int)order.Status >= 2 || (int)order.TradeStatus == 3)
                        {
                            result.Message = "订单状态已改变，确认是否是该订单";
                        }
                        else
                        {
                            order.Status = OrderStatus.TradingClosed;//更改订单状态
                            order.TradeStatus = TradeStatus.HadRefund;//更改支付状态
                            order.ReviewStatus = ReviewStatus.ReviewReject;//审核状态变为审核不通过
                            context.OrderRefundRecords.Add(entity);//新增退款记录
                            if (context.SaveChanges() > 0)
                            {
                                result.Successed = true;
                                result.Message = "订单状态已修改成退款，请确认已打款给买家!";
                            }
                        }
                    }
                    else
                    {
                        result.Message = "退款信息有误";
                    }
                }
                else
                {
                    result.Message = "退款信息有误";
                }
            }
            catch (Exception e)
            {
                result.Message = "退款时出错:" + e.Message;
            }
            return result;
        }

        #endregion

        #region 订单导出
        /// <summary>
        /// 已完成订单导出
        /// </summary>
        /// <param name="type"></param>
        /// <param name="begintime"></param>
        /// <param name="endtime"></param>
        /// <returns></returns>
        public OpResult exportOrders(int type, int isReview, DateTime begintime, DateTime endtime, string orderStauts)
        {
            OpResult result = new OpResult() { Successed = false };
            try
            {
                SqlParameter[] sqlparms = new SqlParameter[5];
                sqlparms[0] = new SqlParameter("@category", type);
                sqlparms[1] = new SqlParameter("@isReview", isReview);
                sqlparms[2] = new SqlParameter("@startTime", begintime);
                sqlparms[3] = new SqlParameter("@endTime", endtime);
                sqlparms[4] = new SqlParameter("@orderStauts", orderStauts);
                DataTable dt = context.Database.SqlQueryForDataTatable("EXEC dbo.proc_exportOrders @category,@isReview,@startTime,@endTime,@orderStauts", sqlparms);
                if (dt.Rows.Count > 0)
                {
                    //dt.Columns.Add("doUser");//增加处理人
                    dt.Columns.Add("isprint");//增加是否打印
                    string[] fields = new string[] { "No", "OrderSource", "reviewStatus", "OrderStatus", "Mobile", "FullName", "Receiver", "ReceiverMobile", "Name", "Size", "Num", "RequiredTimeBucket", "ReceiverAddr", "feeType", "ActualPay", "BirthdayCard", "Remark", "DeliverMsg", "doUser", "isprint" };
                    string[] names = new string[] { "订单号", "订单来源", "审核状态", "订单状态", "会员电话", "会员名", "收货人", "收货人电话", "蛋糕名称", "规格", "数量", "送货时间", "收货地址", "支付方式", "支付金额", "生日牌", "订单备注", "客户留言", "处理人", "是否打印" };
                    FCake.Core.Common.ExportExcel excelExport = new ExportExcel();
                    var fileName = DateTime.Now.ToString("yyyyMMddHHmmss");
                    int[] marger = { 0, 1, 2, 3, 4, 5, 6, 7, 11, 12, 13, 14, 16, 17, 18, 19 };
                    Dictionary<int, int> dic = new Dictionary<int, int>();
                    dic.Add(0, 16);
                    string fileUrl = excelExport.ToExcel(fileName, dt, fields, names, marger, null, dic);
                    return OpResult.Success("导出成功", null, fileUrl);
                }
                else
                {
                    result.Message = "无可导出数据!";
                }
            }
            catch (Exception e)
            {
                result.Message = "导出失败：" + e.Message;
            }
            return result;
        }
        #endregion

    }
}
