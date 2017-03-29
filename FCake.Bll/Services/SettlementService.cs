using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FCake.Bll.Services
{
    public class SettlementService : BaseService
    {
        /// <summary>
        /// 配送时间控制规则描述(N生产时长，E最早可配送时间)
        /// 如果下单时间在营业时间范围内，则E=当前时间顺延N个小时后的最早可配送时间范围内
        /// 如果下单时间在非营业时间范围内，则E=下一个最早营业时间顺延N个小时之后
        /// </summary>
        /// <returns></returns>
        public DateTime GetEarlyAllowDistributionTime(int curOrderMaxInadvanceHours)
        {
            var salesConfig = SysConfigsCache.GetSalesConfig();
            var salesStart = salesConfig.SalesStartHour;
            var salesEnd = salesConfig.SalesEndHour;
            var productionHours = salesConfig.ProductionHours;
            //如果订单中有生产周期小时数大于，系统默认配置的生产周期，就取最大的小时数
            if (curOrderMaxInadvanceHours > productionHours)
            {
                productionHours = curOrderMaxInadvanceHours;
            }

            var nowTime = DateTime.Now;
            var nowHour = nowTime.Hour;
            var earlyAllowDistributionTime = nowTime;

            var nextDayTime = DateTime.Now;
            if (nowHour >= salesStart && nowHour < 24)
            {
                nextDayTime = nextDayTime.AddDays(1);
            }
            var nextDayEarlySalesTime = nextDayTime.Date.AddHours(salesStart);//下一个最早营业时间
            var nextDayLatestSalesTime = nextDayTime.Date.AddHours(salesEnd);//下一个最晚营业时间暂时值
            var nextDayEarlyDistributionTime = nextDayTime.Date.AddHours(salesConfig.EarlyDistributionTime);//下一个最早配送时间
            var nextDayLatestDistributionTime = nextDayTime.Date.AddHours(salesConfig.LatestDistributionTime);//下一个最晚配送时间

            earlyAllowDistributionTime = nowTime.AddHours(productionHours);
            {//计算最早可选日期时间
                if (nowHour >= salesStart && nowHour < salesEnd)
                {//营业时间范围判断
                    //判断是否在配送时间范围内，不在营业时间范围内则顺延到下一个日期的最早配送时间时间
                    if (earlyAllowDistributionTime.Hour < nextDayEarlyDistributionTime.Hour || earlyAllowDistributionTime.Hour >= nextDayLatestDistributionTime.Hour)
                    {
                        earlyAllowDistributionTime = nextDayEarlyDistributionTime;
                    }
                }
                else
                {//非营业时间范围判断
                    //earlyAllowDistributionTime = nextDayEarlySalesTime.AddHours(productionHours);
                    earlyAllowDistributionTime = nextDayEarlySalesTime.Date.AddHours(14);
                    if (earlyAllowDistributionTime.Hour < nextDayEarlyDistributionTime.Hour)
                        earlyAllowDistributionTime = nextDayEarlyDistributionTime;
                }
            }
            if (earlyAllowDistributionTime < salesConfig.TempEarlyDistributionTime)
            {
                earlyAllowDistributionTime = salesConfig.TempEarlyDistributionTime;
            }
            return earlyAllowDistributionTime;
        }
    }
}
