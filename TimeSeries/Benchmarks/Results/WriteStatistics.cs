using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Benchmarks.ReadWrite;

namespace Benchmarks.Results
{
    [DataContract]
    [Serializable]
    class WriteStatistics : WorkerStatistics
    {
        private readonly double totalWriteThroughput;

        public WriteStatistics(IReadOnlyList<BenchmarkEventWriter> writers) : base(writers)
        {
            totalWriteThroughput = writers.Select(x => x.AverageWriteThroughput).Sum();
        }

        public string CreateReport()
        {
            var nl = Environment.NewLine;

            return $"Writers count: {WorkersCount}{nl}{nl}" +
                   $"Average single write latency: {AverageOperationLatency.TotalMilliseconds} ms{nl}" +
                   $"95% of writes were faster than {Latency95ThPercentile.TotalMilliseconds} ms{nl}" +
                   $"98% of writes were faster than {Latency99ThPercentile.TotalMilliseconds} ms{nl}" +
                   $"Average write throughput: {totalWriteThroughput} ev/s{nl}{nl}" +

                   $"Total writes count: {TotalOperationsCount}{nl}" +
                   $"Average writes by single thread: {AverageOperationsPerThread}{nl}{nl}";
        }
    }
}