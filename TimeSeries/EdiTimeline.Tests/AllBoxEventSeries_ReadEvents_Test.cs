using System;
using System.Linq;
using Commons;
using Commons.Testing;
using FluentAssertions;
using NUnit.Framework;
using SKBKontur.Cassandra.CassandraClient.Abstractions;

namespace EdiTimeline.Tests
{
    public class AllBoxEventSeries_ReadEvents_Test : BoxEventSeriesTestBase
    {
        [SetUp]
        public void SetUp()
        {
            partition = allBoxEventSeries.PartitionDuration;

            var now = Timestamp.Now;
            timeSeriesStart = now.AddDays(-1);
            allBoxEventSeriesTicksHolder.SetEventSeriesExclusiveStartTicks(timeSeriesStart.Ticks);

            firstPartitionStart = new Timestamp(now.Ticks - now.Ticks % partition.Ticks);
            firstPartitionEnd = firstPartitionStart + partition - tick;
            secondPartitionStart = firstPartitionEnd + tick;
            secondPartitionEnd = secondPartitionStart + partition - tick;
            emptyPartitionStart = secondPartitionEnd + tick;
            emptyPartitionEnd = emptyPartitionStart + partition - tick;
            lastPartitionStart = emptyPartitionEnd + tick;
            lastPartitionEnd = lastPartitionStart + partition - tick;

            e11 = BoxEvent(firstPartitionStart, 0x11);
            e12 = BoxEvent(firstPartitionStart, 0x12);
            e13 = BoxEvent(firstPartitionStart + second, 0x13);
            e14 = BoxEvent(firstPartitionStart + minute, 0x14);
            e15 = BoxEvent(firstPartitionEnd - minute, 0x15);
            e16 = BoxEvent(firstPartitionEnd - second, 0x16);
            e17 = BoxEvent(firstPartitionEnd, 0x17);
            e18 = BoxEvent(firstPartitionEnd, 0x18);
            e21 = BoxEvent(secondPartitionStart + second, 0x21);
            e22 = BoxEvent(secondPartitionStart + minute, 0x22);
            e23 = BoxEvent(secondPartitionEnd - minute, 0x23);
            e24 = BoxEvent(secondPartitionEnd - second, 0x24);
            ef1 = BoxEvent(lastPartitionStart, 0xf1);
            ef2 = BoxEvent(lastPartitionStart + second, 0xf2);
            ef3 = BoxEvent(lastPartitionEnd - second, 0xf3);
            ef4 = BoxEvent(lastPartitionEnd, 0xf4);
            allBoxEventSeries.WriteEventsWithNoSynchronization(e11, e12, e13, e14, e15, e16, e17, e18, e21, e22, e23, e24, ef1, ef2, ef3, ef4);

            allBoxEventSeriesTicksHolder.SetLastGoodEventTicks(ef4.EventTimestamp.Ticks);
        }

        [Test]
        public void ReadEvents_NullRange()
        {
            allBoxEventSeries.ReadEvents(null, int.MaxValue, x => x).Should().BeEmpty();
        }

        [Test]
        public void ReadEvents_NonPositiveTake()
        {
            var range = new AllBoxEventSeriesRange(timeSeriesStart, GuidHelpers.MaxGuid, lastPartitionEnd, partition);
            allBoxEventSeries.ReadEvents(range, 0, x => x).Should().BeEmpty();
            allBoxEventSeries.ReadEvents(range, -1, x => x).Should().BeEmpty();
        }

        [Test]
        public void ReadEvents_EmptyRange()
        {
            var range = new AllBoxEventSeriesRange(e14.EventTimestamp, GuidHelpers.MaxGuid, e14.EventTimestamp, partition);
            allBoxEventSeries.ReadEvents(range, int.MaxValue, x => x).Should().BeEmpty();
        }

        [Test]
        public void ReadEvents_NoEventsInRange()
        {
            var range = new AllBoxEventSeriesRange(e14.EventTimestamp, e14.EventId, e15.EventTimestamp - tick, partition);
            allBoxEventSeries.ReadEvents(range, int.MaxValue, x => x).Should().BeEmpty();
        }

