using FCake.Core.Common;
using FCake.Core.MvcCommon;
using FCake.Core.Security;
using FCake.Domain;
using FCake.Domain.Entities;
using FCake.Domain.WebModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FCake.Bll.Services
{
    /// <summary>
    /// 会员信息相关
    /// </summary>
    public class MemberService
    {
        EFDbContext context = new EFDbContext();
        public MemberService() { }


        /// <summary>
        /// 返回会员个人信息,停用
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Customers GetMemberInfo(string userId, out CustomerAddress defAddr, out Product cake)
        {
            //var result = (from c in context.Customers
            //              join d in context.CustomerAddress on c.Id equals d.CustomerId
            //              //join p in context.Products on c.FavoriteCake equals p.Id
            //              where c.Id.Equals(userId) && d.IsDef == 0
            //              select new
            //              {
            //                  FullName = c.FullName,
            //                  Sex = (int)c.Sex,
            //                  Mobile = c.Mobile,
            //                  Email = c.Email,
            //                  DefAddress = d.Province + d.City + d.Area + d.Address,
            //                  Birthday = (DateTime)c.Birthday,
            //                  FavoriteCake = c.FavoriteCake
            //              }).SingleOrDefault();
            var member = context.Customers.SingleOrDefault(c => c.Id.Equals(userId) && c.IsDeleted != 1);

            cake = context.Products.SingleOrDefault(p => p.Id.Equals(member.FavoriteCake) && p.IsDeleted != 1);

            defAddr = context.CustomerAddress.SingleOrDefault(ca => ca.CustomerId.Equals(userId) && ca.IsDef == 0 && ca.IsDeleted != 1);
            if (!defAddr.IsObjNullOrEmpty())
            {
                if (!defAddr.LogisticsSiteId.IsNullOrTrimEmpty())
                {
                    var siteId = defAddr.LogisticsSiteId;
                    var siteAddr = context.LogisticsSite.SingleOrDefault(ls => ls.Id.Equals(siteId));
                    defAddr.Province = siteAddr.SiteProvince;
                    defAddr.City = siteAddr.SiteCity;
                    defAddr.Area = siteAddr.SiteArea;
                    defAddr.Address = siteAddr.SiteAddress;
                }
                else
                {
                    defAddr.Province = defAddr.Province;
                    defAddr.City = defAddr.City;
                    defAddr.Area = defAddr.Area;
                    defAddr.Address = defAddr.Address;
                }
            }

            return member;
        }

        /// <summary>前台
        /// 保存会员个人信息 by cloud
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="entity">前台传过来的customer实体</param>
        /// <param name="province"></param>
        /// <param name="city"></param>
        /// <param name="area"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        public OpResult SaveMemberInfo(string userId, Customers entity, string province, string city, string area, string address)
        {
            OpResult result = null;
            var newentity = context.Customers.SingleOrDefault(c => c.Id.Equals(userId) && c.IsDeleted != 1);
            var defAddr = context.CustomerAddress.SingleOrDefault(ca => ca.CustomerId.Equals(userId) && ca.IsDef == 0 && ca.IsDeleted != 1);
            if (newentity != null)
            {
                newentity.FullName = entity.FullName;
                newentity.Sex = entity.Sex;
                newentity.Mobile = entity.Mobile;
                if(context.Customers.Any(c => c.Mobile == entity.Mobile && c.Id != userId))
                    return result = new OpResult() { Successed = false, Message = "该手机号已注册，无法修改" };
                newentity.Email = entity.Email;
                newentity.Birthday = entity.Birthday;
                newentity.FavoriteCake = entity.FavoriteCake;
                newentity.ModifiedOn = DateTime.Now;
                newentity.ModifiedBy = userId;
                context.SaveChanges();
                if (!province.IsNullOrTrimEmpty() && !city.IsNullOrTrimEmpty() && !area.IsNullOrTrimEmpty() && !address.IsNullOrTrimEmpty())
                { //只有当地址信息不为空时才可继续更新

                    if (defAddr != null)
                    { //当有默认地址时
                        if (!defAddr.LogisticsSiteId.IsNullOrTrimEmpty())
                        { //默认地址为自提站点时,比较是否修改过地址
                            if (province != defAddr.Province || city != defAddr.City || area != defAddr.Area || address.Trim() != defAddr.Address)
                            { //地址与数据库不一致时——新增地址，原先的改 isDef=1
                                var newAddr = new CustomerAddress();
                                newAddr.Province = province;
                                newAddr.City = city;
                                newAddr.Area = area;
                                newAddr.Address = address;

                                defAddr.IsDef = 1; //改原先的为 1

                                new CustomerAddressService().AddCustomerAddress(userId, newAddr, 0);
                            }
                            
                        }
                        else
                        { //默认地址不为自提站点时，直接update
                            defAddr.Province = province;
                            defAddr.City = city;
                            defAddr.Area = area;
                            defAddr.Address = address;
                            defAddr.ModifiedBy = userId;
                            defAddr.ModifiedOn = DateTime.Now;
                            context.SaveChanges();
                        }
                    }
                    else
                    { //当无默认地址，即无购物历史时——新增地址
                        var newAddr = new CustomerAddress();
                        newAddr.Province = province;
                        newAddr.City = city;
                        newAddr.Area = area;
                        newAddr.Address = address;
                        newAddr.Receiver = entity.FullName;
                        newAddr.ReceiverMobile = entity.Mobile;

                        new CustomerAddressService().AddCustomerAddress(userId, newAddr, 0);
                    }

                    result = new OpResult() { Successed = true, Message = "保存成功" };
                }
                else
                {
                    result = new OpResult() { Successed = false, Message = "请填写完整地址" };
                }
            }
            return result;
        }


        public OpResult SaveNewPhone(string userId, Customers entity)
        {
            OpResult result = null;
            var newentity = context.Customers.SingleOrDefault(c => c.Id.Equals(userId) && c.IsDeleted != 1);
            if (newentity != null)
            {
                newentity.Mobile = entity.Mobile;
                if (context.Customers.Any(c => c.Mobile == entity.Mobile && c.Id != userId))
                    return result = new OpResult() { Successed = false, Message = "该手机号已注册，无法修改" };
                newentity.ModifiedOn = DateTime.Now;
                newentity.ModifiedBy = userId;
                context.SaveChanges();
                result = new OpResult() { Successed = true, Message = "绑定成功" };
            }
            return result;
        }

        public Customers GetMember(string memberID)
        {
            return context.Customers.SingleOrDefault(c => c.Id.Equals(memberID) && c.IsDeleted != 1);
        }
        public Product GetFavoriteCakeByPid(string id)
        {
            return context.Products.SingleOrDefault(p => p.Id.Equals(id) && p.IsDeleted != 1);
        }
    }
}
