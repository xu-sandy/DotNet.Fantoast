using FCake.API.TenpayApp;
using FCake.API.WxPayAPI;
using FCake.Bll.Services;
using FCake.Domain.Entities;
using FCake.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using ThoughtWorks.QRCode.Codec;

namespace FCake.Web.Controllers
{

    public class TenpayController : BaseController
    {
        private PayService psv = new PayService();  //支付业务
        PayLogService log = new PayLogService();
        #region 财付通相关
        /// <summary>
        /// 跳转支付接口  
        /// </summary>
        /// <returns></returns>
        public ActionResult Index(string id) //传入订单ID  输入订单价格等信息  订单必须是未支付状态
        {
            try
            {
                var order = psv.GetOrderByOrderNo(id);

                if (order.TradeStatus == TradeStatus.NotPay)
                {
                    //创建RequestHandler实例
                    RequestHandler reqHandler = new RequestHandler(HttpContext);
                    //初始化
                    reqHandler.init();
                    reqHandler.init();
                    var tenpay_id = ConfigurationManager.AppSettings["tenpay_bargainor_id"];//商户号
                    var tenpay_key = ConfigurationManager.AppSettings["tenpay_key"];//密钥

                    reqHandler.setKey(tenpay_key);
                    reqHandler.setGateUrl("https://gw.tenpay.com/gateway/pay.htm");

                    //-----------------------------
                    //设置支付参数
                    //-----------------------------
                    reqHandler.setParameter("partner", tenpay_id);		        //商户号
                    reqHandler.setParameter("out_trade_no", order.No);		//商家订单号
                    reqHandler.setParameter("total_fee", (Convert.ToDouble(order.NeedPay) * 100).ToString());			        //商品金额,以分为单位
                    reqHandler.setParameter("return_url", ConfigurationManager.AppSettings["tenpay_return"]);		    //交易完成后跳转的URL
                    reqHandler.setParameter("notify_url", ConfigurationManager.AppSettings["tenpay_notify"]);		    //接收财付通通知的URL
                    reqHandler.setParameter("body", "蛋糕");	                    //商品描述
                    reqHandler.setParameter("bank_type", "DEFAULT");		    //银行类型(中介担保时此参数无效)
                    reqHandler.setParameter("spbill_create_ip", Request.ServerVariables.Get("Local_Addr").ToString());   //用户的公网ip，不是商户服务器IP
                    reqHandler.setParameter("fee_type", "1");                    //币种，1人民币
                    reqHandler.setParameter("subject", "蛋糕");              //商品名称(中介交易时必填)


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

                    //卖家商户号，为空则等同于partner

                    return Redirect(reqHandler.getRequestURL());
                }
                else
                {
                    return Content("该订单已完成支付");
                }
            }
            catch
            {
                return Content("支付失败");
            }
        }
        /// <summary>
        /// 通知页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Notify()
        {
            //创建ResponseHandler实例
            ResponseHandler resHandler = new ResponseHandler(HttpContext);
            resHandler.setKey(TenpayUtil.tenpay_key);

            //判断签名
            if (resHandler.isTenpaySign())
            {
                ///通知id
                string notify_id = resHandler.getParameter("notify_id");
                //通过通知ID查询，确保通知来至财付通
                //创建查询请求
                RequestHandler queryReq = new RequestHandler(HttpContext);
                queryReq.init();
                queryReq.setKey(TenpayUtil.tenpay_key);
                queryReq.setGateUrl("https://gw.tenpay.com/gateway/simpleverifynotifyid.xml");
                queryReq.setParameter("partner", TenpayUtil.bargainor_id);
                queryReq.setParameter("notify_id", notify_id);

                //通信对象
                TenpayHttpClient httpClient = new TenpayHttpClient();
                httpClient.setTimeOut(5);
                //设置请求内容
                httpClient.setReqContent(queryReq.getRequestURL());
                //后台调用
                if (httpClient.call())
                {
                    //设置结果参数
                    ClientResponseHandler queryRes = new ClientResponseHandler();
                    queryRes.setContent(httpClient.getResContent());
                    queryRes.setKey(TenpayUtil.tenpay_key);
                    //判断签名及结果
                    //只有签名正确,retcode为0，trade_state为0才是支付成功
                    if (queryRes.isTenpaySign())
                    {
                        //取结果参数做业务处理
                        string out_trade_no = queryRes.getParameter("out_trade_no");
                        //财付通订单号
                        string transaction_id = queryRes.getParameter("transaction_id");
                        //金额,以分为单位
                        string total_fee = queryRes.getParameter("total_fee");
                        //如果有使用折扣券，discount有值，total_fee+discount=原请求的total_fee
                        string discount = queryRes.getParameter("discount");
                        //支付结果
                        string trade_state = resHandler.getParameter("trade_state");
                        //交易模式，1即时到帐 2中介担保
                        string trade_mode = resHandler.getParameter("trade_mode");
                        #region
                        //判断签名及结果
                        if ("0".Equals(queryRes.getParameter("retcode")))
                        {
                            //Response.Write("id验证成功");

                            if ("1".Equals(trade_mode))
                            {       //即时到账 
                                if ("0".Equals(trade_state))
                                {
                                    //------------------------------
                                    //即时到账处理业务开始
                                    //------------------------------

                                    #region 订单完成逻辑
                                    psv.FinishOrder(out_trade_no, transaction_id, FeeType.TenPay);


                                    #endregion

                                    //处理数据库逻辑
                                    //注意交易单不要重复处理
                                    //注意判断返回金额

                                    //------------------------------
                                    //即时到账处理业务完毕
                                    //------------------------------

                                    //给财付通系统发送成功信息，财付通系统收到此结果后不再进行后续通知
                                    //Response.Write("success");

                                    return RedirectToAction("Success");

                                }
                                else
                                {
                                    return Content("即时到账支付失败");
                                }
                            }
                        }
                        else
                        {
                            return Content("查询验证签名失败或id验证失败");
                        }
                        #endregion
                    }
                    else
                    {
                        return Content("通知ID查询签名验证失败");
                    }
                }
                else
                {
                    return Content("后台调用通信失败");
                }
            }
            else
            {
                return Content("签名验证失败");
            }

            return Content("支付失败");
        }
        /// <summary>
        /// 返回页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Return()
        {
            //创建ResponseHandler实例
            ResponseHandler resHandler = new ResponseHandler(HttpContext);
            resHandler.setKey(TenpayUtil.tenpay_key);

            //判断签名
            if (resHandler.isTenpaySign())
            {

                ///通知id
                string notify_id = resHandler.getParameter("notify_id");
                //商户订单号
                string out_trade_no = resHandler.getParameter("out_trade_no");
                //财付通订单号
                string transaction_id = resHandler.getParameter("transaction_id");
                //金额,以分为单位
                string total_fee = resHandler.getParameter("total_fee");
                //如果有使用折扣券，discount有值，total_fee+discount=原请求的total_fee
                string discount = resHandler.getParameter("discount");
                //支付结果
                string trade_state = resHandler.getParameter("trade_state");
                //交易模式，1即时到账，2中介担保
                string trade_mode = resHandler.getParameter("trade_mode");

                if ("1".Equals(trade_mode))
                {       //即时到账 
                    if ("0".Equals(trade_state))
                    {
                        #region 完成订单逻辑
                        psv.FinishOrder(out_trade_no, transaction_id, FeeType.TenPay);

                        #endregion

                        return RedirectToAction("Success");
                    }
                    else
                    {
                        return Content("即时到账支付失败");
                    }

                }
            }
            else
            {
                return Content("认证签名失败");
            }

            return Content("支付失败");
        }

