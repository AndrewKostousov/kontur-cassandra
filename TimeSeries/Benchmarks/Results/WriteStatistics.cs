using System;
using System.Collections.Generic;
using Benchmarks.ReadWrite;

namespace Benchmarks.Results
{
    class WriteStatistics : WorkerStatistics
    {
        public WriteStatistics(IReadOnlyList<BenchmarkEventWriter> writers) : base(writers) { }

        public string CreateReport()
        {
            var nl = Environment.NewLine;

            return $"Writers count: {WorkersCount}{nl}{nl}" +
                   $"Average single write latency: {AverageOperationLatency.TotalMilliseconds} ms{nl}" +
                   $"95% of writes were faster than {Latency95ThPercentile.TotalMilliseconds} ms{nl}" +
                   $"98% of writes were faster than {Latency99ThPercentile.TotalMilliseconds} ms{nl}" +
                   $"Average write throughput: {TotalThroughput} ev/s{nl}{nl}" +

                   $"Total writes count: {TotalOperationsCount}{nl}" +
                   $"Average writes by single thread: {AverageOperationsPerThread}{nl}{nl}";
        }
    }
}