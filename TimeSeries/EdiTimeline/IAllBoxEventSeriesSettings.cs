using System;

namespace EdiTimeline
{
    public interface IAllBoxEventSeriesSettings
    {
        int MinBatchSizeForRead { get; }
        TimeSpan PartitionDuration { get; }
        TimeSpan NotCommittedEventsTtl { get; }
    }
}