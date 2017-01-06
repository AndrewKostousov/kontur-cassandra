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

        public Guid WriteEvent([NotNull] BoxIdentifier boxId, [NotNull] string documentCirculationId, [NotNull] BoxEventContent boxEventContent)
        {
            return WriteEvent(boxId, Guid.Parse(documentCirculationId), boxEventContent);
        }

        public Guid WriteEvent([NotNull] BoxIdentifier boxId, Guid documentCirculationId, [NotNull] BoxEventContent boxEventContent)
        {
            var eventId = Guid.NewGuid();
            var protoBoxEvent = new ProtoBoxEvent(eventId, boxId, documentCirculationId, boxEventContent);
            lazyAllBoxEventSeriesWriter.Value.Write(protoBoxEvent);
            return eventId;
        }

        private readonly Lazy<AllBoxEventSeriesWriter> lazyAllBoxEventSeriesWriter;
    }
}