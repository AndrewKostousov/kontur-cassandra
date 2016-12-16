using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cassandra;
using CassandraTimeSeries.Model;
using CassandraTimeSeries.ReadWrite;
using Commons;
using Metrics;

namespace Benchmarks.ReadWrite
{
    class BenchmarkEventReader : EventReader, IBenchmarkWorker
    {
        public List<TimeSpan> Latency { get; } = new List<TimeSpan>();
        public List<int> ReadsLength { get; } = new List<int>();
        public double AverageReadThroughput => ReadsLength.Sum()/this.OperationalTime().TotalSeconds;
        public Dictionary<TimeUuid, Timestamp> Timing = new Dictionary<TimeUuid, Timestamp>();

        public int TotalEventsRead => ReadsLength.Sum();

        public BenchmarkEventReader(TimeSeries series, ReaderSettings settings) 
            : base(series, settings) { }

        public override List<Event> ReadNext()
        {
            var sw = Stopwatch.StartNew();
            var events = base.ReadNext();
            var currentTime = Timestamp.Now;

            Latency.Add(sw.Elapsed);
            ReadsLength.Add(events.Count);
            
            foreach (var ev in events)
                Timing.Add(ev.Id, currentTime);

            return events;
        }
    }
}
