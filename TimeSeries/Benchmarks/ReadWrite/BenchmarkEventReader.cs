using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cassandra;
using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Model;
using CassandraTimeSeries.ReadWrite;
using Commons;

namespace Benchmarks.ReadWrite
{
    class BenchmarkEventReader : EventReader, IBenchmarkWorker
    {
        public List<TimeSpan> Latency { get; } = new List<TimeSpan>();
        public List<int> ReadsLength { get; } = new List<int>();
        public double AverageReadThroughput => ReadsLength.Sum()/this.OperationalTime().TotalSeconds;
        public Dictionary<TimeUuid, Timestamp> Timing { get; } = new Dictionary<TimeUuid, Timestamp>();

        public int TotalEventsRead => ReadsLength.Sum();

        public BenchmarkEventReader(ITimeSeries series, ReaderSettings settings) 
            : base(series, settings) { }

        public override List<Event> ReadNext()
        {
            var sw = Stopwatch.StartNew();
            var events = base.ReadNext();

            Latency.Add(sw.Elapsed);
            ReadsLength.Add(events.Count);

            var currentTime = Timestamp.Now;

            foreach (var ev in events)
                Timing.Add(ev.Id, currentTime);

            return events;
        }
    }
}
