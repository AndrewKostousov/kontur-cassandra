using System;
using System.Collections.Generic;
using System.Linq;

namespace SKBKontur.Catalogue.StatisticsAggregator
{
    internal static class StatisticsCellExtensions
    {
        public static long GetCount(this IEnumerable<StatisticsCell> cells)
        {
            return cells.Sum(x => x.Count);
        }

        public static long GetSum(this IEnumerable<StatisticsCell> cells)
        {
            return cells.Sum(x => x.Sum);
        }

        public static long GetMax(this IEnumerable<StatisticsCell> cells)
        {
            return cells.Max(x => x.Max);
        }

        public static long GetQuantile(this IEnumerable<StatisticsCell> cells, int quantile)
        {
            var statisticsCells = cells as IList<StatisticsCell> ?? cells.ToList();

            const int countsLength = 250;
            var count = statisticsCells.GetCount();
            if(count == 0)
                return 0;
            var index = (int)Math.Round(count * ((double)quantile / 100));
            var quantileBoundCount = 0;
            for(var i = 0; i < countsLength; i++)
            {
                quantileBoundCount += statisticsCells.Sum(x => x.counts[i]);
                if(quantileBoundCount >= index)
                    return (int)Math.Round(Math.Pow(10, (double)i / 30));
            }
            return statisticsCells.GetMax();
        }
    }
}