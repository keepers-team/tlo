using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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

        public bool multipart = false;

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

            if (multipart)
            {
                // webRequest.ContentType = "multipart/form-data";   
            }
            else if(webRequest.Method == "POST" || webRequest.Method == "post")
            {
                webRequest.ContentType = "application/x-www-form-urlencoded";
            }
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

                    // var text = "";
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
        
    public class MimePart
    {
        NameValueCollection _headers = new NameValueCollection();
        byte[] _header;

        public NameValueCollection Headers
        {
            get { return _headers; }
        }

        public byte[] Header
        {
            get { return _header; }
        }

        public long GenerateHeaderFooterData(string boundary)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("--");
            stringBuilder.Append(boundary);
            stringBuilder.AppendLine();
            foreach (string key in _headers.AllKeys)
            {
                stringBuilder.Append(key);
                stringBuilder.Append(": ");
                stringBuilder.AppendLine(_headers[key]);
            }
            stringBuilder.AppendLine();

            _header = Encoding.UTF8.GetBytes(stringBuilder.ToString());

            return _header.Length + Data.Length + 2;
        }

        public Stream Data { get; set; }
    }

    public class UploadResponse
    {
        public UploadResponse(HttpStatusCode httpStatusCode, string responseBody)
        {
            HttpStatusCode = httpStatusCode;
            ResponseBody = responseBody;
        }

        public HttpStatusCode HttpStatusCode { get; set; }

        public string ResponseBody { get; set; }
    }

    public UploadResponse Upload(string url, NameValueCollection requestHeaders, NameValueCollection requestParameters)
    {
        using (WebClient client = this)
        {

            List<MimePart> mimeParts = new List<MimePart>();

            try
            {
                foreach (string key in requestHeaders.AllKeys)
                {
                    client.Headers.Add(key, requestHeaders[key]);
                }

                foreach (string key in requestParameters.AllKeys)
                {
                    MimePart part = new MimePart();

                    part.Headers["Content-Disposition"] = "form-data; name=\"" + key + "\"";
                    part.Data = new MemoryStream(Encoding.GetBytes(requestParameters[key]));

                    mimeParts.Add(part);
                }

                // foreach (FileInfo file in files)
                // {
                //     MimePart part = new MimePart();
                //     string name = file.Extension.Substring(1);
                //     string fileName = file.Name;
                //
                //     part.Headers["Content-Disposition"] = "form-data; name=\"" + name + "\"; filename=\"" + fileName + "\"";
                //     part.Headers["Content-Type"] = "application/octet-stream";
                //
                //     part.Data = new MemoryStream(File.ReadAllBytes(file.FullName));
                //
                //     mimeParts.Add(part);
                // }

                string boundary = "----------" + DateTime.Now.Ticks.ToString("x");
                client.Headers.Add(HttpRequestHeader.ContentType, "multipart/form-data; boundary=" + boundary);

                long contentLength = 0;

                byte[] _footer = Encoding.UTF8.GetBytes("--" + boundary + "--\r\n");

                foreach (MimePart mimePart in mimeParts)
                {
                    contentLength += mimePart.GenerateHeaderFooterData(boundary);
                }

                byte[] buffer = new byte[8192];
                byte[] afterFile = Encoding.UTF8.GetBytes("\r\n");
                int read;

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    foreach (MimePart mimePart in mimeParts)
                    {
                        memoryStream.Write(mimePart.Header, 0, mimePart.Header.Length);

                        while ((read = mimePart.Data.Read(buffer, 0, buffer.Length)) > 0)
                            memoryStream.Write(buffer, 0, read);

                        mimePart.Data.Dispose();

                        memoryStream.Write(afterFile, 0, afterFile.Length);
                    }

                    memoryStream.Write(_footer, 0, _footer.Length);
                    var array = memoryStream.ToArray();
                    System.Console.WriteLine(Encoding.GetString(array));
                    System.Console.Out.Flush();
                    byte[] responseBytes = client.UploadData(url, array);
                    string responseString = Encoding.UTF8.GetString(responseBytes);
                    return new UploadResponse(HttpStatusCode.OK, responseString);
                }
            }
            catch (Exception ex)
            {
                foreach (MimePart part in mimeParts)
                    if (part.Data != null)
                        part.Data.Dispose();

                if (ex.GetType().Name == "WebException")
                {
                    WebException webException = (WebException)ex;
                    HttpWebResponse response = (HttpWebResponse)webException.Response;
                    string responseString;

                    using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        responseString = reader.ReadToEnd();
                    }

                    return new UploadResponse(response.StatusCode, responseString);
                }
                else
                {
                    throw;
                }
            }
        }
    }
    }
}