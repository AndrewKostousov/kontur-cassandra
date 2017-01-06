using System;
using JetBrains.Annotations;

namespace EdiTimeline
{
    public class AllBoxEventSeriesColumnValue
    {
        public AllBoxEventSeriesColumnValue([NotNull] BoxIdentifier boxId, Guid documentCirculationId, [NotNull] Lazy<BoxEventContent> eventContent, bool eventIsCommitted)
        {
            BoxId = boxId;
            DocumentCirculationId = documentCirculationId;
            EventContent = eventContent;
            EventIsCommitted = eventIsCommitted;
        }

        [NotNull]
        public BoxIdentifier BoxId { get; private set; }

        public Guid DocumentCirculationId { get; private set; }

        [NotNull]
        public Lazy<BoxEventContent> EventContent { get; private set; }

        public bool EventIsCommitted { get; private set; }

        public override string ToString()
        {
            return string.Format("BoxId: {0}, DocumentCirculationId: {1}, EventIsCommitted: {2}", BoxId, DocumentCirculationId, EventIsCommitted);
        }
    }
}