// Decompiled with JetBrains decompiler
// Type: TLO.local.Settings
// Assembly: TLO.local, Version=2.6.5944.27906, Culture=neutral, PublicKeyToken=null
// MVID: E76CFDB0-1920-4151-9DD8-5FF51DE7CC23
// Assembly location: C:\Users\root\Downloads\TLO_2.6.2.21\TLO.local.exe

using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;

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
        return Path.Combine(Folder, "TLO.local.Settings.xml");
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
          if (!Directory.Exists(Path.GetDirectoryName(FileSettings)))
            Directory.CreateDirectory(Path.GetDirectoryName(FileSettings));
          using (Stream stream = (Stream) File.Open(FileSettings, FileMode.Create, FileAccess.ReadWrite))
          {
            LogLevel = new int?(LogLevel.HasValue ? LogLevel.Value : 0);
            new XmlSerializer(typeof (Settings)).Serialize(stream, (object) this);
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.Message);
          Console.WriteLine(ex.StackTrace);
        }
        _LastWriteTime = File.GetLastWriteTime(FileSettings);
      }
    }

    public void Read()
    {
      try
      {
        lock (this)
        {
          using (Stream stream = (Stream) File.Open(FileSettings, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
          {
            Settings settings = (Settings) new XmlSerializer(typeof (Settings)).Deserialize(stream);
            IsUpdateStatistics = settings.IsUpdateStatistics;
            CountDaysKeepHistory = settings.CountDaysKeepHistory;
            PeriodRunAndStopTorrents = settings.PeriodRunAndStopTorrents;
            CountSeedersReport = settings.CountSeedersReport;
            IsAvgCountSeeders = settings.IsAvgCountSeeders;
            KeeperName = settings.KeeperName;
            KeeperPass = settings.KeeperPass;
            IsSelectLessOrEqual = settings.IsSelectLessOrEqual;
            IsNotSaveStatistics = settings.IsNotSaveStatistics;
            LastUpdateTopics = settings.LastUpdateTopics;
            ReportTop1 = settings.ReportTop1.Replace("\n", "\r\n").Replace("\r\r", "\r");
            ReportTop2 = settings.ReportTop2.Replace("\n", "\r\n").Replace("\r\r", "\r");
            ReportLine = settings.ReportLine.Replace("\n", "\r\n").Replace("\r\r", "\r");
            ReportBottom = settings.ReportBottom;
            HostRuTrackerOrg = settings.HostRuTrackerOrg;
            SetLogger(settings.LogLevel.HasValue ? settings.LogLevel.Value : 0);
            _LastWriteTime = File.GetLastWriteTime(FileSettings);
            LoadDBInMemory = settings.LoadDBInMemory;
            Proxy = settings.Proxy;
            ApiHost = settings.ApiHost;
            DisableServerCertVerify = settings.DisableServerCertVerify;
          }
        }
      }
      catch
      {
        Save();
      }
    }

    public void Checking()
    {
      if (!(File.GetLastWriteTime(FileSettings) != _LastWriteTime))
        return;
      Read();
    }

    public static Settings Current
    {
      get
      {
        if (_data == null)
          _data = new Settings();
        _data.Checking();
        return _data;
      }
    }

    public Settings()
    {
      KeeperName = string.Empty;
      KeeperPass = string.Empty;
      CountDaysKeepHistory = 7;
      PeriodRunAndStopTorrents = 60;
      CountSeedersReport = 10;
      IsSelectLessOrEqual = true;
      IsNotSaveStatistics = true;
      ReportLine = "[*] %%Status%% [url=viewtopic.php?t=%%ID%%]%%Name%%[/url] %%Size%%";
      ReportTop1 = "[b]Актуально на:[/b] %%CreateDate%%\r\n\r\nОбщее количество хранимых раздач подраздела: %%CountTopics%% шт. (%%SizeTopics%%)";
      ReportTop2 = "%%Top1%%[spoiler=\"Раздачи, взятые на хранение, №№ %%NumberTopicsFirst%% - %%NumberTopicsLast%%\"]\r\n[list=1]\r\n%%ReportLines%%\r\n[/list]\r\n[/spoiler]";
      ReportBottom = "";
      ReportCategories = "[hr]\r\n[hr]\r\n[b][color=darkgreen][align=center][size=16]Статистика раздела: {0}[/size][/align][/color][/b][hr]\r\n[hr]\r\n\r\n";
      HostRuTrackerOrg = "rutracker.org";
    }

    [XmlElement]
    public int? LogLevel { get; set; }

    private void SetLogger(int logLevel)
    {
      if (LogLevel.HasValue && LogLevel.Value == logLevel)
        return;
      string str = "BI.Analytics.Expert.Other";
      if (Assembly.GetEntryAssembly() != (Assembly) null)
        str = Assembly.GetEntryAssembly().ManifestModule.Name;
      LoggingConfiguration loggingConfiguration = new LoggingConfiguration();
      FileTarget fileTarget = new FileTarget();
      fileTarget.Layout = (Layout) "${date:format=yyyy-MM-dd HH\\:mm\\:ss}\t${level}\t${message}";
      loggingConfiguration.AddTarget("logfile", (Target) fileTarget);
      fileTarget.FileName = (Layout) Path.Combine(Folder, str + ".log");
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
      _logger.Info(string.Format("OS: {0} (Is64BitOperatingSystem: {1}, Version {2})", (object) Environment.OSVersion.VersionString, (object) Environment.Is64BitOperatingSystem, (object) Environment.OSVersion.Version.ToString()));
      LogLevel = new int?(logLevel);
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
    
    public string ReportCategories { get; set; }
    
    public string ReportSummary { get; set; }

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
