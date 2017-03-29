using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FCake.Core.Common
{
    [Serializable]
    public class PageList<T>:List<T>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source">source</param>
        /// <param name="pageIndex">page index</param>
        /// <param name="pageSize">page size</param>
        public PageList(IQueryable<T> source, int pageIndex, int pageSize)
        {
            this.PageSetting(pageIndex, pageSize, source.Count());
            this.AddRange(source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList());
        }
        /// <summary>
        /// 不分页
        /// </summary>
        /// <param name="source"></param>
        public PageList(IQueryable<T> source)
        {
            var result = source.ToList();
            this.PageSetting(1, result.Count, source.Count());
            this.AddRange(result);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source">source</param>
        /// <param name="pageIndex">page index</param>
        /// <param name="pageSize">page size</param>
        public PageList(IList<T> source, int pageIndex, int pageSize)
        {
            this.PageSetting(pageIndex, pageSize,source.Count());
            this.AddRange(source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList());
        }
        private void PageSetting(int pageIndex,int pageSize,int totalCount)
        {
            this.TotalCount = totalCount;
            if (pageSize != 0)
            {
                this.TotalPages = this.TotalCount / pageSize;
                if (this.TotalCount % pageSize > 0)
                    this.TotalPages++;
            }
            else
            {
                this.TotalPages = 0;
            }

            this.PageSize = pageSize;
            this.PageIndex = pageIndex;
        }
        /// <summary>
        /// page index
        /// </summary>
        [DataMember]
        public int PageIndex { get; private set; }
        /// <summary>
        /// page size
        /// </summary>
        [DataMember]
        public int PageSize { get; private set; }
        /// <summary>
        /// page row count
        /// </summary>
        [DataMember]
        public int TotalCount { get; private set; }
        /// <summary>
        /// total page
        /// </summary>
        [DataMember]
        public int TotalPages { get; private set; }

        [DataMember]
        public bool HasPreviousPage
        {
            get { return (PageIndex > 1); }
        }
        [DataMember]
        public bool HasNextPage
        {
            get { return (PageIndex < TotalPages); }
        }
    }
}
