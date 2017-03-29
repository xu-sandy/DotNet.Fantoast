using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Web;
using System.Web.Mvc;
using FCake.Admin.Models;
using System.Data.Entity;
using FCake.Domain.Entities;
using FCake.Domain;
using FCake.Core.MvcCommon;
using FCake.Bll;
using FCake.Admin.Helper;
using FCake.Domain.Common;

namespace FCake.Admin.Controllers
{
    /// <summary>
    /// 优惠券
    /// </summary>
    public class CouponsController : BaseController
    {
        FCake.Bll.Services.CouponsService CouponsService = new Bll.Services.CouponsService();
        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// 获取已有的优惠券信息
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">页大小</param>
        /// <returns></returns>
        [CheckPermission(controlName = "Coupons", actionName = "Index", permissionCode = "view")]
        public ActionResult GetCoupons(int page = 1, int rows = 20)
        {
            PageInfo pageInfo = new PageInfo() { Page = page, Rows = rows };
            int totalCount = 0;
            var result = CouponsService.GetCouponsByPageInfo(pageInfo, out totalCount);
            return Json(new { total = totalCount, rows = result });
        }
        /// <summary>
        /// 创建优惠券保存页面
        /// </summary>
        /// <returns></returns>
        [CheckPermission(controlName = "Coupons", actionName = "Index", permissionCode = "edit")]
        public ActionResult Save(string id)
        {
            var coupons = CouponsService.GetCouponsByWhere(p => p.Id.Equals(id));
            var coupon = new Coupons();
            foreach (var item in coupons)
            {
                coupon = item;
            }
            return View(coupon);
        }
        /// <summary>
        /// 选择指定客户
        /// </summary>
        /// <returns></returns>
        public ActionResult SelectCustomer()
        {
            return View();
        }
        /// <summary>
        /// 获取所有的会员等级
        /// </summary>
        /// <returns></returns>
        [CheckPermission(controlName = "Coupons", actionName = "Index", permissionCode = "view")]
        public ActionResult GetAllMemberLevel()
        {
            //获取所有会员等级信息
            var memberLevels = new FCake.Bll.MemberLevelService().GetAllMemberLevel();
            return Json(memberLevels);
        }
        /// <summary>
        /// 加载客户列表
        /// </summary>
        /// <param name="name"></param>
        /// <param name="tel"></param>
        /// <returns></returns>
        [HttpPost]
        [CheckPermission(isRelease = true)]
        public ActionResult LoadCustomerList(string name, string tel, int page = 1, int rows = 10)
        {
            var data = new CustomersService().SearchCustomersByPhoneOrder(name, tel, page, rows);
            return Json(data);
        }
        /// <summary>
        /// 过滤用户数据
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="name"></param>
        /// <param name="tel"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        [CheckPermission(isRelease = true)]
        [HttpPost]
        public ActionResult LoadCustomersOutIds(string ids, string name, string tel, int page = 1, int rows = 10)
        {
            PageInfo pageInfo = new PageInfo() { Page = page, Rows = rows };
            var data = new CustomersService().SearchCustomersByWhere(ids, name, tel, pageInfo);
            return Json(data);
        }
        /// <summary>
        /// 已选择的客户加载
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [CheckPermission(isRelease=true)]
        public ActionResult PreLoadCustomers(string ids, int page = 1, int rows = 100)
        {
            var totalCount = 0;
            PageInfo pageInfo = new PageInfo() { Page = page, Rows = rows };
            if (ids == null || ids == "")
            {
                return Json(new { total = totalCount, rows = "" });
            }
            var result = new CustomersService().GetCustomerIdByIds(ids, out totalCount, pageInfo);
            return Json(new { total = totalCount, rows = result });

        }
        /// <summary>
        /// 保存优惠券信息
        /// </summary>
        /// <param name="coupons"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Coupons", actionName = "Index", permissionCode = "view")]
        [HttpPost]
        public ActionResult GenerateCoupons(Coupons coupons)
        {
            OpResult result = new OpResult();
            //填充创建信息
            coupons.CreatedOn = DateTime.Now;
            coupons.CreatedBy = UserCache.CurrentUser.Id;
            coupons.IsDeleted = 0;
            coupons.EndValidDate = coupons.EndValidDate.AddHours(23).AddMinutes(59).AddSeconds(59);
            //执行插入返回结果
            result = CouponsService.CreateCoupons(coupons);
            if (result.Successed && coupons.Status == 1 && coupons.IsSendSMS == 1 && result.Data != null)
            {
                var exportResult = CouponsService.ExportCouponsSendSMSMobile((List<string>)result.Data);
                result.Successed = exportResult.Successed;
                result.Message = exportResult.Message;
                result.Data = exportResult.Data;
            }
            return Json(result);
        }
        /// <summary>
        /// 导出需发送短信的手机列表
        /// </summary>
        /// <param name="couponId"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Coupons", actionName = "Index", permissionCode = "view")]
        [HttpPost]
        public ActionResult ExportMobileList(string couponId)
        {
            OpResult result = new OpResult();
            var mobileList = CouponsService.GetMobileListByCouponId(couponId);
            result = CouponsService.ExportCouponsSendSMSMobile(mobileList);
            return Json(result);
        }
        /// <summary>
        /// 设置优惠券短信为已发送状态
        /// </summary>
        /// <param name="couponId"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Coupons", actionName = "Index", permissionCode = "view")]
        [HttpPost]
        public ActionResult SetCouponSMSAlreadySend(string couponId)
        {
            var result = CouponsService.SetCouponSMSAlreadySend(couponId);
            return Json(result);
        }

