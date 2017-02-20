using System;
using System.Collections.Generic;
using System.Linq;
using CassandraTimeSeries.Interfaces;
using Commons;
using Commons.TimeBasedUuid;
using EdiTimeline;
using EdiTimeline.CassandraHelpers;
using GroBuf;
using GroBuf.DataMembersExtracters;
using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Clusters;

namespace CassandraTimeSeries.Model
{
    public class AllBoxEventSeriesWrapper : ITimeSeries
    {
        private readonly AllBoxEventSeries series;
        private readonly BoxEventsReader reader;
        private readonly AllBoxEventSeriesWriter writer;

        public AllBoxEventSeriesWrapper(ICassandraCluster cluster)
        {
            var serializer = new Serializer(new AllFieldsExtractor(), new DefaultGroBufCustomSerializerCollection(), GroBufOptions.MergeOnRead);
            var ticksHolder = new AllBoxEventSeriesTicksHolder(serializer, cluster);

            series = new AllBoxEventSeries(new AllBoxEventSeriesSettings(), serializer, ticksHolder, cluster);

            reader= new BoxEventsReader(series);
            writer = new AllBoxEventSeriesWriter(series);
        }

        public Timestamp Write(EventProto ev)
        {
            return writer.Write(new ProtoBoxEvent(ev.UserId, ev.Payload));
        }

        public void WriteWithoutSync(Event ev)
        {
            if (ev.Payload == null) throw new ArgumentException("Event payload cannot be null");

            series.WriteEventsWithNoSynchronization(new BoxEvent(ev.Id.ToGuid(), ev.Timestamp, ev.Payload));
        }

        public List<Event> ReadRange(Timestamp startExclusive, Timestamp endInclusive, int count = 1000)
        {
            return ReadRange(reader.TryCreateEventSeriesRange(startExclusive, endInclusive), count);
        }

        public List<Event> ReadRange(TimeGuid startExclusive, TimeGuid endInclusive, int count = 1000)
        {
            var seriesPointer = startExclusive == null
                ? null
                : new AllBoxEventSeriesPointer(startExclusive.GetTimestamp(), startExclusive.ToGuid());

            var range = reader.TryCreateEventSeriesRange(seriesPointer, endInclusive?.GetTimestamp());

            return ReadRange(range, count);
        }

        private List<Event> ReadRange(AllBoxEventSeriesRange range, int count)
        {
            return reader.ReadEvents(range, count, x => x.Select(e => new Event(new TimeGuid(e.EventId), new EventProto(e.EventId, e.Payload))).ToArray()).ToList();
        }
    }
}
