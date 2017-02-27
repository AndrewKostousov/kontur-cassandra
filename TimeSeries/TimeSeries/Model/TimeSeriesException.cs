using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CassandraTimeSeries.Model
{
    public class TimeSeriesException : Exception
    {
        public TimeSeriesException(string message) : base(message) { }
    }
}
