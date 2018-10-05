// Decompiled with JetBrains decompiler
// Type: TLO.local.uTorrentClient
// Assembly: TLO.local, Version=2.6.5944.27906, Culture=neutral, PublicKeyToken=null
// MVID: E76CFDB0-1920-4151-9DD8-5FF51DE7CC23
// Assembly location: C:\Users\root\Downloads\TLO_2.6.2.21\TLO.local.exe

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;

namespace TLO.local
{
  internal class uTorrentClient : ITorrentClient
  {
    private static Logger _logger;
    private TLOWebClient _webClient;
    private string _ServerName;
    private int _ServerPort;
    private string svcCredentials;
    private string token;

    public uTorrentClient(string serverName, int port, string userName, string userPass)
    {
      if (uTorrentClient._logger == null)
        uTorrentClient._logger = LogManager.GetLogger("uTorrentClient");
      this._webClient = new TLOWebClient();
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
        uTorrentClient._logger.Debug(string.Format("Имя сервера: {0}; Порт сервера: {1}", (object) this._ServerName, (object) this._ServerPort));
        throw;
      }
    }

    public bool Ping()
    {
      try
      {
        string[] array = ((IEnumerable<string>) this._webClient.GetString(string.Format("http://{0}:{1}/gui/", (object) this._ServerName, (object) this._ServerPort)).Split(new string[1]
        {
          "div"
        }, StringSplitOptions.RemoveEmptyEntries)).Where<string>((Func<string, bool>) (x => x.Contains("token"))).ToArray<string>();
        if (array.Length != 0)
          this.token = array[0].Split(new char[2]
          {
            '>',
            '<'
          }, StringSplitOptions.RemoveEmptyEntries)[1];
        else
          this.token = (string) null;
        return true;
      }
      catch
      {
        throw;
      }
    }

    public List<TopicInfo> GetAllTorrentHash()
    {
      List<string[]> source = new List<string[]>();
      if (!string.IsNullOrWhiteSpace(this.token))
        source.Add(new string[2]{ "token", this.token });
      source.Add(new string[2]{ "list", "1" });
      object[][] objArray = JsonConvert.DeserializeObject<JObject>(this._webClient.GetJson(string.Format("http://{0}:{1}/gui/?{2}", (object) this._ServerName, (object) this._ServerPort, (object) string.Join("&", source.Select<string[], string>((Func<string[], string>) (x => string.Join("=", x)))))))["torrents"].ToObject<object[][]>();
      if (objArray == null)
        return new List<TopicInfo>();
      return ((IEnumerable<object[]>) objArray).Select(x => new
      {
        Hash = (x[0] as string).ToUpper(),
        Name = x[2] as string,
        Size = x[3].GetType() == typeof (int) ? (long) (int) x[3] : (x[3].GetType() == typeof (long) ? (long) x[3] : 0L),
        Status = this.IntToArrayBool((long) x[1]),
        PercentComplite = (Decimal) ((long) x[4]) * new Decimal(1, 0, 0, false, (byte) 1)
      }).Select(x => new TopicInfo()
      {
        Hash = x.Hash,
        TorrentName = x.Name,
        Size = x.Size,
        IsKeep = x.Status != null && x.PercentComplite == new Decimal(100) && (x.Status[3] && !x.Status[4]) && x.Status[7],
        IsDownload = true,
        IsPause = x.Status[5],
        IsRun = !(x.PercentComplite == new Decimal(100)) || !x.Status[3] || (x.Status[4] || !x.Status[7]) ? new bool?() : new bool?(x.Status[0])
      }).ToList<TopicInfo>();
    }

