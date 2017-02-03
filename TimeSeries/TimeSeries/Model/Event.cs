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
        //public static TimeSpan SliceDutation => TimeSpan.FromHours(10);
        public static TimeSpan SliceDutation => TimeSpan.FromMinutes(1);

        [PartitionKey]
        [Column("slice_id")]
        public long SliceId { get; set; }
        
        [ClusteringKey]
        [Column("event_id")]
        public TimeUuid Id { get; set; }

        [StaticColumn]
        [Column("max_ticks")]
        public long MaxTicks { get; set; } = TimeGuid.MinValue.GetTimestamp().Ticks;

        [Column("ticks")]
        public long Ticks { get; set; }

        public Timestamp Timestamp => Id.ToTimeGuid().GetTimestamp();

        public TimeGuid TimeGuid => Id.ToTimeGuid();

        public Event() { }

        public Event(TimeGuid id, EventProto proto)
        {
            Id = id.ToTimeUuid();
            SliceId = new Timestamp(Id.GetDate()).Floor(SliceDutation).Ticks;
            Payload = proto.Payload;
            UserId = proto.UserId;

            Ticks = id.GetTimestamp().Ticks;
            MaxTicks = Ticks > MaxTicks ? Ticks : MaxTicks;
        }

        public override string ToString()
        {
            return $"Event {Id} at {Timestamp}, user_id: {UserId}";
        }
    }
}
