using System;

namespace CassandraTimeSeries.Model
{
    class OperationTimeoutException : Exception
    {
        public OperationTimeoutException(uint millisecondsTimeout, EventProto rejectedEvent)
            : base($"Failed to perform operation after {millisecondsTimeout} ms. Event is rejected: {rejectedEvent}") { }

        public OperationTimeoutException(uint millisecondsTimeout)
            : base($"Failed to perform operation after {millisecondsTimeout} ms.") { }
    }
}