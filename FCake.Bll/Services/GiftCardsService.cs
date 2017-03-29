using FCake.Bll;
using FCake.Core.MvcCommon;
using FCake.Domain;
using FCake.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Text;
using System.Text.RegularExpressions;
using FCake.Domain.Common;
using FCake.Core.Common;

namespace FCake.Bll
{
    public class GiftCardsService
    {
        EFDbContext context = new EFDbContext();

        /// <summary>
        /// 生成代金卡批次订单
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public OpResult CreateNewOrder(GiftCards order)
        {
            var result = new OpResult();
            context.GiftCards.Add(order);
            if (context.SaveChanges() > 0)
            {
                result = new OpResult() { Successed = true, Message = "提交成功" };
            }
            else
            {
                result = new OpResult() { Successed = false, Message = "数据对象保存错误" };
            }
            return result;
        }

        /// <summary>
        /// 审核批次订单
        /// </summary>
        /// <param name="giftCardId"></param>
        /// <param name="reviewStatus"></param>
        /// <param name="ModifyUser"></param>
        /// <returns></returns>
        public OpResult AuditGiftCardById(string giftCardId, short reviewStatus, string modifyUserId)
        {
            var result = new OpResult();
            var giftCard = context.GiftCards.FirstOrDefault(co => co.Id == giftCardId);
            giftCard.ReviewStatus = (short)reviewStatus;
            giftCard.ModifiedBy = modifyUserId;
            giftCard.ModifiedOn = DateTime.Now;
            if (context.SaveChanges() > 0)
            {
                if (reviewStatus == (short)Domain.Enums.GiftCardReviewStatus.ReviewPass)
                {
                    ///批量生成卡
                    var cs = new GiftCardDetailService();
                    cs.PublishManyGiftCard(giftCard);
                    result = new OpResult() { Successed = true, Data = giftCard.Quantity };
                }
                if (reviewStatus == (short)Domain.Enums.GiftCardReviewStatus.ReviewReject)
                {
                    result = new OpResult() { Successed = false };
                }
                LogReviewStatus(giftCardId, (int)reviewStatus, modifyUserId);
            }
            return result;
        }
        /// <summary>
        /// 审核日志
        /// </summary>
        /// <param name="couponOrderId"></param>
        /// <param name="status"></param>
        /// <param name="userId"></param>
        private void LogReviewStatus(string couponOrderId, int status, string userId)
        {
            var reviewStatus = new ReviewStatusLog()
            {
                Id = DataHelper.GetSystemID(),
                TableName = "GiftCards",
                TableNameId = couponOrderId,
                CreatedBy = userId,
                CreatedOn = DateTime.Now,
                Status = status
            };
            context.ReviewStatusLog.Add(reviewStatus);
            context.SaveChanges();
        }

        //获取所有代金卡批次数据
        public List<GiftCards> GetGiftCardAll(string currentUserId, out int totalCount, PageInfo pageInfo)
        {
            var psv = new PermissionService();

            var query = context.GiftCards.Where(co => co.IsDeleted != 1);

            totalCount = query.Count();
            var data = query.OrderByDescending(co => co.CreatedOn).Skip((pageInfo.Page - 1) * pageInfo.Rows).Take(pageInfo.Rows).ToList();
            return data;
        }
        //根据批次状态筛选数据
        public List<GiftCards> FilterByAuditStatus(short reviewStatus, out int totalCount, PageInfo pageInfo)
        {
            var query = context.GiftCards.Where(p => p.IsDeleted != 1 && p.ReviewStatus == reviewStatus);
            totalCount = query.Count();
            var data = query.OrderByDescending(co => co.CreatedOn).Skip((pageInfo.Page - 1) * pageInfo.Rows).Take(pageInfo.Rows).ToList();
            return data;
        }
    }
}
