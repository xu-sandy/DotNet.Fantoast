using FCake.Core.Common;
using FCake.Core.MvcCommon;
using FCake.Core.Security;
using FCake.Domain;
using FCake.Domain.Common;
using FCake.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.HPSF;
using System.Data.SqlClient;
using System.Data;
using FCake.API.EChi;
using FCake.API;

namespace FCake.Bll.Services
{
    public class CouponsService : BaseService
    {
        public EFDbContext _context = new EFDbContext();
        private readonly SMSService _smsService = new SMSService();
        #region 后台优惠券管理
        /// <summary>
        /// 根据条件获取优惠券信息
        /// </summary>
        /// <param name="whereLambda">lambda表达式</param>
        /// <returns></returns>
        public List<Coupons> GetCouponsByWhere(Expression<Func<Coupons, bool>> whereLambda)
        {
            return _context.Coupons.Where(whereLambda).ToList();
        }
        /// <summary>
        /// 获取单个优惠券
        /// </summary>
        /// <param name="couponId"></param>
        /// <returns></returns>
        public Coupons GetCreateCouponInfo(string couponId)
        {
            return _context.Coupons.SingleOrDefault(p => p.Id.Contains(couponId) && p.IsDeleted != 1);
        }
        /// <summary>
        /// 获取所有的优惠券信息
        /// 分页获取
        /// </summary>
        /// <param name="pageInfo">分页信息</param>
        /// <returns></returns>
        public List<Coupons> GetCouponsByPageInfo(PageInfo pageInfo, out int totalCount)
        {
            totalCount = _context.Coupons.Where(a => a.IsDeleted != 1).Count();
            return _context.Coupons.Where(a => a.IsDeleted != 1).OrderByDescending(a => a.CreatedOn).Skip((pageInfo.Page - 1) * pageInfo.Rows).Take(pageInfo.Rows).ToList();
        }
        /// <summary>
        /// 删除优惠券信息
        /// </summary>
        /// <param name="couponId"></param>
        /// <param name="deleteUser"></param>
        /// <returns></returns>
        public OpResult DropCoupons(string couponId, string deleteUser)
        {
            OpResult result = new OpResult() { Successed = false };
            try
            {
                Coupons coupon = _context.Coupons.SingleOrDefault(p => p.Id.Equals(couponId));
                coupon.ModifiedOn = DateTime.Now;
                coupon.ModifiedBy = deleteUser;
                coupon.IsDeleted = 1;
                if (_context.SaveChanges() > 0)
                {
                    result.Successed = true;
                }
            }
            catch (Exception e)
            {
                result.Message = "删除失败!";
            }
            return result;
        }
        /// <summary>
        /// 创建优惠券
        /// </summary>
        /// <param name="coupons">优惠券实体</param>
        /// <returns>成功或失败，失败信息</returns>
        public OpResult CreateCoupons(Coupons coupons)
        {
            OpResult result = new OpResult() { Successed = false };
            bool isEdit = false;
            try
            {
                if (coupons.Id == null)
                {
                    //id为空 添加id 
                    coupons.Id = CommonRules.GUID;
                    coupons.CouponBatch = CommonRules.CommonNoRules("couponbatch");
                }
                else
                {
                    isEdit = true;
                }

                //如果优惠券是要生成的则执行生成的代码
                if (coupons.Status == 1)
                {
                    if (coupons.GiveWay == 1)
                    {
                        //hstodo:检查短信条数
                        //if (coupons.IsSendSMS == 1 && coupons.DistributingType == 0)
                        //{
                        //    int SMSBalance = 0;
                        //    var SMSBalanceStr = EChiHelper.GetBalance_Normal();
                        //    if (!string.IsNullOrEmpty(SMSBalanceStr))
                        //    {
                        //        try
                        //        {
                        //            SMSBalance = int.Parse(SMSBalanceStr);
                        //        }
                        //        catch (Exception ex)
                        //        {
                        //            return OpResult.Fail("对不起，查询短信余额时发生错误，错误：" + SMSBalanceStr);
                        //        }
                        //        if (SMSBalance < coupons.Quantity)
                        //        {
                        //            return OpResult.Fail(string.Format("对不起，需要发送短信{0}条，短信余额剩余{1}条，短信余额不足，请充值。", coupons.Quantity, SMSBalance));
                        //        }
                        //    }
                        //}
                        Random _random = new Random();
                        switch (coupons.GivenObjectType)
                        {
                            case 1://1的时候是所有用户都加入数据
                                List<Customers> datas = new FCake.Bll.CustomersService().GetCustomerByWhere(a => a.IsDeleted != 1);
                                coupons.Quantity = datas.Count();
                                foreach (var item in datas)
                                {
                                    CreateCouponsDetail(coupons, item, _random);
                                }
                                break;
                            case 2://2的时候按会员等级来
                                List<Customers> customers = new FCake.Bll.CustomersService().GetCustomerByWhere(a => coupons.GivenObjectIds.Contains(a.MemberLevelValue.ToString()));
                                coupons.Quantity = customers.Count();
                                foreach (var item in customers)
                                {
                                    CreateCouponsDetail(coupons, item, _random);
                                }
                                break;
                            case 3://3的时候按指定用户来
                                List<Customers> customer = new FCake.Bll.CustomersService().GetCustomerByWhere(a => coupons.GivenObjectIds.Contains(a.Id));
                                if (coupons.DistributingType == 1)
                                {
                                    var cu = customer.FirstOrDefault();
                                    if (cu != null)
                                    {
                                        for (var i = 0; i < coupons.Quantity; i++)
                                        {
                                            CreateCouponsDetail(coupons, cu, _random);
                                        }
                                    }
                                }
                                else
                                {
                                    coupons.Quantity = customer.Count();
                                    foreach (var item in customer)
                                    {
                                        CreateCouponsDetail(coupons, item, _random);
                                    }
                                }
                                break;
                        }
                    }
                    else
                    {
                        var createCount = coupons.Quantity;
                        Random _random = new Random();
                        //不绑定客户生成优惠券
                        for (int i = 0; i < createCount; i++)
                        {
                            CouponDetail couponDetail = new CouponDetail() { Id = CommonRules.GUID };
                            couponDetail.CouponId = coupons.Id;   //  优惠券id
                            couponDetail.CouponSN = CommonRules.CouponsNo(12, _random);   //优惠券号
                            //查询重复
                            var data = _context.CouponDetails.SingleOrDefault(p => p.CouponSN.Equals(couponDetail.CouponSN) && p.IsDeleted != 1);
                            if (data != null)
                            {
                                createCount++;
                                continue;
                            }
                            //couponDetail.MemberId = customers.Id;
                            couponDetail.UseState = 0;     //生成状态
                            couponDetail.Title = coupons.Title;
                            couponDetail.CouponBatch = coupons.CouponBatch;
                            couponDetail.Denomination = coupons.Denomination;
                            couponDetail.SalesMoney = coupons.SalesMoney;
                            couponDetail.ConditionMoney = coupons.ConditionMoney;
                            couponDetail.BeginValidDate = coupons.BeginValidDate;
                            couponDetail.EndValidDate = coupons.EndValidDate;
                            couponDetail.CreatedOn = DateTime.Now;
                            couponDetail.CreatedBy = coupons.CreatedBy;
                            couponDetail.IsDeleted = 0;
                            _context.CouponDetails.Add(couponDetail);
                        }
                    }
                }
                if (!isEdit)
                {
                    _context.Coupons.Add(coupons);
                }
                else
                {
                    var cdata = _context.Coupons.SingleOrDefault(p => p.Id.Equals(coupons.Id) && p.IsDeleted != 1);
                    if (cdata != null)
                    {
                        cdata.CopyProperty(coupons);
                        cdata.ModifiedOn = DateTime.Now;
                    }
                }
                if (_context.SaveChanges() > 0)
                {
                    result.Successed = true;
                    result.Message = "创建优惠券成功!";
                    result.Code = coupons.Quantity.ToString();

                    #region 发送短信

                    if (coupons.IsSendSMS == 1 && coupons.Status == 1)
                    {
                        List<string> mobileList = new List<string>();
                        mobileList = GetMobileListByCouponId(coupons.Id);
                        //将需要发送短信的号码加入result.Data中,导出Excel手动发送
                        result.Data = mobileList;
                    }
                    #endregion
                }
                else
                {
                    result.Message = "创建优惠券失败!";
                }
            }
            catch (Exception e)
            {
                result.Message = "创建优惠券发生异常！";
            }
            return result;
        }

