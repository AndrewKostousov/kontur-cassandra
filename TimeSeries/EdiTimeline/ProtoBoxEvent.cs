using System;
using Commons;
using JetBrains.Annotations;

namespace EdiTimeline
{
    public class ProtoBoxEvent
    {
        public ProtoBoxEvent(Guid eventId, [NotNull] BoxIdentifier boxId, Guid documentCirculationId, [NotNull] BoxEventContent eventContent)
        {
            if (boxId == null)
                throw new InvalidProgramStateException(string.Format("boxId is required for eventId: {0}, documentCirculationId: {1}, eventContent: {2}", eventId, documentCirculationId, eventContent));
            if (eventContent == null)
                throw new InvalidProgramStateException(string.Format("eventContent is required for eventId: {0}, documentCirculationId: {1}, boxId: {2}", eventId, documentCirculationId, boxId));
            EventId = eventId;
            BoxId = boxId;
            DocumentCirculationId = documentCirculationId;
            EventContent = eventContent;
        }

        public Guid EventId { get; private set; }

        [NotNull]
        public BoxIdentifier BoxId { get; private set; }

        public Guid DocumentCirculationId { get; private set; }

        [NotNull]
        public BoxEventContent EventContent { get; private set; }

        public override string ToString()
        {
            return string.Format("EventId: {0}, BoxId: {1}, DocumentCirculationId: {2}, EventContent: {3}", EventId, BoxId, DocumentCirculationId, EventContent);
        }
    }
}