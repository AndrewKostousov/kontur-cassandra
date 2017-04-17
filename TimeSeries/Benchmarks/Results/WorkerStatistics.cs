using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Benchmarks.ReadWrite;
using Commons;

namespace Benchmarks.Results
{
    [DataContract]
    abstract class WorkerStatistics
    {
        [DataMember] public TimeSpan AverageLatency { get; set; }
        [DataMember] public TimeSpan Latency95ThPercentile { get; set; }
        [DataMember] public TimeSpan Latency99ThPercentile { get; set; }

        [DataMember] public int WorkersCount { get; set; }
        [DataMember] public int TotalThroughput { get; set; }
        [DataMember] public double AverageThroughput { get; set; }

        [DataMember] public List<List<Measurement>> Measurements { get; set; }
        [DataMember] public List<int> Throughput { get; set; }

        protected WorkerStatistics(IReadOnlyCollection<IBenchmarkWorker> workers)
        {
            Measurements = workers.Select(x => x.Measurements.ToList()).ToList();

            if (workers.Count == 0) return;

            WorkersCount = workers.Count;

            var latency = Measurements.SelectMany(x => x.Select(z => z.Latency)).ToArray();

            AverageLatency = latency.Average();
            Latency95ThPercentile = latency.Percentile(95);
            Latency99ThPercentile = latency.Percentile(99);

            TotalThroughput = workers.Sum(x => x.TotalThroughput());
            AverageThroughput = workers.Sum(x => x.AverageThroughput());

            Throughput = workers
                .SelectMany(x => x.Measurements)
                .Select(x => new {Time = x.Stop.Ticks / TimeSpan.TicksPerMillisecond % 100, x.Throughput})
                .GroupBy(x => x.Time)
                .OrderBy(x => x.Key)
                .Select(x => x.Sum(z => z.Throughput))
                .ToList();
        }
    }
}