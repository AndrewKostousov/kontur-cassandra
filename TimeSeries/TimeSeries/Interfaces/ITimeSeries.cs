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
        List<Event> ReadRange(Timestamp startInclusive, Timestamp endExclusive, int count=1000);
        List<Event> ReadRange(TimeGuid startInclusive, TimeGuid endExclusive, int count=1000);
    }
}