        [Test]
        public void ReadEvents_SameTimestamp()
        {
            allBoxEventSeries.ReadEvents(new AllBoxEventSeriesRange(e11.EventTimestamp, GuidHelpers.MinGuid, firstPartitionEnd, partition), int.MaxValue, x => x).ShouldBeEquivalentWithOrderTo(e11, e12, e13, e14, e15, e16, e17, e18);
            allBoxEventSeries.ReadEvents(new AllBoxEventSeriesRange(e11.EventTimestamp, e11.EventId, firstPartitionEnd, partition), int.MaxValue, x => x).ShouldBeEquivalentWithOrderTo(e12, e13, e14, e15, e16, e17, e18);
            allBoxEventSeries.ReadEvents(new AllBoxEventSeriesRange(e11.EventTimestamp, GuidHelpers.MaxGuid, firstPartitionEnd, partition), int.MaxValue, x => x).ShouldBeEquivalentWithOrderTo(e13, e14, e15, e16, e17, e18);
            allBoxEventSeries.ReadEvents(new AllBoxEventSeriesRange(e17.EventTimestamp, GuidHelpers.MinGuid, firstPartitionEnd, partition), int.MaxValue, x => x).ShouldBeEquivalentWithOrderTo(e17, e18);
            allBoxEventSeries.ReadEvents(new AllBoxEventSeriesRange(e17.EventTimestamp, e17.EventId, firstPartitionEnd, partition), int.MaxValue, x => x).ShouldBeEquivalentWithOrderTo(e18);
            allBoxEventSeries.ReadEvents(new AllBoxEventSeriesRange(e17.EventTimestamp, GuidHelpers.MaxGuid, firstPartitionEnd, partition), int.MaxValue, x => x).Should().BeEmpty();
        }

        [Test]
        public void ReadEvents_AllEvents()
        {
            var range = new AllBoxEventSeriesRange(timeSeriesStart, GuidHelpers.MaxGuid, lastPartitionEnd + partition + tick, partition);
            allBoxEventSeries.ReadEvents(range, int.MaxValue, x => x).ShouldBeEquivalentWithOrderTo(e11, e12, e13, e14, e15, e16, e17, e18, e21, e22, e23, e24, ef1, ef2, ef3, ef4);
        }

        [Test]
        public void ReadEvents_AllEvents_WithPaging()
        {
            var range = new AllBoxEventSeriesRange(timeSeriesStart, GuidHelpers.MaxGuid, lastPartitionEnd, partition);
            allBoxEventSeries.ReadEvents(range, 3, x => x).ShouldBeEquivalentWithOrderTo(e11, e12, e13);
            range = new AllBoxEventSeriesRange(e13.EventTimestamp, e13.EventId, lastPartitionEnd, partition);
            allBoxEventSeries.ReadEvents(range, 3, x => x).ShouldBeEquivalentWithOrderTo(e14, e15, e16);
            range = new AllBoxEventSeriesRange(e16.EventTimestamp, e16.EventId, lastPartitionEnd, partition);
            allBoxEventSeries.ReadEvents(range, 3, x => x).ShouldBeEquivalentWithOrderTo(e17, e18, e21);
            range = new AllBoxEventSeriesRange(e21.EventTimestamp, e21.EventId, lastPartitionEnd, partition);
            allBoxEventSeries.ReadEvents(range, 3, x => x).ShouldBeEquivalentWithOrderTo(e22, e23, e24);
            range = new AllBoxEventSeriesRange(e24.EventTimestamp, e24.EventId, lastPartitionEnd, partition);
            allBoxEventSeries.ReadEvents(range, 3, x => x).ShouldBeEquivalentWithOrderTo(ef1, ef2, ef3);
            range = new AllBoxEventSeriesRange(ef3.EventTimestamp, ef3.EventId, lastPartitionEnd, partition);
            allBoxEventSeries.ReadEvents(range, 3, x => x).ShouldBeEquivalentWithOrderTo(ef4);
        }

        [Test]
        public void ReadEvents_Filter()
        {
            var range = new AllBoxEventSeriesRange(timeSeriesStart, GuidHelpers.MaxGuid, lastPartitionEnd, partition);
            allBoxEventSeries.ReadEvents(range, int.MaxValue, x => x.Where(e => GuidTestingHelpers.GetLastByte(e.EventId) % 2 == 0).ToArray()).ShouldBeEquivalentWithOrderTo(e12, e14, e16, e18, e22, e24, ef2, ef4);
        }

