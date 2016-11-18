using System;
using System.Collections.Generic;

namespace CassandraTimeSeries
{
    public class TimeSlices : IEnumerable<DateTimeOffset>
    {
        public DateTimeOffset From { get; }
        public DateTimeOffset To { get; }
        public TimeSpan SliceDuration { get; }

        public TimeSlices(DateTimeOffset from, DateTimeOffset to, TimeSpan sliceDuration)
        {
            From = from; To = to;
            SliceDuration = sliceDuration;
        }

        private IEnumerable<DateTimeOffset> Enumerate()
        {
            var currentSlice = From.RoundDown(SliceDuration);

            if (currentSlice == To)
                yield return currentSlice;

            while (currentSlice < To)
            {
                yield return currentSlice;
                currentSlice += SliceDuration;
            }
        }

        public IEnumerator<DateTimeOffset> GetEnumerator()
        {
            return Enumerate().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
