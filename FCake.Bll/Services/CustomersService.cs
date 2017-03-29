using FCake.Bll;
using FCake.Core.MvcCommon;
using FCake.Domain;
using FCake.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data.Entity;
using LinqKit;
using FCake.Core.Common;
using FCake.Domain.Enums;
using System.Linq.Expressions;
using FCake.Domain.Common;

namespace FCake.Bll
{
    public class CustomersService : BaseService
    {
        public static Bll.CustomersService pro = new Bll.CustomersService();
        EFDbContext context = new EFDbContext();

        public CustomersService() { }
        /// <summary>
        /// 根据条件获取用户信息
        /// </summary>
        /// <returns>所有用户信息集合</returns>
        public List<Customers> GetCustomerByWhere(Expression<Func<Customers, bool>> whereLambda)
        {
            return context.Customers.Where(whereLambda).ToList();
        }
        public CustomersService(EFDbContext context) { this.context = context; }


        /// <summary>
        /// 根据查询条件查询用户信息
        /// </summary>
        /// <param name="sex">性别</param>
        /// <param name="type">类型</param>
        /// <param name="beginage">年龄开始</param>
        /// <param name="endage">年龄结束</param>
        /// <param name="validdateBegin">注册开始时间</param>
        /// <param name="validdateEnd">注册结束时间</param>
        /// <param name="recordsCount">总数据count</param>
        /// <param name="startRecord">分页查询开始</param>
        /// <param name="getCountNum">每次查询条数</param>
        /// <returns></returns>
        public List<Customers> GetCustomers(string fullName, string phone, int memberLevelVal, int orderby, out int recordsCount, int startRecord = 1, int getCountNum = 10)
        {
            recordsCount = 0;
            var result = new List<Customers>();
            var dquery = context.Customers.Where(c => c.IsDeleted != 1);
            if (fullName.IsNotNullOrEmpty())
            {
                dquery = dquery.Where(p => p.FullName.Contains(fullName));
            }
            if (phone.IsNotNullOrEmpty())
            {
                dquery = dquery.Where(p => p.Mobile.Contains(phone));
            }
            //memberLevelVal   -1=全部
            if (memberLevelVal != -1)
            {
                dquery = dquery.Where(p => p.MemberLevelValue.Equals(memberLevelVal));
            }
            //排序 2=按消费金额倒序
            if (orderby == 2)
            {
                dquery = dquery.OrderByDescending(p => p.TotalActualRMBPay);
            }
            else
            {
                dquery = dquery.OrderByDescending(p => p.CreatedOn);
            }
            recordsCount = dquery.Count();

            startRecord = startRecord < 0 ? 1 : startRecord;

            result = dquery.Skip((startRecord - 1) * getCountNum).Take(getCountNum).ToList();
            return result;
        }

