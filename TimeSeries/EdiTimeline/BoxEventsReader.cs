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

        [NotNull]
        public BoxEvent ReadEvent(Guid eventId)
        {
            var boxEvent = allBoxEventSeries.TryReadEvent(eventId);
            if (boxEvent == null)
                throw new InvalidProgramStateException($"Event not found: {eventId}");
            if (boxEvent.Payload == null)
                throw new InvalidProgramStateException($"boxEvent.Payload == null for eventId: {eventId}");
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
            return allBoxEventSeries.ReadEvents(range, take, convertAndFilter);
        }

        private readonly IAllBoxEventSeries allBoxEventSeries;
    }
}