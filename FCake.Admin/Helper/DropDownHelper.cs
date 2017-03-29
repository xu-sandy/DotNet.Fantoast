using FCake.Bll;
using FCake.Core.Common;
using FCake.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FCake.Admin.Helper
{
    public class DropDownHelper
    {
        static CommonService svc = new CommonService();
        /// <summary>
        /// 从新的字典表里查下拉数据
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static List<DropdownItem> GetDictionaryByCode(string code, bool isall = false)
        {
            var list = svc.GetDictionaryData(code);
            var result = new List<DropdownItem>();
            if (isall)
                result.Add(new DropdownItem() { Value = "", Text = "全部" });
            if (list.Count() > 0)
            {
                result.AddRange(from tb in list select new DropdownItem() { Value = tb.Value, Text = tb.Name });
            }
            return result;
        }
        public static List<DropdownItem> GetCakeFromProduct()
        {
            ProductService ps = new ProductService();
            var list = ps.GetAllProducts();
            var result = new List<DropdownItem>();
            if (list.Count() > 0)
            {
                result.AddRange(from tb in list select new DropdownItem() { Value = tb.Id, Text = tb.Name });
            }
            return result;
        }
    }
}