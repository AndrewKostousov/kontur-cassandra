using Cassandra;
using Cassandra.Data.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace CassandraTimeSeries
{
    public class TimeSeries : ITimeSeries
    {
        private readonly Table<Event> table;
        
        public TimeSeries(Table<Event> table)
        {
            this.table = table;
        }

        public void Write(Event ev)
        {
            table.Insert(ev).Execute();
        }
        
        public List<Event> ReadRange(TimeUuid? startInclusive, TimeUuid? endExclusive, int count)
        {
            var start = startInclusive ?? /* TODO: min timeuuid */ TimeUuid.NewId();
            var end = endExclusive ?? /* TODO: max timeuuid */ TimeUuid.NewId();

            return TimeSlicer.Slice(start.GetDate(), end.GetDate(), Event.SliceDutation)
                .Select(sliceId => sliceId.Ticks)
                .SelectMany(sliceId => GetRangeFromTable(startInclusive, endExclusive, sliceId, count))
                .Take(count)
                .ToList();
        }

        public List<Event> ReadRange(DateTimeOffset? startInclusive, DateTimeOffset? endExclusive, int count)
        {
            return startInclusive == endExclusive
                ? ReadRange(startInclusive?.MinTimeUuid(), endExclusive?.MaxTimeUuid(), count)
                : ReadRange(startInclusive?.MinTimeUuid(), endExclusive?.MinTimeUuid(), count);
        }

        private IEnumerable<Event> GetRangeFromTable(TimeUuid? start, TimeUuid? end, long sliceId, int count)
        {
            var idCondition = GetIdCondition(sliceId, start, end);

            return table
                .Where(idCondition)
                .Take(count)
                .Execute();
        }

        private Expression<Func<Event, bool>> GetIdCondition(long sliceId, TimeUuid? start, TimeUuid? end)
        {
            if (start == null && end == null)
                return e => e.SliceId == sliceId;
            if (end == null)
                return e => e.SliceId == sliceId && e.Id.CompareTo(start.Value) >= 0;
            if (start == null)
                return e => e.SliceId == sliceId && e.Id.CompareTo(end.Value) < 0;
            if (start == end)
                return e => e.SliceId == sliceId && e.Id.CompareTo(start.Value) == 0;

            return e => e.SliceId == sliceId && e.Id.CompareTo(start.Value) >= 0 && e.Id.CompareTo(end.Value) < 0;
        }
    }
}