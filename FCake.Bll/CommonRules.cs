using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using FCake.Domain;

namespace FCake.Bll
{
    /// <summary>
    /// 项目上的各种规则归集
    /// </summary>
    public class CommonRules
    {
        public static string GUID
        {
            get { return System.Guid.NewGuid().ToString().Replace("-", "").ToUpper(); }
        }
        /// <summary>
        /// 通用券号生成
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string CommonCardRule(int length, Random _random)
        {
            char[] _resultCardSN = new char[length];
            char[] _giftCardSN = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G' };
            //Random _random = new Random();
            for (int i = 0; i < length; i++)
            {
                _resultCardSN[i] = _giftCardSN[_random.Next(_giftCardSN.Length - 1)];
            }
            return new string(_resultCardSN);
        }
        /// <summary>
        /// 产品类型为其他的数据字典值
        /// </summary>
        public static string OtherProductTypeDicValue
        {
            get { return string.IsNullOrEmpty(ConfigurationManager.AppSettings["other_product_type_dicvalue"]) ? "other" : ConfigurationManager.AppSettings["other_product_type_dicvalue"]; }
        }
        /// <summary>
        /// 代金卡卡号
        /// </summary>
        /// <returns></returns>
        public static string GiftCardSN(int length, Random _random)
        {
            //byte[] buffer = Guid.NewGuid().ToByteArray();
            //return BitConverter.ToInt64(buffer, 0).ToString().ToUpper();
            return CommonCardRule(length, _random);
        }
        /// <summary>
        /// 代金卡密码
        /// </summary>
        /// <returns></returns>
        public static string GiftCardPwd(int length, Random _random)
        {
            //long i = 1;
            //foreach (byte b in Guid.NewGuid().ToByteArray())
            //{
            //    i *= ((int)b + 1);
            //}
            //return string.Format("{0:x}", i - DateTime.Now.Ticks).ToUpper();
            char[] _resultCardPwd = new char[length];
            char[] _giftCardPwd = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

            for (int i = 0; i < length; i++)
            {
                _resultCardPwd[i] = _giftCardPwd[_random.Next(_giftCardPwd.Length - 1)];
            }
            return new string(_resultCardPwd);
        }
        /// <summary>
        /// 优惠券卡号
        /// </summary>
        /// <returns></returns>
        public static string CouponsNo(int length, Random _random)
        {
            //long i = 1;
            //foreach (byte b in Guid.NewGuid().ToByteArray())
            //{
            //    i *= ((int)b + 1);
            //}
            //return string.Format("{0:x}", i - DateTime.Now.Ticks).ToUpper();
            return CommonCardRule(length, _random);
        }
        /// <summary>
        /// 积分抵扣现金比例 默认50即50积分抵扣1元
        /// </summary>
        public static decimal IntegralDeductionCashRate
        {
            get { return string.IsNullOrEmpty(ConfigurationManager.AppSettings["integral_deduction_cash_rate"]) ? 50 : decimal.Parse(ConfigurationManager.AppSettings["integral_deduction_cash_rate"]); }
        }
        /// <summary>
        /// 通用的券号规则（订单号，厨房批次号，代金卡、优惠券批次号）
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string CommonNoRules(string type)
        {
            EFDbContext _context = new EFDbContext();
            //查询数据库最大单据号
            string time_begin = DateTime.Now.ToString("yyyy-MM-dd") + " 00:00:00";
            string time2_end = DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59";
            DateTime beginDate = new DateTime();
            DateTime.TryParse(time_begin, out beginDate);
            DateTime endDate = new DateTime();
            DateTime.TryParse(time2_end, out endDate);
            string currentNo = "";//当前单据号
            string startStr = "";//开头是什么字符
            switch (type)
            {
                case "orderno"://订单号
                    var maxOrder = _context.Orders.Where(p => p.IsDeleted != 1 && p.CreatedOn > beginDate && p.CreatedOn < endDate).OrderByDescending(p => p.CreatedOn).FirstOrDefault();
                    if (maxOrder == null)
                    {
                        currentNo = "0000";
                    }
                    else {
                        currentNo = maxOrder.No.ToString().Remove(0, 8);
                    }
                    startStr = "FC";
                    break;
                case "orderbatch"://生成批次号
                    var maxOrderBatch = _context.OrderBatchs.Where(p => p.CreatedOn > beginDate && p.CreatedOn < endDate && p.IsDeleted != 1).OrderByDescending(p => p.CreatedOn).FirstOrDefault();
                    if (maxOrderBatch == null)
                    {
                        currentNo = "0000";
                    }
                    else
                    {
                        currentNo = maxOrderBatch.BatchNo.ToString().Remove(0, 8);
                    }
                    startStr = "SC";
                    break;
                case "giftbatch"://代金卡批次号
                    var maxGiftBatch = _context.GiftCards.Where(p => p.CreatedOn > beginDate && p.CreatedOn < endDate && p.IsDeleted != 1).OrderByDescending(p => p.CreatedOn).FirstOrDefault();
                    if (maxGiftBatch == null) {
                        currentNo = "0000";
                    }
                    else
                    {
                        currentNo = maxGiftBatch.GiftBatch.ToString().Remove(0, 8);
                    }
                    startStr = "DJ";
                    break;
                case "couponbatch"://优惠券批次号
                    var maxCouponBatch = _context.Coupons.Where(p => p.CreatedOn > beginDate && p.CreatedOn < endDate && p.IsDeleted != 1).OrderByDescending(p => p.CreatedOn).FirstOrDefault();
                    if (maxCouponBatch == null)
                    {
                        currentNo = "0000";
                    }
                    else {
                        currentNo = maxCouponBatch.CouponBatch.ToString().Remove(0, 8);
                    }
                    startStr = "YH";
                    break;
                default:
                    return "1000";
            }
            //拼接最新单据号
            int maxNo;
            int.TryParse(currentNo, out maxNo);
        roclback:
            maxNo = maxNo + 1;
            System.Text.StringBuilder newNo = new System.Text.StringBuilder();
            string id = string.Format("{0:D5}", maxNo);
            newNo.Append(startStr).Append(DateTime.Now.ToString("yyMMdd")).Append(id);

            string no = newNo.ToString();

            int count = 0;
            switch (type)
            {
                case "orderno"://订单号
                    count = _context.Orders.Where(p => p.No.Equals(no) && p.IsDeleted != 1).Count();
                    break;
                case "orderbatch"://生成批次号
                    count = _context.OrderBatchs.Where(p => p.BatchNo.Equals(no) && p.IsDeleted != 1).Count();
                    break;
                case "giftbatch"://代金卡批次号
                    count = _context.GiftCards.Where(p => p.GiftBatch.Equals(no) && p.IsDeleted != 1).Count();
                    break;
                case "couponbatch"://优惠券批次号
                    count = _context.Coupons.Where(p => p.CouponBatch.Equals(no) && p.IsDeleted != 1).Count();
                    break;
            }
            if (count > 0)
            {
                goto roclback;
            }

            return no;
        }


