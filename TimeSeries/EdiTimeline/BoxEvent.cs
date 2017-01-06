using System;
using System.Diagnostics.CodeAnalysis;
using Commons;
using JetBrains.Annotations;

namespace EdiTimeline
{
    public class BoxEvent
    {
        public BoxEvent([NotNull] BoxIdentifier boxId, Guid documentCirculationId, Guid eventId, [NotNull] Timestamp eventTimestamp, [NotNull] Lazy<BoxEventContent> eventContent)
        {
            if(boxId == null)
                throw new InvalidProgramStateException(string.Format("boxId is required for documentCirculationId: {0}, eventId: {1}", documentCirculationId, eventId));
            if(eventTimestamp == null)
                throw new InvalidProgramStateException(string.Format("eventTimestamp is required for documentCirculationId: {0}, eventId: {1}, boxId: {2}", documentCirculationId, eventId, boxId));
            if(eventContent == null)
                throw new InvalidProgramStateException(string.Format("eventContent is required for documentCirculationId: {0}, eventId: {1}, boxId: {2}", documentCirculationId, eventId, boxId));
            BoxId = boxId;
            DocumentCirculationId = documentCirculationId;
            EventId = eventId;
            EventTimestamp = eventTimestamp;
            EventContent = eventContent;
        }

        [NotNull]
        public BoxIdentifier BoxId { get; private set; }

        public Guid DocumentCirculationId { get; private set; }

        public Guid EventId { get; private set; }

        [NotNull]
        public Timestamp EventTimestamp { get; private set; }

        // note: not a field because grobuf is configured to use AllPropertiesExtractor
        // note: CanBeNull in case of a missing specific BoxEventContent contract (unknown types are silently deserialized to null)
        [CanBeNull]
        private Lazy<BoxEventContent> EventContent { get; set; }

        [CanBeNull]
        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        public BoxEventContent TryGetEventContent()
        {
            return EventContent == null ? null : EventContent.Value;
        }

        [NotNull]
        public BoxEventContent GetEventContent()
        {
            if(EventContent == null)
                throw new InvalidProgramStateException(string.Format("EventContent == null for: {0}", this));
            var eventContentValue = EventContent.Value;
            if(eventContentValue == null)
                throw new InvalidProgramStateException(string.Format("EventContent.Value == null for: {0}", this));
            return eventContentValue;
        }

        public override string ToString()
        {
            return ToString(evaluateEventContent : true);
        }

        public string ToString(bool evaluateEventContent)
        {
            var eventContentString = "NOT_EVALUATED";
            if(evaluateEventContent)
            {
                var eventContent = TryGetEventContent();
                eventContentString = eventContent == null ? "NULL" : eventContent.ToString();
            }
            return string.Format("BoxId: {0}, DocumentCirculationId: {1}, EventId: {2}, EventTimestamp: {3}, EventContent: {4}", BoxId, DocumentCirculationId, EventId, EventTimestamp, eventContentString);
        }

        [NotNull]
        public BoxEvent<TEventContent> ToGeneric<TEventContent>() where TEventContent : BoxEventContent
        {
            var eventContent = GetEventContent();
            if(!(eventContent is TEventContent))
                throw new InvalidProgramStateException(string.Format("Cannot cast to {0} event content for: {1}", typeof(TEventContent).FullName, this));
            return new BoxEvent<TEventContent>(BoxId, DocumentCirculationId, EventId, EventTimestamp, (TEventContent)eventContent);
        }
    }

    public class BoxEvent<TEventContent> : BoxEvent where TEventContent : BoxEventContent
    {
        public BoxEvent([NotNull] BoxIdentifier boxId, Guid documentCirculationId, Guid eventId, [NotNull] Timestamp eventTimestamp, [NotNull] TEventContent eventContent)
            : base(boxId, documentCirculationId, eventId, eventTimestamp, new Lazy<BoxEventContent>(() => eventContent))
        {
            EventContent = eventContent;
        }

        [NotNull]
        public TEventContent EventContent { get; private set; }
    }
}