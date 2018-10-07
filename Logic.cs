// Decompiled with JetBrains decompiler
// Type: TLO.local.Logic
// Assembly: TLO.local, Version=2.6.5944.27906, Culture=neutral, PublicKeyToken=null
// MVID: E76CFDB0-1920-4151-9DD8-5FF51DE7CC23
// Assembly location: C:\Users\root\Downloads\TLO_2.6.2.21\TLO.local.exe

using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using TLO.local.Forms;

namespace TLO.local
{
  internal class Logic
  {
    private static Logger logger = LogManager.GetCurrentClassLogger();
    private static RuTrackerOrg _Current;

    public static RuTrackerOrg Current
    {
      get
      {
        if (Logic._Current == null)
          Logic._Current = new RuTrackerOrg(Settings.Current.KeeperName, Settings.Current.KeeperPass);
        return Logic._Current;
      }
    }

    public static void SendTorrentFileToTorrentClient(List<TopicInfo> topics, Category category)
    {
      TorrentClientInfo torrentClientInfo = ClientLocalDB.Current.GetTorrentClients().Where<TorrentClientInfo>((Func<TorrentClientInfo, bool>) (x => x.UID == category.TorrentClientUID)).FirstOrDefault<TorrentClientInfo>();
      if (torrentClientInfo == null)
        return;
      ITorrentClient torrentClient1 = torrentClientInfo.Create();
      if (torrentClient1 == null)
        return;
      if (string.IsNullOrWhiteSpace(category.Folder))
        throw new Exception("В разделе не указан каталог для загрузки");
      foreach (TopicInfo topic in topics)
      {
        if (topic.Status != 7 && topic.Status != 4)
        {
          int topicId;
          if (category.CreateSubFolder != 0)
          {
            if (category.CreateSubFolder != 1)
              throw new Exception("Не поддерживается указаный метод создания подкаталога");
            ITorrentClient torrentClient2 = torrentClient1;
            string folder = category.Folder;
            topicId = topic.TopicID;
            string path2 = topicId.ToString();
            string dir = Path.Combine(folder, path2);
            torrentClient2.SetDefaultFolder(dir);
          }
          byte[] buffer1 = new byte[0];
          if (buffer1.Length == 0)
            buffer1 = Logic.Current.DownloadTorrentFile(topic.TopicID);
          if (buffer1 == null)
            break;
          ITorrentClient torrentClient3 = torrentClient1;
          string path;
          if (category.CreateSubFolder != 1)
          {
            path = category.Folder;
          }
          else
          {
            string folder = category.Folder;
            topicId = topic.TopicID;
            string path2 = topicId.ToString();
            path = Path.Combine(folder, path2);
          }
          string filename = string.Format("[rutracker.org].t{0}.torrent", (object) topic.TopicID);
          byte[] fdata = buffer1;
          torrentClient3.SendTorrentFile(path, filename, fdata);
          if (category.IsSaveTorrentFiles)
          {
            if (!Directory.Exists(category.FolderTorrentFile))
              Directory.CreateDirectory(category.FolderTorrentFile);
            using (FileStream fileStream = File.Create(Path.Combine(category.FolderTorrentFile, string.Format("[rutracker.org].t{0}.torrent", (object) topic.TopicID))))
              fileStream.Write(buffer1, 0, ((IEnumerable<byte>) buffer1).Count<byte>());
          }
          if (category.IsSaveWebPage)
          {
            Thread.Sleep(500);
            byte[] buffer2 = Logic.Current.DownloadWebPages(string.Format("https://rutracker.org/forum/viewtopic.php?t={0}", (object) topic.TopicID));
            if (!Directory.Exists(category.FolderSavePageForum))
              Directory.CreateDirectory(category.FolderSavePageForum);
            using (FileStream fileStream = File.Create(Path.Combine(category.FolderSavePageForum, string.Format("[rutracker.org].t{0}.html", (object) topic.TopicID))))
              fileStream.Write(buffer2, 0, ((IEnumerable<byte>) buffer2).Count<byte>());
          }
          Thread.Sleep(500);
        }
      }
    }

    public static void SendTorrentFileToTorrentClient(TopicInfo topic, Category category)
    {
      if (topic == null || category == null)
        return;
      Logic.SendTorrentFileToTorrentClient(new List<TopicInfo>()
      {
        topic
      }, category);
    }

    public static void SendReportToForum()
    {
      foreach (KeyValuePair<Tuple<int, int>, Tuple<string, string>> report in ClientLocalDB.Current.GetReports(new int?()))
      {
        if (!string.IsNullOrWhiteSpace(report.Value.Item1))
          Logic.Current.SendReport(report.Value.Item1, report.Value.Item2);
      }
    }

    public static void SendReportToForum(ProgressBar pBar)
    {
      Dictionary<Tuple<int, int>, Tuple<string, string>> reports = ClientLocalDB.Current.GetReports(new int?());
      pBar.Visible = true;
      pBar.Minimum = 1;
      pBar.Maximum = reports.Count;
      pBar.Value = 1;
      pBar.Step = 1;
      foreach (KeyValuePair<Tuple<int, int>, Tuple<string, string>> keyValuePair in reports)
      {
        if (!string.IsNullOrWhiteSpace(keyValuePair.Value.Item1))
          Logic.Current.SendReport(keyValuePair.Value.Item1, keyValuePair.Value.Item2);
        pBar.PerformStep();
      }
    }

    public static void LoadHashFromClients(List<TorrentClientInfo> clients = null)
    {
      if (clients == null)
        clients = ClientLocalDB.Current.GetTorrentClients();
      if (clients == null)
        return;
      foreach (TorrentClientInfo client in clients)
      {
        try
        {
          ITorrentClient torrentClient = client.Create();
          if (torrentClient != null)
            ClientLocalDB.Current.SetTorrentClientHash(torrentClient.GetAllTorrentHash());
        }
        catch
        {
        }
      }
    }

    public static void LoadHashFromClients(TorrentClientInfo client)
    {
      if (client == null)
        return;
      Logic.LoadHashFromClients(new List<TorrentClientInfo>()
      {
        client
      });
    }

    internal static void LoadHashFromClients(ProgressBar pBar)
    {
      List<TorrentClientInfo> torrentClients = ClientLocalDB.Current.GetTorrentClients();
      pBar.Visible = true;
      pBar.Minimum = 1;
      pBar.Maximum = torrentClients.Count;
      pBar.Value = 1;
      pBar.Step = 1;
      foreach (TorrentClientInfo torrentClientInfo in torrentClients)
      {
        try
        {
          ITorrentClient torrentClient = torrentClientInfo.Create();
          if (torrentClient != null)
            ClientLocalDB.Current.SetTorrentClientHash(torrentClient.GetAllTorrentHash());
        }
        catch
        {
        }
        pBar.PerformStep();
      }
    }

    public static void LoadHashFromClients(Guid uid)
    {
      TorrentClientInfo client = ClientLocalDB.Current.GetTorrentClients().Where<TorrentClientInfo>((Func<TorrentClientInfo, bool>) (x => x.UID == uid)).FirstOrDefault<TorrentClientInfo>();
      if (client == null)
        return;
      Logic.LoadHashFromClients(client);
      ClientLocalDB.Current.CreateReportByRootCategories();
    }

    public static void UpdateSeedersByCategories(List<Category> categories = null)
    {
      if (categories == null)
        categories = ClientLocalDB.Current.GetCategoriesEnable();
      if (categories == null)
        return;
      foreach (Category category in categories)
        ClientLocalDB.Current.SaveStatus(RuTrackerOrg.Current.GetTopicsStatus(category.CategoryID), true);
    }

    public static void UpdateSeedersByCategory(Category category)
    {
      if (category == null)
        return;
      Logic.UpdateSeedersByCategories(new List<Category>()
      {
        category
      });
    }

