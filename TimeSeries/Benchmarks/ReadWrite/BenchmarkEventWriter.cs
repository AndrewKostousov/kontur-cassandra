using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Model;
using CassandraTimeSeries.ReadWrite;
using Commons;
using Commons.TimeBasedUuid;

namespace Benchmarks.ReadWrite
{
    class BenchmarkEventWriter : EventWriter, IBenchmarkWorker
    {
        public List<TimeSpan> Latency { get; } = new List<TimeSpan>();
        public List<int> WritesLength { get; } = new List<int>();
        public double AverageWriteThroughput => WritesLength.Sum() / this.OperationalTime().TotalSeconds;

        public BenchmarkEventWriter(ITimeSeries series, WriterSettings settings) 
            : base(series, settings) { }

        public override Timestamp[] WriteNext(params EventProto[] events)
        {
            var sw = Stopwatch.StartNew();
            var writtenEventTimestamp = base.WriteNext(events);
            Latency.Add(sw.Elapsed);
            WritesLength.Add(writtenEventTimestamp.Length);
            return writtenEventTimestamp;
        }
    }
}
