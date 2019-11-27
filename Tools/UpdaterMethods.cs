using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TLO.local.Tools
{
    internal static class UpdaterMethods
    {
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
            UpdateSeedersByCategories(new List<Category>()
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
                ClientLocalDB.Current.SaveTopicInfo(
                    RuTrackerOrg.Current.GetTopicsInfo(RuTrackerOrg.Current.GetTopicsStatus(category.CategoryID)
                        .Select(x => x[0]).Distinct().ToArray()), true);
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
                ClientLocalDB.Current.SaveTopicInfo(
                    RuTrackerOrg.Current.GetTopicsInfo(RuTrackerOrg.Current.GetTopicsStatus(category.CategoryID)
                        .Select(x => x[0]).Distinct().ToArray()), true);
                pBar.PerformStep();
            }
        }

        public static void UpdateTopicsByCategory(Category category)
        {
            if (category == null)
                return;
            UpdateTopicsByCategories(new List<Category>()
            {
                category
            });
        }

        public static void UpdateHashFromClients(List<TorrentClientInfo> clients = null)
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

        public static void UpdateHashFromClients(TorrentClientInfo client)
        {
            if (client == null)
                return;
            UpdateHashFromClients(new List<TorrentClientInfo>()
            {
                client
            });
        }

        internal static void UpdateHashFromClients(ProgressBar pBar)
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

        public static void UpdateHashFromClients(Guid uid)
        {
            TorrentClientInfo client = ClientLocalDB.Current.GetTorrentClients()
                .Where(x => x.UID == uid).FirstOrDefault();
            if (client == null)
                return;
            UpdateHashFromClients(client);
            Reports.CreateReportByRootCategories();
        }
    }
}