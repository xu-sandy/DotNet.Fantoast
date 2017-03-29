using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;

namespace FCake.Core.MvcCommon
{
    public static class CultureHelper
    {
        public static readonly string DefaultCultureName = "zh-CN";
        public static readonly string DefaultTwoLetterISOLanguageName = "zh";

        public static CultureInfo GuessCultureInfo(HttpContext ctx)
        {
            var languageCookie = ctx.Request.Cookies["lang"];
            var userLanguages = ctx.Request.UserLanguages;

            var cultureInfo = new CultureInfo(languageCookie != null
                                             ? languageCookie.Value
                                             : userLanguages != null
                                                 ? userLanguages[0]
                                                 : "en");

            return cultureInfo;
        }

        public static void AsignCultureInfoToCurrentThread(CultureInfo cultureInfo)
        {
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(cultureInfo.Name);
        }

        public static IHtmlString MetaAcceptLanguage(this HtmlHelper htmlHelper)
        {
            var acceptLanguage = HttpUtility.HtmlAttributeEncode(CultureInfo.CurrentUICulture.ToString());
            return new HtmlString(string.Format("<meta name=\"accept-language\" content=\"{0}\" />", acceptLanguage));
        }

        public static string JsCultureBundle(this HtmlHelper htmlHelper, string bundlePattern)
        {
            return string.Format(bundlePattern, (CultureInfo.CurrentUICulture.Name));
        }

        public static IHtmlString MetaAcceptLanguage<T>(this HtmlHelper<T> htmlHelper)
        {
            var acceptLanguage = HttpUtility.HtmlAttributeEncode(CultureInfo.CurrentUICulture.ToString());
            return new HtmlString(string.Format("<meta name=\"accept-language\" content=\"{0}\" />", acceptLanguage));
        }

        public static string CultureBundle<T>(this HtmlHelper<T> htmlHelper, string bundlePattern)
        {
            return string.Format(bundlePattern, (CultureInfo.CurrentUICulture.Name));
        }

        public static void LocalizationScript(this BundleCollection bundles, HttpContext ctx, string bundlePattern, params string[] searchPatterns)
        {
            //Create culture specific bundles which contain the JavaScript files that should be served for each culture
            foreach (var culture in CultureInfo.GetCultures(CultureTypes.AllCultures))
            {
                string bundleVirtualPath = string.Format(bundlePattern, culture.Name);
                Bundle bundle = bundles.GetBundleFor(bundleVirtualPath);
                if (bundle == null)
                {
                    bundle = new ScriptBundle(bundleVirtualPath);
                    bundles.Add(bundle);
                }
                foreach (string pattern in searchPatterns)
                {
                    string filePath = DetermineCultureFile(ctx, culture, pattern);
                    if (!string.IsNullOrEmpty(filePath))
                        bundle.Include(filePath);
                }
            }
        }

        public static void LocalizationStyle(this BundleCollection bundles, HttpContext ctx, string bundlePattern, params string[] searchPatterns)
        {
            //Create culture specific bundles which contain the JavaScript files that should be served for each culture
            foreach (var culture in CultureInfo.GetCultures(CultureTypes.AllCultures))
            {
                string bundleVirtualPath = string.Format(bundlePattern, culture.Name);
                Bundle bundle = bundles.GetBundleFor(bundleVirtualPath);
                if (bundle == null)
                {
                    bundle = new ScriptBundle(bundleVirtualPath);
                    bundles.Add(bundle);
                }
                foreach (string pattern in searchPatterns)
                {
                    string filePath = DetermineCultureFile(ctx, culture, pattern);
                    if (!string.IsNullOrEmpty(filePath))
                        bundle.Include(filePath);
                }
            }
        }

        private static string DetermineCultureFile(HttpContext ctx, CultureInfo culture, string filePattern)
        {
            string[] pendingChoices = { culture.Name, culture.TwoLetterISOLanguageName, DefaultCultureName, DefaultTwoLetterISOLanguageName };
            string regionalisedFileToUse = string.Empty;
            foreach (var choice in pendingChoices)
            {
                string checkFilePath = string.Format(filePattern, choice);
                if (File.Exists(ctx.Server.MapPath(checkFilePath)))
                {
                    regionalisedFileToUse = checkFilePath;
                    break;
                }
            }
            return regionalisedFileToUse;
        }
    }
}
