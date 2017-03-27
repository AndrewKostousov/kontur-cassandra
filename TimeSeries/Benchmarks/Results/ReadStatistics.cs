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
    class ReadStatistics : WorkerStatistics
    {
        [DataMember] public double AverageEndToEndLatency { get; set; }
        [DataMember] public double EndToEndLatency95ThPercentile { get; set; }
        [DataMember] public double EndToEndLatency99ThPercentile { get; set; }

        [DataMember] public List<List<double>> WriteToReadLatency { get; set; }

        private readonly int totalOperationsCount;

        public ReadStatistics(IReadOnlyList<BenchmarkEventReader> readers) : base(readers)
        {
            var writeToReadLatency = readers
                .Select(r => r.Timing
                    .Select(kv => kv.Value - kv.Key.ToTimeGuid().GetTimestamp())
                    .ToList())
                .ToList();

            WriteToReadLatency = writeToReadLatency
                .Select(x => x.Select(z => z.TotalMilliseconds).ToList())
                .ToList();

            if (readers.Count == 0) return;

            totalOperationsCount = readers.Select(x => x.OperationsCount()).Sum();

            var latencyBetweenWriteAndRead = WriteToReadLatency.SelectMany(x => x).ToList();

            AverageEndToEndLatency = latencyBetweenWriteAndRead.Average();
            EndToEndLatency95ThPercentile = latencyBetweenWriteAndRead.Percentile(95);
            EndToEndLatency99ThPercentile = latencyBetweenWriteAndRead.Percentile(99);
        }

        public string CreateReport()
        {
            var nl = Environment.NewLine;

            return $"Readers count: {WorkersCount}{nl}{nl}" +

                   $"Average single read latency: {AverageLatency} ms{nl}" +
                   $"95% of reads were faster than {Latency95ThPercentile} ms{nl}" +
                   $"99% of reads were faster than {Latency99ThPercentile} ms{nl}{nl}" +
                   
                   $"Average read throughput: {AverageThroughput} ev/s{nl}" +
                   $"Oprations count: {totalOperationsCount}{nl}" +
                   $"Total events read: {TotalThroughput}{nl}" +
                   $"Average reads length: {TotalThroughput/totalOperationsCount} events/read{nl}{nl}" +

                   $"Measurements between write and read: {AverageEndToEndLatency} ms{nl}" +
                   $"95% of events were read earlier than {EndToEndLatency95ThPercentile} ms since they had been written{nl}" +
                   $"99% of events were read earlier than {EndToEndLatency99ThPercentile} ms since they had been written{nl}{nl}";
        }
    }
}