        public Customers GetById(string id)
        {
            return context.Customers.SingleOrDefault(a => a.IsDeleted != 1 && a.Id.Equals(id));
        }
        public Customers GetByIdOrNew(string id)
        {
            var result = GetById(id);
            if (result == null)
                result = new Customers();
            return result;
        }
        public dynamic CustomerLocate(string condition, string status, out int totalCount, int page = 1, int rows = 30)
        {
            dynamic result;
            totalCount = 0;
            switch (status)
            {
                case "orderno":
                    totalCount = (from c in context.Customers
                                  from d in context.CustomerAddress
                                  from o in context.Orders
                                  where d.CustomerId.Equals(c.Id) && o.CustomerId.Equals(c.Id) && o.No.Contains(condition) && o.IsDeleted != 1 && d.Id == o.CustomerAddressId
                                  select new
                                  {
                                      id = c.Id,
                                      name = c.FullName,
                                      mobile = c.Mobile,
                                      address = d.Province + d.City + d.Area + d.Address,
                                      order = o.No == null ? "" : o.No
                                  }).Count();

                    result = (from c in context.Customers
                              from d in context.CustomerAddress
                              from o in context.Orders
                              where d.CustomerId.Equals(c.Id) && o.CustomerId.Equals(c.Id) && o.No.Contains(condition) && o.IsDeleted != 1 && d.Id == o.CustomerAddressId
                              select new
                              {
                                  id = c.Id,
                                  name = c.FullName,
                                  mobile = c.Mobile,
                                  address = d.Province + d.City + d.Area + d.Address,
                                  order = o.No == null ? "" : o.No
                              }).OrderByDescending(s => s.order).Skip((page - 1) * rows).Take(rows).ToList();

                    break;
                case "customername":
                    totalCount = (from v in context.VCustomOrders
                                  where v.FullName.Contains(condition)
                                  select new
                                  {
                                      id = v.Id,
                                      name = v.FullName,
                                      mobile = v.Mobile,
                                      address = v.Province + v.City + v.Area + v.Address,
                                      order = v.No == null ? "" : v.No
                                  }).Count();
                    result = (from v in context.VCustomOrders
                              where v.FullName.Contains(condition)
                              select new
                              {
                                  id = v.Id,
                                  name = v.FullName,
                                  mobile = v.Mobile,
                                  address = v.Province + v.City + v.Area + v.Address,
                                  order = v.No == null ? "" : v.No
                              }).OrderBy(s => s.name).Skip((page - 1) * rows).Take(rows).ToList();
                    break;
                case "mobile":
                    totalCount = (from v in context.VCustomOrders
                                  where v.Mobile.Contains(condition)
                                  select new
                                  {
                                      id = v.Id,
                                      name = v.FullName,
                                      mobile = v.Mobile,
                                      address = v.Province + v.City + v.Area + v.Address,
                                      order = v.No == null ? "" : v.No
                                  }).Count();
                    result = (from v in context.VCustomOrders
                              where v.Mobile.Contains(condition)
                              select new
                              {
                                  id = v.Id,
                                  name = v.FullName,
                                  mobile = v.Mobile,
                                  address = v.Province + v.City + v.Area + v.Address,
                                  order = v.No == null ? "" : v.No
                              }).OrderBy(s => s.mobile).Skip((page - 1) * rows).Take(rows).ToList();
                    break;
                default:
                    result = null;
                    break;
            }
            return result;
        }

        /// <summary>
        /// 根据客户Id查询所有使用地址
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public List<CustomerAddress> GetCustomerAddByCustomerId(string customerId)
        {
            if (customerId != null)
            {
                var addressModel = context.CustomerAddress.Where(p => p.IsDeleted != 1 && customerId == p.CustomerId);
                return addressModel.ToList();
            }
            return null;

        }
        public List<Dictionary<string, object>> CustomerAddress(string customerAddressId)
        {
            var customersAddress = context.CustomerAddress.Where(p => p.IsDeleted != 1 && p.CustomerId == customerAddressId);
            var temp = (from customerAdd in customersAddress
                        join adds in context.Customers on customerAdd.CustomerId equals adds.Id
                        select new
                        {
                            u = adds,
                            a = customerAdd.Address,
                            area = customerAdd.Area,
                            isdef = customerAdd.IsDef == 0 ? "是" : "否"
                        }).ToList();
            var dic = FCake.Core.Common.DataHelper.ToDic(temp, "u");
            return dic;
        }

        public object SearchCustomersByPhoneOrder(string name, string tel, int page, int rows)
        {
            var svc = new CommonService();
            var customerData = context.Customers.Where(p => p.IsDeleted != 1);
            if (!name.IsNullOrTrimEmpty())
            {
                customerData = customerData.Where(p => p.FullName.Contains(name));
            }
            if (!tel.IsNullOrTrimEmpty())
            {
                customerData = customerData.Where(p => p.Tel.Contains(tel) || p.Mobile.Contains(tel));
            }

            var result = customerData.OrderBy(a => a.Id).Skip((page - 1) * rows).Take(rows).ToList()
                .Select(a => new
                {
                    Id = a.Id,
                    FullName = a.FullName + "",
                    Tel = a.Tel + "",
                    Mobile = a.Mobile + "",
                    Email = a.Email + "",
                    Sex = EnumHelper.GetDescription(((Sex)(a.Sex ?? 3))),
                    CustomerType = svc.GetDictionaryName("CustomerCategory", a.CustomerType + "")
                });

