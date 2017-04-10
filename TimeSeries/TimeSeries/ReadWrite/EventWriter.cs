using System;
using System.Linq;
using System.Threading;
using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Model;
using Commons;

namespace CassandraTimeSeries.ReadWrite
{
    public class EventWriter : IEventWriter
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

            Timestamp[] timestamp;

            try
            {
                timestamp = series.Write(events);
            }
            catch (OperationTimeoutException)
            {
                return new Timestamp[0];
            }

            Thread.Sleep(Settings.MillisecondsSleep);
            return timestamp;
        }

        private EventProto CreateEventProto()
        {
            return new EventProto(rng.Value.NextBytes(Settings.PayloadLength));
        }

        private readonly ThreadLocal<Random> rng = new ThreadLocal<Random>(() => new Random(Guid.NewGuid().GetHashCode()));
    }
}