    public static void UpdateTopicsByCategories(List<Category> categories = null)
    {
      if (categories == null)
        categories = ClientLocalDB.Current.GetCategoriesEnable();
      if (categories == null)
        return;
      foreach (Category category in categories)
        ClientLocalDB.Current.SaveTopicInfo(RuTrackerOrg.Current.GetTopicsInfo(((IEnumerable<int[]>) RuTrackerOrg.Current.GetTopicsStatus(category.CategoryID)).Select<int[], int>((Func<int[], int>) (x => x[0])).Distinct<int>().ToArray<int>()), true);
    }

    public static void UpdateTopicsByCategories(ProgressBar pBar)
    {
      List<Category> categoriesEnable = ClientLocalDB.Current.GetCategoriesEnable();
      pBar.Visible = true;
      pBar.Minimum = 1;
      pBar.Maximum = categoriesEnable.Count;
      pBar.Value = 1;
      pBar.Step = 1;
      foreach (Category category in categoriesEnable)
      {
        ClientLocalDB.Current.SaveTopicInfo(RuTrackerOrg.Current.GetTopicsInfo(((IEnumerable<int[]>) RuTrackerOrg.Current.GetTopicsStatus(category.CategoryID)).Select<int[], int>((Func<int[], int>) (x => x[0])).Distinct<int>().ToArray<int>()), true);
        pBar.PerformStep();
      }
    }

    public static void UpdateTopicsByCategory(Category category)
    {
      if (category == null)
        return;
      Logic.UpdateTopicsByCategories(new List<Category>()
      {
        category
      });
    }

