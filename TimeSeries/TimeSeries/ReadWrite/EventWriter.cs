using System.Threading;
using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Model;
using Commons.TimeBasedUuid;

namespace CassandraTimeSeries.ReadWrite
{
    public class EventWriter
    {
        public WriterSettings Settings { get; }
        private ITimeSeries series;

        public EventWriter(ITimeSeries series, WriterSettings settings)
        {
            Settings = settings;
            this.series = series;
        }

        public virtual Event WriteNext()
        {
            var writtenEvent = series.Write(new EventProto());
            Thread.Sleep(Settings.MillisecondsSleep);
            return writtenEvent;
        }
    }
}