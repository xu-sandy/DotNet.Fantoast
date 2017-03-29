using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;

namespace FCake.API
{
    public class DDSMSHelper
    {
        private static string DingDongUrl
        {
            get { return string.IsNullOrEmpty(ConfigurationManager.AppSettings["DingDongUrl"]) ? "" : ConfigurationManager.AppSettings["DingDongUrl"]; }
        }
        private static string DingDongAppKey
        {
            get { return string.IsNullOrEmpty(ConfigurationManager.AppSettings["DingDongAppKey"]) ? "" : ConfigurationManager.AppSettings["DingDongAppKey"]; }
        }
        private static string DingDongSignName
        {
            get { return string.IsNullOrEmpty(ConfigurationManager.AppSettings["DingDongSignName"]) ? "" : ConfigurationManager.AppSettings["DingDongSignName"]; }
        }

        delegate string DingDongHttpPostTask(string Url, string postDataStr);
        /// <summary>
        /// 发送营销短信
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="content"></param>
        public static void SendMarketingSMS(string mobile, string content)
        {
            try
            {
                mobile = HttpUtility.UrlEncode(mobile, Encoding.UTF8);
                // 发营销短信调用示例
                // 设置您要发送的内容，短信末尾必须带有“退订回T”      
                //content = "【中创xmzc】6.20蛋糕店周年庆，说好的会员50元兑换券，已经砸到你的账户，请点击查收www.xxxx.cn 退订回T";
                content = string.Format("【{0}】" + content + "退订回T", DingDongSignName);
                content = HttpUtility.UrlEncode(content, Encoding.UTF8);
                string data_send_yx = "apikey=" + DingDongAppKey + "&mobile=" + mobile + "&content=" + content;
                //异步调用叮咚短信发送，防止叮咚服务器无响应用户注册不了
                DingDongHttpPostTask caller = new DingDongHttpPostTask(DingDongHttpPost);
                caller.BeginInvoke(DingDongUrl, data_send_yx, null, caller);
                return;
            }
            catch (Exception ex)
            {
                return;
            }
        }

        #region 私有方法
        private static string DingDongHttpPost(string Url, string postDataStr)
        {
            var result = string.Empty;
            byte[] dataArray = Encoding.UTF8.GetBytes(postDataStr);
            // Console.Write(Encoding.UTF8.GetString(dataArray));

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = dataArray.Length;
            //request.CookieContainer = cookie;
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(dataArray, 0, dataArray.Length);
            dataStream.Close();
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                String res = reader.ReadToEnd();
                reader.Close();
                Console.Write("\nResponse Content:\n" + res + "\n");
                result = res;
            }
            catch (Exception e)
            {
                Console.Write(e.Message + e.ToString());
            }

            return result;

        }
        #endregion
    }
}