        #region  注册用户时发放优惠券
        public static string RegisterCouponsTitle
        {
            get { return string.IsNullOrEmpty(ConfigurationManager.AppSettings["register_coupons_title"]) ? "新会员注册优惠" : ConfigurationManager.AppSettings["register_coupons_title"]; }
        }
        public static int RegisterCouponsQuantity
        {
            get { return string.IsNullOrEmpty(ConfigurationManager.AppSettings["register_coupons_quantity"]) ? 0 : int.Parse(ConfigurationManager.AppSettings["register_coupons_quantity"]); }
        }
        public static decimal RegisterCouponsDenomination
        {
            get { return string.IsNullOrEmpty(ConfigurationManager.AppSettings["register_coupons_denomination"]) ? 20 : decimal.Parse(ConfigurationManager.AppSettings["register_coupons_denomination"]); }
        }
        public static decimal RegisterCouponsConditionMoney
        {
            get { return string.IsNullOrEmpty(ConfigurationManager.AppSettings["register_coupons_conditionmoney"]) ? 0 : decimal.Parse(ConfigurationManager.AppSettings["register_coupons_conditionmoney"]); }
        }
        public static short RegisterCouponsIsSendSMS
        {
            get { return string.IsNullOrEmpty(ConfigurationManager.AppSettings["register_coupons_issendsms"]) ? short.Parse("0") : short.Parse(ConfigurationManager.AppSettings["register_coupons_issendsms"]); }
        }
        #endregion
        /// <summary>
        /// 公司内部账号，不累计积分成长值
        /// </summary>
        public static string InternalAccount
        {
            get { return string.IsNullOrEmpty(ConfigurationManager.AppSettings["internal_account"]) ? "" : ConfigurationManager.AppSettings["internal_account"]; }
        }
    }
}
