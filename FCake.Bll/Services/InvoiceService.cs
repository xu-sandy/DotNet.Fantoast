using FCake.Domain;
using FCake.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FCake.Bll
{
    public class InvoiceService
    {
        private EFDbContext context=new EFDbContext();
        /// <summary>
        /// 保存发票
        /// </summary>
        /// <param name="orderid"></param>
        /// <param name="invoicetype"></param>
        /// <param name="invoicetitle"></param>
        /// <param name="context"></param>
        public void SaveInvoice(string orderid, string invoicetype, string invoicetitle, EFDbContext context,string userid)
        {
            var invoice = context.Invoices.SingleOrDefault(a => a.IsDeleted !=1 && a.OrderId.Equals(orderid));
            if (invoice == null)
            {
                invoice = new Invoice();
                invoice.OrderId = orderid;
                invoice.Id = Guid.NewGuid().ToString("N").ToUpper();
                context.Invoices.Add(invoice);
                invoice.CreatedBy = userid;
                invoice.IsDeleted = 0;
                invoice.CreatedOn = DateTime.Now;
            }
            else
            {
                invoice.ModifiedBy = userid;
                invoice.ModifiedOn = DateTime.Now;
            }
            invoice.InvoiceTitle = invoicetitle;
            invoice.InvoiceType = invoicetype;
        }
        /// <summary>
        /// 删除发票
        /// </summary>
        /// <param name="orderid"></param>
        /// <param name="context"></param>
        public void DeleteInvoice(string orderid, EFDbContext context,string userid)
        {
            var invoice = context.Invoices.SingleOrDefault(a => a.IsDeleted !=1 && a.OrderId.Equals(orderid));
            if (invoice != null)
            {
                invoice.IsDeleted = 1;
                invoice.ModifiedBy = userid;
                invoice.ModifiedOn = DateTime.Now;
            }
        }

        public Invoice GetByOrderId(string id)
        {
            return context.Invoices.SingleOrDefault(a => a.IsDeleted !=1 && a.OrderId.Equals(id));
        }
    }
}
