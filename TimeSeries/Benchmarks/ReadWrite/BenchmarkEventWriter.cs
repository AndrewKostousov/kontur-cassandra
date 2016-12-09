using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cassandra;
using CassandraTimeSeries.Model;
using Commons;
using Commons.TimeBasedUuid;
using Metrics;
using Metrics.Core;

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
            var eventWritten = base.WriteNext();
            Latency.Add(sw.Elapsed);
            return eventWritten;
        }
    }
}
