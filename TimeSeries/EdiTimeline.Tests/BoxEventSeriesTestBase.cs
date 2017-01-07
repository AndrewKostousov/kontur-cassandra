using System;
using System.Collections.Generic;
using System.Linq;
using Commons;
using Commons.Testing;
using GroBuf;
using JetBrains.Annotations;
using NUnit.Framework;
using SKBKontur.Cassandra.CassandraClient.Clusters;

namespace EdiTimeline.Tests
{
    [TestFixture]
    public abstract class BoxEventSeriesTestBase
    {
        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            serializer = EdiTimelineTestsEnvironment.Serializer;
            cassandraCluster = EdiTimelineTestsEnvironment.CassandraCluster;
            allBoxEventSeriesTicksHolder = EdiTimelineTestsEnvironment.AllBoxEventSeriesTicksHolder;
            allBoxEventSeries = EdiTimelineTestsEnvironment.AllBoxEventSeries;
        }

        [SetUp]
        public void BoxEventSeriesTestBase_SetUp()
        {
            EdiTimelineTestsEnvironment.ResetState();
        }

        [NotNull]
        protected static BoxEvent BoxEvent(byte? eventId = null)
        {
            return BoxEvent(Timestamp.Now, eventId);
        }

        [NotNull]
        protected static BoxEvent BoxEvent([NotNull] Timestamp eventTimestamp, byte? eventId = null)
        {
            return BoxEvent(null, eventTimestamp, eventId);
        }

        [NotNull]
        protected static BoxEvent BoxEvent(string boxId, byte? eventId = null)
        {
            return BoxEvent(boxId, Timestamp.Now, eventId);
        }

        [NotNull]
        protected static BoxEvent BoxEvent(string boxId, [NotNull] Timestamp eventTimestamp, byte? eventId = null)
        {
            var theEventId = eventId.HasValue ? GuidTestingHelpers.Guid(eventId.Value) : Guid.NewGuid();
            return BoxEvent(boxId, eventTimestamp, theEventId);
        }

        [NotNull]
        protected static BoxEvent BoxEvent(string boxId, [NotNull] Timestamp eventTimestamp, Guid eventId)
        {
            boxId = boxId ?? Guid.NewGuid().ToString();
            var partyId = Guid.NewGuid().ToString();
            var documentCirculationId = Guid.NewGuid();
            var entityId = Guid.NewGuid();
            return new BoxEvent(new BoxIdentifier(boxId, partyId), documentCirculationId, eventId, eventTimestamp, new Lazy<BoxEventContent>(() => new DummyEventContent(entityId)));
        }

        [NotNull]
        protected static ProtoBoxEvent ProtoBoxEvent(byte? eventId = null)
        {
            var boxId = Guid.NewGuid().ToString();
            return ProtoBoxEvent(boxId, eventId);
        }

        [NotNull]
        protected static ProtoBoxEvent ProtoBoxEvent(string boxId, byte? eventId = null)
        {
            var theEventId = eventId.HasValue ? GuidTestingHelpers.Guid(eventId.Value) : Guid.NewGuid();
            var partyId = Guid.NewGuid().ToString();
            var documentCirculationId = Guid.NewGuid();
            var entityId = Guid.NewGuid();
            return new ProtoBoxEvent(theEventId, new BoxIdentifier(boxId, partyId), documentCirculationId, new DummyEventContent(entityId));
        }

        protected List<BoxEvent> WriteToAllBoxEventSeries([NotNull] params ProtoBoxEvent[] boxEvents)
        {
            var queueItems = boxEvents.Select(x => new AllBoxEventSeriesWriterQueueItem(x, new Promise<Timestamp>())).ToList();
            allBoxEventSeries.WriteEventsInAnyOrder(queueItems);
            // ReSharper disable once AssignNullToNotNullAttribute
            return queueItems.Select(x => new BoxEvent(x.ProtoBoxEvent.BoxId, x.ProtoBoxEvent.DocumentCirculationId, x.ProtoBoxEvent.EventId, x.EventTimestamp.Result, new Lazy<BoxEventContent>(() => x.ProtoBoxEvent.EventContent))).ToList();
        }

        protected ISerializer serializer;
        protected ICassandraCluster cassandraCluster;
        protected AllBoxEventSeriesTicksHolder allBoxEventSeriesTicksHolder;
        protected AllBoxEventSeries allBoxEventSeries;

        protected readonly TimeSpan tick = TimeSpan.FromTicks(1);
        protected readonly TimeSpan second = TimeSpan.FromSeconds(1);
        protected readonly TimeSpan minute = TimeSpan.FromMinutes(1);

        public class AllBoxEventSeriesSettingsForTests : IAllBoxEventSeriesSettings
        {
            public AllBoxEventSeriesSettingsForTests(TimeSpan partitionDuration, int minBatchSizeForRead)
            {
                MinBatchSizeForRead = minBatchSizeForRead;
                PartitionDuration = partitionDuration;
                NotCommittedEventsTtl = TimeSpan.FromSeconds(10);
            }

            public int MinBatchSizeForRead { get; private set; }
            public TimeSpan PartitionDuration { get; private set; }
            public TimeSpan NotCommittedEventsTtl { get; private set; }

            public override string ToString()
            {
                return $"MinBatchSizeForRead: {MinBatchSizeForRead}, PartitionDuration: {PartitionDuration}, NotCommittedEventsTtl: {NotCommittedEventsTtl}";
            }
        }
    }
}