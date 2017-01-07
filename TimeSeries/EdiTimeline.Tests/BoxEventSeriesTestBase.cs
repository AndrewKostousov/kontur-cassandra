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
            var theEventId = eventId.HasValue ? GuidTestingHelpers.Guid(eventId.Value) : Guid.NewGuid();
            return new BoxEvent(theEventId, eventTimestamp, payload: rng.NextBytes(100));
        }

        [NotNull]
        protected static ProtoBoxEvent ProtoBoxEvent(byte? eventId = null)
        {
            var theEventId = eventId.HasValue ? GuidTestingHelpers.Guid(eventId.Value) : Guid.NewGuid();
            return new ProtoBoxEvent(theEventId, payload: rng.NextBytes(100));
        }

        protected List<BoxEvent> WriteToAllBoxEventSeries([NotNull] params ProtoBoxEvent[] boxEvents)
        {
            var queueItems = boxEvents.Select(x => new AllBoxEventSeriesWriterQueueItem(x, new Promise<Timestamp>())).ToList();
            allBoxEventSeries.WriteEventsInAnyOrder(queueItems);
            // ReSharper disable once AssignNullToNotNullAttribute
            return queueItems.Select(x => new BoxEvent(x.ProtoBoxEvent.EventId, x.EventTimestamp.Result, x.ProtoBoxEvent.Payload)).ToList();
        }

        protected ISerializer serializer;
        protected ICassandraCluster cassandraCluster;
        protected AllBoxEventSeriesTicksHolder allBoxEventSeriesTicksHolder;
        protected AllBoxEventSeries allBoxEventSeries;

        protected readonly TimeSpan tick = TimeSpan.FromTicks(1);
        protected readonly TimeSpan second = TimeSpan.FromSeconds(1);
        protected readonly TimeSpan minute = TimeSpan.FromMinutes(1);
        private static readonly Random rng = new Random(Guid.NewGuid().GetHashCode());

        public class AllBoxEventSeriesSettingsForTests : IAllBoxEventSeriesSettings
        {
            public AllBoxEventSeriesSettingsForTests(TimeSpan partitionDuration, int minBatchSizeForRead)
            {
                MinBatchSizeForRead = minBatchSizeForRead;
                PartitionDuration = partitionDuration;
                NotCommittedEventsTtl = TimeSpan.FromSeconds(10);
            }

            public int MinBatchSizeForRead { get; }
            public TimeSpan PartitionDuration { get; }
            public TimeSpan NotCommittedEventsTtl { get; }

            public override string ToString()
            {
                return $"MinBatchSizeForRead: {MinBatchSizeForRead}, PartitionDuration: {PartitionDuration}, NotCommittedEventsTtl: {NotCommittedEventsTtl}";
            }
        }
    }
}