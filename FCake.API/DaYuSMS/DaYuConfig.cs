using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace FCake.API
{
    public class DaYuConfig
    {
        /// <summary>
        /// 为预防短信模块被恶意攻击，若一段时间内短信验证码发送量达到限额，则置为false,关闭发送验证码短信接口，并发短信告知开发人员
        /// </summary>
        public static bool IsSendVerifyCode = true;
        public static string Url
        {
            get { return string.IsNullOrEmpty(ConfigurationManager.AppSettings["DaYuUrl"]) ? "" : ConfigurationManager.AppSettings["DaYuUrl"]; }
        }
        public static string AppKey
        {
            get { return string.IsNullOrEmpty(ConfigurationManager.AppSettings["DaYuAppKey"]) ? "" : ConfigurationManager.AppSettings["DaYuAppKey"]; }
        }
        public static string AppSecret
        {
            get { return string.IsNullOrEmpty(ConfigurationManager.AppSettings["DaYuAppSecret"]) ? "" : ConfigurationManager.AppSettings["DaYuAppSecret"]; }
        }
        public static string SMSSignName
        {
            get { return string.IsNullOrEmpty(ConfigurationManager.AppSettings["SMSSignName"]) ? "" : ConfigurationManager.AppSettings["SMSSignName"]; }
        }
        /// <summary>
        /// 通用短信验证码模板
        /// </summary>
        public static string SMSCodeCommonTemplate
        {
            get { return string.IsNullOrEmpty(ConfigurationManager.AppSettings["SMSCodeCommonTemplate"]) ? "" : ConfigurationManager.AppSettings["SMSCodeCommonTemplate"]; }
        }
        /// <summary>
        /// 订单通过模板
        /// </summary>
        public static string OrderApproveTemplate
        {
            get { return string.IsNullOrEmpty(ConfigurationManager.AppSettings["OrderApproveTemplate"]) ? "" : ConfigurationManager.AppSettings["OrderApproveTemplate"]; }
        }
        /// <summary>
        /// 开始配送模板
        /// </summary>
        public static string BeginDeliveryTemplate
        {
            get { return string.IsNullOrEmpty(ConfigurationManager.AppSettings["BeginDeliveryTemplate"]) ? "" : ConfigurationManager.AppSettings["BeginDeliveryTemplate"]; }
        }
        /// <summary>
        /// 订单审核不通过模板
        /// </summary>
        public static string OrderAuditNotThroughTemplate
        {
            get { return string.IsNullOrEmpty(ConfigurationManager.AppSettings["OrderAuditNotThroughTemplate"])? "" :ConfigurationManager.AppSettings["OrderAuditNotThroughTemplate"]; }
        }
        /// <summary>
        /// 验证码短信发送量异常模板
        /// </summary>
        public static string VerifyCodeSMSExceptionTemplate
        {
            get { return string.IsNullOrEmpty(ConfigurationManager.AppSettings["VerifyCodeSMSExceptionTemplate"]) ? "" : ConfigurationManager.AppSettings["VerifyCodeSMSExceptionTemplate"]; }
        }
        /// <summary>
        /// 开发人员手机号
        /// </summary>
        public static string DeveloperMobile
        {
            get { return string.IsNullOrEmpty(ConfigurationManager.AppSettings["DeveloperMobile"]) ? "" : ConfigurationManager.AppSettings["DeveloperMobile"]; }
        }
        /// <summary>
        /// x分钟内限制短信验证码条数
        /// </summary>
        public static int SendVerifyCodeLimitCount
        {
            get { return string.IsNullOrEmpty(ConfigurationManager.AppSettings["SendVerifyCodeLimitCount"]) ? 100 : int.Parse(ConfigurationManager.AppSettings["SendVerifyCodeLimitCount"]); }
        }
        /// <summary>
        /// x分钟内限制短信验证码条数的分钟数（正整数）
        /// </summary>
        public static int SendVerifyCodeLimitMinute
        {
            get { return string.IsNullOrEmpty(ConfigurationManager.AppSettings["SendVerifyCodeLimitMinute"]) ? 5 : int.Parse(ConfigurationManager.AppSettings["SendVerifyCodeLimitMinute"]); }
        }
    }
}
