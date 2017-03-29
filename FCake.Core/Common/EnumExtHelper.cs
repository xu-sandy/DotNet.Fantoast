using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;

namespace FCake.Core.Common
{
    /// <summary>
    /// 这是Enum枚举的拓展类
    /// </summary>
    public static class EnumExtHelper
    {
        public static Type AttributeType = typeof(DescriptionAttribute);

        /// <summary>
        /// 将枚举的描述文件以字符串方式输出
        /// </summary>
        /// <param name="subenum"></param>
        /// <returns></returns>
        public static string GetDescription(this System.Enum subenum)
        {
            string description = subenum.ToString();
            FieldInfo fieldInfo = subenum.GetType().GetField(subenum.ToString());
            object[] objs = fieldInfo.GetCustomAttributes(AttributeType, false);
            if (objs != null && objs.Length != 0)
            {
                DescriptionAttribute da = (DescriptionAttribute)objs[0];
                description = da.Description;
            }
            return description;
        }
    }
}
