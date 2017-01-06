using System;

namespace EdiTimeline
{
    public class AllBoxEventSeriesSettings : IAllBoxEventSeriesSettings
    {
        public AllBoxEventSeriesSettings()
        {
            MinBatchSizeForRead = 100;
            PartitionDuration = TimeSpan.FromMinutes(10);
            NotCommittedEventsTtl = TimeSpan.FromSeconds(60);
        }

        public int MinBatchSizeForRead { get; private set; }
        public TimeSpan PartitionDuration { get; private set; }
        public TimeSpan NotCommittedEventsTtl { get; private set; }
    }
}