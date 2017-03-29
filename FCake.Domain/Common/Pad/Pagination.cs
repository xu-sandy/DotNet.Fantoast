using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FCake.Domain.Common.Pad
{
    public class Pagination : BasePageRequest
    {
        public Pagination(int? pageIndex, int? pageSize)
        {
            if (pageIndex.HasValue)
                Page = pageIndex.Value;
            if (pageSize.HasValue)
                PageSize = pageSize.Value;
        }
        /// <summary>
        /// 总共行数
        /// </summary>
        private int _totalCount = 0;
        /// <summary>
        /// 总共行数
        /// </summary>
        public int TotalCount
        {
            get { return _totalCount; }
        }
        /// <summary>
        /// 总共页数
        /// </summary>
        private int _pageCount = 0;
        /// <summary>
        /// 总共页数
        /// </summary>
        public int PageCount
        {
            get { return _pageCount; }
        }

        #region Methods
        public void SetPagination(int totalCount)
        {
            this._totalCount = totalCount;
            _pageCount = _totalCount / PageSize + (_totalCount % PageSize == 0 ? 0 : 1);
        }
        #endregion
    }
}