        /// <summary>
        /// 发放优惠券
        /// </summary>
        /// <param name="coupons"></param>
        /// <returns></returns>
        [HttpPost]
        [CheckPermission(controlName = "Coupons", actionName = "Save", permissionCode = "edit")]
        public ActionResult GenerateCouponsDetails(string couponId)
        {
            OpResult result = new OpResult() { Successed = false };
            //填充创建信息
            List<Coupons> coupons = CouponsService.GetCouponsByWhere(a => a.Id.Equals(couponId));
            foreach (var item in coupons)
            {
                if (item.Status == 0)
                {
                    item.Status = 1;
                    item.ModifiedOn = DateTime.Now;
                    item.ModifiedBy = UserCache.CurrentUser.Id;
                    //执行插入返回结果
                    result = CouponsService.CreateCoupons(item);
                }
                else
                {
                    result.Message = "指定优惠券已发放!";
                }

            }
            return Json(result);
        }
        /// <summary>
        /// 删除优惠券信息
        /// </summary>
        /// <param name="couponId"></param>
        /// <returns></returns>
        [HttpPost]
        [CheckPermission(controlName = "Coupons", actionName = "Save", permissionCode = "edit")]
        public ActionResult dropCoupons(string couponId)
        {
            OpResult result = new OpResult() { Successed = false };
            //填充创建信息
            result = CouponsService.DropCoupons(couponId, UserCache.CurrentUser.Id);

            return Json(result);
        }

        [CheckPermission(controlName = "Coupons", actionName = "GenerateCoupons", permissionCode = "view")]
        public ActionResult GenerateCoupons()
        {
            return View();
        }
        /// <summary>
        /// 优惠券明细
        /// </summary>
        /// <returns></returns>
        [CheckPermission(controlName = "Coupons", actionName = "CouponsDetails", permissionCode = "view")]
        public ActionResult CouponsDetails()
        {
            return View();
        }
        ///// <summary>
        ///// 优惠券明细数据查询
        ///// </summary>
        ///// <returns></returns>
        //public ActionResult GetCouponsDetails(int pageIndex=1,int pageSize=20)
        //{
        //    PageInfo pageInfo = new PageInfo() { Page = 1, Rows = pageSize };
        //    int totalCount=0;
        //    var datas = CouponsService.GetCouponDetailByPageInfo(pageInfo, out totalCount);
        //    return Json(new{ total=totalCount,rows=datas});
        //}
        /// <summary>
        /// 根据条件过滤优惠券明细数据
        /// </summary>
        /// <param name="couponsNo"></param>
        /// <param name="user"></param>
        /// <param name="status"></param>
        /// <param name="faceVal"></param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Coupons", actionName = "CouponsDetails", permissionCode = "view")]
        public ActionResult GetCouponsDetails(CouponDetail coupondetail, int page = 1, int rows = 20)
        {
            PageInfo pageInfo = new PageInfo() { Page = page, Rows = rows };
            int totalCount = 0;
            var result = CouponsService.GetCouponsDetails(coupondetail, pageInfo, out totalCount);
            return Json(new { total = totalCount, rows = result });
        }
        /// <summary>
        /// 根据客户id取优惠券数据
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Coupons", actionName = "CouponsDetails", permissionCode = "view")]
        public ActionResult GetCouponsByCustomerId(string customerId)
        {
            OpResult result = new OpResult() { Successed = false };
            try
            {
                if (customerId == null || customerId == "")
                {
                    result.Message = "用户为空！";
                    return Json(result);
                }
                result.Data = CouponsService.GetEnabledCouponDetailByMemberId(customerId).OrderBy(p => p.ConditionMoney);
                result.Successed = true;
            }
            catch (Exception e)
            {
                result.Message = "获取用户优惠券失败！";
            }
            return Json(result);
        }
        /// <summary>
        /// 根据会员id查询所有优惠券填充网格
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Coupons", actionName = "CouponsDetails", permissionCode = "view")]
        public ActionResult GetCouponsToDatagridByCustomerId(string customerId, int page = 1, int rows = 10)
        {
            PageInfo pageinfo = new PageInfo() { Page = page, Rows = rows };
            int totalCount = 0;
            var result = CouponsService.GetCouponDetailToDatagridByMemberId(customerId, out totalCount, pageinfo);
            return Json(new { total = totalCount, rows = result });
        }

