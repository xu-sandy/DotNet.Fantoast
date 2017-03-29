using FCake.Bll.Services;
using FCake.Domain.Entities;
using FCake.WebMobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FCake.Core.Common;
using FCake.Bll;
using FCake.Domain.Enums;
using System.Configuration;
using FCake.API.AliWapPay;
using System.Collections.Specialized;
using System.Text;
using System.Xml;
using FCake.Core.MvcCommon;
using FCake.Domain.WebModels;

namespace FCake.WebMobile.Controllers
{
    public class CartController : Controller
    {
        private OrderService osv = new OrderService();
        private PayService psv = new PayService();  //支付业务
        private readonly CartService _cartSvc = new CartService();
        private readonly ProductService _productService = new ProductService();
        private readonly CustomersService _customersService = new CustomersService();

        #region 购物车处理
        /// <summary>
        /// 购物车页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            ViewBag.shortcutSubProductList = _productService.GetShortcutSubProduct();
            return View();
        }
        /// <summary>
        /// 购物车选完后提交到订单处理
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Index(FormCollection c)
        {
            ViewBag.shortcutSubProductList = _productService.GetShortcutSubProduct();
            if (c["itemIDs"].IsNullOrTrimEmpty())
                return View();

            var itemIDs = c["itemIDs"].Split(',').ToList();
            var checkedIds = new CartService().CheckedCarts(itemIDs).Select(a => a.CartID);
            if (checkedIds.Any() == false)
                return View();

            CookieHelper.SetCookie("cartids", string.Join(",", checkedIds));
            return Redirect("/cart/Settlement");
        }
        /// <summary>
        /// 清空购物车
        /// </summary>
        /// <returns></returns>
        public ActionResult ClearCart()
        {
            new CartService().RemoveCart();
            return Redirect("/cart");
        }
        /// <summary>
        /// 添加产品到购物车
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddCart(string id, int num)
        {
            new CartService().AddCart(id, num);
            return Json(new { validate = true, msg = "添加成功" });
        }
        /// <summary>
        /// 订单明细中添加餐具等进购物车，将添加后的CartId放置cookie中，以保证添加后在订单明细中实时显示
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SettlementAddOtherToCart(string id, int num)
        {
            string cartId = new CartService().AddCart(id, num);
            var cartNum = GetCartNum();
            var cartIds = CookieHelper.GetCookieValue("cartids").Split(',').ToList();
            if (!string.IsNullOrEmpty(cartId))
            {
                cartIds.Add(cartId);
                CookieHelper.SetCookie("cartids", string.Join(",", cartIds));
            }

            return Json(new { validate = true, msg = "添加成功", cartnum = cartNum });
        }
        private int GetCartNum()
        {
            return this._cartSvc.GetCartsCount();
        }
        /// <summary>
        /// 从购物车中移除产品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult RemoveCart(string id)
        {
            new CartService().RemoveCart(id);
            return Json(new { validate = true, msg = "删除成功" });
        }
        /// <summary>
        /// 变更购物车项数量
        /// </summary>
        /// <param name="id"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ChangeCartItemNum(string id, int num)
        {
            bool result = new CartService().ChangeCartNum(id, num);
            return Json(new { validate = result, num = num });
        }
        /// <summary>
        /// 变更生日卡牌名称
        /// </summary>
        /// <param name="id"></param>
        /// <param name="card"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveBirthdayCard(string id, string card)
        {
            bool result = new CartService().ChangeCartBirthdayCard(id, card);
            return Json("");
        }
        /// <summary>
        /// 立即购买
        /// </summary>
        /// <param name="id"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public ActionResult BuyNow(string id, int num = 1)
        {
            var cs = new CartService();
            cs.AddCart(id, num);

            CookieHelper.SetCookie("cartids", string.Join(",", cs.GetCartIDsByLinkID()));
            return Redirect("/cart/Index");//立即购买跳转至购物车
            //return Redirect("/cart/Settlement");
        }
        #endregion

