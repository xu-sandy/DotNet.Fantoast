using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace FCake.Core.Common
{
    public static class JsonHelper
    {
        public static String ToJson<T>(this T obj)
        {
            JsonSerializerSettings jsonSs = new JsonSerializerSettings();
            var timeConverter = new IsoDateTimeConverter();
            timeConverter.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            jsonSs.Converters.Add(timeConverter);
            jsonSs.Converters.Add(new DataTableConverter());
            //jsonSs.Converters.Add(new JavaScriptDateTimeConverter());
            //jsonSs.Culture = System.Threading.Thread.CurrentThread.CurrentCulture;
            //jsonSs.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            //jsonSs.DateTimeZoneHandling = DateTimeZoneHandling.Local;
            string json = JsonConvert.SerializeObject(obj, Formatting.None, jsonSs);
            return json;
        }
    }
}
