using FCake.DAL.Repositories;
using FCake.Domain;
using FCake.Domain.Entities;
using FCake.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCake.API;

namespace FCake.Bll.Services
{
    /// <summary>
    /// 支付业务
    /// </summary>
    public class PayService : BaseService
    {
        /// <summary>
        /// 由订单号取订单
        /// </summary>
        /// <param name="orderNo"></param>
        /// <returns></returns>
        public Orders GetOrderByOrderNo(string orderNo)
        {
            return DAL.SingleOrDefault<Orders>(a => a.No.Equals(orderNo));
        }

        /// <summary>
        /// 完成支付
        /// </summary>
        /// <param name="orderNo">订单号</param>
        /// <param name="tradeNo">交易单号</param>
        /// <param name="feeType">交易方式</param>
        /// <param name="isReviewPass">是否默认通过审核 如果为false则待审核</param>
        /// <returns></returns>
        public void FinishOrder(string orderNo, string tradeNo, FeeType feeType,bool isReviewPass=true)
        {
            //取出订单
            var order = GetOrderByOrderNo(orderNo);
            if (order != null && order.TradeStatus == TradeStatus.NotPay)
            {
                //插入订单历史
                OrderHist oh = new OrderHist();
                oh.CopyProperty(order);
                oh.Id = FCake.Core.Common.DataHelper.GetSystemID();
                oh.CreatedOn = DateTime.Now;
                oh.CreatedBy = order.CreatedBy;
                DAL.Entities<OrderHist>().Add(oh);
                //oh.OrderId = order.Id;
                //oh.No = order.No;
                //oh
                //变更支付状态
                //改order 状态  订单状态、支付状态，插入状态，支付状态
                //todo: 更改订单状态 逻辑？
                order.Status = OrderStatus.HadPaid;
                order.TradeNo = tradeNo;
                order.ActualPay = order.TotalPrice - (order.CouponPay + order.GiftCardPay + order.IntegralPay);
                order.TradeStatus = TradeStatus.HadPaid;
                if (isReviewPass)
                {
                    if (order.ReviewStatus != ReviewStatus.ReviewPass && order.ReviewStatus != ReviewStatus.ReviewReject && order.ReviewStatus != ReviewStatus.Canceled)
                    {
                        order.ReviewStatus = ReviewStatus.ReviewPending;//付了款的订单还是要审核
                    }
                }
                else
                {
                    order.ReviewStatus = ReviewStatus.ReviewOnLineNoPay;
                }
                
                //order.ReviewStatus = (isReviewPass ? ReviewStatus.ReviewPending : ReviewStatus.ReviewOnLineNoPay);//付了款的订单还是要审核

                //发送短信给客户
                if (order.ReceiverMobile != "" && order.ReceiverMobile != null) {
                    var customer = (new CustomersService()).GetById(order.CustomerId);
                    if (customer != null) {
                        try
                        {                         
                            //EChiHelper.SendSMS(customer.Mobile, new FCake.Bll.Services.MsgTemplateService().GetMsgTempByCategory("Pass"), FormatType.BuySuccess);
                            var sendSMSErrorMsg = string.Empty;
                            var sendResult = DaYuSMSHelper.SendNotifySMS(customer.Mobile, DaYuConfig.OrderApproveTemplate,out sendSMSErrorMsg);
                            if (!sendResult)
                            {
                                SysLogService.SaveAliSMSErrorLog(sendSMSErrorMsg, customer.Mobile, DaYuConfig.OrderApproveTemplate);
                            }
                        }
                        catch { }
                    }
                    //EChiHelper.SendSMS(order.ReceiverMobile, FormatType.BuySuccess, new FCake.Bll.Services.MsgTemplateService().GetMsgTempByCategory("Pass"));
                }
                //提交
                DAL.Commit();
            }
        }
    }
}
