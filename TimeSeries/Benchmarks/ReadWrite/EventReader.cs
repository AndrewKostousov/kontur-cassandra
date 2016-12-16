using System;
using System.Linq;
using System.Threading;
using CassandraTimeSeries.Model;
using System.Collections.Generic;
using Commons;
using Commons.TimeBasedUuid;

namespace Benchmarks.ReadWrite
{
    class EventReader
    {
        public ReaderSettings Settings { get; }

        private TimeSeries series;
        private Event lastEvent;

        public EventReader(TimeSeries series, ReaderSettings settings)
        {
            Settings = settings;
            this.series = series;
        }

        public virtual void ReadFirst()
        {
            while (lastEvent == null)
            {
                var now = Timestamp.Now;
                lastEvent = series.ReadRange(now - Event.SliceDutation, now + Event.SliceDutation, 1).FirstOrDefault();
            }
        }

        public virtual List<Event> ReadNext()
        {
            var events = series.ReadRange(lastEvent.Id.ToTimeGuid(), 
                TimeGuid.MinForTimestamp(lastEvent.Timestamp + Event.SliceDutation), Settings.EventsToRead);

            if (events.Count != 0)
            {
                var maxId = events.Max(x => x.Id);
                lastEvent = events.First(x => x.Id == maxId);
            }

            Thread.Sleep(Settings.MillisecondsSleep);
            return events;
        }
    }
}