using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCake.Domain.Entities;
using FCake.Domain;
using FCake.Core.MvcCommon;

namespace FCake.Bll.Services
{
    public class MsgTemplateService
    {
        EFDbContext context = new EFDbContext();
        public OpResult CreateMsgTemplate(MsgTemplate msgTemp)
        {
            OpResult result = new OpResult();
            result.Successed = false;
            try
            {
                context.MsgTemplates.Add(msgTemp);

            }
            catch
            {
                result.Message = "添加失败";
            }

            return result;
        }

        public IQueryable<MsgTemplate> GetAllMsgTemp()
        {
            var result = context.MsgTemplates.Where(p => p.IsDeleted != 1);
            return result;
        }
        /// <summary>
        /// 更新短信模版
        /// </summary>
        /// <param name="id"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public bool UpdateMsgTemplate(string id, string content)
        {
            var data = context.MsgTemplates.Where(p => p.Id.Equals(id) && p.IsDeleted != 1).SingleOrDefault();
            if (data != null)
            {
                data.content = content;
            }
            if (context.SaveChanges() > 0)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 根据类别取短信内容
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public string GetMsgTempByCategory(string Category)
        {
            var result = context.MsgTemplates.Where(p => p.Category.Equals(Category)).SingleOrDefault();
            if (result != null)
            {
                return result.content;
            }
            return null;
        }
    }
}
