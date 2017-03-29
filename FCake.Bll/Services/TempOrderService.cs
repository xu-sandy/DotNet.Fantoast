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
    public class TempOrderService : BaseService
    {
        private readonly EFDbContext _context = new EFDbContext();

        public OpResult ChangeOrderInfo(string orderNo, decimal actualPay, decimal couponPay, decimal giftCardPay, int isUpdateCustomer)
        {
            var result = OpResult.Fail("操作失败");
            try
            {
                var order = _context.Orders.Where(o => o.No == orderNo && o.IsDeleted != 1).FirstOrDefault();
                if (order != null)
                {
                    var customer = _context.Customers.Where(o => o.Id == order.CustomerId && o.IsDeleted != 1).FirstOrDefault();
                    if (customer != null)
                    {
                        var changeActualPayVal = actualPay - order.ActualPay;
                        order.ActualPay = actualPay;
                        order.CouponPay = couponPay;
                        order.GiftCardPay = giftCardPay;
                        order.NeedPay = order.TotalPrice - (order.CouponPay + order.GiftCardPay + order.IntegralPay);
                        if (order.NeedPay < 0)
                            return OpResult.Fail("操作失败，需支付金额不能小于0");
                        if (order.ActualPay > order.NeedPay)
                            return OpResult.Fail("操作失败，实际支付金额不能超过需支付金额");

                        if (isUpdateCustomer == 1 && order.Status == Domain.Enums.OrderStatus.Completed)
                        {//更新对应用户信息(现金消费金额、成长值、会员等级)
                            customer.TotalActualRMBPay += changeActualPayVal;
                            customer.GrowthValue += (int)changeActualPayVal;
                            //更新会员等级(取可升级等级值的最大值)
                            var newCurMemberLevel = _context.MemberLevel.Where(o => o.MinGrowthValue <= customer.GrowthValue && o.IsDeleted != 1).ToList();
                            if (newCurMemberLevel != null)
                            {
                                var curLevel = newCurMemberLevel.Max(o => o.MemberLevelValue);
                                customer.MemberLevelValue = curLevel;
                            }
                        }    
                        _context.SaveChanges();
                        result = OpResult.Success("操作成功");
                    }
                    else
                    {
                        result = OpResult.Fail("操作失败，找不到对应的用户");
                    }

                }
                else
                {
                    result = OpResult.Fail("操作失败，找不到对应的订单");
                }
            }
            catch (Exception ex)
            {
                result = OpResult.Fail("操作失败，系统异常:" + ex.Message);
            }
            return result;
        }
    }
}
