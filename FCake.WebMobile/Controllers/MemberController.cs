using FCake.Bll;
using FCake.Bll.Services;
using FCake.Domain.Entities;
using FCake.WebMobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FCake.Core.Common;
using FCake.Domain.Enums;
using FCake.Core.MvcCommon;
using System.Text.RegularExpressions;
using FCake.Domain.WebModels;
using FCake.Bll.Helper;

namespace FCake.WebMobile.Controllers
{
    [Authorize]
    public class MemberController : Controller
    {
        private readonly MemberLevelService _memberLevelService = new MemberLevelService();
        private readonly CouponsService _couponsService = new CouponsService();
        private readonly GiftCardDetailService _giftCardDetailService = new GiftCardDetailService();
        #region 我的账户
        public ActionResult Index(int pageSize = 5, int pageIndex = 1)
        {
            try
            {
                var userId = CurrentMember.MemberId.ToString();
                var user = new CustomersService().GetById(userId);
                //var model = new OrderService().GetOrdersByCustomerId(userId);

                ViewBag.FullName = CurrentMember.DisplayName;
                ViewBag.CustomerType = user.CustomerType;
                if (user == null)
                    user = new Customers();
                ViewBag.Member = user;
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
            catch
            {
                return View();
            }
        }
        /// <summary>
        /// 滚动到底部加载更多数据
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        [HttpPost]
        public string GetOrdersByScroll(string backendUrl, int pageSize = 5, int pageIndex = 2)
        {
            string result = string.Empty;
            var content = string.Empty;

            try
            {
                var userId = CurrentMember.MemberId.ToString();
                var user = new CustomersService().GetById(userId);

                ViewBag.FullName = CurrentMember.DisplayName;
                ViewBag.CustomerType = user.CustomerType;

                int count = 0;
                var data = new OrderService().GetOrdersByCustomerId(userId, out count, pageSize, pageIndex);
                PageHelper.PagerInfo pager = new PageHelper.PagerInfo();
                pager.CurrentPageIndex = pageIndex;
                pager.PageSize = pageSize;
                pager.RecordCount = count;
                PageHelper.PagerQuery<PageHelper.PagerInfo, List<FCake.Domain.Entities.Orders>> pageInfo = new PageHelper.PagerQuery<PageHelper.PagerInfo, List<FCake.Domain.Entities.Orders>>(pager, data);

                if (pageInfo != null)
                {
                    result = @"<div class='tablecontent' style='height:140px;' onclick='clickOrderTr(""{0}"")'>
                                         <div>
                                           <img src='{1}_min.jpg' width='86' height='86' style='float:left; margin-left:3%;margin-top:7.5px;width:86px;height:86px;' />
                                               <div style='width:63%;margin-left:3%;float:left;'>
                                                  <span class='address_text' style='width:100%;height:36px; float: left;margin-top: 7.5px;font-size:0.825em;'>{2}</span>
                                                  <span class='font_color_tint' style='width:100%; font-size:0.75em;float:left;'>订单总额: ¥{3}</span> 
                                                  <span class='font_color_tint' style='width:100%; font-size:0.75em;float:left;'>共{4}件</span>
                                                  <span class='font_color_tint' style='width:100%; font-size:0.75em;float:left;'>
                                                  {5}
                                                  </span>
                                                  <span class='font_color_tint' style='font-size:0.75em;float:left;width:100%;'>
                                                  {6}
                                                  {7}</span>
                                              </div>
                                         </div>
                                     </div>";
                    var no = string.Empty;
                    var url = string.Empty;
                    var pName = string.Empty;
                    var totalPrice = string.Empty;
                    var allcount = 0;
                    var state = string.Empty;
                    var action = string.Empty;
                    var toAction = string.Empty;


                    foreach (var x in pageInfo.EntityList)
                    {
                        var os = new OrderService();
                        List<CartVM> ods = os.GetDetailByNo<CartVM>(x.No);
                        var od = ods.FirstOrDefault();

                        if (od != null)
                        {
                            no = x.No;
                            url = backendUrl + od.Url;
                            pName = od.PName;
                            totalPrice = x.TotalPrice.ToString("N2");
                            allcount = ods.Count;

                            if (x.Status == 0)
                            {
                                state = " <span style='color:#e81a1a'>等待付款</span>";
                            }
                            else
                            {
                                if (x.Status == OrderStatus.Making || x.Status == OrderStatus.MakeCompleted)
                                {
                                    state = " <span style='color:#1f9941'>排产中</span>";
                                }
                                else
                                {
                                    state = "<span style='color:#1f9941'>" + (FCake.Core.Common.EnumHelper.GetDescription((FCake.Domain.Enums.OrderStatus)x.Status)) + "</span>";
                                }
                            }

                            if (x.Status == FCake.Domain.Enums.OrderStatus.NotPay)
                            {
                                action = "<a class='member_button' href='javascript:void(0)' onclick='GoPay(\"" + x.FeeType + "\",\"" + x.No + "\")' >去付款</a>";
                            }
                            else
                            {
                                action = "";
                            }

                            if (x.TradeStatus == FCake.Domain.Enums.TradeStatus.NotPay && x.ReviewStatus != FCake.Domain.Enums.ReviewStatus.ReviewPass && x.Status != OrderStatus.Canceled)
                            {
                                toAction = "<a class='member_button_1' href='#' onclick='CancelOrder(\"" + x.No + "\")'>取消</a>";
                            }
                            else
                            {
                                toAction = "<a class='member_button_1'href='/Product/Index'>再次购买</a>";
                            }
                        }
                        content += string.Format(result, no, url, pName, totalPrice, allcount, state, action, toAction);
                    }
                }
            }
            catch
            {
            }
            return content;
        }
        /// <summary>
        /// 取消订单，主要用于退回钱包支付金额
        /// </summary>
        /// <param name="orderNo"></param>
        /// <returns></returns>
        public ActionResult CancelOrder(string orderNo)
        {
            OrderService os = new OrderService();
            var result = os.CancelOrderByNo(orderNo, 2);
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

                CurrentMember.ReSetMemberSession(CurrentMember.MemberId);
                return new JsonNetResult(result);
            }
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
        public ActionResult EditAddress(string addressId)
        {
            ViewBag.AddressId = addressId;
            return View();
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
                ZipCode = c["code"].IsNullOrEmpty() ? null : int.Parse(c["code"]) as Nullable<int>,
                ReceiverTel = c["tel"],
                ReceiverMobile = c["mobile"],
                Receiver = c["receiver"],
                IsDef = c["isDef"].ToInt32() == 1 ? 0 : 1,
                Id = c["id"]
            });
            return Json(new { validate = result.Successed, msg = result.Message, Data = result.Data });
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

        #region 个人中心
        public ActionResult PersonalCenter()
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

        public ActionResult Address()
        {
            return View();
        }

        public ActionResult MyCoupons()
        {
            var couponList = _couponsService.GetCouponDetailByMemberId(CurrentMember.MemberId)
                .Where(o => (o.UseState == 0 && o.EndValidDate >= DateTime.Now.AddMonths(-1)) || (o.UseState == 1 && o.UseDate != null && o.UseDate >= DateTime.Now.AddMonths(-1)))
                .OrderByDescending(o => o.EndValidDate).ThenByDescending(o => o.Denomination).ToList();
            ViewBag.CouponList = couponList;
            return View();
        }

        public ActionResult MyGiftCard()
        {
            var giftCardList = _giftCardDetailService.GetGiftCardDetailByUsedMemberId(CurrentMember.MemberId);
            ViewBag.GiftCardList = giftCardList;
            return View();
        }

        public ActionResult MemberRule()
        {
            return View();
        }
    }
}