        /// <summary>
        /// 将未绑定的额优惠券绑定到用户
        /// </summary>
        /// <param name="couponNo"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Coupons", actionName = "CouponsDetails", permissionCode = "view")]
        public ActionResult BindCouponByCouponNo(string couponSN, string customerId)
        {
            var result = CouponsService.BindCouponDetailByCouponSN(couponSN, customerId);
            return Json(result);
        }
        /// <summary>
        /// 优惠券导出
        /// </summary>
        /// <param name="couponsDetail"></param>
        /// <param name="memberMobile"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Coupons", actionName = "CouponsDetails", permissionCode = "view")]
        public ActionResult CouponsExportExcel(CouponDetail couponsDetail, string memberMobile)
        {
            EFDbContext _context = new EFDbContext();
            var memberMobiles = "";
            if (memberMobile != "")
            {
                var customer = _context.Customers.Where(p => p.Mobile.Contains(memberMobile) && p.IsDeleted != 1);
                if (customer != null)
                {

                    foreach (var item in customer)
                    {
                        memberMobiles += item.Id + ",";
                    }
                }
            }
            OpResult result = CouponsService.CouponsExport(couponsDetail, memberMobiles);
            return Json(result);
        }
        /// <summary>
        /// 获取保存代金卡的发放信息
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Coupons", actionName = "GenerateCoupons", permissionCode = "view")]
        public ActionResult GetCreatCouponsInfo(string Id)
        {
            CustomersService _cservice = new CustomersService();
            var coupon = CouponsService.GetCreateCouponInfo(Id);
            if (coupon != null)
            {
                var conditionText = "无条件使用";
                if (coupon.ConditionMoney > 0)
                {
                    conditionText = "满 " + String.Format("{0:F}", coupon.ConditionMoney) + "元使用";
                }
                var givenText = "";
                if (coupon.GiveWay == 1)
                {
                    switch (coupon.GivenObjectType)
                    {
                        case 1:
                            givenText = "全部";
                            break;
                        case 2:
                            var allMemberLevel = new FCake.Bll.MemberLevelService().GetAllMemberLevel();
                            var memberLevelText = "";
                            var ids = new List<string>();
                            if (coupon.GivenObjectIds.Contains(","))
                            {
                                ids = new List<string>(coupon.GivenObjectIds.Split(','));
                            }
                            else
                            {
                                ids.Add(coupon.GivenObjectIds);
                            }
                            foreach (var item in ids)
                            {
                                foreach (var itm in allMemberLevel)
                                {
                                    if (item == itm.MemberLevelValue.ToString())
                                    {
                                        memberLevelText += itm.Title + ",";
                                    }
                                }
                            }

                            givenText = "指定会员类型:" + memberLevelText.Substring(0, memberLevelText.Length - 1);
                            break;
                        case 3:
                            givenText = "指定用户";
                            break;
                        default:
                            break;
                    }
                }
                var cardNumber = coupon.Quantity;
                if (coupon.GiveWay == 1)
                {
                    if (coupon.GivenObjectType == 1 || coupon.GivenObjectType == 2)
                    {
                        cardNumber = _cservice.GetCustomersCount(coupon.GivenObjectType, coupon.GivenObjectIds);
                    }
                }
                return Json(new
                {
                    name = coupon.Title,
                    denomination = coupon.Denomination,
                    beginValidDate = coupon.BeginValidDate.ToString("yyyy-MM-dd"),
                    endValidDate = coupon.EndValidDate.ToString("yyyy-MM-dd"),
                    useCondition = conditionText,
                    cardNumber = cardNumber,//发放数量
                    givenObjectStr = givenText,
                    isSendSMS = coupon.IsSendSMS,
                    smsContent = coupon.SMSContent,
                    ConditionMoney = coupon.ConditionMoney,
                    giveWay = coupon.GiveWay,
                    givenObjectType = coupon.GivenObjectType,
                    givenObjectIds = coupon.GivenObjectIds,
                    Id = coupon.Id,
                    couponBatch = coupon.CouponBatch
                });
            }
            return Json("");
        }
    }
}