    public IEnumerable<string> GetFiles(TopicInfo topic)
    {
      if (topic != null && !string.IsNullOrEmpty(topic.Hash))
      {
        List<string[]> source = new List<string[]>();
        if (!string.IsNullOrWhiteSpace(this.token))
          source.Add(new string[2]{ "token", this.token });
        source.Add(new string[2]{ "action", "getfiles" });
        source.Add(new string[2]{ "hash", topic.Hash });
        JToken jtoken1 = JsonConvert.DeserializeObject<JObject>(this._webClient.GetJson(string.Format("http://{0}:{1}/gui/?{2}", (object) this._ServerName, (object) this._ServerPort, (object) string.Join("&", source.Select<string[], string>((Func<string[], string>) (x => string.Join("=", x)))))))["files"];
        if (jtoken1 != null)
        {
          foreach (JToken jtoken2 in (IEnumerable<JToken>) jtoken1.ToObject<JArray>()[1].ToObject<JArray>())
            yield return jtoken2.ToObject<object[]>()[0].ToString();
        }
      }
    }

    private bool[] IntToArrayBool(long value)
    {
      BitArray bitArray = new BitArray(new int[1]
      {
        (int) value
      });
      bool[] flagArray = new bool[bitArray.Count];
      bitArray.CopyTo((Array) flagArray, 0);
      return flagArray;
    }

    public void DistributionStop(IEnumerable<string> data)
    {
      List<string[]> source = new List<string[]>();
      if (!string.IsNullOrWhiteSpace(this.token))
        source.Add(new string[2]{ "token", this.token });
      source.Add(new string[2]{ "action", "stop" });
      source.AddRange(data.Select<string, string[]>((Func<string, string[]>) (x => new string[2]
      {
        "hash",
        x
      })));
      this._webClient.GetJson(string.Format("http://{0}:{1}/gui/?{2}", (object) this._ServerName, (object) this._ServerPort, (object) string.Join("&", source.Select<string[], string>((Func<string[], string>) (x => string.Join("=", x))))));
    }

    public void DistributionPause(IEnumerable<string> data)
    {
      List<string[]> source = new List<string[]>();
      if (!string.IsNullOrWhiteSpace(this.token))
        source.Add(new string[2]{ "token", this.token });
      source.Add(new string[2]{ "action", "pause" });
      source.AddRange(data.Select<string, string[]>((Func<string, string[]>) (x => new string[2]
      {
        "hash",
        x
      })));
      this._webClient.GetJson(string.Format("http://{0}:{1}/gui/?{2}", (object) this._ServerName, (object) this._ServerPort, (object) string.Join("&", source.Select<string[], string>((Func<string[], string>) (x => string.Join("=", x))))));
    }

    public void DistributionStart(IEnumerable<string> data)
    {
      List<string[]> source = new List<string[]>();
      if (!string.IsNullOrWhiteSpace(this.token))
        source.Add(new string[2]{ "token", this.token });
      source.Add(new string[2]{ "action", "start" });
      source.AddRange(data.Select<string, string[]>((Func<string, string[]>) (x => new string[2]
      {
        "hash",
        x
      })));
      this._webClient.GetJson(string.Format("http://{0}:{1}/gui/?{2}", (object) this._ServerName, (object) this._ServerPort, (object) string.Join("&", source.Select<string[], string>((Func<string[], string>) (x => string.Join("=", x))))));
    }

    public bool SetDefaultFolder(string dir)
    {
      try
      {
        List<string[]> source = new List<string[]>();
        if (!string.IsNullOrWhiteSpace(this.token))
          source.Add(new string[2]{ "token", this.token });
        source.Add(new string[2]{ "action", "setsetting" });
        source.Add(new string[2]
        {
          "s",
          "dir_active_download"
        });
        source.Add(new string[2]
        {
          "v",
          HttpUtility.UrlEncode(dir)
        });
        this._webClient.GetJson(string.Format("http://{0}:{1}/gui/?{2}", (object) this._ServerName, (object) this._ServerPort, (object) string.Join("&", source.Select<string[], string>((Func<string[], string>) (x => string.Join("=", x))))));
        Thread.Sleep(100);
        return this.GetDefaultFolder() == dir;
      }
      catch
      {
        return false;
      }
    }

