// Decompiled with JetBrains decompiler
// Type: TLO.local.TorrentClientInfo
// Assembly: TLO.local, Version=2.6.5944.27906, Culture=neutral, PublicKeyToken=null
// MVID: E76CFDB0-1920-4151-9DD8-5FF51DE7CC23
// Assembly location: C:\Users\root\Downloads\TLO_2.6.2.21\TLO.local.exe

using System;

namespace TLO.local
{
  internal class TorrentClientInfo
  {
    public TorrentClientInfo()
    {
      this.UID = Guid.NewGuid();
      this.Name = string.Empty;
      this.Type = "uTorrent";
      this.ServerName = string.Empty;
      this.ServerPort = 999;
      this.UserName = string.Empty;
      this.UserPassword = string.Empty;
      this.LastReadHash = new DateTime(2000, 1, 1);
    }

    public Guid UID { get; set; }

    public string Name { get; set; }

    public string Type { get; set; }

    public string ServerName { get; set; }

    public int ServerPort { get; set; }

    public string UserName { get; set; }

    public string UserPassword { get; set; }

    public DateTime LastReadHash { get; set; }

    public override string ToString()
    {
      return this.Name;
    }

    public ITorrentClient Create()
    {
      ITorrentClient torrentClient = (ITorrentClient) null;
      if (this.Type == "uTorrent")
        torrentClient = (ITorrentClient) new uTorrentClient(this.ServerName, this.ServerPort, this.UserName, this.UserPassword);
      else if (this.Type == "Transmission")
        torrentClient = (ITorrentClient) new TransmissionClient(this.ServerName, this.ServerPort, this.UserName, this.UserPassword);
      else if (this.Type == "Vuze (Vuze Web Remote)")
        torrentClient = (ITorrentClient) new TransmissionClient(this.ServerName, this.ServerPort, this.UserName, this.UserPassword);
      return torrentClient;
    }
  }
}
