using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCake.Domain;
using FCake.Domain.Entities;
using System.Web.Mvc;
using FCake.Core.Common;
using FCake.Core.MvcCommon;

namespace FCake.Bll
{
    public class CustomerAddressService
    {
        EFDbContext context = new EFDbContext();

        public CustomerAddress GetOneAddress(string addressId)
        {
            var address = context.CustomerAddress.SingleOrDefault(a => a.Id.Equals(addressId) && a.IsDeleted != 1);
            return address;
        }
        public List<CustomerAddress> GetCustomerAddressesById(string customerId, int isdef)
        {
            var addressModel = from adds in context.CustomerAddress.Where(p => p.CustomerId == customerId && p.IsDeleted != 1 && p.DeliveryType==0) select adds;
            if (isdef == 1)
            {
                addressModel = addressModel.Where(p => p.IsDef == isdef);

            }
            var addModel = addressModel.ToList();
            return addModel;
        }
        public CustomerAddress GetCustomerDefAddress(string addressId)
        {
            var address = context.CustomerAddress.SingleOrDefault(p => p.Id.Equals(addressId) && p.IsDeleted != 1);
            return address;
        }
        public CustomerAddress SaveCustomerAddress(FormCollection c, string userid)
        {
            CustomerAddress address = new CustomerAddress();
            address.CopyProperty(c.ToDic());

            if (address.CustomerId.IsNullOrTrimEmpty() == false)
            {
                if (address.Id.IsNullOrTrimEmpty() == false)
                {
                    address = context.CustomerAddress.SingleOrDefault(a => a.Id.Equals(address.Id) && a.IsDeleted != 1);
                    if (address == null)
                        return null;
                    address.CopyProperty(c.ToDic());
                    address.ModifiedBy = userid;
                    address.IsDeleted = 0;
                    address.ModifiedOn = DateTime.Now;
                }
                else
                {
                    address.Id = FCake.Core.Common.DataHelper.GetSystemID();
                    context.CustomerAddress.Add(address);
                    address.IsDeleted = 0;
                    address.CreatedBy = userid;
                    address.CreatedOn = DateTime.Now;
                }

                if (address.IsDef == 0)
                {
                    string id = address.Id;
                    //把之前的默认地址先更新为非默认
                    var ar = context.CustomerAddress.Where(p => p.IsDef == 0 && p.Id.Equals(id) == false && p.CustomerId == address.CustomerId && p.IsDeleted == 0);
                    foreach (var x in ar)
                    {
                        x.IsDef = 1;
                    }
                }

                //若该客户从无地址则设为默认
                var cusAddrs = context.CustomerAddress.Where(a => a.CustomerId == address.CustomerId && a.IsDeleted != 1);
                if (cusAddrs.Any() == false)
                {
                    address.IsDef = 0;
                }

                if (address.LogisticsSiteId.IsNullOrTrimEmpty() == false)
                {
                    var siteAddr = context.LogisticsSite.SingleOrDefault(l => l.Id.Equals(address.LogisticsSiteId) && l.IsDeleted != 1);
                    address.Province = siteAddr.SiteProvince;
                    address.City = siteAddr.SiteCity;
                    address.Area = siteAddr.SiteArea;
                    address.Address = siteAddr.SiteAddress;
                }

                context.SaveChanges();
                return address;
            }

            return null;
        }

