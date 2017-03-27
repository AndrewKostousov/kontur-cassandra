using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Benchmarks.Results;
using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Model;
using CassandraTimeSeries.ReadWrite;
using Commons;
using Commons.TimeBasedUuid;

namespace Benchmarks.ReadWrite
{
    class BenchmarkEventWriter : EventWriter, IBenchmarkWorker
    {
        public List<Measurement> Measurements { get; } = new List<Measurement>();

        public BenchmarkEventWriter(ITimeSeries series, WriterSettings settings) 
            : base(series, settings) { }

        public override Timestamp[] WriteNext(params EventProto[] events)
        {
            var measurement = Measurement.Start();

            var writtenEventTimestamp = base.WriteNext(events);

            Measurements.Add(measurement.Stop(writtenEventTimestamp.Length));

            return writtenEventTimestamp;
        }
    }
}
