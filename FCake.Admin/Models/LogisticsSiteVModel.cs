using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FCake.Domain.Entities;

namespace FCake.Admin.Models
{
    public class LogisticsSiteVModel
    {
        public LogisticsSite LogisticsSiteVM { get; set; }

        public LogisticsSiteVModel(string id) 
        {
            this.LogisticsSiteVM = new LogisticsSite();
        }
    }
}