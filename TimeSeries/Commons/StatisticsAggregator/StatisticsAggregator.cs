using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SKBKontur.Catalogue.StatisticsAggregator
{
    public class StatisticsAggregator : IStatisticsAggregator
    {
        public StatisticsAggregator(TimeSpan aggregationCell, TimeSpan aggregationInterval)
        {
            shiftTimeLock = new ReaderWriterLockSlim();
            this.aggregationCell = aggregationCell;
            cellCount = (int)((aggregationInterval.Ticks + aggregationCell.Ticks - 1) / aggregationCell.Ticks);
            cells = Enumerable.Range(0, cellCount).Select(idx => new CellInfo(new StatisticsCell(), new DateTime(0))).ToArray();
        }

        public void AddEvent(int value)
        {
            AddEvent(DateTime.Now, value);
        }

        public void AddEvent(DateTime now, int value)
        {
            GetCell(now).AddValue(value);
        }

        public ValueWithInterval<long> GetCount(DateTime now, TimeSpan fromSeconds)
        {
            return GetCells(now, fromSeconds).MapValue(x => x.GetCount());
        }

        public ValueWithInterval<long> GetSum(DateTime now, TimeSpan fromSeconds)
        {
            return GetCells(now, fromSeconds).MapValue(x => x.GetSum());
        }

        public ValueWithInterval<long> GetQuantile(DateTime now, TimeSpan fromSeconds, int quantile)
        {
            return GetCells(now, fromSeconds).MapValue(x => x.GetQuantile(quantile));
        }

        internal class CellInfo
        {
            public CellInfo(StatisticsCell cell, DateTime start)
            {
                StatisticsCell = cell;
                StartOfCell = start;
            }

            public StatisticsCell StatisticsCell { get; set; }
            public DateTime StartOfCell { get; set; }
        }

        private StatisticsCell GetCell(DateTime now)
        {
            var fullIndex = now.Ticks / aggregationCell.Ticks;
            var currentCellIndex = (int)(fullIndex % cellCount);

            var cell = cells[currentCellIndex];
            if((cell.StartOfCell + aggregationCell) < now)
            {
                shiftTimeLock.EnterWriteLock();
                try
                {
                    if((cell.StartOfCell + aggregationCell) < now)
                    {
                        cell.StatisticsCell = new StatisticsCell();
                        cell.StartOfCell = new DateTime(fullIndex * aggregationCell.Ticks, DateTimeKind.Utc);
                    }
                }
                finally
                {
                    shiftTimeLock.ExitWriteLock();
                }
            }
            return cell.StatisticsCell;
        }

        private ValueWithInterval<List<StatisticsCell>> GetCells(DateTime now, TimeSpan fromSeconds)
        {
            var resultInterval = TimeSpan.FromTicks(0);
            var result = new List<StatisticsCell>();
            var start = now;
            shiftTimeLock.EnterReadLock();
            try
            {
                var fullIndex = now.Ticks / aggregationCell.Ticks;
                var currentCellStartTime = new DateTime(fullIndex * aggregationCell.Ticks, DateTimeKind.Utc);
                var currentCellIndex = (int)(fullIndex % cellCount);
                var length = (fromSeconds.Ticks + aggregationCell.Ticks - 1) / aggregationCell.Ticks;
                if(now - currentCellStartTime < TimeSpan.FromTicks(aggregationCell.Ticks / 2))
                    length += 1;
                for(var i = 0; i < length; i++)
                {
                    try
                    {
                        var index = currentCellIndex - i;
                        index = (index % cellCount + cellCount) % cellCount;

                        if(cells[index].StartOfCell < currentCellStartTime)
                            continue;

                        result.Add(cells[index].StatisticsCell);
                    }
                    finally
                    {
                        resultInterval = (start - currentCellStartTime);
                        currentCellStartTime = currentCellStartTime - aggregationCell;
                    }
                }
            }
            finally
            {
                shiftTimeLock.ExitReadLock();
            }
            return new ValueWithInterval<List<StatisticsCell>>(result, resultInterval);
        }

        private readonly ReaderWriterLockSlim shiftTimeLock;
        private readonly TimeSpan aggregationCell;

        private readonly CellInfo[] cells;
        private readonly int cellCount;
    }
}