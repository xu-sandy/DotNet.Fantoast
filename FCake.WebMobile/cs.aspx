<% @Import Namespace="System.Web" %>

<script language="C#" runat="server">
    private class CS
    {
        private int SiteId = 0;
        private const string ImageDomain = "c.cnzz.com";
        public CS(int SiteId)
        {
            this.SiteId = SiteId;
        }
        public string TrackPageView()
        {
            HttpRequest request = HttpContext.Current.Request;
            string scheme = request != null ? request.IsSecureConnection ? "https" : "http" : "http";
            string referer = request != null && request.UrlReferrer != null && "" != request.UrlReferrer.ToString() ? request.UrlReferrer.ToString() : "";
            String rnd = new Random().Next(0x7fffffff).ToString();
            return scheme + "://" + CS.ImageDomain + "/wapstat.php" + "?siteid=" + this.SiteId + "&r=" + HttpUtility.UrlEncode(referer) + "&rnd=" + rnd;
        }
    }
</script>
