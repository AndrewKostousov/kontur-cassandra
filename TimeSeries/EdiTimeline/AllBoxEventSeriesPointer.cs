using System;
using Commons;
using JetBrains.Annotations;

namespace EdiTimeline
{
    public class AllBoxEventSeriesPointer
    {
        public AllBoxEventSeriesPointer([NotNull] Timestamp eventTimestamp, Guid eventId)
        {
            EventTimestamp = eventTimestamp;
            EventId = eventId;
        }

        [NotNull]
        public Timestamp EventTimestamp { get; }

        public Guid EventId { get; }

        public override string ToString()
        {
            return $"EventTimestamp: {EventTimestamp}, EventId: {EventId}";
        }
    }
}