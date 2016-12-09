using System.Collections.Generic;
using Benchmarks.ReadWrite;

namespace Benchmarks.Results
{
    class WriteStatistics : WorkerStatistics
    {
        public WriteStatistics(List<BenchmarkEventWriter> writers) : base(writers) { }

        public string CreateReport()
        {
            return $"Average write latency: {AverageLatency.TotalMilliseconds} ms\n" +
                   $"95% of writes were faster than: {Latency95ThPercentile.TotalMilliseconds} ms\n" +
                   $"98% of writes were faster than: {Latency98ThPercentile.TotalMilliseconds} ms\n" +
                   $"Average write throughput: {TotalThroughput} ev/s\n\n" +

                   $"Total writes count: {TotalOperationsCount}\n" +
                   $"Average writes by single thread: {AverageOperationsPerThread}\n" +
                   $"95% of threads made less than: {Operations95ThPercentile} writes\n" +
                   $"98% of threads made less than: {Operations98ThPercentile} writes";
        }
    }
}