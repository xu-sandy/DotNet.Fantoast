using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FCake.Admin.Models;
using FCake.Framework;
using FCake.Domain;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using WH= WebHelper;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using FCake.Admin.Helper;

namespace FCake.Admin.Controllers
{
    [UserAuth]
    [CheckPermission(IsBase=true)]
    public class BaseController : Controller
    {
        public QueryInfo GetQueryInfo()
        {

            var queryData = WH.Web.UI.BindingPanel.SaveData<System.Collections.Hashtable>(Request.Form, 0);
            var info = new QueryInfo();
            info.AddParam(queryData);
            return info;
        }


        /// <summary>
        /// 自定义Json视图
        /// </summary>
        public class CustomJsonResult : JsonResult
        {
            public CustomJsonResult()
            {
                Settings = new JsonSerializerSettings
                {
                    //解决.Net MVC EntityFramework Json 序列化循环引用问题.
                    //这句是解决问题的关键,也就是json.net官方给出的解决配置选项.                 
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };
            }
            public JsonSerializerSettings Settings { get; private set; }
            /// <summary>
            /// 格式化字符串
            /// </summary>
            public string FormateStr
            {
                get;
                set;
            }

            /// <summary>
            /// 重写执行视图
            /// </summary>
            /// <param name="context">上下文</param>
            public override void ExecuteResult(ControllerContext context)
            {
                if (context == null)
                    throw new ArgumentNullException("context");
                if (this.JsonRequestBehavior == JsonRequestBehavior.DenyGet && string.Equals(context.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("JSON GET is not allowed");
                HttpResponseBase response = context.HttpContext.Response;
                response.ContentType = string.IsNullOrEmpty(this.ContentType) ? "application/json" : this.ContentType;
                if (this.ContentEncoding != null)
                    response.ContentEncoding = this.ContentEncoding;
                if (this.Data == null)
                    return;
                var timeConverter = new IsoDateTimeConverter();
                timeConverter.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
                Settings.Converters.Add(timeConverter);
                var scriptSerializer = JsonSerializer.Create(this.Settings);
                using (JsonTextWriter writer = new JsonTextWriter(response.Output))
                {
                    scriptSerializer.Serialize(writer, this.Data);
                    writer.Flush();
                }
            }

            /// <summary>  
            /// 将Json序列化的时间由/Date(1294499956278)转为字符串 .
            /// </summary>  
            /// <param name="m">正则匹配</param>
            /// <returns>格式化后的字符串</returns>
            private string ConvertJsonDateToDateString(Match m)
            {
                string result = string.Empty;
                DateTime dt = new DateTime(1970, 1, 1);
                dt = dt.AddMilliseconds(long.Parse(m.Groups[1].Value));
                dt = dt.ToLocalTime();
                result = dt.ToString(FormateStr);
                return result;
            }
        }

        protected override JsonResult Json(object data, string contentType, System.Text.Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            return new CustomJsonResult
            {
                Data = data,
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                JsonRequestBehavior = behavior,
                FormateStr = "yyyy-MM-dd HH:mm:ss"
            };
        }


        #region 基础
        /// <summary>
        /// 判断是否是客服人员角色
        /// </summary>
        /// <returns></returns>
        public bool IsCustomerServiceRole()
        {
            return (UserCache.CurrentUser!=null && UserCache.CurrentUser.Roles!=null && UserCache.CurrentUser.Roles.Count>0 
                && UserCache.CurrentUser.Roles.Any(a => a.RoleCode.Equals(Constants.ROLE_CUSTOMERSERVICE)));
        }
        #endregion
    }
}
