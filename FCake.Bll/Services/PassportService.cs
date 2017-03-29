using FCake.Bll.Services;
using FCake.Core.Common;
using FCake.Core.MvcCommon;
using FCake.Core.Security;
using FCake.Domain;
using FCake.Domain.Entities;
using FCake.Domain.WebModels;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace FCake.Bll
{
    /// <summary>
    /// 网站会员登录注册相关
    /// </summary>
    public class PassportService
    {
        EFDbContext context = new EFDbContext();
        private readonly CouponsService _couponsService = new CouponsService();
        public PassportService() { }
        #region 注册
        /// <summary>
        /// 验证是否允许注册
        /// </summary>
        public bool CheckUserName(string mobile)
        {
            var user = context.Customers.Where(c => c.Mobile == mobile && c.Password != null && c.Password != "").FirstOrDefault();
            if (user != null)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public OpResult Register(RegisterUser user)
        {
            if (RegPwd(user.Password))
            {
                var pwd = GetEncryPwd(user.Password);
                try
                {
                    var entity = context.Customers.Where(c => c.Mobile == user.UserName).FirstOrDefault();
                    if (entity == null || string.IsNullOrEmpty(entity.Password))
                    {
                        if (entity == null)
                        {
                            //未注册过，直接注册设置密码
                            entity = new Customers(user.UserName, pwd);
                            context.Customers.Add(entity);
                        }
                        else
                        {
                            //有购买记录，但是未在网站注册过
                            entity.Password = pwd;
                            entity.ModifiedOn = DateTime.Now;
                            entity.CustomerType = 1;
                        }
                        if (context.SaveChanges() > 0)
                        {
                            _couponsService.RegisterDistributingCoupons(entity.Id);
                            return OpResult.Success(message: "注册成功", data: entity);
                        }
                    }
                    return OpResult.Fail("该手机号码已注册", code: "usernamerepeat");
                }
                catch (Exception ex)
                {
                    return OpResult.Fail(message: "注册用户错误:" + ex.Message);
                }
            }
            return OpResult.Fail(message: "密码格式错误");
        }
        public CaptchasHelper GetCaptchas()
        {
            var cap = new CaptchasHelper();
            return cap;
        }
        /// <summary>
        /// 验证密码格式是否正确
        /// </summary>
        /// <param name="pwd"></param>
        /// <returns></returns>
        private bool RegPwd(string pwd)
        {
            //todo:验证密码格式
            return true;
        }
        #endregion

        #region 登录
        public OpResult CheckLogin(string userName, string password)
        {
            string pwd = GetEncryPwd(password);
            var user = context.Customers.Where(c => c.Mobile == userName && c.Password == pwd).FirstOrDefault();
            if (user != null)
            {
                if (user.IsDisabled == 1)
                {
                    return OpResult.Fail("对不起，您的账号已被禁用");
                }
                return OpResult.Success("验证成功", null, user);
            }
            return OpResult.Fail("用户名或密码错误");
        }
        public Customers GetUser(string userId)
        {
            var user = context.Customers.Where(c => c.Id == userId);
            return user.FirstOrDefault();
        }

        #endregion

        #region 客户自己改密码
        public string ModifyPassword(string userId, string oldPwd, string newPwd)
        {
            var member = context.Customers.SingleOrDefault(c => c.Id.Equals(userId));
            try
            {
                if (member.Password != GetEncryPwd(oldPwd))
                    throw new Exception("原密码不正确！");
                else
                {
                    if (RegPwd(newPwd))
                    {
                        member.Password = GetEncryPwd(newPwd);
                        member.ChangePasswordTimeStamp = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"));
                        context.SaveChanges();
                        return "修改成功";
                    }
                    else
                        throw new Exception("新密码不符合格式");
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }
        #endregion

        #region 忘记密码重置
        public OpResult ResetPassword(string mobile, string code, string newPwd)
        {
            var pcsv = new PhoneCodeService();
            if (pcsv.CheckMobileCode(mobile, code))
            { //true
                var member = context.Customers.SingleOrDefault(c => c.Mobile.Equals(mobile));
                try
                {
                    if (RegPwd(newPwd))
                    {
                        member.Password = GetEncryPwd(newPwd);
                        member.ChangePasswordTimeStamp = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"));
                        context.SaveChanges();
                        pcsv.DeletePhoneCode(mobile, "mobile");
                        return new OpResult() { Successed = true, Message = "修改成功" };
                    }
                    else
                        return new OpResult() { Successed = false, Message = "新密码格式不正确"};
                }
                catch (Exception e)
                {
                    return new OpResult() { Successed = false, Message = e.Message};
                }
            }
            else
            { //false
                return new OpResult() { Successed = false, Message = "验证码不正确"};
            }
        }
        #endregion

        #region 私有
        /// <summary>
        /// 加密密码
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        private string GetEncryPwd(string password)
        {
            if (password.Length > 20)
                return password;
            return Encrypt.GetMD5String(password);
        }
        #endregion
    }
}
