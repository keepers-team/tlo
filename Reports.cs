using System;
using System.Collections.Generic;
using System.Data.SQLite;
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

            var summaryReportTemplate = Settings.Current.ReportSummaryTemplate;
            var categoriesList = new List<object>();
            var summaryReportData = new Dictionary<string, object>()
            {
                {"today", DateTime.Now.ToString("dd.MM.yyyy")},
                {"summary_topics_count", summaryTopicsAmount},
                {"summary_topics_size", summaryTopicsSize.ToString("N")},
                {"categories", categoriesList}
            };
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
                    url = null;

                categoriesList.Add(
                    new Dictionary<string, object>()
                    {
                        {
                            "url",
                            url != null ? string.Format("https://rutracker.org/forum/viewtopic.php?p={0}#{0}", url) : ""
                        },
                        {"category_name", category.FullName},
                        {"topics_count", st.Item3},
                        {"topics_size", st.Item4.ToString("N")},
                    }
                );
            }

            var summaryReportRendered = Stubble.Render(summaryReportTemplate, summaryReportData);

            reports.Add(0, new Dictionary<int, string>());
            reports[0].Add(0, summaryReportRendered);

            ClientLocalDB.Current.SaveReports(reports);

            reports.Clear();

            var headerOfReportTemplate = Settings.Current.ReportCategoryHeaderTemplate;

            foreach (var category in categories)
            {
                var st = allStatistics.Where(x => x.Item1 == category.CategoryID && x.Item3 > 0 && x.Item2 != "All");
                var all = allStatistics.FirstOrDefault(x => x.Item1 == category.CategoryID && x.Item2 == "All");
                if (st.Count() != 0 && all != null)
                {
                    var keepersList = new List<object>();
                    var reportHeader = new Dictionary<string, object>()
                    {
                        {"category_uri", "viewforum.php?f=" + category.CategoryID},
                        {"category_name", category.Name},
                        {"category_check_seeds_uri", "tracker.php?f=" + category.CategoryID + "&tm=-1&o=10&s=1"},
                        {"today", DateTime.Now.ToString("dd.MM.yyyy")},
                        {"topics_count", all.Item3},
                        {"topics_size", all.Item4.ToString("N")},
                        {"keepers_count", st.Count().ToString()},
                        {"keep_topics_count", st.Sum(x => x.Item3).ToString()},
                        {"keep_topics_size", st.Sum(x => x.Item4).ToString("N")},
                        {"keepers", keepersList}
                    };

                    var num = 0;
                    foreach (var tuple2 in st.OrderBy(x => x.Item2))
                    {
                        ++num;
                        keepersList.Add(
                            new Dictionary<string, string>()
                            {
                                {"keeper_number", num.ToString()},
                                {
                                    "keeper_profile_uri",
                                    "profile.php?mode=viewprofile&u=" +
                                    HttpUtility.UrlEncode(tuple2.Item2.Replace("<wbr>", "").Trim())
                                },
                                {"keeper_username", tuple2.Item2.Replace("<wbr>", "")},
                                {"keep_topics_count", tuple2.Item3.ToString()},
                                {"keep_topics_size", tuple2.Item4.ToString("N")},
                            }
                        );
                    }

                    var reportHeaderRendered = Stubble.Render(headerOfReportTemplate, reportHeader);

                    reports.Add(category.CategoryID, new Dictionary<int, string>());
                    reports[category.CategoryID].Add(0, reportHeaderRendered);
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

        public static void CreateReportByRootCategories()
        {
            try
            {
                // TODO вынести запросы обратно в клиент
                using (SQLiteCommand command = ClientLocalDB.Current.CreateCommand())
                {
                    ClientLocalDB.Current.GetStatisticsByAllUsers();
                    Dictionary<int, Dictionary<int, string>> reports = new Dictionary<int, Dictionary<int, string>>();
                    Dictionary<int, Tuple<string, decimal, decimal>> source1 =
                        new Dictionary<int, Tuple<string, decimal, decimal>>();
                    Dictionary<Tuple<int, string>, Tuple<string, decimal, decimal>> dictionary1 =
                        new Dictionary<Tuple<int, string>, Tuple<string, decimal, decimal>>();
                    Dictionary<Tuple<int, string, int>, Tuple<string, decimal, decimal>> dictionary2 =
                        new Dictionary<Tuple<int, string, int>, Tuple<string, decimal, decimal>>();
                    List<Tuple<int, int, string, decimal, decimal>> tupleList =
                        new List<Tuple<int, int, string, decimal, decimal>>();
                    command.CommandText = @"
SELECT c.CategoryID, c.FullName, SUM(Count)Count, SUM(Size)Size
FROM
    (
       SELECT CategoryID, ParentID FROM Category WHERE ParentID > 1000000 UNION
       SELECT c1.CategoryID, c2.ParentID FROM Category AS c1 JOIN Category AS c2 ON (c1.ParentID = c2.CategoryID) WHERE c2.ParentID > 1000000       
    ) AS t    
    JOIN Category AS c ON (t.ParentID = c.CategoryID)    
    JOIN Keeper AS k ON (k.CategoryID = t.CategoryID AND k.KeeperName <> 'All')
GROUP BY
      c.CategoryID, c.FullName
ORDER BY c.FullName";
                    using (SQLiteDataReader sqLiteDataReader = command.ExecuteReader())
                    {
                        while (sqLiteDataReader.Read())
                            source1.Add(sqLiteDataReader.GetInt32(0),
                                new Tuple<string, decimal, decimal>(sqLiteDataReader.GetString(1),
                                    sqLiteDataReader.GetDecimal(2), sqLiteDataReader.GetDecimal(3)));
                    }

                    command.CommandText = @"
SELECT c.CategoryID, c.FullName, k.KeeperName, SUM(Count)Count, SUM(Size)Size
FROM
    (
       SELECT CategoryID, ParentID FROM Category WHERE ParentID > 1000000 UNION
       SELECT c1.CategoryID, c2.ParentID FROM Category AS c1 JOIN Category AS c2 ON (c1.ParentID = c2.CategoryID) WHERE c2.ParentID > 1000000       
    ) AS t    
    JOIN Category AS c ON (t.ParentID = c.CategoryID)    
    JOIN Keeper AS k ON (k.CategoryID = t.CategoryID AND k.KeeperName <> 'All')
GROUP BY
      c.CategoryID, c.FullName, k.KeeperName
ORDER BY c.FullName, k.KeeperName";
                    using (SQLiteDataReader sqLiteDataReader = command.ExecuteReader())
                    {
                        while (sqLiteDataReader.Read())
                            dictionary1.Add(
                                new Tuple<int, string>(sqLiteDataReader.GetInt32(0), sqLiteDataReader.GetString(2)),
                                new Tuple<string, decimal, decimal>(sqLiteDataReader.GetString(1),
                                    sqLiteDataReader.GetDecimal(3), sqLiteDataReader.GetDecimal(4)));
                    }

                    command.CommandText = @"
SELECT t.ParentID, c.CategoryID, c.FullName, k.KeeperName, SUM(Count)Count, SUM(Size)Size
FROM
    (
       SELECT CategoryID, ParentID FROM Category WHERE ParentID > 1000000 UNION
       SELECT c1.CategoryID, c2.ParentID FROM Category AS c1 JOIN Category AS c2 ON (c1.ParentID = c2.CategoryID) WHERE c2.ParentID > 1000000       
    ) AS t    
    JOIN Category AS c ON (t.CategoryID = c.CategoryID)    
    JOIN Keeper AS k ON (k.CategoryID = t.CategoryID AND k.KeeperName <> 'All')
GROUP BY
      t.ParentID, c.FullName, k.KeeperName, c.CategoryID
ORDER BY c.FullName, k.KeeperName";
                    using (SQLiteDataReader sqLiteDataReader = command.ExecuteReader())
                    {
                        while (sqLiteDataReader.Read())
                            dictionary2.Add(
                                new Tuple<int, string, int>(sqLiteDataReader.GetInt32(0), sqLiteDataReader.GetString(3),
                                    sqLiteDataReader.GetInt32(1)),
                                new Tuple<string, decimal, decimal>(sqLiteDataReader.GetString(2),
                                    sqLiteDataReader.GetDecimal(4), sqLiteDataReader.GetDecimal(5)));
                    }

                    command.CommandText = @"
SELECT t.ParentID, c.CategoryID, c.FullName,SUM(Count)Count, SUM(Size)Size
FROM
    (
       SELECT CategoryID, ParentID FROM Category WHERE ParentID > 1000000 UNION
       SELECT c1.CategoryID, c2.ParentID FROM Category AS c1 JOIN Category AS c2 ON (c1.ParentID = c2.CategoryID) WHERE c2.ParentID > 1000000       
    ) AS t    
    JOIN Category AS c ON (t.CategoryID = c.CategoryID)    
    JOIN Keeper AS k ON (k.CategoryID = t.CategoryID AND k.KeeperName <> 'All')
GROUP BY
      c.CategoryID, c.FullName
ORDER BY c.FullName";

                    using (SQLiteDataReader sqLiteDataReader = command.ExecuteReader())
                    {
                        while (sqLiteDataReader.Read())
                            tupleList.Add(new Tuple<int, int, string, decimal, decimal>(sqLiteDataReader.GetInt32(0),
                                sqLiteDataReader.GetInt32(1), sqLiteDataReader.GetString(2),
                                sqLiteDataReader.GetDecimal(3), sqLiteDataReader.GetDecimal(4)));
                    }

                    var rootCategoryReportTemplate = Settings.Current.ReportCategoriesTemplate;
                    foreach (int num1 in source1.Select(x => x.Key))
                    {
                        int c = num1;
                        var rootCategoryReportData = new Dictionary<string, object>()
                        {
                            {"today", DateTime.Now.ToString("dd.MM.yyyy")},
                            {"topics_count", source1[c].Item2},
                            {"topics_size", source1[c].Item3.ToString("N")},
                            {"keepers", new List<object>()},
                            {"categories", new List<object>()},
                        };
                        int num2 = 1;
                        Dictionary<Tuple<int, string>, Tuple<string, decimal, decimal>> source2 = dictionary1;
                        foreach (KeyValuePair<Tuple<int, string>, Tuple<string, decimal, decimal>> keyValuePair1 in
                            source2.Where(x => x.Key.Item1 == c))
                        {
                            var categoriesList = new List<object>();
                            KeyValuePair<Tuple<int, string>, Tuple<string, decimal, decimal>> k = keyValuePair1;
                            ((List<object>) rootCategoryReportData["keepers"]).Add(new Dictionary<string, object>()
                            {
                                {"keeper_number", num2},
                                {"keeper_username", k.Key.Item2},
                                {"keep_topics_count", k.Value.Item2},
                                {"keep_topics_size", k.Value.Item3.ToString("N")},
                                {"categories", categoriesList}
                            });

                            Dictionary<Tuple<int, string, int>, Tuple<string, decimal, decimal>> source3 = dictionary2;
                            foreach (KeyValuePair<Tuple<int, string, int>, Tuple<string, decimal, decimal>>
                                keyValuePair2 in source3.Where(x => x.Key.Item2 == k.Key.Item2 && x.Key.Item1 == c))
                            {
                                categoriesList.Add(new Dictionary<string, object>()
                                {
                                    {"keep_category_name", keyValuePair2.Value.Item1},
                                    {"keep_category_topics_count", keyValuePair2.Value.Item2},
                                    {"keep_category_topics_size", keyValuePair2.Value.Item3.ToString("N")},
                                });
                            }

                            ++num2;
                        }

                        List<Tuple<int, int, string, decimal, decimal>> source4 = tupleList;
                        foreach (Tuple<int, int, string, decimal, decimal> tuple in source4.Where(x => x.Item1 == c)
                            .OrderBy(x => x.Item3))
                        {
                            Tuple<int, int, string, decimal, decimal> k = tuple;
                            var keepersList = new List<object>();
                            ((List<object>) rootCategoryReportData["categories"]).Add(new Dictionary<string, object>()
                            {
                                {"category_name", k.Item3},
                                {"topics_count", k.Item4},
                                {"topics_size", k.Item5.ToString("N")},
                                {"keepers", keepersList},
                            });
                            Dictionary<Tuple<int, string, int>, Tuple<string, decimal, decimal>> source3 = dictionary2;
                            foreach (KeyValuePair<Tuple<int, string, int>, Tuple<string, decimal, decimal>> keyValuePair
                                in source3.Where(x => x.Key.Item3 == k.Item2).OrderBy(x => x.Key.Item2))
                            {
                                keepersList.Add(new Dictionary<string, object>()
                                {
                                    {"keeper_username", keyValuePair.Key.Item2},
                                    {"keep_topics_count", keyValuePair.Value.Item2},
                                    {"keep_topics_size", keyValuePair.Value.Item3.ToString("N")},
                                });
                            }
                        }

                        reports.Add(c, new Dictionary<int, string>());
                        reports[c].Add(0, Stubble.Render(rootCategoryReportTemplate, rootCategoryReportData));
                    }

                    ClientLocalDB.Current.SaveReports(reports);
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}