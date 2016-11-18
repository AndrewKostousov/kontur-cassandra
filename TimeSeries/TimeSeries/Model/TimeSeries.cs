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
        public Table<Event> Table { get; }
        
        public TimeSeries(Table<Event> table)
        {
            Table = table;
        }

        public void Write(Event ev)
        {
            Table.Insert(ev).Execute();
        }
        
        public List<Event> ReadRange(TimeUuid startInclusive, TimeUuid endExclusive, int count)
        {
            return new TimeSlices(startInclusive.GetDate(), endExclusive.GetDate(), Event.SliceDutation)
                .Select(sliceId => sliceId.Ticks)
                .SelectMany(sliceId => GetRangeFromTable(startInclusive, endExclusive, sliceId, count))
                .Take(count)
                .ToList();
        }

        public List<Event> ReadRange(DateTimeOffset startInclusive, DateTimeOffset endExclusive, int count)
        {
            return startInclusive == endExclusive
                ? ReadRange(startInclusive.MinTimeUuid(), endExclusive.MaxTimeUuid(), count)
                : ReadRange(startInclusive.MinTimeUuid(), endExclusive.MinTimeUuid(), count);
        }

        private IEnumerable<Event> GetRangeFromTable(
            TimeUuid start, TimeUuid end,
            long sliceId, int count)
        {
            Expression<Func<Event, bool>> idCondition;

            if (start == end)
                idCondition = (e => e.SliceId == sliceId && e.Id.CompareTo(start) == 0);
            else
                idCondition = (e => e.SliceId == sliceId && e.Id.CompareTo(start) >= 0 && e.Id.CompareTo(end) < 0);

            return Table
                .Where(idCondition)
                .Take(count)
                .Execute();
        }
    }
}