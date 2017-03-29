using FCake.Bll.Services;
using FCake.Domain.Entities;
using FCake.Bll.MemberAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FCake.Core.Common;
using FCake.Web.Models;
using FCake.Bll;
using FCake.Domain.Enums;
using System.Configuration;
using FCake.API.AliDirectPay;
using System.Collections.Specialized;
using FCake.API.TenpayApp;
using System.Text;
using System.Collections;
using FCake.Core.MvcCommon;
using System.Text.RegularExpressions;
using FCake.Domain.WebModels;

namespace FCake.Web.Controllers
{
    public class CartController : BaseController
    {
        private readonly OrderService osv = new OrderService();
        private readonly PayService psv = new PayService();  //支付业务
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
            var cartNum = GetCartNum();
            return Json(new { validate = true, msg = "添加成功", cartnum = cartNum });
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
            var cartNum = GetCartNum();
            return Json(new { validate = true, msg = "删除成功", cartnum = cartNum });
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
            var cartNum = GetCartNum();
            return Json(new { validate = result, num = num, cartnum = cartNum });
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
            string cartId = cs.AddCart(id, num);

            CookieHelper.SetCookie("cartids", string.Join(",", cartId));
            return Redirect("/cart/Settlement");
            //return Redirect("/cart/Index");//立即购买跳转至购物车

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
            var result = _cartSvc.CheckedCarts(cartIds);
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
        /// 订单明细的商品清单列表Html局部页面（用于异步刷新）
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetSettlementProductListHtml()
        {
            var cartIds = CookieHelper.GetCookieValue("cartids").Split(',').ToList();
            var result = _cartSvc.CheckedCarts(cartIds);
            return PartialView("_Partial_Settlement_ProductList", result);
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
                ZipCode = c["code"].IsNullOrTrimEmpty() ? null : int.Parse(c["code"]) as Nullable<int>,
                ReceiverTel = c["tel"],
                ReceiverMobile = c["mobile"],
                Receiver = c["receiver"]
            });
            return Json(new { validate = result.Successed, msg = result.Message, address = result.Data });
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
                    , 0, createOrderModel.TimeBucket,
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
            if (order.FeeType == FeeType.WXPay && order.TradeStatus == TradeStatus.NotPay)
            {
                return Redirect("/Tenpay/GetPayUrl/" + order.No);
            }

