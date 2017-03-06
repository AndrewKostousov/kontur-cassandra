using System.Linq;
using System.Threading;
using CassandraTimeSeries.Model;
using System.Collections.Generic;
using CassandraTimeSeries.Interfaces;
using Commons.TimeBasedUuid;

namespace CassandraTimeSeries.ReadWrite
{
    public class EventReader
    {
        public ReaderSettings Settings { get; }

        private ITimeSeries series;
        private Event lastEvent;

        public EventReader(ITimeSeries series, ReaderSettings settings)
        {
            Settings = settings;
            this.series = series;
        }

        public virtual Event ReadFirst()
        {
            while (lastEvent == null)
                lastEvent = series.ReadRange((TimeGuid) null, null, 1).FirstOrDefault();

            return lastEvent;
        }

        public virtual List<Event> ReadNext()
        {
            var events = series.ReadRange(lastEvent.Timestamp, null, Settings.EventsToRead);

            if (events.Count != 0)
            {
                var maxTimestamp = events.Max(x => x.Timestamp);
                lastEvent = events.First(x => x.Timestamp == maxTimestamp);
            }

            Thread.Sleep(Settings.MillisecondsSleep);
            return events;
        }
    }
}