        /// <summary>
        /// 支付成功页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Success()
        {
            return View();
        }
        
        #endregion

        #region 微信支付相关
        /// <summary>
        /// 获取扫描支付的二维码地址
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult GetPayUrl(string id)
        {

            var order = new PayService().GetOrderByOrderNo(id);

            PrePayInfo info = new PrePayInfo();
            info.OrderNo = order.No;
            info.PrePayNo = FCake.Core.Common.DataHelper.GetSystemID();
            info.NeedPay = int.Parse(order.NeedPay.ToString().Split('.')[0]) * 100;
            info.LogingTime = DateTime.Now;
            new PrePayInfoService().AddPayInfo(info);



            WxPayData data = new WxPayData();
            data.SetValue("body", "Fancake-蛋糕");//商品描述
            data.SetValue("attach", "Fancake");//附加数据
            data.SetValue("out_trade_no", info.PrePayNo);//订单号
            data.SetValue("total_fee", Convert.ToInt32((order.NeedPay * 100).ToString().Split('.')[0]));//总金额(order.NeedPay * 100).ToString().Split('.')[0]
            data.SetValue("time_start", DateTime.Now.ToString("yyyyMMddHHmmss"));//交易起始时间
            data.SetValue("time_expire", DateTime.Now.AddMinutes(10).ToString("yyyyMMddHHmmss"));//交易结束时间
            data.SetValue("goods_tag", "Fancake");//商品标记
            data.SetValue("trade_type", "NATIVE");//交易类型
            data.SetValue("product_id", order.Id);//商品ID

            WxPayData result = WxPayApi.UnifiedOrder(data);//调用统一下单接口
            string temp = result.GetValue("code_url").ToString();//获得统一下单接口返回的二维码链接
            //temp = HttpUtility.UrlEncode(temp);

            //生成二维码
            //初始化二维码生成工具
            QRCodeEncoder qrCodeEncoder = new QRCodeEncoder();
            qrCodeEncoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;
            qrCodeEncoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M;
            qrCodeEncoder.QRCodeVersion = 0;
            qrCodeEncoder.QRCodeScale = 4;
            //将字符串生成二维码图片
            Bitmap image = qrCodeEncoder.Encode(temp, Encoding.Default);
            //end
            MemoryStream ms = new MemoryStream();
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            string strUrl = "data:image/png;base64," + Convert.ToBase64String(ms.ToArray());
            image.Dispose();


            ViewBag.src = strUrl;
            //return File(ms.ToArray(), "png");
            return View();
        }


