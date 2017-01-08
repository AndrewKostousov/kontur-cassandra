using System;
using System.Collections.Generic;
using Commons;
using JetBrains.Annotations;

namespace EdiTimeline
{
    public interface IAllBoxEventSeries
    {
        void WriteEventsInAnyOrder([NotNull] List<AllBoxEventSeriesWriterQueueItem> queueItems);

        [CanBeNull]
        AllBoxEventSeriesRange TryCreateRange([CanBeNull] AllBoxEventSeriesPointer exclusiveStartEventPointer, [CanBeNull] Timestamp inclusiveEndTimestamp);

        [CanBeNull]
        AllBoxEventSeriesRange TryCreateRange([CanBeNull] Timestamp exclusiveStartTimestamp, [CanBeNull] Timestamp inclusiveEndTimestamp);

        [NotNull]
        List<TResultBoxEvent> ReadEvents<TResultBoxEvent>([CanBeNull] AllBoxEventSeriesRange range, int take, [NotNull] Func<BoxEvent[], TResultBoxEvent[]> convertAndFilter);
    }
}