        /// <summary>
        /// 获取优惠券发送的手机号码
        /// </summary>
        /// <param name="couponId"></param>
        /// <returns></returns>
        public List<string> GetMobileListByCouponId(string couponId)
        {
            List<string> mobileList = new List<string>();
            string sendSMSErrorMsg = string.Empty;
            var coupons = _context.Coupons.Where(o => o.Id == couponId && o.IsDeleted == 0).FirstOrDefault();
            if (coupons != null)
            {
                List<string> list = (from cd in _context.CouponDetails
                                     join u in _context.Customers on cd.MemberId equals u.Id
                                     where (!string.IsNullOrEmpty(u.Mobile) && cd.CouponId == coupons.Id)
                                     select new
                                     {
                                         mobile = u.Mobile
                                     }).ToList().Select(o => o.mobile).ToList();
                if (list != null)
                {
                    mobileList = list;
                }
            }
            return mobileList;
        }

        /// <summary>
        /// 将优惠券短信发送状态修改为已发送
        /// </summary>
        /// <param name="couponId"></param>
        /// <returns></returns>
        public OpResult SetCouponSMSAlreadySend(string couponId)
        {
            var result = OpResult.Fail("操作失败");
            var coupons = _context.Coupons.Where(o => o.Id == couponId).FirstOrDefault();
            if (coupons != null)
            {
                coupons.SendSMSStatus = 1;
                coupons.ModifiedOn = DateTime.Now;
                var t = _context.SaveChanges();
                result = OpResult.Success("设置成功");
            }
            return result;
        }