        /// <summary>
        /// 微信的异步支付返回
        /// </summary>
        /// <returns></returns>
        public ActionResult WetChatNotify()
        {
            try
            {
                //PayLog notity1 = new PayLog();
                //notity1.ID = FCake.Core.Common.DataHelper.GetSystemID();
                //notity1.LogTime = DateTime.Now;
                //notity1.Message = "notity:返回结果通知开始执行";
                //log.Add(notity1);
                //需验证支付结果
                WxPayData notifyData = GetNotifyData();

                PayLog notity2 = new PayLog();
                notity2.ID = FCake.Core.Common.DataHelper.GetSystemID();
                notity2.LogTime = DateTime.Now;
                notity2.Message = "notity2接收到的数据:" + notifyData.ToJson().ToString();
                log.Add(notity2);

                WxPayData res = new WxPayData();
                //检查支付结果中transaction_id是否存在
                if (!notifyData.IsSet("transaction_id"))
                {
                    //若transaction_id不存在，则立即返回结果给微信支付后台
                    res.SetValue("return_code", "FAIL");
                    res.SetValue("return_msg", "支付结果中微信订单号不存在");
                    PayLog notity3 = new PayLog();
                    notity3.ID = FCake.Core.Common.DataHelper.GetSystemID();
                    notity3.LogTime = DateTime.Now;
                    notity3.Message = "notity3接收到的数据:没有transaction_id";
                    log.Add(notity3);
                }

                string transaction_id = notifyData.GetValue("transaction_id").ToString();

                //查询订单，判断订单真实性
                if (!QueryOrder(transaction_id))
                {
                    //若订单查询失败，则立即返回结果给微信支付后台
                    res.SetValue("return_code", "FAIL");
                    res.SetValue("return_msg", "订单查询失败");
                }
                //查询订单成功
                else
                {
                    res.SetValue("return_code", "SUCCESS");
                    res.SetValue("return_msg", "OK");
                    var orderId = notifyData.GetValue("out_trade_no").ToString();

                    PayLog notity4 = new PayLog();
                    notity4.ID = FCake.Core.Common.DataHelper.GetSystemID();
                    notity4.LogTime = DateTime.Now;
                    notity4.Message = "notity4接收到的out_trade_no数据:" + orderId;
                    log.Add(notity4);

                    var preInfo = new PrePayInfoService().GetPrePayByPrePayNo(orderId);

                    var order = psv.GetOrderByOrderNo(preInfo.OrderNo);//获取订单信息
                    if (order.Status == OrderStatus.NotPay)
                    {
                        PayLog notity5 = new PayLog();
                        notity5.ID = FCake.Core.Common.DataHelper.GetSystemID();
                        notity5.LogTime = DateTime.Now;
                        notity5.Message = "notity5开始修改" + order.No + "订单状态：";
                        log.Add(notity5);
                        psv.FinishOrder(order.No, transaction_id, FeeType.WXPay);
                    }
                }
                return Content(res.ToString());
            }
            catch (Exception e)
            {
                PayLog notity99 = new PayLog();
                notity99.ID = FCake.Core.Common.DataHelper.GetSystemID();
                notity99.LogTime = DateTime.Now;
                notity99.Message = "回调发生异常" + e.Message;
                log.Add(notity99);
                return Content("error");
            }
        }
        //查询订单
        private bool QueryOrder(string transaction_id)
        {
            WxPayData req = new WxPayData();
            req.SetValue("transaction_id", transaction_id);
            WxPayData res = WxPayApi.OrderQuery(req);
            if (res.GetValue("return_code").ToString() == "SUCCESS" &&
                res.GetValue("result_code").ToString() == "SUCCESS")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public WxPayData GetNotifyData()
        {
            //接收从微信后台post过来的数据
            Stream s = Request.InputStream;
            int count = 0;
            byte[] buffer = new byte[1024];
            StringBuilder builder = new StringBuilder();
            while ((count = s.Read(buffer, 0, 1024)) > 0)
            {
                builder.Append(Encoding.UTF8.GetString(buffer, 0, count));
            }
            s.Flush();
            s.Close();
            s.Dispose();
            WxPayData data = new WxPayData();
            try
            {
                data.FromXml(builder.ToString());
            }
            catch (WxPayException ex)
            {
                WxPayData res = new WxPayData();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", ex.Message);
                return res;
            }
            return data;
        }
        
        #endregion

    }
}
