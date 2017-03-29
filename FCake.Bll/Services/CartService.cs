using FCake.Domain;
using FCake.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCake.Core.Common;
using FCake.Domain.WebModels;

namespace FCake.Bll.Services
{
    public class CartService
    {
        private EFDbContext context = new EFDbContext();
        private readonly ProductService _productService = new ProductService();
        public string LinkId
        {
            get { return CurrentMember.CookieCartId; }
        }
        public string CurrMemberId
        {
            get { return CurrentMember.MemberId; }
        }
        /// <summary>
        /// 根据用户ID或linkid取购物车的条数
        /// </summary>
        /// <returns>int 条数</returns>
        public int GetCartsCount()
        {
            int count = 0;
            if (this.CurrMemberId.IsNullOrTrimEmpty() && this.LinkId.IsNullOrTrimEmpty())
                return count;
            if (this.CurrMemberId.IsNullOrTrimEmpty())
            {//未登录状态购物车数据
                var data = GetAllCarts().Where(o => o.LinkID.Equals(this.LinkId) && o.CreatedBy == string.Empty).ToList();
                if (data.Count() > 0)
                    count = data.Sum(d => d.Num);
            }
            else
            {//登录状态购物车数据
                var data = GetAllCarts().Where(o => o.CreatedBy == this.CurrMemberId).ToList();
                if (data.Count() > 0)
                    count = data.Sum(d => d.Num);

            }
            return count;
        }

