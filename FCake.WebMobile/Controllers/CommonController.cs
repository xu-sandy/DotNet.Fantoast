using FCake.Bll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FCake.WebMobile.Controllers
{
    public class CommonController : Controller
    {
        /// <summary>
        /// 获取下拉地址
        /// </summary>
        /// <param name="position">位置</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetPosition(string position,string value)
        {
            var result = AddressService.GetPositions(position, value);
            return Json(result);
        }
    }
}
