using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace TLO.local
{
    internal static class SenderMethods
    {
        public static void SendTorrentFileToTorrentClient(List<TopicInfo> topics, Category category)
        {
            TorrentClientInfo torrentClientInfo = ClientLocalDB.Current.GetTorrentClients()
                .Where(x => x.UID == category.TorrentClientUID).FirstOrDefault();
            if (torrentClientInfo == null)
                return;
            ITorrentClient torrentClient1 = torrentClientInfo.Create();
            if (torrentClient1 == null)
                return;
            if (String.IsNullOrWhiteSpace(category.Folder))
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
                        buffer1 = RuTrackerOrg.Current.DownloadTorrentFile(topic.TopicID);
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

                    string filename = String.Format("[rutracker.org].t{0}.torrent", topic.TopicID);
                    byte[] fdata = buffer1;
                    torrentClient3.SendTorrentFile(path, filename, fdata);
                    if (category.IsSaveTorrentFiles)
                    {
                        if (!Directory.Exists(category.FolderTorrentFile))
                            Directory.CreateDirectory(category.FolderTorrentFile);
                        using (FileStream fileStream = File.Create(Path.Combine(category.FolderTorrentFile,
                            String.Format("[rutracker.org].t{0}.torrent", topic.TopicID))))
                            fileStream.Write(buffer1, 0, buffer1.Count());
                    }

                    if (category.IsSaveWebPage)
                    {
                        Thread.Sleep(500);
                        byte[] buffer2 = RuTrackerOrg.Current.DownloadWebPages(String.Format(
                            "https://{1}/forum/viewtopic.php?t={0}", topic.TopicID, Settings.Current.HostRuTrackerOrg));
                        if (!Directory.Exists(category.FolderSavePageForum))
                            Directory.CreateDirectory(category.FolderSavePageForum);
                        using (FileStream fileStream = File.Create(Path.Combine(category.FolderSavePageForum,
                            String.Format("[rutracker.org].t{0}.html", topic.TopicID))))
                            fileStream.Write(buffer2, 0, buffer2.Count());
                    }

                    Thread.Sleep(500);
                }
            }
        }

        public static void SendTorrentFileToTorrentClient(TopicInfo topic, Category category)
        {
            if (topic == null || category == null)
                return;
            SendTorrentFileToTorrentClient(new List<TopicInfo>()
            {
                topic
            }, category);
        }

        public static void SendReportToForum()
        {
            foreach (KeyValuePair<Tuple<int, int>, Tuple<string, string>> report in ClientLocalDB.Current.GetReports(
                new int?()))
            {
                if (!String.IsNullOrWhiteSpace(report.Value.Item1))
                    RuTrackerOrg.Current.SendReport(report.Value.Item1, report.Value.Item2);
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
                if (!String.IsNullOrWhiteSpace(keyValuePair.Value.Item1))
                    RuTrackerOrg.Current.SendReport(keyValuePair.Value.Item1, keyValuePair.Value.Item2);
                pBar.PerformStep();
            }
        }
    }
}