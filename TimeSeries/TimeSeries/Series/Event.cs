using System;
using System.Collections;
using System.Collections.Generic;
using Cassandra;
using Cassandra.Mapping.Attributes;
using Commons;
using Commons.TimeBasedUuid;

namespace CassandraTimeSeries.Model
{
    [Table("bulk_time_series")]
    public class EventsCollection : IEnumerable<Event>
    {
        [PartitionKey]
        [Column("partition_id")]
        public long PartitionId { get; set; }

        [ClusteringKey]
        [Column("last_event_id")]
        public TimeUuid LastEventId { get; set; }

        [StaticColumn]
        [Column("max_event_id")]
        public TimeUuid MaxEventId { get; set; } = TimeGuid.MaxValue.ToTimeUuid();

        [Column("event_ids")]
        public TimeUuid[] EventIds { get; set; }

        [Column("user_ids")]
        public Guid[] UserIds { get; set; }

        [Column("payloads")]
        public byte[][] Payloads { get; set; }

        public IEnumerator<Event> GetEnumerator()
        {
            for (var i = 0; i < EventIds.Length; ++i)
                yield return new Event(EventIds[i].ToTimeGuid(), new EventProto(UserIds[i], Payloads[i]));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

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
