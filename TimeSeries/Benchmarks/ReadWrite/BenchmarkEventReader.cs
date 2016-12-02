using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CassandraTimeSeries.Model;

namespace Benchmarks.ReadWrite
{
    class BenchmarkEventReader : EventReader
    {
        public TimeSpan AverageLatency => TimeSpan.FromTicks((long)Latency.Average(x => x.Ticks));
        public List<TimeSpan> Latency { get; } = new List<TimeSpan>();
        public int TotalReadsCount { get; private set; }
        public int TotalEventsReaded { get; private set; }

        public BenchmarkEventReader(TimeSeries series, ReaderSettings settings) 
            : base(series, settings) { }

        public override List<Event> ReadNext()
        {
            var sw = Stopwatch.StartNew();

            var events = base.ReadNext();

            Latency.Add(sw.Elapsed);
            TotalReadsCount++;
            TotalEventsReaded += events.Count;

            return events;
        }
    }
}
