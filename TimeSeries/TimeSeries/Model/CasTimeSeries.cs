using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using Cassandra.Data.Linq;
using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Utils;
using Commons;
using Commons.TimeBasedUuid;

namespace CassandraTimeSeries.Model
{
    public class CasTimeSeries : ITimeSeries
    {
        private readonly Table<Event> table;
        
        public CasTimeSeries(Table<Event> table)
        {
            this.table = table;
        }

        private TimeGuid lastId = TimeGuid.MinValue;

        public Event Write(EventProto ev)
        {
            Event eventToWrite;

            do
            {
                eventToWrite = new Event(lastId, ev);
                lastId = new TimeGuid(lastId.GetTimestamp().AddMilliseconds(1), lastId.GetClockSequence(), lastId.GetNode());

            } while (!table.Insert(eventToWrite).IfNotExists().Execute().Applied);
             
            return eventToWrite;
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
                return table.Execute().OrderBy(ev => ev.Id).Take(count).ToList();

            if (!start.HasValue)
                return GetFromTableAndSort(count, ev => ev.Id.CompareTo(end.Value) <= 0);

            if (!end.HasValue)
                return GetFromTableAndSort(count, ev => ev.Id.CompareTo(start.Value) > 0);

            var slices = TimeSlicer
                .Slice(startExclusive.GetTimestamp(), endInclusive.GetTimestamp(), Event.SliceDutation)
                .Select(s => s.Ticks);

            return table.Where(e => slices.Contains(e.SliceId) && e.Id.CompareTo(start.Value) > 0 && e.Id.CompareTo(end.Value) <= 0)
                .Take(count)
                .Execute()
                .ToList();
        }

        private List<Event> GetFromTableAndSort(int count, Expression<Func<Event, bool>> query)
        {
            return table
                .AllowFiltering()
                .Where(query)
                .Execute()
                .OrderBy(x => x.Id)
                .Take(count)
                .ToList();
        }
    }
}