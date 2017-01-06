using System;
using Commons;
using JetBrains.Annotations;

namespace EdiTimeline
{
    public class AllBoxEventSeriesWriterQueueItem
    {
        public AllBoxEventSeriesWriterQueueItem([NotNull] ProtoBoxEvent protoBoxEvent, [NotNull] Promise<Timestamp> eventTimestamp)
        {
            // note: check for nulls to detect grobuf tricks (unknown event types are silently deserialized to null)
            if (protoBoxEvent.BoxId == null)
                throw new InvalidProgramStateException(string.Format("protoBoxEvent.BoxId is required for: {0}", protoBoxEvent));
            if (protoBoxEvent.EventContent == null)
                throw new InvalidProgramStateException(string.Format("protoBoxEvent.EventContent is required for: {0}", protoBoxEvent));
            ProtoBoxEvent = protoBoxEvent;
            EventTimestamp = eventTimestamp;
        }

        [NotNull]
        public ProtoBoxEvent ProtoBoxEvent { get; private set; }

        [NotNull]
        public Promise<Timestamp> EventTimestamp { get; private set; }

        [NotNull]
        public AllBoxEventSeriesColumnValue GetAllBoxEventSeriesColumnValue(bool eventIsCommitted)
        {
            var eventContent = new Lazy<BoxEventContent>(() => ProtoBoxEvent.EventContent);
            return new AllBoxEventSeriesColumnValue(ProtoBoxEvent.BoxId, ProtoBoxEvent.DocumentCirculationId, eventContent, eventIsCommitted);
        }
    }
}