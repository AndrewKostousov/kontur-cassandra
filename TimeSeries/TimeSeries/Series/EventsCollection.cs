using System;
using System.Collections;
using System.Collections.Generic;
using Cassandra;
using Cassandra.Mapping.Attributes;
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
        [Column("max_id_in_partition")]
        public TimeUuid MaxIdInPartition { get; set; } = TimeGuid.MaxValue.ToTimeUuid();

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
}