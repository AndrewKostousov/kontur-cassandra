using System;
using System.Collections.Generic;

namespace CassandraTimeSeries
{
    public static class TimeSlicer
    {
        public static IEnumerable<DateTimeOffset> Slice(DateTimeOffset from, DateTimeOffset to, TimeSpan sliceDuration)
        {
            var currentSlice = from.RoundDown(sliceDuration);

            if (currentSlice == to)
                yield return currentSlice;

            while (currentSlice < to)
            {
                yield return currentSlice;
                currentSlice += sliceDuration;
            }
        }
    }
}
