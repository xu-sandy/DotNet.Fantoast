using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FCake.Admin.Models;
using System.Data.Entity;
using FCake.Domain.Entities;
using FCake.Core.MvcCommon;
using System.Linq.Expressions;
using FCake.Bll;
using FCake.Domain.Enums;
using FCake.Core.Common;
using FCake.Admin.Helper;

namespace FCake.Admin.Controllers
{
    public class OrderController : BaseController
    {
        private OrderService osv = new OrderService();

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult OrderDetail()
        {
            return View();
        }
        public ActionResult NewOrder()
        {
            return View();
        }


        [CheckPermission(controlName = "Order", actionName = "AddOrder", permissionCode = "view")]
        [HttpPost]
        public ActionResult QueryData(int page = 1, int rows = 20, string no = null)
        {
            return Json(new OrderService().GetQueryData(page, rows, no));
        }
        /// <summary>
        /// 根据id加载客户信息
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public ActionResult OrderDetails(string customerId)
        {
            return View(new CustomersService().GetByIdOrNew(customerId));
        }

        public ViewResult OrderInfo(string orderid, string customerId)
        {
            return View(new OrderService().GetOrderWithDetails(orderid, customerId));
        }
        [HttpPost]
        public ActionResult GetOrderProductByOrderId(string orderid)
        {
            return Json(new OrderService().GetOrderProductByOrderNo(orderid));
        }
        /// <summary>
        /// 根据客户id加载客户历史订单
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        public ActionResult QueryOrderByCustomer(string customerId, int page = 1, int rows = 8)
        {
            return Json(new OrderService().GetOrdersByCustomerId(page, rows, customerId), JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 修改订单信息
        /// </summary>
        /// <param name="order">订单信息</param>
        /// <param name="orderdetails">订单详情</param>
        /// <returns></returns>
        public ActionResult UpdateOrderByOrderNo(Orders order, FormCollection c)
        {
            try
            {
                dynamic msg = new OrderService().UpdateOrderByOrderNo(order, c, UserCache.CurrentUser.Id);
                var result = new OpResult() { Successed = msg.Successed, Message = msg.Message };
                return Json(result);
            }
            catch (Exception e)
            {
                var result = new OpResult() { Successed = false, Message = "订单信息写入失败：" + e.Message };
                return Json(result);
            }
        }

        #region 待审核订单列表

        public ActionResult ReviewPendingOrder()
        {
            return View();
        }
        /// <summary>
        /// 待审核订单列表数据
        /// </summary>
        /// <param name="orderid"></param>
        /// <param name="Customer"></param>
        /// <param name="CustomerMobile"></param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="reqBeginTime"></param>
        /// <param name="reqEndTime"></param>
        /// <param name="status"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Order", actionName = "ReviewPendingOrder", permissionCode = "view")]
        public ActionResult GetReviewPendingOrderList(string orderid, string keyWord, DateTime? beginTime, DateTime? endTime, DateTime? reqBeginTime, DateTime? reqEndTime, int? status, int page, int rows)
        {
            orderid = HttpUtility.UrlDecode(orderid);
            keyWord = HttpUtility.UrlDecode(keyWord);

            int count = 0;
            OrderService os = new OrderService();
            dynamic data = os.GetOrders(orderid, keyWord, beginTime, endTime, reqBeginTime, reqEndTime, out count, status, (int)ReviewStatus.ReviewPending, page, rows);
            return Json(new { total = count, rows = data });
        }
        #endregion



        #region 电话添加订单
        /// <summary>
        /// 根据产品名称、主题、类型查找产品数据
        /// </summary>
        /// <param name="ProductName"></param>
        /// <param name="ProductThemes"></param>
        /// <param name="ProductType"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Order", actionName = "AddOrder", permissionCode = "view")]
        public ActionResult SearchProducts(string name, string themes, string type, int page = 1, int rows = 10)
        {
            var data = new ProductService().SearchProductByPhoneOrder(name, themes, type, page, rows);
            return Json(data);
        }
        /// <summary>
        /// 获取产品子类
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Order", actionName = "AddOrder", permissionCode = "view")]
        public ActionResult GetSubProducts(string id)
        {
            Product p = new ProductService().GetProduct(id, false);
            return View(p);
        }
        /// <summary>
        /// 根据产品ID获取子产品磅数及价格
        /// </summary>
        /// <param name="ProductID"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Order", actionName = "AddOrder", permissionCode = "view")]
        [HttpPost]
        public ActionResult GetSubProducts(string id, FormCollection c)
        {
            //返回数据有ID Price Size
            var result = new ProductService().GetSubProducts(id);
            return Json(result);
        }
        /// <summary>
        /// 根据产品ID获取产品信息
        /// </summary>
        /// <param name="ProductID"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Order", actionName = "AddOrder", permissionCode = "view")]
        [HttpPost]
        public ActionResult GetProductsInfo(string id)
        {
            //返回数据有ID Price Size
            var result = new ProductService().GetProductsInfo(id);
            return Json(result);
        }
        [CheckPermission(controlName = "Order", actionName = "AddOrder", permissionCode = "view")]
        public ActionResult SearchCustomer()
        {
            return View();
        }
        /// <summary>
        /// 查找客户
        /// </summary>
        /// <param name="name"></param>
        /// <param name="tel"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Order", actionName = "AddOrder", permissionCode = "view")]
        [HttpPost]
        public ActionResult SearchCustomer(string name, string tel, int page = 1, int rows = 10)
        {
            var data = new CustomersService().SearchCustomersByPhoneOrder(name, tel, page, rows);
            return Json(data);
        }

        [CheckPermission(controlName = "Order", actionName = "AddOrder", permissionCode = "view")]
        [HttpPost]
        public ActionResult GetCustomerByID(string id)
        {
            return Json(new CustomersService().GetCustomerByID(id));
        }
        /// <summary>
        /// 根据客户ID查询使用过的地址
        /// </summary>
        /// <param name="customerid"></param>
        /// <param name="isdef"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Order", actionName = "AddOrder", permissionCode = "view")]
        public ActionResult GetCustomerAddresById(string id, bool isdef = false, string orderid = "")
        {
            var order = new OrderService().GetByNo(orderid);
            string addressid = "";
            if (order != null)
            {
                if (order.DeliveryType == (int)DeliveryType.D2D)
                {
                    addressid = order.CustomerAddressId;
                }
                if (order.DeliveryType == (int)DeliveryType.FixedSite)
                {
                    addressid = order.LogisticsSiteId;
                }
            }
            ViewBag.addressid = addressid;
            ViewBag.AllLogisticsSites = new FCake.Bll.LogisticsSiteService().GetAllLogisticsSites();

            return View(new CustomerAddressService().GetCustomerAddressesById(id, 0));
        }
        /// <summary>
        /// 用户新增地址保存
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Order", actionName = "AddOrder", permissionCode = "edit")]
        [HttpPost]
        public ActionResult SaveCustomerAddress(FormCollection c)
        {
            var result = new CustomerAddressService().SaveCustomerAddress(c, UserCache.CurrentUser.Id);

            return Json(new { state = result, message = result != null ? "成功" : "失败" });
        }
        /// <summary>
        /// 由订单取得产品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Order", actionName = "AddOrder", permissionCode = "view")]
        [HttpPost]
        public ActionResult GetOrderSubProducts(string id, int getdata = 0)
        {
            var result = new ProductService().GetOrderSubProducts(id);

            if (getdata == 1)
                return Json(result);

            var order = osv.SingleOrDefault<Orders>(a => a.No.Equals(id, StringComparison.OrdinalIgnoreCase));
            var detail = osv.GetQuery<OrderDetails>(a => a.OrderNo.Equals(id, StringComparison.OrdinalIgnoreCase));

            //todo:更改订单状态 ？？？
            return Json(new
            {
                result = result,
                //validate = (order == null || (order.OrderSource == OrderSource.TelOrder && order.Status == OrderStatus.NotPay)),
                validate = (order == null || (order.Status == OrderStatus.HadPaid && order.TradeStatus == TradeStatus.NotPay)),
                candle = (order == null || order.Candle == YesOrNo.Yes) ? 0 : 1,
                birthdaycard = detail.Any() == false ? "" : detail.Max(a => a.BirthdayCard),
                delivermsg = order == null ? "" : order.DeliverMsg,
                remark = order == null ? "" : order.Remark
            });
        }

        [CheckPermission(controlName = "Order", actionName = "AddOrder", permissionCode = "edit")]
        public ActionResult CreateCustomer()
        {
            return View();
        }
        /// <summary>
        /// 根据订单添加客户
        /// </summary>
        /// <param name="fullName">名称</param>
        /// <param name="mobile"></param>
        /// <param name="tel"></param>
        /// <returns></returns>
        [HttpPost]
        [CheckPermission(controlName = "Order", actionName = "AddOrder", permissionCode = "edit")]
        [ValidateInput(false)]
        public ActionResult CreateCustomerByPhone(string fullName, string mobile = "", string tel = "")
        {
            return Json(new CustomersService().CreateCustomerByPhone(fullName, mobile, tel));
        }

        [CheckPermission(controlName = "Order", actionName = "AddOrder", permissionCode = "view")]
        public ActionResult EditAddress(string id = "")
        {
            CustomerAddress address = new CustomerAddressService().GetCustomerDefAddress(id);
            if (address == null)
                address = new CustomerAddress { City = "厦门市", IsDef = 1 };
            return View(address);
        }



        [CheckPermission(controlName = "Order", actionName = "Index", permissionCode = "view")]
        public ActionResult GetLogisticsSitesByCity(string city = "", string id = "")
        {
            return View();
        }



        /// <summary>
        /// 获取单个客户购物历史 by cloud
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Order", actionName = "Index", permissionCode = "view")]
        public ActionResult GetShoppingHistory(string customerId)
        {
            return Json(new OrderService().GetShoppingHistory(customerId));
        }

        /// <summary>
        /// 获取订单详情 by cloud
        /// </summary>
        /// <param name="orderNo"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Order", actionName = "Index", permissionCode = "view")]
        public ActionResult GetOrderDetails(string orderNo)
        {
            return Json(new OrderService().GetOrderDetails(orderNo));
        }

        /// <summary>
        /// 查看订单详情页 by cloud
        /// </summary>
        /// <param name="orderid"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Order", actionName = "Index", permissionCode = "view")]
        public ViewResult _PartialOrderDetail(string orderid)
        {
            ViewBag.orderid = orderid;
            return View();
        }

        #endregion

        /// <summary>
        /// 获取订单列表数据 by michael
        /// </summary>
        /// <param name="orderid"></param>
        /// <param name="status"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Order", actionName = "Index", permissionCode = "view")]
        public ActionResult GetOrders(string orderid, string keyWord, DateTime? beginTime, DateTime? endTime, DateTime? reqBeginTime, DateTime? reqEndTime, int? status, int? reviewStatus, int page, int rows)
        {
            orderid = HttpUtility.UrlDecode(orderid);
            keyWord = HttpUtility.UrlDecode(keyWord);

            int count = 0;
            OrderService os = new OrderService();
            dynamic data = os.GetOrders(orderid, keyWord, beginTime, endTime, reqBeginTime, reqEndTime, out count, status, reviewStatus, page, rows);
            return Json(new { total = count, rows = data });
        }

        /// <summary>
        /// 获取未审核订单，给予提示用的
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [CheckPermission(controlName = "Order", actionName = "Index", permissionCode = "view")]
        public int GetNucheckOrder()
        {
            if (IsCustomerServiceRole())
                return new OrderService().GetNucheckOrder();
            else
                return 0;
        }

        /// <summary>
        /// 订单审核详情对话框
        /// </summary>
        /// <returns></returns>
        [CheckPermission(controlName = "Order", actionName = "Index", permissionCode = "view")]
        public ActionResult OrderDetailCheck(string orderNo)
        {
            ViewBag.orderid = orderNo;
            return View();
        }

        /// <summary>
        /// 订单审核操作
        /// </summary>
        /// <param name="orderNo"></param>
        /// <param name="viewState"></param>
        /// <param name="msgContent"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Order", actionName = "Index", permissionCode = "edit")]
        public ActionResult CheckDone(string orderNo, int? viewState, string msgTemplateId, bool isSend, string remark, string orderType, string reviewUID, string deliverMsg)
        {
            OpResult result = new OpResult();
            OrderService os = new OrderService();
            var r1 = os.UpdateViewStateByNo(orderNo, viewState, remark, orderType, reviewUID, deliverMsg);

            if (r1.Successed == true && isSend == true)
            {
                var r2 = os.SendMessage(orderNo, msgTemplateId);
                if (r2.Successed == false)
                {
                    result.Successed = false;
                    result.Message = r2.Message;
                }
                else
                {
                    result.Successed = true;
                    result.Message = "";
                }
            }
            else if (r1.Successed == true && isSend == false)
            {
                result.Successed = true;
                result.Message = "操作成功";
            }
            else
            {
                result.Successed = false;
                result.Message = r1.Message;
            }
            return Json(result);
        }
        /// <summary>
        /// 取消订单，主要用于退回钱包支付金额
        /// </summary>
        /// <param name="orderNo"></param>
        /// <returns></returns>
        public ActionResult CancelOrder(string orderNo)
        {
            OrderService os = new OrderService();
            var result = os.CancelOrderByNo(orderNo);
            return Json(result);
        }

        #region 打印签收单
        [CheckPermission(controlName = "Order", actionName = "Index", permissionCode = "view")]
        public ActionResult AcknowledgedReceipt(string orderNo)
        {
            var order = osv.SingleOrDefault<Orders>(a => a.No.Equals(orderNo, StringComparison.OrdinalIgnoreCase), "Customers");
            //var order = osv.GetByNo(orderNo, true);
            ViewBag.SubList = new ProductService().GetOrderSubProducts(orderNo);
            return View(order);
        }

        #endregion



        #region redo

        #region 订单编辑
        /// <summary>
        /// 编辑电话订单(增加或编辑已有的)
        /// </summary>
        /// <param name="orderid">订单号(NO)</param>
        /// <param name="customerid">客户ID</param>
        /// <returns></returns>
        [CheckPermission(controlName = "Order", actionName = "AddOrder", permissionCode = "add")]
        public ActionResult AddOrder(string orderid = "", string customerid = "")
        {
            orderid = orderid.Trim();
            customerid = customerid.Trim();
            bool invoice = false;
            bool isperson = true;

            if (customerid.IsNullOrTrimEmpty() == false)
            {
                var customer = osv.SingleOrDefault<Customers>(a => a.Id.Equals(customerid, StringComparison.OrdinalIgnoreCase));
                //var customer = new CustomersService().GetById(customerid);
                if (customer == null)
                    return Redirect("/order/addorder");
            }
            var times = new FCake.Bll.CommonService().GetDictionaryByCode("DistributionTime");

            ViewBag.invoicetype = "";
            if (orderid.IsNullOrTrimEmpty() == false)
            {
                var order = osv.SingleOrDefault<Orders>(a => a.No.Equals(orderid));
                //var order = new OrderService().GetByNo(orderid);
                if (order == null)
                    return Redirect("/order/addorder");
                customerid = order.CustomerId;
                var delivertime = order.RequiredTime == null ? "" : ((DateTime)order.RequiredTime).ToString("yyyy-MM-dd");
                var deliverHour = order.RequiredTime == null ? "" : ((DateTime)order.RequiredTime).ToString("yyyy-MM-dd HH:mm:ss").Split(' ')[1].Substring(0, 5);

                //匹配新旧时间下拉数据信息
                foreach (var item in times.OrderBy(t => t.Sorting))
                {
                    var itemVal = item.Value.Split(':')[0].ToInt32();
                    var deliverVal = deliverHour.Split(':')[0].ToInt32();
                    if (itemVal >= deliverVal)
                    {
                        deliverHour = item.Value;
                        break;
                    }
                }
                ViewBag.delivertime = delivertime;
                ViewBag.deliverHour = deliverHour;
                ViewBag.status = order.Status;
                ViewBag.source = order.OrderSource;
                ViewBag.productenableedit = order.TradeStatus == TradeStatus.NotPay && order.Status == OrderStatus.HadPaid;
                ViewBag.revierName = order.Receiver;
                ViewBag.revierPhone = order.ReceiverMobile ?? order.ReceiverTel;
                //发票中的ORDERID存的是ID  不是NO！！
                var oid = order.Id;
                var invoices = osv.SingleOrDefault<Invoice>(a => a.OrderId.Equals(oid));
                //var invoices = new InvoiceService().GetByOrderId(orderid);     //context.Invoices.SingleOrDefault(a => a.IsDeleted == 0 && a.OrderId.Equals(orderid));
                invoice = (invoices != null);
                if (invoice)
                {
                    ViewBag.invoicetype = invoices.InvoiceType;
                    ViewBag.invoicetitle = invoices.InvoiceTitle;
                }
            }
            ViewBag.times = times;
            ViewBag.isperson = isperson;
            ViewBag.orderid = orderid;
            ViewBag.customerid = customerid;

            //取用户积分
            if (customerid != "")
            {
                var customerInfo = new CustomersService().GetMemberByMemberId(customerid);
                ViewBag.Integral = customerInfo.Integral;
            }
            else
            {
                ViewBag.Integral = 0;
            }
            //取积分兑换比例
            ViewBag.integralDeductionCashRate = CommonRules.IntegralDeductionCashRate;


            return View();
        }
        /// <summary>
        /// 保存编辑订单
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        [CheckPermission(controlName = "Order", actionName = "AddOrder", permissionCode = "edit")]
        public ActionResult SaveOrder(PhoneOrder phoneOrder)
        {
            List<string> giftIds = null;
            List<string> couponIds = null;
            //不能为null 为null会报错
            if (phoneOrder.GiftCardDetailIds.IsNotNullOrEmpty())
            {
                giftIds = phoneOrder.GiftCardDetailIds.Split(',').ToList();
            }
            if (phoneOrder.CouponDetailIds.IsNotNullOrEmpty())
            {
                couponIds = phoneOrder.CouponDetailIds.Split(',').ToList();
            }
            //合并相同产品
            phoneOrder.Products = phoneOrder.Products.GroupBy(a => a.ID)
                .Select(a => new CollectionIDNum { ID = a.Key, Num = a.Sum(b => b.Num), BirthdayCard = a.Max(b => b.BirthdayCard ?? "") }).ToList();

            OpResult result = null;
            Invoice invoice = null;
            if (phoneOrder.InvoiceType != null)
            {
                invoice = new Invoice { InvoiceType = phoneOrder.InvoiceType.ToString(), InvoiceTitle = phoneOrder.InvoiceTitle };
            }
            //地址
            Action<Orders> setAddress;
            if (phoneOrder.AddressType == 0)
            {
                setAddress = osv.SetOrderAddress(a => a.Id.Equals(phoneOrder.AddressId, StringComparison.OrdinalIgnoreCase)
                    && a.CustomerId.Equals(phoneOrder.CustomerId, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                setAddress = osv.SetOrderAddress(null, a => a.Id.Equals(phoneOrder.AddressId, StringComparison.OrdinalIgnoreCase), phoneOrder.RevierName, phoneOrder.RevierPhone);
            }
            //优惠金额处理
            if (phoneOrder.CouponPay > 0 || phoneOrder.GiftCardPay > 0)
            {
                var integralDeductionCashRate = CommonRules.IntegralDeductionCashRate;//积分抵扣比例
                if (phoneOrder.CouponPay > 0)
                {//优惠券支付
                    if (phoneOrder.OrderTotalAmount == phoneOrder.CouponPay)
                    {
                        phoneOrder.IntegralPay = 0;//不需要积分
                    }
                    else if (phoneOrder.OrderTotalAmount < phoneOrder.CouponPay)
                    {//优惠券总额>订单金额
                        phoneOrder.CouponPay = phoneOrder.OrderTotalAmount;
                        phoneOrder.IntegralPay = 0;//不需要积分
                    }
                    else if (phoneOrder.OrderTotalAmount < (phoneOrder.IntegralPay + phoneOrder.CouponPay))
                    {//积分+优惠券》订单总额
                        phoneOrder.IntegralPay = phoneOrder.OrderTotalAmount - phoneOrder.CouponPay;
                    }
                }
                else
                { //代金卡支付
                    if (phoneOrder.OrderTotalAmount < phoneOrder.GiftCardPay) 
                    {
                        phoneOrder.IntegralPay = 0;//不需要积分
                    }
                    else if (phoneOrder.OrderTotalAmount < phoneOrder.GiftCardPay)
                    {//优惠券总额>订单金额
                        phoneOrder.GiftCardPay = phoneOrder.OrderTotalAmount;
                        phoneOrder.IntegralPay = 0;//不需要积分
                    }
                    else if (phoneOrder.OrderTotalAmount < (phoneOrder.IntegralPay + phoneOrder.GiftCardPay))
                    {//积分+优惠券》订单总额
                        phoneOrder.IntegralPay = (phoneOrder.OrderTotalAmount - phoneOrder.GiftCardPay) * integralDeductionCashRate;
                    }
                }
            }
            //积分》订单总额
            if (phoneOrder.IntegralPay > phoneOrder.OrderTotalAmount)
            {
                phoneOrder.IntegralPay = phoneOrder.OrderTotalAmount;
            }

            //添加订单
            #region 添加订单
            if (phoneOrder.NO.IsNullOrTrimEmpty())
            {


                //创建订单
                result = osv.CreateOrder(
                    //客户信息
                    a => a.Id.Equals(phoneOrder.CustomerId, StringComparison.OrdinalIgnoreCase),
                    //客户地址信息
                    setAddress,
                    //取出订单明细
                    a =>
                    {
                        var pids = phoneOrder.Products.Select(b => b.ID);
                        return a.Where(p => pids.Contains(p.Id)).ToList()
                            .Select(b => new OrderDetails
                            {
                                ProductId = b.ParentId,
                                Size = b.Size,
                                BirthdayCard = phoneOrder.Products.Where(c => c.ID.Equals(b.Id)).Max(c => c.BirthdayCard ?? ""),
                                TotalPrice = (decimal)(b.Price == null ? 0 : b.Price) * phoneOrder.Products.Where(c => c.ID.Equals(b.Id)).Sum(c => c.Num),
                                Price = (decimal)(b.Price == null ? 0 : b.Price),
                                SubProductId = b.Id,
                                Num = phoneOrder.Products.Where(c => c.ID.Equals(b.Id)).Sum(c => c.Num)
                            });
                    },
                    //发票
                    invoice,
                    //订单赋值
                    a =>
                    {
                        a.FeeType = FeeType.Cash;   //货到付现金
                        a.OrderSource = OrderSource.TelOrder;   //电话订单
                        a.Candle = phoneOrder.Candle;   //蜡烛
                        a.DeliverMsg = phoneOrder.DeliverMsg;   //备注
                        a.Remark = phoneOrder.Remark;//订单备注
                        a.RequiredTime = phoneOrder.ReceiveTime;    //接收时间
                        a.RequiredTimeBucket = phoneOrder.TimeBucket;//收货时间段
                        a.CouponPay = phoneOrder.CouponPay;//优惠券支付
                        a.GiftCardPay = phoneOrder.GiftCardPay;//代金卡支付
                        a.IntegralPay = phoneOrder.IntegralPay;//积分支付
                        a.UsedIntegralVal = phoneOrder.UsedIntegralVal;//使用积分
                    },
                    //当前用户ID
                    Helper.UserCache.CurrentUser.Id, 2, phoneOrder.TimeBucket, couponIds, giftIds, phoneOrder.OrderTotalAmount
                    );
            }
            #endregion
            //修改订单
            #region
            else
            {
                //修改订单
                result = osv.EditOrder(
                    //客户地址信息
                    setAddress,
                    //取出订单明细
                    a =>
                    {
                        var pids = phoneOrder.Products.Select(b => b.ID).ToList();
                        var ps = a.Where(p => pids.Contains(p.Id)).ToList();

                        return ps
                            .Select(b => new OrderDetails
                            {
                                ProductId = b.ParentId,
                                Size = b.Size,
                                BirthdayCard = phoneOrder.Products.Where(c => c.ID.Equals(b.Id)).Max(c => c.BirthdayCard ?? ""),
                                TotalPrice = (decimal)(b.Price == null ? 0 : b.Price) * phoneOrder.Products.Where(c => c.ID.Equals(b.Id)).Sum(c => c.Num),
                                Price = (decimal)(b.Price == null ? 0 : b.Price),
                                SubProductId = b.Id,
                                Num = phoneOrder.Products.Where(c => c.ID.Equals(b.Id)).Sum(c => c.Num)
                            });
                    },
                    //发票
                    invoice,
                    //订单赋值
                    a =>
                    {
                        a.No = phoneOrder.NO;   //订单号
                        a.Candle = phoneOrder.Candle;   //蜡烛
                        a.DeliverMsg = phoneOrder.DeliverMsg;   //备注
                        a.Remark = phoneOrder.Remark;//客服备注
                        a.RequiredTime = phoneOrder.ReceiveTime;    //接收时间
                        a.RequiredTimeBucket = phoneOrder.TimeBucket;//收货时间段
                        //a.CouponPay = phoneOrder.CouponPay;//优惠券支付
                        //a.GiftCardPay = phoneOrder.GiftCardPay;//代金卡支付
                        //a.IntegralPay = phoneOrder.IntegralPay;//积分支付
                        a.UsedIntegralVal = phoneOrder.UsedIntegralVal;//使用积分
                    },
                    //当前用户ID
                    Helper.UserCache.CurrentUser.Id
                );
            }
            #endregion

            return Json(new
            {
                validate = result.Successed,
                order = result.Data,
                message = result.Message
            });
        }

        #region old
        ///// <summary>
        ///// 保存编辑订单
        ///// </summary>
        ///// <param name="c"></param>
        ///// <returns></returns>
        //[HttpPost]
        //[ValidateInput(false)]
        //[CheckPermission(controlName = "Order", actionName = "AddOrder", permissionCode = "edit")]
        //public ActionResult SaveOrder(FormCollection c)
        //{
        //    var dic = c.ToDic();
        //    var order = new OrderService().SaveOrderByPhone(dic, Helper.UserCache.CurrentUser.Id);

        //    return Json(new
        //    {
        //        validate = order != null,
        //        order = order
        //    });
        //}
        #endregion

        #endregion

        #endregion

        #region 退款管理
        [CheckPermission(controlName = "Order", actionName = "OrderRefundRecords", permissionCode = "view")]
        public ActionResult OrderRefundRecords()
        {
            return View();
        }
        /// <summary>
        /// 查询所有退款记录
        /// </summary>
        /// <param name="orderNo"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Order", actionName = "OrderRefundRecords", permissionCode = "view")]
        public ActionResult FindPageListForOrderRefundRecords(string orderNo)
        {
            int totalRecord = 0;
            var result = osv.GetAllRefundOrder(orderNo, out totalRecord);
            return Json(new { total = totalRecord, rows = result });
        }
        [CheckPermission(controlName = "Order", actionName = "OrderRefundRecords", permissionCode = "view")]
        public ActionResult GetRefundOrder()
        {
            return View();
        }
        /// <summary>
        /// 加载可以退款的所有订单
        /// </summary>
        /// <param name="orderNo"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Order", actionName = "OrderRefundRecords", permissionCode = "view")]
        [HttpPost]
        public ActionResult GetRefundOrderForm(string orderNo)
        {
            var result = osv.GetCanChangesOrder(orderNo);
            return Json(result);
        }
        /// <summary>
        /// 退款信息预加载
        /// </summary>
        /// <param name="orderNo"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Order", actionName = "OrderRefundRecords", permissionCode = "edit")]
        public ActionResult RefundInfo(string orderNo)
        {
            FCake.Domain.Entities.Orders order = new Orders();
            order = osv.GetByNo(orderNo);
            RefundOrderVm vmodel = new RefundOrderVm();
            vmodel.OrderNo = order.No;
            vmodel.RefundAmount = order.ActualPay;
            return View(vmodel);
        }
        /// <summary>
        /// 退款信息更改
        /// </summary>
        /// <param name="orr"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Order", actionName = "OrderRefundRecords", permissionCode = "edit")]
        [HttpPost]
        public ActionResult RefundInfo(OrderRefundRecord orr)
        {
            orr.Id = DataHelper.GetSystemID();
            orr.CreatedOn = DateTime.Now;
            orr.CreatedBy = UserCache.CurrentUser.Id;
            orr.OperUserId = UserCache.CurrentUser.FullName;
            orr.IsDeleted = 0;
            var result = osv.AddRefundOrder(orr);
            return Json(result);
        }
        #endregion


        [CheckPermission(controlName = "Order", actionName = "Index", permissionCode = "view")]
        [HttpPost]
        public ActionResult GetBirthdayCardSelect()
        {
            var card = new CommonService().GetDictionaryByCode("BirthdayCard").Select(a => new { v = a.Value, t = a.Text });
            return Json(card);
        }
        /// <summary>
        /// 订单数据导出
        /// </summary>
        /// <param name="type">1=下单时间 2=送货时间</param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Order", actionName = "Index", permissionCode = "view")]
        public ActionResult exportOrders(int type, int isReview, DateTime beginTime, DateTime endTime, string orderStauts)
        {
            beginTime = DateTime.Parse(beginTime.ToString("yyyy-MM-dd"));
            endTime = DateTime.Parse(endTime.ToString("yyyy-MM-dd 23:59:59"));
            if (string.IsNullOrEmpty(orderStauts))
                orderStauts = "";
            if (type == 1 || type == 2)
            {
                if (beginTime != new DateTime() && endTime != new DateTime())
                {
                    var result = new OrderService().exportOrders(type, isReview, beginTime, endTime, orderStauts);
                    return Json(result);
                }
            }
            return View();
        }
    }
}
