using System;
using JetBrains.Annotations;

namespace EdiTimeline
{
    public class BoxEventsWriter : IBoxEventsWriter
    {
        public BoxEventsWriter(Lazy<AllBoxEventSeriesWriter> lazyAllBoxEventSeriesWriter)
        {
            this.lazyAllBoxEventSeriesWriter = lazyAllBoxEventSeriesWriter;
        }

        public Guid WriteEvent([NotNull] byte[] payload)
        {
            var eventId = Guid.NewGuid();
            var protoBoxEvent = new ProtoBoxEvent(eventId, payload);
            lazyAllBoxEventSeriesWriter.Value.Write(protoBoxEvent);
            return eventId;
        }

        private readonly Lazy<AllBoxEventSeriesWriter> lazyAllBoxEventSeriesWriter;
    }
}