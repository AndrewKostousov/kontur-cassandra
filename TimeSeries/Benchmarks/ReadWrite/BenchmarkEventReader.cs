using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Benchmarks.Results;
using Cassandra;
using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Model;
using CassandraTimeSeries.ReadWrite;
using Commons;

namespace Benchmarks.ReadWrite
{
    class BenchmarkEventReader : EventReader, IBenchmarkWorker
    {
        public List<Measurement> Measurements { get; } = new List<Measurement>();

        public Dictionary<TimeUuid, Timestamp> Timing { get; } = new Dictionary<TimeUuid, Timestamp>();

        public BenchmarkEventReader(ITimeSeries series, ReaderSettings settings) 
            : base(series, settings) { }

        public override Event[] ReadNext()
        {
            var measurement = Measurement.Start();

            var events = base.ReadNext();

            Measurements.Add(measurement.Stop(events.Length));

            var currentTime = Timestamp.Now;

            foreach (var ev in events)
                Timing.Add(ev.Id, currentTime);

            return events;
        }
    }
}
