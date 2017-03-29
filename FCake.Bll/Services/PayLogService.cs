using FCake.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCake.Domain.Entities;

namespace FCake.Bll.Services
{
    public class PayLogService
    {
        private EFDbContext context = new EFDbContext();
        public bool Add(PayLog entity)
        {
            context.PagLog.Add(entity);
            return (context.SaveChanges() > 0);
        }

    }
}
