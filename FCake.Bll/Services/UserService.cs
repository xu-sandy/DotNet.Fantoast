using FCake.Core.Security;
using FCake.Core.MvcCommon;
using FCake.Domain;
using FCake.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCake.Framework;
using FCake.Domain.Common;
using System.Linq.Dynamic;
using FCake.Core.Common;
using System.Data.Entity;
namespace FCake.Bll
{
    public class UserService
    {
        public EFDbContext context = new EFDbContext();
        public bool UserNameExsist(string userName)
        {
            var user = (from tb in context.Users
                        where tb.UserName == userName && tb.IsDeleted != 1 && tb.IsDisabled != 1
                        select tb).FirstOrDefault();
            if (user != null)
                return true;
            else
                return false;
        }
        public User GetUser(string id)
        {
            User user = null;
            if (!string.IsNullOrEmpty(id))
            {
                user = (from tb in context.Users
                        where tb.Id == id && tb.IsDeleted != 1 && tb.IsDisabled != 1
                        select tb).FirstOrDefault();
            }
            else
            {
                user = new User();
            }
            return user;
        }
        public OpResult SaveUser(User entity, string currentUserId)
        {
            User newentity = null;
            if (string.IsNullOrEmpty(entity.Id))
            {
                entity.InitUser(currentUserId, GetPwd(entity.Password));
                newentity = entity;
                context.Users.Add(newentity);
            }
            else
            {
                newentity = context.Users.Where(s => s.Id == entity.Id).FirstOrDefault();
                if (newentity != null)
                {
                    newentity.FullName = entity.FullName;
                    newentity.Mobile = entity.Mobile;
                    newentity.Tel = entity.Tel;
                    newentity.Email = entity.Email;
                    newentity.Sex = entity.Sex;
                    newentity.IsDisabled = entity.IsDisabled;
                    newentity.ModifiedBy = currentUserId;
                    newentity.ModifiedOn = DateTime.Now;
                }
            }
            var result = new OpResult();
            if (newentity != null)
            {
                context.SaveChanges();
                result.Successed = true;
                result.Data = newentity;
                result.Message = "保存成功";
            }
            else
            {
                result.Successed = false;
                result.Message = "数据对象保存错误！";
            }
            return result;
        }
        public OpResult ResetPassword(string id, string password, string currentUserId)
        {
            var entity = context.Users.Where(s => s.Id == id).FirstOrDefault();
            if (entity != null)
            {
                entity.Password = GetPwd(password);
                entity.ModifiedBy = currentUserId;
                entity.ModifiedOn = DateTime.Now;
            }
            var result = new OpResult();
            if (entity != null)
            {
                context.SaveChanges();
                result.Successed = true;
                result.Data = entity;
                result.Message = "重置密码成功";
            }
            else
            {
                result.Successed = false;
                result.Message = "重置密码失败";
            }
            return result;
        }

        public OpResult GetUserDataBySearchInfo(QueryInfo queryInfo, PageInfo pageInfo, out int totalCount)
        {
            IQueryable<User> query = null;
            if (queryInfo.Where.Count() > 0)
            {
                query = context.Users.Where(queryInfo.ToSQLString(), queryInfo.GetParams()).Where(s => s.IsDeleted != 1);
            }
            else
            {
                query = context.Users.Where(s => s.IsDeleted != 1);
            }
            totalCount = query.Count();
            var data = query.OrderBy(s => s.Id).Skip((pageInfo.Page - 1) * pageInfo.Rows).Take(pageInfo.Rows);
            var result = new OpResult() { Successed = true, Data = data };
            return result;
        }

        public IQueryable<UserRole> GetRolesByUserID(string userid)
        {
            userid = userid.IsNullOrTrimEmpty() ? "" : userid.ToUpper();
            return context.UserRoles.Where(a => a.UserId.Equals(userid));
        }

        public string GetPwd(string pwd)
        {
            return Encrypt.GetMD5String(pwd);
        }

        public List<DropdownItem> GetReviewUserDropdownItem(string currentUserId, string reviewUID)
        {
            if (string.IsNullOrEmpty(currentUserId))
                currentUserId = "";
            if (string.IsNullOrEmpty(reviewUID))
                reviewUID = "";
            var result = new List<DropdownItem>();
            var userRolesList = context.UserRoles.Include(o => o.Role).Include(o => o.User)
                      .Where(o => o.Role.Code == "customerservice" || o.Role.Code == "customerservicemg" ||
                      o.UserId == currentUserId || o.UserId == reviewUID).ToList();
            List<User> userList = new List<User>();
            if (userRolesList != null)
            {
                foreach (var ur in userRolesList)
                {
                    if (ur.User != null)
                    {
                        if (userList.Where(o => o.Id == ur.UserId).FirstOrDefault() == null)
                        {
                            userList.Add(ur.User);
                        }
                    }
                }
            }

            if (userList.Count() > 0)
            {
                result.AddRange(from u in userList select new DropdownItem() { Value = u.Id, Text = u.FullName });
            }
            return result;
        }
    }
}
