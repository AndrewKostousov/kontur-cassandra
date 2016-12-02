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
    class BenchmarkEventReader : EventReader
    {
        public TimeSpan AverageLatency => Latency.Average();
        public TimeSpan TotalTime => Latency.Sum();
        public List<TimeSpan> Latency { get; } = new List<TimeSpan>();
        public List<int> ReadsLength { get; } = new List<int>();
        public int TotalReadsCount => Latency.Count;
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
