using FCake.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FCake.Domain.WebModels
{
    /// <summary>
    /// 统计分析报表Model，用于Echart
    /// </summary>
    public class StatisticalAnalysisModel
    {
        public decimal value { get; set; }
        public string name { get; set; }
    }
}