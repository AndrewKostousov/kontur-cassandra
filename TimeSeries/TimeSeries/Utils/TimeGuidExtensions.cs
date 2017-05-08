using Commons.TimeBasedUuid;

namespace CassandraTimeSeries.Utils
{
    static class TimeGuidExtensions
    {
        public static TimeGuid AddMilliseconds(this TimeGuid timeGuid, int milliseconds)
        {
            return TimeGuid.NewGuid(timeGuid.GetTimestamp().AddMilliseconds(milliseconds), timeGuid.GetClockSequence());
        }

        public static TimeGuid AddTicks(this TimeGuid timeGuid, long ticks)
        {
            return TimeGuid.NewGuid(timeGuid.GetTimestamp().AddTicks(ticks), timeGuid.GetClockSequence());
        }
    }
}
