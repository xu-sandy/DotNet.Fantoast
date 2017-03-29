using FCake.Core.Common;
using FCake.Core.MvcCommon;
using FCake.Domain.Entities;
using FCake.Domain.WebModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;

namespace FCake.Bll.MemberAuth
{
    public class FormAuthProvider
    {
        string LoginCookie
        {
            get { return CookieHelper.GetCookie(Constants.LOGIN_COOKIE_INFO); }
        }
        /// <summary>
        /// 登录验证
        /// </summary>
        /// <param name="model"></param>
        /// <param name="urlHost"></param>
        /// <returns></returns>
        public OpResult Authenticate(LoginUser model, string urlHost)
        {
            if (string.IsNullOrEmpty(model.UserName) || string.IsNullOrEmpty(model.Password))
                return OpResult.Fail();

            //验证是否有登录权限
            var svc = new PassportService();
            var loginResult = svc.CheckLogin(model.UserName, model.Password);
            var result = false;
            if (loginResult.Successed)
            {
                result = true;
                var member = (Customers)loginResult.Data;
                result = SetLogin(member, model.IsRememberMe, urlHost);
            }
            else
            {
                return OpResult.Fail(loginResult.Message);
            }

            if (result)
                return OpResult.Success();
            else
                return OpResult.Fail();
        }
        /// <summary>
        /// 自动登录验证
        /// </summary>
        /// <param name="urlHost"></param>
        /// <returns></returns>
        public bool AutoLogin(string urlHost)
        {
            if (!CurrentMember.IsAuthenticated)
            {//todo:cookie 过期的判断
                var cookieVal = LoginCookie;
                if (!string.IsNullOrEmpty(cookieVal))
                {
                    var lgId = cookieVal.Split(',')[0];
                    var stamp = cookieVal.Split(',')[2];
                    long? changePasswordTimeStamp = null;
                    if (!string.IsNullOrEmpty(stamp))
                        changePasswordTimeStamp = long.Parse(stamp);
                    var svc = new PassportService();
                    var member = svc.GetUser(lgId);
                    if (member != null)
                    {
                        if (member.ChangePasswordTimeStamp != changePasswordTimeStamp)
                        {
                            CurrentMember.HasAutoLogin = false;
                            return false;
                        }
                        if (member.IsDisabled == 1)
                        {
                            CurrentMember.HasAutoLogin = false;
                            return false;
                        }
                        SetLogin(member, false, urlHost);
                        CurrentMember.HasAutoLogin = true;
                    }
                    return true;
                }
                return false;
            }
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        public void Logout()
        {
            //清除Cookie
            SetLoginCookie("", 0, "");
            //清除Session
            RemoveSession();
            FormsAuthentication.SignOut();
        }
        public bool SetLogin(Customers member, bool isRememberMe, string urlHost)
        {
            CurrentMember.SetMemberSession(member);
            string userData = string.Format("{0},{1},{2},{3}", member.Id, member.Mobile, member.FullName, member.ChangePasswordTimeStamp);
            CookieHelper.SaveTicketCookie(Constants.LOGIN_COOKIE_NAME, userData, 0, urlHost);
            if (isRememberMe)
            {
                var cookieValue = string.Format("{0},{1},{2}", member.Id, member.Mobile, member.ChangePasswordTimeStamp);
                SetLoginCookie(cookieValue, 43200, urlHost);
            }
            return true;
        }

        private bool SetLoginCookie(string cookieVal, int expiresMinutes, string urlHost)
        {
            CookieHelper.SaveCookie(Constants.LOGIN_COOKIE_INFO, cookieVal, urlHost, expiresMinutes, false);
            return true;
        }
        private void RemoveSession()
        {
            CurrentMember.HasAutoLogin = false;
            CurrentMember.IsLogin = false;
        }
    }
}
