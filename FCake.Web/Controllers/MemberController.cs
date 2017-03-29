using FCake.Bll;
using FCake.Bll.Services;
using FCake.Domain.Entities;
using FCake.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FCake.Core.Common;
using FCake.Core.MvcCommon;
using System.Text.RegularExpressions;
using FCake.Bll.Helper;
namespace FCake.Web.Controllers
{
    [Authorize]
    public class MemberController : BaseController
    {
        private readonly MemberLevelService _memberLevelService = new MemberLevelService();
        private readonly CouponsService _couponsService = new CouponsService();
        private readonly GiftCardsService _giftCardsService = new GiftCardsService();
        private readonly GiftCardDetailService _giftCardDetailService = new GiftCardDetailService();
        #region 我的账户
        public ActionResult Index(int pageSize = 5, int pageIndex = 1)
        {
            var userId = CurrentMember.MemberId.ToString();
            var user = new CustomersService().GetById(userId);
            //var model = new OrderService().GetOrdersByCustomerId(userId);

            ViewBag.FullName =CurrentMember.DisplayName;
            ViewBag.CustomerType = user.CustomerType;
            if (user == null)
                user = new Customers();
            ViewBag.Member =user;
            ViewBag.MemberLevel = _memberLevelService.GetMemberLevelByLevelVal(user.MemberLevelValue);

            int count = 0;
            var data = new OrderService().GetOrdersByCustomerId(userId, out count, pageSize, pageIndex);
            PageHelper.PagerInfo pager = new PageHelper.PagerInfo();
            pager.CurrentPageIndex = pageIndex;
            pager.PageSize = pageSize;
            pager.RecordCount = count;

            PageHelper.PagerQuery<PageHelper.PagerInfo, List<FCake.Domain.Entities.Orders>> pageInfo = new PageHelper.PagerQuery<PageHelper.PagerInfo, List<FCake.Domain.Entities.Orders>>(pager, data);

            return View(pageInfo);
        }
        /// <summary>
        /// 取消订单，主要用于退回钱包支付金额
        /// </summary>
        /// <param name="orderNo"></param>
        /// <returns></returns>
        public ActionResult CancelOrder(string orderNo)
        {
            OrderService os = new OrderService();
            var result = os.CancelOrderByNo(orderNo,1);
            return Json(result);
        }
        #endregion

        #region 个人信息

        public ActionResult Member()
        {
            var userId = CurrentMember.MemberId.ToString();
            CustomerAddress defAddr;
            Product cake;

            var member = new MemberService().GetMemberInfo(userId, out defAddr, out cake);

            ViewBag.Province = defAddr == null ? "" : defAddr.Province;
            ViewBag.City = defAddr == null ? "" : defAddr.City;
            ViewBag.Area = defAddr == null ? "" : defAddr.Area;
            ViewBag.Address = defAddr == null ? "" : defAddr.Address;

            ViewBag.FavoriteCake = cake == null ? "" : cake.Id;

            return View(member);
        }
        /// <summary>
        /// 保存会员个人信息
        /// </summary>
        /// <param name="newEntity"></param>
        /// <param name="ddlProvince"></param>
        /// <param name="ddlCity"></param>
        /// <param name="ddlArea"></param>
        /// <param name="ttbAddress"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonNetResult SaveMemberInfo(Customers newEntity, string ddlProvince, string ddlCity, string ddlArea, string ttbAddress)
        {
            newEntity.Mobile = newEntity.Mobile.Trim();
            if (newEntity.Mobile.IsNullOrTrimEmpty())
                return new JsonNetResult(new OpResult() { Successed = false, Message = "手机不能为空" });
            if (new Regex(@"^1[3|5|7|8|][0-9]{9}$").IsMatch(newEntity.Mobile) == false)
            {
                return new JsonNetResult(new OpResult() { Successed = false, Message = "手机格式不正确" });
            }

            var userId = CurrentMember.MemberId.ToString();

            var code = Request.Params["Code"];
            var customer = new MemberService().GetMember(userId);
            if (customer == null)
            {
                return new JsonNetResult(new OpResult() { Successed = false, Message = "找不到会员信息" });
            }
            else
            {
                if (customer.Mobile.IsNullOrTrimEmpty())
                {
                    return new JsonNetResult(new OpResult() { Successed = false, Message = "找不到会员手机号" });
                }
                if (customer.Mobile != newEntity.Mobile)
                { //当修改手机（账户名）时
                    var r = new PhoneCodeService().CheckMobileCode(newEntity.Mobile, code);
                    if (r == false)
                    {
                        return new JsonNetResult(new OpResult() { Successed = false, Message = "修改手机号时，验证码错误" });
                    }
                }
                var result = new MemberService().SaveMemberInfo(userId, newEntity, ddlProvince, ddlCity, ddlArea, ttbAddress);
                //更新一下用户名称
                CurrentMember.ReSetMemberSession(CurrentMember.MemberId);
                return new JsonNetResult(result);
            }

        }
        /// <summary>
        /// 获取用户记录的最爱蛋糕，该蛋糕有可能已经下架，下架仍然显示
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult GetFavoriteCakeByPid(string id)
        {
            var result = new MemberService().GetFavoriteCakeByPid(id);
            return Json(new { Id = result.Id, Name = result.Name });
        }
        #endregion

