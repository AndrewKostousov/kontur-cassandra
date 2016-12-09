using System;
using System.Threading;
using CassandraTimeSeries.Model;
using Commons;

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
            var @event = new Event(Timestamp.Now);
            series.Write(@event);
            Thread.Sleep(Settings.MillisecondsSleep);
            return @event;
        }
    }
}