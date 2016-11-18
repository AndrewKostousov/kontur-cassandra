using Cassandra;
using System;
using System.Collections.Generic;
using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.Objects.TimeBasedUuid;

namespace CassandraTimeSeries
{
    public interface ITimeSeries
    {
        void Write(Event ev);
        List<Event> ReadRange(DateTimeOffset? startInclusive, DateTimeOffset? endExclusive, int count=1000);
        List<Event> ReadRange(TimeUuid? startInclusive, TimeUuid? endExclusive, int count=1000);
    }
}
