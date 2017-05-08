using System;
using Cassandra;
using Cassandra.Data.Linq;
using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Model;
using CassandraTimeSeries.Utils;
using Commons;
using Commons.TimeBasedUuid;

namespace CassandraTimeSeries.Series
{
    public abstract class BaseTimeSeries : ITimeSeries
    {
        public TimeLinePartitioner Partitioner { get; }
        
        protected readonly Table<EventsCollection> EventsTable;
        protected readonly uint OperationalTimeoutMilliseconds;
        protected readonly TimeGuidGenerator TimeGuidGenerator;
        protected readonly ISession Session;

        protected BaseTimeSeries(Table<EventsCollection> eventsTable, TimeLinePartitioner partitioner, uint operationalTimeoutMilliseconds)
        {
            EventsTable = eventsTable;
            OperationalTimeoutMilliseconds = operationalTimeoutMilliseconds;
            Partitioner = partitioner;
            Session = eventsTable.GetSession();

            TimeGuidGenerator = new TimeGuidGenerator(new PreciseTimestampGenerator(TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(100)));
        }

        public abstract Timestamp[] Write(params EventProto[] events);

        protected TimeGuid NewTimeGuid()
        {
            return new TimeGuid(TimeGuidGenerator.NewGuid());
        }

        protected EventsCollection PackIntoCollection(EventProto[] eventProtos, TimeGuid id)
        {
            return new EventsCollection(id, Partitioner.CreatePartitionId(id.GetTimestamp()), eventProtos);
        }

        public abstract void WriteWithoutSync(Event ev);

        public Event[] ReadRange(Timestamp startExclusive, Timestamp endInclusive, int count = 1000)
        {
            var startGuid = startExclusive?.MaxTimeGuid();
            var endGuid = endInclusive?.MaxTimeGuid();

            return ReadRange(startGuid, endGuid, count);
        }

        public abstract Event[] ReadRange(TimeGuid startExclusive, TimeGuid endInclusive, int count = 1000);
    }
}