        #region 生成订单
        /// <summary>
        /// 购物车下单页面
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public ActionResult Settlement()
        {
            var cartIds = CookieHelper.GetCookieValue("cartids").Split(',').ToList();
            var result = new CartService().CheckedCarts(cartIds);
            var member = _customersService.GetMemberByMemberId(CurrentMember.MemberId);
            if (member == null)
                member = new Customers();

            if (result.Any() == false)
                return Redirect("/cart");
            var curMaxInadvanceHours = result.Max(o => o.InadvanceHours);
            ViewBag.earlyAllowDistributionTime = new SettlementService().GetEarlyAllowDistributionTime(curMaxInadvanceHours);
            ViewBag.Sites = new LogisticsSiteService().GetLogisticsSitesByProvince("福建省");
            ViewBag.shortcutSubProductList = _productService.GetShortcutSubProduct();
            ViewBag.member = member;
            ViewBag.curMaxInadvanceHours = curMaxInadvanceHours;
            return View(result);
        }
        /// <summary>
        /// 保存用户地址
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateAddress(FormCollection c)
        {
            OpResult result = new OpResult();
            result = new CustomerAddressService().SaveWebCustomerAddress(CurrentMember.MemberId.ToString(), new CustomerAddress
            {
                Address = c["address"],
                Area = c["area"],
                City = c["city"],
                Province = c["province"],
                ZipCode = c["code"].IsNotNullOrEmpty() ? null : int.Parse(c["code"]) as Nullable<int>,
                ReceiverTel = c["tel"],
                ReceiverMobile = c["mobile"],
                Receiver = c["receiver"]
            });
            return Json(new { validate = result.Successed, msg = result.Message, Data = result.Data });
        }
        [HttpPost]
        public ActionResult RemoveAddress(string id)
        {
            new CustomerAddressService().RemoveAddress(CurrentMember.MemberId.ToString(), id);
            return Json("");
        }
        /// <summary>
        /// 生成订单
        /// </summary>
        /// <param name="candle"></param>
        /// <param name="addressid"></param>
        /// <param name="rdtype"></param>
        /// <param name="time"></param>
        /// <param name="logistid"></param>
        /// <param name="couponPay"></param>
        /// <param name="feetype"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateOrder(CreateOrderModel createOrderModel)
        {
            string msg = "";
            Orders order = null;
            bool check = true;
            OpResult result = new OpResult { };
            var cartIDs = CookieHelper.GetCookieValue("cartids").Split(',').ToList();
            List<string> couponDetailIdList = null;
            List<string> giftCardDetailIdList = null;
            if (!string.IsNullOrEmpty(createOrderModel.CouponDetailIds))
                couponDetailIdList = createOrderModel.CouponDetailIds.Split(',').ToList();

            if (!string.IsNullOrEmpty(createOrderModel.GiftCardDetailIds))
                giftCardDetailIdList = createOrderModel.GiftCardDetailIds.Split(',').ToList();
            #region 验证
            var validateResult = osv.ValidateOrderIsAllowCreate(createOrderModel);
            if (validateResult.Successed == false)
            {
                check = false;
                msg = validateResult.Message;
            }
            #endregion

            ViewBag.msg = msg;
            if (check)
            {
                string userid = CurrentMember.MemberId.ToString();
                Action<Orders> setAddress;

                #region
                if (createOrderModel.RdType == DeliveryType.D2D)
                    setAddress = osv.SetOrderAddress(a => a.Id.Equals(createOrderModel.AddressId, StringComparison.OrdinalIgnoreCase));
                else
                    setAddress = osv.SetOrderAddress(null, a => a.Id.Equals(createOrderModel.LogistId, StringComparison.OrdinalIgnoreCase), createOrderModel.Name, createOrderModel.Mobile);
                #endregion

                result = osv.CreateOrder(
                    //客户
                    a => a.Id.Equals(userid),
                    //设置地址
                   setAddress,
                    //订单产品
                   a => { return osv.GetOrderDetailsByCarts(a, cartIDs); },
                    //发票
                    null,
                    //订单编辑
                    a =>
                    {
                        a.OrderSource = OrderSource.OnlineOrder;
                        a.Candle = createOrderModel.Candle;
                        a.RequiredTime = createOrderModel.Time;
                        a.FeeType = createOrderModel.FeeType;
                        a.CouponPay = createOrderModel.CouponPay;
                        a.GiftCardPay = createOrderModel.GiftCardPay;
                        a.IntegralPay = createOrderModel.IntegralPay;
                        a.UsedIntegralVal = createOrderModel.UsedIntegralVal;
                        a.DeliverMsg = createOrderModel.DeliverMsg;
                    },
                    //用户
                    userid
                    , 1, createOrderModel.TimeBucket,
                    couponDetailIdList, giftCardDetailIdList, createOrderModel.OrderTotalAmount);

                ViewBag.msg = result.Message;
            }
            if (check == false || result.Successed == false)
            {
                return Content(string.Format(@"<script>alert('{0}');
                                               window.location.href='/cart/settlement'</script>", string.IsNullOrEmpty(ViewBag.msg) ? "" : ViewBag.msg));
                //return Redirect("/cart/settlement");
            }

            order = (Orders)result.Data;
            new CartService().RemoveCart(cartIDs);
            if (order.NeedPay == 0)
            {
                return Redirect("/cart/OrderConfirm/" + order.No);
            }
            //前住支付
            if (createOrderModel.IsPay == 0)
            {
                if (order.FeeType == FeeType.Cash || order.FeeType == FeeType.POS || order.FeeType == FeeType.OtherPay)
                    return Redirect("/cart/OrderConfirm/" + order.No);
                if (order.FeeType == FeeType.ALiPay)
                    return Redirect("/cart/alipay/" + order.No);
                if (order.FeeType == FeeType.TenPay)
                    return Redirect("/Tenpay/Index/" + order.No);
                if (order.FeeType == FeeType.WXPay)
                    return Redirect("/Tenpay/GetWXCode/" + order.No);
                return Redirect("/cart/orderdetail/" + order.No);
            }

            return Redirect("/cart/OrderConfirm/" + order.No);
        }
        #endregion

        #region 支付及订单详情
        #region alipay
        [Authorize]
        public ActionResult AliPay(string id)
        {
            var order = psv.GetOrderByOrderNo(id);

            if (order != null && order.TradeStatus == TradeStatus.NotPay && order.CustomerId.Equals(CurrentMember.MemberId.ToString()))
            {
                #region 支付宝
                ////////////////////////////////////////////请求参数////////////////////////////////////////////
                //返回格式
                string format = "xml";
                //必填，不需要修改

                //返回格式
                string v = "2.0";
                //必填，不需要修改

                //请求号
                string req_id = DateTime.Now.ToString("yyyyMMddHHmmss");
                //必填，须保证每次请求都是唯一

                //支付宝网关地址
                string GATEWAY_NEW = "http://wappaygw.alipay.com/service/rest.htm?";
                //订单号
                string out_trade_no = order.No;
                //订单名称
                string subject = "蛋糕";
                //付款金额
                string total_fee = (order.TotalPrice - (order.CouponPay + order.GiftCardPay + order.IntegralPay)) + "";
                //订单描述        
                // string body = string.Join("\r\n", order.OrderDetails.Select(a => a.Name + "：" + a.Num + "张" + a.AllPrice + "元"));
                //商品展示地址
                string show_url = "";
                //防钓鱼时间戳
                string anti_phishing_key = Submit.Query_timestamp();
                //操作中断返回地址
                string merchant_url = Url.Action("Index", "Home");
                //用户付款中途退出返回商户的地址。需http://格式的完整路径，不允许加?id=123这类自定义参数

                //请求业务参数详细
                string req_dataToken = "<direct_trade_create_req><notify_url>" + ConfigurationManager.AppSettings["alipay_notify"] + "</notify_url><call_back_url>" + ConfigurationManager.AppSettings["alipay_return"] + "</call_back_url><seller_account_name>" + Config.Seller_Email + "</seller_account_name><out_trade_no>" + out_trade_no + "</out_trade_no><subject>" + subject + "</subject><total_fee>" + total_fee + "</total_fee><merchant_url>" + merchant_url + "</merchant_url></direct_trade_create_req>";
                //必填


                //把请求参数打包成数组
                Dictionary<string, string> sParaTempToken = new Dictionary<string, string>();
                sParaTempToken.Add("partner", Config.Partner);
                sParaTempToken.Add("_input_charset", Config.Input_charset.ToLower());
                sParaTempToken.Add("sec_id", Config.Sign_type.ToUpper());
                sParaTempToken.Add("service", "alipay.wap.trade.create.direct");
                sParaTempToken.Add("format", format);
                sParaTempToken.Add("v", v);
                sParaTempToken.Add("req_id", req_id);
                sParaTempToken.Add("req_data", req_dataToken);


                //建立请求
                string sHtmlTextToken = Submit.BuildRequest(GATEWAY_NEW, sParaTempToken);
                //URLDECODE返回的信息
                Encoding code = Encoding.GetEncoding(Config.Input_charset);
                sHtmlTextToken = HttpUtility.UrlDecode(sHtmlTextToken, code);

                //解析远程模拟提交后返回的信息
                Dictionary<string, string> dicHtmlTextToken = Submit.ParseResponse(sHtmlTextToken);

                //获取token
                string request_token = dicHtmlTextToken["request_token"];

                ////////////////////////////////////////////根据授权码token调用交易接口alipay.wap.auth.authAndExecute////////////////////////////////////////////


                //业务详细
                string req_data = "<auth_and_execute_req><request_token>" + request_token + "</request_token></auth_and_execute_req>";
                //必填

                //把请求参数打包成数组
                Dictionary<string, string> sParaTemp = new Dictionary<string, string>();
                sParaTemp.Add("partner", Config.Partner);
                sParaTemp.Add("_input_charset", Config.Input_charset.ToLower());
                sParaTemp.Add("sec_id", Config.Sign_type.ToUpper());
                sParaTemp.Add("service", "alipay.wap.auth.authAndExecute");
                sParaTemp.Add("format", format);
                sParaTemp.Add("v", v);
                sParaTemp.Add("req_data", req_data);

                //建立请求
                string sHtmlText = Submit.BuildRequest(GATEWAY_NEW, sParaTemp, "get", "确认");

                ViewBag.html = sHtmlText;

                return View();
                #endregion
            }

            return Redirect("/");
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AlipayNotify()
        {
            try
            {
                AddPayLog(true);
            }
            catch (Exception e)
            {
                PayLogService ps = new PayLogService();
                PayLog pl = new PayLog();
                pl = new PayLog();
                pl.ID = DataHelper.GetSystemID();
                pl.LogTime = DateTime.Now;
                pl.Message = e.Message;
                ps.Add(pl);
            }
            Dictionary<string, string> sPara = GetRequestPost();

            if (sPara.Count > 0)//判断是否有带返回参数
            {
                Notify aliNotify = new Notify();
                bool verifyResult = aliNotify.VerifyNotify(sPara, Request.Form["sign"]);

                if (verifyResult)//验证成功
                {
                    //获取支付宝的通知返回参数，可参考技术文档中服务器异步通知参数列表

                    //解密（如果是RSA签名需要解密，如果是MD5签名则下面一行清注释掉）
                    //sPara = aliNotify.Decrypt(sPara);

                    //XML解析notify_data数据
                    try
                    {
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(sPara["notify_data"]);
                        //商户订单号
                        string out_trade_no = xmlDoc.SelectSingleNode("/notify/out_trade_no").InnerText;
                        //支付宝交易号
                        string trade_no = xmlDoc.SelectSingleNode("/notify/trade_no").InnerText;
                        //交易状态
                        string trade_status = xmlDoc.SelectSingleNode("/notify/trade_status").InnerText;

                        if (trade_status == "TRADE_FINISHED")
                        {
                            //判断该笔订单是否在商户网站中已经做过处理
                            //如果没有做过处理，根据订单号（out_trade_no）在商户网站的订单系统中查到该笔订单的详细，并执行商户的业务程序
                            //如果有做过处理，不执行商户的业务程序
                            psv.FinishOrder(out_trade_no, trade_no, FeeType.ALiPay);
                            //注意：
                            //该种交易状态只在两种情况下出现
                            //1、开通了普通即时到账，买家付款成功后。
                            //2、开通了高级即时到账，从该笔交易成功时间算起，过了签约时的可退款时限（如：三个月以内可退款、一年以内可退款等）后。

                            return Content("success");  //请不要修改或删除
                        }
                        else if (trade_status == "TRADE_SUCCESS")
                        {
                            //判断该笔订单是否在商户网站中已经做过处理
                            //如果没有做过处理，根据订单号（out_trade_no）在商户网站的订单系统中查到该笔订单的详细，并执行商户的业务程序
                            //如果有做过处理，不执行商户的业务程序
                            psv.FinishOrder(out_trade_no, trade_no, FeeType.ALiPay);
                            //注意：
                            //该种交易状态只在一种情况下出现——开通了高级即时到账，买家付款成功后。


                            return Content("success");  //请不要修改或删除
                        }
                        else
                        {
                            return Content(trade_status);
                        }

                    }
                    catch (Exception exc)
                    {
                        return Content(exc.ToString());
                    }

                }
                else//验证失败
                {
                    return Content("fail");
                }
            }
            else
            {
                return Content("无通知参数");
            }
        }

        /// <summary>
        /// 支付返回get请求
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ValidateInput(false)]
        public ActionResult AlipayReturn()
        {
            //记录返回url
            try
            {
                AddPayLog(false);
            }
            catch (Exception)
            {

            }
            Dictionary<string, string> sPara = GetRequestGet();

            if (sPara.Count > 0)//判断是否有带返回参数
            {
                Notify aliNotify = new Notify();
                bool verifyResult = aliNotify.VerifyReturn(sPara, Request.QueryString["sign"]);

                if (verifyResult)//验证成功
                {
                    //获取支付宝的通知返回参数，可参考技术文档中页面跳转同步通知参数列表

                    //商户订单号
                    string orderNo = Request.QueryString["out_trade_no"];

                    //支付宝交易号
                    string trade_no = Request.QueryString["trade_no"];

                    //交易状态
                    string result = Request.QueryString["result"];
                    //result :判断支付结果及交易状态 result有且只有success一个交易状态

                    //判断是否在商户网站中已经做过了这次通知返回的处理
                    //如果没有做过处理，那么执行商户的业务程序
                    //如果有做过处理，那么不执行商户的业务程序
                    //交易成功
                    psv.FinishOrder(orderNo, trade_no, FeeType.ALiPay);
                    return Redirect("/cart/OrderConfirm/" + orderNo);

                }
                else//验证失败
                {
                    ViewBag.msg = "付款验证失败，请与管理员联系";

                    return View();
                }
            }
            else
            {
                ViewBag.msg = "付款验证失败，请与管理员联系";

                return View();
            }
        }

        public void AddPayLog(bool isNotify)
        {
            PayLog log = new PayLog();
            log.ID = DataHelper.GetSystemID();
            log.OrderNo = Request.QueryString["out_trade_no"];
            if (isNotify)
            {
                log.Message = Request.Form.ToString();
            }
            else
            {
                log.Message = Request.Url.ToString(); ;
            }
            log.LogTime = DateTime.Now;
            PayLogService ps = new PayLogService();
            ps.Add(log);
        }

        /// <summary>
        /// 获取支付宝POST过来通知消息，并以“参数名=参数值”的形式组成数组
        /// </summary>
        /// <returns>request回来的信息组成的数组</returns>
        public Dictionary<string, string> GetRequestPost()
        {

            int i = 0;
            Dictionary<string, string> sArray = new Dictionary<string, string>();
            NameValueCollection coll;
            //Load Form variables into NameValueCollection variable.
            coll = Request.Form;

            // Get names of all forms into a string array.
            String[] requestItem = coll.AllKeys;

            for (i = 0; i < requestItem.Length; i++)
            {
                sArray.Add(requestItem[i], Request.Form[requestItem[i]]);
            }

            return sArray;
        }

        /// <summary>
        /// 获取支付宝GET过来通知消息，并以“参数名=参数值”的形式组成数组
        /// </summary>
        /// <returns>request回来的信息组成的数组</returns>
        public Dictionary<string, string> GetRequestGet()
        {
            int i = 0;
            Dictionary<string, string> sArray = new Dictionary<string, string>();
            NameValueCollection coll;
            //Load Form variables into NameValueCollection variable.
            coll = Request.QueryString;

            // Get names of all forms into a string array.
            String[] requestItem = coll.AllKeys;

            for (i = 0; i < requestItem.Length; i++)
            {
                sArray.Add(requestItem[i], Request.QueryString[requestItem[i]]);
            }

            return sArray;
        }
        #endregion
        [Authorize]
        public ActionResult WeiXinPay(string id)
        {
            return View();
        }
        [Authorize]
        public ActionResult OrderDetail(string id)
        {
            var os = new OrderService();
            var order = os.GetByNo(id, CurrentMember.MemberId.ToString());
            var orderDetails = os.GetDetailByNo<CartVM>(order.No);
            ViewBag.orderDetails = orderDetails;
            //Session["orderid"] = order.No;
            //if (Session["code"] == null)
            //{
            //    return Redirect("/Tenpay/GetWXCode/" + id);
            //}
            return View(order);
        }
        [Authorize]
        public ActionResult OrderConfirm(string id)
        {
            var os = new OrderService();
            var order = os.GetByNo(id, CurrentMember.MemberId.ToString());
            var orderDetails = os.GetDetailByNo<CartVM>(order.No);
            ViewBag.orderDetails = orderDetails;
            return View(order);
        }
        #endregion
    }
}
