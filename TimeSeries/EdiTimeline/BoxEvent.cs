using System;
using Commons;
using JetBrains.Annotations;

namespace EdiTimeline
{
    public class BoxEvent
    {
        public BoxEvent(Guid eventId, [NotNull] Timestamp eventTimestamp, [NotNull] byte[] payload)
        {
            if (eventTimestamp == null)
                throw new InvalidProgramStateException($"{nameof(eventTimestamp)} is required for eventId: {eventId}");
            if (payload == null)
                throw new InvalidProgramStateException($"{nameof(payload)} is required for eventId: {eventId}");
            EventId = eventId;
            EventTimestamp = eventTimestamp;
            Payload = payload;
        }

        public Guid EventId { get; }

        [NotNull]
        public Timestamp EventTimestamp { get; }

        [NotNull]
        public byte[] Payload { get; }

        public override string ToString()
        {
            return $"EventId: {EventId}, EventTimestamp: {EventTimestamp}, Payload: {Payload.ToHexString()}";
        }
    }
}