using System.Web;
using System.Web.Mvc;
using FCake.Admin.Helper;

namespace FCake.Admin
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }

    public class LoginAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (UserCache.CurrentUser == null)
                filterContext.HttpContext.Response.Redirect("/account",true);
        }
    }

}