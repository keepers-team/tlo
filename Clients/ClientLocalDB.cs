// Decompiled with JetBrains decompiler
// Type: TLO.local.ClientLocalDB
// Assembly: TLO.local, Version=2.6.5944.27906, Culture=neutral, PublicKeyToken=null
// MVID: E76CFDB0-1920-4151-9DD8-5FF51DE7CC23
// Assembly location: C:\Users\root\Downloads\TLO_2.6.2.21\TLO.local.exe

using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TLO.local
{
  internal class ClientLocalDB
  {
    private static ClientLocalDB _current;
    private SQLiteConnection _conn;
    private static Logger _logger;

    public static ClientLocalDB Current
    {
      get
      {
        if (_current == null)
          _current = new ClientLocalDB();
        return _current;
      }
    }

    public string FileDatabase
    {
      get
      {
        return Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Database.db");
      }
    }

    public SQLiteCommand CreateCommand()
    {
      return _conn.CreateCommand();
    }

    private ClientLocalDB()
    {
      if (_logger == null)
        _logger = LogManager.GetLogger("ClientServer");
      bool flag = false;
      if (!File.Exists(FileDatabase))
        flag = true;
      try
      {
        _conn = new DBConnectionCreator().Connection;
      }
      catch
      {
        flag = true;
      }
      if (flag)
        CreateDatabase();
      SaveToDatabase();
      UpdateDataBase();
    }

    public void SaveToDatabase()
    {
      if (!DBConnectionCreator.InMemory())
      {
        // не нужно пересохранять в БД если, она не хранится в памяти.
        return;
      }
      try
      {
        if (File.Exists(FileDatabase + ".tmp"))
          File.Delete(FileDatabase + ".tmp");
        using (SQLiteConnection destination = new SQLiteConnection(string.Format("Data Source={0};Version=3;", FileDatabase + ".tmp")))
        {
          destination.Open();
          _conn.BackupDatabase(destination, "main", "main", -1, null, -1);
          destination.Close();
        }
      }
      catch (Exception ex)
      {
        _logger.Error(ex.Message + "\r\n" + ex.StackTrace);
      }
      if (!File.Exists(FileDatabase + ".tmp"))
        return;
      _conn?.Close();
      if (File.Exists(FileDatabase))
        File.Delete(FileDatabase);
      File.Move(FileDatabase + ".tmp", FileDatabase);
      _conn = new DBConnectionCreator().Connection;
    }

    private void CreateDatabase()
    {
      _conn?.Close();
      if (File.Exists(FileDatabase))
        File.Delete(FileDatabase);
      _conn = new DBConnectionCreator().Connection;
      using (SQLiteCommand command = _conn.CreateCommand())
      {
        command.CommandText = "\r\nCREATE TABLE Category(CategoryID INTEGER PRIMARY KEY ASC, ParentID INTEGER, OrderID INT, Name TEXT NOT NULL, FullName TEXT NOT NULL, IsEnable BIT, CountSeeders int, \r\n    TorrentClientUID TEXT, Folder TEXT, AutoDownloads INT, LastUpdateTopics DATETIME, LastUpdateStatus DATETIME, Label TEXT, ReportTopicID INT);\r\nCREATE TABLE Topic (TopicID INT PRIMARY KEY ASC, CategoryID INT, Name TEXT, Hash TEXT, Size INTEGER, Seeders INT, AvgSeeders DECIMAL(18,4), Status INT, IsActive BIT, IsDeleted BIT, IsKeep BIT, IsKeepers BIT, IsBlackList BIT, IsDownload BIT, RegTime DATETIME, PosterID INT);\r\nCREATE INDEX IX_Topic__Hash ON Topic (Hash);\r\nCREATE TABLE TopicStatusHystory (TopicID INT NOT NULL, Date DateTime NOT NULL, Seeders INT, PRIMARY KEY(TopicID ASC, Date ASC));\r\nCREATE TABLE TorrentClient(UID NVARCHAR(50) PRIMARY KEY ASC NOT NULL, Name NVARCHAR(100) NOT NULL, Type VARCHAR(50) NOT NULL, ServerName NVARCHAR(50) NOT NULL, ServerPort INT NOT NULL, UserName NVARCHAR(50), UserPassword NVARCHAR(50), LastReadHash DATETIME);\r\nCREATE TABLE Report(CategoryID INT NOT NULL, ReportNo INT NOT NULL, URL TEXT, Report TEXT, PRIMARY KEY(CategoryID ASC, ReportNo ASC));\r\nCREATE TABLE Keeper (KeeperName nvarchar(100) not null, CategoryID int not null, Count INT NOT NULL, Size DECIMAL(18,4) NOT NULL, PRIMARY KEY(KeeperName ASC, CategoryID ASC));\r\nCREATE TABLE KeeperToTopic(KeeperName NVARCHAR(50) NOT NULL, CategoryID INT NULL, TopicID INT NOT NULL, PRIMARY KEY(KeeperName ASC, TopicID ASC));\r\nCREATE TABLE User (UserID INT PRIMARY KEY ASC NOT NULL, Name NVARCHAR(100) NOT NULL);\r\n";
        command.ExecuteNonQuery();
      }
    }

    public void ClearDatabase()
    {
      using (SQLiteTransaction sqLiteTransaction = _conn.BeginTransaction())
      {
        using (SQLiteCommand command = _conn.CreateCommand())
        {
          HashSet<int> intSet = new HashSet<int>();
          command.Transaction = sqLiteTransaction;
          command.CommandText = "DELETE FROM Topic";
          command.ExecuteNonQuery();
          command.CommandText = "DELETE FROM TopicStatusHystory";
          command.ExecuteNonQuery();
          command.CommandText = "UPDATE Report SET Report = ''";
          command.ExecuteNonQuery();
        }
        sqLiteTransaction.Commit();
        using (SQLiteCommand command = _conn.CreateCommand())
        {
          command.CommandText = "vacuum;";
          command.ExecuteNonQuery();
        }
      }
    }

    public void UpdateDataBase()
    {
      using (var command = _conn.CreateCommand())
      {
        var updated = false;
        command.CommandText = "PRAGMA user_version";
        var result = (long)command.ExecuteScalar();
        for (var i = 0; i <= 1; i++, result++)
        {
          switch (result)
          {
            case 0:
              command.CommandText = @"CREATE INDEX keepername_topicid_idx ON KeeperToTopic (TopicID, KeeperName)";
              command.ExecuteNonQuery();
              updated = true;
              continue;
            default:
              command.CommandText = $"PRAGMA user_version={i}";
              command.ExecuteNonQuery();
              break;
          }
        }

        if (updated)
        {
          SaveToDatabase();
        }
      }
    }

    public IEnumerable<UserInfo> GetUsers()
    {
      List<UserInfo> userInfoList = new List<UserInfo>();
      using (SQLiteCommand command = _conn.CreateCommand())
      {
        command.CommandText = "SELECT * FROM User";
        using (SQLiteDataReader sqLiteDataReader = command.ExecuteReader())
        {
          while (sqLiteDataReader.Read())
            userInfoList.Add(new UserInfo()
            {
              UserID = sqLiteDataReader.GetInt32(0),
              Name = sqLiteDataReader.GetString(1)
            });
        }
      }
      return userInfoList;
    }

    public void SaveUsers(IEnumerable<UserInfo> data)
    {
      if (data == null || data.Count() == 0)
        return;
      using (SQLiteTransaction sqLiteTransaction = _conn.BeginTransaction())
      {
        using (SQLiteCommand command = _conn.CreateCommand())
        {
          command.Transaction = sqLiteTransaction;
          command.CommandText = "INSERT OR REPLACE INTO User(UserID, Name) VALUES(@UserID, @Name);";
          command.Parameters.Add("@UserID", DbType.Int32);
          command.Parameters.Add("@Name", DbType.String);
          command.Prepare();
          foreach (UserInfo userInfo in data)
          {
            command.Parameters[0].Value = userInfo.UserID;
            command.Parameters[1].Value = userInfo.Name ?? "<Удален>";
            command.ExecuteNonQuery();
          }
        }
        sqLiteTransaction.Commit();
      }
    }

    public int[] GetNoUsers()
    {
      List<int> intList = new List<int>();
      using (SQLiteCommand command = _conn.CreateCommand())
      {
        command.CommandText = "\r\nSELECT DISTINCT t.PosterID\r\nFROM \r\n     Topic AS t     \r\n     LEFT JOIN User AS u ON (t.PosterID = u.UserID)     \r\nWHERE\r\n     t.PosterID IS NOT NULL AND u.Name IS NULL";
        using (SQLiteDataReader sqLiteDataReader = command.ExecuteReader())
        {
          while (sqLiteDataReader.Read())
          {
            var posterId = sqLiteDataReader.GetInt32(0);
            if (posterId > 0)
            {
              intList.Add(posterId);
            }
          }
        }
      }
      return intList.ToArray();
    }

    public void CategoriesSave(IEnumerable<Category> data, bool isLoad = false)
    {
      using (SQLiteTransaction sqLiteTransaction = _conn.BeginTransaction())
      {
        using (SQLiteCommand command = _conn.CreateCommand())
        {
          HashSet<int> hash = new HashSet<int>();
          command.Transaction = sqLiteTransaction;
          command.CommandText = string.Format("select CategoryID FROM Category WHERE CategoryID IN ({0})", string.Join(",", data.Select(x => x.CategoryID)));
          using (SQLiteDataReader sqLiteDataReader = command.ExecuteReader())
          {
            while (sqLiteDataReader.Read())
              hash.Add(sqLiteDataReader.GetInt32(0));
          }
          if (isLoad)
          {
            command.CommandText = "UPDATE Category SET ParentID = @ParentID, OrderID = @OrderID, Name = @Name, FullName = @FullName WHERE CategoryID = @ID";
            IEnumerable<Category> source = data;
            foreach (Category category in source.Where(x => hash.Contains(x.CategoryID)))
            {
              command.Parameters.Clear();
              command.Parameters.AddWithValue("@ID", category.CategoryID);
              command.Parameters.AddWithValue("@ParentID", category.ParentID);
              command.Parameters.AddWithValue("@OrderID", category.OrderID);
              command.Parameters.AddWithValue("@Name", category.Name);
              command.Parameters.AddWithValue("@FullName", string.IsNullOrWhiteSpace(category.FullName) ? category.Name : category.FullName);
              command.ExecuteNonQuery();
            }
          }
          else
          {
            command.CommandText = "UPDATE Category SET IsEnable = 0 WHERE IsEnable = 1";
            command.ExecuteNonQuery();
            command.CommandText = "UPDATE Category SET IsEnable = @IsEnable, Folder = @Folder, LastUpdateTopics = @LastUpdateTopics, LastUpdateStatus = @LastUpdateStatus, CountSeeders = @CountSeeders, TorrentClientUID = @TorrentClientUID, Label = @Label WHERE CategoryID = @ID";
            IEnumerable<Category> source = data;
            foreach (Category category in source.Where(x => hash.Contains(x.CategoryID)))
            {
              string str = string.Format("{0}|{1}|{2}|{3}", (object) category.Folder, (object) category.CreateSubFolder, category.IsSaveTorrentFiles ? (object) "1" : (object) "0", category.IsSaveWebPage ? (object) "1" : (object) "0");
              command.Parameters.Clear();
              command.Parameters.AddWithValue("@ID", category.CategoryID);
              command.Parameters.AddWithValue("@IsEnable", category.IsEnable);
              command.Parameters.AddWithValue("@CountSeeders", category.CountSeeders);
              command.Parameters.AddWithValue("@TorrentClientUID", category.TorrentClientUID.ToString());
              command.Parameters.AddWithValue("@Folder", str);
              command.Parameters.AddWithValue("@LastUpdateTopics", category.LastUpdateTopics);
              command.Parameters.AddWithValue("@LastUpdateStatus", category.LastUpdateStatus);
              command.Parameters.AddWithValue("@Label", category.Label);
              command.ExecuteNonQuery();
            }
          }
          command.CommandText = "INSERT OR REPLACE INTO Category (CategoryID, ParentID, OrderID, Name, FullName, IsEnable, Folder, LastUpdateTopics, LastUpdateStatus, Label) \r\nVALUES(@ID, @ParentID, @OrderID, @Name, @FullName, @IsEnable, @Folder, @LastUpdateTopics, @LastUpdateStatus, @Label)";
          IEnumerable<Category> source1 = data;
          foreach (Category category in source1.Where(x => !hash.Contains(x.CategoryID)))
          {
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@ID", category.CategoryID);
            command.Parameters.AddWithValue("@ParentID", category.ParentID);
            command.Parameters.AddWithValue("@OrderID", category.OrderID);
            command.Parameters.AddWithValue("@Name", category.Name);
            command.Parameters.AddWithValue("@FullName", string.IsNullOrWhiteSpace(category.FullName) ? category.Name : category.FullName);
            command.Parameters.AddWithValue("@IsEnable", category.IsEnable);
            command.Parameters.AddWithValue("@CountSeeders", category.CountSeeders);
            command.Parameters.AddWithValue("@Folder", category.Folder);
            command.Parameters.AddWithValue("@LastUpdateTopics", category.LastUpdateTopics);
            command.Parameters.AddWithValue("@LastUpdateStatus", category.LastUpdateStatus);
            command.Parameters.AddWithValue("@Label", category.Label);
            command.ExecuteNonQuery();
          }
        }
        sqLiteTransaction.Commit();
      }
    }

    public List<Category> GetCategories()
    {
      List<Category> categoryList = new List<Category>();
      using (SQLiteCommand command = _conn.CreateCommand())
      {
        command.CommandText = "\r\nSELECT CategoryID, ParentID, OrderID, Name, FullName, IsEnable, Folder, LastUpdateTopics, LastUpdateStatus, CountSeeders, TorrentClientUID, ReportTopicID, Label FROM Category";
        using (SQLiteDataReader sqLiteDataReader = command.ExecuteReader())
        {
          while (sqLiteDataReader.Read())
          {
            Category category = new Category()
            {
              CategoryID = sqLiteDataReader.GetInt32(0),
              ParentID = sqLiteDataReader.GetInt32(1),
              OrderID = sqLiteDataReader.GetInt32(2),
              Name = sqLiteDataReader.GetString(3),
              FullName = sqLiteDataReader.GetString(4),
              IsEnable = sqLiteDataReader.GetBoolean(5),
              CountSeeders = sqLiteDataReader.IsDBNull(9) ? 2 : sqLiteDataReader.GetInt32(9),
              TorrentClientUID = sqLiteDataReader.IsDBNull(10) ? Guid.Empty : Guid.Parse(sqLiteDataReader.GetString(10)),
              LastUpdateTopics = sqLiteDataReader.GetDateTime(7),
              LastUpdateStatus = sqLiteDataReader.GetDateTime(8),
              ReportList = sqLiteDataReader.IsDBNull(11) ? string.Empty : sqLiteDataReader.GetString(11),
              Label = sqLiteDataReader.IsDBNull(12) ? string.Empty : sqLiteDataReader.GetString(12)
            };
            string str = sqLiteDataReader.IsDBNull(6) ? null : sqLiteDataReader.GetString(6);
            if (!string.IsNullOrWhiteSpace(str))
            {
              string[] strArray = str.Split('|');
              if (strArray.Length >= 1)
                category.Folder = strArray[0];
              if (strArray.Length >= 2)
                category.CreateSubFolder = int.Parse(strArray[1]);
              if (strArray.Length >= 3)
              {
                category.IsSaveTorrentFiles = strArray[2] == "1";
                category.FolderTorrentFile = Path.Combine(category.Folder, "!!!Torrent-files!!!");
              }
              if (strArray.Length >= 4)
              {
                category.IsSaveWebPage = strArray[3] == "1";
                category.FolderSavePageForum = Path.Combine(category.Folder, "!!!Web-pages!!!");
              }
            }
            categoryList.Add(category);
          }
        }
      }
      return categoryList;
    }

    public List<Category> GetCategoriesEnable(bool withUnknown = false)
    {
      List<Category> categoryList = new List<Category>();
      using (SQLiteCommand command = _conn.CreateCommand())
      {
        command.CommandText = "\r\nSELECT CategoryID, ParentID, OrderID, Name, FullName, IsEnable, Folder, LastUpdateTopics, LastUpdateStatus, CountSeeders, TorrentClientUID, Label FROM Category WHERE IsEnable = 1 ORDER BY FullName";
        using (SQLiteDataReader sqLiteDataReader = command.ExecuteReader())
        {
          while (sqLiteDataReader.Read())
          {
            Category category = new Category()
            {
              CategoryID = sqLiteDataReader.GetInt32(0),
              ParentID = sqLiteDataReader.GetInt32(1),
              OrderID = sqLiteDataReader.GetInt32(2),
              Name = sqLiteDataReader.GetString(3),
              FullName = sqLiteDataReader.GetString(4),
              IsEnable = sqLiteDataReader.GetBoolean(5),
              CountSeeders = sqLiteDataReader.IsDBNull(9) ? 2 : sqLiteDataReader.GetInt32(9),
              TorrentClientUID = sqLiteDataReader.IsDBNull(10) ? Guid.Empty : Guid.Parse(sqLiteDataReader.GetString(10)),
              LastUpdateTopics = sqLiteDataReader.GetDateTime(7),
              LastUpdateStatus = sqLiteDataReader.GetDateTime(8),
              Label = sqLiteDataReader.IsDBNull(11) ? string.Empty : sqLiteDataReader.GetString(11)
            };
            string str = sqLiteDataReader.IsDBNull(6) ? null : sqLiteDataReader.GetString(6);
            if (!string.IsNullOrWhiteSpace(str))
            {
              string[] strArray = str.Split('|');
              if (strArray.Length >= 1)
                category.Folder = strArray[0];
              if (strArray.Length >= 2)
                category.CreateSubFolder = int.Parse(strArray[1]);
              if (strArray.Length >= 3)
              {
                category.IsSaveTorrentFiles = strArray[2] == "1";
                category.FolderTorrentFile = Path.Combine(category.Folder, "!!!Torrent-files!!!");
              }
              if (strArray.Length >= 4)
              {
                category.IsSaveWebPage = strArray[3] == "1";
                category.FolderSavePageForum = Path.Combine(category.Folder, "!!!Web-pages!!!");
              }
            }
            categoryList.Add(category);
          }
        }
      }
            if (withUnknown)
            {
                Category unknown = new Category();
                unknown.FullName = "Неизвестные";
                unknown.CategoryID = -1;
                categoryList.Add(unknown);
            }

            return categoryList;
    }

    public void ResetFlagsTopicDownloads()
    {
      using (SQLiteCommand command = _conn.CreateCommand())
      {
        command.CommandText = "UPDATE Topic SET IsKeep = 0, IsDownload = 0";
        command.ExecuteNonQuery();
      }
    }

    public void SaveTopicInfo(List<TopicInfo> data, bool isUpdateTopic = false)
    {
      DateTime dateTime = new DateTime(2000, 1, 1);
      using (SQLiteTransaction sqLiteTransaction = _conn.BeginTransaction())
      {
        using (SQLiteCommand command = _conn.CreateCommand())
        {
          command.Transaction = sqLiteTransaction;
          if (isUpdateTopic)
          {
            command.CommandText = "UPDATE Category SET LastUpdateTopics = @LastUpdateTopics WHERE CategoryID = @CategoryID";
            foreach (int num in data.Select(x => x.CategoryID).Distinct())
            {
              command.Parameters.Clear();
              command.Parameters.AddWithValue("@CategoryID", num);
              command.Parameters.AddWithValue("@LastUpdateTopics", DateTime.Now);
              command.ExecuteNonQuery();
            }
          }
          command.CommandText = string.Format("SELECT TopicID FROM Topic WHERE TopicID IN ({0})", string.Join(",", data.Select(x => x.TopicID)));
          List<int> list = new List<int>();
          using (SQLiteDataReader sqLiteDataReader = command.ExecuteReader())
          {
            while (sqLiteDataReader.Read())
              list.Add(sqLiteDataReader.GetInt32(0));
          }
          if (isUpdateTopic)
          {
            command.CommandText = "UPDATE Topic SET CategoryID = @CategoryID, Name = @Name, Hash = @Hash, Size = @Size, Seeders = @Seeders, Status = @Status, IsDeleted = @IsDeleted, RegTime = @RegTime, PosterID = @PosterID WHERE TopicID = @TopicID;";
            List<TopicInfo> source = data;
            foreach (TopicInfo topicInfo in source.Where(x => list.Contains(x.TopicID)))
            {
              command.Parameters.Clear();
              command.Parameters.AddWithValue("@TopicID", topicInfo.TopicID);
              command.Parameters.AddWithValue("@CategoryID", topicInfo.CategoryID);
              command.Parameters.AddWithValue("@Name", topicInfo.Name2);
              command.Parameters.AddWithValue("@Hash", topicInfo.Hash);
              command.Parameters.AddWithValue("@Size", topicInfo.Size);
              command.Parameters.AddWithValue("@Seeders", topicInfo.Seeders);
              command.Parameters.AddWithValue("@Status", topicInfo.Status);
              command.Parameters.AddWithValue("@IsDeleted", 0);
              command.Parameters.AddWithValue("@RegTime", topicInfo.RegTime < dateTime ? dateTime : topicInfo.RegTime);
              command.Parameters.AddWithValue("@PosterID", topicInfo.PosterID);
              command.ExecuteNonQuery();
            }
          }
          else
          {
            command.CommandText = "UPDATE Topic SET CategoryID = @CategoryID, Name = @Name, Hash = @Hash, Size = @Size, Seeders = @Seeders, Status = @Status, IsDeleted = @IsDeleted, IsKeep = @IsKeep, IsKeepers = @IsKeepers, IsBlackList = @IsBlackList, IsDownload = @IsDownload WHERE TopicID = @TopicID;";
            List<TopicInfo> source = data;
            foreach (TopicInfo topicInfo in source.Where(x => list.Contains(x.TopicID)))
            {
              command.Parameters.Clear();
              command.Parameters.AddWithValue("@TopicID", topicInfo.TopicID);
              command.Parameters.AddWithValue("@CategoryID", topicInfo.CategoryID);
              command.Parameters.AddWithValue("@Name", topicInfo.Name2);
              command.Parameters.AddWithValue("@Hash", topicInfo.Hash);
              command.Parameters.AddWithValue("@Size", topicInfo.Size);
              command.Parameters.AddWithValue("@Seeders", topicInfo.Seeders);
              command.Parameters.AddWithValue("@Status", topicInfo.Status);
              command.Parameters.AddWithValue("@IsDeleted", 0);
              command.Parameters.AddWithValue("@IsKeep", topicInfo.IsKeep);
              command.Parameters.AddWithValue("@IsKeepers", topicInfo.IsKeeper);
              command.Parameters.AddWithValue("@IsBlackList", topicInfo.IsBlackList);
              command.Parameters.AddWithValue("@IsDownload", topicInfo.IsDownload);
              command.ExecuteNonQuery();
            }
          }
          command.CommandText = "\r\nINSERT OR REPLACE INTO Topic (TopicID, CategoryID, Name, Hash, Size, Seeders, Status, IsActive, IsDeleted, IsKeep, IsKeepers, IsBlackList, IsDownload, RegTime, PosterID)\r\nVALUES(@TopicID, @CategoryID, @Name, @Hash, @Size, @Seeders, @Status, @IsActive, @IsDeleted, @IsKeep, @IsKeepers, @IsBlackList, @IsDownload, @RegTime, @PosterID);";
          List<TopicInfo> source1 = data;
          foreach (TopicInfo topicInfo in source1.Where(x => !list.Contains(x.TopicID)))
          {
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@TopicID", topicInfo.TopicID);
            command.Parameters.AddWithValue("@CategoryID", topicInfo.CategoryID);
            command.Parameters.AddWithValue("@Name", topicInfo.Name2);
            command.Parameters.AddWithValue("@Hash", topicInfo.Hash);
            command.Parameters.AddWithValue("@Size", topicInfo.Size);
            command.Parameters.AddWithValue("@Seeders", topicInfo.Seeders);
            command.Parameters.AddWithValue("@Status", topicInfo.Status);
            command.Parameters.AddWithValue("@IsActive", 1);
            command.Parameters.AddWithValue("@IsDeleted", 0);
            command.Parameters.AddWithValue("@IsKeep", topicInfo.IsKeep);
            command.Parameters.AddWithValue("@IsKeepers", topicInfo.IsKeeper);
            command.Parameters.AddWithValue("@IsBlackList", topicInfo.IsBlackList);
            command.Parameters.AddWithValue("@IsDownload", topicInfo.IsDownload);
            command.Parameters.AddWithValue("@RegTime", topicInfo.RegTime < dateTime ? dateTime : topicInfo.RegTime);
            command.Parameters.AddWithValue("@PosterID", topicInfo.PosterID);
            command.ExecuteNonQuery();
          }
        }
        sqLiteTransaction.Commit();
      }
      SaveStatus(data.Select(x => new int[2]
      {
        x.TopicID,
        x.Seeders
      }).ToArray(), true);
    }

    internal void DeleteTopicsByCategoryId(int categoryID)
    {
      using (SQLiteTransaction sqLiteTransaction = _conn.BeginTransaction())
      {
        using (SQLiteCommand command = _conn.CreateCommand())
        {
          command.Transaction = sqLiteTransaction;
          command.Parameters.AddWithValue("@categoryID", categoryID);
          command.CommandText = "UPDATE Topic SET IsDeleted = 1 WHERE CategoryID = @categoryID;";
          command.ExecuteNonQuery();
        }
        sqLiteTransaction.Commit();
      }
    }

    public void ClearHistoryStatus()
    {
      using (SQLiteTransaction sqLiteTransaction = _conn.BeginTransaction())
      {
        using (SQLiteCommand command = _conn.CreateCommand())
        {
          command.Transaction = sqLiteTransaction;
          command.Parameters.Add("@Date", DbType.DateTime);
          command.Parameters[0].Value = DateTime.Now.Date.AddDays(-Settings.Current.CountDaysKeepHistory);
          command.CommandText = "DELETE FROM TopicStatusHystory WHERE Date <= @Date;";
          command.ExecuteNonQuery();
        }
        sqLiteTransaction.Commit();
      }
    }

    public void SaveStatus(int[][] data, bool isUpdateStatus = false)
    {
      using (SQLiteTransaction sqLiteTransaction = _conn.BeginTransaction())
      {
        using (SQLiteCommand command = _conn.CreateCommand())
        {
          command.Transaction = sqLiteTransaction;
          command.CommandText = "\r\nUPDATE Topic SET Seeders = @Seeders WHERE TopicID = @TopicID;\r\nINSERT OR REPLACE INTO TopicStatusHystory VALUES(@TopicID, @Date, @Seeders);\r\n";
          if (Settings.Current.IsNotSaveStatistics)
            command.CommandText = "UPDATE Topic SET Seeders = @Seeders WHERE TopicID = @TopicID;";
          command.Parameters.Clear();
          command.Parameters.Add("@Date", DbType.DateTime);
          command.Parameters.Add("@TopicID", DbType.Int32);
          command.Parameters.Add("@Seeders", DbType.Int32);
          command.Parameters[0].Value = DateTime.Now;
          foreach (int[] numArray in data)
          {
            command.Parameters[1].Value = numArray[0];
            command.Parameters[2].Value = numArray[1];
            command.ExecuteNonQuery();
          }
        }
        sqLiteTransaction.Commit();
      }
    }

    public List<TopicInfo> GetTopicsByCategory(int categoyid)
    {
      DateTime dateTime = new DateTime(2000, 1, 1);
      List<TopicInfo> topicInfoList = new List<TopicInfo>();
      using (SQLiteCommand command = _conn.CreateCommand())
      {
        command.CommandText = "SELECT TopicID, CategoryID, Name, Hash, Size, Seeders, Status, IsActive, IsDeleted, IsKeep, IsKeepers, IsBlackList, IsDownload, AvgSeeders, RegTime, PosterID\r\nFROM Topic WHERE (CategoryID = @CategoryID OR @CategoryID = -1) AND IsDeleted = 0 AND Status NOT IN (7,4,11,5) and Hash IS NOT NULL";
        command.Parameters.AddWithValue("@CategoryID", categoyid);
        using (SQLiteDataReader sqLiteDataReader = command.ExecuteReader())
        {
          while (sqLiteDataReader.Read())
            topicInfoList.Add(new TopicInfo()
            {
              TopicID = sqLiteDataReader.GetInt32(0),
              CategoryID = sqLiteDataReader.GetInt32(1),
              Name2 = sqLiteDataReader.GetString(2),
              Hash = sqLiteDataReader.GetString(3),
              Size = sqLiteDataReader.GetInt64(4),
              Seeders = sqLiteDataReader.GetInt32(5),
              Status = sqLiteDataReader.GetInt32(6),
              IsKeep = sqLiteDataReader.GetBoolean(9),
              IsKeeper = sqLiteDataReader.GetBoolean(10),
              IsBlackList = sqLiteDataReader.GetBoolean(11),
              IsDownload = sqLiteDataReader.GetBoolean(12),
              AvgSeeders = sqLiteDataReader.IsDBNull(13) ? new Decimal?() : new Decimal?(sqLiteDataReader.GetDecimal(13)),
              RegTime = sqLiteDataReader.IsDBNull(14) ? dateTime : sqLiteDataReader.GetDateTime(14),
              PosterID = sqLiteDataReader.IsDBNull(15) ? 0 : sqLiteDataReader.GetInt32(15)
            });
        }
      }
      return topicInfoList;
    }

    public List<TopicInfo> GetTopicsAllByCategory(int categoyid)
    {
      DateTime dateTime = new DateTime(2000, 1, 1);
      List<TopicInfo> topicInfoList = new List<TopicInfo>();
      using (SQLiteCommand command = _conn.CreateCommand())
      {
        command.CommandText = "SELECT TopicID, CategoryID, Name, Hash, Size, Seeders, Status, IsActive, IsDeleted, IsKeep, IsKeepers, IsBlackList, IsDownload, AvgSeeders, RegTime, PosterID\r\nFROM Topic WHERE (CategoryID = @CategoryID OR @CategoryID = -1) and Hash is null";
        command.Parameters.AddWithValue("@CategoryID", categoyid);
        using (SQLiteDataReader sqLiteDataReader = command.ExecuteReader())
        {
          while (sqLiteDataReader.Read())
            topicInfoList.Add(new TopicInfo()
            {
              TopicID = sqLiteDataReader.GetInt32(0),
              CategoryID = sqLiteDataReader.GetInt32(1),
              Name2 = sqLiteDataReader.GetString(2),
              Hash = sqLiteDataReader.GetString(3),
              Size = sqLiteDataReader.GetInt64(4),
              Seeders = sqLiteDataReader.GetInt32(5),
              Status = sqLiteDataReader.GetInt32(6),
              IsKeep = sqLiteDataReader.GetBoolean(9),
              IsKeeper = sqLiteDataReader.GetBoolean(10),
              IsBlackList = sqLiteDataReader.GetBoolean(11),
              IsDownload = sqLiteDataReader.GetBoolean(12),
              AvgSeeders = sqLiteDataReader.IsDBNull(13) ? new Decimal?() : new Decimal?(sqLiteDataReader.GetDecimal(13)),
              RegTime = sqLiteDataReader.IsDBNull(14) ? dateTime : sqLiteDataReader.GetDateTime(14),
              PosterID = sqLiteDataReader.IsDBNull(15) ? 0 : sqLiteDataReader.GetInt32(15)
            });
        }
      }
      return topicInfoList;
    }

    public List<TopicInfo> GetTopics(DateTime regTime, int categoyid, int? countSeeders, int? avgCountSeeders, bool? isKeep, bool? isKeepers, bool? isDownload, bool? isBlack, bool? isPoster)
    {
      DateTime dateTime = new DateTime(2000, 1, 1);
      List<TopicInfo> topicInfoList = new List<TopicInfo>();
      using (SQLiteCommand command = _conn.CreateCommand())
      {
        command.CommandText = @"
SELECT t.TopicID, t.CategoryID, t.Name, Hash, Size, Seeders, Status, IsActive, IsDeleted, IsKeep, IsKeepers, IsBlackList, IsDownload, AvgSeeders, RegTime, CAST(CASE WHEN @UserName = u.Name THEN 1 ELSE 0 END AS BIT), 
COUNT(kt.TopicID) AS KeepersCount
FROM Topic AS t    
LEFT JOIN User AS u ON (t.PosterID = u.UserID)
LEFT JOIN KeeperToTopic AS kt ON (kt.TopicID = t.TopicID AND kt.KeeperName <> @UserName)
WHERE 
    t.CategoryID = @CategoryID 
    AND t.RegTime < @RegTime
    AND Status NOT IN (7,4,11,5)
" + (
    countSeeders.HasValue
      ? string.Format(" AND Seeders {1} {0}", countSeeders.Value,
        Settings.Current.IsSelectLessOrEqual ? " <= " : " = ")
      : ""
  )
  + (avgCountSeeders.HasValue
    ? string.Format(" AND AvgSeeders {1} {0}", avgCountSeeders.Value,
      Settings.Current.IsSelectLessOrEqual ? " <= " : " = ")
    : "")
  + (isKeep.HasValue ? string.Format(" AND IsKeep = {0}", isKeep.Value ? 1 : 0) : "")
  + (isKeepers.HasValue
    ? string.Format(" AND CAST(CASE WHEN kt.TopicID IS NOT NULL THEN 1 ELSE 0 END AS BIT) = {0}",
      isKeepers.Value ? 1 : 0)
    : "")
  + (isDownload.HasValue ? string.Format(" AND IsDownload = {0}", isDownload.Value ? 1 : 0) : "")
  + (isPoster.HasValue ? string.Format(" AND @UserName = u.Name", isPoster.Value ? 1 : 0) : "")
  + string.Format(" AND IsBlackList = {0}", !isBlack.HasValue || !isBlack.Value ? 0 : 1)
  + " AND IsDeleted = 0 GROUP BY t.TopicID HAVING t.TopicID IS NOT NULL ORDER BY t.Seeders, t.Name";
        command.Parameters.AddWithValue("@CategoryID", categoyid);
        command.Parameters.AddWithValue("@RegTime", regTime);
        command.Parameters.AddWithValue("@UserName", string.IsNullOrWhiteSpace(Settings.Current.KeeperName) ? "-" : Settings.Current.KeeperName);
        using (SQLiteDataReader sqLiteDataReader = command.ExecuteReader())
        {
          while (sqLiteDataReader.Read())
            topicInfoList.Add(new TopicInfo()
            {
              TopicID = sqLiteDataReader.GetInt32(0),
              CategoryID = sqLiteDataReader.GetInt32(1),
              Name2 = sqLiteDataReader.IsDBNull(2) ? string.Empty : sqLiteDataReader.GetString(2),
              Hash = sqLiteDataReader.IsDBNull(3) ? string.Empty : sqLiteDataReader.GetString(3),
              Size = sqLiteDataReader.GetInt64(4),
              Seeders = sqLiteDataReader.GetInt32(5),
              Status = sqLiteDataReader.GetInt32(6),
              IsKeep = sqLiteDataReader.GetBoolean(9),
              KeeperCount = sqLiteDataReader.GetInt32(16),
              IsBlackList = sqLiteDataReader.GetBoolean(11),
              IsDownload = sqLiteDataReader.GetBoolean(12),
              AvgSeeders = sqLiteDataReader.IsDBNull(13) ? new Decimal?() : new Decimal?(Math.Round(sqLiteDataReader.GetDecimal(13), 3)),
              RegTime = sqLiteDataReader.IsDBNull(14) ? dateTime : sqLiteDataReader.GetDateTime(14),
              IsPoster = sqLiteDataReader.GetBoolean(15)
            });
        }
      }
      return topicInfoList;
    }

    public void SetTorrentClientHash(List<TopicInfo> data)
    {
      using (SQLiteTransaction sqLiteTransaction = _conn.BeginTransaction())
      {
        using (SQLiteCommand command = _conn.CreateCommand())
        {
          command.Transaction = sqLiteTransaction;
          command.CommandText = "UPDATE Topic SET IsDownload = @IsDownload, IsKeep = @IsKeep WHERE Hash = @Hash;";
          foreach (TopicInfo topicInfo in data)
          {
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@Hash", topicInfo.Hash);
            command.Parameters.AddWithValue("@IsDownload", topicInfo.IsDownload);
            command.Parameters.AddWithValue("@IsKeep", topicInfo.IsKeep);
            command.ExecuteNonQuery();
          }
        }
        sqLiteTransaction.Commit();
      }
    }

    public void SaveTorrentClients(IEnumerable<TorrentClientInfo> data, bool isUpdateList = false)
    {
      List<TorrentClientInfo> tc = GetTorrentClients();
      using (SQLiteTransaction sqLiteTransaction = _conn.BeginTransaction())
      {
        using (SQLiteCommand command = _conn.CreateCommand())
        {
          command.Transaction = sqLiteTransaction;
          if (isUpdateList)
          {
            command.CommandText = "DELETE FROM TorrentClient WHERE UID = @UID";
            List<TorrentClientInfo> source = tc;
            foreach (TorrentClientInfo torrentClientInfo in source.Where(x => !data.Select(y => y.UID).Contains(x.UID)))
            {
              command.Parameters.Clear();
              command.Parameters.AddWithValue("@UID", torrentClientInfo.UID.ToString());
              command.ExecuteNonQuery();
            }
          }
          command.CommandText = "UPDATE TorrentClient SET Name = @Name, Type = @Type, ServerName = @ServerName, ServerPort = @ServerPort, UserName = @UserName, UserPassword = @UserPassword, LastReadHash = @LastReadHash WHERE UID = @UID";
          IEnumerable<TorrentClientInfo> source1 = data;
          foreach (TorrentClientInfo torrentClientInfo in source1.Where(x => tc.Select(y => y.UID).Contains(x.UID)))
          {
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@UID", torrentClientInfo.UID.ToString());
            command.Parameters.AddWithValue("@Name", torrentClientInfo.Name);
            command.Parameters.AddWithValue("@Type", torrentClientInfo.Type);
            command.Parameters.AddWithValue("@ServerName", torrentClientInfo.ServerName);
            command.Parameters.AddWithValue("@ServerPort", torrentClientInfo.ServerPort);
            command.Parameters.AddWithValue("@UserName", torrentClientInfo.UserName);
            command.Parameters.AddWithValue("@UserPassword", torrentClientInfo.UserPassword);
            command.Parameters.AddWithValue("@LastReadHash", torrentClientInfo.LastReadHash);
            command.ExecuteNonQuery();
          }
          command.CommandText = "INSERT INTO TorrentClient (UID, Name, Type, ServerName, ServerPort, UserName, UserPassword, LastReadHash) VALUES(@UID, @Name, @Type, @ServerName, @ServerPort, @UserName, @UserPassword, @LastReadHash)";
          IEnumerable<TorrentClientInfo> source2 = data;
          foreach (TorrentClientInfo torrentClientInfo in source2.Where(x => !tc.Select(y => y.UID).Contains(x.UID)))
          {
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@UID", torrentClientInfo.UID.ToString());
            command.Parameters.AddWithValue("@Name", torrentClientInfo.Name);
            command.Parameters.AddWithValue("@Type", torrentClientInfo.Type);
            command.Parameters.AddWithValue("@ServerName", torrentClientInfo.ServerName);
            command.Parameters.AddWithValue("@ServerPort", torrentClientInfo.ServerPort);
            command.Parameters.AddWithValue("@UserName", torrentClientInfo.UserName);
            command.Parameters.AddWithValue("@UserPassword", torrentClientInfo.UserPassword);
            command.Parameters.AddWithValue("@LastReadHash", torrentClientInfo.LastReadHash);
            command.ExecuteNonQuery();
          }
        }
        sqLiteTransaction.Commit();
      }
    }

    public List<TorrentClientInfo> GetTorrentClients()
    {
      List<TorrentClientInfo> torrentClientInfoList = new List<TorrentClientInfo>();
      using (SQLiteCommand command = _conn.CreateCommand())
      {
        command.CommandText = "SELECT * FROM TorrentClient";
        using (SQLiteDataReader sqLiteDataReader = command.ExecuteReader())
        {
          while (sqLiteDataReader.Read())
            torrentClientInfoList.Add(new TorrentClientInfo()
            {
              UID = Guid.Parse(sqLiteDataReader.GetString(0)),
              Name = sqLiteDataReader.GetString(1),
              Type = sqLiteDataReader.GetString(2),
              ServerName = sqLiteDataReader.GetString(3),
              ServerPort = sqLiteDataReader.GetInt32(4),
              UserName = sqLiteDataReader.GetString(5),
              UserPassword = sqLiteDataReader.GetString(6),
              LastReadHash = sqLiteDataReader.GetDateTime(7)
            });
        }
      }
      return torrentClientInfoList;
    }

    public void SaveKeepOtherKeepers(Dictionary<string, Tuple<int, List<int>>> data)
    {
      if (data == null)
        return;
      using (SQLiteTransaction sqLiteTransaction = _conn.BeginTransaction())
      {
        using (SQLiteCommand command = _conn.CreateCommand())
        {
          command.Transaction = sqLiteTransaction;
          command.CommandTimeout = 60000;
          foreach (KeyValuePair<string, Tuple<int, List<int>>> keyValuePair in data)
          {
            KeyValuePair<string, Tuple<int, List<int>>> dt = keyValuePair;
            int[] array = dt.Value.Item2.Distinct().ToArray();
            List<int>[] intListArray = new List<int>[array.Length % 500 == 0 ? array.Length / 500 : array.Length / 500 + 1];
            for (int index1 = 0; index1 < array.Length; ++index1)
            {
              int index2 = index1 / 500;
              if (intListArray[index2] == null)
                intListArray[index2] = new List<int>();
              intListArray[index2].Add(array[index1]);
            }
            foreach (List<int> source in intListArray)
            {
              command.Parameters.Clear();
              command.CommandText = "INSERT OR REPLACE INTO KeeperToTopic(KeeperName, CategoryID, TopicID)\r\n" + string.Join("UNION ", source.Select(x => string.Format("SELECT @KeeperName, {2}, {1}\r\n", dt.Key, x, dt.Value.Item1)));
              command.Parameters.AddWithValue("@KeeperName", dt.Key.Replace("<wbr>", "").Trim());
              command.ExecuteNonQuery();
            }
          }
        }
        sqLiteTransaction.Commit();
      }
    }

    private void SaveKeepStatus(string keepName, List<Tuple<int, int, Decimal>> data)
    {
      using (SQLiteTransaction sqLiteTransaction = _conn.BeginTransaction())
      {
        using (SQLiteCommand command = _conn.CreateCommand())
        {
          command.Transaction = sqLiteTransaction;
          command.CommandText = "INSERT OR REPLACE INTO Keeper VALUES(@KeeperName, @CategoryID, @Count, @Size)";
          command.Parameters.Add("@KeeperName", DbType.String);
          command.Parameters.Add("@CategoryID", DbType.Int32);
          command.Parameters.Add("@Count", DbType.Int64);
          command.Parameters.Add("@Size", DbType.Decimal);
          if (string.IsNullOrWhiteSpace(keepName))
            keepName = Settings.Current.KeeperName;
          if (string.IsNullOrWhiteSpace(keepName))
            return;
          foreach (Tuple<int, int, Decimal> tuple in data)
          {
            command.Parameters[0].Value = keepName.Replace("<wbr>", "").Trim();
            command.Parameters[1].Value = tuple.Item1;
            command.Parameters[2].Value = tuple.Item2;
            command.Parameters[3].Value = Math.Round(tuple.Item3, 2);
            command.ExecuteNonQuery();
          }
        }
        sqLiteTransaction.Commit();
      }
    }

    public void ClearReports()
    {
      using (SQLiteTransaction sqLiteTransaction = _conn.BeginTransaction())
      {
        using (SQLiteCommand command = _conn.CreateCommand())
        {
          command.Transaction = sqLiteTransaction;
          command.CommandText = "UPDATE Report SET Report = @Report WHERE ReportNo <> 0";
          command.Parameters.AddWithValue("@Report", "Удалено");
          command.ExecuteNonQuery();
        }
        sqLiteTransaction.Commit();
      }
    }

    public List<Tuple<int, string, int, Decimal>> GetStatisticsByAllUsers()
    {
      List<Tuple<int, string, int, Decimal>> tupleList = new List<Tuple<int, string, int, Decimal>>();
      bool flag = GetTorrentClients().Any();
      using (SQLiteCommand command = _conn.CreateCommand())
      {
        command.CommandText = "INSERT OR REPLACE INTO Keeper SELECT 'All', CategoryID, COUNT(*) Cnt, SUM(Size) / 1073741824.0 Size FROM Topic WHERE IsDeleted = 0 AND CategoryID <> 0 GROUP BY CategoryID;\r\nINSERT OR REPLACE INTO Keeper SELECT kt.KeeperName, kt.CategoryID, COUNT(*),  CAST(SUM(t.Size) / 1073741824.0  AS NUMERIC(18,4)) Size \r\n    FROM KeeperToTopic AS kt JOIN Topic AS t ON (kt.TopicID = t.TopicID AND kt.KeeperName <> @KeeperName) group by kt.KeeperName, kt.CategoryID;\r\nINSERT OR REPLACE INTO Keeper SELECT @KeeperName, CategoryID,  COUNT(*) Cnt, CAST(SUM(Size) / 1073741824.0 AS NUMERIC(18,4)) Size FROM Topic \r\n        WHERE IsDeleted = 0 AND IsKeep = 1 AND (Seeders <= @Seeders OR @Seeders = -1) AND Status NOT IN (7, 4,11,5) AND IsBlackList = 0 GROUP BY CategoryID;\r\n";
        command.Parameters.AddWithValue("@KeeperName", flag ? Settings.Current.KeeperName : "<no>");
        command.Parameters.AddWithValue("@Seeders", Settings.Current.CountSeedersReport);
        command.ExecuteNonQuery();
        command.CommandText = "SELECT KeeperName, CategoryID, Count, Size FROM Keeper";
        using (SQLiteDataReader sqLiteDataReader = command.ExecuteReader())
        {
          while (sqLiteDataReader.Read())
            tupleList.Add(new Tuple<int, string, int, Decimal>(sqLiteDataReader.GetInt32(1), sqLiteDataReader.GetString(0), sqLiteDataReader.GetInt32(2), Math.Round(sqLiteDataReader.GetDecimal(3), 3)));
        }
      }
      return tupleList;
    }

    public void SaveReports(Dictionary<int, Dictionary<int, string>> reports)
    {
      foreach (KeyValuePair<int, Dictionary<int, string>> report in reports)
      {
        int num = report.Value.Keys.Max(x => x);
        report.Value.Add(num + 1, "Резерв");
        report.Value.Add(num + 2, "Резерв");
      }
      using (SQLiteTransaction sqLiteTransaction = _conn.BeginTransaction())
      {
        using (SQLiteCommand command = _conn.CreateCommand())
        {
          command.Transaction = sqLiteTransaction;
          Dictionary<Tuple<int, int>, Tuple<string, string>> reps = GetReports(new int?());
          if (reports.Any(x => !x.Value.ContainsKey(0)))
          {
            command.CommandText = "UPDATE Report SET Report = @Report WHERE CategoryID = @CategoryID AND ReportNo <> 0";
            foreach (KeyValuePair<int, Dictionary<int, string>> report in reports)
            {
              command.Parameters.Clear();
              command.Parameters.AddWithValue("@CategoryID", report.Key);
              command.Parameters.AddWithValue("@Report", "Резерв");
              command.ExecuteNonQuery();
            }
          }
          command.CommandText = "UPDATE Report SET Report = @Report WHERE CategoryID = @CategoryID AND ReportNo = @ReportNo";
          foreach (KeyValuePair<int, Dictionary<int, string>> report in reports)
          {
            KeyValuePair<int, Dictionary<int, string>> r1 = report;
            Dictionary<int, string> source = r1.Value;
            foreach (KeyValuePair<int, string> keyValuePair in source.Where(x => reps.ContainsKey(new Tuple<int, int>(r1.Key, x.Key))))
            {
              command.Parameters.Clear();
              command.Parameters.AddWithValue("@CategoryID", r1.Key);
              command.Parameters.AddWithValue("@ReportNo", keyValuePair.Key);
              command.Parameters.AddWithValue("@Report", keyValuePair.Value);
              command.ExecuteNonQuery();
            }
          }
          command.CommandText = "INSERT OR REPLACE INTO Report VALUES(@CategoryID, @ReportNo, @URL, @Report)";
          foreach (KeyValuePair<int, Dictionary<int, string>> report in reports)
          {
            KeyValuePair<int, Dictionary<int, string>> r1 = report;
            Dictionary<int, string> source = r1.Value;
            foreach (KeyValuePair<int, string> keyValuePair in source.Where(x => !reps.ContainsKey(new Tuple<int, int>(r1.Key, x.Key))))
            {
              command.Parameters.Clear();
              command.Parameters.AddWithValue("@CategoryID", r1.Key);
              command.Parameters.AddWithValue("@ReportNo", keyValuePair.Key);
              command.Parameters.AddWithValue("@URL", string.Empty);
              command.Parameters.AddWithValue("@Report", keyValuePair.Value);
              command.ExecuteNonQuery();
            }
          }
        }
        sqLiteTransaction.Commit();
      }
    }

    public Dictionary<Tuple<int, int>, Tuple<string, string>> GetReports(int? categoryID = null)
    {
      Dictionary<Tuple<int, int>, Tuple<string, string>> dictionary = new Dictionary<Tuple<int, int>, Tuple<string, string>>();
      using (SQLiteCommand command = _conn.CreateCommand())
      {
        command.CommandText = "SELECT * FROM Report";
        if (categoryID.HasValue)
        {
          command.CommandText += " WHERE CategoryID = @CategoryID";
          command.Parameters.AddWithValue("@CategoryID", categoryID.Value);
        }
        using (SQLiteDataReader sqLiteDataReader = command.ExecuteReader())
        {
          while (sqLiteDataReader.Read())
          {
            if (!dictionary.ContainsKey(new Tuple<int, int>(sqLiteDataReader.GetInt32(0), sqLiteDataReader.GetInt32(1))))
              dictionary.Add(new Tuple<int, int>(sqLiteDataReader.GetInt32(0), sqLiteDataReader.GetInt32(1)), new Tuple<string, string>(sqLiteDataReader.GetString(2), sqLiteDataReader.GetString(3)));
          }
        }
      }
      return dictionary;
    }

    public void SaveSettingsReport(List<Tuple<int, int, string>> result)
    {
      using (SQLiteTransaction sqLiteTransaction = _conn.BeginTransaction())
      {
        using (SQLiteCommand command = _conn.CreateCommand())
        {
          command.Transaction = sqLiteTransaction;
          command.CommandText = "SELECT DISTINCT CategoryID FROM Report WHERE ReportNo = 0";
          HashSet<int> filter = new HashSet<int>();
          using (SQLiteDataReader sqLiteDataReader = command.ExecuteReader())
          {
            while (sqLiteDataReader.Read())
              filter.Add(sqLiteDataReader.GetInt32(0));
          }
          command.CommandText = "UPDATE Report SET URL = @url WHERE CategoryID = @CategoryID AND ReportNo = @ReportNo";
          command.Parameters.Add("@CategoryID", DbType.Int32);
          command.Parameters.Add("@ReportNo", DbType.Int32);
          command.Parameters.Add("@url", DbType.String);
          command.Prepare();
          foreach (Tuple<int, int, string> tuple in result)
          {
            command.Parameters["@CategoryID"].Value = tuple.Item1;
            command.Parameters["@ReportNo"].Value = tuple.Item2;
            command.Parameters["@url"].Value = tuple.Item3;
            command.ExecuteNonQuery();
          }
          command.CommandText = "INSERT OR REPLACE INTO Report VALUES(@CategoryID, @ReportNo, @URL, '')";
          command.Prepare();
          List<Tuple<int, int, string>> source = result;
          foreach (Tuple<int, int, string> tuple in source.Where(x => !filter.Contains(x.Item1)))
          {
            command.Parameters["@CategoryID"].Value = tuple.Item1;
            command.Parameters["@ReportNo"].Value = tuple.Item2;
            command.Parameters["@url"].Value = tuple.Item3;
            command.ExecuteNonQuery();
          }
        }
        sqLiteTransaction.Commit();
      }
    }

    public void UpdateStatistics()
    {
      using (SQLiteTransaction sqLiteTransaction = _conn.BeginTransaction())
      {
        using (SQLiteCommand command = _conn.CreateCommand())
        {
          command.Transaction = sqLiteTransaction;
          List<Decimal[]> numArrayList = new List<Decimal[]>();
          command.CommandText = "update Topic SET AvgSeeders = (SELECT AVG(Seeders) FROM TopicStatusHystory AS st WHERE st.TopicID = Topic.TopicID)";
          command.ExecuteNonQuery();
          command.CommandText = "UPDATE Topic SET AvgSeeders = @Seeders WHERE TopicID = @TopicID";
          foreach (Decimal[] numArray in numArrayList)
          {
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@TopicID", numArray[0]);
            command.Parameters.AddWithValue("@Seeders", numArray[1]);
            command.ExecuteNonQuery();
          }
        }
        sqLiteTransaction.Commit();
      }
    }

    public void ClearKeepers()
    {
      using (SQLiteCommand command = _conn.CreateCommand())
      {
        command.CommandText = "DELETE FROM Keeper;\r\nDELETE FROM KeeperToTopic;\r\nUPDATE Report SET Report = '' WHERE ReportNo = 0";
        command.ExecuteNonQuery();
      }
    }
  }
}
