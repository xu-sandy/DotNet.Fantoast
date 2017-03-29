using FCake.Domain.Entities;
using FCake.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using FCake.Core.Common;
using FCake.Core.MvcCommon;
using FCake.Domain.WebModels;

namespace FCake.Bll
{
    public partial class OrderService
    {
        #region 创建订单优惠部分处理
        /// <summary>
        /// 验证订单是否允许被创建
        /// </summary>
        /// <param name="createOrderModel"></param>
        /// <returns></returns>
        public OpResult ValidateOrderIsAllowCreate(CreateOrderModel createOrderModel)
        {
            var result = OpResult.Success();
            if (createOrderModel.RdType == DeliveryType.FixedSite)
            {
                if (createOrderModel.Name.IsNullOrTrimEmpty())
                {
                    result = OpResult.Fail("提货人名称不能为空");
                    return result;
                }
                if (createOrderModel.Mobile.IsNullOrTrimEmpty())
                {
                    result = OpResult.Fail("提货人手机不能为空");
                    return result;
                }
                if (createOrderModel.Mobile.IsMobile())
                {
                    result = OpResult.Fail("提货人手机格式不正确");
                    return result;
                }
            }
            #region 验证优惠部分
            List<string> couponIdList = null;
            if (!string.IsNullOrEmpty(createOrderModel.CouponDetailIds))
            {
                couponIdList = createOrderModel.CouponDetailIds.Split(',').ToList();
                var isHasRepetition = couponIdList.GroupBy(o => o).Where(g => g.Count() > 1).Count() > 0;
                if (isHasRepetition == true)
                {
                    result = OpResult.Fail("不能重复使用同一张优惠券");
                    return result;
                }
            }
            List<string> giftCardIdList = null;
            if (!string.IsNullOrEmpty(createOrderModel.GiftCardDetailIds))
            {
                giftCardIdList = createOrderModel.GiftCardDetailIds.Split(',').ToList();
                var isHasRepetition = giftCardIdList.GroupBy(o => o).Where(g => g.Count() > 1).Count() > 0;
                if (isHasRepetition == true)
                {
                    result = OpResult.Fail("不能重复使用同一张代金卡");
                    return result;
                }
            }

            //验证优惠券是否可以使用
            result = _couponsService.ValidateCouponDetailsIsAllowUse(couponIdList);
            if (result.Successed == false)
                return result;
            //验证代金卡是否可以使用
            result = _giftCardDetailService.ValidateGiftCardDetailAllowUse(giftCardIdList);
            if (result.Successed == false)
                return result;
            #endregion

            return result;
        }

        /// <summary>
        /// 将优惠券设置成已使用状态
        /// </summary>
        /// <param name="couponIdList"></param>
        /// <param name="orderSN"></param>
        /// <param name="isCommit"></param>
        /// <returns></returns>
        public OpResult SetCouponDetailsToUsed(List<string> couponIdList, string orderSN, string usedMemberId, bool isCommit = false)
        {
            var result = OpResult.Success();
            result = _couponsService.ValidateCouponDetailsIsAllowUse(couponIdList);
            if (result.Successed == false)
                return result;
            if (couponIdList != null)
            {
                foreach (var couponId in couponIdList)
                {
                    var couponDetail = DAL.GetQuery<CouponDetail>(o => o.Id == couponId && o.IsDeleted != 1).FirstOrDefault();
                    if (couponDetail != null)
                    {
                        couponDetail.UseState = 1;
                        couponDetail.UseOrderSN = orderSN;
                        couponDetail.UseMemberId = usedMemberId;
                        couponDetail.UseDate = DateTime.Now;


                    }
                }
                if (isCommit)
                {
                    DAL.Commit();
                }
            }

            return result;
        }

