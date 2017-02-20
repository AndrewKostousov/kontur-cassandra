using System;
using System.Collections.Generic;
using System.Linq;
using Benchmarks.ReadWrite;
using Commons;
using Commons.TimeBasedUuid;

namespace Benchmarks.Results
{
    class ReadStatistics : WorkerStatistics
    {
        public int Reads95ThPercentile { get; }
        public int Reads99ThPercentile { get; }
        public double AverageReadsLength { get; }
        public int TotalEventsRead { get; }
        public double TotalReadThroughput { get; }
        
        public TimeSpan AverageLatencyBetweenWriteAndRead { get; }
        public TimeSpan Percentile95ThBetweenWriteAndRead { get; }
        public TimeSpan Percentile99ThBetweenWriteAndRead { get; }

        public ReadStatistics(List<BenchmarkEventReader> readers) : base(readers)
        {
            if (readers.Count == 0) return;

            TotalEventsRead = readers.Select(x => x.TotalEventsRead).Sum();
            AverageReadsLength = readers.Select(x => x.ReadsLength.Average()).Average();
            Reads95ThPercentile = readers.SelectMany(x => x.ReadsLength).Percentile(95);
            Reads99ThPercentile = readers.SelectMany(x => x.ReadsLength).Percentile(99);
            TotalReadThroughput = readers.Select(x => x.AverageReadThroughput).Sum();

            var latencyBetweenWriteAndRead = readers
                .SelectMany(x => x.Timing.Select(kv => kv.Value - kv.Key.ToTimeGuid().GetTimestamp()))
                .ToList();

            AverageLatencyBetweenWriteAndRead = latencyBetweenWriteAndRead.Average();
            Percentile95ThBetweenWriteAndRead = latencyBetweenWriteAndRead.Percentile(95);
            Percentile99ThBetweenWriteAndRead = latencyBetweenWriteAndRead.Percentile(99);
        }

        public string CreateReport()
        {
            return $"Readers count: {WorkersCount}\r\n\r\n" +
                   $"Average single read latency: {AverageOperationLatency.TotalMilliseconds} ms\r\n" +
                   $"95% of reads were faster than {Latency95ThPercentile.TotalMilliseconds} ms\r\n" +
                   $"99% of reads were faster than {Latency99ThPercentile.TotalMilliseconds} ms\r\n" +
                   $"Average read throughput: {TotalReadThroughput} ev/s\r\n\r\n" +

                   $"Total reads count: {TotalOperationsCount}\r\n" +
                   $"Average reads by single thread: {AverageOperationsPerThread}\r\n\r\n" +

                   $"Total events read: {TotalEventsRead}\r\n" +
                   $"Average reads length: {AverageReadsLength} events/read\r\n" +
                   $"95% of reads were shorter than {Reads95ThPercentile} events\r\n" +
                   $"99% of reads were shorter than {Reads99ThPercentile} events\r\n\r\n" +

                   $"Latency between write and read: {AverageLatencyBetweenWriteAndRead.TotalMilliseconds} ms\r\n" +
                   $"95% of events were read earlier than {Percentile95ThBetweenWriteAndRead.TotalMilliseconds} ms since they had been written\r\n" +
                   $"99% of events were read earlier than {Percentile99ThBetweenWriteAndRead.TotalMilliseconds} ms since they had been written\r\n\r\n";
        }
    }
}