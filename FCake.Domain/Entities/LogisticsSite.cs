using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FCake.Domain.Entities
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class LogisticsSite : BaseEntity
    {
        /// <summary>
        /// 站点编码
        /// </summary>
        [DataMember]
        public string SiteCode { get; set; }
        /// <summary>
        /// 站点名称
        /// </summary>
        [DataMember]
        public string SiteName { get; set; }
        /// <summary>
        /// 站点所在省份
        /// </summary>
        [DataMember]
        public string SiteProvince { get; set; }
        /// <summary>
        /// 站点所在城市
        /// </summary>
        [DataMember]
        public string SiteCity { get; set; }
        /// <summary>
        /// 站点所在区
        /// </summary>
        [DataMember]
        public string SiteArea { get; set; }
        /// <summary>
        /// 站点详细地址
        /// </summary>
        [DataMember]
        public string SiteAddress { get; set; }
        /// <summary>
        /// 站点附近名建筑
        /// </summary>
        [DataMember]
        public string SiteVicinity { get; set; }
        /// <summary>
        /// 是否默认
        /// </summary>
        [DataMember]
        public int? IsDef { get; set; }
        /// <summary>
        /// 状态(0:正常;1:关闭)
        /// </summary>
        [DataMember]
        public short Status { get; set; }
    }
}