        #region 修改密码
        public ActionResult ModifyPWD()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ModifyPassword(string oldPwd, string newPwd)
        {
            var userId = CurrentMember.MemberId.ToString();
            var result = new PassportService().ModifyPassword(userId, oldPwd, newPwd);
            return Json(result);
        }
        #endregion

        #region 收获地址
        public ActionResult DeliverAddress()
        {
            var customerId = CurrentMember.MemberId.ToString();
            var model = new CustomerAddressService().GetCustomerAddressesById(customerId, 0);
            return View(model);
        }
        [HttpPost]
        public ActionResult GetOneAddress(string addressId)
        {
            var address = new CustomerAddressService().GetOneAddress(addressId);
            return Json(address);
        }

        /// <summary>
        /// 删除收获地址
        /// </summary>
        /// <param name="addressId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DelAddress(string addressId)
        {
            var result = new CustomerAddressService().DelAddress(addressId);
            return Json(new { validate = result.IsNullOrEmpty(), msg = result });
        }

        /// <summary>
        /// 保存用户地址
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SaveAddress(FormCollection c)
        {
            OpResult result = new OpResult();
            var currentUserID = CurrentMember.MemberId.ToString();
            result = new CustomerAddressService().SaveWebCustomerAddress(currentUserID, new CustomerAddress
            {
                Address = c["address"],
                Area = c["area"],
                City = c["city"],
                Province = c["province"],
                ZipCode = c["code"].IsNullOrTrimEmpty() ? null : int.Parse(c["code"]) as Nullable<int>,
                ReceiverTel = c["tel"],
                ReceiverMobile = c["mobile"],
                Receiver = c["receiver"],
                IsDef = c["isDef"].ToInt32() == 1 ? 0 : 1,
                Id = c["id"]
            });
            return Json(new { validate = result.Successed, msg = result.Message, address = result.Data });
        }
        /// <summary>
        /// 设置非默认地址为默认地址
        /// </summary>
        /// <param name="addressId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ModifyAsDefAddress(string addressId)
        {
            var userId = CurrentMember.MemberId.ToString();
            var result = new CustomerAddressService().ModifyAsDefAddress(addressId, userId);
            return Json(result);
        }

        #endregion

        #region 配送范围
        public ActionResult DistributionRange()
        {
            return View();
        }
        #endregion

