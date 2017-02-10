using System;
using System.Collections.Generic;
using CassandraTimeSeries.Interfaces;
using Commons;
using Commons.TimeBasedUuid;

namespace CassandraTimeSeries.Model
{
    class AllBoxEventSeriesWrapper : ITimeSeries
    {
        public Event Write(EventProto ev)
        {
            throw new NotImplementedException();
        }

        public List<Event> ReadRange(Timestamp startExclusive, Timestamp endInclusive, int count = 1000)
        {
            throw new NotImplementedException();
        }

        public List<Event> ReadRange(TimeGuid startExclusive, TimeGuid endInclusive, int count = 1000)
        {
            throw new NotImplementedException();
        }
    }
}