    public static void bwDownloadTorrentFiles(object sender, DoWorkEventArgs e)
    {
      BackgroundWorker backgroundWorker = sender as BackgroundWorker;
      Decimal num1 = new Decimal(0, 0, 0, false, (byte) 1);
      backgroundWorker.ReportProgress((int) num1);
      try
      {
        Tuple<List<TopicInfo>, MainForm> tuple = e.Argument as Tuple<List<TopicInfo>, MainForm>;
        List<TopicInfo> topicInfoList = tuple.Item1;
        string folder = string.Empty;
        if (topicInfoList == null || topicInfoList.Count == 0)
          return;
        tuple.Item2.Invoke((MethodInvoker)delegate
        {
          FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
          if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
            return;
          folder = folderBrowserDialog.SelectedPath;
        });
        if (string.IsNullOrWhiteSpace(folder))
        {
          int num2;
          tuple.Item2.Invoke((MethodInvoker)delegate {
              num2 = (int)MessageBox.Show("Не указан каталог для сохранения торрент-файлов", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
          });
        }
        else
        {
          foreach (TopicInfo topicInfo in topicInfoList)
          {
            byte[] buffer = Logic.Current.DownloadTorrentFile(topicInfo.TopicID);
            if (buffer != null)
            {
              using (FileStream fileStream = File.Create(Path.Combine(folder, string.Format("[rutracker.org].t{0}.torrent", (object) topicInfo.TopicID))))
                fileStream.Write(buffer, 0, ((IEnumerable<byte>) buffer).Count<byte>());
              num1 += new Decimal(1000, 0, 0, false, (byte) 1) / (Decimal) topicInfoList.Count;
              backgroundWorker.ReportProgress((int) num1);
            }
          }
        }
      }
      catch (Exception ex)
      {
        Logic.logger.Error(ex.Message);
        Logic.logger.Debug<Exception>(ex);
        int num2 = (int) MessageBox.Show("Произошла ошибка при скачивании торрент-файлов:\r\n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
      }
    }

    public static void bwSendTorrentFileToTorrentClient(object sender, DoWorkEventArgs e)
    {
      BackgroundWorker backgroundWorker = sender as BackgroundWorker;
      Decimal num1 = new Decimal(0, 0, 0, false, (byte) 1);
      backgroundWorker.ReportProgress((int) num1);
      try
      {
        Tuple<MainForm, List<TopicInfo>, Category> tuple = e.Argument as Tuple<MainForm, List<TopicInfo>, Category>;
        List<TopicInfo> topicInfoList = tuple.Item2;
        Category category = tuple.Item3;
        Logic.logger.Info("Запущена задача на скачивание и добавление торрент-файлов в торрент-клиент...");
        Logic.logger.Trace(string.Format("\tКол-во раздач для скачивания торрент-файлов: {0}", (object) topicInfoList.Count));
        TorrentClientInfo torrentClientInfo = ClientLocalDB.Current.GetTorrentClients().Where<TorrentClientInfo>((Func<TorrentClientInfo, bool>) (x => x.UID == category.TorrentClientUID)).FirstOrDefault<TorrentClientInfo>();
        IEnumerable<TopicInfo> source = torrentClientInfo.Create().GetAllTorrentHash().Where<TopicInfo>((Func<TopicInfo, bool>) (x => !string.IsNullOrWhiteSpace(x.Hash)));
        foreach (TopicInfo topicInfo1 in topicInfoList)
        {
          TopicInfo t = topicInfo1;
          TopicInfo topicInfo2 = source.Where<TopicInfo>((Func<TopicInfo, bool>) (x => x.Hash == t.Hash)).FirstOrDefault<TopicInfo>();
          if (topicInfo2 != null)
            t.TorrentName = topicInfo2.TorrentName;
        }
        List<string> list = source.Select<TopicInfo, string>((Func<TopicInfo, string>) (x => x.Hash)).ToList<string>();
        if (torrentClientInfo == null)
        {
          Logic.logger.Warn("Не указан торрент-клиент в категории/подфоруме");
        }
        else
        {
          string folder = category.Folder;
          if (string.IsNullOrWhiteSpace(folder))
            tuple.Item1.Invoke((MethodInvoker)delegate
            {
              FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
              if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
                return;
              folder = folderBrowserDialog.SelectedPath;
            });
          if (string.IsNullOrWhiteSpace(folder))
            throw new Exception("Не указан каталог для загрузки");
          foreach (TopicInfo topicInfo in topicInfoList)
          {
            TopicInfo t = topicInfo;
            try
            {
              if (t.Status != 7)
              {
                if (t.Status != 4)
                {
                  string folder2 = string.Empty;
                  if (category.CreateSubFolder == 0)
                    folder2 = folder;
                  else if (category.CreateSubFolder == 1)
                  {
                    folder2 = Path.Combine(folder, t.TopicID.ToString());
                  }
                  else
                  {
                    if (category.CreateSubFolder != 2)
                      throw new Exception("Не поддерживается указаный метод создания подкаталога");
                    DialogResult result = DialogResult.None;
                    tuple.Item1.Invoke((MethodInvoker)delegate
                    {
                      FolderNameDialog folderNameDialog = new FolderNameDialog();
                      folderNameDialog.SelectedPath = t.Name;
                      result = folderNameDialog.ShowDialog();
                      folder2 = Path.Combine(folder, folderNameDialog.SelectedPath);
                    });
                    if (result == DialogResult.Abort)
                      return;
                    if (result != DialogResult.Cancel)
                    {
                      if (result != DialogResult.OK)
                        throw new Exception("result != DialogResult.OK");
                    }
                    else
                      continue;
                  }
                  if (!list.Contains(t.Hash))
                  {
                    ITorrentClient torrentClient = torrentClientInfo.Create();
                    if (torrentClient == null)
                      throw new ArgumentException("Не удалось создать подключение к торрент-клиенту \"" + torrentClientInfo.Name + "\"");
                    torrentClient.SetDefaultFolder(folder2);
                    byte[] numArray = Logic.Current.DownloadTorrentFile(t.TopicID);
                    if (numArray == null)
                    {
                      Logic.logger.Warn("Не удалось скачать торрент-файл для раздачи \"" + t.Name + "\". Статус раздачи: " + t.Status.ToString());
                      continue;
                    }
                    torrentClient.SendTorrentFile(folder2, string.Format("[rutracker.org].t{0}.torrent", (object) t.TopicID), numArray);
                    torrentClient.SetLabel(t.Hash, string.IsNullOrWhiteSpace(category.Label) ? category.FullName : category.Label);
                    if (category.IsSaveTorrentFiles)
                    {
                      if (!Directory.Exists(category.FolderTorrentFile))
                        Directory.CreateDirectory(category.FolderTorrentFile);
                      using (FileStream fileStream = File.Create(Path.Combine(category.FolderTorrentFile, string.Format("[rutracker.org].t{0}.torrent", (object) t.TopicID))))
                        fileStream.Write(numArray, 0, ((IEnumerable<byte>) numArray).Count<byte>());
                    }
                  }
                  if (category.IsSaveWebPage)
                  {
                    Thread.Sleep(500);
                    byte[] buffer = Logic.Current.DownloadWebPages(string.Format("https://rutracker.org/forum/viewtopic.php?t={0}", (object) t.TopicID));
                    if (!Directory.Exists(category.FolderSavePageForum))
                      Directory.CreateDirectory(category.FolderSavePageForum);
                    using (FileStream fileStream = File.Create(Path.Combine(category.FolderSavePageForum, string.Format("[rutracker.org].t{0}.html", (object) t.TopicID))))
                      fileStream.Write(buffer, 0, ((IEnumerable<byte>) buffer).Count<byte>());
                  }
                  if (!string.IsNullOrWhiteSpace(t.TorrentName))
                  {
                    try
                    {
                      if (Directory.Exists(Path.Combine(category.Folder, t.TorrentName)))
                      {
                        if (!Directory.Exists(Path.Combine(category.Folder, t.TopicID.ToString())))
                          Directory.CreateDirectory(Path.Combine(category.Folder, t.TopicID.ToString()));
                        Directory.Move(Path.Combine(category.Folder, t.TorrentName), Path.Combine(category.Folder, t.TopicID.ToString(), t.TorrentName));
                        continue;
                      }
                      if (File.Exists(Path.Combine(category.Folder, t.TorrentName)))
                      {
                        if (!Directory.Exists(Path.Combine(category.Folder, t.TopicID.ToString())))
                          Directory.CreateDirectory(Path.Combine(category.Folder, t.TopicID.ToString()));
                        File.Move(Path.Combine(category.Folder, t.TorrentName), Path.Combine(category.Folder, t.TopicID.ToString(), t.TorrentName));
                        continue;
                      }
                    }
                    catch
                    {
                    }
                  }
                }
                else
                  continue;
              }
              else
                continue;
            }
            catch (Exception ex)
            {
              Logic.logger.Warn("Не удалось скачать или добавить в торрент-клиент торрент-файл для раздачи \"" + t.Name + "\". Статус раздачи: " + t.Status.ToString() + "\t\t" + ex.Message);
            }
            num1 += new Decimal(1000, 0, 0, false, (byte) 1) / (Decimal) topicInfoList.Count;
            backgroundWorker.ReportProgress((int) num1);
          }
          Logic.logger.Info("Завершена задача на скачивание и добавление торрент-файлов в торрент-клиент.");
        }
      }
      catch (Exception ex)
      {
        Logic.logger.Error("Произошла ошибка при скачивании и добавлении торрент-файлов в торрент-клиент: " + ex.Message);
        Logic.logger.Debug<Exception>(ex);
        int num2 = (int) MessageBox.Show("Поизошла ошибка скачивании торрент-файлов:\r\n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
      }
    }

    public static void bwSetLabels(object sender, DoWorkEventArgs e)
    {
      BackgroundWorker backgroundWorker = sender as BackgroundWorker;
      Decimal num1 = new Decimal(0, 0, 0, false, (byte) 1);
      backgroundWorker.ReportProgress((int) num1);
      try
      {
        Tuple<MainForm, List<TopicInfo>, string> tuple = e.Argument as Tuple<MainForm, List<TopicInfo>, string>;
        List<TopicInfo> topicInfoList = tuple.Item2;
        string label = tuple.Item3;
        Logic.logger.Info("Запущена задача на установку пользовательских меток в торрент-клиент...");
        List<TorrentClientInfo> torrentClients = ClientLocalDB.Current.GetTorrentClients();
        foreach (TorrentClientInfo torrentClientInfo in torrentClients)
        {
          try
          {
            ITorrentClient torrentClient = torrentClientInfo.Create();
            torrentClient.SetLabel((IEnumerable<string>) torrentClient.GetAllTorrentHash().Join<TopicInfo, TopicInfo, string, string>((IEnumerable<TopicInfo>) topicInfoList, (Func<TopicInfo, string>) (tc => tc.Hash), (Func<TopicInfo, string>) (tp => tp.Hash), (Func<TopicInfo, TopicInfo, string>) ((tc, tp) => tp.Hash)).ToArray<string>(), label);
            num1 += new Decimal(1000, 0, 0, false, (byte) 1) / (Decimal) torrentClients.Count<TorrentClientInfo>();
            if (num1 <= new Decimal(100))
              backgroundWorker.ReportProgress((int) num1);
          }
          catch
          {
          }
        }
        backgroundWorker.ReportProgress(100);
      }
      catch (Exception ex)
      {
        Logic.logger.Error("Произошла ошибка при установке пользовательских меток в торрент-клиент: " + ex.Message);
        Logic.logger.Debug<Exception>(ex);
        int num2 = (int) MessageBox.Show("Поизошла ошибка:\r\n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
      }
    }

    public static void bwUpdateCountSeedersByAllCategories(object sender, DoWorkEventArgs e)
    {
      Logic.logger.Info("Запущена задача на обновление информации о кол-ве сидов на раздачах...");
      BackgroundWorker backgroundWorker = sender as BackgroundWorker;
      Decimal num = new Decimal(0, 0, 0, false, (byte) 1);
      backgroundWorker.ReportProgress((int) num);
      try
      {
        Logic.logger.Trace("\t Очищаем историю о кол-ве сидов на раздаче...");
        ClientLocalDB.Current.ClearHistoryStatus();
        List<Category> categoriesEnable = ClientLocalDB.Current.GetCategoriesEnable();
        foreach (Category category in categoriesEnable)
        {
          Logic.logger.Trace("\t " + category.Name + "...");
          try
          {
            ClientLocalDB.Current.SaveStatus(RuTrackerOrg.Current.GetTopicsStatus(category.CategoryID), true);
          }
          catch (Exception ex)
          {
            Logic.logger.Warn("Не удалось обновить кол-во сидов по разделу \"" + category.Name + "\"");
            Logic.logger.Debug<Exception>(ex);
          }
          num += new Decimal(1000, 0, 0, false, (byte) 1) / (Decimal) categoriesEnable.Count;
          backgroundWorker.ReportProgress((int) num);
        }
        if (Settings.Current.IsUpdateStatistics)
        {
          Logic.logger.Trace("\t Обновление статистики...");
          ClientLocalDB.Current.UpdateStatistics();
        }
      }
      catch (Exception ex)
      {
        Logic.logger.Error(ex.Message);
        Logic.logger.Debug<Exception>(ex);
      }
      Logic.logger.Info("Завершена задача по обновлению информации о кол-ве сидов на раздачах.");
    }

    public static void bwUpdateHashFromAllTorrentClients(object sender, DoWorkEventArgs e)
    {
      BackgroundWorker backgroundWorker = sender as BackgroundWorker;
      Decimal num1 = new Decimal(0, 0, 0, false, (byte) 1);
      backgroundWorker.ReportProgress((int) num1);
      try
      {
        ClientLocalDB.Current.ResetFlagsTopicDownloads();
        List<TorrentClientInfo> torrentClients = ClientLocalDB.Current.GetTorrentClients();
        foreach (TorrentClientInfo torrentClientInfo in torrentClients)
        {
          try
          {
            ITorrentClient torrentClient = torrentClientInfo.Create();
            if (torrentClient != null)
              ClientLocalDB.Current.SetTorrentClientHash(torrentClient.GetAllTorrentHash());
          }
          catch (Exception ex)
          {
            Logic.logger.Warn("Не удалось загрузить список статусов раздач из torrent-клиента \"" + torrentClientInfo.Name + "\": \"" + ex.Message + "\". Возможно клиент не запущен или нет доступа.");
            Logic.logger.Debug<Exception>(ex);
          }
          num1 += new Decimal(1000, 0, 0, false, (byte) 1) / (Decimal) torrentClients.Count;
          backgroundWorker.ReportProgress((int) num1);
        }
        Logic.CreateReports();
      }
      catch (Exception ex)
      {
        Logic.logger.Error(ex.Message);
        Logic.logger.Debug<Exception>(ex);
        int num2 = (int) MessageBox.Show("Поизошла ошибка при отправке отчетов:\r\n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
      }
    }

    public static void bwUpdateHashFromTorrentClientsByCategoryUID(object sender, DoWorkEventArgs e)
    {
      BackgroundWorker backgroundWorker = sender as BackgroundWorker;
      Decimal num1 = new Decimal(0, 0, 0, false, (byte) 1);
      backgroundWorker.ReportProgress((int) num1);
      try
      {
        Category category = e.Argument as Category;
        if (category == null)
          return;
        Logic.logger.Info("Обновление списка хранимого из торрент-клиента (по разделу)...");
        List<TorrentClientInfo> list = ClientLocalDB.Current.GetTorrentClients().Where<TorrentClientInfo>((Func<TorrentClientInfo, bool>) (x => x.UID == category.TorrentClientUID)).ToList<TorrentClientInfo>();
        foreach (TorrentClientInfo torrentClientInfo in list)
        {
          try
          {
            ITorrentClient torrentClient = torrentClientInfo.Create();
            if (torrentClient != null)
              ClientLocalDB.Current.SetTorrentClientHash(torrentClient.GetAllTorrentHash());
          }
          catch (Exception ex)
          {
            Logic.logger.Warn("Не удалось загрузить список статусов раздач из torrent-клиента \"" + torrentClientInfo.Name + "\": \"" + ex.Message + "\". Возможно клиент не запущен или нет доступа.");
            Logic.logger.Debug<Exception>(ex);
          }
          num1 += new Decimal(1000, 0, 0, false, (byte) 1) / (Decimal) list.Count;
          backgroundWorker.ReportProgress((int) num1);
        }
        Logic.CreateReports();
        Logic.logger.Info("Завершена задача по обновлению списка хранимого из торрент-клиента (по разделу).");
      }
      catch (Exception ex)
      {
        Logic.logger.Error("Произошла ошибка при обновлении списка хранимого из торрент-клиента: " + ex.Message);
        Logic.logger.Debug<Exception>(ex);
        int num2 = (int) MessageBox.Show("Произошла ошибка при отправке отчетов:\r\n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
      }
    }

    public static void bwUpdateTopicsByCategory(object sender, DoWorkEventArgs e)
    {
      BackgroundWorker backgroundWorker = sender as BackgroundWorker;
      Category category = e.Argument as Category;
      Decimal num1 = new Decimal(0, 0, 0, false, (byte) 1);
      backgroundWorker.ReportProgress((int) num1);
      try
      {
        int[] array = ((IEnumerable<int[]>) RuTrackerOrg.Current.GetTopicsStatus(category.CategoryID)).Select<int[], int>((Func<int[], int>) (x => x[0])).Distinct<int>().ToArray<int>();
        List<int>[] intListArray = new List<int>[array.Length % 100 == 0 ? array.Length / 100 : array.Length / 100 + 1];
        for (int index1 = 0; index1 < array.Length; ++index1)
        {
          int index2 = index1 / 100;
          if (intListArray[index2] == null)
            intListArray[index2] = new List<int>();
          intListArray[index2].Add(array[index1]);
        }
        foreach (List<int> intList in intListArray)
        {
          ClientLocalDB.Current.SaveTopicInfo(RuTrackerOrg.Current.GetTopicsInfo(intList.ToArray()), true);
          num1 += new Decimal(1000, 0, 0, false, (byte) 1) / (Decimal) intListArray.Length;
          backgroundWorker.ReportProgress((int) num1);
        }
        ClientLocalDB.Current.SaveUsers(RuTrackerOrg.Current.GetUsers(ClientLocalDB.Current.GetNoUsers()));
      }
      catch (Exception ex)
      {
        Logic.logger.Error(ex.Message);
        Logic.logger.Debug<Exception>(ex);
        int num2 = (int) MessageBox.Show("Произошла ошибка при отправке отчетов:\r\n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
      }
    }

    public static void bwUpdateTopicsByCategories(object sender, DoWorkEventArgs e)
    {
      Logic.logger.Info("Запущена задача по обновлению топиков...");
      BackgroundWorker backgroundWorker = sender as BackgroundWorker;
      List<Category> categoryList = e.Argument as List<Category>;
      Decimal num1 = new Decimal(0, 0, 0, false, (byte) 1);
      backgroundWorker.ReportProgress((int) num1);
      try
      {
        foreach (Category category in categoryList)
        {
          Logic.logger.Trace("\t Обрабатывается форум \"" + category.Name + "\"...");
          try
          {
            int[] array = ((IEnumerable<int[]>) RuTrackerOrg.Current.GetTopicsStatus(category.CategoryID)).Select<int[], int>((Func<int[], int>) (x => x[0])).Distinct<int>().ToArray<int>();
            List<int>[] intListArray = new List<int>[array.Length % 100 == 0 ? array.Length / 100 : array.Length / 100 + 1];
            for (int index1 = 0; index1 < array.Length; ++index1)
            {
              int index2 = index1 / 100;
              if (intListArray[index2] == null)
                intListArray[index2] = new List<int>();
              intListArray[index2].Add(array[index1]);
            }
            ClientLocalDB.Current.DeleteTopicsByCategoryId(category.CategoryID);
            foreach (List<int> intList in intListArray)
            {
              ClientLocalDB.Current.SaveTopicInfo(RuTrackerOrg.Current.GetTopicsInfo(intList.ToArray()), true);
              num1 += new Decimal(1000, 0, 0, false, (byte) 1) / (Decimal) (categoryList.Count * intListArray.Length);
              backgroundWorker.ReportProgress((int) num1);
            }
          }
          catch (Exception ex)
          {
            Logic.logger.Error("Ошибка при обновлении топиков: " + ex.Message);
            Logic.logger.Debug<Exception>(ex);
          }
          ClientLocalDB.Current.SaveUsers(RuTrackerOrg.Current.GetUsers(ClientLocalDB.Current.GetNoUsers()));
        }
      }
      catch (Exception ex)
      {
        Logic.logger.Error(ex.Message);
        Logic.logger.Debug<Exception>(ex);
        int num2 = (int) MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
      }
      Logic.logger.Info("Завершена задача по обновлению топиков.");
    }

    public static void bwUpdateKeepersByAllCategories(object sender, DoWorkEventArgs e)
    {
      Logic.logger.Info("Запущена задача по обновлению информации о хранителях...");
      BackgroundWorker backgroundWorker = sender as BackgroundWorker;
      Decimal num1 = new Decimal(0, 0, 0, false, (byte) 1);
      backgroundWorker.ReportProgress((int) num1);
      try
      {
        ClientLocalDB.Current.ClearKeepers();
        int[] categories = ClientLocalDB.Current.GetCategoriesEnable().Select<Category, int>((Func<Category, int>) (x => x.CategoryID)).OrderBy<int, int>((Func<int, int>) (x => x)).ToArray<int>();
        var array = ClientLocalDB.Current.GetReports(new int?()).Where<KeyValuePair<Tuple<int, int>, Tuple<string, string>>>((Func<KeyValuePair<Tuple<int, int>, Tuple<string, string>>, bool>) (x =>
        {
          if (x.Key.Item2 == 0 && x.Key.Item1 != 0 && !string.IsNullOrWhiteSpace(x.Value.Item1))
            return ((IEnumerable<int>) categories).Any<int>((Func<int, bool>) (z => z == x.Key.Item1));
          return false;
        })).Select(x =>
        {
          string[] strArray = x.Value.Item1.Split('=');
          if (strArray.Length == 3)
            return new
            {
              TopicID = int.Parse(strArray[2]),
              CategoryID = x.Key.Item1
            };
          if (strArray.Length == 2)
            return new
            {
              TopicID = int.Parse(strArray[1]),
              CategoryID = x.Key.Item1
            };
          return new
          {
            TopicID = 0,
            CategoryID = x.Key.Item1
          };
        }).Where(x => (uint) x.TopicID > 0U).OrderBy(x => x.CategoryID).ToArray();
        RuTrackerOrg ruTrackerOrg = new RuTrackerOrg(Settings.Current.KeeperName, Settings.Current.KeeperPass);
        foreach (var data in array)
        {
          Logic.logger.Trace("\t" + (object) data.CategoryID);
          ClientLocalDB.Current.SaveKeepOtherKeepers(ruTrackerOrg.GetKeeps(data.TopicID, data.CategoryID));
          num1 += new Decimal(1000, 0, 0, false, (byte) 1) / (Decimal) array.Count();
          backgroundWorker.ReportProgress((int) num1);
        }
        ClientLocalDB.Current.CreateReportByRootCategories();
      }
      catch (Exception ex)
      {
        Logic.logger.Error(ex.Message);
        Logic.logger.Debug<Exception>(ex);
        int num2 = (int) MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
      }
      Logic.logger.Info("Завершена задача по обновлению информации о хранителях.");
    }

    public static void bwRuningAndStopingDistributions(object sender, DoWorkEventArgs e)
    {
      Logic.logger.Info("Запущена задача по запуску/остановке раздач в торрент-клиентах...");
      BackgroundWorker backgroundWorker = sender as BackgroundWorker;
      object obj = e.Argument;
      Decimal num = new Decimal(0, 0, 0, false, (byte) 1);
      backgroundWorker.ReportProgress((int) num);
      Dictionary<int, int> countSeedersBycategories = new Dictionary<int, int>();
      try
      {
        IEnumerable<TopicInfo> inner = ClientLocalDB.Current.GetTopicsByCategory(-1).Where<TopicInfo>((Func<TopicInfo, bool>) (x => !x.IsBlackList));
        foreach (Category category in ClientLocalDB.Current.GetCategoriesEnable())
        {
          if (!countSeedersBycategories.ContainsKey(category.CategoryID))
            countSeedersBycategories.Add(category.CategoryID, category.CountSeeders);
        }
        ClientLocalDB.Current.ResetFlagsTopicDownloads();
        List<TorrentClientInfo> torrentClients = ClientLocalDB.Current.GetTorrentClients();
        foreach (TorrentClientInfo torrentClientInfo in torrentClients)
        {
          try
          {
            ITorrentClient torrentClient = torrentClientInfo.Create();
            if (torrentClient != null)
            {
              List<TopicInfo> allTorrentHash = torrentClient.GetAllTorrentHash();
              Logic.logger.Info("\t Кол-во раздач в торрент-клиенте \"" + torrentClientInfo.Name + "\": " + (object) allTorrentHash.Count);
              ClientLocalDB.Current.SetTorrentClientHash(allTorrentHash);
              var list = allTorrentHash.Join(inner, (Func<TopicInfo, string>) (c => c.Hash), (Func<TopicInfo, string>) (a => a.Hash), (c, a) => new
              {
                c = c,
                a = a
              }).Where(_param1 => _param1.c.IsRun.HasValue).Select(_param1 => new
              {
                Hash = _param1.a.Hash,
                IsRun = _param1.c.IsRun.Value,
                IsPause = _param1.c.IsPause,
                Seeders = _param1.a.Seeders,
                MaxSeeders = countSeedersBycategories.ContainsKey(_param1.a.CategoryID) ? new int?(countSeedersBycategories[_param1.a.CategoryID]) : new int?()
              }).ToList();
              string[] array1 = list.Where(x =>
              {
                if (x.IsRun && x.MaxSeeders.HasValue)
                  return x.Seeders > x.MaxSeeders.Value + 1;
                return false;
              }).Select(x => x.Hash).ToArray<string>();
              Logic.logger.Info("\t Кол-во раздач в торрент-клиенте \"" + torrentClientInfo.Name + "\" которые требуется остановить: " + (object) array1.Length + ". Останавливаем...");
              List<string>[] stringListArray1 = new List<string>[array1.Length / 50 + (array1.Length % 50 != 0 ? 1 : 0)];
              for (int index1 = 0; index1 < array1.Length; ++index1)
              {
                int index2 = index1 / 50;
                if (stringListArray1[index2] == null)
                  stringListArray1[index2] = new List<string>();
                stringListArray1[index2].Add(array1[index1]);
              }
              if (stringListArray1.Length == 0)
              {
                num += new Decimal(1000, 0, 0, false, (byte) 1) / (Decimal) (2 * torrentClients.Count);
                backgroundWorker.ReportProgress((int) num);
              }
              foreach (List<string> stringList in stringListArray1)
              {
                if (stringList != null)
                  torrentClient.DistributionStop((IEnumerable<string>) stringList);
                num += new Decimal(1000, 0, 0, false, (byte) 1) / (Decimal) (2 * torrentClients.Count * stringListArray1.Length);
                backgroundWorker.ReportProgress((int) num);
              }
              string[] array2 = list.Where(x =>
              {
                if ((!x.IsRun || x.IsPause) && x.MaxSeeders.HasValue)
                  return x.Seeders <= x.MaxSeeders.Value;
                return false;
              }).Select(x => x.Hash).ToArray<string>();
              List<string>[] stringListArray2 = new List<string>[array2.Length / 50 + (array2.Length % 50 != 0 ? 1 : 0)];
              Logic.logger.Info("\t Кол-во раздач в торрент-клиенте \"" + torrentClientInfo.Name + "\" которые требуется запустить: " + (object) array2.Length + ". Запускаем...");
              for (int index1 = 0; index1 < array2.Length; ++index1)
              {
                int index2 = index1 / 50;
                if (stringListArray2[index2] == null)
                  stringListArray2[index2] = new List<string>();
                stringListArray2[index2].Add(array2[index1]);
              }
              if (stringListArray2.Length == 0)
              {
                num += new Decimal(1000, 0, 0, false, (byte) 1) / (Decimal) (2 * torrentClients.Count);
                backgroundWorker.ReportProgress((int) num);
              }
              foreach (List<string> stringList in stringListArray2)
              {
                if (stringList != null)
                  torrentClient.DistributionStart((IEnumerable<string>) stringList);
                num += new Decimal(1000, 0, 0, false, (byte) 1) / (Decimal) (2 * torrentClients.Count * stringListArray2.Length);
                backgroundWorker.ReportProgress((int) num);
              }
            }
            else
              num += new Decimal(1000, 0, 0, false, (byte) 1) / (Decimal) torrentClients.Count;
            backgroundWorker.ReportProgress((int) num);
          }
          catch (Exception ex)
          {
            Logic.logger.Warn("Не удалось запустить/остановить раздачи на клиенте \"" + torrentClientInfo.Name + "\": " + ex.Message);
            Logic.logger.Debug<Exception>(ex);
            num += new Decimal(1000, 0, 0, false, (byte) 1) / (Decimal) torrentClients.Count;
          }
          backgroundWorker.ReportProgress((int) num);
        }
        Logic.logger.Info("Строим отчеты о хранимом...");
        Logic.CreateReports();
        Logic.logger.Info("Отчеты о хранимом построены.");
      }
      catch (Exception ex)
      {
        Logic.logger.Warn("Произошла критическая ошибка при запуске/остановки раздач");
        Logic.logger.Debug<Exception>(ex);
      }
      Logic.logger.Info("Завершена задача по запуску/остановке раздач в торрент-клиентах.");
      Logic.logger.Debug(string.Format("Размер ОЗУ 1: {0}", (object) GC.GetTotalMemory(false)));
      GC.Collect(2);
      Logic.logger.Debug(string.Format("Размер ОЗУ 2: {0}", (object) GC.GetTotalMemory(false)));
    }

    public static void bwCreateReportsTorrentClients(object sender, DoWorkEventArgs e)
    {
      List<TorrentClientInfo> torrentClients = ClientLocalDB.Current.GetTorrentClients();
      IEnumerable<TopicInfo> inner = ClientLocalDB.Current.GetTopicsByCategory(-1).Where<TopicInfo>((Func<TopicInfo, bool>) (x => !x.IsBlackList));
      Logic.logger.Info("Строим отчет о статистике в торрент-клиенте...");
      StringBuilder stringBuilder = new StringBuilder();
      Dictionary<int, Category> dictionary = ClientLocalDB.Current.GetCategories().ToDictionary<Category, int, Category>((Func<Category, int>) (x => x.CategoryID), (Func<Category, Category>) (x => x));
      int num1 = Math.Max(dictionary.Count == 0 ? 20 : dictionary.Values.Max<Category>((Func<Category, int>) (x => x.FullName.Length)), torrentClients.Count == 0 ? 20 : torrentClients.Max<TorrentClientInfo>((Func<TorrentClientInfo, int>) (x => x.Name.Length)));
      string empty = string.Empty;
      for (int index = 0; index < num1; ++index)
        empty += "*";
      BackgroundWorker backgroundWorker = sender as BackgroundWorker;
      Decimal num2 = new Decimal(0, 0, 0, false, (byte) 1);
      backgroundWorker.ReportProgress((int) num2);
      foreach (TorrentClientInfo torrentClientInfo in torrentClients)
      {
        Logic.logger.Debug("\t" + torrentClientInfo.Name + "...");
        try
        {
          stringBuilder.AppendLine(empty);
          stringBuilder.AppendFormat("*\t{0}\r\n", (object) torrentClientInfo.Name);
          stringBuilder.AppendLine(empty);
          ITorrentClient torrentClient = torrentClientInfo.Create();
          if (torrentClient != null)
          {
            var array1 = torrentClient.GetAllTorrentHash().GroupJoin(inner, (Func<TopicInfo, string>) (t => t.Hash), (Func<TopicInfo, string>) (b => b.Hash), (t, bt) => new
            {
              t = t,
              bt = bt
            }).SelectMany(_param1 => _param1.bt.DefaultIfEmpty<TopicInfo>(), (_param1, b) =>
            {
              int num3 = b != null ? b.CategoryID : -1;
              long size = _param1.t.Size;
              bool? isRun = _param1.t.IsRun;
              int num4;
              if (!isRun.HasValue)
              {
                num4 = -1;
              }
              else
              {
                isRun = _param1.t.IsRun;
                num4 = isRun.Value ? 1 : 0;
              }
              int num5 = _param1.t.IsPause ? 1 : 0;
              int num6 = b == null ? -1 : b.Seeders;
              return new
              {
                CategoryID = num3,
                Size = size,
                IsRun = num4,
                IsPause = num5 != 0,
                Seeders = num6
              };
            }).GroupBy(x => new
            {
              CategoryID = x.CategoryID,
              IsRun = x.IsRun,
              IsPause = x.IsPause,
              Seeders = x.Seeders
            }).Select(x => new
            {
              CategoryID = x.Key.CategoryID,
              IsRun = x.Key.IsRun,
              IsPause = x.Key.IsPause,
              Size = x.Sum(y => y.Size),
              Count = x.Count(),
              Seeders = x.Key.Seeders
            }).ToArray();
            stringBuilder.AppendFormat("\tВсего:\t\t{0,6} шт. ({1})\r\n", (object) array1.Sum(x => x.Count), (object) TopicInfo.sizeToString(array1.Sum(x => x.Size)));
            stringBuilder.AppendFormat("\tРаздаются:\t{0,6} шт. ({1})\r\n", (object) array1.Where(x => x.IsRun == 1).Sum(x => x.Count), (object) TopicInfo.sizeToString(array1.Where(x => x.IsRun == 1).Sum(x => x.Size)));
            stringBuilder.AppendFormat("\tОстановлены:\t{0,6} шт. ({1})\r\n", (object) array1.Where(x => x.IsRun == 0).Sum(x => x.Count), (object) TopicInfo.sizeToString(array1.Where(x => x.IsRun == 0).Sum(x => x.Size)));
            stringBuilder.AppendFormat("\tПрочие:\t\t{0,6} шт. ({1})\r\n", (object) array1.Where(x => x.IsRun == -1).Sum(x => x.Count), (object) TopicInfo.sizeToString(array1.Where(x => x.IsRun == -1).Sum(x => x.Size)));
            stringBuilder.AppendFormat("\tНеизвестные:\t{0,6} шт. ({1})\r\n", (object) array1.Where(x => x.CategoryID == -1).Sum(x => x.Count), (object) TopicInfo.sizeToString(array1.Where(x => x.CategoryID == -1).Sum(x => x.Size)));
            stringBuilder.AppendLine();
            stringBuilder.AppendFormat("\tПо кол-ву сидов:\r\n");
            foreach (var data in array1.GroupBy(x => x.Seeders).Select(x => new
            {
              Seeders = x.Key,
              Count = x.Sum(z => z.Count),
              Size = x.Sum(z => z.Size)
            }).OrderBy(x => x.Seeders))
              stringBuilder.AppendFormat("\t{2}:\t\t{0,5} шт. ({1})\r\n", (object) data.Count, (object) TopicInfo.sizeToString(data.Size), (object) data.Seeders);
            stringBuilder.AppendLine();
            foreach (int num3 in array1.Select(x => x.CategoryID).Distinct<int>().OrderBy<int, int>((Func<int, int>) (x => x)).ToArray<int>())
            {
              int c = num3;
              var array2 = array1.Where(x => x.CategoryID == c).ToArray();
              string str = "Неизвестные";
              if (dictionary.ContainsKey(c))
                str = dictionary[c].FullName;
              stringBuilder.AppendFormat("{0}:\r\n", (object) str);
              stringBuilder.AppendFormat("\tВсего:\t\t{0,5} шт. ({1})\r\n", (object) array2.Sum(x => x.Count), (object) TopicInfo.sizeToString(array2.Sum(x => x.Size)));
              stringBuilder.AppendFormat("\tРаздаются:\t{0,5} шт. ({1})\r\n", (object) array2.Where(x => x.IsRun == 1).Sum(x => x.Count), (object) TopicInfo.sizeToString(array2.Where(x => x.IsRun == 1).Sum(x => x.Size)));
              stringBuilder.AppendFormat("\tОстановлены:\t{0,5} шт. ({1})\r\n", (object) array2.Where(x => x.IsRun == 0).Sum(x => x.Count), (object) TopicInfo.sizeToString(array2.Where(x => x.IsRun == 0).Sum(x => x.Size)));
              stringBuilder.AppendFormat("\tПрочие:\t\t{0,5} шт. ({1})\r\n", (object) array2.Where(x => x.IsRun == -1).Sum(x => x.Count), (object) TopicInfo.sizeToString(array2.Where(x => x.IsRun == -1).Sum(x => x.Size)));
            }
            stringBuilder.AppendLine();
          }
        }
        catch (Exception ex)
        {
          stringBuilder.AppendFormat("Ошибка: {0}\r\n\r\n\r\n", (object) ex.Message);
        }
        stringBuilder.AppendLine();
        num2 += new Decimal(1000, 0, 0, false, (byte) 1) / (Decimal) torrentClients.Count<TorrentClientInfo>();
        if (num2 <= new Decimal(100))
          backgroundWorker.ReportProgress((int) num2);
      }
      Dictionary<int, Dictionary<int, string>> reports = new Dictionary<int, Dictionary<int, string>>();
      reports.Add(0, new Dictionary<int, string>());
      reports[0].Add(1, stringBuilder.ToString());
      try
      {
        ClientLocalDB.Current.SaveReports(reports);
        Logic.logger.Info("Отчет о статистике в торрент-клиенте построен.");
      }
      catch (Exception ex)
      {
        Logic.logger.Error("Произошла ошибка при сохранении отчета в базу данных: " + ex.Message);
        Logic.logger.Trace(ex.StackTrace);
      }
    }

    public static void bwSendReports(object sender, DoWorkEventArgs e)
    {
      Logic.logger.Info("Запущена задача на отправку отчетов на форум....");
      Decimal num1 = new Decimal(0, 0, 0, false, (byte) 1);
      BackgroundWorker backgroundWorker = sender as BackgroundWorker;
      Tuple<string, string>[] array = ClientLocalDB.Current.GetReports(new int?()).Where<KeyValuePair<Tuple<int, int>, Tuple<string, string>>>((Func<KeyValuePair<Tuple<int, int>, Tuple<string, string>>, bool>) (x => !string.IsNullOrWhiteSpace(x.Value.Item1))).OrderBy<KeyValuePair<Tuple<int, int>, Tuple<string, string>>, int>((Func<KeyValuePair<Tuple<int, int>, Tuple<string, string>>, int>) (x => x.Key.Item1)).Select<KeyValuePair<Tuple<int, int>, Tuple<string, string>>, Tuple<string, string>>((Func<KeyValuePair<Tuple<int, int>, Tuple<string, string>>, Tuple<string, string>>) (x => x.Value)).Where<Tuple<string, string>>((Func<Tuple<string, string>, bool>) (x => x.Item1.Split('=').Length == 3)).ToArray<Tuple<string, string>>();
      if (((IEnumerable<Tuple<string, string>>) array).Where<Tuple<string, string>>((Func<Tuple<string, string>, bool>) (x => !string.IsNullOrWhiteSpace(x.Item1))).Count<Tuple<string, string>>() == 0)
      {
        int num2 = (int) MessageBox.Show("Нет ни одного отчета c указанным URL для отправки на форум");
      }
      else
      {
        foreach (Tuple<string, string> tuple in ((IEnumerable<Tuple<string, string>>) array).Where<Tuple<string, string>>((Func<Tuple<string, string>, bool>) (x => !string.IsNullOrWhiteSpace(x.Item1))))
        {
          Logic.logger.Info(tuple.Item1);
          try
          {
            Logic.Current.SendReport(tuple.Item1, tuple.Item2);
          }
          catch (Exception ex)
          {
            Logic.logger.Error(ex.Message);
            Logic.logger.Debug<Exception>(ex);
            int num3 = (int) MessageBox.Show("Произошла ошибка при отправке отчетов:\r\n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
          }
          num1 += new Decimal(1000, 0, 0, false, (byte) 1) / (Decimal) array.Length;
          backgroundWorker.ReportProgress((int) num1);
        }
        Logic.logger.Info("Завершена задача на отправку отчетов на форум.");
      }
    }

    public static void CreateReports()
    {
      ClientLocalDB.Current.ClearReports();
      List<Category> categoriesEnable = ClientLocalDB.Current.GetCategoriesEnable();
      Dictionary<Tuple<int, int>, Tuple<string, string>> reports1 = ClientLocalDB.Current.GetReports(new int?());
      Dictionary<int, Dictionary<int, string>> reports2 = new Dictionary<int, Dictionary<int, string>>();
      Tuple<int, string, int, Decimal>[] array1 = ClientLocalDB.Current.GetStatisticsByAllUsers().Where<Tuple<int, string, int, Decimal>>((Func<Tuple<int, string, int, Decimal>, bool>) (x => !string.IsNullOrWhiteSpace(x.Item2))).ToArray<Tuple<int, string, int, Decimal>>();
      StringBuilder stringBuilder1 = new StringBuilder();
      Tuple<int, string, int, Decimal>[] array2 = ((IEnumerable<Tuple<int, string, int, Decimal>>) array1).Where<Tuple<int, string, int, Decimal>>((Func<Tuple<int, string, int, Decimal>, bool>) (x => x.Item2 == Settings.Current.KeeperName)).ToArray<Tuple<int, string, int, Decimal>>();
      int[] catId = categoriesEnable.Select<Category, int>((Func<Category, int>) (x => x.CategoryID)).ToArray<int>();
      stringBuilder1.Clear();
      stringBuilder1.AppendFormat("Актуально на: {0}\r\n\r\n", (object) DateTime.Now.ToString("dd.MM.yyyy"));
      stringBuilder1.AppendFormat("Общее количество хранимых раздач: {0} шт.\r\n", (object) ((IEnumerable<Tuple<int, string, int, Decimal>>) array2).Where<Tuple<int, string, int, Decimal>>((Func<Tuple<int, string, int, Decimal>, bool>) (x => ((IEnumerable<int>) catId).Contains<int>(x.Item1))).Sum<Tuple<int, string, int, Decimal>>((Func<Tuple<int, string, int, Decimal>, int>) (x => x.Item3)));
      stringBuilder1.AppendFormat("Общий вес хранимых раздач: {0} Gb\r\n", (object) ((IEnumerable<Tuple<int, string, int, Decimal>>) array2).Where<Tuple<int, string, int, Decimal>>((Func<Tuple<int, string, int, Decimal>, bool>) (x => ((IEnumerable<int>) catId).Contains<int>(x.Item1))).Sum<Tuple<int, string, int, Decimal>>((Func<Tuple<int, string, int, Decimal>, Decimal>) (x => x.Item4)).ToString("0.00"));
      stringBuilder1.AppendLine("[hr]");
      foreach (Category category1 in (IEnumerable<Category>) categoriesEnable.OrderBy<Category, string>((Func<Category, string>) (x => x.FullName)))
      {
        Category category = category1;
        Tuple<int, string, int, Decimal> tuple = ((IEnumerable<Tuple<int, string, int, Decimal>>) array2).Where<Tuple<int, string, int, Decimal>>((Func<Tuple<int, string, int, Decimal>, bool>) (x => x.Item1 == category.CategoryID)).FirstOrDefault<Tuple<int, string, int, Decimal>>() ?? new Tuple<int, string, int, Decimal>(category.CategoryID, "<->", 0, Decimal.Zero);
        string empty1 = string.Empty;
        if (reports1.ContainsKey(new Tuple<int, int>(tuple.Item1, 1)))
        {
          string str = reports1[new Tuple<int, int>(tuple.Item1, 1)].Item1;
          string empty2;
          if (!string.IsNullOrWhiteSpace(str))
          {
            if (str.Split('=').Length > 2)
            {
              empty2 = str.Split('=')[2];
              goto label_7;
            }
          }
          empty2 = string.Empty;
label_7:
          stringBuilder1.AppendFormat("{0}{1}{2} - {3} шт. ({4:0.00} GB)\r\n", string.IsNullOrWhiteSpace(empty2) ? (object) "" : (object) string.Format("[url=https://rutracker.org/forum/viewtopic.php?p={0}#{0}]", (object) empty2), (object) category.FullName, string.IsNullOrWhiteSpace(empty2) ? (object) "" : (object) "[/url]", (object) tuple.Item3, (object) tuple.Item4);
        }
      }
      reports2.Add(0, new Dictionary<int, string>());
      reports2[0].Add(0, stringBuilder1.ToString());
      ClientLocalDB.Current.SaveReports(reports2);
      reports2.Clear();
      foreach (Category category1 in categoriesEnable)
      {
        Category category = category1;
        stringBuilder1.Clear();
        IEnumerable<Tuple<int, string, int, Decimal>> source = ((IEnumerable<Tuple<int, string, int, Decimal>>) array1).Where<Tuple<int, string, int, Decimal>>((Func<Tuple<int, string, int, Decimal>, bool>) (x =>
        {
          if (x.Item1 == category.CategoryID && x.Item3 > 0)
            return x.Item2 != "All";
          return false;
        }));
        Tuple<int, string, int, Decimal> tuple1 = ((IEnumerable<Tuple<int, string, int, Decimal>>) array1).Where<Tuple<int, string, int, Decimal>>((Func<Tuple<int, string, int, Decimal>, bool>) (x =>
        {
          if (x.Item1 == category.CategoryID)
            return x.Item2 == "All";
          return false;
        })).FirstOrDefault<Tuple<int, string, int, Decimal>>();
        if (source != null && source.Count<Tuple<int, string, int, Decimal>>() != 0 && tuple1 != null)
        {
          stringBuilder1.AppendFormat("[url=viewforum.php?f={0}][color=darkgreen][b]{1}[/b][/color][/url] | [url=tracker.php?f={0}&tm=-1&o=10&s=1][color=darkgreen][b]Проверка сидов[/b][/color][/url]\r\n\r\n", (object) category.CategoryID, (object) category.Name);
          stringBuilder1.AppendFormat("[b]Актуально на:[/b] {0}\r\n\r\n", (object) DateTime.Now.ToString("dd.MM.yyyy"));
          stringBuilder1.AppendFormat("[b]Общее количество раздач в подразделе:[/b] {0} шт.\r\n", (object) tuple1.Item3);
          stringBuilder1.AppendFormat("[b]Общий размер раздач в подразделе:[/b] {0:0.00} GB.\r\n", (object) tuple1.Item4);
          stringBuilder1.AppendFormat("[b]Количество хранителей:[/b] {0}\r\n", (object) source.Count<Tuple<int, string, int, Decimal>>());
          stringBuilder1.AppendFormat("[b]Общее количество хранимых раздач:[/b] {0} шт.\r\n", (object) source.Sum<Tuple<int, string, int, Decimal>>((Func<Tuple<int, string, int, Decimal>, int>) (x => x.Item3)));
          stringBuilder1.AppendFormat("[b]Общий вес хранимых раздач:[/b] {0:0.00} GB.\r\n", (object) source.Sum<Tuple<int, string, int, Decimal>>((Func<Tuple<int, string, int, Decimal>, Decimal>) (x => x.Item4)));
          stringBuilder1.AppendLine("[hr]");
          int num = 0;
          foreach (Tuple<int, string, int, Decimal> tuple2 in (IEnumerable<Tuple<int, string, int, Decimal>>) source.OrderBy<Tuple<int, string, int, Decimal>, string>((Func<Tuple<int, string, int, Decimal>, string>) (x => x.Item2)))
          {
            ++num;
            stringBuilder1.AppendFormat("[b]Хранитель {0}:[/b] [url=profile.php?mode=viewprofile&u={4}][color=darkgreen][b]{1}[/b][/color][/url] - {2} шт. ({3:0.00} GB)\r\n", (object) num, (object) tuple2.Item2.Replace("<wbr>", ""), (object) tuple2.Item3, (object) tuple2.Item4, (object) HttpUtility.UrlEncode(tuple2.Item2.Replace("<wbr>", "")));
          }
          reports2.Add(category.CategoryID, new Dictionary<int, string>());
          reports2[category.CategoryID].Add(0, stringBuilder1.ToString());
        }
      }
      ClientLocalDB.Current.SaveReports(reports2);
      reports2.Clear();
      string format1 = Settings.Current.ReportTop1.Replace("%%CreateDate%%", "{0}").Replace("%%CountTopics%%", "{1}").Replace("%%SizeTopics%%", "{2}") + "\r\n";
      string format2 = Settings.Current.ReportTop2.Replace("%%CreateDate%%", "{0}").Replace("%%CountTopics%%", "{1}").Replace("%%SizeTopics%%", "{2}").Replace("%%NumberTopicsFirst%%", "{3}").Replace("%%NumberTopicsLast%%", "{4}").Replace("%%ReportLines%%", "{5}").Replace("%%Top1%%", "{6}") + "\r\n";
      string format3 = Settings.Current.ReportLine.Replace("%%ID%%", "{0}").Replace("%%Name%%", "{1}").Replace("%%Size%%", "{2}").Replace("%%Status%%", "{3}").Replace("%%CountSeeders%%", "{4}").Replace("%%Date%%", "{5}");
      int num1 = 115000;
      StringBuilder stringBuilder2 = new StringBuilder();
      StringBuilder stringBuilder3 = new StringBuilder();
      foreach (Category category in categoriesEnable)
      {
        int num2 = 0;
        int num3 = 0;
        int num4 = 1;
        int key = 0;
        stringBuilder2.Clear();
        stringBuilder3.Clear();
        TopicInfo[] array3 = ClientLocalDB.Current.GetTopicsByCategory(category.CategoryID).Where<TopicInfo>((Func<TopicInfo, bool>) (x =>
        {
          if (x.IsKeep && (x.Seeders <= Settings.Current.CountSeedersReport || Settings.Current.CountSeedersReport == -1))
            return !x.IsBlackList;
          return false;
        })).OrderBy<TopicInfo, string>((Func<TopicInfo, string>) (x => x.Name2)).ToArray<TopicInfo>();
        if (array3.Length != 0)
        {
          reports2.Add(category.CategoryID, new Dictionary<int, string>());
          Dictionary<int, string> dictionary = reports2[category.CategoryID];
          string str = string.Format(format1, (object) DateTime.Now.ToString("dd.MM.yyyy"), (object) array3.Length, (object) TopicInfo.sizeToString(((IEnumerable<TopicInfo>) array3).Sum<TopicInfo>((Func<TopicInfo, long>) (x => x.Size))));
          foreach (TopicInfo topicInfo in array3)
          {
            stringBuilder3.AppendLine(string.Format(format3, (object) topicInfo.TopicID, (object) topicInfo.Name2, (object) topicInfo.SizeToString, (object) topicInfo.StatusToString, (object) topicInfo.Seeders, (object) topicInfo.RegTimeToString));
            ++num2;
            ++num3;
            if (num2 % 10 == 0 || array3.Length <= num2)
            {
              if (array3.Length == num2)
              {
                if (num3 == 0)
                  stringBuilder2.AppendFormat("[*={0}{1}", (object) num4, (object) stringBuilder3.ToString().Substring(2));
                else
                  stringBuilder2.AppendLine(stringBuilder3.ToString());
              }
              if (num1 <= stringBuilder2.Length + stringBuilder3.Length + str.Length || array3.Length <= num2)
              {
                ++key;
                int num5 = num2 < array3.Length ? num2 - 10 : num2;
                dictionary.Add(key, string.Format(format2, (object) DateTime.Now.ToString("dd.MM.yyyy"), (object) array3.Length, (object) TopicInfo.sizeToString(((IEnumerable<TopicInfo>) array3).Sum<TopicInfo>((Func<TopicInfo, long>) (x => x.Size))), (object) num4, (object) num5, (object) stringBuilder2.ToString(), (object) str) + Settings.Current.ReportBottom);
                stringBuilder2.Clear();
                num3 = 0;
                num4 = num5 + 1;
                str = string.Empty;
              }
              if (num3 == 0)
                stringBuilder2.AppendFormat("[*={0}{1}\r\n", (object) num4, (object) stringBuilder3.ToString().Substring(2));
              else
                stringBuilder2.AppendLine(stringBuilder3.ToString());
              stringBuilder3.Clear();
            }
          }
        }
      }
      ClientLocalDB.Current.SaveReports(reports2);
    }
  }
}
