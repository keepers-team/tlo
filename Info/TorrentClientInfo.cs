using System;
using TLO.Clients;

namespace TLO.Info
{
    internal class TorrentClientInfo
    {
        public TorrentClientInfo()
        {
            UID = Guid.NewGuid();
            Name = string.Empty;
            Type = "uTorrent";
            ServerName = string.Empty;
            ServerPort = 999;
            UserName = string.Empty;
            UserPassword = string.Empty;
            LastReadHash = new DateTime(2000, 1, 1);
        }

        public Guid UID { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public string ServerName { get; set; }

        public int ServerPort { get; set; }

        public string UserName { get; set; }

        public string UserPassword { get; set; }

        public DateTime LastReadHash { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public ITorrentClient Create()
        {
            ITorrentClient torrentClient = null;
            if (Type == "uTorrent")
                torrentClient = new UTorrentClient(ServerName, ServerPort, UserName, UserPassword);
            else if (Type == "Transmission")
                torrentClient = new TransmissionClient(ServerName, ServerPort, UserName, UserPassword);
            else if (Type == "Vuze (Vuze Web Remote)")
                torrentClient = new TransmissionClient(ServerName, ServerPort, UserName, UserPassword);
            return torrentClient;
        }
    }
}