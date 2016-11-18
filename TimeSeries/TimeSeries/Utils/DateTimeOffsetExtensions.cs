using Cassandra;
using System;

namespace CassandraTimeSeries
{
    public static class DateTimeOffsetExtensions
    {
        public static DateTimeOffset RoundDown(this DateTimeOffset dtoffset, TimeSpan precise)
        {
            if(precise.Ticks <= 0)
                throw new InvalidOperationException($"Precise must be positive: {precise}");
            return new DateTimeOffset(dtoffset.Ticks - dtoffset.Ticks % precise.Ticks, TimeSpan.Zero);
        }

        private static readonly byte[] 
            MinNode = {0x80, 0x80, 0x80, 0x80, 0x80, 0x80}, 
            MaxNode = { 0x7f, 0x7f, 0x7f, 0x7f, 0x7f, 0x7f },
            MinClockSequence = { 0x00, 0x80 },
            MaxClockSequence = { 0x3f, 0x7f };

        public static TimeUuid MinTimeUuid(this DateTimeOffset date)
        {
            return TimeUuid.NewId(MinNode, MinClockSequence, date);
        }

        public static TimeUuid MaxTimeUuid(this DateTimeOffset date)
        {
            return TimeUuid.NewId(MaxNode, MaxClockSequence, date);
        }
    }
}