            return new { total = customerData.Count(), rows = result };
        }
        /// <summary>
        /// 根据条件查找用户信息
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="tel"></param>
        /// <param name="pageInfo"></param>
        /// <returns></returns>
        public object SearchCustomersByWhere(string ids, string name, string tel, PageInfo pageInfo)
        {
            var svc = new CommonService();
            var memberLevel = new MemberLevelService();//12-22新增
            var customerData = context.Customers.Where(p => p.IsDeleted != 1);
            if (!name.IsNullOrTrimEmpty())
            {
                customerData = customerData.Where(p => p.FullName.Contains(name));
            }
            if (!tel.IsNullOrTrimEmpty())
            {
                customerData = customerData.Where(p => p.Tel.Contains(tel) || p.Mobile.Contains(tel));
            }
            if (ids.IsNotNullOrEmpty())
            {
                customerData = customerData.Where(p => !ids.Contains(p.Id));
            }

            var result = customerData.OrderBy(a => a.Id).Skip((pageInfo.Page - 1) * pageInfo.Rows).Take(pageInfo.Rows).ToList()
              .Select(a => new
              {
                  Id = a.Id,
                  FullName = a.FullName + "",
                  Tel = a.Tel + "",
                  Mobile = a.Mobile + "",
                  Email = a.Email + "",
                  Sex = EnumHelper.GetDescription(((Sex)(a.Sex ?? 3))),
                  CustomerType = svc.GetDictionaryName("CustomerCategory", a.CustomerType + ""),
                  Integral = a.Integral,//12-22新增
                  MemberLevelText = memberLevel.GetMemberLevelByLevelVal(a.MemberLevelValue).Title//12-22新增
              });
            return new { total = customerData.Count(), rows = result };
        }
        /// <summary>
        /// 根据一堆id查询客户信息
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="pageinfo"></param>
        /// <returns></returns>
        public object GetCustomerIdByIds(string ids, out int totalCount, PageInfo pageinfo)
        {
            var svc = new CommonService();
            var memberLevel = new MemberLevelService();
            var queryData = context.Customers.Where(p => ids.Contains(p.Id) && p.IsDeleted != 1);
            totalCount = queryData.Count();
            var result = queryData.OrderByDescending(p => p.CreatedOn).Skip((pageinfo.Page - 1) * pageinfo.Rows).Take(pageinfo.Rows).ToList()
                .Select(a => new
                {
                    Id = a.Id,
                    FullName = a.FullName + "",
                    Tel = a.Tel + "",
                    Mobile = a.Mobile + "",
                    Email = a.Email + "",
                    Sex = EnumHelper.GetDescription(((Sex)(a.Sex ?? 3))),
                    CustomerType = svc.GetDictionaryName("CustomerCategory", a.CustomerType + ""),
                    Integral = a.Integral,
                    MemberLevelText = memberLevel.GetMemberLevelByLevelVal(a.MemberLevelValue).Title
                });

            return result;
        }

        public object GetCustomerByID(string id)
        {
            var svc = new CommonService();
            var a = context.Customers.Find(id);
            return new
            {
                Id = a.Id,
                FullName = a.FullName + "",
                Tel = a.Tel + "",
                Mobile = a.Mobile + "",
                Email = a.Email + "",
                Sex = EnumHelper.GetDescription(((Sex)(a.Sex ?? 3))),
                CustomerType = svc.GetDictionaryName("CustomerCategory", a.CustomerType + "")
            };

        }

        public object CreateCustomerByPhone(string fullName, string mobile, string tel)
        {
            try
            {
                var svc = new CommonService();
                if (mobile != "" || tel != "")
                {
                    var isExits = false;
                    if (mobile != "")
                    {
                        isExits = context.Customers.Any(p => p.Mobile == mobile);
                    }
                    else
                    {
                        isExits = context.Customers.Any(p => p.Tel == tel);
                    }
                    if (isExits)
                    {
                        return new { status = false, message = "添加失败：联系号码已存在" };
                    }
                    FCake.Domain.Entities.Customers customer = new FCake.Domain.Entities.Customers();
                    customer.FullName = fullName;
                    customer.Mobile = mobile;
                    customer.Tel = tel;
                    customer.IsDeleted = 0;
                    customer.Id = FCake.Core.Common.DataHelper.GetSystemID();
                    customer.CustomerType = 2;//2=电话会员
                    customer.MemberLevelValue = 1;//首次创建默认为普通卡会员
                    customer.UpdateMemberLevelTime = DateTime.Now;//首次创建默认为当前时间
                    context.Customers.Add(customer);
                    int result = context.SaveChanges();
                    if (result > 0)
                        return new
                        {
                            status = true,
                            message = "添加成功",
                            customer = new
                            {
                                Id = customer.Id,
                                FullName = customer.FullName,
                                Tel = customer.Tel,
                                Mobile = customer.Mobile,
                                Email = customer.Email + "",
                                Sex = EnumHelper.GetDescription(((Sex)(customer.Sex ?? 3))),
                                CustomerType = svc.GetDictionaryName("CustomerCategory", customer.CustomerType + "")
                            }
                        };
                    else
                        return new { status = false, message = "添加失败" };
                }
                else
                {
                    return new { status = false, message = "添加失败,手机号码、固定电话至少填一项" };
                }


            }
            catch (Exception e)
            {
                return new { status = false, message = "添加出错：" + e.Message };
            }
        }

