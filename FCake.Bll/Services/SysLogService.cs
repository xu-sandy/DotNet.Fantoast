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
    public class SysLogService
    {
        public static void SaveSysLog(string uId, string clientIP, string summary, string moduleName)
        {
            try
            {
                EFDbContext _context = new EFDbContext();
                SysLog sysLog = new SysLog();
                sysLog.Type = 10;
                sysLog.UId = uId;
                sysLog.ClientIP = clientIP;
                sysLog.ServerName = "";
                sysLog.Summary = summary;
                sysLog.ModuleName = moduleName;
                sysLog.CreateDT = DateTime.Now;
                _context.SysLog.Add(sysLog);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return;
            }
        }
        public static void SaveAliSMSErrorLog(string errorMsg, string mobile, string templateCode)
        {
            try
            {
                EFDbContext _context = new EFDbContext();
                SysLog sysLog = new SysLog();
                sysLog.Type = 10;
                sysLog.UId = "AliDaYu";
                sysLog.ClientIP = "";
                sysLog.ServerName = "";
                sysLog.Summary = "错误：" + errorMsg + " 模板编号：" + templateCode;
                sysLog.ModuleName = "阿里大鱼短信发送失败,手机号码为：" + mobile;
                sysLog.CreateDT = DateTime.Now;
                _context.SysLog.Add(sysLog);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return;
            }
        }
    }
}
