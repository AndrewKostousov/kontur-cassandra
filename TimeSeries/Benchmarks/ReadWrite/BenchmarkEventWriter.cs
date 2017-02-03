using System;
using System.Collections.Generic;
using System.Diagnostics;
using CassandraTimeSeries.Model;
using CassandraTimeSeries.ReadWrite;
using Commons.TimeBasedUuid;

namespace Benchmarks.ReadWrite
{
    class BenchmarkEventWriter : EventWriter, IBenchmarkWorker
    {
        public List<TimeSpan> Latency { get; } = new List<TimeSpan>();

        public BenchmarkEventWriter(TimeSeries series, WriterSettings settings) 
            : base(series, settings) { }

        public override Event WriteNext()
        {
            var sw = Stopwatch.StartNew();
            var writtenEvent = base.WriteNext();
            Latency.Add(sw.Elapsed);
            return writtenEvent;
        }
    }
}
