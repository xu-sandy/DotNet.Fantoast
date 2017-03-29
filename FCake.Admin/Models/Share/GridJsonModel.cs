using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FCake.Admin.Models
{
    public class GridJsonModel
    {
         public GridJsonModel(int m_total, object m_rows)
        {
            total = m_total;
            rows = m_rows;
        }

         public GridJsonModel(int m_total, object m_rows, object m_footer)
        {
            total = m_total;
            rows = m_rows;
            footer = m_footer;
        }

        public int total { get; set; }
        public object rows { get; set; }
        public object footer { get; set; }
    }
}