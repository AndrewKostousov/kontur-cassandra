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
            var events = series.ReadRange(lastEvent.Id.ToTimeGuid(), 
                TimeGuid.MinForTimestamp(lastEvent.Timestamp + Event.PartitionDutation), Settings.EventsToRead);

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