using FCake.Core.MvcCommon;
using FCake.Domain;
using FCake.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FCake.Bll
{
    public class LogisticsSiteService
    {
        EFDbContext _context = new EFDbContext();

        /// <summary>
        /// 根据市来获取站点自提的站点列表
        /// </summary>
        /// <param name="cityName"></param>
        /// <returns></returns>
        public List<LogisticsSite> GetLogisticsSitesByCity(string cityName)
        {
            return _context.LogisticsSite.Where(a => a.SiteCity.Equals(cityName) && a.IsDeleted != 1).ToList();
        }
        /// <summary>
        /// 获取所有的自提站点
        /// </summary>
        /// <returns></returns>
        public List<LogisticsSite> GetAllLogisticsSites()
        {
            return _context.LogisticsSite.Where(a => a.IsDeleted != 1 && a.Status==0).ToList();
        }
        /// <summary>
        /// 根据ID获取站点信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public LogisticsSite GetLogisticsSiteById(string id = "")
        {
            var site = new LogisticsSite();
            if (id != "")
            {
                site = _context.LogisticsSite.Where(p => p.Id == id && p.IsDeleted != 1).First();
            }
            return site;
        }
        /// <summary>
        /// 站点信息保存
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public OpResult SaveLogisticsSite(LogisticsSite site)
        {
            OpResult result = new OpResult();
            var hasany = _context.LogisticsSite.Any(p => p.Id == site.Id && p.IsDeleted != 1);
            if (hasany)
            {
                result = UpdateLogisticsSite(site);
            }
            else
            {
                result = CreateLogisticsSite(site);
            }
            return result;
        }
        /// <summary>
        /// 创建一个新的自提站点
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public OpResult CreateLogisticsSite(LogisticsSite site)
        {
            OpResult result = new OpResult();
            result.Successed = false;
            try
            {
                var ishas = _context.LogisticsSite.Any(p => p.SiteProvince.Equals(site.SiteProvince) && p.SiteCity.Equals(site.SiteCity) && p.SiteArea.Equals(site.SiteArea) && p.SiteAddress.Equals(site.SiteAddress));
                if (ishas)
                {
                    result.Message = "添加失败：该地址站点已存在";
                    return result;
                }
                if (site.IsDef == 1)
                {
                    bool res = UpdateIsdef(site.SiteCity);
                    if (!res)
                    {
                        result.Successed = true;
                        result.Message = "添加失败：更改默认地址时失败";
                        return result;
                    }
                }
                site.Id = Guid.NewGuid().ToString().Replace("-", "").ToUpper();
                _context.LogisticsSite.Add(site);
                if (_context.SaveChanges() > 0)
                {
                    result.Successed = true;
                    result.Message = "添加成功";
                }
                else
                {

                    result.Message = "添加失败";
                }
            }
            catch (Exception e)
            {
                result.Message = "添加失败:" + e.Message;
            }
            return result;
        }
        /// <summary>
        /// 更新自提站点的信息
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public OpResult UpdateLogisticsSite(LogisticsSite site)
        {
            OpResult result = new OpResult();
            try
            {
                var ishas = _context.LogisticsSite.Any(p => !p.Id.Equals(site.Id) && p.SiteProvince.Equals(site.SiteProvince) && p.SiteCity.Equals(site.SiteCity) && p.SiteArea.Equals(site.SiteArea) && p.SiteAddress.Equals(site.SiteAddress));
                if (ishas)
                {
                    result.Message = "添加失败：该地址站点已存在";
                    return result;
                }
                if (site.IsDef == 1)
                {
                    bool res = UpdateIsdef(site.SiteCity);
                    if (!res)
                    {
                        result.Successed = true;
                        result.Message = "更新失败：更改默认地址时失败";
                        return result;
                    }
                }
                var data = _context.LogisticsSite.Where(p => p.Id == site.Id && p.IsDeleted != 1).First();
                data.SiteCode = site.SiteCode;
                data.SiteName = site.SiteName;
                data.SiteProvince = site.SiteProvince;
                data.SiteCity = site.SiteCity;
                data.SiteArea = site.SiteArea;
                data.SiteAddress = site.SiteAddress;
                data.IsDef = site.IsDef;
                data.Status = site.Status;
                data.SiteProvince = site.SiteProvince;
                var save = _context.SaveChanges();
                if (save > 0)
                {
                    result.Successed = true;
                    result.Message = "更新成功";
                }
                else
                {
                    result.Successed = false;
                    result.Message = "更新失败";
                }
            }
            catch (Exception e)
            {
                result.Successed = false;
                result.Message = "更新失败：" + e.Message;
            }
            return result;
        }
        /// <summary>
        /// 更新数据如果是默认地址需先去掉原本的默认地址
        /// </summary>
        /// <param name="city">一个城市一个默认地址</param>
        /// <returns></returns>
        public bool UpdateIsdef(string city)
        {
            bool result = false;
            var data = _context.LogisticsSite.SingleOrDefault(p => p.SiteCity.Equals(city) && p.IsDef == 1 && p.IsDeleted != 1);
            if (data != null)
            {
                data.IsDef = 0;
                if (_context.SaveChanges() > 0)
                {
                    result = true;
                }
            }
            else
            {
                result = true;
            }
            return result;
        }
        /// <summary>
        /// 根据id软删除自提站点的数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public OpResult DeleteLogisSiteById(string id = "")
        {
            OpResult result = new OpResult();
            result.Successed = false;

            try
            {
                if (id != "")
                {
                    var data = _context.LogisticsSite.Where(p => p.Id.Equals(id) && p.IsDeleted != 1).First();
                    if (data != null)
                    {
                        data.IsDeleted = 1;
                        if (_context.SaveChanges() > 0)
                        {
                            result.Successed = true;
                            result.Message = "删除成功";
                        }
                    }
                    else
                    {
                        result.Message = "删除失败，查无此记录";
                    }
                }
            }
            catch (Exception e)
            {
                result.Message = "删除失败:" + e.Message;
            }
            return result;
        }
        /// <summary>
        /// 根据区域获取所有区域自提站点
        /// </summary>
        /// <param name="area">区域名称</param>
        /// <returns></returns>
        public List<LogisticsSite> GetLogisticeSiteByArea(string city)
        {
            var data = _context.LogisticsSite.Where(p => p.IsDeleted != 1);
            if (city != "")
            {
                data = data.Where(p => p.SiteCity == city);
            }
            return data.ToList();
        }
        /// <summary>
        /// 根据条件获取站点信息
        /// </summary>
        /// <param name="site"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        public List<LogisticsSite> GetLogisticsSites(LogisticsSite site, out int count, int page = 1, int rows = 10)
        {
            if (site != null)
            {
                var data = _context.LogisticsSite.Where(p => p.IsDeleted != 1);
                if (site.SiteCode != null)
                {
                    data = data.Where(p => p.SiteCode == site.SiteCode);
                }
                if (site.SiteName != null)
                {
                    data = data.Where(p => p.SiteName.Contains(site.SiteName));
                }
                if (site.SiteProvince != null)
                {
                    data = data.Where(p => p.SiteProvince == site.SiteProvince);
                }
                if (site.SiteCity != null)
                {
                    data = data.Where(p => p.SiteCity == site.SiteCity);
                }
                if (site.SiteArea != null)
                {
                    data = data.Where(p => p.SiteArea == site.SiteArea);
                }
                if (site.SiteAddress != null)
                {
                    data = data.Where(p => p.SiteAddress == site.SiteAddress);
                }
                page = page < 1 ? 1 : page;

                var temp = data.OrderBy(p => p.SiteProvince).Skip((page - 1) * rows).Take(rows).ToList();
                count = temp.Count();
                if (temp != null)
                {
                    return temp.ToList();
                }
            }
            count = 0;
            return new List<LogisticsSite>();
        }

        public string GetAddressById(string id)
        {
            var site = _context.LogisticsSite.SingleOrDefault(a => a.Id.Equals(id) && a.IsDeleted != 1);
            if (site != null)
            {
                return string.Format("{0} {1}{2}{3},{4}", site.SiteName, site.SiteProvince, site.SiteCity, site.SiteArea, site.SiteAddress);
            }
            return "";
        }
        /// <summary>
        /// 订单详情——根据省份获得对应省份下的所有自提站点列表
        /// </summary>
        /// <param name="provinceName"></param>
        /// <returns></returns>
        public List<LogisticsSite> GetLogisticsSitesByProvince(string provinceName)
        {
            var query = _context.LogisticsSite.Where(d => d.SiteProvince == provinceName && d.IsDeleted != 1 && d.Status==0).OrderBy(d => d.SiteCode);
            var objs = query.ToList();
            return objs;
        }
    }
}
