using FCake.Bll;
using FCake.Bll.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FCake.Web.Controllers
{
    /// <summary>
    /// PC端订单处理公共方法
    /// </summary>
    public class CommonOrderController : BaseController
    {
        private readonly CouponsService _couponsService = new CouponsService();
        private readonly GiftCardDetailService _giftCardDetailService = new GiftCardDetailService();
        public string CurrMemberId
        {
            get { return CurrentMember.MemberId; }
        }

        /// <summary>
        /// 获得当前会员所有可用的优惠券
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetEnabledCouponDetailByMemberId()
        {
            var result = _couponsService.GetEnabledCouponDetailByMemberId(this.CurrMemberId);
            return Json(result);
        }
        /// <summary>
        /// 根据输入的优惠券号绑定优惠券
        /// </summary>
        /// <param name="couponSN"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult BindCouponDetailByCouponSN(string couponSN)
        {
            var result = _couponsService.BindCouponDetailByCouponSN(couponSN, this.CurrMemberId);
            return Json(result);
        }
        /// <summary>
        /// 通过代金卡卡号和密码验证代金卡是否允许使用
        /// </summary>
        /// <param name="giftCardSN"></param>
        /// <param name="giftCardSPwd"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult VerificationGiftCardAllowUse(string giftCardSN, string giftCardSPwd)
        {
            var result = _giftCardDetailService.VerificationGiftCardAllowUse(giftCardSN, giftCardSPwd);
            return Json(result);
        }
    }
}
