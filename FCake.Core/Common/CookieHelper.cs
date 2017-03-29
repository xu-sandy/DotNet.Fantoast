using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using System.Web;

namespace FCake.Core.Common
{
    public class CookieHelper
    {
        public static void ClearTicketCookie(string ticketName, string domain)
        {
            SaveTicketCookie(ticketName, string.Empty, -1, domain);
        }

        public static void SaveTicketCookie(string ticketName, string userName, string userData, int expiresMinutes, bool isPersistent, string domain)
        {
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
            0,
            userName,
            DateTime.Now,
            DateTime.Now.AddYears(1),
            isPersistent,
            userData);
            string cookieStr = FormsAuthentication.Encrypt(ticket);
            HttpCookie cookie = new HttpCookie(ticketName, cookieStr);
            var ticket2 = FormsAuthentication.Decrypt(cookieStr);
            cookie.HttpOnly = true;
            if (expiresMinutes != 0)
                cookie.Expires = DateTime.Now.AddMinutes(expiresMinutes);
            if ((HttpContext.Current.Request.Url.ToString().ToLower().IndexOf("localhost") < 0) && !domain.IsNullOrEmpty())
                cookie.Domain = domain;
            HttpContext.Current.Response.Cookies.Add(cookie);

            var ticket3 = FormsAuthentication.Decrypt(HttpContext.Current.Response.Cookies[ticketName].Value);
        }

        public static void SaveTicketCookie(string ticketName, string userData, int expiresMinutes, string domain)
        {
            FormsAuthentication.SetAuthCookie(userData.Split(',')[0], false);
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
            0,
            ticketName,
            DateTime.Now,
            DateTime.Now.AddYears(1),
            false,
            userData);
            //FormsIdentity identity = new FormsIdentity(ticket);
            string cookieStr = FormsAuthentication.Encrypt(ticket);
            HttpCookie cookie = new HttpCookie(ticketName, cookieStr);
            cookie.HttpOnly = true;
            if (expiresMinutes != 0)
                cookie.Expires = DateTime.Now.AddMinutes(expiresMinutes);
            if ((HttpContext.Current.Request.Url.ToString().ToLower().IndexOf("localhost") < 0) && !domain.IsNullOrEmpty())
                cookie.Domain = domain;
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        public static FormsAuthenticationTicket GetTicketFromCookie(string ticketName)
        {
            if (HttpContext.Current != null && HttpContext.Current.Request.Cookies[ticketName] != null && !HttpContext.Current.Request.Cookies[ticketName].Value.IsNullOrEmpty())
            {
                var ticket = FormsAuthentication.Decrypt(HttpContext.Current.Request.Cookies[ticketName].Value);
                return ticket;
            }

            return null;
        }

        public static string GetUserDataFromCookie(string ticketName)
        {
            if (HttpContext.Current != null && HttpContext.Current.Request.Cookies[ticketName] != null && !HttpContext.Current.Request.Cookies[ticketName].Value.IsNullOrEmpty())
            {
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(HttpContext.Current.Request.Cookies[ticketName].Value);
                return ticket.UserData;
            }

            return string.Empty;
        }

        public static string GetTicketData(string data)
        {
            if (!data.IsNullOrEmpty())
            {
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(data);
                return ticket.UserData;
            }

            return string.Empty;
        }
        public static void SaveCookie(string key, string value, string domain, int expiresMinutes) {
            SaveCookie(key, value, domain, expiresMinutes, true);
        }
        public static void SaveCookie(string key, string value, string domain, int expiresMinutes,bool httpOnly)
        {
            HttpCookie cookie = new HttpCookie(key, value);
            cookie.HttpOnly = httpOnly;
            if ((HttpContext.Current.Request.Url.ToString().ToLower().IndexOf("localhost") < 0) &&
                !domain.IsNullOrEmpty())
            {
                cookie.Domain = domain;
            }
            if (expiresMinutes != 0)
            {
                cookie.Expires = DateTime.Now.AddMinutes(expiresMinutes);
            }
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        public static string GetCookie(string cookieKey)
        {
            if (HttpContext.Current.Request.Cookies[cookieKey] != null && !HttpContext.Current.Request.Cookies[cookieKey].Value.IsNullOrEmpty())
            {
                return HttpContext.Current.Request.Cookies[cookieKey].Value;
            }

            return string.Empty;
        }

        public static void ClearFilterCookies()
        {
            SaveCookie("filter1", string.Empty, string.Empty, -1);
            SaveCookie("filter2", string.Empty, string.Empty, -1);
            SaveCookie("filter6", string.Empty, string.Empty, -1);
            SaveCookie("filter_draff", string.Empty, string.Empty, -1);
            SaveCookie("filter_notarrived", string.Empty, string.Empty, -1);
        }

        /// <summary>
        /// 清除指定Cookie
        /// </summary>
        /// <param name="cookiename">cookiename</param>
        public static void ClearCookie(string cookiename)
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies[cookiename];
            if (cookie != null)
            {
                cookie.Expires = DateTime.Now.AddYears(-3);
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
        }
        /// <summary>
        /// 获取指定Cookie值
        /// </summary>
        /// <param name="cookiename">cookiename</param>
        /// <returns></returns>
        public static string GetCookieValue(string cookiename)
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies[cookiename];
            string str = string.Empty;
            if (cookie != null)
            {
                str = cookie.Value;
            }
            return str;
        }
        /// <summary>
        /// 添加一个Cookie（24小时过期）
        /// </summary>
        /// <param name="cookiename"></param>
        /// <param name="cookievalue"></param>
        public static void SetCookie(string cookiename, string cookievalue)
        {
            SetCookie(cookiename, cookievalue, DateTime.Now.AddDays(1.0));
        }
        /// <summary>
        /// 添加一个Cookie
        /// </summary>
        /// <param name="cookiename">cookie名</param>
        /// <param name="cookievalue">cookie值</param>
        /// <param name="expires">过期时间 DateTime</param>
        public static void SetCookie(string cookiename, string cookievalue, DateTime expires)
        {
            HttpCookie cookie = new HttpCookie(cookiename)
            {
                Value = cookievalue,
                Expires = expires
            };
            HttpContext.Current.Response.Cookies.Add(cookie);
        }
    }
}
