using System;
using Commons;
using JetBrains.Annotations;

namespace EdiTimeline
{
    public class ProtoBoxEvent
    {
        public ProtoBoxEvent(Guid eventId, [NotNull] byte[] payload)
        {
            if (payload == null)
                throw new InvalidProgramStateException($"{nameof(payload)} is required for eventId: {eventId}");
            EventId = eventId;
            Payload = payload;
        }

        public Guid EventId { get; }

        [NotNull]
        public byte[] Payload { get; }

        public override string ToString()
        {
            return $"EventId: {EventId}, Payload: {Payload.ToHexString()}";
        }
    }
}