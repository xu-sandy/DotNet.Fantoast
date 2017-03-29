using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.Linq;
using FCake.Core.Common;

namespace FCake.Bll
{
    public class AddressService
    {
        /// <summary>
        /// 获取地址位置
        /// </summary>
        /// <returns></returns>
        public static List<Dictionary<string, string>> GetPositions(string position,string value)
        {
            string path = @"/files/city.xml";
            XElement root = XElement.Load(HttpContext.Current.Server.MapPath(path));

            var result = (from x in root.Descendants(position)
                          where value.IsNullOrTrimEmpty() || x.Attribute("value").Value.Equals(value)
                          select x);

            List<Dictionary<string, string>> dics = new List<Dictionary<string, string>>();
            foreach (var x in result)
            {
                foreach (var y in x.Elements())
                {
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic.Add("t", y.Attribute("value").Value);
                    dics.Add(dic);
                }
            }

            return dics;
        }
    }
}
