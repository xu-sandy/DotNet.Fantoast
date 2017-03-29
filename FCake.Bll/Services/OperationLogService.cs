using FCake.Domain;
using FCake.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FCake.Bll.Services
{
    public class OperationLogService
    {
        static EFDbContext context = new EFDbContext();
        public static OperationLog SaveOperLog(string operatorId, string category, string businessId, string content)
        {
            var operLog = SaveOperLog(operatorId, operatorId, DateTime.Now, DateTime.Now, category, businessId, content);
            return operLog;
        }
        public static OperationLog SaveOperLog(string createdBy, string operatorId, DateTime createdOn, DateTime operTime, string category, string businessId, string content)
        {
            var operLog = new OperationLog(createdBy, operatorId, createdOn, operTime, category, businessId, content);
            context.OperationLogs.Add(operLog);
            context.SaveChanges();
            return operLog;
        }
    }
}
