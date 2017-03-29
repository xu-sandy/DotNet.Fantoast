using FCake.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCake.Domain.Entities;

namespace FCake.Bll
{
    public class HotProductService
    {
        private EFDbContext context = new EFDbContext();
        /// <summary>
        /// 重置热门产品
        /// </summary>
        public void ResetHotProducts(int count = 6)
        {
            try
            {
                //删除热门产品
                var data = context.HotProducts.Where(p => p.Id != null || p.Id == "");
                foreach (var item in data)
                {
                    context.HotProducts.Remove(item);
                }
                //从订单按数量插入数据
                //var temp = from p in context.OrderDetails
                //           group p by p.ProductId into g
                //           select g;

                var temp = (from a in context.OrderDetails
                            join p in context.Products
                            on a.ProductId equals p.Id
                            where p.IsDeleted != 1 && p.SaleStatus == 1
                            && p.Type != CommonRules.OtherProductTypeDicValue
                            group a by a.ProductId into g
                            select new
                            {
                                ProductID = g.Key,
                                Num = g.Sum(t => t.Num)
                            }).OrderByDescending(a => a.Num).Take(count);
                foreach (var item in temp)
                {
                    var hot = new HotProduct();
                    hot.ProductID = item.ProductID;
                    hot.Num = item.Num;
                    hot.Id = FCake.Core.Common.DataHelper.GetSystemID();
                    hot.CreatedOn = DateTime.Now;
                    context.HotProducts.Add(hot);
                }
                //如果数量不足，则从产品表插入剩余数量的产品(不在热门产品中)
                //优先取推荐产品
                if (temp.Count() < count)
                {
                    var pro = from a in context.Products.Where(p => p.IsDeleted != 1 && p.SaleStatus == 1 && p.Type != CommonRules.OtherProductTypeDicValue)
                                  .OrderByDescending(p => p.IsRecommend).ThenByDescending(p => p.SortNo).Take(count - temp.Count()) select new { ProductID = a.Id, Num = 1 };
                    foreach (var item in pro)
                    {
                        var hotPro = new HotProduct();
                        hotPro.ProductID = item.ProductID;
                        hotPro.Num = item.Num;
                        hotPro.Id = FCake.Core.Common.DataHelper.GetSystemID();
                        hotPro.CreatedOn = DateTime.Now;
                        //item.CreatedBy = UserCache.CurrentUser.Id;
                        context.HotProducts.Add(hotPro);
                    }
                }
                //数据更新保存
                context.SaveChanges();
            }
            catch
            {

            }
        }
    }
}
