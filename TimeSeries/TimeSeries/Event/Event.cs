using Cassandra;
using Cassandra.Mapping.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CassandraTimeSeries
{
    [Table("time_series")]
    public class Event
    {
        public static TimeSpan SliceDutation { get { return TimeSpan.FromMinutes(1); } }

        [PartitionKey]
        [Column("slice_id")]
        public long SliceId { get; set; }
        
        [ClusteringKey]
        [Column("event_id")]
        public TimeUuid Id { get; set; }
        
        [Column("payload")]
        public byte[] Payload { get; set; }

        public DateTimeOffset Timestamp { get { return Id.GetDate(); } }

        public Event() 
            : this(DateTimeOffset.UtcNow) { }

        public Event(int payloadSize) 
            : this(DateTimeOffset.UtcNow, payloadSize) { }

        public Event(DateTimeOffset time, int payloadSize=0)
        {
            Id = TimeUuid.NewId(time);
            SliceId = Timestamp.RoundDown(SliceDutation).Ticks;
            Payload = new byte[payloadSize];
        }

        public override string ToString()
        {
            return String.Format("Event {0} at {1}, payload: {2} bytes", Id, Timestamp, Payload.Length);
        }

        #region ***GetHashCode and Equals***
        public override bool Equals(object obj)
        {
            var other = obj as Event;

            if (other == null) return false;

            var eqId = Id.Equals(other.Id);
            var eqSl = SliceId.Equals(other.SliceId);

            return Id.Equals(other.Id)
                && SliceId.Equals(other.SliceId)
                && Payload.SequenceEqual(other.Payload);
        }

        public override int GetHashCode()
        {
            return (Id.GetHashCode() * 1023)
                ^ (SliceId.GetHashCode() * 31)
                ^ Payload.GetHashCode(); 
        }
        #endregion
    }
}
