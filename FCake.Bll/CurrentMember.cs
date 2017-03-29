using FCake.Bll.MemberAuth;
using FCake.Domain.Entities;
using FCake.Domain.WebModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.SessionState;
using FCake.Core.Common;

namespace FCake.Bll
{
    public class CurrentMember
    {
        static HttpSessionState Session { get { return HttpContext.Current.Session; } }

        public static string MemberId
        {
            get
            {
                if (MemberSession == null)
                    return string.Empty;
                else
                return MemberSession.MemberId;
            }
        }
        public static string DisplayName
        {
            get
            {
                if (MemberSession == null)
                    return string.Empty;
                else
                    return MemberSession.DisplayName;
            }
        }

        public static string CookieCartId
        {
            get{
                return CookieHelper.GetCookieValue(Constants.UNIQUE_VISITOR_ID);
            }
        }
        public static bool IsAuthenticated {
            get { return HttpContext.Current.User.Identity.IsAuthenticated; }
        }
        public static bool IsLogin {
            set {
                Session[Constants.LOGIN_AUTOSESSION_ID] = value;
            }
            get {
                var isAuth = HttpContext.Current.User.Identity.IsAuthenticated;
                if (isAuth)
                    return isAuth;
                else
                {
                    var isAutoLogin = Session[Constants.LOGIN_AUTOSESSION_ID];
                    if (isAutoLogin != null)
                        return (bool)isAutoLogin;
                    else
                        return false;
                }
            }
        }
        public static bool HasAutoLogin {
            set {
                Session[Constants.LOGIN_HASAUTOSESSION_ID] = value;
            }
            get
            {
                var hasAuto = Session[Constants.LOGIN_HASAUTOSESSION_ID];
                if (hasAuto != null)
                    return (bool)hasAuto;
                else
                    return false;
            }
        }

        public static MemberSession MemberSession
        {
            get
            {
                MemberSession member = null;
                if (HttpContext.Current.User.Identity.IsAuthenticated ||IsLogin)
                {
                    if (Session != null && Session[Constants.LOGIN_SESSION_ID] != null)
                    {
                    }
                    else
                    {
                        var memberInfo = CookieHelper.GetUserDataFromCookie(Constants.LOGIN_COOKIE_NAME);
                        if (!string.IsNullOrEmpty(memberInfo))
                        {
                            ReSetMemberSession(memberInfo.Split(',')[0]);
                        }
                    }
                    member = (MemberSession)Session[Constants.LOGIN_SESSION_ID];
                }
                else
                {
                    Session[Constants.LOGIN_SESSION_ID] = null;
                }
                return member;
            }
        }
        public static void ReSetMemberSession(string Id)
        {
            var memberModel = (new PassportService()).GetUser(Id);
            if (memberModel != null)
            {
                SetMemberSession(memberModel);
            }
        }
        public static void SetMemberSession(Customers member)
        {
            var session = new MemberSession(member.Id, member.Mobile, member.FullName);
            Session[Constants.LOGIN_SESSION_ID] = session;
        }
    }
}