        /// <summary>
        /// 将代金卡设置为已使用状态
        /// </summary>
        /// <param name="giftCardDetailIdList"></param>
        /// <param name="orderSN"></param>
        /// <param name="usedMemberId"></param>
        /// <param name="isCommit"></param>
        /// <returns></returns>
        public OpResult SetGiftCardDetailToUsed(List<string> giftCardDetailIdList, string orderSN, string usedMemberId, bool isCommit = false)
        {
            var result = OpResult.Success();
            result = _giftCardDetailService.ValidateGiftCardDetailAllowUse(giftCardDetailIdList);
            if (result.Successed == false)
                return result;
            if (giftCardDetailIdList != null)
            {
                foreach (var giftCardDetailId in giftCardDetailIdList)
                {
                    var giftCardDetail = DAL.GetQuery<GiftCardDetail>(o => o.Id == giftCardDetailId && o.IsDeleted != 1).FirstOrDefault();
                    if (giftCardDetail != null)
                    {
                        giftCardDetail.UseState = 1;
                        giftCardDetail.UseOrderSN = orderSN;
                        giftCardDetail.UseMemberId = usedMemberId;
                        giftCardDetail.UseDate = DateTime.Now;
                    }
                }
                if (isCommit)
                {
                    DAL.Commit();
                }
            }
            return result;
        }
        /// <summary>
        /// 扣除用户积分
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="deductIntegralVal"></param>
        /// <param name="isCommit"></param>
        /// <returns></returns>
        public OpResult DeductMemberIntegral(string memberId, int deductIntegralVal, string orderSN, decimal cashAmount, string currentUserId, bool isCommit = false)
        {
            var result = OpResult.Success();
            var member = DAL.GetQuery<Customers>(o => o.Id == memberId && o.IsDeleted != 1).FirstOrDefault();
            if (member == null)
            {
                result = OpResult.Fail("对不起，找不到该会员");
                return result;
            }
            if (member.Integral < deductIntegralVal)
            {
                result = OpResult.Fail("对不起，会员积分不足");
                return result;
            }
            if (deductIntegralVal < 0)
            {
                result = OpResult.Fail("对不起，扣除的积分不能小于0");
                return result;
            }

            //保存扣除积分记录
            if (deductIntegralVal > 0)
            {
                MemberIntegralLog memberIntegralLog = CreateNewMemberIntegralLog(memberId, orderSN, -deductIntegralVal,
                                                     cashAmount, string.Format("订单号：{0}，使用{1}积分", orderSN, deductIntegralVal));
                DAL.AddOrModify(memberIntegralLog, currentUserId);
            }
            //先记录，再更新，避免记录时获取Context中已更新的数据  
            member.Integral = member.Integral - deductIntegralVal;
            if (isCommit)
            {
                DAL.Commit();
            }

            return result;
        }
        #endregion

        #region 取消订单时退还优惠券、代金卡、积分
        /// <summary>
        /// 退还优惠券
        /// </summary>
        /// <param name="orderNo"></param>
        /// <param name="isCommit"></param>
        /// <returns></returns>
        public OpResult SendBackCouponDetail(string orderNo, bool isCommit = false)
        {
            var result = OpResult.Fail("退还优惠券失败");
            var couponDetailList = context.CouponDetails.Where(o => o.UseOrderSN == orderNo && o.IsDeleted != 1).ToList();
            if (couponDetailList != null)
            {
                foreach (var couponDetail in couponDetailList)
                {
                    couponDetail.UseState = 0;
                    couponDetail.UseMemberId = null;
                    couponDetail.UseOrderSN = null;
                    couponDetail.UseDate = null;
                }
                if (isCommit)
                {
                    context.SaveChanges();
                }
            }
            result = OpResult.Success();
            return result;
        }
        /// <summary>
        /// 退还代金卡
        /// </summary>
        /// <param name="orderNo"></param>
        /// <param name="isCommit"></param>
        /// <returns></returns>
        public OpResult SendBackGiftCardDetail(string orderNo, bool isCommit = false)
        {
            var result = OpResult.Fail("退还代金卡失败");
            var giftCardDetailList = context.GiftCardDetail.Where(o => o.UseOrderSN == orderNo && o.IsDeleted != 1).ToList();
            if (giftCardDetailList != null)
            {
                foreach (var giftCardDetail in giftCardDetailList)
                {
                    if (giftCardDetail.UseState == 1)
                    {
                        giftCardDetail.UseState = 0;
                    }
                    giftCardDetail.UseMemberId = null;
                    giftCardDetail.UseOrderSN = null;
                    giftCardDetail.UseDate = null;
                }
                if (isCommit)
                {
                    context.SaveChanges();
                }
            }
            result = OpResult.Success();
            return result;
        }
        /// <summary>
        /// 退还积分
        /// </summary>
        /// <param name="orderNo"></param>
        /// <param name="isCommit"></param>
        /// <returns></returns>
        public OpResult SendBackIntegral(string orderNo, bool isCommit = false)
        {
            var result = OpResult.Fail("退还积分失败");
            var order = context.Orders.Where(o => o.No == orderNo && o.IsDeleted != 1).FirstOrDefault();
            if (order != null)
            {
                var member = context.Customers.Where(o => o.Id == order.CustomerId && o.IsDeleted != 1).FirstOrDefault();
                if (member != null)
                {
                    if (order.UsedIntegralVal > 0)
                    {
                        //保存退还积分记录
                        if (order.UsedIntegralVal > 0)
                        {
                            MemberIntegralLog memberIntegralLog = CreateNewMemberIntegralLog(order.CustomerId, order.No, order.UsedIntegralVal,
                                                                 order.ActualPay, string.Format("取消订单，订单号：{0}，退还{1}积分", order.No, order.UsedIntegralVal));
                            memberIntegralLog.Id = CommonRules.GUID;
                            context.MemberIntegralLog.Add(memberIntegralLog);
                        }
                        //先记录，再更新，避免记录时获取Context中已更新的数据     
                        member.Integral = member.Integral + order.UsedIntegralVal;
                        if (isCommit)
                        {
                            context.SaveChanges();
                        }
                    }
                }
            }
            return result;
        }
        #endregion