        /// <summary>
        /// 由用户ID或LINKID(cookie)取出购物车数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<CartVM> GetCarts()
        {
            List<CartVM> result = new List<CartVM>();
            List<CartVM> cartList = new List<CartVM>();

            if (this.LinkId.IsNullOrTrimEmpty())
                return result;
            if (this.CurrMemberId.IsNullOrTrimEmpty())
            {//未登录状态购物车数据
                cartList = GetAllCarts().Where(o => o.LinkID.Equals(this.LinkId) && o.CreatedBy == string.Empty).ToList();
            }
            else
            {//登录状态购物车数据
                cartList = GetAllCarts().Where(o => o.CreatedBy == this.CurrMemberId).ToList();
            }
            foreach (var r in cartList)
            {
                r.SizeTitle = _productService.GetSizeTitleBuySubProductId(r.Id);
                r.Price = _productService.GetLastProductPrice(r.Id, r.Price, r.ProductType, true, this.CurrMemberId);
            }
            //其他类型产品放最后
            var productList = cartList.Where(o => o.ProductType != CommonRules.OtherProductTypeDicValue).ToList();
            var otherProductList = cartList.Where(o => o.ProductType == CommonRules.OtherProductTypeDicValue).ToList();
            if (productList != null)
                result.AddRange(productList);
            if (otherProductList != null)
                result.AddRange(otherProductList);
            return result;
        }
        /// <summary>
        /// 增加产品到购物车
        /// </summary>
        /// <param name="subProductID"></param>
        public string AddCart(string subProductID, int num = 1)
        {
            var product = (from x in context.SubProducts
                           join y in context.Products on x.ParentId equals y.Id
                           where x.Id.Equals(subProductID)
                           && x.IsDeleted != 1
                           && y.IsDeleted != 1
                           && y.SaleStatus == 1//上架
                           select x);
            if (product.Any())
            {
                var cart = context.Carts.Where(a => a.IsDeleted != 1 && a.LinkID.Equals(this.LinkId) && a.CreatedBy == string.Empty && a.SubProductID.Equals(subProductID)).FirstOrDefault();
                if (this.CurrMemberId.IsNullOrTrimEmpty())
                {//未登录状态
                }
                else
                {//已登录状态
                    cart = context.Carts.Where(a => a.IsDeleted != 1 && a.CreatedBy == this.CurrMemberId && a.SubProductID.Equals(subProductID)).FirstOrDefault();
                }
                if (cart == null)
                {
                    cart = new Cart();
                    cart.LinkID = this.LinkId;
                    cart.IsDeleted = 0;
                    cart.CreatedBy = this.CurrMemberId;
                    cart.CreatedOn = DateTime.Now;
                    cart.Id = DataHelper.GetSystemID();
                    cart.Num = num;
                    cart.SubProductID = subProductID;
                    context.Carts.Add(cart);
                }
                else
                {
                    cart.Num += num;
                    cart.ModifiedBy = this.CurrMemberId;
                    cart.ModifiedOn = DateTime.Now;
                }
                context.SaveChanges();
                return cart.Id;
            }
            return null;
        }
        /// <summary>
        /// 从购物车移除产品
        /// </summary>
        /// <param name="subProductID"></param>
        public void RemoveCart(string subProductID)
        {
            var carts = context.Carts.Where(a => a.IsDeleted != 1 && a.LinkID.Equals(this.LinkId) && a.SubProductID.Equals(subProductID) && a.CreatedBy == string.Empty);
            if (this.CurrMemberId.IsNullOrTrimEmpty())
            {//未登录状态
                context.Carts.RemoveRange(carts);
            }
            else
            {//已登录状态
                carts = context.Carts.Where(a => a.IsDeleted != 1 && a.CreatedBy.Equals(this.CurrMemberId) && a.SubProductID.Equals(subProductID));
                context.Carts.RemoveRange(carts);
            }
            context.SaveChanges();
        }
        /// <summary>
        /// 从购物车移除产品
        /// </summary>
        /// <param name="subProductID"></param>
        public void RemoveCart(List<string> cartIDs)
        {
            var carts = context.Carts.Where(a => a.IsDeleted != 1 && a.LinkID.Equals(this.LinkId) && cartIDs.Contains(a.Id) && a.CreatedBy == string.Empty);
            if (this.CurrMemberId.IsNullOrTrimEmpty())
            {//未登录状态
                context.Carts.RemoveRange(carts);
            }
            else
            {//已登录状态
                carts = context.Carts.Where(a => a.IsDeleted != 1 && a.CreatedBy.Equals(this.CurrMemberId) && cartIDs.Contains(a.Id));
                context.Carts.RemoveRange(carts);
            }
            context.SaveChanges();
        }
        /// <summary>
        /// 从购物车移除产品
        /// </summary>
        /// <param name="subProductID"></param>
        public void RemoveCart()
        {
            var carts = context.Carts.Where(a => a.IsDeleted != 1 && a.LinkID.Equals(this.LinkId) && a.CreatedBy == string.Empty);
            if (this.CurrMemberId.IsNullOrTrimEmpty())
            {//未登录状态
                context.Carts.RemoveRange(carts);
            }
            else
            {//已登录状态
                carts = context.Carts.Where(a => a.IsDeleted != 1 && a.CreatedBy.Equals(this.CurrMemberId));
                context.Carts.RemoveRange(carts);

            }
            context.SaveChanges();
        }
        /// <summary>
        /// 变更产品数量
        /// </summary>
        /// <param name="subProductID"></param>
        public bool ChangeCartNum(string cartId, int num)
        {
            var cart = context.Carts.Where(a => a.IsDeleted != 1 && a.LinkID.Equals(this.LinkId) && a.Id.Equals(cartId) && a.CreatedBy == string.Empty).FirstOrDefault();
            if (this.CurrMemberId.IsNullOrTrimEmpty())
            {//未登录状态
            }
            else
            {//已登录状态
                cart = context.Carts.Where(a => a.IsDeleted != 1 && a.Id.Equals(cartId) && a.CreatedBy.Equals(this.CurrMemberId)).FirstOrDefault();
            }
            if (cart != null)
            {
                cart.Num = num;
                cart.ModifiedBy = this.CurrMemberId;
                cart.ModifiedOn = DateTime.Now;
            }
            return context.SaveChanges() > 0;
        }
        /// <summary>
        /// 变更购物车中的生日卡
        /// </summary>
        /// <param name="cartID"></param>
        /// <param name="BirthdayCard"></param>
        /// <returns></returns>
        public bool ChangeCartBirthdayCard(string cartID, string BirthdayCard)
        {
            var cart = context.Carts.Where(a => a.IsDeleted != 1 && a.LinkID.Equals(this.LinkId) && a.Id.Equals(cartID) && a.CreatedBy == string.Empty).FirstOrDefault();
            if (this.CurrMemberId.IsNullOrTrimEmpty())
            {//未登录状态
            }
            else
            {//登录状态
                cart = context.Carts.Where(a => a.IsDeleted != 1 && a.Id.Equals(cartID) && a.CreatedBy.Equals(this.CurrMemberId)).FirstOrDefault();
            }
            if (cart != null)
            {
                cart.BirthdayCard = BirthdayCard;
                cart.ModifiedBy = this.CurrMemberId;
                cart.ModifiedOn = DateTime.Now;
            }
            return context.SaveChanges() > 0;
        }
        public List<CartVM> CheckedCarts(List<string> cartIds)
        {
            List<CartVM> result = new List<CartVM>();
            if (this.LinkId.IsNullOrTrimEmpty())
                return result;

            var cartList = GetAllCarts().Where(o => cartIds.Contains(o.CartID)).ToList();
            if (this.CurrMemberId.IsNullOrTrimEmpty())
            {//未登录状态购物车数据
                cartList = cartList.Where(o => o.LinkID.Equals(this.LinkId) && o.CreatedBy == string.Empty).ToList();
            }
            else
            {//登录状态购物车数据
                cartList = cartList.Where(o => o.CreatedBy.Equals(this.CurrMemberId)).ToList();
            }
            foreach (var c in cartList)
            {
                c.SizeTitle = _productService.GetSizeTitleBuySubProductId(c.Id);
                c.Price = _productService.GetLastProductPrice(c.Id, c.Price, c.ProductType, true, this.CurrMemberId);
            }
            //其他类型产品放最后
            var productList = cartList.Where(o => o.ProductType != CommonRules.OtherProductTypeDicValue).ToList();
            var otherProductList = cartList.Where(o => o.ProductType == CommonRules.OtherProductTypeDicValue).ToList();
            if (productList != null)
                result.AddRange(productList);
            if (otherProductList != null)
                result.AddRange(otherProductList);
            //result = cartList;
            return result;
        }

