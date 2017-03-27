using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cassandra;
using Cassandra.Mapping.Attributes;
using Commons.TimeBasedUuid;

namespace CassandraTimeSeries.Model
{
    [Table("bulk_time_series")]
    public class EventsCollection : IEnumerable<EventProto>
    {
        [PartitionKey]
        [Column("partition_id")]
        public long PartitionId { get; set; }

        [ClusteringKey]
        [Column("time_uuid")]
        public TimeUuid TimeUuid { get; set; }

        [StaticColumn]
        [Column("max_id_in_partition")]
        public TimeUuid MaxIdInPartition { get; set; } = TimeGuid.MaxValue.ToTimeUuid();

        [Column("user_ids")]
        public Guid[] UserIds { get; set; }

        [Column("payloads")]
        public byte[][] Payloads { get; set; }

        public TimeGuid TimeGuid => TimeUuid.ToTimeGuid();

        public IEnumerator<EventProto> GetEnumerator()
        {
            for (var i = 0; i < UserIds.Length; ++i)
                yield return new EventProto(UserIds[i], Payloads[i]);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public EventsCollection() { }

        public EventsCollection(TimeUuid id, long partitionId, params EventProto[] eventProtos)
        {
            TimeUuid = id;
            PartitionId = partitionId;

            Payloads = eventProtos.Select(x => x.Payload).ToArray();
            UserIds = eventProtos.Select(x => x.UserId).ToArray();
        }

        public EventsCollection(TimeGuid id, long partitionId, IEnumerable<EventProto> eventProtos)
            : this(id.ToTimeUuid(), partitionId, eventProtos.ToArray()) { }

        public EventsCollection(TimeGuid id, long partitionId, params EventProto[] eventProtos)
            : this(id.ToTimeUuid(), partitionId, eventProtos) { }
    }
}