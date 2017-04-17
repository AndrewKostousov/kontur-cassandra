using System;
using System.Linq;
using Commons;

namespace Benchmarks.ReadWrite
{
    static class BenchmarkWorkerExtensions
    {
        public static int TotalThroughput(this IBenchmarkWorker worker) =>
            worker.Measurements.Select(x => x.Throughput).Sum();

        public static double AverageThroughput(this IBenchmarkWorker worker) =>
            worker.TotalThroughput() / worker.OperationalTime().TotalSeconds;

        public static int OperationThrouput(this IBenchmarkWorker worker) =>
            worker.TotalThroughput() / worker.OperationsCount();

        public static TimeSpan AverageLatency(this IBenchmarkWorker worker) =>
            worker.Measurements.Select(x => x.Latency).Average();

        public static TimeSpan OperationalTime(this IBenchmarkWorker worker) =>
            worker.Measurements.Select(x => x.Latency).Sum();

        public static int OperationsCount(this IBenchmarkWorker worker) => 
            worker.Measurements.Count;
    }
}