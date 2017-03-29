// --------------------------------------------------
// Copyright (C) 2015 版权所有
// 创 建 人：
// 创建时间：2015-11-30
// 描述信息：用于管理本系统中代金卡信息
// --------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCake.Domain.Enums;
using System.Runtime.Serialization;

namespace FCake.Domain.Entities
{
    /// <summary>
    /// 代金卡
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]

    public class GiftCards : BaseEntity
    {
        public GiftCards() { }
        public GiftCards(int couponCount, decimal faceValue, DateTime allowBeginDate,
            DateTime allowEndDate, decimal price, string createdBy)
        {
            Quantity = couponCount;
            Denomination = faceValue;
            BeginValidDate = allowBeginDate;
            EndValidDate = allowEndDate;
            CreatedBy = createdBy;
            CreatedOn = DateTime.Now;
            Id = GetNewCouponOrderId();
            SalesMoney = price;
            //让对象成为刚生成状态
            ReviewStatus = (short)Domain.Enums.GiftCardReviewStatus.ReviewPending;
        }

        private string GetNewCouponOrderId()
        {
            var nowTime = DateTime.Now.ToString("yyyyMMddHHmmss");
            var random1 = System.Guid.NewGuid().ToString().Substring(0, 7);
            return nowTime + random1;
        }


        /// <summary>
        /// 卡名
        /// [长度：50]
        /// </summary>
        [DataMember]
        public string Title { get; set; }
        /// <summary>
        /// 代金卡批次号
        /// 【长度：40】
        /// 【不允许空】
        /// 2015-12-24
        /// </summary>
        [DataMember]
        public string GiftBatch { get; set; }
        /// <summary>
        /// 面额
        /// [长度：19，小数位数：4]
        /// [不允许为空]
        /// </summary>
        [DataMember]
        public decimal Denomination { get; set; }

        /// <summary>
        /// 销售金额
        /// [长度：19，小数位数：4]
        /// [不允许为空]
        /// [默认值：((0))]
        /// </summary>
        [DataMember]
        public decimal SalesMoney { get; set; }

        /// <summary>
        /// 代金卡发放数量
        /// [长度：10]
        /// [不允许为空]
        /// </summary>
        [DataMember]
        public int Quantity { get; set; }

        /// <summary>
        /// 有效期开始时间
        /// [长度：23，小数位数：3]
        /// [不允许为空]
        /// </summary>
        [DataMember]
        public DateTime BeginValidDate { get; set; }

        /// <summary>
        /// 有效期截至时间
        /// [长度：23，小数位数：3]
        /// [不允许为空]
        /// </summary>
        [DataMember]
        public DateTime EndValidDate { get; set; }

        /// <summary>
        /// 代金卡类型（0：纸质，1：电子）
        /// [长度：5]
        /// [不允许为空]
        /// </summary>
        [DataMember]
        public short Type { get; set; }

        /// <summary>
        /// 审核状态（1：待审核，2：审核通过，3：审核未通过）
        /// [长度：5]
        /// [不允许为空]
        /// </summary>
        [DataMember]
        public short ReviewStatus { get; set; }

    }
}
