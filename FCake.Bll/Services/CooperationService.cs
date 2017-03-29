using FCake.Core.MvcCommon;
using FCake.Domain;
using FCake.Domain.Common;
using FCake.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace FCake.Bll.Services
{
    public class CooperationService : BaseService
    {
        EFDbContext context = new EFDbContext();

        #region 后台
        /// <summary>
        /// 获取所有合作公司信息
        /// </summary>
        /// <param name="totalCount"></param>
        /// <param name="pageInfo"></param>
        /// <returns></returns>
        public dynamic GetCooperationsAll(out int totalCount, PageInfo pageInfo)
        {
            //var query = context.Cooperations.Where(c => c.IsDeleted != 1);
            var query = from c in context.Cooperations
                        join u in context.Customers
                        on c.CustomerId equals u.Id
                        where c.IsDeleted != 1
                        select new
                        {
                            CompanyName = c.CompanyName,
                            CompanyAddress = c.CompanyAddress,
                            CompanyPopulation = c.CompanyPopulation,
                            Name = c.Name,
                            Phone = c.Phone,
                            Email = c.Email,
                            CreatedOn = c.CreatedOn,
                            Id = c.Id,
                            CustomerMobile = u.Mobile,
                            ApplyForTime = c.ApplyForTime,
                            Status = c.Status,
                            Description = c.Description
                        };

            totalCount = query.Count();
            var a = query.ToList();
            var data = query.OrderByDescending(c => c.CreatedOn).Skip((pageInfo.Page - 1) * pageInfo.Rows).Take(pageInfo.Rows).ToList();
            return data;
        }
        /// <summary>
        /// 审核企业试吃申请
        /// </summary>
        /// <param name="id"></param>
        /// <param name="description"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public OpResult UpdateCooperation(string id, string description, int result)
        {
            OpResult res = new OpResult();
            res.Successed = false;
            try
            {
                var cooperation = context.Cooperations.SingleOrDefault(p => p.Id.Equals(id) && p.IsDeleted != 1);
                if (cooperation != null)
                {
                    cooperation.Status = result;
                    cooperation.Description = description;
                    //cooperation.Status = 1;//1=已审核
                    if (context.SaveChanges() > 0)
                    {
                        res.Successed = true;
                        res.Message = "保存成功";
                    }
                }
            }
            catch (Exception e)
            {
                res.Message = "保存失败：" + e.Message;
            }
            return res;
        }
        /// <summary>
        /// 查找用户的企业试吃申请记录
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<Cooperation> GetCooperationByUserId(string userId)
        {
            var result = context.Cooperations.Where(p => p.CustomerId.Equals(userId) && p.IsDeleted != 1 && p.Status != 0).ToList();
            return result;
        }
        /// <summary>
        /// 根据lambda表达式查找对应对象
        /// </summary>
        /// <param name="whereLambda"></param>
        /// <returns></returns>
        public Cooperation GetCooperationByWhere(Expression<Func<Cooperation,bool>> whereLambda)
        {
            return context.Cooperations.SingleOrDefault(whereLambda);
        }

        #endregion

        #region 前台
        /// <summary>
        /// 提交合作公司信息
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public OpResult SubmitCooperation(Cooperation entity, string currentUserId)
        {
            OpResult result = null;
            var newEntity = entity;
            if (entity.CompanyName == null || entity.CompanyName.Trim() == "")
                return result = new OpResult() { Successed = false, Message = "请填写您的公司名称！" };
            if (entity.CompanyAddress == null || entity.CompanyAddress.Trim() == "")
                return result = new OpResult() { Successed = false, Message = "请填写您的公司地址！" };
            if (entity.Phone == null || entity.Phone.Trim() == "")
                return result = new OpResult() { Successed = false, Message = "请填写您的联系电话！" };
            if (entity.Email.Trim() == null)
            {
                return result = new OpResult() { Successed = false, Message = "邮箱信息不能为空！" };
            }
            if (entity.Phone.Trim() == null)
            {
                return result = new OpResult() { Successed = false, Message = "电话号码不能为空！" };
            }
            string par = @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
            Regex regex = new Regex(par);
            Match match = regex.Match(entity.Email);
            if (!match.Success)
            {
                return result = new OpResult() { Successed = false, Message = "请输入正确的邮箱！" };
            }
            if (new Regex(@"^1[3|5|7|8|][0-9]{9}$").IsMatch(entity.Phone) == false)
            {
                return result = new OpResult() { Successed = false, Message = "请输入正确的手机号！" };
            }
            var data = context.Cooperations.Where(c => c.IsDeleted != 1 && c.CompanyName.Equals(entity.CompanyName) && c.CustomerId.Equals(currentUserId)).FirstOrDefault();
            if (data == null)
            {//add
                var temp = context.Cooperations.Where(p => p.IsDeleted != 1 && p.CustomerId.Equals(currentUserId) && (p.CompanyAddress.Equals(entity.CompanyAddress) || p.CompanyName.Equals(entity.CompanyName))).ToList();
                if (temp.Count == 0)
                {
                    newEntity.Id = Guid.NewGuid().ToString("N");
                    newEntity.CreatedOn = DateTime.Now;
                    newEntity.ApplyForTime = DateTime.Now;
                    newEntity.CustomerId = currentUserId;
                    newEntity.Status = 0;
                    context.Cooperations.Add(newEntity);
                    context.SaveChanges();
                    result = new OpResult() { Successed = true, Message = "请等待我们与您联系！" };
                }
                else
                    result = new OpResult() { Successed = true, Message = "该企业已经试吃过了！" };

            }
            else
            {//update
                if (data.Status == 0)
                {
                    data.ModifiedBy = currentUserId;
                    data.ModifiedOn = DateTime.Now;
                    data.Status = 0;
                    data.CompanyName = entity.CompanyName;
                    data.CompanyAddress = entity.CompanyAddress;
                    data.CompanyPopulation = entity.CompanyPopulation;
                    data.Name = entity.Name;
                    data.Phone = entity.Phone;
                    data.Email = entity.Email;
                    context.SaveChanges();
                    result = new OpResult() { Successed = true, Message = "试吃申请修改成功！" };
                }
                else
                    result = new OpResult() { Successed = true, Message = "该试吃记录已审核过！" };

            }
            return result;
        }

        /// <summary>
        /// 获取对应客户是否有存在待审核的企业试吃数据
        /// </summary>
        /// <returns></returns>
        public Cooperation FindByCustomerId(string customerId)
        {
            var query = base.DAL.Table<Cooperation>().Where(c => c.Status == 0 && c.CustomerId == customerId).OrderByDescending(c => c.CreatedOn);
            var data = query.FirstOrDefault();
            if (data == null)
            {
                data = new Cooperation()
                {
                    CustomerId = customerId,
                    Status = 0
                };
            }
            return data;
        }
        #endregion


    }
}