        /// <summary>
        /// 导出生成优惠券时手动发送短信的手机号码
        /// </summary>
        /// <param name="mobileList"></param>
        /// <returns></returns>
        public OpResult ExportCouponsSendSMSMobile(List<string> mobileList)
        {
            OpResult result = new OpResult() { Successed = false };
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("mobile");
                foreach (var mobile in mobileList)
                {
                    if (!string.IsNullOrEmpty(mobile))
                    {
                        DataRow row = dt.NewRow();
                        row["mobile"] = mobile;
                        dt.Rows.Add(row);
                    }
                }
                if (dt.Rows.Count > 0)
                {
                    string[] fields = new string[] { "mobile" };
                    string[] names = new string[] { "手机号码" };
                    FCake.Core.Common.ExportExcel excelExport = new ExportExcel();
                    var fileName = DateTime.Now.ToString("手动发送短信手机号_yyyyMMddHHmmss");
                    int[] marger = { 0 };
                    Dictionary<int, int> dic = new Dictionary<int, int>();
                    dic.Add(0, 16);
                    string fileUrl = excelExport.ToExcel(fileName, dt, fields, names, marger, null, dic);
                    return OpResult.Success("优惠券生成成功，导出手机号码成功，请下载并提交至叮咚云手动发送", null, fileUrl);
                }
                else
                {
                    result.Message = "优惠券生成成功，无手机号码导出!";
                }
            }
            catch (Exception e)
            {
                result.Message = "优惠券生成成功，手机号码导出失败：" + e.Message;
            }
            return result;
        }
        /// <summary>
        /// 新增优惠券从表数据
        /// </summary>
        /// <param name="coupons">优惠券主表数据</param>
        /// <param name="customers">优惠券绑定客户信息</param>
        /// <returns></returns>
        public OpResult CreateCouponsDetail(Coupons coupons, Customers customers, Random _random)
        {
            OpResult result = new OpResult() { Successed = false };
            try
            {
                CouponDetail couponDetail = new CouponDetail() { Id = CommonRules.GUID };
                couponDetail.CouponId = coupons.Id;   //  优惠券id
                couponDetail.CouponSN = CommonRules.CouponsNo(12, _random);   //优惠券号
                couponDetail.MemberId = customers.Id;
                couponDetail.UseState = 0;     //生成状态
                couponDetail.CouponBatch = coupons.CouponBatch;
                couponDetail.Title = coupons.Title;
                couponDetail.Denomination = coupons.Denomination;
                couponDetail.SalesMoney = coupons.SalesMoney;
                couponDetail.ConditionMoney = coupons.ConditionMoney;
                couponDetail.BeginValidDate = coupons.BeginValidDate;
                couponDetail.EndValidDate = coupons.EndValidDate;
                couponDetail.CreatedOn = DateTime.Now;
                couponDetail.CreatedBy = coupons.CreatedBy;
                couponDetail.IsDeleted = 0;
                _context.CouponDetails.Add(couponDetail);
                result.Successed = true;
            }
            catch (Exception e)
            {
                result.Message = e.Message;
            }
            return result;
        }
        /// <summary>
        /// 获取优惠券明细数据
        /// </summary>
        /// <param name="pageInfo"></param>
        /// <param name="totalCount"></param>
        /// <returns></returns>
        public dynamic GetCouponDetailByPageInfo(PageInfo pageInfo, out int totalCount)
        {
            totalCount = _context.CouponDetails.Where(p => p.IsDeleted != 1).Count();
            //return _context.CouponDetail.Where(p => p.IsDeleted != 1).OrderByDescending(p => p.CreatedOn).Skip((pageInfo.Page - 1) * pageInfo.Rows).Take(pageInfo.Rows).ToList();
            var result = (from x in _context.CouponDetails
                          join cu1 in _context.Customers
                          on x.MemberId equals cu1.Id into temp1
                          join cu2 in _context.Customers
                          on x.UseMemberId equals cu2.Id into temp2
                          from cu1 in temp1.DefaultIfEmpty()
                          from cu2 in temp2.DefaultIfEmpty()
                          where x.IsDeleted != 1
                          select new
                          {
                              x.CouponSN,
                              x.Denomination,
                              x.SalesMoney,
                              x.CreatedOn,
                              x.BeginValidDate,
                              x.EndValidDate,
                              x.UseDate,
                              x.UseState,
                              x.UseMemberId,
                              UseOrderSN = x.UseOrderSN,
                              x.Title,
                              OwnerMobile = cu1.Mobile,
                              OwnerTel = cu1.Tel,
                              OwnerFullName = cu1.FullName,
                              UsedMobile = cu2.Mobile,
                              UsedTel = cu2.Tel,
                              UsedFullName = cu2.FullName,

                          }).OrderByDescending(p => p.CreatedOn).Skip((pageInfo.Page - 1) * pageInfo.Rows).Take(pageInfo.Rows);
            return result;
        }
        /// <summary>
        /// 优惠券明细数据过滤
        /// </summary>
        /// <param name="couponsNo"></param>
        /// <param name="user"></param>
        /// <param name="status"></param>
        /// <param name="faceVal"></param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public dynamic GetCouponsDetails(CouponDetail coupondetail, PageInfo pageInfo, out int totalCount)
        {
            var query = _context.CouponDetails.Where(p => p.IsDeleted != 1);
            if (!coupondetail.CouponSN.IsNullOrEmpty())
            {
                query = query.Where(p => p.CouponSN.Contains(coupondetail.CouponSN));
            }
            if (!coupondetail.MemberId.IsNullOrEmpty())
            {//coupondetail.MemberId 传手机号
                var customer = new CustomersService().GetCustomerByWhere(a => a.Mobile.Contains(coupondetail.MemberId) && a.IsDeleted != 1);
                if (customer.Count() > 0)
                {
                    string ids = "";
                    foreach (var item in customer)
                    {
                        ids += item.Id + ",";
                    }
                    query = query.Where(p => ids.Contains(p.MemberId));
                }
            }
            //-1=全部
            if (coupondetail.UseState != -1)
            {
                query = query.Where(a => a.UseState.Equals(coupondetail.UseState));
            }
            if (coupondetail.CouponBatch.IsNotNullOrEmpty())
            {
                query = query.Where(a => a.CouponBatch.Contains(coupondetail.CouponBatch));
            }
            if (coupondetail.Denomination > 0)
            {
                query = query.Where(a => a.Denomination.Equals(coupondetail.Denomination));
            }
            if (coupondetail.BeginValidDate != new DateTime())
            {
                query = query.Where(a => a.BeginValidDate < coupondetail.BeginValidDate && a.EndValidDate > coupondetail.BeginValidDate);
            }
            //if (coupondetail.EndValidDate != new DateTime())
            //{
            //    query = query.Where(a => a.EndValidDate < coupondetail.EndValidDate);
            //}
            totalCount = query.Count();
            var result = (from x in query
                          join cu1 in _context.Customers
                          on x.MemberId equals cu1.Id into temp1
                          join cu2 in _context.Customers
                          on x.UseMemberId equals cu2.Id into temp2
                          from cu1 in temp1.DefaultIfEmpty()
                          from cu2 in temp2.DefaultIfEmpty()
                          where x.IsDeleted != 1
                          select new
                          {
                              x.CouponSN,
                              x.Denomination,
                              x.SalesMoney,
                              x.CreatedOn,
                              x.BeginValidDate,
                              x.EndValidDate,
                              x.UseDate,
                              x.UseState,
                              x.UseMemberId,
                              x.ConditionMoney,
                              UseOrderSN = x.UseOrderSN,
                              x.Title,
                              OwnerMobile = cu1.Mobile,
                              OwnerTel = cu1.Tel,
                              OwnerFullName = cu1.FullName,
                              UsedMobile = cu2.Mobile,
                              UsedTel = cu2.Tel,
                              UsedFullName = cu2.FullName,
                              CouponBatch = x.CouponBatch
                          }).OrderBy(p => p.UseState).ThenByDescending(p => p.CreatedOn).Skip((pageInfo.Page - 1) * pageInfo.Rows).Take(pageInfo.Rows);

            return result.ToList();

        }

