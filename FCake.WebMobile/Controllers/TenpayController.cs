using FCake.API.TenpayApp;
using FCake.API.WxPayAPI;
using FCake.Bll.Services;
using FCake.Core.MvcCommon;
using FCake.Domain.Entities;
using FCake.Domain.Enums;
using FCake.WebMobile.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Services;
using System.Xml;

namespace FCake.WebMobile.Controllers
{
    public class TenpayController : Controller
    {
        private PayService psv = new PayService();  //支付业务
        PayLogService log = new PayLogService();

        #region 财付通
        /// <summary>
        /// 财付通支付接口  
        /// </summary>
        /// <returns></returns>
        public ActionResult Index(string id) //传入订单ID  输入订单价格等信息  订单必须是未支付状态
        {
            try
            {
                var order = psv.GetOrderByOrderNo(id);
                var tenpay_id = ConfigurationManager.AppSettings["tenpay_bargainor_id"];//商户号
                var tenpay_key = ConfigurationManager.AppSettings["tenpay_key"];//密钥
                //创建RequestHandler实例
                RequestHandler reqHandler = new RequestHandler(HttpContext);
                //通信对象
                TenpayHttpClient httpClient = new TenpayHttpClient();
                //应答对象
                ClientResponseHandler resHandler = new ClientResponseHandler();

                reqHandler.init();
                //设置密钥
                reqHandler.setKey(TenpayUtil.tenpay_key);
                reqHandler.setGateUrl("https://wap.tenpay.com/cgi-bin/wappayv2.0/wappay_init.cgi");


                //-----------------------------
                //设置支付初始化参数
                //-----------------------------
                reqHandler.setParameter("ver", "2.0");
                reqHandler.setParameter("charset", "1");
                reqHandler.setParameter("bank_type", "0");
                reqHandler.setParameter("desc", "蛋糕");          //订单描述
                reqHandler.setParameter("bargainor_id", tenpay_id);
                reqHandler.setParameter("sp_billno", order.No);                        //订单号
                reqHandler.setParameter("total_fee", (Convert.ToDouble(order.NeedPay) * 100).ToString());
                reqHandler.setParameter("fee_type", "1");
                reqHandler.setParameter("notify_url", ConfigurationManager.AppSettings["tenpay_notify"]);
                reqHandler.setParameter("callback_url", ConfigurationManager.AppSettings["tenpay_return"]);
                reqHandler.setParameter("attach", "attach");

                string initRequestUrl = reqHandler.getRequestURL();
                //设置请求内容
                httpClient.setReqContent(initRequestUrl);
                //设置超时
                httpClient.setTimeOut(5);

                string rescontent = "";
                string payRequestUrl = "";

                //后台调用
                if (httpClient.call())
                {
                    //获取结果
                    rescontent = httpClient.getResContent();

                    //设置结果参数
                    resHandler.setContent(rescontent);

                    string token_id = resHandler.getParameter("token_id");

                    //成功，则token_id有只
                    if (token_id != "")
                    {
                        //生成支付请求
                        payRequestUrl = "https://wap.tenpay.com/cgi-bin/wappayv2.0/wappay_gate.cgi?token_id=" + TenpayUtil.UrlEncode(token_id, Request.ContentEncoding.BodyName);

                        return Redirect(payRequestUrl);
                    }
                    else
                    {
                        return Content("支付初始化错误:" + resHandler.getParameter("err_info") + "<br>");
                    }

                }
                else
                {
                    return Content("call err:" + httpClient.getErrInfo() + "<br>" + httpClient.getResponseCode() + "<br>");
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

                //支付结果
                string pay_result = resHandler.getParameter("pay_result");
                //商户订单号
                string sp_billno = resHandler.getParameter("sp_billno");
                //财付通订单号
                string transaction_id = resHandler.getParameter("transaction_id");
                //金额,以分为单位
                string total_fee = resHandler.getParameter("total_fee");

                if (pay_result == "0")
                {
                    //------------------------------
                    //处理业务开始
                    //------------------------------
                    psv.FinishOrder(sp_billno, transaction_id, FeeType.TenPay);
                    //注意交易单不要重复处理
                    //注意判断返回金额

                    //------------------------------
                    //处理业务完毕
                    //------------------------------	

                    //返回成功处理的标记
                    return RedirectToAction("Success");
                }
                else
                {
                    //当做不成功处理
                    return Content("fail");
                }

            }
            else
            {
                //签名错误返回失败
                return Content("fail");

            }

        }
        /// <summary>
        /// 返回页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Return()
        {
            //创建ResponseHandler实例
            WapPayPageResponseHandler resHandler = new WapPayPageResponseHandler(HttpContext);
            resHandler.setKey(TenpayUtil.tenpay_key);

            //判断签名
            if (resHandler.isTenpaySign())
            {
                //支付结果
                string pay_result = resHandler.getParameter("pay_result");
                //商户订单号
                string sp_billno = resHandler.getParameter("sp_billno");
                //财付通订单号
                string transaction_id = resHandler.getParameter("transaction_id");
                //金额,以分为单位
                string total_fee = resHandler.getParameter("total_fee");

                if ("0".Equals(pay_result))
                {
                    //------------------------------
                    //处理业务开始
                    //------------------------------
                    psv.FinishOrder(sp_billno, transaction_id, FeeType.TenPay);
                    //注意交易单不要重复处理
                    //注意判断返回金额

                    //------------------------------
                    //处理业务完毕
                    //------------------------------	
                    return Redirect("/cart/OrderConfirm/" + sp_billno);
                }
                else
                {
                    //当做不成功处理
                    return Content("支付失败");
                }

            }
            else
            {
                return Content("认证签名失败");
            }


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


        #region 微信支付
        /// <summary>
        /// 接收code参数
        /// </summary>
        /// <returns></returns>
        public ActionResult ResponseParam()
        {
            try
            {
                var code = Request["code"];
                if (code != null)
                {
                    Session["code"] = code;
                }
                return Redirect("/Tenpay/WXPay/" + Session["orderid"]);
            }
            catch (Exception e)
            {
                PayLog model = new PayLog();
                model.ID = FCake.Core.Common.DataHelper.GetSystemID();
                model.LogTime = DateTime.Now;
                model.Message = "error:" + e.Message;
                log.Add(model);
                return Redirect("/");
            }
        }
        /// <summary>
        /// 发送获取code请求
        /// 微信支付入口
        /// </summary>
        /// <returns></returns>
        public ActionResult GetWXCode(string id)
        {
            Session["orderid"] = id;
            var myurl = Request.Url.Scheme + "://" + Request.Url.Host + "/tenpay/ResponseParam";//获取code的地址
            var appid = WxPayConfig.APPID; 
            var url = string.Format("https://open.weixin.qq.com/connect/oauth2/authorize?appid={0}&redirect_uri={1}&response_type=code&scope=snsapi_userinfo&state=STATE#wechat_redirect", appid, myurl);
            #region 日志
            PayLog model = new PayLog();
            model.ID = FCake.Core.Common.DataHelper.GetSystemID();
            model.LogTime = DateTime.Now;
            model.Message = "取code的url:" + url;
            log.Add(model); 
            #endregion
            return Redirect(url);
        }
        /// <summary>
        /// 手机微信支付，统一支付接口
        /// </summary>
        /// <returns></returns>
        public ActionResult WXPay(string id)
        {
            try
            {
                OpResult result = new OpResult();
                result.Successed = false;
                var order = psv.GetOrderByOrderNo(id);//获取订单信息
                ViewBag.orderNo = id;
                //微信预支付数据为单订单发起多次支付
                PrePayInfo info = new PrePayInfo();
                info.OrderNo = order.No;
                info.PrePayNo = FCake.Core.Common.DataHelper.GetSystemID();
                info.NeedPay = int.Parse(order.NeedPay.ToString().Split('.')[0]) * 100;
                info.LogingTime = DateTime.Now;
                bool rt = new PrePayInfoService().AddPayInfo(info);
                if (rt)
                {
                    #region 预支付数据成功创建日志记录
                    PayLog log_prePayData = new PayLog();
                    log_prePayData.ID = FCake.Core.Common.DataHelper.GetSystemID();
                    log_prePayData.LogTime = DateTime.Now;
                    log_prePayData.Message = "创建微信支付id:" + info.PrePayNo + "成功";
                    log.Add(log_prePayData); 
                    #endregion
                }

                if (order.FeeType != FeeType.WXPay && order.Status != 0)
                {
                    result.Message = "订单状态异常!";
                    return Json(result);
                }
                #region 获取tocken
                var appid = WxPayConfig.APPID; //"wx8bf0e66bf9f6a009";
                var secret = WxPayConfig.APPSECRET;// "6b783c1339f64a7638158ccdfaf216f1";
                var code = string.Empty;
                var openid = string.Empty;
                var client = new System.Net.WebClient();
                client.Encoding = System.Text.Encoding.UTF8;
                if (Session["code"] == null)
                {
                    var myurl = Request.Url.Scheme + "://" + Request.Url.Host + "/tenpay/ResponseParam";//获取code的地址
                    var url = string.Format("https://open.weixin.qq.com/connect/oauth2/authorize?appid={0}&redirect_uri={1}&response_type=code&scope=snsapi_userinfo&state=STATE#wechat_redirect", appid, myurl);

                    return Redirect(url);
                }
                else
                {
                    code = Session["code"].ToString();
                    var url = string.Format("https://api.weixin.qq.com/sns/oauth2/access_token?appid={0}&secret={1}&code={2}&grant_type=authorization_code", appid, secret, code);
                    var data = client.DownloadString(url);
                    #region 获取微信access_token结果日志记录
                    //记录返回的access_token
                    PayLog log_accessToken = new PayLog();
                    log_accessToken.ID = FCake.Core.Common.DataHelper.GetSystemID();
                    log_accessToken.LogTime = DateTime.Now;
                    log_accessToken.Message = "accessToken:请求:" + url + "返回:" + data;
                    log.Add(log_accessToken);
                    //token end 
                    #endregion
                    var serializer = new JavaScriptSerializer();
                    var obj = serializer.Deserialize<Dictionary<string, string>>(data);
                    string accessToken;
                    if (!obj.TryGetValue("access_token", out accessToken))
                    {
                        return View();
                    }

                    var opentid = obj["openid"];
                    url = string.Format("https://api.weixin.qq.com/sns/userinfo?access_token={0}&openid={1}&lang=zh_CN", accessToken, opentid);
                    data = client.DownloadString(url);
                    #region 获取微信支付openid结果日志
                    //记录返回的openid
                    PayLog log_openid = new PayLog();
                    log_openid.ID = FCake.Core.Common.DataHelper.GetSystemID();
                    log_openid.LogTime = DateTime.Now;
                    log_openid.Message = "openid:" + data;
                    log.Add(log_openid);
                    //token end 
                    #endregion
                    var userInfo = serializer.Deserialize<Dictionary<string, object>>(data);

                    foreach (var key in userInfo.Keys)
                    {
                        if (key == "openid")
                        {
                            openid = userInfo[key].ToString();
                            break;
                        }

                    }
                }

                //openid get end

                #endregion

                ////若传递了相关参数，则调统一下单接口，获得后续相关接口的入口参数
                JsApiPay jsApiPay = new JsApiPay();
                jsApiPay.hostStr = Request.Url.Host;
                jsApiPay.pathStr = Request.Path;
                jsApiPay.queryStr = Request.Url.Query;
                jsApiPay.codeVal = code;
                jsApiPay.orderNo = info.PrePayNo;//订单号

                jsApiPay.openid = openid;
                jsApiPay.total_fee =Convert.ToInt32((order.NeedPay * 100).ToString().Split('.')[0]); //订单金额，先写死为1   int.Parse((order.NeedPay * 100).ToString());

                //JSAPI支付预处理

                WxPayData unifiedOrderResult = jsApiPay.GetUnifiedOrderResult();

                #region 请求预支付id发送数据日志
                PayLog log_sendPrePayData = new PayLog();
                log_sendPrePayData.ID = FCake.Core.Common.DataHelper.GetSystemID();
                log_sendPrePayData.LogTime = DateTime.Now;
                log_sendPrePayData.Message = "请求预字符id发送数据:" + unifiedOrderResult.ToXml().ToString();
                log.Add(log_sendPrePayData); 
                #endregion


                var wxJsApiParam = jsApiPay.GetJsApiParameters();//获取H5调起JS API参数 

                //记录的Exception
                #region 调jsapi支付参数日志
                PayLog log_sendJsApiData = new PayLog();
                log_sendJsApiData.ID = FCake.Core.Common.DataHelper.GetSystemID();
                log_sendJsApiData.LogTime = DateTime.Now;
                log_sendJsApiData.Message = "pl20:" + wxJsApiParam;
                log.Add(log_sendJsApiData); 
                #endregion
                //Exception end
                //在页面上显示订单信息
                ViewBag.PayInfo = wxJsApiParam;

                if (Session["code"] != null)
                {
                    Session.Remove("code");
                }
                if (Session["orderid"] != null)
                {
                    Session.Remove("orderid");
                }


                return View();
            }
            catch (Exception e)
            {
                #region 异常日志
                //记录的Exception
                PayLog log_Exception = new PayLog();
                log_Exception.ID = FCake.Core.Common.DataHelper.GetSystemID();
                log_Exception.LogTime = DateTime.Now;
                log_Exception.Message = "error:" + e.Message;
                log.Add(log_Exception);
                //Exception end 
                #endregion
            }

            return View();
        } 
      

        /// <summary>
        /// 把XML数据转换为SortedDictionary<string, string>集合
        /// </summary>
        /// <param name="strxml"></param>
        /// <returns></returns>
        protected SortedDictionary<string, string> GetInfoFromXml(string xmlstring)
        {
            SortedDictionary<string, string> sParams = new SortedDictionary<string, string>();
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlstring);
                XmlElement root = doc.DocumentElement;
                int len = root.ChildNodes.Count;
                for (int i = 0; i < len; i++)
                {
                    string name = root.ChildNodes[i].Name;
                    if (!sParams.ContainsKey(name))
                    {
                        sParams.Add(name.Trim(), root.ChildNodes[i].InnerText.Trim());
                    }
                }
            }
            catch { }
            return sParams;
        }
        /// <summary>
        /// 微信的异步支付返回
        /// </summary>
        /// <returns></returns>
        public ActionResult WetChatNotify()
        {
            try
            {
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
                #region 回调异常日志
                PayLog notity99 = new PayLog();
                notity99.ID = FCake.Core.Common.DataHelper.GetSystemID();
                notity99.LogTime = DateTime.Now;
                notity99.Message = "回调发生异常" + e.Message;
                log.Add(notity99);
                return Content("error"); 
                #endregion
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
