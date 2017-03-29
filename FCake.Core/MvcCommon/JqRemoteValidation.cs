using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace FCake.Core.MvcCommon
{
    public static class JqRemoteValidation
    {
        private static ContentResult _validResult = null;

        static JqRemoteValidation()
        {
            _validResult = new ContentResult()
            {
                Content = "\"true\""
            };
        }

        public static ContentResult Valid()
        {
            return _validResult;
        }

        public static ContentResult Invalid(string message)
        {
            ContentResult _invalidResult = new ContentResult()
            {
                Content = "\"" + message + "\""
            };

            return _invalidResult;
        }
    }
}
