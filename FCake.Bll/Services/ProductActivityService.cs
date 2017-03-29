using FCake.Core.Common;
using FCake.Core.MvcCommon;
using FCake.Core.Security;
using FCake.Domain;
using FCake.Domain.Entities;
using FCake.Domain.WebModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCake.Domain.Common;
using System.Collections;
using System.Linq.Expressions;

namespace FCake.Bll.Services
{
    /// <summary>
    /// 产品活动
    /// </summary>
    public class ProductActivityService : BaseService
    {
        private readonly EFDbContext _context = new EFDbContext();
        //private readonly ProductService _productService = new ProductService();
        /// <summary>
        /// 产品活动分页数据
        /// </summary>
        /// <param name="totalCount"></param>
        /// <param name="pageInfo"></param>
        /// <returns></returns>
        public List<ProductActivity> GetProductActivityPaging(out int totalCount, PageInfo pageInfo)
        {
            var query = _context.ProductActivity.Where(o => o.IsDeleted != 1);
            totalCount = query.Count();
            var data = query.OrderByDescending(o => o.CreatedOn).Skip((pageInfo.Page - 1) * pageInfo.Rows).Take(pageInfo.Rows).ToList();
            return data;
        }

        /// <summary>
        /// 根据活动id加载参与活动的产品数据
        /// </summary>
        /// <param name="activityId"></param>
        /// <returns></returns>
        public dynamic GetActivityProInfoByActId(string activityId)
        {
            var queryData = (from x in _context.ProductActivityDetail
                             join y in _context.SubProducts on x.SubProductId equals y.Id
                             join z in _context.Products on y.ParentId equals z.Id
                             where x.IsDeleted != 1 && y.IsDeleted != 1 && x.ProductActivityId.Equals(activityId)
                             select new
                             {
                                 ParentId = x.ProductId,
                                 Id = x.SubProductId,
                                 Name = z.Name,
                                 MainImgId = z.MainImgId,
                                 Size = y.Size,
                                 SizeTitle = "",
                                 Price = x.ActivityPrice,
                                 OrigonPrice = x.ProductPrice
                             }).ToList();

            return queryData.Select(a => new
            {
                ParentId = a.ParentId,
                Id = a.Id,
                Name = a.Name,
                MainImgId = a.MainImgId,
                Size = a.Size,
                SizeTitle = new ProductService().GetSizeTitleBuySubProductId(a.Id),
                Price = a.Price,
                OrigonPrice = a.OrigonPrice
            });

            //var list = queryData.ToList();
            //foreach (var item in list)
            //{
            //    item.SizeTitle = _productService.GetSizeTitleBuySubProductId(item.Id);
            //}
            //return list;
        }

        /// <summary>
        /// 根据条件获取活动数据
        /// </summary>
        /// <param name="whereLambda"></param>
        /// <returns></returns>
        public List<ProductActivity> GetProductActivityByWhereLambda(Expression<Func<ProductActivity, bool>> whereLambda)
        {
            return _context.ProductActivity.Where(whereLambda).ToList();

        }
        /// <summary>
        /// 删除指定活动
        /// </summary>
        /// <param name="activityId"></param>
        /// <returns></returns>
        public OpResult DeleteActivity(string activityId)
        {
            OpResult result = new OpResult() { Successed = false };
            try
            {
                var deleteData = GetProductActivityByWhereLambda(a => a.Id.Equals(activityId));
                foreach (var item in deleteData)
                {
                    item.IsDeleted = 1;
                }
                if (_context.SaveChanges() > 0)
                {
                    result.Successed = true;
                }
            }
            catch (Exception e)
            {
                result.Message = "删除活动失败!";
            }
            return result;
        }
        /// <summary>
        /// 已保存的活动发布
        /// </summary>
        /// <param name="activityId"></param>
        /// <returns></returns>
        public OpResult PushProductActivity(string activityId, string currentUser)
        {
            OpResult result = new OpResult() { Successed = false };
            try
            {
                var queryDatas = _context.ProductActivityDetail.Where(a => a.ProductActivityId.Equals(activityId)).ToList();
                foreach (var item in queryDatas)
                {
                    ProductActivityDetailPool padp = new ProductActivityDetailPool() { Id = CommonRules.GUID };
                    padp.CopyProperty(item);
                    padp.CreatedOn = DateTime.Now;
                    padp.CreatedBy = currentUser;
                    padp.IsDeleted = 0;
                    _context.ProductActivityDetailPool.Add(padp);
                }
                //发布成功改掉活动状态
                var activity = _context.ProductActivity.Single(a => a.Id.Equals(activityId));
                activity.PublishStatus = 1;
                if (_context.SaveChanges() > 0)
                {
                    result.Successed = true;
                }
            }
            catch (Exception e)
            {
                result.Message = "发布失败!";
            }
            return result;
        }
        /// <summary>
        /// 删除过期活动
        /// 判断过期时间＜当前时间
        /// </summary>
        /// <returns></returns>
        public void RemoveTimeoutActivity()
        {
            var currentDate = DateTime.Now;
            var datas = _context.ProductActivityDetailPool.Where(a => a.EndValidDate < currentDate).ToList();
            foreach (var item in datas)
            {
                _context.ProductActivityDetailPool.Remove(item);
            }
        }