        public List<string> GetCartIDsByLinkID()
        {
            if (this.CurrMemberId.IsNullOrTrimEmpty())
            {//未登录状态
                var result = (from x in context.Carts
                              where x.IsDeleted != 1
                               && x.LinkID.Equals(this.LinkId)
                               && x.CreatedBy == string.Empty
                              select x.Id).ToList();
                return result;
            }
            else
            {//登录状态
                var result = (from x in context.Carts
                          where x.IsDeleted != 1
                           && x.CreatedBy.Equals(this.CurrMemberId)
                          select x.Id).ToList();
                return result;
            }

        }

        /// <summary>
        /// 获得所有的购物车数据
        /// </summary>
        /// <returns></returns>
        public IQueryable<CartVM> GetAllCarts()
        {
            var carts = from x in context.Carts
                        join y in context.SubProducts on x.SubProductID equals y.Id
                        join z in context.Products on y.ParentId equals z.Id
                        join m in context.BaseFiles on z.MainImgId equals m.Id
                        where x.IsDeleted != 1
                        && y.IsDeleted != 1
                        && z.IsDeleted != 1
                        && m.IsDeleted != 1
                        && z.SaleStatus == 1
                        orderby x.CreatedOn
                        select new CartVM()
                        {
                            Id = y.Id,
                            PID = z.Id,
                            PName = z.Name,
                            Url = m.Url,
                            Size = y.Size,
                            Price = (decimal)(y.Price == null ? 0 : y.Price),
                            ProductType = z.Type,
                            OriginalPrice = y.OriginalPrice,
                            Num = x.Num,
                            BirthdayCard = x.BirthdayCard,
                            CartID = x.Id,
                            LinkID = x.LinkID,
                            CreatedBy = x.CreatedBy,
                            InadvanceHours = z.InadvanceHours == null ? 0 : (int)z.InadvanceHours
                        };

            return carts;
        }


        /// <summary>
        /// 用户登录后合并购物车
        /// </summary>
        public void MergeCart()
        {
            if (!this.CurrMemberId.IsNullOrTrimEmpty())
            {
                //用户登录成功
                //取出当前用户的购物车数据
                var curUserCarts = GetAllCarts();
                curUserCarts = curUserCarts.Where(c => c.CreatedBy.Equals(this.CurrMemberId));
                var curuserCartList = curUserCarts.ToList();
                List<string> curuserCartIds = new List<string>();
                foreach (var cc in curuserCartList)
                {
                    curuserCartIds.Add(cc.Id);
                }
                //取出当前浏览器购物车数据
                var browserCarts = GetAllCarts();
                var browserCartList = browserCarts.Where(c => c.LinkID.Equals(this.LinkId) && c.CreatedBy == string.Empty).ToList();
                foreach (var browserCart in browserCartList)
                {
                    var bc = context.Carts.FirstOrDefault(c => c.Id == browserCart.CartID);
                    if (curuserCartIds.Contains(bc.SubProductID))
                    {
                        var curBrowserCart = context.Carts.FirstOrDefault(c => c.SubProductID == bc.SubProductID && c.LinkID == this.LinkId && c.IsDeleted != 1 && c.CreatedBy == string.Empty);
                        if (curBrowserCart != null)
                        {
                            var curUserCart = context.Carts.FirstOrDefault(c => c.SubProductID == bc.SubProductID && c.CreatedBy == this.CurrMemberId && c.IsDeleted != 1);
                            curUserCart.Num += bc.Num;
                            context.Carts.Remove(curBrowserCart);
                        }
                    }
                    bc.CreatedBy = this.CurrMemberId;

                }
                context.SaveChanges();
            }
        }
    }
}
