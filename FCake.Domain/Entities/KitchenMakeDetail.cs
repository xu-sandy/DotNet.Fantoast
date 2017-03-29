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
    public class KitchenMakeDetail : BaseEntity
    {
        public KitchenMakeDetail() { }
        public KitchenMakeDetail(string id,string userId,string makeId,string subProductId) {
            Id = id;
            IsDeleted = 0;
            CreatedBy = userId;
            CreatedOn = DateTime.Now;
            KitchenMakeId = makeId;
            SubProductId = subProductId;
            Status = OrderBatchMakeStatus.NotStart;
        }
        /// <summary>
        /// 开始时间
        /// </summary>
        [DataMember]
        public DateTime? BeginTime { get; set; }
        /// <summary>
        /// 完成时间
        /// </summary>
        [DataMember]
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// 制作状态、未开始=0,制作中=1,完成=2
        /// </summary>
        [DataMember]
        public OrderBatchMakeStatus Status { get; set; }
        /// <summary>
        /// 批次制作表id
        /// </summary>
        [DataMember]
        public string KitchenMakeId { get; set; }
        /// <summary>
        /// 产品详情id
        /// </summary>
        [DataMember]
        public string SubProductId { get; set; }
        /// <summary>
        /// 一个订单下有多个产品 1=1
        /// </summary>
        [DataMember]
        public virtual KitchenMake KitchenMake { get; set; }
    }
}
