using System;
using System.Collections.Generic;
using Commons;
using JetBrains.Annotations;

namespace EdiTimeline
{
    public interface IBoxEventsReader
    {
        [NotNull]
        BoxEvent ReadEvent(Guid eventId);

        [CanBeNull]
        AllBoxEventSeriesRange TryCreateEventSeriesRange([CanBeNull] AllBoxEventSeriesPointer exclusiveStartEventPointer, [CanBeNull] Timestamp inclusiveEndTimestamp);

        [CanBeNull]
        AllBoxEventSeriesRange TryCreateEventSeriesRange([CanBeNull] Timestamp exclusiveStartTimestamp, [CanBeNull] Timestamp inclusiveEndTimestamp);

        [CanBeNull]
        AllBoxEventSeriesRange TryCreateEventSeriesRange(Guid? exclusiveStartEventId, [CanBeNull] Timestamp inclusiveEndTimestamp, out bool exclusiveStartEventNotFound);

        [NotNull]
        List<TResultBoxEvent> ReadEvents<TResultBoxEvent>([CanBeNull] AllBoxEventSeriesRange range, int take, [NotNull] Func<BoxEvent[], TResultBoxEvent[]> convertAndFilter);
    }
}