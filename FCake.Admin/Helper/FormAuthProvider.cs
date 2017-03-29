using FCake.Admin.Models;
using FCake.Bll;
using FCake.Core.Common;
using FCake.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;

namespace FCake.Admin.Helper
{
    public class FormAuthProvider
    {
        public bool Authenticate(LoginVM model, bool isCookie, string urlHost)
        {
            var account = new AccountService();
            //验证是否有登录权限
            var pwd = model.Password;
            if (isCookie)
            {
                var cookieValue = CookieHelper.GetCookie(Constants.LOGIN_COOKIE_INFO);
                pwd = cookieValue.Split(',')[1];
            }
            else
                pwd = account.GetEncryptPwd(pwd);
            var loginResult = account.CheckLogin(model.UserName, pwd);
            var result = false;
            if (loginResult.Successed)
            {
                //生成当前用户信息
                var login = (UserData)loginResult.Data;
                string userData = string.Format("{0},{1},{2}", login.Id, login.UserName, login.FullName);
                CookieHelper.SaveTicketCookie(Constants.LOGIN_COOKIE_NAME, userData, 0, urlHost);
                UserCache.SetUserSession(login);
                SetLoginCookie(login.UserName, pwd, model.IsRememberMe, urlHost);
                result = true;
            }
            return result;
        }
        /// <summary>
        /// 设置cookie
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="rememberMe"></param>
        /// <param name="urlHost"></param>
        /// <returns></returns>
        private bool SetLoginCookie(string userName, string password, bool rememberMe, string urlHost)
        {
            if (rememberMe)
            {
                var cookiePwd = CookieHelper.GetCookie(Constants.LOGIN_COOKIE_INFO);
                var resetCookie = false;
                if (string.IsNullOrEmpty(cookiePwd))
                    resetCookie = true;
                if (!string.IsNullOrEmpty(cookiePwd))
                {
                    if (cookiePwd.Split(',')[0] != userName)
                    {
                        resetCookie = true;
                    }
                }
                if (resetCookie)
                {
                    var cookieValue = string.Format("{0},{1}", userName, password);
                    CookieHelper.SaveCookie(Constants.LOGIN_COOKIE_INFO, cookieValue, urlHost, 43200, false);
                }
            }
            else
            {
                CookieHelper.SaveCookie(Constants.LOGIN_COOKIE_INFO, "", urlHost, 0, false);
            }
            return true;
        }

        //public bool Authenticate(string username, string password, bool isRmbPwd)
        //{
        //    var account = new AccountService();
        //    //验证是否有登录权限
        //    var pwd = account.GetEncryptPwd(password);
        //    var loginResult = account.CheckLogin(username, pwd);
        //    var result = false;
        //    if (loginResult.Successed)
        //    {
        //        FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(
        //                    1,
        //                    username,
        //                    DateTime.Now,
        //                    DateTime.Now.AddDays(1),
        //                    false, username);
        //        string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
        //        //生成当前用户信息
        //        UserCache.SetUserSession((UserData)loginResult.Data);

        //        //Cookies
        //        HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
        //        HttpContext.Current.Response.Cookies.Add(authCookie);
        //        HttpCookie cookieName = new HttpCookie("uname", username);
        //        cookieName.Expires = DateTime.MaxValue;
        //        HttpContext.Current.Response.Cookies.Add(cookieName);
        //        if (isRmbPwd)
        //        {
        //            HttpCookie cookiePwd = new HttpCookie("upwd", pwd);
        //            cookiePwd.Expires = DateTime.MaxValue;
        //            HttpContext.Current.Response.Cookies.Add(cookiePwd);
        //        }
        //        else
        //        {
        //            HttpCookie cookiePwd = new HttpCookie("upwd", "");
        //            cookiePwd.Expires = DateTime.MinValue;
        //            HttpContext.Current.Response.Cookies.Add(cookiePwd);
        //        }
        //        result = true;
        //    }
        //    return result;
        //}
    }
}