using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons
{
    public static class EnumerableExtensions
    {
        public static TimeSpan Average(this IEnumerable<TimeSpan> source)
        {
            return TimeSpan.FromTicks((long)source.DefaultIfEmpty(TimeSpan.Zero).Average(x => x.Ticks));
        }

        public static TimeSpan Sum(this IEnumerable<TimeSpan> source)
        {
            return TimeSpan.FromTicks(source.Sum(x => x.Ticks));
        }
    }
}
