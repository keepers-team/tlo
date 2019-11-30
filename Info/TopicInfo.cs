using System;
using System.Collections.Generic;
using System.Web;

namespace TLO.Info
{
    internal class TopicInfo : ICloneable
    {
        public int TopicID { get; set; }

        public int Seeders { get; set; }

        public int Leechers { get; set; }

        public string Hash { get; set; }

        public int CategoryID { get; set; }

        public string Name => HttpUtility.HtmlDecode(Name2);

        public string Name2 { get; set; }

        public string Label { get; set; }

        public string TorrentName { get; set; }

        public List<string> Files { get; set; }

        public int Status { get; set; }

        public long Size { get; set; }

        public DateTime RegTime { get; set; }

        public decimal? AvgSeeders { get; set; }

        public bool IsKeeper { get; set; }

        public bool IsKeep { get; set; }

        public bool IsDownload { get; set; }

        public bool IsBlackList { get; set; }

        public bool IsSelected { get; set; }

        public string Alternative => ">>>>";

        public bool? IsRun { get; set; }

        public bool IsPause { get; set; }

        public bool[] TorrentClientStatus { get; set; }

        public decimal PercentComplite { get; set; }

        public bool Checked { get; set; }

        public string SizeToString => sizeToString(Size);

        public string StatusToString
        {
            get
            {
                switch (Status)
                {
                    case 0:
                        return "*";
                    case 1:
                        return "x";
                    case 2:
                        return "√";
                    case 3:
                        return "?";
                    case 4:
                        return "!";
                    case 5:
                        return "D";
                    case 6:
                        return "©";
                    case 7:
                        return "∑";
                    case 8:
                        return "#";
                    case 9:
                        return "%";
                    case 10:
                        return "T";
                    case 11:
                        return "∏";
                    default:
                        return "-";
                }
            }
        }

        public string RegTimeToString => RegTime.ToString("dd.MM.yyyy");

        public int PosterID { get; set; }

        public bool IsPoster { get; set; }

        public int? KeeperCount { get; set; }

        public object Clone()
        {
            var ti = new TopicInfo();

            foreach (var prop in Type.GetType("TLO.Info.TopicInfo").GetProperties())
            {
                ti.CategoryID = CategoryID;
                Console.WriteLine("Property is " + prop.Name);
                if (prop.CanWrite) prop.SetValue(ti, prop.GetValue(this));
            }

            return ti;
        }

        public static string sizeToString(long size)
        {
            if (size >= new decimal(int.MinValue, 2, 0, false, 1))
                return Math.Round(size / new decimal(int.MinValue, 2, 0, false, 1), 2) + " GB";
            if (size >= new decimal(10485760, 0, 0, false, 1))
                return Math.Round(size / new decimal(10485760, 0, 0, false, 1), 2) + " MB";
            if (size >= new decimal(10240, 0, 0, false, 1))
                return Math.Round(size / new decimal(10240, 0, 0, false, 1), 2) + " KB";
            return Math.Round((decimal) size, 2) + " B";
        }
    }
}