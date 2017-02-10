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

        public AllBoxEventSeriesWrapper()
        {
            var serializer = new Serializer(new AllFieldsExtractor(), new DefaultGroBufCustomSerializerCollection(), GroBufOptions.MergeOnRead);
            var cluster = SetUpCassandraCluster();
            var ticksHolder = new AllBoxEventSeriesTicksHolder(serializer, cluster);

            series = new AllBoxEventSeries(new AllBoxEventSeriesSettings(), serializer, ticksHolder, cluster);

            reader= new BoxEventsReader(series);
            writer = new AllBoxEventSeriesWriter(series);
        }

        private static CassandraCluster SetUpCassandraCluster()
        {
            var localEndPoint = CassandraClusterSettings.ParseEndPoint("127.0.0.1:9160");
            return new CassandraCluster(new CassandraClusterSettings
            {
                ClusterName = "TestCluster",
                Endpoints = new[] { localEndPoint },
                EndpointForFierceCommands = localEndPoint,
                ReadConsistencyLevel = ConsistencyLevel.QUORUM,
                WriteConsistencyLevel = ConsistencyLevel.QUORUM,
                Attempts = 5,
                Timeout = 6000,
                FierceTimeout = 6000,
                ConnectionIdleTimeout = TimeSpan.FromMinutes(10),
                EnableMetrics = false,
            });
        }

        public Timestamp Write(EventProto ev)
        {
            return writer.Write(new ProtoBoxEvent(ev.UserId, ev.Payload));
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
            return reader.ReadEvents(range, count, x => x.Select(e => new Event(new TimeGuid(e.EventId), new EventProto(e.Payload))).ToArray()).ToList();
        }
    }
}
