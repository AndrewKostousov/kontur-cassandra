using Cassandra;
using Commons;
using Commons.TimeBasedUuid;

namespace CassandraTimeSeries.Model
{
    public class Event
    {
        public TimeUuid Id { get; }
        public Timestamp Timestamp => TimeGuid.GetTimestamp();
        public TimeGuid TimeGuid => Id.ToTimeGuid();

        public EventProto Proto { get; }

        public Event(TimeGuid id, EventProto proto)
        {
            Id = id.ToTimeUuid();
            Proto = proto;
        }

        public override string ToString()
        {
            return $"Event {Id} at {Timestamp}";
        }
    }
}
