using Cassandra;
using System;
using System.Collections.Generic;

namespace CassandraTimeSeries
{
    public interface ITimeSeries
    {
        void Write(Event ev);
        List<Event> ReadRange(DateTimeOffset startInclusive, DateTimeOffset endExclusive, int count);
        List<Event> ReadRange(TimeUuid startInclusive, TimeUuid endExclusive, int count);
    }
}
