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
    class BenchmarkEventReader : IEventReader, IBenchmarkWorker
    {
        public List<Measurement> Measurements { get; } = new List<Measurement>();

        public Dictionary<TimeUuid, Timestamp> Timing { get; } = new Dictionary<TimeUuid, Timestamp>();

        private readonly IEventReader reader;

        public BenchmarkEventReader(IEventReader reader)
        {
            this.reader = reader;
        }

        public Event[] ReadFirst()
        {
            return reader.ReadFirst();
        }

        public Event[] ReadNext()
        {
            var measurement = Measurement.Start();

            var events = reader.ReadNext();

            Measurements.Add(measurement.Stop(events.Length));

            var currentTime = Timestamp.Now;

            foreach (var ev in events)
                if (!Timing.ContainsKey(ev.Id))
                    Timing.Add(ev.Id, currentTime);

            return events;
        }
    }
}
