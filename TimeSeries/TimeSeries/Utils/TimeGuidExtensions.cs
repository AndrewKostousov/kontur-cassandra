using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commons.TimeBasedUuid;

namespace CassandraTimeSeries.Utils
{
    static class TimeGuidExtensions
    {
        public static TimeGuid Increment(this TimeGuid timeGuid)
        {
            return new TimeGuid(timeGuid.GetTimestamp().AddMilliseconds(1), timeGuid.GetClockSequence(), timeGuid.GetNode());
        }
    }
}
