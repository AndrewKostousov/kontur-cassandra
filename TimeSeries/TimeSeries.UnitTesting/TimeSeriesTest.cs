using Cassandra.Data.Linq;
using NUnit.Framework;
using System;
using System.Linq;
using FluentAssertions;

namespace CassandraTimeSeries.UnitTesting
{
    [TestFixture]
    public class TimeSeriesTest : TimeSeriesTestBase
    {
        [Test]
        public void Series_DateTimeOffset_CanWriteAndReadSingleEvent()
        {
            RunTest(
                CreateEvents(1),
                (start, end) => Series.ReadRange(start, end, 1)
            );
        }

        [Test]
        public void Series_TimeUuid_CanWriteAndReadSingleEvent()
        {
            RunTest(
                CreateEvents(1),
                (start, end) => Series.ReadRange(start.MinTimeUuid(), end.MaxTimeUuid(), 1)
            );
        }

        [Test]
        public void Series_DateTimeOffset_CanWriteAndReadSeriesOfEvents()
        {
            var count = 10;

            RunTest(
                CreateEvents(count),
                (start, end) => Series.ReadRange(start, end, count)
            );
        }

        [Test]
        public void Series_TimeUuid_CanWriteAndReadSeriesOfEvents()
        {
            var count = 10;

            RunTest(
                CreateEvents(count),
                (start, end) => Series.ReadRange(start.MinTimeUuid(), end.MaxTimeUuid(), count)
            );
        }

        [Test]
        public void Series_DateTimeOffset_ReadShouldBeOrderedByTimeAscending()
        {
            var count = 10;

            var eventsToWrite = CreateEvents(count);

            RunTest(
                eventsToWrite,
                (start, end) => Series.ReadRange(start, end, count),
                actual => actual.ShouldBeEquivalentTo(eventsToWrite)
            );
        }

        [Test]
        public void Series_TimeUuid_ReadShouldBeOrderedByTimeAscending()
        {
            var count = 10;

            var eventsToWrite = CreateEvents(count);

            RunTest(
                eventsToWrite,
                (start, end) => Series.ReadRange(start.MinTimeUuid(), end.MaxTimeUuid(), count),
                actual => actual.ShouldBeEquivalentTo(eventsToWrite)
            );
        }

        [Test]
        public void Series_DateTimeOffset_ShouldNotReadFromPast()
        {
            var count = 10;

            var expected = CreateEvents(count);
            var wrongEvent = new Event(DateTime.Now - TimeSpan.FromHours(1));

            RunTest(
                expected.Union(new[] { wrongEvent }).ToList(),
                (start, end) => Series.ReadRange(start, end, count),
                actual => actual.ShouldBeEquivalentTo(expected)
            );
        }

        [Test]
        public void Series_DateTimeOffset_ShouldNotReadFromFuture()
        {
            var count = 10;

            var expected = CreateEvents(count);
            var wrongEvent = new Event(DateTime.Now + TimeSpan.FromHours(1));

            RunTest(
                expected.Union(new[] { wrongEvent }).ToList(),
                (start, end) => Series.ReadRange(start, end, count),
                actual => actual.ShouldBeEquivalentTo(expected)
            );
        }

        [Test]
        public void Series_TimeUuid_ShouldNotReadFromPast()
        {
            var count = 10;

            var expected = CreateEvents(count);
            var wrongEvent = new Event(DateTime.Now - TimeSpan.FromHours(1));

            RunTest(
                expected.Union(new[] { wrongEvent }).ToList(),
                (start, end) => Series.ReadRange(start.MinTimeUuid(), end.MaxTimeUuid(), count),
                actual => actual.ShouldBeEquivalentTo(expected)
            );
        }

        [Test]
        public void Series_TimeUuid_ShouldNotReadFromFuture()
        {
            var count = 10;

            var expected = CreateEvents(count);
            var wrongEvent = new Event(DateTime.Now + TimeSpan.FromHours(1));

            RunTest(
                expected.Union(new[] { wrongEvent }).ToList(),
                (start, end) => Series.ReadRange(start.MinTimeUuid(), end.MaxTimeUuid(), count),
                actual => actual.ShouldBeEquivalentTo(expected)
            );
        }

