using FCake.API.EChi;
using FCake.Domain;
using FCake.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace FCake.Bll
{
    public class SMSService
    {
        //public YiChiSMS _yiChiSMS = new YiChiSMS();
        public EFDbContext _context = new EFDbContext();
        ///// <summary>
        ///// 发送正常短信[营销+关怀短信]
        ///// </summary>
        ///// <param name="mobiles">要发送的手机号列表，多于100个自动分批次发送</param>
        ///// <param name="content"></param>
        ///// <param name="errorMessage"></param>
        ///// <returns></returns>
        //public bool Send_NormalSMS(List<string> mobiles, string content, out string errorMessage)
        //{
        //    errorMessage = string.Empty;
        //    if (mobiles != null && mobiles.Count > 0)
        //    {
        //        //手机号码，80个一组，用英文状态下的逗号分隔。一次提交最多100个手机，这里就限制一次只提交80个
        //        string strMobiles = string.Empty;
        //        int sendOneTime = 80;//一次发送几个号码
        //        int errorCount = 0;
        //        for (int i = 0; i < mobiles.Count; i++)
        //        {
        //            if (!string.IsNullOrEmpty(strMobiles))
        //            {
        //                strMobiles += ",";
        //            }
        //            strMobiles += mobiles[i].Trim();
        //            if ((i + 1) % sendOneTime == 0)
        //            {
        //                bool isSuccess = _yiChiSMS.Send_NormalSMS(strMobiles, content, out errorMessage);
        //                //bool isSuccess = true;
        //                if (!isSuccess)
        //                {
        //                    errorCount++;
        //                }
        //                if (!string.IsNullOrEmpty(strMobiles))
        //                {
        //                    SaveSendSMSLog(isSuccess, errorMessage, strMobiles.Split(',').Count());
        //                }
        //                strMobiles = string.Empty;
        //                Thread.Sleep(1000);//每次发送80条完毕休眠1秒
        //            }
        //        }
        //        if (!string.IsNullOrEmpty(strMobiles))
        //        {
        //            bool isSuccess = _yiChiSMS.Send_NormalSMS(strMobiles, content, out errorMessage);
        //            //bool isSuccess = true;
        //            if (!isSuccess)
        //            {
        //                errorCount++;
        //            }
        //            if (!string.IsNullOrEmpty(strMobiles))
        //            {
        //                SaveSendSMSLog(isSuccess, errorMessage, strMobiles.Split(',').Count());
        //            }
        //        }
        //        if (errorCount <= 0) { return true; }
        //    }
        //    return false;
        //}

        private void SaveSendSMSLog(bool sendSMSResult, string sendSMSErrorMsg, int count)
        {
            SysLog sysLog = new SysLog();
            sysLog.Type = 10;
            sysLog.UId = "EChi";
            sysLog.ClientIP = "";
            sysLog.ServerName = "";
            sysLog.CreateDT = DateTime.Now;
            if (!sendSMSResult)
            {
                sysLog.Summary = sendSMSErrorMsg;
                sysLog.ModuleName = "发送正常短信失败，失败条数：" + count;
            }
            else
            {
                sysLog.Summary = "";
                sysLog.ModuleName = "发送正常短信成功，成功条数：" + count;
            }
            _context.SysLog.Add(sysLog);
            _context.SaveChanges();
        }
    }
}
