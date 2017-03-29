using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FCake.Bll.MemberAuth
{
    public class Constants
    {
        public const string LOGIN_COOKIE_NAME = "FANCAKEIDENTITY";
        public const string LOGIN_COOKIE_INFO = "FANCAKEIDENTITYINFO";
        public const string UNIQUE_VISITOR_ID = "FANCAKEVISITORID";

        public const string LOGIN_SESSION_ID = "FANCAKECURRENTUSER";
        public const string LOGIN_AUTOSESSION_ID = "LOGINAUTOSESSIONID";
        public const string LOGIN_HASAUTOSESSION_ID = "LOGINHASAUTOSESSIONID";
    }
}