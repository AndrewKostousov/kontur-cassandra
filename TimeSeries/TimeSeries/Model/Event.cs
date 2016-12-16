using System;
using Cassandra;
using Cassandra.Mapping.Attributes;
using CassandraTimeSeries.Utils;
using Commons;
using Commons.TimeBasedUuid;

namespace CassandraTimeSeries.Model
{
    [Table("time_series")]
    public class Event : EventProto
    {
        public static TimeSpan SliceDutation => TimeSpan.FromMinutes(1);

        [PartitionKey]
        [Column("slice_id")]
        public long SliceId { get; set; }
        
        [ClusteringKey]
        [Column("event_id")]
        public TimeUuid Id { get; set; }
        
        public Timestamp Timestamp => new Timestamp(Id.GetDate());

        public Event() { }

        public Event(TimeGuid id, EventProto proto)
        {
            Id = id.ToTimeUuid();
            SliceId = new Timestamp(Id.GetDate()).Floor(SliceDutation).Ticks;
            Payload = proto.Payload;
            UserId = proto.UserId;
        }

        public override string ToString()
        {
            return $"Event {Id} at {Timestamp}, user_id: {UserId}";
        }
    }
}
