using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CassandraTimeSeries.Model;
using Commons;
using Metrics;

namespace Benchmarks.ReadWrite
{
    interface IBenchmarkWorker
    {
        List<TimeSpan> Latency { get; }
    }

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

    class BenchmarkEventReader : EventReader, IBenchmarkWorker
    {
        public List<TimeSpan> Latency { get; } = new List<TimeSpan>();
        public List<int> ReadsLength { get; } = new List<int>();
        public double AverageReadThroughput => ReadsLength.Sum()/this.OperationalTime().TotalSeconds;

        public int TotalEventsRead => ReadsLength.Sum();

        public BenchmarkEventReader(TimeSeries series, ReaderSettings settings) 
            : base(series, settings) { }

        public override List<Event> ReadNext()
        {
            var sw = Stopwatch.StartNew();
            var events = base.ReadNext();
            Latency.Add(sw.Elapsed);
            ReadsLength.Add(events.Count - 1);
            return events;
        }
    }
}
