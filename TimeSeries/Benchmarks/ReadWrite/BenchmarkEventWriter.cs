using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CassandraTimeSeries.Model;
using Commons;
using Metrics;
using Metrics.Core;

namespace Benchmarks.ReadWrite
{
    class BenchmarkEventWriter : EventWriter
    {
        public Timer LatencyTimer = Metric.Timer("Write Latency", Unit.Calls);
        public Counter EventsCounter = Metric.Counter("Events Written", Unit.Events);

        public TimeSpan AverageLatency => Latency.Average();
        public TimeSpan TotalTime => Latency.Sum();
        public List<TimeSpan> Latency { get; } = new List<TimeSpan>();
        public int TotalEventsWritten => Latency.Count;


        public BenchmarkEventWriter(TimeSeries series, WriterSettings settings) 
            : base(series, settings) { }

        public override void WriteNext()
        {
            var sw = Stopwatch.StartNew();

            using (LatencyTimer.NewContext())
                base.WriteNext();

            Latency.Add(sw.Elapsed);

            EventsCounter.Increment();
        }
    }
}
