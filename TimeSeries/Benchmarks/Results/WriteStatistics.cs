using System.Collections.Generic;
using Benchmarks.ReadWrite;

namespace Benchmarks.Results
{
    class WriteStatistics : WorkerStatistics
    {
        public WriteStatistics(List<BenchmarkEventWriter> writers) : base(writers) { }

        public string CreateReport()
        {
            return $"Writers count: {WorkersCount}\n\n" +
                   $"Average total latency: {AverageTotalLatency.TotalSeconds} s\n" +
                   $"Average single write latency: {AverageOperationLatency.TotalMilliseconds} ms\n" +
                   $"95% of writes were faster than {Latency95ThPercentile.TotalMilliseconds} ms\n" +
                   $"98% of writes were faster than {Latency98ThPercentile.TotalMilliseconds} ms\n" +
                   $"Average write throughput: {TotalThroughput} ev/s\n\n" +

                   $"Total writes count: {TotalOperationsCount}\n" +
                   $"Average writes by single thread: {AverageOperationsPerThread}\n\n";
        }
    }
}