using Cassandra;
using JetBrains.Annotations;

namespace Commons.TimeBasedUuid
{
    public static class TimeGuidToDriverAdaptationExtensions
    {
        public static TimeUuid ToTimeUuid([NotNull] this TimeGuid timeGuid)
        {
            // it seems right to make constructor call like new TimeUuid(timeGuid.ToGuid()), but this constructor is private :(,
            // so we are forced to used implicit operator
            return (TimeUuid) timeGuid.ToGuid();
        }

        [NotNull]
        public static TimeGuid ToTimeGuid(this TimeUuid timeUuid)
        {
            return new TimeGuid(timeUuid.ToGuid());
        }
    }
}