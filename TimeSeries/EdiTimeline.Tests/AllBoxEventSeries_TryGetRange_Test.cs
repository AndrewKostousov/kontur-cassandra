using System;
using Commons;
using FluentAssertions;
using NUnit.Framework;

namespace EdiTimeline.Tests
{
    public class AllBoxEventSeries_TryGetRange_Test : BoxEventSeriesTestBase
    {
        [Test]
        public void TryCreateRange_ByPointer()
        {
            var eventId = Guid.NewGuid();
            var eventTimestamp = Timestamp.Now;
            var lastGoodEventTimestamp = eventTimestamp.AddTicks(1);
            allBoxEventSeriesTicksHolder.SetLastGoodEventTicks(lastGoodEventTimestamp.Ticks);
            allBoxEventSeries.TryCreateRange(new AllBoxEventSeriesPointer(eventTimestamp, eventId), inclusiveEndTimestamp: null)
                             .ShouldBeEquivalentTo(new AllBoxEventSeriesRange(eventTimestamp, eventId, lastGoodEventTimestamp, allBoxEventSeries.PartitionDuration));
        }

        [Test]
        public void TryCreateRange_ByPointer_NotNullEndTimestamp()
        {
            var eventId = Guid.NewGuid();
            var eventTimestamp = Timestamp.Now;
            var endTimestamp = eventTimestamp.AddTicks(1);
            allBoxEventSeries.TryCreateRange(new AllBoxEventSeriesPointer(eventTimestamp, eventId), endTimestamp)
                             .ShouldBeEquivalentTo(new AllBoxEventSeriesRange(eventTimestamp, eventId, endTimestamp, allBoxEventSeries.PartitionDuration));
        }

        [Test]
        public void TryCreateRange_ByPointer_StartPointerIsGreaterThanLastGoodEvent()
        {
            var eventTimestamp = Timestamp.Now;
            var lastGoodEventTimestamp = eventTimestamp.AddTicks(-1);
            allBoxEventSeriesTicksHolder.SetLastGoodEventTicks(lastGoodEventTimestamp.Ticks);
            allBoxEventSeries.TryCreateRange(new AllBoxEventSeriesPointer(eventTimestamp, Guid.NewGuid()), inclusiveEndTimestamp: null).Should().BeNull();
        }

        [Test]
        public void TryCreateRange_ByPointer_LastGoodEventNotSet()
        {
            allBoxEventSeries.TryCreateRange(new AllBoxEventSeriesPointer(Timestamp.Now, Guid.NewGuid()), inclusiveEndTimestamp: null).Should().BeNull();
        }

        [Test]
        public void TryCreateRange_ByPointer_NullPointer()
        {
            var eventSeriesStartTimestamp = Timestamp.Now;
            var lastGoodEventTimestamp = eventSeriesStartTimestamp.AddTicks(1);
            allBoxEventSeriesTicksHolder.SetEventSeriesExclusiveStartTicks(eventSeriesStartTimestamp.Ticks);
            allBoxEventSeriesTicksHolder.SetLastGoodEventTicks(lastGoodEventTimestamp.Ticks);
            allBoxEventSeries.TryCreateRange(exclusiveStartEventPointer: null, inclusiveEndTimestamp: null)
                             .ShouldBeEquivalentTo(new AllBoxEventSeriesRange(eventSeriesStartTimestamp, GuidHelpers.MaxGuid, lastGoodEventTimestamp, allBoxEventSeries.PartitionDuration));
        }

        [Test]
        public void TryCreateRange_ByPointer_NullPointer_EventSeriesStartNotSet()
        {
            allBoxEventSeriesTicksHolder.SetLastGoodEventTicks(Timestamp.Now.Ticks);
            allBoxEventSeries.TryCreateRange(exclusiveStartEventPointer: null, inclusiveEndTimestamp: null).Should().BeNull();
        }

        [Test]
        public void TryCreateRange_ByPointer_NullPointer_LastGoodEventNotSet()
        {
            allBoxEventSeriesTicksHolder.SetEventSeriesExclusiveStartTicks(Timestamp.Now.Ticks);
            allBoxEventSeries.TryCreateRange(exclusiveStartEventPointer: null, inclusiveEndTimestamp: null).Should().BeNull();
        }

        [Test]
        public void TryCreateRange_ByTimestamp_EventSeriesStartNotSet()
        {
            allBoxEventSeries.TryCreateRange(exclusiveStartTimestamp: null, inclusiveEndTimestamp: Timestamp.Now).Should().BeNull();
        }

