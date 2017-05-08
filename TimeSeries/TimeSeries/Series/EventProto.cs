using System;
using Commons.TimeBasedUuid;

namespace CassandraTimeSeries.Model
{
    public class EventProto
    {
        public Guid UserId { get; set; }
        public byte[] Payload { get; set; }

        public EventProto(Guid userId, byte[] payload=null)
        {
            UserId = userId;
            Payload = payload ?? new byte[] {0xff, 0xff, 0xff, 0xff};
        }

        public EventProto(byte[] payload=null) : this(TimeGuid.NowGuid().ToGuid(), payload) { }
    }
}
