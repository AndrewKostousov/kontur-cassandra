using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Benchmarks.ReadWrite;

namespace Benchmarks.Results
{
    [DataContract]
    class WriteStatistics : WorkerStatistics
    {
        public WriteStatistics(IReadOnlyCollection<BenchmarkEventWriter> writers) : base(writers) { }

        public string CreateReport()
        {
            var nl = Environment.NewLine;

            return $"Writers count: {WorkersCount}{nl}{nl}" +

                   $"Average single write latency: {AverageLatency} ms{nl}" +
                   $"95% of writes were faster than {Latency95ThPercentile} ms{nl}" +
                   $"98% of writes were faster than {Latency99ThPercentile} ms{nl}{nl}" +

                   $"Average single write throughput: {AverageThroughput} ev/s{nl}" +
                   $"Total events written: {TotalThroughput}{nl}";
        }
    }
}