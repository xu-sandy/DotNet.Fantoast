using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FCake.Domain.Entities
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class OrderDetailHist : BaseEntity
    {
        [DataMember]
        public string OrderHistId { get; set; }
        [DataMember]
        public string OrderDetailId { get; set; }
        [DataMember]
        public string No { get; set; }
        [DataMember]
        public string ProductId { get; set; }
        [DataMember]
        public string SubProductId { get; set; }
        [DataMember]
        public decimal? Price { get; set; }
        [DataMember]
        public int? Num { get; set; }
        [DataMember]
        public decimal? TotalPrice { get; set; }
        [DataMember]
        public string OrderNo { get; set; }
        /// <summary>
        /// 生日卡牌
        /// </summary>
        [DataMember]
        public string BirthdayCard { get; set; }
        [DataMember]
        public string Size { get; set; }
        /// <summary>
        /// 产品规格单位的显示名称
        /// </summary>
        [DataMember]
        public string SizeTitle { get; set; }
    }
}
