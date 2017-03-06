using System;
using Cassandra;
using Commons;
using Commons.TimeBasedUuid;

namespace CassandraTimeSeries.Model
{
    public class Event
    {
        public static TimeSpan PartitionDutation => TimeSpan.FromMinutes(1);
        public long PartitionId { get; set; }
        public TimeUuid Id { get; set; }
        public TimeUuid MaxId { get; set; } = TimeGuid.MaxValue.ToTimeUuid();
        public Timestamp Timestamp => TimeGuid.GetTimestamp();
        public TimeGuid TimeGuid => Id.ToTimeGuid();

        public EventProto Proto { get; set; }

        public Event() { }

        public Event(TimeGuid id, EventProto proto)
        {
            Id = id.ToTimeUuid();
            PartitionId = id.GetTimestamp().Floor(PartitionDutation).Ticks;
            Proto = proto;
        }

        public override string ToString()
        {
            return $"Event {Id} at {Timestamp}";
        }
    }
}
