using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Stubble.Core;
using Stubble.Core.Builders;

namespace TLO.local
{
    internal static class Reports
    {
        private static StubbleVisitorRenderer _stubble;

        public static StubbleVisitorRenderer Stubble
        {
            get
            {
                if (_stubble != null)
                {
                    return _stubble;
                }

                var stubble = new StubbleBuilder()
                    .Configure(settings =>
                    {
                        settings.SetIgnoreCaseOnKeyLookup(true);
                        settings.SetMaxRecursionDepth(512);
                    })
                    .Build();

                return _stubble = stubble;
            }
        }

        public static void CreateReports()
        {
            ClientLocalDB.Current.ClearReports();

            var categories = ClientLocalDB.Current.GetCategoriesEnable();
            var currReports = ClientLocalDB.Current.GetReports(new int?());
            var reports = new Dictionary<int, Dictionary<int, string>>();
            var allStatistics = ClientLocalDB.Current
                .GetStatisticsByAllUsers()
                .Where(x => !string.IsNullOrWhiteSpace(x.Item2))
                .ToArray();

            var statistics = allStatistics
                .Where(x => x.Item2 == Settings.Current.KeeperName)
                .ToArray();
            var catIds = categories.Select(x => x.CategoryID).ToArray();

            var summaryTopicsAmount = statistics
                .Where(x => catIds.Contains(x.Item1))
                .Sum(x => x.Item3);
            var summaryTopicsSize = statistics
                .Where(x => catIds.Contains(x.Item1))
                .Sum(x => x.Item4);

            var sb = new StringBuilder();

            var dataHash = new Dictionary<string, object>()
            {
                {"today", DateTime.Now.ToString("dd.MM.yyyy")},
                {"summary_topics_count", summaryTopicsAmount},
                {"summary_topics_size", summaryTopicsSize},
            };

            var summaryReportTemplate = @"
Актуально на: {0}\r\n\r\n
Общее количество хранимых раздач: {0} шт.\r\n
Общий вес хранимых раздач: {0:0.00} GB\r\n
[hr]
";
//            Stubble.Render()

            foreach (var category in categories.OrderBy(x => x.FullName))
            {
                var st =
                    statistics.FirstOrDefault(x => x.Item1 == category.CategoryID) ??
                    new Tuple<int, string, int, decimal>(category.CategoryID, "<->", 0, decimal.Zero);

                if (!currReports.ContainsKey(new Tuple<int, int>(st.Item1, 1))) continue;

                var url = currReports[new Tuple<int, int>(st.Item1, 1)].Item1;
                if (!string.IsNullOrWhiteSpace(url) && url.Split('=').Length > 2)
                    url = url.Split('=')[2];
                else
                    url = string.Empty;

                var topicLink = string.IsNullOrWhiteSpace(url)
                    ? ""
                    : string.Format("[url=https://rutracker.org/forum/viewtopic.php?p={0}#{0}]", url);

                var closeLink = string.IsNullOrWhiteSpace(url) ? "" : "[/url]";

                sb.AppendFormat(
                    "{0}{1}{2} - {3} шт. ({4:0.00} GB)\r\n",
                    topicLink,
                    category.FullName,
                    closeLink,
                    st.Item3,
                    st.Item4
                );
            }

            reports.Add(0, new Dictionary<int, string>());
            reports[0].Add(0, sb.ToString());

            ClientLocalDB.Current.SaveReports(reports);

            reports.Clear();

            foreach (var category in categories)
            {
                sb.Clear();

                var st = allStatistics.Where(x => x.Item1 == category.CategoryID && x.Item3 > 0 && x.Item2 != "All");
                var all = allStatistics.FirstOrDefault(x => x.Item1 == category.CategoryID && x.Item2 == "All");
                if (st.Count() != 0 && all != null)
                {
                    sb.AppendFormat(
                        "[url=viewforum.php?f={0}][color=darkgreen][b]{1}[/b][/color][/url] | [url=tracker.php?f={0}&tm=-1&o=10&s=1][color=darkgreen][b]Проверка сидов[/b][/color][/url]\r\n\r\n",
                        category.CategoryID, category.Name);
                    sb.AppendFormat("[b]Актуально на:[/b] {0:dd.MM.yyyy}\r\n\r\n", DateTime.Now);
                    sb.AppendFormat("[b]Общее количество раздач в подразделе:[/b] {0} шт.\r\n", all.Item3);
                    sb.AppendFormat("[b]Общий размер раздач в подразделе:[/b] {0:0.00} GB.\r\n", all.Item4);
                    sb.AppendFormat("[b]Количество хранителей:[/b] {0}\r\n", st.Count());
                    sb.AppendFormat("[b]Общее количество хранимых раздач:[/b] {0} шт.\r\n",
                        st.Sum(
                            x => x.Item3));
                    sb.AppendFormat("[b]Общий вес хранимых раздач:[/b] {0:0.00} GB.\r\n",
                        st.Sum(
                            x => x.Item4));
                    sb.AppendLine("[hr]");
                    var num = 0;
                    foreach (var tuple2 in st.OrderBy(x => x.Item2))
                    {
                        ++num;
                        sb.AppendFormat(
                            "[b]Хранитель {0}:[/b] [url=profile.php?mode=viewprofile&u={4}][color=darkgreen][b]{1}[/b][/color][/url] - {2} шт. ({3:0.00} GB)\r\n",
                            (object) num, (object) tuple2.Item2.Replace("<wbr>", ""), (object) tuple2.Item3,
                            (object) tuple2.Item4,
                            (object) HttpUtility.UrlEncode(tuple2.Item2.Replace("<wbr>", "").Trim()));
                    }

                    reports.Add(category.CategoryID, new Dictionary<int, string>());
                    reports[category.CategoryID].Add(0, sb.ToString());
                }
            }

            ClientLocalDB.Current.SaveReports(reports);
            reports.Clear();
            var format1 = Settings.Current.ReportTop1.Replace("%%CreateDate%%", "{0}")
                              .Replace("%%CountTopics%%", "{1}").Replace("%%SizeTopics%%", "{2}") + "\r\n";
            var format2 = Settings.Current.ReportTop2.Replace("%%CreateDate%%", "{0}")
                              .Replace("%%CountTopics%%", "{1}").Replace("%%SizeTopics%%", "{2}")
                              .Replace("%%NumberTopicsFirst%%", "{3}").Replace("%%NumberTopicsLast%%", "{4}")
                              .Replace("%%ReportLines%%", "{5}").Replace("%%Top1%%", "{6}") + "\r\n";
            var format3 = Settings.Current.ReportLine.Replace("%%ID%%", "{0}").Replace("%%Name%%", "{1}")
                .Replace("%%Size%%", "{2}").Replace("%%Status%%", "{3}").Replace("%%CountSeeders%%", "{4}")
                .Replace("%%Date%%", "{5}");
            var num1 = 115000;
            var stringBuilder2 = new StringBuilder();
            var stringBuilder3 = new StringBuilder();
            foreach (var category in categories)
            {
                var num2 = 0;
                var num3 = 0;
                var num4 = 1;
                var key = 0;
                stringBuilder2.Clear();
                stringBuilder3.Clear();
                var array3 = ClientLocalDB.Current.GetTopicsByCategory(category.CategoryID).Where(
                    x =>
                    {
                        if (x.IsKeep && (x.Seeders <= Settings.Current.CountSeedersReport ||
                                         Settings.Current.CountSeedersReport == -1))
                            return !x.IsBlackList;
                        return false;
                    }).OrderBy(x => x.Name2).ToArray();
                if (array3.Length != 0)
                {
                    reports.Add(category.CategoryID, new Dictionary<int, string>());
                    var dictionary = reports[category.CategoryID];
                    var str = string.Format(format1, DateTime.Now.ToString("dd.MM.yyyy"),
                        array3.Length,
                        TopicInfo.sizeToString(
                            array3.Sum(x => x.Size)));
                    foreach (var topicInfo in array3)
                    {
                        stringBuilder3.AppendLine(string.Format(format3, (object) topicInfo.TopicID,
                            (object) topicInfo.Name2, (object) topicInfo.SizeToString,
                            (object) topicInfo.StatusToString, (object) topicInfo.Seeders,
                            (object) topicInfo.RegTimeToString));
                        ++num2;
                        ++num3;
                        if (num2 % 10 == 0 || array3.Length <= num2)
                        {
                            if (array3.Length == num2)
                            {
                                if (num3 == 0)
                                    stringBuilder2.AppendFormat("[*={0}{1}", num4,
                                        stringBuilder3.ToString().Substring(2));
                                else
                                    stringBuilder2.AppendLine(stringBuilder3.ToString());
                            }

                            if (num1 <= stringBuilder2.Length + stringBuilder3.Length + str.Length ||
                                array3.Length <= num2)
                            {
                                ++key;
                                var num5 = num2 < array3.Length ? num2 - 10 : num2;
                                dictionary.Add(key,
                                    string.Format(format2, DateTime.Now.ToString("dd.MM.yyyy"),
                                        (object) array3.Length,
                                        (object) TopicInfo.sizeToString(
                                            array3.Sum(
                                                x => x.Size)), (object) num4, (object) num5,
                                        (object) stringBuilder2.ToString(), (object) str) +
                                    Settings.Current.ReportBottom);
                                stringBuilder2.Clear();
                                num3 = 0;
                                num4 = num5 + 1;
                                str = string.Empty;
                            }

                            if (num3 == 0)
                                stringBuilder2.AppendFormat("[*={0}{1}\r\n", num4,
                                    stringBuilder3.ToString().Substring(2));
                            else
                                stringBuilder2.AppendLine(stringBuilder3.ToString());
                            stringBuilder3.Clear();
                        }
                    }
                }
            }

            ClientLocalDB.Current.SaveReports(reports);
        }
    }
}