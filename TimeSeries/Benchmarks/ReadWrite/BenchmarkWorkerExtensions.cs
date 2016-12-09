using System;
using Commons;

namespace Benchmarks.ReadWrite
{
    static class BenchmarkWorkerExtensions
    {
        public static TimeSpan AverageLatency(this IBenchmarkWorker worker) =>
            worker.Latency.Average();

        public static TimeSpan OperationalTime(this IBenchmarkWorker worker) => 
            worker.Latency.Sum();

        public static int TotalOperationsCount(this IBenchmarkWorker worker) => 
            worker.Latency.Count;

        public static double AverageThroughput(this IBenchmarkWorker worker) =>
            worker.TotalOperationsCount()/worker.OperationalTime().TotalSeconds;
    }
}