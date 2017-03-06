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

        public virtual Timestamp WriteNext(EventProto ev = null)
        {
            if (ev == null) ev = new EventProto(); 

            var writtenEventTimestamp = series.Write(ev);
            Thread.Sleep(Settings.MillisecondsSleep);
            return writtenEventTimestamp;
        }
    }
}