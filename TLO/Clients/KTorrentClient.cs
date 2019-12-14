﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using NLog;
using TLO.Info;

namespace TLO.Clients
{
    internal class KTorrentClient : ITorrentClient
    {
        private static Logger? _logger;
        private string _serverName;
        private int _serverPort;

        public KTorrentClient(string serverName, int port, string userName, string userPass)
        {
            if (_logger == null)
                _logger = LogManager.GetLogger("TransmissionClient");
            var webClient = new TloWebClient(Encoding.UTF8,
                "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:36.0) Gecko/20100101 Firefox/36.0",
                "application/json, text/javascript, */*; q=0.01", true);
            webClient.Encoding = Encoding.UTF8;
            var svcCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(userName + ":" + userPass));
            webClient.Headers.Add("Authorization", "Basic " + svcCredentials);
            _serverName = serverName;
            _serverPort = port;
            try
            {
                Ping();
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("Имя сервера: {0}; Порт сервера: {1}", serverName, port));
                MessageBox.Show(@"Ошибка подключения к торрент клиенту: " + ex.Message);
            }
        }

        public List<TopicInfo> GetAllTorrentHash()
        {
            return new List<TopicInfo>();
        }

        public IEnumerable<string> GetFiles(TopicInfo topic)
        {
            return new List<string>();
        }

        public void DistributionStop(IEnumerable<string> data)
        {
        }

        public void DistributionPause(IEnumerable<string> data)
        {
        }

        public void DistributionStart(IEnumerable<string> data)
        {
        }

        public bool Ping()
        {
            return true;
        }

        public bool SetDefaultFolder(string dir)
        {
            return true;
        }

        public bool SetDefaultLabel(string label)
        {
            return true;
        }

        public string GetDefaultFolder()
        {
            return string.Empty;
        }

        public void SendTorrentFile(string path, string file)
        {
        }

        public void SendTorrentFile(string path, string filename, byte[] fdata)
        {
        }

        public string[]? GetTrackers(string hash)
        {
            return null;
        }

        public bool SetTrackers(string hash, string[] trackers)
        {
            return true;
        }

        public bool SetLabel(string hash, string label)
        {
            return true;
        }

        public bool SetLabel(IEnumerable<string> hash, string label)
        {
            return true;
        }
    }
}