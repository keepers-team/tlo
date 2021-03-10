using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using AngleSharp.Html.Parser;
using AngleSharp.Text;
using NLog;
using TLO.Info;

namespace TLO.Clients
{
    internal class TixatiClient : ITorrentClient
    {
        public const string ClientId = "Tixati";
        public string Id => ClientId;
        private readonly TloWebClient _client;
        private readonly Uri _baseUri;
        private readonly UriTemplate _uriTemplate;
        private readonly UriTemplate _uriTemplate1;
        private readonly UriTemplate _uriTemplateFree;
        private readonly string _host;
        private readonly int _port;
        private readonly string _username;
        private readonly string _password;
        private readonly Logger Logger = LogManager.GetLogger("Tixati");

        public TixatiClient(string host, int port, string username, string password)
        {
            _baseUri = new Uri($"http://{host}:{port}/");
            _uriTemplate = new UriTemplate("{section}/{methodName}");
            _uriTemplate1 = new UriTemplate("{section}");
            _uriTemplateFree = new UriTemplate("{url}");
            _client = new TloWebClient(Encoding.UTF8, "TLO", "text/*,application/*", true);
            _host = host;
            _port = port;
            _username = username;
            _password = password;
            _client.Headers.Add("Referer", _baseUri.ToString());
            _client.UseDefaultCredentials = true;

            // if (this.Ping())
            // {
                // authorize();   
            // }
        }

        private void authorize()
        {
            // try
            // {
            //     var auth = _uriTemplate.BindByName(_baseUri, new Dictionary<string, string>
            //     {
            //         {"section", "auth"}, {"methodName", "login"}
            //     });
            //
            //     var builder = new UriBuilder(auth);
            //     builder.Query = $"username={_username}&password={_password}";
            //     var result = _client.DownloadString(builder.Uri);
            //     Logger.Log(LogLevel.Debug, "Auth: " + result);
            // }
            // catch (WebException e)
            // {
            //     Logger.Error(e);
            //     Logger.Error(e.StackTrace);
            // }
        }

        public List<TopicInfo> GetAllTorrentHash()
        {
            var uri = _uriTemplate1.BindByName(_baseUri, new Dictionary<string, string>
            {
                {"section", "transfers"}
            });
            string result;
            try
            {
                result = _client.DownloadString(uri);
            }
            catch (WebException e)
            {
                Logger.Error(e);
                Logger.Error(e.StackTrace);

                throw e;
            }

            var parser = new HtmlParser();
            var document = parser.ParseDocument(result);
            var trs = document.QuerySelector("table.xferstable").QuerySelectorAll("tr");
            var first = true;
            
            var torrents = new List<TopicInfo>();
            foreach (var tr in trs)
            {
                if (first)
                {
                    first = false;
                    continue;
                }
                var a = tr.QuerySelector("td > a");
                var link = a.Attributes.GetNamedItem("href").Value;
                var downloaded = tr.QuerySelectorAll("td")[3].TextContent.Trim().ToInteger(0);
                var stopped = tr.QuerySelectorAll("td")[4].TextContent.Trim() == "Offline";
        
                var detailUri = _uriTemplateFree.BindByName(_baseUri, new Dictionary<string, string>
                {
                    {"url", link.Trim('/')}
                });
                var eventlogUri = _uriTemplateFree.BindByName(_baseUri, new Dictionary<string, string>
                {
                    {"url", link.Trim('/').Replace("details", "eventlog")}
                });
                
                string details;
                string eventlog;
                try
                {
                    details = _client.DownloadString(detailUri);
                    eventlog = _client.DownloadString(eventlogUri);
                }
                catch (WebException e)
                {
                    Logger.Error(e);
                    Logger.Error(e.StackTrace);

                    throw e;
                }

                var detailsDocument = parser.ParseDocument(details);
                var title = detailsDocument.QuerySelector("div.titlehdr").TextContent.Trim();
                var seeders = detailsDocument.QuerySelector("table.xferdetailstoprightstats").QuerySelectorAll("tr")[1].ChildNodes[1].TextContent.ToInteger(0);
                var leechers = detailsDocument.QuerySelector("table.xferdetailstoprightstats").QuerySelectorAll("tr")[2].ChildNodes[1].TextContent.ToInteger(0);
                    
                var info = new TopicInfo();
                
                info.Hash = Regex.Match(eventlog, "info-hash set to ([A-Z0-9]+)").Groups[1].Value;
                info.Name2 = title;
                info.TorrentName = title;
                info.Size = 0;
                info.Seeders = seeders;
                info.Leechers = leechers;
                info.Label = "";
                info.IsRun = !stopped;
                info.IsKeep = downloaded == 100;
                info.IsPause = stopped;
                info.IsDownload = !stopped && downloaded != 100;
                Logger.Error(info.ToString());
                torrents.Add(info);
            }

            return torrents;
        }

        public IEnumerable<string> GetFiles(TopicInfo topic)
        {
            return new string[0]; // TODO
            throw new NotImplementedException();
        }

        public void DistributionStop(IEnumerable<string> data)
        {
            return; // TODO
            throw new NotImplementedException();
        }

        public void DistributionPause(IEnumerable<string> data)
        {
            return; // TODO
            throw new NotImplementedException();
        }

        public void DistributionStart(IEnumerable<string> data)
        {
            return; // TODO
            throw new NotImplementedException();
        }

