using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Utils;
using Commons;
using Commons.TimeBasedUuid;
using EdiTimeline;

namespace CassandraTimeSeries.Model
{
    public class EdiTimeSeriesWrapper : ITimeSeries
    {
        private class EdiTimeSeriesSettings : IAllBoxEventSeriesSettings
        {
            public int MinBatchSizeForRead => 100;
            public TimeSpan PartitionDuration { get; }
            public TimeSpan NotCommittedEventsTtl => TimeSpan.FromSeconds(60);

            public EdiTimeSeriesSettings(TimeSpan partitionDuration)
            {
                PartitionDuration = partitionDuration;
            }
        }

        public TimeLinePartitioner Partitioner { get; }

        private readonly AllBoxEventSeries series;
        private readonly BoxEventsReader reader;
        private readonly AllBoxEventSeriesTicksHolder ticksHolder;
        private long lastGoodEventTicks;
        private readonly uint operationalTimeoutMilliseconds;

        public EdiTimeSeriesWrapper(EdiTimeSeriesDatabaseController c, TimeLinePartitioner partitioner, uint operationalTimeoutMilliseconds = 10000)
        {
            Partitioner = partitioner;
            this.operationalTimeoutMilliseconds = operationalTimeoutMilliseconds;

            ticksHolder = c.AllBoxEventSeriesTicksHolder;

            series = new AllBoxEventSeries(new EdiTimeSeriesSettings(Partitioner.PartitionDuration), c.Serializer, ticksHolder, c.CassandraCluster);
            reader = new BoxEventsReader(series);
        }

        public Timestamp[] Write(params EventProto[] events)
        {
            var eventsToWrite = events
                .Select(e => new AllBoxEventSeriesWriterQueueItem(new ProtoBoxEvent(e.UserId, e.Payload), new Promise<Timestamp>()))
                .ToList();

            var sw = Stopwatch.StartNew();

            var timestamps = new List<Timestamp>();

            while (eventsToWrite.Count > 0 && sw.ElapsedMilliseconds < operationalTimeoutMilliseconds)
            {
                series.WriteEventsInAnyOrder(eventsToWrite);

                timestamps.AddRange(eventsToWrite.Select(e => e.EventTimestamp.Result).Where(r => r != null));

                eventsToWrite = eventsToWrite
                    .Where(e => e.EventTimestamp.Result == null)
                    .Select(e => new AllBoxEventSeriesWriterQueueItem(e.ProtoBoxEvent, new Promise<Timestamp>()))
                    .ToList();
            }

            if (eventsToWrite.Count > 0) throw new OperationTimeoutException(operationalTimeoutMilliseconds);

            return timestamps.ToArray();
        }

        public void WriteWithoutSync(Event ev)
        {
            if (ev.Proto.Payload == null) throw new ArgumentException("Event payload cannot be null");

            series.WriteEventsWithNoSynchronization(new BoxEvent(ev.Proto.UserId, ev.Timestamp, ev.Proto.Payload));

            if (lastGoodEventTicks < ev.Timestamp.Ticks)
                ticksHolder.SetLastGoodEventTicks(lastGoodEventTicks = ev.Timestamp.Ticks);
        }

        public Event[] ReadRange(Timestamp startExclusive, Timestamp endInclusive, int count = 1000)
        {
            return ReadRange(reader.TryCreateEventSeriesRange(startExclusive, endInclusive), count);
        }

        public Event[] ReadRange(TimeGuid startExclusive, TimeGuid endInclusive, int count = 1000)
        {
            var seriesPointer = startExclusive == null
                ? null
                : new AllBoxEventSeriesPointer(startExclusive.GetTimestamp(), GuidHelpers.MaxGuid);

            var range = reader.TryCreateEventSeriesRange(seriesPointer, endInclusive?.GetTimestamp());

            return ReadRange(range, count);
        }

        private Event[] ReadRange(AllBoxEventSeriesRange range, int count)
        {
            return reader
                .ReadEvents(range, count, x => x
                    .Select(e => new Event(TimeGuid.MinForTimestamp(e.EventTimestamp),  new EventProto(e.EventId, e.Payload))).ToArray())
                .ToArray();
        }
    }
}
