using Cassandra;
using Cassandra.Data.Linq;
using Cassandra.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CassandraTimeSeries
{
    public class TimeSeries : ITimeSeries
    {
        public Table<Event> Table { get; private set; }
        
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
                .SelectMany(sliceId => GetRangeFromTable(sliceId, startInclusive, endExclusive, count))
                .Take(count)
                //.OrderBy(e => e.Id)
                .ToList();
        }

        public List<Event> ReadRange(DateTimeOffset startInclusive, DateTimeOffset endExclusive, int count)
        {
            return ReadRange(startInclusive.MinTimeUuid(), endExclusive.MaxTimeUuid(), count);
        }

        private IEnumerable<Event> GetRangeFromTable(long sliceId, TimeUuid start, TimeUuid end, int count)
        {
            return Table
                .Where(e => e.SliceId == sliceId && e.Id.CompareTo(start) >= 0 && e.Id.CompareTo(end) < 0)
                .Take(count)
                //.OrderBy(e => e.Id)
                .Execute();
        }
    }
}
