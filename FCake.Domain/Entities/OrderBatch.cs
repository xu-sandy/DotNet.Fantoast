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
    public class OrderBatch : BaseEntity
    {
        public OrderBatch()
        {
        }
        public OrderBatch(string id,string userId,string batchNo,int orderNum,int cakeNum)
        {
            // TODO: Complete member initialization
            Id = id;
            IsDeleted = 0;
            BatchNo = batchNo;
            CreatedBy = userId;
            CreatedOn = DateTime.Now;
            Status = BatchReviewStatus.ReviewPending;
            OrderNum = orderNum;
            CakeNum = cakeNum;
            //RequiredTime? Todo:待客户实际生产流程稳定后用算法自动设定，目前因算法参数不明，需先手动维护
        }

        /// <summary>
        /// 批次编号
        /// </summary>
        [DataMember]
        public string BatchNo { get; set; }
        /// <summary>
        /// 要求完成时间
        /// </summary>
        [DataMember]
        public DateTime? RequiredTime { get; set; }
        /// <summary>
        /// 制作开始时间
        /// </summary>
        [DataMember]
        public DateTime? MakeBeginTime { get; set; }
        /// <summary>
        /// 制作完成时间
        /// </summary>
        [DataMember]
        public DateTime? MakeEndTime { get; set; }
        /// <summary>
        /// 审核状态
        /// </summary>
        [DataMember]
        public BatchReviewStatus? Status { get; set; }
        /// <summary>
        /// 制作状态
        /// </summary>
        [DataMember]
        public OrderBatchMakeStatus? MakeStatus { get; set; }
        /// <summary>
        /// 订单数量
        /// </summary>
        [DataMember]
        public int OrderNum { get; set; }
        /// <summary>
        /// 蛋糕数量
        /// </summary>
        [DataMember]
        public int CakeNum { get; set; }
        /// <summary>
        /// 一个批次下面有多个订单，一对多的关系
        /// </summary>
        [DataMember]
        public virtual ICollection<KitchenMake> KitchenMakes { get; set; }
    }
}
