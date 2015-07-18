#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Redmine.Net.Api.Types;

#endregion

namespace RedmineToReport
{
    internal class Csv
    {
        private List<List<Issue>> GetCreateLists(IEnumerable<Issue> allList,
            List<Pair<DateTime, DateTime>> collectionDateList)
        {
            var dateRangeLists = collectionDateList.Select(r => new List<Issue>()).ToList();
            foreach (var t in allList)
            {
                for (var i = 0; i < collectionDateList.Count; i++)
                {
                    if (t.CreatedOn >= collectionDateList[i].Key)
                    {
                        if (t.CreatedOn < collectionDateList[i].Value)
                        {
                            dateRangeLists[i].Add(t);
                            break;
                        }
                    }
                }
            }
            return dateRangeLists;
        }

        private List<List<Issue>> GetStatusLists(IEnumerable<List<Issue>> allList, string statusName)
        {
            return allList.Select(r => r.Where(t => t.Status.Name == statusName).ToList()).ToList();
        }

        private List<List<Issue>> GetUpdateLists(IEnumerable<Issue> allList,
            List<Pair<DateTime, DateTime>> collectionDateList)
        {
            var dateRangeLists = collectionDateList.Select(r => new List<Issue>()).ToList();
            foreach (var t in allList)
            {
                for (var i = 0; i < collectionDateList.Count; i++)
                {
                    if (t.UpdatedOn >= collectionDateList[i].Key)
                    {
                        if (t.UpdatedOn < collectionDateList[i].Value)
                        {
                            dateRangeLists[i].Add(t);
                            break;
                        }
                    }
                }
            }
            return dateRangeLists;
        }

        private void WriteList<T>(string fname, IEnumerable<T> list)
        {
            using (var sr = new StreamWriter(fname, false, Encoding.GetEncoding("utf-8")))
            {
                var cnt = 0;
                var enumerable = list as IList<T> ?? list.ToList();
                foreach (var l in enumerable)
                {
                    sr.Write(l);
                    ++cnt;
                    if (cnt != enumerable.Count())
                    {
                        sr.Write(',');
                    }
                }
            }
        }

        private void WriteLisPair(string fname, List<Pair<string, string>> list)
        {
            using (var sr = new StreamWriter(fname, false, Encoding.GetEncoding("utf-8")))
            {
                var cnt = 0;
                foreach (var l in list)
                {
                    sr.Write(l.Key);
                    sr.Write(',');
                    sr.Write(l.Value);
                    ++cnt;
                    if (cnt != list.Count())
                    {
                        sr.Write(',');
                    }
                }
            }
        }

