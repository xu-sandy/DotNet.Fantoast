using FCake.Bll;
using FCake.Core.MvcCommon;
using FCake.Domain;
using FCake.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Text;
using System.Text.RegularExpressions;
using FCake.Domain.Common;
using FCake.Core.Common;

namespace FCake.Bll
{
    public class MemberLevelService : BaseService
    {
        private readonly EFDbContext _context = new EFDbContext();
        /// <summary>
        /// 获取所有的会员等级信息
        /// </summary>
        /// <returns>会员等级信息集合</returns>
        public List<MemberLevel> GetAllMemberLevel()
        {
            return _context.MemberLevel.Where(o => o.IsDeleted != 1).ToList();
        }

        /// <summary>
        /// 会员等级的分页数据
        /// </summary>
        /// <param name="currentUserId"></param>
        /// <param name="totalCount"></param>
        /// <param name="pageInfo"></param>
        /// <returns></returns>
        public List<MemberLevel> GetMemberLevelPaging(out int totalCount, PageInfo pageInfo)
        {
            var query = _context.MemberLevel.Where(o => o.IsDeleted != 1);
            totalCount = query.Count();
            var data = query.OrderBy(o => o.MemberLevelValue).Skip((pageInfo.Page - 1) * pageInfo.Rows).Take(pageInfo.Rows).ToList();
            return data;
        }
        /// <summary>
        /// 新增或修改会员等级
        /// </summary>
        /// <param name="memberLevel"></param>
        /// <returns></returns>
        public OpResult SaveOrUpdate(MemberLevel memberLevel, string currentUserId)
        {
            var result = new OpResult();
            if (!string.IsNullOrEmpty(memberLevel.Id))
            {
                //更新
                var newMemberLevel = _context.MemberLevel.Where(o => o.Id == memberLevel.Id).FirstOrDefault();
                if (newMemberLevel.MemberLevelValue != memberLevel.MemberLevelValue)
                {
                    var sameMemberLevel = _context.MemberLevel.Where(o => o.MemberLevelValue == memberLevel.MemberLevelValue && o.IsDeleted != 1).FirstOrDefault();
                    if (sameMemberLevel != null)
                    {
                        result = OpResult.Fail("对不起，该会员等级值已存在");
                        return result;
                    }
                }
                newMemberLevel.ModifiedBy = currentUserId;
                newMemberLevel.ModifiedOn = DateTime.Now;
                newMemberLevel.Title = memberLevel.Title;
                newMemberLevel.MemberLevelValue = memberLevel.MemberLevelValue;
                newMemberLevel.MinGrowthValue = memberLevel.MinGrowthValue;
                newMemberLevel.MaxGrowthValue = memberLevel.MaxGrowthValue;
                newMemberLevel.DiscountRate = memberLevel.DiscountRate / 100;
                newMemberLevel.IntegralMultiples = memberLevel.IntegralMultiples;
                newMemberLevel.GrowthValueMultiples = memberLevel.GrowthValueMultiples;
                newMemberLevel.Remark = memberLevel.Remark;
                newMemberLevel.YearDeductGrowthValue = memberLevel.YearDeductGrowthValue;
            }
            else
            {
                //新增
                var sameMemberLevel = _context.MemberLevel.Where(o => o.MemberLevelValue == memberLevel.MemberLevelValue && o.IsDeleted != 1).FirstOrDefault();
                if (sameMemberLevel != null)
                {
                    result = OpResult.Fail("对不起，该会员等级值已存在");
                    return result;
                }
                memberLevel.Id = CommonRules.GUID;
                memberLevel.CreatedOn = DateTime.Now;
                memberLevel.CreatedBy = currentUserId;
                memberLevel.DiscountRate = memberLevel.DiscountRate / 100;
                _context.MemberLevel.Add(memberLevel);
            }
            if (_context.SaveChanges() > 0)
            {
                result = OpResult.Success("操作成功");
            }
            return result;
        }

        public MemberLevel GetMemberLevelByLevelVal(int levelVal)
        {
            var result = _context.MemberLevel.Where(o => o.MemberLevelValue == levelVal && o.IsDeleted != 1).FirstOrDefault();
            if (result == null)
                result = new MemberLevel();
            return result;
        }
    }
}
