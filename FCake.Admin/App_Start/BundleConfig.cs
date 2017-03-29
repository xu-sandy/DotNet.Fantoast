using System.Web;
using System.Web.Optimization;
using FCake.Core.MvcCommon;

namespace FCake.Admin
{
    public class BundleConfig
    {
        // 有关 Bundling 的详细信息，请访问 http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            //jquery
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));
            bundles.Add(
                new ScriptBundle("~/bundles/jquery.validate")
                .Include("~/Scripts/jquery.validate.js")
                .Include("~/Scripts/jquery.validate.additional-methods.js")
                .Include("~/Scripts/jquery.validate.custom-methods.js")
                );

            //easyui
            bundles.Add(new ScriptBundle("~/bundles/jqueryeasyui").Include(
                        "~/Scripts/jquery.easyui.min.js"
                ));
            //grid
            bundles.Add(new ScriptBundle("~/bundles/grid").Include(
                        "~/Scripts/datagrid-detailview.js",
                        "~/Scripts/easyui-ex.js",
                        "~/Scripts/common-grid.js"
                ));
            //form
            bundles.Add(new ScriptBundle("~/bundles/form").Include(
                        "~/Scripts/form.js"
                ));

            ////artDialog
            //bundles.Add(new ScriptBundle("~/bundles/artDialog").Include(
            //            "~/Scripts/controls/artDialog/source/artDialog.js",
            //            "~/Scripts/controls/artDialog/source/artDialog.plugins.js",
            //            "~/Scripts/controls/artDialog/source/jquery.artDialog.js"
            //    ));

            //easyui css
            bundles.Add(new StyleBundle("~/Content/easyui/css").Include(
                        "~/Content/easyui/icon.css",
                        "~/Content/easyui/customicon.css",
                        "~/Content/easyui/color.css",
                        "~/Content/easyui/customcolor.css"
                ));
            bundles.Add(new StyleBundle("~/Content/easyui/default/css").Include(
                        "~/Content/easyui/gray/easyui.css",
                        "~/Content/easyui/gray/customeasyui.css"
                ));
            //artDialog css
            //bundles.Add(new StyleBundle("~/Content/artDialog/css").Include(
            //            "~/Scripts/controls/artDialog/skins/twitter.css"
            //    ));

            //bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
            //            "~/Scripts/jquery-ui-{version}.js"));

            //bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
            //            "~/Scripts/jquery.unobtrusive*",
            //            "~/Scripts/jquery.validate*"));

            //// 使用要用于开发和学习的 Modernizr 的开发版本。然后，当你做好
            //// 生产准备时，请使用 http://modernizr.com 上的生成工具来仅选择所需的测试。
            //bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
            //            "~/Scripts/modernizr-*"));

            //bundles.Add(new StyleBundle("~/Content/css").Include("~/Content/site.css"));

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

            //culture
            #region localization script bundles
            bundles.LocalizationScript(HttpContext.Current, "~/bundles/jquery-culture.{0}",
              "~/Scripts/lang/jquery-validation/messages_{0}.js");
      #endregion
        }
    }
}