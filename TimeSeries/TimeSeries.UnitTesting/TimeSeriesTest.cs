using Cassandra;
using Cassandra.Data.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CassandraTimeSeries.DatabaseTest
{
    public class TimeSeriesTestBase
    {
        #region *** SetUp ***
        static Cluster cluster;
        static ISession session;
        static SimpleSeriesDatabase database;

        public static TimeSeries Series { get; private set; }

        [OneTimeSetUp]
        public void StartSession()
        {
            cluster = Cluster
                .Builder()
                .AddContactPoint("localhost")
                .Build();

            session = cluster.Connect();
            database = new SimpleSeriesDatabase(session, "test");
            Series = new TimeSeries(database.Table);
        }

        [OneTimeTearDown]
        public void DisposeSession()
        {
            session.Dispose();
            cluster.Dispose();
        }

        [SetUp]
        public void TruncateTable()
        {
            Series.Table.Truncate();
        }
        #endregion

        #region *** Test Templates ***
        protected void RunTest(
            List<Event> eventsToWrite,
            Func<DateTimeOffset, DateTimeOffset, List<Event>> read)
        {
            RunTest(
                eventsToWrite,
                read,
                actual => Assert.AreEqual(eventsToWrite, actual)
            );
        }

        protected void RunTest(
            List<Event> eventsToWrite,
            Func<DateTimeOffset, DateTimeOffset, List<Event>> read,
            Action<List<Event>> assert)
        {
            var start = DateTimeOffset.UtcNow - TimeSpan.FromMinutes(1);

            eventsToWrite.ForEach(e => Series.Write(e));

            var end = DateTimeOffset.UtcNow + TimeSpan.FromMinutes(1);

            var retrievedEvents = read(start, end);
            assert(retrievedEvents);
        }
        #endregion
        
        protected List<Event> CreateEvents(int count)
        {
            return Enumerable.Range(0, count)
                .Select(_ => new Event(DateTimeOffset.UtcNow))
                .ToList();
        }
    }

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
        public void Series_ShouldNotReadWrongEvents_IfBeforeRange()
        {
            var count = 10;

            var expected = CreateEvents(count);
            var wrongEvent = new Event(DateTime.Now - TimeSpan.FromHours(1));

            RunTest(
                expected.Union(new Event[] { wrongEvent }).ToList(),
                (start, end) => Series.ReadRange(start, end, count),
                actual => Assert.AreEqual(expected, actual)
            );
        }

        [Test]
        public void Series_ShouldNotReadWrongEvents_IfAfterRange()
        {
            var count = 10;

            var expected = CreateEvents(count);
            var wrongEvent = new Event(DateTime.Now + TimeSpan.FromHours(1));

            RunTest(
                expected.Union(new Event[] { wrongEvent }).ToList(),
                (start, end) => Series.ReadRange(start, end, count),
                actual => Assert.AreEqual(expected, actual)
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
                actual => Assert.AreEqual(expected.Where(x => x.Timestamp != end), actual)
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
                actual => Assert.AreEqual(expected.Where(x => x.Id != end), actual)
            );
        }

        [Test]
        public void Series_DateTimeOffset_ShouldReadExactlyOne_IfStartAndEndAreEqual()
        {
            var count = 10;

            var expected = CreateEvents(count);
            var end = expected.First().Timestamp;
            var start = end;

            RunTest(
                expected,
                (s, e) => Series.ReadRange(start, end, count)
            );
        }

        [Test]
        public void Series_TimeUuid_ShouldReadExactlyOne_IfStartAndEndAreEqual()
        {
            var count = 10;

            var expected = CreateEvents(count);
            var end = expected.First().Id;
            var start = end;

            RunTest(
                expected,
                (s, e) => Series.ReadRange(start, end, count)
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
                actual => Assert.IsEmpty(actual)
            );
        }
    }
}
