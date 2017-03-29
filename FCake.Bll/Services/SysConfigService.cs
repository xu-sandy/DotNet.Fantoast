using FCake.Core.MvcCommon;
using FCake.Domain;
using FCake.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace FCake.Bll.Services
{
    public class SysConfigService
    {
        private EFDbContext _context = new EFDbContext();
        /// <summary>
        /// 修改营业配置信息
        /// </summary>
        /// <param name="type"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public OpResult PublishBusinessManage(string type, string val)
        {
            var retResult = OpResult.Fail("发布失败，请联系管理员");
            try
            {
                type = type.ToLower();
                var data = GetSysConfigByCode(type);
                if (data != null)
                {
                    data.Value = val;
                    if (_context.SaveChanges() > 0)
                    {
                        SysConfigsCache.ResetRegister();
                        var result = PublishBusinessManageApi();
                        if(result.ToString() == "true")
                            retResult = OpResult.Success("发布成功");
                    }
                }
            }
            catch(Exception ex)
            {
                //todo:添加发布出错日志
                retResult = OpResult.Fail(ex.Message);
            }
            return retResult;
        }

        public string PublishBusinessManageApi()
        {
            //todo:
            var token = new SysTemp();
            _context.SysTemps.Add(token);
            _context.SaveChanges();
            var weburlConfig = SysConfigsCache.GetWebUrlConfig();
            var pcUrl = weburlConfig.PCUrl + "Settings/BusinessManage?access_token=" + token.Code+"&a=a";
            var mobileUrl = weburlConfig.MobileUrl + "Settings/BusinessManage?access_token=" + token.Code + "&a=a";
            var result = GetData("", pcUrl);
            var result2 = GetData("", mobileUrl);
            return result2;
        }
        /// <summary>
        /// 根据Code获得对应的系统配置信息
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public SysConfig GetSysConfigByCode(string code)
        {
            var data = _context.SysConfigs.Where(c => c.Code == code).FirstOrDefault();
            return data;
        }
        /// <summary>
        /// 根据PCode获得对应的系统配置信息
        /// </summary>
        /// <param name="pcode"></param>
        /// <returns></returns>
        public List<SysConfig> GetSysConfigByPCode(string pcode)
        {
            var data = _context.SysConfigs.Where(c => c.PCode == pcode).ToList();
            return data;
        }
        /// <summary>
        /// 重置营业管理配置缓存
        /// </summary>
        /// <param name="access_token"></param>
        /// <returns></returns>
        public OpResult ResetSaleConfig(string access_token)
        {
            var result = OpResult.Fail("访问已过期");
            if (!String.IsNullOrEmpty(access_token))
            {
                var endTime = DateTime.Now.AddMinutes(1.0);
                //todo:验证访问令牌是否已经过期
                var query = _context.SysTemps.Where(s => s.Code == access_token && s.CreatedOn <= endTime);
                if (query.FirstOrDefault() != null)
                {
                    SysConfigsCache.ResetSalesConfig();
                    result = OpResult.Success("成功");
                }
            }
            return result;
        }


        private static string GetData(string RequestPara, string Url)
        {
            RequestPara = RequestPara.IndexOf('?') > -1 ? (RequestPara) : ("?" + RequestPara);

            WebRequest hr = HttpWebRequest.Create(Url + RequestPara);

            byte[] buf = System.Text.Encoding.GetEncoding("utf-8").GetBytes(RequestPara);
            hr.Method = "GET";

            System.Net.WebResponse response = hr.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("utf-8"));
            string ReturnVal = reader.ReadToEnd();
            reader.Close();
            response.Close();

            return ReturnVal;
        }
    }
}
