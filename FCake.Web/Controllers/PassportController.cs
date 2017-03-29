using FCake.API.EChi;
using FCake.Bll;
using FCake.Bll.Services;
using FCake.Core.Common;
using FCake.Core.MvcCommon;
using FCake.Domain.Entities;
using FCake.Domain.WebModels;
using FCake.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using FCake.Bll.MemberAuth;
using FCake.API;

namespace FCake.Web.Controllers
{
    public class PassportController : BaseController
    {
        const string CAPTCHAS_TEXT = "CAPTCHASTEXT";
        private readonly CartService _cartService = new CartService();
        //
        // GET: /Passport/

        public ActionResult Index()
        {
            return View();
        }
        #region 注册
        public ActionResult Register()
        {
            return View();
        }
        public void GetCaptchas()
        {
            var svc = new PassportService();
            Random rdm = new Random();
            var result = svc.GetCaptchas();
            Session[CAPTCHAS_TEXT] = result.Text;
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            result.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            Response.ClearContent();
            Response.ContentType = "image/Gif";
            Response.BinaryWrite(ms.ToArray());
        }
        [HttpPost]
        public JsonResult Register(RegisterUser user)
        {
            //if (Session[CAPTCHAS_TEXT] != null)
            //{
            //    if (user.Captchas != Session[CAPTCHAS_TEXT].ToString())
            //        return new JsonNetResult(OpResult.Fail("验证码错误", code: "captchas"));
            //}
            if (new PhoneCodeService().CheckMobileCode(user.UserName, user.MsgVerifyCode) == false)
            {
                return new JsonNetResult(OpResult.Fail("短信验证码错误", code: "msgVerifyCode"));
            }
            var svc = new PassportService();
            var result = svc.Register(user);
            if (result.Successed)
            {
                //注册成功 直接登录
                FormAuthProvider formAuth = new FormAuthProvider();
                formAuth.SetLogin((Customers)result.Data, false, Request.Url.Host);
                 CurrentMember.IsLogin = true;
                //清空cookie中原先要提交到订单列表的cartId,保证点击立即购买->结算再登录跳转至购物车页面
                 CookieHelper.SetCookie("cartids", "");
                //合并购物车
                _cartService.MergeCart();
                return new JsonNetResult(OpResult.Success(message: "注册成功", data: Url.Action("Index", "Home")));
            }
            else
                return new JsonNetResult(OpResult.Fail(result.Message, code: "error"));
        }
        [HttpPost]
        public ActionResult CheckCaptchas(string captchas)
        {
            bool result = false;
            if (Session[CAPTCHAS_TEXT] != null)
            {
                if (captchas == Session[CAPTCHAS_TEXT].ToString())
                result = true;
            }
            return Json(result);
        }
        [HttpPost]
        public JsonResult CheckUserName()
        {
            var result = false;
            var queryStr = Request.QueryString["userName"];
            var queryString = Request.QueryString["Mobile"];
            if (queryStr != null)
            {
                var svc = new PassportService();
                result = svc.CheckUserName(queryStr.ToString());
            }
            if (queryString != null)
            {
                var svc = new PassportService();
                result = svc.CheckUserName(queryString.ToString());
            }
            return new JsonNetResult(result);
        }
        #endregion

        #region 登录
        public ActionResult Login(string returnUrl)
        {
            //判断是否自动登录
            FormAuthProvider authProvider = new FormAuthProvider();
            if (authProvider.AutoLogin(Request.Url.Host))
            {
                CurrentMember.IsLogin = true;
                //清空cookie中原先要提交到订单列表的cartId,保证点击立即购买->结算再登录跳转至购物车页面
                CookieHelper.SetCookie("cartids", "");
                //合并购物车
                _cartService.MergeCart();
                return Redirect(returnUrl ?? Url.Action("Index", "Home"));

            }
            else
                return View();
        }

        [HttpPost]
        public ActionResult Login(LoginUser model, string returnUrl, bool hidden1)
        {
            FormAuthProvider authProvider = new FormAuthProvider();
            if (ModelState.IsValid)
            {
                var authResult = authProvider.Authenticate(model, Request.Url.Host);
                if (authResult.Successed)
                {
                    CurrentMember.IsLogin = true;
                    //清空cookie中原先要提交到订单列表的cartId,保证点击立即购买->结算再登录跳转至购物车页面
                    CookieHelper.SetCookie("cartids", "");
                    //合并购物车
                    _cartService.MergeCart();
                    return Redirect(returnUrl ?? Url.Action("Index", "Home"));
                }
                else
                {
                    ModelState.AddModelError("", authResult.Message);
                    return View(model);
                }
            }
            else
            {
                return View(model);
            }
        }
        #endregion

