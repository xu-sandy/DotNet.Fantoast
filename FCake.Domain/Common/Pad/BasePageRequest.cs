using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FCake.Domain.Common.Pad
{
    public class BasePageRequest
    {
        /// <summary>
        /// 页码
        /// </summary>
        private int _page = 1;
        /// <summary>
        /// 页码
        /// </summary>
        public int Page
        {
            get { return _page; }
            set { _page = value; }
        }

        /// <summary>
        /// 每页数
        /// </summary>
        private int _pageSize = 10;
        /// <summary>
        /// 每页数
        /// </summary>
        public int PageSize
        {
            get { return _pageSize; }
            set { _pageSize = value; }
        }
    }
}
