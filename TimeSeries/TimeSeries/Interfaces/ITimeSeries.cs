﻿using System.Collections.Generic;
using CassandraTimeSeries.Model;
using Commons;
using Commons.TimeBasedUuid;

namespace CassandraTimeSeries.Interfaces
{
    public interface ITimeSeries
    {
        void Write(Event ev);
        List<Event> ReadRange(Timestamp startExclusive, Timestamp endInclusive, int count=1000);
        List<Event> ReadRange(TimeGuid startExclusive, TimeGuid endInclusive, int count=1000);
    }
}
