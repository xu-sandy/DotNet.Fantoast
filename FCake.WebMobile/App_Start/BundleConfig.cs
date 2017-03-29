using System.Web;
using System.Web.Optimization;
using FCake.Core.MvcCommon;

namespace FCake.WebMobile
{
    public class BundleConfig
    {
        // 有关 Bundling 的详细信息，请访问 http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js"));

            //bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
            //            "~/Scripts/jquery.unobtrusive*"
            //            ));
            bundles.Add(new ScriptBundle("~/bundles/jqueryvalidate").Include(
                        "~/Scripts/jquery.validate.js",
                        //"~/Scripts/jquery.validate.unobtrusive.js",
                        "~/Scripts/jquery.validate.additional-methods.js",
                        "~/Scripts/jquery.validate.custom-methods.js"
                        ));

//            <script src="/Scripts/jquery.unobtrusive-ajax.js"></script>
//<script src="/Scripts/jquery.validate.additional-methods.js"></script>
//<script src="/Scripts/jquery.validate.js"></script>
//<script src="/Scripts/jquery.validate.unobtrusive.js"></script>
//<script src="/Scripts/jquery.custom.validate.methods.js"></script>
//<script src="/Scripts/lang/jquery-validation/messages_zh.js"></script>


            // 使用要用于开发和学习的 Modernizr 的开发版本。然后，当你做好
            // 生产准备时，请使用 http://modernizr.com 上的生成工具来仅选择所需的测试。
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new StyleBundle("~/Content/css").Include("~/Content/site.css"));


            //bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
            //            "~/Content/themes/base/jquery.ui.core.css",
            //            "~/Content/themes/base/jquery.ui.resizable.css",
            //            "~/Content/themes/base/jquery.ui.selectable.css",
            //            "~/Content/themes/base/jquery.ui.accordion.css",
            //            "~/Content/themes/base/jquery.ui.autocomplete.css",
            //            "~/Content/themes/base/jquery.ui.button.css",
            //            "~/Content/themes/base/jquery.ui.dialog.css",
            //            "~/Content/themes/base/jquery.ui.slider.css",
            //            "~/Content/themes/base/jquery.ui.tabs.css",
            //            "~/Content/themes/base/jquery.ui.datepicker.css",
            //            "~/Content/themes/base/jquery.ui.progressbar.css",
            //            "~/Content/themes/base/jquery.ui.theme.css"));
            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
                        "~/Content/themes/base/core.css",
                        "~/Content/themes/base/resizable.css",
                        "~/Content/themes/base/selectable.css",
                        "~/Content/themes/base/accordion.css",
                        "~/Content/themes/base/autocomplete.css",
                        "~/Content/themes/base/button.css",
                        "~/Content/themes/base/dialog.css",
                        "~/Content/themes/base/slider.css",
                        "~/Content/themes/base/tabs.css",
                        "~/Content/themes/base/datepicker.css",
                        "~/Content/themes/base/progressbar.css",
                        "~/Content/themes/base/theme.css"));

            //culture
            #region localization script bundles
            bundles.LocalizationScript(HttpContext.Current, "~/bundles/jquery-culture.{0}",
              "~/Scripts/lang/jquery-validation/messages_{0}.js");
            #endregion
        }
    }
}