        #region 忘记密码
        public ActionResult Forget()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Forget(string mobile, string code, string newPwd, string confirmPwd)
        {
            var opResult = new PassportService().ResetPassword(mobile, code, newPwd);
            return Json(new { success = opResult.Successed, msg = opResult.Message });
        }
        [HttpPost]
        public ActionResult SendValidCodeFJXX(string mobile, string type, bool isNewRegister = false,string guess="")
        {
            string IP = (Request.ServerVariables["HTTP_VIA"] != null) ? Request.ServerVariables["HTTP_X_FORWARDED_FOR"].ToString() : Request.ServerVariables["REMOTE_ADDR"].ToString();
            if (Session[CAPTCHAS_TEXT] == null || string.IsNullOrEmpty(guess) || guess.ToLower() != Session[CAPTCHAS_TEXT].ToString().ToLower())
            {
                SysLogService.SaveSysLog("AliDaYu", IP, string.Format("SessionID：{0}，手机号码：{1}，验证码：{2}", Session.SessionID, mobile, guess), "发送短信验证码，图片验证码输入错误");
                return Json(new { msg = "图片验证码输入错误", success = false });
            }
            mobile = mobile.Trim();
            if (mobile.IsNullOrTrimEmpty())
                return Json(new { msg = "手机不能为空", success = false });
            if (new Regex(@"^1[3|5|7|8|][0-9]{9}$").IsMatch(mobile) == false)
            {
                return Json(new { msg = "手机格式不正确", success = false });
            }

            var pcsv = new PhoneCodeService();
            var codeRecord = pcsv.GetPhoneCodeByMobile(mobile);
            if (codeRecord != null && (DateTime.Now - codeRecord.SendTime).TotalMinutes < 1)
            {
                return Json(new { msg = "短信已发送，发送时间为：" + codeRecord.SendTime.ToString("yyyy-MM-dd HH:mm:ss") + "，一分钟后重发", success = false });
            }

            var code = new Random().Next(100000, 999999);
            var isUsed = pcsv.IsPhoneUsed(mobile);
            if (isUsed || isNewRegister)
            { //当情况为注册过的改密码 或 新注册时
                pcsv.CreatePhoneCode(mobile, code.ToString(), "UnKnown");
                //var data = new FCake.Bll.Services.MsgTemplateService().GetMsgTempByCategory(type);
                //var result = EChiHelper.SendSMSResult(mobile, string.Format(data, code), FormatType.MobileCheckCode);
                var sendErrorMsg = string.Empty;

                SysLogService.SaveSysLog("AliDaYu",IP,string.Format("SessionID：{0}，手机号码：{1}，模板ID：{2}",Session.SessionID,mobile,DaYuConfig.SMSCodeCommonTemplate),"调用阿里大鱼发送手机验证码接口");

                var result = DaYuSMSHelper.SendSMSCode(mobile, code.ToString(), out sendErrorMsg);
                if (result)
                {
                    return Json(new { msg = "短信发送成功，一小时内有效", success = true });
                }
                else
                {
                    SysLogService.SaveAliSMSErrorLog(sendErrorMsg,mobile,DaYuConfig.SMSCodeCommonTemplate);
                }
            }
            else
            {
                return Json(new { msg = "该手机号未绑定账户，请确认您的手机号", success = false });
            }
            return Json(new { msg = "短信发送失败", success = false });
        }

        [HttpPost]
        public JsonResult CheckCode()
        {
            var mobile = Request.QueryString["mobile"] ?? Request.QueryString["UserName"];
            var code = Request.QueryString["code"] ?? Request.QueryString["MsgVerifyCode"];
            var result = new PhoneCodeService().CheckMobileCode(mobile, code);
            return new JsonNetResult(result);
        }

        #endregion

        #region 退出
        [Authorize]
        /// <summary>
        /// 注销
        /// </summary>
        /// <returns></returns>
        public ActionResult LogOut()
        {
            FormAuthProvider authProvider = new FormAuthProvider();
            authProvider.Logout();
            return RedirectToAction("Index", "Home");
        }

        #endregion
    }
}
