using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Configuration;
using System.Collections;
using System.IO;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Text.RegularExpressions;
using System.Collections.Specialized;

namespace FCake.Core.Common
{
    public static class ParameterHelper
    {
        public static bool ToBoolean(this object obj)
        {
            bool b = false;
            bool.TryParse(Convert.ToString(obj), out b);
            return b;
        }

        public static long? ToInt64OrNull(this object obj)
        {
            long r;
            if (long.TryParse(Convert.ToString(obj), out r))
                return r;
            else
                return null;
        }

        public static int ToInt32(this object obj)
        {
            int r;
            if (int.TryParse(Convert.ToString(obj), out r))
                return r;
            else
                return 0;
        }

        public static int? ToInt32OrNull(this object obj)
        {
            int r;
            if (int.TryParse(Convert.ToString(obj), out r))
            {
                return r;
            }
            else
            {
                return null;
            }
        }

        public static decimal? ToDecimalOrNull(this object obj)
        {
            decimal r;
            if (decimal.TryParse(Convert.ToString(obj), out r))
            {
                return r;
            }
            else
            {
                return null;
            }
        }

        public static DateTime? ToDateTimeOrNull(this object obj)
        {
            DateTime dt;
            if (DateTime.TryParse(Convert.ToString(obj), out dt))
                return dt;
            else
                return null;
        }

        public static string GetStringOrNull(this NameValueCollection nvc, string name)
        {
            return nvc[name];
        }

        public static string GetString(this NameValueCollection nvc, string name)
        {
            return GetStringOrNull(nvc, name) ?? string.Empty;
        }

        public static int? GetIntOrNull(this NameValueCollection nvc, string name)
        {
            int value = 0;
            if (int.TryParse(nvc[name], out value))
                return value;
            return null;
        }

        public static long? GetInt64OrNull(this NameValueCollection nvc, string name)
        {
            long value = 0;
            if (long.TryParse(nvc[name], out value))
                return value;
            return null;
        }

        public static int GetInt(this NameValueCollection nvc, string name)
        {
            return GetIntOrNull(nvc, name).GetValueOrDefault();
        }

        public static short? GetShortOrNull(this NameValueCollection nvc, string name)
        {
            short value = 0;
            if (short.TryParse(nvc[name], out value))
                return value;
            return null;
        }

        public static short GetShort(this NameValueCollection nvc, string name)
        {
            return GetShortOrNull(nvc, name).GetValueOrDefault();
        }

        public static decimal? GetDecimalOrNull(this NameValueCollection nvc, string name)
        {
            decimal value = 0;
            if (decimal.TryParse(nvc[name], out value))
                return value;
            return null;
        }

        public static decimal GetDecimal(this NameValueCollection nvc, string name)
        {
            return GetDecimalOrNull(nvc, name).GetValueOrDefault();
        }

        public static DateTime? GetDateOrNull(this NameValueCollection nvc, string name)
        {
            DateTime value = DateTime.MinValue;
            if (DateTime.TryParse(nvc[name], out value))
                return value;

            return null;
        }

        public static bool IsObjNullOrEmpty(this object obj)
        {
            string rst = string.Empty;
            if (obj == null)
                return true;
            else
                rst = obj.ToString();
            return string.IsNullOrEmpty(rst);
        }

        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static bool IsNotNullOrEmpty(this object obj)
        {
            if (obj == null)
                return false;
            if (string.IsNullOrEmpty(obj.ToString()))
                return false;
            return true;
        }

        public static string ToNormalString(this object obj)
        {
            if (obj == null)
                return string.Empty;
            else
                return obj.ToString();
        }

        public static DateTime GetDate(this NameValueCollection nvc, string name)
        {
            return GetDateOrNull(nvc, name).GetValueOrDefault();
        }

        public static T? GetEnumOrNull<T>(this NameValueCollection nvc, string name)
            where T : struct
        {
            var val = GetStringOrNull(nvc, name);
            var enumName = nvc[name];
            if (!string.IsNullOrWhiteSpace(val))
            {
                return (T)Enum.Parse(typeof(T), val, true);
            }
            else
            {
                return null;
            }
        }

        public static T GetEnum<T>(this NameValueCollection nvc, string name)
            where T : struct
        {
            var val = GetInt(nvc, name);

            var enumName = nvc[name];
            if (enumName == null || enumName.ToString() == string.Empty)
                enumName = Enum.GetName(typeof(T), val);
            if (enumName != null && (enumName.ToString() != string.Empty))
                return (T)Enum.Parse(typeof(T), enumName, true);
            else
                return default(T);
        }

        public static T GetEnumByName<T>(this NameValueCollection nvc, string name)
            where T : struct
        {
            var val = GetString(nvc, name);
            Array values = System.Enum.GetValues(typeof(T));
            foreach (object value in values)
            {
                if (value.ToString().ToUpper().Equals(val.ToUpper()))
                    return (T)value;
            }

            val = "0";
            return (T)Enum.Parse(typeof(T), val, true);
        }

        public static bool? GetBooleanOrNull(this NameValueCollection nvc, string name)
        {
            var val = GetStringOrNull(nvc, name);
            if (val != null)
            {
                var rst = false;
                bool.TryParse(val, out rst);

                return rst;
            }

            return null;
        }

        public static bool GetBoolean(this NameValueCollection nvc, string name)
        {
            return GetBooleanOrNull(nvc, name).GetValueOrDefault();
        }

        public static NameValueCollection RemoveItem(this NameValueCollection nvc, string name)
        {
            var newNvc = new NameValueCollection(nvc);
            newNvc.Remove(name);

            return newNvc;
        }

        public static NameValueCollection Revert(this NameValueCollection nvc)
        {
            var newNvc = new NameValueCollection();
            for (int i = 0; i < nvc.Count; i++)
            {
                var key = nvc.Keys[i];
                var val = nvc[i];

                newNvc[val] = key;
            }

            return newNvc;
        }
    }
}