        public bool Ping()
        {
            try
            {
                var uri = _uriTemplate1.BindByName(_baseUri, new Dictionary<string, string>
                {
                    {"section", "home"}
                });
                var data = _client.DownloadString(uri);

                return true;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public bool SetDefaultFolder(string dir)
        {
            return true; // TODO
            throw new NotImplementedException();
        }

        public bool SetDefaultLabel(string label)
        {
            return true; // TODO
            throw new NotImplementedException();
        }

        public string GetDefaultFolder()
        {
            return ""; // TODO
            throw new NotImplementedException();
        }

        public void SendTorrentFile(string path, string file)
        {
            return; // TODO
            throw new NotImplementedException();
        }

        public void SendTorrentFile(string path, string filename, byte[] fdata)
        {
            var uri = _uriTemplateFree.BindByName(_baseUri, new Dictionary<string, string>
            {
                {"url", "transfers/action"}
            });

            var str = "----WebKitFormBoundary1vZaMilolI9TchBt";
            using (var memoryStream = new MemoryStream())
            {
                var bytes0 = Encoding.UTF8.GetBytes(
                    $"--{str}\r\nContent-Disposition: form-data; name=\"savepath\"\r\n\r\n{path}\r\n"
                    + $"--{str}\r\nContent-Disposition: form-data; name=\"addmetafile\"\r\n\r\nAdd\r\n"
                    + $"--{str}\r\nContent-Disposition: form-data; name=\"noautostart\"\r\n\r\n1\r\n");
                var bytes1 = Encoding.UTF8.GetBytes(
                    $"--{str}\r\nContent-Disposition: form-data; name=\"metafile\"; filename=\"{filename}\"\r\nContent-Type: application/x-bittorrent\r\n\r\n");
                memoryStream.Write(bytes0, 0, bytes0.Length);
                memoryStream.Write(bytes1, 0, bytes1.Length);
                memoryStream.Write(fdata, 0, fdata.Length);
                var bytes2 = Encoding.UTF8.GetBytes($"\r\n--{str}--\r\n");
                memoryStream.Write(bytes2, 0, bytes2.Length);
                var httpWebRequest = (HttpWebRequest) WebRequest.Create(uri);
                httpWebRequest.Host = this._host;
                httpWebRequest.Method = "POST";
                httpWebRequest.KeepAlive = true;
                httpWebRequest.ContentType = "multipart/form-data; boundary=" + str;
                httpWebRequest.CookieContainer = _client.CookieContainer;
                var array = memoryStream.ToArray();
                httpWebRequest.ContentLength = array.Length;
                using (var requestStream = httpWebRequest.GetRequestStream())
                {
                    requestStream.Write(array, 0, array.Length);
                    requestStream.Flush();
                    requestStream.Close();
                }

                var response = httpWebRequest.GetResponse();
                var responseStream = response.GetResponseStream();
                var reader = new StreamReader(responseStream);
                Logger.Error(reader.ReadToEnd());
                reader.Close();
            }
        }

        public string[] GetTrackers(string hash)
        {
            return new string[0]; // TODO
            throw new NotImplementedException();
        }

        public bool SetTrackers(string hash, string[] trackers)
        {
            return true; // TODO
            throw new NotImplementedException();
        }

        public bool SetLabel(string hash, string label)
        {
            return true;
            try
            {
                var uri = _uriTemplate.BindByName(_baseUri, new Dictionary<string, string>
                {
                    {"section", "torrents"}, {"methodName", "addTags"}
                });
                var data = $"hashes={hash.ToLower()}&tags={WebUtility.UrlEncode(label.Replace(",", "‚"))}";
                var bytes = Encoding.UTF8.GetBytes(data);

                var httpWebRequest = (HttpWebRequest) WebRequest.Create(uri);
                httpWebRequest.Method = "POST";
                httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                httpWebRequest.CookieContainer = _client.CookieContainer;
                httpWebRequest.ContentLength = bytes.Length;
                var stream = httpWebRequest.GetRequestStream();
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush();
                stream.Close();

                var response = httpWebRequest.GetResponse();
                var responseStream = response.GetResponseStream();
                var reader = new StreamReader(responseStream);
                reader.ReadToEnd();
                reader.Close();
                return true;
            }
            catch (WebException e)
            {
                Logger.Error(e.Message);
                Logger.Error(e.StackTrace);
                return false;
            }
        }

        public bool SetLabel(IEnumerable<string> hash, string label)
        {
            return true;
            try
            {
                var uri = _uriTemplate.BindByName(_baseUri, new Dictionary<string, string>
                {
                    {"section", "torrents"}, {"methodName", "addTags"}
                });
                var data =
                    $"hashes={string.Join("|", hash.ToList().ConvertAll((input => input.ToLower())))}&tags={WebUtility.UrlEncode(label.Replace(",", "‚"))}";
                var bytes = Encoding.UTF8.GetBytes(data);

                var httpWebRequest = (HttpWebRequest) WebRequest.Create(uri);
                httpWebRequest.Method = "POST";
                httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                httpWebRequest.CookieContainer = _client.CookieContainer;
                httpWebRequest.ContentLength = bytes.Length;
                var stream = httpWebRequest.GetRequestStream();
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush();
                stream.Close();

                var response = httpWebRequest.GetResponse();
                var responseStream = response.GetResponseStream();
                var reader = new StreamReader(responseStream);
                reader.ReadToEnd();
                reader.Close();

                return true;
            }
            catch (WebException e)
            {
                Logger.Error(e.Message);
                Logger.Error(e.StackTrace);
                return false;
            }
        }
    }
}