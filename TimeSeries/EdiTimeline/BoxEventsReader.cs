using System;
using System.Collections.Generic;
using Commons;
using JetBrains.Annotations;

namespace EdiTimeline
{
    public class BoxEventsReader : IBoxEventsReader
    {
        public BoxEventsReader(IAllBoxEventSeries allBoxEventSeries)
        {
            this.allBoxEventSeries = allBoxEventSeries;
        }

        [CanBeNull]
        public AllBoxEventSeriesRange TryCreateEventSeriesRange([CanBeNull] AllBoxEventSeriesPointer exclusiveStartEventPointer, [CanBeNull] Timestamp inclusiveEndTimestamp)
        {
            return allBoxEventSeries.TryCreateRange(exclusiveStartEventPointer, inclusiveEndTimestamp);
        }

        [CanBeNull]
        public AllBoxEventSeriesRange TryCreateEventSeriesRange([CanBeNull] Timestamp exclusiveStartTimestamp, [CanBeNull] Timestamp inclusiveEndTimestamp)
        {
            return allBoxEventSeries.TryCreateRange(exclusiveStartTimestamp, inclusiveEndTimestamp);
        }

        [NotNull]
        public List<TResultBoxEvent> ReadEvents<TResultBoxEvent>([CanBeNull] AllBoxEventSeriesRange range, int take, [NotNull] Func<BoxEvent[], TResultBoxEvent[]> convertAndFilter)
        {
            return allBoxEventSeries.ReadEvents(range, take, convertAndFilter);
        }

        private readonly IAllBoxEventSeries allBoxEventSeries;
    }
}