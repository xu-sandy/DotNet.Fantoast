using FCake.Bll.Services;
using FCake.Core.MvcCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FCake.WebMobile.Controllers
{
    public class SettingsController :Controller
    {
        //
        // GET: /Settings/
        public JsonResult BusinessManage(string access_token)
        {
            var configService = new SysConfigService();
            var result = configService.ResetSaleConfig(access_token);
            return new JsonNetResult(result.Successed);
        }

    }
}
