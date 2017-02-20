using System.Collections.Generic;
using Benchmarks.ReadWrite;

namespace Benchmarks.Results
{
    class WriteStatistics : WorkerStatistics
    {
        public WriteStatistics(List<BenchmarkEventWriter> writers) : base(writers) { }

        public string CreateReport()
        {
            return $"Writers count: {WorkersCount}\r\n\r\n" +
                   $"Average single write latency: {AverageOperationLatency.TotalMilliseconds} ms\r\n" +
                   $"95% of writes were faster than {Latency95ThPercentile.TotalMilliseconds} ms\r\n" +
                   $"98% of writes were faster than {Latency99ThPercentile.TotalMilliseconds} ms\r\n" +
                   $"Average write throughput: {TotalThroughput} ev/s\r\n\r\n" +

                   $"Total writes count: {TotalOperationsCount}\r\n" +
                   $"Average writes by single thread: {AverageOperationsPerThread}\r\n\r\n";
        }
    }
}