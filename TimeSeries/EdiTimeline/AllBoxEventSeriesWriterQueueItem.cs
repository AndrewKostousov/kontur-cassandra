using Commons;
using JetBrains.Annotations;

namespace EdiTimeline
{
    public class AllBoxEventSeriesWriterQueueItem
    {
        public AllBoxEventSeriesWriterQueueItem([NotNull] ProtoBoxEvent protoBoxEvent, [NotNull] Promise<Timestamp> eventTimestamp)
        {
            if (protoBoxEvent.Payload == null)
                throw new InvalidProgramStateException($"protoBoxEvent.Payload is required for: {protoBoxEvent}");
            ProtoBoxEvent = protoBoxEvent;
            EventTimestamp = eventTimestamp;
        }

        [NotNull]
        public ProtoBoxEvent ProtoBoxEvent { get; }

        [NotNull]
        public Promise<Timestamp> EventTimestamp { get; private set; }

        [NotNull]
        public AllBoxEventSeriesColumnValue GetAllBoxEventSeriesColumnValue(bool eventIsCommitted)
        {
            return new AllBoxEventSeriesColumnValue(ProtoBoxEvent.Payload, eventIsCommitted);
        }
    }
}