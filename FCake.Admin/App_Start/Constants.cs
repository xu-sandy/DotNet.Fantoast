using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FCake.Admin
{
    public class Constants
    {
        #region cookie
        public const string LOGIN_COOKIE_NAME = "FCAKEADMINIDENTITY";
        public const string LOGIN_COOKIE_INFO = "FCAKEADMINIDENTITYINFO";

        public const string LOGIN_SESSION_ID = "FANCAKEADMINCURRENTUSER";
        #endregion

        #region RoleCode
        public const string ROLE_CUSTOMERSERVICE = "customerservice";
        public const string ROLE_DISTRIBUTION = "distribution";
        #endregion
    }
}