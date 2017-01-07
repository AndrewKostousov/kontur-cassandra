using System;
using JetBrains.Annotations;

namespace EdiTimeline
{
    public interface IBoxEventsWriter
    {
        Guid WriteEvent([NotNull] byte[] payload);
    }
}