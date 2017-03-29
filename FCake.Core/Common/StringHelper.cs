using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FCake.Core.Common
{
    public class StringHelper
    {
        public static string IsNEAndTrim(string text) {
            if (string.IsNullOrEmpty(text))
                text = "";
            else
                text = text.Trim();
            return text;
        }

        public static string ConvertStr(object value)
        {
            return value == null ? "" : value.ToString();
        }
    }
}
