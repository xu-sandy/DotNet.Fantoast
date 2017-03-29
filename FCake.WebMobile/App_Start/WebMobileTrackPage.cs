using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FCake.WebMobile
{
    public class WebMobileTrackPage
    {
        private int SiteId = 0;
        private const string ImageDomain = "c.cnzz.com";
        public WebMobileTrackPage(int SiteId)
        {
            this.SiteId = SiteId;
        }
        public string TrackPageView()
        {
            HttpRequest request = HttpContext.Current.Request;
            string scheme = request != null ? request.IsSecureConnection ? "https" : "http" : "http";
            string referer = request != null && request.UrlReferrer != null && "" != request.UrlReferrer.ToString() ? request.UrlReferrer.ToString() : "";
            String rnd = new Random().Next(0x7fffffff).ToString();
            return scheme + "://" + WebMobileTrackPage.ImageDomain + "/wapstat.php" + "?siteid=" + this.SiteId + "&r=" + HttpUtility.UrlEncode(referer) + "&rnd=" + rnd;
        }
    }
}