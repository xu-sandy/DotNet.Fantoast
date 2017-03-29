using FCake.Bll;
using FCake.Domain;
using FCake.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCake.Core.Common;

namespace FCake.Bll
{
    public class RoleService
    {

        public static void SaveRoles(string userid, List<string> roleids)
        {
            userid = userid.IsNullOrTrimEmpty() ? "" : userid.ToUpper();
            roleids = roleids.Select(a => a.ToUpper()).ToList();
            EFDbContext context = new EFDbContext();
            var user = context.Users.Find(userid.ToUpper());
            if (user != null)
            {
                //删除
                var dels = context.UserRoles.Where(a => a.UserId.Equals(userid) && roleids.Contains(a.RoleId) == false);
                foreach (var x in dels)
                {
                    context.UserRoles.Remove(x);
                }
                //添加
                var temp = context.UserRoles.Where(a => a.UserId.Equals(userid)).Select(a => a.RoleId);
                var adds = roleids.Where(a => temp.Contains(a) == false).ToList();
                adds = context.Roles.Where(a => adds.Contains(a.Id)).Select(a => a.Id).ToList();
                foreach (var x in adds)
                {
                    context.UserRoles.Add(new UserRole
                    {
                        Id = FCake.Core.Common.DataHelper.GetSystemID(),
                        RoleId = x,
                        UserId = userid
                    });
                }

                context.SaveChanges();
                PermissionCache.ResetUserRoles(userid, context.UserRoles.Where(a => a.UserId.Equals(userid)).ToList());
            }
        }
    }
}
