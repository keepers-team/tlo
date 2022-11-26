using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TLO.Clients;
using TLO.Info;

namespace TLO.Tools
{
    internal static class UpdaterMethods
    {
        public static void UpdateSeedersByCategories(List<Category> categories = null)
        {
            if (categories == null)
                categories = ClientLocalDb.Current.GetCategoriesEnable();
            if (categories == null)
                return;
            foreach (var category in categories)
                ClientLocalDb.Current.SaveStatus(RuTrackerOrg.Current.GetTopicsStatus(category.CategoryID), true);
        }

        public static void UpdateSeedersByCategory(Category category)
        {
            if (category == null)
                return;
            UpdateSeedersByCategories(new List<Category>
            {
                category
            });
        }

        public static void UpdateTopicsByCategories(List<Category> categories = null)
        {
            if (categories == null)
                categories = ClientLocalDb.Current.GetCategoriesEnable();
            if (categories == null)
                return;
            foreach (var category in categories)
                ClientLocalDb.Current.SaveTopicInfo(
                    RuTrackerOrg.Current.GetTopicsInfo(RuTrackerOrg.Current.GetTopicsStatus(category.CategoryID)
                        .Select(x => x[0]).Distinct().ToArray()), true);
        }

        public static void UpdateTopicsByCategories(ProgressBar pBar)
        {
            var categoriesEnable = ClientLocalDb.Current.GetCategoriesEnable();
            pBar.Visible = true;
            pBar.Minimum = 1;
            pBar.Maximum = categoriesEnable.Count;
            pBar.Value = 1;
            pBar.Step = 1;
            foreach (var category in categoriesEnable)
            {
                ClientLocalDb.Current.SaveTopicInfo(
                    RuTrackerOrg.Current.GetTopicsInfo(RuTrackerOrg.Current.GetTopicsStatus(category.CategoryID)
                        .Select(x => x[0]).Distinct().ToArray()), true);
                pBar.PerformStep();
            }
        }

        public static void UpdateTopicsByCategory(Category category)
        {
            if (category == null)
                return;
            UpdateTopicsByCategories(new List<Category>
            {
                category
            });
        }

        public static void UpdateHashFromClients(List<TorrentClientInfo> clients = null)
        {
            if (clients == null)
                clients = ClientLocalDb.Current.GetTorrentClients();
            if (clients == null)
                return;
            foreach (var client in clients)
            {
                var torrentClient = client.Create();
                if (torrentClient != null)
                    ClientLocalDb.Current.SetTorrentClientHash(torrentClient.GetAllTorrentHash());
            }
        }

        public static void UpdateHashFromClients(TorrentClientInfo client)
        {
            if (client == null)
                return;
            UpdateHashFromClients(new List<TorrentClientInfo>
            {
                client
            });
        }

        internal static void UpdateHashFromClients(ProgressBar pBar)
        {
            var torrentClients = ClientLocalDb.Current.GetTorrentClients();
            pBar.Visible = true;
            pBar.Minimum = 1;
            pBar.Maximum = torrentClients.Count;
            pBar.Value = 1;
            pBar.Step = 1;
            foreach (var torrentClientInfo in torrentClients)
            {
                var torrentClient = torrentClientInfo.Create();
                if (torrentClient != null)
                    ClientLocalDb.Current.SetTorrentClientHash(torrentClient.GetAllTorrentHash());

                pBar.PerformStep();
            }
        }

        public static void UpdateHashFromClients(Guid uid)
        {
            var client = ClientLocalDb.Current.GetTorrentClients()
                .Where(x => x.UID == uid).FirstOrDefault();
            if (client == null)
                return;
            UpdateHashFromClients(client);
            Reports.CreateReportByRootCategories();
        }
    }
}