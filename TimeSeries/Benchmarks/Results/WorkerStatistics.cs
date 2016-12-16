using System;
using System.Collections.Generic;
using System.Linq;
using Benchmarks.ReadWrite;
using Commons;

namespace Benchmarks.Results
{
    abstract class WorkerStatistics
    {
        public TimeSpan AverageOperationLatency { get; }
        public TimeSpan AverageTotalLatency { get; }
        public TimeSpan Latency95ThPercentile { get; }
        public TimeSpan Latency99ThPercentile { get; }

        public int WorkersCount { get; }
        public int TotalOperationsCount { get; }
        public double AverageOperationsPerThread { get; }
        public double TotalThroughput { get; }

        public WorkerStatistics(IReadOnlyList<IBenchmarkWorker> workers)
        {
            if (workers.Count == 0) return;

            WorkersCount = workers.Count;
            AverageOperationLatency = workers.SelectMany(x => x.Latency).Average();
            AverageTotalLatency = workers.Select(x => x.Latency.Sum()).Average();
            Latency95ThPercentile = workers.SelectMany(x => x.Latency).Percentile(95);
            Latency99ThPercentile = workers.SelectMany(x => x.Latency).Percentile(99);

            TotalOperationsCount = workers.Sum(x => x.TotalOperationsCount());
            AverageOperationsPerThread = workers.Select(x => x.TotalOperationsCount()).Average();
            TotalThroughput = workers.Select(x => x.AverageThroughput()).Sum();
        }
    }
}