        public void Write(string prefixFileName, IList<Issue> totalList, int sprintSlize, int offset,
            List<Pair<DateTime, DateTime>> collectionDateList)
        {
            var updateLists = GetUpdateLists(totalList, collectionDateList);
            var createLists = GetCreateLists(totalList, collectionDateList);
            var closeLists = GetStatusLists(updateLists, "終了");
            var cancelLists = GetStatusLists(updateLists, "却下");
            for (var i = 0; i < closeLists.Count; i++)
            {
                for (var j = 0; j < cancelLists[i].Count; j++)
                {
                    closeLists[i].Add(cancelLists[i][j]);
                }
            }
            var oldCreateLists = totalList.Where(t => t.CreatedOn < collectionDateList[0].Key).ToList();
            foreach (var c in createLists)
            {
                foreach (var t in c)
                {
                    var t1 = t;
                    foreach (var o in oldCreateLists.Where(o => t1.Id == o.Id))
                    {
                        oldCreateLists.Remove(o);
                        break;
                    }
                }
            }
            var addList = createLists.Select((t, i) => t.Count - closeLists[i].Count).ToList();
            var accumulationList = new List<int>();
            var closeLifetimeList = new List<float>();
            var totalAdd = offset + oldCreateLists.Count;
            for (var i = 0; i < createLists.Count; i++)
            {
                totalAdd += addList[i];
                accumulationList.Add(totalAdd);
            }
            foreach (var c in closeLists)
            {
                var total = new TimeSpan();
                var result = 0.0f;
                if (c.Count != 0)
                {
                    total = c.Select(t => (TimeSpan) (t.UpdatedOn - t.CreatedOn))
                        .Aggregate(total, (current, ts) => current.Add(ts));
                    result = (float) total.TotalDays/c.Count;
                }
                closeLifetimeList.Add(result);
            }
            WriteList(prefixFileName + "collectiondate.csv",
                collectionDateList.Select(c => c.Key.ToString(CultureInfo.InvariantCulture).Split(' '))
                    .Select(t => t[0])
                    .ToList());
            WriteList(prefixFileName + "addlist.csv", addList);
            WriteList(prefixFileName + "accumlation.csv", accumulationList);
            WriteList(prefixFileName + "closelist.csv", closeLists.Select(c => c.Count).ToList());
            WriteList(prefixFileName + "createlist.csv", createLists.Select(c => c.Count).ToList());
            WriteList(prefixFileName + "closelifetime.csv", closeLifetimeList);

            float startPoint = accumulationList[accumulationList.Count - 1];
            var startDate = collectionDateList[collectionDateList.Count - 1].Key;
            var velocityClose = closeLists.Sum(c => c.Count)/(float) closeLists.Count;
            var velocityCreate = createLists.Sum(c => c.Count)/(float) createLists.Count;
            var div = (velocityClose - velocityCreate);
            var futurePoint = new List<float>();
            var futureDate = new List<string>();
            if (div <= 0.0f)
            {
                div = startPoint;
            }
            for (;;)
            {
                futurePoint.Add(startPoint);
                var t = startDate.ToString(CultureInfo.InvariantCulture).Split(' ');
                futureDate.Add(t[0]);
                if (startPoint <= 0.0f)
                {
                    break;
                }
                startPoint -= div;
                startDate = startDate.AddDays(sprintSlize);
                if (startPoint < 0.0f)
                {
                    startPoint = 0.0f;
                }
            }
            WriteList(prefixFileName + "futuredates.csv", futureDate);
            WriteList(prefixFileName + "futurepoint.csv", futurePoint);

            var trackerDictionaly = new Dictionary<string, int>();
            var totalLen = 0;
            foreach (var l in closeLists)
            {
                foreach (var t in l)
                {
                    if (trackerDictionaly.ContainsKey(t.Author.Name) == false)
                    {
                        trackerDictionaly.Add(t.Author.Name, 0);
                    }
                    trackerDictionaly[t.Author.Name]++;
                    ++totalLen;
                }
            }
            var tmp = trackerDictionaly.ToList();
            tmp.Sort((kvp1, kvp2) => kvp2.Value - kvp1.Value);
            WriteLisPair(prefixFileName + "authors.csv",
                tmp.Select(
                    c =>
                        new Pair<string, string>(c.Key,
                            (((float) c.Value/totalLen)*100.0f).ToString(CultureInfo.InvariantCulture)))
                    .ToList());
        }

        public void WriteReming(string prefixFileName, IEnumerable<Issue> totalList)
        {
            var dateTimeDictionaly = new SortedDictionary<DateTime, List<Issue>>();
            foreach (var t in totalList)
            {
                var yearAndMonth = new DateTime(t.CreatedOn.Value.Year, t.CreatedOn.Value.Month, 1);
                if (dateTimeDictionaly.ContainsKey(yearAndMonth) == false)
                {
                    dateTimeDictionaly.Add(yearAndMonth, new List<Issue>());
                }
                dateTimeDictionaly[yearAndMonth].Add(t);
            }
            var lst = (from d in dateTimeDictionaly
                let date = d.Key.ToString(CultureInfo.InvariantCulture).Split(' ')
                let count = d.Value.Count.ToString()
                select new Pair<string, string>(date[0], count)).ToList();
            WriteLisPair(prefixFileName + "remining-count.csv", lst);
        }
    }
}