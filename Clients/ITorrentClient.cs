// Decompiled with JetBrains decompiler
// Type: TLO.local.ITorrentClient
// Assembly: TLO.local, Version=2.6.5944.27906, Culture=neutral, PublicKeyToken=null
// MVID: E76CFDB0-1920-4151-9DD8-5FF51DE7CC23
// Assembly location: C:\Users\root\Downloads\TLO_2.6.2.21\TLO.local.exe

using System.Collections.Generic;

namespace TLO.local
{
  internal interface ITorrentClient
  {
    List<TopicInfo> GetAllTorrentHash();

    IEnumerable<string> GetFiles(TopicInfo topic);

    void DistributionStop(IEnumerable<string> data);

    void DistributionPause(IEnumerable<string> data);

    void DistributionStart(IEnumerable<string> data);

    bool Ping();

    bool SetDefaultFolder(string dir);

    bool SetDefaultLabel(string label);

    string GetDefaultFolder();

    void SendTorrentFile(string path, string file);

    void SendTorrentFile(string path, string filename, byte[] fdata);

    string[] GetTrackers(string hash);

    bool SetTrackers(string hash, string[] trackers);

    bool SetLabel(string hash, string label);

    bool SetLabel(IEnumerable<string> hash, string label);
  }
}
