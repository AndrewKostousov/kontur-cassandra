﻿using System.Collections.Generic;
using Benchmarks.Results;
using CassandraTimeSeries.Model;
using CassandraTimeSeries.ReadWrite;
using Commons;

namespace Benchmarks.ReadWrite
{
    class BenchmarkEventWriter : IEventWriter, IBenchmarkWorker
    {
        public List<Measurement> Measurements { get; } = new List<Measurement>();

        private readonly IEventWriter writer;

        public BenchmarkEventWriter(IEventWriter writer)
        {
            this.writer = writer;
        }

        public Timestamp[] WriteNext(params EventProto[] events)
        {
            var measurement = Measurement.StartNew();

            var writtenEventTimestamp = writer.WriteNext(events);

            Measurements.Add(measurement.Stop(writtenEventTimestamp.Length));

            return writtenEventTimestamp;
        }
    }
}
