using Cassandra;
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
        public SeriesDatabase Database { get; private set; }
        public TimeSpan SliceDuration {get; private set;}

        public TimeSeries(Cluster cluster, string keyspaceName, string tableName, TimeSpan sliceDuration)
        {
            Database = new SeriesDatabase(cluster, keyspaceName, tableName);
            SliceDuration = sliceDuration;
        }

        public void Write(Event ev)
        {
            ev.SliceId = ev.Timestamp.RoundDown(SliceDuration).Ticks;
            Database.CreateMapper().Insert(ev);
        }

        public void ParallelWrite(IEnumerable<Event> events)
        {
            var mapper = Database.CreateMapper();

            events.AsParallel().ForAll(e =>
            {
                e.SliceId = e.Timestamp.RoundDown(SliceDuration).Ticks;
                mapper.Insert(e);
            });
        }

        //public List<Event> ReadRange(TimeUuid startInclusive, TimeUuid endExclusive, int count)
        //{

        //}

        public IEnumerable<Event> ReadRange(DateTimeOffset startInclusive, DateTimeOffset endExclusive)
        {
            var mapper = Database.CreateMapper();

            return new TimeSlices(startInclusive, endExclusive, SliceDuration).AsParallel()
                .SelectMany(id => mapper.Fetch<Event>(@"WHERE SliceId = ? AND EventId > minTimeuuid(?) AND EventId < maxTimeuuid(?)", id.Ticks, startInclusive, endExclusive));
        }
    }
}
