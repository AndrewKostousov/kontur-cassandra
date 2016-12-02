using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CassandraTimeSeries.Model;

namespace Benchmarks.ReadWrite
{
    class BenchmarkEventWriter : EventWriter
    {
        public TimeSpan AverageLatency => TimeSpan.FromTicks((long)Latency.Average(x => x.Ticks));
        public List<TimeSpan> Latency { get; } = new List<TimeSpan>();
        public int TotalEventsWritten { get; private set; }


        public BenchmarkEventWriter(TimeSeries series, WriterSettings settings) 
            : base(series, settings) { }

        public override void WriteNext()
        {
            var sw = Stopwatch.StartNew();

            base.WriteNext();

            Latency.Add(sw.Elapsed);
            TotalEventsWritten++;
        }
    }
}
