using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Top.Api;
using Top.Api.Request;
using Top.Api.Response;
using FCake.Core.Common;
namespace FCake.API
{
    /// <summary>
    /// 阿里大鱼短信
    /// </summary>
    public class DaYuSMSHelper
    {
        public static List<DateTime> SendVerifyCodeTimeList = new List<DateTime>();
        
        /// <summary>
        /// 发送验证码短信
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="code"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public static bool SendSMSCode(string mobile, string code, out string errorMsg)
        {
            try
            {
                if (DaYuConfig.IsSendVerifyCode == true)
                {
                    #region 验证码短信流量控制  
                    var oldTime = DateTime.Now.AddMinutes(-DaYuConfig.SendVerifyCodeLimitMinute);
                    if (SendVerifyCodeTimeList.Count > DaYuConfig.SendVerifyCodeLimitCount)
                    {
                       //移除旧的发送时间记录
                        var oldTimeList = SendVerifyCodeTimeList.Where(o => o < oldTime).ToList();
                        if (oldTimeList != null && oldTimeList.Count > 0)
                        {
                            foreach (var item in oldTimeList)
                            {
                                SendVerifyCodeTimeList.Remove(item);
                            }
                        }
                        //判断流量是否超过限额
                        if (SendVerifyCodeTimeList.Count > DaYuConfig.SendVerifyCodeLimitCount)
                        {//超过限额，关闭短信功能，并发短信通知开发人员
                            string notifyErrorMsg = string.Empty;
                            SendNotifySMS(DaYuConfig.DeveloperMobile,DaYuConfig.VerifyCodeSMSExceptionTemplate,out notifyErrorMsg);
                            DaYuConfig.IsSendVerifyCode = false;
                            errorMsg = "短信流量达到限额限制，短信接口已关闭";
                            return false;
                        }

                    }
                    #endregion
                    ITopClient client = new DefaultTopClient(DaYuConfig.Url, DaYuConfig.AppKey, DaYuConfig.AppSecret);
                    AlibabaAliqinFcSmsNumSendRequest req = new AlibabaAliqinFcSmsNumSendRequest();
                    req.Extend = mobile;
                    req.SmsType = "normal";
                    req.SmsFreeSignName = DaYuConfig.SMSSignName;
                    req.SmsParam = (new { code = code }).ToJson();
                    req.RecNum = mobile;
                    req.SmsTemplateCode = DaYuConfig.SMSCodeCommonTemplate;
                    AlibabaAliqinFcSmsNumSendResponse rsp = client.Execute(req);
                    errorMsg = rsp.SubErrMsg;
                    if (!rsp.IsError)
                    {//短信发送成功，记录下短信发送时间
                        SendVerifyCodeTimeList.Add(DateTime.Now);
                    }
                    return !rsp.IsError;
                }
                else
                {
                    errorMsg = "短信流量达到限额限制，短信接口已关闭";
                    return false;
                }



            }
            catch (Exception ex)
            {
                errorMsg = "发送短信异常";
                return false;
            }
        }
        /// <summary>
        /// 发送通知类短信
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="templateCode"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public static bool SendNotifySMS(string mobile, string templateCode, out string errorMsg)
        {
            try
            {
                ITopClient client = new DefaultTopClient(DaYuConfig.Url, DaYuConfig.AppKey, DaYuConfig.AppSecret);
                AlibabaAliqinFcSmsNumSendRequest req = new AlibabaAliqinFcSmsNumSendRequest();
                req.Extend = mobile;
                req.SmsType = "normal";
                req.SmsFreeSignName = DaYuConfig.SMSSignName;
                req.SmsTemplateCode = templateCode;
                req.RecNum = mobile;
                AlibabaAliqinFcSmsNumSendResponse rsp = client.Execute(req);
                errorMsg = rsp.SubErrMsg;
                return !rsp.IsError;
            }
            catch (Exception ex)
            {
                errorMsg = "发送短信异常";
                return false;
            }
        }
    }
}
