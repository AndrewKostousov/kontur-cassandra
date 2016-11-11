using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CassandraTimeSeries
{
    public static class DateTimeOffsetExtensions
    {
        public static DateTimeOffset RoundDown(this DateTimeOffset dtoffset, TimeSpan precise)
        {
            return new DateTimeOffset(dtoffset.Ticks - dtoffset.Ticks % precise.Ticks, TimeSpan.Zero);
        }
    }
}
