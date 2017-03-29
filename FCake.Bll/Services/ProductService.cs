using FCake.Bll;
using FCake.Core.MvcCommon;
using FCake.Domain;
using FCake.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FCake.Domain.Common;
using FCake.Core.Common;
using System.Web;
using FCake.Bll.Services;
using System.Collections;
using FCake.Domain.WebModels;
using FCake.Domain.Enums;
using System.Configuration;
using System.Data.Entity;

namespace FCake.Bll
{
    public class ProductService
    {
        EFDbContext context = new EFDbContext();
        private readonly CommonService _commonService = new CommonService();
        private readonly ProductActivityService _productActivityService = new ProductActivityService();
        private readonly CustomersService _customersService = new CustomersService();
        /// <summary>
        /// 审核产品
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>

        #region 后台
        public OpResult ReviewStatus(string id, int status, string userId)
        {
            var result = new OpResult() { Successed = false, Message = "操作失败,找不到对应数据" };
            try
            {
                var product = GetProduct(id);
                if (product != null)
                {
                    if (product.Status.Value != status)
                    {
                        result.Message = "数据已过期,请刷新页面";
                    }
                    else
                    {
                        var newStatus = 0;
                        if (status == 0)
                            newStatus = 1;
                        if (status == 1)
                        {
                            if (product.SaleOn == null || product.SaleOn <= DateTime.Now)
                            {
                                newStatus = 3;
                                product.SaleStatus = 1;
                            }
                            else
                                newStatus = 2;
                        }
                        ChangeProductStatusAndReviewLog(product, userId, newStatus, ref result);
                    }
                }
                else
                {
                    result.Message = "数据已过期,请刷新页面";
                }
            }
            catch (Exception ex)
            {
                result.Successed = false;
                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 上架下架产品
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public OpResult ChangeSaleStatus(string id, int status, string userId)
        {
            var result = new OpResult() { Successed = false, Message = "操作失败,找不到对应数据" };
            try
            {
                var product = GetProduct(id);
                if (product != null)
                {
                    if (product.Status.Value != status)
                    {
                        result.Message = "数据已过期,请刷新页面";
                    }
                    else
                    {
                        var newStatus = 2;
                        if (status == 2)
                            newStatus = 3;
                        if (status == 3)
                            newStatus = 4;

                        if (newStatus == 3)
                        {
                            product.SaleOn = DateTime.Now;
                            product.SaleStatus = 1;
                        }
                        if (newStatus == 4)
                        {
                            product.SaleOff = DateTime.Now;
                            product.SaleStatus = 2;
                        }
                        ChangeProductStatusAndReviewLog(product, userId, newStatus, ref result);
                    }
                }
                else
                {
                    result.Message = "数据已过期,请刷新页面";
                }
            }
            catch (Exception ex)
            {
                result.Successed = false;
                result.Message = ex.Message;
            }
            return result;
        }
        public OpResult ReChangeSaleStatus(string id, int status, string userId)
        {
            var result = new OpResult() { Successed = false, Message = "操作失败,找不到对应数据" };
            try
            {
                var product = GetProduct(id);
                if (product != null)
                {
                    if (product.Status.Value != status)
                    {
                        result.Message = "数据已过期,请刷新页面";
                    }
                    else
                    {
                        var newStatus = 1;
                        if (status == 4)
                        {
                            product.SaleOn = DateTime.Now;
                            product.SaleStatus = null;
                        }
                        ChangeProductStatusAndReviewLog(product, userId, newStatus, ref result);
                    }
                }
                else
                {
                    result.Message = "数据已过期,请刷新页面";
                }
            }
            catch (Exception ex)
            {
                result.Successed = false;
                result.Message = ex.Message;
            }
            return result;
        }
        private void ChangeProductStatusAndReviewLog(Product product, string userId, int status, ref OpResult result)
        {
            try
            {
                string content = string.Format("字段[Status]值由[{0}]更改为[{1}]", StringHelper.ConvertStr(product.Status.Value), StringHelper.ConvertStr(status));
                product.Status = status;
                product.ModifiedBy = userId;
                product.ModifiedOn = DateTime.Now;
                context.SaveChanges();
                result.Successed = true;
                result.Message = "操作成功";
                //保存操作日志
                OperationLogService.SaveOperLog(userId, "Product_Status", product.Id, content);
            }
            catch (Exception ex)
            {
                result.Successed = false;
                result.Message = ex.Message;
            }
        }

        /// <summary>
        /// 保存编辑的产品
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="currentUserId"></param>
        /// <returns></returns> 
        public OpResult SaveProduct(Product entity, string currentUserId, string subproduct)
        {
            OpResult result = null;
            var newentity = entity;

            Regex reg = new Regex(@"^_");

            if (string.IsNullOrEmpty(entity.Id) || reg.IsMatch(entity.Id))
            {//add
                newentity.Id = string.IsNullOrEmpty(entity.Id) ? Guid.NewGuid().ToString("N") : entity.Id.Substring(1);
                newentity.CreatedBy = currentUserId;
                newentity.CreatedOn = DateTime.Now;
                newentity.IsDeleted = 0;
                newentity.Status = 0;
                context.Products.Add(newentity);
            }
            else
            {//update
                newentity = context.Products.Where(s => s.Id == entity.Id).FirstOrDefault();
                if (newentity != null)
                {
                    newentity.Name = entity.Name;
                    newentity.EnglishName = entity.EnglishName;
                    newentity.Themes = entity.Themes;
                    newentity.Type = entity.Type;
                    newentity.Taste = entity.Taste;
                    newentity.IsRecommend = entity.IsRecommend;
                    newentity.SaleOn = entity.SaleOn;
                    newentity.SaleOff = entity.SaleOff;
                    newentity.MainImgId = entity.MainImgId;
                    newentity.Brief = entity.Brief;
                    newentity.Desc = entity.Desc;
                    newentity.InadvanceHours = entity.InadvanceHours;
                    newentity.WarmTips = entity.WarmTips;
                    newentity.ModifiedBy = currentUserId;
                    newentity.ModifiedOn = DateTime.Now;
                    newentity.Material = entity.Material;
                    newentity.SortNo = entity.SortNo;
                    newentity.Expatiate = entity.Expatiate;
                    newentity.MobileDesc = entity.MobileDesc;
                    newentity.IsShortcutButton = entity.IsShortcutButton;
                    newentity.ShortcutButtonTitle = entity.ShortcutButtonTitle;
                }
            }

            var sr = SaveSubProduct(subproduct, newentity.Id, currentUserId);
            if (sr.Successed == false)
                return sr;


            if (newentity != null)
            {
                context.SaveChanges();
                result = new OpResult() { Successed = true, Message = "保存成功" };
            }
            else
                result = new OpResult() { Successed = false, Message = "数据对象保存错误" };

            return result;
        }

        public OpResult SaveSubProduct(string codes, string pid, string userid)
        {
            var sp = codes.Split('|');
            List<string> ids = new List<string>();
            foreach (var x in sp)
            {
                ids.Add(HttpContext.Current.Server.UrlDecode(x.Split(',')[0]));
            }
            var ps = context.SubProducts.Where(a => a.ParentId.Equals(pid) && ids.Contains(a.Id) == false).ToList();
            foreach (var x in ps)
            {
                string tempid = x.Id;
                #region 如果有订单 则不能删除
                if (context.OrderDetails.Where(a => tempid.Equals(a.SubProductId) && a.IsDeleted != 1).Any())
                {
                    return new OpResult { Successed = false, Message = "磅数为" + x.Size + "的产品已被使用，不能删除" };
                }
                #endregion
                x.IsDeleted = 1;
            }

            foreach (var x in sp)
            {
                var t = x.Split(',');
                if (t.Length == 5)
                {
                    var id = HttpContext.Current.Server.UrlDecode(t[0]);
                    var size = HttpContext.Current.Server.UrlDecode(t[1]);
                    var price = HttpContext.Current.Server.UrlDecode(t[2]);
                    var originalprice = HttpContext.Current.Server.UrlDecode(t[3]);
                    var status = t[4] == "" ? 0 : int.Parse(t[4]);
                    //Nullable<ActiveStatus> status = t[4] == "" ? null : (Nullable<ActiveStatus>)(int.Parse(t[4]));
                    decimal oprice = 0;
                    decimal yhPrice = 0;
                    ;
                    if (size.IsNullOrTrimEmpty())
                        continue;

                    SubProduct sub = null;
                    if (id.IsNullOrTrimEmpty())
                    {
                        sub = new SubProduct();
                        sub.Id = FCake.Core.Common.DataHelper.GetSystemID();
                        sub.ParentId = pid;
                        sub.IsDeleted = 0;
                        sub.CreatedBy = userid;

                        sub.CreatedOn = DateTime.Now;
                        context.SubProducts.Add(sub);
                    }
                    else
                        sub = context.SubProducts.Find(id);

                    if (sub != null)
                    {
                        Decimal.TryParse(price, out yhPrice);
                        sub.Price = yhPrice;
                        Decimal.TryParse(originalprice, out oprice);
                        sub.OriginalPrice = oprice;
                        sub.Size = size;
                        sub.ModifiedBy = userid;
                        sub.Status = status;
                        sub.ModifiedOn = DateTime.Now;
                    }
                }
            }

            return new OpResult { Successed = true };

        }

        /// <summary>
        /// 获取单个产品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Product GetProduct(string id)
        {
            var result = context.Products.Where(s => s.Id == id && s.IsDeleted != 1).FirstOrDefault();
            if (result == null)
            {
                result = new Product { Id = "_" + FCake.Core.Common.DataHelper.GetSystemID() };
            }
            return result;
        }
        public Product GetProduct(string id, bool isnew)
        {
            if (isnew)
                return GetProduct(id);
            var result = context.Products.Where(s => s.Id == id && s.IsDeleted != 1).FirstOrDefault();
            return result;
        }
        /// <summary>
        /// 根据产品ID获取子产品磅数及价格
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public object GetSubProducts(string id)
        {
            var result = context.SubProducts.Where(a => a.ParentId.Equals(id) && a.IsDeleted != 1).OrderBy(a => a.Size).ThenBy(a => a.Price).ToList();
            return new { total = result.Count(), rows = result };
        }
        /// <summary>
        /// 根据产品ID获取产品信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public object GetProductsInfo(string id)
        {
            //var result = context.SubProducts.Where(a => a.ParentId.Equals(id) && a.IsDeleted != 1).OrderBy(a => a.Size).ThenBy(a => a.Price).ToList();
            //return new { total = result.Count(), rows = result };
            var result = (from x in context.SubProducts
                          join y in context.Products on x.ParentId equals y.Id
                          where x.ParentId.Equals(id) && x.IsDeleted != 1
                          select new
                          {
                              Id = x.Id,
                              ParentId = x.ParentId,
                              Size = x.Size,
                              Title = x.Title,
                              ImgUrl = x.ImgUrl,
                              Desc = x.Desc,
                              Price = x.Price,
                              OriginalPrice = x.OriginalPrice,
                              Status = x.Status,
                              Type = y.Type
                          }).OrderBy(a => a.Size).ThenBy(a => a.Price).ToList();
            return new { total = result.Count(), rows = result };
        }

        /// <summary>
        /// 获取单个产品的所有子产品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Product GetProductAll(string id)
        {
            var product = context.Products.Include("SubProducts").Include("BaseFile").Where(s => s.IsDeleted != 1 && s.Id == id).FirstOrDefault();
            return product;
        }

        /// <summary>
        /// 筛选产品数据
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        public List<Product> FilterProduct(Product product, string currentUserId, out int totalCount, PageInfo pageInfo)
        {
            var list = new PermissionService().GetPermissionCodes("Product", "Index", currentUserId);
            var isReview = list.Contains("review");

            var name = trimStr(product.Name);
            var theme = trimStr(product.Themes);
            var type = trimStr(product.Type);
            var status = trimStr(product.Status.ToString());
            var result = context.Products.Where(p => p.IsDeleted != 1);
            if (name != "")
            {
                result = result.Where(p => p.Name.Contains(name));
            }
            if (theme != "")
            {
                result = result.Where(p => (p.Themes + ",").Contains(theme + ","));
            }
            if (type != "")
            {
                result = result.Where(p => p.Type == type);
            }
            if (status != "")
            {
                result = result.Where(p => p.Status == product.Status.Value);
            }


            if (isReview)
            {
                result = result.Where(p => p.Status != null).OrderBy(p => p.Status == 0 ? 10 : p.Status).ThenByDescending(p => p.SaleOn);
            }
            else
            {
                result = result.OrderBy(p => p.Status).ThenByDescending(p => p.SaleOn);
            }

            totalCount = result.Count();
            var data = result.Skip((pageInfo.Page - 1) * pageInfo.Rows).Take(pageInfo.Rows).ToList();
            return data;
        }
        //查询条件文本裁剪
        private string trimStr(string text)
        {
            var newText = text == null ? "" : text;
            return newText.Trim();
        }

        #region private
        /// <summary>
        /// 审批记录日志？
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        private ReviewStatusLog AddReviewStatus(string id, int status, string userId)
        {
            var reviewStatus = new ReviewStatusLog()
            {
                Id = DataHelper.GetSystemID(),
                TableName = "Products",
                TableNameId = id,
                CreatedBy = userId,
                CreatedOn = DateTime.Now,
                Status = status
            };
            context.ReviewStatusLog.Add(reviewStatus);
            return reviewStatus;
        }
        #endregion

        /// <summary>
        /// 删除产品图片
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public void DeleteImg(string id)
        {
            BaseFile temp = context.BaseFiles.Find(id);
            if (temp != null)
            {
                var product = context.Products.SingleOrDefault(a => a.MainImgId.Equals(id));
                if (product != null)
                {
                    product.MainImgId = null;
                }
                temp.IsDeleted = 1;
                try
                {
                    System.IO.File.Delete(HttpContext.Current.Server.MapPath(temp.Url));
                    System.IO.File.Delete(HttpContext.Current.Server.MapPath(temp.Url + "_middle.jpg"));
                }
                catch { }
            }
            context.SaveChanges();
        }

        /// <summary>
        /// 获取子产品(规格、价格)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="totalCount"></param>
        /// <param name="pageInfo"></param>
        /// <returns></returns>
        public List<SubProduct> GetSubProduct(string id, out int totalCount, PageInfo pageInfo)
        {
            Regex reg = new Regex(@"^_");
            if (reg.IsMatch(id))
                id = id.Substring(1);

            var query = context.SubProducts.Where(a => a.ParentId.Equals(id, StringComparison.OrdinalIgnoreCase) && a.IsDeleted != 1).OrderBy(a => a.Size);
            totalCount = query.Count();
            var data = query.ToList();
            return data;
        }

        /// <summary>
        /// 由订单号获取订单中的所有产品
        /// </summary>
        /// <param name="orderid"></param>
        /// <returns></returns>
        public IQueryable<Product> GetAllProductByOrderid(string orderid)
        {
            var result = (from x in context.Products
                          where (from a in context.OrderDetails where a.OrderNo.Equals(orderid) select a.ProductId).Contains(x.Id) == false
                          select x);
            return result;
        }

        public object SearchProductByPhoneOrder(string name, string themes, string type, int page, int rows)
        {
            var data = context.Products.Where(p => p.IsDeleted != 1 && p.SaleStatus == 1);

            data = from x in data
                   where context.SubProducts.Count(a => a.IsDeleted != 1 && a.ParentId.Equals(x.Id)) > 0
                   select x;

            if (!name.IsNullOrTrimEmpty())
            {
                data = data.Where(p => p.Name.Contains(name));
            }
            if (!themes.IsNullOrTrimEmpty())
            {
                data = data.Where(p => p.Themes == themes);
            }
            if (!type.IsNullOrTrimEmpty())
            {
                data = data.Where(p => p.Type == type);
            }

            var result = data.OrderBy(a => a.Themes).Skip((page - 1) * rows).Take(rows).ToList()
                .Select(a => new
                {
                    Id = a.Id,
                    MainImgId = new BaseFileService().GetImgUrlById(a.MainImgId),
                    Name = a.Name,
                    Themes = a.Themes,
                    Type = a.Type
                });

            return new { total = data.Count(), rows = result };
        }

        public object GetOrderSubProducts(string id)
        {
            var result = (from x in context.OrderDetails
                          join y in context.Products on x.ProductId equals y.Id
                          where x.OrderNo.Equals(id)
                            && x.IsDeleted == 0
                            && y.IsDeleted == 0
                          orderby y.Type descending
                          select new
                          {
                              Id = x.SubProductId,
                              MainImgId = y.MainImgId,
                              Name = y.Name,
                              Num = x.Num,
                              Size = x.Size,
                              Price = x.Price,
                              BirthdayCard = x.BirthdayCard,
                              SizeTitle = x.SizeTitle
                          }).ToList();

            return result.Select(a => new
            {
                Id = a.Id,
                MainImgId = new BaseFileService().GetImgUrlById(a.MainImgId),
                Name = a.Name,
                Num = a.Num,
                Size = a.Size,
                Price = a.Price,
                BirthdayCard = a.BirthdayCard,
                SizeTitle = a.SizeTitle
            });
        }
        /// <summary>
        /// 删除产品图片
        /// </summary>
        /// <param name="baseFileId"></param>
        /// <returns></returns>
        public OpResult DelProductImg(string baseFileId)
        {
            var result = OpResult.Fail("删除图片失败");
            var baseFile = context.BaseFiles.Where(o => o.Id == baseFileId).FirstOrDefault();
            if (baseFile != null)
            {
                baseFile.IsDeleted = 1;
                context.SaveChanges();
                result = OpResult.Success("删除图片成功");
            }
            return result;
        }
        #endregion

        #region 前台
        #region 产品列表
        /// <summary>
        /// 产品列表
        /// </summary>
        /// <param name="type"></param>
        /// <param name="themes"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public PageList<VProductModel> GetProducts(string type, string themes, int pageSize = 6, int pageIndex = 1)
        {
            var query = context.Database.SqlQuery<VProductModel>("select * from VProducts").AsQueryable();
            if (themes != null && themes.Trim() != "")
            {
                query = query.Where(p => p.Themes.Contains(themes));
            }
            if (type != null && type != "")
            {
                query = query.Where(p => p.Type !=null && p.Type.Contains(type));
            }
            if (type != CommonRules.OtherProductTypeDicValue)
            {
                query = query.Where(p => p.Type != CommonRules.OtherProductTypeDicValue);
            }
            query = query.OrderByDescending(p => p.IsRecommend).ThenByDescending(p => p.SortNo).AsQueryable();
            var vProductModelList = new PageList<VProductModel>(query, pageIndex, pageSize);
            foreach (var vProductModel in vProductModelList)
            {
                vProductModel.SizeTitle = GetSizeTitleBuyProductId(vProductModel.ProductId, vProductModel.Size);
                //列表中显示活动价
                //var salePrice = (decimal)vProductModel.Price;
                //vProductModel.Price = GetLastProductPrice(vProductModel.SubProductId, (decimal)vProductModel.Price, vProductModel.Type);
                var activity = _productActivityService.GetActivityDetailPoolBySubProductId(vProductModel.SubProductId);
                if (activity != null)
                {//有活动
                    vProductModel.isHasActivity = 1;
                    vProductModel.ActivityPrice = activity.ActivityPrice;
                }
            }
            vProductModelList = new PageList<VProductModel>(vProductModelList.OrderByDescending(p => p.IsRecommend).ThenByDescending(o => o.isHasActivity).ThenByDescending(p => p.SortNo).AsQueryable());
            return vProductModelList;
        }
        #endregion

        #region 热卖产品
        /// <summary>
        /// 热卖产品
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public PageList<VProductModel> GetHotProducts(int pageSize, int pageIndex)
        {
            var regetIndex = 0;
        reget:
            var query = context.Database.SqlQuery<VProductModel>("select * from VHotProducts").AsQueryable();
            query = query.Where(o => o.Type != CommonRules.OtherProductTypeDicValue);
            query = query.OrderByDescending(p => p.CreatedOn);
            var data = new PageList<VProductModel>(query, pageIndex, pageSize);

            if (data.Count < pageSize && regetIndex == 0)
            {
                regetIndex = 1;
                new HotProductService().ResetHotProducts(pageSize);
                goto reget;
            }
            if (data.Count > 0)
            {
                foreach (var d in data)
                {
                    d.SizeTitle = GetSizeTitleBuyProductId(d.ProductId, d.Size);
                    //热卖列表显示活动价
                    //d.Price = GetLastProductPrice(d.SubProductId, (decimal)d.Price, d.Type);
                    var activity = _productActivityService.GetActivityDetailPoolBySubProductId(d.SubProductId);
                    if (activity != null)
                    {//有活动
                        d.isHasActivity = 1;
                        d.ActivityPrice = activity.ActivityPrice;
                    }
                }
                return data;
            }
            return null;
        }
        #endregion

        /// <summary>
        /// 根据id查询产品的详细信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Product GetProductById(string id)
        {
            //取产品主表数据
            //var data = context.Products.Include("SubProducts").Where(p => p.Id.Equals(id) && p.SaleStatus == 1 && p.IsDeleted != 1).FirstOrDefault();
            var data = context.Products.Include("SubProducts").Where(p => p.Id.Equals(id) && p.SaleStatus == 1 && p.IsDeleted != 1).FirstOrDefault();
            if (data != null && data.SubProducts != null && data.SubProducts.Count > 0)
            {
                data.SubProducts = data.SubProducts.Where(sp => sp.IsDeleted != 1 && sp.Status != 1).ToList();
            }
            if (data != null)
            {
                if (data.SubProducts != null)
                {
                    foreach (var subProduct in data.SubProducts)
                    {
                        subProduct.SizeTitle = GetSizeTitleBuySubProductId(subProduct.Id);
                        decimal oldPrice = (decimal)subProduct.Price;
                        subProduct.PriceTitle = "";
                        subProduct.Price = GetLastProductPrice(subProduct.Id, (decimal)subProduct.Price, data.Type, true, CurrentMember.MemberId);
                        if (subProduct.Price != oldPrice)
                        {
                            if (GetLastProductPrice(subProduct.Id, oldPrice, data.Type) == (decimal)subProduct.Price)
                            {
                                subProduct.PriceTitle = "活动价 ";
                            }else
                            {
                                subProduct.PriceTitle = "会员价 ";
                            }
                        }
                    }
                }

                return data;
            }
            return null;
        }

        /// <summary>
        /// 根据产品ID查询产品的图片
        /// </summary>
        /// <param name="proId"></param>
        /// <returns></returns>
        public List<BaseFile> GetBaseFileByProductId(string proId)
        {
            if (proId != null && proId != "")
            {
                var data = context.BaseFiles.Where(p => p.LinkId.Equals(proId) && p.IsDeleted != 1).OrderBy(bf => bf.Sorting).ToList();
                if (data.Count > 0)
                {
                    return data;
                }
            }
            return null;
        }

        #endregion

        /// <summary>
        /// 返回5个随机餐具产品
        /// </summary>
        /// <returns></returns>
        public List<T> GetTop5cutleryProducts<T>() where T : new()
        {
            List<T> result = new List<T>();
            var outType = typeof(T);
            var outProInfos = outType.GetProperties();
            var outNames = outProInfos.Select(a => a.Name);

            List<SubProduct> subProductList = new List<SubProduct>();
            List<Product> productList = new List<Product>();
            productList = context.Products.Include(o => o.SubProducts).Include(o => o.BaseFile).Where(o => o.Type == CommonRules.OtherProductTypeDicValue && o.SaleStatus == 1 && o.IsDeleted != 1).ToList();
            foreach (var product in productList)
            {
                var subProduct = product.SubProducts.Where(o => o.IsDeleted != 1).OrderBy(o => o.Size).FirstOrDefault();
                if (subProduct != null)
                    subProductList.Add(subProduct);
            }

            var products = (from x in subProductList
                            select new
                            {
                                x.Id,
                                x.Product.BaseFile.Url
                            }).OrderBy(a => Guid.NewGuid()).Take(5);

            foreach (var cart in products)
            {
                T outItem = new T();
                var type = cart.GetType();
                var proInfos = type.GetProperties();
                foreach (var x in proInfos.Where(p => outNames.Contains(p.Name)))
                {
                    var p = outProInfos.SingleOrDefault(a => a.Name.Equals(x.Name));
                    p.SetValue(outItem, x.GetValue(cart, null), null);
                }
                result.Add(outItem);
            }
            return result;
        }

        /// <summary>
        /// 返回所有产品的名称列表
        /// </summary>
        /// <typeparam name="?"></typeparam>
        /// <returns></returns>
        public List<Product> GetAllProducts()
        {
            var query = (from p in context.Products where p.IsDeleted != 1 && p.SaleStatus == 1 select p).ToList();
            return query;
        }
        /// <summary>
        /// 加载用户最爱蛋糕
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<Product> GetUserFavoriteCakeByUser(string userId)
        {
            var proData = context.Products.Where(p => p.IsDeleted != 1 && p.SaleStatus == 1).ToList();
            var customer = context.Customers.SingleOrDefault(p => p.Id.Equals(userId));
            if (customer.FavoriteCake != null && customer.FavoriteCake != "")
            {
                var data = proData.Select(p => p.Id.Equals(customer.FavoriteCake));
                if (data != null)
                {
                    var d = context.Products.SingleOrDefault(p => p.Id.Equals(customer.FavoriteCake));
                    proData.Add(d);
                }
            }
            return proData;
        }
        /// <summary>
        /// 根据子产品Id获得其产品规格单位
        /// </summary>
        /// <param name="subProductId"></param>
        /// <returns></returns>
        public string GetSizeTitleBuySubProductId(string subProductId)
        {
            string sizeTitle = "";
            var subProduct = context.SubProducts.Include(o => o.Product).Where(o => o.Id == subProductId).FirstOrDefault();
            if (subProduct != null)
            {
                if (subProduct.Product != null)
                {
                    sizeTitle = subProduct.Product.Type == CommonRules.OtherProductTypeDicValue ? _commonService.GetDictionaryName("ProductUnitOther", subProduct.Size) : _commonService.GetDictionaryName("ProductUnitCake", subProduct.Size);
                }
            }

            return sizeTitle;
        }

        /// <summary>
        /// 根据产品Id获得其产品规格单位
        /// </summary>
        /// <param name="subProductId"></param>
        /// <returns></returns>
        public string GetSizeTitleBuyProductId(string productId, string subProductSize)
        {
            string sizeTitle = "";
            var product = context.Products.Where(o => o.Id == productId).FirstOrDefault();
            if (product != null)
            {
                sizeTitle = product.Type == CommonRules.OtherProductTypeDicValue ? _commonService.GetDictionaryName("ProductUnitOther", subProductSize) : _commonService.GetDictionaryName("ProductUnitCake", subProductSize);
            }
            return sizeTitle;
        }
        /// <summary>
        /// 获得可做为快捷按钮的子产品
        /// </summary>
        /// <returns></returns>
        public List<SubProduct> GetShortcutSubProduct()
        {
            List<SubProduct> subProductList = new List<SubProduct>();
            List<Product> productList = new List<Product>();
            productList = context.Products.Include(o => o.SubProducts).Include(o => o.BaseFile).Where(o => o.IsShortcutButton == 1 && o.SaleStatus == 1 && o.IsDeleted != 1).ToList();
            foreach (var product in productList)
            {
                var subProduct = product.SubProducts.Where(o => o.IsDeleted != 1).OrderBy(o => o.Size).FirstOrDefault();
                if (subProduct != null)
                    subProductList.Add(subProduct);
            }

            return subProductList;
        }
        /// <summary>
        /// 获得产品最终售价（产品售价、活动价、会员折扣）
        /// </summary>
        /// <param name="subProductId"></param>
        /// <param name="subProductPrice"></param>
        /// <param name="isCalculateDiscountPrice">是否计算会员折扣价</param>
        /// <returns></returns>
        public decimal GetLastProductPrice(string subProductId, decimal subProductPrice, string productType, bool isCalculateDiscountPrice = false, string memberId = "")
        {
            var result = subProductPrice;
            var activity = _productActivityService.GetActivityDetailPoolBySubProductId(subProductId);
            if (activity != null)
                result = activity.ActivityPrice;
            if (productType == CommonRules.OtherProductTypeDicValue)
                return result;
            if (isCalculateDiscountPrice && memberId.IsNullOrTrimEmpty() == false)
            {
                var discountRate = _customersService.GetDiscountRateByMemberId(memberId);
                var dicountPrice = Math.Round(subProductPrice * discountRate, 2, MidpointRounding.AwayFromZero);
                if (dicountPrice < result)
                    result = dicountPrice;
            }


            return result;
        }

    }
}
