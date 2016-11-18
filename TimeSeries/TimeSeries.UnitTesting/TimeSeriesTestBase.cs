using Cassandra;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CassandraTimeSeries.UnitTesting
{
    public class TimeSeriesTestBase
    {
        #region *** SetUp ***

        private static Cluster cluster;
        private static ISession session;
        private static SimpleSeriesDatabase database;

        protected static TimeSeries Series { get; private set; }

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
            var startTime = DateTimeOffset.UtcNow;

            return Enumerable.Range(0, count)
                .Select(i => new Event(startTime + TimeSpan.FromMilliseconds(i)))
                .ToList();
        }
    }
}
