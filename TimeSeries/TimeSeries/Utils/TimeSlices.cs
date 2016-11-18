using System;
using System.Collections.Generic;
using SKBKontur.Catalogue.Objects;

namespace CassandraTimeSeries
{
    public static class TimeSlicer
    {
        public static IEnumerable<Timestamp> Slice(Timestamp from, Timestamp to, TimeSpan sliceDuration)
        {
            var currentSlice = from.Floor(sliceDuration);

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