    public string GetDefaultFolder()
    {
      try
      {
        List<string[]> source = new List<string[]>();
        if (!string.IsNullOrWhiteSpace(this.token))
          source.Add(new string[2]{ "token", this.token });
        source.Add(new string[2]{ "action", "getsettings" });
        object[][] objArray1 = JsonConvert.DeserializeObject<JObject>(this._webClient.GetJson(string.Format("http://{0}:{1}/gui/?{2}", (object) this._ServerName, (object) this._ServerPort, (object) string.Join("&", source.Select<string[], string>((Func<string[], string>) (x => string.Join("=", x)))))))["settings"].ToObject<object[][]>();
        if (objArray1 == null)
          return string.Empty;
        object[] objArray2 = ((IEnumerable<object[]>) objArray1).Where<object[]>((Func<object[], bool>) (x => x[0] as string == "dir_active_download")).FirstOrDefault<object[]>();
        if (objArray2 == null)
          return string.Empty;
        return objArray2[2] as string;
      }
      catch
      {
        return (string) null;
      }
    }

    public bool SetDefaultLabel(string label)
    {
      try
      {
        List<string[]> source = new List<string[]>();
        if (!string.IsNullOrWhiteSpace(this.token))
          source.Add(new string[2]{ "token", this.token });
        source.Add(new string[2]{ "action", "setsetting" });
        source.Add(new string[2]{ "s", "dir_add_label" });
        source.Add(new string[2]
        {
          "v",
          HttpUtility.UrlEncode(label)
        });
        this._webClient.GetJson(string.Format("http://{0}:{1}/gui/?{2}", (object) this._ServerName, (object) this._ServerPort, (object) string.Join("&", source.Select<string[], string>((Func<string[], string>) (x => string.Join("=", x))))));
        Thread.Sleep(200);
        return this.GetDefaultLabel() == label;
      }
      catch
      {
        return false;
      }
    }

    public string GetDefaultLabel()
    {
      try
      {
        List<string[]> source = new List<string[]>();
        if (!string.IsNullOrWhiteSpace(this.token))
          source.Add(new string[2]{ "token", this.token });
        source.Add(new string[2]{ "action", "getsettings" });
        object[][] objArray1 = JsonConvert.DeserializeObject<JObject>(this._webClient.GetJson(string.Format("http://{0}:{1}/gui/?{2}", (object) this._ServerName, (object) this._ServerPort, (object) string.Join("&", source.Select<string[], string>((Func<string[], string>) (x => string.Join("=", x)))))))["settings"].ToObject<object[][]>();
        if (objArray1 == null)
          return string.Empty;
        object[] objArray2 = ((IEnumerable<object[]>) objArray1).Where<object[]>((Func<object[], bool>) (x => x[0] as string == "dir_add_label")).FirstOrDefault<object[]>();
        if (objArray2 == null)
          return string.Empty;
        return objArray2[2] as string;
      }
      catch
      {
        return (string) null;
      }
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
      string str = "----WebKitFormBoundary1vZaMilolI9TchBt";
      using (MemoryStream memoryStream = new MemoryStream())
      {
        byte[] bytes1 = Encoding.ASCII.GetBytes(string.Format("--{0}\r\nContent-Disposition: form-data; name=\"torrent_file\"; filename=\"{1}\"\r\nContent-Type: application/x-bittorrent\r\n\r\n", (object) str, (object) filename));
        memoryStream.Write(bytes1, 0, bytes1.Length);
        memoryStream.Write(fdata, 0, fdata.Length);
        byte[] bytes2 = Encoding.ASCII.GetBytes(string.Format("\r\n--{0}--\r\n", (object) str));
        memoryStream.Write(bytes2, 0, bytes2.Length);
        List<string[]> source = new List<string[]>();
        if (!string.IsNullOrWhiteSpace(this.token))
          source.Add(new string[2]{ "token", this.token });
        source.Add(new string[2]{ "action", "add-file" });
        HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create(string.Format("http://{0}:{1}/gui/?{2}", (object) this._ServerName, (object) this._ServerPort, (object) string.Join("&", source.Select<string[], string>((Func<string[], string>) (x => string.Join("=", x))))));
        httpWebRequest.Method = "POST";
        httpWebRequest.KeepAlive = true;
        httpWebRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
        httpWebRequest.ContentType = "multipart/form-data; boundary=" + str;
        httpWebRequest.Headers.Add("Authorization", this._webClient.Headers.Get("Authorization"));
        httpWebRequest.Headers.Add("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.5,en;q=0.3");
        byte[] array = memoryStream.ToArray();
        httpWebRequest.ContentLength = (long) array.Length;
        using (Stream requestStream = httpWebRequest.GetRequestStream())
          requestStream.Write(array, 0, array.Length);
      }
    }

