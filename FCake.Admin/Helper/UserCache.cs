using FCake.Bll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FCake.Admin.Helper
{
    public class UserCache
    {
        private static FCake.Domain.Common.UserData _currentUser = null;

        public static FCake.Domain.Common.UserData CurrentUser
        {
            get
            {
                _currentUser = null;
                if (HttpContext.Current.Session != null && HttpContext.Current.Session[Constants.LOGIN_SESSION_ID] != null)
                {
                    _currentUser = (FCake.Domain.Common.UserData)HttpContext.Current.Session[Constants.LOGIN_SESSION_ID];
                }
                else
                {
                    var userId = FCake.Core.Common.CookieHelper.GetUserDataFromCookie(Constants.LOGIN_COOKIE_NAME);
                    if (!string.IsNullOrEmpty(userId))
                    {
                        var accountSvc = new AccountService();
                        var user = (new UserService()).GetUser(userId.Split(',')[0]);
                        if (user != null)
                        {
                            _currentUser = accountSvc.GetUserData(user);
                            SetUserSession(_currentUser);
                        }
                    }
                }
                return _currentUser;
            }
        }

        public static void SetUserSession(FCake.Domain.Common.UserData userData)
        {
            HttpContext.Current.Session[Constants.LOGIN_SESSION_ID] = userData;
        }
    }
}