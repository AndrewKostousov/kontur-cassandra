using Cassandra;
using Cassandra.Mapping.Attributes;
using System;
using System.Linq;

namespace CassandraTimeSeries
{
    [Table("time_series")]
    public class Event
    {
        public static TimeSpan SliceDutation => TimeSpan.FromMinutes(1);

        [PartitionKey]
        [Column("slice_id")]
        public long SliceId { get; set; }
        
        [ClusteringKey]
        [Column("event_id")]
        public TimeUuid Id { get; set; }
        
        [Column("payload")]
        public byte[] Payload { get; set; }

        public DateTimeOffset Timestamp => Id.GetDate();

        public Event() { }

        public Event(DateTimeOffset time, byte[] payload=null)
        {
            Id = TimeUuid.NewId(time);
            SliceId = Timestamp.RoundDown(SliceDutation).Ticks;
            Payload = payload;
        }

        public override string ToString()
        {
            return $"Event {Id} at {Timestamp}, payload: {Payload.Length} bytes";
        }
    }
}
