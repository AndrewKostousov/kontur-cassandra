using Commons;
using JetBrains.Annotations;

namespace EdiTimeline
{
    public class AllBoxEventSeriesColumnValue
    {
        public AllBoxEventSeriesColumnValue([NotNull] byte[] payload, bool eventIsCommitted)
        {
            if (payload == null)
                throw new InvalidProgramStateException("payload is required");
            Payload = payload;
            EventIsCommitted = eventIsCommitted;
        }

        [NotNull]
        public byte[] Payload { get; }

        public bool EventIsCommitted { get; }

        public override string ToString()
        {
            return $"Payload: {Payload.ToHexString()}, EventIsCommitted: {EventIsCommitted}";
        }
    }
}