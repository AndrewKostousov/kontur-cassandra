using System;
using Commons.TimeBasedUuid;
using JetBrains.Annotations;

namespace Commons
{
    public static class TimestampExtensions
    {
        [NotNull]
        public static Timestamp Floor([NotNull] this Timestamp timestamp, TimeSpan precision)
        {
            if (precision.Ticks <= 0)
                throw new InvalidProgramStateException($"Could not run Floor with {precision} precision");
            return new Timestamp((timestamp.Ticks / precision.Ticks) * precision.Ticks);
        }

        [NotNull]
        public static TimeGuid MinTimeGuid([NotNull] this Timestamp timestamp)
        {
            return TimeGuid.MinForTimestamp(timestamp);
        }

        [NotNull]
        public static TimeGuid MaxTimeGuid([NotNull] this Timestamp timestamp)
        {
            return TimeGuid.MaxForTimestamp(timestamp);
        }
    }
}