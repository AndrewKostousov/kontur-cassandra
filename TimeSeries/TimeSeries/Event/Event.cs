using Cassandra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CassandraTimeSeries
{
    public class Event
    {
        public TimeUuid Id { get; set; }
        public long SliceId { get; set; }
        public DateTimeOffset Timestamp { get { return Id.GetDate(); } }
        public byte[] Payload { get; set; }

        public Event() 
            : this(DateTimeOffset.UtcNow, 0) { }

        public Event(int payloadSize) 
            : this(DateTimeOffset.UtcNow, payloadSize) { }

        public Event(DateTimeOffset time, int payloadSize)
        {
            Id = TimeUuid.NewId(time);
            Payload = new byte[payloadSize];
        }

        public override string ToString()
        {
            return String.Format("Event {0} at {1}, payload size: {2}", Id, Timestamp, Payload.Length);
        }
    }
}
