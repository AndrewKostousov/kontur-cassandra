using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using Cassandra;
using Cassandra.Data.Linq;
using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Utils;
using Commons;
using Commons.TimeBasedUuid;

namespace CassandraTimeSeries.Model
{
    public class TimeSeries : ITimeSeries
    {
        private readonly Table<Event> table;
        
        public TimeSeries(Table<Event> table)
        {
            this.table = table;
        }

        public TimeGuid Write(EventProto ev)
        {
            var nowGuid = TimeGuid.NowGuid();
            var eventToWrite = new Event(nowGuid, ev);
            table.Insert(eventToWrite).Execute();

            return nowGuid;
        }

        public List<Event> ReadRange(Timestamp startExclusive, Timestamp endInclusive, int count = 1000)
        {
            var startGuid = startExclusive?.MaxTimeGuid();
            var endGuid = endInclusive?.MaxTimeGuid();

            return ReadRange(startGuid, endGuid, count);
        }

        public List<Event> ReadRange(TimeGuid startExclusive, TimeGuid endInclusive, int count = 1000)
        {
            var start = startExclusive?.ToTimeUuid();
            var end = endInclusive?.ToTimeUuid();

            if (!start.HasValue && !end.HasValue)
                return table.Take(count).Execute().ToList();

            if (!start.HasValue)
                return table.Where(e => e.Id.CompareTo(end.Value) <= 0).Take(count).Execute().ToList();

            if (!end.HasValue)
                return table.Where(e => e.Id.CompareTo(start.Value) > 0).Take(count).Execute().ToList();
            
            return TimeSlicer.Slice(startExclusive.GetTimestamp(), endInclusive.GetTimestamp(), Event.SliceDutation)
                .SelectMany(sliceId => table
                    .Where(e => e.SliceId == sliceId.Ticks && e.Id.CompareTo(start.Value) > 0 && e.Id.CompareTo(end.Value) <= 0)
                    .Take(count)
                    .Execute())
                .Take(count)
                .ToList();
        }
    }
}