        [Test]
        public void TryCreateRange_ByTimestamp_NullStartTimestamp()
        {
            var eventSeriesStartTimestamp = Timestamp.Now;
            var endTimestamp = eventSeriesStartTimestamp.AddTicks(1);
            allBoxEventSeriesTicksHolder.SetEventSeriesExclusiveStartTicks(eventSeriesStartTimestamp.Ticks);
            allBoxEventSeries.TryCreateRange(exclusiveStartTimestamp: null, inclusiveEndTimestamp: endTimestamp)
                             .ShouldBeEquivalentTo(new AllBoxEventSeriesRange(eventSeriesStartTimestamp, GuidHelpers.MaxGuid, endTimestamp, allBoxEventSeries.PartitionDuration));
        }

        [Test]
        public void TryCreateRange_ByTimestamp_StartTimestampIsLessThanEventSeriesStart()
        {
            var eventSeriesStartTimestamp = Timestamp.Now;
            var startTimestamp = eventSeriesStartTimestamp.AddTicks(-1);
            var endTimestamp = eventSeriesStartTimestamp.AddTicks(1);
            allBoxEventSeriesTicksHolder.SetEventSeriesExclusiveStartTicks(eventSeriesStartTimestamp.Ticks);
            allBoxEventSeries.TryCreateRange(startTimestamp, endTimestamp)
                             .ShouldBeEquivalentTo(new AllBoxEventSeriesRange(eventSeriesStartTimestamp, GuidHelpers.MaxGuid, endTimestamp, allBoxEventSeries.PartitionDuration));
        }

        [Test]
        public void TryCreateRange_ByTimestamp_StartTimestampIsGreaterThanEventSeriesStart()
        {
            var eventSeriesStartTimestamp = Timestamp.Now;
            var startTimestamp = eventSeriesStartTimestamp.AddTicks(1);
            var endTimestamp = eventSeriesStartTimestamp.AddTicks(2);
            allBoxEventSeriesTicksHolder.SetEventSeriesExclusiveStartTicks(eventSeriesStartTimestamp.Ticks);
            allBoxEventSeries.TryCreateRange(startTimestamp, endTimestamp)
                             .ShouldBeEquivalentTo(new AllBoxEventSeriesRange(startTimestamp, GuidHelpers.MaxGuid, endTimestamp, allBoxEventSeries.PartitionDuration));
        }

        [Test]
        public void TryCreateRange_ByTimestamp_NullEndTimestamp_LastGooEventNotSet()
        {
            var eventSeriesStartTimestamp = Timestamp.Now;
            var startTimestamp = eventSeriesStartTimestamp.AddTicks(1);
            allBoxEventSeriesTicksHolder.SetEventSeriesExclusiveStartTicks(eventSeriesStartTimestamp.Ticks);
            allBoxEventSeries.TryCreateRange(startTimestamp, inclusiveEndTimestamp: null).Should().BeNull();
        }

        [Test]
        public void TryCreateRange_ByTimestamp_NullEndTimestamp()
        {
            var eventSeriesStartTimestamp = Timestamp.Now;
            var startTimestamp = eventSeriesStartTimestamp;
            var lastGoodEventTimestamp = eventSeriesStartTimestamp.AddTicks(1);
            allBoxEventSeriesTicksHolder.SetEventSeriesExclusiveStartTicks(eventSeriesStartTimestamp.Ticks);
            allBoxEventSeriesTicksHolder.SetLastGoodEventTicks(lastGoodEventTimestamp.Ticks);
            allBoxEventSeries.TryCreateRange(startTimestamp, inclusiveEndTimestamp: null)
                             .ShouldBeEquivalentTo(new AllBoxEventSeriesRange(startTimestamp, GuidHelpers.MaxGuid, lastGoodEventTimestamp, allBoxEventSeries.PartitionDuration));
        }

        [Test]
        public void TryCreateRange_ByTimestamp_StartTimestampIsGreaterThanEndTimestamp()
        {
            var startTimestamp = Timestamp.Now;
            var endTimestamp = startTimestamp.AddTicks(-1);
            allBoxEventSeriesTicksHolder.SetEventSeriesExclusiveStartTicks(startTimestamp.Ticks);
            allBoxEventSeries.TryCreateRange(startTimestamp, endTimestamp).Should().BeNull();
        }
    }
}