        #region 订单完成时更新会员积分、成长值、现金消费金额、会员等级
        /// <summary>
        /// 订单完成时更新会员积分、成长值、现金消费金额、会员等级
        /// </summary>
        /// <param name="orderNo"></param>
        /// <returns></returns>
        public OpResult CompletedOrderUpdateMemberInfo(string orderNo)
        {
            var result = OpResult.Fail("订单完成时，更新会员信息失败");
            try
            {
                var order = context.Orders.Where(o => o.No == orderNo && o.IsDeleted != 1).FirstOrDefault();
                if (order.Status != OrderStatus.Completed)
                {
                    result = OpResult.Fail("订单未完成，无法更新会员信息");
                    return result;
                }
                var member = context.Customers.Where(o => o.Id == order.CustomerId && o.IsDeleted != 1).FirstOrDefault();
                if (CommonRules.InternalAccount.Contains(member.Mobile))
                {//公司内部账号，不累计积分和成长值
                    member.TotalActualRMBPay = member.TotalActualRMBPay + order.ActualPay;
                }
                else
                {
                    var memberLevel = context.MemberLevel.Where(o => o.MemberLevelValue == member.MemberLevelValue && o.IsDeleted != 1).FirstOrDefault();
                    //保存增加积分记录
                    if ((int)(order.ActualPay * memberLevel.IntegralMultiples) > 0)
                    {
                        MemberIntegralLog memberIntegralLog = CreateNewMemberIntegralLog(order.CustomerId, order.No, (int)(order.ActualPay * memberLevel.IntegralMultiples),
                                                             order.ActualPay, string.Format("完成订单，订单号：{0}，增加{1}积分", order.No, (int)(order.ActualPay * memberLevel.IntegralMultiples)));
                        memberIntegralLog.Id = CommonRules.GUID;
                        context.MemberIntegralLog.Add(memberIntegralLog);
                    }
                    //保存增加成长值记录
                    if ((int)(order.ActualPay * memberLevel.GrowthValueMultiples) > 0)
                    {
                        MemberGrowthValueLog memberGrowthValueLog = CreateNewMemberGrowthValueLog(order.CustomerId, order.No, (int)(order.ActualPay * memberLevel.GrowthValueMultiples),
                                                                  order.ActualPay, string.Format("完成订单，订单号：{0}，增加{1}成长值", order.No, (int)(order.ActualPay * memberLevel.GrowthValueMultiples)));
                        memberGrowthValueLog.Id = CommonRules.GUID;
                        context.MemberGrowthValueLog.Add(memberGrowthValueLog);
                    }
                    //更新会员信息（只计算现金支付部分）(先记录后更新，否则记录中会有两倍成长值)
                    member.Integral = member.Integral + (int)(order.ActualPay * memberLevel.IntegralMultiples);
                    member.GrowthValue = member.GrowthValue + (int)(order.ActualPay * memberLevel.GrowthValueMultiples);
                    member.TotalActualRMBPay = member.TotalActualRMBPay + order.ActualPay;
                    //更新会员等级(取可升级等级值的最大值)
                    var newCurMemberLevel = context.MemberLevel.Where(o => o.MinGrowthValue <= member.GrowthValue && o.IsDeleted != 1).ToList();
                    if (newCurMemberLevel != null)
                    {
                        var curLevel = newCurMemberLevel.Max(o => o.MemberLevelValue);
                        if (curLevel != member.MemberLevelValue)
                        {//会员等级发生变化
                            member.UpdateMemberLevelTime = DateTime.Now;
                        }
                        member.MemberLevelValue = curLevel;
                    }
                }
                context.SaveChanges();
                result = OpResult.Success("更新会员信息成功");

            }
            catch (Exception ex)
            {
                result = OpResult.Fail(string.Format("订单完成时，更新会员信息失败，订单号：{0}，异常信息:{1}", orderNo, ex.Message));
            }
            return result;
        }
        #endregion


