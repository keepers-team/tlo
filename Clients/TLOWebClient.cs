// Decompiled with JetBrains decompiler
// Type: TLO.local.TLOWebClient
// Assembly: TLO.local, Version=2.6.5944.27906, Culture=neutral, PublicKeyToken=null
// MVID: E76CFDB0-1920-4151-9DD8-5FF51DE7CC23
// Assembly location: C:\Users\root\Downloads\TLO_2.6.2.21\TLO.local.exe

using NLog;
using System;
using System.Net;
using System.Text;

namespace TLO.local
{
  internal class TLOWebClient : WebClient
  {
    private string _UserAgent = string.Empty;
    private string _Accept = string.Empty;
    private static Logger _logger;
    private bool _IsJson;

    public TLOWebClient()
      : this((Encoding) null, (string) null, (string) null, false)
    {
    }

    public TLOWebClient(Encoding encoding)
      : this(encoding, (string) null, (string) null, false)
    {
    }

    public TLOWebClient(Encoding encoding, string userAgent, string accept, bool isJson = false)
    {
      if (TLOWebClient._logger == null)
        TLOWebClient._logger = LogManager.GetCurrentClassLogger();
      Encoding encoding1;
      if (encoding != null)
        encoding1 = encoding;
      else
        encoding = encoding1 = Encoding.UTF8;
      this.Encoding = encoding1;
      this._UserAgent = string.IsNullOrWhiteSpace(userAgent) ? "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:35.0) Gecko/20100101 Firefox/35.0" : userAgent;
      this._Accept = string.IsNullOrWhiteSpace(accept) ? "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8" : accept;
      this.CookieContainer = new CookieContainer();
      this._IsJson = isJson;
    }

    public TLOWebClient(string userAgent)
    {
      this._UserAgent = userAgent;
    }

    public CookieContainer CookieContainer { get; private set; }

    protected override WebRequest GetWebRequest(Uri address)
    {
      HttpWebRequest webRequest = (HttpWebRequest) base.GetWebRequest(address);
      webRequest.Accept = this._IsJson ? "application/json" : this._Accept;
      webRequest.UserAgent = this._UserAgent;
      webRequest.Headers.Add("Accept-Encoding", "gzip, deflate");
      webRequest.Headers.Add("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.5,en;q=0.3");
      if (this._IsJson)
      {
        webRequest.Headers.Add("X-Request", "JSON");
        webRequest.Headers.Add("X-Requested-With", "XMLHttpRequest");
      }
      webRequest.ContentType = "application/x-www-form-urlencoded";
      webRequest.KeepAlive = true;
      webRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
      webRequest.Headers.Add("Pragma", "no-cache");
      webRequest.Timeout = 3600000;
      if (address.Host == "dl.rutracker.org" && address.AbsoluteUri.Contains("="))
      {
        string[] strArray = address.AbsoluteUri.Split(new char[1]
        {
          '='
        }, StringSplitOptions.RemoveEmptyEntries);
        this.CookieContainer.Add(address, new Cookie("bb_dl", strArray[1]));
        webRequest.Referer = string.Format("http://rutracker.org/forum/viewtopic.php?t={0}", (object) strArray[1]);
      }
      webRequest.CookieContainer = this.CookieContainer;
      return (WebRequest) webRequest;
    }

    public string GetString(string url)
    {
      this._IsJson = false;
      return this.DownloadString(url);
    }

    public string GetJson(string url)
    {
      this._IsJson = true;
      return this.DownloadString(url);
    }
  }
}