            return Redirect("/cart/OrderConfirm/" + order.No);
        }
        #endregion





        #region 支付及订单详情

        #region 财付通支付相关
        public string WeiXinPay(string id)
        {
            var order = psv.GetOrderByOrderNo(id);

            HttpContextBase context = this.HttpContext;

            RequestHandler reqHandler = new RequestHandler(context);
            //初始化
            reqHandler.init();
            var tenpay_id = ConfigurationManager.AppSettings["tenpay_bargainor_id"];//商户号
            var tenpay_key = ConfigurationManager.AppSettings["tenpay_key"];//密钥

            reqHandler.setKey(tenpay_key);
            reqHandler.setGateUrl("https://gw.tenpay.com/gateway/pay.htm");

            //-----------------------------
            //设置支付参数
            //-----------------------------
            reqHandler.setParameter("partner", TenpayUtil.bargainor_id);		        //商户号
            reqHandler.setParameter("out_trade_no", order.No);		//商家订单号
            reqHandler.setParameter("total_fee", (Convert.ToDouble(order.TotalPrice) * 100).ToString());			        //商品金额,以分为单位
            reqHandler.setParameter("return_url", ConfigurationManager.AppSettings["tenpay_return"]);		    //交易完成后跳转的URL
            reqHandler.setParameter("notify_url", ConfigurationManager.AppSettings["tenpay_notify"]);		    //接收财付通通知的URL
            reqHandler.setParameter("body", "测试");	                    //商品描述
            reqHandler.setParameter("bank_type", "DEFAULT");		    //银行类型(中介担保时此参数无效)
            reqHandler.setParameter("spbill_create_ip", Request.ServerVariables.Get("Local_Addr").ToString());   //用户的公网ip，不是商户服务器IP
            reqHandler.setParameter("fee_type", "1");                    //币种，1人民币
            reqHandler.setParameter("subject", "商品名称");              //商品名称(中介交易时必填)


            //系统可选参数
            reqHandler.setParameter("sign_type", "MD5");
            reqHandler.setParameter("service_version", "1.0");
            reqHandler.setParameter("input_charset", "UTF-8");
            reqHandler.setParameter("sign_key_index", "1");

            //业务可选参数

            reqHandler.setParameter("attach", "");                      //附加数据，原样返回
            reqHandler.setParameter("product_fee", "0");                 //商品费用，必须保证transport_fee + product_fee=total_fee
            reqHandler.setParameter("transport_fee", "0");               //物流费用，必须保证transport_fee + product_fee=total_fee
            reqHandler.setParameter("time_start", DateTime.Now.ToString("yyyyMMddHHmmss"));            //订单生成时间，格式为yyyymmddhhmmss
            reqHandler.setParameter("time_expire", "");                 //订单失效时间，格式为yyyymmddhhmmss
            reqHandler.setParameter("buyer_id", "");                    //买方财付通账号
            reqHandler.setParameter("goods_tag", "");                   //商品标记
            reqHandler.setParameter("trade_mode", "1");                 //交易模式，1即时到账(默认)，2中介担保，3后台选择（买家进支付中心列表选择）
            reqHandler.setParameter("transport_desc", "");              //物流说明
            reqHandler.setParameter("trans_type", "1");                  //交易类型，1实物交易，2虚拟交易
            reqHandler.setParameter("agentid", "");                     //平台ID
            reqHandler.setParameter("agent_type", "");                  //代理模式，0无代理(默认)，1表示卡易售模式，2表示网店模式
            reqHandler.setParameter("seller_id", "");                   //卖家商户号，为空则等同于partner



            //获取请求带参数的url
            string requestUrl = reqHandler.getRequestURL();

            //post提交
            StringBuilder sb = new StringBuilder();
            sb.Append("<form id='tenpaysubmit' method=\"post\" action=\"" + reqHandler.getGateUrl() + "\" >\n");
            Hashtable ht = reqHandler.getAllParameters();
            foreach (DictionaryEntry de in ht)
            {
                sb.Append("<input type=\"hidden\" name=\"" + de.Key + "\" value=\"" + de.Value + "\" >\n");
            }
            sb.Append("<input type=\"submit\" value=\"财付通支付\" style=\"display:none\" >\n</form>\n");
            sb.Append("<script>document.forms['tenpaysubmit'].submit();</script>");
            return sb.ToString();
        }
        #endregion


        #region alipay
        [Authorize]
        public ActionResult AliPay(string id)
        {
            var order = psv.GetOrderByOrderNo(id);

            if (order != null && order.TradeStatus == TradeStatus.NotPay && order.CustomerId.Equals(CurrentMember.MemberId.ToString()))
            {
                #region 支付宝
                ////////////////////////////////////////////请求参数////////////////////////////////////////////

                //订单号
                string out_trade_no = order.No;
                //订单名称
                string subject = "蛋糕";
                //付款金额
                string total_fee = ((order.TotalPrice - (order.CouponPay + order.GiftCardPay + order.IntegralPay)) > 0 ? (order.TotalPrice - (order.CouponPay + order.GiftCardPay + order.IntegralPay)).ToString() : "0.1") + "";
                //订单描述        
                // string body = string.Join("\r\n", order.OrderDetails.Select(a => a.Name + "：" + a.Num + "张" + a.AllPrice + "元"));
                //商品展示地址
                string show_url = "";
                //防钓鱼时间戳
                string anti_phishing_key = Submit.Query_timestamp();

                string domain_url = Request.Url.ToString();
                Regex reg = new Regex(@"http://.*?/");
                var res = reg.Match(domain_url);
                if (res.Success)
                {
                    domain_url = res.Value;
                }


                string notify_url = ConfigurationManager.AppSettings["alipay_notify"];
                string return_url = ConfigurationManager.AppSettings["alipay_return"];

                notify_url = domain_url + notify_url;
                return_url = domain_url + return_url;
                ////////////////////////////////////////////////////////////////////////////////////////////////

                //把请求参数打包成数组
                SortedDictionary<string, string> sParaTemp = new SortedDictionary<string, string>();
                sParaTemp.Add("partner", Config.Partner);
                sParaTemp.Add("_input_charset", Config.Input_charset.ToLower());
                sParaTemp.Add("service", "create_direct_pay_by_user");
                sParaTemp.Add("payment_type", "1");
                sParaTemp.Add("notify_url", notify_url);
                sParaTemp.Add("return_url", return_url);
                sParaTemp.Add("seller_email", Config.Seller_Email);
                sParaTemp.Add("out_trade_no", out_trade_no);
                sParaTemp.Add("subject", subject);
                sParaTemp.Add("total_fee", String.Format("{0:F}", Convert.ToDecimal(total_fee)));
                //sParaTemp.Add("body", body);
                sParaTemp.Add("show_url", show_url);
                sParaTemp.Add("anti_phishing_key", anti_phishing_key);
                sParaTemp.Add("exter_invoke_ip", ConfigurationManager.AppSettings["alipay_ip"]);

                //建立请求
                string sHtmlText = Submit.BuildRequest(sParaTemp, "post", "确认");

                ViewBag.html = sHtmlText;

                return View();
                #endregion
            }

            return Redirect("/Member/Index");
        }

        public ActionResult AlipayNotify()
        {
            //记录返回url
            try
            {
                AddPayLog(true);
            }
            catch (Exception)
            {

            }
            SortedDictionary<string, string> sPara = GetRequestPost();

            if (sPara.Count > 0)//判断是否有带返回参数
            {
                Notify aliNotify = new Notify();
                bool verifyResult = aliNotify.Verify(sPara, Request.Form["notify_id"], Request.Form["sign"]);

                if (verifyResult)//验证成功
                {
                    //商户订单号
                    string out_trade_no = Request.Form["out_trade_no"];
                    //支付宝交易号
                    string trade_no = Request.Form["trade_no"];
                    //交易状态
                    string trade_status = Request.Form["trade_status"];

                    if (Request.Form["trade_status"] == "TRADE_FINISHED")
                    {
                        //判断该笔订单是否在商户网站中已经做过处理
                        //如果没有做过处理，根据订单号（out_trade_no）在商户网站的订单系统中查到该笔订单的详细，并执行商户的业务程序
                        //如果有做过处理，不执行商户的业务程序
                        psv.FinishOrder(out_trade_no, trade_no, FeeType.ALiPay);
                        //注意：
                        //该种交易状态只在两种情况下出现
                        //1、开通了普通即时到账，买家付款成功后。
                        //2、开通了高级即时到账，从该笔交易成功时间算起，过了签约时的可退款时限（如：三个月以内可退款、一年以内可退款等）后。
                    }
                    else if (Request.Form["trade_status"] == "TRADE_SUCCESS")
                    {
                        //判断该笔订单是否在商户网站中已经做过处理
                        //如果没有做过处理，根据订单号（out_trade_no）在商户网站的订单系统中查到该笔订单的详细，并执行商户的业务程序
                        //如果有做过处理，不执行商户的业务程序
                        psv.FinishOrder(out_trade_no, trade_no, FeeType.ALiPay);
                        //注意：
                        //该种交易状态只在一种情况下出现——开通了高级即时到账，买家付款成功后。
                    }

                    return Content("success");
                }
                else
                {
                    return Content("fail");
                }
            }
            else
            {
                return Content("无通知参数");
            }
        }
        public ActionResult AlipayReturn()
        {//记录返回url
            try
            {
                AddPayLog(false);
            }
            catch (Exception)
            {

            }

            try
            {
                SortedDictionary<string, string> sPara = GetRequestGet();
                if (sPara.Count > 0)
                {
                    Notify aliNotify = new Notify();
                    bool verifyResult = aliNotify.Verify(sPara, Request.QueryString["notify_id"], Request.QueryString["sign"]);

                    if (verifyResult)
                    {
                        //订单号
                        string orderNo = Request.QueryString["out_trade_no"];
                        //支付宝交易号
                        string trade_no = Request.QueryString["trade_no"];
                        //交易状态
                        string trade_status = Request.QueryString["trade_status"];

                        if (Request.QueryString["trade_status"] == "TRADE_FINISHED" || Request.QueryString["trade_status"] == "TRADE_SUCCESS")
                        {
                            //交易成功
                            psv.FinishOrder(orderNo, trade_no, FeeType.ALiPay);
                            return Redirect("/cart/OrderConfirm/" + orderNo);
                        }
                    }

                }
            }
            catch (Exception ex)
            {

            }
            ViewBag.msg = "付款验证失败，请与管理员联系";

            return View();
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
        public SortedDictionary<string, string> GetRequestPost()
        {
            int i = 0;
            SortedDictionary<string, string> sArray = new SortedDictionary<string, string>();
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
        public SortedDictionary<string, string> GetRequestGet()
        {
            int i = 0;
            SortedDictionary<string, string> sArray = new SortedDictionary<string, string>();
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
        //[Authorize]
        //public ActionResult WeiXinPay(string id)
        //{
        //    return View();
        //}
        [Authorize]
        public ActionResult OrderDetail(string id)
        {
            var os = new OrderService();
            var order = os.GetByNo(id, CurrentMember.MemberId.ToString());
            var orderDetails = os.GetDetailByNo<CartVM>(order.No);
            ViewBag.orderDetails = orderDetails;
            return View(order);
        }
        [Authorize]
        public ActionResult OrderConfirm(string id)
        {
            ViewBag.isTrue = false;
            var os = new OrderService();
            var order = os.GetByNo(id, CurrentMember.MemberId.ToString());


            if (order.TradeStatus == TradeStatus.NotPay && (order.FeeType == FeeType.ALiPay || order.FeeType == FeeType.TenPay || order.FeeType == FeeType.WXPay))
            {//var confirm=window.open(" + Url.Action("OrderComfirm", "Cart") + ");confirm.focus();
                //HttpContext.Response.Write("<script language=javascript>var pay=window.open('" + Url.Action(action, controller, new { id = order.No }) + "');pay.focus();</script>");
                ViewBag.isTrue = true;
            }
            if (order.TradeStatus == TradeStatus.NotPay)
            {
                var reditUrl = string.Empty;
                switch (order.FeeType)
                {
                    case FeeType.ALiPay:
                        reditUrl = "/cart/alipay/" + order.No;
                        break;
                    case FeeType.TenPay:
                        reditUrl = "/Tenpay/Index/" + order.No;
                        break;
                    case FeeType.WXPay:
                        reditUrl = "/Tenpay/GetPayUrl/" + order.No;
                        break;
                }
                ViewBag.hrefUrl = reditUrl;
            }

            return View(order);
        }
        //[Authorize]
        //public ActionResult goPay(Order order)
        //{
        //    var controller = string.Empty;
        //    var action = string.Empty;
        //    if (order.FeeType == FeeType.ALiPay)
        //    {
        //        controller = "cart";
        //        action = "alipay";
        //    }
        //    if (order.FeeType == FeeType.WeiXinPay)
        //    {
        //        controller = "Tenpay";
        //        action = "Index";
        //    }
        //    if (order.TradeStatus == TradeStatus.NotPay && (order.FeeType == FeeType.ALiPay || order.FeeType == FeeType.WeiXinPay))
        //        HttpContext.Response.Write("<script language=javascript>var confirm=window.open(" + Url.Action("OrderComfirm","Cart")+");confirm.focus();var pay=window.open(" + Url.Action(action, controller, new { id = order.No }) + ")</script>");
        //    return View();
        //}
        #endregion
    }
}