        #endregion

        #region 订单优惠券公共处理方法
        /// <summary>
        /// 根据会员Id获得该会员所有可用的优惠券
        /// </summary>
        /// <returns></returns>
        public List<CouponDetail> GetEnabledCouponDetailByMemberId(string memberId)
        {
            var result = GetCouponDetailByMemberId(memberId).Where(o => o.UseState == 0 && o.BeginValidDate <= DateTime.Now && o.EndValidDate >= DateTime.Now).ToList();
            if (result == null)
                result = new List<CouponDetail>();
            return result;

        }
        /// <summary>
        /// 根据会员Id获得该会员所拥有的优惠券
        /// </summary>
        /// <returns></returns>
        public List<CouponDetail> GetCouponDetailByMemberId(string memberId)
        {
            var result = _context.CouponDetails.Where(o => o.MemberId == memberId && o.IsDeleted != 1).OrderByDescending(o => o.Denomination).ThenBy(o => o.EndValidDate).ToList();
            if (result == null)
                result = new List<CouponDetail>();
            return result;
        }
        /// <summary>
        /// 根据会员Id获得该会员所拥有的优惠券分页信息
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="count"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public List<CouponDetail> GetCouponDetailPagingByMemberId(string memberId, out int count, int pageSize = 10, int pageIndex = 1)
        {
            var list = GetCouponDetailByMemberId(memberId);
            count = 0;
            if (list != null)
            {
                //超过一个月，不可用优惠券不显示
                list = list.Where(o => (o.UseState == 0 && o.EndValidDate >= DateTime.Now.AddMonths(-1)) || (o.UseState == 1 && o.UseDate != null && o.UseDate >= DateTime.Now.AddMonths(-1))).ToList();
                count = list.Count();
                if (count != 0)
                    list = list.OrderByDescending(o => o.EndValidDate).ThenByDescending(o => o.Denomination).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            }
            else
            {
                list = new List<CouponDetail>();
            }
            return list;
        }
        /// <summary>
        /// 根据客户id获取客户所有优惠券
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="pageinfo"></param>
        /// <returns></returns>
        public List<CouponDetail> GetCouponDetailToDatagridByMemberId(string memberId, out int totalCount, PageInfo pageinfo)
        {
            var result = _context.CouponDetails.Where(p => p.MemberId.Equals(memberId) && p.IsDeleted != 1).OrderBy(p => p.UseState).ThenByDescending(p => p.CreatedOn).Skip((pageinfo.Page - 1) * pageinfo.Rows).Take(pageinfo.Rows).ToList();
            totalCount = result.Count();
            if (result == null)
            {
                result = new List<CouponDetail>();
                totalCount = 0;
            }
            return result;
        }
        /// <summary>
        /// 根据优惠券号绑定优惠券
        /// </summary>
        /// <param name="couponSN"></param>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public OpResult BindCouponDetailByCouponSN(string couponSN, string memberId)
        {
            var result = new OpResult();
            result = OpResult.Fail("对不起，绑定失败");
            var coupon = _context.CouponDetails.Where(o => o.UseState == 0 && o.CouponSN == couponSN && o.BeginValidDate <= DateTime.Now && o.EndValidDate >= DateTime.Now && o.IsDeleted != 1)
            .FirstOrDefault();
            if (coupon == null)
            {
                result = OpResult.Fail("对不起，您输入的优惠券号不可用，请换一个试试");
            }
            else
            {
                if (!string.IsNullOrEmpty(coupon.MemberId))
                {
                    if (coupon.MemberId == memberId)
                    {
                        result = OpResult.Fail("对不起，该优惠券已经绑定了，请勿重复绑定");
                    }
                    else
                    {
                        result = OpResult.Fail("对不起，该优惠券已被其他用户绑定");
                    }
                }
                else
                {
                    coupon.MemberId = memberId;
                    _context.SaveChanges();
                    result = OpResult.Success("绑定成功", "1", coupon);
                }
            }
            return result;
        }
        /// <summary>
        /// 判断优惠券是否可用
        /// </summary>
        /// <param name="couponIdList"></param>
        /// <returns></returns>
        public OpResult ValidateCouponDetailsIsAllowUse(List<string> couponIdList)
        {
            var result = OpResult.Success();
            if (couponIdList != null)
            {
                foreach (var couponId in couponIdList)
                {
                    var couponDetail = _context.CouponDetails.Where(o => o.Id == couponId && o.IsDeleted != 1).FirstOrDefault();
                    if (couponDetail == null)
                    {
                        result = OpResult.Fail(string.Format("对不起，Id为{0}优惠券不存在", couponId));
                        return result;
                    }
                    if (couponDetail.BeginValidDate > DateTime.Now || couponDetail.EndValidDate < DateTime.Now)
                    {
                        result = OpResult.Fail(string.Format("对不起，Id为{0}优惠券不在有效期内", couponId));
                        return result;
                    }
                    if (couponDetail.UseState == 1)
                    {
                        result = OpResult.Fail(string.Format("对不起，Id为{0}优惠券已被使用", couponId));
                        return result;
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// 优惠券导出到excel
        /// </summary>
        /// <param name="coupons"></param>
        /// <param name="memberMobile"></param>
        /// <returns></returns>
        public OpResult CouponsExport(CouponDetail coupons, string memberMobile)
        {
            SqlParameter[] param = new SqlParameter[7];
            if (coupons.BeginValidDate != new DateTime())
            {
                param[0] = new SqlParameter("@beginValidDate", coupons.BeginValidDate);
            }
            else
            {
                param[0] = new SqlParameter("@beginValidDate", DBNull.Value);
            }
            if (coupons.EndValidDate != new DateTime())
            {
                param[1] = new SqlParameter("@endValidDate", coupons.EndValidDate);
            }
            else
            {
                param[1] = new SqlParameter("@endValidDate", DBNull.Value);
            }
            if (coupons.CouponSN.IsNotNullOrEmpty())
            {
                param[2] = new SqlParameter("@couponSN", coupons.CouponSN);
            }
            else
            {
                param[2] = new SqlParameter("@couponSN", DBNull.Value);
            }
            param[3] = new SqlParameter("@useState", SqlDbType.SmallInt, 2);
            param[3].Value = coupons.UseState;
            param[4] = new SqlParameter("@denomination", coupons.Denomination);
            if (memberMobile.IsNotNullOrEmpty())
            {
                param[5] = new SqlParameter("@memberId", memberMobile);
            }
            else
            {
                param[5] = new SqlParameter("@memberId", DBNull.Value);
            }
            if (coupons.CouponBatch.IsNotNullOrEmpty())
            {
                param[6] = new SqlParameter("@couponBatch", coupons.CouponBatch);
            }
            else
            {
                param[6] = new SqlParameter("@couponBatch", DBNull.Value);
            }
            try
            {
                DataTable dt = _context.Database.SqlQueryForDataTatable("EXEC dbo.Proc_CouponsExport @useState = @useState,@beginValidDate = @beginValidDate,@endValidDate = @endValidDate,@couponSN = @couponSN,@denomination = @denomination,@memberId = @memberId,@couponBatch=@couponBatch", param);
                if (dt.Rows.Count == 0)
                {
                    return OpResult.Fail("无可导出数据!");
                }
                dt = UpdateDataTable(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows[i]["ConditionMoney"].ToString() != "0.0000")
                    {
                        dt.Rows[i]["ConditionMoney"] = "满" + String.Format("{0:F}", Convert.ToDecimal(dt.Rows[i]["ConditionMoney"])) + "元使用";
                    }
                    else
                    {
                        dt.Rows[i]["ConditionMoney"] = "无条件使用";
                    }
                }
                var fileName = DateTime.Now.ToString("yyyyMMddHHmmss");
                string[] fields = { "CouponBatch", "CouponSN", "Title", "Denomination", "CouponStatus", "ConditionMoney", "FullName", "Mobile", "useFullName", "UseDate", "UseOrderSN", "BeginValidDate", "EndValidDate" };
                string[] names = { "批次号", "优惠券券号", "优惠券名称", "金额", "使用状态", "使用条件", "拥有者姓名", "拥有者电话", "使用人", "使用时间", "使用订单", "有效期开始时间", "有效期结束时间" };
                Dictionary<int, int> colWidth = new Dictionary<int, int>();
                colWidth.Add(0, 20);
                colWidth.Add(1, 20);
                FCake.Core.Common.ExportExcel exceExport = new ExportExcel();
                string fileUrl = exceExport.ToExcel(fileName, dt, fields, names, null, null, colWidth);
                return OpResult.Success("导出成功！", null, fileUrl);
            }
            catch (Exception ex)
            {
                return OpResult.Fail("导出失败:" + ex.Message);
            }

        }

        private DataTable UpdateDataTable(DataTable argDataTable)
        {
            DataTable dtResult = new DataTable();
            //克隆表结构
            dtResult = argDataTable.Clone();
            foreach (DataColumn col in dtResult.Columns)
            {
                if (col.ColumnName == "ConditionMoney")
                {
                    //修改列类型
                    col.DataType = typeof(String);
                }
            }
            foreach (DataRow row in argDataTable.Rows)
            {
                DataRow rowNew = dtResult.NewRow();
                rowNew["CouponBatch"] = row["CouponBatch"];
                rowNew["CouponSN"] = row["CouponSN"];
                rowNew["FullName"] = row["FullName"];
                rowNew["Mobile"] = row["Mobile"];
                rowNew["CouponStatus"] = row["CouponStatus"];
                rowNew["useFullName"] = row["useFullName"];
                rowNew["ConditionMoney"] = row["ConditionMoney"];
                rowNew["UseDate"] = row["UseDate"];
                rowNew["UseOrderSN"] = row["UseOrderSN"];
                rowNew["Title"] = row["Title"];
                rowNew["Denomination"] = row["Denomination"];
                rowNew["BeginValidDate"] = row["BeginValidDate"];
                rowNew["EndValidDate"] = row["EndValidDate"];
                dtResult.Rows.Add(rowNew);
            }
            return dtResult;
        }


        #endregion

        #region 注册用户时发放优惠券
        public OpResult RegisterDistributingCoupons(string userId)
        {
            var result = OpResult.Fail();
            try
            {
                if (CommonRules.RegisterCouponsQuantity <= 0)
                {
                    return OpResult.Success("无需赠送优惠券");
                }
                var registerCouponsSMS = new FCake.Bll.Services.MsgTemplateService().GetMsgTempByCategory("RegisterCoupons");
                if (string.IsNullOrEmpty(registerCouponsSMS))
                {
                    registerCouponsSMS = "";
                }
                Coupons coupons = new Coupons();
                coupons.CreatedBy = "System";
                coupons.CreatedOn = DateTime.Now;
                coupons.Title = CommonRules.RegisterCouponsTitle;
                coupons.Denomination = CommonRules.RegisterCouponsDenomination;
                coupons.ConditionMoney = CommonRules.RegisterCouponsConditionMoney;
                coupons.SalesMoney = 0;
                coupons.Quantity = CommonRules.RegisterCouponsQuantity;
                coupons.BeginValidDate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd 00:00:00"));
                coupons.EndValidDate = DateTime.Parse(DateTime.Now.AddMonths(1).ToString("yyyy-MM-dd 23:59:59"));
                coupons.GiveWay = 1;
                coupons.GivenObjectType = 3;
                coupons.GivenObjectIds = userId;
                coupons.IsSendSMS = CommonRules.RegisterCouponsIsSendSMS;
                coupons.SMSContent = registerCouponsSMS;
                coupons.Status = 1;
                coupons.DistributingType = 1;
                coupons.SendSMSStatus = 1;

                result = CreateCoupons(coupons);

                if (result.Successed && coupons.IsSendSMS == 1)
                {//发送营销短信
                    var user = _context.Customers.Where(o => o.Id == userId).FirstOrDefault();
                    if (user != null)
                        DDSMSHelper.SendMarketingSMS(user.Mobile, coupons.SMSContent);
                }
            }
            catch (Exception ex)
            {
                return result;
            }
            return result;
        }
        #endregion
    }
}
