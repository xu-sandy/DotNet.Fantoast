using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace FCake.Bll.Helper
{
    public static class PageHelper
    {
        public static HtmlString ShowPageNavigate(this HtmlHelper htmlHelper, int currentPage, int pageSize, int totalCount)
        {
            var redirectTo = htmlHelper.ViewContext.RequestContext.HttpContext.Request.Url.AbsolutePath;

            var pamas = htmlHelper.ViewContext.RequestContext.HttpContext.Request.QueryString;
            foreach (var x in pamas.AllKeys.Where(a => new List<string> { "pageindex", "pagesize" }.Contains(a.ToLower()) == false))
            {
                if (redirectTo.IndexOf("?") > -1)
                {
                    redirectTo += ("&" + x + "=" + pamas[x]);
                }
                else
                {
                    redirectTo += ("?" + x + "=" + pamas[x]);
                }
            }



                pageSize = pageSize == 0 ? 3 : pageSize;
            var totalPages = Math.Max((totalCount + pageSize - 1) / pageSize, 1); //总页数
            var output = new StringBuilder();
            if (totalPages > 1)
            {
                output.AppendFormat("<a class='pageLink' href='{0}{2}pageIndex=1&pageSize={1}'>首页</a> ", redirectTo, pageSize,redirectTo.IndexOf("?")>-1?"&":"?");
                if (currentPage > 1)
                {//处理上一页的连接
                    output.AppendFormat("<a class='pageLink' href='{0}{3}pageIndex={1}&pageSize={2}'>上一页</a> ", redirectTo, currentPage - 1, pageSize, redirectTo.IndexOf("?") > -1 ? "&" : "?");
                }

                output.Append(" ");
                int currint = 5;
                for (int i = 0; i <= 10; i++)
                {//一共最多显示10个页码，前面5个，后面5个
                    if ((currentPage + i - currint) >= 1 && (currentPage + i - currint) <= totalPages)
                    {
                        if (currint == i)
                        {//当前页处理                           
                            output.AppendFormat("<a class='cpb' href='{0}{4}pageIndex={1}&pageSize={2}'>{3}</a> ", redirectTo, currentPage, pageSize, currentPage, redirectTo.IndexOf("?") > -1 ? "&" : "?");
                        }
                        else
                        {//一般页处理
                            output.AppendFormat("<a class='pageLink' href='{0}{4}pageIndex={1}&pageSize={2}'>{3}</a> ", redirectTo, currentPage + i - currint, pageSize, currentPage + i - currint, redirectTo.IndexOf("?") > -1 ? "&" : "?");
                        }
                    }
                    output.Append(" ");
                }
                if (currentPage < totalPages)
                {//处理下一页的链接
                    output.AppendFormat("<a class='pageLink' href='{0}{3}pageIndex={1}&pageSize={2}'>下一页</a> ", redirectTo, currentPage + 1, pageSize, redirectTo.IndexOf("?") > -1 ? "&" : "?");
                }

                output.Append(" ");
                if (currentPage != totalPages)
                {
                    output.AppendFormat("<a class='pageLink' href='{0}{3}pageIndex={1}&pageSize={2}'>末页</a> ", redirectTo, totalPages, pageSize, redirectTo.IndexOf("?") > -1 ? "&" : "?");
                }
                output.Append(" ");
            }
            //output.AppendFormat("<label>第{0}页 / 共{1}页</label>", currentPage, totalPages);//这个统计加不加都行

            return new HtmlString(output.ToString());
        }
        public class PagerInfo
        {
            public int RecordCount { get; set; }

            public int CurrentPageIndex { get; set; }

            public int PageSize { get; set; }
        }


        public class PagerQuery<TPager, TEntityList>
        {
            public PagerQuery(TPager pager, TEntityList entityList)
            {
                this.Pager = pager;
                this.EntityList = entityList;
            }
            public TPager Pager { get; set; }
            public TEntityList EntityList { get; set; }
        }
    }
}