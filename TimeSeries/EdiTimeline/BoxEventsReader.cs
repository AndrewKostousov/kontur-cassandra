using System;
using System.Collections.Generic;
using System.Linq;
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

        [NotNull]
        public BoxEvent ReadEvent(Guid eventId)
        {
            var boxEvent = allBoxEventSeries.TryReadEvent(eventId);
            if (boxEvent == null)
                throw new InvalidProgramStateException(string.Format("Event not found: {0}", eventId));
            if (boxEvent.TryGetEventContent() == null)
                throw new InvalidProgramStateException(string.Format("boxEvent.TryGetEventContent() == null for eventId: {0}", eventId));
            return boxEvent;
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

        [CanBeNull]
        public AllBoxEventSeriesRange TryCreateEventSeriesRange(Guid? exclusiveStartEventId, [CanBeNull] Timestamp inclusiveEndTimestamp, out bool exclusiveStartEventNotFound)
        {
            return allBoxEventSeries.TryCreateRange(exclusiveStartEventId, inclusiveEndTimestamp, out exclusiveStartEventNotFound);
        }

        [NotNull]
        public List<TResultBoxEvent> ReadEvents<TResultBoxEvent>([CanBeNull] AllBoxEventSeriesRange range, int take, [NotNull] Func<BoxEvent[], TResultBoxEvent[]> convertAndFilter)
        {
            return allBoxEventSeries.ReadEvents(range, take, batch => convertAndFilter(batch.Where(x => x.TryGetEventContent() != null).ToArray()));
        }

        private readonly IAllBoxEventSeries allBoxEventSeries;
    }
}