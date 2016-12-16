using System;
using System.Threading;
using CassandraTimeSeries.Model;
using Commons;
using Commons.TimeBasedUuid;

namespace Benchmarks.ReadWrite
{
    class EventWriter
    {
        public WriterSettings Settings { get; }
        private TimeSeries series;

        public EventWriter(TimeSeries series, WriterSettings settings)
        {
            Settings = settings;
            this.series = series;
        }

        public virtual Event WriteNext()
        {
            var @event = series.Write(new EventProto());
            Thread.Sleep(Settings.MillisecondsSleep);
            return @event;
        }
    }
}