using System;
using Cassandra.Mapping.Attributes;
using Commons.TimeBasedUuid;

namespace CassandraTimeSeries.Model
{
    public class EventProto
    {
        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("payload")]
        public byte[] Payload { get; set; }

        public EventProto(Guid userId, byte[] payload=null)
        {
            UserId = userId;
            Payload = payload ?? new byte[] {0xff, 0xff, 0xff, 0xff};
        }

        public EventProto(byte[] payload=null) : this(TimeGuid.NowGuid().ToGuid(), payload) { }
    }
}
