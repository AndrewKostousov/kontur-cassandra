using System;

namespace CassandraTimeSeries.Model
{
    class WriteTimeoutException : Exception
    {
        public WriteTimeoutException(int attemptsCount)
            : base($"Write failed after {attemptsCount} attempt{(attemptsCount == 1 ? "" : "s")}") { }
    }
}