        [Test]
        public void Series_DateTimeOffset_CheckInclusiveStart()
        {
            var count = 10;

            var expected = CreateEvents(count);
            var start = expected.Min(e => e.Timestamp);

            RunTest(
                expected,
                (_, end) => Series.ReadRange(start, end, count)
            );
        }

        [Test]
        public void Series_TimeUuid_CheckInclusiveStart()
        {
            var count = 10;

            var expected = CreateEvents(count);
            var start = expected.Min(e => e.Id);
            var end = expected.Max(e => e.Timestamp).MaxTimeUuid();

            RunTest(
                expected,
                (s,e) => Series.ReadRange(start, end, count)
            );
        }

        [Test]
        public void Series_DateTimeOffset_CheckExclusiveEnd()
        {
            var count = 10;

            var expected = CreateEvents(count);
            var end = expected.Max(e => e.Timestamp);

            RunTest(
                expected,
                (start, _) => Series.ReadRange(start, end, count),
                actual => actual.ShouldBeEquivalentTo(expected.Where(x => x.Timestamp != end))
            );
        }

        [Test]
        public void Series_TimeUuid_CheckExclusiveEnd()
        {
            var count = 10;

            var expected = CreateEvents(count);
            var end = expected.Max(e => e.Id);
            var start = expected.Min(e => e.Timestamp).MinTimeUuid();

            RunTest(
                expected,
                (s, e) => Series.ReadRange(start, end, count),
                actual => actual.ShouldBeEquivalentTo(expected.Where(x => x.Id != end))
            );
        }

        [Test]
        public void Series_DateTimeOffset_ShouldReadExactlyOne_IfStartAndEndAreEqual()
        {
            var count = 10;

            var eventsToWrite = CreateEvents(count);
            var end = eventsToWrite.First().Timestamp;
            var expected = eventsToWrite.Single(x => x.Timestamp == end);
            var start = end;

            RunTest(
                eventsToWrite,
                (s, e) => Series.ReadRange(start, end, count),
                actual => actual.Single().ShouldBeEquivalentTo(expected)
            );
        }

        [Test]
        public void Series_TimeUuid_ShouldReadExactlyOne_IfStartAndEndAreEqual()
        {
            var count = 10;

            var eventsToWrite = CreateEvents(count);
            var end = eventsToWrite.First().Id;
            var expected = eventsToWrite.Single(x => x.Id == end);
            var start = end;

            RunTest(
                eventsToWrite,
                (s, e) => Series.ReadRange(start, end, count),
                actual => actual.Single().ShouldBeEquivalentTo(expected)
            );
        }

        [Test]
        public void Series_DateTimeOffset_ShouldReadZero_IfStartGreaterThanEnd()
        {
            var count = 10;

            var expected = CreateEvents(count);
            var end = expected.First().Timestamp;
            var start = end + TimeSpan.FromMinutes(1);

            RunTest(
                expected,
                (s, e) => Series.ReadRange(start, end, count),
                Assert.IsEmpty
            );
        }

        [Test]
        public void Series_DateTimeOffset_ShouldReadCount()
        {
            var countToWrite = 10;
            var countToRead = 5;

            var expected = CreateEvents(countToWrite);

            RunTest(
                expected,
                (start, end) => Series.ReadRange(start, end, countToRead),
                actual => actual.ShouldAllBeEquivalentTo(expected.Take(countToRead))
            );
        }

        [Test]
        public void Series_TimeUuid_ShouldReadCount()
        {
            var countToWrite = 10;
            var countToRead = 5;

            var expected = CreateEvents(countToWrite);
            var start = expected.Min(x => x.Id);
            var end = expected.Max(x => x.Id);

            RunTest(
                expected,
                (s, e) => Series.ReadRange(start, end, countToRead),
                actual => actual.ShouldAllBeEquivalentTo(expected.Take(countToRead))
            );
        }
    }
}
