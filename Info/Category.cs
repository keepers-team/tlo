// Decompiled with JetBrains decompiler
// Type: TLO.local.Category
// Assembly: TLO.local, Version=2.6.5944.27906, Culture=neutral, PublicKeyToken=null
// MVID: E76CFDB0-1920-4151-9DD8-5FF51DE7CC23
// Assembly location: C:\Users\root\Downloads\TLO_2.6.2.21\TLO.local.exe

using System;

namespace TLO.local
{
  internal class Category
  {
    public Category()
    {
      this.LastUpdateTopics = new DateTime(2000, 1, 1);
      this.LastUpdateStatus = new DateTime(2000, 1, 1);
      this.CountSeeders = 2;
      this.CreateSubFolder = 2;
    }

    public int CategoryID { get; set; }

    public int ParentID { get; set; }

    public int OrderID { get; set; }

    public string Name { get; set; }

    public string FullName { get; set; }

    public bool IsEnable { get; set; }

    public int CountSeeders { get; set; }

    public Guid TorrentClientUID { get; set; }

    public int CreateSubFolder { get; set; }

    public string Folder { get; set; }

    public string Label { get; set; }

    public bool IsSaveTorrentFiles { get; set; }

    public string FolderTorrentFile { get; set; }

    public bool IsSaveWebPage { get; set; }

    public string FolderSavePageForum { get; set; }

    public string ReportList { get; set; }

    public DateTime LastUpdateTopics { get; set; }

    public DateTime LastUpdateStatus { get; set; }

    public override string ToString()
    {
      return this.FullName;
    }
  }
}
