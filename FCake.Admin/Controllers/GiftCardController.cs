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
    public class GiftCardController : BaseController
    {
        GiftCardsService cos = new GiftCardsService();
        PermissionService psv = new PermissionService();
        int totalCount = 0;

        #region 代金卡批次信息
        [CheckPermission(controlName = "GiftCard", actionName = "GiftCards", permissionCode = "view")]
        //代金卡订单网格查看
        public ActionResult GiftCards()
        {
            var list = psv.GetPermissionCodes("GiftCard", "GiftCards", UserCache.CurrentUser.Id);
            var dicName = "Status";
            if (list.Contains("review"))
            {
                dicName = "StatusReview";
            }
            ViewBag.dicName = dicName;
            return View();
        }
        /// <summary>
        /// 获取代金卡批次数据
        /// </summary>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        [HttpPost]
        [CheckPermission(controlName = "GiftCard", actionName = "GiftCards", permissionCode = "view")]
        public ActionResult GetGiftCards(int page = 1, int rows = 20)
        {
            var pageInfo = new PageInfo() { Page = page, Rows = rows };
            var result = cos.GetGiftCardAll(Helper.UserCache.CurrentUser.Id, out totalCount, pageInfo);
            return Json(new { total = totalCount, rows = result });
        }
        /// <summary>
        /// 根据批次状态筛选数据
        /// </summary>
        /// <param name="giftCard"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        [HttpPost]
        [CheckPermission(controlName = "GiftCard", actionName = "GiftCards", permissionCode = "view")]
        public ActionResult QueryByAuditStatus(GiftCards giftCard, int page = 1, int rows = 20)
        {
            var pageInfo = new PageInfo() { Page = page, Rows = rows };
            var result = cos.FilterByAuditStatus(giftCard.ReviewStatus, out totalCount, pageInfo);
            return Json(new { total = totalCount, rows = result });
        }
        /// <summary>
        /// 审核批次订单，通过生成代金卡，不通过不生成，并改状态
        /// </summary>
        /// <param name="giftCardId"></param>
        /// <param name="isPass"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "GiftCard", actionName = "GiftCards", permissionCode = "review")]
        public ActionResult AuditGiftCards(string giftCardId, bool isPass)
        {
            string modifyUserId = UserCache.CurrentUser.Id;
            var couponOrderStatus = (short)Domain.Enums.GiftCardReviewStatus.ReviewPending;
            if (isPass == true)
            {
                couponOrderStatus = (short)Domain.Enums.GiftCardReviewStatus.ReviewPass;
            }
            else
            {
                couponOrderStatus = (short)Domain.Enums.GiftCardReviewStatus.ReviewReject;
            }
            var result = cos.AuditGiftCardById(giftCardId, couponOrderStatus, modifyUserId);
            return Json(result);
        }

        //代金卡批次订单输入页面
        [CheckPermission(controlName = "GiftCard", actionName = "GiftCards", permissionCode = "add")]
        public ActionResult GiftCardEdit()
        {
            return View();
        }

        /// <summary>
        /// 代金卡批次订单生成
        /// </summary>
        /// <param name="couponsCount"></param>
        /// <param name="denomination"></param>
        /// <param name="allowBeginDate"></param>
        /// <param name="allowEndDate"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        [HttpPost]
        [CheckPermission(controlName = "GiftCard", actionName = "GiftCards", permissionCode = "add")]
        public ActionResult GiftCardsGenerate(GiftCards giftCard)
        {
            if (giftCard.EndValidDate != new DateTime())
            {
                giftCard.EndValidDate = DateTime.Parse(giftCard.EndValidDate.ToString("yyyy-MM-dd 23:59:59"));
            }
            string createdBy = UserCache.CurrentUser.UserName;
            GiftCards card = new GiftCards(giftCard.Quantity, giftCard.Denomination, giftCard.BeginValidDate, giftCard.EndValidDate, giftCard.SalesMoney, createdBy);
            card.Title = string.IsNullOrEmpty(giftCard.Title) ? "枫客代金卡" : giftCard.Title;
            card.GiftBatch = CommonRules.CommonNoRules("giftbatch");
            var result = cos.CreateNewOrder(card);
            return Json(result);
        }
        #endregion

        #region 代金卡信息

        //代金卡信息查看页面
        public ActionResult GiftCardDetail()
        {
            return View();
        }
        /// <summary>
        /// 获取代金卡数据
        /// </summary>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "GiftCard", actionName = "GiftCardDetail", permissionCode = "view")]
        public ActionResult GetAuditGiftCardsData(string giftBatch, string giftCardNo, DateTime? beginTime, DateTime? endTime, decimal? denomination, int page = 1, int rows = 30)
        {
            GiftCardDetailService cs = new GiftCardDetailService();
            var pageInfo = new PageInfo() { Page = page, Rows = rows };
            //var result = cs.GetCouponAll(out totalCount, pageInfo);
            var result = cs.GetGiftCardsByTime(giftBatch, giftCardNo, beginTime, endTime, denomination, out totalCount, pageInfo);
            return new JsonNetResult(new { total = totalCount, rows = result });
        }
        /// <summary>
        /// Excel导出
        /// </summary>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "GiftCard", actionName = "GiftCardDetail", permissionCode = "view")]
        public ActionResult ExportExcel(DateTime? beginTime, DateTime? endTime, decimal? denomination, string giftBatch, string giftCardNo)
        {
            GiftCardDetailService cs = new GiftCardDetailService();
            var result = cs.ExportExcel(beginTime, endTime, denomination, giftBatch, giftCardNo);
            return Json(result);
        }

        /// <summary>
        /// 通过代金卡卡号和密码验证代金卡是否允许使用
        /// </summary>
        /// <param name="giftCardSN"></param>
        /// <param name="giftCardSPwd"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "GiftCard", actionName = "GiftCardDetail", permissionCode = "view")]
        [HttpPost]
        public ActionResult VerificationGiftCardAllowUse(string giftCardSN, string giftCardSPwd)
        {
            var result = new GiftCardDetailService().VerificationGiftCardAllowUse(giftCardSN, giftCardSPwd);
            return Json(result);
        }
        [CheckPermission(controlName = "GiftCard", actionName = "GiftCardDetail", permissionCode = "view")]
        public ActionResult GetGiftCardsToDatagridByMemberId(string memberId, int page = 1, int rows = 10)
        {
            PageInfo pageinfo = new PageInfo() { Page = page, Rows = rows };
            int totalcount = 0;
            var result = new GiftCardDetailService().GetGiftCardToDataGridByMemberId(memberId, out totalcount, pageinfo);
            return Json(new { total = totalcount, rows = result });
        }
        #endregion
    }
}
