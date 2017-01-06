using System;
using JetBrains.Annotations;

namespace EdiTimeline
{
    public interface IBoxEventsWriter
    {
        Guid WriteEvent([NotNull] BoxIdentifier boxId, Guid documentCirculationId, [NotNull] BoxEventContent boxEventContent);
        Guid WriteEvent([NotNull] BoxIdentifier boxId, [NotNull] string documentCirculationId, [NotNull] BoxEventContent boxEventContent);
    }
}