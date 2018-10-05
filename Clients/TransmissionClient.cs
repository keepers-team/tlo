// Decompiled with JetBrains decompiler
// Type: TLO.local.TransmissionClient
// Assembly: TLO.local, Version=2.6.5944.27906, Culture=neutral, PublicKeyToken=null
// MVID: E76CFDB0-1920-4151-9DD8-5FF51DE7CC23
// Assembly location: C:\Users\root\Downloads\TLO_2.6.2.21\TLO.local.exe

using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace TLO.local
{
  internal class TransmissionClient : ITorrentClient
  {
    private static Logger _logger;
    private TLOWebClient _webClient;
    private string _URL;
    private string svcCredentials;

    public TransmissionClient(string serverName, int port, string userName, string userPass)
    {
      if (TransmissionClient._logger == null)
        TransmissionClient._logger = LogManager.GetLogger("TransmissionClient");
      this._webClient = new TLOWebClient(Encoding.UTF8, "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:36.0) Gecko/20100101 Firefox/36.0", "application/json, text/javascript, */*; q=0.01", true);
      this._webClient.Encoding = Encoding.UTF8;
      this.svcCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(userName + ":" + userPass));
      this._webClient.Headers.Add("Authorization", "Basic " + this.svcCredentials);
      this._URL = string.Format("http://{0}:{1}/transmission/rpc", (object) serverName, (object) port);
      try
      {
        this.Ping();
      }
      catch
      {
        TransmissionClient._logger.Debug(string.Format("Имя сервера: {0}; Порт сервера: {1}", (object) serverName, (object) port));
        throw;
      }
    }

    public List<TopicInfo> GetAllTorrentHash()
    {
      try
      {
        TransmissionClient.Querty querty = JsonConvert.DeserializeObject<TransmissionClient.Querty>(Encoding.UTF8.GetString(this._webClient.UploadData(this._URL, "POST", Encoding.UTF8.GetBytes("{\"method\":\"torrent-get\",\"arguments\":{\"fields\":[\"hashString\", \"totalSize\", \"percentDone\", \"error\", \"status\"]}}"))));
        if (querty == null || querty.arguments == null || (querty.arguments.torrents == null || querty.arguments.torrents.Length == 0))
          return new List<TopicInfo>();
        return ((IEnumerable<TransmissionClient.Torrent>) querty.arguments.torrents).Select<TransmissionClient.Torrent, TopicInfo>((Func<TransmissionClient.Torrent, TopicInfo>) (t => new TopicInfo()
        {
          Hash = t.hashString.ToUpper(),
          IsKeep = t.percentDone == Decimal.One && t.error == 0,
          IsDownload = true,
          IsRun = !(t.percentDone == Decimal.One) || t.error != 0 ? new bool?() : new bool?(t.status == 6)
        })).ToList<TopicInfo>();
      }
      catch (Exception ex)
      {
        TransmissionClient._logger.Error(ex.Message);
        TransmissionClient._logger.Debug(ex.StackTrace);
        throw ex;
      }
    }

    public IEnumerable<string> GetFiles(TopicInfo topic)
    {
      yield break;
    }

    public void DistributionStop(IEnumerable<string> data)
    {
      TransmissionClient.Querty querty = JsonConvert.DeserializeObject<TransmissionClient.Querty>(Encoding.UTF8.GetString(this._webClient.UploadData(this._URL, "POST", Encoding.UTF8.GetBytes("{\"method\":\"torrent-get\",\"arguments\":{\"fields\":[\"hashString\", \"id\"]}}"))));
      if (querty == null || querty.arguments == null || (querty.arguments.torrents == null || querty.arguments.torrents.Length == 0))
        return;
      int[] array = ((IEnumerable<TransmissionClient.Torrent>) querty.arguments.torrents).Join<TransmissionClient.Torrent, string, string, int>(data, (Func<TransmissionClient.Torrent, string>) (t => t.hashString.ToUpper()), (Func<string, string>) (d => d), (Func<TransmissionClient.Torrent, string, int>) ((t, d) => t.id)).ToArray<int>();
      if (array.Length == 0)
        return;
      this._webClient.UploadData(this._URL, "POST", Encoding.UTF8.GetBytes(JsonConvert.SerializeObject((object) new
      {
        method = "torrent-stop",
        arguments = new{ ids = array }
      })));
    }

    public void DistributionPause(IEnumerable<string> data)
    {
      TransmissionClient.Querty querty = JsonConvert.DeserializeObject<TransmissionClient.Querty>(Encoding.UTF8.GetString(this._webClient.UploadData(this._URL, "POST", Encoding.UTF8.GetBytes("{\"method\":\"torrent-get\",\"arguments\":{\"fields\":[\"hashString\", \"id\"]}}"))));
      if (querty == null || querty.arguments == null || (querty.arguments.torrents == null || querty.arguments.torrents.Length == 0))
        return;
      int[] array = ((IEnumerable<TransmissionClient.Torrent>) querty.arguments.torrents).Join<TransmissionClient.Torrent, string, string, int>(data, (Func<TransmissionClient.Torrent, string>) (t => t.hashString.ToUpper()), (Func<string, string>) (d => d), (Func<TransmissionClient.Torrent, string, int>) ((t, d) => t.id)).ToArray<int>();
      if (array.Length == 0)
        return;
      this._webClient.UploadData(this._URL, "POST", Encoding.UTF8.GetBytes(JsonConvert.SerializeObject((object) new
      {
        method = "torrent-stop",
        arguments = new{ ids = array }
      })));
    }

    public void DistributionStart(IEnumerable<string> data)
    {
      TransmissionClient.Querty querty = JsonConvert.DeserializeObject<TransmissionClient.Querty>(Encoding.UTF8.GetString(this._webClient.UploadData(this._URL, "POST", Encoding.UTF8.GetBytes("{\"method\":\"torrent-get\",\"arguments\":{\"fields\":[\"hashString\", \"id\"]}}"))));
      if (querty == null || querty.arguments == null || (querty.arguments.torrents == null || querty.arguments.torrents.Length == 0))
        return;
      int[] array = ((IEnumerable<TransmissionClient.Torrent>) querty.arguments.torrents).Join<TransmissionClient.Torrent, string, string, int>(data, (Func<TransmissionClient.Torrent, string>) (t => t.hashString.ToUpper()), (Func<string, string>) (d => d), (Func<TransmissionClient.Torrent, string, int>) ((t, d) => t.id)).ToArray<int>();
      if (array.Length == 0)
        return;
      this._webClient.UploadData(this._URL, "POST", Encoding.UTF8.GetBytes(JsonConvert.SerializeObject((object) new
      {
        method = "torrent-start",
        arguments = new{ ids = array }
      })));
    }

    public bool Ping()
    {
      while (true)
      {
        try
        {
          this._webClient.UploadData(this._URL, "POST", Encoding.UTF8.GetBytes("{\"method\":\"session-get\"}"));
          return true;
        }
        catch (WebException ex)
        {
          this._webClient.Headers.Add("X-Transmission-Session-Id", ex.Response.Headers["X-Transmission-Session-Id"]);
        }
        catch (Exception ex)
        {
          TransmissionClient._logger.Error(ex.Message);
          TransmissionClient._logger.Debug(ex.StackTrace);
          throw ex;
        }
      }
    }

    public bool SetDefaultFolder(string dir)
    {
      return true;
    }

    public bool SetDefaultLabel(string label)
    {
      return true;
    }

    public string GetDefaultFolder()
    {
      return string.Empty;
    }

    public void SendTorrentFile(string path, string file)
    {
      using (MemoryStream memoryStream = new MemoryStream())
      {
        using (FileStream fileStream = System.IO.File.OpenRead(file))
        {
          fileStream.CopyTo((Stream) memoryStream);
          this.SendTorrentFile(path, Path.GetFileName(file), memoryStream.ToArray());
        }
      }
    }

    public void SendTorrentFile(string path, string filename, byte[] fdata)
    {
      string base64String = Convert.ToBase64String(fdata);
      this._webClient.UploadData(this._URL, "POST", Encoding.UTF8.GetBytes(JsonConvert.SerializeObject((object) new
      {
        method = "torrent-add",
        arguments = new
        {
          paused = false,
          downloadDir = path,
          metainfo = base64String
        }
      }).Replace("downloadDir", "download-dir")));
    }

    public string[] GetTrackers(string hash)
    {
      return (string[]) null;
    }

    public bool SetTrackers(string hash, string[] trackers)
    {
      return true;
    }

    public bool SetLabel(string hash, string label)
    {
      return true;
    }

    public bool SetLabel(IEnumerable<string> hash, string label)
    {
      return true;
    }

    private class Querty
    {
      public TransmissionClient.Argument arguments { get; set; }
    }

    private class Argument
    {
      public TransmissionClient.Torrent[] torrents { get; set; }
    }

    private class Torrent
    {
      public int id { get; set; }

      public string hashString { get; set; }

      public long totalSize { get; set; }

      public Decimal percentDone { get; set; }

      public int error { get; set; }

      public int status { get; set; }
    }
  }
}
