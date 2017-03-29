using FCake.DAL.Repositories;
using FCake.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FCake.Bll.Services
{
    public class PhoneCodeService:BaseService
    {
        /// <summary>
        /// 创建手机验证码记录
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="code"></param>
        /// <param name="customerID"></param>
        public void CreatePhoneCode(string mobile, string code, string customerID)
        {
            var pc = new PhoneCode
            {
                Code = code,
                CustomerID = customerID,
                SendTime = DateTime.Now,
                Mobile = mobile,
            };
            DAL.AddOrModify<PhoneCode>(pc, customerID);
            DAL.Commit();
        }

        /// <summary>
        /// 取出验证码 1分钟发送间隔 1小时有效 取最近一个
        /// </summary>
        /// <param name="customerID">客户ID</param>
        /// <returns></returns>
        public PhoneCode GetPhoneCodeByCID(string customerID)
        {
            var validTime = 60; //有效期，单位为分钟
            var codes = DAL.GetQuery<PhoneCode>().Where(a => a.CustomerID.Equals(customerID));
            if (codes.Any())
            {
                var code = codes.OrderByDescending(a => a.SendTime).First();
                if ((DateTime.Now - code.SendTime).TotalMinutes < validTime)
                {
                    return code;
                }
            }
            return null;
        }

        /// <summary>
        /// 取出验证码 2分钟有效(客户无登录)
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <returns></returns>
        public PhoneCode GetPhoneCodeByMobile(string mobile)
        {
            var validTime = 60; //有效期，单位为分钟
            var codes = DAL.GetQuery<PhoneCode>().Where(a => a.Mobile.Equals(mobile));
            if (codes.Any())
            {
                var code = codes.OrderByDescending(a => a.SendTime).First();
                if ((DateTime.Now - code.SendTime).TotalMinutes < validTime)
                {
                    return code;
                }
            }
            return null;
        }

        /// <summary>
        /// 判断手机是否使用过（客户无登录）
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool IsPhoneUsed(string mobile)
        {
            return DAL.GetQuery<Customers>().Any(a => a.Mobile.Equals(mobile));
        }

        /// <summary>
        /// 获得手机验证码重新发送倒计时
        /// </summary>
        /// <param name="customerID"></param>
        /// <returns>剩余时间 2分钟额度</returns>
        public double CountDown(string customerID)
        {
            var codes = DAL.GetQuery<PhoneCode>().Where(a => a.CustomerID.Equals(customerID));
            if (codes.Any())
            {
                var code = codes.OrderByDescending(a => a.SendTime).First();
                var countDown = (DateTime.Now - code.SendTime).TotalSeconds;
                if (countDown < 120)
                {
                    return 120 - countDown;
                }
            }
            return -1;
        }

        /// <summary>
        /// 验证手机验证码是否正确
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool CheckMobileCode(string mobile, string code)
        {
            var mc = GetPhoneCodeByMobile(mobile);
            if (mc != null)
            {
                return mc.Code.Equals(code) && mc.Mobile.Equals(mobile);
            }
            return false;
        }

        /// <summary>
        /// 使用后删除验证码
        /// </summary>
        /// <param name="code"></param>
        /// <param name="customerID"></param>
        /// <returns></returns>
        public int DeletePhoneCode(string mobile, string customerID)
        {
            var codes = DAL.GetQuery<PhoneCode>().Where(a => a.Mobile.Equals(mobile)).ToList();
            foreach (var x in codes)
            {
                DAL.Remove<PhoneCode>(x);
            }
            return DAL.Commit();
        }
    }
}
