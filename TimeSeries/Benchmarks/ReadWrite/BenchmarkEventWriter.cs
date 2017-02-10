using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public BenchmarkEventWriter(ITimeSeries series, WriterSettings settings) 
            : base(series, settings) { }

        public override Timestamp WriteNext(EventProto ev = null)
        {
            var sw = Stopwatch.StartNew();
            var writtenEventTimestamp = base.WriteNext(ev);
            Latency.Add(sw.Elapsed);
            return writtenEventTimestamp;
        }
    }
}