    public string[] GetTrackers(string hash)
    {
      return (string[]) null;
    }

    public bool SetTrackers(string hash, string[] trackers)
    {
      try
      {
        string str = string.Join("\r\n\r\n", trackers) + "\r\n";
        List<string[]> source = new List<string[]>();
        if (!string.IsNullOrWhiteSpace(this.token))
          source.Add(new string[2]{ "token", this.token });
        source.Add(new string[2]{ "setprops", "setprops" });
        source.Add(new string[2]{ "hash", hash });
        source.Add(new string[2]{ "s", "trackers" });
        source.Add(new string[2]
        {
          "v",
          HttpUtility.UrlEncode(str)
        });
        this._webClient.GetJson(string.Format("http://{0}:{1}/gui/?{2}", (object) this._ServerName, (object) this._ServerPort, (object) string.Join("&", source.Select<string[], string>((Func<string[], string>) (x => string.Join("=", x))))));
        Thread.Sleep(100);
        string[] trackers1 = this.GetTrackers(hash);
        if (trackers1 == null)
          return false;
        return string.Join("\r\n\r\n", trackers1) + "\r\n" == str;
      }
      catch
      {
        return false;
      }
    }

    public bool SetLabel(string hash, string label)
    {
      Thread.Sleep(100);
      List<string[]> source = new List<string[]>();
      if (!string.IsNullOrWhiteSpace(this.token))
        source.Add(new string[2]{ "token", this.token });
      source.Add(new string[2]{ "action", "setprops" });
      source.Add(new string[2]{ "hash", hash });
      source.Add(new string[2]{ "s", "label" });
      source.Add(new string[2]{ "v", label });
      this._webClient.GetJson(string.Format("http://{0}:{1}/gui/?{2}", (object) this._ServerName, (object) this._ServerPort, (object) string.Join("&", source.Select<string[], string>((Func<string[], string>) (x => string.Join("=", x))))));
      Thread.Sleep(100);
      return true;
    }

    public bool SetLabel(IEnumerable<string> hashs, string label)
    {
      if (hashs == null || hashs.Count<string>() == 0)
        return true;
      List<string[]> source = new List<string[]>();
      foreach (string str in hashs)
      {
        if (source.Count<string[]>() == 0)
        {
          if (!string.IsNullOrWhiteSpace(this.token))
            source.Add(new string[2]{ "token", this.token });
          source.Add(new string[2]{ "action", "setprops" });
        }
        source.Add(new string[2]{ "s", "label" });
        source.Add(new string[2]{ "hash", str });
        source.Add(new string[2]{ "v", label });
        if (source.Count<string[]>() > 150)
        {
          this._webClient.GetJson(string.Format("http://{0}:{1}/gui/?{2}", (object) this._ServerName, (object) this._ServerPort, (object) string.Join("&", source.Select<string[], string>((Func<string[], string>) (x => string.Join("=", x))))));
          source.Clear();
        }
        Thread.Sleep(100);
      }
      if (source.Count<string[]>() != 0)
        this._webClient.GetJson(string.Format("http://{0}:{1}/gui/?{2}", (object) this._ServerName, (object) this._ServerPort, (object) string.Join("&", source.Select<string[], string>((Func<string[], string>) (x => string.Join("=", x))))));
      return true;
    }

    private class tt
    {
      public int build { get; set; }

      public List<object> files { get; set; }
    }
  }
}
