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
using System.Linq.Expressions;
using System.Data.SqlClient;
using System.Data;
using FCake.Core.Common;
using FCake.Domain.Enums;

namespace FCake.Bll
{
    public class GiftCardDetailService : BaseService
    {
        EFDbContext context = new EFDbContext();

        #region 后台代金卡管理

        #region 创建一个新卡对象，但不保存在数据库
        /// <summary>
        /// 获得一个新卡对象，但不持久化到数据库
        /// </summary>
        /// <param name="denomination"></param>
        /// <param name="beginValidDate"></param>
        /// <param name="endValidDate"></param>
        /// <param name="salesMoney"></param>
        /// <param name="createdBy"></param>
        /// <returns></returns>
        public GiftCardDetail MakeGiftCardAsCanRecharge(GiftCards giftCards, Random _random)
        {
            var results = new GiftCardDetail();
            results.Id = CommonRules.GUID;
            results.GiftCardId = giftCards.Id;
            results.GiftCardSN = CommonRules.GiftCardSN(12, _random);//生成12位
            results.GiftCardPwd = CommonRules.GiftCardPwd(6, _random);//6位密码
            results.Title = giftCards.Title;
            results.Denomination = giftCards.Denomination;
            results.SalesMoney = giftCards.SalesMoney;
            results.BeginValidDate = giftCards.BeginValidDate;
            results.EndValidDate = giftCards.EndValidDate;
            results.CreatedBy = giftCards.CreatedBy;
            results.CreatedOn = DateTime.Now;
            results.GiftBatch = giftCards.GiftBatch;

            ///让对象成为刚生成状态
            results.UseState = (short)Domain.Enums.GiftCardUseStatus.UnUsed;
            return results;
        }
        #endregion

        public int PublishManyGiftCard(GiftCards giftCards)
        {
            var publishCount = 0;
            var quantity = giftCards.Quantity;
            Random _random = new Random();
            for (int i = 0; i < quantity; i++)
            {
                var obj = MakeGiftCardAsCanRecharge(giftCards, _random);
                var count = context.GiftCardDetail.Where(p => p.GiftCardSN.Equals(obj.GiftCardSN) && p.IsDeleted != 1).Count();
                if (count > 0)
                {
                    quantity++;
                }
                else
                {
                    context.GiftCardDetail.Add(obj);
                }
                publishCount += context.SaveChanges();
            }
            return publishCount;
        }


        //获取所有代金卡信息
        public List<GiftCardDetail> GetGiftCardAll(out int totalCount, PageInfo pageInfo)
        {
            var query = context.GiftCardDetail.Where(c => c.IsDeleted != 1);
            totalCount = query.Count();
            var data = query.OrderByDescending(c => c.CreatedOn).Skip((pageInfo.Page - 1) * pageInfo.Rows).Take(pageInfo.Rows).ToList();
            return data;
        }

        /// <summary>
        /// 根据时间段查询代金卡信息
        /// </summary>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="totalCount"></param>
        /// <param name="pageInfo"></param>
        /// <returns></returns>
        public IQueryable GetGiftCardsByTime(string giftBatch, string giftCardNo, DateTime? beginTime, DateTime? endTime, decimal? denomination, out int totalCount, PageInfo pageInfo)
        {


            var result = from c in context.GiftCardDetail
                         join cu1 in context.Customers
                         on c.MemberId equals cu1.Id into temp1
                         join cu2 in context.Customers
                         on c.UseMemberId equals cu2.Id into temp2
                         from cu1 in temp1.DefaultIfEmpty()
                         from cu2 in temp2.DefaultIfEmpty()
                         select new
                         {
                             giftCardDetail = c,
                             ownerMemberName = (cu1.FullName == null) ? cu1.Mobile : cu1.FullName,
                             usedMemberName = (cu2.FullName == null) ? cu2.Mobile : cu2.FullName
                         };
            //动态构建查询条件
            if (beginTime != null || endTime != null)
            {
                if (beginTime != null)
                {
                    result = result.Where(p => p.giftCardDetail.CreatedOn > beginTime);
                }
                if (endTime != null)
                {
                    result = result.Where(p => p.giftCardDetail.CreatedOn < endTime);
                }
            }
            if (denomination.HasValue)
            {
                result = result.Where(p => p.giftCardDetail.Denomination == denomination);
            }
            if (giftBatch.IsNotNullOrEmpty())
            {
                result = result.Where(p => p.giftCardDetail.GiftBatch.Contains(giftBatch) && p.giftCardDetail.IsDeleted != 1);
            }
            if (giftCardNo.IsNotNullOrEmpty())
            {
                result = result.Where(p => p.giftCardDetail.GiftCardSN.Contains(giftCardNo) && p.giftCardDetail.IsDeleted != 1);
            }

            totalCount = result.Count();
            var data = result.OrderByDescending(c => c.giftCardDetail.CreatedOn).Skip((pageInfo.Page - 1) * pageInfo.Rows).Take(pageInfo.Rows);

            return data;
        }
        /// <summary>
        /// 代金卡导出到excel
        /// </summary>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public OpResult ExportExcel(DateTime? beginTime, DateTime? endTime, decimal? denomination, string giftBatch, string giftCardNo)
        {
            OpResult result = new OpResult();
            result.Successed = false;
            try
            {
                SqlParameter[] param = new SqlParameter[5];
                param[0] = new SqlParameter("@beginTime", beginTime);
                param[1] = new SqlParameter("@endTime", endTime);
                param[2] = new SqlParameter("@denomination", denomination == null ? "" : denomination.ToString());
                param[3] = new SqlParameter("@giftBatch", giftBatch == null ? "" : giftBatch);
                param[4] = new SqlParameter("@giftCardNo", giftCardNo == null ? "" : giftCardNo);

                var dt = context.Database.SqlQueryForDataTatable("EXEC dbo.[Proc_GiftCardDetailExport] @beginTime,@endTime,@denomination,@giftBatch,@giftCardNo", param);
                if (dt == null || dt.Rows.Count < 1)
                {
                    result.Message = "无导出数据，请选择正确的时间段!";
                    return result;
                }

                string[] fields = { "GiftBatch", "GiftCardSN", "GiftCardPwd", "Denomination", "SalesMoney", "CreatedOn", "BeginValidDate", "EndValidDate", "UseDate", "UseOrderSN", "UseMember", "UseState" };
                string[] names = { "批次号", "代金卡卡号", "代金卡密码", "面值", "销售价", "生成时间", "有效期始于", "有效期终于", "使用时间", "使用订单号", "使用会员", "使用状态" };

                var fileName = DateTime.Now.ToString("yyyyMMddHHmmss");

                FCake.Core.Common.ExportExcel excelExport = new FCake.Core.Common.ExportExcel();
                string fileUrl = excelExport.ToExcel(fileName, dt, fields, names, null);
                result.Successed = true;
                result.Message = "导出成功";
                result.Data = fileUrl;

            }
            catch (Exception e)
            {
                result.Message = "导出失败:" + e.Message;
            }
            return result;
        }
        /// <summary>
        /// 根据会员id获取所有的优惠券
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="totalCount"></param>
        /// <param name="pageinfo"></param>
        /// <returns></returns>
        public List<GiftCardDetail> GetGiftCardToDataGridByMemberId(string memberId, out int totalCount, PageInfo pageinfo)
        {
            var result = context.GiftCardDetail.Where(p => p.MemberId.Equals(memberId) && p.IsDeleted != 1).OrderBy(p => p.UseState).ThenByDescending(p => p.CreatedOn).Skip((pageinfo.Page - 1) * pageinfo.Rows).Take(pageinfo.Rows).ToList();
            totalCount = result.Count;
            if (result == null)
            {
                result = new List<GiftCardDetail>();
                totalCount = 0;
            }
            return result;
        }
        #endregion

