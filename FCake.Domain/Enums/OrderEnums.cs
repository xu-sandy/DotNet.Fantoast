using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace FCake.Domain.Enums
{
    /// <summary>
    /// 支付类型
    /// </summary>
    public enum FeeType : int
    {
        /// <summary>
        /// 货到付现金
        /// </summary>
        [Description("货到付现金")]
        Cash=0,
        /// <summary>
        /// 货到POS刷卡
        /// </summary>
        [Description("货到POS刷卡")]
        POS = 1,
        /// <summary>
        /// 支付宝支付
        /// </summary>
        [Description("支付宝支付")]
        ALiPay=2,
        /// <summary>
        /// 财付通支付
        /// </summary>
        [Description("财付通支付")]
        TenPay = 3,
        /// <summary>
        /// 其他支付
        /// </summary>
        [Description("其他支付")]
        OtherPay = 4,
        /// <summary>
        /// 微信钱包支付
        /// </summary>
        [Description("微信支付")]
        WXPay=5
        ///// <summary>
        ///// 在线支付
        ///// </summary>
        //[Description("在线支付")]
        //OnlinePay = 1,
        ///// <summary>
        ///// 到付
        ///// </summary>
        //[Description("到付")]
        //ToPay = 2
    }
    /// <summary>
    /// 订单状态
    /// </summary>
    public enum OrderStatus : int
    {
        /// <summary>
        /// 等待付款
        /// </summary>
        [Description("等待付款")]
        NotPay = 0,
        /// <summary>
        /// 下单成功
        /// </summary>
        [Description("下单成功")]
        HadPaid = 1,
        /// <summary>
        /// 排产中
        /// </summary>
        [Description("排产中")]
        Scheduled = 2,
        /// <summary>
        /// 制作中
        /// </summary>
        [Description("制作中")]
        Making = 3,
        /// <summary>
        /// 制作完成
        /// </summary>
        [Description("制作完成")]
        MakeCompleted = 4,
        /// <summary>
        /// 配送中
        /// </summary>
        [Description("配送中")]
        Delivery = 5,
        /// <summary>
        /// 已完成
        /// </summary>
        [Description("已完成")]
        Completed = 6,
        /// <summary>
        /// 已取消
        /// </summary>
        [Description("已取消")]
        Canceled=7,
        /// <summary>
        /// 交易关闭
        /// </summary>
        [Description("交易关闭")]
        TradingClosed = 90,

    }
    /// <summary>
    /// 支付状态
    /// </summary>
    public enum TradeStatus : int
    {
        /// <summary>
        /// 未支付=0
        /// </summary>
        [Description("未支付")]
        NotPay = 0,
        /// <summary>
        /// 已支付=1
        /// </summary>
        [Description("已支付")]
        HadPaid = 1,
        /// <summary>
        /// 已退款=2
        /// </summary>
        [Description("已退款")]
        HadRefund = 2
    }
    public enum OrderSource : int {
        /// <summary>
        /// 在线订单
        /// </summary>
        [Description("在线订单")]
        OnlineOrder = 1,
        /// <summary>
        /// 电话订单
        /// </summary>
        [Description("电话订单")]
        TelOrder = 2
    }
}
