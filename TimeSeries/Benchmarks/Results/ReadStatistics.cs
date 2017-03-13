using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Benchmarks.ReadWrite;
using Commons;
using Commons.TimeBasedUuid;

namespace Benchmarks.Results
{
    [DataContract]
    [Serializable]
    class ReadStatistics : WorkerStatistics
    {
        private readonly int reads95ThPercentile;
        private readonly int reads99ThPercentile;
        private readonly double averageReadsLength;
        private readonly int totalEventsRead;
        private readonly double totalReadThroughput;

        private readonly TimeSpan averageLatencyBetweenWriteAndRead;
        private readonly TimeSpan percentile95ThBetweenWriteAndRead;
        private readonly TimeSpan percentile99ThBetweenWriteAndRead;

        public ReadStatistics(IReadOnlyList<BenchmarkEventReader> readers) : base(readers)
        {
            if (readers.Count == 0) return;

            totalEventsRead = readers.Select(x => x.TotalEventsRead).Sum();
            averageReadsLength = readers.Select(x => x.ReadsLength.Average()).Average();
            reads95ThPercentile = readers.SelectMany(x => x.ReadsLength).Percentile(95);
            reads99ThPercentile = readers.SelectMany(x => x.ReadsLength).Percentile(99);
            totalReadThroughput = readers.Select(x => x.AverageReadThroughput).Sum();

            var latencyBetweenWriteAndRead = readers
                .SelectMany(x => x.Timing.Select(kv => kv.Value - kv.Key.ToTimeGuid().GetTimestamp()))
                .ToList();

            averageLatencyBetweenWriteAndRead = latencyBetweenWriteAndRead.Average();
            percentile95ThBetweenWriteAndRead = latencyBetweenWriteAndRead.Percentile(95);
            percentile99ThBetweenWriteAndRead = latencyBetweenWriteAndRead.Percentile(99);
        }

        public string CreateReport()
        {
            var nl = Environment.NewLine;

            return $"Readers count: {WorkersCount}{nl}{nl}" +
                   $"Average single read latency: {AverageOperationLatency.TotalMilliseconds} ms{nl}" +
                   $"95% of reads were faster than {Latency95ThPercentile.TotalMilliseconds} ms{nl}" +
                   $"99% of reads were faster than {Latency99ThPercentile.TotalMilliseconds} ms{nl}" +
                   $"Average read throughput: {totalReadThroughput} ev/s{nl}{nl}" +

                   $"Total reads count: {TotalOperationsCount}{nl}" +
                   $"Average reads by single thread: {AverageOperationsPerThread}{nl}{nl}" +

                   $"Total events read: {totalEventsRead}{nl}" +
                   $"Average reads length: {averageReadsLength} events/read{nl}" +
                   $"95% of reads were shorter than {reads95ThPercentile} events{nl}" +
                   $"99% of reads were shorter than {reads99ThPercentile} events{nl}{nl}" +

                   $"Latency between write and read: {averageLatencyBetweenWriteAndRead.TotalMilliseconds} ms{nl}" +
                   $"95% of events were read earlier than {percentile95ThBetweenWriteAndRead.TotalMilliseconds} ms since they had been written{nl}" +
                   $"99% of events were read earlier than {percentile99ThBetweenWriteAndRead.TotalMilliseconds} ms since they had been written{nl}{nl}";
        }
    }
}