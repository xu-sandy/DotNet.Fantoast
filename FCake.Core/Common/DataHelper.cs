using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace FCake.Core.Common
{
    public class EasyUI_Tree<T>
    {
        public object value { get; set; }
        public List<EasyUI_Tree<T>> children { get; set; }
        public Dictionary<string, object> attributes { get; set; }
    }
    public static class DataHelper
    {

        #region 正则类
        public static bool IsMobile(this string source)
        {
            Regex reg = new Regex(@"^1[3-8|][0-9]{9}&");
            return reg.IsMatch(source);
        }
        #endregion

        /// <summary>
        /// 获取完整类名及程序集名
        /// </summary>
        /// <param name="souce"></param>
        /// <returns></returns>
        public static string GetTypeName<T>()
        {
            var t = typeof(T);
            return t.FullName + "," + t.Assembly.FullName;
        }
        /// <summary>
        /// 判断是否为空
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsNullOrTrimEmpty(this string source)
        {
            return source == null || source.Trim() == "";
        }
        /// <summary>
        /// 返回GUID去横杆字符串大写
        /// </summary>
        /// <returns></returns>
        public static string GetSystemID()
        {
            var result = Guid.NewGuid().ToString("N");
            return result.ToUpper();
        }
        /// <summary>
        /// 对于两个相同类的属性进行复制
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="source"></param>
        public static void CopyProperty<T>(this T target, T source)
        {
            var flags = BindingFlags.Public | BindingFlags.Instance;
            Func<PropertyInfo, bool> filter = p => p.CanRead && p.CanWrite;
            var type = source.GetType();

            var sourceProperties = type.GetProperties(flags).Where(filter);
            var targetProperties = type.GetProperties(flags).Where(filter);

            foreach (var property in targetProperties)
            {
                var s = sourceProperties.SingleOrDefault(p => p.Name.Equals(property.Name)
                         && property.DeclaringType.IsAssignableFrom(p.DeclaringType));
                if (s != null)
                {
                    property.SetValue(target, s.GetValue(source, null), null);
                }
            }
        }
        /// <summary>
        /// 复制属性
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dic"></param>
        public static void CopyProperty(this object source, Dictionary<string, object> dic)
        {
            var t = source.GetType();
            foreach (var x in t.GetProperties())
            {
                if (dic.Keys.Contains(x.Name))
                {
                    try
                    {
                        var value = dic[x.Name];
                        var type = x.PropertyType;
                        if (value == null)
                        {
                            x.SetValue(source, null, null);
                            continue;
                        }

                        if (type == typeof(int?) || type == typeof(int))
                        {
                            x.SetValue(source, int.Parse(value.ToString()), null);
                            continue;
                        }
                        if (type == typeof(double?) || type == typeof(double))
                        {
                            x.SetValue(source, double.Parse(value.ToString()), null);
                            continue;
                        }
                        if (type == typeof(decimal?) || type == typeof(decimal))
                        {
                            x.SetValue(source, decimal.Parse(value.ToString()), null);
                            continue;
                        }
                        if (type == typeof(DateTime?) || type == typeof(DateTime))
                        {
                            x.SetValue(source, Convert.ToDateTime(value.ToString()), null);
                            continue;
                        }
                        if (type.IsEnum)
                        {
                            x.SetValue(source,int.Parse(value.ToString()), null);
                            continue;
                        }

                        x.SetValue(source, Convert.ChangeType(dic[x.Name], type), null);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
        }
        /// <summary>
        /// 将表单转为字典型
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Dictionary<string, object> ToDic(this FormCollection source)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            foreach (var x in source.AllKeys)
            {
                dic.Add(x, source[x]);
            }
            return dic;
        }
        /// <summary>
        /// 将list转化为字典
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="changes"></param>
        /// <returns></returns>
        public static List<Dictionary<string, object>> ToDic<T>(List<T> source, params string[] changes)
        {
            List<Dictionary<string, object>> dics = new List<Dictionary<string, object>>();
            Type type = null;
            if (source.Any())
                type = source.First().GetType();
            else
                type = typeof(T);
            foreach (var temp in source)
            {
                Dictionary<string, object> dic = new Dictionary<string, object>();
                foreach (var x in type.GetProperties())
                {
                    if (changes.Contains(x.Name, StringComparer.OrdinalIgnoreCase))
                    {
                        foreach (var y in x.PropertyType.GetProperties())
                        {
                            if (dic.Keys.Contains(y.Name, StringComparer.OrdinalIgnoreCase) == false)
                            {
                                dic.Add(y.Name, y.GetValue(x.GetValue(temp, null), null));
                            }
                        }
                    }
                    else
                    {
                        if (dic.Keys.Contains(x.Name, StringComparer.OrdinalIgnoreCase) == false)
                        {
                            dic.Add(x.Name, x.GetValue(temp, null));
                        }
                    }
                }
                dics.Add(dic);
            }
            return dics;
        }
        /// <summary>
        /// 获取属性列表
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<string> GetPropertyList(this Type type)
        {
            var flags = BindingFlags.Public | BindingFlags.Instance;
            var ps = type.GetProperties(flags);

            List<string> result = new List<string>();

            foreach (var x in ps)
            {
                //if (typeof(int).IsAssignableFrom(x.PropertyType) || typeof(string).IsAssignableFrom(x.PropertyType))
                result.Add(x.Name);
            }

            return result;
        }

        public static List<Dictionary<string, object>> ToTreeDic<T>(this List<EasyUI_Tree<T>> source)
        {
            List<Dictionary<string, object>> dic = new List<Dictionary<string, object>>();
            foreach (var x in source)
            {
                dic.Add(x.ToTreeDic<T>());
            }
            return dic;
        }

        public static Dictionary<string, object> ToTreeDic<T>(this EasyUI_Tree<T> souce)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            var ps = typeof(T).GetProperties();
            foreach (var x in ps)
            {
                var value = x.GetValue(souce.value, null);
                dic.Add(x.Name, value == null ? "" : value);
            }
            List<Dictionary<string, object>> children = new List<Dictionary<string, object>>();
            foreach (var x in souce.children)
            {
                children.Add(x.ToTreeDic<T>());
            }
            dic.Add("children", children);
            dic.Add("attributes", souce.attributes);
            return dic;
        }






    }
}
