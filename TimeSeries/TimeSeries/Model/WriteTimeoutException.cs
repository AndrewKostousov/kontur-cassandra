using System;

namespace CassandraTimeSeries.Model
{
    class WriteTimeoutException : Exception
    {
        public WriteTimeoutException(uint attemptsCount)
            : base($"Write attempts limit exceeded: unable to write event after {attemptsCount} attempt{(attemptsCount == 1 ? "" : "s")}") { }
    }
}