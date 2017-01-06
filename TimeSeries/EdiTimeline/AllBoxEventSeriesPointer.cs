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
        public Timestamp EventTimestamp { get; private set; }

        public Guid EventId { get; private set; }

        public override string ToString()
        {
            return string.Format("EventTimestamp: {0}, EventId: {1}", EventTimestamp, EventId);
        }
    }
}