        #region 订单代金卡公共处理方法
        /// <summary>
        /// 通过代金卡卡号和密码验证代金卡是否允许使用
        /// </summary>
        /// <param name="giftCardSN"></param>
        /// <param name="giftCardPwd"></param>
        /// <returns></returns>
        public OpResult VerificationGiftCardAllowUse(string giftCardSN, string giftCardSPwd)
        {
            var result = OpResult.Fail("对不起，您输入的代金卡不存在，请检查卡号或密码是否正确");
            var giftCard = context.GiftCardDetail.Where(o => o.GiftCardSN == giftCardSN && o.GiftCardPwd == giftCardSPwd && o.IsDeleted != 1).FirstOrDefault();
            if (giftCard != null)
            {
                if (giftCard.UseState != 0)
                    result = OpResult.Fail(string.Format("对不起，您输入的代金卡{0}，无法使用", giftCard.UseState == 2 ? "已回收" : "已使用"));
                else if (giftCard.BeginValidDate >= DateTime.Now || giftCard.EndValidDate <= DateTime.Now)
                    result = OpResult.Fail(string.Format("对不起，您输入的代金卡不在有效期内，该卡有效期为{0}至{1}", giftCard.BeginValidDate.ToString("yyyy-MM-dd"),
                                            giftCard.EndValidDate.ToString("yyyy-MM-dd")));
                else
                {
                    result = OpResult.Success("该代金卡可以使用", "1", giftCard);
                }
            }
            return result;
        }
        /// <summary>
        /// 验证代金卡是否允许使用
        /// </summary>
        /// <param name="GiftCardIds"></param>
        /// <returns></returns>
        public OpResult ValidateGiftCardDetailAllowUse(List<string> giftCardIdList)
        {
            var result = OpResult.Success();
            if (giftCardIdList != null)
            {
                foreach (var giftCardId in giftCardIdList)
                {
                    var giftCard = context.GiftCardDetail.Where(o => o.Id == giftCardId && o.IsDeleted != 1).FirstOrDefault();
                    if (giftCard == null)
                    {
                        result = OpResult.Fail(string.Format("对不起，Id为{0}的代金卡不存在", giftCard));
                        return result;
                    }
                    if (giftCard.UseState != 0)
                    {
                        result = OpResult.Fail(string.Format("对不起，Id为{0}的代金卡已使用或已回收", giftCard));
                        return result;
                    }
                    if (giftCard.BeginValidDate > DateTime.Now || giftCard.EndValidDate < DateTime.Now)
                    {
                        result = OpResult.Fail(string.Format("对不起，Id为{0}的代金卡不在有效期内", giftCard));
                        return result;
                    }

                }
            }
            return result;
        }
        /// <summary>
        /// 通过会员Id获得该会员所有使用的代金卡
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public List<GiftCardDetail> GetGiftCardDetailByUsedMemberId(string memberId)
        {
            var result = context.GiftCardDetail.Where(o => o.UseState == 1 && o.UseMemberId == memberId && o.IsDeleted != 1).OrderByDescending(o => o.UseDate).ToList();
            return result;
        }
        public List<GiftCardDetail> GetGiftCardDetailPagingByUsedMemberId(string memberId, out int count, int pageSize = 10, int pageIndex = 1)
        {
            var list = GetGiftCardDetailByUsedMemberId(memberId);
            count = 0;
            if (list != null)
            {
                count = list.Count();
                if (count != 0)
                    list = list.OrderByDescending(o => o.UseDate).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            }
            else
            {
                list = new List<GiftCardDetail>();
            }
            return list;
        }
        #endregion
    }
}