        public static string GetCustomNameByCustomID(string id)
        {
            var c = new EFDbContext().Customers.Find(id);
            if (c != null)
                return c.FullName;
            return "";
        }

        public Customers GetCustomerInfoById(string customerId, out string province, out string city, out string area, out string address)
        {
            Customers customer = context.Customers.Where(c => c.Id == customerId && c.IsDeleted != 1).FirstOrDefault();
            CustomerAddress cusAddr = context.CustomerAddress.Where(cd => cd.CustomerId == customerId && cd.IsDef == 0 && cd.IsDeleted != 1).FirstOrDefault();
            if (cusAddr != null)
            {
                province = cusAddr.Province == null ? "" : cusAddr.Province;
                city = cusAddr.City == null ? "" : cusAddr.City;
                area = cusAddr.Area == null ? "" : cusAddr.Area;
                address = cusAddr.Address == null ? "" : cusAddr.Address;
                if (cusAddr.LogisticsSiteId != null)
                {
                    var logisticsSite = context.LogisticsSite.SingleOrDefault(l => l.Id.Equals(cusAddr.LogisticsSiteId) && l.IsDeleted != 1);
                    if (logisticsSite != null)
                    {
                        province = logisticsSite.SiteProvince == null ? "" : logisticsSite.SiteProvince;
                        city = logisticsSite.SiteCity == null ? "" : logisticsSite.SiteCity;
                        area = logisticsSite.SiteArea == null ? "" : logisticsSite.SiteArea;
                        address = logisticsSite.SiteAddress == null ? "" : logisticsSite.SiteAddress;
                    }
                }
            }
            else
            {
                province = "";
                city = "";
                area = "";
                address = "";
            }
            return customer;
        }

        /// <summary>
        /// 编辑保存客户资料 by cloud
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        public OpResult SaveCustomerInfo(VCustomer customer, string userId)
        {
            OpResult result = null;
            Customers newentity = context.Customers.Where(c => c.Id == customer.Id).FirstOrDefault();
            CustomerAddress newAddr = context.CustomerAddress.Where(a => a.CustomerId == customer.Id && a.IsDef == 0).SingleOrDefault();
            if (newentity != null)
            {
                newentity.FullName = customer.FullName;
                newentity.Sex = customer.Sex;
                newentity.Tel = customer.Tel;
                newentity.Mobile = customer.Mobile;
                newentity.Email = customer.Email;
                newentity.IsDisabled = customer.IsDisabled;
                //修改地址
                if (newAddr == null || newAddr.LogisticsSiteId != null)
                {
                    newAddr = new CustomerAddress();
                    newAddr.Id = FCake.Core.Common.DataHelper.GetSystemID();
                    context.CustomerAddress.Add(newAddr);
                    newAddr.CustomerId = customer.Id;
                    var ar = context.CustomerAddress.Where(p => p.IsDef == 0 && p.Id.Equals(newAddr.Id) == false && p.CustomerId == customer.Id && p.IsDeleted != 1);
                    foreach (var x in ar)
                    {
                        x.IsDef = 1;
                    }
                    newAddr.IsDef = 0;
                    newAddr.IsDeleted = 0;
                    newAddr.Receiver = customer.FullName;
                    newAddr.ReceiverMobile = customer.Mobile;
                    newAddr.ReceiverTel = customer.Tel;
                    newAddr.CreatedBy = userId;
                    newAddr.CreatedOn = DateTime.Now;
                }
                newAddr.Province = customer.Province;
                newAddr.City = customer.City;
                newAddr.Area = customer.Area;
                newAddr.Address = customer.Address;
            }
            try
            {
                if (context.SaveChanges() > 0)
                {
                    result = new OpResult() { Successed = true, Message = "用户信息更新成功" };
                }
                else
                    result = new OpResult() { Successed = false, Message = "无需要更新的数据" };
            }
            catch (Exception ex)
            {
                result = new OpResult() { Successed = false, Message = ex.Message };
            }

            return result;
        }

