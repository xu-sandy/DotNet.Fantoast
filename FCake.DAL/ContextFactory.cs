using FCake.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FCake.DAL.Repositories
{
    public class ContextFactory
    {
        public static EFDbContext GetCurrentContext()
        {
            return new EFDbContext();
        }
    }
}
