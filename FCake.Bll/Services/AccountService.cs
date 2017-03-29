using FCake.Core.MvcCommon;
using FCake.Core.Security;
using FCake.Domain;
using FCake.Domain.Common;
using FCake.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FCake.Bll
{
    public class AccountService
    {
        public EFDbContext context = new EFDbContext();
        public string GetEncryptPwd(string password)
        {
            var pwd = password;
            if (password.Length < 21)
            {
                pwd = Encrypt.GetMD5String(pwd);
            }
            return pwd;
        }
        public OpResult CheckLogin(string userName, string password)
        {
            OpResult result = new OpResult() { Successed = false, Message = "非法用户，无登录权限！" };
            var userSql = (from tb in context.Users.Include("UserRoles").Include("UserRoles.Role")
                           where tb.UserName == userName && tb.Password == password
                           && tb.IsDeleted != 1 && tb.IsDisabled != 1
                           select tb);
            var user = userSql.FirstOrDefault();
            if (user != null)
            {
                var userData = GetUserData(user);
                if (userData != null) {
                    result.Successed = true;
                    result.Message = "登录成功";
                    result.Data = userData;
                    return result;
                }
                else
                {
                    result.Message = "该用户尚未配置权限，请联系管理员！";
                }
            }
            return result;
        }
        public UserData GetUserData(User user)
        {
            UserData userData = null;
            if (user != null) {
                var isAdmin = false;
                if (user.IsAdmin.HasValue)
                    isAdmin = user.IsAdmin.Value == 1;
                userData = new UserData()
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    FullName = user.FullName,
                    IsAdmin = isAdmin,
                    OfficeId = user.OfficeId
                };
                if (!isAdmin && user.UserRoles != null && user.UserRoles.Count > 0)
                {
                    var roleList = user.UserRoles.Where(ur => ur.Role != null).Select(ur => ur.Role).ToList();
                    if (roleList != null && roleList.Count > 0)
                    {
                        var supperRoleCount = roleList.Where(r => r != null && r.Code.ToLower() == "supperadmin").Count();
                        if (supperRoleCount == 1)
                            isAdmin = true;
                        userData.IsAdmin = isAdmin;
                        userData.Roles = (from r in roleList select new RoleData() { RoleId = r.Id, RoleCode = r.Code, RoleName = r.Name }).ToList();
                        return userData;
                    }
                }
            }
            return userData;
        }
    }
}
