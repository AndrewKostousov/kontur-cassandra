using System;
using System.Collections.Generic;
using Commons;

namespace CassandraTimeSeries.Utils
{
    public class TimeLinePartitioner
    {
        private  static readonly TimeSpan DefaultPartitionDuration = TimeSpan.FromMinutes(1);

        public TimeSpan PartitionDuration { get; }

        public TimeLinePartitioner(TimeSpan? partitionDuration = null)
        {
            PartitionDuration = partitionDuration ?? DefaultPartitionDuration;
        }

        public long CreatePartitionId(Timestamp timestamp)
        {
            return timestamp.Floor(PartitionDuration).Ticks;
        }

        public IEnumerable<Timestamp> SplitIntoPartitions(Timestamp from, Timestamp to)
        {
            if (from >= to) yield break;

            var currentSlice = from.Floor(PartitionDuration);

            while (currentSlice <= to)
            {
                yield return currentSlice;
                currentSlice += PartitionDuration;
            }
        }
    }
}
