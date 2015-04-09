#region

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Redmine.Net.Api;
using Redmine.Net.Api.Types;

#endregion

namespace RedmineToReport
{
    internal class Program
    {

        private static Dictionary<IdentifiableName, List<Pair<Issue, float>>> GetAverageCloseTicketDays(
            IList<Issue> allList, string closeStatusName)
        {
            var statusDictionaly = new Dictionary<IdentifiableName, List<Pair<Issue, float>>>();
            foreach (var t in allList.Where(t => t.Status.Name == closeStatusName))
            {
                if (statusDictionaly.ContainsKey(t.Tracker) == false)
                {
                    statusDictionaly.Add(t.Tracker, new List<Pair<Issue, float>>());
                }
                var ts = (TimeSpan) (t.UpdatedOn - t.CreatedOn);
                statusDictionaly[t.Tracker].Add(new Pair<Issue, float>(t, ts.Days + (ts.Hours/8.0f)));
            }
            return statusDictionaly;
        }

        private static List<Pair<DateTime, DateTime>> GetMeasurList(IEnumerable<Issue> alList, int sprintSize)
        {
            var collectionDateList = new List<Pair<DateTime, DateTime>>();
            var minTime = new DateTime(2900, 1, 1);
            var maxTime = new DateTime(1900, 1, 1);
            foreach (var t in alList)
            {
                if (t.UpdatedOn < minTime)
                {
                    minTime = (DateTime) t.UpdatedOn;
                }
                if (t.UpdatedOn > maxTime)
                {
                    maxTime = (DateTime) t.UpdatedOn;
                }
            }
            var range = maxTime - minTime;
            var sprintRange = range.Days/sprintSize;
            Console.WriteLine("集計期間 " + minTime + "～" + maxTime + " (" + sprintRange + ")区間に分割します");
            for (var i = 0; i < sprintRange + 1; i++)
            {
                var t = minTime.AddDays(sprintSize);
                collectionDateList.Add(new Pair<DateTime, DateTime>(minTime, t));
                minTime = t;
            }
            return collectionDateList;
        }


        public static IList<Issue> GetTicketList(string reportId, string projUrl , string apikey)
        {
            var manager = new RedmineManager(projUrl, apikey)
            {
                PageSize = 100
            };
            var parameters = new NameValueCollection {{"query_id", reportId}};
            return manager.GetTotalObjectList<Issue>(parameters);
        }

        private static void SortingTicket(out IList<Issue> bug, out IList<Issue> task, IList<Issue> totalList,string keyword)
        {
            task = new List<Issue>();
            bug = new List<Issue>();
            foreach (var t in totalList)
            {
                if (t.Tracker.Name == keyword)
                {
                    bug.Add(t);
                }
                else
                {
                    task.Add(t);
                }
            }
        }

        /// <summary>
        ///     メイン処理
        /// </summary>
        public static int Main(string[] args)
        {
            var csv = new Csv();
            var settings = new Settings();
            if (args.Length == 0)
            {
                Console.WriteLine("XMLファイルが指定されていません。");
                return 1;
            }

            // XMLファイルを読み込み
            try
            {
                using (var xr = new XmlTextReader(args[0]))
                {
                    while (xr.Read())
                    {
                        var serializer = new XmlSerializer(typeof (Settings));
                        settings = (Settings) serializer.Deserialize(xr);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 1;
            }

            // 指定クエリでチケットリストをRedmineから取得
            Console.WriteLine("Get Redmine : " + settings.GetRedmineProjectUrl() + " Query:" + settings.RedmineQueryId);
            var totalList = GetTicketList(settings.RedmineQueryId, settings.GetRedmineProjectUrl(), settings.RedmineApiKey);
            Console.WriteLine("Get Redmine : " + settings.GetRedmineProjectUrl() + " Query:" + settings.RedmineAllRemainingQueryId);
            var allReminingList = GetTicketList(settings.RedmineAllRemainingQueryId, settings.GetRedmineProjectUrl(), settings.RedmineApiKey);
            IList<Issue> bugList, taskList, bugListRemining, taskListRemining;
            Console.WriteLine("チケットの取得数 : " + (totalList.Count + allReminingList.Count));
            // totalListに含まれているものと重複している物をallReminingListから除外する
            foreach (var t in totalList)
            {
                var tid = t.Id;
                foreach (var r in allReminingList.Where(r => r.Id == tid))
                {
                    allReminingList.Remove(r);
                    break;
                }
            }
            Console.WriteLine("チケットの重複削除後の数 : " + (totalList.Count + allReminingList.Count));
            // totalListをバグとタスクに分割
            SortingTicket(out bugList, out taskList, totalList, settings.BugKeyword);
            // allReminingListをバグとタスクに分割
            SortingTicket(out bugListRemining, out taskListRemining, allReminingList, settings.BugKeyword);
            Console.WriteLine("バグチケット抽出 : " + (bugList.Count + bugListRemining.Count));
            Console.WriteLine("タスチケット抽出 : " + (taskList.Count + taskListRemining.Count));

            var collectionDateList = GetMeasurList(totalList, settings.SipriteSize);
            csv.Write("task-", taskList, settings.SipriteSize, taskListRemining.Count, collectionDateList);
            Console.WriteLine("task csv出力");
            csv.Write("bug-", bugList, settings.SipriteSize, bugListRemining.Count, collectionDateList);
            Console.WriteLine("bug csv出力");
            csv.WriteReming("task-", taskListRemining);
            csv.WriteReming("bug-", bugListRemining);
            Console.WriteLine("reming csv出力");
            Console.WriteLine("正常に終了しました。");
            return 0;
        }
    }
}

/*
var trackerDictionaly = new Dictionary<IdentifiableName, List<Issue>>();
foreach (var t in totalList)
{
    if (trackerDictionaly.ContainsKey(t.Tracker) == false)
    {
        trackerDictionaly.Add(t.Tracker, new List<Issue>());
    }
    trackerDictionaly[t.Tracker].Add(t);
}
var statusDictionaly = new Dictionary<IdentifiableName, Dictionary<IdentifiableName, List<Issue>>>();
foreach (var t in totalList)
{
    if (statusDictionaly.ContainsKey(t.Tracker) == false)
    {
        statusDictionaly.Add(t.Tracker, new Dictionary<IdentifiableName, List<Issue>>());
    }
    if (statusDictionaly[t.Tracker].ContainsKey(t.Status) == false)
    {
        statusDictionaly[t.Tracker].Add(t.Status, new List<Issue>());
    }
    statusDictionaly[t.Tracker][t.Status].Add(t);
}
var averageClose = GetAverageCloseTicketDays(totalList, "終了");
foreach (var avg in averageClose)
{
    var total = (float)avg.Value.Sum(time => time.Value) / (float)avg.Value.Count;
    Console.WriteLine(avg.Key.Name + " " + "平均寿命:" + total);
}
foreach (var t in trackerDictionaly)
{
    Console.WriteLine(t.Key.Name + ":" + t.Value.Count);
}
*/