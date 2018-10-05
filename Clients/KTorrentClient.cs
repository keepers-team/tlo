// Decompiled with JetBrains decompiler
// Type: TLO.local.KTorrentClient
// Assembly: TLO.local, Version=2.6.5944.27906, Culture=neutral, PublicKeyToken=null
// MVID: E76CFDB0-1920-4151-9DD8-5FF51DE7CC23
// Assembly location: C:\Users\root\Downloads\TLO_2.6.2.21\TLO.local.exe

using NLog;
using System;
using System.Collections.Generic;
using System.Text;

namespace TLO.local
{
  internal class KTorrentClient : ITorrentClient
  {
    private static Logger _logger;
    private TLOWebClient _webClient;
    private string _ServerName;
    private int _ServerPort;
    private string svcCredentials;

    public KTorrentClient(string serverName, int port, string userName, string userPass)
    {
      if (KTorrentClient._logger == null)
        KTorrentClient._logger = LogManager.GetLogger("TransmissionClient");
      this._webClient = new TLOWebClient(Encoding.UTF8, "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:36.0) Gecko/20100101 Firefox/36.0", "application/json, text/javascript, */*; q=0.01", true);
      this._webClient.Encoding = Encoding.UTF8;
      this.svcCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(userName + ":" + userPass));
      this._webClient.Headers.Add("Authorization", "Basic " + this.svcCredentials);
      this._ServerName = serverName;
      this._ServerPort = port;
      try
      {
        this.Ping();
      }
      catch
      {
        KTorrentClient._logger.Debug(string.Format("Имя сервера: {0}; Порт сервера: {1}", (object) serverName, (object) port));
        throw;
      }
    }

    public List<TopicInfo> GetAllTorrentHash()
    {
      return new List<TopicInfo>();
    }

    public IEnumerable<string> GetFiles(TopicInfo topic)
    {
      return (IEnumerable<string>) new List<string>();
    }

    public void DistributionStop(IEnumerable<string> data)
    {
    }

    public void DistributionPause(IEnumerable<string> data)
    {
    }

    public void DistributionStart(IEnumerable<string> data)
    {
    }

    public bool Ping()
    {
      return true;
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
    }

    public void SendTorrentFile(string path, string filename, byte[] fdata)
    {
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
  }
}
