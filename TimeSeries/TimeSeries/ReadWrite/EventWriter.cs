using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Model;
using Commons;
using Commons.TimeBasedUuid;

namespace CassandraTimeSeries.ReadWrite
{
    public class EventWriter
    {
        public WriterSettings Settings { get; }
        private readonly ITimeSeries series;

        public EventWriter(ITimeSeries series, WriterSettings settings)
        {
            Settings = settings;
            this.series = series;
        }

        public virtual Timestamp[] WriteNext(params EventProto[] events)
        {
            if (events.Length == 0)
                events = Enumerable.Range(0, Settings.BulkSize).Select(x => CreateEventProto()).ToArray();

            var timestamp = series.Write(events);
            Thread.Sleep(Settings.MillisecondsSleep);
            return timestamp;
        }

        private EventProto CreateEventProto()
        {
            return new EventProto(new byte[Settings.PayloadLength]);
        }
    }
}