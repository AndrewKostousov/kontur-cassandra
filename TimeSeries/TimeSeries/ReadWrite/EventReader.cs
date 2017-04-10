using System.Linq;
using System.Threading;
using CassandraTimeSeries.Model;
using System.Collections.Generic;
using CassandraTimeSeries.Interfaces;
using Commons;
using Commons.TimeBasedUuid;

namespace CassandraTimeSeries.ReadWrite
{
    public class EventReader : IEventReader
    {
        public ReaderSettings Settings { get; }

        private readonly ITimeSeries series;
        private Timestamp lastTimestamp;

        public EventReader(ITimeSeries series, ReaderSettings settings)
        {
            Settings = settings;
            this.series = series;
        }

        public virtual Event[] ReadFirst()
        {
            if (lastTimestamp != null) return ReadNext();

            Event[] events = null;

            while (events == null || events.Length == 0)
                 events = series.ReadRange((TimeGuid) null, null, Settings.EventsToRead);

            lastTimestamp = events.Max(x => x.Timestamp);

            return events;
        }

        public virtual Event[] ReadNext()
        {
            Event[] events;

            try
            {
                events = series.ReadRange(lastTimestamp, null, Settings.EventsToRead);
            }
            catch (OperationTimeoutException)
            {
                return new Event[0];
            }

            if (events.Length != 0)
                lastTimestamp = events.Max(x => x.Timestamp);

            Thread.Sleep(Settings.MillisecondsSleep);
            return events;
        }
    }
}