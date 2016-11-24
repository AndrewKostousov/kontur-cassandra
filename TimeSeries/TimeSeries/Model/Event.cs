using Cassandra;
using Cassandra.Mapping.Attributes;
using System;
using System.Linq;
using Commons;
using Commons.TimeBasedUuid;
using SKBKontur.Catalogue.CassandraStorageCore.CqlCore;

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

        public Timestamp Timestamp => new Timestamp(Id.GetDate());

        public Event() { }

        public Event(Timestamp time, byte[] payload=null)
        {
            Id = TimeGuid.NewGuid(time).ToTimeUuid();
            SliceId = time.Floor(SliceDutation).Ticks;
            Payload = payload;
        }

        public Event(TimeGuid id, byte[] payload = null)
        {
            Id = id.ToTimeUuid();
            SliceId = new Timestamp(Id.GetDate()).Floor(SliceDutation).Ticks;
            Payload = payload;
        }

        public override string ToString()
        {
            return $"Event {Id} at {Timestamp}, payload: {Payload.Length} bytes";
        }
    }
}
