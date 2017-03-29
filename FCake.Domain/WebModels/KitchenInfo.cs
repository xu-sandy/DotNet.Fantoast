using FCake.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FCake.Domain.WebModels
{
    public class KitchenInfo
    {
        /// <summary>
        /// 批次号
        /// </summary>
        public string BatchNo { get; set; }
        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderNo { get; set; }
        /// <summary>
        /// 要求完成时间
        /// </summary>
        public DateTime? BatchRequiredTime { get; set; }
        /// <summary>
        /// 订单要求送达时间
        /// </summary>
        public DateTime? OrderRequiredTime { get; set; }
        /// <summary>
        /// 蛋糕名称
        /// </summary>
        public string ProductName { get; set; }
        /// <summary>
        /// 磅数
        /// </summary>
        public string Size { get; set; }
        /// <summary>
        /// 磅数
        /// </summary>
        public string SizeTitle { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int? Num { get; set; }
        /// <summary>
        /// 订单状态
        /// </summary>
        public OrderBatchMakeStatus OrderStatus { get; set; }
        /// <summary>
        /// 批次状态
        /// </summary>
        public OrderBatchMakeStatus KitStatus { get; set; }
        /// <summary>
        /// 产品状态
        /// </summary>
        public OrderBatchMakeStatus ProductStatus { get; set; }
        /// <summary>
        /// 产品ID
        /// </summary>
        public string ProductId { get; set; }
        /// <summary>
        /// 子产品ID
        /// </summary>
        public string SubProductId { get; set; }
        /// <summary>
        /// 厨房明细ID
        /// </summary>
        public string KitchenMakeDetailId { get; set; }

    }
}
