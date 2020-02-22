using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
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
            : this(encoding, null, null, enableProxy: true)
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
            if (webRequest == null)
            {
                throw new Exception("Empty WebRequest");
            }

            if (!_enableProxy)
            {
                webRequest.Proxy = new WebProxy();
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

            webRequest.CookieContainer = CookieContainer;
            if (Settings.Current.DisableServerCertVerify.GetValueOrDefault(false))
                webRequest.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;

            logRequest(webRequest);

            return webRequest;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            WebResponse response;
            try
            {
                response = base.GetWebResponse(request);
            }
            catch (WebException e)
            {
                if (e.Response != null) logResponse((HttpWebResponse) e.Response);

                throw;
            }
            catch (Exception e)
            {
                if (!(e.InnerException is WebException))
                {
                    throw;
                }

                logResponse((HttpWebResponse) request.GetResponse());

                throw;
            }

            if (response != null)
            {
                logResponse((HttpWebResponse) response);
            }
            
            return response;
        }


        private static void logRequest(WebRequest webRequest)
        {
            var request = webRequest as HttpWebRequest;

            var body = "";
            try
            {
                if (request != null) body = new StreamReader(request.GetRequestStream()).ReadToEnd();
            }
            catch (Exception e)
            {
                body = e.Message;
            }

            if (request != null)
                _logger.Trace(
                    $"\r\n\r\nSENDING HTTP REQUEST {request.RequestUri}\r\n{request.Method} {request.RequestUri.PathAndQuery} HTTP/{request.ProtocolVersion}\r\n" +
                    request.Headers + "\r\n\r\n" + body
                );
        }

        private static void logResponse(HttpWebResponse response)
        {
            var responseStream = response.GetResponseStream();

            if (responseStream != null)
            {
                Stream streamReplace = new MemoryStream();
                responseStream.CopyTo(streamReplace);
                streamReplace.Seek(0, SeekOrigin.Begin);

                var length = streamReplace.Length;
                var buffer = new byte[length];
                var read = 0;
                do
                {
                    var readed = streamReplace.Read(buffer, 0, (int) length);
                    read += readed;
                } while (read < length);

                _logger.Trace($"Charset {response.CharacterSet}");

                var text = response.CharacterSet != null && response.CharacterSet.ToLower().Contains("1251")
                    ? Encoding.GetEncoding(1251).GetString(buffer)
                    : Encoding.GetEncoding("UTF-8").GetString(buffer);

                streamReplace.Seek(0, SeekOrigin.Begin);

                var fieldInfo = response
                    .GetType()
                    .GetField(
                        "m_ConnectStream",
                        BindingFlags.Instance | BindingFlags.NonPublic
                    );
                if (fieldInfo != null) fieldInfo.SetValue(response, streamReplace);
                var httpWebResponse = response;
                response.Headers["Set-Cookie"] = "--HIDDEN FOR SECURITY REASONS--";
                _logger.Trace(
                    $"\r\n\r\nRECEIVED HTTP RESPONSE\r\nHTTP/{httpWebResponse.ProtocolVersion} {(int) httpWebResponse.StatusCode} {httpWebResponse.StatusDescription}\r\n" +
                    response.Headers +
                    "\r\n\r\n" +
                    text);
            }
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