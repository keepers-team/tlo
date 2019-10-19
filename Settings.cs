// Decompiled with JetBrains decompiler
// Type: TLO.local.Settings
// Assembly: TLO.local, Version=2.6.5944.27906, Culture=neutral, PublicKeyToken=null
// MVID: E76CFDB0-1920-4151-9DD8-5FF51DE7CC23
// Assembly location: C:\Users\root\Downloads\TLO_2.6.2.21\TLO.local.exe

using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace TLO.local
{
  public class Settings
  {
    private static Logger _logger = LogManager.GetLogger("Settings");
    private DateTime _LastWriteTime;
    private static Settings _data;

    public string FileSettings
    {
      get
      {
        return Path.Combine(this.Folder, "TLO.local.Settings.xml");
      }
    }

    public string Folder
    {
      get
      {
        return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
      }
    }

    public void Save()
    {
      lock (this)
      {
        try
        {
          if (!Directory.Exists(Path.GetDirectoryName(this.FileSettings)))
            Directory.CreateDirectory(Path.GetDirectoryName(this.FileSettings));
          using (Stream stream = (Stream) File.Open(this.FileSettings, FileMode.Create, FileAccess.ReadWrite))
          {
            this.LogLevel = new int?(this.LogLevel.HasValue ? this.LogLevel.Value : 0);
            new XmlSerializer(typeof (Settings)).Serialize(stream, (object) this);
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.Message);
          Console.WriteLine(ex.StackTrace);
        }
        this._LastWriteTime = File.GetLastWriteTime(this.FileSettings);
      }
    }

    public void Read()
    {
      try
      {
        lock (this)
        {
          using (Stream stream = (Stream) File.Open(this.FileSettings, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
          {
            Settings settings = (Settings) new XmlSerializer(typeof (Settings)).Deserialize(stream);
            this.IsUpdateStatistics = settings.IsUpdateStatistics;
            this.CountDaysKeepHistory = settings.CountDaysKeepHistory;
            this.PeriodRunAndStopTorrents = settings.PeriodRunAndStopTorrents;
            this.CountSeedersReport = settings.CountSeedersReport;
            this.IsAvgCountSeeders = settings.IsAvgCountSeeders;
            this.KeeperName = settings.KeeperName;
            this.KeeperPass = settings.KeeperPass;
            this.IsSelectLessOrEqual = settings.IsSelectLessOrEqual;
            this.IsNotSaveStatistics = settings.IsNotSaveStatistics;
            this.LastUpdateTopics = settings.LastUpdateTopics;
            this.ReportTop1 = settings.ReportTop1.Replace("\n", "\r\n").Replace("\r\r", "\r");
            this.ReportTop2 = settings.ReportTop2.Replace("\n", "\r\n").Replace("\r\r", "\r");
            this.ReportLine = settings.ReportLine.Replace("\n", "\r\n").Replace("\r\r", "\r");
            this.ReportBottom = settings.ReportBottom;
            this.HostRuTrackerOrg = settings.HostRuTrackerOrg;
            this.SetLogger(settings.LogLevel.HasValue ? settings.LogLevel.Value : 0);
            this._LastWriteTime = File.GetLastWriteTime(this.FileSettings);
            this.LoadDBInMemory = settings.LoadDBInMemory;
            this.Proxy = settings.Proxy;
            this.ApiHost = settings.ApiHost;
            this.DisableServerCertVerify = settings.DisableServerCertVerify;
          }
        }
      }
      catch
      {
        this.Save();
      }
    }

    public void Checking()
    {
      if (!(File.GetLastWriteTime(this.FileSettings) != this._LastWriteTime))
        return;
      this.Read();
    }

    public static Settings Current
    {
      get
      {
        if (Settings._data == null)
          Settings._data = new Settings();
        Settings._data.Checking();
        return Settings._data;
      }
    }

    public Settings()
    {
      this.KeeperName = string.Empty;
      this.KeeperPass = string.Empty;
      this.CountDaysKeepHistory = 7;
      this.PeriodRunAndStopTorrents = 60;
      this.CountSeedersReport = 10;
      this.IsSelectLessOrEqual = true;
      this.IsNotSaveStatistics = true;
      this.ReportLine = "[*] %%Status%% [url=viewtopic.php?t=%%ID%%]%%Name%%[/url] %%Size%%";
      this.ReportTop1 = "[b]Актуально на:[/b] %%CreateDate%%\r\n\r\nОбщее количество хранимых раздач подраздела: %%CountTopics%% шт. (%%SizeTopics%%)";
      this.ReportTop2 = "%%Top1%%[spoiler=\"Раздачи, взятые на хранение, №№ %%NumberTopicsFirst%% - %%NumberTopicsLast%%\"]\r\n[list=1]\r\n%%ReportLines%%\r\n[/list]\r\n[/spoiler]";
      this.ReportBottom = "";
      this.HostRuTrackerOrg = "rutracker.org";
    }

    [XmlElement]
    public int? LogLevel { get; set; }

    private void SetLogger(int logLevel)
    {
      if (this.LogLevel.HasValue && this.LogLevel.Value == logLevel)
        return;
      string str = "BI.Analytics.Expert.Other";
      if (Assembly.GetEntryAssembly() != (Assembly) null)
        str = Assembly.GetEntryAssembly().ManifestModule.Name;
      LoggingConfiguration loggingConfiguration = new LoggingConfiguration();
      FileTarget fileTarget = new FileTarget();
      fileTarget.Layout = (Layout) "${date:format=yyyy-MM-dd HH\\:mm\\:ss}\t${level}\t${message}";
      loggingConfiguration.AddTarget("logfile", (Target) fileTarget);
      fileTarget.FileName = (Layout) Path.Combine(this.Folder, str + ".log");
      fileTarget.Encoding = Encoding.UTF8;
      fileTarget.ArchiveAboveSize = 20971520L;
      if (Environment.UserInteractive)
      {
        ColoredConsoleTarget coloredConsoleTarget = new ColoredConsoleTarget();
        loggingConfiguration.AddTarget("console", (Target) coloredConsoleTarget);
        coloredConsoleTarget.Layout = (Layout) "${date:format=yyyy-MM-dd HH\\:mm\\:ss}\t${level}\t${message}\t${file}:${line}";
        LoggingRule loggingRule = new LoggingRule("*", NLog.LogLevel.Debug, (Target) coloredConsoleTarget);
        loggingConfiguration.LoggingRules.Add(loggingRule);
      }
      LoggingRule loggingRule1 = logLevel > 0 ? (logLevel != 1 ? (logLevel != 2 ? new LoggingRule("*", NLog.LogLevel.Trace, (Target) fileTarget) : new LoggingRule("*", NLog.LogLevel.Debug, (Target) fileTarget)) : new LoggingRule("*", NLog.LogLevel.Info, (Target) fileTarget)) : new LoggingRule("*", NLog.LogLevel.Warn, (Target) fileTarget);
      loggingConfiguration.LoggingRules.Add(loggingRule1);
      LogManager.Configuration = loggingConfiguration;
      Settings._logger.Info(string.Format("OS: {0} (Is64BitOperatingSystem: {1}, Version {2})", (object) Environment.OSVersion.VersionString, (object) Environment.Is64BitOperatingSystem, (object) Environment.OSVersion.Version.ToString()));
      this.LogLevel = new int?(logLevel);
    }

    [XmlAttribute]
    public string KeeperName { get; set; }

    [XmlAttribute]
    public string KeeperPass { get; set; }

    [XmlAttribute]
    public bool IsUpdateStatistics { get; set; }

    [XmlAttribute]
    public int CountDaysKeepHistory { get; set; }

    [XmlAttribute]
    public int PeriodRunAndStopTorrents { get; set; }

    [XmlAttribute]
    public int CountSeedersReport { get; set; }

    [XmlAttribute]
    public bool IsAvgCountSeeders { get; set; }

    [XmlAttribute]
    public bool IsSelectLessOrEqual { get; set; }

    [XmlAttribute]
    public bool IsNotSaveStatistics { get; set; }

    [XmlAttribute]
    public DateTime LastUpdateTopics { get; set; }

    [XmlElement]
    public string ReportTop1 { get; set; }

    [XmlElement]
    public string ReportTop2 { get; set; }

    [XmlElement]
    public string ReportLine { get; set; }

    [XmlElement]
    public string ReportBottom { get; set; }

    [XmlElement]
    public string HostRuTrackerOrg { get; set; }
    
    [XmlElement]
    public bool? LoadDBInMemory { get; set; }

    [XmlElement]
    public string Proxy { get; set; }
    
    [XmlElement]
    public bool? DisableServerCertVerify { get; set; }
    
    [XmlElement]
    public string ApiHost { get; set; }
  }
}