        #region 绑定新手机
        public ActionResult BindNewPhone()
        {
            var userId = CurrentMember.MemberId.ToString();
            CustomerAddress defAddr;
            Product cake;

            var member = new MemberService().GetMemberInfo(userId, out defAddr, out cake);

            ViewBag.Province = defAddr == null ? "" : defAddr.Province;
            ViewBag.City = defAddr == null ? "" : defAddr.City;
            ViewBag.Area = defAddr == null ? "" : defAddr.Area;
            ViewBag.Address = defAddr == null ? "" : defAddr.Address;

            ViewBag.FavoriteCake = cake == null ? "" : cake.Id;

            return View(member);
        }

        [HttpPost]
        public JsonNetResult SaveNewPhone(Customers newEntity)
        {
            newEntity.Mobile = newEntity.Mobile.Trim();
            if (newEntity.Mobile.IsNullOrTrimEmpty())
                return new JsonNetResult(new OpResult() { Successed = false, Message = "手机不能为空" });
            if (new Regex(@"^1[3|5|7|8|][0-9]{9}$").IsMatch(newEntity.Mobile) == false)
            {
                return new JsonNetResult(new OpResult() { Successed = false, Message = "手机格式不正确" });
            }

            var userId = CurrentMember.MemberId.ToString();

            var code = Request.Params["Code"];
            var customer = new MemberService().GetMember(userId);
            if (customer == null)
            {
                return new JsonNetResult(new OpResult() { Successed = false, Message = "找不到会员信息" });
            }
            else
            {
                if (customer.Mobile.IsNullOrTrimEmpty())
                {
                    return new JsonNetResult(new OpResult() { Successed = false, Message = "找不到会员手机号" });
                }
                if (customer.Mobile != newEntity.Mobile)
                { //当修改手机（账户名）时
                    var r = new PhoneCodeService().CheckMobileCode(newEntity.Mobile, code);
                    if (r == false)
                    {
                        return new JsonNetResult(new OpResult() { Successed = false, Message = "验证码错误" });
                    }
                    else
                    {
                        return new JsonNetResult(new MemberService().SaveNewPhone(userId, newEntity));
                    }
                }
                else
                {
                    return new JsonNetResult(new OpResult() { Successed = false, Message = "该手机号已注册" });
                }

                var result = new OpResult() { Successed = false, Message = "绑定失败" };
                return new JsonNetResult(result);
            }
        }
        #endregion 


        public ActionResult MyCoupons(int pageSize = 5, int pageIndex = 1)
        {
            int count = 0;
            var myCouponList = _couponsService.GetCouponDetailPagingByMemberId(CurrentMember.MemberId, out count, pageSize, pageIndex);
            PageHelper.PagerInfo pager = new PageHelper.PagerInfo();
            pager.CurrentPageIndex = pageIndex;
            pager.PageSize = pageSize;
            pager.RecordCount = count;

            PageHelper.PagerQuery<PageHelper.PagerInfo, List<CouponDetail>> pageInfo = new PageHelper.PagerQuery<PageHelper.PagerInfo, List<CouponDetail>>(pager, myCouponList);
            return View(pageInfo);
        }

        public ActionResult MyGiftCard(int pageSize = 5, int pageIndex = 1)
        {
            int count = 0;
            var myGiftCardList = _giftCardDetailService.GetGiftCardDetailPagingByUsedMemberId(CurrentMember.MemberId, out count, pageSize, pageIndex);
            PageHelper.PagerInfo pager = new PageHelper.PagerInfo();
            pager.CurrentPageIndex = pageIndex;
            pager.PageSize = pageSize;
            pager.RecordCount = count;

            PageHelper.PagerQuery<PageHelper.PagerInfo, List<GiftCardDetail>> pageInfo = new PageHelper.PagerQuery<PageHelper.PagerInfo, List<GiftCardDetail>>(pager, myGiftCardList);
            return View(pageInfo);
        }

        public ActionResult MemberRule()
        {
            return View();
        }
    }
}
