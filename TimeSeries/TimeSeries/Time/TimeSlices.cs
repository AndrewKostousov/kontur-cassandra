using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CassandraTimeSeries
{
    public class TimeSlices : IEnumerable<DateTimeOffset>
    {
        public DateTimeOffset From { get; private set; }
        public DateTimeOffset To { get; private set; }
        public TimeSpan SliceDuration { get; private set; }

        public TimeSlices(DateTimeOffset from, DateTimeOffset to, TimeSpan sliceDuration)
        {
            From = from; To = to;
            SliceDuration = sliceDuration;
        }

        private IEnumerable<DateTimeOffset> Enumerate()
        {
            var currentSlice = From.RoundDown(SliceDuration);

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
