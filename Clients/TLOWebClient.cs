using System;
using System.Net;
using System.Text;
using MihaZupan;
using NLog;

namespace TLO.Clients
{
    internal class TloWebClient : WebClient
    {
        private static Logger _logger;
        private readonly string _accept = string.Empty;
        private readonly string _userAgent;
        private bool _isJson;
        private readonly bool _enableProxy;

        public TloWebClient(bool enableProxy = false)
            : this(null, null, null, enableProxy: enableProxy)
        {
        }

        public TloWebClient(Encoding encoding)
            : this(encoding, null, null)
        {
        }

        public TloWebClient(Encoding encoding, string userAgent, string accept, bool isJson = false,
            bool enableProxy = false)
        {
            if (_logger == null)
                _logger = LogManager.GetCurrentClassLogger();
            Encoding encoding1;
            if (encoding != null)
                encoding1 = encoding;
            else
                encoding = encoding1 = Encoding.UTF8;
            Encoding = encoding1;
            _userAgent = string.IsNullOrWhiteSpace(userAgent)
                ? "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:35.0) Gecko/20100101 Firefox/35.0"
                : userAgent;
            _accept = string.IsNullOrWhiteSpace(accept)
                ? "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8"
                : accept;
            CookieContainer = new CookieContainer();
            _isJson = isJson;
            _enableProxy = enableProxy;
        }

        public TloWebClient(string userAgent)
        {
            _userAgent = userAgent;
        }

        public CookieContainer CookieContainer { get; }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var webRequest = (HttpWebRequest) base.GetWebRequest(address);
            if (webRequest != null && Settings.Current.UseProxy == true && _enableProxy)
            {
                var proxy = Settings.Current.SelectedProxy;
                if (proxy.Contains("http://"))
                {
                    webRequest.Proxy = new WebProxy(proxy);
                }
                else
                {
                    var uri = new Uri(proxy);
                    webRequest.Proxy = new HttpToSocks5Proxy(uri.Host, uri.Port);
                }
            }

            webRequest.Accept = _isJson ? "application/json" : _accept;
            webRequest.UserAgent = _userAgent;
            webRequest.Headers.Add("Accept-Encoding", "gzip, deflate");
            webRequest.Headers.Add("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.5,en;q=0.3");
            if (_isJson)
            {
                webRequest.Headers.Add("X-Request", "JSON");
                webRequest.Headers.Add("X-Requested-With", "XMLHttpRequest");
            }

            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.KeepAlive = true;
            webRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            webRequest.Headers.Add("Pragma", "no-cache");
            webRequest.Timeout = 60000;
            if (address.Host == "dl.rutracker.org" && address.AbsoluteUri.Contains("="))
            {
                var strArray = address.AbsoluteUri.Split(new char[1]
                {
                    '='
                }, StringSplitOptions.RemoveEmptyEntries);
                CookieContainer.Add(address, new Cookie("bb_dl", strArray[1]));
                webRequest.Referer = string.Format("https://{1}/forum/viewtopic.php?t={0}", strArray[1],
                    Settings.Current.HostRuTrackerOrg);
            }

            webRequest.CookieContainer = CookieContainer;
            if (Settings.Current.DisableServerCertVerify.GetValueOrDefault(false))
                webRequest.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;

            return webRequest;
        }

        public string GetString(string url)
        {
            _isJson = false;
            return DownloadString(url);
        }

        public string GetJson(string url)
        {
            _isJson = true;
            return DownloadString(url);
        }
    }
}