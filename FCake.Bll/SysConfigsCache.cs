using FCake.Domain;
using FCake.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FCake.Bll
{
    /// <summary>
    /// 系统配置缓存
    /// </summary>
    public class SysConfigsCache
    {
        public static void Register() {
            _context = new EFDbContext();
            ResetRegister();
        }
        public static void ResetRegister()
        {
            ResetSalesConfig();
            ResetWebUrlConfig();
        }

        private static EFDbContext _context = null;
        private static SalesConfig _salesConfig = null;
        private static WebUrlConfig _webUrlConfig = null;
        /// <summary>
        /// 获得营业时间配置
        /// </summary>
        /// <returns></returns>
        public static SalesConfig GetSalesConfig()
        {
            if (_salesConfig == null){
                ResetSalesConfig();
            }
            return _salesConfig;
        }

        public static void ResetSalesConfig()
        {
            _salesConfig = new Bll.SalesConfig();
            var query = _context.SysConfigs.AsNoTracking().Where(s => s.PCode == "SalesConfig").ToList();
            var data = query.ToDictionary<SysConfig, string>(s => s.Code.ToLower());

            if (data.ContainsKey("productionhours"))
            {
                _salesConfig.ProductionHours = int.Parse(data["productionhours"].Value);
            }
            if (data.ContainsKey("salesstarthour"))
            {
                _salesConfig.SalesStartHour = int.Parse(data["salesstarthour"].Value);
            }
            if (data.ContainsKey("salesendhour"))
            {
                _salesConfig.SalesEndHour = int.Parse(data["salesendhour"].Value);
            }
            if (data.ContainsKey("earlydistributiontime"))
            {
                _salesConfig.EarlyDistributionTime = double.Parse(data["earlydistributiontime"].Value);
            }
            if (data.ContainsKey("latestdistributiontime"))
            {
                _salesConfig.LatestDistributionTime = double.Parse(data["latestdistributiontime"].Value);
            }
            if (data.ContainsKey("tempearlydistributiontime"))
            {
                var dtime = DateTime.Now;
                try
                {
                    if (data["tempearlydistributiontime"].Value != null)
                    {
                        dtime = Convert.ToDateTime(data["tempearlydistributiontime"].Value);
                        if (dtime < DateTime.Now)
                            dtime = DateTime.Now;
                    }
                }
                catch { }
                _salesConfig.TempEarlyDistributionTime = dtime;
            }
        }
        /// <summary>
        /// 网站地址配置信息
        /// </summary>
        /// <returns></returns>
        public static WebUrlConfig GetWebUrlConfig() {
            if (_webUrlConfig == null)
            {
                ResetWebUrlConfig();
            }
            return _webUrlConfig;
            
        }
        /// <summary>
        /// 重置网站地址配置信息
        /// </summary>
        public static void ResetWebUrlConfig()
        {
            _webUrlConfig = new Bll.WebUrlConfig();
            var query = _context.SysConfigs.AsNoTracking().Where(s => s.PCode == "WebUrlConfig").ToList();
            var data = query.ToDictionary<SysConfig, string>(s => s.Code.ToLower());

            if (data.ContainsKey("pcurl"))
            {
                _webUrlConfig.PCUrl = data["pcurl"].Value;
            }
            if (data.ContainsKey("mobileurl"))
            {
                _webUrlConfig.MobileUrl = data["mobileurl"].Value;
            }
        }
    }
    /// <summary>
    /// 营业时间配置
    /// </summary>
    public class SalesConfig {
        /// <summary>
        /// 生产时间周期(小时数)
        /// </summary>
        public int ProductionHours { get; set; }
        /// <summary>
        /// 每天最早营业时间(时)
        /// </summary>
        public int SalesStartHour { get; set; }
        /// <summary>
        /// 每天最晚营业时间(时)
        /// </summary>
        public int SalesEndHour { get; set; }
        /// <summary>
        /// 每天最早配送开始时间
        /// </summary>
        public double EarlyDistributionTime { get; set; }
        /// <summary>
        /// 每天最晚配送结束时间
        /// </summary>
        public double LatestDistributionTime { get; set; }
        /// <summary>
        /// 临时最早可选配送时间
        /// </summary>
        public DateTime TempEarlyDistributionTime { get; set; }
    }

    /// <summary>
    /// 网站地址配置信息
    /// </summary>
    public class WebUrlConfig{
        /// <summary>
        /// PC地址前缀
        /// </summary>
        public string PCUrl{get;set;}
        /// <summary>
        /// PC地址前缀
        /// </summary>
        public string MobileUrl{get;set;}
    }
}