        //删除收货地址 by cloud
        public string DelAddress(string addressId)
        {
            try
            {
                var record = context.CustomerAddress.Where(ca => ca.Id == addressId && ca.IsDeleted != 1).FirstOrDefault();
                if (record != null)
                {
                    if (record.IsDef == 0)//若为默认地址
                    {
                        throw new Exception("默认地址无法删除！");
                    }
                    record.IsDeleted = 1;
                    context.SaveChanges();
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        /// <summary>
        /// 从网页上保存地址
        /// </summary>
        /// <param name="currentUserID"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        public OpResult SaveWebCustomerAddress(string currentUserID, CustomerAddress address)
        {
            OpResult result = new OpResult();
            result.Successed = false;
            try
            {
                CustomerAddress custAddress = new CustomerAddress();
                bool isExist = address.Id.IsNotNullOrEmpty();//false为add，true为update
                var addresses = context.CustomerAddress.Where(a => a.CustomerId.Equals(currentUserID) && a.IsDeleted != 1);


                if (address.Id.IsNullOrTrimEmpty())
                {   //Add
                    if (addresses.Count() >= 10)
                        throw new Exception("最多保存10条收获地址");
                    custAddress.Id = custAddress.NewGuid();
                    custAddress.CreatedBy = currentUserID;
                    custAddress.CreatedOn = DateTime.Now;
                }
                else
                {   //update
                    if (address.LogisticsSiteId.IsNotNullOrEmpty())
                    { //原为自提则判断地址是否修改，修改则add
                        var site = context.LogisticsSite.SingleOrDefault(l => l.Id.Equals(address.LogisticsSiteId) && l.IsDeleted != 1);
                        if (address.Province != site.SiteProvince || address.City != site.SiteCity || address.Area != site.SiteArea || address.Address != site.SiteAddress)
                        {
                            custAddress.Id = custAddress.NewGuid();
                            custAddress.CreatedBy = currentUserID;
                            custAddress.CreatedOn = DateTime.Now;
                        }
                    }
                    else
                    {
                        custAddress = context.CustomerAddress.SingleOrDefault(a => a.Id.Equals(address.Id) && a.IsDeleted != 1);
                        if (custAddress != null)
                        {
                            custAddress.ModifiedBy = currentUserID;
                            custAddress.ModifiedOn = DateTime.Now;
                        }
                    }

                    //默认地址不能在修改时改为非默认
                    if (custAddress.IsDef == 0 && address.IsDef == 1)
                        throw new Exception("保存失败！请先设其他地址为默认地址");

                }

                //custAddress.Id = custAddress.NewGuid();
                //custAddress.CreatedBy = currentUserID;
                //custAddress.CreatedOn = DateTime.Now;
                custAddress.CustomerId = currentUserID;
                custAddress.IsDeleted = 0;

                if (address.IsDef == 0)
                {
                    foreach (var addr in addresses)
                    {
                        addr.IsDef = 1;
                    }
                    custAddress.IsDef = 0;
                }
                else
                    custAddress.IsDef = 1;

                //收货人不能为空
                #region
                if (address.Receiver.IsNullOrTrimEmpty())
                    throw new Exception("收货人不能为空");
                if (address.Receiver.Length > 20)
                    throw new Exception("收货人名称过长");
                custAddress.Receiver = address.Receiver;
                #endregion
                //地区不能为空
                #region
                if (address.Province.IsNullOrTrimEmpty())
                    throw new Exception("省份不能为空");
                if (address.Province.Length > 10)
                    throw new Exception("省份名称过长");
                custAddress.Province = address.Province;

                if (address.City.IsNullOrTrimEmpty())
                    throw new Exception("城市不能为空");
                if (address.City.Length > 10)
                    throw new Exception("城市名称过长");
                custAddress.City = address.City;

                if (address.Area.IsNullOrTrimEmpty())
                    throw new Exception("地区不能为空");
                if (address.Area.Length > 10)
                    throw new Exception("地区名称过长");
                custAddress.Area = address.Area;
                #endregion
                //街道地址不能为空
                #region
                if (address.Address.IsNullOrTrimEmpty())
                    throw new Exception("街道地址不能为空");
                if (address.Address.Length > 100)
                    throw new Exception("街道地址过长");
                custAddress.Address = address.Address;
                #endregion
                //手机与固定电话
                #region
                if (address.ReceiverTel.IsNullOrTrimEmpty() && address.ReceiverMobile.IsNullOrTrimEmpty())
                    throw new Exception("手机或固定电话至少填写一项");
                if (address.ReceiverTel.Length > 20)
                    throw new Exception("固定电话过长");
                if (address.ReceiverMobile.Length > 20)
                    throw new Exception("手机号码过长");
                custAddress.ReceiverTel = address.ReceiverTel;
                custAddress.ReceiverMobile = address.ReceiverMobile;
                #endregion
                //邮编
                custAddress.ZipCode = address.ZipCode;



                if (isExist == false)
                    context.CustomerAddress.Add(custAddress);

                if (context.SaveChanges() > 0)
                { 
                    result.Successed=true;
                    result.Data = custAddress;
                }




                return result;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                return result;
            }
        }

        public CustomerAddress GetCustomerAddressByUserId(string userId)
        {
            var address = context.CustomerAddress.SingleOrDefault(p => p.CustomerId.Equals(userId) && p.IsDef == 0 && p.IsDeleted != 1);
            return address;
        }

        public void RemoveAddress(string currentUserID, string addressID)
        {
            var address = context.CustomerAddress.SingleOrDefault(a => a.IsDeleted != 1 && a.Id.Equals(addressID) && a.CustomerId.Equals(currentUserID));
            if (address != null)
            {
                address.ModifiedBy = currentUserID;
                address.ModifiedOn = DateTime.Now;
                address.IsDeleted = 1;
                context.SaveChanges();
            }
        }

        /// <summary>
        /// 前台新增地址 by cloud
        /// </summary>
        /// <param name="customerId">客户Id</param>
        /// <param name="entity">前台传过来的地址对象</param>
        /// <param name="isDef">是否设为默认地址，0为是</param>
        /// <returns></returns>
        public CustomerAddress AddCustomerAddress(string customerId, CustomerAddress entity, int isDef)
        {
            CustomerAddress newAddr = entity; ;
            newAddr.Id = Guid.NewGuid().ToString("N");
            newAddr.CustomerId = customerId;
            newAddr.Province = entity.Province;
            newAddr.City = entity.City;
            newAddr.Area = entity.Area;
            newAddr.Address = entity.Address;
            if (isDef == 0)
            {
                var addrs = context.CustomerAddress.Where(c => c.CustomerId.Equals(customerId) && c.Id.Equals(newAddr.Id) == false && c.IsDeleted != 1);
                foreach (var addr in addrs)
                {
                    addr.IsDef = 1;
                }
            }
            newAddr.IsDef = isDef == 0 ? 0 : isDef; //默认地址值为0
            newAddr.CreatedBy = customerId;
            newAddr.CreatedOn = DateTime.Now;
            newAddr.IsDeleted = 0;
            context.CustomerAddress.Add(newAddr);
            context.SaveChanges();

            return null;
        }

        /// <summary>
        /// 设置非默认地址为默认
        /// </summary>
        /// <param name="addressId"></param>
        /// <param name="currentUserID"></param>
        /// <returns></returns>
        public bool ModifyAsDefAddress(string addressId, string currentUserID)
        {
            var addresses = context.CustomerAddress.Where(a => a.CustomerId.Equals(currentUserID) && a.IsDeleted != 1);
            foreach (var addr in addresses)
            {
                addr.IsDef = 1;
            }
            var curAddress = context.CustomerAddress.SingleOrDefault(a => a.Id.Equals(addressId));
            curAddress.IsDef = 0;
            context.SaveChanges();
            return true;
        }
    }
}
