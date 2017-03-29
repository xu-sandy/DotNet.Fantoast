using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FCake.Admin.Models
{
    /// <summary>
    /// 营业管理
    /// </summary>
    public class BusinessManageModel
    {
        /// <summary>
        /// 临时最早可选配送时间
        /// </summary>
        public DateTime TempEarlyDistributionTime { get; set; }
        /// <summary>
        /// 生产时长
        /// </summary>
        public int ProductionHours { get; set; }
    }
}