        #region 公共方法
        /// <summary>
        /// 创建新的积分记录对象(未保存)
        /// </summary>
        /// <returns></returns>
        public MemberIntegralLog CreateNewMemberIntegralLog(string memberId, string orderSN, int changeIntegral, decimal cashAmount, string remark)
        {
            MemberIntegralLog memberIntegralLog = new MemberIntegralLog();
            memberIntegralLog.CreatedOn = DateTime.Now;
            memberIntegralLog.MemberId = memberId;
            memberIntegralLog.OrderSN = orderSN;
            memberIntegralLog.ChangeIntegral = changeIntegral;
            memberIntegralLog.CashAmount = cashAmount;
            memberIntegralLog.Remark = remark;
            memberIntegralLog.IsDeleted = 0;
            var member = context.Customers.FirstOrDefault(o => o.Id == memberId && o.IsDeleted != 1);
            if (member != null)
            {
                memberIntegralLog.TotalIntegral = member.Integral + changeIntegral;
                var memberLevel = context.MemberLevel.FirstOrDefault(o => o.MemberLevelValue == member.MemberLevelValue && o.IsDeleted != 1);
                if (memberLevel != null)
                {
                    memberIntegralLog.Multiple = memberLevel.IntegralMultiples;
                }
            }

            return memberIntegralLog;
        }
        /// <summary>
        /// 创建新的成长值记录对象（未保存）
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="orderSN"></param>
        /// <param name="changeGrowthValue"></param>
        /// <param name="cashAmount"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        public MemberGrowthValueLog CreateNewMemberGrowthValueLog(string memberId, string orderSN, int changeGrowthValue, decimal cashAmount, string remark)
        {
            MemberGrowthValueLog memberGrowthValueLog = new MemberGrowthValueLog();
            memberGrowthValueLog.CreatedOn = DateTime.Now;
            memberGrowthValueLog.MemberId = memberId;
            memberGrowthValueLog.OrderSN = orderSN;
            memberGrowthValueLog.ChangeGrowthValue = changeGrowthValue;
            memberGrowthValueLog.CashAmount = cashAmount;
            memberGrowthValueLog.Remark = remark;
            memberGrowthValueLog.IsDeleted = 0;
            var member = context.Customers.FirstOrDefault(o => o.Id == memberId && o.IsDeleted != 1);
            if (member != null)
            {
                memberGrowthValueLog.TotalGrowthValue = member.GrowthValue + changeGrowthValue;
                var memberLevel = context.MemberLevel.FirstOrDefault(o => o.MemberLevelValue == member.MemberLevelValue && o.IsDeleted != 1);
                if (memberLevel != null)
                {
                    memberGrowthValueLog.Multiple = memberLevel.GrowthValueMultiples;
                }
            }
            return memberGrowthValueLog;
        }

        #endregion

    }
}
