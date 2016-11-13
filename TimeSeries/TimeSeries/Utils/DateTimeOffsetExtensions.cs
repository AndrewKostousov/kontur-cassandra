using Cassandra;
using System;

namespace CassandraTimeSeries
{
    public static class DateTimeOffsetExtensions
    {
        public static DateTimeOffset RoundDown(this DateTimeOffset dtoffset, TimeSpan precise)
        {
            return new DateTimeOffset(dtoffset.Ticks - dtoffset.Ticks % precise.Ticks, TimeSpan.Zero);
        }

        public static TimeUuid MinTimeUuid(this DateTimeOffset date)
        {
            return TimeUuid.NewId(new byte[] { 0x80, 0x80, 0x80, 0x80, 0x80, 0x80 },
                new byte[] { 0x00, 0x80 }, date);
        }

        public static TimeUuid MaxTimeUuid(this DateTimeOffset date)
        {
            return TimeUuid.NewId(new byte[] { 0x7f, 0x7f, 0x7f, 0x7f, 0x7f, 0x7f },
                new byte[] { 0x3f, 0x7f }, date);
        }
    }
}
