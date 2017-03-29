using FCake.Domain;
using FCake.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FCake.Bll.Services
{
    public class PrePayInfoService
    {
        private EFDbContext context = new EFDbContext();
        /// <summary>
        /// 新增prepayinfo
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public bool AddPayInfo(PrePayInfo info)
        {
            bool result=false;
            context.PrePayInfo.Add(info);
            if (context.SaveChanges() > 0)
            {
                result = true;
            }
            return result;
        }
        /// <summary>
        /// 根据根据预支付no取得PrePayInfo
        /// </summary>
        /// <param name="prePayNo"></param>
        /// <returns></returns>
        public PrePayInfo GetPrePayByPrePayNo(string prePayNo)
        {
            return context.PrePayInfo.Where(p => p.PrePayNo.Equals(prePayNo)).FirstOrDefault();
        }
    }
}
