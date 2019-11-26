using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using NLog;
using TLO.local.Forms;

namespace TLO.local
{
    internal static class WorkerMethods
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static void bwDownloadTorrentFiles(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker backgroundWorker = sender as BackgroundWorker;
            Decimal num1 = new Decimal(0, 0, 0, false, 1);
            backgroundWorker.ReportProgress((int) num1);
            try
            {
                Tuple<List<TopicInfo>, MainForm> tuple = e.Argument as Tuple<List<TopicInfo>, MainForm>;
                List<TopicInfo> topicInfoList = tuple.Item1;
                string folder = String.Empty;
                if (topicInfoList == null || topicInfoList.Count == 0)
                    return;
                tuple.Item2.Invoke((MethodInvoker) delegate
                {
                    FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                    if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
                        return;
                    folder = folderBrowserDialog.SelectedPath;
                });
                if (String.IsNullOrWhiteSpace(folder))
                {
                    int num2;
                    tuple.Item2.Invoke((MethodInvoker) delegate
                    {
                        num2 = (int) MessageBox.Show("Не указан каталог для сохранения торрент-файлов", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
                    });
                }
                else
                {
                    foreach (TopicInfo topicInfo in topicInfoList)
                    {
                        byte[] buffer = RuTrackerOrg.Current.DownloadTorrentFile(topicInfo.TopicID);
                        if (buffer != null)
                        {
                            using (FileStream fileStream = File.Create(Path.Combine(folder,
                                String.Format("[rutracker.org].t{0}.torrent", topicInfo.TopicID))))
                                fileStream.Write(buffer, 0, buffer.Count());
                            num1 += new Decimal(1000, 0, 0, false, 1) / topicInfoList.Count;
                            backgroundWorker.ReportProgress((int) num1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                Logger.Debug(ex);
                int num2 = (int) MessageBox.Show("Произошла ошибка при скачивании торрент-файлов:\r\n" + ex.Message,
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
            }
        }

        public static void bwSendTorrentFileToTorrentClient(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker backgroundWorker = sender as BackgroundWorker;
            Decimal num1 = new Decimal(0, 0, 0, false, 1);
            backgroundWorker.ReportProgress((int) num1);
            try
            {
                Tuple<MainForm, List<TopicInfo>, Category> tuple =
                    e.Argument as Tuple<MainForm, List<TopicInfo>, Category>;
                List<TopicInfo> topicInfoList = tuple.Item2;
                Category category = tuple.Item3;
                Logger.Info("Запущена задача на скачивание и добавление торрент-файлов в торрент-клиент...");
                Logger.Trace(String.Format("\tКол-во раздач для скачивания торрент-файлов: {0}",
                    topicInfoList.Count));
                TorrentClientInfo torrentClientInfo = ClientLocalDB.Current.GetTorrentClients()
                    .Where(x => x.UID == category.TorrentClientUID)
                    .FirstOrDefault();
                IEnumerable<TopicInfo> source = torrentClientInfo.Create().GetAllTorrentHash()
                    .Where(x => !String.IsNullOrWhiteSpace(x.Hash));
                foreach (TopicInfo topicInfo1 in topicInfoList)
                {
                    TopicInfo t = topicInfo1;
                    TopicInfo topicInfo2 = source.Where(x => x.Hash == t.Hash)
                        .FirstOrDefault();
                    if (topicInfo2 != null)
                        t.TorrentName = topicInfo2.TorrentName;
                }

                List<string> list = source.Select(x => x.Hash)
                    .ToList();
                if (torrentClientInfo == null)
                {
                    Logger.Warn("Не указан торрент-клиент в категории/подфоруме");
                }
                else
                {
                    string folder = category.Folder;
                    if (String.IsNullOrWhiteSpace(folder))
                        tuple.Item1.Invoke((MethodInvoker) delegate
                        {
                            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                            if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
                                return;
                            folder = folderBrowserDialog.SelectedPath;
                        });
                    if (String.IsNullOrWhiteSpace(folder))
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
                                    string folder2 = String.Empty;
                                    if (category.CreateSubFolder == 0)
                                        folder2 = folder;
                                    else if (category.CreateSubFolder == 1)
                                    {
                                        folder2 = Path.Combine(folder, t.TopicID.ToString());
                                    }
                                    else
                                    {
                                        if (category.CreateSubFolder != 2)
                                            throw new Exception(
                                                "Не поддерживается указаный метод создания подкаталога");
                                        DialogResult result = DialogResult.None;
                                        tuple.Item1.Invoke((MethodInvoker) delegate
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
                                            throw new ArgumentException(
                                                "Не удалось создать подключение к торрент-клиенту \"" +
                                                torrentClientInfo.Name + "\"");
                                        torrentClient.SetDefaultFolder(folder2);
                                        byte[] numArray = RuTrackerOrg.Current.DownloadTorrentFile(t.TopicID);
                                        if (numArray == null)
                                        {
                                            Logger.Warn("Не удалось скачать торрент-файл для раздачи \"" +
                                                        t.Name + "\". Статус раздачи: " + t.Status.ToString());
                                            continue;
                                        }

                                        torrentClient.SendTorrentFile(folder2,
                                            String.Format("[rutracker.org].t{0}.torrent", t.TopicID),
                                            numArray);
                                        torrentClient.SetLabel(t.Hash,
                                            String.IsNullOrWhiteSpace(category.Label)
                                                ? category.FullName
                                                : category.Label);
                                        if (category.IsSaveTorrentFiles)
                                        {
                                            if (!Directory.Exists(category.FolderTorrentFile))
                                                Directory.CreateDirectory(category.FolderTorrentFile);
                                            using (FileStream fileStream = File.Create(
                                                Path.Combine(category.FolderTorrentFile,
                                                    String.Format("[rutracker.org].t{0}.torrent", t.TopicID))))
                                                fileStream.Write(numArray, 0,
                                                    numArray.Count());
                                        }
                                    }

                                    if (category.IsSaveWebPage)
                                    {
                                        Thread.Sleep(500);
                                        byte[] buffer = RuTrackerOrg.Current.DownloadWebPages(
                                            String.Format("https://{1}/forum/viewtopic.php?t={0}", t.TopicID,
                                                Settings.Current.HostRuTrackerOrg));
                                        if (!Directory.Exists(category.FolderSavePageForum))
                                            Directory.CreateDirectory(category.FolderSavePageForum);
                                        using (FileStream fileStream = File.Create(
                                            Path.Combine(category.FolderSavePageForum,
                                                String.Format("[rutracker.org].t{0}.html", t.TopicID))))
                                            fileStream.Write(buffer, 0, buffer.Count());
                                    }

                                    if (!String.IsNullOrWhiteSpace(t.TorrentName))
                                    {
                                        try
                                        {
                                            if (Directory.Exists(Path.Combine(category.Folder, t.TorrentName)))
                                            {
                                                if (!Directory.Exists(Path.Combine(category.Folder,
                                                    t.TopicID.ToString())))
                                                    Directory.CreateDirectory(Path.Combine(category.Folder,
                                                        t.TopicID.ToString()));
                                                Directory.Move(Path.Combine(category.Folder, t.TorrentName),
                                                    Path.Combine(category.Folder, t.TopicID.ToString(), t.TorrentName));
                                                continue;
                                            }

                                            if (File.Exists(Path.Combine(category.Folder, t.TorrentName)))
                                            {
                                                if (!Directory.Exists(Path.Combine(category.Folder,
                                                    t.TopicID.ToString())))
                                                    Directory.CreateDirectory(Path.Combine(category.Folder,
                                                        t.TopicID.ToString()));
                                                File.Move(Path.Combine(category.Folder, t.TorrentName),
                                                    Path.Combine(category.Folder, t.TopicID.ToString(), t.TorrentName));
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
                            Logger.Warn(
                                "Не удалось скачать или добавить в торрент-клиент торрент-файл для раздачи \"" +
                                t.Name + "\". Статус раздачи: " + t.Status.ToString() + "\t\t" + ex.Message);
                        }

                        num1 += new Decimal(1000, 0, 0, false, 1) / topicInfoList.Count;
                        backgroundWorker.ReportProgress((int) num1);
                    }

                    Logger.Info("Завершена задача на скачивание и добавление торрент-файлов в торрент-клиент.");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Произошла ошибка при скачивании и добавлении торрент-файлов в торрент-клиент: " +
                             ex.Message);
                Logger.Debug(ex);
                int num2 = (int) MessageBox.Show("Поизошла ошибка при скачивании торрент-файлов:\r\n" + ex.Message,
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
            }
        }

        public static Logger logger = LogManager.GetCurrentClassLogger();

        public static void bwSetLabels(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker backgroundWorker = sender as BackgroundWorker;
            Decimal num1 = new Decimal(0, 0, 0, false, 1);
            backgroundWorker.ReportProgress((int) num1);
            try
            {
                Tuple<MainForm, List<TopicInfo>, string> tuple = e.Argument as Tuple<MainForm, List<TopicInfo>, string>;
                List<TopicInfo> topicInfoList = tuple.Item2;
                string label = tuple.Item3;
//        logger.Info("Запущена задача на установку пользовательских меток в торрент-клиент...");
                List<TorrentClientInfo> torrentClients = ClientLocalDB.Current.GetTorrentClients();
                foreach (TorrentClientInfo torrentClientInfo in torrentClients)
                {
                    try
                    {
                        ITorrentClient torrentClient = torrentClientInfo.Create();
                        torrentClient.SetLabel(
                            torrentClient.GetAllTorrentHash()
                                .Join(topicInfoList, tc => tc.Hash, tp => tp.Hash, (tc, tp) => tp.Hash).ToArray(),
                            label);
                        num1 += new Decimal(1000, 0, 0, false, 1) / torrentClients.Count();
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
                logger.Error("Произошла ошибка при установке пользовательских меток в торрент-клиент: " + ex.Message);
                logger.Debug(ex);
                int num2 = (int) MessageBox.Show("Произошла ошибка:\r\n" + ex.Message, "Ошибка", MessageBoxButtons.OK,
                    MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
            }
        }

        public static void bwUpdateCountSeedersByAllCategories(object sender, DoWorkEventArgs e)
        {
            logger.Info("Запущена задача на обновление информации о кол-ве сидов на раздачах...");
            BackgroundWorker backgroundWorker = sender as BackgroundWorker;
            Decimal num = new Decimal(0, 0, 0, false, 1);
            backgroundWorker.ReportProgress((int) num);
            try
            {
                logger.Trace("\t Очищаем историю о кол-ве сидов на раздаче...");
                ClientLocalDB.Current.ClearHistoryStatus();
                List<Category> categoriesEnable = ClientLocalDB.Current.GetCategoriesEnable();
                foreach (Category category in categoriesEnable)
                {
                    logger.Trace("\t " + category.Name + "...");
                    try
                    {
                        ClientLocalDB.Current.SaveStatus(RuTrackerOrg.Current.GetTopicsStatus(category.CategoryID),
                            true);
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("Не удалось обновить кол-во сидов по разделу \"" + category.Name + "\"");
                        logger.Debug(ex);
                    }

                    num += new Decimal(1000, 0, 0, false, 1) / categoriesEnable.Count;
                    backgroundWorker.ReportProgress((int) num);
                }

                if (Settings.Current.IsUpdateStatistics)
                {
                    logger.Trace("\t Обновление статистики...");
                    ClientLocalDB.Current.UpdateStatistics();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                logger.Debug(ex);
            }

            logger.Info("Завершена задача по обновлению информации о кол-ве сидов на раздачах.");
        }

        public static void bwUpdateHashFromAllTorrentClients(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker backgroundWorker = sender as BackgroundWorker;
            Decimal num1 = new Decimal(0, 0, 0, false, 1);
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
                        logger.Warn("Не удалось загрузить список статусов раздач из torrent-клиента \"" +
                                    torrentClientInfo.Name + "\": \"" + ex.Message +
                                    "\". Возможно клиент не запущен или нет доступа.");
                        logger.Debug(ex);
                    }

                    num1 += new Decimal(1000, 0, 0, false, 1) / torrentClients.Count;
                    backgroundWorker.ReportProgress((int) num1);
                }

                Reports.CreateReports();
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                logger.Debug(ex);
                int num2 = (int) MessageBox.Show("Поизошла ошибка при отправке отчетов:\r\n" + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
            }
        }

        public static void bwUpdateHashFromTorrentClientsByCategoryUID(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker backgroundWorker = sender as BackgroundWorker;
            Decimal num1 = new Decimal(0, 0, 0, false, 1);
            backgroundWorker.ReportProgress((int) num1);
            try
            {
                Category category = e.Argument as Category;
                if (category == null)
                    return;
                logger.Info("Обновление списка хранимого из торрент-клиента (по разделу)...");
                List<TorrentClientInfo> list = ClientLocalDB.Current.GetTorrentClients()
                    .Where(x => x.UID == category.TorrentClientUID).ToList();
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
                        logger.Warn("Не удалось загрузить список статусов раздач из torrent-клиента \"" +
                                    torrentClientInfo.Name + "\": \"" + ex.Message +
                                    "\". Возможно клиент не запущен или нет доступа.");
                        logger.Debug(ex);
                    }

                    num1 += new Decimal(1000, 0, 0, false, 1) / list.Count;
                    backgroundWorker.ReportProgress((int) num1);
                }

                Reports.CreateReports();
                logger.Info("Завершена задача по обновлению списка хранимого из торрент-клиента (по разделу).");
            }
            catch (Exception ex)
            {
                logger.Error("Произошла ошибка при обновлении списка хранимого из торрент-клиента: " + ex.Message);
                logger.Debug(ex);
                int num2 = (int) MessageBox.Show("Произошла ошибка при отправке отчетов:\r\n" + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
            }
        }

        public static void bwUpdateTopicsByCategory(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker backgroundWorker = sender as BackgroundWorker;
            Category category = e.Argument as Category;
            Decimal num1 = new Decimal(0, 0, 0, false, 1);
            backgroundWorker.ReportProgress((int) num1);
            try
            {
                int[] array = RuTrackerOrg.Current.GetTopicsStatus(category.CategoryID).Select(x => x[0]).Distinct()
                    .ToArray();
                List<int>[] intListArray =
                    new List<int>[array.Length % 100 == 0 ? array.Length / 100 : array.Length / 100 + 1];
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
                    num1 += new Decimal(1000, 0, 0, false, 1) / intListArray.Length;
                    backgroundWorker.ReportProgress((int) num1);
                }

                ClientLocalDB.Current.SaveUsers(RuTrackerOrg.Current.GetUsers(ClientLocalDB.Current.GetNoUsers()));
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                logger.Debug(ex);
                int num2 = (int) MessageBox.Show("Произошла ошибка при отправке отчетов:\r\n" + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
            }
        }

        public static void bwUpdateTopicsByCategories(object sender, DoWorkEventArgs e)
        {
            logger.Info("Запущена задача по обновлению топиков...");
            BackgroundWorker backgroundWorker = sender as BackgroundWorker;
            List<Category> categoryList = e.Argument as List<Category>;
            Decimal num1 = new Decimal(0, 0, 0, false, 1);
            backgroundWorker.ReportProgress((int) num1);
            try
            {
                foreach (Category category in categoryList)
                {
                    logger.Trace("\t Обрабатывается форум \"" + category.Name + "\"...");
                    try
                    {
                        int[] array = RuTrackerOrg.Current.GetTopicsStatus(category.CategoryID).Select(x => x[0])
                            .Distinct().ToArray();
                        List<int>[] intListArray =
                            new List<int>[array.Length % 100 == 0 ? array.Length / 100 : array.Length / 100 + 1];
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
                            ClientLocalDB.Current.SaveTopicInfo(RuTrackerOrg.Current.GetTopicsInfo(intList.ToArray()),
                                true);
                            num1 += new Decimal(1000, 0, 0, false, 1) / (categoryList.Count * intListArray.Length);
                            backgroundWorker.ReportProgress((int) num1);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Ошибка при обновлении топиков: " + ex.Message);
                        logger.Debug(ex);
                    }

                    ClientLocalDB.Current.SaveUsers(RuTrackerOrg.Current.GetUsers(ClientLocalDB.Current.GetNoUsers()));
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                logger.Debug(ex);
                int num2 = (int) MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand,
                    MessageBoxDefaultButton.Button1);
            }

            logger.Info("Завершена задача по обновлению топиков.");
        }

        public static void bwUpdateKeepersByAllCategories(object sender, DoWorkEventArgs e)
        {
            logger.Info("Запущена задача по обновлению информации о хранителях...");
            BackgroundWorker backgroundWorker = sender as BackgroundWorker;
            Decimal num1 = new Decimal(0, 0, 0, false, 1);
            backgroundWorker.ReportProgress((int) num1);
            try
            {
                ClientLocalDB.Current.ClearKeepers();
                int[] categories = ClientLocalDB.Current.GetCategoriesEnable().Select(x => x.CategoryID).OrderBy(x => x)
                    .ToArray();
                var array = ClientLocalDB.Current.GetReports(new int?()).Where(x =>
                {
                    if (x.Key.Item2 == 0 && x.Key.Item1 != 0 && !String.IsNullOrWhiteSpace(x.Value.Item1))
                        return categories.Any(z => z == x.Key.Item1);
                    return false;
                }).Select(x =>
                {
                    string[] strArray = x.Value.Item1.Split('=');
                    if (strArray.Length == 3)
                        return new
                        {
                            TopicID = Int32.Parse(strArray[2]),
                            CategoryID = x.Key.Item1
                        };
                    if (strArray.Length == 2)
                        return new
                        {
                            TopicID = Int32.Parse(strArray[1]),
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
                    logger.Trace("\t" + data.CategoryID);
                    ClientLocalDB.Current.SaveKeepOtherKeepers(ruTrackerOrg.GetKeeps(data.TopicID, data.CategoryID));
                    num1 += new Decimal(1000, 0, 0, false, 1) / array.Count();
                    backgroundWorker.ReportProgress((int) num1);
                }

                ClientLocalDB.Current.CreateReportByRootCategories();
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                logger.Debug(ex);
                int num2 = (int) MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand,
                    MessageBoxDefaultButton.Button1);
            }

            logger.Info("Завершена задача по обновлению информации о хранителях.");
        }

        public static void bwRuningAndStopingDistributions(object sender, DoWorkEventArgs e)
        {
            logger.Info("Запущена задача по запуску/остановке раздач в торрент-клиентах...");
            BackgroundWorker backgroundWorker = sender as BackgroundWorker;
            object obj = e.Argument;
            Decimal num = new Decimal(0, 0, 0, false, 1);
            backgroundWorker.ReportProgress((int) num);
            Dictionary<int, int> countSeedersBycategories = new Dictionary<int, int>();
            try
            {
                IEnumerable<TopicInfo> inner = ClientLocalDB.Current.GetTopicsByCategory(-1).Where(x => !x.IsBlackList);
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
                            logger.Info("\t Кол-во раздач в торрент-клиенте \"" + torrentClientInfo.Name + "\": " +
                                        allTorrentHash.Count);
                            ClientLocalDB.Current.SetTorrentClientHash(allTorrentHash);
                            var list = allTorrentHash.Join(inner, c => c.Hash, a => a.Hash, (c, a) => new
                            {
                                c = c,
                                a = a
                            }).Where(_param1 => _param1.c.IsRun.HasValue).Select(_param1 => new
                            {
                                Hash = _param1.a.Hash,
                                IsRun = _param1.c.IsRun.Value,
                                IsPause = _param1.c.IsPause,
                                Seeders = _param1.a.Seeders,
                                MaxSeeders = countSeedersBycategories.ContainsKey(_param1.a.CategoryID)
                                    ? new int?(countSeedersBycategories[_param1.a.CategoryID])
                                    : new int?()
                            }).ToList();
                            string[] array1 = list.Where(x =>
                            {
                                if (x.IsRun && x.MaxSeeders.HasValue)
                                    return x.Seeders > x.MaxSeeders.Value + 1;
                                return false;
                            }).Select(x => x.Hash).ToArray();
                            logger.Info("\t Кол-во раздач в торрент-клиенте \"" + torrentClientInfo.Name +
                                        "\" которые требуется остановить: " + array1.Length + ". Останавливаем...");
                            List<string>[] stringListArray1 =
                                new List<string>[array1.Length / 50 + (array1.Length % 50 != 0 ? 1 : 0)];
                            for (int index1 = 0; index1 < array1.Length; ++index1)
                            {
                                int index2 = index1 / 50;
                                if (stringListArray1[index2] == null)
                                    stringListArray1[index2] = new List<string>();
                                stringListArray1[index2].Add(array1[index1]);
                            }

                            if (stringListArray1.Length == 0)
                            {
                                num += new Decimal(1000, 0, 0, false, 1) / (2 * torrentClients.Count);
                                backgroundWorker.ReportProgress((int) num);
                            }

                            foreach (List<string> stringList in stringListArray1)
                            {
                                if (stringList != null)
                                    torrentClient.DistributionStop(stringList);
                                num += new Decimal(1000, 0, 0, false, 1) /
                                       (2 * torrentClients.Count * stringListArray1.Length);
                                backgroundWorker.ReportProgress((int) num);
                            }

                            string[] array2 = list.Where(x =>
                            {
                                if ((!x.IsRun || x.IsPause) && x.MaxSeeders.HasValue)
                                    return x.Seeders <= x.MaxSeeders.Value;
                                return false;
                            }).Select(x => x.Hash).ToArray();
                            List<string>[] stringListArray2 =
                                new List<string>[array2.Length / 50 + (array2.Length % 50 != 0 ? 1 : 0)];
                            logger.Info("\t Кол-во раздач в торрент-клиенте \"" + torrentClientInfo.Name +
                                        "\" которые требуется запустить: " + array2.Length + ". Запускаем...");
                            for (int index1 = 0; index1 < array2.Length; ++index1)
                            {
                                int index2 = index1 / 50;
                                if (stringListArray2[index2] == null)
                                    stringListArray2[index2] = new List<string>();
                                stringListArray2[index2].Add(array2[index1]);
                            }

                            if (stringListArray2.Length == 0)
                            {
                                num += new Decimal(1000, 0, 0, false, 1) / (2 * torrentClients.Count);
                                backgroundWorker.ReportProgress((int) num);
                            }

                            foreach (List<string> stringList in stringListArray2)
                            {
                                if (stringList != null)
                                    torrentClient.DistributionStart(stringList);
                                num += new Decimal(1000, 0, 0, false, 1) /
                                       (2 * torrentClients.Count * stringListArray2.Length);
                                backgroundWorker.ReportProgress((int) num);
                            }
                        }
                        else
                            num += new Decimal(1000, 0, 0, false, 1) / torrentClients.Count;

                        backgroundWorker.ReportProgress((int) num);
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("Не удалось запустить/остановить раздачи на клиенте \"" + torrentClientInfo.Name +
                                    "\": " + ex.Message);
                        logger.Debug(ex);
                        num += new Decimal(1000, 0, 0, false, 1) / torrentClients.Count;
                    }

                    backgroundWorker.ReportProgress((int) num);
                }

                logger.Info("Строим отчеты о хранимом...");
                Reports.CreateReports();
                logger.Info("Отчеты о хранимом построены.");
            }
            catch (Exception ex)
            {
                logger.Warn("Произошла критическая ошибка при запуске/остановке раздач");
                logger.Debug(ex);
            }

            logger.Info("Завершена задача по запуску/остановке раздач в торрент-клиентах.");
            logger.Debug(String.Format("Размер ОЗУ 1: {0}", GC.GetTotalMemory(false)));
            GC.Collect(2);
            logger.Debug(String.Format("Размер ОЗУ 2: {0}", GC.GetTotalMemory(false)));
        }

        public static void bwCreateReportsTorrentClients(object sender, DoWorkEventArgs e)
        {
            List<TorrentClientInfo> torrentClients = ClientLocalDB.Current.GetTorrentClients();
            IEnumerable<TopicInfo> inner = ClientLocalDB.Current.GetTopicsByCategory(-1).Where(x => !x.IsBlackList);
            logger.Info("Строим отчет о статистике в торрент-клиенте...");
            StringBuilder stringBuilder = new StringBuilder();
            Dictionary<int, Category> dictionary =
                ClientLocalDB.Current.GetCategories().ToDictionary(x => x.CategoryID, x => x);
            int num1 = Math.Max(dictionary.Count == 0 ? 20 : dictionary.Values.Max(x => x.FullName.Length),
                torrentClients.Count == 0 ? 20 : torrentClients.Max(x => x.Name.Length));
            string empty = String.Empty;
            for (int index = 0; index < num1; ++index)
                empty += "*";
            BackgroundWorker backgroundWorker = sender as BackgroundWorker;
            Decimal num2 = new Decimal(0, 0, 0, false, 1);
            backgroundWorker.ReportProgress((int) num2);
            foreach (TorrentClientInfo torrentClientInfo in torrentClients)
            {
                logger.Debug("\t" + torrentClientInfo.Name + "...");
                try
                {
                    stringBuilder.AppendLine(empty);
                    stringBuilder.AppendFormat("*\t{0}\r\n", torrentClientInfo.Name);
                    stringBuilder.AppendLine(empty);
                    ITorrentClient torrentClient = torrentClientInfo.Create();
                    if (torrentClient != null)
                    {
                        var array1 = torrentClient.GetAllTorrentHash().GroupJoin(inner, t => t.Hash, b => b.Hash,
                            (t, bt) => new
                            {
                                t = t,
                                bt = bt
                            }).SelectMany(_param1 => _param1.bt.DefaultIfEmpty(), (_param1, b) =>
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
                        stringBuilder.AppendFormat("\tВсего:\t\t{0,6} шт. ({1})\r\n", array1.Sum(x => x.Count),
                            TopicInfo.sizeToString(array1.Sum(x => x.Size)));
                        stringBuilder.AppendFormat("\tРаздаются:\t{0,6} шт. ({1})\r\n",
                            array1.Where(x => x.IsRun == 1).Sum(x => x.Count),
                            TopicInfo.sizeToString(array1.Where(x => x.IsRun == 1).Sum(x => x.Size)));
                        stringBuilder.AppendFormat("\tОстановлены:\t{0,6} шт. ({1})\r\n",
                            array1.Where(x => x.IsRun == 0).Sum(x => x.Count),
                            TopicInfo.sizeToString(array1.Where(x => x.IsRun == 0).Sum(x => x.Size)));
                        stringBuilder.AppendFormat("\tПрочие:\t\t{0,6} шт. ({1})\r\n",
                            array1.Where(x => x.IsRun == -1).Sum(x => x.Count),
                            TopicInfo.sizeToString(array1.Where(x => x.IsRun == -1).Sum(x => x.Size)));
                        stringBuilder.AppendFormat("\tНеизвестные:\t{0,6} шт. ({1})\r\n",
                            array1.Where(x => x.CategoryID == -1).Sum(x => x.Count),
                            TopicInfo.sizeToString(array1.Where(x => x.CategoryID == -1).Sum(x => x.Size)));
                        stringBuilder.AppendLine();
                        stringBuilder.AppendFormat("\tПо кол-ву сидов:\r\n");
                        foreach (var data in array1.GroupBy(x => x.Seeders).Select(x => new
                        {
                            Seeders = x.Key,
                            Count = x.Sum(z => z.Count),
                            Size = x.Sum(z => z.Size)
                        }).OrderBy(x => x.Seeders))
                            stringBuilder.AppendFormat("\t{2}:\t\t{0,5} шт. ({1})\r\n", data.Count,
                                TopicInfo.sizeToString(data.Size), data.Seeders);
                        stringBuilder.AppendLine();
                        foreach (int num3 in array1.Select(x => x.CategoryID).Distinct().OrderBy(x => x).ToArray())
                        {
                            int c = num3;
                            var array2 = array1.Where(x => x.CategoryID == c).ToArray();
                            string str = "Неизвестные";
                            if (dictionary.ContainsKey(c))
                                str = dictionary[c].FullName;
                            stringBuilder.AppendFormat("{0}:\r\n", str);
                            stringBuilder.AppendFormat("\tВсего:\t\t{0,5} шт. ({1})\r\n", array2.Sum(x => x.Count),
                                TopicInfo.sizeToString(array2.Sum(x => x.Size)));
                            stringBuilder.AppendFormat("\tРаздаются:\t{0,5} шт. ({1})\r\n",
                                array2.Where(x => x.IsRun == 1).Sum(x => x.Count),
                                TopicInfo.sizeToString(array2.Where(x => x.IsRun == 1).Sum(x => x.Size)));
                            stringBuilder.AppendFormat("\tОстановлены:\t{0,5} шт. ({1})\r\n",
                                array2.Where(x => x.IsRun == 0).Sum(x => x.Count),
                                TopicInfo.sizeToString(array2.Where(x => x.IsRun == 0).Sum(x => x.Size)));
                            stringBuilder.AppendFormat("\tПрочие:\t\t{0,5} шт. ({1})\r\n",
                                array2.Where(x => x.IsRun == -1).Sum(x => x.Count),
                                TopicInfo.sizeToString(array2.Where(x => x.IsRun == -1).Sum(x => x.Size)));
                        }

                        stringBuilder.AppendLine();
                    }
                }
                catch (Exception ex)
                {
                    stringBuilder.AppendFormat("Ошибка: {0}\r\n\r\n\r\n", ex.Message);
                }

                stringBuilder.AppendLine();
                num2 += new Decimal(1000, 0, 0, false, 1) / torrentClients.Count();
                if (num2 <= new Decimal(100))
                    backgroundWorker.ReportProgress((int) num2);
            }

            Dictionary<int, Dictionary<int, string>> reports = new Dictionary<int, Dictionary<int, string>>();
            reports.Add(0, new Dictionary<int, string>());
            reports[0].Add(1, stringBuilder.ToString());
            try
            {
                ClientLocalDB.Current.SaveReports(reports);
                logger.Info("Отчет о статистике в торрент-клиенте построен.");
            }
            catch (Exception ex)
            {
                logger.Error("Произошла ошибка при сохранении отчета в базу данных: " + ex.Message);
                logger.Trace(ex.StackTrace);
            }
        }

        public static void bwCreateUnknownTorrentsReport(object sender, DoWorkEventArgs e)
        {
            List<TorrentClientInfo> torrentClients = ClientLocalDB.Current.GetTorrentClients();
            IEnumerable<TopicInfo> inner = ClientLocalDB.Current.GetTopicsByCategory(-1).Where(x => !x.IsBlackList);
            logger.Info("Строим отчет о статистике в торрент-клиенте...");
            StringBuilder stringBuilder = new StringBuilder();
            Dictionary<int, Category> dictionary =
                ClientLocalDB.Current.GetCategories().ToDictionary(x => x.CategoryID, x => x);
            int num1 = Math.Max(dictionary.Count == 0 ? 20 : dictionary.Values.Max(x => x.FullName.Length),
                torrentClients.Count == 0 ? 20 : torrentClients.Max(x => x.Name.Length));
            string empty = String.Empty;
            for (int index = 0; index < num1; ++index)
                empty += "*";
            BackgroundWorker backgroundWorker = sender as BackgroundWorker;
            Decimal num2 = new Decimal(0, 0, 0, false, 1);
            backgroundWorker.ReportProgress((int) num2);
            var listUnknown = new StringBuilder();
            listUnknown.AppendLine("Клиент;Метка;Торрент;Размер");
            foreach (TorrentClientInfo torrentClientInfo in torrentClients)
            {
                logger.Debug("\t" + torrentClientInfo.Name + "...");
                try
                {
                    ITorrentClient torrentClient = torrentClientInfo.Create();
                    if (torrentClient != null)
                    {
                        var array1 = torrentClient.GetAllTorrentHash().GroupJoin(inner, t => t.Hash, b => b.Hash,
                            (t, bt) => new
                            {
                                t = t,
                                bt = bt
                            }).SelectMany(_param1 => _param1.bt.DefaultIfEmpty(), (_param1, b) =>
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
                                Name = _param1.t.TorrentName,
                                Size = size,
                                IsRun = num4,
                                IsPause = num5 != 0,
                                Seeders = num6,
                                Label = _param1.t.Label
                            };
                        }).GroupBy(x => new
                        {
                            CategoryID = x.CategoryID,
                            Name = x.Name,
                            IsRun = x.IsRun,
                            IsPause = x.IsPause,
                            Seeders = x.Seeders,
                            Label = x.Label
                        }).Select(x => new
                        {
                            CategoryID = x.Key.CategoryID,
                            Name = x.Key.Name,
                            IsRun = x.Key.IsRun,
                            IsPause = x.Key.IsPause,
                            Size = x.Sum(y => y.Size),
                            Count = x.Count(),
                            Seeders = x.Key.Seeders,
                            Label = x.Key.Label
                        }).ToArray();
                        var countUnknown = array1.Where(x => x.CategoryID == -1).Sum(x => x.Count);
                        foreach (var info in array1.Where(x => x.CategoryID == -1).ToList())
                        {
                            listUnknown.AppendLine(String.Join(";", torrentClientInfo.Name, info.Label, info.Name,
                                TopicInfo.sizeToString(info.Size)));
                        }
                    }
                }
                catch (Exception ex)
                {
                    listUnknown.AppendFormat("Ошибка: {0}\r\n\r\n\r\n", ex.Message);
                }

                num2 += new Decimal(1000, 0, 0, false, 1) / torrentClients.Count();
                if (num2 <= new Decimal(100))
                    backgroundWorker.ReportProgress((int) num2);
            }

            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.AddExtension = true;
            saveFileDialog.DefaultExt = "csv";
            saveFileDialog.Filter = @".csv|CSV файл|.txt|Текстовый документ";
            saveFileDialog.OverwritePrompt = true;
            var form = (MainForm) e.Argument;
            form.Invoke((MethodInvoker) delegate
            {
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var file = saveFileDialog.OpenFile();
                    var writer = new StreamWriter(file, Encoding.UTF8);
                    writer.Write(listUnknown.ToString());
                    writer.Flush();
                    file.Close();
                }
            });
        }

        public static void bwSendReports(object sender, DoWorkEventArgs e)
        {
            logger.Info("Запущена задача на отправку отчетов на форум....");
            Decimal num1 = new Decimal(0, 0, 0, false, 1);
            BackgroundWorker backgroundWorker = sender as BackgroundWorker;
            Tuple<string, string>[] array = ClientLocalDB.Current.GetReports(new int?())
                .Where(x => !String.IsNullOrWhiteSpace(x.Value.Item1)).OrderBy(x => x.Key.Item1).Select(x => x.Value)
                .Where(x => x.Item1.Split('=').Length == 3).ToArray();
            if (array.Where(x => !String.IsNullOrWhiteSpace(x.Item1)).Count() == 0)
            {
                int num2 = (int) MessageBox.Show("Нет ни одного отчета c указанным URL для отправки на форум");
            }
            else
            {
                foreach (Tuple<string, string> tuple in array.Where(x => !String.IsNullOrWhiteSpace(x.Item1)))
                {
                    logger.Info(tuple.Item1);
                    try
                    {
                        RuTrackerOrg.Current.SendReport(tuple.Item1, tuple.Item2);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.Message);
                        logger.Debug(ex);
                        int num3 = (int) MessageBox.Show("Произошла ошибка при отправке отчетов:\r\n" + ex.Message,
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
                    }

                    num1 += new Decimal(1000, 0, 0, false, 1) / array.Length;
                    backgroundWorker.ReportProgress((int) num1);
                }

                logger.Info("Завершена задача на отправку отчетов на форум.");
            }
        }
    }
}