        [Test]
        public void ReadEvents_Filter_WithPaging()
        {
            Func<BoxEvent[], BoxEvent[]> filter = x => x.Where(e => GuidTestingHelpers.GetLastByte(e.EventId) % 2 == 1).ToArray();
            var range = new AllBoxEventSeriesRange(timeSeriesStart, GuidHelpers.MaxGuid, lastPartitionEnd, partition);
            allBoxEventSeries.ReadEvents(range, 3, filter).ShouldBeEquivalentWithOrderTo(e11, e13, e15);
            range = new AllBoxEventSeriesRange(e15.EventTimestamp, e15.EventId, lastPartitionEnd, partition);
            allBoxEventSeries.ReadEvents(range, 3, filter).ShouldBeEquivalentWithOrderTo(e17, e21, e23);
            range = new AllBoxEventSeriesRange(e23.EventTimestamp, e23.EventId, lastPartitionEnd, partition);
            allBoxEventSeries.ReadEvents(range, 3, filter).ShouldBeEquivalentWithOrderTo(ef1, ef3);
        }

        [Test]
        public void ReadEvents_Filter_SmallBatch()
        {
            var settings = new AllBoxEventSeriesSettingsForTests(partition, minBatchSizeForRead: 1);
            allBoxEventSeries = new AllBoxEventSeries(settings, serializer, allBoxEventSeriesTicksHolder, cassandraCluster);
            var expectedBoxEvents = new[] {e11, e15, e18};
            Func<BoxEvent[], BoxEvent[]> filter = x => x.Where(e => expectedBoxEvents.Any(y => y.EventId == e.EventId)).ToArray();
            var range = new AllBoxEventSeriesRange(firstPartitionStart.AddTicks(-1), GuidHelpers.MaxGuid, lastPartitionEnd, partition);
            allBoxEventSeries.ReadEvents(range, 3, filter).ShouldBeEquivalentWithOrderTo(expectedBoxEvents);
        }

        [Test]
        public void ReadEvents_PartitionSwitch_OverEmptyPartition()
        {
            var range = new AllBoxEventSeriesRange(firstPartitionEnd - tick, GuidHelpers.MaxGuid, lastPartitionStart, partition);
            allBoxEventSeries.ReadEvents(range, int.MaxValue, x => x).ShouldBeEquivalentWithOrderTo(e17, e18, e21, e22, e23, e24, ef1);
        }

        [Test]
        public void ReadEvents_ExclusiveStart_OnLowerPartitionBoundary_WithEventAtCursor()
        {
            var range = new AllBoxEventSeriesRange(firstPartitionStart, e11.EventId, lastPartitionEnd, partition);
            allBoxEventSeries.ReadEvents(range, 1, x => x).Single().ShouldBeEquivalentTo(e12);
        }

        [Test]
        public void ReadEvents_ExclusiveStart_OnLowerPartitionBoundary_WithNoEventAtCursor()
        {
            var range = new AllBoxEventSeriesRange(secondPartitionStart, GuidHelpers.MinGuid, lastPartitionEnd, partition);
            allBoxEventSeries.ReadEvents(range, 1, x => x).Single().ShouldBeEquivalentTo(e21);
        }

        [Test]
        public void ReadEvents_ExclusiveStart_InsidePartition_WithEventAtCursor()
        {
            var range = new AllBoxEventSeriesRange(e14.EventTimestamp, e14.EventId, lastPartitionEnd, partition);
            allBoxEventSeries.ReadEvents(range, 1, x => x).Single().ShouldBeEquivalentTo(e15);
        }

        [Test]
        public void ReadEvents_ExclusiveStart_InsidePartition_WithNoEventAtCursor()
        {
            var range = new AllBoxEventSeriesRange(e14.EventTimestamp + second, GuidHelpers.MinGuid, lastPartitionEnd, partition);
            allBoxEventSeries.ReadEvents(range, 1, x => x).Single().ShouldBeEquivalentTo(e15);
        }

        [Test]
        public void ReadEvents_ExclusiveStart_OnUpperPartitionBoundary_WithEventAtCursor()
        {
            var range = new AllBoxEventSeriesRange(firstPartitionEnd, e18.EventId, lastPartitionEnd, partition);
            allBoxEventSeries.ReadEvents(range, 1, x => x).Single().ShouldBeEquivalentTo(e21);
        }

        [Test]
        public void ReadEvents_ExclusiveStart_OnUpperPartitionBoundary_WithNoEventAtCursor()
        {
            var range = new AllBoxEventSeriesRange(secondPartitionEnd, GuidHelpers.MinGuid, lastPartitionEnd, partition);
            allBoxEventSeries.ReadEvents(range, 1, x => x).Single().ShouldBeEquivalentTo(ef1);
        }

