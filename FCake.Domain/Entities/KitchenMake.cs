using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using FCake.Domain.Enums;


namespace FCake.Domain.Entities
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class KitchenMake : BaseEntity
    {
        public KitchenMake() { }
        public KitchenMake(string id,string batchNo, string orderNo,string userId) {
            Id = id;
            IsDeleted = 0;
            CreatedBy = userId;
            CreatedOn = DateTime.Now;
            BatchNo = batchNo;
            OrderNo = orderNo;
            Status = OrderBatchMakeStatus.NotStart;
        }
        /// <summary>
        /// 批次编号
        /// </summary>
        [DataMember]
        public string BatchNo { get; set; }
        /// <summary>
        /// 订单号
        /// </summary>
        [DataMember]
        public string OrderNo { get; set; }
        /// <summary>
        /// 订单状态（未开始=0,制作中=1,完成=2）
        /// </summary>
        [DataMember]
        public OrderBatchMakeStatus Status { get; set; }
        /// <summary>
        /// 一个订单属于一个批次 1=1
        /// </summary>
        [DataMember]
        public virtual OrderBatch OrderBatch { get; set; }
        /// <summary>
        /// 一个订单下有多个产品 1=N
        /// </summary>
        [DataMember]
        public virtual ICollection<KitchenMakeDetail> KitchenMakeDetails { get; set; }
    }
}
