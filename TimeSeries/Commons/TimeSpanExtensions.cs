using System;

namespace Commons
{
    public static class TimeSpanExtensions
    {
        public static TimeSpan Multiply(this TimeSpan timeSpan, int factor)
        {
            return TimeSpan.FromTicks(timeSpan.Ticks * factor);
        }

        public static TimeSpan Divide(this TimeSpan timeSpan, int divisor)
        {
            return TimeSpan.FromTicks(timeSpan.Ticks/divisor);
        }
    }
}