        [Test]
        public void ReadEvents_InclusiveEnd_OnLowerPartitionBoundary_WithEventAtCursor()
        {
            var range = new AllBoxEventSeriesRange(timeSeriesStart, GuidHelpers.MinGuid, lastPartitionStart, partition);
            allBoxEventSeries.ReadEvents(range, int.MaxValue, x => x).Last().ShouldBeEquivalentTo(ef1);
        }

        [Test]
        public void ReadEvents_InclusiveEnd_OnLowerPartitionBoundary_WithNoEventAtCursor()
        {
            var range = new AllBoxEventSeriesRange(timeSeriesStart, GuidHelpers.MinGuid, secondPartitionStart, partition);
            allBoxEventSeries.ReadEvents(range, int.MaxValue, x => x).Last().ShouldBeEquivalentTo(e18);
        }

        [Test]
        public void ReadEvents_InclusiveEnd_InsidePartition_WithEventAtCursor()
        {
            var range = new AllBoxEventSeriesRange(timeSeriesStart, GuidHelpers.MinGuid, e15.EventTimestamp, partition);
            allBoxEventSeries.ReadEvents(range, int.MaxValue, x => x).Last().ShouldBeEquivalentTo(e15);
        }

        [Test]
        public void ReadEvents_InclusiveEnd_InsidePartition_WithNoEventAtCursor()
        {
            var range = new AllBoxEventSeriesRange(timeSeriesStart, GuidHelpers.MinGuid, e15.EventTimestamp - tick, partition);
            allBoxEventSeries.ReadEvents(range, int.MaxValue, x => x).Last().ShouldBeEquivalentTo(e14);
        }

        [Test]
        public void ReadEvents_InclusiveEnd_OnUpperPartitionBoundary_WithEventAtCursor()
        {
            var range = new AllBoxEventSeriesRange(e16.EventTimestamp, e16.EventId, firstPartitionEnd, partition);
            allBoxEventSeries.ReadEvents(range, int.MaxValue, x => x).ShouldBeEquivalentWithOrderTo(e17, e18);
        }

        [Test]
        public void ReadEvents_InclusiveEnd_OnUpperPartitionBoundary_WithNoEventAtCursor()
        {
            var range = new AllBoxEventSeriesRange(timeSeriesStart, GuidHelpers.MinGuid, secondPartitionEnd, partition);
            allBoxEventSeries.ReadEvents(range, int.MaxValue, x => x).Last().ShouldBeEquivalentTo(e24);
        }

        [Test]
        public void ReadEvents_StopOnUncommittedEvent()
        {
            var range = new AllBoxEventSeriesRange(timeSeriesStart, GuidHelpers.MaxGuid, lastPartitionEnd, partition);
            WriteUncommitedEvent(BoxEvent(e15.EventTimestamp - tick));
            allBoxEventSeries.ReadEvents(range, int.MaxValue, x => x).ShouldBeEquivalentWithOrderTo(e11, e12, e13, e14);
        }

        private void WriteUncommitedEvent(BoxEvent e)
        {
            var eventsConnection = cassandraCluster.RetrieveColumnFamilyConnection(BoxEventSeriesCassandraSchemaConfigurator.BoxEventSeriesKeyspace, BoxEventSeriesCassandraSchemaConfigurator.AllBoxEventSeriesEventsColumnFamily);
            eventsConnection.AddColumn(AllBoxEventSeriesCassandraHelpers.FormatPartitionKey(e.EventTimestamp, partition), new Column
                {
                    Name = AllBoxEventSeriesCassandraHelpers.FormatColumnName(e.EventTimestamp, e.EventId),
                    Value = serializer.Serialize(new AllBoxEventSeriesColumnValue(e.Payload, eventIsCommitted: false)),
                    Timestamp = e.EventTimestamp.Ticks,
                    TTL = null,
                });
        }

        private TimeSpan partition;
        private Timestamp timeSeriesStart;
        private BoxEvent e11, e12, e13, e14, e15, e16, e17, e18, e21, e22, e23, e24, ef1, ef2, ef3, ef4;
        private Timestamp firstPartitionStart, firstPartitionEnd, secondPartitionStart, secondPartitionEnd, emptyPartitionStart, emptyPartitionEnd, lastPartitionStart, lastPartitionEnd;
    }
}