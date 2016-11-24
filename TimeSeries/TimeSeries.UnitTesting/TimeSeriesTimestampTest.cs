using System;
using System.Linq;
using Commons;
using Commons.TimeBasedUuid;
using FluentAssertions;
using NUnit.Framework;

namespace CassandraTimeSeries.UnitTesting
{
    [TestFixture]
    public class TimeSeriesTimestampTest : TimeSeriesTestBase
    {
        [Test]
        public void Series_Timestamp_CanWriteAndReadSeriesOfEvents()
        {
            var count = 10;

            RunTest(
                CreateEvents(count),
                (start, end) => Series.ReadRange(start, end, count)
            );
        }

        [Test]
        public void Series_Timestamp_CanWriteAndReadSingleEvent()
        {
            RunTest(
                CreateEvents(1),
                (start, end) => Series.ReadRange(start, end, 1)
            );
        }

        [Test]
        public void Series_Timestamp_CheckExclusiveEnd()
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
        public void Series_Timestamp_CheckInclusiveStart()
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
        public void Series_Timestamp_ReadShouldBeOrderedByTimeAscending()
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
        public void Series_Timestamp_ShouldExcludeMax_IfExplicitlyPassed()
        {
            var maxTimestamp = new Event(TimeGuid.MaxValue.GetTimestamp());
            var maxTimeGuid = new Event(TimeGuid.MaxValue);

            RunTest(
                new[] {maxTimestamp, maxTimeGuid},
                (s, e) =>
                    Series.ReadRange(TimeGuid.MaxValue.GetTimestamp() - TimeSpan.FromSeconds(10),
                        TimeGuid.MaxValue.GetTimestamp(), 2),
                actual => actual.Should().BeEmpty()
            );
        }

        [Test]
        public void Series_Timestamp_ShouldIncludeMax_IfPassedNull()
        {
            var maxTimestamp = new Event(TimeGuid.MaxValue.GetTimestamp());
            var maxTimeGuid = new Event(TimeGuid.MaxValue);

            RunTest(
                new[] {maxTimestamp, maxTimeGuid},
                (s, e) => Series.ReadRange(TimeGuid.MaxValue.GetTimestamp() - TimeSpan.FromSeconds(10), null, 2)
            );
        }

        [Test]
        public void Series_Timestamp_ShouldNotReadFromFuture()
        {
            var count = 10;

            var expected = CreateEvents(count);
            var wrongEvent = new Event(Timestamp.Now + TimeSpan.FromHours(1));

            RunTest(
                expected.Union(new[] {wrongEvent}).ToList(),
                (start, end) => Series.ReadRange(start, end, count),
                actual => actual.ShouldBeEquivalentTo(expected)
            );
        }

        [Test]
        public void Series_Timestamp_ShouldNotReadFromPast()
        {
            var count = 10;

            var expected = CreateEvents(count);
            var wrongEvent = new Event(Timestamp.Now - TimeSpan.FromHours(1));

            RunTest(
                expected.Union(new[] {wrongEvent}).ToList(),
                (start, end) => Series.ReadRange(start, end, count),
                actual => actual.ShouldBeEquivalentTo(expected)
            );
        }

        [Test]
        public void Series_Timestamp_ShouldReadCount()
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
        public void Series_Timestamp_ShouldReadExactlyOne_IfStartAndEndAreEqual()
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
        public void Series_Timestamp_ShouldReadFromBeginning_IfStartIsNull()
        {
            var minTimestamp = new Event(TimeGuid.MinValue.GetTimestamp());
            var maxTimestamp = new Event(TimeGuid.MaxValue.GetTimestamp());
            var minTimeGuid = new Event(TimeGuid.MinValue);
            var maxTimeGuid = new Event(TimeGuid.MaxValue);

            RunTest(
                new[] {minTimestamp, maxTimestamp, minTimeGuid, maxTimeGuid},
                (s, e) => Series.ReadRange((Timestamp) null, null, 2),
                actual => actual.ShouldBeEquivalentTo(new[] {minTimeGuid, minTimestamp})
            );
        }

        [Test]
        public void Series_Timestamp_ShouldReadZero_IfStartGreaterThanEnd()
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
    }
}