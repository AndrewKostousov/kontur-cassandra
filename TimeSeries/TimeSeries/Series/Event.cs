using System;
using Cassandra;
using Cassandra.Mapping.Attributes;
using Commons;
using Commons.TimeBasedUuid;

namespace CassandraTimeSeries.Model
{
    [Table("time_series")]
    public class Event : EventProto
    {
        public static TimeSpan PartitionDutation => TimeSpan.FromMinutes(1);

        [PartitionKey]
        [Column("partition_id")]
        public long PartitionId { get; set; }
        
        [ClusteringKey]
        [Column("event_id")]
        public TimeUuid Id { get; set; }

        [StaticColumn]
        [Column("max_id")]
        public TimeUuid MaxId { get; set; } = TimeGuid.MaxValue.ToTimeUuid();

        public Timestamp Timestamp => TimeGuid.GetTimestamp();

        public TimeGuid TimeGuid => Id.ToTimeGuid();

        public Event() { }

        public Event(TimeGuid id, EventProto proto)
        {
            Id = id.ToTimeUuid();
            PartitionId = id.GetTimestamp().Floor(PartitionDutation).Ticks;
            Payload = proto.Payload;
            UserId = proto.UserId;
        }

        public override string ToString()
        {
            return $"Event {Id} at {Timestamp}, user_id: {UserId}";
        }
    }
}
