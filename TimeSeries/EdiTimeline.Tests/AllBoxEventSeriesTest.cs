using System;
using System.Collections.Generic;
using System.Linq;
using Commons;
using FluentAssertions;
using JetBrains.Annotations;
using NUnit.Framework;

namespace EdiTimeline.Tests
{
    public class AllBoxEventSeriesTest : BoxEventSeriesTestBase
    {
        [Test]
        public void ReadEvent()
        {
            var expectedEvent = BoxEvent();
            allBoxEventSeries.TryReadEvent(expectedEvent.EventId).Should().BeNull();
            WriteWithNoSync(expectedEvent);
            allBoxEventSeries.TryReadEvent(expectedEvent.EventId).ShouldBeEquivalentTo(expectedEvent);
        }

        [Test]
        public void ReadEventsToEnd_StartFromEvent()
        {
            var firstEvent = BoxEvent(0xff);
            WriteWithNoSync(firstEvent);
            var expectedEvents = GenerateEvents(10);
            var actualEvents = ReadEventsToEnd(firstEvent);
            actualEvents.ShouldBeEquivalentWithOrderTo(expectedEvents);
            for (var i = 0; i < expectedEvents.Count; i++)
            {
                actualEvents = ReadEventsToEnd(expectedEvents[i]);
                actualEvents.ShouldBeEquivalentWithOrderTo(expectedEvents.Skip(i + 1).ToList());
            }
        }

        [Test]
        public void ReadEventsToEnd_StartFromEventId()
        {
            var boxEvents = GenerateEvents(10);
            var exlusiveStartEventId = boxEvents[2].EventId;
            var expectedEvents = boxEvents.Skip(3).ToList();
            ReadEventsToEnd(exlusiveStartEventId).ShouldBeEquivalentWithOrderTo(expectedEvents);
        }

        [Test]
        public void ReadEventsToEnd_StartFromTimestamp()
        {
            var expectedEvents = GenerateEvents(10).Skip(3).ToList();
            var exlusiveStartTimestamp = expectedEvents.First().EventTimestamp.Subtract(TimeSpan.FromTicks(1));
            ReadEventsToEnd(exlusiveStartTimestamp).ShouldBeEquivalentWithOrderTo(expectedEvents);
        }

        private List<BoxEvent> GenerateEvents(byte count)
        {
            var expectedEvents = Enumerable.Range(0, count).Select(x => BoxEvent((byte) x)).ToList();
            WriteWithNoSync(expectedEvents.ToArray());
            return expectedEvents;
        }

        private void WriteWithNoSync([NotNull] params BoxEvent[] boxEvents)
        {
            allBoxEventSeries.WriteEventsWithNoSynchronization(boxEvents);
            allBoxEventSeriesTicksHolder.SetLastGoodEventTicks(boxEvents.Last().EventTimestamp.Ticks);
        }

        [NotNull]
        private List<BoxEvent> ReadEventsToEnd([NotNull] BoxEvent exclusiveStartEvent)
        {
            var range = allBoxEventSeries.TryCreateRange(new AllBoxEventSeriesPointer(exclusiveStartEvent.EventTimestamp, exclusiveStartEvent.EventId), inclusiveEndTimestamp: null);
            range.Should().NotBeNull();
            return allBoxEventSeries.ReadEvents(range, int.MaxValue, x => x);
        }

        [NotNull]
        private List<BoxEvent> ReadEventsToEnd(Guid exclusiveStartEventId)
        {
            bool exclusiveStartEventNotFound;
            var range = allBoxEventSeries.TryCreateRange(exclusiveStartEventId, null, out exclusiveStartEventNotFound);
            range.Should().NotBeNull();
            exclusiveStartEventNotFound.Should().BeFalse();
            return allBoxEventSeries.ReadEvents(range, int.MaxValue, x => x);
        }

        [NotNull]
        private List<BoxEvent> ReadEventsToEnd([NotNull] Timestamp exclusiveStartTimestamp)
        {
            var range = allBoxEventSeries.TryCreateRange(exclusiveStartTimestamp, inclusiveEndTimestamp: null);
            range.Should().NotBeNull();
            return allBoxEventSeries.ReadEvents(range, int.MaxValue, x => x);
        }
    }
}