using System;
using System.Collections.Generic;
using System.Linq;
using CassandraTimeSeries.Interfaces;
using CassandraTimeSeries.Utils;
using Commons;
using Commons.TimeBasedUuid;
using EdiTimeline;
using GroBuf;
using GroBuf.DataMembersExtracters;
using SKBKontur.Cassandra.CassandraClient.Clusters;

namespace CassandraTimeSeries.Model
{
    public class AllBoxEventSeriesWrapper : ITimeSeries
    {
        public TimeLinePartitioner Partitioner { get; }

        private readonly AllBoxEventSeries series;
        private readonly BoxEventsReader reader;
        private readonly AllBoxEventSeriesWriter writer;
        private readonly AllBoxEventSeriesTicksHolder ticksHolder;
        private long lastGoodEventTicks;

        public AllBoxEventSeriesWrapper(ICassandraCluster cluster, TimeLinePartitioner partitioner)
        {
            var serializer = new Serializer(new AllFieldsExtractor(), new DefaultGroBufCustomSerializerCollection(), GroBufOptions.MergeOnRead);

            ticksHolder = new AllBoxEventSeriesTicksHolder(serializer, cluster);
            ticksHolder.SetEventSeriesExclusiveStartTicks(Timestamp.Now.AddDays(-1).Ticks);

            series = new AllBoxEventSeries(new AllBoxEventSeriesSettings(), serializer, ticksHolder, cluster);

            reader= new BoxEventsReader(series);
            writer = new AllBoxEventSeriesWriter(series);

            Partitioner = partitioner;
        }

        public Timestamp[] Write(params EventProto[] events)
        {
            return events.Select(Write).ToArray();
        }

        private Timestamp Write(EventProto ev)
        {
            var timestamp = writer.Write(new ProtoBoxEvent(ev.UserId, ev.Payload));

            if (lastGoodEventTicks < timestamp.Ticks)
                lastGoodEventTicks = timestamp.Ticks;

            return timestamp;
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
                    .Select(e => new Event(TimeGuid.MinForTimestamp(e.EventTimestamp), new EventProto(e.EventId, e.Payload)))
                    .ToArray())
                .ToArray();
        }
    }
}