        public string GetOrderIdByNo(string orderNo)
        {
            var data = context.Orders.Where(o => o.No == orderNo).FirstOrDefault();
            if (data != null)
                return data.Id;
            else
                return string.Empty;
        }

        //获取客户默认地址（可能是站点自提地址）
        public CustomerAddress GetDefAddress(string customerId)
        {
            var defRecord = context.CustomerAddress.Where(ca => ca.IsDeleted != 1 && ca.IsDef == 0).SingleOrDefault();
            if (!defRecord.LogisticsSiteId.IsNullOrTrimEmpty())
            {
                var defIfLogistics = context.LogisticsSite.Where(ls => ls.IsDeleted != 1 && ls.Id == defRecord.LogisticsSiteId).SingleOrDefault();
                defRecord.Province = defIfLogistics.SiteProvince;
                defRecord.City = defIfLogistics.SiteCity;
                defRecord.Area = defIfLogistics.SiteArea;
                defRecord.Address = defIfLogistics.SiteAddress;
                context.SaveChanges();
            }
            return defRecord;
        }

        public void EditDefAddress(CustomerAddress customerAddress)
        {

        }
        /// <summary>
        /// 根据会员Id获得其对应的折扣
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public decimal GetDiscountRateByMemberId(string memberId)
        {
            decimal result = 1;
            var customer = context.Customers.FirstOrDefault(o => o.Id == memberId);
            if (customer != null)
            {
                var memberLevel = context.MemberLevel.FirstOrDefault(o => o.MemberLevelValue == customer.MemberLevelValue);
                if (memberLevel != null)
                {
                    result = memberLevel.DiscountRate;
                }
            }
            return result;
        }
        /// <summary>
        /// 根据Id获得会员对象
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public Customers GetMemberByMemberId(string memberId)
        {
            var customer = context.Customers.FirstOrDefault(o => o.Id == memberId && o.IsDeleted != 1);
            return customer;
        }
        /// <summary>
        /// 获取会员的成长值记录
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public List<MemberGrowthValueLog> GetMemberGrowthValueLogById(string memberId, PageInfo pageinfo, out int totalCount)
        {
            totalCount = context.MemberGrowthValueLog.Where(p => p.MemberId.Equals(memberId) && p.IsDeleted != 1).Count();
            return context.MemberGrowthValueLog.Where(p => p.MemberId.Equals(memberId) && p.IsDeleted != 1).OrderByDescending(p => p.CreatedOn).Skip((pageinfo.Page - 1) * pageinfo.Rows).Take(pageinfo.Rows).ToList();
        }
        /// <summary>
        /// 获取会员的积分记录
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public List<MemberIntegralLog> GetMemberIntegralLogById(string memberId, PageInfo pageinfo, out int totalCount)
        {
            totalCount = context.MemberIntegralLog.Where(p => p.MemberId.Equals(memberId) && p.IsDeleted != 1).Count();
            return context.MemberIntegralLog.Where(p => p.MemberId.Equals(memberId) && p.IsDeleted != 1).OrderByDescending(p => p.CreatedOn).Skip((pageinfo.Page - 1) * pageinfo.Rows).Take(pageinfo.Rows).ToList();
        }
        /// <summary>
        /// 统计会员数量
        /// </summary>
        /// <returns></returns>
        public int GetCustomersCount(int type, string data)
        {
            int result = 0;
            switch (type)
            {
                case 1:
                    result = context.Customers.Where(p => p.IsDeleted != 1).Count();
                    break;
                case 2:
                    result = context.Customers.Where(p => data.Contains(p.MemberLevelValue.ToString()) && p.IsDeleted != 1).Count();
                    break;
            }
            return result;
        }

    }
}