        /// <summary>
        /// 创建产品活动
        /// </summary>
        /// <param name="productActivity"></param>
        /// <returns></returns>
        public OpResult CreateProductActivity(ProductActivity productActivity, string currentUser)
        {
            var createStatus = false;
            OpResult result = new OpResult() { Successed = false };
            try
            {
                //添加活动主要数据
                if (productActivity.Id == null)
                {
                    createStatus = true;
                    productActivity.Id = CommonRules.GUID;
                    productActivity.CreatedBy = currentUser;
                    productActivity.CreatedOn = DateTime.Now;
                    productActivity.IsDeleted = 0;
                }
                else
                {
                    productActivity.ModifiedBy = currentUser;
                    productActivity.ModifiedOn = DateTime.Now;
                    //删除原来添加的产品
                    var oldProdata = _context.ProductActivityDetail.Where(a => a.ProductActivityId.Equals(productActivity.Id)).ToList();
                    foreach (var item in oldProdata)
                    {
                        item.IsDeleted = 1;
                    }
                    var oldProPoolData = _context.ProductActivityDetailPool.Where(a => a.ProductActivityId.Equals(productActivity.Id)).ToList();
                    foreach (var item in oldProPoolData)
                    {
                        _context.ProductActivityDetailPool.Remove(item);
                    }

                }

                //多个产品做活动
                if (productActivity.ProductIds.Contains(","))
                {
                    var arrayIds = productActivity.ProductIds.Trim(',').Split(',');
                    var arrayPIds = productActivity.ParentProductIds.Trim(',').Split(',');
                    var arrayPrices = productActivity.ActivityProPrices.Trim(',').Split(',');
                    var arrayOprices = productActivity.ProOrginPrice.Trim(',').Split(',');
                    if (arrayIds.Count() != arrayPIds.Count() || arrayPIds.Count() != arrayPrices.Count() || arrayPIds.Count() != arrayOprices.Count())
                    {
                        result.Message = "提交数据异常，创建数据失败!";
                        return result;
                    }
                    for (var i = 0; i < arrayPIds.Count(); i++)
                    {
                        ProductActivityDetail proActivityDetail = new ProductActivityDetail();
                        //插入活动历史表

                        proActivityDetail.Id = CommonRules.GUID;
                        proActivityDetail.ProductId = arrayPIds[i];
                        proActivityDetail.SubProductId = arrayIds[i];
                        proActivityDetail.ProductActivityId = productActivity.Id;
                        proActivityDetail.ProductPrice = Convert.ToDecimal(arrayOprices[i]);//产品原价
                        proActivityDetail.ActivityPrice = Convert.ToDecimal(arrayPrices[i]);
                        proActivityDetail.BeginValidDate = productActivity.BeginValidDate;
                        proActivityDetail.EndValidDate = productActivity.EndValidDate;
                        proActivityDetail.CreatedBy = currentUser;
                        proActivityDetail.CreatedOn = DateTime.Now;
                        proActivityDetail.IsDeleted = 0;
                        _context.ProductActivityDetail.Add(proActivityDetail);
                        //发布状态的话插入活动明细数据
                        if (productActivity.PublishStatus == 1)
                        {
                            //插入当前活动表
                            ProductActivityDetailPool proActivityDetailPool = new ProductActivityDetailPool();
                            proActivityDetailPool.CopyProperty(proActivityDetail);
                            proActivityDetailPool.CreatedBy = currentUser;
                            proActivityDetailPool.CreatedOn = DateTime.Now;
                            proActivityDetailPool.IsDeleted = 0;
                            proActivityDetailPool.Id = CommonRules.GUID;


                            _context.ProductActivityDetailPool.Add(proActivityDetailPool);
                        }
                    }
                }
                else//单个产品做活动
                {
                    ProductActivityDetail proActivityDetail = new ProductActivityDetail();

                    //插入活动历史表

                    proActivityDetail.CopyProperty(productActivity);
                    proActivityDetail.Id = CommonRules.GUID;
                    proActivityDetail.ProductActivityId = productActivity.Id;
                    proActivityDetail.ProductId = productActivity.ParentProductIds;
                    proActivityDetail.SubProductId = productActivity.ProductIds;
                    proActivityDetail.ProductPrice = Convert.ToDecimal(productActivity.ProOrginPrice);
                    proActivityDetail.ActivityPrice = Convert.ToDecimal(productActivity.ActivityProPrices);
                    proActivityDetail.CreatedOn = DateTime.Now;
                    proActivityDetail.CreatedBy = currentUser;
                    proActivityDetail.IsDeleted = 0;
                    _context.ProductActivityDetail.Add(proActivityDetail);

                    //发布状态的话插入活动明细数据
                    if (productActivity.PublishStatus == 1)
                    {
                        //插入当前活动表
                        ProductActivityDetailPool proActivityDetailPool = new ProductActivityDetailPool();
                        proActivityDetailPool.CopyProperty(productActivity);
                        proActivityDetailPool.ProductActivityId = productActivity.Id;
                        proActivityDetailPool.ProductId = productActivity.ParentProductIds;
                        proActivityDetailPool.SubProductId = productActivity.ProductIds;
                        proActivityDetailPool.ProductPrice = Convert.ToDecimal(productActivity.ProOrginPrice);
                        proActivityDetailPool.ActivityPrice = Convert.ToDecimal(productActivity.ActivityProPrices);
                        proActivityDetailPool.Id = CommonRules.GUID;
                        proActivityDetailPool.CreatedOn = DateTime.Now;
                        proActivityDetailPool.CreatedBy = currentUser;
                        proActivityDetailPool.IsDeleted = 0;
                        _context.ProductActivityDetailPool.Add(proActivityDetailPool);
                    }
                }


                if (createStatus)
                {
                    _context.ProductActivity.Add(productActivity);
                }
                else
                {
                    var act = _context.ProductActivity.SingleOrDefault(a => a.Id.Equals(productActivity.Id));
                    act.ModifiedBy = currentUser;
                    act.ModifiedOn = DateTime.Now;
                    act.PublishStatus = productActivity.PublishStatus;

                }
                //移除过期活动
                RemoveTimeoutActivity();
                if (_context.SaveChanges() > 0)
                {
                    result.Successed = true;
                }
            }
            catch (Exception e)
            {
                result.Message = "活动创建失败!";
            }

            return result;
        }

        /// <summary>
        /// 根据子产品Id获得正在进行的活动信息（取最后创建的活动）
        /// </summary>
        /// <param name="subProductId"></param>
        /// <returns></returns>
        public ProductActivityDetailPool GetActivityDetailPoolBySubProductId(string subProductId)
        {
            var result = _context.ProductActivityDetailPool.Where(o => o.SubProductId == subProductId && o.BeginValidDate <= DateTime.Now && o.EndValidDate >= DateTime.Now && o.IsDeleted != 1)
                        .OrderByDescending(o => o.CreatedOn).FirstOrDefault();
            return result;
        }
    }
}
