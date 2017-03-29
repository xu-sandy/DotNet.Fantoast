using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FCake.Web.Models;
using FCake.Framework;
using FCake.Domain;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace FCake.Web.Controllers
{

    [DeviceFilter]
    public class BaseController : Controller
    {
        public EFDbContext context = new EFDbContext();
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

                //if (context == null)
                //{
                //    throw new ArgumentNullException("context");
                //}

                //HttpResponseBase response = context.HttpContext.Response;

                //if (string.IsNullOrEmpty(this.ContentType))
                //{
                //    response.ContentType = this.ContentType;
                //}
                //else
                //{
                //    response.ContentType = "application/json";
                //}

                //if (this.ContentEncoding != null)
                //{
                //    response.ContentEncoding = this.ContentEncoding;
                //}

                //if (this.Data != null)
                //{
                //    JavaScriptSerializer jss = new JavaScriptSerializer();
                //    string jsonString = jss.Serialize(Data);
                //    string p = @"\\/Date\((\d+)\)\\/";
                //    MatchEvaluator matchEvaluator = new MatchEvaluator(this.ConvertJsonDateToDateString);
                //    Regex reg = new Regex(p);
                //    jsonString = reg.Replace(jsonString, matchEvaluator);

                //    response.Write(jsonString);
                //}
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
    }
}
