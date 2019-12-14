using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using TLO.Clients;
using TLO.Info;

namespace TLO
{
    internal static class SenderMethods
    {
        public static void SendTorrentFileToTorrentClient(List<TopicInfo> topics, Category category)
        {
            var torrentClientInfo = ClientLocalDb.Current.GetTorrentClients()
                .Where(x => x.UID == category.TorrentClientUID).FirstOrDefault();
            if (torrentClientInfo == null)
                return;
            var torrentClient1 = torrentClientInfo.Create();
            if (torrentClient1 == null)
                return;
            if (string.IsNullOrWhiteSpace(category.Folder))
                throw new Exception("В разделе не указан каталог для загрузки");
            foreach (var topic in topics)
                if (topic.Status != 7 && topic.Status != 4)
                {
                    int topicId;
                    if (category.CreateSubFolder != 0)
                    {
                        if (category.CreateSubFolder != 1)
                            throw new Exception("Не поддерживается указаный метод создания подкаталога");
                        var torrentClient2 = torrentClient1;
                        var folder = category.Folder;
                        topicId = topic.TopicID;
                        var path2 = topicId.ToString();
                        var dir = Path.Combine(folder, path2);
                        torrentClient2.SetDefaultFolder(dir);
                    }

                    var buffer1 = new byte[0];
                    if (buffer1.Length == 0)
                        buffer1 = RuTrackerOrg.Current.DownloadTorrentFile(topic.TopicID);
                    if (buffer1 == null)
                        break;
                    var torrentClient3 = torrentClient1;
                    string path;
                    if (category.CreateSubFolder != 1)
                    {
                        path = category.Folder;
                    }
                    else
                    {
                        var folder = category.Folder;
                        topicId = topic.TopicID;
                        var path2 = topicId.ToString();
                        path = Path.Combine(folder, path2);
                    }

                    var filename = string.Format("[rutracker.org].t{0}.torrent", topic.TopicID);
                    var fdata = buffer1;
                    torrentClient3.SendTorrentFile(path, filename, fdata);
                    if (category.IsSaveTorrentFiles)
                    {
                        if (!Directory.Exists(category.FolderTorrentFile))
                            Directory.CreateDirectory(category.FolderTorrentFile);
                        using (var fileStream = File.Create(Path.Combine(category.FolderTorrentFile,
                            string.Format("[rutracker.org].t{0}.torrent", topic.TopicID))))
                        {
                            fileStream.Write(buffer1, 0, buffer1.Count());
                        }
                    }

                    if (category.IsSaveWebPage)
                    {
                        Thread.Sleep(500);
                        var buffer2 = RuTrackerOrg.Current.DownloadWebPages(string.Format(
                            "https://{1}/forum/viewtopic.php?t={0}", topic.TopicID, Settings.Current.HostRuTrackerOrg));
                        if (!Directory.Exists(category.FolderSavePageForum))
                            Directory.CreateDirectory(category.FolderSavePageForum);
                        using (var fileStream = File.Create(Path.Combine(category.FolderSavePageForum,
                            string.Format("[rutracker.org].t{0}.html", topic.TopicID))))
                        {
                            fileStream.Write(buffer2, 0, buffer2.Count());
                        }
                    }

                    Thread.Sleep(500);
                }
        }

        public static void SendTorrentFileToTorrentClient(TopicInfo topic, Category category)
        {
            if (topic == null || category == null)
                return;
            SendTorrentFileToTorrentClient(new List<TopicInfo>
            {
                topic
            }, category);
        }

        public static void SendReportToForum()
        {
            foreach (var report in ClientLocalDb.Current.GetReports(
                new int?()))
                if (!string.IsNullOrWhiteSpace(report.Value.Item1))
                    RuTrackerOrg.Current.SendReport(report.Value.Item1, report.Value.Item2);
        }

        public static void SendReportToForum(ProgressBar pBar)
        {
            var reports = ClientLocalDb.Current.GetReports(new int?());
            pBar.Visible = true;
            pBar.Minimum = 1;
            pBar.Maximum = reports.Count;
            pBar.Value = 1;
            pBar.Step = 1;
            foreach (var keyValuePair in reports)
            {
                if (!string.IsNullOrWhiteSpace(keyValuePair.Value.Item1))
                    RuTrackerOrg.Current.SendReport(keyValuePair.Value.Item1, keyValuePair.Value.Item2);
                pBar.PerformStep();
            }
        }
    }
}