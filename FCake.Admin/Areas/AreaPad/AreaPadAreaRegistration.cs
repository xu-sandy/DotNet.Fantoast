using System.Web.Mvc;

namespace FCake.Admin.Areas.AreaPad
{
    public class AreaPadAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "AreaPad";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "AreaPad_default",
                "AreaPad/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }, new string[] { "FCake.Admin.Areas.AreaPad.Controllers" }
            );
        }
    }
}
